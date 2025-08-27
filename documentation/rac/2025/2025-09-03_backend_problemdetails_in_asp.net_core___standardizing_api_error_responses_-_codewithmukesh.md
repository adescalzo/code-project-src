```yaml
---
title: ProblemDetails in ASP.NET Core – Standardizing API Error Responses - codewithmukesh
source: https://codewithmukesh.com/blog/problem-details-in-aspnet-core/?utm_source=convertkit&utm_medium=email&utm_campaign=Stop%20returning%20random%20error%20responses%20in%20your%20APIs!%20-%2018859426
date_published: 2025-09-04T00:00:00.000Z
date_captured: 2025-09-04T11:43:54.539Z
domain: codewithmukesh.com
author: Unknown
category: backend
technologies: [ASP.NET Core, .NET, RFC 7807, IExceptionHandler, IProblemDetailsService, ValidationProblemDetails, ProblemDetailsContext, IHostEnvironment, StatusCodes]
programming_languages: [C#, JSON]
tags: [api, error-handling, aspnet-core, dotnet, web-api, rfc-7807, http, exception-handling, validation, middleware]
key_concepts: [standardized-error-responses, problem-details, exception-handling, global-exception-handler, validation-errors, http-status-codes, api-design, middleware, problem-details-extensions]
code_examples: false
difficulty_level: intermediate
summary: |
  This article details the importance of standardized API error responses using ProblemDetails in ASP.NET Core. It explains RFC 7807, the IETF standard for problem details, and demonstrates how to implement it for various error scenarios. The content covers enabling ProblemDetails, customizing responses globally, handling unhandled exceptions with `IExceptionHandler`, and managing validation errors using `ValidationProblemDetails`. Emphasizing consistency and security, the article provides practical code examples and best practices for building developer-friendly APIs.
---
```

# ProblemDetails in ASP.NET Core – Standardizing API Error Responses - codewithmukesh

# ProblemDetails in ASP.NET Core – Standardizing API Error Responses

---

When you build APIs, you focus a lot on the happy paths — returning the right data, keeping performance high, making endpoints clean. But the reality is, **errors are just as important as successful responses**.

Think about a client application consuming your API: a mobile app, a web frontend, or even another microservice. If every endpoint throws errors in different shapes, the client developer has to write special parsing logic for each scenario. That wastes time and increases bugs.

Standardized error responses solve this. Instead of guessing what an error looks like, the client can reliably expect a **predictable JSON schema**. This consistency improves:

*   **Debugging speed** → Errors are easier to read and trace.
*   **Client integration** → Frontends can handle errors with less boilerplate.
*   **Maintainability** → You don’t have to document 20 different error response styles.

APIs that use standardized error handling are **developer-friendly** — and that’s the kind of API people actually enjoy working with.

---

## The problem with custom error formats

I’ll be honest — I used to return random JSON objects for errors in my APIs. Something like:

```json
{  "message": "Something went wrong",  "status": 500}
```

On another endpoint, I’d return:

```json
{  "error": "User not found"}
```

And once, embarrassingly, I just returned a plain text string:

```
Invalid Request
```

It worked fine _for me_, but it was a nightmare for anyone consuming the API. Each endpoint had its own “style.” The frontend team constantly pinged me: _“What does this error mean? Why is this one a string and that one an object?”_

Custom error formats might feel fast in the beginning, but they quickly become **technical debt**. When your API grows, inconsistency multiplies, and suddenly you’re spending more time fixing error handling than building features.

That’s exactly why ASP.NET Core ships with **ProblemDetails** — a standard, RFC-compliant way of representing errors. It eliminates this chaos by enforcing a single, predictable structure for every error response.

---

## What is ProblemDetails in ASP.NET Core?

### The RFC 7807 standard explained

ProblemDetails is based on an IETF standard called **RFC 7807: Problem Details for HTTP APIs**.

