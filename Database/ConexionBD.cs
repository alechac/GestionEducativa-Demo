using GestionEducativa.Utils;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GestionEducativa.Database
{
    public class ConexionBD
    {
        private readonly string _connectionString;

        public ConexionBD()
        {
            _connectionString = CargarCadenaConexion();
        }

        private string CargarCadenaConexion()
        {
            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
                return null;
            var cs = ConfigurationManager.ConnectionStrings["GestionEducativa"];
            if (cs == null || string.IsNullOrWhiteSpace(cs.ConnectionString))
                throw new InvalidOperationException(
                    "No se encontró la cadena de conexión 'GestionEducativa' en App.config.");

            // La contraseña puede estar cifrada con DPAPI o en texto plano
            // (texto plano solo ocurre la primera vez, antes de pasar por el formulario)
            var builder = new MySqlConnectionStringBuilder(cs.ConnectionString);
            if (CifradoHelper.EstaCifrado(builder.Password))
                builder.Password = CifradoHelper.Descifrar(builder.Password);

            return builder.ConnectionString;
        }

        public string ConnectionString => _connectionString;

        public MySqlConnection ObtenerConexion()
        {
            try
            {
                return new MySqlConnection(_connectionString);
            }
            catch (Exception ex)
            {
                throw new Exception("Error al crear la conexión: " + ex.Message);
            }
        }

        public bool ProbarConexion()
        {
            try
            {
                using (var conexion = ObtenerConexion())
                {
                    conexion.Open();
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}

