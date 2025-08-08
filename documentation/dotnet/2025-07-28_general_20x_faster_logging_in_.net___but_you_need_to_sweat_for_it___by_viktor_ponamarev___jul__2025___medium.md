# 20x faster logging in .NET — but you need to sweat for it | by Viktor Ponamarev | Jul, 2025 | Medium

**Source:** https://medium.com/@vikpoca/20x-faster-logging-in-net-but-you-need-to-sweat-for-it-3ddfdf7db988
**Date Captured:** 2025-07-28T16:15:21.567Z
**Domain:** medium.com
**Author:** Viktor Ponamarev
**Category:** general

---

Member-only story

# 20x faster logging in .NET — but you need to sweat for it

[

![Viktor Ponamarev](https://miro.medium.com/v2/resize:fill:64:64/1*VFo0BfB-Xt4ihBLYfWwKGg.png)





](/@vikpoca?source=post_page---byline--3ddfdf7db988---------------------------------------)

[Viktor Ponamarev](/@vikpoca?source=post_page---byline--3ddfdf7db988---------------------------------------)

Follow

8 min read

·

Jul 17, 2025

85

4

Listen

Share

More

What if I tell you that 5–10% of CPU is wasted on logging in an average high-intensive .NET program? Not much? But can save hundred of thousands of $$$ on infrastructure. Let’s see how we can optimize.

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/1*iAesbh0chxnHtr8-2tH3KQ.png)

Traditional `ILogger.LogInformation()` calls are convenient but hide memory allocations, boxing, and message-template parsing that slow hot code paths. Two complementary techniques—`LoggerMessage.Define` and compile-time source-generated logging—eliminate most per-call overhead by caching delegates and moving boilerplate work to build time. We examine the problems, walk through code, benchmark gains, and end with production-ready design patterns.

Traditional `ILogger.LogInformation()` calls are convenient but hide memory allocations, boxing, and message-template parsing that slow hot code paths.

Two complementary techniques—`LoggerMessage.Define` and compile-time source-generated logging—eliminate most per-call overhead by caching delegates and moving boilerplate work to build time. We examine the problems, walk through code, benchmark gains, and end with production-ready design patterns.

# Anatomy of Logging Costs

## Boxing and Array Allocations

Every interpolated or template call that passes value types (`int`, `double`, `enum`, etc.) via `params object[]` forces boxing and array allocation, pushing objects onto the GC heap. The GC pauses may dwarf the log I/O itself.

## Template Parsing on Every Call

Message templates (`"Order {Id} processed"`) must be parsed to extract placeholders each time unless cached, adding CPU cycles.

## Inefficient Guarding

Calling `LogDebug` even when the effective level is Information still executes argument evaluation unless an explicit `IsEnabled` guard is used.

# Baseline Code and Its Pitfalls

// Naive approach  
\_logger.LogDebug("Retrieving weather forecast for {Days} days", days);

*   The compiler creates an `object[]` with one boxed `int`.
*   The template is parsed every call.
*   The call runs even when Debug is disabled, wasting cycles.

**Baseline Flow**

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/1*LVmI-q1Y1B_fD_ScIT5pPQ.png)

The red path shows wasted work when level is lower than the call.

## Guarding with `IsEnabled`

if (\_logger.IsEnabled(LogLevel.Debug))  
{  
    \_logger.LogDebug("Retrieving forecast for {Days} days", days);  
}

*   Eliminates work when Debug is off.
*   Adds repetitive if-clauses and still boxes when Debug is on.

# Pattern 1 — `LoggerMessage.Define` Delegates

## How It Works

`LoggerMessage.Define<T1,T2>` returns an `Action<ILogger,T1,T2,Exception?>` delegate that is cached in a static field, ensuring:

1.  Template parsing happens once at startup.
2.  Strongly typed parameters avoid boxing unless `Exception` is passed.

