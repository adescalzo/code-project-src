```yaml
---
title: Caching in .NET using Fusion Cache
source: https://www.nikolatech.net/blogs/fusion-cache-in-aspnetcore
date_published: 2025-02-05T02:10:41.315Z
date_captured: 2025-08-08T21:42:05.729Z
domain: www.nikolatech.net
author: Unknown
category: performance
technologies: [.NET, FusionCache, Redis, ASP.NET Core, NuGet, StackExchangeRedis, OpenTelemetry, IDistributedCache, IMemoryCache, HybridCache, Entity Framework Core, System.Text.Json]
programming_languages: [C#, SQL, Shell]
tags: [caching, dotnet, performance, redis, distributed-caching, in-memory-caching, fusioncache, hybridcache, data-access, application-performance]
key_concepts: [multi-level-caching, cache-stampede, fail-safe, cache-tagging, dependency-injection, distributed-caching, in-memory-caching, data-resiliency]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces FusionCache, an advanced caching library designed for .NET applications to enhance performance and reduce load. It details FusionCache's capabilities in managing both in-memory (L1) and distributed (L2) caching, highlighting features like cache stampede prevention, fail-safe mechanisms, and tagging. The post provides practical code examples for integrating FusionCache with ASP.NET Core and Redis, demonstrating common caching operations such as `SetAsync`, `GetOrSetAsync`, and `RemoveAsync`. It also positions FusionCache as the first production-ready implementation of Microsoft's `HybridCache` abstraction. The author concludes by recommending FusionCache for its comprehensive features, resiliency, and excellent documentation.
---
```

# Caching in .NET using Fusion Cache

# In this article

