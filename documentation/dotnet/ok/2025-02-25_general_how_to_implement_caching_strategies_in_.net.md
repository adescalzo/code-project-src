```yaml
---
title: How To Implement Caching Strategies in .NET
source: https://antondevtips.com/blog/how-to-implement-caching-strategies-in-dotnet
date_published: 2025-02-25T08:45:59.289Z
date_captured: 2025-08-06T17:36:36.659Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [.NET, ASP.NET Core, HybridCache, Redis, IMemoryCache, IDistributedCache, StackExchangeRedis, Nuget, System.Threading.Channels, ADO.NET, EntityDeveloper]
programming_languages: [C#, SQL]
tags: [caching, dotnet, performance, scalability, redis, in-memory-cache, distributed-cache, data-access, web-development, architecture]
key_concepts: [caching-strategies, cache-aside, read-through-cache, write-around-cache, write-back-cache, write-through-cache, cache-stampede, eventual-consistency]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to implementing five common caching strategies in .NET applications: Cache Aside, Read Through, Write Around, Write Back, and Write Through. It explains the mechanics, ideal use cases, and the pros and cons of each strategy, accompanied by practical C# code examples utilizing the new .NET 9 HybridCache library. The post also covers essential setup for HybridCache in ASP.NET Core and addresses critical caching concerns like the cache stampede problem. The goal is to help developers select the most suitable caching approach based on their application's read/write patterns, performance targets, and consistency requirements.
---
```

# How To Implement Caching Strategies in .NET

![How To Implement Caching Strategies in .NET](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fdotnet%2Fcover_dotnet_caching_strategies.png&w=3840&q=100)

# How To Implement Caching Strategies in .NET

Feb 25, 2025

[Download source code](/source-code/how-to-implement-caching-strategies-in-dotnet)

11 min read

### Newsletter Sponsors

