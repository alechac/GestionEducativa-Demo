using GestionEducativa.Models;
using GestionEducativa.Services;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    /// <summary>
    /// Panel de configuración del envío automático de comprobantes por mail
    /// al padre/tutor que registra el pago.
    /// </summary>
    public class EmailConfigPanel : Panel
    {
        private readonly EmailConfigService _configSvc;
        private readonly EmailService _emailSvc;

        private CheckBox _chkHabilitado;
        private TextBox _txtSmtpHost;
        private NumericUpDown _numSmtpPuerto;
        private TextBox _txtSmtpUsuario;
        private TextBox _txtSmtpPassword;
        private CheckBox _chkUsarSsl;
        private TextBox _txtRemitenteEmail;
        private TextBox _txtRemitenteNombre;
        private TextBox _txtCopiaOculta;
        private TextBox _txtDestinatarioPrueba;
        private Button _btnGuardar;
        private Button _btnProbar;

        public EmailConfigPanel()
        {
            _configSvc = new EmailConfigService();
            _emailSvc = new EmailService();
            Construir();
            Cargar();
        }

        private void Construir()
        {
            AutoScroll = true;
            BackColor = Color.White;
            Padding = new Padding(16);

            int y = 16;

            Agregar(new Label
            {
                Text = "Envío de comprobantes por mail",
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 60, 117),
                AutoSize = true,
                Location = new Point(0, y)
            });
            y += 28;

            Agregar(new Label
            {
                Text = "Al emitir un comprobante, si el padre/tutor que registró el pago tiene un\n" +
                       "email cargado, se le envía automáticamente una copia como evidencia de entrega.",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                ForeColor = Color.Gray,
                AutoSize = true,
                Location = new Point(0, y)
            });
            y += 44;

            _chkHabilitado = new CheckBox
            {
                Text = "Habilitar el envío automático de comprobantes por mail",
                Location = new Point(0, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 174, 96)
            };
            Controls.Add(_chkHabilitado);
            y += 36;

            AgregarEtiqueta("Servidor SMTP (ej: smtp.gmail.com):", y);
            _txtSmtpHost = AgregarTextBox(y + 20, "");
            _txtSmtpHost.Size = new Size(300, 28);
            y += 56;

            AgregarEtiqueta("Puerto:", y);
            _numSmtpPuerto = new NumericUpDown
            {
                Location = new Point(0, y + 20),
                Size = new Size(100, 28),
                Minimum = 1,
                Maximum = 65535,
                Value = 587,
                Font = new Font("Segoe UI", 9.5F)
            };
            Controls.Add(_numSmtpPuerto);

            _chkUsarSsl = new CheckBox
            {
                Text = "Usar SSL/TLS",
                Location = new Point(120, y + 22),
                AutoSize = true,
                Checked = true,
                Font = new Font("Segoe UI", 9.5F)
            };
            Controls.Add(_chkUsarSsl);
            y += 56;

            AgregarEtiqueta("Usuario SMTP (normalmente el mismo email remitente):", y);
            _txtSmtpUsuario = AgregarTextBox(y + 20, "");
            _txtSmtpUsuario.Size = new Size(300, 28);
            y += 56;

            AgregarEtiqueta("Contraseña / clave de aplicación:", y);
            _txtSmtpPassword = AgregarTextBox(y + 20, "");
            _txtSmtpPassword.Size = new Size(300, 28);
            _txtSmtpPassword.UseSystemPasswordChar = true;
            y += 56;

            AgregarEtiqueta("Email remitente:", y);
            _txtRemitenteEmail = AgregarTextBox(y + 20, "");
            _txtRemitenteEmail.Size = new Size(300, 28);
            y += 56;

            AgregarEtiqueta("Nombre del remitente (ej: Instituto San Martín):", y);
            _txtRemitenteNombre = AgregarTextBox(y + 20, "");
            _txtRemitenteNombre.Size = new Size(300, 28);
            y += 56;

            AgregarEtiqueta("Copia oculta a administración (opcional):", y);
            _txtCopiaOculta = AgregarTextBox(y + 20, "");
            _txtCopiaOculta.Size = new Size(300, 28);
            y += 64;

            _btnGuardar = new Button
            {
                Text = "💾 Guardar",
                Location = new Point(0, y),
                Size = new Size(120, 34),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 60, 117),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            _btnGuardar.FlatAppearance.BorderSize = 0;
            _btnGuardar.Click += BtnGuardar_Click;
            Controls.Add(_btnGuardar);
            y += 50;

            AgregarEtiqueta("Enviar mail de prueba a:", y);
            _txtDestinatarioPrueba = AgregarTextBox(y + 20, "");
            _txtDestinatarioPrueba.Size = new Size(260, 28);

            _btnProbar = new Button
            {
                Text = "🔌 Enviar prueba",
                Location = new Point(270, y + 20),
                Size = new Size(140, 28),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F),
                Cursor = Cursors.Hand
            };
            _btnProbar.FlatAppearance.BorderSize = 0;
            _btnProbar.Click += BtnProbar_Click;
            Controls.Add(_btnProbar);
        }

        private void Cargar()
        {
            try
            {
                var cfg = _configSvc.Cargar();
                _chkHabilitado.Checked = cfg.Habilitado;
                _txtSmtpHost.Text = cfg.SmtpHost ?? "";
                _numSmtpPuerto.Value = cfg.SmtpPuerto > 0 ? Math.Min(cfg.SmtpPuerto, 65535) : 587;
                _chkUsarSsl.Checked = cfg.UsarSsl;
                _txtSmtpUsuario.Text = cfg.SmtpUsuario ?? "";
                _txtSmtpPassword.Text = cfg.SmtpPassword ?? "";
                _txtRemitenteEmail.Text = cfg.RemitenteEmail ?? "";
                _txtRemitenteNombre.Text = cfg.RemitenteNombre ?? "";
                _txtCopiaOculta.Text = cfg.CopiaOculta ?? "";
            }
            catch { /* primera vez, sin datos */ }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            var cfg = LeerCampos();
            _configSvc.Guardar(cfg);
            MessageBox.Show("✓ Configuración de email guardada correctamente.",
                "Email", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnProbar_Click(object sender, EventArgs e)
        {
            var cfg = LeerCampos();
            string destino = _txtDestinatarioPrueba.Text.Trim();

            if (string.IsNullOrEmpty(destino))
            {
                MessageBox.Show("Ingresá un email de destino para la prueba.", "Email",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            _btnProbar.Enabled = false;
            _btnProbar.Text = "Enviando...";
            Application.DoEvents();

            try
            {
                _emailSvc.EnviarPrueba(cfg, destino);
                MessageBox.Show($"✓ Mail de prueba enviado a {destino}.", "Email — OK",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("✗ No se pudo enviar el mail de prueba.\n\n" + ex.Message,
                    "Email — Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                _btnProbar.Enabled = true;
                _btnProbar.Text = "🔌 Enviar prueba";
            }
        }

        private EmailConfig LeerCampos() => new EmailConfig
        {
            Habilitado = _chkHabilitado.Checked,
            SmtpHost = _txtSmtpHost.Text.Trim(),
            SmtpPuerto = (int)_numSmtpPuerto.Value,
            UsarSsl = _chkUsarSsl.Checked,
            SmtpUsuario = _txtSmtpUsuario.Text.Trim(),
            SmtpPassword = _txtSmtpPassword.Text,
            RemitenteEmail = _txtRemitenteEmail.Text.Trim(),
            RemitenteNombre = _txtRemitenteNombre.Text.Trim(),
            CopiaOculta = _txtCopiaOculta.Text.Trim()
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
