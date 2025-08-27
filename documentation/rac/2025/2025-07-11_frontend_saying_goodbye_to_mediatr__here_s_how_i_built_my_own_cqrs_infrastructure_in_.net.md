```yaml
---
title: "Saying Goodbye to MediatR? Here's How I Built My Own CQRS Infrastructure in .NET"
source: https://newsletter.kanaiyakatarmal.com/p/saying-goodbye-to-mediatr-heres-how
date_published: 2025-07-11T04:31:15.000Z
date_captured: 2025-08-25T15:07:20.157Z
domain: newsletter.kanaiyakatarmal.com
author: Kanaiya Katarmal
category: frontend
technologies: [MediatR, .NET, ASP.NET Core, Microsoft.Extensions.DependencyInjection, ILogger]
programming_languages: [C#]
tags: [cqrs, mediatr, dotnet, dependency-injection, architecture, pipeline, refactoring, custom-implementation, design-patterns, middleware]
key_concepts: [Command Query Responsibility Segregation (CQRS), Dependency Injection, Pipeline Pattern, Handler Pattern, Request/Response Pattern, Architectural Refactoring, Interface-based Programming, Reflection]
code_examples: false
difficulty_level: intermediate
summary: |
  This article guides developers on how to replace the MediatR library with a custom, lightweight CQRS infrastructure in .NET. It demonstrates building a MediatR-free setup using only .NET interfaces and built-in dependency injection, emphasizing minimal abstraction and explicit wiring. The author provides C# code examples for core interfaces, a `Sender` class, infrastructure setup, and custom pipeline behaviors like logging. This approach aims to offer greater control, simplify debugging, and eliminate third-party dependency concerns for applications heavily reliant on MediatR.
---
```

# Saying Goodbye to MediatR? Here's How I Built My Own CQRS Infrastructure in .NET

# Saying Goodbye to MediatR? Here's How I Built My Own CQRS Infrastructure in .NET

### How I replaced MediatR with a clean, testable CQRS setup using just interfaces and DI.

You’ve probably heard the news by now, **MediatR has moved to a commercial license**.

If you're starting a new project or only loosely tied to MediatR, switching away might not be a big deal. But for applications heavily built around it, this shift can cause significant friction. Migrating isn’t just about replacing a package, it often demands **careful architectural changes** and thoughtful refactoring.

In this article, I’ll walk you through how I **removed MediatR entirely** from my project and implemented a lightweight, MediatR-free CQRS-style infrastructure using **just .NET interfaces and dependency injection**, no external dependencies.

## ⚒️ Goals for My MediatR Replacement

When designing my replacement, I focused on the following principles:

✅ **Minimal abstraction**  
✅ **No third-party dependencies**  
✅ **Explicit and clear wiring**  
✅ **Easy to debug, test, and understand**

## CQRS Without MediatR? Totally Possible.

MediatR is great for implementing CQRS, but it's not essential. You can achieve the same clean separation of concerns with just a few well-defined interfaces and .NET’s built-in features.

Let’s break it down step by step:

## 🔌 Core Interfaces

Define the essential interfaces to represent requests, handlers, and pipeline behaviors:

```csharp
public interface IRequest<TResponse> { }

public interface IRequestHandler<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

public interface IPipelineBehavior<TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next);
}

public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
```

## 🚀 The `Sender` Class

The `Sender` class resolves the appropriate handler and applies any registered pipeline behaviors:

```csharp
public class Sender(IServiceProvider provider) : ISender
{
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handler = provider.GetRequiredService(handlerType);

        RequestHandlerDelegate<TResponse> handlerDelegate = () =>
            ((dynamic)handler).Handle((dynamic)request, cancellationToken);

        var pipelineType = typeof(IPipelineBehavior<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var behaviors = provider.GetServices(pipelineType).Cast<dynamic>().ToList();

        foreach (var behavior in behaviors.AsEnumerable().Reverse())
        {
            var next = handlerDelegate;
            handlerDelegate = () => behavior.Handle((dynamic)request, cancellationToken, next);
        }

        return handlerDelegate();
    }
}
```

## 🧱 Setting Up the Infrastructure

Create a static helper to register your request handlers and pipeline behaviors:

