using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("batches")]
[Index("CookieCode", Name = "fk_batches_cookie_code")]
public partial class Batch
{
  [Key] [Column("batch_id")] public int BatchId { get; set; }

  [Column("cookie_code")]
  [StringLength(10)]
  public string CookieCode { get; set; } = null!;

  [Column("qty_made")] public int QtyMade { get; set; }

  [Column("total_cost")]
  [Precision(10, 2)]
  public decimal? TotalCost { get; set; }

  [Column("produced_at", TypeName = "datetime")]
  public DateTime? ProducedAt { get; set; }

  [ForeignKey("CookieCode")]
  [InverseProperty("Batches")]
  public virtual Cookie Cookie { get; set; } = null!;

  [InverseProperty("Batch")]
  public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}