```yaml
---
title: Handling Exceptions with IExceptionHandler in ASP.NET Core 8
source: https://okyrylchuk.dev/blog/handling-exceptions-in-asp-net-core-8/
date_published: 2024-01-26T08:37:55.000Z
date_captured: 2025-08-06T18:29:19.052Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [ASP.NET Core 8, .NET 8, Minimal APIs, ProblemDetails, IExceptionHandler, IMiddleware, HTTP API, RFC standards]
programming_languages: [C#]
tags: [exception-handling, asp.net-core, .net-8, middleware, minimal-apis, error-handling, web-api, http-status-codes, dependency-injection]
key_concepts: [exception handling, problem details, middleware pattern, IExceptionHandler interface, custom exceptions, HTTP status codes, dependency injection, API error responses]
code_examples: false
difficulty_level: intermediate
summary: |
  [This post details three methods for handling custom exceptions in ASP.NET Core 8, particularly within Minimal APIs. It starts by showing how raw exceptions expose sensitive information and then introduces `ProblemDetails` for generating standardized, secure error responses. The article then demonstrates using custom middleware as an alternative for handling exceptions, noting its improved code organization. Finally, it highlights the new `IExceptionHandler` interface in ASP.NET Core 8 as a more modern and flexible approach, explaining its chaining capabilities and distinction from `IExceptionFilter` in MVC.]
---
```

# Handling Exceptions with IExceptionHandler in ASP.NET Core 8

# Handling Exceptions with IExceptionHandler in ASP.NET Core 8

In this post, I’ll describe three ways to handle custom exceptions in ASP.NET Core 8

For my examples, I use **Minimal APIs**.

Let’s create a simple endpoint for getting a user by ID that throws **NotFoundException**.

```csharp
app.MapGet("/user/{id}", (int id) =>
{
    throw new NotFoundException(id, "User");
});
```

The implementation of **NotFoundException** is following.

```csharp
public class NotFoundException : Exception
{
    public NotFoundException(int id, string name) 
        : base($"{name} with id {id} not found.")
    {
    }
}
```

When we request our endpoint, you'll get an Internal Server Error with a stack trace.

