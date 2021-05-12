using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LeftRover.Models.AppDBContext;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using LeftRover.Models;

namespace LeftRover
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
            services.AddDbContext<AppDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("LeftRover")));

            services.AddDbContext<UserInfoDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("LeftRover")));

            services.AddDbContext<UserAddressDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("LeftRover")));

            services.AddDbContext<DonationsDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("LeftRover")));

            services.AddDbContext<LeftRoverDBContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("LeftRover")));

            services.AddIdentity<IdentityUser, IdentityRole>().AddEntityFrameworkStores<AppDBContext>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Donor", policy =>
                                  policy.RequireClaim("UserType", "Donor"));
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Recipient", policy =>
                                  policy.RequireClaim("UserType", "Recipient"));
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("Admin", policy =>
                                  policy.RequireClaim("UserType", "Admin"));
            });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("MAR", policy =>
                                  policy.RequireAssertion(context =>
                                  context.User.HasClaim(c =>
                                  (c.Type.Equals("UserType") && (c.Value.Equals("Donor") || c.Value.Equals("Recipient"))))));
            });

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
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
