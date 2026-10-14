using System;

namespace GestionEducativa.Models
{
    /// <summary>
    /// Configuración del servidor SMTP usado para enviar los comprobantes
    /// de pago por mail al padre/tutor que figura como pagador.
    /// </summary>
    public class EmailConfig
    {
        /// <summary>Habilita/deshabilita el envío automático de comprobantes por mail.</summary>
        public bool Habilitado { get; set; }

        public string SmtpHost { get; set; }
        public int SmtpPuerto { get; set; } = 587;
        public string SmtpUsuario { get; set; }
        public string SmtpPassword { get; set; }
        public bool UsarSsl { get; set; } = true;

        /// <summary>Dirección que figura como remitente (normalmente igual a SmtpUsuario).</summary>
        public string RemitenteEmail { get; set; }
        public string RemitenteNombre { get; set; }

        /// <summary>Opcional: copia oculta a administración para tener registro de todo lo enviado.</summary>
        public string CopiaOculta { get; set; }
    }
}
