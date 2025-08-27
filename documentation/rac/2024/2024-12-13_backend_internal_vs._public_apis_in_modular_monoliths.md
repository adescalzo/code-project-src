```yaml
---
title: Internal vs. Public APIs in Modular Monoliths
source: https://www.milanjovanovic.tech/blog/internal-vs-public-apis-in-modular-monoliths?utm_source=LinkedIn&utm_medium=social&utm_campaign=25.08.2025
date_published: 2024-12-14T00:00:00.000Z
date_captured: 2025-08-28T14:33:07.683Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: backend
technologies: [ASP.NET Core, Entity Framework Core, SQL Server, .NET, GitHub Actions, Dometrain, Depot]
programming_languages: [C#, SQL]
tags: [modular-monolith, api-design, software-architecture, data-isolation, coupling, dotnet, event-driven-architecture, database-design, module-communication, clean-architecture]
key_concepts: [Modular Monolith Architecture, Public APIs, Internal APIs, Module Communication, Data Isolation, Contract Definition, Dependency Control, Event-Driven Patterns]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article clarifies the crucial distinction between internal and public APIs within a modular monolith architecture. It emphasizes that public APIs are essential for defining explicit, controllable coupling points between modules, preventing the chaos of direct database or internal service access. The author provides practical guidance on designing public APIs around specific use cases, not generic CRUD operations, and highlights the importance of data isolation through separate database schemas, connection strings, and read models. The article also touches on handling cross-cutting concerns using event-driven patterns and dedicated query models, ultimately aiming for maintainable and evolvable systems.]
---
```

# Internal vs. Public APIs in Modular Monoliths

![Blog cover image for "Internal vs. Public APIs in Modular Monoliths" with a blue and black geometric design and "MJ Tech" logo.](blog-covers/mnw_120.png?imwidth=3840)

# Internal vs. Public APIs in Modular Monoliths

5 min read · December 14, 2024

Every article about modular monoliths tells you to use public APIs between modules. But they rarely tell you why these APIs exist or how to design them properly.

A modular monolith organizes an application into independent modules that have clear boundaries. The module boundaries are logical and group related business capabilities together.

After building several large-scale modular monoliths, I've learned that public APIs are not just about clean code - they're about controlling chaos. Let me show you what I mean.

## The Reality of Module Communication

Here's what nobody tells you about public APIs in modular monoliths: they represent intentional coupling points. Yes, you read that right. Public APIs don't eliminate coupling - they make it explicit and controllable.

When Module A needs something from Module B, you have three options:

1.  Let Module A read directly from Module B's database
2.  Let Module A access Module B's internal services
3.  Create a public API that explicitly defines what Module A can do

![Diagram illustrating three module communication options: 1) Module A directly accessing Module B's database, 2) Module A accessing Module B's internal services, and 3) Module A communicating with Module B via a Public API.](blogs/mnw_120/module_communication_options.png?imwidth=3840)

The first two options lead to chaos. I've seen entire systems become unmaintainable because every module was freely accessing the data and services of other modules.

The previous options are examples of synchronous communication between modules. But you can also implement [**asynchronous module communication**](modular-monolith-communication-patterns) using messaging. We have to adjust the technical implementation. However, modules still have a public API in message contracts.

## Why We Need Public APIs

Public APIs serve three critical purposes:

1.  **Contract Definition**: They explicitly state what other modules can and cannot do
2.  **Dependency Control**: They force you to think about module dependencies
3.  **Change Management**: They provide a stable interface while allowing internal changes

Here's a practical example. Imagine you have an Orders module and a Shipping module.

This is what you want to avoid:

```csharp
public class ShippingService
{
    private readonly OrdersDbContext _ordersDb; // Direct database access

    public async Task ShipOrder(string orderId)
    {
        // Directly reading from another module's database
        var order = await _ordersDb.Orders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == orderId);

        // What happens if the Orders module changes its schema?
        // What if it moves to a different database?
    }
}
```

