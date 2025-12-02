using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmcookies.Models;
using cmcookies.Models.ViewModels.Admin;

namespace cmcookies.Controllers;

/// <summary>
/// Controlador para todas las funcionalidades del Admin.
/// Solo accesible por usuarios con rol "Admin".
/// </summary>
[Authorize(Roles = "Admin")]
public class AdminController : Controller
{
  private readonly CmcDBContext _context;

  public AdminController(CmcDBContext context)
  {
    _context = context;
  }

  /// <summary>
  /// Dashboard principal del Admin con estadísticas completas.
  /// </summary>
  public async Task<IActionResult> Dashboard()
  {
    var viewModel = new DashboardViewModel
    {
      // ===== ESTADÍSTICAS FINANCIERAS =====
      TotalRevenue = await _context.Orders
        .Where(o => o.Status == "delivered")
        .SelectMany(o => o.OrderDetails)
        .SumAsync(od => od.UnitPrice * od.Qty),

      TotalCosts = await _context.Batches.SumAsync(b => b.TotalCost),

      // ===== CONTADORES DE PEDIDOS =====
      TotalOrders = await _context.Orders.CountAsync(),
      PendingOrders = await _context.Orders.CountAsync(o => o.Status == "pending"),
      CompletedOrders = await _context.Orders.CountAsync(o => o.Status == "delivered"),

      // ===== CONTADORES DE GALLETAS =====
      TotalCookies = await _context.Cookies.CountAsync(),
      LowStockCookies = await _context.Cookies.CountAsync(c => c.Stock < 15),

      // ===== TOP SELLING COOKIES (Top 5) =====
      TopSellingCookies = await (
        from od in _context.OrderDetails
        join o in _context.Orders on od.OrderId equals o.OrderId
        join c in _context.Cookies on od.CookieCode equals c.CookieCode
        where o.Status == "delivered"
        group od by new { od.CookieCode, c.CookieName }
        into g
        orderby g.Sum(x => x.UnitPrice * x.Qty) descending
        select new TopSellerViewModel
        {
          CookieName = g.Key.CookieName,
          TotalSold = g.Sum(x => x.Qty),
          TotalRevenue = g.Sum(x => x.UnitPrice * x.Qty)
        }
      ).Take(1).ToListAsync(),

      // ===== TOP CUSTOMERS (Top 5) =====
      TopCustomers = await _context.Orders
        .Where(o => o.Status == "delivered")
        .GroupBy(o => new
        {
          o.Customer.CustomerId,
          o.Customer.User.FirstName,
          o.Customer.User.LastName,
          o.Customer.User.Email
        })
        .Select(g => new TopCustomerViewModel
        {
          CustomerName = g.Key.FirstName + " " + g.Key.LastName,
          Email = g.Key.Email,
          TotalOrders = g.Count(),
          TotalSpent = g.SelectMany(o => o.OrderDetails).Sum(od => od.UnitPrice * od.Qty)
        })
        .OrderByDescending(x => x.TotalSpent)
        .Take(5)
        .ToListAsync(),

      // ===== RECENT ORDERS (Últimos 10) =====
      RecentOrders = await _context.Orders
        .Include(o => o.Customer)
        .ThenInclude(c => c.User)
        .Include(o => o.OrderDetails)
        .OrderByDescending(o => o.CreatedAt)
        .Take(10)
        .Select(o => new RecentOrderViewModel
        {
          OrderId = o.OrderId,
          CustomerName = o.Customer.User.FirstName + " " + o.Customer.User.LastName,
          TotalAmount = o.OrderDetails.Sum(od => od.UnitPrice * od.Qty),
          Status = o.Status ?? "unknown", // Manejar posible null
          CreatedAt = o.CreatedAt ?? DateTime.Now // Manejar posible null
        })
        .ToListAsync(),

      // ===== LOW STOCK ALERTS =====
      LowStockAlerts = new List<LowStockAlertViewModel>()
    };

    // Agregar alertas de galletas con stock bajo
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