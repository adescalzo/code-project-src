```yaml
---
title: API Key Authentication in ASP.NET Core
source: https://www.nikolatech.net/blogs/api-key-authentication-in-aspnet-core
date_published: 2025-09-04T00:00:00.000Z
date_captured: 2025-09-05T11:31:33.894Z
domain: www.nikolatech.net
author: Unknown
category: security
technologies: [ASP.NET Core, .NET, IOptions, IMiddleware, IAuthorizationFilter, IEndpointFilter, appsettings.json, HTTP]
programming_languages: [C#]
tags: [api-key, authentication, aspnet-core, security, middleware, authorization-filter, endpoint-filter, web-api, dotnet]
key_concepts: [api-key-authentication, http-request-pipeline, middleware, authorization-filters, endpoint-filters, minimal-apis, controllers, options-pattern, dependency-injection]
code_examples: false
difficulty_level: intermediate
summary: |
  [API Key authentication offers a straightforward method to secure access to APIs, particularly suitable for internal projects or low-risk environments. This article explores three distinct approaches to implement API Key authentication in ASP.NET Core: using custom Middleware, leveraging the IAuthorizationFilter for controller-based APIs, and employing the IEndpointFilter for minimal APIs. Each method provides varying levels of flexibility and control over where authentication is applied within the request pipeline. While effective for simple access control, API Keys are not recommended for public-facing APIs or scenarios requiring granular, user-specific permissions. The article includes code examples for each implementation strategy.]
---
```

# API Key Authentication in ASP.NET Core

# In this article

