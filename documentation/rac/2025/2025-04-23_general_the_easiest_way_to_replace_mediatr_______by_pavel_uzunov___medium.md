```yaml
---
title: "The Easiest Way to Replace MediatR! 🚀 | by Pavel Uzunov | Medium"
source: https://medium.com/@paveluzunov/the-easiest-way-to-replace-mediatr-cb6a0fa07ded
date_published: 2025-04-23T16:15:39.680Z
date_captured: 2025-08-08T12:31:02.167Z
domain: medium.com
author: Pavel Uzunov
category: general
technologies: [MediatR, .NET]
programming_languages: [C#]
tags: [mediator-pattern, cqrs, dotnet, design-patterns, dependency-injection, clean-code, software-architecture, custom-implementation, pipeline, licensing]
key_concepts: [cqrs, mediator-pattern, dependency-injection, pipeline-behavior, cross-cutting-concerns, interfaces, generics, domain-driven-design]
code_examples: false
difficulty_level: intermediate
summary: |
  This article presents a lightweight, custom alternative to MediatR for .NET projects, prompted by MediatR's shift to a commercial license. It guides readers through building their own mediator implementation that adheres to the CQRS pattern, separating commands and queries. The solution includes support for pipeline behaviors, enabling cross-cutting concerns like logging or validation. Detailed C# code examples are provided for core interfaces, the mediator implementation, and real-world usage with command/query handlers and DTO mapping. The author highlights the advantages of this approach, such as zero external dependencies, full control over the codebase, and freedom from licensing concerns.
---
```

# The Easiest Way to Replace MediatR! 🚀 | by Pavel Uzunov | Medium

