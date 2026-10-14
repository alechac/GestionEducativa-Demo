using GestionEducativa.Models;
using GestionEducativa.Presenters;
using GestionEducativa.Services;
using GestionEducativa.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    public partial class PagosForm : Form, IPagosView
    {
        private readonly PagosPresenter _presenter;
        private readonly AlumnoService _alumnoService;
        private readonly CuotaService _cuotaService;
        private readonly PadreService _padreService;

        // Estado interno
        private Alumno _alumnoSeleccionado;
        private List<Alumno> _alumnosHermanos = new List<Alumno>();
        private bool _mostrandoHermanos = false; // true mientras la grilla muestra el alumno + hermanos juntos
        private List<Cuota> _todasLasCuotas = new List<Cuota>();

        // Flag para evitar re-entrada en eventos encadenados
        private bool _actualizando = false;

        // ===== IPagosView =====
        public int SeccionActual { get; set; }
        private decimal MontoRecibidoActual =>
            string.IsNullOrWhiteSpace(numMonto.Text) ? 0m : numMonto.Value;

        public decimal MontoRecibido => MontoRecibidoActual;
        public string NombrePagador => txtPagador.Text.Trim();
        public string DocumentoPagador => txtDocumento.Text.Trim();
        public string TipoDocumento => rdnDNI.Checked ? "DNI" : "CUIL";
        public string FormaPago => cmbFormaPago.SelectedItem?.ToString();
        public string Observaciones => txtObservaciones.Text.Trim();
        public bool EnviarPorMail => chkEnviarPorMail.Checked;
        public MesCuota MesSeleccionado => (MesCuota)(cmbMes.SelectedIndex + 1); // Enero=0 → MesCuota.Enero=1
        public int AnioSeleccionado => (int)numAnio.Value;

        /// <summary>
        /// Devuelve la lista de cuotas que serán efectivamente cubiertas por el monto ingresado,
        /// en orden cronológico (cuotas más antiguas primero). Si el monto no alcanza para cubrir
        /// una cuota completa, incluye igual esa cuota con el saldo parcial correspondiente.
        /// </summary>
        public List<Cuota> CuotasParaPagar
        {
            get
            {
                // Obtener cuotas marcadas en el grid (respetando orden de aparición = más antigua primero)
                var marcadas = new List<Cuota>();
                foreach (DataGridViewRow row in gridCuotas.Rows)
                {
                    if (row.Cells["colSeleccionar"].Value is bool chk && chk && row.Tag is Cuota cuota)
                        marcadas.Add(cuota);
                }
                return marcadas;
            }
        }

        public event EventHandler AlumnoCambiado;

        public event EventHandler RegistrarPagoClicked;
        public event EventHandler GenerarCuotaClicked;
        public event EventHandler ImprimirComprobanteClicked;

        #region Declaración de controles
        // Panel Alumnos
        private System.Windows.Forms.Panel panelAlumnos;
        private System.Windows.Forms.Label lblTituloAlumnos, lblFiltroNombre, lblFiltroNivel, lblContadorAlumnos;
        private System.Windows.Forms.TextBox txtFiltroNombre;
        private System.Windows.Forms.ComboBox cmbFiltroNivel;
        private System.Windows.Forms.Button btnFiltrar;
        private System.Windows.Forms.DataGridView gridAlumnos;

        // Panel Padre
        private System.Windows.Forms.Panel panelPadre;
        private System.Windows.Forms.Label lblTituloPadre, lblNombrePadre, lblDocPadre, lblTelPadre;
        private System.Windows.Forms.Label lblValNombrePadre, lblValDocPadre, lblValTelPadre;
        private System.Windows.Forms.Button btnVerHermanos;

        // Separadores
        private System.Windows.Forms.Panel panelSeparador1, panelSeparador2;

        // Panel Cuotas
        private System.Windows.Forms.Panel panelCuotas;
        private System.Windows.Forms.Label lblTituloCuotas, lblAlumnoSeleccionado, lblNombreAlumno;
        private System.Windows.Forms.DataGridView gridCuotas;
        private System.Windows.Forms.CheckBox chkMostrarTodas;
        private System.Windows.Forms.Label lblTotalSeleccionado;

        // Panel Generar Cuotas Masivas (reemplaza los viejos controles individuales)
        private System.Windows.Forms.Panel panelGenerarMasiva;
        private System.Windows.Forms.Label lblGenerar, lblMes, lblAnio, lblMonto, lblNiveles, lblRecargo;
        private ComboBox cmbTipoCuota;
        private System.Windows.Forms.ComboBox cmbMes;
        private System.Windows.Forms.NumericUpDown numAnio, numMontoCuota;
        private System.Windows.Forms.CheckedListBox clbNiveles;
        private System.Windows.Forms.Button btnGenerarCuota, btnSeleccionarTodos;
        // (conservamos estas como propiedades para no romper el Designer existente)
        private System.Windows.Forms.Label lblDummy1, lblDummy2;

        // Panel Cobro
        private System.Windows.Forms.Panel panelCobro, panelResumen;
        private System.Windows.Forms.ComboBox cmbFormaPago;
        private System.Windows.Forms.Label lblTituloCobro, lblEtiquetaTotalDeuda, lblTotalDeuda;
        private System.Windows.Forms.Label lblEtiquetaVuelto, lblVuelto, lblEtiquetaDeuda, lblDeuda, lblMontoRecibido;
        private System.Windows.Forms.NumericUpDown numMonto;
        private System.Windows.Forms.Label lblSepPagador, lblPagador;
        private System.Windows.Forms.TextBox txtPagador;
        private System.Windows.Forms.RadioButton rdnDNI, rdnCUIT;
        private System.Windows.Forms.TextBox txtDocumento;
        private System.Windows.Forms.Label lblObservaciones;
        private System.Windows.Forms.TextBox txtObservaciones;
        private System.Windows.Forms.Button btnRegistrarPago, btnImprimir;
        private ConceptosPanel _panelConceptos;

        // Nuevo: botón de distribución automática
        private System.Windows.Forms.Button btnDistribuirMonto;
        private System.Windows.Forms.Label lblAyudaDistribucion;

        // Nuevo: checkbox para guardar/vincular el padre/tutor al registrar el pago.
        // Cubre dos casos con el mismo control (cambia texto y modo):
        //  - El documento tipeado no existe en la base → "Guardar como nuevo padre/tutor".
        //  - El documento ya existe pero como tutor de OTRO alumno (ej: la mamá ya está
        //    cargada para otro hermano) → "Vincular como tutor de este alumno". Esto es lo
        //    que hace que, a partir de ese momento, los hermanos con distintos tutores
        //    (cada hijo con su propio papá, pero la misma mamá) se relacionen solos.
        // En ningún caso se toca la base hasta que se confirma el cobro: se resuelve recién
        // en GuardarOVincularPadreSiCorresponde(), llamado por el presenter tras registrar el pago.
        private System.Windows.Forms.CheckBox chkGuardarPadre;
        private System.Windows.Forms.Label lblElegirPadre;
        private System.Windows.Forms.ComboBox cmbElegirPadre;
        private List<Padre> _padresDelAlumnoActual = new List<Padre>();
        private System.Windows.Forms.CheckBox chkEnviarPorMail;
        private Padre _padreDetectadoNoVinculado;
        #endregion

        public PagosForm(int seccion)
        {
            SeccionActual = seccion;
            _alumnoService = new AlumnoService();
            _cuotaService = new CuotaService();
            _padreService = new PadreService();
            InitializeComponent();
            if (DesignMode || LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;
            _presenter = new PagosPresenter(this);
            CargarFiltroNiveles();
            CargarMeses();
            CargarAlumnos();
            _panelConceptos.ConceptoGenerado += (s, e) => CargarCuotasAlumno();
            numAnio.Value = DateTime.Now.Year;

            // Construir el panel de generación masiva (lo hacemos en código,
            // no en el Designer, para no ensuciar el .Designer.cs)
            ConstruirPanelGenerarMasiva();

            // Al salir del campo documento, buscar el padre automáticamente
            txtDocumento.Leave += (s, e) => BuscarPadrePorDocumento();

            AgregarCheckGuardarPadre();
            AgregarCheckEnviarPorMail();
            AgregarComboElegirPadre();
        }

        private void PagosForm_Load(object sender, EventArgs e)
        {
            if (DesignMode) return;
            if (!_recargosAplicados)
            {
                _cuotaService.AplicarRecargosMora();
                _recargosAplicados = true;
            }
        }

        private void CargarFiltroNiveles()
        {
            cmbFiltroNivel.Items.Clear();
            cmbFiltroNivel.Items.Add("(Todos)");

            if (SeccionActual == (int)Seccion.Italiano)
            {
                cmbFiltroNivel.Items.Add("A1");
                cmbFiltroNivel.Items.Add("A2");
                cmbFiltroNivel.Items.Add("B1");
                cmbFiltroNivel.Items.Add("B2");
                cmbFiltroNivel.Items.Add("Parliamo");
            }
            else
            {
                cmbFiltroNivel.Items.Add("1 ANO");
                cmbFiltroNivel.Items.Add("2 ANO");
                cmbFiltroNivel.Items.Add("3 ANO");
                cmbFiltroNivel.Items.Add("4 ANO");
                cmbFiltroNivel.Items.Add("5 ANO");
                cmbFiltroNivel.Items.Add("6 ANO");
                cmbFiltroNivel.Items.Add("JARDIN DE 3 ANOS");
                cmbFiltroNivel.Items.Add("JARDIN DE 4 ANOS");
                cmbFiltroNivel.Items.Add("JARDIN DE 5 ANOS");
                cmbFiltroNivel.Items.Add("1 GRADO");
                cmbFiltroNivel.Items.Add("2 GRADO");
                cmbFiltroNivel.Items.Add("3 GRADO");
                cmbFiltroNivel.Items.Add("4 GRADO");
                cmbFiltroNivel.Items.Add("5 GRADO");
                cmbFiltroNivel.Items.Add("6 GRADO");
            }

            cmbFiltroNivel.SelectedIndex = 0;
        }

        public void CambiarSeccion(int nueva)
        {
            SeccionActual = nueva;
            CargarFiltroNiveles(); // si no, puede quedar seleccionado un nivel de la sección
                                    // anterior y generar un filtro contradictorio (nivel='ITALIANO'
                                    // Y nivel='3 GRADO' a la vez → nunca trae resultados)
            CargarNivelesGeneracionMasiva(); // ídem para el checklist de Generar Cuotas Masivas
            CargarAlumnos();
        }

        private void CargarAlumnos()
        {
            string filtroNombre = txtFiltroNombre?.Text.Trim() ?? "";
            string filtroNivel = (cmbFiltroNivel?.SelectedIndex > 0)
                ? cmbFiltroNivel.SelectedItem.ToString() : null;

            // incluirFuturos: true porque acá el cajero busca puntualmente a un alumno para
            // cobrarle algo (por ejemplo la matrícula de un preinscripto que ingresa el año
            // que viene). La columna "Ingresa en" deja claro cuándo todavía no es un alumno
            // actual, para no confundirlo con uno que ya está cursando.
            var alumnos = _alumnoService.ObtenerTodos(SeccionActual, filtroNivel, filtroNombre, incluirFuturos: true);

            gridAlumnos.DataSource = null;
            gridAlumnos.DataSource = alumnos;

            if (gridAlumnos.Columns.Count > 0)
            {
                gridAlumnos.Columns["Id"].Visible = false;
                gridAlumnos.Columns["Activo"].Visible = false;
                gridAlumnos.Columns["Seccion"].Visible = false;
                gridAlumnos.Columns["PorcentajeDescuento"].Visible = false;
                gridAlumnos.Columns["SaldoPendiente"].Visible = false;

                if (gridAlumnos.Columns.Contains("Nombre"))
                    gridAlumnos.Columns["Nombre"].HeaderText = "Alumno";
                if (gridAlumnos.Columns.Contains("Nivel"))
                    gridAlumnos.Columns["Nivel"].HeaderText = "Nivel";
                if (gridAlumnos.Columns.Contains("AnioIngreso"))
                {
                    gridAlumnos.Columns["AnioIngreso"].HeaderText = "Ingresa en";
                    gridAlumnos.Columns["AnioIngreso"].Width = 75;
                }
            }

            if (lblContadorAlumnos != null)
                lblContadorAlumnos.Text = $"{alumnos.Count} alumnos";
        }

        private void CargarMeses()
        {
            cmbMes.Items.Clear();
            cmbMes.Items.AddRange(new object[] {
                "Enero","Febrero","Marzo","Abril","Mayo","Junio",
                "Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre"
            });
            // Seleccionar mes actual
            int idxMes = DateTime.Now.Month - 1;
            cmbMes.SelectedIndex = idxMes >= 0 && idxMes < cmbMes.Items.Count ? idxMes : 0;
        }

        // ── Panel de generación masiva de cuotas ──────────────────────────────
        private void gridAlumnos_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            txtDocumento.Text = "";
            txtPagador.Text = "";
            if (e.RowIndex < 0) return;
            _alumnoSeleccionado = (Alumno)gridAlumnos.Rows[e.RowIndex].DataBoundItem;
            _mostrandoHermanos = false; // alumno nuevo → arranca mostrando solo sus propias cuotas

            lblNombreAlumno.Text = _alumnoSeleccionado.Nombre;
            lblNombreAlumno.ForeColor = Color.FromArgb(39, 174, 96);

            CargarDatosPadreAutomatico();
            CargarCuotasAlumno();

            _panelConceptos.CargarConceptos(_alumnoSeleccionado, (int)numAnio.Value);
            AlumnoCambiado?.Invoke(this, EventArgs.Empty);
        }

        public void LimpiarFormulario()
        {
            numMonto.Value = 0;
            txtPagador.Clear();
            txtDocumento.Clear();
            txtObservaciones.Clear();
            lblTotalDeuda.Text = "$ 0,00";
            lblVuelto.Text = "$ 0,00";
            lblDeuda.Text = "$ 0,00";
            chkGuardarPadre.Visible = false;
            chkGuardarPadre.Checked = false;
            chkEnviarPorMail.Checked = true;
            _padreDetectadoNoVinculado = null;
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

        // ===== Eventos de controles =====

        private void btnFiltrar_Click(object sender, EventArgs e) => CargarAlumnos();

        private void txtFiltroNombre_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                CargarAlumnos();
                e.Handled = true;
                e.SuppressKeyPress = true;
            }
        }

        private void cmbAlumno_SelectedIndexChanged(object sender, EventArgs e) => AlumnoCambiado?.Invoke(this, e);
        private void btnRegistrarPago_Click(object sender, EventArgs e) => RegistrarPagoClicked?.Invoke(this, e);
        private void btnGenerarCuota_Click(object sender, EventArgs e) => GenerarCuotaClicked?.Invoke(this, e);
        private void btnImprimir_Click(object sender, EventArgs e) => ImprimirComprobanteClicked?.Invoke(this, e);
    }
}
