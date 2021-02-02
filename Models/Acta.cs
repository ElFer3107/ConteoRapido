using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Models
{
    public class Acta
    {
        [DisplayName("CODIGO DE JUNTA")]
        public int COD_JUNTA { get; set; }
        [DisplayName("CODIGO DE USUARIO")]
        public int COD_USUARIO { get; set; }
        [RegularExpression("(^[0-9]+$)", ErrorMessage = "Solo se permiten números")]
        [DisplayName("VOTOS")]
        public int VOT_JUNTA { get; set; }
        [RegularExpression("(^[0-9]+$)", ErrorMessage = "Solo se permiten números")]
        [Required(ErrorMessage = "Campo Obligatorio")]
        [DisplayName("BLANCOS")]
        public int BLA_JUNTA { get; set; }
        [RegularExpression("(^[0-9]+$)", ErrorMessage = "Solo se permiten números")]
        [Required(ErrorMessage = "Campo Obligatorio")]
        [DisplayName("NULOS")]
        public int NUL_JUNTA { get; set; }
        public int Estado_Acta { get; set; }
        public string PROVINCIA { get; set; }
        public string CANTON { get; set; }
        public string PARROQUIA { get; set; }
        public string ZONA { get; set; }
        public string JUNTA { get; set; }
        public int TOT_ELECTORES { get; set; }
        public int NOV_ACTA { get; set; }
        public string SEGURO { get; set; }
    }
}
