```yaml
---
title: Pattern Matching in C#
source: https://okyrylchuk.dev/blog/pattern-matching-in-csharp/
date_published: 2024-02-09T20:23:46.000Z
date_captured: 2025-08-06T18:29:08.101Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: programming
technologies: [.NET, C#]
programming_languages: [C#]
tags: [pattern-matching, csharp, dotnet, language-features, control-flow, modern-csharp, code-expressiveness]
key_concepts: [type-pattern, declaration-pattern, constant-pattern, relational-pattern, var-pattern, property-pattern, list-pattern, exception-pattern]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive overview of pattern matching in C#, a powerful feature introduced in C# 7.0 and continuously enhanced in subsequent versions. It explains various pattern types, including Type, Declaration, Constant, Relational, var, Property, and List patterns, demonstrating their usage with practical code examples. The author illustrates how these patterns can be combined for more expressive and concise code, while also cautioning against overcomplication. Additionally, the article highlights the application of pattern matching in exception handling, showcasing its versatility.
---
```
 
# Pattern Matching in C#

*Image: A profile picture of Oleg Kyrylchuk, the author.*

Pattern matching was introduced in C# 7.0 alongside .NET Core 2.0. Do you remember .NET Core?

Almost every new C# version improves pattern matching, making it a powerful feature with extensive abilities.  

Pattern matching is a C# feature that allows you to write more expressive code. You can test the expression and take action when the expression matches. It enables you to match the shape and properties of values using patterns, making it easier to work with complex data structures.

Let’s unlock the power of pattern matching in C#. 

## Type Pattern

**Type** pattern checks the run-time type of an expression.

```csharp
object greeting = "Hello, World!";
if (greeting is string)
{
    Console.WriteLine(greeting.ToString());
}
```

You can use it (like all other patterns) with **switch** expressions.

```csharp
string TestShape(object type)
{
    return type switch
    {
        Circle => "I'm Circle",
        Rectangle => "I'm Rectangle",
        _ => "Wrong Shape",
    };
}

public interface IShape { }
public class Circle : IShape { }
public class Rectangle : IShape { }
```

In the preceding example, you can notice the **Discard** pattern **\_**. It matches any expression, including **null**.

The power of Pattern matching is that you can combine many patterns. Let’s see how to use **Type** patterns with **Parenthesized** and **Logical** patterns.

```csharp
IShape shape = new Circle();
if (shape is (Circle or Rectangle))
{
    // Add code here
}
```

## Declaration Pattern

The **Declaration** pattern is very similar to the **Type** pattern. 

It also checks the expression in runtime, but additionally, it declares a variable and assigns the converted expression result to it. 

```csharp
object greeting = "Hello, World!";
if (greeting is string message)
{
    Console.WriteLine(message);
}
```

## Constant Pattern

Another straightforward pattern is the **Constant** pattern.

It tests expressions against a constant value. The value could be an integer, floating-point literal, char, string literal, bool, enum value, const field or local, or null.

```csharp
string Choice(int choice) =>
        choice switch
        {
            1 => "You chose 1",
            2 => "You chose 2",
            _ => "You made an unknown choice",
        };
```

But the most common usage is against **null**.

```csharp
if (greeting is null)
{
    // Add code here
}
```

When you test your expression against **null**, the compiler guarantees that the overloaded equality operator **\==** is invoked.

So it’s slightly safer to use **is null** than **\==** and more readable. I encourage you to use it if you still don’t.

To get the same result as !=, you need to add a **Negation** pattern.

```csharp
if (greeting is not null)
{
    // Add code here
}
```

Such code is more expressive, isn’t it?

## Relational Pattern

The **Relational** pattern is similar to the **Constant** pattern, but you can use relational operators **<, >, <=, >=** against constant.

```csharp
string GetCalendarSeason(DateOnly date)
    => date.Month switch
    {
        >= 3 and < 6 => "Spring",
        >= 6 and < 9 => "Summer",
        >= 9 and < 12 => "Autumn",
        12 or (>= 1 and < 3) => "Winter",
        _ => "Really?",
    };
```

In the preceding example, you can see two other **Logical** patterns, the **Conjunctive** **and** pattern, and the **Disjunctive** **or** pattern

How many patterns did you count so far?

## var Pattern

The **var** pattern matches any expression and assigns its result do a declared variable. At first glance, it’s the same as the **Declaration** pattern.

However, you can use **var** pattern when you need a temporary variable within an expression to hold the intermediate calculations.

```csharp
string CategorizePerson(Person person) => person switch
{
    var (age, height) when age < 18 && height < 160 => "Young and short",
    var (age, height) when age < 18 && height >= 160 => "Young and tall",
    var (age, height) when age >= 18 && height < 160 => "Adult and short",
    var (age, height) when age >= 18 && height >= 160 => "Adult and tall",
    _ => "Invalid person",
};

public record Person(int Age, int Height);
```

