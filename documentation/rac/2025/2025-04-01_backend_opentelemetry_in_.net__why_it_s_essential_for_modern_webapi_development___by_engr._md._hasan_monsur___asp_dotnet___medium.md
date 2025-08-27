```yaml
---
title: "OpenTelemetry in .NET: Why It’s Essential for Modern WebAPI Development | by Engr. Md. Hasan Monsur | ASP DOTNET | Medium"
source: https://medium.com/asp-dotnet/opentelemetry-in-net-why-its-essential-for-modern-webapi-development-3e380e1d4f60
date_published: 2025-04-01T13:32:06.710Z
date_captured: 2025-08-27T10:52:38.167Z
domain: medium.com
author: Engr. Md. Hasan Monsur
category: backend
technologies: [OpenTelemetry, .NET 8, ASP.NET Core, WebAPI, ILogger, Application Insights, Prometheus, Jaeger, Grafana, Azure Monitor, Zipkin, New Relic, Honeycomb, ML.NET, Docker, podman, NuGet, Swashbuckle.AspNetCore.Swagger, HttpClientFactory, W3C Trace Context]
programming_languages: [C#, SQL]
tags: [observability, opentelemetry, dotnet, web-api, distributed-tracing, metrics, logging, microservices, cloud-native, monitoring]
key_concepts: [distributed-tracing, vendor-neutral-instrumentation, metrics, log-correlation, observability, microservices, cloud-native, AI-powered-monitoring]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article emphasizes OpenTelemetry's crucial role in modern .NET WebAPI development, particularly for distributed and cloud-native applications. It details how OpenTelemetry provides vendor-neutral instrumentation, seamless distributed tracing, and automatic/custom metrics, offering a superior alternative to traditional logging tools. The author provides a comprehensive, step-by-step guide to integrate OpenTelemetry into a .NET 8 WebAPI project, including configuration for tracing, metrics, and logging, and visualization using Jaeger. Additionally, it explores advanced configurations like log correlation and span filtering, concluding with the potential for AI-powered monitoring when combined with ML.NET for predictive insights.]
---
```

# OpenTelemetry in .NET: Why It’s Essential for Modern WebAPI Development | by Engr. Md. Hasan Monsur | ASP DOTNET | Medium

# OpenTelemetry in .NET: Why It’s Essential for Modern WebAPI Development

## **OpenTelemetry is revolutionizing how .NET developers monitor, trace, and debug distributed WebAPI applications. In this deep dive, we’ll explore why OpenTelemetry has become a critical tool for modern .NET ecosystems — offering vendor-neutral instrumentation, seamless distributed tracing, and unparalleled observability. Learn how adopting OpenTelemetry can help you reduce debugging time, improve application performance, and future-proof your .NET WebAPIs against evolving cloud-native architectures. We’ll also discuss its advantages over traditional logging tools, integration with Azure Monitor and Prometheus, and why Microsoft’s increasing investment in OpenTelemetry signals its long-term dominance in .NET observability.**

