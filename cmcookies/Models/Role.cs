using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("roles")]
public partial class Role : IdentityRole<int>
{
  // Identity ya provee:
  // - ID (lo mapeamos a role_id)
  // - Name
  // - NormalizedName

  // Mapeamos el Id de Identity a role_id
  [Key]
  [Column("role_id")]
  public override int Id { get; set; }

  // Mapeamos Name de Identity a role_type
  [Column("role_type")]
  [StringLength(50)]
  public override string? Name { get; set; }

  // Relaciones (se mantienen igual)
  [InverseProperty("Role")]
  public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}