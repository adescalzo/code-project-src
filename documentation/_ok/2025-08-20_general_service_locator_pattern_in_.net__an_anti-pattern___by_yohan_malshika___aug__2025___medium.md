```yaml
---
title: "Service Locator Pattern in .NET: An Anti-Pattern | by Yohan Malshika | Aug, 2025 | Medium"
source: https://malshikay.medium.com/service-locator-pattern-in-net-an-anti-pattern-c5206c26fbf6
date_published: 2025-08-20T11:57:11.305Z
date_captured: 2025-08-27T11:18:22.272Z
domain: malshikay.medium.com
author: Yohan Malshika
category: general
technologies: [.NET, .NET Core, .NET Framework, IServiceProvider]
programming_languages: [C#]
tags: [service-locator, anti-pattern, dependency-injection, ioc, dotnet, software-design, testing, code-quality, design-patterns, solid-principles]
key_concepts: [Service Locator pattern, Dependency Injection, Inversion of Control, Hidden Dependencies, Unit Testing, Tight Coupling, SOLID principles, Code Maintainability]
code_examples: false
difficulty_level: intermediate
summary: |
  The article critically analyzes the Service Locator pattern, labeling it an anti-pattern in modern .NET development. It details how Service Locator harms code quality by hiding dependencies, violating Inversion of Control, complicating unit testing, and fostering poor design practices. The author strongly advocates for Dependency Injection as the superior alternative, highlighting its benefits for dependency visibility, testability, and adherence to SOLID principles. While acknowledging its presence in legacy systems, the article concludes that Dependency Injection offers a clearer and more maintainable approach for contemporary applications.
---
```

# Service Locator Pattern in .NET: An Anti-Pattern | by Yohan Malshika | Aug, 2025 | Medium

# Service Locator Pattern in .NET: An Anti-Pattern

## Why Service Locator hurts code quality in .NET

![Two developers working at their desks, one in the foreground with their back to the camera, looking at code on a monitor, and another in the background.](https://miro.medium.com/v2/resize:fit:700/0*GC4MI9Zska_cQx55)

The Service Locator pattern is one of those concepts that sounds helpful at first. You create a central “locator” where your application can get any service it needs. No need to pass dependencies everywhere — just ask the locator for them.

But in modern .NET development, this pattern is widely considered an **anti-pattern**. Let’s explore why.

## What Is the Service Locator Pattern?

The Service Locator is an object that knows how to return instances of services (classes, interfaces, etc.) on demand.
It usually looks like this in C#:

```csharp
public interface IServiceLocator
{
    T GetService<T>();
}

public class ServiceLocator : IServiceLocator
{
    private readonly IServiceProvider _provider;
    public ServiceLocator(IServiceProvider provider)
    {
        _provider = provider;
    }
    public T GetService<T>()
    {
        return (T)_provider.GetService(typeof(T));
    }
}
```

Anywhere in your code, you can call:

```csharp
var myService = ServiceLocator.Current.GetService<IMyService>();
```

It feels convenient — but that’s also the problem.

## Why It’s Considered an Anti-Pattern

## 1. Hides Dependencies

With the Service Locator, a class can silently pull in any dependency it wants.
You don’t see these dependencies in the constructor, so they’re hidden from anyone reading the code.

Example:

```csharp
public class OrderProcessor
{
    public void Process()
    {
        var paymentService = ServiceLocator.Current.GetService<IPaymentService>();
        paymentService.Pay();
    }
}
```

From the outside, `OrderProcessor` looks simple. But in reality, it depends on `IPaymentService` — something you can’t tell without reading the method body.

## 2. Breaks Inversion of Control (IoC)

The whole idea of **Dependency Injection (DI)** is that classes declare their dependencies, and an external container provides them.
With Service Locator, your code is asking for dependencies instead of having them injected, which is the opposite of IoC.

## 3. Harder to Test

Unit tests rely on easily swapping real dependencies with mocks or fakes.
When you use a Service Locator inside a class, you must configure the locator for each test, which adds complexity.

With DI:

```csharp
public class OrderProcessor
{
    private readonly IPaymentService _paymentService;

    public OrderProcessor(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    public void Process()
    {
        _paymentService.Pay();
    }
}
```

In tests, you can pass a fake `IPaymentService` directly — no locator needed.

## 4. Encourages Bad Design

Because it’s so easy to “just grab” a service from anywhere, developers may skip proper design.
Over time, this creates code that’s tightly coupled and hard to refactor.

## The Better Alternative: Dependency Injection in .NET

In .NET Core and later, **Dependency Injection is built-in**.
Instead of using a Service Locator, register your services in `Program.cs`:

```csharp
builder.Services.AddScoped<IPaymentService, PaymentService>();
```

Then inject dependencies through constructors:

```csharp
public class OrderProcessor
{
    private readonly IPaymentService _paymentService;

    public OrderProcessor(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }
    public void Process()
    {
        _paymentService.Pay();
    }
}
```

This way:

*   Dependencies are **visible** in the constructor.
*   Code is easier to **test**.
*   Design follows **SOLID principles**.

## When You Might Still See It

You might find Service Locator in:

*   Very old .NET Framework projects without built-in DI.
*   Libraries that can’t rely on the application’s DI container.
*   Legacy code where refactoring is risky.

Even then, most experts recommend phasing it out.

## Final Thoughts

The Service Locator pattern can be tempting because it seems to make dependency management easier. But in reality, it hides dependencies, breaks IoC, makes testing harder, and encourages poor design.

In modern .NET applications, **Dependency Injection** is the recommended approach. It’s clearer, more maintainable, and fully supported by the framework.