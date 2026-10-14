using GestionEducativa.Models;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    partial class NuevoAlumnoForm
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
            this.lblNombre = new System.Windows.Forms.Label();
            this.lblDni = new System.Windows.Forms.Label();
            this.lblApellido = new System.Windows.Forms.Label();
            this.lblNivel = new System.Windows.Forms.Label();
            this.lblDivision = new System.Windows.Forms.Label();
            this.lblDescuento = new System.Windows.Forms.Label();
            this.lblDescNota = new System.Windows.Forms.Label();
            this.lblFechaNac = new System.Windows.Forms.Label();
            this.txtNombre = new System.Windows.Forms.TextBox();
            this.txtDni = new System.Windows.Forms.TextBox();
            this.txtDescuento = new System.Windows.Forms.TextBox();
            this.txtDivision = new System.Windows.Forms.TextBox();
            this.cmbNivel = new System.Windows.Forms.ComboBox();
            this.dtpFechaNacimiento = new System.Windows.Forms.DateTimePicker();
            this.chkTieneFechaNac = new System.Windows.Forms.CheckBox();
            this.lblIngresoFuturo = new System.Windows.Forms.Label();
            this.chkIngresoFuturo = new System.Windows.Forms.CheckBox();
            this.lblIngresoFuturoNota = new System.Windows.Forms.Label();
            this.btnGuardar = new System.Windows.Forms.Button();
            this.btnCancelar = new System.Windows.Forms.Button();
            this.panelHeader = new System.Windows.Forms.Panel();
            this.lblTitulo = new System.Windows.Forms.Label();
            this.panelHeader.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblNombre
            // 
            this.lblNombre.AutoSize = true;
            this.lblNombre.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.lblNombre.Location = new System.Drawing.Point(20, 72);
            this.lblNombre.Name = "lblNombre";
            this.lblNombre.Size = new System.Drawing.Size(122, 17);
            this.lblNombre.TabIndex = 1;
            this.lblNombre.Text = "Apellido y Nombre:";
            // 
            // lblApellido
            // 
            this.lblApellido.AutoSize = true;
            this.lblApellido.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.lblApellido.Location = new System.Drawing.Point(20, 148);
            this.lblApellido.Name = "lblApellido";
            this.lblApellido.Size = new System.Drawing.Size(0, 17);
            this.lblApellido.TabIndex = 3;
            // 
            // lblNivel
            // 
            this.lblNivel.AutoSize = true;
            this.lblNivel.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.lblNivel.Location = new System.Drawing.Point(20, 152);
            this.lblNivel.Name = "lblNivel";
            this.lblNivel.Size = new System.Drawing.Size(40, 17);
            this.lblNivel.TabIndex = 5;
            this.lblNivel.Text = "Nivel:";
            // 
            // lblDivision
            // 
            this.lblDivision.AutoSize = true;
            this.lblDivision.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.lblDivision.Location = new System.Drawing.Point(20, 190);
            this.lblDivision.Name = "lblDivision";
            this.lblDivision.Size = new System.Drawing.Size(56, 17);
            this.lblDivision.TabIndex = 7;
            this.lblDivision.Text = "División:";
            // 
            // lblDescuento
            // 
            this.lblDescuento.AutoSize = true;
            this.lblDescuento.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.lblDescuento.Location = new System.Drawing.Point(20, 228);
            this.lblDescuento.Name = "lblDescuento";
            this.lblDescuento.Size = new System.Drawing.Size(87, 17);
            this.lblDescuento.TabIndex = 9;
            this.lblDescuento.Text = "% Descuento:";
            // 
            // lblDescNota
            // 
            this.lblDescNota.AutoSize = true;
            this.lblDescNota.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.lblDescNota.ForeColor = System.Drawing.Color.Gray;
            this.lblDescNota.Location = new System.Drawing.Point(228, 228);
            this.lblDescNota.Name = "lblDescNota";
            this.lblDescNota.Size = new System.Drawing.Size(52, 15);
            this.lblDescNota.TabIndex = 11;
            this.lblDescNota.Text = "(0 a 100)";
            // 
            // lblFechaNac
            // 
            this.lblFechaNac.AutoSize = true;
            this.lblFechaNac.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.lblFechaNac.Location = new System.Drawing.Point(20, 266);
            this.lblFechaNac.Name = "lblFechaNac";
            this.lblFechaNac.Size = new System.Drawing.Size(114, 17);
            this.lblFechaNac.TabIndex = 12;
            this.lblFechaNac.Text = "Fecha Nacimiento:";
            // 
            // txtNombre
            // 
            this.txtNombre.Location = new System.Drawing.Point(160, 68);
            this.txtNombre.Name = "txtNombre";
            this.txtNombre.Size = new System.Drawing.Size(270, 24);
            this.txtNombre.TabIndex = 2;
            // 
            // lblDni
            // 
            this.lblDni.AutoSize = true;
            this.lblDni.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.lblDni.Location = new System.Drawing.Point(20, 110);
            this.lblDni.Name = "lblDni";
            this.lblDni.Size = new System.Drawing.Size(38, 17);
            this.lblDni.TabIndex = 20;
            this.lblDni.Text = "DNI:";
            // 
            // txtDni
            // 
            this.txtDni.Location = new System.Drawing.Point(160, 106);
            this.txtDni.MaxLength = 12;
            this.txtDni.Name = "txtDni";
            this.txtDni.Size = new System.Drawing.Size(150, 24);
            this.txtDni.TabIndex = 21;
            // 
            // txtDescuento
            // 
            this.txtDescuento.Location = new System.Drawing.Point(160, 224);
            this.txtDescuento.Name = "txtDescuento";
            this.txtDescuento.Size = new System.Drawing.Size(60, 24);
            this.txtDescuento.TabIndex = 10;
            this.txtDescuento.Text = "0";
            // 
            // txtDivision
            // 
            this.txtDivision.Location = new System.Drawing.Point(160, 186);
            this.txtDivision.Name = "txtDivision";
            this.txtDivision.Size = new System.Drawing.Size(270, 24);
            this.txtDivision.TabIndex = 8;
            // 
            // cmbNivel
            // 
            this.cmbNivel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbNivel.Location = new System.Drawing.Point(160, 148);
            this.cmbNivel.Name = "cmbNivel";
            this.cmbNivel.Size = new System.Drawing.Size(270, 25);
            this.cmbNivel.TabIndex = 6;
            // 
            // dtpFechaNacimiento
            // 
            this.dtpFechaNacimiento.Enabled = false;
            this.dtpFechaNacimiento.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dtpFechaNacimiento.Location = new System.Drawing.Point(184, 262);
            this.dtpFechaNacimiento.Name = "dtpFechaNacimiento";
            this.dtpFechaNacimiento.Size = new System.Drawing.Size(180, 24);
            this.dtpFechaNacimiento.TabIndex = 14;
            this.dtpFechaNacimiento.Value = new System.DateTime(2010, 1, 1, 0, 0, 0, 0);
            // 
            // chkTieneFechaNac
            // 
            this.chkTieneFechaNac.AutoSize = true;
            this.chkTieneFechaNac.Location = new System.Drawing.Point(160, 266);
            this.chkTieneFechaNac.Name = "chkTieneFechaNac";
            this.chkTieneFechaNac.Size = new System.Drawing.Size(15, 14);
            this.chkTieneFechaNac.TabIndex = 13;
            this.chkTieneFechaNac.CheckedChanged += new System.EventHandler(this.chkTieneFechaNac_CheckedChanged);
            // 
            // lblIngresoFuturo
            // 
            this.lblIngresoFuturo.AutoSize = true;
            this.lblIngresoFuturo.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(60)))), ((int)(((byte)(60)))));
            this.lblIngresoFuturo.Location = new System.Drawing.Point(20, 300);
            this.lblIngresoFuturo.Name = "lblIngresoFuturo";
            this.lblIngresoFuturo.Size = new System.Drawing.Size(220, 17);
            this.lblIngresoFuturo.TabIndex = 17;
            this.lblIngresoFuturo.Text = "Ingresa recién el año que viene:";
            // 
            // chkIngresoFuturo
            // 
            this.chkIngresoFuturo.AutoSize = true;
            this.chkIngresoFuturo.Location = new System.Drawing.Point(250, 300);
            this.chkIngresoFuturo.Name = "chkIngresoFuturo";
            this.chkIngresoFuturo.Size = new System.Drawing.Size(15, 14);
            this.chkIngresoFuturo.TabIndex = 18;
            this.chkIngresoFuturo.CheckedChanged += new System.EventHandler(this.chkIngresoFuturo_CheckedChanged);
            // 
            // lblIngresoFuturoNota
            // 
            this.lblIngresoFuturoNota.AutoSize = true;
            this.lblIngresoFuturoNota.Font = new System.Drawing.Font("Segoe UI", 8.5F, System.Drawing.FontStyle.Italic);
            this.lblIngresoFuturoNota.ForeColor = System.Drawing.Color.Gray;
            this.lblIngresoFuturoNota.Location = new System.Drawing.Point(20, 322);
            this.lblIngresoFuturoNota.Name = "lblIngresoFuturoNota";
            this.lblIngresoFuturoNota.Size = new System.Drawing.Size(400, 30);
            this.lblIngresoFuturoNota.TabIndex = 19;
            this.lblIngresoFuturoNota.Text = "";
            // 
            // btnGuardar
            // 
            this.btnGuardar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(174)))), ((int)(((byte)(96)))));
            this.btnGuardar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnGuardar.FlatAppearance.BorderSize = 0;
            this.btnGuardar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnGuardar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnGuardar.ForeColor = System.Drawing.Color.White;
            this.btnGuardar.Location = new System.Drawing.Point(79, 366);
            this.btnGuardar.Name = "btnGuardar";
            this.btnGuardar.Size = new System.Drawing.Size(130, 36);
            this.btnGuardar.TabIndex = 15;
            this.btnGuardar.Text = "Guardar";
            this.btnGuardar.UseVisualStyleBackColor = false;
            this.btnGuardar.Click += new System.EventHandler(this.btnGuardar_Click);
            // 
            // btnCancelar
            // 
            this.btnCancelar.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(189)))), ((int)(((byte)(195)))), ((int)(((byte)(199)))));
            this.btnCancelar.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnCancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancelar.FlatAppearance.BorderSize = 0;
            this.btnCancelar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnCancelar.Font = new System.Drawing.Font("Segoe UI", 10F, System.Drawing.FontStyle.Bold);
            this.btnCancelar.ForeColor = System.Drawing.Color.White;
            this.btnCancelar.Location = new System.Drawing.Point(219, 366);
            this.btnCancelar.Name = "btnCancelar";
            this.btnCancelar.Size = new System.Drawing.Size(130, 36);
            this.btnCancelar.TabIndex = 16;
            this.btnCancelar.Text = "Cancelar";
            this.btnCancelar.UseVisualStyleBackColor = false;
            // 
            // panelHeader
            // 
            this.panelHeader.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(26)))), ((int)(((byte)(82)))), ((int)(((byte)(118)))));
            this.panelHeader.Controls.Add(this.lblTitulo);
            this.panelHeader.Dock = System.Windows.Forms.DockStyle.Top;
            this.panelHeader.Location = new System.Drawing.Point(0, 0);
            this.panelHeader.Name = "panelHeader";
            this.panelHeader.Size = new System.Drawing.Size(460, 50);
            this.panelHeader.TabIndex = 0;
            // 
            // lblTitulo
            // 
            this.lblTitulo.AutoSize = true;
            this.lblTitulo.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            this.lblTitulo.ForeColor = System.Drawing.Color.White;
            this.lblTitulo.Location = new System.Drawing.Point(16, 12);
            this.lblTitulo.Name = "lblTitulo";
            this.lblTitulo.Size = new System.Drawing.Size(164, 25);
            this.lblTitulo.TabIndex = 0;
            this.lblTitulo.Text = "Datos del Alumno";
            // 
            // NuevoAlumnoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 17F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(245)))), ((int)(((byte)(248)))), ((int)(((byte)(250)))));
            this.ClientSize = new System.Drawing.Size(460, 448);
            this.Controls.Add(this.panelHeader);
            this.Controls.Add(this.lblNombre);
            this.Controls.Add(this.txtNombre);
            this.Controls.Add(this.lblDni);
            this.Controls.Add(this.txtDni);
            this.Controls.Add(this.lblApellido);
            this.Controls.Add(this.lblNivel);
            this.Controls.Add(this.cmbNivel);
            this.Controls.Add(this.lblDivision);
            this.Controls.Add(this.txtDivision);
            this.Controls.Add(this.lblDescuento);
            this.Controls.Add(this.txtDescuento);
            this.Controls.Add(this.lblDescNota);
            this.Controls.Add(this.lblFechaNac);
            this.Controls.Add(this.chkTieneFechaNac);
            this.Controls.Add(this.dtpFechaNacimiento);
            this.Controls.Add(this.lblIngresoFuturo);
            this.Controls.Add(this.chkIngresoFuturo);
            this.Controls.Add(this.lblIngresoFuturoNota);
            this.Controls.Add(this.btnGuardar);
            this.Controls.Add(this.btnCancelar);
            this.Font = new System.Drawing.Font("Segoe UI", 9.5F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "NuevoAlumnoForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Nuevo Alumno";
            this.panelHeader.ResumeLayout(false);
            this.panelHeader.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        private System.Windows.Forms.Label lblNombre;
        private System.Windows.Forms.Label lblDni;
        private System.Windows.Forms.Label lblApellido;
        private System.Windows.Forms.Label lblNivel;
        private System.Windows.Forms.Label lblDivision;
        private System.Windows.Forms.Label lblDescuento;
        private System.Windows.Forms.Label lblDescNota;
        private System.Windows.Forms.Label lblFechaNac;
        private System.Windows.Forms.TextBox txtNombre;
        private System.Windows.Forms.TextBox txtDni;
        private System.Windows.Forms.TextBox txtDescuento;
        private System.Windows.Forms.TextBox txtDivision;
        private System.Windows.Forms.ComboBox cmbNivel;
        private System.Windows.Forms.DateTimePicker dtpFechaNacimiento;
        private System.Windows.Forms.CheckBox chkTieneFechaNac;
        private System.Windows.Forms.Label lblIngresoFuturo;
        private System.Windows.Forms.CheckBox chkIngresoFuturo;
        private System.Windows.Forms.Label lblIngresoFuturoNota;
        private System.Windows.Forms.Button btnGuardar;
        private System.Windows.Forms.Button btnCancelar;
        private System.Windows.Forms.Panel panelHeader;
        private System.Windows.Forms.Label lblTitulo;

        #endregion
    }
}