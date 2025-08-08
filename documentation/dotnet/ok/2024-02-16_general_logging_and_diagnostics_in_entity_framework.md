```yaml
---
title: Logging and Diagnostics in Entity Framework
source: https://okyrylchuk.dev/blog/logging-and-diagnostics-in-entity-framework/
date_published: 2024-02-16T19:55:21.000Z
date_captured: 2025-08-06T18:29:19.651Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [Entity Framework, .NET, Microsoft.Extensions.Logging, Serilog, SQL Server]
programming_languages: [C#, SQL]
tags: [logging, diagnostics, entity-framework, dotnet, csharp, database, performance, debugging, interceptors, query-optimization]
key_concepts: [query-inspection, simple-logging, advanced-logging, sensitive-data-logging, detailed-errors, database-interceptors, diagnostic-listeners, event-handling]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to logging and diagnostics in Entity Framework 8, essential for application debugging and performance tuning. It covers basic query inspection using `ToQueryString` and simple `LogTo` configurations, including options for sensitive data and detailed errors. The post then advances to integrating robust logging frameworks like `Microsoft.Extensions.Logging` and Serilog for production-ready solutions. Finally, it explores advanced mechanisms such as `Interceptors` for fine-grained control over database operations and `Diagnostic Listeners` for system-wide event monitoring, offering a holistic view of EF's diagnostic capabilities.]
---
```

# Logging and Diagnostics in Entity Framework

# Logging and Diagnostics in Entity Framework

