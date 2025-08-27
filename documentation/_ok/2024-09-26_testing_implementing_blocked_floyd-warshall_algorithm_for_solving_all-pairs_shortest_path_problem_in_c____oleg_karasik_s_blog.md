```yaml
---
title: "Implementing Blocked Floyd-Warshall algorithm for solving all-pairs shortest path problem in C# – Oleg Karasik's blog"
source: https://olegkarasik.wordpress.com/2024/09/26/implementing-blocked-floyd-warshall-algorithm-for-solving-all-pairs-shortest-path-problem-in-c/?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=implementing-blocked-floyd-warshall-algorithm&_bhlid=fba050748f7c98d10b7ea1cb8eda2af5cdae6574
date_published: 2024-09-26T19:52:32.000Z
date_captured: 2025-08-08T13:46:23.419Z
domain: olegkarasik.wordpress.com
author: Oleg Karasik
category: testing
technologies: [.NET SDK 8.0, .NET Runtime 8.0, Intel Xeon CPU, Intel VTune, System.Numerics.Vector, ThreadPool, CountdownEvent, L1 Cache, L2 Cache, L3 Cache, AVX2]
programming_languages: [C#]
tags: [algorithms, shortest-path, performance, csharp, dotnet, cpu-cache, vectorization, parallelism, data-structures, graph-theory]
key_concepts: [Blocked Floyd-Warshall algorithm, All-Pairs Shortest Path (APSP), CPU caching, Vectorization, Task Parallelism, Memory access patterns, Performance optimization, Graph algorithms]
code_examples: false
difficulty_level: intermediate
summary: |
  This article details the implementation of the Blocked Floyd-Warshall algorithm in C# for solving the all-pairs shortest path problem. It begins with an in-depth explanation of CPU cache mechanisms (L1, L2, L3) and their impact on high-performance computing. The author then presents the Blocked Floyd-Warshall algorithm, illustrating its block-based matrix operations with clear pseudo-code and diagrams. The post provides C# implementations, starting with a baseline, then introducing vectorization using `System.Numerics.Vector`, and finally incorporating parallelism with `ThreadPool` and `CountdownEvent`. Experimental results and Intel VTune profiler data demonstrate how cache-aware design, vectorization, and parallelism significantly improve performance, especially for the Blocked Floyd-Warshall algorithm compared to its traditional counterpart.
---
```

# Implementing Blocked Floyd-Warshall algorithm for solving all-pairs shortest path problem in C# – Oleg Karasik's blog

# Implementing Blocked Floyd-Warshall algorithm for solving all-pairs shortest path problem in C#

