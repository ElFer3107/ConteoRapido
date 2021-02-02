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
using System.IO;
using System.Threading.Tasks;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace CoreCRUDwithORACLE.Controllers
{
    public class UsuarioController : Controller
    {
        
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

            //if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
            //    return RedirectToAction("Logout", "Account");

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

           // if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
           //     return RedirectToAction("Logout", "Account");

            if (string.IsNullOrEmpty(id))
                return RedirectToAction("Logout", "Account");

            int codigoRol = Convert.ToInt32(HttpContext.Session.GetString("cod_rol"));
            int codigoProvincia = Convert.ToInt32(HttpContext.Session.GetString("cod_provincia"));

            var cedula = protector.Unprotect(id); 
            Usuario usuario = servicioUsuario.GetUsuario(cedula);

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
                if (respuesta==null)
                {
                    //ViewBag.Message = ;
                    ModelState.AddModelError(string.Empty, "Cédula o correo no permitito(pertenecen a otro usuario)");
                    return View(usuarioMod);
                }                    
                else
                {
                    
                    ModelState.AddModelError(string.Empty, "Usuario actualizado exitosamente!");
                    return View(usuarioMod);
                    //return RedirectToAction("Index");
                }
                    
            }
            catch (Exception ex)
            {


                ModelState.AddModelError(string.Empty, "Error al actualizar");
                return View(usuarioMod);
            }

        }
        public ActionResult IngresaUsuario()
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Logout", "Account");

           // if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
            //    return RedirectToAction("Logout", "Account");

            int codigoRol = Convert.ToInt32(HttpContext.Session.GetString("cod_rol"));
            int codigoProvincia = Convert.ToInt32(HttpContext.Session.GetString("cod_provincia"));

            UsuarioViewModel usuarioViewModel = new UsuarioViewModel();
            
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

            //if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
            //    return RedirectToAction("Logout", "Account");

           // if (usuarionNew == null)
             //   return RedirectToAction("Logout", "Account");

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


            Usuario validacionUsuario = servicioUsuario.GetUsuarioxCedulaMail(usuarionNew.CEDULAC, usuarionNew.MAIL);

            // validacionUsuario = servicioUsuario.GetUsuario(usuarionNew.CEDULAC);

            

            if (validacionUsuario != null)
            {
                ModelState.AddModelError(string.Empty, "Ya existe un usuario con la cédula o correo ingresada.");
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
                ViewBag.Message = "Usuario ingresado exitosamente!";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                if (usuario.CLAVE=="12345678" || usuario.CLAVE =="87654321")
                { 
                    ModelState.AddModelError(string.Empty, "La clave no puede ser números consecutivos");
                }
                else
                    { 
                    ModelState.AddModelError(string.Empty, "Existió un error al ingresar el usuario.");
                }
                return View(usuarionNew);
            }
        }
        [Route("Usuario/ActualizaClave/{cedula}")]
        public ActionResult ActualizaClave(string cedula, string email)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Logout", "Account");

            //if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
            //    return RedirectToAction("Logout", "Account");

            if (string.IsNullOrEmpty(cedula))
                return RedirectToAction("Logout", "Account");

            var id = protector.Unprotect(cedula); 

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

            string id = usuarioNew.CEDULA;
            usuarioNew.CEDULA = protector.Unprotect(id);

            Usuario usuario = null;

            usuario = servicioUsuario.ActualizaClave(usuarioNew, 0);
            if ((usuario.LOGEO == "88") || (usuario.LOGEO == "99") || (usuario.LOGEO == "77") || (usuario.LOGEO == "33"))
            {
                string mesanje = string.Empty;
                if (usuario.LOGEO == "33")
                    mesanje = "Error, la clave no puede ser números consecutivos";
                if (usuario.LOGEO == "77")
                    mesanje = "No se actualizó ningún usuario";
                if (usuario.LOGEO == "88")
                    mesanje = "Error al actualizar en la base de datos";
                if (usuario.LOGEO == "99")
                    mesanje = "Error, la nueva clave no debe ser igual a la anterior.";
                ModelState.AddModelError(string.Empty, mesanje);                
                return View(usuarioNew);
            }
            else
            {

                ModelState.AddModelError(string.Empty, "Contraseña actualizada correctamente");
                return View(usuarioNew);

            }
            /*
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
            */
        }
        [Route("Usuario/AltaClave/{cedula}")]
        public ActionResult AltaClave(string cedula)
        {
            //if (!User.Identity.IsAuthenticated)
            //    return RedirectToAction("Logout", "Account");

            //if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
            //    return RedirectToAction("Logout", "Account");
            string id = string.Empty;

            if (string.IsNullOrEmpty(cedula))
                return RedirectToAction("Logout", "Account");

            try
            {
                id = protector.Unprotect(cedula);
            }
            catch (Exception)
            {
                return RedirectToAction("Logout", "Account");
            } 

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
            //if (string.IsNullOrEmpty(HttpContext.Session.GetString("cod_rol")))
            //    return RedirectToAction("Logout", "Account");

            if (usuarioAlta == null)
                return RedirectToAction("Logout", "Account");

            Usuario usuario = null;
           
            string id = usuarioAlta.CEDULA;
            usuarioAlta.CEDULA = protector.Unprotect(id);

            usuario = servicioUsuario.ActualizaClave(usuarioAlta, 1);

            if ((usuario.LOGEO == "88")|| (usuario.LOGEO=="99")|| (usuario.LOGEO == "77") || (usuario.LOGEO == "33"))
            {
                string mesanje = string.Empty;
                if (usuario.LOGEO == "33")
                    mesanje = "Error, la clave no puede ser números consecutivos";
                if (usuario.LOGEO == "77")
                    mesanje = "No se actualizó ningún usuario";
                if (usuario.LOGEO == "88")
                    mesanje = "Error al actualizar en la base de datos";
                if (usuario.LOGEO == "99")
                    mesanje = "Error, la nueva clave no debe ser igual a la anterior.";
                ModelState.AddModelError(string.Empty, mesanje);
                return View(usuario);
            }
            else
            {

                ModelState.AddModelError(string.Empty, "Contraseña actualizada correctamente");
                return View(usuario);

            }
        }
        public ActionResult EnceraBase(string DET_CONFIGURACION, string VER_CONFIGURACION, string VER_PDF)
        {
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Logout", "Account");

            String Detalle_Carga = "";
            string version_carga = servicioUsuario.MuestraVersion();
            ViewBag.Detalle_Carga = version_carga;
            String Contr = VER_PDF;
            String Detalle = DET_CONFIGURACION;
            String Version = VER_CONFIGURACION;
            String Acta = "";
            String Resultados = "";
            String Asistencia = "";

            if (Contr == "2")
            {
                string dat = servicioUsuario.GeneraPDF();

                char delimitador = ';';
                string[] valores = dat.Split(delimitador);

                Document doc = new Document(PageSize.LETTER);
                // Indicamos donde vamos a guardar el documento
                MemoryStream memoryStream1 = new MemoryStream();
                PdfWriter writer = PdfWriter.GetInstance(doc, memoryStream1);
                // Le colocamos el título y el autor
                doc.AddTitle("Reporte Enceramiento Datos");
                doc.AddCreator("Consejo Nacional Electoral.");
                // Abrimos el archivo
                doc.Open();
                // Creamos el tipo de Font que vamos utilizar
                iTextSharp.text.Font _standardFont = new iTextSharp.text.Font(iTextSharp.text.Font.FontFamily.HELVETICA, 8, iTextSharp.text.Font.NORMAL, BaseColor.BLACK);
                // Escribimos el encabezamiento en el documento
                iTextSharp.text.Image imagen = iTextSharp.text.Image.GetInstance("images/logo1.png");
                imagen.Alignment = iTextSharp.text.Element.ALIGN_CENTER;
                doc.Add(imagen);

                doc.Add(new Paragraph(" "));
                // Creamos una tabla que devuelva los candidatos
                PdfPTable tblDatosAdicionales1 = new PdfPTable(1);
                tblDatosAdicionales1.WidthPercentage = 100;
                tblDatosAdicionales1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;


                // Configuramos el título de las columnas de la tabla
                PdfPCell clFirma1 = new PdfPCell(new Phrase("ELECCIONES GENERALES 2021", FontFactory.GetFont("ARIAL", 18, iTextSharp.text.Font.BOLD)));
                clFirma1.BorderWidth = 0;
                clFirma1.BorderWidthBottom = 0;
                clFirma1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;

                // Añadimos las celdas a la tabla
                tblDatosAdicionales1.AddCell(clFirma1);

                clFirma1 = new PdfPCell(new Phrase("7 DE FEBRERO 2021", FontFactory.GetFont("ARIAL", 18, iTextSharp.text.Font.BOLD)));
                clFirma1.BorderWidth = 0;
                clFirma1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;
                tblDatosAdicionales1.AddCell(clFirma1);

                doc.Add(tblDatosAdicionales1);

                doc.Add(Chunk.NEWLINE);

                doc.Add(new Paragraph("REPORTE DE ENCERAMIENTO - CONTEO RAPIDO", FontFactory.GetFont("ARIAL", 14, iTextSharp.text.Font.BOLD)));
                doc.Add(new Paragraph("Dignidad:  PRESIDENTE", FontFactory.GetFont("ARIAL", 13, iTextSharp.text.Font.BOLD)));
                doc.Add(new Paragraph("Hora de Enceramiento:" + DateTime.Now.ToString("t"), FontFactory.GetFont("ARIAL", 13, iTextSharp.text.Font.BOLD)));

                //doc.Add(new Paragraph(DateTime.Now.ToString("t")));
                doc.Add(Chunk.NEWLINE);

                // Creamos una tabla que 
                PdfPTable tblReporte = new PdfPTable(3);
                tblReporte.WidthPercentage = 100;

                // Configuramos el título de las columnas de la tabla
                PdfPCell clActas = new PdfPCell(new Phrase("Actas Enceradas", FontFactory.GetFont("ARIAL", 11, iTextSharp.text.Font.BOLD)));
                clActas.BorderWidth = 0;
                clActas.BorderWidthBottom = 0.75f;

                PdfPCell clResultados = new PdfPCell(new Phrase("Resultados Encerados", FontFactory.GetFont("ARIAL", 11, iTextSharp.text.Font.BOLD)));
                clResultados.BorderWidth = 0;
                clResultados.BorderWidthBottom = 0.75f;

                PdfPCell clAsistencia = new PdfPCell(new Phrase("Asistencia sin Encerar", FontFactory.GetFont("ARIAL", 11, iTextSharp.text.Font.BOLD)));
                clAsistencia.BorderWidth = 0;
                clAsistencia.BorderWidthBottom = 0.75f;

                // Añadimos las celdas a la tabla
                tblReporte.AddCell(clActas);
                tblReporte.AddCell(clResultados);
                tblReporte.AddCell(clAsistencia);

                // Llenamos la tabla con información
                clActas = new PdfPCell(new Phrase(valores[0].ToString(), FontFactory.GetFont("ARIAL", 11, iTextSharp.text.Font.NORMAL)));
                clActas.BorderWidth = 1;

                clResultados = new PdfPCell(new Phrase(valores[1].ToString(), FontFactory.GetFont("ARIAL", 11, iTextSharp.text.Font.NORMAL)));
                clResultados.BorderWidth = 1;

                clAsistencia = new PdfPCell(new Phrase(valores[2].ToString(), FontFactory.GetFont("ARIAL", 11, iTextSharp.text.Font.NORMAL)));
                clAsistencia.BorderWidth = 1;

                // Añadimos las celdas a la tabla
                tblReporte.AddCell(clActas);
                tblReporte.AddCell(clResultados);
                tblReporte.AddCell(clAsistencia);

                doc.Add(tblReporte);

                doc.Add(new Paragraph("Detalle de Votos Encerados", FontFactory.GetFont("ARIAL", 11, iTextSharp.text.Font.BOLD)));
                doc.Add(Chunk.NEWLINE);
                //////////////////////////
                // Creamos una tabla que devuelva los candidatos
                PdfPTable tblReporteCandidatos = new PdfPTable(5);
                tblReporte.WidthPercentage = 100;
                tblReporte.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;


                // Configuramos el título de las columnas de la tabla
                PdfPCell clFila1 = new PdfPCell(new Phrase("  ", _standardFont));
                clFila1.BorderWidth = 0;
                clFila1.BorderWidthBottom = 0;
                clFila1.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;

                PdfPCell clFila2 = new PdfPCell(new Phrase("Numero", FontFactory.GetFont("ARIAL", 13, iTextSharp.text.Font.BOLD)));
                clFila2.BorderWidth = 0;
                clFila2.BorderWidthBottom = 0.75f;
                clFila2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;

                PdfPCell clFila3 = new PdfPCell(new Phrase("Candidato", FontFactory.GetFont("ARIAL", 13, iTextSharp.text.Font.BOLD)));
                clFila3.BorderWidth = 0;
                clFila3.BorderWidthBottom = 0.75f;
                clFila3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;

                PdfPCell clFila4 = new PdfPCell(new Phrase("Votos", FontFactory.GetFont("ARIAL", 13, iTextSharp.text.Font.BOLD)));
                clFila4.BorderWidth = 0;
                clFila4.BorderWidthBottom = 0.75f;///
                clFila4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;

                PdfPCell clFila5 = new PdfPCell(new Phrase("  ", _standardFont));
                clFila5.BorderWidth = 0;
                clFila5.BorderWidthBottom = 0;///
                clFila5.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;

                // Añadimos las celdas a la tabla
                tblReporteCandidatos.AddCell(clFila1);
                tblReporteCandidatos.AddCell(clFila2);
                tblReporteCandidatos.AddCell(clFila3);
                tblReporteCandidatos.AddCell(clFila4);
                tblReporteCandidatos.AddCell(clFila5);

                string[,] Candidato = new string[16, 3];
                Candidato = servicioUsuario.Resultados_Candidato();
                int longitud = 16;
                int y = 0;
                while (y < longitud)
                {
                    int z = 0;
                    clFila1 = new PdfPCell(new Phrase(" ", _standardFont));
                    clFila1.BorderWidth = 0;

                    clFila2 = new PdfPCell(new Phrase(Candidato[y, z].ToString(), FontFactory.GetFont("ARIAL", 11, iTextSharp.text.Font.NORMAL)));
                    clFila2.BorderWidth = 1;
                    clFila2.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;

                    clFila3 = new PdfPCell(new Phrase(Candidato[y, z + 1].ToString(), FontFactory.GetFont("ARIAL", 11, iTextSharp.text.Font.NORMAL)));
                    clFila3.BorderWidth = 1;
                    clFila3.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;

                    clFila4 = new PdfPCell(new Phrase(Candidato[y, z + 2].ToString(), FontFactory.GetFont("ARIAL", 11, iTextSharp.text.Font.NORMAL)));
                    clFila4.BorderWidth = 1;
                    clFila4.HorizontalAlignment = iTextSharp.text.Element.ALIGN_CENTER;

                    clFila5 = new PdfPCell(new Phrase(" ", _standardFont));
                    clFila5.BorderWidth = 0;
                    tblReporteCandidatos.AddCell(clFila1);
                    tblReporteCandidatos.AddCell(clFila2);
                    tblReporteCandidatos.AddCell(clFila3);
                    tblReporteCandidatos.AddCell(clFila4);
                    tblReporteCandidatos.AddCell(clFila5);



                    y++;
                }
                doc.Add(tblReporteCandidatos);
                doc.Add(Chunk.NEWLINE);
                // Añadimos las celdas a la tabla
                ///////////////////
                // Creamos una tabla que devuelva los candidatos
                PdfPTable tblDatosAdicionales = new PdfPTable(1);
                tblDatosAdicionales.WidthPercentage = 100;
                tblDatosAdicionales.HorizontalAlignment = iTextSharp.text.Element.ALIGN_JUSTIFIED;


                // Configuramos el título de las columnas de la tabla
                PdfPCell clFirma = new PdfPCell(new Phrase("Firma Coordinador Nacional de Conteo Rápido", FontFactory.GetFont("ARIAL", 11, iTextSharp.text.Font.BOLD)));
                clFirma.BorderWidth = 0;
                clFirma.BorderWidthBottom = 0;
                clFirma.HorizontalAlignment = iTextSharp.text.Element.ALIGN_JUSTIFIED;

                // Añadimos las celdas a la tabla
                tblDatosAdicionales.AddCell(clFirma);

                clFirma = new PdfPCell(new Phrase("Cedula:_________________", FontFactory.GetFont("ARIAL", 11, iTextSharp.text.Font.BOLD)));
                clFirma.BorderWidth = 0;
                clFirma.HorizontalAlignment = iTextSharp.text.Element.ALIGN_JUSTIFIED;
                tblDatosAdicionales.AddCell(clFirma);

                clFirma = new PdfPCell(new Phrase("Nombre:_______________________________________", FontFactory.GetFont("ARIAL", 11, iTextSharp.text.Font.BOLD)));
                clFirma.BorderWidth = 0;
                clFirma.HorizontalAlignment = iTextSharp.text.Element.ALIGN_JUSTIFIED;

                tblDatosAdicionales.AddCell(clFirma);
                doc.Add(tblDatosAdicionales);
                ////////////////77777
                doc.Close();

                writer.Close();

                MemoryStream pdfstream = new MemoryStream(memoryStream1.ToArray());
                var bytes = pdfstream.ToArray();
                return File(bytes, System.Net.Mime.MediaTypeNames.Application.Octet, "Reporte.pdf");
                ///////////////////////////////////////
            }

            if ((Detalle is null) && (Version is null))
            {
                ViewBag.Detalle = "";
                ViewBag.VersionSistema = "";
            }
            else
            {
                if (Contr == "1")
                {
                    String usuario = "";

                    usuario = servicioUsuario.EncerarBase(Detalle, Version, Contr);

                    char delimitador = ';';
                    string[] valores = usuario.Split(delimitador);


                    if ((Convert.ToInt32(valores[0]) > 0) && (Convert.ToInt32(valores[1]) > 0) && (Convert.ToInt32(valores[2]) == 0))
                    {
                        ViewBag.Message = "Base actualizado exitosamente!";
                        //return RedirectToAction("Logout", "Account");
                        ViewBag.Detalle = DET_CONFIGURACION;
                        ViewBag.VersionSistema = VER_CONFIGURACION;
                        ViewBag.Acta = valores[0];
                        ViewBag.Resultados = valores[1];
                        ViewBag.Asistencia = valores[2];
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "No se pudo actualizar la base.");
                    }

                }

            }
            return View();
        }

    }
}
