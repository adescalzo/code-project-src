```yaml
---
title: "Understanding Middlewares in ASP.NET Core - Everything you Need to Know! - codewithmukesh"
source: https://codewithmukesh.com/blog/middlewares-in-aspnet-core/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2027
date_published: 2025-03-26T00:00:00.000Z
date_captured: 2025-08-12T22:44:37.528Z
domain: codewithmukesh.com
author: Unknown
category: ai_ml
technologies: [ASP.NET Core, .NET, Web API, MVC, Razor Pages, Serilog, Redis]
programming_languages: [C#]
tags: [aspnet-core, middleware, web-api, http-pipeline, request-processing, csharp, web-development, architecture, dotnet, security]
key_concepts: [http-request-pipeline, middleware-architecture, request-delegate, httpcontext, custom-middleware, short-circuiting, middleware-execution-order, cross-cutting-concerns]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to understanding middlewares in ASP.NET Core, which are fundamental components of the request-processing pipeline. It explains how middlewares function, their execution order, and the core concepts of Request Delegates and HttpContext. The content covers both built-in and custom middleware implementations, demonstrating how to build and register your own. Additionally, it discusses the concept of short-circuiting the pipeline for performance optimization and outlines best practices for designing and ordering middlewares in Web APIs. The article emphasizes the importance of mastering middleware for building efficient, secure, and maintainable ASP.NET Core applications.]
---
```

# Understanding Middlewares in ASP.NET Core - Everything you Need to Know! - codewithmukesh

1.  [](/)
2.  …
3.  [Blog](/blog/)
4.  [Middlewares In Aspnet Core](/blog/middlewares-in-aspnet-core/)

{"@context":"https://schema.org","@type":"BreadcrumbList","itemListElement":[{"@type":"ListItem","position":1,"item":{"@id":"/","name":"Home"}},{"@type":"ListItem","position":2,"item":{"@id":"/blog/","name":"Blog"}},{"@type":"ListItem","position":3,"item":{"@id":"/blog/middlewares-in-aspnet-core/","name":"Middlewares In Aspnet Core"}}]}

14 min read

Updated on March 26, 2025

# Understanding Middlewares in ASP.NET Core - Everything you Need to Know!

