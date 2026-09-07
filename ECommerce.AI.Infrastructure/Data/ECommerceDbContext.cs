using ECommerce.AI.Domain.Entities;
using ECommerce.AI.Domain.ValueObjects;
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

            // Value objects: EF Core can't map these directly, so each gets an explicit
            // conversion to/from the scalar column type it's persisted as.
            entity.Property(p => p.SKU)
                  .HasField("_sku") // EF's convention lowercases only the first letter ("sKU"), which doesn't match "_sku"
                  .HasConversion(sku => sku.Value, value => new ProductSKU(value))
                  .IsRequired()
                  .HasMaxLength(50);

            entity.Property(p => p.Price)
                  .HasConversion(price => price.Amount, amount => new Money(amount, "USD"))
                  .HasColumnType("decimal(18,2)");

            entity.Property(p => p.ComparePrice)
                  .HasConversion(
                      price => price == null ? (decimal?)null : price.Amount,
                      amount => amount == null ? null : new Money(amount.Value, "USD"))
                  .HasColumnType("decimal(18,2)");

            entity.Property(p => p.Weight)
                  .HasConversion(weight => weight.Value, value => new ProductWeight(value, "kg"));

            entity.Property(p => p.Dimensions)
                  .HasConversion(
                      dimensions => dimensions == null ? null : dimensions.ToString(),
                      value => value == null ? null : ProductDimensions.FromString(value, "cm"))
                  .HasMaxLength(100);

            entity.Property(p => p.Brand).HasMaxLength(100);
            entity.Property(p => p.Model).HasMaxLength(100);

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

            // The public ImageUrl property unwraps its backing field to a plain string, so EF
            // can't map it directly (type mismatch with the field). Map the field itself as a
            // field-only property instead, and exclude the computed public property.
            entity.Ignore(pi => pi.ImageUrl);
            entity.Property<ImageUrl>("_imageUrl")
                  .HasColumnName("ImageUrl")
                  .HasConversion(url => url.Value, value => new ImageUrl(value))
                  .IsRequired()
                  .HasMaxLength(500);

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

            // Name/Value unwrap their backing value-object fields to plain strings, so EF can't
            // map them directly (type mismatch with the field). Map the fields themselves as
            // field-only properties instead, and exclude the computed public properties.
            entity.Ignore(ps => ps.Name);
            entity.Ignore(ps => ps.Value);

            entity.Property<SpecificationName>("_name")
                  .HasColumnName("Name")
                  .HasConversion(name => name.Value, value => new SpecificationName(value))
                  .IsRequired()
                  .HasMaxLength(100);

            entity.Property<SpecificationValue>("_value")
                  .HasColumnName("Value")
                  .HasConversion(val => val.Value, value => new SpecificationValue(value))
                  .IsRequired()
                  .HasMaxLength(500);
            
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