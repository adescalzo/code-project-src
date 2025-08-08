```yaml
---
title: Logging Requests and Responses for API Requests and Httpclient in ASP.NET Core
source: https://antondevtips.com/blog/logging-requests-and-responses-for-api-requests-and-httpclient-in-aspnetcore
date_published: 2025-07-15T07:45:08.593Z
date_captured: 2025-08-06T11:48:07.602Z
domain: antondevtips.com
author: Anton Martyniuk
category: backend
technologies: [ASP.NET Core, HttpLogging, HttpClient, Microsoft.Extensions.Logging, Serilog, Refit, Polly, .NET, JSON]
programming_languages: [C#]
tags: [logging, aspnet-core, http, web-api, httpclient, middleware, security, observability, troubleshooting, serilog]
key_concepts: [http-logging, middleware, dependency-injection, logging-levels, sensitive-data-redaction, IHttpLoggingInterceptor, DelegatingHandler, endpoint-specific-logging]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to logging HTTP requests and responses in ASP.NET Core applications. It details how to enable and configure the built-in HttpLogging middleware, including setting logging levels and customizing options like body limits. The guide also covers advanced techniques such as implementing custom IHttpLoggingInterceptor for dynamic log customization and per-endpoint configuration. Furthermore, it emphasizes the critical importance of redacting sensitive data for security and compliance, offering practical examples for both server-side API requests and outgoing HttpClient calls using DelegatingHandler. The content is crucial for developers aiming to enhance application observability, troubleshoot issues, and ensure data privacy.]
---
```

# Logging Requests and Responses for API Requests and Httpclient in ASP.NET Core

![A dark blue and purple banner image with the title "LOGGING REQUESTS AND RESPONSES FOR API REQUESTS AND HTTP CLIENT IN ASP.NET CORE" in white text. On the left, there is a white square icon with a code tag `</>` and the text "dev tips" below it.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fdotnet_asp_logging_requests_responses.png&w=3840&q=100)

# Logging Requests and Responses for API Requests and HttpClient in ASP.NET Core

Jul 15, 2025

[Download source code](/source-code/logging-requests-and-responses-for-api-requests-and-httpclient-in-aspnetcore)

8 min read

### Newsletter Sponsors

Building authentication in-house? The **[2025 State of Homegrown Authentication](https://fusionauth.link/3GI6WQJ)** surveyed 144 developers to reveal hidden costs: 67% build from scratch vs buying solutions, yet 20% faced security breaches anyway. Get the real data on what actually works and what doesn't. [Download the free report](https://fusionauth.link/3GI6WQJ)

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Logging HTTP requests and responses helps developers quickly troubleshoot issues, monitor performance and health, and audit user interactions within their applications. ASP.NET Core provides built-in support with HttpLogging, which you can easily configure and extend according to your specific needs.

In today's newsletter, we'll cover in-depth:

*   How to enable and configure HTTP logging in your ASP.NET Core projects
*   Logging options and settings
*   Customizing logs and using endpoint-specific configurations
*   How to redact sensitive data in logs
*   How to log requests and responses when sending requests with HttpClient

Let's dive in.

[](#getting-started-with-httplogging-in-aspnet-core)

## Getting Started with HttpLogging in ASP.NET Core

ASP.NET Core provides a built-in `HttpLogging` middleware that logs all incoming Web API requests and responses.

To enable HTTP logging, you only need two simple steps:

**Step 1: Add `HttpLogging` middleware in Program.cs:**

```csharp
var builder = WebApplication.CreateBuilder(args);

// Enable HTTP logging services
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = HttpLoggingFields.Request | HttpLoggingFields.Response;
});

var app = builder.Build();

// Add middleware to the HTTP request pipeline
app.UseHttpLogging();

app.Run();
```

**Step 2: Adjust Logging Levels in appsettings.json**

To ensure HTTP logging output appears in your logs, you should adjust the logging level. Typically, setting this to "Information" is enough:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.Hosting.Diagnostics": "Warning",
      "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware": "Information"
    }
  }
}
```

Once configured, HTTP requests and responses will automatically log to your configured log outputs (console, file, etc.). You should see log entries similar to this example when making a request:

```csharp
info: Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware[1]
      Request:
      Protocol: HTTP/1.1
      Method: GET
      Scheme: https
      PathBase: 
      Path: /
      Headers:
        Host: localhost:5001
        User-Agent: Mozilla/5.0...
      Response:
      StatusCode: 200
      Headers:
        Content-Type: text/plain; charset=utf-8
