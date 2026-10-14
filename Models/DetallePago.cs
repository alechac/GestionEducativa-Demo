using System;

namespace GestionEducativa.Models
{
    /// <summary>
    /// Detalle de cada cuota incluida en un pago multi-cuota / multi-alumno.
    /// </summary>
    public class DetallePago
    {
        public int Id { get; set; }
        public int PagoId { get; set; }
        public int CuotaId { get; set; }
        public int AlumnoId { get; set; }
        public string AlumnoNombre { get; set; }
        public string PeriodoCuota { get; set; }
        public string TipoCuota { get; set; }
        // Nombre real del concepto tal como lo cargó el usuario (ej: "Reserva de Matrícula").
        // Si está presente, el comprobante debe usar este texto en vez del genérico derivado de TipoCuota.
        public string NombreConcepto { get; set; }
        public decimal SaldoAnterior { get; set; }
        public decimal RecargoAplicado { get; set; } // Recargo por mora incluido en este pago
        public decimal MontoAplicado { get; set; }
        public decimal SaldoRestante { get; set; }

        public decimal CuotaBaseAplicada => MontoAplicado - RecargoAplicado;
        public bool TieneRecargo => RecargoAplicado > 0;

        // Datos para el comprobante tipo factura ARCA
        public decimal PrecioUnitario { get; set; }   // montoFinal antes del pago parcial
        public decimal PorcentajeBonif { get; set; }  // % descuento del alumno
        public decimal ImporteBonif { get; set; }     // monto del descuento
        public decimal Subtotal { get; set; }         // precio - bonif

        /// <summary>
        /// Estado que debe quedar la cuota ("Pagado", "Pago Parcial", etc.) si este detalle
        /// se termina confirmando. Se calcula al simular el pago (antes de pedir el CAE) y
        /// se usa recién al confirmarlo en la base de datos. No se persiste en pagos_detalle.
        /// </summary>
        public string EstadoCuotaNuevo { get; set; }
    }
}
