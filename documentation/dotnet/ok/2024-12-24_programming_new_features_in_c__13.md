```yaml
---
title: New Features in C# 13
source: https://antondevtips.com/blog/new-features-in-csharp-13
date_published: 2024-12-24T08:55:52.402Z
date_captured: 2025-08-06T17:34:52.410Z
domain: antondevtips.com
author: Anton Martyniuk
category: programming
technologies: [.NET 9, C# 13, System.Threading.Lock, System.Threading.Monitor, System.Runtime.CompilerServices, "System.Span<T>", "System.ReadOnlySpan<T>", LINQ]
programming_languages: [C#]
tags: [csharp, dotnet, new-features, language-features, concurrency, performance, ref-struct, async, iterators, source-generators]
key_concepts: [params-collections, thread-synchronization, partial-members, overload-resolution, implicit-index-access, ref-safety, ref-structs, async-iterators]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides an overview of 9 new features introduced in C# 13, released alongside .NET 9. It highlights the expanded `params` keyword, which now supports various collection types like `List<T>` and `Span<T>`. A significant addition is the `System.Threading.Lock` object for more efficient thread synchronization, replacing the older `Monitor` API. The update also brings support for partial properties and indexers, useful for source generators, and introduces `OverloadResolutionPriority` for method suggestion ordering. Furthermore, C# 13 enables implicit index access using the `^` operator in object initializers, allows `ref` and `unsafe` contexts within async and iterator methods (with boundary restrictions), and permits `ref struct` types as generic arguments and interface implementations while enforcing ref safety rules.
---
```

# New Features in C# 13

![A dark blue background with abstract purple shapes. On the left, a white square icon with a code tag (`</>`) inside is positioned above the text "dev tips". On the right, in large white sans-serif font, the text "NEW FEATURES IN C# 13" is displayed.](https://antondevtips.com/media/covers/csharp/cover_csharp_13.png)

# New Features in C# 13

Dec 24, 2024

[Download source code](/source-code/new-features-in-csharp-13)

3 min read

.NET 9 and C# 13 were released on November 12, 2024. In this blog post, I want to show you a list of new features in C# 13.

## 1. Params Collections

Previously, when using `params` keyword you were able to use only arrays:

```csharp
PrintNumbers(1, 2, 3, 4, 5);

public void PrintNumbers(params int[] numbers)
{
    // ...
}
```

C# 13 introduces Params Collections, allowing you to use the following concrete types:

*   Arrays
*   IEnumerable<T>
*   List<T>
*   Span<T>

For example, you can use a list:

```csharp
List<int>numbers = [ 1, 2, 3, 4, 5 ];
PrintNumbers(numbers);

public void PrintNumbers(params List<int> numbers)
{
    // ...
}
```

And further in `PrintNumbers` method you can use, for example, LINQ methods over the params collection.

## 2. New Lock Object

`System.Threading.Lock` is a new thread synchronization type in .NET 9 runtime. It offers better and faster thread synchronization through the API.

How it works:

*   Lock.EnterScope() method enters an exclusive scope and returns a ref struct.
*   Dispose method exits the exclusive scope

When you replace `object` with a new `Lock` type inside a `lock` statement, it starts using a new thread synchronization API. Rather than using old API through `System.Threading.Monitor`.

```csharp
public class LockClass
{
    private readonly System.Threading.Lock _lockObj = new();

    public void Do(int i)
    {
        lock (_lockObj)
        {
            Console.WriteLine($"Do work: {i}");
        }
    }
}
```

## 3. Partial Properties and Indexers

C# 13 adds support for partial `Properties` and `Indexers`. Partial Properties and Indexers let you split the logic for getters, setters, and index accessors across multiple files.

This feature can be useful particularly for source generators.

```csharp
public partial class C
{
    // Declaring declaration
    public partial string Name { get; set; }
}

public partial class C
{
    // implementation declaration:
    private string _name;
    public partial string Name 
    {
        get => _name;
        set => _name = value;
    }
}
```

## 4. Overload Resolution Priority

With **Overload Resolution Priority**, you can select the exact order of methods that will be shown in the code editor suggestions.

```csharp
using System.Runtime.CompilerServices;

public static class MappingExtensions
{
    [OverloadResolutionPriority(1)]
    public static Product ToResponse(this Product entity, Result result)
        => new Product(...);

    [OverloadResolutionPriority(0)]
    public static Product ToResponse(this Product entity)
        => new Product(...);
}
```

## 5. Implicit Index Access

C# 13 now allows using "from the end" index operator **(^)** in an object initializer expression.

For example, you can now initialize an array that counts down from 4 to 0:

```csharp
var countdown = new TimerRemaining()
{
    buffer =
    {
        [^1] = 0,
        [^2] = 1,
        [^3] = 2,
        [^4] = 3,
        [^5] = 4
    }
};
```

In previous C# versions, you were not able to use the **^** operator in object initializers.

## 6. New Escape Sequence

In previous C# versions you needed to use `\u001b` or `\x1b` for the ESCAPE character. But this code had potential for collisions with other sequences.

In C# 13 you can use `\e` as a character for the ESCAPE character.

```csharp
var text = "\x1b[1mThis text is bold\x1b[0m";
Console.WriteLine(text);

var text = "\e[1mThis text is bold\e[0m";
Console.WriteLine(text);
```

## 7. Ref and Unsafe in Iterators and Async Methods

Before C# 13, the following methods couldn't declare local **ref** variables or have an unsafe context:

*   async methods
*   iterator methods - methods that use yield return

C# 13 removes these restrictions, and you can use Spans (that are ref structs) inside async methods:

```csharp
public async Task ProcessTextAsync()
{
    string text = await GetTextAsync(...);
    
    ReadOnlySpan<char> span = text;
    span = span.Slice(2, 10);

    Console.WriteLine(span.ToString());
}
```

However, those ref struct and unsafe variables can't be accessed across an await boundary. Neither can they be accessed across a yield return boundary.

## 8. Allows Ref Struct

Before C# 13, ref struct types couldn't be declared as the type argument for a generic type or method.

Now, generic type declarations can add an anti-constraint, **allows ref struct**. This anti-constraint declares that the type argument supplied for that type parameter can be a ref struct type. The compiler enforces ref safety rules on all instances of that type parameter.

This enables types such as `System.Span<T>` and `System.ReadOnlySpan<T>` to be used with generic algorithms, where applicable.

```csharp
public class MyClass<T> where T : allows ref struct
{
    // Use T as a ref struct:
    public void Do(scoped T p)
    {
        // The parameter p must follow ref safety rules
    }
}
```

## 9. Ref Struct Interfaces

Starting from C# 13, ref struct can now implement interfaces.

However, to ensure ref safety rules, a ref struct type can't be converted to an interface type. That conversion is a boxing conversion, and could violate ref safety. From that rule, ref struct types can't declare methods that explicitly implement an interface method. Also, ref struct types must implement all methods declared in an interface, including those methods with a default implementation.

```csharp
public interface ICoffee
{
}

public ref struct Coffee : ICoffee
{
}

Coffee coffee = new Coffee();

// This is not allowed
ICoffee coffee2 = (ICoffee) coffee;
```

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/new-features-in-csharp-13)