```yaml
---
title: "How I Took My .NET 9 Minimal API From 400ms to 40ms (Without Losing My Mind) | by Prem Chandak | Sep, 2025 | Medium"
source: https://medium.com/@premchandak_11/how-i-took-my-net-9-minimal-api-from-400ms-to-40ms-without-losing-my-mind-30df1dcb887c
date_published: 2025-09-02T12:46:10.400Z
date_captured: 2025-09-11T12:17:46.257Z
domain: medium.com
author: Prem Chandak
category: backend
technologies: [.NET 9, ASP.NET Core, Minimal API, Entity Framework Core, dotnet-counters, wrk, IMemoryCache, dotnet-trace, PerfView, System.Text.Json, JsonSerializerContext]
programming_languages: [C#, SQL]
tags: [dotnet, minimal-api, performance-optimization, api-performance, async-await, database-optimization, caching, profiling, web-api, middleware]
key_concepts: [performance-profiling, asynchronous-programming, database-indexing, response-caching, middleware-optimization, load-testing, data-projection, connection-pooling]
code_examples: false
difficulty_level: intermediate
summary: |
  The article details a practical journey of optimizing a .NET 9 Minimal API endpoint, reducing its response time from 400ms to 40ms. The author outlines an eight-step process, starting with baseline measurement using tools like `dotnet-counters` and `wrk`. Key optimizations included trimming unnecessary middleware, implementing `async/await` patterns, and optimizing database access through projections, indexing, and connection pooling. The article also emphasizes the importance of smart response caching and using profiling tools like `dotnet-trace` and `PerfView` to identify bottlenecks. It concludes by stressing the iterative process of measuring, fixing, and re-measuring for effective performance improvement.
---
```

# How I Took My .NET 9 Minimal API From 400ms to 40ms (Without Losing My Mind) | by Prem Chandak | Sep, 2025 | Medium

# How I Took My .NET 9 Minimal API From 400ms to 40ms (Without Losing My Mind)

Learn how I cut .NET 9 Minimal API response times from 400ms to 40ms with profiling, async patterns, and smart optimizations.

You know that moment when you hit an API endpoint and wait… and wait… and it feels like you could make a cup of coffee before the response comes back? That was me a few weeks ago. My shiny new **.NET 9 Minimal API** project was taking **400ms** to respond to a simple GET request. For a “minimal” API, it didn’t feel minimal at all.

