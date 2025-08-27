```yaml
---
title: "“ZLinq”, a Zero-Allocation LINQ Library for .NET | by Yoshifumi Kawai | Medium"
source: https://neuecc.medium.com/zlinq-a-zero-allocation-linq-library-for-net-1bb0a3e5c749
date_published: 2025-05-19T03:20:18.283Z
date_captured: 2025-08-06T17:50:46.133Z
domain: neuecc.medium.com
author: Yoshifumi Kawai
category: ai_ml
technologies: [ZLinq, .NET, .NET 10, .NET 9, .NET Standard 2.0, Unity, Godot, System.Linq, BenchmarkDotNet, GitHub Actions, linq.js, Reactive Extensions, UniRx, R3, LINQ to GameObject, LINQ to BigQuery, SimdLinq, ZString, ZLogger, MessagePack-CSharp, MemoryPack, Roslyn, System.Text.Json, JsonNode, Vector, LinqAF]
programming_languages: [C#, JavaScript, Rust]
tags: [linq, performance, zero-allocation, dotnet, csharp, optimization, simd, structs, source-generator, unity]
key_concepts: [zero-allocation, struct-based-linq, simd, source-generators, ref-structs, internal-iterators, linq-to-span, linq-to-tree]
code_examples: false
difficulty_level: intermediate
summary: |
  [ZLinq is a newly released, zero-allocation LINQ library for .NET that leverages structs and generics to achieve superior performance and memory efficiency compared to standard System.Linq. It offers 100% operator coverage, behavior compatibility, and advanced optimizations including SIMD support and specialized LINQ to Span and LINQ to Tree functionalities. The library is compatible with various .NET versions, Unity, and Godot, and introduces architectural innovations like `TryGetNext` and `TryCopyTo` for enhanced iteration and data access. ZLinq aims to be a practical, high-performance alternative, making .NET 9+ LINQ optimizations available across older .NET generations. The article also touches on the challenges of open-source software maintenance.]
---
```

# “ZLinq”, a Zero-Allocation LINQ Library for .NET | by Yoshifumi Kawai | Medium

# “ZLinq”, a Zero-Allocation LINQ Library for .NET

