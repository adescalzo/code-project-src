```yaml
---
title: Performance Improvements in .NET 8 - .NET Blog
source: https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-8/
date_published: 2023-09-13T12:05:00.000Z
date_captured: 2025-08-28T17:17:20.225Z
domain: devblogs.microsoft.com
author: Stephen Toub - MSFT
category: performance
technologies: [.NET 8, .NET 7, .NET 6, .NET 5, .NET Core 3.0, .NET Core 2.1, .NET Core 2.0, ASP.NET Core 8, BenchmarkDotNet, C# 12, NuGet, Bing Chat, GitHub, Windows 11, Linux, Ubuntu 22.04, x64, Arm, Arm64, AVX, AVX2, AVX512, SSE2, SSE4.2, AdvSimd, BMI2, System.Runtime.Intrinsics.X86, "Vector<T>", "Vector128<T>", "Vector256<T>", "Vector512<T>", crossgen, ReadyToRun (R2R), Roslyn, Visual Studio 2022, WSL, System.Runtime.Intrinsics.Wasm.PackedSimd, ICU, Blazor WebAssembly, System.IO.Hashing, Crc32, Crc64, XxHash32, XxHash64, XxHash3, XxHash128, System.Text.RegularExpressions, Regex, RegexInterpreter, RegexCompiler, Regex Source Generator, System.Text.Json, JsonSerializer, JsonNode, Utf8JsonWriter, JsonSerializerContext, JsonSourceGenerationOptions, System.Collections.Frozen, FrozenDictionary, FrozenSet, System.Collections.Immutable, ImmutableDictionary, ImmutableSet, ImmutableArray, Microsoft.Extensions.Logging, LoggerFactory, ILogger, LoggerMessage.Define, Logging Source Generator, Microsoft.Extensions.Configuration, ConfigurationBuilder, IConfiguration, ConfigurationBinder, Configuration Source Generator, System.ComponentModel.DataAnnotations, ValidationContext, ValidationResult, ValidationAttribute, OptionsValidator, DataAnnotationValidateOptions, System.Net.Sockets, Socket, NetworkStream, System.Net.Security, SslStream, SslServerAuthenticationOptions, SslStreamCertificateContext, HttpClient, SocketsHttpHandler, HttpMessageInvoker, YARP, Windows CNG, bcrypt.dll, ncrypt.dll, System.Formats.Asn1, AsnWriter, AsnReader, ArrayPool, System.TimeProvider, FakeTimeProvider, System.Threading.Lock, System.Threading.PortableThreadPool, System.Threading.WindowsThreadPool, System.Buffers.SearchValues, System.Text.Ascii, System.Buffers.Text.Base64, System.Buffers.Text.Utf8Formatter, System.Uri, System.Web.HttpUtility, System.Text.StringBuilder, System.IO.MemoryStream, System.Reflection.Metadata, Microsoft.Win32.Registry, System.Xml, System.Runtime.Serialization.Xml, System.Linq.Expressions, System.Collections.Concurrent.ConcurrentDictionary, System.Collections.Generic.List, Queue, Stack, LinkedList, PriorityQueue, SortedDictionary, SortedList, HashSet, Dictionary, System.Collections.ObjectModel.ReadOnlyCollection, ReadOnlyDictionary, ReadOnlyObservableCollection, System.Collections.ConditionalWeakTable, System.Diagnostics.Tracing.EventListener, System.Diagnostics.Stopwatch, System.Security.Cryptography, RSA, ECDsa, SHA256, RandomNumberGenerator, System.Security.Cryptography.X509Certificates, X509Certificate2, CertificateRequest, System.Numerics.BitOperations, System.Runtime.CompilerServices.UnsafeAccessor, System.Runtime.InteropServices.ImmutableCollectionsMarshal, System.Runtime.CompilerServices.InlineArrayAttribute, System.Runtime.CompilerServices.CollectionBuilderAttribute, System.Runtime.CompilerServices.InterceptsLocationAttribute]
programming_languages: [C#, Assembly, IL, WASM]
tags: [performance-tuning, dotnet, csharp, jit-compiler, aot-compilation, memory-optimization, vectorization, runtime, benchmarking, code-generation]
key_concepts: [Dynamic PGO, Tiered Compilation, Native AOT, Vectorization, Constant Folding, Bounds Check Elimination, Garbage Collection, Source Generators, Span-based APIs]
code_examples: true
difficulty_level: advanced
summary: |
  This extensive article details the significant performance improvements introduced in .NET 8 across various components of the runtime and libraries. Key enhancements include dynamic Profile-Guided Optimization (PGO) being enabled by default, leading to smarter JIT compilation and devirtualization. Vectorization capabilities are greatly expanded with AVX512 support, accelerating data processing in core types and algorithms. The article also highlights substantial reductions in Native AOT application size, numerous optimizations for primitive types, strings, spans, collections, and networking, as well as new features like `SearchValues` and `FrozenCollections`. These improvements aim to boost throughput, reduce memory allocation, and enhance startup times for a wide range of .NET applications.
---
```

