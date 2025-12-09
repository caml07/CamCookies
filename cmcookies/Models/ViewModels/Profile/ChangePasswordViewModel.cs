using System.ComponentModel.DataAnnotations;

namespace cmcookies.Models.ViewModels.Profile;

/// <summary>
/// ViewModel para cambiar la contraseña.
/// Requiere la contraseña ACTUAL por seguridad.
/// </summary>
public class ChangePasswordViewModel
{
    // ========================================================================
    // CONTRASEÑA ACTUAL (Verificación de seguridad)
    // ========================================================================
    // ¿Por qué pedimos la actual?
    // Para asegurarnos que es el usuario real, no alguien que dejó la sesión abierta
    // ========================================================================
    
    [Required(ErrorMessage = "Debes ingresar tu contraseña actual")]
    [DataType(DataType.Password)]
    [Display(Name = "Contraseña Actual")]
    public string CurrentPassword { get; set; } = string.Empty;

    // ========================================================================
    // NUEVA CONTRASEÑA
    // ========================================================================
    
    [Required(ErrorMessage = "La nueva contraseña es requerida")]
    [StringLength(100, MinimumLength = 6, 
        ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
    [DataType(DataType.Password)]
    [Display(Name = "Nueva Contraseña")]
    [RegularExpression(@"^(?=.*[a-z]).{6,}$",
        ErrorMessage = "Debe tener al menos 6 caracteres y una letra minúscula")]
    public string NewPassword { get; set; } = string.Empty;

    // ========================================================================
    // CONFIRMAR NUEVA CONTRASEÑA
    // ========================================================================
    
    [Required(ErrorMessage = "Debes confirmar la nueva contraseña")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirmar Nueva Contraseña")]
    [Compare("NewPassword", ErrorMessage = "Las contraseñas no coinciden")]
    public string ConfirmPassword { get; set; } = string.Empty;
}