```yaml
---
title: "Implementing Floyd-Warshall algorithm for solving all-pairs shortest paths problem in C# – Oleg Karasik's blog"
source: https://olegkarasik.wordpress.com/2021/04/25/implementing-floyd-warshall-algorithm-for-solving-all-pairs-shortest-paths-problem-in-c/
date_published: 2021-04-25T20:57:14.000Z
date_captured: 2025-08-08T13:45:38.853Z
domain: olegkarasik.wordpress.com
author: Oleg Karasik
category: testing
technologies: [.NET, Benchmark.NET, System.Threading.Tasks.Parallel, System.Numerics.Vector, System.Runtime.Intrinsics, GitHub, Windows 10, Intel Core i7-7700, Hyper-Threading, CPU registers, JIT]
programming_languages: [C#, Assembly]
tags: [algorithms, performance, optimization, graph-theory, shortest-path, floyd-warshall, csharp, dotnet, parallel-computing, vectorization]
key_concepts: [floyd-warshall-algorithm, all-pairs-shortest-paths, graph-theory, performance-optimization, data-locality, parallel-computing, vectorization, branch-prediction]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to implementing the Floyd-Warshall algorithm for solving the All-Pairs Shortest Paths (APSP) problem in C#. It begins with a foundational implementation and then systematically introduces advanced performance optimizations. The post delves into leveraging data characteristics for sparse graphs, parallelizing computations using .NET's `Parallel.For`, and applying CPU-level vectorization (SIMD) with `Vector<T>`. Through detailed explanations and Benchmark.NET results, the author demonstrates how these techniques can drastically reduce computation time, highlighting the importance of understanding both data and hardware for optimal algorithm performance.
---
```

# Implementing Floyd-Warshall algorithm for solving all-pairs shortest paths problem in C# – Oleg Karasik's blog

# Implementing Floyd-Warshall algorithm for solving all-pairs shortest paths problem in C#

