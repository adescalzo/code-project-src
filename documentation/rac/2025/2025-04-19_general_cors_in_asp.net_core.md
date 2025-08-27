```yaml
---
title: CORS in ASP.NET Core
source: https://www.nikolatech.net/blogs/cors-in-aspnetcore
date_published: 2025-04-19T10:05:31.595Z
date_captured: 2025-08-08T12:49:30.212Z
domain: www.nikolatech.net
author: Unknown
category: general
technologies: [ASP.NET Core, .NET, HTTP]
programming_languages: [C#]
tags: [cors, aspnet-core, web-api, security, http, browser-security, middleware, dotnet, cross-origin, web-development]
key_concepts: [Cross-Origin Resource Sharing (CORS), Same-Origin Policy, HTTP headers, preflight requests, ASP.NET Core middleware, ASP.NET Core attributes, minimal APIs, web security]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to Cross-Origin Resource Sharing (CORS) in ASP.NET Core, explaining its role as a browser security feature. It details how to configure CORS policies using `AddCors()`, specifying allowed origins, headers, and methods. The post demonstrates three distinct methods for applying CORS: globally via middleware, selectively using `[EnableCors]` attributes on controllers, and directly on minimal API endpoints with `RequireCors()`. Additionally, it clarifies the concept of preflight requests and how ASP.NET Core handles them, including setting cache expiration. The aim is to help developers properly secure their Web APIs while maintaining necessary accessibility.]
---
```

# CORS in ASP.NET Core

# In this article

[Intro](#Intro)
[What is CORS?](#CORS)
[Enable CORS](#EnableCORS)
[Using Middleware](#MiddlewareNamedPolicy)
[Using Attributes](#CORSAttribute)
[With Minimal Endpoints](#CORSMinimalEndpoints)
[Preflight Requests](#PreflightRequests)
[Conclusion](#Conclusion)

![Banner: A blue background with the text "CORS Enable CORS in ASP.NET Core" in white, along with a logo and an icon representing two arrows pointing in opposite directions.](https://coekcx.github.io/BlogImages/banners/cors-in-aspnetcore-banner.png)

#### CORS in ASP.NET Core

19 Apr 2025

5 min

**CORS** is one of those errors almost every developer runs into at least once while building a Web API.

We all manage to find a quick fix at some point, but the understanding of how CORS actually works is often overlooked.

In this blog post, we’ll take a closer look at what CORS is and how to handle it properly.

## What is CORS?

**Cross-Origin Resource Sharing (CORS)** is a browser security feature that controls how web applications can request resources from a different origin (domain, protocol, or port).

It helps prevent malicious websites from making unauthorized requests to another site on behalf of the user.

![CORS Diagram: A diagram showing a "Web Client" at www.nikolatech.net making a "Cross-origin request" to a "Web API" at www.contoso.com.](https://coekcx.github.io/BlogImages/images/cors-diagram.png)

By default, browsers block cross-origin requests due to the **Same-Origin Policy**.

For example, a frontend running on https://nikolatech.net cannot access an API at https://contoso.com unless the API explicitly allows it.

CORS enables controlled access by using specific HTTP headers. A server can indicate which origins are allowed using headers like **Access-Control-Allow-Origin**.

## Enable CORS

First step is to register and configure CORS using **AddCors()**:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://your-frontend.com")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

CORS policies should help you define how the server responds to cross-origin requests, you can define one or more named CORS policies.

Here are most common methods used to configure:

*   **WithOrigin()** will allow only specific domains to make cross-origin requests.
*   **AllowAnyOrigin()** will allow any domain to make requests.
*   **WithHeaders()** will allow only specific headers in requests.
*   **AllowAnyHeader()** will allow any header.
*   **WithMethods()** will allow only specific HTTP methods.
*   **AllowAnyMethod()** will any HTTP method.

**NOTE:** You can use **AddDefaultPolicy()** to add a default policy instead of using **AddPolicy()**.

Once you configure your CORS policies, ASP.NET Core allow you to apply cross-origin requests in three different ways:

*   Using **Middleware**
*   Using **Attributes**
*   With **Minimal Endpoints**

### Using Middleware

Using middleware to apply a common CORS policy is the most common approach because it allows you to apply CORS globally, affecting all endpoints.

To add middleware properly, it's important to add it before **UseAuthorization()** and after **UseRouting()**:

```csharp
app.UseRouting();

app.UseCors("CorsPolicy");

app.UseAuthorization();
```

**NOTE:** If you're applying the default policy, there's no need to specify a policy name.

### Using Attributes

In case you are working with controllers and you want to control CORS policies at the controller or endpoint level you can use **[EnableCors]** and **[DisableCors]** attributes:

```csharp
[ApiController]
[Route("[controller]")]
[EnableCors("CorsPolicy")]
public class ExampleController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(Random.Shared.Next());
}
```

### With Minimal Endpoints

In case you are working with minimal endpoints and you want to control CORS policies at the endpoint or group level you can use **RequireCors()** method:

```csharp
var group = app.MapGroup("products").RequireCors("CorsPolicy");
```

```csharp
app.MapGet("/weatherforecast", () => 
{
    var forecast = GetWeatherForecasts();
    return forecast;
})
.RequireCors("CorsPolicy");
```

## Preflight Requests

A preflight request is an automatic HTTP OPTIONS request sent by the browser before the actual request. It checks whether the actual cross-origin request is safe to send.

When CORS is enabled with the appropriate policy, ASP.NET Core generally responds to CORS preflight requests automatically.

You can set the preflight cache expiration time. The **Access-Control-Max-Age** header specifies how long the response to the preflight request can be cached.

To set this header, call **SetPreflightMaxAge()**:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("MySetPreflightExpirationPolicy",
        policy =>
        {
            policy.WithOrigins("http://example.com")
                   .SetPreflightMaxAge(TimeSpan.FromSeconds(2520));
        });
});
```

## Conclusion

CORS may seem like just another annoying error when building Web APIs, but it's actually a nice browser security feature.

In .NET you can easily apply CORS policies, whether globally through middleware, at the controller level with attributes, or directly on minimal endpoints.

By setting up CORS properly, you ensure that your APIs are both accessible and secure, enabling only the right origins to interact with your backend.

If you want to check out examples I created, you can find the source code here:

[Source Code](https://www.nikolatech.net/codes/cors-example)

I hope you enjoyed it, subscribe and get a notification when a new blog is up!

# Subscribe

Stay tuned for valuable insights every Thursday morning.

##### Share This Article: