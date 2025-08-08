```yaml
---
title: "Cross-Origin Resource Sharing (CORS) in ASP.NET Core: A Comprehensive Guide"
source: https://antondevtips.com/blog/cors-in-asp-net-core-a-comprehensive-guide
date_published: 2024-05-23T11:00:18.417Z
date_captured: 2025-08-06T17:20:48.772Z
domain: antondevtips.com
author: Anton Martyniuk
category: frontend
technologies: [ASP.NET Core, .NET]
programming_languages: [C#]
tags: [cors, aspnet-core, web-api, security, http, middleware, configuration, dotnet, cross-origin]
key_concepts: [Cross-Origin Resource Sharing, middleware pipeline, dependency injection, API security, minimal APIs, controller attributes, HTTP headers]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to implementing Cross-Origin Resource Sharing (CORS) in ASP.NET Core applications. It explains how CORS works, its importance for enabling cross-domain API access, and the process of configuring it using `AddCors` and `UseCors` methods. The guide details applying CORS policies globally, to specific controllers, or individual minimal API endpoints through attributes and `RequireCors`. Furthermore, it covers various CORS policy options such as allowed origins, HTTP methods, headers, and credentials, emphasizing best practices for both development and production environments.]
---
```

# Cross-Origin Resource Sharing (CORS) in ASP.NET Core: A Comprehensive Guide

![Banner image for the article titled "CORS in ASP.NET Core: A Comprehensive Guide", featuring a code icon and "dev tips" text.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_asp_cors.png&w=3840&q=100)

# Cross-Origin Resource Sharing (CORS) in ASP.NET Core: A Comprehensive Guide

May 23, 2024

3 min read

Cross-Origin Resource Sharing (CORS) is a standard in web applications that allows or restricts web pages from making requests to a domain different from the one that served the initial web page. Today we will explore how to implement and manage CORS in ASP.NET Core applications effectively.

## How CORS works

When a web page makes a cross-origin HTTP request, the browser automatically adds an `Origin` header to the request. The server checks this header against its CORS policy. If the origin is allowed, the server responds with a `Access-Control-Allow-Origin` CORS header. This indicates that the request is allowed to be served on the server.

The server doesn't return an error if the CORS policy doesn't allow the request to be executed. The client is responsible for returning error to the client and blocking the response.

CORS is a way for a server to allow web browsers to execute a cross-origin requests. Browsers without CORS can't do cross-origin requests.

## When To Use CORS

**CORS should be enabled in ASP.NET Core:**

*   To allow your API to be accessed from different domains
*   When your frontend and backend are hosted separately
*   To control specific resources in your application to be accessible from other domains
*   In development mode

To enable CORS in ASP.NET Core, call the `AddCors` method on the `IServiceCollection` to add it to the DI container:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        policyBuilder => policyBuilder.WithOrigins("http://example.com"));
});
```

And add CORS to the middleware pipeline by calling `UseCors` method:

```csharp
app.UseCors("AllowSpecificOrigin");
```

> Often CORS are enabled in ASP.NET Core apps only in the **development mode** to simplify development when frontend and backend are run on different hosts or ports. Consider using CORS in **production** only when it is absolutely required

## Correct Order of Middlewares When Using CORS

Here are few important tips when placing `UseCors` middleware:

*   `UseCors` middleware should be placed before `UseResponseCaching` due to this [bug](https://github.com/dotnet/aspnetcore/issues/23218).
*   `UseCors` middleware should be placed before `UseStaticFiles` to ensure CORS headers are properly added to the static file responses

```csharp
app.UseCors();
app.UseResponseCaching();
app.UseStaticFiles();
```

## Enable CORS in WebApi Controllers

To apply CORS policies to specific endpoints, use the `RequireCors` method in endpoint routing configuration:

```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers().RequireCors("AllowSpecificOrigin");
});
```

You can enable or disable CORS for a specific controller:

```csharp
[EnableCors("AllowSpecificOrigin")]
public class UsersController : ControllerBase
{
    // Controller methods
}

[DisableCors]
public class ProductsController : ControllerBase
{
    // Controller methods
}
```

You can also enable or disable CORS for different controller methods:

```csharp
public class UsersController : ControllerBase
{
    [EnableCors("CorsPolicy1")]
    [HttpGet]
    public ActionResult<IEnumerable<string>> Get(Guid id)
    {
        var user = new User
        {
            Id = id,
            Name = "Anton"
        };
        
        return Ok(user);
    }
    
    [EnableCors("CorsPolicy2")]
    [HttpPost]
    public ActionResult<IEnumerable<string>> Create(CreateUserRequest request)
    {
        return Ok();
    }
    
    [DisableCors]
    [HttpDelete]
    public ActionResult<IEnumerable<string>> Create(Guid id)
    {
        return NoContent();
    }
}
```

## Enable CORS in Minimal APIs

To enable CORS for minimal API endpoints simply call `RequireCors` method for each endpoint:

```csharp
app.MapGet("/api/books", () =>
{
    var books = SeedService.GetBooks(10);
    return Results.Ok(books);
}).RequireCors("AllowAllOrigins");
```

## CORS Policy Options

We already learned what the CORS is and how to enable it in the ASP.NET Core. It's time to explore what policy options does the CORS provide us:

*   Set the allowed origins
*   Set the allowed HTTP methods
*   Set the allowed request headers
*   Set the exposed response headers
*   Credentials in cross-origin requests

### Allowed Origin

Specify what origins are allowed to access the resource.

```csharp
builder.WithOrigins("http://example.com");
```

### Allowed HTTP Methods

Define what HTTP methods can be used when accessing the resource.

```csharp
builder.WithMethods("GET", "POST", "PUT", "DELETE", "PATCH");
```

### Allowed Request Headers

Specify headers that can be used when making the request.

```csharp
builder.WithHeaders("Content-Type", "Authorization");
```

### Exposed Response Headers

Control which headers are exposed to the browser.

```csharp
builder.WithExposedHeaders("X-Custom-Header");
```

### Credentials in Cross-Origin Requests

Determine if cookies should be included with requests.

```csharp
builder.AllowCredentials();
```

## Allow Any Policy Options

There is a way to enable all policy options, for example for development:

```csharp
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAllOrigins",
            policyBuilder => policyBuilder.AllowAnyHeader()
                .AllowAnyMethod()
                .AllowAnyOrigin()
                //.AllowCredentials()
                .SetIsOriginAllowed(_ => true)
        );
    });   
}

// ...

if (app.Environment.IsDevelopment())
{
    app.UseCors("AllowAllOrigins");
}
```

> **NOTE:** you can't use AllowAnyOrigin (wild card, allowing all origins) with AllowCredentials at the same time

## Summary

Implementing CORS in ASP.NET Core is essential for modern web applications to securely manage cross-origin requests in the web browsers. You can configure CORS for controllers and minimal APIs. You can use CORS in **development mode** to simplify your frontend and backend development. Use CORS in **production mode** only when it is absolutely required.

Hope you find this newsletter useful. See you next time.