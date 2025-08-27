```yaml
---
title: "IOptions vs Snapshot vs Monitor in .NET (Explained) | Medium"
source: https://medium.com/@alexbel83/ioptions-vs-ioptionssnapshot-vs-ioptionsmonitor-dotnet-6f2305b09770
date_published: 2025-08-31T23:33:12.273Z
date_captured: 2025-09-04T11:42:58.703Z
domain: medium.com
author: Aliaksandr Marozka
category: ai_ml
technologies: [.NET, ASP.NET Core, appsettings.json, Azure App Configuration, KeyVault, KeyPerFile, Dependency Injection, IHostedService, Minimal API]
programming_languages: [C#, JSON]
tags: [dotnet, configuration, options-pattern, dependency-injection, aspnet-core, appsettings, live-updates, config-management, csharp, web-api]
key_concepts: [Options Pattern, Configuration Management, Dependency Injection Scopes, Singleton Lifetime, Scoped Lifetime, Live Configuration Updates, Named Options, Configuration Validation]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article clarifies the distinctions between `IOptions<T>`, `IOptionsSnapshot<T>`, and `IOptionsMonitor<T>` in .NET for managing application configuration. It details each interface's lifetime, update behavior, and ideal use cases, such as `IOptions<T>` for static values, `IOptionsSnapshot<T>` for per-request updates in web apps, and `IOptionsMonitor<T>` for immediate, live configuration changes in long-lived services. The post also covers advanced topics like named options, validation, post-configuration, and common pitfalls, providing practical code examples and a decision guide to help developers choose the correct API for their specific needs.]
---
```

# IOptions vs Snapshot vs Monitor in .NET (Explained) | Medium

# IOptions vs IOptionsSnapshot vs IOptionsMonitor in .NET — What’s the Difference?

