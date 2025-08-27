```yaml
---
title: Three new LINQ methods in .NET 9
source: https://steven-giesel.com/blogPost/0594ba85-356b-47f1-89a9-70e9761c582e?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=unit-testing-clean-architecture-use-cases&_bhlid=e28224014ba6b6b302a0d29cf8cca1d3679df36d
date_published: 2024-01-27T07:00:00.000Z
date_captured: 2025-08-12T18:16:43.704Z
domain: steven-giesel.com
author: Unknown
category: architecture
technologies: [.NET 9, .NET 8, LINQ]
programming_languages: [C#]
tags: [linq, dotnet, csharp, .net-9, language-features, collection-methods, data-aggregation]
key_concepts: [linq-extension-methods, data-aggregation, collection-processing, countby, aggregateby, index]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces three new LINQ methods that are part of .NET 9: `CountBy`, `AggregateBy`, and `Index`. It explains the purpose of each method, providing clear C# code examples to demonstrate their usage and output. The `CountBy` method groups elements by a selector and returns their counts, while `AggregateBy` allows for custom aggregation based on a key and a seed. The `Index` method provides both the item and its index within a collection, with a note on its parameter order compared to `Select`.
---
```

# Three new LINQ methods in .NET 9

# Three new LINQ methods in .NET 9

1/27/2024

2 minute read

Even though we are in the alpha of .NET 9 and .NET 8 was released not more than two months ago, the dotnet team does not sleep and pushes new changes! In this blog post, we are checking what new methods were added to everyones favorite: **LINQ**.

## `CountBy`

Many **LINQ** functions have a **By** extension, where you can provide a selector function to group the elements by. For example, `MinBy`, `MaxBy`, `DistinctBy`, and so on. Now we have a new one: `CountBy`. It groups the elements by the selector function and returns an enumeration of `KeyValuePair`s. The key is the object, and the value is the count of the elements in the group.

```csharp
public record Person(string FirstName, string LastName);

List<Person> people =
[
    new("Steve", "Jobs"),
    new("Steve", "Carell"),
    new("Elon", "Musk")
];

foreach (var peopleWithSameFirstName in people.CountBy(p => p.FirstName))
{
    Console.WriteLine($"There are {peopleWithSameFirstName.Value} people with the name {peopleWithSameFirstName.Key}");
}
```

This prints:

```no
There are 2 people with the name Steve
There are 1 people with the name Elon
```

## `AggregateBy`

This method is similar to `CountBy`, but instead of counting the elements, it aggregates them. You can provide a seed and an aggregation function. The aggregation function gets the current aggregation and the current element as parameters. The aggregation function must return the new aggregation.

```csharp
public record Person(string FirstName, string LastName, int SomeNumber);

List<Person> people =
[
    new("Steve", "Jobs", 10),
    new("Steve", "Carell", 100),
    new("Elon", "Musk", 10)
];

var aggregateBy = people.AggregateBy(person => person.SomeNumber, x => 0, (x, y) => x + y.SomeNumber);
foreach (var kvp in aggregateBy)
{
    Console.WriteLine($"Sum of SomeNumber for Key {kvp.Key} is {kvp.Value}");
}
```

This will print:

```no
Sum of SomeNumber for key 10 is 20
Sum of SomeNumber for key 100 is 100
```

## `Index`

Now this isn't necessarily really new, because you could do this as of today. `Index` does return the item and the index of the item in the collection.

```csharp
public record Person(string FirstName, string LastName);

List<Person> people =
[
    new("Steve", "Jobs"),
    new("Steve", "Carell"),
    new("Elon", "Musk")
];

foreach (var (index, item) in people.Index())
{
    Console.WriteLine($"Entry {index}: {item}");
}
```

This will print:

```no
Entry 0: Person { FirstName = Steve, LastName = Jobs }
Entry 1: Person { FirstName = Steve, LastName = Carell }
Entry 2: Person { FirstName = Elon, LastName = Musk }
```

You could utilize `Select` overload to do the same:

```csharp
foreach (var (item, index) in people.Select((item, index) => (item, index)))
{
    Console.WriteLine($"Entry {index}: {item}");
}
```

What bugs me the most is, that the order of `index` and `item` is reversed! But that was a concious decision by the dotnet team:

> We decided to return Index first, Value second because it feels more natural despite the fact that the existing Select() method provides value first and index second.

Source: [https://github.com/dotnet/runtime/issues/95563#issuecomment-1852656539](https://github.com/dotnet/runtime/issues/95563#issuecomment-1852656539)

## More information

*   `CountBy`: [https://github.com/dotnet/runtime/issues/77716](https://github.com/dotnet/runtime/issues/77716)
*   `AggregateBy`: [https://github.com/dotnet/runtime/issues/91533](https://github.com/dotnet/runtime/issues/91533)
*   `Index`: [https://github.com/dotnet/runtime/issues/95563](https://github.com/dotnet/runtime/issues/95563)