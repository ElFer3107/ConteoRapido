using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.ViewModels.Reportes
{
    public class InformacionGeneral
    {
        public int COD_PROVINCIA { get; set; }
        public int COD_CANTON { get; set; }
        public int COD_PARROQUIA { get; set; }
        public string NOM_PROVINCIA { get; set; }
        public string NOM_CANTON { get; set; }
        public string NOM_PARROQUIA { get; set; }
        public string NOM_ZONA { get; set; }
        public int JUNTAS { get; set; }
        public int ASIGNADAS { get; set; }
        public int DESCARGADAS { get; set; }
        public int REPORTADAS { get; set; }
        public int TRANSMITIDAS { get; set; }
        public string SEGUROCOD { get; set; }
    }
}
