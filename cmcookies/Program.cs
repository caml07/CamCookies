using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using cmcookies.Models; //para la coneccion con la database
using cmcookies.Data; //para que pueda modificar data dentro de la database
using cmcookies.Models.Factories;
using cmcookies.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

//Registrar DbContext con MySQL
builder.Services.AddDbContext<CmcDBContext>(options =>
  options.UseMySql(
    builder.Configuration.GetConnectionString("DefaultConnection"),
    ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("DefaultConnection"))
  )
);

// Configurar ASP.NET Identity
builder.Services.AddIdentity<User, Role>(options =>
  {
    // Configuración de contraseñas
    options.Password.RequireDigit = false;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = false;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 6;

    // Configuración de usuario
    options.User.RequireUniqueEmail = true;

    // Configuración de bloqueo de cuenta
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;
  })
  .AddEntityFrameworkStores<CmcDBContext>()
  .AddDefaultTokenProviders()
  .AddRoleManager<RoleManager<Role>>() // ← AGREGAR ESTA LÍNEA
  .AddUserManager<UserManager<User>>(); // ← AGREGAR ESTA LÍNEA

// Configurar cookies de autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
  options.LoginPath = "/Account/Login"; // Ruta para login
  options.LogoutPath = "/Account/Logout"; // Ruta para logout
  options.AccessDeniedPath = "/Account/AccessDenied"; // Ruta si no tiene permisos
  options.ExpireTimeSpan = TimeSpan.FromDays(14); // Cookie dura 14 días si RememberMe = true
  options.SlidingExpiration = true; // Renueva la cookie automáticamente
  options.Cookie.HttpOnly = true; // Protege contra XSS
  options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Solo HTTPS
});

// ===== SERVICIOS Y FACTORIES - Dependency Injection =====
builder.Services.AddScoped<ICookieFactory, CookieFactory>();
builder.Services.AddScoped<IBatchService, BatchService>();

// NUEVO: Agregar servicio de Session y Cache (necesario para session)
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
  options.IdleTimeout = TimeSpan.FromMinutes(30); // El carrito dura 30 mins inactivo
  options.Cookie.HttpOnly = true;
  options.Cookie.IsEssential = true;
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
  app.UseExceptionHandler("/Home/Error");
  // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
  app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication(); //lee la cookie de secion para verificar qué usuario es el que se inicia sesión
app.UseAuthorization();

app.UseSession();

app.MapControllerRoute(
  "default",
  "{controller=Home}/{action=Index}/{id?}");

// ============================================================================
// 🌱 SEEDER - CONTROL MANUAL
// ============================================================================
// Descomentar SOLO la opción que necesites:

using (var scope = app.Services.CreateScope())
{
  var context = scope.ServiceProvider.GetRequiredService<CmcDBContext>();
  var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
  var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<Role>>();

  // ┌───────────────────────────────────┐
  // │ OPCIÓN 1: SEED COMPLETO (admin + customer + galletas)  │
  // │ Descomentar para poblar con datos completos             │
  // └───────────────────────────────────┘
  // await DbSeeder.SeedAsync(context, userManager, roleManager);

  // ┌───────────────────────────────────┐
  // │ OPCIÓN 2: LIMPIEZA TOTAL borra tod o y deja solo admin│
  // │ ⚠️ ADVERTENCIA: Esto BORRA todos los datos              │
  // └───────────────────────────────────┘
  // await DbSeeder.CleanAndSeedAsync(context, userManager, roleManager);

  // ┌───────────────────────────────┐
  // │ OPCIÓN 3: SEED AUTOMÁTICO (solo si BD está vacía)│
  // │ Útil para producción - no borra datos existentes │
  // └───────────────────────────────┘
  if (!await userManager.Users.AnyAsync()) await DbSeeder.SeedAsync(context, userManager, roleManager);
}

app.Run();