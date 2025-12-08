using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmcookies.Models;
using cmcookies.Models.Store;           // Para CartItem
using cmcookies.Models.ViewModels.Store; // Para CheckoutViewModel
using cmcookies.Extensions;             // Para Session Helpers

// ============================================================================
// STORE CONTROLLER - El corazón del e-commerce 🛒
// ============================================================================
// Este controlador maneja TODA la experiencia del cliente:
// 1. Ver el menú de galletas (Index)
// 2. Agregar al carrito (AddBulkToCart)
// 3. Hacer checkout (Checkout GET y POST)
// 4. Ver confirmación de pedido (OrderConfirmation)
// 5. Ver historial de pedidos (MyOrders)
//
// IMPORTANTE: El carrito se guarda en SESSION, no en base de datos.
// Esto significa que si cierras el navegador, pierdes el carrito (como Amazon).
// ============================================================================

namespace cmcookies.Controllers
{
    public class StoreController : Controller
    {
        private readonly CmcDBContext _context;
        private readonly UserManager<User> _userManager;

        public StoreController(CmcDBContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Store (El Catálogo)
        public async Task<IActionResult> Index()
        {
            // Solo mostramos galletas ACTIVAS y con STOCK > 0
            var cookies = await _context.Cookies
                .Where(c => c.IsActive == true && c.Stock > 0)
                .OrderByDescending(c => c.Category) // Seasonal primero
                .ToListAsync();

            return View(cookies);
        }

        // POST: Store/AddBulkToCart (Agregar selección masiva)
        [HttpPost]
        public async Task<IActionResult> AddBulkToCart(Dictionary<string, int> products)
        {
            // 'products' es un diccionario: Clave = CookieCode, Valor = Cantidad
            
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
            int itemsAdded = 0;

            foreach (var entry in products)
            {
                string code = entry.Key;
                int qty = entry.Value;

                if (qty > 0)
                {
                    var cookie = await _context.Cookies.FindAsync(code);
                    if (cookie != null && cookie.Stock >= qty)
                    {
                        // Buscar si ya existe en el carrito
                        var item = cart.FirstOrDefault(c => c.CookieCode == code);
                        if (item != null)
                        {
                            item.Quantity += qty;
                        }
                        else
                        {
                            cart.Add(new CartItem
                            {
                                CookieCode = cookie.CookieCode,
                                CookieName = cookie.CookieName,
                                Price = cookie.Price,
                                Quantity = qty,
                                ImagePath = cookie.ImagePath ?? "/images/logo.png"
                            });
                        }
                        itemsAdded += qty;
                    }
                }
            }

            if (itemsAdded > 0)
            {
                HttpContext.Session.Set("Cart", cart);
                TempData["Success"] = $"¡Genial! Se agregaron {itemsAdded} galletas. Finaliza tu compra ahora.";
                // FLUJO MEJORADO: Ir directo al checkout
                return RedirectToAction(nameof(Checkout)); 
            }
            else
            {
                TempData["Error"] = "No seleccionaste ninguna galleta válida.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Store/Checkout
        [Authorize] // <--- Obliga a iniciar sesión
        public async Task<IActionResult> Checkout()
        {
            // 1. Recuperar carrito
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart");
            if (cart == null || !cart.Any())
            {
                TempData["Error"] = "Tu carrito está vacío.";
                return RedirectToAction(nameof(Index));
            }

            // 2. Obtener datos del usuario logueado
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // 3. Preparar el formulario con datos pre-cargados
            var viewModel = new CheckoutViewModel
            {
                CustomerName = $"{user.FirstName} {user.LastName}",
                Email = user.Email ?? "",
                Phone = user.PhoneNumber ?? "", 
                BillingType = "cash", // Valor por defecto
                ShippingType = "on campus", // Valor por defecto
                ShippingSite = "", // El usuario debe llenar esto
                TotalItems = cart.Sum(x => x.Quantity),
                TotalAmount = cart.Sum(x => x.Total)
            };

            return View(viewModel);
        }

        // ============================================================================
        // POST: Store/Checkout - EL PROCESO MÁS IMPORTANTE DE TODO EL SISTEMA 💳
        // ============================================================================
        // Qué hace esto? (Prepárate, es largo):
        // 1. Valida que el carrito exista (no puedes comprar nada si no hay nada)
        // 2. Obtiene el usuario logueado (necesitamos saber quién eres)
        // 3. Auto-registra al cliente si es su primer pedido (magia ✨)
        // 4. Crea o encuentra el método de pago (efectivo/tarjeta)
        // 5. Crea o encuentra el método de envío (on campus/outside)
        // 6. Crea la orden con estado PENDING (aún no se descuenta inventario)
        // 7. Guarda los items del pedido (OrderDetails)
        // 8. Relaciona el cliente con el billing (CustomerBillings)
        // 9. Relaciona el cliente con el shipping (CustomerShippings)
        // 10. Actualiza el teléfono si cambió
        // 11. Guarda TODO en la BD
        // 12. Limpia el carrito de la sesión (adiós carrito 👋)
        // 13. Redirige a la página de confirmación
        //
        // NOTA IMPORTANTE: En estado PENDING no se descuenta inventario.
        // El inventario se descuenta cuando el admin cambia el estado a "on_preparation".
        // Esto evita que alguien haga 100 pedidos y nos deje sin stock sin pagar.
        // ============================================================================
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel model)
        {
            // 1. Validar carrito de nuevo
            var cart = HttpContext.Session.Get<List<CartItem>>("Cart");
            if (cart == null || !cart.Any()) 
            {
                TempData["Error"] = "Tu carrito está vacío.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid) 
            {
                // Recargar totales en caso de error
                model.TotalAmount = cart.Sum(x => x.Total);
                model.TotalItems = cart.Sum(x => x.Quantity);
                return View(model);
            }

            // 2. Obtener Usuario
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();
            
            // 3. AUTO-REGISTRO DE CLIENTE
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);
            if (customer == null)
            {
                customer = new Customer { UserId = user.Id };
                _context.Customers.Add(customer);
                await _context.SaveChangesAsync();
            }

            // ============================================================================
            // 4. PROCESAR BILLING (Método de Pago)
            // ============================================================================
            var billing = await _context.Billings
                .FirstOrDefaultAsync(b => b.BillingType == model.BillingType);
            
            if (billing == null)
            {
                // Crear nuevo método de pago si no existe
                billing = new Billing { BillingType = model.BillingType };
                _context.Billings.Add(billing);
                await _context.SaveChangesAsync();
            }

            // ============================================================================
            // 5. PROCESAR SHIPPING (Método de Envío)
            // ============================================================================
            var shipping = await _context.Shippings
                .FirstOrDefaultAsync(s => 
                    s.ShippingType == model.ShippingType && 
                    s.ShippingSite == model.ShippingSite);
            
            if (shipping == null)
            {
                // Crear nuevo registro de shipping
                shipping = new Shipping 
                { 
                    ShippingType = model.ShippingType,
                    ShippingSite = model.ShippingSite
                };
                _context.Shippings.Add(shipping);
                await _context.SaveChangesAsync();
            }

            // ============================================================================
            // 6. CREAR LA ORDEN
            // ============================================================================
            var totalCookies = cart.Sum(x => x.Quantity);
            var order = new Order
            {
                CustomerId = customer.CustomerId,
                Status = "pending",
                Bag = totalCookies <= 2 ? "small" : "medium", // Lógica automática
                Sticker = totalCookies >= 3, // Lógica automática
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync(); // Guardamos para obtener OrderId

            // ============================================================================
            // 7. GUARDAR ORDER DETAILS (Items del pedido)
            // ============================================================================
            foreach (var item in cart)
            {
                var detail = new OrderDetail
                {
                    OrderId = order.OrderId,
                    CookieCode = item.CookieCode,
                    Qty = item.Quantity,
                    UnitPrice = item.Price
                };
                _context.OrderDetails.Add(detail);
            }
            await _context.SaveChangesAsync(); // Guardamos los detalles

            // ============================================================================
            // 8. RELACIONAR CUSTOMER CON BILLING (Tabla pivot)
            // ============================================================================
            // Nota: Asumimos que cada OrderDetail necesita su propio CustomerBilling
            // Si prefieres una relación 1-a-1 con Order, modifica esta lógica
            foreach (var detail in _context.OrderDetails.Where(od => od.OrderId == order.OrderId))
            {
                var customerBilling = new CustomerBilling
                {
                    CustomerId = customer.CustomerId,
                    BillingId = billing.BillingId,
                    OrderDetailId = detail.OrderDetailId,
                    Amount = detail.Qty * detail.UnitPrice
                };
                _context.CustomerBillings.Add(customerBilling);
            }

            // ============================================================================
            // 9. RELACIONAR CUSTOMER CON SHIPPING (Tabla pivot)
            // ============================================================================
            foreach (var detail in _context.OrderDetails.Where(od => od.OrderId == order.OrderId))
            {
                var customerShipping = new CustomerShipping
                {
                    CustomerId = customer.CustomerId,
                    ShippingId = shipping.ShippingId,
                    OrderDetailId = detail.OrderDetailId
                };
                _context.CustomerShippings.Add(customerShipping);
            }

            // ============================================================================
            // 10. ACTUALIZAR TELÉFONO SI CAMBIÓ
            // ============================================================================
            if (user.PhoneNumber != model.Phone)
            {
                user.PhoneNumber = model.Phone;
                await _userManager.UpdateAsync(user);
            }

            // ============================================================================
            // 11. GUARDAR TODO
            // ============================================================================
            await _context.SaveChangesAsync();

            // ============================================================================
            // 12. LIMPIAR CARRITO
            // ============================================================================
            HttpContext.Session.Remove("Cart");

            // ============================================================================
            // 13. MENSAJE DE ÉXITO
            // ============================================================================
            TempData["Success"] = $"¡Pedido #{order.OrderId} confirmado! " +
                                  $"Método de pago: {billing.BillingType}. " +
                                  $"Entrega: {shipping.ShippingType} en {shipping.ShippingSite}.";

            return RedirectToAction(nameof(OrderConfirmation), new { id = order.OrderId });
        }

        // GET: Store/OrderConfirmation/5
        [Authorize]
        public IActionResult OrderConfirmation(int id)
        {
            return View(id);
        }

        // GET: Store/MyOrders
        [Authorize]
        public async Task<IActionResult> MyOrders()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Buscar el Customer asociado al Usuario
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.UserId == user.Id);

            // Si no es cliente aún, retornamos lista vacía
            if (customer == null) return View(new List<Order>());

            // Traer órdenes con sus detalles
            var orders = await _context.Orders
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.CookieCodeNavigation) // Para mostrar nombres de galletas
                .Where(o => o.CustomerId == customer.CustomerId)
                .OrderByDescending(o => o.CreatedAt) // Las más recientes primero
                .ToListAsync();

            return View(orders);
        }
    }
}   