private static readonly Action<ILogger,int,Exception?> \_retrieveForecast =  
    LoggerMessage.Define<int\>(  
        LogLevel.Information,  
        new EventId(1001, nameof(RetrieveForecast)),  
        "Retrieving weather forecast for {Days} days");  
  
public static void RetrieveForecast(this ILogger logger, int days)  
    => \_retrieveForecast(logger, days, null);

1.  `Define<int>` tells the compiler a single `int` parameter will be substituted into the placeholder `{Days}`.
2.  The delegate is stored in `_retrieveForecast`; loading a delegate is near-zero cost compared to re-parsing templates.
3.  Extension method `RetrieveForecast` hides plumbing from call sites.

**Delegate Pattern**

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/1*CwCa4U-29mB38ZCdaHx5wg.png)

## Step-by-Step Flow

1.  **App Calls Delegate:** Instead of calling `LogDebug` directly, the application invokes the cached delegate, passing in the logger instance and any parameters (such as `days` in the weather example).
2.  **Delegate Handles Formatting:** The delegate, already set up with the message template and parameter types, formats the log entry efficiently. Since the delegate is strongly typed, value types like `int` are not boxed, and the message template is not re-parsed each time.
3.  **Provider Writes Log:** The formatted log message and its parameters are forwarded to the logging provider, which handles outputting the log (to a console, file, etc.).

## Why This Pattern Is Efficient

*   **Single Template Parsing:** The message template is parsed once, not on every log call.
*   **No Boxing:** Strongly typed parameters avoid unnecessary heap allocations.
*   **Minimal Overhead:** The only runtime cost is invoking a pre-compiled delegate, making this approach extremely fast and allocation-free.

# Pattern 2 — Compile-Time Source-Generated Logging

.NET 6 introduced `LoggerMessageAttribute`; the compiler auto-generates the delegate and extension method.

public static partial class WeatherLogging  
{  
    \[LoggerMessage(  
        EventId = 1001,  
        Level = LogLevel.Information,  
        Message = "Retrieving weather forecast for {Days} days")\]  
    public static partial void RetrieveForecast(this ILogger logger, int days);  
}

## What the Generator Emits (simplified)

private static readonly Action<ILogger,int,Exception?> \_\_RetrieveForecast =  
    LoggerMessage.Define<int\>(LogLevel.Information,  
        new EventId(1001, "RetrieveForecast"),  
        "Retrieving weather forecast for {Days} days",  
        new LogDefineOptions { SkipEnabledCheck = false });  
  
public static void RetrieveForecast(this ILogger logger, int days)  
{  
    if (logger.IsEnabled(LogLevel.Information))  
        \_\_RetrieveForecast(logger, days, null);  
}

_Same zero allocations, but no hand-written delegate._

## SkipEnabledCheck

Set `SkipEnabledCheck = true` for always-on levels (e.g., Warning+) to shave off an extra branch.

## Comparative Table

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:1000/1*gRpMx1M7M36L6Dzk5vratg.png)

# End-to-End Implementation Guide

## 1\. Create a Logging Extensions Class

namespace Contoso.Weather.Infrastructure.Logging;  
  
public static partial class WeatherLog // source gen file  
{  
    \[LoggerMessage(1000, LogLevel.Information,  
        "Forecast retrieved for {Date}")\]  
    public static partial void ForecastRetrieved(this ILogger logger, DateOnly date);  
    \[LoggerMessage(1001, LogLevel.Warning,  
        "Slow external API call took {ElapsedMs}ms", SkipEnabledCheck = true)\]  
    public static partial void ExternalApiSlow(this ILogger logger, double elapsedMs);  
}

_Every call site becomes:_

\_logger.ForecastRetrieved(requestedDate);

## 2\. Unit Test Log Content

Use `ILogger<WeatherLog>` with a TestSink to assert correct event IDs and structured properties.

## 3\. Configure Providers

builder.Logging.ClearProviders()  
       .AddConsole()  
       .AddJsonConsole()  
       .AddSerilog();

