using CoreCRUDwithORACLE.Comunes;
using CoreCRUDwithORACLE.Interfaces;
using CoreCRUDwithORACLE.Models;
using CoreCRUDwithORACLE.ViewModels.Reportes;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Controllers
{
    public class ReportesController : Controller
    {
        private readonly IServicioReportes servicioReportes;
        private readonly ApplicationUser applicationUser;
        private readonly IDataProtector protector;
        //private static Auxiliar _helper = new Auxiliar();

        //// GET: ReportesController
        //public ActionResult Index()
        //{
        //    return View();
        //}

        //// GET: ReportesController/Details/5
        //public ActionResult Details(int id)
        //{
        //    return View();
        //}

        //// GET: ReportesController/Create
        //public ActionResult Create()
        //{
        //    return View();
        //}

        //// POST: ReportesController/Create
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Create(IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: ReportesController/Edit/5
        //public ActionResult Edit(int id)
        //{
        //    return View();
        //}

        //// POST: ReportesController/Edit/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Edit(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}

        //// GET: ReportesController/Delete/5
        //public ActionResult Delete(int id)
        //{
        //    return View();
        //}

        //// POST: ReportesController/Delete/5
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult Delete(int id, IFormCollection collection)
        //{
        //    try
        //    {
        //        return RedirectToAction(nameof(Index));
        //    }
        //    catch
        //    {
        //        return View();
        //    }
        //}
        //[ValidateAntiForgeryToken]
        //public async Task
        public ReportesController(IServicioReportes _servicioReportes, ApplicationUser _applicationUser,
                            IDataProtectionProvider dataProtectionProvider, Helper dataHelper)
        {
            servicioReportes = _servicioReportes;
            applicationUser = _applicationUser;
            this.protector = dataProtectionProvider.CreateProtector(dataHelper.CodigoEnrutar);
        }

        public async Task<IActionResult> Index(string sortOrder, string currentFilter,
                                                string textoBuscar, int? pageNumber)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");

            ViewData["CurrentSort"] = sortOrder;
            ViewData["ProvSortParm"] = String.IsNullOrEmpty(sortOrder) ? "prov_desc" : "";
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

            IEnumerable<AOperadoresProvincia> operadoresProvincias = null;
            int codigoProvincia = Convert.ToInt32(HttpContext.Session.GetString("cod_provincia"));
            int codigoRol = Convert.ToInt32(HttpContext.Session.GetString("cod_rol"));
            
            if (codigoRol != 5)
            {
                if (codigoProvincia == 0)
                    operadoresProvincias = await servicioReportes.OperadoresProvincia();
                else
                    operadoresProvincias = await servicioReportes.OperadoresProvincia(codigoProvincia);
            }
            else
                operadoresProvincias = await servicioReportes.OperadoresProvincia();

            if ((operadoresProvincias == null) || (operadoresProvincias.Count() == 0))
            {
                ModelState.AddModelError(string.Empty, "No existen Registros.");
                return View();
            }

            operadoresProvincias = operadoresProvincias.Select(e =>
              {
                  e.SEGUROP = protector.Protect(e.COD_PROV.ToString());
                  return e;
              });

            if (!String.IsNullOrEmpty(textoBuscar))
            {
                if (Int32.TryParse(textoBuscar, out number))
                {
                    operadoresProvincias = operadoresProvincias.Where(a => a.COD_PROV == number);
                }
                else
                {
                    operadoresProvincias = operadoresProvincias.Where(s => s.PROVINCIA.Contains(textoBuscar));
                }
            }

            switch (sortOrder)
            {
                case "prov_desc":
                    operadoresProvincias = operadoresProvincias.OrderByDescending(a => a.PROVINCIA);
                    break;
                default:
                    operadoresProvincias = operadoresProvincias.OrderBy(a => a.PROVINCIA);
                    break;
            }

            int pageSize = operadoresProvincias.Count();

            return View(await PaginatedListAsync<AOperadoresProvincia>.CreateAsync(operadoresProvincias.AsQueryable(), pageNumber ?? 1, pageSize));
        }

        [Route("Reportes/OperadoresCanton/{codProvincia}")]
        public async Task<IActionResult> OperadoresCanton(string codProvincia, string sortOrder, string currentFilter,
                                                string textoBuscar, int? pageNumber)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");

            ViewData["CantonSortParm"] = String.IsNullOrEmpty(sortOrder) ? "canton_desc" : "";
            ViewData["ProvSortParm"] = String.IsNullOrEmpty(sortOrder) ? "prov_desc" : "nom_prov";
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

            IEnumerable<AOperadoresCanton> operadoresCanton = null;
            
            int iCodProvincia = Convert.ToInt32(protector.Unprotect(codProvincia));

            if (iCodProvincia == 0)
                operadoresCanton = await servicioReportes.OperadoresCanton();
            else
                operadoresCanton = await servicioReportes.OperadoresCanton(iCodProvincia);

            if ((operadoresCanton == null) || (operadoresCanton.Count() == 0))
            {
                ModelState.AddModelError(string.Empty, "No existen Registros.");
                return View();
            }

            operadoresCanton = operadoresCanton.Select(e =>
            {
                e.operadoresProvincia.SEGUROP = protector.Protect(e.COD_CANTON.ToString());
                return e;
            });

            if (!String.IsNullOrEmpty(textoBuscar))
            {
                if (Int32.TryParse(textoBuscar, out number))
                {
                    operadoresCanton = operadoresCanton.Where(a => a.COD_CANTON == number);
                }
                else
                {
                    operadoresCanton = operadoresCanton.Where(s => s.CANTON.Contains(textoBuscar)
                                                                || s.operadoresProvincia.PROVINCIA.Contains(textoBuscar));
                }
            }

            switch (sortOrder)
            {
                case "canton_desc":
                    operadoresCanton = operadoresCanton.OrderByDescending(a => a.CANTON);
                    break;
                case "prov_desc":
                    operadoresCanton = operadoresCanton.OrderByDescending(a => a.operadoresProvincia.PROVINCIA);
                    break;
                case "nom_prov":
                    operadoresCanton = operadoresCanton.OrderBy(a => a.operadoresProvincia.PROVINCIA);
                    break;
                default:
                    operadoresCanton = operadoresCanton.OrderBy(a => a.CANTON);
                    break;
            }

            int pageSize = operadoresCanton.Count();

            return View(await PaginatedListAsync<AOperadoresCanton>.CreateAsync(operadoresCanton.AsQueryable(), pageNumber ?? 1, pageSize));

        }

        [Route("Reportes/OperadoresParroquia/{codCanton}")]
        public async Task<IActionResult> OperadoresParroquia(string codCanton, string sortOrder, string currentFilter,
                                                string textoBuscar, int? pageNumber)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");

            ViewData["CanSortParm"] = String.IsNullOrEmpty(sortOrder) ? "canton_desc" : "nom_cant";
            ViewData["ProvSortParm"] = String.IsNullOrEmpty(sortOrder) ? "prov_desc" : "nom_prov";
            ViewData["ParrSortParm"] = String.IsNullOrEmpty(sortOrder) ? "parr_desc" : ""; 
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

            IEnumerable<AOperadoresParroquia> operadoresParroquia = null;
           
            int iCodCanton = Convert.ToInt32(protector.Unprotect(codCanton));
            if (iCodCanton == 0)
                operadoresParroquia = await servicioReportes.OperadoresParroquia();
            else
                operadoresParroquia = await servicioReportes.OperadoresParroquia(iCodCanton);

            if ((operadoresParroquia == null) || (operadoresParroquia.Count() == 0))
            {
                ModelState.AddModelError(string.Empty, "No existen Registros.");
                return View();
            }

            operadoresParroquia = operadoresParroquia.Select(e =>
            {
                e.operadoresCanton.operadoresProvincia.SEGUROP = protector.Protect(e.PCODIGO.ToString());
                return e;
            });

            if (!String.IsNullOrEmpty(textoBuscar))
            {
                if (Int32.TryParse(textoBuscar, out number))
                {
                    operadoresParroquia = operadoresParroquia.Where(a => a.PCODIGO == number);
                }
                else
                {
                    operadoresParroquia = operadoresParroquia.Where(s => s.PARROQUIA.Contains(textoBuscar)
                                                                || s.operadoresCanton.operadoresProvincia.PROVINCIA.Contains(textoBuscar)
                                                                || s.operadoresCanton.CANTON.Contains(textoBuscar));
                }
            }

            switch (sortOrder)
            {
                case "parr_desc":
                    operadoresParroquia = operadoresParroquia.OrderByDescending(a => a.PARROQUIA);
                    break;
                case "canton_desc":
                    operadoresParroquia = operadoresParroquia.OrderByDescending(a => a.operadoresCanton.CANTON);
                    break;
                case "nom_cant":
                    operadoresParroquia = operadoresParroquia.OrderBy(a => a.operadoresCanton.CANTON);
                    break;
                case "prov_desc":
                    operadoresParroquia = operadoresParroquia.OrderByDescending(a => a.operadoresCanton.operadoresProvincia.PROVINCIA);
                    break;
                case "nom_prov":
                    operadoresParroquia = operadoresParroquia.OrderBy(a => a.operadoresCanton.operadoresProvincia.PROVINCIA);
                    break;
                default:
                    operadoresParroquia = operadoresParroquia.OrderBy(a => a.PARROQUIA);
                    break;
            }

            int pageSize = operadoresParroquia.Count();

            return View(await PaginatedListAsync<AOperadoresParroquia>.CreateAsync(operadoresParroquia.AsQueryable(), pageNumber ?? 1, pageSize));
            
        }

        [Route("Reportes/DetalleOperadores/{codParroquia}")]
        public async Task<IActionResult> DetalleOperadores(string codParroquia, string sortOrder, string currentFilter,
                                                string textoBuscar, int? pageNumber)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");

            ViewData["CanSortParm"] = String.IsNullOrEmpty(sortOrder) ? "canton_desc" : "nom_cant";
            ViewData["ProvSortParm"] = String.IsNullOrEmpty(sortOrder) ? "prov_desc" : "nom_prov";
            ViewData["ParrSortParm"] = String.IsNullOrEmpty(sortOrder) ? "parr_desc" : "";
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

            IEnumerable<DetalleOperadores> operadoresDetalle = null;

            int iCodParroquia = Convert.ToInt32(protector.Unprotect(codParroquia));

            if (iCodParroquia == 0)
                operadoresDetalle = await servicioReportes.OperadoresDetalle();
            else
                operadoresDetalle = await servicioReportes.OperadoresDetalle(iCodParroquia);

            if ((operadoresDetalle == null) || (operadoresDetalle.Count() == 0))
            {
                ModelState.AddModelError(string.Empty, "No existen Registros.");
                return View();
            }


            if (!String.IsNullOrEmpty(textoBuscar))
            {
                if (Int32.TryParse(textoBuscar, out number))
                {
                    operadoresDetalle = operadoresDetalle.Where(a => a.CODIGO == number);
                }
                else
                {
                    operadoresDetalle = operadoresDetalle.Where(s => s.PARROQUIA.Contains(textoBuscar)
                                                                || s.PROVINCIA.Contains(textoBuscar)
                                                                || s.CANTON.Contains(textoBuscar)
                                                                || s.USUARIO.Contains(textoBuscar));
                }
            }

            switch (sortOrder)
            {
                case "parr_desc":
                    operadoresDetalle = operadoresDetalle.OrderByDescending(a => a.PARROQUIA);
                    break;
                case "canton_desc":
                    operadoresDetalle = operadoresDetalle.OrderByDescending(a => a.CANTON);
                    break;
                case "nom_cant":
                    operadoresDetalle = operadoresDetalle.OrderBy(a => a.CANTON);
                    break;
                case "prov_desc":
                    operadoresDetalle = operadoresDetalle.OrderByDescending(a => a.PROVINCIA);
                    break;
                case "nom_prov":
                    operadoresDetalle = operadoresDetalle.OrderBy(a => a.PROVINCIA);
                    break;
                default:
                    operadoresDetalle = operadoresDetalle.OrderBy(a => a.PARROQUIA);
                    break;
            }

            int pageSize = operadoresDetalle.Count();

            return View(await PaginatedListAsync<DetalleOperadores>.CreateAsync(operadoresDetalle.AsQueryable(), pageNumber ?? 1, pageSize));
            
        }
        [Route("Reportes/TransmitidasProvincia")]
        public async Task<IActionResult> TransmitidasProvincia(string sortOrder, string currentFilter,
                                                string textoBuscar, int? pageNumber)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");

            ViewData["CurrentSort"] = sortOrder;
            ViewData["ProvSortParm"] = String.IsNullOrEmpty(sortOrder) ? "prov_desc" : "";
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

            int codigoProvincia = Convert.ToInt32(HttpContext.Session.GetString("cod_provincia"));
            int codigoRol = Convert.ToInt32(HttpContext.Session.GetString("cod_rol"));
           
            IEnumerable<ATransmitidasProvincia> transmitidasProvincias = null;
            
            if (codigoRol != 5)
            {
                if (codigoProvincia == 0)
                    transmitidasProvincias = await servicioReportes.TransmitidasProvincia();
                else
                    transmitidasProvincias = await servicioReportes.TransmitidasProvincia(codigoProvincia);
            }
            else
                transmitidasProvincias = await servicioReportes.TransmitidasProvincia();


            if ((transmitidasProvincias == null) || (transmitidasProvincias.Count() == 0))
            {
                ModelState.AddModelError(string.Empty, "No existen Registros.");
                return View();
            }

            transmitidasProvincias = transmitidasProvincias.Select(e =>
            {
                e.SEGUROCOD = protector.Protect(e.CODIGO.ToString());
                return e;
            });

            if (!String.IsNullOrEmpty(textoBuscar))
            {
                if (Int32.TryParse(textoBuscar, out number))
                {
                    transmitidasProvincias = transmitidasProvincias.Where(a => a.CODIGO == number);
                }
                else
                {
                    transmitidasProvincias = transmitidasProvincias.Where(s => s.PROVINCIA.Contains(textoBuscar));
                }
            }

            switch (sortOrder)
            {
                case "prov_desc":
                    transmitidasProvincias = transmitidasProvincias.OrderByDescending(a => a.PROVINCIA);
                    break;
                default:
                    transmitidasProvincias = transmitidasProvincias.OrderBy(a => a.PROVINCIA);
                    break;
            }

            int pageSize = transmitidasProvincias.Count();

            return View(await PaginatedListAsync<ATransmitidasProvincia>.CreateAsync(transmitidasProvincias.AsQueryable(), pageNumber ?? 1, pageSize));
            
        }
        [Route("Reportes/TransmitidasCanton/{codProvincia}")]
        public async Task<IActionResult> TransmitidasCanton(string codProvincia, string sortOrder, string currentFilter,
                                                string textoBuscar, int? pageNumber)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");

            ViewData["CantonSortParm"] = String.IsNullOrEmpty(sortOrder) ? "canton_desc" : "";
            ViewData["ProvSortParm"] = String.IsNullOrEmpty(sortOrder) ? "prov_desc" : "nom_prov";
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

            IEnumerable<ATransmitidasCanton> transmitidasCanton = null;
            int iCodProvincia = Convert.ToInt32(protector.Unprotect(codProvincia));
            
            if (iCodProvincia == 0)
                transmitidasCanton = await servicioReportes.TransmitidasCanton();
            else
                transmitidasCanton = await servicioReportes.TransmitidasCanton(iCodProvincia);

            if ((transmitidasCanton == null) || (transmitidasCanton.Count() == 0))
            {
                ModelState.AddModelError(string.Empty, "No existen Registros.");
                return View();
            }

            transmitidasCanton = transmitidasCanton.Select(e =>
            {
                e.SEGUROCOD = protector.Protect(e.CODIGO.ToString());
                return e;
            });

            if (!String.IsNullOrEmpty(textoBuscar))
            {
                if (Int32.TryParse(textoBuscar, out number))
                {
                    transmitidasCanton = transmitidasCanton.Where(a => a.CODIGO == number);
                }
                else
                {
                    transmitidasCanton = transmitidasCanton.Where(s => s.CANTON.Contains(textoBuscar)
                                                                || s.PROVINCIA.Contains(textoBuscar));
                }
            }

            switch (sortOrder)
            {
                case "canton_desc":
                    transmitidasCanton = transmitidasCanton.OrderByDescending(a => a.CANTON);
                    break;
                case "prov_desc":
                    transmitidasCanton = transmitidasCanton.OrderByDescending(a => a.PROVINCIA);
                    break;
                case "nom_prov":
                    transmitidasCanton = transmitidasCanton.OrderBy(a => a.PROVINCIA);
                    break;
                default:
                    transmitidasCanton = transmitidasCanton.OrderBy(a => a.CANTON);
                    break;
            }

            int pageSize = transmitidasCanton.Count();

            return View(await PaginatedListAsync<ATransmitidasCanton>.CreateAsync(transmitidasCanton.AsQueryable(), pageNumber ?? 1, pageSize));

        }
        [Route("Reportes/TransmitidasParroquia/{codCanton}")]
        public async Task<IActionResult> TransmitidasParroquia(string codCanton, string sortOrder, string currentFilter,
                                                string textoBuscar, int? pageNumber)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");

            ViewData["CantonSortParm"] = String.IsNullOrEmpty(sortOrder) ? "canton_desc" : "nom_cant";
            ViewData["ProvSortParm"] = String.IsNullOrEmpty(sortOrder) ? "prov_desc" : "nom_prov";
            ViewData["ParrSortParm"] = String.IsNullOrEmpty(sortOrder) ? "parr_desc" : "";
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

            int iCodCanton = Convert.ToInt32(protector.Unprotect(codCanton));
            IEnumerable<ATransmitidasParroquias> transmitidasParroquia = null;
            
            if (iCodCanton == 0)
                transmitidasParroquia = await servicioReportes.TransmitidasParroquia();
            else
                transmitidasParroquia = await servicioReportes.TransmitidasParroquia(iCodCanton);

            if ((transmitidasParroquia == null) || (transmitidasParroquia.Count() == 0))
            {
                ModelState.AddModelError(string.Empty, "No existen Registros.");
                return View();
            }

            transmitidasParroquia = transmitidasParroquia.Select(e =>
            {
                e.SEGUROCOD = protector.Protect(e.CODIGO.ToString());
                return e;
            });

            if (!String.IsNullOrEmpty(textoBuscar))
            {
                if (Int32.TryParse(textoBuscar, out number))
                {
                    transmitidasParroquia = transmitidasParroquia.Where(a => a.CODIGO == number);
                }
                else
                {
                    transmitidasParroquia = transmitidasParroquia.Where(s => s.PARROQUIA.Contains(textoBuscar)
                                                                || s.PROVINCIA.Contains(textoBuscar)
                                                                || s.CANTON.Contains(textoBuscar));
                }
            }

            switch (sortOrder)
            {
                case "parr_desc":
                    transmitidasParroquia = transmitidasParroquia.OrderByDescending(a => a.PARROQUIA);
                    break;
                case "canton_desc":
                    transmitidasParroquia = transmitidasParroquia.OrderByDescending(a => a.CANTON);
                    break;
                case "nom_cant":
                    transmitidasParroquia = transmitidasParroquia.OrderBy(a => a.CANTON);
                    break;
                case "prov_desc":
                    transmitidasParroquia = transmitidasParroquia.OrderByDescending(a => a.PROVINCIA);
                    break;
                case "nom_prov":
                    transmitidasParroquia = transmitidasParroquia.OrderBy(a => a.PROVINCIA);
                    break;
                default:
                    transmitidasParroquia = transmitidasParroquia.OrderBy(a => a.PARROQUIA);
                    break;
            }

            int pageSize = transmitidasParroquia.Count();

            return View(await PaginatedListAsync<ATransmitidasParroquias>.CreateAsync(transmitidasParroquia.AsQueryable(), pageNumber ?? 1, pageSize));
           
        }
        [Route("Reportes/TransmitidasDetalle/{codParroquia}")]
        public async Task<IActionResult> TransmitidasDetalle(string codParroquia, string sortOrder, string currentFilter,
                                                string textoBuscar, int? pageNumber)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");

            ViewData["CantonSortParm"] = String.IsNullOrEmpty(sortOrder) ? "canton_desc" : "nom_cant";
            ViewData["ProvSortParm"] = String.IsNullOrEmpty(sortOrder) ? "prov_desc" : "nom_prov";
            ViewData["ParrSortParm"] = String.IsNullOrEmpty(sortOrder) ? "parr_desc" : "";
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
            int iCodParroquia = Convert.ToInt32(protector.Unprotect(codParroquia));
            IEnumerable<DetallesTransmitidas> transmitidasParroquia = null;
            
            if (iCodParroquia == 0)
                transmitidasParroquia = await servicioReportes.TransmitidasDetalle();
            else
                transmitidasParroquia = await servicioReportes.TransmitidasDetalle(iCodParroquia);

            if ((transmitidasParroquia == null) || (transmitidasParroquia.Count() == 0))
            {
                ModelState.AddModelError(string.Empty, "No existen Registros.");
                return View();
            }

            if (!String.IsNullOrEmpty(textoBuscar))
            {
                if (Int32.TryParse(textoBuscar, out number))
                {
                    transmitidasParroquia = transmitidasParroquia.Where(a => a.CODIGO == number);
                }
                else
                {
                    transmitidasParroquia = transmitidasParroquia.Where(s => s.PARROQUIA.Contains(textoBuscar)
                                                                || s.PROVINCIA.Contains(textoBuscar)
                                                                || s.CANTON.Contains(textoBuscar)
                                                                || s.USUARIO.Contains(textoBuscar));
                }
            }

            switch (sortOrder)
            {
                case "parr_desc":
                    transmitidasParroquia = transmitidasParroquia.OrderByDescending(a => a.PARROQUIA);
                    break;
                case "canton_desc":
                    transmitidasParroquia = transmitidasParroquia.OrderByDescending(a => a.CANTON);
                    break;
                case "nom_cant":
                    transmitidasParroquia = transmitidasParroquia.OrderBy(a => a.CANTON);
                    break;
                case "prov_desc":
                    transmitidasParroquia = transmitidasParroquia.OrderByDescending(a => a.PROVINCIA);
                    break;
                case "nom_prov":
                    transmitidasParroquia = transmitidasParroquia.OrderBy(a => a.PROVINCIA);
                    break;
                default:
                    transmitidasParroquia = transmitidasParroquia.OrderBy(a => a.PARROQUIA);
                    break;
            }

            int pageSize = transmitidasParroquia.Count();

            return View(await PaginatedListAsync<DetallesTransmitidas>.CreateAsync(transmitidasParroquia.AsQueryable(), pageNumber ?? 1, pageSize));
            
        }
        [Route("Reportes/GeneralProvincia")]
        public async Task<IActionResult> GeneralProvincia(string sortOrder, string currentFilter,
                                                string textoBuscar, int? pageNumber)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");

            ViewData["CurrentSort"] = sortOrder;
            ViewData["ProvSortParm"] = String.IsNullOrEmpty(sortOrder) ? "prov_desc" : "";
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

            IEnumerable<InformacionGeneral> generalProvincias = null;
            int codigoProvincia = Convert.ToInt32(HttpContext.Session.GetString("cod_provincia"));
            int codigoRol = Convert.ToInt32(HttpContext.Session.GetString("cod_rol"));
            
            if (codigoRol != 5)
            {
                if (codigoProvincia == 0)
                    generalProvincias = await servicioReportes.GeneralProvincia();
                else
                    generalProvincias = await servicioReportes.GeneralProvincia(codigoProvincia);
            }
            else
                generalProvincias = await servicioReportes.GeneralProvincia();

            if ((generalProvincias == null) || (generalProvincias.Count() == 0))
            {
                ModelState.AddModelError(string.Empty, "No existen Registros.");
                return View();
            }

            generalProvincias = generalProvincias.Select(e =>
            {
                e.SEGUROCOD = protector.Protect(e.COD_PROVINCIA.ToString());
                return e;
            });

            if (!String.IsNullOrEmpty(textoBuscar))
            {
                if (Int32.TryParse(textoBuscar, out number))
                {
                    generalProvincias = generalProvincias.Where(a => a.COD_PROVINCIA == number);
                }
                else
                {
                    generalProvincias = generalProvincias.Where(s => s.NOM_PROVINCIA.Contains(textoBuscar));
                }
            }

            switch (sortOrder)
            {
                case "prov_desc":
                    generalProvincias = generalProvincias.OrderByDescending(a => a.NOM_PROVINCIA);
                    break;
                default:
                    generalProvincias = generalProvincias.OrderBy(a => a.NOM_PROVINCIA);
                    break;
            }

            int pageSize = generalProvincias.Count();

            return View(await PaginatedListAsync<InformacionGeneral>.CreateAsync(generalProvincias.AsQueryable(), pageNumber ?? 1, pageSize));
            
        }
        [Route("Reportes/GeneralCanton/{codigoProvincia}")]
        public async Task<IActionResult> GeneralCanton(string codigoProvincia, string sortOrder, string currentFilter,
                                               string textoBuscar, int? pageNumber)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");

            ViewData["CantonSortParm"] = String.IsNullOrEmpty(sortOrder) ? "canton_desc" : "";
            ViewData["ProvSortParm"] = String.IsNullOrEmpty(sortOrder) ? "prov_desc" : "nom_prov";
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
            int ICodigoProvincia = Convert.ToInt32(protector.Unprotect(codigoProvincia));
            IEnumerable<InformacionGeneral> generalesCanton = null;

            if (ICodigoProvincia == 0)
                generalesCanton = await servicioReportes.GeneralCanton();
            else
                generalesCanton = await servicioReportes.GeneralCanton(ICodigoProvincia);

            if ((generalesCanton == null) || (generalesCanton.Count() == 0))
            {
                ModelState.AddModelError(string.Empty, "No existen Registros.");
                return View();
            }

            generalesCanton = generalesCanton.Select(e =>
            {
                e.SEGUROCOD = protector.Protect(e.COD_CANTON.ToString());
                return e;
            });

            if (!String.IsNullOrEmpty(textoBuscar))
            {
                if (Int32.TryParse(textoBuscar, out number))
                {
                    generalesCanton = generalesCanton.Where(a => a.COD_CANTON == number);
                }
                else
                {
                    generalesCanton = generalesCanton.Where(s => s.NOM_CANTON.Contains(textoBuscar)
                                                                || s.NOM_PROVINCIA.Contains(textoBuscar));
                }
            }

            switch (sortOrder)
            {
                case "canton_desc":
                    generalesCanton = generalesCanton.OrderByDescending(a => a.NOM_CANTON);
                    break;
                case "prov_desc":
                    generalesCanton = generalesCanton.OrderByDescending(a => a.NOM_PROVINCIA);
                    break;
                case "nom_prov":
                    generalesCanton = generalesCanton.OrderBy(a => a.NOM_PROVINCIA);
                    break;
                default:
                    generalesCanton = generalesCanton.OrderBy(a => a.NOM_CANTON);
                    break;
            }

            int pageSize = generalesCanton.Count();

            return View(await PaginatedListAsync<InformacionGeneral>.CreateAsync(generalesCanton.AsQueryable(), pageNumber ?? 1, pageSize));

        }
        [Route("Reportes/GeneralesParroquia/{codCanton}")]
        public async Task<IActionResult> GeneralesParroquia(string codCanton, string sortOrder, string currentFilter,
                                                string textoBuscar, int? pageNumber)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");

            ViewData["ZonSortParm"] = String.IsNullOrEmpty(sortOrder) ? "zon_desc" : "nom_zon";
            ViewData["CantonSortParm"] = String.IsNullOrEmpty(sortOrder) ? "canton_desc" : "nom_cant";
            ViewData["ProvSortParm"] = String.IsNullOrEmpty(sortOrder) ? "prov_desc" : "nom_prov";
            ViewData["ParrSortParm"] = String.IsNullOrEmpty(sortOrder) ? "parr_desc" : "";
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

            IEnumerable<InformacionGeneral> generalesParroquia = null;
            
            int iCodCanton = Convert.ToInt32(protector.Unprotect(codCanton));
            if (iCodCanton == 0)
                generalesParroquia = await servicioReportes.GeneralParroquia();
            else
                generalesParroquia = await servicioReportes.GeneralParroquia(iCodCanton);

            if ((generalesParroquia == null) || (generalesParroquia.Count() == 0))
            {
                ModelState.AddModelError(string.Empty, "No existen Registros.");
                return View();
            }

            generalesParroquia = generalesParroquia.Select(e =>
            {
                e.SEGUROCOD = protector.Protect(e.COD_PARROQUIA.ToString());
                return e;
            });

            if (!String.IsNullOrEmpty(textoBuscar))
            {
                if (Int32.TryParse(textoBuscar, out number))
                {
                    generalesParroquia = generalesParroquia.Where(a => a.COD_PARROQUIA == number);
                }
                else
                {
                    generalesParroquia = generalesParroquia.Where(s => s.NOM_CANTON.Contains(textoBuscar)
                                                                || s.NOM_PARROQUIA.Contains(textoBuscar)
                                                                || s.NOM_PROVINCIA.Contains(textoBuscar)
                                                                || s.NOM_ZONA.Contains(textoBuscar));
                }
            }

            switch (sortOrder)
            {
                case "parr_desc":
                    generalesParroquia = generalesParroquia.OrderByDescending(a => a.NOM_PARROQUIA);
                    break;
                case "canton_desc":
                    generalesParroquia = generalesParroquia.OrderByDescending(a => a.NOM_CANTON);
                    break;
                case "nom_cant":
                    generalesParroquia = generalesParroquia.OrderBy(a => a.NOM_CANTON);
                    break;
                case "prov_desc":
                    generalesParroquia = generalesParroquia.OrderByDescending(a => a.NOM_PROVINCIA);
                    break;
                case "nom_prov":
                    generalesParroquia = generalesParroquia.OrderBy(a => a.NOM_PROVINCIA);
                    break;
                case "zon_desc":
                    generalesParroquia = generalesParroquia.OrderByDescending(a => a.NOM_ZONA);
                    break;
                case "nom_zon":
                    generalesParroquia = generalesParroquia.OrderBy(a => a.NOM_ZONA);
                    break;
                default:
                    generalesParroquia = generalesParroquia.OrderBy(a => a.NOM_PARROQUIA);
                    break;
            }

            int pageSize = generalesParroquia.Count();

            return View(await PaginatedListAsync<InformacionGeneral>.CreateAsync(generalesParroquia.AsQueryable(), pageNumber ?? 1, pageSize));
            
        }



        public async Task<IActionResult> GeneralAsistencia(string sortOrder, string currentFilter,
                                                string textoBuscar, int? pageNumber)
        {
            if (!User.Identity.IsAuthenticated)
                return Redirect("Account/LogOut");

            ViewData["CurrentSort"] = sortOrder;
            ViewData["ProvSortParm"] = String.IsNullOrEmpty(sortOrder) ? "prov_desc" : "";
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

            IEnumerable<GeneralAsistencia> generalAsistencia = null;
            int codigoProvincia = Convert.ToInt32(HttpContext.Session.GetString("cod_provincia"));
            int codigoRol = Convert.ToInt32(HttpContext.Session.GetString("cod_rol"));

            if (codigoRol <= 5 && codigoRol!=4)
            {
                if (codigoProvincia == 0)
                    generalAsistencia = await servicioReportes.GeneralAsistencia();
                else
                    generalAsistencia = await servicioReportes.GeneralAsistencia(codigoProvincia);
            }
            else
                generalAsistencia = await servicioReportes.GeneralAsistencia();

            if ((generalAsistencia == null) || (generalAsistencia.Count() == 0))
            {
                ModelState.AddModelError(string.Empty, "No existen Registros.");
                return View();
            }

            generalAsistencia = generalAsistencia.Select(e =>
            {
                e.SEGURO = protector.Protect(e.COD_PROVINCIA.ToString());
                return e;
            });

            if (!String.IsNullOrEmpty(textoBuscar))
            {
                if (Int32.TryParse(textoBuscar, out number))
                {
                    generalAsistencia = generalAsistencia.Where(a => a.COD_PROVINCIA == number);
                }
                else
                {
                    generalAsistencia = generalAsistencia.Where(s => s.NOM_PROVINCIA.Contains(textoBuscar));
                }
            }

            switch (sortOrder)
            {
                case "prov_desc":
                    generalAsistencia = generalAsistencia.OrderByDescending(a => a.NOM_PROVINCIA);
                    break;
                default:
                    generalAsistencia = generalAsistencia.OrderBy(a => a.NOM_PROVINCIA);
                    break;
            }

            int pageSize = generalAsistencia.Count();

            return View(await PaginatedListAsync<GeneralAsistencia>.CreateAsync(generalAsistencia.AsQueryable(), pageNumber ?? 1, pageSize));

        }




    }
}
