```yaml
---
title: "Global Exception Handling in ASP.NET Core: From Throw to ProblemDetails | by Kittikawin L. 🍀 | Aug, 2025 | Medium"
source: https://medium.com/@kittikawin_ball/global-exception-handling-in-asp-net-core-from-throw-to-problemdetails-6544b00bf9d2
date_published: 2025-08-21T15:01:37.032Z
date_captured: 2025-08-29T12:23:48.922Z
domain: medium.com
author: Kittikawin L. 🍀
category: ai_ml
technologies: [ASP.NET Core, .NET, ProblemDetails (Microsoft.AspNetCore.Mvc), RFC 7807, System.Text.Json, Serilog, OpenTelemetry]
programming_languages: [C#, JSON]
tags: [exception-handling, aspnet-core, middleware, web-api, error-handling, problemdetails, http-status-codes, logging, csharp, dotnet]
key_concepts: [global-exception-handling, middleware-pattern, problem-details-standard, http-status-codes, correlation-id, structured-logging, separation-of-concerns, custom-exceptions]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to implementing global exception handling in ASP.NET Core APIs. It emphasizes using custom middleware to centralize error management, ensuring consistent and structured error responses. The author demonstrates how to create and register middleware that catches unhandled exceptions, maps them to appropriate HTTP status codes, and formats the output according to the ProblemDetails standard (RFC 7807). Additionally, the guide covers enhancing production readiness by incorporating correlation IDs and robust logging practices for improved observability and debugging.]
---
```

# Global Exception Handling in ASP.NET Core: From Throw to ProblemDetails | by Kittikawin L. 🍀 | Aug, 2025 | Medium

Member-only story

# Global Exception Handling in ASP.NET Core: From Throw to ProblemDetails