*   [Intro](#Intro)
*   [Fusion Cache](#FusionCache)
*   [Cache Levels](#CacheLevels)
*   [Getting Started](#GettingStarted)
*   [Redis](#Redis)
*   [Basic Usage](#BasicUsage)
*   [HybridCache](#HybridCache)
*   [Tagging](#Tagging)
*   [Conclusion](#Conclusion)

![Banner showing "Fusion Cache" and "Multi-Level Caching" with a stylized "NK" logo and a sloth character with a lightning bolt on its face.](https://coekcx.github.io/BlogImages/banners/fusion-cache-in-aspnetcore-banner.png)

#### Caching in .NET using Fusion Cache

###### 05 Feb 2025

###### 8 min

### Special Thanks to Our Sponsors:

**Stefan Djokic** is launching his YouTube channel and building **The CodeMan Community** - your hub for .NET content, mini-courses, and expert advice!

**The first 100 members get in for just $4/month! 🚀**

Join now and grab his first ebook for free: [https://www.skool.com/thecodeman](https://www.skool.com/thecodeman)

**Caching** is a crucial feature when you want to enhance performance and reduce the load on your application.

In complex systems, applications may need both in-memory and distributed caching to complement each other.

Memory cache improves local performance, while distributed cache enables scaling and ensures availability across applications in your system.

In .NET, we have [IDistributedCache](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.distributed.idistributedcache?view=net-9.0-pp) and [IMemoryCache](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.memory.imemorycache?view=net-9.0-pp), but these interfaces are relatively basic and lack several advanced features. Additionally, managing them in parallel can be challenging.

**HybridCache** could potentially replace these interfaces in your application and address some of the missing features.

However, based on current insights, once it’s released, it will most likely be a less powerful version of FusionCache.

## FusionCache

**FusionCache** is a powerful caching library designed to provide high performance, resilience and flexibility when handling data caching.

It’s user-friendly while also providing a range of advanced features:

*   **L1+L2 Caching** - You can manage both in-memory and distributed caching.
*   **Cache Stampede** - Prevents redundant parallel requests for the same data, improving efficiency under load.
*   **Fail-Safe** - Mechanism that allows you to reuse an expired entry as a temporary fallback avoiding transient failures.
*   **Tagging** - Enables grouping of cache entries for easier management, such as bulk invalidation by tags.
*   **OpenTelemetry** - Native support for OpenTelemetry.

This is just a small selection of features I’ve used in the past.

For a full list, it's best to check them on GitHub. The link is provided below.

### Cache Levels

*   The first level, **L1** (In-Memory), stores data locally in the application's memory, providing rapid access to frequently requested data with minimal overhead.
*   The second level, **L2** (Distributed), supports larger, scalable storage solutions, such as Redis. This layer ensures that data can be available across multiple instances and environments.

## Getting Started

To get started with **Fusion Cache**, you'll first need to install the necessary NuGet packages. You can do this via the NuGet Package Manager or by running the following command in the Package Manager Console:

```shell
dotnet add package ZiggyCreatures.FusionCache
```

To start using FusionCache, you’ll need to register it to your dependency injection setup. This can be done simply using the **AddFusionCache** method:

```csharp
builder.Services
    .AddFusionCache();
```

For this example I will also use redis as my distributed cache.

### Redis

To use Redis all you need to do is to add [StackExchangeRedis](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.stackexchangeredis.rediscache?view=net-9.0-pp) package and configure it alongside with FusionCache like this:

```csharp
builder.Services
    .AddFusionCache()
    .WithDistributedCache(_ =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Redis");
        var options = new RedisCacheOptions { Configuration = connectionString };

        return new RedisCache(options);
    })
    .WithSerializer(new FusionCacheSystemTextJsonSerializer());
```

Once Redis is registered, FusionCache will use it as the secondary cache and use the serializer configured for it.

## Basic Usage

FusionCache offers a set of methods to manage cache entries and perform various operations.

Here’s an overview of its methods:

### SetAsync

When you want to set a new or update an exisisting entry you can use **SetAsync** method.

```csharp
public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
{
    var product = await dbContext.Products
        .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

    if (product is null)
    {
        return Result.NotFound();
    }

    product.Update(request.Name, request.Description, request.Price);

    await dbContext.SaveChangesAsync(cancellationToken);

    await cache.SetAsync(
        $"products-{product.Id}",
        product,
        token: cancellationToken);

    return Result.Success();
}
```

It's a useful method when you want to store an object without retrieving it first.

### GetOrSetAsync

When you want to retrieve data and add data to cache if it's not found you can use **GetOrSetAsync** method.

```csharp
public async Task<Result<ProductResponse>> Handle(Query request, CancellationToken cancellationToken)
{
    var key = $"products-{request.Id}";

    var product = await cache.GetOrSetAsync(
        key,
        async _ =>
        {
            return await dbContext.Products
                .FirstOrDefaultAsync(
                    p => p.Id == request.Id,
                    cancellationToken);
        },
        token: cancellationToken);

    return product is null
        ? Result.NotFound()
        : Result.Success(product.Adapt<ProductResponse>());
}
```

### RemoveAsync

When the underlying data for a cache entry changes before it expires, you can remove the entry explicitly by calling **RemoveAsync** with the key to the entry.

```csharp
public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
{
    var product = await dbContext.Products
        .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

    if (product is null)
    {
        return Result.NotFound();
    }

    dbContext.Products.Remove(product);

    await dbContext.SaveChangesAsync(cancellationToken);

    var key = $"products-{product.Id}";

    await cache.RemoveAsync(key, token: cancellationToken);

    return Result.Success();
}
```

By default, all methods will update both L1 and L2 caches unless specified otherwise. You can configure the cache to control the behavior of each cache level independently, allowing for more granular control over the caching strategy.

```csharp
public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
{
    var product = new Product(
        Guid.NewGuid(),
        DateTime.UtcNow,
        request.Name,
        request.Description,
        request.Price);

    dbContext.Products.Add(product);

    await dbContext.SaveChangesAsync(cancellationToken);

    var key = $"products-{product.Id}";

    await cache.SetAsync(
        key,
        product,
        token: cancellationToken,
        options: new FusionCacheEntryOptions(TimeSpan.FromMinutes(5))
            .SetSkipDistributedCache(true, true));

    return Result.Success(product.Id);
}
```

## HybridCache Abstraction

[HybridCache](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.caching.hybrid.hybridcache?view=net-9.0-pp) is the abstraction for caching implementation and FusionCache is the first production-ready implementation of it.

It's not just the first third-party implementation, but the very first implementation overall, even ahead of Microsoft's own official release.

Here is an example how to use HybridCache with FusionCache implementation:

```csharp
builder.Services
    .AddFusionCache()
    .WithDistributedCache(_ =>
    {
        var connectionString = builder.Configuration.GetConnectionString("Redis");
        var options = new RedisCacheOptions { Configuration = connectionString };

        return new RedisCache(options);
    })
    .WithSerializer(new FusionCacheSystemTextJsonSerializer())
    .AsHybridCache();
```

```csharp
internal sealed class Handler(
    IApplicationDbContext dbContext,
    HybridCache cache) : IRequestHandler<Query, Result<ProductResponse>>
{
    public async Task<Result<ProductResponse>> Handle(Query request, CancellationToken cancellationToken)
    {
        var key = $"products-{product.Id}";

        var product = await cache.GetOrCreateAsync(
            key,
            async _ =>
            {
                return await dbContext.Products
                    .FirstOrDefaultAsync(
                        p => p.Id == request.Id,
                        cancellationToken);
            },
            cancellationToken: cancellationToken);

        return product is null
            ? Result.NotFound()
            : Result.Success(product.Adapt<ProductResponse>());
    }
}
```

## Tagging

From v2 FusionCache also supports tagging. Tags can be used to group cache entries and invalidate them together.

For example you could set tags when calling **GetOrSetAsync** method:

```csharp
public async Task<Result<ProductResponse>> Handle(Query request, CancellationToken cancellationToken)
{
    string[] tags = ["products"];

    var key = $"products-{product.Id}";

    var product = await cache.GetOrSetAsync(
        key,
        async _ =>
        {
            return await dbContext.Products
                .FirstOrDefaultAsync(
                    p => p.Id == request.Id,
                    cancellationToken);
        },
        tags: tags,
        token: cancellationToken);

    return product is null
        ? Result.NotFound()
        : Result.Success(product.Adapt<ProductResponse>());
}
```

And later on you can remove cache entries by a tag or multiple tags using **RemoveByTagAsync** method:

```csharp
public async Task<Result> Handle(Command request, CancellationToken cancellationToken)
{
    await dbContext.Products.ExecuteDeleteAsync(cancellationToken);

    await cache.RemoveByTagAsync(CacheTags, token: cancellationToken);

    return Result.Success();
}
```

## Conclusion

The story of FusionCache is too large for a single blog post, so I’ll leave a deep dive into its advanced features and a comparison with Microsoft's HybridCache implementation for future posts.

I first learned about FusionCache when I needed a fail-safe mechanism for an application and since then, it has become my go-to caching library.

Not only was it the first to offer L1+L2 caching, but FusionCache also includes a variety of useful features and advanced resiliency.

Additionally, I must comment the excellent documentation provided by FusionCache, which is perfect for anyone wanting to explore the library’s capabilities in depth.

Feel free to check out the project on GitHub and give it a star: [Fusion Cache Github](https://github.com/ZiggyCreatures/FusionCache)

If you want to check out examples I created, you can find the source code here:

[Source Code](https://www.nikolatech.net/codes/fusion-cache-in-dotnet-example)

I hope you enjoyed it, subscribe and get a notification when a new blog is up!

# Subscribe

###### Stay tuned for valuable insights every Thursday morning.

##### Share This Article: