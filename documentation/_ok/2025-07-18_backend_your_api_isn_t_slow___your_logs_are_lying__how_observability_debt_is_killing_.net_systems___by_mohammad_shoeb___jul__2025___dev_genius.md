```yaml
---
title: "Your API Isn’t Slow — Your Logs Are Lying: How Observability Debt Is Killing .NET Systems | by Mohammad Shoeb | Jul, 2025 | Dev Genius"
source: https://blog.devgenius.io/your-api-isnt-slow-your-logs-are-lying-how-observability-debt-is-killing-net-systems-8b4522f5ea72
date_published: 2025-07-18T13:23:55.571Z
date_captured: 2025-08-12T21:03:44.335Z
domain: blog.devgenius.io
author: Mohammad Shoeb
category: backend
technologies: [ASP.NET Core, .NET, Azure Application Insights, Azure Functions, Azure Service Fabric, Cosmos DB, Blob Storage, Azure SQL, Service Bus, Event Hub, Azure Data Explorer, Kusto, OpenTelemetry, Redis, Docker, Grafana, Event Grid]
programming_languages: [C#, SQL, KQL]
tags: [observability, logging, monitoring, .net, azure, microservices, distributed-systems, performance, troubleshooting, telemetry]
key_concepts: [Observability Debt, Structured Logging, Correlation ID, Distributed Tracing, OpenTelemetry, Metrics, Log Sampling, Root Cause Analysis, Technical Debt]
code_examples: false
difficulty_level: intermediate
summary: |
  The article defines "Observability Debt" as incomplete or misleading telemetry that hinders debugging and problem-solving in production systems. It highlights how poor logging can mask real issues, citing an incident where API slowdowns were not reflected in "successful" logs. The author proposes several patterns to combat this debt, including structured logging with scopes, propagating correlation IDs, using OpenTelemetry with Azure Application Insights, and smart log sampling. It concludes with a blueprint for designing observability in large-scale IoT systems using Azure services and Kusto, emphasizing the importance of treating observability as a first-class system requirement.
---
```

# Your API Isn’t Slow — Your Logs Are Lying: How Observability Debt Is Killing .NET Systems | by Mohammad Shoeb | Jul, 2025 | Dev Genius

# Your API Isn’t Slow — Your Logs Are Lying: How Observability Debt Is Killing .NET Systems

> _You scaled your database. Tuned your thread pool.  
> Still slow.  
> Still failing.  
> Still no clue why._

Here’s the truth nobody talks about in daily standups:

> _🔥 Most .NET production issues aren’t caused by bad code.  
> They’re caused by_ **_bad observability_**_._

In this blog, we’ll explore what **Observability Debt** is, how to spot it, and patterns to fix it. I will also share how we built **structured logging** into our Azure-native, multi-region IoT system. With the right trace IDs and Kusto queries, we now pinpoint issues across 30+ microservices in minutes.

# 💡 What Is Observability Debt?

We all know technical debt — hacks, shortcuts, TODOs we never finish.

**Observability debt** is its evil twin:

*   Incomplete logs
*   Inconsistent telemetry
*   Misleading metrics
*   Silent failure paths
*   No way to _see_ what the system is actually doing

> _And when your logs lie, you debug the wrong thing — or worse, deploy the wrong fix._

# 🧨 A Real Incident: API Slowdowns That Weren’t Real

In one of my past .NET projects:

*   Azure Application Insights showed **“healthy” 200 OKs**
*   API looked responsive… on the outside

Yet customers kept reporting:

*   Slowness
*   Missing notifications
*   Timeouts on mobile

Turns out:

*   We **logged success before the database commit completed**
*   **Retries weren’t logged at all**
*   Frontend users saw failures, but telemetry showed “green”

That’s observability debt in action.  
Logs told a half-truth — and we paid the price.

# 🩹 Symptoms of Observability Debt

