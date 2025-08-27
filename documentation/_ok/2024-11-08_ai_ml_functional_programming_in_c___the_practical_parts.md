```yaml
---
title: "Functional Programming in C#: The Practical Parts"
source: https://www.milanjovanovic.tech/blog/functional-programming-in-csharp-the-practical-parts?utm_source=newsletter&utm_medium=email&utm_campaign=tnw115
date_published: 2024-11-09T00:00:00.000Z
date_captured: 2025-08-17T21:41:21.048Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: ai_ml
technologies: [.NET, LINQ, OneOf, System.Collections.Immutable]
programming_languages: [C#]
tags: [functional-programming, csharp, dotnet, software-design, immutability, error-handling, higher-order-functions, monads, pure-functions, linq]
key_concepts: [functional-programming, higher-order-functions, errors-as-values, monadic-binding, pure-functions, immutability, discriminated-unions, result-pattern]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores practical applications of functional programming patterns in C#, demystifying concepts often perceived as academic. It highlights how C# features like records, LINQ, and lambda expressions support a more functional style. The author demonstrates five key patterns: higher-order functions for flexible code, explicit error handling using the OneOf library, monadic binding for chaining operations, pure functions for predictability, and immutability for safer state management. The piece emphasizes that adopting these patterns incrementally can lead to more robust, testable, and maintainable C# applications.
---
```

# Functional Programming in C#: The Practical Parts

