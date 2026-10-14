using GestionEducativa.Utils;
using Xunit;

namespace GestionEducativa.Tests
{
    /// <summary>
    /// Progresión de nivel: a qué grado/año pasa un alumno el año siguiente.
    /// Es la lógica que decide, por ejemplo, la matrícula del año que viene —
    /// un error acá afecta directamente lo que se le cobra a cada familia.
    /// </summary>
    public class NivelUtilsTests
    {
        [Theory]
        [InlineData("Jardin de 3 anos", "Jardin de 4 anos")]
        [InlineData("Jardin de 5 anos", "1 grado")]
        [InlineData("6 grado", "1 ano")]     // pasaje primaria -> secundaria
        [InlineData("5 ano", "6 ano")]
        public void ProgresionesConocidas_DevuelvenElNivelCorrecto(string actual, string esperado)
        {
            string resultado = NivelUtils.ObtenerNivelSiguiente(actual);
            Assert.Equal(esperado, resultado);
        }

        [Fact]
        public void UltimoAnio_DevuelveNull_PorqueEgresa()
        {
            string resultado = NivelUtils.ObtenerNivelSiguiente("6 ano");
            Assert.Null(resultado);
        }

        [Fact]
        public void NivelNoReconocido_DevuelveNull()
        {
            string resultado = NivelUtils.ObtenerNivelSiguiente("Parliamo");
            Assert.Null(resultado);
        }

        [Theory]
        [InlineData("  1 GRADO  ")]   // espacios y mayúsculas
        [InlineData("1 grado")]
        public void NoDistingueMayusculasNiEspacios(string variante)
        {
            Assert.Equal("2 grado", NivelUtils.ObtenerNivelSiguiente(variante));
        }
    }
}
