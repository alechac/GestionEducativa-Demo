using System;
using System.Collections.Generic;
using System.Linq;
using GestionEducativa.Database;
using GestionEducativa.Models;
using GestionEducativa.Services;
using MySql.Data.MySqlClient;
using Xunit;

namespace GestionEducativa.Tests
{
    /// <summary>
    /// Cubre el flujo de cobro que realmente escribe en la base (a diferencia de
    /// PagoServiceCalculosTests, que solo simula en memoria) y los dos ajustes
    /// manuales que se hacen desde el diálogo de PagosForm. Usa una base de
    /// PRUEBA (la de App.config de este proyecto) — nunca la del cliente.
    ///
    /// Para preparar cada cuota de prueba se reutiliza CuotaService.GenerarCuotasMasivas
    /// (ya cubierto en CuotaServiceIntegrationTests) en vez de insertar filas a mano,
    /// porque `montoFinal` y `saldo` son columnas calculadas por la base — insertarlas
    /// a mano podría no coincidir con la fórmula real.
    /// </summary>
    public class PagoServiceIntegrationTests : IDisposable
    {
        private readonly AlumnoService _alumnoService = new AlumnoService();
        private readonly CuotaService _cuotaService = new CuotaService();
        private readonly PagoService _pagoService = new PagoService();
        private readonly List<int> _alumnosCreados = new List<int>();

        private Alumno CrearAlumnoConCuotaPendiente(string nivel, string mes, int anio,
            decimal monto, out Cuota cuota)
        {
            var alumno = new Alumno
            {
                Nombre = "TEST_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                Dni = new Random().Next(90000000, 99999999).ToString(),
                Nivel = nivel,
                Division = "A",
                PorcentajeDescuento = 0,
                Seccion = (int)Seccion.Escuela
            };
            _alumnoService.Insertar(alumno);

            var creado = _alumnoService.ObtenerTodos(seccion: (int)Seccion.Escuela, busqueda: alumno.Nombre)
                .Find(a => a.Nombre == alumno.Nombre);
            Assert.NotNull(creado);
            _alumnosCreados.Add(creado.Id);

            string tipoCuota = "TEST_" + Guid.NewGuid().ToString("N").Substring(0, 6);
            var (generadas, _) = _cuotaService.GenerarCuotasMasivas(
                new List<string> { nivel }, mes, anio, monto, tipoCuota);
            Assert.Equal(1, generadas); // si esto falla, revisar que el nivel/alumno hayan quedado bien creados

            cuota = _cuotaService.ObtenerCuotasPendientesAlumno(creado.Id)
                .Find(c => c.TipoCuota == tipoCuota);
            Assert.NotNull(cuota);
            return creado;
        }

        private Cuota RefrescarCuota(int cuotaId)
        {
            using (var cn = new MySqlConnection(new ConexionBD().ConnectionString))
            {
                cn.Open();
                using (var cmd = new MySqlCommand(
                    "SELECT saldo, montoPagado, estado, recargo FROM cuotas WHERE id=@id", cn))
                {
                    cmd.Parameters.AddWithValue("@id", cuotaId);
                    using (var r = cmd.ExecuteReader())
                    {
                        r.Read();
                        return new Cuota
                        {
                            Id = cuotaId,
                            Saldo = Convert.ToDecimal(r["saldo"]),
                            MontoPagado = Convert.ToDecimal(r["montoPagado"]),
                            Estado = r["estado"].ToString(),
                            Recargo = Convert.ToDecimal(r["recargo"])
                        };
                    }
                }
            }
        }

        [Fact]
        public void ConfirmarPagoConCae_PagoTotal_DejaLaCuotaPagadaYSaldoEnCero()
        {
            CrearAlumnoConCuotaPendiente("1 grado", "Junio", 2026, 15000m, out Cuota cuota);

            var simulado = _pagoService.SimularPagoMultiCuota(
                cuota.Saldo, new List<Cuota> { cuota },
                "Juan Pérez", "30111222", "DNI", "Efectivo", "pago de prueba");

            var confirmado = _pagoService.ConfirmarPagoConCae(simulado);

            Assert.True(confirmado.Id > 0); // el pago quedó insertado y tiene Id real

            var cuotaActualizada = RefrescarCuota(cuota.Id);
            Assert.Equal(0m, cuotaActualizada.Saldo);
            Assert.Equal("Pagado", cuotaActualizada.Estado);
            Assert.Equal(cuota.Saldo, cuotaActualizada.MontoPagado);
        }

