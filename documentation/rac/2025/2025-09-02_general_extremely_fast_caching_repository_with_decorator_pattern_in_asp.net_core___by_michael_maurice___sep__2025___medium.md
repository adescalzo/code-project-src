```yaml
---
title: "Extremely FAST Caching Repository With Decorator Pattern in ASP.NET Core | by Michael Maurice | Sep, 2025 | Medium"
source: https://medium.com/@michaelmaurice410/extremely-fast-caching-repository-with-decorator-pattern-in-asp-net-core-f263396cfa23
date_published: 2025-09-02T17:01:39.000Z
date_captured: 2025-09-10T12:48:01.138Z
domain: medium.com
author: Michael Maurice
category: general
technologies: [ASP.NET Core, Entity Framework Core, SQL Server, IMemoryCache, Redis, Scrutor, StackExchange.Redis, BenchmarkDotNet, System.Diagnostics.Metrics, .NET 9]
programming_languages: [C#, SQL]
tags: [caching, decorator-pattern, aspnet-core, performance-optimization, repository-pattern, dependency-injection, distributed-caching, in-memory-caching, database-performance, .net]
key_concepts: [Decorator Pattern, Caching Strategies, Repository Pattern, Dependency Injection, Cache Invalidation, Performance Monitoring, Performance Benchmarking, Database Optimization]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article demonstrates how to implement a fast caching repository in ASP.NET Core using the Decorator Pattern to dramatically improve application performance. It covers both in-memory caching with `IMemoryCache` and distributed caching with Redis, showcasing how to reduce database calls by up to 95% and achieve 10x faster response times. The content provides detailed code examples for setting up the repository, implementing the caching decorator, configuring dependency injection with Scrutor, and advanced topics like smart cache invalidation and performance benchmarking. The author emphasizes maintaining clean architecture, testability, and scalability while achieving significant performance gains.]
---
```

# Extremely FAST Caching Repository With Decorator Pattern in ASP.NET Core | by Michael Maurice | Sep, 2025 | Medium

# Extremely FAST Caching Repository With Decorator Pattern in ASP.NET Core

