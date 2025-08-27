```yaml
---
title: "Monitor Application Performance in .NET | by Engr. Md. Hasan Monsur | ASP DOTNET | Medium"
source: https://medium.com/asp-dotnet/monitor-application-performance-in-net-45fb69d9bc53
date_published: 2024-09-07T17:21:10.059Z
date_captured: 2025-08-06T17:48:34.784Z
domain: medium.com
author: Engr. Md. Hasan Monsur
category: performance
technologies: [.NET, ASP.NET Core, Azure Application Insights, Azure Monitor, NuGet, Windows Performance Monitor (PerfMon), PerformanceCounterLib, PerfView, dotnet-trace, Visual Studio Profiler, dotnet-counters, dotMemory, dotTrace (JetBrains), Serilog, NLog, Log4Net, Prometheus, Grafana, prometheus-net, KestrelMetricServer, SQL Server Profiler, Entity Framework Core, SQL Azure Metrics, Jaeger, OpenTelemetry, Zipkin, StatsD, InfluxDB, StatsdClient]
programming_languages: [C#, SQL]
tags: [application-performance-monitoring, .net, asp.net-core, azure, profiling, logging, health-checks, distributed-tracing, metrics, database-monitoring]
key_concepts: [application-performance-monitoring, telemetry, performance-counters, garbage-collection, profiling, http-request-monitoring, health-checks, distributed-tracing, custom-metrics]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to monitoring application performance in .NET, outlining twelve key strategies. It covers the use of cloud-based solutions like Azure Application Insights, leveraging built-in .NET tools such as Performance Counters and profiling utilities, and integrating third-party libraries for logging and metrics. The guide also delves into monitoring HTTP requests, implementing health checks, tracking SQL database performance, and utilizing distributed tracing for complex architectures. By combining these techniques, developers can effectively identify and resolve performance bottlenecks, ensuring efficient and robust .NET applications.
---
```

# Monitor Application Performance in .NET | by Engr. Md. Hasan Monsur | ASP DOTNET | Medium

# Monitor Application Performance in .NET

![A banner image for the article "Monitor Application Performance in .NET". On the left, a graphic depicts a performance meter with a needle pointing to high performance, and a laptop showing an upward trend graph, symbolizing application monitoring. On the right, a circular profile picture of Engr. Md. Hasan Monsur, the author, is shown speaking into a microphone, with his name and location "Hasan Monsur, Dhaka Bangladesh" below.](https://miro.medium.com/v2/resize:fit:700/1*-IuzsKnpISMnzB7rLBMJ8g.png)

Monitoring application performance in .NET is essential to ensure that your system is running efficiently and to quickly identify and resolve performance issues. Various tools and techniques are available to track performance, including built-in .NET capabilities, third-party services, and custom logging approaches.

Below are some of the key strategies to monitor application performance in .NET:

## 1. Use Application Insights (Azure Monitor)

**Azure Application Insights** is a powerful, cloud-based monitoring tool that provides detailed telemetry data for .NET applications. It tracks metrics like request performance, dependency tracking, exceptions, and custom events.

## Steps to Set Up Application Insights:

Install the **Microsoft.ApplicationInsights.AspNetCore** NuGet package:

```bash
dotnet add package Microsoft.ApplicationInsights.AspNetCore
```

Add Application Insights to your application in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);
  
builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:InstrumentationKey"]);
  
