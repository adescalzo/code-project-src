```yaml
---
title: "Decorator Pattern — C#. This is a structural design pattern… | by Tiago Martins | Medium"
source: https://medium.com/@martinstm/decorator-pattern-c-cbf4b7ccf474
date_published: 2025-04-15T09:53:44.100Z
date_captured: 2025-08-08T12:32:03.526Z
domain: medium.com
author: Tiago Martins
category: architecture
technologies: [.NET 8, ASP.NET Core, Memory Cache]
programming_languages: [C#]
tags: [decorator-pattern, design-patterns, csharp, dotnet, dependency-injection, software-architecture, extensibility, composition, caching, retry-policy]
key_concepts: [Decorator Pattern, Open/Closed Principle, Single Responsibility Principle, Dependency Injection, Composition over Inheritance, API design, Caching, Retry Policy]
code_examples: false
difficulty_level: intermediate
summary: |
  The article explains the Decorator Pattern in C#, a structural design pattern that dynamically adds behavior to objects without modifying their original code. It emphasizes its benefits for extensibility, adherence to SOLID principles, and practical applications like adding caching or retry logic to an existing service. The author uses a coffee shop analogy and provides detailed C# code examples, demonstrating implementation for an API's user service. It also covers various dependency injection strategies in ASP.NET Core to manage decorator chaining.
---
```

# Decorator Pattern — C#. This is a structural design pattern… | by Tiago Martins | Medium

# Decorator Pattern — C#

