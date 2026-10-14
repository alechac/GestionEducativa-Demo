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
    // Grilla de cuotas: selección, distribución automática del monto recibido, ajuste manual de saldo/recargo.
    // Parte de PagosForm (partial class) — separado sólo para que el archivo
    // principal no acumule tanto código; es la MISMA clase, no cambia nada
    // en tiempo de ejecución.
    public partial class PagosForm
    {
        private void AgregarCheckEnviarPorMail()
        {
            chkEnviarPorMail = new System.Windows.Forms.CheckBox
            {
                Name = "chkEnviarPorMail",
                Text = "📧 Enviar comprobante por mail al padre/tutor",
                AutoSize = true,
                ForeColor = Color.FromArgb(26, 82, 118),
                Location = new Point(12, 468),
                Checked = false 
            };
            panelCobro.Controls.Add(chkEnviarPorMail);
            chkEnviarPorMail.BringToFront();

            const int desplazamiento2 = 24;
            lblObservaciones.Top += desplazamiento2;
            txtObservaciones.Top += desplazamiento2;
            btnRegistrarPago.Top += desplazamiento2;
            btnImprimir.Top += desplazamiento2;
        }

        private void CargarCuotasAlumno()
        {
            if (_alumnoSeleccionado == null) return;

            try
            {
                bool mostrarTodas = chkMostrarTodas.Checked;

                // Si se estaba mostrando la vista combinada de hermanos, se mantiene después
                // de refrescar (por ejemplo, tras ajustar el recargo de una cuota) — antes acá
                // se volvía a cargar solo el alumno seleccionado y se perdía la vista de
                // hermanos, obligando a apretar "Ver hermanos" de nuevo después de cada cambio.
                if (_mostrandoHermanos && _alumnosHermanos.Count > 0)
                {
                    var ids = _alumnosHermanos.Select(a => a.Id).ToList();
                    _todasLasCuotas = mostrarTodas
                        ? _cuotaService.ObtenerCuotasAlumnos(ids)
                        : _cuotaService.ObtenerCuotasPendientesAlumnos(ids);
                }
                else if (mostrarTodas)
                {
                    _todasLasCuotas = _cuotaService.ObtenerCuotasAlumno(_alumnoSeleccionado.Id);
                }
                else
                {
                    _todasLasCuotas = _cuotaService.ObtenerCuotasPendientesAlumno(_alumnoSeleccionado.Id);
                }

                MostrarCuotas(_todasLasCuotas);
            }
            catch (Exception ex)
            {
                MostrarMensaje("Error al cargar cuotas: " + ex.Message, true);
            }
        }

        public void MostrarCuotas(List<Cuota> cuotas)
        {
            gridCuotas.CellValueChanged -= gridCuotas_CellValueChanged;
            gridCuotas.DataSource = null;
            gridCuotas.Columns.Clear();

            var colCheck = new DataGridViewCheckBoxColumn
            {
                Name = "colSeleccionar",
                HeaderText = "✓",
                Width = 35,
                ReadOnly = false
            };
            gridCuotas.Columns.Add(colCheck);

            gridCuotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAlumno", HeaderText = "Alumno", Width = 120, ReadOnly = true });
            gridCuotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPeriodo", HeaderText = "Período", Width = 80, ReadOnly = true });
            gridCuotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTipo", HeaderText = "Tipo / Concepto", Width = 140, ReadOnly = true });
            gridCuotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMontoFinal", HeaderText = "Cuota", Width = 80, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            gridCuotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDescuento", HeaderText = "Descuento", Width = 55, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "0'%'" } });
            gridCuotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPagado", HeaderText = "Pagado", Width = 80, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            gridCuotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSaldo", HeaderText = "Saldo", Width = 85, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2" } });
            gridCuotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colRecargo", HeaderText = "Recargo", Width = 95, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", ForeColor = System.Drawing.Color.FromArgb(180, 60, 0) } });
            gridCuotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colVencimiento", HeaderText = "Vence", Width = 90, ReadOnly = true });
            gridCuotas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEstado", HeaderText = "Estado", Width = 90, ReadOnly = true });

            foreach (var cuota in cuotas)
            {
                decimal saldoConRecargo = CuotaService.CalcularSaldoConRecargo(cuota);
                bool tieneRecargo = saldoConRecargo > cuota.Saldo && cuota.Saldo > 0;

                int rowIdx = gridCuotas.Rows.Add();
                var row = gridCuotas.Rows[rowIdx];
                row.Tag = cuota;
                bool esBonificada100 = cuota.Descuento >= 100 && cuota.Saldo <= 0 && cuota.Estado != "Pagado";

                row.Cells["colSeleccionar"].Value = cuota.Estado != "Pagado";
                row.Cells["colAlumno"].Value = cuota.AlumnoNombre ?? _alumnoSeleccionado?.Nombre ?? "";
                row.Cells["colPeriodo"].Value = $"{cuota.Mes} {cuota.Anio}";
                // Preferimos el nombre real del concepto (ej. "Reserva Matrícula 2027") por sobre
                // la categoría genérica interna (TipoCuota = "Matricula"): antes ambas mostraban
                // lo mismo acá y no se podía distinguir una matrícula normal de una reserva.
                row.Cells["colTipo"].Value = !string.IsNullOrWhiteSpace(cuota.NombreConcepto)
                    ? cuota.NombreConcepto
                    : (cuota.TipoCuota ?? "Curricular");
                row.Cells["colMontoFinal"].Value = cuota.MontoOriginal;
                row.Cells["colDescuento"].Value = cuota.Descuento;
                row.Cells["colPagado"].Value = cuota.MontoPagado;
                row.Cells["colSaldo"].Value = cuota.Saldo;
                row.Cells["colRecargo"].Value = cuota.Recargo;
                row.Cells["colVencimiento"].Value = cuota.FechaVencimiento.HasValue
                    ? cuota.FechaVencimiento.Value.ToString("dd/MM/yyyy") : "";
                row.Cells["colEstado"].Value = esBonificada100 ? "Bonificada 100%" : cuota.Estado;

                // Color de fila
                if (esBonificada100)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(214, 234, 248);   // celeste: bonificada, no es deuda
                else if (cuota.Estado == "Pagado")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(212, 247, 223);   // verde
                else if (tieneRecargo)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 210);   // naranja: vencida con recargo
                else if (cuota.Estado == "Pago Parcial")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 249, 219);   // amarillo
                else
                    row.DefaultCellStyle.BackColor = Color.FromArgb(253, 237, 236);   // rojo claro: pendiente
            }

            gridCuotas.CellValueChanged += gridCuotas_CellValueChanged;
            RecalcularDesdeCheckboxes();
        }

        // =====================================================================
        // NÚCLEO DE LA LÓGICA: distribución del monto recibido
        // =====================================================================

        /// <summary>
        /// Recalcula cuántas cuotas cubre el monto ingresado y actualiza el resumen.
        /// NO modifica los checkboxes — eso lo hace DistribuirMontoEnCuotas().
        /// </summary>
        private void RecalcularDesdeCheckboxes()
        {
            decimal totalBase = 0;
            decimal totalRecargo = 0;

            foreach (DataGridViewRow row in gridCuotas.Rows)
            {
                if (!(row.Cells["colSeleccionar"].Value is bool chk && chk)) continue;
                if (!(row.Tag is Cuota cuota)) continue;

                // Usar el saldo con recargo si existe en la columna colRecargo
                if (row.Cells["colSaldo"].Value is decimal saldoConRec && saldoConRec > 0)
                {
                    totalBase += cuota.Saldo;
                    totalRecargo += cuota.Recargo;
                }
                else
                {
                    totalBase += cuota.Saldo;
                }
            }

            decimal totalConRecargo = totalBase;

            // Mostrar desglose si hay recargo en alguna cuota seleccionada
            if (totalRecargo > 0)
                lblTotalSeleccionado.Text =
                    $"Total: $ {totalConRecargo:N2}  " +
                    $"(cuota $ {totalBase:N2} + recargo $ {totalRecargo:N2})";
            else
                lblTotalSeleccionado.Text = $"Total seleccionado: $ {totalConRecargo:N2}";

            lblTotalSeleccionado.ForeColor = totalRecargo > 0
                ? Color.FromArgb(180, 60, 0)
                : Color.FromArgb(26, 82, 118);

            lblTotalDeuda.Text = $"$ {totalConRecargo:N2}";

            ActualizarCalculoPago(totalConRecargo, MontoRecibidoActual);
        }

        /// <summary>
        /// Dado el monto recibido, marca automáticamente las cuotas que quedan cubiertas
        /// (total o parcialmente) de más antigua a más nueva.
        /// Este método es el que implementa el caso de uso principal:
        /// "El padre trae $500.000, ¿qué cuotas cubre?"
        /// </summary>
        private void DistribuirMontoEnCuotas(decimal montoDisponible)
        {
            if (_actualizando) return;
            _actualizando = true;

            gridCuotas.CellValueChanged -= gridCuotas_CellValueChanged;

            decimal restante = montoDisponible;

            foreach (DataGridViewRow row in gridCuotas.Rows)
            {
                if (!(row.Tag is Cuota cuota)) continue;
                if (cuota.Estado == "Pagado")
                {
                    row.Cells["colSeleccionar"].Value = false;
                    continue;
                }

                if (restante > 0)
                {
                    row.Cells["colSeleccionar"].Value = true;
                    // Descontar el saldo total con recargo incluido (colRecargo es SOLO
                    // el monto del recargo, no el total; hay que sumarlo al saldo base).
                    decimal recargoCuota = row.Cells["colRecargo"].Value is decimal conRec ? conRec : 0m;
                    decimal aDescontar = cuota.Saldo + recargoCuota;
                    restante -= aDescontar;
                }
                else
                {
                    // Ya no hay dinero disponible: desmarcar el resto
                    row.Cells["colSeleccionar"].Value = false;
                }
            }

            gridCuotas.CellValueChanged += gridCuotas_CellValueChanged;
            _actualizando = false;

            // Recalcular el resumen con los nuevos checkboxes
            RecalcularDesdeCheckboxes();
        }

        private void gridCuotas_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == gridCuotas.Columns["colSeleccionar"]?.Index && !_actualizando)
                RecalcularDesdeCheckboxes();
        }

        private void gridCuotas_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            if (gridCuotas.IsCurrentCellDirty)
                gridCuotas.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        /// <summary>
        /// Doble clic en una fila de la grilla → ofrece ajustar el saldo de esa cuota.
        /// Útil para la carga inicial cuando el alumno ya tiene pagos previos al sistema.
        /// </summary>
        private void gridCuotas_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = gridCuotas.Rows[e.RowIndex];
            if (!(row.Tag is Cuota cuota)) return;
            if (cuota.Estado == "Pagado") return;

            MostrarDialogoAjusteSaldo(cuota);
        }

        private void MostrarDialogoAjusteSaldo(Cuota cuota)
        {
            string nombreAlumno = cuota.AlumnoNombre ?? _alumnoSeleccionado?.Nombre ?? "";

            using (var dlg = new Form())
            {
                dlg.Text = "Ajustar saldo de cuota";
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.ClientSize = new System.Drawing.Size(400, 320);
                dlg.BackColor = System.Drawing.Color.White;

                // Info de la cuota
                var lblInfo = new Label
                {
                    Text = $"Alumno: {nombreAlumno}\n" +
                                $"Cuota:  {cuota.Mes} {cuota.Anio} ({cuota.TipoCuota})\n" +
                                $"Monto total: $ {cuota.MontoFinal:N2}\n" +
                                $"Ya pagado:   $ {cuota.MontoPagado:N2}\n" +
                                $"Saldo actual: $ {cuota.Saldo:N2}",
                    Location = new System.Drawing.Point(16, 12),
                    AutoSize = false,
                    Size = new System.Drawing.Size(368, 95),
                    Font = new Font("Segoe UI", 9.5f),
                    ForeColor = System.Drawing.Color.FromArgb(44, 62, 80)
                };
                dlg.Controls.Add(lblInfo);

                // Separador
                var sep = new Label
                {
                    BorderStyle = BorderStyle.Fixed3D,
                    Location = new System.Drawing.Point(16, 112),
                    Size = new System.Drawing.Size(368, 2)
                };
                dlg.Controls.Add(sep);

                var lblNuevo = new Label
                {
                    Text = "¿Cuánto debe realmente el alumno?",
                    Location = new System.Drawing.Point(16, 122),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
                };
                dlg.Controls.Add(lblNuevo);

                var numSaldo = new NumericUpDown
                {
                    Location = new System.Drawing.Point(16, 148),
                    Size = new System.Drawing.Size(160, 26),
                    DecimalPlaces = 2,
                    Maximum = cuota.MontoFinal,
                    Minimum = 0,
                    Value = cuota.Saldo,
                    Increment = 100,
                    ThousandsSeparator = true,
                    Font = new Font("Segoe UI", 11f, FontStyle.Bold)
                };
                dlg.Controls.Add(numSaldo);

                var lblAyuda = new Label
                {
                    Text = "Si ya pagó todo → poner 0\nSi nunca pagó nada → dejar el valor actual",
                    Location = new System.Drawing.Point(190, 148),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = System.Drawing.Color.Gray
                };
                dlg.Controls.Add(lblAyuda);

                var sep2 = new Label
                {
                    BorderStyle = BorderStyle.Fixed3D,
                    Location = new System.Drawing.Point(16, 184),
                    Size = new System.Drawing.Size(368, 2)
                };
                dlg.Controls.Add(sep2);

                var lblRecargo = new Label
                {
                    Text = "Recargo por mora de esta cuota",
                    Location = new System.Drawing.Point(16, 194),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold)
                };
                dlg.Controls.Add(lblRecargo);

                var numRecargo = new NumericUpDown
                {
                    Location = new System.Drawing.Point(16, 220),
                    Size = new System.Drawing.Size(160, 26),
                    DecimalPlaces = 2,
                    Maximum = 99999999,
                    Minimum = 0,
                    Value = cuota.Recargo,
                    Increment = 100,
                    ThousandsSeparator = true,
                    Font = new Font("Segoe UI", 11f, FontStyle.Bold),
                    ForeColor = System.Drawing.Color.FromArgb(180, 60, 0)
                };
                dlg.Controls.Add(numRecargo);

                var lblAyudaRecargo = new Label
                {
                    Text = "Poné 0 si el pago real ya se hizo\na tiempo (ej: carga de pagos\natrasados de meses anteriores).",
                    Location = new System.Drawing.Point(190, 220),
                    AutoSize = true,
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = System.Drawing.Color.Gray
                };
                dlg.Controls.Add(lblAyudaRecargo);


                var btnOk = new Button
                {
                    Text = "✔ Confirmar",
                    Location = new System.Drawing.Point(16, 276),
                    Size = new System.Drawing.Size(130, 32),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = System.Drawing.Color.FromArgb(39, 174, 96),
                    ForeColor = System.Drawing.Color.White,
                    Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                    Cursor = Cursors.Hand,
                    DialogResult = DialogResult.OK
                };
                btnOk.FlatAppearance.BorderSize = 0;
                dlg.Controls.Add(btnOk);
                dlg.AcceptButton = btnOk;

                var btnCancel = new Button
                {
                    Text = "Cancelar",
                    Location = new System.Drawing.Point(158, 276),
                    Size = new System.Drawing.Size(90, 32),
                    FlatStyle = FlatStyle.Flat,
                    DialogResult = DialogResult.Cancel,
                    Cursor = Cursors.Hand
                };
                dlg.Controls.Add(btnCancel);
                dlg.CancelButton = btnCancel;

                if (dlg.ShowDialog(this) != DialogResult.OK) return;

                decimal nuevoSaldo = numSaldo.Value;
                decimal nuevoRecargo = numRecargo.Value;
                bool cambioSaldo = nuevoSaldo != cuota.Saldo;
                bool cambioRecargo = nuevoRecargo != cuota.Recargo;
                if (!cambioSaldo && !cambioRecargo) return;

                try
                {
                    if (cambioRecargo)
                        _cuotaService.AjustarRecargoCuota(cuota.Id, nuevoRecargo);
                    if (cambioSaldo)
                        _cuotaService.AjustarSaldoCuota(cuota.Id, nuevoSaldo);

                    var partes = new List<string>();
                    if (cambioSaldo)
                        partes.Add($"Saldo: $ {cuota.Saldo:N2} → $ {nuevoSaldo:N2}");
                    if (cambioRecargo)
                        partes.Add($"Recargo: $ {cuota.Recargo:N2} → $ {nuevoRecargo:N2}");

                    MostrarMensaje("✓ Cuota actualizada.\n" + string.Join("\n", partes));
                    CargarCuotasAlumno(); // refrescar la grilla
                }
                catch (Exception ex)
                {
                    MostrarMensaje("Error al ajustar el saldo: " + ex.Message, true);
                }
            }
        }

        // =====================================================================
        // EVENTOS DEL PANEL DE COBRO
        // =====================================================================

        /// <summary>
        /// Cuando el operador cambia el monto recibido, actualizamos el resumen.
        /// NO distribuimos automáticamente para no interferir si el operador
        /// prefiere marcar las cuotas a mano.
        /// </summary>
        private void numMonto_ValueChanged(object sender, EventArgs e)
        {
            if (_actualizando) return;
            RecalcularDesdeCheckboxes();
        }

        /// <summary>
        /// Botón "Distribuir": aplica el monto al grid marcando cuotas automáticamente.
        /// </summary>
        private void btnDistribuirMonto_Click(object sender, EventArgs e)
        {
            if (MontoRecibidoActual <= 0)
            {
                MostrarMensaje("Ingrese un monto mayor a cero para distribuir.", true);
                return;
            }
            DistribuirMontoEnCuotas(MontoRecibidoActual);
        }

        private void chkMostrarTodas_CheckedChanged(object sender, EventArgs e)
        {
            CargarCuotasAlumno();
        }

        // =====================================================================
        // IMPLEMENTACIÓN IPagosView
        // =====================================================================

        public List<Cuota> ObtenerCuotasSeleccionadas()
        {
            var seleccionadas = new List<Cuota>();
            foreach (DataGridViewRow row in gridCuotas.Rows)
            {
                if (row.Cells["colSeleccionar"].Value is bool marcada && marcada && row.Tag is Cuota cuota)
                    seleccionadas.Add(cuota);
            }
            return seleccionadas;
        }

        public void MostrarResumenPago(decimal totalDeuda, decimal montoRecibido)
        {
            lblTotalDeuda.Text = $"$ {totalDeuda:N2}";
            ActualizarCalculoPago(totalDeuda, montoRecibido);
        }

        public void ActualizarCalculoPago(decimal totalDeuda, decimal montoRecibido)
        {
            decimal vuelto = montoRecibido > totalDeuda ? montoRecibido - totalDeuda : 0;
            decimal deuda = montoRecibido < totalDeuda ? totalDeuda - montoRecibido : 0;

            lblVuelto.Text = vuelto > 0 ? $"$ {vuelto:N2}" : "$ 0,00";
            lblDeuda.Text = deuda > 0 ? $"$ {deuda:N2}" : "$ 0,00";

            lblVuelto.ForeColor = vuelto > 0 ? Color.FromArgb(39, 174, 96) : Color.Gray;
            lblDeuda.ForeColor = deuda > 0 ? Color.FromArgb(231, 76, 60) : Color.Gray;
        }

    }
}
