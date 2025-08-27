```yaml
---
title: Unlocking the Delegate’s Potential in C#
source: https://okyrylchuk.dev/blog/unlocking-the-delegates-potential-in-csharp/
date_published: 2024-05-03T20:23:50.000Z
date_captured: 2025-08-20T18:59:39.427Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: programming
technologies: [.NET, LINQ, Console, List]
programming_languages: [C#, C++]
tags: [csharp, delegates, lambda-expressions, events, dotnet, programming, functional-programming, callbacks, type-safe, built-in-delegates]
key_concepts: [delegates, function-pointers, lambda-expressions, event-handling, action-delegate, func-delegate, predicate-delegate, method-chaining, covariance-contravariance]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to delegates in C#, defining them as type-safe function pointers that enable object-oriented method encapsulation. It demonstrates how to create and use custom delegates, chain multiple methods to a single delegate instance, and leverage lambda expressions for concise delegate creation. The post also covers the widely used built-in delegates like Action, Func, and Predicate, illustrating their practical applications, including their role in LINQ. Finally, it explains how delegates are fundamental to implementing event handling in C#, providing a practical example of a custom event.
---
```

# Unlocking the Delegate’s Potential in C#

# Unlocking the Delegate’s Potential in C#

![Author's avatar: A profile picture of Oleg Kyrylchuk.](https://okyrylchuk.dev/wp-content/litespeed/avatar/a85a89199a7d4486a29f67dc2fc371d0.jpg?ver=1755571532)By [Oleg Kyrylchuk](https://okyrylchuk.dev/blog/author/kyrylchuk-oleggmail-com/ "View all posts by Oleg Kyrylchuk") / May 3, 2024

Table Of Contents

1.  [What is a delegate?](#what-is-a-delegate)
2.  [Creating and using a delegate](#creating-and-using-a-delegate)
3.  [Lambda expressions](#lambda-expressions)
4.  [Built-in delegates](#built-in-delegates)
5.  [Events](#events)
6.  [Summary](#summary)

## What is a delegate?

A delegate is a type-safe function pointer that can reference one or more methods with a compatible signature.

Unlike pointers to functions in other languages, like C++, the delegates in C# are full-object oriented.

Having the delegate, you can pass a method as a parameter to other methods, return from methods, or even store delegates in the collections. 

You can call the methods via the delegate instance.

## Creating and using a delegate

To create a delegate, you need to use a delegate keyword.

In the example below, we create a delegate with a **NumberDelegate** name, accepting methods that return nothing and get one integer parameter.

```csharp
public delegate void NumberDelegate(int i);
```

To create the delegate instance, you must provide the name of the method the delegate will wrap.

```csharp
void PrintNumber(int i) => Console.WriteLine($"A number is {i}");

// Create an instance of delegate
// assigning the PrintNumber method
NumberDelegate numberDelegate = PrintNumber;

// Calling the delegate
numberDelegate(2);
// Output: A number is 2
```

The delegate can point to many methods so that you can chain them to one delegate instance.

## Lambda expressions

Lambda expressions are the second option for creating a delegate or adding another method to an existing delegate instance.  

```csharp
numberDelegate += (int i) => Console.WriteLine($"A multiplied number {i} is {i * i}");

numberDelegate(2);
// Output: A number is 2
// A multiplied number 2 is 4
```

As you can see, calling one delegate instance invokes two methods one by one.

Lambda expressions are compiled to delegates.

```csharp
var action = (string message) => Console.WriteLine(message);
```

![Placeholder image for a diagram or code illustration.](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSI3ODQiIGhlaWdodD0iMjE0IiB2aWV3Qm94PSIwIDAgNzg0IDIxNCI+PHJlY3Qgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgc3R5bGU9ImZpbGw6I2NmZDRkYjtmaWxsLW9wYWNpdHk6IDAuMTsiLz48L3N2Zz4=)

## Built-in delegates

The most commonly used built-in delegates in .NET:

*   **Action**
*   **Func**
*   **Predicate**

You can notice that in the previous image, the action variable is an **Action** delegate.

It encapsulates the method that does not return value and has no parameters.

There is a generic equivalent **Action<T>** for methods taking parameters. The maximum count of parameters is 16.

My **NumberDelegate** is not needed. It can be substituted by Action<int> delegate.

```csharp
Action<int> numberDelegate = PrintNumber;
```

The **Func** delegate encapsulates the methods that return value and can have parameters.

If the methods have parameters, you define all parameters and the return value as the last. 

```csharp
Func<int, int> multipleNumbers = (int number) => number * number;
```

In this example, the delegate attaches the method, taking one integer parameter and returning an integer value.

The **Func** delegate can also accept a method with a maximum of 16 parameters.  
  
The **Predicate** delegate encapsulates the methods that define a set of criteria and determine whether the specified object meets those criteria.

Basically, the **Predicate** delegate is the same as **Func<in T, bool>** delegate, but with an explicit name.

```csharp
Predicate<int> predicate = x => x % 2 == 0;

Func<int, bool> funcAsPredicate = x => x % 2 == 0;
```

At this point, if you’re a fan of LINQ using method syntax, you can realize that you always use delegates. If you didn’t know that ![Smiling emoji.](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIzMiIgaGVpZ2h0PSIzMiIgdmlld0JveD0iMCAwIDMyIDMyIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBzdHlsZT0iZmlsbDojY2ZkNGRiO2ZpbGwtb3BhY2l0eTogMC4xOyIvPjwvc3ZnPg==)

```csharp
List<int> list = [1, 2, 3, 4];
list.Exists(predicate);
list.Where(funcAsPredicate);
```

## Events

The event is a class member that can notify other objects when something happens.

If you develop a desktop application, this concept should be familiar.

The events require delegates. 

To show that, let’s create a naive implementation of the **MyProgress** class.

```csharp
public delegate void NumberChanged(int i);

public class MyProgress(int end)
{
    private int _current = 0;
    private int _end = end;

    public event NumberChanged ProgressChanged;

    public void AddProgress(int currentProgress)
    {
        if (_current < _end)
        {
            _current += currentProgress;
            ProgressChanged(_current);
        }
    }
}
```

The class takes the end for progress and starts from 0. You need a delegate to create an event, which will occur every time we add progress.

The usage is simple.  

```csharp
MyProgress myProgress = new MyProgress(100);
myProgress.ProgressChanged += (int current) => Console.WriteLine($"Current progress is {current}");

myProgress.AddProgress(10);
myProgress.AddProgress(20);
myProgress.AddProgress(30);

// Current progress is 10
// Current progress is 30
// Current progress is 60
```

The event is fired on every valid AddProgress method call, and our delegate printing the current progress is invoked.

## Summary

The delegates are a powerful feature in C#.  
  
The delegate is a type-safe pointer to the method, making method encapsulation object-oriented.  
  
The delegates allow passing methods as parameters. They can attach many methods.  
  
You can create call-back methods with delegates.    
  
The lambda expression (in some contexts) is compiled to delegate.  
  
With delegates, you can create events in C#.  
  
The methods don’t have to match the delegate type exactly. You can read more about [Using Variance in Delegates](https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/concepts/covariance-contravariance/using-variance-in-delegates "Using Variance in Delegates"). 

Post navigation

[← Previous Post](https://okyrylchuk.dev/blog/how-to-investigate-performance-counters-in-dotnet/ "How to Investigate Performance Counters in .NET")

[Next Post →](https://okyrylchuk.dev/blog/efficient-bulk-updates-in-entity-framework/ "Efficient Bulk Updates in Entity Framework")

## Related Posts

![Placeholder image for a related article thumbnail titled 'Records in C#'.](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMjgwIiBoZWlnaHQ9IjcyMCIgdmlld0JveD0iMCAwIDEyODAgNzIwIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBzdHlsZT0iZmlsbDojY2ZkNGRiO2ZpbGwtb3BhY2l0eTogMC4xOyIvPjwvc3ZnPg==)](https://okyrylchuk.dev/blog/records-in-csharp/)

### [Records in C#](https://okyrylchuk.dev/blog/records-in-csharp/)

[.NET](https://okyrylchuk.dev/blog/category/dotnet/), [C#](https://okyrylchuk.dev/blog/category/csharp/) / February 2, 2024

![Placeholder image for a related article thumbnail titled 'Pattern Matching in C#'.](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMjgwIiBoZWlnaHQ9IjcyMCIgdmlld0JveD0iMCAwIDEyODAgNzIwIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBzdHlsZT0iZmlsbDojY2ZkNGRiO2ZpbGwtb3BhY2l0eTogMC4xOyIvPjwvc3ZnPg==)](https://okyrylchuk.dev/blog/pattern-matching-in-csharp/)

### [Pattern Matching in C#](https://okyrylchuk.dev/blog/pattern-matching-in-csharp/)

[.NET](https://okyrylchuk.dev/blog/category/dotnet/), [C#](https://okyrylchuk.dev/blog/category/csharp/) / February 9, 2024