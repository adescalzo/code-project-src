```yaml
---
title: "Strategy Pattern in .NET: The Cleanest Way to Decouple Business Logic | by DotNet Full Stack Dev | Jun, 2025 | Medium"
source: https://dotnetfullstackdev.medium.com/strategy-pattern-in-net-the-cleanest-way-to-decouple-business-logic-059102a2b2bc
date_published: 2025-06-27T15:31:59.447Z
date_captured: 2025-08-06T17:51:20.230Z
domain: dotnetfullstackdev.medium.com
author: DotNet Full Stack Dev
category: general
technologies: [.NET, .NET Core]
programming_languages: [C#]
tags: [design-patterns, strategy-pattern, dotnet, csharp, software-architecture, decoupling, oop, dependency-injection, clean-code, maintainability]
key_concepts: [Strategy Pattern, Decoupling, Encapsulation, Open/Closed Principle, Single Responsibility Principle, Dependency Injection, Polymorphism, Testability]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces the Strategy Pattern, a design pattern that enables defining interchangeable algorithms within separate classes. It highlights the pattern's benefits, such as improved flexibility, testability, and adherence to principles like Open/Closed and Single Responsibility, by avoiding large conditional statements. The author illustrates its practical application through a real-world scenario of integrating multiple payment gateways in an e-commerce application. Detailed C# code examples demonstrate how to implement the pattern using interfaces, concrete strategies, and a context class, with a bonus mention of integrating it with Dependency Injection in .NET Core. The piece emphasizes the pattern's role in building flexible, testable, and sustainable software systems.
---
```

# Strategy Pattern in .NET: The Cleanest Way to Decouple Business Logic | by DotNet Full Stack Dev | Jun, 2025 | Medium

# Strategy Pattern in .NET: The Cleanest Way to Decouple Business Logic

# What is the Strategy Pattern?

> The **Strategy Pattern** allows you to define a family of algorithms (strategies), put them in separate classes, and make them interchangeable at runtime.

This pattern is about **encapsulation** and **flexibility**. You define _what_ to do, but delegate _how_ to do it to separate strategy classes.

![A diagram illustrating the Strategy Pattern in .NET, showing PaymentContext depending on the IPaymentStrategy interface, which is implemented by concrete strategy classes like CreditCardPayment, PayPalPayment, and CryptoPayment, all defining a Pay(decimal amount) method.](https://miro.medium.com/v2/resize:fit:700/0*4Su1B5gELmg5rSgZ)

# Why Use It?

Without Strategy Pattern, you often end up with:

*   🔁 Huge `if-else` or `switch` statements
*   🔗 Tightly coupled logic
*   🚫 Hard-to-maintain, non-testable code

With Strategy Pattern:

*   🧩 You can swap logic without changing consumers
*   🧪 You can easily mock or test strategies
*   🧼 Open/Closed Principle: add new behaviors without modifying existing ones

# Real-World Scenario: Payment Gateway Integration

Let’s say you’re building an e-commerce app that supports multiple payment methods:

*   Credit Card
*   PayPal
*   Crypto

**Without Strategy Pattern (Tightly Coupled)**

```csharp
public class PaymentService  
{  
    public void Pay(string paymentType)  
    {  
        if (paymentType == "CreditCard")  
        {  
            // Credit card logic  
        }  
        else if (paymentType == "PayPal")  
        {  
            // PayPal logic  
        }  
        else if (paymentType == "Crypto")  
        {  
            // Crypto logic  
        }  
    }  
}
```

**Problems:**

*   Every time a new method is added, the `Pay` method grows.
*   Difficult to unit test or reuse logic.
*   Violates Single Responsibility & Open/Closed Principles.

# With Strategy Pattern (Cleanly Decoupled)

# 1. Create an Interface

```csharp
public interface IPaymentStrategy  
{  
    void Pay(decimal amount);  
}
```

**2. Implement Each Strategy**

```csharp
public class CreditCardPayment : IPaymentStrategy  
{  
    public void Pay(decimal amount)  
    {  
        Console.WriteLine($"Paid {amount} using Credit Card");  
    }  
}  
  
public class PayPalPayment : IPaymentStrategy  
{  
    public void Pay(decimal amount)  
    {  
        Console.WriteLine($"Paid {amount} using PayPal");  
    }  
}  
  
public class CryptoPayment : IPaymentStrategy  
{  
    public void Pay(decimal amount)  
    {  
        Console.WriteLine($"Paid {amount} using Cryptocurrency");  
    }  
}
```

**3. Create a Context Class**

```csharp
public class PaymentContext  
{  
    private readonly IPaymentStrategy _paymentStrategy;  
  
    public PaymentContext(IPaymentStrategy paymentStrategy)  
    {  
        _paymentStrategy = paymentStrategy;  
    }  
  
    public void ExecutePayment(decimal amount)  
    {  
        _paymentStrategy.Pay(amount);  
    }  
}
```

**4. Use It in Your App**

```csharp
var context = new PaymentContext(new CreditCardPayment());  
context.ExecutePayment(100);  
  
context = new PaymentContext(new PayPalPayment());  
context.ExecutePayment(200);
```

**Bonus: Strategy with Dependency Injection in .NET Core**

You can register strategies using DI:

```csharp
services.AddTransient<IPaymentStrategy, CreditCardPayment>();  
services.AddTransient<IPaymentStrategy, PayPalPayment>();
```

Then inject them using constructor or factory pattern.

# Final Thought

The **Strategy Pattern truly shines** when you want to plug-and-play different behaviors — without rewriting core business logic.

It’s not just about architecture — it’s about **building a system that’s flexible, testable, and sustainable.**