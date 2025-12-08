using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmcookies.Models;

namespace cmcookies.Controllers;

// ============================================================================
// ORDERS CONTROLLER - El Gerente de Pedidos 📦
// ============================================================================
// Este controlador es SOLO para ADMINS.
// Maneja toda la gestión de pedidos:
// 1. Ver lista de pedidos (con filtros por estado)
// 2. Ver detalles de un pedido específico
// 3. CAMBIAR ESTADO de pedidos (aquí pasa la magia del inventario)
//
// ESTADOS POSIBLES:
// - pending         → Pedido creado, esperando confirmación
// - on_preparation  → En cocina (SE DESCUENTA INVENTARIO AQUÍ)
// - delivered       → Entregado al cliente
// - cancelled       → Cancelado (no se descuenta inventario)
//
// FLUJO IMPORTANTE:
// Cliente hace pedido → Estado: PENDING (inventario NO se toca)
// Admin confirma     → Estado: ON_PREPARATION (inventario SE DESCUENTA)
// Admin entrega      → Estado: DELIVERED (inventario ya estaba descontado)
//
// ¿Por qué no descontamos en PENDING?
// Porque si 10 clientes hacen pedidos a la vez, podrían reservar TODO el
// inventario sin pagar. Esperamos a que el admin confirme el pago.
// ============================================================================

[Authorize(Roles = "Admin")]  // 🚪 Solo admins pueden entrar aquí
public class OrdersController : Controller
{
  private readonly CmcDBContext _context;

  public OrdersController(CmcDBContext context)
  {
    _context = context;
  }

  // GET: Orders (Listado con filtros)
  public async Task<IActionResult> Index(string status = "all")
  {
    var query = _context.Orders
      .Include(o => o.Customer)
      .ThenInclude(c => c.User)
      .Include(o => o.OrderDetails)
      .AsQueryable();

    // Filtro por estado
    if (status != "all") query = query.Where(o => o.Status == status);

    // Ordenar por fecha (los más nuevos primero)
    var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();

    ViewData["CurrentStatus"] = status;
    return View(orders);
  }

  // GET: Orders/Details/5
  public async Task<IActionResult> Details(int? id)
  {
    if (id == null) return NotFound();

    var order = await _context.Orders
      .Include(o => o.Customer)
      .ThenInclude(c => c.User)
      .Include(o => o.OrderDetails)
      .ThenInclude(od => od.CookieCodeNavigation)
      .FirstOrDefaultAsync(m => m.OrderId == id);

    if (order == null) return NotFound();

    return View(order);
  }

  // ============================================================================
  // POST: Orders/UpdateStatus - EL CORAZÓN DEL SISTEMA DE INVENTARIO 📦⚔️
  // ============================================================================
  // Esta es LA función más crítica del sistema de pedidos.
  // Aquí se maneja el descuento de inventario cuando un pedido se confirma.
  //
  // QUÉ HACE:
  // 1. Valida que el pedido exista
  // 2. Si cambia de PENDING → ON_PREPARATION:
  //    a. Verifica que haya suficiente stock de cada galleta
  //    b. Descuenta las galletas del inventario
  //    c. Calcula qué bolsa se necesita (small/medium)
  //    d. Descuenta 1 bolsa del inventario de materiales
  //    e. Si necesita sticker (3+ galletas), descuenta 1 sticker
  // 3. Actualiza el estado del pedido
  // 4. Guarda todo en la BD
  //
  // TRANSACCIÓN:
  // Todo pasa dentro de una transacción. Si algo falla, se hace rollback
  // y el inventario NO se descuenta. Todo o nada. 🛡️
  //
  // REGLAS DE NEGOCIO:
  // - pending → on_preparation: SE DESCUENTA INVENTARIO
  // - on_preparation → delivered: NO se toca inventario (ya estaba descontado)
  // - cancelled: NO se descuenta nada
  //
  // IMPORTANTE: No permitimos volver de on_preparation a pending para no
  // complicar la lógica de "devolver" inventario. Esto es una simplificación.
  // En un sistema real, tendrías que implementar reversa de inventario.
  // ============================================================================
  
