```yaml
---
title: Monitoring .NET Applications with OpenTelemetry and Grafana
source: https://www.milanjovanovic.tech/blog/monitoring-dotnet-applications-with-opentelemetry-and-grafana?utm_source=LinkedIn&utm_medium=social&utm_campaign=21.07.2025
date_published: 2025-06-21T00:00:00.000Z
date_captured: 2025-08-06T17:48:46.733Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: ai_ml
technologies: [.NET, OpenTelemetry, Grafana, Grafana Cloud, ASP.NET Core, Entity Framework Core, StackExchange.Redis, Npgsql, RabbitMQ, Kafka]
programming_languages: [C#, PowerShell, JSON]
tags: [monitoring, observability, dotnet, opentelemetry, grafana, distributed-tracing, logging, apm, cloud, production]
key_concepts: [observability, distributed-tracing, metrics, logs, OpenTelemetry-Protocol, automatic-instrumentation, alerting, correlation]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to setting up monitoring for .NET applications using OpenTelemetry and Grafana. It details the installation of core OpenTelemetry packages and instrumentation for various libraries like ASP.NET Core, Entity Framework Core, Redis, and Npgsql. The guide explains how to configure OTLP export to Grafana Cloud, including steps to obtain necessary credentials such as the OTLP endpoint and API token. Finally, it demonstrates how to view and analyze collected traces and logs within the Grafana Cloud interface, emphasizing the correlation between logs and traces. The goal is to achieve full observability, enabling developers to effectively diagnose performance bottlenecks and errors in production environments.]
---
```

# Monitoring .NET Applications with OpenTelemetry and Grafana

![Cover image: .NET Monitoring using Grafana + OpenTelemetry with a purple and black abstract background.](/blog-covers/mnw_147.png?imwidth=3840)

# Monitoring .NET Applications with OpenTelemetry and Grafana

Your .NET application is running in production, but you're flying blind.

When something breaks, you're stuck digging through logs, guessing at performance bottlenecks, and trying to piece together what actually happened across your distributed system.

That ends today.

