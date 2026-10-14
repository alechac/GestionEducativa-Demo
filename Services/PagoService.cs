using GestionEducativa.Constants;
using GestionEducativa.Database;
using GestionEducativa.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GestionEducativa.Services
{
    public class PagoService
    {
        private readonly ConexionBD conexionBD;

        public PagoService()
        {
            conexionBD = new ConexionBD();
        }

        // ==================== PAGOS ====================

        // ══════════════════════════════════════════════════════════════════════════
        //  Cálculo puro de la distribución del pago entre cuotas — NO toca la base
        //  de datos. Se usa tanto para simular el pago (antes de pedir el CAE) como
        //  para confirmarlo definitivamente, así el detalle que se le mandó a ARCA es
        //  exactamente el mismo que se termina guardando.
        // ══════════════════════════════════════════════════════════════════════════
        private List<DetallePago> CalcularDetallesPago(decimal montoRecibido, List<Cuota> cuotasSeleccionadas,
            out decimal totalAplicado, out decimal vuelto)
        {
            decimal saldoDisponible = montoRecibido;
            var detalles = new List<DetallePago>();

            foreach (var cuota in cuotasSeleccionadas)
            {
                if (saldoDisponible <= 0 && cuota.Saldo > 0) break;

                decimal saldoCuota = cuota.Saldo;
                decimal montoAplicar = Math.Min(saldoDisponible, saldoCuota);
                decimal saldoRestante = saldoCuota - montoAplicar;

                string nuevoEstado = saldoRestante <= 0 ? EstadosCuota.Pagado
                    : montoAplicar > 0 ? EstadosCuota.PagoParcial
                    : cuota.Estado;

                // Precio unitario y bonificación para el comprobante:
                // — Pago total DE UNA CUOTA SIN PAGOS PREVIOS: precio = montoOriginal, bonif solo para Curricular/Extracurricular
                // — Pago parcial, O pago que cancela el saldo de una cuota que ya tenía
                //   pagos previos (ej: matrícula abonada en dos veces): precio = lo que se
                //   pagó HOY (montoAplicar), sin bonificación (para no repetir el original completo
                //   ni la bonificación ya reflejada en el/los comprobantes anteriores).

                bool pagoParcial = saldoRestante > 0;
                bool teniaPagoPrevio = cuota.MontoPagado > 0;
                bool esCuotaConDescuento = !pagoParcial && !teniaPagoPrevio &&
                    cuota.Descuento > 0 &&
                    (string.Equals(cuota.TipoCuota, "Curricular", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(cuota.TipoCuota, "Extracurricular", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(cuota.TipoCuota, "Programatica", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(cuota.TipoCuota, "Programática", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(cuota.TipoCuota, "Extraprogramatica", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(cuota.TipoCuota, "Extraprogramática", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(cuota.TipoCuota, "Diferencia", StringComparison.OrdinalIgnoreCase) ||
                     string.Equals(cuota.TipoCuota, "Cuota Mensual", StringComparison.OrdinalIgnoreCase));
                decimal precioUnitario = (pagoParcial || teniaPagoPrevio)
                    ? montoAplicar
                    : (cuota.MontoOriginal > 0 ? cuota.MontoOriginal : saldoCuota);
                decimal pctBonifDetalle = esCuotaConDescuento ? cuota.Descuento : 0m;
                decimal impBonifDetalle = esCuotaConDescuento && cuota.MontoOriginal > 0
                    ? Math.Round(cuota.MontoOriginal * cuota.Descuento / 100m, 2)
                    : 0m;

                detalles.Add(new DetallePago
                {
                    CuotaId = cuota.Id,
                    AlumnoId = cuota.AlumnoId,
                    AlumnoNombre = cuota.AlumnoNombre,
                    PeriodoCuota = cuota.PeriodoDescripcion,
                    TipoCuota = cuota.TipoCuota,
                    NombreConcepto = cuota.NombreConcepto,
                    SaldoAnterior = saldoCuota,
                    MontoAplicado = montoAplicar,
                    SaldoRestante = saldoRestante,
                    PrecioUnitario = precioUnitario,
                    PorcentajeBonif = pctBonifDetalle,
                    ImporteBonif = impBonifDetalle,
                    Subtotal = precioUnitario - impBonifDetalle,
                    RecargoAplicado = 0m,
                    EstadoCuotaNuevo = nuevoEstado
                });

                saldoDisponible -= montoAplicar;
            }

            totalAplicado = montoRecibido - saldoDisponible;
            vuelto = saldoDisponible;
            return detalles;
        }
        /// <summary>
        /// Calcula cómo se distribuiría el pago entre las cuotas seleccionadas, PERO NO
        /// escribe nada en la base de datos: ni actualiza el saldo de las cuotas ni inserta
        /// el pago. Sirve para armar el comprobante que se le manda a ARCA a pedir el CAE
        /// ANTES de dar el cobro por registrado. El <see cref="Pago"/> que devuelve tiene
        /// Id = 0 hasta que se confirme con <see cref="ConfirmarPagoConCae"/>.
        /// </summary>
        public Pago SimularPagoMultiCuota(decimal montoRecibido, List<Cuota> cuotasSeleccionadas,
            string nombrePagador, string documentoPagador, string tipoPagador, string formaPago, string observaciones)
        {
            if (cuotasSeleccionadas == null || cuotasSeleccionadas.Count == 0)
                throw new ArgumentException("Debe haber al menos una cuota para registrar el pago.");

            var detalles = CalcularDetallesPago(montoRecibido, cuotasSeleccionadas,
                out decimal totalAplicado, out decimal vuelto);

            return new Pago
            {
                Id = 0, // todavía no existe en la base — recién se crea en ConfirmarPagoConCae
                MontoRecibido = montoRecibido,
                TotalAplicado = totalAplicado,
                Vuelto = vuelto,
                FechaPago = DateTime.Now,
                NombrePagador = nombrePagador,
                DocumentoPagador = documentoPagador,
                TipoPagador = tipoPagador,
                Observaciones = observaciones,
                Detalles = detalles,
                FormaPago = formaPago ?? "Efectivo",
                PeriodoDesde = detalles.Count > 0
                    ? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1)
                    : DateTime.Now,
                PeriodoHasta = DateTime.Now.Date
            };
        }
        /// <summary>
        /// Persiste definitivamente en la base de datos un pago que ya fue simulado con
        /// <see cref="SimularPagoMultiCuota"/>. Recién ACÁ se actualiza el saldo/estado de
        /// las cuotas y se inserta el pago — en una única transacción atómica. Si el pago
        /// trae CAE cargado (pagoSimulado.CAE), se guarda junto con el resto en el mismo
        /// INSERT, marcando estadoArca = 'Emitido'; si no trae CAE, queda en 'Pendiente'
        /// (uso interno/histórico, no debería ocurrir en el flujo normal de cobro).
        /// </summary>
        public Pago ConfirmarPagoConCae(Pago pagoSimulado)
        {
            if (pagoSimulado == null) throw new ArgumentNullException(nameof(pagoSimulado));
            if (pagoSimulado.Detalles == null || pagoSimulado.Detalles.Count == 0)
                throw new ArgumentException("El pago no tiene detalles para registrar.");

            using (MySqlConnection conexion = conexionBD.ObtenerConexion())
            {
                conexion.Open();
                using (MySqlTransaction transaccion = conexion.BeginTransaction())
                {
                    try
                    {
                        // Una Nota de Crédito (TipoComprobante = 13) hace exactamente lo contrario que
                        // una factura sobre la cuota: en vez de sumar al monto pagado, lo resta (la
                        // cuota vuelve a deber lo que se está acreditando). El estado final de cada
                        // cuota ya viene calculado desde SimularPagoMultiCuota / SimularNotaCredito
                        // (detalle.EstadoCuotaNuevo), así que acá solo cambia el signo de la operación.
                        bool esNotaCredito = pagoSimulado.TipoComprobante == 13;

                        foreach (var detalle in pagoSimulado.Detalles)
                        {
                            string sqlCuota = esNotaCredito
                               ? @"UPDATE cuotas SET
                                        montoPagado = montoPagado - @monto,

                                        estado = @estado
                                    WHERE id = @cuotaId"
                               : @"UPDATE cuotas SET
                                        montoPagado = montoPagado + @monto,

                                        estado = @estado
                                    WHERE id = @cuotaId";

                            using (MySqlCommand cmd = new MySqlCommand(sqlCuota, conexion, transaccion))
                            {
                                cmd.Parameters.AddWithValue("@monto", detalle.MontoAplicado);

                                cmd.Parameters.AddWithValue("@estado", detalle.EstadoCuotaNuevo ?? EstadosCuota.PagoParcial);
                                cmd.Parameters.AddWithValue("@cuotaId", detalle.CuotaId);
                                cmd.ExecuteNonQuery();
                            }
                        }

                        bool tieneCae = !string.IsNullOrEmpty(pagoSimulado.CAE);

                        string sqlPago = @"
                            INSERT INTO pagos 
                                (montoRecibido, totalAplicado, vuelto, fechaPago,
                                 nombrePagador, documentoPagador, tipoPagador, observaciones, formaPago,
                                 cae, caeVencimiento, nroComprobanteArca, estadoArca,
                                 tipoComprobante, pagoAsociadoId)
                            VALUES 
                                (@montoRecibido, @totalAplicado, @vuelto, @fechaPago,
                                 @nombrePagador, @documentoPagador, @tipoPagador, @observaciones, @formaPago,
                                 @cae, @caeVenc, @nroArca, @estadoArca,
                                 @tipoComprobante, @pagoAsociadoId);
                            SELECT LAST_INSERT_ID();";

                        int pagoId;
                        using (MySqlCommand cmd = new MySqlCommand(sqlPago, conexion, transaccion))
                        {
                            cmd.Parameters.AddWithValue("@montoRecibido", pagoSimulado.MontoRecibido);
                            cmd.Parameters.AddWithValue("@totalAplicado", pagoSimulado.TotalAplicado);
                            cmd.Parameters.AddWithValue("@vuelto", pagoSimulado.Vuelto);
                            cmd.Parameters.AddWithValue("@fechaPago", pagoSimulado.FechaPago);
                            cmd.Parameters.AddWithValue("@nombrePagador", (object)pagoSimulado.NombrePagador ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@documentoPagador", (object)pagoSimulado.DocumentoPagador ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@tipoPagador", (object)pagoSimulado.TipoPagador ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@formaPago", pagoSimulado.FormaPago ?? "Efectivo");
                            cmd.Parameters.AddWithValue("@observaciones", (object)pagoSimulado.Observaciones ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@cae", tieneCae ? (object)pagoSimulado.CAE : DBNull.Value);
                            cmd.Parameters.AddWithValue("@caeVenc", tieneCae && pagoSimulado.CAEVencimiento.HasValue ? (object)pagoSimulado.CAEVencimiento.Value.Date : DBNull.Value);
                            cmd.Parameters.AddWithValue("@nroArca", tieneCae ? (object)pagoSimulado.NroComprobanteArca : DBNull.Value);
                            cmd.Parameters.AddWithValue("@estadoArca", tieneCae ? "Emitido" : "Pendiente");
                            cmd.Parameters.AddWithValue("@tipoComprobante", pagoSimulado.TipoComprobante > 0 ? pagoSimulado.TipoComprobante : 11);
                            cmd.Parameters.AddWithValue("@pagoAsociadoId", (object)pagoSimulado.PagoAsociadoId ?? DBNull.Value);
                            pagoId = Convert.ToInt32(cmd.ExecuteScalar());
                        }

                        foreach (var detalle in pagoSimulado.Detalles)
                        {
                            string sqlDetalle = @"
                                INSERT INTO pagos_detalle 
                                    (pagoId, cuotaId, alumnoId, periodoDescripcion, tipoCuota, nombreConcepto,
                                     saldoAnterior, montoAplicado, saldoRestante)
                                VALUES 
                                    (@pagoId, @cuotaId, @alumnoId, @periodo, @tipoCuota, @nombreConcepto,
                                     @saldoAnterior, @montoAplicado, @saldoRestante)";

                            using (MySqlCommand cmd = new MySqlCommand(sqlDetalle, conexion, transaccion))
                            {
                                cmd.Parameters.AddWithValue("@pagoId", pagoId);
                                cmd.Parameters.AddWithValue("@cuotaId", detalle.CuotaId);
                                cmd.Parameters.AddWithValue("@alumnoId", detalle.AlumnoId);
                                cmd.Parameters.AddWithValue("@periodo", detalle.PeriodoCuota);
                                cmd.Parameters.AddWithValue("@tipoCuota", (object)detalle.TipoCuota ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@nombreConcepto", (object)detalle.NombreConcepto ?? DBNull.Value);
                                cmd.Parameters.AddWithValue("@saldoAnterior", detalle.SaldoAnterior);
                                cmd.Parameters.AddWithValue("@montoAplicado", detalle.MontoAplicado);
                                cmd.Parameters.AddWithValue("@saldoRestante", detalle.SaldoRestante);
                                cmd.ExecuteNonQuery();
                            }

                            detalle.PagoId = pagoId;
                        }

                        transaccion.Commit();

                        pagoSimulado.Id = pagoId;

                        new LogService().Registrar(
                            esNotaCredito ? "Nota de crédito" : "Pago registrado",
                            $"Pago #{pagoId} — {pagoSimulado.NombrePagador} — Total: ${pagoSimulado.TotalAplicado:N2}" +
                            (tieneCae ? $" — CAE {pagoSimulado.CAE}" : ""));

                        return pagoSimulado;
                    }
                    catch
                    {
                        transaccion.Rollback();
                        throw;
                    }
                }
            }
        }
        // ══════════════════════════════════════════════════════════════════════════
        //  NOTAS DE CRÉDITO
        //  Siguen el mismo patrón de dos pasos que los pagos: primero se simula (sin
        //  tocar la BD), se pide el CAE a ARCA con esa simulación, y solo si lo aprueba
        //  se confirma con ConfirmarPagoConCae (comparte método con las facturas: ahí
        //  se detecta que TipoComprobante = 13 y se resta en vez de sumar).
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Busca facturas (Factura C, con CAE) para elegir cuál acreditar con una Nota de
        /// Crédito. El filtro busca por nombre/documento del pagador o por número de
        /// comprobante ARCA. Devuelve como máximo 50 resultados, más recientes primero.
        /// </summary>
        public List<Pago> BuscarFacturasParaAcreditar(string filtro)
        {
            var lista = new List<Pago>();
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = @"
                        SELECT p.id, p.fechaPago, p.nombrePagador, p.documentoPagador, p.tipoPagador,
                               p.totalAplicado, p.nroComprobanteArca, p.cae,
                               (SELECT COUNT(*) FROM pagos nc WHERE nc.pagoAsociadoId = p.id) AS cantidadNc
                        FROM pagos p
                        WHERE p.tipoComprobante = 11
                          AND p.cae IS NOT NULL
                          AND (@filtro = ''
                               OR p.nombrePagador LIKE CONCAT('%', @filtro, '%')
                               OR p.documentoPagador LIKE CONCAT('%', @filtro, '%')
                               OR p.nroComprobanteArca LIKE CONCAT('%', @filtro, '%'))
                        ORDER BY p.fechaPago DESC
                        LIMIT 50";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@filtro", filtro?.Trim() ?? "");
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                lista.Add(new Pago
                                {
                                    Id = Convert.ToInt32(r["id"]),
                                    FechaPago = Convert.ToDateTime(r["fechaPago"]),
                                    NombrePagador = r["nombrePagador"] == DBNull.Value ? null : r["nombrePagador"].ToString(),
                                    DocumentoPagador = r["documentoPagador"] == DBNull.Value ? null : r["documentoPagador"].ToString(),
                                    TipoPagador = r["tipoPagador"] == DBNull.Value ? null : r["tipoPagador"].ToString(),
                                    TotalAplicado = Convert.ToDecimal(r["totalAplicado"]),
                                    NroComprobanteArca = r["nroComprobanteArca"] == DBNull.Value ? null : r["nroComprobanteArca"].ToString(),
                                    CAE = r["cae"] == DBNull.Value ? null : r["cae"].ToString(),
                                    TipoComprobante = 11,
                                    CantidadNotasCredito = Convert.ToInt32(r["cantidadNc"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al buscar facturas: " + ex.Message);
            }
            return lista;
        }
        /// <summary>
        /// Calcula (sin tocar la BD) la Nota de Crédito que acreditaría los ítems
        /// seleccionados de una factura ya emitida. <paramref name="itemsACreditar"/> debe
        /// traer, por cada detalle de la factura original que se quiere acreditar, cuánto
        /// se acredita de ese ítem (hasta el monto que se pagó en ese ítem).
        /// </summary>
        public Pago SimularNotaCredito(Pago facturaOriginal, List<(DetallePago detalleOriginal, decimal montoACreditar)> itemsACreditar, string observaciones)
        {
            if (facturaOriginal == null) throw new ArgumentNullException(nameof(facturaOriginal));
            if (itemsACreditar == null || itemsACreditar.Count == 0)
                throw new ArgumentException("Elegí al menos un ítem para acreditar.");

            using (MySqlConnection conexion = conexionBD.ObtenerConexion())
            {
                conexion.Open();

                var detalles = new List<DetallePago>();
                decimal totalAcreditado = 0;

                foreach (var (detalleOriginal, montoACreditar) in itemsACreditar)
                {
                    if (montoACreditar <= 0) continue;

                    if (montoACreditar > detalleOriginal.MontoAplicado + 0.004m)
                        throw new ArgumentException(
                            $"No se puede acreditar $ {montoACreditar:N2} de '{detalleOriginal.PeriodoCuota}': " +
                            $"la factura original solo aplicó $ {detalleOriginal.MontoAplicado:N2} a ese ítem.");

                    // Leer el estado ACTUAL (en vivo) de la cuota — puede haber cambiado desde
                    // que se emitió la factura original (ej: se le aplicaron más pagos después).
                    string sqlLeer = "SELECT montoFinal, montoPagado FROM cuotas WHERE id = @id";
                    decimal montoFinal, montoPagadoActual;
                    using (MySqlCommand cmd = new MySqlCommand(sqlLeer, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id", detalleOriginal.CuotaId);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            if (!r.Read())
                                throw new Exception($"No se encontró la cuota de '{detalleOriginal.PeriodoCuota}' (id {detalleOriginal.CuotaId}).");
                            montoFinal = Convert.ToDecimal(r["montoFinal"]);
                            montoPagadoActual = Convert.ToDecimal(r["montoPagado"]);
                        }
                    }

                    if (montoACreditar > montoPagadoActual + 0.004m)
                        throw new ArgumentException(
                            $"No se puede acreditar $ {montoACreditar:N2} de '{detalleOriginal.PeriodoCuota}': " +
                            $"esa cuota hoy solo tiene $ {montoPagadoActual:N2} pagados (puede que ya se le haya " +
                            $"hecho otra Nota de Crédito, o se le haya ajustado el saldo manualmente).");

                    decimal saldoAntes = montoFinal - montoPagadoActual;
                    decimal montoPagadoNuevo = montoPagadoActual - montoACreditar;
                    decimal saldoDespues = montoFinal - montoPagadoNuevo;

                    string nuevoEstado = saldoDespues <= 0 ? EstadosCuota.Pagado
                        : montoPagadoNuevo > 0 ? EstadosCuota.PagoParcial
                        : EstadosCuota.Pendiente;

                    detalles.Add(new DetallePago
                    {
                        CuotaId = detalleOriginal.CuotaId,
                        AlumnoId = detalleOriginal.AlumnoId,
                        AlumnoNombre = detalleOriginal.AlumnoNombre,
                        PeriodoCuota = detalleOriginal.PeriodoCuota,
                        TipoCuota = detalleOriginal.TipoCuota,
                        NombreConcepto = detalleOriginal.NombreConcepto,
                        SaldoAnterior = saldoAntes,
                        MontoAplicado = montoACreditar,     // se guarda en positivo: el signo lo da TipoComprobante=13
                        SaldoRestante = saldoDespues,
                        PrecioUnitario = montoACreditar,
                        PorcentajeBonif = 0,
                        ImporteBonif = 0,
                        Subtotal = montoACreditar,
                        RecargoAplicado = 0,
                        EstadoCuotaNuevo = nuevoEstado
                    });

                    totalAcreditado += montoACreditar;
                }

                if (detalles.Count == 0)
                    throw new ArgumentException("No hay ningún monto válido para acreditar.");

                return new Pago
                {
                    Id = 0,
                    TipoComprobante = 13,
                    PagoAsociadoId = facturaOriginal.Id,
                    NroComprobanteAsociado = facturaOriginal.NroComprobanteArca,
                    NombrePagador = facturaOriginal.NombrePagador,
                    DocumentoPagador = facturaOriginal.DocumentoPagador,
                    TipoPagador = facturaOriginal.TipoPagador,
                    FormaPago = facturaOriginal.FormaPago,
                    MontoRecibido = totalAcreditado,
                    TotalAplicado = totalAcreditado,
                    Vuelto = 0,
                    FechaPago = DateTime.Now,
                    Observaciones = observaciones,
                    Detalles = detalles,
                    PeriodoDesde = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1),
                    PeriodoHasta = DateTime.Now.Date
                };
            }
        }
        public Pago ObtenerPagoCompleto(int pagoId)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();

                    // Cabecera del pago
                    string sqlPago = @"
                        SELECT p.id, p.montoRecibido, p.totalAplicado, p.vuelto, p.fechaPago,
                               p.nombrePagador, p.documentoPagador, p.tipoPagador, p.observaciones,
                               p.formaPago, p.cae, p.caeVencimiento, p.nroComprobanteArca,
                               p.tipoComprobante, p.pagoAsociadoId,
                               asoc.nroComprobanteArca AS nroComprobanteAsociado
                        FROM pagos p
                        LEFT JOIN pagos asoc ON asoc.id = p.pagoAsociadoId
                        WHERE p.id = @id";

                    Pago pago = null;
                    using (MySqlCommand cmd = new MySqlCommand(sqlPago, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id", pagoId);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            if (!r.Read()) throw new Exception("Pago no encontrado.");
                            pago = new Pago
                            {
                                Id = Convert.ToInt32(r["id"]),
                                MontoRecibido = Convert.ToDecimal(r["montoRecibido"]),
                                TotalAplicado = Convert.ToDecimal(r["totalAplicado"]),
                                Vuelto = Convert.ToDecimal(r["vuelto"]),
                                FechaPago = Convert.ToDateTime(r["fechaPago"]),
                                NombrePagador = r["nombrePagador"] == DBNull.Value ? null : r["nombrePagador"].ToString(),
                                DocumentoPagador = r["documentoPagador"] == DBNull.Value ? null : r["documentoPagador"].ToString(),
                                TipoPagador = r["tipoPagador"] == DBNull.Value ? null : r["tipoPagador"].ToString(),
                                Observaciones = r["observaciones"] == DBNull.Value ? null : r["observaciones"].ToString(),
                                FormaPago = r["formaPago"] == DBNull.Value ? null : r["formaPago"].ToString(),
                                CAE = r["cae"] == DBNull.Value ? null : r["cae"].ToString(),
                                NroComprobanteArca = r["nroComprobanteArca"] == DBNull.Value ? null : r["nroComprobanteArca"].ToString(),
                                TipoComprobante = r.HasColumn("tipoComprobante") && r["tipoComprobante"] != DBNull.Value
                                    ? Convert.ToInt32(r["tipoComprobante"]) : 11,
                                PagoAsociadoId = r.HasColumn("pagoAsociadoId") && r["pagoAsociadoId"] != DBNull.Value
                                    ? Convert.ToInt32(r["pagoAsociadoId"]) : (int?)null,
                                NroComprobanteAsociado = r.HasColumn("nroComprobanteAsociado") && r["nroComprobanteAsociado"] != DBNull.Value
                                    ? r["nroComprobanteAsociado"].ToString() : null,
                                PeriodoDesde = Convert.ToDateTime(r["fechaPago"]).Date,
                                PeriodoHasta = Convert.ToDateTime(r["fechaPago"]).Date,
                            };
                        }
                    }

                    // Detalles del pago
                    string sqlDet = @"
                        SELECT pd.id, pd.cuotaId, pd.alumnoId, pd.periodoDescripcion, pd.tipoCuota, pd.nombreConcepto,
                               pd.saldoAnterior, pd.montoAplicado, pd.saldoRestante,
                               a.nombre AS alumnoNombre, a.nivel AS alumnoNivel
                        FROM pagos_detalle pd
                        JOIN alumnos a ON pd.alumnoId = a.id
                        WHERE pd.pagoId = @pagoId
                        ORDER BY pd.id";

                    pago.Detalles = new List<DetallePago>();
                    using (MySqlCommand cmd = new MySqlCommand(sqlDet, conexion))
                    {
                        cmd.Parameters.AddWithValue("@pagoId", pagoId);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                pago.Detalles.Add(new DetallePago
                                {
                                    PagoId = pagoId,
                                    CuotaId = Convert.ToInt32(r["cuotaId"]),
                                    AlumnoId = Convert.ToInt32(r["alumnoId"]),
                                    PeriodoCuota = r["periodoDescripcion"].ToString(),
                                    TipoCuota = r["tipoCuota"] == DBNull.Value ? null : r["tipoCuota"].ToString(),
                                    NombreConcepto = r.HasColumn("nombreConcepto") && r["nombreConcepto"] != DBNull.Value
                                        ? r["nombreConcepto"].ToString() : null,
                                    SaldoAnterior = Convert.ToDecimal(r["saldoAnterior"]),
                                    MontoAplicado = Convert.ToDecimal(r["montoAplicado"]),
                                    SaldoRestante = Convert.ToDecimal(r["saldoRestante"]),
                                    AlumnoNombre = r["alumnoNombre"].ToString(),
                                });
                            }
                        }
                    }

                    return pago;
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el pago: " + ex.Message);
            }
        }


        /// Guarda el comprobante en la tabla `comprobantes` para poder reimprimirlo después.
        /// </summary>
        public void GuardarComprobante(Pago pago)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();

                    string concepto = pago.Detalles != null && pago.Detalles.Count > 0
                        ? string.Join(", ", pago.Detalles.Select(d => d.PeriodoCuota).Distinct())
                        : "Cobro de cuotas";

                    var primerDet = pago.Detalles?.FirstOrDefault();

                    // Columnas alineadas con la tabla real:
                    // NumeroComprobante, Fecha, Seccion, AlumnoId, AlumnoNombre,
                    // NombrePagador, DocumentoPagador, TipoDocumentoPagador,
                    // ConceptoCobro, MontoTotal, MontoRecibido, SaldoDeuda, Anulado
                    string sql = @"INSERT INTO comprobantes
                        (NumeroComprobante, Fecha, Seccion, AlumnoId, AlumnoNombre,
                         NombrePagador, DocumentoPagador, TipoDocumentoPagador,
                         ConceptoCobro, MontoTotal, MontoRecibido, SaldoDeuda, Anulado)
                        VALUES
                        (@nro, @fecha, @seccion, @alumnoId, @alumnoNombre,
                         @nombrePagador, @docPagador, @tipoDoc,
                         @concepto, @montoTotal, @montoRecibido, @saldo, 0)
                        ON DUPLICATE KEY UPDATE
                            Fecha         = @fecha,
                            MontoTotal    = @montoTotal,
                            MontoRecibido = @montoRecibido";

                    using (MySqlCommand cmd = new MySqlCommand(sql, conexion))
                    {
                        cmd.Parameters.AddWithValue("@nro", pago.NroComprobanteArca ?? pago.Id.ToString("D8"));
                        cmd.Parameters.AddWithValue("@fecha", pago.FechaPago);
                        cmd.Parameters.AddWithValue("@seccion", 1);
                        cmd.Parameters.AddWithValue("@alumnoId", primerDet?.AlumnoId ?? 0);
                        cmd.Parameters.AddWithValue("@alumnoNombre", primerDet?.AlumnoNombre ?? "");
                        cmd.Parameters.AddWithValue("@nombrePagador", pago.NombrePagador ?? "");
                        cmd.Parameters.AddWithValue("@docPagador", pago.DocumentoPagador ?? "");
                        cmd.Parameters.AddWithValue("@tipoDoc", pago.TipoPagador ?? "DNI");
                        cmd.Parameters.AddWithValue("@concepto", concepto);
                        cmd.Parameters.AddWithValue("@montoTotal", pago.TotalAplicado);
                        cmd.Parameters.AddWithValue("@montoRecibido", pago.MontoRecibido);
                        cmd.Parameters.AddWithValue("@saldo", 0.00m);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar comprobante: " + ex.Message + "\n\nINNER: " + ex.InnerException?.Message);
            }
        }

        // ==================== ARCA ====================

        public void GuardarDatosArca(int pagoId, string cae, DateTime caeVencimiento, string nroComprobante)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string sql = @"UPDATE pagos SET
                    cae                 = @cae,
                    caeVencimiento      = @venc,
                    nroComprobanteArca  = @nro,
                    estadoArca          = 'Emitido'
                WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conexion))
                    {
                        cmd.Parameters.AddWithValue("@cae", cae);
                        cmd.Parameters.AddWithValue("@venc", caeVencimiento.Date);
                        cmd.Parameters.AddWithValue("@nro", nroComprobante);
                        cmd.Parameters.AddWithValue("@id", pagoId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al guardar datos ARCA: " + ex.Message);
            }
        }
        public void MarcarPagoOmitidoArca(int pagoId)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string sql = "UPDATE pagos SET estadoArca = 'Omitido' WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id", pagoId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar estado ARCA: " + ex.Message);
            }
        }

    }

}
