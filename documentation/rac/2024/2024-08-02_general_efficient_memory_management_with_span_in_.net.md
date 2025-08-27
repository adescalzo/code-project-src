```yaml
---
title: Efficient Memory Management with Span in .NET
source: https://okyrylchuk.dev/blog/efficient-memory-management-with-spans-in-dotnet/
date_published: 2024-08-02T20:39:29.000Z
date_captured: 2025-08-20T21:15:30.617Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [.NET, "Span<T>", "ReadOnlySpan<T>", "Memory<T>", "ReadOnlyMemory<T>", CollectionsMarshal, BenchmarkDotNet, SearchValues]
programming_languages: [C#]
tags: [.net, csharp, memory-management, performance, span, optimization, benchmarking, high-performance, zero-allocation, string-operations]
key_concepts: ["Span<T>", "ReadOnlySpan<T>", "Memory<T>", stack-allocation, heap-allocation, zero-allocation, memory-slicing, performance-optimization, immutability, SearchValues]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores Span<T>, a powerful .NET feature for efficient and safe memory management. It explains how Span<T> and ReadOnlySpan<T> provide views into contiguous memory regions, avoiding costly allocations and data copying. The post demonstrates creating spans from arrays, strings, and stack-allocated memory, highlighting performance gains through benchmarks against traditional string operations. It also discusses limitations, such as Span<T>'s inability to be used in classes or asynchronous operations (prior to C# 13), and introduces the .NET 8 SearchValues type for optimized string searching.
---
```

# Efficient Memory Management with Span in .NET

# Efficient Memory Management with Span in .NET

In this issue, we’re diving deep into one of the most powerful and performance-enhancing features introduced in .NET: Span<T>. Whether you’re building high-performance applications or just curious about optimizing your code, Span<T> offers a way to handle memory more efficiently and safely.

## What is Span

Span<T> and ReadOnlySpan<T> are types that can work with contiguous memory regions safely and efficiently. Unlike traditional arrays and collections, spans allow for slicing, dicing, and manipulating data without the overhead of additional allocations.

The Span types are structures, meaning they live on the stack. They don’t allocate additional memory as they are just pointers to the existing values in the memory.

However, there are Memory<T> and ReadOnlyMemory<T> types, which are heap-allocatable counterparts to Span<T> and ReadOnlySpan<T>.

You can imagine Spans as views for a memory.

