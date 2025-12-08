using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmcookies.Models;
using cmcookies.Models.ViewModels.Admin;

namespace cmcookies.Controllers;

// ============================================================================
// ADMIN CONTROLLER - El Cuarto de Control 🖥️📊
// ============================================================================
// Este es el cerebro del sistema.
// Aquí el admin ve TODO lo que pasa en el negocio:
//
// ESTADÍSTICAS QUE MUESTRA:
// 1. 💰 Finanzas:
//    - Ingresos totales (de pedidos entregados)
//    - Costos totales (de batches producidos)
//    - Profit = Ingresos - Costos
//
// 2. 📦 Pedidos:
//    - Total de pedidos
//    - Pedidos pendientes
//    - Pedidos completados
//
// 3. 🍪 Galletas:
//    - Total de tipos de galletas
//    - Galletas con stock bajo (< 15 unidades)
//
// 4. 🏆 TOP PERFORMERS:
//    - Top 5 galletas más vendidas (por revenue)
//    - Top 5 clientes más fieles (por gasto total)
//
// 5. 🚨 ALERTAS:
//    - Galletas con stock bajo
//    - Materiales con stock bajo
//
// 6. 📅 ACTIVIDAD RECIENTE:
//    - Últimos 10 pedidos
//
// TODO ESTO SE CALCULA CON LINQ QUERIES COMPLEJAS.
// Es como hacer SQL, pero en C# (más fácil y type-safe).
// ============================================================================

/// <summary>
/// Controlador para todas las funcionalidades del Admin.
/// Solo accesible por usuarios con rol "Admin".
/// </summary>
[Authorize(Roles = "Admin")]  // 🔐 Solo admins pueden ver esto
public class AdminController : Controller
{
  private readonly CmcDBContext _context;

  public AdminController(CmcDBContext context)
  {
    _context = context;
  }

  // ============================================================================
  // Dashboard - El Centro de Comando 🏛️
  // ============================================================================
  // Este método calcula TODAS las estadísticas del dashboard.
  // Cada sección usa LINQ para consultar la BD de manera eficiente.
  //
  // LINQ (Language Integrated Query) es como SQL, pero en C#:
  // - SQL:  SELECT SUM(price) FROM orders WHERE status = 'delivered'
  // - LINQ: orders.Where(o => o.Status == "delivered").Sum(o => o.Price)
  //
  // VENTAJAS DE LINQ:
  // 1. Type-safe (el compilador detecta errores)
  // 2. IntelliSense (autocompletado en el IDE)
  // 3. Más legible para programadores de C#
  // 4. Entity Framework lo convierte a SQL automáticamente
  // ============================================================================
  
