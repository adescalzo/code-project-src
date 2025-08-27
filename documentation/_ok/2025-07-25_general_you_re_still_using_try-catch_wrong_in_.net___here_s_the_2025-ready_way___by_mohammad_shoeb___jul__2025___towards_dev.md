```yaml
---
title: "You’re Still Using Try-Catch Wrong in .NET — Here’s the 2025-Ready Way | by Mohammad Shoeb | Jul, 2025 | Towards Dev"
source: https://towardsdev.com/youre-still-using-try-catch-wrong-in-net-here-s-the-2025-ready-way-66f6f2a810dd
date_published: 2025-07-25T11:12:45.233Z
date_captured: 2025-08-12T21:01:44.164Z
domain: towardsdev.com
author: Mohammad Shoeb
category: general
technologies: [.NET, ASP.NET Core, Polly, HTTP Client, ILogger, ProblemDetails middleware, IExceptionHandlerFeature, ExceptionDispatchInfo, W3C Trace-Context]
programming_languages: [C#]
tags: [exception-handling, dotnet, error-management, best-practices, resilience, middleware, logging, anti-patterns, csharp, software-design]
key_concepts: [exception handling, try-catch, exception filters, defensive programming, domain-specific exceptions, middleware error handling, resilience patterns, logging]
code_examples: false
difficulty_level: intermediate
summary: |
  The article critiques common `try-catch` anti-patterns in .NET, such as generic catch-all blocks that hide bugs and lead to silent failures. It proposes "2025-ready" solutions for robust exception management. Key recommendations include using `when` filters for precise exception handling, employing defensive programming to prevent exceptions, and defining domain-specific exceptions for clarity. The author also advises leveraging ASP.NET Core middleware for centralized error responses and implementing layered resilience with libraries like Polly. The core message emphasizes treating `try-catch` as a precision tool to ensure applications fail loudly and recover gracefully, rather than silently ignoring issues.
---
```

# You’re Still Using Try-Catch Wrong in .NET — Here’s the 2025-Ready Way | by Mohammad Shoeb | Jul, 2025 | Towards Dev

# You’re Still Using Try-Catch Wrong in .NET — Here’s the 2025-Ready Way

**You added** `**try-catch**` **thinking it would protect your app.**
Instead, it hid bugs, triggered retry storms, and made failures harder to trace.

In .NET, `try-catch` isn’t always your friend — sometimes, it’s the reason your system quietly breaks.

Let’s fix that.
Because handling exceptions isn’t just about catching errors — it’s about building apps that fail loudly, recover gracefully, and never leave you guessing.

# 🚨 Real Problem

```csharp
try
{
    var user = await _userService.GetUserAsync(id);
    _logger.LogInformation("Fetched user");
}
catch (Exception ex)
{
    // Generic catch-all
    _logger.LogError(ex, "Something went wrong");
}
```

✅ Looks safe
❌ Silently hides root cause
⚠️ Violates separation of concerns
💥 Leads to retry loops, missing metrics, and impossible debugging

# ❌ 1. Top 5 Try-Catch Anti-Patterns (with Fixes)

