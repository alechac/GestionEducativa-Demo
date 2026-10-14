using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionEducativa.Models
{
    public class ArcaConfig
    {
        // ── Datos del emisor ──────────────────────────────────────────────────────
        public string CUIT { get; set; }
        public string RazonSocial { get; set; }
        public string DomicilioComercial { get; set; }   // "Congreso 273 - San Miguel De Tucumán"
        public string IngresosBrutos { get; set; }       // Nro de IIBB (suele ser el CUIT)
        public string FechaInicioActividades { get; set; } // "21/05/1992"
        public int PuntoVenta { get; set; }

        // ── Certificado ──────────────────────────────────────────────────────────
        public string RutaPfx { get; set; }   // ej: "C:\arca\colegio.pfx"
        public string PasswordPfx { get; set; }   // contraseña del .pfx (puede ser "")

        // ── Ambiente ─────────────────────────────────────────────────────────────
        /// <summary>
        /// false = Homologación (testing), true = Producción.
        /// Cambiar a true solo cuando el certificado real esté aprobado.
        /// </summary>
        public bool Produccion { get; set; }

        // ── URLs según ambiente ───────────────────────────────────────────────────
        public string UrlWsaa => Produccion
            ? "https://wsaa.afip.gov.ar/ws/services/LoginCms"
            : "https://wsaahomo.afip.gov.ar/ws/services/LoginCms";

        public string UrlWsfe => Produccion
            ? "https://servicios1.afip.gov.ar/wsfev1/service.asmx"
            : "https://wswhomo.afip.gov.ar/wsfev1/service.asmx";
    }
}
