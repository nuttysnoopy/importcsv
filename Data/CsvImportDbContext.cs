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

            entity.HasIndex(e => e.Email)
                .IsUnique();
        });
    }
}
