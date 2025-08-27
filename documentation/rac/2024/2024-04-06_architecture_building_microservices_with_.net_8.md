```yaml
---
title: Building Microservices With .NET 8
source: https://juliocasal.com/blog/Building-Microservices-With-Dotnet-8?utm_source=convertkit&utm_medium=email&utm_campaign=Welcome%20To%20The%20.NET%20Saturday!%20-%208807570
date_published: 2024-04-06T07:00:00.000Z
date_captured: 2025-08-27T14:51:15.364Z
domain: juliocasal.com
author: Julio Casal
category: architecture
technologies: [.NET 8, ASP.NET Core, Microsoft.Extensions.Http.Resilience, Polly, HttpClient, NuGet, Docker, .NET CLI, Azure Container Registry, OpenTelemetry, Azure Monitor Application Insights, Grafana, .NET Aspire, PostgreSQL, pgAdmin]
programming_languages: [C#]
tags: [microservices, dotnet, resilience, containers, observability, aot, cloud-native, distributed-systems, performance, deployment]
key_concepts: [microservices-architecture, resilience-patterns, containerization, native-aot-compilation, observability, opentelemetry, distributed-tracing, .net-aspire]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores why .NET 8 is an optimal platform for building microservices, highlighting five key features that enhance development. It details improvements in application resilience through `Microsoft.Extensions.Http.Resilience`, streamlined container tooling for Docker image generation, and the performance benefits of Native AOT compilation. The author also covers .NET 8's robust support for observability via OpenTelemetry and introduces .NET Aspire as an opinionated stack for developing cloud-native distributed applications. These advancements collectively position .NET 8 as a powerful and efficient choice for creating scalable, resilient, and observable microservices.
---
```

# Building Microservices With .NET 8

# Building Microservices With .NET 8

Apr 6, 2024

_Read time: 4 minutes_

Today I want to talk about [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) and why I think it is the best version of .NET to build microservices.

