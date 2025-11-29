using System.ComponentModel.DataAnnotations;

namespace cmcookies.Models.ViewModels.Account;

public class RegisterViewModel
{
  [Required(ErrorMessage = "The first name is required")]
  [StringLength(50, ErrorMessage = "The first name cannot be longer than 50 characters")]
  [Display(Name = "First Name")]
  public string
    FirstName { get; set; } //almacena el primer nombre y se valida que no este vacio y que no pase de 50 caracteres

  [Required(ErrorMessage = "The last name is required")]
  [StringLength(50, ErrorMessage = "The last name cannot be longer than 50 characters")]
  [Display(Name = "Last Name")]
  public string LastName { get; set; } //lo mismo que first name, nada mas que con el apallido

  [Required(ErrorMessage = "The email address is required")]
  [EmailAddress(ErrorMessage = "The email address is not a valid email address")]
  [Display(Name = "Email")]
  public string
    Email { get; set; } //validacion, ya lo hace el framework y busca que tenga el formato de un correo electronico

  [Required(ErrorMessage = "The phone number is required")]
  [Phone(ErrorMessage = "The phone number is not a valid phone number")]
  [StringLength(20, ErrorMessage = "The phone number cannot be longer than 20 characters")]
  [Display(Name = "Phone Number")]
  public string
    PhoneNumber
  {
    get;
    set;
  } //No exite dentro de User.cs y esto de aca lo guarda dentro de la tabla de phones que esta separada

  [Required(ErrorMessage = "The password is required")]
  [StringLength(100, ErrorMessage = "The password must be at least {2} characters long", MinimumLength = 8)]
  [DataType(DataType.Password)]
  [Display(Name = "Password")]
  [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
    ErrorMessage = "Password must have at least one uppercase, one lowercase, one digit and one special character")]
  public string
    Password
  {
    get;
    set;
  } //Temporalmente en memoria contiene la contraseña en texto plano, no se guarda en la db asi y Identity se encarga de hashearla antes de guardarla

  [Required(ErrorMessage = "You must confirm the password")]
  [DataType(DataType.Password)]
  [Display(Name = "Confirm password")]
  [Compare("Password", ErrorMessage = "The passwords do not match")] //verifica que Password == ConfirmPasword
  public string
    ConfirmPassword
  {
    get;
    set;
  } //existe aca, dentro del viewmodel meramente para confirmar de que el usuario escribio bien su contraseña y 
}