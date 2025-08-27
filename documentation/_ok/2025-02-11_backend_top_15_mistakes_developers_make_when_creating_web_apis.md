```yaml
---
title: TOP 15 Mistakes Developers Make When Creating Web APIs
source: https://antondevtips.com/blog/top-15-mistakes-developers-make-when-creating-web-apis
date_published: 2025-02-11T08:45:28.790Z
date_captured: 2025-08-06T17:36:05.960Z
domain: antondevtips.com
author: Anton Martyniuk
category: backend
technologies: [ASP.NET Core, .NET, Nutrient PDF SDKs, OAuth 2.0, JWT, MemoryCache, HybridCache, Redis, Brotli, GZIP, Open Telemetry, Serilog, Seq, Jaeger, Loki, Prometheus, Grafana, .NET Aspire, Application Insights, Swagger, OpenAPI, Entity Framework Core, Dapper, Fluent Validation, SQL Server, Npgsql, HttpClient, JwtBearerDefaults, SymmetricSecurityKey, SqlCommand, ControllerBase, Minimal APIs]
programming_languages: [C#, SQL]
tags: [web-api, dotnet, aspnet-core, best-practices, api-design, security, performance, error-handling, data-access, monitoring]
key_concepts: [API Versioning, RESTful API design, Authentication and Authorization, Asynchronous Programming, Input Validation, Performance Optimization, Logging and Monitoring, Code Organization]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article identifies 15 common mistakes developers make when building Web APIs in .NET and provides practical solutions for each. It covers essential topics such as implementing API versioning, robust error handling with ProblemDetails, and securing endpoints using authentication and authorization. The post also emphasizes performance enhancements through asynchronous programming, efficient data access, caching strategies, and response compression. Additionally, it highlights the importance of adhering to RESTful conventions, validating input data, following security best practices, and ensuring comprehensive logging, monitoring, and API documentation. Finally, it advocates for better code organization by avoiding "fat controllers" and leveraging Minimal APIs for improved maintainability and performance.]
---
```

# TOP 15 Mistakes Developers Make When Creating Web APIs

![Cover image for "TOP 15 Mistakes Developers Make When Creating Web APIs" showing a code icon and "dev tips" text on a dark blue background with purple abstract shapes.](https://antondevtips.com/media/covers/aspnetcore/cover_asp_top_api_mistakes.png)

# TOP 15 Mistakes Developers Make When Creating Web APIs

Feb 11, 2025

6 min read

### Newsletter Sponsors

Bad PDFs drive users away. Slow load times, broken annotations, and clunky UX frustrates them giving them bad user experience.
Nutrient’s [PDF SDKs](https://www.nutrient.io/sdk?utm_campaign=newsletter1-2025-02-11&utm_source=anton-dev-tips&utm_medium=sponsoring) gives seamless document experiences, fast rendering, smooth annotations, real-time collaboration, and 100+ features. Used by 10K+ developers and serving over half a billion users worldwide.
[Explore the SDK](https://www.nutrient.io/sdk?utm_campaign=newsletter1-2025-02-11&utm_source=anton-dev-tips&utm_medium=sponsoring) with their free plan.

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

In this blog post, I'll walk you through the top 15 mistakes developers often make when building Web APIs in .NET. I will show you solutions to avoid and fix these mistakes.

Let's dive in!

## 1. Not Using API Versioning

**Mistake:** Without versioning, changes to your API can break existing client applications that rely on older endpoints.

```csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/api/products", async (IProductService service) =>
{
    var products = await service.GetProductsAsync();
    return Results.Ok(products);
});

app.Run();
```

**Solution:** Implement API versioning using URL segments, query parameters, or headers to ensure backward compatibility and smooth transitions between versions.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1,0);
    options.AssumeDefaultVersionWhenUnspecified = true;
});

var app = builder.Build();

app.MapGet("/api/v1/products", async (IProductService service) =>
{
    var products = await service.GetProductsAsync();
    return Results.Ok(products);
});

app.Run();
```

## 2. Poor Error Handling

**Mistake:** Poor error messages make it tough for API consumers to diagnose what went wrong.

```csharp
app.MapGet("/api/v1/order/{id}", (int id) =>
{
    if (id < 0)
    {
        return Results.Problem();
    }

    return Results.Ok(order);
});
```

**Solution:** Use standardized error responses like `ProblemDetails` and appropriate HTTP status codes to help clients understand and resolve problems.

```csharp
app.MapGet("/api/v1/order/{id}", (int id) =>
{
    if (id < 0)
    {
        return Results.Problem(
            detail: "The order ID provided is invalid.",
            statusCode: StatusCodes.Status400BadRequest,
            title: "Invalid Request"
        );
    }

    return Results.Ok(order);
});
```

## 3. Lack of Authentication and Authorization

**Mistake:** Failing to secure your API exposes it to unauthorized access and potential data breaches.

```csharp
// Program.cs - Mistake (no authentication setup, open endpoints)
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

// Anyone can call this endpoint without any credentials
app.MapGet("/api/orders", async (IOrder service) =>
{
    var orders = await service.GetOrdersAsync();
    return Results.Ok(orders);
});

