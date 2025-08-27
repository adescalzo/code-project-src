```yaml
---
title: "Understanding Pass-by-Value in C# and Struct Performance | by Julien Aspirot | Medium"
source: https://medium.com/@julienaspirot/understanding-pass-by-value-in-c-and-struct-performance-36be80cd7af1
date_published: 2025-03-29T15:35:55.150Z
date_captured: 2025-08-06T17:49:42.031Z
domain: medium.com
author: Julien Aspirot
category: performance
technologies: [.NET, C# record types, C# record struct]
programming_languages: [C#]
tags: [csharp, pass-by-value, value-types, reference-types, structs, performance, memory-management, in-modifier]
key_concepts: [pass-by-value, value types, reference types, struct performance, in modifier, heap allocation, stack allocation]
code_examples: false
difficulty_level: intermediate
summary: |
  This article clarifies C#'s pass-by-value mechanism, explaining how it applies differently to value types (like structs and primitives) and reference types (like classes). It demonstrates that value types are fully copied when passed to methods, which can lead to performance overhead for larger structs. The author advises keeping structs small (under 16 bytes) or utilizing the `in` modifier to pass them by read-only reference, thereby optimizing performance by avoiding unnecessary copies. In contrast, reference types only copy their memory address, making them efficient to pass but involving heap allocation.
---
```

# Understanding Pass-by-Value in C# and Struct Performance | by Julien Aspirot | Medium

# Understanding Pass-by-Value in C# and Struct Performance

![Julien Aspirot](https://miro.medium.com/v2/da:true/resize:fill:64:64/0*-au7M9Odu6xKZ69v)

Julien Aspirot

Follow

2 min read

·

Mar 12, 2025

Listen

Share

More

With the introduction of **record types**, I’ve seen a lot of great use cases leveraging them effectively. However, I’ve also noticed some misuse for record struct, likely due to a lack of understanding of how C# handles values and references types.

Let’s break it down from the basics.

# C# is a Pass-by-Value Language

In C#, **all method arguments are passed by value**. This means that when you pass an argument to a function, a **copy** of that value is made.

# Value Types: Changes Are Local

For **value types** (such as primitive types and structs), modifications inside a method do **not** affect the original value outside the method scope.

```csharp
struct Point  
{  
    public int X;  
    public int Y;  
  
    public Point(int x, int y) => (X, Y) = (x, y);  
}  
  
void ModifyPoint(Point p)  
{  
    p.X = 100; // This change is local to this function  
}  
  
Point original = new Point(10, 20);  
ModifyPoint(original);  
  
Console.WriteLine(original.X); // Output: 10 (unchanged)
```

Here, `ModifyPoint` receives a **copy** of `original`, so modifying `p.X` inside the function **does not** affect the original variable.

# Reference Types: Changes Affect the Original Object

For **reference types** (such as classes), what gets copied is **only the reference**, not the actual object. Thus, modifications inside a function affect the original object.

```csharp
class Person  
{  
    public string Name;  
}  
  
void ModifyPerson(Person p)  
{  
    p.Name = "Alice"; // This change affects the original object  
}  
  
Person original = new Person { Name = "Bob" };  
ModifyPerson(original);  
  
Console.WriteLine(original.Name); // Output: Alice (modified)
```

Since `Person` is a reference type, both `original` and `p` point to the same object in memory, so modifying `p.Name` affects `original.Name`.

# Performance Considerations: The Cost of Copying

Since **structs** are value types, every time they are passed to a function, a **full copy** of their memory is created. This can be costly depending on their size.

*   A **reference type** is just a pointer (~4 bytes on 32-bit, ~8 bytes on 64-bit), making it efficient to pass around.
*   A **value type** (struct) is fully copied, which can be expensive.
*   The general guideline suggests keeping structs **under 16 bytes** to minimize the cost of copying.

# Optimizing Structs: Use `in` for Efficiency

To avoid unnecessary copying of large structs, you can use the `in` modifier to **pass them by reference** in a read-only manner.

```csharp
struct LargeStruct  
{  
    public long A, B, C, D; // 32 bytes total
}  
// Pass by reference to avoid copying 
void ProcessStruct(in LargeStruct s)  
{  
    Console.WriteLine(s.A);  
}
```

This allows the function to **read** the struct without copying it, reducing performance overhead.

# Summary

*   **C# passes everything by value** by default.
*   **Value types (structs, primitives)** are fully copied, while **reference types (classes)** only copy the reference.
*   **Copying large structs is costly**, so prefer **keeping structs small (<16 bytes)** or using `in` when appropriate.
*   **Classes avoid large memory copies** but involve heap allocation.