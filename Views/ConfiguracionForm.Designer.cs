using System.Windows.Forms;

namespace GestionEducativa.Views
{
    partial class ConfiguracionForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            var tabControl = new TabControl { Dock = DockStyle.Fill };

            var tabArca = new TabPage("ARCA / Fact. Electrónica");
            var panelArca = new ArcaConfigPanel { Dock = DockStyle.Fill };
            tabArca.Controls.Add(panelArca);
            tabControl.TabPages.Add(tabArca);

            var tabEmail = new TabPage("Email / Notificaciones");
            var panelEmail = new EmailConfigPanel { Dock = DockStyle.Fill };
            tabEmail.Controls.Add(panelEmail);
            tabControl.TabPages.Add(tabEmail);

            Controls.Add(tabControl);
            this.txtRazonSocial   = new System.Windows.Forms.TextBox();
            this.txtCUIT          = new System.Windows.Forms.TextBox();
            this.txtDomicilio     = new System.Windows.Forms.TextBox();
            this.txtTelefono      = new System.Windows.Forms.TextBox();
            this.txtPuntoVenta    = new System.Windows.Forms.TextBox();
            this.btnGuardarConfig = new System.Windows.Forms.Button();
            this.btnGuardarPrecio = new System.Windows.Forms.Button();
            this.btnNuevoPrecio   = new System.Windows.Forms.Button();
            this.gridPrecios      = new System.Windows.Forms.DataGridView();
            this.cmbNivelPrecio   = new System.Windows.Forms.ComboBox();
            this.numMontoPrecio   = new System.Windows.Forms.NumericUpDown();
            this.numAnioPrecio    = new System.Windows.Forms.NumericUpDown();
            this.rdnEscuelaPrecio  = new System.Windows.Forms.RadioButton();
            this.rdnItalianoPrecio = new System.Windows.Forms.RadioButton();

