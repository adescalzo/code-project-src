```yaml
---
title: When to Use Frozen Collections in .NET
source: https://okyrylchuk.dev/blog/when-to-use-frozen-collections-in-dotnet/
date_published: 2024-03-29T09:01:22.000Z
date_captured: 2025-08-20T18:59:29.788Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [.NET 8, FrozenSet, FrozenDictionary, ImmutableList, ReadOnlyCollection, List, Dictionary, HashSet, BenchmarkDotNet]
programming_languages: [C#]
tags: [dotnet, collections, performance, benchmarking, immutability, read-only, data-structures, optimization]
key_concepts: [frozen-collections, immutable-collections, read-only-collections, performance-benchmarking, lookup-optimization, collection-creation-cost, data-structures]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces .NET 8's new Frozen Collections, specifically FrozenSet and FrozenDictionary, and explains their purpose. It differentiates them from existing Immutable and ReadOnly collections, highlighting their immutability and optimization for fast lookups. Through BenchmarkDotNet, the author demonstrates that Frozen Collections offer superior lookup performance but come with a higher creation cost. The post concludes that these collections are best suited for scenarios in long-lived services where data is initialized infrequently but accessed frequently, making them valuable for runtime optimization.
---
```

# When to Use Frozen Collections in .NET

# When to Use Frozen Collections in .NET

![Oleg Kyrylchuk's avatar](https://okyrylchuk.dev/wp-content/litespeed/avatar/a85a89199a7d4486a29f67dc2fc371d0.jpg?ver=1755571532)

.NET 8 introduced Frozen Collections.

.NET already has Immutable and ReadOnly Collections, so what are the benefits of Frozen Collections?

Let’s find out!

## Frozen Collections

First, look at the new types **FrozenSet<T>** and **FrozenDictionary<TKey, TValue>**.

These are abstract types, so you can not create instances of them. However, there are extension methods for doing that.

```csharp
List<int> numbers = [1, 2, 3, 4, 5];
Dictionary<int, string> dictionary = [];
dictionary.Add(1, "One");
dictionary.Add(2, "Two");

FrozenSet<int> frozenNumbers = numbers.ToFrozenSet();
FrozenDictionary<int, string> frozenDict = dictionary.ToFrozenDictionary();
```

After collections freeze, you can’t modify their values.

The key difference is that Frozen Collections are optimized for fast lookup and enumeration. However, freezing has a relatively high cost to create such a collection.

That’s why Frozen Collections are significant for scenarios when the creation of a collection is very infrequently but is used very frequently at run-time.

We’ll see it in the benchmarks below.

But before going further, I’d like to point out one significant difference between the ReadOnly collection and the Immutable and Frozen collection.

Look at the code below.

```csharp
List<int> numbers = [1, 2, 3, 4, 5];

FrozenSet<int> frozenNumbers = numbers.ToFrozenSet();
ImmutableList<int> immutableNumbers = numbers.ToImmutableList();
ReadOnlyCollection<int> readOnlyNumbers = numbers.AsReadOnly();

numbers.Add(6);

Console.WriteLine($"Frozen: {string.Join(", ", frozenNumbers)}");
Console.WriteLine($"Immutable: {string.Join(", ", immutableNumbers)}");
Console.WriteLine($"ReadOnly: {string.Join(", ", readOnlyNumbers)}");
// Frozen: 1, 2, 3, 4, 5
// Immutable: 1, 2, 3, 4, 5
// ReadOnly: 1, 2, 3, 4, 5, 6
```

The ReadOnly collection is a wrapper for the existing collection. You can’t modify the wrapper but can change the existing collection.

## Lookup Benchmarks

Let’s check the benchmarks for looking up through different collections.

As a tool, I use the beloved [BenchmarkDotNet](https://benchmarkdotnet.org/ "BenchmarkDotNet").

The setup for benchmarking looks the following.

```csharp
[Params(100, 1000, 10_000)]
public int CollectionSize { get; set; }

private List<int> _list;
private ImmutableList<int> _immutableList;
private HashSet<int> _hashSet;
private FrozenSet<int> _frozenSet;

[GlobalSetup]
public void SetUp()
{
    _list = Enumerable.Range(0, CollectionSize).ToList();
    _immutableList = Enumerable.Range(0, CollectionSize).ToImmutableList();
    _hashSet = Enumerable.Range(0, CollectionSize).ToHashSet();
    _frozenSet = Enumerable.Range(0, CollectionSize).ToFrozenSet();
}
```

I’ve added HashSet for comparison. The benchmarks test the collections with 100, 1000, and 10 000 elements.

The benchmarks look as follows.

```csharp
[Benchmark(Baseline = true)]
public void LookupList()
{
    for (var i = 0; i < CollectionSize; i++)
        _ = _list.Contains(i);
}

[Benchmark]
public void LookupImmutableList()
{
    for (var i = 0; i < CollectionSize; i++)
        _ = _immutableList.Contains(i);
}

[Benchmark]
public void LookupHashSet()
{
    for (var i = 0; i < CollectionSize; i++)
        _ = _hashSet.Contains(i);
}

[Benchmark]
public void LookupFrozenSet()
{
    for (var i = 0; i < CollectionSize; i++)
        _ = _frozenSet.Contains(i);
}
```

Results:

![BenchmarkDotNet results showing lookup performance comparison between List, ImmutableList, HashSet, and FrozenSet for various collection sizes. FrozenSet consistently shows the lowest mean time.](https://i.imgur.com/example_lookup_benchmark_image.png)

As you can see, the FrozenSet wins even HashSet.

## Creation Benchmarks

Let’s find out how much FrozenSet creation costs compared to other collections.

```csharp
public class CreationBenchmarks
{
    private readonly int[] Numbers = Enumerable.Range(0, 1000).ToArray();

    [Benchmark(Baseline = true)]
    public List<int> ToList() => Numbers.ToList();

    [Benchmark]
    public FrozenSet<int> ToFrozenSet() => Numbers.ToFrozenSet();

    [Benchmark]
    public HashSet<int> ToHashSet() => Numbers.ToHashSet();
}
```

Results:

![BenchmarkDotNet results showing creation performance comparison between List, FrozenSet, and HashSet. FrozenSet shows a significantly higher mean time for creation.](https://i.imgur.com/example_creation_benchmark_image.png)

Here FrozenSet loses.

## Summary

In this post, we discovered that Frozen Collections are immutable and read-only, optimized for fast lookup and enumeration.

They’re suitable for scenarios where the collection is created once and used frequently in long-lived services.

The benchmarks are available on my [GitHub](https://github.com/okyrylchuk/dotnet-newsletter/tree/main/FrozenCollections "GitHub").