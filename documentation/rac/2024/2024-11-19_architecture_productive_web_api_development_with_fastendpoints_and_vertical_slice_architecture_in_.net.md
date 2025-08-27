```yaml
---
title: Productive Web API Development with FastEndpoints and Vertical Slice Architecture in .NET
source: https://antondevtips.com/blog/productive-web-api-development-with-fast-endpoints-and-vertical-slice-architecture-in-dotnet
date_published: 2024-11-19T11:55:52.373Z
date_captured: 2025-08-06T17:29:57.974Z
domain: antondevtips.com
author: Anton Martyniuk
category: architecture
technologies: [.NET, ASP.NET Core, FastEndpoints, Entity Framework Core, FluentValidation, ErrorOr]
programming_languages: [C#]
tags: [web-api, dotnet, architecture, fastendpoints, vertical-slice-architecture, domain-driven-design, entity-framework-core, validation, productivity, boilerplate-reduction]
key_concepts: [vertical-slice-architecture, repr-pattern, rich-domain-model, domain-driven-design, minimal-apis, model-binding, dependency-injection, boilerplate-reduction]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores building productive Web APIs in .NET using FastEndpoints and Vertical Slice Architecture (VSA). It demonstrates how FastEndpoints simplifies API development by reducing boilerplate and providing a clear structure, which integrates well with VSA's feature-centric organization. The author illustrates these concepts by implementing a "Shipping Application," showcasing domain entities, EF Core integration, and built-in validation with FluentValidation. The content highlights the benefits of a Rich Domain Model for encapsulating business logic and discusses the advantages of FastEndpoints over traditional Minimal APIs. It provides practical code examples and project structure insights for developing maintainable and efficient .NET Web APIs.
---
```

# Productive Web API Development with FastEndpoints and Vertical Slice Architecture in .NET

![Cover image for the article titled "Productive Web API Development with FastEndpoints and Vertical Slice Architecture in .NET"](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_asp_fast_endpoints_vsa.png&w=3840&q=100)

# Productive Web API Development with FastEndpoints and Vertical Slice Architecture in .NET

Nov 19, 2024

[Download source code](/source-code/productive-web-api-development-with-fast-endpoints-and-vertical-slice-architecture-in-dotnet)

6 min read

### Newsletter Sponsors

