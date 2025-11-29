using System.ComponentModel.DataAnnotations;

namespace cmcookies.Models.ViewModels.Admin;

public class BatchFormViewModel
{
  [Required(ErrorMessage = "You must select a cookie")]
  [Display(Name = "Cookie to produce")]
  public int CookieCode { get; set; }

  //información de la cookie seleccionada, que se llena luego de la seleccion
  public string CookieName { get; set; }
  public decimal CostPerBatch { get; set; } //se calcula automaticamente

  //produccion fija en 20 galletas por batch nuevo
  public int QuantityPerBatch { get; } = 20;

  [Display(Name = "Production Date")] public DateTime ProducedAt { get; set; } = DateTime.Now;
}