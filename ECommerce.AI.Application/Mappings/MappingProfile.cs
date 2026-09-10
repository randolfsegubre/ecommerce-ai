using AutoMapper;
using ECommerce.AI.Application.DTOs;
using ECommerce.AI.Application.DTOs.Commands;
using ECommerce.AI.Domain.Entities;
using ECommerce.AI.Domain.ValueObjects;

namespace ECommerce.AI.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Product mappings
        // Category is mapped as a shallow summary (ToCategorySummaryDto), not the full recursive
        // Category -> CategoryDto map: EF's relationship fixup wires Product.Category and
        // Category.Products to the same tracked entities, so mapping the full graph here would
        // walk straight back into this same product (and, via SubCategories, every sibling).
        // AutoMapper's MaxDepth was tried first but throttles unrelated sibling collections
        // (Images/Specifications) too, since the depth counter isn't scoped per member.
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.SKU, opt => opt.MapFrom(src => src.SKU.Value))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.ComparePrice, opt => opt.MapFrom(src => src.ComparePrice != null ? src.ComparePrice.Amount : (decimal?)null))
            .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Weight.Value))
            .ForMember(dest => dest.Dimensions, opt => opt.MapFrom(src => src.Dimensions != null ? src.Dimensions.ToString() : null))
            .ForMember(dest => dest.Category, opt => opt.MapFrom(src => ToCategorySummaryDto(src.Category)))
            .ForMember(dest => dest.IsInStock, opt => opt.MapFrom(src => src.IsInStock()))
            .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.IsLowStock()))
            .ForMember(dest => dest.HasDiscount, opt => opt.MapFrom(src => src.HasDiscount()));

        // Category mappings
        // ParentCategory is likewise mapped as a shallow summary: a real category hierarchy makes
        // Category <-> ParentCategory <-> SubCategories a genuine bidirectional cycle once EF loads
        // more than one level, even though today's seed data is flat and never exercises it.
        CreateMap<Category, CategoryDto>()
            .ForMember(dest => dest.ParentCategory, opt => opt.MapFrom(src => ToCategorySummaryDto(src.ParentCategory)));
        CreateMap<CreateCategoryCommand, Category>()
            .ConstructUsing(src => new Category(src.Name, src.Description, src.ImageUrl, src.ParentCategoryId));

        // Product Image mappings
        CreateMap<ProductImage, ProductImageDto>()
            .ForMember(dest => dest.ImageUrl, opt => opt.MapFrom(src => src.ImageUrl)); // Map from value object

        // Product Specification mappings
        CreateMap<ProductSpecification, ProductSpecificationDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name)) // Map from value object
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value)); // Map from value object
    }

    /// <summary>
    /// Projects a Category into a CategoryDto without its Products/SubCategories/ParentCategory
    /// navigation, for use wherever a Category is nested inside another mapped object. Breaks the
    /// Product/Category and Category/ParentCategory cycles without limiting unrelated siblings.
    /// </summary>
    private static CategoryDto? ToCategorySummaryDto(Category? category)
    {
        if (category is null)
            return null;

        return new CategoryDto
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ImageUrl = category.ImageUrl,
            IsActive = category.IsActive,
            ParentCategoryId = category.ParentCategoryId,
            CreatedAt = category.CreatedAt,
            UpdatedAt = category.UpdatedAt
        };
    }
}