using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("cookie_materials")]
[Index("CookieCode", Name = "fk_cookiematerials_cookie_code")]
[Index("MaterialId", Name = "fk_cookiematerials_material_id")]
public partial class CookieMaterial
{
    [Key]
    [Column("cookie_material_id")]
    public int CookieMaterialId { get; set; }

    [Column("cookie_code")]
    public int CookieCode { get; set; }

    [Column("material_id")]
    public int MaterialId { get; set; }

    [Column("consumption_per_batch")]
    [Precision(10, 2)]
    public decimal ConsumptionPerBatch { get; set; }

    [ForeignKey("CookieCode")]
    [InverseProperty("CookieMaterials")]
    public virtual Cookie CookieCodeNavigation { get; set; } = null!;

    [ForeignKey("MaterialId")]
    [InverseProperty("CookieMaterials")]
    public virtual Material Material { get; set; } = null!;
}
