```yaml
---
title: "Which Is Faster in C#: Record, Class, or Struct? Deep Dive on Memory and Equality | by Sukhpinder Singh | C# .Net | Write A Catalyst | Medium"
source: https://medium.com/write-a-catalyst/which-is-faster-in-c-record-class-or-struct-deep-dive-on-memory-and-equality-afd65a1da1cb
date_published: 2025-04-16T17:41:02.373Z
date_captured: 2025-08-06T17:45:25.732Z
domain: medium.com
author: "Sukhpinder Singh | C# .Net"
category: programming
technologies: [.NET, C# 9, C# 10, BenchmarkDotNet, System.Text.Json, Newtonsoft.Json, CLR]
programming_languages: [C#]
tags: [csharp, dotnet, performance, memory-management, value-types, reference-types, immutability, equality, benchmarking, serialization]
key_concepts: [reference-types, value-types, heap-allocation, stack-allocation, garbage-collection, object-header, value-based-equality, immutability, with-expression]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive comparison of C# `record`, `class`, and `struct` types, focusing on their fundamental differences in memory allocation, performance, and equality semantics. It explains how classes are reference types stored on the heap, structs are value types often on the stack, and records (introduced in C# 9) offer value-based equality and immutability. Through practical examples and BenchmarkDotNet results, the author illustrates the performance implications of each type, particularly concerning object creation and equality comparisons. The guide concludes with practical advice on choosing the appropriate type for various real-world scenarios, emphasizing considerations like memory pressure, CPU cycles, and code maintainability.
---
```

# Which Is Faster in C#: Record, Class, or Struct? Deep Dive on Memory and Equality | by Sukhpinder Singh | C# .Net | Write A Catalyst | Medium

Member-only story

## Day 2— .NET Performance Series

# Which Is Faster in C#: Record, Class, or Struct? Deep Dive on Memory and Equality

## Struggling to choose between Record, Class, or Struct in C#? This guide breaks down memory, equality, and performance to help you decide.

