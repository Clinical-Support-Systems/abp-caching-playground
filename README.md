# ABP Caching Playground with .NET Aspire

This repository demonstrates various caching strategies using ABP Framework with .NET Aspire integration. It includes examples of both Redis and Fusion caching implementations in a distributed environment.

## Todo

- [x] Setup basics
- [x] Data seeding
- [x] Can we get caching metrics in aspire?
- [ ] Make sure we're doing sensible caching of products
- [ ] Get fusion cache example working

## Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet) or later
- [Node.js](https://nodejs.org/) v18 or v20
- [Redis](https://redis.io/) (for distributed caching)
- [Docker](https://www.docker.com/) (optional, for containerized development)

## Solution Structure

The solution is organized into several projects:

### Aspire Projects
- `AbpCachingPlayground.FusionCacheAppHost` - Aspire host for Fusion Cache implementation
- `AbpCachingPlayground.RedisCacheAppHost` - Aspire host for Redis Cache implementation
- `AbpCachingPlayground.ServiceDefaults` - Shared service configurations

### Core Projects
- `AbpCachingPlayground.Domain` - Domain layer
- `AbpCachingPlayground.Domain.Shared` - Shared domain layer
- `AbpCachingPlayground.Application.Contracts` - Application contracts
- `AbpCachingPlayground.Application` - Application layer
- `AbpCachingPlayground.EntityFrameworkCore` - EF Core integration
- `AbpCachingPlayground.HttpApi` - HTTP API layer
- `AbpCachingPlayground.HttpApi.Client` - HTTP API client

### Web Applications
- `AbpCachingPlayground.AuthServer` - Authentication server
- `AbpCachingPlayground.HttpApi.Host` - API host
- `AbpCachingPlayground.Web` - Web application

### Utility Projects
- `AbpCachingPlayground.DbMigrator` - Database migration tool
- `AbpCachingPlayground.TestBase` - Testing infrastructure

## Getting Started

1. Clone the repository
```bash
git clone [your-repository-url]
cd AbpCachingPlayground
```

2. Install ABP CLI if you haven't already
```bash
dotnet tool install -g Volo.Abp.Cli
```

3. Install client-side dependencies
```bash
abp install-libs
```

4. Set up the database
```bash
cd src/AbpCachingPlayground.DbMigrator
dotnet run
```

5. Generate development certificates (if needed)
```bash
dotnet dev-certs https -v -ep openiddict.pfx -p your-secure-password
```

6. Start the application using Aspire
```bash
cd aspire/AbpCachingPlayground.RedisCacheAppHost # or FusionCacheAppHost
dotnet run
```

## Caching Implementations

### Redis Cache
The Redis implementation demonstrates distributed caching using Redis as the backing store. Key features include:
- Distributed cache implementation
- Cache invalidation strategies
- Cache-aside pattern examples

### Fusion Cache
The Fusion implementation showcases memory caching with additional features:
- In-memory caching
- Double-checked locking pattern
- Cache eviction policies

## Development Notes

- The solution uses .NET Aspire for orchestrating the microservices architecture
- Both Redis and Fusion cache implementations are available for comparison
- The `ServiceDefaults` project contains shared configuration that's applied across all services
- Test projects are available under the `test` directory for each implementation

## Configuration

### Redis Configuration
Update the Redis connection string in `appsettings.json`:
```json
{
  "Redis": {
    "Configuration": "localhost:6379"
  }
}
```

### Cache Settings
Cache settings can be modified in the respective host projects:
```json
{
  "Cache": {
    "DefaultExpirationTime": "00:30:00"
  }
}
```

## Testing

Run the tests using:
```bash
dotnet test
```

The test projects include:
- Unit tests
- Integration tests
- Cache implementation tests

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## Additional Resources

- [ABP Framework Documentation](https://docs.abp.io/)
- [.NET Aspire Documentation](https://learn.microsoft.com/en-us/dotnet/aspire/)
- [Redis Documentation](https://redis.io/documentation)

## License

This project is licensed under the MIT License - see the LICENSE file for details.