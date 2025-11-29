using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using cmcookies.Models;
using cmcookies.Models.ViewModels.Account;

namespace cmcookies.Controllers;

/// <summary>
/// Controller encargado de la autenticación y gestión de cuentas de usuario.
/// Maneja registro, login, logout y acceso denegado.
/// </summary>
public class AccountController : Controller
{
  // Servicios de Identity que vamos a usar
  private readonly UserManager<User> _userManager; // Para crear y gestionar usuarios
  private readonly SignInManager<User> _signInManager; // Para login/logout
  private readonly RoleManager<Role> _roleManager; // Para gestionar roles
  private readonly CmcDBContext _context; // Para acceder a la BD directamente

  /// <summary>
  /// Constructor - Identity inyecta automáticamente estos servicios
  /// </summary>
  public AccountController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    RoleManager<Role> roleManager,
    CmcDBContext context)
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _roleManager = roleManager;
    _context = context;
  }

  // Aquí van los métodos (siguiente parte)
}