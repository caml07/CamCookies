using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("user_roles")]
[Index("RoleId", Name = "fk_user_roles_role")]
[Index("UserId", Name = "fk_user_roles_user")]
public partial class UserRole
{
  [Key] [Column("user_role_id")] public int UserRoleId { get; set; }

  [Column("user_id")] public int UserId { get; set; }

  [Column("role_id")] public int RoleId { get; set; }

  [Column("created_at", TypeName = "datetime")]
  public DateTime? CreatedAt { get; set; }

  [Column("updated_at", TypeName = "datetime")]
  public DateTime? UpdatedAt { get; set; }

  [ForeignKey("RoleId")]
  [InverseProperty("UserRoles")]
  public virtual Role Role { get; set; } = null!;

  [ForeignKey("UserId")]
  [InverseProperty("UserRoles")]
  public virtual User User { get; set; } = null!;
}