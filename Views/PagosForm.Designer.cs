using Org.BouncyCastle.Pqc.Crypto.Lms;
using System.Drawing;

namespace GestionEducativa.Views
{
    partial class PagosForm
    {
        private System.ComponentModel.IContainer components = null;
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle4 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelAlumnos = new System.Windows.Forms.Panel();
            this.lblTituloAlumnos = new System.Windows.Forms.Label();
            this.lblFiltroNombre = new System.Windows.Forms.Label();
            this.txtFiltroNombre = new System.Windows.Forms.TextBox();
            this.lblFiltroNivel = new System.Windows.Forms.Label();
            this.cmbFiltroNivel = new System.Windows.Forms.ComboBox();
            this.btnFiltrar = new System.Windows.Forms.Button();
            this.panelPadre = new System.Windows.Forms.Panel();
            this.lblTituloPadre = new System.Windows.Forms.Label();
            this.lblNombrePadre = new System.Windows.Forms.Label();
            this.lblValNombrePadre = new System.Windows.Forms.Label();
            this.lblDocPadre = new System.Windows.Forms.Label();
            this.lblValDocPadre = new System.Windows.Forms.Label();
            this.lblTelPadre = new System.Windows.Forms.Label();
            this.lblValTelPadre = new System.Windows.Forms.Label();
            this.btnVerHermanos = new System.Windows.Forms.Button();
            this.gridAlumnos = new System.Windows.Forms.DataGridView();
            this.lblContadorAlumnos = new System.Windows.Forms.Label();
            this.panelCuotas = new System.Windows.Forms.Panel();
            this.lblTituloCuotas = new System.Windows.Forms.Label();
            this.lblAlumnoSeleccionado = new System.Windows.Forms.Label();
            this.lblNombreAlumno = new System.Windows.Forms.Label();
            this.gridCuotas = new System.Windows.Forms.DataGridView();
            this.chkMostrarTodas = new System.Windows.Forms.CheckBox();
            this.lblTotalSeleccionado = new System.Windows.Forms.Label();
            this.lblGenerar = new System.Windows.Forms.Label();
            this.lblMes = new System.Windows.Forms.Label();
            this.cmbMes = new System.Windows.Forms.ComboBox();
            this.lblAnio = new System.Windows.Forms.Label();
            this.numAnio = new System.Windows.Forms.NumericUpDown();
            this.btnGenerarCuota = new System.Windows.Forms.Button();
            this._panelConceptos = new GestionEducativa.Views.ConceptosPanel();
            this.panelCobro = new System.Windows.Forms.Panel();
            this.lblTituloCobro = new System.Windows.Forms.Label();
            this.panelResumen = new System.Windows.Forms.Panel();
            this.lblEtiquetaTotalDeuda = new System.Windows.Forms.Label();
            this.lblTotalDeuda = new System.Windows.Forms.Label();
            this.lblEtiquetaVuelto = new System.Windows.Forms.Label();
            this.lblVuelto = new System.Windows.Forms.Label();
            this.lblEtiquetaDeuda = new System.Windows.Forms.Label();
            this.lblDeuda = new System.Windows.Forms.Label();
            this.lblMontoRecibido = new System.Windows.Forms.Label();
            this.numMonto = new System.Windows.Forms.NumericUpDown();
            this.btnDistribuirMonto = new System.Windows.Forms.Button();
            this.lblAyudaDistribucion = new System.Windows.Forms.Label();
            this.lblSepPagador = new System.Windows.Forms.Label();
            this.lblPagador = new System.Windows.Forms.Label();
            this.txtPagador = new System.Windows.Forms.TextBox();
            this.rdnDNI = new System.Windows.Forms.RadioButton();
            this.rdnCUIT = new System.Windows.Forms.RadioButton();
            this.txtDocumento = new System.Windows.Forms.TextBox();
            this.lblObservaciones = new System.Windows.Forms.Label();
            this.txtObservaciones = new System.Windows.Forms.TextBox();
            this.cmbFormaPago = new System.Windows.Forms.ComboBox();
            this.btnRegistrarPago = new System.Windows.Forms.Button();
            this.btnImprimir = new System.Windows.Forms.Button();
            this.lblFormaPago = new System.Windows.Forms.Label();
            this.panelSeparador1 = new System.Windows.Forms.Panel();
            this.panelSeparador2 = new System.Windows.Forms.Panel();
            this.panelAlumnos.SuspendLayout();
            this.panelPadre.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAlumnos)).BeginInit();
            this.panelCuotas.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridCuotas)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAnio)).BeginInit();
            this.panelCobro.SuspendLayout();
            this.panelResumen.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMonto)).BeginInit();
            this.SuspendLayout();
            // 
            // panelAlumnos
            // 
            this.panelAlumnos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(255)))));
            this.panelAlumnos.Controls.Add(this.lblTituloAlumnos);
            this.panelAlumnos.Controls.Add(this.lblFiltroNombre);
            this.panelAlumnos.Controls.Add(this.txtFiltroNombre);
            this.panelAlumnos.Controls.Add(this.lblFiltroNivel);
            this.panelAlumnos.Controls.Add(this.cmbFiltroNivel);
            this.panelAlumnos.Controls.Add(this.btnFiltrar);
            this.panelAlumnos.Controls.Add(this.panelPadre);
            this.panelAlumnos.Controls.Add(this.gridAlumnos);
            this.panelAlumnos.Controls.Add(this.lblContadorAlumnos);
            this.panelAlumnos.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelAlumnos.Location = new System.Drawing.Point(2, 0);
            this.panelAlumnos.Name = "panelAlumnos";
            this.panelAlumnos.Padding = new System.Windows.Forms.Padding(10);
            this.panelAlumnos.Size = new System.Drawing.Size(251, 711);
            this.panelAlumnos.TabIndex = 3;
            // 
            // lblTituloAlumnos
            // 
            this.lblTituloAlumnos.AutoSize = true;
            this.lblTituloAlumnos.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTituloAlumnos.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.lblTituloAlumnos.Location = new System.Drawing.Point(10, 10);
            this.lblTituloAlumnos.Name = "lblTituloAlumnos";
            this.lblTituloAlumnos.Size = new System.Drawing.Size(148, 20);
            this.lblTituloAlumnos.TabIndex = 0;
            this.lblTituloAlumnos.Text = "Listado de Alumnos";
            // 
            // lblFiltroNombre
            // 
            this.lblFiltroNombre.AutoSize = true;
            this.lblFiltroNombre.Location = new System.Drawing.Point(10, 42);
            this.lblFiltroNombre.Name = "lblFiltroNombre";
            this.lblFiltroNombre.Size = new System.Drawing.Size(45, 15);
            this.lblFiltroNombre.TabIndex = 1;
            this.lblFiltroNombre.Text = "Buscar:";
            // 
            // txtFiltroNombre
            // 
            this.txtFiltroNombre.Location = new System.Drawing.Point(65, 39);
            this.txtFiltroNombre.Name = "txtFiltroNombre";
            this.txtFiltroNombre.Size = new System.Drawing.Size(101, 23);
            this.txtFiltroNombre.TabIndex = 2;
            this.txtFiltroNombre.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtFiltroNombre_KeyDown);
            // 
            // lblFiltroNivel
            // 
            this.lblFiltroNivel.AutoSize = true;
            this.lblFiltroNivel.Location = new System.Drawing.Point(10, 72);
            this.lblFiltroNivel.Name = "lblFiltroNivel";
            this.lblFiltroNivel.Size = new System.Drawing.Size(37, 15);
            this.lblFiltroNivel.TabIndex = 3;
            this.lblFiltroNivel.Text = "Nivel:";
            // 
            // cmbFiltroNivel
            // 
            this.cmbFiltroNivel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFiltroNivel.Location = new System.Drawing.Point(65, 69);
            this.cmbFiltroNivel.Name = "cmbFiltroNivel";
            this.cmbFiltroNivel.Size = new System.Drawing.Size(171, 23);
            this.cmbFiltroNivel.TabIndex = 4;
            // 
            // btnFiltrar
            // 
            this.btnFiltrar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnFiltrar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnFiltrar.FlatAppearance.BorderSize = 0;
            this.btnFiltrar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnFiltrar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnFiltrar.ForeColor = System.Drawing.Color.White;
            this.btnFiltrar.Location = new System.Drawing.Point(172, 36);
            this.btnFiltrar.Name = "btnFiltrar";
            this.btnFiltrar.Size = new System.Drawing.Size(64, 23);
            this.btnFiltrar.TabIndex = 5;
            this.btnFiltrar.Text = "Buscar";
            this.btnFiltrar.UseVisualStyleBackColor = false;
            this.btnFiltrar.Click += new System.EventHandler(this.btnFiltrar_Click);
            // 
            // panelPadre
            // 
            this.panelPadre.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.panelPadre.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelPadre.Controls.Add(this.lblTituloPadre);
            this.panelPadre.Controls.Add(this.lblNombrePadre);
            this.panelPadre.Controls.Add(this.lblValNombrePadre);
            this.panelPadre.Controls.Add(this.lblDocPadre);
            this.panelPadre.Controls.Add(this.lblValDocPadre);
            this.panelPadre.Controls.Add(this.lblTelPadre);
            this.panelPadre.Controls.Add(this.lblValTelPadre);
            this.panelPadre.Controls.Add(this.btnVerHermanos);
            this.panelPadre.Location = new System.Drawing.Point(6, 418);
            this.panelPadre.Name = "panelPadre";
            this.panelPadre.Size = new System.Drawing.Size(229, 143);
            this.panelPadre.TabIndex = 6;
            // 
            // lblTituloPadre
            // 
            this.lblTituloPadre.AutoSize = true;
            this.lblTituloPadre.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblTituloPadre.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.lblTituloPadre.Location = new System.Drawing.Point(5, 5);
            this.lblTituloPadre.Name = "lblTituloPadre";
            this.lblTituloPadre.Size = new System.Drawing.Size(154, 15);
            this.lblTituloPadre.TabIndex = 0;
            this.lblTituloPadre.Text = "👨‍👩‍👧 Datos del Padre / Tutor";
            // 
            // lblNombrePadre
            // 
            this.lblNombrePadre.AutoSize = true;
            this.lblNombrePadre.ForeColor = System.Drawing.Color.Gray;
            this.lblNombrePadre.Location = new System.Drawing.Point(5, 28);
            this.lblNombrePadre.Name = "lblNombrePadre";
            this.lblNombrePadre.Size = new System.Drawing.Size(54, 15);
            this.lblNombrePadre.TabIndex = 1;
            this.lblNombrePadre.Text = "Nombre:";
            // 
            // lblValNombrePadre
            // 
            this.lblValNombrePadre.AutoSize = true;
            this.lblValNombrePadre.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblValNombrePadre.Location = new System.Drawing.Point(70, 28);
            this.lblValNombrePadre.Name = "lblValNombrePadre";
            this.lblValNombrePadre.Size = new System.Drawing.Size(12, 15);
            this.lblValNombrePadre.TabIndex = 2;
            this.lblValNombrePadre.Text = "-";
            // 
            // lblDocPadre
            // 
            this.lblDocPadre.AutoSize = true;
            this.lblDocPadre.ForeColor = System.Drawing.Color.Gray;
            this.lblDocPadre.Location = new System.Drawing.Point(5, 50);
            this.lblDocPadre.Name = "lblDocPadre";
            this.lblDocPadre.Size = new System.Drawing.Size(31, 15);
            this.lblDocPadre.TabIndex = 3;
            this.lblDocPadre.Text = "Doc:";
            // 
            // lblValDocPadre
            // 
            this.lblValDocPadre.AutoSize = true;
            this.lblValDocPadre.Location = new System.Drawing.Point(70, 50);
            this.lblValDocPadre.Name = "lblValDocPadre";
            this.lblValDocPadre.Size = new System.Drawing.Size(12, 15);
            this.lblValDocPadre.TabIndex = 4;
            this.lblValDocPadre.Text = "-";
            // 
            // lblTelPadre
            // 
            this.lblTelPadre.AutoSize = true;
            this.lblTelPadre.ForeColor = System.Drawing.Color.Gray;
            this.lblTelPadre.Location = new System.Drawing.Point(5, 72);
            this.lblTelPadre.Name = "lblTelPadre";
            this.lblTelPadre.Size = new System.Drawing.Size(24, 15);
            this.lblTelPadre.TabIndex = 5;
            this.lblTelPadre.Text = "Tel:";
            // 
            // lblValTelPadre
            // 
            this.lblValTelPadre.AutoSize = true;
            this.lblValTelPadre.Location = new System.Drawing.Point(70, 72);
            this.lblValTelPadre.Name = "lblValTelPadre";
            this.lblValTelPadre.Size = new System.Drawing.Size(12, 15);
            this.lblValTelPadre.TabIndex = 6;
            this.lblValTelPadre.Text = "-";
            // 
            // btnVerHermanos
            // 
            this.btnVerHermanos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(68)))), ((int)(((byte)(173)))));
            this.btnVerHermanos.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnVerHermanos.FlatAppearance.BorderSize = 0;
            this.btnVerHermanos.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnVerHermanos.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Bold);
            this.btnVerHermanos.ForeColor = System.Drawing.Color.White;
            this.btnVerHermanos.Location = new System.Drawing.Point(5, 90);
            this.btnVerHermanos.Name = "btnVerHermanos";
            this.btnVerHermanos.Size = new System.Drawing.Size(211, 28);
            this.btnVerHermanos.TabIndex = 7;
            this.btnVerHermanos.UseVisualStyleBackColor = false;
            this.btnVerHermanos.Visible = false;
            this.btnVerHermanos.Click += new System.EventHandler(this.btnVerHermanos_Click);
            // 
            // gridAlumnos
            // 
            this.gridAlumnos.AllowUserToAddRows = false;
            this.gridAlumnos.AllowUserToDeleteRows = false;
            this.gridAlumnos.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            dataGridViewCellStyle1.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle1.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle1.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle1.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle1.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridAlumnos.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle1;
            this.gridAlumnos.ColumnHeadersHeight = 30;
            this.gridAlumnos.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(234)))), ((int)(((byte)(248)))));
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.Color.Black;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridAlumnos.DefaultCellStyle = dataGridViewCellStyle2;
            this.gridAlumnos.EnableHeadersVisualStyles = false;
            this.gridAlumnos.Location = new System.Drawing.Point(10, 100);
            this.gridAlumnos.MultiSelect = false;
            this.gridAlumnos.Name = "gridAlumnos";
            this.gridAlumnos.ReadOnly = true;
            this.gridAlumnos.RowHeadersVisible = false;
            this.gridAlumnos.RowTemplate.Height = 26;
            this.gridAlumnos.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridAlumnos.Size = new System.Drawing.Size(229, 282);
            this.gridAlumnos.TabIndex = 7;
            this.gridAlumnos.CellClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridAlumnos_CellClick);
            // 
            // lblContadorAlumnos
            // 
            this.lblContadorAlumnos.AutoSize = true;
            this.lblContadorAlumnos.ForeColor = System.Drawing.Color.Gray;
            this.lblContadorAlumnos.Location = new System.Drawing.Point(11, 392);
            this.lblContadorAlumnos.Name = "lblContadorAlumnos";
            this.lblContadorAlumnos.Size = new System.Drawing.Size(62, 15);
            this.lblContadorAlumnos.TabIndex = 8;
            this.lblContadorAlumnos.Text = "0 alumnos";
            // 
            // panelCuotas
            // 
            this.panelCuotas.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(255)))));
            this.panelCuotas.Controls.Add(this.lblTituloCuotas);
            this.panelCuotas.Controls.Add(this.lblAlumnoSeleccionado);
            this.panelCuotas.Controls.Add(this.lblNombreAlumno);
            this.panelCuotas.Controls.Add(this.gridCuotas);
            this.panelCuotas.Controls.Add(this.chkMostrarTodas);
            this.panelCuotas.Controls.Add(this.lblTotalSeleccionado);
            this.panelCuotas.Controls.Add(this.lblGenerar);
            this.panelCuotas.Controls.Add(this.lblMes);
            this.panelCuotas.Controls.Add(this.cmbMes);
            this.panelCuotas.Controls.Add(this.lblAnio);
            this.panelCuotas.Controls.Add(this.numAnio);
            this.panelCuotas.Controls.Add(this.btnGenerarCuota);
            this.panelCuotas.Controls.Add(this._panelConceptos);
            this.panelCuotas.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelCuotas.Location = new System.Drawing.Point(253, 0);
            this.panelCuotas.Name = "panelCuotas";
            this.panelCuotas.Padding = new System.Windows.Forms.Padding(12);
            this.panelCuotas.Size = new System.Drawing.Size(855, 711);
            this.panelCuotas.TabIndex = 0;
            // 
            // lblTituloCuotas
            // 
            this.lblTituloCuotas.AutoSize = true;
            this.lblTituloCuotas.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTituloCuotas.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.lblTituloCuotas.Location = new System.Drawing.Point(12, 10);
            this.lblTituloCuotas.Name = "lblTituloCuotas";
            this.lblTituloCuotas.Size = new System.Drawing.Size(57, 20);
            this.lblTituloCuotas.TabIndex = 0;
            this.lblTituloCuotas.Text = "Cuotas";
            // 
            // lblAlumnoSeleccionado
            // 
            this.lblAlumnoSeleccionado.AutoSize = true;
            this.lblAlumnoSeleccionado.ForeColor = System.Drawing.Color.Gray;
            this.lblAlumnoSeleccionado.Location = new System.Drawing.Point(12, 40);
            this.lblAlumnoSeleccionado.Name = "lblAlumnoSeleccionado";
            this.lblAlumnoSeleccionado.Size = new System.Drawing.Size(53, 15);
            this.lblAlumnoSeleccionado.TabIndex = 1;
            this.lblAlumnoSeleccionado.Text = "Alumno:";
            // 
            // lblNombreAlumno
            // 
            this.lblNombreAlumno.AutoSize = true;
            this.lblNombreAlumno.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblNombreAlumno.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.lblNombreAlumno.Location = new System.Drawing.Point(70, 38);
            this.lblNombreAlumno.Name = "lblNombreAlumno";
            this.lblNombreAlumno.Size = new System.Drawing.Size(164, 19);
            this.lblNombreAlumno.TabIndex = 2;
            this.lblNombreAlumno.Text = "(Seleccione un alumno)";
            // 
            // gridCuotas
            // 
            this.gridCuotas.AllowUserToAddRows = false;
            this.gridCuotas.AllowUserToDeleteRows = false;
            this.gridCuotas.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle3.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            dataGridViewCellStyle3.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle3.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle3.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle3.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle3.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridCuotas.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle3;
            this.gridCuotas.ColumnHeadersHeight = 30;
            this.gridCuotas.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dataGridViewCellStyle4.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle4.BackColor = System.Drawing.SystemColors.Window;
            dataGridViewCellStyle4.Font = new System.Drawing.Font("Segoe UI", 12F);
            dataGridViewCellStyle4.ForeColor = System.Drawing.SystemColors.ControlText;
            dataGridViewCellStyle4.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle4.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle4.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
            this.gridCuotas.DefaultCellStyle = dataGridViewCellStyle4;
            this.gridCuotas.EnableHeadersVisualStyles = false;
            this.gridCuotas.Location = new System.Drawing.Point(12, 90);
            this.gridCuotas.Name = "gridCuotas";
            this.gridCuotas.RowHeadersVisible = false;
            this.gridCuotas.RowTemplate.Height = 40;
            this.gridCuotas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridCuotas.Size = new System.Drawing.Size(575, 346);
            this.gridCuotas.TabIndex = 3;
            this.gridCuotas.CellDoubleClick += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridCuotas_CellDoubleClick);
            this.gridCuotas.CellValueChanged += new System.Windows.Forms.DataGridViewCellEventHandler(this.gridCuotas_CellValueChanged);
            this.gridCuotas.CurrentCellDirtyStateChanged += new System.EventHandler(this.gridCuotas_CurrentCellDirtyStateChanged);
            // 
            // chkMostrarTodas
            // 
            this.chkMostrarTodas.AutoSize = true;
            this.chkMostrarTodas.ForeColor = System.Drawing.Color.Gray;
            this.chkMostrarTodas.Location = new System.Drawing.Point(12, 67);
            this.chkMostrarTodas.Name = "chkMostrarTodas";
            this.chkMostrarTodas.Size = new System.Drawing.Size(216, 19);
            this.chkMostrarTodas.TabIndex = 4;
            this.chkMostrarTodas.Text = "Mostrar todas (incluyendo pagadas)";
            this.chkMostrarTodas.CheckedChanged += new System.EventHandler(this.chkMostrarTodas_CheckedChanged);
            // 
            // lblTotalSeleccionado
            // 
            this.lblTotalSeleccionado.AutoSize = true;
            this.lblTotalSeleccionado.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblTotalSeleccionado.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.lblTotalSeleccionado.Location = new System.Drawing.Point(15, 447);
            this.lblTotalSeleccionado.Name = "lblTotalSeleccionado";
            this.lblTotalSeleccionado.Size = new System.Drawing.Size(181, 19);
            this.lblTotalSeleccionado.TabIndex = 5;
            this.lblTotalSeleccionado.Text = "Total seleccionado: $ 0,00";
            // 
            // lblGenerar
            // 
            this.lblGenerar.AutoSize = true;
            this.lblGenerar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblGenerar.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.lblGenerar.Location = new System.Drawing.Point(15, 477);
            this.lblGenerar.Name = "lblGenerar";
            this.lblGenerar.Size = new System.Drawing.Size(90, 15);
            this.lblGenerar.TabIndex = 6;
            this.lblGenerar.Text = "Generar cuota:";
            // 
            // lblMes
            // 
            this.lblMes.AutoSize = true;
            this.lblMes.Location = new System.Drawing.Point(15, 502);
            this.lblMes.Name = "lblMes";
            this.lblMes.Size = new System.Drawing.Size(32, 15);
            this.lblMes.TabIndex = 7;
            this.lblMes.Text = "Mes:";
            // 
            // cmbMes
            // 
            this.cmbMes.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbMes.Location = new System.Drawing.Point(53, 499);
            this.cmbMes.Name = "cmbMes";
            this.cmbMes.Size = new System.Drawing.Size(140, 23);
            this.cmbMes.TabIndex = 8;
            // 
            // lblAnio
            // 
            this.lblAnio.AutoSize = true;
            this.lblAnio.Location = new System.Drawing.Point(203, 502);
            this.lblAnio.Name = "lblAnio";
            this.lblAnio.Size = new System.Drawing.Size(32, 15);
            this.lblAnio.TabIndex = 9;
            this.lblAnio.Text = "Año:";
            // 
            // numAnio
            // 
            this.numAnio.Location = new System.Drawing.Point(233, 499);
            this.numAnio.Maximum = new decimal(new int[] {
            2100,
            0,
            0,
            0});
            this.numAnio.Minimum = new decimal(new int[] {
            2020,
            0,
            0,
            0});
            this.numAnio.Name = "numAnio";
            this.numAnio.Size = new System.Drawing.Size(75, 23);
            this.numAnio.TabIndex = 10;
            this.numAnio.Value = new decimal(new int[] {
            2026,
            0,
            0,
            0});
            // 
            // btnGenerarCuota
            // 
            this.btnGenerarCuota.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnGenerarCuota.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGenerarCuota.FlatAppearance.BorderSize = 0;
            this.btnGenerarCuota.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerarCuota.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnGenerarCuota.ForeColor = System.Drawing.Color.White;
            this.btnGenerarCuota.Location = new System.Drawing.Point(318, 497);
            this.btnGenerarCuota.Name = "btnGenerarCuota";
            this.btnGenerarCuota.Size = new System.Drawing.Size(130, 27);
            this.btnGenerarCuota.TabIndex = 11;
            this.btnGenerarCuota.Text = "Generar Cuota";
            this.btnGenerarCuota.UseVisualStyleBackColor = false;
            this.btnGenerarCuota.Click += new System.EventHandler(this.btnGenerarCuota_Click);
            // 
            // _panelConceptos
            // 
            this._panelConceptos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(255)))));
            this._panelConceptos.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this._panelConceptos.Location = new System.Drawing.Point(596, 90);
            this._panelConceptos.Name = "_panelConceptos";
            this._panelConceptos.Size = new System.Drawing.Size(253, 346);
            this._panelConceptos.TabIndex = 12;
            // 
            // panelCobro
            // 
            this.panelCobro.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(250)))), ((int)(((byte)(252)))), ((int)(((byte)(255)))));
            this.panelCobro.Controls.Add(this.lblTituloCobro);
            this.panelCobro.Controls.Add(this.panelResumen);
            this.panelCobro.Controls.Add(this.lblMontoRecibido);
            this.panelCobro.Controls.Add(this.numMonto);
            this.panelCobro.Controls.Add(this.btnDistribuirMonto);
            //this.panelCobro.Controls.Add(this.lblAyudaDistribucion);
            this.panelCobro.Controls.Add(this.lblSepPagador);
            this.panelCobro.Controls.Add(this.lblPagador);
            this.panelCobro.Controls.Add(this.txtPagador);
            this.panelCobro.Controls.Add(this.rdnDNI);
            this.panelCobro.Controls.Add(this.rdnCUIT);
            this.panelCobro.Controls.Add(this.txtDocumento);
            this.panelCobro.Controls.Add(this.lblObservaciones);
            this.panelCobro.Controls.Add(this.txtObservaciones);
            this.panelCobro.Controls.Add(this.cmbFormaPago);
            this.panelCobro.Controls.Add(this.btnRegistrarPago);
            this.panelCobro.Controls.Add(this.btnImprimir);
            this.panelCobro.Controls.Add(this.lblFormaPago);
            this.panelCobro.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelCobro.Location = new System.Drawing.Point(1108, 0);
            this.panelCobro.Name = "panelCobro";
            this.panelCobro.Padding = new System.Windows.Forms.Padding(12);
            this.panelCobro.Size = new System.Drawing.Size(234, 711);
            this.panelCobro.TabIndex = 1;
            // 
            // lblTituloCobro
            // 
            this.lblTituloCobro.AutoSize = true;
            this.lblTituloCobro.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTituloCobro.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.lblTituloCobro.Location = new System.Drawing.Point(12, 10);
            this.lblTituloCobro.Name = "lblTituloCobro";
            this.lblTituloCobro.Size = new System.Drawing.Size(135, 20);
            this.lblTituloCobro.TabIndex = 0;
            this.lblTituloCobro.Text = "Registro de Cobro";
            // 
            // panelResumen
            // 
            this.panelResumen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(248)))), ((int)(((byte)(255)))));
            this.panelResumen.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelResumen.Controls.Add(this.lblEtiquetaTotalDeuda);
            this.panelResumen.Controls.Add(this.lblTotalDeuda);
            this.panelResumen.Controls.Add(this.lblEtiquetaVuelto);
            this.panelResumen.Controls.Add(this.lblVuelto);
            this.panelResumen.Controls.Add(this.lblEtiquetaDeuda);
            this.panelResumen.Controls.Add(this.lblDeuda);
            this.panelResumen.Location = new System.Drawing.Point(12, 40);
            this.panelResumen.Name = "panelResumen";
            this.panelResumen.Size = new System.Drawing.Size(215, 100);
            this.panelResumen.TabIndex = 1;
            // 
            // lblEtiquetaTotalDeuda
            // 
            this.lblEtiquetaTotalDeuda.AutoSize = true;
            this.lblEtiquetaTotalDeuda.ForeColor = System.Drawing.Color.Gray;
            this.lblEtiquetaTotalDeuda.Location = new System.Drawing.Point(8, 10);
            this.lblEtiquetaTotalDeuda.Name = "lblEtiquetaTotalDeuda";
            this.lblEtiquetaTotalDeuda.Size = new System.Drawing.Size(81, 15);
            this.lblEtiquetaTotalDeuda.TabIndex = 0;
            this.lblEtiquetaTotalDeuda.Text = "Total a cobrar:";
            // 
            // lblTotalDeuda
            // 
            this.lblTotalDeuda.AutoSize = true;
            this.lblTotalDeuda.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.lblTotalDeuda.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.lblTotalDeuda.Location = new System.Drawing.Point(103, 6);
            this.lblTotalDeuda.Name = "lblTotalDeuda";
            this.lblTotalDeuda.Size = new System.Drawing.Size(53, 20);
            this.lblTotalDeuda.TabIndex = 1;
            this.lblTotalDeuda.Text = "$ 0,00";
            // 
            // lblEtiquetaVuelto
            // 
            this.lblEtiquetaVuelto.AutoSize = true;
            this.lblEtiquetaVuelto.ForeColor = System.Drawing.Color.Gray;
            this.lblEtiquetaVuelto.Location = new System.Drawing.Point(8, 40);
            this.lblEtiquetaVuelto.Name = "lblEtiquetaVuelto";
            this.lblEtiquetaVuelto.Size = new System.Drawing.Size(44, 15);
            this.lblEtiquetaVuelto.TabIndex = 2;
            this.lblEtiquetaVuelto.Text = "Vuelto:";
            // 
            // lblVuelto
            // 
            this.lblVuelto.AutoSize = true;
            this.lblVuelto.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblVuelto.ForeColor = System.Drawing.Color.Gray;
            this.lblVuelto.Location = new System.Drawing.Point(107, 36);
            this.lblVuelto.Name = "lblVuelto";
            this.lblVuelto.Size = new System.Drawing.Size(49, 19);
            this.lblVuelto.TabIndex = 3;
            this.lblVuelto.Text = "$ 0,00";
            // 
            // lblEtiquetaDeuda
            // 
            this.lblEtiquetaDeuda.AutoSize = true;
            this.lblEtiquetaDeuda.ForeColor = System.Drawing.Color.Gray;
            this.lblEtiquetaDeuda.Location = new System.Drawing.Point(8, 68);
            this.lblEtiquetaDeuda.Name = "lblEtiquetaDeuda";
            this.lblEtiquetaDeuda.Size = new System.Drawing.Size(101, 15);
            this.lblEtiquetaDeuda.TabIndex = 4;
            this.lblEtiquetaDeuda.Text = "Queda adeudado:";
            // 
            // lblDeuda
            // 
            this.lblDeuda.AutoSize = true;
            this.lblDeuda.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.lblDeuda.ForeColor = System.Drawing.Color.Gray;
            this.lblDeuda.Location = new System.Drawing.Point(107, 66);
            this.lblDeuda.Name = "lblDeuda";
            this.lblDeuda.Size = new System.Drawing.Size(49, 19);
            this.lblDeuda.TabIndex = 5;
            this.lblDeuda.Text = "$ 0,00";
            // 
            // lblMontoRecibido
            // 
            this.lblMontoRecibido.AutoSize = true;
            this.lblMontoRecibido.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblMontoRecibido.Location = new System.Drawing.Point(12, 152);
            this.lblMontoRecibido.Name = "lblMontoRecibido";
            this.lblMontoRecibido.Size = new System.Drawing.Size(113, 15);
            this.lblMontoRecibido.TabIndex = 2;
            this.lblMontoRecibido.Text = "Monto recibido ($):";
            // 
            // numMonto
            // 
            this.numMonto.DecimalPlaces = 2;
            this.numMonto.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.numMonto.Increment = new decimal(new int[] {
            1000,
            0,
            0,
            0});
            this.numMonto.Location = new System.Drawing.Point(12, 172);
            this.numMonto.Maximum = new decimal(new int[] {
            9999999,
            0,
            0,
            0});
            this.numMonto.Name = "numMonto";
            this.numMonto.Size = new System.Drawing.Size(113, 29);
            this.numMonto.TabIndex = 3;
            this.numMonto.ValueChanged += new System.EventHandler(this.numMonto_ValueChanged);
            // 
            // btnDistribuirMonto
            // 
            this.btnDistribuirMonto.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(243)))), ((int)(((byte)(156)))), ((int)(((byte)(18)))));
            this.btnDistribuirMonto.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnDistribuirMonto.FlatAppearance.BorderSize = 0;
            this.btnDistribuirMonto.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnDistribuirMonto.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnDistribuirMonto.ForeColor = System.Drawing.Color.White;
            this.btnDistribuirMonto.Location = new System.Drawing.Point(134, 172);
            this.btnDistribuirMonto.Name = "btnDistribuirMonto";
            this.btnDistribuirMonto.Size = new System.Drawing.Size(93, 30);
            this.btnDistribuirMonto.TabIndex = 4;
            this.btnDistribuirMonto.Text = "⚡ Distribuir";
            this.btnDistribuirMonto.UseVisualStyleBackColor = false;
            this.btnDistribuirMonto.Click += new System.EventHandler(this.btnDistribuirMonto_Click);
            // 
            // lblAyudaDistribucion
            // 
            this.lblAyudaDistribucion.Font = new System.Drawing.Font("Segoe UI", 8F);
            this.lblAyudaDistribucion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(100)))), ((int)(((byte)(100)))), ((int)(((byte)(100)))));
            this.lblAyudaDistribucion.Location = new System.Drawing.Point(13, 226);
            this.lblAyudaDistribucion.Name = "lblAyudaDistribucion";
            this.lblAyudaDistribucion.Size = new System.Drawing.Size(235, 73);
            this.lblAyudaDistribucion.TabIndex = 5;
            this.lblAyudaDistribucion.Text = "↑ Ingresá el monto y presioná Distribuir para marcar\nautomáticamente las cuotas q" +
    "ue cubre (más antiguas primero).";
            // 
            // lblSepPagador
            // 
            this.lblSepPagador.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.lblSepPagador.Location = new System.Drawing.Point(12, 323);
            this.lblSepPagador.Name = "lblSepPagador";
            this.lblSepPagador.Size = new System.Drawing.Size(215, 10);
            this.lblSepPagador.TabIndex = 6;
            // 
            // lblPagador
            // 
            this.lblPagador.AutoSize = true;
            this.lblPagador.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblPagador.Location = new System.Drawing.Point(12, 340);
            this.lblPagador.Name = "lblPagador";
            this.lblPagador.Size = new System.Drawing.Size(124, 15);
            this.lblPagador.TabIndex = 7;
            this.lblPagador.Text = "Nombre del pagador:";
            // 
            // txtPagador
            // 
            this.txtPagador.Location = new System.Drawing.Point(12, 359);
            this.txtPagador.Name = "txtPagador";
            this.txtPagador.Size = new System.Drawing.Size(215, 23);
            this.txtPagador.TabIndex = 8;
            // 
            // rdnDNI
            // 
            this.rdnDNI.AutoSize = true;
            this.rdnDNI.Checked = true;
            this.rdnDNI.Location = new System.Drawing.Point(12, 390);
            this.rdnDNI.Name = "rdnDNI";
            this.rdnDNI.Size = new System.Drawing.Size(45, 19);
            this.rdnDNI.TabIndex = 9;
            this.rdnDNI.TabStop = true;
            this.rdnDNI.Text = "DNI";
            // 
            // rdnCUIT
            // 
            this.rdnCUIT.AutoSize = true;
            this.rdnCUIT.Location = new System.Drawing.Point(70, 390);
            this.rdnCUIT.Name = "rdnCUIT";
            this.rdnCUIT.Size = new System.Drawing.Size(50, 19);
            this.rdnCUIT.TabIndex = 10;
            this.rdnCUIT.Text = "CUIL";
            // 
            // txtDocumento
            // 
            this.txtDocumento.Location = new System.Drawing.Point(130, 387);
            this.txtDocumento.Name = "txtDocumento";
            this.txtDocumento.Size = new System.Drawing.Size(97, 23);
            this.txtDocumento.TabIndex = 11;
            // 
            // lblObservaciones
            // 
            this.lblObservaciones.AutoSize = true;
            this.lblObservaciones.ForeColor = System.Drawing.Color.Gray;
            this.lblObservaciones.Location = new System.Drawing.Point(12, 451);
            this.lblObservaciones.Name = "lblObservaciones";
            this.lblObservaciones.Size = new System.Drawing.Size(87, 15);
            this.lblObservaciones.TabIndex = 12;
            this.lblObservaciones.Text = "Observaciones:";
            // 
            // txtObservaciones
            // 
            this.txtObservaciones.Location = new System.Drawing.Point(12, 469);
            this.txtObservaciones.Multiline = true;
            this.txtObservaciones.Name = "txtObservaciones";
            this.txtObservaciones.Size = new System.Drawing.Size(215, 30);
            this.txtObservaciones.TabIndex = 13;
            // 
            // cmbFormaPago
            // 
            this.cmbFormaPago.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbFormaPago.Items.AddRange(new object[] {
            "Efectivo",
            "Transferencia",
            "Cheque"});
            this.cmbFormaPago.Location = new System.Drawing.Point(120, 418);
            this.cmbFormaPago.Name = "cmbFormaPago";
            this.cmbFormaPago.Size = new System.Drawing.Size(95, 23);
            this.cmbFormaPago.TabIndex = 14;
            // 
            // btnRegistrarPago
            // 
            this.btnRegistrarPago.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnRegistrarPago.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnRegistrarPago.FlatAppearance.BorderSize = 0;
            this.btnRegistrarPago.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnRegistrarPago.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Bold);
            this.btnRegistrarPago.ForeColor = System.Drawing.Color.White;
            this.btnRegistrarPago.Location = new System.Drawing.Point(12, 513);
            this.btnRegistrarPago.Name = "btnRegistrarPago";
            this.btnRegistrarPago.Size = new System.Drawing.Size(215, 38);
            this.btnRegistrarPago.TabIndex = 15;
            this.btnRegistrarPago.Text = "✔ Registrar Cobro";
            this.btnRegistrarPago.UseVisualStyleBackColor = false;
            this.btnRegistrarPago.Click += new System.EventHandler(this.btnRegistrarPago_Click);
            // 
            // btnImprimir
            // 
            this.btnImprimir.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnImprimir.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnImprimir.FlatAppearance.BorderSize = 0;
            this.btnImprimir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImprimir.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnImprimir.ForeColor = System.Drawing.Color.White;
            this.btnImprimir.Location = new System.Drawing.Point(12, 567);
            this.btnImprimir.Name = "btnImprimir";
            this.btnImprimir.Size = new System.Drawing.Size(215, 32);
            this.btnImprimir.TabIndex = 16;
            this.btnImprimir.Text = "🖨 Imprimir Último Comprobante";
            this.btnImprimir.UseVisualStyleBackColor = false;
            this.btnImprimir.Click += new System.EventHandler(this.btnImprimir_Click);
            // 
            // lblFormaPago
            // 
            this.lblFormaPago.AutoSize = true;
            this.lblFormaPago.ForeColor = System.Drawing.Color.Gray;
            this.lblFormaPago.Location = new System.Drawing.Point(12, 421);
            this.lblFormaPago.Name = "lblFormaPago";
            this.lblFormaPago.Size = new System.Drawing.Size(90, 15);
            this.lblFormaPago.TabIndex = 17;
            this.lblFormaPago.Text = "Forma de pago:";
            // 
            // panelSeparador1
            // 
            this.panelSeparador1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.panelSeparador1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelSeparador1.Location = new System.Drawing.Point(0, 0);
            this.panelSeparador1.Name = "panelSeparador1";
            this.panelSeparador1.Size = new System.Drawing.Size(2, 711);
            this.panelSeparador1.TabIndex = 4;
            // 
            // panelSeparador2
            // 
            this.panelSeparador2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.panelSeparador2.Dock = System.Windows.Forms.DockStyle.Right;
            this.panelSeparador2.Location = new System.Drawing.Point(1342, 0);
            this.panelSeparador2.Name = "panelSeparador2";
            this.panelSeparador2.Size = new System.Drawing.Size(2, 711);
            this.panelSeparador2.TabIndex = 2;
            // 
            // PagosForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.ClientSize = new System.Drawing.Size(1344, 711);
            this.Controls.Add(this.panelCuotas);
            this.Controls.Add(this.panelCobro);
            this.Controls.Add(this.panelSeparador2);
            this.Controls.Add(this.panelAlumnos);
            this.Controls.Add(this.panelSeparador1);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.Name = "PagosForm";
            this.Text = "Registro de Cobros";
            this.Load += new System.EventHandler(this.PagosForm_Load);
            this.panelAlumnos.ResumeLayout(false);
            this.panelAlumnos.PerformLayout();
            this.panelPadre.ResumeLayout(false);
            this.panelPadre.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridAlumnos)).EndInit();
            this.panelCuotas.ResumeLayout(false);
            this.panelCuotas.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridCuotas)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.numAnio)).EndInit();
            this.panelCobro.ResumeLayout(false);
            this.panelCobro.PerformLayout();
            this.panelResumen.ResumeLayout(false);
            this.panelResumen.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numMonto)).EndInit();
            this.ResumeLayout(false);

        }

        private System.Windows.Forms.Label lblFormaPago;
    }
}