app.Run();
```

**Solution:** Implement robust authentication and authorization mechanisms, such as OAuth 2.0 or JWT, to protect your API endpoints.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes("your-secret-key"))
        };
    });

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/orders", async (IOrder service) =>
{
    var orders = await service.GetOrdersAsync();
    return Results.Ok(orders);
}).RequireAuthorization();

app.Run();
```

## 4. Ignoring Asynchronous Programming

**Mistake:** Synchronous code can lead to thread blocking and preventing them from handling other requests. This reduces performance significantly.

```csharp
app.MapGet("/api/v1/products", (IProductService productService) =>
{
    var products = productService.GetProducts();
    return Ok(products);
});
```

**Solution:** Use `async` and `await` keywords paired with asynchronous methods, improving scalability of web server.

Asynchronous programming prevents your application from blocking threads while waiting for I/O operations to complete. Among these operations: reading files, waiting for database results, waiting for API call results, etc.

```csharp
app.MapGet("/api/v1/products", async (IProductService productService) =>
{
    var products = await productService.GetProductsAsync();
    return Ok(products);
});
```

## 5. Not Following RESTful Conventions

**Mistake:** Ignoring RESTful principles can lead to inconsistent and non-standard APIs.

```csharp
// Not following REST: using GET for deletion
app.MapGet("/api/deleteUser?id=123", () =>
{
    // Delete logic here
    return Results.Ok("Deleted");
});
```

**Solution:** Design your API following RESTful conventions, using appropriate HTTP methods (GET, POST, PUT, DELETE, etc.) and status codes.

```csharp
app.MapGet("/api/users/{id}", (int id) =>
{
    var user = ...;
    return Results.Ok(user);
});

app.MapPost("/api/users", (CreateUserRequest request) =>
{
    var user = request.MapToEntity();
    return Results.Created(user);
});

app.MapPut("/api/users/{id}", (int id, UpdateUserRequest request) =>
{
    // Update User
    return Results.NoContent();
});

app.MapDelete("/api/users/{id}", (int id) =>
{
    // Delete User    
    return Results.NoContent();
});
```

## 6. Not Validating Input Data

**Mistake:** Accepting unvalidated input can lead to security vulnerabilities and data integrity issues.

```csharp
app.MapPost("/api/users", (CreateUserRequest request) =>
{
    // No validation performed
    return Results.Ok(user);
});
```

**Solution:** Validate all incoming data using Data Annotations or Fluent Validation. Always be pessimistic about data your APIs consume.

```csharp
app.MapPost("/api/users", (CreateUserRequest request,
    IValidator<CreateUserRequest> validator) =>
{
    var validationResult = await validator.ValidateAsync(request);
    if (!validationResult.IsValid)
    {
        return Results.ValidationProblem(validationResult.ToDictionary());
    }
    
    return Results.Ok(user);
});
```

## 7. Ignoring Security Best Practices

**Mistake:** Ignoring security measures can expose your API to attacks like SQL injection or cross-site scripting.

```csharp
app.MapGet("/api/product/{id}", (string name) =>
{
    // Vulnerable SQL string concatenation
    var command = new SqlCommand(
        "SELECT * FROM Products WHERE Name = " + name, connection);
        
    connection.Open();
    using var reader = command.ExecuteReader();
    // ...
    return Results.Ok();
});
```

This API endpoint is vulnerable to SQL injection, as `name` parameter can accept any string like an SQL command.

**Solution:** Follow security best practices, such as using parameterized queries, encrypting sensitive data, and enforcing HTTPS.

```csharp
// Use safe methods from ORM like EF Core or Dapper
// Or use parameters to prevent SQL injection
app.MapGet("/api/product/{id}", (string name, ProductDbContext dbContext) =>
{
    var products = await dbContext.Products
        .Where(x => x.Name === name)
        .ToListAsync();
        
    return Results.Ok(products);
});
```

## 8. Poor Logging and Monitoring

**Mistake:** Without proper logging, diagnosing issues becomes challenging, especially in distributed systems.

**Solution:** Add Open Telemetry and Logging to your application to track application metrics, traces and logs. Use tools like Serilog for logging and Seq or Jaeger to view distributed traces. You can use Seq both for logs and distributed traces.

Consider these free tools for monitoring:

*   Jaeger (distributed traces)
*   Loki (logs)
*   Prometheus and Grafana (metrics)
*   Seq (logs and distributed traces) (free only for a single user)
*   .NET Aspire (logging, metrics, distributed traces)

You can also use cloud-based solutions for monitoring your applications like Application Insights.

```csharp
builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService("ShippingService"))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddRedisInstrumentation()
            .AddNpgsql();

        tracing.AddOtlpExporter();
    });
```

## 9. Lack of API Documentation

**Mistake:** Without clear documentation, clients struggle to understand how to use your API.

**Solution:** Provide thorough documentation using tools like Swagger/OpenAPI to generate interactive API docs.

```csharp
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/api/products", () => new[] { "Product1", "Product2" });

app.Run();
```

## 10. Not Optimizing Database and Data Access Layer

