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

        public IActionResult ByPass(string textoBuscar)
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
            ViewBag.Validaciones = string.Format("Name: {0} {1}", "Hola desde el Controlador", "Jhairo Rivera");
            return View("ConsultaResultados", resultadosVotos);
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
        private bool validaciones(ResultadosVotos resultadosVotos)
        {
            try
            {
                int totCandidatos = 0;
                foreach (var item in resultadosVotos.Resultados)
                {
                    totCandidatos += item.VOTOS;
                }
                //Validacion 1: Total Sufragantes > 0
                if (resultadosVotos.Acta.VOT_JUNTA == 0)
                {
                    return false;
                }
                //Validacion 2: Total Sufragantes <= Total Electores + 8
                if (resultadosVotos.Acta.VOT_JUNTA > resultadosVotos.Acta.TOT_ELECTORES + 8)
                {
                    return false;
                }
                //Validacion 3: Blancos + Nulos + Total Candidatos >= Total Sufragrantes - 1%
                if (resultadosVotos.Acta.NUL_JUNTA + resultadosVotos.Acta.BLA_JUNTA + totCandidatos < (resultadosVotos.Acta.VOT_JUNTA - (resultadosVotos.Acta.VOT_JUNTA * 0.01)))
                {
                    return false;
                }
                //Validacion 4: Blancos + Nulos + Total Candidatos <= Total Sufragrantes + 1%
                if (resultadosVotos.Acta.NUL_JUNTA + resultadosVotos.Acta.BLA_JUNTA + totCandidatos > (resultadosVotos.Acta.VOT_JUNTA + (resultadosVotos.Acta.VOT_JUNTA * 0.01)))
                {
                    return false;
                }
                //Validacion 5: Total de algún Candidato <= Total Validos
                foreach (var item in resultadosVotos.Resultados)
                {
                    if (item.VOTOS > resultadosVotos.Acta.VOT_JUNTA)
                        return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
            return false;
        }
        [HttpPost]
        public IActionResult ConsultaResultados(ResultadosVotos resultadosVotos)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");

            var _list = new List<Resultado>();
            for (int i = 0; i < resultadosVotos._CodCandidato.Count; i++)
            {
                _list.Add(new Resultado
                {
                    Cod_Candidato = resultadosVotos._CodCandidato[i],
                    Candidato = resultadosVotos._Candidato[i],
                    Orden = resultadosVotos._Orden[i],
                    VOTOS = resultadosVotos._Votos[i]
                });
            }
            var smsVal = validaciones(resultadosVotos);
            resultadosVotos.Resultados = _list.AsEnumerable();
            string cedula = User.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            Usuario usuario = servicioUsuario.GetUsuario(cedula);
            resultadosVotos.Acta.COD_USUARIO = usuario.COD_USUARIO;
            Respuesta respuesta = servicioActa.ActualizarVotosActa(resultadosVotos);
            ViewBag.Validaciones = string.Format("Name: {0} {1}", "Hola desde el Controlador", "Jhairo Rivera");
            if (respuesta == null)
            {
                ModelState.AddModelError(string.Empty, "Existió un error al ingresar los resultados.");
            }
            return  RedirectPreserveMethod("ByPass?textoBuscar=39003");

        }
    }
}
