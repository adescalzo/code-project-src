```yaml
---
title: The Power of SearchValues in .NET
source: https://okyrylchuk.dev/blog/the-power-of-searchvalues-in-dotnet/
date_published: 2024-08-09T19:42:18.000Z
date_captured: 2025-08-20T21:15:18.683Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [.NET 8, .NET 9, HttpClient, BenchmarkDotNet, GitHub]
programming_languages: [C#]
tags: [dotnet, performance, optimization, search, string-manipulation, benchmarking, memory-management, simd, bloom-filter, data-structures]
key_concepts: [SearchValues, ReadOnlySpan, string-searching, performance-optimization, vector-processing, Bloom-filter, benchmarking, memory-efficiency]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces `SearchValues`, a new .NET 8 type designed for highly efficient character searching. It demonstrates how `SearchValues` significantly outperforms traditional `string.IndexOfAny` and `ReadOnlySpan<char>.IndexOfAny` methods through detailed benchmarks. The author explains that `SearchValues` achieves its speed by pre-computing and caching search optimizations, such as vector processing (SIMD) or Bloom filters, eliminating redundant calculations during multiple lookups. The post also provides a preview of the upcoming `SearchValues<string>` functionality in .NET 9, which will extend this optimization to multi-string searches. This feature is presented as a crucial tool for writing more performant and memory-efficient code in scenarios requiring frequent value lookups.
---
```

# The Power of SearchValues in .NET

# The Power of SearchValues in .NET

