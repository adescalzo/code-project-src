```yaml
---
title: Logging Best Practices in ASP.NET Core
source: https://antondevtips.com/blog/logging-best-practices-in-asp-net-core
date_published: 2024-09-03T11:00:05.383Z
date_captured: 2025-08-06T17:28:18.182Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [ASP.NET Core, Serilog, Microsoft.Extensions.Logging, Nuget, Serilog.Sinks.Console, Serilog.Sinks.File, Serilog.Sinks.Async, Serilog.Sinks.Seq, Seq, OpenTelemetry, Quartz, ELK Stack, Elasticsearch, Logstash, Kibana, Datadog, New Relic, Loggly, GrayLog, Azure Monitor Logs, Amazon CloudWatch Logs, Docker]
programming_languages: [C#, JSON, Bash]
tags: [logging, aspnet-core, serilog, best-practices, structured-logging, error-logging, log-management, dotnet, performance, monitoring]
key_concepts: [structured-logging, logging-levels, logging-filters, log-rotation, log-retention, centralized-logging, sensitive-data-handling, asynchronous-logging]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to implementing logging best practices in ASP.NET Core applications, strongly recommending Serilog for its structured logging capabilities and extensive sink ecosystem. It covers essential practices such as utilizing appropriate logging levels, implementing logging filters to manage log volume, and leveraging structured logging for easier analysis. The post also emphasizes the critical importance of avoiding sensitive information in logs and effectively logging errors with contextual details. Finally, it discusses managing log size and performance through log rotation, retention, and asynchronous logging, and advocates for centralizing logs with tools like Seq for enhanced monitoring and visualization.]
---
```

# Logging Best Practices in ASP.NET Core

![A dark blue and purple banner image with a white code icon and text "dev tips" on the left, and "LOGGING BEST PRACTISES IN ASP.NET CORE" on the right.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Farchitecture%2Fcover_architecture_logging_best_practises.png&w=3840&q=100)

# Logging Best Practices in ASP.NET Core

Sep 3, 2024

8 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Logging is an essential aspect of any application, especially in a production environment. Logging provides crucial insights into the behavior of your application, helping to diagnose issues, track the flow of execution, and monitor performance.

In this blog post, I will share with you my experience on what are the best practices for implementing logging in ASP.NET Core applications.

## Best Practise 1: Use Serilog Library for Logging

ASP.NET Core has a built-in logging provider - `Microsoft.Extensions.Logging`. While it is a good option, it lacks some important features.

So I recommend using Serilog as a logging library that is very performant and supports structured logging. It adds on top of `Microsoft.Extensions.Logging` package, and you don't need to add a Serilog package to all your projects.

Serilog has a big ecosystem of sinks, and its flexibility in configuration makes it an excellent choice for logging. Sink is a source where you can output and store your logs, it can be a console, file, database, or a monitoring system.

