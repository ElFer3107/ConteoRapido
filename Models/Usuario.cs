using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Models
{
    public class Usuario
    {
        [DisplayName("CODIGO")]
        public int COD_USUARIO { get; set; }
        public int COD_PROVINCIA { get; set; }

        
        public string PROVINCIA { get; set; }
        [RegularExpression("(^[0-9]+$)", ErrorMessage = "Solo se permiten números")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "El campo cédula debe tener mínimo 10 dígitos")]
        [Required(ErrorMessage = "La cédula debe ser ingresada")]
        public string CEDULA { get; set; }
        public string DIGITO { get; set; }
        public string LOGEO { get; set; }
        [StringLength(8, MinimumLength = 8, ErrorMessage = "El campo clave debe tener mínimo 8 dígitos")]
        [RegularExpression(@"^(?=.*\d)(?=.*)(?!.*(.)\1{4})\S{8,}", ErrorMessage = "Formato de clave incorrecta")]
       // [RegularExpression("(^[0-9]+$)", ErrorMessage = "Solo se permiten números")]
        [Required(ErrorMessage = "La clave debe ser ingresada.")]

        public string CLAVE { get; set; }
        [Required(ErrorMessage = "Debe ingresar un nombre válido.")]
        public string NOMBRE { get; set; }

        public int COD_ROL { get; set; }
       
        public string ROL { get; set; }
        [NotMapped]
        public string SEGURO { get; set; }
        public bool ESTADO { get; set; }
        [EmailAddress(ErrorMessage = "El email es incorrecto")]
        [Required(ErrorMessage = "El email debe ser ingresado")]
        [RegularExpression(@"[a-z0-9._%+-]+@[a-z0-9.-]+\.[a-z]{2,}$", ErrorMessage = "El email es incorrecto")]
        public string MAIL { get; set; }
        [StringLength(10, MinimumLength = 9, ErrorMessage = "El campo teléfono debe tener mínimo 9 dígitos")]
        [Required(ErrorMessage = "El teléfono debe ser ingresado")]       
        [RegularExpression("(^[0-9]+$)", ErrorMessage = "Solo se permiten números")]
        [DisplayName("CELULAR")]
        public string TELEFONO { get; set; }
    }
}
