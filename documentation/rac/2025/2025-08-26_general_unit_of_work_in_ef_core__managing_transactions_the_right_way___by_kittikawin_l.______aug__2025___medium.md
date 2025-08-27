```yaml
---
title: "Unit of Work in EF Core: Managing Transactions the Right Way | by Kittikawin L. 🍀 | Aug, 2025 | Medium"
source: https://medium.com/@kittikawin_ball/unit-of-work-in-ef-core-managing-transactions-the-right-way-6b6ce41d2df4
date_published: 2025-08-26T15:01:45.560Z
date_captured: 2025-09-04T11:42:01.340Z
domain: medium.com
author: Kittikawin L. 🍀
category: general
technologies: [Entity Framework Core, .NET, Dapper]
programming_languages: [C#, SQL]
tags: [unit-of-work, ef-core, transactions, database, data-consistency, orm, repository-pattern, dotnet, architecture, data-access]
key_concepts: [Unit of Work pattern, transactions, data consistency, DbContext, repository-pattern, dependency-injection, explicit-transactions, change-tracking]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores the Unit of Work (UoW) pattern, highlighting its importance in maintaining data consistency by grouping multiple database operations into a single transaction. It clarifies that Entity Framework Core's `DbContext` inherently provides UoW functionality through its change tracker and `SaveChanges()` method. The author then details specific scenarios where implementing a custom UoW is beneficial, such as managing transactions across multiple `DbContexts`, integrating with other data access technologies like Dapper, or enforcing architectural patterns like Domain-Driven Design. Practical C# code examples illustrate both EF Core's default behavior and the implementation of a custom UoW with repositories, along with advanced explicit transaction handling. The article concludes with best practices, advising a custom UoW for complex, layered architectures while relying on `DbContext` for simpler projects.]
---
```

# Unit of Work in EF Core: Managing Transactions the Right Way | by Kittikawin L. 🍀 | Aug, 2025 | Medium

# Unit of Work in EF Core: Managing Transactions the Right Way

![An open hard disk drive (HDD) showing its internal components, including the shiny platter, read/write head, and actuator arm, illuminated with warm, golden light against a dark background.](https://miro.medium.com/v2/resize:fit:700/0*B6ZOaCYMRJWjgLvI)

Photo by [benjamin lehman](https://unsplash.com/@abject?utm_source=medium&utm_medium=referral) on [Unsplash](https://unsplash.com/?utm_source=medium&utm_medium=referral)

When working with databases, it’s not uncommon to have multiple operations that should succeed or fail as a group. For example, creating a new order, updating stock levels, and recording a payment must all happen together. Otherwise, you risk leaving your system in an inconsistent state.

This is where the **Unit of Work (UoW) pattern** shines. In Entity Framework Core, UoW helps you group operations into a _single transaction_ to ensure consistency and reliability.

But the key point is that **EF Core already has built-in Unit of Work.** So when do you actually need to use it yourself? Let’s break it down.

## What Is the Unit of Work Pattern?

The Unit of Work pattern is about **treating multiple changes as a single transaction**. If everything succeeds, commit the changes. If anything fails, roll everything back.

Think of it like a shopping cart — You don’t want to pay for items until all of them are checked out successfully.

### In terms of code.

*   Start a unit of work (transaction)
*   Perform multiple operations
*   Commit all if successful, rollback if not

## How EF Core Implements Unit of Work

Entity Framework Core `DbContext` is essentially a **Unit of Work + Repository** combined.

*   It tracks changes to entities
*   Groups them together
*   Applies them to the database when you call `SaveChanges()`

### So, by default.

```csharp
using var context = new AppDbContext();  
  
var user = new User { Name = "Alice" };  
var order = new Order { UserId = user.Id, Total = 200 };  
context.Users.Add(user);  
context.Orders.Add(order);  
  
// Unit of Work in action: both are saved together  
await context.SaveChangesAsync();
```

Here, EF Core change tracker bundles operations into a single transaction. If one fails, the whole transaction rolls back.

## When Do You Need a Custom Unit of Work?

If `DbContext` already does this, why bother with a custom UoW implementation?

### When you will need it.

*   You want **explicit transaction control** across multiple `DbContexts`.
*   You’re combining EF Core with other data sources (e.g., EF + Dapper).
*   You want a **clear abstraction** to enforce consistency in your architecture (e.g., DDD).
*   You need a testable pattern for mocking repositories and transactions.

## Implementing Unit of Work in EF Core

### Step 1: Define the Interface

```csharp
public interface IUnitOfWork : IDisposable  
{  
    Task<int> CommitAsync();  
}
```

### Step 2: Implement It with EF Core

```csharp
public class UnitOfWork : IUnitOfWork  
{  
    private readonly AppDbContext _context;  
    public UnitOfWork(AppDbContext context)  
    {  
        _context = context;  
    }  
    public async Task<int> CommitAsync()  
    {  
        return await _context.SaveChangesAsync();  
    }  
    public void Dispose()  
    {  
        _context.Dispose();  
    }  
}
```

### Step 3: Use It with Repositories

```csharp
public class OrderService  
{  
    private readonly IOrderRepository _orders;  
    private readonly IUserRepository _users;  
    private readonly IUnitOfWork _unitOfWork;  
    public OrderService(IOrderRepository orders, IUserRepository users, IUnitOfWork unitOfWork)  
    {  
        _orders = orders;  
        _users = users;  
        _unitOfWork = unitOfWork;  
    }  
    public async Task PlaceOrderAsync(User user, Order order)  
    {  
        _users.Add(user);  
        _orders.Add(order);  
        // Commit both user and order in one UoW  
        await _unitOfWork.CommitAsync();  
    }  
}
```

## Advanced: Handling Explicit Transactions

Sometimes you need even more control, like spanning multiple contexts.

```csharp
using var transaction = await context.Database.BeginTransactionAsync();  
  
try  
{  
    context.Users.Add(new User { Name = "Bob" });  
    await context.SaveChangesAsync();  
  
    context.Orders.Add(new Order { UserId = 1, Total = 300 });  
    await context.SaveChangesAsync();  
  
    await transaction.CommitAsync();  
}  
catch  
{  
    await transaction.RollbackAsync();  
    throw;  
}
```

This ensures both operations succeed or none at all.

## Best Practices

*   **Don’t over-abstract.**  
    EF Core already acts like a UoW — only add custom UoW if you need it.
*   **Use DI with `Scoped` lifetime**  
    For `DbContext` and UoW.
*   **Keep it lightweight.**  
    Avoid bloated UoW interfaces with too many responsibilities.
*   **Use explicit transactions**  
    For multi-DbContext or cross-database operations.
*   **Test with mocks.**  
    UoW abstraction makes it easier to unit test services.

## Conclusion

The Unit of Work pattern ensures **data consistency and transactional integrity.** Something every scalable system needs. With EF Core, you already get this through `DbContext`.

However, in **complex applications** (especially with multiple repositories or data sources), implementing a custom **UoW** gives you more _explicit control_, _better testability_, and _cleaner architecture_.

### In short

*   Small projects → Rely on `DbContext.SaveChanges()`
*   Larger, layered architectures → Consider a custom `Unit of Work`

Either way, understanding how EF Core manages transactions will make your apps more reliable and scalable.