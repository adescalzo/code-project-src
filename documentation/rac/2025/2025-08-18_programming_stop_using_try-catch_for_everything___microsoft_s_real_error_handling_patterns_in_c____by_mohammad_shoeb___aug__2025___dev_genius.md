```yaml
---
title: "Stop Using Try-Catch for Everything — Microsoft’s Real Error Handling Patterns in C# | by Mohammad Shoeb | Aug, 2025 | Dev Genius"
source: https://blog.devgenius.io/stop-using-try-catch-for-everything-microsofts-real-error-handling-patterns-in-c-c847085c6965
date_published: 2025-08-18T15:28:05.276Z
date_captured: 2025-08-22T11:19:03.627Z
domain: blog.devgenius.io
author: Mohammad Shoeb
category: programming
technologies: [ASP.NET Core, .NET, ProblemDetails, RFC 7807, IProblemDetailsService, Exception Handler Middleware, IExceptionHandler, ApiController, UseStatusCodePages, Roslyn, .editorconfig, Visual Studio, Task Parallel Library, ExceptionDispatchInfo, Microsoft.Extensions.Logging, HTTP, CancellationToken, HttpContext, StatusCodes, System.IO, ArgumentNullException, RequestFailedException, AggregateException, OperationCanceledException, IOException, InvalidOperationException, FileNotFoundException]
programming_languages: [C#]
tags: [error-handling, exceptions, dotnet, aspnet-core, best-practices, performance, code-quality, middleware, api, csharp]
key_concepts: [structured-exception-handling, over-catching, architectural-boundaries, problem-details, exception-middleware, typed-exception-handlers, fail-fast, async-error-handling, code-analysis, exception-performance]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article advocates for adopting Microsoft's recommended error handling patterns in C# and .NET, moving beyond the common anti-pattern of over-catching with generic `try-catch` blocks. It explains how improper exception handling can degrade performance and hide bugs. The author details modern ASP.NET Core error handling using `ProblemDetails`, `UseExceptionHandler`, and `IExceptionHandler` for consistent and diagnosable API responses. The post also outlines ten specific patterns, including handling common conditions, restoring state, failing fast, using specific exception types, and proper asynchronous error management. Finally, it emphasizes enforcing these best practices through `.editorconfig` and Roslyn analyzers to ensure code quality and consistency across development teams.]
---
```

# Stop Using Try-Catch for Everything — Microsoft’s Real Error Handling Patterns in C# | by Mohammad Shoeb | Aug, 2025 | Dev Genius

Member-only story

# Stop Using Try-Catch for Everything — Microsoft’s Real Error Handling Patterns in C#

