using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using cmcookies.Models;
using cmcookies.Models.ViewModels.Admin;

namespace cmcookies.Controllers;

// ============================================================================
// USERS CONTROLLER - Gestión de Usuarios (SOLO ADMIN) 👥
// ============================================================================
// Permite al administrador:
// 1. Ver lista de todos los usuarios
// 2. Editar datos de cualquier usuario (nombre, email, teléfono, ROL)
// 3. Cambiar contraseña de cualquier usuario (sin necesitar la actual)
// 4. Activar/Desactivar cuentas
// 5. Eliminar usuarios permanentemente
//
// IMPORTANTE: Solo accesible para usuarios con rol "Admin"
// ============================================================================

[Authorize(Roles = "Admin")] //SOLO ADMINS
public class UsersController : Controller
{
  private readonly UserManager<User> _userManager;
  private readonly CmcDBContext _context;

  public UsersController(
    UserManager<User> userManager,
    CmcDBContext context)
  {
    _userManager = userManager;
    _context = context;
  }

// ========================================================================
// GET: /Admin/Users/Index - Lista de todos los usuarios
// ========================================================================
  public async Task<IActionResult> Index()
  {
    //Obtener TODOS los usuarios de la base de datos
    var users = await _userManager.Users
      .OrderBy(u => u.LastName) // Ordenar alfabéticamente por apellido
      .ThenBy(u => u.FirstName)
      .ToListAsync();

    //Para cada usuario, obtener su ROL
    // ¿Por qué? Porque los roles NO están en la tabla Users,
    // están en una tabla separada (UserRoles)
    var userViewModels = new List<(User User, string Role, bool IsActive)>();

    foreach (var user in users)
    {
      // Obtener los roles del usuario (un usuario puede tener múltiples roles)
      var roles = await _userManager.GetRolesAsync(user);

      // Tomar el primer rol (en nuestro caso, cada usuario tiene 1 solo rol)
      var role = roles.FirstOrDefault() ?? "Sin Rol";

      // Añadir a la lista
      userViewModels.Add((user, role, user.IsActive ?? false));
    }

    // 3️⃣ Pasar la lista a la vista
    ViewData["Title"] = "Gestión de Usuarios";
    return View(userViewModels);
  }

  // ========================================================================
// GET: /Admin/Users/Edit/5 - Formulario de edición
// ========================================================================
  [HttpGet]
  public async Task<IActionResult> Edit(int id)
  {
    //Buscar el usuario por ID
    var user = await _userManager.FindByIdAsync(id.ToString());

    if (user == null)
    {
      TempData["Error"] = "Usuario no encontrado";
      return RedirectToAction(nameof(Index));
    }

    //Buscar el Customer (para obtener los teléfonos)
    var customer = await _context.Customers
      .Include(c => c.Phone)
      .FirstOrDefaultAsync(c => c.UserId == user.Id);

    //Obtener el rol actual del usuario
    var roles = await _userManager.GetRolesAsync(user);
    var currentRole = roles.FirstOrDefault() ?? "Customer";

    //Crear el ViewModel con los datos actuales
    var viewModel = new UserEditViewModel
    {
      UserId = user.Id,
      FirstName = user.FirstName,
      LastName = user.LastName,
      Email = user.Email ?? string.Empty,
      PhoneNumber = customer?.Phone?.Phone1 ?? string.Empty,
      PhoneNumber2 = customer?.Phone?.Phone2,
      Role = currentRole,
      IsActive = user.IsActive ?? false,
      CreatedAt = user.CreatedAt,
    };

    // Pasar lista de roles disponibles al ViewBag
    // Para el dropdown de roles
    ViewBag.AvailableRoles = new List<string> { "Admin", "Customer" };

    ViewData["Title"] = $"Editar Usuario: {user.FirstName} {user.LastName}";

    return View(viewModel);
  }

