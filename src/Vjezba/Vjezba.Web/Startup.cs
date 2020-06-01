using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using System.Web.Mvc;
using Vjezba.DAL;
using Vjezba.Model;

namespace Vjezba.Web
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
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<AwfulDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("AwfulDbContext")));
            

           
            services.AddIdentity<AppUser, IdentityRole>()
              .AddRoleManager<RoleManager<IdentityRole>>()
              .AddDefaultUI()
              .AddDefaultTokenProviders()
              .AddEntityFrameworkStores<AwfulDbContext>();


            //services.AddAuthentication().AddGoogle(options =>
            //{
            //    IConfigurationSection googleAuthNSection =
            //        Configuration.GetSection("Authentication:Google");

            //    options.ClientId = googleAuthNSection["ClientID"];
            //    options.ClientSecret = googleAuthNSection["ClientSecret"];
            //});
            services.AddAuthentication().AddGoogle(googleOptions =>
            {
                googleOptions.ClientId = Configuration["Authentication:Google:ClientId"];
                googleOptions.ClientSecret = Configuration["Authentication:Google:ClientSecret"];
            });
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseExceptionHandler("/Views/Shared/Error");
                app.UseDeveloperExceptionPage();
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();


            var supportedCultures = new[] { new CultureInfo("hr"), new CultureInfo("en-US") }; 
            app.UseRequestLocalization(new RequestLocalizationOptions { 
                DefaultRequestCulture = new RequestCulture("hr"), 
                SupportedCultures = supportedCultures, 
                SupportedUICultures = supportedCultures 
            });

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Comic}/{action=Index}");
                routes.MapRoute(
                    name: "archive",
                    template: "{controller=Comic}/{action=Archive}");
            });

        }
    }
}