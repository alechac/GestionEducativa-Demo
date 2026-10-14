using GestionEducativa.Models;
using GestionEducativa.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    /// <summary>
    /// Registro de gastos/egresos de la institución, con el comprobante de respaldo
    /// cargado a mano. Pensado para, junto con Cobros, poder armar un cierre de caja
    /// completo (ingresos y egresos).
    /// </summary>
    public class GastosForm : Form
    {
        private readonly GastoService _gastoService = new GastoService();

        public int SeccionActual { get; set; }

        // Filtros / listado
        private DateTimePicker _dtDesde, _dtHasta;
        private ComboBox _cmbFiltroTipo;
        private TextBox _txtBuscar;
        private Button _btnBuscar;
        private Button _btnNuevo;
        private DataGridView _gridGastos;
        private Label _lblTotalFiltrado;

        // Ficha del gasto
        private Label _lblFicha;
        private DateTimePicker _dtFecha;
        private ComboBox _cmbTipoGasto;
        private TextBox _txtDetalle;
        private NumericUpDown _numMonto;
        private ComboBox _cmbFormaPago;
        private ComboBox _cmbComprobanteTipo;
        private TextBox _txtComprobanteNumero;
        private TextBox _txtComprobanteProveedor;
        private TextBox _txtComprobanteCuit;
        private DateTimePicker _dtComprobanteFecha;
        private TextBox _txtObservaciones;
        private Button _btnGuardar, _btnEliminar, _btnCancelar;

        private Label _lblEstado;

        private Gasto _gastoActual; // null = alta nueva

        private static readonly string[] TiposGastoSugeridos = {
            "Sueldos y Cargas Sociales", "Servicios (Luz/Agua/Gas/Internet)", "Alquiler",
            "Mantenimiento y Reparaciones", "Insumos y Materiales", "Impuestos y Tasas",
            "Seguros", "Proveedores / Compras", "Honorarios Profesionales", "Otro"
        };

        public GastosForm(int seccion)
        {
            SeccionActual = seccion;
            Text = "Gastos";
            Width = 1040;
            Height = 900;
            AutoScroll = true;
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F);

            Construir();
            BuscarGastos();
            HabilitarFicha(false);
        }

        public void CambiarSeccion(int nueva)
        {
            SeccionActual = nueva;
            HabilitarFicha(false);
            BuscarGastos();
        }

        private void Construir()
        {
            var lblTitulo = new Label
            {
                Text = "💸 Gastos",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 60, 117),
                Location = new Point(16, 12),
                AutoSize = true
            };
            Controls.Add(lblTitulo);

            // ── Filtros ──────────────────────────────────────────────────────────
            AgregarEtiqueta("Desde:", 16, 50);
            _dtDesde = new DateTimePicker { Location = new Point(70, 47), Size = new Size(120, 26), Format = DateTimePickerFormat.Short, Value = DateTime.Now.AddDays(-30) };
            Controls.Add(_dtDesde);

            AgregarEtiqueta("Hasta:", 200, 50);
            _dtHasta = new DateTimePicker { Location = new Point(250, 47), Size = new Size(120, 26), Format = DateTimePickerFormat.Short, Value = DateTime.Now };
            Controls.Add(_dtHasta);

            AgregarEtiqueta("Tipo:", 384, 50);
            _cmbFiltroTipo = new ComboBox { Location = new Point(420, 47), Size = new Size(200, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            Controls.Add(_cmbFiltroTipo);

            _txtBuscar = new TextBox { Location = new Point(630, 47), Size = new Size(180, 26) };
            _txtBuscar.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BuscarGastos(); } };
            Controls.Add(_txtBuscar);

            _btnBuscar = new Button
            {
                Text = "🔍 Buscar",
                Location = new Point(820, 46),
                Size = new Size(95, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White
            };
            _btnBuscar.FlatAppearance.BorderSize = 0;
            _btnBuscar.Click += (s, e) => BuscarGastos();
            Controls.Add(_btnBuscar);

            _btnNuevo = new Button
            {
                Text = "➕ Nuevo gasto",
                Location = new Point(820, 80),
                Size = new Size(140, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White
            };
            _btnNuevo.FlatAppearance.BorderSize = 0;
            _btnNuevo.Click += (s, e) => IniciarAltaNueva();
            Controls.Add(_btnNuevo);

            // ── Grilla ───────────────────────────────────────────────────────────
            _gridGastos = new DataGridView
            {
                Location = new Point(16, 118),
                Size = new Size(944, 220),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            _gridGastos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFecha", HeaderText = "Fecha", Width = 80 });
            _gridGastos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTipo", HeaderText = "Tipo", Width = 160 });
            _gridGastos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDetalle", HeaderText = "Detalle", Width = 260 });
            _gridGastos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colProveedor", HeaderText = "Proveedor", Width = 150 });
            _gridGastos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colComprobante", HeaderText = "Comprobante", Width = 130 });
            _gridGastos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFormaPago", HeaderText = "Forma de Pago", Width = 110 });
            _gridGastos.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMonto", HeaderText = "Monto", Width = 100, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } });
            _gridGastos.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) CargarFicha((Gasto)_gridGastos.Rows[e.RowIndex].Tag); };
            Controls.Add(_gridGastos);

            _lblTotalFiltrado = new Label
            {
                Location = new Point(16, 344),
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(192, 57, 43)
            };
            Controls.Add(_lblTotalFiltrado);

            // ── Ficha del gasto ──────────────────────────────────────────────────
            _lblFicha = new Label
            {
                Text = "Nuevo gasto",
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                Location = new Point(16, 376),
                AutoSize = true
            };
            Controls.Add(_lblFicha);

            AgregarEtiqueta("Fecha del gasto:", 16, 406);
            _dtFecha = new DateTimePicker { Location = new Point(16, 426), Size = new Size(160, 26), Format = DateTimePickerFormat.Short };
            Controls.Add(_dtFecha);

            AgregarEtiqueta("Tipo de gasto:", 200, 406);
            _cmbTipoGasto = new ComboBox { Location = new Point(200, 426), Size = new Size(280, 26), DropDownStyle = ComboBoxStyle.DropDown };
            foreach (var t in TiposGastoSugeridos) _cmbTipoGasto.Items.Add(t);
            Controls.Add(_cmbTipoGasto);

            AgregarEtiqueta("Monto:", 500, 406);
            _numMonto = new NumericUpDown { Location = new Point(500, 426), Size = new Size(140, 26), Maximum = 999999999, DecimalPlaces = 2, ThousandsSeparator = true };
            Controls.Add(_numMonto);

            AgregarEtiqueta("Forma de pago:", 660, 406);
            _cmbFormaPago = new ComboBox { Location = new Point(660, 426), Size = new Size(160, 26), DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbFormaPago.Items.AddRange(new object[] { "Efectivo", "Transferencia", "Cheque" });
            Controls.Add(_cmbFormaPago);

            AgregarEtiqueta("Especificación / detalle del gasto:", 16, 462);
            _txtDetalle = new TextBox { Location = new Point(16, 482), Size = new Size(804, 26) };
            Controls.Add(_txtDetalle);

            // ── Comprobante (cargado a mano) ─────────────────────────────────────
            var lblComprobante = new Label
            {
                Text = "Comprobante asociado",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(16, 520),
                AutoSize = true
            };
            Controls.Add(lblComprobante);

            AgregarEtiqueta("Tipo:", 16, 546);
            _cmbComprobanteTipo = new ComboBox { Location = new Point(16, 566), Size = new Size(160, 26), DropDownStyle = ComboBoxStyle.DropDown };
            _cmbComprobanteTipo.Items.AddRange(new object[] { "Factura A", "Factura B", "Factura C", "Recibo", "Ticket", "Nota de Débito", "Otro" });
            Controls.Add(_cmbComprobanteTipo);

            AgregarEtiqueta("Número:", 190, 546);
            _txtComprobanteNumero = new TextBox { Location = new Point(190, 566), Size = new Size(150, 26) };
            Controls.Add(_txtComprobanteNumero);

            AgregarEtiqueta("Fecha del comprobante:", 356, 546);
            _dtComprobanteFecha = new DateTimePicker { Location = new Point(356, 566), Size = new Size(160, 26), Format = DateTimePickerFormat.Short };
            Controls.Add(_dtComprobanteFecha);

            AgregarEtiqueta("Proveedor / Razón social:", 16, 602);
            _txtComprobanteProveedor = new TextBox { Location = new Point(16, 622), Size = new Size(340, 26) };
            Controls.Add(_txtComprobanteProveedor);

            AgregarEtiqueta("CUIT del proveedor:", 372, 602);
            _txtComprobanteCuit = new TextBox { Location = new Point(372, 622), Size = new Size(160, 26) };
            Controls.Add(_txtComprobanteCuit);

            AgregarEtiqueta("Observaciones:", 16, 658);
            _txtObservaciones = new TextBox { Location = new Point(16, 678), Size = new Size(804, 26) };
            Controls.Add(_txtObservaciones);

            // ── Botones ──────────────────────────────────────────────────────────
            _btnGuardar = new Button
            {
                Text = "💾 Guardar",
                Location = new Point(16, 716),
                Size = new Size(120, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 60, 117),
                ForeColor = Color.White
            };
            _btnGuardar.FlatAppearance.BorderSize = 0;
            _btnGuardar.Click += (s, e) => Guardar();
            Controls.Add(_btnGuardar);

            _btnEliminar = new Button
            {
                Text = "🗑 Eliminar",
                Location = new Point(146, 716),
                Size = new Size(120, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White
            };
            _btnEliminar.FlatAppearance.BorderSize = 0;
            _btnEliminar.Click += (s, e) => Eliminar();
            Controls.Add(_btnEliminar);

            _btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(276, 716),
                Size = new Size(100, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(230, 230, 230)
            };
            _btnCancelar.FlatAppearance.BorderSize = 0;
            _btnCancelar.Click += (s, e) => { HabilitarFicha(false); BuscarGastos(); };
            Controls.Add(_btnCancelar);

            _lblEstado = new Label
            {
                Location = new Point(16, 760),
                Size = new Size(944, 40),
                Font = new Font("Segoe UI", 9F)
            };
            Controls.Add(_lblEstado);
        }

        private void AgregarEtiqueta(string texto, int x, int y)
        {
            Controls.Add(new Label { Text = texto, Location = new Point(x, y), AutoSize = true });
        }

        // ══════════════════════════════════════════════════════════════════════

        private void CargarComboFiltroTipo()
        {
            string seleccionado = _cmbFiltroTipo.SelectedItem?.ToString();
            _cmbFiltroTipo.Items.Clear();
            _cmbFiltroTipo.Items.Add("(Todos)");
            try
            {
                foreach (var t in _gastoService.ObtenerTiposExistentes())
                    if (!_cmbFiltroTipo.Items.Contains(t)) _cmbFiltroTipo.Items.Add(t);
            }
            catch { /* si falla, se queda solo con "(Todos)" */ }

            int idx = seleccionado != null ? _cmbFiltroTipo.Items.IndexOf(seleccionado) : 0;
            _cmbFiltroTipo.SelectedIndex = idx >= 0 ? idx : 0;
        }

        private void BuscarGastos()
        {
            try
            {
                CargarComboFiltroTipo();
                string tipo = _cmbFiltroTipo.SelectedIndex > 0 ? _cmbFiltroTipo.SelectedItem.ToString() : null;

                var gastos = _gastoService.Buscar(_dtDesde.Value, _dtHasta.Value, SeccionActual, tipo, _txtBuscar.Text);
                _gridGastos.Rows.Clear();

                decimal total = 0;
                foreach (var g in gastos)
                {
                    int idx = _gridGastos.Rows.Add(
                        g.Fecha.ToString("dd/MM/yyyy"),
                        g.TipoGasto,
                        g.Detalle,
                        g.ComprobanteProveedor,
                        string.IsNullOrEmpty(g.ComprobanteTipo) ? "" : $"{g.ComprobanteTipo} {g.ComprobanteNumero}",
                        g.FormaPago,
                        g.Monto
                    );
                    _gridGastos.Rows[idx].Tag = g;
                    total += g.Monto;
                }

                _lblTotalFiltrado.Text = $"Total del período filtrado: $ {total:N2}  ({gastos.Count} gasto(s))";
                MostrarEstado($"{gastos.Count} gasto(s) encontrado(s).", false);
            }
            catch (Exception ex)
            {
                MostrarEstado("Error al buscar gastos: " + ex.Message, true);
            }
        }

        private void IniciarAltaNueva()
        {
            _gastoActual = null;
            _dtFecha.Value = DateTime.Now;
            _cmbTipoGasto.Text = "";
            _txtDetalle.Clear();
            _numMonto.Value = 0;
            _cmbFormaPago.SelectedIndex = 0;
            _cmbComprobanteTipo.Text = "";
            _txtComprobanteNumero.Clear();
            _txtComprobanteProveedor.Clear();
            _txtComprobanteCuit.Clear();
            _dtComprobanteFecha.Value = DateTime.Now;
            _txtObservaciones.Clear();
            _lblFicha.Text = "Nuevo gasto";
            HabilitarFicha(true);
            _cmbTipoGasto.Focus();
        }

        private void CargarFicha(Gasto g)
        {
            _gastoActual = g;
            _dtFecha.Value = g.Fecha;
            _cmbTipoGasto.Text = g.TipoGasto;
            _txtDetalle.Text = g.Detalle;
            _numMonto.Value = g.Monto;
            _cmbFormaPago.SelectedItem = g.FormaPago;
            if (_cmbFormaPago.SelectedIndex < 0) _cmbFormaPago.SelectedIndex = 0;
            _cmbComprobanteTipo.Text = g.ComprobanteTipo ?? "";
            _txtComprobanteNumero.Text = g.ComprobanteNumero ?? "";
            _txtComprobanteProveedor.Text = g.ComprobanteProveedor ?? "";
            _txtComprobanteCuit.Text = g.ComprobanteCuitProveedor ?? "";
            _dtComprobanteFecha.Value = g.ComprobanteFecha ?? g.Fecha;
            _txtObservaciones.Text = g.Observaciones ?? "";
            _lblFicha.Text = $"Editando gasto del {g.Fecha:dd/MM/yyyy}";
            HabilitarFicha(true);
        }

        private void HabilitarFicha(bool habilitar)
        {
            foreach (var c in new Control[] {
                _dtFecha, _cmbTipoGasto, _txtDetalle, _numMonto, _cmbFormaPago,
                _cmbComprobanteTipo, _txtComprobanteNumero, _txtComprobanteProveedor,
                _txtComprobanteCuit, _dtComprobanteFecha, _txtObservaciones,
                _btnGuardar, _btnCancelar
            }) c.Enabled = habilitar;

            _btnEliminar.Enabled = habilitar && _gastoActual != null;

            if (!habilitar)
            {
                _lblFicha.Text = "Elegí un gasto de la lista, o registrá uno nuevo";
            }
        }

        private void Guardar()
        {
            if (string.IsNullOrWhiteSpace(_cmbTipoGasto.Text))
            {
                MostrarEstado("Elegí o escribí el tipo de gasto.", true);
                return;
            }
            if (string.IsNullOrWhiteSpace(_txtDetalle.Text))
            {
                MostrarEstado("La especificación/detalle del gasto es obligatoria.", true);
                return;
            }
            if (_numMonto.Value <= 0)
            {
                MostrarEstado("El monto tiene que ser mayor a cero.", true);
                return;
            }

            var gasto = new Gasto
            {
                Id = _gastoActual?.Id ?? 0,
                Fecha = _dtFecha.Value.Date,
                TipoGasto = _cmbTipoGasto.Text.Trim(),
                Detalle = _txtDetalle.Text.Trim(),
                Monto = _numMonto.Value,
                FormaPago = _cmbFormaPago.SelectedItem?.ToString() ?? "Efectivo",
                Seccion = SeccionActual,
                ComprobanteTipo = string.IsNullOrWhiteSpace(_cmbComprobanteTipo.Text) ? null : _cmbComprobanteTipo.Text.Trim(),
                ComprobanteNumero = string.IsNullOrWhiteSpace(_txtComprobanteNumero.Text) ? null : _txtComprobanteNumero.Text.Trim(),
                ComprobanteProveedor = string.IsNullOrWhiteSpace(_txtComprobanteProveedor.Text) ? null : _txtComprobanteProveedor.Text.Trim(),
                ComprobanteCuitProveedor = string.IsNullOrWhiteSpace(_txtComprobanteCuit.Text) ? null : _txtComprobanteCuit.Text.Trim(),
                ComprobanteFecha = string.IsNullOrWhiteSpace(_cmbComprobanteTipo.Text) ? (DateTime?)null : _dtComprobanteFecha.Value.Date,
                Observaciones = string.IsNullOrWhiteSpace(_txtObservaciones.Text) ? null : _txtObservaciones.Text.Trim()
            };

            try
            {
                if (_gastoActual == null)
                {
                    gasto.Id = _gastoService.Insertar(gasto);
                    MostrarEstado($"✓ Gasto registrado correctamente (${gasto.Monto:N2}).", false);
                }
                else
                {
                    _gastoService.Actualizar(gasto);
                    MostrarEstado("✓ Gasto actualizado correctamente.", false);
                }

                HabilitarFicha(false);
                BuscarGastos();
            }
            catch (Exception ex)
            {
                MostrarEstado("Error al guardar el gasto: " + ex.Message, true);
            }
        }

        private void Eliminar()
        {
            if (_gastoActual == null) return;

            var confirmacion = MessageBox.Show(
                $"¿Eliminar el gasto de \"{_gastoActual.Detalle}\" por $ {_gastoActual.Monto:N2}?",
                "Confirmar eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirmacion != DialogResult.Yes) return;

            try
            {
                _gastoService.EliminarLogico(_gastoActual.Id);
                MostrarEstado("✓ Gasto eliminado.", false);
                HabilitarFicha(false);
                BuscarGastos();
            }
            catch (Exception ex)
            {
                MostrarEstado("Error al eliminar: " + ex.Message, true);
            }
        }

        private void MostrarEstado(string mensaje, bool esError)
        {
            _lblEstado.Text = mensaje;
            _lblEstado.ForeColor = esError ? Color.FromArgb(192, 57, 43) : Color.FromArgb(39, 174, 96);
        }
    }
}
