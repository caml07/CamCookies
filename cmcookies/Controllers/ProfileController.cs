using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmcookies.Models;
using cmcookies.Models.ViewModels.Profile;

namespace cmcookies.Controllers;

// ============================================================================
// PROFILE CONTROLLER - Gestión del Perfil del Usuario 👤
// ============================================================================
// Permite al usuario (cliente o admin) editar:
// 1. Datos básicos (nombre, apellido, teléfonos)
// 2. Contraseña
//
// IMPORTANTE: Solo el usuario puede editar SU PROPIO perfil
// (más adelante hare un panel de admin para editar OTROS usuarios)
// ============================================================================

[Authorize] //Solo usuarios logueados
public class ProfileController : Controller
{
  private readonly UserManager<User> _userManager;
  private readonly SignInManager<User> _signInManager;
  private readonly CmcDBContext _context;

  public ProfileController(
    UserManager<User> userManager,
    SignInManager<User> signInManager,
    CmcDBContext context)
  {
    _userManager = userManager;
    _signInManager = signInManager;
    _context = context;
  }

// ========================================================================
// GET: Profile/Edit - Mostrar formulario de edición
// ========================================================================
  [HttpGet]
  public async Task<IActionResult> Edit()
  {
    //Obtener el usuario logueado
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return Challenge(); // Si no hay usuario, redirigir a login

    //Buscar el Customer (para obtener los teléfonos)
    var customer = await _context.Customers
      .Include(c => c.Phone) // Traer también la tabla Phone
      .FirstOrDefaultAsync(c => c.UserId == user.Id);

    //Crear el ViewModel con los datos actuales
    var viewModel = new EditProfileViewModel
    {
      FirstName = user.FirstName,
      LastName = user.LastName,
      Email = user.Email ?? string.Empty,
      PhoneNumber = customer?.Phone?.Phone1 ?? string.Empty,
      PhoneNumber2 = customer?.Phone?.Phone2
    };

    //Mostrar la vista con el ViewModel
    return View(viewModel);
  }