  /// <summary>
  /// Dashboard principal del Admin con estadísticas completas.
  /// </summary>
  public async Task<IActionResult> Dashboard()
  {
    var viewModel = new DashboardViewModel
    {
      // ========================================================================
      // 💰 FINANZAS: INGRESOS TOTALES
      // ========================================================================
      // Cómo se calcula:
      // 1. Tomamos solo pedidos ENTREGADOS (status == "delivered")
      // 2. Por cada pedido, sumamos todos sus items (OrderDetails)
      // 3. Multiplicamos UnitPrice * Qty de cada item
      // 4. Sumamos todo
      //
      // Ejemplo:
      // Pedido 1: 2 Oreos ($70) + 1 S'mores ($80) = $220
      // Pedido 2: 3 Oreos ($70) = $210
      // Total Revenue = $430
      // ========================================================================
      TotalRevenue = await _context.Orders
        .Where(o => o.Status == "delivered")        // Solo pedidos entregados
        .SelectMany(o => o.OrderDetails)            // Aplanar a OrderDetails
        .SumAsync(od => od.UnitPrice * od.Qty),     // Sumar precio * cantidad

      // ========================================================================
      // 💸 FINANZAS: COSTOS TOTALES
      // ========================================================================
      // Suma el costo de TODOS los batches producidos.
      // Cada batch tiene un TotalCost calculado al momento de creación.
      // (TotalCost = suma de todos los materiales usados)
      // ========================================================================
      TotalCosts = await _context.Batches.SumAsync(b => (decimal?)b.TotalCost) ?? 0,

      // ========================================================================
      // 📦 PEDIDOS: CONTADORES SIMPLES
      // ========================================================================
      TotalOrders = await _context.Orders.CountAsync(),                        // Todos
      PendingOrders = await _context.Orders.CountAsync(o => o.Status == "pending"),  // Pendientes
      CompletedOrders = await _context.Orders.CountAsync(o => o.Status == "delivered"), // Completados

      // ========================================================================
      // 🍪 GALLETAS: CONTADORES
      // ========================================================================
      TotalCookies = await _context.Cookies.CountAsync(),                    // Total de tipos
      LowStockCookies = await _context.Cookies.CountAsync(c => c.Stock < 15), // Con stock bajo

      // ========================================================================
      // 🏆 TOP 5 GALLETAS MÁS VENDIDAS
      // ========================================================================
      // Este es un LINQ QUERY COMPLEJO (equivalente a un JOIN + GROUP BY en SQL)
      //
      // Qué hace:
      // 1. JOIN OrderDetails + Orders + Cookies
      // 2. Filtra solo pedidos ENTREGADOS
      // 3. Agrupa por galleta (CookieCode + CookieName)
      // 4. Calcula para cada grupo:
      //    - TotalSold = suma de cantidades
      //    - TotalRevenue = suma de precio * cantidad
      // 5. Ordena por Revenue (de mayor a menor)
      // 6. Toma solo las primeras 5
      //
      // En SQL sería algo así:
      // SELECT c.CookieName, SUM(od.Qty), SUM(od.UnitPrice * od.Qty)
      // FROM OrderDetails od
      // JOIN Orders o ON od.OrderId = o.OrderId
      // JOIN Cookies c ON od.CookieCode = c.CookieCode
      // WHERE o.Status = 'delivered'
      // GROUP BY c.CookieCode, c.CookieName
      // ORDER BY SUM(od.UnitPrice * od.Qty) DESC
      // LIMIT 5
      // ========================================================================
      TopSellingCookies = await (
        from od in _context.OrderDetails           // De la tabla OrderDetails
        join o in _context.Orders on od.OrderId equals o.OrderId  // JOIN con Orders
        join c in _context.Cookies on od.CookieCode equals c.CookieCode  // JOIN con Cookies
        where o.Status == "delivered"              // Solo pedidos entregados
        group od by new { od.CookieCode, c.CookieName }  // Agrupar por galleta
        into g                                     // Alias del grupo
        orderby g.Sum(x => x.UnitPrice * x.Qty) descending  // Ordenar por revenue
        select new TopSellerViewModel
        {
          CookieName = g.Key.CookieName,           // Nombre de la galleta
          TotalSold = g.Sum(x => x.Qty),           // Total de unidades vendidas
          TotalRevenue = g.Sum(x => x.UnitPrice * x.Qty)  // Revenue total
        }
      ).Take(1).ToListAsync(),  // Tomar solo las primeras 5

      // ========================================================================
      // 🥇 TOP 5 CLIENTES MÁS FIELES
      // ========================================================================
      // Agrupa pedidos por cliente y calcula cuánto han gastado en total.
      // Similar al anterior, pero más simple.
      // ========================================================================
      TopCustomers = await _context.Orders
        .Where(o => o.Status == "delivered")       // Solo pedidos completados
        .GroupBy(o => new                          // Agrupar por cliente
        {
          o.Customer.CustomerId,
          o.Customer.User.FirstName,
          o.Customer.User.LastName,
          o.Customer.User.Email
        })
        .Select(g => new TopCustomerViewModel
        {
          CustomerName = g.Key.FirstName + " " + g.Key.LastName,  // Nombre completo
          Email = g.Key.Email,
          TotalOrders = g.Count(),                               // Cuántos pedidos ha hecho
          TotalSpent = g.SelectMany(o => o.OrderDetails)         // Cuánto ha gastado en total
                        .Sum(od => od.UnitPrice * od.Qty)
        })
        .OrderByDescending(x => x.TotalSpent)      // Ordenar por gasto (de mayor a menor)
        .Take(5)                                   // Tomar solo los 5 primeros
        .ToListAsync(),

      // ========================================================================
      // 📅 ÚLTIMOS 10 PEDIDOS
      // ========================================================================
      // Muestra actividad reciente para que el admin sepa qué está pasando.
      // ========================================================================
      RecentOrders = await _context.Orders
        .Include(o => o.Customer)                  // Incluir datos del cliente
        .ThenInclude(c => c.User)                  // Y del usuario asociado
        .Include(o => o.OrderDetails)              // Y los items del pedido
        .OrderByDescending(o => o.CreatedAt)       // Más recientes primero
        .Take(10)                                  // Solo los últimos 10
        .Select(o => new RecentOrderViewModel
        {
          OrderId = o.OrderId,
          CustomerName = o.Customer.User.FirstName + " " + o.Customer.User.LastName,
          TotalAmount = o.OrderDetails.Sum(od => od.UnitPrice * od.Qty),
          Status = o.Status ?? "unknown",          // Manejar posible null
          CreatedAt = o.CreatedAt ?? DateTime.Now  // Manejar posible null
        })
        .ToListAsync(),

      // ========================================================================
      // 🚨 ALERTAS DE STOCK BAJO
      // ========================================================================
      // Esta lista la llenamos después (más abajo) con galletas + materiales
      // ========================================================================
      LowStockAlerts = new List<LowStockAlertViewModel>()
    };

    // ==========================================================================
    // 🚨 CONSTRUIR ALERTAS DE STOCK BAJO
    // ==========================================================================
    // Combinamos alertas de GALLETAS (< 15 unidades) y MATERIALES (< 100)
    // ==========================================================================
    
    // 🍪 Galletas con stock bajo
    var lowStockCookies = await _context.Cookies
      .Where(c => c.Stock < 15)
      .Select(c => new LowStockAlertViewModel
      {
        ItemName = c.CookieName,
        ItemType = "Cookie",
        CurrentStock = (decimal)c.Stock,
        Unit = "units"
      })
      .ToListAsync();

    // Agregar alertas de materiales con stock bajo
    var lowStockMaterials = await _context.Materials
      .Where(m => m.Stock < 100)
      .Select(m => new LowStockAlertViewModel
      {
        ItemName = m.Name,
        ItemType = "Material",
        CurrentStock = m.Stock,
        Unit = m.Unit
      })
      .ToListAsync();

    viewModel.LowStockAlerts.AddRange(lowStockCookies);
    viewModel.LowStockAlerts.AddRange(lowStockMaterials);

    return View(viewModel);
  }
}