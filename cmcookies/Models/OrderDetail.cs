using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("order_details")]
[Index("CookieCode", Name = "fk_order_details_cookie_code")]
[Index("OrderId", Name = "fk_order_details_order_id")]
public partial class OrderDetail
{
  [Key] [Column("order_detail_id")] public int OrderDetailId { get; set; }

  [Column("order_id")] public int OrderId { get; set; }

  [Column("cookie_code")] public int CookieCode { get; set; }

  [Column("qty")] public int Qty { get; set; }

  [Column("unit_price")]
  [Precision(10, 2)]
  public decimal UnitPrice { get; set; }

  [ForeignKey("CookieCode")]
  [InverseProperty("OrderDetails")]
  public virtual Cookie CookieCodeNavigation { get; set; } = null!;

  [InverseProperty("OrderDetail")]
  public virtual ICollection<CustomerBilling> CustomerBillings { get; set; } = new List<CustomerBilling>();

  [InverseProperty("OrderDetail")]
  public virtual ICollection<CustomerShipping> CustomerShippings { get; set; } = new List<CustomerShipping>();

  [ForeignKey("OrderId")]
  [InverseProperty("OrderDetails")]
  public virtual Order Order { get; set; } = null!;
}