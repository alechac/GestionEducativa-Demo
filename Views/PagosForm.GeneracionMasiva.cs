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
    // Panel y lógica de generación masiva de cuotas por nivel/mes.
    // Parte de PagosForm (partial class) — separado sólo para que el archivo
    // principal no acumule tanto código; es la MISMA clase, no cambia nada
    // en tiempo de ejecución.
    public partial class PagosForm
    {
        private void ConstruirPanelGenerarMasiva()
        {
            // Reemplazamos los viejos controles individuales del Designer por un panel cohesivo.
            // Primero ocultamos los que ya están en el Designer (el Designer los instanció).
            if (lblGenerar != null) lblGenerar.Visible = false;
            if (lblMes != null) lblMes.Visible = false;
            if (cmbMes != null) cmbMes.Visible = false;
            if (lblAnio != null) lblAnio.Visible = false;
            if (numAnio != null) numAnio.Visible = false;
            if (btnGenerarCuota != null) btnGenerarCuota.Visible = false;

            // Crear el panel contenedor
            panelGenerarMasiva = new System.Windows.Forms.Panel
            {
                BackColor = System.Drawing.Color.FromArgb(240, 248, 255),
                BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle,
                Location = new System.Drawing.Point(12, 450),
                Size = new System.Drawing.Size(820, 130),
                Padding = new System.Windows.Forms.Padding(8)
            };
            panelCuotas.Controls.Add(panelGenerarMasiva);
            panelGenerarMasiva.BringToFront();

            // Título
            var lblTit = new System.Windows.Forms.Label
            {
                Text = "Generar cuotas mensuales por nivel",
                Font = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold),
                ForeColor = System.Drawing.Color.FromArgb(26, 82, 118),
                AutoSize = true,
                Location = new System.Drawing.Point(8, 8)
            };
            panelGenerarMasiva.Controls.Add(lblTit);

            // Etiqueta recargo
            lblRecargo = new System.Windows.Forms.Label
            {
                Text = "⚠ Vence el día 15 — recargo 4% por mes de mora",
                Font = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Italic),
                ForeColor = System.Drawing.Color.FromArgb(180, 80, 0),
                AutoSize = true,
                Location = new System.Drawing.Point(8, 26)
            };
            panelGenerarMasiva.Controls.Add(lblRecargo);

            // Mes
            var lMes = new System.Windows.Forms.Label { Text = "Mes:", AutoSize = true, Location = new System.Drawing.Point(8, 55) };
            panelGenerarMasiva.Controls.Add(lMes);
            cmbMes = new System.Windows.Forms.ComboBox
            {
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,
                Location = new System.Drawing.Point(40, 52),
                Size = new System.Drawing.Size(130, 23)
            };
            cmbMes.Items.AddRange(new object[] {
                "Enero","Febrero","Marzo","Abril","Mayo","Junio",
                "Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre"
            });
            cmbMes.SelectedIndex = Math.Max(0, DateTime.Now.Month - 1);
            panelGenerarMasiva.Controls.Add(cmbMes);

            cmbTipoCuota = new System.Windows.Forms.ComboBox
            {
                DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList,
                Location = new System.Drawing.Point(340, 22),
                Size = new System.Drawing.Size(150, 23)
            };
            // El valor que se guarda en tipoCuota pasa a ser "Programatica" / "Extraprogramatica".
            // IMPORTANTE: esto requiere migrar la columna generada `cuotas.MontoFinal` en MySQL (ver
            // notas del chat) para que su CASE WHEN contemple estos nuevos valores además de los
            // históricos "Curricular"/"Extracurricular" — si no, el descuento no se restaría para
            // las cuotas nuevas.
            cmbTipoCuota.Items.AddRange(new object[] { "Programatica", "Extraprogramatica", "Cuota Mensual" });
            panelGenerarMasiva.Controls.Add(cmbTipoCuota);


            // Año
            var lAnio = new System.Windows.Forms.Label { Text = "Año:", AutoSize = true, Location = new System.Drawing.Point(180, 55) };
            panelGenerarMasiva.Controls.Add(lAnio);
            numAnio = new System.Windows.Forms.NumericUpDown
            {
                Location = new System.Drawing.Point(210, 52),
                Size = new System.Drawing.Size(70, 23),
                Minimum = 2020,
                Maximum = 2100,
                Value = DateTime.Now.Year
            };
            panelGenerarMasiva.Controls.Add(numAnio);

            // Monto
            var lMonto = new System.Windows.Forms.Label { Text = "Monto $:", AutoSize = true, Location = new System.Drawing.Point(290, 55) };
            panelGenerarMasiva.Controls.Add(lMonto);
            numMontoCuota = new System.Windows.Forms.NumericUpDown
            {
                Location = new System.Drawing.Point(350, 52),
                Size = new System.Drawing.Size(110, 23),
                Minimum = 0,
                Maximum = 9999999,
                DecimalPlaces = 2,
                Increment = 100,
                ThousandsSeparator = true
            };
            panelGenerarMasiva.Controls.Add(numMontoCuota);

            // Niveles — CheckedListBox
            var lNiv = new System.Windows.Forms.Label { Text = "Niveles:", AutoSize = true, Location = new System.Drawing.Point(8, 83) };
            panelGenerarMasiva.Controls.Add(lNiv);

            clbNiveles = new System.Windows.Forms.CheckedListBox
            {
                Location = new System.Drawing.Point(65, 80),
                Size = new System.Drawing.Size(470, 42),
                CheckOnClick = true,
                MultiColumn = true,
                ColumnWidth = 150,
                ScrollAlwaysVisible = false,
                Font = new System.Drawing.Font("Segoe UI", 8.5f)
            };

            CargarNivelesGeneracionMasiva();
            panelGenerarMasiva.Controls.Add(clbNiveles);

            // Botón Todos / Ninguno
            btnSeleccionarTodos = new System.Windows.Forms.Button
            {
                Text = "Todos",
                Location = new System.Drawing.Point(545, 80),
                Size = new System.Drawing.Size(55, 22),
                Font = new System.Drawing.Font("Segoe UI", 8f),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(52, 152, 219),
                ForeColor = System.Drawing.Color.White,
                Cursor = System.Windows.Forms.Cursors.Hand
            };
            btnSeleccionarTodos.FlatAppearance.BorderSize = 0;
            btnSeleccionarTodos.Click += (s, e) =>
            {
                bool todosChecked = clbNiveles.CheckedItems.Count == clbNiveles.Items.Count;
                for (int i = 0; i < clbNiveles.Items.Count; i++)
                    clbNiveles.SetItemChecked(i, !todosChecked);
                btnSeleccionarTodos.Text = todosChecked ? "Todos" : "Ninguno";
            };
            panelGenerarMasiva.Controls.Add(btnSeleccionarTodos);

            // Botón Generar
            btnGenerarCuota = new System.Windows.Forms.Button
            {
                Text = "✔ Generar",
                Location = new System.Drawing.Point(608, 52),
                Size = new System.Drawing.Size(100, 52),
                Font = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Bold),
                FlatStyle = System.Windows.Forms.FlatStyle.Flat,
                BackColor = System.Drawing.Color.FromArgb(39, 174, 96),
                ForeColor = System.Drawing.Color.White,
                Cursor = System.Windows.Forms.Cursors.Hand
            };
            btnGenerarCuota.FlatAppearance.BorderSize = 0;
            btnGenerarCuota.Click += (s, e) => GenerarCuotaClicked?.Invoke(this, e);
            panelGenerarMasiva.Controls.Add(btnGenerarCuota);
        }

        /// <summary>
        /// Ejecuta la generación masiva leyendo los datos del panel.
        /// Llamado desde PagosPresenter vía el evento GenerarCuotaClicked.
        /// </summary>
        private void CargarNivelesGeneracionMasiva()
        {
            clbNiveles.Items.Clear();

            List<string> niveles;
            try
            {
                // Se filtra en la propia consulta por la sección activa (columna seccion),
                // así en Scuola Italiano aparecen A1/A2/B1/B2/Parliamo (o los niveles reales
                // que tenga cargados) y en el Colegio los grados normales — nunca mezclados.
                niveles = _cuotaService.ObtenerNivelesExistentes(SeccionActual);
            }
            catch
            {
                // Fallback con la lista conocida, por si falla la consulta a la BD
                niveles = SeccionActual == (int)Seccion.Italiano
                    ? new List<string> { "A1", "A2", "B1", "B2", "Parliamo" }
                    : new List<string> {
                        "1 ANO","2 ANO","3 ANO","4 ANO","5 ANO","6 ANO",
                        "1 GRADO","2 GRADO","3 GRADO","4 GRADO","5 GRADO","6 GRADO",
                        "JARDIN DE 3 ANOS","JARDIN DE 4 ANOS","JARDIN DE 5 ANOS"
                    };
            }

            foreach (var n in niveles) clbNiveles.Items.Add(n, false);
        }

        public void EjecutarGenerarCuotasMasivas()
        {
            // Validaciones
            var nivelesSeleccionados = new List<string>();
            foreach (var item in clbNiveles.CheckedItems)
                nivelesSeleccionados.Add(item.ToString());

            if (nivelesSeleccionados.Count == 0)
            {
                MostrarMensaje("Seleccioná al menos un nivel.", true);
                return;
            }

            string mes = cmbMes.SelectedItem?.ToString();
            string tipoCuota = cmbTipoCuota.SelectedItem?.ToString();
            int anio = (int)numAnio.Value;
            decimal monto = numMontoCuota.Value;

            if (string.IsNullOrEmpty(mes))
            {
                MostrarMensaje("Seleccioná el mes.", true);
                return;
            }
            if (monto <= 0)
            {
                MostrarMensaje("Ingresá el monto de la cuota.", true);
                return;
            }

            string resumen = $"Se generarán cuotas {tipoCuota} de {mes} {anio}\n" +
                             $"Monto: $ {monto:N2}\n" +
                             $"Niveles: {string.Join(", ", nivelesSeleccionados)}\n\n" +
                             $"¿Confirmás?";

            if (!ConfirmarAccion(resumen)) return;

            try
            {
                btnGenerarCuota.Enabled = false;
                btnGenerarCuota.Text = "Generando...";
                var (generadas, salteadas) = _cuotaService.GenerarCuotasMasivas(
                    nivelesSeleccionados, mes, anio, monto, tipoCuota);

                string msg = $"✓ Cuotas generadas: {generadas}";
                if (salteadas > 0)
                    msg += $"\n⚠ Salteadas (ya existían): {salteadas}";
                MostrarMensaje(msg);

                // Refrescar la grilla del alumno seleccionado si hay uno
                if (_alumnoSeleccionado != null) CargarCuotasAlumno();
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al generar cuotas: " + ex.Message, true);
            }
            finally
            {
                btnGenerarCuota.Enabled = true;
                btnGenerarCuota.Text = "✔ Generar";
            }
        }

        // ===== Eventos del grid de alumnos =====

    }
}
