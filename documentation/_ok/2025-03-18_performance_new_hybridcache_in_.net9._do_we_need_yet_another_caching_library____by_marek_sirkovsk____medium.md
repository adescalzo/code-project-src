```yaml
---
title: "New HybridCache in .NET9. Do we need yet another caching library… | by Marek Sirkovský | Medium"
source: https://mareks-082.medium.com/new-hybridcache-in-net9-29b1fa5a674f
date_published: 2025-03-18T06:10:14.529Z
date_captured: 2025-08-19T11:26:02.123Z
domain: mareks-082.medium.com
author: Marek Sirkovský
category: performance
technologies: [.NET 9, HybridCache, ASP.NET Core, System.Web.Caching, System.Runtime.Caching, IMemoryCache, IDistributedCache, FusionCache, CacheTower, RedisCache, StackExchange.Redis, OpenAPI, Swagger]
programming_languages: [C#, SQL]
tags: [caching, dotnet, distributed-cache, in-memory-cache, performance, dependency-injection, .net9, web-development, software-architecture, preview-features]
key_concepts: [two-level-caching, dependency-injection, read-through-pattern, cache-aside-pattern, cache-stampede-prevention, performance-optimization, abstraction-leaking, distributed-systems]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces HybridCache, a new two-level caching solution in .NET 9, designed to provide a more advanced built-in caching mechanism. It details HybridCache's integration with existing `IMemoryCache` and `IDistributedCache` interfaces, emphasizing its dependency injection-driven design and current API limitations. The author explores key features such as cache stampede prevention and performance enhancements through optimized buffer usage. While acknowledging its potential, the article concludes that HybridCache, in its preview state, is not yet a comprehensive replacement for mature third-party caching libraries like FusionCache, raising questions about its future impact on the .NET ecosystem.
---
```

# New HybridCache in .NET9. Do we need yet another caching library… | by Marek Sirkovský | Medium

# New HybridCache in .NET9

