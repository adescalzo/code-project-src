```yaml
---
title: Object Pools in C# • alexeyfv
source: https://www.alexeyfv.xyz/en/post/2024-12-09-object-pool/
date_published: 2024-12-09T00:00:00.000Z
date_captured: 2025-08-19T11:25:10.491Z
domain: www.alexeyfv.xyz
author: Alexey Fedorov
category: programming
technologies: [ASP.NET Core, Microsoft.Extensions.ObjectPool, System.Buffers, BenchmarkDotNet, .NET, Astro, Giscus, NuGet]
programming_languages: [C#, JavaScript]
tags: [object-pooling, csharp, performance, design-patterns, memory-management, benchmarking, concurrency, dotnet, optimization, data-structures]
key_concepts: [Object Pool Pattern, Array Pooling, Performance Optimization, Memory Management, Concurrency, Thread Local Storage, Benchmarking, Synchronization]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive analysis of the Object Pool design pattern in C#, focusing on its implementation via `ObjectPool<T>` and `ArrayPool<T>`. It explains the internal mechanisms of `DefaultObjectPool`, `SharedArrayPool`, and `ConfigurableArrayPool`, detailing how objects are rented and returned. The author presents performance benchmarks comparing pooled and non-pooled object usage across various thread counts and object sizes. The conclusion emphasizes that while pooling can improve performance for expensive object initialization, developers must carefully evaluate its benefits in multithreaded scenarios due to potential synchronization overhead.]
---
```

# Object Pools in C# • alexeyfv

# Object Pools in C#

