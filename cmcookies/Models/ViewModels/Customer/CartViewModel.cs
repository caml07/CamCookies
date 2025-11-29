namespace cmcookies.Models.ViewModels.Customer;

public class CartViewModel
{
    // Lista de todos los items en el carrito (cada item es un CartItemViewModel)
    public List<CartItemViewModel> Items { get; set; } = new List<CartItemViewModel>();

    // Propiedades calculadas que se actualizan automáticamente cuando cambia Items
    
    // Suma la cantidad total de galletas en el carrito (ejemplo: 3 Chocolate + 2 Oatmeal = 5 total)
    public int TotalCookies => Items.Sum(i => i.Quantity);
    
    // Suma el subtotal de todos los items (precio × cantidad de cada item)
    public decimal Subtotal => Items.Sum(i => i.Subtotal);
    
    // Lógica de negocio de Cam Cookies para determinar el tamaño de bolsa
    // 1-2 galletas → bolsa pequeña, 3+ galletas → bolsa mediana
    public string BagSize => TotalCookies >= 3 ? "medium" : "small";
    
    // Solo se incluye sticker si el cliente compra 3 o más galletas
    public bool HasSticker => TotalCookies >= 3;
    
    // Costo de la bolsa según el tamaño (estos valores podrían venir de la BD en el futuro)
    public decimal BagCost => BagSize == "medium" ? 3.50m : 1.50m;
    
    // Costo del sticker (C$0.60 si HasSticker es true, C$0.00 si es false)
    public decimal StickerCost => HasSticker ? 0.60m : 0.00m;
    
    // Total final: Subtotal de galletas + costo de bolsa + costo de sticker
    public decimal Total => Subtotal + BagCost + StickerCost;
}