Other PDF SDKs break under scale — with laggy scrolling, buggy behavior, poor mobile UX, and tons of bugs that waste your time. [Nutrient's SDK](https://www.nutrient.io/sdk?utm_campaign=newsletter2-2025-02-25&utm_source=anton-dev-tips&utm_medium=sponsoring) handles billion-page workloads effortlessly, trusted by ~1 billion users in 150+ countries. [See the most trusted SDK](https://www.nutrient.io/sdk?utm_campaign=newsletter2-2025-02-25&utm_source=anton-dev-tips&utm_medium=sponsoring) and streamline your workflow.

Accelerate your applications with the best combination of a [visual ORM designer](https://www.devart.com/entitydeveloper/?utm_source=martyniuk&utm_medium=referral&utm_campaign=Q1) and high-performance [ADO.NET data providers](https://www.devart.com/dotconnect/?utm_source=martyniuk&utm_medium=referral&utm_campaign=Q1). Download a 30-day free trial today for reliable updates, expert support, and top-tier performance! Get 15% OFF with promo code **MARTYNIUK15**

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Caching is a powerful technique to improve the performance and scalability of applications. In .NET, we typically use in-memory or distributed caching like Redis.

Depending on read/write frequency, data freshness, and consistency requirements, you may consider using different **Caching Strategies**.

In this blog post, I want to show you 5 **Caching Strategies** and their implementations in .NET. We will discuss the use cases for each strategy, as well as cons and pros of each option.

In all code examples I will use new `HybridCache` available in .NET 9, but you can use the same code if prefer to use directly `IMemoryCache`, `IDistributedCache` or any Redis SDK.

## Setting Up HybridCache in ASP.NET Core Application

When working with cache, we must be aware of the cache stampede problem: when multiple requests may receive a cache miss and will all call the database to retrieve the entity. Instead of calling the database once to get the entity and write it into cache.

You can introduce manual locking to solve this problem, but this approach doesn't scale so well.

That's why I am using a new caching library `HybridCache` available in .NET 9 that prevents a cache stampede problem.

`HybridCache` is a great replacement of old `IMemoryCache` and `IDistributedCache` as it combines them both and can work with:

*   in-memory cache
*   distributed cache like Redis

Here is how `HybridCache` works:

*   First checks if the item is present in a memory cache (Level 1 Cache)
*   If an item is not found - then it fallbacks to search in the distributed cache like Redis (Level 2 Cache)
*   If an item is not found - then it fallbacks to the callback method and gets the value from a database (you need to provide a delegate)

To get started with `HybridCache` add the following Nuget packages:

```bash
dotnet add package Microsoft.Extensions.Caching.Hybrid
dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis
```

Here is how you can register it in DI:

```csharp
#pragma warning disable EXTEXP0018
services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(10)
    };
});
#pragma warning restore EXTEXP0018
```

You need to disable the warning with pragma as `HybridCache` is in preview at the moment (it might be already released when you're reading this).

By default, it enables the InMemory Cache.

Here is how you can register Redis in DI, and it will be automatically picked by `HybridCache`:

```csharp
services
    .AddStackExchangeRedisCache(options =>
    {
        options.Configuration = configuration.GetConnectionString("Redis")!;
    });
```

Now let's dive into Caching Strategies.

## 1. Cache Aside

**How It Works:**

*   The application checks the cache first when reading data.
*   On a cache miss, data is fetched from the database and added to the cache.
*   Future reads use the newly cached data.

**Best For:**

*   When data that is read often but can handle occasional misses where data is fetched from the database.

**Analogy:**

*   Checking your library (cache) for a book first, and if the book is not found, you go to the book's store (database) and then refill your library.

This is how it is implemented in the code:

```csharp
public class CacheAsideProductService(IMemoryCache cache, IProductRepository repository)
    : IProductService
{
    public async Task<Product?> GetByIdAsync(int id)
    {
        // Define the cache key
        var cacheKey = $"product:{id}";

        // 1. Try to get the product from the cache
        var product = cache.Get<Product>(cacheKey);
        if (product is not null)
        {
            return product; // Cache hit
        }

        // 2. If not found, load the product from the database
        product = await repository.GetByIdAsync(id);
        if (product != null)
        {
            // 3. Update the cache with the fetched product
            cache.Set(cacheKey, product, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
        }

        return product;
    }
}
```

In this example, we only query the database if the cache is empty. This approach keeps the cache up-to-date "on demand" and is one of the most widely used patterns.

**Pros:**

*   Straightforward to implement.
*   Reduces database load on subsequent reads.
*   Data is only cached when needed (on demand).

**Cons:**

*   Potential cache misses on first request after data changes (if the cache is not pre-populated).
*   Requires extra logic in your application to handle fetching and storing data in the cache.

**Real-World Use Cases:**

**1. E-Commerce Product Catalog:**

*   **Scenario:** In an online store, product details (name, price, description) rarely change compared to how often they are read.
*   **Why Cache Aside?** You can lazily load product details into the cache whenever they are requested, reducing the initial overhead of populating the cache. If the data changes, removing or invalidating the cache entry is relatively straightforward, and the next read causes a fresh load.
*   **Benefit:** Only the products actually being viewed get cached, saving memory while speeding up repeated queries for popular items.

**2. Blogs WebSite:**

*   **Scenario:** Articles and blog posts don't change frequently, but are read heavily.
*   **Why Cache Aside?** When a user first requests a new article, it's fetched from the source and then cached, making future retrievals almost instant.

## 2. Read Through Cache

**How It Works:**

*   The cache itself is responsible for fetching data from the data store when there is a cache miss.
*   Your application simply interacts with the cache layer, which "reads through" to the database on a miss.

**Best For:**

*   Frequently accessed data.
*   Reducing the complexity of “where data comes from” in your application.

**Analogy:**

*   Imagine a coffee machine in an office that automatically refills itself with coffee beans (database) when it runs out. You, as a user, only interact with the coffee machine (cache) to get your coffee, without ever needing to worry about where the coffee beans come from or how they are restocked.

This is how it is implemented in the code:

```csharp
public class ReadThroughCacheProductService(HybridCache cache, IProductRepository repository)
    : IProductService
{
    public async Task<Product?> GetByIdAsync(int id)
    {
        // Define the cache key
        var cacheKey = $"product:{id}";

        // Read-through cache implementation
        var product = await cache.GetOrCreateAsync<Product?>(
            cacheKey,
            // Data loader function to fetch from DB
            async token => await repository.GetByIdAsync(id),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10)
            });

        return product;
    }
}
```

Here, the HybridCache is in charge of checking if the product is in cache. If not, it automatically calls `IProductRepository.GetByIdAsync` and caches the result. This abstracts the caching logic into the cache layer itself, simplifying the application code.

**Pros:**

*   Centralizes caching logic in one place (the cache layer).
*   Simplifies application code by abstracting away database fetches.
*   Ensures consistent and seamless data loading.

**Cons:**

*   Cache library or service must support read-through capability.
*   Potentially more complex configuration or setup than Cache Aside.
*   Application has less direct control over database reads.

**Real-World Use Cases:**

**1. Product Recommendations:**

*   **Scenario:** Personalized product recommendations rely on machine learning models or advanced queries.
*   **Why Read Through?** Instead of coding the "fetch from ML service then put in cache" logic, you can let the cache manage those fetches. When a recommendation entry is not in cache, the caching layer automatically calls the model or a specialized service.
*   **Benefit:** Keeps your application code clean and centralizes the "loading on miss" logic in the caching layer.

**2. Global Configuration / Feature Flags:**

*   **Scenario:** An application might read configuration flags (for A/B testing or feature toggles) every time it needs to check if a feature is enabled.
*   **Why Read Through?** If a flag isn't in the cache, the system automatically retrieves it from the configuration store, ensuring the latest settings are always available with minimal application code overhead.

## 3. Write Around

**How It Works:**

*   Data is written directly to the database, bypassing the cache.
*   The cache gets updated only when the next read occurs (cache miss on subsequent reads).

How does it differ from the Cache Aside Pattern? Both strategies are similar on the **read side** (cache first, fallback to the database on a miss), but the difference is:

*   **Cache Aside:** Writes can also update or invalidate the cache immediately, ensuring consistency.
*   **Write Around:** Writes completely ignore the cache, leaving it stale until the next read refreshes it.

**Best For:**

*   Write-heavy systems where immediate cache updates on every write can be expensive.
*   Situations where data changes frequently but read requests are less frequent or can tolerate stale data until the next read.

**Analogy:**

*   When you buy a new book for the library (database), you don't immediately update the library's public catalog (cache). Instead, the catalog only gets updated the next time someone asks for that book.

This is how it is implemented in the code:

```csharp
public class WriteAroundCacheProductService(IMemoryCache cache, IProductRepository repository)
    : IProductService
{
    public async Task<Product?> GetByIdAsync(int id)
    {
        // Define the cache key
        var cacheKey = $"product:{id}";

        // 1. Try to get the product from the cache
        var product = cache.Get<Product>(cacheKey);
        if (product is not null)
        {
            return product; // Cache hit
        }

        // 2. If not found, load the product from the database
        product = await repository.GetByIdAsync(id);
        if (product != null)
        {
            // 3. Update the cache with the fetched product
            cache.Set(cacheKey, product, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
            });
        }

        return product;
    }

    public async Task AddAsync(Product product)
    {
        // Add the product to the database. Cache is not updated
        await repository.AddAsync(product);
    }
}
```

With Write Around, you skip the cache on writes, which can reduce cache invalidation overhead. The cost is potentially serving stale data until the next read forces a database fetch.

When reading a value, you can either use the **Cache Aside** or **Read Through Cache** strategy. **Write Around** strategy is often combined with one of these two strategies.

**Pros:**

*   Reduces overhead on frequent writes (no need to update cache on every write).
*   Simple to implement if already using a Cache Aside approach for reads.
*   Helps avoid frequent invalidation of cache on writes.

**Cons:**

*   Potentially stale data served from the cache until the next read triggers an update.
*   If data is updated often but also read often, the cache can become out of sync frequently.
*   Slightly more complex read logic if mixing with other patterns.

**Real-World Use Cases:**

**1. Logging & Analytics Pipelines:**

*   **Scenario:** Large volumes of logs are continuously written, but often analyzed in batches or only upon specific queries.
*   **Why Write Around?** Writing logs directly to a datastore (like a NoSQL DB or a log server) is faster and simpler, and you only cache data when it's read for analysis. This avoids the overhead of updating the cache with every single log entry.

**2. High-Frequency Data Ingestion:**

*   **Scenario:** IoT devices generating high volumes of sensor readings that get stored in a time-series database, but read sporadically (e.g., for historical analysis).
*   **Why Write Around?** Constantly updating the cache for every sensor reading is wasteful, especially if those readings are infrequently queried. Write around ensures the DB always has the latest data, and the cache populates only on actual read requests.

## 4. Write Back

**How It Works:**

*   Data is first written to the cache.
*   The cache asynchronously writes the data back to the database after a certain condition or interval.

**Best For:**

*   High-speed, write-intensive scenarios.
*   Applications that can handle eventual consistency (database might temporarily lag behind the cache).

**Analogy:**

*   Quickly saving a phone number on a piece of paper (cache) and then later adding it to your phone's contact list (database) when you have more time.

This is how it is implemented in the code:

```csharp
public class WriteBackCacheProductCartService(
    HybridCache cache,
    IProductCartRepository repository,
    IProductRepository productRepository,
    Channel<ProductCartDispatchEvent> channel)
{
    public async Task<ProductCartResponse?> GetByIdAsync(Guid id)
    {
        // Define the cache key
        var cacheKey = $"productCart:{id}";

        // Retrieve the ProductCart from the cache (or from the database if missing)
        var productCartResponse = await cache.GetOrCreateAsync<ProductCartResponse?>(
            cacheKey,
            // Data loader function to fetch from DB
            async token => 
            {
                var productCart = await repository.GetByIdAsync(id);
                return productCart?.MapToProductCartResponse();
            },
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10)
            });

        return productCartResponse;
    }

    public async Task<ProductCartResponse> AddAsync(ProductCartRequest request)
    {
        var productCart = new ProductCart
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CartItems = request.ProductCartItems.Select(x => x.MapToCartItem()).ToList()
        };
        
        var cacheKey = $"productCart:{productCart.Id}";

        // Add or update the ProductCart in the cache
        var productCartResponse = productCart.MapToProductCartResponse();
        await cache.SetAsync(cacheKey, productCartResponse);

        // Queue the ProductCart for database syncing
        await channel.Writer.WriteAsync(new ProductCartDispatchEvent(productCart, false));

        return productCartResponse;
    }
}
```

When creating a new `ProductCart` it is immediately added to the cache and after written to the database asynchronously in another thread.

Here I am using a bounded `Channel` that fires an asynchronous in-process event about created `ProductCart`:

```csharp
var channelOptions = new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait
};

services.AddSingleton(_ => Channel.CreateBounded<ProductCartDispatchEvent>(channelOptions));
```

I have a BackgroundService that reads event from the Channel and asynchronously updates the database:

```csharp
public record ProductCartDispatchEvent(ProductCart ProductCart, bool IsDeleted);

public class WriteBackCacheBackgroundService(IServiceScopeFactory scopeFactory,
    Channel<ProductCartDispatchEvent> channel) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var command in channel.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IProductCartRepository>();
            
            if (command.IsDeleted)
            {
                await repository.DeleteAsync(command.ProductCart.Id);
                return;
            }

            var existingCart = await repository.GetByIdAsync(command.ProductCart.Id);
            if (existingCart is null)
            {
                await repository.AddAsync(command.ProductCart);
            }
            else
            {
                existingCart.CartItems = command.ProductCart.CartItems;
                await repository.UpdateAsync(existingCart);
            }
        }
    }
}
```

This pattern can dramatically speed up writes but requires robust conflict resolution and failure handling for consistency.

**Pros:**

*   Very fast writes (the database isn't hit immediately).
*   Reduces write load on the database significantly.
*   Can improve application throughput in high-write scenarios.

**Cons:**

*   Risk of data loss if the cache node fails before flushing changes to the database.
*   More complex synchronization logic (you need a background process or event to handle DB updates).
*   Inconsistency between cache and database until the data is written back.

**Real-World Use Cases:**

**1. Shopping Carts in E-Commerce:**

*   **Scenario:** Users frequently add or remove items from their online shopping carts. The final state is only really critical at checkout.
*   **Why Write Back?** Writes can be very fast since they hit the cache first, and the system can periodically flush updates to the database. This allows for high write throughput during peak shopping times, with the database eventually getting the final cart details.
*   **Caution:** Ensuring cart state is never lost in case of a cache failure requires robust replication or backup mechanisms.

**2. Online Gaming Session State:**

*   **Scenario:** Multiplayer games often handle frequent player state changes (scores, items, etc.). Real-time performance is crucial.
*   **Why Write Back?** You can update the in-memory or distributed cache instantly for fast gameplay. A background job pushes changes to the persistent store (database) on a schedule or at session end.
*   **Caution:** If the cache goes down unexpectedly, recent changes might be lost unless well-protected by replication.

## 5. Write Through

**How It Works:**

*   Data is written to both the cache and the database in a single operation.
*   Ensures that the cache and database stay synchronized immediately.

**Best For:**

*   Consistency-critical systems where reads must see the latest updates right away.
*   Environments where the overhead of writing to two places is acceptable.

**Analogy:**

*   Making a journal entry in your paper planner (database) while simultaneously updating a shared digital calendar (cache) to keep both in sync.

This is how it is implemented in the code:

```csharp
public class WriteThroughCacheProductService(HybridCache cache, IProductRepository repository) : IProductService
{
    public async Task<Product?> GetByIdAsync(int id)
    {
        // Define the cache key
        var cacheKey = $"product:{id}";

        // Cache should always have the value but just in case we can check the database on a cache miss
        var product = await cache.GetOrCreateAsync<Product?>(
            cacheKey,
            // Data loader function to fetch from DB
            async token => await repository.GetByIdAsync(id),
            new HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromMinutes(10)
            });

        return product;
    }

    public async Task AddAsync(Product product)
    {
        // Add the product to the database
        await repository.AddAsync(product);

        // Write the product to the cache (write through)
        var cacheKey = $"product:{product.Id}";
        
        await cache.SetAsync(cacheKey, product, new HybridCacheEntryOptions
        {
            Expiration = TimeSpan.FromMinutes(10)
        });
    }
}
```

Here, every time you save (update) a product, you do so both in the cache and the database, ensuring immediate consistency.

**Write Through** strategy is often combined with **Cache Aside** or **Read Through Cache** strategies on the reading side too.

**Pros:**

*   Immediate consistency — cache and database are always in sync.
*   Simpler logic for reading (always trust the cache).
*   Ideal for critical data that cannot risk inconsistency.

**Cons:**

*   Each write is more expensive, since it hits both the cache and the database.
*   If either the cache or database is slow, writes can be bottlenecked.
*   Doesn't reduce write load on the database.

**Real-World Use Cases:**

**1. Financial Transactions:**

*   **Scenario:** Banking, trading, or payment systems that cannot risk stale or lost data (e.g., account balances).
*   **Why Write Through?** Every write updates both the cache and the database simultaneously, ensuring absolute consistency for critical data like balances or transaction records. The cost in performance overhead is justified by accuracy requirements.

**2. Inventory with Real-Time Accuracy:**

*   **Scenario:** Certain e-commerce or retail systems need real-time accurate stock counts (e.g., for just-in-time inventory management).
*   **Why Write Through?** When an item is sold, you update both the DB and the cache. Immediately, the correct inventory count is available everywhere, preventing overselling or concurrency issues.

## Summary

**Let's recap the Caching Strategies:**

*   **Cache Aside:** Lazy loading of cache entries on a miss, widely used for flexible read scenarios.
*   **Read Through:** The cache handles loading the data from the database automatically.
*   **Write Around:** Writes go straight to the database, and the cache only updates on the next read.
*   **Write Back:** Writes go to the cache first, and the database is updated asynchronously later.
*   **Write Through:** Writes go to both the cache and the database at the same time.

**Choosing the Right Strategy:**

*   If you want simplicity and a typical approach: Cache Aside.
*   If your caching layer can autonomously load data, reducing application complexity: Read Through.
*   If your system is write-heavy and you don't need immediate consistency: Write Around or Write Back.
*   If immediate consistency in cache and DB is crucial: Write Through.

Carefully choose a caching strategy based on:

*   Read/Write Mix
*   Performance requirements
*   Consistency requirements

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-implement-caching-strategies-in-dotnet)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-implement-caching-strategies-in-dotnet&title=How%20To%20Implement%20Caching%20Strategies%20in%20.NET)[X](https://twitter.com/intent/tweet?text=How%20To%20Implement%20Caching%20Strategies%20in%20.NET&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-implement-caching-strategies%2Fdotnet)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-implement-caching-strategies-in-dotnet)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.

---
**Image Description:** The article's cover image features a dark background with abstract purple swooshes. On the left, a white square icon containing `</>` symbols is displayed above the text "dev tips". On the right, large white text reads "HOW TO IMPLEMENT CACHING STRATEGIES IN .NET".