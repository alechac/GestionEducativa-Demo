using System;

namespace GestionEducativa.Models
{
    /// <summary>
    /// Representa una cuota generada para un alumno en un mes/año determinado.
    /// </summary>
    public class Cuota
    {
        public int Id { get; set; }
        public int AlumnoId { get; set; }
        public string AlumnoNombre { get; set; }
        public string Mes { get; set; }
        public int Anio { get; set; }
        public string TipoCuota { get; set; }        // "Curricular" | "Extracurricular"
        public int? ConceptoId { get; set; }
        // Nombre real del concepto cargado por el usuario (ej: "Reserva de Matrícula", "Seguro Escolar").
        // Puede diferir del TipoCuota genérico ("Matricula") usado internamente para lógica de negocio.
        public string Nivel { get; set; }
        public string NombreConcepto { get; set; }
        public decimal MontoOriginal { get; set; }
        public decimal Descuento { get; set; }
        public decimal MontoFinal { get; set; }
        public decimal MontoPagado { get; set; }
        public decimal Saldo { get; set; }
        public string Estado { get; set; }
        public decimal Recargo { get; set; }
        public DateTime? FechaVencimiento { get; set; }

        // Nivel al que pasa el alumno (se setea al cargar cuotas de matrícula)
        public string NivelSiguiente { get; set; }

        // True si esta cuota de matrícula es la del nivel ACTUAL del alumno (este año),
        // false si es la del nivel siguiente (año que viene) o no aplica (no es matrícula).
        public bool MatriculaEsDeEsteAnio { get; set; }

        // Descripción combinada para mostrar en grillas y comprobantes
        public string PeriodoDescripcion
        {
            get
            {
                bool esMatricula = string.Equals(TipoCuota, "Matricula", System.StringComparison.OrdinalIgnoreCase);
                if (esMatricula && !string.IsNullOrEmpty(NivelSiguiente))
                {
                    // Antes esta rama solo devolvía "PARA {NIVEL} {AÑO}", sin identificar de qué
                    // concepto de matrícula se trataba. Con "Reserva de Matrícula" como concepto
                    // aparte de la matrícula regular, dos cuotas distintas terminaban mostrando el
                    // mismo texto (ej. en el resumen de confirmación de cobro). Se antepone el
                    // nombre real del concepto cuando existe.
                    string etiquetaMatricula = !string.IsNullOrWhiteSpace(NombreConcepto)
                        ? NombreConcepto.ToUpper()
                        : "MATRÍCULA";
                    // El "+1" sólo corresponde cuando la matrícula es la del nivel SIGUIENTE
                    // (año que viene). Si es la del nivel actual (este año, para el alumno que
                    // todavía no la pagó), se muestra el año de la cuota tal cual, sin sumar nada.
                    int anioAMostrar = MatriculaEsDeEsteAnio ? Anio : Anio + 1;
                    return $" {etiquetaMatricula} PARA {NivelSiguiente.ToUpper()} {anioAMostrar}";
                }
                var level = string.IsNullOrEmpty(Nivel) ? "" : $" {Nivel}";
                return string.IsNullOrEmpty(TipoCuota)
                    ? $"{Mes} {Anio}   {Nivel}"
                    : $"{Mes} {Anio}  {Nivel}";
            }
        }
    }
}
