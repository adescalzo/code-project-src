```yaml
---
title: "C# 13: How params Collections Transform Development | by Juan España | Medium"
source: https://medium.com/@juanal98/c-13-how-params-collections-transform-development-abdcc1729d4d
date_published: 2024-12-02T17:48:58.487Z
date_captured: 2025-08-08T18:07:15.401Z
domain: medium.com
author: Juan España
category: programming
technologies: [C# 13, C# 12, .NET, "Span<T>", "IReadOnlyList<T>", "IEnumerable<T>", LINQ]
programming_languages: [C#]
tags: [csharp, dotnet, language-features, collections, params, performance, code-optimization, csharp-13, csharp-12]
key_concepts: [params-keyword, collection-expressions, stack-allocation, heap-allocation, method-overloading, type-inference, code-readability]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores the new `params` collection feature introduced in C# 13, which extends the `params` keyword to accept any collection type compatible with C# 12's collection expressions, moving beyond the previous array-only limitation. It highlights how this enhancement improves code clarity, flexibility, and performance, particularly when using `Span<T>` for stack allocations. The author details the benefits, such as optimized performance, clearer method intent, and greater flexibility for method users. The article also discusses method overloading and resolution with the new `params` capabilities, providing practical C# code examples throughout.]
---
```

# C# 13: How params Collections Transform Development | by Juan España | Medium

# C# 13: How params Collections Transform Development

![A banner image with a dark purple background featuring the C# logo and 'C# 13' in white text. Below it, the text 'HOW PARAMS COLLECTIONS TRANSFORM DEVELOPMENT' is displayed in purple. The ByteHide logo is in the bottom right corner.](https://miro.medium.com/v2/resize:fit:700/1*eB9dvMfHO-mYLRjcDqwlTg.png)

With the arrival of C# 13, the language continues to evolve to provide a smoother, safer, and more efficient development experience. One of the standout features in this new version is the ability to use any type of collection compatible with collection expressions in `params` parameters. This enhancement, while seemingly small, opens up new possibilities for writing clearer, more optimized, and expressive code.

In this article, we’ll explore how this new feature works, how it relates to the collection expressions introduced in C# 12, and why it should be part of your toolkit as a .NET developer.

# What are Collection Expressions?

Collection expressions, introduced in C# 12, provide a simpler and more unified way to create different types of collections. Previously, creating collections required methods and syntax specific to the type of collection being used.

For example, before C# 12, using arrays or `Span` required more verbose syntax:

```csharp
// Before C# 12  
WriteByteArray(new[] { (byte)1, (byte)2, (byte)3 });  
WriteByteSpan(stackalloc[] { (byte)1, (byte)2, (byte)3 });
```

With collection expressions, this process is significantly simplified, improving readability and reducing code complexity:

```csharp
// After C# 12  
WriteByteArray([1, 2, 3]);  
WriteByteSpan([1, 2, 3]);  
  
static void WriteByteArray(byte[] bytes) { }  
static void WriteByteSpan(Span<byte> bytes) { }
```

In addition to improving syntax, these expressions automatically infer both the type of the collection and the type of its elements, making them more intuitive and easier to use.

# `params` in C# 13: Beyond Arrays

Since its introduction in C# 1.0, `params` parameters have allowed developers to declare methods that accept a variable number of arguments, passed as an array. This feature simplifies method calls when the number of arguments is unknown at compile time.

However, until C# 12, `params` parameters were exclusively limited to arrays. For example:

```csharp
static void WriteByteArray(params byte[] bytes) { }  
  
// Usage  
WriteByteArray(1, 2, 3);// List of values  
WriteByteArray();// No values
```

C# 13 removes this limitation by allowing `params` parameters to accept any type of collection compatible with collection expressions. This includes types like `Span<T>`, `IReadOnlyList<T>`, and even `IEnumerable<T>`:

```csharp
static void WriteByteSpan(params Span<byte> bytes) { }  
  
// Usage  
WriteByteSpan(1, 2, 3);  
WriteByteSpan();
```

This flexibility not only improves code clarity but can also optimize performance in certain scenarios, as we’ll see below.

# Benefits of `params` with Collections

**1. Optimized Performance with** `**Span<T>**`

Using collections like `Span<T>` instead of traditional arrays allows the compiler to create the collection directly on the stack, avoiding heap allocations. This reduces memory overhead and improves performance:

```csharp
static void WriteNumbers(params Span<int> numbers) { }   
WriteNumbers(1, 2, 3); // Uses stackalloc internally
```

**2. Clearer Communication of Method Purpose**

Specifying more descriptive types like `IReadOnlyList<T>` or `IEnumerable<T>` helps method users understand how the data will be handled within the method. For example:

```csharp
static void ProcessData(params IReadOnlyList<int> data) { }
```

Here, the `IReadOnlyList<T>` type clearly communicates that the collection will not be modified within the method.

**3. Greater Flexibility for Method**

Users Developers can pass data in a variety of forms: lists of values, arrays, LINQ collections, and more. For example:

```csharp
static void WriteByteArray(params IEnumerable<byte> bytes) { }    
byte[] data = [1, 2, 3, 4];    
WriteByteArray(data.Where(x => x < 4));// Filters values before passing
```

**4. Compatibility with Preexisting Collections**

`params` parameters now accept any collection implementing IEnumerable, allowing smoother integration with the .NET ecosystem.

# Overloading and Method Resolution

Support for collections in params parameters also enhances method overloading capabilities in C#. For example, you can have two methods with the same name but different types of params parameters:

```csharp
static void WriteNumbers(params IEnumerable<int> values) => Console.WriteLine("IEnumerable");  
static void WriteNumbers(params ReadOnlySpan<int> values) => Console.WriteLine("Span");
```

The compiler selects the most appropriate overload based on context:

*   A list of values or an array is mapped to `Span<T>`
*   Collections like `List<T>` or results from LINQ queries are mapped to `IEnumerable<T>`.

```csharp
WriteNumbers(1, 2, 3);// Span  
WriteNumbers(new[] { 1, 2, 3 });// Span  
WriteNumbers(new List<int> { 1 });// IEnumerable
```

This improves efficiency by leveraging Span whenever possible while maintaining flexibility with other collections.

# Considerations When Using `params`

Although this new feature expands the language’s capabilities, it’s important to maintain consistency in method functionality. If multiple overloads have the same name, they should perform similar tasks, differing only in performance or how they process data.

For example, overloaded methods should delegate to a common implementation to avoid inconsistencies:

```csharp
private static void WriteNumbers(params IEnumerable<int> values) => WriteCommon(values);  
private static void WriteNumbers(params ReadOnlySpan<int> values) => WriteCommon(values.ToArray());  
  
private static void WriteCommon(IEnumerable<int> values) => Console.WriteLine(string.Join(", ", values));
```

# Conclusion

C# 13 transforms the use of `params` parameters by enabling collections beyond traditional arrays. This change not only simplifies how developers write and consume methods but also introduces significant improvements in terms of performance and flexibility.

With this new capability, code becomes clearer and more adaptive, facilitating the integration of different collection types and optimizing resources. It’s a powerful tool for writing modern, efficient applications, and incorporating this feature into your projects can make a real difference in your development workflows. Now is the perfect time to experiment with these improvements and take full advantage of the new possibilities that C# 13 offers.