```yaml
---
title: What’s New in ASP.NET Core 9
source: https://okyrylchuk.dev/blog/whats-new-in-asp-net-core-9/
date_published: 2024-11-15T17:28:52.000Z
date_captured: 2025-08-12T11:28:13.176Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [ASP.NET Core 9, .NET 9, .NET 8, Microsoft.AspNetCore.OpenApi, Swashbuckle, System.Text.Json, HybridCache, Blazor, SignalR]
programming_languages: [C#]
tags: [asp.net-core, .net, new-features, minimal-api, openapi, performance, caching, middleware, exception-handling, web-api]
key_concepts: [Minimal API enhancements, OpenAPI integration, Keyed Services in Middleware, HTTP Metrics control, Improved Developer Exception Page, Customizable Exception Handling, HybridCache, Performance optimizations]
code_examples: false
difficulty_level: intermediate
summary: |
  [ASP.NET Core 9 introduces significant enhancements, focusing on performance and developer experience. Key improvements include a substantial memory allocation reduction and faster exception handling in Minimal APIs. The new release natively supports OpenAPI document generation, removing Swashbuckle from templates, and enables the use of Keyed Services within Middleware. Additionally, developers gain more control over HTTP metrics, an enriched Developer Exception Page, and a new `StatusCodeSelector` for custom exception responses. The upcoming HybridCache promises simplified and more robust caching with built-in stampede protection, making it a valuable addition for high-performance applications.]
---
```

# What’s New in ASP.NET Core 9

# What’s New in ASP.NET Core 9

By Oleg Kyrylchuk / November 15, 2024

.NET 9 is out! It brings a lot of new features and performance improvements.

Let’s explore what ASP.NET Core 9 brings new to us. There are many improvements, but I picked some practical and interesting ones.

## **Minimal API**

