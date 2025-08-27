```yaml
---
title: Performance Improvements in .NET 9 - .NET Blog
source: https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-9/
date_published: 2024-09-12T10:50:00.000Z
date_captured: 2025-08-28T16:49:02.424Z
domain: devblogs.microsoft.com
author: Stephen Toub - MSFT
category: performance
technologies: [ASP.NET Core, .NET 9, .NET 8, .NET 7, .NET 6, .NET 5, .NET Core 3.0, .NET Core 2.1, .NET Core 2.0, .NET MAUI, Android, Visual Studio, GitHub Copilot, BenchmarkDotNet, NuGet, JIT, PGO, R2R, Arm64, ARM SVE, AVX10.1, AVX512, AVX2, SSE2, Neon, Linux, Windows, macOS, GC, Mono, Native AOT, ETW, System.Threading.Lock, Interlocked, Task, System.Runtime.Intrinsics, System.Numerics.BigInteger, System.Numerics.Tensors.TensorPrimitives, System.Numerics, System.Buffers.SearchValues, System.Text.RegularExpressions.Regex, System.Buffers.Text.Base64, System.Buffers.Text.Base64Url, System.IO.Hashing, zlib-ng, System.Security.Cryptography, System.Net.Http.HttpClient, System.Net.Security.SslStream, System.Net.WebSockets.WebSocket, System.Text.Json, System.Diagnostics.Metrics.Meter, Microsoft.Extensions.DependencyInjection, Microsoft.Extensions.Caching.Memory, Microsoft.Extensions.Logging.Console, System.Drawing.ColorTranslator, System.Buffers.ArrayPool, System.Runtime.CompilerServices.Unsafe, System.Collections.Frozen, System.Collections.Concurrent, System.Collections.Generic, System.Linq, System.Text.StringBuilder, System.Net.WebUtility, System.Web.HttpUtility, System.Uri, System.Reflection, System.Runtime.InteropServices, System.Globalization, MSBuild, OpenAI tiktoken, Hyperscan, Rust aho_corasick, System.IO.Pipelines]
programming_languages: [C#, x86asm, IL, JavaScript, Visual Basic.NET, Python]
tags: [dotnet, performance, optimization, jit, gc, aot, simd, benchmarking, csharp, runtime]
key_concepts: [Profile Guided Optimization, Native AOT, SIMD, Garbage Collection, "Span<T>", "SearchValues<T>", Vectorization, Bounds Check Elimination]
code_examples: false
difficulty_level: intermediate
summary: |
  [This comprehensive article details over 350 performance improvements in .NET 9 across various core components. Key advancements include significant enhancements to the JIT compiler, such as dynamic Profile Guided Optimization (PGO) for casts and integer values, alongside extensive loop and bounds check optimizations. The runtime benefits from expanded support for Arm64, ARM SVE, AVX10.1, and AVX512 SIMD instruction sets, and major improvements to the Garbage Collector (GC) with DATAS enabled by default. Native AOT applications achieve reduced binary sizes and new whole-program optimizations. Additionally, core libraries like LINQ, `SearchValues<T>`, `Regex`, `BigInteger`, `System.Text.Json`, and networking components receive numerous allocation reductions, vectorized operations, and algorithmic enhancements, solidifying .NET 9's position as the fastest release to date.]
---
```

# Performance Improvements in .NET 9 - .NET Blog

# Performance Improvements in .NET 9

## Table of contents

