using System;
using System.Collections.Generic;
using GestionEducativa.Models;

namespace GestionEducativa.Views.Interfaces
{
    public interface IConfiguracionView
    {
        string RazonSocial { get; set; }
        string CUIT { get; set; }
        string Domicilio { get; set; }
        string Telefono { get; set; }
        string PuntoVenta { get; set; }

        void MostrarPrecios(List<PrecioCuota> precios);
        PrecioCuota PrecioSeleccionado { get; }

        void MostrarMensaje(string mensaje, bool esError = false);

        event EventHandler GuardarConfigClicked;
        event EventHandler GuardarPrecioClicked;
        event EventHandler NuevoPrecioClicked;
    }
}
