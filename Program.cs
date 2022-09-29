using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using MusicStreamingService;
using MusicStreamingService.Data;
using MusicStreamingService.Models;

#pragma warning disable CS8602

namespace MusicSteamingService
{
    public class Program
    {
        public static void Main()
        {
            var builder = WebApplication.CreateBuilder();

            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<IRepository, Repository>();
            builder.Services.AddScoped<IEnvironmentWorker, EnvironmentWorker>();
            builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            builder.Services.AddHostedService<SongDeleterWorker>();

            if (builder.Environment.IsDevelopment())
            {
                Console.WriteLine("--> using InMem Db");
                builder.Services.AddDbContext<AppDbContext>(opt =>
                {
                    opt.UseInMemoryDatabase("InMem");
                }, ServiceLifetime.Scoped);
            }
            else
            {
                Console.WriteLine("--> using SqlLite Db");
                builder.Services.AddDbContext<AppDbContext>(opt =>
                {
                    opt.UseSqlite("FuckSpotify.db");
                }, ServiceLifetime.Scoped);
            }

            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
            {
                options.LoginPath = "/home/login";
                options.AccessDeniedPath = "/home/denied";
            });

            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseStatusCodePages();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseMiddleware<UserSecurityMiddleware>();

            app.UseStaticFiles(new StaticFileOptions
            {
                ServeUnknownFileTypes = true,
                OnPrepareResponse = (ctx) =>
                {
                    void killResponse()
                    {
                        ctx.Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        ctx.Context.Response.ContentLength = 0;
                        ctx.Context.Response.Body = Stream.Null;
                    }

                    return;
                }
            });

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.Run();
        }
    }
}
