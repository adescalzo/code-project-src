```yaml
---
title: Resilient .NET Application with Polly Resilience Pipelines
source: https://www.nikolatech.net/blogs/resilient-dotnet-application-with-polly-resilience-pipelines
date_published: 2024-12-27T14:23:48.882Z
date_captured: 2025-08-08T13:18:09.022Z
domain: www.nikolatech.net
author: Unknown
category: general
technologies: [.NET, Polly, Microsoft.Extensions.Resilience, Microsoft.Extensions.Http.Resilience, HttpClient, NuGet]
programming_languages: [C#, PowerShell]
tags: [resilience, dotnet, polly, error-handling, distributed-systems, httpclient, fault-tolerance, dependency-injection, design-patterns, transient-faults]
key_concepts: [resilience, resilience-pipelines, transient-faults, reactive-strategies, proactive-strategies, retry, circuit-breaker, fallback, hedging, timeout, rate-limiting, dependency-injection, http-resilience]
code_examples: true
difficulty_level: intermediate
summary: |
  This article explains how to build resilient .NET applications to gracefully handle failures, especially in complex distributed systems. It introduces Polly, a powerful .NET library for managing transient faults, detailing its reactive strategies like Retry, Circuit-breaker, Fallback, and Hedging, as well as proactive strategies such as Timeout and Rate Limiter. The content demonstrates implementing resilience using the Microsoft.Extensions.Resilience and Microsoft.Extensions.Http.Resilience NuGet packages, including how to construct and register resilience pipelines with dependency injection. The article also covers applying these resilience mechanisms specifically to HttpClient for robust HTTP communication, ensuring applications remain functional and reliable.
---
```

# Resilient .NET Application with Polly Resilience Pipelines

