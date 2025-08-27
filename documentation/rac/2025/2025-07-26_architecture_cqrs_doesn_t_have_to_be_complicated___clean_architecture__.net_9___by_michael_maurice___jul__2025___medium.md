```yaml
---
title: "CQRS Doesn’t Have To Be Complicated | Clean Architecture, .NET 9 | by Michael Maurice | Jul, 2025 | Medium"
source: https://medium.com/@michaelmaurice410/cqrs-doesnt-have-to-be-complicated-clean-architecture-net-9-8865a063e0c4
date_published: 2025-07-26T17:02:37.620Z
date_captured: 2025-08-22T11:01:56.780Z
domain: medium.com
author: Michael Maurice
category: architecture
technologies: [.NET 9, ASP.NET Core, MediatR, System.Text.Json, XUnit, Moq, LiteBus, OpenMediator, LINQ]
programming_languages: [C#, SQL]
tags: [cqrs, clean-architecture, dotnet, design-patterns, web-api, software-architecture, dependency-injection, testing, scalability, performance]
key_concepts: [Command Query Responsibility Segregation, Command Query Separation, Clean Architecture, Dependency Injection, Repository Pattern, Unit of Work, Event Sourcing, Native AOT]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article demystifies CQRS, demonstrating its practical implementation in .NET 9 using Clean Architecture principles without relying on external libraries like MediatR. It emphasizes separating read (queries) and write (commands) operations to achieve clearer separation of concerns, independent optimization, and enhanced testability. The content provides detailed C# code examples for defining abstractions, creating commands/queries, implementing handlers, and integrating with ASP.NET Core controllers. It also discusses the benefits of .NET 9 features like performance improvements and Native AOT, alongside common pitfalls and an evolution path for scaling CQRS. The author advocates for starting with a simple approach and evolving the architecture as business needs grow.]
---
```

# CQRS Doesn’t Have To Be Complicated | Clean Architecture, .NET 9 | by Michael Maurice | Jul, 2025 | Medium

# CQRS Doesn’t Have To Be Complicated | Clean Architecture, .NET 9

