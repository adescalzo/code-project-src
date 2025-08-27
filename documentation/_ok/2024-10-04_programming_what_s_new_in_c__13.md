```yaml
---
title: What’s New in C# 13
source: https://okyrylchuk.dev/blog/whats-new-in-charp13/
date_published: 2024-10-04T18:20:26.000Z
date_captured: 2025-08-20T21:16:58.628Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: programming
technologies: [.NET 9, C# 13]
programming_languages: [C#]
tags: [csharp, dotnet, language-features, new-features, performance, concurrency, memory-management, syntax, compiler]
key_concepts: [object-initializer, params-keyword, thread-synchronization, partial-members, ref-struct, generics, interfaces, overload-resolution, escape-sequences, method-groups]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive overview of the new features introduced in C# 13, which will be released with .NET 9. It details ten significant updates, including enhancements to object initializers with the "from the end" operator and expanded `params` keyword functionality for various collection types. The article also covers improvements in thread synchronization with a new `Lock` type, the introduction of partial properties and indexers, and the ability to use `ref struct` types in async methods and as generic type arguments. Additionally, it highlights the `OverloadResolutionPriority` attribute, a new `\e` escape sequence, and refined method group natural type resolution.
---
```

# What’s New in C# 13

# What’s New in C# 13

.NET 9 with C# 13 is coming soon. It’s time to overview what it brings for us.

## **From the End Operator in Object Initializer**

In C#, the `^` operator is called the “from the end” operator. It is used to index sequence elements starting from the end. This operator allows you to access elements relative to the end of the collection without needing to calculate the index manually.

Now, you can use the “from the end” operator in object initializer expressions.

The example below initializes an array from 5 to 1.

```csharp
var test = new Test
{
    Numbers =
        {
            [^1] = 1,
            [^2] = 2,
            [^3] = 3,
            [^4] = 4,
            [^5] = 5,
        }
};

Console.WriteLine(string.Join(", ", test.Numbers));
// 5, 4, 3, 2, 1

public class Test
{
    public int[] Numbers = new int[5];
}
```

## **Params Collections**

The `params` keyword allows you to pass a variable number of arguments to a method. It is used when the number of arguments is unknown at compile time and lets you specify a method parameter that takes a variable number of arguments of a specified type.

Before C# 13, the parameter must be a single dimensional array.

Now, you can use `params` with any recognized collection type, including Spans and types that implement `IEnumerable<T>` and have an `Add` method.

```csharp
// Before C# 13
void Method1(params object[] args) { }

// C# 13
void Method2(params IList<object> args)
{ }

// C# 13
void Method3(params ICollection<object> args)
{ }

// C# 13
void Method4(params ReadOnlySpan<object> args)
{ }
```

## **New Lock Object**

The `lock` statement in C# ensures that a block of code is executed by only one thread at a time, preventing race conditions when multiple threads try to access shared resources. It works by acquiring a lock on a specified object for the duration of the code block, ensuring that other threads must wait until the lock is released.

```csharp
public class OldLock
{
    private readonly object _lockObj = new();
    private static int sharedResource = 0;

    public void IncrementResource()
    {
        lock (_lockObj)
        {
            sharedResource++;
        }
    }
}
```

C# 13 introduces a new `Lock` type. It has a new efficient API for better thread synchronization. The `Lock.EnterScope()` method enters an exclusive scope, which is a ref struct supporting the `Dispose` pattern. It means you can use it with `using` statement as other disposable objects.

```csharp
public class NewLock
{
    private readonly Lock _lockObj = new();
    private static int sharedResource = 0;

    public void IncrementResource()
    {
        using (_lockObj.EnterScope())
        {
            sharedResource++;
        }
    }
}
```

However, you can also use a new `Lock` type with a `lock` statement. It recognizes if the target of the lock is a `Lock` object and uses a new API rather than a traditional `Monitor` API.

## **Partial Properties And Indexers**

The `partial` keyword splits the definition of a class, struct, interface, or method across multiple files. This allows you to organize code more effectively, especially in large projects or when dealing with auto-generated code.

Since C#, you can declare partial properties and indexers. This feature extends the possibilities of source generators.

