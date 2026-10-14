using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GestionEducativa.Models;
using GestionEducativa.Presenters;
using GestionEducativa.Views.Interfaces;

namespace GestionEducativa.Views
{
    public partial class ConfiguracionForm : Form, IConfiguracionView
    {
        private readonly ConfiguracionPresenter _presenter;

        // IConfiguracionView
        public string RazonSocial { get => txtRazonSocial.Text; set => txtRazonSocial.Text = value; }
        public string CUIT        { get => txtCUIT.Text;        set => txtCUIT.Text = value; }
        public string Domicilio   { get => txtDomicilio.Text;   set => txtDomicilio.Text = value; }
        public string Telefono    { get => txtTelefono.Text;    set => txtTelefono.Text = value; }
        public string PuntoVenta  { get => txtPuntoVenta.Text;  set => txtPuntoVenta.Text = value; }

        public PrecioCuota PrecioSeleccionado
        {
            get
            {
                if (gridPrecios.SelectedRows.Count == 0) return null;

                var nivel   = (Nivel)cmbNivelPrecio.SelectedIndex + 1;
                var seccion = rdnEscuelaPrecio.Checked ? Seccion.Escuela : Seccion.Italiano;

                return new PrecioCuota
                {
                    Nivel = nivel,
                    Seccion = seccion,
                    Anio = (int)numAnioPrecio.Value,
                    Monto = numMontoPrecio.Value,
                    FechaVigencia = DateTime.Today
                };
            }
        }

        public event EventHandler GuardarConfigClicked;
        public event EventHandler GuardarPrecioClicked;
        public event EventHandler NuevoPrecioClicked;

        // Controls
        private TextBox txtRazonSocial, txtCUIT, txtDomicilio, txtTelefono, txtPuntoVenta;
        private Button btnGuardarConfig, btnGuardarPrecio, btnNuevoPrecio;
        private DataGridView gridPrecios;
        private ComboBox cmbNivelPrecio;
        private NumericUpDown numMontoPrecio, numAnioPrecio;
        private RadioButton rdnEscuelaPrecio, rdnItalianoPrecio;


        public ConfiguracionForm()
        {
            InitializeComponent();
            _presenter = new ConfiguracionPresenter(this);
            CargarNiveles();
            _presenter.Inicializar();
        }

        private void CargarNiveles()
        {
            cmbNivelPrecio.Items.Clear();
            cmbNivelPrecio.Items.AddRange(new object[] {
                "Jardín","Primario","Secundario","Italiano Infantil","Italiano Adultos" });
            cmbNivelPrecio.SelectedIndex = 0;
        }

        public void MostrarPrecios(List<PrecioCuota> precios)
        {
            gridPrecios.DataSource = null;
            gridPrecios.DataSource = precios;

            if (gridPrecios.Columns.Count > 0)
            {
                foreach (DataGridViewColumn col in gridPrecios.Columns) col.Visible = false;
                ShowCol("NivelDescripcion", "Nivel", 160);
                ShowCol("SeccionDescripcion", "Sección", 100);
                ShowCol("Anio", "Año", 70);
                ShowCol("Monto", "Monto ($)", 120);
                if (gridPrecios.Columns.Contains("Monto"))
                    gridPrecios.Columns["Monto"].DefaultCellStyle.Format = "N2";
            }
        }

        private void ShowCol(string name, string header, int width)
        {
            if (gridPrecios.Columns.Contains(name))
            {
                gridPrecios.Columns[name].Visible = true;
                gridPrecios.Columns[name].HeaderText = header;
                gridPrecios.Columns[name].Width = width;
            }
        }

        public void MostrarMensaje(string mensaje, bool esError = false)
        {
            if (esError)
                MessageBox.Show(mensaje, "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                MessageBox.Show(mensaje, "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void btnGuardarConfig_Click(object sender, EventArgs e)  => GuardarConfigClicked?.Invoke(this, e);
        private void btnGuardarPrecio_Click(object sender, EventArgs e)  => GuardarPrecioClicked?.Invoke(this, e);
        private void btnNuevoPrecio_Click(object sender, EventArgs e)    => NuevoPrecioClicked?.Invoke(this, e);

        private void gridPrecios_SelectionChanged(object sender, EventArgs e)
        {
            if (gridPrecios.SelectedRows.Count == 0) return;
            var precio = (PrecioCuota)gridPrecios.SelectedRows[0].DataBoundItem;

            // Pre-llenar formulario de edición
            for (int i = 0; i < cmbNivelPrecio.Items.Count; i++)
            {
                if ((int)precio.Nivel - 1 == i) { cmbNivelPrecio.SelectedIndex = i; break; }
            }
            rdnEscuelaPrecio.Checked  = precio.Seccion == Seccion.Escuela;
            rdnItalianoPrecio.Checked = precio.Seccion == Seccion.Italiano;
            numAnioPrecio.Value  = precio.Anio;
            numMontoPrecio.Value = precio.Monto;
        }
    }
}
