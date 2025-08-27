```yaml
---
title: Structured Logging in ASP.NET Core - Coding Sonata
source: https://codingsonata.com/structured-logging-in-asp-net-core/#respond
date_published: 2025-08-30T05:47:00.000Z
date_captured: 2025-08-30T19:46:56.704Z
domain: codingsonata.com
author: Aram
category: general
technologies: [ASP.NET Core, .NET, .NET Core, Serilog, NuGet, Visual Studio 2022, SQL Server, Postman, Seq, Elasticsearch, Logstash, Kibana, Grafana, Azure Monitor, CloudWatch, OpenTelemetry]
programming_languages: [C#, SQL]
tags: [structured-logging, logging, aspnet-core, serilog, web-api, dotnet, configuration, sinks, error-handling, monitoring]
key_concepts: [structured-logging, logging-providers, serilog-sinks, application-configuration, dependency-injection, log-levels, exception-logging, log-enrichment, correlation-ids, centralized-logging]
code_examples: false
difficulty_level: intermediate
summary: |
  [This tutorial demonstrates how to implement structured logging in an ASP.NET Core Web API using the Serilog library. It explains the benefits of structured logging, which captures log entries as contextual data rather than plain text, making them easier to search and analyze. The article guides readers through importing necessary Serilog NuGet packages, configuring logging settings in `appsettings.json` for dynamic control, and setting up Serilog in `Program.cs`. It also illustrates how to use the abstract `ILogger<T>` interface within a controller to log information and exceptions, with examples of writing logs to both file and SQL Server sinks. Finally, it provides a comprehensive list of 12 best practices for effective structured logging in ASP.NET Core applications.]
---
```

# Structured Logging in ASP.NET Core - Coding Sonata

# Structured Logging in ASP.NET Core

In this tutorial we will learn how to implement structured logging in ASP.NET Core Web API using Serilog.

Logging is one of the most important and critical features in development that allows the developers and product owners to troubleshoot and analyze application errors.

Structured logging is a way of writing logs so that they are not just plain text messages but data with context. Instead of treating a log entry as a string, structured logging captures key information as fields (like JSON). This makes logs easier to store, search, and analyze with modern log management tools.

Using Serilog or any other logging provider, whenever your application writes comprehensive and extensive logs you will be able to trace any error or issue that happen and that will greatly help in finding the solution for the problem.

It is pretty easy and straightforward to introduce logging with Serilog in ASP.NET Core Web API.

In fact, there is a specific NuGet package for Serilog that is tailored for the ASP.NET Core Projects.

And the power of Serilog is represented in the massive number of Sinks it supports. These sinks will help you dump and write logs into virtually any external storage, like files, databases, indexes, or even cloud locations.

Here is a tutorial for how to implement structured logging in ASP.NET Core using Serilog to write your logs to different outputs, using different sinks and in a structured and properly formatted way:

## 1. Import Serilog and Related NuGet Packages

Import the required NuGet packages that includes the core library for Serilog in addition to the other libraries that Serilog depends on for both enriching the structured formatting as well as dumping the logs into different outputs via sinks.

You can either use the NuGet Package Manager GUI for VS 2022, or from the NuGet Package Manager console you can run the below commands:

```bash
dotnet add package Serilog.AspNetCore
dotnet add package Serilog.Settings.Configuration
dotnet add package Serilog.Sinks.File
dotnet add package Serilog.Sinks.MSSqlServer
dotnet add package Serilog.Enrichers.Environment
dotnet add package Serilog.Enrichers.Thread
```

## 2. AppSettings.json Configurations

To make your logging more dynamic, it is always recommended to keep the logging configurations inside your appsettings.json files so that you can change the settings without having to redeploy your whole ASP.NET Core project.

Such settings can include general log level, and sink specific configurations like file path and arguments, log template structure, credentials for access controlled providers like DBs or index-based providers.

So here is an example for Appsettings.json file that includes Serilog configurations for file and SQL Server Sinks:

```json
{
  "Serilog": {
    "Using": [ "Serilog.Sinks.File", "Serilog.Sinks.MSSqlServer" ],
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "Enrich": [ "FromLogContext", "WithMachineName", "WithThreadId" ],
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "logs/app-.log",
          "rollingInterval": "Day",
          "retainedFileCountLimit": 7,
          "outputTemplate": "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
        }
      },
      {
        "Name": "MSSqlServer",
        "Args": {
          "connectionString": "Server=Home\\\\SQLEXPRESS;Database=TasksDb;Trusted_Connection=True;MultipleActiveResultSets=true",
          "tableName": "Logs",
          "autoCreateSqlTable": true,
          "columnOptionsSection": {
            "addStandardColumns": [ "LogEvent", "Properties" ]
          }
        }
      }
    ]
  }
}
```

## 3. Program.cs Setup

