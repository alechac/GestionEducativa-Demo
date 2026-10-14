using System;
using GestionEducativa.Database;
using GestionEducativa.Models;
using MySql.Data.MySqlClient;

namespace GestionEducativa.Services
{
    public class ComprobanteService
    {
        private readonly ConexionBD conexionBD;

        public ComprobanteService()
        {
            conexionBD = new ConexionBD();
        }

        public int ObtenerProximoNumero()
        {
            try
            {
                using (MySqlConnection conexion = conexionBD.ObtenerConexion())
                {
                    conexion.Open();
                    string consulta = "SELECT COALESCE(MAX(id), 0) + 1 FROM pagos";
                    using (MySqlCommand cmd = new MySqlCommand(consulta, conexion))
                        return Convert.ToInt32(cmd.ExecuteScalar());
                }
            }
            catch
            {
                return 1;
            }
        }
    }
}