```

It works with a built-in ASP.NET Core logging provider - `Microsoft.Extensions.Logging`. It also works with a custom logging provider, such as [Serilog](https://antondevtips.com/blog/logging-best-practices-in-asp-net-core#best-practise-1-use-serilog-library-for-logging).

Serilog has a large ecosystem of sinks, and its flexible configuration makes it an excellent choice for logging. A sink is a source where you can output and store your logs; it can be a console, file, database, or monitoring system.

This is how you can configure Serilog to support `HttpLogging`:

```json
{
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.File"
    ],
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft.AspNetCore": "Warning",
        "Microsoft.AspNetCore.Hosting.Diagnostics": "Warning",
        "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware": "Information"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "service.log", "rollingInterval": "Day" } }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ],
    "Properties": {
      "Application": "ApplicationName"
    }
  }
}
```

[](#configuring-httplogging)

## Configuring HttpLogging

`HttpLogging` middleware can be configured using the `HttpLoggingOptions` class.

The main option is the `HttpLoggingFields` enum, which controls what should be logged. You can combine multiple flags to get the desired output in logs.

You can view all the available options in the [Microsoft documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.httplogging.httploggingfields).

Here is an example of how you can configure logging for HTTP requests:

```csharp
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = 
        HttpLoggingFields.RequestMethod |
        HttpLoggingFields.RequestPath |
        HttpLoggingFields.RequestQuery |
        HttpLoggingFields.RequestHeaders |
        HttpLoggingFields.ResponseStatusCode |
        HttpLoggingFields.Duration;
});
```

You can also limit the headers that are logged:

```csharp
builder.Services.AddHttpLogging(options =>
{
    options.RequestHeaders.Add("X-API-Version");
    options.RequestHeaders.Add("User-Agent");
    options.ResponseHeaders.Add("Content-Type");
});
```

Logging request and response body is especially powerful for debugging APIs. However, it's critical to consider the performance and security aspects:

*   **Performance:** Logging request and response body can slow down your application, as it needs to read the entire object from the HTTP body. Use this option sparingly in production.
    
*   **Security and Privacy:** Bodies might contain sensitive user data (passwords, credit card information, personally identifiable information, etc.). Always implement redaction strategies or enable body logging only in safe environments (development and staging).
    

If your logs are growing too large, you can configure the maximum size of the request/response body that's allowed to be logged:

```csharp
builder.Services.AddHttpLogging(options =>
{
    options.RequestBodyLogLimit = 4096; // limit in bytes
    options.ResponseBodyLogLimit = 4096;
});
```

The default value is 32 KB.

Here is how you can customize logging for development and production environments:

```csharp
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields =
        HttpLoggingFields.RequestMethod |
        HttpLoggingFields.RequestPath |
        HttpLoggingFields.RequestQuery |
        HttpLoggingFields.RequestHeaders |
        HttpLoggingFields.ResponseStatusCode |
        HttpLoggingFields.ResponseHeaders |
        HttpLoggingFields.Duration;

    // Explicitly log only these headers
    options.RequestHeaders.Add("User-Agent");
    options.ResponseHeaders.Add("Content-Type");
    
    options.RequestBodyLogLimit = 2048; // 2 KB
    options.ResponseBodyLogLimit = 2048; // 2 KB

    // Limit request/response body logging for safe environments
    if (builder.Environment.IsDevelopment())
    {
        options.LoggingFields |= HttpLoggingFields.RequestBody | HttpLoggingFields.ResponseBody;
        options.RequestBodyLogLimit = 1024 * 32; // 32 KB
        options.ResponseBodyLogLimit = 1024 * 32; // 32 KB
    }
});
```

[](#customizing-logs-using-ihttplogginginterceptor)

## Customizing Logs Using IHttpLoggingInterceptor

You might want to customize log entries for specific endpoints:

*   Exclude particular headers dynamically
*   Do not log request and response bodies for specific endpoints
*   Modify logged data to enhance readability or security.

For these cases, you can create your own `IHttpLoggingInterceptor` implementation. That allows you to intercept and customize logs before they are written to the logs.

Implementing this interface enables you to:

*   Dynamically exclude or include specific headers or fields.
*   Redact sensitive data from requests or responses.
*   Add custom information to logs for additional context.

Let's explore an example:

```csharp
public class CustomLoggingInterceptor : IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext context)
    {
        // Example: Remove specific headers from being logged
        context.HttpContext.Request.Headers.Remove("X-API-Key");

        // Example: Add custom information to log
        context.AddParameter("RequestId", Guid.NewGuid().ToString());

        return ValueTask.CompletedTask;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext context)
    {
        // Example: Remove sensitive response header
        logContext.HttpContext.Response.Headers.Remove("Set-Cookie");

        // Example: Log additional context
        context.AddParameter("new-response-field", Guid.NewGuid().ToString());

        return ValueTask.CompletedTask;
    }
}
```

In this example:

*   `OnRequestAsync` is called right before request data is logged, allowing you to customize request logs.
*   `OnResponseAsync` lets you adjust response logs.

Once you have your interceptor class, register it in the Program.cs:

```csharp
builder.Services.AddHttpLogging(loggingOptions =>
{
    // ...
});

