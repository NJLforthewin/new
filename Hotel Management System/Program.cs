using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using Hotel_Management_System.Models;
using Hotel_Management_System.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services.AddDbContext<HotelManagementDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<PayMongoService>();

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Access/Login";
        options.LogoutPath = "/Access/Logout";
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});

builder.Services.AddLogging(loggingBuilder =>
{
    loggingBuilder.ClearProviders();
    loggingBuilder.AddConsole();
    loggingBuilder.AddDebug();
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();
app.UseRouting();

app.UseCors("AllowAll");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "payment",
    pattern: "PaymentView/{action}/{id?}",
    defaults: new { controller = "PaymentView" });

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Access}/{action=Login}/{id?}");

app.Run();
