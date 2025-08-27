```yaml
---
title: "Global Error Handling in ASP.NET Core: From Middleware to Modern Handlers"
source: https://www.milanjovanovic.tech/blog/global-error-handling-in-aspnetcore-from-middleware-to-modern-handlers?utm_source=newsletter&utm_medium=email&utm_campaign=tnw150
date_published: 2025-07-12T00:00:00.000Z
date_captured: 2025-08-06T17:49:37.521Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: ai_ml
technologies: [ASP.NET Core, .NET 8, .NET 9, FluentValidation, IProblemDetailsService, ProblemDetails]
programming_languages: [C#]
tags: [error-handling, aspnet-core, middleware, exception-handling, web-api, dotnet, problem-details, validation, logging]
key_concepts: [global-error-handling, middleware, problem-details-standard, iexceptionhandler, iproblemdetailsservice, dependency-injection, request-pipeline, exception-chaining]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores the evolution of global error handling in ASP.NET Core applications, emphasizing the importance of providing useful context to callers. It begins by demonstrating traditional middleware-based exception handling, highlighting its simplicity but also its limitations for complex scenarios. The content then introduces `IProblemDetailsService` as a way to standardize error responses using the Problem Details format (RFC 9457). Finally, the article presents `IExceptionHandler`, introduced in ASP.NET Core 8, as the modern and most maintainable approach, allowing for specific, chained handlers for different exception types, exemplified with FluentValidation. The author recommends `IExceptionHandler` for new projects due to its cleaner, more testable, and scalable nature.]
---
```

# Global Error Handling in ASP.NET Core: From Middleware to Modern Handlers

![Global Error Handling in ASP.NET Core: From Middleware to Modern Handlers - Blog Cover](https://www.milanjovanovic.tech/blog-covers/mnw_150.png?imwidth=3840)

# Global Error Handling in ASP.NET Core: From Middleware to Modern Handlers

5 min read · July 12, 2025

Let's talk about something we all deal with but often put off until the last minute - error handling in our ASP.NET Core apps.

When something breaks in production, the last thing you want is a cryptic 500 error with zero context. Proper error handling isn't just about logging exceptions. It's about making sure your app fails gracefully and gives useful info to the caller (and you).

In this article, I'll walk through the main options for global error handling in ASP.NET Core.

We'll look at how I used to do it, what ASP.NET Core 9 offers now, and where each approach makes sense.

## Middleware-Based Error Handling

The classic way to catch unhandled exceptions is with custom [**middleware**](3-ways-to-create-middleware-in-asp-net-core). This is where most of us start, and honestly, it still works great for most scenarios.

```csharp
internal sealed class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred");

            // Make sure to set the status code before writing to the response body
            context.Response.StatusCode = ex switch
            {
                ApplicationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            await context.Response.WriteAsJsonAsync(
                new ProblemDetails
                {
                    Type = ex.GetType().Name,
                    Title = "An error occured",
                    Detail = ex.Message
                });
        }
    }
}
```

Don't forget to add the middleware to the request pipeline:

```csharp
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
```

This approach is solid and works everywhere in your pipeline. The beauty is its simplicity: wrap everything in a try-catch, log the error, and return a consistent response.

But once you start adding specific rules for different exception types (e.g. `ValidationException`, `NotFoundException`), this becomes a mess. You end up with long `if` / `else` chains or more abstractions to handle each exception type.

Plus, you're manually crafting JSON responses, which means you're probably not following [RFC 9457 (Problem Details)](https://www.rfc-editor.org/rfc/rfc9457) standards.

## Enter IProblemDetailsService

Microsoft recognized this pain point and gave us `IProblemDetailsService` to standardize error responses. Instead of manually serializing our own error objects, we can use the built-in Problem Details format.

```csharp
internal sealed class GlobalExceptionHandlerMiddleware(
    RequestDelegate next,
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandlerMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unhandled exception occurred");

            // Make sure to set the status code before writing to the response body
            context.Response.StatusCode = ex switch
            {
                ApplicationException => StatusCodes.Status400BadRequest,
                _ => StatusCodes.Status500InternalServerError
            };

            await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = new ProblemDetails
                {
                    Type = exception.GetType().Name,
                    Title = "An error occured",
                    Detail = exception.Message
                }
            });
        }
    }
}
```

This is much cleaner. We're now using a standard format that API consumers expect, and we're not manually fiddling with JSON serialization. But we're still stuck with that growing switch statement problem. [**You can learn more about using Problem Details in .NET here**](problem-details-for-aspnetcore-apis).

## The Modern Way: IExceptionHandler

ASP.NET Core 8 introduced `IExceptionHandler`, and it's a game-changer. Instead of one massive middleware handling everything, we can create focused handlers for specific exception types.

Here's how it works:

```csharp
internal sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "Unhandled exception occurred");

        httpContext.Response.StatusCode = exception switch
        {
            ApplicationException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Type = exception.GetType().Name,
                Title = "An error occured",
                Detail = exception.Message
            }
        });
    }
}
```

The key here is the return value. If your handler can deal with the exception, return `true`. If not, return `false` and let the next handler try.

Don't forget to register it with DI and the request pipeline:

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// And in your pipeline
app.UseExceptionHandler();
```

This approach is so much cleaner. Each handler has one job, and the code is easy to test and maintain.

## Chaining Exception Handlers

You can chain multiple [**exception handlers**](global-error-handling-in-aspnetcore-8) together, and they'll run in the order you register them. ASP.NET Core will use the first one that returns `true` from `TryHandleAsync`.

Example: One for validation errors, one global fallback.

```csharp
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
```

Let's say you're using [FluentValidation](https://fluentvalidation.net/) (and you should be). Here's a complete setup:

```csharp
internal sealed class ValidationExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<ValidationExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not ValidationException validationException)
        {
            return false;
        }

        logger.LogError(exception, "Unhandled exception occurred");

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        var context = new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Detail = "One or more validation errors occurred",
                Status = StatusCodes.Status400BadRequest
            }
        };

        var errors = validationException.Errors
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key.ToLowerInvariant(),
                g => g.Select(e => e.ErrorMessage).ToArray()
            );
        context.ProblemDetails.Extensions.Add("errors", errors);

        return await problemDetailsService.TryWriteAsync(context);
    }
}
```

And in your app, just throw like this:

```csharp
// In your controller or service - IValidator<CreateUserRequest>
public async Task<IActionResult> CreateUser(CreateUserRequest request)
{
    await _validator.ValidateAndThrowAsync(request);

    // Your business logic here
}
```

The execution order is important. The framework will try each handler in the order you registered them. So put your most specific handlers first, and your catch-all handler last.

## Summary

We've come a long way from the days of manually crafting error responses in middleware. The evolution looks like this:

*   [**Middleware**](3-ways-to-create-middleware-in-asp-net-core): Simple, works everywhere, but gets complex fast
*   [**IProblemDetailsService**](problem-details-for-aspnetcore-apis): Standardizes response format, still manageable
*   [**IExceptionHandler**](global-error-handling-in-aspnetcore-8): Modern, testable, and scales beautifully

For new projects, I'd go straight to `IExceptionHandler`. It's cleaner, more maintainable, and gives you the flexibility to handle different exception types exactly how you want.

The key takeaway? Don't let error handling be an afterthought. Set it up early, make it consistent, and your users (and your future self) will thank you when things inevitably go wrong.

Thanks for reading.

And stay awesome!