  // ========================================================================
// POST: Profile/EditProfile - Actualizar datos básicos
// ========================================================================
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> EditProfile(EditProfileViewModel model)
  {
    //Validar el modelo
    if (!ModelState.IsValid)
      // Si hay errores, volver a mostrar el formulario con los errores
      return View("Edit", model);

    //Obtener el usuario logueado
    var user = await _userManager.GetUserAsync(User);
    if (user == null) return Challenge();

    //Actualizar los datos del User
    user.FirstName = model.FirstName;
    user.LastName = model.LastName;
    user.UpdatedAt = DateTime.Now;

    //Guardar cambios en User (con Identity)
    var result = await _userManager.UpdateAsync(user);

    if (!result.Succeeded)
    {
      // Si hubo error, mostrar los errores
      foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
      return View("Edit", model);
    }

    //Actualizar los teléfonos (tabla Phone)
    var customer = await _context.Customers
      .Include(c => c.Phone)
      .FirstOrDefaultAsync(c => c.UserId == user.Id);

    if (customer?.Phone != null)
    {
      // El customer ya tiene un Phone, actualizarlo
      customer.Phone.Phone1 = model.PhoneNumber;
      customer.Phone.Phone2 = model.PhoneNumber2;
    }
    else if (customer != null)
    {
      // El customer existe pero no tiene Phone, crearlo
      var newPhone = new Phone
      {
        Phone1 = model.PhoneNumber,
        Phone2 = model.PhoneNumber2
      };
      _context.Phones.Add(newPhone);
      await _context.SaveChangesAsync();

      customer.PhoneId = newPhone.PhoneId;
    }

    //Guardar cambios en la BD
    await _context.SaveChangesAsync();

    //Mensaje de éxito
    TempData["Success"] = "¡Perfil actualizado exitosamente! ✅";

    return RedirectToAction(nameof(Edit));
  }
  // ========================================================================
// POST: Profile/ChangePassword - Cambiar contraseña
// ========================================================================
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
  {
    //Validar el modelo
    if (!ModelState.IsValid)
    {
      // Si hay errores, volver al formulario
      // IMPORTANTE: Necesitamos también los datos del EditProfileViewModel
      // para mostrar la página completa
      var user = await _userManager.GetUserAsync(User);
      if (user == null) return Challenge();

      var customer = await _context.Customers
        .Include(c => c.Phone)
        .FirstOrDefaultAsync(c => c.UserId == user.Id);

      var editModel = new EditProfileViewModel
      {
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email ?? string.Empty,
        PhoneNumber = customer?.Phone?.Phone1 ?? string.Empty,
        PhoneNumber2 = customer?.Phone?.Phone2
      };

      // Pasar ambos modelos a la vista (lo veremos en la vista)
      ViewBag.ChangePasswordModel = model;
      return View("Edit", editModel);
    }

    //Obtener el usuario logueado
    var currentUser = await _userManager.GetUserAsync(User);
    if (currentUser == null) return Challenge();

    //SEGURIDAD: Verificar que la contraseña actual es correcta
    var isPasswordCorrect = await _userManager.CheckPasswordAsync(
      currentUser,
      model.CurrentPassword
    );

    if (!isPasswordCorrect)
    {
      // La contraseña actual es incorrecta
      ModelState.AddModelError(string.Empty,
        "La contraseña actual es incorrecta");

      // Recargar datos para mostrar la página completa
      var customer = await _context.Customers
        .Include(c => c.Phone)
        .FirstOrDefaultAsync(c => c.UserId == currentUser.Id);

      var editModel = new EditProfileViewModel
      {
        FirstName = currentUser.FirstName,
        LastName = currentUser.LastName,
        Email = currentUser.Email ?? string.Empty,
        PhoneNumber = customer?.Phone?.Phone1 ?? string.Empty,
        PhoneNumber2 = customer?.Phone?.Phone2
      };

      ViewBag.ChangePasswordModel = model;
      return View("Edit", editModel);
    }

    //VALIDACIÓN ADICIONAL: Nueva contraseña no puede ser igual a la actual
    if (model.CurrentPassword == model.NewPassword)
    {
      ModelState.AddModelError(string.Empty,
        "La nueva contraseña debe ser diferente a la actual");

      var customer = await _context.Customers
        .Include(c => c.Phone)
        .FirstOrDefaultAsync(c => c.UserId == currentUser.Id);

      var editModel = new EditProfileViewModel
      {
        FirstName = currentUser.FirstName,
        LastName = currentUser.LastName,
        Email = currentUser.Email ?? string.Empty,
        PhoneNumber = customer?.Phone?.Phone1 ?? string.Empty,
        PhoneNumber2 = customer?.Phone?.Phone2
      };

      ViewBag.ChangePasswordModel = model;
      return View("Edit", editModel);
    }

    //Cambiar la contraseña
    var result = await _userManager.ChangePasswordAsync(
      currentUser,
      model.CurrentPassword,
      model.NewPassword
    );

    if (!result.Succeeded)
    {
      // Si hubo error (ej: no cumple política de contraseñas)
      foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);

      var customer = await _context.Customers
        .Include(c => c.Phone)
        .FirstOrDefaultAsync(c => c.UserId == currentUser.Id);

      var editModel = new EditProfileViewModel
      {
        FirstName = currentUser.FirstName,
        LastName = currentUser.LastName,
        Email = currentUser.Email ?? string.Empty,
        PhoneNumber = customer?.Phone?.Phone1 ?? string.Empty,
        PhoneNumber2 = customer?.Phone?.Phone2
      };

      ViewBag.ChangePasswordModel = model;
      return View("Edit", editModel);
    }

    //IMPORTANTE: Re-loguear al usuario
    // ¿Por qué? Porque cambiar la contraseña invalida la cookie actual
    await _signInManager.RefreshSignInAsync(currentUser);

    //Mensaje de éxito
    TempData["Success"] = "¡Contraseña cambiada exitosamente! 🔐";

    return RedirectToAction(nameof(Edit));
  }
}