```yaml
---
title: New APIs in .NET 9 You Should Know About
source: https://okyrylchuk.dev/blog/new-apis-in-dotnet-9-you-should-know-about/?utm_source=emailoctopus&utm_medium=email&utm_campaign=APIs%20in%20.NET%209
date_published: 2024-11-29T17:09:59.000Z
date_captured: 2025-08-08T16:18:26.982Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: backend
technologies: [.NET 9, .NET 8, .NET 6, LINQ, Task, HttpClient, ReadOnlySet, OrderedDictionary, Guid, Base64Url, Regex, PriorityQueue, TimeSpan, System.Text.Json]
programming_languages: [C#]
tags: [.net, .net-9, csharp, new-features, linq, collections, concurrency, performance, api, utilities]
key_concepts: [linq-extensions, asynchronous-programming, immutable-collections, ordered-dictionaries, guid-versions, base64-encoding, regular-expressions, priority-queues]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides an overview of new APIs introduced in .NET 9, aiming to enhance code cleanliness and efficiency. It highlights significant additions to LINQ, including `CountBy`, `AggregateBy`, and `Index` methods, which simplify data manipulation. The post also covers `Task.WhenEach` for improved asynchronous task management and new collection types like `ReadOnlySet` and a generic `OrderedDictionary`. Furthermore, it introduces `Guid Version 7`, `Base64Url` helper for URL-safe encoding, `Regex.EnumerateSplits` for allocation-free string splitting, and the `Remove` method for `PriorityQueue`, along with new `TimeSpan.From` overloads for better precision.
---
```

# New APIs in .NET 9 You Should Know About

# New APIs in .NET 9 You Should Know About

