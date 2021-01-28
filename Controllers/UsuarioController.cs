using CoreCRUDwithORACLE.Comunes;
using CoreCRUDwithORACLE.Interfaces;
using CoreCRUDwithORACLE.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Controllers
{
    public class UsuarioController : Controller
    {
        //[Produces("application/json")]
        IServicioUsuario servicioUsuario;
        private static Auxiliar auxiliar;
        private readonly IDataProtector protector;
        private readonly ApplicationUser auc;
        private readonly ILogger _logger;
        private string mensaje = string.Empty;

        public UsuarioController(IServicioUsuario _servicioUsuario, ApplicationUser auc, ILoggerFactory logger,
                            IDataProtectionProvider dataProtectionProvider, Helper dataHelper)
        {
            servicioUsuario = _servicioUsuario;
            this.auc = auc;
            _logger = logger.CreateLogger(typeof(UsuarioController));
            this.protector = dataProtectionProvider.CreateProtector(dataHelper.CodigoEnrutar);
        }
        public IActionResult Create()
        {
            return View();
        }
        public IActionResult Index()
        {

            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Logout", "Account");

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
                return RedirectToAction("Logout", "Account");

            IEnumerable<Usuario> usuarios = servicioUsuario.GetUsuarios(Convert.ToInt32(HttpContext.Session.GetString("cod_rol")),
                                                                    Convert.ToInt32(HttpContext.Session.GetString("cod_provincia")));
            if (usuarios == null)
            {
                ModelState.AddModelError(string.Empty, "No existen usuarios");
                return View();
            }
            if (!string.IsNullOrEmpty(mensaje))
                ViewBag.Message = mensaje;

            usuarios = usuarios.Select(e =>
            {
                e.SEGURO = protector.Protect(e.CEDULA);
                return e;
            });

            return View(usuarios);

        }
        //[Route("Usuario/Edit/'{id}'")]
        public ActionResult Edit(string id)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Logout", "Account");

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
                return RedirectToAction("Logout", "Account");

            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Logout", "Account");

            int codigoRol = Convert.ToInt32(HttpContext.Session.GetString("cod_rol"));
            int codigoProvincia = Convert.ToInt32(HttpContext.Session.GetString("cod_provincia"));

            //auxiliar = new Auxiliar();
            var cedula = protector.Unprotect(id); //auxiliar.DesencriptarClave(id);
            Usuario usuario = servicioUsuario.GetUsuario(cedula);
            //Usuario usuario = servicioUsuario.GetUsuario(id);

            if (usuario == null)
            {
                ModelState.AddModelError(string.Empty, "Información de usuario incorrecta.");
                return RedirectToAction("Logout", "Account");
            }


            UsuarioViewModel usuarioViewModel = new UsuarioViewModel();
            usuarioViewModel.CEDULA = usuario.CEDULA;
            usuarioViewModel.COD_USUARIO = usuario.COD_USUARIO;
            usuarioViewModel.ESTADO = usuario.ESTADO;
            usuarioViewModel.DIGITO = usuario.CEDULA.Substring(9, 1);
            usuarioViewModel.LOGEO = usuario.LOGEO;
            usuarioViewModel.MAIL = usuario.MAIL;
            usuarioViewModel.NOMBRE = usuario.NOMBRE;
            usuarioViewModel.PROVINCIA = usuario.PROVINCIA;
            usuarioViewModel.TELEFONO = usuario.TELEFONO;
            usuarioViewModel.ROL = usuario.ROL;

            var provincias = (from Provincia in auc.PROVINCIA
                              where Provincia.COD_PROVINCIA == usuario.COD_PROVINCIA
                              orderby Provincia.NOM_PROVINCIA
                              select new SelectListItem()
                              {
                                  Text = Provincia.NOM_PROVINCIA,
                                  Value = Provincia.COD_PROVINCIA.ToString()
                              }).ToList();

            usuarioViewModel.provincias = provincias;

            var roles = (from Rol in auc.ROL
                         where Rol.COD_ROL == usuario.COD_ROL
                         select new SelectListItem()
                         {
                             Text = Rol.DES_ROL,
                             Value = Rol.COD_ROL.ToString()
                         }).ToList();

            usuarioViewModel.roles = roles;

            return View(usuarioViewModel);
        }
        //[Route("Usuario/Edit/'{id}'")]
        [HttpPost]
        public ActionResult Edit(UsuarioViewModel usuarioMod)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Logout", "Account");

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
                return RedirectToAction("Logout", "Account");

            if (usuarioMod == null)
                return RedirectToAction("Logout", "Account");

            var provincias = (from Provincia in auc.PROVINCIA
                              where Provincia.COD_PROVINCIA > 0 && Provincia.COD_PROVINCIA < 26
                              orderby Provincia.NOM_PROVINCIA
                              select new SelectListItem()
                              {
                                  Text = Provincia.NOM_PROVINCIA,
                                  Value = Provincia.COD_PROVINCIA.ToString()
                              }).ToList();

            provincias.Insert(0, new SelectListItem()
            {
                Text = "----Elija Provincia----",
                Value = string.Empty
            });

            var roles = (from Rol in auc.ROL
                         where Rol.COD_ROL > 1
                         select new SelectListItem()
                         {
                             Text = Rol.DES_ROL,
                             Value = Rol.COD_ROL.ToString(),
                             Selected = false
                         }).ToList();

            roles.Insert(0, new SelectListItem()
            {
                Text = "----Elija Rol----",
                Value = string.Empty
            });

            usuarioMod.provincias = provincias;
            usuarioMod.roles = roles;

            auxiliar = new Auxiliar();

            if (!auxiliar.validarCedula(usuarioMod.CEDULA))
            {
                ModelState.AddModelError(string.Empty, "La cédula ingresada es incorrecta.");
                return View(usuarioMod);
            }

            UsuarioResponse usuario = new UsuarioResponse()
            {
                CEDULA = usuarioMod.CEDULA.Substring(0, 9),
                CODIGO_PROVINCIA = usuarioMod.codProvincia,
                CODIGO_ROL = usuarioMod.codRol,
                COD_USUARIO = usuarioMod.COD_USUARIO,
                DIGITO = usuarioMod.DIGITO,
                ESTADO = usuarioMod.ESTADO,
                LOGEO = usuarioMod.LOGEO,
                MAIL = usuarioMod.MAIL,
                NOMBRE = usuarioMod.NOMBRE,
                TELEFONO = usuarioMod.TELEFONO,
                PROVINCIA = usuarioMod.PROVINCIA,
                ROL = usuarioMod.ROL
            };

            try
            {
                Usuario respuesta = servicioUsuario.ActualizaUsuario(usuario);
                ViewBag.Message = "Usuario actualizado exitosamente!";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {


                ModelState.AddModelError(string.Empty, "Error al actualizar");
                return View(usuarioMod);
            }


            //return View(respuesta);
            //if (respuesta != null)
            //return RedirectToPage("/Index");

            //else
            //return View();
        }
        public ActionResult IngresaUsuario()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Logout", "Account");

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
                return RedirectToAction("Logout", "Account");

            int codigoRol = Convert.ToInt32(HttpContext.Session.GetString("cod_rol"));
            int codigoProvincia = Convert.ToInt32(HttpContext.Session.GetString("cod_provincia"));

            UsuarioViewModel usuarioViewModel = new UsuarioViewModel();
            //if (codigoRol == 3)
            //{
            switch (codigoRol)
            {
                case 1:
                    var provincias = (from Provincia in auc.PROVINCIA
                                      where Provincia.COD_PROVINCIA == 0
                                      orderby Provincia.NOM_PROVINCIA
                                      select new SelectListItem()
                                      {
                                          Text = Provincia.NOM_PROVINCIA,
                                          Value = Provincia.COD_PROVINCIA.ToString()
                                      }).ToList();

                    var roles = (from Rol in auc.ROL
                                 where Rol.COD_ROL == 2
                                 select new SelectListItem()
                                 {
                                     Text = Rol.DES_ROL,
                                     Value = Rol.COD_ROL.ToString(),
                                     Selected = false
                                 }).ToList();

                    //roles.Insert(0, new SelectListItem()
                    //{
                    //    Text = "----Elija Rol----",
                    //    Value = string.Empty
                    //});
                    usuarioViewModel.provincias = provincias;
                    usuarioViewModel.roles = roles;
                    break;
                case 2:
                    provincias = (from Provincia in auc.PROVINCIA
                                  where Provincia.COD_PROVINCIA > 0 && Provincia.COD_PROVINCIA < 26
                                  orderby Provincia.NOM_PROVINCIA
                                  select new SelectListItem()
                                  {
                                      Text = Provincia.NOM_PROVINCIA,
                                      Value = Provincia.COD_PROVINCIA.ToString()
                                  }).ToList();

                    provincias.Insert(0, new SelectListItem()
                    {
                        Text = "----Elija Provincia----",
                        Value = string.Empty
                    });

                    roles = (from Rol in auc.ROL
                             where Rol.COD_ROL == 3 || Rol.COD_ROL == 5 || Rol.COD_ROL == 6 || Rol.COD_ROL == 8
                             select new SelectListItem()
                             {
                                 Text = Rol.DES_ROL,
                                 Value = Rol.COD_ROL.ToString(),
                                 Selected = false
                             }).ToList();

                    roles.Insert(0, new SelectListItem()
                    {
                        Text = "----Elija Rol----",
                        Value = string.Empty
                    });
                    usuarioViewModel.provincias = provincias;
                    usuarioViewModel.roles = roles;

                    break;
                case 3:
                    provincias = (from Provincia in auc.PROVINCIA
                                  where Provincia.COD_PROVINCIA == codigoProvincia
                                  orderby Provincia.NOM_PROVINCIA
                                  select new SelectListItem()
                                  {
                                      Text = Provincia.NOM_PROVINCIA,
                                      Value = Provincia.COD_PROVINCIA.ToString()
                                  }).ToList();

                    roles = (from Rol in auc.ROL
                             where Rol.COD_ROL == 4
                             select new SelectListItem()
                             {
                                 Text = Rol.DES_ROL,
                                 Value = Rol.COD_ROL.ToString(),
                                 Selected = false
                             }).ToList();

                    usuarioViewModel.provincias = provincias;
                    usuarioViewModel.roles = roles;
                    break;
            }
            return View(usuarioViewModel);
        }
        [HttpPost]
        public async Task<IActionResult> IngresaUsuario(UsuarioViewModel usuarionNew)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Logout", "Account");

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
                return RedirectToAction("Logout", "Account");

            if (usuarionNew == null)
                return RedirectToAction("Logout", "Account");

            int codigoRol = Convert.ToInt32(HttpContext.Session.GetString("cod_rol"));
            int codigoProvincia = Convert.ToInt32(HttpContext.Session.GetString("cod_provincia"));
            auxiliar = new Auxiliar();

            switch (codigoRol)
            {
                case 1:
                    var provincias = (from Provincia in auc.PROVINCIA
                                      where Provincia.COD_PROVINCIA == 0
                                      orderby Provincia.NOM_PROVINCIA
                                      select new SelectListItem()
                                      {
                                          Text = Provincia.NOM_PROVINCIA,
                                          Value = Provincia.COD_PROVINCIA.ToString()
                                      }).ToList();

                    var roles = (from Rol in auc.ROL
                                 where Rol.COD_ROL == 2
                                 select new SelectListItem()
                                 {
                                     Text = Rol.DES_ROL,
                                     Value = Rol.COD_ROL.ToString(),
                                     Selected = false
                                 }).ToList();

                    //roles.Insert(0, new SelectListItem()
                    //{
                    //    Text = "----Elija Rol----",
                    //    Value = string.Empty
                    //});
                    usuarionNew.provincias = provincias;
                    usuarionNew.roles = roles;
                    break;
                case 2:
                    provincias = (from Provincia in auc.PROVINCIA
                                  where Provincia.COD_PROVINCIA > 0 && Provincia.COD_PROVINCIA < 26
                                  orderby Provincia.NOM_PROVINCIA
                                  select new SelectListItem()
                                  {
                                      Text = Provincia.NOM_PROVINCIA,
                                      Value = Provincia.COD_PROVINCIA.ToString()
                                  }).ToList();

                    provincias.Insert(0, new SelectListItem()
                    {
                        Text = "----Elija Provincia----",
                        Value = string.Empty
                    });

                    roles = (from Rol in auc.ROL
                             where Rol.COD_ROL == 3 || Rol.COD_ROL == 5 || Rol.COD_ROL == 6
                             select new SelectListItem()
                             {
                                 Text = Rol.DES_ROL,
                                 Value = Rol.COD_ROL.ToString(),
                                 Selected = false
                             }).ToList();

                    roles.Insert(0, new SelectListItem()
                    {
                        Text = "----Elija Rol----",
                        Value = string.Empty
                    });
                    usuarionNew.provincias = provincias;
                    usuarionNew.roles = roles;

                    break;
                case 3:
                    provincias = (from Provincia in auc.PROVINCIA
                                  where Provincia.COD_PROVINCIA == codigoProvincia
                                  orderby Provincia.NOM_PROVINCIA
                                  select new SelectListItem()
                                  {
                                      Text = Provincia.NOM_PROVINCIA,
                                      Value = Provincia.COD_PROVINCIA.ToString()
                                  }).ToList();

                    roles = (from Rol in auc.ROL
                             where Rol.COD_ROL == 4
                             select new SelectListItem()
                             {
                                 Text = Rol.DES_ROL,
                                 Value = Rol.COD_ROL.ToString(),
                                 Selected = false
                             }).ToList();

                    usuarionNew.provincias = provincias;
                    usuarionNew.roles = roles;
                    break;
            }

            if (!auxiliar.validarCedula(usuarionNew.CEDULAC))
            {
                ModelState.AddModelError(string.Empty, "La cédula ingresada es incorrecta.");
                return View(usuarionNew);
            }

            Usuario validacionUsuario = servicioUsuario.GetUsuario(usuarionNew.CEDULAC);

            if (validacionUsuario != null)
            {
                ModelState.AddModelError(string.Empty, "Ya existe un usuario con la cédula ingresada.");
                return View(usuarionNew);
            }

            UsuarioResponse usuario = new UsuarioResponse()
            {
                CEDULA = usuarionNew.CEDULAC,
                CODIGO_PROVINCIA = usuarionNew.codProvincia,
                CLAVE = usuarionNew.CLAVE,
                CODIGO_ROL = usuarionNew.codRol,
                COD_USUARIO = usuarionNew.COD_USUARIO,
                ESTADO = true,
                LOGEO = usuarionNew.LOGEO,
                MAIL = usuarionNew.MAIL.ToLower(),
                NOMBRE = usuarionNew.NOMBRE,
                TELEFONO = usuarionNew.TELEFONO,
                PROVINCIA = usuarionNew.PROVINCIA,
                ROL = usuarionNew.ROL
            };

            int respuesta = await servicioUsuario.IngresaUsuario(usuario);
            if (respuesta > 0)
            {
                var nombreUsuario = User.Claims.FirstOrDefault(x => x.Type == "Id").Value;
                _logger.LogInformation("Usuario:" + nombreUsuario + " Ingresa: " + usuario.CEDULA);
                //ModelState.
                ViewBag.Message = "Usuario ingresado exitosamente!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Existió un error al ingresar el usuario.");
                return View(usuarionNew);
            }
        }
        [Route("Usuario/ActualizaClave/{cedula}")]
        public ActionResult ActualizaClave(string cedula)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Logout", "Account");

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
                return RedirectToAction("Logout", "Account");

            if (string.IsNullOrEmpty(cedula))
                return RedirectToAction("Logout", "Account");

            var id = protector.Unprotect(cedula); //auxiliar.DesencriptarClave(id);

            Usuario usuario = servicioUsuario.GetUsuario(id);

            if (usuario == null)
                return RedirectToAction("Logout", "Account");

            usuario.CLAVE = string.Empty;
            return View(usuario);
        }
        [Route("Usuario/ActualizaClave/{cedula}")]
        [HttpPost]
        public ActionResult ActualizaClave(Usuario usuarioNew)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
                return RedirectToAction("Logout", "Account");

            if (usuarioNew == null)
                return RedirectToAction("Logout", "Account");

            //string codRol = usuarioNew.
            string id = usuarioNew.CEDULA;
            usuarioNew.CEDULA = protector.Unprotect(id);

            Usuario usuario = null;

            usuario = servicioUsuario.ActualizaClave(usuarioNew, 0);

            if (usuario != null)
            {
                mensaje = "Usuario actualizado exitosamente!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                ModelState.AddModelError(string.Empty, "No se pudo actualizar el usuario. Revise la clave.");
                return View(usuarioNew);
            }
        }
        [Route("Usuario/AltaClave/{cedula}")]
        public ActionResult AltaClave(string cedula)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Logout", "Account");

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
                return RedirectToAction("Logout", "Account");

            if (string.IsNullOrEmpty(cedula))
                return RedirectToAction("Logout", "Account");

            var id = protector.Unprotect(cedula); //auxiliar.DesencriptarClave(id);

            Usuario usuario = servicioUsuario.GetUsuario(id);

            if (usuario == null)
                return RedirectToAction("Logout", "Account");

            ViewBag.ESTCLAVE = 0;
            usuario.CLAVE = string.Empty;
            return View(usuario);
        }
        [Route("Usuario/AltaClave/{cedula}")]
        [HttpPost]
        public ActionResult AltaClave(Usuario usuarioAlta)
        {
            if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
                return RedirectToAction("Logout", "Account");

            if (usuarioAlta == null)
                return RedirectToAction("Logout", "Account");

            Usuario usuario = null;
           
            string id = usuarioAlta.CEDULA;
            usuarioAlta.CEDULA = protector.Unprotect(id);

            usuario = servicioUsuario.ActualizaClave(usuarioAlta, 1);

            if (usuario != null)
            {
                ViewBag.Message = "Usuario actualizado exitosamente!";
                return RedirectToAction("Logout", "Account");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "No se pudo actualizar el usuario. Revise la contraseña.");
                return View(usuarioAlta);
            }
        }
        public ActionResult EncerarBase()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Logout", "Account");

            if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
                return RedirectToAction("Logout", "Account");

            int usuario = 0;

            usuario = servicioUsuario.EncerarBase();

            if (usuario>0)
            {
                ViewBag.Message = "Base actualizado exitosamente!";
                return RedirectToAction("Logout", "Account");
            }
            else
            {
                ModelState.AddModelError(string.Empty, "No se pudo actualizar la base.");
                return View();
            }
        }
    }
}