![.NET 9 Minimal API Performance comparison showing 15% increase in Requests Per Second and 93% reduction in Memory usage compared to .NET 8.](https://okyrylchuk.dev/wp-content/uploads/2024/11/perf-1024x558.avif)

The Minimal API **gains a performance boost**, as you can see on the chart provided by Microsoft.

But the most expressive is the memory allocation! It was reduced to 93%! It’s imposing.

To achieve such results, there are a lot of improvements under the good:

*   **The exception handling is 50% faster**. The previous structured exception handling was removed. The new implementation is based on the Native AOT model.
*   **The runtime uses more vectorization to leverage SIMD** (Single Instruction, Multiple Data) operations.
*   **Dynamic Profile Guided Optimization** (PGO) has been updated to optimize more code patterns.
*   **System.Text.Json** internals were significantly optimized. Learn more about what’s new in [**System.Text.Json in .NET 9**](/blog/whats-new-in-system-text-json-in-dotnet-9/).
*   **LINQ** common operations also got improvements. For example, the Take method is up to 10 times faster when the underlying collection is empty.

And many more improvements.

## **OpenAPI**

ASP.NET Core 9 **removes Swashbuckle** from the template.

The .NET team added support for **OpenAPI** document generation.

To use it, you have to install **Microsoft.AspNetCore.OpenApi** package.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.Run();
```

The **AddOpenApi** method registers the required dependencies.

The **MapOpenApi** method registers OpenAPI endpoints.

You can find the generated **OpenAPI** document by the link **/openapi/v1.json**.

If you like **Swagger**, you can add it as a separate package and configure it.

```csharp
app.UseSwaggerUi(c => c.DocumentPath = "/openapi/v1.json");
```

To learn about more **OpenAPI** features, visit the [Generate OpenAPI documents](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/openapi/aspnetcore-openapi?view=aspnetcore-9.0&tabs=visual-studio).

## **Keyed Services in Middleware**

The **Keyed Services** were introduced in .NET 8.

However, you couldn’t use them in the **Middleware**.

ASP.NET Core 9 fixes the issue.

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKeyedSingleton<MySingleton>("singleton");
builder.Services.AddKeyedScoped<MyScoped>("");

var app = builder.Build();
app.UseMiddleware<MyMiddleware>();
app.Run();

internal class MyMiddleware
{
    private readonly RequestDelegate _next;

    public MyMiddleware(RequestDelegate next,
        [FromKeyedServices("singleton")] MySingleton service)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context,
        [FromKeyedServices("scoped")]
                MyScoped scopedService) => _next(context);
}

public class MySingleton
{ }

public class MyScoped
{ }
```

## **Disabling Metrics**

ASP NET Core 9 allows the **disabling of HTTP metrics** and the non-recording of values for specific endpoints and requests.

For instance, automated systems check the **Health** endpoint, so recording metrics for it is not particularly helpful.

```csharp
var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapHealthChecks("/health").DisableHttpMetrics();

app.Run();
```

## **Developer Exception Page**

The ASP.NET Core displays the **developer exception page** when unhandled exceptions occur while developing.

The **developer exception page** now shows the endpoint metadata.

![Developer Exception Page in ASP.NET Core 9 showing endpoint metadata, including RuntimeMethodInfo, HttpMethodMetadata, and RouteDiagnosticsMetadata.](https://okyrylchuk.dev/wp-content/uploads/2024/11/metadata-png.avif)

## **StatusCodeSelector**

ASP NET Core 8 introduced the [**IExceptionHandler**](/blog/handling-exceptions-in-asp-net-core-8/).

The issue was that the default exception handler always set the response status code to 500 (Internal Server Error).

ASP NET Core 9 introduces the new option **StatusCodeSelector**, which allows you to choose the status code based on the exception.

```csharp
app.UseExceptionHandler(new ExceptionHandlerOptions
{
  StatusCodeSelector = ex => is TimeoutException
    ? StatusCodes.Status503ServiceUnavailable
    : StatusCodes.Status500InternalServerError
});
```

## **TypedResults.InternalServerError**

The **TypedResults** class offers a convenient way to return strongly typed, status code-based responses from minimal APIs. Now, it supports a method for creating an **Internal Server Error** (500) response.

```csharp
app.MapGet("/", () => TypedResults.InternalServerError("Something went wrong!"));
```

## **HybridCache**

**HybridCache** is still in preview but will be fully released _after_ .NET 9.0 in a future minor release of .NET Extensions.

The new **HybridCache** is a better version of the **IDistributedCache** and **IMemoryCache** interfaces, making adding cache code more straightforward.

If you used **IDistributedCache**, you know that before setting a value in the cache, you have to deserialize the object to bytes of JSON — the same for getting value from the cache. Also, when many threads concurrently hit the cache, you can get problems with cache misses.

The new **HybridCache** does all the required work for you. It also implements **“Stampede protection,”** which refers to techniques used in software systems to prevent multiple processes, threads, or servers from overwhelming a resource or performing redundant work when accessing shared data or resources simultaneously.

To add **HybridCache** to your application, add the line.

```csharp
builder.Services.AddHybridCache();
```

Then, usage of the cache is simple.

```csharp
public class WeatherForecastService(HybridCache cache)
{
    public async Task<WeatherForecast> GetWeatherForecast
        (string name, int id, CancellationToken token = default)
    {
        return await cache.GetOrCreateAsync(
            $"weatherforecast:{name}:{id}", // Unique key.
            async cancel => await GetWeatherForecastFromApi(name, id, cancel),
            cancellationToken: token
        );
    }

    private async Task<WeatherForecast> GetWeatherForecastFromApi(
        string name, int id, CancellationToken cancel)
    {
        // Simulate a call to an external API.
        return new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now),
            25,
            null
        );
    }
}
```

## Summary

ASP.NET Core 9 brings many improvements and features. One impressive one is reducing memory allocation in the Minimal API. ASP.NET Core 9 supports OpenAPI generation. Also, it has many handy small improvements that make developing APIs better.

And shortly, we’ll get a new, better HybridCache to work with a cache.

It’s not all features and improvements. Also, Blazor and SignalR get some upgrades. Read more [here](https://learn.microsoft.com/en-us/aspnet/core/release-notes/aspnetcore-9.0?view=aspnetcore-9.0).