![CQRS Pattern in .NET 9 with Clean Architecture Diagram](https://miro.medium.com/v2/resize:fit:700/1*UkwCSBXi4zI8CDYVbXI7rA.png)
*Image Description: A diagram titled "CQRS PATTERN IN .NET 9 WITH CLEAN ARCHITECTURE". It illustrates a layered architecture with components like "Command", "Handler", "Domain", "Application", "Infrastructure", and "Presentation". On the right, two database icons are labeled "DEPENDENCY INJECTION" and "SCALABLE ARCHITECTURE", with a ".NET" logo at the bottom right. The diagram visually represents the flow of commands through different architectural layers, emphasizing the role of dependency injection and the potential for scalable architecture.*

If you want the full source code, download it from this link: [https://www.elitesolutions.shop/](https://www.elitesolutions.shop/)

CQRS (Command Query Responsibility Segregation) has earned a reputation for being complex, intimidating, and reserved only for enterprise-scale applications. But here’s the truth: CQRS doesn’t have to be complicated. In fact, when implemented thoughtfully in .NET 9 with Clean Architecture principles, it can be one of the most elegant and maintainable patterns in your developer toolkit.

# The CQRS Misconception

Many developers encounter CQRS in the context of event sourcing, separate databases, eventual consistency, and complex messaging systems. This association has created a false narrative that CQRS inherently requires architectural complexity. The reality is much simpler: CQRS is simply about separating your read operations (queries) from your write operations (commands).

At its core, CQRS is the architectural evolution of the Command Query Separation (CQS) principle, which states that every method should either be a command that performs an action or a query that returns data, but never both. CQRS takes this concept level.

# Simple CQRS in .NET 9

Let’s start with the simplest possible CQRS implementation. You don’t need MediatR, separate databases, or event sourcing. Here’s how to implement CQRS with just clean C# 13 code in .NET 9:

# Define Your Abstractions

```csharp
namespace CleanApp.Application.Abstractions;  
public interface ICommand<TResult>  
{  
}  
public interface ICommand : ICommand<Unit>  
{  
}  
public interface IQuery<TResult>  
{  
}  
public interface ICommandHandler<TCommand, TResult>   
    where TCommand : ICommand<TResult>  
{  
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);  
}  
public interface IQueryHandler<TQuery, TResult>   
    where TQuery : IQuery<TResult>  
{  
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);  
}  
public readonly record struct Unit;
```

# Create Specific Commands and Queries

```csharp
// Command for creating a blog post  
public sealed record CreateBlogPostCommand(    string Title,  
    string Content,  
    string[] Tags) : ICommand<Guid>;  
  
// Query for getting blog post details  
public sealed record GetBlogPostQuery(Guid Id) : IQuery<BlogPostResponse>;  
public sealed record BlogPostResponse(    Guid Id,  
    string Title,  
    string Content,  
    string[] Tags,  
    DateTime CreatedAt);
```

# Implement the Handlers

```csharp
public sealed class CreateBlogPostHandler   
    : ICommandHandler<CreateBlogPostCommand, Guid>  
{  
    private readonly IBlogRepository _repository;  
    private readonly IUnitOfWork _unitOfWork;  
public CreateBlogPostHandler(IBlogRepository repository, IUnitOfWork unitOfWork)  
    {  
        _repository = repository;  
        _unitOfWork = unitOfWork;  
    }  
    public async Task<Guid> HandleAsync(        CreateBlogPostCommand command,   
        CancellationToken cancellationToken = default)  
    {  
        var blogPost = BlogPost.Create(command.Title, command.Content, command.Tags);  
          
        await _repository.AddAsync(blogPost, cancellationToken);  
        await _unitOfWork.SaveChangesAsync(cancellationToken);  
          
        return blogPost.Id;  
    }  
}  
public sealed class GetBlogPostHandler   
    : IQueryHandler<GetBlogPostQuery, BlogPostResponse>  
{  
    private readonly IQueryDbContext _queryContext;  
    public GetBlogPostHandler(IQueryDbContext queryContext)  
    {  
        _queryContext = queryContext;  
    }  
    public async Task<BlogPostResponse> HandleAsync(        GetBlogPostQuery query,   
        CancellationToken cancellationToken = default)  
    {  
        var blogPost = await _queryContext.BlogPosts  
            .Where(bp => bp.Id == query.Id)  
            .Select(bp => new BlogPostResponse(  
                bp.Id,  
                bp.Title,  
                bp.Content,  
                bp.Tags,  
                bp.CreatedAt))  
            .FirstOrDefaultAsync(cancellationToken);  
        return blogPost ?? throw new BlogPostNotFoundException(query.Id);  
    }  
}
```

# Register With Dependency Injection

```csharp
// Program.cs in .NET 9  
var builder = WebApplication.CreateBuilder(args);  
// Register handlers  
builder.Services.AddScoped<ICommandHandler<CreateBlogPostCommand, Guid>, CreateBlogPostHandler>();  
builder.Services.AddScoped<IQueryHandler<GetBlogPostQuery, BlogPostResponse>, GetBlogPostHandler>();  
var app = builder.Build();
```

# Use in Controllers

```csharp
[ApiController]  
[Route("api/[controller]")]  
public class BlogPostsController : ControllerBase  
{  
    private readonly ICommandHandler<CreateBlogPostCommand, Guid> _createHandler;  
    private readonly IQueryHandler<GetBlogPostQuery, BlogPostResponse> _getHandler;  
  
public BlogPostsController(        ICommandHandler<CreateBlogPostCommand, Guid> createHandler,  
        IQueryHandler<GetBlogPostQuery, BlogPostResponse> getHandler)  
    {  
        _createHandler = createHandler;  
        _getHandler = getHandler;  
    }  
    [HttpPost]  
    public async Task<IActionResult> CreateBlogPost(CreateBlogPostRequest request)  
    {  
        var command = new CreateBlogPostCommand(request.Title, request.Content, request.Tags);  
        var id = await _createHandler.HandleAsync(command);  
        return CreatedAtAction(nameof(GetBlogPost), new { id }, id);  
    }  
    [HttpGet("{id:guid}")]  
    public async Task<IActionResult> GetBlogPost(Guid id)  
    {  
        var query = new GetBlogPostQuery(id);  
        var response = await _getHandler.HandleAsync(query);  
        return Ok(response);  
    }  
}
```

# Why This Simple Approach Works

This implementation gives you all the core benefits of CQRS without the complexity:

# Clear Separation of Concerns

Commands handle business logic and state changes, while queries focus purely on data retrieval. This separation makes your code easier to understand, test, and maintain.

# Independent Optimization

You can optimize commands for consistency and business logic validation, while optimizing queries for performance with techniques like projections, caching, or even read replicas.

# Scalability Foundation

While you start with a single database, this structure makes it easy to scale reads and writes independently later.

# Testability

Each handler has a single responsibility, making unit testing straightforward.

# Leveraging .NET 9 Features

.NET 9 brings several enhancements that make CQRS implementation even more efficient:

# Performance Improvements

.NET 9 includes over 1,000 performance improvements, with enhanced garbage collection, vectorization, and loop optimizations that benefit both command and query handlers.

# Native AOT Support

Commands and queries compile to native code with Native AOT, reducing startup time and memory usage — perfect for microservices and serverless deployments.

# Enhanced LINQ

New LINQ methods like `CountBy` and `AggregateBy` make query optimization more efficient, especially for reporting and analytics scenarios.

# Improved JSON Handling

System.Text.Json enhancements in .NET 9 provide better performance for API responses and command serialization.

# When to Keep It Simple vs. When to Scale

# Start Simple When:

*   Building CRUD-heavy applications
*   Working with small to medium teams
*   Performance requirements are moderate
*   Business logic is relatively straightforward

# Consider Advanced CQRS When:

*   Read and write loads are significantly different
*   You need different data models for reads and writes
*   Scalability requirements demand separate storage systems
*   Audit trails and event history are critical business requirements

# Common Pitfalls to Avoid

# Over-Engineering Early

Don’t start with event sourcing and separate databases unless you have a clear business need. The simple approach shown above can handle most scenarios effectively.

# Generic Repository Anti-Pattern

Avoid creating generic `IRepository<T>` interfaces for CQRS. Instead, create specific repositories for each aggregate root that align with your command boundaries.

# Premature Event Sourcing

Event sourcing adds significant complexity and should only be used when you need complete audit trails or temporal queries.

# Ignoring Command/Query Boundaries

Never let commands return domain data or let queries perform side effects. This violates the fundamental principle of CQS.

# Testing Your CQRS Implementation

```csharp
public class CreateBlogPostHandlerTests  
{  
    [Fact]  
    public async Task HandleAsync_ShouldCreateBlogPost_WhenValidCommand()  
    {  
        // Arrange  
        var mockRepository = new Mock<IBlogRepository>();  
        var mockUnitOfWork = new Mock<IUnitOfWork>();  
        var handler = new CreateBlogPostHandler(mockRepository.Object, mockUnitOfWork.Object);  
          
        var command = new CreateBlogPostCommand("Test Title", "Test Content", ["tech", "dotnet"]);  
  
// Act  
        var result = await handler.HandleAsync(command);  
        // Assert  
        result.Should().NotBe(Guid.Empty);  
        mockRepository.Verify(r => r.AddAsync(It.IsAny<BlogPost>(), It.IsAny<CancellationToken>()), Times.Once);  
        mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);  
    }  
}
```

# Evolution Path: From Simple to Sophisticated

The beauty of starting with simple CQRS is that it provides a clear evolution path:

1.  Phase 1: Single database, separate command/query handlers
2.  Phase 2: Add caching for queries
3.  Phase 3: Introduce read replicas for query scaling
4.  Phase 4: Separate read/write databases if needed
5.  Phase 5: Add event sourcing for audit requirements

Each phase builds upon the previous one without requiring a complete rewrite.

# Alternatives to MediatR

With MediatR going commercial, several alternatives have emerged:

# Manual Handler Registration

The approach shown above with explicit dependency injection registration.

# LiteBus

A lightweight, free alternative focused on CQS principles.

# OpenMediator

Another open-source option that maintains MediatR-like syntax.

# Custom Dispatcher

Building your own simple dispatcher for automatic handler resolution:

```csharp
public interface IDispatcher  
{  
    Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);  
    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);  
}  
  
public class Dispatcher : IDispatcher  
{  
    private readonly IServiceProvider _serviceProvider;  
    public Dispatcher(IServiceProvider serviceProvider)  
    {  
        _serviceProvider = serviceProvider;  
    }  
    public async Task<TResult> ExecuteAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)  
    {  
        var handlerType = typeof(ICommandHandler<,>).MakeGenericType(command.GetType(), typeof(TResult));  
        var handler = _serviceProvider.GetRequiredService(handlerType);  
        var method = handlerType.GetMethod(nameof(ICommandHandler<ICommand<TResult>, TResult>.HandleAsync));  
        return await (Task<TResult>)method!.Invoke(handler, [command, cancellationToken])!;  
    }  
    public async Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)  
    {  
        var handlerType = typeof(IQueryHandler<,>).MakeGenericType(query.GetType(), typeof(TResult));  
        var handler = _serviceProvider.GetRequiredService(handlerType);  
        var method = handlerType.GetMethod(nameof(IQueryHandler<IQuery<TResult>, TResult>.HandleAsync));  
        return await (Task<TResult>)method!.Invoke(handler, [query, cancellationToken])!;  
    }  
}
```

# Conclusion

CQRS doesn’t have to be the architectural monster that many developers fear. When implemented thoughtfully in .NET 9 with Clean Architecture principles, it becomes a powerful tool for building maintainable, scalable applications. Start simple, focus on separation of concerns, and evolve your architecture as your needs grow.

The key is understanding that CQRS is fundamentally about organizing your code around business use cases rather than technical concerns. By separating commands and queries, you create a codebase that’s easier to understand, test, and maintain — and that’s something every development team can benefit from, regardless of scale.

Remember: the best architecture is the one that solves your current problems without over-engineering for problems you don’t yet have. CQRS, when kept simple, provides exactly that balance.

**If you want the full source code, download it from this link**: [https://www.elitesolutions.shop/](https://www.elitesolutions.shop/)