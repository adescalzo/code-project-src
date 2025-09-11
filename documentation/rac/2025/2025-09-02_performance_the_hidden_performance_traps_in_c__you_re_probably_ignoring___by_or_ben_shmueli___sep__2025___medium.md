```yaml
---
title: "The Hidden Performance Traps in C# You’re Probably Ignoring | by Or Ben Shmueli | Sep, 2025 | Medium"
source: https://medium.com/@orbens/the-hidden-performance-traps-in-c-youre-probably-ignoring-8d7c24f5519a
date_published: 2025-09-02T14:26:49.037Z
date_captured: 2025-09-08T11:25:13.550Z
domain: medium.com
author: Or Ben Shmueli
category: performance
technologies: [.NET runtime, LINQ, StringBuilder, Task, ValueTask, HttpClient, "Span<T>", "ReadOnlySpan<char>", "ArrayPool<T>", System.Text.Json, JsonSerializerOptions, JsonNamingPolicy, ILogger, BenchmarkDotNet]
programming_languages: [C#]
tags: [csharp, performance, optimization, dotnet, memory-management, garbage-collection, async-await, linq, concurrency, best-practices]
key_concepts: [memory-allocation, garbage-collection, asynchronous-programming, lazy-evaluation, thread-synchronization, memory-pooling, span-t, benchmarking]
code_examples: false
difficulty_level: intermediate
summary: |
  This article uncovers 20 common performance pitfalls in C# and the .NET runtime that can significantly impact application speed and resource consumption. It highlights issues such as excessive memory allocations, boxing/unboxing, inefficient LINQ usage, and improper asynchronous programming patterns. For each trap, the author provides clear explanations of why it occurs and offers practical, optimized code solutions. The content emphasizes the importance of understanding .NET internals, using appropriate data structures like `Span<T>` and `ArrayPool<T>`, and critically, the necessity of benchmarking with tools like `BenchmarkDotNet` to validate performance improvements.
---
```

# The Hidden Performance Traps in C# You’re Probably Ignoring | by Or Ben Shmueli | Sep, 2025 | Medium

# The Hidden Performance Traps in C# You’re Probably Ignoring