[

![Kittikawin L. 🍀](https://miro.medium.com/v2/resize:fill:64:64/2*TdfmhjV1K94GviFrOweSwg.png)

](/@kittikawin_ball?source=post_page---byline--6544b00bf9d2---------------------------------------)

[Kittikawin L. 🍀](/@kittikawin_ball?source=post_page---byline--6544b00bf9d2---------------------------------------)

Follow

3 min read

·

Aug 21, 2025

Listen

Share

More

Press enter or click to view image in full size

![A MacBook Pro laptop displaying an IDE with Python code and an API testing tool, resting on a wooden desk.](https://miro.medium.com/v2/resize:fit:700/0*qo7_7PMZNkm_9Pn5)

Photo by [Douglas Lopes](https://unsplash.com/@douglasamarelo?utm_source=medium&utm_medium=referral) on [Unsplash](https://unsplash.com/?utm_source=medium&utm_medium=referral)

Every developer has seen it before. An API suddenly throws an unhandled exception, and the client receives a **500 Internal Server Error**. This not only frustrates the consumer but also leaks unnecessary internal details.

In modern .NET, there’s a better way → **global exception handling middleware** that converts exceptions into a consistent and meaningful error response. ASP.NET Core also supports [**ProblemDetails**](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails?view=aspnetcore-9.0) **(**[**RFC 7807**](https://datatracker.ietf.org/doc/html/rfc7807)**)**, a standard JSON format for HTTP API errors.

### This article will show you how.

*   Centralize error handling in your ASP.NET Core API
*   Map exceptions to appropriate HTTP status codes
*   Return structured ProblemDetails responses
*   Add logging and correlation IDs for production-ready observability

## Why Middleware for Exception Handling?

Instead of distributing _try/catch blocks_ or _relying on filters_, the middleware provides the following.

*   **Centralization**  
    One place to handle all unhandled exceptions.
*   **Consistency**  
    Every error response follows the same contract.
*   **Flexibility**  
    Easy to map custom exceptions to custom responses.
*   **Separation of concerns**  
    Business code focuses on business logic, not error formatting.

### Step 1: Create Custom Middleware

```csharp
using Microsoft.AspNetCore.Http;  
using Microsoft.AspNetCore.Mvc;  
using System.Net;  
using System.Text.Json;  
  
public class ExceptionHandlingMiddleware  
{  
    private readonly RequestDelegate _next;  
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;  
    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)  
    {  
        _next = next;  
        _logger = logger;  
    }  
    public async Task InvokeAsync(HttpContext context)  
    {  
        try  
        {  
            await _next(context); // continue pipeline  
        }  
        catch (Exception ex)  
        {  
            _logger.LogError(ex, "Unhandled exception occurred");  
            await HandleExceptionAsync(context, ex);  
        }  
    }  
    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)  
    {  
        var problem = new ProblemDetails  
        {  
            Type = "type of the problem",  
            Title = "An unexpected error occurred",  
            Status = (int)HttpStatusCode.InternalServerError,  
            Detail = exception.Message,  
            Instance = context.TraceIdentifier  
        };  
          
        context.Response.ContentType = "application/json";  
        context.Response.StatusCode = problem.Status.Value;  
  
        var options = new JsonSerializerOptions {  
          PropertyNamingPolicy = JsonNamingPolicy.CamelCase  
        };  
  
        await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));  
    }  
}
```

### Step 2: Register the Middleware

In your `Program.cs` (for .NET 9 minimal APIs or ASP.NET Core)

```csharp
var builder = WebApplication.CreateBuilder(args);  
var app = builder.Build();  
  
// Add our middleware before app.MapControllers()  
app.UseMiddleware<ExceptionHandlingMiddleware>();  
app.MapControllers();  
app.Run();
```

### Step 3: Map Custom Exceptions

You can improve this further by mapping **domain exceptions** to specific HTTP codes.

```csharp
private static async Task HandleExceptionAsync(HttpContext context, Exception exception)  
{  
    ProblemDetails problem;  
      
    switch (exception)  
    {  
        case UnauthorizedAccessException:  
            problem = new ProblemDetails  
            {  
                Title = "Unauthorized",  
                Status = StatusCodes.Status401Unauthorized,  
                Detail = "You are not authorized to access this resource."  
            };  
            break;  
        case KeyNotFoundException:  
            problem = new ProblemDetails  
            {  
                Title = "Resource Not Found",  
                Status = StatusCodes.Status404NotFound,  
                Detail = exception.Message  
            };  
            break;  
        default:  
            problem = new ProblemDetails  
            {  
                Title = "Unexpected Error",  
                Status = StatusCodes.Status500InternalServerError,  
                Detail = exception.Message  
            };  
            break;  
    }  
  
    problem.Instance = context.TraceIdentifier;  
    context.Response.ContentType = "application/json";  
    context.Response.StatusCode = problem.Status.Value;  
  
    await context.Response.WriteAsJsonAsync(problem);  
}
```

### Step 4: Add Correlation IDs and Logging

For production readiness.

*   Use a **correlation ID** to trace requests across logs.
*   Log the full exception internally but return a safe, minimal message to clients.
*   Pair with **Serilog or OpenTelemetry** for structured logging.

**Example with a correlation ID.**

```csharp
problem.Extensions["traceId"] = context.TraceIdentifier;
```

Now clients can provide this `traceId` when reporting issues.

### Step 5: Test It

*   Request an endpoint that throws an exception.
*   You should receive a [**ProblemDetails**](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails?view=aspnetcore-9.0) **JSON response** like:

```json
{  
  "type": "type of the problem",  
  "title": "Unexpected Error",  
  "status": 500,  
  "detail": "Object reference not set to an instance of an object.",  
  "instance": "XXXB1234XYZ",  
  "traceId": "XXXB1234XYZ"  
}
```

## Final Thought

Global exception handling middleware transforms your .NET API from “chaotic error dumps” into **predictable, production-ready responses**. By embracing [**ProblemDetails**](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails?view=aspnetcore-9.0), you can standardize error payloads, improve debugging, and provide a better developer experience for your customers.

> Don’t wait for the next **500** error to **bite** you — implement global exception handling now and make your APIs easier to use, maintain, and trust.

## Thanks for reading!

[**Follow**](/@kittikawin_ball) and [**subscribe**](/@kittikawin_ball/subscribe) to get updates on my latest articles.  
See you, **Kittikawin**

[

## Create web APIs with ASP.NET Core

### Learn the basics of creating a web API in ASP.NET Core.

learn.microsoft.com

](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-9.0&source=post_page-----6544b00bf9d2---------------------------------------#automatic-http-400-responses)

[

## RFC 7807: Problem Details for HTTP APIs

### This document defines a "problem detail" as a way to carry machine- readable details of errors in a HTTP response to…

datatracker.ietf.org

](https://datatracker.ietf.org/doc/html/rfc7807?source=post_page-----6544b00bf9d2---------------------------------------)

[

## ProblemDetails Class (Microsoft.AspNetCore.Mvc)

### A machine-readable format for specifying errors in HTTP API responses based on https://tools.ietf.org/html/rfc7807.

learn.microsoft.com

](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.problemdetails?view=aspnetcore-9.0&source=post_page-----6544b00bf9d2---------------------------------------)