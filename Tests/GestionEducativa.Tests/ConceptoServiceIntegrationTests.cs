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
    /// Antes, a un alumno que ya está cursando SIEMPRE se le ofrecía únicamente la
    /// matrícula del nivel siguiente (para el año que viene) — nunca la de su nivel
    /// actual, aunque todavía no la hubiera pagado. Ahora ObtenerConceptos debe
    /// devolver las DOS matrículas para que el colegio pueda generar la que corresponda.
    ///
    /// El catálogo real tiene una fila de matrícula POR AÑO (una para el año actual, otra
    /// para el siguiente, cada una con su propio `anio` ya correcto) — por eso acá "este
    /// año" se carga con `anio` y "año que viene" con `anio + 1`: son dos filas distintas
    /// del catálogo, no la misma fila reinterpretada.
    /// </summary>
    public class ConceptoServiceIntegrationTests : IDisposable
    {
        private readonly ConceptoService _conceptoService = new ConceptoService();
        private readonly CuotaService _cuotaServiceParaGenerar = new CuotaService();
        private readonly AlumnoService _alumnoService = new AlumnoService();
        private readonly List<int> _alumnosCreados = new List<int>();
        private readonly List<int> _conceptosCreados = new List<int>();

        private Alumno CrearAlumnoCursante(string nivel)
        {
            var alumno = new Alumno
            {
                Nombre = "TEST_" + Guid.NewGuid().ToString("N").Substring(0, 8),
                Dni = new Random().Next(90000000, 99999999).ToString(),
                Nivel = nivel,
                Division = "A",
                PorcentajeDescuento = 0,
                Seccion = (int)Seccion.Escuela,
                AnioIngreso = null // ya está cursando, no es preinscripto
            };
            _alumnoService.Insertar(alumno);
            var creado = _alumnoService.ObtenerTodos(seccion: (int)Seccion.Escuela, busqueda: alumno.Nombre)
                .Find(a => a.Nombre == alumno.Nombre);
            Assert.NotNull(creado);
            _alumnosCreados.Add(creado.Id);
            return creado;
        }

        private void CrearMatricula(string nivel, int anio, decimal monto)
        {
            var c = _conceptoService.CrearConcepto("Matrícula " + nivel, "Matricula", nivel, monto, anio);
            _conceptosCreados.Add(c.Id);
        }

        [Fact]
        public void AlumnoCursante_ConAmbasMatriculasCargadas_LeOfreceLasDos()
        {
            int anio = 2031; // año "de catálogo" propio de este test, no choca con datos reales
            var alumno = CrearAlumnoCursante("3 grado");
            CrearMatricula("3 grado", anio, 20000m);       // este año (todavía no la pagó)
            CrearMatricula("4 grado", anio + 1, 22000m);   // año que viene — fila propia, catálogo del año siguiente

            var conceptos = _conceptoService.ObtenerConceptos(anio, alumno.Nivel, esPreinscripto: false);
            var matriculas = conceptos.Where(c => c.Categoria == "Matricula").ToList();

            Assert.Equal(2, matriculas.Count);
            Assert.Contains(matriculas, c => c.Nivel == "3 grado" && c.Anio == anio);
            Assert.Contains(matriculas, c => c.Nivel == "4 grado" && c.Anio == anio + 1);
        }

        [Fact]
        public void AlumnoCursante_SoloConMatriculaDelAnioSiguienteCargada_LeOfreceSoloEsa()
        {
            int anio = 2032;
            var alumno = CrearAlumnoCursante("5 grado");
            CrearMatricula("6 grado", anio + 1, 22000m); // sólo cargaron la del año que viene

            var conceptos = _conceptoService.ObtenerConceptos(anio, alumno.Nivel, esPreinscripto: false);
            var matriculas = conceptos.Where(c => c.Categoria == "Matricula").ToList();

            Assert.Single(matriculas);
            Assert.Equal("6 grado", matriculas[0].Nivel);
            Assert.Equal(anio + 1, matriculas[0].Anio);
        }

        [Fact]
        public void AlumnoPreinscripto_SoloLeOfreceLaMatriculaDelNivelAlQueEntra()
        {
            int anio = 2033;
            // Preinscripto: Nivel ya representa el nivel real al que va a entrar, y no hay
            // ambigüedad de "este año / año que viene" — se busca directo en `anio`.
            CrearMatricula("Jardin de 4 anos", anio, 18000m);
            CrearMatricula("Jardin de 5 anos", anio + 1, 19000m); // no debería ofrecerse ésta

            var conceptos = _conceptoService.ObtenerConceptos(anio, "Jardin de 4 anos", esPreinscripto: true);
            var matriculas = conceptos.Where(c => c.Categoria == "Matricula").ToList();

            Assert.Single(matriculas);
            Assert.Equal("Jardin de 4 anos", matriculas[0].Nivel);
        }

        [Fact]
        public void MatriculaDelNivelActualGenerada_AlReleerlaImprimeElAnioCorrectoNoElSiguiente()
        {
            int anio = 2034;
            var alumno = CrearAlumnoCursante("2 ano");
            CrearMatricula("2 ano", anio, 20000m);       // este año — la que a veces falta pagar
            CrearMatricula("3 ano", anio + 1, 22000m);   // año que viene

            var conceptos = _conceptoService.ObtenerConceptos(anio, alumno.Nivel, esPreinscripto: false);
            var conceptoEsteAnio = conceptos.First(c => c.Categoria == "Matricula" && c.Nivel == "2 ano");

            // Igual que hace ConceptosPanel: el año a grabar es directamente concepto.Anio,
            // sin calcular ni comparar nada — el catálogo ya lo tiene correcto.
            _cuotaServiceParaGenerar.GenerarCuotaConcepto(alumno.Id, conceptoEsteAnio.Id, conceptoEsteAnio.Anio, 0, conceptoEsteAnio.Monto);

            var cuotaGenerada = _cuotaServiceParaGenerar.ObtenerCuotasPendientesAlumno(alumno.Id)
                .First(c => c.ConceptoId == conceptoEsteAnio.Id);

            Assert.Equal(anio, cuotaGenerada.Anio);
            Assert.Contains($"2 ANO {anio}", cuotaGenerada.PeriodoDescripcion);
            Assert.DoesNotContain($"2 ANO {anio + 1}", cuotaGenerada.PeriodoDescripcion);
        }

        [Fact]
        public void MatriculaDelAnioQueViene_SeGrabaConElAnioSiguienteYaCorrecto()
        {
            int anio = 2035;
            var alumno = CrearAlumnoCursante("4 ano");
            CrearMatricula("4 ano", anio, 20000m);       // este año
            CrearMatricula("5 ano", anio + 1, 22000m);   // año que viene — fila propia del catálogo 2036

            var conceptos = _conceptoService.ObtenerConceptos(anio, alumno.Nivel, esPreinscripto: false);
            var conceptoProximoAnio = conceptos.First(c => c.Categoria == "Matricula" && c.Nivel == "5 ano");

            _cuotaServiceParaGenerar.GenerarCuotaConcepto(alumno.Id, conceptoProximoAnio.Id, conceptoProximoAnio.Anio, 0, conceptoProximoAnio.Monto);

            var cuotaGenerada = _cuotaServiceParaGenerar.ObtenerCuotasPendientesAlumno(alumno.Id)
                .First(c => c.ConceptoId == conceptoProximoAnio.Id);

            Assert.Equal(anio + 1, cuotaGenerada.Anio); // se grabó ya con el año correcto
            Assert.Contains($"5 ANO {anio + 1}", cuotaGenerada.PeriodoDescripcion);
            Assert.DoesNotContain($"{anio + 2}", cuotaGenerada.PeriodoDescripcion); // no se dobla el +1
        }

        public void Dispose()
        {
            using (var cn = new MySqlConnection(new ConexionBD().ConnectionString))
            {
                cn.Open();
                foreach (var id in _conceptosCreados)
                {
                    using (var cmd = new MySqlCommand("DELETE FROM conceptos WHERE id=@id", cn))
                    { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
                }
                foreach (var id in _alumnosCreados)
                {
                    using (var cmd = new MySqlCommand("DELETE FROM cuotas WHERE alumnoId=@id", cn))
                    { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
                    using (var cmd = new MySqlCommand("DELETE FROM alumnos WHERE id=@id", cn))
                    { cmd.Parameters.AddWithValue("@id", id); cmd.ExecuteNonQuery(); }
                }
            }
        }
    }
}
