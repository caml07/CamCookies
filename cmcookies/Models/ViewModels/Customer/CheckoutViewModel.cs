using System.ComponentModel.DataAnnotations;

namespace cmcookies.Models.ViewModels.Customer;

public class CheckoutViewModel
{
  //Obtiene la info que tiene Cart, para dar un resumen del carrito aca mismo
  public CartViewModel Cart { get; set; }

  // Billing information
  [Required(ErrorMessage = "The payment method is required")]
  [Display(Name = "Payment Method")]
  public string BillingType { get; set; } //Un campo necesario, que da el tipo de pago 'cash' o 'tarjeta'

  // Shipping information
  [Required(ErrorMessage = "The shipping type is required")]
  [Display(Name = "Shipping Type")]
  public string
    ShippingType { get; set; } //otro campo necesario, para saber donde val el pedido 'on campus' o 'outside campus'

  [Required(ErrorMessage = "The delivery location is required")]
  [StringLength(255, ErrorMessage = "The location cannot be longer than 255 characters")]
  [Display(Name = "Delivery Location")]
  public string
    ShippingSite { get; set; } //otro campo necesario para saber mas especificamente donde debe de ir el pedido

  // Optional: Customer notes
  [StringLength(500, ErrorMessage = "The notes cannot be longer than 500 characters")]
  [Display(Name = "Additional Notes (optional)")]
  public string
    Notes { get; set; } //un campo opcional que puede servir para que el usuario ponga algo extra si el desea
}