using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("customer_shippings")]
[Index("CustomerId", Name = "fk_custship_customer_id")]
[Index("OrderDetailId", Name = "fk_custship_orderdetail_id")]
[Index("ShippingId", Name = "fk_custship_shipping_id")]
public partial class CustomerShipping
{
    [Key]
    [Column("customer_shippings_id")]
    public int CustomerShippingsId { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("shipping_id")]
    public int ShippingId { get; set; }

    [Column("order_detail_id")]
    public int OrderDetailId { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("CustomerShippings")]
    public virtual Customer Customer { get; set; } = null!;

    [ForeignKey("OrderDetailId")]
    [InverseProperty("CustomerShippings")]
    public virtual OrderDetail OrderDetail { get; set; } = null!;

    [ForeignKey("ShippingId")]
    [InverseProperty("CustomerShippings")]
    public virtual Shipping Shipping { get; set; } = null!;
}
