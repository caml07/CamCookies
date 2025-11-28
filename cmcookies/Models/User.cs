using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("users")]
public partial class User : IdentityUser<int>
{
  // Identity ya provee:
  // - Id (lo mapeamos a user_id)
  // - UserName
  // - Email
  // - PasswordHash
  // - etc.

  // Mapeamos el Id de Identity a user_id
  [Key]
  [Column("user_id")]
  public override int Id { get; set; }

  // Campos personalizados de tu BD
  [Column("first_name")]
  [StringLength(50)]
  public string FirstName { get; set; } = null!;

  [Column("last_name")]
  [StringLength(50)]
  public string LastName { get; set; } = null!;

  // Email ya existe en IdentityUser, solo lo mapeamos a tu columna
  [Column("email")]
  [StringLength(100)]
  public override string? Email { get; set; }

  // PasswordHash ya existe en IdentityUser, lo mapeamos
  [Column("password_hash")]
  [StringLength(255)] // Cambiado a 255 porque Identity usa hashes más largos
  public override string? PasswordHash { get; set; }

  [Column("is_active")]
  public bool? IsActive { get; set; }

  [Column("created_at", TypeName = "datetime")]
  public DateTime? CreatedAt { get; set; }

  [Column("updated_at", TypeName = "datetime")]
  public DateTime? UpdatedAt { get; set; }

  // Relaciones (se mantienen igual)
  [InverseProperty("User")]
  public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();

  [InverseProperty("User")]
  public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}