![A profile picture of Pavel Uzunov.](https://miro.medium.com/v2/da:true/resize:fill:64:64/0*C8QpMhkUleBWo4QB)

![A dark blue background with white text that reads "The Easiest Way to Replace MediatR" and a white rocket icon below the text.](https://miro.medium.com/v2/resize:fit:700/0*i7mZDowz5EeVpcyM)

If you’ve been using MediatR in your .NET projects, you’re probably familiar with how it helps structure your code. But with MediatR going commercial, I wondered: **Do we really need it?** 🤔

In this article, I’ll show you how to build your own lightweight alternative with simple, clean code that follows the CQRS pattern. No external dependencies, no licensing worries, and full control over your codebase.

# MediatR Going Commercial 💼

MediatR is moving to a commercial license, which means the full-featured version is no longer free. If you prefer simplicity without extra dependencies or licensing concerns, now is the perfect time to consider a custom alternative.
For more details, check out the official announcement [here](https://www.jimmybogard.com/automapper-and-mediatr-going-commercial/).

# Do We Really Need MediatR? 🤔

MediatR is great, but it’s not essential. Here’s why creating your own implementation might make sense:

*   **Simplicity**: A custom implementation can be tailored exactly to your needs
*   **No External Dependencies**: Reduce package dependencies in your project
*   **Full Control**: Understand every part of your codebase
*   **Licensing Freedom**: No commercial licensing concerns

# Building a Custom CQRS Mediator 🔧

**CQRS** separates reads (**queries**) and writes (**commands**) — a pattern that brings clarity to your application’s data flow. Here’s how to implement your own mediator that supports both patterns:

## Core Interfaces

First, define the basic interfaces for commands and queries:

```csharp
// Command interfaces  
public interface ICommand<TResult> { }  
  
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>  
{  
    Task<TResult> HandleAsync(TCommand command, CancellationToken cancellationToken = default);  
}  
  
// Query interfaces  
public interface IQuery<TResult> { }  
  
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>  
{  
    Task<TResult> HandleAsync(TQuery query, CancellationToken cancellationToken = default);  
}
```

## Pipeline Behavior

Add support for cross-cutting concerns with pipeline behaviors:

```csharp
public interface IPipelineBehavior<in TInput, TOutput>  
{  
    Task<TOutput> HandleAsync(TInput input, Func<Task<TOutput>> next, CancellationToken cancellationToken = default);  
}
```

## The Mediator Interface

Define a clean interface for your mediator:

```csharp
public interface IMediator  
{  
    Task<TResult> SendCommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>;  
  
    Task<TResult> SendQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>;  
}
```

## Mediator Implementation

Here’s the complete implementation with pipeline support:

```csharp
public class Mediator(IServiceProvider provider) : IMediator  
{  
    public async Task<TResult> SendCommandAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand<TResult>  
    {  
        var handler = provider.GetService<ICommandHandler<TCommand, TResult>>();  
        if (handler == null)  
        {  
            throw new InvalidOperationException($"No handler registered for {typeof(TCommand).Name}");  
        }  
  
        var behaviors = provider.GetServices<IPipelineBehavior<TCommand, TResult>>().Reverse();  
  
        Func<Task<TResult>> handlerDelegate = () => handler.HandleAsync(command, cancellationToken);  
        foreach (var behavior in behaviors)  
        {  
            var next = handlerDelegate;  
            handlerDelegate = () => behavior.HandleAsync(command, next, cancellationToken);  
        }  
  
        return await handlerDelegate();  
    }  
  
    public async Task<TResult> SendQueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) where TQuery : IQuery<TResult>  
    {  
        var handler = provider.GetService<IQueryHandler<TQuery, TResult>>();  
        if (handler == null)  
        {  
            throw new InvalidOperationException($"No handler registered for {typeof(TQuery).Name}");  
        }  
          
        var behaviors = provider.GetServices<IPipelineBehavior<TQuery, TResult>>().Reverse();  
        Func<Task<TResult>> handlerDelegate = () => handler.HandleAsync(query, cancellationToken);  
        foreach (var behavior in behaviors)  
        {  
            var next = handlerDelegate;  
            handlerDelegate = () => behavior.HandleAsync(query, next, cancellationToken);  
        }  
  
        return await handlerDelegate();  
    }  
}
```

# Real-World Usage Example 💻

Let’s see how this works in practice with some example commands and queries.

## Command Example

```csharp
// Command definition  
public record CreateUserCommand(string UserName, string Email) : ICommand<UserDto>;  
  
// Command handler  
public class CreateUserCommandHandler(IUserRepository userRepository) : ICommandHandler<CreateUserCommand, UserDto>  
{  
    public async Task<UserDto> HandleAsync(CreateUserCommand command, CancellationToken cancellationToken)  
    {  
        // Create domain object  
        var user = new User(command.UserName, command.Email);  
  
        // Create and return DTO from saved domain object  
        var createdUser = await userRepository.CreateUserAsync(user, cancellationToken);  
        return createdUser.ToDto();  
    }  
}
```

## Query Example

```csharp
// Query definition  
public record GetUserByIdQuery(int UserId) : IQuery<UserDto?>;  
  
// Query handler  
public class GetUserByIdQueryHandler(IUserRepository userRepository) : IQueryHandler<GetUserByIdQuery, UserDto?>  
{  
    public async Task<UserDto?> HandleAsync(GetUserByIdQuery query, CancellationToken cancellationToken)  
    {  
        var user = await userRepository.GetUserByIdAsync(query.UserId, cancellationToken);  
        return user?.ToDto();  
    }  
}
```

## Mapping Extensions

When working with CQRS, you’ll often map between domain models and DTOs. A simple extension method makes this process cleaner:

```csharp
// Extension method for mapping  
public static class UserMappingExtensions  
{  
    public static UserDto ToDto(this User user)  
    {  
        return new UserDto  
        {  
            Id = user.Id,  
            UserName = user.UserName,  
            Email = user.Email  
        };  
    }  
}
```

## Using the Mediator

```csharp
// Note: Using the mediator injected via constructor  
  
// Execute a command  
var createCommand = new CreateUserCommand("john.doe", "john.doe@example.com");  
var newUser = await mediator.SendCommandAsync<CreateUserCommand, UserDto>(createCommand, cancellationToken);  
  
// Execute a query  
var getUserQuery = new GetUserByIdQuery(newUser.Id);  
var user = await mediator.SendQueryAsync<GetUserByIdQuery, UserDto?>(getUserQuery, cancellationToken);
```

## Pipeline Behavior Example

```csharp
// Pipeline behavior  
public class LoggingBehavior<TInput, TOutput> : IPipelineBehavior<TInput, TOutput>  
{  
    public async Task<TOutput> HandleAsync(TInput input, Func<Task<TOutput>> next, CancellationToken cancellationToken)  
    {  
        Console.WriteLine($"Starting: {typeof(TInput).Name}");  
        var result = await next();  
        Console.WriteLine($"Finished: {typeof(TOutput).Name}");  
        return result;  
    }  
}
```

## Service Registration

```csharp
// Register mediator  
services.AddSingleton<IMediator, Mediator>();  
  
// Register handlers  
services.AddTransient<IQueryHandler<GetUserByIdQuery, UserDto?>, GetUserByIdQueryHandler>();  
services.AddTransient<ICommandHandler<CreateUserCommand, UserDto>, CreateUserCommandHandler>();  
  
// Register pipeline behaviors  
services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
```

# Advantages of Your Custom Implementation 💯

*   **Zero Dependencies**: No external packages required
*   **Lightweight**: Only includes what you need
*   **Full Transparency:** You understand every line of code
*   **Extensible Pipeline:** Add behaviors for logging, validation, caching, etc.

# When to Consider This Approach 🤓

This solution is ideal for teams that:

*   Want to avoid commercial licensing costs
*   Prefer simplicity and control
*   Need only core CQRS functionality
*   Want to minimize external dependencies
*   Enjoy understanding their entire codebase

# Conclusion 💬

Building your own MediatR replacement gives you complete freedom to shape your codebase exactly how you want. With the implementation shown here, you get the core benefits of CQRS and the mediator pattern without any external dependencies or licensing concerns.

It’s lightweight, flexible, and easy to understand — perfect for developers who appreciate clean, minimal code that does exactly what they need, nothing more and nothing less.

Give it a try and see how it fits your workflow!

# Check Out the Code

I’ve created a GitHub repo that follows the same approach and abstraction from this article: [github.com/pavuzu/simple-cqrs-mediator](https://github.com/pavuzu/simple-cqrs-mediator)