Read more about it here: [Problem Details for HTTP APIs](https://www.rfc-editor.org/rfc/rfc7807.html).

Instead of every developer inventing their own error format, RFC 7807 defines a simple, predictable JSON schema that all APIs can use.

Here’s a typical ProblemDetails response for a `404 Not Found`:

```json
{  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",  "title": "Not Found",  "status": 404,  "detail": "The user with ID 123 was not found.",  "instance": "/api/users/123"}
```

Let’s break it down:

*   **type** → A URI that identifies the problem type. (Could point to documentation, or to the RFC section itself.)
*   **title** → A short, human-readable summary of the problem.
*   **status** → The HTTP status code (400, 404, 500, etc.).
*   **detail** → A human-readable explanation of what went wrong.
*   **instance** → The specific request path or resource that caused the error.

The key point: this structure is always the same, regardless of the actual error. Clients can safely rely on it.

Before I discovered RFC 7807, I was creating my own JSON schemas like this:

```json
{  "error": "Validation failed",  "fields": {    "email": "Invalid email format"  }}
```

It worked, but when the API grew, I had multiple “error shapes” floating around. With ProblemDetails, you get **one standard schema**, and you can extend it if needed.

![Flowchart showing how a Client Application interacts with an API Endpoint. If an exception occurs, an error is returned using IExceptionHandler, ProblemDetailsService, and RFC 7807 ProblemDetails. If no exception occurs, a success response is returned.](/_astro/error-diagram.BY7D0ZkF_Z1jhqNc.webp "ProblemDetails in ASP.NET Core")

---

### Built-in support in ASP.NET Core

The good news is you don’t need to implement RFC 7807 from scratch — ASP.NET Core already supports it out of the box.

*   If you return a `NotFound()`, `BadRequest()`, or any of the standard result helpers, ASP.NET Core automatically wraps them in a `ProblemDetails` response (in newer versions of .NET).
*   For validation errors, ASP.NET Core uses a special subclass called **ValidationProblemDetails**, which includes an `errors` dictionary alongside the standard fields.
*   You can also manually return a ProblemDetails response using `Results.Problem(...)` in minimal APIs or `ControllerBase.Problem(...)` in controllers.

## Enabling ProblemDetails in ASP.NET Core

ProblemDetails doesn’t kick in automatically. You have to enable it in your pipeline.

Open your `Program.cs` and add:

```csharp
var builder = WebApplication.CreateBuilder(args);
// Register ProblemDetails service
builder.Services.AddProblemDetails();
var app = builder.Build();
// Add status code pages (so even plain 404s / 500s return a body)
app.UseStatusCodePages();
app.Run();
```

That’s it. From this point, your app has the **ProblemDetails infrastructure** wired in.

---

### What those lines actually do

*   `AddProblemDetails()` This wires up the `IProblemDetailsService`, which ASP.NET Core uses internally to generate RFC-7807 compliant error responses.
*   `UseStatusCodePages()` Normally, if you just return a 404, you’ll only get the status code with an empty body. With this middleware, you’ll get an **actual JSON payload** for non-successful status codes.

👉 If you’re building **pure APIs**, some teams skip `UseStatusCodePages()` and rely only on `Results.Problem(...)` or exception handling. But for dev/test environments, it’s very handy.

---

## Returning a Simple Not Found

Let’s see what happens when you return a 404 in a minimal API:

```csharp
app.MapGet("/users/{id}", (int id) =>
{
    return Results.Problem(
        title: "User not found",
        detail: $"User with ID {id} does not exist.",
        statusCode: StatusCodes.Status404NotFound,
        instance: $"/users/{id}");
});
```

**Response (404):**

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
    "title": "User not found",
    "status": 404,
    "detail": "User with ID 10 does not exist.",
    "instance": "/users/10",
    "traceId": "00-92e1ee919f7be2d26ec1b93df13dffef-c6ec16cfe60c3fc4-00"
}
```

That’s a clean RFC-7807 shape, straight out of the box.

---

## Customizing the ProblemDetails Response Globally

Out of the box, ProblemDetails is minimal. In real projects you’ll want to add:

*   **Trace IDs** (to correlate with logs)
*   **Timestamps** (when the error occurred)
*   **Instance** (where the error occured)
*   **Mappings** for custom exceptions

You can do all of this in one place:

```csharp
builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        // Always include useful metadata
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
        ctx.ProblemDetails.Extensions["timestamp"] = DateTime.UtcNow;
        ctx.ProblemDetails.Instance = $"{ctx.HttpContext.Request.Method} {ctx.HttpContext.Request.Path}";
    };
});
```

And the Response now will be,

```json
{
    "type": "https://tools.ietf.org/html/rfc9110#section-15.5.5",
    "title": "User not found",
    "status": 404,
    "detail": "User with ID 10 does not exist.",
    "instance": "GET /users/10",
    "traceId": "0HNFB9VCIFIEF:00000002",
    "timestamp": "2025-09-04T01:10:44.4466116Z"
}
```

Now **every** ProblemDetails payload will carry extra context — without you touching each endpoint.

---

## How to use IExceptionHandler with ProblemDetails in .NET?

Returning `Results.Problem(...)` manually works, but for unhandled exceptions you don’t want random stack traces leaking out. Use a centralized exception handler:

```csharp
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IProblemDetailsService _problemDetails;

    public GlobalExceptionHandler(
        ILogger<GlobalExceptionHandler> logger,
        IProblemDetailsService problemDetails)
    {
        _logger = logger;
        _problemDetails = problemDetails;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // 1) Log with correlation
        _logger.LogError(exception, "Unhandled exception. TraceId: {TraceId}", httpContext.TraceIdentifier);

        // 2) Map exception → HTTP status + title/type
        var (status, title) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, "Resource Not Found"),
            ArgumentException => (StatusCodes.Status400BadRequest, "Invalid Request"),
            _ => (StatusCodes.Status500InternalServerError, "Server Error")
        };

        // 3) Build ProblemDetails (don’t leak internals in prod)
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Type = exception.GetType().Name,
            Detail = httpContext.RequestServices
                                 .GetRequiredService<IHostEnvironment>()
                                 .IsDevelopment()
                     ? exception.Message
                     : null,
            Instance = httpContext.Request.Path
        };

        // 4) Enrich universally useful metadata
        problem.Extensions["traceId"] = httpContext.TraceIdentifier;
        problem.Extensions["timestamp"] = DateTime.UtcNow;

        // 5) Write response
        await _problemDetails.WriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problem,
        });

        // Tell the pipeline we handled it
        return true;
    }
}
```

Also, add the following to your `Program.cs`

```csharp
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
app.UseExceptionHandler();
```

With this in place, your API never spits out HTML error pages or raw exceptions. Everything stays RFC-7807.

Note that now you can start throwing exception from your app or API layer, like,

```csharp
app.MapGet("/users/{id}", (int id) =>
{
    throw new KeyNotFoundException($"User with ID {id} was not found.");
});
```

and your responses will be shaped like,

```json
{
    "type": "KeyNotFoundException",
    "title": "Resource Not Found",
    "status": 404,
    "detail": "User with ID 10 was not found.",
    "instance": "/users/10",
    "traceId": "00-5f15926a7ab5b23e16d0e68d50ff7eff-5b37eddc805180a7-00",
    "timestamp": "2025-09-04T02:02:57.510385Z"
}
```

## Handling Validation Errors with ValidationProblemDetails

Validation is one of the most common error scenarios in APIs — missing fields, invalid formats, or values that don’t meet business rules. ASP.NET Core automatically returns these as **ValidationProblemDetails**, a subclass of ProblemDetails. This is what you get automatically when using data annotations like `[Required]`, `[EmailAddress]`, etc.

Here’s what happens when a request fails model validation in a controller or minimal API:

### Example Request

```http
POST /users
Content-Type: application/json

