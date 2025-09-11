```yaml
---
title: "Entire Caching Strategy will be replaced by this .NET 9 Feature | by Is It Vritra - SDE I | Sep, 2025 | Medium"
source: https://isitvritra101.medium.com/entire-caching-strategy-will-be-replaced-by-this-net-9-feature-bc18c92041c8
date_published: 2025-09-06T14:17:37.289Z
date_captured: 2025-09-10T12:46:58.110Z
domain: isitvritra101.medium.com
author: Is It Vritra - SDE I
category: general
technologies: [.NET 9, HybridCache, Redis, ASP.NET Core]
programming_languages: [C#]
tags: [caching, dotnet, .net-9, performance, web-development, distributed-caching, in-memory-caching, api, optimization, data-access]
key_concepts: [hybrid-caching, in-memory-caching, distributed-caching, performance-optimization, database-load-reduction, simplified-api, cache-invalidation]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces .NET 9's new HybridCache feature, designed to simplify and enhance caching strategies in applications. It highlights the complexities of traditional caching with Redis, such as extensive configuration and manual serialization, contrasting it with HybridCache's "one-liner" implementation. HybridCache operates with a two-layer approach, combining fast in-app memory and shared distributed memory, significantly boosting API performance by up to 18x. The author provides practical use cases and demonstrates how to integrate HybridCache into an ASP.NET Core application for improved responsiveness and reduced database load.
---
```

# Entire Caching Strategy will be replaced by this .NET 9 Feature | by Is It Vritra - SDE I | Sep, 2025 | Medium

![.NET 9 HybridCache: The new unified caching API that combines in-memory and distributed caching.](https://miro.medium.com/v2/resize:fit:1000/0*-r1sGMIsMhDJnHRW)

# **Entire Caching Strategy will be replaced by this** .NET 9 Feature

## Hybrid cache in production

You knew that I worked with the Portfolio investment project, right? So, every time a user opens their dashboard, our app hits the database to fetch stock prices, calculate portfolio values, and display transaction history.

With 10,000 users checking their portfolios multiple times a day, your database is in cry-mode like babies, and your cloud bill LOL...

![Three icons depicting a browser window interacting with a database, symbolizing data access.](https://miro.medium.com/v2/resize:fit:700/1*9R4PF-FgDBJNiN87AyqEVQ.png)

And that’s why we do **Caching!** And I know everybody uses that in a project. uhmm… We keep frequently used files on our desk instead of walking to the filing cabinet every time!? _right_

_We kept that after a req and all other 99 reqs. will hit the Cache, not the database_

But there is a glitch! Traditional caching meant workaround with Redis servers, managing complex configurations, and writing a lot of code lines just to store and retrieve data. But thats where the .NET 9 comes into the act..

## Performance Score

We were working on something that need us to recheck our Cache implementation and that time we found something, something very interesting and we ended up with our API performance by **18x times** — from 1,370 requests per second to 25,798 RPS.

> Same code, same server, just one change.

The change was switching to the **.Net9’s** new caching approach LOL,

## Meet HybridCache

![Text overlay "Hello HybridCache!" on a subtle gradient background.](https://miro.medium.com/v2/resize:fit:700/1*-C9Z1kKMRYtOtBm5UaIoPg.png)

idk but Microsoft introduce this quitly… something called **HybridCache** that makes Redis setup look like using a flip phone in 2024.

This is how traditional Cache works basically —

*   Set up a Redis server
*   Write 20+ lines of configuration
*   Handle serialization/deserialization
*   Manage cache invalidation
*   Debug connection issues (on loop)

And **HybridCache** is like this — _Literally one line_

```csharp
var portfolioValue = await _cache.GetOrCreateAsync($"portfolio_{userId}", GetPortfolioValue);
```

That’s it!

## How This Actually Works :)

Think of **HybridCache** as having two layers of memory —

![Diagram illustrating Hybrid Cache with two boxes: "Fast Memory (in your app)" and "Shared memory (across servers)".](https://miro.medium.com/v2/resize:fit:700/1*H4YMCOH225KJFesC05gNXQ.png)

1.  **Fast memory** (in your app) — like your desk drawer
2.  **Shared memory** (across servers) — like a shared filing cabinet

When your user opens their portfolio:

*   **First check:** Is it in the desk drawer? Grab it instantly, ZAP! (0.001ms)
*   **Second check**: Is it in the shared cabinet? Get it quickly.. (5ms)
*   **Last resort**: Calculate from database (200ms)

> But why it’s one liner — because it handles everything automatically.. c’mon it’s **2025!**

_now,_ **_last but not least_**

## What This Means for You?

![An icon of a person with a browser window and code tags for a head, symbolizing a developer or an application.](https://miro.medium.com/v2/resize:fit:700/1*4pZLPTr67U4e5HGP2xeRDw.jpeg)

I would say in my application it is, **Perfect for:**

*   User dashboards and profiles
*   Stock prices and market data
*   Portfolio calculations
*   API responses that change hourly/daily
*   Financial reports and analytics

And, **Not ideal for:**

*   Real-time trading data (changes every second)
*   Live chat messages
*   Payment processing (never cache sensitive operations)
*   Data that must be 100% fresh always

and this is a one-liner for you —

> _“If your users won’t mind seeing 5-minute-old data, cache it. If they’re making million-dollar trades, don’t cache it”_

Here’s how to add this —

### Program.cs

```csharp
builder.Services.AddHybridCache();
```

**Controller**

```csharp
public class PortfolioController : ControllerBase  
{  
    private readonly HybridCache _cache;  
      
    public PortfolioController(HybridCache cache)  
    {  
        _cache = cache;  
    }  
      
    [HttpGet]  
    public async Task<PortfolioData> GetPortfolio(int userId)  
    {  
        return await _cache.GetOrCreateAsync(  
            $"portfolio_{userId}",  
            async _ => await CalculatePortfolioValue(userId),  
            TimeSpan.FromMinutes(10)  
        );  
    }  
}
```

_That’s literally it…_🖤