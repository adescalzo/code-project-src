```yaml
---
title: The New LINQ Methods from .NET 6 to .NET 9
source: https://antondevtips.com/blog/the-new-linq-methods-from-dotnet-6-to-dotnet-9
date_published: 2025-01-28T06:59:20.592Z
date_captured: 2025-08-06T17:35:25.349Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [.NET 6, .NET 8, .NET 9, LINQ]
programming_languages: [C#]
tags: [linq, dotnet, csharp, collections, data-manipulation, code-reduction, new-features, programming, performance, productivity]
key_concepts: [linq-extensions, collection-manipulation, code-simplification, grouping, aggregation, indexing, range-expressions]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores new LINQ methods introduced in .NET versions 6 through 9, designed to streamline common collection operations. It demonstrates how methods like `Chunk`, extended `Zip`, `MinBy`, `MaxBy`, `Take` with range support, `CountBy`, `AggregateBy`, and `Index` simplify code previously requiring more verbose implementations. Each section provides clear "before" and "after" C# code examples, illustrating the benefits of these new additions. The post highlights how these enhancements reduce boilerplate, improve readability, and boost developer productivity when working with data collections in C#.
---
```

# The New LINQ Methods from .NET 6 to .NET 9

![A dark blue banner with abstract purple shapes. On the left, a white square icon with a black angle bracket symbol `</>` inside, and below it, the text "dev tips". On the right, large white text reads "THE NEW LINQ METHODS FROM .NET 6 TO .NET 9".](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fcsharp%2Fcover_csharp_linq_methods_net6_to_net9.png&w=3840&q=100)

# The New LINQ Methods from .NET 6 to .NET 9

Jan 28, 2025

[Download source code](/source-code/the-new-linq-methods-from-dotnet-6-to-dotnet-9)

5 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

In this blog post, we'll explore some of the latest enhancements to LINQ introduced between .NET 6 and .NET 9. All these methods are really amazing and reduce a significant amount of code you had to write before.

Let's dive in.

## 1. Chunk

Splitting large collections into smaller sub-collections (or chunks) required a lot of code. You might have written loops using `Skip` and `Take` repeatedly, just to break a list into evenly-sized chunks.

```csharp
var fruits = new List<string> { "Banana", "Pear", "Apple", "Orange" };
var chunkSize = 2;

var chunks = new List<string[]>();
for (var i = 0; i < fruits.Count; i += chunkSize)
{
    chunks.Add(fruits.Skip(i).Take(chunkSize).ToArray());
}

foreach (var chunk in chunks)
{
    Console.WriteLine($"[{string.Join(", ", chunk)}]");
}
```

In .NET 6, `Chunk` method provides a straightforward way to splitting a collection into chunks with a single line of code:

```csharp
var fruits = new List<string> { "Banana", "Pear", "Apple", "Orange" };
var chunks = fruits.Chunk(2);

foreach (var chunk in chunks)
{
    Console.WriteLine($"[{string.Join(", ", chunk)}]");
}
```

**How it works:**

`Chunk` method partitions the collection into sub-arrays of a given size. If the last chunk is not large enough to fill the specified size, it just returns a smaller chunk.

## 2. Zip

The `Zip` extension method has been around for a while, but .NET 6 made it more powerful by allowing you to combine three (and more) sequences seamlessly.

Here is how you were able to combine three (or more) collections:

```csharp
public record FruitData(string Name, string Color, string Origin);

var fruits = new List<string> { "Banana", "Pear", "Apple" };
var colors = new List<string> { "Yellow", "Green", "Red" };
var origins = new List<string> { "Ecuador", "France", "Canada" };

var combined = new List<FruitData>();

for (var i = 0; i < fruits.Count; i++)
{
    // Assuming all lists have the same length
    var name = fruits[i];
    var color = colors[i];
    var origin = origins[i];

    combined.Add(new FruitData(name, color, origin));
}

foreach (var data in combined)
{
    Console.WriteLine($"{data.Name} is {data.Color} and comes from {data.Origin}");
}
```

Instead of doing multiple passes or writing triple loops, you can now zip them all at once:

```csharp
var fruits = new List<string> { "Banana", "Pear", "Apple" };
var colors = new List<string> { "Yellow", "Green", "Red" };
var origins = new List<string> { "Ecuador", "France", "Canada" };

var zipped = fruits.Zip(colors, origins);

Console.WriteLine("Now in .NET 6 (Zip with three sequences):");
foreach (var (fruit, color, origin) in zipped)
{
    Console.WriteLine($"{fruit} is {color} and comes from {origin}");
}
```

