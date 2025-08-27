```yaml
---
title: How To Create Custom Middlewares in ASP.NET Core
source: https://antondevtips.com/blog/how-to-create-custom-middlewares-in-asp-net-core
date_published: 2024-04-23T11:00:15.167Z
date_captured: 2025-08-06T17:19:49.575Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [ASP.NET Core, .NET]
programming_languages: [C#]
tags: [aspnet-core, middleware, http-pipeline, web-development, dotnet, custom-middleware, dependency-injection, chain-of-responsibility]
key_concepts: [middleware-architecture, http-request-pipeline, chain-of-responsibility-pattern, dependency-injection, request-delegate, custom-middleware, short-circuiting]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to creating custom middlewares in ASP.NET Core, explaining their role in the HTTP request pipeline. It details three distinct methods for implementation: using a delegate with `WebApplication.Use`, creating a conventional middleware class, and implementing the `IMiddleware` interface. The post emphasizes the importance of middleware order and demonstrates how to short-circuit the pipeline. Additionally, it covers the nuances of dependency injection within middlewares, discussing how service lifetimes affect their usage. The author recommends implementing the `IMiddleware` interface for a safer and more controlled approach.]
---
```

# How To Create Custom Middlewares in ASP.NET Core

![Article banner: A dark blue background with abstract purple shapes. On the left, a white square icon with angle brackets `</>` inside, and the text "dev tips" below it. On the right, in large white letters, the title "HOW TO CREATE CUSTOM MIDDLEWARES IN ASP.NET CORE". Below the title, a white line and the text "Improve your skills on antondevtips.com".](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_asp_custom_middlewares.png&w=3840&q=100)

# How To Create Custom Middlewares in ASP.NET Core

Apr 23, 2024

4 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

ASP.NET Core's middleware architecture offers a powerful way to build and configure the HTTP request pipeline in your applications. In this post, you'll explore what middleware is and how to create a custom middlewares in ASP.NET Core.

## What is Middleware in ASP.NET Core?

Middleware in ASP.NET Core is a software component that is a part of application pipeline that handles requests and responses. In ASP.NET Core there are multiple middlewares that are combined in a chain with each other.

Each middleware component in the pipeline is responsible for invoking the next component in the sequence. Any middleware can stop other middlewares from execution by short-circuiting the chain if necessary. Middlewares in ASP.NET Core is a classic implementation of **chain of responsibility design pattern**.

ASP.NET Core has a lot of built-in middlewares and many provided by Nuget packages. The **order** in which middlewares are added to the application pipeline is **critical**. It defines how the incoming HTTP requests travel through the pipeline and in what sequence the responses are sent back.

Middlewares are executed in the order they are added to the pipeline in the WebApplication object. If you want to learn more about common middlewares and their correct order - [read my blog post](https://antondevtips.com/blog/getting-started-with-middlewares-in-aspnet-core).

## How To Create Custom Middleware in ASP.NET Core

You can create a custom middleware in the following ways:

*   provide a delegate for `Use` method in `WebApplication` class
*   create a Middleware class by convention
*   create a Middleware class by inheriting from `IMiddleware` interface

### With a Use Method in WebApplication Class

You can call a `Use` method on the `WebApplication` class to create a middleware:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

var app = builder.Build();

app.Use(async (context, next) =>
{
    Console.WriteLine("Request is starting...");

    await next();

    Console.WriteLine("Request has finished");
});

app.MapGet("/api/books", () =>
{
    var books = SeedService.GetBooks(10);
    return Results.Ok(books);
});

await app.RunAsync();
```

In this example when calling a `/api/books` endpoint, the middleware declared in the `Use` method is called first. `await next.Invoke()` calls the books endpoint itself, but before and after we have a message logged to console:

```
Request is starting...
Request has finished
```

Middlewares are executed in the order they are added to the pipeline in the WebApplication object. Each middleware can perform operations before and after the next middleware:

**Before:** executing operations before calling the next middleware can include tasks like logging, authentication, validation, etc.

**After:** operations after calling the next middleware can include tasks like response modification or error handling.

The real power of middlewares is that you can chain them freely in any order you want. To stop the request from executing and short-cut the middleware chain (stop other middlewares from executing) - write a response directly into HttpContext instead of calling the `await next.Invoke()` method:

```csharp
await context.Response.WriteAsync("Some response here");
```

### With a Middleware Class by Convention

You can extract a middleware to a separate class that follows the specific convention:

```csharp
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        Console.WriteLine($"Request: {context.Request.Method} {context.Request.Path}");

        await _next(context);

        Console.WriteLine($"Response: {context.Response.StatusCode}");
    }
}
```

To add this middleware to the pipeline, call the `UseMiddleware` method on the `WebApplication` class:

```csharp
app.Use(async (context, next) =>
{
    // Middleware from previous example
});

