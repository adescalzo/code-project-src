```yaml
---
title: "Strategy Pattern vs Factory Pattern in .NET — Understanding the Difference with an Investment Plan | by DotNet Full Stack Dev | Sep, 2025 | Medium"
source: https://dotnetfullstackdev.medium.com/strategy-pattern-vs-factory-pattern-in-net-understanding-the-difference-with-an-investment-plan-ebd707b83ddf
date_published: 2025-09-06T13:42:29.290Z
date_captured: 2025-09-10T12:44:27.360Z
domain: dotnetfullstackdev.medium.com
author: DotNet Full Stack Dev
category: general
technologies: [.NET]
programming_languages: [C#]
tags: [design-patterns, strategy-pattern, factory-pattern, dotnet, software-architecture, oop, object-creation, behavior, software-design]
key_concepts: [Strategy Pattern, Factory Pattern, object creation, algorithm encapsulation, behavior encapsulation, interfaces, polymorphism, software design, enterprise systems]
code_examples: false
difficulty_level: intermediate
summary: |
  This article clarifies the distinct roles of the Strategy Pattern and Factory Pattern within .NET development, using an investment plan example. It illustrates how the Strategy Pattern enables dynamic algorithm switching by defining a family of interchangeable behaviors, while the Factory Pattern centralizes and abstracts the process of object creation. Through practical C# code examples, the author demonstrates the implementation of each pattern and explains how they can be effectively combined in real-world applications to manage both behavior and object instantiation. The piece emphasizes that despite their superficial similarities, these patterns serve fundamentally different responsibilities in software design.
---
```

# Strategy Pattern vs Factory Pattern in .NET — Understanding the Difference with an Investment Plan | by DotNet Full Stack Dev | Sep, 2025 | Medium

# Strategy Pattern vs Factory Pattern in .NET — Understanding the Difference with an Investment Plan

When working in enterprise systems, especially in finance, you often encounter situations where **different algorithms** or **creation logics** need to be chosen dynamically. That’s where two famous design patterns come into play:

*   **Strategy Pattern** → Focused on **behavior (algorithms)**
*   **Factory Pattern** → Focused on **object creation**

They look similar but serve very different responsibilities. Let’s dive in.

![Diagram illustrating the core differences between Strategy Pattern and Factory Pattern. The Strategy Pattern focuses on behavior, encapsulating algorithms and defining a family of investment plans (Aggressive, Balanced, Conservative) through an `InvestmentContext` and `IInvestmentStrategy` interface. The Factory Pattern focuses on object creation, encapsulating instantiation and creating an `IInvestmentStrategy` based on input using an `InvestmentStrategyFactory`.](https://miro.medium.com/v2/resize:fit:700/0*Ncx9fKNLl6Oqntn0)

## 1\. The Business Problem: Investment Plans

Imagine you’re building a fintech system that provides different investment plans to customers:

*   **Aggressive Plan** → high risk, high return
*   **Balanced Plan** → medium risk, medium return
*   **Conservative Plan** → low risk, low return

Two common requirements arise:

1.  **Strategy need** → Apply different calculation algorithms (returns, risk scores) dynamically at runtime.
2.  **Factory need** → Create the right investment plan object based on user choice or external input.

## 2\. Strategy Pattern — Focus on Behavior

The **Strategy Pattern** says: _“Define a family of algorithms, put each one in a separate class, and make them interchangeable at runtime.”_

## Step 1: Define the Strategy Interface

```csharp
public interface IInvestmentStrategy  
{  
    decimal CalculateReturn(decimal amount, int years);  
}
```

## Step 2: Concrete Strategies

```csharp
public class AggressiveStrategy : IInvestmentStrategy  
{  
    public decimal CalculateReturn(decimal amount, int years)  
        => amount * (decimal)Math.Pow(1.15, years); // 15% growth  
}  
  
public class BalancedStrategy : IInvestmentStrategy  
{  
    public decimal CalculateReturn(decimal amount, int years)  
        => amount * (decimal)Math.Pow(1.08, years); // 8% growth  
}  
  
public class ConservativeStrategy : IInvestmentStrategy  
{  
    public decimal CalculateReturn(decimal amount, int years)  
        => amount * (decimal)Math.Pow(1.04, years); // 4% growth  
}
```

## Step 3: Context Class

```csharp
public class InvestmentContext  
{  
    private readonly IInvestmentStrategy _strategy;  
  
    public InvestmentContext(IInvestmentStrategy strategy)  
    {  
        _strategy = strategy;  
    }  
  
    public decimal ExecuteStrategy(decimal amount, int years)  
    {  
        return _strategy.CalculateReturn(amount, years);  
    }  
}
```

## Usage

```csharp
var context = new InvestmentContext(new BalancedStrategy());  
var returns = context.ExecuteStrategy(10000, 5);  
Console.WriteLine($"Projected return: {returns:C}");
```

👉 **Key Point:**  
The **Strategy Pattern** is all about switching _behavior (algorithm)_ at runtime.

## 3\. Factory Pattern — Focus on Object Creation

The **Factory Pattern** says: _“Centralize the creation logic of objects so the client doesn’t need to know which class to instantiate.”_

Here, instead of directly creating `AggressiveStrategy` or `BalancedStrategy`, we let a factory decide.

## Factory Example

```csharp
public static class InvestmentStrategyFactory  
{  
    public static IInvestmentStrategy CreateStrategy(string type) =>  
        type.ToLower() switch  
        {  
            "aggressive" => new AggressiveStrategy(),  
            "balanced"   => new BalancedStrategy(),  
            "conservative" => new ConservativeStrategy(),  
            _ => throw new ArgumentException("Invalid strategy type")  
        };  
}
```

## Usage

```csharp
var strategy = InvestmentStrategyFactory.CreateStrategy("aggressive");  
var context = new InvestmentContext(strategy);  
  
var returns = context.ExecuteStrategy(20000, 10);  
Console.WriteLine($"Projected return: {returns:C}");
```

👉 **Key Point:**  
The **Factory Pattern** is about **object creation encapsulation**. The client just asks the factory, not worrying about _new-ing up_ specific classes.

## 4\. Combined Usage in Real Systems

In practice, you often use **both together**:

1.  The **Factory** decides _which strategy to instantiate_ (based on config, user choice, or market rules).
2.  The **Strategy** executes the behavior (calculate returns, risk analysis).

Example:

```csharp
var chosenStrategy = InvestmentStrategyFactory.CreateStrategy("conservative");  
var context = new InvestmentContext(chosenStrategy);  
  
var result = context.ExecuteStrategy(50000, 7);  
Console.WriteLine($"Conservative plan returns: {result:C}");
```

## 5\. Real-World Analogy

*   **Strategy** = The _chef’s cooking style_ (spicy, mild, diet).
*   **Factory** = The _restaurant menu_ that picks which chef to assign to you.
*   Together → You order “Balanced Meal” (factory picks the chef) → Chef cooks in his style (strategy).

## ✅ Key Takeaways

*   Strategy → Defines **how behavior changes dynamically**.
*   Factory → Defines **how objects are created without exposing new**.
*   They look similar because both involve interfaces and multiple implementations, but their responsibilities are different.
*   In real-world projects (like investment plans), you often **combine them**: Factory picks the right strategy, Strategy defines the algorithm.