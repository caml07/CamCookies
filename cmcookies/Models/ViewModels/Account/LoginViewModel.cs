using System.ComponentModel.DataAnnotations;

namespace cmcookies.Models.ViewModels.Account;

public class LoginViewModel
{
  [Required(ErrorMessage = "The email address is required")]
  [EmailAddress(ErrorMessage = "The email address is not a valid email address")]
  [Display(Name = "Email")]
  public string Email { get; set; }

  [Required(ErrorMessage = "The password is required")]
  [DataType(DataType.Password)]
  [Display(Name = "Password")]
  public string Password { get; set; }

  [Display(Name = "Remember me")]
  public bool
    RememberMe
  {
    get;
    set;
  } //Si esto de aca se pone en true, la cookiesession durara 14 dias si es false expira al cerrar el navegador GET/POST i think??
  //tambien Identity maneja esto automaticamente
}