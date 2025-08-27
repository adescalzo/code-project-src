```yaml
---
title: How to Deal With NullReferenceException? Object reference not set to an instance of an object. – PostSharp Blog
source: https://blog.postsharp.net/nullreferenceexception-object-reference-not-set
date_published: 2024-05-20T07:00:01.000Z
date_captured: 2025-08-27T13:35:27.745Z
domain: blog.postsharp.net
author: Unknown
category: general
technologies: [C#, .NET, Visual Studio, Rider, Metalama, Metalama.Patterns.Contracts, PolySharp, GitHub, NuGet]
programming_languages: [C#]
tags: [nullreferenceexception, csharp, dotnet, error-handling, defensive-programming, nullability, debugging, code-quality, metaprogramming, static-analysis]
key_concepts: [NullReferenceException, reference-and-nullable-types, debugging, defensive-programming, nullable-reference-types, contract-based-programming, metaprogramming]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explains the `NullReferenceException` in C#, detailing its cause when attempting to access members of a `null` reference. It covers how to identify the source of the exception using debuggers or stack traces and distinguishes between legitimate optional `null` values and unexpected ones. To prevent future occurrences, the author advocates for enabling nullable reference types, adopting a contract-based mindset, and implementing defensive programming with `ArgumentNullException` checks. The article also introduces Metalama as a tool to automate these precondition checks, thereby reducing boilerplate code and human errors.]
---
```

# How to Deal With NullReferenceException? Object reference not set to an instance of an object. – PostSharp Blog

# How to Deal With NullReferenceException? Object reference not set to an instance of an object.

by Metalama Team on 20 May 2024

