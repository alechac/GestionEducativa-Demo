using GestionEducativa.Models;
using GestionEducativa.Views;
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
    public partial class MainForm : Form
    {
        public int SeccionActual { get; private set; } = 1;
        public event EventHandler SeccionCambiada;

        private TabControl tabPrincipal;
        private TabPage tabInicio, tabAlumnos, tabPadres, tabPagos, tabGastos, tabNotasCredito, tabInformes, tabConfiguracion;
        private Panel panelSeccion;
        private Button btnEscuela, btnItaliano;
        private Label lblTitulo;

        // Formularios hijos embebidos
        private InicioForm _formInicio;
        private AlumnosForm _formAlumnos;
        private PadresForm _formPadres;
        private PagosForm _formPagos;
        private GastosForm _formGastos;
        private NotaCreditoForm _formNotasCredito;
        private InformesForm _formInformes;
        private ConfiguracionForm _formConfiguracion;

        public MainForm()
        {
            InitializeComponent();
            PedirIdentificacionDelOperador();
            InicializarFormulariosHijos();
            CambiarSeccion(1); // aplica el resaltado correcto desde el arranque (Escuela por defecto)
        }

        /// <summary>
        /// No es login: si cancela o cierra la ventana sin escribir nada, la app sigue
        /// funcionando igual (queda anotado como "Sin identificar" en el log). Sólo
        /// busca que, cuando alguien SÍ pone su nombre, quede identificado en el
        /// registro de auditoría de ahí en adelante.
        /// </summary>
        private void PedirIdentificacionDelOperador()
        {
            using (var dlg = new IdentificacionOperadorForm())
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(dlg.NombreIngresado))
                    GestionEducativa.Utils.SesionActual.Operador = dlg.NombreIngresado;
            }
        }

        private void InicializarFormulariosHijos()
        {
            _formInicio = new InicioForm();
            _formInicio.IrACobrosClicked += (s, e) => tabPrincipal.SelectedTab = tabPagos;
            _formInicio.IrAGastosClicked += (s, e) => tabPrincipal.SelectedTab = tabGastos;
            EmbedirFormulario(_formInicio, tabInicio);

            _formAlumnos = new AlumnosForm(SeccionActual);
            EmbedirFormulario(_formAlumnos, tabAlumnos);

            _formPadres = new PadresForm();
            EmbedirFormulario(_formPadres, tabPadres);

            _formPagos = new PagosForm(SeccionActual);
            EmbedirFormulario(_formPagos, tabPagos);

            _formGastos = new GastosForm(SeccionActual);
            EmbedirFormulario(_formGastos, tabGastos);

            _formNotasCredito = new NotaCreditoForm();
            EmbedirFormulario(_formNotasCredito, tabNotasCredito);

            _formInformes = new InformesForm();
            EmbedirFormulario(_formInformes, tabInformes);

            _formConfiguracion = new ConfiguracionForm();
            EmbedirFormulario(_formConfiguracion, tabConfiguracion);
        }

        private void EmbedirFormulario(Form form, TabPage tab)
        {
            form.TopLevel = false;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Dock = DockStyle.Fill;
            tab.Controls.Add(form);
            form.Show();
        }

        private void CambiarSeccion(int nueva)
        {
            SeccionActual = nueva;

            // La sección inactiva queda gris neutro (no otro tono de azul, que se confundía
            // con la activa). La activa queda con color sólido + borde grueso bien visible.
            bool esEscuela = nueva == 1;
            EstilizarBotonSeccion(btnEscuela, activo: esEscuela, colorActivo: Color.FromArgb(26, 82, 118));
            EstilizarBotonSeccion(btnItaliano, activo: !esEscuela, colorActivo: Color.FromArgb(0, 135, 81));

            lblTitulo.Text = nueva == 1
                ? "Institución Educativa  —  Escuela"
                : "Institución Educativa  —  Scuola Italiano";

            // Propagar a formularios hijos
            _formAlumnos.CambiarSeccion(nueva);
            _formPagos.CambiarSeccion(nueva);
            _formGastos.CambiarSeccion(nueva);

            SeccionCambiada?.Invoke(this, EventArgs.Empty);
        }

        private static readonly Color GrisInactivo = Color.FromArgb(210, 210, 210);
        private static readonly Color TextoInactivo = Color.FromArgb(90, 90, 90);

        private void EstilizarBotonSeccion(Button btn, bool activo, Color colorActivo)
        {
            string textoBase = btn.Text.TrimStart('✓', ' ');
            if (activo)
            {
                btn.BackColor = colorActivo;
                btn.ForeColor = Color.White;
                btn.FlatAppearance.BorderColor = Color.White;
                btn.FlatAppearance.BorderSize = 3;
                btn.Text = "✓ " + textoBase;
                btn.Font = new Font(btn.Font, FontStyle.Bold);
            }
            else
            {
                btn.BackColor = GrisInactivo;
                btn.ForeColor = TextoInactivo;
                btn.FlatAppearance.BorderColor = Color.FromArgb(170, 170, 170);
                btn.FlatAppearance.BorderSize = 1;
                btn.Text = textoBase;
                btn.Font = new Font(btn.Font, FontStyle.Regular);
            }
        }

        private void btnEscuela_Click(object sender, EventArgs e) => CambiarSeccion(1);
        private void btnItaliano_Click(object sender, EventArgs e) => CambiarSeccion(2);

    }
}
