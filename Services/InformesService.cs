using GestionEducativa.Database;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionEducativa.Services
{
    public class InformesService
    {
        private readonly ConexionBD _db;
        public InformesService() { _db = new ConexionBD(); }

        // ──────────────────────────────────────────────────────────
        // 1. INGRESOS POR FECHA  (rango de fechas)
        // ──────────────────────────────────────────────────────────
        public DataTable IngresosPorFecha(DateTime desde, DateTime hasta)
        {
            // Antes esto agrupaba por día (fecha, cantidad, total). Ahora lista cada pago
            // individual sin discriminar, y el total sigue apareciendo abajo de la grilla
            // (lo calcula sola la pantalla de informes, sumando la columna "Cobrado").
            // Una Nota de Crédito (tipoComprobante = 13) resta en vez de sumar, igual que
            // en el resto de los informes.
            string sql = @"
                SELECT
                    DATE(p.fechaPago)                    AS Fecha,
                    DATE_FORMAT(p.fechaPago, '%H:%i')    AS Hora,
                    p.nombrePagador                      AS Pagador,
                    p.documentoPagador                   AS Documento,
                    GROUP_CONCAT(
                        CONCAT(pd.periodoDescripcion, ' (', a.nombre, ')')
                        ORDER BY a.nombre
                        SEPARATOR ' | '
                    )                                    AS Conceptos,
                    p.formaPago                          AS `Forma de Pago`,
                    CASE WHEN p.tipoComprobante = 13 THEN 'Nota de Crédito' ELSE 'Factura C' END AS Comprobante,
                    p.nroComprobanteArca                 AS `Nro de Comprobante`,
                    CASE WHEN p.tipoComprobante = 13 THEN -p.totalAplicado ELSE p.totalAplicado END AS `Cobrado`
                FROM pagos p
                JOIN pagos_detalle pd ON pd.pagoId = p.id
                JOIN alumnos a ON pd.alumnoId = a.id
                WHERE DATE(p.fechaPago) BETWEEN @desde AND @hasta
                GROUP BY p.id
                ORDER BY p.fechaPago DESC";
            return Ejecutar(sql, p => {
                p.AddWithValue("@desde", desde.Date);
                p.AddWithValue("@hasta", hasta.Date);
            });
        }

        // ──────────────────────────────────────────────────────────
        // 1B. PROYECCIÓN MENSUAL — cuánto se facturó, cuánto se cobró
        //     y cuánto falta cobrar de un mes (o de todo el año)
        // ──────────────────────────────────────────────────────────
        public DataTable ProyeccionMensual(int anio, int? mes = null, string nivel = null)
        {
            // A diferencia de los otros informes, este NO se arma a partir de "pagos":
            // sale directo de "cuotas", que ya tiene todo neteado (descuento, recargo,
            // y lo que se descontó por Notas de Crédito) en montoFinal/montoPagado/saldo.
            // Facturado = lo que el colegio espera cobrar ese mes.
            // Cobrado   = lo que ya efectivamente entró.
            // Pendiente = lo que todavía falta cobrar.
            bool filtraMes = mes.HasValue && mes.Value > 0;
            bool filtraNivel = !string.IsNullOrEmpty(nivel);
            string sql = $@"
                SELECT
                    a.nivel                    AS Nivel,
                    COUNT(DISTINCT c.alumnoId) AS Alumnos,
                    SUM(c.montoFinal)          AS Facturado,
                    SUM(c.montoPagado)         AS Cobrado,
                    SUM(c.saldo)               AS Pendiente
                FROM cuotas c
                JOIN alumnos a ON c.alumnoId = a.id
                WHERE c.anio = @anio
                  {(filtraMes ? "AND c.mes = @mes" : "")}
                  {(filtraNivel ? "AND a.nivel = @nivel" : "")}
                GROUP BY a.nivel
                ORDER BY a.nivel";
            return Ejecutar(sql, p => {
                p.AddWithValue("@anio", anio);
                if (filtraMes) p.AddWithValue("@mes", mes.Value);
                if (filtraNivel) p.AddWithValue("@nivel", nivel);
            });
        }

        // ──────────────────────────────────────────────────────────
        // 2. INGRESOS POR NIVEL
        // ──────────────────────────────────────────────────────────
        public DataTable IngresosPorNivel(int anio, int? mes = null)
        {
            bool filtraMes = mes.HasValue && mes.Value > 0;
            string sql = $@"
                SELECT
                    a.nivel                     AS Nivel,
                    COUNT(DISTINCT pd.alumnoId) AS Alumnos,
                    SUM(CASE WHEN p.tipoComprobante = 13 THEN -pd.montoAplicado ELSE pd.montoAplicado END) AS `Total Cobrado`
                FROM pagos_detalle pd
                JOIN pagos p   ON pd.pagoId   = p.id
                JOIN alumnos a ON pd.alumnoId = a.id
                JOIN cuotas c  ON pd.cuotaId  = c.id
                WHERE c.anio = @anio
                  {(filtraMes ? "AND c.mes = @mes" : "")}
                GROUP BY a.nivel
                ORDER BY a.nivel";
            return Ejecutar(sql, p => {
                p.AddWithValue("@anio", anio);
                if (filtraMes) p.AddWithValue("@mes", mes.Value);
            });
        }

        // ──────────────────────────────────────────────────────────
        // 3. INGRESOS POR MES (resumen anual)
        // ──────────────────────────────────────────────────────────
        public DataTable IngresosPorMes(int anio, string nivel = null)
        {
            bool filtraNivel = !string.IsNullOrEmpty(nivel);
            string sql = $@"
                SELECT
                    c.mes                       AS Mes,
                    COUNT(DISTINCT pd.alumnoId) AS Alumnos,
                    SUM(CASE WHEN p.tipoComprobante = 13 THEN -pd.montoAplicado ELSE pd.montoAplicado END) AS `Total Cobrado`
                FROM pagos_detalle pd
                JOIN pagos p   ON pd.pagoId   = p.id
                JOIN cuotas c  ON pd.cuotaId  = c.id
                JOIN alumnos a ON pd.alumnoId = a.id
                WHERE c.anio = @anio
                  AND c.conceptoId IS NULL
                  {(filtraNivel ? "AND a.nivel = @nivel" : "")}
                GROUP BY c.mes
                ORDER BY c.mes";
            return Ejecutar(sql, p => {
                p.AddWithValue("@anio", anio);
                if (filtraNivel) p.AddWithValue("@nivel", nivel);
            });
        }

        // ──────────────────────────────────────────────────────────
        // 4. HISTORIAL DE PAGOS DE UN ALUMNO
        // ──────────────────────────────────────────────────────────
        public DataTable PagosAlumno(int alumnoId)
        {
            string sql = @"
                SELECT
                    DATE(p.fechaPago)       AS Fecha,
                    p.nombrePagador         AS Pagador,
                    pd.periodoDescripcion   AS Concepto,
                    pd.tipoCuota            AS Tipo,
                    p.formaPago             AS `Forma de Pago`,
                    CASE WHEN p.tipoComprobante = 13 THEN 'Nota de Crédito' ELSE 'Factura C' END AS Comprobante,
                    CASE WHEN p.tipoComprobante = 13 THEN -pd.montoAplicado ELSE pd.montoAplicado END AS `Monto Pagado`,
                    p.nroComprobanteArca    AS `Nro de Comprobante`
                FROM pagos_detalle pd
                JOIN pagos p ON pd.pagoId = p.id
                WHERE pd.alumnoId = @alumnoId
                ORDER BY p.fechaPago DESC";
            return Ejecutar(sql, p => p.AddWithValue("@alumnoId", alumnoId));
        }

        // ──────────────────────────────────────────────────────────
        // 5. HISTORIAL DE PAGOS DE UN PADRE/TUTOR
        // ──────────────────────────────────────────────────────────
        public DataTable PagosPadre(string nombrePagador = null, string documento = null)
        {
            bool porNombre = !string.IsNullOrEmpty(nombrePagador);
            bool porDoc = !string.IsNullOrEmpty(documento);
            string filtro = porDoc ? "AND p.documentoPagador = @doc"
                           : porNombre ? "AND p.nombrePagador LIKE @nombre"
                           : "";
            string sql = $@"
                SELECT
                    DATE(p.fechaPago)       AS Fecha,
                    p.nombrePagador         AS Pagador,
                    p.documentoPagador      AS Documento,
                    a.nombre                AS Alumno,
                    pd.periodoDescripcion   AS Concepto,
                    pd.tipoCuota            AS Tipo,
                    p.formaPago             AS `Forma de Pago`,
                    CASE WHEN p.tipoComprobante = 13 THEN 'Nota de Crédito' ELSE 'Factura C' END AS Comprobante,
                    CASE WHEN p.tipoComprobante = 13 THEN -pd.montoAplicado ELSE pd.montoAplicado END AS `Monto Pagado`,
                    CASE WHEN p.tipoComprobante = 13 THEN -p.totalAplicado ELSE p.totalAplicado END AS `Total del Pago`,
                    p.nroComprobanteArca    AS `Nro de Comprobante`
                FROM pagos p
                JOIN pagos_detalle pd ON pd.pagoId = p.id
                JOIN alumnos a ON pd.alumnoId = a.id
                WHERE 1=1 {filtro}
                ORDER BY p.fechaPago DESC";
            return Ejecutar(sql, p => {
                if (porDoc) p.AddWithValue("@doc", documento);
                if (porNombre) p.AddWithValue("@nombre", "%" + nombrePagador + "%");
            });
        }

        // ──────────────────────────────────────────────────────────
        // 6. MATRICULADOS — con fecha de matrícula, monto y pagador
        // ──────────────────────────────────────────────────────────
        public DataTable Matriculados(int anio, string nivelEducativo = null, string nivel = null, string division = null)
        {
            bool filtraGrupo = !string.IsNullOrEmpty(nivelEducativo);
            bool filtraNivel = !string.IsNullOrEmpty(nivel);
            bool filtraDivision = !string.IsNullOrEmpty(division);

            // Mismo criterio de clasificación que en InformesForm.ClasificarNivelEducativo:
            // jardines = Inicial, "grado" = Primaria, "ano/año" = Secundaria.
            const string grupoCase = @"
                CASE
                    WHEN LOWER(a.nivel) LIKE '%jardin%' OR LOWER(a.nivel) LIKE '%jardín%' THEN 'Inicial'
                    WHEN LOWER(a.nivel) LIKE '%grado%' THEN 'Primaria'
                    WHEN LOWER(a.nivel) LIKE '%ano%' OR LOWER(a.nivel) LIKE '%año%' THEN 'Secundaria'
                    ELSE 'Otro'
                END";

            string sql = $@"
                SELECT
                    a.nombre                AS Alumno,
                    {grupoCase}              AS `Nivel Educativo`,
                    a.nivel                  AS Grado,
                    a.division               AS División,
                    c.montoFinal             AS Monto,
                    c.estado                 AS `Estado Matrícula`,
                    (SELECT MIN(DATE(p2.fechaPago))
                     FROM pagos p2
                     JOIN pagos_detalle pd2 ON p2.id = pd2.pagoId
                     WHERE pd2.cuotaId = c.id
                    )                        AS `Fecha de Pago`,
                    -- El pagador solo aparece si el alumno efectivamente entró (hay al
                    -- menos un pago aplicado a la cuota de matrícula); si no pagó nada
                    -- todavía, esta columna queda vacía.
                    (SELECT p3.nombrePagador
                     FROM pagos p3
                     JOIN pagos_detalle pd3 ON p3.id = pd3.pagoId
                     WHERE pd3.cuotaId = c.id
                     ORDER BY p3.fechaPago ASC
                     LIMIT 1
                    )                        AS Pagador
                FROM cuotas c
                JOIN alumnos a ON c.alumnoId = a.id
                WHERE c.anio = @anio
                  AND LOWER(c.tipoCuota) IN ('matricula','matrícula')
                  AND a.Activo = 1
                  {(filtraGrupo ? $"AND ({grupoCase}) = @grupo" : "")}
                  {(filtraNivel ? "AND a.nivel = @nivel" : "")}
                  {(filtraDivision ? "AND a.division = @division" : "")}
                ORDER BY a.nivel, a.division, a.nombre";
            return Ejecutar(sql, p => {
                p.AddWithValue("@anio", anio);
                if (filtraGrupo) p.AddWithValue("@grupo", nivelEducativo);
                if (filtraNivel) p.AddWithValue("@nivel", nivel);
                if (filtraDivision) p.AddWithValue("@division", division);
            });
        }

        // ──────────────────────────────────────────────────────────
        // 6b. ESTADO DE PAGO POR MES — quién pagó y quién no, por curso/división
        // ──────────────────────────────────────────────────────────
        /// <summary>
        /// Para un mes/año puntual, muestra a cada alumno activo con su estado de pago
        /// de ese mes (Pagado / Pago Parcial / No Pagado / Sin cuota generada), agrupado
        /// por curso (nivel) y división. Si el alumno tiene más de una cuota ese mes
        /// (ej. mensual + algún concepto extra), se agregan en un solo estado: "Pagado"
        /// sólo si TODAS están pagadas, "No Pagado" si ninguna tiene nada abonado, y
        /// "Pago Parcial" en el resto de los casos. No incluye cuotas de matrícula
        /// (esas ya se ven en el informe de Matriculados).
        /// </summary>
        public DataTable EstadoPagoPorMes(int anio, int mes, string nivel = null, string division = null)
        {
            bool filtraNivel = !string.IsNullOrEmpty(nivel);
            bool filtraDivision = !string.IsNullOrEmpty(division);

            string sql = $@"
                SELECT
                    a.nivel      AS Curso,
                    a.division   AS División,
                    a.nombre     AS Alumno,
                    CASE
                        WHEN COUNT(c.id) = 0 THEN 'Sin cuota generada'
                        WHEN SUM(CASE WHEN c.estado = 'Pagado' THEN 1 ELSE 0 END) = COUNT(c.id) THEN 'Pagado'
                        WHEN SUM(c.montoPagado) = 0 THEN 'No Pagado'
                        ELSE 'Pago Parcial'
                    END          AS Estado,
                    COALESCE(SUM(c.montoOriginal), 0) AS `Monto Cuota`,
                    COALESCE(SUM(c.recargo), 0)     AS Recargo,
                    COALESCE(SUM(c.montoPagado), 0) AS Cobrado,
                    COALESCE(SUM(c.saldo), 0)       AS Saldo
                FROM alumnos a
                LEFT JOIN cuotas c
                    ON c.alumnoId = a.id
                   AND c.mes = @mes
                   AND c.anio = @anio
                   AND LOWER(c.tipoCuota) NOT IN ('matricula','matrícula')
                WHERE a.Activo = 1
                  AND (a.anioIngreso IS NULL OR a.anioIngreso <= @anio)
                  {(filtraNivel ? "AND a.nivel = @nivel" : "")}
                  {(filtraDivision ? "AND a.division = @division" : "")}
                GROUP BY a.id, a.nivel, a.division, a.nombre
                ORDER BY a.nivel, a.division, a.nombre";

            return Ejecutar(sql, p => {
                p.AddWithValue("@mes", mes);
                p.AddWithValue("@anio", anio);
                if (filtraNivel) p.AddWithValue("@nivel", nivel);
                if (filtraDivision) p.AddWithValue("@division", division);
            });
        }
        // ──────────────────────────────────────────────────────────
        // 6c. ALUMNOS SIN PAGOS REGISTRADOS — nunca pagaron nada, con su
        // deuda acumulada desde un mes puntual (por defecto, julio)
        // ──────────────────────────────────────────────────────────
        /// <summary>
        /// Alumnos activos que NUNCA registraron un pago (ni parcial ni total, en
        /// ninguna cuota, de ningún mes/año) — agrupados por curso y división, con
        /// una columna de cuánto adeudan acumulado desde `mesDesde` (por defecto,
        /// julio) del año indicado.
        /// </summary>
        public DataTable AlumnosSinPagos(int anio, int mesDesde = 7, string nivel = null, string division = null)
        {
            bool filtraNivel = !string.IsNullOrEmpty(nivel);
            bool filtraDivision = !string.IsNullOrEmpty(division);

            string nombreMes = new System.Globalization.CultureInfo("es-AR")
                .DateTimeFormat.GetMonthName(mesDesde);
            string aliasColumna = $"Deuda desde {nombreMes}";

            string sql = $@"
                SELECT
                    a.nivel      AS Curso,
                    a.division   AS División,
                    a.nombre     AS Alumno,
                    COALESCE(SUM(
                        CASE WHEN c.mes >= @mesDesde AND c.anio = @anio
                             AND LOWER(c.tipoCuota) NOT IN ('matricula','matrícula')
                        THEN c.saldo ELSE 0 END
                    ), 0) AS `{aliasColumna}`
                FROM alumnos a
                LEFT JOIN cuotas c ON c.alumnoId = a.id
                WHERE a.Activo = 1
                  AND (a.anioIngreso IS NULL OR a.anioIngreso <= @anio)
                  AND NOT EXISTS (
                      SELECT 1 FROM cuotas c2
                      WHERE c2.alumnoId = a.id AND c2.montoPagado > 0
                  )
                  {(filtraNivel ? "AND a.nivel = @nivel" : "")}
                  {(filtraDivision ? "AND a.division = @division" : "")}
                GROUP BY a.id, a.nivel, a.division, a.nombre
                ORDER BY a.nivel, a.division, a.nombre";

            return Ejecutar(sql, p => {
                p.AddWithValue("@mesDesde", mesDesde);
                p.AddWithValue("@anio", anio);
                if (filtraNivel) p.AddWithValue("@nivel", nivel);
                if (filtraDivision) p.AddWithValue("@division", division);
            });
        }
        // ──────────────────────────────────────────────────────────
        // 7. LISTADO DE ALUMNOS — campos configurables
        // ──────────────────────────────────────────────────────────
        public DataTable ListadoAlumnos(string nivel = null, string division = null,
            bool incluirNombre = true, bool incluirNivel = true,
            bool incluirDivision = true, bool incluirDni = false,
            bool incluirDescuento = false, bool incluirSaldo = false,
            bool incluirPadre = false, bool incluirTelefono = false,
            bool incluirDniPadre = false, bool incluirMailPadre = false)
        {
            // Construir SELECT dinámico
            var cols = new List<string>();
            if (incluirNombre) cols.Add("a.nombre           AS Alumno");
            if (incluirNivel) cols.Add("a.nivel            AS Nivel");
            if (incluirDivision) cols.Add("a.division         AS División");
            if (incluirDni) cols.Add("a.dni              AS DNI");
            if (incluirDescuento) cols.Add("a.descuento        AS `% Descuento`");
            if (incluirSaldo) cols.Add(@"COALESCE((SELECT SUM(c2.saldo) FROM cuotas c2
                                    WHERE c2.alumnoId = a.id AND c2.saldo > 0),0)
                                    AS `Saldo Pendiente`");
            if (incluirPadre) cols.Add(@"(SELECT CONCAT(p2.apellido, ', ', p2.nombre) FROM padres p2
                    JOIN alumnos_padres ap ON p2.id = ap.padreId
                    WHERE ap.alumnoId = a.id LIMIT 1)
                    AS `Padre/Madre`");
            if (incluirDniPadre) cols.Add(@"(SELECT COALESCE(p2.dni, p2.cuit) FROM padres p2
                    JOIN alumnos_padres ap ON p2.id = ap.padreId
                    WHERE ap.alumnoId = a.id LIMIT 1)
                    AS `DNI/CUIL Padre`");
            if (incluirTelefono) cols.Add(@"(SELECT p2.telefono FROM padres p2
                                    JOIN alumnos_padres ap ON p2.id = ap.padreId
                                    WHERE ap.alumnoId = a.id LIMIT 1)
                                    AS Teléfono");
            if (incluirMailPadre) cols.Add(@"(SELECT p2.email FROM padres p2 
                                    JOIN alumnos_padres ap ON p2.id = ap.padreId
                                    WHERE ap.alumnoId = a.id LIMIT 1)
                                    AS `Mail Padre`");
            if (cols.Count == 0) cols.Add("a.nombre AS Alumno");

            bool filtraNivel = !string.IsNullOrEmpty(nivel);
            bool filtraDivision = !string.IsNullOrEmpty(division);
            string sql = $@"
                SELECT {string.Join(", ", cols)}
                FROM alumnos a
                WHERE a.Activo = 1
                  {(filtraNivel ? "AND a.nivel = @nivel" : "")}
                  {(filtraDivision ? "AND a.division = @division" : "")}
                ORDER BY a.nivel, a.division, a.nombre";
            return Ejecutar(sql, p => {
                if (filtraNivel) p.AddWithValue("@nivel", nivel);
                if (filtraDivision) p.AddWithValue("@division", division);
            });
        }

        // ──────────────────────────────────────────────────────────
        // 8. CIERRE DE CAJA — efectivo del día
        // ──────────────────────────────────────────────────────────
        public DataTable CierreCaja(DateTime fecha)
        {
            // Se unen Cobros (tabla pagos) y Gastos (tabla gastos) en un mismo listado
            // cronológico, para que el cierre de caja muestre todo lo que entró y lo que
            // salió durante el día, en el orden en que efectivamente pasó.
            string sql = @"
                SELECT * FROM (
                    SELECT
                        p.fechaPago                          AS FechaHora,
                        DATE_FORMAT(p.fechaPago, '%H:%i')    AS Hora,
                        p.nombrePagador                      AS Pagador,
                        p.documentoPagador                   AS Documento,
                        GROUP_CONCAT(
                            CONCAT(pd.periodoDescripcion, ' (', a.nombre, ')')
                            ORDER BY a.nombre
                            SEPARATOR ' | '
                        )                                    AS Conceptos,
                        CASE WHEN p.tipoComprobante = 13 THEN 'Nota de Crédito' ELSE 'Factura C' END AS Comprobante,
                        CASE WHEN p.tipoComprobante = 13 THEN -p.totalAplicado ELSE p.totalAplicado END AS Cobrado,
                        CASE WHEN p.cae IS NOT NULL
                             AND p.cae != '' THEN '✓' ELSE '–' END AS Factura
                    FROM pagos p
                    JOIN pagos_detalle pd ON pd.pagoId = p.id
                    JOIN alumnos a ON pd.alumnoId = a.id
                    WHERE DATE(p.fechaPago) = @fecha
                      AND p.formaPago = 'Efectivo'
                    GROUP BY p.id

                    UNION ALL

                    SELECT
                        g.fechaRegistro                          AS FechaHora,
                        DATE_FORMAT(g.fechaRegistro, '%H:%i')    AS Hora,
                        COALESCE(g.comprobanteProveedor, '-')    AS Pagador,
                        COALESCE(g.comprobanteCuitProveedor, '') AS Documento,
                        CONCAT(g.tipoGasto, ': ', g.detalle)     AS Conceptos,
                        'Gasto'                                  AS Comprobante,
                        -g.monto                                 AS Cobrado,
                        CASE WHEN g.comprobanteNumero IS NOT NULL
                             AND g.comprobanteNumero != '' THEN '✓' ELSE '–' END AS Factura
                    FROM gastos g
                    WHERE g.fecha = @fecha
                      AND g.formaPago = 'Efectivo'
                      AND g.activo = 1
                ) t
                ORDER BY t.FechaHora";
            var dt = Ejecutar(sql, p => p.AddWithValue("@fecha", fecha.Date));
            dt.Columns.Remove("FechaHora"); // era solo para ordenar, no hace falta mostrarla
            return dt;
        }

        public (decimal totalRecibido, decimal totalCobrado, decimal totalVuelto, int cantPagos, decimal totalGastos, decimal totalNeto)
            ResumenCaja(DateTime fecha)
        {
            // Una Nota de Crédito en efectivo (poco común, pero puede pasar si se le
            // devuelve la plata en mano al padre) resta del total del día — no es "otro
            // cobro más" para el conteo de pagos, ni suma al total recibido/cobrado.
            string sql = @"
                SELECT
                    COUNT(CASE WHEN tipoComprobante = 13 THEN NULL ELSE 1 END)         AS cant,
                    SUM(CASE WHEN tipoComprobante = 13 THEN -montoRecibido ELSE montoRecibido END) AS recibido,
                    SUM(CASE WHEN tipoComprobante = 13 THEN -totalAplicado ELSE totalAplicado END) AS cobrado,
                    SUM(CASE WHEN tipoComprobante = 13 THEN 0 ELSE vuelto END)        AS vuelto
                FROM pagos
                WHERE DATE(fechaPago) = @fecha
                  AND formaPago = 'Efectivo'";

            decimal recibido = 0, cobrado = 0, vuelto = 0;
            int cant = 0;

            using (var con = _db.ObtenerConexion())
            {
                con.Open();
                using (var cmd = new MySqlCommand(sql, con))
                {
                    cmd.Parameters.AddWithValue("@fecha", fecha.Date);
                    using (var r = cmd.ExecuteReader())
                    {
                        if (r.Read())
                        {
                            recibido = r["recibido"] == DBNull.Value ? 0 : Convert.ToDecimal(r["recibido"]);
                            cobrado = r["cobrado"] == DBNull.Value ? 0 : Convert.ToDecimal(r["cobrado"]);
                            vuelto = r["vuelto"] == DBNull.Value ? 0 : Convert.ToDecimal(r["vuelto"]);
                            cant = r["cant"] == DBNull.Value ? 0 : Convert.ToInt32(r["cant"]);
                        }
                    }
                }
            }

            // Gastos en efectivo del día (todas las secciones, igual que el resto de este
            // resumen no distingue Colegio de Scuola Italiano).
            decimal gastos = new GastoService().TotalGastosDia(fecha, seccion: null, formaPago: "Efectivo");
            decimal neto = cobrado - gastos;

            return (recibido, cobrado, vuelto, cant, gastos, neto);
        }

        // ──────────────────────────────────────────────────────────
        // 9. Listas auxiliares para filtros
        // ──────────────────────────────────────────────────────────
        public List<string> ObtenerNiveles()
        {
            var lista = new List<string>();
            using (var con = _db.ObtenerConexion())
            {
                con.Open();
                using (var cmd = new MySqlCommand(
                    "SELECT DISTINCT nivel FROM alumnos WHERE Activo=1 ORDER BY nivel", con))
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) if (!r.IsDBNull(0)) lista.Add(r.GetString(0));
            }
            return lista;
        }

        public List<string> ObtenerDivisiones(string nivel = null)
        {
            var lista = new List<string>();
            string sql = "SELECT DISTINCT division FROM alumnos WHERE Activo=1 AND division IS NOT NULL"
                       + (nivel != null ? " AND nivel=@nivel" : "")
                       + " ORDER BY division";
            using (var con = _db.ObtenerConexion())
            {
                con.Open();
                using (var cmd = new MySqlCommand(sql, con))
                {
                    if (nivel != null) cmd.Parameters.AddWithValue("@nivel", nivel);
                    using (var r = cmd.ExecuteReader())
                        while (r.Read()) if (!r.IsDBNull(0)) lista.Add(r.GetString(0));
                }
            }
            return lista;
        }

        public List<string> ObtenerPadresUnicos()
        {
            var lista = new List<string>();
            using (var con = _db.ObtenerConexion())
            {
                con.Open();
                using (var cmd = new MySqlCommand(
                    "SELECT DISTINCT nombrePagador FROM pagos WHERE nombrePagador IS NOT NULL ORDER BY nombrePagador", con))
                using (var r = cmd.ExecuteReader())
                    while (r.Read()) if (!r.IsDBNull(0)) lista.Add(r.GetString(0));
            }
            return lista;
        }

        // ──────────────────────────────────────────────────────────
        // Helper genérico
        // ──────────────────────────────────────────────────────────
        private DataTable Ejecutar(string sql, Action<MySqlParameterCollection> parametros)
        {
            var dt = new DataTable();
            try
            {
                using (var con = _db.ObtenerConexion())
                {
                    con.Open();
                    using (var cmd = new MySqlCommand(sql, con))
                    {
                        parametros?.Invoke(cmd.Parameters);
                        using (var adapter = new MySqlDataAdapter(cmd))
                            adapter.Fill(dt);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al generar informe: " + ex.Message);
            }
            return dt;
        }
    }
}
