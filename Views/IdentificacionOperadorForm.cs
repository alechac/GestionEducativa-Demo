using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    /// <summary>
    /// NO es una pantalla de login: no valida contraseña, no bloquea el acceso, y
    /// cualquiera puede escribir cualquier nombre. Su único objetivo es dejar de ser
    /// anónimos en el registro de auditoría (Services/LogService.cs) — quien abre la
    /// app dice quién es, y ese nombre queda anotado junto a cada operación sensible
    /// (ajuste de saldo, nota de crédito, eliminar un alumno, etc.).
    /// </summary>
    public class IdentificacionOperadorForm : Form
    {
        private TextBox _txtNombre;

        public string NombreIngresado { get; private set; }

        public IdentificacionOperadorForm()
        {
            Text = "Identificación";
            Size = new Size(380, 190);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Font = new Font("Segoe UI", 9.5F);

            var lbl = new Label
            {
                AutoSize = false,
                Size = new Size(330, 40),
                Location = new Point(16, 14),
                Text = "¿Quién va a usar el sistema hoy?\n(Se anota junto a cada operación sensible)"
            };

            _txtNombre = new TextBox
            {
                Location = new Point(16, 62),
                Size = new Size(330, 26),
                Font = new Font("Segoe UI", 11F)
            };

            var btnOk = new Button
            {
                Text = "Continuar",
                DialogResult = DialogResult.OK,
                Location = new Point(200, 104),
                Size = new Size(146, 32),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnOk.Click += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(_txtNombre.Text))
                {
                    MessageBox.Show("Ingresá un nombre para continuar.", "Falta el nombre",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    DialogResult = DialogResult.None; // no cierra el form
                    return;
                }
                NombreIngresado = _txtNombre.Text.Trim();
            };

            Controls.Add(lbl);
            Controls.Add(_txtNombre);
            Controls.Add(btnOk);

            AcceptButton = btnOk;
        }
    }
}
