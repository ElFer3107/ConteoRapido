using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.ViewModels.Reportes
{
    public class AOperadoresProvincia
    {
        [DisplayName("CODIGO DE PROVINCIA")]
        public int COD_PROV { get; set; }
        public string PROVINCIA { get; set; }
        public int JUNTAS { get; set; }
        public int OPERADORES { get; set; }
        [NotMapped]
        public string SEGUROP { get; set; }
    }
}