[C#](/en/tags/c-sharp) [ObjectPool](/en/tags/objectpool) [Design Patterns](/en/tags/design-patterns) [Performance](/en/tags/performance) [Algorithms](/en/tags/algorithms)

![A cartoon image of a swimming pool with "object" written multiple times on the water, and a beach ball, visually representing an object pool.](/_astro/cover.ktx_WJUG_Z1X3N3u.webp)

---

Index

*   [Disclaimer](#disclaimer)
*   [What is Object Pool?](#what-is-object-pool)
*   [ObjectPool class](#objectpool-class)
*   [How it works](#how-it-works)
*   [Performance](#performance)
*   [ArrayPool](#arraypool)
*   [How it works](#how-it-works-1)
    *   [SharedArrayPool](#sharedarraypool)
    *   [ConfigurableArrayPool](#configurablearraypool)
*   [Performance](#performance-1)
*   [Conclusion](#conclusion)

Object Pool is a design pattern that allows reusing objects instead of creating new ones. This can be very useful in scenarios where object initialization is expensive. It is widely used, especially in game development and applications where low memory usage is critically important. In this article, we will look at how this pattern is implemented in C# and how it can improve performance.

![An illustrative image depicting the concept of object pools, showing objects being reused.](/_astro/cover.ktx_WJUG_7IOON.webp)

_This article is presented as a part of [C# Advent 2024](https://www.csadvent.christmas/)._

# Disclaimer

The results of the benchmarks in this article are very conditional. I admit, that the benchmark may show different results on a different computer, with a different CPU, with a different compiler or in a different scenario. Always check your code in your specific conditions and don’t trust to the articles from the internet.

The source code and raw results are located in [this repo](https://github.com/alexeyfv/object-pool).

# What is Object Pool?

[Object Pool](https://en.wikipedia.org/wiki/Object_pool_pattern) is a design pattern that allows reusing objects instead of creating new ones. This can be very useful in scenarios where object initialization is expensive. The typical usage of an object pool consists of these steps:

1.  Rent an object from the pool.
2.  Use the object to perform some work.
3.  Return the object to the pool.
4.  Optionally, the object pool can reset the object’s state when it is returned.

The pseudocode for using an object pool looks like this:

```csharp
var obj = objectPool.Get();

try
{
    // do some work with obj
}
finally
{
    objectPool.Return(obj, reset: true);
}
```

The Object Pool pattern is widely used, especially in game development and applications where low memory usage is critically important.

![A screenshot of a GitHub search result page for "objectpool language:C#", showing various repositories implementing object pooling.](/_astro/image01.D9qmczA9_Z2dywQc.webp)

.NET provides several classes that implement the Object Pool pattern:

*   [ObjectPool](https://learn.microsoft.com/en-us/aspnet/core/performance/objectpool): A general-purpose object pool.
*   [ArrayPool](https://learn.microsoft.com/en-us/dotnet/api/system.buffers.arraypool-1): A class designed specifically for pooling arrays.

These classes may look similar, but their implementation is different. We will consider them separately.

# ObjectPool class

The `ObjectPool` class is available by default only in ASP.NET Core applications. You can find Its [source code](https://github.com/dotnet/aspnetcore/tree/eb68e016a554b4da50d7fb0aeffe897cfabf36c7/src/ObjectPool/src) here. For other types of C# applications, you need to install the [Microsoft.Extensions.ObjectPool package](https://www.nuget.org/packages/Microsoft.Extensions.ObjectPool/).

To use a pool, call the `Create<T>` method from the static ObjectPool class:

```csharp
var pool = ObjectPool.Create<SomeType>();
var obj = pool.Get();
```

You can also define a custom pooling policy and pass it to the `Create<T>` method. A policy lets you control how objects are created and cleaned up. For example, to reuse a list of integers, you can define the following policy:

```csharp
public class ListPolicy : IPooledObjectPolicy<List<int>>
{
    public List<int> Create() => [];

    public bool Return(List<int> obj)
    {
        obj.Clear();
        return true;
    }
}
```

Now let’s take a look at how the `ObjectPool` class works internally.

## How it works

When [retrieving an object from the pool](https://github.com/dotnet/aspnetcore/blob/eb68e016a554b4da50d7fb0aeffe897cfabf36c7/src/ObjectPool/src/DefaultObjectPool.cs#L48), the `ObjectPool` works as follows:

1.  It checks if `_fastItem` is not null and can be taken by the current thread using `Interlocked.CompareExchange`.
2.  If `_fastItem` is `null` or already taken by another thread, it tries to dequeue an object from the `ConcurrentQueue _items`.
3.  If both `_fastItem` and the queue are empty, a new object is created using the factory function.

![A diagram illustrating the internal structure of ObjectPool<T>, showing CPU cores, threads, a fast item cache (_fastItem), and a concurrent queue (_items) for object storage.](/_astro/image02.DHtSCwfy_2qEMgR.webp)

When [returning an object to the pool](https://github.com/dotnet/aspnetcore/blob/eb68e016a554b4da50d7fb0aeffe897cfabf36c7/src/ObjectPool/src/DefaultObjectPool.cs#L76), the `ObjectPool` works in an opposite way:

1.  It checks if the object passes the `_returnFunc` validation. If not, it means that the object should be discarded by [policy](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.objectpool.ipooledobjectpolicy-1?view=net-9.0-pp).
2.  If `_fastItem` is `null`, the object is stored there using `Interlocked.CompareExchange`.
3.  If `_fastItem` is already in use, the object is added to the `ConcurrentQueue` if the total number of items is within the maximum capacity.
4.  If the pool is full, the object is discarded, and the item count is adjusted.

## Performance

To test how `ObjectPool<T>` affects performance, I created [two benchmarks](https://github.com/alexeyfv/object-pool/blob/main/ObjectPoolBenchmark.cs):

*   without pooling (creates a new list for each operation);
*   with the object pool.

Each benchmark does the following in a loop:

1.  Creates a new list or rents from the pool.
2.  Adds the values in the list.
3.  Returns the list to the pool (if pooling is used).

The benchmarks repeat this process 100 times for each thread. The threads count varies from 1 to 32. The list size varies from 10 to 1,000,000.

The [results](https://github.com/alexeyfv/object-pool/tree/main/BenchmarkDotNet.Artifacts/results) are shown in the diagram below. The x-axis is a logarithmic scale, and the y-axis shows the percentage difference compared to the baseline without pooling.

![A line graph showing ObjectPool<T> benchmark results, displaying percentage difference compared to the baseline without pooling across different collection sizes and thread counts. The x-axis is logarithmic.](/_astro/image03.Dv1aMzA1_Z1A9Tj5.webp)

From the results, we can see that using `ObjectPool` in a single-thread scenario is 10% – 50% faster, compared to creating a new list for each iteration. However, in a multithreaded scenario, `ObjectPool` performs worse for relatively small objects. This is most probably due to thread synchronization latency when accessing to the `_fastItem` and `ConcurrentQueue`.

![A line graph showing ObjectPool<T> benchmark results, displaying absolute values of execution time across different collection sizes and thread counts.](/_astro/image04.C0VJRTRf_15YVwb.webp)

# ArrayPool

`ArrayPool<T>` is a class which is available from any C# application. It locates in `System.Buffers` namespace. You can find its source code [here](https://github.com/dotnet/runtime/tree/234e2f7ec5dd315014f31574719900c0715f9477/src/libraries/System.Private.CoreLib/src/System/Buffers). The `ArrayPool` class is an abstract and has 2 implemenations: [SharedArrayPool](https://github.com/dotnet/runtime/blob/234e2f7ec5dd315014f31574719900c0715f9477/src/libraries/System.Private.CoreLib/src/System/Buffers/SharedArrayPool.cs) and [ConfigurableArrayPool](https://github.com/dotnet/runtime/blob/234e2f7ec5dd315014f31574719900c0715f9477/src/libraries/System.Private.CoreLib/src/System/Buffers/ConfigurableArrayPool.cs).

The usage of `ArrayPool<T>` follows the typical object pool pattern and is quite simple. Here’s an example that uses the shared pool internally.

```csharp
var pool = ArrayPool<int>.Shared;
var buffer = pool.Rent(10);
try
{
    // do some work with array
}
finally
{
    pool.Return(buffer, clear: true);
}
```

You can also configure the pool. Static method Create returns a `ConfigurableArrayPool` instance.

```csharp
var pool = ArrayPool<int>.Create(
    maxArrayLength: 1000,
    maxArraysPerBucket: 20);
```

This method lets you specify the maximum array length and the maximum number of arrays per bucket (we’ll learn about buckets later). By default, these values are `2^20` and `50` respectively.

It’s important to note that the size of the array returned will always meet the requested size, but it may be larger:

```csharp
using System.Buffers;

var (pow, cnt) = (4, 0);
while (pow <= 30)
{
    var x = (1 << pow) - 1;
    var arr = ArrayPool<int>.Shared.Rent(x);
    Console.WriteLine(
        "Renting #{0}. Requested size: {1}. Actual size: {2}.",
        ++cnt, x, arr.Length);
    pow++;
}

// Renting #1. Requested size: 15. Actual size: 16.
// Renting #2. Requested size: 31. Actual size: 32.
// Renting #3. Requested size: 63. Actual size: 64.
// ...
// Renting #26. Requested size: 536870911. Actual size: 536870912.
// Renting #27. Requested size: 1073741823. Actual size: 1073741824.
```

## How it works

As said earlier, `ArrayPool<T>` has 2 implementations. We will consider them separately.

### SharedArrayPool

SharedArrayPool has 2 tiers of cache: per-thread and shared caches.

The per-thread cache is implemented as a private static field named `t_tlsBuckets`, which is essentially an array of arrays. Each thread gets its own instance of this cache due to [Thread Local Storage](https://en.wikipedia.org/wiki/Thread-local_storage), achieved by applying the [`ThreadStaticAttribute`](https://learn.microsoft.com/en-us/dotnet/api/system.threadstaticattribute?view=net-8.0) to the `t_tlsBuckets` field.  
This allows each thread to maintain a small cache for various array sizes, ranging from `2^4` to `2^30` (27 buckets in total).

When we’re trying to get an array from a pool, the algorithm tries to get it from `t_tlsBuckets` field. If an array of the needed size is not found in `t_tlsBuckets`, the algorithm checks the shared cache, stored in `_buckets`. This shared cache is an array of `Partitions` objects, one for each allowed bucket size (27 buckets in total). Each `Partitions` object contains an array of N `Partition` objects, where N is the number of processors. Each `Partition` works like a stack that can hold up to 32 arrays. Yeah, sounds complicated, so see the diagram below.

![A complex diagram illustrating the internal structure of SharedArrayPool<T>, showing per-thread caches (t_tlsBuckets) and shared caches (_buckets) with partitions and stacks, highlighting the multi-tiered caching mechanism.](/_astro/image05.VSAGd-Ve_Z1UqFWG.webp)

When we’re returning the array to the pool, the algorithm first tries to store it in the per-thread cache. If `t_tlsBuckets` already contains an array for the same size, the existing array from `t_tlsBuckets` is pushed into the shared cache and the new array is saved in `t_tlsBuckets` for better performance (CPU cache locality). If the current core’s stack is full, it searches for space in the stacks of other cores. If all stacks are full, the array is dropped.

### ConfigurableArrayPool

`ConfigurableArrayPool` is simpler compared to `SharedArrayPool`. It has only one private field for storing pooled arrays, called `_buckets`. This field is an array of `Bucket` instances, where each `Bucket` represents a collection of arrays (see the diagram below). Since `_buckets` field is shared across all threads, each `Bucket` [uses a SpinLock](https://github.com/dotnet/runtime/blob/234e2f7ec5dd315014f31574719900c0715f9477/src/libraries/System.Private.CoreLib/src/System/Buffers/ConfigurableArrayPool.cs#L191) for thread-safe access.

![A simpler diagram illustrating the internal structure of ConfigurableArrayPool<T>, showing _buckets containing Bucket instances that use SpinLocks for thread-safe access.](/_astro/image06.CkLfFb_M_ZpaENj.webp)

## Performance

The `ArrayPool<T>` [benchmarks](https://github.com/alexeyfv/object-pool/blob/main/ArrayPoolBenchmark.cs) are similar to the `ObjectPool<T>` benchmarks:

*   without pooling (creates a new array for each operation);
*   with the shared pool;
*   with the configurable pool.

![A line graph showing ArrayPool<T> benchmark results, displaying percentage difference compared to the baseline without pooling across different collection sizes and thread counts.](/_astro/image07.DbcJMNjH_Clln1.webp)

As we can see from the results, `SharedArrayPool` is faster almost in all cases, especially with a multiple threads scenario. The only exception is when the array size is 10.

The opposite situation with a `ConfiguratbleArrayPool`. This class has worse performance in multithreading scenario for relatively small arrays. I believe the reason is the same as in `ObjectPool<T>`: thread synchronization latency when accessing arrays inside Bucket instances.

![A line graph showing ArrayPool<T> benchmark results, displaying absolute values of execution time across different collection sizes and thread counts.](/_astro/image08.BND-FBB7_Z2vx5jp.webp)

# Conclusion

`ObjectPool` and `ArrayPool` can improve performance in scenarios where objects are expensive to create and reuse is possible. However, in multithreaded scenarios, the benefits of pooling are less clear. For small objects, the overhead of synchronization mechanisms can outweigh the performance gains. Developers should carefully benchmark and evaluate pooling in their specific use cases before integrating it into production systems.

## Related Posts

![Thumbnail image for "Tricky enum in .NET" article.](/en/post/2022-01-27-insidious-enum-in-dotnet/cover_xybQQ.webp)

Jan 31, 2022 • 5 min read

[

### Tricky enum in .NET

Limitations and issues with enums in .NET, and how to solve them using the enum class pattern

](/en/post/2022-01-27-insidious-enum-in-dotnet)

![Thumbnail image for "Functional programming in F# 1" article.](/en/post/2022-05-01-fsharp-01/cover_1HCIou.webp)

May 1, 2022 • 3 min read

[

### Functional programming in F# 1

An example of how to implement the Fibonacci sequence in F# and C#

](/en/post/2022-05-01-fsharp-01)

![Thumbnail image for "Functional programming in F# 2" article.](/en/post/2022-05-02-fsharp-02/cover_Z10gQna.webp)

May 8, 2022 • 4 min read

[

### Functional programming in F# 2

Data types, functions, and currying in F#

](/en/post/2022-05-02-fsharp-02)

![Thumbnail image for "Impact of EF mapping strategy on SQL queries performance" article.](/_astro/cover.ylu7Yi4A_Euh59.webp)

Feb 25, 2023 • 7 min read

[

### Impact of EF mapping strategy on SQL queries performance

A performance comparison of different EF mapping strategies in SQL Server

](/en/post/2023-02-25-ef-inheritance)

![Thumbnail image for "Understanding DDD: from anemic to rich domain model" article.](/en/post/2023-06-05-understanding-ddd-01/cover_SdFKv.webp)

Jun 5, 2023 • 7 min read

[

### Understanding DDD: from anemic to rich domain model

A guide to improving your code with Domain-Driven Design and Rich Domain Model

](/en/post/2023-06-05-understanding-ddd-01)

![Thumbnail image for "How to call Program.Main method with top-level statements" article.](/en/post/2023-08-10-top-level-statements/cover_WcdKm.webp)

Aug 10, 2023 • 2 min read

[

### How to call Program.Main method with top-level statements

A guide on how to call the Main method in C# programs that use top-level statements

](/en/post/2023-08-10-top-level-statements)

![Thumbnail image for "Performance issues when using a method as a parameter in C#" article.](/en/post/2023-10-18-method-as-a-parameter/cover_Z1GOEOA.webp)

Oct 18, 2023 • 3 min read

[

### Performance issues when using a method as a parameter in C#

Investigating performance issues when passing a method as a parameter in C#

](/en/post/2023-10-18-method-as-a-parameter)

![Thumbnail image for "Fastest way to extract a substring in C#" article.](/en/post/2023-10-28-substring/cover_Z1wTmI3.webp)

Oct 28, 2023 • 2 min read

[

### Fastest way to extract a substring in C#

A performance comparison of different substring extraction methods in C#

](/en/post/2023-10-28-substring)

![Thumbnail image for "What is ReadOnlySpan<T> in C# and how fast is it?" article.](/en/post/2023-12-02-readonlyspan-vs-string/cover_Z1sRk51.webp)

Dec 2, 2023 • 4 min read

[

### What is ReadOnlySpan<T> in C# and how fast is it?

A performance comparison between ReadOnlySpan<T> and string operations in C#

](/en/post/2023-12-02-readonlyspan-vs-string)

![Thumbnail image for "What's wrong with the Options pattern in C#?" article.](/en/post/2024-01-09-whats-wrong-with-options-pattern/cover_fukFa.webp)

Jan 9, 2024 • 4 min read

[

### What's wrong with the Options pattern in C#?

Analysis of the limitations of the Options pattern in C#

](/en/post/2024-01-09-whats-wrong-with-options-pattern)

© 2025 Alexey Fedorov

Powered by [Astro](https://astro.build)