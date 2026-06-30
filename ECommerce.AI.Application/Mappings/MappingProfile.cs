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
        CreateMap<Product, ProductDto>()
            .ForMember(dest => dest.SKU, opt => opt.MapFrom(src => src.SKU.Value))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price.Amount))
            .ForMember(dest => dest.ComparePrice, opt => opt.MapFrom(src => src.ComparePrice != null ? src.ComparePrice.Amount : (decimal?)null))
            .ForMember(dest => dest.Weight, opt => opt.MapFrom(src => src.Weight.Value))
            .ForMember(dest => dest.Dimensions, opt => opt.MapFrom(src => src.Dimensions != null ? src.Dimensions.ToString() : null))
            .ForMember(dest => dest.IsInStock, opt => opt.MapFrom(src => src.IsInStock()))
            .ForMember(dest => dest.IsLowStock, opt => opt.MapFrom(src => src.IsLowStock()))
            .ForMember(dest => dest.HasDiscount, opt => opt.MapFrom(src => src.HasDiscount()));

        // Category mappings
        CreateMap<Category, CategoryDto>();
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
}