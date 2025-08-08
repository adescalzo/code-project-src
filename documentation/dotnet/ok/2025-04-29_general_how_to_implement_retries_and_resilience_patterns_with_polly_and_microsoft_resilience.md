```yaml
---
title: How To Implement Retries and Resilience Patterns With Polly and Microsoft Resilience
source: https://antondevtips.com/blog/how-to-implement-retries-and-resilience-patterns-with-polly-and-microsoft-resilience
date_published: 2025-04-29T07:45:27.026Z
date_captured: 2025-08-06T16:44:51.008Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [.NET, ASP.NET Core, Polly, Microsoft.Extensions.Resilience, HttpClientFactory, HttpClient, NuGet]
programming_languages: [C#]
tags: [resilience, retry-pattern, circuit-breaker, timeout, fallback, hedging, polly, dotnet, httpclient, error-handling]
key_concepts: [Resilience Patterns, Retry Pattern, Circuit Breaker Pattern, Timeout Pattern, Fallback Pattern, Hedging Pattern, HttpClientFactory, Exponential Backoff with Jitter]
code_examples: false
difficulty_level: intermediate
summary: |
  [Modern .NET applications often rely on external services, making resilience crucial for handling transient failures and network instability. This article demonstrates how to build robust applications using Polly and Microsoft.Extensions.Resilience. It covers five key resilience patterns: Retry, Circuit Breaker, Timeout, Fallback, and Hedging. Each pattern is explained with its purpose, states, and practical C# code examples, showcasing integration with HttpClientFactory. By implementing these strategies, developers can create stable .NET applications that gracefully manage external service disruptions.]
---
```

# How To Implement Retries and Resilience Patterns With Polly and Microsoft Resilience

![Banner image for an article titled "How To Implement Retries and Resilience Patterns With Polly and Microsoft Resilience", featuring a "dev tips" logo with a code icon on a dark background with purple abstract shapes.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fdotnet%2Fcover_dotnet_resilience.png&w=3840&q=100)

# How To Implement Retries and Resilience Patterns With Polly and Microsoft Resilience

Apr 29, 2025

[Download source code](/source-code/how-to-implement-retries-and-resilience-patterns-with-polly-and-microsoft-resilience)

7 min read

### Newsletter Sponsors

