```yaml
---
title: "Dependency Injection, Inversion of Control in C# .NET 8 | by Anderson Godoy | Medium"
source: https://medium.com/@anderson.buenogod/dependency-injection-inversion-of-control-in-c-net-8-2caef0086332
date_published: 2024-09-19T10:38:40.799Z
date_captured: 2025-09-03T21:56:48.258Z
domain: medium.com
author: Anderson Godoy
category: programming
technologies: [.NET 8, ASP.NET Core, Moq]
programming_languages: [C#]
tags: [dependency-injection, inversion-of-control, solid-principles, design-patterns, .net, csharp, software-architecture, extensibility, testability, service-lifetimes]
key_concepts: [Dependency Injection, Inversion of Control, SOLID principles, Factory Pattern, Strategy Pattern, Access Modifiers, Service Lifetimes, Mocking, Polymorphism, Encapsulation]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides an in-depth exploration of Dependency Injection (DI) and Inversion of Control (IoC) within C# .NET 8, focusing on building robust, scalable, and testable applications. It emphasizes adhering to SOLID principles, leveraging access modifiers, and integrating design patterns like Factory and Strategy. The author explains the importance of not sealing classes for extensibility and testability, and details the three primary DI service lifetimes: Singleton, Scoped, and Transient. Through practical C# code examples, the content demonstrates how to combine these advanced concepts to achieve a flexible and maintainable software architecture.
---
```

# Dependency Injection, Inversion of Control in C# .NET 8 | by Anderson Godoy | Medium

# Dependency Injection, Inversion of Control in C# .NET 8