*   [![Twitter social media icon](/assets/images/icons/twitter_p.svg)](https://twitter.com/postsharp "Follow us on X")
*   [![LinkedIn social media icon](/assets/images/icons/linkedin_p.svg)](https://www.linkedin.com/company/postsharp "Follow on LinkedIn")
*   [![RSS feed icon](/assets/images/icons/rss.svg)](/feed.xml "RSS feed")
*   [![Slack social media icon](/assets/images/icons/slack.svg)](https://www.postsharp.net/slack "Join us on Slack")

As a C# programmer, you’ve likely come across this exception: `System.NullReferenceException: Object reference not set to an instance of an object`. So, what does it mean? How can you deal with it? And better yet, how can you prevent it from happening?

## What is a NullReferenceException?

With reference types (classes, interfaces, delegates, or arrays, but not structs), variables (i.e. local variables, fields, or parameters) do not contain the object itself but a _reference_ to the object. This allows two different variables to _reference_ the same object. A reference can either point to an object or be `null`, which means it doesn’t point to any object.

`NullReferenceException`, as the name suggests, is thrown when you attempt to get the object referenced by a `null` reference. Specifically, it occurs when you try to access a non-static member (a property, method, field, or event) of a `null` reference.

Confusingly, C# also has reference parameters, variables, and return values, which are denoted by the `ref` keyword. This allows representing a reference to value types or… references to references! But since `ref` locations can’t be `null`, you can’t get a `NullReferenceException` from accessing them, and we won’t cover them further in this article.

Another related concept is nullable value types, which are value types that can be `null`. These are denoted as `T?` (or `Nullable<T>`), where `T` is a value type (a struct or enum). You can’t get a `NullReferenceException` by accessing a nullable value type because they’re not references. Instead, accessing `Value` on a `null` nullable value type throws an `InvalidOperationException`.

## How to identify the cause of a NullReferenceException?

Before you can fix a `NullReferenceException`, you first need to understand where it happened.

The simplest approach is to use the debugger of your IDE (like Visual Studio or Rider) to run your code. Consider this simple example:

```csharp
class Shop(Cart cart)
{
    public void AddToCartWithDiscount(Product product, int discount)
    {
        product.Discount = discount;
        cart.Add(product);
    }
}
```

When running code that calls the `AddToCartWithDiscount` method in the Visual Studio debugger, you might see something like this:

![Visual Studio in debugger mode with NullReferenceException shown](/assets/images/2024/2024-04-16-nullreferenceexception/vs-debugging.png)

Here, you can see where the exception occurred, the values of local variables, the call stack, and even which variable caused the exception by being `null`. If the `null` reference was passed as a parameter, as in this case, you can double-click a method in the call stack to see its state, getting closer to the root cause of the exception.

But what if you can’t use a debugger? For instance, if you’re working on a production issue and can’t reproduce it locally. In that scenario, you should have access to the exception and its stack trace, which might look like this:

```
System.NullReferenceException: Object reference not set to an instance of an object.
   at Shop.AddToCartWithDiscount(Product product, Int32 discount) in Shop.cs:line 5
   at Program.Main() in Program.cs:line 5
```

There’s less information here, but you can still see where the exception occurred and what the call stack is, which should suffice to figure out which value is `null`. However, keep in mind that the line numbers might not be accurate, especially if you’re using a release build.

You could also add logging to your code to get more information about the state of your application when the exception happens.

If debugging information wasn’t available when the exception occurred (it’s usually contained in a .pdb file, but can also [be embedded directly in your .NET executable](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-options/code-generation#debugtype)), you won’t even get line numbers. In that case, unraveling what’s `null` will require more detective work, making logging even more valuable.

When tracing the source of the `null` value, remember that it might not be visible as `null` in the code. In C#, `null` is the default value for reference types, so it could also come from a field that was never assigned or an array element that was never set.

By convention, in well-written applications using well-written libraries, all instances of `NullReferenceException` must be blamed on the author of the code that throws this exception. However, in codebases that do not follow defensive programming practices, finding the root cause can be more challenging.

Before showing how you can implement defensive programming and make these exceptions easier to diagnose, we will see how to cope with `null` values when they are legitimate and expected.

The full source code of examples in this article is available on [GitHub](https://github.com/postsharp/TimelessDotNetEngineer/tree/main/src/nullreferenceexception).

## How to fix a NullReferenceException?

Once you’ve identified what variable is `null`, you can address the issue.

There are essentially two different kinds of situations, each with a different solution:

1.  When `null` is a legitimate value for the variable or parameter because it is _optional_, and you need to cope with it. For instance, if in a class representing a tree node, the `Parent` property can legitimately contain a `null` reference for the root node.
2.  When `null` is _not_ an expected value, but some external code sent it to you by mistake, you must _defend_ your code against it. This technique is called _defensive programming_.

Let’s look at the first approach first.

## How to cope with legitimate null references?

When a reference can legitimately be `null` (likely because the value is optional), you must cope with it. In that case, you must check if the reference is `null` before accessing the member. The C# language provides various ways to do this, from a simple `!= null` check in an `if` statement, through pattern matching with `is` and `switch` expressions, to null-related operators `?.` and `??`. Which one to use depends on the context and your (or your team’s) preference.

Here are some examples:

```csharp
// Simple null check.
if ( product != null )
{
    cart.Add( product );
}

// Pattern matching with an empty pattern checks for null and assigns to a variable.
if ( shop.SpecialOffer is { } specialOffer )
{
    cart.SetDiscount( specialOffer );
}

// Pattern matching using negated null pattern.
if ( shop is { SpecialOffer: not null } )
{
    cart.SetDiscount( shop.SpecialOffer );
}

// Null-conditional and null-coalescing operators combined to set a default value when the reference is null.
var actualDiscount = discount?.Percent ?? 0;
```

## How do I get rid of a NullReferenceException altogether?

Okay. You fixed a `NullReferenceException`. Now, you want to avoid them from happening in the future. How to do that? This is not a trivial question. Indeed, this problem has been called [the billion-dollar mistake](https://en.wikipedia.org/wiki/Null_pointer#History).

We suggest a four-step approach:

1.  Enable nullability analysis in your project and fix all warnings
2.  Adopt a contract-based mindset
3.  Check preconditions at the surface of your component
4.  Reduce boilerplate code and human errors

### Step 1. Enable nullability analysis in your project and fix all warnings

You might wonder how you can know if a variable, parameter, or field can or cannot contain a null reference so that you can decide if you need to write the code to handle the `null` case.

To resolve this problem, the C# language now includes a feature called _nullable reference types_. It allows you, and the libraries you use, to specify which references can be `null` and which can’t. This feature is optional but enabled by default in new projects targeting modern .NET. We suggest you enable this feature in all projects.

You can enable it by adding the following code to your `csproj` file or, better, to `Directory.Build.props`:

```xml
<PropertyGroup>
  <Nullable>enable</Nullable>
  <LangVersion>latest</LangVersion>
</PropertyGroup>
```

To enable the latest C# language features with legacy target frameworks (such as .NET Framework 4), you can use [PolySharp](https://github.com/Sergio0694/PolySharp).

Now that nullable reference types are enabled, you mark a reference type as nullable by adding a `?` to it. Conversely, if you don’t mark a reference as nullable, it is considered to be non-nullable. Based on these annotations, the compiler or the IDE will report warnings in two cases:

*   when you try to access a member of a nullable reference without checking if it’s `null`, and
*   when you try to assign `null` or a nullable reference to a non-nullable variable, field, or parameter.

For instance, the C# compiler reports a warning in the following code:

```csharp
public decimal ComputePrice( Product product, decimal quantity, Discount? discount )
{
    // C# reports a warning on `discount.Percent` because `discount` can be null.
    return product.Price * quantity * (1 - discount.Percent / 100m);
}
```

But the following code is correct:

```csharp
public decimal ComputePrice( Product product, decimal quantity, Discount? discount )
{
    return product.Price * quantity * (1 - (discount?.Percent ?? 0) / 100m);
}
```

### Step 2. Adopt a contract-based mindset

A key mindset in programming is that of a _contract_. When you write a method, you are implicitly entering a contract with the caller of the method. This contract is “if you call me and you fulfill conditions `A`, `B`, and `C`, I will perform actions `P`, `Q`, `R`, and return `Z`”.

Nullability annotations on a method can be seen as a contract between your method and its caller. If you mark a parameter as nullable, it means that you are willingly accepting null values. However, if you don’t, it means that it is the caller’s responsibility to provide you with a non-null value.

The same applies to the return value: unless you mark the return type as nullable, you are committed to returning a non-null value to the caller. If you return null, then your method, and not the caller, is to blame for the `NullReferenceException` it will cause.

So, who is to blame for the `NullReferenceException` in the following snippet? The author of `GetLastOrder` or `RepeatLastOrder`? Of course, `RepeatLastOrder`, because `GetLastOrder` clearly stated it could return null!

```csharp
// This method requires a Customer but does not promise to return an Order.
public Order? GetLastOrder( Customer customer )
    => customer.Orders.OrderByDescending( o => o.Date ).FirstOrDefault();

public void RepeatLastOrder( Customer customer )
{
    var lastOrder = this.GetLastOrder( customer );

    // This will throw NullReferenceException if the customer never ordered.
    var newOrder = lastOrder.Clone( DateTime.Today );
    this.PostOrder( newOrder );
}
```

For more complex nullability contracts, Microsoft also provides several [nullability contract attributes](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis). For example, consider the `Dictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value)` method. For a non-nullable reference type `TValue`, its `value` parameter is `null` if the method returns `false`, but not-null if it returns `true`. This can’t be expressed just by applying `?`, which is why these attributes exist.

### Step 3. Check preconditions at the surface of your component

To make it easier to diagnose `NullReferenceException` in large applications, a good practice is to implement a practice called _defensive programming_. Instead of accepting that your code could fail because of someone else’s mistake (because this person could ignore warnings), defensive programming advocates that you should validate all inputs (specifically all _preconditions_) and throw a specific kind of exception if any precondition is not fulfilled.

There are three categories of exceptions in .NET:

*   **Precondition failure exceptions** are the ones that put the blame on the _caller_ of the component:
    
    *   `ArgumentException`: means that the caller sends an invalid parameter. For instance, throw `NullArgumentException` if the caller sends you a `null` that you don’t expect.
    *   `InvalidOperationException`: means that the operation is not available in the current state of the object. For instance, you are trying to write into a file opened in read-only mode.
    *   `NotSupportedException`: means that the operation is never available for this kind of object.
*   Some must be blamed on the component itself: `NullReferenceException`, `ArithmeticException`, `InvalidCastException`, …
*   Finally, other exceptions must be blamed on the user or the environment: `OutOfMemoryException`, `IOException`, `OperationCanceledException`, …

Because any `NullReferenceException` in your code will be blamed on nobody else than you, it’s a good practice, not only to cover your back but also to ease the process of code troubleshooting, to check that the sender does not send you a `null` reference by mistake. In this case, you must throw an `ArgumentNullException`.

Another advantage of `ArgumentNullException` is consistency: if a method mistakenly receives a `null` reference, it might throw a `NullReferenceException` only sometimes. Such buggy code might linger in your codebase for a long time, and the exception might be hard to reproduce. Conversely, if you throw an `ArgumentNullException`, the caller will immediately see the bug and will hopefully fix it promptly.

The C# language offers you three ways to throw `ArgumentNullException`:

#### 1\. Use an if statement

The most obvious approach is to add the following code at the top of your method:

```csharp
if ( customer == null )
{
    throw new ArgumentNullException( nameof(customer) );
}
```

#### 2\. Use the new ThrowIfNull

Alternatively, you can use the helper `ThrowIfNull` method:

```csharp
ArgumentNullException.ThrowIfNull( customer );
```

This version is shorter and simpler but provides the same information (the name of the parameter that was `null` is captured using [caller argument expression](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/caller-argument-expression)). Plus, it can even be slightly more efficient because it moves the rarely-executed `throw` statement to a separately compiled method.

#### 3\. Use an inline expression check

A less readable but compact approach is to use add `?? throw` the first time you are using a parameter.

```csharp
public Order? GetLastOrder( Customer customer )
    => (customer ?? throw new ArgumentNullException( nameof(customer) ))
        .Orders
        .OrderByDescending( o => o.Date )
        .FirstOrDefault();
```

Now, where exactly should you place these checks? Best practice is to do this at the surface area of your components. That means checking parameters of public methods and constructors, and the set value in public property setters. This way, you can focus on one component at a time, ensuring that `null` reference issues don’t arise from interactions between components.

### Step 4. Reduce boilerplate code and human errors

If you’re thinking that checking parameters of all public methods sounds like tedious work, you’re absolutely right. Not only is this work boring and time-consuming, but it’s also error-prone. It’s easy to forget to check for nullability in an entry point of your API, and the C# compiler won’t remind you.

This redundant code can be avoided using a tool like Metalama, the free and open-source meta-programming framework. With the [Metalama.Patterns.Contracts](https://www.nuget.org/packages/Metalama.Patterns.Contracts) package, you can annotate a parameter with the [\[NotNull\]](https://doc.postsharp.net/metalama/api/metalama-patterns-contracts-notnullattribute) attribute, and it will automatically generate the `null` check for you. The same attribute works on fields and properties as well. This is a slight improvement, but nothing too exciting.

```csharp
public Order? GetLastOrder( [NotNull] Customer customer )
    => customer.Orders.OrderByDescending( o => o.Date ).FirstOrDefault();
```

The significant improvement comes from the ability to apply `[NotNull]` to an entire project at once. You can do this using a [fabric](https://doc.postsharp.net/metalama/conceptual/using/fabrics) that calls the [VerifyNotNullableDeclarations](https://doc.postsharp.net/metalama/api/metalama-patterns-contracts-contractextensions-verifynotnullabledeclarations) method:

```csharp
using Metalama.Framework.Fabrics;
using Metalama.Patterns.Contracts;

internal class Fabric : ProjectFabric
{
    public override void AmendProject( IProjectAmender amender )
        => amender.VerifyNotNullableDeclarations();
}
```

This will add `null` checks to non-nullable reference types in all public members of your project. There are also ways to limit this to certain namespaces or types, or to expand it to include private members. If you need even more customization, all the relevant code is open source, so you can modify it to suit your exact needs.

## Other questions

### How to handle a NullReferenceException?

You should not! The only exceptions that should be handled are the one that stem from the environment, like I/O or networking exceptions. Any `NullReferenceException` must be considered as a bug and must be fixed. If the `NullReferenceException` is due to a bug in a third-party library, you should study if it can be worked around by sending different input to this library, or handle the exception to avoid making further damages in your application.

### What is a first-chance exception?

Sometimes, you may get a message like _a first-chance exception of type system NullReferenceException occurred in your application_. A first-chance exception is an exception that has not been handled yet.

## Summary

`NullReferenceException` is one of the most common exception types in C#. They can be avoided by adopting a few best practices:

*   Enable null reference types in your C# project and use nullability annotations.
*   Aim for zero warnings.
*   Implement _defensive programming_, i.e., validate your inputs and proactively throw an `ArgumentNullException`.

Defensive programming makes it much easier to diagnose the root cause of a `NullReferenceException` because it logically isolates components from each other, with each component being responsible for itself and applying a least-trust principle to other components. Although it’s a best practice, it also requires writing a lot of redundant code. Tools like Metalama can help add precondition checks without boilerplate code, leveraging the benefits of precondition checking without its inconveniences.

This article was first published on a [https://blog.postsharp.net](https://blog.postsharp.net) under the title [How to Deal With NullReferenceException? Object reference not set to an instance of an object.](https://blog.postsharp.net/nullreferenceexception-object-reference-not-set).