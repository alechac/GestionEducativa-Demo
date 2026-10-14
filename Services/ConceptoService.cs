using GestionEducativa.Database;
using GestionEducativa.Models;
using GestionEducativa.Utils;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GestionEducativa.Services
{
    /// <summary>
    /// Catálogo de conceptos (matrícula, seguro, viaje, etc.) y precios de
    /// cuota por nivel. Antes vivía mezclado dentro de PagoService junto con
    /// el cobro de pagos y la generación de cuotas — responsabilidades
    /// distintas que ahora están separadas en sus propias clases.
    /// </summary>
    public class ConceptoService
    {
        private readonly ConexionBD conexionBD;

        public ConceptoService()
        {
            conexionBD = new ConexionBD();
        }

        // ==================== PRECIOS ====================

        public List<PrecioCuota> ObtenerPrecios(int anio)
        {
            var lista = new List<PrecioCuota>();
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = "SELECT * FROM preciosCuota WHERE anio = @anio ORDER BY nivel";
                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                    {
                        cmd.Parameters.AddWithValue("@anio", anio);
                        using (MySqlDataReader r = cmd.ExecuteReader())
                        {
                            while (r.Read())
                            {
                                lista.Add(new PrecioCuota
                                {
                                    Id = Convert.ToInt32(r["id"]),
                                    Nivel = (Nivel)Convert.ToInt32(r["nivel"]),
                                    Seccion = (Seccion)Convert.ToInt32(r["seccion"]),
                                    Anio = Convert.ToInt32(r["anio"]),
                                    Monto = Convert.ToDecimal(r["monto"]),
                                    FechaVigencia = Convert.ToDateTime(r["fechaVigencia"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener precios: " + ex.Message);
            }
            return lista;
        }
        public List<Concepto> ObtenerConceptos(int anio, string nivelAlumno = null, bool esPreinscripto = false)
        {
            var lista = new List<Concepto>();
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();

                    // Conceptos no-matrícula: del nivel actual y año actual
                    string consultaNormal = @"
                SELECT id, nombre, categoria, nivel, monto, anio, descripcion, activo
                FROM conceptos
                WHERE anio = @anio AND activo = 1
                  AND LOWER(categoria) <> 'matricula'
                  AND (nivel IS NULL OR nivel = '' OR LOWER(nivel) = LOWER(@nivel))
                ORDER BY categoria, nombre";

                    using (MySqlCommand cmd = new MySqlCommand(consultaNormal, conexion))
                    {
                        cmd.Parameters.AddWithValue("@anio", anio);
                        cmd.Parameters.AddWithValue("@nivel", nivelAlumno ?? "");
                        using (MySqlDataReader r = cmd.ExecuteReader())
                            while (r.Read())
                                lista.Add(MapearConcepto(r));
                    }

                    // Matrículas:
                    // — Alumno preinscripto (ingresa recién el año que viene): `nivelAlumno` YA es el
                    //   nivel real al que va a entrar, así que se busca la matrícula de ESE nivel
                    //   directamente (no tiene sentido ofrecerle también la de "este año").
                    // — Alumno que ya está cursando: se ofrecen las DOS matrículas posibles —
                    //   la de su nivel actual (por si todavía no la pagó este año) y la del nivel
                    //   siguiente (para el año que viene) — así el colegio genera la que
                    //   corresponda en cada caso, en vez de asumir siempre "la del año que viene".
                    if (!string.IsNullOrEmpty(nivelAlumno))
                    {
                        if (esPreinscripto)
                        {
                            AgregarMatricula(conexion, lista, anio, nivelAlumno);
                        }
                        else
                        {
                            AgregarMatricula(conexion, lista, anio, nivelAlumno); // este año (nivel actual)

                            string nivelSiguiente = NivelUtils.ObtenerNivelSiguiente(nivelAlumno);
                            if (nivelSiguiente != null)
                                AgregarMatricula(conexion, lista, anio + 1, nivelSiguiente); // año que viene — catálogo del año siguiente
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al obtener conceptos: " + ex.Message);
            }
            return lista;
        }

        /// <summary>
        /// Busca la matrícula (categoria = 'matricula') de un nivel/año puntual y la agrega
        /// a la lista si existe y todavía no está (evita duplicados si, por ej., el nivel
        /// actual y el siguiente terminaran apuntando al mismo concepto).
        /// </summary>
        private void AgregarMatricula(MySqlConnection conexion, List<Concepto> lista, int anio, string nivel)
        {
            string consultaMatricula = @"
                SELECT id, nombre, categoria, nivel, monto, anio, descripcion, activo
                FROM conceptos
                WHERE anio = @anio AND activo = 1
                  AND LOWER(categoria) = 'matricula'
                  AND (nivel IS NULL OR nivel = '' OR LOWER(nivel) = LOWER(@nivel))
                ORDER BY nombre";

            using (MySqlCommand cmd = new MySqlCommand(consultaMatricula, conexion))
            {
                cmd.Parameters.AddWithValue("@anio", anio);
                cmd.Parameters.AddWithValue("@nivel", nivel);
                using (MySqlDataReader r = cmd.ExecuteReader())
                {
                    while (r.Read())
                    {
                        var concepto = MapearConcepto(r);
                        if (!lista.Any(c => c.Id == concepto.Id))
                            lista.Add(concepto);
                    }
                }
            }
        }
        // Mapear helper 
        private Concepto MapearConcepto(MySqlDataReader r)
        {
            return new Concepto
            {
                Id = Convert.ToInt32(r["id"]),
                Nombre = r["nombre"].ToString(),
                Categoria = r["categoria"].ToString(),
                Nivel = r["nivel"] == DBNull.Value ? null : r["nivel"].ToString(),
                Monto = Convert.ToDecimal(r["monto"]),
                Anio = Convert.ToInt32(r["anio"]),
                Descripcion = r["descripcion"] == DBNull.Value ? null : r["descripcion"].ToString(),
                Activo = Convert.ToBoolean(r["activo"])
            };

        }
        /// <summary>
        /// Crea un nuevo concepto en el catálogo de conceptos.
        /// Si nivel es null, el concepto aplica a todos los niveles.
        /// Devuelve el concepto con el Id asignado por la BD.
        /// </summary>
        public Concepto CrearConcepto(string nombre, string categoria, string nivel, decimal monto, int anio, string descripcion = null)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del concepto no puede estar vacío.");
            if (anio < 2020 || anio > 2099)
                throw new ArgumentException("El año es inválido.");

            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();

                    string insert = @"
                INSERT INTO conceptos (nombre, categoria, nivel, monto, anio, descripcion, activo)
                VALUES (@nombre, @categoria, @nivel, @monto, @anio, @descripcion, 1);
                SELECT LAST_INSERT_ID();";

                    using (MySqlCommand cmd = new MySqlCommand(insert, conexion))
                    {
                        cmd.Parameters.AddWithValue("@nombre", nombre.Trim());
                        cmd.Parameters.AddWithValue("@categoria", (categoria ?? "Otro").Trim());
                        cmd.Parameters.AddWithValue("@nivel", string.IsNullOrWhiteSpace(nivel) ? (object)DBNull.Value : nivel.Trim());
                        cmd.Parameters.AddWithValue("@monto", monto);
                        cmd.Parameters.AddWithValue("@anio", anio);
                        cmd.Parameters.AddWithValue("@descripcion", string.IsNullOrWhiteSpace(descripcion) ? (object)DBNull.Value : descripcion.Trim());

                        int newId = Convert.ToInt32(cmd.ExecuteScalar());

                        return new Concepto
                        {
                            Id = newId,
                            Nombre = nombre.Trim(),
                            Categoria = (categoria ?? "Otro").Trim(),
                            Nivel = string.IsNullOrWhiteSpace(nivel) ? null : nivel.Trim(),
                            Monto = monto,
                            Anio = anio,
                            Descripcion = descripcion?.Trim(),
                            Activo = true
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear el concepto: " + ex.Message);
            }
        }
    }
}
