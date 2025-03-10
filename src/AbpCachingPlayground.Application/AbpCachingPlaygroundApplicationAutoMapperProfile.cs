using System;
using AbpCachingPlayground.Shared;
using Volo.Abp.AutoMapper;
using AbpCachingPlayground.Products;
using AutoMapper;

namespace AbpCachingPlayground;

public class AbpCachingPlaygroundApplicationAutoMapperProfile : Profile
{
    public AbpCachingPlaygroundApplicationAutoMapperProfile()
    {
        /* You can configure your AutoMapper mapping configuration here.
         * Alternatively, you can split your mapping configurations
         * into multiple profile classes for a better organization. */

        CreateMap<Product, ProductDto>();
        CreateMap<Product, ProductExcelDto>();
    }
}