![A banner image with a blue and black gradient background. It features the text 'RESILIENCE IN .NET' and 'Polly Resilience Pipelines' in prominent white and black fonts. The top left corner has an 'NK' logo, and the bottom left includes a colorful parrot-like bird logo next to a stylized Windows logo, indicating a technical topic related to Microsoft technologies.](https://coekcx.github.io/BlogImages/banners/resilient-dotnet-application-with-polly-resilience-pipelines-banner.png)

#### Resilient .NET Application with Polly Resilience Pipelines

###### 27 Dec 2024

###### 6 min

## Happy New Year! 🎉

With the arrival of 2025, I’d like to take a moment to wish you a year filled with success, growth and new possibilities.

Thank you for being a part of this journey. I look forward to sharing more insights, stories and exciting content with you throughout the year.

Let’s make 2025 our best year yet. Happy reading!

## Resilience

Building resilient applications means handling failures gracefully to ensure they continue functioning smoothly despite errors or unexpected behavior.

This is especially crucial in distributed systems, where failures are common due to their complexity and dependencies.

By implementing resilience strategies, developers can keep applications functional, performant, and reliable even in the face of unforeseen issues.

## Polly

**Polly** is a powerful library for .NET that helps you handle transient faults and improve the resilience of your applications.

With Polly, you can easily define and apply different resilient strategies, which are categorized into two main groups:

**Reactive** - These strategies handle specific exceptions or results by the callbacks executed within the strategy.

*   **Retry** - Attempt the operation again if it fails, can be useful when the issue is temporary and likely to resolve itself.
*   **Circuit-breaker** - Stop attempting if something is broken or overloaded, helping prevent wasted time while allowing the system to recover.
*   **Fallback** - Perform an alternative action if something fails, enhancing user experience and ensuring everything continues to function.
*   **Hedging** - Execute multiple actions simultaneously and use the fastest result, improving speed and responsiveness.

**Proactive** - Unlike reactive strategies, proactive strategies focus on preventing errors by canceling or rejecting callback executions, rather than handling errors after they occur.

*   **Timeout** - Abort if something takes too long, helping improve performance by freeing up resources and space.
*   **Rate Limiter** - Restrict the number of requests you make or accept, helping control load and avoid issues or penalties.

## Getting started

In this blog, instead of using Polly directly, we will use the **Microsoft.Extensions.Resilience** and **Microsoft.Extensions.Http.Resilience** packages, which are built on top of Polly.

To get started, you need to install the NuGet packages. You can do this via the NuGet Package Manager or by running the following command in the Package Manager Console:

```powershell
Install-Package Microsoft.Extensions.Resilience
Install-Package Microsoft.Extensions.Http.Resilience
```

## Resilience Pipelines

To implement resilience, you need to build a pipeline of resilience-based strategies. Each strategy runs in the order it is set up, so the sequence is important.

To begin, create an instance of **ResiliencePipelineBuilder**, which serves as the starting point for configuring and combining resilience strategies into a unified pipeline.

To add a strategy to the pipeline, use one of the available Add\* extension methods on the ResiliencePipelineBuilder instance.

Once you add desired strategies, use the **Build** method to create a configured ResiliencePipeline instance that applies these strategies.

```csharp
var pipeline = new ResiliencePipelineBuilder<CurrencyLimitResponse?>()
    .AddFallback(new FallbackStrategyOptions<CurrencyLimitResponse?>
    {
        FallbackAction = _ => Outcome.FromResultAsValueTask<CurrencyLimitResponse?>(
            new CurrencyLimitResponse())
    })
    .Build();
```

In this example, we added a fallback strategy using the **AddFallback** method and provided the appropriate options to define the outcome in case of a failure.

This approach ensures the application can recover gracefully by substituting a default or previously cached response when an operation fails.

To execute the pipeline, call the **ExecuteAsync** method, passing in the desired delegate:

```csharp
var response = await pipeline.ExecuteAsync(
    async ct => await cryptoApi.GetLimits(ct),
    cancellationToken);
```

However, realistically, no one would want to create and configure a pipeline every time one is needed, which is why it's better to register resilience pipelines with dependency injection.

To register a pipeline, you can configure it for a specific key and use that key to retrieve the corresponding resilience pipeline from the provider.

```csharp
builder.Services.AddResiliencePipeline("retry", pipelineBuilder =>
{
    pipelineBuilder.AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<Exception>(),
        Delay = TimeSpan.FromSeconds(2),
        MaxRetryAttempts = 3,
        UseJitter = true,
        OnRetry = args =>
        {
            Console.WriteLine($"Attempt: {args.AttemptNumber}");
            return ValueTask.CompletedTask;
        }
    });
});
```

To use resilience pipelines configured with dependency injection, we can utilize the **ResiliencePipelineProvider**, which provides a **GetPipeline** method to retrieve the pipeline instance using the key provided during registration.

```csharp
app.MapGet("crypto/limits", async (
    CryptoApiClient cryptoApi,
    ResiliencePipelineProvider<string> pipelineProvider,
    CancellationToken cancellationToken) =>
{
    ResiliencePipeline pipeline = pipelineProvider.GetPipeline("retry");
    var response = await pipeline.ExecuteAsync(
        async ct => await cryptoApi.GetLimits(ct),
        cancellationToken);
    return Results.Ok(response);
});
```

## Http Client

Building resilient HTTP apps that can recover from transient errors is a common task.

To assist in this, the **Microsoft.Extensions.Http.Resilience** NuGet package provides resilience mechanisms specifically for HttpClient. You can add resilience to an HttpClient by chaining it with the AddHttpClient method.

The library offers predefined handlers like **AddStandardResilienceHandler** and **AddStandardHedgingHandler**, which follow various industry best practices.

```csharp
builder.Services.AddHttpClient<CryptoApiClient>(client =>
        client.BaseAddress = new Uri("https://cex.io/api/"))
    .AddStandardResilienceHandler();
    
builder.Services.AddHttpClient<NikolaTechApiClient>(client =>
        client.BaseAddress = new Uri("https://nikolatech.net/"))
    .AddStandardHedgingHandler();
```

These handlers implement five different strategies, and you can customize their default settings to adjust their behavior.

It’s recommended to add only one resilience handler and avoid stacking multiple handlers. If you need more than one, consider using the **AddResilienceHandler** extension method, which lets you customize the resilience strategies to meet your needs.

## Conclusion

With resilient apps, you can relax and fully enjoy your holidays without any worries.

The ability to automatically recover from faults leads to better system performance and reduced operational overhead. Additionally, as systems grow more complex, having resilience mechanisms in place becomes essential to manage the increasing volume of requests and errors effectively.

By implementing resilience pipelines, you can ensure your applications continue to run seamlessly.

If you want to check out examples I created, you can find the source code here:

[Source Code](https://www.nikolatech.net/codes/resilience-examples)

I hope you enjoyed it, subscribe and get a notification when a new blog is up!

###### Stay tuned for valuable insights every Thursday morning.

##### Share This Article: