# Implement Rate Limiting in ASP.NET Core

**Source:** https://www.nikolatech.net/blogs/rate-limiting-asp-dotnet-core

---

[![Home](/_next/image?url=%2F_next%2Fstatic%2Fmedia%2FLogo.6b98f122.png&w=96&q=75)Nikola Knezevic](/)

[Blogs](/blogs)

[Sponsorship](/sponsorship)

---

Subscribe

{"@context":"https://schema.org","@type":"BlogPosting","headline":"Implement Rate Limiting in ASP.NET Core","description":"With ASP.NET Core&apos;s built-in rate limiting middleware, implementing these protections has never been easier.","image":"https://coekcx.github.io/BlogImages/banners/rate-limiting-asp-dotnet-core-banner.png","datePublished":"2025-07-23T23:02:43+00:00","author":{"@type":"Person","name":"Nikola Knez","url":"https://nikolatech.net"},"publisher":{"@type":"Organization","name":"NikolaTech","logo":{"@type":"ImageObject","url":"https://www.nikolatech.net/\_next/image?url=%2F\_next%2Fstatic%2Fmedia%2FLogo.6b98f122.png&amp;w=96&amp;q=75"}}}

# In this article

[

Intro

](#Intro)[

Rate Limiting

](#RateLimiting)[

Getting Started

](#GettingStarted)[

Fixed Window Limiter

](#FixedWindowLimiter)[

Sliding Window Limiter

](#SlidingWindowLimiter)[

Token Bucket Limiter

](#TokenBucketLimiter)[

Concurrency Limiter

](#ConcurrencyLimiter)[

Applying Rate Limiting

](#ApplyingRateLimiting)[

Conclusion

](#Conclusion)

![Banner](https://coekcx.github.io/BlogImages/banners/rate-limiting-asp-dotnet-core-banner.png)

#### Implement Rate Limiting in ASP.NET Core

###### 23 Jul 2025

###### 5 min

[

Sponsor Newsletter



](/sponsorship)

 

Too many requests sent to a server or API in a short time can cause issues.

Without a mechanism to control traffic, systems risk being overwhelmed, leading to degraded performance or outages.

Restricting the number of requests a particular endpoint can handle within a specific time frame helps maintain system stability, ensuring fair usage and protecting against abuse like brute-force attacks or resource exhaustion.

## Rate Limiting in ASP.NET Core

Starting with .NET 7, you can use the rate limiting middlewares from **Microsoft.AspNetCore.RateLimiting**.

It allows applications to configure rate limiting policies and then attach the policies to endpoints.

This middleware supports 4 different rate limiting algorithms:

*   **Fixed window**
*   **Sliding window**
*   **Token bucket**
*   **Concurrency**

Additionally, you can configure these rate limits to be applied:

*   Globally
*   On specific endpoints
*   By user identity
*   By IP address

You can even combine multiple policies together.

**NOTE:** While rate limiting can help mitigate the risk of DoS attacks by limiting the rate at which requests are processed, it's not a best solution for DDoS attacks. DDoS attacks involve multiple systems overwhelming an app making it difficult to handle with rate limiting alone. For robust DDoS protection consider using a commercial DDoS protection service.

## Getting Started

You don't need to install anything manually, rate limiting support is built into the ASP.NET Core.

The first step is to register the rate limiter middleware using **AddRateLimiter**.

csharp

```
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRateLimiter(options =>
{
    // Here you would implement policies
});

var app = builder.Build();

app.UseRateLimiter();

app.Run();
```

You can define one or more rate limiting policies, we'll go over each of them in detail later.

Once the rate limiter is configured, you must register the middleware with the request pipeline using **UseRateLimiter**.

**NOTE:** It must be placed before endpoint definitions.

## Fixed Window Limiter

One of the simplest and most commonly used rate limiting strategies is the **Fixed Window** limiter.

It limits the number of requests within a defined, fixed time window. Once the window expires, the counter resets and the next time window begins.

To apply rate limiting using a fixed time window use the **AddFixedWindowLimiter** method.

csharp

```
builder.Services.AddRateLimiter(_ =>
    _.AddFixedWindowLimiter(policyName: "fixed", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2; 
    }));
```

Inside the method we defined:

*   **PermitLimit** - Limits number of request per window
*   **Window** - Window expires in 12 seconds
*   **QueueProcessingOrder** - Requests that exceeded the limit can be queued in FIFO or LIFO order.
*   **QueueLimit** - Number of requests that can wait, rest will be rejected.

## Sliding Window Limiter

The **Sliding Window** limiter is basically an upgrade compared to the fixed window strategy.

Instead of resetting the request counter abruptly at the end of a time window, it spreads the limit more evenly over time.

This approach makes smoother traffic control compared to Fixed window limiter.

csharp

```
builder.Services.AddRateLimiter(_ =>
    _.AddSlidingWindowLimiter(policyName: "sliding", options =>
    {
        options.PermitLimit = 4;
        options.Window = TimeSpan.FromSeconds(12);
        options.SegmentsPerWindow = 2;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 2;
    }));
```

Inside the method we defined:

*   **PermitLimit** - Limits number of request per window
*   **Window** - Window expires in 12 seconds
*   **SegmentsPerWindow** - The window is divided into two 6-second segments.
*   **QueueProcessingOrder** - Requests that exceeded the limit can be queued in FIFO or LIFO order.
*   **QueueLimit** - Number of requests that can wait, rest will be rejected.

## Token Bucket Limiter

The **Token Bucket** limiter is similar to the Sliding Window limiter, but rather than adding back the requests taken from the expired segment, a fixed number of tokens are added each replenishment period.

The tokens added each segment can't increase the available tokens to a number higher than the token bucket limit.

csharp

```
builder.Services.AddRateLimiter(_ =>
    _.AddTokenBucketLimiter(policyName: "token", options =>
    {
        options.TokenLimit = 12;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 4;
        options.ReplenishmentPeriod = TimeSpan.FromSeconds(5);
        options.TokensPerPeriod = 2;
        options.AutoReplenishment = true;
    }));
```

Inside the method we defined:

*   **TokenLimit** - The bucket can hold a maximum of 12 tokens at any time.
*   **TokensPerPeriod** - Every 5 seconds, 2 tokens are added to the bucket.
*   **ReplenishmentPeriod** - Controls how frequently tokens are added.
*   **QueueLimit** - If the bucket is empty, up to 4 requests can wait in the queue.
*   **QueueProcessingOrder** - Requests that exceeded the limit can be queued in FIFO or LIFO order.
*   **AutoReplenishment** - Tokens are replenished automatically by the system.

## Concurrency Limiter

The **Concurrency** limiter limits the number of concurrent requests.

Each request reduces the concurrency limit by one. When a request completes, the limit is increased by one.

Unlike the other request limiters that limit the total number of requests for a specified period, the Concurrency limiter limits only the number of concurrent requests.

csharp

```
builder.Services.AddRateLimiter(_ =>
    _.AddConcurrencyLimiter(policyName: "concurrency", options =>
    {
        options.PermitLimit = 5;
        options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        options.QueueLimit = 3;
    }));
```

Inside the method we defined:

*   **PermitLimit** - Only 5 requests are allowed to run concurrently.
*   **QueueLimit** - If all permits are in use, up to 3 more requests will wait in a queue.
*   **QueueProcessingOrder** - Requests that exceeded the limit can be queued in FIFO or LIFO order.

## Applying Rate Limiting

After exploring each rate limiting strategy, the next step is to apply them to your endpoints. In .NET Core, you can apply rate limiting globally, per controller, or per endpoint.

### Global Rate Limiting

To apply rate limiting globally to all controllers, you can use the **RequireRateLimiting** extension method when mapping controllers in your endpoint configuration. For example, to apply the **fixed** rate limiting policy to all controller endpoints:

csharp

```
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers().RequireRateLimiting("fixed");
});
```

This configuration ensures that every controller endpoint in your application is protected by the fixed rate limiting policy.

**NOTE:** This approach applies the rate limiting policy only to controller endpoints mapped with **MapControllers**. Other endpoints, such as minimal APIs or Razor Pages, will not be affected unless you explicitly apply rate limiting to them.

### Minimal APIs

For minimal APIs, you can apply rate limiting using the **EnableRateLimiting** and **DisableRateLimiting** extension methods.

These can be applied to individual endpoints, or to groups of endpoints using **RouteGroupBuilder** (introduced in .NET 7+), which allows you to organize and configure multiple endpoints together.

Here's how to apply rate limiting to individual minimal API endpoints:

csharp

```
app.MapGet("/api/limited", () => "Rate limited endpoint")
    .EnableRateLimiting("fixed");

app.MapGet("/api/unlimited", () => "No rate limit")
    .DisableRateLimiting();
```

If you want to apply rate limiting to a group of minimal API endpoints (for example, all endpoints under a certain route prefix), you can use **MapGroup** to create a route group and then call **EnableRateLimiting** or **DisableRateLimiting** on the group.

This is useful for applying policies to several endpoints at once without repeating yourself.

csharp

```
var limitedGroup = app.MapGroup("/api/limited-group")
    .EnableRateLimiting("fixed");

limitedGroup.MapGet("/one", () => "Limited endpoint 1");
limitedGroup.MapGet("/two", () => "Limited endpoint 2");

// You can also disable rate limiting for a group:
var unlimitedGroup = app.MapGroup("/api/unlimited-group")
    .DisableRateLimiting();

unlimitedGroup.MapGet("/one", () => "Unlimited endpoint 1");
unlimitedGroup.MapGet("/two", () => "Unlimited endpoint 2");
```

This approach makes it easy to manage rate limiting policies for multiple minimal API endpoints in a clean and maintainable way.

### Controller-based APIs

For controller-based APIs, you can apply rate limiting at the controller or action level using attributes:

csharp

```
[ApiController]
[Route("[controller]")]
[EnableRateLimiting("fixed")] // Apply to all actions in the controller
public class WeatherForecastController : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        return Ok("Rate limited endpoint");
    }

    [HttpGet("unlimited")]
    [DisableRateLimiting] // Disable for this specific action
    public IActionResult GetUnlimited()
    {
        return Ok("No rate limit");
    }
}
```

You can also apply different rate limiting policies to different actions within the same controller:

csharp

```
[HttpGet("sliding")]
[EnableRateLimiting("sliding")]
public IActionResult GetWithSlidingWindow()
{
    return Ok("Sliding window rate limited");
}
```

## Conclusion

Rate limiting is a crucial aspect of building resilient and secure applications.

With ASP.NET Core's built-in rate limiting middleware, implementing these protections has never been easier.

While rate limiting is an essential security measure, remember that it's just one piece of the puzzle. For comprehensive protection against DDoS attacks, you should consider additional security measures and possibly a commercial DDoS protection service.

If you want to check out examples I created, you can find the source code here:

[Source Code](https://www.nikolatech.net/codes/rate-limiting-examples)

I hope you enjoyed it, subscribe and get a notification when a new blog is up!

# Subscribe

###### Stay tuned for valuable insights every Thursday morning.

##### Share This Article: