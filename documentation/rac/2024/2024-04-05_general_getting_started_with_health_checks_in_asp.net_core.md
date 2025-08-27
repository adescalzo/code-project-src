```yaml
---
title: Getting started with Health Checks in ASP.NET Core
source: https://okyrylchuk.dev/blog/getting-started-with-health-checks-in-asp-net-core/
date_published: 2024-04-05T08:48:26.000Z
date_captured: 2025-08-20T18:56:51.673Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [ASP.NET Core, Entity Framework, Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore, .NET]
programming_languages: [C#]
tags: [health-checks, aspnet-core, monitoring, web-api, dotnet, entity-framework, middleware, dependency-checks]
key_concepts: [health-checks, http-endpoints, ihealthcheck-interface, dependency-checks, healthstatus, middleware, application-monitoring]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a beginner's guide to implementing Health Checks in ASP.NET Core applications. It explains how health checks expose HTTP endpoints to indicate application and dependency health, crucial for monitoring and load balancing. The guide covers basic setup, creating custom health checks by implementing the `IHealthCheck` interface, and integrating Entity Framework Core for database health checks. It also demonstrates how to configure different health statuses like Healthy, Unhealthy, and Degraded, offering a practical introduction to this essential feature.]
---
```

# Getting started with Health Checks in ASP.NET Core

# Getting started with Health Checks in ASP.NET Core

The Health Checks are essential. Your app exposes them as HTTP endpoints.

Such an endpoint indicates if your app is “healthy” or “unhealthy”. Health checks may also include checks for the health of the API’s dependencies, such as databases, external services, caching systems, etc.

Health Checks can be used by alerting and monitoring systems to provide real-time visibility into the API’s health status. They are also commonly used for traffic routing and load balancing.

ASP.NET Core offers Health Check middleware for easy configuration. You register the Health Check service and map the endpoint for the basic configuration.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHealthChecks();

var app = builder.Build();

app.MapHealthChecks("/healthz");

app.Run();
```

By default, there are no specific health checks to test dependencies. It returns a simple Healthy response in plain text with a 200 OK status code.

![Screenshot showing a successful health check response with 'Healthy' status and 200 OK.](https://ci3.googleusercontent.com/meips/ADKq_NboJYBas9abXhXLHGa_-r3oi7UbOXBgvYHeVzs5XzgU516TMDX8QX23GR2ZRl7CSAODmqRRiYyVSMG4lwgW4v9cQuBejZCqoBKWKwcq04jm-iejrkWGo4MVqQvf9Bgs9XXSTV_Xc0pYPWuVZiH3lXx9rS32cmEGTzd_L7CCG3FlHPt7RSqIfug9-5o=s0-d-e1-ft#https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1712177179899-basic_result.png)

To create a custom health check, you have to implement the **IHealthCheck** interface.

```csharp
public class MyHealthCheck : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
    {
        bool healthy = true;

        if (healthy)
            return Task.FromResult(
                HealthCheckResult.Healthy("The API is healthy"));

        return Task.FromResult(
            HealthCheckResult.Unhealthy("The API is unhealthy"));
    }
}
```

Registration of it is easy.

```csharp
builder.Services
    .AddHealthChecks()
    .AddCheck<MyHealthCheck>("Custom");
```

If you use Entity Framework, you don’t need to write a custom Health Check to probe the connection with a database.

You need to add [Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore](https://www.nuget.org/packages/Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore "Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore") NuGet package.

Then, register the Health Check for your DbContext.

```csharp
builder.Services
    .AddHealthChecks()
    .AddCheck<MyHealthCheck>("Custom")
    .AddDbContextCheck<MyContext>();
```

By default, the database health check calls EF’s **CanConnectAsync** method. You can change it using the **AddDbContextCheck** method overloads.

If your database is unavailable, you’ll get Unhealthy results.

![Expected screenshot showing an 'Unhealthy' health check response, likely due to a database connection issue.](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI1MzUiIGhlaWdodD0iMjc4IiB2aWV3Qm94PSIwIDAgNTM1IDI3OCI+PHJlY3Qgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgc3R5bGU9ImZpbGw6I2NmZDRkYjtmaWxsLW9wYWNpdHk6IDAuMTsiLz48L3N2Zz4=)

In addition to Healthy and Unhealthy failure statuses, there is also a Degraded status. You can change it using the parameter in the **AddCheck** and **AddDbContextCheck** methods or the **HealthCheckResult** in the custom Health Check.

```csharp
builder.Services
    .AddHealthChecks()
    .AddCheck<MyHealthCheck>("Custom")
    .AddDbContextCheck<MyContext>(
        failureStatus: HealthStatus.Degraded);
```

The response:

![Expected screenshot showing a 'Degraded' health check response.](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy5wMy5vcmcvMjAwMC9zdmciIHdpZHRoPSI1MjgiIGhlaWdodD0iMjczIiB2aWV3Qm94PSIwIDAgNTI4IDI3MyI+PHJlY3Qgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgc3R5bGU9ImZpbGw6I2NmZDRkYjtmaWxsLW9wYWNpdHk6IDAuMTsiLz48L3N2Zz4=)

The built-in Health Checks are flexible. You can customize the output, failure status, and HTTP status code.

It’s a good practice to have Health Checks for your applications. If you don’t use them, it’s time to start. I hope this simple tutorial will help you with that.