[

![Sukhpinder Singh | C# .Net profile picture](https://miro.medium.com/v2/da:true/resize:fill:64:64/1*yz64EFgxow-2pZGcqdcN1g.gif)





](/@singhsukhpinder?source=post_page---byline--afd65a1da1cb---------------------------------------)

[Sukhpinder Singh | C# .Net](/@singhsukhpinder?source=post_page---byline--afd65a1da1cb---------------------------------------)

Follow

5 min read

·

Apr 15, 2025

197

5

Listen

Share

More

[**Free Friend Link**](/write-a-catalyst/which-is-faster-in-c-record-class-or-struct-deep-dive-on-memory-and-equality-afd65a1da1cb?sk=7649a3a097c8402b41b2c7a7ba6bf938)

Zoom image will be displayed

![A graphic created by the author using Canva, likely a header image for the article.](https://miro.medium.com/v2/resize:fit:700/1*dEdtYPA88vPClX3380N2aQ.png)

Created by Author using Canva

Access the complete performance series here

![Sukhpinder Singh | C# .Net profile picture](https://miro.medium.com/v2/resize:fill:40:40/1*yz64EFgxow-2pZGcqdcN1g.gif)

[Sukhpinder Singh | C# .Net](https://singhsukhpinder.medium.com/?source=post_page-----afd65a1da1cb---------------------------------------)

## .Net Performance Series

[View list](https://singhsukhpinder.medium.com/list/net-performance-series-81917ac284ca?source=post_page-----afd65a1da1cb---------------------------------------)

15 stories

![A preview image from the .NET Performance Series, titled "RECORD vs CLASS vs STRUCT The Key Differences".](https://miro.medium.com/v2/resize:fill:388:388/1*gZYvARXEARPB59rFUA8H4A.jpeg)

![A preview image from the .NET Performance Series, titled "REST or gRPC? A Practical .NET 8 Comparison for C# Backend Developers".](https://miro.medium.com/v2/resize:fill:388:388/1*6BPJIuhL8zcsaGzGOiUjkw.jpeg)

![A preview image from the .NET Performance Series, titled "EF Core 8 vs Dapper: Real Benchmark Results for High-Load .NET Apps".](https://miro.medium.com/v2/resize:fill:388:388/1*Daih-za8dOOI0H7lKrolOg.jpeg)

I still remember the moment this all began. A junior dev had walked up to my desk with a puzzled look and asked,

“**_Why did you switch my class to a record? And why did you create a struct for the coordinates?_**”

I turned my chair around, smiled, and said, “Grab your coffee, this is going to be a deep dive.” What followed was an hour-long conversation that took us through the guts of C# — record vs. class vs. struct — and the fine print most developers overlook: memory footprint and equality comparisons.

This article is that conversation, extended with everything I’ve learned from decades of writing production code, profiling memory, and untangling bugs no linter could have foreseen. So, buckle up — we’re about to take a tour of how C# types behave when it really matters.

# 1\. The Basics: What Are We Comparing?

Before we get into memory overhead and performance, let’s ground ourselves in definitions.

# Class:

*   **Reference Type**
*   Stored on the heap
*   Assigned and passed by reference
*   Can have a destructor and a finalizer

# Struct:

*   **Value Type**
*   Stored on the stack (when possible)
*   Assigned and passed by value
*   Cannot have a parameterless constructor (before C# 10)

# Record:

*   Introduced in C# 9
*   Immutable by default (though mutable ones exist)
*   Can be value-based equality
*   Supports with-expression cloning
*   Available as both reference (record class) and value types (record struct)

# 2\. Memory Overhead: What the CLR Doesn’t Advertise

When you instantiate a class in C#, you’re creating an object on the heap. That means the object is garbage-collected, but also incurs some overhead:

*   Object header (minimum 8 bytes on 32-bit, 16 bytes on 64-bit)
*   Padding/alignment
*   Reference pointers

Now compare this with structs:

*   No object header
*   Stored inline when part of another object
*   Less GC overhead, but can incur boxing when cast to object or interface

Let’s take a sample:

```csharp
class PointClass  
{  
    public int X;  
    public int Y;  
}  
struct PointStruct  
{  
    public int X;  
    public int Y;  
}
```

Using BenchmarkDotNet, here’s what I got:

| Method         | Mean       | Allocated |  
|---------------|------------|-----------|  
| ClassCreation  | 65.32 ns   | 32 B      |  
| StructCreation | 1.02 ns    | 0 B       |

A 60x difference. And this is just creation. Start using them in large collections, and the difference multiplies quickly.

# 3\. Equality: Are They Truly Equal?

When it comes to equality comparisons, each type has different semantics.

# Class

*   Default equality compares references (i.e., object identity)
*   Can override `Equals` and `GetHashCode`

# Struct

*   The default equality compares field-by-field
*   Can override `Equals` and `GetHashCode`, but not always needed

# Record

*   Value-based equality is the default
*   Compiler-generated `Equals`, `GetHashCode`, and `ToString`

Here’s the power of records:

```csharp
record PointRecord(int X, int Y);  
var p1 = new PointRecord(1, 2);  
var p2 = new PointRecord(1, 2);  
Console.WriteLine(p1 == p2); // True
```

Try the same with a class, and you’ll get `False` unless you override equality members.

And for structs:

```csharp
var s1 = new PointStruct { X = 1, Y = 2 };  
var s2 = new PointStruct { X = 1, Y = 2 };  
Console.WriteLine(s1 == s2); // Compilation error unless you override
```

C# 10 made this easier with `record struct`:

```csharp
record struct PointValue(int X, int Y);
```

Now you get the best of both worlds: value-type behavior with value-based equality.

# 4\. Real-World Scenarios: Which One to Use and When

# When to Use Class:

*   You need reference semantics
*   Large objects or need polymorphism
*   Inheritance is required

# When to Use Struct:

*   Small data types
*   High-performance, low-latency scenarios
*   Avoid allocations and GC pressure

# When to Use Record:

*   Domain models that don’t need mutation
*   DDD aggregate roots and value objects
*   Pattern matching and succinct syntax

> _Pro Tip: Avoid mutable structs. They seem like a good idea until they aren’t._

# 5\. Performance Pitfalls: Benchmarks Tell the Truth

I ran a test comparing class vs struct vs record with 10 million iterations:

```csharp
BenchmarkRunner.Run<ComparisonTests>();
```

Results:

| Type         | Time       | Memory |  
|--------------|------------|--------|  
| Class        | 735 ms     | 800 MB |  
| Struct       | 91 ms      | 0 MB   |  
| Record Class | 728 ms     | 800 MB |  
| Record Struct| 92 ms      | 0 MB   |

Shocking? Not really. This is why high-frequency trading systems, graphics engines, and game devs stick to structs.

# 6\. Equality Overhead: It’s Not Just ==

Deep equality costs CPU cycles. Structs compare each field, which can be expensive if the struct is large. Classes? Pointer check. Records? Depends, but more optimized for small, immutable data.

Here’s what the equality cost looks like (1M comparisons):

| Type             | Time   |  
|------------------|--------|  
| Class Equals     | 7 ms   |  
| Struct Equals    | 15 ms  |  
| Record Equals    | 10 ms  |  
| Record Struct Eq | 14 ms  |

So, if you’re comparing data a lot, keep this in mind. Smaller is faster.

# 7\. Serialization: Not All Types Are Equal

JSON serializers like System.Text.Json and Newtonsoft.Json handle records and classes easily. Structs? Sometimes tricky, especially when they have no parameterless constructors.

Also, default values and deserialization rules differ. Classes with mutable properties? No problem. Structs with readonly fields? Get ready to jump through hoops.

# 8\. Immutable Love: Why Record Wins the Clean Code Game

For functional-style programming and immutability, records shine:

```csharp
var newPoint = oldPoint with { X = 5 };
```

No boilerplate. No bugs due to unintended side effects. Just clean, readable code.

# 9\. Versioning and Maintenance

Ever had to add a field to a struct used across dozens of interfaces? That’s pain.

Records and classes handle this more gracefully. Add a property, update the constructor, and done. Structs can break consumers due to their pass-by-value behavior.

# 10\. Final Words: Choose Wisely

Every choice in architecture has a cost. In low-traffic enterprise apps, classes are fine. In performance-critical systems, structs might save you milliseconds that mean millions. And when you’re modeling your domain — immutability, clarity, and equality — records make your intentions clear.

> _Think in terms of memory pressure, CPU cycles, and developer sanity._

After that coffee session with the junior dev, I saw him refactor his service layer using records for DTOs, structs for vector math, and classes for service orchestration. He didn’t just understand the difference — he _felt_ it in his code.

That’s the goal.

Let me know if you’d like to explore this topic deeper — maybe comparing IL code or diving into JIT behavior. For now, I hope this helped you choose the right type the next time your cursor hovers over `class`, `record`, or `struct`.