  // ========================================================================
// POST: /Admin/Users/Edit - Guardar cambios del usuario
// ========================================================================
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Edit(UserEditViewModel model)
  {
    //Validar el modelo
    if (!ModelState.IsValid)
    {
      // Si hay errores, volver al formulario
      ViewBag.AvailableRoles = new List<string> { "Admin", "Customer" };
      return View(model);
    }

    //Buscar el usuario
    var user = await _userManager.FindByIdAsync(model.UserId.ToString());

    if (user == null)
    {
      TempData["Error"] = "Usuario no encontrado";
      return RedirectToAction(nameof(Index));
    }

    // ========================================================================
    //ACTUALIZAR DATOS BÁSICOS DEL USER
    // ========================================================================

    user.FirstName = model.FirstName;
    user.LastName = model.LastName;
    user.IsActive = model.IsActive;
    user.UpdatedAt = DateTime.Now;

    // Actualizar email (Identity maneja esto especialmente)
    if (user.Email != model.Email)
    {
      // SetEmailAsync actualiza Email Y UserName (que es el email)
      var setEmailResult = await _userManager.SetEmailAsync(user, model.Email);
      if (!setEmailResult.Succeeded)
      {
        foreach (var error in setEmailResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
        ViewBag.AvailableRoles = new List<string> { "Admin", "Customer" };
        return View(model);
      }

      // También actualizar el UserName (que en nuestro caso es el email)
      await _userManager.SetUserNameAsync(user, model.Email);
    }

    // Guardar cambios en User
    var updateResult = await _userManager.UpdateAsync(user);
    if (!updateResult.Succeeded)
    {
      foreach (var error in updateResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
      ViewBag.AvailableRoles = new List<string> { "Admin", "Customer" };
      return View(model);
    }

    // ========================================================================
    //ACTUALIZAR TELÉFONOS (Tabla PHONES)
    // ========================================================================

    var customer = await _context.Customers
      .Include(c => c.Phone)
      .FirstOrDefaultAsync(c => c.UserId == user.Id);

    if (customer?.Phone != null)
    {
      // Actualizar teléfonos existentes
      customer.Phone.Phone1 = model.PhoneNumber;
      customer.Phone.Phone2 = model.PhoneNumber2;
    }
    else if (customer != null)
    {
      // Crear Phone si no existe
      var newPhone = new Phone
      {
        Phone1 = model.PhoneNumber,
        Phone2 = model.PhoneNumber2
      };
      _context.Phones.Add(newPhone);
      await _context.SaveChangesAsync();

      customer.PhoneId = newPhone.PhoneId;
    }

    await _context.SaveChangesAsync();

    // ========================================================================
    //ACTUALIZAR ROL (Si cambió)
    // ========================================================================

    var currentRoles = await _userManager.GetRolesAsync(user);
    var currentRole = currentRoles.FirstOrDefault();

    if (currentRole != model.Role)
    {
      // Quitar el rol actual
      if (!string.IsNullOrEmpty(currentRole)) await _userManager.RemoveFromRoleAsync(user, currentRole);

      // Asignar el nuevo rol
      await _userManager.AddToRoleAsync(user, model.Role);
    }

    // ========================================================================
    //CAMBIAR CONTRASEÑA (Si se proporcionó una nueva)
    // ========================================================================

    if (!string.IsNullOrEmpty(model.NewPassword))
    {
      // El admin NO necesita saber la contraseña actual
      // Usamos RemovePasswordAsync + AddPasswordAsync

      // Paso 1: Quitar la contraseña actual
      var removePasswordResult = await _userManager.RemovePasswordAsync(user);

      if (removePasswordResult.Succeeded)
      {
        // Paso 2: Añadir la nueva contraseña
        var addPasswordResult = await _userManager.AddPasswordAsync(user, model.NewPassword);

        if (!addPasswordResult.Succeeded)
        {
          foreach (var error in addPasswordResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
          ViewBag.AvailableRoles = new List<string> { "Admin", "Customer" };
          return View(model);
        }
      }
    }

    // ========================================================================
    //MENSAJE DE ÉXITO Y REDIRECT
    // ========================================================================

    TempData["Success"] = $"Usuario {user.FirstName} {user.LastName} actualizado exitosamente ✅";
    return RedirectToAction(nameof(Index));
  }
  // ========================================================================
// POST: /Admin/Users/Delete/5 - Eliminar usuario permanentemente
// ========================================================================
  [HttpPost]
  [ValidateAntiForgeryToken]
  public async Task<IActionResult> Delete(int id)
  {
    //Buscar el usuario
    var user = await _userManager.FindByIdAsync(id.ToString());
    
    if (user == null)
    {
      TempData["Error"] = "Usuario no encontrado";
      return RedirectToAction(nameof(Index));
    }

    //SEGURIDAD: No permitir que el admin se elimine a sí mismo
    var currentUser = await _userManager.GetUserAsync(User);
    if (currentUser != null && currentUser.Id == user.Id)
    {
      TempData["Error"] = "No puedes eliminar tu propia cuenta ❌";
      return RedirectToAction(nameof(Index));
    }

    //Eliminar el usuario (Identity maneja CASCADE para roles, etc.)
    var result = await _userManager.DeleteAsync(user);
    
    if (result.Succeeded)
    {
      TempData["Success"] = $"Usuario {user.FirstName} {user.LastName} eliminado permanentemente 🗑️";
    }
    else
    {
      TempData["Error"] = "Error al eliminar el usuario";
    }
    return RedirectToAction(nameof(Index));
  }
}