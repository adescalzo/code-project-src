```yaml
---
title: High-Performance Logging in .NET 9
source: https://goatreview.com/high-performance-logging-dotnet/
date_published: 2025-02-24T09:00:35.000Z
date_captured: 2025-08-08T18:05:54.649Z
domain: goatreview.com
author: Unknown
category: performance
technologies: [.NET 9, Microsoft.Extensions.Logging, Serilog, NLog, Microsoft.Extensions.Logging.Abstractions, BenchmarkDotNet, .NET Aspire, ASP.NET Core, GitHub]
programming_languages: [C#]
tags: [logging, dotnet, performance, structured-logging, benchmarks, .net-9, optimization, source-generation, application-monitoring, debugging]
key_concepts: [high-performance-logging, structured-logging, string-interpolation, source-generation, benchmarkdotnet, ilogger, garbage-collection, boxing, log-aggregation]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a detailed analysis of high-performance logging in .NET 9, comparing three main approaches: string interpolation, structured logging, and a new compiler-generated method using `LoggerMessage`. It presents benchmark results using BenchmarkDotNet, clearly demonstrating the superior performance and reduced memory allocations of the high-performance logging extensions. The content explains the underlying mechanisms, such as source generation and the elimination of boxing operations, that contribute to these performance gains. Furthermore, it highlights the benefits of structured logging for enhanced log analysis and integration with tools like .NET Aspire, offering practical guidance for choosing the optimal logging strategy for various production scenarios.
---
```

# High-Performance Logging in .NET 9

# High-Performance Logging in .NET 9

Discover how to optimize .NET logging performance using string interpolation, structured logging, and high-performance extensions. Learn implementation strategies and best practices for production systems.

In the .NET development world, logging is a fundamental practice for application monitoring and debugging. With .NET 9, we have several approaches to implement logging, each with its own characteristics in terms of performance and usability.

In this article, we'll explore in detail the three main logging methods and their rendering in visualization tools like .NET Aspire.

## Different Logging Approaches

When it comes to logging in .NET applications, Microsoft's built-in logging framework provides comprehensive functionality without the need for third-party solutions.

The `ILogger` interface, part of the `Microsoft.Extensions.Logging` namespace, serves as the foundation for all logging operations in the .NET ecosystem. While alternatives like Serilog or NLog exist, the default implementation offers robust features that satisfy most application requirements.

The first approach, and probably the most intuitive, is string interpolation:

```csharp
_logger.LogInformation($"Goat number {i} logged in at {_loginTime}");
```

The second method uses structured logging with templates:

```csharp
_logger.LogInformation("Goat number {i} logged in at {LoginTime}", i, _loginTime);
```

And finally, the new approach introduced with LoggingExtensions:

```csharp
_logger.UserLoggedIn(i, _loginTime);

// Defined as:
internal static partial class HighPerformanceLoggingExtensions
{
    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, Message = "Goat number {Id} logged in at {LoginTime}")]
    public static partial void UserLoggedIn(this ILogger logger, int id, DateTime loginTime);
}
```

To leverage the `LoggerMessage` attribute, you'll need to install the `Microsoft.Extensions.Logging.Abstractions` NuGet package. This attribute enables source generation during compilation, which is crucial for the high-performance characteristics of this approach.

## Performance Analysis

Let's compare these approaches using a benchmark produced with BenchmarkDotNet:

```csharp
[Benchmark]
  public void InterpolatedLogging()
  {
      for (int i = 0; i < 1000; i++)
          _logger.LogInformation($"Goat number {i} logged in at {_loginTime}");
  }

  [Benchmark]
  public void StructuredLogging()
  {
      for (int i = 0; i < 1000; i++)
          _logger.LogInformation("Goat number {i} logged in at {LoginTime}", i, _loginTime);
  }

  [Benchmark]
  public void HighPerformanceLogging()
  {
      for (int i = 0; i < 1000; i++)
          _logger.UserLoggedIn(i, _loginTime);
  }
```

The benchmark results are revealing:

```
BenchmarkDotNet v0.14.0, Windows 11 (10.0.26100.3194)
13th Gen Intel Core i9-13900H, 1 CPU, 20 logical and 14 physical cores
.NET SDK 9.0.101
  [Host]   : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2
  .NET 9.0 : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX2

Job=.NET 9.0  Runtime=.NET 9.0

| Method                 | Mean        | Error       | StdDev      | Allocated | Alloc Ratio |
|----------------------- |------------:|------------:|------------:|----------:|------------:|
| InterpolatedLogging    | 74,690.3 ns | 1,399.59 ns | 2,890.40 ns |  120000 B |        1.00 |
| StructuredLogging      | 31,211.9 ns |   547.98 ns |   586.33 ns |   88000 B |        0.73 |
| HighPerformanceLogging |    896.1 ns |    11.07 ns |    10.35 ns |         - |        0.00 |
```

