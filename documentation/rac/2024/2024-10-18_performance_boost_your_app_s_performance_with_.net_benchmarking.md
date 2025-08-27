```yaml
---
title: Boost Your App’s Performance with .NET Benchmarking
source: https://okyrylchuk.dev/blog/boost-your-apps-performance-with-dotnet-benchmarking/
date_published: 2024-10-18T16:00:24.000Z
date_captured: 2025-08-12T11:27:17.349Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: performance
technologies: [.NET, BenchmarkDotNet, NuGet, .NET 7, .NET 8, dotnet CLI]
programming_languages: [C#]
tags: [benchmarking, performance, dotnet, csharp, memory-management, code-optimization, development-tools]
key_concepts: [performance-measurement, code-benchmarking, memory-profiling, parameterization, cross-runtime-benchmarking, baseline-comparison]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces BenchmarkDotNet, an essential open-source library for .NET developers to measure and improve application performance. It covers the basics of setting up benchmarks, comparing method execution times, and analyzing memory usage. The post demonstrates features like defining a baseline for comparison, hiding output columns, parameterizing benchmarks for varying inputs, and running tests across multiple .NET versions. By providing practical code examples and clear explanations, the article highlights how BenchmarkDotNet simplifies the process of gathering crucial performance data to guide optimization efforts.]
---
```

# Boost Your App’s Performance with .NET Benchmarking

# Boost Your App’s Performance with .NET Benchmarking

