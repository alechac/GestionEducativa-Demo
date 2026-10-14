using GestionEducativa.Models;
using GestionEducativa.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Remoting.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using static Org.BouncyCastle.Math.EC.ECCurve;

namespace GestionEducativa.Services
{
        public class ArcaService
    {
        private readonly ArcaConfig _config;

        // Cache static — persiste entre instancias para no pedir ticket nuevo cada vez
        // ARCA rechaza con "El CEE ya posee un TA válido" si se pide uno mientras el anterior sigue activo
        private static string _tokenCache;
        private static string _signCache;
        private static DateTime _ticketExpira = DateTime.MinValue;
        private static string _cuitCache;

        public ArcaService(ArcaConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        // ══════════════════════════════════════════════════════════════════
        //  PASO 1 — WSAA: obtener Token + Sign
        // ══════════════════════════════════════════════════════════════════

        public (string token, string sign) ObtenerTicketAcceso()
        {
            // Reutilizar caché si todavía es válido (con 5 min de margen) y el CUIT no cambió
            bool cacheValido = !string.IsNullOrEmpty(_tokenCache)
                            && DateTime.UtcNow < _ticketExpira.AddMinutes(-5)
                            && _cuitCache == _config.CUIT;

            if (cacheValido)
                return (_tokenCache, _signCache);

            // 1. Generar el CMS firmado (LoginTicketRequest firmado en Base64)
            string cmsFirmado = CertificadoHelper.GenerarLoginTicketRequest(
                _config.RutaPfx, _config.PasswordPfx, servicio: "wsfe");

            // 2. Llamar al WSAA
            string respuestaXml = LlamarWsaa(cmsFirmado);

            // 3. Parsear la respuesta — el WSAA devuelve el loginTicketResponse
            //    como XML escapado dentro de <loginCmsReturn>, no como XML directo
            var (token, sign, expStr) = ParsearRespuestaWsaa(respuestaXml);

            _ticketExpira = !string.IsNullOrEmpty(expStr)
                ? DateTime.Parse(expStr).ToUniversalTime()
                : DateTime.UtcNow.AddHours(10);

            _tokenCache = token;
            _signCache = sign;
            _cuitCache = _config.CUIT;

            return (token, sign);
        }

        private (string token, string sign, string expiracion) ParsearRespuestaWsaa(string respuestaXml)
        {
            var doc = new XmlDocument();
            doc.LoadXml(respuestaXml);

            // Usar GetElementsByTagName con wildcard namespace — más robusto que XPath
            // cuando hay namespaces por defecto anidados
            XmlNodeList nodos = doc.GetElementsByTagName("loginCmsReturn");
            if (nodos.Count == 0)
                nodos = doc.GetElementsByTagName("loginCmsReturn",
                    "http://wsaa.view.sua.dvadac.desein.afip.gov/");

            if (nodos.Count == 0)
                throw new Exception(
                    "WSAA: no se encontró loginCmsReturn en la respuesta.\n" + respuestaXml);

            // InnerText decodifica automáticamente &lt; → <  &gt; → >  &quot; → "
            string xmlInterno = nodos[0].InnerText;

            if (string.IsNullOrWhiteSpace(xmlInterno))
                throw new Exception("WSAA: loginCmsReturn está vacío.");

            // Parsear el XML interno (el loginTicketResponse real)
            var docInterno = new XmlDocument();
            docInterno.LoadXml(xmlInterno.Trim());

            string token = docInterno.GetElementsByTagName("token")[0]?.InnerText;
            string sign = docInterno.GetElementsByTagName("sign")[0]?.InnerText;
            string exp = docInterno.GetElementsByTagName("expirationTime")[0]?.InnerText;

            if (string.IsNullOrEmpty(token))
                throw new Exception(
                    "WSAA: loginTicketResponse no contiene token.\n" + xmlInterno);
            if (string.IsNullOrEmpty(sign))
                throw new Exception(
                    "WSAA: loginTicketResponse no contiene sign.\n" + xmlInterno);

            return (token, sign, exp ?? "");
        }

        private string LlamarWsaa(string cmsFirmadoBase64)
        {
            // El WSAA de ARCA espera exactamente este envelope SOAP.
            // IMPORTANTE:
            //   - El namespace del servicio es "http://wsaa.view.sua.dvadac.desein.afip.gov/"
            //   - El argumento se llama "in0" (no "cms" ni "CMS")
            //   - SOAPAction debe ser cadena vacía ""
            string soapEnvelope =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<soapenv:Envelope " +
                "  xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
                "  xmlns:wsaa=\"http://wsaa.view.sua.dvadac.desein.afip.gov/\">" +
                "  <soapenv:Header/>" +
                "  <soapenv:Body>" +
                "    <wsaa:loginCms>" +
                "      <wsaa:in0>" + cmsFirmadoBase64 + "</wsaa:in0>" +
                "    </wsaa:loginCms>" +
                "  </soapenv:Body>" +
                "</soapenv:Envelope>";

            // SOAPAction vacío para el WSAA (así lo indica la especificación AFIP)
            return EnviarSoap(_config.UrlWsaa, soapEnvelope, soapAction: "");
        }

        // ══════════════════════════════════════════════════════════════════
        //  FEDummy — verificar disponibilidad de servidores
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Llama a FEDummy (no requiere autenticación) para verificar
        /// que AppServer, AuthServer y DbServer de ARCA estén en línea.
        /// Es el método más simple para testear la conectividad.
        /// </summary>
        public bool VerificarDisponibilidad()
        {
            try
            {
                // FEDummy no requiere Token/Sign — sirve para verificar
                // conectividad sin necesidad de certificado
                string soapEnvelope =
                    "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                    "<soapenv:Envelope " +
                    "  xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
                    "  xmlns:ar=\"http://ar.gov.afip.dif.FEV1/\">" +
                    "  <soapenv:Header/>" +
                    "  <soapenv:Body>" +
                    "    <ar:FEDummy/>" +
                    "  </soapenv:Body>" +
                    "</soapenv:Envelope>";

                string resp = EnviarSoap(
                    _config.UrlWsfe,
                    soapEnvelope,
                    soapAction: "http://ar.gov.afip.dif.FEV1/FEDummy");

                var doc = new XmlDocument();
                doc.LoadXml(resp);

                // El response tiene xmlns="http://ar.gov.afip.dif.FEV1/" en FEDummyResponse,
                // por eso //AppServer sin namespace no encuentra nada.
                // Usamos local-name() para ignorar el namespace.
                string appServer = doc.SelectSingleNode("//*[local-name()='AppServer']")?.InnerText ?? "ERROR";
                string authServer = doc.SelectSingleNode("//*[local-name()='AuthServer']")?.InnerText ?? "ERROR";
                string dbServer = doc.SelectSingleNode("//*[local-name()='DbServer']")?.InnerText ?? "ERROR";

                // Guardar los valores para que quien llame pueda mostrarlos
                UltimoEstadoAppServer = appServer;
                UltimoEstadoAuthServer = authServer;
                UltimoEstadoDbServer = dbServer;

                return appServer == "OK" && authServer == "OK" && dbServer == "OK";
            }
            catch (WebException webEx)
            {
                // Capturar el body del error HTTP para dar un mensaje útil
                string detalle = LeerCuerpoError(webEx);
                throw new Exception("Error de red al contactar ARCA:\n" + detalle, webEx);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al verificar disponibilidad ARCA:\n" + ex.Message, ex);
            }
        }

        // Propiedades para que la UI pueda mostrar el estado de cada servidor
        public string UltimoEstadoAppServer { get; private set; } = "";
        public string UltimoEstadoAuthServer { get; private set; } = "";
        public string UltimoEstadoDbServer { get; private set; } = "";

        // ══════════════════════════════════════════════════════════════════
        //  PASO 2 — WSFEv1: solicitar CAE
        // ══════════════════════════════════════════════════════════════════

        /// <summary>
        /// Consulta un comprobante ya emitido por su número en ARCA (FECompConsultar).
        /// Sirve para verificar que el comprobante quedó registrado correctamente.
        /// </summary>
        public string ConsultarComprobante(long nroComprobante, int tipoCbte,
            (string token, string sign) ticket)
        {
            string soap =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<soapenv:Envelope " +
                "  xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
                "  xmlns:ar=\"http://ar.gov.afip.dif.FEV1/\">" +
                "  <soapenv:Header/>" +
                "  <soapenv:Body>" +
                "    <ar:FECompConsultar>" +
                "      <ar:Auth>" +
                "        <ar:Token>" + ticket.token + "</ar:Token>" +
                "        <ar:Sign>" + ticket.sign + "</ar:Sign>" +
                "        <ar:Cuit>" + _config.CUIT + "</ar:Cuit>" +
                "      </ar:Auth>" +
                "      <ar:FeCompConsReq>" +
                "        <ar:CbteTipo>" + tipoCbte + "</ar:CbteTipo>" +
                "        <ar:CbteNro>" + nroComprobante + "</ar:CbteNro>" +
                "        <ar:PtoVta>" + _config.PuntoVenta + "</ar:PtoVta>" +
                "      </ar:FeCompConsReq>" +
                "    </ar:FECompConsultar>" +
                "  </soapenv:Body>" +
                "</soapenv:Envelope>";

            // Devuelve el XML crudo para que puedas inspeccionarlo
            return EnviarSoap(_config.UrlWsfe, soap,
                "http://ar.gov.afip.dif.FEV1/FECompConsultar");
        }

        /// <summary>
        /// Diagnóstico del certificado PFX:
        /// — Verifica que se pueda leer con la contraseña dada
        /// — Muestra Subject, emisor, vigencia y si tiene clave privada
        /// — Verifica que el CN del certificado contenga el CUIT configurado
        /// </summary>
        public string DiagnosticarPfx()
        {
            var sb = new System.Text.StringBuilder();
            try
            {
                if (!System.IO.File.Exists(_config.RutaPfx))
                {
                    return $"❌ Archivo PFX no encontrado: {_config.RutaPfx}";
                }

                var cert = new System.Security.Cryptography.X509Certificates.X509Certificate2(
                    _config.RutaPfx,
                    _config.PasswordPfx,
                    System.Security.Cryptography.X509Certificates.X509KeyStorageFlags.MachineKeySet);

                sb.AppendLine("✅ PFX leído correctamente");
                sb.AppendLine($"   Subject:      {cert.Subject}");
                sb.AppendLine($"   Emisor:       {cert.Issuer}");
                sb.AppendLine($"   Válido desde: {cert.NotBefore:dd/MM/yyyy HH:mm}");
                sb.AppendLine($"   Válido hasta: {cert.NotAfter:dd/MM/yyyy HH:mm}");
                sb.AppendLine($"   Tiene clave privada: {cert.HasPrivateKey}");
                sb.AppendLine($"   Thumbprint:   {cert.Thumbprint}");

                // Verificar vigencia
                if (DateTime.Now < cert.NotBefore)
                    sb.AppendLine("⚠ ADVERTENCIA: El certificado todavía no es válido (fecha de inicio futura)");
                else if (DateTime.Now > cert.NotAfter)
                    sb.AppendLine("❌ ERROR: El certificado está VENCIDO");
                else
                    sb.AppendLine($"✅ Certificado vigente ({(cert.NotAfter - DateTime.Now).Days} días restantes)");

                // Verificar que el Subject contenga el CUIT
                string cuitLimpio = (_config.CUIT ?? "").Replace("-", "");
                if (!string.IsNullOrEmpty(cuitLimpio) && !cert.Subject.Contains(cuitLimpio))
                    sb.AppendLine($"⚠ ADVERTENCIA: El Subject del certificado no contiene el CUIT {cuitLimpio}");
                else if (!string.IsNullOrEmpty(cuitLimpio))
                    sb.AppendLine($"✅ CUIT {cuitLimpio} presente en el Subject");

                if (!cert.HasPrivateKey)
                    sb.AppendLine("❌ ERROR CRÍTICO: El PFX no contiene la clave privada — no se puede firmar");

                // Entorno
                sb.AppendLine($"\n   Entorno configurado: {(_config.Produccion ? "PRODUCCIÓN" : "HOMOLOGACIÓN")}");
                sb.AppendLine($"   URL WSFE:  {_config.UrlWsfe}");
                sb.AppendLine($"   Punto de venta: {_config.PuntoVenta}");
                sb.AppendLine($"   CUIT configurado: {_config.CUIT}");
            }
            catch (System.Security.Cryptography.CryptographicException ex)
            {
                sb.AppendLine("❌ Error al leer el PFX — contraseña incorrecta o archivo corrupto");
                sb.AppendLine($"   Detalle: {ex.Message}");
            }
            catch (Exception ex)
            {
                sb.AppendLine($"❌ Error inesperado: {ex.Message}");
            }
            return sb.ToString();
        }

        public ResultadoCAE SolicitarCAE(Pago pago, (string token, string sign) ticket)
        {
            if (pago == null) throw new ArgumentNullException(nameof(pago));

            int tipoCbte = pago.TipoComprobante > 0 ? pago.TipoComprobante : 11;

            // Consultar el último número autorizado ANTES de enviar el nuevo.
            // Importante: la numeración de una Nota de Crédito C (13) es INDEPENDIENTE
            // de la de Factura C (11) — cada tipo de comprobante lleva su propio correlativo.
            long ultimoNro = ObtenerUltimoNroComprobante(ticket, tipoCbte);
            long nuevoNro = ultimoNro + 1;

            string soapBody = ArmarSolicitudFECAE(pago, ticket, nuevoNro, tipoCbte);

            string respuestaXml = EnviarSoap(
                _config.UrlWsfe,
                soapBody,
                soapAction: "http://ar.gov.afip.dif.FEV1/FECAESolicitar");

            return ParsearRespuestaCAE(respuestaXml, nuevoNro);
        }

        /// <summary>Overload público para llamar desde UI de diagnóstico.</summary>
        public long ObtenerUltimoNroComprobante(int tipoCbte, (string token, string sign) ticket)
            => ObtenerUltimoNroComprobante(ticket, tipoCbte);

        private long ObtenerUltimoNroComprobante((string token, string sign) ticket, int tipoCbte)
        {
            string soap =
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<soapenv:Envelope " +
                "  xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
                "  xmlns:ar=\"http://ar.gov.afip.dif.FEV1/\">" +
                "  <soapenv:Header/>" +
                "  <soapenv:Body>" +
                "    <ar:FECompUltimoAutorizado>" +
                "      <ar:Auth>" +
                "        <ar:Token>" + ticket.token + "</ar:Token>" +
                "        <ar:Sign>" + ticket.sign + "</ar:Sign>" +
                "        <ar:Cuit>" + _config.CUIT + "</ar:Cuit>" +
                "      </ar:Auth>" +
                "      <ar:PtoVta>" + _config.PuntoVenta + "</ar:PtoVta>" +
                "      <ar:CbteTipo>" + tipoCbte + "</ar:CbteTipo>" +
                "    </ar:FECompUltimoAutorizado>" +
                "  </soapenv:Body>" +
                "</soapenv:Envelope>";

            string resp = EnviarSoap(
                _config.UrlWsfe,
                soap,
                soapAction: "http://ar.gov.afip.dif.FEV1/FECompUltimoAutorizado");

            var doc = new XmlDocument();
            doc.LoadXml(resp);
            VerificarErroresXml(doc, "FECompUltimoAutorizado");

            string nroStr = doc.SelectSingleNode("//*[local-name()='CbteNro']")?.InnerText ?? "0";
            return long.TryParse(nroStr, out long nro) ? nro : 0;
        }

        private string ArmarSolicitudFECAE(Pago pago, (string token, string sign) ticket,
            long nroComprobante, int tipoCbte)
        {
            string fechaCbte = pago.FechaPago.ToString("yyyyMMdd");

            // Concepto 2 = Servicios
            string fchServDesde = new DateTime(pago.FechaPago.Year, pago.FechaPago.Month, 1)
                .ToString("yyyyMMdd");
            string fchServHasta = new DateTime(pago.FechaPago.Year, pago.FechaPago.Month,
                DateTime.DaysInMonth(pago.FechaPago.Year, pago.FechaPago.Month))
                .ToString("yyyyMMdd");

            // Tipo de documento del receptor
            int docTipo;
            long docNro;
            if (string.IsNullOrWhiteSpace(pago.DocumentoPagador) || pago.DocumentoPagador == "0")
            {
                docTipo = 99; docNro = 0;   // Consumidor Final
            }
            else if (pago.TipoPagador == "CUIT")
            {
                docTipo = 80;
                docNro = long.Parse(pago.DocumentoPagador.Replace("-", "").Replace(" ", ""));
            }
            else
            {
                docTipo = 96;   // DNI
                docNro = long.Parse(pago.DocumentoPagador.Replace(".", "").Replace(" ", ""));
            }

            decimal impTotal = Math.Round(
                pago.TotalAplicado > 0 ? pago.TotalAplicado : pago.MontoRecibido, 2);

            int condIvaReceptor = docTipo == 99 ? 5 : 1;

            string imp = impTotal.ToString("F2", System.Globalization.CultureInfo.InvariantCulture);

            // Para Notas de Crédito, ARCA exige informar a qué comprobante acreditan
            // (CbtesAsoc). Va entre MonCotiz y CondicionIVAReceptorId según el esquema
            // de WSFEv1. Si por algún motivo faltara el número de la factura asociada,
            // preferimos dejarlo afuera y que ARCA rechace con un error claro, antes que
            // mandar un XML mal armado.
            string cbtesAsocXml = "";
            bool esNotaCredito = tipoCbte == 13 || tipoCbte == 3 || tipoCbte == 8;
            if (esNotaCredito && !string.IsNullOrEmpty(pago.NroComprobanteAsociado))
            {
                var partesAsoc = pago.NroComprobanteAsociado.Split('-');
                if (partesAsoc.Length == 2
                    && int.TryParse(partesAsoc[0], out int ptoVtaAsoc)
                    && long.TryParse(partesAsoc[1], out long nroAsoc))
                {
                    cbtesAsocXml =
                        "            <ar:CbtesAsoc>" +
                        "              <ar:CbteAsoc>" +
                        "                <ar:Tipo>11</ar:Tipo>" +
                        "                <ar:PtoVta>" + ptoVtaAsoc + "</ar:PtoVta>" +
                        "                <ar:Nro>" + nroAsoc + "</ar:Nro>" +
                        "              </ar:CbteAsoc>" +
                        "            </ar:CbtesAsoc>";
                }
            }

            return
                "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                "<soapenv:Envelope " +
                "  xmlns:soapenv=\"http://schemas.xmlsoap.org/soap/envelope/\" " +
                "  xmlns:ar=\"http://ar.gov.afip.dif.FEV1/\">" +
                "  <soapenv:Header/>" +
                "  <soapenv:Body>" +
                "    <ar:FECAESolicitar>" +
                "      <ar:Auth>" +
                "        <ar:Token>" + ticket.token + "</ar:Token>" +
                "        <ar:Sign>" + ticket.sign + "</ar:Sign>" +
                "        <ar:Cuit>" + _config.CUIT + "</ar:Cuit>" +
                "      </ar:Auth>" +
                "      <ar:FeCAEReq>" +
                "        <ar:FeCabReq>" +
                "          <ar:CantReg>1</ar:CantReg>" +
                "          <ar:PtoVta>" + _config.PuntoVenta + "</ar:PtoVta>" +
                "          <ar:CbteTipo>" + tipoCbte + "</ar:CbteTipo>" +
                "        </ar:FeCabReq>" +
                "        <ar:FeDetReq>" +
                "          <ar:FECAEDetRequest>" +
                "            <ar:Concepto>2</ar:Concepto>" +
                "            <ar:DocTipo>" + docTipo + "</ar:DocTipo>" +
                "            <ar:DocNro>" + docNro + "</ar:DocNro>" +
                "            <ar:CbteDesde>" + nroComprobante + "</ar:CbteDesde>" +
                "            <ar:CbteHasta>" + nroComprobante + "</ar:CbteHasta>" +
                "            <ar:CbteFch>" + fechaCbte + "</ar:CbteFch>" +
                "            <ar:ImpTotal>" + imp + "</ar:ImpTotal>" +
                "            <ar:ImpTotConc>0.00</ar:ImpTotConc>" +
                "            <ar:ImpNeto>" + imp + "</ar:ImpNeto>" +
                "            <ar:ImpOpEx>0.00</ar:ImpOpEx>" +
                "            <ar:ImpTrib>0.00</ar:ImpTrib>" +
                "            <ar:ImpIVA>0.00</ar:ImpIVA>" +
                "            <ar:FchServDesde>" + fchServDesde + "</ar:FchServDesde>" +
                "            <ar:FchServHasta>" + fchServHasta + "</ar:FchServHasta>" +
                "            <ar:FchVtoPago>" + fchServHasta + "</ar:FchVtoPago>" +
                "            <ar:MonId>PES</ar:MonId>" +
                "            <ar:MonCotiz>1</ar:MonCotiz>" +
                cbtesAsocXml +
                "            <ar:CondicionIVAReceptorId>" + condIvaReceptor + "</ar:CondicionIVAReceptorId>" +
                "          </ar:FECAEDetRequest>" +
                "        </ar:FeDetReq>" +
                "      </ar:FeCAEReq>" +
                "    </ar:FECAESolicitar>" +
                "  </soapenv:Body>" +
                "</soapenv:Envelope>";
        }

        private ResultadoCAE ParsearRespuestaCAE(string xml, long nroComprobante)
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            var resultado = new ResultadoCAE
            {
                NroComprobanteRaw = nroComprobante,
                NroComprobante = $"{_config.PuntoVenta:D5}-{nroComprobante:D8}"
            };

            var errNode = doc.SelectSingleNode("//*[local-name()='Errors']/*[local-name()='Err']");
            if (errNode != null)
            {
                resultado.Aprobado = false;
                resultado.MensajeError = $"Error ARCA [{errNode.SelectSingleNode("*[local-name()='Code']")?.InnerText}]: " +
                                          errNode.SelectSingleNode("*[local-name()='Msg']")?.InnerText;
                return resultado;
            }

            resultado.Aprobado = doc.SelectSingleNode("//*[local-name()='Resultado']")?.InnerText == "A";

            if (resultado.Aprobado)
            {
                resultado.CAE = doc.SelectSingleNode("//*[local-name()='CAE']")?.InnerText ?? "";
                string vencStr = doc.SelectSingleNode("//*[local-name()='CAEFchVto']")?.InnerText ?? "";
                if (vencStr.Length == 8)
                    resultado.CAEVencimiento = DateTime.ParseExact(vencStr, "yyyyMMdd",
                        System.Globalization.CultureInfo.InvariantCulture);
            }
            else
            {
                var obs = doc.SelectSingleNode("//*[local-name()='Obs']/*[local-name()='Ob']");
                resultado.MensajeError = obs != null
                    ? $"Rechazado [{obs.SelectSingleNode("*[local-name()='Code']")?.InnerText}]: {obs.SelectSingleNode("*[local-name()='Msg']")?.InnerText}"
                    : "Comprobante rechazado por ARCA (sin detalle).";
            }

            resultado.Observaciones = doc.SelectSingleNode("//*[local-name()='Observaciones']//*[local-name()='Msg']")?.InnerText;
            return resultado;
        }