In this step, we will be assigning the logging provider within our ASP.NET Core project as Serilog, also we will be configuring Serilog to read its configurations from appsettings.json file according to the current environment (as specified in the launch.json file).

So here is the code that you need to include as part of the Program.cs file:

```csharp
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Load Serilog configuration from appsettings.json
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Host.UseSerilog();

builder.Services.AddControllers();

var app = builder.Build();

app.MapControllers();

app.Run();
```

### 4. Using Abstract ILogger<T>

The good thing about Serilog (and many other logging libraries), that they can make use of the abstract generic ILogger<T> interface to provide logging features based on the functions available in ILogger<T>.

This is a great example of how abstraction can provide a powerful way to extend functions through the different providers without having to change your code implementation.

Below is an example of using ILogger<T> inside a controller:

```csharp
using Microsoft.AspNetCore.Mvc;

namespace StructuredLogging.Controllers
{
    public record Task(int UserId, string Title, string Description);

    [ApiController]
    [Route("api/[controller]")]
    public class TasksController(ILogger<TasksController> logger) : ControllerBase
    {
        [HttpPost]
        public IActionResult CreateTask(Task task)
        {
            logger.LogInformation("User {UserId} created task {Title} with description {Description}",
                task.UserId, task.Title, task.Description);
            try
            {
                if (string.IsNullOrWhiteSpace(task.Title))
                    throw new ArgumentException("Task title cannot be empty");
                return Ok(new { Status = "Task created" });
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create task for {UserId} with title {Title}", task.UserId, task.Title);
                return BadRequest("Task creation failed");
            }
        }
    }
}
```

## 5. Testing the API via Postman

I will use postman to simulate calling CreateTask endpoint to trigger inserting the log according to the log level defined and will be dumped to the sinks as defined inside the log files:

After the app runs for the first time, it will create the table in SQL Server with the provided settings, as you can see in the below screenshot:

### Sql Server Sink (MSSqlServer)

![Screenshot showing the dbo.Logs table created under the TasksDb database in SQL Server.](https://i0.wp.com/codingsonata.com/wp-content/uploads/2025/08/image-10.png?resize=216%2C145&ssl=1)

And a quick search in the table, will give us the log added with complete details:

![Screenshot of a SQL Server query result showing a structured log entry in the 'Logs' table, including Id, Message, MessageTemplate, Level, TimeStamp, Exception, Properties, and LogEvent columns.](https://i0.wp.com/codingsonata.com/wp-content/uploads/2025/08/image-12.png?resize=735%2C213&ssl=1)

## File Sink

Also you will find the log written inside the file, as per the configuration settings:

![Screenshot of a file explorer showing an 'app-20250830.log' file in the 'logs' directory of the StructuredLogging application.](https://i0.wp.com/codingsonata.com/wp-content/uploads/2025/08/image-11.png?resize=646%2C129&ssl=1)

And openning the file, will show you the structured log dumped inside:

![Screenshot of a text editor displaying the content of 'app-20250830.log', showing a structured log entry in JSON format with timestamp, level, message, and properties.](https://i0.wp.com/codingsonata.com/wp-content/uploads/2025/08/image-13.png?resize=735%2C187&ssl=1)

## 12 Rules for Structured Logging in ASP.NET Core

Here is a list of rules and best practices to follow when implementing structured logging in ASP.NET Core

*   Use **ILogger<T>** to keep your code decoupled from providers.
*   Choose the right **log level** (Debug, Info, Warning, Error, Critical).
*   Log exceptions properly. Always include the **exception object and stack trace**.
*   Keep logs **structured** by using prominent logging libraries like Serilog, and use {} placeholders, not string concatenation.
*   **Never log sensitive data**. Mask passwords and personal info.
*   Add **context & correlation IDs**. Trace requests, users, and operations across services.
*   Configure **log levels in appsettings.json** and Adjust per environment (Debug in Dev, Warning in Prod).
*   **Centralize logs** using Seq, ELK, Grafana, Azure Monitor, or CloudWatch.
*   Set **retention & rotation policies** to prevent log files from growing uncontrollably.
*   **Enrich logs** with alerts & monitoring. Act on recurring warnings and critical failures.
*   Leverage **OpenTelemetry & async logging**. Unify logs, traces, metrics, and boost performance.
*   **Secure log storage** with access control on log files and dashboards

## Conclusion

Structured logging with Serilog in ASP.NET Core makes logs far more useful than plain text.

By capturing data as key–value pairs, you can query, filter, and analyze logs with precision.

Using the abstract generic ILogger<T>, you keep standard .NET practices while gaining Serilog’s power: multiple sinks, enrichers, and configuration through appsettings.json.

This approach ensures consistent, searchable, and actionable logs across console, files, and SQL Server, helping you monitor and troubleshoot your applications effectively.

## References

[Serilog](https://serilog.net/)

[Logging in .NET and ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-9.0)