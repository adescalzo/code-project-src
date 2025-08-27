```yaml
---
title: 📦 Simplifying Dependency Injection in .NET Using Scrutor
source: https://newsletter.kanaiyakatarmal.com/p/simplifying-dependency-injection
date_published: 2025-07-08T04:45:44.000Z
date_captured: 2025-08-15T11:19:14.526Z
domain: newsletter.kanaiyakatarmal.com
author: Kanaiya Katarmal
category: ai_ml
technologies: [.NET, Scrutor, Microsoft.Extensions.DependencyInjection, ASP.NET Core, .NET Core 3.1, .NET 5, .NET 6, .NET 7, .NET 8]
programming_languages: [C#]
tags: [dependency-injection, dotnet, scrutor, assembly-scanning, convention-based, clean-code, service-registration, boilerplate-reduction]
key_concepts: [dependency-injection, assembly-scanning, convention-based-registration, decorator-pattern, service-lifetime, inversion-of-control, boilerplate-reduction]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces Scrutor, a lightweight, open-source library that extends the .NET built-in Dependency Injection container. It addresses the verbosity of manual service registration in large .NET applications by enabling automated, convention-based assembly scanning. The guide demonstrates how to install Scrutor and use its features like `FromAssemblyOf`, `AddClasses`, `AsMatchingInterface`, and `WithScopedLifetime` to simplify DI setup. By automating service discovery and registration, Scrutor promotes cleaner code, reduces boilerplate, and enhances maintainability in .NET projects.
---
```

# 📦 Simplifying Dependency Injection in .NET Using Scrutor

# 📦 Simplifying Dependency Injection in .NET Using Scrutor

### Stop Repeating Yourself: Automate DI Registrations with Scrutor

Dependency Injection (DI) is a fundamental design pattern in .NET, enabling clean separation of concerns and better testability. While .NET’s built-in DI container is powerful and easy to use, it can become verbose and repetitive when registering multiple services manually.

Enter **Scrutor** — a lightweight, open-source library that extends the Microsoft DI container with assembly scanning and decoration support.

In this article, we’ll explore how Scrutor simplifies dependency registration and enhances maintainability.

## 🔧 Why Scrutor?

Scrutor builds on top of `Microsoft.Extensions.DependencyInjection`, offering:

*   **Assembly scanning**: Automatically register services by scanning assemblies.
*   **Convention-based registration**: Register services based on naming or interface matching.
*   **Decorator support**: Apply the decorator pattern without extra boilerplate.

## 🔧 Problem

When building a .NET application with many services (e.g., `OrderService`, `CustomerService`, `InvoiceService`), manually registering each one becomes repetitive and error-prone:

```csharp
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IInvoiceService, InvoiceService>();
// ... and so on
```

This boilerplate adds up quickly, especially in large-scale projects.

### ✅ Solution: Assembly Scanning with Scrutor

**Scrutor** is a powerful library that extends the built-in dependency injection system in .NET. It allows you to **automatically register services** using **convention-based scanning**.

The method `.FromAssemblyOf<T>()` makes it super easy to scan an entire assembly and register all services at once.

### 🚀 Getting Started

#### Step 1: Install Scrutor

Use NuGet or the .NET CLI:

```bash
dotnet add package Scrutor
```

#### Step 2: Define Interfaces and Implementations

```csharp
public interface IOrderService
{
    void ProcessOrder();
}

public class OrderService : IOrderService
{
    public void ProcessOrder()
    {
        Console.WriteLine("Order processed.");
    }
}
```

🔁 **Traditional Way (Manual Registration)**

```csharp
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
// etc...
```

✅ **Modern Way (Using Scrutor)**

```csharp
builder.Services.Scan(scan => scan
    .FromAssemblyOf<OrderService>()    // Pick any class from your services assembly
    .AddClasses()                      // Add all public non-abstract classes
    .AsMatchingInterface()             // Match IOrderService -> OrderService
    .WithScopedLifetime());           // Set service lifetime (Scoped here)
```

🔍 What Do These Methods Do?

*   `.FromAssemblyOf<T>()`: Scans the assembly where type `T` (e.g., `OrderService`) is defined.
*   `.AddClasses()`: Finds all public, non-abstract classes.
*   `.AsMatchingInterface()`: Automatically maps `OrderService` to `IOrderService` if they follow naming conventions.
*   `.WithScopedLifetime()`: Sets the lifetime. Other options include:
    *   `.WithSingletonLifetime()`
    *   `.WithTransientLifetime()`

### 🧠 Example Controller Usage

```csharp
[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet("process")]
    public IActionResult ProcessOrder()
    {
        _orderService.ProcessOrder();
        return Ok("Order processed.");
    }
}
```

### 🎯 Benefits

*   ✅ **No repetition**: Avoid writing dozens of `.AddScoped<TInterface, TImplementation>()` lines.
*   ✅ **Auto-discovery**: Automatically picks up new services added to your assembly.
*   ✅ **Cleaner Startup**: Keeps your `Program.cs` or `Startup.cs` concise and maintainable.
*   ✅ **Convention-based**: Encourages consistent naming and architecture.

### 📌 Notes

*   `.FromAssemblyOf<T>()` is supported in **Scrutor 3.0.0+**.
*   Compatible with **.NET Core 3.1**, **.NET 5**, **.NET 6**, **.NET 7**, and **.NET 8**.
*   Make sure your interfaces and implementations follow naming conventions like `IFoo` / `Foo` for `.AsMatchingInterface()` to work.

### 💬 Want More?

You can also filter services using:

```csharp
.AddClasses(classes => classes.Where(type => type.Name.EndsWith("Service")))
```

Or register by base class:

```csharp
.AddClasses().AssignableTo<BaseService>()
```

### 🧼 Final Thoughts

Using `Scrutor` with `.FromAssemblyOf<T>()` simplifies service registration and enforces clean, scalable architecture. It’s especially helpful in large projects with many services.

I hope you found this guide helpful and informative.

Thanks for reading!