![Anderson Godoy](https://miro.medium.com/v2/resize:fill:64:64/1*0NVGVLLAljoMkq7cw9DopQ@2x.jpeg)

[Anderson Godoy](/@anderson.buenogod?source=post_page---byline--2caef0086332---------------------------------------)

Follow

5 min read

·

Sep 19, 2024

17

Listen

Share

More

Advanced Concepts with SOLID, Access Modifiers, and Design Patterns

![A stylized diagram illustrating Dependency Injection (DI) in .NET 8, showing the relationships between classes, different service lifetimes (Singleton, Scoped, Transient), and design patterns like Factory and Strategy. The diagram uses abstract icons such as cubes, spheres, clouds, computers, and servers to represent these architectural concepts.](https://miro.medium.com/v2/resize:fit:700/0*qj0cuHjipW_EZGne)

In software development, **Dependency Injection (DI)** and **Inversion of Control (IoC)** are key architectural patterns used for building maintainable, scalable, and testable applications. But in C# .NET 8, we can push these concepts further by leveraging **access modifiers**, **SOLID principles**, and **design patterns** to build more robust solutions.

In this article, we’ll take a deep dive into how you can use DI and IoC while adhering to SOLID principles, access control, and applying common design patterns like **Factory** and **Strategy**.

## What is Dependency Injection?

At its core, **Dependency Injection** is a technique where a class receives its dependencies from an external source rather than creating them internally. This promotes **Single Responsibility** and **Open/Closed** principles from SOLID, making your codebase more maintainable.

## Why Are the Classes Not Sealed?

In C#, the `sealed` keyword prevents a class from being inherited. While sealing a class can be useful to prevent unintended inheritance, there are valid reasons for **not sealing** classes in the context of DI and SOLID principles:

1.  **Extensibility (Open/Closed Principle)**: By allowing classes to be inherited, we make the code open for extension. This means developers can create specialized versions of the service classes without modifying the original code. For example, if we had sealed the `EmailNotificationService`, creating an `AdvancedEmailNotificationService` to extend its functionality would be impossible.
2.  **Testability**: In unit testing, it’s common to use **mocking frameworks** to create mock versions of classes or services. Sealing a class can limit the flexibility of testing tools like Moq, which rely on the ability to override and substitute class methods.
3.  **Design Patterns (Strategy, Factory, etc.)**: Many design patterns depend on class inheritance and polymorphism. Sealing classes would restrict the application of these patterns in more complex systems.

That said, you should **seal classes** if you explicitly intend to **prevent** inheritance to ensure immutability or to follow specific security requirements.

## Advanced Dependency Injection with Factory Pattern

The **Factory Pattern** is ideal for situations where we need to instantiate objects dynamically based on certain conditions. This pattern can be combined with DI to provide greater flexibility.

**Example 1: DI with Factory Pattern**

Let’s extend our example to include both email and SMS notifications, dynamically deciding which service to use based on the input.

```csharp
public interface INotificationService  
{  
    void Send(string message);  
}  
  
internal class EmailNotificationService : INotificationService  
{  
    public void Send(string message)  
    {  
        Console.WriteLine($"Email: {message}");  
    }  
}  
  
internal class SmsNotificationService : INotificationService  
{  
    public void Send(string message)  
    {  
        Console.WriteLine($"SMS: {message}");  
    }  
}  
  
// Factory for creating notification services  
public class NotificationFactory  
{  
    public static INotificationService Create(string type)  
    {  
        return type.ToLower() switch  
        {  
            "email" => new EmailNotificationService(),  
            "sms" => new SmsNotificationService(),  
            _ => throw new ArgumentException("Invalid notification type")  
        };  
    }  
}
```

By abstracting the instantiation logic into a `NotificationFactory`, we adhere to the **Single Responsibility Principle**. The factory pattern also allows our code to remain **open for extension** while ensuring that the client code is **closed for modification**.

## Different Service Lifetimes in Dependency Injection

In .NET, the Dependency Injection (DI) container provides three primary service lifetimes. Understanding these lifetimes is critical for managing memory and ensuring that objects are reused or created as required.

1.  **Singleton**: A single instance of the service is created and shared throughout the entire lifetime of the application. This is useful for services that maintain state or are expensive to initialize (e.g., database connections or cache systems).

    ```csharp
    builder.Services.AddSingleton<INotificationService, EmailNotificationService>();
    ```

    **When to Use**: When your service is stateless or you want the same instance to be used globally across the application.

2.  **Scoped**: A new instance of the service is created per HTTP request (in the context of web applications). Each request gets its own service instance, but the same instance is used throughout the life of that request.

    ```csharp
    builder.Services.AddScoped<INotificationService, EmailNotificationService>();
    ```

    **When to Use**: When you need to maintain some form of state across a single request but want isolation between requests.

3.  **Transient**: A new instance of the service is created every time it is requested from the container. This is useful for lightweight, stateless services that do not maintain state between invocations.

    ```csharp
    builder.Services.AddTransient<INotificationService, EmailNotificationService>();
    ```

    **When to Use**: When the service is stateless and you want a new instance every time to ensure no residual state is carried over between uses.

### Practical Example of Service Lifetimes

Let’s say we have different notification services, and we need to manage their lifecycle in the DI container.

```csharp
public interface INotificationService  
{  
    void Send(string message);  
}  
  
internal class EmailNotificationService : INotificationService  
{  
    public void Send(string message)  
    {  
        Console.WriteLine($"Email: {message}");  
    }  
}  
  
internal class SmsNotificationService : INotificationService  
{  
    public void Send(string message)  
    {  
        Console.WriteLine($"SMS: {message}");  
    }  
}  
  
public class NotificationController  
{  
    private readonly INotificationService _emailService;  
    private readonly INotificationService _smsService;  
  
    // Constructor injection with different lifetimes  
    public NotificationController(INotificationService emailService, INotificationService smsService)  
    {  
        _emailService = emailService;  
        _smsService = smsService;  
    }  
  
    public void ProcessEmail(string message)  
    {  
        _emailService.Send(message);  
    }  
  
    public void ProcessSms(string message)  
    {  
        _smsService.Send(message);  
    }  
}  
  
// In Program.cs or Startup.cs  
// Assuming 'builder' is an instance of WebApplicationBuilder or similar
// For demonstration, these registrations would typically be for different concrete types
// or named services if both were INotificationService.
// Here, the example implies two distinct services are being registered with different lifetimes.
// For a real scenario, you might register:
// builder.Services.AddSingleton<IEmailNotificationService, EmailNotificationService>();
// builder.Services.AddTransient<ISmsNotificationService, SmsNotificationService>();
// And then inject those specific interfaces.
// The provided code snippet has a slight ambiguity if both registrations are for INotificationService,
// as the last one would override the first for that interface.
// However, for the purpose of demonstrating lifetimes, it illustrates the syntax.
// Let's assume for the article's intent that these are distinct registrations for different purposes
// or that the example is simplified to show the AddSingleton/AddTransient calls.
// For the sake of clarity in the improved content, I'll keep the original code as is,
// but note the potential ambiguity if both were for the same interface without further distinction.
builder.Services.AddSingleton<INotificationService, EmailNotificationService>(); // Singleton  
builder.Services.AddTransient<INotificationService, SmsNotificationService>();   // Transient
```

In this case, we have an `EmailNotificationService` that uses the `Singleton` lifetime because we want to maintain a single instance for the entire application, while the `SmsNotificationService` is transient because we want a new instance for every request.

## IoC with Strategy Pattern and Access Modifiers

**Inversion of Control (IoC)** is the broader principle behind DI. The idea is that instead of an object controlling its own dependencies, a central container or factory does it. Here’s an advanced IoC example, where we combine **DI, IoC, access control, and design patterns**.

**Example 2: IoC with Strategy and Factory Patterns**

```csharp
public interface INotificationService  
{  
    void Send(string message);  
}  
  
internal class EmailNotificationService : INotificationService  
{  
    public void Send(string message)  
    {  
        Console.WriteLine($"Sending email: {message}");  
    }  
}  
  
internal class SmsNotificationService : INotificationService  
{  
    public void Send(string message)  
    {  
        Console.WriteLine($"Sending SMS: {message}");  
    }  
}  
  
// Abstract Factory pattern for creating notification services  
public interface INotificationFactory  
{  
    INotificationService CreateNotificationService();  
}  
  
public class EmailNotificationFactory : INotificationFactory  
{  
    public INotificationService CreateNotificationService()  
    {  
        return new EmailNotificationService();  
    }  
}  
  
public class SmsNotificationFactory : INotificationFactory  
{  
    public INotificationService CreateNotificationService()  
    {  
        return new SmsNotificationService();  
    }  
}  
  
// IoC container manages the factories and injects dependencies  
public class NotificationHandler  
{  
    private readonly INotificationFactory _factory;  
  
    public NotificationHandler(INotificationFactory factory)  
    {  
        _factory = factory;  
    }  
  
    public void Process(string message)  
    {  
        var service = _factory.CreateNotificationService();  
        service.Send(message);  
    }  
}  
  
// Program.cs or Startup.cs  
var builder = WebApplication.CreateBuilder(args);  
  
// IoC container manages the lifecycle of the factories  
builder.Services.AddSingleton<INotificationFactory, EmailNotificationFactory>(); // Can swap for SmsNotificationFactory  
builder.Services.AddSingleton<NotificationHandler>();  
  
var app = builder.Build();
```

In this example, we’ve combined the **Factory Pattern** and **IoC** principles. `NotificationHandler` depends on `INotificationFactory`, which provides the flexibility to create different notification services. The **IoC container** (in this case, the .NET DI container) manages the factory, ensuring we can easily swap implementations.

## Conclusion

Leveraging **Dependency Injection**, **Inversion of Control**, and **Design Patterns** such as **Factory** and **Strategy** while adhering to **SOLID principles** allows you to build flexible, scalable, and maintainable applications in C# .NET 8. By incorporating access control through **modifiers**, you enhance encapsulation and improve overall security and reliability.

This approach not only adheres to best practices but also prepares your application for future growth and changes with minimal impact.

> I hope this guide helps you take your architecture to the next level. If you have questions or suggestions, feel free to drop a comment!