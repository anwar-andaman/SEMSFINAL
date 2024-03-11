using Microsoft.EntityFrameworkCore;
using SEMS.Models;

namespace SEMS
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDistributedMemoryCache();
            
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(600);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });
            var provider=builder.Services.BuildServiceProvider();
            var config=provider.GetService<IConfiguration>();
            builder.Services.AddDbContext<ElectionDBContext>(item => item.UseSqlServer(config.GetConnectionString("dbConnection")));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            //app.UseDirectoryBrowser();
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();

            app.UseHttpsRedirection();
                    
          

            app.UseAuthorization();




            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Login}/{id?}");

            app.Run();
        }
    }
}
