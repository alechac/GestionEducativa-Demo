using GestionEducativa.Models;
using GestionEducativa.Services;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Information;
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
    public partial class NuevoConceptoForm : Form
    {
        private readonly CuotaService _cuotaService;
        private readonly ConceptoService _conceptoService;
        private readonly int _anio;

        // Controles
        private TextBox txtNombre;
        private TextBox txtDescripcion;
        private ComboBox cmbCategoria;
        private TextBox txtMonto;
        private CheckedListBox chkNiveles;
        private CheckBox chkTodosLosNiveles;
        private NumericUpDown nudAnio;
        private Button btnGuardar;
        private Button btnCancelar;
        private Label lblInfo;

        // Resultado público
        public Concepto ConceptoCreado { get; private set; }
        /// <summary>
        /// Niveles seleccionados. NULL en la lista significa "todos los niveles".
        /// Si se eligió "Todos los niveles", se crea UN concepto con Nivel = null.
        /// Si se eligieron niveles específicos, se crean N conceptos (uno por nivel).
        /// </summary>
        public List<Concepto> ConceptosCreados { get; private set; } = new List<Concepto>();

        public NuevoConceptoForm(CuotaService cuotaService, ConceptoService conceptoService, int anio = 0)
        {
            _cuotaService = cuotaService ?? throw new ArgumentNullException(nameof(cuotaService));
            _conceptoService = conceptoService ?? throw new ArgumentNullException(nameof(conceptoService));
            _anio = anio > 0 ? anio : DateTime.Now.Year;
            InicializarComponentes();
            CargarNiveles();
        }

        private void InicializarComponentes()
        {
            this.Text = "Nuevo Concepto";
            this.Size = new Size(460, 600);
            this.StartPosition = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.BackColor = Color.FromArgb(245, 248, 252);
            this.Font = new Font("Segoe UI", 9.5F);

            int xLabel = 16;
            int xControl = 16;
            int wControl = 408;
            int y = 16;

            // ─── Título ───────────────────────────────────────────────
            var lblTitulo = new Label
            {
                AutoSize = false,
                Size = new Size(wControl, 32),
                Location = new Point(xLabel, y),
                Text = "➕  Crear nuevo concepto de cobro",
                Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 82, 118),
                TextAlign = ContentAlignment.MiddleLeft
            };
            y += 38;

            // ─── Nombre ───────────────────────────────────────────────
            AgregarLabel("Nombre del concepto *", xLabel, ref y);
            txtNombre = new TextBox
            {
                Location = new Point(xControl, y),
                Size = new Size(wControl, 26)
            };
            y += 34;

            // ─── Categoría ────────────────────────────────────────────
            AgregarLabel("Categoría *", xLabel, ref y);
            cmbCategoria = new ComboBox
            {
                Location = new Point(xControl, y),
                Size = new Size(wControl, 26),
                DropDownStyle = ComboBoxStyle.DropDown // Permite escribir una nueva o elegir de la lista
            };
            // Opciones por defecto opcionales (puedes personalizarlas o cargarlas de la BD)
            cmbCategoria.Items.AddRange(new object[] { "Otro", "Matrícula", "Uniforme" });
            cmbCategoria.SelectedIndex = 0;
            y += 34;

            // ─── Monto ────────────────────────────────────────────────
            AgregarLabel("Monto ($) — dejá 0 si se define al momento de cobrar", xLabel, ref y);
            txtMonto = new TextBox
            {
                Location = new Point(xControl, y),
                Size = new Size(200, 26),
                Text = "0"
            };
            txtMonto.Enter += (s, e) => txtMonto.SelectAll();
            y += 34;

            // ─── Año ──────────────────────────────────────────────────
            AgregarLabel("Año", xLabel, ref y);
            nudAnio = new NumericUpDown
            {
                Location = new Point(xControl, y),
                Size = new Size(100, 26),
                Minimum = 2020,
                Maximum = 2099,
                Value = _anio
            };
            y += 34;

            // ─── Niveles ──────────────────────────────────────────────
            AgregarLabel("Niveles que aplican *", xLabel, ref y);

            chkTodosLosNiveles = new CheckBox
            {
                AutoSize = true,
                Location = new Point(xControl, y),
                Text = "Todos los niveles",
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 174, 96),
                Checked = false
            };
            chkTodosLosNiveles.CheckedChanged += (s, e) =>
            {
                chkNiveles.Enabled = !chkTodosLosNiveles.Checked;
                if (chkTodosLosNiveles.Checked)
                    for (int i = 0; i < chkNiveles.Items.Count; i++)
                        chkNiveles.SetItemChecked(i, false);
            };
            y += 28;

            chkNiveles = new CheckedListBox
            {
                Location = new Point(xControl, y),
                Size = new Size(wControl, 90),
                CheckOnClick = true,
                BorderStyle = BorderStyle.FixedSingle,
                BackColor = Color.White
            };
            y += 98;

            // ─── Descripción (opcional) ───────────────────────────────
            AgregarLabel("Descripción / nota interna (opcional)", xLabel, ref y);
            txtDescripcion = new TextBox
            {
                Location = new Point(xControl, y),
                Size = new Size(wControl, 50),
                Multiline = true,
                ScrollBars = ScrollBars.Vertical
            };
            y += 58;

            // ─── Info ─────────────────────────────────────────────────
            lblInfo = new Label
            {
                AutoSize = false,
                Size = new Size(wControl, 18),
                Location = new Point(xLabel, y),
                ForeColor = Color.FromArgb(149, 165, 166),
                Font = new Font("Segoe UI", 8F, FontStyle.Italic),
                Text = ""
            };
            y += 24;

            // ─── Botones ──────────────────────────────────────────────
            btnGuardar = new Button
            {
                Text = "💾  Guardar concepto",
                Location = new Point(xControl, y),
                Size = new Size(200, 34),
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            btnGuardar.FlatAppearance.BorderSize = 0;
            btnGuardar.Click += BtnGuardar_Click;

            btnCancelar = new Button
            {
                Text = "Cancelar",
                Location = new Point(224, y),
                Size = new Size(110, 34),
                BackColor = Color.FromArgb(149, 165, 166),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancelar.FlatAppearance.BorderSize = 0;

            this.AcceptButton = btnGuardar;
            this.CancelButton = btnCancelar;

            this.Controls.AddRange(new Control[]
            {
                lblTitulo, txtNombre, cmbCategoria, txtMonto, nudAnio,
                chkTodosLosNiveles, chkNiveles, txtDescripcion, lblInfo,
                btnGuardar, btnCancelar
            });

            // Ajustar alto del form según contenido
            this.ClientSize = new Size(440, y + 50);
        }

        private void AgregarLabel(string texto, int x, ref int y)
        {
            var lbl = new Label
            {
                AutoSize = true,
                Location = new Point(x, y),
                Text = texto,
                ForeColor = Color.FromArgb(80, 80, 80),
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold)
            };
            this.Controls.Add(lbl);
            y += 20;
        }

        private void CargarNiveles()
        {
            chkNiveles.Items.Clear();
            try
            {
                var niveles = _cuotaService.ObtenerNivelesExistentes();
                foreach (var n in niveles)
                    chkNiveles.Items.Add(n, false);

                if (niveles.Count == 0)
                    lblInfo.Text = "⚠ No hay niveles en la BD; se aplicará a todos automáticamente.";
            }
            catch
            {
                // Si falla, el usuario puede elegir "Todos"
                lblInfo.Text = "No se pudieron cargar los niveles. Usá 'Todos los niveles'.";
                chkTodosLosNiveles.Checked = true;
            }
        }

        private void BtnGuardar_Click(object sender, EventArgs e)
        {
            // ─── Validaciones ─────────────────────────────────────────
            string nombre = txtNombre.Text.Trim();
            if (string.IsNullOrEmpty(nombre))
            {
                MostrarError("Ingresá un nombre para el concepto.");
                txtNombre.Focus();
                return;
            }

            string categoria = cmbCategoria.Text.Trim();
            if (string.IsNullOrEmpty(categoria))
            {
                MostrarError("Ingresá o seleccioná una categoría.");
                cmbCategoria.Focus();
                return;
            }
            // Parsear monto
            string montoStr = txtMonto.Text.Replace(".", "").Replace(",", ".");
            if (!decimal.TryParse(montoStr,
                System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture,
                out decimal monto) || monto < 0)
            {
                MostrarError("El monto debe ser un número mayor o igual a cero.");
                txtMonto.Focus();
                return;
            }

            bool todosNiveles = chkTodosLosNiveles.Checked;
            var nivelesSeleccionados = chkNiveles.CheckedItems.Cast<string>().ToList();

            if (!todosNiveles && nivelesSeleccionados.Count == 0)
            {
                MostrarError("Seleccioná al menos un nivel o marcá 'Todos los niveles'.");
                return;
            }

            int anio = (int)nudAnio.Value;
            string descripcion = txtDescripcion.Text.Trim();

            // ─── Confirmar ────────────────────────────────────────────
            string resNiveles = todosNiveles ? "TODOS los niveles"
                : string.Join(", ", nivelesSeleccionados);
            string montoTexto = monto > 0 ? $"$ {monto:N2}" : "A definir al cobrar";

            string msg = $"¿Crear el concepto?\n\n" +
                         $"   Nombre:    {nombre}\n" +
                         $" Categoria : {categoria }\n" +
                         $"   Monto:     {montoTexto}\n" +
                         $"   Año:       {anio}\n" +
                         $"   Niveles:   {resNiveles}";

            if (MessageBox.Show(msg, "Confirmar nuevo concepto",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            // ─── Guardar ──────────────────────────────────────────────
            try
            {
                btnGuardar.Enabled = false;
                btnGuardar.Text = "Guardando...";

                ConceptosCreados.Clear();

                if (todosNiveles || nivelesSeleccionados.Count == 0)
                {
                    // Un concepto para todos los niveles (nivel = null)
                    var c = _conceptoService.CrearConcepto(nombre, categoria, null, monto, anio, descripcion);
                    ConceptosCreados.Add(c);
                }
                else if (nivelesSeleccionados.Count == 1)
                {
                    // Un solo nivel
                    var c = _conceptoService.CrearConcepto(nombre, categoria, nivelesSeleccionados[0], monto, anio, descripcion);
                    ConceptosCreados.Add(c);
                }
                else
                {
                    // Múltiples niveles → un concepto por nivel
                    foreach (var nivel in nivelesSeleccionados)
                    {
                        var c = _conceptoService.CrearConcepto(nombre, categoria, nivel, monto, anio, descripcion);
                        ConceptosCreados.Add(c);
                    }
                }

                ConceptoCreado = ConceptosCreados.FirstOrDefault();

                MessageBox.Show(
                    $"✓ Concepto \"{nombre}\" creado correctamente.\n" +
                    $"Ya aparece en la lista de conceptos disponibles.",
                    "Concepto creado", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                btnGuardar.Enabled = true;
                btnGuardar.Text = "💾  Guardar concepto";
                MostrarError("Error al guardar: " + ex.Message);
            }
        }

        private void MostrarError(string mensaje)
        {
            MessageBox.Show(mensaje, "Validación", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