[Optimize SQL write operations](https://dapper-plus.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) for speed, scale, and minimal server load. Insert and update your data up to 75x faster — and reduce save time by 99% compared to Dapper. Discover [Dapper Plus](https://dapper-plus.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) — Bulk Insert, Update, Delete & Merge

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Modern .NET applications often rely on external services for data, messaging, and more. A single network error can cause cascading failures if not handled properly.

Here are a few problems you can suffer when calling external APIs:

*   Transient failures: An external service might be slow or unreachable for a brief moment.
*   Partial outages: Another system could be in maintenance mode, limiting your access.
*   Network instability: Users with slow internet connections might experience short outages or timeouts.
*   Overloaded service: Another service could be overloaded with requests, leading to slow responses.

Networks are unstable, and you will suffer such problems sooner or later.

Here is the phrase that changed how I think about building resilient systems: "A systems resilience is not defined by its lack of errors. But its ability to survive many errors."

That's where **Resilience Patterns** come in. Instead of failing immediately, you give these services a second chance to retry or to route calls to a backup service, improving application robustness and stability.

Today I want to show you how to build resilient applications with **Polly** and **Microsoft.Extensions.Resilience** packages.

Let's dive in.

## Getting Started with Polly and Microsoft.Resilience

Let's explore two services: Orders and Payments. When a user order is created, the next step is to perform a payment.

```csharp
using System;
using System.Net.Http;

builder.Services.AddHttpClient("PaymentsClient", client =>
{
    client.BaseAddress = new Uri("http://localhost:5221");
    client.Timeout = TimeSpan.FromSeconds(60);
});

var client = clientFactory.CreateClient("PaymentsClient");
var response = await client.PostAsync("/api/payments/create", null);
response.EnsureSuccessStatusCode();
```

This is crucial to ensure that when a user pays money, the payment request is processed correctly without any issues.

**There are 5 main Resilience patterns:**

*   Retry
*   Circuit Breaker
*   Timeout
*   Fallback
*   Hedging

In .NET applications these Resilience patterns are implemented with the help of two libraries:

*   [Polly](https://github.com/App-vNext/Polly)
*   [Microsoft.Extensions.Resilience](https://learn.microsoft.com/en-us/dotnet/core/resilience/http-resilience)

**Polly** is a classic .NET library known for years that helps you implement Resilience patterns with minimal effort. It allows creating Resilience pipelines around any code: network calls, database, caching, sending emails, etc.

**Microsoft.Extensions.Resilience** is built on top of Polly. It provides seamless integration with the .NET **HttpClientFactory**.

This allows you to apply resilience pipelines to your external HTTP calls.

Add the following Nuget packages to the Orders Service:

```bash
dotnet add package Polly
dotnet add package Microsoft.Extensions.Resilience
```

Now let's dive into each Resilience pattern.

## Implementing Retry Pattern

When a service call fails due to network errors, devs often just show a general error message: "Please wait and try again later." on the screen. Instead of failing fast, implementing **retries** improves system resilience by giving the service another chance to recover before throwing an error.

Here are the main types of retries:

*   **Linear Retry**: Retries after a fixed delay period between attempts.
*   **Exponential Retry**: Retries with an exponentially increasing delay between attempts.
*   **Random Retry**: Adds a randomized delay between retries to distribute load and avoid retries clustering.
*   **Exponential Backoff with Jitter**: Combines exponential growth in delays with random jitter to avoid retry storms.

**Exponential Backoff with Jitter** is considered as the most reliable retrying strategy. Luckily you don't need to implement it yourself, Polly has a built-in pipeline for it.

This is how you can define a Retry pipeline:

```csharp
using Polly;
using Polly.Retry;

var retryOptions = new RetryStrategyOptions
{
    ShouldHandle = new PredicateBuilder()
        .Handle<HttpRequestException>(),
        
    BackoffType = DelayBackoffType.Exponential,
    UseJitter = true,
    MaxRetryAttempts = 5,
    Delay = TimeSpan.FromSeconds(3),
};

var pipeline = new ResiliencePipelineBuilder()
    .AddRetry(retryOptions)
    .Build();
```

Now you can wrap any code that will be automatically retried. Here is how you can use it for HttpClient calls:

```csharp
using System.Net.Http;
using System.Threading;
using Polly;

// Usage
var result = await pipeline.ExecuteAsync(async token =>
{
    var response = await httpClient.PostAsync("/api/payments/create");
    response.EnsureSuccessStatusCode(); 
    return await response.Content.ReadAsStringAsync();
}, cancellationToken);
```

Microsoft.Extensions.Resilience provides a more convenient way to apply retries to all HttpClient calls:

```csharp
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Retry;

builder.Services.AddHttpClient("PaymentService", client =>
{
    client.BaseAddress = new Uri("https://azure.paymentservice");
})
.AddResilienceHandler("RetryStrategy", resilienceBuilder =>
{
    resilienceBuilder.AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 4,
        Delay = TimeSpan.FromSeconds(3),
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
        ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
            .Handle<HttpRequestException>()
            .HandleResult(response => !response.IsSuccessStatusCode)
    });
});
```

When sending any requests by the given HttpClient, a Retry pipeline will automatically be called by a `DelegateHandler` created behind the scenes.

Having retries is always beneficial as it avoids unnecessary failures caused by transient network issues.

## Implementing Circuit Breaker Pattern

Imagine a service is not just briefly unavailable but is consistently failing. Endless retries can make the situation even worse. A **circuit breaker** cuts off requests when it detects a high number of failures, returning errors immediately until a cool-down period ends.

Circuit break works like an electric circuit.

**It has the following states:**

*   **Closed State:** everything works fine; network calls go through.
*   **Open State:** after too many failures, the circuit "opens" and blocks calls for a set duration. It returns errors immediately.
*   **Half-Open State:** when the break time ends, it tests a few calls to see if the service is healthy again. If successful, it transitions back to Closed; if not, it stays Open.

Here is how you can define a Circuit Breaker pipeline:

```csharp
using Polly;
using Polly.CircuitBreaker;

var options = new CircuitBreakerStrategyOptions
{
    FailureRatio = 0.5,
    SamplingDuration = TimeSpan.FromSeconds(10),
    MinimumThroughput = 8,
    BreakDuration = TimeSpan.FromSeconds(30),
    ShouldHandle = new PredicateBuilder()
        .Handle<HttpRequestException>()
};

var pipeline = new ResiliencePipelineBuilder()
    .AddCircuitBreaker(options)
    .Build();
```

Let's test our circuit breaker on HttpClient:

```csharp
for (int i = 0; i < 10; i++)
{
    var result = await pipeline.ExecuteAsync(async token =>
    {
        var response = await httpClient.PostAsync("/api/payments/create");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }, cancellationToken);
}
```

Microsoft.Extensions.Resilience provides a more convenient way to add Circuit Breaker to all HttpClient calls:

```csharp
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.CircuitBreaker;
using System.Net; // For HttpStatusCode

builder.Services.AddHttpClient("PaymentService", client =>
{
    client.BaseAddress = new Uri("https://azure.paymentservice");
})
.AddResilienceHandler("CurcuitBreakerStrategy", resilienceBuilder =>
{
    resilienceBuilder.AddCircuitBreaker(
        new HttpCircuitBreakerStrategyOptions
        {
            FailureRatio = 0.5,
            SamplingDuration = TimeSpan.FromSeconds(10),
            MinimumThroughput = 8,
            BreakDuration = TimeSpan.FromSeconds(30),
            ShouldHandle = static args => ValueTask.FromResult(args is
            {
                Outcome.Result.StatusCode:
                HttpStatusCode.RequestTimeout or
                HttpStatusCode.TooManyRequests
            })
        });
});
```

Circuit breaker allows failing fast when external service is unavailable and quickly recover when the service is back online.

## Implementing Timeout Pattern

Some services can be really fast to respond, others can be really slow or even hang indefinitely.

That's why you need to implement timeouts for your network requests.

A **Timeout pattern** ensures your request fails in a controlled manner.

**Best practices:**

*   Set a global timeout on your HttpClient (e.g., 30–60 seconds).
*   Within each retry, have a smaller "per attempt" timeout so you don't wait too long each time.

Timeout pattern is especially relevant for HttpClient calls. Here is how you can set it up:

```csharp
using Microsoft.Extensions.Http.Resilience;
using Polly;

builder.Services.AddHttpClient("PaymentService", client =>
{
    client.BaseAddress = new Uri("https://azure.paymentservice");
    client.Timeout = TimeSpan.FromSeconds(60); // Global request timeout
})
.AddResilienceHandler("TimeoutStrategy", resilienceBuilder =>
{
    resilienceBuilder
        .AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 4,
            Delay = TimeSpan.FromSeconds(3),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .HandleResult(response => !response.IsSuccessStatusCode)
        })
        .AddTimeout(TimeSpan.FromSeconds(10)); // per attempt timeout
});
```

## Implementing Fallback Pattern

What if all retries fail? A **Fallback pattern** provides a graceful way to handle the worst-case scenario.

It allows executing an action when all retries fail. It is great for emergency routines: logging errors, sending alerts, or returning cached data.

Often a Fallback pattern comes together with retries. Here is how to create it:

```csharp
using Polly;
using Polly.Fallback;
using Polly.Retry;
using System.Net; // For HttpStatusCode

var retryOptions = new RetryStrategyOptions<HttpResponseMessage>
{
    ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
        .Handle<HttpRequestException>(),
    
    BackoffType = DelayBackoffType.Exponential,
    UseJitter = true,
    MaxRetryAttempts = 5,
    Delay = TimeSpan.FromSeconds(3),
};

var fallbackOptions = new FallbackStrategyOptions<HttpResponseMessage>
{
    ShouldHandle = new PredicateBuilder<HttpResponseMessage>().Handle<ApplicationException>(),
    FallbackAction = args =>
    {
        Console.WriteLine("All retries failed. Sending alert email...");
        //await SendFailureEmailAsync();
        return Outcome.FromResultAsValueTask(
            new HttpResponseMessage(HttpStatusCode.InternalServerError));
    }
};
```

Here is how you can combine Retry and Fallback pipelines and wrap HttpClient calls with a combined pipeline:

```csharp
using System.Net.Http;
using System.Threading;
using Polly;

var pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>()
    .AddFallback(fallbackOptions)
    .AddRetry(retryOptions)
    .Build();

var result = await pipeline.ExecuteAsync(async token =>
{
    var response = await httpClient.PostAsync("/api/payments/create", token);
    response.EnsureSuccessStatusCode();
    return await response.Content.ReadAsStringAsync();
}, CancellationToken.None);
```

Here is how you can configure Fallbacks for HttpClient:

```csharp
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Polly.Fallback;
using Polly.Retry;
using System.Net; // For HttpStatusCode

builder.Services.AddHttpClient("PaymentService", client =>
{
    client.BaseAddress = new Uri("https://azure.paymentservice");
})
.AddResilienceHandler("RetryFallbackStrategy", resilienceBuilder =>
{
    resilienceBuilder
        .AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = TimeSpan.FromSeconds(2),
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .Handle<HttpRequestException>()
                .HandleResult(response => !response.IsSuccessStatusCode)
        })
        .AddFallback(new FallbackStrategyOptions<HttpResponseMessage>
        {
            ShouldHandle = new PredicateBuilder<HttpResponseMessage>()
                .