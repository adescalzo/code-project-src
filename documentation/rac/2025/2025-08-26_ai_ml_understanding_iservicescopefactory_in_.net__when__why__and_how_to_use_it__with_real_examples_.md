```yaml
---
title: "Understanding IServiceScopeFactory in .NET: When, Why, and How to Use It (With Real Examples)"
source: https://www.c-sharpcorner.com/article/understanding-iservicescopefactory-in-net-when-why-and-how-to-use-it-with-r/?utm_source=dotnetnews.beehiiv.com&utm_medium=newsletter&utm_campaign=the-net-news-daily-issue-277
date_published: 2025-08-27T00:00:00.000Z
date_captured: 2025-09-04T11:42:39.542Z
domain: www.c-sharpcorner.com
author: Jay Krishna Reddy
category: ai_ml
technologies: [.NET, ASP.NET Core, MyDbContext, BackgroundService, IServiceScopeFactory, IServiceScope, IServiceProvider, DbContext, Controller, Razor Pages, Task, CancellationToken, TimeSpan]
programming_languages: [C#, SQL]
tags: [dependency-injection, dotnet, aspnet-core, service-lifetimes, background-services, hosted-services, di-container, scope-management, csharp, web-development]
key_concepts: [Dependency Injection, Service Lifetimes, Scoped Services, Singleton Services, Transient Services, IServiceScopeFactory, IServiceScope, IServiceProvider, Background Tasks, Memory Leaks, Anti-patterns]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to `IServiceScopeFactory` in .NET, explaining its purpose, usage, and common pitfalls. It highlights `IServiceScopeFactory` as a crucial tool for managing scoped services outside the typical HTTP request pipeline, particularly within singleton background services or message processors. The author demonstrates how to safely create new service scopes to resolve scoped dependencies, preventing lifetime mismatches and potential memory leaks. Practical examples illustrate correct implementation in hosted services and queue-based processing, alongside warnings against its misuse in controllers or for long-lived scopes. The article emphasizes maintaining proper Dependency Injection lifetimes for cleaner and more maintainable code.
---
```

# Understanding IServiceScopeFactory in .NET: When, Why, and How to Use It (With Real Examples)

# Understanding IServiceScopeFactory in .NET: When, Why, and How to Use It (With Real Examples)

In modern .NET applications, especially in ASP.NET Core and background services, we rely heavily on dependency injection (DI) to manage object lifetimes and dependencies. While singleton and transient services are easy to understand, things become nuanced when it comes to scoped services, especially outside of the request pipeline.

This is where IServiceScopeFactory becomes crucial.

This article breaks down:

*   What IServiceScopeFactory is
*   When and why to use it
*   Real-world examples
*   Pitfalls to avoid
*   When **not** to use it

## What is IServiceScopeFactory?

IServiceScopeFactory is an interface provided by the .NET DI container that allows you to create a new service scope manually.

When you call `CreateScope()`, it returns an `IServiceScope`, which has its own `IServiceProvider`. You can use this scoped provider to resolve **scoped services, which would otherwise be inaccessible** during a web request.

```csharp
public interface IServiceScopeFactory
{
    IServiceScope CreateScope();
}
```

## Why is it needed?

Typically, scoped services (like a `DbContext`) are created **per HTTP request**. But what if you’re in a background task or a singleton service and need to use a scoped service?

You can’t directly inject a scoped service into a singleton. That would break the lifetime rules and throw an exception (or worse, cause memory leaks).

That’s where IServiceScopeFactory shines. It allows you to **safely create a scope** and use scoped services, even from singletons or hosted services.

## When to Use IServiceScopeFactory?

Let’s look at practical scenarios.

### Using Scoped Services in Hosted Background Services

Let’s say you’re running a background task that syncs data every few minutes, and you need to access `MyDbContext`.

`MyDbContext` is registered as scoped, but `BackgroundService` is a singleton.

Correct way of using IServiceScopeFactory:

```csharp
public class DataSyncService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public DataSyncService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            using var scope = _scopeFactory.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();

            var records = await dbContext.Records.ToListAsync();
            // process records...

            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}
```

This safely creates a scope and ensures all scoped services are disposed of properly.

### Queue-based Processing (Worker Pattern)

Let’s say you’re queuing jobs and processing them using a singleton `MessageProcessor`. You need access to scoped services per message.

```csharp
public class MessageProcessor
{
    private readonly IServiceScopeFactory _scopeFactory;

    public MessageProcessor(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task ProcessMessageAsync(MyMessage message)
    {
        using var scope = _scopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<IMessageHandler>();
        await handler.HandleAsync(message);
    }
}
```

This ensures every message gets its own fresh scope.

### On-Demand Scoped Dependencies in Console Apps or Middleware

In a console app or custom middleware where there’s no implicit scope, you can create one manually.

```csharp
var serviceScopeFactory = serviceProvider.GetRequiredService<IServiceScopeFactory>();
using var scope = serviceScopeFactory.CreateScope();

var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
```

## When Not to Use IServiceScopeFactory?

While it’s powerful, misuse can create hard-to-debug memory leaks or broken DI lifetimes.

### 1. Don’t Use IServiceScopeFactory Inside Controllers

Controllers and Razor Pages already operate within a scope—no need to create another.

**Bad Example**

```csharp
public class HomeController : Controller
{
    private readonly IServiceScopeFactory _scopeFactory;

    public HomeController(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;

    public IActionResult Index()
    {
        using var scope = _scopeFactory.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        // Not needed!
    }
}
```

**Good example**

```csharp
public class HomeController : Controller
{
    private readonly MyDbContext _db;

    public HomeController(MyDbContext db) => _db = db;

    public IActionResult Index()
    {
        var data = _db.Records.ToList();
        return View(data);
    }
}
```

### 2. Don’t Keep a Scope Alive Too Long

A scope is meant to be short-lived (per request or operation). Don’t hold onto a scope and reuse it across multiple operations.

This can lead to:

*   Memory leaks
*   Database connection leaks
*   Incorrect disposal of scoped services

### Key Benefits of IServiceScopeFactory

*   Enables the use of scoped services in singleton/background services
*   Ensures services are disposed of properly
*   Helps maintain correct DI lifetimes
*   Prevents common anti-patterns like injecting scoped into a singleton

### Alternative: Use IServiceProvider.CreateScope()

If you’re in `Program.cs` or somewhere you already have access to `IServiceProvider`, you can call:

```csharp
using var scope = app.Services.CreateScope();
var myService = scope.ServiceProvider.GetRequiredService<IMyService>();
```

But in long-lived services, it’s better to inject `IServiceScopeFactory` to avoid capturing the root `IServiceProvider`.

If you’re building long-running applications or background services in .NET, mastering `IServiceScopeFactory` will make your code cleaner, safer, and more maintainable.

Thank you for reading. Please share your questions, thoughts, or feedback in the comments section. I appreciate your feedback and encouragement.

_Keep learning…! 😊_