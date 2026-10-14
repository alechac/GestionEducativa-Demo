using System;

namespace GestionEducativa.Models
{
    public class Alumno
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Dni { get; set; }
        public string Nivel { get; set; }
        public string Division { get; set; }   // A, B, C...
        public int Seccion { get; set; }
        public DateTime? FechaNacimiento { get; set; }

        // Descuento por familia (0-100, es un PORCENTAJE)
        public int PorcentajeDescuento { get; set; }

        public bool Activo { get; set; } = true;

        // Año lectivo en el que el alumno EMPIEZA a cursar el nivel indicado en `Nivel`.
        // - NULL: alumno ya cursando normalmente (comportamiento de siempre, sin restricciones).
        // - Año futuro (mayor al actual): alumno "preinscripto" — se anotó/pagó matrícula
        //   para el año que viene pero todavía no arrancó. No debe aparecer en los listados
        //   de alumnos "actuales" ni recibir cuotas del ciclo en curso, y su Nivel ya
        //   representa el nivel REAL al que va a entrar (no hay que calcularle "nivel siguiente").
        public int? AnioIngreso { get; set; }

        /// <summary>
        /// True si el alumno todavía no empezó a cursar (se anotó para un año lectivo posterior
        /// al de referencia, típicamente el año en curso).
        /// </summary>
        public bool EsPreinscripto(int anioReferencia) =>
            AnioIngreso.HasValue && AnioIngreso.Value > anioReferencia;

        // Calculado: saldo deudor acumulado
        public decimal SaldoPendiente { get; set; }

        // Datos del padre/tutor (cargados junto al alumno cuando se necesitan)
        public string PadreNombre { get; set; }
        public string PadreTelefono { get; set; }
        public string PadreDomicilio { get; set; }
    }
}
