﻿using CoreCRUDwithORACLE.Interfaces;
using CoreCRUDwithORACLE.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using CoreCRUDwithORACLE.Comunes;
using System.Threading.Tasks;
using CoreCRUDwithORACLE.ViewModels.Reportes;
using Microsoft.AspNetCore.DataProtection;

namespace CoreCRUDwithORACLE.Controllers
{
    public class ResultadosController : Controller
    {
        private readonly IServicioReportes servicioReportes;
        private readonly ApplicationUser applicationUser;
        private readonly IDataProtector protector;
        IServicioActa servicioActa;
        private readonly IServicioUsuario servicioUsuario;
      //  private readonly IDataProtector protector;

        public ResultadosController(IServicioActa _servicioActa, IServicioUsuario _servicioUsuario
            )
        {
            servicioActa = _servicioActa;
            servicioUsuario = _servicioUsuario;
            //this.protector = dataProtectionProvider.CreateProtector(dataHelper.CodigoEnrutar);
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
            ViewBag.Success = "1";
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
            //var cod_junta = protector.Protect(textoBuscar);
            int codJunta = Convert.ToInt32(textoBuscar);
            //string codJuta = cod_junta.ToString();
            resultadosVotos = servicioActa.ConsultaResultados(codJunta);

            if (resultadosVotos == null)
            {
                ModelState.AddModelError(string.Empty, "No existe información.");
                return View();
            }
            string codCandidatos = "";
            foreach (var item in resultadosVotos.Resultados)
                codCandidatos += item.Cod_Candidato + "|";
            ViewBag.CodCandidatos = codCandidatos.TrimEnd('|');
            ViewBag.Success = resultadosVotos.Acta.VOT_JUNTA > 0 ? "2" : "";
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
            //var smsVal = validaciones(resultadosVotos);
            resultadosVotos.Resultados = _list.AsEnumerable();
            string cedula = User.Claims.FirstOrDefault(x => x.Type == "Id").Value;
            Usuario usuario = servicioUsuario.GetUsuario(cedula);
            resultadosVotos.Acta.COD_USUARIO = usuario.COD_USUARIO;
            Respuesta respuesta = servicioActa.ActualizarVotosActa(resultadosVotos);
            if (respuesta == null)
            {
                ModelState.AddModelError(string.Empty, "Existió un error al ingresar los resultados.");
            }
            return RedirectPreserveMethod("ByPass?textoBuscar=" + resultadosVotos.Acta.COD_JUNTA);
        }

        
    }
}