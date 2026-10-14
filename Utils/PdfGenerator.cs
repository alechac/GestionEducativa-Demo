using GestionEducativa.Models;
using GestionEducativa.Services;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Policy;
using System.Text;
using QRCoder;
using System.Drawing;

namespace GestionEducativa.Utils
{
    /// <summary>
    /// Genera comprobantes en formato texto/HTML hasta que se integre iTextSharp.
    /// Los archivos se guardan en Mis Documentos\GestionEducativa\Comprobantes\.
    /// </summary>
    public class PdfGenerator
    {
        private readonly ArcaConfigService _configService;

        public PdfGenerator()
        {
            _configService = new ArcaConfigService();
        }

        // ─────────────────────────────────────────────
        private string ObtenerCarpeta()
        {
            string carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "GestionEducativa", "Comprobantes");
            Directory.CreateDirectory(carpeta);
            return carpeta;
        }

        /// <summary>
        /// Genera el comprobante HTML y lo abre en el navegador predeterminado.
        /// Devuelve la ruta del archivo generado.
        /// </summary>
        public string GenerarComprobantePagoMultiple(Pago pago)
        {
            ArcaConfig cfg = null;
            try { cfg = _configService.Cargar(); } catch { }

            string carpeta = ObtenerCarpeta();

            // Usar el número oficial de ARCA si está disponible, si no el ID de BD.
            // Prefijo FC/NC porque Factura y Nota de Crédito tienen numeración
            // INDEPENDIENTE en ARCA — sin esto, una Factura Nº 42 y una NC Nº 42
            // generarían el mismo nombre de archivo y se pisarían entre sí.
            string prefijoTipo = pago.TipoComprobante == 13 ? "NC" : "FC";
            string nroArchivo;
            if (!string.IsNullOrEmpty(pago.NroComprobanteArca))
                nroArchivo = pago.NroComprobanteArca.Replace("-", "_");
            else
                nroArchivo = pago.Id.ToString("D8");

            string archivo = Path.Combine(carpeta, $"Comprobante_{prefijoTipo}_{nroArchivo}.html");

            string html = BuildHtml(pago, cfg);
            File.WriteAllText(archivo, html, Encoding.UTF8);
            return archivo;
        }

