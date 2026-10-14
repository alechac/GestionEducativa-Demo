using GestionEducativa.Models;
using GestionEducativa.Services;
using GestionEducativa.Views.Interfaces;
using System;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace GestionEducativa.Presenters
{
    public class InformesPresenter
    {
        private readonly IInformesView _view;
        private readonly InformesService _svc;
        private DataTable _ultimosDatos;

        public InformesPresenter(IInformesView view)
        {
            _view = view;
            _svc = new InformesService();

            _view.GenerarInformeClicked += (s, e) => Generar();
            _view.ExportarExcelClicked += (s, e) => ExportarCsv();
            _view.ImprimirClicked += (s, e) => ImprimirHtml();
            _view.ReimprimirComprobanteClicked += (s, e) => ReimprimirComprobante();
        }

        private void Generar()
        {
            try
            {
                DataTable dt = null;
                string tipo = _view.TipoInforme;

                switch (tipo)
                {
                    case "IngresosFecha":
                        dt = _svc.IngresosPorFecha(_view.FechaDesde, _view.FechaHasta);
                        break;
                    case "IngresosMes":
                        dt = _svc.IngresosPorMes(_view.AnioFiltro, _view.NivelFiltroTexto);
                        break;
                    case "IngresosNivel":
                        dt = _svc.IngresosPorNivel(_view.AnioFiltro,
                            _view.MesFiltro > 0 ? _view.MesFiltro : (int?)null);
                        break;

                    case "ProyeccionMensual":
                        dt = _svc.ProyeccionMensual(_view.AnioFiltro,
                            _view.MesFiltro > 0 ? _view.MesFiltro : (int?)null,
                            _view.NivelFiltroTexto);
                        break;

                    case "PagosAlumno":
                        if (_view.AlumnoId <= 0) { _view.MostrarMensaje("Seleccion· un alumno.", true); return; }
                        dt = _svc.PagosAlumno(_view.AlumnoId);
                        break;
                    case "PagosPadre":
                        dt = _svc.PagosPadre(_view.NombrePadre, _view.DocumentoPadre);
                        break;
                    case "Matriculados":
                        dt = _svc.Matriculados(_view.AnioFiltro, _view.NivelEducativoFiltro,
                            _view.NivelFiltroTexto, _view.DivisionFiltro);
                        break;
                    case "EstadoPagoMes":
                        if (_view.MesFiltro <= 0) { _view.MostrarMensaje("Seleccion√° un mes.", true); return; }
                        dt = _svc.EstadoPagoPorMes(_view.AnioFiltro, _view.MesFiltro,
                            _view.NivelFiltroTexto, _view.DivisionFiltro);
                        break;
                    case "AlumnosSinPagos":
                        if (_view.MesFiltro <= 0) { _view.MostrarMensaje("Seleccion√° desde que mes contar la deuda.", true); return; }
                        dt = _svc.AlumnosSinPagos(_view.AnioFiltro, _view.MesFiltro,
                            _view.NivelFiltroTexto, _view.DivisionFiltro);
                        break;
                    case "ListadoAlumnos":
                        dt = _svc.ListadoAlumnos(
                            _view.NivelFiltroTexto, _view.DivisionFiltro,
                            _view.ColNombre, _view.ColNivel, _view.ColDivision,
                            _view.ColDni, _view.ColDescuento, _view.ColSaldo,
                            _view.ColPadre, _view.ColTelefono, _view.ColDniPadre, _view.ColMailPadre);
                        break;
                    case "CierreCaja":
                        dt = _svc.CierreCaja(_view.FechaDesde);
                        var (rec, cob, vto, cant, gastos, neto) = _svc.ResumenCaja(_view.FechaDesde);
                        _view.MostrarResumenCaja(cant, cob, rec, vto, gastos, neto);
                        break;
                }

                if (dt == null || dt.Rows.Count == 0)
                {
                    _view.MostrarMensaje("No hay datos para los filtros seleccionados.");
                    return;
                }

                _ultimosDatos = dt;
                _view.MostrarDatos(dt);
                _view.MostrarMensaje($"? {dt.Rows.Count} registros encontrados.");
            }
            catch (Exception ex)
            {
                _view.MostrarMensaje("Error: " + ex.Message, true);
            }
        }

        private void ExportarCsv()
        {
            if (_ultimosDatos == null || _ultimosDatos.Rows.Count == 0)
            {
                _view.MostrarMensaje("Primero gener· el informe.", true); return;
            }
            using (var dlg = new SaveFileDialog
            {
                Filter = "CSV (*.csv)|*.csv",
                FileName = $"Informe_{_view.TipoInforme}_{DateTime.Now:yyyyMMdd}.csv"
            })
            {
                if (dlg.ShowDialog() != DialogResult.OK) return;
                var sb = new StringBuilder();
                // Encabezado
                foreach (DataColumn col in _ultimosDatos.Columns)
                    sb.Append($"\"{col.ColumnName}\";");
                sb.AppendLine();
                // Filas
                foreach (DataRow row in _ultimosDatos.Rows)
                {
                    foreach (var cell in row.ItemArray)
                        sb.Append($"\"{cell}\";");
                    sb.AppendLine();
                }
                File.WriteAllText(dlg.FileName, sb.ToString(), Encoding.UTF8);
                _view.MostrarMensaje($"? Exportado: {dlg.FileName}");
            }
        }

        private void ImprimirHtml()
        {
            if (_ultimosDatos == null || _ultimosDatos.Rows.Count == 0)
            {
                _view.MostrarMensaje("Primero gener· el informe.", true); return;
            }

            string carpeta = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                "GestionEducativa", "Informes");
            Directory.CreateDirectory(carpeta);
            string archivo = Path.Combine(carpeta, $"Informe_{DateTime.Now:yyyyMMdd_HHmmss}.html");

            var sb = new StringBuilder();
            sb.AppendLine("<!DOCTYPE html><html><head><meta charset='UTF-8'>");
            sb.AppendLine("<style>body{font-family:Arial,sans-serif;font-size:12px;}");
            sb.AppendLine("h2{color:#1a5276;}");
            sb.AppendLine("table{border-collapse:collapse;width:100%;}");
            sb.AppendLine("th{background:#1a5276;color:#fff;padding:6px 10px;text-align:left;}");
            sb.AppendLine("td{padding:5px 10px;border-bottom:1px solid #ddd;}");
            sb.AppendLine("tr:nth-child(even){background:#f5f5f5;}");
            sb.AppendLine(".btn{display:block;margin:12px auto;padding:7px 28px;background:#1a5276;");
            sb.AppendLine("color:#fff;border:none;border-radius:4px;cursor:pointer;font-size:13px;}");
            sb.AppendLine("</style></head><body>");
            sb.AppendLine($"<button class='btn' onclick='window.print()'>?? Imprimir</button>");
            sb.AppendLine($"<h2>{_view.TipoInforme} ó {DateTime.Now:dd/MM/yyyy HH:mm}</h2>");
            sb.AppendLine("<table><thead><tr>");
            foreach (DataColumn col in _ultimosDatos.Columns)
                sb.AppendLine($"<th>{col.ColumnName}</th>");
            sb.AppendLine("</tr></thead><tbody>");
            foreach (DataRow row in _ultimosDatos.Rows)
            {
                sb.AppendLine("<tr>");
                foreach (var cell in row.ItemArray)
                    sb.AppendLine($"<td>{cell}</td>");
                sb.AppendLine("</tr>");
            }
            sb.AppendLine("</tbody></table></body></html>");
            File.WriteAllText(archivo, sb.ToString(), Encoding.UTF8);
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(archivo)
            { UseShellExecute = true });
        }

        private void ReimprimirComprobante()
        {
            try
            {
                string nroComprobante = _view.NroComprobanteSeleccionado;
                if (string.IsNullOrWhiteSpace(nroComprobante))
                {
                    _view.MostrarMensaje("Seleccion√° una fila con n√∫mero de comprobante.", true);
                    return;
                }

                string nroArchivo = nroComprobante.Replace("-", "_");
                string carpeta = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                    "GestionEducativa", "Comprobantes");

                

                string archivo = Path.Combine(carpeta, $"Comprobante_FC_{nroArchivo}.html");
                if (!File.Exists(archivo))
                    archivo = Path.Combine(carpeta, $"Comprobante_NC_{nroArchivo}.html");
                if( !File.Exists(archivo))
                    archivo = Path.Combine(carpeta, $"Comprobante_{nroArchivo}.html");
                if(!File.Exists(archivo))
                    archivo = Path.Combine(carpeta, $"Comprobante_{nroArchivo}.pdf");
                if ( !File.Exists(archivo))
                    archivo = Path.Combine( carpeta, $"Comprobante_FC_{nroArchivo}.pdf");
                if (!File.Exists(archivo))
                    archivo = Path.Combine(carpeta, $"Comprobante_NC_{nroArchivo}.pdf");


                if (!File.Exists(archivo))
                {
                    _view.MostrarMensaje($"No se encontr√≥ el archivo del comprobante para el N¬∞ {nroComprobante}.", true);
                    return;
                }

                System.Diagnostics.Process.Start(
                    new System.Diagnostics.ProcessStartInfo(archivo) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                _view.MostrarMensaje("Error al reimprimir: " + ex.Message, true);
            }
        }
    }


}
