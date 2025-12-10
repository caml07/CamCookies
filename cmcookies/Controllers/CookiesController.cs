using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using cmcookies.Models;
using cmcookies.Models.Factories;
using cmcookies.Models.ViewModels.Cookie;

namespace cmcookies.Controllers;

/// <summary>
/// Controller para el CRUD completo de Cookies.
/// Utiliza Factory Pattern para la creación de cookies.
/// Solo accesible por Administradores.
/// </summary>
[Authorize(Roles = "Admin")]
public class CookiesController : Controller
{
  private readonly CmcDBContext _context;
  private readonly ICookieFactory _cookieFactory;
  private readonly IWebHostEnvironment _webHostEnvironment;

  // Configuración de uploads
  private const long MAX_FILE_SIZE = 5 * 1024 * 1024; // MAX  5 MB
  private readonly string[] ALLOWED_EXTENSIONS = { ".jpg", ".jpeg", ".png", ".webp" };

  /// <summary>
  /// Constructor con Dependency Injection.
  /// </summary>
  /// <param name="context">DbContext para acceso a la base de datos</param>
  /// <param name="cookieFactory">Factory para crear cookies (patrón Factory)</param>
  /// <param name="webHostEnvironment">Para obtener la ruta wwwroot</param>
  public CookiesController(
    CmcDBContext context,
    ICookieFactory cookieFactory,
    IWebHostEnvironment webHostEnvironment)
  {
    _context = context;
    _cookieFactory = cookieFactory;
    _webHostEnvironment = webHostEnvironment;
  }

  // ========================================================================
  // INDEX - Lista todas las cookies
  // ========================================================================

  /// <summary>
  /// GET: /Cookies
  /// Muestra lista de todas las cookies con su información.
  /// </summary>
  public async Task<IActionResult> Index()
  {
    // Obtener todas las cookies ordenadas por nombre
    var cookies = await _context.Cookies
      .OrderBy(c => c.CookieName)
      .ToListAsync();

    return View(cookies);
  }

  // ========================================================================
  // CREATE - Crear nueva cookie
  // ========================================================================

  /// <summary>
  /// GET: /Cookies/Create
  /// Muestra el formulario para crear una nueva cookie.
  /// </summary>
  public IActionResult Create()
  {
    // Crear ViewModel vacío con valores por defecto
    var viewModel = new CookieViewModel
    {
      Category = "normal",
      Stock = 0,
      IsActive = true
    };

    return View(viewModel);
  }

  /// <summary>
  /// POST: /Cookies/Create
  /// Procesa la creación de una nueva cookie.
  /// </summary>
  /// <param name="viewModel">Datos del formulario</param>
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Create(CookieViewModel viewModel)
  {
    // Validar que el modelo sea válido
    if (!ModelState.IsValid) return View(viewModel);

    // Validar imagen si se subió
    if (viewModel.ImageFile != null)
    {
      var imageValidation = ValidateImage(viewModel.ImageFile);
      if (!imageValidation.IsValid)
      {
        ModelState.AddModelError("ImageFile", imageValidation.ErrorMessage!);
        return View(viewModel);
      }
    }

    try
    {
      // Verificar si ya existe una cookie con el mismo CookieCode
      if (await _context.Cookies.AnyAsync(c => c.CookieCode == viewModel.CookieCode))
      {
        ModelState.AddModelError("CookieCode", "Ya existe una cookie con este código. Por favor, elija otro.");
        return View(viewModel);
      }

      // ===== FACTORY PATTERN EN ACCIÓN =====
      // En lugar de: var cookie = new Cookie() { ... }
      // Usamos el Factory que encapsula la lógica de creación
      var cookie = _cookieFactory.CreateFromViewModel(viewModel);

      // Guardar imagen si se subió
      if (viewModel.ImageFile != null)
      {
        var imagePath = await SaveImageAsync(viewModel.ImageFile);
        cookie.ImagePath = imagePath;
      }

      // Agregar a la base de datos
      _context.Cookies.Add(cookie);
      await _context.SaveChangesAsync();

      TempData["SuccessMessage"] = $"Cookie '{cookie.CookieName}' creada exitosamente!";
      return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
      ModelState.AddModelError("", $"Error al crear la cookie: {ex.Message}");
      return View(viewModel);
    }
  }

  // ========================================================================
  // EDIT - Editar cookie existente
  // ========================================================================

