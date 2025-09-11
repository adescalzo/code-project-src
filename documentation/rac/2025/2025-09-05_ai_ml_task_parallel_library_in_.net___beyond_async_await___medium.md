```yaml
---
title: "Task Parallel Library in .NET – Beyond Async/Await | Medium"
source: https://medium.com/@karthikns999/task-parallel-library-dotnet-0bd03e1861aa
date_published: 2025-09-05T05:12:23.816Z
date_captured: 2025-09-11T11:29:00.977Z
domain: medium.com
author: Karthikeyan NS
category: ai_ml
technologies: [.NET, Task Parallel Library (TPL), PLINQ, ThreadPool, CancellationTokenSource]
programming_languages: [C#]
tags: [parallelism, concurrency, dotnet, tpl, cpu-bound, async-await, performance, multithreading, linq, tasks]
key_concepts: [Task Parallel Library, CPU-bound workloads, I/O-bound workloads, parallel loops, task continuations, Parallel LINQ, cancellation, degree of parallelism, ThreadPool]
code_examples: false
difficulty_level: intermediate
summary: |
  This article delves into the Task Parallel Library (TPL) in .NET, presenting it as a robust solution for CPU-bound workloads, distinct from async/await which is optimized for I/O-bound operations. It thoroughly explores TPL's core constructs, including the `Parallel` class for parallel loops, `Task` continuations for sequential execution, and PLINQ for parallelizing LINQ queries. The content also covers advanced TPL features like cancellation and controlling the degree of parallelism, accompanied by practical C# code examples. The author provides best practices, emphasizing the importance of choosing the appropriate concurrency model to achieve scalable and performant applications.
---
```

# Task Parallel Library in .NET – Beyond Async/Await | Medium

# Task Parallel Library (TPL) in .NET — Beyond Async/Await

## Harnessing Parallelism for Performance