![Diagram illustrating Span<T> as a view over a portion of an array, showing elements 3, 4, 5, and 6 being "spanned" within a larger array containing elements 1 through 7.](https://ci3.googleusercontent.com/meips/ADKq_NaKR7CylCzfxr4lFmf5NV1PckHPLf_rum6HOAE9Haik0W51ECC9sG770qG7fcjmnjvywiWn26lF6QzWeI2ycGHxOFQ1jG8mJqfYDQNPipcS2szj2QrJoLK26wXgifOAjlkW-Wb-i9_V_uD63E6ZCN7JLk0m85B9jOHrXt9Oj5x3tk=s0-d-e1-ft#https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1722459667768-view.png)

## How to Create Span

Creating spans in .NET is straightforward and versatile. You can create spans from various memory sources, such as arrays, stack-allocated memory, strings, and unmanaged memory.

For example, array and string types have extension methods `AsSpan` for that.

```csharp
int[] array = [1, 2, 3, 4, 5];

Span<int> arraySpan = array.AsSpan();

var message = "Hello, World";

ReadOnlySpan<char> charsSpan = message.AsSpan();
```

One of the powerful features of spans is the ability to create slices without copying data.

The `AsSpan` extension method has overloads supporting range definition, including `Index` and `Range` types.

```csharp
var message = "Hello, World";

 ReadOnlySpan<char> charsSpan1 = message.AsSpan(1);
 ReadOnlySpan<char> charsSpan2 = message.AsSpan(1, 3);
 ReadOnlySpan<char> charsSpan3 = message.AsSpan(1..3);
```

You can use `stackalloc` to allocate memory on the stack for short-lived, temporary data.

```csharp
Span<int> span = stackalloc int[5] { 1, 2, 3, 4, 5 };
```

When working with unmanaged memory, such as pointers, you can create spans using the `Span<T>` constructor that takes a pointer and length. This requires unsafe code.

```csharp
unsafe
{
    int* ptr = stackalloc int[5] { 1, 2, 3, 4, 5 };
    Span<int> span = new Span<int>(ptr, 5);
}
```

You cannot create spans directly from `List<T>` because a `Span<T>` requires a **contiguous** region of memory, and `List<T>` does not guarantee contiguous storage.

However, you can convert `List<T>` to an array and create spans. Remember that the `ToArray` method makes a copy of the list’s elements, which might be expensive for large lists.

```csharp
List<int> list = [1, 2, 3, 4, 5];
int[] array = list.ToArray();
Span<int> span = array;
```

You can avoid copying data to the array using the `CollectionsMarshal.AsSpan` method. However, this method involves some unsafe code and should be used cautiously. The `CollectionsMarshal` approach relies on the internal implementation of `List<T>`, which might change in future versions of .NET.

```csharp
List<int> list = new List<int> { 1, 2, 3, 4, 5 };
Span<int> span = CollectionsMarshal.AsSpan(list);
```

## Using Span

Spans implement `Enumerator`. You can enumerate spans like other collections. But remember that spans are not collections but views of the underlying memory.

```csharp
Span<int> span = [1, 2, 3, 4];

foreach (var item in span)
{
    Console.WriteLine(item);
}
```

We can slice the memory using the `Slice` method or `AsSpan` extension methods.

```csharp
Span<int> span = [1, 2, 3, 4];
Span<int> slice = span.Slice(0, 2);
```

As you know, any manipulation of string type in .NET creates a new string object. We often need to return a substring of a string. Let’s take a look at the very simple benchmark between `string.Substring` method and span slicing.

```csharp
[MemoryDiagnoser]
public class SpansBenchmarks
{
    public string str = "Hello, World";

    [Benchmark]
    public void Substring() => str.Substring(3, 4);

    [Benchmark]
    public void Slicing() => str.AsSpan(3, 4);
}
```

The results are mind-blowing.

![Benchmark results comparing string.Substring and Span slicing. Slicing shows significantly lower Mean time (0.0169 ns vs 2.7240 ns) and zero allocations compared to Substring (32 B).](https://ci3.googleusercontent.com/meips/ADKq_NZn5IdJgLRDnCNFe2ToEn7WdDTWqJYcXPhlRNSZtJDh33C0GnvaFAzhezvtm_ynvJ8DecpcbBvUu1X_IILyKdvcwyDhfYIBTLopgnF77hLnsx95j73juTEMIm2OappqeWeLyLtTBlR8ZQewZioAZxmavfpeE1NP0CUu65MHDqCJuA3x6uEapA-QktIcDq4m7k2sMEI=s0-d-e1-ft#https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1722494676861-substring%20benchmark.png)

The slicing is super fast, and there are 0 allocations as it does not create any objects on the heap. The span lives on the stack and is removed automatically after method execution. There is no additional job for a Garbage Collector.

Let’s examine the benchmarks in a more real-world scenario. Let’s assume we have a file with a million records to process.

```csharp
private const string FilePath = "to_process.txt";

 [GlobalSetup]
 public void Setup()
 {
     using var writer = new StreamWriter(FilePath);
     for (int i = 0; i < 1_000_000; i++)
          writer.WriteLine($"Line {i}: Some message data.");
 }
```

The benchmarks are the following.

```csharp
[Benchmark(Baseline = true)]
public void ProcessFileWithSubstring()
{
    foreach (var line in File.ReadLines(FilePath))
    {
        int separatorIndex = line.IndexOf(':');
        string linePart = line.Substring(0, separatorIndex);
        string messagePart = line.Substring(separatorIndex + 2);

        ProcessSubstring(linePart, messagePart);
    }
}

[Benchmark]
public void ProcessFileWithSpan()
{
    foreach (var line in File.ReadLines(FilePath))
    {
        ReadOnlySpan<char> lineSpan = line;

        int separatorIndex = lineSpan.IndexOf(':');
        ReadOnlySpan<char> linePart = lineSpan.Slice(0, separatorIndex);
        ReadOnlySpan<char> messagePart = lineSpan.Slice(separatorIndex + 2);

        ProcessSpans(linePart, messagePart);
    }
}
```

![Benchmark results for processing a file with Substring vs. Span. Span processing is faster (14.65 ms vs 17.65 ms) and allocates significantly less memory (15.5 MB vs 32.2 MB).](https://ci3.googleusercontent.com/meips/ADKq_NYC-2qSfVb3jDpGZ9ip-BZTQlX1kynzuiWIPK0TfAP6ZeClIxwc7L-FCIjjHEhFh8ixZtWo7xH_vkAIEZ8yIx1fT160nqbKWGk6iPVV7L7OjVYQY28gskUhaNkOU2fLnaMQR6kybJo15w0CpZKWpc_9Nk8y6yMKKhQkUj01dRQoi_j5ZzWH-Pdldui3uIxPU_-obZeV23NVow=s0-d-e1-ft#https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1722615484055-substring%20benchmark%201.png)

The processing was faster for spans. The difference is smaller this time because reading the files creates strings for every benchmark.

But take a look at the memory allocation. It’s more than two times smaller for spans!

## Limitations

You cannot use Spans in the classes or the asynchronous operations because it’s a `ref struct`. You must use `Memory<T>` and `ReadOnlyMemory<T>` for such cases.

The above statement for asynchronous operations is valid for C# 7.3 – 12 versions. However, C# 13 (in the preview when the issue was published) will allow using ref variables in async and iterator methods. But you still cannot access them across await and yield boundaries because doing so is unsafe.

```csharp
async Task AsyncMethod()
{
    await Task.Delay(1000);
    ReadOnlySpan<char> span = "Hello, World".AsSpan();
    // do something with span
}
```

## Immutability

The spans are immutable types, but it’s important to understand what it means. Let’s take a look at the example below.

```csharp
int[] array = [1, 2, 3, 4];
Span<int> span = array;
span[2] = 5;

foreach (var number in array)
    Console.WriteLine(number);
// 1 2 5 4
```

We changed the value in the array using span. So what about immutability? The span is immutable because you cannot change the span itself when it is created. You cannot change the range of spanned memory. Every slicing or trimming creates a new span.

If you want to forbid changing values using a span, use `ReadOnlySpan`. This type is returned when you create a span from a string.

One more thing. If the span exists, you must be sure the underlying memory didn’t change. For example, if the array is removed, you can read the garbage data because the span still points to the same underlying memory.

## Efficient String Searching

.NET 8 introduced the new type `SearchValues`. It provides an immutable, read-only set of values optimized for efficient searching. The following extension method overloads for Span, and ReadOnlySpan accepts new `SearchValues` type:

*   `ContainsAny`
*   `IndexOfAny`
*   `LastIndexOfAny`

```csharp
char[] chars = ['!', 'p', '&', '2', '.'];
SearchValues<string> searchValues =
    SearchValues.Create(chars);

var searchString =
    """
    Lorem Ipsum is simply dummy text of
    the printing and typesetting industry.
    """;

var index = searchString
    .AsSpan()
    .IndexOfAny(searchValues);
```

This type is developing as C# 13 will allow search by strings, not only chars.

More about `SearchValues` you can read in my post [The Power Of SearchValues in .NET](/blog/the-power-of-searchvalues-in-dotnet/).

## Summary

The spans are powerful types for handling contiguous memory efficiently.

They allow you to work directly with memory safely with zero allocations on the heap and reduce copying of data.

You can slice the part of memory that interests you. However, there are no boundary checks, so it’s your responsibility to ensure that you stay within the bounds of underlying memory.

The spans are very helpful when writing a performance code.