**How It Works:**

`Zip` can merge two, three, or more sequences into a single sequence of tuples. Each element in the resulting sequence is a tuple containing items from each source list at the same index.

## 3, 4: MinBy, MaxBy

Selecting the minimum or maximum object from a sequence based on a particular property often required sorting or writing custom aggregations:

```csharp
var raceCars = new List<RaceCar>
{
    new RaceCar("Mach 5", 220),
    new RaceCar("Bullet", 180),
    new RaceCar("RoadRunner", 250)
};

// Finding max or min meant sorting or using Aggregate
var fastestCar = raceCars
    .OrderByDescending(car => car.Speed)
    .First();
    
var slowestCar = raceCars
    .OrderBy(car => car.Speed)
    .First();

Console.WriteLine($"Fastest Car: {fastestCar.Name}, Speed: {fastestCar.Speed}");
Console.WriteLine($"Slowest Car: {slowestCar.Name}, Speed: {slowestCar.Speed}");
```

Notice how sorting is done just to pick the first result, which can be less performant for larger lists.

With `MinBy` and `MaxBy`, you can do so directly without additional overhead.

```csharp
var raceCars = new List<RaceCar>
{
    new RaceCar("Mach 5", 220),
    new RaceCar("Bullet", 180),
    new RaceCar("RoadRunner", 250)
};

var fastestCar = raceCars.MaxBy(car => car.Speed);
var slowestCar = raceCars.MinBy(car => car.Speed);

Console.WriteLine($"Fastest Car: {fastestCar.Name}, Speed: {fastestCar.Speed}");
Console.WriteLine($"Slowest Car: {slowestCar.Name}, Speed: {slowestCar.Speed}");
```

**How It Works:**

*   `MaxBy` returns the element with the highest value based on your selector.
*   `MinBy` returns the element with the lowest value.

## 5. Range Support for Take

If you've used range expressions `collection[start..end]` in C#, you know how convenient slicing can be.

But when it comes to LINQ, you had to use both `Skip` and `Take` methods:

```csharp
var fruits = new List<string> { "Banana", "Pear", "Apple", "Orange", "Plum" };
        
// Slice from index 2 to 3
var slice = fruits.Skip(2).Take(2);

foreach (var fruit in slice)
{
    Console.WriteLine(fruit);
}
```

Now, starting from .NET 8, you can apply range-based slicing directly in LINQ using `Take` method with range support:

```csharp
var fruits = new List<string> { "Banana", "Pear", "Apple", "Orange", "Plum" };

var slice = fruits.Take(2..4);

foreach (var fruit in slice)
{
    Console.WriteLine(fruit);
}
```

**How It Works:**

`Take(2..4)` indicates you want elements starting at index 2 up to (but not including) index 4.

## 6. CountBy

If you do group-counting, e.g., "count the number of orders by name" you had to write a `GroupBy(...).Count()` or something similar.

```csharp
public record Order(string Name, string Category, int Quantity, double Price);

var orders = new List<Order>
{
    new Order("Phone", "Electronics", 2, 299.99),
    new Order("Phone", "Electronics", 2, 299.99),
    new Order("TV", "Electronics", 1, 499.99),
    new Order("TV", "Electronics", 1, 499.99),
    new Order("TV", "Electronics", 1, 499.99),
    new Order("Bread", "Groceries", 5, 2.49),
    new Order("Milk", "Groceries", 2, 1.99)
};

var countByName = orders
    .GroupBy(p => p.Name)
    .ToDictionary(
        g => g.Key,
        g => g.Count()
    );

foreach (var item in countByName)
{
    Console.WriteLine($"Name: {item.Key}, Count: {item.Value}");
}
```

`CountBy(...)` simplifies the approach by automatic grouping and counting in one step. This feature was released in .NET 9:

```csharp
var orders = new List<Order>
{
    new Order("Phone", "Electronics", 2, 299.99),
    new Order("Phone", "Electronics", 2, 299.99),
    new Order("TV", "Electronics", 1, 499.99),
    new Order("TV", "Electronics", 1, 499.99),
    new Order("TV", "Electronics", 1, 499.99),
    new Order("Bread", "Groceries", 5, 2.49),
    new Order("Milk", "Groceries", 2, 1.99)
};

var countByName = orders.CountBy(p => p.Name);

foreach (var item in countByName)
{
    Console.WriteLine($"Name: {item.Key}, Count: {item.Value}");
}
```