            ((System.ComponentModel.ISupportInitialize)this.gridPrecios).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.numMontoPrecio).BeginInit();
            ((System.ComponentModel.ISupportInitialize)this.numAnioPrecio).BeginInit();
            this.SuspendLayout();

            var colorPrimario = System.Drawing.Color.FromArgb(26, 82, 118);
            var colorVerde    = System.Drawing.Color.FromArgb(39, 174, 96);
            var colorFondo    = System.Drawing.Color.FromArgb(245, 248, 250);
            var fuenteNormal  = new System.Drawing.Font("Segoe UI", 9.5F);
            var fuenteNeg     = new System.Drawing.Font("Segoe UI", 9.5F, System.Drawing.FontStyle.Bold);

            

            // ── PANEL DERECHO: Precios de cuotas ────────────────────────
            var panelPrecios = new System.Windows.Forms.Panel
            {
                Dock = System.Windows.Forms.DockStyle.Fill,
                Padding = new System.Windows.Forms.Padding(15)
            };

            panelPrecios.Controls.Add(new System.Windows.Forms.Label
            {
                Text = "💲 Precios de Cuotas", AutoSize = true,
                Location = new System.Drawing.Point(15, 15),
                Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold),
                ForeColor = colorPrimario
            });

            // Grilla de precios
            this.gridPrecios.Location = new System.Drawing.Point(15, 45);
            this.gridPrecios.Size = new System.Drawing.Size(500, 280);
            this.gridPrecios.AllowUserToAddRows = false;
            this.gridPrecios.AllowUserToDeleteRows = false;
            this.gridPrecios.ReadOnly = true;
            this.gridPrecios.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridPrecios.RowHeadersVisible = false;
            this.gridPrecios.RowTemplate.Height = 30;
            this.gridPrecios.BackgroundColor = System.Drawing.Color.White;
            this.gridPrecios.ColumnHeadersHeight = 34;
            this.gridPrecios.ColumnHeadersDefaultCellStyle.BackColor = colorPrimario;
            this.gridPrecios.ColumnHeadersDefaultCellStyle.ForeColor = System.Drawing.Color.White;
            this.gridPrecios.ColumnHeadersDefaultCellStyle.Font = fuenteNeg;
            this.gridPrecios.DefaultCellStyle.Font = fuenteNormal;
            this.gridPrecios.SelectionChanged += new System.EventHandler(this.gridPrecios_SelectionChanged);
            panelPrecios.Controls.Add(this.gridPrecios);

            // Formulario nuevo/editar precio
            int py = 340;
            panelPrecios.Controls.Add(new System.Windows.Forms.Label
            {
                Text = "──── Agregar / Editar precio ──────────────────────",
                Location = new System.Drawing.Point(15, py), AutoSize = true,
                Font = new System.Drawing.Font("Segoe UI", 8F), ForeColor = System.Drawing.Color.Gray
            });
            py += 22;

            // Nivel
            panelPrecios.Controls.Add(new System.Windows.Forms.Label { Text = "Nivel:", Location = new System.Drawing.Point(15, py + 4), AutoSize = true, Font = fuenteNormal });
            this.cmbNivelPrecio.Location = new System.Drawing.Point(65, py);
            this.cmbNivelPrecio.Size = new System.Drawing.Size(165, 28);
            this.cmbNivelPrecio.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNivelPrecio.Font = fuenteNormal;
            panelPrecios.Controls.Add(this.cmbNivelPrecio);

            // Sección
            this.rdnEscuelaPrecio.Text = "Escuela"; this.rdnEscuelaPrecio.Checked = true;
            this.rdnEscuelaPrecio.Location = new System.Drawing.Point(240, py);
            this.rdnEscuelaPrecio.AutoSize = true; this.rdnEscuelaPrecio.Font = fuenteNormal;
            this.rdnItalianoPrecio.Text = "Italiano";
            this.rdnItalianoPrecio.Location = new System.Drawing.Point(320, py);
            this.rdnItalianoPrecio.AutoSize = true; this.rdnItalianoPrecio.Font = fuenteNormal;
            panelPrecios.Controls.Add(this.rdnEscuelaPrecio);
            panelPrecios.Controls.Add(this.rdnItalianoPrecio);
            py += 38;

            // Año y monto
            panelPrecios.Controls.Add(new System.Windows.Forms.Label { Text = "Año:", Location = new System.Drawing.Point(15, py + 4), AutoSize = true, Font = fuenteNormal });
            this.numAnioPrecio.Location = new System.Drawing.Point(55, py);
            this.numAnioPrecio.Size = new System.Drawing.Size(80, 28);
            this.numAnioPrecio.Minimum = 2020; this.numAnioPrecio.Maximum = 2099;
            this.numAnioPrecio.Value = System.DateTime.Now.Year;
            this.numAnioPrecio.Font = fuenteNormal;
            panelPrecios.Controls.Add(this.numAnioPrecio);

            panelPrecios.Controls.Add(new System.Windows.Forms.Label { Text = "Monto ($):", Location = new System.Drawing.Point(148, py + 4), AutoSize = true, Font = fuenteNormal });
            this.numMontoPrecio.Location = new System.Drawing.Point(225, py);
            this.numMontoPrecio.Size = new System.Drawing.Size(130, 28);
            this.numMontoPrecio.Minimum = 0; this.numMontoPrecio.Maximum = 99999999;
            this.numMontoPrecio.DecimalPlaces = 2; this.numMontoPrecio.ThousandsSeparator = true;
            this.numMontoPrecio.Font = fuenteNormal;
            panelPrecios.Controls.Add(this.numMontoPrecio);
            py += 42;

            EstiloBtn(this.btnGuardarPrecio, "💾 Guardar Precio", colorVerde, fuenteNeg);
            this.btnGuardarPrecio.Location = new System.Drawing.Point(15, py);
            this.btnGuardarPrecio.Size = new System.Drawing.Size(160, 34);
            this.btnGuardarPrecio.Click += new System.EventHandler(this.btnGuardarPrecio_Click);
            panelPrecios.Controls.Add(this.btnGuardarPrecio);

            this.Controls.Add(panelPrecios);

            this.Font = fuenteNormal;
            this.BackColor = System.Drawing.Color.White;

            ((System.ComponentModel.ISupportInitialize)this.gridPrecios).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.numMontoPrecio).EndInit();
            ((System.ComponentModel.ISupportInitialize)this.numAnioPrecio).EndInit();
            this.ResumeLayout(false);
        }

        private void AddField(System.Windows.Forms.Panel panel, string label,
            int lx, int y, int cx, int tw,
            System.Windows.Forms.TextBox txt,
            System.Drawing.Font fNormal, System.Drawing.Font fNeg)
        {
            panel.Controls.Add(new System.Windows.Forms.Label
            { Text = label, Location = new System.Drawing.Point(lx, y + 5), AutoSize = true, Font = fNeg });
            txt.Location = new System.Drawing.Point(cx, y);
            txt.Size = new System.Drawing.Size(tw, 28);
            txt.Font = fNormal;
            panel.Controls.Add(txt);
        }

        private void EstiloBtn(System.Windows.Forms.Button btn, string texto,
            System.Drawing.Color color, System.Drawing.Font fuente)
        {
            btn.Text = texto; btn.BackColor = color;
            btn.ForeColor = System.Drawing.Color.White; btn.Font = fuente;
            btn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.Cursor = System.Windows.Forms.Cursors.Hand;
        }
    }
}