Even with Serilog or Elastic, the front-end `ILogger` code path still benefits from zero-alloc patterns.

## 4\. Avoid Common Mistakes

*   **String-Interpolation inside templates** destroys structured logging and negates caching.
*   **Anonymous objects or non-primitive structs** still incur boxing; pass primitives or flatten objects.
*   **Too many distinct templates** defeat caching; reuse messages where possible.

# Benchmark Snapshot

Let’s see benchmark results:

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/1*rjJB_bgwgbSpEF1OCFGtYg.png)

The table compares the performance of various logging and string formatting methods in .NET. Each row represents a different method, and the columns show detailed statistics about their execution speed and consistency.

*   **Method**: The approach being benchmarked (e.g., string formatting, naive logging, advanced logging).
*   **Mean**: The average time (in nanoseconds) it takes to execute the method once.
*   **Ratio**: Relative performance compared to the baseline (here, `StringFormat` and `NaiveLogDebug` are the baseline at 1.00).

## What the Results Show

**StringFormat** (`string.Format`):

*   Mean: ~21.9 ns
*   Baseline for comparison (Ratio 1.00)
*   Fast, but not as fast as optimized logging.

**StringInterpolation** (`$"..."`):

*   Mean: ~24.6 ns
*   Slightly slower than `StringFormat` (Ratio 1.12)
*   Still very fast, but incurs minor overhead from interpolation.

**NaiveLogDebug**:

*   Mean: ~21.9 ns (almost identical to `StringFormat`)
*   This is a typical logging call without any optimizations or guards.
*   It pays the cost of message formatting and parameter boxing, even if the log level is disabled.

## IsEnabledGuard:

*   Mean: ~1.25 ns
*   Adds an explicit check (`if (_logger.IsEnabled(...))`), so the log message is only constructed if needed.
*   Drastically reduces overhead (Ratio 0.06), making it over 15x faster than naive logging.

**LoggerMessageDefine**:

*   Mean: ~1.51 ns
*   Uses a cached delegate for logging, avoiding repeated parsing and allocations.
*   Nearly as fast as the `IsEnabledGuard` approach (Ratio 0.07).

**SourceGenerated**:

*   Mean: ~1.24 ns
*   Uses source-generated logging methods introduced in .NET 6+.
*   Fastest method in our benchmark (Ratio 0.06), with extremely low variance.

## Practical Implications

*   For high-performance .NET applications, **prefer source-generated logging or delegate-based logging** over naive approaches.
*   **Guard log calls with** `**IsEnabled**` if you cannot use advanced patterns, to avoid unnecessary work.
*   **Avoid string interpolation or formatting inside log calls** unless you are certain the log will always be emitted.

## Benchmark code

using System;  
using BenchmarkDotNet.Attributes;  
using BenchmarkDotNet.Running;  
using Microsoft.Extensions.Logging;  
using Microsoft.Extensions.Logging.Abstractions;  
  
\[AllStatisticsColumn\]  
public class LoggingBenchmarks  
{  
    private readonly ILogger<LoggingBenchmarks> \_logger;  
    private static readonly Action<ILogger, int, Exception?> \_defineDelegate =  
        LoggerMessage.Define<int\>(  
            LogLevel.Debug,  
            new EventId(1000, "RetrieveForecast"),  
            "Retrieving weather forecast for {Days} days");  
  
    public LoggingBenchmarks()  
    {  
        // Use a NullLogger to avoid actual I/O  
        \_logger = LoggerFactory.Create(builder => builder.AddProvider(NullLoggerProvider.Instance)).CreateLogger<LoggingBenchmarks>();  
    }  
  
    \[Benchmark(Baseline = true)\]  
    public void StringFormat()  
    {  
        \_logger.LogDebug("Retrieving weather forecast for {0} days", 5);  
    }  
  
