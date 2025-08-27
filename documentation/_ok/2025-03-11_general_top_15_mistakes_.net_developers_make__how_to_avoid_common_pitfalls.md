```yaml
---
title: "Top 15 Mistakes .NET Developers Make: How to Avoid Common Pitfalls"
source: https://antondevtips.com/blog/top-15-mistakes-dotnet-developers-make-how-to-avoid-common-pitfalls
date_published: 2025-03-11T08:45:06.708Z
date_captured: 2025-08-06T17:36:14.719Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [.NET, ASP.NET Core, Entity Framework Core, Serilog, NLog, OpenTelemetry, FluentValidation, DataAnnotations, xUnit, NUnit, TUnit, AutoMapper, "Nutrient's PDF SDK", Neon, Seq, MongoDB, MassTransit, RabbitMQ]
programming_languages: [C#, SQL]
tags: [dotnet, aspnet-core, best-practices, architecture, performance, testing, logging, data-access, web-api, clean-code]
key_concepts: [dependency-injection, input-validation, structured-logging, distributed-tracing, orm, async-await, automated-testing, architectural-patterns]
code_examples: false
difficulty_level: intermediate
summary: |
  This article identifies the top 15 common mistakes made by .NET developers and provides actionable solutions to avoid them. It covers essential practices such as implementing Dependency Injection, robust input validation, and comprehensive logging with OpenTelemetry. The content also emphasizes efficient resource management, leveraging asynchronous programming, and the critical role of automated testing. Furthermore, it delves into architectural considerations, advocating for organizing code by features, avoiding over-engineering, and exploring modern patterns like Vertical Slices and Clean Architecture, alongside the adoption of Minimal APIs and adherence to clean code principles.
---
```

# Top 15 Mistakes .NET Developers Make: How to Avoid Common Pitfalls

![A dark blue and purple banner image with the text "TOP 15 MISTAKES DOTNET DEVELOPERS MAKE: HOW TO AVOID COMMON PITFALLS" and "dev tips" logo.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fdotnet%2Fcover_dotnet_top_15_mistakes.png&w=3840&q=100)

# Top 15 Mistakes .NET Developers Make: How to Avoid Common Pitfalls

Mar 11, 2025

8 min read

### Newsletter Sponsors

