using System.ComponentModel.DataAnnotations;

namespace cmcookies.Models.ViewModels.Admin;

/// <summary>
/// ViewModel para que el ADMIN edite cualquier usuario.
/// Incluye TODO: datos personales, rol, contraseña, estado.
/// </summary>
public class UserEditViewModel
{
    // ========================================================================
    // ID DEL USUARIO (Para identificarlo en el POST)
    // ========================================================================
    public int UserId { get; set; }

    // ========================================================================
    // DATOS PERSONALES
    // ========================================================================
    
    [Required(ErrorMessage = "El nombre es requerido")]
    [StringLength(50)]
    [Display(Name = "Nombre")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es requerido")]
    [StringLength(50)]
    [Display(Name = "Apellido")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "El email es requerido")]
    [EmailAddress(ErrorMessage = "Email inválido")]
    [Display(Name = "Email")]
    public string Email { get; set; } = string.Empty;

    // ========================================================================
    // TELÉFONOS
    // ========================================================================
    
    [Required(ErrorMessage = "El teléfono principal es requerido")]
    [Phone]
    [Display(Name = "Teléfono Principal")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Phone]
    [Display(Name = "Teléfono Secundario")]
    public string? PhoneNumber2 { get; set; }

    // ========================================================================
    // ROL (El admin puede cambiar el rol)
    // ========================================================================
    
    [Required(ErrorMessage = "Debes seleccionar un rol")]
    [Display(Name = "Rol")]
    public string Role { get; set; } = "Customer";  // Por defecto Customer

    // ========================================================================
    // ESTADO DE LA CUENTA
    // ========================================================================
    
    [Display(Name = "Cuenta Activa")]
    public bool IsActive { get; set; } = true;

    // ========================================================================
    // CAMBIAR CONTRASEÑA (OPCIONAL)
    // ========================================================================
    // Si el admin deja esto vacío, NO se cambia la contraseña
    // Si pone algo, se cambia
    
    [StringLength(100, MinimumLength = 6)]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva Contraseña (dejar vacío si no quieres cambiarla)")]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirmar Nueva Contraseña")]
    [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
    public string? ConfirmPassword { get; set; }

    // ========================================================================
    // METADATOS (Para mostrar info al admin)
    // ========================================================================
    
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
}