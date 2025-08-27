```yaml
---
title: Performant Compile-Time Logging in .NET
source: https://okyrylchuk.dev/blog/performant-compile-time-logging-in-dotnet/
date_published: 2024-09-20T15:48:42.000Z
date_captured: 2025-08-12T11:27:02.577Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [.NET, .NET 6, .NET 9, ILogger, LoggerMessage.Define, LoggerMessageAttribute, Microsoft.Extensions.Logging.Generators, ELK Stack, Azure Monitor]
programming_languages: [C#]
tags: [logging, dotnet, performance, compile-time, source-generators, structured-logging, iloggermessage, optimization, csharp, memory-management]
key_concepts: [structured-logging, compile-time-logging, source-generators, LoggerMessage.Define, LoggerMessageAttribute, memory-allocation-optimization, CPU-usage-optimization, dependency-injection]
code_examples: true
difficulty_level: intermediate
summary: |
  This article explains how to achieve performant, compile-time logging in .NET applications, moving beyond traditional string interpolation and basic positional logging. It highlights the inefficiencies of standard logging methods, such as boxing value types and unnecessary string formatting. The post then introduces `LoggerMessage.Define` for optimized logging and, more importantly, the `LoggerMessageAttribute` in .NET 6, which leverages source generators to automate the creation of highly efficient logging methods. This approach minimizes memory allocations, reduces CPU usage, and provides compile-time safety for log messages, ultimately enhancing application performance and maintainability.
---
```

# Performant Compile-Time Logging in .NET

# Performant Compile-Time Logging in .NET

Logging is a critical part of software development.

It is the process of recording information about an application’s execution. This information can include messages about the application’s state, performance metrics, error reports, and user activity. Logs are crucial in providing insight into software’s behavior during runtime.

## Traditional Logging

.NET has an abstraction, ILogger, representing a type to perform logging.

```csharp
public class MyService(ILogger<MyService> logger)
{
    public void PrintMessage(string message)
    {
        logger.LogInformation($"PrintMessage: {message}");

        Console.WriteLine(message);
    }
}
```

Above, you can see how to perform logging information about calling the method PrintMessage with a parameter passed into this method.

To format a message, I used string interpolation. **Don’t do that!**

As string interpolation is a great feature, you must refrain from using it in the logging.

That’s because you lose structured logging — where key-value pairs are logged in a structured way, making logs more helpful in searching, filtering, and analysis. String interpolation turns the message into a plain string, losing valuable structure.

```json
{
  "EventId": 0,
  "LogLevel": "Information",
  "Category": "MyService",
  "Message": "PrintMessage: Hello, World!",
  "State": {
    "Message": "PrintMessage: Hello, World!",
    "{OriginalFormat}": "PrintMessage: Hello, World!"
  }
}
```

A better approach is to use positional logging.

```csharp
logger.LogInformation("PrintMessage: {message}", message);
```

The log looks as follows:

```json
{
  "EventId": 0,
  "LogLevel": "Information",
  "Category": "MyService",
  "Message": "PrintMessage: Hello, World!",
  "State": {
    "Message": "PrintMessage: Hello, World!",
    "message": "Hello, World!",
    "{OriginalFormat}": "PrintMessage: {message}"
  }
}
```

Another reason to avoid string interpolation is that it generates the final string at the time of execution, even if the log message won’t be written (due to log level settings).

However, it’s also related to positional logging. A better approach is to check if the log level is enabled.

```csharp
if (logger.IsEnabled(LogLevel.Information))
    logger.LogInformation("PrintMessage: {message}", message);
```

However, positional logging has its disadvantages. Let’s take a look at the definition of the method.

```csharp
public static void LogInformation(this ILogger logger, string? message, params object?[] args)
{
    logger.Log(LogLevel.Information, message, args);
}
```

args parameters is an array of objects. It means boxing for value types.

Also, if you pass the wrong number of parameters to format the message, you’ll encounter an exception at run time.

## LoggerMessage.Define

The optimal way is to use LoggerMessage.Define method.

```csharp
private static readonly Action<ILogger, string, Exception?> _logPrintMessage =
    LoggerMessage.Define<string>(
        LogLevel.Information,
        new EventId(0),
        "PrintMessage: {message}");

public void PrintMessage(string message)
{
    _logPrintMessage(logger, message, null);

    Console.WriteLine(message);
}
```

This approach does template parsing and caches the result for the application lifetime.

The Define method forces us to pass the correct number of parameters.

Also, the Define method checks if the current log level is enabled for us.

However, the disadvantage is that it’s quite verbose.

## LoggerMessageAttribute

### Extension method approach

.NET 6 introduced the LoggerMessageAttribute. When you use it, the source generator generates the functionality of LoggerMessage.Define method for you as the extension method of the ILogger type.

Let’s see how to use it.

```csharp
public static partial class MyServiceLogging
{
    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Information,
        Message = "PrintMessage: {message}")]
    public static partial void LogPrintMessage(this ILogger logger, string message);
}
```

Logging methods must be partial and return void. If a logging method is static, the ILogger instance is required as a parameter.

When you go to the method definition, you’ll see that the source generator generates LoggerMessage.Define the method with a check if the current log level is enabled.

