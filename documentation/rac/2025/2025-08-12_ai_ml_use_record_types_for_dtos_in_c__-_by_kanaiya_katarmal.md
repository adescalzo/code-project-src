```yaml
---
title: Use Record Types for DTOs in C# - by Kanaiya Katarmal
source: https://newsletter.kanaiyakatarmal.com/p/use-record-types-for-dtos-in-c
date_published: 2025-08-12T04:30:15.000Z
date_captured: 2025-09-09T14:57:49.177Z
domain: newsletter.kanaiyakatarmal.com
author: Kanaiya Katarmal
category: ai_ml
technologies: [C# 9, .NET]
programming_languages: [C#]
tags: [csharp, records, dtos, immutability, data-transfer, dotnet, csharp-9, value-equality, code-quality, best-practices]
key_concepts: [Data Transfer Objects (DTOs), Record types, Immutability, Value-based equality, Boilerplate reduction, Positional parameters, Reference types, init-only setters]
code_examples: false
difficulty_level: intermediate
summary: |
  This article advocates for using C# record types as Data Transfer Objects (DTOs) to simplify data transfer in modern C# development. It explains how records offer significant advantages over traditional classes by providing immutability by default and automatic value-based equality, thereby reducing boilerplate code. The post illustrates both positional and named property syntax for records and discusses scenarios where they are most beneficial. It also outlines situations where records might not be the ideal choice, such as when mutability is required or when working with older C# versions. Ultimately, records are presented as a cleaner, safer, and more concise solution for defining DTOs, especially from C# 9 onwards.
---
```

# Use Record Types for DTOs in C# - by Kanaiya Katarmal

# Use Record Types for DTOs in C#

### Simplifying Data Transfer with Immutable and Concise Structures

In modern C# development, **Data Transfer Objects (DTOs)** are everywhere — transporting data between APIs, services, and databases.

Traditionally, we’ve defined them as **classes**:

```csharp
public sealed class ProductResponse
{
    public Guid Id        { get; set; }
    public string Name    { get; set; }
    public decimal Price  { get; set; }
    public string? Brand  { get; set; }
}
```

It works, but it comes with **extra boilerplate** and **mutable state** we often don’t need.

With **C# 9**, we now have a better option: **record types**.

---

## What Is a Record?

A **record** is a special kind of reference type in C# designed for **immutable data** and **value-based equality**.

Key features:

*   **Immutable by default** — values can only be set at creation time.
*   **Value-based equality** — two records with the same values are considered equal.
*   **Concise syntax** — define all properties in a single line.

Example:

```csharp
public sealed record ProductResponse(
    Guid Id,
    string Name,
    decimal Price,
    string? Brand);
```

This one-liner replaces the verbose class version and makes your DTOs safer and cleaner.

## Why Records Are Great for DTOs

### 1. Encourages Immutability

Most DTOs are read-only after creation. Records enforce this when using positional parameters.

```csharp
var product = new ProductResponse(Guid.NewGuid(), "Laptop", 1299.99m, "Contoso");
// product.Price = 999.99m; ❌ Compile-time error
```

No accidental property changes after initialization.

### 2. Value-Based Equality

With classes, two objects with the same values are not equal unless you manually override `Equals` and `GetHashCode`.
Records solve this automatically:

```csharp
var p1 = new ProductResponse(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Laptop", 1299.99m, "Contoso");
var p2 = new ProductResponse(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Laptop", 1299.99m, "Contoso");

Console.WriteLine(p1 == p2); // True ✅
```

### 3. Less Boilerplate, More Clarity

With classes, every property must be explicitly written.
With records, you can define everything inline — making DTOs shorter and easier to maintain.

## Alternative Syntax

If you prefer named properties over positional parameters, you can still use records:

```csharp
public sealed record ProductResponse
{
    public Guid Id       { get; init; }
    public string Name   { get; init; }
    public decimal Price { get; init; }
    public string? Brand { get; init; }
}
```

Same benefits, just a different style.

## When Not to Use Records

Records are not always the right choice:

*   Your DTO needs to be **mutable** after creation.
*   You’re using **C# 8 or earlier**.
*   Your team prefers explicit property declarations.

## Final Thoughts

DTOs are about **moving data cleanly and safely**.
C# records fit this role perfectly — concise, immutable, and equality-friendly.

If you’re still writing verbose class-based DTOs, try records in your next project.
You’ll write less code, avoid accidental bugs, and keep your data models cleaner.