![Illustration showing three diverging roads, each leading to a different .NET Options API (IOptions, IOptionsSnapshot, IOptionsMonitor) with associated concepts like Singleton, Scoped, Live, and appsettings.json.](https://miro.medium.com/v2/resize:fit:700/1*44XHIUBXA5VbbYvo1QykuA.jpeg)

_Are you sure your app reloads config without a restart? Spoiler: it depends on_ **_which Options API_** _you inject._ In one of my production services, a single wrong injection (`IOptions<T>` instead of `IOptionsMonitor<T>`) silently ignored live config updates for hours. The feature flag changed, the pods kept old values, and a rollout stalled. If you’ve ever been puzzled by why your app “sometimes” picks up **appsettings.json** edits and “sometimes” doesn’t — this post is for you.

We’ll compare `**IOptions<T>**`, `**IOptionsSnapshot<T>**`, and `**IOptionsMonitor<T>**` with real code, common pitfalls, and practical rules you can drop into your project **today**.

🔗 **Quick plug:** I share weekly .NET tips on [https://amarozka.dev](https://amarozka.dev). If this helped, you’ll love the newsletter.

## When to Use Which

`**IOptions<T>**`

*   **Lifetime/Scope:** Singleton-like value cached for the entire app lifetime.
*   **Gets New Values:** **No** — the value is fixed after the first resolve.
*   **Change Notifications:** **No**.
*   **Typical Use:** Simple apps, constants, or values that never change at runtime.

`**IOptionsSnapshot<T>**`

*   **Lifetime/Scope:** **Scoped** — a new value for each DI scope (in ASP.NET Core, per HTTP request).
*   **Gets New Values:** **Yes, per scope** (e.g., you see updates on the **next** web request).
*   **Change Notifications:** **No**.
*   **Typical Use:** Controllers/handlers that should see the latest config on the next request.

`**IOptionsMonitor<T>**`

*   **Lifetime/Scope:** **Singleton**.
*   **Gets New Values:** **Yes** — updates **immediately** when configuration changes.
*   **Change Notifications:** **Yes** via `OnChange`.
*   **Typical Use:** Background services, long‑lived singletons, caches, connection pools.

> _Rule:_ **_Singletons →_** `**_IOptionsMonitor<T>_**`_,_ **_Scoped web code →_** `**_IOptionsSnapshot<T>_**`_,_ **_“Never changes” →_** `**_IOptions<T>_**`_._

![Table summarizing .NET Options API at a glance, comparing IOptions, IOptionsSnapshot, and IOptionsMonitor based on whether they provide options, notify on changes, and their lifetime (Singleton or Scoped).](https://miro.medium.com/v2/resize:fit:700/1*u6kbeI9aS3sWAwhH_BBFwg.jpeg)

## The Options Pattern in 90 Seconds

The Options pattern binds configuration (files, env vars, KeyVault, etc.) into a **strongly‑typed class** and registers it in DI.

```csharp
public sealed class MyFeatureOptions  
{  
    public bool Enabled { get; set; }  
    public int MaxItems { get; set; } = 10;  
}  
  
// Program.cs  
builder.Services  
    .AddOptions<MyFeatureOptions>()  
    .Bind(builder.Configuration.GetSection("MyFeature"))  
    .ValidateDataAnnotations()        // if you add attributes  
    .Validate(o => o.MaxItems > 0, "MaxItems must be positive")  
    .ValidateOnStart();               // fail fast on startup
```

How you **consume** `MyFeatureOptions` is the whole story:

*   `IOptions<MyFeatureOptions>` — one snapshot for the whole app lifetime.
*   `IOptionsSnapshot<MyFeatureOptions>` — a fresh snapshot **per scope** (in web apps: per request).
*   `IOptionsMonitor<MyFeatureOptions>` — a live, thread‑safe singleton that updates when configuration changes and can **notify** you.

## Configuration That Actually Reloads

Hot reload only happens if your **providers** support it (e.g., JSON file with `reloadOnChange: true`, Azure App Configuration with refresh, KeyPerFile, etc.). Environment variables are **not** watched by default.

```csharp
// Program.cs (ASP.NET Core)  
builder.Configuration  
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)  
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)  
    .AddEnvironmentVariables();
```

`IOptionsSnapshot<T>` will see new values **on the next scope**. `IOptionsMonitor<T>` sees them **immediately** and can trigger callbacks.

## Demo Setup

**appsettings.json**

```json
{  
  "MyFeature": {  
    "Enabled": true,  
    "MaxItems": 25  
  }  
}
```

**Program.cs**

```csharp
var builder = WebApplication.CreateBuilder(args);  
  
builder.Configuration  
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)  
    .AddEnvironmentVariables();  
builder.Services.AddOptions<MyFeatureOptions>()  
       .Bind(builder.Configuration.GetSection("MyFeature"))  
       .Validate(o => o.MaxItems > 0, "MaxItems must be positive");  
builder.Services.AddControllers();  
var app = builder.Build();  
app.MapControllers();  
app.Run();
```

## `IOptions<T>` — Fixed After First Resolve

`IOptions<T>` is the simplest and safest when the value **never changes** while the app runs. Once the container creates the value (lazily on first use), it’s **cached** for the lifetime of the app.

```csharp
public sealed class StaticBannerService  
{  
    private readonly string _banner;  
    public StaticBannerService(IOptions<MyFeatureOptions> options)  
    {  
        // Value is frozen for the entire app lifetime  
        _banner = options.Value.Enabled ? "Feature ON" : "Feature OFF";  
    }  
    public string GetBanner() => _banner;  
}
```

**Pitfall:** editing **appsettings.json** won’t affect this service until you **restart**.

**Use it when**:

*   Config is treated as a **constant** after start.
*   You value **zero** overhead and no reloading.

## `IOptionsSnapshot<T>` — Fresh Per Request (Web)

`IOptionsSnapshot<T>` is **scoped**. In ASP.NET Core, a DI scope is created **per HTTP request**, so each request gets a fresh copy bound from the **current** configuration.

```csharp
[ApiController]  
[Route("api/[controller]")]  
public sealed class ItemsController : ControllerBase  
{  
    private readonly IOptionsSnapshot<MyFeatureOptions> _opts;  
    public ItemsController(IOptionsSnapshot<MyFeatureOptions> opts) => _opts = opts;  
  
    [HttpGet("limit")]  
    public int GetLimitForThisRequest() => _opts.Value.MaxItems;  
}
```

Change `MyFeature:MaxItems` in **appsettings.json**, save the file, send **another** request — you’ll see the new value.

**Pitfalls & notes:**

*   Works great in **web** apps. In a plain console/worker, you usually don’t create scopes per “operation,” so `IOptionsSnapshot<T>` won’t naturally refresh.
*   Don’t inject `IOptionsSnapshot<T>` into **singletons** (it’s a scoped service). The container will prevent it.

**Use it when**:

*   You want controllers/handlers to pick up new config **on the next request**.
*   You don’t need live notifications.

## `IOptionsMonitor<T>` — Live, Notified, Singleton‑Friendly

`IOptionsMonitor<T>` is a **singleton** that updates its `CurrentValue` when the underlying config changes and can notify you via `OnChange`.

```csharp
public sealed class CacheWarmupService : IHostedService  
{  
    private readonly IOptionsMonitor<MyFeatureOptions> _monitor;  
    private IDisposable? _subscription;  
  
    public CacheWarmupService(IOptionsMonitor<MyFeatureOptions> monitor)  
        => _monitor = monitor;  
  
    public Task StartAsync(CancellationToken ct)  
    {  
        WarmUp(_monitor.CurrentValue);  
        _subscription = _monitor.OnChange(WarmUp);  
        return Task.CompletedTask;  
    }  
  
    public Task StopAsync(CancellationToken ct)  
    {  
        _subscription?.Dispose();  
        return Task.CompletedTask;  
    }  
  
    private void WarmUp(MyFeatureOptions opts)  
    {  
        // react instantly to changes  
        if (opts.Enabled)  
        {  
            // refresh caches, refill a pool, etc.  
        }  
        else  
        {  
            // pause background work, etc.  
        }  
    }  
}
```

**Pitfalls & notes:**

*   Always **dispose** the subscription (e.g., in `StopAsync`) to avoid leaks.
*   Register `OnChange` in **singletons** only. Subscribing inside scoped objects (per request) creates many dangling callbacks.
*   `OnChange` may be triggered **more than once** per write depending on the provider — design for idempotence.

**Use it when**:

*   Long‑lived services must react **immediately** to config edits.
*   You need a single source of truth for config across the process.

## Named Options — Multiple Configs of the Same Type

You can register **named options** to reuse the same type with different sources/sections.

```csharp
public sealed class PaymentOptions  
{  
    public string ApiKey { get; set; } = string.Empty;  
    public Uri Endpoint { get; set; } = new("https://example");  
}  
  
// Program.cs  
builder.Services.AddOptions<PaymentOptions>("Stripe")  
    .Bind(builder.Configuration.GetSection("Payments:Stripe"));  
  
builder.Services.AddOptions<PaymentOptions>("PayPal")  
    .Bind(builder.Configuration.GetSection("Payments:PayPal"));
```

Consume with either `IOptionsSnapshot<PaymentOptions>` or `IOptionsMonitor<PaymentOptions>`:

```csharp
public sealed class PaymentClientFactory  
{  
    private readonly IOptionsMonitor<PaymentOptions> _monitor;  
    public PaymentClientFactory(IOptionsMonitor<PaymentOptions> monitor) => _monitor = monitor;  
  
    public PaymentClient Create(string name)  
    {  
        var opts = _monitor.Get(name); // or snapshot.Get(name)  
        return new PaymentClient(opts.ApiKey, opts.Endpoint);  
    }  
}
```

## Validation: Fail Fast and Keep It Safe

```csharp
builder.Services  
    .AddOptions<MyFeatureOptions>()  
    .Bind(builder.Configuration.GetSection("MyFeature"))  
    .ValidateDataAnnotations()  
    .Validate(o => o.MaxItems <= 100, "MaxItems must be ≤ 100")  
    .ValidateOnStart(); // throws during startup if invalid
```

*   `**ValidateOnStart()**` ensures the app won’t boot with bad config.
*   For named options, use `Validate` overloads that receive the **name**.
*   For complex rules, implement `IValidateOptions<T>`.

## Post‑Configuration (Derive Values)

Need to derive or normalize values after binding? Use `PostConfigure` or `PostConfigureAll`.

```csharp
builder.Services.PostConfigure<MyFeatureOptions>(o =>  
{  
    if (o.MaxItems == 0) o.MaxItems = 10; // defaulting  
});
```

Order matters: `Bind` → `Validate` → `PostConfigure` runs after binding and before consumers read the value.

## Advanced: Under the Hood (Quick Tour)

*   `IOptionsFactory<T>` creates option instances using configured `IConfigureOptions<T>` and `IPostConfigureOptions<T>`.
*   `IOptionsMonitor<T>` keeps an `IOptionsMonitorCache<T>` and listens to `IChangeToken` from providers. It updates the cached value and fires `OnChange`.
*   `IOptionsSnapshot<T>` caches per **scope** using the factory each time a scope is created.
*   `IOptions<T>` asks the factory **once** and never again.

You rarely need to touch these, but knowing the flow helps debug “why didn’t my value update?”

## Common Pitfalls (and How I Avoid Them)

1.  **Using** `**IOptions<T>**` **in a singleton expecting live updates.**  
    Use `IOptionsMonitor<T>` instead. `IOptions<T>` won’t reload.
2.  **Subscribing to** `**OnChange**` **in per‑request services.**  
    This leaks handlers. Subscribe in a singleton and **dispose** on shutdown.
3.  **Assuming env vars auto‑reload.**  
    They don’t. Prefer a provider that supports reload (JSON with `reloadOnChange`, Azure App Configuration, etc.).
4.  `**IOptionsSnapshot<T>**` **in non‑web workers.**  
    Without scopes, it won’t refresh. Either create explicit scopes around operations or use `IOptionsMonitor<T>`.
5.  **Forgetting to validate.**  
    Add `ValidateOnStart()` to catch bad values early.
6.  **Not making actions idempotent.**  
    `OnChange` can fire multiple times; guard updates with checks.
7.  **Large object graphs on every request.**  
    With `IOptionsSnapshot<T>`, very heavy options can increase per‑request cost. If values change rarely, consider `IOptionsMonitor<T>` and cache derived results.

## Decision Guide (Cheat‑Sheet)

*   **Web controller/service** that should see config on the **next request** → `IOptionsSnapshot<T>`.
*   **HostedService / singleton** reacting **immediately** → `IOptionsMonitor<T>` (+ `OnChange`).
*   **Simple constants** (no need to reload) → `IOptions<T>`.
*   **Multiple variants of the same type** → **Named options** with `Get(name)`.
*   **Need to fail fast** → add `ValidateOnStart()`.

![Decision tree flowchart for choosing the correct .NET Options API, starting with "Options need to be reloaded?" and branching to IOptions, IOptionsMonitor<T>, or IOptionsSnapshot<T>.](https://miro.medium.com/v2/resize:fit:700/1*5OzaLNsQI8biGd0kOKUQrw.jpeg)

## Example: Mixing All Three in One App

```csharp
public sealed class FeatureDecider  
{  
    private readonly IOptionsSnapshot<MyFeatureOptions> _snapshot; // per request  
    private readonly IOptionsMonitor<MyFeatureOptions> _monitor;   // for immediate checks/logging  
    private readonly IOptions<MyFeatureOptions> _static;           // baseline defaults  
  
    public FeatureDecider(        IOptionsSnapshot<MyFeatureOptions> snapshot,  
        IOptionsMonitor<MyFeatureOptions> monitor,  
        IOptions<MyFeatureOptions> @static)  
    {  
        _snapshot = snapshot;  
        _monitor = monitor;  
        _static = @static;  
    }  
  
    public (bool now, bool nextRequest, bool atStartup) Inspect()  
        => (_monitor.CurrentValue.Enabled,  
            _snapshot.Value.Enabled,  
            _static.Value.Enabled);  
}
```

This is contrived, but it shows the **three perspectives**: current live, next‑scope, and startup.

## Minimal API Quick Demo

```csharp
var builder = WebApplication.CreateBuilder(args);  
  
builder.Configuration.AddJsonFile("appsettings.json", false, true);  
  
builder.Services.AddOptions<MyFeatureOptions>()  
    .Bind(builder.Configuration.GetSection("MyFeature"))  
    .ValidateOnStart();  
  
var app = builder.Build();  
  
app.MapGet("/monitor", (IOptionsMonitor<MyFeatureOptions> m) => m.CurrentValue);  
app.MapGet("/snapshot", (IOptionsSnapshot<MyFeatureOptions> s) => s.Value);  
app.MapGet("/static", (IOptions<MyFeatureOptions> o) => o.Value);  
  
app.Run();
```

Run, change the JSON, hit `/monitor` → **immediate** change, `/snapshot` → **next** request after save, `/static` → **no change** until restart.

## Performance Notes

*   `IOptions<T>`: negligible overhead; created once.
*   `IOptionsSnapshot<T>`: created per scope; cost proportional to configurators/binding work.
*   `IOptionsMonitor<T>`: one instance, cheap reads; `OnChange` callbacks run on thread pool.

For most apps, the difference is tiny. Choose based on **semantics**, not micro‑benchmarks.

## Choose Semantics, Not Hype

**IOptions vs Snapshot vs Monitor — pick by _how and when_ your code should see change.** If the value is effectively a **constant**, `IOptions<T>` keeps things simple. If you’re in **web** code and want changes **on the next request**, use `IOptionsSnapshot<T>`. If you need **immediate** updates and **notifications**, reach for `IOptionsMonitor<T>` — and subscribe responsibly.

👉 For more .NET architecture guides and production checklists, visit [amarozka.dev](https://amarozka.dev).

Your turn: which one have you misused before, and what happened? Drop a comment — I read them all and love war stories.