To get started with Serilog in an ASP.NET Core application, install the following Nuget packages:

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Sinks.Console
dotnet add package Serilog.Sinks.File
```

Then add the following configuration to your `appsettings.json` file to configure logging to Console and File:

```json
{
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.File"
    ],
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      { "Name": "File", "Args": { "path": "service.log", "rollingInterval": "Day" } }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ],
    "Properties": {
      "Application": "ApplicationName"
    }
  }
}
```

The final step is to register Serilog to work on top of Microsoft Logging:

```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog((context, loggerConfig) =>
    loggerConfig.ReadFrom.Configuration(context.Configuration)
);
```

You can use `ILogger` from the `Serilog` namespace, or you can continue using `ILogger` from the `Microsoft.Extensions.Logging` namespace and Serilog will handle logging in both cases.

You can find the full list of supported Sinks [in the Serilog GitHub page](https://github.com/serilog/serilog/wiki/Provided-Sinks).

## Best Practise 2: Use The Appropriate Logging Level

When logging, you need to use various types of log levels depending on each message's importance:

*   **Trace:** logs that contain the most detailed messages. These messages may contain sensitive application data. They are disabled by default and should be used sparingly.
*   **Debug:** logs that are used for interactive investigation during development. These should primarily be enabled during development and testing.
*   **Information:** logs that track the general flow of the application. These logs should have long-term value.
*   **Warning:** logs that highlight an unexpected event in the application flow but do not cause the application to stop.
*   **Error:** logs that highlight when the current flow of execution is stopped due to a failure. These should indicate a failure in the current activity or request.
*   **Critical:** logs that describe an unrecoverable application or system crash, or a catastrophic failure that requires immediate attention.

By default, in most of the applications, the default logging level should be set to `Info` or `Warning`. Make sure to log the most important information for your application. Enable `Debug` and `Trace` in production when you need extra information or need to investigate any issues.

## Best Practise 3: Use Logging Filters

Logs can be huge, that can take from gigabytes to terabytes of space. That's why you need to log only the important information.

I recommend using logging filters in Serilog to control what is logged. With log filters, you can specify a minimum logging level for each logging namespace, for example:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information",
        "OpenTelemetry": "Debug",
        "Quartz": "Information",
        "Microsoft.AspNetCore.Mvc": "Warning",
        "Microsoft.AspNetCore.Routing": "Warning",
        "Microsoft.AspNetCore.HttpLogging.HttpLoggingMiddleware": "Information"
      }
    },
    "WriteTo": [
      ...
    ]
  }
}
```

Here I am setting minimum log levels for standard asp.net core loggers, for Open Telemetry and Quartz. All logs that have log level lower than the specified - won't be logged. For example, I will log only `Warnings`, `Errors` and `Critical` messages for `Microsoft.AspNetCore.Mvc` while `Information`, `Debug` and `Trace` logs will be skipped.

## Best Practise 4: Use Structured Logging

Serilog allows you to log structured data (key-value pairs) instead of plain text, making it easier to query and analyze logs.

```csharp
logger.LogInformation("Shipment for order '{OrderId}' is already created", request.OrderId);

logger.LogInformation("Created shipment: {@Shipment}", shipment);
```

The log message "Shipment for order '{OrderId}' is already created" includes the `OrderId` as a structured property. Instead of embedding the `OrderId` directly in the log message as plain text, it is passed as a named parameter. This allows logging systems to capture `OrderId` as a separate, searchable field.

The log message "Created shipment: {@Shipment}" uses the @ notation to serialize the shipment object into a structured format. This means that all the properties of the shipment object are logged as separate fields, preserving the structure and making it easier to analyze.

Please never use string interpolation when logging, or you will end up with plain-text logs that are not searchable by important parameters:

```csharp
logger.LogInformation($"Shipment for order '{request.OrderId}' is already created");
```

Another example of structured logging could be:

```csharp
logger.LogInformation("Shipment for order '{OrderId}' is already created", request.OrderId);
logger.LogInformation("Updated state of shipment {ShipmentNumber} to {NewState}", request.ShipmentNumber, request.Status);
```

By implementing logging in such a structured way, you will be able to search logs in log view tool to get, for example, all the events related to a given ShipmentNumber, State or OrderId.

## Best Practise 5: Avoid Logging Sensitive Information

Ensure that sensitive information such as passwords, credit card numbers, or personally identifiable information is not logged. Logging sensitive data can lead to security vulnerabilities.

You should also avoid logging such security information as API Keys, authentication tokens, connection strings, etc.

Serilog out of the box provides several features and practices to help avoid logging sensitive information:

**1\. Use Destructuring Policies:**

Serilog allows you to control how objects are logged using destructuring policies. These policies enable you to sanitize or mask sensitive information before it is logged. For example, if you have a complex object that contains sensitive data, you can define a destructuring policy to exclude or mask specific properties:

