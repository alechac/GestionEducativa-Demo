using System;
using System.Collections.Generic;

namespace GestionEducativa.Models
{
    /// <summary>
    /// Registra una transacción de cobro que puede cubrir múltiples cuotas
    /// de uno o varios alumnos (hermanos).
    /// </summary>
    public class Pago
    {
        public int Id { get; set; }

        // Pagador (padre/tutor)
        public string NombrePagador { get; set; }
        public string DocumentoPagador { get; set; }   // DNI o CUIT
        public string TipoPagador { get; set; }         // "DNI" | "CUIT"

        // Montos
        public decimal MontoRecibido { get; set; }      // Dinero entregado por el padre
        public decimal TotalAplicado { get; set; }      // Suma de lo aplicado a cuotas
        public decimal Vuelto { get; set; }             // Si pagó de más

        public DateTime FechaPago { get; set; }
        public string NumeroPago { get; set; }          // Número correlativo del comprobante
        public string Observaciones { get; set; }

        // Para el comprobante
        public int? ComprobanteId { get; set; }

        // Detalle de cuotas cubiertas por este pago
        public List<DetallePago> Detalles { get; set; } = new List<DetallePago>();

        // Datos ARCA — se completan después de emitir manualmente
        public string CAE { get; set; }
        public DateTime? CAEVencimiento { get; set; }
        public string NroComprobanteArca { get; set; }   // ej: "00001-00000042"
        public string EstadoArca { get; set; } = "Pendiente";

        // Datos del receptor para el comprobante
        public string DomicilioPagador { get; set; }
        public int TipoComprobante { get; set; } = 11;   // 11=Factura C, 13=Nota de Crédito C, 6=Factura B

        // ── Notas de Crédito ──────────────────────────────────────────────
        // Solo se usan cuando TipoComprobante = 13 (Nota de Crédito C).
        public int? PagoAsociadoId { get; set; }          // Id de la factura que esta NC acredita
        public string NroComprobanteAsociado { get; set; } // Nro. de esa factura, ej: "00001-00000042" (solo lectura, para mostrar/imprimir)
        public int CantidadNotasCredito { get; set; }      // Transitorio: cuántas NC ya tiene esta factura (para no dejar acreditarla dos veces por el total)

        // Período facturado (para cuotas de servicio)
        public DateTime PeriodoDesde { get; set; }
        public DateTime PeriodoHasta { get; set; }

        public string FormaPago { get; set; } = "Efectivo"; // "Efectivo" | "Transferencia" | "Cheque"



        // -- Propiedades de compatibilidad (para código existente) --
        public int AlumnoId { get; set; }
        public string AlumnoNombre { get; set; }
        public int CuotaId { get; set; }
        public decimal MontoCuota { get; set; }
        public decimal SaldoDeuda { get; set; }
    }
}
