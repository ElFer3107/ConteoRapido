using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.ViewModels.Reportes
{
    public class GeneralAsistencia
    {
        public int COD_PROVINCIA { get; set; }
        public string NOM_PROVINCIA { get; set; }
        public int TRANSMITIDAS { get; set; }
        public int PENDIENTES { get; set; }
    }
}