var app = builder.Build();
```

**You can now monitor:**

*   Request Rates
*   Failed Requests
*   Dependency Calls (e.g., SQL queries, HTTP requests)
*   Response Times
*   Custom Events

Application Insights provides real-time performance monitoring, including latency and throughput, and you can view telemetry in the Azure portal.

## 2. Use .NET Performance Counters

Windows **Performance Counters** can provide insights into various .NET-specific metrics such as CPU usage, memory allocation, garbage collection, and thread pool utilization.

**Example Performance Counters to Monitor:**

*   **Processor Time** (`% Processor Time`)
*   **Memory** (`Private Bytes`, `Working Set`)
*   **ASP.NET Requests** (`Requests/sec`)
*   **Thread Pool Usage** (`ThreadPool Completed Work Items`, `Thread Count`)

To capture these, you can use tools like **Windows Performance Monitor** (PerfMon), or write custom code:

```csharp
PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
float currentCpuUsage = cpuCounter.NextValue();
```

You can also use third-party libraries like **PerformanceCounterLib** to automate collection and reporting of these metrics.

## 3. Monitor Garbage Collection (GC) Performance

Garbage collection (GC) performance can be a critical factor in your application’s memory management and overall efficiency. Monitoring GC activity helps detect potential memory leaks and inefficient allocations.

*   **GC.GetTotalMemory()**: This method can give you an idea of how much memory your application is currently using.
*   Use the **.NET Runtime GC Counters**: These are available through **Performance Counters** (in Windows) or **Event Tracing for Windows (ETW)**.
*   **Gen 0, Gen 1, Gen 2 Collections**: Track the frequency of garbage collections across different generations.

You can use tools like **PerfView** or **dotnet-trace** to monitor GC performance:

```bash
dotnet-trace collect --process-id <pid>
```

## 4. Use Profiling Tools

.NET provides several profiling tools to measure the performance of an application, including CPU usage, memory allocation, and other critical metrics.

## Tools to Consider:

*   **Visual Studio Profiler**: Built into Visual Studio, it allows you to monitor CPU usage, memory allocation, and I/O performance.
*   **dotnet-counters**: A cross-platform performance monitoring tool for .NET applications that provides metrics such as GC collections, thread pool activity, and exceptions.

```bash
dotnet-counters monitor System.Runtime --process-id <pid>
```

**dotMemory** and **dotTrace** (JetBrains): These are powerful third-party tools for memory and performance profiling.

## 5. Monitor HTTP Requests and Response Times

Monitoring HTTP request and response times is vital for web applications. Slow responses could indicate performance bottlenecks in the server, database, or network.

## ASP.NET Core Middleware for Monitoring:

You can implement custom middleware to log request times:

```csharp
public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
      
    public RequestTimingMiddleware(RequestDelegate next)
    {
        _next = next;
    }
  
    public async Task InvokeAsync(HttpContext context)
    {
        var watch = Stopwatch.StartNew();
        await _next(context);
        watch.Stop();
          
        Console.WriteLine($"Request {context.Request.Method} {context.Request.Path} took {watch.ElapsedMilliseconds}ms");
    }
}
```

You can also integrate **Serilog** or **NLog** with ASP.NET Core to log request performance.

## 6. Use Health Checks

In .NET Core, you can use **Health Checks** to actively monitor the health and availability of your services. This is useful for ensuring services are responsive, database connections are open, and external dependencies are functioning.

Install the health check NuGet package:

```bash
dotnet add package Microsoft.Extensions.Diagnostics.HealthChecks
```

Add health checks in `Program.cs`:

```csharp
builder.Services.AddHealthChecks()
    .AddCheck("Database", new SqlConnectionHealthCheck("YourConnectionString"));
```

Add the health check middleware:

```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapHealthChecks("/health");
});
```

Now, you can expose a `/health` endpoint that returns the health status of your application.

## 7. Log Performance Metrics with Serilog, NLog, or Log4Net

Logging libraries such as **Serilog**, **NLog**, and **Log4Net** allow you to log detailed performance data for better diagnosis.

*   **Serilog** is especially good for structured logging. You can capture performance metrics in your logs:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("logs/logfile.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();
  
Log.Information("Application started");
```

*   You can add performance-specific logging to track request times, database query execution times, and external API response times.

## 8. Monitor with Prometheus and Grafana

Prometheus is an open-source monitoring tool that collects and stores time-series data. It can be used with Grafana to visualize metrics like CPU usage, memory consumption, and request throughput.

*   **Prometheus**: Instrument your .NET application to expose metrics via an HTTP endpoint.
*   **Grafana**: Visualize these metrics on dashboards.

Use the **prometheus-net** NuGet package to integrate Prometheus with your .NET application.

```bash
dotnet add package prometheus-net
```

Then, expose metrics from your application:

```csharp
var metricServer = new KestrelMetricServer(port: 1234);
metricServer.Start();
```

## 9. Track SQL Database Performance

For applications that rely heavily on databases, it’s essential to monitor query execution times and database connections. Tools like **SQL Server Profiler**, **Entity Framework Core Logging**, and **SQL Azure Metrics** can help you track database performance.

## EF Core Logging:

You can log query execution times in **Entity Framework Core** by enabling logging:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
{
    optionsBuilder
        .UseSqlServer("YourConnectionString")
        .LogTo(Console.WriteLine, LogLevel.Information);
}
```

## 10. Use Distributed Tracing

**Distributed tracing** helps monitor and track the flow of requests across different services in microservices architectures. Tools like **Jaeger**, **OpenTelemetry**, and **Zipkin** provide insights into request paths, latency, and bottlenecks across services.

## Example with OpenTelemetry:

```bash
dotnet add package OpenTelemetry.Exporter.Console
dotnet add package OpenTelemetry.Extensions.Hosting
dotnet add package OpenTelemetry.Instrumentation.AspNetCore
```

Configure OpenTelemetry in your application to capture trace data.

## 11. Measure CPU and Memory Usage

You can track CPU and memory consumption of your .NET applications using built-in .NET diagnostics tools like:

*   **dotnet-trace**: Provides CPU profiling and tracing.
*   **dotnet-dump**: Captures and analyzes process memory dumps.

```bash
dotnet-trace collect --process-id <pid>
```

## 12. Track Custom Metrics

You can create and track your own custom performance metrics using libraries like **StatsD**, **InfluxDB**, or custom counters.

## Example with StatsD:

```bash
dotnet add package StatsdClient
```

You can now send custom metrics like:

```csharp
var metrics = new Statsd();
metrics.Counter("custom_metric", 1);
```

## Conclusion

Monitoring application performance in .NET involves a combination of using built-in tools, external services, and logging mechanisms. Techniques like **Application Insights**, **Performance Counters**, **Health Checks**, and **custom logging** allow you to proactively monitor and improve application performance. Selecting the right tools and strategies depends on the specific needs of your application, whether it be response times, CPU/memory usage, or overall system health.