![Diagram showing the reduction of API response time from 400ms to 40ms by optimizing middleware and database interactions.](https://miro.medium.com/v2/resize:fit:700/0*o4O5IqeNxixXaz_W)

Fast forward some debugging, profiling, and a few late nights, and I had that same endpoint responding in just **40ms**. Let me walk you through how I got there — step by step, no magic, no over-engineering. Just practical fixes you can use today.

## Why Performance Matters

Performance isn’t just about bragging rights. A snappy API means:

*   Better **user experience** (fewer frustrated refresh clicks).
*   Lower **infrastructure costs** (fast APIs scale better).
*   Happier **developers** (debugging is faster when requests don’t crawl).

## Step 1: Start With a Baseline (Measure First)

Before fixing anything, I measured. Guessing is the fastest way to waste time.

I used **dotnet-counters** and simple logging to track response times. Here’s my initial test endpoint:

```csharp
app.MapGet("/users", async () =>  
{  
    await Task.Delay(200); // simulating DB call  
    return new[] { "Alice", "Bob", "Charlie" };  
});
```

> When I hit this with `wrk` (a load testing tool), it showed **~400ms latency**.  
> So the first lesson: **don’t optimize blind — measure.**

## Step 2: Kill the Unnecessary Middleware

Minimal APIs are lean by default, but I had layered in some extras (logging, request tracing, even CORS headers that weren’t needed for local tests).

Each middleware in the pipeline adds a few milliseconds. Multiply that under load, and it stacks up.

I trimmed my `Program.cs` to only what was required:

```csharp
var builder = WebApplication.CreateBuilder(args);  
var app = builder.Build();  
app.MapGet("/", () => "Hello World!");  
app.Run();
```

![Diagram illustrating the concept of middleware, showing how fewer layers lead to a faster flow from input to output.](https://miro.medium.com/v2/resize:fit:576/0*GiVIoOxbidjpfeHI)

> After stripping down, I already shaved **~40ms** off the response time.

## Step 3: Go Async Everywhere

Blocking calls are the silent killers of performance. In my case, a fake `Task.Delay` turned into an actual blocking DB query when connected to production.

I swapped out synchronous calls for `async/await` wherever possible:

```csharp
app.MapGet("/users", async (DbContext db) =>  
{  
    var users = await db.Users.ToListAsync();  
    return users;  
});
```

> This alone brought my endpoint closer to **250ms**.

## Step 4: Optimize Database Access

The DB was the real bottleneck. Here’s where I made three quick fixes:

1.  **Use projection instead of** `**ToListAsync()**`

```csharp
var users = await db.Users  
    .Select(u => new { u.Id, u.Name })  
    .ToListAsync();
```

No need to fetch unused columns.)

**2\. Add proper indexes**  
Queries went from ~150ms → ~30ms after indexing the right columns.

**3\. Connection pooling**  
Ensured connections were reused, not opened per request.

> At this stage, my latency was ~120ms. Progress!

## Step 5: Response Caching (Don’t Recompute Everything)

Some endpoints don’t change every second. For my “list users” API, caching made sense.

```csharp
app.MapGet("/users", async (IMemoryCache cache, DbContext db) =>  
{  
    if (!cache.TryGetValue("users", out List<User> users))  
    {  
        users = await db.Users.ToListAsync();  
        cache.Set("users", users, TimeSpan.FromSeconds(30));  
    }  
    return users;  
});
```

> Suddenly, repeat requests dropped to **40ms**.

## Step 6: Profile, Don’t Guess

One mistake I made early was guessing at performance hotspots. Tools like **dotnet-trace** and **PerfView** gave me real insights.

*   Found a hidden serialization bottleneck.
*   Switched from `System.Text.Json` default options to precompiled `JsonSerializerContext`.

> Result: a few more milliseconds shaved off.

## Step 7: Keep It Minimal (Seriously)

It’s called a **Minimal API** for a reason. Adding heavy frameworks, unnecessary filters, or bloated DI containers kills the whole point.

In fact, one of my hand-drawn notes looked like this:

```
[ Request ]  
     |  
  Middleware (5 layers?) --- X  
     |  
  DB Call (optimize async + index)  
     |  
  Response (cache if possible)
```

> Every box is a possible bottleneck. The fewer boxes, the better.

## Step 8: Load Test Again and Again

After each change, I ran `wrk` again. Seeing the latency drop from 400ms → 120ms → 80ms → 40ms kept me motivated.

## Key Takeaways (TL;DR)

*   **Measure before you optimize.** Don’t guess.
*   **Trim middleware.** Every layer costs time
*   **Async everything.** Blocking kills performance.
*   **Fix your database.** Indexes and projections matter.
*   **Cache smartly.** If data doesn’t change, don’t re-fetch.

![Two gauges, one red showing 400ms and one green showing 40ms, representing the performance improvement of a .NET 9 Minimal API.](https://miro.medium.com/v2/resize:fit:576/0*wJxMi9D8xIksKigi)

## Final Thoughts

Optimizing APIs isn’t about premature optimization or writing “clever” code. It’s about removing friction, step by step.

My .NET 9 Minimal API went from **400ms** to **40ms**, but the bigger win was learning a repeatable process: **measure, fix, measure again.**

If you’re building with .NET 9, don’t just trust that “minimal” means “fast.” With the right tweaks, you can actually make it lightning fast.