using GestionEducativa.Models;
using GestionEducativa.Services;
using iTextSharp.text;
using Org.BouncyCastle.Asn1.Crmf;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    public class ArcaEmisionForm : Form
    {
        private readonly Pago _pago;
        private readonly PagoService _pagoService;
        private readonly ArcaConfigService _configService;

        // Resultado público para que el presenter pueda leerlo
        public ResultadoCAE Resultado { get; private set; }

        // Controles
        private Label _lblEstado;
        private ProgressBar _progress;
        private RichTextBox _txtLog;
        private Button _btnCerrar;
        private Button _btnReintentar;
        private Panel _panelResultado;

        // Controles de entrada CAE
        private TextBox txtCAE;
        private TextBox txtNroComprobante;
        private DateTimePicker dtpVencCAE;
        private Button btnGuardarCAE;
        private Button btnOmitir;
        private Button btnAbrirArca;
        private Label lblEstado;

        public ArcaEmisionForm(Pago pago)
        {
            _pago = pago ?? throw new ArgumentNullException(nameof(pago));
            _pagoService = new PagoService();
            _configService = new ArcaConfigService();
            InicializarComponentes();
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  UI
        // ══════════════════════════════════════════════════════════════════════════

        private void InicializarComponentes()
        {
            Text = "Emisión Electrónica ARCA";
            Size = new Size(520, 480);
            StartPosition = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            BackColor = Color.White;

            // ── Encabezado ────────────────────────────────────────────────────────
            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 60,
                BackColor = Color.FromArgb(39, 60, 117)
            };
            var lblTitulo = new Label
            {
                Text = _pago.TipoComprobante == 13 ? "Emitiendo Nota de Crédito Electrónica" : "Emitiendo Factura Electrónica",
                Font = new System.Drawing.Font("Segoe UI", 13F, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = false,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter
            };
            header.Controls.Add(lblTitulo);
            Controls.Add(header);

            // ── Estado ────────────────────────────────────────────────────────────
            _lblEstado = new Label
            {
                Text = "Iniciando comunicación con ARCA...",
                Font = new System.Drawing.Font("Segoe UI", 10F),
                ForeColor = Color.FromArgb(60, 60, 60),
                AutoSize = false,
                Location = new Point(20, 80),
                Size = new Size(470, 24),
                TextAlign = ContentAlignment.MiddleLeft
            };
            Controls.Add(_lblEstado);

            // ── Barra de progreso ─────────────────────────────────────────────────
            _progress = new ProgressBar
            {
                Location = new Point(20, 110),
                Size = new Size(470, 16),
                Style = ProgressBarStyle.Marquee,
                MarqueeAnimationSpeed = 30
            };
            Controls.Add(_progress);

            // ── Log de pasos ──────────────────────────────────────────────────────
            _txtLog = new RichTextBox
            {
                Location = new Point(20, 140),
                Size = new Size(470, 170),
                ReadOnly = true,
                BackColor = Color.FromArgb(245, 245, 245),
                Font = new System.Drawing.Font("Consolas", 9F),
                BorderStyle = BorderStyle.FixedSingle,
                ScrollBars = RichTextBoxScrollBars.Vertical
            };
            Controls.Add(_txtLog);

            // ── Panel resultado ───────────────────────────────────────────────────
            _panelResultado = new Panel
            {
                Location = new Point(20, 320),
                Size = new Size(470, 40),
                Visible = false
            };
            Controls.Add(_panelResultado);

            // ── Botones ───────────────────────────────────────────────────────────
            _btnCerrar = new Button
            {
                Text = "Cerrar",
                Location = new Point(390, 365),
                Size = new Size(100, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 60, 117),
                ForeColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Cursor = Cursors.Hand,
                Enabled = false
            };
            _btnCerrar.FlatAppearance.BorderSize = 0;
            _btnCerrar.Click += (s, e) => Close();
            Controls.Add(_btnCerrar);

            _btnReintentar = new Button
            {
                Text = "Reintentar",
                Location = new Point(280, 365),
                Size = new Size(100, 32),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(230, 126, 34),
                ForeColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Cursor = Cursors.Hand,
                Visible = false
            };
            _btnReintentar.FlatAppearance.BorderSize = 0;
            _btnReintentar.Click += (s, e) => EjecutarEmision();
            Controls.Add(_btnReintentar);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            EjecutarEmision();
        }

        // ══════════════════════════════════════════════════════════════════════════
        //  Lógica de emisión
        // ══════════════════════════════════════════════════════════════════════════

        private void EjecutarEmision()
        {
            _btnCerrar.Enabled = false;
            _btnReintentar.Visible = false;
            _txtLog.Clear();
            _panelResultado.Visible = false;
            _progress.Style = ProgressBarStyle.Marquee;

            // Usar un hilo para no bloquear la UI durante las llamadas SOAP
            var worker = new System.ComponentModel.BackgroundWorker();
            worker.DoWork += (s, e) => e.Result = EjecutarPasosEmision();
            worker.RunWorkerCompleted += (s, e) =>
            {
                if (e.Error != null)
                    MostrarError("Error inesperado: " + e.Error.Message);
                else
                    MostrarResultadoFinal((ResultadoCAE)e.Result);
            };
            worker.RunWorkerAsync();
        }

        private ResultadoCAE EjecutarPasosEmision()
        {
            // Paso 1: cargar y validar configuración
            ActualizarEstado("Cargando configuración...");
            ArcaConfig config;
            ArcaConfigService configSvc = new ArcaConfigService();
            try
            {
                config = configSvc.Cargar();
                configSvc.Validar(config);
                Log("✓ Configuración cargada correctamente.", Color.FromArgb(39, 174, 96));
            }
            catch (Exception ex)
            {
                throw new Exception("Configuración ARCA incompleta:\n" + ex.Message);
            }

            var arcaSvc = new ArcaService(config);

            // Paso 2: verificar disponibilidad
            ActualizarEstado("Verificando disponibilidad de ARCA...");
            if (!arcaSvc.VerificarDisponibilidad())
            {
                Log("⚠ FEDummy respondió fuera de servicio. Se continúa de todas formas...", Color.FromArgb(230, 126, 34));
            }
            else
            {
                Log("✓ Servicios ARCA disponibles (FEDummy OK).", Color.FromArgb(39, 174, 96));
            }

            // Paso 3: obtener ticket de acceso (WSAA)
            ActualizarEstado("Autenticando con WSAA...");
            (string token, string sign) ticket;
            try
            {
                ticket = arcaSvc.ObtenerTicketAcceso();
                Log("✓ Ticket de acceso obtenido.", Color.FromArgb(39, 174, 96));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al autenticar con WSAA:\n" + ex.Message);
            }

            // Paso 4: solicitar CAE
            ActualizarEstado("Solicitando CAE a WSFEv1...");
            ResultadoCAE resultado;
            try
            {
                resultado = arcaSvc.SolicitarCAE(_pago, ticket);
                if (resultado.Aprobado)
                    Log($"✓ CAE obtenido: {resultado.CAE}", Color.FromArgb(39, 174, 96));
                else
                    Log($"✗ Rechazado: {resultado.MensajeError}", Color.FromArgb(192, 57, 43));
            }
            catch (Exception ex)
            {
                throw new Exception("Error al solicitar CAE:\n" + ex.Message);
            }

            // Paso 5: si fue aprobado, NO se guarda nada acá. En este punto el pago
            // TODAVÍA no existe en la base de datos (se pide el CAE antes de registrar
            // el cobro, justamente para no dejar pagos sin CAE si falla la conexión).
            // Es el presenter quien, al ver Resultado.Aprobado == true, recién ahí llama
            // a PagoService.ConfirmarPagoConCae(...) para insertar el pago completo
            // (con este mismo CAE) en una única operación.
            if (resultado.Aprobado)
            {
                if (!string.IsNullOrEmpty(resultado.Observaciones))
                    Log($"⚠ Obs: {resultado.Observaciones}", Color.FromArgb(230, 126, 34));
            }

            return resultado;
        }

        private void MostrarResultadoFinal(ResultadoCAE resultado)
        {
            Resultado = resultado;
            _progress.Style = ProgressBarStyle.Continuous;
            _progress.Value = 100;

            if (resultado.Aprobado)
            {
                _progress.ForeColor = Color.FromArgb(39, 174, 96);
                ActualizarEstado(_pago.TipoComprobante == 13
                    ? "✓ Nota de Crédito emitida correctamente."
                    : "✓ Factura emitida correctamente.");

                // Panel con los datos del CAE
                _panelResultado.Controls.Clear();
                _panelResultado.Visible = true;

                AgregarDatoCae("Comprobante", resultado.NroComprobante, 0);
                AgregarDatoCae("CAE", resultado.CAE, 1);
                AgregarDatoCae("Vence", resultado.CAEVencimiento.ToString("dd/MM/yyyy"), 2);
            }
            else
            {
                _progress.ForeColor = Color.FromArgb(192, 57, 43);
                ActualizarEstado("✗ La emisión fue rechazada por ARCA.");
                MostrarError(resultado.MensajeError);
            }

            _btnCerrar.Enabled = true;
            _btnReintentar.Visible = !resultado.Aprobado;
        }

        private void AgregarDatoCae(string etiqueta, string valor, int indice)
        {
            int x = indice * 158;
            _panelResultado.Controls.Add(new Label
            {
                Text = etiqueta,
                Font = new System.Drawing.Font("Segoe UI", 7.5F),
                ForeColor = Color.Gray,
                Location = new Point(x, 0),
                Size = new Size(150, 16)
            });
            _panelResultado.Controls.Add(new Label
            {
                Text = valor,
                Font = new System.Drawing.Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 60, 117),
                Location = new Point(x, 16),
                Size = new Size(150, 20)
            });
        }

        private void MostrarError(string mensaje)
        {
            Log("ERROR: " + mensaje, Color.FromArgb(192, 57, 43));
            _btnCerrar.Enabled = true;
            _btnReintentar.Visible = true;
            _progress.Style = ProgressBarStyle.Continuous;
            _progress.Value = 0;
        }

        // ── Helpers de UI thread-safe ─────────────────────────────────────────────

        private void ActualizarEstado(string texto)
        {
            if (_lblEstado.InvokeRequired)
                _lblEstado.Invoke(new Action(() => _lblEstado.Text = texto));
            else
                _lblEstado.Text = texto;
        }

        private void Log(string linea, Color color)
        {
            if (_txtLog.InvokeRequired)
            {
                _txtLog.Invoke(new Action(() => AppendLog(linea, color)));
            }
            else
            {
                AppendLog(linea, color);
            }
        }

        private void AppendLog(string linea, Color color)
        {
            _txtLog.SelectionStart = _txtLog.TextLength;
            _txtLog.SelectionLength = 0;
            _txtLog.SelectionColor = color;
            _txtLog.AppendText(linea + "\n");
            _txtLog.SelectionColor = _txtLog.ForeColor;
            _txtLog.ScrollToCaret();
        }
    }
}
