using GestionEducativa.Models;
using GestionEducativa.Services;
using GestionEducativa.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    /// <summary>
    /// Permite buscar una factura ya emitida y generar una Nota de Crédito C que la
    /// acredite total o parcialmente. Sigue el mismo esquema de seguridad que los cobros:
    /// primero se simula (sin tocar la base), se pide el CAE a ARCA, y solo si lo aprueba
    /// se confirma en la base de datos — así nunca queda una "NC fantasma" sin CAE.
    /// </summary>
    public class NotaCreditoForm : Form
    {
        private readonly PagoService _pagoService = new PagoService();
        private readonly PdfGenerator _pdfGen = new PdfGenerator();
        private readonly PadreService _padreService = new PadreService();
        private readonly EmailService _emailService = new EmailService();

        private TextBox _txtFiltro;
        private Button _btnBuscar;
        private DataGridView _gridFacturas;

        private Label _lblFacturaSeleccionada;
        private DataGridView _gridItems;
        private Button _btnMarcarTodo;
        private CheckBox _chkEnviarPorMail;
        private TextBox _txtMotivo;
        private Button _btnEmitir;
        private Label _lblEstado;

        private Pago _facturaSeleccionada;

        public NotaCreditoForm()
        {
            Text = "Notas de Crédito";
            Width = 1000;
            Height = 750;
            AutoScroll = true; // si en una pantalla chica no entra todo, aparece scroll en vez de tapar controles
            StartPosition = FormStartPosition.CenterScreen;
            Font = new Font("Segoe UI", 9F);

            Construir();
        }

        private void Construir()
        {
            var lblTitulo = new Label
            {
                Text = "🧾 Emitir Nota de Crédito",
                Font = new Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 60, 117),
                Location = new Point(16, 12),
                AutoSize = true
            };
            Controls.Add(lblTitulo);

            var lblSub = new Label
            {
                Text = "Buscá la factura que el padre necesita que se acredite (por N° de comprobante, DNI/CUIT o nombre) y elegí qué ítems acreditar.",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.Gray,
                Location = new Point(16, 42),
                AutoSize = true
            };
            Controls.Add(lblSub);

            // ── Búsqueda ─────────────────────────────────────────────────────────
            var lblFiltro = new Label { Text = "Buscar:", Location = new Point(16, 74), AutoSize = true };
            Controls.Add(lblFiltro);

            _txtFiltro = new TextBox { Location = new Point(75, 71), Size = new Size(300, 26) };
            _txtFiltro.KeyDown += (s, e) => { if (e.KeyCode == Keys.Enter) { e.SuppressKeyPress = true; BuscarFacturas(); } };
            Controls.Add(_txtFiltro);

            _btnBuscar = new Button
            {
                Text = "🔍 Buscar",
                Location = new Point(385, 70),
                Size = new Size(100, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White
            };
            _btnBuscar.FlatAppearance.BorderSize = 0;
            _btnBuscar.Click += (s, e) => BuscarFacturas();
            Controls.Add(_btnBuscar);

            // ── Grilla de facturas encontradas ──────────────────────────────────
            _gridFacturas = new DataGridView
            {
                Location = new Point(16, 108),
                Size = new Size(950, 180),
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill,
                RowHeadersVisible = false
            };
            _gridFacturas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colFecha", HeaderText = "Fecha", Width = 90 });
            _gridFacturas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colComprobante", HeaderText = "Comprobante", Width = 110 });
            _gridFacturas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPagador", HeaderText = "Pagador", Width = 200 });
            _gridFacturas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDocumento", HeaderText = "Documento", Width = 100 });
            _gridFacturas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colTotal", HeaderText = "Total", Width = 90, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } });
            _gridFacturas.Columns.Add(new DataGridViewTextBoxColumn { Name = "colNc", HeaderText = "NC previas", Width = 80 });
            _gridFacturas.CellDoubleClick += (s, e) => { if (e.RowIndex >= 0) SeleccionarFacturaEnFila(e.RowIndex); };
            Controls.Add(_gridFacturas);

            var btnSeleccionar = new Button
            {
                Text = "↓ Usar esta factura",
                Location = new Point(16, 294),
                Size = new Size(160, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(93, 173, 226),
                ForeColor = Color.White
            };
            btnSeleccionar.FlatAppearance.BorderSize = 0;
            btnSeleccionar.Click += (s, e) =>
            {
                if (_gridFacturas.SelectedRows.Count > 0)
                    SeleccionarFacturaEnFila(_gridFacturas.SelectedRows[0].Index);
                else
                    MessageBox.Show("Seleccioná primero una factura de la lista.", "Notas de Crédito", MessageBoxButtons.OK, MessageBoxIcon.Information);
            };
            Controls.Add(btnSeleccionar);

            // ── Factura seleccionada + ítems a acreditar ────────────────────────
            _lblFacturaSeleccionada = new Label
            {
                Text = "Ninguna factura seleccionada todavía.",
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Location = new Point(16, 332),
                AutoSize = true
            };
            Controls.Add(_lblFacturaSeleccionada);

            _gridItems = new DataGridView
            {
                Location = new Point(16, 360),
                Size = new Size(950, 200),
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            var colCheck = new DataGridViewCheckBoxColumn { Name = "colAcreditar", HeaderText = "Acreditar", Width = 70 };
            _gridItems.Columns.Add(colCheck);
            _gridItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAlumno", HeaderText = "Alumno", Width = 150, ReadOnly = true });
            _gridItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "colConcepto", HeaderText = "Concepto / Período", Width = 220, ReadOnly = true });
            _gridItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPagado", HeaderText = "Pagado en la factura", Width = 130, ReadOnly = true, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight } });
            _gridItems.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMonto", HeaderText = "Monto a acreditar", Width = 130, DefaultCellStyle = new DataGridViewCellStyle { Format = "N2", Alignment = DataGridViewContentAlignment.MiddleRight, BackColor = Color.LightYellow } });
            Controls.Add(_gridItems);

            _btnMarcarTodo = new Button
            {
                Text = "✓ Nota de crédito TOTAL (marcar todo)",
                Location = new Point(16, 566),
                Size = new Size(260, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(230, 230, 230)
            };
            _btnMarcarTodo.FlatAppearance.BorderSize = 0;
            _btnMarcarTodo.Click += (s, e) => MarcarTodosLosItems();
            Controls.Add(_btnMarcarTodo);

            var lblMotivo = new Label { Text = "Motivo de la Nota de Crédito (obligatorio):", Location = new Point(16, 604), AutoSize = true };
            Controls.Add(lblMotivo);

            _txtMotivo = new TextBox { Location = new Point(16, 626), Size = new Size(950, 26) };
            Controls.Add(_txtMotivo);

            _chkEnviarPorMail = new CheckBox
            {
                Text = "📧 Enviar comprobante por mail al padre/tutor",
                AutoSize = true,
                ForeColor = Color.FromArgb(26, 82, 118),
                Location = new Point(16, 660),
                Checked = true // por defecto se envía, igual que en Cobros — se puede destildar
            };
            Controls.Add(_chkEnviarPorMail);

            _btnEmitir = new Button
            {
                Text = "🧾 Emitir Nota de Crédito",
                Location = new Point(16, 692),
                Size = new Size(220, 38),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(192, 57, 43),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold)
            };
            _btnEmitir.FlatAppearance.BorderSize = 0;
            _btnEmitir.Click += async (s, e) => await BtnEmitir_ClickAsync();
            Controls.Add(_btnEmitir);

            _lblEstado = new Label
            {
                Location = new Point(250, 700),
                Size = new Size(700, 40),
                Font = new Font("Segoe UI", 9F)
            };
            Controls.Add(_lblEstado);
        }

        // ══════════════════════════════════════════════════════════════════════

        private void BuscarFacturas()
        {
            try
            {
                var facturas = _pagoService.BuscarFacturasParaAcreditar(_txtFiltro.Text);
                _gridFacturas.Rows.Clear();

                foreach (var f in facturas)
                {
                    int idx = _gridFacturas.Rows.Add(
                        f.FechaPago.ToString("dd/MM/yyyy"),
                        f.NroComprobanteArca,
                        f.NombrePagador,
                        f.DocumentoPagador,
                        f.TotalAplicado,
                        f.CantidadNotasCredito > 0 ? $"{f.CantidadNotasCredito} ya emitida(s)" : ""
                    );
                    _gridFacturas.Rows[idx].Tag = f;
                }

                MostrarEstado(facturas.Count == 0
                    ? "No se encontraron facturas con ese criterio de búsqueda."
                    : $"{facturas.Count} factura(s) encontrada(s).", false);
            }
            catch (Exception ex)
            {
                MostrarEstado("Error al buscar: " + ex.Message, true);
            }
        }

        private void SeleccionarFacturaEnFila(int filaIndex)
        {
            var resumen = (Pago)_gridFacturas.Rows[filaIndex].Tag;

            try
            {
                _facturaSeleccionada = _pagoService.ObtenerPagoCompleto(resumen.Id);
            }
            catch (Exception ex)
            {
                MostrarEstado("Error al abrir la factura: " + ex.Message, true);
                return;
            }

            _lblFacturaSeleccionada.Text =
                $"Factura seleccionada: {_facturaSeleccionada.NroComprobanteArca} — " +
                $"{_facturaSeleccionada.NombrePagador} — Total $ {_facturaSeleccionada.TotalAplicado:N2}" +
                (resumen.CantidadNotasCredito > 0 ? $"  ⚠ Ya tiene {resumen.CantidadNotasCredito} Nota(s) de Crédito emitida(s)" : "");
            _lblFacturaSeleccionada.ForeColor = resumen.CantidadNotasCredito > 0
                ? Color.FromArgb(192, 57, 43)
                : Color.FromArgb(39, 60, 117);

            _gridItems.Rows.Clear();
            foreach (var det in _facturaSeleccionada.Detalles)
            {
                int idx = _gridItems.Rows.Add(false, det.AlumnoNombre, det.PeriodoCuota, det.MontoAplicado, det.MontoAplicado);
                _gridItems.Rows[idx].Tag = det;
            }
        }

        private void MarcarTodosLosItems()
        {
            foreach (DataGridViewRow row in _gridItems.Rows)
            {
                row.Cells["colAcreditar"].Value = true;
                var det = (DetallePago)row.Tag;
                row.Cells["colMonto"].Value = det.MontoAplicado;
            }
        }

        /// <summary>
        /// Genera el comprobante y lo deja EN PDF (no en HTML) en disco. El HTML es un
        /// paso intermedio interno (lo necesita HtmlToPdfConverter como fuente), pero una
        /// <summary>
        /// Convierte un comprobante YA GENERADO (HTML persistente) a PDF, únicamente para
        /// adjuntarlo al mail — los padres no sabían abrir el .html directamente. Este PDF
        /// es descartable: se borra apenas se manda el mail. El HTML queda guardado para
        /// siempre como el comprobante "oficial" (liviano, ~30KB contra ~200KB del PDF).
        /// Si la conversión falla, se manda directamente el HTML como adjunto.
        /// </summary>
        private async Task<(string archivoParaAdjuntar, string archivoPdfTemporal)> PrepararAdjuntoParaMail(string archivoHtml)
        {
            try
            {
                string archivoPdf = await GestionEducativa.Utils.HtmlToPdfConverter.ConvertirArchivoAsync(archivoHtml);
                return (archivoPdf, archivoPdf);
            }
            catch (Exception exPdf)
            {
                MostrarEstado(
                    $"⚠ No se pudo convertir el comprobante a PDF para el mail, se enviará en HTML. Motivo: {exPdf.GetType().Name}: {exPdf.Message}",
                    true);
                return (archivoHtml, null);
            }
        }

        private async Task BtnEmitir_ClickAsync()
        {
            if (_facturaSeleccionada == null)
            {
                MostrarEstado("Elegí primero qué factura vas a acreditar.", true);
                return;
            }

            if (string.IsNullOrWhiteSpace(_txtMotivo.Text))
            {
                MostrarEstado("Ingresá el motivo de la Nota de Crédito (ej: \"Pago registrado por error\", \"Devolución a pedido del padre\").", true);
                return;
            }

            _gridItems.EndEdit();

            var items = new List<(DetallePago detalleOriginal, decimal montoACreditar)>();
            foreach (DataGridViewRow row in _gridItems.Rows)
            {
                bool marcado = row.Cells["colAcreditar"].Value is bool b && b;
                if (!marcado) continue;

                var det = (DetallePago)row.Tag;
                if (!decimal.TryParse(row.Cells["colMonto"].Value?.ToString(), out decimal monto) || monto <= 0)
                {
                    MostrarEstado($"El monto a acreditar de '{det.PeriodoCuota}' no es válido.", true);
                    return;
                }

                items.Add((det, monto));
            }

            if (items.Count == 0)
            {
                MostrarEstado("Marcá al menos un ítem para acreditar (columna 'Acreditar').", true);
                return;
            }

            decimal totalNc = items.Sum(i => i.montoACreditar);
            var confirmacion = MessageBox.Show(
                $"Se va a emitir una Nota de Crédito por $ {totalNc:N2}, referida a la factura " +
                $"{_facturaSeleccionada.NroComprobanteArca}.\n\n" +
                "Esto va a pedir el CAE a ARCA y, recién si lo aprueba, va a revertir el pago " +
                "de las cuotas correspondientes (van a volver a figurar como adeudadas por ese monto).\n\n" +
                "¿Confirmás la emisión?",
                "Confirmar Nota de Crédito", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirmacion != DialogResult.Yes) return;

            try
            {
                // ── Paso 1: simular (NO toca la base de datos) ──────────────────
                var ncSimulada = _pagoService.SimularNotaCredito(_facturaSeleccionada, items, _txtMotivo.Text.Trim());

                // ── Paso 2: pedir el CAE a ARCA ──────────────────────────────────
                var arcaForm = new ArcaEmisionForm(ncSimulada);
                arcaForm.ShowDialog();

                if (arcaForm.Resultado?.Aprobado != true)
                {
                    MostrarEstado("✗ ARCA no aprobó la Nota de Crédito (o no hay conexión). No se modificó ningún saldo.", true);
                    return;
                }

                // ── Paso 3: recién acá se confirma en la base de datos ───────────
                ncSimulada.CAE = arcaForm.Resultado.CAE;
                ncSimulada.CAEVencimiento = arcaForm.Resultado.CAEVencimiento;
                ncSimulada.NroComprobanteArca = arcaForm.Resultado.NroComprobante;

                var ncConfirmada = _pagoService.ConfirmarPagoConCae(ncSimulada);

                // Genera el comprobante EN HTML (el formato que se guarda para siempre) y lo
                // abre. Guardamos la ruta para reutilizarla al enviar el mail: ahí se
                // convierte a PDF solo para ese envío.
                string archivo = _pdfGen.GenerarComprobantePagoMultiple(ncConfirmada);
                Process.Start(new ProcessStartInfo(archivo) { UseShellExecute = true });

                MostrarEstado($"✓ Nota de Crédito {ncConfirmada.NroComprobanteArca} emitida por $ {ncConfirmada.TotalAplicado:N2}.", false);

                // Solo se envía por mail si quedó tildado el checkbox. Se bloquea toda la
                // interfaz mientras se convierte a PDF y se manda, para que no se pueda
                // seguir operando el sistema hasta que termine ese envío.
                if (_chkEnviarPorMail.Checked)
                {
                    BloqueoUI.Mostrar();
                    try { await EnviarPorMailSiCorresponde(ncConfirmada, archivo); }
                    finally { BloqueoUI.Ocultar(); }
                }

                // Limpiar para la próxima
                _facturaSeleccionada = null;
                _gridItems.Rows.Clear();
                _lblFacturaSeleccionada.Text = "Ninguna factura seleccionada todavía.";
                _chkEnviarPorMail.Checked = true;
                _txtMotivo.Clear();
                BuscarFacturas();
            }
            catch (Exception ex)
            {
                MostrarEstado("Error al emitir la Nota de Crédito: " + ex.Message, true);
            }
        }

        private async Task EnviarPorMailSiCorresponde(Pago nc, string archivoHtmlYaGenerado)
        {
            try
            {
                string doc = (nc.DocumentoPagador ?? "").Replace(".", "").Replace("-", "").Trim();
                if (string.IsNullOrEmpty(doc)) return;

                var padre = _padreService.ObtenerPorDocumento(doc);
                if (padre == null || string.IsNullOrWhiteSpace(padre.Email)) return;

                string archivoHtml = archivoHtmlYaGenerado ?? _pdfGen.GenerarComprobantePagoMultiple(nc);
                var (archivoParaAdjuntar, archivoPdfTemporal) = await PrepararAdjuntoParaMail(archivoHtml);

                bool enviado = _emailService.EnviarComprobante(nc, padre, archivoParaAdjuntar, out string error);

                // El PDF era descartable, solo para este envío — el HTML queda guardado igual.
                if (archivoPdfTemporal != null)
                {
                    try { File.Delete(archivoPdfTemporal); }
                    catch { /* no crítico */ }
                }

                _lblEstado.Text += enviado
                    ? $"  ·  ✓ Enviada por mail a {padre.Email}."
                    : $"  ·  ⚠ No se pudo enviar por mail: {error}";
            }
            catch
            {
                // No es crítico: la NC ya quedó emitida y guardada igual.
            }
        }

        private void MostrarEstado(string mensaje, bool esError)
        {
            _lblEstado.Text = mensaje;
            _lblEstado.ForeColor = esError ? Color.FromArgb(192, 57, 43) : Color.FromArgb(39, 174, 96);
        }
    }
}
