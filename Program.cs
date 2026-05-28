using MediBook.Data;
using Microsoft.EntityFrameworkCore;
using MediBook.Services;

namespace MediBook
{
    /*
    ==============================Code Attribution==================================
    ASP.NET Core Program Entry Point
    Author: Microsoft
    Link: [https://learn.microsoft.com/en-us/aspnet/core/fundamentals/startup](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/startup)
    Date Accessed: 28 April 2026
    ==============================Code Attribution==================================
    */

    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllersWithViews();

            // Register the Azure SQL database connection from appsettings.json.
            builder.Services.AddDbContext<MediBookDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("MediBookDbContext")));

            // Register BlobService for Azure Blob Storage uploads and deletions.
            builder.Services.AddSingleton<BlobService>();

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
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