![Oleg Kyrylchuk's avatar](https://okyrylchuk.dev/wp-content/litespeed/avatar/a85a89199a7d4486a29f67dc2fc371d0.jpg?ver=1755571532)By [Oleg Kyrylchuk](https://okyrylchuk.dev/blog/author/kyrylchuk-oleggmail-com/) / August 9, 2024

The `SearchValues` is a new type introduced in .NET 8. It provides an immutable, read-only set of values optimized for efficient searching.

But how is it better than traditional search? How does it work? Why is it faster? Let’s dive into the details.

Table Of Contents

1.  [Intro](#intro)
2.  [Classic way](#classic-way)
3.  [Spans](#spans)
4.  [SearchValues](#searchvalues)
5.  [Search by multiple strings](#search-by-multiple-strings)
6.  [Summary](#summary)

## Intro

Text searching is often a task in programming. When we search for some characters or substrings in relatively small strings, it works fast. However, we can expect slower performance when finding a “needle” in the “haystack”.

In programming, “haystack” is often used metaphorically to describe the larger data set in which you search for a specific item, commonly called the “needle.” The phrase “finding a needle in a haystack” frequently illustrates the challenge of searching for a small, specific item within a large dataset.

For example, let’s take the book content of [Hackers, Heroes of the Computer Revolution. Chapters 1 and 2](https://www.gutenberg.org/cache/epub/729/pg729.txt).

Imagine we want to count all letters and digits from 0 to 9. That is “ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789”.

## Classic way

Let’s get a book content.

**NOTE:**
I use `.Result` instead of the `await` keyword because I use Spans later. In .NET 8, you cannot use Spans with async/await even if they are not part of the async block because Spans are `ref struct`. This issue [will be fixed in .NET 9](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-13#ref-and-unsafe-in-iterators-and-async-methods).

```csharp
string bookContent = new HttpClient()
    .GetStringAsync("https://www.gutenberg.org/cache/epub/729/pg729.txt")
    .Result;
```

Having the string and search values, we are going to use `string.IndexOfAny` method.

```csharp
[Benchmark]
public int StringIndexOfAny()
{
    char[] searchValues =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789"
        .ToArray();

    int count = 0, index = 0;
    while ((index = bookContent.IndexOfAny(searchValues, index)) >= 0)
    {
        count++;
        index++;
    }

    return count;
}
```

The mean execution time for this method is **22,336.9 μs** (microseconds). It’s fast, but we can do better.

All benchmarks are at the end of the post.

## Spans

Let’s solve the same task using `ReadOnlySpan<char>.IndexOfAny` method.

```csharp
[Benchmark]
public int SpanIndexOfAny()
{
    string searchValues
        = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
    ReadOnlySpan<char> remaining = bookContent;
    int count = 0, pos;
    while ((pos = remaining.IndexOfAny(searchValues)) >= 0)
    {
        count++;
        remaining = remaining.Slice(pos + 1);
    }

    return count;
}
```

This time, the mean execution time is **4,055.2** μs. It’s **5.5** times faster!

The thing is that the Spans `IndexOfAny` method is optimized for search.

It uses [vector processing](https://en.wikipedia.org/wiki/Vector_processor), also known as SIMD (Single Instruction, Multiple Data), if the search set is a maximum of 5 characters. Vector processing is a technique used in modern CPUs to simultaneously perform operations on multiple data points. It allows for significant performance improvements, particularly in tasks that involve large amounts of data processing.

However, if the search set is bigger than 5 characters (like in our case), it uses the [Bloom filter](https://en.wikipedia.org/wiki/Bloom_filter). In .NET, it’s a probabilistic map implementation. You can take a look at the [implementation of IndexOfAny](https://github.com/dotnet/runtime/blob/c723f067fddf39528384d65cd79e8a86197555e2/src/libraries/System.Private.CoreLib/src/System/MemoryExtensions.cs#L1997).

In simple words, it creates a bitmap. For every needle character, it sets 2 bits in that bitmap. Each character is checked against a bitmap when searching the haystack to see if both corresponding bits are set. If neither bit is set, the character can’t be part of the needle, allowing the search to proceed. However, if both bits are set, it’s probable — though not sure — that the character exists in the needle. At this point, the needle is searched for the character to confirm a match.

It’s a more efficient way to search the values. But can we do even better?

## SearchValues

.NET 8 introduced the new type `SearchValues`. It provides an immutable, read-only set of values optimized for efficient searching.

First, we need to create the instance of our search values.

```csharp
private static readonly SearchValues<char> s_searchValues
    = SearchValues.Create("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789");
```

Now, we can use them to find all our search values in the book content.

```csharp
[Benchmark(Baseline = true)]
public int Search_Values()
{
    ReadOnlySpan<char> remaining = bookContent;
    int count = 0, pos;
    while ((pos = remaining.IndexOfAny(s_searchValues)) >= 0)
    {
        count++;
        remaining = remaining.Slice(pos + 1);
    }

    return count;
}
```

The mean execution time is **582.9** μs!

All benchmarks look the following.

![Benchmark results table comparing String.IndexOfAny, Span.IndexOfAny, and SearchValues performance. SearchValues shows significantly faster execution times.](https://i.imgur.com/example_benchmark_image.png)

The `SearchValues` do the same job almost 7 times faster than `Span.IndexOfAny` and 38 times faster than the classic `string.IndexOfAny`!

But how?

In the case of Spans `IndexOfAny`, the searching algorithm first examines if the search can be vectorized; if they’re not, it builds the probabilistic map.

The problem is that these steps are done for every lookup. Over and over again.

The whole magic of `SearchValues` is to do it once and cache the result for multiple lookups.

## Search by multiple strings

In .NET 8, the `SearchValues` type is limited to searching by characters. However, in .NET 9 (in the preview when this post is published), it will be possible to search by multiple strings.

```csharp
ReadOnlySpan<string> searchWords = ["dummy", "text", "and"];
SearchValues<string> searchValues =
    SearchValues.Create(searchWords, StringComparison.OrdinalIgnoreCase);

var searchString =
    """
    Lorem Ipsum is simply dummy text of
    the printing and typesetting industry.
    """;

var index = searchString
    .AsSpan()
    .IndexOfAny(searchValues);
```

## Summary

`SearchValues` in .NET is a powerful structure that improves the efficiency of search operations. Providing a dedicated and optimized method for lookups, helps you write more performant and cleaner code, especially in scenarios where checking for multiple values is frequent.

The source code for benchmarks you can find on my [GitHub](https://github.com/okyrylchuk/dotnet-newsletter/tree/main/SearchValuesBenchmarks).