  /// <summary>
  /// GET: /Cookies/Edit/ABC
  /// Muestra el formulario para editar una cookie existente.
  /// </summary>
  /// <param name="cookieCode">Código de la cookie a editar</param>
  public async Task<IActionResult> Edit(string? cookieCode)
  {
    if (string.IsNullOrEmpty(cookieCode)) return NotFound();

    var cookie = await _context.Cookies.FindAsync(cookieCode);

    if (cookie == null) return NotFound();

    // Mapear Cookie → CookieViewModel
    var viewModel = new CookieViewModel
    {
      CookieCode = cookie.CookieCode,
      CookieName = cookie.CookieName,
      Description = cookie.Description,
      Price = cookie.Price,
      Category = cookie.Category,
      Stock = cookie.Stock,
      IsActive = cookie.IsActive ?? true,
      CurrentImagePath = cookie.ImagePath,
      CreatedAt = cookie.CreatedAt,
      UpdatedAt = cookie.UpdatedAt
    };

    return View(viewModel);
  }

  /// <summary>
  /// POST: /Cookies/Edit/ABC
  /// Procesa la actualización de una cookie.
  /// </summary>
  /// <param name="cookieCode">Código de la cookie</param>
  /// <param name="viewModel">Datos actualizados del formulario</param>
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(string cookieCode, CookieViewModel viewModel)
  {
    if (cookieCode != viewModel.CookieCode) return NotFound();

    if (!ModelState.IsValid) return View(viewModel);

    // Validar nueva imagen si se subió
    if (viewModel.ImageFile != null)
    {
      var imageValidation = ValidateImage(viewModel.ImageFile);
      if (!imageValidation.IsValid)
      {
        ModelState.AddModelError("ImageFile", imageValidation.ErrorMessage!);
        return View(viewModel);
      }
    }

    try
    {
      // Obtener la cookie existente de la base de datos
      var cookie = await _context.Cookies.FindAsync(cookieCode);

      if (cookie == null) return NotFound();

      // Guardar imagen anterior por si hay que eliminarla
      var oldImagePath = cookie.ImagePath;

      // ===== FACTORY PATTERN PARA UPDATE =====
      // El Factory actualiza los campos del objeto existente
      _cookieFactory.UpdateFromViewModel(cookie, viewModel);

      // Si se subió nueva imagen, guardarla y eliminar la anterior
      if (viewModel.ImageFile != null)
      {
        var newImagePath = await SaveImageAsync(viewModel.ImageFile);
        cookie.ImagePath = newImagePath;

        // Eliminar imagen anterior si existe
        if (!string.IsNullOrEmpty(oldImagePath)) DeleteImage(oldImagePath);
      }

      // Actualizar en la base de datos
      _context.Update(cookie);
      await _context.SaveChangesAsync();

      TempData["SuccessMessage"] = $"Cookie '{cookie.CookieName}' actualizada exitosamente!";
      return RedirectToAction(nameof(Index));
    }
    catch (DbUpdateConcurrencyException)
    {
      if (!CookieExists(cookieCode)) return NotFound();
      throw;
    }
    catch (Exception ex)
    {
      ModelState.AddModelError("", $"Error al actualizar la cookie: {ex.Message}");
      return View(viewModel);
    }
  }

  // ========================================================================
  // DELETE - Eliminar cookie
  // ========================================================================

  /// <summary>
  /// GET: /Cookies/Delete/ABC
  /// Muestra la confirmación para eliminar una cookie.
  /// </summary>
  /// <param name="cookieCode">Código de la cookie a eliminar</param>
  public async Task<IActionResult> Delete(string? cookieCode)
  {
    if (string.IsNullOrEmpty(cookieCode)) return NotFound();

    var cookie = await _context.Cookies
      .FirstOrDefaultAsync(m => m.CookieCode == cookieCode);

    if (cookie == null) return NotFound();

    return View(cookie);
  }