[Oleg Karasik](https://olegkarasik.wordpress.com/author/olegkarasik/ "Posts by Oleg Karasik") [Book of All-pairs Shortest Paths](https://olegkarasik.wordpress.com/category/developers-story/book-of-all-pairs-shortest-paths/), [Developer's Story](https://olegkarasik.wordpress.com/category/developers-story/) April 25, 2021September 26, 2024 21 Minutes

**Notice**

This post is a free form translation (with some clarifications) of my post in Russian published on [techrocks.ru](https://techrocks.ru/2021/03/18/floyd-warshall-algorithm-implementation-in-c-language/). So if you know Russian, you are welcome to read it there. Otherwise, I invite you to join me here and have some fun.

## Introduction

---

All of us solve a “[shortest path](https://en.wikipedia.org/wiki/Shortest_path_problem)” problem many times a day. Unintentionally of course. We solve it on our way work or when we browse the Internet or when we arrange our things on a desk.

Sounds a bit… too much? Let’s find out.

Imagine, you have decided to meet with friends, well … let’s say in cafe. First of all, you need to find a route (or path) to cafe, so you start looking for available public transport (if you are on foot) or routes and streets (if you are driving). You are looking for a route from your current location to cafe but not “any” route – the shortest or fastest one.

Here is a one more example, which isn’t so obvious as the previous one. During you way work you decide to take a “short cut” through a side street because well… it is a “short cut” and it is “faster” to go this way. But how did you know this “short cut” is faster? Based on personal experience you solved a “shortest path” problem and selected a route, which goes through the side street.

In both examples, the “shortest” route is determined in either distance or time required to get from one place to another. Traveling examples are very natural applications of [graph theory](https://en.wikipedia.org/wiki/Graph_theory) and “shortest path” problem in particular. However, what about the example with arranging things on a desk? In this case the “shortest” can represent a number or complexity of actions you have to perform to get, for example, a sheet of paper: open desk, open folder, take a sheet of paper vs take a sheet of paper right from desk.

All of the above examples represent a problem of finding a shortest path between two vertexes in a graph (a route or path between two places, a number of actions or complexity of getting a sheet of paper from one place or another). This class of shortest path problems is called **SSSP (Single Source Shortest Path)** and the base algorithm for solving these problems is [Dijkstra’s algorithm](https://en.wikipedia.org/wiki/Dijkstra%27s_algorithm), which has a `O(n2)` computational complexity.

But, sometimes we need to find all shortest paths between all vertexes. Consider the following example: you are creating a map for you regular movements between **home**, **work** and **theatre**. In this scenario you will end up with 6 routes: `work ⭢ home`, `home ⭢ work`, `work ⭢ theatre`, `theatre ⭢ work`, `home ⭢ theatre` and `theatre ⭢ home` (the reverse routes can be different because of one-way roads for example).

Adding more place to the map will result in a significant grow of combinations – according to [permutations of n](https://en.wikipedia.org/wiki/Permutation#k-permutations_of_n) formula of [combinatorics](https://en.wikipedia.org/wiki/Combinatorics) it can be calculated as:

```
A(k, n) = n! / (n - m)!
// where n - is a number of elements,
//       k - is a number of elements in permutation (in our case k = 2)
```

Which gives us 12 routes for 4 places and 90 routes for 10 places (which is impressive). Note… this is without considering intermediate points between places i.e., to get from home to work you have to cross 4 streets, walk along the river and cross the bridge. If we imagine, some routes can have common intermediate points… well … as the result we will have a very large graph, with a lot of vertexes, where each vertex will represent either a place or a significant intermediate point. The class of problems, where we need to find all shortest paths between all pairs of vertexes in the graph, is called **APSP (All Pairs Shortest Paths)** and the base algorithm for solving these problems is [Floyd-Warshall algorithm](https://en.wikipedia.org/wiki/Floyd–Warshall_algorithm), which has `O(n3)` computational complexity.

And this is the algorithm we will implement today 🙂

## Floyd-Warshall algorithm

---

Floyd-Warshall algorithm finds all shortest paths between every pair of vertexes in a graph. The algorithms was published by Robert Floyd in \[1\] (see “References” section for more details). In the same year, Peter Ingerman in \[2\] described a modern implementation of the algorithm in form of three nested `for` loops:

```
algorithm FloydWarshall(W) do
  for k = 0 to N - 1 do
    for i = 0 to N - 1 do
        for j = 0 to N - 1 do
            W[i, j] = min(W[i, j], W[i, k] + W[k, j])
        end for
    end for
  end for
end algorithm
// where W     - is a weight matrix of N x N size,
//       min() - is a function which returns lesser of it's arguments
```

If you never had a change to work with a graph represented in form of matrix then it could be difficult to understand what the above algorithm does. So, to ensure we are on the same page, let’s look into how graph can be represented in form of matrix and why such representation is beneficial to solve shortest path problem.

The picture bellow illustrates a [directed, weighted](https://en.wikipedia.org/wiki/Glossary_of_graph_theory) graph of 5 vertexes. On the left, the graph is presented in visual form, which is made of circles and arrows, where each circle represents a vertex and arrow represents an edge with direction. Number inside a circle corresponds to a vertex number and number above an edge corresponds to edge’s weight. On the right, the same graph is presented in form of weight matrix. Weight matrix is a form of [adjacency matrix](https://en.wikipedia.org/wiki/Adjacency_matrix) where each matrix cell contains a “weight” – a distance between vertex `i` (row) and vertex `j` (column). Weight matrix doesn’t include information about a “path” between vertexes (a list of vertexes through which you get from `i` to `j`) – just a weight (distance) between these vertexes.

![Picture 1. Representation of a directed, weighted graph of 5 vertexes in visual form (on the left) and weighted matrix form (on the right). The graph shows nodes 0, 1, 2, 3, 4 with weighted directed edges. The matrix W shows initial weights between directly connected nodes, with empty cells indicating no direct path.](https://olegkarasik.wordpress.com/wp-content/uploads/2021/03/01.png?w=712)

In a weight matrix we can see that cell values are equal to weights between [adjacent](https://en.wikipedia.org/wiki/Glossary_of_graph_theory) vertexes. That is why, if we inspect paths from vertex `0` (row `0`), we will see that … there is only one path – from `0` to `1`. But, on a visual representation we can clearly see paths from vertex `0` to vertexes `2` and `3` (through vertex `1`). The reason for this is simple – in an initial state, weight matrix contains distance between adjacent vertexes only. However, this information alone is enough to **find** the rest.

Let’s see how it works. Pay attention to cell `W[0, 1]`. It’s value indicates, there is a path from vertex `0` to vertex `1` with a weight equal to `1` (in short we can write in as: `0 ⭢ 1: 1`). With this knowledge, we now can scan all cells of row `1` (which contains all weights of all paths from vertex `1`) and back port this information to row `0`, increasing the weight by `1` (value of `W[0, 1]`).

![Picture 2. Illustration of finding all paths from vertex 0 to vertexes adjacent to vertex 1. The graph highlights the path 0->1. The matrix shows how paths from row 1 (e.g., 1->2 with weight 1, 1->3 with weight 3) are used to update row 0 (e.g., 0->2 becomes 1+1=2, 0->3 becomes 1+3=4).](https://olegkarasik.wordpress.com/wp-content/uploads/2021/03/02.png?w=711)

Using the same steps we can find paths from vertex `0` through other vertexes. During the search, it might happen that there are more than one path leading to the same vertex and what is more important the weights of these paths could be different. An example of such a situation is illustrated on the picture bellow, where search from vertex `0` through vertex `2` revealed one more path to vertex `3` of a smaller weight.

![Picture 3. Illustration of a situation, where search from vertex 0 through vertex 2 revealed an additional, shorter path to vertex 3. The graph highlights the path 0->1->2->3. The matrix shows that the path 0->3, initially 4, is updated to 3 because a shorter path 0->2->3 (1+1+1=3) was found.](https://olegkarasik.wordpress.com/wp-content/uploads/2021/03/03.png?w=724)

We have two paths: an original path – `0 ⭢ 3: 4` and a new path we have just discovered – `0 ⭢ 2 ⭢ 3: 3` (keep in mind, weight matrix doesn’t contain paths, so we don’t know which vertexes are included in original path). This is a moment when we make a decision to keep a shortest one and write `3` into cell `W[0, 3]`.

Looks like we have just found our first shortest path!

The steps we have just saw are the essence of Floyd-Warshall algorithm. Look at the algorithm’s pseudocode one more time:

```
algorithm FloydWarshall(W) do
  for k = 0 to N - 1 do
    for i = 0 to N - 1 do
        for j = 0 to N - 1 do
            W[i, j] = min(W[i, j], W[i, k] + W[k, j])
        end for
    end for
  end for
end algorithm
```

The outermost cycle `for` on `k` iterates over all vertexes in a graph and on each iteration, variable `k` represents a vertex **we are searching paths through**. Inner cycle `for` on `i` also iterates over all vertexes in the graph and on each iteration, `i` represents a vertex **we are searching paths from**. And lastly an innermost cycle `for` on `j` iterates over all vertexes in the graph and on each iteration, `j` represents a vertex **we are searching path to.** In combination it gives us the following: on each iteration `k` we are searching paths from all vertexes `i` into all vertexes `j` through vertex `k`. Inside a cycle we compare path `i ⭢ j` (represented by `W[i, j]`) with a path `i ⭢ k ⭢ j` (represented by sum of `W[I, k]` and `W[k, j]`) and writing the shortest one back into `W[i, j]`.

Now, when we understand the mechanics it is time to implement the algorithm.

## Basic implementation

---

> The source code and experimental graphs are available in [repository](https://github.com/OlegKarasik/apsp-problems) on GitHub.
> 
> The experimental graphs can be found in `Data/Sparse-Graphs.zip` directory. All benchmarks in this post are implemented in [APSP01.cs](https://github.com/OlegKarasik/apsp-problems/blob/master/src/Benchmarks/APSP01.cs) file.
> 
> Author’s note

Before diving into implementation we need to clarify a few technical moments:

1.  All implementations work with weight matrix represented in form of lineal array.
2.  All implementations use integer arithmetic. Absence of path between vertexes is represented by a special constant weight: `NO_EDGE = (int.MaxValue / 2) - 1`.

Now, let’s figure out why is that.

**Regarding #1.** When we speak about matrixes we define cells in terms of “rows” and “columns”. Because of this it seems natural to imagine a matrix in form of “square” or “rectangle” (Picture 4a).

![Picture 4. Multiple representations of a matrix. a) imaginary “square” representation; b) array of array representation; c) lineal array representation. This image shows three ways to conceptualize a 4x4 matrix: a) as a traditional grid, b) as an array of arrays with rows as individual arrays, and c) as a single, flat linear array where elements are stored contiguously row by row.](https://olegkarasik.wordpress.com/wp-content/uploads/2021/04/screenshot-2021-04-12-at-22.55.31.png?w=646)

However, this doesn’t necessary mean we have to represent matrix in form of array of arrays (Picture 4b) to stick to our imagination. Instead of this, we can represent a matrix in form of lineal array (Picture 4c) where index of each cell is calculated using the following formula:

```
i = row * row_size + col;
// where row     - cell row index,
//       col     - cell column index,
//       row_size - number of cells in a row.
```

Lineal array of weight matrix is a **precondition** for effective execution of Floyd-Warshall algorithm. The reason for that isn’t simple and detailed explanation deserves a separate post… or a few posts. However, currently, it is important to mention that such representation significantly improves [data locality](https://en.wikipedia.org/wiki/Locality_of_reference), which in fact has a great impact on algorithm’s performance.

> Here, I am asking you to believe me and just this information in mind as a precondition, however, at the same time I recommend you to spend some time and study the question, and by the way – don’t believe people on the Internet.
> 
> Author’s note

**Regarding #2.** If you take a closer look at the algorithm pseudocode, you won’t find any checks related to existence of a path between two vertexes. Instead, pseudocode simply use `min()` function. The reason is simple – originally, if there is no path between to vertexes, a cell value is set to `infinity` and in all languages, except may be JavaScript, all values are less than `infinity`. In case of integers it might be tempting to use `int.MaxValue` as a “no path” value. However this will lead to integer overflow in cases when values of both `i ⭢ k` and `k ⭢ j` paths will be equal to `int.MaxValue`. That is why we use a value which is one less than a half of the `int.MaxValue`.

> Hey! But why we can’t just check whether if path exists before doing any calculations. For example by comparing paths both to `0` (if we take a zero as “no path” value).
> 
> Thoughtful reader

It is indeed possible but unfortunately it will lead to a significant performance penalty. In short, CPU keeps statistics of branch evaluation results ex. when some of `if` statements evaluate to `true` or `false`. It uses this statistics to execute code of “statistically predicted branch” upfront before the actual `if` statement is evaluated (this is called [speculative execution](https://en.wikipedia.org/wiki/Speculative_execution)) and therefore execute code more efficiently. However, when CPU prediction is inaccurate, it causes a significant performance loss compared to correct prediction and unconditional execution because CPU has to stop and calculate the correct branch.

Because on each iteration `k` we update a significant portion of weight matrix, CPU branch statistics becomes useless because there is no code pattern, all branches are based exclusively on data. So such check will result in significant amount of [branch mispredictions](https://en.wikipedia.org/wiki/Branch_predictor).

Here I am also asking of you to believe me (for now) and then spend some time to study the topic.

Uhh, looks like we are done with the theoretical part – let’s implement the algorithm (we denote this implementation as `Baseline`):

```csharp
public void Baseline(int[] matrix, int sz)
{
  for (var k = 0; k < sz; ++k)
  {
    for (var i = 0; i < sz; ++i)
    {
      for (var j = 0; j < sz; ++j)
      {
        var distance = matrix[i * sz + k] + matrix[k * sz + j];
        if (matrix[i * sz + j] > distance)
        {
          matrix[i * sz + j] = distance;
        }
      }
    }
  }
}
```

The above code is almost an identical copy of the previously mentioned pseudocode with a single exception – instead of `Math.Min()` we use `if` to update a cell only when necessary.

> Hey! Wait a minute, wasn’t it you who just wrote a lot of words about why if’s aren’t good here and a few lines later we ourselves introduce an if?
> 
> Thoughtful reader

The reason is simple. At the moment of writing JIT emits almost equivalent code for both `if` and `Math.Min` implementations. You can check it in details on [sharplab.io](https://sharplab.io/#v2:EYLgxg9gTgpgtADwGwBYA0AXEBDAzgWwB8ABAJgEYBYAKGIGYACMhgYQYG8aHunHiUGAMQA2EAJ4ATAOrYouABbZhwgPoAGNQAoAlgDsMAbQC6DfNgxRtCNAz0YGuAF4BKBlx6dqPHgDNoDTQA3WQYAawYAXgY1AG4whgAeB0c4gGpU0Od3b3Y3L29ffyCQ7UjouNKkpzTU7Sz8go5sxoY/KADg9oArMtiGHqqUhnSu+paPZvHOhgltXAxsXTAYMrMLKwNSgCpk4bCTVNNzSwQDcJ2nPa6jGMmW7R8AtZPNhgvHK5MAPhm5haWYGNxhMGsDuM8NttdodrmVZvNFstbqDxgBfO7edEonhYxq4nHNZr0JgCETiaSyBRKVQaFQAWT0On0xiO62stn0yVczU8jTaHRC4SifXCgxqmTuuQxrSK01KwoqiWSNTq0t5wP5xW6vTiA2Vw1So2l3HVYOm8P+y1Wx0hb2h+z2ENO53t12RYPBNtOUMuMJMUTp5nkADoGbpNMaWk7Xu9PjYLYjAe7gfi8XdUwwsaigA) but here are snippets of main loop bodies:

```assembly
53: movsxd r14, r14d
// compare matrix[i * sz + j]
//     and distance
56: cmp [rdx+r14*4+0x10], ebx
5b: jle short 64
// if matrix[i * sz + j]
//     greater than distance
//       write distance to matrix
5d: movsxd rbp, ebp
60: mov [rdx+rbp*4+0x10], ebx
64: // end of loop
```
Assembly code of implementation of innermost loop `for` of `j` using `if`.

```assembly
4f: movsxd rbp, ebp
52: mov r14d, [rdx+rbp*4+0x10]
// compare matrix[i * sz + j]
//     and distance
57: cmp r14d, ebx
// if matrix[i * sz + j]
//     less than distance
//       write value to matrix
5a: jle short 5e
// otherwise
//   write distance to matrix
5c: jmp short 61
5e: mov ebx, r14d
61: mov [rdx+rbp*4+0x10], ebx
65: // end of loop
```
Assembly code of implementation of innermost loop `for` of `j` using `Math.Min`.

You may see, regardless of whether we use `if` or `Math.Min` there is still a conditional check. However, in case of `if` there is no unnecessary write.

Now when we have finished with the implementation, it is time to run the code and see how fast is it?!

> You can verify the code correctness yourself by running tests in a [repository](https://github.com/OlegKarasik/apsp-problems).
> 
> Author’s note

I use [Benchmark.NET](https://github.com/dotnet/BenchmarkDotNet) (version 0.12.1) to benchmark code. All graphs used in benchmarks are [directed, acyclic](https://ru.wikipedia.org/wiki/%D0%93%D0%BB%D0%BE%D1%81%D1%81%D0%B0%D1%80%D0%B8%D0%B9_%D1%82%D0%B5%D0%BE%D1%80%D0%B8%D0%B8_%D0%B3%D1%80%D0%B0%D1%84%D0%BE%D0%B2) graphs of 300, 600, 1200, 2400 and 4800 vertexes. Number of edges in graphs is around 80% of possible maximum which for directed, acyclic graphs can be calculated as:

```
var max = v * (v - 1)) / 2;
// where v - is a number of vertexes in a graph.
```

Let’s rock!

Here are the results of benchmarks run on my PC (Windows 10.0.19042, Intel Core i7-7700 CPU 3.60Ghz (Kaby Lake) / 8 logical processors / 4 cores):

| Method | Size | Mean | Error | StdDev |
|---|---|---|---|---|
| Baseline | 300 | 27.525 ms | 0.1937 ms | 0.1617 ms |
| Baseline | 600 | 217.897 ms | 1.6415 ms | 1.5355 ms |
| Baseline | 1200 | 1,763.335 ms | 7.4561 ms | 6.2262 ms |
| Baseline | 2400 | 14,533.335 ms | 63.3518 ms | 52.9016 ms |
| Baseline | 4800 | 119,768.219 ms | 181.5514 ms | 160.9406 ms |

Table 1. Experimental result of “Baseline” implementation running on Windows 10.0.19042, Intel Core i7-7700 for 5 graphs.

From the results we can see, calculation time grows dramatically compared to a size of a graph – for a graph of 300 vertexes it took 27 milliseconds, for a graph of 2400 vertex – 14.5 seconds and for a graph of 4800 – 119 seconds which is **almost 2 minutes**!

Looking at algorithm’s code it might be hard to imagine, there is something we can do to speed up calculations… because well… there are THREE loops, just THREE loops.

However, as it usually happens – possibilities are hidden in details 🙂

## Know you data – sparse graphs

---

Floyd-Warshall algorithm is a base algorithm for solving all-pairs shortest path problem, especially when it comes to [dense](https://en.wikipedia.org/wiki/Glossary_of_graph_theory) or [complete](https://en.wikipedia.org/wiki/Glossary_of_graph_theory) graphs (because algorithm searches paths between all pairs of vertexes).

However, in our experiments we use **directed, acyclic** graphs, which have a wonderful property – if there is a path from vertex `1` to vertex `2`, then there is no path from vertex `2` to vertex `1`. For us, it means, there are a lot of non-adjacent vertexes which we can skip if there is no path from `i` to `k` (we denote this implementation as `SpartialOptimisation`).

```csharp
public void SpartialOptimisation(int[] matrix, int sz)
{
  for (var k = 0; k < sz; ++k)
  {
    for (var i = 0; i < sz; ++i)
    {
      if (matrix[i * sz + k] == NO_EDGE)
      {
        continue;
      }
      for (var j = 0; j < sz; ++j)
      {
        var distance = matrix[i * sz + k] + matrix[k * sz + j];
        if (matrix[i * sz + j] > distance)
        {
          matrix[i * sz + j] = distance;
        }
      }
    }
  }
}
```

Here are the results of previous (`Baseline`) and current (`SpartialOptimisation`) implementations on the same set of graphs:

| Method | Size | Mean | Error | StdDev | Ratio |
|---|---|---|---|---|---|
| Baseline | 300 | 27.525 ms | 0.1937 ms | 0.1617 ms | 1.00 |
| **SpartialOptimisation** | 300 | **12.399 ms** | 0.0943 ms | 0.0882 ms | 0.45 |
| Baseline | 600 | 217.897 ms | 1.6415 ms | 1.5355 ms | 1.00 |
| **SpartialOptimisation** | 600 | **99.122 ms** | 0.8230 ms | 0.7698 ms | 0.45 |
| Baseline | 1200 | 1,763.335 ms | 7.4561 ms | 6.2262 ms | 1.00 |
| **SpartialOptimisation** | 1200 | **766.675 ms** | 6.1147 ms | 5.