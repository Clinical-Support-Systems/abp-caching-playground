using AbpCachingPlayground.Web.Pages.Products;
using Volo.Abp.AutoMapper;
using AbpCachingPlayground.Products;
using AutoMapper;

namespace AbpCachingPlayground.Web;

public class AbpCachingPlaygroundWebAutoMapperProfile : Profile
{
    public AbpCachingPlaygroundWebAutoMapperProfile()
    {
        //Define your AutoMapper configuration here for the Web project.

        CreateMap<ProductDto, ProductUpdateViewModel>();
        CreateMap<ProductUpdateViewModel, ProductUpdateDto>();
        CreateMap<ProductCreateViewModel, ProductCreateDto>();
    }
}