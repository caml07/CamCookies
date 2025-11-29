namespace cmcookies.Models.ViewModels.Customer;

public class OrderDetailsViewModel //practicamente como un recibo que ofrece un resumen detallado de la orden. Aca obtiene el Id de la orde, su estatus y cuando fue creado
{
    public int OrderId { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }
    
    //Informacion del cliente
    public string CustomerName { get; set; }
    public string CustomerEmail { get; set; }
    public string CustomerPhone { get; set; }
    
    //Items de la orden
    public List<OrderItemViewModel> Items { get; set; } = new List<OrderItemViewModel>();
    
    // Bag y sticker
    public string BagSize { get; set; }
    public bool HasSticker { get; set; }
    
    // Billing
    public string BillingType { get; set; }
    public decimal TotalAmount { get; set; }
    
    // Shipping
    public string ShippingType { get; set; }
    public string ShippingSite { get; set; }
}

public class OrderItemViewModel
{
    public string CookieName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}
