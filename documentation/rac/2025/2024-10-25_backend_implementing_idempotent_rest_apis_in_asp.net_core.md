```yaml
---
title: Implementing Idempotent REST APIs in ASP.NET Core
source: https://www.milanjovanovic.tech/blog/implementing-idempotent-rest-apis-in-aspnetcore
date_published: 2024-10-26T00:00:00.000Z
date_captured: 2025-09-09T13:46:01.111Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: backend
technologies: [ASP.NET Core, HTTP, IDistributedCache, Redis, .NET, Postman, BoldSign]
programming_languages: [C#]
tags: [idempotency, rest-api, aspnet-core, http, caching, distributed-systems, web-api, csharp, middleware, best-practices]
key_concepts: [idempotency, rest-api-design, http-methods, idempotency-keys, distributed-caching, action-filters, endpoint-filters, distributed-locking]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article delves into implementing idempotent REST APIs in ASP.NET Core to ensure reliability and consistency in distributed systems. It clarifies the concept of idempotency, distinguishing between inherently idempotent HTTP methods like GET, PUT, and DELETE, and non-idempotent ones like POST. The core implementation strategy involves using unique idempotency keys, with server-side caching of responses to prevent duplicate processing. Practical C# code examples are provided for both MVC controllers using `IAsyncActionFilter` and Minimal APIs with `IEndpointFilter`, demonstrating how to store and retrieve results from a distributed cache. The piece concludes with best practices, including cache duration, concurrency considerations, and handling cases where idempotency keys are reused with different request bodies.]
---
```

# Implementing Idempotent REST APIs in ASP.NET Core

![Blog cover: 'Idempotency ASP.NET Core, RESTful APIs' with the MJ Tech logo.](/blog-covers/mnw_113.png?imwidth=3840)

# Implementing Idempotent REST APIs in ASP.NET Core

7 min read · October 26, 2024

