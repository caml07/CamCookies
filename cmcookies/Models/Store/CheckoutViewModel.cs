using System.ComponentModel.DataAnnotations;

namespace cmcookies.Models.Store;

public class CheckoutViewModel
{
    [Display(Name = "Nombre Completo")]
    [Required(ErrorMessage = "El nombre es obligatorio.")]
    public string CustomerName { get; set; }

    [Display(Name = "Correo Electrónico")]
    [Required(ErrorMessage = "El email es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del email no es válido.")]
    public string Email { get; set; }

    [Display(Name = "Teléfono de Contacto")]
    [Required(ErrorMessage = "El teléfono es obligatorio.")]
    [Phone(ErrorMessage = "El formato del teléfono no es válido.")]
    public string Phone { get; set; }

    // --- Datos para mostrar en el resumen, no son parte del form ---
    public int TotalItems { get; set; }
    public decimal TotalAmount { get; set; }
}