Don't let a wrong PDF SDK slow you down — choose [Nutrient's PDF SDK](https://www.nutrient.io/sdk?utm_campaign=newsletter3-2025-03-11&utm_source=anton-dev-tips&utm_medium=sponsoring) for instant rendering, seamless annotations, and real-time collaboration. Trusted by over 10K developers and 5000+ companies like IBM, SAP, DocuSign, and Disney, it's time to experience faster, more stable document handling. [Try it now](https://www.nutrient.io/sdk?utm_campaign=newsletter3-2025-03-11&utm_source=anton-dev-tips&utm_medium=sponsoring).

With Neon's Dev/Test environments, .NET developers can experiment freely without fear of breaking production — spin up isolated database branches instantly, test changes, and reset with ease. No more hesitation — move fast and innovate with confidence! [Try Neon for free](https://neon.tech/signup/?refcode=44WD03UH).

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

During 11 years of my professional experience, I have faced a lot of issues and pitfalls. I made mistakes, learned from them and became better as a developer.

Today I want to share with you the **top 15 mistakes** .NET developers make in their projects. Even experience developers can fall into few traps.

So you don't have to learn yourself a hard way. As these mistakes can lead to inefficient, hard-to-maintain, or buggy code.

We will explore mistakes and actionable solutions. Whether you're a beginner or a senior developer, these insights will help you write cleaner, more maintainable, and robust applications.

[](#1-not-using-dependency-injection)

## 1\. Not Using Dependency Injection

**Problem:**

Hardcoding dependencies within your classes lead to tight coupling, making your code difficult to test, extend, and maintain.

When dependencies are directly instantiated within your classes, you lose the flexibility to change them as your project evolves or substitute implementations during testing. This tight coupling makes unit testing nearly impossible and the code less resilient to change.

```csharp
[ApiController]
[Route("api/products")]
public class ProductController : ControllerBase
{
    private readonly ProductRepository _repository;

    public ProductController()
    {
        // No dependency injection: Instantiating the repository directly
        _repository = new ProductRepository();
    }

    [HttpGet]
    public IEnumerable<Product> Get()
    {
        return _repository.GetAllProducts();
    }
}
```

**Solution:**

Use Microsoft's built-in DI (Dependency Injection) container to register services as Transient, Scoped, or Singleton based on their usage.

For better testability, it is recommended to use interfaces for the dependencies:

```csharp
public interface IProductRepository
{
    IEnumerable<Product> GetAllProducts();
}

public class ProductRepository : IProductRepository
{ }

[ApiController]
[Route("api/products]")]
public class ProductController : ControllerBase
{
    private readonly IProductRepository _repository;

    // Constructor injection through DI
    public ProductController(IProductRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    public IEnumerable<Product> Get()
    {
        return _repository.GetAllProducts();
    }
}
```

And register them in the DI container:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register the repository as a Scoped service
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Register the controllers
builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();
app.Run();
```

[](#2-not-using-validation)

## 2\. Not Using Validation

**Problem:**

Skipping input validation can lead to runtime errors, security vulnerabilities, or inconsistent data within your application.

Without proper validation, malicious or incorrect data may pass through your application, causing issues such as SQL injection, data corruption, or unexpected behavior.

**Solution:**

Use `FluentValidation` Nuget package or `DataAnnotations` to validate all the input data. Always be pessimistic about data your APIs receive.

Here is an example of using DataAnnotations attributes for validation:

```csharp
using System.ComponentModel.DataAnnotations;

public class CreateProductRequest
{
    [Required(ErrorMessage = "Product name is required.")]
    [StringLength(100, ErrorMessage = "Product name cannot exceed 100 characters.")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Price is required.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0.")]
    public decimal Price { get; set; }

    [Range(0, int.MaxValue, ErrorMessage = "Stock count must be 0 or greater.")]
    public int Stock { get; set; }
}
```

Here is how you can validate `CreateProductRequest` model using FluentValidation package:

```csharp
using FluentValidation;

public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
    public CreateProductRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required.")
            .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.");

        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Price must be greater than 0.");

        RuleFor(x => x.Stock)
            .GreaterThanOrEqualTo(0).WithMessage("Stock count must be 0 or greater.");
    }
}
```
```csharp
app.MapPost("/api/product",async (
    CreateProductRequest request,
    IValidator<CreateProductRequest> validator
) =>
{
    var validationResult = await validator.ValidateAsync(request);

    if (!validationResult.IsValid)
    {
        return Results.BadRequest(...);
    }

    return Results.Ok("Product created successfully.");
});
```

Personally, I prefer using FluentValidation as it is much more flexible and decouples validation logic from the models, thus following Single Responsibility principle.

[](#3-not-using-logging-and-opentelemetry)

## 3\. Not Using Logging and OpenTelemetry

**Problem:**

Without structured logging and observability, diagnosing issues and understanding application performance becomes extremely challenging.

Without a proper logging and monitoring setup, you'll struggle to understand why something went wrong or how your application behaves under different loads. This is especially true in distributed systems.

**Solution:**

*   **Logging Frameworks:** Use logging frameworks like Serilog or NLog.
*   **Observability:** Add OpenTelemetry to capture logs, traces and metrics.

![A screenshot of a distributed tracing tool (likely Seq) showing a timeline of service calls and database operations, illustrating distributed tracing and logging.](https://antondevtips.com/media/code_screenshots/architecture/seq-logging-traces/img_aspnet_seq_4.png)

Here is a brief example on how to add `Serilog` and `OpenTelemetry` to your project:

```csharp
var builder = WebApplication.CreateBuilder(args);

 // Use Serilog for logging
builder.Host.UseSerilog();

// Configure OpenTelemetry Tracing:
builder.Services.AddOpenTelemetryTracing(tracerProviderBuilder =>
{
    tracerProviderBuilder
        .AddAspNetCoreInstrumentation()
        .AddHttpClientInstrumentation()
        .AddOtlpExporter();
});
```

For a more detailed guide - check my [blog post](https://antondevtips.com/blog/how-to-implement-structured-logging-and-distributed-tracing-for-microservices-with-seq?utm_source=linkedin&utm_medium=social&utm_campaign=website) where I showed how to implement structured logging and distributed tracing for microservices.

[](#4-reinventing-the-wheel-not-using-libraries)

## 4\. Reinventing the Wheel (Not Using Libraries)

**Problem:**

Building common functionality from scratch wastes time and can introduce new bugs that have already been solved by the community. When you write custom implementations for problems that already have robust solutions, you not only waste development time but also risk introducing bugs or missing out on performance optimizations.

**Solution:**

Prefer using well-known, community-tested libraries and NuGet packages. Focus on unique business logic rather than re-creating what's already solved.

[See this list of 16 libraries](https://www.linkedin.com/posts/anton-martyniuk_dotnet-aspnetcore-efcore-activity-7283022261601665024-ttm6/?utm_source=share&utm_medium=member_desktop) I have personally used, and I recommend for efficient backend development in .NET.

[](#5-ignoring-ef-core)

## 5\. Ignoring EF Core

**Problem:**

Treating EF Core as a black box can lead to unoptimized queries, the N+1 problem, and overall poor performance.

Entity Framework Core is a powerful ORM, but without understanding how it converts LINQ queries to SQL, you might write inefficient queries that lead to performance issues.

**Solution:**

Learn EF Core's features and query optimization techniques. Understand how EF translates LINQ to SQL. Using EF Core is more efficient in terms of development speed than writing manual SQL queries.

To become better in EF Core, I recommend checking out my 2 articles:

*   [Increase EF Core Performance for Read Queries](https://antondevtips.com/blog/how-to-increase-ef-core-performance-for-read-queries-in-dotnet?utm_source=linkedin&utm_medium=social&utm_campaign=website)
*   [Avoid Top 10 Mistakes in EF Core](https://antondevtips.com/blog/top-10-mistakes-developers-make-in-ef-core?utm_source=linkedin&utm_medium=social&utm_campaign=website)

[](#6-not-disposing-resources)

## 6\. Not Disposing Resources

**Problem:**

Not disposing resources for IDisposable objects leads to memory and handle leaks.

Many objects (like database connections, file streams, etc.) implement IDisposable and require explicit cleanup. Ignoring this can result in resource exhaustion and application crashes.

```csharp
public async Task<List<Product>> GetProductsByPriceAsync(decimal price)
{
    // DbContext is created but never disposed. Memory leak! Bad code!
    var dbContext = _dbContextFactory.CreateDbContext();

    return await dbContext.Products.Where(p => p.Price > price).ToListAsync();
}
```

**Solution:**

Use using statements, await using, and the Dispose() pattern consistently to ensure proper resource cleanup.

```csharp
public async Task<List<Product>> GetProductsByPriceAsync(decimal price)
{
    // Properly dispose the DbContext using a 'using' block
    using var dbContext = _dbContextFactory.CreateDbContext();

    return await dbContext.Products.Where(p => p.Price > price).ToListAsync();
}
```

[](#7-ignoring-asyncawait)

## 7\. Ignoring async/await

**Problem:**

Blocking threads with synchronous I/O operations decreases application scalability and throughput.

```csharp
[HttpGet("items")]
public IActionResult GetItems()
{
    // Using synchronous code can lead to bottlenecks
    var items = _itemService.GetItems();
    return Ok(items);
}
```

**Solution:**

Use an async/await pattern that enables non-blocking operations, thus reducing thread starvation, improving scalability and responsiveness.

```csharp
[HttpGet("items")]
public async Task<IActionResult> GetItems()
{
    var items = await _itemService.GetItemsAsync();
    return Ok(items);
}
```

[](#8-not-writing-tests)

## 8\. Not Writing Tests

**Problem:**

Without automated tests, regressions go unnoticed and bugs slip into production.

Testing ensures that your code behaves as expected and provides a safety net for refactoring. Without tests, even small changes can introduce bugs that are difficult to diagnose.

**Solution:**

Use xUnit/NUnit/TUnit and write tests early. Write unit and integration tests. Remember that testable code is much more maintainable. Time spent on writing tests is usually smaller than time spent fixing bugs and resolving maintenance issues in the future.

For testing, I recommend checking [ASP.NET Core Integration Testing Best Practises](https://antondevtips.com/blog/asp-net-core-integration-testing-best-practises?utm_source=linkedin&utm_medium=social&utm_campaign=website) article.

You can find a list of my articles about testing in .NET - [here](https://antondevtips.com/blog?categoryType=tests&utm_source=linkedin&utm_medium=social&utm_campaign=website).

[](#9-using-mapping-libraries)

## 9\. Using Mapping Libraries

**Problem:**

Many believe AutoMapper speeds up development. I find using Mapping Libraries as an uncontrolled evil force that is a hidden source of bugs and complexity.

While libraries such as AutoMapper reduce boilerplate code, they perform mapping dynamically, which may mask mapping issues until runtime.

**Solution:**

Prefer using manual mapping; although more verbose, with AI tools nowadays it doesn't take much time to write the mapping code.

Manual mapping has the following advantages:

*   **Easy Navigation.** You don't need to search for mapping classes in the entire solution to understand how the libraries do the mapping magic.
*   **You have code safety**. If you forget to update the mapping method - a compiler error is raised.
*   **You have entire control over the mapping process**, you don't need to spend time learning how to do the fancy mapping stuff in the libraries.
*   **Better performance.**
*   **Straightforward Debugging.** This is really hard or almost impossible to debug an AutoMapper profile. Forget about this problem and have the stress-less debugging.

When using the **required** keyword in C#, you can achieve compile time safety when making mapping manually:

![A C# code snippet showing a manual mapping extension method with a compiler error highlighting the `required` keyword for compile-time safety in object initialization.](https://antondevtips.com/media/code_screenshots/aspnetcore/best-mapping/img_aspnet_mapping_1.png)

[](#10-using-exceptions-for-control-flow)

## 10\. Using Exceptions for Control Flow

**Problem:**

Using exceptions for control flow slows down performance and complicates the control flow

Exceptions should signal only exceptional conditions — not normal or business operations. Relying on exceptions for flow control creates overhead and makes your code unpredictable.

```csharp
public interface IShipmentService
{
    Task<CreateShipmentResponse> CreateAsync(CreateShipmentRequest request);
}

public void MapEndpoint(WebApplication app)
{
    app.MapPost("/api/v1/shipments", Handle);
}

private static async Task<IResult> Handle(
    [FromBody] CreateShipmentRequest request,
    IShipmentService service,
    CancellationToken cancellationToken)
{
    try
    {
        var response = await service.CreateAsync(request, cancellationToken);
        return Results.Ok(response);
    }
    catch (ShipmentAlreadyExistsException ex)
    {
        return Results.Conflict(new { message = ex.Message });
    }
}
```

From the service interface, it is not obvious that it can throw exceptions. Often it is not documented.

**Solution:**

Use exceptions only for exceptional situations, guard clauses, and libraries. Use Result Pattern when you need to analyze a result.

```csharp
public interface IShipmentService
{
    Task<Result<CreateShipmentResponse>> CreateAsync(CreateShipmentRequest request);
}

public void MapEndpoint(WebApplication app)
{
    app.MapPost("/api/v1/shipments", Handle);
}

private static async Task<IResult> Handle(
    [FromBody] CreateShipmentRequest request,
    IShipmentService service,
    CancellationToken cancellationToken)
{
    var command = request.MapToCommand();
    var response = await service.CreateAsync(command, cancellationToken);
    
    return response.IsSuccess ? Results.Ok(response.Value) : Results.Conflict(response.Error);
}
```

For more in-depth guide on using Result Pattern, check out my [article](https://antondevtips.com/blog/how-to-replace-exceptions-with-result-pattern-in-dotnet?utm_source=linkedin&utm_medium=social&utm_campaign=website).

And remember to include `GlobalExceptionHandler` in your ASP.NET Core applications.

[](#11-organizing-code-by-technical-folders)

## 11\. Organizing Code by Technical Folders

**Problem:** Grouping files solely by their technical roles (e.g., Controllers, Models, Services) can make code navigation harder and slow feature development.

When code is organized strictly by technology, developers may struggle to understand how pieces of functionality interact. This organization can increase the cognitive load during debugging or when adding new features.

When a project is structured by technical folder names, developers jump back and forward across the solution to implement one features that starts from webapi request down to the database.

**Solution:**

Organize code by business features or domains. This improves code navigation and increases development speed. Also when structuring folders by features, your solution structure "screams" about project's features.

Just by looking at this screenshot, you can understand what is the functionality of the web application:

![A file explorer view showing a project structure organized by business features (e.g., Shipments, with subfolders for CancelShipment, CreateShipment, etc.), demonstrating Vertical Slice Architecture.](https://antondevtips.com/media/code_screenshots/architecture/clean-architecture-with-vertical-slices/ca_with_vsa_3.png)

[](#12-overengineering-solutions)

## 12\. Over-Engineering Solutions

**Problem:**

Adding unnecessary layers, abstractions, and patterns creates complexity and slows development.

While designing for scalability is important, adding abstractions and patterns before they are needed (or in excess) can make your code harder to read and maintain. Simplicity is key.

**Solution:**

Start Simple: Implement only the necessary layers and add complexity only when required.

For example, EF Core already implements Repository and Unit of Work patterns. You can use it directly in your application use cases.

If you have code duplication of complex EF Core queries - you can then extract it into a repository. But you can also avoid code duplication in other ways.

[](#13-using-only-traditional-nlayered-architecture-for-code-structure)

## 13\. Using Only Traditional N-Layered Architecture for Code Structure

**Problem:**

Controller-Service-Repository layers often result in boilerplate-heavy and less maintainable code. Such code isn't optimal for all scenarios.

While layering your application can be beneficial, sticking to a traditional approach in every situation may lead to unnecessary complexity. Modern architectural patterns like Vertical Slices or Clean Architecture can provide more focused and maintainable solutions.

**Solution:**

Consider using other architectures for code design. Adopt Domain-Driven Design (DDD), Vertical slices, or Clean Architecture patterns to improve your code architecture.

Learn more in my articles:

*   [Structuring Your Project with Vertical Slice Architecture](https://antondevtips.com/blog/vertical-slice-architecture-the-best-ways-to-structure-your-project?utm_source=linkedin&utm_medium=social&utm_campaign=website).
*   [Using Vertical Slice with Clean Architecture](https://antondevtips.com/blog/the-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices?utm_source=linkedin&utm_medium=social&utm_campaign=website).

[](#14-ignoring-minimal-apis)

## 14\. Ignoring Minimal APIs

**Problem:**

Relying solely on controllers and traditional MVC patterns can result in more complex code and less maintainable solutions.

**Solution:**

Adopt minimal APIs in .NET for simpler, more concise endpoint definitions that improve the development experience. Also, Minimal APIs (especially) in .NET 9 are much faster.

You can read about Minimal APIs performance more [here](https://www.linkedin.com/posts/anton-martyniuk_csharp-dotnet-aspnetcore-activity-7264178028262436864-r0t_/?utm_source=share&utm_medium=member_desktop).

[](#15-ignoring-clean-and-good-code)

## 15\. Ignoring Clean and Good Code

**Problem:**

Spaghetti code, unclear naming, and inconsistent formatting slow development and increase the risk of bugs.

**Solution:**

Maintaining clean code is fundamental for long-term project success. Clear naming, consistent formatting, and adherence to principles like SOLID, DRY, and KISS help ensure that code is readable, maintainable, and less error-prone.

To learn how to write clean and better code, check my [article](https://antondevtips.com/blog/how-to-write-better-and-cleaner-code-in-dotnet?utm_source=linkedin&utm_medium=social&utm_campaign=website).

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Ftop-15-mistakes-dotnet-developers-make-how-to-avoid-common-pitfalls&title=Top%2015%20Mistakes%20.NET%20Developers%20Make%3A%20How%20to%20Avoid%20Common%20Pitfalls)[X](https://twitter.com/intent/tweet?text=Top%2015%20Mistakes%20.NET%20Developers%20Make%3A%20How%20to%20Avoid%20Common%20Pitfalls&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Ftop-15-mistakes-dotnet-developers-make-how-to-avoid-common-pitfalls)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Ftop-15-mistakes-dotnet-developers-make-how-to-avoid-common-pitfalls)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.