builder.Services.AddSingleton<IHttpLoggingInterceptor, CustomLoggingInterceptor>();

var app = builder.Build();

app.UseHttpLogging();
```

[](#endpointspecific-logging-configuration)

## Endpoint-specific Logging Configuration

Often, not all endpoints require the same logging detail. ASP.NET Core lets you configure HTTP logging on a per-endpoint basis.

Here's an example using minimal APIs:

```csharp
app.MapGet("/health", () => Results.Ok())
   .DisableHttpLogging(); // This endpoint logs nothing.

app.MapPost("/api/orders", (OrderRequest request) => 
{
    // Handle order processing
    return Results.Created();
})
.WithHttpLogging(logging =>
{
    logging.LoggingFields = HttpLoggingFields.RequestBody | HttpLoggingFields.ResponseStatusCode;
});
```

[](#how-to-redact-sensitive-data-in-logs)

## How to Redact Sensitive Data in Logs

Keep in mind that logs can capture sensitive information such as passwords, API keys, tokens, and personally identifiable information (PII). Exposing such data in logs can lead to serious security risks and compliance issues.

It's crucial to understand why redaction matters:

*   **Security Risks:** Logs containing tokens, passwords, or API keys can expose your application to unauthorized access and breaches.
    
*   **Compliance Requirements:** Regulations such as GDPR, HIPAA, or PCI DSS explicitly require the careful handling and logging of sensitive information.
    
*   **Trust and Privacy:** Maintaining user privacy and trust demands proactively protecting personal and sensitive information.
    

ASP.NET Core provides built-in capabilities and best practices to redact sensitive data from HTTP logs securely:

*   **Selective header logging:** Explicitly specify which headers to log, thus excluding sensitive headers.
    
*   **Using IHttpLoggingInterceptor:** Dynamically remove or alter sensitive data from logs using a custom interceptor implementation.
    

Here is how you can allow logging only specific headers:

```csharp
builder.Services.AddHttpLogging(options =>
{
    options.LoggingFields = 
        HttpLoggingFields.RequestMethod |
        HttpLoggingFields.RequestPath |
        HttpLoggingFields.ResponseStatusCode;

    // Only explicitly log non-sensitive headers
    options.RequestHeaders.Add("User-Agent");
    options.ResponseHeaders.Add("Content-Type");
});
```

Here is how you can redact sensitive data from logs using an interceptor:

```csharp
public class SensitiveDataRedactionInterceptor : IHttpLoggingInterceptor
{
    public ValueTask OnRequestAsync(HttpLoggingInterceptorContext context)
    {
        if (context.HttpContext.Request.Method == "POST")
        {
            // Don't log anything if the request is a POST.
            context.LoggingFields = HttpLoggingFields.None;
        }

        // Don't enrich if we're not going to log any part of the request.
        if (!context.IsAnyEnabled(HttpLoggingFields.Request))
        {
            return default;
        }

        if (context.TryDisable(HttpLoggingFields.RequestPath))
        {
            RedactPath(context);
        }

        if (context.TryDisable(HttpLoggingFields.RequestHeaders))
        {
            RedactRequestHeaders(context);
        }

        EnrichRequest(context);

        return default;
    }

