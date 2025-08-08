```yaml
---
title: How To Increase Performance of Web APIs in .NET
source: https://antondevtips.com/blog/how-to-increase-performance-of-web-apis-in-dotnet
date_published: 2025-01-21T08:06:20.307Z
date_captured: 2025-08-06T17:35:19.332Z
domain: antondevtips.com
author: Anton Martyniuk
category: performance
technologies: [.NET, ASP.NET Core, Entity Framework Core, Dapper, System.Text.Json, Newtonsoft.Json, Redis, OutputCache, IDistributedCache, HybridCache, Fusion Cache, Brotli, GZIP, Kestrel, HTTP/2, HTTP/3, HTTP/1.1, CDN, Cloudflare, GraphQL, Hot Chocolate, OpenTelemetry, Application Insights]
programming_languages: [C#, SQL]
tags: [web-api, performance, dotnet, caching, database, optimization, asynchronous-programming, graphql, minimal-apis, middleware]
key_concepts: [asynchronous-programming, data-access-optimization, data-transfer-reduction, json-serialization, caching-strategies, response-compression, middleware-pipeline, http-protocol-optimization, content-delivery-networks, rate-limiting, minimal-apis, graphql-integration]
code_examples: false
difficulty_level: intermediate
summary: |
  This article outlines 12 essential techniques to significantly enhance the performance of Web APIs built with .NET. It covers fundamental optimizations like asynchronous programming and efficient data access patterns using EF Core or Dapper, alongside strategies for reducing data transfer through projections, pagination, and filtering. The post delves into advanced performance boosts such as minimizing JSON serialization overhead, implementing various caching mechanisms (OutputCache, HybridCache, Redis), and enabling response compression. Furthermore, it discusses optimizing the middleware pipeline, leveraging modern HTTP protocols (HTTP/2, HTTP/3), and utilizing Content Delivery Networks. Finally, it explores implementing rate limiting, adopting Minimal APIs for reduced overhead, and integrating GraphQL for more efficient data fetching.
---
```

# How To Increase Performance of Web APIs in .NET

![Cover image for the article "How To Increase Performance of Web APIs in .NET", featuring a white code icon and "dev tips" text on a dark blue background with abstract purple swooshes.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_asp_webapi_performance.png&w=3840&q=100)

# How To Increase Performance of Web APIs in .NET

Jan 21, 2025

7 min read

### Newsletter Sponsors

[Master The Clean Architecture](https://www.courses.milanjovanovic.tech/a/aff_q2snzxhl/external?affcode=1486372_j2vpyytw). This comprehensive course will teach you how to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture. Join 3,600+ students to accelerate your growth as a software architect.

[Master The Modular Monolith Architecture](https://www.courses.milanjovanovic.tech/a/aff_l825rpg0/external?affcode=1486372_j2vpyytw). This in-depth course will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario. Join 1,600+ students to accelerate your growth as a software architect.

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

In modern software development, creating high-performance APIs is crucial to deliver a fast, stable, and responsive user experience. Whether you are building a new web application or optimizing an existing one, in this blog post you will learn the proven strategies in .NET to increase performance.

This blog post will walk you through 12 essential techniques that you can adopt to speed up your APIs, enhance scalability, and improve maintainability.

## 1\. Asynchronous Programming with async/await

Asynchronous programming prevents your application from blocking threads while waiting for I/O operations to complete. Among these operations: reading files, waiting for database results, waiting for API call results, etc.

By using async/await, .NET can serve more requests concurrently without blocking the threads. This increases scalability. Prefer using asynchronous methods over synchronous:

```csharp
[HttpGet("items")]
public async Task<IActionResult> GetItemsAsync()
{
    var items = await _itemService.GetItemsAsync();
    return Ok(items);
}
```

## 2\. Improve Data Access Patterns for Read Queries (with EF Core or Dapper)

Data access patterns greatly impact API performance. There are 2 popular libraries for database access: **EF Core** and **Dapper**.

To improve data access patterns, [consider optimizing your queries](https://antondevtips.com/blog/how-to-increase-ef-core-performance-for-read-queries-in-dotnet). Add database indexes, optimize your queries, in EF Core consider using `AsNoTracking` for read-only queries.

```csharp
// Read-only query with AsNoTracking
public async Task<List<User>> GetUsersAsync()
{
    return await _dbContext.Users
        .AsNoTracking()
        .Where(u => u.IsActive)
        .ToListAsync();
}
```

If you are using Dapper, ensure parameterized queries, proper indexes, and minimal round trips to the database.

## 3\. Reduce Data Transfer with Projections, Pagination and Filtering

Transferring large data sets can slow down your API. Instead, return only the data your client needs.

When reading data from the database, consider using:

*   Projection
*   Filtering, sorting
*   Pagination for large data sets

Projection allows you to select only the needed fields instead of retrieving a whole row:

```csharp
var book = await context.Books
    .Include(b => b.Author)
    .Where(b => b.Id == id)
    .Select(b => new BooksPreviewResponse
    {
        Title = b.Title, Author = b.Author.Name, Year = b.Year
    })
    .FirstOrDefaultAsync(cancellationToken);
```

To [increase performance of read queries](https://antondevtips.com/blog/how-to-increase-ef-core-performance-for-read-queries-in-dotnet) consider playing around with indexes, filtering and sort conditions.

For large datasets, consider using pagination to prevent returning too much data to the client at once:

```csharp
[HttpGet("users")]
public async Task<IActionResult> GetUsersAsync(int pageNumber = 1,
    int pageSize = 10)
{
    var query = _dbContext.Users.AsNoTracking().Where(u => u.IsActive);

    var users = await query
        .Skip((pageNumber - 1) * pageSize)
        .Take(pageSize)
        .Select(u => new UserDto
        {
            Id = u.Id,
            Name = u.Name
        })
        .ToListAsync();

    return Ok(users);
}
```

## 4\. Minimize JSON Serialization Overhead

When returning JSON responses, the default `System.Text.Json` library is often faster than `Newtonsoft.Json`. Configure serialization options to trim unnecessary fields and speed up the process.

```csharp
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            
        options.JsonSerializerOptions.IgnoreNullValues = true;
    });
```

*   `PropertyNamingPolicy = JsonNamingPolicy.CamelCase`: Ensures consistent naming in JSON.
*   `IgnoreNullValues = true`: Excludes null properties from the output, reducing payload size.

This is a small optimization but can be really handy.

## 5\. Add Caching

Caching is one of the most significant optimizations you can apply. Storing frequently accessed data in memory or using a distributed cache (like Redis) reduces database round trips.

I recommend using two types of caching:

*   OutputCache
*   IDistributedCache / HybridCache (.NET 9) / Fusion Cache (3rd party library that exists for years)

Output caching stores the entire HTTP response for a set duration, returning it directly without executing the controller or Minimal API method again:

```csharp
builder.Services.AddOutputCache();

var app = builder.Build();
app.UseOutputCache();

[HttpGet("products")]
[OutputCache(Duration = 60)] // Cache response for 60 seconds
public async Task<IActionResult> GetProductsAsync()
{
    var products = await _productService.GetProductsAsync();
    return Ok(products);
}
```

This is the easiest type of caching to be added to your application as it requires almost no code. You can invalidate (evict) cache by using tags.

OutputCache also supports Redis.

If you need a more granular approach and have more control over caching, consider using one of the caching libraries. Recently I have been exploring [Hybrid Cache in .NET 9](https://antondevtips.com/blog/how-to-improve-performance-of-my-aspnetcore-web-api-in-18x-times-using-hybridcache-in-dotnet-9) and can recommend using it. This is a two-level cache: InMemory + Distributed that solves the **Cache Stampede** problem (when multiple requests get a cache miss and all call the database in the cache-aside pattern).

Here is how you can use **HybridCache**:

```csharp
builder.Services.AddHybridCache();

[HttpGet("orders/{id}")]
public async Task<IActionResult> GetOrderAsync(int id,
    [FromServices] IHybridCache cache)
{
    string cacheKey = $"Order_{id}";
    var order = await cache.GetOrCreateAsync(cacheKey, async entry =>
    {
        entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10);
        using var context = new AppDbContext();
        return await context.Orders.FindAsync(id);
    });

    if (order is null)
    {
        return NotFound();
    }

    return Ok(order);
}
```

Use Distributed Cache like Redis when multiple applications need to connect to the cache or when you need to persist cache across application restarts.

## 6\. Enable Response Compression

Compressing your responses with Brotli or GZIP can significantly reduce payload size. Smaller responses mean faster data transfer and a better user experience.

Brotli and Gzip reduce the size of the outgoing JSON, HTML or Static files data. Adding them early in the pipeline will ensure smaller payloads.

```csharp
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
});

builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = System.IO.Compression.CompressionLevel.Fastest;
});

var app = builder.Build();
app.UseResponseCompression();
```

## 7\. Optimize Middleware Pipeline

The order of middleware affects performance. Certain middlewares should run as early as possible, while others (like routing, authentication, and authorization) should follow a logical sequence.

If you're using response compression, make sure to add it before serving the static files, to get smaller payloads:

```csharp
var app = builder.Build();

// Correct middleware ordering
app.UseResponseCompression(); // Early in the pipeline
app.UseStaticFiles();         // Serve static files

app.UseRouting();

app.UseAuthentication(); // Authentication before authorization
app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});
```

## 8\. Enable HTTP/2 and HTTP/3 Protocols

HTTP/2 and HTTP/3 provide significant performance improvements when compared to HTTP/1, including multiplexing and reduced latency. Configuring Kestrel to support multiple protocols is straightforward:

```csharp
// In Program.cs
builder.WebHost.ConfigureKestrel(options =>
{
    // HTTP/1.1
    options.ListenAnyIP(5000);
    
    // HTTPS with HTTP/1.1, HTTP/2 and HTTP/3
    options.ListenAnyIP(5001, listenOptions =>
    {
        listenOptions.UseHttps();
        listenOptions.Protocols = HttpProtocols.Http1AndHttp2AndHttp3;
    });
});

var app = builder.Build();
// Rest of the configuration
```

Clients that support newer protocols will automatically use them, while older clients can fall back to HTTP/1.1.

## 9\. Use Content Delivery Networks (CDNs)

A content delivery network (**CDN**) is a geographically distributed group of servers that caches content close to end users. A CDN allows for the quick transfer of assets needed for loading Internet content, including HTML pages, JavaScript files, stylesheets, images, and videos.

When a client in Europe accesses a website from the server located in New-York, a latency issues may occur. When a website uses CDN (Cloudflare, for example), the nearest server to the client (in Europe) caches the website contents and returns it to the client without sending requests to the original server.

By offloading static content delivery to a CDN, you reduce load on your own servers and improve global performance.

CDN gives you the following benefits:

*   Fewer Round Trips: CDNs serve content from edge nodes closer to the client, leading to quicker load times.
*   Reduced Server Load: Your application servers focus on core API functionalities.
*   Also, a lot of CDNs offer security and defense from SPAM, DDOS and BOT attacks.

## 10\. Implement Rate Limiting and Throttling

Rate limiting controls resource usage and prevents abusive behavior. By restricting the number of requests within a certain time window, you protect your API from excessive load or DDoS attacks.

Also, rate limiting can be implemented to handle a certain number of requests based on the user's subscription and restricting extra requests. This is how ChatGPT works, for example.

Here is how you can implement rate limiting with built-in capabilities in ASP.NET Core:

```csharp
// In Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, IPAddress>(context =>
    {
        var ipAddress = context.Connection.RemoteIpAddress;
        return RateLimitPartition.GetFixedWindowLimiter(ipAddress,
            _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = 100, // Allow 100 requests
            Window = TimeSpan.FromMinutes(1), // Per 1 minute window
            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
            QueueLimit = 0
        });
    });
});

var app = builder.Build();
app.UseRateLimiter();
```

This example uses the .NET rate limiting APIs to limit each IP to 100 requests per minute.

## 11\. Replace Controllers with Minimal APIs

Minimal APIs remove some overhead of the traditional MVC pipeline.

Minimal APIs in .NET 9 received a huge performance boost and can process **15%** more requests per second than in .NET 8. Also, Minimal APIs consume **93%** less memory compared to a previous version.

They are concise, fast, and support dependency injection, filters, and other common ASP.NET Core features:

```csharp
var app = builder.Build();

// Minimal API endpoint
app.MapGet("/customers/{id}", async (int id) =>
{
    var customer = await _dbContext.Customers.FindAsync(id);
    
    return customer is not null
        ? Results.Ok(customer)
        : Results.NotFound();
});

app.Run();
```

You can literally build anything using Minimal APIs when compared to Controllers. And Microsoft in each .NET release focuses mainly on Minimal APIs rather than Controllers.

## 12\. Use GraphQL

GraphQL can improve performance by allowing the client to query exactly the fields it needs from multiple resources in a single request. This is different from REST APIs when you need to send a new request for each resource.

The most popular and feature rich library for implementing GraphQL in .NET is `Hot Chocolate`:

Consider the following models:

```csharp
public record Product(
    int Id,
    string Name,
    decimal Price,
    bool IsAvailable
);

public record Review(
    int Id,
    int ProductId,
    string Content,
    int Rating
);

public record Recommendation(
    int Id,
    string Name,
    decimal Price
);
```

And the following GraphQL Queries:

```csharp
public record Product(int Id, string Name, decimal Price);

public class Query
{
    [UseOffsetPaging(MaxPageSize = 100, IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetProducts(AppDbContext context)
    {
        return context.Products;
    }

    public Task<Product?> GetProductByIdAsync(AppDbContext context, int id)
    {
        return await context.Products.FirstOrDefaultAsync(p => p.Id == id);
    }
    
    public async Task<Recommendation> GetProductRecommendationsByIdAsync(
        AppDbContext context,
        IRecommendationService service,
        int productId,
        int recommendationsCount)
    {
        var product = await context.Products.FindAsync(productId);
        if (product is null)
        {
            return [];
        }

        var recommendations = await service.GetRecommendationsForProduct(product.Name, recommendationsCount)
            .ToListAsync();

        return recommendations;
    }

    public async Task<Review> GetReviewsByProductIdAsync(
        AppDbContext context, int productId)
    {
        var reviews = await context.Reviews
            .AsNoTracking()
            .Where(r => r.ProductId == productId)
            .ToListAsync();

        return reviews;
    }
}
```

`IQueryable<Product> GetProducts` - returns IQueryable that is hooked to the GraphQL engine with the following middlewares:

*   filtering
*   sorting
*   pagination

Other methods are typical methods you have in your Web API endpoints using Controllers or Minimal APIs.

Here is how you can use GraphQL in your client web applications:

```csharp
query {
  getProducts(
    where: { name: { contains: "Phone" }, price: { eq: 1000 } }
    order: [{ name: ASC }
    skip: 10
    take: 20
  ) {
    id
    name
    price
    isAvailable
  }
}
```

We have full support for filtering, sorting and pagination with almost zero code; all is handled by Hot Chocolate.

And here is where GraphQL shines when compared to REST APIs:

```csharp
query {
  getProductById(id: 1) {
    id
    name
    price
    isAvailable
    
    getReviewsByProductId(productId: 1) {
      id
      content
      rating
    }
    getProductRecommendationsById(productId: 1) {
      id
      name
      price
    }
  }
}
```

Here we get product by identifier, product's reviews and recommendations in a single HTTP request to the server. When using REST APIs you will need to send 3 separate requests or implement some API aggregator on your own.

## Summary

Consider the following techniques to significantly boost your APIs throughput and user experience:

*   Leveraging async/await
*   Optimizing database queries
*   Employing projections, pagination, and filtering
*   Minimizing JSON serialization overhead
*   Implementing caching (Output Cache, In-memory, Redis, etc.)
*   Using response compression
*   Ordering middleware carefully
*   Enabling HTTP/2 and HTTP/3
*   Serving static assets through CDNs
*   Adding rate limiting and throttling
*   Switching to Minimal APIs in .NET 9
*   Integrating GraphQL

Adopt these tips incrementally, measure improvements using tools like OpenTelemetry, Benchmarks, or cloud-based tools like Application Insights and continuously refine your .NET web applications for the best results.

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-increase-performance-of-web-apis-in-dotnet&title=How%20To%20Increase%20Performance%20of%20Web%20APIs%20in%20.NET)[X](https://twitter.com/intent/tweet?text=How%20To%20Increase%20Performance%20of%20Web%20APIs%20in%20.NET&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-increase-performance-of-web-apis-in-dotnet)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-increase-performance-of-web-apis-in-dotnet)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.