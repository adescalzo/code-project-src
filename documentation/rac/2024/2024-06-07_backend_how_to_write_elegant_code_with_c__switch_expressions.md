```yaml
---
title: How To Write Elegant Code with C# Switch Expressions
source: https://antondevtips.com/blog/how-to-write-elegant-code-with-csharp-switch-expressions
date_published: 2024-06-07T11:00:55.203Z
date_captured: 2025-08-06T17:20:58.321Z
domain: antondevtips.com
author: Anton Martyniuk
category: backend
technologies: [C# 8.0, C# 9.0, .NET]
programming_languages: [C#]
tags: [csharp, switch-expressions, code-elegance, readability, conditional-logic, pattern-matching, dotnet, programming-tips]
key_concepts: [switch expressions, conditional logic, pattern matching, code readability, code conciseness, when clauses, if statements, switch statements]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces C# switch expressions, highlighting their benefits in simplifying conditional logic and improving code readability. It demonstrates how to refactor traditional `if` and `switch` statements into more concise switch expressions. The post also clarifies when switch expressions are appropriate, noting their requirement to return a value, and explores advanced features like `when` clauses and pattern matching for more complex scenarios. Through practical examples, it illustrates how these features contribute to writing cleaner and more maintainable C# code.]
---
```

# How To Write Elegant Code with C# Switch Expressions