```csharp
Log.Logger = new LoggerConfiguration()
    .Destructure.ByMaskingProperties("Password", "CreditCardNumber")
    .WriteTo.Console()
    .CreateLogger();

var user = new
{
    Username = "anton",
    Password = "password_secret_information",
    CreditCardNumber = "1000-1000-1000-1000"
};

Log.Information("User details: {@User}", user);
```

In this example, the `Password` and `CreditCardNumber` properties are masked before being logged.

**2\. Redact Sensitive Information Manually:**

If you are logging individual pieces of information, you can manually redact or sanitize sensitive data before passing it to the logger.

```csharp
var password = "password_secret_information";
var sanitizedPassword = new string('*', password.Length);

Log.Information("User attempted to login with password: {Password}", sanitizedPassword);
```

Here, the actual password is replaced with a string of asterisks, ensuring that sensitive data is not logged.

**3\. Configure Filters to Exclude Sensitive Information:**

Serilog allows you to configure filters that can exclude specific log events or properties based on certain conditions. You can set up filters to prevent sensitive information from being logged.

```csharp
Log.Logger = new LoggerConfiguration()
    .Filter.ByExcluding(logEvent => logEvent.Properties.ContainsKey("Password"))
    .WriteTo.Console()
    .CreateLogger();

Log.Information("User details: {Username}, {Password}", "anton", "password_secret_information");
```

In this example, any log event containing a `Password` property will be excluded from the logs.

Regularly review your logs to ensure that no sensitive information is being inadvertently logged. Implement automated checks or manual reviews as part of your security practices to detect any potential issues.

## Best Practise 6: Log Errors

Error logging is essential for diagnosing and troubleshooting issues within an application. When errors occur, detailed logs can provide insights into the cause, context, and impact of the error. This information is crucial for resolving issues quickly and ensuring the reliability and stability of your application.

Depending on your application's security requirements, you may or may not log an exception stacktrace. But please, never expose stacktrace to your end users, for example, as a part of your "500 Internal Server Error" response.

Here is how you can log an exception with stacktrace with Serilog

```csharp
try
{
    // The code might throw an exception
}
catch (Exception ex)
{
    Log.Error(ex, "An unexpected error occurred");
}
```

When logging errors, you can include relevant contextual information that can help diagnose the issue. This could include information about the current user, request details, or the state of the application at the time of the error.

For example:

```csharp
try
{
    // The code might throw an exception
}
catch (Exception ex)
{
    Log.ForContext("UserId", userId)
       .ForContext("RequestPath", requestPath)
       .Error(ex, "An error occurred while processing the request");
}
```

Serilog supports custom **enrichers** that allow you to automatically add specific pieces of information to all log events, including errors. This can ensure that critical contextual information is always included in error logs.

```csharp
Log.Logger = new LoggerConfiguration()
    .Enrich.WithProperty("ApplicationName", "ApplicationName")
    .Enrich.WithProperty("Environment", "Production")
    .WriteTo.Console()
    .CreateLogger();

try
{
    // The code might throw an exception
}
catch (Exception ex)
{
    Log.Error(ex, "An error occurred");
}
```

## Best Practise 7: Monitor Log Size and Performance

Logging if not managed properly, it can introduce performance bottlenecks and excessive storage consumption.

You can limit the log size by applying the following techniques:

*   using appropriate logging levels
*   using logging filters
*   implementing log rotation and retention for file logging

We have already talked about using appropriate logging levels and logging filters. Let's explore the log rotation and retention for file logging.

Log rotation involves automatically archiving and creating new log files at specified intervals, such as daily or weekly. Log retention policies define how long archived logs should be kept before they are deleted. Both of these practices help manage disk usage by preventing log files from growing indefinitely.

For example:

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.File(
        "logs/service.log",
        rollingInterval: RollingInterval.Day, // Rotate logs daily
        retainedFileCountLimit: 7)  // Retain only the last 7 days of logs
    .CreateLogger();
