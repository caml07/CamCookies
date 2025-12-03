using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("cookies")]
public partial class Cookie
{
  [Key]
  [Column("cookie_code")]
  [StringLength(10)]
  public string CookieCode { get; set; } = null!;

  [Column("cookie_name")]
  [StringLength(50)]
  public string CookieName { get; set; } = null!;

  [Column("description")]
  [StringLength(255)]
  public string? Description { get; set; }

  [Column("price")] [Precision(10, 2)] public decimal Price { get; set; }

  [Column("category", TypeName = "enum('normal','seasonal')")]
  public string? Category { get; set; }

  [Column("image_path")]
  [StringLength(255)]
  public string? ImagePath { get; set; }

  [Column("stock")] public int? Stock { get; set; }

  [Column("is_active")] public bool? IsActive { get; set; }

  [Column("created_at", TypeName = "datetime")]
  public DateTime? CreatedAt { get; set; }

  [Column("updated_at", TypeName = "datetime")]
  public DateTime? UpdatedAt { get; set; }

  [InverseProperty("CookieCodeNavigation")]
  public virtual ICollection<Batch> Batches { get; set; } = new List<Batch>();

  [InverseProperty("CookieCodeNavigation")]
  public virtual ICollection<CookieMaterial> CookieMaterials { get; set; } = new List<CookieMaterial>();

  [InverseProperty("CookieCodeNavigation")]
  public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}