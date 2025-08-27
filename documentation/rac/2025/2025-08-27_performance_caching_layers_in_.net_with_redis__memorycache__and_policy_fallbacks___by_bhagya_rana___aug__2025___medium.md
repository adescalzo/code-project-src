```yaml
---
title: "Caching Layers in .NET with Redis, MemoryCache, and Policy Fallbacks | by Bhagya Rana | Aug, 2025 | Medium"
source: https://medium.com/@bhagyarana80/caching-layers-in-net-with-redis-memorycache-and-policy-fallbacks-8e73c2fde398
date_published: 2025-08-27T14:31:43.538Z
date_captured: 2025-09-05T11:06:06.372Z
domain: medium.com
author: Bhagya Rana
category: performance
technologies: [.NET, Redis, MemoryCache, StackExchange.Redis, Polly, System.Text.Json]
programming_languages: [C#]
tags: [caching, dotnet, redis, performance, resilience, memory-cache, distributed-cache, layered-caching, polly, data-access]
key_concepts: [layered-caching, in-memory-caching, distributed-caching, fallback-policies, resilience-patterns, retry-policy, graceful-degradation, dependency-injection]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide on implementing layered caching strategies in .NET applications to significantly boost performance and resilience. It outlines a multi-tier approach, starting with `MemoryCache` for ultra-fast in-process data access (L1), followed by Redis for distributed and shared caching across instances (L2), with the database serving as the ultimate source of truth. The author demonstrates practical C# code examples for integrating both caching mechanisms and introduces Polly for building robust fallback and retry policies. This strategy ensures graceful degradation and high availability, even when cache layers or the database encounter issues.
---
```

# Caching Layers in .NET with Redis, MemoryCache, and Policy Fallbacks | by Bhagya Rana | Aug, 2025 | Medium

Member-only story

# Caching Layers in .NET with Redis, MemoryCache, and Policy Fallbacks

## How layered caching strategies in .NET can supercharge performance while keeping your system resilient.

