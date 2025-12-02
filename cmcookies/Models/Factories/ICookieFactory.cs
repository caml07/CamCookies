namespace cmcookies.Models.Factories;

/// <summary>
/// Interface para el Factory Pattern de creación de Cookies.
/// Esto nos permite tener múltiples implementaciones y facilita el testing.
/// </summary>
public interface ICookieFactory
{
  /// <summary>
  /// Crea una nueva cookie Normal (categoría por defecto).
  /// </summary>
  Cookie CreateNormalCookie(string cookieCode, string name, string description, decimal price, int stock);

  /// <summary>
  /// Crea una nueva cookie Seasonal (de temporada).
  /// </summary>
  Cookie CreateSeasonalCookie(string cookieCode, string name, string description, decimal price, int stock);

  /// <summary>
  /// Crea una cookie desde un ViewModel (usado en Create/Edit).
  /// </summary>
  Cookie CreateFromViewModel(ViewModels.Cookie.CookieViewModel viewModel);

  /// <summary>
  /// Actualiza una cookie existente con datos del ViewModel.
  /// </summary>
  void UpdateFromViewModel(Cookie cookie, ViewModels.Cookie.CookieViewModel viewModel);
}