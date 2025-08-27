```yaml
---
title: "Health Checks for ASP.NET Core and Entity Framework Core | Khalid Abuhakmeh"
source: https://khalidabuhakmeh.com/health-checks-for-aspnet-core-and-entity-framework-core?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=the-memento-design-pattern-in-c
date_published: 2024-07-23T00:00:00.000Z
date_captured: 2025-08-08T13:24:49.142Z
domain: khalidabuhakmeh.com
author: Khalid Abuhakmeh
category: ai_ml
technologies: [ASP.NET Core, Entity Framework Core, .NET Aspire, Microsoft.Extensions.Diagnostics.HealthChecks, Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore, NuGet]
programming_languages: [C#, SQL]
tags: [health-checks, aspnet-core, entity-framework-core, dotnet, monitoring, web-api, database, middleware, dependency-injection, diagnostics]
key_concepts: [health-checks, middleware, dependency-injection, IHealthCheck-interface, DbContext, asynchronous-operations, application-monitoring, system-diagnostics]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article details how to implement health checks in ASP.NET Core applications using the `Microsoft.Extensions.Diagnostics.HealthChecks` package. It guides readers through setting up the health check middleware, registering custom health checks via the `IHealthCheck` interface or inline functions, and integrating them into the application's request pipeline. A key focus is on performing database health checks for Entity Framework Core applications using a dedicated extension package and custom queries. The author highlights the importance of efficient checks and mentions the need for external monitoring tools to leverage these endpoints effectively.]
---
```

# Health Checks for ASP.NET Core and Entity Framework Core | Khalid Abuhakmeh

July 23, 2024

# Health Checks for ASP.NET Core and Entity Framework Core

