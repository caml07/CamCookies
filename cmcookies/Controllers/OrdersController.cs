using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmcookies.Models;

namespace cmcookies.Controllers
{
    [Authorize(Roles = "Admin")]
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
            if (status != "all")
            {
                query = query.Where(o => o.Status == status);
            }

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

        // POST: Orders/UpdateStatus
        // AQUÍ ESTÁ LA LÓGICA DE NEGOCIO PESADA
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, string newStatus)
        {
            // Usamos transacción porque vamos a tocar inventarios de Cookies Y Materiales
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var order = await _context.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == id);

                if (order == null) return NotFound();

                // REGLA: Solo descontamos inventario si pasamos de 'pending' a 'on_preparation'
                if (order.Status == "pending" && newStatus == "on_preparation")
                {
                    // 1. Validar y Descontar Stock de Galletas
                    foreach (var item in order.OrderDetails)
                    {
                        var cookie = await _context.Cookies.FindAsync(item.CookieCode);
                        if (cookie == null) throw new Exception($"Galleta {item.CookieCode} no encontrada.");

                        if (cookie.Stock < item.Qty)
                        {
                            TempData["ErrorMessage"] = $"No hay suficiente stock de {cookie.CookieName}. Tienes {cookie.Stock}, necesitas {item.Qty}. ¡Produce un Batch primero!";
                            return RedirectToAction(nameof(Details), new { id = id });
                        }

                        cookie.Stock -= item.Qty; // Restar galleta
                    }

                    // 2. Calcular Packaging (Bolsas y Stickers)
                    var totalCookies = order.OrderDetails.Sum(x => x.Qty);
                    string bagNeeded = (totalCookies >= 3) ? "Medium Bag" : "Small Bag";
                    bool stickerNeeded = (totalCookies >= 3);

                    // 3. Descontar Materiales de Empaque
                    var bagMaterial = await _context.Materials.FirstOrDefaultAsync(m => m.Name == bagNeeded);
                    if (bagMaterial != null)
                    {
                        bagMaterial.Stock -= 1; // 1 bolsa por pedido
                        order.Bag = bagNeeded;  // Guardar qué bolsa se usó
                    }

                    if (stickerNeeded)
                    {
                        var stickerMaterial = await _context.Materials.FirstOrDefaultAsync(m => m.Name == "Sticker");
                        if (stickerMaterial != null)
                        {
                            stickerMaterial.Stock -= 1;
                            order.Sticker = true;
                        }
                    }
                }
                
                // REGLA DE SEGURIDAD: Evitar reversar inventario por error (simplificación)
                // Si el pedido ya estaba "delivered" o "on_preparation", no dejamos volver a "pending" fácilmente
                // para no complicar la lógica de "devolver" stock.
                
                order.Status = newStatus;
                order.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Pedido #{order.OrderId} actualizado a: {newStatus.ToUpper()}";
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "Error procesando el pedido: " + ex.Message;
            }

            return RedirectToAction(nameof(Details), new { id = id });
        }
    }
}