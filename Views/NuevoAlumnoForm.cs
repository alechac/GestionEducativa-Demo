using GestionEducativa.Models;
using GestionEducativa.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    public partial class NuevoAlumnoForm : Form
    {
        public Alumno AlumnoResultante { get; private set; }

        private readonly int _seccion;
        private readonly Alumno _alumnoEdicion;
        private readonly AlumnoService _alumnoService;


        public NuevoAlumnoForm(int seccion, Alumno alumnoEdicion = null)
        {
            _seccion = seccion;
            _alumnoEdicion = alumnoEdicion;
            _alumnoService = new AlumnoService();
            InitializeComponent();
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;
            CargarNiveles();

            if (_alumnoEdicion != null)
                PreCargarEdicion();
        }

        private void CargarNiveles()
        {
            cmbNivel.Items.Clear();

            if (_seccion == (int)Seccion.Italiano)
            {
                // Los niveles reales de Scuola Italiano. La sección ya no depende de este
                // valor (se distingue por la columna "seccion"), así que acá el combo queda
                // habilitado para elegir el nivel real del alumno.
                cmbNivel.Items.Add("A1");
                cmbNivel.Items.Add("A2");
                cmbNivel.Items.Add("B1");
                cmbNivel.Items.Add("B2");
                cmbNivel.Items.Add("Parliamo"); // conversación — cuota distinta ($20.000 vs $40.000)
                cmbNivel.SelectedIndex = 0;
                cmbNivel.Enabled = true;
                return;
            }

            string[] niveles = {
                "1 ano","2 ano","3 ano","4 ano","5 ano","6 ano",
                "jardin de 3 anos","jardin de 4 anos","jardin de 5 anos",
                "1 grado","2 grado","3 grado","4 grado","5 grado","6 grado"
            };
            foreach (var n in niveles) cmbNivel.Items.Add(n);
            cmbNivel.SelectedIndex = 0;
        }

        private void PreCargarEdicion()
        {
            this.Text = "Editar Alumno";

            txtNombre.Text = _alumnoEdicion.Nombre;
            txtDni.Text = _alumnoEdicion.Dni ?? "";
            txtDescuento.Text = _alumnoEdicion.PorcentajeDescuento.ToString();
            txtDivision.Text = _alumnoEdicion.Division ?? "";

            // Nivel
            string nivelLower = _alumnoEdicion.Nivel?.ToLower() ?? "";
            for (int i = 0; i < cmbNivel.Items.Count; i++)
                if (cmbNivel.Items[i].ToString().ToLower() == nivelLower)
                { cmbNivel.SelectedIndex = i; break; }

            // Fecha nacimiento
            if (_alumnoEdicion.FechaNacimiento.HasValue)
            {
                chkTieneFechaNac.Checked = true;
                dtpFechaNacimiento.Enabled = true;
                dtpFechaNacimiento.Value = _alumnoEdicion.FechaNacimiento.Value;
            }

            // Ingreso a futuro (preinscripto)
            if (_alumnoEdicion.EsPreinscripto(DateTime.Now.Year))
            {
                chkIngresoFuturo.Checked = true;
                ActualizarTextoIngresoFuturo();
            }
        }

        private void chkTieneFechaNac_CheckedChanged(object sender, EventArgs e)
        {
            dtpFechaNacimiento.Enabled = chkTieneFechaNac.Checked;
        }

        private void chkIngresoFuturo_CheckedChanged(object sender, EventArgs e)
        {
            ActualizarTextoIngresoFuturo();
        }

        /// <summary>
        /// Aclara en pantalla qué significa "Nivel" según si el alumno ya está cursando o
        /// todavía no arrancó: para un alumno actual, Nivel es el nivel EN EL QUE ESTÁ ahora
        /// (y el sistema le calcula la matrícula del nivel siguiente para el año que viene).
        /// Para un alumno que ingresa recién el año que viene, en cambio, hay que elegir
        /// directamente el nivel AL QUE VA A ENTRAR (ej: Jardín de 4 años), y el sistema le
        /// cobrará la matrícula de ESE nivel, no la del siguiente.
        /// </summary>
        private void ActualizarTextoIngresoFuturo()
        {
            if (chkIngresoFuturo.Checked)
            {
                int anioIngreso = DateTime.Now.Year + 1;
                lblNivel.Text = "Nivel al que ingresa:";
                lblIngresoFuturoNota.Text =
                    $"El alumno todavía no está cursando: no aparecerá en los listados de\n" +
                    $"alumnos actuales y se le cobrará la matrícula de {anioIngreso} para el\n" +
                    $"nivel elegido arriba (no la del nivel siguiente).";
            }
            else
            {
                lblNivel.Text = "Nivel:";
                lblIngresoFuturoNota.Text = "";
            }
        }

        private void btnGuardar_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Por favor, complete el Nombre del alumno.", "Campo obligatorio",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string dni = txtDni.Text.Trim().Replace(".", "");
            if (string.IsNullOrWhiteSpace(dni))
            {
                MessageBox.Show("Por favor, complete el DNI del alumno.", "Campo obligatorio",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (!dni.All(char.IsDigit))
            {
                MessageBox.Show("El DNI debe contener solo números (sin puntos ni letras).", "Valor inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (!int.TryParse(txtDescuento.Text.Trim(), out int descuento) || descuento < 0 || descuento > 100)
            {
                MessageBox.Show("El descuento debe ser un número entero entre 0 y 100.", "Valor inválido",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string nombreCompleto = txtNombre.Text.Trim().ToUpper();

            AlumnoResultante = new Alumno
            {
                Id = _alumnoEdicion?.Id ?? 0,
                Nombre = nombreCompleto,
                Dni = dni,
                Nivel = cmbNivel.SelectedItem?.ToString() ?? "",
                Division = txtDivision.Text.Trim().ToUpper(),
                PorcentajeDescuento = descuento,
                FechaNacimiento = chkTieneFechaNac.Checked ? dtpFechaNacimiento.Value.Date : (DateTime?)null,
                Activo = true,
                Seccion = _seccion,
                AnioIngreso = chkIngresoFuturo.Checked ? (int?)(DateTime.Now.Year + 1) : null
            };

            this.DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
