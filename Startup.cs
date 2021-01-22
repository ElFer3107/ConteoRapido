using CoreCRUDwithORACLE.Interfaces;
using CoreCRUDwithORACLE.Models;
using CoreCRUDwithORACLE.Servicios;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CoreCRUDwithORACLE
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            services.AddSession(options =>
            {
                options.Cookie.Name = ".AppSession_Conteo.session";
                // Set a short timeout for easy testing. 
                options.IdleTimeout = TimeSpan.FromMinutes(14);
                // You might want to only set the application cookies over a secure connection: 
                options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                options.Cookie.SameSite = SameSiteMode.Strict;
                options.Cookie.HttpOnly = true;
                // Make the session cookie essential 
                options.Cookie.IsEssential = true;
            });

            //services.AddDataProtection()
            //                        .PersistKeysToFileSystem(new DirectoryInfo(@"\\server\share\directory\"))
            //                        .ProtectKeysWithCertificate("thumbprint");
            services.AddAntiforgery();
            services.AddDataProtection()
                            .SetDefaultKeyLifetime(TimeSpan.FromDays(8))
                            .SetApplicationName("Conteo Rapido 2021");



            //services.AddIdentity<IdentityUser, IdentityRole>();
            services.AddDbContext<ApplicationUser>(options => options.UseOracle(Configuration.GetConnectionString("OracleDBConnection")));
            services.AddControllersWithViews();
            services.AddTransient<IServicioJunta, ServicioJunta>();
            services.AddTransient<IServicioUsuario, ServicioUsuario>();
            services.AddTransient<IServicioActa, ServicioActa>();
            services.AddTransient<IServicioReportes, ServicioReportes>();
            //services.AddControllersWithViews();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IConfiguration>(Configuration);

            services.AddMvc().AddRazorPagesOptions(options =>
            {
                //options.Conventions.AddPageRoute("/Junta/Index","");
                options.Conventions.AddPageRoute("/Account/Login", "");
            });
            services.AddDistributedMemoryCache();


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            loggerFactory.AddFile("Logs/Log-{Date}.txt");
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseSession();

            app.UseStaticFiles();

            app.UseRouting();


            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Account}/{action=Login}/{id?}");
            });
            //app.UseEndpoints(endpoints =>
            //{
            //    endpoints.MapControllerRoute(
            //        name: "default",
            //        pattern: "{controller=Account}/{action=Login}");
            //});
        }
    }
}