[Oleg Karasik](https://olegkarasik.wordpress.com/author/olegkarasik/ "Posts by Oleg Karasik") [Book of All-pairs Shortest Paths](https://olegkarasik.wordpress.com/category/developers-story/book-of-all-pairs-shortest-paths/), [Developer's Story](https://olegkarasik.wordpress.com/category/developers-story/) September 26, 2024November 14, 2024 24 Minutes

## Introduction

---

In the previous posts we have implemented a [Floyd-Warshall algorithm](https://olegkarasik.wordpress.com/2021/04/25/implementing-floyd-warshall-algorithm-for-solving-all-pairs-shortest-paths-problem-in-c/) (in four variants) and a [routes reconstruction algorithm](https://olegkarasik.wordpress.com/2022/05/11/routes-of-all-pairs-shortest-paths-with-floyd-warshall-algorithm/). In these posts we made our way through the basics of all-pairs shortest path problem, data representation in memory, parallelism, vectorisation and how we can adapt algorithms to data specifics.

In this post we will continue our journey and explore a new, more efficient, way to solve all-pairs shortest path problem. However, this time, besides employing vector and parallel capabilities of the CPU we also will employ L1, L2 and L3 caches.

Sounds interesting? Then let’s write some code 🙂

Before exploring theory and code behind the algorithm I suggest to spend a few moments and revise basic information about “CPU caches”.

> If you have a solid understanding about the topic you are free to skip it. In case you decided to read it (because you are just curious or you want to revise the basics) please take in account that the following information is an oversimplified overview.
> 
> Author’s note

## What is cache?

---

Most probably you heard something about CPU cache, at least in advertisements of a new Intel or AMD chips or during any kind of hardware discussions with friends or colleagues. Here, we aren’t going to discuss what CPU cache exactly is or who is better in implementing it Intel, AMD or Apple.

Instead, we will try to build a mental model of the cache and learn how to use it to:

*   … better understand limits we can or can’t push when writing high-performance code in C# and .NET.
*   … better understand application behaviour in high-performance scenarios on different platforms, with different parameters.
*   … brag in front of our friend when someone says “cache”.

So, let’s start from the basics… what a heck is a cache?

Imagine yourself sitting in an empty apartment in front of a PC. You are working on a new feature and you are doing great. You are blazing fast, no errors and no mistakes. You are in a perfect flow.

_You are the **CPU**._

Periodically, you need to consult documentation. All documentation you need is available in the library across the road.

_Library is the **main memory**_.

You are free to enter the library anytime and read everything you need. However, every visit to the library takes time and prevents you from advancing your work, which doesn’t sound productive. To improve the situation, you started to write down pieces of the documentation on sticky notes and put them on the display.

_Sticky note on a display is the **first level cache**._

Now when you need something from the documentation you first look at sticky notes and only if information isn’t there you have to visit the library.

_When you found what you need on sticky note – it is called a **cache hit**, otherwise – a **cache miss**._

Having sticky notes on the display reduced time spent on “library visits” but didn’t remove it completely because you can’t know what documentation you would need in advance. There is a natural limit of how much useful information you can bring back from the library.

_The amount of useful information you can bring from the library is called a **cache line**._

You can’t have millions of sticky notes on the display, so when there is not enough space you have to throw some of them into trash.

_When you throw away sticky note because of space limitation – it is called an **eviction from cache**._

To stop throwing useful information away (and stop visiting library when you need it again) you bought yourself a “small box”. Now, when you bring “new” sticky notes from library and need to free up some space on the display, instead of throwing “old” sticky notes into trash you put them into a “small box”. This way, if you need this information once again, you will be able to pick it up from the “small box” and put it back on the display and save yourself a library visit.

_“Small box” is the **second level cache**._

The “small box” can fit much more sticky notes than the display, however, it is not unlimited. Therefore, when some of sticky notes don’t fit in the “small box” you still had to throw them in the trash. That is why you decided to buy a “huge box”. Now, when you bring new sticky notes from the library you first make copies of them and put these copies into a “huge box” and only then you put originals on the display. If you need to replace sticky notes on the display, you as previously, put them into a “small box” and if “small box” is overloaded you throw them into trash. However, this time if you need this information once again – you just make a copy of the sticky note from the “huge box” and put it back on the display.

_“Huge box” is the **third level cache (aka LLC)**_

“Huge box” doesn’t eliminate “library visits”, however, if the size of a “huge box” is considerably large, then you will end up having a lot of related pieces of documentation in it, which in turn will allow you to do a significant amount of work without interruption.

That is basically it.

The above model isn’t full nor ideal but, at least I hope so, it clearly illustrates the main idea of why modern chips have multiple levels cache — to reduce number of round trips to main memory, and keep CPU busy as long as possible.

What might not be so obvious, from the model, is that all caches (L1 “sticky notes on a display”, L2 “small box” and L3 “huge box”) have different access time (aka latency) \[1\] and size.

Here is an example for [Intel i7-6700 (Skylake)](https://www.7-cpu.com/cpu/Skylake.html) CPU:

Cache

Access time (aka Latency)

Size

L1

4 cycles

64 KB

L2

12 cycles

256 KB

L3

42 cycles

8 MB

Table 1. L1, L2 and L3 caches access times and sizes of [Intel i7-6700 (Skylake)](https://www.7-cpu.com/cpu/Skylake.html) processor.

> Please note, the above numbers are just one example. Different CPUs have different characteristics. However, the overall relation more of less remains the same — L1 is the smallest but also the fastest one, L2 is somewhere in between, and L3 is the largest but also the slowest one.
> 
> Author’s note

To truly understand these numbers, it is critical to know how much time it takes to access main memory. On the same Intel i7-6700 (Skylake) CPU accessing main memory takes around 42 cycles + 51 nanosecond. You can notice, that besides cycles, we also have a fixed interval of “51 nanosecond” here. This isn’t a mistake because cycles can be converted (approximately) to nanoseconds using a very simple formula:

![Mathematical formula for calculating CPU cycle time. t = 1/f, where t is approximate time of one CPU cycle and f is CPU frequency.](https://s0.wp.com/latex.php?latex=%5Cbegin%7Baligned%7Dt%3D%5Cfrac%7B1%7D%7Bf%7D%5Cend%7Baligned%7D+%5C%5C%5C%5C%5C%5C+%5Cbegin%7Baligned%7D%5Ctext%7Bwhere%7D%5C+%26t+%26%26-%5C+%5Ctext%7Bis+an+approximate+time+of+one+CPU+cycle%7D+%5C%5C+%26f+%26%26-%5C+%5Ctext%7Bis+a+CPU+frequency%7D%5Cend%7Baligned%7D&bg=ffffff&fg=000000&s=0&c=20201002)

In our example, it would be:

![Calculation of CPU cycle time for a 4 GHz CPU. t = 1 cycle / (4 * 10^9 cycle/s) = 0.25 * 10^-9 s = 0.25 ns.](https://s0.wp.com/latex.php?latex=t%3D%5Cfrac%7B1%5C+cycle%7D%7B4%2A10%5E9%5Cfrac%7Bcycle%7D%7Bs%7D%7D%3D0.25%2A10%5E%7B-9%7Ds%3D0.25ns&bg=ffffff&fg=000000&s=0&c=20201002)

Converting access time values in previous table from cycles to access time gives us the following:

Element

Access time (aka Latency)

L1

1 ns

L2

3 ns

L3

10.5 ns

Memory

10.5 ns + 51 ns = 60.5 ns

Table 2. L1, L2, L3 caches and main memory approximate access times in nanoseconds of [Intel i7-6700 (Skylake)](https://www.7-cpu.com/cpu/Skylake.html) processor.

Even taking in account that this is a very approximate conversion, which ignores a lot of details, it still looks stunning — accessing L1 cache is 60 times faster than main memory.

Of course in real application it isn’t so simple, and cache related optimisations doesn’t result in x60 performance improvements, however, as I hope we would be able to see in this post, they still can significantly improve application performance.

> A reader familiar with the topic for sure would notice that I haven’t mentioned anything about cache modes (exclusive and inclusive), as well as anything related to instruction caches and other things like memory alignment, hardware and software prefetch, and so on. I did that intentionally for simplicity purposes.
> 
> However, if you just stumbled on this note and want to know more, you can start from this Wikipedia [page](https://en.wikipedia.org/wiki/CPU_cache) and if you want to start thinking about low-level performance I can recommend you a [book](https://book.easyperf.net/perf_book) by Denis Bakhvalov and a [paper](https://people.freebsd.org/~lstewart/articles/cpumemory.pdf) by Ulrich Drepper
> 
> Author’s note

## Blocked Floyd-Warshall algorithm

---

Blocked version of Floyd-Warshall algorithm (aka Blocked Floyd-Warshall) was introduced in \[2\]. The algorithm works with a graph `G` of vertices `V`, represented as weight matrix `W`. It splits the matrix into blocks of equal sizes (creating a new matrix of blocks `B`) and uses these blocks to calculate all-pairs of shortest paths between all vertices in a graph.

This definition doesn’t sound obvious, so here is an example.

Imagine a 8×8 weight matrix `W` (see Picture 1a). We split the matrix into 2×2 blocks to create a new 4×4 matrix of blocks `B` (see Picture 1b).

![Illustrations of the process of splitting the 8x8 weight matrix W (a) into a 4x4 matrix of 2x2 blocks B (b).](https://olegkarasik.wordpress.com/wp-content/uploads/2023/07/screenshot-2024-07-28-at-01.27.29.png)

Picture 1. Illustrations of the process of splitting the 8×8 weight matrix W (a) into a 4×4 matrix of 2×2 blocks B (b).

Every block of the matrix `B` includes a part of all paths of the matrix `W`, for instance (see Picture 2):

*   `B[0,0]` includes paths from vertices 0 and 1 to 0 and 1
*   `B[0,3]` includes paths from vertices 0 and 1 to 6 and 7
*   `B[2,0]` includes paths from vertices 4 and 5 to 0 and 1
*   … and `B[2,3]` includes paths from vertices 4 and 5 to vertices 6 and 7

![Illustration of paths represented by different blocks of 4x4 matrix of blocks. Specific blocks B[0,0], B[0,3], B[2,0], B[2,3] are highlighted with text descriptions of the vertex ranges they cover.](https://olegkarasik.wordpress.com/wp-content/uploads/2023/07/screenshot-2024-07-28-at-01.28.07.png)

Picture 2. Illustration of paths represented by different blocks of 4×4 matrix of blocks.

All vertices, represented by these four blocks are tightly related. Just looking at the matrix `B` it is noticeable that you can move from vertex 4 to 0, then from 0 to 7 and by those find a path from 4 to 7 through vertex 0, which, then can be compared with the existing path from 4 to 7, which can be updated if necessary with the shortest one (see Picture 3).

![Illustration of a path from vertex 4 to vertex 7 through vertex 0 on a matrix of blocks B. Blocks B[2,0], B[0,3], and B[2,3] are highlighted, and an arrow shows the path 4 -> 0 -> 7.](https://olegkarasik.wordpress.com/wp-content/uploads/2023/07/screenshot-2024-08-07-at-00.20.29.png)

Picture 3. Illustration of a path from vertex 4 to vertex 7 through vertex 0 on a matrix of blocks B.

If you remember the Floyd-Warshall algorithm, then these manipulations must sound extremely familiar because they resemble what it does:

```
algorithm FloydWarshall(W) do
  for k = 0 to N - 1 do
    for i = 0 to N - 1 do
        for j = 0 to N - 1 do
            // Select the shortest path between existing path from i to j and
            // a new path from i to k to j, and replace the existing path
            // with the smallest
            //
            W[i, j] = min(W[i, j], W[i, k] + W[k, j])
        end for
    end for
  end for
end algorithm
// where W     - is a weight matrix of N x N size,
//       min() - is a function which returns lesser of it's arguments
```

However, Floyd-Warshall algorithm works with matrix `W` (not `B`). Fortunately, it can be easily rewritten into a `Procedure` which accepts three blocks `B1`, `B2` and `B3` instead of matrix `W`:

```
function Procedure(B1, B2, B3) do
  for k = 0 to L - 1 do
    for i = 0 to L - 1 do
      for j = 0 to L - 1 do
        B1[i, j] = min(B1[i, j], B2[i, k] + B3[k, j])
      end for
    end for
  end for
end function
// where B1, B2 and B3 - are blocks from a block matrix B,
//       L             - is a size of the blocks (B1, B2 and B3 are L x L size)
//       min() - is a function which returns lesser of it's arguments
```

If we invoke the the `Procedure` with `B1 = B[2,3]`, `B2 = B[2,0]` and `B3 = B[0,3]`, it will recalculate all paths from vertices 4, 5 to vertices 6, 7 through vertices 0, 1.

The important moment here is while `Procedure` indeed recalculates paths from vertices 4, 5 to vertices 6, 7 (through 0 and 1), these paths aren’t guaranteed to be THE SHORTEST because recalculation relies on the existing paths stored in blocks `B[2,0]` and `B[0,3]`.

Luckily, prior to recalculating the paths in block `B[2,3]`, we can recalculate paths in blocks `B[2,0]` and `B[0,3]` using … the same `Procedure` but with different input parameters (see Picture 4):

*   To recalculate `B[2,0]` we invoke the `Procedure` with `B1 = B[2,0]`, `B2 = B[2,0]` and `B3 = B[0,0]`. This recalculates all paths from vertices 4, 5 into 0, 1 through 0 and 1.
*   To recalculate `B[0,3]` we invoke the `Procedure` with `B1 = B[0,3]`, `B2 = B[0,0]` and `B3 = B[0,3]`. This recalculates all paths from vertices 0, 1 into 6, 7 through 0 and 1.

![Illustration of calculation of a path 4 -> 0 through vertex 0 (on the left) and a path 0 -> 7 through vertex 1 (on the right) as part of recalculation of B[2,0] and B[0,3] blocks. Each sub-diagram shows the relevant blocks, input parameters, loop function, and result.](https://olegkarasik.wordpress.com/wp-content/uploads/2023/07/screenshot-2024-08-10-at-01.11.45.png)

Picture 4. Illustration of calculation of a path 4 → 0 through vertex 0 (on the left) and a path 0 → 7 through vertex 1 as part of recalculation of B\[2,0\] and B\[0,3\] blocks.

You might have already noticed — recalculation of blocks `B[2,0]` and `B[0,3]` relies on the data from block `B[0,0]` in the same way as recalculation of block `B[2,3]` relies on blocks `B[2,0]` and `B[0,3]`.

Luckily (again), prior to recalculating the paths in blocks `B[2,0]` and `B[0,3]`, we can recalculate paths in `B[0,0]` using … the same `Procedure` but (again) with different input parameters (see Picture 5):

*   To recalculate `B[0,0]` we invoke the Procedure with `B1 = B[0,0]`, `B2 = B[0,0]` and `B3 = B[0,0]`. This recalculates all paths from vertices 0, 1 into 0, 1 through 0 and 1.

![Illustration of calculation of a path from 0 -> 1 through vertex 0 as part of recalculation B[0,0] block. The diagram shows the B[0,0] block highlighted, with input parameters B1=B[0,0], B2=B[0,0], B3=B[0,0], and the loop function.](https://olegkarasik.wordpress.com/wp-content/uploads/2023/07/screenshot-2024-08-04-at-01.48.36.png)

Picture 5. Illustration of calculation of a path from 0 → 1 through vertex 0 as part of recalculation B\[0,0\] block.

It might look surprising a bit (to recalculating the block by the block itself) but if you remember, we inferred the code of the `Procedure` from the Floyd-Warshall algorithm, which means, when all input parameters are set to the same block, the `Procedure` completely resembles the Floyd-Warshall algorithm and recalculates paths within the block:

```
function Procedure(B, B, B) do
  for k = 0 to L - 1 do
    for i = 0 to L - 1 do
      for j = 0 to L - 1 do
        B[i, j] = min(B[i, j], B[i, k] + B[k, j])
      end for
    end for
  end for
end function
```

In combination, the process of recalculation of paths from vertices 4, 5 to vertices 6, 7 through vertices 0 and 1 is the following:

1.  Invoke `Procedure` with `B1 = B[0,0]`, `B2 = B[0,0]` and `B3 = B[0,0]` to calculate all paths from vertices 0, 1 to vertices 0, 1 (represented by a block `B[0,0]`) through vertices 0 and 1 (see Picture 6a).
2.  Invoke `Procedure` with B1 = B\[0,3\], B2 = B\[0,0\] and B3 = B\[0,3\] to calculate all paths from vertices 0, 1 to vertices 6, 7 (represented by a block `B[0,3]`) through vertices 0 and 1 (see Picture 6b).
3.  Invoke `Procedure` with `B1 = B[2,0]`, `B2 = B[2,0]` and `B3 = B[0,0]` to calculate all paths from vertices 4, 5 to vertices 0, 1 (represented by a block `B[2,0]`) through vertices 0 and 1 (see Picture 6c).
4.  Invoke `Procedure` with B1 = B\[2,3\], B2 = B\[2,0\] and B3 = B\[0,3\] to calculate all paths from vertices 4, 5 to vertices 6, 7 (represented by a block `B[2,3]`) through vertices 0 and 1 (see Picture 6d).

![Illustration of calculation of a path 4 -> 7 through vertex 0 by first calculating a path from 0 -> 1 through 0 (a), then 0 -> 7 through vertex 1 (b), then 4 -> 0 through 0 (c) and 4 -> 7 through 0 (d). Each sub-diagram shows the specific block calculation with inputs and results.](https://olegkarasik.wordpress.com/wp-content/uploads/2023/07/screenshot-2024-08-10-at-01.12.50.png)

Picture 6. Illustration of calculation of a path 4 → 7 through vertex 0 by first calculating a path from 0 → 1 through 0 (a), then 0 → 7 through vertex 1 (b), then 4 → 0 through 0 (c) and 4 → 7 through 0 (d).

In code, these steps can be represented by four sequential invocations of the `Procedure`:

```
Procedure(B[0,0], B[0,0], B[0,0])
Procedure(B[0,3], B[0,0], B[0,3])
Procedure(B[2,0], B[2,0], B[0,0])
Procedure(B[2,3], B[2,0], B[0,3])
```

Interestingly, by slightly adjusting the above code we can recalculate paths from vertices 4,5 to 2,3 through 0 and 1 (all we need to do, is to replace `B[0,3]` with `B[0,1]`):

```
Procedure(B[0,0], B[0,0], B[0,0])
Procedure(B[0,1], B[0,0], B[0,1])
Procedure(B[2,0], B[2,0], B[0,0])
Procedure(B[2,1], B[2,0], B[0,1])
```

… and if we replace `B[0,1]` with `B[0,2]` … I think you got the idea. We can continue and calculate all paths between all vertices through vertices 0 and 1 (see Picture 7):

![Illustration of recalculation of all paths between all vertices in the matrix through vertices 0 and 1. The diagram shows the entire block matrix, with the first row and first column of blocks highlighted, indicating that paths through vertices 0 and 1 are being updated.](https://olegkarasik.wordpress.com/wp-content/uploads/2023/07/screenshot-2024-08-12-at-22.46.46.png)

Picture 7. Illustration of recalculation of all paths between all vertices in the matrix through vertices 0 and 1.