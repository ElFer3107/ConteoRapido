using CoreCRUDwithORACLE.Interfaces;
using CoreCRUDwithORACLE.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Controllers
{
    public class ResultadosController : Controller
    {
        IServicioActa servicioActa;
        private readonly IServicioUsuario servicioUsuario;

        public ResultadosController(IServicioActa _servicioActa, IServicioUsuario _servicioUsuario)
        {
            servicioActa = _servicioActa;
            servicioUsuario = _servicioUsuario;
        }

        public IActionResult ConsultaResultados(string textoBuscar)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");

            ResultadosVotos resultadosVotos = null;

            ViewData["CurrentFilter"] = textoBuscar;

            if (string.IsNullOrEmpty(textoBuscar))
            {
                ModelState.AddModelError(string.Empty, "Ingrese información de Junta.");
                return View(resultadosVotos);
            }
            if (string.IsNullOrWhiteSpace(textoBuscar))
            {
                ModelState.AddModelError(string.Empty, "Ingrese información de Junta.");
                return View(resultadosVotos);
            }

            int codJunta = Convert.ToInt32(textoBuscar);

            
            resultadosVotos = servicioActa.ConsultaResultados(codJunta);

            if (resultadosVotos == null)
            {
                ModelState.AddModelError(string.Empty, "No existe información.");
                return View();
            }

            return View(resultadosVotos);

        }
        [HttpPost]
        public IActionResult ConsultaResultados(ResultadosVotos resultadosVotos)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");

            string cedula = User.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            Usuario usuario = servicioUsuario.GetUsuario(cedula);
            resultadosVotos.Acta.COD_USUARIO = usuario.COD_USUARIO;
            Respuesta respuesta = servicioActa.ActualizarVotosActa(resultadosVotos);
            if (respuesta == null)
            {
                ModelState.AddModelError(string.Empty, "Existió un error al ingresar los resultados.");
                return View();
            }
            return View(respuesta);

        }
    }
}