    public ValueTask OnResponseAsync(HttpLoggingInterceptorContext logContext)
    {
        // Don't enrich if we're not going to log any part of the response
        if (!logContext.IsAnyEnabled(HttpLoggingFields.Response))
        {
            return default;
        }

        if (logContext.TryDisable(HttpLoggingFields.ResponseHeaders))
        {
            RedactResponseHeaders(logContext);
        }

        EnrichResponse(logContext);

        return default;
    }

    private static void RedactPath(HttpLoggingInterceptorContext logContext)
    {
        logContext.AddParameter(nameof(logContext.HttpContext.Request.Path), "[REDACTED]");
    }

    private static void RedactRequestHeaders(HttpLoggingInterceptorContext logContext)
    {
        foreach (var header in logContext.HttpContext.Request.Headers)
        {
            logContext.AddParameter(header.Key, "[REDACTED]");
        }
    }

    private static void EnrichRequest(HttpLoggingInterceptorContext logContext)
    {
        logContext.AddParameter("new-request-field", Guid.NewGuid().ToString());
    }

    private static void RedactResponseHeaders(HttpLoggingInterceptorContext logContext)
    {
        foreach (var header in logContext.HttpContext.Response.Headers)
        {
            logContext.AddParameter(header.Key, "[REDACTED]");
        }
    }

    private static void EnrichResponse(HttpLoggingInterceptorContext logContext)
    {
        logContext.AddParameter("new-response-field", Guid.NewGuid().ToString());
    }
}
```

Let's test a few HTTP requests and how they are logged:

```
GET https://localhost:5000/api/books/7dcec396-4150-4354-a51c-f2f13be3ef4c

info: Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware[1]
      Request:
      Path: [REDACTED]
      Accept: [REDACTED]
      Host: [REDACTED]
      User-Agent: [REDACTED]
      Accept-Encoding: [REDACTED]
      Authorization: [REDACTED]
      new-request-field: 8860d646-24a3-49d3-9405-e1eccc5b7c71
      Method: GET
      QueryString:
info: Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware[2]
      Response:
      Content-Type: [REDACTED]
      new-response-field: a68d1cc0-269f-43e6-90e6-6f9a8ca178a2
      StatusCode: 200
info: Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware[4]
      ResponseBody: {"id":"7dcec396-4150-4354-a51c-f2f13be3ef4c","title":"Fish","year":2024,"authorId":"91a2d8f6-a5cf-4689-8abf-4284bc8cd5a3"}
info: Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware[8]
      Duration: 142.8386ms
```

In this example, the whole response body is logged.

And here is what the 404 Not Found response may look like:

```
GET https://localhost:5000/api/books/d81b2b46-b725-4594-9465-133c443641cb

info: Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware[1]
      Request:
      Path: [REDACTED]
      Accept: [REDACTED]
      Host: [REDACTED]
      User-Agent: [REDACTED]
      Accept-Encoding: [REDACTED]
      Authorization: [REDACTED]
      new-request-field: d856b313-668f-42a7-b8d7-3b40363375bf
      Method: GET
      QueryString:
info: Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware[2]
      Response:
      new-response-field: 3d1717f3-b23d-4689-ad09-de1b81d02cda
      StatusCode: 404
info: Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware[8]
      Duration: 134.0404ms
```

[](#logging-request-and-responses-when-sending-requests-with-httpclient)

## Logging Request and Responses When Sending Requests with HttpClient

We have covered how to log HTTP requests and responses in ASP.NET Core.

However, it's essential to understand how to log requests and responses when using `HttpClient` to send requests to external services. For observability and debugging purposes, it's essential to log interactions with external APIs or services.

You can use the `DelegatingHandler` to inspect all HTTP traffic from your HttpClient. `DelegatingHandler` is a middleware to the HttpClient pipeline that allows you to intercept and modify requests and responses.

Let's explore an example of such a Handler:

```csharp
public class HttpLoggingHandler : DelegatingHandler
{
    private readonly ILogger _logger;
    
