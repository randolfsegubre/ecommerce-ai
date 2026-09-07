using ECommerce.AI.Domain.Entities;
using ECommerce.AI.Domain.ValueObjects;

namespace ECommerce.AI.Infrastructure.Data;

/// <summary>
/// Seeds sample data for local development so the API and UI have something to show on first run.
/// </summary>
public static class DbInitializer
{
    public static async Task SeedAsync(ECommerceDbContext context)
    {
        if (context.Categories.Any())
            return;

        var electronics = new Category("Electronics", "Electronic devices and accessories");
        var laptops = new Category("Laptops", "Laptop and notebook computers", parentCategoryId: null);
        context.Categories.AddRange(electronics, laptops);
        await context.SaveChangesAsync();

        var product1 = new Product(
            name: "Wireless Mouse",
            description: "Ergonomic wireless mouse with USB receiver",
            sku: new ProductSKU("ELEC-MOUSE-001"),
            price: new Money(19.99m),
            stockQuantity: 150,
            minStockLevel: 20,
            weight: new ProductWeight(0.2),
            categoryId: electronics.Id,
            brand: "Logitech");

        var product2 = new Product(
            name: "Mechanical Keyboard",
            description: "RGB backlit mechanical keyboard with blue switches",
            sku: new ProductSKU("ELEC-KEYB-001"),
            price: new Money(79.99m),
            stockQuantity: 60,
            minStockLevel: 10,
            weight: new ProductWeight(0.9),
            categoryId: electronics.Id,
            brand: "Corsair");

        var product3 = new Product(
            name: "14-inch Ultrabook",
            description: "Lightweight 14-inch laptop, 16GB RAM, 512GB SSD",
            sku: new ProductSKU("LAP-ULTRA-001"),
            price: new Money(999.00m),
            stockQuantity: 25,
            minStockLevel: 5,
            weight: new ProductWeight(1.3),
            categoryId: laptops.Id,
            brand: "Dell",
            model: "XPS 14");

        product1.Images.Add(new ProductImage("https://picsum.photos/seed/mouse/400/300", product1.Id, "Wireless Mouse", 0, true));
        product2.Images.Add(new ProductImage("https://picsum.photos/seed/keyboard/400/300", product2.Id, "Mechanical Keyboard", 0, true));
        product3.Images.Add(new ProductImage("https://picsum.photos/seed/ultrabook/400/300", product3.Id, "14-inch Ultrabook", 0, true));

        context.Products.AddRange(product1, product2, product3);
        await context.SaveChangesAsync();
    }
}
