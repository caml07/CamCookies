using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("customer_billings")]
[Index("BillingId", Name = "fk_custbill_billing_id")]
[Index("CustomerId", Name = "fk_custbill_customer_id")]
[Index("OrderDetailId", Name = "fk_custbill_order_detail_id")]
public partial class CustomerBilling
{
  [Key] [Column("customer_billing_id")] public int CustomerBillingId { get; set; }

  [Column("customer_id")] public int CustomerId { get; set; }

  [Column("billing_id")] public int BillingId { get; set; }

  [Column("order_detail_id")] public int OrderDetailId { get; set; }

  [Column("amount")] [Precision(10, 2)] public decimal Amount { get; set; }

  [ForeignKey("BillingId")]
  [InverseProperty("CustomerBillings")]
  public virtual Billing Billing { get; set; } = null!;

  [ForeignKey("CustomerId")]
  [InverseProperty("CustomerBillings")]
  public virtual Customer Customer { get; set; } = null!;

  [ForeignKey("OrderDetailId")]
  [InverseProperty("CustomerBillings")]
  public virtual OrderDetail OrderDetail { get; set; } = null!;
}