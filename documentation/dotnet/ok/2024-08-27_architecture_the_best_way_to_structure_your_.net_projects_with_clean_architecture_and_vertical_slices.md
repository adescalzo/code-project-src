```yaml
---
title: The Best Way To Structure Your .NET Projects with Clean Architecture and Vertical Slices
source: https://antondevtips.com/blog/the-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices
date_published: 2024-08-27T11:00:21.758Z
date_captured: 2025-08-06T17:28:25.505Z
domain: antondevtips.com
author: Anton Martyniuk
category: architecture
technologies: [.NET, ASP.NET Core, MediatR, ErrorOr, Faker]
programming_languages: [C#]
tags: [clean-architecture, vertical-slice-architecture, dotnet, software-architecture, domain-driven-design, web-api, project-structure, design-patterns, mediatr, minimal-api]
key_concepts: [Clean Architecture, Vertical Slice Architecture, Domain-Driven Design, Separation of Concerns, Repository Pattern, Unit of Work, MediatR, Architectural Patterns]
code_examples: true
difficulty_level: intermediate
summary: |
  This article explores how to effectively structure .NET projects by combining the strengths of Clean Architecture and Vertical Slice Architecture. It details the benefits and drawbacks of each architectural style, such as Clean Architecture's separation of concerns and Vertical Slice Architecture's feature-focused development. The proposed hybrid approach retains Clean Architecture's Domain and Infrastructure layers while integrating Vertical Slices for the Application and Presentation layers. This combination aims to provide a robust structure for complex applications, leveraging domain-centric design and improving code organization and maintainability. Code examples in C# illustrate the implementation using MediatR and Minimal APIs.
---
```

# The Best Way To Structure Your .NET Projects with Clean Architecture and Vertical Slices

![Title image for the article, "The Best Way To Structure Your .NET Projects with Clean Architecture and Vertical Slices"](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Farchitecture%2Fcover_architecture_ca_with_vsa.png&w=3840&q=100)

# The Best Way To Structure Your .NET Projects with Clean Architecture and Vertical Slices

Aug 27, 2024

[Download source code](/source-code/the-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices)

7 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

**Clean Architecture** aims to separate the concerns of the application into distinct layers, promoting high cohesion and low coupling.

**Vertical Slice Architecture** (VSA) is a design approach that structures an application by features rather than technical layers. Each slice encapsulates all aspects of a specific feature, including the UI, business logic, and data access. This approach contrasts with traditional architectures that typically segregate an application into horizontal layers.

In this blog post you will learn the benefits of both architecture styles and how you can combine them to create an even better architecture.

## Clean Architecture

Clean Architecture aims to separate the concerns of the application into distinct layers, promoting high cohesion and low coupling. It consists of the following layers:

1.  Domain: contains core business objects such as entities.
2.  Application Layer: implementation of Application use cases.
3.  Infrastructure: implementation of external dependencies like database, cache, message queue, authentication provider, etc.
4.  Presentation: implementation of an interface with the outside world like WebApi, gRPC, GraphQL, MVC, etc.