![OpenTelemetry in .NET: Why It’s Essential for Modern WebAPI Development - Article Header](https://miro.medium.com/v2/resize:fit:700/1*T0-5iWzJZEUtUYiAceCxng.png)
*Image: Article header with the title "OpenTelemetry in .NET: Why It’s Essential for Modern WebAPI Development" and an OpenTelemetry logo featuring a telescope.*

## The Observability Challenge in Modern .NET WebAPIs

As .NET WebAPI applications grow in complexity — shifting to microservices, serverless architectures, and cloud-native deployments — traditional logging and monitoring tools like `ILogger` and Application Insights struggle to provide end-to-end visibility. Debugging latency spikes, tracing distributed transactions, and correlating logs across services become painful without a unified observability framework.

**Enter OpenTelemetry (OTel):** The open-source standard for instrumentation, metrics, and distributed tracing that’s transforming how .NET developers monitor applications.

## Why OpenTelemetry is a Game-Changer for .NET WebAPIs

### 1. Vendor-Neutral Instrumentation

*   **Problem:** Traditional APM tools (Application Insights, New Relic) lock you into proprietary SDKs.
*   **Solution:** OpenTelemetry provides a **standardized API** that works with any backend (Prometheus, Jaeger, Grafana, Azure Monitor).

**Example:**

```csharp
services.AddOpenTelemetry()    
    .WithTracing(builder => builder    
        .AddAspNetCoreInstrumentation()    
        .AddHttpClientInstrumentation()    
        .AddOtlpExporter()); // Export to any OTel-compatible tool  
```

### 2. End-to-End Distributed Tracing

*   **Problem:** In microservices, tracking a request across services is nearly impossible with logs alone.
*   **Solution:** OTel’s **W3C Trace Context** propagates trace IDs across HTTP calls, queues, and gRPC.
*   **Visualization:** Tools like Jaeger or Zipkin map entire request flows:

### 3. Automatic & Custom Metrics

*   **Key Metrics Tracked:**
    *   Request duration (P99 latency)
    *   Error rates
    *   Dependency calls (SQL, Redis, HTTP)

**Custom Metrics Example:**

```csharp
var meter = new Meter("Shop.Payments");    
var requestCounter = meter.CreateCounter<int>("payment_requests");    
requestCounter.Add(1, new("status", "success"));  
```

### 4. Future-Proofing with Open Standards

*   Microsoft now **recommends OpenTelemetry over Application Insights SDK** for .NET.
*   Cloud providers (Azure, AWS, GCP) all support OTel natively.
*   Emerging **AI-powered observability** tools (e.g., Honeycomb) rely on OTel data.

## Implementing OpenTelemetry in a .NET WebAPI Project

Let’s create a complete .NET 8 WebAPI project with OpenTelemetry integration for metrics, tracing, and logging. This implementation will include:

1.  Basic WebAPI setup
2.  OpenTelemetry instrumentation
3.  Exporting to Jaeger for visualization
4.  Custom metrics and spans
5.  Log correlation

### Step 1: Create a new .NET WebAPI Project

```bash
dotnet new webapi -n OtelWebApi  
cd OtelWebApi
```

### Step 2: Add Required NuGet Packages

```bash
dotnet add package OpenTelemetry.Extensions.Hosting  
dotnet add package OpenTelemetry.Instrumentation.AspNetCore  
dotnet add package OpenTelemetry.Instrumentation.Http  
dotnet add package OpenTelemetry.Exporter.Console  
dotnet add package OpenTelemetry.Exporter.OpenTelemetryProtocol  
dotnet add package OpenTelemetry.Extensions.DependencyInjection  
dotnet add package Microsoft.Extensions.Http  
dotnet add package OpenTelemetry.Instrumentation.Runtime  
dotnet add package Swashbuckle.AspNetCore.SwaggerGen  
dotnet add package Swashbuckle.AspNetCore.SwaggerUI
```

### Step 3: Configure OpenTelemetry in Program.cs

```csharp
using OpenTelemetry;  
using OpenTelemetry.Metrics;  
using OpenTelemetry.Resources;  
using OpenTelemetry.Trace;  
  
var builder = WebApplication.CreateBuilder(args);  
  
// Add services to the container  
builder.Services.AddControllers();  
builder.Services.AddEndpointsApiExplorer();  
builder.Services.AddSwaggerGen();  
builder.Services.AddHttpClient();  
  
// Configure OpenTelemetry  
builder.Services.AddOpenTelemetry()  
    .ConfigureResource(resource => resource  
        .AddService(serviceName: builder.Environment.ApplicationName))  
    .WithTracing(tracing => tracing  
        .AddAspNetCoreInstrumentation(options =>  
        {  
            options.RecordException = true;  
            options.EnrichWithHttpRequest = (activity, request) =>  
            {  
                activity.SetTag("requestProtocol", request.Protocol);  
            };  
        })  
        .AddHttpClientInstrumentation()  
        .AddConsoleExporter()  
        .AddOtlpExporter(o =>  
        {  
            o.Endpoint = new Uri("http://localhost:4317"); // Jaeger OTLP endpoint  
        }))  
    .WithMetrics(metrics => metrics  
        .AddAspNetCoreInstrumentation()  
        .AddHttpClientInstrumentation()  
        .AddRuntimeInstrumentation()  
        .AddProcessInstrumentation()  
        .AddConsoleExporter()  
        .AddOtlpExporter(o =>  
        {  
            o.Endpoint = new Uri("http://localhost:4317"); // Jaeger OTLP endpoint  
        }));  
  
var app = builder.Build();  
  
// Configure the HTTP request pipeline  
if (app.Environment.IsDevelopment())  
{  
    app.UseSwagger();  
    app.UseSwaggerUI();  
}  
  
app.UseHttpsRedirection();  
app.UseAuthorization();  
app.MapControllers();  
  
app.Run();
```

### Step 4: Create a Sample Controller with Instrumentation

```csharp
using System.Diagnostics;  
using System.Diagnostics.Metrics;  
using Microsoft.AspNetCore.Mvc;  
  
namespace OtelWebApi.Controllers;  
  
[ApiController]  
[Route("[controller]")]  
public class WeatherForecastController : ControllerBase  
{  
    private static readonly string[] Summaries = new[]  
    {  
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"  
    };  
  
    private readonly IHttpClientFactory _httpClientFactory;  
    private static readonly Meter _meter = new("WeatherForecast");  
    private static readonly Counter<int> _requestCounter = _meter.CreateCounter<int>("weather_forecast_requests");  
  
    public WeatherForecastController(IHttpClientFactory httpClientFactory)  
    {  
        _httpClientFactory = httpClientFactory;  
    }  
  
    [HttpGet(Name = "GetWeatherForecast")]  
    public async Task<IEnumerable<WeatherForecast>> Get()  
    {  
        // Track requests with custom metric  
        _requestCounter.Add(1, new KeyValuePair<string, object?>("path", Request.Path));  
          
        using var activity = Activity.Current?.Source.StartActivity("GenerateWeatherForecast");  
        activity?.SetTag("sample.tag", "custom-value");  
          
        try  
        {  
            // Simulate external API call  
            var client = _httpClientFactory.CreateClient();  
            var response = await client.GetAsync("https://jsonplaceholder.typicode.com/todos/1");  
            response.EnsureSuccessStatusCode();  
              
            var forecast = Enumerable.Range(1, 5).Select(index => new WeatherForecast  
            {  
                Date = DateTime.Now.AddDays(index),  
                TemperatureC = Random.Shared.Next(-20, 55),  
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]  
            })  
            .ToArray();  
  
            activity?.SetStatus(ActivityStatusCode.Ok);  
            return forecast;  
        }  
        catch (Exception ex)  
        {  
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);  
            throw;  
        }  
    }  
}  
  
public class WeatherForecast  
{  
    public DateTime Date { get; set; }  
    public int TemperatureC { get; set; }  
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);  
    public string? Summary { get; set; }  
}
```

### Step 5: Set Up Jaeger for Visualization

Run Jaeger with OTLP receiver using Docker:

```bash
podman pull jaegertracing/all-in-one:latest  
  
podman run -d --name jaeger \
  -e COLLECTOR_OTLP_ENABLED=true \
  -p 127.0.0.1:16686:16686 \
  -p 127.0.0.1:4317:4317 \
  -p 127.0.0.1:4318:4318 \
  -p 127.0.0.1:14268:14268 \
  -p 127.0.0.1:14250:14250 \
  jaegertracing/all-in-one:latest
```

Access Jaeger UI at [http://localhost:16686](http://localhost:16686/)

![Jaeger UI Search Page](https://miro.medium.com/v2/resize:fit:700/1*bd7QzthzHUweykBHirMcSA.png)
*Image: The Jaeger UI search page, showing a Gopher mascot and search filters for service, operation, tags, and time range.*

### Step 6: Run and Test the Application

Start the application:

```bash
dotnet run
```

![Terminal Output of dotnet run](https://miro.medium.com/v2/resize:fit:700/1*BhqwHoRU4iG8612v4fef3g.png)
*Image: A terminal window displaying the output of `dotnet run`, indicating the ASP.NET Core application has started and is listening on specified ports.*

Make requests to generate telemetry:

```bash
curl https://localhost:5001/weatherforecast
```

![Terminal Output of curl command](https://miro.medium.com/v2/resize:fit:700/1*zumUpvsgnmHW1swk-hxXLQ.png)
*Image: A terminal window showing the `curl` command and its JSON response, which is a list of weather forecasts.*

View traces in Jaeger ([http://localhost:16686](http://localhost:16686/)) and metrics in the console.

![Jaeger UI showing traces](https://miro.medium.com/v2/resize:fit:700/1*qEUqwFpiFcfBN6iCe94YXw.png)
*Image: The Jaeger UI displaying a list of traces for the "OtelWebApi" service, showing two traces with their durations and number of spans.*

## Advanced Configuration Options

### 1. Adding Log Correlation

Add these packages:

```bash
dotnet add package OpenTelemetry.Instrumentation.Logging  
dotnet add package OpenTelemetry.Exporter.Console
```

Update Program.cs:

```csharp
builder.Logging.AddOpenTelemetry(options =>  
{  
    options.IncludeFormattedMessage = true;  
    options.IncludeScopes = true;  
    options.ParseStateValues = true;  
    options.AddConsoleExporter();  
});
```

### 2. Filtering Spans

```csharp
.WithTracing(tracing => tracing  
    .AddAspNetCoreInstrumentation(options =>  
    {  
        options.Filter = httpContext =>   
            !httpContext.Request.Path.StartsWithSegments("/health");  
    }))
```

### 3. Custom Resource Attributes

```csharp
.ConfigureResource(resource => resource  
    .AddService(  
        serviceName: builder.Environment.ApplicationName,  
        serviceVersion: "1.0.0")  
    .AddAttributes(new Dictionary<string, object>  
    {  
        ["deployment.environment"] = builder.Environment.EnvironmentName,  
        ["host.name"] = Environment.MachineName  
    }))
```

### Download Full Project — [OpenTelemetry-in-.NET](https://github.com/hasanmonsur/OpenTelemetry-in-.NET)

## Conclusion:

OpenTelemetry is revolutionizing .NET observability, and when combined with ML.NET, it unlocks AI-powered monitoring for WebAPIs. By feeding OTel’s rich metrics into ML.NET models, developers can predict failures, auto-scale services, and detect anomalies in real-time. The pros are clear: vendor-neutral instrumentation, seamless distributed tracing, and machine-learning-enhanced insights. However, consider the cons: added complexity in initial setup and potential overhead in high-throughput systems. As .NET evolves, this duo (OTel + ML.NET) will become indispensable for building self-healing, intelligent APIs. Start small — instrument one service, analyze traces with basic ML, then scale. The future of .NET observability isn’t just reactive; it’s predictive.