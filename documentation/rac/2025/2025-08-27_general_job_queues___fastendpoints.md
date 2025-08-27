```yaml
---
title: "Job Queues | FastEndpoints"
source: https://fast-endpoints.com/docs/job-queues#queueing-a-job
date_published: unknown
date_captured: 2025-08-27T18:10:54.026Z
domain: fast-endpoints.com
author: Unknown
category: general
technologies: [FastEndpoints, ASP.NET Core, .NET, SQL Server, MongoDB, LiteDB, EF Core, GitHub]
programming_languages: [C#, SQL]
tags: [job-queue, background-processing, fastendpoints, commands, concurrency, persistence, dotnet, asynchronous, web-api, data-access]
key_concepts: [job-queueing, command-pattern, background-processing, dependency-injection, concurrency-control, job-persistence, cancellation-tokens, progress-tracking]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details FastEndpoints' Job Queue functionality, designed for background processing of tasks like sending emails or generating reports. It explains how to define and queue commands for asynchronous execution, with options for delayed execution, expiry times, and configurable concurrency limits. A key component is job persistence, which requires implementing a custom storage provider to ensure data reliability across server restarts. The guide also covers advanced features such as cancelling queued jobs, retrieving results from completed commands, and tracking the real-time execution progress of long-running tasks. Practical code examples and references to example projects are provided for implementation guidance.]
---
```

# Job Queues | FastEndpoints

# Job Queueing With Commands

Work that can be processed in the background such as sending emails, pdf generation, report creation, etc. that doesn't require a result to be available immediately would be prime candidates for the **Job Queue** functionality in FastEndpoints.

Job queues allow you to schedule [Commands](command-bus#_1-define-a-command) to be executed in the background while limiting how many command instances of the same type can be executed at the same time. It is sometimes crucial to be in control of the degree of parallelism for certain types of tasks such as long-running or CPU intensive work to prevent the server from grinding to a halt, as well as to stay within access control limits of third-party services.

## Queueing A Job

Similarly to the [Command Bus](command-bus), the same **ICommand** and it's companion **ICommandHandler<TCommand>** is used to define the data contract and the execution logic such as the following:

```csharp
sealed class MyCommand : ICommand
{
    // ... command properties
}

sealed class MyCommandHandler : ICommandHandler<MyCommand>
{
    public Task ExecuteAsync(MyCommand command, CancellationToken ct)
    {
        // ... command execution logic
        return Task.CompletedTask;
    }
}
```

When you need to queue a command as a job, instead of executing it immediately, simply call the extension method **QueueJobAsync()** on the command DTO like so:

```csharp
await new MyCommand { /* ... command data ... */ }.QueueJobAsync();
```

A background job encapsulating the command is created and added to a queue for that type of command. There's a job queue per each command type in your application. If there's 10 command types, there'd be 10 independent queues processing jobs.

### Execution Options

At the time of queueing, it's possible to specify a future point of time after which the command/job is to be executed instead of immediately (which is the default behavior if you don't specify anything). This does not however mean that the job will be executed at the exact given time. It just will not be executed before that time.

The default expiry time of jobs is 4 hours from the time of creation, which you can override as shown below. If for some reason the job doesn't execute/complete successfully before the expiry time, it will be considered stale/incomplete and can be purged from the queue (which is discussed in the job persistence section below).

```csharp
.QueueJobAsync(
    executeAfter: DateTime.UtcNow.AddMinutes(30),
    expireOn: DateTime.UtcNow.AddHours(8));
```

### Enabling Job Queues

Job queues are not enabled by default and must be configured at startup. Since job queues are designed to be reliable and not lose data in case of server restarts/crashes, it's required of you to implement a storage provider on any database/storage medium of your choice. How to implement the storage provider is discussed below. For now, let's focus on the startup configuration.

`program.cs`