        [Fact]
        public void ConfirmarPagoConCae_PagoParcial_DejaSaldoPendienteConLoQueFalta()
        {
            CrearAlumnoConCuotaPendiente("2 grado", "Junio", 2026, 15000m, out Cuota cuota);
            decimal montoAPagar = cuota.Saldo - 5000m; // paga todo menos 5000

            var simulado = _pagoService.SimularPagoMultiCuota(
                montoAPagar, new List<Cuota> { cuota },
                "Juan Pérez", "30111222", "DNI", "Efectivo", null);
            _pagoService.ConfirmarPagoConCae(simulado);

            var cuotaActualizada = RefrescarCuota(cuota.Id);
            Assert.Equal(5000m, cuotaActualizada.Saldo);
            Assert.Equal("Pago Parcial", cuotaActualizada.Estado);
        }

        [Fact]
        public void AjustarSaldoCuota_BajaElSaldoAMano_ActualizaMontoPagadoYEstado()
        {
            CrearAlumnoConCuotaPendiente("3 grado", "Julio", 2026, 20000m, out Cuota cuota);

            // El colegio ajusta a mano: "en realidad ya debe solo 8000" (ej. venía de otro sistema)
            _cuotaService.AjustarSaldoCuota(cuota.Id, 8000m, "Migración de saldo inicial");

            var actualizada = RefrescarCuota(cuota.Id);
            Assert.Equal(12000m, actualizada.MontoPagado); // 20000 - 8000
            Assert.Equal("Pago Parcial", actualizada.Estado);
        }

        [Fact]
        public void AjustarSaldoCuota_LlevarloACero_LaMarcaComoPagada()
        {
            CrearAlumnoConCuotaPendiente("4 grado", "Julio", 2026, 20000m, out Cuota cuota);

            _cuotaService.AjustarSaldoCuota(cuota.Id, 0m);

            var actualizada = RefrescarCuota(cuota.Id);
            Assert.Equal("Pagado", actualizada.Estado);
        }

        [Fact]
        public void AjustarSaldoCuota_SaldoNegativo_Rechaza()
        {
            CrearAlumnoConCuotaPendiente("5 grado", "Julio", 2026, 20000m, out Cuota cuota);

            Assert.Throws<ArgumentException>(() => _cuotaService.AjustarSaldoCuota(cuota.Id, -100m));
        }

        [Fact]
        public void AjustarSaldoCuota_MayorAlMontoDeLaCuota_Rechaza()
        {
            CrearAlumnoConCuotaPendiente("6 grado", "Julio", 2026, 20000m, out Cuota cuota);

            Assert.Throws<ArgumentException>(() => _cuotaService.AjustarSaldoCuota(cuota.Id, 999999m));
        }

        [Fact]
        public void AjustarRecargoCuota_AgregaRecargoManual_SumaAlSaldoYVuelvePendiente()
        {
            CrearAlumnoConCuotaPendiente("1 ano", "Julio", 2026, 20000m, out Cuota cuota);

            _cuotaService.AjustarRecargoCuota(cuota.Id, 2000m);

            var actualizada = RefrescarCuota(cuota.Id);
            Assert.Equal(2000m, actualizada.Recargo);
            Assert.Equal("Pendiente", actualizada.Estado); // sigue sin pagos, ahora debe 22000
        }

        public void Dispose()
        {
            using (var cn = new MySqlConnection(new ConexionBD().ConnectionString))
            {
                cn.Open();
                foreach (var id in _alumnosCreados)
                {
                    var pagoIds = new List<int>();
                    using (var cmd = new MySqlCommand(@"
                        SELECT DISTINCT pd.pagoId FROM pagos_detalle pd
                        JOIN cuotas c ON pd.cuotaId = c.id
                        WHERE c.alumnoId=@id", cn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (var r = cmd.ExecuteReader())
                            while (r.Read()) pagoIds.Add(Convert.ToInt32(r["pagoId"]));
                    }

                    using (var cmd = new MySqlCommand(
                        "DELETE pd FROM pagos_detalle pd JOIN cuotas c ON pd.cuotaId = c.id WHERE c.alumnoId=@id", cn))
                    { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }

                    foreach (var pagoId in pagoIds)
                    {
                        using (var cmd = new MySqlCommand("DELETE FROM pagos WHERE id=@pagoId", cn))
                        { cmd.Parameters.AddWithValue("@pagoId", pagoId); cmd.ExecuteNonQuery(); }
                    }

                    using (var cmd = new MySqlCommand("DELETE FROM cuotas WHERE alumnoId=@id", cn))
                    { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }

                    using (var cmd = new MySqlCommand("DELETE FROM alumnos WHERE id=@id", cn))
                    { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
                }
            }
        }
    }
}
