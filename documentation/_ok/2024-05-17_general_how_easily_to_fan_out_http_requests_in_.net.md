```yaml
---
title: How Easily to Fan Out HTTP Requests in .NET
source: https://okyrylchuk.dev/blog/how-to-fan-out-http-requests-in-dotnet/
date_published: 2024-05-17T19:12:19.000Z
date_captured: 2025-08-20T21:13:33.832Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [.NET, HttpClient, GitHub REST API, Task Parallel Library (TPL), .NET Framework 4, .NET 6]
programming_languages: [C#]
tags: [dotnet, http-requests, parallel-programming, asynchronous-programming, performance, web-api, task-parallel-library, fan-out, concurrency]
key_concepts: [fan-out, parallel-programming, asynchronous-programming, httpclient, task-parallel-library, parallel.foreachasync, maxdegreeofparallelism, sequential-execution]
code_examples: false
difficulty_level: intermediate
summary: |
  The article introduces the "fan out" pattern for distributing HTTP requests in .NET applications. It demonstrates how to fetch multiple GitHub user details by comparing two approaches: a classic sequential `foreach` loop and the `Parallel.ForEachAsync` method from the Task Parallel Library. The `Parallel.ForEachAsync` approach is shown to significantly improve performance by executing requests concurrently, allowing for control over the maximum degree of parallelism. This method is presented as a simple and efficient way to handle concurrent HTTP requests and other parallelizable tasks in .NET.
---
```

# How Easily to Fan Out HTTP Requests in .NET

# How Easily to Fan Out HTTP Requests in .NET

“Fan out” refers to distributing or spreading functionality or data flow from a single point to multiple points. In messaging systems, it means to send one message to many recipients. 

“Fan out” can also be used in parallel programming when tasks or computations are distributed across multiple processing units or cores to improve performance and efficiency.

Let’s look at how easily you can fan out HTTP requests in .NET. 

## The Task

Assume you have to fetch the GitHub user details via GitHub REST API. You can do it by sending a GET request for a specific username.  
   
https://api.github.com/users/okyrylchuk  
  
The API will return the details about the okyrylchuk (me) user. 

## Setup

First, we need to create a GitHub client.

```csharp
using HttpClient gitHubClient = new()
{
    BaseAddress = new Uri("https://api.github.com"),
};
```

Second, we need the list of user handlers.

```csharp
var userHandlers = new[]
{
        "users/okyrylchuk",
        "users/jaredpar",
        "users/davidfowl",
        "users/shanselman",
};
```

And the last thing is to define the response data.

```csharp
public record GitHubUser(string Name, string Bio);
```

## Classic Foreach

The classic way to fetch all user details is to run HTTP requests in the ForEach loop.

```csharp
foreach (var userHandler in userHandlers)
{
    var user = await gitHubClient.GetFromJsonAsync<GitHubUser>(userHandler);

    Console.WriteLine($"Name: {user.Name}\nBio: {user.Bio}\n");
}
```

The output:
(Screenshot showing console output of user details fetched sequentially)

All four requests have been sent one by one, and it took **975 milliseconds** overall.

## **Parallel.ForEachAsync**

You can use the [TPL](https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl "TPL ") (Task Parallel Library) to make requests parallel easily.  
  
The TPL has the Parallel.ForEach method since .NET Framework 4. It executes a foreach operation in which iterations may run in parallel. However, it is a synchronous method.

.NET 6 introduced the asynchronous version Parallel.ForEachAsync.  

Both methods take the ParallelOptions parameter, which you can use to specify the maximum degree of parallelism. It takes a maximum number of concurrent tasks.  

```csharp
ParallelOptions options = new()
{
    MaxDegreeOfParallelism = 3
};
```

And the usage is simple.

```csharp
await Parallel.ForEachAsync(userHandlers, options, async (uri, token) =>
{
    var user = await gitHubClient.GetFromJsonAsync<GitHubUser>(uri, token);
    Console.WriteLine($"Name: {user.Name}\nBio: {user.Bio}\n");
});
```

Now, all requests are sent in parallel. It took **191 milliseconds** overall! It is more than five times faster than the classic foreach loop.

## Summary

The Parallel.ForEachAsync is simple and fast. 

You can use it for HTTP requests and other tasks you can run concurrently.