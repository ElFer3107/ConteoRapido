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
        public string SEGURO { get; set; }
        //detalle
        public string NOM_CANTON { get; set; }
        public string NOM_PARROQUIA { get; set; }
        public string NOM_ZONA { get; set; }
        public string SEXO { get; set; }
        public string JUNTA { get; set; }
        public string OPERADOR { get; set; }
        public string ESTADO { get; set; }
        public int COD_JUNTA { get; set; }
    }
}
