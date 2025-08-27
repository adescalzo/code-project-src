```yaml
---
title: Getting Started with Middlewares in ASP.NET Core
source: https://antondevtips.com/blog/getting-started-with-middlewares-in-aspnet-core
date_published: 2024-03-30T12:00:43.379Z
date_captured: 2025-08-06T17:19:42.527Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [ASP.NET Core, .NET, Swagger, Nuget]
programming_languages: [C#]
tags: [aspnet-core, middleware, http-pipeline, web-development, csharp, web-api, dotnet, request-processing, application-architecture]
key_concepts: [middleware-architecture, http-request-pipeline, chain-of-responsibility-pattern, middleware-order, short-circuiting, built-in-middlewares, custom-middlewares, error-handling, authentication, authorization, caching, logging]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides an introduction to ASP.NET Core's middleware architecture, explaining how these software components form a chain to process HTTP requests and responses. It covers the creation of custom middleware and demonstrates how to short-circuit the pipeline. The post also details common built-in middlewares such as `UseExceptionHandler`, `UseStaticFiles`, `UseAuthentication`, and `UseAuthorization`. A key focus is placed on the critical importance of middleware order within the pipeline, offering a typical registration sequence and practical tips for optimal placement to ensure correct application functionality, security, and performance.]
---
```

# Getting Started with Middlewares in ASP.NET Core

![Cover image for the article "Getting Started with Middlewares in ASP.NET Core". It features a dark blue background with abstract purple wave-like shapes. On the left, a white square icon with a black `< />` symbol inside, next to the text 'dev tips'. On the right, large white text reads 'MIDDLEWARES IN ASP.NET CORE'. Below this, a thin white line separates the text 'Improve your coding skills'.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_asp_middlewares.png&w=3840&q=100)

# Getting Started with Middlewares in ASP.NET Core

Mar 30, 2024

3 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

ASP.NET Core's middleware architecture offers a powerful way to build and configure the HTTP request pipeline in your applications. In this post, you'll explore what middleware is, the correct order of middlewares, and discuss some of the most widely used middlewares.

## What is Middleware in ASP.NET Core?

Middleware in ASP.NET Core is a software component that is a part of application pipeline that handles requests and responses. In ASP.NET Core there are multiple middlewares that are combined in a chain with each other. Each middleware component in the pipeline is responsible for invoking the next component in the sequence. Any middleware can stop other middlewares from execution by short-circuiting the chain if necessary. Middlewares in ASP.NET Core is a classic implementation of **chain of responsibility design pattern**.

Creating a middleware in ASP.NET Core is straightforward. All you need to do is to call a `Use` method on the `WebApplication` object:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Logging.AddConsole();

var app = builder.Build();

app.Use(async (context, next) =>
{
    var logger = context.RequestServices.GetService<ILogger<Program>>();
    logger?.LogInformation("Request is starting...");

    await next.Invoke();
    
    logger?.LogInformation("Request has finished");
});

app.MapGet("/api/books", () =>
{
    var books = SeedService.GetBooks(10);
    return Results.Ok(books);
});

await app.RunAsync();
```

In this example when calling a `/api/books` endpoint, the middleware declared in the `Use` method is called first. `await next.Invoke()` calls the books endpoint itself, but before and after we have a message logged to console.

Middlewares are executed in the order they are added to the pipeline in the WebApplication object. Each middleware can perform operations before and after the next middleware:

**Before:** executing operations before calling the next middleware can include tasks like logging, authentication, validation, etc.  
**After:** operations after calling the next middleware can include tasks like response modification or error handling.

The real power of middlewares is that you can chain them freely in any order you want. To stop the request from executing and short-cut the middleware chain (stop other middlewares from executing) - write a response directly into HttpContext instead of calling the `await next.Invoke()` method:

```csharp
await context.Response.WriteAsync("Some response here");
```

## The Most Common Built-in Middlewares in ASP.NET Core

ASP.NET Core has a lot of built-in middlewares and many provided by Nuget packages. Let's explore the most common middlewares:

*   **UseExceptionHandler**: first in the pipeline to catch and handle exceptions thrown in later stages.
*   **UseRateLimiter**: limits the number of requests a client can make in a given time.
*   **UseHsts**: enforces the use of HTTPS in the application.
*   **UseHttpsRedirection**: redirects HTTP requests to HTTPS.
*   **UseStaticFiles**: serves static files and should come before any dynamic processing.
*   **UseHttpLogging**: logs HTTP request and response data.
*   **UseForwardedHeaders**: processes headers from load balancers, forwarding and reverse proxies.
*   **UseRouting**: determines the routing of the application.
*   **UseCors**: manages Cross-Origin Resource Sharing policies.
*   **UseAuthentication**: manages user authentication.
*   **UseAuthorization**: manages user authorization.
*   **UseOutputCache**: stores responses to cache for performance improvement.
*   **Swagger (only for development)**: provides a UI with documentation for the Web API.
*   **Map... Methods**: maps the incoming requests to appropriate endpoints.
*   **Fallback**: provides a handler for unmatched routes.

## Understanding the Correct Order of Middlewares

The order in which middlewares are added to the application pipeline is critical. It defines how the incoming HTTP requests travel through the pipeline and in what sequence the responses are sent back.

Middlewares are executed in the order they are added to the pipeline in the WebApplication object.

Here is a typical order of middleware registration in an ASP.NET Core application:

```csharp
var app = builder.Build();

app.UseExceptionHandler(_ => {});

app.UseRateLimiter();

app.UseHsts();
app.UseHttpsRedirection();

app.UseResponseCaching();
app.UseResponseCompression();
app.UseStaticFiles();

app.UseHttpLogging();

app.UseForwardedHeaders();

app.UseRouting();
app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.UseOutputCache();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapControllers();

app.MapHealthChecks("/health");

app.Use(async (context, next) =>
{
    // Custom middlewares here
});

app.MapGet("/api/books", () =>
{
    var books = SeedService.GetBooks(10);
    return Results.Ok(books);
});

app.MapFallbackToFile("wwwroot/index.html");

await app.RunAsync();
```

When the webapp receives an HTTP request, **UseExceptionHandler** middleware is the first to go, going down in the order these middlewares are defined in Program.cs. When the Minimal API or Controller endpoint is executed (books are retrieved in this example) the response comes up in the chain of middlewares. Here are some of the important tips when placing the middlewares:

*   **UseExceptionHandler** is placed as first middleware to handle any errors occured in our http endpoints or other middlewares.
*   Here we use **UseHttpLogging** middleware down after the **UseResponseCompression**. It means that after executing the endpoint, response with books is correctly logged before being compressed. If **UseHttpLogging** was placed before the **UseResponseCompression** we would end up with a mess of bytes and symbols in the response log instead of a readable json.
*   When an HTTP endpoint requires authentication and the request doesn't provide valid auth credentials: the **UseAuthentication** endpoint breaks the chain of middlewares and returns 401 Unauthorized response.
*   **UseOutputCache** is placed after the authentication and authorization middlewares to make sure that only authenticated and authorized client can get the cached response.

## Summary

Middlewares are a powerful feature in ASP.NET Core, offering developers the flexibility to customize the request pipeline to suit the application's needs. Understanding and correctly configuring middlewares ensure your application handles requests and responses efficiently while maintaining security and performance.

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-middlewares-in-aspnet-core&title=Getting%20Started%20with%20Middlewares%20in%20ASP.NET%20Core)[X](https://twitter.com/intent/tweet?text=Getting%20Started%20with%20Middlewares%20in%20ASP.NET%20Core&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-middlewares-in-aspnet-core)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-middlewares-in-aspnet-core)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.