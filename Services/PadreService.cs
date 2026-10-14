using GestionEducativa.Database;
using GestionEducativa.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace GestionEducativa.Services
{
    public class PadreService
    {
        private readonly ConexionBD conexionBD;

        public PadreService()
        {
            conexionBD = new ConexionBD();
        }

        /// <summary>
        /// Obtiene los padres asociados a un alumno.
        /// </summary>
        public List<Padre> ObtenerPadresPorAlumno(int alumnoId)
        {
            var padres = new List<Padre>();
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = @"
                        SELECT p.id, p.nombre, p.apellido, p.dni, p.cuit, p.telefono, p.email, p.activo
                        FROM padres p
                        JOIN alumnos_padres ap ON p.id = ap.padreId
                        WHERE ap.alumnoId = @alumnoId AND p.activo = 1
                        ORDER BY p.apellido, p.nombre";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@alumnoId", alumnoId);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                                padres.Add(MapearPadre(r));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener padres del alumno: " + ex.Message);
            }
            return padres;
        }

        /// <summary>
        /// Obtiene los alumnos (hijos/hermanos) asociados a un padre.
        /// </summary>
        public List<Alumno> ObtenerAlumnosPorPadre(int padreId)
        {
            var alumnos = new List<Alumno>();
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = @"
                        SELECT a.id, a.nombre, a.nivel, a.descuento, a.Activo
                        FROM alumnos a
                        JOIN alumnos_padres ap ON a.id = ap.alumnoId
                        WHERE ap.padreId = @padreId AND a.Activo = 1
                        ORDER BY a.nombre";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@padreId", padreId);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                alumnos.Add(new Alumno
                                {
                                    Id = Convert.ToInt32(r["id"]),
                                    Nombre = r["nombre"].ToString(),
                                    Nivel = r["nivel"].ToString(),
                                    PorcentajeDescuento = Convert.ToInt32(r["descuento"]),
                                    Activo = Convert.ToBoolean(r["Activo"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener alumnos del padre: " + ex.Message);
            }
            return alumnos;
        }

        /// <summary>
        /// Busca padres por nombre, apellido, DNI o CUIT. Si el filtro viene vacío, devuelve
        /// todos (igual que ObtenerTodos, pero acá con el mismo método para la pantalla de CRUD).
        /// </summary>
        public List<Padre> Buscar(string filtro)
        {
            var padres = new List<Padre>();
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = @"
                        SELECT id, nombre, apellido, dni, cuit, telefono, email, activo
                        FROM padres
                        WHERE activo = 1
                          AND (@filtro = ''
                               OR nombre LIKE CONCAT('%', @filtro, '%')
                               OR apellido LIKE CONCAT('%', @filtro, '%')
                               OR dni LIKE CONCAT('%', @filtro, '%')
                               OR cuit LIKE CONCAT('%', @filtro, '%'))
                        ORDER BY apellido, nombre";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@filtro", filtro?.Trim() ?? "");
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                                padres.Add(MapearPadre(r));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al buscar padres: " + ex.Message);
            }
            return padres;
        }

        public List<Padre> ObtenerTodos()
        {
            var padres = new List<Padre>();
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = @"
                        SELECT id, nombre, apellido, dni, cuit, telefono, email, activo
                        FROM padres WHERE activo = 1 ORDER BY apellido, nombre";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    using (MySqlDataReader r = cmd.ExecuteReader())
                    {
                        while (r.Read())
                            padres.Add(MapearPadre(r));
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener padres: " + ex.Message);
            }
            return padres;
        }

        public Padre ObtenerPorId(int id)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = "SELECT id, nombre, apellido, dni, cuit, telefono, email, activo FROM padres WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            if (r.Read()) return MapearPadre(r);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener padre: " + ex.Message);
            }
            return null;
        }

        public int Insertar(Padre padre)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = @"
                        INSERT INTO padres (nombre, apellido, dni, cuit, telefono, email, activo)
                        VALUES (@nombre, @apellido, @dni, @cuit, @telefono, @email, 1);
                        SELECT LAST_INSERT_ID();";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@nombre", padre.Nombre);
                        cmd.Parameters.AddWithValue("@apellido", padre.Apellido);
                        cmd.Parameters.AddWithValue("@dni", (object)padre.DNI ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@cuit", (object)padre.CUIT ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@telefono", (object)padre.Telefono ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@email", (object)padre.Email ?? DBNull.Value);
                        return Convert.ToInt32(cmd.ExecuteScalar());
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al insertar padre: " + ex.Message);
            }
        }

        public void Actualizar(Padre padre)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = @"
                        UPDATE padres SET nombre=@nombre, apellido=@apellido, dni=@dni,
                            cuit=@cuit, telefono=@telefono, email=@email
                        WHERE id=@id";

                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id", padre.Id);
                        cmd.Parameters.AddWithValue("@nombre", padre.Nombre);
                        cmd.Parameters.AddWithValue("@apellido", padre.Apellido);
                        cmd.Parameters.AddWithValue("@dni", (object)padre.DNI ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@cuit", (object)padre.CUIT ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@telefono", (object)padre.Telefono ?? DBNull.Value);
                        cmd.Parameters.AddWithValue("@email", (object)padre.Email ?? DBNull.Value);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al actualizar padre: " + ex.Message);
            }
        }

        public void VincularAlumno(int padreId, int alumnoId)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = @"
                        INSERT IGNORE INTO alumnos_padres (padreId, alumnoId)
                        VALUES (@padreId, @alumnoId)";
                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@padreId", padreId);
                        cmd.Parameters.AddWithValue("@alumnoId", alumnoId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al vincular alumno con padre: " + ex.Message);
            }
        }

        public void DesvincularAlumno(int padreId, int alumnoId)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = "DELETE FROM alumnos_padres WHERE padreId=@padreId AND alumnoId=@alumnoId";
                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@padreId", padreId);
                        cmd.Parameters.AddWithValue("@alumnoId", alumnoId);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al desvincular alumno: " + ex.Message);
            }
        }

        /// <summary>
        /// Busca un padre por DNI o CUIL (ignora puntos y guiones). Devuelve null si no existe.
        /// </summary>
        public Padre ObtenerPorDocumento(string documento)
        {
            if (string.IsNullOrWhiteSpace(documento)) return null;
            string doc = documento.Replace(".", "").Replace("-", "").Trim();
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string sql = @"SELECT id, nombre, apellido, dni, cuit, telefono, email, activo
                                   FROM padres
                                   WHERE REPLACE(REPLACE(dni,'.',''),'-','') = @doc
                                      OR REPLACE(REPLACE(cuit,'.',''),'-','') = @doc
                                   LIMIT 1";
                    using (MySqlCommand cmd = new MySqlCommand(sql, conexion))
                    {
                        cmd.Parameters.AddWithValue("@doc", doc);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                            return r.Read() ? MapearPadre(r) : null;
                    }
                }
            }
            catch { return null; }
        }

        /// <summary>
        /// Da de alta un padre ingresado a mano (cuando la búsqueda por DNI/CUIL no encontró
        /// coincidencia en la base) y, si se indica un alumno, lo vincula automáticamente.
        /// Antes de insertar vuelve a chequear por las dudas que no exista ya (evita duplicados
        /// si dos cajeros cargan el mismo padre casi al mismo tiempo).
        /// </summary>
        public Padre GuardarPadreNuevo(string nombreCompleto, string documento, string tipoDocumento, int? alumnoIdParaVincular = null)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto))
                throw new ArgumentException("El nombre del padre/tutor es obligatorio.");
            if (string.IsNullOrWhiteSpace(documento))
                throw new ArgumentException("El DNI o CUIL es obligatorio.");

            var existente = ObtenerPorDocumento(documento);
            if (existente != null)
            {
                if (alumnoIdParaVincular.HasValue)
                    VincularAlumno(existente.Id, alumnoIdParaVincular.Value);
                return existente;
            }

            // Separamos nombre/apellido de forma simple: la última palabra es el apellido.
            // El operador puede después corregirlo desde la ficha del padre si hace falta.
            string texto = nombreCompleto.Trim();
            int idxEspacio = texto.LastIndexOf(' ');
            string nombre = idxEspacio > 0 ? texto.Substring(0, idxEspacio).Trim() : texto;
            string apellido = idxEspacio > 0 ? texto.Substring(idxEspacio + 1).Trim() : "";

            string docLimpio = documento.Replace(".", "").Replace("-", "").Trim();
            var nuevoPadre = new Padre
            {
                Nombre = nombre,
                Apellido = apellido,
                DNI = tipoDocumento == "CUIT" || tipoDocumento == "CUIL" ? null : docLimpio,
                CUIT = tipoDocumento == "CUIT" || tipoDocumento == "CUIL" ? docLimpio : null
            };

            int id = Insertar(nuevoPadre);
            nuevoPadre.Id = id;

            if (alumnoIdParaVincular.HasValue)
                VincularAlumno(id, alumnoIdParaVincular.Value);

            return nuevoPadre;
        }

        /// <summary>
        /// Devuelve los "hermanos" de un alumno: la unión de todos los alumnos que comparten
        /// AL MENOS UN padre/tutor con él. Antes se usaba solo el primer padre cargado para
        /// el alumno, así que si dos hermanos tenían cada uno un tutor distinto (típico en
        /// familias ensambladas: cada hijo con su propio papá, pero la misma mamá) no se
        /// relacionaban entre sí aunque compartieran madre.
        ///
        /// Si se pasa `documentoAdicional` (por ejemplo el DNI/CUIL de quien está pagando en
        /// este momento), también se busca por ese documento y se suma a la unión, aunque ese
        /// padre todavía no esté vinculado a este alumno en particular. Así, si la madre paga
        /// con su propio DNI y ese DNI ya está cargado como tutor de OTRO hijo suyo, el sistema
        /// los relaciona igual sin que haga falta que esté vinculada a los tres.
        /// </summary>
        public List<Alumno> ObtenerHermanos(int alumnoId, string documentoAdicional = null)
        {
            var padreIds = new HashSet<int>();
            foreach (var p in ObtenerPadresPorAlumno(alumnoId))
                padreIds.Add(p.Id);

            if (!string.IsNullOrWhiteSpace(documentoAdicional))
            {
                var padreExtra = ObtenerPorDocumento(documentoAdicional);
                if (padreExtra != null) padreIds.Add(padreExtra.Id);
            }

            var alumnosPorId = new Dictionary<int, Alumno>();
            foreach (int padreId in padreIds)
                foreach (var al in ObtenerAlumnosPorPadre(padreId))
                    alumnosPorId[al.Id] = al;

            var lista = new List<Alumno>(alumnosPorId.Values);
            lista.Sort((a, b) => string.Compare(a.Nombre, b.Nombre, StringComparison.OrdinalIgnoreCase));
            return lista;
        }

        /// <summary>
        /// Borrado lógico (no borra la fila, solo la marca inactiva) — mismo criterio que
        /// AlumnoService.EliminarLogico. Así no se pierde el historial: los comprobantes que
        /// ya se emitieron con los datos de este padre siguen intactos, y si algún día hace
        /// falta se lo puede reactivar desde la base sin perder nada.
        /// </summary>
        public void EliminarLogico(int id)
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = "UPDATE padres SET activo = 0 WHERE id = @id";
                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@id", id);
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al eliminar el padre: " + ex.Message);
            }
        }

        private Padre MapearPadre(MySqlDataReader r)
        {
            return new Padre
            {
                Id = Convert.ToInt32(r["id"]),
                Nombre = r["nombre"].ToString(),
                Apellido = r["apellido"].ToString(),
                DNI = r["dni"] == DBNull.Value ? null : r["dni"].ToString(),
                CUIT = r["cuit"] == DBNull.Value ? null : r["cuit"].ToString(),
                Telefono = r["telefono"] == DBNull.Value ? null : r["telefono"].ToString(),
                Email = r["email"] == DBNull.Value ? null : r["email"].ToString(),
                Activo = Convert.ToBoolean(r["activo"])
            };
        }
    }
}
