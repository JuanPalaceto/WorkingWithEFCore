using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace WorkingWithEFCore.AutoGen;

public partial class Northwind : DbContext
{
    public Northwind()
    {
    }

    public Northwind(DbContextOptions<Northwind> options)
        : base(options)
    {
    }

    public virtual DbSet<Category> Categories { get; set; }

    public virtual DbSet<Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see http://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=Northwind;User Id=sa;Password=12345;TrustServerCertificate=True");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.UseCollation("SQL_Latin1_General_CP1_CI_AS");

        modelBuilder.Entity<Product>(entity =>
        {
            entity.Property(e => e.ReorderLevel).HasDefaultValueSql("(0)");
            entity.Property(e => e.UnitPrice).HasDefaultValueSql("(0)");
            entity.Property(e => e.UnitsInStock).HasDefaultValueSql("(0)");
            entity.Property(e => e.UnitsOnOrder).HasDefaultValueSql("(0)");

            entity.HasOne(d => d.Category).WithMany(p => p.Products).HasConstraintName("FK_Products_Categories");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
