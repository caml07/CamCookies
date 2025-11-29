using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using cmcookies.Models;

namespace cmcookies.Data;

/// <summary>
/// Clase encargada de poblar (seed) la base de datos con datos iniciales.
/// Incluye dos métodos principales:
/// - SeedAsync: Puebla con datos completos (admin + customer + galletas + materiales)
/// - CleanAndSeedAsync: Borra TODO y deja solo 1 admin
/// </summary>
public static class DbSeeder
{
  /// <summary>
  /// Limpia COMPLETAMENTE la base de datos y deja solo 1 usuario admin.
  /// ADVERTENCIA: Esto BORRA TODOS los datos existentes (usuarios, galletas, pedidos, TODO).
  /// </summary>
  public static async Task CleanAndSeedAsync(
    CmcDBContext context,
    UserManager<User> userManager,
    RoleManager<Role> roleManager)
  {
    Console.WriteLine("🧹 LIMPIEZA TOTAL: Borrando toda la base de datos...");

    // PASO 1: Borrar todos los datos de las tablas.
    // El orden es importante para evitar errores de claves foráneas (foreign keys).
    // Se empieza por las tablas que no tienen dependencias o cuyas dependencias ya se eliminaron.
    context.Transactions.RemoveRange(context.Transactions);
    context.CustomerShippings.RemoveRange(context.CustomerShippings);
    context.CustomerBillings.RemoveRange(context.CustomerBillings);
    context.OrderDetails.RemoveRange(context.OrderDetails);
    context.Orders.RemoveRange(context.Orders);
    context.Batches.RemoveRange(context.Batches);
    context.CookieMaterials.RemoveRange(context.CookieMaterials);
    context.Cookies.RemoveRange(context.Cookies);
    context.Materials.RemoveRange(context.Materials);
    context.Shippings.RemoveRange(context.Shippings);
    context.Billings.RemoveRange(context.Billings);
    context.Customers.RemoveRange(context.Customers);
    context.Phones.RemoveRange(context.Phones);
    context.UserRoles.RemoveRange(context.UserRoles);

    // Borrar usuarios de forma segura a través del UserManager de Identity.
    var users = await userManager.Users.ToListAsync();
    foreach (var user in users) await userManager.DeleteAsync(user);

    // Borrar roles de forma segura a través del RoleManager de Identity.
    var roles = await roleManager.Roles.ToListAsync();
    foreach (var role in roles) await roleManager.DeleteAsync(role);

    // Guarda todos los cambios de borrado en la base de datos.
    await context.SaveChangesAsync();
    Console.WriteLine("✅ Todos los datos borrados");

    // PASO 2: Resetear el contador AUTO_INCREMENT de cada tabla a 1.
    // Esto asegura que los nuevos registros empiecen con ID 1, como en una BD nueva.
    Console.WriteLine("🔄 Reseteando AUTO_INCREMENT a 1...");

    await context.Database.ExecuteSqlRawAsync("ALTER TABLE users AUTO_INCREMENT = 1");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE roles AUTO_INCREMENT = 1");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE user_roles AUTO_INCREMENT = 1");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE phones AUTO_INCREMENT = 1");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE customers AUTO_INCREMENT = 1");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE cookies AUTO_INCREMENT = 1");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE materials AUTO_INCREMENT = 1");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE cookie_materials AUTO_INCREMENT = 1");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE batches AUTO_INCREMENT = 1");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE orders AUTO_INCREMENT = 1");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE order_details AUTO_INCREMENT = 1");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE billings AUTO_INCREMENT = 1");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE customer_billings AUTO_INCREMENT = 1");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE shippings AUTO_INCREMENT = 1");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE customer_shippings AUTO_INCREMENT = 1");
    await context.Database.ExecuteSqlRawAsync("ALTER TABLE transactions AUTO_INCREMENT = 1");

    Console.WriteLine("✅ AUTO_INCREMENT reseteado");

    // PASO 3: Crear los datos mínimos para que el sistema funcione.
    Console.WriteLine("👤 Creando ÚNICAMENTE usuario admin...");

    // Crear roles "Admin" y "Customer", necesarios para la asignación de usuarios.
    var adminRole = new Role { Name = "Admin" };
    var customerRole = new Role { Name = "Customer" };
    await roleManager.CreateAsync(adminRole);
    await roleManager.CreateAsync(customerRole);

    // Crear el usuario administrador principal.
    var adminUser = new User
    {
      UserName = "admin@camcookies.com",
      Email = "admin@camcookies.com",
      FirstName = "Eduardo",
      LastName = "Quant",
      EmailConfirmed = true, // Se crea como confirmado para no requerir validación por email.
      IsActive = true,
      CreatedAt = DateTime.Now,
      UpdatedAt = DateTime.Now
    };

    // Usa UserManager para crear el usuario con su contraseña hasheada.
    var result = await userManager.CreateAsync(adminUser, "Admin@123");

    if (result.Succeeded)
    {
      // Asignar los roles de "Admin" y "Customer" al nuevo usuario.
      await userManager.AddToRoleAsync(adminUser, "Admin");
      await userManager.AddToRoleAsync(adminUser, "Customer");

      // Crear una entrada de teléfono para el admin.
      var adminPhone = new Phone
      {
        Phone1 = "+505 58899827",
        Phone2 = null
      };
      context.Phones.Add(adminPhone);
      await context.SaveChangesAsync();

      // Crear la entidad 'Customer' asociada al usuario admin.
      var adminCustomer = new Customer
      {
        UserId = adminUser.Id,
        PhoneId = adminPhone.PhoneId
      };
      context.Customers.Add(adminCustomer);
      await context.SaveChangesAsync();

      Console.WriteLine("✅ Admin creado: admin@camcookies.com / Admin@123");
      Console.WriteLine("🎉 ¡Limpieza completada! Base de datos vacía con solo 1 admin.");
    }
    else
    {
      // Si la creación del usuario falla, muestra los errores en la consola.
      Console.WriteLine("❌ Error al crear admin:");
      foreach (var error in result.Errors) Console.WriteLine($"   - {error.Description}");
    }
  }

  /// <summary>
  /// Puebla la base de datos con datos iniciales COMPLETOS.
  /// Incluye: Admin + Customer + Galletas + Materiales + Recetas + Capital inicial.
  /// Solo inserta datos si las tablas están vacías.
  /// </summary>
  public static async Task SeedAsync(
    CmcDBContext context,
    UserManager<User> userManager,
    RoleManager<Role> roleManager)
  {
    Console.WriteLine("🌱 Iniciando seed completo...");

    // ============================================================================
    // 1. CREAR ROLES
    // ============================================================================
    if (!await roleManager.Roles.AnyAsync())
    {
      Console.WriteLine("👥 Creando roles...");

      var adminRole = new Role { Name = "Admin" };
      var customerRole = new Role { Name = "Customer" };

      await roleManager.CreateAsync(adminRole);
      await roleManager.CreateAsync(customerRole);

      Console.WriteLine("✅ Roles creados: Admin, Customer");
    }

    // ============================================================================
    // 2. CREAR USUARIOS (Admin + Customer)
    // ============================================================================
    if (!await userManager.Users.AnyAsync())
    {
      Console.WriteLine("👤 Creando usuarios...");

      // --- ADMIN ---
      var adminUser = new User
      {
        UserName = "admin@camcookies.com",
        Email = "admin@camcookies.com",
        FirstName = "Eduardo",
        LastName = "Quant",
        EmailConfirmed = true,
        IsActive = true,
        CreatedAt = DateTime.Now,
        UpdatedAt = DateTime.Now
      };

      var adminResult = await userManager.CreateAsync(adminUser, "Admin@123");

      if (adminResult.Succeeded)
      {
        await userManager.AddToRoleAsync(adminUser, "Admin");
        await userManager.AddToRoleAsync(adminUser, "Customer");

        var adminPhone = new Phone
        {
          Phone1 = "+505 58899827",
          Phone2 = null
        };
        context.Phones.Add(adminPhone);
        await context.SaveChangesAsync();

        var adminCustomer = new Customer
        {
          UserId = adminUser.Id,
          PhoneId = adminPhone.PhoneId
        };
        context.Customers.Add(adminCustomer);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Admin creado: admin@camcookies.com / Admin@123");
      }

      // --- CUSTOMER ---
      var customerUser = new User
      {
        UserName = "customer@test.com",
        Email = "customer@test.com",
        FirstName = "María",
        LastName = "López",
        EmailConfirmed = true,
        IsActive = true,
        CreatedAt = DateTime.Now,
        UpdatedAt = DateTime.Now
      };

      var customerResult = await userManager.CreateAsync(customerUser, "Customer@123");

      if (customerResult.Succeeded)
      {
        await userManager.AddToRoleAsync(customerUser, "Customer");

        var customerPhone = new Phone
        {
          Phone1 = "+505 88888888",
          Phone2 = null
        };
        context.Phones.Add(customerPhone);
        await context.SaveChangesAsync();

        var customer = new Customer
        {
          UserId = customerUser.Id,
          PhoneId = customerPhone.PhoneId
        };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        Console.WriteLine("✅ Customer creado: customer@test.com / Customer@123");
      }
    }

    // ============================================================================
    // 3. CREAR MATERIALES
    // ============================================================================
    if (!context.Materials.Any())
    {
      Console.WriteLine("🥄 Creando materiales...");

      var materials = new List<Material>
      {
        // Ingredientes secos
        new() { Name = "Harina", Unit = "g", Stock = 5000, UnitCost = 0.035m },
        new() { Name = "Azúcar blanca", Unit = "g", Stock = 3000, UnitCost = 0.035m },
        new() { Name = "Azúcar morena", Unit = "g", Stock = 2000, UnitCost = 0.042m },
        new() { Name = "Azúcar glass", Unit = "g", Stock = 1000, UnitCost = 0.035m },
        new() { Name = "Cocoa", Unit = "g", Stock = 500, UnitCost = 0.573m },
        new() { Name = "Bicarbonato", Unit = "g", Stock = 200, UnitCost = 0.03m },
        new() { Name = "Sal", Unit = "g", Stock = 500, UnitCost = 0.00m },

        // Lácteos y huevos
        new() { Name = "Mantequilla La Perfecta", Unit = "g", Stock = 2000, UnitCost = 0.53m },
        new() { Name = "Huevo", Unit = "unidad", Stock = 60, UnitCost = 5.5m },

        // Chocolates
        new() { Name = "Chispas chocolate", Unit = "g", Stock = 1000, UnitCost = 0.56m },
        new() { Name = "Chocolate blanco", Unit = "g", Stock = 500, UnitCost = 0.52m },
        new() { Name = "Mini chocolate", Unit = "unidad", Stock = 100, UnitCost = 7.30m },

        // Extras
        new() { Name = "Oreo normal", Unit = "unidad", Stock = 100, UnitCost = 1.95m },
        new() { Name = "Marshmallow", Unit = "g", Stock = 500, UnitCost = 0.27m },
        new() { Name = "Pistacho sin cáscara", Unit = "g", Stock = 300, UnitCost = 0.794m },
        new() { Name = "Pistacho con cáscara", Unit = "g", Stock = 300, UnitCost = 0.593m },
        new() { Name = "Nutella", Unit = "g", Stock = 1000, UnitCost = 0.50m },

        // Saborizantes
        new() { Name = "Esencia", Unit = "ml", Stock = 100, UnitCost = 1.2m },
        new() { Name = "Colorante", Unit = "ml", Stock = 50, UnitCost = 1.17m },

        // Empaque
        new() { Name = "Vasito cupcake", Unit = "unidad", Stock = 200, UnitCost = 0.24m },
        new() { Name = "Bolsa celofán", Unit = "unidad", Stock = 200, UnitCost = 1.15m },
        new() { Name = "Bolsa papel pequeña", Unit = "unidad", Stock = 50, UnitCost = 1.50m },
        new() { Name = "Bolsa papel mediana", Unit = "unidad", Stock = 30, UnitCost = 3.50m },
        new() { Name = "Sticker", Unit = "unidad", Stock = 100, UnitCost = 0.60m }
      };

      context.Materials.AddRange(materials);
      await context.SaveChangesAsync();

      Console.WriteLine($"✅ {materials.Count} materiales creados");
    }

    // ============================================================================
    // 4. CREAR GALLETAS
    // ============================================================================
    if (!context.Cookies.Any())
    {
      Console.WriteLine("🍪 Creando galletas...");

      var cookies = new List<Cookie>
      {
        new()
        {
          CookieName = "S'mores",
          Description = "Deliciosa galleta con malvaviscos y chocolate, inspirada en el clásico de campamento",
          Price = 431.05m,
          Category = "normal",
          Stock = 0,
          IsActive = true,
          ImagePath = "/images/cookies/smores.jpg"
        },
        new()
        {
          CookieName = "Oreo",
          Description = "Irresistible galleta con trozos de Oreo y Nutella",
          Price = 389.20m,
          Category = "normal",
          Stock = 0,
          IsActive = true,
          ImagePath = "/images/cookies/oreo.jpg"
        }
      };

      context.Cookies.AddRange(cookies);
      await context.SaveChangesAsync();

      Console.WriteLine($"✅ {cookies.Count} galletas creadas");
    }

    // ============================================================================
    // 5. CREAR RELACIONES COOKIE-MATERIALS (Recetas)
    // ============================================================================
    if (!context.CookieMaterials.Any())
    {
      Console.WriteLine("📝 Creando recetas (cookie-materials)...");

      var smores = await context.Cookies.FirstAsync(c => c.CookieName == "S'mores");
      var oreo = await context.Cookies.FirstAsync(c => c.CookieName == "Oreo");

      var getMaterialId = new Func<string, int>(name =>
        context.Materials.First(m => m.Name == name).MaterialId);

      var cookieMaterials = new List<CookieMaterial>
      {
        // ===== RECETA S'MORES =====
        new()
        {
          CookieCode = smores.CookieCode, MaterialId = getMaterialId("Mantequilla La Perfecta"),
          ConsumptionPerBatch = 230
        },
        new() { CookieCode = smores.CookieCode, MaterialId = getMaterialId("Azúcar morena"), ConsumptionPerBatch = 90 },
        new() { CookieCode = smores.CookieCode, MaterialId = getMaterialId("Azúcar blanca"), ConsumptionPerBatch = 90 },
        new() { CookieCode = smores.CookieCode, MaterialId = getMaterialId("Harina"), ConsumptionPerBatch = 500 },
        new()
        {
          CookieCode = smores.CookieCode, MaterialId = getMaterialId("Chispas chocolate"), ConsumptionPerBatch = 100
        },
        new() { CookieCode = smores.CookieCode, MaterialId = getMaterialId("Huevo"), ConsumptionPerBatch = 3 },
        new() { CookieCode = smores.CookieCode, MaterialId = getMaterialId("Esencia"), ConsumptionPerBatch = 5 },
        new() { CookieCode = smores.CookieCode, MaterialId = getMaterialId("Marshmallow"), ConsumptionPerBatch = 120 },
        new()
        {
          CookieCode = smores.CookieCode, MaterialId = getMaterialId("Mini chocolate"), ConsumptionPerBatch = 20
        },
        new() { CookieCode = smores.CookieCode, MaterialId = getMaterialId("Sal"), ConsumptionPerBatch = 2 },
        new()
        {
          CookieCode = smores.CookieCode, MaterialId = getMaterialId("Vasito cupcake"), ConsumptionPerBatch = 20
        },
        new() { CookieCode = smores.CookieCode, MaterialId = getMaterialId("Bolsa celofán"), ConsumptionPerBatch = 20 },

        // ===== RECETA OREO =====
        new()
        {
          CookieCode = oreo.CookieCode, MaterialId = getMaterialId("Mantequilla La Perfecta"), ConsumptionPerBatch = 230
        },
        new() { CookieCode = oreo.CookieCode, MaterialId = getMaterialId("Azúcar morena"), ConsumptionPerBatch = 90 },
        new() { CookieCode = oreo.CookieCode, MaterialId = getMaterialId("Azúcar blanca"), ConsumptionPerBatch = 90 },
        new() { CookieCode = oreo.CookieCode, MaterialId = getMaterialId("Harina"), ConsumptionPerBatch = 500 },
        new()
        {
          CookieCode = oreo.CookieCode, MaterialId = getMaterialId("Chispas chocolate"), ConsumptionPerBatch = 100
        },
        new()
        {
          CookieCode = oreo.CookieCode, MaterialId = getMaterialId("Oreo normal"), ConsumptionPerBatch = 29
        }, // 9 triturada + 20 decoración
        new() { CookieCode = oreo.CookieCode, MaterialId = getMaterialId("Huevo"), ConsumptionPerBatch = 3 },
        new() { CookieCode = oreo.CookieCode, MaterialId = getMaterialId("Esencia"), ConsumptionPerBatch = 5 },
        new() { CookieCode = oreo.CookieCode, MaterialId = getMaterialId("Sal"), ConsumptionPerBatch = 2 },
        new() { CookieCode = oreo.CookieCode, MaterialId = getMaterialId("Nutella"), ConsumptionPerBatch = 160 },
        new() { CookieCode = oreo.CookieCode, MaterialId = getMaterialId("Vasito cupcake"), ConsumptionPerBatch = 20 },
        new() { CookieCode = oreo.CookieCode, MaterialId = getMaterialId("Bolsa celofán"), ConsumptionPerBatch = 20 }
      };

      context.CookieMaterials.AddRange(cookieMaterials);
      await context.SaveChangesAsync();

      Console.WriteLine($"✅ {cookieMaterials.Count} relaciones cookie-material creadas");
    }

    // ============================================================================
    // 6. CREAR TRANSACCIÓN INICIAL (Capital inicial)
    // ============================================================================
    if (!context.Transactions.Any())
    {
      Console.WriteLine("💰 Creando capital inicial...");

      var initialCapital = new Transaction
      {
        TransactionType = "initial_capital",
        Amount = 2000.00m,
        Description = "Inyección de capital inicial",
        CreatedAt = DateTime.Now
      };

      context.Transactions.Add(initialCapital);
      await context.SaveChangesAsync();

      Console.WriteLine("✅ Capital inicial de C$2,000.00 creado.");
    }

    Console.WriteLine("🎉 ¡Seed completo exitoso!");
  }
}