```yaml
---
title: Enrich Logs with CorrelationId in ASP.NET Core
source: https://www.nikolatech.net/blogs/enrich-logs-with-correlation-id
date_published: 2025-08-07T00:00:00.000Z
date_captured: 2025-08-08T12:29:15.903Z
domain: www.nikolatech.net
author: Unknown
category: general
technologies: [ASP.NET Core, Serilog, .NET, HttpClient, HTTP]
programming_languages: [C#]
tags: [logging, correlation-id, aspnet-core, middleware, distributed-systems, observability, debugging, monitoring, http-headers, serilog]
key_concepts: [correlation-id, asp.net-core-middleware, distributed-tracing, logging-context, http-header-propagation, ihttpcontextaccessor, delegatinghandler]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explains how to implement correlation IDs in ASP.NET Core applications to enhance debugging and monitoring in distributed systems. It details creating a custom middleware to assign and propagate a unique identifier for each request, either from incoming headers or using the built-in TraceIdentifier. The guide also covers configuring Serilog to include the correlation ID in log output and demonstrates how to propagate this ID when making requests to external services using HttpClient and DelegatingHandlers. This approach ensures all related log entries across multiple services can be easily linked, significantly improving traceability and troubleshooting.]
---
```

# Enrich Logs with CorrelationId in ASP.NET Core

![A banner image with a blue background and abstract shapes. The text "Correlation ID" is prominently displayed in black, with "Enriching Logs in ASP.NET" below it in white. A stylized "NK" logo is in the top left corner, and a white log file icon is in the bottom left.](https://coekcx.github.io/BlogImages/banners/enrich-logs-with-correlation-id-banner.png)

#### Enrich Logs with CorrelationId in ASP.NET Core

###### 07 Aug 2025

###### 6 min

When building distributed applications, tracking requests across multiple services can be challenging.

For example, a request to place an order might first hit an API gateway, then go through an authentication service, payment processor, inventory system and finally a shipping service.

When something goes wrong, it becomes very difficult to trace where the failure happened.

Fortunately, the solution is quite simple to implement.

Assigning a unique identifier to the request at the entry point and carrying it through each service allows all related log entries across the system to be linked to that single request.

## Correlation ID

**Correlation ID** is a unique identifier that helps you trace a request as it flows through your services and their dependencies.

By enriching your logs with correlation IDs, you can easily correlate log entries across different components, making debugging and monitoring much more effective.

This is especially valuable in microservices architectures where a single request might touch multiple services.

It allows you to:

*   Track a request across multiple services
*   Correlate log entries from different components
*   Debug issues more effectively

The correlation ID is typically included in HTTP headers, making it easy to propagate across service boundaries.

## Implementing Correlation ID Middleware

The most effective way to implement correlation IDs in .NET is through middleware. This ensures that every request gets a correlation ID automatically.

Here's how to create a correlation ID middleware:

```csharp
public class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.Headers.TryGetValue("correlation-id-header", out var correlationIds);
        var correlationId = correlationIds.FirstOrDefault() ?? context.TraceIdentifier;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
```

This middleware does several important things:

*   Checks if a correlation ID is already present in the request headers
*   If not found, uses the built-in TraceIdentifier as a fallback
*   Pushes the correlation ID into the logging context using Serilog's LogContext
*   Ensures the correlation ID is available throughout the request pipeline

The **LogContext.PushProperty** method is crucial here, it makes the correlation ID available to all log entries within the request scope.

## Registering the Middleware

To use this middleware, you need to register it in your application pipeline:

```csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

// Use the middleware early in the pipeline
app.UseMiddleware<CorrelationIdMiddleware>();

app.Run();
```

**NOTE:** It's important to register this middleware early in the pipeline, ideally right after UseRouting, to ensure the correlation ID is available for all subsequent middleware and controllers.

## Configuring Serilog

To see the correlation ID in your logs, you need to configure Serilog to include it in the output template:

```csharp
builder.Host.UseSerilog((_, configuration) =>
    configuration
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3} {CorrelationId}] {Message:lj}{NewLine}{Exception}")
        .MinimumLevel.Information()
        .Enrich.FromLogContext());
```

With this configuration, your logs will look like:

```logs
[14:44:51 INF ] Request starting HTTP/2 GET https://localhost:7208/api/Orders - null null
[14:44:51 INF 0HNEDQHS7532O:00000019] Executing endpoint 'CorrelationId.Api.Controllers.OrdersController.Get (CorrelationId.Api)'
[14:44:51 INF 0HNEDQHS7532O:00000019] Route matched with { action = "Get", controller = "Orders" }. Executing controller action with signature Microsoft.AspNetCore.Mvc.IActionResult Get() on controller CorrelationId.Api.Controllers.OrdersController (CorrelationId.Api).
[14:44:51 INF 0HNEDQHS7532O:00000019] Get Endpoint Started
[14:44:51 INF 0HNEDQHS7532O:00000019] Get Endpoint Finished
[14:44:51 INF 0HNEDQHS7532O:00000019] Executing StatusCodeResult, setting HTTP status code 200
[14:44:51 INF 0HNEDQHS7532O:00000019] Executed action CorrelationId.Api.Controllers.OrdersController.Get (CorrelationId.Api) in 2.8194ms
[14:44:51 INF 0HNEDQHS7532O:00000019] Executed endpoint 'CorrelationId.Api.Controllers.OrdersController.Get (CorrelationId.Api)'
[14:44:51 INF ] Request finished HTTP/2 GET https://localhost:7208/api/Orders - 200 0 null 9.5752ms
```

## Propagating to External Services

When making HTTP requests to external services, you should propagate the correlation ID to maintain traceability:

```csharp
public class CorrelationIdDelegatingHandlers(IHttpContextAccessor httpContextAccessor) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var correlationId = httpContextAccessor.HttpContext?.Items["CorrelationId"]?.ToString() ?? httpContextAccessor.HttpContext?.TraceIdentifier;

        if (!string.IsNullOrEmpty(correlationId))
        {
            request.Headers.Add("correlation-id-header", correlationId);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
```

You also need to register the **IHttpContextAccessor** in your services:

```csharp
builder.Services.AddHttpContextAccessor();
```

And update your middleware to store the correlation ID in **HttpContext.Items**:

```csharp
public class CorrelationIdMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.Headers.TryGetValue("correlation-id-header", out var correlationIds);
        var correlationId = correlationIds.FirstOrDefault() ?? context.TraceIdentifier;

        context.Items["CorrelationId"] = correlationId;

        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await next(context);
        }
    }
}
```

Now just register this handler with your **HttpClient**:

```csharp
builder.Services
    .AddHttpClient("CorrelationId.Api", client => client.BaseAddress = new Uri("https://localhost:7208"))
    .AddHttpMessageHandler<CorrelationIdDelegatingHandlers>();
```

## Conclusion

Correlation IDs are a simple yet powerful tool for improving observability in distributed applications.

By implementing the middleware pattern shown in this blog, you can automatically enrich all your logs with correlation IDs, making debugging and monitoring much more effective.

The combination of .NET middleware and Serilog's LogContext provides a clean, maintainable solution that scales well with your application.

If you want to check out examples I created, you can find the source code here:

[Source Code](https://www.nikolatech.net/codes/correlation-id-example)