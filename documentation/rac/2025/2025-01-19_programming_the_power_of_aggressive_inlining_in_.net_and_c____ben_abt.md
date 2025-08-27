```yaml
---
title: "The power of Aggressive Inlining in .NET and C# | BEN ABT"
source: https://benjamin-abt.com/blog/2025/01/20/csharp_aggressive-inlining-benefit/
date_published: 2025-01-20T00:00:00.000Z
date_captured: 2025-09-03T21:50:58.786Z
domain: benjamin-abt.com
author: BEN ABT
category: programming
technologies: [.NET, C#, BenchmarkDotNet, RyuJIT, GitHub, Microsoft Azure]
programming_languages: [C#]
tags: [csharp, dotnet, performance, optimization, inlining, compiler, runtime, benchmarking, code-quality, method-impl-options]
key_concepts: [aggressive-inlining, method-call-overhead, compiler-optimization, runtime-optimization, performance-benchmarking, code-duplication, extension-methods, just-in-time-compilation]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores `MethodImplOptions.AggressiveInlining` in .NET and C#, a technique to optimize performance by instructing the compiler to directly embed method code into the calling method, thereby reducing method call overhead. It illustrates the concept with C# code examples, demonstrating how inlining can significantly improve execution speed for small, frequently invoked methods. Using `BenchmarkDotNet`, the author showcases a substantial performance gain (over 25 times faster) for an aggressively inlined string utility method compared to its non-inlined counterpart. The article concludes by advising its use for methods where call overhead outweighs the actual work, while also cautioning about potential increases in application size due to code duplication.
---
```

# The power of Aggressive Inlining in .NET and C# | BEN ABT

# The power of Aggressive Inlining in .NET and C#

![Cover image for the article on Aggressive Inlining in .NET and C#](/blog/2025/01/20/csharp_aggressive-inlining-benefit/blog-cover.png)

If you browse through certain libraries or the .NET Runtime from time to time, you will notice that the attribute [MethodImplOptions.AggressiveInlining](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.methodimploptions?view=net-8.0&WT.mc_id=DT-MVP-5001507) can be found in some places - but what is this actually?

## Method Implementations

First of all, it must be said that the C# compiler and the .NET runtime are very clever - they usually know exactly how code should be compiled and executed. But sometimes it can happen that the compiler or the runtime are not as smart as we would like them to be.

By default, a method call leads to its jump in simplified terms: it jumps from method A to method B. The following example code illustrates this:

```csharp
public class TestClass
{
    public void Check()
    {
        string myVar = "true";

        if( IsTrue( myVar ) )
        {
            Console.WriteLine( "It's true!" );
        }
        else
        {
            Console.WriteLine( "It's false!" );
        }
    }

    public static bool IsTrue(string value)
    {
        if (bool.TryParse(value, out bool boolValue))
        {
            return boolValue;
        }
        
        return false;
    }
}
```

In this example, the method `IsTrue` is called. The compiler therefore jumps from `Check` to `IsTrue` and back again. And exactly this is an overhead - and the simpler the method, the greater the share of the overhead in the runtime.

## Aggressive inlining

To counteract this overhead, we can tell the compiler not to generate the method call, but to insert the content of the method directly into the calling method, if this works. This is known as _inlining_. The compiler copies code to the appropriate places.

```csharp
public class TestClass
{
    public void Check()
    {
        string myVar = "true";

        if( bool.TryParse(value, out bool boolValue) && boolValue )
        {
            Console.WriteLine( "It's true!" );
        }
        else
        {
            Console.WriteLine( "It's false!" );
        }
    }
}
```

## Benchmark

To be able to demonstrate this at runtime, I have [written a small sample](https://github.com/BenjaminAbt/SustainableCode/blob/main/csharp/string-aggressive-inlining/) .

```csharp
public static class StringExtensions
{
    public static bool HasUnicode_NonAggressive(string source)
    {
        foreach (char c in source)
        {
            if (c > 255)
            {
                return true;
            }
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasUnicode_Aggressive(string source)
    {
        foreach (char c in source)
        {
            if (c > 255)
            {
                return true;
            }
        }

        return false;
    }
}
```

Both methods do exactly the same thing; it checks character by character whether it is a Unicode character - very simple and efficient. The only difference is the declaration of the attribute `[MethodImpl(MethodImplOptions.AggressiveInlining)]`.

The benchmark looks like this:

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90, baseline: true)]
[HideColumns(Column.Job)]
public class Benchmark
{
    [Benchmark]
    public bool HasUnicode_NonAggressive()
    {
        return StringExtensions
        .HasUnicode_NonAggressive("Hello 🌍! This is a test: 𝒜𝓁𝓅𝒽𝒶, γ, δ, ε, Ω, π, ∞, ❤️");
    }

    [Benchmark]
    public bool HasUnicode_Aggressive()
    {
        return StringExtensions
        .HasUnicode_Aggressive("Hello 🌍! This is a test: 𝒜𝓁𝓅𝒽𝒶, γ, δ, ε, Ω, π, ∞, ❤️");
    }
}
```

The result is as follows:

```shell
// * Summary *

BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.5247/22H2/2022Update)
AMD Ryzen 9 9950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK 9.0.200-preview.0.24575.35
  [Host]   : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  .NET 9.0 : .NET 9.0.0 (9.0.24.52809), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=.NET 9.0  Runtime=.NET 9.0

| Method                   | Mean      | Error     | StdDev    | Ratio | RatioSD |
|------------------------- |----------:|----------:|----------:|------:|--------:|
| HasUnicode_NonAggressive | 0.1933 ns | 0.0069 ns | 0.0065 ns |  1.00 |    0.05 |
|                          |           |           |           |       |         |
| HasUnicode_Aggressive    | 0.0073 ns | 0.0040 ns | 0.0037 ns |  1.65 |    2.19 |
```

See full code here [🌳 Sustainable Code - Aggressive Inlining 📊](https://github.com/BenjaminAbt/SustainableCode/blob/main/csharp/string-aggressive-inlining)

The example shows very impressively that the inlining variant is more than 25 times faster. This shows that the actual work of the method, i.e. checking the characters, is very fast, but the overhead of the method call still plays a huge role. A perfect example of how inlining can have a positive effect on performance - and why it is so powerful.

## When should you use it?

*   For “small” methods that are called frequently; especially something like extension classes
*   When the function call itself takes more time than the actual work of the method.

However, it should be noted that every inlining leads to the code being duplicated and the application becoming larger overall.

---

This and many other examples of efficient everyday code [https://github.com/BenjaminAbt/SustainableCode](https://github.com/BenjaminAbt/SustainableCode)

---