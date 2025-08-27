```yaml
---
title: HybridCache in ASP.NET Core - New Caching Library
source: https://www.milanjovanovic.tech/blog/hybrid-cache-in-aspnetcore-new-caching-library?utm_source=LinkedIn&utm_medium=social&utm_campaign=11.08.2025
date_published: 2024-11-16T00:00:00.000Z
date_captured: 2025-08-15T11:19:14.447Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: performance
technologies: [ASP.NET Core, .NET 9, HybridCache, IMemoryCache, IDistributedCache, Redis, SQL Server, NuGet, JetBrains Rider, Entity Framework Core]
programming_languages: [C#, PowerShell, SQL]
tags: [caching, aspnet-core, .net, performance, distributed-caching, in-memory-caching, hybrid-cache, dotnet-9, web-development, data-access]
key_concepts: [caching, in-memory-caching, distributed-caching, cache-stampede, tag-based-invalidation, two-level-caching, dependency-injection, serialization]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces HybridCache, a new caching library in .NET 9 designed to simplify and enhance caching in ASP.NET Core applications. It addresses the limitations of traditional in-memory and distributed caching by combining both into a two-level (L1/L2) system. The post explains key features like protection against cache stampede, tag-based invalidation, and configurable serialization. It provides practical C# code examples demonstrating how to install, configure, and use HybridCache for common scenarios like getting/creating, setting, and removing cache entries, including integration with Redis as an L2 cache. The library aims to streamline complex caching problems for building fast and scalable applications.]
---
```

# HybridCache in ASP.NET Core - New Caching Library

