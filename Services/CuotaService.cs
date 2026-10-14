using GestionEducativa.Database;
using GestionEducativa.Models;
using GestionEducativa.Constants;
using GestionEducativa.Utils;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GestionEducativa.Services
{
    /// <summary>
    /// Todo lo relacionado a la cuota en sí: consultarlas, generarlas (una por
    /// una, por concepto, o en tanda mensual masiva), calcular y aplicar el
    /// recargo por mora, y ajustar saldo/recargo manualmente.
    /// Separado de PagoService, que se ocupa de REGISTRAR COBROS sobre esas
    /// cuotas (multi-cuota, notas de crédito, ARCA) — son responsabilidades
    /// distintas que antes vivían mezcladas en una sola clase de 1800 líneas.
    /// </summary>
    public class CuotaService
    {
        private readonly ConexionBD conexionBD;

        public CuotaService()
        {
            conexionBD = new ConexionBD();
        }

        public List<Cuota> ObtenerCuotasAlumno(int alumnoId)
        {
            var cuotas = new List<Cuota>();
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = @"
                        SELECT c.id, c.alumnoId, c.mes, c.anio, c.tipoCuota, c.conceptoId,
                               c.montoOriginal, c.descuento, c.montoFinal,
                               c.montoPagado, c.saldo, c.estado, c.fechaVencimiento,c.recargo,
                               a.nombre AS alumnoNombre, a.nivel AS alumnoNivel,
                               a.anioIngreso AS alumnoAnioIngreso, co.nombre AS nombreConcepto, co.nivel AS conceptoNivel
                        FROM cuotas c
                        JOIN alumnos a ON c.alumnoId = a.id
                        LEFT JOIN conceptos co ON c.conceptoId = co.id
                        WHERE c.alumnoId = @alumnoId
                        ORDER BY c.anio DESC, c.mes ASC";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@alumnoId", alumnoId);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                                cuotas.Add(MapearCuota(r));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener las cuotas del alumno: " + ex.Message);
            }
            return cuotas;
        }
        public List<Cuota> ObtenerCuotasPendientesAlumno(int alumnoId)
        {
            var cuotas = new List<Cuota>();
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = @"
                        SELECT c.id, c.alumnoId, c.mes, c.anio, c.tipoCuota, c.conceptoId,
                               c.montoOriginal, c.descuento, c.montoFinal,
                               c.montoPagado, c.saldo, c.estado, c.fechaVencimiento, c.recargo,
                               a.nombre AS alumnoNombre, a.nivel AS alumnoNivel,
                               a.anioIngreso AS alumnoAnioIngreso, co.nombre AS nombreConcepto, co.nivel AS conceptoNivel
                        FROM cuotas c
                        JOIN alumnos a ON c.alumnoId = a.id
                        LEFT JOIN conceptos co ON c.conceptoId = co.id
                        WHERE c.alumnoId = @alumnoId AND (c.saldo > 0 OR (c.descuento >= 100 AND c.estado <> 'Pagado'))
                        ORDER BY c.anio ASC, c.mes ASC";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@alumnoId", alumnoId);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                                cuotas.Add(MapearCuota(r));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener cuotas pendientes: " + ex.Message);
            }
            return cuotas;
        }
        public List<Cuota> ObtenerCuotasAlumnos(List<int> alumnoIds)
        {
            var cuotas = new List<Cuota>();
            if (alumnoIds == null || alumnoIds.Count == 0) return cuotas;

            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();

                    string parametros = string.Join(",", alumnoIds.ConvertAll(id => "@id" + id));
                    string consulta = $@"
                        SELECT c.id, c.alumnoId, c.mes, c.anio, c.tipoCuota, c.conceptoId,
                               c.montoOriginal, c.descuento, c.montoFinal,
                               c.montoPagado, c.saldo, c.estado, c.fechaVencimiento, c.recargo,
                               a.nombre AS alumnoNombre, a.nivel AS alumnoNivel,
                               a.anioIngreso AS alumnoAnioIngreso, co.nombre AS nombreConcepto, co.nivel AS conceptoNivel
                        FROM cuotas c
                        JOIN alumnos a ON c.alumnoId = a.id
                        LEFT JOIN conceptos co ON c.conceptoId = co.id
                        WHERE c.alumnoId IN ({parametros})
                        ORDER BY c.anio DESC, c.mes ASC, a.nombre ASC";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        foreach (int id in alumnoIds)
                            cmd.Parameters.AddWithValue("@id" + id, id);

                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                var cuota = MapearCuota(r);
                                if (!r.IsDBNull(r.GetOrdinal("alumnoNombre")))
                                    cuota.AlumnoNombre = r["alumnoNombre"].ToString();
                                cuotas.Add(cuota);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener cuotas de alumnos: " + ex.Message);
            }
            return cuotas;
        }
        public List<Cuota> ObtenerCuotasPendientesAlumnos(List<int> alumnoIds)
        {
            var cuotas = new List<Cuota>();
            if (alumnoIds == null || alumnoIds.Count == 0) return cuotas;

            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();

                    string parametros = string.Join(",", alumnoIds.ConvertAll(id => "@id" + id));
                    string consulta = $@"
                        SELECT c.id, c.alumnoId, c.mes, c.anio, c.tipoCuota, c.conceptoId,
                               c.montoOriginal, c.descuento, c.montoFinal,
                               c.montoPagado, c.saldo, c.estado, c.fechaVencimiento, c.recargo,
                               a.nombre AS alumnoNombre, a.nivel AS alumnoNivel,
                               a.anioIngreso AS alumnoAnioIngreso, co.nombre AS nombreConcepto, co.nivel AS conceptoNivel
                        FROM cuotas c
                        JOIN alumnos a ON c.alumnoId = a.id
                        LEFT JOIN conceptos co ON c.conceptoId = co.id
                        WHERE c.alumnoId IN ({parametros}) AND (c.saldo > 0 OR (c.descuento >= 100 AND c.estado <> 'Pagado'))
                        ORDER BY c.anio ASC, c.mes ASC, a.nombre ASC";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        foreach (int id in alumnoIds)
                            cmd.Parameters.AddWithValue("@id" + id, id);

                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                var cuota = MapearCuota(r);
                                if (!r.IsDBNull(r.GetOrdinal("alumnoNombre")))
                                    cuota.AlumnoNombre = r["alumnoNombre"].ToString();
                                cuotas.Add(cuota);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener cuotas pendientes de alumnos: " + ex.Message);
            }
            return cuotas;
        }
        private Cuota MapearCuota(MySqlDataReader r)
        {
            var level = r.HasColumn("alumnoNivel") && r["alumnoNivel"] != DBNull.Value
                ? r["alumnoNivel"].ToString() : null;
            var nivelYAnio = CalcularNivelYAnioParaEtiquetaMatricula(r);
            return new Cuota
            {
                Id = Convert.ToInt32(r["id"]),
                AlumnoId = Convert.ToInt32(r["alumnoId"]),
                AlumnoNombre = r.HasColumn("alumnoNombre") && r["alumnoNombre"] != DBNull.Value
                    ? r["alumnoNombre"].ToString() : null,
                // mes es INT en la BD → convertir a nombre para mostrar
                Mes = NumeroANombreMes(Convert.ToInt32(r["mes"])),
                Anio = Convert.ToInt32(r["anio"]),
                TipoCuota = r.HasColumn("tipoCuota") && r["tipoCuota"] != DBNull.Value
                    ? r["tipoCuota"].ToString() : "Curricular",
                ConceptoId = r.HasColumn("conceptoId") && r["conceptoId"] != DBNull.Value
                    ? Convert.ToInt32(r["conceptoId"]) : (int?)null,
                NombreConcepto = r.HasColumn("nombreConcepto") && r["nombreConcepto"] != DBNull.Value
                    ? r["nombreConcepto"].ToString() : null,
                MontoOriginal = r["montoOriginal"] == DBNull.Value ? 0m : Convert.ToDecimal(r["montoOriginal"]),
                Descuento = r["descuento"] == DBNull.Value ? 0m : Convert.ToDecimal(r["descuento"]),
                MontoFinal = r["montoFinal"] == DBNull.Value ? 0m : Convert.ToDecimal(r["montoFinal"]),
                MontoPagado = r["montoPagado"] == DBNull.Value ? 0m : Convert.ToDecimal(r["montoPagado"]),
                Saldo = r["saldo"] == DBNull.Value ? 0m : Convert.ToDecimal(r["saldo"]),
                Estado = r["estado"].ToString(),
                FechaVencimiento = r["fechaVencimiento"] == DBNull.Value
                    ? (DateTime?)null : Convert.ToDateTime(r["fechaVencimiento"]),
                Recargo = r["recargo"] == DBNull.Value ? 0m : Convert.ToDecimal(r["recargo"]),
                Nivel = r.HasColumn("alumnoNivel") && r["alumnoNivel"] != DBNull.Value
                    ? r["alumnoNivel"].ToString() : null,
                NivelSiguiente = nivelYAnio.nivel,
                MatriculaEsDeEsteAnio = !nivelYAnio.sumarUnAnioEnLabel
            };
        }
        /// <summary>
        /// Calcula qué nivel mostrar en la etiqueta de una cuota de matrícula:
        /// — Alumno que ya está cursando: nivel SIGUIENTE al actual (al que va a pasar).
        /// — Alumno preinscripto cuya cuota es justamente la reserva/matrícula para el año
        ///   en que ingresa: se usa su nivel TAL CUAL está cargado (ya es el nivel de
        ///   destino), sin saltar uno más.
        /// </summary>
        /// <summary>
        /// Calcula qué nivel mostrar en la etiqueta de una cuota de matrícula, y si hay
        /// que sumarle un año al mostrarla.
        /// IMPORTANTE: usa el nivel real guardado en el CONCEPTO de esa cuota
        /// (conceptos.nivel), no el nivel actual "en vivo" del alumno — porque desde que
        /// se puede generar tanto la matrícula del nivel actual (este año) como la del
        /// nivel siguiente (año que viene), recalcular "nivel siguiente" a partir del
        /// nivel actual del alumno ya no alcanza para saber a cuál de las dos corresponde
        /// esta cuota puntual.
        ///
        /// El "+1" en el label SÓLO aplica al esquema histórico de preinscriptos, donde
        /// la cuota se graba con anio = añoDeIngreso - 1 a propósito. Para un alumno
        /// cursante, desde este cambio el año correcto (2026 o 2027, según corresponda)
        /// ya se graba directo en `cuotas.anio` al generarla — así que acá NO hay que
        /// sumarle nada más, sólo mostrarlo tal cual. (Ver ConceptosPanel.ResolverAnioDeLaCuota,
        /// que es donde se decide y graba ese año al momento de generar la cuota).
        /// </summary>
        private static (string nivel, bool sumarUnAnioEnLabel) CalcularNivelYAnioParaEtiquetaMatricula(MySqlDataReader r)
        {
            if (!r.HasColumn("alumnoNivel") || r["alumnoNivel"] == DBNull.Value)
                return (null, false);

            string alumnoNivel = r["alumnoNivel"].ToString();

            int? anioIngreso = r.HasColumn("alumnoAnioIngreso") && r["alumnoAnioIngreso"] != DBNull.Value
                ? (int?)Convert.ToInt32(r["alumnoAnioIngreso"]) : null;
            int anioCuota = Convert.ToInt32(r["anio"]);
            bool esMatriculaDeSuIngreso = anioIngreso.HasValue && anioIngreso.Value == anioCuota + 1;
            if (esMatriculaDeSuIngreso)
                return (alumnoNivel, true); // preinscripto: esquema histórico, sigue sumando +1

            // Cursante: el concepto sabe a qué nivel corresponde esta matrícula puntual,
            // y el año ya viene correctamente grabado en la cuota — no se suma nada más.
            string conceptoNivel = r.HasColumn("conceptoNivel") && r["conceptoNivel"] != DBNull.Value
                ? r["conceptoNivel"].ToString() : null;

            if (!string.IsNullOrEmpty(conceptoNivel))
                return (conceptoNivel, false);

            // Fallback (concepto sin nivel específico, ej. aplica a "todos"): se mantiene el
            // comportamiento histórico de asumir nivel siguiente.
            return (NivelUtils.ObtenerNivelSiguiente(alumnoNivel), false);
        }
        private static string NumeroANombreMes(int mes)
        {
            switch (mes)
            {
                case 1: return "Enero";
                case 2: return "Febrero";
                case 3: return "Marzo";
                case 4: return "Abril";
                case 5: return "Mayo";
                case 6: return "Junio";
                case 7: return "Julio";
                case 8: return "Agosto";
                case 9: return "Septiembre";
                case 10: return "Octubre";
                case 11: return "Noviembre";
                case 12: return "Diciembre";
                default: return mes.ToString();
            }
        }
        private static string CategoriAATipoCuota(string categoria)
        {
            if (string.IsNullOrEmpty(categoria)) return "Otro";
            string cat = categoria.Trim().ToLowerInvariant()
                .Replace("á", "a").Replace("é", "e").Replace("í", "i")
                .Replace("ó", "o").Replace("ú", "u");
            switch (cat)
            {
                case "matricula": return "Matricula";
                case "seguro": return "Seguro";
                case "viaje": return "Viaje";
                case "entornodigital":
                case "entorno digital": return "Entorno digital";
                case "quadernetti": return "Quadernetti";
                case "bingo": return "Bingo";

                default:
                    // Categoría personalizada: se guarda tal como la escribió el usuario
                    return System.Globalization.CultureInfo.CurrentCulture.TextInfo
                        .ToTitleCase(categoria.Trim().ToLower());
            }
        }
        public void GenerarCuotaConcepto(int alumnoId, int conceptoId, int anio, int porcentajeDescuento = 0, decimal montoOverride = 0)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();

                    // Verificar duplicado
                    string checkDup = @"
                SELECT COUNT(*) FROM cuotas
                WHERE alumnoId = @alumnoId AND conceptoId = @conceptoId AND anio = @anio";
                    using (MySqlCommand chk = new MySqlCommand(checkDup, conexion))
                    {
                        chk.Parameters.AddWithValue("@alumnoId", alumnoId);
                        chk.Parameters.AddWithValue("@conceptoId", conceptoId);
                        chk.Parameters.AddWithValue("@anio", anio);
                        int existe = Convert.ToInt32(chk.ExecuteScalar());
                        if (existe > 0)
                            throw new Exception("Ya existe una cuota de este concepto para el alumno en ese año.");
                    }

                    // Obtener concepto (incluimos categoria para derivar tipoCuota)
                    string getConcepto = "SELECT nombre, categoria, monto FROM conceptos WHERE id = @id";
                    string nombreConcepto = "";
                    string categoriaConcepto = "";
                    decimal montoBase = 0;
                    using (MySqlCommand gc = new MySqlCommand(getConcepto, conexion))
                    {
                        gc.Parameters.AddWithValue("@id", conceptoId);
                        using (MySqlDataReader r = gc.ExecuteReader())
                        {
                            if (!r.Read()) throw new Exception("Concepto no encontrado.");
                            nombreConcepto = r["nombre"].ToString();
                            categoriaConcepto = r["categoria"].ToString();
                            montoBase = Convert.ToDecimal(r["monto"]);
                        }
                    }

                    // Mapear categoria → tipoCuota
                    string tipoCuota = CategoriAATipoCuota(categoriaConcepto);

                    // Si se pasó un monto manual (ej: matrícula), usarlo en lugar del de la BD
                    if (montoOverride > 0)
                        montoBase = montoOverride;

                    decimal descuento = Math.Round(montoBase * porcentajeDescuento / 100m, 2);
                    decimal montoFinal = montoBase - descuento;

                    string insert = @"
    INSERT INTO cuotas
        (alumnoId, mes, anio, tipoCuota, montoOriginal, descuento,
         montoPagado, estado, fechaVencimiento, conceptoId)
    VALUES
        (@alumnoId, @mes, @anio, @tipoCuota, @montoOriginal, @descuento,
         0, 'Pendiente', NULL, @conceptoId)";

                    using (MySqlCommand cmd = new MySqlCommand(insert, conexion))
                    {
                        cmd.Parameters.AddWithValue("@alumnoId", alumnoId);
                        cmd.Parameters.AddWithValue("@mes", 0);
                        cmd.Parameters.AddWithValue("@anio", anio);
                        cmd.Parameters.AddWithValue("@tipoCuota", tipoCuota);
                        cmd.Parameters.AddWithValue("@montoOriginal", montoBase);
                        cmd.Parameters.AddWithValue("@descuento", porcentajeDescuento); // porcentaje, no importe
                        cmd.Parameters.AddWithValue("@conceptoId", conceptoId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al generar cuota de concepto: " + ex.Message);
            }
        }
        // ==================== GENERACIÓN MASIVA ====================

        /// <summary>
        /// Lista los niveles que existen en la tabla alumnos (distintos y activos).
        /// </summary>
        public List<string> ObtenerNivelesExistentes(int? seccion = null)
        {
            var lista = new List<string>();
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string sql = "SELECT DISTINCT nivel FROM alumnos WHERE Activo = 1";
                    if (seccion.HasValue) sql += " AND seccion = @seccion";
                    sql += " ORDER BY nivel";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conexion))
                    {
                        if (seccion.HasValue) cmd.Parameters.AddWithValue("@seccion", seccion.Value);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                            while (r.Read())
                                if (!r.IsDBNull(0)) lista.Add(r.GetString(0));
                    }
                }
            }
            catch (Exception ex) { throw new Exception("Error al obtener niveles: " + ex.Message); }
            return lista;
        }
        /// <summary>
        /// Genera cuotas mensuales para todos los alumnos de los niveles indicados.
        /// — Vencimiento: día 15 del mes correspondiente.
        /// — Si ya existe cuota del mismo alumno/mes/año/tipo (UK_Cuotas) la saltea.
        /// — Aplica el porcentaje de descuento individual de cada alumno.
        /// Devuelve (generadas, salteadas) para mostrar al usuario.
        /// </summary>
        public (int generadas, int salteadas) GenerarCuotasMasivas(
            List<string> niveles, string mes, int anio,
            decimal montoBase, string tipoCuota)
        {
            if (niveles == null || niveles.Count == 0)
                throw new ArgumentException("Seleccioná al menos un nivel.");
            if (montoBase <= 0)
                throw new ArgumentException("El monto de la cuota debe ser mayor a cero.");

            // mes llega como nombre ("Julio") → convertir a int para la BD
            int nroMes = MesANumero(mes);
            DateTime fechaVencimiento = new DateTime(anio, nroMes, 15);

            int generadas = 0, salteadas = 0;

            using (MySqlConnection conexion = conexionBD.ObtenerConexion())
            {
                conexion.Open();

                var paramNames = new List<string>();
                for (int i = 0; i < niveles.Count; i++)
                    paramNames.Add("@niv" + i);

                string sqlAlumnos = $@"
                    SELECT id, nombre, nivel, descuento
                    FROM alumnos
                    WHERE Activo = 1
                      AND nivel IN ({string.Join(",", paramNames)})
                      AND (anioIngreso IS NULL OR anioIngreso <= @anio)
                    ORDER BY nombre";

                var alumnos = new List<(int id, string nombre, string nivel, int descuento)>();
                using (MySqlCommand cmd = new MySqlCommand(sqlAlumnos, conexion))
                {
                    for (int i = 0; i < niveles.Count; i++)
                        cmd.Parameters.AddWithValue("@niv" + i, niveles[i]);
                    cmd.Parameters.AddWithValue("@anio", anio);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                            alumnos.Add((
                                Convert.ToInt32(r["id"]),
                                r["nombre"].ToString(),
                                r["nivel"].ToString(),
                                Convert.ToInt32(r["descuento"])
                            ));
                }

                foreach (var alumno in alumnos)
                {
                    // mes es INT en la BD — comparar con número
                    string sqlCheck = @"
                        SELECT COUNT(*) FROM cuotas
                        WHERE alumnoId = @alumnoId AND mes = @mes AND anio = @anio AND tipoCuota = @tipoCuota
                          AND (conceptoId IS NULL OR conceptoId = 0)";

                    using (MySqlCommand chk = new MySqlCommand(sqlCheck, conexion))
                    {
                        chk.Parameters.AddWithValue("@alumnoId", alumno.id);
                        chk.Parameters.AddWithValue("@mes", nroMes);   // INT
                        chk.Parameters.AddWithValue("@anio", anio);
                        chk.Parameters.AddWithValue("@tipoCuota", tipoCuota);
                        int existe = Convert.ToInt32(chk.ExecuteScalar());
                        if (existe > 0) { salteadas++; continue; }
                    }

                    //decimal descImporte = Math.Round(montoBase * alumno.descuento / 100m, 2);
                    //decimal montoFinal = montoBase - descImporte;



                    string sqlInsert = @"
                        INSERT INTO cuotas
                            (alumnoId, mes, anio, tipoCuota, montoOriginal, descuento,
                             montoPagado,  estado, fechaVencimiento, conceptoId)
                        VALUES
                            (@alumnoId, @mes, @anio, @tipoCuota, @montoOriginal, @descuento,
                              0, 'Pendiente', @fechaVencimiento, NULL)";

                    using (MySqlCommand ins = new MySqlCommand(sqlInsert, conexion))
                    {
                        ins.Parameters.AddWithValue("@alumnoId", alumno.id);
                        ins.Parameters.AddWithValue("@mes", nroMes);   // INT
                        ins.Parameters.AddWithValue("@anio", anio);
                        ins.Parameters.AddWithValue("@tipoCuota", tipoCuota);
                        ins.Parameters.AddWithValue("@montoOriginal", montoBase);
                        ins.Parameters.AddWithValue("@descuento", alumno.descuento);
                        //ins.Parameters.AddWithValue("@montoFinal", montoFinal);
                        ins.Parameters.AddWithValue("@fechaVencimiento", fechaVencimiento);
                        ins.ExecuteNonQuery();
                    }

                    AplicarRecargosMora(); // aplicar recargo a las cuotas vencidas antes de generar nuevas, para evitar que se acumulen sin control
                    generadas++;
                }
            }
            return (generadas, salteadas);
        }
        // ==================== RECARGO POR VENCIMIENTO ====================

        /// <summary>
        /// Regla: a partir del día 15 del mes de vencimiento se aplica 4% de recargo.
        /// Por cada mes adicional de mora se suma otro 4% acumulativo.
        /// El recargo se aplica sobre el saldo pendiente (que ya tiene descontado
        /// cualquier descuento del alumno — nunca sobre el monto original sin descuento).
        /// Ejemplo: 1 mes vencido → 4%; 2 meses → 8%; 3 meses → 12% ...
        /// </summary>
        public static decimal CalcularSaldoConRecargo(Cuota cuota, DateTime? hoy = null)
        {
            if (cuota.Saldo <= 0) return 0m;                   // ya pagada
            if (!cuota.FechaVencimiento.HasValue) return cuota.Saldo; // sin fecha, sin recargo

            DateTime fechaHoy = (hoy ?? DateTime.Today).Date;
            DateTime vencimiento = cuota.FechaVencimiento.Value.Date;

            if (fechaHoy <= vencimiento)
                return cuota.Saldo;

            int meses = ((fechaHoy.Year - vencimiento.Year) * 12)
                      + (fechaHoy.Month - vencimiento.Month);

            meses++; // el primer mes empieza apenas vence

            decimal porcentaje = meses * 0.04m;

            return Math.Round(
                cuota.Saldo * (1 + porcentaje),
                2);
        }
        /// <summary>
        /// Aplica y persiste el recargo de mora en todas las cuotas vencidas pendientes.
        /// Llamar una vez al iniciar la aplicación o desde un proceso de cierre diario.
        /// Actualiza el campo 'saldo' y el estado a 'Vencida' cuando corresponde.
        /// </summary>
        public int AplicarRecargosMora(bool forzar = false)
        {
            int actualizadas = 0;
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();

                    using (MySqlCommand crearTabla = new MySqlCommand(@"
                            CREATE TABLE IF NOT EXISTS parametros_sistema (
                                parametro VARCHAR(100) PRIMARY KEY,
                                valor VARCHAR(255)
                            )", conexion))
                    {
                        crearTabla.ExecuteNonQuery();
                    }

                    // Excepción puntual: mes/año en que NO se debe aplicar recargo por
                    // mora (ej. el mes de un aumento de cuota, para no mezclar recargo
                    // con la diferencia retroactiva del nuevo monto). Se configura con
                    // una fila en parametros_sistema — ver Database/excepcion_recargo.sql.
                    // Si no hay ninguna cargada, no excluye nada (comportamiento normal).
                    int? mesSinRecargo = null, anioSinRecargo = null;
                    using (MySqlCommand selExcepcion = new MySqlCommand(
                        "SELECT parametro, valor FROM parametros_sistema WHERE parametro IN ('mes_sin_recargo','anio_sin_recargo')",
                        conexion))
                    using (MySqlDataReader rdExc = selExcepcion.ExecuteReader())
                    {
                        while (rdExc.Read())
                        {
                            string clave = rdExc["parametro"].ToString();
                            if (int.TryParse(rdExc["valor"].ToString(), out int valorInt))
                            {
                                if (clave == "mes_sin_recargo") mesSinRecargo = valorInt;
                                else if (clave == "anio_sin_recargo") anioSinRecargo = valorInt;
                            }
                        }
                    }

                    string hoyStr = DateTime.Today.ToString("yyyy-MM-dd");

                    if (!forzar)
                    {
                        string ultimaEjecucion = null;
                        using (MySqlCommand sel = new MySqlCommand(
                            "SELECT valor FROM parametros_sistema WHERE parametro = 'ultima_ejecucion_recargos_mora'",
                            conexion))
                        using (MySqlDataReader rd = sel.ExecuteReader())
                        {
                            if (rd.Read()) ultimaEjecucion = rd["valor"].ToString();
                        }

                        if (ultimaEjecucion == hoyStr)
                            return 0; // ya se procesaron los recargos hoy
                    }

                    using (MySqlCommand ins = new MySqlCommand(
                        "INSERT INTO parametros_sistema (parametro, valor) VALUES ('ultima_ejecucion_recargos_mora', @hoy)",
                        conexion))
                    {
                        ins.Parameters.AddWithValue("@hoy", hoyStr);
                        string sqlPendientes = @"
                        SELECT id, saldo,montoOriginal, montoFinal, montoPagado, fechaVencimiento, estado, recargo, descuento
                        FROM cuotas
                        WHERE saldo > 0 AND fechaVencimiento IS NOT NULL
                          AND fechaVencimiento < CURDATE()
                          AND estado NOT IN ('Pagado') ";

                        var cuotas = new List<Cuota>();
                        using (MySqlCommand cmd = new MySqlCommand(sqlPendientes, conexion))
                        using (MySqlDataReader r = cmd.ExecuteReader())
                            while (r.Read())
                                cuotas.Add(new Cuota
                                {
                                    Id = Convert.ToInt32(r["id"]),
                                    Saldo = Convert.ToDecimal(r["saldo"]),
                                    MontoOriginal = Convert.ToDecimal(r["montoOriginal"]),
                                    MontoFinal = Convert.ToDecimal(r["montoFinal"]),
                                    MontoPagado = Convert.ToDecimal(r["montoPagado"]),
                                    FechaVencimiento = Convert.ToDateTime(r["fechaVencimiento"]),
                                    Estado = r["estado"].ToString(),
                                    Recargo = Convert.ToDecimal(r["recargo"]),
                                    Descuento = Convert.ToDecimal(r["descuento"]) / 100
                                });

                        foreach (var cuota in cuotas)
                        {
                            // Si ya se registró algún pago (parcial o total) sobre esta cuota,
                            // el recargo queda CONGELADO en lo que ya se calculó — no se
                            // le vuelve a tocar, aunque siga sin terminar de pagarse.
                            // Antes esto se controlaba mirando si "cuota.Recargo > 0", pero
                            // eso tiene un problema real: una cuota que nunca se tocó (sin
                            // pagos, recargo todavía en 0) se queda sin actualizar el primer
                            // mes que corresponde. Lo correcto es mirar si YA HUBO UN PAGO
                            // (MontoPagado > 0), no si ya tiene un recargo cargado — así una
                            // cuota 100% impaga sigue escalando 4% por mes normalmente, y
                            // sólo se congela una vez que la familia empezó a pagarla.
                            if (cuota.MontoPagado > 0)
                                continue;

                            if (mesSinRecargo.HasValue && anioSinRecargo.HasValue
                                && cuota.FechaVencimiento.Value.Month == mesSinRecargo.Value
                                && cuota.FechaVencimiento.Value.Year == anioSinRecargo.Value)
                                continue; // mes exceptuado a propósito (ver excepcion_recargo.sql)

                            DateTime hoy = DateTime.Today;

                            int meses = ((hoy.Year - cuota.FechaVencimiento.Value.Year) * 12)
                                      + (hoy.Month - cuota.FechaVencimiento.Value.Month);

                            if (hoy.Day >= 15)
                                meses++;

                            if (meses <= 0)
                                continue;

                            decimal montoFinal = 0;
                            if (cuota.Descuento != 0)
                            {
                                montoFinal = cuota.MontoOriginal * (1 - cuota.Descuento);
                            }
                            else
                            {
                                montoFinal = cuota.MontoOriginal;
                            }

                            decimal recargo = Math.Round(montoFinal * (meses * 0.04m), 2);

                            decimal nuevoSaldo =
                            cuota.MontoFinal +
                            recargo -
                            cuota.MontoPagado;

                            string nuevoEstado = EstadosCuota.Vencida;

                            using (MySqlCommand upd = new MySqlCommand(
                                @"UPDATE cuotas
                              SET recargo = @recargo,
                                  estado  = @estado
                              WHERE id = @id",
                                conexion))
                            {
                                upd.Parameters.AddWithValue("@recargo", recargo);
                                upd.Parameters.AddWithValue("@estado", nuevoEstado);
                                upd.Parameters.AddWithValue("@id", cuota.Id);
                                upd.ExecuteNonQuery();
                            }

                            actualizadas++;
                        }

                        // Dejar registrado que hoy ya se procesaron los recargos,
                        // para que la próxima vez que se abra Pagos (hoy) no se vuelva a hacer.
                        using (MySqlCommand marcar = new MySqlCommand(@"
                    INSERT INTO parametros_sistema (parametro, valor)
                    VALUES ('ultima_ejecucion_recargos_mora', @fecha)
                    ON DUPLICATE KEY UPDATE valor = @fecha",
                            conexion))
                        {
                            marcar.Parameters.AddWithValue("@fecha", hoyStr);
                            marcar.ExecuteNonQuery();
                        }
                    }
                } 
            }
            
            catch (Exception ex) { throw new Exception("Error al aplicar recargos: " + ex.Message); }
            return actualizadas;
        }
        // ── Helpers ──────────────────────────────────────────────
        private static int MesANumero(string mes)
        {
            if (string.IsNullOrEmpty(mes)) return DateTime.Now.Month;
            // Si ya es un número, devolverlo directamente
            if (int.TryParse(mes.Trim(), out int n) && n >= 1 && n <= 12) return n;
            switch (mes.Trim().ToLower()
                .Replace("á", "a").Replace("é", "e").Replace("í", "i")
                .Replace("ó", "o").Replace("ú", "u"))
            {
                case "enero": return 1;
                case "febrero": return 2;
                case "marzo": return 3;
                case "abril": return 4;
                case "mayo": return 5;
                case "junio": return 6;
                case "julio": return 7;
                case "agosto": return 8;
                case "septiembre": return 9;
                case "octubre": return 10;
                case "noviembre": return 11;
                case "diciembre": return 12;
                default: return DateTime.Now.Month;
            }
        }
        public void AjustarSaldoCuota(int cuotaId, decimal nuevoSaldo, string motivo = "Ajuste saldo inicial")
        {
            if (nuevoSaldo < 0)
                throw new ArgumentException("El saldo no puede ser negativo.");

            using (MySqlConnection conexion = conexionBD.ObtenerConexion())
            {
                conexion.Open();

                // Leer la cuota actual
                string sqlLeer = "SELECT montoFinal, saldo, montoPagado FROM cuotas WHERE id = @id";
                decimal montoFinal = 0, saldoActual = 0;
                using (MySqlCommand cmd = new MySqlCommand(sqlLeer, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", cuotaId);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) throw new Exception("Cuota no encontrada.");
                        montoFinal = Convert.ToDecimal(r["montoFinal"]);
                        saldoActual = Convert.ToDecimal(r["saldo"]);
                    }
                }

                if (nuevoSaldo > montoFinal)
                    throw new ArgumentException(
                        $"El saldo ({nuevoSaldo:N2}) no puede ser mayor al monto de la cuota ({montoFinal:N2}).");

                decimal montoPagadoNuevo = montoFinal - nuevoSaldo;
                string nuevoEstado = nuevoSaldo <= 0 ? EstadosCuota.Pagado
                                   : montoPagadoNuevo > 0 ? EstadosCuota.PagoParcial
                                   : EstadosCuota.Pendiente;

                string sqlUpdate = @"
                    UPDATE cuotas SET
                        montoPagado = @montoPagado,
                        estado      = @estado
                    WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sqlUpdate, conexion))
                {
                    cmd.Parameters.AddWithValue("@montoPagado", montoPagadoNuevo);
                    cmd.Parameters.AddWithValue("@estado", nuevoEstado);
                    cmd.Parameters.AddWithValue("@id", cuotaId);
                    cmd.ExecuteNonQuery();
                }
            }

            new LogService().Registrar("Ajuste manual de saldo",
                $"Cuota #{cuotaId} → nuevo saldo ${nuevoSaldo:N2} ({motivo})");
        }
        public void AjustarRecargoCuota(int cuotaId, decimal nuevoRecargo)
        {
            if (nuevoRecargo < 0)
                throw new ArgumentException("El recargo no puede ser negativo.");

            using (MySqlConnection conexion = conexionBD.ObtenerConexion())
            {
                conexion.Open();

                string sqlLeer = "SELECT montoFinal, montoPagado FROM cuotas WHERE id = @id";
                decimal montoFinal = 0, montoPagado = 0;
                using (MySqlCommand cmd = new MySqlCommand(sqlLeer, conexion))
                {
                    cmd.Parameters.AddWithValue("@id", cuotaId);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        if (!r.Read()) throw new Exception("Cuota no encontrada.");
                        montoFinal = Convert.ToDecimal(r["montoFinal"]);
                        montoPagado = Convert.ToDecimal(r["montoPagado"]);
                    }
                }

                decimal saldoResultante = montoFinal + nuevoRecargo - montoPagado;
                string nuevoEstado = saldoResultante <= 0 ? EstadosCuota.Pagado
                                   : montoPagado > 0 ? EstadosCuota.PagoParcial
                                   : EstadosCuota.Pendiente;

                string sqlUpdate = @"
            UPDATE cuotas SET
                recargo = @recargo,
                estado  = @estado
            WHERE id = @id";

                using (MySqlCommand cmd = new MySqlCommand(sqlUpdate, conexion))
                {
                    cmd.Parameters.AddWithValue("@recargo", nuevoRecargo);
                    cmd.Parameters.AddWithValue("@estado", nuevoEstado);
                    cmd.Parameters.AddWithValue("@id", cuotaId);
                    cmd.ExecuteNonQuery();
                }
            }

            new LogService().Registrar("Ajuste manual de recargo",
                $"Cuota #{cuotaId} → nuevo recargo ${nuevoRecargo:N2}");
        }
    }
}
