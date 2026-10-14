using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionEducativa.Models
{
    public class ResultadoCAE
    {
        public bool Aprobado { get; set; }
        public string CAE { get; set; }   // 14 dígitos
        public DateTime CAEVencimiento { get; set; }   // Fecha límite para imprimir
        public string NroComprobante { get; set; }   // ej: "00001-00000042"
        public long NroComprobanteRaw { get; set; }   // solo el número, ej: 42

        // En caso de error o rechazo
        public string MensajeError { get; set; }
        public string Observaciones { get; set; }   // Advertencias no excluyentes
    }
}
