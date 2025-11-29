using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using cmcookies.Models; //para la coneccion con la database
using cmcookies.Data; //para que pueda modificar data dentro de la database

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
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
        options.Password.RequireDigit = true;           //Requiere números
        options.Password.RequireLowercase = true;       //Requiere minúsculas
        options.Password.RequireUppercase = true;       //Requiere mayúsculas
        options.Password.RequireNonAlphanumeric = true; //Requiere caracteres especiales
        options.Password.RequiredLength = 8;             // Mínimo 8 caracteres
    
        // Configuración de usuario
        options.User.RequireUniqueEmail = true;          // Email debe ser único
    
        // Configuración de bloqueo de cuenta (lockout)
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);  // Bloqueo por 5 minutos
        options.Lockout.MaxFailedAccessAttempts = 5;     // Máximo 5 intentos fallidos
        options.Lockout.AllowedForNewUsers = true;       // Habilitar bloqueo para nuevos usuarios
    })
    .AddEntityFrameworkStores<CmcDBContext>()  // Usar nuestro DbContext
    .AddDefaultTokenProviders();               // Para reset de contraseñas, confirmación de email, etc.

// Configurar cookies de autenticación
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";          // Ruta para login
    options.LogoutPath = "/Account/Logout";        // Ruta para logout
    options.AccessDeniedPath = "/Account/AccessDenied";  // Ruta si no tiene permisos
    options.ExpireTimeSpan = TimeSpan.FromDays(14);      // Cookie dura 14 días si RememberMe = true
    options.SlidingExpiration = true;              // Renueva la cookie automáticamente
    options.Cookie.HttpOnly = true;                // Protege contra XSS
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always; // Solo HTTPS
});

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
app.UseAuthentication(); //lee la cookie de secion para verificar qué usuario es el que se inicia sesión
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

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
    if (!await userManager.Users.AnyAsync())
    {
        await DbSeeder.SeedAsync(context, userManager, roleManager);
    }
}

app.Run();