![Bhagya Rana](https://miro.medium.com/v2/resize:fill:64:64/1*HIW9u8a6lsjqLk27kPGbKQ.png)

[Bhagya Rana](/@bhagyarana80?source=post_page---byline--8e73c2fde398---------------------------------------)

Follow

4 min read

·

Aug 27, 2025

Listen

Share

More

Press enter or click to view image in full size

![Diagram illustrating the layered caching architecture in .NET, showing a request flow from a .NET application to MemoryCache (In-Process), then to Redis (Distributed Cache), and finally to the Database (Source of Truth). The title reads "Caching Layers in .NET with Redis & MemoryCache".](https://miro.medium.com/v2/resize:fit:700/1*hqBOBnfY4TJhhy_6QX2sHg.png)

_Learn how to speed up .NET apps with layered caching using MemoryCache, Redis, and policy-based fallbacks for resilience and performance._

If you’ve ever worked on a high-traffic .NET application, you know the feeling: requests start piling up, databases gasp for air, and users hit refresh like it’s a sport.

Caching isn’t just a “nice-to-have.” It’s the difference between an app that feels snappy and one that feels like dial-up. But let’s be real — just throwing Redis into the mix isn’t enough. You need **layers of caching**, each with a fallback strategy, to keep latency low and resilience high.

That’s what we’ll unpack: **how to design layered caching in .NET with MemoryCache, Redis, and smart fallbacks**.

## Why Layered Caching Matters

Think of caching like a food chain:

*   **L1 Cache (In-Memory):** Lightning fast, lives inside the application process.
*   **L2 Cache (Distributed, e.g., Redis):** Shared across instances, survives app restarts.
*   **Fallback Policies:** What happens if caches miss or fail?

Without layers, you either overburden your distributed cache or fall back too often to your database. With layers, you combine **speed, scalability, and reliability**.

## Architecture Flow

Here’s the high-level caching flow:

```
Client Request  
     │  
     ▼  
 ┌─────────────┐  
 │ MemoryCache │  ← Ultra-fast (in-process)  
 └──────┬──────┘  
        │ (miss)  
        ▼  
 ┌─────────────┐  
 │   Redis     │  ← Shared, distributed  
 └──────┬──────┘  
        │ (miss)  
        ▼  
 ┌─────────────┐  
 │   Database  │  ← Source of truth  
 └─────────────┘
```

*   **L1 (MemoryCache):** For hot data needed instantly.
*   **L2 (Redis):** For cross-instance consistency.
*   **Fallback:** Always a safe path to the DB.

## Implementing MemoryCache (L1)

`MemoryCache` is built into .NET and is blazing fast.

```csharp
using Microsoft.Extensions.Caching.Memory;  
  
public class ProductService  
{  
    private readonly IMemoryCache _memoryCache;  
  
    public ProductService(IMemoryCache memoryCache)  
    {  
        _memoryCache = memoryCache;  
    }  
  
    public async Task<Product> GetProductAsync(int id)  
    {  
        string cacheKey = $"product:{id}";  
        if (_memoryCache.TryGetValue(cacheKey, out Product product))  
            return product;  
  
        // simulate DB call  
        product = await LoadFromDatabase(id);  
  
        _memoryCache.Set(cacheKey, product, TimeSpan.FromMinutes(5));  
        return product;  
    }  
}
```

This works great for a single instance. But as soon as you scale horizontally, you’ll need Redis.

## Redis as L2 Cache

Redis keeps your data **distributed and durable** (well, as durable as memory can be).

Using `StackExchange.Redis`:

```csharp
using StackExchange.Redis;  
using System.Text.Json;  
  
public class RedisCacheService  
{  
    private readonly IDatabase _redisDb;  
  
    public RedisCacheService(IConnectionMultiplexer redis)  
    {  
        _redisDb = redis.GetDatabase();  
    }  
  
    public async Task SetAsync<T>(string key, T value, TimeSpan expiry)  
    {  
        string json = JsonSerializer.Serialize(value);  
        await _redisDb.StringSetAsync(key, json, expiry);  
    }  
  
    public async Task<T?> GetAsync<T>(string key)  
    {  
        var json = await _redisDb.StringGetAsync(key);  
        if (json.IsNullOrEmpty) return default;  
        return JsonSerializer.Deserialize<T>(json!);  
    }  
}
```

Redis ensures that if your app restarts — or if you have multiple servers — the cache still holds.

## Layering MemoryCache + Redis

Now let’s put them together.

```csharp
public class LayeredCacheService  
{  
    private readonly IMemoryCache _memoryCache;  
    private readonly RedisCacheService _redis;  
  
    public LayeredCacheService(IMemoryCache memoryCache, RedisCacheService redis)  
    {  
        _memoryCache = memoryCache;  
        _redis = redis;  
    }  
  
    public async Task<Product> GetProductAsync(int id)  
    {  
        string cacheKey = $"product:{id}";  
  
        // Step 1: Check in-memory cache  
        if (_memoryCache.TryGetValue(cacheKey, out Product product))  
            return product;  
  
        // Step 2: Check Redis  
        product = await _redis.GetAsync<Product>(cacheKey);  
        if (product != null)  
        {  
            _memoryCache.Set(cacheKey, product, TimeSpan.FromMinutes(1));  
            return product;  
        }  
  
        // Step 3: Fallback to DB  
        product = await LoadFromDatabase(id);  
  
        // Save to both caches  
        _memoryCache.Set(cacheKey, product, TimeSpan.FromMinutes(1));  
        await _redis.SetAsync(cacheKey, product, TimeSpan.FromMinutes(10));  
  
        return product;  
    }  
}
```

Now your requests check the fastest cache first, fall back gracefully, and always keep hot data ready.

## Policy Fallbacks with Polly

What if Redis goes down? Or your DB is slow? You don’t want the whole app to collapse. That’s where **Polly** (a .NET resilience library) comes in.

Example: wrap Redis calls with a retry + fallback.

```csharp
using Polly;  
using Polly.Caching.Memory; // Note: This specific line might not be directly used in the example, but Polly is mentioned.
using StackExchange.Redis; // Required for RedisConnectionException

// Assume Product class exists
public class Product { public int Id { get; set; } public string Name { get; set; } }

// Assume _redis is an instance of RedisCacheService
// Assume LoadFromDatabase is available

var retryPolicy = Policy  
    .Handle<RedisConnectionException>()  
    .RetryAsync(3);  
  
var fallbackPolicy = Policy<Product>  
    .Handle<Exception>()  
    .FallbackAsync(new Product { Id = -1, Name = "Fallback Product" });  
  
var policyWrap = Policy.WrapAsync(retryPolicy, fallbackPolicy);  
  
// usage  
// In a real scenario, you'd pass a Func<Task<Product>> that calls _redis.GetAsync<Product>("key")
// For demonstration, let's assume _redis.GetAsync<Product>("key") is wrapped
var product = await policyWrap.ExecuteAsync(async () => 
{
    // Simulate a Redis call that might fail
    // For a complete example, you'd integrate this into the LayeredCacheService
    // For now, let's just show the policy usage
    Console.WriteLine("Attempting to get product from Redis via policy...");
    // This line would typically call _redis.GetAsync<Product>("key")
    // For a standalone example, we'll simulate a potential failure
    if (new Random().Next(0, 10) < 3) // 30% chance of simulating a Redis failure
    {
        throw new RedisConnectionException("Simulated Redis connection failure.");
    }
    return await Task.FromResult(new Product { Id = 100, Name = "Product from Redis" });
});

Console.WriteLine($"Product retrieved: Id={product.Id}, Name={product.Name}");
```

This ensures your system **degrades gracefully** instead of failing outright.

## Real-World Example: E-Commerce Checkout

Picture this: an e-commerce site under load.

*   **Hot data (cart items)** → MemoryCache
*   **Inventory stock levels** → Redis
*   **Final confirmation** → Database

If MemoryCache misses, Redis covers. If Redis fails, you retry with Polly and fall back to the DB. Users might see a slightly slower response, but the app stays alive.

It’s like having airbags for your caching system.

## Analogy: Coffee Shop Efficiency

Think of your DB as the kitchen. Redis is like a barista keeping commonly ordered drinks ready. MemoryCache? That’s the cup you already poured and left on your desk.

*   Reach for your own cup first (fastest).
*   If not there, ask the barista (still quick).
*   If all else fails, order from the kitchen (slow, but guaranteed).

## Closing Thoughts

Caching in .NET isn’t about picking one tool. It’s about **layering MemoryCache for raw speed, Redis for distributed consistency, and policies for resilience**.

Get this right, and your users will feel instant speed — while your infrastructure breathes easier.

And let’s be honest: your database will thank you, too.

_What caching strategies have you used in .NET apps? Share your experience in the comments — and follow for more deep dives into real-world performance tuning._