![Cover image for "How To Write Elegant Code with C# Switch Expressions" showing a code icon and abstract purple shapes on a dark background.](https://antondevtips.com/media/covers/csharp/cover_csharp_switch_expressions.png)

# How To Write Elegant Code with C# Switch Expressions

Jun 7, 2024

4 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Introduced in C# 8.0 switch expressions, provide an elegant way to handle multiple conditions and return values, simplifying your code and improving readability.

In this post, you will explore how to use switch expressions, what are their benefits and how to write elegant code in C# by using switch expressions. You'll see how switch expressions can simplify your code and improve readability.

## Converting If Statements and Regular Switch to Switch Expressions

First, let's explore the old ways of conditional operators like `if` and `switch`. Let's explore an example of getting a string representation for a `Color` enum:

```csharp
public enum Color
{
    Red,
    Green,
    Blue,
    Yellow
}

public string GetColorNameIf(Color color)
{
    if (color == Color.Red)
    {
        return "Red";
    }
    if (color == Color.Green)
    {
        return "Green";
    }
    if (color == Color.Blue)
    {
        return "Blue";
    }
    if (color == Color.Yellow)
    {
        return "Yellow";
    }

    return "Unknown";
}
```

Instead of using a lot of `if` statement you can rewrite this code by using `switch` operator:

```csharp
public string GetColorNameSwitch(Color color)
{
    switch (color)
    {
        case Color.Red:
            return "Red";
        case Color.Green:
            return "Green";
        case Color.Blue:
            return "Blue";
        case Color.Yellow:
            return "Yellow";
        default:
            return "Unknown";
    }
}
```

While the second option is more readable than the first one, you can make it more elegant by using switch expressions:

```csharp
public string GetColorName(Color color)
{
    return color switch
    {
        Color.Red => "Red",
        Color.Green => "Green",
        Color.Blue => "Blue",
        Color.Yellow => "Yellow",
        _ => "Unknown"
    };
}
```

Definitely, this code is much cleaner than the previous two examples. It is more concise, more readable and maintainable.

## Is Switch Statement Deprecated?

So, switch expressions make a switch statement useless, right? Not really.

While switch expressions are so elegant, they have one limitation: they must return a value. You can't rewrite the following code to switch expressions:

```csharp
public void LogMessage(string text, LoggerType type)
{
    switch (type)
    {
        case LoggerType.Console:
            Console.WriteLine(text);
            break;
        case LoggerType.File:
            LogToFile(text);
            break;
    }
}
```

We are not returning here anything, we are calling the method. So you can't use switch expressions here. Though you can rewrite this code in a different manner using switch expressions:

```csharp
public void LogMessageImproved(string text, LoggerType type)
{
    ILogger logger = type switch
    {
        LoggerType.Console => new ConsoleLogger(),
        LoggerType.File => new FileLogger(),
        _ => throw new UnreachableException()
    };

    logger.LogMessage(text);
}
```

This code is greatly simplified, in real applications you should be using Dependency Injection and a Logger Factory to resolve loggers by type.

## Switch Expression Examples

Let's explore more examples with switch expressions, like calculating discounts based on the subscription type:

```csharp
public enum SubscriptionType
{
    Free,
    Developer,
    Pro,
    Enterprise
}

public decimal GetDiscount(SubscriptionType type)
{
    return type switch
    {
        SubscriptionType.Free => 0.0m,
        SubscriptionType.Developer => 0.05m,
        SubscriptionType.Pro => 0.1m,
        SubscriptionType.Enterprise => 0.2m,
        _ => throw new UnreachableException()
    };
}
```

Switch expressions can also handle more complex logic. For instance, calculating shipping costs based on weight and destination:

```csharp
public decimal CalculateShippingCost(decimal weight, string destination)
{
    return (weight, destination) switch
    {
        (<= 1.0m, "Local") => 5.0m,
        (<= 1.0m, "International") => 15.0m,
        (<= 5.0m, "Local") => 10.0m,
        (<= 5.0m, "International") => 25.0m,
        _ => 50.0m
    };
}
```

## Switch Expressions with When Clauses

Switch expressions become even more powerful when combined with `when` clauses. The `when` keyword allows you to add additional conditions to each case, enabling more control over your logic.

Let's explore a more complex example involving shipping costs based on weight and destination with additional conditions:

```csharp
public decimal CalculateShippingCost(decimal weight, string destination)
{
    return (weight, destination) switch
    {
        (<= 1.0m, "Local") => 5.0m,
        (<= 1.0m, "International") => 15.0m,
        (<= 5.0m, "Local") when weight > 1.0m => 10.0m,
        (<= 5.0m, "International") when weight > 1.0m => 25.0m,
        _ => 50.0m
    };
}
```

In this example, additional `when` clauses give more control on the conditions under which different shipping costs are applied.

Now let's explore another example to classify temperatures based on their value:

```csharp
public string ClassifyTemperature(int temperature)
{
    return temperature switch
    {
        _ when temperature < 0 => "Freezing",
        _ when temperature >= 0 && temperature < 10 => "Cold",
        _ when temperature >= 10 && temperature < 20 => "Cool",
        _ when temperature >= 20 && temperature < 30 => "Warm",
        _ when temperature >= 30 => "Hot",
        _ => "Unknown"
    };
}
```

Here we are discarding the parameters in the switch expression and only use the `when` clauses. By using pattern matching, introduced in C# 9.0, we can simplify this code:

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

Let me explain this code:

*   If the temperature is below 0, function will return "Freezing".
*   For a temperature between 0 (inclusive) and 10 (exclusive), it returns "Cold".
*   If it's between 10 (inclusive) and 20 (exclusive), it returns "Cool".
*   For temperature between 20 (inclusive) and 30 (exclusive), it returns "Warm".
*   Any temperature that is 30 and above will result in "Hot".

This is a compact and efficient way to handle multiple conditions in C#. It makes the code cleaner and more readable comparing to the previous example.

At first look, this code might seem weird. But if you get used to it, you will prefer this concise version.

## Summary

Switch expressions are a powerful addition to C#, allowing developers to write cleaner, more concise code. By using switch expressions, you can reduce the complexity of your conditional logic, making your code more readable and less error-prone. Whether you are converting enum values, calculating discounts, or handling complex logic, switch expressions can help you write more elegant code.

Start experimenting with switch expressions today and experience the benefits for yourself!

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-write-elegant-code-with-csharp-switch-expressions&title=How%20To%20Write%20Elegant%20Code%20with%20C%23%20Switch%20Expressions)[X](https://twitter.com/intent/tweet?text=How%20To%20Write%20Elegant%20Code%20with%23%20Switch%20Expressions&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-write-elegant-code-with-csharp-switch-expressions)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-write-elegant-code-with-csharp-switch-expressions)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.