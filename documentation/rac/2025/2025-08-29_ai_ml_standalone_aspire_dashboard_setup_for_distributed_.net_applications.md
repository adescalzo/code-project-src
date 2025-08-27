```yaml
---
title: Standalone Aspire Dashboard Setup for Distributed .NET Applications
source: https://www.milanjovanovic.tech/blog/standalone-aspire-dashboard-setup-for-distributed-dotnet-applications?utm_source=newsletter&utm_medium=email&utm_campaign=tnw157
date_published: 2025-08-30T00:00:00.000Z
date_captured: 2025-08-30T19:48:24.720Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: ai_ml
technologies: [Aspire Dashboard, .NET, Docker Compose, Kubernetes, OpenTelemetry, OTLP, Jaeger, Prometheus, Application Insights, Npgsql, ASP.NET Core, gRPC]
programming_languages: [C#, YAML]
tags: [dotnet, aspire, observability, distributed-systems, opentelemetry, docker-compose, logging, tracing, metrics, development-tools]
key_concepts: [distributed-tracing, structured-logging, real-time-metrics, cloud-native-development, containerization, telemetry-visualization, microservices, local-development]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details how to set up the .NET Aspire Dashboard as a standalone container for local development and debugging of distributed .NET applications. It explains the benefits of using the dashboard for drop-in observability, offering distributed tracing, structured logging, and real-time metrics via OpenTelemetry without requiring Aspire's full orchestration. The guide provides practical steps, including Docker Compose configuration for the dashboard and environment variables for services, along with C# code for OpenTelemetry instrumentation. While ideal for development due to its in-memory nature, the article also contrasts it with production-grade observability solutions like Jaeger and Prometheus. The standalone dashboard offers immediate value for understanding application behavior and identifying bottlenecks.]
---
```

# Standalone Aspire Dashboard Setup for Distributed .NET Applications

![Standalone Aspire Dashboard Setup for Distributed .NET Applications](/blog-covers/mnw_157.png?imwidth=3840)

# Standalone Aspire Dashboard Setup for Distributed .NET Applications

4 min read · August 30, 2025

**Meet Auggie CLI**. Augment Code is bringing the power of its AI coding agent and context engine right to your terminal. You can use Auggie in a standalone interactive terminal session alongside your favorite editor or in any part of your software development stack. **Auggie CLI is now generally available**.

**Build AI Agents with Postman**. Design, test, and launch AI agents effortlessly with Postman. Use trusted APIs and no-code tools to build workflows and transform them into agent-ready code. Postman gives you everything you need to design, test and launch AI Agents—all in one platform. **Start Building**.

You've built a distributed .NET application. Multiple services, databases, message queues. Now something's slow, and you need to figure out why.

**The Aspire Dashboard runs perfectly as a standalone container**, giving you [**distributed tracing**](introduction-to-distributed-tracing-with-opentelemetry-in-dotnet), [**structured logs**](5-serilog-best-practices-for-better-structured-logging), and real-time metrics without the full orchestration framework.