![An hourglass next to a flowchart titled "Task Parallel Library (TPL) in .NET Beyond Async/Await", showing TPL branching into "Parallel.For", "Tasks", and "PLINQ".](https://miro.medium.com/v2/resize:fit:700/0*5RVI4hbHtdxtM3ZN)

When we talk about concurrency in .NET, most developers immediately think of `async/await`. And for good reason — async/await has become the go-to model for writing non-blocking I/O code. But what about **CPU-bound workloads**? What if we need raw parallelism, such as processing large data sets or running simulations?

That’s where the **Task Parallel Library (TPL)** shines. Introduced in .NET 4.0, TPL provides higher-level abstractions for parallelism that go **beyond async/await** — including parallel loops, task continuations, and PLINQ.

In this article, we’ll dive deep into TPL, explore its constructs with examples, compare it to async/await, and cover best practices for using it effectively.

## Learning Objectives

By the end of this article, we’ll:

*   Understand what the Task Parallel Library (TPL) is and why it exists.
*   Explore constructs like `Parallel`, `Task` continuations, and PLINQ.
*   Compare TPL with async/await to see where each fits.
*   Learn best practices for handling CPU-bound workloads in .NET.

## What Is the Task Parallel Library?

The **Task Parallel Library (TPL)** is a set of public types and APIs in the `System.Threading.Tasks` namespace that simplifies adding parallelism and concurrency to applications.

Key facts:

*   Built on top of the **ThreadPool** for efficient scheduling.
*   Focuses on **CPU-bound** parallel workloads, unlike async/await which is best for I/O.
*   Provides constructs like `Parallel`, `Task`, `TaskFactory`, and **PLINQ (Parallel LINQ)**.

![A flowchart titled "TPL Workflow" showing "Main Program" branching into three components: "Parallel.For", "Tasks", and "PLINQ (Parallel LINQ)".](https://miro.medium.com/v2/resize:fit:700/0*26MTGx0kb1UdHysQ)

## The Parallel Class

The `Parallel` class is one of the simplest entry points into TPL. It provides static methods like `Parallel.For` and `Parallel.ForEach`.

**Parallel.For Example**

```csharp
Parallel.For(0, 10, i =>  
{  
    Console.WriteLine($"Iteration {i} on thread {Thread.CurrentThread.ManagedThreadId}");  
});
```

This runs loop iterations concurrently using available threads.

**Parallel.ForEach Example**

```csharp
var items = Enumerable.Range(1, 5);  
Parallel.ForEach(items, item =>  
{  
    Console.WriteLine($"Processing {item} on thread {Thread.CurrentThread.ManagedThreadId}");  
});
```

*   Excellent for CPU-bound work (e.g., image processing, number crunching).
*   Not suited for async I/O (use async/await instead).

## Tasks with Continuations

Tasks are more than just wrappers around threads. They support **continuations**, which let us chain work after a task completes.

**Example: Task with Continuation**

```csharp
var task = Task.Run(() => "Hello TPL")  
    .ContinueWith(t => Console.WriteLine($"Task result: {t.Result}"));
```

Here, the second task starts only after the first completes. This allows us to build pipelines and workflows.

## PLINQ (Parallel LINQ)

Parallel LINQ is one of the most powerful (and underrated) features of TPL. It enables us to run LINQ queries in parallel.

**Example: Parallel LINQ**

```csharp
var numbers = Enumerable.Range(1, 20);  
  
var squares = numbers.AsParallel()  
    .Select(n => n * n)  
    .ToList();  
  
Console.WriteLine(string.Join(", ", squares));
```

*   Easy to convert existing LINQ to parallel.
*   We won't guarantee the order if we don't call `.AsOrdered()`.

## TPL vs Async/Await

Async/await and TPL solve different problems.

![A comparison table showing the differences between Async/Await and TPL (Parallel/PLINQ) across features like "Best for" (I/O-bound vs. CPU-bound), "Simplicity" (Very high vs. Medium), "Control" (Compiler-managed vs. Fine-grained control), and "Example Use" (API calls, DB queries vs. Image processing, math ops).](https://miro.medium.com/v2/resize:fit:700/1*1kjoXeSd9sHoWg_gYARmUQ.png)

Async/await frees threads while waiting.  
👉 TPL **utilizes threads** to crunch data faster.

## Advanced Features of TPL

Beyond parallel loops and basic tasks, the Task Parallel Library offers advanced capabilities such as cancellation, controlling parallelism, and custom scheduling.

### 1\. Cancellation with CancellationToken

We can cancel loops or tasks:

```csharp
var cts = new CancellationTokenSource();  
  
Parallel.For(0, 10, new ParallelOptions { CancellationToken = cts.Token }, i =>  
{  
    Console.WriteLine($"Iteration {i}");  
    if (i == 5) cts.Cancel();  
});
```

### 2\. Controlling Degree of Parallelism

By default, TPL uses all available cores. We can limit it:

```csharp
Parallel.For(0, 10, new ParallelOptions { MaxDegreeOfParallelism = 2 }, i =>  
{  
    Console.WriteLine($"Iteration {i}");  
});
```

### 3\. Custom TaskScheduler

Advanced developers can plug in a custom scheduler to control task execution order or priority.

## Best Practices

*   Use TPL for **CPU-bound** work.
*   Prefer `async/await` for **I/O-bound** work.
*   Control the degree of parallelism to avoid CPU oversubscription.
*   Use **PLINQ** for data processing pipelines, but watch out for ordering issues.
*   Profile before optimizing the parallelism adds overhead too.

## Final Thoughts

The Task Parallel Library (TPL) is a powerful tool in the .NET toolbox that complements async/await. While async/await dominates in modern apps, TPL still shines when we need raw parallelism for **CPU-bound workloads** like data processing, simulations, or large-scale computations.

Understanding both gives us the flexibility to choose the right tool for the job, ensuring our applications are both **scalable and performant**.