![Marek Sirkovský](https://miro.medium.com/v2/resize:fill:64:64/1*sd6GN4VkST6HdJW7e5xMKQ.jpeg)

Do we really need another caching library? [HybridCache](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid?view=aspnetcore-9.0) is Microsoft’s latest attempt to build a more advanced, built-in caching solution for .NET. It introduces a two-level caching mechanism and, while still in preview, is stable enough to evaluate its strengths and weaknesses.

![A plain red notebook or small book lies flat on a dark green, textured surface, viewed from directly above.](https://miro.medium.com/v2/resize:fit:700/1*fcYffZPC5GBj3d4eAK475w.jpeg)

Photo by [Kelly Sikkema](https://unsplash.com/@kellysikkema?utm_content=creditCopyText&utm_medium=referral&utm_source=unsplash) on [Unsplash](https://unsplash.com/photos/closed-red-book-w7C39ujHP5U?utm_content=creditCopyText&utm_medium=referral&utm_source=unsplash)

# Caching Reinvented (Again)

As usual, I’m including a paragraph or two about the history of caching in .NET. This gives you some context, but feel free to skip it if you’re not a history fan like I am.

Caching in .NET has evolved significantly over the years. In .NET 1.1, ASP.NET Cache (`System.Web.Caching`) was introduced, providing in-memory caching for web applications with expiration policies and dependency-based invalidation. However, it was tightly coupled to ASP.NET and not so suitable for non-web applications. To address this, .NET 4.0 introduced MemoryCache (`System.Runtime.Caching`), a more flexible, standalone caching library that could be used everywhere.

Like many other things, caching was redesigned with the advent of .NET Core 1.0+. `IMemoryCache` was introduced for local in-memory caching, and `IDistributedCache` was introduced for external distributed caching.

Most recently, .NET 9 (2025) introduced HybridCache, which we discuss in this blog post.

# HybridCache and others

I thought caching in .NET was already settled, especially given robust libraries like FusionCache. Yet, just as with the [OpenAPI vs. Swagger situation](https://codewithmukesh.com/blog/dotnet-swagger-alternatives-openapi/), Microsoft is adding more built-in functionality to the .NET ecosystem.

## HybridCache vs. IMemoryCache vs. IDistributedCache

For a long time, we have used `IMemoryCache` and `IDistributedCache` interfaces in ASP.NET Core for in-memory and distributed caching, respectively. The HybridCache library is a new abstraction that sits on top of these interfaces and provides a more sophisticated caching mechanism.

HybridCache uses `IMemoryCache` for its internal memory caching, which functions as a level-one cache and is a necessary dependency of HybridCache, as you will see later.

`IDistributedCache` manages distributed caching. Unlike `IMemoryCache`, it’s not a required dependency. If there is no registered implementation of `IDistributedCache` in the DI container, HybridCache will not utilize this cache and will behave _almost_ like `IMemoryCache`.

Let’s see the simplest way to set HybridCache:

```csharp
// Initialize in program.cs  
services.AddHybridCache();  
  
// Get the hybrid cache instance  
var cache = serviceProvider.GetRequiredService<HybridCache>();  
  
await cache.GetOrCreateAsync(  
  $"your-key",   
  async _ => await DbService.GetAsync(region, id));  
)
```

As you can see, HybridCache provides a `GetOrCreateAsync` method that takes a key and a factory function. The factory function is called only once for each key, and the result is stored in the cache. If the key is already in the cache, the factory function is not called, and the cached value is returned.

HybridCache does not offer a synchronous version of the `GetOrCreate` method. I like this design choice. Most APIs that require significant time and resources are asynchronous, and we also have `ValueTask`, which serves as an optimization for possible synchronous values.

# Dependency injection-driven development

Although the title “Dependency Injection-Driven Development” might seem cheeky, I chose it because of the significant role dependency injection (DI) plays in HybridCache. For example, have you noticed that we didn’t need to define any other services, like in-memory cache?

```csharp
// No need to for services.AddMemoryCache();  
services.AddHybridCache();
```

If you open the implementation of `AddHybridCache`, you can see that `IMemoryCache` is added automatically.

```csharp
AddHybridCache method:  
// Redacted  
...   
services.TryAddSingleton<TimeProvider>(TimeProvider.System);  
services.AddMemoryCache();  
services.TryAddSingleton<IHybridCacheSerializerFactory, DefaultJsonSerializerFactory>();  
services.TryAddSingleton<IHybridCacheSerializer<string>>((IHybridCacheSerializer<string>) InbuiltTypeSerializer.Instance);  
services.TryAddSingleton<IHybridCacheSerializer<byte[]>>((IHybridCacheSerializer<byte[]>) InbuiltTypeSerializer.Instance);  
services.TryAddSingleton<HybridCache, DefaultHybridCache>();  
...
```

Another example is the `DefaultHybridCache` class, which is a standard implementation of the HybridCache interface and is **internal**. This suggests that using the DI container is required. Even if you want to experiment with it in a console application, you still need to include the entire .NET DI container in your app to get an instance of HybridCache.

Moreover, HybridCache always utilizes the `IDistributedCache` provided through dependency injection. If you prefer HybridCache not to use the distributed cache, you can disable it via configuration flags. However, this approach completely disables the distributed cache in HybridCache.

```csharp
// Disable it for one item:  
cache.GetOrCreateAsync<TItem?>(key, factory: null!,  
  new HybridCacheEntryOptions()  
  {  
    Flags = HybridCacheEntryFlags.DisableUnderlyingData  
  },  
  cancellationToken: cancellationToken);  
  
// Disable it globally  
services.AddHybridCache(options =>  
{  
    options.MaximumPayloadBytes = 1024 * 1024;  
    options.MaximumKeyLength = 1024;  
    options.DefaultEntryOptions = new HybridCacheEntryOptions  
    {  
        Expiration = TimeSpan.FromMinutes(5),  
        LocalCacheExpiration = TimeSpan.FromMinutes(5),  
        Flags = HybridCacheEntryFlags.DisableDistributedCache  
    };  
});
```

All of this means you have bad luck if you already use `IDistributedCache` in your application and don’t want HybridCache to use it. You can use keyed services to overcome this limitation. But just be aware that non-keyed `IDistributedCache` will always be reserved for HybridCache.

```csharp
// This will be used by HybridCache  
services.AddSingleton<IDistributedCache, RedisCache>(sp =>  
{  
  var options = new RedisCacheOptions  
  {  
    Configuration = "localhost:6379",  
    InstanceName = "SampleInstance1"  
  };  
  return new RedisCache(Options.Create(options));  
});  
  
// This instance can be obtained via the GetKeyedService method.  
services.AddKeyedSingleton<IDistributedCache, RedisCache>(  
    "keyedRedis",   
    (sp, key) =>  
    {  
      var options = new RedisCacheOptions  
      {  
        Configuration = "localhost:6379",  
        InstanceName = "SampleInstance2"  
      };  
    return new RedisCache(Options.Create(options));  
});
```

# Limited API

We saw that the easiest way to utilize HybridCache is as follows:

```csharp
await cache.GetOrCreateAsync(  
  $"your-key",   
  async _ => await DbService.GetAsync(region, id));  
)
```

This method represents the [Read-Through pattern](https://www.enjoyalgorithms.com/blog/read-through-caching-strategy) in caching. It’s a pattern where the cache is the source of truth, and the cache is responsible for fetching the data if it’s missing in the cache.

Implementing a different type of pattern, such as [Cache-Aside](https://www.enjoyalgorithms.com/blog/cache-aside-caching-strategy) (Lazy Loading), requires manually checking the cache, fetching the data, and setting it in the cache. This requires a low-level API to get the item from the cache without calling any data fetching in the background if the value is missing. Unfortunately, the HybridCache API isn’t very useful in this case. It lacks this direct method, which is quite odd.

Fortunately, there is a workaround that can be implemented as an extension method:

```csharp
public static async Task<T?> GetAsync<T>(this HybridCache cache, string key, CancellationToken cancellationToken = default)  
{  
  var options = new HybridCacheEntryOptions  
  {  
    Flags = HybridCacheEntryFlags.DisableUnderlyingData |  
      HybridCacheEntryFlags.DisableLocalCacheWrite |  
      HybridCacheEntryFlags.DisableDistributedCacheWrite  
  };  
  
  var result = await cache.GetOrCreateAsync<T>(  
    key,  
    factory: null!,  
    options,  
    cancellationToken: cancellationToken  
  );  
    
  return result;  
}
```

A bit crazy with `factory: null!` and all the flags, but it works.

Another issue is the reliance on the abstract class (`HybridCache`) instead of an interface. In .NET 9, there's no `IHybridCache` interface, only the abstract `HybridCache` class. This design makes mocking more challenging compared to `IMemoryCache` or `IDistributedCache` interfaces.

The lack of low-level methods and the worse testability caused by using an abstract class make me think you’d probably need to add a custom abstraction layer/service on top of HybridCache_._ I mean, you will likely need to create your own `IHybridCache` interface and its implementation, incorporating the required low-level methods and improving testability.

_Note: I was surprised by this decision. The reason for this decision was that HybridCache requires internal state management (e.g., for stampede protection). If you want to write your own implementation of HybridCache, it’s easier to use an abstract class with a lot of functionality already in place than to start from scratch. It’s a valid point, but I’m still not sold. I think the IHybridCache interface should be there, too._

# Extensible

HybridCache in NET9 introduces both an implementation and an abstraction layer. As I mentioned, you can create your own implementation by inheriting from the abstract class `HybridCache`. Additionally, HybridCache supports custom serializers. However, the question remains: is this enough?

From my point of view, the HybridCache API surface is quite small, so even if you use FusionCache or CacheTower as a second layer, you get only a small subset of their features. What surprised me a bit was when I found out that FusionCache had already introduced [its own implementation of HybridCache](https://github.com/ZiggyCreatures/FusionCache/blob/main/docs/MicrosoftHybridCache.md). However, the limited public API of HybridCache prevents you from using FusionCache’s advanced features.

It’s like using a Swiss Army knife but only unfolding the bottle opener — you’re carrying the full toolset, but you’re only accessing a small part of its capabilities. But if you need only a bottle opener, why not?

# Abstraction leaking

Since HybridCache builds its functionality on top of existing types, such as `IMemoryCache` and various implementations of `IDistributedCache`, there is a risk of abstraction leaking.

For example, see the following snippet:

```csharp
// Redis is ready, redacted  
services.AddStackExchangeRedisCache(options =>  
  ...  
  
services.AddHybridCache(options =>  
{  
  options.DefaultEntryOptions = new HybridCacheEntryOptions  
  {  
     // I'm disabling local cache!  
     Flags = HybridCacheEntryFlags.DisableLocalCache  
   };  
});  
  
var cache = serviceProvider.GetRequiredService<HybridCache>();  
  
var valueA = await cache.GetOrCreateAsync(  
  "key",  
  async _ => await Task.FromResult(Guid.NewGuid().ToString()),  
  cancellationToken: CancellationToken.None  
);  
  
var valueB = await cache.GetOrCreateAsync(  
  "key",  
  async _ => await Task.FromResult(Guid.NewGuid().ToString()),  
  cancellationToken: CancellationToken.None  
);  
```

Can you guess what values `valueA` and `valueB` will take when you run the code?

The application returns two different values (`ValueA` ≠ `ValueB`). Since you don’t have a local cache enabled, the distributed cache is ignored. Honestly, I would prefer that an error be thrown in this situation.

You might wonder why we would want this behavior at all — ignoring the local cache and utilizing solely the distributed one. The answer is that it can be quite handy for certain edge cases. In some scenarios, you would need to work on a distributed system that includes read-only and write-only caching nodes. Write-only caching nodes do not require an in-memory cache; they need only a mechanism to upload data into a distributed cache.

It's also important to consider the complexities of distributed caches to understand how HybridCache interacts with `IMemory` and `IDistributedCache`. For instance, the sample code in the documentation is a bit misleading.

Documentation:

```csharp
services.AddStackExchangeRedisCache(options =>  
{  
    options.Configuration = "localhost:6379";  
});
```

While we are using a Redis cache, the absence of the`InstanceName` option means that the cached values will not be shared among different instances of your application - which essentially undermines the purpose of using distributed caches.

The correct way:

```csharp
services.AddStackExchangeRedisCache(options =>  
{  
    options.Configuration = "localhost:6379";  
    // Add this:  
    options.InstanceName = "{Your name of instance}";   
});  
```

# Cache stampedes

One of the main selling points of HybridCache is preventing cache stampedes.

A cache stampede occurs when multiple users or threads attempt to retrieve the same missing data from a cache simultaneously. As a result, they all bypass the cache and make concurrent requests to the database or backend system, which can lead to excessive load and potentially cause system failure.

For now, stampede prevention seems to be limited to the in-memory cache. However, even local stampede prevention adds a lot of value to HybridCache.

_Note: You may have noticed that the .NET community has been discussing adding a new_ [_in-memory cache_](https://github.com/dotnet/extensions/issues/4766) _for some time. However, this new cache_ **_won’t have_** _cache stampede prevention, so HybridCache becomes even more appealing._

# Performance improvement

Speaking of stampede prevention, it’s important to note that this feature is not free. It often involves a locking mechanism, which can potentially degrade performance.

To minimize lock contention, the creators of HybridCache employ double-checked locking combined with request deduplication, where identical incoming requests are queued alongside an already executing operation.

Additionally, as a cherry on top, they implemented a partitioned locking mechanism. Specifically, the lock is split into eight partitions, with each lock acquired based on the lowest three bits of the hash code. This reduces lock contention by almost an order of magnitude(8x).

Another enhancement is the introduction of the new `IBufferDistributedCache` interface. This optional interface is designed to minimize memory allocations typically caused by the use of `byte[]`. It leverages buffer-oriented methods, such as `IBufferWriter<byte>` and `ReadOnlySequence<byte>.`HybridCache dynamically checks if the underlying background service implements`IBufferDistributedCache`; if available, it utilizes these optimized methods—otherwise, it falls back to the standard _slow_ `IDistributedCache` interface. You, as a consumer, just need to check if your backend cache implements this new `IBufferDistributedCache`.

# Is it mature enough?

Even though HybridCache is still in preview, its public API seems stable and finished, at least for .NET 9. So, it’s safe to say HybridCache is quite limited compared to FusionCache or CacheTower.

HybridCache lacks certain advanced features, such as a fail-safe mechanism. Without a fail-safe, if the distributed cache becomes unavailable, HybridCache cannot retrieve data and will throw an exception. Additionally, features like auto-recovery during distributed cache outages, soft timeouts when interacting with distributed caches, and background cache updates would be nice to have.

However, to be fair, I’m just probably spoiled by FusionCache. I didn’t expect that in HybridCache 1.0/2.0.

# Verdict

HybridCache is a good start but not a drop-in replacement for FusionCache or CacheTower. I also have mixed feelings about this Microsoft attempt. It’s great that the .NET ecosystem has more caching libraries, but it must also be a bit demotivating for the authors of third-party caching libraries. In the worst-case scenario, there’s a risk that HybridCache may not reach parity with existing caching solutions(that’s a very real scenario, to be honest) and potentially slow down the innovation of 3rd party caching libraries by discouraging its development. I hope this doesn’t happen.