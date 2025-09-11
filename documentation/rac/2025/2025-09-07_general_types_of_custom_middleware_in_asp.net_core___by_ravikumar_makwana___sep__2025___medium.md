```yaml
---
title: "Types of Custom Middleware in ASP.NET Core | by Ravikumar Makwana | Sep, 2025 | Medium"
source: https://medium.com/@ravikumar.makwana/types-of-custom-middleware-in-asp-net-core-632cc5b615fb
date_published: 2025-09-07T14:50:53.029Z
date_captured: 2025-09-10T18:26:10.501Z
domain: medium.com
author: Ravikumar Makwana
category: general
technologies: [ASP.NET Core, .NET, ILogger, HttpContext, RequestDelegate, IMiddleware, Console]
programming_languages: [C#]
tags: [asp.net-core, middleware, web-development, http-pipeline, dependency-injection, csharp, logging, custom-middleware, dotnet, web-api]
key_concepts: [Middleware pipeline, Dependency Injection, HTTP request processing, Inline middleware, Convention-based middleware, DI-friendly middleware, Service lifetimes, RequestDelegate]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to creating custom middleware in ASP.NET Core, explaining its role in the HTTP request pipeline. It details three distinct approaches: inline middleware for quick experiments, convention-based middleware for reusable class-based components, and DI-friendly middleware using `IMiddleware` for complex scenarios requiring per-request service lifetimes. Each method is illustrated with practical C# code examples, highlighting their respective advantages, disadvantages, and ideal use cases. The article concludes with best practices and a personal rule to help developers choose the most suitable middleware implementation for their projects.]
---
```

# Types of Custom Middleware in ASP.NET Core | by Ravikumar Makwana | Sep, 2025 | Medium

# Types of Custom Middleware in ASP.NET Core

![Author Ravikumar Makwana's profile picture](https://miro.medium.com/v2/resize:fill:64:64/1*7J-MYybAbbtnB0kcnYD9rQ.jpeg)

Ravikumar Makwana

Follow

3 min read

·

4 days ago

Listen

Share

More

## 🌟 Introduction

If you’ve ever built an ASP.NET Core application, you’ve already worked with **middleware** — even if you didn’t realize it. Every incoming HTTP request flows through a pipeline of middleware components before hitting your controller, and then flows back through the same pipeline when sending the response.

Think of middleware as a series of checkpoints in an airport:

*   Security check
*   Passport control
*   Each checkpoint can let you through, add additional info, or stop you right there.

In ASP.NET Core, you’re not stuck with the built-in checkpoints. You can **create your own middleware** for tasks like logging, adding custom headers, or validating requests.

![Title image for the article: "Types of Custom Middleware in ASP.NET Core" in colorful text on a light background.](https://miro.medium.com/v2/resize:fit:700/1*sYPfQ_O6qdnjRYkmhiAL3g.png)

And here’s the cool part: ASP.NET Core gives you **three different ways** to write custom middleware. Let’s walk through them as if we were designing a real project.

## 1️⃣ Inline Middleware

Imagine you’re prototyping a new API, and you just want to log every request quickly. You don’t want to create a new file, just a quick inline snippet. That’s where **inline middleware** shines.

```csharp
app.Use(async (context, next) =>  
{  
    Console.WriteLine($"➡️ Request: {context.Request.Method} {context.Request.Path}");  
    await next(); // pass to the next middleware  
    Console.WriteLine($"⬅️ Response: {context.Response.StatusCode}");  
});
```

This snippet:

*   Logs every incoming request (method + path).
*   Lets the request continue (`await next()`).
*   Logs the response status code on the way back.

✅ Perfect for **quick experiments or one-off checks.**  
❌ Not great for **production** because inline delegates get messy and aren’t reusable.

## 2️⃣ Convention-Based Middleware

Now, let’s say your little logging snippet works great, but you want to **use it in multiple apps** or make it testable. That’s when you move it into a proper **class** — what I call **convention-based middleware**.

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
        Console.WriteLine($"[Convention] Request: {context.Request.Path}");  
        await _next(context);  
    }  
}
```

Register it:

```csharp
app.UseMiddleware<LoggingMiddleware>();
```

**Dependency Injection:**

*   The middleware class is a **singleton by nature.**
*   Constructor → Inject Singleton & Transient Services only.
*   Invoke method → Inject Scoped, Singleton, Transient Services.

✅ **Pros**

*   Reusable, testable, and very common in real projects.
*   Supports constructor DI. (services are injected automatically)
*   Simple and familiar — no extra setup needed.

❌ **Cons**

*   The middleware class is instantiated once and reused, so injected services follow the middleware’s lifetime (usually singleton-like).
*   If you want dependencies to be created **per-request** (e.g., `Scoped` services), you’ll need to manage that carefully — and this is where `IMiddleware` shines.

## 3️⃣ DI-Friendly Middleware (`IMiddleware`)

With `IMiddleware`, the DI container creates a **new instance per request**, respecting the scoped lifetimes of services. This is particularly useful if your middleware depends on scoped services (like `DbContext`) and you want clean, per-request disposal.

```csharp
public class HeaderMiddleware : IMiddleware  
{  
    private readonly ILogger<HeaderMiddleware> _logger;  
  
    public HeaderMiddleware(ILogger<HeaderMiddleware> logger)  
    {  
        _logger = logger;  
    }  
  
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)  
    {  
        _logger.LogInformation("Adding custom header...");  
        context.Response.Headers.Add("X-Custom", "IMiddleware Example");  
        await next(context);  
    }  
}
```

Register in DI:

```csharp
builder.Services.AddTransient<HeaderMiddleware>();  
app.UseMiddleware<HeaderMiddleware>();
```

✅ **Great for complex apps with many dependencies.**  
❌ More setup compared to conventional.

## ✅ Best Practices

*   **Order matters**: Middleware is sequential. For example, authentication must come before authorization.
*   Start **inline** for experiments, but refactor to **class-based** for production.
*   Use `**IMiddleware**` when middleware needs multiple services from DI.
*   Keep middleware **focused** — don’t overload one component with too many responsibilities.

## 📝 Conclusion

Middleware is what makes ASP.NET Core’s pipeline flexible. By understanding the **three ways to build custom middleware** — **inline, convention-based, and DI-friendly (**`**IMiddleware**`**)** — you’ll know exactly which approach to use depending on your project’s stage and complexity.

👉 My personal rule:

*   Quick idea → Inline
*   Reusable middleware → Convention-based class
*   Complex DI-heavy middleware → `IMiddleware`

Do you have ideas or questions? Let me know in the comments.

Thanks for reading — and happy coding! 🚀