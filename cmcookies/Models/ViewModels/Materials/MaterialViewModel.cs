using System.ComponentModel.DataAnnotations;

namespace cmcookies.Models.ViewModels.Material;

public class MaterialViewModel
{
  public int MaterialId { get; set; }

  [Required(ErrorMessage = "El nombre es obligatorio")]
  [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres")]
  [Display(Name = "Nombre del Material")]
  public string Name { get; set; } = string.Empty;

  [Required(ErrorMessage = "La unidad es obligatoria (ej: kg, gr, unidad)")]
  [StringLength(20, ErrorMessage = "La unidad es muy larga")]
  [Display(Name = "Unidad de Medida")]
  public string Unit { get; set; } = string.Empty;

  [Required(ErrorMessage = "El stock inicial es obligatorio")]
  [Range(0, double.MaxValue, ErrorMessage = "El stock no puede ser negativo")]
  [Display(Name = "Stock Actual")]
  public decimal Stock { get; set; }

  [Required(ErrorMessage = "El costo unitario es obligatorio")]
  [Range(0.01, double.MaxValue, ErrorMessage = "El costo debe ser mayor a 0")]
  [Display(Name = "Costo por Unidad")]
  public decimal UnitCost { get; set; }
}