[Microservices](https://learn.microsoft.com/azure/architecture/guide/architecture-styles/microservices) are the best way to build cloud-ready systems at scale, and they are especially popular across large organizations that build big and complex systems across multiple teams.

But as with every distributed system, building microservices is hard. Lots of things to worry about, like resilience, deployment, observability, and more.

Fortunately, .NET 8 introduced an impressive set of features that make building microservices with .NET easier than ever.

Let’s dive in.

  

### **1\. Resilient app development**

Resiliency is the ability of an app to recover from transient failures and continue to function.

This is a key feature of microservices because they are essentially distributed systems, and failures are inevitable.

In the past, you had to import external libraries like [Polly](https://github.com/App-vNext/Polly) to add resilience to your microservices, but with .NET 8 there’s a much easier way.

You can install the new [Microsoft.Extensions.Http.Resilience](https://www.nuget.org/packages/Microsoft.Extensions.Http.Resilience) NuGet package into your microservice and then you can do things like this:

```
builder.Services.AddHttpClient<GamesClient>(client =>
{
    client.BaseAddress = new("http://localhost:5115");
})
.AddStandardResilienceHandler();
```

The **AddStandardResilienceHandler()** call adds the standard resilience handler, which uses multiple resilience strategies stacked atop one another, with default options to send the requests and handle any transient errors.

Specifically, it will add default strategies for:

*   Rate limiting
*   Total request timeout
*   Retries
*   Circuit breaking
*   Timeouts per retry

To learn more, check the official docs over [here](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience) or my [YouTube video](https://youtu.be/pgeHRp2Otlc).

  

### **2\. Container tooling**

[Containers](https://learn.microsoft.com/dotnet/core/docker/introduction) are the most popular way to package cloud applications consistently so that they can be deployed anywhere.

They are a fundamental technology for microservices because they allow you to package each microservice into its own container, which can then be deployed and scaled independently.

Native container tooling was introduced in .NET 7, but in .NET 8 it was improved so that is it now easier than ever to generate Docker container images with the .NET CLI.

For instance, this single command:

```
dotnet publish /t:PublishContainer -p ContainerImageTag=1.0.0 \
-p ContainerRegistry=gamestore.azurecr.io
```

Will turn your .NET microservice into a Docker container image, tag it and push it to your Azure Container Registry. No Dockerfile needed!

I created a tutorial based on the .NET 7 container support over [here](https://youtu.be/PtGTU7thBuY), but for the latest on .NET 8 support check [this official doc](https://learn.microsoft.com/en-us/dotnet/core/docker/publish-as-container).

  

### **3\. Native AOT**

[Native AOT (Ahead-Of-Time)](https://learn.microsoft.com/dotnet/core/deploying/native-aot) compilation is the process of producing a self-contained app that has been ahead-of-time (AOT) compiled into native code.

Apps that are published using AOT are smaller in size, use less memory, and can start faster.

This quality is essential for microservices because they are often deployed as containers that are expected to start quickly and use as few resources as possible.

To enable Native AOT for your .NET microservice you’ll need to add the **PublishAot** property to your project file:

```
<PropertyGroup>
  <PublishAot>true</PublishAot>
</PropertyGroup>
```

And then you’ll use the new **CreateSlimBuilder** method, along with the **JSON serializer source generator** to define your HTTP APIs, like in this example:

```csharp
var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

var app = builder.Build();

Game[] games =
[
    new (1,"Street Fighter II",19.99M,new DateOnly(1992, 7, 15)),
    new (2,"Final Fantasy XIV",59.99M,new DateOnly(2010, 9, 30)),
    new (3,"FIFA 23",69.99M,new DateOnly(2022, 9, 27))
];

app.MapGet("games", () => games);

app.Run();

public record class Game(int Id, string Name, decimal Price, DateOnly ReleaseDate);

[JsonSerializable(typeof(Game[]))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}
```

CreateSlimBuilder() will ensure that only the essential features are enabled by default, while the JSON serializer source generator is needed to generate serialization code at build time (required for Native AOT compilation).

More info on Native AOT over [here](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/native-aot).

  

### **4\. Observability and OpenTelemetry**

**Observability** is the ability to monitor your application so that you can tell how well it is performing and to detect potential problems before they become larger.

This is incredibly important for microservices because it can be hard to understand what is happening with them as they interact with each other and with other external services in the cloud.

And that’s where [OpenTelemetry](https://opentelemetry.io) comes in. It is a cross-platform, open standard for collecting and emitting telemetry data.

And .NET has built-in support for OpenTelemetry, so you can easily add it to your microservices to collect and emit telemetry data.

For instance, you can enable OpenTelemetry and export telemetry data to [Azure Monitor Application Insights](https://learn.microsoft.com/en-us/azure/azure-monitor/app/app-insights-overview#application-insights-overview) by installing one NuGet package:

```
dotnet add package Azure.Monitor.OpenTelemetry.AspNetCore
```

And then adding this one line of code to your microservice Program.cs file:

```
builder.Services.AddOpenTelemetry().UseAzureMonitor();
```

That is enough to get **Logging**, **Metrics** and **Distributed Tracing** (the 3 pillars of observability) for your microservice in **Azure**.

That was all available already in .NET 7, but in .NET 8 the team added tons of new useful metrics to ASP.NET Core, and they even created [Grafana](https://grafana.com/grafana) dashboards that are [open source on GitHub](https://aka.ms/dotnet/grafana-source) so you can get something like this for your app:

![Grafana dashboard displaying various ASP.NET Core metrics such as requests duration, error rates, current connections, current requests, total requests, unhandled exceptions, secured requests, HTTP protocol distribution, and top requested/unhandled endpoints.](assets/images/grafana-aspnetcore-dashboard.png)

To learn more, you can check my previous article over [here](https://juliocasal.com/blog/Dont-Be-Blind-In-Prod), [this YouTube video](https://youtu.be/Zg94FgUtmlI) or the official docs [here](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/observability-with-otel).

  

### **5\. .NET Aspire**

[.NET Aspire](https://learn.microsoft.com/dotnet/aspire/get-started/aspire-overview) is an opinionated, cloud-ready stack for building observable, production-ready, distributed applications.

It is available for .NET 8 applications and it essentially adds all the stuff I mentioned above (except for Native AOT) to your microservice by default, so you don’t have to worry about it.

And it adds a lot more stuff to improve the experience of building .NET microservices via a consistent, opinionated set of tools and patterns designed to build and run distributed apps.

It is still in preview at the time of writing, but the .NET team is heavily invested in it, it is getting a lot of amazing support from the community and its built-in dashboard is incredibly useful:

![.NET Aspire dashboard showing a list of resources for a GameStore application, including PostgreSQL containers, a storage container, and backend/frontend projects, along with their status, start time, source, and endpoints.](assets/images/aspire-dashboard.jpg)

To learn more about **.NET Aspire** check out my previous article [here](https://juliocasal.com/blog/Going-Cloud-Native-With-Dotnet-Aspire) and my YouTube videos [here](https://youtu.be/pk6FJfHhfq8) and [here](https://youtu.be/XtWubiUzz-k).

  

### **Summary**

.NET 8 is the best version of .NET to build microservices. In fact, it may be the best platform to build microservices, period.

It has everything you need to build resilient, scalable, observable microservices that can be deployed anywhere, and it is incredibly easy to use.

If you are building microservices with .NET, and you are not using .NET 8, it is definitively time to upgrade.

---

  

**Whenever you’re ready, there are 4 ways I can help you:**

1.  **[.NET Cloud Developer Bootcamp](https://juliocasal.com/courses/dotnetbootcamp)**: A complete path from ASP.NET Core fundamentals to building, containerizing, and deploying production-ready, cloud-native apps on Azure.
    
2.  **​[Building Microservices With .NET](https://dotnetmicroservices.com)**: Transform the way you build .NET systems at scale.
    
3.  **​[​Get the full source code](https://www.patreon.com/juliocasal)**: Download the working project from this newsletter, grab exclusive course discounts, and join a private .NET community.
    
4.  **[Promote your business to 25,000+ developers](https://juliocasal.com/sponsorship)** by sponsoring this newsletter.
    

[](/blog/Building-Microservices-With-Dotnet-8.html)