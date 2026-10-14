using GestionEducativa.Utils;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    public partial class ConexionConfigForm : Form
    {
        private Label lblTitulo, lblServidor, lblPuerto, lblUsuario, lblPassword, lblBaseDatos, lblInfo;
        private TextBox txtServidor, txtPuerto, txtUsuario, txtPassword, txtBaseDatos;
        private Button btnGuardar, btnCancelar, btnProbar;
        private CheckBox chkMostrarPassword;

        public ConexionConfigForm()
        {
            InicializarComponentes();
            CargarValoresActuales();
        }

        private void InicializarComponentes()
        {
            this.Text = "Configuración de Base de Datos";
            this.Size = new System.Drawing.Size(420, 420);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;

            int x = 20, y = 15, ancho = 250;

            lblTitulo = new Label { Text = "Configuración de conexión a MySQL", Left = x, Top = y, Width = 370, Font = new System.Drawing.Font("Segoe UI", 10, System.Drawing.FontStyle.Bold) };
            y += 30;

            lblInfo = new Label { Text = "La contraseña se guardará cifrada. Solo funciona en esta PC.", Left = x, Top = y, Width = 370, ForeColor = System.Drawing.Color.DarkGreen };
            y += 30;

            lblServidor = new Label { Text = "Servidor:", Left = x, Top = y + 3, Width = 90 };
            txtServidor = new TextBox { Left = x + 95, Top = y, Width = ancho, Text = "localhost" };
            y += 30;

            lblPuerto = new Label { Text = "Puerto:", Left = x, Top = y + 3, Width = 90 };
            txtPuerto = new TextBox { Left = x + 95, Top = y, Width = ancho, Text = "3306" };
            y += 30;

            lblBaseDatos = new Label { Text = "Base de datos:", Left = x, Top = y + 3, Width = 90 };
            txtBaseDatos = new TextBox { Left = x + 95, Top = y, Width = ancho, Text = "colegio" };
            y += 30;

            lblUsuario = new Label { Text = "Usuario:", Left = x, Top = y + 3, Width = 90 };
            txtUsuario = new TextBox { Left = x + 95, Top = y, Width = ancho, Text = "root" };
            y += 30;

            lblPassword = new Label { Text = "Contraseña:", Left = x, Top = y + 3, Width = 90 };
            txtPassword = new TextBox { Left = x + 95, Top = y, Width = ancho, UseSystemPasswordChar = true };
            y += 30;

            chkMostrarPassword = new CheckBox { Text = "Mostrar contraseña", Left = x + 95, Top = y, Width = 200 };
            chkMostrarPassword.CheckedChanged += (s, e) => txtPassword.UseSystemPasswordChar = !chkMostrarPassword.Checked;
            y += 35;

            btnProbar = new Button { Text = "Probar conexión", Left = x, Top = y, Width = 130 };
            btnProbar.Click += BtnProbar_Click;

            btnGuardar = new Button { Text = "Guardar", Left = x + 145, Top = y, Width = 90 };
            btnGuardar.Click += BtnGuardar_Click;

            btnCancelar = new Button { Text = "Cancelar", Left = x + 245, Top = y, Width = 90 };
            btnCancelar.Click += (s, e) => { this.DialogResult = DialogResult.Cancel; this.Close(); };

            this.Controls.AddRange(new Control[] {
                lblTitulo, lblInfo,
                lblServidor, txtServidor,
                lblPuerto, txtPuerto,
                lblBaseDatos, txtBaseDatos,
                lblUsuario, txtUsuario,
                lblPassword, txtPassword,
                chkMostrarPassword,
                btnProbar, btnGuardar, btnCancelar
            });

            DeshabilitarControles();
        }

        // Carga los valores actuales del App.config (descifra la contraseña si ya estaba cifrada)

        private void DeshabilitarControles()
        {
            txtServidor.Enabled = false;
            txtPuerto.Enabled = false;
            txtBaseDatos.Enabled = false;
            txtUsuario.Enabled = false;
        }
        private void CargarValoresActuales()
        {
            try
            {
                var cs = ConfigurationManager.ConnectionStrings["GestionEducativa"];
                if (cs == null) return;

                var builder = new MySqlConnectionStringBuilder(cs.ConnectionString);
                txtServidor.Text = builder.Server;
                txtPuerto.Text = builder.Port.ToString();
                txtBaseDatos.Text = builder.Database;
                txtUsuario.Text = builder.UserID;

                // La contraseña en App.config puede estar cifrada (base64 DPAPI)
                // o en texto plano (primera configuración manual)
                string passGuardada = builder.Password;
                if (CifradoHelper.EstaCifrado(passGuardada))
                    txtPassword.Text = CifradoHelper.Descifrar(passGuardada);
                else
                    txtPassword.Text = passGuardada;
            }
            catch { /* Si no hay config previa, dejamos los defaults */ }
        }

        private string ConstruirCadena(string password)
        {
            return $"server={txtServidor.Text.Trim()};" +
                   $"port={txtPuerto.Text.Trim()};" +
                   $"user={txtUsuario.Text.Trim()};" +
                   $"password={password};" +
                   $"database={txtBaseDatos.Text.Trim()};" +
                   $"CharSet=utf8mb4;";
        }

        private void BtnProbar_Click(object sender, EventArgs e)
        {
            try
            {
                string cadena = ConstruirCadena(txtPassword.Text);
                using (var conn = new MySqlConnection(cadena))
                {
                    conn.Open();
                    MessageBox.Show("¡Conexión exitosa!", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo conectar:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Ingresá la contraseña antes de guardar.", "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                // Ciframos la contraseña con DPAPI antes de guardarla
                string passwordCifrada = CifradoHelper.Cifrar(txtPassword.Text);
                string cadenaFinal = ConstruirCadena(passwordCifrada);

                // Actualizamos App.config en disco
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                config.ConnectionStrings.ConnectionStrings["GestionEducativa"].ConnectionString = cadenaFinal;
                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("connectionStrings");

                MessageBox.Show(
                    "Configuración guardada correctamente.\nLa contraseña fue cifrada y solo funciona en esta PC.",
                    "Guardado", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error al guardar:\n\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
