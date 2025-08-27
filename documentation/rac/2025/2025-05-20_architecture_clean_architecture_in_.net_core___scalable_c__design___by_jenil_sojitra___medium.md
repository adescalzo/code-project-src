```yaml
---
title: "Clean Architecture in .NET Core | Scalable C# Design | by Jenil Sojitra | Medium"
source: https://medium.com/@jenilsojitra/clean-architecture-in-net-core-e18b4ad229c8
date_published: 2025-05-21T00:32:32.844Z
date_captured: 2025-09-05T12:37:07.660Z
domain: medium.com
author: Jenil Sojitra
category: architecture
technologies: [ASP.NET Core, .NET 8, Entity Framework Core, Dapper, MediatR, AutoMapper, FluentValidation, Blazor, MVC, Web API]
programming_languages: [C#, SQL]
tags: [clean-architecture, dotnet, csharp, software-design, architecture, web-api, scalability, testability, maintainability, design-patterns]
key_concepts: [Clean Architecture, Separation of Concerns, Layered Architecture, Dependency Inversion Principle, Repository Pattern, CQRS, Domain-Driven Design, Testability]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces Clean Architecture, a software design philosophy by Robert C. Martin, emphasizing separation of concerns, testability, and maintainability in .NET Core and .NET 8+ applications. It details the layered structure, including Domain, Application, Infrastructure, and Presentation layers, explaining their roles in building scalable systems. Key principles like Dependency Inversion and patterns such as Repository and CQRS are discussed with practical C# code examples. The content also covers best practices, common pitfalls, and modern tools like EF Core, MediatR, and AutoMapper for implementing Clean Architecture. It concludes by recommending Clean Architecture for large, complex enterprise applications while advising against its use for simple projects.
---
```

# Clean Architecture in .NET Core | Scalable C# Design | by Jenil Sojitra | Medium

# Clean Architecture in .NET Core | Scalable C# Design

