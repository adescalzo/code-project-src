```yaml
---
title: 3 Methods to Create Middleware in ASP.NET Core
source: https://okyrylchuk.dev/blog/three-methods-to-create-middleware-in-asp-net-core/
date_published: 2024-09-13T15:50:00.000Z
date_captured: 2025-08-20T21:16:49.105Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [ASP.NET Core, .NET]
programming_languages: [C#]
tags: [asp.net-core, middleware, web-development, http-pipeline, csharp, dotnet, request-processing]
key_concepts: [middleware-pipeline, request-delegate, convention-based-middleware, factory-based-middleware, short-circuiting, dependency-injection]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores three distinct methods for creating custom middleware in ASP.NET Core, a crucial component of the request-processing pipeline. It begins by demonstrating the use of a `RequestDelegate` via `app.Use()` and `app.Run()` for inline or terminal middleware. Next, it details the convention-based approach, which involves injecting `RequestDelegate` into the constructor and implementing an `InvokeAsync` method. Lastly, the article explains factory-based middleware, requiring the implementation of the `IMiddleware` interface and explicit registration in the service container. Each method provides a flexible way to customize HTTP request and response handling.]
---
```

# 3 Methods to Create Middleware in ASP.NET Core

# 3 Methods to Create Middleware in ASP.NET Core

In ASP.NET Core, middleware is a component of the request-processing pipeline. Middleware components are pieces of software that are executed sequentially to handle HTTP requests and responses.

Middleware can handle tasks such as:

*   Authentication
*   Authorization
*   Logging
*   Exception handling
*   Routing
*   Static file serving

Let’s see how we can create custom middleware in ASP.NET Core. 

## **Request Delegate**

The Use method is a quick way to add new middleware. It adds a middleware delegate defined in-line to the application’s request pipeline. 

```csharp
var app = builder.Build();

app.Use(async (context, next) =>
{
    // Do something before calling the next middleware

    await next();

    // Do something after calling the next middleware
});

app.Run();
```

You can chain many delegates using the Use method.

When you don’t call the next delegate, you short-circuit the request pipeline. Short-circuiting is often desirable because it avoids unnecessary work.

When middleware shorts circuits, it’s called terminal middleware because it prevents further middleware from processing the request.

You can also add terminal middleware by using the Run method. 

```csharp
var app = builder.Build();

app.UseRouting();

app.MapGet("/", () => "Hello World!");

app.UseEndpoints(e => { });

app.Run(context =>
{
    context.Response.StatusCode = 404;
    return Task.CompletedTask;
});

app.Run();
```

When adding a terminal middleware:

*   The middleware must be added after UseEndpoints.
*   The app needs to call UseRouting and UseEndpoints so that the terminal middleware can be placed at the correct location.

## **Convention-based**

The second method to create a middleware is by convention. 

The convention is the following:

*   You must inject RequestDelegate via the constructor
*   You must add the InvokeAsync method with the first HttpContext parameter.

```csharp
public class ConventionalMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Do something before calling the next middleware

        await next(context);

        // Do something after calling the next middleware
    }
}
```

You must also “use” your middleware in your application.

```csharp
app.UseMiddleware<ConventionalMiddleware>();
```

## **Factory-based**

The third method is factory-based middleware. It’s similar to conventional-based middleware, but you implement the IMiddleware interface. 

```csharp
public class FactoryBasedMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        // Do something before calling the next middleware

        await next(context);

        // Do something after calling the next middleware
    }
}
```

The difference with conventional-based middleware is that you must register factory-based middleware first before using it. 

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddTransient<FactoryBasedMiddleware>();

var app = builder.Build();

app.UseMiddleware<FactoryBasedMiddleware>();

app.Run();
```

## Summary

In the post, I demonstrated three ways to create custom middleware in ASP.NET Core:

1.  Using a Request Delegate
2.  Convention-based
3.  Factory-based

Each method provides a way to customize how requests and responses are handled in the ASP.NET Core request pipeline.