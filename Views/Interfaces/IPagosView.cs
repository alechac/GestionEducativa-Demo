using System;
using System.Collections.Generic;
using GestionEducativa.Models;

namespace GestionEducativa.Views.Interfaces
{
    public interface IPagosView
    {
        int SeccionActual { get; set; }

        // Datos del formulario de cobro
        decimal MontoRecibido { get; }
        string NombrePagador { get; }
        string DocumentoPagador { get; }
        string TipoDocumento { get; }    // "DNI" | "CUIT"
        string FormaPago { get; }        // "Efectivo" | "Transferencia" | "Cheque"
        string Observaciones { get; }
        bool EnviarPorMail { get; }
        MesCuota MesSeleccionado { get; }
        int AnioSeleccionado { get; }

        // Cuotas seleccionadas para pagar (puede ser de varios alumnos)
        List<Cuota> CuotasParaPagar { get; }

        void MostrarCuotas(List<Cuota> cuotas);
        void MostrarResumenPago(decimal totalDeuda, decimal montoRecibido);
        void ActualizarCalculoPago(decimal totalDeuda, decimal montoRecibido);
        void LimpiarFormulario();
        void GuardarOVincularPadreSiCorresponde();
        void CargarDatosPadre(Padre padre);

        void MostrarMensaje(string mensaje, bool esError = false);
        bool ConfirmarAccion(string mensaje);

        event EventHandler AlumnoCambiado;
        event EventHandler RegistrarPagoClicked;
        event EventHandler GenerarCuotaClicked;
        event EventHandler ImprimirComprobanteClicked;
    }
}
