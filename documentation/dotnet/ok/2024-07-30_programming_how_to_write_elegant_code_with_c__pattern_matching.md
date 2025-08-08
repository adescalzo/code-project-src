```yaml
---
title: How To Write Elegant Code with C# Pattern Matching
source: https://antondevtips.com/blog/how-to-write-elegant-code-with-csharp-pattern-matching
date_published: 2024-07-30T11:00:24.944Z
date_captured: 2025-08-06T17:25:47.979Z
domain: antondevtips.com
author: Anton Martyniuk
category: programming
technologies: [.NET, C# 8.0, C# 11]
programming_languages: [C#]
tags: [csharp, pattern-matching, language-features, code-elegance, conditional-logic, switch-expressions, dotnet, programming]
key_concepts: [pattern-matching, type-patterns, constant-patterns, property-patterns, positional-patterns, list-patterns, switch-expressions, when-clauses]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to C# Pattern Matching, a powerful language feature introduced in C# 8.0 and continuously evolving. It explains various pattern types, including Type, Constant, Property, Positional, and List patterns, illustrating each with clear C# code examples. The post also demonstrates how to combine pattern matching with switch expressions and `when` clauses to handle complex conditional logic, such as range checks, enum comparisons, and multi-value matching. The author emphasizes how pattern matching helps developers write more concise, readable, and elegant code by reducing the complexity of conditional statements.
---
```

# How To Write Elegant Code with C# Pattern Matching

![newsletter](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fcsharp%2Fcover_csharp_pattern_matching.png&w=3840&q=100)
*Image: A dark blue background with abstract purple shapes. On the left, a white square icon with `</>` inside and the text "dev tips" below it. On the right, large white text reads "HOW TO WRITE ELEGANT CODE C# PATTERN MA", which is a partial title for the article.*

# How To Write Elegant Code with C# Pattern Matching

Jul 30, 2024

7 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Introduced in C# 8.0 **Pattern Matching**, provides an elegant way to write more expressive and concise code. Pattern Matching is evolving in every new C# version.

In this post, you will explore how to use **Pattern Matching** and how to write elegant code in C# by using Pattern Matching.

