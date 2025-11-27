using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("shipping")]
public partial class Shipping
{
    [Key]
    [Column("shipping_id")]
    public int ShippingId { get; set; }

    [Column("shipping_type", TypeName = "enum('on campus','outside campus')")]
    public string? ShippingType { get; set; }

    [Column("shipping_site")]
    [StringLength(100)]
    public string ShippingSite { get; set; } = null!;

    [InverseProperty("Shipping")]
    public virtual ICollection<CustomerShipping> CustomerShippings { get; set; } = new List<CustomerShipping>();
}
