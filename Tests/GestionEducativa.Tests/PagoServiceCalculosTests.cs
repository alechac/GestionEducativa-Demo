using System.Collections.Generic;
using GestionEducativa.Models;
using GestionEducativa.Services;
using Xunit;

namespace GestionEducativa.Tests
{
    /// <summary>
    /// SimularPagoMultiCuota reparte un monto recibido entre una lista de cuotas
    /// EN MEMORIA — no toca la base de datos (recién ConfirmarPagoConCae persiste).
    /// Es el corazón de la pantalla de cobro: si esto falla, se cobra mal.
    /// </summary>
    public class PagoServiceCalculosTests
    {
        private readonly PagoService _pagoService = new PagoService();

        [Fact]
        public void MontoAlcanzaParaUnaSolaCuota_LaDejaPagadaSinVuelto()
        {
            var cuotas = new List<Cuota>
            {
                new Cuota { Id = 1, Saldo = 15000m, MontoOriginal = 15000m, Estado = "Pendiente" }
            };

            var pago = _pagoService.SimularPagoMultiCuota(15000m, cuotas,
                "Juan Pérez", "30111222", "DNI", "Efectivo", null);

            Assert.Equal(15000m, pago.TotalAplicado);
            Assert.Equal(0m, pago.Vuelto);
            Assert.Single(pago.Detalles);
            Assert.Equal("Pagado", pago.Detalles[0].EstadoCuotaNuevo);
            Assert.Equal(0m, pago.Detalles[0].SaldoRestante);
        }

        [Fact]
        public void MontoMenorAlaCuota_QuedaPagoParcialConSaldoPendiente()
        {
            var cuotas = new List<Cuota>
            {
                new Cuota { Id = 1, Saldo = 15000m, MontoOriginal = 15000m, Estado = "Pendiente" }
            };

            var pago = _pagoService.SimularPagoMultiCuota(10000m, cuotas,
                "Juan Pérez", "30111222", "DNI", "Efectivo", null);

            Assert.Equal(10000m, pago.TotalAplicado);
            Assert.Equal(0m, pago.Vuelto);
            Assert.Equal("Pago Parcial", pago.Detalles[0].EstadoCuotaNuevo);
            Assert.Equal(5000m, pago.Detalles[0].SaldoRestante);
        }

        [Fact]
        public void MontoCubreVariasCuotas_LasVaCompletandoEnOrdenYDejaVuelto()
        {
            // Dos cuotas de 10000 cada una, se paga 25000: la primera y la segunda
            // quedan pagadas, y sobran 5000 de vuelto (no hay una tercera cuota que los absorba).
            var cuotas = new List<Cuota>
            {
                new Cuota { Id = 1, Saldo = 10000m, MontoOriginal = 10000m, Estado = "Pendiente" },
                new Cuota { Id = 2, Saldo = 10000m, MontoOriginal = 10000m, Estado = "Pendiente" }
            };

            var pago = _pagoService.SimularPagoMultiCuota(25000m, cuotas,
                "Juan Pérez", "30111222", "DNI", "Efectivo", null);

            Assert.Equal(20000m, pago.TotalAplicado);
            Assert.Equal(5000m, pago.Vuelto);
            Assert.Equal(2, pago.Detalles.Count);
            Assert.All(pago.Detalles, d => Assert.Equal("Pagado", d.EstadoCuotaNuevo));
        }

        [Fact]
        public void SinCuotasSeleccionadas_RechazaElPago()
        {
            Assert.Throws<System.ArgumentException>(() =>
                _pagoService.SimularPagoMultiCuota(1000m, new List<Cuota>(),
                    "Juan Pérez", "30111222", "DNI", "Efectivo", null));
        }
    }
}
