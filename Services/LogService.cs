using GestionEducativa.Database;
using GestionEducativa.Models;
using GestionEducativa.Utils;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace GestionEducativa.Services
{
    /// <summary>
    /// Registro simple de auditoría: "quién hizo qué y cuándo" en las operaciones
    /// sensibles (ajustar saldo/recargo a mano, notas de crédito, eliminar un alumno).
    /// No es un sistema de permisos — no bloquea nada, sólo deja constancia.
    /// </summary>
    public class LogService
    {
        private readonly ConexionBD conexionBD;

        public LogService()
        {
            conexionBD = new ConexionBD();
        }

        /// <summary>
        /// Nunca debe interrumpir la operación que se está registrando: si guardar el
        /// log falla (por ej. la tabla no existe todavía porque no se corrió la
        /// migración), sólo se ignora — la acción real ya se hizo y no tiene sentido
        /// bloquearla por un problema del log en sí.
        /// </summary>
        public void Registrar(string accion, string detalle = null)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = @"INSERT INTO logs (fecha, operador, accion, detalle)
                                         VALUES (NOW(), @operador, @accion, @detalle)";
                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@operador", SesionActual.Operador);
                        cmd.Parameters.AddWithValue("@accion", accion);
                        cmd.Parameters.AddWithValue("@detalle", (object)detalle ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Ver comentario de la clase: un log fallido no debe frenar la operación real.
            }
        }

        /// <summary>Para una futura pantalla de "ver historial" — no forma parte del alcance inicial.</summary>
        public List<LogEntrada> ObtenerUltimos(int cantidad = 200)
        {
            var lista = new List<LogEntrada>();
            using (MySqlConnection conexion = conexionBD.ObtenerConexion())
            {
                conexion.Open();
                string consulta = "SELECT id, fecha, operador, accion, detalle FROM logs ORDER BY fecha DESC LIMIT @cantidad";
                using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                {
                    cmd.Parameters.AddWithValue("@cantidad", cantidad);
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                        {
                            lista.Add(new LogEntrada
                            {
                                Id = Convert.ToInt32(r["id"]),
                                Fecha = Convert.ToDateTime(r["fecha"]),
                                Operador = r["operador"].ToString(),
                                Accion = r["accion"].ToString(),
                                Detalle = r["detalle"] == DBNull.Value ? null : r["detalle"].ToString()
                            });
                        }
                    }
                }
            }
            return lista;
        }
    }
}
