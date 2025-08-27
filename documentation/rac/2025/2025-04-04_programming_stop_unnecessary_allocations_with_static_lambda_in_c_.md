```yaml
---
title: Stop Unnecessary Allocations with Static Lambda in C#
source: https://okyrylchuk.dev/blog/stop-unnecessary-allocations-with-static-lambda-in-csharp/
date_published: 2025-04-04T17:24:54.000Z
date_captured: 2025-08-11T16:15:07.926Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: programming
technologies: [.NET, BenchmarkDotNet]
programming_languages: [C#]
tags: [csharp, performance, memory-management, lambdas, optimization, dotnet, csharp-9, benchmarking]
key_concepts: [static-lambdas, variable-capture, closures, memory-allocation, performance-optimization, benchmarking]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores the performance benefits of using static lambdas, a feature introduced in C# 9, to reduce unnecessary memory allocations. It explains how regular lambdas can lead to "variable captures," causing the compiler to generate hidden closure classes and thus increasing memory footprint. By contrast, static lambdas prevent such captures, ensuring the lambda operates solely on its explicit parameters. The post provides clear code examples and demonstrates a significant performance improvement through benchmarks, showing zero memory allocation for static lambdas compared to megabytes for their non-static counterparts. It also highlights the trade-offs, such as the inability of static lambdas to access local variables or `this` implicitly, making them ideal for self-contained, high-performance code.
---
```

# Stop Unnecessary Allocations with Static Lambda in C#

# Stop Unnecessary Allocations with Static Lambda in C#

Lambda expressions are a powerful feature in C# and are widely used for concise function definitions, LINQ queries, and event handling. But did you know that adding the static modifier to lambdas can optimize performance? Introduced in C# 9, **static lambdas** prevent unintended variable captures, reducing memory allocations and improving efficiency.

In this post, we’ll explore **why and when to use static lambdas.**

## **Unintended Variable Captures**

Let’s look at the problem with unintended variable captures in lambdas.

Lambdas in C# can access variables from their enclosing scope, which are known as captured variables. When a lambda captures such a variable, the compiler generates a hidden class (a closure) to store it. This allows the lambda to keep using the variable even after the method that defined it has exited. While this feature is powerful, it can lead to unexpected memory allocations and affect performance, especially in tight loops or asynchronous code.

The following example demonstrates this:

```csharp
int number = 5;

Func<int, int> multiply = x => x * number; // Capturing 'number'
Action updateNumber = () => number = 10;  // Modifying captured variable

Console.WriteLine($"Number: {number}");  // Output: 5
Console.WriteLine($"Before update: {multiply(3)}"); // Output: 15
updateNumber(); // Update the captured variable
Console.WriteLine($"Number: {number}");  // Output: 10
Console.WriteLine($"After update: {multiply(3)}");  // Output: 30
```

## **Static Lambda**

To avoid unintended variable captures from the enclosing context, which can result in unexpected retention of captured objects or unexpected additional allocations, C# 9 introduced **static lambdas**.

```csharp
const int number = 5;

Func<int, int> multiply = static x => x * number; // No capture

Console.WriteLine($"Multiply: {multiply(2)}"); // Output: 10
```

While static lambdas offer performance benefits, they also come with a few trade-offs you should be aware of:

*   **No Access to Local Variables**: A static lambda **cannot capture** any variables from the enclosing method or instance. This means you must pass everything it needs explicitly as parameters.
*   **No Access to this**: Inside a static lambda, you cannot access instance members or methods of the containing class unless passed explicitly.
*   **More Verbose Code**: Since you must pass all required context explicitly, static lambdas can sometimes make code longer or less elegant — especially in LINQ queries or callbacks that naturally rely on an external state.

Despite these limitations, static lambdas are powerful when you need **predictable, allocation-free, high-performance code**.

## **Benchmarks**

Let’s run benchmarks creating and running a lot of regular and static lambdas.

The **CapturingLambdas** method creates and executes many lambdas with the captured **factor** variable.

The **StaticLambdas** method creates and executes many static lambdas without capturing any variable.

```csharp
BenchmarkRunner.Run<LambdaBenchmark>();

[MemoryDiagnoser]
public class LambdaBenchmark
{
    private int factor = 5;
    private List<Func<int, int>> capturingLambdas = new();
    private List<Func<int, int>> staticLambdas = new();

    private const int Count = 100_000;

    [Benchmark]
    public void CapturingLambdas()
    {
        capturingLambdas.Clear();

        // Captures 'factor' from outer scope
        for (int i = 0; i < Count; i++)
            capturingLambdas.Add(x => x * factor);

        // Execute the lambdas
        for (int i = 0; i < Count; i++)
            capturingLambdas[i](i);
    }

    [Benchmark]
    public void StaticLambdas()
    {
        staticLambdas.Clear();

        // No capture — everything passed as parameter
        for (int i = 0; i < Count; i++)
            staticLambdas.Add(static x => x * 5);

        // Execute the lambdas
        for (int i = 0; i < Count; i++)
            staticLambdas[i](i);
    }
}
```

Let’s see the benchmark result:

![Benchmark results comparing memory allocation of capturing vs. static lambdas, showing 6.4 MB allocated for capturing lambdas and 0 B for static lambdas.](https://okyrylchuk.dev/wp-content/uploads/2025/04/benchmarks-png.avif)

The result is impressive.

The **CapturingLambdas** method allocated **6,4 MB** when the **StaticLambdas** method allocated **0 B**.

## **When to Use Static Lambda?**

Use static lambdas when your lambda doesn’t need any external context to do its job. If it works only with its input parameters, making it static is a safe and efficient choice.

They’re also a great choice to prevent accidental captures — **static** enforces this at compile time, making your code more predictable.

In performance-sensitive code, like tight loops, async workflows, or high-frequency delegates, static lambdas help reduce memory allocations and improve execution speed.

Using static lambdas generally shows clear intent: your logic is self-contained and free of external dependencies.

## **Conclusion**

Static lambdas are a simple yet effective way to **improve performance and memory efficiency** in C#. While they may not be necessary for every scenario, using them in performance-critical sections of your code can lead to cleaner and more optimized applications.

As with many performance optimizations, **measure before and after to ensure the change benefits your use case!**