using System;
using System.Collections.Generic;

namespace GestionEducativa.Models
{
    /// <summary>
    /// Comprobante de pago generado para ARCA/AFIP.
    /// </summary>
    public class Comprobante
    {
        public int Id { get; set; }
        public string NumeroComprobante { get; set; }  // Correlativo: e.g. 0001-00000001
        public DateTime Fecha { get; set; }
        public Seccion Seccion { get; set; }

        // Datos del alumno
        public int AlumnoId { get; set; }
        public string AlumnoNombre { get; set; }
        public string AlumnoDNI { get; set; }
        public string AlumnoNivel { get; set; }

        // Datos del pagador
        public string NombrePagador { get; set; }
        public string DocumentoPagador { get; set; }
        public string TipoDocumentoPagador { get; set; }

        // Datos del cobro
        public string ConceptoCobro { get; set; }     // "Cuota Marzo 2025", "Matrícula 2025"
        public decimal MontoTotal { get; set; }
        public decimal MontoRecibido { get; set; }
        public decimal SaldoDeuda { get; set; }

        // Estado
        public bool Anulado { get; set; }
        public string MotivoAnulacion { get; set; }
    }
}
