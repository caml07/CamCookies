namespace cmcookies.Models.Factories;

// ============================================================================
// COOKIE FACTORY - Patrón de Diseño Factory 🏭
// ============================================================================
// ¿Qué es el Factory Pattern?
// Es como una fábrica de galletas real: tú le dices qué tipo de galleta quieres
// y la fábrica se encarga de crearla con todos los ingredientes correctos.
//
// ¿Por qué usamos Factory en vez de simplemente hacer "new Cookie()"?
// 1. Encapsulación: La lógica de creación está en UN solo lugar
// 2. Flexibilidad: Puedo cambiar cómo se crean las galletas sin tocar todo el código
// 3. Consistencia: Todas las galletas se crean de la misma manera
// 4. Testing: Puedo hacer mock de la factory fácilmente
// 5. Principio SOLID: Single Responsibility (cada clase hace UNA cosa)
//
// Ejemplo de uso:
// var factory = new CookieFactory();
// var galleta = factory.CreateNormalCookie("ORE001", "Oreo", "Deliciosa", 70, 20);
//
// En vez de:
// var galleta = new Cookie { 
//     CookieCode = "ORE001", 
//     CookieName = "Oreo", 
//     Category = "normal",  // ← Puedo olvidar esto
//     IsActive = true,      // ← O esto
//     CreatedAt = DateTime.Now  // ← O esto
// };
// ============================================================================

/// <summary>
/// Factory concreto que implementa la lógica de creación de Cookies.
/// Encapsula las reglas de negocio para crear diferentes tipos de cookies.
/// </summary>
public class CookieFactory : ICookieFactory
{
  /// <summary>
  /// Crea una cookie Normal con valores por defecto seguros.
  /// </summary>
  public Cookie CreateNormalCookie(string cookieCode, string name, string description, decimal price, int stock)
  {
    return new Cookie
    {
      CookieCode = cookieCode,
      CookieName = name,
      Description = description,
      Price = price,
      Stock = stock,
      Category = "normal",
      IsActive = true,
      CreatedAt = DateTime.Now,
      UpdatedAt = DateTime.Now,
      ImagePath = null // Se asignará después si se sube imagen
    };
  }

  /// <summary>
  /// Crea una cookie Seasonal (de temporada).
  /// </summary>
  public Cookie CreateSeasonalCookie(string cookieCode, string name, string description, decimal price, int stock)
  {
    return new Cookie
    {
      CookieCode = cookieCode,
      CookieName = name,
      Description = description,
      Price = price,
      Stock = stock,
      Category = "seasonal",
      IsActive = true,
      CreatedAt = DateTime.Now,
      UpdatedAt = DateTime.Now,
      ImagePath = null
    };
  }

  /// <summary>
  /// Crea una cookie desde un ViewModel.
  /// Decide automáticamente si es normal o seasonal según el ViewModel.
  /// </summary>
  public Cookie CreateFromViewModel(ViewModels.Cookie.CookieViewModel viewModel)
  {
    // Decisión: usar el método apropiado según la categoría
    if (viewModel.Category?.ToLower() == "seasonal")
      return CreateSeasonalCookie(
        viewModel.CookieCode,
        viewModel.CookieName,
        viewModel.Description ?? string.Empty,
        viewModel.Price,
        viewModel.Stock ?? 0
      );

    return CreateNormalCookie(
      viewModel.CookieCode,
      viewModel.CookieName,
      viewModel.Description ?? string.Empty,
      viewModel.Price,
      viewModel.Stock ?? 0
    );
  }

  /// <summary>
  /// Actualiza una cookie existente con datos del ViewModel.
  /// NO crea nueva instancia, solo modifica la existente.
  /// </summary>
  public void UpdateFromViewModel(Cookie cookie, ViewModels.Cookie.CookieViewModel viewModel)
  {
    // El CookieCode no se debe actualizar una vez creado.
    cookie.CookieName = viewModel.CookieName;
    cookie.Description = viewModel.Description;
    cookie.Price = viewModel.Price;
    cookie.Stock = viewModel.Stock;
    cookie.Category = viewModel.Category;
    cookie.IsActive = viewModel.IsActive;
    cookie.UpdatedAt = DateTime.Now;
    // ImagePath se actualiza por separado si hay nueva imagen
  }
}