```yaml
---
title: Improve Readability of Your Code with C# Collection Expressions
source: https://antondevtips.com/blog/improve-readability-of-your-code-with-csharp-collection-expressions
date_published: 2024-08-02T11:00:29.031Z
date_captured: 2025-08-06T17:24:56.589Z
domain: antondevtips.com
author: Anton Martyniuk
category: backend
technologies: [C# 12, .NET]
programming_languages: [C#]
tags: [csharp, csharp-12, collections, code-readability, language-features, programming, dotnet, spread-element, initializers]
key_concepts: [collection-expressions, collection-initializers, spread-element, code-conciseness, language-syntax, memory-optimization]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article introduces C# 12's collection expressions, a new language feature aimed at improving code readability and conciseness for collection manipulation. It covers collection initializers for various types like lists and arrays, including efficient ways to initialize empty collections. The post also details the "spread element" (..) for merging elements from multiple collections into one. Through practical C# code examples, the author demonstrates how these expressions simplify common collection operations. The feature is presented as a significant improvement over traditional methods, encouraging its adoption for cleaner code.]
---
```

# Improve Readability of Your Code with C# Collection Expressions

![A dark blue banner with abstract purple/blue shapes. On the left, a white square icon with `</>` inside and the text "dev tips". On the right, large white text reads "IMPROVE READABILITY OF YOUR CODE W C# COLLECTION EXPRESSIONS".](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fcsharp%2Fcover_csharp_collection_expressions.png&w=3840&q=100)

# Improve Readability of Your Code with C# Collection Expressions

Aug 2, 2024

3 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

C# 12 introduced one interesting feature called **collection expressions**. Collection expressions allow writing more concise and readable code when working with collections.

In today's blog post, you will learn about collection expression initializers, various expression usages and the spread element.

## Collection Initializers

Collection expressions provide a simple and consistent syntax across many different collection types. When initializing a collection with a collection expression, the compiler generates code that is functionally equivalent to using a collection initializer.

Let's explore the usual ways to initialize a list:

```csharp
// Using var
var list1 = new List<int> { 1, 2, 3, 4, 5 };

// Or using new()
List<int> list2 = new() { 1, 2, 3, 4, 5 };
```

Collection expressions provide a new syntax to initialize collections:

```csharp
List<int> list = [ 1, 2, 3, 4, 5 ];
```

You can also replace the usual array initialization:

```csharp
var array1 = new int[5] { 1, 2, 3, 4, 5 };

var array2 = new int[] { 1, 2, 3, 4, 5 };

var array3 = new[] { 1, 2, 3, 4, 5 };
```

With a new syntax:

```csharp
int[] array = [ 1, 2, 3, 4, 5 ];
```

When using a new syntax, you need to specify a concrete type of collection, as it can't be inferred directly from the collection initialization.

You can use this new collection expression initialization for collections of any type. Let's explore a few examples:

```csharp
char[] letters = [ 'a', 'b', 'c', 'd' ];

List<string> names = [ "Anton", "Bill", "John" ];

List<Person> persons = [
	new("Anton", 30),
	new("Bill", 25),
	new("John", 20)
];
```

Personally, I like this new syntax, it makes code more concise and more readable for me.

## Initializing Empty Collections

You can use a new syntax to initialize empty collections in a new more concise and readable way.

Here is the most common approach to initialize an empty array and list:

```csharp
var emptyArray = new int[0] {};

var emptyList = new List<int>();
```

When using this approach, you allocate memory to create an empty collection. So instead, you can use the following initializers that don't allocate any memory at all:

```csharp
var emptyArray = Array.Empty<int>();

var emptyList = Enumerable.Empty<int>();
```

But you can replace all these with a new empty collection expression initializer:

```csharp
int[] emptyArray = [];

List<int> emptyList = [];
```

This approach is more concise and readable and moreover, the compiler translates it to using `Array.Empty<T>` and `Enumerable.Empty<T>` under the hood, so you don't need to worry about memory.

## Using a Spread Element

The spread element (**..** two dots) allows you to include the elements of one collection into another collection concisely. This feature is particularly useful when you want to combine or merge collections.

Let's say you have two arrays of numbers, and you want to combine them into a single array:

```csharp
int[] oneTwoThree = [1, 2, 3];
int[] fourFiveSix = [4, 5, 6];

int[] allNumbers = [..oneTwoThree, 50, 60, ..fourFiveSix];

Console.WriteLine(string.Join(", ", allNumbers));
Console.WriteLine($"Length: {allNumbers.Length}");
```

This will output the following result:

```
1, 2, 3, 50, 60, 4, 5, 6
Length: 8
```

It doesn't end up with numbers, you can combine any objects you want, for example, strings:

```csharp
string[] greetings = ["Hello", "Hi"];
string[] farewells = ["Goodbye", "See you"];

string[] allMessages = [..greetings, "How are you?", ..farewells];

Console.WriteLine(string.Join(", ", allMessages));
Console.WriteLine($"Length: {allMessages.Length}");
```

That outputs:

```
Hello, Hi, How are you?, Goodbye, See you
Length: 5
```

And objects:

```csharp
Person[] groupA =
[
	new Person("John", 20),
	new Person("Jane", 22)
];

Person[] groupB =
[
	new Person("Alice", 25),
	new Person("Bob", 27)
];

Person[] allPeople = [..groupA, new Person("Charlie", 30), ..groupB];

foreach (var person in allPeople)
{
    Console.WriteLine(person);
}
```

That outputs:

```
Person { Name = John, Age = 20 }
Person { Name = Jane, Age = 22 }
Person { Name = Charlie, Age = 30 }
Person { Name = Alice, Age = 25 }
Person { Name = Bob, 27 }
```

This approach is really nice to read and it is easier to write such code than manually creating a list that combines all the values.

This reminds me of a spread operator in JavaScript and TypeScript, but in C# it's not that powerful yet. And in C# it's not an operator but an element. Hope it will be further improved in the future and will allow us to combine objects with a spread syntax.

## Summary

**Collection expressions** are a great feature introduced in C# 12. You can use collection initializers and spread element to write more concise and readable code when working with collections.

In your team, you have to decide whether to adapt this feature or not. But for me, this feature is a complete win comparing to the old ways of working with collections.

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fimprove-readability-of-your-code-with-csharp-collection-expressions&title=Improve%20Readability%20of%20Your%20Code%20with%20C%23%20Collection%20Expressions)[X](https://twitter.com/intent/tweet?text=Improve%20Readability%20of%20Your%20Code%20with%20C%23%20Collection%20Expressions&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fimprove-readability-of-your-code-with-csharp-collection-expressions)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fimprove-readability-of-your-code-with-csharp-collection-expressions)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.