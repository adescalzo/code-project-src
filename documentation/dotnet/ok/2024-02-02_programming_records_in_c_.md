```yaml
---
title: Records in C#
source: https://okyrylchuk.dev/blog/records-in-csharp/
date_published: 2024-02-02T20:46:33.000Z
date_captured: 2025-08-06T18:29:09.327Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: programming
technologies: [.NET 5.0, .NET, C# 9.0, C# 10, Entity Framework]
programming_languages: [C#]
tags: [csharp, records, dotnet, immutability, value-equality, pattern-matching, language-features, data-transfer-objects, domain-modeling, init-accessor]
key_concepts: [records, positional-records, immutability, value-equality, nondestructive-mutations, pattern-matching, init-accessor, primary-constructor]
code_examples: true
difficulty_level: intermediate
summary: |
  This article provides a detailed exploration of C# Records, a feature introduced in C# 9.0 and .NET 5.0. It corrects the common misconception that records are inherently immutable, explaining that they offer shallow immutability and can be defined as mutable. The post covers essential record features including positional records, value equality, nondestructive mutations via `with` expressions, and compiler-generated `ToString()` overrides. It also demonstrates how records facilitate deconstruction and enhance pattern matching capabilities. The author concludes by outlining suitable use cases for records, such as DTOs and commands, while advising against their use as Entity Framework entity types.
---
```

# Records in C# 

“Records are immutable”.

I often get this answer from people asking: “What do you know about Records?”. That’s it, nothing else.

Records were introduced in C# 9.0 and released in November 2020 as part of the .NET 5.0 release. It’s almost been four years of having them!

It’s not a complete answer. First, it’s only partially accurate that records are immutable. Second, Records have more features. So, let’s dive into the details!

Table Of Contents

