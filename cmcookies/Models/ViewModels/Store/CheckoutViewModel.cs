using System.ComponentModel.DataAnnotations;

namespace cmcookies.Models.ViewModels.Store
{
  public class CheckoutViewModel
  {
    [Display(Name = "Nombre Completo")]
    public string CustomerName { get; set; } = string.Empty;

    [Display(Name = "Correo Electrónico")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Necesitamos un teléfono para contactarte.")]
    [Phone]
    [Display(Name = "Teléfono de Contacto")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Notas / Instrucciones Especiales")]
    public string? Notes { get; set; }
        
    // Resumen de la orden (Solo lectura)
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
  }
}