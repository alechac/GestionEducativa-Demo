using GestionEducativa.Models;
using GestionEducativa.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    public class ConceptosPanel : Panel
    {
        private readonly ConceptoService _pagoService; // ObtenerConceptos
        private readonly CuotaService _cuotaService;   // GenerarCuotaConcepto

        private Alumno _alumnoActual;
        private bool _esPreinscriptoActual;
        private List<Concepto> _conceptos = new List<Concepto>();

        public event EventHandler<ConceptoGeneradoEventArgs> ConceptoGenerado;

        // Controles internos
        private Label lblTitulo;
        private Label lblAlumno;
        private Label lblSinConceptos;
        private Label lblError;
        private Button btnNuevoConcepto;
        // El scroll lo maneja un Panel interno con AutoScroll
        private Panel panelScroll;

        public ConceptosPanel()
        {
            _pagoService = new ConceptoService();
            _cuotaService = new CuotaService();
            InicializarUI();
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return;
        }

        private void InicializarUI()
        {
            this.BackColor = Color.FromArgb(250, 252, 255);
            this.BorderStyle = BorderStyle.FixedSingle;
            // NO AutoScroll en el panel raíz — lo gestiona panelScroll interno
            this.AutoScroll = false;

            lblTitulo = new Label
            {
                AutoSize = true,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 82, 118),
                Location = new Point(8, 8),
                Text = "📋 Otros Conceptos"
            };

            btnNuevoConcepto = new Button
            {
                AutoSize = false,
                Size = new Size(130, 22),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(52, 152, 219),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                Text = "➕ Nuevo concepto",
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right
            };
            btnNuevoConcepto.FlatAppearance.BorderSize = 0;
            btnNuevoConcepto.Click += BtnNuevoConcepto_Click;

            lblAlumno = new Label
            {
                AutoSize = false,
                Size = new Size(250, 18),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8.5F),
                Location = new Point(8, 32),
                Text = "Seleccione un alumno"
            };

            lblSinConceptos = new Label
            {
                AutoSize = false,
                Size = new Size(250, 32),
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 8.5F, FontStyle.Italic),
                Text = "No hay conceptos disponibles\npara este nivel/año.",
                Visible = false,
                Location = new Point(8, 58)
            };

            lblError = new Label
            {
                AutoSize = false,
                Size = new Size(250, 40),
                ForeColor = Color.FromArgb(192, 57, 43),
                Font = new Font("Segoe UI", 8F),
                Visible = false,
                Location = new Point(8, 58)
            };

            // Panel interno con scroll que ocupa el espacio debajo de los labels de cabecera
            panelScroll = new Panel
            {
                Location = new Point(0, 54),
                AutoScroll = true,
                BackColor = Color.FromArgb(250, 252, 255)
            };

            this.Controls.Add(lblTitulo);
            this.Controls.Add(btnNuevoConcepto);
            this.Controls.Add(lblAlumno);
            this.Controls.Add(lblSinConceptos);
            this.Controls.Add(lblError);
            this.Controls.Add(panelScroll);

            // Ajustar panelScroll cuando cambia el tamaño del panel raíz
            this.Resize += (s, e) => AjustarTamanoPanelScroll();
            AjustarTamanoPanelScroll();
        }

        private void AjustarTamanoPanelScroll()
        {
            if (panelScroll == null) return;
            int y = 54;
            panelScroll.Location = new Point(0, y);
            panelScroll.Size = new Size(this.Width, Math.Max(0, this.Height - y));

            // Posicionar el botón "Nuevo concepto" alineado a la derecha del título
            if (btnNuevoConcepto != null)
                btnNuevoConcepto.Location = new Point(Math.Max(8, this.Width - btnNuevoConcepto.Width - 6), 8);
        }

        public void CargarConceptos(Alumno alumno, int anio = 0)
        {
            _alumnoActual = alumno;
            if (anio == 0) anio = DateTime.Now.Year;

            panelScroll.Controls.Clear();
            lblSinConceptos.Visible = false;
            lblError.Visible = false;

            if (alumno == null)
            {
                lblAlumno.Text = "Seleccione un alumno";
                return;
            }

            lblAlumno.Text = alumno.Nombre;

            try
            {
                bool esPreinscripto = alumno.EsPreinscripto(DateTime.Now.Year);
                _esPreinscriptoActual = esPreinscripto;
                _conceptos = _pagoService.ObtenerConceptos(anio, alumno.Nivel, esPreinscripto);
            }
            catch (Exception ex)
            {
                _conceptos = new List<Concepto>();
                lblError.Text = "Error al cargar: " + ex.Message;
                lblError.Visible = true;
                return;
            }

            if (_conceptos.Count == 0)
            {
                lblSinConceptos.Visible = true;
                return;
            }

            RenderizarConceptos();
        }

        private void RenderizarConceptos()
        {
            panelScroll.Controls.Clear();

            int anchoUtil = panelScroll.Width > 20 ? panelScroll.Width - 4 : 260;
            int y = 4;

            var categorias = _conceptos
                .GroupBy(c => c.Categoria ?? "Otro")
                .OrderBy(g => OrdenCategoria(g.Key));

            foreach (var grupo in categorias)
            {
                // ---- Header de categoría ----
                var lblCat = new Label
                {
                    AutoSize = false,
                    Size = new Size(anchoUtil, 22),
                    Location = new Point(2, y),
                    Text = ObtenerIconoCategoria(grupo.Key) + "  " + grupo.Key.ToUpper(),
                    Font = new Font("Segoe UI", 8F, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = ObtenerColorCategoria(grupo.Key),
                    Padding = new Padding(6, 3, 0, 0),
                    TextAlign = ContentAlignment.MiddleLeft
                };
                panelScroll.Controls.Add(lblCat);
                y += 26;

                foreach (var concepto in grupo)
                {
                    var fila = CrearFilaConcepto(concepto, anchoUtil, y);
                    panelScroll.Controls.Add(fila);
                    y += 38;
                }

                y += 4; // espacio entre categorías
            }
        }

        /// <summary>
        /// Ahora que el catálogo de conceptos tiene una fila de matrícula POR AÑO real
        /// (una para 2026, otra para 2027, cada una con su propio `anio` ya correcto —
        /// ver ObtenerConceptos), no hace falta calcular ni comparar nada: el año a
        /// mostrar y a grabar es directamente concepto.Anio.
        /// </summary>
        private string TextoConAclaracionDeAnio(Concepto concepto)
        {
            bool esMatricula = string.Equals(concepto.Categoria, "matricula", StringComparison.OrdinalIgnoreCase);
            if (!esMatricula || _esPreinscriptoActual)
                return concepto.Nombre;

            return concepto.Nombre + $" ({concepto.Anio})";
        }

        private Panel CrearFilaConcepto(Concepto concepto, int anchoUtil, int yPos)
        {
            var fila = new Panel
            {
                Location = new Point(2, yPos),
                Size = new Size(anchoUtil, 34),
                BackColor = Color.White,
                Tag = concepto
            };

            // Línea separadora inferior
            fila.Paint += (s, e) =>
            {
                e.Graphics.DrawLine(
                    new Pen(Color.FromArgb(230, 230, 230)),
                    0, fila.Height - 1, fila.Width, fila.Height - 1);
            };

            int anchoBoton = 62;
            int anchoMonto = 80;
            int anchoNombre = anchoUtil - anchoBoton - anchoMonto - 10;

            var lblNombre = new Label
            {
                AutoSize = false,
                Size = new Size(anchoNombre, 34),
                Location = new Point(4, 0),
                Text = TextoConAclaracionDeAnio(concepto),
                Font = new Font("Segoe UI", 8.5F),
                TextAlign = ContentAlignment.MiddleLeft,
                ForeColor = Color.FromArgb(40, 40, 40)
            };

            var lblMonto = new Label
            {
                AutoSize = false,
                Size = new Size(anchoMonto, 34),
                Location = new Point(4 + anchoNombre, 0),
                Text = concepto.Monto > 0 ? $"$ {concepto.Monto:N0}" : "A definir",
                Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                ForeColor = concepto.Monto > 0
                    ? Color.FromArgb(26, 82, 118)
                    : Color.FromArgb(160, 160, 160),
                TextAlign = ContentAlignment.MiddleRight
            };

            var btnGenerar = new Button
            {
                Size = new Size(anchoBoton, 24),
                Location = new Point(anchoUtil - anchoBoton - 4, 5),
                Text = "Generar",
                Tag = concepto,
                FlatStyle = FlatStyle.Flat,
                BackColor = concepto.Monto > 0
                    ? Color.FromArgb(39, 174, 96)
                    : Color.FromArgb(200, 200, 200),
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                Cursor = Cursors.Hand,
                Enabled = true // siempre habilitado, el monto se puede ingresar manualmente si es 0 o categoría matrícula
            };
            btnGenerar.FlatAppearance.BorderSize = 0;
            btnGenerar.Click += BtnGenerar_Click;

            fila.Controls.Add(lblNombre);
            fila.Controls.Add(lblMonto);
            fila.Controls.Add(btnGenerar);

            return fila;
        }

        private string MostrarInputMonto(string mensaje)
        {
            string resultado = null;

            using (var dlg = new Form())
            {
                dlg.Text = "Ingresar Monto";
                dlg.Size = new Size(380, 200);
                dlg.StartPosition = FormStartPosition.CenterParent;
                dlg.FormBorderStyle = FormBorderStyle.FixedDialog;
                dlg.MaximizeBox = false;
                dlg.MinimizeBox = false;
                dlg.Font = new Font("Segoe UI", 9.5F);

                var lbl = new Label
                {
                    AutoSize = false,
                    Size = new Size(340, 60),
                    Location = new Point(16, 12),
                    Text = mensaje
                };

                var txt = new TextBox
                {
                    Location = new Point(16, 80),
                    Size = new Size(340, 26),
                    Font = new Font("Segoe UI", 12F, FontStyle.Bold),
                    Text = "0"
                };
                txt.Enter += (s, e) => txt.SelectAll();

                var btnOk = new Button
                {
                    Text = "Aceptar",
                    DialogResult = DialogResult.OK,
                    Location = new Point(170, 118),
                    Size = new Size(90, 30),
                    BackColor = Color.FromArgb(39, 174, 96),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnOk.FlatAppearance.BorderSize = 0;

                var btnCancelar = new Button
                {
                    Text = "Cancelar",
                    DialogResult = DialogResult.Cancel,
                    Location = new Point(270, 118),
                    Size = new Size(90, 30),
                    BackColor = Color.FromArgb(149, 165, 166),
                    ForeColor = Color.White,
                    FlatStyle = FlatStyle.Flat
                };
                btnCancelar.FlatAppearance.BorderSize = 0;

                dlg.AcceptButton = btnOk;
                dlg.CancelButton = btnCancelar;
                dlg.Controls.Add(lbl);
                dlg.Controls.Add(txt);
                dlg.Controls.Add(btnOk);
                dlg.Controls.Add(btnCancelar);

                if (dlg.ShowDialog() == DialogResult.OK)
                    resultado = txt.Text.Trim();
            }

            return resultado;
        }

        private void BtnNuevoConcepto_Click(object sender, EventArgs e)
        {
            int anio = DateTime.Now.Year;
            using (var form = new NuevoConceptoForm(_cuotaService, _pagoService, anio))
            {
                if (form.ShowDialog(this.FindForm()) == System.Windows.Forms.DialogResult.OK
                    && form.ConceptosCreados.Count > 0)
                {
                    // Recargar la lista de conceptos para que aparezca el nuevo
                    if (_alumnoActual != null)
                        CargarConceptos(_alumnoActual, anio);
                    else
                    {
                        // Sin alumno seleccionado, igual recargamos internamente
                        try
                        {
                            _conceptos = _pagoService.ObtenerConceptos(anio, null);
                            RenderizarConceptos();
                        }
                        catch { /* no-op */ }
                    }
                }
            }
        }

        private void BtnGenerar_Click(object sender, EventArgs e)
        {
            if (_alumnoActual == null) return;

            var concepto = (Concepto)((Button)sender).Tag;
            decimal montoOverride = 0;

            // Para matrícula el monto se ingresa manualmente en cada caso
            bool esMatricula = concepto.Categoria.Contains( "Matricula" );

            if (esMatricula || concepto.Monto == 0)
            {
                // Pedir el monto al operador con un form propio (sin dependencia de Microsoft.VisualBasic)
                string input = MostrarInputMonto(
                    $"Ingrese el monto de \"{concepto.Nombre}\"\npara {_alumnoActual.Nombre}:\n" +
                    "(El monto puede variar según el alumno)");

                if (input == null) return; // canceló
                input = input.Replace(".", "").Replace(",", ".");
                if (!decimal.TryParse(input,
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out montoOverride) || montoOverride <= 0)
                {
                    MessageBox.Show("Ingrese un monto válido mayor a cero.", "Monto inválido",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            // Confirmación
            decimal montoMostrar = montoOverride > 0 ? montoOverride : concepto.Monto;


            string msg = $"¿Generar \"{concepto.Nombre}\" para {_alumnoActual.Nombre}?\n" +
                         $"Monto: $ {montoMostrar:N2}";

            if (MessageBox.Show(msg, "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                != DialogResult.Yes) return;

            try
            {
                _cuotaService.GenerarCuotaConcepto(
                    _alumnoActual.Id,
                    concepto.Id,
                    concepto.Anio, // el catálogo ya tiene el año correcto para cada matrícula (2026 o 2027)
                    _alumnoActual.PorcentajeDescuento,
                    montoOverride
                );

                MessageBox.Show(
                    $"✓ Cuota de \"{concepto.Nombre}\" generada.\nYa aparece en el listado de cuotas.",
                    "Listo", MessageBoxButtons.OK, MessageBoxIcon.Information);

                ConceptoGenerado?.Invoke(this, new ConceptoGeneradoEventArgs
                {
                    Alumno = _alumnoActual,
                    Concepto = concepto
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // También volvemos a renderizar si el panel cambia de tamaño (ej: form maximizado)
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            AjustarTamanoPanelScroll();
            if (_conceptos != null && _conceptos.Count > 0)
                RenderizarConceptos();
        }

        // ---- Helpers ----

        private static int OrdenCategoria(string cat)
        {
            switch (cat?.ToLower())
            {
                case "matricula": return 0;
                case "seguro": return 1;
                case "material": return 2;
                case "viaje": return 3;
                case "bingo": return 4;
                default: return 9;
            }
        }

        private static string ObtenerIconoCategoria(string categoria)
        {
            switch (categoria?.ToLower())
            {
                case "seguro": return "🛡";
                case "viaje": return "✈";
                case "matricula": return "📝";
                case "material": return "📚";
                case "bingo": return "🎱";
                default: return "💲";
            }
        }

        private static Color ObtenerColorCategoria(string categoria)
        {
            switch (categoria?.ToLower())
            {
                case "seguro": return Color.FromArgb(52, 152, 219);
                case "viaje": return Color.FromArgb(155, 89, 182);
                case "matricula": return Color.FromArgb(230, 126, 34);
                case "material": return Color.FromArgb(22, 160, 133);
                case "bingo": return Color.FromArgb(192, 57, 43);
                default: return Color.FromArgb(127, 140, 141);
            }
        }
    }

    /// <summary>Datos del evento ConceptoGenerado.</summary>
    public class ConceptoGeneradoEventArgs : EventArgs
    {
        public Alumno Alumno { get; set; }
        public Concepto Concepto { get; set; }
        /// <summary>
        /// Muestra un diálogo simple para ingresar un monto decimal.
        /// Devuelve el string ingresado, o null si el usuario canceló.
        /// </summary>

    }
}

