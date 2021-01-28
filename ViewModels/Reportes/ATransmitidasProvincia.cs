using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.ViewModels.Reportes
{
    public class ATransmitidasProvincia
    {
        [DisplayName("CODIGO DE PROVINCIA")]
        public int CODIGO { get; set; }
        public string PROVINCIA { get; set; }
        public int JUNTAS { get; set; }
        public int TRANSMITIDAS { get; set; }
        [NotMapped]
        public string SEGUROCOD { get; set; }
    }
}
