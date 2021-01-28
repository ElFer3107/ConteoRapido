using CoreCRUDwithORACLE.Comunes;
using CoreCRUDwithORACLE.Interfaces;
using CoreCRUDwithORACLE.Models;
using CoreCRUDwithORACLE.ViewModels;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE.Controllers
{
    public class AccountController : Controller
    {
        private readonly IServicioUsuario usuarioManager;
        private readonly IDataProtector protector;


        public AccountController(IServicioUsuario usuarioManager,
                            IDataProtectionProvider dataProtectionProvider, Helper dataHelper)
        {
            this.usuarioManager = usuarioManager;
            this.protector = dataProtectionProvider.CreateProtector(dataHelper.CodigoEnrutar);
        }

        [HttpGet]
        public async Task<IActionResult> Login()
        {
            if (User.Identity.IsAuthenticated)
                await HttpContext.SignOutAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme);
            return View();
        }

        [ValidateAntiForgeryToken]
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                Login result = usuarioManager.GetAutenticacionUsuario(model.Email, model.Password);

                if (result != null)
                {

                    if (result.COD_ROL == 4)
                    {
                        ModelState.AddModelError(string.Empty, "Usuario no permitido.");
                        return View();
                    }

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.Name, result.NOMBRE),
                        new Claim("CodRol", result.COD_ROL.ToString()),
                        new Claim("Id", result.CEDULA.ToString()),
                        new Claim(ClaimTypes.Role, "Administrator"),
                    };

                    var claimsIdentity = new ClaimsIdentity(
                        claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    var authProperties = new AuthenticationProperties
                    {
                        //AllowRefresh = <bool>,
                        // Refreshing the authentication session should be allowed.

                        //ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(10),
                        // The time at which the authentication ticket expires. A 
                        // value set here overrides the ExpireTimeSpan option of 
                        // CookieAuthenticationOptions set with AddCookie.

                        //IsPersistent = true,
                        // Whether the authentication session is persisted across 
                        // multiple requests. When used with cookies, controls
                        // whether the cookie's lifetime is absolute (matching the
                        // lifetime of the authentication ticket) or session-based.

                        //IssuedUtc = <DateTimeOffset>,
                        // The time at which the authentication ticket was issued.

                        //RedirectUri = <string>
                        // The full path or absolute URI to be used as an http 
                        // redirect response value.
                    };

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity),
                        authProperties);

                    HttpContext.Session.SetString("cod_rol", result.COD_ROL.ToString());
                    ViewBag.CODROL = result.COD_ROL;

                    if (result.EST_CLAVE == 0)
                    {
                        return RedirectToAction("AltaClave", new RouteValueDictionary(
                                                        new { controller = "Usuario", action = "AltaClave", cedula = protector.Protect(result.CEDULA) }));
                    }
                    
                    HttpContext.Session.SetString("cod_provincia", result.COD_PROVINCIA.ToString());
                    
                    return RedirectToAction("index", "home");
                }
            }

            ModelState.AddModelError(string.Empty, "Intento de ingreso inválido.");
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            HttpContext.Session.SetString("cod_rol", "");

            await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login", "Account");
        }

    }
}
