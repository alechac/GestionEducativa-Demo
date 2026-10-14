using System;

namespace GestionEducativa.Models
{
    /// <summary>
    /// Representa un egreso/gasto de la institución (sueldos, servicios, alquiler, etc.),
    /// con el respaldo del comprobante cargado a mano. Sirve para poder hacer, más adelante,
    /// un cierre de caja que contemple tanto lo que entró (Pagos) como lo que salió (Gastos).
    /// </summary>
    public class Gasto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; } = DateTime.Now.Date;
        public string TipoGasto { get; set; }
        public string Detalle { get; set; }
        public decimal Monto { get; set; }
        public string FormaPago { get; set; } = "Efectivo";
        public int Seccion { get; set; } = 1;

        // Datos del comprobante, cargados a mano
        public string ComprobanteTipo { get; set; }
        public string ComprobanteNumero { get; set; }
        public string ComprobanteProveedor { get; set; }
        public string ComprobanteCuitProveedor { get; set; }
        public DateTime? ComprobanteFecha { get; set; }

        public string Observaciones { get; set; }
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
        public bool Activo { get; set; } = true;
    }
}