**How It Works:**

*   `CountBy` automatically groups your elements by a key and counts how many times that key appears.
*   Returns a dictionary-like structure of key → count.

## 7. AggregateBy

Aggregating values by groups is another common pattern: summing total price by category, computing average score by department, etc.

```csharp
public record Order(string Name, string Category, int Quantity, double Price);

var orders = new List<Order>
{
    new Order("Phone", "Electronics", 2, 299.99),
    new Order("TV", "Electronics", 1, 499.99),
    new Order("Bread", "Groceries", 5, 2.49),
    new Order("Milk", "Groceries", 2, 1.99)
};

var totalPricesByCategory = orders
    .GroupBy(x => x.Category)
    .ToDictionary(
        g => g.Key,
        g => g.Sum(x => x.Quantity * x.Price)
    );

foreach (var item in totalPricesByCategory)
{
    Console.WriteLine($"Category: {item.Key}, Total Price: {item.Value}");
}
```

With `AggregateBy`, you group and aggregate in one step with minimal syntax. This method was added in .NET 9:

```csharp
var orders = new List<Order>
{
    new Order("Phone", "Electronics", 2, 299.99),
    new Order("TV", "Electronics", 1, 499.99),
    new Order("Bread", "Groceries", 5, 2.49),
    new Order("Milk", "Groceries", 2, 1.99)
};

var totalPricesByCategory = orders.AggregateBy(
    keySelector: x => x.Category,
    seed: 0.0,
    func: (total, order) => total + order.Quantity * order.Price
);

foreach (var item in totalPricesByCategory)
{
    Console.WriteLine($"Category: {item.Key}, Total Price: {item.Value}");
}
```

**How It Works:**

*   Similar to GroupBy, but you provide a seedFactory (initial value) and an aggregator function to accumulate values.
*   This single-step approach is easier compared to writing a GroupBy(...) and then calling .ToDictionary(...) with a .Sum(...).

## 8. Index

Sometimes you need the index of each element while iterating a sequence. You might have used an external index counter or enumerators.

```csharp
var orders = new List<Order>
{
    new Order("Phone", "Electronics", 2, 299.99),
    new Order("TV", "Electronics", 1, 499.99)
};

var index = 0;
foreach (var item in orders)
{
    Console.WriteLine($"Order #{index}: {item}");
    index++;
}
```

The new `Index` method yields each element paired with its index, simplifying the access pattern:

```csharp
var orders = new List<Order>
{
    new Order("Phone", "Electronics", 2, 299.99),
    new Order("TV", "Electronics", 1, 499.99)
};

foreach (var (index, item) in orders.Index())
{
    Console.WriteLine($"Order #{index}: {item}");
}
```

**How It Works:**

*   `Index` automatically pairs each element with its zero-based index.
*   Greatly simplifies code where you need an index for display, logging, or additional logic.

## Summary

New LINQ methods reduce the need for boilerplate loops, manual grouping, or custom indexing. Here is the list of new methods:

*   **Chunk** (.NET 6) allows splitting large collections into smaller sub-collections (chunks).
*   **Zip** (three sequences) (.NET 6) merges multiple sequences into one.
*   **MinBy** & **MaxBy** (.NET 6) let you quickly pick the objects with the smallest or largest property values.
*   **Range Support for Take** (.NET 8) eliminates mixing Skip and Take for slicing.
*   **CountBy** (.NET 9) merges grouping and counting into a single step.
*   **AggregateBy** (.NET 9) merges grouping and any custom aggregation with minimal steps.
*   **Index** (.NET 9) allows yielding an element together with its index.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/the-new-linq-methods-from-dotnet-6-to-dotnet-9)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fthe-new-linq-methods-from-dotnet-6-to-dotnet-9&title=The%20New%20LINQ%20Methods%20from%20.NET%206%20to%20.NET%209)[X](https://twitter.com/intent/tweet?text=The%20New%20LINQ%20Methods%20from%20.NET%206%20to%20.NET%209&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fthe-new-linq-methods-from-dotnet-6-to-dotnet-9)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fthe-new-linq-methods-from-dotnet-6-to-dotnet-9)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.