```csharp
public static class MyMediator
{
    public static IServiceCollection AddMyMediator(this IServiceCollection services, Action<MediatorOptions>? configure = null)
    {
        var options = new MediatorOptions();
        configure?.Invoke(options);

        var assemblies = options.TargetAssemblies.Any()
            ? options.TargetAssemblies
            : new List<Assembly> { Assembly.GetCallingAssembly() };

        services.AddScoped<ISender, Sender>();

        foreach (var assembly in assemblies)
        {
            var handlerInterface = typeof(IRequestHandler<,>);
            var handlerTypes = assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface)
                .SelectMany(t => t.GetInterfaces()
                    .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface)
                    .Select(i => new { Interface = i, Implementation = t }));

            foreach (var handler in handlerTypes)
            {
                services.AddScoped(handler.Interface, handler.Implementation);
            }
        }

        foreach (var (serviceType, implementationType) in options.PipelineBehaviors)
        {
            services.AddScoped(serviceType, implementationType);
        }

        return services;
    }

    public static IServiceCollection AddPipeline<T>(this IServiceCollection services)
        where T : class
    {
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(T));
        return services;
    }
}
```

## 🧪 Sample Usage

### Request and Response Models

```csharp
public class AddEventRequestModel : IRequest<AddEventResponseModel>
{
    public string EventName { get; set; }
}

public class AddEventResponseModel
{
    public EventRepresentationModel Event { get; set; }
}
```

**Handler Implementation**

```csharp
public class AddEventHandler : IRequestHandler<AddEventRequestModel, AddEventResponseModel>
{
    public Task<AddEventResponseModel> Handle(AddEventRequestModel request, CancellationToken cancellationToken)
    {
        var @event = request.Adapt<Event>();
        @event.Id = new Random().Next(100, 999);

        var eventModel = @event.Adapt<EventRepresentationModel>();

        return Task.FromResult(new AddEventResponseModel
        {
            Event = eventModel
        });
    }
}
```

## 📋 Registering Your Handlers and Behaviors

You can wire everything up during application startup:

```csharp
builder.Services.AddMyMediator(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
});
```

## 🧾 Example Logging Behavior

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        _logger.LogInformation("Handling {RequestName}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Handled {RequestName}", typeof(TRequest).Name);
        return response;
    }
}
```

## 🎯 Testing Your Setup

You can now use your custom `ISender` to handle requests:

```csharp
[HttpGet]
public async Task<IActionResult> GetAllEvent(CancellationToken cancellationToken)
{
    var response = await sender.Send(new GetEventRequestModel(), cancellationToken);
    return Ok(response);
}
```

## 🧬 Custom Pipeline Behaviors — No MediatR Needed

One of the powerful features of MediatR is its **pipeline behavior** support — letting you inject logic like logging, performance tracking, validation, and more between request and response.

Good news: I’ve implemented a similar pipeline mechanism with just interfaces and .NET DI.

The key abstraction is:

```csharp
public interface IPipelineBehavior<TRequest, TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next);
}
```

This allows you to build behaviors like logging, caching, metrics, etc., and **chain them dynamically at runtime**, just like MediatR.

```csharp
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        CancellationToken cancellationToken,
        RequestHandlerDelegate<TResponse> next)
    {
        _logger.LogInformation("Starting request: {Request}", typeof(TRequest).Name);
        var response = await next();
        _logger.LogInformation("Completed request: {Request}", typeof(TRequest).Name);
        return response;
    }
}
```

You can register behaviors the same way you would handlers:

```csharp
builder.Services.AddMyMediator(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
    config.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
});
```

This gives you a fully extensible, clean, and **MediatR-free pipeline behavior system**, keeping your CQRS logic modular and maintainable.

## ✅ Final Thoughts

Replacing MediatR might seem daunting at first, but if you're aiming for **simplicity, transparency, and full control**, rolling out your own infrastructure can be incredibly rewarding.

This lightweight setup:

*   Keeps your CQRS structure intact
*   Eliminates third-party dependency concerns
*   Gives you better insight into your application's request handling flow

## **📁 GitHub Example**

👉 Full working code available at:  
🔗 [https://sourcecode.kanaiyakatarmal.com/mymediatr](https://sourcecode.kanaiyakatarmal.com/mymediatr)

I hope you found this guide helpful and informative.

Thanks for reading!

If you enjoyed this article, feel free to **share it** and **[follow me](https://www.linkedin.com/in/kanaiyakatarmal/)** for more practical, developer-friendly content like this.