```

This configuration ensures that log files are rotated daily and that only the last 7 days of logs are retained, preventing old logs from consuming excessive disk space.

You can also configure this in the `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information"
    },
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/service.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7, // Retain logs for 7 days
          "fileSizeLimitBytes": 10485760 // Limit file size to 10MB
        }
      }
    ]
  }
}
```

Logging can impact application performance, particularly if logs are being written to disk or sent over the network. Monitor the overhead introduced by logging, especially in high-traffic or performance-critical applications. Consider using asynchronous logging to minimize the impact on application performance.

```csharp
Log.Logger = new LoggerConfiguration()
    .WriteTo.Async(a => a.File("logs/log.txt", rollingInterval: RollingInterval.Day))
    .CreateLogger();
```

Here I use `Serilog.Sinks.Async` - an asynchronous wrapper for Serilog sinks that logs on a background thread. It can be useful for file logging that may be affected by I/O bottlenecks.

It is also very important to **turn off console logging** in your production environment. Console logging can slow down your application significantly.

## Best Practise 8: Centralize and Visualize Logs with a Logging UI

In modern applications, especially those running in distributed environments or microservices architectures, logging can quickly become overwhelming. Logs are often spread across multiple servers, services, or containers, making it difficult to gain insights into the overall system health or to troubleshoot specific issues. A centralized logging UI, like Seq, addresses these challenges by aggregating logs from various sources into a single, searchable interface that provides powerful visualization and analysis tools.

Using a logging UI like Seq not only centralizes your logs but also enhances your ability to monitor, search, and analyze log data in real-time.

Remember I told you about structured logging? You can search for specific log parameters using Seq, or any other similar tools.

Another advantage of using centralized tools for logging management - is being able to configure alerts on errors or other important logs.

To get started with Seq, you need to add the following Nuget package:

```bash
dotnet add package Serilog.Sinks.Seq
```

And update Serilog logging configuration in `appsettings.json`:

```csharp
{
  "Serilog": {
    "Using": [
      "Serilog.Sinks.Console",
      "Serilog.Sinks.Seq"
    ],
    "MinimumLevel": {
      "Default": "Debug",
      "Override": {
        "Microsoft": "Information"
      }
    },
    "WriteTo": [
      { "Name": "Console" },
      {
        "Name": "Seq",
        "Args": {
          "serverUrl": "http://localhost:5341"
        }
      }
    ],
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ],
    "Properties": {
      "Application": "ShippingService"
    }
  }
}
```

Here, we configure logging to Console (don't use in production) and Seq. We point the Seq's URL to http://localhost:5341 when running locally. When running service inside a docker container - you need to use the docker container's name instead of a localhost: http://seq:5341.

Here is how logging looks like in Seq:

![A screenshot of the Seq logging UI showing a list of log events, with a search bar at the top containing the query `NewState in [ 'Processing', 'Dispatched', 'InTransit' ]`. The log entries show "Updated state of tracking/shipment" messages with timestamps.](https://antondevtips.com/media/code_screenshots/architecture/seq-logging-traces/img_aspnet_seq_2.png)

If you want to learn How to Implement Structured Logging and Distributed Tracing for Microservices with Seq, make sure to check out my [blog post](https://antondevtips.com/blog/how-to-implement-structured-logging-and-distributed-tracing-for-microservices-with-seq).

Here is a list of Seq alternatives, in case you need something else:

*   ELK Stack: Elasticsearch, Logstash, and Kibana
*   Datadog
*   New Relic
*   Loggly
*   GrayLog
*   Azure Monitor Logs
*   Amazon CloudWatch Logs (AWS)

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Flogging-best-practices-in-asp-net-core&title=Logging%20Best%20Practices%20in%20ASP.NET%20Core)[X](https://twitter.com/intent/tweet?text=Logging%20Best%20Practices%20in%20ASP.NET%20Core&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Flogging-best-practices-in-asp-net-core)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Flogging-best-practices-in-asp-net-core)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.