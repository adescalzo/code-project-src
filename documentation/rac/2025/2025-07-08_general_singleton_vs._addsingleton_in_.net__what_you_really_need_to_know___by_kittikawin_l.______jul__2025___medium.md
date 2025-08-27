```yaml
---
title: "Singleton vs. AddSingleton in .NET: What You Really Need to Know | by Kittikawin L. 🍀 | Jul, 2025 | Medium"
source: https://medium.com/@kittikawin_ball/singleton-vs-addsingleton-in-net-what-you-really-need-to-know-2b99f65a4a60
date_published: 2025-07-08T12:06:24.003Z
date_captured: 2025-08-13T11:19:56.451Z
domain: medium.com
author: Kittikawin L. 🍀
category: general
technologies: [.NET, ASP.NET Core, "System.Lazy<T>"]
programming_languages: [C#]
tags: [singleton-pattern, dependency-injection, dotnet, aspnet-core, design-patterns, software-architecture, testability, lifetime-management, concurrency]
key_concepts: [Singleton Pattern, Dependency Injection, Service Lifetimes, Thread Safety, Testability, Coupling, Boilerplate Code, Constructor Injection]
code_examples: false
difficulty_level: intermediate
summary: |
  This article thoroughly compares the traditional Singleton design pattern implementation in C# with the `AddSingleton()` method offered by ASP.NET Core's built-in Dependency Injection container. It details the core principles and common use cases for the Singleton pattern, such as logging and configuration management. The author provides practical C# code examples for both manual, thread-safe singletons (including one using `Lazy<T>`) and `AddSingleton()` registrations. The piece highlights the significant advantages of using `AddSingleton()` in modern .NET applications, emphasizing improved testability, cleaner design, and framework-managed lifecycle control, while advising against manual singletons in most ASP.NET Core scenarios.
---
```

# Singleton vs. AddSingleton in .NET: What You Really Need to Know | by Kittikawin L. 🍀 | Jul, 2025 | Medium

# Singleton vs. AddSingleton in .NET: What You Really Need to Know