This is what you want to achieve instead:

```csharp
public class ShippingService
{
    private readonly IOrdersModule _orders; // Public API access

    public async Task ShipOrder(string orderId)
    {
        // Using the public API
        var order = await _orders.GetOrderForShippingAsync(orderId);

        // The Orders module can change its internals
        // as long as it maintains this contract
    }
}
```

## Controlling What Gets Exposed

The hardest part of designing public APIs is deciding what to expose. Here's my rule of thumb:

1.  Start with nothing public
2.  Expose only what other modules actually need
3.  Design the API around use cases, not data

Here's how this looks in practice:

```csharp
public interface IOrdersModule
{
    // Don't expose generic CRUD operations
    // Task<Order> GetOrderAsync(string orderId); // Bad

    // Instead, expose specific use cases
    Task<OrderShippingInfo> GetOrderForShippingAsync(string orderId);
    Task<OrderPaymentInfo> GetOrderForPaymentAsync(string orderId);
    Task<OrderSummary> GetOrderForCustomerAsync(string orderId);
}
```

## Protecting Your Module's Data

Public APIs aren't enough. You also need to [**protect your module's data**](modular-monolith-data-isolation). Here's what I've found works:

1.  **Separate Schemas**: Each module gets its own database schema.

```sql
CREATE SCHEMA Orders;
CREATE SCHEMA Shipping;

-- Orders module can only access its schema
CREATE USER OrdersUser WITH DEFAULT_SCHEMA = Orders;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Orders TO OrdersUser;

-- Shipping module can only access its schema
CREATE USER ShippingUser WITH DEFAULT_SCHEMA = Shipping;
GRANT SELECT, INSERT, UPDATE, DELETE ON SCHEMA::Shipping TO ShippingUser;
```

We can also lock down the user's access to a given schema to only allow reading and writing data.

2.  **Different Connection Strings**: Each module gets its own database user with a respective connection string.

```csharp
builder.Services.AddDbContext<OrdersDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("OrdersConnection")));

builder.Services.AddDbContext<ShippingDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ShippingConnection")));
```

If you want to learn more about this, check out this article about [**using multiple EF Core DbContexts**](using-multiple-ef-core-dbcontext-in-single-application).

3.  **Read Models**: Create specific read models for other modules.

```csharp
internal class Order
{
    // Internal domain model with full complexity
}

public class OrderShippingInfo
{
    // Public DTO with only what shipping needs
    public string OrderId { get; init; }
    public Address ShippingAddress { get; init; }
    public List<ShippingItem> Items { get; init; }
}
```

## Dealing with Cross-Cutting Concerns

Some features naturally span multiple modules. For example, when a customer views their order history, you might need data from the Orders, Shipping, and Payments modules.

Don't try to force this through module APIs. Instead:

1.  Create a separate query model
2.  Use event-driven patterns to keep it updated
3.  Own it in a dedicated module or one of the existing modules

```csharp
public class OrderHistoryModule
{
    public async Task<CustomerOrderHistory> GetOrderHistoryAsync(string customerId)
    {
        // Read from a dedicated read model that's kept
        // updated through events from other modules
        return await _orderHistoryRepository.GetCustomerHistoryAsync(customerId);
    }
}
```

## Summary

Public APIs in modular monoliths are not about preventing coupling - they're about controlling it. Every public API is a contract that says: "Yes, these modules are coupled, and this is exactly how they depend on each other."

The goal isn't to eliminate dependencies between modules. The goal is to make them explicit, controlled, and maintainable.

Get this right, and your modular monolith will be easier to maintain, test, and evolve. Get it wrong, and you'll end up with a distributed big ball of mud.

Want to master building modular monoliths with clean APIs and event-driven patterns? Check out my [**Modular Monolith Architecture**](/modular-monolith-architecture) course, where I'll show you how to build maintainable systems using practical examples from real projects.

That's all for today. Stay awesome, and I'll see you next week.