[Master The Clean Architecture](https://www.courses.milanjovanovic.tech/a/aff_q2snzxhl/external?affcode=1486372_j2vpyytw). This comprehensive course will teach you how to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture. Join 3,150+ students to accelerate your your growth as a software architect.

[Master The Modular Monolith Architecture](https://www.courses.milanjovanovic.tech/a/aff_l825rpg0/external?affcode=1486372_j2vpyytw). This in-depth course will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario. Join 1,050+ students to accelerate your growth as a software architect.

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

In recent projects, I started using [Vertical Slice Architecture](https://antondevtips.com/blog/vertical-slice-architecture-the-best-ways-to-structure-your-project) (VSA). I chose VSA because it provides fast feature development and easy navigation in the codebase. Each slice encapsulates all aspects of a specific feature, including the Web API, business logic, and data access.

I was looking for a way to become more productive with my VSA when creating Web APIs. And I found a [FastEndPoints library](https://antondevtips.com/blog/getting-started-with-fastendpoints-for-building-web-apis-in-dotnet) that helped me with productivity.

Today I want to share with you my personal experience — how to be more productive when developing Web APIs with **FastEndpoints** and **Vertical Slice Architecture**. We will go step-by-step and implement a production ready application that has Web API, validation, stores and retrieves data from a database.

[](#what-is-fastendpoints)

## What is FastEndpoints?

[FastEndpoints](https://fast-endpoints.com/) is an open-source library for .NET that simplifies the creation of Web APIs by eliminating the need of writing boilerplate code. Built on top of ASP.NET Core Minimal APIs, it leverages all the performance benefits while providing a more straightforward programming model.

In the Minimal APIs, you need to define yourself how you want to structure your endpoints, how to group or not group them together in a single file. In **FastEndpoints** you define each endpoint in a separate class, which results in a Single Responsible and maintainable endpoints.

For me, this concept ideally fits in [Vertical Slice Architecture](https://antondevtips.com/blog/vertical-slice-architecture-the-best-ways-to-structure-your-project).

Now let's explore an application we will be building.

[](#the-application-we-will-be-building)

## The Application We Will Be Building

Today I'll show you how to implement a **Shipping Application** that is responsible for creating customers, orders and shipments for ordered products.

This application implements the following use cases:

*   Create a customer
*   Create an Order with OrderItems, place a Shipment
*   Get a Shipment by number
*   Update Shipment state

Initial steps you need to follow when building this application:

1.  Create domain entities
2.  Create EF Core DbContext and mapping for the entities
3.  Create database migrations
4.  Implement use cases as Web API endpoints

Let's explore an `Order` and `OrderItem` entities:

```csharp
public class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; }
    public string OrderNumber { get; private set; }
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; }
    public DateTime Date { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();

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

My entities represent a Rich Domain Model — a concept from Domain Driven Design.

This concept allows me to implement business rules within my entities, in one place. This allows me to avoid spreading business rules throughout the code base in different classes, making my code more manageable.

For example, in my `Shipment` entity I have the following methods:

```csharp
public ErrorOr<Success> Process()
{
    if (Status is not ShipmentStatus.Created)
    {
        return Error.Validation("Can only update to Processing from Created status");
    }

    Status = ShipmentStatus.Processing;
    UpdatedAt = DateTime.UtcNow;

    return Result.Success;
}

public ErrorOr<Success> Dispatch()
{
    if (Status is not ShipmentStatus.Processing)
    {
        return Error.Validation("Can only update to Dispatched from Processing status");
    }

    Status = ShipmentStatus.Dispatched;
    UpdatedAt = DateTime.UtcNow;

    return Result.Success;
}
```

This encapsulates `Shipment` state changes within the `Shipment` entity.

```csharp
public enum ShipmentStatus
{
    Created,
    Processing,
    Dispatched,
    InTransit,
    Delivered,
    Received,
    Cancelled
}
```

> You can download the source code for the entire application at the end of this blog post.

You can follow Domain Driven Design principle for your domain entities or use anemic entities with plain `get` and `set` properties:

```csharp
public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; }
    public Guid CustomerId { get; set; }
    public Customer Customer { get; set; }
    public DateTime Date { get; set; }
    public List<OrderItem> Items { get; set; } = [];
}
```

If you don't have such business logic as I have with shipments, I recommend using plain `get` and `set` properties.

Now let's create a Web API endpoint for creating orders. There are several code architecture styles for implementing use cases presented by an API:

*   Layered Architecture (N-Tier)
*   Clean Architecture
*   Vertical Slice Architecture (VSA)

Now a few words why I prefer VSA.

In Layered Architecture, there is a tendency to create too big classes for services and repositories. In Clean Architecture, while having a good design in the codebase, a single feature implementation is spread throughout multiple projects.

In Vertical Slice Architecture, I have all implementation needed for a single feature in a single folder or even a single file. Check out my [blog post](https://antondevtips.com/blog/vertical-slice-architecture-the-best-ways-to-structure-your-project) to learn more about different ways to structure your projects with Vertical Slices.

With Vertical Slices, I tend to start simple: implement all the logic in the webapi endpoint directly without using extra abstractions. If my endpoint becomes too complex or the logic should be reused across multiple endpoints, I extract the logic into the Application Layer by creating MediatR commands and queries.

[](#implementing-createorder-use-case)

## Implementing CreateOrder Use Case

Let's create request and response models for creating `Order` with items:

```csharp
public sealed record CreateOrderRequest(
    string CustomerId,
    List<OrderItemRequest> Items,
    Address ShippingAddress,
    string Carrier,
    string ReceiverEmail);

public sealed record OrderItemRequest(
    string ProductName,
    int Quantity);

public sealed record OrderResponse(
    Guid OrderId,
    string OrderNumber,
    DateTime OrderDate,
    List<OrderItemResponse> Items);

public sealed record OrderItemResponse(
    string ProductName,
    int Quantity);
```

And now let's create an API endpoint using FastEndpoints:

```csharp
public class CreateOrderEndpoint(
    ShippingDbContext dbContext,
    ILogger<CreateOrderEndpoint> logger)
    : Endpoint<CreateOrderRequest, Results<Ok<OrderResponse>, ValidationProblem, Conflict<string>, NotFound<string>>>
{
    public override void Configure()
    {
        Post("/api/orders");
        AllowAnonymous();
        Validator<CreateOrderRequestValidator>();
    }

    public override async Task<Results<Ok<OrderResponse>, ValidationProblem, Conflict<string>, NotFound<string>>> ExecuteAsync(
        CreateOrderRequest request, CancellationToken cancellationToken)
    {
        var customer = await dbContext.Set<Customer>()
            .FirstOrDefaultAsync(c => c.Id == Guid.Parse(request.CustomerId), cancellationToken);

        if (customer is null)
        {
            logger.LogWarning("Customer with ID '{CustomerId}' does not exist", request.CustomerId);
            return TypedResults.NotFound($"Customer with ID '{request.CustomerId}' does not exist");
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

        var shipmentItems = CreateShipmentItems(order.Items, shipment.Id);
        shipment.AddItems(shipmentItems);

        await dbContext.Set<Order>().AddAsync(order, cancellationToken);
        await dbContext.Set<Shipment>().AddAsync(shipment, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created order: {@Order} with shipment: {@Shipment}", order, shipment);

        var response = order.MapToResponse();
        return TypedResults.Ok(response);
    }
}
```

As you can see I am using EF Core DbContext directly inside endpoint `ExecuteAsync` method. And nothing is wrong with this approach. If I need to create an order from multiple places, like from other API endpoints or event handlers - I will extract this logic into a MediatR command.

In the `CreateOrder` file, I also have the Validator:

```csharp
public class CreateOrderRequestValidator : Validator<CreateOrderRequest>
{
    public CreateOrderRequestValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty()
            .Must(id => Guid.TryParse(id, out _));

        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("Order must have at least one item.")
            .ForEach(item =>
            {
                item.SetValidator(new OrderItemRequestValidator());
            });

        RuleFor(x => x.ShippingAddress)
            .NotNull()
            .SetValidator(new AddressValidator());

        RuleFor(x => x.Carrier)
            .NotEmpty();

        RuleFor(x => x.ReceiverEmail)
            .NotEmpty()
            .EmailAddress();
    }
}

public class OrderItemRequestValidator : Validator<OrderItemRequest>
{
    public OrderItemRequestValidator()
    {
        RuleFor(x => x.ProductName)
            .NotEmpty();

        RuleFor(x => x.Quantity)
            .GreaterThan(0);
    }
}
```

And in the end of the file, I have mapping:

```csharp
static file class MappingExtensions
{
    public static OrderResponse MapToResponse(this Order order)
    {
        return new OrderResponse(
            OrderId: order.Id,
            OrderNumber: order.OrderNumber,
            OrderDate: order.Date,
            Items: order.Items.Select(x => new OrderItemResponse(
                ProductName: x.Product,
                Quantity: x.Quantity)).ToList()
        );
    }
}
```

Our use case for creating order looks really simple: API endpoint, database logic, validation and mapping all in a single place — in one file.

Here is how all the Vertical Slices look like in my solution.

![Screenshot showing the project structure for Vertical Slices, with folders for "Features", "Customers", "Orders", and "Shipments". Each feature folder contains C# files for specific operations like "CreateCustomer.cs", "CreateOrder.cs", "CancelShipment.cs", etc.](/media/code_screenshots/aspnetcore/fast-endpoints-vsa/img_1.png)

Here is how my solution looks like:

![Screenshot showing the overall solution structure in Visual Studio. It includes two projects: "FastEndpointsVerticalSlices" and "ShippingService.Domain". The "ShippingService" project contains folders like "_requests", "Database", "Extensions", "Features", and "Services", along with configuration files and Program.cs. The "ShippingService.Domain" project contains domain entities like Customer.cs, Order.cs, OrderItem.cs, Shipment.cs, etc.](/media/code_screenshots/aspnetcore/fast-endpoints-vsa/img_2.png)

You can extract validation and mapping into other files, this is also a good great approach. For example:

![Screenshot showing a more granular Vertical Slice project structure. Within a feature folder like "CreateShipment", there are separate C# files for "CreateShipment.cs", "CreateShipment.Mapping.cs", and "CreateShipment.Validators.cs". Other features like "GetShipmentByNumber" and "UpdateShipmentStatus" also show their respective endpoint and validator files.](/media/code_screenshots/architecture/vertical-slice-architecture-project-structure/vsa_project_structure_4.png)

[](#why-fastendpoints)

## Why FastEndpoints?

Why am I using FastEndpoints and not Minimal APIs?

**FastEndpoints** offer the following advantages:

*   FastEndpoints offer a ready code structure for API endpoints with a great design, so you don't need to implement your own with Minimal APIs
*   FastEndpoints implement REPR pattern (Request-Endpoint-Response) where you need to specify a Request and Response types for the endpoint. It brings compiler time safety as you can't return a wrong object or HTTP status code by a mistake.
*   Each endpoint is implemented in a separate Single Responsible class which makes it an ideal choice for Vertical Slices
*   FastEndpoints has built-in Model Binding which is more flexible than built-in model binding in Minimal APIs
*   FastEndpoints has built-in support for FluentValidation

[](#implementing-delivershipmentendpoint-use-case)

## Implementing DeliverShipmentEndpoint Use Case

Let's explore how to implement a `DeliverShipmentEndpoint` that changes `Shipment` state into `Delivered` state:

```csharp
public class DeliverShipmentEndpoint(ShippingDbContext dbContext,
    ILogger<DeliverShipmentEndpoint> logger)
    : EndpointWithoutRequest<Results<NoContent, NotFound<string>, ProblemHttpResult>>
{
    public override void Configure()
    {
        Post("/api/shipments/deliver/{shipmentNumber}");
        AllowAnonymous();
    }

    public override async Task<Results<NoContent, NotFound<string>, ProblemHttpResult>> ExecuteAsync(CancellationToken cancellationToken)
    {
        var shipmentNumber = Route<string>("shipmentNumber");

        var shipment = await dbContext.Shipments.FirstOrDefaultAsync(x => x.Number == shipmentNumber, cancellationToken);
        if (shipment is null)
        {
            logger.LogDebug("Shipment with number {ShipmentNumber} not found", shipmentNumber);
            return TypedResults.NotFound($"Shipment with number '{shipmentNumber}' not found");
        }

        var response = shipment.Deliver();
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Delivered shipment with {ShipmentNumber}", shipmentNumber);
        return TypedResults.NoContent();
    }
}
```

Look how easy it is to change the shipment's state by just calling `shipment.Deliver()` method. That's because we have encapsulated all business logic inside a Shipment entity.

All the rest use cases in this project are implemented in a same manner.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/productive-web-api-development-with-fast-endpoints-and-vertical-slice-architecture-in-dotnet)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fproductive-web-api-development-with-fast-endpoints-and-vertical-slice-architecture-in-dotnet&title=Productive%20Web%20API%20Development%20with%20FastEndpoints%20and%20Vertical%20Slice%20Architecture%20in%20.NET)[X](https://twitter.com/intent/tweet?text=Productive%20Web%20API%20Development%20with%20FastEndpoints%20and%20Vertical%20Slice%20Architecture%20in%20.NET&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fproductive-web-api-development-with-fast-endpoints-and-vertical-slice-architecture-in-dotnet)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fproductive%2Dweb%2Dapi%2Ddevelopment%2Dwith%2Dfast%2Dendpoints%2Dand%2Dvertical%2Dslice%2Darchitecture%2Din%2Ddotnet)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.