![A stylized graphic featuring a large, faceted crystal-like shape in the center, predominantly green and purple, with the text "C#" prominently displayed in white. In the background, there are abstract lines resembling code snippets and smaller, colorful geometric cubes. The overall color scheme is green, suggesting a tech or programming theme.](https://miro.medium.com/v2/resize:fit:700/1*HOUJvlhVqmM1jC1g0KsIgg.jpeg)

## 1. Introduction

### What is Clean Architecture?

Introduced by Robert C. Martin (Uncle Bob), clean architecture is a software design philosophy emphasizing the **separation of concerns**, **testability**, and **maintainability**. It organizes a system into layers with clear boundaries, ensuring that **business logic remains independent** of frameworks, databases, or UI.

### Why It Matters in Modern .NET Development?

In **.NET Core** and **.NET 8+**, applications must be **scalable**, **testable**, and **adaptable** to changing business needs. Clean Architecture helps by:

*   **Decoupling** business rules from infrastructure.
*   Making **testing easier** (unit, integration, and end-to-end).
*   **Reducing technical debt** by enforcing clear boundaries.

### Real-World Benefits

1.  **Scalability** — Easy to extend without breaking existing code.
2.  **Testability** — Business logic can be tested in isolation.
3.  **Maintainability** — Clear structure reduces spaghetti code.

## 2. Core Principles of Clean Architecture

1.  **Independence of Frameworks** — Avoid tight coupling with ASP.NET Core, Entity Framework, etc.
2.  **Testability at All Levels** — Business logic should be testable without databases or UI.
3.  **UI & Database as Plugins** — External concerns (like APIs & DBs) depend on core logic, not vice versa.
4.  **Business Logic at the Center** — The **domain layer** is the heart of the application.

## 3. Layered Structure in Clean Architecture

### 1. Domain Layer (Entities)

*   Contains **business models** and **rules**.
*   Pure C# classes with **no external dependencies**.

### 2. Application Layer (Use Cases)

*   Defines **application logic** (e.g., CQRS, MediatR).
*   Contains **interfaces** for repositories and services.

### 3. Infrastructure Layer

*   Implements **external concerns**:
    *   **Database (EF Core, Dapper)**
    *   **APIs, Email, File Storage**
*   Depends on the **Application Layer**, not the other way around.

### 4. Presentation Layer (UI)

*   **ASP.NET Core Web API / MVC / Blazor**
*   Minimal logic; delegates work to the **Application Layer**.

📌 **Visual Diagram:**

*   Presentation Layer (API/UI)
    ↓
*   Application Layer (Use Cases)
    ↓
*   Domain Layer (Entities & Business Rules)
    ↑
*   Infrastructure Layer (DB, External Services)

## 4. Implementing Clean Architecture in .NET

### Project Structure

*   **MyApp.Domain** (Entities & Business Rules)
*   **MyApp.Application** (Use Cases, Interfaces)
*   **MyApp.Infrastructure** (EF Core, External Services)
*   **MyApp.WebApi** (ASP.NET Core Controllers)

### Folder-by-Feature vs. Layer-by-Layer

*   **Layer-by-Layer**: Traditional, but can lead to bloated layers.
*   **Folder-by-Feature**: Better for large apps (e.g., `Users/`, `Orders/`).

### Example: ASP.NET Core Web API

```csharp
// Domain Layer (No dependencies)
public class Product    
{  
    public int Id { get; set; }    
    public string Name { get; set; }    
}  
  
// Application Layer (Interfaces)  
public interface IProductRepository    
{  
    Task<Product> GetByIdAsync(int id);    
}  
  
// Infrastructure Layer (EF Core Implementation)  
public class ProductRepository : IProductRepository    
{  
    public async Task<Product> GetByIdAsync(int id)    
    {  
        // DB logic here    
        return await Task.FromResult(new Product { Id = id, Name = $"Product {id}" }); // Example
    }    
}  
  
// Web API (Minimal Logic)  
[ApiController]    
[Route("api/[controller]")]
public class ProductsController : ControllerBase    
{  
    private readonly IProductRepository _repo;    
    public ProductsController(IProductRepository repo) => _repo = repo;    
    
    [HttpGet("{id}")]    
    public async Task<IActionResult> Get(int id) => Ok(await _repo.GetByIdAsync(id));    
}
```

## 5. Dependency Inversion & Interfaces

### Key Concept: DIP (Dependency Inversion Principle)

*   **High-level modules** (Domain) should not depend on **low-level modules** (Infrastructure).
*   Both should depend on **abstractions (interfaces)**.

### Example: Repository Pattern + DI

```csharp
// In Application Layer  
public interface IOrderRepository    
{  
    Task<Order> GetOrderAsync(int id);    
}  
  
// In Infrastructure Layer  
public class OrderRepository : IOrderRepository    
{  
    // EF Core implementation    
    public Task<Order> GetOrderAsync(int id)
    {
        // Example implementation
        return Task.FromResult(new Order { Id = id, CustomerName = "Example Customer" });
    }
}  
  
// In Startup.cs (Dependency Injection)  
// Assuming 'services' is an IServiceCollection
services.AddScoped<IOrderRepository, OrderRepository>();
```

## 6. Best Practices for Clean Architecture in .NET

*   **Keep Controllers Thin** — Only handle HTTP, delegate logic to **Application Layer**.
*   **Avoid Logic in Infrastructure** — Just implementations, no business rules.
*   **Enforce Boundaries** — Use **interfaces** to prevent layer leakage.
*   **Automate Testing** — Unit tests for **Domain**, Integration for **Infrastructure**.

## 7. Common Pitfalls to Avoid

*   **Over-Engineering Small Projects** — Not every app needs Clean Architecture.
*   **Leaking Dependencies** — Don’t reference **EF Core** in the **Domain Layer**.
*   **Circular References** — Ensure one-way dependencies (Domain ← Application ← Infrastructure ← UI).

## 8. Clean Architecture with Modern Tools

*   **Entity Framework Core** — Use **Repository Pattern** to abstract DB access.
*   **MediatR** — Decouple commands/queries (CQRS).
*   **AutoMapper** — Simplify DTO mappings.
*   **FluentValidation** — Keep validation logic clean.

### Popular .NET Clean Architecture Templates

*   [**Jason Taylor’s Clean Architecture Template**](https://github.com/jasontaylordev/CleanArchitecture)
*   [**Ardalis’ Clean Architecture Solution**](https://github.com/ardalis/cleanarchitecture)
*   [**Jenil’s Clean Architecture Demo**](https://github.com/JenilSojitra/CleanArchitectureDemo)

## 9. Sample GitHub Repositories & Resources

1.  **Books:**

*   “Clean Architecture” by Robert C. Martin
*   “Domain-Driven Design” by Eric Evans

2.  **Courses:**

*   [Pluralsight: Clean Architecture in .NET](https://www.pluralsight.com/)
*   [Udemy: Clean Architecture with .NET Core](https://www.udemy.com/)

## 10. Conclusion

Clean Architecture is **ideal for large, complex .NET applications** where **scalability, testability, and maintainability** are crucial.

*   **Use It For:** Enterprise apps, long-term projects, and teams.
*   **Avoid For:** Simple CRUD apps, prototypes.

By following **Clean Architecture in .NET**, you ensure a **future-proof**, **testable**, and **well-structured** application.