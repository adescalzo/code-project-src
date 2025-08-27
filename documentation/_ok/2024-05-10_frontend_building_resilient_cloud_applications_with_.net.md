```yaml
---
title: Building Resilient Cloud Applications With .NET
source: https://www.milanjovanovic.tech/blog/building-resilient-cloud-applications-with-dotnet?utm_source=newsletter&utm_medium=email&utm_campaign=tnw127
date_published: 2024-05-11T00:00:00.000Z
date_captured: 2025-08-08T21:42:13.847Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: frontend
technologies: [.NET, .NET 8, Microsoft.Extensions.Resilience, Microsoft.Extensions.Http.Resilience, Polly, NuGet, HttpClient, ASP.NET Core]
programming_languages: [C#, PowerShell]
tags: [resilience, dotnet, cloud-applications, microservices, http-requests, polly, error-handling, dependency-injection, design-patterns]
key_concepts: [resilience, resilience-pipelines, retry-pattern, timeout-pattern, fallback-pattern, circuit-breaker, rate-limiting, dependency-injection]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article delves into building resilient cloud applications using .NET, focusing on handling transient failures in distributed systems. It introduces `Microsoft.Extensions.Resilience` and `Microsoft.Extensions.Http.Resilience` in .NET 8, which leverage the Polly library. The author explains various resilience strategies such as retries, timeouts, fallbacks, circuit breakers, hedging, and rate limiting. Practical C# code examples demonstrate how to implement these resilience pipelines, including integration with dependency injection and HttpClient. The piece concludes by emphasizing the importance of these tools for creating robust and reliable software.]
---
```

# Building Resilient Cloud Applications With .NET

