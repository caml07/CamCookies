using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmcookies.Models;
using cmcookies.Models.Store;
using cmcookies.Extensions; // Para usar nuestros helpers

namespace cmcookies.Controllers;

public class StoreController : Controller
{
  private readonly CmcDBContext _context;

  public StoreController(CmcDBContext context)
  {
    _context = context;
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
          TempData["Success"] = $"¡Listo! Se agregaron {itemsAdded} galletas a tu orden.";
      }
      else
      {
          TempData["Error"] = "No seleccionaste ninguna galleta o no hay stock suficiente.";
      }

      return RedirectToAction(nameof(Index));
  }
}