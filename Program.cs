using GestionEducativa.Database;
using GestionEducativa.Views;
using System;
using System.Configuration;
using System.Windows.Forms;

namespace GestionEducativa
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--generar-chromium-bundle")
            {
                try
                {
                    string ruta = Utils.HtmlToPdfConverter.GenerarCarpetaBundleParaInstaladorAsync()
                        .GetAwaiter().GetResult();
                    MessageBox.Show($"Chromium descargado en:\n{ruta}\n\n" +
                                     "Sumá la carpeta 'ChromiumBundled' al instalador.",
                                     "Listo", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error generando el bundle: {ex.Message}",
                                     "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                // Si la contraseña en App.config está en texto plano (o vacía),
                // mostramos el formulario de configuración de conexión.
                // Esto ocurre la PRIMERA VEZ o si alguien editó el App.config a mano.
                if (RequiereConfiguracion())
                {
                    using (var formConfig = new ConexionConfigForm())
                    {
                        if (formConfig.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                        {
                            MessageBox.Show(
                                "No se configuró la conexión. La aplicación no puede iniciar.",
                                "Configuración requerida",
                                MessageBoxButtons.OK,
                                MessageBoxIcon.Warning);
                            return;
                        }
                    }
                }

                try
                {
                    Utils.HtmlToPdfConverter.AsegurarNavegadorAsync().GetAwaiter().GetResult();
                }
                catch (Exception exPdf)
                {
                    MessageBox.Show(
                        $"No se pudo preparar el motor de generación de PDF: {exPdf.Message}\n" +
                        "La aplicación va a iniciar igual; los comprobantes se podrán enviar en HTML si esto vuelve a fallar.",
                        "Aviso",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                }


                Application.Run(new MainForm());
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Error al iniciar la aplicación:\n\n{ex.Message}\n\nContacte al soporte técnico.",
                    "Error de inicio",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Devuelve true si la contraseña en App.config NO está cifrada con DPAPI,
        /// lo que indica que hay que mostrar el formulario de configuración.
        /// </summary>
        private static bool RequiereConfiguracion()
        {
            try
            {
                var cs = ConfigurationManager.ConnectionStrings["GestionEducativa"];
                if (cs == null) return true;

                var builder = new MySql.Data.MySqlClient.MySqlConnectionStringBuilder(cs.ConnectionString);
                string pass = builder.Password;

                // Si la contraseña está vacía o en texto plano → mostrar formulario
                return !Utils.CifradoHelper.EstaCifrado(pass);
            }
            catch
            {
                return true;
            }
        }
    }
}