![Blog cover image for HybridCache in ASP.NET Core, featuring the title "HybridCache New Caching Library, .NET 9" on a blue and black background with geometric patterns.](https://www.milanjovanovic.tech/blog-covers/mnw_116.png?imwidth=3840)

# HybridCache in ASP.NET Core - New Caching Library

5 min read · November 16, 2024

🎉 [**JetBrains Rider**](https://www.jetbrains.com/rider/?utm_campaign=rider_free&utm_content=site&utm_medium=cpc&utm_source=milan_jovanovic_newsletter) is now free for non-commercial use. Great news for all hobbyists, students, content creators, and open source contributors! Now you can use Rider, a cross-platform .NET and game dev IDE, for non-commercial development for free. [**Download and start today**](https://www.jetbrains.com/rider/?utm_campaign=rider_free&utm_content=site&utm_medium=cpc&utm_source=milan_jovanovic_newsletter)!

Remember when APIs were an afterthought? We're [**not going back**](https://shortclick.link/djx5wv). 74% of devs surveyed in the 2024 State of the API Report are API-first, up from 66% in 2023. What else? We're getting faster. 63% of devs can produce an API within a week, up from 47% in 2023. Collaboration on APIs is still a challenge. APIs are increasingly revenue generators. Read the [**rest of the report**](https://shortclick.link/djx5wv) for fresh insights.

[

Sponsor this newsletter

](/sponsor-the-newsletter)

[**Caching**](caching-in-aspnetcore-improving-application-performance) is essential for building fast, scalable applications. ASP.NET Core has traditionally offered two caching options: in-memory caching and distributed caching. Each has its trade-offs. In-memory caching using `IMemoryCache` is fast but limited to a single server. Distributed caching with `IDistributedCache` works across multiple servers using a backplane.

.NET 9 introduces `HybridCache`, a new library that combines the best of both approaches. It prevents common caching problems like cache stampede. It also adds useful features like tag-based invalidation and better performance monitoring.

In this week's issue, I'll show you how to use `HybridCache` in your applications.

## What is HybridCache?

The traditional caching options in ASP.NET Core have limitations. In-memory caching is fast but limited to one server. Distributed caching works across servers but is slower.

[HybridCache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid) combines both approaches and adds important features:

*   Two-level caching (L1/L2)
    *   L1: Fast in-memory cache
    *   L2: Distributed cache (Redis, SQL Server, etc.)
*   Protection against [cache stampede](https://en.wikipedia.org/wiki/Cache_stampede) (when many requests hit an empty cache at once)
*   Tag-based cache invalidation
*   Configurable serialization
*   Metrics and monitoring

The L1 cache runs in your application's memory. The L2 cache can be Redis, SQL Server, or any other distributed cache. You can use HybridCache with just the L1 cache if you don't need distributed caching.

## Installing HybridCache

Install the `Microsoft.Extensions.Caching.Hybrid` NuGet package:

```powershell
Install-Package Microsoft.Extensions.Caching.Hybrid
```

Add `HybridCache` to your services:

```csharp
builder.Services.AddHybridCache(options =>
{
    // Maximum size of cached items
    options.MaximumPayloadBytes = 1024 * 1024 * 10; // 10MB
    options.MaximumKeyLength = 512;

    // Default timeouts
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(30),
        LocalCacheExpiration = TimeSpan.FromMinutes(30)
    };
});
```

For custom types, you can add your own serializer:

```csharp
builder.Services.AddHybridCache()
    .AddSerializer<CustomType, CustomSerializer>();
```

## Using HybridCache

`HybridCache` provides several methods to work with cached data. The most important ones are `GetOrCreateAsync`, `SetAsync`, and various remove methods. Let's see how to use each one in real-world scenarios.

### Getting or Creating Cache Entries

The `GetOrCreateAsync` method is your main tool for working with cached data. It handles both cache hits and misses automatically. If the data isn't in the cache, it calls your factory method to get the data, caches it, and returns it.

Here's an endpoint that gets product details:

```csharp
app.MapGet("/products/{id}", async (
    int id,
    HybridCache cache,
    ProductDbContext db,
    CancellationToken ct) =>
{
    var product = await cache.GetOrCreateAsync(
        $"product-{id}",
        async token =>
        {
            return await db.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id, token);
        },
        cancellationToken: ct
    );

    return product is null ? Results.NotFound() : Results.Ok(product);
});
```

In this example:

*   The cache key is unique per product
*   If the product is in the cache, it's returned immediately
*   If not, the factory method runs to get the data
*   Other concurrent requests for the same product wait for the first one to finish

### Setting Cache Entries Directly

Sometimes you need to update the cache directly, like after modifying data. The `SetAsync` method handles this:

```csharp
app.MapPut("/products/{id}", async (int id, Product product, HybridCache cache) =>
{
    // First update the database
    await UpdateProductInDatabase(product);

    // Then update the cache with custom expiration
    var options = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromHours(1),
        LocalCacheExpiration = TimeSpan.FromMinutes(30)
    };

    await cache.SetAsync(
        $"product-{id}",
        product,
        options
    );

    return Results.NoContent();
});
```

Key points about `SetAsync`:

*   It updates both L1 and L2 cache
*   You can specify different timeouts for L1 and L2
*   It overwrites any existing value for the same key

### Using Cache Tags

Tags are powerful for managing groups of related cache entries. You can invalidate multiple entries at once using tags:

```csharp
app.MapGet("/categories/{id}/products", async (
    int id,
    HybridCache cache,
    ProductDbContext db,
    CancellationToken ct) =>
{
    var tags = [$"category-{id}", "products"];

    var products = await cache.GetOrCreateAsync(
        $"products-by-category-{id}",
        async token =>
        {
            return await db.Products
                .Where(p => p.CategoryId == id)
                .Include(p => p.Category)
                .ToListAsync(token);
        },
        tags: tags,
        cancellationToken: ct
    );

    return Results.Ok(products);
});

// Endpoint to invalidate all products in a category
app.MapPost("/categories/{id}/invalidate", async (
    int id,
    HybridCache cache,
    CancellationToken ct) =>
{
    await cache.RemoveByTagAsync($"category-{id}", ct);

    return Results.NoContent();
});
```

Tags are useful for:

*   Invalidating all products in a category
*   Clearing all cached data for a specific user
*   Refreshing all related data when something changes

### Removing Single Entries

For direct cache invalidation of specific items, use `RemoveAsync`:

```csharp
app.MapDelete("/products/{id}", async (int id, HybridCache cache) =>
{
    // First delete from database
    await DeleteProductFromDatabase(id);

    // Then remove from cache
    await cache.RemoveAsync($"product-{id}");

    return Results.NoContent();
});
```

`RemoveAsync`:

*   Removes the item from both L1 and L2 cache
*   Works immediately, no delay
*   Does nothing if the key doesn't exist
*   Is safe to call multiple times

Remember that `HybridCache` handles all the complexity of distributed caching, serialization, and stampede protection for you. You just need to focus on your cache keys and when to invalidate the cache.

## Adding Redis as L2 Cache

To use [Redis](https://redis.io/) as your distributed cache:

1.  Install the `Microsoft.Extensions.Caching.StackExchangeRedis` NuGet package:

```powershell
Install-Package Microsoft.Extensions.Caching.StackExchangeRedis
```

2.  Configure Redis and `HybridCache`:

```csharp
// Add Redis
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "your-redis-connection-string";
});

// Add HybridCache - it will automatically use Redis as L2
builder.Services.AddHybridCache();
```

`HybridCache` will automatically detect and use Redis as the L2 cache.

## Summary

`HybridCache` simplifies caching in .NET applications. It combines fast in-memory caching with distributed caching, prevents common problems like cache stampede, and works well in both single-server and distributed systems.

Start with the default settings and basic usage patterns - the library is designed to be simple to use while solving complex caching problems.

Thanks for reading.

And stay awesome!

---

Whenever you're ready, there are 4 ways I can help you:

1.  [Pragmatic Clean Architecture:](/pragmatic-clean-architecture) Join 4,200+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [Modular Monolith Architecture:](/modular-monolith-architecture) Join 2,100+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [(NEW) Pragmatic REST APIs:](/pragmatic-rest-apis) Join 1,200+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
4.  [Patreon Community:](https://www.patreon.com/milanjovanovic) Join a community of 5,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Become a Better .NET Software Engineer

Join 70,000+ engineers who are improving their skills every Saturday morning.

Join 70K+ Engineers

.formkit-form\[data-uid="134c4e25db"\] \*{box-sizing:border-box;}.formkit-form\[data-uid="134c4e25db"\]{-webkit-font-smoothing:antialiased;-moz-osx-font-smoothing:grayscale;}.formkit-form\[data-uid="134c4e25db"\] legend{border:none;font-size:inherit;margin-bottom:10px;padding:0;position:relative;display:table;}.formkit-form\[data-uid="134c4e25db"\] fieldset{border:0;padding:0.01em 0 0 0;margin:0;min-width:0;}.formkit-form\[data-uid="134c4e25db"\] body:not(:-moz-handler-blocked) fieldset{display:table-cell;}.formkit-form\[data-uid="134c4e25db"\] h1,.formkit-form\[data-uid="134c4e25db"\] h2,.formkit-form\[data-uid="134c4e25db"\] h3,.formkit-form\[data-uid="134c4e25db"\] h4,.formkit-form\[data-uid="134c4e25db"\] h5,.formkit-form\[data-uid="134c4e25db"\] h6{color:inherit;font-size:inherit;font-weight:inherit;}.formkit-form\[data-uid="134c4e25db"\] h2{font-size:1.5em;margin:1em 0;}.formkit-form\[data-uid="134c4e25db"\] h3{font-size:1.17em;margin:1em 0;}.formkit-form\[data-uid="134c4e25db"\] p{color:inherit;font-size:inherit;font-weight:inherit;}.formkit-form\[data-uid="134c4e25db"\] ol:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] ul:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] blockquote:not(\[template-default\]){text-align:left;}.formkit-form\[data-uid="134c4e25db"\] p:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] hr:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] blockquote:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] ol:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] ul:not(\[template-default\]){color:inherit;font-style:initial;}.formkit-form\[data-uid="134c4e25db"\] .ordered-list,.formkit-form\[data-uid="134c4e25db"\] .unordered-list{list-style-position:outside !important;padding-left:1em;}.formkit-form\[data-uid="134c4e25db"\] .list-item{padding-left:0;}.formkit-form\[data-uid="134c4e25db"\]\[data-format="modal"\]{display:none;}.formkit-form\[data-uid="134c4e25db"\]\[data-format="slide in"\]{display:none;}.formkit-form\[data-uid="134c4e25db"\]\[data-format="sticky bar"\]{display:none;}.formkit-sticky-bar .formkit-form\[data-uid="134c4e25db"\]\[data-format="sticky bar"\]{display:block;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input,.formkit-form\[data-uid="134c4e25db"\] .formkit-select,.formkit-form\[data-uid="134c4e25db"\] .formkit-checkboxes{width:100%;}.formkit-form\[data-uid="134c4e25db"\] .formkit-button,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit{border:0;border-radius:5px;color:#ffffff;cursor:pointer;display:inline-block;text-align:center;font-size:15px;font-weight:500;cursor:pointer;margin-bottom:15px;overflow:hidden;padding:0;position:relative;vertical-align:middle;}.formkit-form\[data-uid="134c4e25db"\] .formkit-button:hover,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:hover,.formkit-form\[data-uid="134c4e25db"\] .formkit-button:focus,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:focus{outline:none;}.formkit-form\[data-uid="134c4e25db"\] .formkit-button:hover > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:hover > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-button:focus > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:focus > span{background-color:rgba(0,0,0,0.1);}.formkit-form\[data-uid="134c4e25db"\] .formkit-button > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit > span{display:block;-webkit-transition:all 300ms ease-in-out;transition:all 300ms ease-in-out;padding:12px 24px;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input{background:#ffffff;font-size:15px;padding:12px;border:1px solid #e3e3e3;-webkit-flex:1 0 auto;-ms-flex:1 0 auto;flex:1 0 auto;line-height:1.4;margin:0;-webkit-transition:border-color ease-out 300ms;transition:border-color ease-out 300ms;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input:focus{outline:none;border-color:#1677be;-webkit-transition:border-color ease 300ms;transition:border-color ease 300ms;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input::-webkit-input-placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input::-moz-placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input:-ms-input-placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input::placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="dropdown"\]{position:relative;display:inline-block;width:100%;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="dropdown"\]::before{content:"";top:calc(50% - 2.5px);right:10px;position:absolute;pointer-events:none;border-color:#4f4f4f transparent transparent transparent;border-style:solid;border-width:6px 6px 0 6px;height:0;width:0;z-index:999;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="dropdown"\] select{height:auto;width:100%;cursor:pointer;color:#333333;line-height:1.4;margin-bottom:0;padding:0 6px;-webkit-appearance:none;-moz-appearance:none;appearance:none;font-size:15px;padding:12px;padding-right:25px;border:1px solid #e3e3e3;background:#ffffff;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="dropdown"\] select:focus{outline:none;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\]{text-align:left;margin:0;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\]{margin-bottom:10px;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] \*{cursor:pointer;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\]:last-of-type{margin-bottom:0;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] input\[type="checkbox"\]{display:none;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] input\[type="checkbox"\] + label::after{content:none;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] input\[type="checkbox"\]:checked + label::after{border-color:#ffffff;content:"";}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] input\[type="checkbox"\]:checked + label::before{background:#10bf7a;border-color:#10bf7a;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] label{position:relative;display:inline-block;padding-left:28px;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] label::before,.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] label::after{position:absolute;content:"";display:inline-block;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] label::before{height:16px;width:16px;border:1px solid #e3e3e3;background:#ffffff;left:0px;top:3px;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] label::after{height:4px;width:8px;border-left:2px solid #4d4d4d;border-bottom:2px solid #4d4d4d;-webkit-transform:rotate(-45deg);-ms-transform:rotate(-45deg);transform:rotate(-45deg);left:4px;top:8px;}.formkit-form\[data-uid="134c4e25db"\] .formkit-alert{background:#f9fafb;border:1px solid #e3e3e3;border-radius:5px;-webkit-flex:1 0 auto;-ms-flex:1 0 auto;flex:1 0 auto;list-style:none;margin:25px auto;padding:12px;text-align:center;width:100%;}.formkit-form\[data-uid="134c4e25db"\] .formkit-alert:empty{display:none;}.formkit-form\[data-uid="134c4e25db"\] .formkit-alert-success{background:#d3fbeb;border-color:#10bf7a;color:#0c905c;}.formkit-form\[data-uid="134c4e25db"\] .formkit-alert-error{background:#fde8e2;border-color:#f2643b;color:#ea4110;}.formkit-form\[data-uid="134c4e25db"\] .formkit-spinner{display:-webkit-box;display:-webkit-flex;display:-ms-flexbox;display:flex;height:0px;width:0px;margin:0 auto;position:absolute;top:0;left:0;right:0;width:0px;overflow:hidden;text-align:center;-webkit-transition:all 300ms ease-in-out;transition:all 300ms ease-in-out;}.formkit-form\[data-uid="134c4e25db"\] .formkit-spinner > div{margin:auto;width:12px;height:12px;background-color:#fff;opacity:0.3;border-radius:100%;display:inline-block;-webkit-animation:formkit-bouncedelay-formkit-form-data-uid-134c4e25db- 1.4s infinite ease-in-out both;animation:formkit-bouncedelay-formkit-form-data-uid-134c4e25db- 1.4s infinite ease-in-out both;}.formkit-form\[data-uid="134c4e25db"\] .formkit-spinner > div:nth-child(1){-webkit-animation-delay:-0.32s;animation-delay:-0.32s;}.formkit-form\[data-uid="134c4e25db"\] .formkit-spinner > div:nth-child(2){-webkit-animation-delay:-0.16s;animation-delay:-0.16s;}.formkit-form\[data-uid="134c4e25db"\] .formkit-submit\[data-active\] .formkit-spinner{opacity:1;height:100%;width:50px;}.formkit-form\[data-uid="134c4e25db"\] .formkit-submit\[data-active\] .formkit-spinner ~ span{opacity:0;}.formkit-form\[data-uid="134c4e25db"\] .formkit-powered-by\[data-active="false"\]{opacity:0.35;}.formkit-form\[data-uid="134c4e25db"\] .formkit-powered-by-convertkit-container{display:-webkit-box;display:-webkit-flex;display:-ms-flexbox;display:flex;width:100%;margin:10px 0;position:relative;}.formkit-form\[data-uid="134c4e25db"\] .formkit-powered-by-convertkit-container\[data-active="false"\]{opacity:0.35;}.formkit-form\[data-uid="134c4e25db"\] .formkit-powered-by-convertkit{-webkit-align-items:center;-webkit-box-align:center;-ms-flex-align:center;align-items:center;background-color:#ffffff;border-radius:9px;color:#3d3d3d;cursor:pointer;display:block;height:36px;margin:0 auto;opacity:0.95;padding:0;-webkit-text-decoration:none;text-decoration:none;text-indent:100%;-webkit-transition:ease-in-out all 200ms;transition:ease-in-out all 200ms;white-space:nowrap;overflow:hidden;-webkit-user-select:none;-moz-user-select:none;-ms-user-select:none;user-select:none;width:157px;background-repeat:no-repeat;background-position:center;background-image:url("data:image/svg+xml;charset=utf8,%3Csvg width='133' height='36' viewBox='0 0 133 36' fill='none' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath d='M0.861 25.5C0.735 25.5 0.651 25.416 0.651 25.29V10.548C0.651 10.422 0.735 10.338 0.861 10.338H6.279C9.072 10.338 10.668 11.451 10.668 13.824