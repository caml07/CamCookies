using System.ComponentModel.DataAnnotations;

namespace cmcookies.Models.ViewModels.Store
{
  public class CheckoutViewModel
  {
    // ===== DATOS DEL CLIENTE =====
    [Display(Name = "Nombre Completo")]
    public string CustomerName { get; set; } = string.Empty;

    [EmailAddress(ErrorMessage = "El correo no es válido.")]
    [Display(Name = "Correo Electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Necesitamos un teléfono para contactarte.")]
    [Phone]
    [Display(Name = "Teléfono de Contacto")]
    public string Phone { get; set; } = string.Empty;

    // ===== MÉTODO DE PAGO =====
    [Required(ErrorMessage = "Selecciona un método de pago.")]
    [Display(Name = "Método de Pago")]
    public string BillingType { get; set; } = "cash"; // 'cash' o 'card'

    // ===== MÉTODO DE ENVÍO =====
    [Required(ErrorMessage = "Selecciona dónde quieres recibir tu pedido.")]
    [Display(Name = "Tipo de Envío")]
    public string ShippingType { get; set; } = "on campus"; // 'on campus' o 'outside campus'

    [Required(ErrorMessage = "Especifica el lugar de entrega.")]
    [Display(Name = "Lugar de Entrega")]
    [StringLength(100, ErrorMessage = "Máximo 100 caracteres.")]
    public string ShippingSite { get; set; } = string.Empty;

    // ===== NOTAS ADICIONALES =====
    [Display(Name = "Notas / Instrucciones Especiales")]
    [StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
    public string? Notes { get; set; }
        
    // ===== RESUMEN DE LA ORDEN (Solo lectura) =====
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
  }
}
