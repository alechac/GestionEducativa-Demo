using System;

namespace GestionEducativa.Utils
{
    /// <summary>
    /// Reglas de progresión de nivel/grado (a qué nivel pasa un alumno el año
    /// siguiente). Se usa tanto para calcular a qué matrícula corresponde un
    /// alumno (CuotaService) como para saber qué matrícula ofrecerle en el
    /// catálogo de conceptos (ConceptoService) — antes estaba duplicado en
    /// PagoService, ahora vive en un solo lugar.
    /// </summary>
    public static class NivelUtils
    {
        /// <summary>
        /// Devuelve el nivel al que pasará el alumno el año siguiente.
        /// Retorna null si el alumno está en el último nivel (6° año = egresa).
        /// </summary>
        public static string ObtenerNivelSiguiente(string nivelActual)
        {
            switch (nivelActual?.Trim().ToLower())
            {
                case "jardin de 3 anos": return "Jardin de 4 anos";
                case "jardin de 4 anos": return "Jardin de 5 anos";
                case "jardin de 5 anos": return "1 grado";
                case "1 grado": return "2 grado";
                case "2 grado": return "3 grado";
                case "3 grado": return "4 grado";
                case "4 grado": return "5 grado";
                case "5 grado": return "6 grado";
                case "6 grado": return "1 ano";
                case "1 ano": return "2 ano";
                case "2 ano": return "3 ano";
                case "3 ano": return "4 ano";
                case "4 ano": return "5 ano";
                case "5 ano": return "6 ano";
                case "6 ano": return null; // egresa
                default: return null;
            }
        }
    }
}
