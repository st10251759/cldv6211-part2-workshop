using MediBook.Data;
using Microsoft.EntityFrameworkCore;
using MediBook.Services;

namespace MediBook
{
/*
==============================Code Attribution==================================
ASP.NET Core Program Entry Point
Author: Microsoft
Link: https://learn.microsoft.com/en-us/aspnet/core/fundamentals/startup
Date Accessed: 28 April 2026
==============================Code Attribution==================================
*/

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();

            // Register MediBookDbContext with the SQL LocalDB connection string from appsettings.json
            builder.Services.AddDbContext<MediBookDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MediBookDbContext")));

            //Regsitering Blob Service as a singleton to be used across the application
            builder.Services.AddSingleton<MediBook.Services.BlobService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
