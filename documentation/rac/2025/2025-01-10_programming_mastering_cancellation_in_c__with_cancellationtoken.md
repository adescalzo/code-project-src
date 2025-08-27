```yaml
---
title: Mastering Cancellation in C# with CancellationToken
source: https://okyrylchuk.dev/blog/mastering-cancellation-in-csharp-with-cancellationtoken/
date_published: 2025-01-10T16:51:35.000Z
date_captured: 2025-08-11T16:16:29.235Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: programming
technologies: [.NET, Task Parallel Library, HttpClient]
programming_languages: [C#]
tags: [cancellation, async-await, concurrency, dotnet, task-management, asynchronous-programming, error-handling, best-practices]
key_concepts: [CancellationToken, CancellationTokenSource, asynchronous-operations, task-cancellation, polling, callbacks, linked-cancellation-tokens, OperationCanceledException]
code_examples: true
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to mastering cancellation in C# using CancellationToken. It explains how CancellationToken and CancellationTokenSource facilitate graceful termination of tasks and asynchronous operations. The post covers various cancellation methods, including synchronous, asynchronous, and timed cancellations. It also demonstrates how to listen for cancellation requests through polling and callback mechanisms, and how to link multiple cancellation tokens for complex scenarios. Finally, it outlines essential best practices for implementing robust cancellation support in .NET applications.
---
```

# Mastering Cancellation in C# with CancellationToken

# Mastering Cancellation in C# with CancellationToken

The **CancellationToken** and its related types provide developers a unified way to gracefully cancel tasks, threads, or asynchronous operations. In this blog post, we’ll explore how **CancellationToken** works and its best practices.

## **What is a CancellationToken?**

A **CancellationToken** is a struct that represents a notification to cancel an operation. It’s part of the Task Parallel Library (TPL) and is particularly useful in async/await scenarios where you must cleanly cancel long-running operations.

## **Basics of CancellationToken**

To initiate and signal a cancellation, you use the **CancellationTokenSource** class, which acts as the controller.

```csharp
using var cts = new CancellationTokenSource();

await LongAwaitedOperation(cts.Token);
```

The **CancellationTokenSource** class implements **an IDisposable** interface. So remember to call the **Dispose** method or create the CancellationTokenSource with _**using**_ keyword to call the **Dispose** method automatically to free any unmanaged resources it holds.

## **Canceling Operation**

There are three methods to cancel the operation. All methods request the operation cancellation. You can request the operation cancellation synchronously, asynchronously, and after some time.

```csharp
// Notify the operation to cancel synchronously
cts.Cancel();

// Notify the operation to cancel asynchronously
await cts.CancelAsync();

// Notify the operation to cancel after 5 seconds
cts.CancelAfter(5000);
```

You can define the timeout when the operation is canceled automatically.

```csharp
using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));
```

## **Listening the Cancellation Requests**

### Polling

You can listen for a cancellation request by periodically polling the value of the **IsCancellationRequested** property.

```csharp
async Task LongAwaitedOperation(CancellationToken token)
{
    for (int i = 0; i < 100; i++)
    {
        if (token.IsCancellationRequested)
        {
            Console.WriteLine("Operation cancelled");
            break;
        }
        await Task.Delay(1000, token);
    }
}
```

You can also use the **ThrowIfCancellationRequested** that throws the **OperationCanceledException** when the cancellation is requested.

```csharp
async Task LongAwaitedOperation(CancellationToken token)
{
    try
    {
        for (int i = 0; i < 100; i++)
        {
            token.ThrowIfCancellationRequested();
            await Task.Delay(1000, token);
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Operation cancelled");
    }
}
```

### Callback

In some operations, you cannot use polling to check if the operation has been canceled. For such scenarios, you can register a callback.

```csharp
async Task LongAwaitedOperation(CancellationToken token)
{
    HttpClient client = new();

    token.Register(() => {
        client.CancelPendingRequests();
        Console.WriteLine("Operation cancelled");
    });

    var result = await client.GetStringAsync("https://www.example.com");
}
```

## **Linking Multiple Cancellation Tokens**

Sometimes, you need to cancel an operation based on multiple conditions. For example, you want to specify an operation timeout but have external **CancellationToken** to handle.

```csharp
async Task DoWorkWithMultipleTokensAsync(CancellationToken externalToken)
{
    using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
    using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(externalToken, timeoutCts.Token);

    try
    {
        await DoWorkAsync(linkedCts.Token);
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine("Operation was cancelled or timed out");
    }
}
```

## **Best Practices**

1.  **Respect CancellationToken:** Don’t ignore the CancellationToken as a method parameter.
2.  **Dispose** **CancellationTokenSource:** Use _**using**_ keyword when creating CancellationTokenSource**.**
3.  **Don’t Create Unnecessary Sources**: If you pass through a token, don’t create a new source.
4.  **Don’t Ignore Cancellation**: Always handle OperationCanceledException appropriately.

## **Conclusion**

**CancellationToken** is an essential tool for building responsive and reliable .NET applications. By following these best practices, you can implement robust application cancellation support, leading to better resource management and user experience.

![Thumbnail for an article titled 'Records in C#'](https://okyrylchuk.dev/wp-content/uploads/2024/09/featured5.png.webp)

### [Records in C#](https://okyrylchuk.dev/blog/records-in-csharp/)

![Thumbnail for an article titled 'Pattern Matching in C#'](https://okyrylchuk.dev/wp-content/uploads/2024/09/featured6.png.webp)

### [Pattern Matching in C#](https://okyrylchuk.dev/blog/pattern-matching-in-csharp/)