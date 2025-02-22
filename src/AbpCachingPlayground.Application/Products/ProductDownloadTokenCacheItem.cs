using System;

namespace AbpCachingPlayground.Products;

public abstract class ProductDownloadTokenCacheItemBase
{
    public string Token { get; set; } = null!;
}