  /// <summary>
  /// POST: /Cookies/Delete/ABC
  /// Confirma y ejecuta la eliminación de la cookie.
  /// 
  /// BUSINESS RULE: No se puede eliminar una cookie si tiene pedidos activos.
  /// Un pedido activo es aquel que está en estado:
  /// - "pending" (aún no se prepara, pero el cliente lo solicitó)
  /// - "on_preparation" (ya en proceso, inventario comprometido)
  /// 
  /// Esta validación evita:
  /// 1. Referencias rotas en OrderDetails (FK a Cookie inexistente)
  /// 2. Errores al procesar pedidos (al intentar descontar inventario)
  /// 3. Pérdida de datos históricos de pedidos
  /// </summary>
  /// <param name="cookieCode">Código de la cookie a eliminar</param>
  [HttpPost]
  [ActionName("Delete")]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> DeleteConfirmed(string cookieCode)
  {
    try
    {
      var cookie = await _context.Cookies.FindAsync(cookieCode);

      if (cookie == null) return NotFound();

      // ===== VALIDACIÓN CRÍTICA: Verificar pedidos activos =====
      // ¿Hay OrderDetails con esta Cookie en pedidos pending o on_preparation?
      var hasActiveOrders = await _context.OrderDetails
        .Include(od => od.Order)
        .AnyAsync(od => od.CookieCode == cookieCode && 
                       (od.Order.Status == "pending" || od.Order.Status == "on_preparation"));

      if (hasActiveOrders)
      {
        // Contar cuántos pedidos activos hay
        var activeOrdersCount = await _context.OrderDetails
          .Include(od => od.Order)
          .Where(od => od.CookieCode == cookieCode && 
                      (od.Order.Status == "pending" || od.Order.Status == "on_preparation"))
          .Select(od => od.Order.OrderId)
          .Distinct()
          .CountAsync();

        TempData["ErrorMessage"] = 
          $"❌ No se puede eliminar '{cookie.CookieName}' porque tiene {activeOrdersCount} " +
          $"pedido(s) activo(s). <br/><br/>" +
          $"<strong>Opciones:</strong><br/>" +
          $"1️⃣ Espera a que los pedidos se completen o cancelen<br/>" +
          $"2️⃣ Deshabilita la cookie (editar > Activo = OFF) para que no se puedan hacer nuevos pedidos";

        return RedirectToAction(nameof(Index));
      }

      // ===== Si no hay pedidos activos, permitir eliminación =====

      // Verificar si hay pedidos históricos (delivered o cancelled)
      var hasHistoricalOrders = await _context.OrderDetails
        .AnyAsync(od => od.CookieCode == cookieCode);

      if (hasHistoricalOrders)
      {
        // Advertir pero permitir (los pedidos delivered/cancelled son históricos)
        TempData["WarningMessage"] = 
          $"⚠️ Esta cookie tiene pedidos históricos (entregados/cancelados). " +
          $"Se eliminará pero el historial quedará con referencias a esta cookie.";
      }

      // Eliminar imagen del disco si existe
      if (!string.IsNullOrEmpty(cookie.ImagePath)) DeleteImage(cookie.ImagePath);

      // Eliminar de la base de datos
      _context.Cookies.Remove(cookie);
      await _context.SaveChangesAsync();

      TempData["SuccessMessage"] = $"Cookie '{cookie.CookieName}' eliminada exitosamente! ✅";
      return RedirectToAction(nameof(Index));
    }
    catch (Exception ex)
    {
      TempData["ErrorMessage"] = $"Error al eliminar la cookie: {ex.Message}";
      return RedirectToAction(nameof(Index));
    }
  }

  // ========================================================================
  // RECIPE MANAGEMENT - Administración de recetas
  // ========================================================================

  // GET: Cookies/Recipe/5
  public async Task<IActionResult> Recipe(string id)
  {
    if (id == null) return NotFound();

    var cookie = await _context.Cookies
      .Include(c => c.CookieMaterials)
      .ThenInclude(cm => cm.Material)
      .FirstOrDefaultAsync(m => m.CookieCode == id);

    if (cookie == null) return NotFound();

    // Preparamos el ViewModel
    var viewModel = new RecipeViewModel
    {
      CookieCode = cookie.CookieCode,
      CookieName = cookie.CookieName,
      CookieImage = cookie.ImagePath,
      Ingredients = cookie.CookieMaterials.Select(cm => new RecipeItem
      {
        CookieMaterialId = cm.CookieMaterialId,
        MaterialName = cm.Material.Name,
        Unit = cm.Material.Unit,
        Quantity = cm.ConsumptionPerBatch,
        CostCalculated = cm.ConsumptionPerBatch * cm.Material.UnitCost
      }).ToList(),
      // Cargamos la lista de materiales para el dropdown (excluyendo los que ya están en la receta si quisieras ser muy pro, pero por ahora todos)
      MaterialsList = new SelectList(_context.Materials.OrderBy(m => m.Name), "MaterialId", "Name")
    };

    return View(viewModel);
  }

