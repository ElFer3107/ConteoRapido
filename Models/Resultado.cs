using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Models
{
    public class Resultado
    {
        public int Cod_Candidato { get; set; }
        public string Candidato { get; set; }
        public int Orden { get; set; }
        [RegularExpression("(^[0-9]+$)", ErrorMessage = "Solo se permiten números")]
        public int VOTOS { get; set; }
    }
}