![](https://okyrylchuk.dev/wp-content/litespeed/avatar/a85a89199a7d4486a29f67dc2fc371d0.jpg?ver=1754361075)By [Oleg Kyrylchuk](https://okyrylchuk.dev/blog/author/kyrylchuk-oleggmail-com/ "View all posts by Oleg Kyrylchuk") / October 18, 2024

In this post, we’re exploring a tool every .NET developer should have in their toolkit: **BenchmarkDotNet**. Whether you’re optimizing existing code or writing performance-critical applications, measuring execution time and improving efficiency is crucial. Let’s explore BenchmarkDotNet and how to use it to level up your .NET performance game.

Table Of Contents

1.  [What is BenchmarkDotNet?](#what-is-benchmarkdotnet)
2.  [Getting Started](#getting-started)
3.  [Baseline](#baseline)
4.  [Hide Columns](#hide-columns)
5.  [Memory Diagnoser](#memory-diagnoser) 
6.  [Parameterization](#parameterization) 
7.  [Multiple .NET Versions](#multiple-net-versions) 
8.  [Summary](#summary)

## **What is BenchmarkDotNet?**

[**BenchmarkDotNet**](https://github.com/dotnet/BenchmarkDotNet) is an open-source benchmarking library for .NET applications. It helps you measure your methods’ performance by running them multiple times, collecting statistical data, and providing detailed reports on execution time, memory usage, and more. The library is powerful yet simple to integrate into existing projects.

## **Getting Started**

To get started, install the BenchmarkDotNet NuGet package:

```
Install-Package BenchmarkDotNet
```

Or:

```
dotnet add package BenchmarkDotNet
```

Once installed, you only need a basic setup to benchmark your code. Here’s a simple example to illustrate:

```
public class StringBenchmarks
{
    private const int Count = 1000;

    [Benchmark]
    public string StringConcatenation()
    {
        string result = string.Empty;
        for (int i = 0; i < Count; i++)
            result += "Hello World!";
        return result;
    }

    [Benchmark]
    public string StringBuilder()
    {
        StringBuilder sb = new StringBuilder();
        for (int i = 0; i < Count; i++)
            sb.Append("Hello World!");
        return sb.ToString();
    }
}
```

Let’s break down the example above. 

To create a benchmark, you must create a public class with public methods and decorate the benchmarking methods with the Benchmark attribute.

In the example, two methods do the same: create an empty string and add “Hello World!” 1,000 times in the loop. The first method uses classic string concatenation, and the second method uses the StringBuilder type.

The BenchmarkRunner runs your benchmarks. You must run your app in the **Release** mode. Ideally, close your IDE (and other applications) and use CLI:

```
dotnet run -c Release
```

In the console, you’ll get the following result:

![](https://okyrylchuk.dev/wp-content/uploads/2024/10/53887302-b7d9-11ee-b61e-9583dbb2845e_media-manager_1729199417103-result1.png.webp)
*A console output table showing benchmark results for `StringConcatenation` and `StringBuilder` methods, with columns for Method, Mean, Error, and StdDev.*

By default, you see four columns: Method, Mean, Error, and StdDev.

The **Method** column is prominent; it’s the name of the benchmark. 

The **Mean** column shows the **average execution time** for the benchmarked method across all iterations.

The **Error** column represents the **margin of error** in the mean result, indicating how accurate the mean is.

The **Standard Deviation (StdDev)** column shows the **degree of variation** in the execution times across different runs.

## **Baseline**

You can mark a method as a baseline:

```
[Benchmark(Baseline = true)]
public string StringBuilder()
```

It will add a Ratio column to the summary result:

![](https://okyrylchuk.dev/wp-content/uploads/2024/10/ratio.png.webp)
*A console output table showing benchmark results including a 'Ratio' column, comparing `StringConcatenation` to `StringBuilder` (baseline), indicating `StringBuilder` is significantly faster.*

The **Ratio** column shows the performance of the benchmarked method relative to the **baseline method**.

In our example, the StringBuilder is faster than string concatenation by almost 150 times. 

The **RatioSD** column shows the **standard deviation** of the ratio, indicating how much the ratio varies across different runs.

## **Hide Columns**

You can hide columns from summary results with a **\[HideColumns\]** attribute: 

```
[HideColumns("Error", "StdDev", "RatioSD")]
public class StringBenchmarks
```

The result:

![](https://okyrylchuk.dev/wp-content/uploads/2024/10/hide.png.webp)
*A console output table showing simplified benchmark results with 'Error' and 'StdDev' columns hidden, displaying only Method, Mean, and Ratio.*

## **Memory Diagnoser** 

BenchmarkDotNet can also analyze memory usage. You have to add the **\[MemoryDiagnoser\]** attribute to your class. 

```
[MemoryDiagnoser]
public class StringBenchmarks
```

The result:

![](https://okyrylchuk.dev/wp-content/uploads/2024/10/memory.png.webp)
*A console output table showing benchmark results with memory diagnostics, including columns for Method, Mean, Median, Ratio, Allocated memory, and Allocation Ratio.*

## **Parameterization** 

The **\[Params\]** attribute in BenchmarkDotNet specifies multiple input values for a benchmark method. It allows you to **run the same benchmark with different values** for a given parameter, making it easy to test how performance changes with varying inputs. This is especially useful when you want to see how your method performs with different data sizes, configurations, or workloads.

```
[Params(100, 1_000, 10_000)]
public int Count { get; set; }
```

The result:

![](https://okyrylchuk.dev/wp-content/uploads/2024/10/count.png.webp)
*A console output table demonstrating parameterized benchmark results, showing performance metrics for `StringConcatenation` and `StringBuilder` across different 'Count' values (100, 1000, 10000).*

## **Multiple .NET Versions** 

You can run your benchmarks for multiple .NET versions with a **\[SimpleJob\]** attribute and a **RuntimeMoniker** option. 

```
[SimpleJob(RuntimeMoniker.Net70)]
[SimpleJob(RuntimeMoniker.Net80)]
public class StringBenchmarks
```

You must install all .NET versions you want to run benchmarks and define them in the project settings. 

```
<PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFrameworks>net7.0;net8.0</TargetFrameworks>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
</PropertyGroup>
```

The result:

![](https://okyrylchuk.dev/wp-content/uploads/2024/10/moniker.png.webp)
*A console output table showing benchmark results for `StringConcatenation` and `StringBuilder` run across multiple .NET versions (.NET 7.0 and .NET 8.0).*

## Summary

This is an intro to the BenchmarkDotNet tool. I showed the most used features by me.

However, the BenchmarkDotNet has many more features. I encourage you to explore the tool. 

Benchmarking is an essential skill for any .NET developer. BenchmarkDotNet makes setting up and gathering performance data to guide your optimization efforts easy.

You can find the source on my [GitHub](https://github.com/okyrylchuk/dotnet-newsletter/tree/main/BenchmarkDotNetSample). 

@import url('https://fonts.googleapis.com/css2?family=Lato&display=swap'); \[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] { --background-color:#E9ECEF; --border-color:#E9ECEF; --text-colour:#343A40; --font-family:Lato, "Trebuchet MS", sans-serif; } \[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .emailoctopus-form { --label-size:12px; --field-colour:#FFFFFF; --label-colour:#636363; --button-colour:#6000CD; --button-text-colour:#FFFFFF; --field-border-colour:#e9ecef; } \[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\].modal-container{align-items:center;background:rgba(77,77,77,.8);display:flex;flex-direction:column;inset:0;justify-content:center;opacity:0;overflow:hidden;position:fixed;transition:opacity .3s ease-in;width:100vw;z-index:999999}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\].modal-container.active{opacity:1}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\].modal-container .modal-container-inner{background:#fefefe;border-radius:5px;max-width:600px;padding:20px;position:relative}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\].modal-container .modal-container-inner button.close{align-items:center;background-color:rgba(0,0,0,.8);border-color:transparent;border-radius:50%;border-width:0;color:#fff;display:flex;height:34px;margin:0;opacity:1;padding:6px;position:absolute;right:-17px;top:-17px;transition:background-color .5s;width:34px;z-index:2}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\].modal-container .modal-container-inner button.close:hover{background-color:#000;cursor:pointer}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\].modal-container .modal-container-inner button.close:focus{border-color:rgba(72,99,156,.3);box-shadow:0 0 0 .2rem rgba(0,123,255,.33);outline:none}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\].modal-container .modal-container-inner button.close svg{height:22px;text-align:center;width:22px}@media (max-width:700px){\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\].modal-container .modal-container-inner{margin:22px}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\].modal-container .modal-container-inner>div{max-height:90vh;overflow:scroll}}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\].inline-container{max-width:600px!important;position:relative}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\]{display:block;font-family:var(--font-family);--eo-form-font-family:var(--font-family);--eo-form-font-size:var(--base-font-size,16px);--eo-form-font-color:#071c35;--eo-form-font-weight:400;--eo-form-dropdown-font-size:var(--base-font-size,16px);--eo-form-label-color:var(--label-colour,#071c35);--eo-form-label-weight:400;--eo-form-placeholder:#495057;--eo-form-input-background:var(--field-colour,#fff);--eo-form-padding-top:0.375rem;--eo-form-padding-left:0.75rem;--eo-form-border-color:var(--field-border-colour,#d4d4d4);--eo-form-icon-color:#8d8d8d;--eo-form-icon-hover:#737373;--eo-form-icon-active:#5c5c5c;--configured-primary-color:var(--primary-brand-colour,#3c3c3c);--eo-form-primary:var(--configured-primary-color);--eo-form-engaged:color-mix(in srgb,var(--configured-primary-color),#000 18%);--eo-form-active:color-mix(in srgb,var(--configured-primary-color),#000 62%);--eo-form-highlight:color-mix(in srgb,var(--configured-primary-color),#000 41%);--eo-form-hover:color-mix(in srgb,var(--configured-primary-color),#fff 85%);--eo-form-pressed:color-mix(in srgb,var(--configured-primary-color),#fff 72%);--eo-form-accent:color-mix(in srgb,var(--configured-primary-color),#fff 90%)}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-control{background-clip:padding-box;background-color:#fff;border:1px solid #ced4da;border-radius:.25rem;color:#495057;display:block;font-size:1rem;font-weight:400;height:calc(1.5em + .75rem + 2px);line-height:1.5;padding:.375rem .75rem;transition:border-color .15s ease-in-out,box-shadow .15s ease-in-out;width:100%}@media (prefers-reduced-motion:reduce){\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-control{transition:none}}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-control::-ms-expand{background-color:transparent;border:0}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-control:focus{background-color:#fff;border-color:#80bdff;box-shadow:0 0 0 .2rem rgba(0,123,255,.25);color:#495057;outline:0}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-control::placeholder{color:#6c757d;opacity:1}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-control:disabled,\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-control\[readonly\]{background-color:#e9ecef;opacity:1}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] select.form-control:-moz-focusring{color:transparent;text-shadow:0 0 0 #495057}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] select.form-control:focus::-ms-value{background-color:#fff;color:#495057}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-control-file,\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-control-range{display:block;width:100%}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .col-form-label{font-size:inherit;line-height:1.5;margin-bottom:0;padding-bottom:calc(.375rem + 1px);padding-top:calc(.375rem + 1px)}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .col-form-label-lg{font-size:1.25rem;line-height:1.5;padding-bottom:calc(.5rem + 1px);padding-top:calc(.5rem + 1px)}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .col-form-label-sm{font-size:.875rem;line-height:1.5;padding-bottom:calc(.25rem + 1px);padding-top:calc(.25rem + 1px)}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-control-plaintext{background-color:transparent;border:solid transparent;border-width:1px 0;color:#212529;display:block;font-size:1rem;line-height:1.5;margin-bottom:0;padding:.375rem 0;width:100%}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-control-plaintext.form-control-lg,\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-control-plaintext.form-control-sm{padding-left:0;padding-right:0}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-control-sm{border-radius:.2rem;font-size:.875rem;height:calc(1.5em + .5rem + 2px);line-height:1.5;padding:.25rem .5rem}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-control-lg{border-radius:.3rem;font-size:1.25rem;height:calc(1.5em + 1rem + 2px);line-height:1.5;padding:.5rem 1rem}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] select.form-control\[multiple\],\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] select.form-control\[size\],\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] textarea.form-control{height:auto}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-group{margin-bottom:1rem}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-text{display:block;margin-top:.25rem}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-row{display:flex;flex-wrap:wrap;margin-left:-5px;margin-right:-5px}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-row>.col,\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-row>\[class\*=col-\]{padding-left:5px;padding-right:5px}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-check{display:block;padding-left:1.25rem;position:relative}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-check-input{margin-left:-1.25rem;margin-top:.3rem;position:absolute}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-check-input:disabled~.form-check-label,\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-check-input\[disabled\]~.form-check-label{color:#6c757d}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-check-label{margin-bottom:0}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-check-inline{align-items:center;display:inline-flex;margin-right:.75rem;padding-left:0}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .form-check-inline .form-check-input{margin-left:0;margin-right:.3125rem;margin-top:0;position:static}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .valid-feedback{color:#28a745;display:none;font-size:.875em;margin-top:.25rem;width:100%}\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .valid-tooltip{background-color:rgba(40,167,69,.9);border-radius:.25rem;color:#fff;display:none;font-size:.875rem;left:0;line-height:1.5;margin-top:.1rem;max-width:100%;padding:.25rem .5rem;position:absolute;top:100%;z-index:5}.form-row>.col>\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .valid-tooltip,.form-row>\[class\*=col-\]>\[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"\] .valid-tooltip{left:5px}.was-validated \[data-form="5d3d3006-5e67-11ef-a799-ab5b15e662c1"