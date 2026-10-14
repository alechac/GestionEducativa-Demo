using GestionEducativa.Models;
using GestionEducativa.Presenters;
using GestionEducativa.Services;
using GestionEducativa.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    // Datos del padre/tutor: búsqueda por documento, hermanos, carga y limpieza de datos.
    // Parte de PagosForm (partial class) — separado sólo para que el archivo
    // principal no acumule tanto código; es la MISMA clase, no cambia nada
    // en tiempo de ejecución.
    public partial class PagosForm
    {
        private void AgregarCheckGuardarPadre()
        {
            chkGuardarPadre = new System.Windows.Forms.CheckBox
            {
                Name = "chkGuardarPadre",
                Text = "Guardar como nuevo padre/tutor",
                AutoSize = true,
                ForeColor = Color.FromArgb(26, 82, 118),
                Location = new Point(12, 444),
                Visible = false
            };
            panelCobro.Controls.Add(chkGuardarPadre);
            chkGuardarPadre.BringToFront();

            const int desplazamiento = 24;
            lblObservaciones.Top += desplazamiento;
            txtObservaciones.Top += desplazamiento;
            btnRegistrarPago.Top += desplazamiento;
            btnImprimir.Top += desplazamiento;
        }

        /// <summary>
        /// Combo para elegir CUÁL padre/tutor paga, cuando el alumno tiene más de uno
        /// vinculado (ej. un día viene el papá, otro día la mamá) — así el cajero no
        /// tiene que tipear el documento entero a mano para cambiarlo, sólo elegirlo
        /// de la lista. Se ubica en un espacio libre que ya existía en el panel, entre
        /// el monto recibido y los datos del pagador — por eso no hace falta desplazar
        /// ningún otro control (a diferencia de chkGuardarPadre, arriba).
        /// Permanece oculto salvo que el alumno tenga 2 o más padres/tutores cargados.
        /// </summary>
        private void AgregarComboElegirPadre()
        {
            lblElegirPadre = new System.Windows.Forms.Label
            {
                Name = "lblElegirPadre",
                Text = "¿Quién paga hoy?",
                AutoSize = true,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                Location = new Point(12, 215),
                Visible = false
            };
            cmbElegirPadre = new System.Windows.Forms.ComboBox
            {
                Name = "cmbElegirPadre",
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new Point(12, 234),
                Size = new Size(215, 23),
                Visible = false
            };
            cmbElegirPadre.SelectedIndexChanged += (s, e) =>
            {
                int i = cmbElegirPadre.SelectedIndex;
                if (i >= 0 && i < _padresDelAlumnoActual.Count)
                    CargarDatosPadre(_padresDelAlumnoActual[i]);
            };

            panelCobro.Controls.Add(lblElegirPadre);
            panelCobro.Controls.Add(cmbElegirPadre);
        }

        /// <summary>
        /// Se llama después de registrar el pago con éxito. Si el cajero tildó el checkbox,
        /// resuelve según el modo detectado en la última búsqueda por documento:
        ///  - _padreDetectadoNoVinculado == null → el documento no existía: se da de alta.
        ///  - _padreDetectadoNoVinculado != null → ya existía (tutor de otro alumno): se
        ///    vincula también a este alumno, sin duplicarlo.
        /// No se aborta el flujo si esto falla: el pago ya quedó registrado igual.
        /// </summary>
        public void GuardarOVincularPadreSiCorresponde()
        {
            if (chkGuardarPadre == null || !chkGuardarPadre.Visible || !chkGuardarPadre.Checked)
                return;

            try
            {
                if (_padreDetectadoNoVinculado == null)
                {
                    var padre = _padreService.GuardarPadreNuevo(txtPagador.Text.Trim(), txtDocumento.Text.Trim(),
                        TipoDocumento, _alumnoSeleccionado?.Id);
                    MostrarMensaje($"✓ Padre/tutor \"{padre.NombreCompleto}\" guardado en la base" +
                        (_alumnoSeleccionado != null ? $" y vinculado a {_alumnoSeleccionado.Nombre}." : "."));
                }
                else if (_alumnoSeleccionado != null)
                {
                    _padreService.VincularAlumno(_padreDetectadoNoVinculado.Id, _alumnoSeleccionado.Id);
                    MostrarMensaje($"✓ \"{_padreDetectadoNoVinculado.NombreCompleto}\" quedó vinculado también como tutor de {_alumnoSeleccionado.Nombre}.");
                }
            }
            catch (Exception ex)
            {
                MostrarMensaje("El pago se registró, pero no se pudo guardar/vincular el padre: " + ex.Message, true);
            }
            finally
            {
                chkGuardarPadre.Visible = false;
                chkGuardarPadre.Checked = false;
                _padreDetectadoNoVinculado = null;
                ActualizarHermanos();
            }
        }

        private bool _recargosAplicados = false;
        private void BuscarPadrePorDocumento()
        {
            string doc = txtDocumento.Text.Trim();

            if (string.IsNullOrEmpty(doc))
            {
                chkGuardarPadre.Visible = false;
                chkGuardarPadre.Checked = false;
                _padreDetectadoNoVinculado = null;
                ActualizarHermanos();
                return;
            }

            try
            {
                var padre = _padreService.ObtenerPorDocumento(doc);
                if (padre != null && !string.IsNullOrEmpty(padre.NombreCompleto))
                {
                    txtPagador.Text = padre.NombreCompleto;

                    bool yaVinculadoAEsteAlumno = _alumnoSeleccionado != null &&
                        _padreService.ObtenerPadresPorAlumno(_alumnoSeleccionado.Id)
                            .Any(p => p.Id == padre.Id);

                    if (yaVinculadoAEsteAlumno || _alumnoSeleccionado == null)
                    {
                        // Ya es tutor de este alumno (o todavía no hay alumno elegido): nada que ofrecer.
                        _padreDetectadoNoVinculado = null;
                        chkGuardarPadre.Visible = false;
                        chkGuardarPadre.Checked = false;
                    }
                    else
                    {
                        // Está cargado en la base (seguramente como tutor de otro hijo/hermano)
                        // pero no de ESTE alumno: ofrecemos vincularlo también acá al confirmar el
                        // cobro, para que la próxima vez el sistema ya los relacione como hermanos.
                        _padreDetectadoNoVinculado = padre;
                        chkGuardarPadre.Text = "Vincular como tutor de este alumno";
                        chkGuardarPadre.Visible = true;
                        chkGuardarPadre.Checked = false;
                    }
                }
                else
                {
                    // No está en la base: se puede seguir cargando el nombre a mano,
                    // y ofrecer guardarlo como padre/tutor nuevo para la próxima vez.
                    _padreDetectadoNoVinculado = null;
                    chkGuardarPadre.Text = "Guardar como nuevo padre/tutor";
                    chkGuardarPadre.Visible = true;
                    chkGuardarPadre.Checked = false;
                }
            }
            catch
            {
                /* Si falla la búsqueda, simplemente no autocompletamos */
                _padreDetectadoNoVinculado = null;
                chkGuardarPadre.Text = "Guardar como nuevo padre/tutor";
                chkGuardarPadre.Visible = !string.IsNullOrEmpty(doc);
            }

            // El documento tipeado puede corresponder a un padre que ya es tutor de otro
            // alumno (hermano): lo tenemos en cuenta para la lista de hermanos aunque
            // todavía no esté formalmente vinculado a este alumno puntual.
            ActualizarHermanos();
        }

        private void CargarDatosPadreAutomatico()
        {
            if (_alumnoSeleccionado == null) return;

            try
            {
                var padres = _padreService.ObtenerPadresPorAlumno(_alumnoSeleccionado.Id);
                _padresDelAlumnoActual = padres;

                if (padres.Count > 1)
                {
                    // Más de un padre/tutor vinculado: se deja elegir cuál paga hoy, en vez
                    // de asumir siempre el primero. El combo dispara CargarDatosPadre solo
                    // (ver AgregarComboElegirPadre) al cambiar de selección.
                    cmbElegirPadre.Items.Clear();
                    foreach (var p in padres) cmbElegirPadre.Items.Add(p.NombreCompleto);
                    lblElegirPadre.Visible = true;
                    cmbElegirPadre.Visible = true;
                    cmbElegirPadre.SelectedIndex = 0; // puede no disparar el evento si ya estaba en 0
                    CargarDatosPadre(padres[0]); // por eso se llama también acá, explícito
                }
                else
                {
                    lblElegirPadre.Visible = false;
                    cmbElegirPadre.Visible = false;

                    if (padres.Count == 1)
                    {
                        CargarDatosPadre(padres[0]);
                    }
                    else if (_alumnoSeleccionado.Seccion == (int)Seccion.Italiano
                             && !string.IsNullOrWhiteSpace(_alumnoSeleccionado.Dni))
                    {
                        // No tiene padre/tutor vinculado, pero sí tiene su propio documento
                        // cargado — típico de alumnos adultos de Scuola Italiano, que pagan
                        // ellos mismos su cuota. Se autocompleta con sus propios datos, así el
                        // cajero no tiene que tipearlo a mano cada vez.
                        // OJO: esto es EXCLUSIVO de Italiano — un alumno del colegio (jardín,
                        // primaria, secundaria) nunca paga su propia cuota, aunque por algún
                        // motivo no tenga un padre/tutor cargado todavía. Para esos casos se
                        // deja el campo vacío a propósito, para que el cajero lo complete a
                        // mano con el dato de quien realmente está pagando.
                        CargarDatosPropiosDelAlumno(_alumnoSeleccionado);
                    }
                    else
                    {
                        LimpiarDatosPadre();
                    }
                }

                ActualizarHermanos();
            }
            catch
            {
                LimpiarDatosPadre();
            }
        }

        /// <summary>
        /// Autocompleta el pagador con los datos del propio alumno (nombre + documento),
        /// para el caso de alumnos que pagan su propia cuota sin un padre/tutor de por medio.
        /// A diferencia de CargarDatosPadre, esto no oculta el checkbox de "guardar como
        /// padre": no tendría sentido crear un registro de Padre para la misma persona que
        /// ya es Alumno.
        /// </summary>
        private void CargarDatosPropiosDelAlumno(Alumno alumno)
        {
            _padreDetectadoNoVinculado = null;
            chkGuardarPadre.Visible = false;
            chkGuardarPadre.Checked = false;

            lblValNombrePadre.Text = $"{alumno.Nombre} (paga su propia cuota)";
            string docLimpio = (alumno.Dni ?? "").Replace(".", "").Replace("-", "").Trim();
            bool esCuil = docLimpio.Length >= 11; // CUIL/CUIT: 11 dígitos: DNI: menos

            lblValDocPadre.Text = esCuil ? $"CUIL: {docLimpio}" : $"DNI: {docLimpio}";
            lblValTelPadre.Text = "-";

            txtPagador.Text = alumno.Nombre;
            txtDocumento.Text = docLimpio;
            if (esCuil) rdnCUIT.Checked = true; else rdnDNI.Checked = true;
        }

        /// <summary>
        /// Recalcula la lista de "hermanos" del alumno seleccionado, teniendo en cuenta TODOS
        /// los padres/tutores vinculados a él (no solo el primero) y, además, el documento que
        /// el cajero haya tipeado en el campo "Documento" del pagador: si ese documento ya
        /// corresponde a un padre cargado como tutor de otro alumno (aunque no de este), igual
        /// se suman como hermanos. Se llama tanto al elegir un alumno como al salir del campo
        /// de documento, para que la lista se actualice a medida que se tipea.
        /// </summary>
        private void ActualizarHermanos()
        {
            if (_alumnoSeleccionado == null)
            {
                _alumnosHermanos.Clear();
                btnVerHermanos.Visible = false;
                return;
            }

            try
            {
                _alumnosHermanos = _padreService.ObtenerHermanos(_alumnoSeleccionado.Id, txtDocumento.Text.Trim());
                bool tieneHermanos = _alumnosHermanos.Any(a => a.Id != _alumnoSeleccionado.Id);
                btnVerHermanos.Visible = tieneHermanos;
                btnVerHermanos.Text = tieneHermanos
                    ? $"+ Ver hermanos ({_alumnosHermanos.Count - 1})"
                    : "";
            }
            catch
            {
                _alumnosHermanos.Clear();
                btnVerHermanos.Visible = false;
            }
        }

        public void CargarDatosPadre(Padre padre)
        {
            if (padre == null) { LimpiarDatosPadre(); return; }

            chkGuardarPadre.Visible = false;
            chkGuardarPadre.Checked = false;
            _padreDetectadoNoVinculado = null;

            lblValNombrePadre.Text = padre.NombreCompleto;
            lblValDocPadre.Text = !string.IsNullOrEmpty(padre.DNI)
                ? $"DNI: {padre.DNI}"
                : !string.IsNullOrEmpty(padre.CUIT)
                    ? $"CUIL: {padre.CUIT}"
                    : "-";
            lblValTelPadre.Text = padre.Telefono ?? "-";

            txtPagador.Text = padre.NombreCompleto;
            if (!string.IsNullOrEmpty(padre.DNI))
            {
                txtDocumento.Text = padre.DNI;
                rdnDNI.Checked = true;
            }
            else if (!string.IsNullOrEmpty(padre.CUIT))
            {
                txtDocumento.Text = padre.CUIT;
                rdnCUIT.Checked = true;
            }
        }

        private void LimpiarDatosPadre()
        {
            lblValNombrePadre.Text = "-";
            lblValDocPadre.Text = "-";
            lblValTelPadre.Text = "-";
        }

        private void btnVerHermanos_Click(object sender, EventArgs e)
        {
            if (_alumnoSeleccionado == null || _alumnosHermanos.Count == 0) return;

            try
            {
                _mostrandoHermanos = true;
                CargarCuotasAlumno();

                lblNombreAlumno.Text = $"{_alumnoSeleccionado.Nombre} + hermanos";
                lblNombreAlumno.ForeColor = Color.FromArgb(142, 68, 173);
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al cargar cuotas de hermanos: " + ex.Message, true);
            }
        }

    }
}