app.UseMiddleware<LoggingMiddleware>();
```

As a result of executing this middleware the following will be logged to the console when executing an `/api/books` endpoint:

```
Request is starting...
Request: GET /api/books

Response: 200
Request has finished
```

This approach is called by convention, because middleware class must follow these rules:

*   middleware class should have a `InvokeAsync` method with a required `HttpContext` argument
*   middleware class should inject a next `RequestDelegate` in the constructor
*   middleware class call the next `RequestDelegate` delegate and pass it the `HttpContext` argument

### With a Middleware Class That Implements `IMiddleware` Interface

The previous approach has its drawbacks: the developer needs to create a middleware class that follows all mentioned above rules, otherwise a middleware won't work. But there is a safer way to create a middleware: implement the `IMiddleware` interface:

```csharp
public class ExecutionTimeMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var watch = Stopwatch.StartNew();
        
        await next(context);
        
        watch.Stop();
        
        Console.WriteLine($"Request executed in {watch.ElapsedMilliseconds}ms");
    }
}
```

This approach is much safer as the compiler tells how the middleware class should look like.

For this approach you need to manually register `ExecutionTimeMiddleware` in the DI container:

```csharp
builder.Services.AddScoped<ExecutionTimeMiddleware>();
```

To add this middleware to the pipeline, call the `UseMiddleware` method on the `WebApplication` class:

```csharp
app.Use(async (context, next) =>
{
    // Middleware from previous example
});

app.UseMiddleware<LoggingMiddleware>();
app.UseMiddleware<ExecutionTimeMiddleware>();
```

As a result of executing this middleware the following will be logged to the console when executing an `/api/books` endpoint:

```
Request is starting...
Request: GET /api/books
Request executed in 68ms

Response: 200
Request has finished
```

## Middlewares and Dependency Injection

Middlewares by convention have `Singleton` lifetime by default and all dependencies injected in constructor must be **singletons** too. As we already know, middlewares run per each request, and you can inject **scoped** dependencies in the `InvokeAsync` method after `HttpContext`. Here we are injecting a `ILoggingService` that is registered as **scoped** service in DI:

```csharp
builder.Services.AddScoped<ILoggingService, ConsoleLoggingService>();
```
```csharp
public class LoggingMiddleware
{
    private readonly RequestDelegate _next;

    public LoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ILoggingService loggingService)
    {
        loggingService.LogRequest(context.Request.Method, context.Request.Path);

        await _next(context);

        loggingService.LogResponse(context.Response.StatusCode);
    }
}
```

This approach is suitable only for middleware classes created by **convention**. To inject scoped services into middleware classes that implement `IMiddleware` interface, simply use the constructor:

```csharp
public class ExecutionTimeMiddleware : IMiddleware
{
    private readonly ILoggingService _loggingService;

    public ExecutionTimeMiddleware(ILoggingService loggingService)
    {
        _loggingService = loggingService;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // ...
    }
}
```

**NOTE:** when creating a middleware class that implements `IMiddleware` interface - you are responsible for selecting an appropriate DI lifetime for it. You can create `Singleton`, `Scoped` or `Transient` middleware, select what suits the best in each use case.

## Summary

You can create a custom middleware in the following ways:

*   provide a delegate for `Use` method in `WebApplication` class
*   create a Middleware class by convention
*   create a Middleware class by inheriting from `IMiddleware` interface

My preferred choice is to create a middleware by inheriting from the `IMiddleware` interface. This approach offers a safer, more convenient way to create middlewares and a straightforward dependency injection strategy through a constructor. And it also gives a full control over the middleware lifetime.

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-create-custom-middlewares-in-asp-net-core&title=How%20To%20Create%20Custom%20Middlewares%20in%20ASP.NET%20Core)[X](https://twitter.com/intent/tweet?text=How%20To%20Create%20Custom%20Middlewares%20in%20ASP.NET%20Core&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-create-custom-middlewares-in-asp-net-core)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-create-custom-middlewares-in-asp-net-core)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.