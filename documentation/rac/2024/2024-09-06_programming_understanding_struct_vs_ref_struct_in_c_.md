```yaml
---
title: Understanding struct vs ref struct in C#
source: https://okyrylchuk.dev/blog/understanding-struct-vs-ref-struct-in-csharp/
date_published: 2024-09-06T15:42:56.000Z
date_captured: 2025-08-20T21:16:15.723Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: programming
technologies: [.NET, C#, "Span<T>", "ReadOnlySpan<T>"]
programming_languages: [C#]
tags: [csharp, value-types, memory-management, performance, dotnet, structs, ref-structs, csharp-13, low-level-programming, programming-concepts]
key_concepts: [struct, ref-struct, value-types, stack-allocation, heap-allocation, boxing, immutability, span-t]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a detailed comparison between `struct` and `ref struct` in C#, highlighting their distinct characteristics and appropriate use cases. It explains that `struct` is a versatile value type primarily allocated on the stack, supporting copy semantics and boxing. In contrast, `ref struct`, introduced in C# 7.2, is a specialized stack-only value type designed for high-performance, low-level memory manipulation, explicitly preventing boxing and restricting usage in async or iterator methods. The post also covers recent relaxations in C# 13, which allow local `ref struct` variables in async/iterator methods and enable `ref struct` types to implement interfaces. Understanding these differences is crucial for optimizing memory management and performance in C# applications.
---
```

# Understanding struct vs ref struct in C#

# Understanding struct vs ref struct in C#

![Author's avatar, a stylized profile picture.](https://okyrylchuk.dev/wp-content/litespeed/avatar/a85a89199a7d4486a29f67dc2fc371d0.jpg?ver=1755571532)

In C#, structs are powerful tools for creating lightweight value types that offer both efficiency and simplicity. However, when dealing with scenarios requiring precise memory management and high performance, mainly where stack allocation is critical, C# introduces a specialized type known as ref struct. While both struct and ref struct serve the purpose of creating value types, they are designed for different use cases and come with distinct characteristics and restrictions.

This post dives into the similarities and differences between struct and ref struct in C#, helping you understand when and why to use each.

Table Of Contents

## **What is a struct?**

A struct in C# is a value type used to define small, simple types that are typically immutable and can be used in scenarios where performance is crucial. Structs are allocated on the stack, making them faster to allocate and deallocate than heap-allocated objects (reference types).

#### **Key Characteristics of struct:**

*   **Value Type**: Structs are stored on the stack when declared in a local scope, providing fast allocation and deallocation.
*   **Copy Semantics**: When a struct is assigned to another struct, a shallow copy of the data is made.
*   **Boxing and Unboxing**: Structs can be boxed (converted to an object or interface type), which moves them to the heap, incurring performance overhead.
*   **Immutability**: While structs can be mutable, it’s often recommended to design them as immutable to avoid unexpected behavior.
*   **Usage**: Commonly used for small, lightweight objects like coordinates, colors, or complex numbers.

Example:

```csharp
public struct Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y)
    {
        X = x;
        Y = y;
    }
}
```

## **What is a ref struct?**

Introduced in C# 7.2, ref struct is a more specialized type of struct that is always allocated on the stack and cannot be moved to the heap. This is particularly useful in scenarios where you need to work with spans, buffers, or other low-level memory constructs that must remain on the stack for performance and safety reasons.

#### **Key Characteristics of ref struct:**

*   **Stack-Only Allocation**: ref struct types are always allocated on the stack and cannot be moved to the heap, ensuring they remain within a specific scope.
*   **No Boxing**: ref struct types cannot be boxed, meaning they cannot be converted to object, dynamic, or any interface type, avoiding unintended heap allocations.
*   **Restricted Use**: ref struct types cannot be used in async methods, iterator methods, or lambda expressions because these might cause them to escape to the heap.
*   **Safety**: By being restricted to the stack, ref struct types avoid issues like dangling pointers or memory leaks, making them safer for low-level programming.
*   **Usage**: Commonly used in scenarios involving spans (Span<T> and ReadOnlySpan<T>), which are often used for high-performance memory manipulation.

Example:

```csharp
public ref struct SpanWrapper
{
    private Span<int> _span;

    public SpanWrapper(Span<int> span)
    {
        _span = span;
    }

    public int this[int index]
    {
        get => _span[index];
        set => _span[index] = value;
    }
}
```

Let’s see the boxing of the struct and ref struct in the IDE.

```csharp
var point = new Point(1, 2);
var spanWrapper = new SpanWrapper([3, 4]);

var obj1 = (object)point;
var obj2 = (object)spanWrapper;
```

The compiler will show a compilation error in the last line.

Spans are ref structs in C#. You can read more about Span in my previous post “[Efficient Memory Management with Spans in .NET](/blog/efficient-memory-management-with-spans-in-dotnet/)“.

## **Changes in C# 13**

[C# 13](/blog/whats-new-in-charp13/ "C# 13") (in the preview when this post was published) reduces some restrictions for ref structs.

Before C# 13, you could not use ref structs in the async methods or iterators. C# 13 allows the declaration of local ref struct variables in the async methods or iterators. However, those variables can’t be accessed across an await boundary. Neither can they be accessed across a yield return boundary.

```csharp
async Task AsyncMethod()
{
    await Task.Delay(1000);
    ReadOnlySpan<char> span = "Hello, World".AsSpan();
    // do something with span
}
```

C# 13 allows ref structs to implement interfaces. However, you can’t cast it to the interface type because of boxing.

```csharp
Foo foo = new();
var _ = (IFoo)foo;

interface IFoo
{ }

ref struct Foo : IFoo
{ }
```

The compiler will show a compilation error in the second line.

Before C# 13, ref struct types couldn’t be declared as the type argument for a generic type or method. Now, C# 13 allows to do that with an anti-constraint, allows ref struct.

```csharp
class Foo<T> where T : allows ref struct
{
}

class Boo<T>
{
}

class Example<T> where T : allows ref struct
{
    private Foo<T> _foo; // allowed

    private Boo<T> _boo; // disallowed
}
```

## Summary

Both struct and ref struct serve essential roles in C# programming, each with advantages and limitations. Understanding their differences is crucial for making informed decisions about which type to use in various scenarios. While struct is a versatile tool for general-purpose value types, ref struct offers a specialized solution for scenarios demanding stack-only allocation and high performance.