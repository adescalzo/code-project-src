```yaml
---
title: "Mastering the Decorator Pattern in C# .NET 8: Advanced Use Cases and Best Practices. | by Anderson Godoy | Medium"
source: https://medium.com/@anderson.buenogod/mastering-the-decorator-pattern-in-c-net-8-advanced-use-cases-and-best-practices-378974abe9be
date_published: 2024-12-22T11:40:41.522Z
date_captured: 2025-08-06T17:40:08.582Z
domain: medium.com
author: Anderson Godoy
category: programming
technologies: [.NET 8, ASP.NET Core, Polly, Application Insights, OpenTelemetry]
programming_languages: [C#]
tags: [design-patterns, decorator-pattern, csharp, dotnet, software-architecture, dependency-injection, logging, validation, authorization, resilience]
key_concepts: [Decorator Pattern, Design Patterns, Open/Closed Principle, Separation of Concerns, Dependency Injection, Resilience, Telemetry, Observability]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to the Decorator Pattern in C# .NET 8, a structural design pattern for dynamically extending object functionality. It covers the pattern's core principles, basic structure, and practical applications such as logging, input validation, and authorization. The content further delves into advanced integrations, demonstrating how to combine decorators with Dependency Injection, enhance resilience using Polly, and implement telemetry with tools like Application Insights. The author concludes with best practices to ensure robust, scalable, and maintainable implementations of the Decorator Pattern in modern software development.
---
```

# Mastering the Decorator Pattern in C# .NET 8: Advanced Use Cases and Best Practices. | by Anderson Godoy | Medium

An abstract, illustrative image depicting a complex control panel or workbench. The central element is a laptop displaying a circular interface with various data points and graphs. Surrounding the laptop are numerous mechanical components, spools of thread, rulers, and tools like wrenches and pliers, all arranged on a wooden surface. A small, stylized figure appears to be working on one of the components, suggesting a process of construction, customization, or intricate engineering. The overall aesthetic is detailed and somewhat fantastical, conveying the idea of building or enhancing systems.

