```yaml
---
title: "C# Shorts: Faster Collections — ConcurrentBag and ConcurrentDictionary | by Tiago Martins | Medium"
source: https://medium.com/@martinstm/c-shorts-faster-collections-concurrentbag-and-concurrentdictionary-1f31decf71bf
date_published: 2025-03-12T10:45:02.633Z
date_captured: 2025-08-08T12:32:51.665Z
domain: medium.com
author: Tiago Martins
category: programming
technologies: [ConcurrentBag, ConcurrentDictionary, .NET, Task Parallel Library]
programming_languages: [C#]
tags: [concurrent-collections, thread-safety, csharp, dotnet, data-structures, concurrency, multithreading, performance]
key_concepts: [thread-safety, concurrent-collections, unordered-collections, key-value-pairs, atomic-operations, producer-consumer, object-pooling, caching, race-conditions]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a concise comparison of `ConcurrentBag<T>` and `ConcurrentDictionary<TKey, TValue>`, two essential thread-safe collections in C#. It outlines the key characteristics, ideal use cases, and provides practical code examples for each. The content emphasizes when to choose one over the other, such as `ConcurrentBag` for unordered, duplicate-allowing scenarios like object pooling, and `ConcurrentDictionary` for unique key-value storage with fast concurrent lookups, suitable for caching. The article aims to guide developers in selecting the appropriate concurrent collection for their specific multithreaded programming needs.
---
```

# C# Shorts: Faster Collections — ConcurrentBag and ConcurrentDictionary | by Tiago Martins | Medium

# C# Shorts: Faster Collections — ConcurrentBag and ConcurrentDictionary

[

![Tiago Martins](https://miro.medium.com/v2/resize:fill:64:64/1*4mcEu2ft0SfxzXI-GWA56Q.jpeg)

](/@martinstm?source=post_page---byline--1f31decf71bf---------------------------------------)

[Tiago Martins](/@martinstm?source=post_page---byline--1f31decf71bf---------------------------------------)

Follow

2 min read

·

Mar 12, 2025

Listen

Share

More

Press enter or click to view image in full size

![A graphic with a blue to purple gradient background, featuring a large purple hexagon with a white C# logo inside. The text "SHORTS" is at the top, and "Small Tricks Big Gains" is at the bottom, indicating a series of concise C# tips.](https://miro.medium.com/v2/resize:fit:700/0*Y5HFKZdTVzjK_4Te.png)

Both `ConcurrentBag<T>` and `ConcurrentDictionary<TKey, TValue>` are thread-safe collections in C# but they serve different purposes. Let’s compare them in depth.

# What is a ConcurrentBag?

A thread-safe, unordered collection of objects. It’s best suited for scenarios where the order of elements doesn’t matter.

## Key Points

*   **Unordered**: Items are not stored in a specific sequence.
*   **Thread-safe**: Multiple threads can add or remove items concurrently without additional synchronization.
*   **Duplicates allowed**: You can store duplicate items.
*   **Optimized for Add/Remove**: It has low overhead for these operations.
*   **Use case**: Often used for producer-consumer scenarios where the order is irrelevant, like object pooling.

## When?

*   You need to store **unordered, duplicate values** safely across multiple threads.
*   You don’t need fast-indexed access.
*   The collection is mostly **write-heavy**, with multiple threads adding elements.

## Let’s see a code sample

```csharp
ConcurrentBag<int> bag = new();
  
Parallel.For(0, 5, i =>
{
    bag.Add(i);
});
  
foreach (var item in bag)
{
    Console.WriteLine(item);
}
  
// Output Result:
// 2
// 1
// 0
// 4
// 3
```

Every time you execute this code, the output will be different.

# What is a ConcurrentDictionary?

A thread-safe collection of key-value pairs. It ensures consistency while allowing multiple threads to access it.

## Key Points

*   **Thread-safe**: Avoids race conditions with built-in locking for modifications.
*   **Fast lookups**: Optimized for frequent read and write operations.
*   **Prevents duplicate keys**: Each key is unique.
*   **Use case**: Best for caching or storing data accessed concurrently by multiple threads.

## When?

*   You need to store **key-value pairs** with **unique keys**.
*   You perform **frequent lookups**.
*   You need **atomic updates** (e.g., `TryAdd`, `TryRemove`, `GetOrAdd`).

## Let’s see a code sample

```csharp
ConcurrentDictionary<int, string> dict = new();
  
Parallel.For(0, 5, i =>
{
    dict.TryAdd(i, $"Value{i}");
});
  
foreach (var pair in dict)
{
    Console.WriteLine($"{pair.Key}: {pair.Value}");
}
  
// Output Result:
// 0: Value0
// 1: Value1
// 2: Value2
// 3: Value3
// 4: Value4
```

# Conclusion

Use `**ConcurrentBag**` for storing unordered, **duplicate** elements in a thread-safe manner.
Use `**ConcurrentDictionary**`when you need **key-value storage** with **fast concurrent access**.