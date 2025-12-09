using System.ComponentModel.DataAnnotations;

namespace cmcookies.Models.ViewModels.Profile;

/// <summary>
/// ViewModel para editar datos básicos del perfil.
/// NO incluye la contraseña (eso es otra acción).
/// </summary>
public class EditProfileViewModel
{
    // ========================================================================
    // DATOS BÁSICOS
    // ========================================================================
    
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(50, ErrorMessage = "El nombre no puede tener más de 50 caracteres")]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido")]
    [StringLength(50, ErrorMessage = "El apellido no puede tener más de 50 caracteres")]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    // ========================================================================
    // EMAIL (Solo lectura - NO se puede cambiar)
    // ========================================================================
    // ¿Por qué no se puede cambiar?
    // Porque el email es el USERNAME en Identity y cambiarlo es más complejo
    // (requiere re-autenticación, verificación por email, etc.)
    // Para simplicidad, lo dejamos read-only
    // ========================================================================
    
    [Display(Name = "Correo Electrónico")]
    public string Email { get; set; } = string.Empty;

    // ========================================================================
    // TELÉFONOS (Se guardan en la tabla PHONES)
    // ========================================================================
    
    [Required(ErrorMessage = "El teléfono principal es requerido")]
    [Phone(ErrorMessage = "Formato de teléfono inválido")]
    [Display(Name = "Teléfono Principal")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Phone(ErrorMessage = "Formato de teléfono inválido")]
    [Display(Name = "Teléfono Secundario (Opcional)")]
    public string? PhoneNumber2 { get; set; }
}