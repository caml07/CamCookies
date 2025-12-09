//CRUD DE LAS COOKIES :D, solo puede ser editadas por el admin

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace cmcookies.Models.ViewModels.Admin;

public class CookieFormViewModel
{
  public int?
    CookieCode { get; set; } //Es NULL si es una galleta nueva, y tiene valor por qué ya hay una galleta con el code

  [Required(ErrorMessage = "The cookie name is required")]
  [StringLength(50, ErrorMessage = "The name cannot be longer than 50 characters")]
  [Display(Name = "Name")]
  public string
    CookieName { get; set; } //Guardar el nombre de la cookie, con constrain de no mayor a 50 caracteres le nombre

  [StringLength(255, ErrorMessage = "The description cannot be longer than 255 characters")]
  [Display(Name = "Description")]
  public string Description { get; set; } //descripcion de la galleta

  [Required(ErrorMessage = "The price is required")]
  [Range(0.01, 10000, ErrorMessage = "The price must be between C$0.01 and C$10,000")]
  [Display(Name = "Price (C$)")]
  public decimal
    Price
  {
    get;
    set;
  } //precio de la galleta, con restrinccion entre 1 centavo a 10000 cordobas (no creo que una cookie llege a costar más de 200 C$ realisticamente, pero queda asi por si acaso, quien sabe vd??)

  [Required(ErrorMessage = "Please select a category (Normal or Seasonal)")]
  [Display(Name = "Category")]
  public string Category { get; set; } // 'normal' o 'seasonal'

  [Range(0, 10000, ErrorMessage = "The stock must be between 0 and 10,000")]
  [Display(Name = "Initial Stock")]
  public int Stock { get; set; } //cantidad inicial de galletas, evita que el stock sea negativo

  [Display(Name = "Active?")] public bool IsActive { get; set; } = true;

  //subir la imagen de la galleta
  [Display(Name = "Image")] public IFormFile ImageFile { get; set; }

  //sirve para editar, guarda la ruta de la imagen que esta en uso actualmente solo para poderla mostarar en el form y no perderla en el caso de que no se suba una nueva imagen
  public string CurrentImagePath { get; set; }
}