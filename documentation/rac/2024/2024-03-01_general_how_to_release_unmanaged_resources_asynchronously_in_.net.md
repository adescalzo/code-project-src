```yaml
---
title: How to Release Unmanaged Resources Asynchronously in .NET
source: https://okyrylchuk.dev/blog/how-to-release-unmanaged-resources-asynchronously-in-dotnet/
date_published: 2024-03-01T21:12:37.000Z
date_captured: 2025-08-06T18:29:00.140Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [.NET, .NET Core 3, .NET 6, Microsoft.Extensions.DependencyInjection]
programming_languages: [C#]
tags: [dotnet, csharp, asynchronous, resource-management, idisposable, iasyncdisposable, dispose-pattern, memory-management, dependency-injection, unmanaged-resources]
key_concepts: [garbage-collection, unmanaged-resources, idisposable-interface, iasyncdisposable-interface, dispose-pattern, asynchronous-programming, dependency-injection-scope, resource-leak]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explains how to properly release unmanaged resources in .NET, focusing on both synchronous and asynchronous approaches. It begins by revisiting the `IDisposable` interface and the recommended Dispose pattern for synchronous resource cleanup. The core of the article introduces `IAsyncDisposable`, a .NET Core 3 feature, demonstrating its implementation with `ValueTask` and the `await using` statement. The author highlights common use cases for `IAsyncDisposable`, including streams and asynchronous dependency injection scopes, and provides a crucial warning about implementing both interfaces to prevent resource leaks.
---
```

# How to Release Unmanaged Resources Asynchronously in .NET 

By Oleg Kyrylchuk / March 1, 2024

In .NET, Garbage Collector does memory management for us. Simply, it cleans our allocated objects when they are no longer used in the application.

Some .NET objects rely on native memory. The Garbage Collector cannot clean native memory.

An IDisposable interface allows us to release native memory. The interface has a **synchronous** method, Dispose, which we should call to release the unmanaged resources.

But what if we want to dispose of unmanaged resources **asynchronously**? .NET Core 3 introduced the IAsyncDisposable interface to release native memory asynchronously.

Let’s find out how we can use it.

Table Of Contents

1.  [IDisposable](#idisposable)
2.  [IAsyncDisposable](#iasyncdisposable)
3.  [Use Cases](#use-cases)

## **IDisposable**

It’s worth recalling how to use the old IDisposable interface. If you don’t need it, you can skip this section.

The simple implementation is the following.

```csharp
class Sample : IDisposable
{
    private MemoryStream _memory;

    public Sample1() =>
        _memory = new(100);

    public void Dispose()
    {
        _memory.Dispose();
        GC.SuppressFinalize(this);
    }
}
```

The implementation is simple. However, Microsoft recommends using the [Dispose pattern](https://learn.microsoft.com/en-us/dotnet/standard/design-guidelines/dispose-pattern "Dispose pattern").

```csharp
class Sample : IDisposable
{
    private MemoryStream _memory;

    public Sample() =>
        _memory = new(100);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            _memory.Dispose();
        }
    }
}
```

We can see another Dispose protected method, but it’s virtual and takes a boolean parameter disposing. Also, the disposing logic was moved here. The original Dispose method invokes the new one passing true value.

The reason for this pattern is to handle releasing the unmanaged resources by the Dispose method and optional finalizer.

The root .NET Object has a virtual [Finalize](https://learn.microsoft.com/en-us/dotnet/api/system.object.finalize?view=net-8.0 "Finalize") method, which can be overridden to free resources before the Garbage Collector reclaims the object. C# also provides a destructor instead of overriding the Finalize method.

OK, we’ve implemented the Dispose pattern. To release unmanaged resources, we need to call the Dispose method. However, we don’t have to do that manually. We can use a statement to define a scope, and the Dispose method will be called automatically at the end of the scope.

```csharp
using var sample = new Sample();

// At the end of this block,
// the sample object is disposed.
```

## **IAsyncDisposable**

The downside of the Dispose method is that it is synchronous. The IAsyncDisposable interface comes to fill the gap.

Let’s see the implementation. It follows the Dispose pattern right away.

```csharp
class SampleAsync : IAsyncDisposable
{
    private MemoryStream? _memoryStream
        = new MemoryStream(100);

    public async ValueTask DisposeAsync()
    {
        await DisposeAsyncCore();
        GC.SuppressFinalize(this);
    }

    protected virtual async ValueTask DisposeAsyncCore()
    {
        if (_memoryStream is not null)
            await _memoryStream.DisposeAsync();

        _memoryStream = null;
    }
}
```

The IAsyncDisposable has one method to implement: DisposeAsync. It returns ValueTask. Its implementation is very similar to the Dispose method.

The DisposeAsyncCore method performs asynchronous cleanup for cascading calls to DisposeAsync. It should be virtual so the derived classes can define a custom cleanup. If the class is sealed, then the DisposeAsyncCore method is not needed.

We can also use the automatic call of the DisposeAsync method.

```csharp
await using var sampleAsync = new SampleAsync();

// At the end of this block,
// the sampleAsync object is disposed.
```

Sometimes, you should use two awaits in a single line.

```csharp
await using var transaction = await context.Database.BeginTransactionAsync();
```

Here, you await the BeginTransactionAsync method to create a transaction that returns DbTransaction, which implements IAsyncDisposable.

**The warning**!

If you implement only IAsyncDisposable and a consumer calls only the Dispose method, the DisposeAsync method will never be called. It’d lead to a resource leak. In such a case, you must implement both IDisposable and IAsyncDisposable interfaces.

## Use Cases

Many .NET APIs implement the IAsyncDisposable interface — for example, streams.

```csharp
await using var memoryStream = new MemoryStream(100);

await using var fileStream = new FileStream("file.txt", FileMode.Create);
```

.NET 6 introduced the creation of the asynchronous scope in the Dependency Injection.

```csharp
await using var provider = new ServiceCollection()
        .AddScoped<SampleAsync>()
        .BuildServiceProvider();

await using var scope = provider.CreateAsyncScope();

var sampleAsync = scope.ServiceProvider.GetRequiredService<SampleAsync>();
// At the end of this scope,
// the sampleAsync object is disposed asynchronously.
```

Many APIs for I/O operations, like database or network APIs, also implement IAsyncDisposable interfaces.

The IAsyncDisposable interface unlocks possibilities to make your code more asynchronous.