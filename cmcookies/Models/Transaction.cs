using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("transactions")]
[Index("BatchId", Name = "fk_transactions_batch")]
[Index("MaterialId", Name = "fk_transactions_material")]
[Index("OrderId", Name = "fk_transactions_order")]
public partial class Transaction
{
  [Key] [Column("transaction_id")] public int TransactionId { get; set; }

  [Column("transaction_type", TypeName = "enum('sale','production','material_purchase')")]
  public string TransactionType { get; set; } = null!;

  [Column("amount")] [Precision(10, 2)] public decimal Amount { get; set; }

  [Column("description")]
  [StringLength(255)]
  public string? Description { get; set; }

  [Column("order_id")] public int? OrderId { get; set; }

  [Column("batch_id")] public int? BatchId { get; set; }

  [Column("material_id")] public int? MaterialId { get; set; }

  [Column("created_at", TypeName = "datetime")]
  public DateTime? CreatedAt { get; set; }

  [ForeignKey("BatchId")]
  [InverseProperty("Transactions")]
  public virtual Batch? Batch { get; set; }

  [ForeignKey("MaterialId")]
  [InverseProperty("Transactions")]
  public virtual Material? Material { get; set; }

  [ForeignKey("OrderId")]
  [InverseProperty("Transactions")]
  public virtual Order? Order { get; set; }
}