```csharp
public class MyService(ILogger<MyService> logger)
{
    public void PrintMessage(string message)
    {
        logger.LogPrintMessage(message);

        Console.WriteLine(message);
    }
}
```

When you go to the LogPrintMessage method definition, you’ll see that the source generator generates LoggerMessage.Define the method with a check if the current log level is enabled.

```csharp
partial class MyServiceLogging
{
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Extensions.Logging.Generators", "8.0.10.11423")]
    private static readonly global::System.Action<global::Microsoft.Extensions.Logging.ILogger, global::System.String, global::System.Exception?> __LogPrintMessageCallback =
            global::Microsoft.Extensions.Logging.LoggerMessage.Define<global::System.String>(global::Microsoft.Extensions.Logging.LogLevel.Information, new global::Microsoft.Extensions.Logging.EventId(0, nameof(LogPrintMessage)), "PrintMessage: {message}", new global::Microsoft.Extensions.Logging.LogDefineOptions() { SkipEnabledCheck = true });

    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.Extensions.Logging.Generators", "8.0.10.11423")]
    public static partial void LogPrintMessage(this global::Microsoft.Extensions.Logging.ILogger logger, global::System.String message)
    {
        if (logger.IsEnabled(global::Microsoft.Extensions.Logging.LogLevel.Information))
        {
            __LogPrintMessageCallback(logger, message, null);
        }
    }
}
```

### Normal method approach

You can generate the logging method as an extension method or as a normal one. However, the class must also be partial.

```csharp
public partial class MyService(ILogger<MyService> logger)
{
    public void PrintMessage(string message)
    {
        LogPrintMessage(logger, message);

        Console.WriteLine(message);
    }

    [LoggerMessage(
      EventId = 0,
      Level = LogLevel.Information,
      Message = "PrintMessage: {message}")]
    public static partial void LogPrintMessage(ILogger logger, string message);
}
```

### Dependency injection approach

You can also use dependency injection to create a logger method. The method must not be static, then.

```csharp
public partial class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void PrintMessage(string message)
    {
        LogPrintMessage(message);

        Console.WriteLine(message);
    }

    [LoggerMessage(
        EventId = 0,
        Level = LogLevel.Information,
        Message = "PrintMessage: {message}")]
    public partial void LogPrintMessage(string message);
}
```

In the example above, I don’t use the primary constructor. That’s because it doesn’t work with primary constructors. However, this issue is fixed [.NET 9](https://learn.microsoft.com/en-us/dotnet/core/whats-new/dotnet-9/libraries#logging-source-generator ".NET 9").

### Dynamic log level change

The LoggerMessage attribute supports a dynamic log level change.

```csharp
[LoggerMessage(
    EventId = 0,
    Message = "PrintMessage: {message}")]
public static partial void LogPrintMessage(this ILogger logger, LogLevel logLevel, string message);
```

Dynamic log levels are not compatible with LoggerMessage.Define(); however, the source generator will do its best to handle the case.

## Summary

The LoggerMessage source generator in .NET is a powerful tool for optimizing logging by generating highly efficient, boilerplate-free logging code. Here’s why you should use it:

1.  **Performance**: It minimizes memory allocations and CPU usage by deferring string formatting until it’s needed, ensuring that log messages are only created when the log level is enabled.
2.  **Compile-Time Safety**: The source generator produces logging methods at compile time, reducing the chance of runtime errors like format string mismatches.
3.  **Maintainability**: Generated methods encapsulate logging logic, making it easier to manage, refactor, and reuse across your application.
4.  **Structured Logging**: It supports structured logging and helps produce logs that are easier to query and analyze with tools like ELK Stack or Azure Monitor.

In short, it improves performance, safety, and maintainability of logging in .NET applications.

## Related Posts

[![Intro to Serialization with Source Generation in System.Text.Json](https://okyrylchuk.dev/wp-content/uploads/2024/02/featured8.png.webp)](https://okyrylchuk.dev/blog/intro-to-serialization-with-source-generation-in-system-text-json/)

### [Intro to Serialization with Source Generation in System.Text.Json](https://okyrylchuk.dev/blog/intro-to-serialization-with-source-generation-in-system-text-json/)

[.NET](https://okyrylchuk.dev/blog/category/dotnet/), [JSON](https://okyrylchuk.dev/blog/category/json/), [Source Generators](https://okyrylchuk.dev/blog/category/source-generators/), [System.Text.Json](https://okyrylchuk.dev/blog/category/system-text-json/) / February 23, 2024

[![How Easily to Fan Out HTTP Requests in .NET](https://okyrylchuk.dev/wp-content/uploads/2024/05/featured19.png.webp)](https://okyrylchuk.dev/blog/how-to-fan-out-http-requests-in-dotnet/)

### [How Easily to Fan Out HTTP Requests in .NET](https://okyrylchuk.dev/blog/how-to-fan-out-http-requests-in-dotnet/)

[.NET](https://okyrylchuk.dev/blog/category/dotnet/), [.NET 6](https://okyrylchuk.dev/blog/category/dotnet-6/), [C#](https://okyrylchuk.dev/blog/category/csharp/), [Performance](https://okyrylchuk.dev/blog/category/performance/) / May 17, 2024