[

![Mohammad Shoeb](https://miro.medium.com/v2/resize:fill:64:64/1*FZAGQUBsOxAr7U-m_cl9zQ.jpeg)

](https://medium.com/@mohsho10?source=post_page---byline--c847085c6965---------------------------------------)

[Mohammad Shoeb](https://medium.com/@mohsho10?source=post_page---byline--c847085c6965---------------------------------------)

Follow

6 min read

·

6 days ago

70

2

Listen

Share

More

Structured exception handling is powerful — but if you treat `try-catch` as your only error-handling tool, you’re silently breaking **performance** and **maintainability**.

Microsoft has clear, production-tested guidance on **when** and **where** to throw and catch exceptions — and it’s not what most developers do.

**Microsoft has quietly evolved how it wants you to handle exceptions in .NET 7/8 — and most developers haven’t caught up. In this post, I’ll show you 10 modern patterns straight from Microsoft’s own playbook, with real code examples you can use today.**

Not a medium member? you can read this blog [here](https://medium.com/@mohsho10/stop-using-try-catch-for-everything-microsofts-real-error-handling-patterns-in-c-c847085c6965?sk=60fec67139488a8e6ba98a4fad2fba51).

# 🧠 Why This Blog Matters

You’ve seen code like this:

```csharp
try  
{  
    var customer = repository.GetCustomer(id);  
}  
catch (Exception ex)  
{  
    Logger.LogError(ex, "Something went wrong");  
}
```

Looks harmless?  
[Microsoft](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions) calls this **over-catching** — and it’s one of the most common anti-patterns in real .NET code.

Handled wrong, exceptions will:

*   Hurt performance (stack unwinding and metadata lookup is expensive)
*   Hide bugs (swallowing `Exception` masks root causes)
*   Make error handling unpredictable

# 🏛 Where Exceptions Belong — Microsoft’s Architectural Boundaries

Different layers follow **different exception policies**.

![Diagram illustrating Microsoft Exception Boundaries in Architecture and an Exception Decision Flow. The architectural boundaries show an API Layer mapping exceptions to HTTP responses, flowing to a Domain Layer that throws domain-specific exceptions, which then flows to a Global Handler for centralized logging and telemetry. The decision flow starts with "Expected failure?". If yes, "Use TryXxx API". If no, it asks "Is this recoverable here?". If yes, "Catch specific exception". If no, "Let it bubble to boundary handler."](https://miro.medium.com/v2/resize:fit:700/1*mqektzQSTwwu4TbbV5IV3Q.png)

# 🌐 Modern ASP.NET Core Error Handling with ProblemDetails (Microsoft’s Way)

In **ASP.NET Core 8/9**, Microsoft’s recommended approach to API error handling combines **four interconnected patterns**. Each can stand alone, but together they give you a clean, consistent, and diagnosable error-handling pipeline.

# 1️⃣ Standardize with ProblemDetails

`ProblemDetails` implements the [RFC 7807](https://datatracker.ietf.org/doc/html/rfc7807) format for consistent, machine-readable errors.

```csharp
builder.Services.AddControllers();  
builder.Services.AddProblemDetails(); // Registers IProblemDetailsService
```

*   One error contract for all responses
*   Central place to customize formatting

📄 [Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-9.0#problem-details)

# 2️⃣ Centralize with UseExceptionHandler

Place **Exception Handler Middleware** early to catch unhandled exceptions and return a standardized error.

```csharp
if (!app.Environment.IsDevelopment())  
{  
    app.UseExceptionHandler(); // Global handler  
    app.UseHsts();  
}
```

*   Keeps controllers clean
*   Ensures uniform responses and logging

📄 [Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-9.0#problem-details)

# 3️⃣ Translate with Typed IExceptionHandler

From .NET 8+, implement `IExceptionHandler` for domain-specific exception handling.

```csharp
public sealed class InvalidOrderStateHandler : IExceptionHandler  
{  
    public async ValueTask<bool> TryHandleAsync(HttpContext ctx, Exception ex, CancellationToken ct)  
    {  
        if (ex is InvalidOrderStateException orderEx)  
        {  
            var pd = new ProblemDetails  
            {  
                Title = "Invalid order state",  
                Status = StatusCodes.Status400BadRequest,  
                Detail = orderEx.Message,  
                Instance = ctx.Request.Path  
            };  
            ctx.Response.StatusCode = 400;  
            await ctx.Response.WriteAsJsonAsync(pd, ct);  
            return true;  
        }  
        return false;  
    }  
}
```

*   Separation of concerns (business code throws, handler formats)
*   Multiple handlers can be registered

📄 [Docs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/error-handling?view=aspnetcore-9.0#iexceptionhandler)

# 4️⃣ Handle Expected Client Errors Automatically

*   `[ApiController]` auto-returns HTTP 400 for model validation failures — no exceptions needed.
*   `UseStatusCodePages()` can add simple bodies for empty 4xx/5xx responses.

```csharp
app.UseStatusCodePages(); // Optional
```

📄 [Docs](https://learn.microsoft.com/en-us/aspnet/core/web-api/?view=aspnetcore-8.0#automatic-http-400-responses)

# 🛠 How They Work Together

1.  **ProblemDetails** — defines the contract.
2.  **UseExceptionHandler** — globally captures unexpected errors.
3.  **IExceptionHandler** — translates known exceptions to clear, typed responses.
4.  **\[ApiController\] & StatusCodePages** — handle expected client errors gracefully.

✅ **Takeaway:** This layered approach gives you **predictable**, **uniform**, and **diagnosable** error handling that scales from small APIs to enterprise systems — exactly how Microsoft does it.

# 📏 Microsoft’s Core Rule: Exceptions Are for Exceptional Cases

From [Microsoft Docs:](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

> _“Use exceptions for exceptional conditions. Don’t use exceptions for regular control flow.”_

✅ **Good**: Network outage, corrupted file, DB unavailable  
❌ **Bad**: Checking if a file exists, parsing user input, validating a model

# 📏 Pattern 1 — Handle common conditions to avoid exceptions

For conditions that are likely to occur but might trigger an exception, consider handling them in a way that avoids the exception. For example, if you try to close a connection that’s already closed, you’ll get an `InvalidOperationException`. You can avoid that by using an `if` statement to check the connection state before trying to close it.

```csharp
if (conn.State != ConnectionState.Closed)  
{  
    conn.Close();  
}
```

If you don’t check the connection state before closing, you can catch the `InvalidOperationException` exception.

```csharp
try  
{  
    conn.Close();  
}  
catch (InvalidOperationException ex)  
{  
    Console.WriteLine(ex.GetType().FullName);  
    Console.WriteLine(ex.Message);  
}
```

From [Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

# 📏 Pattern 2— Restore state when methods don’t complete due to exceptions

```csharp
private static void TransferFunds(Account from, Account to, decimal amount)  
{  
    string withdrawalTrxID = from.Withdrawal(amount);  
    try  
    {  
        to.Deposit(amount);  
    }  
    catch  
    {  
        from.RollbackTransaction(withdrawalTrxID);  
        throw;  
    }  
}
```

From [Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

# 📏 Pattern 3— Fail Fast, Fail Loud

From [ASP.NET Core source](https://github.com/dotnet/aspnetcore):

```csharp
try  
{  
    _processor.Run();  
}  
catch (IOException ioEx)  
{  
    _logger.LogError(ioEx, "I/O error while processing");  
    throw; // Preserves stack trace  
}
```

# 📏 Pattern 4— Validate Before You Throw

From [Microsoft’s best practices:](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

```csharp
if (!File.Exists(path))  
{  
    _logger.LogWarning("File missing: {Path}", path);  
    return;  
}  
  
var data = File.ReadAllText(path);
```

# 📏 Pattern 5— Don’t Catch `System.Exception` (Except at Boundaries)

From [Design Guidelines:](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

```csharp
app.Use(async (context, next) =>  
{  
    try  
    {  
        await next();  
    }  
    catch (Exception ex)  
    {  
        logger.LogError(ex, "Unhandled exception");  
        throw;  
    }  
});
```

# 📏 Pattern 6— Use Specific Exception Types & Filters

.NET has hundreds of built-in exception types (list).

```csharp
catch (RequestFailedException ex) when (ex.Status == 404)  
{  
    _logger.LogInformation("Blob not found");  
}
```

**Why filters?**

*   Checked **before** stack unwind
*   Great for logging/metrics without catching
*   Exceptions inside filters are swallowed → handle carefully

# 📏 Pattern 7— Throw Early, Not Late

From [Framework Guidelines:](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/exception-handling-statements)

```csharp
public void SendEmail(string address)  
{  
    ArgumentNullException.ThrowIfNull(address);  
}
```

# 📏 Pattern 8— Skip Exceptions in Hot Paths

From [.NET Performance Docs:](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

```csharp
if (int.TryParse(input, out var num))  
{  
    // use num  
}  
else  
{  
    num = 0;  
}
```

# 📏 Pattern 9— Handle Async & Cancellation Properly

From [Task Exception Handling](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/exception-handling-task-parallel-library):

```csharp
try  
{  
    await Task.WhenAll(task1, task2);  
}  
catch (Exception ex)  
{  
    foreach (var inner in (ex as AggregateException)?.InnerExceptions ?? new[] { ex })  
    {  
        _logger.LogError(inner, "Task failed");  
    }  
}
```

**Guidance:**

*   Always `await` to observe exceptions
*   Use `AggregateException` with parallel tasks
*   Treat `OperationCanceledException` as **non-error**
*   Avoid blocking on `.Result` or `.Wait()`

# 📏 Pattern 10— Preserve Stack Across Threads

From [ExceptionDispatchInfo](https://gist.github.com/MohammadShoeb-cmd/42c2e9a176a3545becaac1b82d17d82f) and [Microsoft Docs](https://learn.microsoft.com/en-us/dotnet/standard/exceptions/best-practices-for-exceptions)

```csharp
ExceptionDispatchInfo? edi = null;  
try  
{  
    var txt = File.ReadAllText(@"C:\\temp\\file.txt");  
}  
catch (FileNotFoundException e)  
{  
    edi = ExceptionDispatchInfo.Capture(e);  
}  
  
// ...  
  
Console.WriteLine("I was here.");  
  
if (edi is not null)  
    edi.Throw();
```

If the file in the example code doesn’t exist, the following output is produced:

```
I was here.  
Unhandled exception. System.IO.FileNotFoundException: Could not find file 'C:\\temp\\file.txt'.  
File name: 'C:\\temp\\file.txt'  
   at Microsoft.Win32.SafeHandles.SafeFileHandle.CreateFile(String fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options)  
   at Microsoft.Win32.SafeHandles.SafeFileHandle.Open(String fullPath, FileMode mode, FileAccess access, FileShare share, FileOptions options, Int64 preallocationSize, Nullable`1 unixCreateMode)  
   at System.IO.Strategies.OSFileStreamStrategy..ctor(String path, FileMode mode, FileAccess access, FileShare share, FileOptions options, Int64 preallocationSize, Nullable`1 unixCreateMode)  
   at System.IO.Strategies.FileStreamHelpers.ChooseStrategyCore(String path, FileMode mode, FileAccess access, FileShare share, FileOptions options, Int64 preallocationSize, Nullable`1 unixCreateMode)  
   at System.IO.StreamReader.ValidateArgsAndOpenPath(String path, Encoding encoding, Int32 bufferSize)  
   at System.IO.File.ReadAllText(String path, Encoding encoding)  
   at Example.ProcessFile.Main() in C:\\repos\\ConsoleApp1\\Program.cs:line 12  
--- End of stack trace from previous location ---  
   at Example.ProcessFile.Main() in C:\\repos\\ConsoleApp1\\Program.cs:line 24
```

# 📊 The Real Cost — Microsoft Benchmarks

From .NET performance guidelines:

# 📦 Enforce This with .editorconfig

One of the easiest ways to make Microsoft’s exception-handling guidelines **non-optional** for your team is to enforce them with Roslyn analyzer rules in `.editorconfig`.  
Once added to your repo, these rules run automatically in **Visual Studio,** `**dotnet build**`**, and CI pipelines**, flagging violations during pull requests — before bad patterns hit production.

📄 [Analyzer Rules Documentation](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/)

```ini
dotnet_diagnostic.CA1031.severity = warning    # Don’t catch System.Exception  
dotnet_diagnostic.CA2200.severity = warning    # Use 'throw;' to preserve stack  
dotnet_diagnostic.CA2219.severity = warning    # Don’t throw in finally/filter  
dotnet_diagnostic.CA1065.severity = warning    # Avoid throwing from unexpected members  
dotnet_diagnostic.CA2201.severity = warning    # Don’t throw reserved/general exception types  
dotnet_diagnostic.CA1510.severity = suggestion # Prefer ThrowIfNull
```

**Why this matters:**

*   Prevents hidden performance issues and diagnosability problems.
*   Makes exception-handling patterns **consistent across all developers**.
*   Integrates into PR reviews without extra manual effort.

# 📌 Anti-Pattern → Fix Table

# 🧭 Exception Decision Flow (Microsoft-Style)

[Expected failure?] → YES → Use TryXxx API  
                    → NO  → Is this recoverable here?  
                             → YES → Catch specific exception  
                             → NO  → Let it bubble to boundary handler

# 🎯 Developer Challenge

Audit your last 10 production exceptions:

*   How many could be avoided with `TryXxx`?
*   How many were over-catches?
*   How many swallowed `[OperationCanceledException](https://learn.microsoft.com/en-us/dotnet/api/system.operationcanceledexception?view=net-9.0)` as errors?

Share your results — you might cut error noise by around 20% overnight.