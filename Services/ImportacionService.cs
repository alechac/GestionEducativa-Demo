using System;
using System.Collections.Generic;
using System.IO;
using GestionEducativa.Models;

namespace GestionEducativa.Services
{
    public class ImportacionService
    {
        public class ResultadoImportacion
        {
            public int Importados { get; set; }
            public int Errores { get; set; }
            public List<string> Mensajes { get; set; } = new List<string>();
        }

            
    }
}