    public HttpLoggingHandler(ILogger logger)
    {
        _logger = logger;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var traceId = request.Headers.TryGetValues("trace-id", out var values) ? values.FirstOrDefault() : null;
        traceId ??= Guid.NewGuid().ToString();

        var requestBuilder = new StringBuilder();

        var url = $"{request.RequestUri?.Host}:{request.RequestUri?.Port}{request.RequestUri?.AbsolutePath}";
        var headers = request.Headers.ExceptSensitiveHeaders().Select(x => $"[{x.Key}, {string.Join(",", x.Value)}]");

        requestBuilder.AppendLine($"[REQUEST] {traceId}");
        requestBuilder.AppendLine($"{request.Method}: {request.RequestUri?.Scheme}://{url}");
        requestBuilder.AppendLine($"Headers: {string.Join(", ", headers)}");

        if (request.Content != null)
        {
            if (request.Content.Headers.Any())
            {
                var contentHeaders = request.Content.Headers
                    .ExceptSensitiveHeaders().Select(x => $"[{x.Key}, {string.Join(",", x.Value)}]");
                
                requestBuilder.AppendLine($"Content headers: {string.Join(", ", contentHeaders)}");
            }

            if (RequestCanBeLogged(request.RequestUri?.AbsolutePath))
            {
                requestBuilder.AppendLine("Content:");
                requestBuilder.AppendLine(await request.Content.ReadAsStringAsync(cancellationToken));
            }
        }

        _logger.LogDebug("{Request}", requestBuilder.ToString());

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var response = await base.SendAsync(request, cancellationToken);
        stopwatch.Stop();

        var responseBuilder = new StringBuilder();
        responseBuilder.AppendLine($"[RESPONSE] {traceId}");
        responseBuilder.AppendLine($"{request.Method}: {request.RequestUri?.Scheme}://{url} {(int)response.StatusCode} {response.ReasonPhrase} executed in {stopwatch.Elapsed.TotalMilliseconds} ms");
        responseBuilder.AppendLine($"Headers: {string.Join(", ", response.Headers.Select(x => $"[{x.Key}, {string.Join(",", x.Value)}]"))}");

        if (response.Content.Headers.Any())
        {
            var contentHeaders = response.Content
                .Headers.Select(x => $"[{x.Key}, {string.Join(",", x.Value)}]");
            
            requestBuilder.AppendLine($"Content headers: {string.Join(", ", contentHeaders)}");
        }

        if (ResponseCanBeLogged(request.RequestUri?.AbsolutePath) && ResponseCanBeLogged(request.RequestUri?.AbsolutePath))
        {
            responseBuilder.AppendLine("Content:");
            responseBuilder.AppendLine(await response.Content.ReadAsStringAsync(cancellationToken));
        }

        _logger.LogDebug("{Response}", responseBuilder.ToString());
        _logger.LogDebug("Request completed in {ElapsedTotalMilliseconds}ms", stopwatch.Elapsed.TotalMilliseconds);
        return response;
    }
}
```

Here, I use the following methods to redact the data:

1.  `ExceptSensitiveHeaders`: Redact sensitive headers from the logs.
2.  `RequestCanBeLogged`: Check if the request can be logged.
3.  `ResponseCanBeLogged`: Check if the response can be logged.

For `ExceptSensitiveHeaders`, I have created an extension method on `HttpRequestHeaders` and `HttpContentHeaders`:

```csharp
internal static class HttpLoggingHelpers
{
    public static IEnumerable<KeyValuePair<string, IEnumerable<string>>> ExceptSensitiveHeaders(
        this HttpRequestHeaders headers)
    {
        return headers.Where(x => !x.Key.Contains("Authorization",
            StringComparison.OrdinalIgnoreCase));
    }

    public static IEnumerable<KeyValuePair<string, IEnumerable<string>>> ExceptSensitiveHeaders(
        this HttpContentHeaders headers)
    {
        return headers.Where(x => !x.Key.Contains("secret-token",
            StringComparison.OrdinalIgnoreCase));
    }
}
```

Here is the naive implementation of checking for allowed URLs:

```csharp
private static readonly List<string> NotAllowedRequestUrls = new()
{
    "/api/users/login",
    "/api/users/refresh"
};

private static readonly List<string> NotAllowedResponseUrls = new()
{
    "/api/users/login",
    "/api/users/refresh"
};