```csharp
public partial class Foo
{
    [GeneratedRegex("abc|def")]
    private static partial Regex AbcRegex { get; }

    public bool IsMatchAbc(string text)
        => AbcRegex.IsMatch(text);
}
```

## **Ref in Async Methods**

Before C# 13, you couldn’t use `ref struct` (for instance, Spans) as local variables in the async methods.

The code below wouldn’t compile.

```csharp
async Task AsyncMethod()
{
    await Task.Delay(1000);
    ReadOnlySpan<char> span = "Hello, World";
    // do something with span
}
```

C# 13 allows local `ref struct` to be used in the async methods. You can also create `ref struct` local variables in the iterator methods (when you use `yield return`).

However, you cannot access such variables across an `await` and `yield return` boundary.

Additionally, in the same manner, you can use unsafe context in async and iterator methods.

## **Ref Struct as Generic Type**

Before C# 13, `ref struct` types couldn’t be used as type arguments in generic types or methods.

With the introduction of an anti-constraint allowing `ref struct`, you can use `ref struct` in generic type declarations.

This anti-constraint specifies that the type argument for a type parameter can be a `ref struct`. The compiler ensures that all instances of that type parameter follow ref safety rules.

```csharp
class Foo<T> where T : allows ref struct
{}

class Boo<T>
{}

class Example<T> where T : allows ref struct
{
    private Foo<T> _foo; // allowed

    private Boo<T> _boo; // disallowed
}
```

## **Ref Struct Interface**

C# 13 allows for `ref struct` to implement interfaces.

However, you cannot convert the `ref struct` type to an interface (line 3 in the example below) because it’s a boxing conversion, which is forbidden by ref safety. Also, you cannot declare methods explicitly implementing an interface method in the `ref struct`.

```csharp
Foo foo = new Foo();

var _ = (IFoo)foo;

interface IFoo
{ }

ref struct Foo : IFoo
{ }
```

## **Overload Resolution Priority Attribute**

C# 13 introduces a new `OverloadResolutionPriority` attribute.

API authors can use it to prioritize overloads within a type, guiding consumers to specific APIs, even when they might be ambiguous or not chosen by C#’s default overload resolution rules.

The higher number means higher priority.

In the example below, the compiler selects the `Display` method overload with the `ReadOnlySpan<char>` parameter.

```csharp
var service = new MyService();

service.Display("Hello World!");

public class MyService
{
    [OverloadResolutionPriority(1)]
    public void Display(string chars) =>
        Console.WriteLine(chars);

    [OverloadResolutionPriority(2)]
    public void Display(ReadOnlySpan<char> chars) =>
        Console.WriteLine(chars.ToArray());
}
```

## **New Escape Sequence**

Let’s see the following example.

```csharp
string text = "\u001b[32mThis is green text\u001b[0m";
Console.WriteLine(text);
```

The sequence `\u001b` is a Unicode escape sequence in C# representing the ESCAPE character (Unicode U+001B).

This character is used in ANSI escape codes to control text formatting in terminals.

*   `\u`: Indicates that the following four hexadecimal digits represent a Unicode character.
*   `001b`: The hexadecimal code for the ESCAPE character (U+001B).

The sequence `[32m` is an ANSI escape code used to change the text color in terminals that support ANSI escape sequences. Specifically, `32m` sets the text color to green.

Similarly, `[0m` is used to reset the text formatting to the default settings.

Instead of `\u001b` you could use `\x1b`. However, it’s not recommended because if the next characters following `1b` were valid hexadecimal digits, those characters became part of the escape sequence.

Since C# 13, you can use a new `\e` escape sequence.

```csharp
string text = "\e[32mThis is green text\e[0m";
Console.WriteLine(text);
```

## **Method Group Natural Type**

C# 13 introduces improvements in overload resolution for method groups.

The method group is a method, and all overloads have the same name.

Previously, the compiler generated the complete set of candidate methods for a method group. When a natural type was required, it was derived from this entire set of candidates.

The new approach is to prune the set of candidate methods at each scope, removing non-applicable candidates. For instance, it removes generic methods with the wrong number of type parameters. The process moves to the next outer scope if no candidate methods are found.