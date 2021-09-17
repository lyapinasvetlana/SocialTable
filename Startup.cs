using System;
using Microsoft.AspNetCore.Authentication.Certificate;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.Facebook;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SocialNetWork.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using SocialNetWork.ReverseProxyHttpsEnforcer;

namespace SocialNetWork
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders =
                    ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            });
            services.AddEntityFrameworkNpgsql().AddDbContext<ApplicationContext>(options =>
            {
                options.UseNpgsql(Config.Config.SetConfig());
            });
            
            /*//добавили роли
            services.AddIdentity<IdentityUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true) 
                .AddEntityFrameworkStores<ApplicationContext>();;*/
           
            services.AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;

                })
                .AddCookie()
                .AddGoogle(options =>
                {
                    options.ClientId = Environment.GetEnvironmentVariable("IDGOOGLE");
                    options.ClientSecret = Environment.GetEnvironmentVariable("SECRETGOOGLE");
                })
                .AddYahoo(yahooOptions=>
                {
                    yahooOptions.ClientId = Environment.GetEnvironmentVariable("IDYAHOO");
                    yahooOptions.ClientSecret = Environment.GetEnvironmentVariable("SECRETYAHOO");
                })
                .AddGitHub(options =>
                {
                    options.ClientId = Environment.GetEnvironmentVariable("IDGITHUB");
                    options.ClientSecret = Environment.GetEnvironmentVariable("SECRETGITHUB");
                });
            services.AddControllersWithViews();
            
            /*services.Configure<SecurityStampValidatorOptions>(options =>
            {
                options.ValidationInterval = TimeSpan.Zero;   
            });*/
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders();
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseForwardedHeaders();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseForwardedHeaders();
                app.UseReverseProxyHttpsEnforcer();
                app.UseHsts();
            }

            app.Use((context, next) =>
            {
                context.Request.Scheme = "https";
                return next();
            });
            
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}