![Functional Programming in C#: The Practical Parts blog cover](https://www.milanjovanovic.tech/blog-covers/mnw_115.png?imwidth=3840)

# Functional Programming in C#: The Practical Parts

6 min read · November 09, 2024

You know that feeling when you have an idea in your head but can't think of the word for it? Or you have documents in MongoDB Atlas but not the exact text to search them? [**Vector search**](https://shortclick.link/1oo137) might be the answer. Adding MongoDB Atlas Vector Search is pretty [**straightforward**](https://shortclick.link/1oo137), and can come in handy.

Auto-Generate [**enterprise-grade C# SDKs**](https://www.speakeasy.com/docs/sdk-design/csharp/oss-comparison-csharp?utm_source=net-weekly) from OpenAPI specs. Create customizable, strongly-typed C# SDKs with Speakeasy's SDK generator - designed for enterprise APIs where open-source solutions fall short. Compare our SDK generator to popular OSS alternatives and [**create your first SDK for free**](https://www.speakeasy.com/docs/sdk-design/csharp/oss-comparison-csharp?utm_source=net-weekly).

[Sponsor this newsletter](/sponsor-the-newsletter)

Functional programming patterns can feel academic and abstract. Terms like "monads" and "functors" scare many developers away. But beneath the intimidating terminology are practical patterns that can make your code safer and more maintainable.

C# has embraced many functional programming features over the years.

*   Records for immutability
*   LINQ for functional transformations
*   Lambda expressions for first-class functions

These features aren't just syntax sugar - they help prevent bugs and make code easier to reason about.

Let's look at five practical patterns you can use in your C# projects today.

## [Higher-Order Functions](#higher-order-functions)

Higher-order functions can take other functions as parameters or return them as results. They let you write code that's more flexible and composable because you can pass behavior around like data.

Common examples of higher-order functions are LINQ's `Where` and `Select`, which take functions to transform data.

Let's refactor this validation example with higher-order functions:

```csharp
public class OrderValidator
{
    public bool ValidateOrder(Order order)
    {
        if (order.Items.Count == 0) return false;
        if (order.TotalAmount <= 0) return false;
        if (order.ShippingAddress == null) return false;
        return true;
    }
}

// What if we need:
// - different validation rules for different countries?
// - to reuse some validations but not others?
// - to combine validations differently?
```

Here's how higher-order functions make this more flexible:

```csharp
public static class OrderValidation
{
    public static Func<Order, bool> CreateValidator(string countryCode, decimal minimumOrderValue)
    {
        var baseValidations = CombineValidations(
            o => o.Items.Count > 0,
            o => o.TotalAmount >= minimumOrderValue,
            o => o.ShippingAddress != null
        );

        return countryCode switch
        {
            "US" => CombineValidations(
                baseValidations,
                order => IsValidUSAddress(order.ShippingAddress)),
            "EU" => CombineValidations(
                baseValidations,
                order => IsValidVATNumber(order.VatNumber)),
            _ => baseValidations
        };
    }

    private static Func<Order, bool> CombineValidations(params Func<Order, bool>[] validations) =>
        order => validations.All(v => v(order));
}

// Usage
var usValidator = OrderValidation.CreateValidator("US", minimumOrderValue: 25.0m);
var euValidator = OrderValidation.CreateValidator("EU", minimumOrderValue: 30.0m);
```

The higher-order function approach makes validators composable, testable, and easy to extend. Each validation rule is a simple function that we can compose.

## [Errors as Values](#errors-as-values)

Error handling in C# often looks like this:

```csharp
public class UserService
{
    public User CreateUser(string email, string password)
    {
        if (string.IsNullOrEmpty(email))
        {
            throw new ArgumentException("Email is required");
        }

        if (password.Length < 8)
        {
            throw new ArgumentException("Password too short");
        }

        if (_userRepository.EmailExists(email))
        {
            throw new DuplicateEmailException(email);
        }

        // Create user...
    }
}
```

The problem?

*   [Exceptions are expensive](https://youtu.be/E3dU9Y1CsnI)
*   Callers often forget to handle exceptions
*   The method signature lies - it claims to return a User but might throw

We can make errors explicit using the [OneOf](https://github.com/mcintyre321/OneOf) library. It provides discriminated unions for C#, using a custom type `OneOf<T0, ... Tn>`.

```csharp
public class UserService
{
    public OneOf<User, ValidationError, DuplicateEmailError> CreateUser(string email, string password)
    {
        if (string.IsNullOrEmpty(email))
        {
            return new ValidationError("Email is required");
        }

        if (password.Length < 8)
        {
            return new ValidationError("Password too short");
        }

        if (_userRepository.EmailExists(email))
        {
            return new DuplicateEmailError(email);
        }

        return new User(email, password);
    }
}
```

By making the errors explicit:

*   The method signature tells the whole truth
*   Callers must handle all possible outcomes
*   No performance overhead from exceptions
*   The flow is easier to follow

Here's how you use it:

```csharp
var result = userService.CreateUser(email, password);

result.Switch(
    user => SendWelcomeEmail(user),
    validationError => HandleError(validationError),
    duplicateError => HandleError(duplicateError)
);
```

## [Monadic Binding](#monadic-binding)

A **monad** is a container for values - like `List<T>`, `IEnumerable<T>`, or `Task<T>`. What makes it special is that you can chain operations on the contained values without dealing with the container directly. This chaining is called monadic binding.

You use monadic binding daily with LINQ, but you might not know it. It's what allows us to chain operations that transform data.

Map (`Select`) transforms values:

```csharp
// Simple transformations with Select (Map)
var numbers = new[] { 1, 2, 3, 4 };

var doubled = numbers.Select(x => x * 2);
```

Bind (`SelectMany`) transforms and flattens:

```csharp
// Operations that return multiple values use SelectMany (Bind)
var folders = new[] { "docs", "photos" };

var files = folders.SelectMany(folder => Directory.GetFiles(folder));
```

A popular example of applying monads in practice is the [**Result pattern**](functional-error-handling-in-dotnet-with-the-result-pattern), which provides a clean way to chain operations that might fail.

## [Pure Functions](#pure-functions)

Pure functions are predictable: they depend only on their inputs and don't change anything in the system. No database calls, no API requests, no global state. This constraint makes them easier to understand, test, and debug.

```csharp
// Impure - relies on hidden state
public class PriceCalculator
{
    private decimal _taxRate;
    private List<Discount> _activeDiscounts;

    public decimal CalculatePrice(Order order)
    {
        var price = order.Items.Sum(i => i.Price);

        foreach (var discount in _activeDiscounts)
        {
            price -= discount.Calculate(price);
        }

        return price * (1 + _taxRate);
    }
}
```

Here's the same example as a pure function:

```csharp
// Pure - everything is explicit
public static class PriceCalculator
{
    public static decimal CalculatePrice(
        Order order,
        decimal taxRate,
        IReadOnlyList<Discount> discounts)
    {
        var basePrice = order.Items.Sum(i => i.Price);

        var afterDiscounts = discounts.Aggregate(
            basePrice,
            (price, discount) => price - discount.Calculate(price));

        return afterDiscounts * (1 + taxRate);
    }
}
```

Pure functions are thread-safe, easy to test, and simple to reason about because all dependencies are explicit.

## [Immutability](#immutability)

Immutable objects can't be changed after creation. Instead, they create new instances for every change. This simple constraint eliminates entire categories of bugs: race conditions, accidental modifications, and inconsistent state.

Here's an example of a mutable type:

```csharp
public class Order
{
    public List<OrderItem> Items { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; }

    public void AddItem(OrderItem item)
    {
        Items.Add(item);
        Total += item.Price;
        // Bug: Thread safety issues
        // Bug: Can modify shipped orders
        // Bug: Total might not match Items
    }
}
```

Let's make this an immutable type:

```csharp
public record Order
{
    public ImmutableList<OrderItem> Items { get; init; }
    public OrderStatus Status { get; init; }
    public decimal Total => Items.Sum(x => x.Price);

    public Order AddItem(OrderItem item)
    {
        if (Status != OrderStatus.Created)
        {
            throw new InvalidOperationException("Can't modify shipped orders");
        }

        return this with
        {
            Items = Items.Add(item)
        };
    }
}
```

The immutable version:

*   Is thread-safe by default
*   Makes invalid states impossible
*   Keeps data and calculations consistent
*   Makes changes explicit and traceable

## [Takeaway](#takeaway)

[**Functional programming**](how-to-apply-functional-programming-in-csharp) isn't just about writing "cleaner" code. These patterns fundamentally change how you handle complexity:

*   **Push errors to compile time** - Catch problems before running the code
*   **Make invalid states impossible** - Don't rely on documentation or conventions
*   **Make the happy path obvious** - When everything is explicit, the flow is clear

You can adopt these patterns gradually. Start with one class, one module, one feature. The goal isn't to write purely functional code. The goal is to write code that's safer, more predictable, and easier to maintain.

Hope this was helpful. See you next week.

---

Whenever you're ready, there are 4 ways I can help you:

1.  [Pragmatic Clean Architecture:](/pragmatic-clean-architecture) Join 4,200+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [Modular Monolith Architecture:](/modular-monolith-architecture) Join 2,100+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [(NEW) Pragmatic REST APIs:](/pragmatic-rest-apis) Join 1,200+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
4.  [Patreon Community:](https://www.patreon.com/milanjovanovic) Join a community of 5,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Become a Better .NET Software Engineer

Join 70,000+ engineers who are improving their skills every Saturday morning.

Join 70K+ Engineers

Accelerate Your .NET Skills 🚀

![Pragmatic Clean Architecture course cover showing a layered architecture diagram](https://www.milanjovanovic.tech/_next/static/media/cover.27333f2f.png?imwidth=384)

Pragmatic Clean Architecture

![Modular Monolith Architecture course cover](https://www.milanjovanovic.tech/_next/static/media/cover.31e11f05.png?imwidth=384)

Modular Monolith Architecture

![Pragmatic REST APIs course cover](https://www.milanjovanovic.tech/_next/static/media/cover_1.fc0deb78.png?imwidth=384)

Pragmatic REST APIs

NEW