[![Mukesh Murugan](/_astro/mukesh_murugan.BJd3isoA_tRN3C.webp "Mukesh Murugan")

Mukesh Murugan

@iammukeshm



](https://www.linkedin.com/in/iammukeshm)

[#dotnet](/categories/dotnet) [.NET Web API Zero to Hero Course](/courses/dotnet-webapi-zero-to-hero/)

---

ASP.NET Core follows a powerful and flexible request-processing pipeline that enables developers to control how HTTP requests are handled. At the heart of this pipeline is middleware—a series of components that process requests and responses.

Every request that reaches an ASP.NET Core application follows a well-defined journey through the request pipeline before generating a response. This pipeline is built using middleware, a powerful mechanism that allows developers to inspect, modify, or short-circuit requests and responses.

In this article, we’ll explore how ASP.NET Core processes incoming requests, how middleware functions, and how you can build your own middleware to enhance your Web API’s capabilities.

> This article is part of my [.NET Web API Zero to Hero course](/courses/dotnet-webapi-zero-to-hero/), where I cover everything you need to know to build efficient and scalable APIs. Understanding middleware will help you control how requests are processed, manage logging, handle security, and improve performance. It ensures that your API is structured correctly, with proper error handling, authentication, and routing.

By learning middleware, you will also be able to create custom components to extend ASP.NET Core and add features specific to your application. Mastering this concept will make it easier to work with more advanced topics like security, performance tuning, and API optimizations. If you want to build better Web APIs, this is an essential step.

## What are Middlewares in ASP.NET Core?

Middleware in ASP.NET Core is a fundamental building block of the request pipeline. It acts as a series of software components that process HTTP requests and responses. Each middleware component can inspect, modify, or even terminate a request before it reaches the application’s core logic. This allows developers to handle tasks like authentication, logging, error handling, and response modification in a structured way.

When a request enters the application, it flows through the middleware pipeline in the order they are registered. Each middleware component has the option to process the request, make changes to it, or pass it along to the next middleware in line. If needed, middleware can also generate a response immediately, effectively short-circuiting the pipeline and preventing further execution of other middleware components.

The middleware pipeline is designed to be highly flexible. Developers can use built-in middleware for common tasks such as routing, authentication, logging, and exception handling, or create custom middleware tailored to specific needs. Since middleware components are executed in sequence, their order in the pipeline is critical. For example, an authentication middleware must run before authorization, ensuring that a user’s identity is established before checking permissions.

ASP.NET Core makes it easy to register middleware in the `Program.cs` file, where they are added to the pipeline using methods like `app.Use`, `app.Run`, and `app.Map`. This structured approach ensures that requests are handled consistently while providing developers with the ability to customize and extend the pipeline as needed.

## How Middlewares Work?

Each middleware in the pipeline follows a simple pattern: it receives the HttpContext, performs some processing, and then either calls the next middleware or short-circuits the pipeline by generating a response immediately. If middleware short-circuits the request, it prevents further execution of other middleware components, allowing for optimizations like handling errors or returning cached responses early.

By structuring middleware correctly, developers can efficiently manage logging, security, error handling, and request transformation, ensuring a smooth and predictable request-processing workflow in ASP.NET Core applications.

Here is a simple illustration of how middlewares work in ASP.NET Core Applications.

![Diagram illustrating the ASP.NET Core middleware pipeline. An HTTP Request enters from the left, passing sequentially through Middleware #1, Middleware #2, and Middleware #n. Each middleware box contains a "Modify Request" step, followed by "await next()", which passes control to the subsequent middleware. After the last middleware, the request reaches "Application Code". The HTTP Response then flows back from the "Application Code" through each middleware in reverse order, where each middleware performs a "Modify Response" step before the response exits to the left.](/_astro/middlewares-explained.BO30-Gyz_2uARXF.webp "Middlewares Explained")

---

## Middleware Execution Order

The order in which middleware components are added to the request pipeline is critical in ASP.NET Core. Middleware executes sequentially in the order they are registered, meaning each middleware can modify the request before passing it to the next component or modify the response on the way back.

When a request enters the pipeline, it flows through each middleware **in the order they are registered**. If a middleware calls `await next()`, the request continues to the next middleware. On the way back, the response passes through the middleware in **reverse order**, allowing for modifications before it reaches the client.

### **Example of Middleware Execution Order**

```csharp
app.Use(async (context, next) =>{    Console.WriteLine("Middleware 1: Incoming request");    await next();  // Pass control to the next middleware    Console.WriteLine("Middleware 1: Outgoing response");});
app.Use(async (context, next) =>{    Console.WriteLine("Middleware 2: Incoming request");    await next();    Console.WriteLine("Middleware 2: Outgoing response");});
app.Run(async (context) =>{    Console.WriteLine("Middleware 3: Handling request and terminating pipeline");    await context.Response.WriteAsync("Hello, world!");});
```

### **Expected Execution Flow**

```
Middleware 1: Incoming requestMiddleware 2: Incoming requestMiddleware 3: Handling request and terminating pipelineMiddleware 2: Outgoing responseMiddleware 1: Outgoing response
```

*   Middleware executes in the order they are added in `Program.cs`.
*   `app.Use()` allows the request to continue down the pipeline and modifies responses on the way back.
*   `app.Run()` short-circuits the pipeline and prevents further middleware execution.
*   Order matters—placing authentication middleware before authorization middleware ensures users are authenticated before checking permissions.

---

## Request Delegate & HttpContext - Core Concepts

When a request reaches an ASP.NET Core application, it goes through a series of middleware components before generating a response. At the heart of this process are **request delegates** and **HttpContext**.

A **request delegate** is a function that handles an HTTP request and determines how it should be processed. It can either modify the request, generate a response, or pass control to the next middleware in the pipeline. This allows for fine-grained control over request processing.

On the other hand, **HttpContext** provides all the details about the current HTTP request and response. It contains information such as headers, query parameters, authentication details, and response settings, enabling developers to interact with and manipulate the request lifecycle effectively.

### **Request Delegate**

In ASP.NET Core, a **request delegate** is a function that processes HTTP requests. It is the core building block of middleware and defines how each request is handled in the pipeline. Request delegates can either process a request and pass it to the next middleware or generate a response directly.

There are three ways to define request delegates:

1.  **Inline Middleware (Using Lambda Expressions)**
    
    ```csharp
    app.Use(async (context, next) =>{    Console.WriteLine("Incoming request: " + context.Request.Path);    await next();    Console.WriteLine("Outgoing response: " + context.Response.StatusCode);});
    ```
    
    Here, `next()` ensures that the next middleware in the pipeline gets executed.
2.  **Using `Run` to Short-Circuit the Pipeline**
    
    ```csharp
    app.Run(async context =>{    await context.Response.WriteAsync("Hello from the last middleware!");});
    ```
    
    `app.Run` does not call `next()`, meaning it short-circuits the request pipeline.
3.  **Using Middleware Classes**
    
    ```csharp
    public class CustomMiddleware{    private readonly RequestDelegate _next;
        public CustomMiddleware(RequestDelegate next)    {        _next = next;    }
        public async Task InvokeAsync(HttpContext context)    {        Console.WriteLine("Custom Middleware Executing...");        await _next(context);        Console.WriteLine("Custom Middleware Finished.");    }}
    ```
    
    Register it in `Program.cs`:
    
    ```csharp
    app.UseMiddleware<CustomMiddleware>();
    ```
    

---

### **HttpContext**

`HttpContext` represents the current HTTP request and response. It contains everything related to the request being processed.

#### **Commonly Used Properties of `HttpContext`**

1.  **Accessing Request Data**
    
    ```csharp
    string method = context.Request.Method;string path = context.Request.Path;string query = context.Request.QueryString.ToString();
    ```
    
2.  **Reading Request Headers**
    
    ```csharp
    string userAgent = context.Request.Headers["User-Agent"];
    ```
    
3.  **Handling Query Parameters**
    
    ```csharp
    string name = context.Request.Query["name"];
    ```
    
4.  **Setting Response Data**
    
    ```csharp
    context.Response.StatusCode = 200;await context.Response.WriteAsync("Hello, World!");
    ```
    
5.  **Checking Authentication & Authorization**
    
    ```csharp
    bool isAuthenticated = context.User.Identity.IsAuthenticated;
    ```
    

---

### **How Request Delegate and HttpContext Work Together**

Each request delegate receives an `HttpContext`, processes it, and either:

*   Calls the next middleware in the pipeline (`await next();`)
*   Returns a response immediately (`context.Response.WriteAsync("Hello!");`)

---

## Built-In Middlewares

ASP.NET Core provides several built-in middleware components that handle essential functionalities. These middlewares can be added to the request pipeline to enable various features such as authentication, routing, exception handling, and more. Here are some commonly used built-in middlewares:

### **1\. Exception Handling Middleware**

This middleware captures unhandled exceptions and provides a centralized mechanism for handling errors. It ensures that errors are logged properly and can return custom error responses.

```csharp
app.UseExceptionHandler("/Home/Error");
```

### **2\. Routing Middleware**

Routing determines how incoming HTTP requests map to the appropriate endpoints in the application. The routing middleware is crucial for defining API routes and MVC actions.

```csharp
app.UseRouting();
```

### **3\. Authentication and Authorization Middleware**

These middlewares handle user authentication and access control, ensuring that only authorized users can access certain endpoints.

```csharp
app.UseAuthentication();app.UseAuthorization();
```

### **4\. Static Files Middleware**

It serves static content like HTML, CSS, JavaScript, and images directly from the `wwwroot` folder.

```csharp
app.UseStaticFiles();
```

### **5\. CORS Middleware**

Cross-Origin Resource Sharing (CORS) middleware controls how your API handles requests from different domains.

```csharp
app.UseCors(options =>    options.WithOrigins("https://example.com")           .AllowAnyMethod()           .AllowAnyHeader());
```

### **6\. Response Compression Middleware**

Improves performance by compressing responses before sending them to the client, reducing bandwidth usage.

```csharp
app.UseResponseCompression();
```

### **7\. Session Middleware**

This enables session management by storing user session data in-memory or distributed stores like Redis.

```csharp
app.UseSession();
```

### **8\. HTTPS Redirection Middleware**

Forces all requests to use HTTPS, ensuring secure communication.

```csharp
app.UseHttpsRedirection();
```

### **9\. Request Logging Middleware**

Logs HTTP requests for debugging, auditing, or monitoring purposes.

```csharp
app.UseSerilogRequestLogging();
```

### **10\. Endpoint Middleware**

The final step in request processing, this middleware matches incoming requests to their respective controllers, Razor pages, or minimal API endpoints.

```csharp
app.UseEndpoints(endpoints =>{    endpoints.MapControllers();    endpoints.MapRazorPages();});
```

---

## Custom Middleware – Writing Your Own

While ASP.NET Core provides several built-in middleware components, there are times when you need to implement custom middleware to handle specific application requirements. Custom middleware allows you to modify requests and responses, implement logging, authentication, caching, or any other functionality required in your application.

### **Creating a Custom Middleware**

A middleware component in ASP.NET Core must:

1.  Accept a `RequestDelegate` in its constructor.
2.  Implement an `Invoke` or `InvokeAsync` method that processes the request.

#### **Step 1: Create a Middleware Class**

```csharp
public class CustomMiddleware{    private readonly RequestDelegate _next;
    public CustomMiddleware(RequestDelegate next)    {        _next = next;    }
    public async Task InvokeAsync(HttpContext context)    {        // Before the request is processed        Console.WriteLine("Custom Middleware: Request Processing Started");
        await _next(context); // Call the next middleware
        // After the request is processed        Console.WriteLine("Custom Middleware: Response Sent");    }}
```

#### **Step 2: Register Middleware in the Pipeline**

Now, you need to add this middleware to the application request pipeline in `Program.cs`.

```csharp
app.UseMiddleware<CustomMiddleware>();
```

Alternatively, you can register middleware using an extension method:

```csharp
public static class CustomMiddlewareExtensions{    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder)    {        return builder.UseMiddleware<CustomMiddleware>();    }}
```

Now, in `Program.cs`, simply use:

```csharp
app.UseCustomMiddleware();
```

### **Example: Custom Middleware for Logging Requests**

Here’s an example of a middleware that logs incoming requests:

```csharp
public class RequestLoggingMiddleware{    private readonly RequestDelegate _next;    private readonly ILogger<RequestLoggingMiddleware> _logger;
    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)    {        _next = next;        _logger = logger;    }
    public async Task InvokeAsync(HttpContext context)    {        _logger.LogInformation($"Incoming Request: {context.Request.Method} {context.Request.Path}");
        await _next(context); // Continue the pipeline
        _logger.LogInformation($"Response Status Code: {context.Response.StatusCode}");    }}
```

Register it:

```csharp
app.UseMiddleware<RequestLoggingMiddleware>();
```

## 2 Common Ways to Create Middleware in ASP.NET Core

In ASP.NET Core, middleware can be created in different ways depending on the level of customization required. The two most common approaches are **Request Delegate Based Middleware** and **Convention-Based Middleware**.

### **Request Delegate Based Middleware**

This is the simplest way to create middleware using inline request delegates. It allows you to define middleware logic directly within the `Program.cs` file without creating a separate class.

```csharp
app.Use(async (context, next) =>{    Console.WriteLine("Request Received: " + context.Request.Path);    await next(); // Call the next middleware in the pipeline    Console.WriteLine("Response Sent: " + context.Response.StatusCode);});
```

This approach is useful for small, quick modifications to the request pipeline, such as logging or modifying request headers. However, for more complex logic, using convention-based middleware is recommended.

---

### **Convention-Based Middleware**

Convention-based middleware follows a structured approach by defining a middleware class. This improves reusability, maintainability, and separation of concerns.

```csharp
public class CustomMiddleware{    private readonly RequestDelegate _next;
    public CustomMiddleware(RequestDelegate next)    {        _next = next;    }
    public async Task InvokeAsync(HttpContext context)    {        Console.WriteLine("Custom Middleware Executing...");        await _next(context);        Console.WriteLine("Custom Middleware Finished.");    }}
```

Here are the required conventions,

*   The constructor must take a RequestDelegate parameter, which represents the next middleware in the pipeline.
*   This allows the middleware to pass control to the next component if necessary.
*   The method must be named Invoke or InvokeAsync.
*   It must accept an HttpContext parameter.
*   It should return a Task to support asynchronous processing.

Convention-based middleware is the preferred approach when building reusable middleware components that handle logging, security, request modifications, or response transformations.

---

### What’s the Right Approach?

Use **request delegate-based middleware** for simple tasks like request logging or setting headers. When you need more flexibility, **convention-based middleware** is the better choice for complex logic that should be reusable across different applications.

## Short-Circuiting the Pipeline

In some scenarios, you may need to stop the request from proceeding further in the middleware pipeline. This is known as **short-circuiting** the pipeline. Instead of passing the request to the next middleware using `_next(context)`, you can generate a response immediately. This technique is useful for scenarios like maintenance mode, authentication checks, rate limiting, or returning cached responses early to improve performance.

```csharp
public class MaintenanceMiddleware{    private readonly RequestDelegate _next;
    public MaintenanceMiddleware(RequestDelegate next)    {        _next = next;    }
    public async Task InvokeAsync(HttpContext context)    {        context.Response.StatusCode = 503;        await context.Response.WriteAsync("Service is under maintenance.");    }}
```

Register this middleware before other middlewares to take effect:

```csharp
app.UseMiddleware<MaintenanceMiddleware>();
```

## Best Practices for Middleware in Web APIs

Middleware plays a crucial role in processing requests and responses in ASP.NET Core Web APIs. Properly designing and structuring middleware ensures better performance, maintainability, and security. Here are some best practices to follow when working with middleware in Web APIs.

### **1\. Keep Middleware Lightweight**

Middleware should be focused on a single responsibility and avoid performing heavy computations or long-running tasks. If complex logic is required, consider offloading it to background services or separate application layers.

### **2\. Order Middleware Correctly**

Middleware executes in the order they are registered, so it’s important to place them strategically. For example:

*   **Exception handling** middleware should be registered first to catch all unhandled exceptions.
*   **Authentication** should come before **authorization** to ensure the user is identified before access checks.
*   **Static file handling** should be placed before request-processing middlewares to improve performance.

### **3\. Use Built-in Middleware Whenever Possible**

ASP.NET Core provides a rich set of built-in middleware for exception handling, authentication, CORS, response compression, etc. Instead of writing custom middleware from scratch, prefer built-in solutions to ensure reliability and maintainability.

```csharp
app.UseExceptionHandler("/error");app.UseAuthentication();app.UseAuthorization();app.UseRouting();app.UseEndpoints(endpoints => endpoints.MapControllers());
```

### **4\. Avoid Blocking Calls in Middleware**

Middleware should be asynchronous to avoid blocking the request pipeline and degrading performance. Use `async` and `await` when handling requests.

**Bad Practice (Blocking Call)**

```csharp
public void Invoke(HttpContext context){    var result = SomeLongRunningOperation().Result; // Blocks the thread    context.Response.WriteAsync(result);}
```

**Good Practice (Asynchronous Call)**

```csharp
public async Task InvokeAsync(HttpContext context){    var result = await SomeLongRunningOperation();    await context.Response.WriteAsync(result);}
```

### **5\. Short-Circuit the Pipeline When Necessary**

If a request can be handled early (such as returning a cached response or handling maintenance mode), short-circuit the pipeline to improve efficiency.

```csharp
public async Task InvokeAsync(HttpContext context){    if (context.Request.Path == "/maintenance")    {        context.Response.StatusCode = 503;        await context.Response.WriteAsync("Service is under maintenance.");        return; // Stop further middleware execution    }
    await _next(context);}
```

### **6\. Use Middleware Extensions for Clean Code**

To keep `Program.cs` clean and modular, encapsulate middleware registration inside extension methods.

```csharp
public static class CustomMiddlewareExtensions{    public static IApplicationBuilder UseCustomMiddleware(this IApplicationBuilder builder)    {        return builder.UseMiddleware<CustomMiddleware>();    }}
```

Now, register it simply as:

```csharp
app.UseCustomMiddleware();
```

### **7\. Log Middleware Execution for Debugging**

Adding logging inside middleware helps track request processing and diagnose issues.

```csharp
public class LoggingMiddleware{    private readonly RequestDelegate _next;    private readonly ILogger<LoggingMiddleware> _logger;
    public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)    {        _next = next;        _logger = logger;    }
    public async Task InvokeAsync(HttpContext context)    {        _logger.LogInformation($"Request: {context.Request.Method} {context.Request.Path}");        await _next(context);        _logger.LogInformation($"Response: {context.Response.StatusCode}");    }}
```

### **8\. Avoid Middleware Overuse**

Not everything needs to be a middleware. If logic is specific to certain controllers or actions, consider using **action filters** or **service layers** instead. Middleware should handle **cross-cutting concerns** such as logging, authentication, and exception handling.

---

## Recommended Middleware Execution Order (BONUS)

Here is how you should arrange your middlewares in your .NET Applications to maximize performance!

```csharp
var app = builder.Build();
app.UseExceptionHandler("/error");      // 1. Global Exception Handlingapp.UseHttpsRedirection();              // 2. Enforce HTTPSapp.UseRouting();                       // 3. Routing Middlewareapp.UseCors();                           // 4. Enable CORS (if needed)app.UseAuthentication();                 // 5. Authenticate Usersapp.UseAuthorization();                  // 6. Check Permissionsapp.UseMiddleware<CustomMiddleware>();   // 7. Custom Middleware (e.g., Logging)app.UseEndpoints(endpoints =>            // 8. Endpoint Execution{    endpoints.MapControllers();});
app.Run();
```

### Why This Order?

*   [Exception Handling](/blog/global-exception-handling-in-aspnet-core/) First: Ensures all unhandled exceptions are caught before reaching the client.
*   HTTPS Redirection Early: Redirects HTTP to HTTPS as soon as possible.
*   Routing Before Authentication: Ensures requests are mapped before authentication checks.
*   Authentication Before Authorization: A user must be authenticated before checking permissions.
*   Custom Middleware Before Endpoints: Logging, rate-limiting, or request modification should happen before hitting controllers.

## **Summary**

Middleware is a fundamental part of the ASP.NET Core request pipeline, allowing developers to handle cross-cutting concerns like authentication, logging, error handling, and request transformations. Understanding how middleware works, the correct execution order, and best practices ensures that your Web APIs are efficient, secure, and maintainable.

In this article, we covered:

*   **What Middleware is** and how it processes requests.
*   **Built-in Middlewares** in ASP.NET Core and their roles.
*   **Request Delegates & HttpContext**, which are the building blocks of middleware.
*   **Custom Middleware** and how to write your own for specific requirements.
*   **Short-Circuiting the Pipeline** to optimize performance when needed.
*   **Middleware Execution Order** and the recommended best practices for structuring your pipeline.

Mastering middleware is crucial for any ASP.NET Core developer. Whether you’re handling authentication, error logging, or performance optimizations, middleware provides a clean and modular way to manage requests and responses.

This article is part of my **.NET Web API Zero to Hero** free course, where I break down essential concepts for building high-quality Web APIs. If you found this helpful, make sure to **follow my course** for more in-depth content. Also, feel free to **share this with your colleagues** who might benefit from it! 🚀

Check out the full course here: [.NET Web API Zero to Hero](https://codewithmukesh.com/courses/dotnet-webapi-zero-to-hero/)

[

Support ❤️

If you have enjoyed my content, support me by buying a couple of coffees.](/refer/bmc)

window.bmcClick = function () { window.posthog.capture("bmc\_clicked"); };

Share this Article

Share this article with your network to help others!

[](https://www.facebook.com/sharer.php?u=https://codewithmukesh.com/blog/middlewares-in-aspnet-core//&t=Check%20out%20this%20interesting%20article!🔥)[](https://www.linkedin.com/shareArticle?mini=true&url=https://codewithmukesh.com/blog/middlewares-in-aspnet-core/&title=Check%20out%20this%20interesting%20article!🔥)[](https://twitter.com/intent/tweet?text=Check%20out%20this%20interesting%20article!🔥,%20by%20@iammukeshm%0A%0Ahttps://codewithmukesh.com/blog/middlewares-in-aspnet-core/)[](mailto:?subject=Understanding Middlewares in ASP.NET Core - Everything you Need to Know!!🔥&body=Hey%2C%20I%20found%20this%20interesting%20article%20and%20thought%20you%20might%20enjoy%20it.%0A%0Ahttps%3A%2F%2Fcodewithmukesh.com%2Fblog%2Fmiddlewares-in-aspnet-core%2F)

What's your Feedback?

Do let me know your thoughts around this article.

Popular Articles 🚀

*   [ASP.NET Core 9 Web API CRUD with Entity...](/blog/aspnet-core-webapi-crud-with-entity-framework-core-full-course/)
*   [Essential AWS Services Every .NET Developer Should Master!](/blog/essential-aws-services-for-dotnet-developers/)
*   [Swagger is Dead? Here's the Alternative!](/blog/dotnet-swagger-alternatives-openapi/)
*   [GitHub Actions CI/CD Pipeline for Deploying .NET Web...](/blog/github-actions-deploy-dotnet-webapi-to-amazon-ecs/)
*   [Choosing the Best AWS Compute Service for your...](/blog/best-aws-compute-service-for-your-dotnet-solution/)

Table Of Content

1.  [What are Middlewares in ASP.NET Core?](#what-are-middlewares-in-aspnet-core)
2.  [How Middlewares Work?](#how-middlewares-work)
3.  [Middleware Execution Order](#middleware-execution-order)
4.  [Request Delegate & HttpContext - Core Concepts](#request-delegate--httpcontext---core-concepts)
5.  [Built-In Middlewares](#built-in-middlewares)
6.  [Custom Middleware – Writing Your Own](#custom-middleware--writing-your-own)
7.  [2 Common Ways to Create Middleware in ASP.NET Core](#2-common-ways-to-create-middleware-in-aspnet-core)
8.  [Short-Circuiting the Pipeline](#short-circuiting-the-pipeline)
9.  [Best Practices for Middleware in Web APIs](#best-practices-for-middleware-in-web-apis)
10.  [Recommended Middleware Execution Order (BONUS)](#recommended-middleware-execution-order-bonus)
11.  [Summary](#summary)

const observer = new IntersectionObserver( (entries) => { for (const entry of entries) { const headingFragment = \`#${entry.target.id}\`; const tocItem = document.querySelector(\`a\[href="${headingFragment}"\]\`); if (entry.isIntersecting) { const previouslyActivatedItem = document.querySelector(".active-toc-item"); previouslyActivatedItem?.classList.remove("active-toc-item"); tocItem.classList.add("active-toc-item"); } else { const isAnyOtherEntryIntersecting = entries.some( (e) => e.target.id !== entry.target.id && e.isIntersecting, ); if (isAnyOtherEntryIntersecting) { tocItem.classList.remove("active-toc-item"); } } } }, { root: null, rootMargin: "0px", threshold: \[1\] }, ); const sectionHeadings = document.querySelectorAll( "article > h2, article > h3", ); for (const heading of sectionHeadings) { observer.observe(heading); }

[

Buy Me A Coffee!

If you've found my content valuable, support me by buying a coffee.](https://buymeacoffee.com/codewithmukesh)