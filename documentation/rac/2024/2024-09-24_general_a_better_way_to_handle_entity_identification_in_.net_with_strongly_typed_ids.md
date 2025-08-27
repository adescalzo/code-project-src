```yaml
---
title: A Better Way to Handle Entity Identification in .NET with Strongly Typed IDs
source: https://antondevtips.com/blog/a-better-way-to-handle-entity-identification-in-dotnet-with-strongly-typed-ids
date_published: 2024-09-24T11:00:16.694Z
date_captured: 2025-08-06T17:29:22.030Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [.NET, StronglyTypedId, Entity Framework Core]
programming_languages: [C#, SQL]
tags: [strongly-typed-ids, value-objects, domain-driven-design, type-safety, dotnet, csharp, entity-framework-core, code-quality, primitive-obsession, refactoring]
key_concepts: [Strongly Typed IDs, Primitive Obsession, Value Objects, Domain-Driven Design, Type Safety, Immutability, C# Records, EF Core Value Conversions]
code_examples: true
difficulty_level: intermediate
summary: |
  This article introduces Strongly Typed IDs as a robust solution to the Primitive Obsession anti-pattern in .NET applications, specifically for entity identifiers. It explains how encapsulating IDs in custom types like C# records or structs enhances type safety, code clarity, and maintainability, preventing common runtime errors. The post demonstrates various implementation methods, including using the `StronglyTypedId` NuGet package and native C# record types, with a preference for `readonly record structs`. Furthermore, it provides guidance on mapping these custom IDs with Entity Framework Core and integrating them seamlessly with request/response DTO models.
---
```

# A Better Way to Handle Entity Identification in .NET with Strongly Typed IDs

![Cover image for the article titled 'A Better Way to Handle Entity Identification in .NET with Strongly Typed IDs', featuring a code icon and 'dev tips' text.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Farchitecture%2Fcover_architecture_strongly_typed_ids.png&w=3840&q=100)

# A Better Way to Handle Entity Identification in .NET with Strongly Typed IDs

Sep 24, 2024

[Download source code](/source-code/a-better-way-to-handle-entity-identification-in-dotnet-with-strongly-typed-ids)

7 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

**Strongly Typed IDs** are custom types that are used to represent entity identifiers (IDs) in your application instead of using primitive types like int, Guid, or string. Instead of using these primitive types directly to represent IDs, you create a specific class or struct that encapsulates the ID value. This approach helps to make the code more expressive, safer, and easier to maintain.

## Primitive Obsession Problem

