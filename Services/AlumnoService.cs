using GestionEducativa.Database;
using GestionEducativa.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace GestionEducativa.Services
{
    public class AlumnoService
    {
        private readonly ConexionBD conexionBD;

        public AlumnoService()
        {
            conexionBD = new ConexionBD();
        }

        /// <summary>
        /// Obtiene los alumnos. Por defecto NO incluye a los "preinscriptos" (alumnos cargados
        /// con un anioIngreso futuro, que todavía no empezaron a cursar): así no aparecen
        /// mezclados con los alumnos actuales en los listados ni en la pantalla de Pagos.
        /// Pasar incluirFuturos=true para verlos también (ej. una pantalla de "preinscriptos").
        /// </summary>
        public List<Alumno> ObtenerTodos(int seccion = 1, string nivel = null, string busqueda = null,
            bool incluirFuturos = false, int? anioReferencia = null)
        {
            var alumnos = new List<Alumno>();
            try
            {
                int anioRef = anioReferencia ?? DateTime.Now.Year;

                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = "SELECT id, nivel, nombre, dni, descuento, division, Activo, anioIngreso, seccion FROM alumnos WHERE Activo = 1";

                    bool tieneNivel = !string.IsNullOrEmpty(nivel);
                    bool tieneBusqueda = !string.IsNullOrEmpty(busqueda);

                    // La columna "seccion" es el verdadero diferenciador entre el Colegio y
                    // Scuola Italiano (antes se usaba nivel = 'ITALIANO', pero eso dejó de
                    // alcanzar el día que Scuola Italiano pasó a tener sus propios niveles:
                    // A1, A2, B1, B2, Parliamo). seccion = 0 significa "todas las secciones"
                    // (ej: al vincular un padre, que puede tener hijos en ambas secciones).
                    if (seccion != 0)
                        consulta += " AND seccion = @seccion";

                    if (!incluirFuturos)
                        consulta += " AND (anioIngreso IS NULL OR anioIngreso <= @anioRef)";
                    if (tieneNivel) consulta += " AND nivel = @nivel";
                    if (tieneBusqueda) consulta += " AND nombre LIKE @busqueda";
                    consulta += " ORDER BY nombre";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        if (seccion != 0) cmd.Parameters.AddWithValue("@seccion", seccion);
                        if (!incluirFuturos) cmd.Parameters.AddWithValue("@anioRef", anioRef);
                        if (tieneNivel) cmd.Parameters.AddWithValue("@nivel", nivel);
                        if (tieneBusqueda) cmd.Parameters.AddWithValue("@busqueda", "%" + busqueda + "%");

                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                alumnos.Add(new Alumno
                                {
                                    Id = Convert.ToInt32(r["id"]),
                                    Nombre = r["nombre"].ToString(),
                                    Dni = r["dni"] == DBNull.Value ? null : r["dni"].ToString(),
                                    PorcentajeDescuento = Convert.ToInt32(r["descuento"]),
                                    Nivel = r["nivel"].ToString(),
                                    Division = r["division"] == DBNull.Value ? null : r["division"].ToString(),
                                    Activo = Convert.ToBoolean(r["Activo"]),
                                    AnioIngreso = r["anioIngreso"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["anioIngreso"]),
                                    Seccion = r["seccion"] == DBNull.Value ? 1 : Convert.ToInt32(r["seccion"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener los alumnos: " + ex.Message);
            }
            return alumnos;
        }

        public Alumno ObtenerPorId(int id)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = "SELECT id, nivel, nombre, dni, descuento, Activo, anioIngreso FROM alumnos WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read())
                                return new Alumno
                                {
                                    Id = Convert.ToInt32(r["id"]),
                                    Nombre = r["nombre"].ToString(),
                                    Dni = r["dni"] == DBNull.Value ? null : r["dni"].ToString(),
                                    PorcentajeDescuento = Convert.ToInt32(r["descuento"]),
                                    Nivel = r["nivel"].ToString(),
                                    Activo = Convert.ToBoolean(r["Activo"]),
                                    AnioIngreso = r["anioIngreso"] == DBNull.Value ? (int?)null : Convert.ToInt32(r["anioIngreso"])
                                };
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener el alumno: " + ex.Message);
            }
            return null;
        }

        public void Insertar(Alumno alumno)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = @"INSERT INTO alumnos 
                        (nombre, dni, nivel, division, descuento, fechaNacimiento, Activo, seccion, anioIngreso)
                        VALUES (@nombre, @dni, @nivel, @division, @descuento, @fechaNac, 1, @seccion, @anioIngreso)";
                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@nombre", alumno.Nombre);
                        cmd.Parameters.AddWithValue("@dni", (object)alumno.Dni ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@nivel", alumno.Nivel);
                        cmd.Parameters.AddWithValue("@division", (object)alumno.Division ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@descuento", alumno.PorcentajeDescuento);
                        cmd.Parameters.AddWithValue("@fechaNac", (object)alumno.FechaNacimiento ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@seccion", alumno.Seccion);
                        cmd.Parameters.AddWithValue("@anioIngreso", (object)alumno.AnioIngreso ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al insertar el alumno: " + ex.Message);
            }
        }

        public void Actualizar(Alumno alumno)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    using (MySqlTransaction tx = conexion.BeginTransaction())
                    {
                        // 1. Actualizar datos del alumno
                        string updAlumno = @"UPDATE alumnos SET 
                            nombre = @nombre, dni = @dni, nivel = @nivel, division = @division,
                            descuento = @descuento, fechaNacimiento = @fechaNac, anioIngreso = @anioIngreso
                            WHERE id = @id";
                        using (MySqlCommand cmd = new MySqlCommand(updAlumno, conexion, tx))
                        {
                            cmd.Parameters.AddWithValue("@nombre", alumno.Nombre);
                            cmd.Parameters.AddWithValue("@dni", (object)alumno.Dni ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@nivel", alumno.Nivel);
                            cmd.Parameters.AddWithValue("@division", (object)alumno.Division ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@descuento", alumno.PorcentajeDescuento);
                            cmd.Parameters.AddWithValue("@fechaNac", (object)alumno.FechaNacimiento ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@anioIngreso", (object)alumno.AnioIngreso ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@id", alumno.Id);
                            cmd.ExecuteNonQuery();
                        }

                        // 2. Recalcular descuento y montoFinal en cuotas pendientes (no pagadas)
                        //    saldo = montoFinal - montoPagado (respeta pagos parciales ya realizados)
                        string updCuotas = @"
                            UPDATE cuotas
                            SET descuento    = @pct
                            WHERE alumnoId = @alumnoId
                              AND estado != 'Pagado'";
                        using (MySqlCommand cmd = new MySqlCommand(updCuotas, conexion, tx))
                        {
                            cmd.Parameters.AddWithValue("@pct", alumno.PorcentajeDescuento);
                            cmd.Parameters.AddWithValue("@alumnoId", alumno.Id);
                            cmd.ExecuteNonQuery();
                        }

                        tx.Commit();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar el alumno: " + ex.Message);
            }
        }

        public void EliminarLogico(int id)
        {
            string nombreAlumno = null;
            try { nombreAlumno = ObtenerPorId(id)?.Nombre; } catch { /* si falla, se loguea sólo el id */ }

            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = "UPDATE alumnos SET Activo = 0 WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }

                new LogService().Registrar("Eliminar alumno",
                    nombreAlumno != null ? $"Alumno #{id} — {nombreAlumno}" : $"Alumno #{id}");
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el alumno: " + ex.Message);
            }
        }
    }
}
