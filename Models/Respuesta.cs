using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Models
{
    public class Respuesta : ResultadosVotos
    {
        public int Codigo { get; set; }
        public string Mensaje { get; set; }
        public int codigoResultado { get; set; }
        public string CodigoVerificacion { get; set; }
    }
}