In this example, there are two more patterns: Logical and Positional. Let’s focus on the **Positional** pattern since we have already seen **Logical** patterns.  

The **Positional** pattern allows the deconstructing of an expression and matching the resulting values against the corresponding nesting patterns. You can see the deconstruction of the Person record to do an additional check with the when clause using a nested **Relational** Pattern.

Besides records, you can deconstruct tuples.

## Property Pattern

The **Property** pattern allows matching an expression’s properties or fields against patterns. Of course, you can combine it with other patterns.

Let’s simplify our example with **var** Pattern by replacing it with **Property** pattern. 

```csharp
string CategorizePerson1(Person person) => person switch
{
    { Age: < 18 } and { Height: < 160 } => "Young and short",
    { Age: < 18 } and { Height: >= 160 } => "Young and tall",
    { Age: >= 18 } and { Height: < 160 } => "Adult and short",
    { Age: >= 18 } and { Height: >= 160 } => "Adult and tall",
    _ => "Invalid person",
};
```

It’s shorter and cleaner.

You can also add a run-time type check to this pattern.

```csharp
string CategorizePerson1(Person person) => person switch
{
    Person { Age: < 18 } and { Height: < 160 } => "Young and short",
    Person { Age: < 18 } and { Height: >= 160 } => "Young and tall",
    Person { Age: >= 18 } and { Height: < 160 } => "Adult and short",
    Person { Age: >= 18 } and { Height: >= 160 } => "Adult and tall",
    _ => "Invalid person",
};
```

We often write the **if** statements that check if the reference variable is not **null** and add additional conditions to check. Now, we don’t do that. The **Property** pattern checks the **nulls** for us.

It’s convenient when we have nested properties.

```csharp
var person = new Person(18, 170, new Address("New York", "5th Avenue"));

if (person != null
    && person.Address != null
    && person.Age >= 18
    && person.Address.City == "New York")
    Console.WriteLine("Adult New Yorker");

// VS

if (person is { Age: >= 18, Address.City: "New York" })
    Console.WriteLine("Adult New Yorker");

public record Person(int Age, int Height, Address Address);
public record Address(string City, string Street);
```

## List Pattern

The **List** pattern is the newest one so far (when I wrote it, C# 12 was the last version).

It allows matching arrays or lists against a sequence of patterns.

It’s super powerful but also the most confusing, in my opinion. We can easily overcomplicate readability with it. 

```csharp
var numbers = new int[] { 1, 2, 3, 4, 5 };

Console.WriteLine(numbers is [1, 2, 3, ..]);     // True
Console.WriteLine(numbers is [> 0, _, < 4, ..]); // True
Console.WriteLine(numbers is [.., 5]);           // True
Console.WriteLine(numbers is [.., 4, 5 or 6]);   // True
Console.WriteLine(numbers is [.., > 0 and < 6]); // True
Console.WriteLine(numbers is [not 2, ..]);       // True
```

You can see a bunch of patterns combined with a **List** pattern. There is a **Relational**, **Discard**, and **Slice ..** pattern.

The **Slice** pattern matches zero or more elements. You can use it only once and only in the **List** pattern.

## Summary

Have you counted all the patterns?

There are many of them. However, learning them is worth it because they can improve your code, making it more expressive.

Pattern matching also provides many ways to combine them. 

Very quickly, we can make unreadable code. 

```csharp
string CategorizePerson(IList<Person> person)
{
    return person switch
    {
    [var (age, height, address), _, Person, ..]
        when age >= 18 && height < 160 && address.City is "New York"
            => "The first person is adult and short New Yorker",

            [_, { Age: >= 18, Height: >= 160, Address.City: "New York" }, ..]
                            => "The second person is adult and tall New Yorker",

        _ => "Invalid person"
    };
}
```

What do you think about such code?

It’s hard to read. However, it works perfectly.

I used almost all the patterns discussed earlier. You can try to count them.

C#, like other programming languages, allows us to write messy code if we overuse its features. Pattern Matching is no exception. Use the feature wisely.

Speaking about exceptions…

## Bonus: Exceptions

You can use Pattern Matching to catch exceptions.

```csharp
try
{
    // Some code that may throw exceptions
}
catch (Exception ex) when (ex is ArgumentException)
{
    Console.WriteLine("Caught an argument exception.");
}
catch (Exception ex)
    when (ex is DbException and { ErrorCode: 2627 })
{
    Console.WriteLine(
        "Caught a database exception for a duplicate key.");
}
catch (Exception ex)
{
    Console.WriteLine(
        $"Caught an unexpected exception: {ex.GetType().Name}");
}
```