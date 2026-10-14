using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionEducativa.Models
{
    public class Concepto
    {
        public int Id { get; set; }

        /// <summary>Nombre visible: "Seguro Escolar", "Viaje GEA", "Entorno Digital", etc.</summary>
        public string Nombre { get; set; }

        /// <summary>
        /// Agrupa conceptos en familias para filtrar y colorear en la UI.
        /// Valores posibles: "Seguro" | "Viaje" | "Matricula" | "Material" | "Bingo" | "Otro"
        /// </summary>
        public string Categoria { get; set; }

        /// <summary>
        /// Nivel al que aplica (ej: "5 grado"). NULL significa que aplica a todos los niveles.
        /// </summary>
        public string Nivel { get; set; }

        public decimal Monto { get; set; }
        public int Anio { get; set; }

        /// <summary>Texto libre que aparece en el comprobante de pago.</summary>
        public string Descripcion { get; set; }

        public bool Activo { get; set; } = true;

        /// <summary>
        /// Devuelve true si este concepto aplica al nivel del alumno.
        /// Un concepto con Nivel NULL aplica a todos.
        /// La comparación es case-insensitive y tolera variaciones menores.
        /// </summary>
        public bool AplicaANivel(string nivelAlumno)
        {
            if (string.IsNullOrEmpty(Nivel)) return true;
            return string.Equals(
                Nivel.Trim(),
                nivelAlumno?.Trim() ?? "",
                System.StringComparison.OrdinalIgnoreCase);
        }

        public override string ToString() => $"{Nombre} — $ {Monto:N2}";
    }
}
