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
        }
    }
}

