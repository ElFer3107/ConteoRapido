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
        public List<int> _CodCandidato { get; set; }
        public List<String> _Candidato { get; set; }
        public List<int> _Orden { get; set; }
        public List<int> _Votos { get; set; }

    }
}
