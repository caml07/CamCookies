using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;

namespace cmcookies.Models;

public partial class CmcDBContext : DbContext
{
    public CmcDBContext()
    {
    }

    public CmcDBContext(DbContextOptions<CmcDBContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Batch> Batches { get; set; }

    public virtual DbSet<Billing> Billings { get; set; }

    public virtual DbSet<Cookie> Cookies { get; set; }

    public virtual DbSet<CookieMaterial> CookieMaterials { get; set; }

    public virtual DbSet<Customer> Customers { get; set; }

    public virtual DbSet<CustomerBilling> CustomerBillings { get; set; }

    public virtual DbSet<CustomerShipping> CustomerShippings { get; set; }

    public virtual DbSet<Material> Materials { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderDetail> OrderDetails { get; set; }

    public virtual DbSet<Phone> Phones { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Shipping> Shippings { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseMySql("server=localhost;database=cmcookiesdb;user=root;password=camyvik;port=3306", Microsoft.EntityFrameworkCore.ServerVersion.Parse("9.4.0-mysql"));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<Batch>(entity =>
        {
            entity.HasKey(e => e.BatchId).HasName("PRIMARY");

            entity.Property(e => e.ProducedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.QtyMade).HasDefaultValueSql("'20'");

            entity.HasOne(d => d.CookieCodeNavigation).WithMany(p => p.Batches)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_batches_cookie_code");
        });

        modelBuilder.Entity<Billing>(entity =>
        {
            entity.HasKey(e => e.BillingId).HasName("PRIMARY");

            entity.Property(e => e.BillingType).HasDefaultValueSql("'cash'");
        });

        modelBuilder.Entity<Cookie>(entity =>
        {
            entity.HasKey(e => e.CookieCode).HasName("PRIMARY");

            entity.Property(e => e.Category).HasDefaultValueSql("'normal'");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.Stock).HasDefaultValueSql("'0'");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<CookieMaterial>(entity =>
        {
            entity.HasKey(e => e.CookieMaterialId).HasName("PRIMARY");

            entity.HasOne(d => d.CookieCodeNavigation).WithMany(p => p.CookieMaterials)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cookiematerials_cookie_code");

            entity.HasOne(d => d.Material).WithMany(p => p.CookieMaterials)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_cookiematerials_material_id");
        });

        modelBuilder.Entity<Customer>(entity =>
        {
            entity.HasKey(e => e.CustomerId).HasName("PRIMARY");

            entity.HasOne(d => d.Phone).WithMany(p => p.Customers).HasConstraintName("fk_customer_id");

            entity.HasOne(d => d.User).WithMany(p => p.Customers)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_customers_user_id");
        });

        modelBuilder.Entity<CustomerBilling>(entity =>
        {
            entity.HasKey(e => e.CustomerBillingId).HasName("PRIMARY");

            entity.HasOne(d => d.Billing).WithMany(p => p.CustomerBillings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_custbill_billing_id");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerBillings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_custbill_customer_id");

            entity.HasOne(d => d.OrderDetail).WithMany(p => p.CustomerBillings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_custbill_order_detail_id");
        });

        modelBuilder.Entity<CustomerShipping>(entity =>
        {
            entity.HasKey(e => e.CustomerShippingsId).HasName("PRIMARY");

            entity.HasOne(d => d.Customer).WithMany(p => p.CustomerShippings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_custship_customer_id");

            entity.HasOne(d => d.OrderDetail).WithMany(p => p.CustomerShippings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_custship_orderdetail_id");

            entity.HasOne(d => d.Shipping).WithMany(p => p.CustomerShippings)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_custship_shipping_id");
        });

        modelBuilder.Entity<Material>(entity =>
        {
            entity.HasKey(e => e.MaterialId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.Status).HasDefaultValueSql("'pending'");
            entity.Property(e => e.Sticker).HasDefaultValueSql("'0'");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Customer).WithMany(p => p.Orders)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_orders_customer");
        });

        modelBuilder.Entity<OrderDetail>(entity =>
        {
            entity.HasKey(e => e.OrderDetailId).HasName("PRIMARY");

            entity.HasOne(d => d.CookieCodeNavigation).WithMany(p => p.OrderDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_order_details_cookie_code");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_order_details_order_id");
        });

        modelBuilder.Entity<Phone>(entity =>
        {
            entity.HasKey(e => e.PhoneId).HasName("PRIMARY");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PRIMARY");

            entity.Property(e => e.RoleType).HasDefaultValueSql("'customer'");
        });

        modelBuilder.Entity<Shipping>(entity =>
        {
            entity.HasKey(e => e.ShippingId).HasName("PRIMARY");

            entity.Property(e => e.ShippingType).HasDefaultValueSql("'on campus'");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.TransactionId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Batch).WithMany(p => p.Transactions)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_transactions_batch");

            entity.HasOne(d => d.Material).WithMany(p => p.Transactions)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_transactions_material");

            entity.HasOne(d => d.Order).WithMany(p => p.Transactions)
                .OnDelete(DeleteBehavior.SetNull)
                .HasConstraintName("fk_transactions_order");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.IsActive).HasDefaultValueSql("'1'");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.UserRoleId).HasName("PRIMARY");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.Property(e => e.UpdatedAt)
                .ValueGeneratedOnAddOrUpdate()
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasOne(d => d.Role).WithMany(p => p.UserRoles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_roles_role");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("fk_user_roles_user");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