![Table showing symptoms of observability debt and their true meaning. Symptoms include "All logs say 'Success'", "You can't trace a request across services", "Logs show no performance issues", "You get huge App Insights bills", and "Your 'LogError' has zero context".](https://miro.medium.com/v2/resize:fit:700/1*Un0mVQ2dS7gS4BsgTnPwVA.png)

# ⚠️ Warning: These Logs Are Lying

```csharp
_logger.LogInformation("Order placed successfully");
```

This log tells you… nothing.

*   Which order?
*   By which user?
*   How long did it take?
*   What was the outcome of the payment event that followed?

If you’re building APIs, microservices, or Azure Functions and using logs like that, you’re **flying blind** in production.

# ✅ Pattern: Structured Logging with Scopes

```csharp
using (_logger.BeginScope(new Dictionary<string, object>  
{  
    ["CorrelationId"] = context.TraceIdentifier,  
    ["UserId"] = user.Id  
}))  
{  
    _logger.LogInformation("Placing order {OrderId}", order.Id);  
}
```

# 🔥 Why It Works:

*   Adds automatic context to every log in the request.
*   Easily queryable in App Insights or Log Analytics.
*   Enables **distributed correlation** across microservices.

Reference: [Structured Logging Microsoft](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line)

# ✅ Pattern: Propagate CorrelationId Everywhere

```csharp
app.Use(async (context, next) =>  
{  
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault() ?? Guid.NewGuid().ToString();  
    context.Items["CorrelationId"] = correlationId;  
    using (_logger.BeginScope("{CorrelationId}", correlationId))  
    {  
        await next();  
    }  
});
```

> _Then pass that_ `_CorrelationId_` _to downstream services and log every operation against it._

Reference: [Correlation ID Microsoft](https://microsoft.github.io/code-with-engineering-playbook/observability/correlation-id/)

# ✅ Pattern: Use OpenTelemetry with App Insights

```csharp
services.AddOpenTelemetry()  
    .WithTracing(builder => builder  
        .AddAspNetCoreInstrumentation()  
        .AddHttpClientInstrumentation()  
        .AddAzureMonitorTraceExporter())  
    .WithMetrics(builder => builder  
        .AddRuntimeInstrumentation()  
        .AddAzureMonitorMetricExporter());
```

# 🔥 Why It Works:

*   Enables distributed traces across services, queues, databases.
*   View actual call graphs and pinpoint latency hotspots.

Reference: [Use OpenTelemetry with App Insights Microsoft](https://learn.microsoft.com/en-us/azure/azure-monitor/app/opentelemetry-enable?tabs=aspnetcore)

# ✅ Pattern: Sample Your Logs Smartly

```csharp
builder.Services.ConfigureTelemetryModule<SamplingTelemetryProcessor>(  
    (module, o) =>  
    {  
        module.SamplingPercentage = 10; // Sample 10% to reduce cost  
    });
```

> _App Insights gets expensive fast. Smart sampling keeps costs down_ without losing critical signal_._

Reference: [Sample Your Logs Smartly Microsoft](https://devblogs.microsoft.com/dotnet/finetune-the-volume-of-logs-your-app-produces/)

# ✅ Pattern: Push Metrics with Context

```csharp
_metrics.LogMetric("OrdersProcessed", 1, new Dictionary<string, string>  
{  
    { "Region", "WestEurope" },  
    { "Channel", "Mobile" }  
});
```

> _Use dimensions wisely. Avoid high-cardinality fields like full URLs or emails._

# 🏭 My Experience — Prod-Level Blueprint: Observability at IoT Scale

> Real numbers, real pain-points, real wins.

Our flagship IoT platform ingests telemetry from **8 million devices** spread across **four geographic regions**. It runs on an all-Azure, microservice-heavy stack:

![Table outlining the layers and services in play for an IoT platform's observability blueprint. Layers include Compute (Azure Functions, Azure Service Fabric actors), Data (Cosmos DB, Blob Storage, Azure SQL), Messaging (Service Bus, Event Hub), and Logging (Centralised ADX/Kusto + Blob archive).](https://miro.medium.com/v2/resize:fit:692/1*dWrL9nvn7x2pZHE7FMoQUA.png)

# 🔑 How We Designed Observability From Day 0

1.  **Global Identifiers**

*   **Global Message Id** — a GUID stamped at the first ingress point (Device → API Gateway).
*   **Correlation Id** — scoped to the request chain within a single user action.
*   **Activity Id** — generated per microservice hop (ASP.NET Core Activity/DiagnosticSource).

**2\. TagId Per Code Section**  
Every microservice method that could plausibly fail includes:

```csharp
const string TagId = "7B8EAA9D-95F7-4C5B-9F1A-7CE2E4E1AB5B"; // human-friendly GUID  
  
logger.LogError(ex,  
    "TAG:{TagId} DEVICE:{DeviceId} MSG:{MessageId} Failure writing to Cosmos",  
    TagId, deviceId, messageId);
```

When an alert fires, we grep by **TagId** first — it pin-points the exact code path in seconds.

**3\. Blob → Kusto Log Pipeline**

*   Each function or service drops structured JSON logs to **Blob Storage**.
*   An Event Grid trigger streams blobs into an **Azure Data Explorer** (Kusto) table.
*   **Retention policy** = 45 days (hot) ⇒ blobs roll to cool/archive after that.

**4\. Query Playbooks**

*   **Root-cause**: `Logs | where GlobalMessageId == '<guid>' | order by Timestamp`
*   **Drill-down by Tag**: `Logs | where TagId == '<guid>' | summarize count() by ActivityId`
*   **Cross-service latency**: join `Requests`, `Dependencies` on `ActivityId`.

**5\. Why It Works**

*   🔍 _Single pane of glass_ — Kusto holds every byte of telemetry across 30 + services.
*   ⚡ _Blazing queries_ — sub-second search on billions of rows.
*   💰 _Cost control_ — Blob is cheap storage, Kusto is hot only for 45 days.
*   ⏱ Post-incident MTTR dropped from _hours_ to **minutes**.

> **_Lesson learned:_** _At scale, the_ metadata _on your logs is as important as the message itself._

# 🧠 Quick Wins to Reduce Observability Debt

![Table listing quick wins to reduce observability debt. Problems include "No traceability", "Missing failures", "Blind to latency", "App Insights overload", and "Poor root cause analysis", each with a corresponding fix.](https://miro.medium.com/v2/resize:fit:678/1*Ii57sSGSKFCF_8HnV7W02g.png)

# 📊 Real Dashboard You Should Build

1.  **Top 5 Failed Endpoints** (last 1h)
2.  **Dependency Duration > 1s** (Azure SQL, Redis, Blob, etc.)
3.  **Requests Without CorrelationId**
4.  **Retry Loops > 3x**
5.  **Cache Miss Rate**

Build them in Azure Monitor, Log Analytics, or Grafana with KQL and OpenTelemetry.

# 💬 Final Thought

> _Your logs aren’t for you today.  
> They’re for the_ you _who’s debugging a 2AM prod incident next week._

Observability is a **first-class system requirement** — just like security, scalability, or uptime.

And if you don’t design for it, you’ll keep fixing symptoms instead of causes.

# 🔗 References

*   🔹 [**Logging in .NET**](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging) — Official guide to logging abstractions, `ILogger<T>`, and structured logging fundamentals.
*   🔹 [**High-Performance Logging with LoggerMessage**](https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging) — Microsoft’s pattern for fast, allocation-free, semantic logging using `LoggerMessage.Define`.
*   🔹 [**Logging in ASP.NET Core**](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/) — Covers structured logging with scopes, filtering, and telemetry in ASP.NET Core apps.
*   🔹 [**Structured Logging in .NET Aspire**](https://learn.microsoft.com/en-us/dotnet/aspire/fundamentals/dashboard/overview?tabs=bash) — Modern structured logging support with dashboards in .NET Aspire microservice environments.
*   🔹 [**Semantic Logging Application Block (SLAB)**](https://learn.microsoft.com/en-us/previous-versions/msp-n-p/dn440729\(v=pandp.60\)) — Archived Microsoft guidance on early semantic logging architecture and patterns.