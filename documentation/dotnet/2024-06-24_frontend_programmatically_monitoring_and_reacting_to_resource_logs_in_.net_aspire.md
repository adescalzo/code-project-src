## Summary
This article demonstrates two methods for programmatically monitoring and reacting to resource logs within a .NET Aspire application. It explains how to retrieve logs for a specific resource, such as a MongoDB container, by leveraging Aspire's `IDistributedApplicationLifecycleHook` and `ResourceNotificationService`. The first method uses the `Docker.DotNet` library to access container logs directly, while the second, more generic approach, utilizes Aspire's built-in `ResourceLoggerService` to stream logs from any resource, highlighting the extensibility of these services.

---

# Programmatically Monitoring and Reacting to Resource Logs in .NET Aspire

```markdown
**Source:** https://anthonysimmon.com/programmatically-monitoring-reacting-resource-logs-dotnet-aspire/?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=programmatically-monitoring-and-reacting-to-logs-in-net-aspire
**Date Captured:** 2025-07-28T19:33:10.514Z
**Domain:** anthonysimmon.com
**Author:** Unknown
**Category:** frontend
```

---

*This article explores how to combine `IDistributedApplicationLifecycleHook`, `ResourceNotificationService`, and `ResourceLoggerService` to react to any changes in resources and their logs.*

Jun 24, 2024 | 4 minute read

I was recently asked if it is possible to obtain the ID of a container orchestrated by .NET Aspire to monitor its logs. The answer is yes, and in this article, we will see two ways to achieve this by retrieving the logs of an arbitrary MongoDB resource named `mongo` and displaying them in the console. The first method will be specific to containers, while the second method can be applied to any resource.

```csharp
var builder = DistributedApplication.CreateBuilder(args);

builder.AddMongoDB("mongo");

builder.Build().Run();
```

## Method 1: Retrieving Container Logs with Docker.DotNet

For the first method, it is necessary to implement an `IDistributedApplicationLifecycleHook` where we will put the logic to intercept the container ID. Injecting the `ResourceNotificationService` provided by .NET Aspire will allow us to receive notifications of changes made to resources during their execution. We will then filter these notifications to keep only those concerning our `mongo` resource.