![C# Decorator Pattern Logo](https://miro.medium.com/v2/resize:fit:700/1*pUPh2U3zkwFwxBQf3DxBMg.png)

This is a structural design pattern used to dynamically add behavior to objects without modifying their original code. It’s a powerful way to offer more flexibility and promote composition over inheritance.
In summary, a **decorator wraps an existing object** and adds new functionality either **before** or **after** delegating a task to the original object.

# Why?

There are several reasons, the most important ones are:

*   New features around the original code without changing it
*   Create and define multiple behaviors at runtime
*   **Open/Closed principle** applied. (Open enough for extension, closed enough for modification)
*   **Single responsibility principle** applied. Each decorator has one specific concern.

# When?

There are some scenarios where a new feature is required to enhance the original code. However, nobody wants to change that original code and take the risk to change anything else. Also, the original code could be complex.
Working with third-party libraries we can’t modify the original class.

# A real-world analogy

Imagine yourself at your favorite coffee shop. You order a simple plain coffee — that’s your core drink, the base, just like a core service in your app.

But maybe you’re in the mood for something extra. So you ask for milk. Now it’s not just coffee, it’s **coffee with milk** — something like adding a decorator that enhances the original.

Still not fancy enough? Add a little sugar… or go wild with some whipped cream on top. Each add-on builds on the last, wrapping your drink with something new, without changing the original coffee itself.

That’s exactly how the decorator pattern works — it lets you stack features or behaviors without changing the main functionality.

# Let’s code!

There is an API that wants to return a list of all existing users. The new requirements ask for a new cache implementation and a retry policy rule to be applied around the existing implementation.

## The diagram of this solution

This is the target UML diagram to implement:

![UML Diagram of Decorator Pattern Implementation](https://miro.medium.com/v2/resize:fit:522/1*bG7tnhAaDYmW_pjW6oqxSw.png)

`IUserService` — The main interface that defines the `GetAll` method.

`UserService` — The main and original interface implementation.

`UserServiceDecorator` — Wraps the original implementation and acts as a centralized decorator to be extended.

`UserServiceCache` and `UserServiceRetry` — The new features to be added.

Next, let’s see the base decorator class.

```csharp
// IUserService.cs
public interface IUserService
{
    IEnumerable<User> GetAll();
}

// UserService.cs
public class UserService : IUserService
{
    public IEnumerable<User> GetAll()
    {
        // Simulate fetching users from a database
        Console.WriteLine("Fetching all users from database...");
        return new List<User>
        {
            new User { Id = 1, Name = "Alice" },
            new User { Id = 2, Name = "Bob" }
        };
    }
}

// UserServiceDecorator.cs
public abstract class UserServiceDecorator : IUserService
{
    protected readonly IUserService _userService;

    protected UserServiceDecorator(IUserService userService)
    {
        _userService = userService;
    }

    public virtual IEnumerable<User> GetAll()
    {
        return _userService.GetAll();
    }
}

// UserServiceCache.cs
public class UserServiceCache : UserServiceDecorator
{
    private readonly IMemoryCache _memoryCache;
    private const string CacheKey = "AllUsersCache";

    public UserServiceCache(IUserService userService, IMemoryCache memoryCache)
        : base(userService)
    {
        _memoryCache = memoryCache;
    }

    public override IEnumerable<User> GetAll()
    {
        if (_memoryCache.TryGetValue(CacheKey, out IEnumerable<User> users))
        {
            Console.WriteLine("Fetching users from cache.");
            return users;
        }

        Console.WriteLine("Fetching users from underlying service and caching.");
        users = base.GetAll();
        _memoryCache.Set(CacheKey, users, TimeSpan.FromMinutes(5)); // Cache for 5 minutes
        return users;
    }
}

// UserServiceRetry.cs
public class UserServiceRetry : UserServiceDecorator
{
    private readonly int _maxRetries;

    public UserServiceRetry(IUserService userService, int maxRetries = 3)
        : base(userService)
    {
        _maxRetries = maxRetries;
    }

    public override IEnumerable<User> GetAll()
    {
        int attempts = 0;
        while (attempts < _maxRetries)
        {
            try
            {
                Console.WriteLine($"Attempt {attempts + 1} to fetch users.");
                return base.GetAll();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching users: {ex.Message}. Retrying...");
                attempts++;
                if (attempts >= _maxRetries)
                {
                    throw; // Re-throw if max retries reached
                }
                Thread.Sleep(1000); // Wait 1 second before retrying
            }
        }
        return Enumerable.Empty<User>(); // Should not be reached
    }
}

// User.cs (Simple DTO)
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}
```

Now a simple example of both decorator's implementation.

Simple, right? The next question should be how to define the dependency injection for this and how to use it.

## Dependency Injection

The main idea is to create an extension method from `IServiceCollection` to be used in the API project startup.

```csharp
// ServiceCollectionExtensions.cs
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDecoratedUserService(this IServiceCollection services)
    {
        services.AddMemoryCache(); // Add memory cache service

        // Register the base implementation with a key
        services.AddKeyedScoped<IUserService, UserService>("baseUserService");

        // Register the chain of decorators
        services.AddScoped<IUserService>(sp =>
        {
            // Resolve the base service using its key
            var baseService = sp.GetRequiredKeyedService<IUserService>("baseUserService");

            // Apply the retry decorator
            var retryService = new UserServiceRetry(baseService);

            // Apply the cache decorator on top of the retry decorator
            var cacheService = new UserServiceCache(retryService, sp.GetRequiredService<IMemoryCache>());

            return cacheService;
        });

        return services;
    }
}
```

So let’s explain this step by step. First, add the memory cache since it’s necessary for the cache decorator. Next, define the main implementation of `IUserService` using the new feature introduced by .NET 8 `AddKeyedScoped` which allows us to register and resolve services by a specific key. Very useful in this chaining decorator example. The next step, adding the decorators. Once we get the “main” implementation is possible to just pass that class into the decorator creation. After that just need to add the next decorator, in this case, the cache one.

# An enhanced way to inject the decorators

As we saw, the example follows an approach I like to call “all-in-one”, where everything is injected within a single extension method. However, what if we need to provide some decision-making on the users who want these implementations? This means that we should have 4 options:

*   The original implementation without features
*   Use cache feature
*   Use retry feature
*   Use both

So let’s see how to handle the dependency injection to support all these options.

The basic option:

```csharp
// In Program.cs or Startup.cs
// Option 1: Original implementation only
builder.Services.AddScoped<IUserService, UserService>();
```

Now with the retry feature:

```csharp
// Option 2: With Retry feature
builder.Services.AddScoped<IUserService>(sp =>
{
    var baseService = new UserService();
    return new UserServiceRetry(baseService);
});
```

And now with the cache feature:

```csharp
// Option 3: With Cache feature
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IUserService>(sp =>
{
    var baseService = new UserService();
    var memoryCache = sp.GetRequiredService<IMemoryCache>();
    return new UserServiceCache(baseService, memoryCache);
});
```

Finally a version for both:

```csharp
// Option 4: With both Retry and Cache features
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IUserService>(sp =>
{
    var baseService = new UserService();
    var retryService = new UserServiceRetry(baseService);
    var memoryCache = sp.GetRequiredService<IMemoryCache>();
    return new UserServiceCache(retryService, memoryCache);
});
```

# How to use them?

Now is up to you. Just choose one of the next ways to register what you need.

In the API controller, you only need to inject `IUserService`—there's no need to specify which features (like caching or retries) are being used. Since the existing code already depends on this interface, no changes are required in the controller.

```csharp
// Example API Controller
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var users = _userService.GetAll();
        return Ok(users);
    }
}
```

Depending on what was registered in your startup class the decorators will be executed accordingly without any specific code in the endpoint itself.

# Conclusion

This design pattern is a great way to add extra behavior to a service without changing its original code. With it, you can easily add features like caching or retry logic by wrapping one class around another.
It helps keep the code clean, flexible, and easy to maintain — especially when the core logic stays the same, but you need to add features over time.