  // POST: Cookies/AddIngredient
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> AddIngredient(RecipeViewModel model)
  {
    // Validar que no estemos duplicando el material
    var exists = await _context.CookieMaterials
      .AnyAsync(cm => cm.CookieCode == model.CookieCode && cm.MaterialId == model.NewMaterialId);

    if (exists)
    {
      TempData["ErrorMessage"] = "Este material ya está en la receta. Edítalo o bórralo si deseas cambiarlo.";
      return RedirectToAction(nameof(Recipe), new { id = model.CookieCode });
    }

    if (model.NewQuantity > 0)
    {
      var newIngredient = new CookieMaterial
      {
        CookieCode = model.CookieCode,
        MaterialId = model.NewMaterialId,
        ConsumptionPerBatch = model.NewQuantity
      };

      _context.Add(newIngredient);
      await _context.SaveChangesAsync();
      TempData["SuccessMessage"] = "Ingrediente agregado a la receta.";
    }

    return RedirectToAction(nameof(Recipe), new { id = model.CookieCode });
  }

  // POST: Cookies/RemoveIngredient
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> RemoveIngredient(int id)
  {
    var ingredient = await _context.CookieMaterials.FindAsync(id);
    if (ingredient != null)
    {
      var cookieCode = ingredient.CookieCode; // Guardamos ID para el redirect
      _context.CookieMaterials.Remove(ingredient);
      await _context.SaveChangesAsync();
      TempData["SuccessMessage"] = "Ingrediente eliminado.";
      return RedirectToAction(nameof(Recipe), new { id = cookieCode });
    }

    return RedirectToAction(nameof(Index));
  }


  // ========================================================================
  // HELPER METHODS - Métodos auxiliares privados
  // ========================================================================

  /// <summary>
  /// Valida que una imagen cumpla con los requisitos.
  /// </summary>
  /// <param name="file">Archivo a validar</param>
  /// <returns>Resultado de la validación</returns>
  private (bool IsValid, string? ErrorMessage) ValidateImage(IFormFile file)
  {
    // Validar tamaño
    if (file.Length > MAX_FILE_SIZE) return (false, $"La imagen no puede superar {MAX_FILE_SIZE / 1024 / 1024} MB");

    // Validar extensión
    var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
    if (!ALLOWED_EXTENSIONS.Contains(extension))
      return (false, $"Solo se permiten imágenes: {string.Join(", ", ALLOWED_EXTENSIONS)}");

    return (true, null);
  }

  /// <summary>
  /// Guarda una imagen en el servidor y retorna su path relativo.
  /// </summary>
  /// <param name="file">Archivo de imagen</param>
  /// <returns>Path relativo de la imagen (ej: /images/cookies/abc123.jpg)</returns>
  private async Task<string> SaveImageAsync(IFormFile file)
  {
    // Generar nombre único para evitar colisiones
    var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

    // Path completo en el servidor
    var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "cookies");

    // Crear carpeta si no existe
    if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

    // Guardar el archivo
    using (var fileStream = new FileStream(filePath, FileMode.Create))
    {
      await file.CopyToAsync(fileStream);
    }

    // Retornar path relativo para guardarlo en DB
    return $"/images/cookies/{uniqueFileName}";
  }

  /// <summary>
  /// Elimina una imagen del servidor.
  /// </summary>
  /// <param name="imagePath">Path relativo de la imagen</param>
  private void DeleteImage(string imagePath)
  {
    try
    {
      // Convertir path relativo a absoluto
      var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, imagePath.TrimStart('/'));

      if (System.IO.File.Exists(fullPath)) System.IO.File.Delete(fullPath);
    }
    catch (Exception)
    {
      // Log error pero no fallar la operación
      // En producción deberías usar un logger real
    }
  }

  /// <summary>
  /// Verifica si una cookie existe en la base de datos.
  /// </summary>
  /// <param name="cookieCode">Código de la cookie a verificar</param>
  /// <returns>True si existe, false si no</returns>
  private bool CookieExists(string cookieCode)
  {
    return _context.Cookies.Any(e => e.CookieCode == cookieCode);
  }
}