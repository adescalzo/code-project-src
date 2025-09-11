```yaml
---
title: "Middlewares in ASP.NET Core. For example : | by Karim Samir | easydotnet | Sep, 2025 | Medium"
source: https://medium.com/easydotnet/middlewares-in-asp-net-core-f101c0cc2a33
date_published: 2025-09-04T19:37:03.862Z
date_captured: 2025-09-10T12:47:30.611Z
domain: medium.com
author: Karim Samir
category: general
technologies: [ASP.NET Core, .NET]
programming_languages: [C#]
tags: [middleware, aspnet-core, http-pipeline, authentication, authorization, logging, web-development, custom-middleware, dotnet]
key_concepts: [middleware, http-request-pipeline, custom-middleware, authentication, authorization, request-delegation, dependency-injection, access-control]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explains the concept of middlewares in ASP.NET Core, defining them as components that process HTTP requests and responses in a pipeline. It uses a marketplace application scenario to demonstrate the need for access control. The author provides practical C# code examples for implementing three custom middlewares: one for authentication, one for authorization, and another for logging store visits. The article concludes by showing how to correctly register these custom middlewares in the `Program.cs` file, emphasizing the importance of their execution order within the application's request pipeline.]
---
```

# Middlewares in ASP.NET Core. For example : | by Karim Samir | easydotnet | Sep, 2025 | Medium

# Middlewares in ASP.NET Core

![A diagram illustrating client-server communication, showing HTTP methods (GET, POST, PUT, DELETE) as arrows flowing from a laptop (Client) to a stack of cylinders (Server/Database).](https://miro.medium.com/v2/resize:fit:640/0*VQHBtRM4XEBHaTVs)

Consider a Marketplace application with different user roles:
-   **Users**, who create stores and products.
-   **Public clients**, who search and buy products.
-   **Admins**, who manage the application, including adding other admins.

This application requires various interfaces:
-   Interfaces for admins (e.g., add new admins).
-   Interfaces for users (e.g., add new product).
-   Interfaces for public clients (e.g., check one product).
-   Common interfaces (e.g., Home and Login).

Such an application necessitates handling access for each role (admin, public user, and client). This is precisely where **Middlewares** are used.

**Middleware** is a piece of code that sits in the HTTP request/response pipeline. Before every HTTP Request, a check (**Middleware**) can be inserted to determine if the current user has access to a specific interface.

![A conceptual diagram depicting a series of rectangular blocks, chained together, with buckets of water being passed along, symbolizing the HTTP request pipeline and how middlewares process requests sequentially.](https://miro.medium.com/v2/resize:fit:575/1*zaomtLc-gHM8xL_DLJhdOQ.png)

**Practical Example: Implementing 3 Custom Middlewares**
We will add three custom middlewares:
1.  One for Authentication.
2.  One for Authorization.
3.  One for logging visitors.

### 1. Authentication Middleware

```csharp
public class AuthenticationMiddleware
{
    private readonly RequestDelegate _next;
  
    public AuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }
  
    public async Task Invoke(HttpContext context)
    {
        // Skip certain public routes
        if (context.Request.Path.StartsWithSegments("/home") ||
            context.Request.Path.StartsWithSegments("/login"))
        {
            await _next(context);
            return;
        }
  
        // Check if Authorization header exists
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            context.Response.Redirect("/home"); // redirect if not logged in
            return; // stop pipeline
        }
  
        // Simulate getting user ID from token
        context.Items["UserId"] = "123";
        await _next(context); // continue pipeline
    }
}
```

### 2. Authorization Middleware

```csharp
public class AuthorizationMiddleware
{
    private readonly RequestDelegate _next;
  
    public AuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }
  
    public async Task Invoke(HttpContext context)
    {
        var userId = context.Items["UserId"] as string;
  
        // Skip if user is not authenticated
        if (userId == null)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden: Login first.");
            return;
        }
  
        // Simulate store owner check (only user 123 owns store)
        if (context.Request.Path.StartsWithSegments("/owner/stores") && userId != "123")
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden: Not store owner.");
            return;
        }
  
        await _next(context); // continue pipeline
    }
}
```

### 3. Store Visit Logger Middleware

```csharp
public class StoreVisitLoggerMiddleware
{
    private readonly RequestDelegate _next;
    private static Dictionary<string, int> _visitCounts = new();
  
    public StoreVisitLoggerMiddleware(RequestDelegate next)
    {
        _next = next;
    }
  
    public async Task Invoke(HttpContext context)
    {
        // Only log visits for store routes
        if (context.Request.Path.StartsWithSegments("/stores"))
        {
            var userId = context.Items["UserId"] ?? "Anonymous";
            var storeId = context.Request.Path.Value.Split("/")[2]; // e.g., /stores/5
            if (!_visitCounts.ContainsKey(storeId))
                _visitCounts[storeId] = 0;
  
            _visitCounts[storeId]++;
            Console.WriteLine($"User {userId} visited store {storeId}. Total visits: {_visitCounts[storeId]}");
        }
  
        await _next(context); // continue pipeline
    }
}
```

### 4. Register Middlewares in `Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();
  
// Order matters: Authentication → Authorization → Logging
app.UseMiddleware<AuthenticationMiddleware>();
app.UseMiddleware<AuthorizationMiddleware>();
app.UseMiddleware<StoreVisitLoggerMiddleware>();
  
app.Run();
```

![A table summarizing different request scenarios and their outcomes when processed by the implemented middlewares, showing how authentication, authorization, and logging interact.](https://miro.medium.com/v2/resize:fit:664/1*GATsX_WFtjHOOq_0eC684A.png)