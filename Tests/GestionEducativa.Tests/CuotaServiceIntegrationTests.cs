using System;
using System.Collections.Generic;
using GestionEducativa.Models;
using GestionEducativa.Services;
using MySql.Data.MySqlClient;
using Xunit;

namespace GestionEducativa.Tests
{
    /// <summary>
    /// A diferencia de los tests "Calculos", estos SÍ tocan una base de datos real —
    /// la que está configurada en App.config de este proyecto (gestioneducativa_test,
    /// NUNCA la base del cliente). Por eso son más lentos y necesitan que esa base
    /// exista y tenga las tablas creadas (correr las migraciones de Database/ contra
    /// ella una vez, igual que se hizo con la base real).
    ///
    /// Cada test crea su propio alumno de prueba con un DNI único (para poder correr
    /// varias veces sin chocar) y lo borra al final, pase o no el test.
    /// </summary>
    public class CuotaServiceIntegrationTests : IDisposable
    {
        private readonly CuotaService _cuotaService = new CuotaService();
        private readonly AlumnoService _alumnoService = new AlumnoService();
        private readonly List<int> _alumnosCreados = new List<int>();

        private Alumno CrearAlumnoDePrueba(string nivel)
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

            // Insertar() no devuelve el Id generado — lo recuperamos filtrando por el
            // nombre único que acabamos de generar.
            var todos = _alumnoService.ObtenerTodos(seccion: (int)Seccion.Escuela, busqueda: alumno.Nombre);
            var creado = todos.Find(a => a.Nombre == alumno.Nombre);
            Assert.NotNull(creado); // si esto falla, revisar que ObtenerTodos/Insertar no hayan cambiado de firma
            _alumnosCreados.Add(creado.Id);
            return creado;
        }

        private int ContarCuotasDe(int alumnoId, int mes, int anio, string tipoCuota)
        {
            using (var cn = new MySqlConnection(new GestionEducativa.Database.ConexionBD().ConnectionString))
            {
                cn.Open();
                using (var cmd = new MySqlCommand(
                    "SELECT COUNT(*) FROM cuotas WHERE alumnoId=@a AND mes=@m AND anio=@y AND tipoCuota=@t", cn))
                {
                    cmd.Parameters.AddWithValue("@a", alumnoId);
                    cmd.Parameters.AddWithValue("@m", mes);
                    cmd.Parameters.AddWithValue("@y", anio);
                    cmd.Parameters.AddWithValue("@t", tipoCuota);
                    return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
        }

        [Fact]
        public void GenerarCuotasMasivas_AlumnoActivoEnElNivel_LeGeneraUnaCuotaPendiente()
        {
            var alumno = CrearAlumnoDePrueba("1 grado");
            string tipoCuota = "TEST_" + Guid.NewGuid().ToString("N").Substring(0, 6);

            var (generadas, salteadas) = _cuotaService.GenerarCuotasMasivas(
                new List<string> { "1 grado" }, "Marzo", 2026, 15000m, tipoCuota);

            Assert.Equal(1, generadas);
            Assert.Equal(0, salteadas);
            Assert.Equal(1, ContarCuotasDe(alumno.Id, 3, 2026, tipoCuota));
        }

        [Fact]
        public void GenerarCuotasMasivas_CorridaDosVecesElMismoMes_LaSegundaLaSaltea()
        {
            var alumno = CrearAlumnoDePrueba("2 grado");
            string tipoCuota = "TEST_" + Guid.NewGuid().ToString("N").Substring(0, 6);
            var niveles = new List<string> { "2 grado" };

            _cuotaService.GenerarCuotasMasivas(niveles, "Abril", 2026, 12000m, tipoCuota);
            var (generadas, salteadas) = _cuotaService.GenerarCuotasMasivas(niveles, "Abril", 2026, 12000m, tipoCuota);

            Assert.Equal(0, generadas);
            Assert.Equal(1, salteadas);
            // Sigue habiendo UNA sola cuota, no dos — no se duplicó.
            Assert.Equal(1, ContarCuotasDe(alumno.Id, 4, 2026, tipoCuota));
        }

        [Fact]
        public void GenerarCuotasMasivas_AlumnoDeOtroNivel_NoLoToca()
        {
            var alumno = CrearAlumnoDePrueba("3 grado");
            string tipoCuota = "TEST_" + Guid.NewGuid().ToString("N").Substring(0, 6);

            // Generamos para "4 grado" — nuestro alumno de prueba está en "3 grado"
            var (generadas, _) = _cuotaService.GenerarCuotasMasivas(
                new List<string> { "4 grado" }, "Mayo", 2026, 12000m, tipoCuota);

            Assert.Equal(0, generadas);
            Assert.Equal(0, ContarCuotasDe(alumno.Id, 5, 2026, tipoCuota));
        }

        public void Dispose()
        {
            // Limpieza: borra las cuotas y el alumno de prueba, pase o no el test.
            using (var cn = new MySqlConnection(new GestionEducativa.Database.ConexionBD().ConnectionString))
            {
                cn.Open();
                foreach (var id in _alumnosCreados)
                {
                    using (var cmd = new MySqlCommand("DELETE FROM cuotas WHERE alumnoId=@id", cn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new MySqlCommand("DELETE FROM alumnos WHERE id=@id", cn))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
