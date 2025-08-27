```yaml
---
title: How to Use BackgroundService in .NET for Long-Running Tasks
source: https://okyrylchuk.dev/blog/how-to-use-backgroundservice-in-dotnet-for-long-running-tasks/
date_published: 2025-02-14T15:55:00.000Z
date_captured: 2025-08-08T13:19:34.310Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [.NET 8, ASP.NET Core, BackgroundService, IHostedService, Dependency Injection, IOptions]
programming_languages: [C#]
tags: [background-tasks, dotnet, csharp, hosted-services, asynchronous-programming, dependency-injection, long-running-tasks, service-lifetimes, best-practices]
key_concepts: [BackgroundService, IHostedService, Dependency Injection, Service Lifetimes, Asynchronous Programming, Cancellation Token, Resource Management, Error Handling, IOptions Pattern]
code_examples: true
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide on implementing long-running background tasks in .NET using the `BackgroundService` class, available since .NET 8. It explains what `BackgroundService` is and how it simplifies continuous operations by implementing `IHostedService`. The post includes practical C# code examples demonstrating how to create and register a periodic background service. A crucial section addresses the common challenge of correctly injecting and consuming scoped services within a singleton `BackgroundService`, offering a solution using manual scope creation. Finally, it outlines essential best practices for building robust and reliable background services, covering error handling, cancellation, resource management, logging, and configuration.
---
```

# How to Use BackgroundService in .NET for Long-Running Tasks

# How to Use BackgroundService in .NET for Long-Running Tasks

In software development, we often need to process long-running background tasks. These can include polling another service, monitoring tasks, processing queue messages, scheduling clean-up tasks, etc.

Since .NET 8, handling such scenarios is easier than ever. You can use .NET’s hosting model to create background services. Today, we will explore the **BackgroundService** class.

## **What is BackgroundService?**

**BackgroundService** is an abstract base class in .NET that implements **IHostedService**, providing a straightforward way to create long-running background tasks. It’s designed specifically for scenarios where you need a task to run continuously throughout the application’s lifetime, such as:

*   Processing messages from a queue
*   Monitoring file system changes
*   Performing periodic cleanup operations
*   Handling scheduled tasks
*   Processing data in the background

## **Creating Your First BackgroundService**

Let’s create your first background service that does the task in time intervals – **PeriodicBackgroundService**.

```csharp
public class PeriodicBackgroundService: BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        PeriodicTimer time = new(TimeSpan.FromSeconds(1));
        
        while (await time.WaitForNextTickAsync(stoppingToken))
        {
            Console.WriteLine($"Running background task: {DateTime.UtcNow}");
        }
    }
}
```

To use your **PeriodicBackgroundService**, you need to register it with the dependency injection container:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHostedService<PeriodicBackgroundService>();

var app = builder.Build();

app.Run();
```

When you start the application, you’ll see that **PeriodicBackgroundService** is also running.

![Console output showing a .NET application starting and the periodic background task logging its execution with timestamps.](https://okyrylchuk.dev/wp-content/uploads/2025/02/output-png.avif "output")

## **Scoped Services**

By default, **Scoped** Services are created per HTTP request in ASP.NET Core web applications.

But what about **BackgroundService**? When you inject the service registered as **Scoped** to the **BackgroundService**, you’ll get the error:

System.AggregateException: Some services are not able to be constructed (Error while validating the service descriptor ‘ServiceType: Microsoft.Extensions.Hosting.IHostedService Lifetime: Singleton ImplementationType: BackgroundServiceExample.PeriodicBackgroundService’: Cannot consume scoped service ‘IScopedService’ from singleton ‘Microsoft.Extensions.Hosting.IHostedService’.)

That’s because the **BackgroundService** is registered as Singleton. I have already written about [how to call Scoped Service in the Singleton](/blog/service-lifetimes-in-dotnet).

To inject Scoped Service in the **BackgroundService**, you must manually create a scope. Also, you can [create an asynchronous scope](/blog/how-to-release-unmanaged-resources-asynchronously-in-dotnet/).

```csharp
public class AnotherBackgroundService(IServiceProvider serviceProvider): BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        PeriodicTimer time = new(TimeSpan.FromSeconds(1));
        
        while (await time.WaitForNextTickAsync(stoppingToken))
        {
            using IServiceScope scope = serviceProvider.CreateScope();
            var scopedService = scope.ServiceProvider.GetRequiredService<IScopedService>();
            
            scopedService.DoTask();
        }
    }
}
```

## **Best Practices**

When implementing **BackgroundService**, keep these best practices in mind:

1.  **Error Handling**: Always wrap your main processing logic in try-catch blocks to prevent unhandled exceptions from crashing your service.
2.  **Cancellation Token**: Respect the cancellation token passed to ExecuteAsync to ensure your service can shut down gracefully.
3.  **Resource Management**: Properly dispose of resources using IDisposable when your service is stopped.
4.  **Logging**: Implement comprehensive logging to track the service’s operation and troubleshoot issues.
5.  **Configuration**: Use [IOptions](/blog/how-to-handle-options-in-asp-net-core-better/) pattern for configurable values rather than hardcoding them.

## **Conclusion**

Background services in .NET provide a powerful way to handle asynchronous tasks. By leveraging **BackgroundService** or **IHostedService**, you can implement long-running tasks efficiently. Following best practices like handling exceptions, using cancellation tokens, and managing scoped dependencies will ensure your background services are robust and reliable.