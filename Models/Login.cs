using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Models
{
    public class Login
    {
        public int COD_ROL { get; set; }
        public int COD_PROVINCIA { get; set; }
        public int EST_CLAVE { get; set; }
        public string CEDULA { get; set; }
        public string NOMBRE { get; set; }
    }
}
