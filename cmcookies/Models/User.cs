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
  // - PasswordHash (aparentemente utiliza PBKDF2-SHA256)
  // - etc.

  // Mapeamos el Id de Identity a user_id
  [Key]
  [Column("user_id")]
  public override int Id { get; set; }

  // Mapeamos UserName de Identity a username
  [Column("first_name")]
  [StringLength(50)]
  public string FirstName { get; set; } = null!;

  [Column("last_name")]
  [StringLength(50)]
  public string LastName { get; set; } = null!;

  // Email ya existe en IdentityUser, solo lo mapeamos a la columna email :D
  [Column("email")]
  [StringLength(100)]
  public override string? Email { get; set; }

  // PasswordHash ya existe en IdentityUser, lo mapeamos
  [Column("password_hash")]
  [StringLength(255)] // Cambiado a 255 porque Identity usa hashes más largos con lo de las salts y toda onda yk mr felix i dunno at the moment
  public override string? PasswordHash { get; set; }

  [Column("is_active")] //Puede ser util para cosas como mantener el historial de pedidos pero deshabilitar el login
  public bool? IsActive { get; set; }

  [Column("created_at", TypeName = "datetime")]
  public DateTime? CreatedAt { get; set; }

  [Column("updated_at", TypeName = "datetime")]
  public DateTime? UpdatedAt { get; set; }

  // Relaciones (se mantienen igual) 
  [InverseProperty("User")] 
  public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
  //un user puede tener multiples roles como un admin ser customer para que tambien pueda comprar y vender (caso real, el tortillazo en mi caso)  

  [InverseProperty("User")]
  public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}