using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace GestionEducativa.Utils
{
    public static class CifradoHelper
    {
        public static string Cifrar(string texto)
        {
            if (string.IsNullOrEmpty(texto)) return "";
            byte[] datos = Encoding.UTF8.GetBytes(texto);
            byte[] cifrado = ProtectedData.Protect(datos, null, DataProtectionScope.CurrentUser);
            return Convert.ToBase64String(cifrado);
        }

        public static string Descifrar(string textoCifrado)
        {
            if (string.IsNullOrEmpty(textoCifrado)) return "";
            byte[] cifrado = Convert.FromBase64String(textoCifrado);
            byte[] datos = ProtectedData.Unprotect(cifrado, null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(datos);
        }

        public static bool EstaCifrado(string valor)
        {
            // Heurística: si es base64 válido y largo, asumimos que está cifrado
            if (string.IsNullOrEmpty(valor) || valor.Length < 20) return false;
            try
            {
                Convert.FromBase64String(valor);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
