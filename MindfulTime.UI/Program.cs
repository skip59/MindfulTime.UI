using Microsoft.AspNetCore.Authentication.Cookies;
using MindfulTime.UI.Interfaces;
using MindfulTime.UI.Services;

namespace MindfulTime.UI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(o =>
            {
                o.LoginPath = "/Auth";
            });
            builder.Services.AddSession(o =>
            {
                o.IdleTimeout = TimeSpan.FromMinutes(30);
                o.Cookie.HttpOnly = true;
                o.Cookie.IsEssential = true;
            });
            builder.Services.AddTransient<IHttpRequestService, HttpRequestService>();
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/WorkSpace/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseSession();
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=WorkSpace}/{action=Auth}");

            app.Run();
        }
    }
}
