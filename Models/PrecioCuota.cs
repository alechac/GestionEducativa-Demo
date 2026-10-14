using System;

namespace GestionEducativa.Models
{
    public class PrecioCuota
    {
        public int Id { get; set; }
        public Nivel Nivel { get; set; }
        public string NivelDescripcion => ObtenerDescripcionNivel(Nivel);
        public Seccion Seccion { get; set; }
        public int Anio { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaVigencia { get; set; }

        private string ObtenerDescripcionNivel(Nivel nivel)
        {
            switch (nivel)
            {
                case Nivel.Jardin: return "Jardín";
                case Nivel.Primario: return "Primario";
                case Nivel.Secundario: return "Secundario";
                case Nivel.ItalianoInfantil: return "Italiano Infantil";
                case Nivel.ItalianoAdultos: return "Italiano Adultos";
                default: return nivel.ToString();
            }
        }
    }
}
