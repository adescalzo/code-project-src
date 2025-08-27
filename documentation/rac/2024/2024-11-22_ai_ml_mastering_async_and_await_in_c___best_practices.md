```yaml
---
title: "Mastering Async and Await in C#: Best Practices"
source: https://okyrylchuk.dev/blog/mastering-async-and-await-in-csharp-best-practices/
date_published: 2024-11-22T16:56:51.000Z
date_captured: 2025-08-12T11:28:36.189Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: ai_ml
technologies: [ASP.NET Core, .NET]
programming_languages: [C#]
tags: [async-await, csharp, asynchronous-programming, best-practices, concurrency, performance, error-handling, dotnet]
key_concepts: [async-await-pattern, task, valuetask, cancellationtoken, synchronization-context, fire-and-forget, task-whenall, iasyncenumerable]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides essential best practices for effectively using the `async/await` pattern in C#. It highlights common pitfalls like blocking calls and `void` returns, advocating for `Task` or `Task<T>` for better error handling. The author explains the importance of `ConfigureAwait(false)` in non-UI or library code to prevent deadlocks and improve performance, noting its reduced necessity in modern ASP.NET Core. Furthermore, it covers handling cancellation with `CancellationToken`, optimizing with `ValueTask` when appropriate, and leveraging `Task.WhenAll` for efficient concurrent task execution. Finally, the article introduces `IAsyncEnumerable` as the preferred method for asynchronous data streams, ensuring cleaner, more reliable, and performant asynchronous C# code.]
---
```

# Mastering Async and Await in C#: Best Practices

# Mastering Async and Await in C#: Best Practices

The **async/await** pattern in C# simplifies asynchronous programming, making code easier to read and maintain. However, improper use can lead to bugs, performance issues, and deadlocks. Here are some best practices to ensure you’re using **async/await** effectively:

## **Use async All the Way**

Always propagate **async** methods throughout your call stack. Avoid blocking calls like **.Result** or **.Wait()** on asynchronous operations, which can cause deadlocks.

**Bad example:**

```csharp
var result = GetDataAsync().Result;
```

**Good example:**

```csharp
var result = await GetDataAsync();
```

## **Return Task, Not void**

Asynchronous methods should return **Task** or **Task<T>** except for event handlers. Using **void** makes error handling difficult because it doesn’t return a task for the caller to observe.

**Bad example:**

```csharp
async void ProcessDataAsync() {}
```

**Good example:**

```csharp
async Task ProcessDataAsync() {}
```

## **Avoid Fire-and-Forget**

Unobserved exceptions in **fire-and-forget** tasks can lead to silent failures. If you need fire-and-forget behavior, log errors or handle them explicitly.

**Bad example:**

```csharp
ProcessDataAsync(); // Task not awaited, potential issues ignored
```

**Good example:**

```csharp
ProcessDataAsync()
    .ContinueWith(t => LogError(t.Exception), TaskContinuationOptions.OnlyOnFaulted);
```

## Use ConfigureAwait(false) Where Appropriate

By default, **await** captures the current **synchronization context**, which is unnecessary in libraries or non-UI code. Use **ConfigureAwait(false)** to improve performance and prevent deadlocks.

**Example:**

```csharp
await ProcessDataAsync().ConfigureAwait(false);
```

Modern ASP.NET Core applications **do not have a synchronization context**, as the framework is designed to optimize for asynchronous operations. Because of this, using **ConfigureAwait(false)** is generally not required in ASP.NET Core apps.

If you’re writing reusable libraries (mainly ones used outside ASP.NET Core, like in desktop apps or legacy frameworks), using **ConfigureAwait(false)** is still a good practice to avoid inadvertently tying your code to a synchronization context.

## **Avoid Async Void Lambdas**

Use **async lambdas** that return **Task** instead of **void** for better exception handling.

**Bad example:**

```csharp
var _ = async () => { await ProcessDataAsync(); };
```

**Good example:**

```csharp
var _ = async () => await ProcessDataAsync();
```

## **Respect Cancellation Token**

When working with asynchronous operations, respect **CancellationToken** to allow graceful task termination.

```csharp
async Task GetDataAsync(CancellationToken token)
{
    await Task.Delay(1000, token);
}
```

## **Use ValueTask Sparingly**

**ValueTask** can optimize frequent synchronous completions but introduces complexity. Stick to Task unless performance profiling justifies **ValueTask**.

## **Use WhenAll for Concurrent Tasks**

Use **Task.WhenAll** for running many asynchronous tasks in parallel is often the **better choice** when you want to wait for multiple tasks to complete efficiently.

**Example:**

```csharp
var result1 = await DoWorkAsync(1);
var result2 = await DoWorkAsync(2);
var result3 = await DoWorkAsync(3);
```

**Better example:**

```csharp
List<Task<int>> tasks = [ 
        DoWorkAsync(1), 
        DoWorkAsync(2), 
        DoWorkAsync(3) 
    ];

var results = await Task.WhenAll(tasks);
```

## **Use IAsyncEnumerable for Asynchronous Streams**

Using **IAsyncEnumerable<T>** is often the **preferred approach** for handling asynchronous streams in C#, especially when dealing with large datasets or streams of data that arrive over time. It allows you to process data lazily and asynchronously, making your code more efficient and memory-friendly compared to alternatives like returning a **List<T>** or **Task<IEnumerable<T>>**.

**Example:**

```csharp
await foreach (var item in GetItemsAsync())
{
    Console.WriteLine(item);
}

async IAsyncEnumerable<int> GetItemsAsync()
{
    for (int i = 0; i < 10; i++)
    {
        await Task.Delay(100); // Simulate async work
        yield return i;
    }
}
```

## Summary

By following these best practices, you can write cleaner, more reliable, and performant asynchronous code in C#. Always strive for clarity and handle edge cases to ensure your code behaves as expected.

## Images
- Image 1: A dark blue background with abstract light blue and purple wavy lines. The text ".NET Pulse #5" is in the top right corner. The main title "Records in C#" is prominently displayed in yellow in the center. The website "okyrylchuk.dev" is in the bottom left corner.
- Image 2: A cropped section of a similar dark blue background with abstract light blue wavy lines. Part of the yellow text "Pattern" is visible.