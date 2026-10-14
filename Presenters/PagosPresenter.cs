using GestionEducativa.Models;
using GestionEducativa.Services;
using GestionEducativa.Utils;
using GestionEducativa.Views;
using GestionEducativa.Views.Interfaces;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionEducativa.Presenters
{
    public class PagosPresenter
    {
        private readonly IPagosView _view;
        private readonly PagoService _pagoService;
        private readonly AlumnoService _alumnoService;
        private readonly ComprobanteService _compService;
        private readonly PdfGenerator _pdfGen;
        private readonly PadreService _padreService;
        private readonly EmailService _emailService;

        private Pago _ultimoPago;

        public PagosPresenter(IPagosView view)
        {
            _view = view;
            _pagoService = new PagoService();
            _alumnoService = new AlumnoService();
            _compService = new ComprobanteService();
            _pdfGen = new PdfGenerator();
            _padreService = new PadreService();
            _emailService = new EmailService();

            _view.RegistrarPagoClicked += async (s, e) => await RegistrarPago();
            _view.GenerarCuotaClicked += (s, e) => GenerarCuota();
            _view.ImprimirComprobanteClicked += async (s, e) => await ImprimirUltimoComprobante();
        }

        private async Task RegistrarPago()
        {
            var cuotasSeleccionadas = _view.CuotasParaPagar;

            if (cuotasSeleccionadas == null || cuotasSeleccionadas.Count == 0)
            {
                _view.MostrarMensaje("Seleccione al menos una cuota para cobrar (marque con ✓).", true);
                return;
            }

            decimal montoRecibido = _view.MontoRecibido;
            if (montoRecibido <= 0)
            {
                _view.MostrarMensaje("Ingrese un monto recibido válido.", true);
                return;
            }

            // ── Validar documento del pagador ─────────────────────────────────
            string doc = _view.DocumentoPagador.Replace(".", "").Replace("-", "").Trim();
            string tipoDoc = _view.TipoDocumento;
            string pagador = _view.NombrePagador?.Trim();

            if (string.IsNullOrEmpty(pagador))
            {
                _view.MostrarMensaje("Ingresá el nombre del pagador antes de continuar.", true);
                return;
            }

            if (string.IsNullOrEmpty(doc))
            {
                _view.MostrarMensaje("Ingresá el DNI o CUIT del pagador antes de continuar.", true);
                return;
            }

            if (!doc.All(char.IsDigit))
            {
                _view.MostrarMensaje("El documento solo debe contener números.", true);
                return;
            }

            if (tipoDoc == "DNI" && (doc.Length < 7 || doc.Length > 8))
            {
                _view.MostrarMensaje("El DNI debe tener 7 u 8 dígitos.", true);
                return;
            }

            if (tipoDoc == "CUIT" && doc.Length != 11)
            {
                _view.MostrarMensaje("El CUIT/CUIL debe tener 11 dígitos.", true);
                return;
            }

            // ── Validar forma de pago ──────────────────────────────────────────
            // El combo es DropDownList sin ítem por defecto: si el cajero no lo toca,
            // SelectedItem queda null y antes se registraba igual, cayendo silenciosamente
            // en "Efectivo" (el flujo viejo de un solo paso ya no existe). Ahora se exige elegirlo.
            if (string.IsNullOrEmpty(_view.FormaPago))
            {
                _view.MostrarMensaje("Seleccioná la forma de pago (Efectivo, Transferencia o Cheque).", true);
                return;
            }


            decimal totalDeuda = cuotasSeleccionadas.Sum(c => c.Saldo);

            // Construir resumen para confirmación
            // IMPORTANTE: cuando el monto recibido no alcanza a cubrir todo lo adeudado,
            // el resumen debe mostrar lo que efectivamente se está aplicando a CADA cuota
            // en este cobro (puede ser solo una parte del saldo de la cuota), no el valor
            // total/original de la cuota. Simulamos acá la misma distribución secuencial
            // del monto recibido que se aplica al confirmar el pago (SimularPagoMultiCuota
            // + ConfirmarPagoConCae; aplica cuota por cuota, en el orden seleccionado,
            // hasta agotar el monto recibido).

            var sb = new StringBuilder();
            sb.AppendLine("═══ RESUMEN DEL COBRO ═══\n");

            decimal saldoDisponibleSimulado = montoRecibido;
            var porAlumno = cuotasSeleccionadas.GroupBy(c => c.AlumnoNombre ?? "Alumno");
            foreach (var grupo in porAlumno)
            {
                sb.AppendLine($"  👤 {grupo.Key}:");
                foreach (var c in grupo)
                {
                    decimal saldoConRec = CuotaService.CalcularSaldoConRecargo(c);
                    decimal recargo = Math.Round(saldoConRec - c.Saldo, 2);
                    decimal saldoCuota = c.Saldo;



                    decimal montoAplicar = Math.Max(0, Math.Min(saldoDisponibleSimulado, saldoCuota));
                    saldoDisponibleSimulado -= montoAplicar;
                    bool esParcial = montoAplicar < saldoCuota;

                    string etiquetaParcial = esParcial
                        ? (montoAplicar <= 0 ? "  (sin cubrir esta vez)" : "  (pago parcial)")
                        : "";

                    var descuento = c.MontoOriginal * c.Descuento / 100;

                    if (recargo > 0)
                        sb.AppendLine($"     • {c.PeriodoDescripcion} — Cuota : $ {c.MontoOriginal:N2} - Descuento {descuento:N2}  +  Recargo: $ {c.Recargo:N2} = ${c.MontoFinal} →  Paga ahora: $ {montoAplicar:N2}{etiquetaParcial}");
                    else
                        sb.AppendLine($"     • {c.PeriodoDescripcion} — Cuota: $ {c.MontoOriginal:N2} - Descuento {descuento:N2}   →  Paga ahora: $ {montoAplicar:N2}{etiquetaParcial}");
                }
            }


            sb.AppendLine($"\n  Total adeudado seleccionado:  $ {totalDeuda:N2}");
            sb.AppendLine($"  Monto recibido:               $ {montoRecibido:N2}");

            decimal vuelto = montoRecibido > totalDeuda ? montoRecibido - totalDeuda : 0;
            decimal deudaRest = montoRecibido < totalDeuda ? totalDeuda - montoRecibido : 0;

            if (vuelto > 0)
                sb.AppendLine($"  ↩ Vuelto a entregar:          $ {vuelto:N2}");
            if (deudaRest > 0)
                sb.AppendLine($"  ⚠ Queda pendiente:            $ {deudaRest:N2}");

            sb.AppendLine("\n¿Confirmar el cobro?");

            if (!_view.ConfirmarAccion(sb.ToString())) return;

            try
            {
                // ── Paso 1: simular el pago (NO toca la base de datos todavía) ──────
                var pagoSimulado = _pagoService.SimularPagoMultiCuota(
                    montoRecibido,
                    cuotasSeleccionadas,
                    _view.NombrePagador,
                    _view.DocumentoPagador,
                    _view.TipoDocumento,
                    _view.FormaPago,
                    _view.Observaciones
                );

                // ── Paso 2: pedir el CAE a ARCA usando el pago simulado ─────────────
                // Importante: esto se hace ANTES de registrar nada en la base. Si no hay
                // conexión a internet, ARCA rechaza el comprobante, o se cierra el diálogo
                // sin aprobar, el pago NO se registra: el saldo de las cuotas queda
                // exactamente igual que antes de intentar el cobro.
                var arcaForm = new ArcaEmisionForm(pagoSimulado);
                arcaForm.ShowDialog();

                if (arcaForm.Resultado?.Aprobado != true)
                {
                    _view.MostrarMensaje(
                        "✗ No se emitió el comprobante electrónico (sin conexión a ARCA o " +
                        "rechazado), por lo tanto el cobro NO fue registrado y el saldo de " +
                        "las cuotas no cambió. Volvé a intentarlo cuando haya conexión.", true);
                    return;
                }

                // ── Paso 3: recién acá se confirma el pago en la base de datos ──────
                // (junto con el CAE ya obtenido, en una única transacción atómica).
                pagoSimulado.CAE = arcaForm.Resultado.CAE;
                pagoSimulado.CAEVencimiento = arcaForm.Resultado.CAEVencimiento;
                pagoSimulado.NroComprobanteArca = arcaForm.Resultado.NroComprobante;

                _ultimoPago = _pagoService.ConfirmarPagoConCae(pagoSimulado);

                try { _pagoService.GuardarComprobante(_ultimoPago); }
                catch (Exception ex)
                {
                    _view.MostrarMensaje("Error al guardar el comprobante: " + ex.Message, true);
                }

                string msgExito = $"✓ Cobro registrado correctamente.\n" +
                                  $"Cuotas cubiertas: {_ultimoPago.Detalles.Count}\n" +
                                  $"Total aplicado: $ {_ultimoPago.TotalAplicado:N2}";

                if (_ultimoPago.Vuelto > 0)
                    msgExito += $"\nVuelto: $ {_ultimoPago.Vuelto:N2}";

                bool enviarPorMail = _view.EnviarPorMail;

                _view.MostrarMensaje(msgExito);
                _view.GuardarOVincularPadreSiCorresponde();
                _view.LimpiarFormulario();

                // Genera el comprobante EN HTML (el formato que se guarda para siempre) y lo
                // abre para el cajero. Guardamos la ruta para reutilizarla en el mail: si el
                // cajero también manda por mail, ahí se convierte a PDF solo para ese envío.
                string archivoComprobante = await ImprimirUltimoComprobante();

                // Solo se envía por mail si el cajero dejó tildado el checkbox de este cobro.
                // Si llegamos a este punto es porque ARCA ya aprobó y el pago quedó realmente
                // emitido y confirmado en la BD — el mail es lo único que se puede omitir.
                if (enviarPorMail)
                {
                    // Se bloquea toda la interfaz mientras se convierte a PDF y se envía el
                    // mail, para que el cajero no pueda seguir operando el sistema (cambiar
                    // de pestaña, cobrar otro alumno, etc.) hasta que termine ese envío.
                    BloqueoUI.Mostrar();
                    try { await EnviarComprobantePorMailSiCorresponde(archivoComprobante); }
                    finally { BloqueoUI.Ocultar(); }
                }
            }
            catch (Exception ex)
            {
                _view.MostrarMensaje($"Error al registrar el cobro: {ex.Message}", true);
            }
        }

        private void GenerarCuota()
        {
            // Delegar completamente a PagosForm que ya tiene el panel con los datos
            // El panel llama a PagoService.GenerarCuotasMasivas directamente
            // y muestra el resultado. Este método queda como punto de entrada del evento.
            (_view as GestionEducativa.Views.PagosForm)?.EjecutarGenerarCuotasMasivas();
        }

        /// <summary>
        /// Convierte un comprobante YA GENERADO (HTML persistente) a PDF, únicamente para
        /// adjuntarlo al mail — los padres no sabían abrir el .html directamente. Este PDF
        /// es descartable: se borra apenas se manda el mail. El archivo que queda guardado
        /// para siempre en el sistema es el HTML (liviano, ~30KB contra ~200KB del PDF),
        /// que es al que apunta "Reimprimir comprobante" en Informes.
        /// Si la conversión falla, se manda directamente el HTML como adjunto — no es ideal
        /// para el padre, pero es mejor que no mandar nada.
        /// </summary>
        private async Task<(string archivoParaAdjuntar, string archivoPdfTemporal)> PrepararAdjuntoParaMail(string archivoHtml)
        {
            try
            {
                string archivoPdf = await GestionEducativa.Utils.HtmlToPdfConverter.ConvertirArchivoAsync(archivoHtml);
                return (archivoPdf, archivoPdf); // el segundo valor marca que hay que borrarlo después de enviar
            }
            catch (Exception exPdf)
            {
                _view.MostrarMensaje(
                    $"⚠ No se pudo convertir el comprobante a PDF para el mail, se enviará en HTML. Motivo: {exPdf.GetType().Name}: {exPdf.Message}",
                    true);
                return (archivoHtml, null); // se manda el HTML tal cual, nada que borrar después
            }
        }

        /// <summary>
        /// Busca al padre/tutor que figura como pagador (por su documento) y, si tiene un
        /// email cargado en la BD, le envía por mail una copia del comprobante recién emitido.
        /// No es un paso crítico del cobro: si falla, se informa pero no interrumpe el flujo
        /// (el comprobante ya quedó emitido y guardado igual).
        /// </summary>
        /// <param name="archivoHtmlYaGenerado">
        /// Ruta del HTML ya generado en este mismo cobro (por ejemplo al imprimirlo justo
        /// antes), para no volver a generarlo. Si se llama de forma independiente, se genera acá.
        /// </param>
        private async Task EnviarComprobantePorMailSiCorresponde(string archivoHtmlYaGenerado = null)
        {
            if (_ultimoPago == null) return;

            try
            {
                string doc = (_ultimoPago.DocumentoPagador ?? "").Replace(".", "").Replace("-", "").Trim();
                if (string.IsNullOrEmpty(doc)) return;

                var padre = _padreService.ObtenerPorDocumento(doc);
                if (padre == null || string.IsNullOrWhiteSpace(padre.Email))
                {
                    // No es un error: simplemente no hay un padre registrado con ese documento,
                    // o no tiene mail cargado en la BD. El comprobante ya se emitió normalmente.
                    return;
                }

                string archivoHtml = archivoHtmlYaGenerado ?? _pdfGen.GenerarComprobantePagoMultiple(_ultimoPago);
                var (archivoParaAdjuntar, archivoPdfTemporal) = await PrepararAdjuntoParaMail(archivoHtml);

                bool enviado = _emailService.EnviarComprobante(_ultimoPago, padre, archivoParaAdjuntar, out string error);

                // El PDF era descartable, solo para este envío — el HTML queda guardado igual.
                if (archivoPdfTemporal != null)
                {
                    try { File.Delete(archivoPdfTemporal); }
                    catch { /* no crítico */ }
                }

                if (enviado)
                    _view.MostrarMensaje($"✓ Comprobante enviado por mail a {padre.NombreCompleto} ({padre.Email}).");
                else
                    _view.MostrarMensaje($"⚠ El comprobante se emitió correctamente, pero no se pudo enviar por mail: {error}", true);
            }
            catch (Exception ex)
            {
                _view.MostrarMensaje($"⚠ El comprobante se emitió correctamente, pero no se pudo enviar por mail: {ex.Message}", true);
            }
        }

        /// <summary>
        /// Genera el último comprobante en HTML —formato que se guarda para siempre en el
        /// sistema— y lo abre con el navegador para el cajero (se ve y se imprime bien
        /// igual que un PDF). Devuelve la ruta para que RegistrarPago la reutilice al
        /// mandar el mail, evitando generar el comprobante dos veces.
        /// </summary>
        private async Task<string> ImprimirUltimoComprobante()
        {
            if (_ultimoPago == null)
            {
                _view.MostrarMensaje("No hay comprobante disponible para imprimir.", true);
                return null;
            }

            try
            {
                string archivo = _pdfGen.GenerarComprobantePagoMultiple(_ultimoPago);
                Process.Start(new ProcessStartInfo(archivo) { UseShellExecute = true });
                _view.MostrarMensaje($"✓ Comprobante generado: {archivo}");
                return await Task.FromResult(archivo);
            }
            catch (Exception ex)
            {
                _view.MostrarMensaje($"Error al generar el comprobante: {ex.Message}", true);
                return null;
            }
        }
    }
}