![Screenshot showing an HTTP 500 Internal Server Error response with a detailed stack trace in the response body.](https://ci3.googleusercontent.com/meips/ADKq_NYHLO2QXzQyj8GLMgjt3AS9hSgtpMDkON5b36wSW3Z3ONCu0VDriAVJdSxB3SoHaRK8qJ8Y0-crBdZF9_worgTRNpE3kOlRZSnj8xKxdJeEGDC3UnEieNbyvFNclSLAzA5-5kXFaUwAPpKailPcE8Bb5fdgo3lUpAE3Po6dgqMHfyjWOOTQqoM0mw=s0-d-e1-ft#https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1706133828998-stack_trace.png)

We don’t want to return our stack trace because it can expose sensitive information about our server and potentially compromise its security.

## Problem Details

We can use the **AddProblemDetails** service to create a **ProblemDetails** object for failed requests. The **ProblemDetails** class is a machine-readable format for specifying errors in HTTP API responses based on [RFC standards](https://datatracker.ietf.org/doc/html/rfc7807 "RFC standards").

```csharp
builder.Services.AddProblemDetails();
```

Let’s execute our endpoint again and see the response.

![Screenshot showing an HTTP 500 Internal Server Error response formatted as Problem Details, still including the exception details.](https://ci3.googleusercontent.com/meips/ADKq_NaImzhBxzhrVQIlHeUFDJ_u8Y7zBJQjkhWPVi2KuEpKXu7GrRReey_jYc9dNOs4m-_rNw7AiNAOMOg0rt3J3I1skwJtL8ozJfDZaZEkf8pevu_xtfiP62XBjXsrtw5O_O5UTJcbKBZDGzhuE6FZUWqFJfiatEs3qK45L6r-DgLTGabeRBXAFAsXZ0poSbsSbXpjB-ruyFf7-n1MnAie=s0-d-e1-ft#https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1706135664338-problem_details_with_exception.png)

The response changed, but we still see the stack trace in the exception property. You can customize problem details. Let’s do that.

```csharp
builder.Services.AddProblemDetails(opt =>
    opt.CustomizeProblemDetails = context => context.ProblemDetails
        = new ProblemDetails 
        { 
            Detail = context.ProblemDetails.Detail,
            Status = context.ProblemDetails.Status,
            Title = context.ProblemDetails.Title,
            Type = context.ProblemDetails.Type
        });
```

The response is the following.

![Screenshot showing an HTTP 500 Internal Server Error response with customized Problem Details, where the stack trace is removed.](https://ci3.googleusercontent.com/meips/ADKq_Nbzge8cM6qjzRXYuutSjDtZkX5wPF7pnKMLAF_feO68emzYqC9Hbo95_Y26TsTPlkBwnZ9t55Gl7ajzFwXzYFDr3naH25ijEinaTvaG-Jb4ysY0pD5EEOO3CjFvQrg7eh8CqVJnQ0f6pD2_xPJw5NPNibKv5ROgHv1cD7ZQzV61t7fO5SbxlyQ55C2hSMXjc_DLkhca2vF3E3Q=s0-d-e1-ft#https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1706135939162-customized_problem_details.png)

Yeah! We have standardized responses according to RFC standards. We hid the stack trace.

The first issue is that the customization of **ProblemDetails** could be more handy. The second issue is that we get an Internal Server Error but want to get a Not Found 404 HTTP code.

We can handle our custom exceptions using **ProblemDetails**.

```csharp
builder.Services.AddProblemDetails(opt =>
    opt.CustomizeProblemDetails = context =>
        {
            if (context.Exception is NotFoundException notFoundException)
            {
                context.ProblemDetails = new ProblemDetails 
                {
                    Detail = notFoundException.Message,
                    Status = StatusCodes.Status404NotFound,
                    Title = "Resource not found",
                    Type = context.ProblemDetails.Type
                };
            }

            context.HttpContext.Response.StatusCode 
                = context.ProblemDetails.Status.Value;
        });
```

The response is the following.

![Screenshot showing an HTTP 404 Not Found response with Problem Details, correctly reflecting the custom NotFoundException.](https://ci3.googleusercontent.com/meips/ADKq_NavJiSs75R3V8pRf_1CjMibnkJd1j3GQ3t97IN2OI2ah87PAF3_220rd0OEPrAieoMr4iHfMQyb3WVM3Eth9iTIPIu-xHKSvO2OyYt1zwHUlhNTWvMbFUm87aThyYJPfVr1nNRaaYNOW4XeAflDzJsiI9cI66AS4pf7jSrNKsXHipLxaClxpdGtUlrO10NOJGEib_iRS-cYDg=s0-d-e1-ft#https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1706208082519-problem_details_not_found.png)

Finally, we handled our custom exception!

You can notice that the syntax is not very readable. Of course, you can move the implementation to a separate method or class.

There is a little bit better solution.

## Middleware

The different way to handle custom exceptions in ASP.NET Core is middleware.

Middleware refers to components that handle requests and responses as they move through an ASP.NET Core application’s pipeline. The middleware components are arranged in a pipeline, each performing a specific function in processing the HTTP request or response. Middleware components in ASP.NET Core can perform tasks such as authentication, authorization, logging, routing, and more.

So, let’s use the middleware to handle our exceptions. First, we need to create a middleware. There are a few ways to do that. You can build it using delegates, conventions, or a middleware factory. Read more in “[3 Methods to Create Middleware in ASP.NET Core](/blog/three-methods-to-create-middleware-in-asp-net-core/ "3 Methods to Create Middleware in ASP.NET Core")“.

I prefer a middleware factory. Therefore, I implement an **IMiddleware** interface.

```csharp
public class ExceptionHandlingMiddleware : IMiddleware
{ 
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        try
        {
            await next(context);
        }
        catch (NotFoundException ex)
        {
            var problemDetails = new ProblemDetails
            {
                Detail = ex.Message,
                Status = StatusCodes.Status404NotFound,
                Title = "Resource not found",
                Type = ex.GetType().Name
            };

            context.Response.StatusCode = problemDetails.Status.Value;

            await context.Response.WriteAsJsonAsync(problemDetails);
        }
    }
}
```

We need to register the created middleware in the DI and use it in our application.

```csharp
builder.Services.AddTransient<ExceptionHandlingMiddleware>();

app.UseMiddleware<ExceptionHandlingMiddleware>();
```

The response is the following.

![Screenshot showing an HTTP 404 Not Found response, handled by custom middleware, returning Problem Details.](https://ci3.googleusercontent.com/meips/ADKq_NYx5UBXfH183G1BrBULCv0izo2v0V_zk-BwIhA5OWwxED2ykt4GuWdWVzt3_Z1CND9lEyNhFlranMRQJXEoQqlVLF304fEDg6Yfama6EnlzQjHzW8vjpRD0D3dzTLI-sAnBzFSQM3UkmseKFCfFpzYBQ2XRN4XeSZRpejBjH_vc3CQ2sAjOKArTN2Snckb-zr2=s0-d-e1-ft#https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1706210391548-middleware_response.png)

The implementation is the same as that of **ProblemDetails** customization. However, the code is more readable, as it’s in a separate class by design, and it’s easier to debug.

However, we have to catch our exception. In the real application, there will be many custom exceptions. The implementation of catching different exceptions will grow. We can create many middlewares each to catch specific exceptions. But everywhere we should catch it.

Let’s see what new ASP.NET Core 8 brings to us.

## **IExceptionHandler**

ASP.NET Core 8 introduces **IExceptionHandler**. It represents an interface for handling exceptions.

**Please don’t confuse it with IExceptionFilter in ASP.NET Core MVC!** You can use it only in MVC apps. The **IExceptionHandler** belongs to ASP.NET Core.

Let’s create our exception handler.

```csharp
public class NotFoundExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        if (exception is NotFoundException)
        {
            var problemDetails = new ProblemDetails
            {
                Detail = exception.Message,
                Status = StatusCodes.Status404NotFound,
                Title = "Resource not found",
                Type = exception.GetType().Name
            };

            context.Response.StatusCode = problemDetails.Status.Value;

            await context.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        return false;
    }
}
```

The implementation is similar, but we don’t need to catch the exception. We only need to check the type of our exception.

As middlewares, we need to register our exception handler and use it.

```csharp
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();

app.UseExceptionHandler(_ => { });
```

If you don’t have any configuration for the exception handler, you can get rid of the ugly **\_ => {}** by registering **ProblemDetails** as it was shown in the section with **ProblemDetails**.

The difference you can spot is that the return value is **bool**. If we return **true**, the exception’s processing stops. If we return **false**, the pipeline will continue processing its execution and invoke the next Exception handler.

It means you can chain many exception handlers. They process exceptions one by one until one of them returns **true**. **So the registration order matters!** If you have the global exception handler for unhandled exceptions, it must be registered as the last one.

```csharp
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<UnhandledExceptionHandler>();
```

## Summary

In this post, I described three ways to handle exceptions in the ASP.NET Core.

The **IExceptionHandler** is a brand-new way to do that in ASP.NET Core 8.

However, it’s not all ways to handle exceptions in ASP.NET Core. You can read more about that in the [Microsoft Documentation](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-8.0 "Microsoft Documentation").