![Author's avatar, a stylized profile picture.](https://okyrylchuk.dev/wp-content/litespeed/avatar/a85a89199a7d4486a29f67dc2fc371d0.jpg?ver=1754361075)By [Oleg Kyrylchuk](https://okyrylchuk.dev/blog/author/kyrylchuk-oleggmail-com/ "View all posts by Oleg Kyrylchuk") / February 16, 2024

[Sponsor this newsletter](/sponsorship "Sponsor this newsletter")

Logging is crucial for every application. It helps with debugging, troubleshooting, performance optimization, monitoring, alerting, security auditing, etc.

Entity Framework provides several mechanisms for generating logs and obtaining diagnostics.

Let’s dive deep into how we can use those mechanisms.

**Note**: All examples use EF 8.  

Table Of Contents

1.  [ToQueryString](#toquerystring)
2.  [Simple Logging](#simple-logging)
3.  [Advanced Logging](#advanced-logging)
4.  [Interceptors](#interceptors)
5.  [Diagnostic Listeners](#diagnostic-listeners)
6.  [Summary](#summary)

## ToQueryString

The simplest way for us to see the query EF generates is through the **ToQueryString** method.

```csharp
var query =  context.People
    .Where(p => p.Id == 1)
    .Select(p => new { p.FirstName, p.LastName })
    .ToQueryString();

Console.WriteLine(query);

// DECLARE @__p_0 bigint = CAST(1 AS bigint);

// SELECT[p].[FirstName], [p].[LastName]
// FROM[People] AS[p]
// WHERE[p].[Id] = @__p_0
```

The method returns a string representation of the fetching queries. Easy, but limited.

It doesn’t work for direct executions like creating, modifying, or deletion. It would be best if you used it only in debugging.

## Simple Logging

If you want to get logs for all EF operations, including transaction management and migrations, you can configure simple logging when configuring a **DbContext**.

```csharp
protected override void OnConfiguring(
    DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .LogTo(Console.WriteLine)
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors();
```

The **LogTo** method accepts a delegate that accepts a string. EF calls this delegate for each log message generated. It’s often used to write each log to the console using **Console.WriteLine**.

But you can also write the logs to the debug window using **Debug.WriteLine** or directly to the file using **StreamWriter**.

EF doesn’t include any data values in the exception messages by default. You can enable it with the **EnableSensitiveDataLogging** method.

**EnableDetailedErrors** adds try-catch blocks for each call to the database provider. This is helpful, as some exceptions can be hard to diagnose without it.

Simple logging is only for debugging during application development. You don’t want to expose sensitive production data and degrade performance because of try-catch blocks.

You can filter the logs you want to see if you’re looking for a specific issue.

```csharp
protected override void OnConfiguring(
    DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .LogTo(Console.WriteLine, LogLevel.Warning)
            .LogTo(Console.WriteLine, new[] { CoreEventId.SaveChangesCompleted });
```

## Advanced Logging

For advanced logging, you can use the **UserLoggerFactory** method to use **Microsoft.Extensions.Logging** for logging. You can use Microsoft Logger or any other supported logger, like **Serilog**. 

```csharp
var logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddProvider(
    new SerilogLoggerProvider(logger));
```

```csharp
internal class MyContext : DbContext
{
    public DbSet<Person> People { get; set; }

    private readonly ILoggerFactory _loggerFactory;

    public MyContext(DbContextOptions<MyContext> options,
        ILoggerFactory loggerFactory) : base(options)
    {
        _loggerFactory = loggerFactory;
    }

    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseLoggerFactory(_loggerFactory);
}
```

This approach is suitable for all environments, including production. Remember about logging sensitive data. Turn it off or configure it so as not to expose sensitive data.

## Interceptors

Interceptors allow you to intercept and potentially modify database operations before or after they are sent to the database. They can also be used for logging and diagnostics.

There are several interceptor interfaces to implement.

**IDbCommandInterceptor** intercepts creating and executing commands, command failures, and disposing of the commands’s DbDataReader.

**IDbConnectionInterceptor** intercepts opening and closing connections and connection failures.

**IDbTransactionInterceptor** intercepts everything regarding transactions and savepoints.

To register the interceptor, you need to use the **AddInterceptors** method while using the **DbContext** configuration. 

```csharp
class CommandQueryInterceptor : DbCommandInterceptor
{

    public override InterceptionResult<DbDataReader> ReaderExecuting(
               DbCommand command,
                      CommandEventData eventData,
                             InterceptionResult<DbDataReader> result)
    {
        Console.WriteLine(command.CommandText);

        return base.ReaderExecuting(command, eventData, result);
    }
}
```

```csharp
protected override void OnConfiguring(
    DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder
            .AddInterceptors(new CommandQueryInterceptor());
```

You can register many interceptors. Their limitation is that they are registered per **DbContext**.  

## Diagnostic Listeners

The diagnostic listeners can provide the same information as interceptors. However, they do so for many DbContexts instances in the current .NET process.

The diagnostic listeners are a standard mechanism across .NET. But they are not designed for logging.

As the name suggests, they are suitable for diagnostics. Some configuration is required to resolve EF events. First, you need to create an observer and register it globally. 

```csharp
public class DiagnosticObserver : IObserver<DiagnosticListener>
{
    public void OnCompleted()
        => throw new NotImplementedException();

    public void OnError(Exception error)
        => throw new NotImplementedException();

    public void OnNext(DiagnosticListener value)
    {
        // "Microsoft.EntityFrameworkCore"
        if (value.Name == DbLoggerCategory.Name)
        {
            value.Subscribe(new KeyValueObserver());
        }
    }
}
```

```csharp
DiagnosticListener.AllListeners.Subscribe(new DiagnosticObserver());
```

In the **OnNext** method, we look for **DiagnosticListener** from EF. 

Now, we have to create a key-value observer for specific EF events. 

```csharp
public class KeyValueObserver : IObserver<KeyValuePair<string, object>>
{
    public void OnCompleted()
        => throw new NotImplementedException();

    public void OnError(Exception error)
        => throw new NotImplementedException();

    public void OnNext(KeyValuePair<string, object> value)
    {
        if (value.Key == CoreEventId.ContextInitialized.Name)
        {
            var payload = (ContextInitializedEventData)value.Value;
            Console.WriteLine($"EF is initializing {payload.Context.GetType().Name} ");
        }
    }
}
```

In the **OnNext** method, we look for an EF event responsible for context initialization.

## Summary

Logging and diagnostics are vital in Entity Framework for debugging, optimizing performance, ensuring security compliance, real-time monitoring, and educating developers.

EF offers several approaches to diagnosing or logging issues. Each has pros and cons. Understanding and learning how each can help you solve your needs is essential. 

Post navigation

[← Previous Post](https://okyrylchuk.dev/blog/pattern-matching-in-csharp/ "Pattern Matching in C#")

[Next Post →](https://okyrylchuk.dev/blog/intro-to-serialization-with-source-generation-in-system-text-json/ "Intro to Serialization with Source Generation in System.Text.Json")

## Related Posts

![Thumbnail for an article titled "Complex Types in Entity Framework 8".](https://okyrylchuk.dev/wp-content/uploads/2024/01/featured2-1.png.webp)

### [Complex Types in Entity Framework 8](https://okyrylchuk.dev/blog/complex-types-in-entity-framework-8/)

[Entity Framework](https://okyrylchuk.dev/blog/category/entity-framework/), [Entity Framework 8](https://okyrylchuk.dev/blog/category/entity-framework-8/) / January 12, 2024

![Thumbnail for an article titled "Single vs. Split Query in Entity Framework".](https://okyrylchuk.dev/wp-content/uploads/2024/03/featured10.png.webp)

### [Single vs. Split Query in Entity Framework](https://okyrylchuk.dev/blog/single-vs-split-query-in-entity-framework/)

[Entity Framework](https://okyrylchuk.dev/blog/category/entity-framework/), [Performance](https://okyrylchuk.dev/blog/category/performance/) / March 8, 2024

![A banner image for a blog post titled "Complex Types in Entity Framework 8", part of ".NET Pulse #2". The image has a dark blue background with abstract light blue wave patterns and yellow text.](data:image/jpeg;base64,/9j/4AAQSkZJRgABAQAAAQABAAD/2wBDAAYEBQYFBAYGBQYHBwYIChAKCgkJChQODwwQFxQYGBcUFhYaHSUfGhsjHBYWICwgIyYnKSopGR8tMC0oMCUoKSj/2wBDAQcHBwoIChMKChMoGhYaKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCgoKCj/wAARCAH0B4ADASIAAhEBAxEB/8QAHwAAAQUBAQEBAQEAAAAAAAAAAAECAwQFBgcICQoL/8QAtRAAAgEDAwIEAwUFBAQAAAF9AQIDAAQRBRIhMUEGE1FhByJxFDKBkaEII0KxwRVS0fAkM2JyggkKFhcYGRolJicoKSo0NTY3ODk6Q0RFRkdISUpTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmpucnZ6fkqOkpaanqKmqq6ytrq+wsbKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3+Pn6/8QAHwEAAwEBAQEBAQEBAQAAAAAAAAECAwQFBgcICQoL/8QAtREAAgECBAQDBAcFBAQAAQJ3AAECAxEEBSExBhJBUQdhcRMiMoEIFEKRobHBCSMzUvAVYnLRChYkNOEl8RcYGRomJygpKjU2Nzg5OkNERUZHSElKTVFVWV1hZWmNkZWZnaGlqc3R1dnd4eXqDhIWGh4iJipKTlJWWl5iZmpucnZ6fkqOkpaanqKmqq6ytrq+wsbKztLW2t7i5usLDxMXGx8jJytLT1NXW19jZ2uHi4+Tl5ufo6erx8vP09fb3/Pn6/9oADAMBAAIRAxEAPwD9/KKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACiiigAooooAKKKKACii