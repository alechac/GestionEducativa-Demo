using GestionEducativa.Models;
using GestionEducativa.Presenters;
using GestionEducativa.Services;
using GestionEducativa.Views.Interfaces;
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
    public partial class InformesForm : Form, IInformesView
    {
        private readonly InformesPresenter _presenter;
        private readonly InformesService _svc;
        private readonly AlumnoService _alumnoSvc;

        // ── IInformesView ──────────────────────────────────────────
        public string TipoInforme => _tipoActual;
        public int AnioFiltro => (int)numAnio.Value;
        public int MesFiltro => cmbMesFiltro.SelectedIndex; // 0=todos, 1=Enero...
        public string NivelFiltroTexto => cmbNivel.SelectedIndex <= 0 ? null : cmbNivel.SelectedItem?.ToString();
        public string NivelEducativoFiltro => cmbNivelEducativo.SelectedIndex <= 0 ? null : cmbNivelEducativo.SelectedItem?.ToString();
        public string DivisionFiltro => cmbDivision.SelectedIndex <= 0 ? null : cmbDivision.SelectedItem?.ToString();
        public DateTime FechaDesde => dtpDesde.Value.Date;
        public DateTime FechaHasta => dtpHasta.Value.Date;
        public int AlumnoId => _alumnoSelId;
        public string NombrePadre => txtBuscarPadre.Text.Trim();
        public string DocumentoPadre => txtDocPadre.Text.Trim();

        /// <summary>Devuelve el Nro de Comprobante de la fila seleccionada.</summary>
        public string NroComprobanteSeleccionado
        {
            get
            {
                if (gridInforme.SelectedRows.Count == 0) return null;
                var celda = gridInforme.SelectedRows[0].Cells["Nro de Comprobante"];
                if (celda == null || celda.Value == DBNull.Value) return null;
                return celda.Value?.ToString();
            }
        }

        public bool ColNombre => chkColNombre.Checked;
        public bool ColNivel => chkColNivel.Checked;
        public bool ColDivision => chkColDivision.Checked;
        public bool ColDni => chkColDni.Checked;
        public bool ColDescuento => chkColDescuento.Checked;
        public bool ColSaldo => chkColSaldo.Checked;
        public bool ColPadre => chkColPadre.Checked;
        public bool ColDniPadre => chkColDniPadre.Checked;
        public bool ColTelefono => chkColTelefono.Checked;
        public bool ColMailPadre => chkColMailPadre.Checked;

        public event EventHandler GenerarInformeClicked;
        public event EventHandler ExportarExcelClicked;
        public event EventHandler ImprimirClicked;
        public event EventHandler ReimprimirComprobanteClicked;

        // ── Estado interno ─────────────────────────────────────────
        private string _tipoActual = "IngresosFecha";
        private int _alumnoSelId = 0;
        private Label _lblTotales;
        private Button _btnReimprimir;



        // ── Botones del menú lateral ───────────────────────────────
        private readonly Dictionary<string, Button> _btnsTipo = new Dictionary<string, Button>();

        public InformesForm()
        {
            _svc = new InformesService();
            _alumnoSvc = new AlumnoService();
            InitializeComponent();
            ConstruirUI();
            _presenter = new InformesPresenter(this);
            CargarFiltrosIniciales();
            ActivarTipo("IngresosFecha");
        }

        // ══════════════════════════════════════════════════════════
        // Construcción de la UI en código
        // ══════════════════════════════════════════════════════════
        private void ConstruirUI()
        {
            // Los paneles principales (panelTipos, panelMain, panelFiltros,
            // panelResumenCaja, panelBotones, panelDatos, gridInforme)
            // ya están configurados en InitializeComponent() (Designer).
            // Aquí solo agregamos los botones del menú lateral dinámicamente.

            // ── Label de totales al pie del grid ──────────────────────
            _lblTotales = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 32,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 82, 118),
                BackColor = Color.FromArgb(213, 230, 248),
                Padding = new Padding(10, 0, 0, 0),
                Text = "",
                Visible = false
            };
            // El gridInforme tiene Dock=Fill; agregar el label con Dock=Bottom
            // hace que el grid ocupe el espacio restante automáticamente.
            panelDatos.Controls.Add(_lblTotales);

            // ── Botón Reimprimir (solo visible en Pagos de Alumno) ────
            _btnReimprimir = new Button
            {
                Text = "🖨 Reimprimir Comprobante",
                Visible = false,
                Enabled = false,
                Height = 32,
                Width = 220,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Dock = DockStyle.Right
            };
            _btnReimprimir.FlatAppearance.BorderSize = 0;
            _btnReimprimir.Click += (s, e) => ReimprimirComprobanteClicked?.Invoke(this, EventArgs.Empty);
            panelBotones.Controls.Add(_btnReimprimir);

            // Habilitar el botón cuando se selecciona una fila con Nro de Comprobante
            gridInforme.SelectionChanged += (s, e) =>
            {
                if (!_btnReimprimir.Visible) return;
                bool tieneComprobante = gridInforme.SelectedRows.Count > 0 &&
                    gridInforme.SelectedRows[0].Cells["Nro de Comprobante"]?.Value?.ToString() is string nro &&
                    !string.IsNullOrWhiteSpace(nro);
                _btnReimprimir.Enabled = tieneComprobante;
            };
            AgregarBotonMenu("📅 Ingresos por Fecha", "IngresosFecha");
            AgregarBotonMenu("📆 Ingresos por Mes", "IngresosMes");
            AgregarBotonMenu("🏫 Ingresos por Nivel", "IngresosNivel");
            AgregarBotonMenu("💵 Proyección del Mes (Facturado/Cobrado/Pendiente)", "ProyeccionMensual");
            AgregarBotonMenu("👤 Pagos de Alumno", "PagosAlumno");
            AgregarBotonMenu("👨‍👩‍👧 Pagos de Padre", "PagosPadre");
            AgregarBotonMenu("📝 Matriculados", "Matriculados");
            AgregarBotonMenu("✅ Pagos por Curso/División", "EstadoPagoMes");
            AgregarBotonMenu("🚫 Sin Pagos Registrados", "AlumnosSinPagos");
            AgregarBotonMenu("📋 Listado Alumnos", "ListadoAlumnos");
            AgregarBotonMenu("💰 Cierre de Caja", "CierreCaja");
        }

        private void AgregarBotonMenu(string texto, string tipo)
        {
            var btn = new Button
            {
                Text = texto,
                Dock = DockStyle.Top,
                Height = 42,
                FlatStyle = FlatStyle.Flat,
                ForeColor = Color.White,
                BackColor = Color.Transparent,
                TextAlign = ContentAlignment.MiddleLeft,
                Padding = new Padding(16, 0, 0, 0),
                Font = new Font("Segoe UI", 9.5f),
                Cursor = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = Color.FromArgb(40, 116, 166);
            btn.Tag = tipo;
            btn.Click += (s, e) => ActivarTipo(tipo);
            _btnsTipo[tipo] = btn;
            panelTipos.Controls.Add(btn);
            // DockStyle.Top apila de arriba a abajo — agregar al frente
            btn.BringToFront();
        }





        // ══════════════════════════════════════════════════════════
        // Cambio de tipo de informe — reconstruye el panel de filtros
        // ══════════════════════════════════════════════════════════
        private void ActivarTipo(string tipo)
        {
            _tipoActual = tipo;
            _btnReimprimir.Visible = tipo == "PagosAlumno" || tipo == "PagosPadre";
            _btnReimprimir.Enabled = false;

            // Resaltar botón activo
            foreach (var kv in _btnsTipo)
                kv.Value.BackColor = kv.Key == tipo
                    ? Color.FromArgb(40, 116, 166) : Color.Transparent;

            panelFiltros.Controls.Clear();
            panelFiltros.AutoScroll = true; // por si el panel de filtros no entra en 1360x768
            panelResumenCaja.Visible = (tipo == "CierreCaja");
            gridInforme.DataSource = null;

            int y = 12;
            switch (tipo)
            {
                case "IngresosFecha":
                    AgregarLabel("Desde:", 10, y); dtpDesde = AgregarDatePicker(70, y); dtpDesde.Value = DateTime.Today;
                    AgregarLabel("Hasta:", 240, y); dtpHasta = AgregarDatePicker(300, y); dtpHasta.Value = DateTime.Today;
                    panelFiltros.Height = 140;
                    break;

                case "IngresosMes":
                    AgregarLabel("Año:", 10, y); numAnio = AgregarNumAnio(50, y);
                    AgregarLabel("Nivel:", 180, y); cmbNivel = AgregarComboNivel(220, y);
                    AgregarLabel("División:", 400, y); cmbDivision = AgregarComboDivision(465, y);
                    panelFiltros.Height = 70;
                    break;

                case "IngresosNivel":
                    AgregarLabel("Año:", 10, y); numAnio = AgregarNumAnio(50, y);
                    AgregarLabel("Nivel:", 180, y); cmbNivel = AgregarComboNivel(220, y);
                    AgregarLabel("Mes:", 400, y); cmbMesFiltro = AgregarComboMes(440, y, incluirTodos: true);
                    panelFiltros.Height = 70;
                    break;

                case "ProyeccionMensual":
                    AgregarLabel("Año:", 10, y); numAnio = AgregarNumAnio(50, y);
                    AgregarLabel("Mes:", 180, y); cmbMesFiltro = AgregarComboMes(220, y, incluirTodos: true);
                    AgregarLabel("Nivel (opcional):", 400, y); cmbNivel = AgregarComboNivel(510, y);
                    panelFiltros.Height = 70;
                    break;

                case "Matriculados":
                    AgregarLabel("Año:", 10, y); numAnio = AgregarNumAnio(50, y);
                    AgregarLabel("Nivel educativo:", 180, y); cmbNivelEducativo = AgregarComboNivelEducativo(300, y);
                    AgregarLabel("Grado:", 460, y); cmbNivel = AgregarComboNivel(510, y);
                    AgregarLabel("División:", 700, y); cmbDivision = AgregarComboDivision(765, y);
                    panelFiltros.Height = 70;
                    break;

                case "EstadoPagoMes":
                    AgregarLabel("Año:", 10, y); numAnio = AgregarNumAnio(50, y);
                    AgregarLabel("Mes:", 180, y); cmbMesFiltro = AgregarComboMes(220, y, incluirTodos: true);
                    cmbMesFiltro.SelectedIndex = DateTime.Now.Month; // el mes actual (0="(Todos)" no aplica acá)
                    AgregarLabel("Curso:", 400, y); cmbNivel = AgregarComboNivel(445, y);
                    AgregarLabel("División:", 640, y); cmbDivision = AgregarComboDivision(705, y);
                    panelFiltros.Height = 70;
                    break;

                case "AlumnosSinPagos":
                    AgregarLabel("Año:", 10, y); numAnio = AgregarNumAnio(50, y);
                    AgregarLabel("Deuda desde el mes:", 180, y);
                    cmbMesFiltro = AgregarComboMes(310, y, incluirTodos: true);
                    cmbMesFiltro.SelectedIndex = 7; // Julio (índice 1=Enero...7=Julio, igual que el resto de los informes)
                    AgregarLabel("Curso:", 500, y); cmbNivel = AgregarComboNivel(545, y);
                    AgregarLabel("División:", 740, y); cmbDivision = AgregarComboDivision(805, y);
                    panelFiltros.Height = 70;
                    break;

                case "PagosAlumno":
                    AgregarLabel("Alumno:", 10, y);
                    cmbAlumno = new ComboBox
                    {
                        Location = new Point(70, y),
                        Size = new Size(360, 26),
                        DropDownStyle = ComboBoxStyle.DropDownList,
                        Font = new Font("Segoe UI", 9f)
                    };
                    CargarAlumnos();
                    cmbAlumno.SelectedIndexChanged += (s, e) =>
                        _alumnoSelId = cmbAlumno.SelectedItem is AlumnoItem ai ? ai.Id : 0;
                    panelFiltros.Controls.Add(cmbAlumno);
                    panelFiltros.Height = 70;
                    break;

                case "PagosPadre":
                    AgregarLabel("Nombre pagador:", 10, y);
                    txtBuscarPadre = new TextBox
                    {
                        Location = new Point(130, y),
                        Size = new Size(240, 26),
                        Font = new Font("Segoe UI", 9f),
                    };
                    panelFiltros.Controls.Add(txtBuscarPadre);
                    AgregarLabel("o DNI/CUIT:", 390, y);
                    txtDocPadre = new TextBox
                    {
                        Location = new Point(470, y),
                        Size = new Size(140, 26),
                        Font = new Font("Segoe UI", 9f)
                    };
                    panelFiltros.Controls.Add(txtDocPadre);
                    panelFiltros.Height = 70;
                    break;

                case "ListadoAlumnos":
                    AgregarLabel("Nivel:", 10, y); cmbNivel = AgregarComboNivel(55, y);
                    AgregarLabel("División:", 220, y); cmbDivision = AgregarComboDivision(278, y);

                    // Checkboxes para columnas — sin ref, asignación directa
                    int cx = 10, cy = y + 40;
                    var colsConfig = new[]
                    {
                        ("Nombre",    true),  ("Nivel",     true),
                        ("División",  true),  ("DNI",       false),
                        ("Descuento", false), ("Padre",     false),
                        ("DNI Padre", false), ("Teléfono",  false), ("Mail Padre", false)
                    };

                    chkColNombre = CrearCheckCol(colsConfig[0].Item1, colsConfig[0].Item2, cx, cy); panelFiltros.Controls.Add(chkColNombre); cx += 110;
                    chkColNivel = CrearCheckCol(colsConfig[1].Item1, colsConfig[1].Item2, cx, cy); panelFiltros.Controls.Add(chkColNivel); cx += 110;
                    chkColDivision = CrearCheckCol(colsConfig[2].Item1, colsConfig[2].Item2, cx, cy); panelFiltros.Controls.Add(chkColDivision); cx += 110;
                    chkColDni = CrearCheckCol(colsConfig[3].Item1, colsConfig[3].Item2, cx, cy); panelFiltros.Controls.Add(chkColDni); cx += 110;
                    if (cx > 900) { cx = 10; cy += 24; }
                    chkColDescuento = CrearCheckCol(colsConfig[4].Item1, colsConfig[4].Item2, cx, cy); panelFiltros.Controls.Add(chkColDescuento); cx += 110;
                    chkColPadre = CrearCheckCol(colsConfig[5].Item1, colsConfig[5].Item2, cx, cy); panelFiltros.Controls.Add(chkColPadre); cx += 110;
                    chkColDniPadre = CrearCheckCol(colsConfig[6].Item1, colsConfig[6].Item2, cx, cy); panelFiltros.Controls.Add(chkColDniPadre); cx += 110;
                    if (cx > 900) { cx = 10; cy += 24; }
                    chkColTelefono = CrearCheckCol(colsConfig[7].Item1, colsConfig[7].Item2, cx, cy); panelFiltros.Controls.Add(chkColTelefono);
                    chkColMailPadre = CrearCheckCol(colsConfig[8].Item1, colsConfig[8].Item2, cx + 110, cy); panelFiltros.Controls.Add(chkColMailPadre);
                    panelFiltros.Height = 140;
                    break;

                case "CierreCaja":
                    AgregarLabel("Fecha:", 10, y); dtpDesde = AgregarDatePicker(65, y); dtpDesde.Value = DateTime.Today;
                    panelFiltros.Height = 70;
                    break;
            }

            // Asegurar controles no-null para no romper la interfaz
            if (dtpDesde == null) dtpDesde = new DateTimePicker { Value = DateTime.Today };
            if (dtpHasta == null) dtpHasta = new DateTimePicker { Value = DateTime.Today };
            if (numAnio == null) { numAnio = new NumericUpDown { Minimum = 2020, Maximum = 2100, Value = DateTime.Now.Year }; }
            if (cmbMesFiltro == null) cmbMesFiltro = new ComboBox();
            if (cmbNivel == null) cmbNivel = new ComboBox();
            if (cmbNivelEducativo == null) cmbNivelEducativo = new ComboBox();
            if (cmbDivision == null) cmbDivision = new ComboBox();
            if (txtBuscarPadre == null) txtBuscarPadre = new TextBox();
            if (txtDocPadre == null) txtDocPadre = new TextBox();
            if (cmbAlumno == null) cmbAlumno = new ComboBox();

            EnsureCheckboxes();
        }

        private CheckBox CrearCheckCol(string texto, bool marcado, int x, int y)
        {
            return new CheckBox
            {
                Text = texto,
                Location = new Point(x, y),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f),
                Checked = marcado
            };
        }

        private void EnsureCheckboxes()
        {
            if (chkColNombre == null) chkColNombre = new CheckBox { Checked = true };
            if (chkColNivel == null) chkColNivel = new CheckBox { Checked = true };
            if (chkColDivision == null) chkColDivision = new CheckBox { Checked = true };
            if (chkColDni == null) chkColDni = new CheckBox();
            if (chkColDescuento == null) chkColDescuento = new CheckBox();
            if (chkColSaldo == null) chkColSaldo = new CheckBox();
            if (chkColPadre == null) chkColPadre = new CheckBox();
            if (chkColDniPadre == null) chkColDniPadre = new CheckBox();
            if (chkColTelefono == null) chkColTelefono = new CheckBox();
            if (chkColMailPadre == null) chkColMailPadre = new CheckBox();
        }

        // ── Helpers para crear controles ───────────────────────────
        private Label AgregarLabel(string texto, int x, int y)
        {
            var lbl = new Label
            {
                Text = texto,
                Location = new Point(x, y + 3),
                AutoSize = true,
                Font = new Font("Segoe UI", 9f)
            };
            panelFiltros.Controls.Add(lbl);
            return lbl;
        }

        private DateTimePicker AgregarDatePicker(int x, int y)
        {
            var dtp = new DateTimePicker
            {
                Location = new Point(x, y),
                Size = new Size(150, 26),
                Format = DateTimePickerFormat.Short,
                Font = new Font("Segoe UI", 9f)
            };
            panelFiltros.Controls.Add(dtp);
            return dtp;
        }

        private NumericUpDown AgregarNumAnio(int x, int y)
        {
            var num = new NumericUpDown
            {
                Location = new Point(x, y),
                Size = new Size(80, 26),
                Minimum = 2020,
                Maximum = 2100,
                Value = DateTime.Now.Year,
                Font = new Font("Segoe UI", 9f)
            };
            panelFiltros.Controls.Add(num);
            return num;
        }

        private ComboBox AgregarComboNivelEducativo(int x, int y)
        {
            var cmb = new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(150, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };
            cmb.Items.Add("(Todos)");
            cmb.Items.Add("Inicial");
            cmb.Items.Add("Primaria");
            cmb.Items.Add("Secundaria");
            cmb.SelectedIndex = 0;

            // Al elegir un nivel educativo, filtramos el combo de Grado para que solo
            // muestre los grados que corresponden a ese nivel (ej: Inicial → jardines).
            cmb.SelectedIndexChanged += (s, e) =>
            {
                if (cmbNivel == null) return;
                string grupo = cmb.SelectedIndex > 0 ? cmb.SelectedItem.ToString() : null;
                cmbNivel.Items.Clear();
                cmbNivel.Items.Add("(Todos)");
                try
                {
                    foreach (var n in _svc.ObtenerNiveles())
                        if (grupo == null || ClasificarNivelEducativo(n) == grupo)
                            cmbNivel.Items.Add(n);
                }
                catch { }
                cmbNivel.SelectedIndex = 0;
            };
            panelFiltros.Controls.Add(cmb);
            return cmb;
        }

        /// <summary>
        /// Clasifica un nivel/grado exacto (ej: "jardin de 4 anos", "3 grado", "2 ano")
        /// en su nivel educativo: Inicial, Primaria o Secundaria. Debe reflejar la misma
        /// convención de nombres usada en el resto del sistema (jardines = Inicial,
        /// "grado" = Primaria, "ano/año" = Secundaria).
        /// </summary>
        private static string ClasificarNivelEducativo(string nivel)
        {
            if (string.IsNullOrEmpty(nivel)) return "Otro";
            string n = nivel.Trim().ToLowerInvariant();
            if (n.Contains("jardin") || n.Contains("jardín")) return "Inicial";
            if (n.Contains("grado")) return "Primaria";
            if (n.Contains("ano") || n.Contains("año")) return "Secundaria";
            return "Otro";
        }

        private ComboBox AgregarComboNivel(int x, int y)
        {
            var cmb = new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(170, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };
            cmb.Items.Add("(Todos)");
            try { foreach (var n in _svc.ObtenerNiveles()) cmb.Items.Add(n); } catch { }
            cmb.SelectedIndex = 0;
            cmb.SelectedIndexChanged += (s, e) =>
            {
                if (cmbDivision != null)
                {
                    string niv = cmb.SelectedIndex > 0 ? cmb.SelectedItem.ToString() : null;
                    cmbDivision.Items.Clear();
                    cmbDivision.Items.Add("(Todas)");
                    try { foreach (var d in _svc.ObtenerDivisiones(niv)) cmbDivision.Items.Add(d); } catch { }
                    cmbDivision.SelectedIndex = 0;
                }
            };
            panelFiltros.Controls.Add(cmb);
            return cmb;
        }

        private ComboBox AgregarComboDivision(int x, int y)
        {
            var cmb = new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(100, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };
            cmb.Items.Add("(Todas)");
            try { foreach (var d in _svc.ObtenerDivisiones()) cmb.Items.Add(d); } catch { }
            cmb.SelectedIndex = 0;
            panelFiltros.Controls.Add(cmb);
            return cmb;
        }

        private ComboBox AgregarComboMes(int x, int y, bool incluirTodos = false)
        {
            var cmb = new ComboBox
            {
                Location = new Point(x, y),
                Size = new Size(140, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f)
            };
            if (incluirTodos) cmb.Items.Add("(Todos)");
            cmb.Items.AddRange(new object[] {
                "Enero","Febrero","Marzo","Abril","Mayo","Junio",
                "Julio","Agosto","Septiembre","Octubre","Noviembre","Diciembre" });
            cmb.SelectedIndex = 0;
            panelFiltros.Controls.Add(cmb);
            return cmb;
        }

        private void CargarAlumnos()
        {
            cmbAlumno.Items.Clear();
            cmbAlumno.Items.Add(new AlumnoItem(0, "(seleccionar alumno)"));
            try
            {
                foreach (var a in _alumnoSvc.ObtenerTodos())
                    cmbAlumno.Items.Add(new AlumnoItem(a.Id, $"{a.Nombre} — {a.Nivel}"));
            }
            catch { }
            cmbAlumno.SelectedIndex = 0;
        }

        private void CargarFiltrosIniciales()
        {
            // Los filtros se crean dinámicamente al activar el tipo,
            // así que aquí solo nos aseguramos del estado inicial.
        }

        // ══════════════════════════════════════════════════════════
        // IInformesView — implementación
        // ══════════════════════════════════════════════════════════
        public void MostrarDatos(DataTable datos)
        {
            gridInforme.DataSource = null;
            gridInforme.DataSource = datos;
            foreach (DataGridViewColumn col in gridInforme.Columns)
            {
                string n = col.Name.ToLower();
                if (n == "pagoid") { col.Visible = false; continue; }
                if (n.Contains("monto") || n.Contains("pagado") || n.Contains("saldo") ||
                    n.Contains("total") || n.Contains("recibido") || n.Contains("cobrado") ||
                    n.Contains("vuelto") || n.Contains("facturado") || n.Contains("pendiente") ||
                    n.Contains("recargo"))
                    col.DefaultCellStyle.Format = "N2";
            }
            ColorearFilasPorEstado(datos);
            ActualizarLabelTotales(datos);
        }

        /// <summary>
        /// Si el informe tiene columna "Estado" (ej. Pagos por Curso/División), pinta cada
        /// fila según su valor: verde = Pagado, rojo = No Pagado, amarillo = Pago Parcial,
        /// gris = Sin cuota generada. Puramente visual, no afecta otros informes.
        /// </summary>
        private void ColorearFilasPorEstado(DataTable datos)
        {
            if (!datos.Columns.Contains("Estado")) return;
            foreach (DataGridViewRow row in gridInforme.Rows)
            {
                if (row.Cells["Estado"]?.Value == null) continue;
                string estado = row.Cells["Estado"].Value.ToString();
                Color color;
                switch (estado)
                {
                    case "Pagado": color = Color.FromArgb(215, 245, 215); break;
                    case "No Pagado": color = Color.FromArgb(250, 214, 210); break;
                    case "Pago Parcial": color = Color.FromArgb(252, 243, 207); break;
                    default: color = Color.FromArgb(235, 235, 235); break; // Sin cuota generada
                }
                row.DefaultCellStyle.BackColor = color;
            }
        }

        private void ActualizarLabelTotales(DataTable datos)
        {
            var partes = new List<string>();
            foreach (DataColumn col in datos.Columns)
            {
                string n = col.ColumnName.ToLower();
                bool esNumerica = col.DataType == typeof(decimal) || col.DataType == typeof(double) ||
                                  col.DataType == typeof(float) || col.DataType == typeof(int) ||
                                  col.DataType == typeof(long);
                // Solo columnas monetarias relevantes — excluye Vuelto y Recibido
                bool esMonetaria = n.Contains("cobrado") || n.Contains("monto") ||
                                   n.Contains("pagado") || n.Contains("facturado") ||
                                   n.Contains("pendiente") || n.Contains("adeudado") ||
                                   n.Contains("recargo") || n.Contains("saldo");
                if (!esNumerica || !esMonetaria) continue;

                decimal suma = 0;
                foreach (DataRow row in datos.Rows)
                    if (row[col] != DBNull.Value) suma += Convert.ToDecimal(row[col]);

                partes.Add($"{col.ColumnName}: $ {suma:N2}");
            }

            if (partes.Count == 0)
            {
                _lblTotales.Visible = false;
                return;
            }

            _lblTotales.Text = "TOTAL  —  " + string.Join("     |     ", partes);
            _lblTotales.Visible = true;
        }

        public void MostrarMensaje(string mensaje, bool esError = false)
        {
            MessageBox.Show(mensaje,
                esError ? "Atención" : "Información",
                MessageBoxButtons.OK,
                esError ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
        }

        public void MostrarResumenCaja(int cantPagos, decimal totalCobrado,
                                       decimal totalRecibido, decimal totalVuelto,
                                       decimal totalGastos, decimal totalNeto)
        {
            panelResumenCaja.Visible = true;
            lblCajaCantPagos.Text = cantPagos.ToString();
            lblCajaCobrado.Text = $"$ {totalCobrado:N2}";
            lblCajaRecibido.Text = $"$ {totalRecibido:N2}";
            lblCajaVuelto.Text = $"$ {totalVuelto:N2}";
            lblCajaVuelto.ForeColor = totalVuelto > 0
                ? Color.FromArgb(231, 76, 60) : Color.FromArgb(26, 82, 118);

            lblCajaGastos.Text = $"$ {totalGastos:N2}";
            lblCajaNeto.Text = $"$ {totalNeto:N2}";
            lblCajaNeto.ForeColor = totalNeto >= 0
                ? Color.FromArgb(39, 174, 96)   // neto positivo: verde
                : Color.FromArgb(192, 57, 43);  // neto negativo: rojo (gastó más de lo que cobró)
        }

        // ══════════════════════════════════════════════════════════
        // Helper interno
        // ══════════════════════════════════════════════════════════
        private class AlumnoItem
        {
            public int Id { get; }
            public string Desc { get; }
            public AlumnoItem(int id, string desc) { Id = id; Desc = desc; }
            public override string ToString() => Desc;
        }
    }
}