*   [Benchmarking Setup](#benchmarking-setup)
*   [JIT](#jit)
*   [PGO](#pgo)
*   [Tier 0](#tier-0)
*   [Loops](#loops)
*   [Bounds Checks](#bounds-checks)
*   [Arm64](#arm64)
*   [ARM SVE](#arm-sve)
*   [AVX10.1](#avx10.1)
*   [AVX512](#avx512)
*   [Vectorization](#vectorization)
*   [Branching](#branching)
*   [Write Barriers](#write-barriers)
*   [Object Stack Allocation](#object-stack-allocation)
*   [Inlining](#inlining)
*   [GC](#gc)
*   [VM](#vm)
*   [Mono](#mono)
*   [Native AOT](#native-aot)
*   [Threading](#threading)
*   [Reflection](#reflection)
*   [Numerics](#numerics)
*   [Primitive Types](#primitive-types)
*   [BigInteger](#biginteger)
*   [TensorPrimitives](#tensorprimitives)
*   [Strings, Arrays, Spans](#strings-arrays-spans)
*   [IndexOf](#indexof)
*   [Regex](#regex)
*   [Encoding](#encoding)
*   [Span, Span, and more Span](#span-span-and-more-span)
*   [Collections](#collections)
*   [LINQ](#linq)
*   [Core Collections](#core-collections)
*   [Compression](#compression)
*   [Cryptography](#cryptography)
*   [Networking](#networking)
*   [JSON](#json)
*   [Diagnostics](#diagnostics)
*   [Peanut Butter](#peanut-butter)
*   [What’s Next?](#what’s-next)

Each year, summer arrives to find me daunted and excited to write about the performance improvements in the upcoming release of .NET. “Daunted,” because these posts, covering [.NET 8](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-8/), [.NET 7](https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7/), [.NET 6](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-6), [.NET 5](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-5), [.NET Core 3.0](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-core-3-0), [.NET Core 2.1](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-core-2-1), and [.NET Core 2.0](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-core), have garnered a bit of a reputation I want to ensure the next iteration lives up to. And “excited,” because there’s such an abundance of material to cover due to just how much goodness has been packed into the next .NET release, I struggle to get it all written down as quickly as my thoughts whirl.

And so, every year, I start these posts talking about how the next release of .NET is the fastest and best release to date. That’s true for .NET 9 as well, of course, but the statement that .NET 9 is the fastest and best release of .NET to date is now a bit… mundane. So, let’s spice it up a bit. How about… a haiku?

```text
As the falcon flies,
.NET 9 speeds joy into
Developers' hearts.
```

Or, maybe a limerick:

```text
In the coding world, there's a star,
.NET 9, the best by far.
With speed that's supreme,
It's every coder's dream,
Taking development to a new par.
```

A little gimmicky? Maybe something more classical, a sonnet perhaps:

```text
In realms of code where brilliance finds its way,
.NET 9 shines with an unmatched array.
Its speed and grace, a marvel to behold,
Transforming tasks to treasures, fast and bold.

Developers, with joy, embrace its might,
Their projects soar, efficiency in sight.
No longer bound by limits of the past,
In .NET 9, their dreams are built to last.

Its libs, a symphony of pure delight,
Turning complex to simple, dim to light.
With every line of code, a masterpiece,
In .NET 9, dev burdens find release.

Oh, wondrous .NET 9, you light the way,
In your embrace, our future's bright as day.
```

Ok, so, yeah, I should stick to writing software rather than poetry (something with which my college poetry professor likely agreed). Nevertheless, the sentiment remains: .NET 9 is an incredibly exciting release. More than 7,500 pull requests (PRs) have merged into [dotnet/runtime](https://github.com/dotnet/runtime) in the last year, of which a significant percentage have touched on performance in one way, shape, or form. In this post, we’ll take a tour through over 350 PRs that have all found their way into packing .NET 9 full of performance yumminess. Please grab a large cup of your favorite hot beverage, sit back, settle in, and enjoy.

## Benchmarking Setup

In this post, I’ve included micro-benchmarks to showcase various performance improvements. Most of these benchmarks are implemented using [BenchmarkDotNet](https://github.com/dotnet/benchmarkdotnet) [v0.14.0](https://www.nuget.org/packages/BenchmarkDotNet/0.14.0), and, unless otherwise noted, there is a simple setup for each.

To follow along, first make sure you have [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0) and [.NET 9](https://dotnet.microsoft.com/download/dotnet/9.0) installed. The numbers I share were gathered using the .NET 9 Release Candidate.

Once you have the appropriate prerequisites installed, create a new C# project in a new benchmarks directory:

```sh
dotnet new console -o benchmarks
cd benchmarks
```

The resulting directory will contain two files: `benchmarks.csproj`, which is the project file with information about how the application should be compiled, and `Program.cs`, which contains the code for the application. Replace the entire contents of `benchmarks.csproj` with this:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFrameworks>net9.0;net8.0</TargetFrameworks>
    <LangVersion>Preview</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <ServerGarbageCollection>true</ServerGarbageCollection>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="BenchmarkDotNet" Version="0.14.0" />
  </ItemGroup>

</Project>
```

The preceding project file tells the build system we want:

*   to build a runnable application, as opposed to a library.
*   to be able to run on both .NET 8 and .NET 9, so that BenchmarkDotNet can build multiple versions of the application, one to run on each version, in order to compare the results.
*   to be able to use all of the latest features from the C# language even though C# 13 hasn’t officially shipped yet.
*   to automatically import common namespaces.
*   to be able to use nullable reference type annotations in the code.
*   to be able to use the `unsafe` keyword in the code.
*   to configure the garbage collector (GC) into its “server” configuration, which impacts the trade-offs it makes between memory consumption and throughput. This isn’t required, but it’s how most services are configured.
*   to pull in `BenchmarkDotNet` v0.14.0 from NuGet so that we’re able to use the library in `Program.cs`.

For each benchmark, I’ve then included the full `Program.cs` source; to test it, just replace the entire contents of your `Program.cs` with the shown benchmark. Each test may be configured slightly differently from others, in order to highlight the key aspects being shown. For example, some tests include the `[MemoryDiagnoser(false)]` attribute, which tells BenchmarkDotNet to track allocation-related metrics, or the `[DisassemblyDiagnoser]` attribute, which tells BenchmarkDotNet to find and share the assembly code for the test, or the `[HideColumns]` attribute, which removes some output columns that BenchmarkDotNet might otherwise emit but that are unnecessary clutter for our needs in this post.

Running the benchmarks is then simple. Each test includes a comment at its top for the `dotnet` command to use to run the benchmark. It’s typically something like this:

```sh
dotnet run -c Release -f net8.0 --filter "*" --runtimes net8.0 net9.0
```

That:

*   builds the benchmarks in a Release build. Compiling for Release is important as both the C# compiler and the JIT compiler have optimizations that are disabled for Debug. Thankfully, BenchmarkDotNet warns if Debug is accidentally used:

    ```text
    // Validating benchmarks:
    //    * Assembly Benchmarks which defines benchmarks is non-optimized
    Benchmark was built without optimization enabled (most probably a DEBUG configuration). Please, build it in RELEASE.
    If you want to debug the benchmarks, please see https://benchmarkdotnet.org/articles/guides/troubleshooting.html#debugging-benchmarks.
    ```

*   targets .NET 8 for the host project. There are multiple builds involved here: the “host” application you run with the above command, which uses BenchmarkDotNet, which will in turn generate and build an application per target runtime. Because the code for the benchmark is compiled into all of these, you typically want the host project to target the oldest runtime you’ll be testing, so that building the host application will fail if you try to use an API that’s not available in all of the target runtimes.
*   runs all of the benchmarks in the whole program. If you don’t specify the `--filter` argument, BenchmarkDotNet will prompt you to ask which benchmarks to run. By specifying “\*”, we’re saying “don’t prompt, just run ’em all.” You can also specify an expression to filter down which subset of the tests you want invoked.
*   runs the tests on both .NET 8 and .NET 9.

Throughout the post, I’ve shown many benchmarks and the results I received from running them. Unless otherwise stated (e.g. because I’m demonstrating an OS-specific improvement), the results shown for benchmarks are from running them on Linux (Ubuntu 22.04) on an x64 processor.

```text
BenchmarkDotNet v0.14.0, Ubuntu 22.04.3 LTS (Jammy Jellyfish) WSL
11th Gen Intel Core i9-11950H 2.60GHz, 1 CPU, 16 logical and 8 physical cores
.NET SDK 9.0.100-rc.1.24452.12
  [Host]     : .NET 9.0.0 (9.0.24.43107), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
```

My standard caveat: these are micro-benchmarks, often measuring operations that take very short periods of time, but where improvements to those times add up to be impactful when executed over and over and over. Different hardware, different operating systems, what other processes might be running on your machine, who you had breakfast with this morning, and the alignment of the planets can all impact the numbers you get out. In short, the numbers you see are unlikely to match exactly the numbers I share here; however, I’ve chosen benchmarks that should be broadly repeatable.

With all that out of the way, let’s do this!

## JIT

Improvements in .NET show up at all levels of the stack. Some changes result in large improvements in one specific area. Other changes result in small improvements across many things. When it comes to broad-reaching impact, there are few areas of .NET that result in changes more broadly-impactful than those changes made to the Just In Time (JIT) compiler. Code generation improvements help make everything better, and it’s where we’ll start our journey.

### PGO

In [Performance Improvements in .NET 8](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-8/), I called out the enabling of dynamic profile guided optimization (PGO) as my favorite feature in the release, so PGO seems like a good place to start for .NET 9.

As a brief refresher, dynamic PGO is a feature that enables the JIT to profile code and use what it learns from that profiling to help it generate more efficient code based on the exact usage patterns of the application. The JIT utilizes tiered compilation, which allows code to be compiled and then re-compiled, possibly multiple times, achieving something new each time the code is compiled. For example, a typical method might start out at “tier 0,” where the JIT applies very few optimizations and has a goal of simply getting to functional assembly as quickly as possible. This helps with startup performance, as optimizations are one of the most costly things a compiler does. Then the runtime tracks the number of times the method is invoked, and if the number of invocations trips over a particular threshold, such that it seems like performance could actually matter, the JIT will re-generate code for it, still at tier 0, but this time with a bunch of additional instrumentation injected into the method, tracking all manner of things that could help the JIT better optimize, e.g. for a given virtual dispatch, what is the most common type on which the call is being performed. Then after enough data has been gathered, the JIT can compile the method yet again, this time at “tier 1,” fully optimized, also incorporating all of the learnings from that profile data. This same flow is relevant as well for code that’s already been pre-compiled with ReadyToRun (R2R), except instead of instrumenting tier 0 code, the JIT will generate optimized, instrumented code on its way to generating a re-optimized implementation.

In .NET 8, the JIT in particular paid attention to PGO data about types and methods involved in virtual, interface, and delegate dispatch. In .NET 9, it’s also able to use PGO data to optimize casts. Thanks to [dotnet/runtime#90594](https://github.com/dotnet/runtime/pull/90594), [dotnet/runtime#90735](https://github.com/dotnet/runtime/pull/90735), [dotnet/runtime#96597](https://github.com/dotnet/runtime/pull/96597), [dotnet/runtime#96731](https://github.com/dotnet/runtime/pull/96731), and [dotnet/runtime#97773](https://github.com/dotnet/runtime/pull/97773), dynamic PGO is now able to track the most common input types to cast operations (`castclass`/`isinst`, e.g. what you get from doing operations like `(T)obj` or `obj is T`), and then when generating the optimized code, emit special checks that add fast paths for the most common types. For example, in the following benchmark, we have a field of type `A` initialized to a type `C` that’s derived from both `B` and `A`. Then the benchmark is type checking the instance stored in that `A` field to see whether it’s a `B` or anything derived from `B`.

```csharp
// dotnet run -c Release -f net8.0 --filter "*" --runtimes net8.0 net9.0

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Tests).Assembly).Run(args);

[DisassemblyDiagnoser(maxDepth: 0)]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
public class Tests
{
    private A _obj = new C();

    [Benchmark]
    public bool IsInstanceOf() => _obj is B;

    public class A { }
    public class B : A { }
    public class C : B { }
}
```

That `IsInstanceOf` benchmark results in the following disassembly on .NET 8:

```x86asm
; Tests.IsInstanceOf()
       push      rax
       mov       rsi,[rdi+8]
       mov       rdi,offset MT_Tests+B
       call      qword ptr [7F3D91524360]; System.Runtime.CompilerServices.CastHelpers.IsInstanceOfClass(Void*, System.Object)
       test      rax,rax
       setne     al
       movzx     eax,al
       add       rsp,8
       ret
; Total bytes of code 35
```

but now on .NET 9, it produces this:

```x86asm
; Tests.IsInstanceOf()
       push      rbp
       mov       rbp,rsp
       mov       rsi,[rdi+8]
       mov       rcx,rsi
       test      rcx,rcx
       je        short M00_L00
       mov       rax,offset MT_Tests+C
       cmp       [rcx],rax
       jne       short M00_L01
M00_L00:
       test      rcx,rcx
       setne     al
       movzx     eax,al
       pop       rbp
       ret
M00_L01:
       mov       rdi,offset MT_Tests+B
       call      System.Runtime.CompilerServices.CastHelpers.IsInstanceOfClass(Void*, System.Object)
       mov       rcx,rax
       jmp       short M00_L00
; Total bytes of code 62
```

On .NET 8, it’s loading the reference to the object and the desired method token for `B`, and calling the `CastHelpers.IsInstanceOfClass` JIT helper to do the type check. On .NET 9, instead it’s loading the method token for `C`, which it saw during profiling to be the most common type used, and then comparing that against the actual object’s method token. If they match, since the JIT knows that `C` derives from `B`, it then knows the object is in fact a `B`. If they don’t match, then it jumps down to the fallback path where it does the same thing that was being done on .NET 8, loading the reference and the desired method token for `B` and calling `IsInstanceOfClass`.

It’s also capable of optimizing for the negative case where the cast most often fails. Consider this benchmark:

```csharp
// dotnet run -c Release -f net8.0 --filter "*" --runtimes net8.0 net9.0

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

BenchmarkSwitcher.FromAssembly(typeof(Tests).Assembly).Run(args);

[DisassemblyDiagnoser(maxDepth: 0)]
[HideColumns("Job", "Error", "StdDev", "Median", "RatioSD")]
public class Tests
{
    private object _obj = "hello";

    [Benchmark]
    public bool IsInstanceOf() => _obj is Tests;
}
```

On .NET 9, we get this assembly:

```x86asm
; Tests.IsInstanceOf()
       push      rbp
       mov       rbp,rsp
       mov       rsi,[rdi+8]
       mov       rcx,rsi
       test      rcx,rcx
       je        short M00_L00
       mov       rax,offset MT_System.String
       cmp       [rcx],rax