using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

// Mapea esta clase a la tabla "users" de la base de datos.
[Table("users")]
// Hereda de Identity para la funcionalidad de login. Usa 'int' como tipo de ID.
public partial class User : IdentityUser<int>
{
  // Identity ya provee campos como UserName, Email, PasswordHash, etc.

  // Mapea el ID de Identity a 'user_id' y la define como llave primaria.
  [Key] [Column("user_id")] public override int Id { get; set; }

  // Campos personalizados que no existen en la clase IdentityUser.
  [Column("first_name")]
  [StringLength(50)]
  public string FirstName { get; set; } = null!;

  [Column("last_name")]
  [StringLength(50)]
  public string LastName { get; set; } = null!;

  // Mapea la propiedad Email de Identity a nuestra columna 'email'.
  [Column("email")] [StringLength(100)] public override string Email { get; set; } = null!;

  // Mapea el PasswordHash de Identity a nuestra columna 'password_hash'.
  [Column("password_hash")]
  [StringLength(255)] // Largo extendido para los hashes de Identity.
  public override string PasswordHash { get; set; } = null!;

  [Column("is_active")] public bool? IsActive { get; set; }

  [Column("created_at", TypeName = "datetime")]
  public DateTime? CreatedAt { get; set; }

  [Column("updated_at", TypeName = "datetime")]
  public DateTime? UpdatedAt { get; set; }

  // --- Relaciones con otras tablas ---

  // Define la relación uno-a-muchos con la tabla de Clientes.
  [InverseProperty("User")] // Conecta con la propiedad 'User' en la clase Customer.
  public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

  // Define la relación uno-a-muchos con la tabla de Roles.
  // Identity maneja esto automáticamente, NO necesitamos [InverseProperty]
  public virtual ICollection<IdentityUserRole<int>> UserRoles { get; set; } = new List<IdentityUserRole<int>>();
}
