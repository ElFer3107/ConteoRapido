
using CoreCRUDwithORACLE.Comunes;
using CoreCRUDwithORACLE.Interfaces;
using CoreCRUDwithORACLE.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.DataProtection;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Controllers
{
    public class ActaController : Controller
    {
        IServicioActa servicioActa;
        private readonly IDataProtector protector;
        private readonly IServicioUsuario servicioUsuario;
        private string mensaje = string.Empty;

        public ActaController(IServicioActa _servicioActa, IServicioUsuario _servicioUsuario, IDataProtectionProvider dataProtectionProvider, Helper dataHelper)
           // public ActaController(IServicioActa _servicioActa, IServicioUsuario _servicioUsuario)
        {
            servicioActa = _servicioActa;
            servicioUsuario = _servicioUsuario;
            this.protector = dataProtectionProvider.CreateProtector(dataHelper.CodigoEnrutar);
        }

        public IActionResult Index(string sortOrder, string currentFilter, 
                                                string textoBuscar, int? pageNumber)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");
            ViewData["CurrentSort"] = sortOrder;
            ViewData["CanSortParm"] = String.IsNullOrEmpty(sortOrder) ? "canton_desc" : "";
            ViewData["ParrSortParm"] = String.IsNullOrEmpty(sortOrder) ? "parroquia_desc" : "nom_parroquia";
            ViewData["CurrentFilter"] = textoBuscar;

            int number;

            if (textoBuscar != null)
            {
                pageNumber = 1;
            }
            else
            {
                textoBuscar = currentFilter;
            }

            ViewData["CurrentFilter"] = textoBuscar;

            var actas = servicioActa.GetActas(Convert.ToInt32(HttpContext.Session.GetString("cod_provincia")));

            if (actas == null)
            {
                ModelState.AddModelError(string.Empty, "No existe información.");
                return View();
            }
            actas = actas.Select(e =>
            {
                e.SEGURO = protector.Protect(e.COD_JUNTA.ToString()) ;
                return e;
            });
            if (!String.IsNullOrEmpty(textoBuscar))
            {
                if (Int32.TryParse(textoBuscar, out number))
                {
                    actas = actas.Where(a => a.COD_JUNTA == number);
                }
                else
                {
                    actas = actas.Where(s => s.USUARIO.Contains(textoBuscar)
                                           || s.Nom_Canton.Contains(textoBuscar)
                                           || s.Nom_Parroquia.Contains(textoBuscar)
                                           || s.Nom_Zona.Contains(textoBuscar));
                }
            }

            switch (sortOrder)
            {
                case "canton_desc":
                    actas = actas.OrderByDescending(a => a.Nom_Canton);
                    break;
                case "nom_parroquia":
                    actas = actas.OrderBy(a => a.Nom_Parroquia);
                    break;
                case "parroquia_desc":
                    actas = actas.OrderByDescending(a => a.Nom_Parroquia);
                    break;
                case "nom_zona":
                    actas = actas.OrderBy(a => a.Nom_Zona);
                    break;
                case "sexo":
                    actas = actas.OrderBy(a => a.sexo);
                    break;
                case "junta":
                    actas = actas.OrderBy(a => a.junta);
                    break;
                default:
                    actas = actas.OrderBy(a => a.Nom_Canton);
                    break;
            }

            int pageSize = 10;

            if (!string.IsNullOrEmpty(mensaje))
                ViewBag.Message = mensaje;

           

            return View(PaginatedList<ActaResponse>.Create(actas.AsQueryable(), pageNumber ?? 1, pageSize));
            
        }

        // GET: ActaController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ActaController/Create
        public ActionResult Create()
        {

            return View();
        }

        // POST: ActaController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        public ActionResult Edit(string id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Logout", "Account");

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
                return RedirectToAction("Logout", "Account");

            if (!String.IsNullOrEmpty(id))
            {
                var cod_junta = protector.Unprotect(id.ToString());
                ActaResponse acta = servicioActa.GetActa(Convert.ToInt32(cod_junta.ToString()));
                mensaje = string.Empty;
                return View(acta);
            }
            return RedirectToAction("Logout", "Account");
        }

        // POST: ActaController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, ActaResponse collection,string ok )
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
                return RedirectToAction("Logout", "Account");

            if (collection== null)
                return RedirectToAction("Logout", "Account");

            try
            {
                Usuario usuario = servicioUsuario.GetUsuario(collection.CEDULA);
                if (usuario != null && usuario.COD_PROVINCIA == Convert.ToInt32(HttpContext.Session.GetString("cod_provincia")))
                {
                    collection.COD_USUARIO = usuario.COD_USUARIO;
                    Acta acta = servicioActa.ConsultarAsignacion(usuario.COD_USUARIO);
                    if (acta != null)
                    {
                        ModelState.AddModelError(string.Empty, "Operador ya se encuentra asignado a otra junta");
                        return View();
                    }

                    int respuesta = servicioActa.ActualizaActa(collection.COD_USUARIO, collection.COD_JUNTA);
                    if (respuesta > 0)
                    {
                        mensaje = "Acta Actualizada";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                        return View();
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Operador no pertenece a la provincia");
                    return View();
                }
            }
            catch
            {
                return View();
            }
        }

        // GET: ActaController/Delete/5
        public ActionResult Delete(string id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Logout", "Account");

            try
            {
                var cod_junta = protector.Unprotect(id.ToString());
                int respuesta = servicioActa.ActualizaAsignacion(Convert.ToInt32(cod_junta));
                if (respuesta > 0)
                {
                    mensaje = "Acta Liberada";
                    return RedirectToAction(nameof(Index));
                }

                ModelState.AddModelError(string.Empty, "Problemas para quitar la asignación revise la información.");
                return View();
            }
            catch
            {
                return View();
            }
        }

    }
}
