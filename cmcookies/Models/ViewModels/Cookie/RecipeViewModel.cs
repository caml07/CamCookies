using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace cmcookies.Models.ViewModels.Cookie
{
  public class RecipeViewModel
  {
    // Datos de la Galleta (Solo lectura)
    public string CookieCode { get; set; } = null!;
    public string CookieName { get; set; } = null!;
    public string? CookieImage { get; set; }

    // Lista de ingredientes actuales (Para la tabla)
    public List<RecipeItem> Ingredients { get; set; } = new();

    // Formulario para agregar nuevo ingrediente
    [Display(Name = "Insumo")]
    [Required(ErrorMessage = "Selecciona un material")]
    public int NewMaterialId { get; set; }

    [Display(Name = "Cantidad por Batch (20u)")]
    [Required(ErrorMessage = "Ingresa la cantidad")]
    [Range(0.01, 9999, ErrorMessage = "La cantidad debe ser mayor a 0")]
    public decimal NewQuantity { get; set; }

    // Dropdown para el formulario
    public SelectList? MaterialsList { get; set; }
  }

  public class RecipeItem
  {
    public int CookieMaterialId { get; set; }
    public string MaterialName { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public decimal Quantity { get; set; }
    public decimal CostCalculated { get; set; } // Costo unitario * cantidad
  }
}