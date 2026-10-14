using System;
using System.Windows.Forms;
using GestionEducativa.Database;
using GestionEducativa.Models;
using GestionEducativa.Services;
using GestionEducativa.Views.Interfaces;

namespace GestionEducativa.Presenters
{
    public class ConfiguracionPresenter
    {
        private readonly IConfiguracionView _view;
        private readonly ConceptoService _pagoService; // sólo se usa (comentado) para precios — vive en ConceptoService
        private readonly ConexionBD _db;

        public ConfiguracionPresenter(IConfiguracionView view)
        {
            _view = view;
            _pagoService = new ConceptoService();
            //_db = DatabaseManager.Instance;

            _view.GuardarConfigClicked += (s, e) => GuardarConfiguracion();
            _view.GuardarPrecioClicked += (s, e) => GuardarPrecio();
            _view.NuevoPrecioClicked   += (s, e) => NuevoPrecio();
        }

        public void Inicializar()
        {
            //_view.RazonSocial = _db.GetConfiguracion("RazonSocial");
            //_view.CUIT        = _db.GetConfiguracion("CUIT");
            //_view.Domicilio   = _db.GetConfiguracion("Domicilio");
            //_view.Telefono    = _db.GetConfiguracion("Telefono");
            //_view.PuntoVenta  = _db.GetConfiguracion("PuntoVenta");
            //CargarPrecios();
        }

        private void CargarPrecios()
        {
            //var precios = _pagoService.ObtenerPrecios(DateTime.Now.Year);
            //_view.MostrarPrecios(precios);
        }

        private void GuardarConfiguracion()
        {
            //try
            //{
            //    _db.SetConfiguracion("RazonSocial", _view.RazonSocial);
            //    _db.SetConfiguracion("CUIT",        _view.CUIT);
            //    _db.SetConfiguracion("Domicilio",   _view.Domicilio);
            //    _db.SetConfiguracion("Telefono",    _view.Telefono);
            //    _db.SetConfiguracion("PuntoVenta",  _view.PuntoVenta.PadLeft(4, '0'));
            //    _view.MostrarMensaje("✓ Configuración guardada correctamente.");
            //}
            //catch (Exception ex)
            //{
            //    _view.MostrarMensaje($"Error: {ex.Message}", true);
            //}
        }

        private void GuardarPrecio()
        {
            var precio = _view.PrecioSeleccionado;
            if (precio == null) return;

            try
            {
                //_pagoService.GuardarPrecio(precio);
                _view.MostrarMensaje("✓ Precio guardado correctamente.");
                CargarPrecios();
            }
            catch (Exception ex)
            {
                _view.MostrarMensaje($"Error: {ex.Message}", true);
            }
        }

        private void NuevoPrecio()
        {
            // La vista maneja el formulario inline de nuevo precio
        }
    }
}
