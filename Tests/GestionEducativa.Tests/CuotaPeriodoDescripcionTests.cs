using GestionEducativa.Models;
using Xunit;

namespace GestionEducativa.Tests
{
    /// <summary>
    /// Cuota.PeriodoDescripcion es lo que termina impreso en el comprobante y en el
    /// resumen de confirmación de cobro (Utils/PdfGenerator.cs y PagosPresenter.cs).
    /// Es una propiedad pura — no toca la base — así que se puede testear armando el
    /// objeto Cuota a mano, sin mocks.
    ///
    /// Regresión concreta que motivó estos tests: al agregar la posibilidad de generar
    /// la matrícula del NIVEL ACTUAL (este año) además de la del nivel siguiente, el
    /// texto impreso seguía sumando "+1" al año SIEMPRE, sin importar cuál de las dos
    /// se había generado — un comprobante de "este año" salía impreso como si fuera
    /// del año que viene.
    /// </summary>
    public class CuotaPeriodoDescripcionTests
    {
        [Fact]
        public void MatriculaDelNivelActual_NoSumaUnAnio()
        {
            var cuota = new Cuota
            {
                TipoCuota = "Matricula",
                Anio = 2026,
                NombreConcepto = "Matrícula 3 grado",
                NivelSiguiente = "3 grado",     // el nivel que corresponde a ESTA matrícula
                MatriculaEsDeEsteAnio = true
            };

            Assert.Equal(" MATRÍCULA 3 GRADO PARA 3 GRADO 2026", cuota.PeriodoDescripcion);
        }

        [Fact]
        public void MatriculaDePreinscripto_SumaUnAnio()
        {
            // Esquema histórico exclusivo de preinscriptos: la cuota se graba con
            // anio = añoDeIngreso - 1 a propósito, y el label le suma 1 para mostrar
            // el año real de ingreso. A diferencia de un alumno cursante (donde,
            // desde este cambio, el año correcto ya se graba directo), acá SÍ sigue
            // sumando.
            var cuota = new Cuota
            {
                TipoCuota = "Matricula",
                Anio = 2026,
                NombreConcepto = "Matrícula 4 grado",
                NivelSiguiente = "4 grado",
                MatriculaEsDeEsteAnio = false
            };

            Assert.Equal(" MATRÍCULA 4 GRADO PARA 4 GRADO 2027", cuota.PeriodoDescripcion);
        }

        [Fact]
        public void MatriculaSinNombreDeConcepto_UsaEtiquetaGenerica()
        {
            var cuota = new Cuota
            {
                TipoCuota = "Matricula",
                Anio = 2026,
                NombreConcepto = null,
                NivelSiguiente = "5 grado",
                MatriculaEsDeEsteAnio = true
            };

            Assert.Equal(" MATRÍCULA PARA 5 GRADO 2026", cuota.PeriodoDescripcion);
        }

        [Fact]
        public void CuotaNormalNoMatricula_MuestraMesYAnioComoSiempre()
        {
            var cuota = new Cuota
            {
                TipoCuota = "Programatica",
                Mes = "Abril",
                Anio = 2026,
                Nivel = "3 grado"
            };

            Assert.Equal("Abril 2026  3 grado", cuota.PeriodoDescripcion);
        }
    }
}