![Or Ben Shmueli](https://miro.medium.com/v2/resize:fill:64:64/0*m8RgOBLHT5k3_hkb.jpg)

Or Ben Shmueli

Follow

5 min read

·

5 days ago

Listen

Share

More

C# and the .NET runtime provide a modern, safe, and powerful development platform. However, performance pitfalls are everywhere — even in code that looks “clean” and “best practice.”
Some of these traps cost you milliseconds per request, others can eat up entire CPU cores in production without you realizing.

In this post, we’ll cover **real-world performance traps** that even experienced developers fall into, why they happen, and exactly how to fix them.
We’ll go deep into memory allocation patterns, JIT optimizations, concurrency bottlenecks, and framework-level quirks — with **full code examples**.

## 1. Excessive Memory Allocations

Every allocation puts pressure on the Garbage Collector (GC). The more short-lived objects you create, the more GC cycles you trigger, and the more your application stalls.
Many developers underestimate how much allocation happens “under the hood” — especially in LINQ queries, boxing/unboxing, and string manipulations.

## Example: Accidental Allocations in Loops

```csharp
for (int i = 0; i < 1_000_000; i++)
{
    var data = new string('x', 100); // Allocates 100 chars * 1M times
}
```

Even though `new string('x', 100)` looks harmless, it creates **100 million characters** in memory in this loop.
If you run this in production under load, the **GC will constantly pause** your threads to clean up.

**Better:** Reuse buffers or use `StringBuilder` if building strings incrementally:

```csharp
var sb = new StringBuilder(100);
for (int i = 0; i < 1_000_000; i++)
{
    sb.Clear();
    sb.Append('x', 100);
    var result = sb.ToString();
}
```

## 2. Boxing and Unboxing

Boxing occurs when a value type (e.g., `int`) is converted to `object` or an interface type. This forces the runtime to allocate heap memory and copy the value — a silent performance killer.

## Example: Boxing in LINQ

```csharp
var numbers = Enumerable.Range(1, 1_000_000);
var sum = numbers.Cast<object>().Count();
```

Here, **each int is boxed** into an object. That’s **1 million allocations**.

**Fix:** Avoid unnecessary type conversions. If you must work with generics, use `struct` constraints or write overloads that handle value types directly.

## 3. Overusing `async` / `await`

Async is fantastic for I/O-bound workloads, but adding `async`/`await` to every method can **increase allocations and state machine overhead**.
Every `async` method creates a hidden state machine and sometimes captures the synchronization context, which adds latency.

## Bad Example:

```csharp
public async Task<int> Add(int a, int b)
{
    return a + b;
}
```

This method is **purely CPU-bound** and doesn’t await anything. The compiler still builds a state machine for it.

**Better:** Make it synchronous:

```csharp
public int Add(int a, int b) => a + b;
```

**Tip:** Only use `async` when awaiting asynchronous operations.

## 4. Poor `ToList()` Usage in LINQ

LINQ queries are lazily evaluated. Calling `ToList()` forces **immediate materialization** of all results into memory, which may be wasteful.

## Example:

```csharp
var activeUsers = users.Where(u => u.IsActive).ToList();
```

If `users` is already an `IList<User>`, `ToList()` just allocates a **new list** unnecessarily.

**Better:** Work with the existing enumerable unless you truly need a separate collection.

## 5. String Concatenation in Loops

The `+` operator on strings creates a **new string every time** (strings are immutable).

## Bad Example:

```csharp
var result = "";
for (int i = 0; i < 1000; i++)
{
    result += i + ",";
}
```

This creates 1000 separate strings, causing **massive allocation pressure**.

**Better:** Use `StringBuilder`:

```csharp
var sb = new StringBuilder();
for (int i = 0; i < 1000; i++)
{
    sb.Append(i).Append(',');
}
```

## 6. Misusing `lock` and Causing Contention

Using `lock` on high-frequency paths can serialize operations unnecessarily.

## Example — Wrong Lock:

```csharp
private static readonly object _lock = new();
private static int _counter = 0;

public void Increment()
{
    lock (_lock)
    {
        _counter++;
    }
}
```

If `Increment()` is called millions of times across threads, they’ll all line up waiting.

**Fix — Use Interlocked:**

```csharp
public void Increment() => Interlocked.Increment(ref _counter);
```

`Interlocked` uses CPU instructions and avoids lock contention.

## 7. Using `Task.Run` Unnecessarily

`Task.Run` is for offloading CPU work. Using it for I/O or everywhere creates **thread-pool starvation**.

## Bad:

```csharp
public async Task<string> DownloadAsync(string url)
{
    return await Task.Run(() =>
    {
        using var client = new HttpClient();
        return client.GetStringAsync(url).Result;
    });
}
```

This blocks threads and wastes resources.

**Better:**

```csharp
public async Task<string> DownloadAsync(string url)
{
    using var client = new HttpClient();
    return await client.GetStringAsync(url);
}
```

## 8. Not Using `Span<T>` for Slices

Strings and arrays often get copied unnecessarily. `Span<T>` provides a view into memory without extra allocation.

## Example — Inefficient Substring:

```csharp
string input = "1234567890";
string slice = input.Substring(2, 4); // allocates new string
```

**Better:**

```csharp
ReadOnlySpan<char> span = input.AsSpan().Slice(2, 4);
```

This avoids allocations and is extremely fast.

## 9. Ignoring `ArrayPool<T>`

Allocating big arrays repeatedly kills performance. Use pooling.

## Bad:

```csharp
byte[] buffer = new byte[1024 * 1024]; // 1MB
```

**Better:**

```csharp
var pool = ArrayPool<byte>.Shared;
byte[] buffer = pool.Rent(1024 * 1024);

try
{
    // use buffer
}
finally
{
    pool.Return(buffer);
}
```

## 10. Using Exceptions for Control Flow

Exceptions are expensive. Throwing them in tight loops kills performance.

## Bad:

```csharp
foreach (var item in items)
{
    try
    {
        Process(item);
    }
    catch (InvalidOperationException)
    {
        // ignore
    }
}
```

**Better:**

```csharp
foreach (var item in items)
{
    if (IsValid(item))
        Process(item);
}
```

## 11. Misusing `IEnumerable` and Deferred Execution

LINQ chains are lazy. Accidentally re-enumerating them repeats work.

## Example:

```csharp
var query = expensiveDataSource.Where(x => x.IsActive);
var count = query.Count();    // executes
var first = query.FirstOrDefault(); // executes again
```

**Fix — Materialize Once:**

```csharp
var results = query.ToList();
var count = results.Count;
var first = results.FirstOrDefault();
```

## 12. Inefficient JSON Serialization

`System.Text.Json` is fast but creating new `JsonSerializerOptions` per call allocates.

## Bad:

```csharp
var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
});
```

**Better:**

```csharp
private static readonly JsonSerializerOptions _options =
    new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

var json = JsonSerializer.Serialize(obj, _options);
```

## 13. Async Streams Without `ConfigureAwait(false)`

Streaming async without controlling context causes overhead.

```csharp
await foreach (var line in ReadLinesAsync(file))
{
    Console.WriteLine(line);
}
```

If called in ASP.NET, it captures the SynchronizationContext for each `await`.

**Fix:**

```csharp
await foreach (var line in ReadLinesAsync(file).ConfigureAwait(false))
{
    Console.WriteLine(line);
}
```

## 14. Inefficient Dictionary Lookups

Using `ContainsKey` followed by `TryGetValue` doubles work.

## Bad:

```csharp
if (dict.ContainsKey(key))
{
    var val = dict[key];
}
```

**Better:**

```csharp
if (dict.TryGetValue(key, out var val))
{
    // use val
}
```

## 15. Not Using ValueTask

Returning `Task` always allocates. `ValueTask` avoids allocation when result is already available.

## Example:

```csharp
public Task<int> GetCachedAsync()
{
    if (_cache.TryGetValue("x", out int val))
        return Task.FromResult(val); // allocates

    return FetchFromDbAsync();
}
```

**Better:**

```csharp
public ValueTask<int> GetCachedAsync()
{
    if (_cache.TryGetValue("x", out int val))
        return new ValueTask<int>(val);

    return new ValueTask<int>(FetchFromDbAsync());
}
```

## 16. Overuse of Reflection

Reflection is slow. Avoid it in hot paths.

**Bad:**

```csharp
var prop = typeof(User).GetProperty("Name");
var val = prop.GetValue(user);
```

**Better:**

*   Use compiled expressions
*   Use `Func<User, string>` delegates
*   Or source generators

## 17. Inefficient Logging

String interpolation in logs allocates even when log level is off.

## Bad:

```csharp
_logger.LogDebug($"User {user.Name} logged in at {DateTime.Now}");
```

**Better:**

```csharp
_logger.LogDebug("User {Name} logged in at {Time}", user.Name, DateTime.Now);
```

This defers formatting until necessary.

## 18. Forgetting `ConfigureAwait(false)` in Libraries

If you’re writing a library, don’t force SynchronizationContext capture.

**Always:**

```csharp
await SomeAsyncCall().ConfigureAwait(false);
```

This reduces context switching overhead.

## 19. Premature Parallelization

Throwing everything into `Parallel.ForEach` isn’t always faster. For small workloads, the scheduling cost outweighs benefits.

Measure before using parallelism. Use `BenchmarkDotNet`.

## 20. Not Benchmarking

The ultimate performance trap: guessing. Use [BenchmarkDotNet](https://benchmarkdotnet.org/) to measure real performance.

```csharp
[MemoryDiagnoser]
public class StringConcatBench
{
    [Benchmark]
    public string Plus()
    {
        var s = "";
        for (int i = 0; i < 1000; i++) s += i;
        return s;
    }

    [Benchmark]
    public string Builder()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < 1000; i++) sb.Append(i);
        return sb.ToString();
    }
}
```