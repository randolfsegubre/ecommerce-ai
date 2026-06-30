using ECommerce.AI.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.AI.Infrastructure.Data;

public class ECommerceDbContext : DbContext
{
    public ECommerceDbContext(DbContextOptions<ECommerceDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductSpecification> ProductSpecifications => Set<ProductSpecification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Category
        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).IsRequired().HasMaxLength(200);
            entity.Property(c => c.Description).IsRequired().HasMaxLength(1000);
            entity.Property(c => c.ImageUrl).HasMaxLength(500);
            entity.HasIndex(c => c.Name);
            
            // Self-referencing relationship
            entity.HasOne(c => c.ParentCategory)
                  .WithMany(c => c.SubCategories)
                  .HasForeignKey(c => c.ParentCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Global query filter for soft delete
            entity.HasQueryFilter(c => !c.IsDeleted);
        });

        // Configure Product
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).IsRequired().HasMaxLength(300);
            entity.Property(p => p.Description).IsRequired().HasMaxLength(2000);
            entity.Property(p => p.SKU).IsRequired().HasMaxLength(50);
            entity.Property(p => p.Price).HasColumnType("decimal(18,2)");
            entity.Property(p => p.ComparePrice).HasColumnType("decimal(18,2)");
            entity.Property(p => p.Brand).HasMaxLength(100);
            entity.Property(p => p.Model).HasMaxLength(100);
            entity.Property(p => p.Dimensions).HasMaxLength(100);
            
            entity.HasIndex(p => p.SKU).IsUnique();
            entity.HasIndex(p => p.Name);
            entity.HasIndex(p => new { p.CategoryId, p.IsActive });

            // Relationship with Category
            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);

            // Global query filter for soft delete
            entity.HasQueryFilter(p => !p.IsDeleted);
        });

        // Configure ProductImage
        modelBuilder.Entity<ProductImage>(entity =>
        {
            entity.HasKey(pi => pi.Id);
            entity.Property(pi => pi.ImageUrl).IsRequired().HasMaxLength(500);
            entity.Property(pi => pi.AltText).HasMaxLength(200);
            
            entity.HasIndex(pi => new { pi.ProductId, pi.SortOrder });

            // Relationship with Product
            entity.HasOne(pi => pi.Product)
                  .WithMany(p => p.Images)
                  .HasForeignKey(pi => pi.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Global query filter for soft delete
            entity.HasQueryFilter(pi => !pi.IsDeleted);
        });

        // Configure ProductSpecification
        modelBuilder.Entity<ProductSpecification>(entity =>
        {
            entity.HasKey(ps => ps.Id);
            entity.Property(ps => ps.Name).IsRequired().HasMaxLength(100);
            entity.Property(ps => ps.Value).IsRequired().HasMaxLength(500);
            
            entity.HasIndex(ps => new { ps.ProductId, ps.SortOrder });

            // Relationship with Product
            entity.HasOne(ps => ps.Product)
                  .WithMany(p => p.Specifications)
                  .HasForeignKey(ps => ps.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Global query filter for soft delete
            entity.HasQueryFilter(ps => !ps.IsDeleted);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries().Where(e => e.State == EntityState.Modified))
        {
            if (entry.Entity is ECommerce.AI.Domain.Common.BaseEntity entity)
            {
                entity.MarkAsUpdated();
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}