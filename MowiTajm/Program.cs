using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MowiTajm;
using MowiTajm.Data;
using MowiTajm.Models;
using MowiTajm.Services;

var builder = WebApplication.CreateBuilder(args);

// L�gg till r�tt connection string beroende p� milj�
var connectionString = builder.Environment.IsDevelopment()
    ? builder.Configuration.GetConnectionString("LocalConnection")
    : builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<ApplicationUser>(options => options.SignIn.RequireConfirmedAccount = true) // --- �ndra till ApplicationUser fr�n IdentityUser.
    .AddRoles<IdentityRole>() // <--- Lagt till AddRoles.
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddScoped<IUserService, UserService>(); // <--- Lagt till UserService.

builder.Services.AddScoped<MovieService>(); // <--- Lagt till MovieService

builder.Services.AddScoped<ReviewService>(); // <--- Lagt till ReviewService

builder.Services.AddRazorPages();

builder.Services.AddHttpClient<OmdbService>();

var app = builder.Build();

var scope = app.Services.CreateScope();
var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
RoleSeeder.SeedRolesAsync(roleManager).Wait();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();
