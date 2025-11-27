using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace cmcookies.Models;

[Table("phones")]
[Index("Phone1", Name = "phone_1", IsUnique = true)]
[Index("Phone2", Name = "phone_2", IsUnique = true)]
public partial class Phone
{
    [Key]
    [Column("phone_id")]
    public int PhoneId { get; set; }

    [Column("phone_1")]
    [StringLength(20)]
    public string Phone1 { get; set; } = null!;

    [Column("phone_2")]
    [StringLength(20)]
    public string? Phone2 { get; set; }

    [InverseProperty("Phone")]
    public virtual ICollection<Customer> Customers { get; set; } = new List<Customer>();
}