# Performance Improvements in .NET 8 - .NET Blog

September 13th, 2023

# Performance Improvements in .NET 8

![Stephen Toub - MSFT](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2022/03/stoub_square-96x96.jpg)

[Stephen Toub - MSFT](https://devblogs.microsoft.com/dotnet/author/toub)

Partner Software Engineer

I look forward to summer every year. Sun, beach, warm nights, and putting the finishing touches on the next version of .NET. It’s also the time I get to continue a tradition I started for myself back in 2017 of writing about the performance improvements that have gone into the latest .NET incarnation. A year ago that was [Performance Improvements in .NET 7](https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7/), which followed similar posts for [.NET 6](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-6), [.NET 5](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-5), [.NET Core 3.0](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-core-3-0), [.NET Core 2.1](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-core-2-1), and [.NET Core 2.0](https://devblogs.microsoft.com/dotnet/performance-improvements-in-net-core).

Since [.NET 7](https://devblogs.microsoft.com/dotnet/performance_improvements_in_net_7/) was released a year ago, you’ve likely been inundated with news about AI and ChatGPT. You may have even invested effort in using AI and/or [incorporated it into your applications](https://devblogs.microsoft.com/dotnet/demystifying-retrieval-augmented-generation-with-dotnet/). And I got to wondering: could AI write this blog post for me? So I issued the following request to [Bing Chat](http://bing.com/chat):

> “You are an expert .NET developer with a penchant for pedagogy and a love of details. Write a treatise covering the most important performance improvements in .NET 8. The article should educate developers not just about what improvements were made, but also about why and how they were made so that developers can learn the relevant techniques and apply them in their own codebases. The article should also excite developers about the upcoming release, highlighting the very real benefits to upgrading their services, applications, and libraries to the new version. And the article should give credit to the myriad of developers that contributed significantly to the release, calling out the hundreds upon hundreds of pull requests that were merged to make .NET 8 the fastest .NET to date. The article should follow the style of the post about .NET 7 performance at https://devblogs.microsoft.com/dotnet/performance\_improvements\_in\_net\_7/. Based on the extent and depth of everything to cover, it is estimated that the article should be around 200 pages in length, including all code samples and exposition.”

And the response?

> “I’m sorry, but I can’t write a 200-page treatise for you. That’s beyond the scope of my capabilities and my chat settings. I can only generate short and concise content, such as summaries, snippets, or paragraphs. If you want to learn more about the performance improvements in .NET 8, you can check out some of the web search results I found for you. You can also read the official release notes and the GitHub issue tracker for more details. I hope you understand and appreciate my limitations. Thank you for your interest in .NET 8. 😊”

Fair enough (especially since, after writing this post and measuring the token count, it’s north of 300,000). And I’m happy with that outcome, because it means I still get to have the fun of writing this myself.

Throughout the past year, as I was reviewing PRs in various .NET repos, I maintained a list of all the PRs that I might want to cover in this post, which is focused on the core runtime and libraries ([Performance Improvements in ASP.NET Core 8](https://devblogs.microsoft.com/dotnet/performance-improvements-in-aspnet-core-8/) provides an in-depth focus on ASP.NET). And as I sat down to write this, I found myself staring at a daunting list of 1289 links. This post can’t cover all of them, but it does take a tour through more than 500 PRs, all of which have gone into making .NET 8 an irresistible release, one I hope you’ll all upgrade to as soon as humanly possible.

.NET 7 was super fast. .NET 8 is faster.

## Benchmarking Setup

Throughout this post, I include microbenchmarks to highlight various aspects of the improvements being discussed. Most of those benchmarks are implemented using [BenchmarkDotNet](https://benchmarkdotnet.org/) [v0.13.8](https://www.nuget.org/packages/BenchmarkDotNet/0.13.8), and, unless otherwise noted, there is a simple setup for each of these benchmarks.

To follow along, first make sure you have [.NET 7](https://dotnet.microsoft.com/download/dotnet/7.0?wt.mc_id=net8perf) and [.NET 8](https://dotnet.microsoft.com/download/dotnet/8.0?wt.mc_id=net8perf) installed. For this post, I’ve used the .NET 8 Release Candidate (8.0.0-rc.1.23419.4).

With those prerequisites taken care of, create a new C# project in a new `benchmarks` directory:

```sh
dotnet new console -o benchmarks
cd benchmarks
```

That directory will contain two files: `benchmarks.csproj` (the project file with information about how the application should be built) and `Program.cs` (the code for the application). Replace the entire contents of `benchmarks.csproj` with this:

```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFrameworks>net8.0;net7.0</TargetFrameworks>
    <LangVersion>Preview</LangVersion>
    <ImplicitUsings>enable</ImplicitUsings>
    <AllowUnsafeBlocks>true</AllowUnsafeBlocks>
    <ServerGarbageCollection>true</ServerGarbageCollection>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="BenchmarkDotNet" Version="0.13.8" />
  </ItemGroup>

</Project>
```

The preceding project file tells the build system we want:

*   to build a runnable application (as opposed to a library),
*   to be able to run on both .NET 8 and .NET 7 (so that BenchmarkDotNet can run multiple processes, one with .NET 7 and one with .NET 8, in order to be able to compare the results),
*   to be able to use all of the latest features from the C# language even though C# 12 hasn’t officially shipped yet,
*   to automatically import common namespaces,
*   to be able to use the `unsafe` keyword in the code,
*   and to configure the garbage collector (GC) into its “server” configuration, which impacts the tradeoffs it makes between memory consumption and throughput (this isn’t strictly necessary, I’m just in the habit of using it, and it’s the default for ASP.NET apps.)

The `<PackageReference/>` at the end pulls in BenchmarkDotNet from [NuGet](https://www.nuget.org/) so that we’re able to use the library in `Program.cs`. (A handful of benchmarks require additional packages be added; I’ve noted those where applicable.)

For each benchmark, I’ve then included the full `Program.cs` source; just copy and paste that code into `Program.cs`, replacing its entire contents. In each test, you’ll notice several attributes may be applied to the `Tests` class. The `[MemoryDiagnoser]` attribute indicates I want it to track managed allocation, the `[DisassemblyDiagnoser]` attribute indicates I want it to report on the actual assembly code generated for the test (and by default one level deep of functions invoked by the test), and the `[HideColumns]` attribute simply suppresses some columns of data BenchmarkDotNet might otherwise emit by default but are unnecessary for our purposes here.

Running the benchmarks is then straightforward. Each shown test also includes a comment at the beginning for the `dotnet` command to run the benchmark. Typically, it’s something like this:

```sh
dotnet run -c Release -f net7.0 --filter "*" --runtimes net7.0 net8.0
```

The preceding `dotnet run` command:

*   builds the benchmarks in a Release build. This is important for performance testing, as most optimizations are disabled in Debug builds, in both the C# compiler and the JIT compiler.
*   targets .NET 7 for the host project. In general with BenchmarkDotNet, you want to target the lowest-common denominator of all runtimes you’ll be executing against, so as to ensure that all of the APIs being used are available everywhere they’re needed.
*   runs all of the benchmarks in the whole program. The `--filter` argument can be refined to scope down to just a subset of benchmarks desired, but `"*"` says “run ’em all.”
*   runs the tests on both .NET 7 and .NET 8.

Throughout the post, I’ve shown many benchmarks and the results I received from running them. All of the code works well on all supported operating systems and architectures. Unless otherwise stated, the results shown for benchmarks are from running them on Linux (Ubuntu 22.04) on an x64 processor (the one bulk exception to this is when I’ve used `[DisassemblyDiagnoser]` to show assembly code, in which case I’ve run them on Windows 11 due to a sporadic issue on Unix with `[DisassemblyDiagnoser]` on .NET 7 not always producing the requested assembly). My standard caveat: these are _microbenchmarks_, often measuring operations that take very short periods of time, but where improvements to those times add up to be impactful when executed over and over and over. Different hardware, different operating systems, what else is running on your machine, your current mood, and what you ate for breakfast can all affect the numbers involved. In short, don’t expect the numbers you see to match exactly the numbers I report here, though I have chosen examples where the _magnitude_ of differences cited is expected to be fully repeatable.

With all that out of the way, let’s dive in…

## JIT

Code generation permeates every single line of code we write, and it’s critical to the end-to-end performance of applications that the compiler doing that code generation achieves high code quality. In .NET, that’s the job of the Just-In-Time (JIT) compiler, which is used both “just in time” as an application executes as well as in Ahead-Of-Time (AOT) scenarios as the workhorse to perform the codegen at build-time. Every release of .NET has seen significant improvements in the JIT, and .NET 8 is no exception. In fact, I dare say the improvements in .NET 8 in the JIT are an incredible leap beyond what was achieved in the past, in large part due to dynamic PGO…

### Tiering and Dynamic PGO

To understand dynamic PGO, we first need to understand “tiering.” For many years, a .NET method was only ever compiled once: on first invocation of the method, the JIT would kick in to generate code for that method, and then that invocation and every subsequent one would use that generated code. It was a simple time, but also one frought with conflict… in particular, a conflict between how much the JIT should invest in code quality for the method and how much benefit would be gained from that enhanced code quality. Optimization is one of the most expensive things a compiler does; a compiler can spend an untold amount of time searching for additional ways to shave off an instruction here or improve the instruction sequence there. But none of us has an infinite amount of time to wait for the compiler to finish, especially in a “just in time” scenario where the compilation is happening as the application is running. As such, in a world where a method is compiled once for that process, the JIT has to either pessimize code quality or pessimize how long it takes to run, which means a tradeoff between steady-state throughput and startup time.

As it turns out, however, the vast majority of methods invoked in an application are only ever invoked once or a small number of times. Spending a lot of time optimizing such methods would actually be a deoptimization, as likely it would take much more time to optimize them than those optimizations would gain. So, .NET Core 3.0 introduced a new feature of the JIT known as “tiered compilation.” With tiering, a method could end up being compiled multiple times. On first invocation, the method would be compiled in “tier 0,” in which the JIT prioritizes speed of compilation over code quality; in fact, the mode the JIT uses is often referred to as “min opts,” or minimal optimization, because it does as little optimization as it can muster (it still maintains a few optimizations, primarily the ones that result in less code to be compiled such that the JIT actually runs faster). In addition to minimizing optimizations, however, it also employs call counting “stubs”; when you invoke the method, the call goes through a little piece of code (the stub) that counts how many times the method was invoked, and once that count crosses a predetermined threshold (e.g. 30 calls), the method gets queued for re-compilation, this time at “tier 1,” in which the JIT throws every optimization it’s capable of at the method. Only a small subset of methods make it to tier 1, and those that do are the ones worthy of additional investment in code quality. Interestingly, there are things the JIT can learn about the method from tier 0 that can lead to even better tier 1 code quality than if the method had been compiled to tier 1 directly. For example, the JIT knows that a method “tiering up” from tier 0 to tier 1 has already been executed, and if it’s already been executed, then any `static readonly` fields it accesses are now already initialized, which means the JIT can look at the values of those fields and base the tier 1 code gen on what’s actually in the field (e.g. if it’s a `static readonly bool`, the JIT can now treat the value of that field as if it were `const bool`). If the method were instead compiled directly to tier 1, the JIT might not be able to make the same optimizations. Thus, with tiering, we can “have our cake and eat it, too.” We get both good startup and good throughput. Mostly…

One wrinkle to this scheme, however, is the presence of longer-running methods. Methods might be important because they’re invoked many times, but they might also be important because they’re invoked only a few times but end up running forever, in particular due to looping. As such, tiering was disabled by default for methods containing backward branches, such that those methods would go straight to tier 1. To address that, .NET 7 introduced On-Stack Replacement (OSR). With OSR, the code generated for loops also included a counting mechanism, and after a loop iterated to a certain threshold, the JIT would compile a new optimized version of the method and jump from the minimally-optimized code to continue execution in the optimized variant. Pretty slick, and with that, in .NET 7 tiering was also enabled for methods with loops.

But why is OSR important? If there are only a few such long-running methods, what’s the big deal if they just go straight to tier 1? Surely startup isn’t significantly negatively impacted? First, it can be: if you’re trying to trim milliseconds off startup time, every method counts. But second, as noted before, there are throughput benefits to going through tier 0, in that there are things the JIT can learn about a method from tier 0 which can then improve its tier 1 compilation. And the list of things the JIT can learn gets a whole lot bigger with dynamic PGO.

Profile-Guided Optimization (PGO) has been around for decades, for many languages and environments, including in .NET world. The typical flow is you build your application with some additional instrumentation, you then run your application on key scenarios, you gather up the results of that instrumentation, and then you rebuild your application, feeding that instrumentation data into the optimizer, allowing it to use the knowledge about how the code executed to impact how it’s optimized. This approach is often referred to as “static PGO.” “Dynamic PGO” is similar, except there’s no effort required around how the application is built, scenarios it’s run on, or any of that. With tiering, the JIT is already generating a tier 0 version of the code and then a tier 1 version of the code… why not sprinkle some instrumentation into the tier 0 code as well? Then the JIT can use the results of that instrumentation to better optimize tier 1. It’s the same basic “build, run and collect, re-build” flow as with static PGO, but now on a per-method basis, entirely within the execution of the application, and handled automatically for you by the JIT, with zero additional dev effort required and zero additional investment needed in build automation or infrastructure.

Dynamic PGO first previewed in .NET 6, off by default. It was improved in .NET 7, but remained off by default. Now, in .NET 8, I’m thrilled to say it’s not only been significantly improved, it’s now on by default. This one-character PR to enable it might be the most valuable PR in all of .NET 8: [dotnet/runtime#86225](https://github.com/dotnet/runtime/pull/86225).

There have been a multitude of PRs to make all of this work better in .NET 8, both on tiering in general and then on dynamic PGO in particular. One of the more interesting changes is [dotnet/runtime#70941](https://github.com/dotnet/runtime/pull/70941), which added more tiers, though we still refer to the unoptimized as “tier 0” and the optimized as “tier 1.” This was done primarily for two reasons. First, instrumentation isn’t free; if the goal of tier 0 is to make compilation as cheap as possible, then we want to avoid adding yet more code to be compiled. So, the PR adds a new tier to address that. Most code first gets compiled to an unoptimized and uninstrumented tier (though methods with loops currently skip this tier). Then after a certain number of invocations, it gets recompiled unoptimized but instrumented. And then after a certain number of invocations, it gets compiled as optimized using the resulting instrumentation data. Second, `crossgen`/`ReadyToRun` (R2R) images were previously unable to participate in dynamic PGO. This was a _big_ problem for taking full advantage of all that dynamic PGO offers, in particular because there’s a significant amount of code that every .NET application uses that’s already R2R’d: the core libraries. `ReadyToRun` is an AOT technology that enables most of the code generation work to be done at build-time, with just some minimal fix-ups applied when that precompiled code is prepared for execution. That code is optimized and not instrumented, or else the instrumentation would slow it down. So, this PR also adds a new tier for R2R. After an R2R method has been invoked some number of times, it’s recompiled, again with optimizations but this time also with instrumentation, and then when that’s been invoked sufficiently, it’s promoted again, this time to an optimized implementation utilizing the instrumentation data gathered in the previous tier.

![Code flow between JIT tiers](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2023/09/TierFlow.png)
_Diagram showing the flow of code through different JIT tiers. For "Not Prejitted" methods, it goes from "Tier 0 (not optimized, not instrumented)" to "Tier 0 Instrumentated (not optimized, instrumented)" and then to "Tier 1 (optimized with dynamic PGO)". For "Prejitted (R2R)" methods, it goes from "Use existing R2R (optimized, not instrumented)" to "Tier 1 Instrumentated (optimized, instrumented)" and then to "Tier 1 (optimized with dynamic PGO)"._

There have also been multiple changes focused on doing more optimization in tier 0. As noted previously, the JIT wants to be able to compile tier 0 as quickly as possible, however some optimizations in code quality actually help it to do that. For example, [dotnet/runtime#82412](https://github.com/dotnet/runtime/pull/82412) teaches it to do some amount of constant folding (evaluating constant expressions at compile time rather than at execution time), as that can enable it to generate much less code. Much of the time the JIT spends compiling in tier 0 is for interactions with the Virtual Machine (VM) layer of the .NET runtime, such as resolving types, and so if it can significantly trim away branches that won’t ever be used, it can actually speed up tier 0 compilation while also getting better code quality. We can see this with a simple repro app like the following:

```C
// dotnet run -c Release -f net8.0

MaybePrint(42.0);

static void MaybePrint<T>(T value)
{
    if (value is int)
        Console.WriteLine(value);
}
```

I can set the `DOTNET_JitDisasm` environment variable to `*MaybePrint*`; that will result in the JIT printing out to the console the code it emits for this method. On .NET 7, when I run this (`dotnet run -c Release -f net7.0`), I get the following tier 0 code:

```assembly
; Assembly listing for method Program:<<Main>$>g__MaybePrint|0_0[double](double)
; Emitting BLENDED_CODE for X64 CPU with AVX - Windows
; Tier-0 compilation
; MinOpts code
; rbp based frame
; partially interruptible

G_M000_IG01:                ;; offset=0000H
       55                   push     rbp
       4883EC30             sub      rsp, 48
       C5F877               vzeroupper
       488D6C2430           lea      rbp, [rsp+30H]
       33C0                 xor      eax, eax
       488945F8             mov      qword ptr [rbp-08H], rax