using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.ViewModels.Reportes
{
    public class ATransmitidasParroquias
    {
        [DisplayName("CODIGO DE CANTON")]
        public int CCANTON { get; set; }
        [DisplayName("CODIGO DE PARROQUIA")]
        public int CODIGO { get; set; }
        public string PROVINCIA { get; set; }
        public string CANTON { get; set; }
        public string PARROQUIA { get; set; }
        public int JUNTAS { get; set; }
        public int TRANSMITIDAS { get; set; }
        [NotMapped]
        public string SEGUROCOD { get; set; }
    }
}
