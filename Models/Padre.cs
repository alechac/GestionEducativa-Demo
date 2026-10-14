using System;

namespace GestionEducativa.Models
{
    /// <summary>
    /// Representa el padre/tutor de uno o más alumnos.
    /// </summary>
    public class Padre
    {
        public int Id { get; set; }
        public string Nombre { get; set; }
        public string Apellido { get; set; }
        public string NombreCompleto => $"{Apellido}, {Nombre}";
        public string DNI { get; set; }
        public string CUIT { get; set; }
        public string Telefono { get; set; }
        public string Email { get; set; }
        public bool Activo { get; set; } = true;
    }
}