![](https://miro.medium.com/v2/resize:fit:700/0*yv8V8nvEMw3ZBnou)

Member-only story

# Mastering the Decorator Pattern in C# .NET 8: Advanced Use Cases and Best Practices.

[

![Anderson Godoy](https://miro.medium.com/v2/resize:fill:64:64/1*0NVGVLLAljoMkq7cw9DopQ@2x.jpeg)

](/@anderson.buenogod?source=post_page---byline--378974abe9be---------------------------------------)

[Anderson Godoy](/@anderson.buenogod?source=post_page---byline--378974abe9be---------------------------------------)

Follow

4 min read

·

Dec 22, 2024

66

2

Listen

Share

More

# What is the Decorator Pattern?

The **Decorator Pattern** is a structural design pattern that allows you to dynamically add responsibilities or behaviors to objects without modifying their existing code. It achieves this by “wrapping” the object with additional layers, each layer adding its own functionality.

## Key Features of the Decorator Pattern

1.  **Dynamic Behavior**: Add or remove functionalities at runtime.
2.  **Reusability**: Separate functionalities into reusable components.
3.  **Open/Closed Principle**: Objects remain open for extension but closed for modification.
4.  **Separation of Concerns**: Isolate responsibilities into individual decorators, making the code cleaner and easier to maintain.

## Where is it Useful?

*   Adding logging to specific operations.
*   Validating user input dynamically.
*   Implementing security mechanisms like authentication and authorization.
*   Enhancing functionality without creating a tangled inheritance hierarchy.

# Basic Structure of the Decorator Pattern

The Decorator Pattern is built on three main components:

*   **Base Interface**: Defines the common functionality.
*   **Concrete Component**: Implements the base interface and represents the core functionality.
*   **Decorators**: Wrap the concrete component to add or extend behaviors dynamically.

```csharp
// Base Interface  
public interface IRequestHandler  
{  
    void Handle(string request);  
}  
  
// Concrete Component  
public class ConcreteHandler : IRequestHandler  
{  
    public void Handle(string request)  
    {  
        Console.WriteLine($"Processing request: {request}");  
    }  
}  
  
// Base Decorator  
public abstract class HandlerDecorator : IRequestHandler  
{  
    private readonly IRequestHandler _next;  
  
    protected HandlerDecorator(IRequestHandler next)  
    {  
        _next = next;  
    }  
  
    public virtual void Handle(string request)  
    {  
        _next.Handle(request);  
    }  
}
```

This basic structure allows you to dynamically compose additional functionalities by “wrapping” a concrete component with one or more decorators.

# Real-World Use Cases

## 1\. Dynamic Logging

Adding logs to track operations is a common requirement in modern applications. Using the Decorator Pattern, we can dynamically add logging to specific parts of the system.

```csharp
public class LoggingDecorator : HandlerDecorator  
{  
    public LoggingDecorator(IRequestHandler next) : base(next) { }  
  
    public override void Handle(string request)  
    {  
        Console.WriteLine($"[Log]: Starting request: {request}");  
        base.Handle(request);  
        Console.WriteLine($"[Log]: Finished request: {request}");  
    }  
}
```

**Usage**:

```csharp
IRequestHandler handler = new LoggingDecorator(new ConcreteHandler());  
handler.Handle("Sample request");
```

**Output**:

```
[Log]: Starting request: Sample request  
Processing request: Sample request  
[Log]: Finished request: Sample request
```

## 2\. Input Validation

Validating user input is another practical use case. A decorator can ensure the input is valid before proceeding with the operation.

```csharp
public class ValidationDecorator : HandlerDecorator  
{  
    public ValidationDecorator(IRequestHandler next) : base(next) { }  
  
    public override void Handle(string request)  
    {  
        if (string.IsNullOrWhiteSpace(request))  
        {  
            Console.WriteLine("Error: Request cannot be empty.");  
            return;  
        }  
        base.Handle(request);  
    }  
}
```

**Usage**:

```csharp
IRequestHandler handler = new ValidationDecorator(new ConcreteHandler());  
handler.Handle(""); // Invalid request  
handler.Handle("Valid request"); // Valid request
```

**Output**:

```
Error: Request cannot be empty.  
Processing request: Valid request
```

## 3\. Authorization

Adding authorization checks to restrict access is critical in secure systems.

```csharp
public class AuthorizationDecorator : HandlerDecorator  
{  
    private readonly bool _isAuthorized;  
  
    public AuthorizationDecorator(IRequestHandler next, bool isAuthorized) : base(next)  
    {  
        _isAuthorized = isAuthorized;  
    }  
  
    public override void Handle(string request)  
    {  
        if (!_isAuthorized)  
        {  
            Console.WriteLine("Error: Unauthorized access.");  
            return;  
        }  
        base.Handle(request);  
    }  
}
```

**Usage**:

```csharp
IRequestHandler handler = new AuthorizationDecorator(new ConcreteHandler(), isAuthorized: false);  
handler.Handle("Restricted request");
```

**Output**:

```
Error: Unauthorized access.
```

## 4\. Combining Decorators

Decorators can be stacked to handle multiple responsibilities in sequence.

```csharp
IRequestHandler handler = new ConcreteHandler();  
  
// Adding multiple decorators  
handler = new AuthorizationDecorator(handler, isAuthorized: true);  
handler = new ValidationDecorator(handler);  
handler = new LoggingDecorator(handler);  
  
handler.Handle("Combined request");
```

**Output**:

```
[Log]: Starting request: Combined request  
Processing request: Combined request  
[Log]: Finished request: Combined request
```

# Advanced Enhancements

## 1\. Dependency Injection (DI) Integration

Using DI, you can dynamically inject and configure decorators based on runtime conditions.

```csharp
var builder = WebApplication.CreateBuilder(args);  
  
builder.Services.AddTransient<IRequestHandler, ConcreteHandler>();  
builder.Services.Decorate<IRequestHandler, LoggingDecorator>();  
builder.Services.Decorate<IRequestHandler, ValidationDecorator>();  
  
var app = builder.Build();  
  
app.MapGet("/", (IRequestHandler handler) =>  
{  
    handler.Handle("Request via DI");  
    return Results.Ok();  
});  
  
app.Run();
```

## 2\. Resilience with Polly

Enhance your decorators with retry logic or circuit breakers using the **Polly** library.

```csharp
public class ResilientDecorator : HandlerDecorator  
{  
    private readonly AsyncPolicy _policy;  
  
    public ResilientDecorator(IRequestHandler next) : base(next)  
    {  
        _policy = Policy  
            .Handle<Exception>()  
            .RetryAsync(3, (exception, retryCount) =>  
            {  
                Console.WriteLine($"Retry {retryCount} due to {exception.Message}");  
            });  
    }  
  
    public override async Task Handle(string request)  
    {  
        await _policy.ExecuteAsync(() => Task.Run(() => base.Handle(request)));  
    }  
}
```

## 3\. Telemetry and Observability

Use decorators to integrate telemetry tools like **Application Insights** or **OpenTelemetry** for detailed logging and performance monitoring.

```csharp
public class TelemetryDecorator : HandlerDecorator  
{  
    private readonly TelemetryClient _telemetryClient;  
  
    public TelemetryDecorator(IRequestHandler next, TelemetryClient telemetryClient) : base(next)  
    {  
        _telemetryClient = telemetryClient;  
    }  
  
    public override void Handle(string request)  
    {  
        var startTime = DateTime.UtcNow;  
        _telemetryClient.TrackEvent("Request Started");  
        base.Handle(request);  
        var duration = DateTime.UtcNow - startTime;  
        _telemetryClient.TrackMetric("RequestDuration", duration.TotalMilliseconds);  
    }  
}
```

# Best Practices

1.  **Single Responsibility**: Each decorator should have a focused responsibility.
2.  **Avoid Overuse**: Too many decorators can complicate the execution flow.
3.  **Combine Patterns**: Use with **Chain of Responsibility** or **Pipeline** for advanced workflows.
4.  **Test Independently**: Write unit tests for each decorator to ensure correctness.
5.  **Leverage DI**: Use DI for dynamic composition and easier maintenance.

# Conclusion

The **Decorator Pattern** is a versatile design pattern that can solve common software development challenges like logging, validation, and security. By integrating advanced features such as **Dependency Injection**, **resilience with Polly**, and **telemetry**, you can make your implementations robust, scalable, and future-proof.

> Start applying these enhancements in your projects today, and transform your code into a modular, maintainable, and efficient system.

> If this article helped you, don’t forget to **clap**, **share**, and **comment**! Follow me for more insights on **C# .NET 8**, design patterns, and best practices in software development. 🚀