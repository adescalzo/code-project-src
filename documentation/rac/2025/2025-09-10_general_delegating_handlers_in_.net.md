```yaml
---
title: Delegating Handlers in .NET
source: https://www.nikolatech.net/blogs/delegating-handlers-in-aspnet-core
date_published: 2025-09-11T00:00:00.000Z
date_captured: 2025-09-11T11:14:14.190Z
domain: www.nikolatech.net
author: Unknown
category: general
technologies: [.NET, HttpClient, ASP.NET Core, Swagger, SwaggerGen, Dependency Injection, CEX.IO API]
programming_languages: [C#]
tags: [httpclient, delegating-handlers, dotnet, aspnet-core, middleware, web-api, logging, dependency-injection, http-requests, cross-cutting-concerns]
key_concepts: [Delegating Handlers, HttpClient pipeline, Cross-cutting concerns, Dependency Injection, Request/Response interception, Logging, Error handling, Resiliency]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces Delegating Handlers in .NET, explaining how they provide middleware-like flexibility to the HttpClient pipeline. It demonstrates their utility in centralizing cross-cutting concerns such as logging, error handling, and retries for HTTP requests. The content provides a practical guide, starting with a basic HttpClient setup and then illustrating how to implement and register a custom `LoggingDelegatingHandler` using dependency injection. By properly attaching these handlers, developers can maintain cleaner, more consistent, and easier-to-manage HTTP integrations within their .NET applications.]
---
```

# Delegating Handlers in .NET

![Banner image for the article titled "HTTP Client Delegating Handlers in .NET"](https://coekcx.github.io/BlogImages/banners/delegating-handlers-in-aspnet-core-banner.png)

#### Delegating Handlers in .NET
###### 11 Sept 2025
###### 5 min

It’s common for APIs to communicate with each other using HTTP requests.

The most straightforward way to handle this in .NET is with [HttpClient](https://www.nikolatech.net/blogs/http-requests-httpclient-dotnet), which is native and easy to get started with.

However, working with HttpClient can be a real drag if you’re not familiar with the right patterns and practices.

**Delegating handlers** bring flexibility to HttpClient, making common tasks easier to implement and maintain.

## Delegating Handlers

A delegating handler is a component that sits in the request/response pipeline of an HttpClient.

![Diagram illustrating the HttpClient request/response pipeline with OuterHandler, InnerHandler, HttpClientHandler, and an external service represented by a globe icon.](https://coekcx.github.io/BlogImages/images/Delegating%20Handlers%20Diagram.png)

Delegating handlers function in a way similar to API middleware, except that they're built for HttpClient.

They are especially useful for centralizing cross-cutting concerns such as logging, resiliency and more.

## Getting Started

Before adding delegating handlers, let’s set up a simple application that integrates with an external service via HTTP requests:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddHttpClient<CryptoApiClient>(client => 
    client.BaseAddress = new Uri("https://cex.io/api/"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapGet("crypto/limits", async (CryptoApiClient cryptoApi, CancellationToken cancellationToken) =>
{
    var response = await cryptoApi.GetLimits(cancellationToken);

    return Results.Ok(response);
});

app.Run();
```

For this example, we’ll create a simple client that fetches data from a crypto API:

```csharp
public class CryptoApiClient(HttpClient client)
{
    public async Task<CurrencyLimitResponse?> GetLimits(CancellationToken cancellationToken = default) =>
        await client.GetFromJsonAsync<CurrencyLimitResponse>(
            "currency_limits",
            cancellationToken);
}
```

## Adding Delegating Handler

With the setup complete, we can now add a delegating handler:

```csharp
public class LoggingDelegatingHandler(ILogger<LoggingDelegatingHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        logger.LogInformation("Sending HTTP {Method} request to {Url}", request.Method, request.RequestUri);

        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            logger.LogInformation("Received {StatusCode} from {Url}", response.StatusCode, request.RequestUri);

            return response;
        }
        catch (Exception e)
        {
            logger.LogError(e, "Request to {Url} failed with exception", request.RequestUri);
            throw;
        }
    }
}
```

To define a delegating handler, we use **DelegatingHandler**, which is a base class that allows you to intercept, process, or modify HTTP requests and responses within the HttpClient pipeline.

Next, register the handler with dependency injection:

```csharp
builder.Services.AddTransient<LoggingDelegatingHandler>();
```

Each delegating handler should be registered as transient. Once registered, you can attach it to your HttpClient:

```csharp
builder.Services.AddHttpClient<CryptoApiClient>(client =>
        client.BaseAddress = new Uri("https://cex.io/api/"))
    .AddHttpMessageHandler<LoggingDelegatingHandler>();
```

**NOTE:** If you register multiple handlers, they’ll be executed in the same order as they were added.

Now, whenever the CryptoApiClient sends a request, it will first pass through the LoggingDelegatingHandler.

## Conclusion

Delegating handlers bring the flexibility of middleware to your HttpClient pipeline.

They make it easy to centralize and manage common concerns like logging, error handling, retries and more.

By registering and attaching them properly, you can keep your HTTP integrations clean, consistent and easier to maintain.

If you want to check out examples I created, you can find the source code here:

[Source Code](https://www.nikolatech.net/codes/delegating-handler-examples)