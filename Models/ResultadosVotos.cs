using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Models
{
    public class ResultadosVotos
    {
        public Acta Acta { get; set; }
        public IEnumerable<Resultado> Resultados { get; set; }
    }
}