```csharp
var bld = WebApplication.CreateBuilder();
bld.Services
   .AddFastEndpoints()
   .AddJobQueues<JobRecord, JobStorageProvider>(); //ignore generic arguments for now

var app = bld.Build();
app.UseFastEndpoints()
   .UseJobQueues();
app.Run();
```

#### Per Queue Execution Limits

By default, each queue will process multiple commands in parallel. The default limit is the number of logical processors of the machine. For example, if the server has 4 cores/threads, **at most** only 4 commands of the same type will execute at the same time. You can customize the max concurrency setting at startup like this:

```csharp
.UseJobQueues(o => o.MaxConcurrency = 2);
```

Queued jobs can be given a certain amount of time to execute. Command executions exceeding that time limit would automatically get cancelled and retried. By default however, commands are allowed to execute without a limit. You can specify a maximum execution time like so:

```csharp
.UseJobQueues(o => o.ExecutionTimeLimit = TimeSpan.FromSeconds(10));
```

Specifying the limits like above applies to all types of commands, which you can override per type if needed:

```csharp
.UseJobQueues(o =>
{
    //general per queue limits
    o.MaxConcurrency = 2; 
    o.ExecutionTimeLimit = TimeSpan.FromSeconds(10);
    
    //applicable only to MyCommand
    o.LimitsFor<MyCommand>( 
        maxConcurrency: 8,
        timeLimit: TimeSpan.FromSeconds(5));
});
```

That's all the configuration needed (other than implementing the storage provider discussed below). As with the command bus, there's no need to register individual commands & handlers. They are auto discovered by the library.

## Job Persistence

In order to provide the storage mechanism for job queues, two interfaces must be implemented.

