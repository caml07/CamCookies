using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmcookies.Models;
using cmcookies.Models.Store;
using cmcookies.Extensions; // Para usar nuestros helpers

namespace cmcookies.Controllers;

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
    // (Regla de negocio: Si no hay stock, no se vende)
    var cookies = await _context.Cookies
      .Where(c => c.IsActive == true && c.Stock > 0)
      .OrderByDescending(c => c.Category) // Seasonal primero
      .ToListAsync();

    return View(cookies);
  }

  // POST: Agregar al Carrito
  [HttpPost]
  public async Task<IActionResult> AddToCart(string cookieCode, int qty)
  {
    var cookie = await _context.Cookies.FindAsync(cookieCode);
    if (cookie == null || cookie.Stock < qty)
    {
      TempData["Error"] = "Stock insuficiente o producto no válido.";
      return RedirectToAction(nameof(Index));
    }

    // 1. Recuperar carrito actual de la sesión
    var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();

    // 2. Buscar si ya existe el producto
    var item = cart.FirstOrDefault(c => c.CookieCode == cookieCode);

    if (item != null)
      item.Quantity += qty; // Si existe, sumamos
    else
      // Si no, agregamos nuevo
      cart.Add(new CartItem
      {
        CookieCode = cookie.CookieCode,
        CookieName = cookie.CookieName,
        Price = cookie.Price,
        Quantity = qty,
        ImagePath = cookie.ImagePath ?? "/images/default-cookie.png"
      });

    // 3. Guardar carrito actualizado en sesión
    HttpContext.Session.Set("Cart", cart);

    TempData["Success"] = $"Agregaste {qty}x {cookie.CookieName} al carrito.";
    return RedirectToAction(nameof(Index));
  }

  // POST: Store/AddBulkToCart
  [HttpPost]
  public async Task<IActionResult> AddBulkToCart(Dictionary<string, int> products)
  {
      // 'products' es un diccionario: Clave = CookieCode, Valor = Cantidad
      
      // 1. Recuperar carrito actual
      var cart = HttpContext.Session.Get<List<CartItem>>("Cart") ?? new List<CartItem>();
      int itemsAdded = 0;

      // 2. Iterar sobre lo que envió el usuario
      foreach (var entry in products)
      {
          string code = entry.Key;
          int qty = entry.Value;

          if (qty > 0) // Solo procesamos si pidió algo
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

      // 3. Guardar y notificar
      if (itemsAdded > 0)
      {
          HttpContext.Session.Set("Cart", cart);
      }
      else
      {
          TempData["Error"] = "No seleccionaste ninguna galleta o no hay stock suficiente.";
          return RedirectToAction(nameof(Index));
      }

      return RedirectToAction(nameof(Checkout));
  }

  // GET: Store/Checkout
  [Authorize] //Obliga a iniciar sesión
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
          Email = user.Email,
          Phone = user.PhoneNumber ?? "", // Si ya tiene teléfono, lo ponemos
          TotalItems = cart.Sum(x => x.Quantity),
          TotalAmount = cart.Sum(x => x.Total)
      };

      return View(viewModel);
  }

  // POST: Store/Checkout
  [HttpPost]
  [Authorize]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Checkout(CheckoutViewModel model)
  {
      // 1. Validar carrito de nuevo
      var cart = HttpContext.Session.Get<List<CartItem>>("Cart");
      if (cart == null || !cart.Any()) return RedirectToAction(nameof(Index));

      if (!ModelState.IsValid) return View(model);

      // 2. Obtener Usuario
      var user = await _userManager.GetUserAsync(User);
      if (user == null) return Challenge();
      
      // --- AUTO-REGISTRO DE CLIENTE ---
      // Buscamos si este usuario ya es un "Customer" en nuestra tabla de negocio
      var customer = await _context.Customers.FirstOrDefaultAsync(c => c.UserId == user.Id);

      if (customer == null)
      {
          // Si es la primera vez que compra, creamos su perfil de cliente
          customer = new Customer { UserId = user.Id };
          _context.Customers.Add(customer);
          await _context.SaveChangesAsync(); // Guardamos para tener el CustomerId
      }

      // 3. Crear la Orden
      var order = new Order
      {
          CustomerId = customer.CustomerId,
          Status = "pending", // Nace pendiente
          CreatedAt = DateTime.Now,
          UpdatedAt = DateTime.Now,
          Sticker = false // Se calcula en Admin al despachar
          // Bag: Se calcula en Admin
      };

      _context.Orders.Add(order);
      await _context.SaveChangesAsync(); // Guardamos para obtener OrderId

      // 4. Guardar los Detalles (Items)
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
      
      // Opcional: Guardar el teléfono actualizado si el usuario lo cambió en el form
      if (user.PhoneNumber != model.Phone)
      {
          user.PhoneNumber = model.Phone;
          await _userManager.UpdateAsync(user);
      }

      await _context.SaveChangesAsync();

      // 5. ¡LIMPIEZA! Borramos el carrito de la sesión
      HttpContext.Session.Remove("Cart");

      return RedirectToAction(nameof(OrderConfirmation), new { id = order.OrderId });
  }

  // GET: Store/OrderConfirmation/5
  [Authorize]
  public IActionResult OrderConfirmation(int id)
  {
      return View(id);
  }
}