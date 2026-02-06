using CsvImportApp.Models;
using Microsoft.EntityFrameworkCore;

namespace CsvImportApp.Data;

public class CsvImportDbContext : DbContext
{
    public CsvImportDbContext(DbContextOptions<CsvImportDbContext> options)
        : base(options)
    {
    }

    public DbSet<CsvRecord> CsvRecords { get; set; }
    public DbSet<Product> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<CsvRecord>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Phone)
                .HasMaxLength(20);

            entity.Property(e => e.Address)
                .HasMaxLength(500);

            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);

            entity.HasIndex(e => e.Email)
                .IsUnique();
        });
        // ⭐ Product Configuration
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);

            entity.Property(e => e.ProductCode)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(255);

            entity.Property(e => e.Description)
                .HasMaxLength(2000);

            entity.Property(e => e.Price)
                .IsRequired()
                .HasColumnType("decimal(18,2)"); // ทศนิยม 2 ตำแหน่ง

            entity.Property(e => e.Cost)
                .IsRequired()
                .HasColumnType("decimal(18,2)");

            entity.Property(e => e.Stock)
                .IsRequired()
                .HasDefaultValue(0);

            entity.Property(e => e.MinStock)
                .HasDefaultValue(0);

            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500);

            entity.Property(e => e.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            entity.Property(e => e.CreatedAt)
                .IsRequired()
                .HasDefaultValueSql("GETUTCDATE()");

            entity.Property(e => e.UpdatedAt)
                .IsRequired(false);

            // Indexes
            entity.HasIndex(e => e.ProductCode)
                .IsUnique();

            entity.HasIndex(e => e.Name);

            /* entity.HasIndex(e => e.CategoryId);*/

            // Foreign Key Relationships
            /* entity.HasOne(e => e.Category)
                 .WithMany(c => c.Products)
                 .HasForeignKey(e => e.CategoryId)
                 .OnDelete(DeleteBehavior.Restrict); // ป้องกันลบ Category ถ้ามี Product*/

            /*entity.HasOne(e => e.Supplier)
                .WithMany(s => s.Products)
                .HasForeignKey(e => e.SupplierId)
                .OnDelete(DeleteBehavior.SetNull) // ถ้าลบ Supplier ให้ set null
                .IsRequired(false);*/
        });
    }
}
