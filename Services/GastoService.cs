using GestionEducativa.Database;
using GestionEducativa.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace GestionEducativa.Services
{
    public class GastoService
    {
        private readonly ConexionBD conexionBD;

        public GastoService()
        {
            conexionBD = new ConexionBD();
        }

        public int Insertar(Gasto gasto)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = @"
                        INSERT INTO gastos
                            (fecha, tipoGasto, detalle, monto, formaPago, seccion,
                             comprobanteTipo, comprobanteNumero, comprobanteProveedor,
                             comprobanteCuitProveedor, comprobanteFecha, observaciones)
                        VALUES
                            (@fecha, @tipoGasto, @detalle, @monto, @formaPago, @seccion,
                             @comprobanteTipo, @comprobanteNumero, @comprobanteProveedor,
                             @comprobanteCuitProveedor, @comprobanteFecha, @observaciones);
                        SELECT LAST_INSERT_ID();";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        AgregarParametros(cmd, gasto);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al registrar el gasto: " + ex.Message);
            }
        }

        public void Actualizar(Gasto gasto)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = @"
                        UPDATE gastos SET
                            fecha = @fecha, tipoGasto = @tipoGasto, detalle = @detalle,
                            monto = @monto, formaPago = @formaPago, seccion = @seccion,
                            comprobanteTipo = @comprobanteTipo, comprobanteNumero = @comprobanteNumero,
                            comprobanteProveedor = @comprobanteProveedor,
                            comprobanteCuitProveedor = @comprobanteCuitProveedor,
                            comprobanteFecha = @comprobanteFecha, observaciones = @observaciones
                        WHERE id = @id";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id", gasto.Id);
                        AgregarParametros(cmd, gasto);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el gasto: " + ex.Message);
            }
        }

        /// <summary>Borrado lógico — mismo criterio que Alumnos y Padres.</summary>
        public void EliminarLogico(int id)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = "UPDATE gastos SET activo = 0 WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el gasto: " + ex.Message);
            }
        }

        /// <summary>
        /// Busca gastos por rango de fechas (obligatorio), sección, y opcionalmente por
        /// tipo de gasto o texto libre (detalle/proveedor).
        /// </summary>
        public List<Gasto> Buscar(DateTime desde, DateTime hasta, int seccion, string tipoGasto = null, string filtroTexto = null)
        {
            var lista = new List<Gasto>();
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = @"
                        SELECT * FROM gastos
                        WHERE activo = 1
                          AND fecha BETWEEN @desde AND @hasta
                          AND seccion = @seccion";

                    bool tieneTipo = !string.IsNullOrWhiteSpace(tipoGasto);
                    bool tieneTexto = !string.IsNullOrWhiteSpace(filtroTexto);
                    if (tieneTipo) consulta += " AND tipoGasto = @tipoGasto";
                    if (tieneTexto) consulta += " AND (detalle LIKE @texto OR comprobanteProveedor LIKE @texto)";
                    consulta += " ORDER BY fecha DESC, id DESC";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@desde", desde.Date);
                        cmd.Parameters.AddWithValue("@hasta", hasta.Date);
                        cmd.Parameters.AddWithValue("@seccion", seccion);
                        if (tieneTipo) cmd.Parameters.AddWithValue("@tipoGasto", tipoGasto);
                        if (tieneTexto) cmd.Parameters.AddWithValue("@texto", $"%{filtroTexto}%");

                        using (MySqlDataReader r = cmd.ExecuteReader())
                            while (r.Read())
                                lista.Add(Mapear(r));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al buscar gastos: " + ex.Message);
            }
            return lista;
        }

        public decimal TotalGastosDia(DateTime fecha, int? seccion = null, string formaPago = null)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = "SELECT COALESCE(SUM(monto), 0) FROM gastos WHERE activo = 1 AND fecha = @fecha";
                    if (seccion.HasValue) consulta += " AND seccion = @seccion";
                    if (!string.IsNullOrEmpty(formaPago)) consulta += " AND formaPago = @formaPago";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@fecha", fecha.Date);
                        if (seccion.HasValue) cmd.Parameters.AddWithValue("@seccion", seccion.Value);
                        if (!string.IsNullOrEmpty(formaPago)) cmd.Parameters.AddWithValue("@formaPago", formaPago);
                        return Convert.ToDecimal(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al calcular el total de gastos: " + ex.Message);
            }
        }

        /// <summary>Tipos de gasto ya usados alguna vez, para ir enriqueciendo el combo con el tiempo.</summary>
        public List<string> ObtenerTiposExistentes()
        {
            var lista = new List<string>();
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = "SELECT DISTINCT tipoGasto FROM gastos WHERE activo = 1 ORDER BY tipoGasto";
                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    using (MySqlDataReader r = cmd.ExecuteReader())
                        while (r.Read())
                            if (!r.IsDBNull(0)) lista.Add(r.GetString(0));
                }
            }
            catch (Exception ex) { throw new Exception("Error al obtener tipos de gasto: " + ex.Message); }
            return lista;
        }

        private static void AgregarParametros(MySqlCommand cmd, Gasto g)
        {
            cmd.Parameters.AddWithValue("@fecha", g.Fecha.Date);
            cmd.Parameters.AddWithValue("@tipoGasto", g.TipoGasto);
            cmd.Parameters.AddWithValue("@detalle", g.Detalle);
            cmd.Parameters.AddWithValue("@monto", g.Monto);
            cmd.Parameters.AddWithValue("@formaPago", g.FormaPago ?? "Efectivo");
            cmd.Parameters.AddWithValue("@seccion", g.Seccion);
            cmd.Parameters.AddWithValue("@comprobanteTipo", (object)g.ComprobanteTipo ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@comprobanteNumero", (object)g.ComprobanteNumero ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@comprobanteProveedor", (object)g.ComprobanteProveedor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@comprobanteCuitProveedor", (object)g.ComprobanteCuitProveedor ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@comprobanteFecha", g.ComprobanteFecha.HasValue ? (object)g.ComprobanteFecha.Value.Date : DBNull.Value);
            cmd.Parameters.AddWithValue("@observaciones", (object)g.Observaciones ?? DBNull.Value);
        }

        private static Gasto Mapear(MySqlDataReader r)
        {
            return new Gasto
            {
                Id = Convert.ToInt32(r["id"]),
                Fecha = Convert.ToDateTime(r["fecha"]),
                TipoGasto = r["tipoGasto"].ToString(),
                Detalle = r["detalle"].ToString(),
                Monto = Convert.ToDecimal(r["monto"]),
                FormaPago = r["formaPago"] == DBNull.Value ? "Efectivo" : r["formaPago"].ToString(),
                Seccion = Convert.ToInt32(r["seccion"]),
                ComprobanteTipo = r["comprobanteTipo"] == DBNull.Value ? null : r["comprobanteTipo"].ToString(),
                ComprobanteNumero = r["comprobanteNumero"] == DBNull.Value ? null : r["comprobanteNumero"].ToString(),
                ComprobanteProveedor = r["comprobanteProveedor"] == DBNull.Value ? null : r["comprobanteProveedor"].ToString(),
                ComprobanteCuitProveedor = r["comprobanteCuitProveedor"] == DBNull.Value ? null : r["comprobanteCuitProveedor"].ToString(),
                ComprobanteFecha = r["comprobanteFecha"] == DBNull.Value ? (DateTime?)null : Convert.ToDateTime(r["comprobanteFecha"]),
                Observaciones = r["observaciones"] == DBNull.Value ? null : r["observaciones"].ToString(),
                FechaRegistro = Convert.ToDateTime(r["fechaRegistro"]),
                Activo = Convert.ToBoolean(r["activo"])
            };
        }
    }
}
