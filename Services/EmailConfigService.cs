using GestionEducativa.Database;
using GestionEducativa.Models;
using GestionEducativa.Utils;
using MySql.Data.MySqlClient;
using System;

namespace GestionEducativa.Services
{
    /// <summary>
    /// Persiste la configuración SMTP en la misma tabla genérica "configuracion"
    /// que ya usa ArcaConfigService (clave/valor), bajo el prefijo "Email_".
    /// </summary>
    public class EmailConfigService
    {
        private readonly ConexionBD _conexionBD;

        public EmailConfigService()
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
        /// La contraseña SMTP puede estar cifrada con DPAPI (CifradoHelper) o en texto
        /// plano (si quedó guardada así de antes de este cambio) — igual que ya se
        /// maneja la contraseña de conexión a MySQL en ConexionBD.CargarCadenaConexion.
        /// La próxima vez que se guarde, queda cifrada.
        /// </summary>
        private static string DescifrarSiCorresponde(string valor)
        {
            return CifradoHelper.EstaCifrado(valor) ? CifradoHelper.Descifrar(valor) : valor;
        }

        public EmailConfig Cargar()
        {            return new EmailConfig
            {
                Habilitado = GetConfiguracion("Email_Habilitado") == "1",
                SmtpHost = GetConfiguracion("Email_SmtpHost"),
                SmtpPuerto = int.TryParse(GetConfiguracion("Email_SmtpPuerto"), out int p) ? p : 587,
                SmtpUsuario = GetConfiguracion("Email_SmtpUsuario"),
                SmtpPassword = DescifrarSiCorresponde(GetConfiguracion("Email_SmtpPassword")),
                UsarSsl = GetConfiguracion("Email_UsarSsl") != "0", // por defecto true
                RemitenteEmail = GetConfiguracion("Email_RemitenteEmail"),
                RemitenteNombre = GetConfiguracion("Email_RemitenteNombre"),
                CopiaOculta = GetConfiguracion("Email_CopiaOculta")
            };
        }

        public void Guardar(EmailConfig config)
        {
            SetConfiguracion("Email_Habilitado", config.Habilitado ? "1" : "0");
            SetConfiguracion("Email_SmtpHost", config.SmtpHost ?? "");
            SetConfiguracion("Email_SmtpPuerto", config.SmtpPuerto.ToString());
            SetConfiguracion("Email_SmtpUsuario", config.SmtpUsuario ?? "");
            SetConfiguracion("Email_SmtpPassword", CifradoHelper.Cifrar(config.SmtpPassword ?? ""));
            SetConfiguracion("Email_UsarSsl", config.UsarSsl ? "1" : "0");
            SetConfiguracion("Email_RemitenteEmail", config.RemitenteEmail ?? "");
            SetConfiguracion("Email_RemitenteNombre", config.RemitenteNombre ?? "");
            SetConfiguracion("Email_CopiaOculta", config.CopiaOculta ?? "");
        }

        /// <summary>
        /// Valida que la configuración tenga los campos mínimos para poder enviar mails.
        /// Lanza excepción descriptiva si falta algo.
        /// </summary>
        public void Validar(EmailConfig config)
        {
            if (string.IsNullOrWhiteSpace(config.SmtpHost))
                throw new Exception("Falta el servidor SMTP en la configuración de Email.");

            if (string.IsNullOrWhiteSpace(config.RemitenteEmail))
                throw new Exception("Falta el email remitente en la configuración de Email.");

            if (config.SmtpPuerto <= 0)
                throw new Exception("El puerto SMTP debe ser mayor a 0.");
        }
    }
}