Looking for a developer-friendly, affordable eSignature solution? [**BoldSign**](https://boldsign.com/?utm_source=milanjovanovic&utm_medium=newsletter_ad&utm_campaign=boldsign) offers highly responsive and secure API integration for your .NET applications with transparent pricing—making it the perfect [**alternative to DocuSign**](https://boldsign.com/docusign-alternative/?utm_source=milanjovanovic&utm_medium=newsletter_ad&utm_campaign=boldsign). Start your free trial today.

Transform your team's API development. See how [**Postman Flows**](https://app.zuddl.com/p/a/event/b718e16b-9456-4da3-9af1-f990b252ceb5?utm_source=influencer&utm_medium=Newsletter&utm_campaign=Flows_collab&utm_term=Milan_Jovanovic&utm_content=Webinar) eliminates bottlenecks & enables seamless collaboration. Join the finale of our "Visualize, Accelerate, & Collaborate" series to supercharge your workflow. [**Register now**](https://app.zuddl.com/p/a/event/b718e16b-9456-4da3-9af1-f990b252ceb5?utm_source=influencer&utm_medium=Newsletter&utm_campaign=Flows_collab&utm_term=Milan_Jovanovic&utm_content=Webinar).

[Sponsor this newsletter](/sponsor-the-newsletter)

Idempotency is a crucial concept for REST APIs that ensures the reliability and consistency of your system. An idempotent operation can be repeated multiple times without changing the result beyond the initial API request. This property is especially important in distributed systems, where network failures or timeouts can lead to repeated requests.

Implementing idempotency in your API brings several benefits:

*   It prevents unintended duplicate operations
*   It improves reliability in distributed systems
*   It helps handle network issues and retries gracefully

In this week's issue, we'll explore how to implement idempotency in ASP.NET Core APIs, ensuring your system remains robust and reliable.

## What is Idempotence?

Idempotence, in the context of web APIs, means that making multiple identical requests should have the same effect as making a single request. In other words, no matter how many times a client sends the same request, the server-side effect should only occur once.

The [RFC 9110](https://www.rfc-editor.org/rfc/rfc9110) standard about HTTP Semantics offers a definition we could use. Here's what it says about **idempotent methods**:

> A request method is considered "idempotent" if the intended effect on the server of multiple identical requests with that method is the same as the effect for a single such request.
> 
> Of the request methods defined by this specification, PUT, DELETE, and safe request methods \[(GET, HEAD, OPTIONS, and TRACE) - author's note\] are idempotent.

_— [RFC 9110 (HTTP Semantics), Section 9.2.2, Paragraph 1](https://www.rfc-editor.org/rfc/rfc9110#section-9.2.2-1)_

However, the following paragraph is quite interesting. It clarifies that the server can implement "other non-idempotent side effects" that don't apply to the resource.

> ... the idempotent property only applies to what has been requested by the user; a server is free to log each request separately, retain a revision control history, or implement other non-idempotent side effects for each idempotent request.

_— [RFC 9110 (HTTP Semantics), Section 9.2.2, Paragraph 2](https://www.rfc-editor.org/rfc/rfc9110#section-9.2.2-2)_

The benefits of implementing idempotency extend beyond just adhering to HTTP method semantics. It significantly improves the reliability of your API, especially in distributed systems where network issues can lead to retried requests. By implementing idempotency, you prevent duplicate operations that could occur due to client retries.

## Which HTTP Methods are Idempotent?

Several HTTP methods are inherently idempotent:

*   `GET`, `HEAD`: Retrieve data without modifying the server state.
*   `PUT`: Update a resource, resulting in the same state regardless of repetition.
*   `DELETE`: Remove a resource with the same outcome for multiple requests.
*   `OPTIONS`: Retrieve communication options information.

`POST` is not inherently idempotent, as it typically creates resources or processes data. Repeated `POST` requests could create multiple resources or trigger multiple actions.

However, we can implement idempotency for `POST` methods using custom logic.

**Note**: While `POST` requests aren't naturally idempotent, we can design them to be. For example, checking for existing resources before creation ensures that repeated `POST` requests don't result in duplicate actions or resources.

## Implementing Idempotency in ASP.NET Core

To implement idempotency, we'll use a strategy involving idempotency keys:

1.  The client generates a unique key for each operation and sends it in a custom header.
2.  The server checks if it has seen this key before:
    *   For a new key, process the request and store the result.
    *   For a known key, return the stored result without reprocessing.

This ensures that retried requests (e.g., due to network issues) are processed only once on the server.

We can implement idempotency for controllers by combining an `Attribute` and `IAsyncActionFilter`. Now, we can specify the `IdempotentAttribute` to apply idempotency to a controller endpoint.

**Note**: When a request fails (returns 4xx/5xx), we don't cache the response. This allows clients to retry with the same idempotency key. However, this means a failed request followed by a successful one with the same key will succeed - make sure this aligns with your business requirements.

```csharp
[AttributeUsage(AttributeTargets.Method)]
internal sealed class IdempotentAttribute : Attribute, IAsyncActionFilter
{
    private const int DefaultCacheTimeInMinutes = 60;
    private readonly TimeSpan _cacheDuration;

    public IdempotentAttribute(int cacheTimeInMinutes = DefaultCacheInMinutes)
    {
        _cacheDuration = TimeSpan.FromMinutes(cacheTimeInMinutes);
    }

    public async Task OnActionExecutionAsync(
        ActionExecutingContext context,
        ActionExecutionDelegate next)
    {
        // Parse the Idempotence-Key header from the request
        if (!context.HttpContext.Request.Headers.TryGetValue(
                "Idempotence-Key",
                out StringValues idempotenceKeyValue) ||
            !Guid.TryParse(idempotenceKeyValue, out Guid idempotenceKey))
        {
            context.Result = new BadRequestObjectResult("Invalid or missing Idempotence-Key header");
            return;
        }

        IDistributedCache cache = context.HttpContext
            .RequestServices.GetRequiredService<IDistributedCache>();

        // Check if we already processed this request and return a cached response (if it exists)
        string cacheKey = $"Idempotent_{idempotenceKey}";
        string? cachedResult = await cache.GetStringAsync(cacheKey);
        if (cachedResult is not null)
        {
            IdempotentResponse response = JsonSerializer.Deserialize<IdempotentResponse>(cachedResult)!;

            var result = new ObjectResult(response.Value) { StatusCode = response.StatusCode };
            context.Result = result;

            return;
        }

        // Execute the request and cache the response for the specified duration
        ActionExecutedContext executedContext = await next();

        if (executedContext.Result is ObjectResult { StatusCode: >= 200 and < 300 } objectResult)
        {
            int statusCode = objectResult.StatusCode ?? StatusCodes.Status200OK;
            IdempotentResponse response = new(statusCode, objectResult.Value);

            await cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(response),
                new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = _cacheDuration }
            );
        }
    }
}

internal sealed class IdempotentResponse
{
    [JsonConstructor]
    public IdempotentResponse(int statusCode, object? value)
    {
        StatusCode = statusCode;
        Value = value;
    }

    public int StatusCode { get; }
    public object? Value { get; }
}
```

**Note**: There's a small [**race condition**](solving-race-conditions-with-ef-core-optimistic-locking) window between checking and setting the cache. For absolute consistency, we should consider using a distributed lock pattern, though this adds complexity and latency.

Now, we can apply this attribute to our controller actions:

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpPost]
    [Idempotent(cacheTimeInMinutes: 60)]
    public IActionResult CreateOrder([FromBody] CreateOrderRequest request)
    {
        // Process the order...

        return CreatedAtAction(nameof(GetOrder), new { id = orderDto.Id }, orderDto);
    }
}
```

**Idempotency with Minimal APIs**

To implement idempotency with Minimal APIs, we can use an `IEndpointFilter`.

```csharp
internal sealed class IdempotencyFilter(int cacheTimeInMinutes = 60)
    : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        // Parse the Idempotence-Key header from the request
        if (TryGetIdempotenceKey(out Guid idempotenceKey)) // This line seems incomplete, assuming it's a placeholder for actual TryGetValue logic.
        {
            return Results.BadRequest("Invalid or missing Idempotence-Key header");
        }

        IDistributedCache cache = context.HttpContext
            .RequestServices.GetRequiredService<IDistributedCache>();

        // Check if we already processed this request and return a cached response (if it exists)
        string cacheKey = $"Idempotent_{idempotencyKey}";
        string? cachedResult = await cache.GetStringAsync(cacheKey);
        if (cachedResult is not null)
        {
            IdempotentResponse response = JsonSerializer.Deserialize<IdempotentResponse>(cachedResult)!;
            return new IdempotentResult(response.StatusCode, response.Value);
        }

        object? result = await next(context);

        // Execute the request and cache the response for the specified duration
        if (result is IStatusCodeHttpResult { StatusCode: >= 200 and < 300 } statusCodeResult
            and IValueHttpResult valueResult)
        {
            int statusCode = statusCodeResult.StatusCode ?? StatusCodes.Status200OK;
            IdempotentResponse response = new(statusCode, valueResult.Value);

            await cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(response),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(cacheTimeInMinutes)
                }
            );
        }

        return result;
    }
}