![Diagram illustrating the traditional Clean Architecture layers: Presentation, Application, Domain, and Infrastructure.](https://antondevtips.com/media/code_screenshots/architecture/clean-architecture-with-vertical-slices/ca_with_vsa_4.png)

**Clean Architecture has the following advantages:**

1.  **Separation of Concerns:** Clean Architecture enforces a clear separation between different layers of the application, such as the UI, business logic, and data access. This separation allows each layer to focus on its specific responsibility, making the codebase more maintainable and understandable.
    
2.  **Testability:** By isolating the business logic from the infrastructure and UI, Clean Architecture makes it easier to write unit tests. The core of the application (the use cases and entities) can be tested without worrying about external dependencies.
    
3.  **Flexibility:** Clean Architecture allows you to change the technology stack (e.g., switching from one database provider to another) with minimal impact on the core business logic. This flexibility is achieved by abstracting infrastructure concerns behind interfaces that the core application depends on.
    
4.  **Code Reusability:** By decoupling the core business logic from the implementation details, Clean Architecture encourages code reusability across different projects or layers within the same project.
    
5.  **Long-term Adaptability:** Clean Architecture is designed to withstand changes in technology or business requirements. By keeping the business logic independent of external factors, the architecture remains adaptable over time.
    

Every architectural style is a tradeoff that comes with benefits and drawbacks.

**Here are the drawbacks of Clean Architecture:**

1.  **Complexity:** Clean Architecture introduces multiple layers and abstractions, which can increase the complexity of the codebase, especially for small projects. Developers may find it overwhelming if the architecture is applied unnecessarily to simple applications.
    
2.  **Overhead:** The separation of concerns and the use of interfaces can lead to additional boilerplate code, which might slow down the development process. This overhead can be particularly noticeable in smaller projects where the benefits of Clean Architecture may not be as noticeable.
    
3.  **Learning Curve:** For developers who are not familiar with Clean Architecture, there is a steep learning curve. Understanding the principles and correctly applying them can take time, especially for those new to software architecture patterns.
    
4.  **Initial Setup Time:** Setting up a Clean Architecture project from scratch requires careful planning and organization. The initial setup time can be longer compared to more straightforward architectural approaches.
    

## Vertical Slice Architecture

[Vertical Slice Architecture](https://antondevtips.com/blog/vertical-slice-architecture-the-best-ways-to-structure-your-project) is an extremely popular way to structure your projects nowadays. It strives for high cohesion within a slice (feature) and loose coupling between slices.

It structures an application by features rather than technical layers. Each slice encapsulates all aspects of a specific feature, including the UI, business logic, and data access.

![Diagram illustrating Vertical Slice Architecture, showing features cutting across traditional layers (Controllers, Services, Repositories, DB).](https://antondevtips.com/media/code_screenshots/architecture/clean-architecture-with-vertical-slices/ca_with_vsa_5.png)

**VSA has the following advantages:**

1.  **Feature Focused:** changes are isolated to specific features, reducing the risk of unintended side effects.
2.  **Scalability:** easier to scale development by allowing other developers and teams to work on different features independently.
3.  **Flexibility:** allows using different technologies or approaches within each slice as needed.
4.  **Maintainability:** easier to navigate in the solution, understand and maintain since all aspects of a feature are contained within a single slice.
5.  **Reduced Coupling:** minimizes dependencies between different slices.

**Let's explore what are the disadvantages of Vertical Slice Architecture:**

1.  **Duplication:** potential for code duplication across slices.
2.  **Consistency:** ensuring consistency across slices and managing cross-cutting concerns (e.g., error handling, logging, validation) requires careful planning.
3.  **Large number of classes and files:** large application can have a lot of vertical slices, each containing multiple small classes.

With the first two disadvantages, you can deal by carefully designing your architecture. For example, you can extract common functionality to its own classes. And use MediatR pipelines to manage the cross-cutting concerns such as error handling, logging, validation, etc.

The third disadvantage could be dealt with a good folder structure.

## Clean Architecture vs Vertical Slice Architecture

On one hand, Clean Architecture offers clear separation between different layers of the application. But on the other hand, you need to navigate across multiple projects to explore the implementation of a single use case.

The best part of Clean Architecture is that you have a Domain-centric design of your application that significantly simplifies the development of complex domains and projects.

Vertical Slice Architecture instead allows you to organize your code that offers rapid navigation and development. A single use case implementation is one place.

What if we can take the best parts of both worlds and combine Clean Architecture with Vertical Slices? Let's get into it.

## How To Structure Your Projects with Clean Architecture and Vertical Slices

I found that combining Clean Architecture with Vertical Slices is a great architecture design for the complex applications. In small applications or in applications that don't have complex business logic, you can use Vertical Slices without Clear Architecture.

As a core, I use Clean Architecture layers and combine them with Vertical Slices.

Here is how the layers are being modified:

1.  Domain: contains core business objects such as entities (remains unchanged).
2.  Infrastructure: implementation of external dependencies like database, cache, message queue, authentication provider, etc (remains unchanged).
3.  Application and Presentation Layers are combined with Vertical Slices.

Let's jump straight into the code.

We'll explore the Shipment Application. Here's my solution structure:

![Screenshot of a Visual Studio Solution Explorer showing the project structure for "CleanArchitectureAndVerticalSlices", including Domain, Features, Host, and Infrastructure projects.](https://antondevtips.com/media/code_screenshots/architecture/clean-architecture-with-vertical-slices/ca_with_vsa_1.png)

### Domain Project

Let's start exploring this application from the **Domain project**:

![Screenshot of the `ShippingService.Domain` project structure, showing `Shipments` folder with `Address.cs`, `Shipment.cs`, `ShipmentItem.cs`, `ShipmentStatus.cs`, `IShipmentRepository.cs`, and `IUnitOfWork.cs`.](https://antondevtips.com/media/code_screenshots/architecture/clean-architecture-with-vertical-slices/ca_with_vsa_2.png)

I use Domain-Driven Design (DDD) for my domain project, here what the `Shipment` entity looks like:

```csharp
public class Shipment
{
	private readonly List<ShipmentItem> _items = [];
	
	public Guid Id { get; private set; }
	public string Number { get; private set; }
	public string OrderId { get; private set; }
	public Address Address { get; private set; }
	public string Carrier { get; private set; }
	public string ReceiverEmail { get; private set; }
	public ShipmentStatus Status { get; private set; }
	public IReadOnlyList<ShipmentItem> Items => _items.AsReadOnly();
	public DateTime CreatedAt { get; private set; }
	public DateTime? UpdatedAt { get; private set; }
	
	private Shipment() { }

	public static Shipment Create(
		string number,
		string orderId,
		Address address,
		string carrier,
		string receiverEmail,
		List<ShipmentItem> items)
	{
		var shipment = new Shipment
		{
			Id = Guid.NewGuid(),
			Number = number,
			OrderId = orderId,
			Address = address,
			Carrier = carrier,
			ReceiverEmail = receiverEmail,
			Status = ShipmentStatus.Created,
			CreatedAt = DateTime.UtcNow
		};
		
		shipment.AddItems(items);

		return shipment;
	}
}
```

`Shipment` entity has additional methods following DDD:

```csharp
public void AddItem(ShipmentItem item)
{
    _items.Add(item);
    UpdatedAt = DateTime.UtcNow;
}

public void AddItems(List<ShipmentItem> items)
{
    _items.AddRange(items);
    UpdatedAt = DateTime.UtcNow;
}

public void RemoveItem(ShipmentItem item)
{
    _items.Remove(item);
    UpdatedAt = DateTime.UtcNow;
}

public void UpdateAddress(Address newAddress)
{
    Address = newAddress;
    UpdatedAt = DateTime.UtcNow;
}

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
...
```

For complex domains with various business rules, a domain-driven design is a good fit. In DDD you encapsulate all business logic within the entities as they implement the business rules.

In our example `Shipment` can be in one of the following states:

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

So it's beneficial to have all business rules on how states are changed in a single class.

### Infrastructure Project

In the **Infrastructure project** I like putting implementation of external integrations like: a database, cache, authentication, etc. If in your projects you don't need to implement repositories or other external integrations — you can simply omit the Infrastructure project.

Be pragmatic, as I showed in one of the [previous posts](https://antondevtips.com/blog/vertical-slice-architecture-the-best-ways-to-structure-your-project) — you can use EF Core directly in the Vertical Slices.

### Features Projects

Here is how my **Features project** looks like

![Screenshot of the `ShippingService.Features` project structure, showing `Shipments` folder containing various feature-specific files like `CreateShipment`, `DispatchShipment`, etc.](https://antondevtips.com/media/code_screenshots/architecture/clean-architecture-with-vertical-slices/ca_with_vsa_3.png)

Here I combine Application and Presentation Layers into Vertical Slices.

For complex Features (Slices) I like using MediatR commands and queries:

```csharp
internal sealed record CreateShipmentCommand(
    string OrderId,
    Address Address,
    string Carrier,
    string ReceiverEmail,
    List<ShipmentItem> Items)
    : IRequest<ErrorOr<ShipmentResponse>>;

internal sealed class CreateShipmentCommandHandler(
    IShipmentRepository repository,
    IUnitOfWork unitOfWork,
    ILogger<CreateShipmentCommandHandler> logger)
    : IRequestHandler<CreateShipmentCommand, ErrorOr<ShipmentResponse>>
{
    public async Task<ErrorOr<ShipmentResponse>> Handle(
        CreateShipmentCommand request,
        CancellationToken cancellationToken)
    {
        var shipmentAlreadyExists = await repository.ExistsByOrderIdAsync(request.OrderId, cancellationToken);
        if (shipmentAlreadyExists)
        {
            logger.LogInformation("Shipment for order '{OrderId}' is already created", request.OrderId);
            return Error.Conflict($"Shipment for order '{request.OrderId}' is already created");
        }

        var shipmentNumber = new Faker().Commerce.Ean8();
        var shipment = request.MapToShipment(shipmentNumber);

        await repository.AddAsync(shipment, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created shipment: {@Shipment}", shipment);

        var response = shipment.MapToResponse();
        return response;
    }
}
```

I use minimal API endpoints that call the given MediatR command/query:

```csharp
public class CreateShipmentEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/shipments", Handle);
    }
    
    private static async Task<IResult> Handle(
        [FromBody] CreateShipmentRequest request,
        IValidator<CreateShipmentRequest> validator,
        IMediator mediator,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }
                
        var command = request.MapToCommand();

        var response = await mediator.Send(command, cancellationToken);
        if (response.IsError)
        {
            return response.Errors.ToProblem();
        }

        return Results.Ok(response.Value);
    }
}
```

I extract cross-cutting concerns like mapping and validation into separate files within a feature's folder.

In more simple use cases, I can use repository or even EF Core directly inside the feature's endpoint:

```csharp
public class DispatchShipmentEndpoint : IEndpoint
{
	public void MapEndpoint(WebApplication app)
	{
		app.MapPost("/api/shipments/dispatch/{shipmentNumber}", Handle);
	}

	private static async Task<IResult> Handle(
		[FromRoute] string shipmentNumber,
		IShipmentRepository repository,
		IUnitOfWork unitOfWork,
		ILogger<DispatchShipmentEndpoint> logger,
		IMediator mediator,
		CancellationToken cancellationToken)
	{
		var shipment = await repository.GetByNumberAsync(shipmentNumber, cancellationToken);
		if (shipment is null)
		{
			logger.LogDebug("Shipment with number {ShipmentNumber} not found", shipmentNumber);
			return Error.NotFound("Shipment.NotFound", $"Shipment with number '{shipmentNumber}' not found").ToProblem();
		}

		var response = shipment.Dispatch();
		if (response.IsError)
		{
			return response.Errors.ToProblem();
		}
		
		await unitOfWork.SaveChangesAsync(cancellationToken);

		logger.LogInformation("Dispatched shipment with {ShipmentNumber}", shipmentNumber);
		return Results.NoContent();
	}
}
```

Thanks to Domain-Driven Design all my business logic with dispatching `Shipment` is encapsulated within the entity. This significantly simplifies our use-case implementation and reduces the code duplication.

Here is the diagram, what my updated Architecture looks like:

![Diagram illustrating the combined Clean Architecture with Vertical Slices, showing Domain and Infrastructure as horizontal layers, and Application/Presentation combined into vertical slices.](https://antondevtips.com/media/code_screenshots/architecture/clean-architecture-with-vertical-slices/ca_with_vsa_6.png)

## Summary

I find the best way to structure complex projects is by using Domain and Infrastructure Layers from Clean Architecture and combining the Application and Presentation Layers into Vertical Slices.

**Domain Layer** allows you to encapsulate business rule within the corresponding entities.

The **Infrastructure Layer** allows you to avoid code duplication of external integrations.

Vertical Slices are a fantastic way to achieve high cohesion within each slice and low coupling across different slices.

If you want to learn more about [Vertical Slices](https://antondevtips.com/blog/vertical-slice-architecture-the-best-ways-to-structure-your-project) - I recommend reading my [blog post](https://antondevtips.com/blog/vertical-slice-architecture-the-best-ways-to-structure-your-project).

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/the-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fthe-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices&title=The%20Best%20Way%20To%20Structure%20Your%20.NET%20Projects%20with%20Clean%20Architecture%20and%20Vertical%20Slices)[X](https://twitter.com/intent/tweet?text=The%20Best%20Way%20To%20Structure%20Your%20.NET%20Projects%20with%20Clean%20Architecture%20and%20Vertical%20Slices&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fthe-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fthe-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.