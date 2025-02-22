using System;
using System.Threading.Tasks;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Uow;
using AbpCachingPlayground.Products;

namespace AbpCachingPlayground.Products
{
    public class ProductsDataSeedContributor : IDataSeedContributor, ISingletonDependency
    {
        private bool IsSeeded = false;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public ProductsDataSeedContributor(IProductRepository productRepository, IUnitOfWorkManager unitOfWorkManager)
        {
            _productRepository = productRepository;
            _unitOfWorkManager = unitOfWorkManager;

        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (IsSeeded)
            {
                return;
            }

            await _productRepository.InsertAsync(new Product
            (
                id: Guid.Parse("e4e70e03-5940-4d1c-861b-f241e52e9c46"),
                name: "79f407aaca44414a9ba9cd0af653b320eea6977134b24f4da124642adf4a51d7",
                description: "d45feb908b8f41e29d974f18c789",
                price: 190109241
            ));

            await _productRepository.InsertAsync(new Product
            (
                id: Guid.Parse("f43678c4-14e6-4ae3-9c69-0d2ac0113372"),
                name: "afa4f4545c164ae4a1e57f25d38484fd044bba002ea049ad808095ca0d702e4de727",
                description: "57544cb0709d47288ba680999",
                price: 50633842
            ));

            await _unitOfWorkManager!.Current!.SaveChangesAsync();

            IsSeeded = true;
        }
    }
}