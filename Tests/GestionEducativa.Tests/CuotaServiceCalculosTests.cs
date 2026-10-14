using System;
using GestionEducativa.Models;
using GestionEducativa.Services;
using Xunit;

namespace GestionEducativa.Tests
{
    /// <summary>
    /// CalcularSaldoConRecargo es un método static que no toca la base de datos:
    /// recibe una Cuota y una fecha, devuelve un número. Por eso se puede testear
    /// directo, sin mocks ni base de prueba — corre en milisegundos.
    /// </summary>
    public class CuotaServiceCalculosTests
    {
        [Fact]
        public void CuotaSinVencer_NoAplicaRecargo()
        {
            var cuota = new Cuota
            {
                Saldo = 10000m,
                FechaVencimiento = new DateTime(2026, 1, 10)
            };

            decimal resultado = CuotaService.CalcularSaldoConRecargo(cuota, new DateTime(2026, 1, 5));

            Assert.Equal(10000m, resultado);
        }

        [Fact]
        public void CuotaVencidaMismoMes_Aplica4PorCiento()
        {
            var cuota = new Cuota
            {
                Saldo = 10000m,
                FechaVencimiento = new DateTime(2026, 1, 10)
            };

            // 5 días después del vencimiento, dentro del mismo mes calendario
            decimal resultado = CuotaService.CalcularSaldoConRecargo(cuota, new DateTime(2026, 1, 15));

            Assert.Equal(10400m, resultado); // 10000 + 4%
        }

        [Fact]
        public void CuotaVencidaUnMesCalendarioDespues_Aplica8PorCiento()
        {
            var cuota = new Cuota
            {
                Saldo = 10000m,
                FechaVencimiento = new DateTime(2026, 1, 10)
            };

            // Un mes calendario completo de atraso (enero -> febrero)
            decimal resultado = CuotaService.CalcularSaldoConRecargo(cuota, new DateTime(2026, 2, 15));

            Assert.Equal(10800m, resultado); // 10000 + 8% (2 meses contados)
        }

        [Fact]
        public void CuotaYaPagada_NuncaAplicaRecargo()
        {
            var cuota = new Cuota
            {
                Saldo = 0m,
                FechaVencimiento = new DateTime(2026, 1, 10)
            };

            decimal resultado = CuotaService.CalcularSaldoConRecargo(cuota, new DateTime(2026, 6, 1));

            Assert.Equal(0m, resultado);
        }

        [Fact]
        public void CuotaSinFechaDeVencimiento_NoAplicaRecargo()
        {
            var cuota = new Cuota
            {
                Saldo = 5000m,
                FechaVencimiento = null
            };

            decimal resultado = CuotaService.CalcularSaldoConRecargo(cuota, new DateTime(2026, 6, 1));

            Assert.Equal(5000m, resultado);
        }
    }
}
