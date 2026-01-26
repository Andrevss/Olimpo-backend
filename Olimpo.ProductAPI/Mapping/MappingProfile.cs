using AutoMapper;
using Olimpo.ProductAPI.Model.Entities;
using Olimpo.ProductAPI.Model.DTOs;

namespace Olimpo.ProductAPI.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ========== CATEGORY MAPPINGS ==========
            CreateMap<Category, CategoryDTO>();
            CreateMap<CreateCategoryDTO, Category>();
            CreateMap<UpdateCategoryDTO, Category>();

            // ========== PRODUCT MAPPINGS ==========
            CreateMap<Product, ProductDTO>()
                .ForMember(dest => dest.CategoryName,
                          opt => opt.MapFrom(src => src.Category.Name));

            CreateMap<CreateProductDTO, Product>();
            CreateMap<UpdateProductDTO, Product>();

            // ========== ORDER MAPPINGS ==========
            CreateMap<Order, OrderDTO>()
            .ForMember(dest => dest.Status,
                      opt => opt.MapFrom(src => src.Status.ToString()));

            CreateMap<CreateOrderDTO, Order>()
                .ForMember(dest => dest.OrderItems, opt => opt.Ignore())
                .ForMember(dest => dest.Total, opt => opt.Ignore())
                .ForMember(dest => dest.Status, opt => opt.Ignore());

            CreateMap<OrderItem, OrderItemDTO>();

            CreateMap<CreateOrderItemDTO, OrderItem>()
                .ForMember(dest => dest.ProductName, opt => opt.Ignore())
                .ForMember(dest => dest.UnitPrice, opt => opt.Ignore())
                .ForMember(dest => dest.TotalPrice, opt => opt.Ignore());
        }
    }
}

