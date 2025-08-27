```yaml
---
title: "Clean Architecture in .NET 9: A Modern Approach to Building Scalable Apps | by Md. Mahamudul Hasan Tonoy | Jul, 2025 | Medium"
source: https://medium.com/@tonoy300/clean-architecture-in-net-9-a-modern-approach-to-building-scalable-apps-6405ebdfddc6
date_published: 2025-07-12T15:38:25.764Z
date_captured: 2025-08-13T11:19:25.980Z
domain: medium.com
author: Md. Mahamudul Hasan Tonoy
category: architecture
technologies: [.NET 9, ASP.NET Core, Entity Framework Core 9, SQL Server, MediatR, Blazor, MVC]
programming_languages: [C#, SQL]
tags: [clean-architecture, dotnet, web-api, scalability, design-patterns, software-architecture, entity-framework-core, minimal-apis, maintainability, cqrs]
key_concepts: [clean-architecture, separation-of-concerns, dependency-rule, repository-pattern, cqrs, dependency-injection, minimal-apis, aot-compilation]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces Clean Architecture, a structured approach for building scalable and testable applications, and demonstrates its implementation using .NET 9. It outlines the four core layers: Domain, Application, Infrastructure, and Presentation, explaining their roles. The post provides a practical step-by-step guide to setting up a Task Management System, showcasing code examples for each layer, including the use of Entity Framework Core 9 and MediatR with ASP.NET Core Minimal APIs. The author emphasizes the benefits of Clean Architecture, such as improved maintainability, testability, and future-proofing, highlighting how .NET 9's features like AOT compilation further enhance performance.
---
```

# Clean Architecture in .NET 9: A Modern Approach to Building Scalable Apps | by Md. Mahamudul Hasan Tonoy | Jul, 2025 | Medium

# Clean Architecture in .NET 9: A Modern Approach to Building Scalable Apps

As software systems grow in complexity, maintaining a clean, scalable, and testable architecture becomes crucial. Clean Architecture, introduced by Robert C. Martin (Uncle Bob), provides a structured way to design applications by enforcing the separation of concerns.

With .NET 9, Microsoft continues to enhance developer productivity, performance, and cross-platform capabilities. In this post, we’ll explore how to implement Clean Architecture in .NET 9, leveraging its latest features to build maintainable and future-proof applications.

![Diagram illustrating Clean Architecture layers (Core, Application, Infrastructure) with .NET 9, showing elements like servers, clouds, buildings, and a monitor, all under the title "A Modern Approach to Building Scalable Apps".](https://miro.medium.com/v2/resize:fit:700/1*09Iet6p7qf_OOSUZMFOoOg.jpeg)

# Layers of Clean Architecture

1.  **Domain Layer:** Contains business logic and entities (`User`, `Order`)
2.  **Application Layer:** Defines use cases and interfaces (`IUserRepository`)
3.  **Infrastructure Layer:** Implements external concerns (databases, APIs)
4.  **Presentation Layer:** Handles user interaction (Web API, Blazor, MVC)

# Implementing Clean Architecture in .NET 9

Let’s build a simple Task Management System using Clean Architecture in .NET 9.

# 1. Setting Up the Project Structure

Create a new solution with these projects:

```bash
dotnet new sln -n TaskManager  
dotnet new classlib -n TaskManager.Domain  
dotnet new classlib -n TaskManager.Application  
dotnet new classlib -n TaskManager.Infrastructure  
dotnet new webapi -n TaskManager.Api
```

# 2. Domain Layer (Core Business Logic)

Define entities and business rules:

```csharp
// TaskManager.Domain/Entities/TaskItem.cs  
public class TaskItem  
{  
    public Guid Id { get; set; }  
    public string Title { get; set; }  
    public bool IsCompleted { get; set; }  
}
```

# 3. Application Layer (Use Cases & Interfaces)

Define commands, queries, and interfaces:

```csharp
// TaskManager.Application/Interfaces/ITaskRepository.cs  
public interface ITaskRepository  
{  
    Task<TaskItem> GetByIdAsync(Guid id);  
    Task AddAsync(TaskItem task);  
}
```

# 4. Infrastructure Layer (Persistence & External Services)

Implement repositories using Entity Framework Core (EF Core 9):

```csharp
// TaskManager.Infrastructure/Repositories/TaskRepository.cs  
public class TaskRepository : ITaskRepository  
{  
    private readonly AppDbContext _context;  
    public TaskRepository(AppDbContext context) => _context = context;  
  
    public async Task<TaskItem> GetByIdAsync(Guid id)   
            => await _context.Tasks.FindAsync(id);  
}
```

# 5. Presentation Layer (Web API in .NET 9)

Configure Minimal APIs in `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);  
  
// Add MediatR for CQRS  
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(GetTaskByIdQuery).Assembly));  
  
// Register Infrastructure (EF Core, Repositories)  
builder.Services.AddDbContext<AppDbContext>(options =>   
    options.UseSqlServer(builder.Configuration.GetConnectionString("Default")));  
builder.Services.AddScoped<ITaskRepository, TaskRepository>();  
var app = builder.Build();  
  
// API Endpoints  
app.MapGet("/tasks/{id}", async (Guid id, IMediator mediator) =>   
    await mediator.Send(new GetTaskByIdQuery(id)));  
app.Run();
```

# Why Use Clean Architecture in .NET 9?

1.  **Improved Maintainability:** Clear separation makes code easier to modify.
2.  **Better Testability:** Mock dependencies effortlessly.
3.  **Future-Proof:** Swap databases, UIs, or frameworks without breaking core logic.
4.  **Performance Optimizations:** .NET 9 brings AOT compilation, enhanced minimal APIs, and faster EF Core queries.

Clean Architecture in .NET 9 provides a robust foundation for building scalable and maintainable applications. By following the Dependency Rule and structuring your solution into clear layers, you ensure long-term flexibility and testability.