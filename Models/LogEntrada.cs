using System;

namespace GestionEducativa.Models
{
    public class LogEntrada
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public string Operador { get; set; }
        public string Accion { get; set; }
        public string Detalle { get; set; }
    }
}