// We have to implement a custom result to write the status code
internal sealed class IdempotentResult : IResult
{
    private readonly int _statusCode;
    private readonly object? _value;

    public IdempotentResult(int statusCode, object? value)
    {
        _statusCode = statusCode;
        _value = value;
    }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        httpContext.Response.StatusCode = _statusCode;

        return httpContext.Response.WriteAsJsonAsync(_value);
    }
}
```

Now, we can apply this endpoint filter to our Minimal API endpoint:

```csharp
app.MapPost("/api/orders", CreateOrder)
    .RequireAuthorization()
    .WithOpenApi()
    .AddEndpointFilter<IdempotencyFilter>();
```

An alternative to the previous two implementations is implementing idempotency logic in a custom middleware.

## Best Practices and Considerations

Here are the key things I always keep in mind when implementing idempotency.

Cache duration is tricky. I aim to cover reasonable retry windows without holding onto stale data. A reasonable cache time typically ranges from a few minutes to 24-48 hours, depending on your specific use case.

Concurrency can be a pain, especially in high-traffic APIs. A thread-safe implementation using a distributed lock works great. It keeps things in check when multiple requests hit at once. But this should be a rare occurrence.

For distributed setups, Redis is my go-to. It's perfect as a shared cache, keeping idempotency consistent across all your API instances. Plus, it handles distributed locking.

What if a client reuses an idempotency key with a different request body? I return an error in this case. My approach is to hash the request body and store it with the idempotency key. When a request comes in, I compare the request body hashes. If they differ, I return an error. This prevents misuse of idempotency keys and maintains the integrity of your API.

## Summary

Implementing idempotency in REST APIs enhances service reliability and consistency. It ensures identical requests yield the same result, preventing unintended duplicates and gracefully handling network issues.

While our implementation provides a foundation, I recommend adapting it to your needs. Focus on critical operations in your APIs, especially those that modify the system state or trigger important business processes.

By embracing idempotency, you're building more robust and user-friendly APIs.

That's all for today.

See you next week.

---

Whenever you're ready, there are 4 ways I can help you:

1.  [Pragmatic Clean Architecture:](/pragmatic-clean-architecture) Join 4,200+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [Modular Monolith Architecture:](/modular-monolith-architecture) Join 2,100+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [(NEW) Pragmatic REST APIs:](/pragmatic-rest-apis) Join 1,200+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
4.  [Patreon Community:](https://www.patreon.com/milanjovanovic) Join a community of 5,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Become a Better .NET Software Engineer

Join 72,000+ engineers who are improving their skills every Saturday morning.

Join 72K+ Engineers

.formkit-form\[data-uid="134c4e25db"\] \*{box-sizing:border-box;}.formkit-form\[data-uid="134c4e25db"\]{-webkit-font-smoothing:antialiased;-moz-osx-font-smoothing:grayscale;}.formkit-form\[data-uid="134c4e25db"\] legend{border:none;font-size:inherit;margin-bottom:10px;padding:0;position:relative;display:table;}.formkit-form\[data-uid="134c4e25db"\] fieldset{border:0;padding:0.01em 0 0 0;margin:0;min-width:0;}.formkit-form\[data-uid="134c4e25db"\] body:not(:-moz-handler-blocked) fieldset{display:table-cell;}.formkit-form\[data-uid="134c4e25db"\] h1,.formkit-form\[data-uid="134c4e25db"\] h2,.formkit-form\[data-uid="134c4e25db"\] h3,.formkit-form\[data-uid="134c4e25db"\] h4,.formkit-form\[data-uid="134c4e25db"\] h5,.formkit-form\[data-uid="134c4e25db"\] h6{color:inherit;font-size:inherit;font-weight:inherit;}.formkit-form\[data-uid="134c4e25db"\] h2{font-size:1.5em;margin:1em 0;}.formkit-form\[data-uid="134c4e25db"\] h3{font-size:1.17em;margin:1em 0;}.formkit-form\[data-uid="134c4e25db"\] p{color:inherit;font-size:inherit;font-weight:inherit;}.formkit-form\[data-uid="134c4e25db"\] ol:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] ul:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] blockquote:not(\[template-default\]){text-align:left;}.formkit-form\[data-uid="134c4e25db"\] p:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] hr:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] blockquote:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] ol:not(\[template-default\]),.formkit-form\[data-uid="134c4e25db"\] ul:not(\[template-default\]){color:inherit;font-style:initial;}.formkit-form\[data-uid="134c4e25db"\] .ordered-list,.formkit-form\[data-uid="134c4e25db"\] .unordered-list{list-style-position:outside !important;padding-left:1em;}.formkit-form\[data-uid="134c4e25db"\] .list-item{padding-left:0;}.formkit-form\[data-uid="134c4e25db"\]\[data-format="modal"\]{display:none;}.formkit-form\[data-uid="134c4e25db"\]\[data-format="slide in"\]{display:none;}.formkit-form\[data-uid="134c4e25db"\]\[data-format="sticky bar"\]{display:none;}.formkit-sticky-bar .formkit-form\[data-uid="134c4e25db"\]\[data-format="sticky bar"\]{display:block;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input,.formkit-form\[data-uid="134c4e25db"\] .formkit-select,.formkit-form\[data-uid="134c4e25db"\] .formkit-checkboxes{width:100%;}.formkit-form\[data-uid="134c4e25db"\] .formkit-button,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit{border:0;border-radius:5px;color:#ffffff;cursor:pointer;display:inline-block;text-align:center;font-size:15px;font-weight:500;cursor:pointer;margin-bottom:15px;overflow:hidden;padding:0;position:relative;vertical-align:middle;}.formkit-form\[data-uid="134c4e25db"\] .formkit-button:hover,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:hover,.formkit-form\[data-uid="134c4e25db"\] .formkit-button:focus,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:focus{outline:none;}.formkit-form\[data-uid="134c4e25db"\] .formkit-button:hover > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:hover > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-button:focus > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit:focus > span{background-color:rgba(0,0,0,0.1);}.formkit-form\[data-uid="134c4e25db"\] .formkit-button > span,.formkit-form\[data-uid="134c4e25db"\] .formkit-submit > span{display:block;-webkit-transition:all 300ms ease-in-out;transition:all 300ms ease-in-out;padding:12px 24px;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input{background:#ffffff;font-size:15px;padding:12px;border:1px solid #e3e3e3;-webkit-flex:1 0 auto;-ms-flex:1 0 auto;flex:1 0 auto;line-height:1.4;margin:0;-webkit-transition:border-color ease-out 300ms;transition:border-color ease-out 300ms;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input:focus{outline:none;border-color:#1677be;-webkit-transition:border-color ease 300ms;transition:border-color ease 300ms;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input::-webkit-input-placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input::-moz-placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input:-ms-input-placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] .formkit-input::placeholder{color:inherit;opacity:0.8;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="dropdown"\]{position:relative;display:inline-block;width:100%;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="dropdown"\]::before{content:"";top:calc(50% - 2.5px);right:10px;position:absolute;pointer-events:none;border-color:#4f4f4f transparent transparent transparent;border-style:solid;border-width:6px 6px 0 6px;height:0;width:0;z-index:999;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="dropdown"\] select{height:auto;width:100%;cursor:pointer;color:#333333;line-height:1.4;margin-bottom:0;padding:0 6px;-webkit-appearance:none;-moz-appearance:none;appearance:none;font-size:15px;padding:12px;padding-right:25px;border:1px solid #e3e3e3;background:#ffffff;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="dropdown"\] select:focus{outline:none;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\]{text-align:left;margin:0;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\]{margin-bottom:10px;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] \*{cursor:pointer;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\]:last-of-type{margin-bottom:0;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] input\[type="checkbox"\]{display:none;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] input\[type="checkbox"\] + label::after{content:none;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] input\[type="checkbox"\]:checked + label::after{border-color:#ffffff;content:"";}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] input\[type="checkbox"\]:checked + label::before{background:#10bf7a;border-color:#10bf7a;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] label{position:relative;display:inline-block;padding-left:28px;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] label::before,.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] label::after{position:absolute;content:"";display:inline-block;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] label::before{height:16px;width:16px;border:1px solid #e3e3e3;background:#ffffff;left:0px;top:3px;}.formkit-form\[data-uid="134c4e25db"\] \[data-group="checkboxes"\] \[data-group="checkbox"\] label::after{height:4px;width:8px;border-left:2px solid #4d4d4d;border-bottom:2px solid #4d4d4d;-webkit-transform:rotate(-45deg);-ms-transform:rotate(-45deg);transform:rotate(-45deg);left:4px;top:8px;}.formkit-form\[data-uid="134c4e25db"\] .formkit-alert{background:#f9fafb;border:1px solid #e3e3e3;border-radius:5px;-webkit-flex:1 0 auto;-ms-flex:1 0 auto;flex:1 0 auto;list-style:none;margin:25px auto;padding:12px;text-align:center;width