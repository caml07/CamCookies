using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace cmcookies.Models.ViewModels.Cookie;

/// <summary>
/// ViewModel para Create y Edit de Cookies.
/// Contiene validaciones y lógica de presentación separada del modelo de DB.
/// </summary>
public class CookieViewModel
{
    // ===== DATOS BÁSICOS =====

    /// <summary>
    /// Código único de la cookie (ej. "CHOCHIP", "OATMEAL").
    /// </summary>
    [Required(ErrorMessage = "El código de la cookie es requerido")]
    [StringLength(10, MinimumLength = 3, ErrorMessage = "El código debe tener entre 3 y 10 caracteres")]
    [RegularExpression("^[A-Z0-9]+$", ErrorMessage = "El código solo puede contener letras mayúsculas y números")]
    [Display(Name = "Código de Cookie")]
    public string CookieCode { get; set; } = null!;

    /// <summary>
    /// Nombre de la cookie (requerido, máx 50 caracteres).
    /// </summary>
    [Required(ErrorMessage = "El nombre de la cookie es requerido")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder 50 caracteres")]
    [Display(Name = "Nombre de la Cookie")]
    public string CookieName { get; set; } = null!;

    /// <summary>
    /// Descripción de la cookie (opcional, máx 255 caracteres).
    /// </summary>
    [StringLength(255, ErrorMessage = "La descripción no puede exceder 255 caracteres")]
    [Display(Name = "Descripción")]
    public string? Description { get; set; }

    /// <summary>
    /// Precio de la cookie (requerido, debe ser mayor a 0).
    /// </summary>
    [Required(ErrorMessage = "El precio es requerido")]
    [Range(0.01, 999999.99, ErrorMessage = "El precio debe estar entre 0.01 y 999,999.99")]
    [Display(Name = "Precio (C$)")]
    [DataType(DataType.Currency)]
    public decimal Price { get; set; }

    /// <summary>
    /// Categoría: 'normal' o 'seasonal'.
    /// </summary>
    [Required(ErrorMessage = "La categoría es requerida")]
    [RegularExpression("^(normal|seasonal)$", ErrorMessage = "La categoría debe ser 'normal' o 'seasonal'")]
    [Display(Name = "Categoría")]
    public string? Category { get; set; } = "normal";

    /// <summary>
    /// Stock actual de la cookie (opcional, default 0).
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
    [Display(Name = "Stock Inicial")]
    public int? Stock { get; set; } = 0;

    /// <summary>
    /// Si la cookie está activa o no (default true).
    /// </summary>
    [Display(Name = "Activa")]
    public bool IsActive { get; set; } = true;

    // ===== UPLOAD DE IMAGEN =====

    /// <summary>
    /// Archivo de imagen para subir (solo en Create/Edit).
    /// </summary>
    [Display(Name = "Imagen de la Cookie")]
    public IFormFile? ImageFile { get; set; }

    /// <summary>
    /// Path de la imagen actual (solo para Edit, muestra preview).
    /// </summary>
    public string? CurrentImagePath { get; set; }

    // ===== METADATA (Solo para visualización) =====

    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}