using System;
using System.Collections.Generic;
using GestionEducativa.Models;

namespace GestionEducativa.Views.Interfaces
{
    public interface IAlumnosView
    {
        // Filtros
        string TextoBusqueda { get; }
        string NivelFiltro { get; }
        int SeccionActual { get; set; }
        bool MostrarPreinscriptos { get; }

        // Datos
        void MostrarAlumnos(List<Alumno> alumnos);
        Alumno AlumnoSeleccionado { get; }

        // Mensajes
        void MostrarMensaje(string mensaje, bool esError = false);
        bool ConfirmarAccion(string mensaje);

        // Eventos
        event EventHandler BuscarClicked;
        event EventHandler NuevoAlumnoClicked;
        event EventHandler EditarAlumnoClicked;
        event EventHandler EliminarAlumnoClicked;
        event EventHandler VerCuotasClicked;
        event EventHandler ImportarExcelClicked;
    }
}
