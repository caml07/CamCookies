using System.ComponentModel.DataAnnotations;

namespace cmcookies.Models.ViewModels.Admin;

public class MaterialFormViewModel
{
  public int? MaterialId { get; set; } //es nulo si es nuevo, y tiene valor si se esta editando

  [Required(ErrorMessage = "The material name is required")]
  [StringLength(50, ErrorMessage = "The name cannot be longer than 50 characters")]
  [Display(Name = "Name")]
  public string Name { get; set; } //nombre del material

  [Required(ErrorMessage = "The unit of measure is required")]
  [StringLength(20, ErrorMessage = "The unit cannot be longer than 20 characters")]
  [Display(Name = "Unit")]
  public string Unit { get; set; } // kg, unidad, gramo, libra, etc.

  [Required(ErrorMessage = "The stock is required")]
  [Range(0, 100000, ErrorMessage = "The stock must be between 0 and 100,000")]
  [Display(Name = "Stock")]
  public decimal
    Stock
  {
    get;
    set;
  } //no puede ser ningun número negativo y tiene un límite hasta 100000 y puede ser decimal, si se quiere registrar por ej., que queda 1.5 Kg de harina o que sé yo

  [Required(ErrorMessage = "The unit cost is required")]
  [Range(0.01, 10000, ErrorMessage = "The cost must be between C$0.01 and C$10,000")]
  [Display(Name = "Unit Cost (C$)")]
  public decimal UnitCost { get; set; } //
}