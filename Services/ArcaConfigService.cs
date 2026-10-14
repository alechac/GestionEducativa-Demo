using GestionEducativa.Utils;
using GestionEducativa.Database;
using GestionEducativa.Models;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionEducativa.Services
{
        public class ArcaConfigService
        {
            private readonly ConexionBD _conexionBD;

            public ArcaConfigService()
            {
                _conexionBD = new ConexionBD();
            }

            // ══════════════════════════════════════════════════════════════════════
            //  Métodos internos: GetConfiguracion / SetConfiguracion
            // ══════════════════════════════════════════════════════════════════════

            private string GetConfiguracion(string clave)
            {
                try
                {
                    using (var conexion = _conexionBD.ObtenerConexion())
                    {
                        conexion.Open();
                        string sql = "SELECT valor FROM configuracion WHERE clave = @clave LIMIT 1";
                        using (var cmd = new MySqlCommand(sql, conexion))
                        {
                            cmd.Parameters.AddWithValue("@clave", clave);
                            var resultado = cmd.ExecuteScalar();
                            return resultado?.ToString() ?? "";
                        }
                    }
                }
                catch
                {
                    return "";
                }
            }

            private void SetConfiguracion(string clave, string valor)
            {
                using (var conexion = _conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    // INSERT si no existe, UPDATE si ya existe (upsert)
                    string sql = @"INSERT INTO configuracion (clave, valor)
                               VALUES (@clave, @valor)
                               ON DUPLICATE KEY UPDATE valor = @valor";
                    using (var cmd = new MySqlCommand(sql, conexion))
                    {
                        cmd.Parameters.AddWithValue("@clave", clave);
                        cmd.Parameters.AddWithValue("@valor", valor);
                        cmd.ExecuteNonQuery();
                    }
                }
            }

            // ══════════════════════════════════════════════════════════════════════
            //  API pública
            // ══════════════════════════════════════════════════════════════════════

            /// <summary>
            /// La contraseña del certificado (.pfx) puede estar cifrada con DPAPI
            /// (CifradoHelper) o en texto plano (si quedó guardada así de antes de este
            /// cambio) — igual que ya se maneja la contraseña de conexión a MySQL en
            /// ConexionBD.CargarCadenaConexion. La próxima vez que se guarde, queda cifrada.
            /// </summary>
            private static string DescifrarSiCorresponde(string valor)
            {
                return CifradoHelper.EstaCifrado(valor) ? CifradoHelper.Descifrar(valor) : valor;
            }

            public ArcaConfig Cargar()
            {
                return new ArcaConfig
                {
                    CUIT = GetConfiguracion("Arca_CUIT"),
                    RazonSocial = GetConfiguracion("Arca_RazonSocial"),
                    DomicilioComercial = "Congreso de Tucumán 273",
                    IngresosBrutos = GetConfiguracion("Arca_IngresosBrutos"),
                    FechaInicioActividades = GetConfiguracion("Arca_FechaInicioActividades"),
                    PuntoVenta = int.TryParse(GetConfiguracion("Arca_PuntoVenta"), out int pv) ? pv : 1,
                    RutaPfx = GetConfiguracion("Arca_RutaPfx"),
                    PasswordPfx = DescifrarSiCorresponde(GetConfiguracion("Arca_PasswordPfx")),
                    Produccion = GetConfiguracion("Arca_Produccion") == "1"
                };
            }

            public void Guardar(ArcaConfig config)
            {
            SetConfiguracion("Arca_CUIT", config.CUIT ?? "");
            SetConfiguracion("Arca_RazonSocial", config.RazonSocial ?? "");
            SetConfiguracion("Arca_DomicilioComercial", config.DomicilioComercial ?? "");
            SetConfiguracion("Arca_IngresosBrutos", config.IngresosBrutos ?? "");
            SetConfiguracion("Arca_FechaInicioActividades", config.FechaInicioActividades ?? "");
            SetConfiguracion("Arca_PuntoVenta", config.PuntoVenta.ToString());
            SetConfiguracion("Arca_RutaPfx", config.RutaPfx ?? "");
            SetConfiguracion("Arca_PasswordPfx", CifradoHelper.Cifrar(config.PasswordPfx ?? ""));
            SetConfiguracion("Arca_Produccion", config.Produccion ? "1" : "0");
        }

            /// <summary>
            /// Valida que la configuración tenga los campos mínimos para operar.
            /// Lanza excepción descriptiva si falta algo.
            /// </summary>
            public void Validar(ArcaConfig config)
            {
                if (string.IsNullOrWhiteSpace(config.CUIT))
                    throw new Exception("Falta el CUIT del emisor en la configuración ARCA.");

                if (string.IsNullOrWhiteSpace(config.RutaPfx))
                    throw new Exception("Falta la ruta del certificado (.pfx) en la configuración ARCA.");

                if (!System.IO.File.Exists(config.RutaPfx))
                    throw new Exception($"No se encontró el certificado en:\n{config.RutaPfx}\n\nVerificá la ruta en Configuración → ARCA.");

                if (config.PuntoVenta <= 0)
                    throw new Exception("El punto de venta debe ser mayor a 0.");
            }
        }
    }
