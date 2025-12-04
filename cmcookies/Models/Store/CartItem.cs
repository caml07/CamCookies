namespace cmcookies.Models.Store;

public class CartItem
{
  public string CookieCode { get; set; } = string.Empty;
  public string CookieName { get; set; } = string.Empty;
  public decimal Price { get; set; }
  public int Quantity { get; set; }
  public string ImagePath { get; set; } = string.Empty;

  // Propiedad calculada: Total de esta línea
  public decimal Total => Price * Quantity;
}