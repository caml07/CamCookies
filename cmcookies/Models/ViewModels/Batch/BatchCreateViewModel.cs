using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace cmcookies.Models.ViewModels.Batch
{
  public class BatchCreateViewModel
  {
    [Display(Name = "Seleccionar Galleta a Producir")]
    [Required(ErrorMessage = "Debes seleccionar una galleta")]
    // CORRECCIÓN: Cambiado de int a string
    public string CookieCode { get; set; } = string.Empty;

    public SelectList? CookiesList { get; set; }
  }
}