[

![Michael Maurice](https://miro.medium.com/v2/resize:fill:64:64/1*Vydee41-YhCgiyTaA_dPoA.png)

](/@michaelmaurice410?source=post_page---byline--f263396cfa23---------------------------------------)

[Michael Maurice](/@michaelmaurice410?source=post_page---byline--f263396cfa23---------------------------------------)

Follow

11 min read

·

Sep 2, 2025

75

Listen

Share

More

Press enter or click to view image in full size

![Diagram illustrating the performance benefits of implementing a Caching Repository with the Decorator Pattern in ASP.NET Core. It contrasts a "Before" scenario, where a Controller directly accesses a Repository and Database, leading to high latency (10-50ms per call) and heavy database load. The "After" scenario shows the Decorator Pattern integrating an in-memory or Redis cache between the Controller and the base Repository, resulting in sub-1ms cache hits, 95% fewer database calls, and 10x faster responses without requiring code changes in the controller. The diagram also highlights the ability to track metrics like cache hits, misses, and response duration.](https://miro.medium.com/v2/resize:fit:700/1*kVYiMB9GXINAoUhKwy8juw.png)

**If you want the full source code, download it from this link:** [https://www.elitesolutions.shop/](https://www.elitesolutions.shop/)

The Decorator Pattern combined with caching is one of the most effective ways to dramatically improve repository performance without changing existing code. This powerful combination can reduce database calls by 95% and improve response times by 10x or more, all while maintaining clean, testable, and maintainable code architecture.

## Understanding the Performance Problem

## The Database Bottleneck

In typical ASP.NET Core applications, every request that needs data goes directly to the database:

```csharp
// Traditional approach - hits database every time  
public class CustomerController : ControllerBase  
{  
    private readonly ICustomerRepository _repository;  
    [HttpGet("{id}")]  
    public async Task<Customer> Get(int id)  
    {  
        return await _repository.GetByIdAsync(id); // Database hit every time  
    }  
}
```

Performance Issues:

*   Database Load: Every request creates database traffic
*   Response Time: 10–50ms per database query adds up
*   Resource Consumption: Database connections and CPU usage
*   Scalability Limits: Database becomes bottleneck under load

## The Solution: Decorator Pattern + Caching

The Decorator Pattern allows you to add caching behavior without modifying existing repository code:

```csharp
// After: Same interface, cached behavior  
var customer = await _repository.GetByIdAsync(id); // Cached response < 1ms
```

Performance Gains:

*   95% fewer database calls: Most requests served from cache
*   10x faster response times: Sub-millisecond cached responses
*   Reduced database load: Only cache misses hit database
*   Zero code changes: Existing controllers work unchanged

## Implementing the Basic Caching Decorator

## Step 1: Repository Interface

Start with a clean repository interface:

```csharp
// Domain/Interfaces/ICustomerRepository.cs  
public interface ICustomerRepository  
{  
    Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default);  
    Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);  
    Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default);  
    Task<List<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default);  
    Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default);  
    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);  
    Task DeleteAsync(int id, CancellationToken cancellationToken = default);  
}
```

## Step 2: Base Repository Implementation

Implement the standard repository without caching:

```csharp
// Infrastructure/Repositories/CustomerRepository.cs  
public class CustomerRepository : ICustomerRepository  
{  
    private readonly ApplicationDbContext _context;  
    private readonly ILogger<CustomerRepository> _logger;  
    public CustomerRepository(ApplicationDbContext context, ILogger<CustomerRepository> logger)  
    {  
        _context = context;  
        _logger = logger;  
    }  
    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)  
    {  
        _logger.LogDebug("Fetching customer {CustomerId} from database", id);  
          
        return await _context.Customers  
            .Include(c => c.Orders)  
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);  
    }  
    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)  
    {  
        _logger.LogDebug("Fetching customer by email {Email} from database", email);  
          
        return await _context.Customers  
            .Include(c => c.Orders)  
            .FirstOrDefaultAsync(c => c.Email == email, cancellationToken);  
    }  
    public async Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default)  
    {  
        _logger.LogDebug("Fetching all customers from database");  
          
        return await _context.Customers  
            .Include(c => c.Orders)  
            .OrderBy(c => c.Name)  
            .ToListAsync(cancellationToken);  
    }  
    public async Task<List<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)  
    {  
        _logger.LogDebug("Fetching active customers from database");  
          
        return await _context.Customers  
            .Where(c => c.IsActive)  
            .Include(c => c.Orders)  
            .OrderBy(c => c.Name)  
            .ToListAsync(cancellationToken);  
    }  
    public async Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)  
    {  
        _context.Customers.Add(customer);  
        await _context.SaveChangesAsync(cancellationToken);  
        return customer;  
    }  
    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)  
    {  
        _context.Customers.Update(customer);  
        await _context.SaveChangesAsync(cancellationToken);  
    }  
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)  
    {  
        var customer = await _context.Customers.FindAsync(new object[] { id }, cancellationToken);  
        if (customer != null)  
        {  
            _context.Customers.Remove(customer);  
            await _context.SaveChangesAsync(cancellationToken);  
        }  
    }  
}
```

## Step 3: Caching Decorator Implementation

Create the caching decorator that wraps the base repository:

```csharp
// Infrastructure/Repositories/CachedCustomerRepository.cs  
public class CachedCustomerRepository : ICustomerRepository  
{  
    private readonly ICustomerRepository _decorated;  
    private readonly IMemoryCache _cache;  
    private readonly ILogger<CachedCustomerRepository> _logger;  
      
    // Cache configuration  
    private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(15);  
    private readonly TimeSpan _shortCacheDuration = TimeSpan.FromMinutes(5);  
      
    public CachedCustomerRepository(        ICustomerRepository decorated,  
        IMemoryCache cache,  
        ILogger<CachedCustomerRepository> logger)  
    {  
        _decorated = decorated;  
        _cache = cache;  
        _logger = logger;  
    }  
    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)  
    {  
        var cacheKey = $"customer_id_{id}";  
          
        if (_cache.TryGetValue(cacheKey, out Customer? cachedCustomer))  
        {  
            _logger.LogDebug("Cache hit for customer {CustomerId}", id);  
            return cachedCustomer;  
        }  
        _logger.LogDebug("Cache miss for customer {CustomerId}", id);  
          
        var customer = await _decorated.GetByIdAsync(id, cancellationToken);  
          
        if (customer != null)  
        {  
            var cacheOptions = new MemoryCacheEntryOptions  
            {  
                AbsoluteExpirationRelativeToNow = _defaultCacheDuration,  
                SlidingExpiration = TimeSpan.FromMinutes(5),  
                Priority = CacheItemPriority.Normal  
            };  
            _cache.Set(cacheKey, customer, cacheOptions);  
            _logger.LogDebug("Cached customer {CustomerId} for {Duration}", id, _defaultCacheDuration);  
        }  
        return customer;  
    }  
    public async Task<Customer?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)  
    {  
        var cacheKey = $"customer_email_{email.ToLowerInvariant()}";  
          
        if (_cache.TryGetValue(cacheKey, out Customer? cachedCustomer))  
        {  
            _logger.LogDebug("Cache hit for customer email {Email}", email);  
            return cachedCustomer;  
        }  
        _logger.LogDebug("Cache miss for customer email {Email}", email);  
          
        var customer = await _decorated.GetByEmailAsync(email, cancellationToken);  
          
        if (customer != null)  
        {  
            var cacheOptions = new MemoryCacheEntryOptions  
            {  
                AbsoluteExpirationRelativeToNow = _defaultCacheDuration,  
                SlidingExpiration = TimeSpan.FromMinutes(5)  
            };  
            _cache.Set(cacheKey, customer, cacheOptions);  
              
            // Also cache by ID for cross-reference  
            var idCacheKey = $"customer_id_{customer.Id}";  
            _cache.Set(idCacheKey, customer, cacheOptions);  
        }  
        return customer;  
    }  
    public async Task<List<Customer>> GetAllAsync(CancellationToken cancellationToken = default)  
    {  
        const string cacheKey = "all_customers";  
          
        if (_cache.TryGetValue(cacheKey, out List<Customer>? cachedCustomers))  
        {  
            _logger.LogDebug("Cache hit for all customers");  
            return cachedCustomers!;  
        }  
        _logger.LogDebug("Cache miss for all customers");  
          
        var customers = await _decorated.GetAllAsync(cancellationToken);  
          
        var cacheOptions = new MemoryCacheEntryOptions  
        {  
            AbsoluteExpirationRelativeToNow = _shortCacheDuration, // Shorter cache for large datasets  
            Priority = CacheItemPriority.Low // Lower priority for large datasets  
        };  
        _cache.Set(cacheKey, customers, cacheOptions);  
        _logger.LogDebug("Cached {Count} customers", customers.Count);  
        return customers;  
    }  
    public async Task<List<Customer>> GetActiveCustomersAsync(CancellationToken cancellationToken = default)  
    {  
        const string cacheKey = "active_customers";  
          
        if (_cache.TryGetValue(cacheKey, out List<Customer>? cachedCustomers))  
        {  
            _logger.LogDebug("Cache hit for active customers");  
            return cachedCustomers!;  
        }  
        _logger.LogDebug("Cache miss for active customers");  
          
        var customers = await _decorated.GetActiveCustomersAsync(cancellationToken);  
          
        var cacheOptions = new MemoryCacheEntryOptions  
        {  
            AbsoluteExpirationRelativeToNow = _shortCacheDuration,  
            Priority = CacheItemPriority.Normal  
        };  
        _cache.Set(cacheKey, customers, cacheOptions);  
        return customers;  
    }  
    // Write operations invalidate cache  
    public async Task<Customer> AddAsync(Customer customer, CancellationToken cancellationToken = default)  
    {  
        var result = await _decorated.AddAsync(customer, cancellationToken);  
          
        // Invalidate relevant cache entries  
        InvalidateCustomerCaches();  
          
        _logger.LogDebug("Invalidated caches after adding customer");  
          
        return result;  
    }  
    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)  
    {  
        await _decorated.UpdateAsync(customer, cancellationToken);  
          
        // Invalidate specific customer cache and list caches  
        InvalidateCustomerCache(customer.Id);  
        InvalidateCustomerCaches();  
          
        _logger.LogDebug("Invalidated caches after updating customer {CustomerId}", customer.Id);  
    }  
    public async Task DeleteAsync(int id, CancellationToken cancellationToken = default)  
    {  
        await _decorated.DeleteAsync(id, cancellationToken);  
          
        // Invalidate specific customer cache and list caches  
        InvalidateCustomerCache(id);  
        InvalidateCustomerCaches();  
          
        _logger.LogDebug("Invalidated caches after deleting customer {CustomerId}", id);  
    }  
    private void InvalidateCustomerCache(int customerId)  
    {  
        var cacheKey = $"customer_id_{customerId}";  
        _cache.Remove(cacheKey);  
    }  
    private void InvalidateCustomerCaches()  
    {  
        // Invalidate list caches  
        _cache.Remove("all_customers");  
        _cache.Remove("active_customers");  
          
        // Could also implement more sophisticated cache invalidation  
        // by tracking which individual customer caches to invalidate  
    }  
}
```

## Advanced Configuration with Scrutor

## Dependency Injection Setup

Use the Scrutor library for elegant decorator registration:

```csharp
// Program.cs  
using Scrutor;  
var builder = WebApplication.CreateBuilder(args);  
// Add basic services  
builder.Services.AddDbContext<ApplicationDbContext>(options =>  
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));  
builder.Services.AddMemoryCache();  
builder.Services.AddLogging();  
// Register base repository  
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();  
// Decorate with caching using Scrutor  
builder.Services.Decorate<ICustomerRepository, CachedCustomerRepository>();  
// Alternative: Manual registration (without Scrutor)  
// builder.Services.AddScoped<ICustomerRepository>(provider =>  
// {  
//     var baseRepository = new CustomerRepository(  
//         provider.GetRequiredService<ApplicationDbContext>(),  
//         provider.GetRequiredService<ILogger<CustomerRepository>>());  
//       
//     return new CachedCustomerRepository(  
//         baseRepository,  
//         provider.GetRequiredService<IMemoryCache>(),  
//         provider.GetRequiredService<ILogger<CachedCustomerRepository>>());  
// });  
var app = builder.Build();  
app.MapControllers();  
app.Run();
```

## Scrutor Installation

```bash
# Install Scrutor for elegant decorator registration  
dotnet add package Scrutor
```

## Multiple Decorators with Scrutor

Stack multiple decorators for different concerns:

```csharp
// Register base repository  
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();  
// Apply decorators in order  
builder.Services.Decorate<ICustomerRepository, LoggingCustomerRepository>();  
builder.Services.Decorate<ICustomerRepository, CachedCustomerRepository>();  
builder.Services.Decorate<ICustomerRepository, MetricsCustomerRepository>();  
// Execution order: Controller -> Metrics -> Cache -> Logging -> Base Repository
```

## Performance Optimizations

## Smart Cache Key Generation

Implement efficient cache key strategies:

```csharp
public class CacheKeyGenerator  
{  
    private const string KeyPrefix = "customer";  
    private const char Separator = '_';  
    public static string ForCustomerId(int id) => $"{KeyPrefix}{Separator}id{Separator}{id}";  
      
    public static string ForCustomerEmail(string email) =>   
        $"{KeyPrefix}{Separator}email{Separator}{email.ToLowerInvariant()}";  
      
    public static string ForActiveCustomers() => $"{KeyPrefix}{Separator}active";  
      
    public static string ForAllCustomers() => $"{KeyPrefix}{Separator}all";  
      
    public static string ForCustomersByCity(string city) =>   
        $"{KeyPrefix}{Separator}city{Separator}{city.ToLowerInvariant()}";  
    // Hierarchical key for easy invalidation  
    public static class Patterns  
    {  
        public const string CustomerById = "customer_id_*";  
        public const string CustomerByEmail = "customer_email_*";  
        public const string CustomerLists = "customer_all*";  
    }  
}
```

## Cache Entry Options Configuration

Optimize cache behavior for different data types:

```csharp
public class CacheOptionsFactory  
{  
    public static MemoryCacheEntryOptions ForSingleEntity()  
    {  
        return new MemoryCacheEntryOptions  
        {  
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),  
            SlidingExpiration = TimeSpan.FromMinutes(5),  
            Priority = CacheItemPriority.Normal,  
            Size = 1 // For cache size limiting  
        };  
    }  
    public static MemoryCacheEntryOptions ForEntityList()  
    {  
        return new MemoryCacheEntryOptions  
        {  
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),  
            SlidingExpiration = TimeSpan.FromMinutes(2),  
            Priority = CacheItemPriority.Low, // Lists are lower priority  
            Size = 10 // Lists take more space  
        };  
    }  
    public static MemoryCacheEntryOptions ForFrequentlyAccessed()  
    {  
        return new MemoryCacheEntryOptions  
        {  
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1),  
            SlidingExpiration = TimeSpan.FromMinutes(15),  
            Priority = CacheItemPriority.High,  
            Size = 1  
        };  
    }  
    public static MemoryCacheEntryOptions WithEvictionCallback(PostEvictionDelegate callback)  
    {  
        var options = ForSingleEntity();  
        options.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration  
        {  
            EvictionCallback = callback  
        });  
        return options;  
    }  
}
```

## Advanced Cache Invalidation

Implement sophisticated cache invalidation strategies:

```csharp
public class SmartCacheInvalidator  
{  
    private readonly IMemoryCache _cache;  
    private readonly ILogger<SmartCacheInvalidator> _logger;  
      
    // Track cache dependencies  
    private readonly ConcurrentDictionary<string, HashSet<string>> _dependencies = new();  
    public SmartCacheInvalidator(IMemoryCache cache, ILogger<SmartCacheInvalidator> logger)  
    {  
        _cache = cache;  
        _logger = logger;  
    }  
    public void SetWithDependencies(string key, object value, MemoryCacheEntryOptions options, params string[] dependencies)  
    {  
        // Track dependencies  
        foreach (var dependency in dependencies)  
        {  
            _dependencies.AddOrUpdate(dependency,   
                new HashSet<string> { key },   
                (_, existing) =>   
                {  
                    existing.Add(key);  
                    return existing;  
                });  
        }  
        // Set up eviction callback to clean up dependencies  
        options.PostEvictionCallbacks.Add(new PostEvictionCallbackRegistration  
        {  
            EvictionCallback = (evictedKey, value, reason, state) => CleanupDependencies(evictedKey.ToString()!)  
        });  
        _cache.Set(key, value, options);  
    }  
    public void InvalidateByDependency(string dependency)  
    {  
        if (_dependencies.TryGetValue(dependency, out var dependentKeys))  
        {  
            foreach (var key in dependentKeys.ToList())  
            {  
                _cache.Remove(key);  
                _logger.LogDebug("Invalidated cache key {CacheKey} due to dependency {Dependency}", key, dependency);  
            }  
              
            _dependencies.TryRemove(dependency, out _);  
        }  
    }  
    private void CleanupDependencies(string key)  
    {  
        // Remove this key from all dependency tracking  
        foreach (var kvp in _dependencies.ToList())  
        {  
            kvp.Value.Remove(key);  
            if (!kvp.Value.Any())  
            {  
                _dependencies.TryRemove(kvp.Key, out _);  
            }  
        }  
    }  
}  
// Usage in cached repository  
public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)  
{  
    var cacheKey = CacheKeyGenerator.ForCustomerId(id);  
      
    if (_cache.TryGetValue(cacheKey, out Customer? cachedCustomer))  
    {  
        return cachedCustomer;  
    }  
    var customer = await _decorated.GetByIdAsync(id, cancellationToken);  
      
    if (customer != null)  
    {  
        var options = CacheOptionsFactory.ForSingleEntity();  
          
        // Track dependencies for smart invalidation  
        _cacheInvalidator.SetWithDependencies(  
            cacheKey,   
            customer,   
            options,  
            $"customer_{id}",      // Individual customer dependency  
            "all_customers",       // All customers list dependency  
            $"city_{customer.City}" // City-based dependency  
        );  
    }  
    return customer;  
}
```

## Distributed Caching with Redis

## Redis Cache Implementation

Scale beyond single server with Redis:

```csharp
// Install Redis packages  
// dotnet add package Microsoft.Extensions.Caching.StackExchangeRedis  
// dotnet add package StackExchange.Redis  
public class RedisCachedCustomerRepository : ICustomerRepository  
{  
    private readonly ICustomerRepository _decorated;  
    private readonly IDistributedCache _cache;  
    private readonly ILogger<RedisCachedCustomerRepository> _logger;  
    private readonly JsonSerializerOptions _jsonOptions;  
      
    private readonly TimeSpan _defaultCacheDuration = TimeSpan.FromMinutes(15);  
    public RedisCachedCustomerRepository(        ICustomerRepository decorated,  
        IDistributedCache cache,  
        ILogger<RedisCachedCustomerRepository> logger)  
    {  
        _decorated = decorated;  
        _cache = cache;  
        _logger = logger;  
          
        _jsonOptions = new JsonSerializerOptions  
        {  
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,  
            WriteIndented = false,  
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull  
        };  
    }  
    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)  
    {  
        var cacheKey = CacheKeyGenerator.ForCustomerId(id);  
          
        var cachedJson = await _cache.GetStringAsync(cacheKey, cancellationToken);  
        if (!string.IsNullOrEmpty(cachedJson))  
        {  
            _logger.LogDebug("Redis cache hit for customer {CustomerId}", id);  
            return JsonSerializer.Deserialize<Customer>(cachedJson, _jsonOptions);  
        }  
        _logger.LogDebug("Redis cache miss for customer {CustomerId}", id);  
          
        var customer = await _decorated.GetByIdAsync(id, cancellationToken);  
          
        if (customer != null)  
        {  
            var serializedCustomer = JsonSerializer.Serialize(customer, _jsonOptions);  
            var cacheOptions = new DistributedCacheEntryOptions  
            {  
                AbsoluteExpirationRelativeToNow = _defaultCacheDuration,  
                SlidingExpiration = TimeSpan.FromMinutes(5)  
            };  
            await _cache.SetStringAsync(cacheKey, serializedCustomer, cacheOptions, cancellationToken);  
            _logger.LogDebug("Cached customer {CustomerId} in Redis", id);  
        }  
        return customer;  
    }  
    // Write operations with distributed cache invalidation  
    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)  
    {  
        await _decorated.UpdateAsync(customer, cancellationToken);  
          
        // Invalidate distributed cache  
        var cacheKey = CacheKeyGenerator.ForCustomerId(customer.Id);  
        await _cache.RemoveAsync(cacheKey, cancellationToken);  
          
        // Invalidate list caches  
        await _cache.RemoveAsync(CacheKeyGenerator.ForAllCustomers(), cancellationToken);  
        await _cache.RemoveAsync(CacheKeyGenerator.ForActiveCustomers(), cancellationToken);  
          
        _logger.LogDebug("Invalidated Redis cache after updating customer {CustomerId}", customer.Id);  
    }  
}  
// Program.cs configuration  
builder.Services.AddStackExchangeRedisCache(options =>  
{  
    options.Configuration = builder.Configuration.GetConnectionString("Redis");  
    options.InstanceName = "CustomerAPI";  
});  
// Register Redis cached repository  
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();  
builder.Services.Decorate<ICustomerRepository, RedisCachedCustomerRepository>();
```

## Performance Monitoring and Metrics

## Cache Performance Tracking

Monitor cache effectiveness:

```csharp
public class CacheMetricsDecorator : ICustomerRepository  
{  
    private readonly ICustomerRepository _decorated;  
    private readonly ILogger<CacheMetricsDecorator> _logger;  
    private readonly IMetrics _metrics;  
      
    // Metrics counters  
    private readonly Counter<long> _cacheHits;  
    private readonly Counter<long> _cacheMisses;  
    private readonly Histogram<double> _responseTime;  
    public CacheMetricsDecorator(        ICustomerRepository decorated,  
        ILogger<CacheMetricsDecorator> logger,  
        IMeterFactory meterFactory)  
    {  
        _decorated = decorated;  
        _logger = logger;  
          
        var meter = meterFactory.Create("CustomerRepository");  
        _cacheHits = meter.CreateCounter<long>("customer_cache_hits_total");  
        _cacheMisses = meter.CreateCounter<long>("customer_cache_misses_total");  
        _responseTime = meter.CreateHistogram<double>("customer_repository_duration_ms");  
    }  
    public async Task<Customer?> GetByIdAsync(int id, CancellationToken cancellationToken = default)  
    {  
        var stopwatch = Stopwatch.StartNew();  
          
        try  
        {  
            // This will be the cached repository  
            var result = await _decorated.GetByIdAsync(id, cancellationToken);  
              
            stopwatch.Stop();  
              
            // Record metrics based on whether we got a result quickly (likely cached)  
            // or slowly (likely database)  
            if (stopwatch.ElapsedMilliseconds < 10) // Likely cache hit  
            {  
                _cacheHits.Add(1, new TagList { { "method", "GetByIdAsync" } });  
            }  
            else  
            {  
                _cacheMisses.Add(1, new TagList { { "method", "GetByIdAsync" } });  
            }  
              
            _responseTime.Record(stopwatch.ElapsedMilliseconds,   
                new TagList { { "method", "GetByIdAsync" } });  
              
            return result;  
        }  
        finally  
        {  
            if (stopwatch.IsRunning) stopwatch.Stop();  
        }  
    }  
    // Implement other methods with similar metrics tracking...  
}  
// Register with metrics  
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();  
builder.Services.Decorate<ICustomerRepository, CachedCustomerRepository>();  
builder.Services.Decorate<ICustomerRepository, CacheMetricsDecorator>();
```

## Performance Benchmarking

Create benchmarks to measure cache effectiveness:

```csharp
[MemoryDiagnoser]  
[SimpleJob(RuntimeMoniker.Net90)]  
public class