While [**Aspire's orchestration**](dotnet-aspire-a-game-changer-for-cloud-native-development) is incredibly powerful for managing distributed applications, sometimes you just need the observability piece. Maybe you're already using [**Docker Compose**](using-dotnet-aspire-with-the-docker-publisher) or [Kubernetes](https://doineedkubernetes.com/). Maybe you're debugging an existing system. The standalone dashboard gives you valuable telemetry visualization with minimal setup.

Let's get it running in under 5 minutes.

## Why Run the Aspire Dashboard Standalone?

Most teams already have their deployment story figured out. Docker Compose, Kubernetes, or some platform-specific orchestration. You don't want to rewrite everything just to get observability.

The standalone **Aspire Dashboard** hits a sweet spot **for development**:

*   **Drop-in observability** - Just add a container to your existing setup
*   **Full OpenTelemetry support** - Works with any OTLP-compatible application
*   **Developer-friendly** - Designed for local development and debugging
*   **Immediate value** - See traces, logs, and metrics within minutes

One caveat: it's **in-memory only**. Perfect for development and debugging, not for production. For production, you'll want something like [**Jaeger**](introduction-to-distributed-tracing-with-opentelemetry-in-dotnet), [Prometheus](https://prometheus.io/), or a commercial APM solution.

But for understanding what your code is doing right now? It's exactly what you need.

## Step 1: Add the Dashboard Container

Drop this into your `docker-compose.yml`:

```yaml
aspire-dashboard:
  container_name: aspire-dashboard
  image: mcr.microsoft.com/dotnet/aspire-dashboard:9.0
  ports:
    - 18888:18888
```

That's it. The dashboard is running. Navigate to `http://localhost:18888` and... you'll need a token.

![Aspire Dashboard login screen showing a prompt for an authentication token](blogs/mnw_157/aspire_dashboard_login.png?imwidth=3840)

**Check the container logs** for the login link. The dashboard generates a unique authentication token on startup:

![Aspire Dashboard container logs showing the login URL with an authentication token](blogs/mnw_157/aspire_dashboard_login_link.png?imwidth=3840)

Click that link, and you're in. Empty for now, but not for long.

## Step 2: Wire Up Your .NET Services

Your services need to know where to send their telemetry. Add these environment variables to your API containers:

```yaml
users.api:
  image: ${DOCKER_REGISTRY-}usersapi
  build:
    context: .
    dockerfile: Users.Api/Dockerfile
  ports:
    - 5100:5100
    - 5101:5101
  environment:
    - OTEL_EXPORTER_OTLP_ENDPOINT=http://aspire-dashboard:18889
    - OTEL_EXPORTER_OTLP_PROTOCOL=grpc
  depends_on:
    - users.database
```

Notice port `18889`? That's the OTLP ingestion endpoint. The dashboard listens on `18888` for the UI, `18889` for telemetry data.

## Step 3: Configure OpenTelemetry in Your Code

Install the necessary [OpenTelemetry packages](https://www.nuget.org/packages?q=OpenTelemetry):

```xml
<PackageReference Include="Npgsql.OpenTelemetry" Version="9.0.3" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.12.0" />
<PackageReference Include="OpenTelemetry.Extensions.Hosting" Version="1.12.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.12.0" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.12.0" />
```

Then configure OpenTelemetry in your `Program.cs`:

```csharp
builder.Services.AddOpenTelemetry()
    .ConfigureResource(resource => resource.AddService(builder.Environment.ApplicationName))
    .WithTracing(tracing => tracing
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation()
        .AddNpgsql())
    .WithMetrics(metrics => metrics
        .AddHttpClientInstrumentation()
        .AddAspNetCoreInstrumentation());

builder.Logging.AddOpenTelemetry(options =>
{
    options.IncludeScopes = true;
    options.IncludeFormattedMessage = true;
});

builder.Services.AddOpenTelemetry().UseOtlpExporter();
```

This configuration:

*   **Traces** HTTP calls, ASP.NET Core requests, and database queries
*   **Collects metrics** on request duration, response codes, and throughput
*   **Structured logging** with full context and formatted messages
*   **Exports everything** to the Aspire Dashboard via OTLP

The `UseOtlpExporter()` method automatically picks up the `OTEL_EXPORTER_OTLP_ENDPOINT` environment variable you configured earlier.

## What You Get

Start your application and make a few requests. The dashboard immediately lights up with data.

### Structured Logs

Every log entry includes full context: trace IDs, request paths, user identities. Click any log to see the complete structured data.

![Aspire Dashboard displaying structured logs with details like trace IDs, span IDs, and formatted messages](blogs/mnw_157/structured_logs.png?imwidth=3840)

### Distributed Traces

See the complete request flow across all your services. Which database query is slow? Which HTTP call is failing? The trace view shows you exactly where time is spent.

![Aspire Dashboard showing a distributed trace visualization, illustrating the flow of a request across multiple services and their durations](blogs/mnw_157/distributed_traces.png?imwidth=3840)

You can click into a trace to see the individual spans and any metadata associated with them.

![Aspire Dashboard displaying detailed information for a specific distributed trace, including individual spans and their attributes](blogs/mnw_157/distributed_trace_details.png?imwidth=3840)

### Real-Time Metrics

Response times, error rates, throughput, all updating live. Perfect for load testing or understanding traffic patterns.

![Aspire Dashboard showing real-time metrics such as request duration, error rates, and throughput for various services](blogs/mnw_157/metrics.png?imwidth=3840)

## Summary

The standalone **Aspire Dashboard** is perfect for local development and debugging. Spin up your stack, make requests, and instantly see what's happening across all your services. Find bottlenecks in the trace view, correlate logs with requests, watch metrics update in real-time.

Remember: this is for development only since data is in-memory and disappears on restart. That last part might be fixed soon, according to the [**Aspire roadmap**](https://youtu.be/zvBu0OOCVos). For production, you'll want proper solutions like Jaeger for tracing, Prometheus for metrics, or a commercial APM like Application Insights.

But for that immediate "what is my code actually doing?" question during development? You've got professional observability in under 5 minutes.

Just add the container, configure OpenTelemetry, and start debugging like a pro.

That's all for today.

See you next Saturday.