{  "email": "not-an-email",  "age": -1}
```

### Response (400 Bad Request)

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "email": [ "The Email field is not a valid e-mail address." ],
    "age": [ "The field Age must be between 1 and 120." ]
  },
  "traceId": "00-9acb0c02caba874bf1c23387d0d1e437-6f3bcd024fb84f42-00"
}
```

Notice a few things:

*   It follows the **ProblemDetails schema**.
*   There’s an extra `errors` property (dictionary of field → list of validation messages).
*   Clients can **easily bind field-level errors** to show in forms (e.g., highlight “email” and “age” fields).

### Returning ValidationProblemDetails Manually

You can also return validation errors explicitly from an endpoint:

```csharp
app.MapPost("/users", (User user) =>
{
    if (!user.Email.Contains("@"))
    {
        return Results.ValidationProblem(new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email must contain @ symbol." } }
        });
    }

    return Results.Ok(user);
});
```

This gives you a predictable 400 response in the same shape as automatic model validation.

### Why This Matters

*   Clients don’t have to guess error formats.
*   Field-level errors are structured and easy to consume.
*   Works consistently whether validation is automatic (via data annotations) or manual (via business rules).

## Best Practices for ProblemDetails

When using ProblemDetails, the most important rule is to **avoid leaking sensitive information**. Stack traces, SQL queries, or environment details should never appear in production responses. Use the `Detail` field only for safe, user-facing messages, and keep debugging information in logs.

Always use **consistent `type` URIs**. You don’t have to invent new ones for every error — reusing `https://httpstatuses.com/404` or creating stable URIs under your own docs works well. Clients should be able to rely on these identifiers without them changing every release.

ProblemDetails supports **extensions**, and that’s where you should add metadata like `traceId`, `timestamp`, or correlation IDs that make debugging easier. Just don’t overload it — keep responses lean so clients don’t need to parse unnecessary noise.

Differentiate between **expected errors** and **unexpected exceptions**. For known cases like “not found” or “bad request”, return `Results.Problem(...)` directly from the endpoint. For unhandled exceptions, rely on a global `IExceptionHandler` that maps exceptions to ProblemDetails. This keeps the flow predictable and consistent.

Finally, **test your error responses** just like your success responses. Integration tests should assert that `status`, `title`, and `type` are always present, and that custom fields like `traceId` exist when expected. This prevents accidental regressions and ensures clients never break due to inconsistent error shapes.

## Key Takeaways

*   **ProblemDetails is the standard** → It gives you predictable, RFC 7807–compliant error responses out of the box in ASP.NET Core.
*   **Enable it early** → Add `AddProblemDetails()` (and `UseExceptionHandler()` in .NET 9) to make every error consistent.
*   **Use the right tool** → Return `Results.Problem(...)` for expected errors, and rely on a global `IExceptionHandler` + `IProblemDetailsService` for unhandled exceptions.
*   **Customize once, apply everywhere** → Add `traceId`, `timestamp`, and `instance` globally via `CustomizeProblemDetails` so every response is enriched.
*   **Stay safe** → Never leak sensitive data in error payloads; keep details for logs, not clients.
*   **Test error responses** → Validate the shape (`status`, `title`, `type`) in integration tests, not just the happy path.

## Wrapping Up

Error handling isn’t the glamorous part of building APIs, but it’s what separates a messy backend from a **developer-friendly API** that people actually enjoy working with. By leaning on ProblemDetails, you remove guesswork, enforce consistency, and make your services easier to debug, integrate, and maintain.