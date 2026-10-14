namespace GestionEducativa.Utils
{
    /// <summary>
    /// No es un sistema de login — nadie valida contraseña ni permisos acá. Es sólo
    /// el nombre que la persona escribió al abrir la app ("¿Quién va a usar el sistema
    /// hoy?"), guardado en memoria mientras la app esté abierta, para poder anotarlo
    /// junto a cada operación sensible (LogService.Registrar). El objetivo es dejar de
    /// ser anónimos, no restringir quién puede hacer qué.
    /// </summary>
    public static class SesionActual
    {
        public static string Operador { get; set; } = "Sin identificar";
    }
}
