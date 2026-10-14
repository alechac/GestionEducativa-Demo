using System;
using System.Drawing;
using System.Windows.Forms;

namespace GestionEducativa.Views
{
    /// <summary>
    /// Ventanita de "cargando" sin bordes, siempre arriba, con un mensaje. Se usa junto con
    /// <see cref="BloqueoUI"/> para tapar toda la aplicación mientras se manda un mail —
    /// así el cajero no puede seguir tocando cosas (cambiar de pestaña, cobrar otro alumno,
    /// etc.) hasta que termine ese envío.
    /// </summary>
    public class CargandoOverlay : Form
    {
        public CargandoOverlay(string mensaje)
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            Size = new Size(340, 110);
            BackColor = Color.White;
            TopMost = true;
            ShowInTaskbar = false;

            var lbl = new Label
            {
                Text = "⏳  " + mensaje,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                ForeColor = Color.FromArgb(39, 60, 118)
            };
            Controls.Add(lbl);

            Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(52, 152, 219), 2))
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            };
        }
    }

    /// <summary>
    /// Muestra/oculta el overlay de "cargando" y, mientras tanto, deshabilita el resto de
    /// la aplicación (todas las ventanas abiertas) para que no se pueda seguir operando el
    /// sistema hasta que termine la tarea de fondo (ej: convertir a PDF y mandar el mail).
    /// Se llama en pares: Mostrar() antes de la tarea, Ocultar() en un finally después.
    /// </summary>
    public static class BloqueoUI
    {
        private static CargandoOverlay _overlay;

        public static void Mostrar(string mensaje = "Enviando comprobante por mail, un momento...")
        {
            foreach (Form f in Application.OpenForms)
                f.Enabled = false;

            _overlay = new CargandoOverlay(mensaje);
            _overlay.Enabled = true;
            _overlay.Show();
            _overlay.Refresh();
        }

        public static void Ocultar()
        {
            try { _overlay?.Close(); }
            catch { /* no crítico */ }
            finally { _overlay = null; }

            foreach (Form f in Application.OpenForms)
                f.Enabled = true;
        }
    }
}
