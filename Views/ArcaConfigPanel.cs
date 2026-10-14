using GestionEducativa.Database;
using GestionEducativa.Models;
using GestionEducativa.Services;
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    public class ArcaConfigPanel : Panel
    {
        private readonly ArcaConfigService _configSvc;

        private TextBox _txtCuit;
        private TextBox _txtRazonSocial;
        private NumericUpDown _numPuntoVenta;
        private TextBox _txtRutaPfx;
        private TextBox _txtPasswordPfx;
        private CheckBox _chkProduccion;
        private Label _lblEstadoCert;
        private Button _btnGuardar;
        private Button _btnProbar;

        public ArcaConfigPanel()
        {
            _configSvc = new ArcaConfigService();
            Construir();
            Cargar();
        }

        private void Construir()
        {
            AutoScroll = true;
            BackColor = Color.White;
            Padding = new Padding(16);

            int y = 16;

            // ── Título ────────────────────────────────────────────────────────────
            Agregar(new Label
            {
                Text = "Configuración ARCA — Factura Electrónica",
                Font = new System.Drawing.Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 60, 117),
                AutoSize = true,
                Location = new Point(0, y)
            });
            y += 34;

            // ── CUIT ──────────────────────────────────────────────────────────────
            AgregarEtiqueta("CUIT del emisor (sin guiones):", y);
            _txtCuit = AgregarTextBox(y + 20, "Ej: 20123456789");
            y += 56;

            // ── Razón Social ──────────────────────────────────────────────────────
            AgregarEtiqueta("Razón Social:", y);
            _txtRazonSocial = AgregarTextBox(y + 20, "Ej: Instituto San Martín S.A.");
            y += 56;

            // ── Punto de Venta ────────────────────────────────────────────────────
            AgregarEtiqueta("Punto de Venta:", y);
            _numPuntoVenta = new NumericUpDown
            {
                Location = new Point(0, y + 20),
                Size = new Size(100, 28),
                Minimum = 1,
                Maximum = 9999,
                Value = 1,
                Font = new System.Drawing.Font("Segoe UI", 9.5F)
            };
            Controls.Add(_numPuntoVenta);
            y += 56;

            // ── Ruta PFX ──────────────────────────────────────────────────────────
            AgregarEtiqueta("Ruta del certificado (.pfx):", y);

            _txtRutaPfx = new TextBox
            {
                Location = new Point(0, y + 20),
                Size = new Size(360, 28),
                Font = new System.Drawing.Font("Segoe UI", 9.5F),
                Text = "D:\\CertificadosOpenSSL\\colegio.pfx"
            };
            Controls.Add(_txtRutaPfx);

            var btnBuscar = new Button
            {
                Text = "📂 Buscar",
                Location = new Point(368, y + 20),
                Size = new Size(90, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            btnBuscar.FlatAppearance.BorderSize = 0;
            btnBuscar.Click += BtnBuscarPfx_Click;
            Controls.Add(btnBuscar);

            _lblEstadoCert = new Label
            {
                Location = new Point(0, y + 52),
                Size = new Size(460, 18),
                Font = new System.Drawing.Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.Gray,
                Text = ""
            };
            Controls.Add(_lblEstadoCert);
            y += 78;

            // ── Password PFX ──────────────────────────────────────────────────────
            AgregarEtiqueta("Contraseña del certificado (dejar vacío si no tiene):", y);
            _txtPasswordPfx = AgregarTextBox(y + 20, "");
            _txtPasswordPfx.UseSystemPasswordChar = true;
            _txtPasswordPfx.Size = new Size(220, 28);
            y += 56;

            // ── Ambiente ──────────────────────────────────────────────────────────
            _chkProduccion = new CheckBox
            {
                Text = "Ambiente de Producción  (desmarcar para usar Homologación/Testing)",
                Location = new Point(0, y),
                AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(192, 57, 43)
            };
            Controls.Add(_chkProduccion);
            y += 36;

            // ── Nota Homologación ─────────────────────────────────────────────────
            var lblNota = new Label
            {
                Text = "⚠  Mientras gestionás el certificado, dejá desmarcado 'Producción' para operar\n" +
                            "    contra el ambiente de Homologación (pruebas, sin efectos reales).",
                Location = new Point(0, y),
                Size = new Size(460, 40),
                Font = new System.Drawing.Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.FromArgb(180, 100, 0)
            };
            Controls.Add(lblNota);
            y += 52;

            // ── Botones ───────────────────────────────────────────────────────────
            _btnProbar = new Button
            {
                Text = "🔌 Probar conexión",
                Location = new Point(0, y),
                Size = new Size(148, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            _btnProbar.FlatAppearance.BorderSize = 0;
            _btnProbar.Click += BtnProbar_Click;
            Controls.Add(_btnProbar);

            _btnGuardar = new Button
            {
                Text = "💾 Guardar",
                Location = new Point(156, y),
                Size = new Size(110, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 60, 117),
                ForeColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            _btnGuardar.FlatAppearance.BorderSize = 0;
            _btnGuardar.Click += BtnGuardar_Click;
            Controls.Add(_btnGuardar);

            // Botón diagnóstico PFX + comprobante
            var btnDiag = new Button
            {
                Text = "🔍 Diagnosticar",
                Location = new Point(275, y),
                Size = new Size(130, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(142, 68, 173),
                ForeColor = Color.White,
                Font = new System.Drawing.Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            btnDiag.FlatAppearance.BorderSize = 0;
            btnDiag.Click += (s, e) =>
            {
                try
                {
                    BtnGuardar_Click(s, e); // guardar primero para usar config actualizada
                    var cfg = _configSvc.Cargar();
                    var svc = new GestionEducativa.Services.ArcaService(cfg);

                    string diag = svc.DiagnosticarPfx();

                    // Además consultar el último comprobante emitido
                    try
                    {
                        var ticket = svc.ObtenerTicketAcceso();
                        int tipoCbte = cfg.Produccion ? 11 : 11;
                        long ultimo = svc.ObtenerUltimoNroComprobante(tipoCbte, ticket);
                        diag += $"\n\n📋 Último Nro. comprobante en ARCA (tipo {tipoCbte}): {ultimo}";

                        if (ultimo > 0)
                        {
                            string xmlConsulta = svc.ConsultarComprobante(ultimo, tipoCbte, ticket);
                            // Mostrar solo si tiene CAE o error — evitar mostrar XML crudo largo
                            bool tieneCAE = xmlConsulta.Contains("<CAE>");
                            bool tieneError = xmlConsulta.Contains("<Errors>");
                            if (tieneCAE)
                                diag += $"\n✅ Comprobante {ultimo} encontrado en ARCA con CAE válido.";
                            else if (tieneError)
                                diag += $"\n⚠ ARCA reportó error al consultar el comprobante {ultimo}.\n" +
                                        $"   Esto puede indicar que el punto de venta no está dado de alta\n" +
                                        $"   como 'Web Services' en ARCA.";
                            else
                                diag += $"\n⚠ Respuesta inesperada de ARCA al consultar comprobante {ultimo}.";
                        }
                        else
                            diag += "\n⚠ No hay comprobantes emitidos todavía para este punto de venta.";
                    }
                    catch (Exception exC)
                    {
                        diag += $"\n⚠ No se pudo consultar ARCA: {exC.Message}";
                    }

                    MessageBox.Show(diag, "Diagnóstico ARCA / PFX",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "Diagnóstico",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            };
            Controls.Add(btnDiag);
        }

        private void Cargar()
        {
            try
            {
                var cfg = _configSvc.Cargar();
                _txtCuit.Text = cfg.CUIT ?? "";
                _txtRazonSocial.Text = cfg.RazonSocial ?? "";
                _numPuntoVenta.Value = Math.Max(1, cfg.PuntoVenta);
                _txtRutaPfx.Text = cfg.RutaPfx ?? "";
                _txtPasswordPfx.Text = cfg.PasswordPfx ?? "";
                _chkProduccion.Checked = cfg.Produccion;
                ActualizarEstadoCert(cfg.RutaPfx);
            }
            catch { /* primera vez, sin datos */ }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            var cfg = LeerCampos();
            _configSvc.Guardar(cfg);
            ActualizarEstadoCert(cfg.RutaPfx);
            MessageBox.Show("✓ Configuración ARCA guardada correctamente.",
                "ARCA", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnProbar_Click(object sender, EventArgs e)
        {
            var cfg = LeerCampos();

            _btnProbar.Enabled = false;
            _btnProbar.Text = "Probando...";
            Application.DoEvents();

            try
            {
                _configSvc.Validar(cfg);
                var svc = new ArcaService(cfg);
                bool ok = svc.VerificarDisponibilidad();

                string estadoServidores =
                    $"  AppServer:  {svc.UltimoEstadoAppServer}\n" +
                    $"  AuthServer: {svc.UltimoEstadoAuthServer}\n" +
                    $"  DbServer:   {svc.UltimoEstadoDbServer}";

                if (ok)
                    MessageBox.Show(
                        "✓ Conexión exitosa con ARCA.\n\nFEDummy:\n" + estadoServidores,
                        "ARCA — Conexión OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(
                        "⚠ Conexión establecida pero algún servidor no responde:\n\n" +
                        estadoServidores + "\n\nPuede ser mantenimiento temporal. Intentalo en unos minutos.",
                        "ARCA — Servicio no disponible", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                // Mostrar el mensaje completo — ahora ArcaService propaga el detalle real
                MessageBox.Show(
                    "✗ No se pudo conectar con ARCA.\n\n" + ex.Message,
                    "ARCA — Error de conexión", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnProbar.Enabled = true;
                _btnProbar.Text = "🔌 Probar conexión";
            }
        }

        private void BtnBuscarPfx_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog())
            {
                dlg.Title = "Seleccionar certificado ARCA (.pfx)";
                dlg.Filter = "Certificado PKCS#12 (*.pfx)|*.pfx|Todos los archivos|*.*";

                if (dlg.ShowDialog() == DialogResult.OK)
                {
                    _txtRutaPfx.Text = dlg.FileName;
                    ActualizarEstadoCert(dlg.FileName);
                }
            }
        }

        private void ActualizarEstadoCert(string ruta)
        {
            if (string.IsNullOrWhiteSpace(ruta))
            {
                _lblEstadoCert.Text = "";
                return;
            }

            if (File.Exists(ruta))
            {
                _lblEstadoCert.Text = "✓ Archivo encontrado.";
                _lblEstadoCert.ForeColor = Color.FromArgb(39, 174, 96);
            }
            else
            {
                _lblEstadoCert.Text = "✗ Archivo no encontrado en esa ruta.";
                _lblEstadoCert.ForeColor = Color.FromArgb(192, 57, 43);
            }
        }

        private ArcaConfig LeerCampos() => new ArcaConfig
        {
            CUIT = _txtCuit.Text.Trim(),
            RazonSocial = _txtRazonSocial.Text.Trim(),
            PuntoVenta = (int)_numPuntoVenta.Value,
            RutaPfx = _txtRutaPfx.Text.Trim(),
            PasswordPfx = _txtPasswordPfx.Text,
            Produccion = _chkProduccion.Checked
        };

        // ── Helpers de construcción ───────────────────────────────────────────────

        private void AgregarEtiqueta(string texto, int y)
        {
            Controls.Add(new Label
            {
                Text = texto,
                Font = new Font("Segoe UI", 9F),
                ForeColor = Color.FromArgb(80, 80, 80),
                Location = new Point(0, y),
                AutoSize = true
            });
        }

        private TextBox AgregarTextBox(int y, string placeholder)
        {
            var txt = new TextBox
            {
                Location = new Point(0, y),
                Size = new Size(460, 28),
                Font = new Font("Segoe UI", 9.5F),
                Text = placeholder
            };
            Controls.Add(txt);
            return txt;
        }

        private void Agregar(Control c) => Controls.Add(c);
    }
}