I’ve released [ZLinq](https://github.com/Cysharp/ZLinq) v1 last month! By building on structs and generics, it achieves zero allocations. It includes extensions like LINQ to Span, LINQ to SIMD, LINQ to Tree (FileSystem, JSON, GameObject, etc.), a drop-in replacement Source Generator for arbitrary types, and support for multiple platforms including .NET Standard 2.0, Unity, and Godot. It has now exceeded 2000 GitHub stars.

*   [https://github.com/Cysharp/ZLinq](https://github.com/Cysharp/ZLinq)

Struct-based LINQ itself isn’t particularly rare, and many implementations have attempted this approach over the years. However, none have been truly practical until now. They’ve typically suffered from extreme assembly size bloat, insufficient operator coverage, or performance issues due to inadequate optimization, never evolving beyond experimental status. With ZLinq, we aimed to create something practical by implementing 100% coverage of all methods and overloads in .NET 10 (including new ones like Shuffle, RightJoin, LeftJoin), ensuring 99% behavior compatibility, and implementing optimizations beyond just allocation reduction, including SIMD support, to outperform in most scenarios.

This was possible because of my extensive experience implementing LINQ. In April 2009, I released [linq.js](https://github.com/neuecc/linq.js/), a LINQ to Objects library for JavaScript (it’s wonderful to see that linq.js is still being maintained by someone who forked it!). I’ve also implemented the widely-used Reactive Extensions library [UniRx](https://github.com/neuecc/UniRx) for Unity, and recently released its evolution, [R3](https://github.com/Cysharp/R3). I’ve created variants like [LINQ to GameObject](https://assetstore.unity.com/packages/tools/integration/linq-to-gameobject-24256), [LINQ to BigQuery](https://github.com/neuecc/LINQ-to-BigQuery), and [SimdLinq](https://github.com/Cysharp/SimdLinq/). By combining these experiences with knowledge from zero-allocation related libraries ([ZString](https://github.com/Cysharp/ZString), [ZLogger](https://github.com/Cysharp/ZLogger)) and high-performance serializers ([MessagePack-CSharp](https://github.com/MessagePack-CSharp/MessagePack-CSharp/), [MemoryPack](https://github.com/Cysharp/MemoryPack)), we achieved the ambitious goal of creating a superior alternative to the standard library.

![A benchmark chart comparing memory allocations of standard LINQ (LinqWhere, LinqWhereTake, LinqWhereTakeSelect) versus ZLinq (ZLinqWhere, ZLinqWhereTake, ZLinqWhereTakeSelect). It shows standard LINQ allocating memory (48B, 104B, 160B) while ZLinq consistently allocates 0B.](https://miro.medium.com/v2/resize:fit:674/0*SDn0WRgt7viYsqrV.jpg)

This simple benchmark shows that while normal LINQ allocations increase as you chain more methods (Where, Where.Take, Where.Take.Select), ZLinq remains at zero.

Performance varies depending on the source, quantity, element type, and method chaining. To confirm that ZLinq performs better in most cases, we’ve prepared various benchmark scenarios that run on GitHub Actions: [ZLinq/actions/Benchmark](https://github.com/Cysharp/ZLinq/actions/workflows/benchmark.yaml). While there are cases where ZLinq structurally can’t win, it outperforms in most practical scenarios.

For extreme differences in benchmarks, consider repeatedly calling Select multiple times. Neither System.LINQ nor ZLinq apply special optimizations in this case, but ZLinq shows a significant performance advantage:

![A benchmark chart showing performance (Mean time in ns) for chained `Select` operations (LinqSelect1 to LinqSelect4) for both standard LINQ and ZLinq. ZLinq consistently shows lower mean times, indicating better performance.](https://miro.medium.com/v2/resize:fit:281/1*_pyu2LwhltriPXPlbNvg4w.png)

(Memory measurement 1B is BenchmarkDotNet MemoryDiagnoser errors. The documentation clearly states that [MemoryDiagnoser has an accuracy of 99.5%](https://benchmarkdotnet.org/articles/configs/diagnosers.html#restrictions), which means slight measurement errors can occur.)

In simple cases, operations that require intermediate buffers like Distinct or OrderBy show large differences because aggressive pooling significantly reduces allocations (ZLinq uses somewhat aggressive pooling since it’s primarily based on `ref struct`, which is expected to be short-lived):

![A benchmark chart comparing `Distinct` and `OrderBy` operations for System.Linq and ZLinq. ZLinq shows significantly lower mean times and zero Gen0 allocations for these operations compared to System.Linq.](https://miro.medium.com/v2/resize:fit:379/1*AmIHnJJT-svEvUCL72Hwuw.png)

LINQ applies special optimizations based on method call patterns, so reducing allocations alone isn’t enough to always outperform it. For operator chain optimizations, such as those introduced in .NET 9 and described in [Performance Improvements in .NET 9](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-9/), ZLinq implements all these optimizations to achieve even higher performance:

![A benchmark chart comparing various LINQ operator chains (e.g., `DistinctFirst`, `AppendSelectLast`, `RangeReverseCount`) for System.Linq and ZLinq. ZLinq generally shows lower mean times and zero Gen0 allocations, indicating performance improvements.](https://miro.medium.com/v2/resize:fit:563/1*fdpvCMz_vCFG_Pz7l1Ht2w.png)

A great benefit of ZLinq is that these LINQ evolution optimizations become available to all .NET generations (including .NET Framework), not just the latest versions.

Usage is simple — just add an `AsValueEnumerable()` call. Since all operators are 100% covered, replacing existing code works without issues:

```csharp
using ZLinq;  
  
var seq = source  
    .AsValueEnumerable() // only add this line  
    .Where(x => x % 2 == 0)  
    .Select(x => x * 3);  
foreach (var item in seq) { }
```

To ensure behavior compatibility, ZLinq ports System.Linq.Tests from dotnet/runtime and continuously runs them at [ZLinq/System.Linq.Tests](https://github.com/Cysharp/ZLinq/tree/main/tests/System.Linq.Tests).

![A screenshot showing the results of running System.Linq.Tests against ZLinq, indicating 9294 tests run, 89 skipped, and a run time of 4.852s, demonstrating high compatibility.](https://miro.medium.com/v2/resize:fit:559/0*g2Y4revC1BE8Pd70.png)

9000 test cases guarantee behavior (Skip cases are due to ref struct limitations where identical test code can’t be run, etc.)

Additionally, ZLinq provides a Source Generator for Drop-In Replacement that can optionally eliminate even the need for `AsValueEnumerable()`:

```csharp
[assembly: ZLinq.ZLinqDropInAttribute("", ZLinq.DropInGenerateTypes.Everything)]
```

![A code snippet demonstrating the use of `ZLinqDropInAttribute` for enabling drop-in replacement of standard LINQ with ZLinq operators.](https://miro.medium.com/v2/resize:fit:326/0*L96lrxd9neATfZMF.jpg)

This mechanism allows you to freely control the scope of the Drop-In Replacement. `ZLinq/System.Linq.Tests` itself uses Drop-In Replacement to run existing test code with ZLinq without changing the tests.

# ValueEnumerable Architecture and Optimization

For usage, please refer to the ReadMe. Here, I’ll delve deeper into optimization. The architectural distinction goes beyond simply implementing lazy sequence execution, containing many innovations compared to collection processing libraries in other languages.

The definition of `ValueEnumerable<T>`, which forms the basis of chaining, looks like this:

```csharp
public readonly ref struct ValueEnumerable<TEnumerator, T>(TEnumerator enumerator)  
    where TEnumerator : struct, IValueEnumerator<T>, allows ref struct // allows ref struct only in .NET 9 or later  
{  
    public readonly TEnumerator Enumerator = enumerator;  
}  
  
public interface IValueEnumerator<T> : IDisposable  
{  
    bool TryGetNext(out T current); // as MoveNext + Current  
    // Optimization helper  
    bool TryGetNonEnumeratedCount(out int count);  
    bool TryGetSpan(out ReadOnlySpan<T> span);  
    bool TryCopyTo(scoped Span<T> destination, Index offset);  
}
```

Based on this, operators like Where chain as follows:

```csharp
public static ValueEnumerable<Where<TEnumerator, TSource>, TSource> Where<TEnumerator, TSource>(this ValueEnumerable<TEnumerator, TSource> source, Func<TSource, Boolean> predicate)  
    where TEnumerator : struct, IValueEnumerator<TSource>, allows ref struct
```

We chose this approach rather than using `IValueEnumerable<T>` because with a definition like `(this TEnumerable source) where TEnumerable : struct, IValueEnumerable<TSource>`, type inference for `TSource` would fail. This is due to a C# language limitation where type inference doesn't work from type parameter constraints ([dotnet/csharplang#6930](https://github.com/dotnet/csharplang/discussions/6930)). If implemented with that definition, it would require defining instance methods for a vast number of combinations. [LinqAF](https://github.com/kevin-montrose/LinqAF) took that approach, resulting in [100,000+ methods and massive assembly sizes](https://kevinmontrose.com/2018/01/17/linqaf-replacing-linq-and-not-allocating/), which wasn't ideal.

In LINQ, all implementation is in `IValueEnumerator<T>`, and since all Enumerators are structs, I realized that instead of using `GetEnumerator()`, we could simply copy-pass the common `Enumerator`, allowing each Enumerator to process with its independent state. This led to the final structure of wrapping `IValueEnumerator<T>` with `ValueEnumerable<TEnumerator, T>`. This way, types appear in type declarations rather than constraints, avoiding type inference issues.

# TryGetNext

Let’s examine MoveNext, the core of iteration, in more detail:

```csharp
// Traditional interface  
public interface IEnumerator<out T> : IDisposable  
{  
    bool MoveNext();  
    T Current { get; }  
}  
  
// iterate example  
while (e.MoveNext())  
{  
    var item = e.Current; // invoke get_Current()  
}
// ZLinq interface  
public interface IValueEnumerator<T> : IDisposable  
{  
    bool TryGetNext(out T current);  
}
// iterate example  
while (e.TryGetNext(out var item))  
{  
}
```

C#’s `foreach` expands to `MoveNext() + Current`, which presents two issues. First, each iteration requires two method calls: MoveNext and get\_Current. Second, Current requires holding a variable. Therefore, I combined them into `bool TryGetNext(out T current)`. This reduces method calls to one per iteration, improving performance.

This `bool TryGetNext(out T current)` approach is also used in [Rust's iterator](https://doc.rust-lang.org/std/iter/trait.Iterator.html):

```rust
pub trait Iterator {  
    type Item;  
    // Required method  
    fn next(&mut self) -> Option<Self::Item>;  
}
```

To understand the variable holding issue, let’s look at the Select implementation:

```csharp
public sealed class LinqSelect<TSource, TResult>(IEnumerator<TSource> source, Func<TSource, TResult> selector) : IEnumerator<TResult>  
{  
    // Three fields  
    IEnumerator<TSource> source = source;  
    Func<TSource, TResult> selector = selector;  
    TResult current = default!;  
  
    public TResult Current => current;  
  
    public bool MoveNext()  
    {  
        if (source.MoveNext())  
        {  
            current = selector(source.Current);  
            return true;  
        }  
        return false;  
    }  
}  
  
public ref struct ZLinqSelect<TEnumerator, TSource, TResult>(TEnumerator source, Func<TSource, TResult> selector) : IValueEnumerator<TResult>  
    where TEnumerator : struct, IValueEnumerator<TSource>, allows ref struct  
{  
    // Two fields  
    TEnumerator source = source;  
    Func<TSource, TResult> selector = selector;  
    public bool TryGetNext(out TResult current)  
    {  
        if (source.TryGetNext(out var value))  
        {  
            current = selector(value);  
            return true;  
        }  
        current = default!;  
        return false;  
    }  
}
```

`IEnumerator<T>` requires a `current` field because it advances with `MoveNext()` and returns with `Current`. However, ZLinq advances and returns values simultaneously, eliminating the need to store the field. This makes a significant difference in ZLinq's struct-based architecture. Since ZLinq embraces a structure where each method chain encompasses the previous struct entirely (`TEnumerator` being a struct), struct size grows with each method chain. While performance remains acceptable within reasonable method chain lengths, smaller structs mean lower copy costs and better performance. The adoption of `TryGetNext` was essential to minimize struct size.

A drawback of TryGetNext is that it cannot support covariance and contravariance. However, I believe iterators and arrays should abandon covariance/contravariance support altogether. They’re incompatible with `Span<T>`, making them outdated concepts when weighing pros and cons. For example, array Span conversion can fail at runtime without compile-time detection:

```csharp
// Due to generic variance, Derived[] is accepted by Base[]  
Base[] array = new Derived[] { new Derived(), new Derived() };  
  
// In this case, casting to Span<T> or using AsSpan() causes a runtime error!  
// System.ArrayTypeMismatchException: Attempted to access an element as a type incompatible with the array.  
Span<Base> foo = array;  
class Base;  
class Derived : Base;
```

While this behavior exists because these features were added before `Span<T>`, it's problematic in modern .NET where Span is widely used, making features that can cause runtime errors practically unusable.

# TryGetNonEnumeratedCount / TryGetSpan / TryCopyTo

Naively enumerating everything doesn’t maximize performance. For example, when calling ToArray, if the size doesn’t change (e.g., `array.Select().ToArray()`), we can create a fixed-length array with `new T[count]`. System.LINQ internally uses an `Iterator<T>` type for such optimizations, but since the parameter is `IEnumerable<T>`, code like `if (source is Iterator<TSource> iterator)` is always needed.

Since ZLinq is designed specifically for LINQ from the start, we’ve prepared for these optimizations. To avoid assembly size bloat, we’ve carefully selected the minimal set of definitions that provide maximum effect, resulting in these three methods.

`TryGetNonEnumeratedCount(out int count)` succeeds when the original source has a finite count and no filtering methods (Where, Distinct, etc., though Take and Skip are calculable) intervene. This benefits ToArray and methods requiring intermediate buffers like OrderBy and Shuffle.

`TryGetSpan(out ReadOnlySpan<T> span)` potentially delivers dramatic performance improvements when the source can be accessed as contiguous memory, enabling SIMD operations or Span-based loop processing for aggregation performance.

`TryCopyTo(scoped Span<T> destination, Index offset)` enhances performance through internal iterators. To explain external vs. internal iterators, consider that `List<T>` offers both `foreach` and `ForEach`:

```csharp
// external iterator  
foreach (var item in list) { Do(item); }  
  
// internal iterator  
list.ForEach(Do);
```

They look similar but perform differently. Breaking down the implementations:

```csharp
// external iterator  
List<T>.Enumerator e = list.GetEnumerator();  
while (e.MoveNext())  
{  
    var item = e.Current;  
    Do(item);  
}  
  
// internal iterator  
for (int i = 0; i < _size; i++)  
{  
    action(_items[i]);  
}
```

This becomes a competition between delegate call overhead (+ delegate creation allocation) vs. iterator MoveNext + Current calls. The iteration speed itself is faster with internal iterators. In some cases, delegate calls may be lighter, making internal iterators potentially advantageous in benchmarks.

Of course, this varies case by case, and since lambda captures and normal control flow (like continue, break, await, etc…) aren’t available, I personally believe `ForEach` shouldn't be used, nor should custom extension methods be defined to mimic it. However, this structural difference exists.

`TryCopyTo(scoped Span<T> destination, Index offset)` achieves limited internal iteration by accepting a `Span` rather than a delegate.

Using Select as an example, for ToArray when Count is available, it passes a Span for internal iteration:

```csharp
public ref struct Select  
{  
    public bool TryCopyTo(Span<TResult> destination, Index offset)  
    {  
        if (source.TryGetSpan(out var span))  
        {  
            if (EnumeratorHelper.TryGetSlice(span, offset, destination.Length, out var slice))  
            {  
                // loop inlining  
                for (var i = 0; i < slice.Length; i++)  
                {  
                    destination[i] = selector(slice[i]);  
                }  
                return true;  
            }  
        }  
        return false;  
    }  
}  
  
// ------------------  
  
// ToArray  
if (enumerator.TryGetNonEnumeratedCount(out var count))  
{  
    var array = GC.AllocateUninitializedArray<TSource>(count);  
    // try internal iterator  
    if (enumerator.TryCopyTo(array.AsSpan(), 0))  
    {  
        return array;  
    }  
    // otherwise, use external iterator  
    var i = 0;  
    while (enumerator.TryGetNext(out var item))  
    {  
        array[i] = item;  
        i++;  
    }  
    return array;  
}
```

Thus, while Select can’t create a Span, if the original source can, processing as an internal iterator accelerates loop processing.

`TryCopyTo` differs from regular `CopyTo` by including an `Index offset` and allowing destination to be smaller than the source (normal .NET CopyTo fails if destination is smaller). This enables ElementAt representation when destination size is 1 - index 0 becomes First, ^1 becomes Last. Adding `First`, `Last`, `ElementAt` directly to `IValueEnumerator<T>` would create redundancy in class definitions (affecting assembly size), but combining small destinations with Index allows one method to cover more optimization cases:

```csharp
public static TSource ElementAt<TEnumerator, TSource>(this ValueEnumerable<TEnumerator, TSource> source, Index index)  
    where TEnumerator : struct, IValueEnumerator<TSource>, allows ref struct  
{  
    using var enumerator = source.Enumerator;  
    var value = default(TSource)!;  
    var span = new Span<T>(ref value); // create single span  
    if (enumerator.TryCopyTo(span, index))  
    {  
        return value;  
    }  
    // else...  
}
```

# LINQ to Span

In .NET 9 and above, ZLinq allows chaining all LINQ operators on `Span<T>` and `ReadOnlySpan<T>`:

```csharp
using ZLinq;  
  
// Can also be applied to Span (only in .NET 9/C# 13 environments that support allows ref struct)  
Span<int> span = stackalloc int[5] { 1, 2, 3, 4, 5 };  
var seq1 = span.AsValueEnumerable().Select(x => x * x);  
// If enables Drop-in replacement, you can call LINQ operator directly.  
var seq2 = span.Select(x => x);
```

While some libraries claim to support LINQ for Spans, they typically only define extension methods for `Span<T>` without a generic mechanism. They offer limited operators due to language constraints that previously prevented receiving `Span<T>` as a generic parameter. Generic processing became possible with the introduction of `allows ref struct` in .NET 9.

In ZLinq, there’s no distinction between `IEnumerable<T>` and `Span<T>` - they're treated equally.

However, since `allows ref struct` requires language/runtime support, while ZLinq supports all .NET versions from .NET Standard 2.0 up, Span support is limited to .NET 9 and above. This means in .NET 9+, all operators are `ref struct`, which differs from earlier versions.

# LINQ to SIMD

System.Linq accelerates certain aggregation methods with SIMD. For example, calling Sum or Max directly on primitive type arrays provides faster processing than using a for loop. However, being based on `IEnumerable<T>`, applicable types are limited. ZLinq makes this more generic through `IValueEnumerator.TryGetSpan`, targeting collections where `Span<T>` can be obtained (including direct `Span<T>` application).

Supported methods include:

*   **Range** to ToArray/ToList/CopyTo/etc…
*   **Repeat** for `unmanaged struct` and `size is power of 2` to ToArray/ToList/CopyTo/etc...
*   **Sum** for `sbyte`, `short`, `int`, `long`, `byte`, `ushort`, `uint`, `ulong`, `double`
*   **SumUnchecked** for `sbyte`, `short`, `int`, `long`, `byte`, `ushort`, `uint`, `ulong`, `double`
*   **Average** for `sbyte`, `short`, `int`, `long`, `byte`, `ushort`, `uint`, `ulong`, `double`
*   **Max** for `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `nint`, `nuint`, `Int128`, `UInt128`
*   **Min** for `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `nint`, `nuint`, `Int128`, `UInt128`
*   **Contains** for `byte`, `sbyte`, `short`, `ushort`, `int`, `uint`, `long`, `ulong`, `bool`, `char