    \[Benchmark\]  
    public void StringIterpolation()  
    {  
        \_logger.LogDebug($"Retrieving weather forecast for {5} days");  
    }  
  
    // Structured logging with parameter  
    \[Benchmark\]  
    public void NaiveLogDebug()  
    {  
        \_logger.LogDebug("Retrieving weather forecast for {Days} days", 5);  
    }  
  
    \[Benchmark\]  
    public void IsEnabledGuard()  
    {  
        if (\_logger.IsEnabled(LogLevel.Debug))  
        {  
            \_logger.LogDebug("Retrieving weather forecast for {Days} days", 5);  
        }  
    }  
  
    \[Benchmark\]  
    public void LoggerMessageDefine()  
    {  
        \_defineDelegate(\_logger, 5, null);  
    }  
  
    \[Benchmark\]  
    public void SourceGenerated()  
    {  
        LoggingExtensions.RetrieveForecast(\_logger, 5);  
    }  
}  
  
// Source-generated logging extension (requires Microsoft.Extensions.Logging.Generators)  
public static partial class LoggingExtensions  
{  
    \[LoggerMessage(  
        EventId = 1000,  
        Level = LogLevel.Debug,  
        Message = "Retrieving weather forecast for {Days} days")\]  
    public static partial void RetrieveForecast(this ILogger logger, int days);  
}  
  
public class Program  
{  
    public static void Main(string\[\] args)  
    {  
        BenchmarkRunner.Run<LoggingBenchmarks>();  
    }  
}

# Advanced Design Patterns

## Adapter per Bounded Context

Wrap domain loggers inside typed adapters so business code never touches `ILogger` directly. The adapters internally call generated helpers, keeping messages consistent and localized.

## Conditional Build Symbols

Use `#if DEBUG` to enable extra Debug/Trace helpers without muddying release builds. The unused methods are trimmed by the compiler.

## Third-Party Zero-Allocation Libraries

Libraries like [ZeroLog](https://github.com/Abc-Arbitrage/ZeroLog) go further by pooling buffers and background-formatting. They can replace or supplement Microsoft’s abstractions when nanosecond latency matters (e.g., high-frequency trading).

## End-to-End Flow with Source Generation

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/1*TXBATKKaLUhhQ_lgb8Cfww.png)

Let’s break it down:

1\. Build Phase

*   When you build your application, the C# compiler detects any logging methods marked with the `LoggerMessageAttribute`.
*   The source generator automatically creates new C# code (usually in a `.g.cs` file) that implements highly efficient logging methods for you.

2\. Compilation

*   The generated code is compiled along with the rest of your application.
*   This code includes static delegates and optimized log methods that avoid unnecessary allocations and checks.

3\. Publish and Deployment

*   When you publish your application, the generated logging code is packaged together with your app, ready for deployment.

4\. Runtime Logging Flow

*   When your application runs and a log call is made:
*   The calling code invokes the source-generated log method instead of a standard logging method.
*   The generated method first checks if the log level (e.g., Information, Warning) is enabled.
*   If the log level is not enabled, the method returns immediately — no message formatting or parameter boxing occurs, so there’s almost zero overhead.
*   If the log level is enabled, the method uses a cached delegate to efficiently format the message and pass it to the logging provider.
*   The logging provider (such as Console, Serilog, or Application Insights) then handles the actual output of the log message.

## Key Efficiency Gains

*   **No unnecessary work:** Message templates are parsed and delegates created only once at build time.
*   **Minimal runtime cost:** At runtime, only a quick check and a delegate call are performed.
*   **Zero allocations:** Value types are not boxed, and object arrays are not created unless the log is actually written.

# Conclusion

High-throughput microservices need logging that is both rich and virtually free. The combination of `LoggerMessage.Define` and compile-time source generators introduced in .NET 6 provides that sweet spot—zero allocations, cached templates, and minimal branching—without losing structured logging semantics. Adopt these patterns, benchmark frequently, and your logs will illuminate, not throttle, your production systems.