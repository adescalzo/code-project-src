```yaml
---
title: "Caching in ASP.NET Core: Improving Application Performance"
source: https://www.milanjovanovic.tech/blog/caching-in-aspnetcore-improving-application-performance?utm_source=newsletter&utm_medium=email&utm_campaign=tnw93
date_published: 2024-06-08T00:00:00.000Z
date_captured: 2025-08-21T16:55:32.412Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: performance
technologies: [ASP.NET Core, .NET 9, Redis, SQL Server, Entity Framework Core, StackExchange.Redis, Microsoft.Extensions.Caching.StackExchangeRedis]
programming_languages: [C#, SQL]
tags: [caching, aspnet-core, performance, redis, distributed-caching, in-memory-caching, dotnet, web-development, data-access, optimization]
key_concepts: [caching, in-memory-caching, distributed-caching, cache-aside-pattern, dependency-injection, cache-expiration, cache-stampede, concurrency-control, hybrid-cache]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores caching techniques in ASP.NET Core to enhance application performance. It details the use of `IMemoryCache` for in-process caching and `IDistributedCache` for distributed scenarios, specifically demonstrating integration with Redis. The author explains the Cache-Aside pattern with practical C# code examples, including considerations for cache expiration. Furthermore, the article addresses advanced topics like the cache stampede problem and introduces the upcoming `HybridCache` in .NET 9, emphasizing the importance of concurrency control.]
---
```

# Caching in ASP.NET Core: Improving Application Performance