[](#what-is-pattern-matching-in-c)

## What Is Pattern Matching in C#

Pattern matching in C# is used to check a value against a pattern. It can be used in various scenarios, such as type checking, deconstructing tuples, and working with different data structures.

**Pattern matching in C# includes the following components:**

*   Type Patterns
*   Type Patterns with Nullable Types
*   Constant Patterns
*   Property Patterns
*   Positional Patterns
*   List Patterns

[](#type-patterns)

## Type Patterns

**Type patterns** are used to check if a value is of a specific type. They can be used with the `is` keyword. You can use this pattern to assign a matched type to a variable:

```csharp
public void Process(object obj)
{
    if (obj is string s)
    {
        Console.WriteLine($"String: {s}");
    }
    else if (obj is int i)
    {
        Console.WriteLine($"Integer: {i}");
    }
}
```

In this example, we check if `obj` is a `string` or an `integer` and handle each case accordingly.

[](#type-patterns-with-nullable-types)

### Type Patterns with Nullable Types

**Type patterns** can also be used with nullable types. You can use the `is` and `is not` keywords to check if a value is `null` or not:

```csharp
public void CheckForNull(object? obj)
{
    if (obj is null)
    {
        Console.WriteLine("Object is null.");
    }
    else if (obj is not null)
    {
        Console.WriteLine("Object is not null.");
    }
}
```

You can use the `is` keyword to check if a value is of a specific type, including nullable types:

```csharp
public void ProcessNullable(int? number)
{
    if (number is int value)
    {
        Console.WriteLine($"Number is {value}.");
    }
    else
    {
        Console.WriteLine("Number is null.");
    }
}
```

[](#constant-patterns)

## Constant Patterns

**Constant patterns** are used to compare a value with a constant. Together with [switch expressions](https://antondevtips.com/blog/how-to-write-elegant-code-with-csharp-switch-expressions) - pattern matching looks very elegant:

```csharp
public string GetDayType(int day)
{
    return day switch
    {
        0 => "Sunday",
        1 => "Monday",
        2 => "Tuesday",
        3 => "Wednesday",
        4 => "Thursday",
        5 => "Friday",
        6 => "Saturday",
        _ => "Invalid day"
    };
}
```

Here, we use a **switch expression** with **constant patterns** to return the name of the day based on the integer value.

[](#range-constant-patterns)

### Range Constant Patterns

You can use `and`, `or` pattern combinators to create **range** comparisons for numbers:

```csharp
public string EvaluateNumber(int number)
{
    return number switch
    {
        > 0 and <= 10 => "Number is between 1 and 10",
        > 10 and <= 20 => "Number is between 11 and 20",
        > 20 and <= 30 => "Number is between 21 and 30",
        _ => "Number is out of range"
    };
}

public string CheckSpecialNumber(int number)
{
    return number switch
    {
        5 or 23 or 42 => "Special number",
        _ => "Regular number"
    };
}
```

You can use `not` to exclude a range of numbers:

```csharp
public string ExcludeNumber(int number)
{
    return number switch
    {
        not (>= 10 and <= 20) => "Number is not between 10 and 20",
        _ => "Number is between 10 and 20"
    };
}
```

Such pattern matching is not limited to only numbers, you can also use it to check characters:

```csharp
public string EvaluateLetter(char letter)
{
    return letter switch
    {
        >= 'A' and <= 'Z' => "Uppercase letter",
        >= 'a' and <= 'z' => "Lowercase letter",
        _ => "Not a letter"
    };
}

public string CheckVowel(char letter)
{
    return letter switch
    {
        'A' or 'E' or 'I' or 'O' or 'U' or 'a' or 'e' or 'i' or 'o' or 'u' => "Vowel",
        _ => "Consonant"
    };
}
```

You can use `not` to exclude a range of characters:

```csharp
public string ExcludeSpecialCharacter(char character)
{
    return character switch
    {
        not (>= '0' and <= '9') and not (>= 'A' and <= 'Z') and not (>= 'a' and <= 'z') => "Special character",
        _ => "Alphanumeric character"
    };
}
```

[](#enum-constant-patterns)

### Enum Constant Patterns

You can use pattern matching on **enums**. Let's define a `OrderStatus` enum:

```csharp
public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Returned
}
```

You can use `and`, `or`, `not` for enums as well:

```csharp
public string GetOrderStatusMessage(OrderStatus status)
{
    return status switch
    {
        OrderStatus.Pending or OrderStatus.Processing => "Order is in progress.",
        OrderStatus.Shipped or OrderStatus.Delivered => "Order is on its way or has been delivered.",
        OrderStatus.Cancelled or OrderStatus.Returned => "Order has been cancelled or returned.",
        _ => "Unknown order status."
    };
}
```

You can negate enum patterns by using `not`. You can check if an order is finished in the following way:

```csharp
public bool IsOrderFinished(OrderStatus status)
{
    return status switch
    {
        not (OrderStatus.Delivered or OrderStatus.Cancelled or OrderStatus.Returned) => true,
        _ => false
    };
}
```

When using `not` with `or`, it's important to understand how the logic is applied. The `not` operator negates the entire expression that follows it.

If you need to check if an order is not `Shipped` and not `Delivered`, put parentheses for all expressions that should be negated with `not`:

```csharp
not (OrderStatus.Shipped or OrderStatus.Delivered)
```

If you miss the parentheses, the following interpretation will be incorrect:

```csharp
not OrderStatus.Shipped or OrderStatus.Delivered
```

Because it will check if an order **is not** `Shipped` or order **is** `Delivered`, instead of checking "is not" for both of the options.

[](#property-patterns)

## Property Patterns

**Property patterns** are used to match an object based on its properties.

```csharp
public record Person(string Name, int Age);

public void DisplayPersonInfo(Person person)
{
    if (person is { Name: "Anton", Age: 30 })
    {
        Console.WriteLine("Anton is 30 years old.");
    }
}
```

In this example, we check if the `Person` object has the name "Anton" and age 30.

Let's explore another code example that checks if a person is not null, has a dedicated Name and Age:

```csharp
public void DisplayPersonInfo(Person? person)
{
    if (person != null && person.Name == "Anton" && person.Age == 30)
    {
        Console.WriteLine("Anton is 30 years old.");
    }
}
```

With pattern matching, you can make this code more concise and readable:

```csharp
public void DisplayPersonInfo(Person? person)
{
    if (person is { Name: "Anton", Age: 30 })
    {
        Console.WriteLine("Anton is 30 years old.");
    }
}
```

In case you need to use the person object that matches the expression, you can assign a variable in pattern matching:

```csharp
public void DisplayPersonInfo(Person? person)
{
    if (person is { Name: "Anton", Age: 30 } p)
    {
        Console.WriteLine($"{p.Name} is {p.Age} years old.");
    }
}
```

Here, you can use `p` variable to access `Name` and `Age` properties of a Person.

You can also use negate expressions in pattern matching to simplify the code. Instead of using a few logical expressions to match a person who is not "Anton" or is null:

```csharp
public void DisplayPersonInfo(Person? person)
{
    if (person is null || (person.Name != "Anton" && person.Age != 30))
    {
        Console.WriteLine("User is not found.");
    }
}
```

You can use `is not` expression that will match all persons who are not "Anton", including null persons:

```csharp
public void DisplayPersonInfo(Person? person)
{
    if (person is not { Name: "Anton", Age: 30 })
    {
        // Either user is null or not Anton
        Console.WriteLine("User is not found.");
    }
}
```

You can also check for `null` and `not null` using empty property pattern matching:

```csharp
public void CheckForNull(object? obj)
{
    if (obj is {})
    {
        Console.WriteLine("Object is not null.");
    }
    else if (obj is not {})
    {
        Console.WriteLine("Object is null.");
    }
}
```

[](#positional-patterns)

## Positional Patterns

**Positional patterns** are used to deconstruct a value into its parts and match them.

For example, we can deconstruct a Point object into its `x` and `y` coordinates and handle different cases:

```csharp
public readonly struct Point
{
    public int X { get; }
    public int Y { get; }

    public Point(int x, int y) => (X, Y) = (x, y);

    public void Deconstruct(out int x, out int y) => (x, y) = (X, Y);
}

public void DisplayPointInfo(Point point)
{
    if (point is (0, 0))
    {
        Console.WriteLine("Point is at the origin.");
    }
    else if (point is (int x, int y))
    {
        Console.WriteLine($"Point is at ({x}, {y}).");
    }
}
```

[](#list-patterns)

## List Patterns

**List patterns**, introduced in C# 11, enable pattern matching on sequences. For example, you can match a list of integers against different patterns:

```csharp
public void AnalyzeList(List<int> numbers)
{
    switch (numbers)
    {
        case []:
            Console.WriteLine("The list is empty.");
            break;
        case [1, 2, 3]:
            Console.WriteLine("The list contains 1, 2, 3.");
            break;
        case [var first, var second, ..]:
            Console.WriteLine($"The list starts with {first} and {second}.");
            break;
        default:
            Console.WriteLine("The list has a different pattern.");
            break;
    }
}
```

[](#pattern-matching-on-multiple-values)

## Pattern Matching On Multiple Values

You can use Pattern Matching for multiple values in switch expressions. For example, calculating count of days for a shipment to be delivered, based on destination and weight:

```csharp
public int CalculateDeliveryDays(string destination, decimal weight)
{
    return (destination, weight) switch
    {
        ("Local", <= 1.0m) => 1,
        ("Local", <= 5.0m) => 2,
        ("Local", > 5.0m) => 3,
        
        ("International", <= 1.0m) => 5,
        ("International", <= 5.0m) => 7,
        ("International", > 5.0m) => 10,

        _ => 10 // Default case for unspecified scenarios
    };
}
```

[](#pattern-matching-with-switch-expressions-and-when-clauses)

## Pattern Matching with Switch Expressions and When Clauses

Pattern Matching with switch expressions become even more powerful when combined with `when` clauses. The `when` keyword allows you to add additional conditions to each case, enabling more control over your logic.

Let's explore an example involving calculation of a discount based on category, price and a member vs. non-member client:

```csharp
public decimal CalculateDiscount(string category, decimal price, bool isMember)
{
    return (category, price) switch
    {
        // 5% discount on electronics under $100
        ("Electronics", <= 100.0m) when !isMember => price * 0.05m,

        // 10% discount on electronics under $500
        ("Electronics", <= 500.0m) when !isMember => price * 0.10m,

        // 15% discount on electronics over $500
        ("Electronics", > 500.0m) when !isMember => price * 0.15m,

        // 7% discount for members on electronics under $100
        ("Electronics", > 100.0m) when isMember => price * 0.07m,

        // 12% discount for members on electronics under $500
        ("Electronics", <= 500.0m) when isMember => price * 0.12m,

        // 18% discount for members on electronics over $500
        ("Electronics", > 500.0m) when isMember => price * 0.18m,

        _ => 0.0m // No discount for unspecified scenarios
    };
}
```

Here a member gets an additional discount when compared to a regular customer.

With a `when` clause you can even discard all the patterns and write the logical expression to make the pattern matching:

```csharp
public enum MemberType
{
    Bronze,
    Silver,
    Gold
}

public decimal CalculateDiscount(string category, decimal price, MemberType memberType)
{
    return (category, price) switch
    {
        // 3% discount for Bronze members on electronics
        _ when memberType is MemberType.Bronze && category == "Electronics" => price * 0.03m,

        // 5% discount for Silver members on electronics
        _ when memberType is MemberType.Silver && category == "Electronics" => price * 0.05m,

        // 8% discount for Gold members on electronics
        _ when memberType is MemberType.Gold && category == "Electronics" => price * 0.08m,

        _ => 0.0m
    };
}
```

One of my favourite examples of how elegant code you can write with pattern matching and switch expressions is to identify the current temperature:

```csharp
public string ClassifyTemperatureImproved(int temperature)
{
    return temperature switch
    {
        < 0 => "Freezing",
        >= 0 and < 10 => "Cold",
        >= 10 and < 20 => "Cool",
        >= 20 and < 30 => "Warm",
        >= 30 => "Hot"
    };
}
```

[](#summary)

## Summary

**Pattern Matching** together with [switch expressions](https://antondevtips.com/blog/how-to-write-elegant-code-with-csharp-switch-expressions) is a powerful addition to C#, allowing developers to write cleaner, more concise code. You can reduce the complexity of your conditional logic, making your code more readable and less error-prone. Whether you are checking number ranges, converting enum values, calculating discounts, or handling complex logic, pattern matching helps you write more elegant code.

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-write-elegant-code-with-csharp-pattern-matching&title=How%20To%20Write%20Elegant%20Code%20with%20C%23%20Pattern%20Matching)[X](https://twitter.com/intent/tweet?text=How%20To%20Write%20Elegant%20Code%20with%20C%23%20Pattern%20Matching&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-write-elegant-code-with-csharp-pattern-matching)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-write-elegant-code-with-csharp-pattern-matching)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.