![](https://okyrylchuk.dev/wp-content/litespeed/avatar/a85a89194a7d4486a29f67dc2fc371d0.jpg?ver=1754361075)By [Oleg Kyrylchuk](https://okyrylchuk.dev/blog/author/kyrylchuk-oleggmail-com/ "View all posts by Oleg Kyrylchuk") / November 29, 2024

The latest .NET release has new APIs to make your code cleaner and more efficient.

**LINQ** gets a boost with **CountBy**, **AggregateBy**, and **Index**, while **Task.WhenEach** simplifies parallelism. Collections see exciting additions like **ReadOnlySet** and a generic **OrderedDictionary**.

In this post, we’ll dive into these features, showing how they can solve common problems and elevate your .NET projects. Let’s explore!

Table Of Contents

1.  [CountBy](#countby)
2.  [AggregateBy](#aggregateby)
3.  [Index](#index)
4.  [Task.WhenEach](#taskwheneach)
5.  [ReadOnlySet](#readonlyset)
6.  [Generic OrderedDictionary](#generic-ordereddictionary)
7.  [Guid Version 7](#guid-version-7)
8.  [Base64Url Helper](#base64url-helper)
9.  [Regex.EnumerateSplits](#regexenumeratesplits)
10. [PriorityQueue.Remove](#priorityqueueremove)
11. [TimeSpan.From Overloads](#timespanfrom-overloads)

## **CountBy**

Let’s start with LINQ! .NET 9 introduces the new LINQ method **CountBy**.

The **CountBy** method allows for the calculation of the frequency of a key.

```csharp
Person[] persons = 
    [
        new ("Jan", "Kowalski", "Poland"),
        new ("John", "Doe", "US"),
        new ("Tom", "Riddle", "UK"),
        new ("Jane", "Doe", "US"),
    ];

var countByCountry = persons.CountBy(p => p.Country);

foreach (KeyValuePair<string, int> count in countByCountry)
    Console.WriteLine($"{count.Key} {count.Value}");
// Poland 1
// US 2
// UK 1

record Person(string Name, string Surname, string Country);
```

## **AggregateBy**

The following new LINQ method is the **AggregateBy**.

The **AggregateBy** method allows for grouping elements by a key and returns the accumulated value for each group.

```csharp
Product[] products =
[
    new ("Ball", "Sports", 10),
    new ("Laptop", "Electronics", 1500),
    new ("Bike", "Sports", 300),
    new ("Monitor", "Electronics", 400)
];

var aggregated = products
    .AggregateBy(p => p.Category,
                 seed: 0M,
                 (totalPrice, curr) => totalPrice + curr.Price);

foreach (var item in aggregated)
    Console.WriteLine($"{item.Key} total price is ${item.Value}");

// Sports total price is $310
// Electronics total price is $1900

public record Product(string Name, string Category, decimal Price);
```

## **Index**

If you ever needed the element index of collection in the **foreach** loop, you could use the **Select** method.

.NET 9 brings a better and cleaner way to obtain the element index. Meet the **Index** method!

```csharp
string message = "Hello";

foreach ((int index, char @char) in message.Index())
{
    Console.WriteLine($"Character {@char} has index {index}");
}

// Character H has index 0
// Character e has index 1
// Character l has index 2
// Character l has index 3
// Character o has index 4
```

## **Task.WhenEach**

.NET 9 introduces the **Task.WhenEach** method.

The **Task.WhenEach** method allows to join scheduled tasks and iterate through them as each one is completed.

```csharp
using HttpClient http = new()
{
    BaseAddress = new Uri("https://api.github.com")
};
http.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Dotnet", "9"));

Task<GitHubUser> user1 = http.GetFromJsonAsync<GitHubUser>("users/okyrylchuk");
Task<GitHubUser> user2 = http.GetFromJsonAsync<GitHubUser>("users/jaredpar");
Task<GitHubUser> user3 = http.GetFromJsonAsync<GitHubUser>("users/davidfowl");

await foreach (Task<GitHubUser> task in Task.WhenEach(user1, user2, user3))
{
    Console.WriteLine($"Name: {task.Result.Name}, Bio: {task.Result.Bio}");
}
//Name: David Fowler, Bio: Distinguished Engineer
//Name: Oleg Kyrylchuk, Bio: Microsoft MVP | Software developer
//Name: Jared Parsons, Bio: C# compiler lead

record GitHubUser(string Name, string Bio);
```

Previously, you had to repeatedly use **Task.WaitAny** on a set of tasks to pick off the next one that completes.

```csharp
List<Task<GitHubUser>> tasks = [ user1, user2, user3 ];
List<GitHubUser> users = new();

while (tasks.Any())
{
    var completedTask = await Task.WhenAny(tasks);
    tasks.Remove(completedTask);

    users.Add(await completedTask);
}
```

## **ReadOnlySet**

.NET 9 introduces the **ReadOnlySet**.

The built-in read-only wrapper around an arbitrary mutable **HashSet** was missing in previous .NET versions.

```csharp
HashSet<int> set = [ 1, 2, 3, 4, 5 ];

ReadOnlySet<int> readOnlySet = new(set);
```

## **Generic OrderedDictionary**

The **OrderedDictionary** type has existed in .NET since an early age.

.NET 9 introduces the **generic counterpart**.

The **OrderedDictionary** creates a dictionary where the order of key-value pairs can be maintained.

```csharp
OrderedDictionary<int, string> d = new()
{
    [1] = "apple",
    [2] = "banana",
    [3] = "cherry",
};

d.Add(4, "orange");
d.RemoveAt(1);  // Remove "banana"
d.RemoveAt(2);  // Remove "orange"
d.Insert(1, 5, "elderberry");  // Insert "elderberry" at index 1

foreach (KeyValuePair<int, string> entry in d)
    Console.WriteLine(entry);

// Output:
// [1, apple]
// [5, elderberry]
// [3, cherry]
```

## Guid Version 7

.NET 9 introduces a new GUID implementation based on [timestamp and random](https://en.wikipedia.org/wiki/Universally_unique_identifier#Version_7_\(timestamp_and_random\)).

You can create a Guid using the **CreateVersion7()** method.

More about GUID 7 you can read in [my previous post](/blog/guid-version7-in-dotnet-9/).

```csharp
var guid7 = Guid.CreateVersion7();

Console.WriteLine($"V{guid7.Version}: {guid7}");
// V7: 019378c3-ef98-773f-a043-762914c97d8c
```

## Base64Url Helper

.NET 9 introduces a new **Base64Url** type.

The existing **Convert.ToBase64String** method can produce a string with ‘/’, ‘+’, or ‘=’ characters. They are not safe for URLs because they have special meanings in URLs.

The **Base64Url** helper produces the string without these characters.

```csharp
byte[] toEncodeAsBytes = Encoding.UTF8.GetBytes("hello world"); 
var oldBase64 = Convert.ToBase64String(toEncodeAsBytes);

var newBase64 = Base64Url.EncodeToString(toEncodeAsBytes);

Console.WriteLine(oldBase64);
// aGVsbG8gd29ybGQ=
Console.WriteLine(newBase64);
// aGVsbG8gd29ybGQ
```

## **Regex.EnumerateSplits**

.NET 9 introduces the **Regex.EnumerateSplits** method.

It works like existing **Regex.Split** method, it splits the string by given **Regex**.

The difference is that the new method accepts **ReadOnlySpan<char>** and returns **Range** struct without incurring any allocation.

```csharp
ReadOnlySpan<char> input = "abcdefghij";
foreach (Range r in Regex.EnumerateSplits(input, "[aei]"))
{
    Console.Write($"{input[r]}");
}
// Output: bcdfghj
```

## **PriorityQueue.Remove**

The **PriorityQueue** type was introduced in .NET 6.

However, it missed the **Remove** method, which is helpful in various algorithms, such as **Dijkstra’s algorithm**.

.NET 9 adds the **Remove** method.

```csharp
PriorityQueue<string, int> pq = new();

pq.Enqueue("A", 2);
pq.Enqueue("B", 2);
pq.Enqueue("C", 1);

pq.Remove("B", out string rElement, out int rPriority);

Console.WriteLine(
    $"Removed element: {rElement}, priority: {rPriority}");
// Output: Removed element: B, priority: 2

while (pq.Count > 0)
{
    var element = pq.Dequeue();
    Console.WriteLine($"Element: {element}");
}
// Output:
// Element: C
// Element: A
```

## **TimeSpan.From Overloads**

.NET 9 adds new overloads for the **TimeSpan.From** methods.

Previously, they accepted **double** type, which is a binary-based floating-point format. It can cause bugs.

The new overloads accept **int** and **long** to achieve the desired result.

```csharp
// .NET 8 and older
var timeSpan1 = TimeSpan.FromSeconds(101.832);

Console.WriteLine(timeSpan1);
// 00:01:41.8319999

// .NET 9
var timeSpan2 = TimeSpan.FromSeconds(seconds: 101, milliseconds: 832);

Console.WriteLine(timeSpan2);
// 00:01:41.0008320
```

---

Post navigation

[← Previous Post](https://okyrylchuk.dev/blog/mastering-async-and-await-in-csharp-best-practices/ "Mastering Async and Await in C#: Best Practices")

[Next Post →](https://okyrylchuk.dev/blog/high-performance-file-access-in-dotnet-with-randomaccess/ "High-Performance File Access in .NET with RandomAccess")

## Related Posts

[![Guid Version 7 in .NET 9](https://okyrylchuk.dev/wp-content/uploads/2024/08/featured27-1.png.webp)](https://okyrylchuk.dev/blog/guid-version7-in-dotnet-9/)
*Image Description: A dark blue background with abstract light blue lines. Large yellow text in the center reads "Guid Version 7 in .NET 9". In the top right corner, small white text says ".NET Pulse #27". The website "okyrylchuk.dev" is in the bottom left corner.*

### [Guid Version 7 in .NET 9](https://okyrylchuk.dev/blog/guid-version7-in-dotnet-9/)

[.NET](https://okyrylchuk.dev/blog/category/dotnet/), [.NET 9](https://okyrylchuk.dev/blog/category/dotnet-9/), [C#](https://okyrylchuk.dev/blog/category/csharp/) / August 23, 2024

[![What’s new in System.Text.Json in .NET 9](https://okyrylchuk.dev/wp-content/uploads/2024/11/featured36.png.webp)](https://okyrylchuk.dev/blog/whats-new-in-system-text-json-in-dotnet-9/)
*Image Description: A cropped image showing a dark blue background with abstract light blue lines. The beginning of large white text is visible, reading "What's new in System.Text.Json in .NET 9".*

### [What’s new in System.Text.Json in .NET 9](https://okyrylchuk.dev/blog/whats-new-in-system-text-json-in-dotnet-9/)

[.NET 9](https://okyrylchuk.dev/blog/category/dotnet-9/), [System.Text.Json](https://okyrylchuk.dev/blog/category/system-text-json/) / November 1, 2024