![Table showing 5 common try-catch anti-patterns, their fallout, and 2025-ready fixes. Anti-patterns include blanket catch, log-and-ignore, retry-inside-catch, exception-driven flow, and async void with unobserved tasks.](https://miro.medium.com/v2/resize:fit:700/1*mGxk5lSxFpbtz0NWLwf7Dg.png)

# ✅ 2. The Recommended Fixes (2025-Ready)

# ✔️ a. Use `when` Filters

```csharp
try
{
    // some logic
}
catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
{
    _logger.LogWarning("User not found");
}
```

> _🔗_ [_Microsoft Docs — Exception Filters_](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/using-user-filtered-exception-handlers)

**Why it works:**
Avoids unnecessary catch for unrelated errors — filters keep your handling **precise and intentional**.

📝 **Note:** It’s best to filter and catch only exceptions you can meaningfully handle, use this with caution. However, in rare cases — like unknown library failures — a broad `try-catch` for log-and-rethrow can help capture diagnostics without suppressing critical issues.

# ✔️ b. Don’t Catch What You Can Prevent

```csharp
if (!File.Exists(path))
{
    _logger.LogWarning("File missing: {path}", path);
    return;
}
// instead of blindly catching FileNotFoundException
```

> _📌 Defensive programming wins over exception-based control flow. This is an option, consider using it if it makes sense in your application._

📌 This approach avoids the overhead of exception handling and makes intent explicit — use it when it improves readability or performance, especially in IO or file-based scenarios.

# ✔️ c. Create Domain-Specific Exceptions

```csharp
public class PaymentDeclinedException : Exception
{
    public PaymentDeclinedException(string reason) : base(reason) { }
}
```

And catch only what you can act on:

```csharp
try
{
    await _paymentService.ProcessAsync();
}
catch (PaymentDeclinedException ex)
{
    _logger.LogWarning("Payment declined: {reason}", ex.Message);
    return BadRequest(ex.Message);
}
```

> 📌 Use domain-specific exceptions only for rare, non-happy-path conditions that are truly exceptional in your domain — not for regular control flow.
>
> _📚 Improves clarity, testability, and recovery logic._
>
> _🔗_ [_Microsoft Docs — User Defined Exception_](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/how-to-create-user-defined-exceptions)

# ✔️ d. Let the Middleware Handle It

Use `.UseExceptionHandler()` or ProblemDetails middleware.

```csharp
app.UseExceptionHandler("/error");
```

Then centralize your error responses:

```csharp
app.Map("/error", (HttpContext context) =>
{
    var feature = context.Features.Get<IExceptionHandlerFeature>();
    return Results.Problem(detail: feature?.Error.Message);
});
```

> _🔗_ [_Microsoft Docs — Middleware Error Handing_](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling?view=aspnetcore-9.0)

# ✔️ e. Avoid Swallowing in Background Jobs

```csharp
try
{
    await _processor.RunAsync();
}
catch (Exception ex)
{
    _logger.LogCritical(ex, "Job failed");
    throw; // Don’t swallow — let orchestration retry or fail visibly
}
```

Consider rethrow exceptions in background tasks so orchestrators (like Azure Functions or Hangfire) can detect failures, trigger retries, or alert — silent failures are dangerous and invisible.

> _This is an option, consider using it if it makes sense in your application. Not advising to use it all the times._

# ✔️ f. Layered Resilience (Polly)

```csharp
services.AddHttpClient("Users")
        .AddTransientHttpErrorPolicy(p =>
            p.WaitAndRetryAsync(3, retry => TimeSpan.FromSeconds(Math.Pow(2, retry))));
```

Add retry, timeout, and fallback policies at the HTTP client layer to handle transient faults cleanly without cluttering your business logic.

# 🛠 Pro‑Tier Techniques

![Table showing advanced exception handling techniques. Scenarios include preserving stack when re-throwing using ExceptionDispatchInfo, linking logs across services with W3C Trace-Context, and catching once to observe many using ILogger.BeginScope().](https://miro.medium.com/v2/resize:fit:1000/1*ACjQ82BGpFWMi5-aJwwZig.png)

# ✅ Bonus: Production-Ready Exception Handling Checklist

✅ Use filters instead of broad catch
✅ Don’t catch what you can’t recover from
✅ Use domain-specific exceptions
✅ Centralize logging and response generation
✅ Always rethrow if your system can’t act on it
✅ Avoid async void + unhandled task exceptions
✅ Don’t retry inside catch blocks blindly — use Polly

# 💬 Final Thoughts

**Try-catch isn’t a safety net — it’s a precision tool. Use it with intention.**
Most .NET outages I’ve dealt with didn’t happen because an exception was left uncaught — they happened because the exception was caught, logged, and quietly ignored.
No alerts. No retries. Just silent failure until users noticed.

# 📚 Further Reading

1.  [Best practices for exceptions (.NET)](https://learn.microsoft.com/dotnet/standard/exceptions/best-practices-for-exceptions)
2.  [ProblemDetails middleware](https://learn.microsoft.com/aspnet/core/fundamentals/error-handling)
3.  [Polly resilience strategies](https://github.com/App-vNext/Polly)