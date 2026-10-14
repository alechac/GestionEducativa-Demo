using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.Pkcs;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml;

namespace GestionEducativa.Utils
{
    public static class CertificadoHelper
    {
        /// <summary>
        /// Genera el CMS firmado en Base64 listo para enviar al WSAA.
        /// </summary>
        /// <param name="rutaPfx">Ruta completa al archivo .pfx</param>
        /// <param name="passwordPfx">Contraseña del .pfx (puede ser vacía "")</param>
        /// <param name="servicio">Siempre "wsfe" para facturación electrónica</param>
        public static string GenerarLoginTicketRequest(string rutaPfx, string passwordPfx, string servicio = "wsfe")
        {
            // 1. Cargar el certificado desde el .pfx
            var cert = new X509Certificate2(rutaPfx, passwordPfx,
                X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);

            // 2. Armar el XML del LoginTicketRequest
            DateTime ahora = DateTime.UtcNow;
            string xml = ArmarLoginTicketRequestXml(servicio, ahora);

            // 3. Firmar con CMS (PKCS#7 / SignedData)
            byte[] xmlBytes = Encoding.UTF8.GetBytes(xml);
            var content = new ContentInfo(xmlBytes);
            var signedCms = new SignedCms(content, detached: false);

            var signer = new CmsSigner(SubjectIdentifierType.IssuerAndSerialNumber, cert)
            {
                DigestAlgorithm = new Oid("SHA256"),         // SHA-256 — ARCA acepta SHA1 y SHA256
                IncludeOption = X509IncludeOption.EndCertOnly
            };

            signedCms.ComputeSignature(signer, silent: true);
            byte[] cms = signedCms.Encode();

            return Convert.ToBase64String(cms);
        }

        private static string ArmarLoginTicketRequestXml(string servicio, DateTime ahora)
        {
            // ARCA exige: generationTime 10min antes, expirationTime 10min después (max 12hs)
            string generacion = ahora.AddMinutes(-10).ToString("yyyy-MM-ddTHH:mm:ssZ");
            string expiracion = ahora.AddMinutes(600).ToString("yyyy-MM-ddTHH:mm:ssZ");  // +10 hs
            long uniqueId = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

            var sb = new StringBuilder();
            sb.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\" ?>");
            sb.AppendLine("<loginTicketRequest version=\"1.0\">");
            sb.AppendLine("  <header>");
            sb.AppendLine($"    <uniqueId>{uniqueId}</uniqueId>");
            sb.AppendLine($"    <generationTime>{generacion}</generationTime>");
            sb.AppendLine($"    <expirationTime>{expiracion}</expirationTime>");
            sb.AppendLine("  </header>");
            sb.AppendLine($"  <service>{servicio}</service>");
            sb.AppendLine("</loginTicketRequest>");

            return sb.ToString();
        }
    }
}
