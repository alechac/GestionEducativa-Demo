using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    /// <summary>
    /// Pantalla que se ve primero al abrir el sistema: un menú simple para elegir si se
    /// va a registrar un Cobro o un Gasto. El resto de las pestañas (Alumnos, Padres,
    /// Notas de Crédito, Informes, Configuración) se siguen usando igual, esta es solo
    /// una entrada rápida para las dos operaciones del día a día.
    /// </summary>
    public class InicioForm : Form
    {
        public event EventHandler IrACobrosClicked;
        public event EventHandler IrAGastosClicked;

        public InicioForm()
        {
            Dock = DockStyle.Fill;
            BackColor = Color.White;
            Font = new Font("Segoe UI", 9F);

            Construir();
        }

        private void Construir()
        {
            var lblBienvenida = new Label
            {
                Text = "¿Qué querés registrar hoy?",
                Font = new Font("Segoe UI", 18F, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 60, 117),
                AutoSize = true
            };
            Controls.Add(lblBienvenida);

            var panelBotones = new Panel { Size = new Size(760, 220) };
            Controls.Add(panelBotones);

            var btnCobros = CrearBotonGrande("💰", "Registrar Cobros", "Cobrar cuotas y conceptos a los alumnos", Color.FromArgb(39, 60, 118));
            btnCobros.Location = new Point(0, 0);
            btnCobros.Click += (s, e) => IrACobrosClicked?.Invoke(this, EventArgs.Empty);
            panelBotones.Controls.Add(btnCobros);

            var btnGastos = CrearBotonGrande("💸", "Registrar Gastos", "Cargar los egresos de la institución", Color.FromArgb(192, 57, 43));
            btnGastos.Location = new Point(380, 0);
            btnGastos.Click += (s, e) => IrAGastosClicked?.Invoke(this, EventArgs.Empty);
            panelBotones.Controls.Add(btnGastos);

            Resize += (s, e) => Centrar(lblBienvenida, panelBotones);
            Centrar(lblBienvenida, panelBotones);
        }

        private void Centrar(Label lbl, Panel panel)
        {
            lbl.Location = new Point((ClientSize.Width - lbl.Width) / 2, ClientSize.Height / 2 - 160);
            panel.Location = new Point((ClientSize.Width - panel.Width) / 2, ClientSize.Height / 2 - 80);
        }

        private Button CrearBotonGrande(string icono, string titulo, string subtitulo, Color color)
        {
            var btn = new Button
            {
                Size = new Size(360, 200),
                FlatStyle = FlatStyle.Flat,
                BackColor = color,
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                TextAlign = ContentAlignment.MiddleCenter,
                Text = $"{icono}\n\n{titulo}\n\n{subtitulo}"
            };
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = ControlPaint.Light(color, 0.15f);

            // El ícono grande arriba, el título abajo en negrita, el subtítulo chico —
            // se arma con Font distinto por línea usando un Label superpuesto sería más
            // prolijo, pero por simplicidad se deja todo en el mismo botón con un solo
            // tamaño de fuente grande y legible.
            btn.Font = new Font("Segoe UI", 13F, FontStyle.Bold);

            return btn;
        }
    }
}
