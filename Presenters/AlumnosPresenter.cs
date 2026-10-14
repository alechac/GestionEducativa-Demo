using System;
using System.Windows.Forms;
using GestionEducativa.Models;
using GestionEducativa.Services;
using GestionEducativa.Views.Interfaces;

namespace GestionEducativa.Presenters
{
    public class AlumnosPresenter
    {
        private readonly IAlumnosView _view;
        private readonly AlumnoService _alumnoService;
        private readonly ImportacionService _importService;

        public AlumnosPresenter(IAlumnosView view)
        {
            _view = view;
            _alumnoService = new AlumnoService();
            _importService = new ImportacionService();

            _view.BuscarClicked += (s, e) => CargarAlumnos();
            _view.NuevoAlumnoClicked += (s, e) => AbrirNuevoAlumno();
            _view.EditarAlumnoClicked += (s, e) => AbrirEditarAlumno();
            _view.EliminarAlumnoClicked += (s, e) => EliminarAlumno();
            _view.ImportarExcelClicked += (s, e) => ImportarDesdeExcel();
        }

        public void Inicializar()
        {
            CargarAlumnos();
        }

        private void CargarAlumnos()
        {
            
            try
            {
                if (_view.NivelFiltro == "Todos")
                {
                    var alumnos = _alumnoService.ObtenerTodos(
                    _view.SeccionActual,
                    "",
                    _view.TextoBusqueda,
                    incluirFuturos: _view.MostrarPreinscriptos);
                    _view.MostrarAlumnos(alumnos);

                }
                else
                {
                    var alumnos = _alumnoService.ObtenerTodos(
                        _view.SeccionActual,
                        _view.NivelFiltro,
                        _view.TextoBusqueda,
                        incluirFuturos: _view.MostrarPreinscriptos);

                    _view.MostrarAlumnos(alumnos);
                }
            }
            catch (Exception ex)
            {
                _view.MostrarMensaje($"Error al cargar alumnos: {ex.Message}", true);
            }
        }



        private void AbrirNuevoAlumno()
        {
            using (var form = new Views.NuevoAlumnoForm(_view.SeccionActual))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _alumnoService.Insertar(form.AlumnoResultante);
                        _view.MostrarMensaje("✓ Alumno registrado correctamente.");
                        CargarAlumnos();
                    }
                    catch (Exception ex)
                    {
                        _view.MostrarMensaje($"Error al guardar: {ex.Message}", true);
                    }
                }
            }
        }

        private void AbrirEditarAlumno()
        {
            var alumno = _view.AlumnoSeleccionado;
            if (alumno == null)
            {
                _view.MostrarMensaje("Por favor, seleccione un alumno.", true);
                return;
            }

            using (var form = new Views.NuevoAlumnoForm(_view.SeccionActual, alumno))
            {
                if (form.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        _alumnoService.Actualizar(form.AlumnoResultante);
                        _view.MostrarMensaje("✓ Alumno actualizado correctamente.");
                        CargarAlumnos();
                    }
                    catch (Exception ex)
                    {
                        _view.MostrarMensaje($"Error al actualizar: {ex.Message}", true);
                    }
                }
            }
        }

        private void EliminarAlumno()
        {
            var alumno = _view.AlumnoSeleccionado;
            if (alumno == null)
            {
                _view.MostrarMensaje("Por favor, seleccione un alumno.", true);
                return;
            }

            if (_view.ConfirmarAccion($"¿Desea dar de baja al alumno {alumno.Nombre}?\n\nSus datos de pago se conservarán."))
            {
                try
                {
                    _alumnoService.EliminarLogico(alumno.Id);
                    _view.MostrarMensaje("✓ Alumno dado de baja correctamente.");
                    CargarAlumnos();
                }
                catch (Exception ex)
                {
                    _view.MostrarMensaje($"Error: {ex.Message}", true);
                }
            }
        }

        private void ImportarDesdeExcel()
        {
            //using (var dlg = new OpenFileDialog
            //{
            //    Title = "Seleccionar archivo Excel",
            //    Filter = "Archivos Excel (*.xlsx;*.xls)|*.xlsx;*.xls",
            //    Multiselect = false
            //})
            //{
            //    if (dlg.ShowDialog() != DialogResult.OK) return;

            //    try
            //    {
            //        var alumnos = _importService.LeerAlumnosDesdeExcel(dlg.FileName, _view.SeccionActual);

            //        if (alumnos.Count == 0)
            //        {
            //            _view.MostrarMensaje("No se encontraron alumnos en el archivo.", true);
            //            return;
            //        }

            //        if (_view.ConfirmarAccion($"Se encontraron {alumnos.Count} alumnos en el archivo.\n¿Desea importarlos?"))
            //        {
            //            int importados = 0;
            //            foreach (var a in alumnos)
            //            {
            //                _alumnoService.Insertar(a);
            //                importados++;
            //            }
            //            _view.MostrarMensaje($"✓ Se importaron {importados} alumnos correctamente.");
            //            CargarAlumnos();
            //        }
            //    }
            //    catch (Exception ex)
            //    {
            //        _view.MostrarMensaje($"Error al importar: {ex.Message}", true);
            //    }
            //}
        }
    }
}
