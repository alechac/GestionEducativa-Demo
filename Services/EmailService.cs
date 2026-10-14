using GestionEducativa.Models;
using System;
using System.IO;
using System.Net;
using System.Net.Mail;

namespace GestionEducativa.Services
{
    /// <summary>
    /// Envía por mail el comprobante de pago emitido al padre/tutor que figura
    /// como pagador, para que quede evidencia de la entrega ante una eventual
    /// pérdida del comprobante en papel/PDF.
    /// </summary>
    public class EmailService
    {
        private readonly EmailConfigService _configService;

        public EmailService()
        {
            _configService = new EmailConfigService();
        }

        /// <summary>
        /// Envía el comprobante ya generado (archivo HTML) al padre indicado.
        /// Devuelve true si se envió correctamente; si no, "error" trae el motivo
        /// (no se lanza excepción para no interrumpir el flujo de cobro/impresión).
        /// </summary>
        public bool EnviarComprobante(Pago pago, Padre padreDestino, string archivoAdjunto, out string error)
        {
            error = null;

            EmailConfig cfg;
            try
            {
                cfg = _configService.Cargar();
            }
            catch (Exception ex)
            {
                error = "No se pudo cargar la configuración de email: " + ex.Message;
                return false;
            }

            if (!cfg.Habilitado)
            {
                error = "El envío de comprobantes por mail está deshabilitado (Configuración → Email).";
                return false;
            }

            try
            {
                _configService.Validar(cfg);
            }
            catch (Exception ex)
            {
                error = ex.Message;
                return false;
            }

            if (padreDestino == null || string.IsNullOrWhiteSpace(padreDestino.Email))
            {
                error = "El padre/tutor que registró el pago no tiene un email cargado en el sistema.";
                return false;
            }

            if (!EsEmailValido(padreDestino.Email))
            {
                error = $"El email cargado para {padreDestino.NombreCompleto} ('{padreDestino.Email}') no es válido.";
                return false;
            }

            try
            {
                using (var mensaje = new MailMessage())
                {
                    string nombreRemitente = string.IsNullOrWhiteSpace(cfg.RemitenteNombre)
                        ? cfg.RemitenteEmail
                        : cfg.RemitenteNombre;

                    mensaje.From = new MailAddress(cfg.RemitenteEmail, nombreRemitente);
                    mensaje.To.Add(new MailAddress(padreDestino.Email.Trim(), padreDestino.NombreCompleto));

                    if (!string.IsNullOrWhiteSpace(cfg.CopiaOculta))
                        mensaje.Bcc.Add(new MailAddress(cfg.CopiaOculta.Trim()));

                    string numero = !string.IsNullOrEmpty(pago.NroComprobanteArca)
                        ? pago.NroComprobanteArca
                        : pago.Id.ToString("D8");

                    string tipoTexto = pago.TipoComprobante == 13 ? "Nota de Crédito" : "Comprobante de pago";
                    mensaje.Subject = $"{tipoTexto} N° {numero} — {padreDestino.NombreCompleto}";
                    mensaje.IsBodyHtml = true;
                    mensaje.Body = "Se adjunta el comprobante emitido.";

                    if (!string.IsNullOrEmpty(archivoAdjunto) && File.Exists(archivoAdjunto))
                        mensaje.Attachments.Add(new Attachment(archivoAdjunto));

                    using (var smtp = new SmtpClient(cfg.SmtpHost, cfg.SmtpPuerto))
                    {
                        smtp.EnableSsl = cfg.UsarSsl;
                        smtp.Credentials = new NetworkCredential(cfg.SmtpUsuario, cfg.SmtpPassword);
                        smtp.Send(mensaje);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                error = "No se pudo enviar el email: " + ex.Message;
                return false;
            }
        }

        /// <summary>Prueba la conexión/autenticación SMTP enviando un mail de prueba al propio remitente.</summary>
        public void EnviarPrueba(EmailConfig cfg, string destinatarioPrueba)
        {
            _configService.Validar(cfg);

            using (var mensaje = new MailMessage())
            {
                string nombreRemitente = string.IsNullOrWhiteSpace(cfg.RemitenteNombre) ? cfg.RemitenteEmail : cfg.RemitenteNombre;
                mensaje.From = new MailAddress(cfg.RemitenteEmail, nombreRemitente);
                mensaje.To.Add(new MailAddress(destinatarioPrueba));
                mensaje.Subject = "Prueba de configuración de email — Gestión Educativa";
                mensaje.IsBodyHtml = false;
                mensaje.Body = "Este es un mail de prueba. Si lo recibiste, la configuración de envío de comprobantes está funcionando correctamente.";

                using (var smtp = new SmtpClient(cfg.SmtpHost, cfg.SmtpPuerto))
                {
                    smtp.EnableSsl = cfg.UsarSsl;
                    smtp.Credentials = new NetworkCredential(cfg.SmtpUsuario, cfg.SmtpPassword);
                    smtp.Send(mensaje);
                }
            }
        }

        private static bool EsEmailValido(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var addr = new MailAddress(email.Trim());
                return addr.Address == email.Trim();
            }
            catch
            {
                return false;
            }
        }
    }
}
