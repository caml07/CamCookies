using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("billing")]
public partial class Billing
{
  [Key] [Column("billing_id")] public int BillingId { get; set; }

  [Column("billing_type", TypeName = "enum('cash','card')")]
  public string? BillingType { get; set; }

  [InverseProperty("Billing")]
  public virtual ICollection<CustomerBilling> CustomerBillings { get; set; } = new List<CustomerBilling>();
}