*   [IJobStorageRecord](https://github.com/FastEndpoints/FastEndpoints/blob/main/Src/Library/Messaging/Jobs/IJobStorageRecord.cs) for the job storage entity. ([See example](https://github.com/FastEndpoints/Job-Queue-Demo/blob/main/src/Storage/JobRecord.cs))
*   [IJobStorageProvider<TStorageRecord>](https://github.com/FastEndpoints/FastEndpoints/blob/main/Src/Library/Messaging/Jobs/IJobStorageProvider.cs) for the storage provider. ([See example](https://github.com/FastEndpoints/Job-Queue-Demo/blob/main/src/Storage/JobProvider.cs))

The storage record entity is simply a POCO containing the actual command DTO together with some metadata. As for the storage provider class, it simply needs to delegate data access to whatever database/storage engine that stores the jobs as shown with the MongoDB example below:

```csharp
sealed class JobStorageProvider : IJobStorageProvider<JobRecord>
{
    private readonly DbContext db;

    public JobStorageProvider(DbContext db)
    {
        this.db = db; //inject the dbContext
    }

    public Task StoreJobAsync(JobRecord job, CancellationToken ct)
    {
        // persist the provided job record to the database
        return db.SaveAsync(job, ct);
    }

    public async Task<IEnumerable<JobRecord>> GetNextBatchAsync(PendingSearchParams<JobRecord> p)
    {
        // return a batch of pending jobs to be processed next
        return await db
            .Find<JobRecord>()
            .Match(p.Match) //use the provided boolean lambda expression to match entities
            .Limit(p.Limit) //either use the provided limit or choose your own
            .ExecuteAsync(p.CancellationToken); //pass the provided cancellation token
    }

    public Task MarkJobAsCompleteAsync(JobRecord job, CancellationToken ct)
    {
        // either replace the supplied job record in the db.
        // or do a partial update of just the 'IsComplete' property.
        // or delete the entity now if batch deleting later is not preferred.
        return db
            .Update<JobRecord>()
            .MatchID(job.ID)
            .Modify(r => r.IsComplete, true)
            .ExecuteAsync(ct);
    }
    
    public  Task CancelJobAsync(Guid trackingId, CancellationToken ct)
    {
        // do a partial update of just the 'IsComplete' property.
        // or delete the entity now if batch deleting later is not preferred.
        return db.Update<JobRecord>()
                 .Match(r => r.TrackingID == trackingId)
                 .Modify(r => r.IsComplete, true)
                 .ExecuteAsync(ct);
    } 

    public Task OnHandlerExecutionFailureAsync(JobRecord job, Exception e, CancellationToken c)
    {
        // this is called whenever execution of a command's handler fails.
        // do nothing here if you'd like it to be automatically retried.
        // or update the 'ExecuteAfter' property to reschedule it to a future time.
        // or delete (or mark as complete) the entity if retry is unnecessary.
        return db
            .Update<JobRecord>()
            .MatchID(job.ID)
            .Modify(r => r.ExecuteAfter, DateTime.UtcNow.AddMinutes(1))
            .ExecuteAsync(c);
    }

    public Task PurgeStaleJobsAsync(StaleJobSearchParams<JobRecord> p)
    {
        // this method is called hourly.
        // do whatever you like with the stale (completed/expired) jobs.
        return db.DeleteAsync(p.Match, p.CancellationToken);
    }
}
```

The full source code of the above example is available on [GitHub](https://github.com/FastEndpoints/Job-Queue-Demo).

**TIP**

Using a document database such as MongoDB, LiteDB, etc. may be more suitable for implementing a job storage provider rather than using EF Core & SQL Server, as the EF Core DbContext needs additional configuration in order to support embedding command objects as well as supporting multithreading when being used as a singleton. See [this example project](https://github.com/FastEndpoints/Job-Queue-EF-Core-Demo) that shows how to configure a [pooled db context factory](https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=without-di%2Cexpression-api-with-constant), which is the recommended way to use EF Core DbContext in storage providers.

## Job Cancellations

Queued jobs can be cancelled anytime/from anywhere with its **Tracking Id**. When cancelled, the job will not be picked up for execution. If the job is already running, cancellation is requested via the cancellation token passed down to the command handler. Periodically check if the cancellation token is in a **cancellation requested** state and gracefully stop execution without throwing exceptions. See example [here](https://github.com/FastEndpoints/Job-Queue-Demo/blob/main/src/Commands/LongRunningCommand.cs).

```csharp
var trackingId = await new LongRunningCommand().QueueJobAsync();

await JobTracker<LongRunningCommand>.CancelJobAsync(trackingId);
```

Use either the **JobTracker<TCommand>** generic class or inject a **IJobTracker<TCommand>** instance from the DI Container to access the **CancelJobAsync()** method.

## Jobs With Results

A command that returns a result (**ICommand<TResult>**) can also be queued up as a job. The result can be retrieved from anywhere via the **JobTracker** using the job's **Tracking Id**. To enable support for job results, simply implement the following addon interfaces:

`JobRecord.cs`

```csharp
sealed class JobRecord : IJobStorageRecord, IJobResultStorage
{
    // ... existing properties
    public object? Result { get; set; } // a property for storing the result
}
```

`JobStorageProvider.cs`

```csharp
sealed class JobStorageProvider : IJobStorageProvider<JobRecord>, IJobResultProvider
{
    // ... existing methods

    public Task StoreJobResultAsync<TResult>(Guid trackingId, TResult result, CancellationToken ct)
    {
        // 1.) retrieve the job by trackingId.
        
        // 2.) set the result on the job like so:        
        // ((IJobResultStorage)job).SetResult(result); // Example: job.Result = result;
        
        // 3.) persist the job entity back to the database.
        return Task.CompletedTask; // Placeholder
    }

    public Task<TResult?> GetJobResultAsync<TResult>(Guid trackingId, CancellationToken ct)
    {
        // 1.) retrieve the job by trackingId.
        
        // 2.) extract the result from the job like so:
        // var result = ((IJobResultStorage)job).GetResult<TResult>(); // Example: (TResult?)job.Result;
        
        // 3.) return the result
        return Task.FromResult<TResult?>(default); // Placeholder
    }
}
```

Once the storage record entity and provider is set up, you can queue commands that return results as usual and use the job tracker to retrieve the result as follows:

```csharp
// queue the command as a job and obtain the tracking id 
var trackingId = await new MyCommand { /* ... command data ... */ }.QueueJobAsync();

// retrieve the result of the command using the tracking id
var result = await JobTracker<MyCommand>.GetJobResultAsync<MyResult>(trackingId);
```

Use either the **JobTracker<TCommand>** generic class or inject a **IJobTracker<TCommand>** instance from the DI Container to access the **GetJobResultAsync()** method. The result will be **default** for value types and **null** for reference types until the command handler completes its work. [Click here](https://github.com/FastEndpoints/Job-Queue-EF-Core-Demo) for an EF Core example.

## Tracking Job Execution Progress

With job progress tracking, the command handler can provide intermediate progress data during execution. This progress data can be retrieved from anywhere using the job's **Tracking Id**. To use this functionality, ensure the following:

*   The job storage record and storage provider must be set up to handle results as explained above.
*   Command classes must implement the **ITrackableJob<JobResult<TResult>>** interface (instead of **ICommand<TResult>**), where your actual result is wrapped in a customizable **JobResult<T>** wrapper.

An example command/job can be written like so:

`MyJob.cs`

```csharp
sealed class MyJob : ITrackableJob<JobResult<MyEndResult>>
{
    public Guid TrackingID { get; set; } // required by the interface
    public string MyName { get; set; } = string.Empty;
}

sealed class MyEndResult
{
    public string MyMessage { get; set; } = string.Empty;
}
```

The command handler for the above would look like this:

```csharp
sealed class MyJobHandler(IJobTracker<MyJob> tracker) // inject the job tracker
    : ICommandHandler<MyJob, JobResult<MyEndResult>>
{
    public async Task<JobResult<MyEndResult>> ExecuteAsync(MyJob job, CancellationToken ct)
    {
        var jobResult = new JobResult<MyEndResult>(totalSteps: 100); // set total number of steps

        for (var i = 0; i < 100; i++)
        {
            // update & store the current progress via tracker
            jobResult.CurrentStep = i;
            jobResult.CurrentStatus = $"completed step: {i}";
            await tracker.StoreJobResultAsync(job.TrackingID, jobResult, ct);
            await Task.Delay(100, ct); // Simulate work
        }

        jobResult.CurrentStatus = "all done!";
        jobResult.Result = new() { MyMessage = $"thank you {job.MyName}!" }; // set the end-result

        return jobResult; // return the job-result instance
    }
}
```

Here's an example endpoint that can be used to poll the ongoing progress of the above job and ultimately retrieve the end-result:

```csharp
sealed class JobProgressEndpoint : EndpointWithoutRequest<string>
{
    public override void Configure()
    {
        Post("job/progress/{trackingId:guid}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(CancellationToken c)
    {
        var trackingId = Route<Guid>("trackingId");
        var jobResult = await JobTracker<MyJob>
                            .GetJobResultAsync<JobResult<MyEndResult>>(trackingId, c);

        if (jobResult is null)
        {
            await SendOkAsync("job execution hasn't begun yet!");
            return;
        }

        switch (jobResult.IsComplete)
        {
            case false:
                await SendOkAsync($"[{jobResult.ProgressPercentage}%] |" +
                                   $" status: {jobResult.CurrentStatus}");
                break;
            case true:
                await SendOkAsync($"end result: {jobResult.Result.MyMessage}");
                break;
        }
    }
}
```

Working examples of all of the above can be found [here](https://github.com/FastEndpoints/Job-Queue-EF-Core-Demo).

---

Previous [<- Command Bus](/docs/command-bus)

Next [Remote Procedure Calls ->](/docs/remote-procedure-calls)

© FastEndpoints 2025