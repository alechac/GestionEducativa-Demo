using System;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    partial class InformesForm
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



        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.panelTipos = new System.Windows.Forms.Panel();
            this.lblMenuTit = new System.Windows.Forms.Label();
            this.panelMain = new System.Windows.Forms.Panel();
            this.panelDatos = new System.Windows.Forms.Panel();
            this.gridInforme = new System.Windows.Forms.DataGridView();
            this.panelBotones = new System.Windows.Forms.Panel();
            this.btnGenerar = new System.Windows.Forms.Button();
            this.btnExportar = new System.Windows.Forms.Button();
            this.btnImprimir = new System.Windows.Forms.Button();
            this.panelResumenCaja = new System.Windows.Forms.Panel();
            this.lblLCant = new System.Windows.Forms.Label();
            this.lblLCob = new System.Windows.Forms.Label();
            this.lblLRec = new System.Windows.Forms.Label();
            this.lblLVto = new System.Windows.Forms.Label();
            this.lblLGastos = new System.Windows.Forms.Label();
            this.lblLNeto = new System.Windows.Forms.Label();
            this.lblCajaCantPagos = new System.Windows.Forms.Label();
            this.lblCajaCobrado = new System.Windows.Forms.Label();
            this.lblCajaRecibido = new System.Windows.Forms.Label();
            this.lblCajaVuelto = new System.Windows.Forms.Label();
            this.lblCajaGastos = new System.Windows.Forms.Label();
            this.lblCajaNeto = new System.Windows.Forms.Label();
            this.panelFiltros = new System.Windows.Forms.Panel();
            this.numAnio = new System.Windows.Forms.NumericUpDown();
            this.cmbMesFiltro = new System.Windows.Forms.ComboBox();
            this.cmbNivel = new System.Windows.Forms.ComboBox();
            this.cmbDivision = new System.Windows.Forms.ComboBox();
            this.dtpDesde = new System.Windows.Forms.DateTimePicker();
            this.dtpHasta = new System.Windows.Forms.DateTimePicker();
            this.cmbAlumno = new System.Windows.Forms.ComboBox();
            this.txtBuscarPadre = new System.Windows.Forms.TextBox();
            this.txtDocPadre = new System.Windows.Forms.TextBox();
            this.chkColNombre = new System.Windows.Forms.CheckBox();
            this.chkColNivel = new System.Windows.Forms.CheckBox();
            this.chkColDivision = new System.Windows.Forms.CheckBox();
            this.chkColDni = new System.Windows.Forms.CheckBox();
            this.chkColDescuento = new System.Windows.Forms.CheckBox();
            this.chkColSaldo = new System.Windows.Forms.CheckBox();
            this.chkColPadre = new System.Windows.Forms.CheckBox();
            this.chkColTelefono = new System.Windows.Forms.CheckBox();
            this.panelTipos.SuspendLayout();
            this.panelMain.SuspendLayout();
            this.panelDatos.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.gridInforme)).BeginInit();
            this.panelBotones.SuspendLayout();
            this.panelResumenCaja.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAnio)).BeginInit();
            this.SuspendLayout();
            // 
            // panelTipos
            // 
            this.panelTipos.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.panelTipos.Controls.Add(this.lblMenuTit);
            this.panelTipos.Dock = System.Windows.Forms.DockStyle.Left;
            this.panelTipos.Location = new System.Drawing.Point(0, 0);
            this.panelTipos.Name = "panelTipos";
            this.panelTipos.Padding = new System.Windows.Forms.Padding(0, 10, 0, 0);
            this.panelTipos.Size = new System.Drawing.Size(200, 641);
            this.panelTipos.TabIndex = 1;
            // 
            // lblMenuTit
            // 
            this.lblMenuTit.BackColor = System.Drawing.Color.Transparent;
            this.lblMenuTit.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblMenuTit.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.lblMenuTit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(174)))), ((int)(((byte)(214)))), ((int)(((byte)(241)))));
            this.lblMenuTit.Location = new System.Drawing.Point(0, 10);
            this.lblMenuTit.Name = "lblMenuTit";
            this.lblMenuTit.Size = new System.Drawing.Size(200, 32);
            this.lblMenuTit.TabIndex = 0;
            this.lblMenuTit.Text = "INFORMES";
            this.lblMenuTit.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panelMain
            // 
            this.panelMain.BackColor = System.Drawing.Color.Transparent;
            this.panelMain.Controls.Add(this.panelDatos);
            this.panelMain.Controls.Add(this.panelBotones);
            this.panelMain.Controls.Add(this.panelResumenCaja);
            this.panelMain.Controls.Add(this.panelFiltros);
            this.panelMain.Dock = System.Windows.Forms.DockStyle.None;
            this.panelMain.Location = new System.Drawing.Point(200, 0);
            this.panelMain.Name = "panelMain";
            this.panelMain.Padding = new System.Windows.Forms.Padding(12);
            this.panelMain.Size = new System.Drawing.Size(984, 541);
            this.panelMain.TabIndex = 0;
            // 
            // panelDatos
            // 
            this.panelDatos.Controls.Add(this.gridInforme);
            this.panelDatos.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panelDatos.Location = new System.Drawing.Point(12, 198);
            this.panelDatos.Name = "panelDatos";
            this.panelDatos.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.panelDatos.Size = new System.Drawing.Size(900, 300);
            this.panelDatos.TabIndex = 0;
            // 
            // gridInforme
            // 
            this.gridInforme.AllowUserToAddRows = false;
            this.gridInforme.AllowUserToDeleteRows = false;
            this.gridInforme.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.AllCells;
            this.gridInforme.BackgroundColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            dataGridViewCellStyle2.Font = new System.Drawing.Font("Segoe UI", 9F);
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;
            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridViewCellStyle2.WrapMode = System.Windows.Forms.DataGridViewTriState.True;
            this.gridInforme.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;
            this.gridInforme.ColumnHeadersHeight = 30;
            this.gridInforme.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.gridInforme.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gridInforme.EnableHeadersVisualStyles = false;
            this.gridInforme.Location = new System.Drawing.Point(0, 6);
            this.gridInforme.Name = "gridInforme";
            this.gridInforme.ReadOnly = true;
            this.gridInforme.RowHeadersVisible = false;
            this.gridInforme.RowTemplate.Height = 26;
            this.gridInforme.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            this.gridInforme.Size = new System.Drawing.Size(860, 425);
            this.gridInforme.TabIndex = 0;
            // 
            // panelBotones
            // 
            this.panelBotones.BackColor = System.Drawing.Color.Transparent;
            this.panelBotones.Controls.Add(this.btnGenerar);
            this.panelBotones.Controls.Add(this.btnExportar);
            this.panelBotones.Controls.Add(this.btnImprimir);
            this.panelBotones.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelBotones.Location = new System.Drawing.Point(12, 152);
            this.panelBotones.Name = "panelBotones";
            this.panelBotones.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
            this.panelBotones.Size = new System.Drawing.Size(860, 46);
            this.panelBotones.TabIndex = 1;
            // 
            // btnGenerar
            // 
            this.btnGenerar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnGenerar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGenerar.FlatAppearance.BorderSize = 0;
            this.btnGenerar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGenerar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnGenerar.ForeColor = System.Drawing.Color.White;
            this.btnGenerar.Location = new System.Drawing.Point(0, 0);
            this.btnGenerar.Name = "btnGenerar";
            this.btnGenerar.Size = new System.Drawing.Size(130, 34);
            this.btnGenerar.TabIndex = 0;
            this.btnGenerar.Text = "🔍 Generar";
            this.btnGenerar.UseVisualStyleBackColor = false;
            this.btnGenerar.Click += new System.EventHandler(this.btnGenerar_Click);
            // 
            // btnExportar
            // 
            this.btnExportar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(52)))), ((int)(((byte)(152)))), ((int)(((byte)(219)))));
            this.btnExportar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnExportar.FlatAppearance.BorderSize = 0;
            this.btnExportar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnExportar.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnExportar.ForeColor = System.Drawing.Color.White;
            this.btnExportar.Location = new System.Drawing.Point(145, 0);
            this.btnExportar.Name = "btnExportar";
            this.btnExportar.Size = new System.Drawing.Size(130, 34);
            this.btnExportar.TabIndex = 1;
            this.btnExportar.Text = "📥 Exportar CSV";
            this.btnExportar.UseVisualStyleBackColor = false;
            this.btnExportar.Click += new System.EventHandler(this.btnExportar_Click);
            // 
            // btnImprimir
            // 
            this.btnImprimir.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(142)))), ((int)(((byte)(68)))), ((int)(((byte)(173)))));
            this.btnImprimir.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnImprimir.FlatAppearance.BorderSize = 0;
            this.btnImprimir.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImprimir.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold);
            this.btnImprimir.ForeColor = System.Drawing.Color.White;
            this.btnImprimir.Location = new System.Drawing.Point(290, 0);
            this.btnImprimir.Name = "btnImprimir";
            this.btnImprimir.Size = new System.Drawing.Size(145, 34);
            this.btnImprimir.TabIndex = 2;
            this.btnImprimir.Text = "🖨 Imprimir HTML";
            this.btnImprimir.UseVisualStyleBackColor = false;
            this.btnImprimir.Click += new System.EventHandler(this.btnImprimir_Click);
            // 
            // panelResumenCaja
            // 
            this.panelResumenCaja.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(240)))), ((int)(((byte)(255)))), ((int)(((byte)(240)))));
            this.panelResumenCaja.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelResumenCaja.Controls.Add(this.lblLCant);
            this.panelResumenCaja.Controls.Add(this.lblLCob);
            this.panelResumenCaja.Controls.Add(this.lblLRec);
            this.panelResumenCaja.Controls.Add(this.lblLVto);
            this.panelResumenCaja.Controls.Add(this.lblLGastos);
            this.panelResumenCaja.Controls.Add(this.lblLNeto);
            this.panelResumenCaja.Controls.Add(this.lblCajaCantPagos);
            this.panelResumenCaja.Controls.Add(this.lblCajaCobrado);
            this.panelResumenCaja.Controls.Add(this.lblCajaRecibido);
            this.panelResumenCaja.Controls.Add(this.lblCajaVuelto);
            this.panelResumenCaja.Controls.Add(this.lblCajaGastos);
            this.panelResumenCaja.Controls.Add(this.lblCajaNeto);
            this.panelResumenCaja.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelResumenCaja.Location = new System.Drawing.Point(12, 82);
            this.panelResumenCaja.Name = "panelResumenCaja";
            this.panelResumenCaja.Padding = new System.Windows.Forms.Padding(4);
            this.panelResumenCaja.Size = new System.Drawing.Size(860, 108);
            this.panelResumenCaja.TabIndex = 2;
            this.panelResumenCaja.Visible = false;
            // 
            // lblLCant
            // 
            this.lblLCant.AutoSize = true;
            this.lblLCant.ForeColor = System.Drawing.Color.Gray;
            this.lblLCant.Location = new System.Drawing.Point(12, 10);
            this.lblLCant.Name = "lblLCant";
            this.lblLCant.Size = new System.Drawing.Size(42, 15);
            this.lblLCant.TabIndex = 0;
            this.lblLCant.Text = "Pagos:";
            // 
            // lblLCob
            // 
            this.lblLCob.AutoSize = true;
            this.lblLCob.ForeColor = System.Drawing.Color.Gray;
            this.lblLCob.Location = new System.Drawing.Point(232, 10);
            this.lblLCob.Name = "lblLCob";
            this.lblLCob.Size = new System.Drawing.Size(85, 15);
            this.lblLCob.TabIndex = 1;
            this.lblLCob.Text = "Total Cobrado:";
            // 
            // lblLRec
            // 
            this.lblLRec.AutoSize = true;
            this.lblLRec.ForeColor = System.Drawing.Color.Gray;
            this.lblLRec.Location = new System.Drawing.Point(452, 10);
            this.lblLRec.Name = "lblLRec";
            this.lblLRec.Size = new System.Drawing.Size(101, 15);
            this.lblLRec.TabIndex = 2;
            this.lblLRec.Text = "Efectivo Recibido:";
            // 
            // lblLVto
            // 
            this.lblLVto.AutoSize = true;
            this.lblLVto.ForeColor = System.Drawing.Color.Gray;
            this.lblLVto.Location = new System.Drawing.Point(672, 10);
            this.lblLVto.Name = "lblLVto";
            this.lblLVto.Size = new System.Drawing.Size(44, 15);
            this.lblLVto.TabIndex = 3;
            this.lblLVto.Text = "Vuelto:";
            // 
            // lblLGastos
            // 
            this.lblLGastos.AutoSize = true;
            this.lblLGastos.ForeColor = System.Drawing.Color.Gray;
            this.lblLGastos.Location = new System.Drawing.Point(12, 48);
            this.lblLGastos.Name = "lblLGastos";
            this.lblLGastos.Size = new System.Drawing.Size(85, 15);
            this.lblLGastos.TabIndex = 8;
            this.lblLGastos.Text = "Gastos del día:";
            // 
            // lblLNeto
            // 
            this.lblLNeto.AutoSize = true;
            this.lblLNeto.ForeColor = System.Drawing.Color.Gray;
            this.lblLNeto.Location = new System.Drawing.Point(232, 48);
            this.lblLNeto.Name = "lblLNeto";
            this.lblLNeto.Size = new System.Drawing.Size(140, 15);
            this.lblLNeto.TabIndex = 9;
            this.lblLNeto.Text = "Neto (Cobrado - Gastos):";
            // 
            // lblCajaCantPagos
            // 
            this.lblCajaCantPagos.AutoSize = true;
            this.lblCajaCantPagos.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCajaCantPagos.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.lblCajaCantPagos.Location = new System.Drawing.Point(12, 30);
            this.lblCajaCantPagos.Name = "lblCajaCantPagos";
            this.lblCajaCantPagos.Size = new System.Drawing.Size(19, 21);
            this.lblCajaCantPagos.TabIndex = 4;
            this.lblCajaCantPagos.Text = "0";
            // 
            // lblCajaCobrado
            // 
            this.lblCajaCobrado.AutoSize = true;
            this.lblCajaCobrado.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCajaCobrado.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.lblCajaCobrado.Location = new System.Drawing.Point(232, 30);
            this.lblCajaCobrado.Name = "lblCajaCobrado";
            this.lblCajaCobrado.Size = new System.Drawing.Size(54, 21);
            this.lblCajaCobrado.TabIndex = 5;
            this.lblCajaCobrado.Text = "$ 0,00";
            // 
            // lblCajaRecibido
            // 
            this.lblCajaRecibido.AutoSize = true;
            this.lblCajaRecibido.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCajaRecibido.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.lblCajaRecibido.Location = new System.Drawing.Point(452, 30);
            this.lblCajaRecibido.Name = "lblCajaRecibido";
            this.lblCajaRecibido.Size = new System.Drawing.Size(54, 21);
            this.lblCajaRecibido.TabIndex = 6;
            this.lblCajaRecibido.Text = "$ 0,00";
            // 
            // lblCajaVuelto
            // 
            this.lblCajaVuelto.AutoSize = true;
            this.lblCajaVuelto.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCajaVuelto.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.lblCajaVuelto.Location = new System.Drawing.Point(672, 30);
            this.lblCajaVuelto.Name = "lblCajaVuelto";
            this.lblCajaVuelto.Size = new System.Drawing.Size(54, 21);
            this.lblCajaVuelto.TabIndex = 7;
            this.lblCajaVuelto.Text = "$ 0,00";
            // 
            // lblCajaGastos
            // 
            this.lblCajaGastos.AutoSize = true;
            this.lblCajaGastos.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCajaGastos.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(57)))), ((int)(((byte)(43)))));
            this.lblCajaGastos.Location = new System.Drawing.Point(12, 68);
            this.lblCajaGastos.Name = "lblCajaGastos";
            this.lblCajaGastos.Size = new System.Drawing.Size(54, 21);
            this.lblCajaGastos.TabIndex = 10;
            this.lblCajaGastos.Text = "$ 0,00";
            // 
            // lblCajaNeto
            // 
            this.lblCajaNeto.AutoSize = true;
            this.lblCajaNeto.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Bold);
            this.lblCajaNeto.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.lblCajaNeto.Location = new System.Drawing.Point(232, 68);
            this.lblCajaNeto.Name = "lblCajaNeto";
            this.lblCajaNeto.Size = new System.Drawing.Size(54, 21);
            this.lblCajaNeto.TabIndex = 11;
            this.lblCajaNeto.Text = "$ 0,00";
            // 
            // panelFiltros
            // 
            this.panelFiltros.BackColor = System.Drawing.Color.White;
            this.panelFiltros.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panelFiltros.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelFiltros.Location = new System.Drawing.Point(12, 12);
            this.panelFiltros.Name = "panelFiltros";
            this.panelFiltros.Padding = new System.Windows.Forms.Padding(4);
            this.panelFiltros.Size = new System.Drawing.Size(860, 70);
            this.panelFiltros.TabIndex = 3;
            // 
            // numAnio
            // 
            this.numAnio.Location = new System.Drawing.Point(-500, -500);
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
            this.numAnio.Size = new System.Drawing.Size(80, 20);
            this.numAnio.TabIndex = 0;
            this.numAnio.Value = new decimal(new int[] {
            2026,
            0,
            0,
            0});
            // 
            // cmbMesFiltro
            // 
            this.cmbMesFiltro.Location = new System.Drawing.Point(0, 0);
            this.cmbMesFiltro.Name = "cmbMesFiltro";
            this.cmbMesFiltro.Size = new System.Drawing.Size(121, 21);
            this.cmbMesFiltro.TabIndex = 0;
            // 
            // cmbNivel
            // 
            this.cmbNivel.Location = new System.Drawing.Point(0, 0);
            this.cmbNivel.Name = "cmbNivel";
            this.cmbNivel.Size = new System.Drawing.Size(121, 21);
            this.cmbNivel.TabIndex = 0;
            // 
            // cmbDivision
            // 
            this.cmbDivision.Location = new System.Drawing.Point(0, 0);
            this.cmbDivision.Name = "cmbDivision";
            this.cmbDivision.Size = new System.Drawing.Size(121, 21);
            this.cmbDivision.TabIndex = 0;
            // 
            // dtpDesde
            // 
            this.dtpDesde.Location = new System.Drawing.Point(0, 0);
            this.dtpDesde.Name = "dtpDesde";
            this.dtpDesde.Size = new System.Drawing.Size(200, 20);
            this.dtpDesde.TabIndex = 0;
            // 
            // dtpHasta
            // 
            this.dtpHasta.Location = new System.Drawing.Point(0, 0);
            this.dtpHasta.Name = "dtpHasta";
            this.dtpHasta.Size = new System.Drawing.Size(200, 20);
            this.dtpHasta.TabIndex = 0;
            // 
            // cmbAlumno
            // 
            this.cmbAlumno.Location = new System.Drawing.Point(0, 0);
            this.cmbAlumno.Name = "cmbAlumno";
            this.cmbAlumno.Size = new System.Drawing.Size(121, 21);
            this.cmbAlumno.TabIndex = 0;
            // 
            // txtBuscarPadre
            // 
            this.txtBuscarPadre.Location = new System.Drawing.Point(0, 0);
            this.txtBuscarPadre.Name = "txtBuscarPadre";
            this.txtBuscarPadre.Size = new System.Drawing.Size(100, 20);
            this.txtBuscarPadre.TabIndex = 0;
            // 
            // txtDocPadre
            // 
            this.txtDocPadre.Location = new System.Drawing.Point(0, 0);
            this.txtDocPadre.Name = "txtDocPadre";
            this.txtDocPadre.Size = new System.Drawing.Size(100, 20);
            this.txtDocPadre.TabIndex = 0;
            // 
            // chkColNombre
            // 
            this.chkColNombre.Location = new System.Drawing.Point(0, 0);
            this.chkColNombre.Name = "chkColNombre";
            this.chkColNombre.Size = new System.Drawing.Size(104, 24);
            this.chkColNombre.TabIndex = 0;
            // 
            // chkColNivel
            // 
            this.chkColNivel.Location = new System.Drawing.Point(0, 0);
            this.chkColNivel.Name = "chkColNivel";
            this.chkColNivel.Size = new System.Drawing.Size(104, 24);
            this.chkColNivel.TabIndex = 0;
            // 
            // chkColDivision
            // 
            this.chkColDivision.Location = new System.Drawing.Point(0, 0);
            this.chkColDivision.Name = "chkColDivision";
            this.chkColDivision.Size = new System.Drawing.Size(104, 24);
            this.chkColDivision.TabIndex = 0;
            // 
            // chkColDni
            // 
            this.chkColDni.Location = new System.Drawing.Point(0, 0);
            this.chkColDni.Name = "chkColDni";
            this.chkColDni.Size = new System.Drawing.Size(104, 24);
            this.chkColDni.TabIndex = 0;
            // 
            // chkColDescuento
            // 
            this.chkColDescuento.Location = new System.Drawing.Point(0, 0);
            this.chkColDescuento.Name = "chkColDescuento";
            this.chkColDescuento.Size = new System.Drawing.Size(104, 24);
            this.chkColDescuento.TabIndex = 0;
            // 
            // chkColSaldo
            // 
            this.chkColSaldo.Location = new System.Drawing.Point(0, 0);
            this.chkColSaldo.Name = "chkColSaldo";
            this.chkColSaldo.Size = new System.Drawing.Size(104, 24);
            this.chkColSaldo.TabIndex = 0;
            // 
            // chkColPadre
            // 
            this.chkColPadre.Location = new System.Drawing.Point(0, 0);
            this.chkColPadre.Name = "chkColPadre";
            this.chkColPadre.Size = new System.Drawing.Size(104, 24);
            this.chkColPadre.TabIndex = 0;
            // 
            // chkColTelefono
            // 
            this.chkColTelefono.Location = new System.Drawing.Point(0, 0);
            this.chkColTelefono.Name = "chkColTelefono";
            this.chkColTelefono.Size = new System.Drawing.Size(104, 24);
            this.chkColTelefono.TabIndex = 0;
            // 
            // InformesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(236)))), ((int)(((byte)(240)))), ((int)(((byte)(241)))));
            this.ClientSize = new System.Drawing.Size(1084, 641);
            this.Controls.Add(this.panelMain);
            this.Controls.Add(this.panelTipos);
            this.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.MinimumSize = new System.Drawing.Size(1100, 680);
            this.Name = "InformesForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Informes y Estadísticas";
            this.panelTipos.ResumeLayout(false);
            this.panelMain.ResumeLayout(false);
            this.panelDatos.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.gridInforme)).EndInit();
            this.panelBotones.ResumeLayout(false);
            this.panelResumenCaja.ResumeLayout(false);
            this.panelResumenCaja.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numAnio)).EndInit();
            this.ResumeLayout(false);

        }

        // ── Event handlers del Designer ────────────────────────────────
        private void btnGenerar_Click(object sender, EventArgs e)
            => GenerarInformeClicked?.Invoke(this, e);

        private void btnExportar_Click(object sender, EventArgs e)
            => ExportarExcelClicked?.Invoke(this, e);

        private void btnImprimir_Click(object sender, EventArgs e)
            => ImprimirClicked?.Invoke(this, e);

        // ── Declaraciones de campos ────────────────────────────────────
        private System.Windows.Forms.Panel panelTipos;
        private System.Windows.Forms.Label lblMenuTit;
        private System.Windows.Forms.Panel panelMain;
        private System.Windows.Forms.Panel panelFiltros;
        private System.Windows.Forms.Panel panelResumenCaja;
        private System.Windows.Forms.Panel panelBotones;
        private System.Windows.Forms.Panel panelDatos;
        private System.Windows.Forms.DataGridView gridInforme;
        private System.Windows.Forms.Button btnGenerar;
        private System.Windows.Forms.Button btnExportar;
        private System.Windows.Forms.Button btnImprimir;
        private System.Windows.Forms.Label lblCajaCantPagos;
        private System.Windows.Forms.Label lblCajaCobrado;
        private System.Windows.Forms.Label lblCajaRecibido;
        private System.Windows.Forms.Label lblCajaVuelto;
        private System.Windows.Forms.Label lblCajaGastos;
        private System.Windows.Forms.Label lblCajaNeto;
        private Label lblLCant;
        private Label lblLCob;
        private Label lblLRec;
        private Label lblLVto;
        private Label lblLGastos;
        private Label lblLNeto;
        private System.Windows.Forms.NumericUpDown numAnio;
        private System.Windows.Forms.ComboBox cmbMesFiltro;
        private System.Windows.Forms.ComboBox cmbNivel;
        private System.Windows.Forms.ComboBox cmbNivelEducativo;
        private System.Windows.Forms.ComboBox cmbDivision;
        private System.Windows.Forms.DateTimePicker dtpDesde;
        private System.Windows.Forms.DateTimePicker dtpHasta;
        private System.Windows.Forms.ComboBox cmbAlumno;
        private System.Windows.Forms.TextBox txtBuscarPadre;
        private System.Windows.Forms.TextBox txtDocPadre;
        private System.Windows.Forms.CheckBox chkColNombre;
        private System.Windows.Forms.CheckBox chkColNivel;
        private System.Windows.Forms.CheckBox chkColDivision;
        private System.Windows.Forms.CheckBox chkColDni;
        private System.Windows.Forms.CheckBox chkColDescuento;
        private System.Windows.Forms.CheckBox chkColSaldo;
        private System.Windows.Forms.CheckBox chkColPadre;
        private System.Windows.Forms.CheckBox chkColDniPadre;
        private System.Windows.Forms.CheckBox chkColTelefono;

        private System.Windows.Forms.CheckBox chkColMailPadre;
    }
}
