﻿using Xunit;

namespace AbpCachingPlayground.EntityFrameworkCore;

[CollectionDefinition(AbpCachingPlaygroundTestConsts.CollectionDefinitionName)]
public class AbpCachingPlaygroundEntityFrameworkCoreCollection : ICollectionFixture<AbpCachingPlaygroundEntityFrameworkCoreFixture>
{

}