        // ══════════════════════════════════════════════════════════════════
        //  HTTP helper
        // ══════════════════════════════════════════════════════════════════

        private string EnviarSoap(string url, string soapBody, string soapAction)
        {
            // Forzar TLS 1.2 — requerido por ARCA (los servicios AFIP no aceptan SSL3/TLS1.0)
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12
                                                 | SecurityProtocolType.Tls11;

            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "text/xml; charset=utf-8";
            request.Timeout = 30_000;

            // SOAPAction: si es vacío, mandar comillas vacías (estándar SOAP 1.1)
            // Si tiene valor, mandarlo sin comillas adicionales (ARCA lo espera así)
            request.Headers["SOAPAction"] = string.IsNullOrEmpty(soapAction)
                ? "\"\""
                : soapAction;

            byte[] bytes = Encoding.UTF8.GetBytes(soapBody);
            request.ContentLength = bytes.Length;

            using (var stream = request.GetRequestStream())
                stream.Write(bytes, 0, bytes.Length);

            try
            {
                using (var response = (HttpWebResponse)request.GetResponse())
                using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    return reader.ReadToEnd();
            }
            catch (WebException webEx) when (webEx.Response != null)
            {
                // ARCA devuelve errores SOAP como HTTP 500 — hay que leer el body igual
                using (var errStream = webEx.Response.GetResponseStream())
                using (var reader = new StreamReader(errStream, Encoding.UTF8))
                {
                    string errorBody = reader.ReadToEnd();
                    // Si es un SOAP Fault, parsearlo y lanzar un mensaje legible
                    if (errorBody.Contains("<faultstring>"))
                    {
                        var errDoc = new XmlDocument();
                        try { errDoc.LoadXml(errorBody); } catch { }
                        string fault = errDoc.SelectSingleNode("//*[local-name()='faultstring']")?.InnerText ?? errorBody;
                        throw new Exception("ARCA devolvió error SOAP: " + fault);
                    }
                    throw new Exception("Error HTTP desde ARCA:\n" + errorBody);
                }
            }
        }

        private void VerificarErroresXml(XmlDocument doc, string metodo)
        {
            var errNode = doc.SelectSingleNode("//*[local-name()='Errors']/*[local-name()='Err']");
            if (errNode != null)
                throw new Exception(
                    $"ARCA/{metodo} error [{errNode.SelectSingleNode("*[local-name()='Code']")?.InnerText}]: " +
                    errNode.SelectSingleNode("*[local-name()='Msg']")?.InnerText);
        }

        private static string LeerCuerpoError(WebException webEx)
        {
            if (webEx.Response == null) return webEx.Message;
            try
            {
                using (var s = webEx.Response.GetResponseStream())
                using (var r = new StreamReader(s, Encoding.UTF8))
                    return r.ReadToEnd();
            }
            catch { return webEx.Message; }
        }
    }
}