![A yellow toy ambulance on a concrete-like surface, with text overlay "Health Checks for ASP.NET Core and Entity Framework Core" on a gradient background. This image visually represents the "health" aspect of the article.](https://res.cloudinary.com/abuhakmeh/image/fetch/c_limit,f_auto,q_auto,w_800/https://khalidabuhakmeh.com/assets/images/posts/misc/aspnet-core-ef-core-health-checks.jpg) Photo by [Zhen H](https://unsplash.com/@zhenh2424)

I’ve recently been reading up on .NET Aspire and have found a lot of cool .NET tech underpinning the offering. One of the tools that has gotten a glow-up since I last looked at it has been `Microsoft.Extensions.Diagnostics.HealthChecks` package provides you with the infrastructure to perform various types of system health monitoring.

In this post, we’ll look at installing the health checks package into existing ASP.NET Core applications and using an additional package to perform health checks on your databases using Entity Framework Core.

## [Install and Set Up Health Checks](#install-and-set-up-health-checks)

In an existing ASP.NET Core application, you’ll need to install the following package.

```
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks
```

Once the package has been installed, you must do several setup tasks in your `Program` file.

The first step is to register the `HealthChecks` middleware and services in the services collection.

```csharp
builder.Services.AddHealthChecks();
```

C#

Copy

Next, in your ASP.NET Core request pipeline, you’ll need to register the middleware. You can choose whether to place the middleware before or after the authorization middleware, but as you’ll see, there might not be much reason to have this behind an auth check.

You’ll need to choose a URL path to access your health check. I used the `/health` path for this case, but feel free to choose whatever suits you.

```csharp
app.UseRouting();

app.UseHealthChecks("/health");

app.UseAuthorization();
```

C#

Copy

Navigating to the `/health` endpoint will get you the following response.

```
Healthy
```

The response will always be a string of `Healthy`, `Unhealthy`, or `Degraded`. This makes it simple for third-party health check systems to determine your application’s general health quickly. Your logging system will handle the details of the issues.

Let’s write a new health check! You’ll need to implement the `IHealthCheck` interface and implement the `CheckHealthAsync` method.

```csharp
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Healthnut.Services;

public class KhalidHealthCheck: IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = new CancellationToken())
    {
        return Task.FromResult(
            HealthCheckResult.Degraded("Khalid ate some ice cream")
        );
    }
}
```

C#

Copy

These classes are added to your services collection, so they can opt-in to any dependency injection mechanisms you might want to use. Let’s add it to our health checks. Modify the registration in the `Program` file.

```csharp
builder.Services
    .AddHealthChecks()
    .AddCheck<KhalidHealthCheck>("Khalid");
```

C#

Copy

Note that health checks require a unique name. You’ll see a `Degraded` string after rerunning the app and hitting the `/health` endpoint. What’s more interesting is that you’ll see a new log message on your terminal.

```
warn: Microsoft.Extensions.Diagnostics.HealthChecks.DefaultHealthCheckService[103]
      Health check Khalid with status Degraded completed after 2.5366ms with message 'Khalid ate some ice cream'
```

Health checks are `async`, so you can write any logic you need to determine the health of your system, but be sure it’s snappy and non-resource-intensive checks; otherwise, ironically, the health checks could lead to unhealthy results.

If you hate the formalities of classes and interfaces, you can also choose to implement health checks directly at the point of registration. This can be helpful for microservices or smaller utility applications.

```csharp
builder.Services
    .AddHealthChecks()
    .AddCheck<KhalidHealthCheck>("Khalid")
    .AddAsyncCheck("Butter", async () =>
    {
        await Task.Delay(1000);
        return new HealthCheckResult(HealthStatus.Healthy, "Butter is good");
    });
```

C#

Copy

So far, it’s been good, but let’s now use health checks for our database.

## [Health checks for Entity Framework Core](#health-checks-for-entity-framework-core)

You’ll need to install a new package to get a new extension method for EF Core health checks. You should already have an Entity Framework Core application with a `DbContext` implementation. If you don’t, create one. Now, the package.

```
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks.EntityFrameworkCore
```

Once installed, we can update our health check registration using the following line.

```csharp
builder.Services
    .AddHealthChecks()
    .AddCheck<KhalidHealthCheck>("Khalid")
    .AddAsyncCheck("Butter", async () =>
    {
        await Task.Delay(1000);
        return new HealthCheckResult(HealthStatus.Healthy, "Butter is good");
    })
    .AddDbContextCheck<Database>(
        "people check",
        customTestQuery: (db, token) => db.People.AnyAsync(token)
    );
```

C#

Copy

The `AddDbContextCheck` takes a generic argument of a `DbContext` implementation, and the `customTestQuery` argument is a query you can execute to verify the health of your database. The provided query must return a single `bool` value. Use logical LINQ operators such as `Any` and `All`, or write queries that evaluate entirely on the server with limited results returned. Oh, and keep these queries snappy and to the point. Doing expensive database checks may impact the health of your application.

Rerunning the application, we’ll see the log message we expect.

```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand (14ms) [Parameters=[], CommandType='Text', CommandTimeout='30']
      SELECT EXISTS (
          SELECT 1
          FROM "People" AS "p")
fail: Microsoft.Extensions.Diagnostics.HealthChecks.DefaultHealthCheckService[103]
      Health check people check with status Unhealthy completed after 1545.2538ms with message '(null)'
```

Neat!

## [Conclusion](#conclusion)

We barely scratched the surface of what’s possible with the health checks feature of ASP.NET Core, and there’s so much more you can do to provide real-time health updates of your applications. There’s also a wide variety of health check extensions on NuGet, so you’ll rarely have to write your own, but it’s nice knowing you could with such a straightforward interface.

Finally, It’s important to remember that you’ll need a monitoring tool to watch these endpoints to get the most out of them, but I’ll leave that decision up to you.

As always, thanks for reading and cheers.

Tags: [aspnet](/tag/aspnet/) [efcore](/tag/efcore/)

![A headshot of Khalid Abuhakmeh, the author.](/assets/images/authorimage.jpg)

## About Khalid Abuhakmeh

Khalid is a developer advocate at JetBrains focusing on .NET technologies and tooling.

## Read Next

July 16, 2024

### [How To Fix .NET Nullability Warnings For Guarded Members](/how-to-fix-dotnet-nullability-warnings-for-guarded-members)

![A code snippet showing C# code with nullability warnings, illustrating the topic of fixing .NET nullability issues.](https://res.cloudinary.com/abuhakmeh/image/fetch/c_limit,f_auto,q_auto,w_500/https://khalidabuhakmeh.com/assets/images/posts/misc/fix-nullability-warnings-dotnet-csharp.jpg)

July 30, 2024

### [Fix .NET MAUI MissingEntitlement and Provisioning Profiles Issues](/fix-dotnet-maui-missingentitlement-and-provisioning-profiles-issues)

![A mobile phone screen displaying an error message related to MissingEntitlement and Provisioning Profiles, indicating a problem with a .NET MAUI application.](https://res.cloudinary.com/abuhakmeh/image/fetch/c_limit,f_auto,q_auto,w_500/https://khalidabuhakmeh.com/assets/images/posts/misc/missingentitlement-provisioning-dotnet-maui-issues-help.jpg)