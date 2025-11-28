using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

/*
 *me sirve para representar roles del sistema y estamos heredando de IdentityRole para obtener funcionalidades como
 *la gestion y asignacion automatica de roles, verificacion de permisos, normalizacion de roels(Uppercasing o que no este con espacio), permisos avanzado de los roles tambien
 */

[Table("roles")] //se mapea a la tabla roles
public partial class Role : IdentityRole<int>
{
  // Identity ya provee:
  // - ID (lo mapeamos a role_id)
  // - Name
  // - NormalizedName (por si se quieren hacer busquedas como por ej: "ADMIN", "admin")

  // Mapeamos el Id de Identity a role_id
  [Key]
  [Column("role_id")]
  public override int Id { get; set; }

  // Mapeamos Name de Identity a role_type
  [Column("role_type")]
  [StringLength(50)]
  public override string? Name { get; set; }

  /*
   * nos permite navegar desde un role que tiene todos los usuarios a otro role que solo ciertos usuarios tendran, como lo de admins que pueden ser customers
   */
  
  // Relaciones (se mantienen igual)
  [InverseProperty("Role")]
  public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}
/*
 * ===================================================================
 * ¿CÓMO FUNCIONA EL SISTEMA DE ROLES?
 * ===================================================================
 * 
 * 1. CREACIÓN DE ROLES (solo se hace una vez):
 *    
 *    var adminRole = new Role { Name = "Admin" };
 *    await _roleManager.CreateAsync(adminRole);
 * 
 * 2. ASIGNACIÓN DE ROLES A USUARIOS:
 *    
 *    await _userManager.AddToRoleAsync(user, "Admin");
 *    await _userManager.AddToRoleAsync(user, "Customer");
 * 
 * 3. VERIFICACIÓN DE ROLES:
 *    
 *    // En un controlador:
 *    bool isAdmin = User.IsInRole("Admin");
 *    
 *    // En una vista Razor:
 *    @if (User.IsInRole("Admin"))
 *    {
 *        <a href="/Admin/Dashboard">Panel Admin</a>
 *    }
 * 
 * 4. PROTECCIÓN DE CONTROLADORES:
 *    
 *    [Authorize(Roles = "Admin")]
 *    public class AdminController : Controller
 *    {
 *        // Solo admins pueden acceder aquí
 *    }
 * 
 * 5. OBTENER ROLES DE UN USUARIO:
 *    
 *    var roles = await _userManager.GetRolesAsync(user);
 *    // Devuelve: ["Admin", "Customer"]
 * 
 * 6. VERIFICAR SI UN USUARIO TIENE UN ROL:
 *    
 *    bool hasRole = await _userManager.IsInRoleAsync(user, "Admin");
 * 
 * ===================================================================
 * ROLES EN CAM COOKIES:
 * ===================================================================
 * 
 * Tenemos 2 roles principales:
 * 
 * 1. CUSTOMER (Cliente):
 *    - Puede ver el catálogo
 *    - Puede hacer pedidos
 *    - Puede ver su historial de pedidos
 *    - NO puede acceder al panel de admin
 * 
 * 2. ADMIN (Administrador):
 *    - Todos los permisos de Customer +
 *    - Puede gestionar pedidos
 *    - Puede gestionar galletas (CRUD)
 *    - Puede gestionar materiales
 *    - Puede producir batches
 *    - Puede ver estadísticas
 *    - Puede asignar roles a otros usuarios
 * 
 * UN USUARIO PUEDE TENER AMBOS ROLES:
 * - Eduardo tiene roles: ["Admin", "Customer"]
 * - Puede comprar galletas (Customer)
 * - Puede gestionar el negocio (Admin)
 * - Un usuario normal solo tiene: ["Customer"]
 */