using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("orders")]
[Index("CustomerId", Name = "fk_orders_customer")]
public partial class Order
{
    [Key]
    [Column("order_id")]
    public int OrderId { get; set; }

    [Column("customer_id")]
    public int CustomerId { get; set; }

    [Column("status", TypeName = "enum('pending','on_preparation','delivered','cancelled')")]
    public string? Status { get; set; }

    [Column("bag", TypeName = "enum('small','medium')")]
    public string? Bag { get; set; }

    [Column("sticker")]
    public bool? Sticker { get; set; }

    [Column("created_at", TypeName = "datetime")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at", TypeName = "datetime")]
    public DateTime? UpdatedAt { get; set; }

    [ForeignKey("CustomerId")]
    [InverseProperty("Orders")]
    public virtual Customer Customer { get; set; } = null!;

    [InverseProperty("Order")]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