In the previous blog post, we explored what is a [Primitive Obsession](https://antondevtips.com/blog/a-modern-way-to-create-value-objects-to-solve-primitive-obsession-in-net) and how it can be solved by using [Value Objects](https://antondevtips.com/blog/a-modern-way-to-create-value-objects-to-solve-primitive-obsession-in-net).

In short, **primitive obsession** is a tendency to use basic data types to represent more complex concepts. **Primitive obsession** occurs when basic data types (such as int, string, or DateTime) are overused to represent complex concepts in your domain.

It is a common anti-pattern that can lead to unclear code and harder-to-maintain systems.

The same problem goes with identifiers (IDs) if you have multiple entities in your project. If you're using `Guid` for all entities identifiers, like CustomerId, OrderId, OrderItemId, it could lead to mistakenly passing an `OrderId` where a `OrderItemId` is expected. All because the same types are used everywhere.

This **primitive obsession** problem can be solved by using [Value Objects](https://antondevtips.com/blog/a-modern-way-to-create-value-objects-to-solve-primitive-obsession-in-net). **Value Objects** represent a value in your domain that has no identity but is defined by its attributes.

A **primitive obsession** problem for entity identifiers is solved by using **Strongly Typed IDs** that are a sort of **Value Objects**, but only applied to entity identifiers.

## Examples of Primitive Obsession with Entity Identifiers

Let's explore an application example that has `Order` and `OrderItem` entities. Both entities have guids as their entity identifier type:

```csharp
public class Order
{
    public Guid Id { get; set; }
}

public class OrderItem
{
    public Guid Id { get; set; }
}
```

Imagine that you're calling the `ProcessOrderAsync` method:

```csharp
public Task ProcessOrderAsync(Guid orderId, Guid orderItemId)
{
    // Logic to process the order and its item
}

// Correct usage
ProcessOrder(order.Id, orderItem.Id);

// Incorrect usage - No compile-time error
ProcessOrder(orderItem.Id, order.Id);
```

This code compiles and executes successfully. Did you notice a problem?

As we use `Guid` type for both `Order.Id` and `OrderItem.Id` - you can pass parameters in the wrong order. You will end up with wrong data in the database which can lead to serious problems.

The solution to this problem is to use **Strongly Typed IDs**, which encapsulate entity identifiers into a custom meaningful unit.

## What Are Strongly Typed IDs?

**Strongly Typed IDs** are custom types that are used to represent entity identifiers (IDs) in your application instead of using primitive types like int, Guid, or string. Instead of using these primitive types directly to represent IDs, you create a specific class or struct that encapsulates the ID value. This approach helps to make the code more expressive, safer, and easier to maintain.

**Key characteristics of Strongly Typed IDs:**

*   Immutability: once created, a Strongly Typed Id cannot be changed. Any modification results in a new instance.
*   Equality: Strongly Typed IDs are compared based on their value, not by reference.

**Benefits of Strongly Typed IDs:**

*   **Enhanced Type Safety:** one of the most significant benefits of strongly typed IDs is that they enhance type safety. This reduces the chances of accidentally mixing up different types of IDs, leading to fewer bugs and runtime errors.
*   **Improved Code Clarity:** Strongly typed IDs make your code more expressive and self-documenting. When you see a method that accepts an OrderId, it's immediately clear what kind of ID is expected, as opposed to a generic Guid or int.
*   **Better Domain Modeling:** in Domain-Driven Design (DDD), strongly typed IDs help reinforce the concept of entities and their identities. This makes the domain model more robust and aligned with the real-world concepts it represents.
*   **Support for Future Enhancements:** if the requirements for an ID change (e.g., needing to change a type of entity identifier), a strongly typed ID class can be updated or extended to support these new requirements without breaking existing code.
*   **Easier Refactoring:** Strongly typed IDs make refactoring easier, as changes to ID-related logic can be made in a single place, reducing the possibility of introducing errors during the process.

Now let's explore what options do we have for creating Strongly Typed IDs in .NET.

## An Example Application

Today I'll show you how to implement **Strongly Typed IDs** for a **Shipping Application** that is responsible for creating and updating customers, orders and shipments for ordered products.

This application has the following entities:

*   Customers
*   Orders, OrderItems
*   Shipments, ShipmentItems

I am using Domain Driven Design practices for my entities. Let's explore an `Order` and `Order Item` entities that use primitive types for the entity identifiers:

```csharp
public class Order
{
    public Guid Id { get; private set; }

    public OrderNumber OrderNumber { get; private set; }

    public Guid CustomerId { get; private set; }

    public Customer Customer { get; private set; }

    public DateTime Date { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private readonly List<OrderItem> _items = new();

    private Order() { }

    public static Order Create(OrderNumber orderNumber, Customer customer, List<OrderItem> items)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            Customer = customer,
            CustomerId = customer.Id,
            Date = DateTime.UtcNow
        };

        order.AddItems(items);

        return order;
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

    public ProductName Product { get; private set; }

    public int Quantity { get; private set; }

    public Guid OrderId { get; private set; }

    public Order Order { get; private set; } = null!;

    private OrderItem() { }

    public OrderItem(ProductName productName, int quantity)
    {
        Id = Guid.NewGuid();
        Product = productName;
        Quantity = quantity;
    }
}
```

As you can see, both entities use **Value Objects** for their properties. If you want to learn more about Value Objects — make sure to check out my corresponding [blog post](https://antondevtips.com/blog/a-modern-way-to-create-value-objects-to-solve-primitive-obsession-in-net).

Now let's explore how to replace these primitive types for entity identifiers with Strongly Typed IDs.

## Creating Strongly Typed Ids in .NET

I can list the following most popular options for creating Strongly Typed Ids:

*   using [StronglyTypedId](https://github.com/andrewlock/StronglyTypedId) package written by Andrew Lock
*   using C# Records
*   using C# Record Structs

Let's explore each option more in-depth.

### Creating Strongly Typed Ids with StronglyTypedId package

By using the `StronglyTypedId` package, you can generate strongly-typed ID structs for your entities.

First, you need to install the package:

```bash
dotnet add package StronglyTypedId
```

You need to define a `partial struct` and add a `StronglyTypedId` attribute:

```csharp
[StronglyTypedId]
public readonly partial struct OrderId { }

[StronglyTypedId]
public readonly partial struct OrderItemId { }
```

`StronglyTypedId` package will use source generators to implement both `OrderId` and `OrderItemId` structs. By default, the underline type is `Guid`.

Here is how you can create an OrderId:

```csharp
var orderId = new OrderId(Guid.NewGuid());
var orderItemId = new OrderItemId(Guid.NewGuid());
```

You can use a `Value` property to retrieve a value hidden inside a strongly typed id:

```csharp
var guid = orderId.Value;
var guid2 = orderItemId.Value;
```

If you need to change the type, specify a `Template` in the attribute:

```csharp
[StronglyTypedId(Template.Int)]
public readonly partial struct OrderId { }
```

Currently supported built-in backing types are:

*   Guid (default)
*   int
*   long
*   string

### Creating Strongly Typed IDs with Records

You don't have to use external packages as C# **records** already have all you need for strongly typed ids.

Records are a really modern-way to create Strongly Typed IDs in .NET.

Records are immutable reference types and their support equality comparison out of the box. They are compared based on their properties, not by reference.

Here's how you can define the same Strongly Typed IDs using records:

```csharp
public record OrderId(Guid Value);

public record OrderItemId(Guid Value);
```

### Creating Strongly Typed IDs with Record Structs

Records are a wonderful choice for Strongly Typed IDs, but they are reference types. If you care about memory allocations, you can use `readonly record structs` for **Strongly Typed IDs**. They behave the same as `records` but they are value types and not allocated on the heap.

This is my personal choice for creating Strongly Typed IDs.

Here is how you can define the Strongly Typed IDs with `record structs`:

```csharp
public readonly record struct OrderId(Guid Value);

public readonly record struct OrderItemId(Guid Value);
```

Here is how the `Order` and `OrderItem` entities will look like with Strongly Typed IDs:

```csharp
public class Order
{
    public OrderId Id { get; private set; }

    public OrderNumber OrderNumber { get; private set; }

    public CustomerId CustomerId { get; private set; }

    public Customer Customer { get; private set; }

    public DateTime Date { get; private set; }

    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

    private readonly List<OrderItem> _items = new();

    private Order() { }

    public static Order Create(OrderNumber orderNumber, Customer customer, List<OrderItem> items)
    {
        var order = new Order
        {
            Id = new OrderId(Guid.NewGuid()),
            OrderNumber = orderNumber,
            Customer = customer,
            CustomerId = customer.Id,
            Date = DateTime.UtcNow
        };

        order.AddItems(items);

        return order;
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
    public OrderItemId Id { get; private set; }

    public ProductName Product { get; private set; }

    public int Quantity { get; private set; }

    public OrderId OrderId { get; private set; }

    public Order Order { get; private set; } = null!;

    private OrderItem() { }

    public OrderItem(ProductName productName, int quantity)
    {
        Id = new OrderItemId(Guid.NewGuid());
        Product = productName;
        Quantity = quantity;
    }
}
```

If you try to misuse the entity identifiers, you will get a compilation error:

![Screenshot of C# code in an IDE showing a compile-time error when an OrderItemId is incorrectly passed where an OrderId is expected, demonstrating enhanced type safety.](https://antondevtips.com/media/code_screenshots/architecture/strongly-typed-ids/img_1.png)

## Mapping Strongly Typed IDs in EF Core

After introducing Strongly Typed IDs in your entity models, you need to modify your EF Core Mapping.

You need to use conversion to tell EF Core how to map Strongly Typed IDs to the database, and how to map database values to ids.

For example, for `Order` entity:

```csharp
builder.Property(x => x.Id)
    .HasConversion(
        id => id.Value,
        value => new OrderId(value)
    )
    .IsRequired();
```

## Strongly Typed IDs and Request/Response/DTO models

Strongly Typed IDs are your domain-specific models, the outside world should not know about them. And moreover, your public request/response/DTO models should be as simple as possible.

It is a good practice to have plain primitives types in your request/response/DTO models and map them to domain entities and vice versa.

For example, I am mapping `CustomerId` with `Guid` type to `CustomerId` in my "Create Order" use case:

```csharp
public sealed record CreateOrderRequest(
    Guid CustomerId,
    List<OrderItemRequest> Items,
    Address ShippingAddress,
    string Carrier,
    string ReceiverEmail);

public static CreateOrderCommand MapToCommand(this CreateOrderRequest request)
{
    return new CreateOrderCommand(
        CustomerId: new CustomerId(request.CustomerId),
        Items: request.Items,
        ShippingAddress: request.ShippingAddress,
        Carrier: request.Carrier,
        ReceiverEmail: request.ReceiverEmail
    );
}
```

And the reverse mapping to `CustomerResponse` from Strongly Typed `OrderId`:

```csharp
public static OrderResponse MapToResponse(this Order order)
{
    return new OrderResponse(
        OrderId: order.Id.Value,
        OrderNumber: order.OrderNumber.Value,
        OrderDate: order.Date,
        Items: order.Items.Select(x => new OrderItemResponse(
            ProductName: x.Product.Value,
            Quantity: x.Quantity)).ToList()
    );
}
```

## Summary

Strongly typed IDs are a powerful tool for improving the type safety, clarity, and maintainability of your .NET applications. By encapsulating IDs within dedicated records or structs, you can prevent common mistakes and make your code more expressive.

C# records and readonly record structs provide you an elegant, easy and fast way to implement Strongly Typed IDs without boilerplate code.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/a-better-way-to-handle-entity-identification-in-dotnet-with-strongly-typed-ids)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fa-better-way-to-handle-entity-identification-in-dotnet-with-strongly-typed-ids&title=A%20Better%20Way%20to%20Handle%20Entity%20Identification%20in%20.NET%20with%20Strongly%20Typed%20IDs)[X](https://twitter.com/intent/tweet?text=A%20Better%20Way%20to%20Handle%20Entity%20Identification%20in%20.NET%20with%20Strongly%20Typed%20IDs&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fa-better-way-to-handle-entity-identification-in-dotnet-with-strongly-typed-ids)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fa-better-way-to-handle-entity-identification-in-dotnet-with-strongly-typed-ids)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.