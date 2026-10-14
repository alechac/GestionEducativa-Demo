// IMainView.cs
using GestionEducativa.Models;

namespace GestionEducativa.Views.Interfaces
{
    public interface IMainView
    {
        Seccion SeccionActual { get; }
        event System.EventHandler SeccionCambiada;
    }
}
