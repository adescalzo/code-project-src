```yaml
---
title: "Rate Limiting | FastEndpoints"
source: https://fast-endpoints.com/docs/rate-limiting#endpoint-rate-limiting
date_published: unknown
date_captured: 2025-08-27T15:10:53.181Z
domain: fast-endpoints.com
author: Unknown
category: general
technologies: [FastEndpoints, ASP.NET Core, Minimal APIs, GUID, HTTP, Swagger]
programming_languages: [C#]
tags: [rate-limiting, throttling, web-api, dotnet, fastendpoints, http-headers, client-identification, performance, aspnet-core, api-design]
key_concepts: [Rate Limiting, Endpoint Throttling, Client Identification, HTTP Status Codes, Performance Implications, Gateway Rate Limiting, Minimal APIs Rate Limiting, X-Forwarded-For Header]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details how to implement endpoint-specific rate limiting using a library, likely FastEndpoints, by configuring a hit limit and duration. It explains how clients are identified through HTTP headers like "X-Client-Id" or "X-Forwarded-For," or by IP address, and the automatic 429 Too Many Requests response for exceeding limits. The content emphasizes the unreliability of certain headers for client identification and recommends using a unique generated ID. It also outlines limitations, such as its unsuitability for DDoS protection, potential performance degradation, and the lack of global limits, suggesting out-of-process gateway-level solutions for better security and performance. Finally, it briefly mentions ASP.NET Core Minimal APIs' built-in rate limiting features.]
---
```

# Rate Limiting | FastEndpoints

# Rate Limiting

## Endpoint Rate Limiting

It is possible to rate limit individual endpoints based on the presence of an HTTP header in the incoming request like below:

```csharp
public override void Configure()
{
    Post("/order/create");
    Throttle(
        hitLimit: 120,
        durationSeconds: 60,
        headerName: "X-Client-Id" // this is optional
    );
}
```

## Hit Limit & Window Duration

The above for example will only allow 120 requests from each unique client (identified by the header value) within a 60 second window.

If 121 requests are made by a client within 60 seconds, a **429 too many requests** response will be automatically sent for the 121st request.

The counter is reset every 60 seconds and the client is able to make another 120 requests in the next 60 seconds, and so on.

## Header Name

The header name can be set to anything you prefer.

If it's not specified, the library will try to read the value of **X-Forwarded-For** header from the incoming request.

If that's unsuccessful, it will try to read the **HttpContext.Connection.RemoteIpAddress** in order to uniquely identify the client making the request.

If all attempts are unsuccessful, a **403 Forbidden** response will be sent.

## Header Reliability

Both **X-Forwarded-For** and **HttpContext.Connection.RemoteIpAddress** could be unreliable for uniquely identifying clients if they are behind a NAT, reverse proxy, or anonymizing vpn/proxy etc.

Therefore, the recommended strategy is to generate a unique identifier such as a GUID in your client application and use that as the header value in each request for the entirety of the session/app cycle.

## Limitations & Warnings

*   Should not be used for security or DDOS protection. A malicious client can easily set a unique header value per request in order to circumvent the throttling.
    
*   Should be aware of the slight performance degradation due to resource allocation and amount of work being done.
    
*   Only per endpoint limits can be set. No global limits can be enforced. This won't ever be added due to performance implications.
    
*   Consider a rate limiting solution that is out of process at the gateway level for better performance/security.
    

---

**TIP**

Minimal APIs [Rate limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit?view=aspnetcore-7.0) features can be added to endpoints via the **RequireRateLimiting(...)** extension method.

```csharp
public override void Configure()
{
    ...
    Options(x => x.RequireRateLimiting("limiterPolicy"));
}
```

---

Previous [<- Response Caching](/docs/response-caching)

Next [Swagger Support ->](/docs/swagger-support)

© FastEndpoints 2025