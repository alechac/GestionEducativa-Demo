using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace GestionEducativa.Utils
{
    /// <summary>
    /// Convierte los comprobantes HTML (generados por PdfGenerator) a PDF real,
    /// usando Chromium headless vía PuppeteerSharp. Se eligió este enfoque porque
    /// el HTML de los comprobantes usa CSS moderno (display:flex en header/footer/QR)
    /// que las librerías "livianas" de HTML→PDF para .NET (HtmlRenderer.PdfSharp, etc.)
    /// no soportan correctamente. Con un navegador real el PDF queda pixel-perfect
    /// respecto de lo que se ve al abrir el .html.
    ///
    /// ESTRATEGIA (tras el error "Failed to download ChromeHeadlessShell" en PC de
    /// cliente sin acceso a storage.googleapis.com): se prueban tres fuentes de
    /// navegador, EN ESTE ORDEN, y la primera que funcione se usa — ninguna de las
    /// dos primeras requiere internet en la PC del cliente:
    ///
    ///   1) Chromium "empaquetado": una copia ya descargada que viaja DENTRO de la
    ///      carpeta de instalación de la app (subcarpeta "ChromiumBundled"), incluida
    ///      en el instalador. Se genera UNA VEZ en la PC del desarrollador (ver
    ///      GenerarCarpetaBundleParaInstalador más abajo) y after eso nunca más se
    ///      descarga nada, ni en dev ni en el cliente.
    ///   2) Chrome o Edge YA INSTALADO en la PC (rutas típicas de Program Files).
    ///      Cubre el caso de que por algún motivo la carpeta bundled no se haya
    ///      copiado bien con el instalador.
    ///   3) Descarga on-the-fly de Chromium vía PuppeteerSharp (comportamiento
    ///      original). Solo se llega acá si fallan las dos anteriores; en una PC de
    ///      cliente sin internet esto va a fallar igual que antes, pero el llamador
    ///      (PagosPresenter) ya tiene un catch que hace fallback a enviar el HTML sin
    ///      convertir, así que el flujo de cobro nunca se rompe por esto.
    /// </summary>
    public static class HtmlToPdfConverter
    {
        // Se cachea entre llamadas para no repetir la búsqueda/descarga cada vez.
        private static bool _navegadorListo = false;
        private static string _executablePath = null;
        private static readonly object _lock = new object();

        // Carpeta con el Chromium empaquetado dentro de la instalación de la app.
        // Debe coincidir con la carpeta que se genera con GenerarCarpetaBundleParaInstalador
        // y que se incluye en el instalador (mismo nombre, al lado del .exe).
        private static readonly string CarpetaChromiumBundled =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ChromiumBundled");

        // Rutas típicas donde Windows instala Chrome/Edge (se prueban en este orden).
        private static readonly string[] RutasNavegadoresConocidos = new[]
        {
            Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Google\Chrome\Application\chrome.exe"),
            Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Google\Chrome\Application\chrome.exe"),
            Environment.ExpandEnvironmentVariables(@"%LocalAppData%\Google\Chrome\Application\chrome.exe"),
            Environment.ExpandEnvironmentVariables(@"%ProgramFiles(x86)%\Microsoft\Edge\Application\msedge.exe"),
            Environment.ExpandEnvironmentVariables(@"%ProgramFiles%\Microsoft\Edge\Application\msedge.exe"),
        };

        /// <summary>
        /// Busca, SIN DESCARGAR NADA, un Chromium ya presente en CarpetaChromiumBundled
        /// (la que viaja empaquetada con el instalador). Devuelve la ruta al ejecutable
        /// o null si esa carpeta no existe o está vacía.
        /// </summary>
        private static string BuscarChromiumBundled()
        {
            if (!Directory.Exists(CarpetaChromiumBundled))
                return null;

            try
            {
                var fetcher = new BrowserFetcher(new BrowserFetcherOptions { Path = CarpetaChromiumBundled });
                var instalados = fetcher.GetInstalledBrowsers()?.ToList();
                var encontrado = instalados?.FirstOrDefault();
                if (encontrado != null && File.Exists(encontrado.GetExecutablePath()))
                    return encontrado.GetExecutablePath();
            }
            catch
            {
                // Carpeta corrupta/incompleta: seguimos con las otras estrategias.
            }

            return null;
        }

        /// <summary>
        /// Busca un Chrome o Edge ya instalado en la PC. Devuelve la ruta al .exe
        /// o null si no encontró ninguno.
        /// </summary>
        private static string BuscarNavegadorInstalado()
        {
            foreach (var ruta in RutasNavegadoresConocidos)
            {
                if (File.Exists(ruta))
                    return ruta;
            }
            return null;
        }

        /// <summary>
        /// Prepara el motor de PDF probando, en orden: Chromium bundled → Chrome/Edge
        /// instalado → descarga on-the-fly. Llamar una vez al iniciar la app (Program.cs)
        /// para que esto no ocurra "de sorpresa" en medio del primer cobro del día.
        /// Si ya está listo, no hace nada y vuelve al toque.
        /// </summary>
        public static async Task AsegurarNavegadorAsync()
        {
            if (_navegadorListo) return;

            string bundled = BuscarChromiumBundled();
            if (bundled != null)
            {
                lock (_lock) { _executablePath = bundled; _navegadorListo = true; }
                return;
            }

            string instalado = BuscarNavegadorInstalado();
            if (instalado != null)
            {
                lock (_lock) { _executablePath = instalado; _navegadorListo = true; }
                return;
            }

            // Última opción: descarga on-the-fly (requiere internet). Si falla acá,
            // se propaga la excepción y el llamador decide qué hacer (fallback a HTML).
            var browserFetcher = new BrowserFetcher();
            await browserFetcher.DownloadAsync();

            lock (_lock) { _executablePath = null; _navegadorListo = true; }
        }

        /// <summary>
        /// USO ÚNICO EN LA PC DEL DESARROLLADOR: descarga Chromium a CarpetaChromiumBundled
        /// para después copiar esa carpeta dentro del instalador. Ejecutar esto una vez
        /// (por ejemplo desde un botón oculto de "Configuración" o un pequeño programa de
        /// consola aparte) cada vez que se quiera actualizar la versión de Chromium a
        /// empaquetar. NO llamar desde el flujo normal de la app.
        /// </summary>
        public static async Task<string> GenerarCarpetaBundleParaInstaladorAsync()
        {
            Directory.CreateDirectory(CarpetaChromiumBundled);
            var fetcher = new BrowserFetcher(new BrowserFetcherOptions { Path = CarpetaChromiumBundled });
            var instalado = await fetcher.DownloadAsync();
            return instalado.GetExecutablePath();
        }

        /// <summary>
        /// Convierte un archivo .html ya generado a .pdf (mismo nombre, extensión .pdf).
        /// Devuelve la ruta del PDF resultante.
        /// </summary>
        public static string ConvertirArchivo(string rutaHtml)
        {
            return ConvertirArchivoAsync(rutaHtml).GetAwaiter().GetResult();
        }

        public static async Task<string> ConvertirArchivoAsync(string rutaHtml)
        {
            if (string.IsNullOrWhiteSpace(rutaHtml) || !File.Exists(rutaHtml))
                throw new FileNotFoundException("No se encontró el HTML del comprobante para convertir a PDF.", rutaHtml);

            string rutaPdf = Path.ChangeExtension(rutaHtml, ".pdf");

            await AsegurarNavegadorAsync();

            var opcionesLanzamiento = new LaunchOptions
            {
                Headless = true,
                // Si encontramos un Chromium (bundled o instalado), usamos ESE ejecutable
                // en vez de que PuppeteerSharp busque el suyo propio descargado.
                ExecutablePath = _executablePath,
                // Sandbox de Chrome puede fallar en algunos entornos Windows corporativos/VM;
                // no representa un riesgo acá porque el HTML es 100% generado localmente
                // (no se navega a contenido externo ni se ejecuta JS de terceros).
                Args = new[] { "--no-sandbox" }
            };

            using (var browser = await Puppeteer.LaunchAsync(opcionesLanzamiento))
            using (var page = await browser.NewPageAsync())
            {
                // file:// + ruta absoluta para que cargue el HTML desde disco tal cual quedó guardado.
                string urlArchivo = new Uri(rutaHtml).AbsoluteUri;
                await page.GoToAsync(urlArchivo, WaitUntilNavigation.Networkidle0);

                await page.PdfAsync(rutaPdf, new PdfOptions
                {
                    Format = PaperFormat.A4,
                    PrintBackground = true, // conserva colores de fondo (badge, líneas, etc.)
                    MarginOptions = new MarginOptions
                    {
                        Top = "0",
                        Bottom = "0",
                        Left = "0",
                        Right = "0"
                    }
                });
            }

            return rutaPdf;
        }
    }
}
