using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Session;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using Microsoft.Extensions.Hosting;

namespace KitsuneAdminDashboard.Web
{
    public class Startup
    {
        const string ALI_CLOUD = "ALICLOUD";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers()
             .AddNewtonsoftJson();
            services.AddControllersWithViews();
            services.AddDistributedMemoryCache();
            services.AddSession();
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromHours(24);
            });
            services.AddSingleton<IConfiguration>(Configuration);
            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
               .AddCookie(o => {
                   o.LoginPath = new PathString("/k-admin/Home/Login");
                   o.LogoutPath = new PathString("/k-admin/Home/Login");
               });
               /*OnRedirectToLogin = context =>
               {
                   context.Response.Redirect(System.Uri.UnescapeDataString(context.RedirectUri));
                   return Task.FromResult(0);
               };*/
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
            }
            else
            {
                app.UseExceptionHandler("/k-admin/Home/Error");
            }

            //app.UseStaticFiles();
            app.UseAuthentication();
            app.UseSession();

            app.UseStaticFiles(new StaticFileOptions()
            {
                RequestPath = new PathString("/k-admin")
            });

            app.Use((context, next) =>
            {
                context.Response.Headers.Add("Cache-Control", "max-age=0");
                if (IsHostedOnAlicloud()) {
                    try {
                        StringValues aliHost = "";
                        context.Request.Headers.TryGetValue("Ali-Swift-Log-Host", out aliHost);
                        if (!String.IsNullOrEmpty(aliHost.ToString())) {
                            context.Request.Headers.Remove("Host");
                            context.Request.Headers.Add("Host", aliHost.ToString());
                        }
                    } catch (ArgumentNullException ex) {
                        Console.WriteLine(JsonConvert.SerializeObject(ex));
                    }
                }
                return next();
            });

            //var options = new RewriteOptions().AddRedirect(@"(.*[(\.js)])$", "kadmin/$1");

            //app.UseRewriter(options);


            ////Handles all routing coming from forwarded loadbalancer
            //app.UseMiddleware<RouteHandlerMiddleware>();

            app.UseRouting();
            app.UseAuthorization();
            app.UseCors("AnyOrigin");
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(name: "default",
                pattern: "k-admin/{controller=Home}/{action=Index}/{id?}");
            });
        }

        private Boolean IsHostedOnAlicloud()
        {
            Boolean isAliCloud = false;
            try
            {
                isAliCloud = (Environment.GetEnvironmentVariable("CLOUD_PROVIDER").Equals(ALI_CLOUD));
            }
            catch (Exception ex)
            {

            }
            return isAliCloud;
        }
    }
}
