using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Models
{
    public class ActaResponse : Acta
    {
        public string USUARIO { get; set; }
        public int Cod_Provincia { get; set; }
        [DisplayName("PROVINCIA")]
        public string Nom_Provincia { get; set; }
        public int Cod_Canton { get; set; }
        [DisplayName("CANTON")]
        public string Nom_Canton { get; set; }
        public int Cod_Parroquia { get; set; }
        [DisplayName("PARROQUIA")]
        public string Nom_Parroquia { get; set; }
        public int Cod_Zona { get; set; }
        [DisplayName("ZONA")]
        public string Nom_Zona { get; set; }
        [DisplayName("JUNTA")]
        public string junta { get; set; }
        [DisplayName("SEXO")]
        public string sexo { get; set; }
        [StringLength(10, MinimumLength = 10, ErrorMessage = "El campo cédula debe tener mínimo 10 dígitos")]
        public string CEDULA { get; set; }
        public int CodigoJunta { get; set; }
        public List<SelectListItem> Juntas { get; set; }
        public int CodigoUsuario { get; set; }
        public List<SelectListItem> Usuarios { get; set; }

    }
}
