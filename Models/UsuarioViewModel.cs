using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Models
{
    public class UsuarioViewModel : Usuario
    {
        [StringLength(10, MinimumLength = 10, ErrorMessage ="El campo cédula debe tener mínimo 10 dígitos")]
        [Required(ErrorMessage = "La cédula debe ser ingresada")]
        [DisplayName("CEDULA")]
        public string CEDULAC { get; set; }
        [DisplayName("PROVINCIA")]
        public int codProvincia { get; set; }
        public List<SelectListItem> provincias { get; set; }
        [DisplayName("ROL")]
        public int codRol { get; set; }
        public List<SelectListItem> roles { get; set; }

    }
}
