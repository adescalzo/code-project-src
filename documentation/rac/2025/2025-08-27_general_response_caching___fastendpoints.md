```yaml
---
title: "Response Caching | FastEndpoints"
source: https://fast-endpoints.com/docs/response-caching
date_published: unknown
date_captured: 2025-08-27T15:06:32.271Z
domain: fast-endpoints.com
author: Unknown
category: general
technologies: [FastEndpoints, ASP.NET Core, .NET, Redis]
programming_languages: [C#]
tags: [caching, response-caching, output-caching, aspnet-core, fastendpoints, web-api, performance, middleware, http-headers]
key_concepts: [response-caching, output-caching, middleware, http-headers, endpoint-configuration, dependency-injection, cache-control]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explains how to implement response caching within an ASP.NET Core application using FastEndpoints. It details the necessary steps to enable the response caching middleware and configure caching options on individual endpoints using the `ResponseCache()` method. A crucial distinction is drawn between response caching, which manipulates HTTP headers for client-side and proxy caching, and output caching, which stores responses on the server. Code examples illustrate the setup in `Program.cs` and endpoint configuration, also briefly touching on Minimal APIs' `CacheOutput` feature.
---
```

# Response Caching | FastEndpoints

# Response Caching

In order to get response caching working, you need to enable the response caching middleware and specify options for caching using the **ResponseCache()** method in the endpoint configuration. This method supports all arguments of the **\[ResponseCache\]** attribute you'd typically use with mvc except for the **CacheProfileName** argument as cache profiles are not supported.

**Note:** Response caching is not the same thing as Output caching. Response caching does not store anything on the server itself and simply manipulates headers with instructions on how to cache the responses downstream in web browsers, proxy servers etc. Output caching on the other hand stores responses on the server either in-memory or via a cache storage provider such as Redis. See [this document](https://docs.microsoft.com/en-us/aspnet/core/performance/caching/response?view=aspnetcore-6.0) for more details on response caching in ASP.NET.

Program.cs

```csharp
var bld = WebApplication.CreateBuilder();
bld.Services
   .AddFastEndpoints()
   .AddResponseCaching(); //add this

var app = bld.Build();
app.UseResponseCaching() //add this before FE
   .UseFastEndpoints();
app.Run();
```

MyEndpoint.cs

```csharp
public class MyEndpoint : EndpointWithoutRequest
{
    public override void Configure()
    {
        Get("/api/cached-ticks");
        ResponseCache(60); //cache for 60 seconds
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        return Send.OkAsync(new
        {
            Message = "this response is cached",
            Ticks = DateTime.UtcNow.Ticks
        });
    }
}
```

---

TIP

Minimal APIs [Output caching](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/output?view=aspnetcore-7.0) features can be added to endpoints via the **CacheOutput(...)** extension method.

```csharp
public override void Configure()
{
    // Other configurations...
    Options(x => x.CacheOutput(p => p.Expire(TimeSpan.FromSeconds(60))));
}
```

---

Previous [<- File Handling](/docs/file-handling)

Next [Rate Limiting \->](/docs/rate-limiting)

© FastEndpoints 2025