  // POST: Orders/UpdateStatus
  // AQUÍ ESTÁ LA LÓGICA DE NEGOCIO PESADA
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> UpdateStatus(int id, string newStatus)
  {
    // 🔒 Iniciamos transacción - Todo o nada (como Thanos, pero con galletas)
    using var transaction = await _context.Database.BeginTransactionAsync();

    try
    {
      // 🔍 Buscar el pedido con sus items
      var order = await _context.Orders
        .Include(o => o.OrderDetails)  // Traemos los items del pedido
        .FirstOrDefaultAsync(o => o.OrderId == id);

      if (order == null) return NotFound();

      // ========================================================================
      // LÓGICA PRINCIPAL: DESCUENTO DE INVENTARIO
      // ========================================================================
      // SOLO descontamos inventario cuando pasamos de 'pending' a 'on_preparation'
      // Esto significa que el admin confirmó el pago y va a preparar el pedido.
      // ========================================================================
      if (order.Status == "pending" && newStatus == "on_preparation")
      {
        // ====================================================================
        // PASO 1: VALIDAR Y DESCONTAR GALLETAS 🍪
        // ====================================================================
        foreach (var item in order.OrderDetails)
        {
          var cookie = await _context.Cookies.FindAsync(item.CookieCode);
          if (cookie == null) 
              throw new Exception($"Galleta {item.CookieCode} no encontrada.");

          // ❌ Si no hay suficiente stock, abortamos TODO
          if (cookie.Stock < item.Qty)
          {
            TempData["ErrorMessage"] =
              $"No hay suficiente stock de {cookie.CookieName}. Tienes {cookie.Stock}, necesitas {item.Qty}. ¡Produce un Batch primero!";
            return RedirectToAction(nameof(Details), new { id = id });
          }

          // ➖ Descontar las galletas del inventario
          cookie.Stock -= item.Qty;
        }

        // ====================================================================
        // PASO 2: CALCULAR EMPAQUE NECESARIO 🎁
        // ====================================================================
        // Reglas:
        // - 1-2 galletas: Small Bag, sin sticker
        // - 3+ galletas: Medium Bag, con sticker
        // ====================================================================
        var totalCookies = order.OrderDetails.Sum(x => x.Qty);
        var bagNeeded = totalCookies >= 3 ? "Medium Bag" : "Small Bag";
        var stickerNeeded = totalCookies >= 3;

        // ====================================================================
        // PASO 3: DESCONTAR BOLSA DEL INVENTARIO 👜
        // ====================================================================
        var bagMaterial = await _context.Materials.FirstOrDefaultAsync(m => m.Name == bagNeeded);
        if (bagMaterial != null)
        {
          bagMaterial.Stock -= 1; // 1 bolsa por pedido
          order.Bag = bagNeeded;   // Guardamos qué bolsa se usó (para registro)
        }

        // ====================================================================
        // PASO 4: DESCONTAR STICKER SI ES NECESARIO 🏷️
        // ====================================================================
        if (stickerNeeded)
        {
          var stickerMaterial = await _context.Materials.FirstOrDefaultAsync(m => m.Name == "Sticker");
          if (stickerMaterial != null)
          {
            stickerMaterial.Stock -= 1;
            order.Sticker = true;  // Marcamos que sí lleva sticker
          }
        }
      }

      // ========================================================================
      // REGLA DE SEGURIDAD: NO REVERSA DE INVENTARIO
      // ========================================================================
      // Si el pedido ya estaba "delivered" o "on_preparation", NO dejamos
      // volver a "pending" fácilmente para no complicar la lógica de "devolver"
      // stock. Esto es una simplificación para el proyecto académico.
      //
      // En un sistema real, deberías:
      // 1. Permitir cancelaciones (devolver inventario)
      // 2. Manejar devoluciones
      // 3. Auditar cambios de estado con logs
      // ========================================================================

      // 💾 ACTUALIZAR EL ESTADO Y LA FECHA
      order.Status = newStatus;
      order.UpdatedAt = DateTime.Now;

      // 💾 GUARDAR TODO EN LA BASE DE DATOS
      await _context.SaveChangesAsync();
      
      // ✅ Si llegamos aquí, todo salió bien, hacemos commit
      await transaction.CommitAsync();

      TempData["SuccessMessage"] = $"Pedido #{order.OrderId} actualizado a: {newStatus.ToUpper()}";
    }
    catch (Exception ex)
    {
      // 🚫 Si algo falló, hacemos rollback (volvemos todo como estaba)
      await transaction.RollbackAsync();
      TempData["ErrorMessage"] = "Error procesando el pedido: " + ex.Message;
    }

    return RedirectToAction(nameof(Details), new { id = id });
  }
}