*   [Intro](#Intro)
*   [API Key Authentication](#ApiKeyAuthentication)
*   [Getting Started](#GettingStarted)
*   [Middleware](#MiddlewareValidation)
*   [IAuthorizationFilter](#IAuthorizationFilterValidation)
*   [IEndpointFilter](#IEndpointFilterValidation)
*   [Conclusion](#Conclusion)

![Banner: A blue and black banner with the text "API Key Authentication in ASP.NET Core" and a shield icon with a key.](https://coekcx.github.io/BlogImages/banners/api-key-authentication-in-aspnet-core-banner.png)

#### API Key Authentication in ASP.NET Core

###### 04 Sept 2025

###### 8 min

**API Key authentication** is one of the simplest ways to secure access to an API.

It works by issuing a unique key, which must be included with request.

This method ensures that only clients/applications with valid keys can interact with the API.

It's most commonly used in internal projects and lower risk environments where simplicity is more important.

In .NET there are several simple approaches to implement API key authentication.

## API Key Authentication

API Key is commonly used because they:

*   Are easy to generate and use
*   Don’t require a full authentication flow
*   Work well for service-to-service communication inside a network.

However, as any tool they are not a silver bullet:

*   They can be stolen if leaked
*   They are not a good aproach for requests to a specific user, just to the key
*   They are not a good appraoch for granular permissions, just for access

Basically for internal APIs, API Key Authentication is great but for public and sensitive APIs I strongly recomend checking out alternative approaches:

*   [JWT Authentication](https://www.nikolatech.net/blogs/implement-jwt-authentication-in-dotnet)
*   [Role-Based Authorization](https://www.nikolatech.net/blogs/role-based-authorization-aspnetcore)

## Getting Started

To get started, you don’t need any additional libraries.

All that’s required is a securely stored API key and logic to validate it.

In this example, the API key is stored inside appsettings.json:

```csharp
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "Auth": {
    "ApiKey": 
    "d784da6c-c2e6-4046-93f7-9052a8b40b79"
  }
}
```

For validation, regardless of the presentation style you choose, you can always rely on **Middleware**. Just ensure it’s registered first in the pipeline.

If you’re using **controllers**, attributes provide a convenient way to enforce API key validation.

Lastly, if you’re using **minimal APIs**, the IEndpointFilter interface provides a clean way to implement API key authentication.

## Validate API Key with Middleware

Middlewares are simple and unified approach for API Key authentification in .NET.

Only downside with this approach is if you have endpoints that require API Key authentification and endpoints that don't require them since middlewares will be applied to every request by default.

Here's an example of validaiton of api key using middleware:

```csharp
public class AuthApiKeyMiddleware : IMiddleware
{
    private const string HeaderKey = "x-api-key";

    private readonly AuthOptions _options;

    public AuthApiKeyMiddleware(IOptions<AuthOptions> options) =>
        _options = options.Value;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        if (!context.Request.Headers.TryGetValue(HeaderKey, out var apiKey))
        {
            context.Response.StatusCode = 401;
            return;
        }

        if (_options.ApiKey != apiKey)
        {
            context.Response.StatusCode = 401;
            return;
        }

        await next(context);
    }
}
```

I am big fan of the options pattern, which is why I used **IOptions<T>** to inject configuration from appsettings.json.

I'm also a big fan of **IMiddleware** (factory approach) to implement middleware, to learn more about middlewares checkout: [Middlewares in ASP.NET Core](https://www.nikolatech.net/blogs/middlewares-in-aspnetcore)

In short, middleware sits in the HTTP request pipeline and can inspect, modify or reject requests before passing them further.

In middleware we expect the requests to contain header named x-api-key.

If header is not present or doesn't match expected value for api key we will reject the request immediately with 401 Unauthorized.

If the key is valid, the request is passed down the pipeline to the next middleware or endpoint.

## Validate API Key with IAuthorizationFilter

If you are using controllers, you can use **IAuthorizationFilter** and add an attribute to easily apply authentication at the controller or endpoint level.

This approach solves the lack of flexibility that middlewares have:

```csharp
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ControllerAuthApiKeyAttribute : Attribute, IAuthorizationFilter
{
    private const string HeaderKey = "x-api-key";

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        if (!httpContext.Request.Headers.TryGetValue(HeaderKey, out var apiKey))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        var authApiKeyOptions = httpContext.RequestServices
            .GetRequiredService<IOptions<AuthOptions>>()
            .Value;

        if (authApiKeyOptions.ApiKey != apiKey)
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
```

**AttributeUsage** defines a custom attribute that can be applied to a controller class or an action method.

It implements **IAuthorizationFilter** which allows it to run before the action executes to handle the authorization logic.

Just like in the middleware version, this attribute expects the API key to be sent in the request header named x-api-key.

If header is not present or keys don't match, it immediately stops execution and returns 401 Unauthorized.

Now you can apply this filter globaly in Program.cs if you want to:

```csharp
builder.Services.AddControllers(x =>
    x.Filters.Add<ControllerAuthApiKeyAttribute>());
```

Additionally, for a more granular approach you can apply the attribute at the controller level afffecting all endpoints within the controller:

```csharp
[ControllerAuthApiKey]
public class ExampleController : ControllerBase
{
    // ...
}
```

Or you could apply the attribute at the endpoint level:

```csharp
public class ExampleController : ControllerBase
{
    [HttpGet("controller-example")]
    [ControllerAuthApiKey]
    public IActionResult GetExample(string someValue)
    {
        // ...
    }
}
```

## Validate API Key with IEndpointFilter

If you are using minimal API, you can use **IEndpointFilter** to apply authentication at the group or endpoint level.

This approach also solves the lack of flexibility that middlewares have:

```csharp
public class MinimalApiAuthApiKeyFilter : IEndpointFilter
{
    private const string HeaderKey = "x-api-key";

    private readonly AuthOptions _options;

    public MinimalApiAuthApiKeyFilter(IOptions<AuthOptions> options) =>
        _options = options.Value;

    public async ValueTask InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(HeaderKey, out var apiKey))
        {
            return Results.Unauthorized();
        }

        if (_options.ApiKey != apiKey)
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}
```

IEndpointFilter lets you intercept requests before they reach the endpoint handler.

To apply filters to an entire endpoint group:

```csharp
var group = app.MapGroup("example")
    .AddEndpointFilter<MinimalApiAuthApiKeyFilter>();
```

To apply filters to an individual endpoints:

```csharp
app.MapGet("minimalapi-example", (string someString) => Results.Ok(someString))
    .AddEndpointFilter<MinimalApiAuthApiKeyFilter>();
```

## Conclusion

API Key authentication is a simple, effective way to secure access to APIs, especially in internal projects or low risk environments.

In .NET, there are several approaches to implement API key authentication, each with its own advantages.

However, keep in mind, they are not suitable for public APIs or scenarios requiring user specific access or granular permissions.

If you want to check out examples I created, you can find the source code here:

[Source Code](https://www.nikolatech.net/codes/api-key-authentication-example)