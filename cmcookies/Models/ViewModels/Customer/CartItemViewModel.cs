namespace cmcookies.Models.ViewModels.Customer;

public class CartItemViewModel
{
    public int CookieCode { get; set; } //para que cuando el cliente quiera cambiar la cantidad o eliminar el producto del carrito, el sistema ocupa CookieCode para saber a que cookie se refiere
    public string CookieName { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string ImagePath { get; set; }
    public int AvailableStock { get; set; }

    //Una propiedad calculada que multiplica el precio de las galletas que el cliente lleva por la cantidad de esas galletas que el cliente lleva
    public decimal Subtotal => Price * Quantity;
}