![](data:image/svg+xml,%3csvg%20xmlns=%27http://www.w3.org/2000/svg%27%20version=%271.1%27%20width=%271280%27%20height=%27720%27/%3e)![Blog cover: Resilience, Polly - Building Resilient .NET Applications with MJ Tech logo](/blog-covers/mnw_089.png?imwidth=3840)

![Blog cover: Resilience, Polly - Building Resilient .NET Applications with MJ Tech logo](/blog-covers/mnw_089.png?imwidth=3840)

# Building Resilient Cloud Applications With .NET

7 min read · May 11, 2024

Build API applications visually using [**Postman Flows**](https://learning.postman.com/docs/postman-flows/gs/flows-overview/). Postman Flows is a visual tool for building API-driven applications for the API-first world. You can use Flows to chain requests, handle data, and create real-world workflows in your Postman workspace. [**Get started here**](https://learning.postman.com/docs/postman-flows/gs/flows-overview/).

[**9 Best Practices for Building Blazor Web Applications**](https://www.telerik.com/blogs/blazor-basics-9-best-practices-building-blazor-web-applications?utm_medium=cpm&utm_source=milanjovanovic&utm_campaign=blazor-general-awareness): In this article, you will learn nine best practices for building Blazor web applications by the .NET developer and YouTube influencer Claudio Bernasconi. [**Read it here**](https://www.telerik.com/blogs/blazor-basics-9-best-practices-building-blazor-web-applications?utm_medium=cpm&utm_source=milanjovanovic&utm_campaign=blazor-general-awareness).

[

Sponsor this newsletter

](/sponsor-the-newsletter)

From my experience working with microservices systems, things don't always go as planned. Network requests randomly fail, application servers become overloaded, and unexpected errors appear. That's where resilience comes in.

Resilient applications can recover from transient failures and continue to function. Resilience is achieved by designing applications that can handle failures gracefully and recover quickly.

By designing your applications with resilience in mind, you can create robust and reliable systems, even when the going gets tough.

In this newsletter, we'll explore the tools and techniques we have in .NET to build resilient systems.

## [Resilience: Why You Should Care](#resilience-why-you-should-care)

Sending HTTP requests is a common approach for remote communication between services. However, HTTP requests are susceptible to failures from network or server issues. These failures can disrupt service availability, especially as dependencies increase and the risk of cascading failures grows.

So, how can you improve the resilience of your applications and services?

Here are a few strategies you can consider to increase resilience:

*   **Retries**: Retry requests that fail due to transient errors.
*   **Timeouts**: Cancel requests that exceed a specified time limit.
*   **Fallbacks**: Define alternative actions or results for failed operations.
*   **Circuit Breakers**: Temporarily suspend communication with unavailable services.

You can use these strategies individually or in combination for optimal HTTP request resilience.

Let's see how we can introduce resilience in a .NET application.

## [Resilience Pipelines](#resilience-pipelines)

With .NET 8, integrating resilience into your applications has become much simpler. We can use `Microsoft.Extensions.Resilience` and `Microsoft.Extensions.Http.Resilience`, which are built on top of [Polly](https://github.com/App-vNext/Polly). Polly is a .NET resilience and transient fault-handling library. Polly allows us to define resilience strategies such as retry, circuit breaker, timeout, rate-limiting, fallback, and hedging.

Polly received a new API surface in its latest version (V8), which was implemented in collaboration with Microsoft. You can learn more about the [**Polly V8 API in this video**](https://youtu.be/PqVQFUCTzUM).

If you were previously using `Microsoft.Extensions.Http.Polly`, it is recommended that you switch to one of the previously mentioned packages.

Let's start by installing the required NuGet packages:

```powershell
Install-Package Microsoft.Extensions.Resilience
Install-Package Microsoft.Extensions.Http.Resilience
```

To use resilience, you must first build a pipeline consisting of resilience [strategies](https://www.pollydocs.org/strategies/). Each strategy that we configure as part of the pipeline will execute in order of configuration. Order is important with resilience pipelines. Keep that in mind.

We start by creating an instance of `ResiliencePipelineBuilder`, which allows us to configure resilience strategies.

```csharp
ResiliencePipeline pipeline = new ResiliencePipelineBuilder()
    .AddRetry(new RetryStrategyOptions
    {
        ShouldHandle = new PredicateBuilder().Handle<ConflictException>(),
        Delay = TimeSpan.FromSeconds(1),
        MaxRetryAttempts = 2,
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true
    })
    .AddTimeout(new TimeoutStrategyOptions
    {
        Timeout = TimeSpan.FromSeconds(10)
    })
    .Build();

await pipeline.ExecuteAsync(
    async ct => await httpClient.GetAsync("https://modularmonolith.com", ct),
    cancellationToken);
```

Here's what we're adding to the resilience pipeline:

*   `AddRetry` - Configures a retry resilience strategy, which we can further configure by passing in a `RetryStrategyOptions` instance. We can provide a predicate for the `ShouldHandle` property to define which exceptions the resilience strategy should handle. The retry strategy also comes with some sensible [default values](https://www.pollydocs.org/strategies/retry.html#defaults).
*   `AddTimeout` - Configures a timeout strategy that will throw a `TimeoutRejectedException` if the delegate does not complete before the timeout. We can provide a custom timeout by passing in a `TimeoutStrategyOptions` instance. The default timeout is 30 seconds.

Finally, we can `Build` the resilience pipeline and get back a configured `ResiliencePipeline` instance that will apply the respective resilience strategies. To use the `ResiliencePipeline`, we can call the `ExecuteAsync` method and pass in a delegate.

## [Resilience Pipelines and Dependency Injection](#resilience-pipelines-and-dependency-injection)

Configuring a resilience pipeline every time we want to use it is cumbersome. .NET 8 introduces a new extension method for the `IServiceCollection` interface that allows us to register resilience pipelines with dependency injection.

Instead of manually configuring resilience every time, you ask for a pre-made pipeline by name.

We start by calling the `AddResiliencePipeline` method, which allows us to configure the resilience pipeline. Each resilience pipeline needs to have a unique key. We can use this key to resolve the respective resilience pipeline instance.

In this example, we're passing in a `string` key which allows us to configure the non-generic `ResiliencePipelineBuilder`.

```csharp
services.AddResiliencePipeline("retry", builder =>
{
    builder.AddRetry(new RetryStrategyOptions
    {
        Delay = TimeSpan.FromSeconds(1),
        MaxRetryAttempts = 2,
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true
    });
});
```

However, we can also specify generic arguments when calling `AddResiliencePipeline`. This allows us to configure a typed resilience pipeline using `ResiliencePipelineBuilder<TResult>`. Using this approach, we can access the [hedging](https://www.pollydocs.org/strategies/hedging.html) and [fallback](https://www.pollydocs.org/strategies/fallback.html) strategies.

In the following example, we're configuring a fallback strategy by calling `AddFallback`. This allows us to provide a fallback value that we can return in case of a failure. The fallback could be a static value or come from another HTTP request or the database.

```csharp
services.AddResiliencePipeline<string, GitHubUser?>("gh-fallback", builder =>
{
    builder.AddFallback(new FallbackStrategyOptions<GitHubUser?>
    {
        FallbackAction = _ =>
            Outcome.FromResultAsValueTask<GitHubUser?>(GitHubUser.Empty)
    });
});
```

To use resilience pipelines configured with dependency injection, we can use the `ResiliencePipelineProvider`. It exposes a `GetPipeline` method for obtaining the pipeline instance. We have to provide the key used to register the resilience pipeline.

```csharp
app.MapGet("users", async (
    HttpClient httpClient,
    ResiliencePipelineProvider<string> pipelineProvider) =>
{
    ResiliencePipeline<GitHubUser?> pipeline =
        pipelineProvider.GetPipeline<GitHubUser?>("gh-fallback");

    var user = await pipeline.ExecuteAsync(async token =>
        await httpClient.GetAsync("api/users", token),
        cancellationToken);
});
```

## [Resilience Strategies and Polly](#resilience-strategies-and-polly)

[Resilience strategies](https://www.pollydocs.org/strategies/) are the core component of Polly. They're designed to run custom callbacks while introducing an additional layer of resilience. We can't run these strategies directly. Instead, we execute them through a resilience pipeline.

Polly categorizes resilience strategies into **reactive** and **proactive**. Reactive strategies handle specific exceptions or results. Proactive strategies decide to cancel or reject the execution of callbacks using a rate limiter or a timeout resilience strategy.

Polly has the following built-in resilience strategies:

*   **Retry**: The classic "try again" approach. Works great for temporary network glitches. You can configure how many retries you have and even add some randomness (jitter) to avoid overloading the system if everyone retries at once.
*   **Circuit-breaker**: Like an electrical circuit breaker, this prevents hammering a failing system. If errors pile up, the circuit breaker "trips" temporarily to give the system time to recover.
*   **Fallback**: Provides a safe, default response if your primary call fails. It might be a cached result or a simple "service unavailable" message.
*   **Hedging**: Makes multiple requests simultaneously, taking the first successful response. It is helpful if your system has numerous ways of handling something.
*   **Timeout**: Prevents requests from hanging forever by terminating them if the timeout is exceeded.
*   **Rate-limiter**: Throttles outgoing requests to prevent overwhelming external services.

## [HTTP Request Resilience](#http-request-resilience)

Sending HTTP calls to external services is how your application interacts with the outside world. These could be third-party services like payment gateways and identity providers or other services your team owns and operates.

The `Microsoft.Extensions.Http.Resilience` library comes with ready-to-use resilience pipelines for sending HTTP requests.

We can add resilience to outgoing [**HttpClient requests**](the-right-way-to-use-httpclient-in-dotnet) using the `AddStandardResilienceHandler` method.

```csharp
services.AddHttpClient<GitHubService>(static (httpClient) =>
{
    httpClient.BaseAddress = new Uri("https://api.github.com/");
})
.AddStandardResilienceHandler();
```

This also means you can eliminate any [**delegating handlers**](extending-httpclient-with-delegating-handlers-in-aspnetcore) you previously used for resilience.

The standard resilience handler combines five Polly strategies to create a resilience pipeline suitable for most scenarios. The standard pipeline contains the following strategies:

*   **Rate limiter**: Limits the maximum number of concurrent requests sent to the dependency.
*   **Total request timeout**: Introduces a total timeout, including any retry attempts.
*   **Retry**: Retries a request if it fails because of a timeout or a transient error.
*   **Circuit breaker**: Prevents sending further requests if too many failures are detected.
*   **Attempt timeout**: Introduces a timeout for an individual request.

You can customize any aspect of the standard resilience pipeline by configuring the `HttpStandardResilienceOptions`.

## [Takeaway](#takeaway)

Resilience isn't just a buzzword; it's a core principle for building reliable software systems. We're fortunate to have powerful tools like `Microsoft.Extensions.Resilience` and Polly at our disposal. We can use them to design systems that gracefully handle any transient failures.

Good [**monitoring and observability**](introduction-to-distributed-tracing-with-opentelemetry-in-dotnet) are essential to understand how your resilience mechanisms work in production. Remember, the goal isn't to eliminate failures but to gracefully handle them and keep your application functioning.

Ready to dive deeper into resilient architecture? My advanced course on [**building modular monoliths**](/modular-monolith-architecture) will equip you with the skills to design and implement robust, scalable systems. Check out [**Modular Monolith Architecture**](/modular-monolith-architecture).

**Challenge**: Take a look at your existing .NET projects. Are there any critical areas where a little resilience could go a long way? Pick one and try applying some of the techniques we've discussed here.

That's all for today.

See you next week.

---

Whenever you're ready, there are 4 ways I can help you:

1.  [Pragmatic Clean Architecture:](/pragmatic-clean-architecture) Join 4,200+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [Modular Monolith Architecture:](/modular-monolith-architecture) Join 2,100+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [(NEW) Pragmatic REST APIs:](/pragmatic-rest-apis) Join 1,100+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
4.  [Patreon Community:](https://www.patreon.com/milanjovanovic) Join a community of 5,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Become a Better .NET Software Engineer

Join 70,000+ engineers who are improving their skills every Saturday morning.

Join 70K+ Engineers

.formkit-form\[data-uid="134c4e25db"\] \*{box-sizing:border-box;}.formkit-form\[data-uid="134c4e25db"\]{-webkit-font-smoothing:antialiased;-moz-osx-font-smoothing:grayscale;}.formkit-form\[data-uid="134c4e25db"\] legend{border:none;font-size:inherit;margin-bottom:10px;padding:0;position:relative;display:table;}.formkit-form\[data-uid="134c4e25db"\] fieldset{border:0;padding:0.01em 0 0 0;margin:0;min-width:0;}.formkit-form\[data-uid="134c4e25db"\] body:not(:-moz-handler-blocked) fieldset{display:table-cell;}.formkit-form\[data-uid="134c4e25db"\] h1,.formkit-form\[data-uid="134c4e25db"\] h2,.formkit-form\[data-uid="134c4e25db"\] h3,.formkit-form\[data-uid="134c4e25db"\] h4,.formkit-form\[data-uid="134c4e25db"\] h5,.formkit-form\[data-uid="134c4e25db"\] h6{color:inherit;font-size:inherit;font-weight:inherit;}.formkit-form\[data-uid="134c4e25db"\] h2{font-size:1.5em;margin:1em 0;}.formkit-form\[data-uid="134c4e25db"\] h3{font-size:1.17em;margin:1em 0;}.formkit-form\[data-uid="134c4e25db"\] p{color:inherit;font-size:inherit;font-weight:inherit;}.formkit-form\[data-uid="134c4e25db"\] ol:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] ul:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] blockquote:not(\[template-default\]){text-align:left;}.formkit-form\[data-uid="134c4e25db"\] p:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] hr:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] blockquote:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] ol:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] ul:not(\[template-default\]){color:inherit;font-style:initial;}.formkit-form\[data-uid="134c4e25db"\] .ordered-list,.formkit-form\[data-uid="134c4e25db"\] .unordered-list{list-style-position:outside !important;padding-left:1em;}.formkit-form\[data-uid="134c4e25db"\] .list-item{padding-left:0;}.formkit-form\[data-uid="134c4e25db"\]\[data-format="modal"\]{display:none;}.formkit-form\[data-uid="134c4e25db"\]\[data-format="slide in"\]{display:none;}.formkit-form\[data-uid="134c4e25db"\]\[data-format="sticky bar"\]{display:none;}.formkit-sticky-bar .formkit-form\[data-uid="134c4e25db"\]\[data-format="sticky bar"\]{display:block;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input,.formkit-form\[data-uid="134c4e25db"\] .formkit-select,.formkit-form\[data-uid="134c4e25db"\] .formkit-checkboxes{width:100%;}.formkit-form\[data-uid="134c4e25db"\] .formkit-button,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit{border:0;border-radius:5px;color:#ffffff;cursor:pointer;display:inline-block;text-align:center;font-size:15px;font-weight:500;cursor:pointer;margin-bottom:15px;overflow:hidden;padding:0;position:relative;vertical-align:middle;}.formkit-form\[data-uid="134c4e25db"\] .formkit-button:hover,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:hover,.formkit-form\[data-uid="134c4e25db"\] .formkit-button:focus,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:focus{outline:none;}.formkit-form\[data-uid="134c4e25db"\] .formkit-button:hover > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:hover > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-button:focus > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:focus > span{background-color:rgba(0,0,0,0.1);}.formkit-form\[data-uid="134c4e25db"\] .formkit-button > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit > span{display:block;-webkit-transition:all 300ms ease-in-out;transition:all 300ms ease-in-out;padding:12px 24px;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input{background:#ffffff;font-size:15px;padding:12px;border:1px solid #e3e3e3;-webkit-flex:1 0 auto;-ms-flex:1 0 auto;flex:1 0 auto;line-height:1.4;margin:0;-webkit-transition:border-color ease-out 300ms;transition:border-color ease-out 300ms;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input:focus{outline:none;border-color:#1677be;-webkit-transition:border-color ease 300ms;transition:border-color ease 300ms;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input::-webkit-input-placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input::-moz-placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input:-ms-input-placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input::placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="dropdown"\]{position:relative;display:inline-block;width:100%;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="dropdown"\]::before{content:"";top:calc(50% - 2.5px);right:10px;position:absolute;pointer-events:none;border-color:#4f4f4f transparent transparent transparent;border-style:solid;border-width:6px 6px 0 6px;height:0;width:0;z-index:999;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="dropdown"\] select{height:auto;width:100%;cursor:pointer;color:#333333;line-height:1.4;margin-bottom:0;padding:0 6px;-webkit-appearance:none;-moz-appearance:none;appearance:none;font-size:15px;padding:12px;padding-right:25px;border:1px solid #e3e3e3;background:#ffffff;}.formkit-form\[data-uid="134c4e25db