**Mistake:** Unoptimized database queries and inefficient data access code can lead to slow performance and scalability issues.

```csharp
// Fetching ALL columns
var book = await context.Books
    .Include(b => b.Author)
    .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
```

**Solution:** Optimize your database by indexing, optimizing queries, and using efficient data access patterns in EF Core or Dapper.

```csharp
// Fetching only needed columns without tracking
var book = await context.Books
    .Include(b => b.Author)
    .Where(b => b.Id == id)
    .Select(b => new BooksPreviewResponse
    {
        Title = b.Title, Author = b.Author.Name, Year = b.Year
    })
    .FirstOrDefaultAsync(cancellationToken);
```

## 11. Returning Too Much Data Without Paging, Filtering, or Sorting

**Mistake:** Sending large datasets can lead to performance bottlenecks and increased bandwidth usage.

```csharp
// Selecting all books (entire database)
var allBooks = await context.Books
    .Include(b => b.Author)
    .ToListAsync();
```

**Solution:** Implement paging, filtering, and sorting mechanisms to allow clients to retrieve only the data they need.

```csharp
// Use paging to select fixed number of records
int pageSize = 50;
int pageNumber = 1;

var books = context.Books
    .AsNoTracking()
    .OrderBy(p => p.Title)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToList();
```

## 12. Not Using Caching

**Mistake:** Not using cache for frequently accessed data can result in unnecessary database hits and slower response times.

**Solution:** Implement caching strategies using in-memory caches like MemoryCache/HybridCache (.NET 9) or distributed caches like Redis to improve performance.

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

## 13. Returning Big Payloads Without Compression

**Mistake:** Large uncompressed responses consume more bandwidth and can slow down data transfer rates.

**Solution:** Compressing your responses with Brotli or GZIP can significantly reduce payload size. Smaller responses mean faster data transfer and a better user experience.

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

## 14. Fat Controllers

**Mistake:** Overloaded controllers with too much logic and methods become hard to maintain and test.

```csharp
public class ProductsController(
    IProductRepository productRepository,
    ILoggingService loggingService,
    ICacheService cacheService,
    IEmailService emailService,
    IAuthenticationService authService,
    IReportGenerator reportGenerator,
    IFeatureFlagService featureFlagService
) : ControllerBase
{
    public IActionResult GetAllProducts() { }
    public IActionResult GetProductById(int id) { }
    public IActionResult CreateProduct() { }
    public IActionResult UpdateProduct(int id) { }
    public IActionResult DeleteProduct(int id) { }
    public IActionResult GetProductsByCategory(string category) { }
    public IActionResult ExportProducts()  { }
    public IActionResult SendProductNewsletter()  { }
    public IActionResult GetProductStats()  { }
    public IActionResult GetProductRecommendations() { }
}
```

**Solution:** Apply the Single Responsibility Principle by moving business logic into services or other layers. Break big controllers to smaller ones - keeping them focused on one thing or entity.

```csharp
// For example, break them into specialized controllers or Minimal APIs
// ProductController - focuses on product entity
public record Product(int Id, string Name);

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAllProducts() 
    {
        // ...
    }

    [HttpGet("{id}")]
    public IActionResult GetProductById(int id) 
    {
        // ...
    }

    // ...
}
```

## 15. Not Using Minimal APIs

**Mistake:** Controllers can quickly become out of order with too much logic inside.

**Solution:** Replace Controllers with Minimal APIs or Fast Endpoints creating a new class per endpoint. This makes endpoints single responsible and not coupled with other endpoints.

Minimal APIs in .NET 9 received a huge performance boost and can process **15%** more requests per second than in .NET 8. Also, Minimal APIs consume **93%** less memory compared to a previous version.

```csharp
app.MapGet("/api/v1/products", async (IProductService service) =>
{
    var products = await service.GetProducts();
    return Results.Ok(products);
});

app.MapGet("/api/orders", async (IOrder service) =>
{
    var orders = await service.GetOrders();
    return Results.Ok(orders);
});
```

## Summary

By avoiding these 15 common pitfalls - your .NET Web APIs will be more reliable, secure, and scalable.

*   Keep your endpoints versioned for backward compatibility.
*   Offer meaningful error messages.
*   Secure your APIs with authentication and authorization.
*   Use async/await for better scalability and performance.
*   Stick to RESTful conventions.
*   Validate input, be pessimistic about data you receive.
*   Follow security best practices.
*   Log and monitor your applications.
*   Document APIs with Swagger/OpenAPI.
*   Optimize your data access.
*   Use paging/filtering/sorting for large datasets.
*   Cache often-used data.
*   Compress large responses.
*   Keep controllers small and lean.
*   Use Minimal APIs for speed and simplicity

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Ftop-15-mistakes-developers-make-when-creating-web-apis&title=TOP%2015%20Mistakes%20Developers%20Make%20When%20Creating%20Web%20APIs)[X](https://twitter.com/intent/tweet?text=TOP%2015%20Mistakes%20Developers%20Make%20When%20Creating%20Web%20APIs&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Ftop-15-mistakes-developers-make-when-creating-web-apis)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Ftop-15-mistakes-developers-make-when-creating-web-apis)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.