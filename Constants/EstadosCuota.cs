namespace GestionEducativa.Constants
{
    /// <summary>
    /// Valores posibles de `cuotas.estado`. Antes estos 4 strings estaban sueltos
    /// y repetidos en CuotaService y PagoService — un typo en cualquiera de las
    /// copias rompía un filtro o una comparación en silencio (ej. WHERE estado
    /// &lt;&gt; 'Pagado'). Centralizados acá para escribirlos una sola vez.
    /// </summary>
    public static class EstadosCuota
    {
        public const string Pendiente = "Pendiente";
        public const string PagoParcial = "Pago Parcial";
        public const string Pagado = "Pagado";
        public const string Vencida = "Vencida";
    }
}
