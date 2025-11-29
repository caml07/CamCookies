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
      UserManager<User> userManager, //Identity gestiona automaticamente el crear un usuario y hashear la contraseña, buscarlo por email, asignar un rol y verificar si la contraseña es correcta
      SignInManager<User> signInManager, //Identity gestiona automaticamente el login y logout de un usuario
      RoleManager<Role> roleManager, //Identity gestiona automaticamente el manejo de roles, verifica si existe y si no crea un nuevo rol
      CmcDBContext context) //para pasarle el DB context que se usara luego apra la creacion de Phone y Customere en el registro

    //Dependency Injection, que lo maneja automaticamente Identity
    //ASP.NET Core inyecta automaticamente los servicios y no se necesitan crear con instancias new, y ya estan configurados dentro de Program.cs
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _roleManager = roleManager;
    _context = context;
  }

  // ============================================================================
// REGISTER - Mostrar formulario de registro
// ============================================================================

  /// <summary>
  /// GET: /Account/Register
  /// Muestra el formulario de registro de nuevos usuarios.
  /// </summary>
  [HttpGet]
  public IActionResult Register()
  {
    return View();
  }

  /// <summary>
  /// POST: /Account/Register
  /// Procesa el formulario de registro y crea un nuevo usuario.
  /// </summary>
  [HttpPost]
  [ValidateAntiForgeryToken] // Protege contra ataques CSRF (Cross-Site Request Forgery)
  public async Task<IActionResult> Register(RegisterViewModel model)
  {
    //Validar que el modelo tenga datos correctos
    if (!ModelState.IsValid)
    {
        return View(model); // Si hay errores, vuelve a mostrar el formulario
    }

    //Crear el objeto User con los datos del formulario
    var user = new User
    {
      UserName = model
        .Email, // Identity usa UserName para login y desde que queremos que se logueen con el email por eso esta de esa manera
      Email = model.Email,
      FirstName = model.FirstName,
      LastName = model.LastName,
      EmailConfirmed = true, // Por ahora no pedimos confirmación por email
      IsActive = true,
      CreatedAt = DateTime.Now,
      UpdatedAt = DateTime.Now
    };

    //Crear el usuario en la BD (Identity hashea la contraseña automáticamente)
    var result = await _userManager.CreateAsync(user, model.Password);

    if (result.Succeeded)
    {
      //Asignar el rol "Customer" al nuevo usuario
      await _userManager.AddToRoleAsync(user, "Customer");

      //Crear el registro de teléfono
      var phone = new Phone
      {
        Phone1 = model.PhoneNumber,
        Phone2 = null
      };
      _context.Phones.Add(phone);
      await _context.SaveChangesAsync(); //Gracias a esto phone.PhoneId tiene ahora un valor

      //Crear el registro de Customer
      var customer = new Customer
      {
        UserId = user.Id, //ya tenemos el UserId
        PhoneId = phone.PhoneId //ya tenemos tambien el PhoneId
      };
      _context.Customers.Add(customer);
      await _context.SaveChangesAsync();

      // Iniciar sesión automáticamente
      // isPersistent: false → Cookie expira al cerrar navegador
      // isPersistent: true → Cookie dura 14 días (Remember Me)
      await _signInManager.SignInAsync(user, isPersistent: false);

      //Redirigir al inicio
      return RedirectToAction("Index", "Home");
    }

    //Si hubo errores, mostrarlos en el formulario, como contraseñas o algun correo mal introducido
    foreach (var error in result.Errors)
    {
        ModelState.AddModelError(string.Empty, error.Description);
    }

    return View(model);
  }

// ============================================================================
// LOGIN - Iniciar sesión
// ============================================================================

  /// <summary>
  /// GET: /Account/Login
  /// Muestra el formulario de inicio de sesión.
  /// </summary>
  [HttpGet]
  public IActionResult Login(string returnUrl = null) //Si un usuario no loggeado intenta acceder desde el navegado a Admin/Dashboard por ejemplo, ASP.NET lo redirige a que se logge, y si este es admin, si puede pasar a la Dashboard, pero sin esto, va a la pag que seguiria despues del Logg
  {
    // Guardar la URL a la que debe volver después del login
    ViewData["ReturnUrl"] = returnUrl;
    return View();
  }

  /// <summary>
  /// POST: /Account/Login
  /// Procesa el inicio de sesión del usuario.
  /// </summary>
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Login(LoginViewModel model, string returnUrl = null)
  {
    // Guardar returnUrl para el formulario
    ViewData["ReturnUrl"] = returnUrl;

    // PASO 1: Validar el modelo
    if (!ModelState.IsValid)
    {
        return View(model);
    }

    // Intentar iniciar sesión
    // Parámetros:
    // - model.Email: el UserName (que es el email)
    // - model.Password: la contraseña en texto plano
    // - model.RememberMe: si true, cookie dura 14 días
    // - lockoutOnFailure: si true, bloquea cuenta después de 5 intentos fallidos
    var result = await _signInManager.PasswordSignInAsync(
      model.Email, // Identity usa UserName para login y desde que queremos que se logueen con el email por eso esta de esa manera
      model.Password, //Contraseña en texto planito
      model.RememberMe, //La cookie aun existe??
      lockoutOnFailure: true //Bloqueamos la cuenta tras fallos??
    );

    if (result.Succeeded)
    {
      // Login exitoso

      // A)Si había una URL guardada (ej: intentó acceder a /Admin sin login)
      if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
      {
          return Redirect(returnUrl); // Volver a donde intentaba ir
      }

      // Si no había URL guardada, ir al inicio
      return RedirectToAction("Index", "Home");
    }

    if (result.IsLockedOut)
    {
      // B)Cuenta bloqueada (demasiados intentos fallidos)
      ModelState.AddModelError(string.Empty,
        "Your account has been locked due to multiple failed login attempts. Please try again in 5 minutes.");
      return View(model);
    }

    // C)Credenciales incorrectas
    ModelState.AddModelError(string.Empty, "Invalid email or password.");
    return View(model);
  }

// ============================================================================
// LOGOUT - Cerrar sesión
// ============================================================================

  /// <summary>
  /// POST: /Account/Logout
  /// Cierra la sesión del usuario actual.
  /// </summary>
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Logout()
  {
    // Cerrar sesión (borra la cookie de autenticación)
    await _signInManager.SignOutAsync(); 

    // Redirigir al inicio
    return RedirectToAction("Index", "Home");
  }

// ============================================================================
// ACCESS DENIED - Acceso denegado
// ============================================================================

  /// <summary>
  /// GET: /Account/AccessDenied
  /// Página mostrada cuando un usuario intenta acceder a un recurso sin permisos.
  /// </summary>
  [HttpGet]
  public IActionResult AccessDenied()
  {
    return View();
  }
}
