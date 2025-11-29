using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("materials")]
public partial class Material
{
  [Key] [Column("material_id")] public int MaterialId { get; set; }

  [Column("name")] [StringLength(50)] public string Name { get; set; } = null!;

  [Column("unit")] [StringLength(50)] public string Unit { get; set; } = null!;

  [Column("stock")] [Precision(10, 2)] public decimal Stock { get; set; }

  [Column("unit_cost")]
  [Precision(10, 2)]
  public decimal UnitCost { get; set; }

  [Column("created_at", TypeName = "datetime")]
  public DateTime? CreatedAt { get; set; }

  [Column("updated_at", TypeName = "datetime")]
  public DateTime? UpdatedAt { get; set; }

  [InverseProperty("Material")]
  public virtual ICollection<CookieMaterial> CookieMaterials { get; set; } = new List<CookieMaterial>();

  [InverseProperty("Material")]
  public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}