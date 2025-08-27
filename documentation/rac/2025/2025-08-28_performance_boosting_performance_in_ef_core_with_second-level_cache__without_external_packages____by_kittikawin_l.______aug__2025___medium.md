```yaml
---
title: "Boosting Performance in EF Core with Second-Level Cache (Without External Packages) | by Kittikawin L. 🍀 | Aug, 2025 | Medium"
source: https://medium.com/@kittikawin_ball/boosting-performance-in-ef-core-with-second-level-cache-without-external-packages-29acf4faad09
date_published: 2025-08-28T15:01:44.486Z
date_captured: 2025-09-03T12:09:22.691Z
domain: medium.com
author: Kittikawin L. 🍀
category: performance
technologies: [Entity Framework Core, .NET, ASP.NET Core, IMemoryCache, IDistributedCache, Redis, SQL Server, System.Text.Json]
programming_languages: [C#, SQL]
tags: [ef-core, caching, performance, dotnet, database, data-access, memory-cache, distributed-cache, redis, sql-server]
key_concepts: [second-level-cache, first-level-cache, performance-optimization, repository-pattern, dependency-injection, in-memory-caching, distributed-caching, serialization]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explains how to implement second-level caching in Entity Framework Core applications without relying on external NuGet packages. It differentiates between first-level and second-level caching, highlighting the latter's importance for performance in high-traffic scenarios. The author demonstrates practical implementations using .NET's built-in `IMemoryCache` for single-server applications and `IDistributedCache` (with examples for Redis and SQL Server) for scalable, multi-server environments. The article also covers integrating caching with the Repository Pattern and provides best practices for effective cache utilization, ultimately aiming to reduce database load and improve application responsiveness.]
---
```

# Boosting Performance in EF Core with Second-Level Cache (Without External Packages) | by Kittikawin L. 🍀 | Aug, 2025 | Medium

# Boosting Performance in EF Core with Second-Level Cache (Without External Packages)

![A close-up view of the internal components of a hard disk drive (HDD), showing the shiny platter and the read/write head mechanism. This image visually represents data storage and retrieval, relevant to database operations and caching.](https://miro.medium.com/v2/resize:fit:700/0*Nl-Fq05Z676BCWMB)

Entity Framework Core (EF Core) simplifies database access in .NET applications, but it has a significant drawback. Every new `DbContext` always executes queries against the database, even if the same query has been executed previously.

In high-traffic applications, this **repeated database access** can quickly become a performance bottleneck.

Fortunately, you don’t need any external NuGet packages to enable caching. The .NET already provides the tools you need — `**IMemoryCache**` and `**IDistributedCache**`.

## First-Level vs. Second-Level Cache in EF Core

### First-level cache

Built into EF Core. It tracks entities within a single `DbContext`. Once that context is disposed, the cache is cleared.

### Second-level cache

A shared cache across multiple `DbContext` instances. Queries are executed once and reused by subsequent calls, even across different contexts.

While EF Core doesn’t include a second-level cache by default, we can implement one using ASP.NET Core caching mechanisms.

## Option 1: Using IMemoryCache

For single-server applications, `IMemoryCache` is the simplest caching option.

### Example: Caching Active Products

```csharp
public class ProductService  
{  
    private readonly ApplicationDbContext _context;  
    private readonly IMemoryCache _cache;  
    public ProductService(ApplicationDbContext context, IMemoryCache cache)  
    {  
        _context = context;  
        _cache = cache;  
    }  
    public async Task<List<Product>> GetActiveProductsAsync()  
    {  
        string cacheKey = "active_products";  
        if (!_cache.TryGetValue(cacheKey, out List<Product> products))  
        {  
            products = await _context.Products  
                .Where(p => p.IsActive)  
                .ToListAsync();  
            var cacheOptions = new MemoryCacheEntryOptions()  
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));  
            _cache.Set(cacheKey, products, cacheOptions);  
        }  
        return products;  
    }  
}
```

### How it works

*   The first call runs SQL and caches results for 5 minutes.
*   Subsequent calls return data directly from memory.

**Pros**: Fast, lightweight  
**Cons**: Not shared across servers (works only in-memory per instance)

## Option 2: Using IDistributedCache (e.g., Redis, SQL Server)

For distributed applications or APIs running on multiple servers, a **shared cache** is essential. The .NET provides `IDistributedCache`, which supports providers such as _Redis_ and _SQL Server_.

### Example: Caching with Redis

```csharp
public class ProductService  
{  
    private readonly ApplicationDbContext _context;  
    private readonly IDistributedCache _cache;  
    public ProductService(ApplicationDbContext context, IDistributedCache cache)  
    {  
        _context = context;  
        _cache = cache;  
    }  
    public async Task<List<Product>> GetActiveProductsAsync()  
    {  
        string cacheKey = "active_products";  
        var cachedData = await _cache.GetStringAsync(cacheKey);  
        if (!string.IsNullOrEmpty(cachedData))  
        {  
            return JsonSerializer.Deserialize<List<Product>>(cachedData);  
        }  
        var products = await _context.Products  
            .Where(p => p.IsActive)  
            .ToListAsync();  
        var options = new DistributedCacheEntryOptions()  
            .SetAbsoluteExpiration(TimeSpan.FromMinutes(5));  
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(products), options);  
        return products;  
    }  
}
```

### How it works

*   Cached results are stored in Redis (or another provider).
*   Works across multiple app servers.
*   Ideal for cloud deployments and microservices.

**Pros**: Scalable across servers  
**Cons**: Requires serialization and deserialization

## Option 3: Repository Layer with Cache Wrapper

If you use the **Repository Pattern**, caching logic can be centralized in the repository itself.

```csharp
public interface IProductRepository  
{  
    Task<List<Product>> GetActiveProductsAsync();  
}  
  
public class CachedProductRepository : IProductRepository  
{  
    private readonly IProductRepository _inner;  
    private readonly IMemoryCache _cache;  
    public CachedProductRepository(IProductRepository inner, IMemoryCache cache)  
    {  
        _inner = inner;  
        _cache = cache;  
    }  
    public async Task<List<Product>> GetActiveProductsAsync()  
    {  
        return await _cache.GetOrCreateAsync("active_products", async entry =>  
        {  
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);  
            return await _inner.GetActiveProductsAsync();  
        });  
    }  
}
```

## Best Practices

*   Cache read-heavy queries  
    **e.g.**, product catalogs, reference data.
*   Avoid caching frequently changing data  
    **e.g.**, order status, account balances.
*   Use expiration policies to minimize stale data.
*   Monitor cache hit/miss ratios to validate effectiveness.

## Conclusion

Although EF Core doesn’t provide second-level caching out of the box, you can implement it easily with built-in tools. With `IMemoryCache` or `IDistributedCache`, you can build a straightforward and effective caching layer.

*   Use `**IMemoryCache**` for small applications or single-server deployments.
*   Use `**IDistributedCache**` (Redis or SQL Server) for scalable, cloud-ready solutions.

By applying these techniques, you can improve EF Core performance, reduce database load, and deliver faster, more efficient .NET applications.

## Thanks for reading!