As you will see in the code below, each resource notification is associated with a snapshot that represents the current state of the resource. It contains a property bag in which .NET Aspire, once the container has started, stores the container ID with a [well-known property name](https://github.com/dotnet/aspire/blob/v8.0.1/src/Shared/Model/KnownProperties.cs#L30) `container.id`. This property is then displayed on the dashboard.

Once in possession of the container ID, we can use the [Docker.DotNet](https://www.nuget.org/profiles/Docker.DotNet) library to connect to the Docker API and retrieve the logs.

Without further ado, here is the code for the distributed lifecycle hook for this first method. Don’t forget to register it in the distributed application builder services:

```csharp
builder.Services.AddLifecycleHook<GetMongoResourceLogsLifecycleHook>();

internal sealed class GetMongoResourceLogsLifecycleHook(
    ResourceNotificationService resourceNotificationService,
    ILogger<GetMongoResourceLogsLifecycleHook> logger) : IDistributedApplicationLifecycleHook, IAsyncDisposable
{
    private readonly CancellationTokenSource _shutdownCts = new();
    private Task? _getMongoResourceLogsTask;

    public Task AfterResourcesCreatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
    {
        this._getMongoResourceLogsTask = this.GetMongoResourceLogsAsync(this._shutdownCts.Token);
        return Task.CompletedTask;
    }

    private async Task GetMongoResourceLogsAsync(CancellationToken cancellationToken)
    {
        try
        {
            string? mongoContainerId = null;
            await foreach (var notification in resourceNotificationService.WatchAsync(cancellationToken))
            {
                if (notification.Resource.Name == "mongo" &&
                    notification.Snapshot.Properties.FirstOrDefault(x => x.Name == "container.id") is { Value: string { Length: > 0 } containerId })
                {
                    mongoContainerId = containerId;
                    break;
                }
            }

            if (mongoContainerId is not null)
            {
                using var dockerClient = new DockerClientConfiguration().CreateClient();

                var logParameters = new ContainerLogsParameters { Timestamps = true, Follow = true, ShowStderr = true, ShowStdout = true };
                await dockerClient.Containers.GetContainerLogsAsync(mongoContainerId, logParameters, cancellationToken, new Progress<string>(line =>
                {
                    logger.LogInformation("{MongoOutput}", line);
                }));
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the application is shutting down.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred while getting the logs for the mongo resource");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await this._shutdownCts.CancelAsync();

        if (this._getMongoResourceLogsTask is not null)
        {
            try
            {
                await this._getMongoResourceLogsTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        this._shutdownCts.Dispose();
    }
}
```

It is important to note that once the [support for restarting resources from the dashboard](https://github.com/dotnet/aspire/issues/295) is implemented in a future version of .NET Aspire, this code will need to be adapted to handle potential restarts of the `mongo` resource.

This method of retrieving logs is restrictive because it is limited to containers and requires the third-party library `Docker.DotNet`, although it is an official NuGet package [hosted under the dotnet GitHub organization](https://github.com/dotnet/Docker.DotNet/).

## Method 2: Retrieving Resource Logs with ResourceLoggerService

There is another way to retrieve resource logs, and it uses the `ResourceLoggerService`, a service also provided by .NET Aspire that exposes log streams of different resources. Here is the code for this second method:

```csharp
builder.Services.AddLifecycleHook<GetMongoContainerLogsLifecycleHook>();

internal sealed class GetMongoResourceLogsLifecycleHook(
    ResourceNotificationService resourceNotificationService,
    ResourceLoggerService resourceLoggerService,
    ILogger<GetMongoResourceLogsLifecycleHook> logger) : IDistributedApplicationLifecycleHook, IAsyncDisposable
{
    private readonly CancellationTokenSource _shutdownCts = new();
    private Task? _getMongoResourceLogsTask;

    public Task AfterResourcesCreatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken = default)
    {
        this._getMongoResourceLogsTask = this.GetMongoResourceLogsAsync(this._shutdownCts.Token);
        return Task.CompletedTask;
    }

    private async Task GetMongoResourceLogsAsync(CancellationToken cancellationToken)
    {
        try
        {
            string? mongoResourceId = null;
            await foreach (var notification in resourceNotificationService.WatchAsync(cancellationToken))
            {
                if (notification.Resource.Name == "mongo")
                {
                    mongoResourceId = notification.ResourceId;
                    break;
                }
            }

            if (mongoResourceId is not null)
            {
                await foreach (var batch in resourceLoggerService.WatchAsync(mongoResourceId).WithCancellation(cancellationToken))
                {
                    foreach (var logLine in batch)
                    {
                        logger.LogInformation("{MongoOutput}", logLine);
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when the application is shutting down.
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An exception occurred while getting the logs for the mongo resource");
        }
    }

    public async ValueTask DisposeAsync()
    {
        await this._shutdownCts.CancelAsync();

        if (this._getMongoResourceLogsTask is not null)
        {
            try
            {
                await this._getMongoResourceLogsTask;
            }
            catch (OperationCanceledException)
            {
            }
        }

        this._shutdownCts.Dispose();
    }
}
```

It is interesting to note that the logs of the .NET Aspire dashboard, which is actually just an executable orchestrated by .NET Aspire, are forwarded to the console using [this exact same technique](https://github.com/dotnet/aspire/blob/v8.0.1/src/Aspire.Hosting/Dashboard/DashboardLifecycleHook.cs).

Of course, this article is written solely for the purposes of demonstrating the simplified use of `IDistributedApplicationLifecycleHook`, `ResourceNotificationService`, and `ResourceLoggerService`. These types, once combined, offer numerous extensibility possibilities.

Licensed under CC BY 4.0

## Related Content

*   ![Featured image for Referencing external Docker containers in .NET Aspire using the new custom resources API](https://anthonysimmon.com/referencing-external-docker-containers-dotnet-aspire-custom-resources/dashboard-external-container.9c0154f40122a15bc7f87f7053b41e90_hu_fb703f8191489cbf.png) [Referencing external Docker containers in .NET Aspire using the new custom resources API](/referencing-external-docker-containers-dotnet-aspire-custom-resources/)
*   ![Featured image for Better Azure Identity authentication support and performance during local development with .NET Aspire](https://anthonysimmon.com/dotnet-aspire-better-azure-identity-support/cover.e94a462bb876faeae91e56a802f6e995_hu_3dae01ae006b92e.png) [Better Azure Identity authentication support and performance during local development with .NET Aspire](/dotnet-aspire-better-azure-identity-support/)
*   ![Featured image for Must-have resources for new .NET Aspire developers](https://anthonysimmon.com/must-have-resources-for-new-dotnet-aspire-developers/cover.1527166066ea4b2721141ce8071304ee_hu_68e021f6f66cf0fc.jpg) [Must-have resources for new .NET Aspire developers](/must-have-resources-for-new-dotnet-aspire-developers/)
*   ![Featured image for Disabling .NET Aspire authentication to skip the login page](https://anthonysimmon.com/disabling-dotnet-aspire-authentication-skip-login-page/cover.98c3c13c4312bf8b3a22be714b3c36cb_hu_6bdb0894d91c775.png) [Disabling .NET Aspire authentication to skip the login page](/disabling-dotnet-aspire-authentication-skip-login-page/)
*   ![Featured image for .NET Aspire is the best way to experiment with Dapr during local development](https://anthonysimmon.com/dotnet-aspire-best-way-to-experiment-dapr-local-dev/cover.4f8b981897161a895e55ee9dfb12d7ae_hu_4c9b22f5b141c5df.png) [.NET Aspire is the best way to experiment with Dapr during local development](/dotnet-aspire-best-way-to-experiment-dapr-local-dev/)

---

![Screenshot of the .NET Aspire dashboard showing console logs for a 'mongo' resource, displaying JSON-formatted log entries.](AspireWatchlogs.png)

![Cropped screenshot of the .NET Aspire dashboard showing a table with resource details including 'Start time', 'Source', 'Endpoints', and 'Logs' columns.](reds7.png)

![Cropped screenshot of the .NET Aspire dashboard showing resource states as 'Running' with associated endpoints and actions.](running.png)