        // ─────────────────────────────────────────────
        // HTML que replica la factura C de ARCA
        // ─────────────────────────────────────────────
        private string BuildHtml(Pago pago, ArcaConfig cfg)
        {
            // ── Datos del emisor ──
            string razonSocial = cfg?.RazonSocial ?? "SOC DANTE ALIGHIERI DE TUCUMAN";
            string domComercial = cfg?.DomicilioComercial ?? "";
            string cuitEmisor = cfg?.CUIT ?? "";
            string cuitReceptor = "";  // lo llenamos según el pago
            string iibb = cuitEmisor;
            string inicioAct = "21/05/1992";

            int puntoVenta = cfg?.PuntoVenta ?? 1;

            // ── Tipo de comprobante ──
            bool esNotaCredito = pago.TipoComprobante == 13;
            bool esFactC = !esNotaCredito && (pago.TipoComprobante == 11 || pago.TipoComprobante == 0);
            string tipoLetra = (esFactC || esNotaCredito) ? "C" : "B";
            string tipoNombre = esNotaCredito ? "NOTA DE CRÉDITO" : "FACTURA";
            string tipoLabel = $"COD. {(esNotaCredito ? "013" : esFactC ? "011" : "006")}";

            // ── Numeración ──
            string ptoVtaStr = puntoVenta.ToString("D5");
            string nroCompStr = pago.Id.ToString("D8");
            // Si ya tenemos el nro oficial de ARCA lo usamos
            if (!string.IsNullOrEmpty(pago.NroComprobanteArca))
            {
                var parts = pago.NroComprobanteArca.Split('-');
                if (parts.Length == 2) { ptoVtaStr = parts[0]; nroCompStr = parts[1]; }
            }

            // ── Fechas ──
            string fechaEmision = pago.FechaPago.ToString("dd/MM/yyyy");
            DateTime periodoDesde = pago.PeriodoDesde == default
                ? new DateTime(pago.FechaPago.Year, pago.FechaPago.Month, 1)
                : pago.PeriodoDesde;
            DateTime periodoHasta = pago.PeriodoHasta == default
                ? pago.FechaPago.Date
                : pago.PeriodoHasta;
            string vtoStr = pago.FechaPago.ToString("dd/MM/yyyy");

            // ── Receptor ──
            // Mostrar CUIT si tiene, si no DNI — nunca dejar en blanco
            string docReceptorLabel;
            string docReceptorValor;
            if (pago.TipoPagador == "CUIT" && !string.IsNullOrEmpty(pago.DocumentoPagador))
            {
                docReceptorLabel = "CUIL:";
                docReceptorValor = pago.DocumentoPagador;
                cuitReceptor = pago.DocumentoPagador;
            }
            else if (!string.IsNullOrEmpty(pago.DocumentoPagador))
            {
                docReceptorLabel = "DNI:";
                docReceptorValor = pago.DocumentoPagador;
                cuitReceptor = "";   // no tiene CUIT
            }
            else
            {
                docReceptorLabel = "DNI:";
                docReceptorValor = "";
            }
            string condIVAReceptor = "Consumidor Final";

            string nombreReceptor = pago.NombrePagador ?? "";
            string domReceptor = pago.DomicilioPagador ?? "";

            // ── CAE ──
            string caeNro = pago.CAE ?? "";
            string caeVto = pago.CAEVencimiento.HasValue
                ? pago.CAEVencimiento.Value.ToString("dd/MM/yyyy") : "";

            string formaPago = pago.FormaPago;

            // ── Ítems ──
            decimal subtotalGral = 0;
            var sbItems = new StringBuilder();
            foreach (var det in pago.Detalles)
            {
                string desc = BuildDescripcion(det, esNotaCredito);

                decimal precioUnit = det.PrecioUnitario;
                decimal pctBonif = det.PorcentajeBonif;
                decimal impBonif = det.ImporteBonif;
                decimal subtotal = det.Subtotal > 0 ? det.Subtotal : precioUnit - impBonif;

                subtotalGral += subtotal;

                sbItems.AppendLine($@"
                <tr>
                  <td class='desc'>{desc}</td>
                  <td class='num'>1,00</td>
                  <td>unidades</td>
                  <td class='num'>{precioUnit:N2}</td>
                  <td class='num'>{(pctBonif > 0 ? pctBonif.ToString("0.##") : "0,00")}</td>
                  <td class='num'>{(impBonif > 0 ? impBonif.ToString("N2") : "0,00")}</td>
                  <td class='num'>{subtotal:N2}</td>
                </tr>");
            }

            // Observaciones al final del último ítem
            if (!string.IsNullOrWhiteSpace(pago.Observaciones))
            {
                sbItems.AppendLine($@"
                <tr>
                  <td class='desc' colspan='7' style='font-style:italic;color:#444;border-top:2px solid #aaa;'>
                    <strong>Observaciones:</strong> {Esc(pago.Observaciones)}
                  </td>
                </tr>");
            }

            // Si hay recargo por mora lo mostramos como ítem separado
            // SOLO para pagos que incluyan cuotas Curriculares (no matrículas ni otros conceptos)
            bool tieneCuotaCurricular = pago.Detalles != null &&
                pago.Detalles.Any(d => string.Equals(d.TipoCuota, "Curricular", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(d.TipoCuota, "Programatica", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(d.TipoCuota, "Programática", StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(d.TipoCuota, "Cuota Mensual", StringComparison.OrdinalIgnoreCase));
            decimal recargo = pago.TotalAplicado - subtotalGral;
            if (recargo > 0.01m && tieneCuotaCurricular)
            {
                sbItems.AppendLine($@"
                <tr>
                  <td class='desc'>PAGO FUERA DE TERMINO</td>
                  <td class='num'>1,00</td>
                  <td>unidades</td>
                  <td class='num'>{recargo:N2}</td>
                  <td class='num'>0,00</td>
                  <td class='num'>0,00</td>
                  <td class='num'>{recargo:N2}</td>
                </tr>");
                subtotalGral += recargo;
            }

            decimal total = pago.TotalAplicado > 0 ? pago.TotalAplicado : subtotalGral;

            // ── QR ARCA (especificación oficial RG 4892/2020) ──
            string qrUrl = "";
            string qrHtml = "";
            if (!string.IsNullOrEmpty(pago.CAE))
            {
                // Determinar tipo y número de documento receptor
                int tipoDocRec = 0;
                long nroDocRec = 0;
                if (pago.TipoPagador == "CUIT" && !string.IsNullOrEmpty(pago.DocumentoPagador))
                {
                    tipoDocRec = 80;  // CUIT
                    long.TryParse(pago.DocumentoPagador.Replace("-", "").Replace(".", ""), out nroDocRec);
                }
                else if (!string.IsNullOrEmpty(pago.DocumentoPagador))
                {
                    tipoDocRec = 96;  // DNI
                    long.TryParse(pago.DocumentoPagador.Replace(".", "").Trim(), out nroDocRec);
                }

                long cuitNum = 0;
                long.TryParse((cuitEmisor ?? "").Replace("-", ""), out cuitNum);
                long caeNum = 0;
                long.TryParse(pago.CAE, out caeNum);

                // Extraer el número de comprobante de NroComprobanteArca (ej: "00004-00000055" → 55)
                long nroCompQR = pago.Id;  // fallback al ID interno
                if (!string.IsNullOrEmpty(pago.NroComprobanteArca))
                {
                    var partsQR = pago.NroComprobanteArca.Split('-');
                    if (partsQR.Length == 2 && long.TryParse(partsQR[1], out long nroParsed))
                        nroCompQR = nroParsed;
                }

                // Construir JSON según especificación oficial RG 4892/2020
                //string importeStr = (total == Math.Floor(total))
                //        ? ((long)total).ToString(System.Globalization.CultureInfo.InvariantCulture)
                //        : total.ToString("G", System.Globalization.CultureInfo.InvariantCulture);
                string importeStr = total.ToString("0.00", CultureInfo.InvariantCulture);
                var jsonParts = new System.Collections.Generic.List<string>
                {
                    "\"ver\":1",
                    $"\"fecha\":\"{pago.FechaPago:yyyy-MM-dd}\"",
                    $"\"cuit\":{cuitNum}",
                    $"\"ptoVta\":{puntoVenta}",
                    "\"tipoCmp\":" + pago.TipoComprobante,
                    $"\"nroCmp\":{nroCompQR}",
                    $"\"importe\":{total.ToString("F2", CultureInfo.InvariantCulture)}",
                    $"\"moneda\":\"PES\"",
                    $"\"ctz\":1"
                };

                if (tipoDocRec > 0 && nroDocRec > 0)
                {
                    jsonParts.Add($"\"tipoDocRec\":{tipoDocRec}");
                    jsonParts.Add($"\"nroDocRec\":{nroDocRec}");
                }

                jsonParts.Add("\"tipoCodAut\":\"E\"");
                jsonParts.Add($"\"codAut\":{caeNum}");

                string json = "{" + string.Join(",", jsonParts) + "}";

                // Base64 URL-safe: sin +, /, = para que los lectores QR lo lean bien
                string base64Raw = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(json));

                qrUrl = $"https://www.arca.gob.ar/fe/qr/?p={base64Raw}";

                var qrGenerator = new QRCodeGenerator();

                var qrData = qrGenerator.CreateQrCode(
                    qrUrl,
                    QRCodeGenerator.ECCLevel.Q);

                var qrCode = new PngByteQRCode(qrData);

                byte[] qrBytes = qrCode.GetGraphic(20);

                string qrImageBase64 =
                    Convert.ToBase64String(qrBytes);



                qrHtml = $@"
                    <img
                    src='data:image/png;base64,{qrImageBase64}'
                    width='150'
                    height='150'
                    alt='QR ARCA'
                    style='display:block;' />";
            }
            else
            {
                // Sin CAE todavía: cuadrado gris de placeholder
                qrHtml = "<div style='width:80px;height:80px;border:2px solid #ccc;background:#f5f5f5;" +
                          "display:flex;align-items:center;justify-content:center;font-size:9px;" +
                          "color:#aaa;text-align:center;'>Sin<br/>CAE</div>";
            }


            // ─────── CSS ───────
            string css = @"
* { box-sizing: border-box; margin: 0; padding: 0; }
body {
    font-family: Arial, sans-serif;
    font-size: 11px;
    color: #111;
    background: #fff;
    padding: 20px;
}
/* Limitar el ancho al A4 cuando se imprime */
@media print {
    body { padding: 0; }
    .no-print { display: none; }
}
/* Botón de imprimir: solo se ve en pantalla (navegador). El PDF que se genera con
   Puppeteer usa el mismo media type 'print' que usa el navegador al imprimir, así
   que .no-print también lo oculta ahí — no aparece ni en la impresión ni en el PDF
   que se manda por mail, solo lo ve la administrativa al abrir el comprobante. */
.btn-imprimir {
    position: fixed;
    top: 16px;
    left: 50%;
    transform: translateX(-50%);
    background: #1a5276;
    color: #fff;
    border: none;
    border-radius: 6px;
    padding: 10px 20px;
    font-size: 14px;
    font-weight: bold;
    font-family: Arial, sans-serif;
    cursor: pointer;
    box-shadow: 0 2px 6px rgba(0,0,0,0.25);
}
.btn-imprimir:hover { background: #21618c; }
.page {
    max-width: 780px;
    margin: 0 auto;
    border: 1px solid #ccc;
    padding: 16px;
}
/* ── Encabezado ── */
.header {
    display: grid;
    grid-template-columns: 1fr 120px 1fr;
    gap: 8px;
    border: 1px solid #000;
    margin-bottom: 4px;
}
.header-left, .header-right {
    padding: 8px;
}
.header-left p, .header-right p { margin-bottom: 3px; }
.header-left .razon { font-weight: bold; font-size: 13px; }
.header-left .label { color: #444; }
.badge {
    border-left: 1px solid #000;
    border-right: 1px solid #000;
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 8px 4px;
    text-align: center;
}
.badge .letra {
    font-size: 52px;
    font-weight: bold;
    line-height: 1;
}
.badge .cod {
    font-size: 10px;
    margin-top: 2px;
}
.badge .tipo {
    font-size: 18px;
    font-weight: bold;
    margin-top: 4px;
}
/* ── Datos nro/fecha ── */
.datos-comp {
    display: flex;
    justify-content: flex-end;
    gap: 40px;
    font-size: 11px;
    margin-bottom: 4px;
}
.datos-comp .field { margin-bottom: 2px; }
.datos-comp .label { color: #555; margin-right: 4px; }
/* ── Período ── */
.periodo {
    border: 1px solid #000;
    display: grid;
    grid-template-columns: 1fr 1fr 1fr;
    font-size: 10px;
    padding: 4px 8px;
    margin-bottom: 4px;
}
.periodo span { }
.periodo strong { font-size: 11px; }
/* ── Receptor ── */
.receptor {
    border: 1px solid #000;
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 4px;
    padding: 6px 8px;
    font-size: 11px;
    margin-bottom: 4px;
}
.receptor .field { margin-bottom: 2px; }
.receptor .label { font-weight: bold; }
/* ── Items ── */
table.items {
    width: 100%;
    border-collapse: collapse;
    font-size: 10.5px;
    margin-bottom: 4px;
}
table.items th {
    background: #eee;
    border: 1px solid #aaa;
    padding: 4px 6px;
    text-align: center;
}
table.items td {
    border: 1px solid #ccc;
    padding: 4px 6px;
    vertical-align: top;
}
table.items td.desc { min-width: 200px; }
table.items td.num  { text-align: right; white-space: nowrap; }
/* ── Totales ── */
.totales {
    width: 100%;
    margin-top: 4px;
    display: flex;
    justify-content: flex-end;
}
.totales table {
    font-size: 11px;
    border-collapse: collapse;
}
.totales td { padding: 3px 10px; border: 1px solid #ccc; }
.totales td.lbl { text-align: right; color: #444; }
.totales td.val { text-align: right; font-weight: bold; min-width: 100px; }
.totales tr.grand td { font-size: 13px; background: #f5f5f5; }
/* ── Footer ARCA ── */
.footer-arca {
    margin-top: 12px;
    border-top: 2px solid #000;
    padding-top: 8px;
    display: flex;
    align-items: flex-start;
    gap: 16px;
}
.footer-arca .cae-block { flex: 1; font-size: 11px; }
.footer-arca .cae-block p { margin-bottom: 3px; }
.footer-arca .authorized { font-weight: bold; font-style: italic; margin-top: 4px; }
.footer-nota {
    margin-top: 6px;
    font-size: 9px;
    color: #666;
    font-style: italic;
}
/* Botón imprimir */
.btn-print {
    display: block;
    margin: 16px auto 0;
    padding: 8px 32px;
    background: #1a5276;
    color: #fff;
    border: none;
    border-radius: 4px;
    font-size: 13px;
    cursor: pointer;
}
";

            // ─────── HTML ───────
            var sb = new StringBuilder();
            sb.AppendLine($@"<!DOCTYPE html>
<html lang='es'>
<head>
<meta charset='UTF-8'>
<title>Comprobante {ptoVtaStr}-{nroCompStr}</title>
<style>{css}</style>
</head>
<body>
 
<button class='no-print btn-imprimir' onclick='window.print()'>🖨️ Imprimir</button>
 
<div class='page'>
 
<!-- ══ ENCABEZADO ══ -->
<div class='header'>
  <div class='header-left'>
    <p class='razon'>{Esc(razonSocial)}</p>
    <p class='label'>Razón Social: <strong>{Esc(razonSocial)}</strong></p>
    <p class='label'>Domicilio Comercial: <strong>{Esc(domComercial)}</strong></p>
    <p class='label'>Condición frente al IVA: <strong>IVA Sujeto Exento</strong></p>
  </div>
 
  <div class='badge'>
    <div class='letra'>{tipoLetra}</div>
    <div class='cod'>{tipoLabel}</div>
    <div class='tipo'>{tipoNombre}</div>
  </div>
 
  <div class='header-right'>
    <p><strong>Punto de Venta: {ptoVtaStr} &nbsp;&nbsp; Comp. Nro: {nroCompStr}</strong></p>
    <p>Fecha de Emisión: <strong>{fechaEmision}</strong></p>
    <br/>
    <p>CUIT: <strong>{FormatCuit(cuitEmisor)}</strong></p>
    <p>Ingresos Brutos: <strong>{iibb}</strong></p>
    <p>Fecha de Inicio de Actividades: <strong>{inicioAct}</strong></p>
  </div>
</div>
 
<!-- ══ PERÍODO FACTURADO ══ -->
<div class='periodo'>
  <span><strong>Período Facturado Desde:</strong> {periodoDesde:dd/MM/yyyy}</span>
  <span><strong>Hasta:</strong> {periodoHasta:dd/MM/yyyy}</span>
  <span><strong>Fecha de Vto. para el pago:</strong> {vtoStr}</span>
</div>
 
<!-- ══ RECEPTOR ══ -->
<div class='receptor'>
  <div>
    <div class='field'><span class='label'>{docReceptorLabel}</span> {Esc(docReceptorValor)}</div>
    <div class='field'><span class='label'>Condición frente al IVA:</span> {condIVAReceptor}</div>
    <div class='field'><span class='label'>Condición de venta:</span>{Esc(formaPago)}</div>
  </div>
  <div>
    <div class='field'><span class='label'>Apellido y Nombre / Razón Social:</span> {Esc(nombreReceptor)}</div>
    <div class='field'><span class='label'>Domicilio:</span> {Esc(domReceptor)}</div>
    {(esNotaCredito ? $"<div class='field'><span class='label'>Fac. C:</span> {Esc(pago.NroComprobanteAsociado ?? "-")}</div>" : "")}
  </div>
</div>
 
<!-- ══ TABLA DE ÍTEMS ══ -->
<table class='items'>
  <thead>
    <tr>
      <th style='text-align:left;'>Producto / Servicio</th>
      <th>Cantidad</th>
      <th>U. Medida</th>
      <th>Precio Unit.</th>
      <th>% Bonif</th>
      <th>Imp. Bonif.</th>
      <th>Subtotal</th>
    </tr>
  </thead>
  <tbody>
    {sbItems}
  </tbody>
</table>
 
<!-- ══ TOTALES ══ -->
<div class='totales'>
  <table>
    <tr><td class='lbl'>Subtotal: $</td><td class='val'>{total:N2}</td></tr>
    <tr><td class='lbl'>Importe Otros Tributos: $</td><td class='val'>0,00</td></tr>
    <tr class='grand'><td class='lbl'>Importe Total: $</td><td class='val'>{total:N2}</td></tr>
  </table>
</div>
 
<!-- ══ FOOTER ARCA ══ -->
<div class='footer-arca'>
  <a href='{qrUrl}' target='_blank' title='Verificar comprobante en ARCA' style='display:inline-block;'>
    {qrHtml}
  </a>
  <div style='display:flex;flex-direction:column;gap:4px;'>
    <img src='https://www.afip.gob.ar/common/img/logo_afip.gif' alt='ARCA'
         style='height:36px;' onerror=""this.style.display='none'"" />
    <span style='font-size:9px;color:#555;'>AGENCIA DE RECAUDACIÓN<br/>Y CONTROL ADUANERO</span>
  </div>
  <div class='cae-block'>
    <p>Pág. 1/1</p>
    <p><strong>CAE N°: {caeNro}</strong></p>
    <p><strong>Fecha de Vto. de CAE: {caeVto}</strong></p>
    <p class='authorized'>Comprobante Autorizado</p>
    <p class='footer-nota'>Esta Agencia no se responsabiliza por los datos ingresados en el detalle de la operación</p>
  </div>
</div>
 
</div><!-- .page -->
 
</body>
</html>");

            return sb.ToString();
        }

        // ─────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────

        /// <summary>
        /// Devuelve HTML listo para insertar en la celda de la tabla.
        /// — Pago total:   "CTA JULIO CURRICULAR<br/>CHACON ALEJANDRO"
        /// — Pago parcial: "A CUENTA CTA JULIO CURRICULAR<br/>CHACON ALEJANDRO"
        /// </summary>
        private static string BuildDescripcion(DetallePago det, bool esNotaCredito = false)
        {
            // Si el concepto tiene un nombre propio cargado por el usuario (ej: "Reserva de Matrícula",
            // "Seguro Escolar"), ese nombre tiene prioridad sobre la etiqueta genérica derivada de
            // TipoCuota — así el comprobante refleja lo que realmente se cobró, no solo la familia
            // ("Matricula") a la que pertenece internamente.
            string nombreConcepto = (det.NombreConcepto ?? "").Trim();

            string tipoCuota = (det.TipoCuota ?? "").Trim();
            string tipoCuotaUp = tipoCuota.ToUpper();

            string tipo;
            if (!string.IsNullOrEmpty(nombreConcepto))
                tipo = nombreConcepto.ToUpper();
            else if (tipoCuotaUp == "MATRICULA" || tipoCuotaUp == "MATRÍCULA")
                tipo = "MATRÍCULA";
            // "EXTRA" cubre tanto el valor histórico "Extracurricular" como el nuevo "Extraprogramática"
            else if (tipoCuotaUp.Contains("EXTRA"))
                tipo = "EXTRAPROGRAMÁTICA";
            // "CURRICULAR" cubre el valor histórico; "PROGRAMATICA"/"PROGRAMÁTICA" el nuevo, tras el
            // cambio de nomenclatura de la resolución estudiantil.
            else if (tipoCuotaUp == "CURRICULAR" || tipoCuotaUp == "PROGRAMATICA" || tipoCuotaUp == "PROGRAMÁTICA")
                tipo = "CUOTA PROGRAMÁTICA";
            else if (string.IsNullOrEmpty(tipoCuota))
                // Antes esto caía en "CURRICULAR" por defecto, lo que hacía que cualquier cuota con
                // tipoCuota vacío/NULL en la base (por ej. un concepto mal cargado) apareciera en el
                // comprobante como si fuera una cuota Programática más, generando líneas "duplicadas".
                // Mostrarlo como "OTRO CONCEPTO" en vez de ocultar el problema.
                tipo = "OTRO CONCEPTO";
            else
                tipo = tipoCuotaUp; // Seguro, Viaje, Quadernetti, etc.

            string periodo = det.PeriodoCuota ?? "";
            // PeriodoCuota puede venir con el tipo/concepto repetido al principio o al final
            // (ej: "RESERVA DE MATRÍCULA PARA 2 AÑO 2027" o "ABRIL 2026 (PROGRAMATICA)") — eso es
            // útil en otras pantallas (ARCA/AFIP, resumen de confirmación) donde el tipo no se
            // muestra por separado, pero ACÁ ya lo tenemos como prefijo (variable `tipo`), así que
            // lo sacamos de `periodo` para no repetirlo en la misma línea del comprobante.
            //int parenIdx = periodo.IndexOf('(');
            //if (parenIdx > 0)
            //    periodo = periodo.Substring(0, parenIdx).Trim();
            //string periodoUp = periodo.ToUpper().Trim();
            //if (!string.IsNullOrEmpty(tipo) && periodoUp.StartsWith(tipo, StringComparison.Ordinal))
            //    periodoUp = periodoUp.Substring(tipo.Length).Trim();
            //periodo = Esc(periodoUp);

            // Prefijo "A CUENTA" cuando el pago no cubrió la cuota completa.
            // En una Nota de Crédito no aplica (no es "a cuenta" de nada, es una acreditación).
            bool esParcial = !esNotaCredito && det.SaldoRestante > 0;
            string prefijo = esParcial ? "A CUENTA " : "";

            string linea1 = $"{prefijo}{tipo}/ {periodo}";
            string alumno = det.AlumnoNombre?.ToUpper().Trim() ?? "";

            // Cuando el pago fue parcial, se informa a continuación del ítem cuánto queda
            // adeudado de esa cuota puntual — como texto normal dentro de la descripción,
            // igual que hace AFIP/ARCA con las leyendas de un comprobante (sin columna aparte
            // ni resaltado, para no alterar el formato estándar del comprobante). En una Nota
            // de Crédito, esto muestra el saldo que la cuota vuelve a deber tras acreditarla.
            string debe = det.SaldoRestante > 0.004m
                ? $"<br/>(SALDO $ {det.SaldoRestante:N2})"
                : "";

            return (string.IsNullOrEmpty(alumno)
                ? linea1
                : $"{linea1}<br/>{Esc(alumno)}") + debe;
        }

        private static string FormatCuit(string cuit)
        {
            if (string.IsNullOrEmpty(cuit) || cuit.Length != 11) return cuit;
            return $"{cuit.Substring(0, 2)}-{cuit.Substring(2, 8)}-{cuit.Substring(10, 1)}";
        }

        private static string Esc(string s) =>
            (s ?? "").Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
    }

}