private static bool RequestCanBeLogged(string? url)
{
    if (string.IsNullOrEmpty(url))
    {
        return false;
    }

    return !NotAllowedRequestUrls.Any(x => x.Contains(url, StringComparison.OrdinalIgnoreCase));
}

private static bool ResponseCanBeLogged(string? url)
{
    if (string.IsNullOrEmpty(url))
    {
        return false;
    }

    return !NotAllowedResponseUrls.Any(x => x.Contains(url, StringComparison.OrdinalIgnoreCase));
}
```

For production, you should configure these options, but for example, it's more than sufficient.

To use this handler, you must register it when configuring HttpClient in `Program.cs`:

```csharp
builder.Services.AddHttpClient<ITodoClient, TodoClient>(client =>
{
    client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
})
.AddHttpMessageHandler(configure =>
{
    var logger = configure.GetRequiredService<ILoggerFactory>()
        .CreateLogger("json-placeholder-todos");
    
    return new HttpLoggingHandler(logger);
});
```

This approach also fits well with Refit and Polly:

```csharp
builder.Services.AddTransient<LoggingHandler>();

services
    .AddRefitClient<ITodoApi>()
    .ConfigureHttpClient((provider, c) =>
    {
        client.BaseAddress = new Uri("https://jsonplaceholder.typicode.com/");
    })
    .AddHttpMessageHandler(configure =>
    {
        var logger = configure.GetRequiredService<ILoggerFactory>()
            .CreateLogger("json-placeholder-todos");
        
        return new HttpLoggingHandler(logger);
    });
```

`jsonplaceholder` is a public API that allows you to test HTTP requests. Let's send a request to `https://jsonplaceholder.typicode.com/todos/50` and see what the logs look like:

```
dbug: json-placeholder-todos[0]
      [REQUEST] 4d251738-5aeb-4342-824e-5d23ddf738a2
      GET: https://jsonplaceholder.typicode.com:443/todos/50
      Headers:
      
dbug: json-placeholder-todos[0]
      [RESPONSE] 4d251738-5aeb-4342-824e-5d23ddf738a2
      GET: https://jsonplaceholder.typicode.com:443/todos/50 200 OK executed in 561,7971 ms
      Headers: [Date, Tue, 10 Jun 2025 05:18:23 GMT], [Cache-Control, max-age=43200]
      Content headers: [Content-Type, application/json; charset=utf-8], [Content-Length, 121]
      Content:
      {
        "userId": 3,
        "id": 50,
        "title": "cupiditate necessitatibus ullam aut quis dolor voluptate",
        "completed": true
      }

dbug: json-placeholder-todos[0]
      Request completed in 406,5857ms
```

[](#summary)

## Summary

Logging is essential for maintaining robust, secure, and maintainable ASP.NET Core applications. Proper logging of HTTP requests and responses enables developers and teams to quickly diagnose issues, monitor application performance, and meet critical compliance requirements.

Throughout this guide, we've explored various built-in and advanced techniques provided by ASP.NET Core to achieve precise control over HTTP logging.

**Follow these recommendations to keep sensitive data secure in logs:**

*   **Default to Exclusion:** Log only what's explicitly safe. Avoid logging sensitive information by default.
    
*   **Environment-specific Logging:** Enable detailed logs with sensitive data only for testing purposes during development, and never in production.
    
*   **Regex for Data Masking:** Carefully use regular expressions or structured data parsers (like JSON parsers) to mask sensitive fields precisely.
    
*   **Testing:** Verify your redaction rules through automated tests, ensuring no sensitive data accidentally leaks into logs.
    

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/logging-requests-and-responses-for-api-requests-and-httpclient-in-aspnetcore)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Flogging-requests-and-responses-for-api-requests-and-httpclient-in-aspnetcore&title=Logging%20Requests%20and%20Responses%20for%20API%20Requests%20and%20HttpClient%20in%20ASP.NET%20Core)[X](https://twitter.com/intent/tweet?text=Logging%20Requests%20and%20Responses%20for%20API%20Requests%20and%20HttpClient%20in%20ASP.NET%20Core&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Flogging-requests-and-responses-for-api-requests-and-httpclient-in-aspnetcore)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Flogging-requests-and-responses-for-api-requests-and-httpclient-in-aspnetcore)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 1