[Grafana](https://grafana.com/) is a complete observability platform that unifies metrics, logs, and traces in one place.

With Grafana, you get:

*   **Unified dashboards** that combine metrics, logs, and traces
*   **Advanced alerting** that actually works when things go wrong
*   **Deep trace analysis** to understand request flows across services
*   **Log correlation** that connects your traces to the exact log entries that matter

[Grafana Cloud](https://grafana.com/products/cloud/) makes this even easier. No infrastructure to manage, automatic scaling, and built-in integrations with [OpenTelemetry](https://opentelemetry.io/). There's a generous free tier that allows you to get started without any upfront costs.

When you combine Grafana with OpenTelemetry, you get vendor-neutral observability that actually delivers insights instead of just pretty charts.

## Setting Up OpenTelemetry in .NET

First, install the core OpenTelemetry packages:

```powershell
Install-Package OpenTelemetry.Extensions.Hosting
Install-Package OpenTelemetry.Exporter.OpenTelemetryProtocol
Install-Package OpenTelemetry.Instrumentation.AspNetCore
Install-Package OpenTelemetry.Instrumentation.Http
```

You can also add instrumentation for other libraries as needed:

```powershell
Install-Package OpenTelemetry.Instrumentation.EntityFrameworkCore
Install-Package OpenTelemetry.Instrumentation.StackExchangeRedis
Install-Package Npgsql.OpenTelemetry
```

Configure OpenTelemetry in your `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(serviceName))
    .WithTracing(tracing =>
    {
        tracing
            .AddAspNetCoreInstrumentation()
            .AddHttpClientInstrumentation()
            .AddEntityFrameworkCoreInstrumentation()
            .AddRedisInstrumentation()
            .AddNpgsql();

        tracing.AddOtlpExporter();
    });

builder.Logging.AddOpenTelemetry(logging =>
{
    logging.IncludeScopes = true;
    logging.IncludeFormattedMessage = true;

    logging.AddOtlpExporter();
});

var app = builder.Build();

// Your app configuration...

app.Run();
```

This configuration:

*   Sets up [**tracing with OpenTelemetry**](introduction-to-distributed-tracing-with-opentelemetry-in-dotnet)
*   Sets up automatic instrumentation for ASP.NET Core and HTTP requests
*   Adds Entity Framework Core and Redis instrumentation
*   Configures PostgreSQL instrumentation if you're using Npgsql
*   Configures OTLP export for traces and logs (we can also add metrics later)

## Configuring OTLP Export to Grafana Cloud

Get your Grafana Cloud credentials:

1.  Log into [Grafana Cloud](https://grafana.com/auth/sign-up/create-user)
    
2.  Go to **My Account** → **Stack Details**
    

![Screenshot showing Grafana Cloud Stack Details page with OTLP endpoint and API Key details highlighted.](/blogs/mnw_147/grafana_setup.png?imwidth=3840)

3.  Find your **OTLP endpoint** (looks like `https://otlp-gateway-prod-eu-west-2.grafana.net/otlp`)

![Screenshot highlighting the OTLP endpoint URL in Grafana Cloud Stack Details.](/blogs/mnw_147/grafana_setup_endpoint.png?imwidth=3840)

4.  Generate an **API token** with permissions

![Screenshot showing how to generate an API token in Grafana Cloud.](/blogs/mnw_147/grafana_setup_token.png?imwidth=3840)

You should also see the environment variables you can use to configure OpenTelemetry:

![Screenshot displaying OpenTelemetry environment variables for Grafana Cloud configuration.](/blogs/mnw_147/grafana_setup_env_vars.png?imwidth=3840)

Configure the OTLP exporter in your `appsettings.json`:

```json
{
  "OTEL_EXPORTER_OTLP_ENDPOINT": "https://otlp-gateway-prod-eu-west-2.grafana.net/otlp",
  "OTEL_EXPORTER_OTLP_PROTOCOL": "http/protobuf",
  "OTEL_EXPORTER_OTLP_HEADERS": "Authorization=Basic <your-base64-encoded-token>"
}
```

You can also set these as environment variables in your hosting environment.

## Viewing and Analyzing Data in Grafana

Start your application and generate some traffic. Now head to your Grafana Cloud instance.

**Traces**

If everything is set up correctly, you should see traces from your application in the **Traces** section.

Here's an example of a trace view in Grafana Cloud. It's a `POST users/register` request that contains multiple spans:

![Screenshot of a distributed trace view in Grafana Cloud, showing multiple spans for a POST request.](/blogs/mnw_147/grafana_trace_1.png?imwidth=3840)

Here's another example of a trace that includes messages sent to a message broker (like RabbitMQ or Kafka).

![Screenshot of a distributed trace in Grafana Cloud, including spans for message broker interactions.](/blogs/mnw_147/grafana_trace_2.png?imwidth=3840)

**Logs**

You can also view logs in Grafana Cloud.

Here's an example of a log view that shows multiple log entries for our application.

![Screenshot of log entries displayed in Grafana Cloud.](/blogs/mnw_147/grafana_logs_1.png?imwidth=3840)

You can drill down into individual log entries, filter by severity, and search for specific terms.

![Screenshot of a detailed log entry in Grafana Cloud, showing trace and span IDs.](/blogs/mnw_147/grafana_logs_2.png?imwidth=3840)

OpenTelemetry automatically correlates your logs with traces. In the trace detail view, click **Logs** to see all log entries that occurred during that request.

Your logs will include trace and span IDs.

## Conclusion

You now have full observability for your .NET application.

When something goes wrong in production, you won't be guessing anymore. You'll see exactly which requests were slow, what errors occurred, and how they propagated through your system.

Grafana gives you the dashboards and alerting to catch problems before users do. OpenTelemetry gives you the detailed traces to understand exactly what happened.

This setup scales from a single service to hundreds of microservices without changing your instrumentation code.

**Ready to take your observability further?**

This article showed you the basics, but modern applications need advanced patterns like distributed tracing across event-driven architectures, custom instrumentation strategies, and observability-driven development practices.

I cover all of this in-depth in my [**Modular Monolith Architecture**](/modular-monolith-architecture) course, where you'll learn how to implement OpenTelemetry across complex, event-driven systems that actually scale in production.

That's all for today.

See you next Saturday.

---

Whenever you're ready, there are 4 ways I can help you:

1.  [Pragmatic Clean Architecture:](/pragmatic-clean-architecture) Join 4,200+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [Modular Monolith Architecture:](/modular-monolith-architecture) Join 2,100+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [(NEW) Pragmatic REST APIs:](/pragmatic-rest-apis) Join 1,100+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
4.  [Patreon Community:](https://www.patreon.com/milanjovanovic) Join a community of 5,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Accelerate Your .NET Skills 🚀

![PCA Cover](/_next/static/media/cover.27333f2f.png?imwidth=384)

Pragmatic Clean Architecture

![MMA Cover](/_next/static/media/cover.31e11f05.png?imwidth=384)

Modular Monolith Architecture

![PRA Cover](/_next/static/media/cover_1.fc0deb78.png?imwidth=384)

Pragmatic REST APIs

NEW