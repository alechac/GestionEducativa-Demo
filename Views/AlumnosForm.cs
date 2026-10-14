using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using GestionEducativa.Models;
using GestionEducativa.Presenters;
using GestionEducativa.Views.Interfaces;

namespace GestionEducativa.Views
{
    public partial class AlumnosForm : Form, IAlumnosView
    {
        private readonly AlumnosPresenter _presenter;

        // IAlumnosView
        public string TextoBusqueda => txtBusqueda.Text.Trim();
        public string NivelFiltro   => cmbNivel.Text.Trim();
        public int SeccionActual { get; set; }
        public bool MostrarPreinscriptos => chkMostrarPreinscriptos != null && chkMostrarPreinscriptos.Checked;
        public Alumno AlumnoSeleccionado => gridAlumnos.SelectedRows.Count > 0
            ? (Alumno)gridAlumnos.SelectedRows[0].DataBoundItem : null;

        public event EventHandler BuscarClicked;
        public event EventHandler NuevoAlumnoClicked;
        public event EventHandler EditarAlumnoClicked;
        public event EventHandler EliminarAlumnoClicked;
        public event EventHandler VerCuotasClicked;
        public event EventHandler ImportarExcelClicked;

        private System.Windows.Forms.Label lblBuscar;
        private System.Windows.Forms.Label lblNivel;
        private System.Windows.Forms.CheckBox chkMostrarPreinscriptos;


        public AlumnosForm(int seccion)
        {
            SeccionActual = seccion;
            InitializeComponent();
            ConstruirCheckPreinscriptos();
            _presenter = new AlumnosPresenter(this);
            CargarFiltroNivel();
            _presenter.Inicializar();
        }

        /// <summary>
        /// Checkbox agregado por código (para no tocar el Designer) que permite ver también
        /// a los alumnos "preinscriptos" (anotados para el año que viene, que todavía no
        /// cursan) — por defecto quedan ocultos del listado de alumnos actuales.
        /// </summary>
        private void ConstruirCheckPreinscriptos()
        {
            chkMostrarPreinscriptos = new System.Windows.Forms.CheckBox
            {
                AutoSize = true,
                Location = new Point(660, 17),
                Text = "Ver preinscriptos (año que viene)"
            };
            chkMostrarPreinscriptos.CheckedChanged += (s, e) => BuscarClicked?.Invoke(this, EventArgs.Empty);
            panelFiltros.Controls.Add(chkMostrarPreinscriptos);
        }

        public void CambiarSeccion(int nueva)
        {
            SeccionActual = nueva;
            CargarFiltroNivel(); // si no se recarga acá, el combo se queda con el nivel de la sección anterior
                                  // seleccionado, y termina generando un filtro contradictorio (ver ObtenerTodos)
            BuscarClicked?.Invoke(this, EventArgs.Empty);
        }

        private void CargarFiltroNivel()
        {
            cmbNivel.Items.Clear();

            // "Todos" siempre primero y queda seleccionado por defecto — así, apenas se cambia
            // de sección, se ve algo en la grilla en vez de quedar filtrado por un nivel que no
            // corresponde a la sección nueva.
            cmbNivel.Items.Add("Todos");

            if (SeccionActual == (int)Seccion.Italiano)
            {
                cmbNivel.Items.Add("A1");
                cmbNivel.Items.Add("A2");
                cmbNivel.Items.Add("B1");
                cmbNivel.Items.Add("B2");
                cmbNivel.Items.Add("Parliamo");
            }
            else
            {
                cmbNivel.Items.Add("1 ANO");
                cmbNivel.Items.Add("2 ANO");
                cmbNivel.Items.Add("3 ANO");
                cmbNivel.Items.Add("4 ANO");
                cmbNivel.Items.Add("5 ANO");
                cmbNivel.Items.Add("6 ANO");
                cmbNivel.Items.Add("JARDIN DE 3 ANOS");
                cmbNivel.Items.Add("JARDIN DE 4 ANOS");
                cmbNivel.Items.Add("JARDIN DE 5 ANOS");
                cmbNivel.Items.Add("1 GRADO");
                cmbNivel.Items.Add("2 GRADO");
                cmbNivel.Items.Add("3 GRADO");
                cmbNivel.Items.Add("4 GRADO");
                cmbNivel.Items.Add("5 GRADO");
                cmbNivel.Items.Add("6 GRADO");
            }

            cmbNivel.DisplayMember = "Value";
            cmbNivel.ValueMember = "Key";
            cmbNivel.SelectedIndex = 0; // "Todos"
        }

        public void MostrarAlumnos(List<Alumno> alumnos)
        {
            gridAlumnos.DataSource = null;
            gridAlumnos.DataSource = alumnos;

            // Configurar columnas
            if (gridAlumnos.Columns.Count > 0)
            {
                foreach (DataGridViewColumn col in gridAlumnos.Columns)
                    col.Visible = false;

                VisibleCol("nombre", "Alumno", 220);
                VisibleCol("nivel", "Nivel", 200);
                VisibleCol("PorcentajeDescuento", "Descuento %", 200);
                VisibleCol("AnioIngreso", "Ingresa en", 90);
            }

            lblContador.Text = $"Total: {alumnos.Count} alumno(s)";
        }

        private void VisibleCol(string name, string header, int width)
        {
            if (gridAlumnos.Columns.Contains(name))
            {
                gridAlumnos.Columns[name].Visible = true;
                gridAlumnos.Columns[name].HeaderText = header;
                gridAlumnos.Columns[name].Width = width;
            }
        }

        public void MostrarMensaje(string mensaje, bool esError = false)
        {
            if (esError)
                MessageBox.Show(mensaje, "Atención", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            else
                MessageBox.Show(mensaje, "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public bool ConfirmarAccion(string mensaje) =>
            MessageBox.Show(mensaje, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;

        // Event handlers del form
        private void btnBuscar_Click(object sender, EventArgs e)       => BuscarClicked?.Invoke(this, e);
        private void btnNuevo_Click(object sender, EventArgs e)         => NuevoAlumnoClicked?.Invoke(this, e);
        private void btnEditar_Click(object sender, EventArgs e)        => EditarAlumnoClicked?.Invoke(this, e);
        private void btnEliminar_Click(object sender, EventArgs e)      => EliminarAlumnoClicked?.Invoke(this, e);
        private void btnImportar_Click(object sender, EventArgs e)      => ImportarExcelClicked?.Invoke(this, e);
        private void txtBusqueda_KeyDown(object sender, KeyEventArgs e) { if (e.KeyCode == Keys.Enter) BuscarClicked?.Invoke(this, e); }
        private void gridAlumnos_CellDoubleClick(object sender, DataGridViewCellEventArgs e) => EditarAlumnoClicked?.Invoke(this, EventArgs.Empty);
        private void cmbNivel_SelectedIndexChanged(object sender, EventArgs e) => BuscarClicked?.Invoke(this, EventArgs.Empty);
    }
}
