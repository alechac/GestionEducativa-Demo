using System;
using System.Data;
using GestionEducativa.Models;

namespace GestionEducativa.Views.Interfaces
{
    public interface IInformesView
    {
        // Tipo de informe seleccionado
        string TipoInforme { get; }

        // Filtros comunes
        int AnioFiltro { get; }
        int MesFiltro { get; }   // 0 = todos
        string NivelFiltroTexto { get; }   // null = todos (grado/nivel exacto, ej: "3 grado")
        string NivelEducativoFiltro { get; }   // null = todos ("Inicial" | "Primaria" | "Secundaria")
        string DivisionFiltro { get; }   // null = todas
        DateTime FechaDesde { get; }
        DateTime FechaHasta { get; }

        // Filtros de alumno/padre
        int AlumnoId { get; }
        string NombrePadre { get; }
        string DocumentoPadre { get; }

        // Columnas para listado configurable
        bool ColNombre { get; }
        bool ColNivel { get; }
        bool ColDivision { get; }
        bool ColDni { get; }
        bool ColDescuento { get; }
        bool ColSaldo { get; }
        bool ColPadre { get; }
        bool ColDniPadre { get; }
        bool ColTelefono { get; }
        bool ColMailPadre { get; }

        void MostrarDatos(DataTable datos);
        void MostrarMensaje(string mensaje, bool esError = false);
        void MostrarResumenCaja(int cantPagos, decimal totalCobrado,
                                decimal totalRecibido, decimal totalVuelto,
                                decimal totalGastos, decimal totalNeto);

        event EventHandler GenerarInformeClicked;
        event EventHandler ExportarExcelClicked;
        event EventHandler ImprimirClicked;
        event EventHandler ReimprimirComprobanteClicked;
        string NroComprobanteSeleccionado { get; }
    }


}
