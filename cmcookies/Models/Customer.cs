using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("customers")]
[Index("PhoneId", Name = "fk_customer_id")]
[Index("UserId", Name = "fk_customers_user_id")]
public partial class Customer
{
  [Key] [Column("customer_id")] public int CustomerId { get; set; }

  [Column("user_id")] public int UserId { get; set; }

  [Column("phone_id")] public int? PhoneId { get; set; }

  [InverseProperty("Customer")]
  public virtual ICollection<CustomerBilling> CustomerBillings { get; set; } = new List<CustomerBilling>();

  [InverseProperty("Customer")]
  public virtual ICollection<CustomerShipping> CustomerShippings { get; set; } = new List<CustomerShipping>();

  [InverseProperty("Customer")] public virtual ICollection<Order> Orders { get; set; } = new List<Order>();

  [ForeignKey("PhoneId")]
  [InverseProperty("Customers")]
  public virtual Phone? Phone { get; set; }

  [ForeignKey("UserId")]
  [InverseProperty("Customers")]
  public virtual User User { get; set; } = null!;
}