![](https://miro.medium.com/v2/resize:fit:700/0*glSxurOCu1pMOu1V)

*Image: A top-down view of a developer's desk setup, featuring an open MacBook Pro displaying code, an iPhone, a set of keys, a notebook with handwritten notes and diagrams, a pen, a smartphone, and a Nikon camera lens, all arranged on a rustic wooden surface.*

The **Singleton Pattern** is one of the most accepted design patterns in software engineering, and it’s one of the **most misused things**.

In the modern world of .NET development, developers often wonder

> Should I implement a singleton manually or just use AddSingleton() in ASP.NET Core?

Let’s take a deep dive into the **differences**, **use cases**, and **obstacles** between the two approaches to using C#.

# What Is the Singleton Pattern?

The **Singleton Pattern** ensures a class has **only one instance** and provides a **global access point** to that instance.

The **Singleton Pattern** is a creative **design pattern** that ensures:

1.  **Only one instance** of a class is created
2.  That instance is **globally accessible**

It provides **controlled access** to a single, shared resource or service, making it ideal for **cross-cutting concerns** like _logging_, _configuration_, or _state management_.

You would generally use a singleton when:

*   You want to **share the configuration or logging**
*   You need a **single connection or cache**
*   You want to **coordinate a resource globally**

# Why Use Singleton?

Here are some classic use cases for the Singleton pattern.

*   Logging service  
    — Centralizes log output
*   App configuration  
    — Single source of truth
*   In-memory cache  
    — One instance shared across the app
*   License control manager  
    — Prevent reuse

# Manual Singleton in C# (Thread-Safe Version)

Here’s how to implement a **manual thread-safe Singleton** in C#:

```csharp
public class Logger  
{  
    private static Logger? _instance;  
    private static readonly object _lock = new();  
  
    private Logger() { }  
  
    public static Logger Instance  
    {  
        get  
        {  
            lock (_lock)  
            {  
                return _instance ??= new Logger();  
            }  
        }  
    }  
  
    public void Log(string message)  
    {  
        Console.WriteLine($"[LOG]: {message}");  
    }  
}  
  
public class Program  
{  
    public static void Main()  
    {  
        Logger.Instance.Log("App started.");  
    }  
}
```

## Pros of Manual Singleton

*   Full control over `**instance**`
*   Good for non-DI scenarios
*   Lightweight for small utilities

## Cons of Manual Singleton

*   Requires [boilerplate](https://en.wikipedia.org/wiki/Boilerplate_code)
*   Not mockable
*   Strongly interconnected
*   Easy to **misuse** in multi-threaded or heavy test environments

# Cleaner Singleton with Lazy

If you must go manual, prefer `Lazy<T`\> for built-in thread safety.

```csharp
public sealed class Logger  
{  
    private static readonly Lazy<Logger> _instance = new(() => new Logger());  
  
    private Logger() { }  
  
    public static Logger Instance => _instance.Value;  
  
    public void Log(string message)  
    {  
        Console.WriteLine($"[LOG]: {message}");  
    }  
}  
  
public class Program  
{  
    public static void Main()  
    {  
        Logger.Instance.Log("Using Lazy<T> version.");  
    }  
}
```

## Lazy Class (System)

### Provides support for lazy initialization.

learn.microsoft.com

# Using `AddSingleton()` In ASP.NET Core

ASP.NET Core includes built-in **Dependency Injection (DI).**

You can register for the service as follows:

*   `AddSingleton<T>()`
*   `AddScoped<T>()`
*   `AddTransient<T>()`

```csharp
public interface ILoggerService  
{  
    void Log(string message);  
}  
  
public class LoggerService : ILoggerService  
{  
    public void Log(string message)  
    {  
        Console.WriteLine($"[LOG]: {message}");  
    }  
}
```

Register it in `Startup.cs` or `Program.cs`

```csharp
services.AddSingleton<ILoggerService, LoggerService>();
```

Now every class that depends on `ILoggerService` will get the **same instance** injected, just like a singleton, but **managed by the framework**.

# Manual Singleton vs. AddSingleton()

## **Manual Singleton**

*   Thread safety  
    — if done right
*   Testability  
    — Hard to mock
*   Coupling  
    — High
*   Lifetime control  
    — Manual
*   Best for  
    — Legacy apps, quick tools

## `AddSingleton()` **in .NET**

*   Thread safety  
    — Built-in
*   Testability  
    — Easily mockable via DI
*   Coupling  
    — Low
*   Lifetime control  
    — Controlled by DI container
*   Best for  
    — Modern web apps and services

# Why prefer AddSingleton() in .NET?

*   **Easier to test:** You can mock interfaces in unit tests.
*   **Cleaner design:** Services declare what they depend on via constructor injection.
*   **Better maintainability:** Your class is separated from the concrete implementation.
*   **Lifecycle control:** .NET handles instantiation, disposition, and threading for you.

# When to Avoid Manual Singleton

Avoid implementing manual singletons when:

*   You’re using ASP.NET Core (just use DI)
*   You care about _testability_ or _mocking_
*   You’re working with multi-threaded logic
*   You need to scale the service across environments

# Use Cases

## Manual Singleton

*   Logging in a console app
*   Third-party SDK in a legacy app

## AddSingleton()

*   Logging in ASP.NET Core
*   Configuration manager
*   Shared cache or registry

# Final Thoughts

Both methods give you **singleton** behavior, but `AddSingleton()` is the **preferred and modern** method in the .NET Core environment.

The Singleton pattern is still **relevant**, but it has evolved. With ASP.NET Core and Dependency Injection, you no longer need to write boilerplate code to control instances.

**Leverage** `**AddSingleton()**` for better _structure_, _testability_, and _scalability,_ and save manual singleton code for legacy or simple use cases.

# 🙌 Stay in the loop:

*   ☕️ Enjoying the content? [Buy me a coffee](https://buymeacoffee.com/rookiedev) to support more writing like this.
*   📬 [Follow](/@kittikawin_ball) and [subscribe](/@kittikawin_ball/subscribe) for weekly tech news, practical tips, coding patterns, and tech reflections.

> _Thanks for reading!_

See you,

**Kittikawin**

## Singleton pattern - Wikipedia

### In object-oriented programming, the singleton pattern is a software design pattern that restricts the instantiation of…

en.wikipedia.org