1.  [Positional Records](#positional-records)
2.  [Immutability](#immutability)
3.  [Value Equality](#value-equality)
4.  [Nondestructive Mutations](#nondestructive-mutations)
5.  [Compiler-generated ToString()](#compiler-generated-tostring)
6.  [Deconstructing](#deconstructing)
7.  [Pattern Matching](#pattern-matching)
8.  [Use cases](#use-cases)
9.  [Summary](#summary)

## Positional Records

Many people could not answer what positional records are, even though they answered that **Records** are immutable.

I bet you have seen Positional Records many times and probably use them all the time.

```csharp
public record Person(string Name, string Surname, int Age);
```

In the example, we use a primary constructor for the Record. The compiler will generate public properties with **init** modifiers for us. The parameters in the primary constructor are called positional parameters. The compiler creates positional properties that mirror the positional parameters. That’s why they called **Positional Records**.

## Immutability

“Records are immutable”.

That’s not a complete answer. First, you must specify that you’re talking about **Positional Records** because we can define mutable records.

```csharp
public record Person 
{ 
    public string Name { get; set; }
    public string Surname { get; set; }
    public int Age { get; set; }
}
```

Also, the properties defined with the **init** access modifier (by the compiler or you) have only **_shallow immutability_**.

With **init** you can’t change the value or the reference of reference type properties. But you can change the data that a reference type property refers to.

Let’s see the example with the following Record.

```csharp
public record Post( string Name, string Content, string[] Tags);
```

Our Post has an array of Tags. After initialization, we can’t change the Tags with a new array.

```csharp
var post = new Post("My Post", "My Content", ["tag1", "tag2"]);
post.Tags = ["tag3"];
```

However, we can change the values of existing Tags 🙂

```csharp
var post = new Post("My Post", "My Content", ["tag1", "tag2"]);
post.Tags[0] = "tag3";

Console.WriteLine(post.Tags[0]);
// tag3
```

It’s important to remember that **Records** do not have deep immutability but shallow immutability.

## Value Equality

When Records were released, they were reference types. Since C# 10, we can also create Records as value types.

```csharp
// Reference type
public record Person(string Name, string Surname, int Age);

// Reference type
public record class Person(string Name, string Surname, int Age);

// Value type
public record struct Person(string Name, string Surname, int Age);
```

The first option creates a reference type by default. The .NET team left it for backward compatibility.

But let’s go back to reference types. As we know, the reference types are compared by references in .NET. Records are different. They are compared **by values**.

```csharp
var johnDoe1 = new Person("John", "Doe", 30);
var johnDoe2 = new Person("John", "Doe", 30);

Console.WriteLine(johnDoe1 ==  johnDoe2);
// True
```

## Nondestructive Mutations

We can create a new instance of the Record with some modifications. It’s called **nondestructive mutation**. We can do it with a **with** expression.

```csharp
var johnDoe = new Person("John", "Doe", 30);

var janeDoe = johnDoe with { Name = "Jane", Age = 27 };
```

We created a new instance **janeDoe** from **johnDoe** changing only **Name** and **Age** properties. The **Surname** property is copied.

## **Compiler-generated ToString()**

The compiler generates a built-in display of the Record properties for us. It’s beneficial because you know that the not overridden **ToString()** method in classes and structures returns only the type name.

The format is the following.  
**<record type name> { <property name> = <value> }** 

```csharp
Console.WriteLine(johnDoe);
// Person { Name = John, Surname = Doe, Age = 30 }

Console.WriteLine(janeDoe);
// Person { Name = Jane, Surname = Doe, Age = 27 }
```

## Deconstructing

We can deconstruct the Record into variables. However, we can do it **only with Positional Records**! It doesn’t work for usual Records. 

```csharp
var (name, surname, age) = johnDoe;
Console.WriteLine(name);    // John
Console.WriteLine(surname); // Doe
Console.WriteLine(age);     // 30
```

## Pattern Matching

The Positional Records work seamlessly with C#’s pattern-matching syntax. They can be used in switch expressions to destructure and match on properties easily.

```csharp
var white = new Color(255, 255, 255);

var color = white switch
{
    Color(255, 255, 255) => "White",
    Color(0, 0, 0) => "Black",
    Color(255, 0, 0) => "Red",
    Color(0, 255, 0) => "Green",
    Color(0, 0, 255) => "Blue",
    _ => "Unknown"
};

Console.WriteLine(color); // White

public record Color(int Red, int Green, int Blue);
```

More about [Pattern Matching](http://blog/pattern-matching-in-csharp/ "Pattern Matching") read in my other post.

## Use cases

We can use them for DTOs, Events, Commands, and other immutable models.

Records are suitable for representing data received from external APIs or for deserializing JSON objects into strongly typed structures.

Records are helpful when you want instances with the same values to be considered equal.

Records can be used to create lightweight, immutable representations of events or messages when logging or tracing information.

However, Records are not suitable for representing entity types in Entity Framework.

## Summary

Remember to mention Positional Records when saying that Records are immutable by default. Also, Records have shallow immutability.

The Records is an excellent feature in C#. They are powerful when we understand all their features.

Oh, wait! Records support [inheritance](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#inheritance "inheritance"), like classes. But it’s a story for another post 🙂 

Post navigation

[← Previous Post](https://okyrylchuk.dev/blog/handling-exceptions-in-asp-net-core-8/ "Handling Exceptions with IExceptionHandler in ASP.NET Core 8")

[Next Post →](https://okyrylchuk.dev/blog/pattern-matching-in-csharp/ "Pattern Matching in C#")

## Related Posts

![Thumbnail for an article titled 'Pattern Matching in C#', featuring a dark blue background with abstract light blue lines and large yellow text.](https://okyrylchuk.dev/wp-content/uploads/2024/09/featured6.png.webp)

### [Pattern Matching in C#](https://okyrylchuk.dev/blog/pattern-matching-in-csharp/)

[.NET](https://okyrylchuk.dev/blog/category/dotnet/), [C#](https://okyrylchuk.dev/blog/category/csharp/) / February 9, 2024

![Thumbnail for an article about System.Text.Json, showing a dark blue background with abstract light blue lines and the beginning of the title "Source Generation".](https://okyrylchuk.dev/wp-content/uploads/2024/02/featured8.png.webp)

### [Intro to Serialization with Source Generation in System.Text.Json](https://okyrylchuk.dev/blog/intro-to-serialization-with-source-generation-in-system-text-json/)

[.NET](https://okyrylchuk.dev/blog/category/dotnet/), [JSON](https://okyrylchuk.dev/blog/category/json/), [Source Generators](https://okyrylchuk.dev/blog/category/source-generators/), [System.Text.Json](https://okyrylchuk.dev/blog/category/system-text-json/) / February 23, 2024