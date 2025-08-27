```yaml
---
title: Implementing Unit of Work Pattern in EF Core
source: https://antondevtips.com/blog/implementing-unit-of-work-pattern-in-ef-core
date_published: 2024-09-10T11:00:14.891Z
date_captured: 2025-08-06T17:28:16.966Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [Entity Framework Core, .NET, DbContext]
programming_languages: [C#]
tags: [unit-of-work, ef-core, design-patterns, data-access, dotnet, repository-pattern, clean-architecture, vertical-slices, domain-driven-design, transaction-management]
key_concepts: [unit-of-work-pattern, repository-pattern, clean-architecture, vertical-slice-architecture, domain-driven-design, data-consistency, dependency-injection, change-tracking]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explains the Unit of Work (UoW) pattern and its implementation in Entity Framework Core (EF Core). It highlights how UoW manages multiple database operations as a single atomic transaction, ensuring data consistency, especially when dealing with multiple repositories. The post demonstrates implementing UoW by wrapping or directly using EF Core's DbContext, showcasing how to delegate `SaveChangesAsync` calls. It provides practical C# code examples for entities, repositories, and a service handler in a shipping application context, illustrating how to coordinate operations across customer, order, and shipment entities.
---
```

# Implementing Unit of Work Pattern in EF Core

![A dark blue background with abstract purple shapes. In the bottom left, a white square icon with a black angle bracket symbol (`</>`) is displayed, with the text "dev tips" below it. To the right, the white text "IMPLEMENTING WORK PATTERN CORE" is partially visible, suggesting the full title "Implementing Unit of Work Pattern in EF Core".](https://antondevtips.com/media/covers/efcore/cover_ef_unit_of_work.png)

# Implementing Unit of Work Pattern in EF Core

Sep 10, 2024

The **Unit of Work (UoW)** pattern is a design pattern that allows managing multiple operations that should be treated as a single transaction. In this blog post, we'll explore the Unit of Work pattern and how it can be implemented in Entity Framework Core (EF Core).

In EF Core, the Unit of Work pattern typically wraps the DbContext, providing an abstraction layer that coordinates the work of multiple repositories by collecting their operations into a single transaction. Once all the operations are ready, the Unit of Work commits them to the database in one go, ensuring that either all operations succeeded or none of them. This ensures data consistency.

The Unit of Work pattern is especially useful in scenarios where multiple operations must be completed together. If you have multiple entities, you might have a repository per each entity. Even if entities are related with each other - you shouldn't create a single monstrous repository.

Unit of Work pattern can handle data from multiple repositories ensuring that all changes are persisted together, avoiding partial updates that could leave your data in an inconsistent state.

This pattern also has a good scalability. As your application grows and the number of repositories increases, the Unit of Work pattern helps manage the complexity by coordinating the operations across repositories, ensuring they all participate in the same transaction.

**When to use Unit of Work Pattern:**

*   **Complex Transactions:** when your application performs multiple operations on multiple entities that must be committed as a single transaction.
*   **Multiple Repositories:** if your architecture involves multiple repositories, the Unit of Work pattern can help manage the coordination between them.
*   **Undo or Rollback Support:** if there's a need to undo or roll back changes if something goes wrong during a transaction.

## Implementing the Unit of Work Pattern in EF Core

Today I'll show you how to implement Unit Of Work pattern for a **Shipping Application** that is responsible for creating and updating customers, orders and shipments for ordered products.

This application has the following entities:

*   Customers
*   Orders, OrderItems
*   Shipments, ShipmentItems

I am using Domain Driven Design practices for my entities. Let's explore a few entities:

```csharp
public class Customer
{
    public Guid Id { get; private set; }
    public string FirstName { get; private set; }
    public string LastName { get; private set; }
    public string Email { get; private set; }
    public string PhoneNumber { get; private set; }
    public IReadOnlyList<Order> Orders => _orders.AsReadOnly();

    private readonly List<Order> _orders = [];

    private Customer() { }

    public static Customer Create(
        string firstName,
        string lastName,
        string email,
        string phoneNumber)
    {
        return new Customer
        {
            Id = Guid.NewGuid(),
            FirstName = firstName,
        LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber
        };
    }

    public void AddOrder(Order order)
    {
        _orders.Add(order);
    }
}
```
```csharp
public class Order
{
    public Guid Id { get; private set; }

    public string OrderNumber { get; private set; }

    public Guid CustomerId { get; private set; }

    public Customer Customer { get; private set; }

    public DateTime Date { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private readonly List<OrderItem> _items = new();

    private Order() { }

    public static Order Create(string orderNumber, Customer customer, List<OrderItem> items)
    {
        return new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            Customer = customer,
            CustomerId = customer.Id,
            Date = DateTime.UtcNow
        }.AddItems(items);
    }

    private Order AddItems(List<OrderItem> items)
    {
        _items.AddRange(items);
        return this;
    }
}
```
```csharp
public class OrderItem
{
    public Guid Id { get; private set; }

    public string Product { get; private set; } = null!;

    public int Quantity { get; private set; }

    public Guid OrderId { get; private set; }

    public Order Order { get; private set; } = null!;

    private OrderItem() { }

    public OrderItem(string productName, int quantity)
    {
        Id = Guid.NewGuid();
        Product = productName;
        Quantity = quantity;
    }
}
```

In my projects, I like using a combination of [Clean Architecture](https://antondevtips.com/blog/the-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices) and [Vertical Slices](https://antondevtips.com/blog/vertical-slice-architecture-the-best-ways-to-structure-your-project). I like having a Domain and Infrastructure projects from the [Clean Architecture](https://antondevtips.com/blog/the-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices) and combine Application and Presentation Layer into [Vertical Slices](https://antondevtips.com/blog/vertical-slice-architecture-the-best-ways-to-structure-your-project). In some cases, I can even combine Infrastructure, Application and Presentation into Vertical Slices.

For these entities, I have the respecting repositories and their implementations:

```csharp
public interface ICustomerRepository
{
    Task AddAsync(Customer customer, CancellationToken cancellationToken);

    Task UpdateAsync(Customer customer, CancellationToken cancellationToken);

    Task<Customer?> GetByIdAsync(Guid customerId, CancellationToken cancellationToken);

    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken);
}

public interface IOrderRepository
{
    Task AddAsync(Order order, CancellationToken cancellationToken);

    Task UpdateAsync(Order order, CancellationToken cancellationToken);

    Task<Order?> GetByNumberAsync(string orderNumber, CancellationToken cancellationToken);

    Task<bool> ExistsByNumberAsync(string orderNumber, CancellationToken cancellationToken);
}

public interface IShipmentRepository
{
    Task<bool> ExistsByOrderIdAsync(Guid orderId, CancellationToken cancellationToken);

    Task AddAsync(Shipment shipment, CancellationToken cancellationToken);

    Task<Shipment?> GetByNumberAsync(string shipmentNumber, CancellationToken cancellationToken);
}
```

I grouped together Order and OrderItems into a single repository, as well as Shipment and ShipmentItem. However, all the entities are related together, and it will be a bad decision to create a single monstrous repository for all of them.

When creating an order, we also need to create a respective shipment, we need to have both operations atomic. If we implement two database calls, we can end up with inconsistent data if an order is created in the database and shipment is not:

```csharp
await orderRepository.AddAsync(order, cancellationToken);
await shipmentRepository.AddAsync(shipment, cancellationToken);
```

In such a case we can use `IUnitOfWork` to solve our consistency problem. Let's implement it.

First, we need to define `IUnitOfWork` interface:

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

You have 2 ways for implementing `IUnitOfWork`:

*   create an implementation that receives EF Core DbContext as a constructor parameter
*   or directly use EF Core DbContext as it already implements the IUnitOfWork pattern out of the box

Both approaches are fine, and I like using the 2nd approach.

I inherit my ShippingDbContext from the `IUnitOfWork` interface:

```csharp
public class ShippingDbContext(DbContextOptions<ShippingDbContext> options)
    : DbContext(options), IUnitOfWork
{
    public DbSet<Shipment> Shipments { get; set; }
    public DbSet<ShipmentItem> ShipmentItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("shipping");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ShippingDbContext).Assembly);
    }
}
```

The signature of `IUnitOfWork.SaveChangesAsync` is identical to the existing `DbContext.SaveChangesAsync` method from the base class. So you don't need to do anything else in the DbContext.

You need to register the `IUnitOfWork` interface and resolve the `ShippingDbContext` from the current scope:

```csharp
services.AddScoped<IUnitOfWork>(c => c.GetRequiredService<ShippingDbContext>());
```

Now let's rework the `AddAsync` methods of all our repositories and remove the `SaveChangesAsync` call to the DbContext:

```csharp
public async Task AddAsync(Order order, CancellationToken cancellationToken)
{
    await context.Set<Order>().AddAsync(order, cancellationToken);
}
```

The saving changes is delegated to our **Unit of Work**, so we can update our code as follows:

```csharp
await orderRepository.AddAsync(order, cancellationToken);
await shipmentRepository.AddAsync(shipment, cancellationToken);
await unitOfWork.SaveChangesAsync(cancellationToken);
```

The main idea is that all repositories make corresponding changes in the EF Core's [Change Tracker](https://antondevtips.com/blog/understanding-change-tracking-for-better-performance-in-ef-core) and UnitOfWork saves them all in a single atomic transaction.

Here is the full code that handles the order creation:

```csharp
public async Task<ErrorOr<OrderResponse>> Handle(
    CreateOrderCommand request,
    CancellationToken cancellationToken)
{
    var customer = await customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
    if (customer is null)
    {
        logger.LogWarning("Customer with ID '{CustomerId}' does not exist", request.CustomerId);
        return Error.NotFound($"Customer with ID '{request.CustomerId}' does not exist");
    }

    var order = Order.Create(
        orderNumber: GenerateNumber(),
        customer,
        request.Items.Select(x => new OrderItem(x.ProductName, x.Quantity)).ToList()
    );

    var shipment = Shipment.Create(
        number: GenerateNumber(),
        orderId: order.Id,
        address: request.ShippingAddress,
        carrier: request.Carrier,
        receiverEmail: request.ReceiverEmail,
        items: []
    );

    var shipmentItems = CreateShipmentItems(order.Items, shipment.Id).ToList();
    shipment.AddItems(shipmentItems);

    await orderRepository.AddAsync(order, cancellationToken);
    await shipmentRepository.AddAsync(shipment, cancellationToken);
    await unitOfWork.SaveChangesAsync(cancellationToken);

    logger.LogInformation("Created order: {@Order} with shipment: {@Shipment}", order, shipment);

    var response = order.MapToResponse();
    return response;
}
```

In our application, we could have a use case when a new customer makes on order on the website, and we need to:

*   create customer
*   create an order with order items
*   create shipment with shipment items

in a single operation.

With a UnitOfWork pattern it will be as easy as the following:

```csharp
await customerRepository.AddAsync(customer, cancellationToken);
await orderRepository.AddAsync(order, cancellationToken);
await shipmentRepository.AddAsync(shipment, cancellationToken);
await unitOfWork.SaveChangesAsync(cancellationToken);
```

## Summary:

The Unit of Work pattern is a powerful design pattern that can greatly enhance the integrity, maintainability, and testability of your EF Core-based applications. By treating a set of operations as a single transaction, it ensures data consistency and reduces the complexity of transaction management across multiple repositories. Now you should have a clear understanding on how to implement the Unit of Work pattern in EF Core in your applications.

Hope you find this newsletter useful. See you next time.