### Under The Hood of High Performance

What makes high-performance logging special? Let's examine the generated code:

```csharp
internal static class HighPerformanceLoggingExtension
{
    [GeneratedCode("Microsoft.Extensions.Logging.Generators", "9.0.12.6610")]
    private static readonly Action<ILogger, int, DateTime, Exception?> __UserLoggedInCallback = 
        LoggerMessage.Define<int, DateTime>(
            LogLevel.Information,
            new EventId(1000, "UserLoggedIn"),
            "Goat number {Id} logged in at {LoginTime}",
            new LogDefineOptions() { SkipEnabledCheck = true }
        );

    [LoggerMessage(EventId = 1000, Level = LogLevel.Information, 
                  Message = "Goat number {Id} logged in at {LoginTime}")]
    [GeneratedCode("Microsoft.Extensions.Logging.Generators", "9.0.12.6610")]
    public static void UserLoggedIn(this ILogger logger, int id, DateTime loginTime)
    {
        if (!logger.IsEnabled(LogLevel.Information))
            return;
        HighPerformanceLoggingExtension.__UserLoggedInCallback(logger, id, loginTime, null);
    }
}
```

The source-generated code reveals the elegance of high-performance logging through its optimization techniques. At its core lies a pre-compiled delegate that handles logging operations efficiently, with built-in template validation and parameter handling that avoids boxing operations entirely.

Interpolated string logging, while intuitive, creates new allocations with each call. Every log message requires string formatting operations, temporary object creation, and value type boxing - all of which consume memory and processing time. The runtime must manage these allocations continuously, leading to additional garbage collection overhead.

Structured logging improves efficiency by reusing message templates, but still requires object arrays and boxing operations for each log call. Though it reduces memory usage compared to interpolation (88KB vs 120KB in our benchmarks), it retains some allocation overhead that impacts performance in high-throughput scenarios.

The high-performance approach leverages compile-time generation to eliminate these runtime costs. By generating specialized methods that handle logging without allocations, it achieves optimal performance while maintaining all the benefits of structured logging.

## The Power of Structured Logging

While performance is important, the real value of structured logging lies in its ability to make logs actionable and analyzable. Let's see the difference:

```json
// interpolated log
{
    "message": "Goat number 42 logged in at 2024-02-21T10:15:30"
}

vs
// structured log
{
    "message": "Goat number {id} logged in at {loginTime}",
    "loginTime": "2024-02-21T10:15:30",
}
```

With structured logs, you can:

1.  Search for specific goat IDs without text parsing
2.  Analyze login patterns by time ranges
3.  Group events by type
4.  Create metrics and alerts based on specific properties

The high-performance logging approach maintains these benefits while eliminating the performance overhead. The generated code ensures that logs remain structured while optimizing the generation process.

To see how it's really used, take a look at the Github solution, which shows a simple example of an ASP.NET Core application triggering logs with .NET Aspire to monitor them:

[GitHub - goatreview/BlogArticles at high-performance-logging](https://github.com/goatreview/BlogArticles/tree/high-performance-logging?ref=goatreview.com)

You can see in the log details that the structured log contains one more line corresponding to the parameter value. This makes it easier to filter and search logs, particularly for variables, which is not possible with a character-only string, as is the case with interpolated logs.

![Screenshot showing log details in .NET Aspire, comparing an interpolated log (left, showing only a message string) with a structured log (right, showing message template and separate fields for parameters like 'loginTime').](https://goatreview.com/content/images/2025/02/Sans-titre.png)

### Conclusion

When choosing a logging approach, consider your application's needs: use string interpolation for quick prototypes, structured logging for better log analysis, and high-performance logging for production systems where every nanosecond counts. The compiler-generated approach not only eliminates allocations but also provides type safety and better performance without sacrificing log quality.

For production applications, structured logging should be the minimum standard, as it enables effective log aggregation and analysis. When performance is critical, especially in high-throughput scenarios, the high-performance extensions provide an excellent balance of usability and efficiency.

To go further:

[High-performance logging - .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/high-performance-logging?wt.mc_id=MVP_388247&ref=goatreview.com)

Have a goat day 🐐

---

![A futuristic mountain goat standing on digital streams, representing high-performance concepts.](https://goatreview.com/content/images/size/w300/2025/02/pierrebelin_A_futuristic_mountain_goat_standing_on_digital_stre_46c6a471-64c8-4652-bbbe-7dc49d440e66.webp)
![GitHub repository link card for 'goatreview/BlogArticles'](https://opengraph.githubassets.com/2f02d0ca43161ddd74f6fe5508f89a57c6f633db3ffc340eb633f53387cd1abb/goatreview/BlogArticles)
![Microsoft Most Valuable Professional (MVP) badge.](https://goatreview.com/assets/icons/mvpbadge.png)