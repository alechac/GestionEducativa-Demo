namespace GestionEducativa.Views
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.panelSeccion = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.btnEscuela = new System.Windows.Forms.Button();
            this.btnItaliano = new System.Windows.Forms.Button();
            this.tabPrincipal = new System.Windows.Forms.TabControl();
            this.tabInicio = new System.Windows.Forms.TabPage();
            this.tabAlumnos = new System.Windows.Forms.TabPage();
            this.tabPadres = new System.Windows.Forms.TabPage();
            this.tabPagos = new System.Windows.Forms.TabPage();
            this.tabGastos = new System.Windows.Forms.TabPage();
            this.tabNotasCredito = new System.Windows.Forms.TabPage();
            this.tabInformes = new System.Windows.Forms.TabPage();
            this.tabConfiguracion = new System.Windows.Forms.TabPage();
            this.panelSeccion.SuspendLayout();
            this.tabPrincipal.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelSeccion
            // 
            this.panelSeccion.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.panelSeccion.Controls.Add(this.lblTitulo);
            this.panelSeccion.Controls.Add(this.btnEscuela);
            this.panelSeccion.Controls.Add(this.btnItaliano);
            this.panelSeccion.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelSeccion.Location = new System.Drawing.Point(0, 0);
            this.panelSeccion.Name = "panelSeccion";
            this.panelSeccion.Size = new System.Drawing.Size(1024, 60);
            this.panelSeccion.TabIndex = 1;
            // 
            // lblTitulo
            // 
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(12, 15);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(550, 30);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Institución Educativa  —  Escuela";
            // 
            // btnEscuela
            // 
            this.btnEscuela.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.btnEscuela.FlatAppearance.BorderColor = System.Drawing.Color.White;
            this.btnEscuela.FlatAppearance.BorderSize = 3;
            this.btnEscuela.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnEscuela.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnEscuela.ForeColor = System.Drawing.Color.White;
            this.btnEscuela.Location = new System.Drawing.Point(620, 10);
            this.btnEscuela.Name = "btnEscuela";
            this.btnEscuela.Size = new System.Drawing.Size(130, 38);
            this.btnEscuela.TabIndex = 1;
            this.btnEscuela.Text = "✓ 🏫 Escuela";
            this.btnEscuela.UseVisualStyleBackColor = false;
            this.btnEscuela.Click += new System.EventHandler(this.btnEscuela_Click);
            // 
            // btnItaliano
            // 
            this.btnItaliano.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(210)))), ((int)(((byte)(210)))), ((int)(((byte)(210)))));
            this.btnItaliano.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(((int)(((byte)(170)))), ((int)(((byte)(170)))), ((int)(((byte)(170)))));
            this.btnItaliano.FlatAppearance.BorderSize = 1;
            this.btnItaliano.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnItaliano.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Regular);
            this.btnItaliano.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(90)))), ((int)(((byte)(90)))), ((int)(((byte)(90)))));
            this.btnItaliano.Location = new System.Drawing.Point(760, 10);
            this.btnItaliano.Name = "btnItaliano";
            this.btnItaliano.Size = new System.Drawing.Size(180, 38);
            this.btnItaliano.TabIndex = 2;
            this.btnItaliano.Text = "🇮🇹 Scuola Italiano";
            this.btnItaliano.UseVisualStyleBackColor = false;
            this.btnItaliano.Click += new System.EventHandler(this.btnItaliano_Click);
            // 
            // tabPrincipal
            // 
            this.tabPrincipal.Controls.Add(this.tabInicio);
            this.tabPrincipal.Controls.Add(this.tabAlumnos);
            this.tabPrincipal.Controls.Add(this.tabPadres);
            this.tabPrincipal.Controls.Add(this.tabPagos);
            this.tabPrincipal.Controls.Add(this.tabGastos);
            this.tabPrincipal.Controls.Add(this.tabNotasCredito);
            this.tabPrincipal.Controls.Add(this.tabInformes);
            this.tabPrincipal.Controls.Add(this.tabConfiguracion);
            this.tabPrincipal.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabPrincipal.Font = new System.Drawing.Font("Segoe UI", 10F);
            this.tabPrincipal.ItemSize = new System.Drawing.Size(120, 35);
            this.tabPrincipal.Location = new System.Drawing.Point(0, 60);
            this.tabPrincipal.Name = "tabPrincipal";
            this.tabPrincipal.SelectedIndex = 0;
            this.tabPrincipal.Size = new System.Drawing.Size(1024, 640);
            this.tabPrincipal.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabPrincipal.TabIndex = 0;
            // 
            // tabInicio
            // 
            this.tabInicio.Location = new System.Drawing.Point(4, 39);
            this.tabInicio.Name = "tabInicio";
            this.tabInicio.Size = new System.Drawing.Size(192, 57);
            this.tabInicio.TabIndex = 0;
            this.tabInicio.Text = "🏠  Inicio";
            this.tabInicio.UseVisualStyleBackColor = true;
            // 
            // tabAlumnos
            // 
            this.tabAlumnos.Location = new System.Drawing.Point(4, 39);
            this.tabAlumnos.Name = "tabAlumnos";
            this.tabAlumnos.Size = new System.Drawing.Size(1016, 597);
            this.tabAlumnos.TabIndex = 1;
            this.tabAlumnos.Text = "👥  Alumnos";
            this.tabAlumnos.UseVisualStyleBackColor = true;
            // 
            // tabPadres
            // 
            this.tabPadres.Location = new System.Drawing.Point(4, 39);
            this.tabPadres.Name = "tabPadres";
            this.tabPadres.Size = new System.Drawing.Size(192, 57);
            this.tabPadres.TabIndex = 2;
            this.tabPadres.Text = "👪  Padres";
            this.tabPadres.UseVisualStyleBackColor = true;
            // 
            // tabPagos
            // 
            this.tabPagos.Location = new System.Drawing.Point(4, 39);
            this.tabPagos.Name = "tabPagos";
            this.tabPagos.Size = new System.Drawing.Size(192, 57);
            this.tabPagos.TabIndex = 3;
            this.tabPagos.Text = "💰  Cobros";
            this.tabPagos.UseVisualStyleBackColor = true;
            // 
            // tabGastos
            // 
            this.tabGastos.Location = new System.Drawing.Point(4, 39);
            this.tabGastos.Name = "tabGastos";
            this.tabGastos.Size = new System.Drawing.Size(192, 57);
            this.tabGastos.TabIndex = 4;
            this.tabGastos.Text = "💸  Gastos";
            this.tabGastos.UseVisualStyleBackColor = true;
            // 
            // tabNotasCredito
            // 
            this.tabNotasCredito.Location = new System.Drawing.Point(4, 39);
            this.tabNotasCredito.Name = "tabNotasCredito";
            this.tabNotasCredito.Size = new System.Drawing.Size(192, 57);
            this.tabNotasCredito.TabIndex = 5;
            this.tabNotasCredito.Text = "🧾  Notas de Crédito";
            this.tabNotasCredito.UseVisualStyleBackColor = true;
            // 
            // tabInformes
            // 
            this.tabInformes.Location = new System.Drawing.Point(4, 39);
            this.tabInformes.Name = "tabInformes";
            this.tabInformes.Size = new System.Drawing.Size(192, 57);
            this.tabInformes.TabIndex = 6;
            this.tabInformes.Text = "📊  Informes";
            this.tabInformes.UseVisualStyleBackColor = true;
            // 
            // tabConfiguracion
            // 
            this.tabConfiguracion.Location = new System.Drawing.Point(4, 39);
            this.tabConfiguracion.Name = "tabConfiguracion";
            this.tabConfiguracion.Size = new System.Drawing.Size(192, 57);
            this.tabConfiguracion.TabIndex = 7;
            this.tabConfiguracion.Text = "⚙️  Configuración";
            this.tabConfiguracion.UseVisualStyleBackColor = true;
            // 
            // MainForm
            // 
            this.ClientSize = new System.Drawing.Size(1024, 700);
            this.Controls.Add(this.tabPrincipal);
            this.Controls.Add(this.panelSeccion);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MinimumSize = new System.Drawing.Size(1024, 700);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sistema de Gestión Educativa";
            this.WindowState = System.Windows.Forms.FormWindowState.Maximized;
            this.panelSeccion.ResumeLayout(false);
            this.tabPrincipal.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion
    }
}