![Blog cover image with text "Caching in ASP.NET Core" on a blue and black geometric background.](data:image/svg+xml,%3csvg%20xmlns=%27http://www.w3.org/2000/svg%27%20version=%271.1%27%20width=%271280%27%20height=%27720%27/%3e)![Caching in ASP.NET Core: Improving Application Performance](/blog-covers/mnw_093.png?imwidth=3840)

![Blog cover image with text "Caching in ASP.NET Core" on a blue and black geometric background.](/blog-covers/mnw_093.png?imwidth=3840)

# Caching in ASP.NET Core: Improving Application Performance

6 min read · June 08, 2024

[**dotConnect**](https://www.devart.com/dotconnect/?utm_medium=cpm&utm_source=milanjovanovic&utm_campaign=dotconnect-general) is a feature-rich ADO.NET provider for developing data-related .NET applications. It's fast, reliable, and works for all major databases and cloud services. dotConnect is compatible with all Entity Framework and EF Core versions. [**Start FREE trial today**](https://www.devart.com/dotconnect/?utm_medium=cpm&utm_source=milanjovanovic&utm_campaign=dotconnect-general)!

Use [**Postman V11 collaboration features**](https://learning.postman.com/docs/collaborating-in-postman/collaborate-in-postman-overview/) to collaborate with your team and API Consumers. Features include changing your workspace visibility, assigning roles to collaborators, sharing workspaces, and using Postman's API version control. [**Learn more here**](https://learning.postman.com/docs/collaborating-in-postman/collaborate-in-postman-overview/)!

[

Sponsor this newsletter

](/sponsor-the-newsletter)

Caching is one of the simplest techniques to significantly improve your application's performance. It's the process of temporarily storing data in a faster access location. You will typically cache the results of expensive operations or frequently accessed data.

Caching allows subsequent requests for the same data to be served from the cache instead of fetching the data from its source.

ASP.NET Core offers several types of caches, such as `IMemoryCache`, `IDistributedCache`, and the upcoming `HybridCache` (.NET 9).

In this newsletter, we will explore how to implement [caching in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/memory) applications.

## [How Caching Improves Application Performance](#how-caching-improves-application-performance)

Caching improves your application's performance by reducing latency and server load while enhancing scalability and user experience.

*   **Faster data retrieval**: Cached data can be accessed much faster than retrieving it from the source (like a database or an API). Caches are typically stored in memory (RAM).
*   **Fewer database queries**: Caching frequently accessed data reduces the number of database queries. This reduces the load on the database server.
*   **Lower CPU usage**: Rendering web pages or processing API responses can consume significant CPU resources. Caching the results reduces the need for repetitive CPU-intensive tasks.
*   **Handling increased traffic**: By reducing the load on backend systems, caching allows your application to handle more concurrent users and requests.
*   **Distributed caching**: Distributed cache solutions like [Redis](https://redis.io/) enable scaling the cache across multiple servers, further improving performance and resilience.

In a recent project I worked on, we used Redis to scale to more than 1,000,000 users. We only had one SQL Server instance with a read-replica for reporting. The power of caching, eh?

## [Caching Abstractions in ASP.NET Core](#caching-abstractions-in-aspnet-core)

ASP.NET Core provides two primary abstractions for working with caches:

*   `IMemoryCache`: Stores data in the memory of the web server. Simple to use but not suitable for distributed scenarios.
*   `IDistributedCache`: Offers a more robust solution for distributed applications. It allows you to store cached data in a distributed cache like Redis.

We have to register these services with DI to use them. `AddDistributedMemoryCache` will configure the in-memory implementation of `IDistributedCache`, which isn't distributed.

```csharp
builder.Services.AddMemoryCache();

builder.Services.AddDistributedMemoryCache();
```

Here's how you can use the `IMemoryCache`. We will first check if the cached value is present and return it directly if it's there. Otherwise, we must fetch the value from the database and cache it for subsequent requests.

```csharp
app.MapGet(
    "products/{id}",
    (int id, IMemoryCache cache, AppDbContext context) =>
    {
        if (!cache.TryGetValue(id, out Product product))
        {
            product = context.Products.Find(id);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(10))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2));

            cache.Set(id, product, cacheEntryOptions);
        }

        return Results.Ok(product);
    });
```

Cache expiration is another important topic to discuss. We want to remove cache entries that aren't used and become stale. You can pass in the `MemoryCacheEntryOptions`, allowing you to configure cache expiration. For example, we can set the `AbsoluteExpiration` and `SlidingExpiration` values to control when the cache entry will expire.

## [Cache-Aside Pattern](#cache-aside-pattern)

The cache-aside pattern is the most common caching strategy. Here's how it works:

1.  **Check the cache**: Look for the requested data in the cache.
2.  **Fetch from source (if cache miss)**: If the data isn't in the cache, fetch it from the source.
3.  **Update the cache**: Store the fetched data in the cache for subsequent requests.

![Diagram illustrating the Cache-Aside pattern workflow. It shows a client requesting data, the application checking the cache, fetching from the database if not found, updating the cache, and returning data to the client.](data:image/svg+xml,%3csvg%20xmlns=%27http://www.w3.org/2000/svg%27%20version=%271.1%27%20width=%271301%27%20height=%27826%27/%3e)![Cache-aside pattern.](/blogs/mnw_093/cache_aside.png?imwidth=3840)

Here's how you can implement the cache-aside pattern as an extension method for `IDistributedCache`:

```csharp
public static class DistributedCacheExtensions
{
    public static DistributedCacheEntryOptions DefaultExpiration => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
    };

    public static async Task<T> GetOrCreateAsync<T>(
        this IDistributedCache cache,
        string key,
        Func<Task<T>> factory,
        DistributedCacheEntryOptions? cacheOptions = null)
    {
        var cachedData = await cache.GetStringAsync(key);

        if (cachedData is not null)
            return JsonSerializer.Deserialize<T>(cachedData);

        var data = await factory();

        await cache.SetStringAsync(
            key,
            JsonSerializer.Serialize(data),
            cacheOptions ?? DefaultExpiration);

        return data;
    }
}
```

We're using `JsonSerializer` to manage serialization to and from a JSON string. The `SetStringAsync` method also accepts a `DistributedCacheEntryOptions` argument to control cache expiration.

Here's how we would use this extension method:

```csharp
app.MapGet(
    "products/{id}",
    (int id, IDistributedCache cache, AppDbContext context) =>
    {
        var product = cache.GetOrCreateAsync($"products-{id}", async () =>
        {
            var productFromDb = await context.Products.FindAsync(id);

            return productFromDb;
        });

        return Results.Ok(product);
    });
```

## [Pros and Cons of In-Memory Caching](#pros-and-cons-of-in-memory-caching)

Pros:

*   Extremely fast
*   Simple to implement
*   No external dependencies

Cons:

*   Cache data is lost if the server restarts
*   Limited to the memory (RAM) of a single server
*   Cache data is not shared across multiple instances of your application

## [Distributed Caching With Redis](#distributed-caching-with-redis)

[Redis](https://redis.io/) is a popular in-memory data store often used as a high-performance distributed cache. To use Redis in your ASP.NET Core application, you can use the `StackExchange.Redis` library.

However, there's also the `Microsoft.Extensions.Caching.StackExchangeRedis` library, allowing you to integrate Redis with `IDistributedCache`.

```powershell
Install-Package Microsoft.Extensions.Caching.StackExchangeRedis
```

Here's how you can configure it with DI by providing a connection string to Redis:

```csharp
string connectionString = builder.Configuration.GetConnectionString("Redis");

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = connectionString;
});
```

An alternative approach is to register an `IConnectionMultiplexer` as a service. Then, we will use it to provide a function for the `ConnectionMultiplexerFactory`.

```csharp
string connectionString = builder.Configuration.GetConnectionString("Redis");

IConnectionMultiplexer connectionMultiplexer =
    ConnectionMultiplexer.Connect(connectionString);

builder.Services.AddSingleton(connectionMultiplexer);

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.ConnectionMultiplexerFactory =
        () => Task.FromResult(connectionMultiplexer);
});
```

Now, when you inject `IDistributedCache`, it will use Redis under the hood.

## [Cache Stampede and HybridCache](#cache-stampede-and-hybridcache)

The in-memory cache implementations in ASP.NET Core are susceptible to race conditions, which can cause a cache stampede. A [cache stampede](https://en.wikipedia.org/wiki/Cache_stampede) happens when concurrent requests encounter a cache miss and try to fetch the data from the source. This can overload your application and negate the benefits of caching.

Locking is one solution for the cache stampede problem. .NET offers many options for [locking and concurrency control](introduction-to-locking-and-concurrency-control-in-dotnet-6). The most commonly used locking primitives are the `lock` statement and the `Semaphore` (or `SemaphoreSlim`) class.

Here's how we could use `SemaphoreSlim` to introduce locking before fetching data:

```csharp
public static class DistributedCacheExtensions
{
    private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1, 1);

    // Arguments omitted for brevity
    public static async Task<T> GetOrCreateAsync<T>(...)
    {
        // Fetch data from cache, and return if present

        // Cache miss
        try
        {
            await Semaphore.WaitAsync();

            // Check if the data was added to the cache by another request

            // If not, proceed to fetch data and cache it
            var data = await factory();

            await cache.SetStringAsync(
                key,
                JsonSerializer.Serialize(data),
                cacheOptions ?? DefaultExpiration);
        }
        finally
        {
            Semaphore.Release();
        }

        return data;
    }
}
```

The previous implementation has a lock contention issue since all requests have to wait for the semaphore. A much better solution would be locking based on the `key` value.

.NET 9 introduces a new caching abstraction called [`HybridCache`](hybrid-cache-in-aspnetcore-new-caching-library), which aims to solve the shortcomings of `IDistributedCache`. Learn more about this in the [Hybrid cache documentation](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid).

## [Summary](#summary)

Caching is a powerful technique for improving web application performance. ASP.NET Core's caching abstractions make it easy to implement various caching strategies.

We can choose from `IMemoryCache` for in-memory cache and `IDistributedCache` for distributed caching.

Here are a few guidelines to wrap up this week's issue:

*   Use `IMemoryCache` for simple, in-memory caching
*   Implement the cache aside pattern to minimize database hits
*   Consider Redis as a high-performance distributed cache implementation
*   Use `IDistributedCache` for sharing cached data across multiple applications

That's all for today.

See you next week.

---

Whenever you're ready, there are 4 ways I can help you:

1.  [Pragmatic Clean Architecture:](/pragmatic-clean-architecture) Join 4,200+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [Modular Monolith Architecture:](/modular-monolith-architecture) Join 2,100+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [(NEW) Pragmatic REST APIs:](/pragmatic-rest-apis) Join 1,200+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
4.  [Patreon Community:](https://www.patreon.com/milanjovanovic) Join a community of 5,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Become a Better .NET Software Engineer

Join 70,000+ engineers who are improving their skills every Saturday morning.

Join 70K+ Engineers

.formkit-form\[data-uid="134c4e25db"\] \*{box-sizing:border-box;}.formkit-form\[data-uid="134c4e25db"\]{-webkit-font-smoothing:antialiased;-moz-osx-font-smoothing:grayscale;}.formkit-form\[data-uid="134c4e25db"\] legend{border:none;font-size:inherit;margin-bottom:10px;padding:0;position:relative;display:table;}.formkit-form\[data-uid="134c4e25db"\] fieldset{border:0;padding:0.01em 0 0 0;margin:0;min-width:0;}.formkit-form\[data-uid="134c4e25db"\] body:not(:-moz-handler-blocked) fieldset{display:table-cell;}.formkit-form\[data-uid="134c4e25db"\] h1,.formkit-form\[data-uid="134c4e25db"\] h2,.formkit-form\[data-uid="134c4e25db"\] h3,.formkit-form\[data-uid="134c4e25db"\] h4,.formkit-form\[data-uid="134c4e25db"\] h5,.formkit-form\[data-uid="134c4e25db"\] h6{color:inherit;font-size:inherit;font-weight:inherit;}.formkit-form\[data-uid="134c4e25db"\] h2{font-size:1.5em;margin:1em 0;}.formkit-form\[data-uid="134c4e25db"\] h3{font-size:1.17em;margin:1em 0;}.formkit-form\[data-uid="134c4e25db"\] p{color:inherit;font-size:inherit;font-weight:inherit;}.formkit-form\[data-uid="134c4e25db"\] ol:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] ul:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] blockquote:not(\[template-default\]){text-align:left;}.formkit-form\[data-uid="134c4e25db"\] p:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] hr:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] blockquote:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] ol:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] ul:not(\[template-default\]){color:inherit;font-style:initial;}.formkit-form\[data-uid="134c4e25db"\] .ordered-list,.formkit-form\[data-uid="134c4e25db"\] .unordered-list{list-style-position:outside !important;padding-left:1em;}.formkit-form\[data-uid="134c4e25db"\] .list-item{padding-left:0;}.formkit-form\[data-uid="134c4e25db"\]\[data-format="modal"\]{display:none;}.formkit-form\[data-uid="134c4e25db"\]\[data-format="slide in"\]{display:none;}.formkit-form\[data-uid="134c4e25db"\]\[data-format="sticky bar"\]{display:none;}.formkit-sticky-bar .formkit-form\[data-uid="134c4e25db"\]\[data-format="sticky bar"\]{display:block;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input,.formkit-form\[data-uid="134c4e25db"\] .formkit-select,.formkit-form\[data-uid="134c4e25db"\] .formkit-checkboxes{width:100%;}.formkit-form\[data-uid="134c4e25db"\] .formkit-button,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit{border:0;border-radius:5px;color:#ffffff;cursor:pointer;display:inline-block;text-align:center;font-size:15px;font-weight:500;cursor:pointer;margin-bottom:15px;overflow:hidden;padding:0;position:relative;vertical-align:middle;}.formkit-form\[data-uid="134c4e25db"\] .formkit-button:hover,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:hover,.formkit-form\[data-uid="134c4e25db"\] .formkit-button:focus,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:focus{outline:none;}.formkit-form\[data-uid="134c4e25db"\] .formkit-button:hover > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:hover > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-button:focus > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:focus > span{background-color:rgba(0,0,0,0.1);}.formkit-form\[data-uid="134c4e25db"\] .formkit-button > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit > span{display:block;-webkit-transition:all 300ms ease-in-out;transition:all 300ms ease-in-out;padding:12px 24px;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input{background:#ffffff;font-size:15px;padding:12px;border:1px solid #e3e3e3;-webkit-flex:1 0 auto;-ms-flex:1 0 auto;flex:1 0 auto;line-height:1.4;margin:0;-webkit-transition:border-color ease-out 300ms;transition:border-color ease-out 300ms;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input:focus{outline:none;border-color:#1677be;-webkit-transition:border-color ease 300ms;transition:border-color ease 300ms;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input::-webkit-input-placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input::-moz-placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input:-ms-input-placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input::placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="dropdown"\]{position:relative;display:inline-block;width:100%;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="dropdown"\]::before{content:"";top:calc(50% - 2.5px);right:10px;position:absolute;pointer-events:none;border-color:#4f4f4f transparent transparent transparent;border-style:solid;border-width:6px 6px 0 6px;height:0;width:0;z-index:999;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="dropdown"\] select{height:auto;width:100%;cursor:pointer;color:#333333;line-height:1.4;margin-bottom:0;padding:0 6px;-webkit-appearance:none;-moz-appearance:none;appearance:none;font-size:15px;padding:12px;padding-right:25px;border:1px solid #e3e3e3;background:#ffffff;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="dropdown"\] select:focus{outline:none;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\]{text-align:left;margin:0;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\]{margin-bottom:10px;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] \*{cursor:pointer;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\]:last-of-type{margin-bottom:0;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] input\[type="checkbox"\]{display:none;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] input\[type="checkbox"\] + label::after{content:none;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] input\[type="checkbox"\]:checked + label::after{border-color:#ffffff;content:"";}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] input\[type="checkbox"\]:checked + label::before{background:#10bf7a;border-color:#10bf7a;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] label{position:relative;display:inline-block;padding-left:28px;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] label::before,.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] label::after{position:absolute;content:"";display:inline-block;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] label::before{height:16px;width:16px;border:1px solid #e3e3e3;background:#ffffff;left:0px;top:3px;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] label::after{height:4px;width:8px;border-left:2px solid #4d4d4d;border-bottom:2px solid #4d4d4d;-webkit-transform:rotate(-45deg);-ms-transform:rotate(-45deg);transform:rotate(-45deg);left:4px;top:8px;}.formkit-form\[data-uid="134c4e25db"\] .formkit-alert{background:#f9fafb;border:1px solid #e3e3e3;border-radius:5px;-webkit-flex:1 0 auto;-ms-flex:1 0 auto;flex:1 0 auto;list-style:none;margin:25px auto;padding:12px;text-align:center;width:100%;}.formkit-form\[data-uid="134c4e25db"\] .formkit-alert:empty{display:none;}.formkit-form\[data-uid="134c4e25db"\] .formkit-alert-success{background:#d3fbeb;border-color:#10bf7a;color:#0c905c;}.formkit-form\[data-uid="134c4e25db"\] .formkit-alert-error{background:#fde8e2;border-color:#f2643b;color:#ea4110;}.formkit-form\[data-uid="134c4e25db"\] .formkit-spinner{display:-webkit-box;display:-webkit-flex;display:-ms-flexbox;display:flex;height:0px;width:0px;margin:0 auto;position:absolute;top:0;left:0;right:0;width:0px;overflow:hidden;text-align:center;-webkit-transition:all 300ms ease-in-out;transition:all 300ms ease-in-out;}.formkit-form\[data-uid="134c4e25db"\] .formkit-spinner > div{margin:auto;width:12px;height:12px;background-color:#fff;opacity:0.3;border-radius:100%;display:inline-