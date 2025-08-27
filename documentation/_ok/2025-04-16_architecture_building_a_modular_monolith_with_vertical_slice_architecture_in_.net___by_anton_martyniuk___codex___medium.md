```yaml
---
title: "Building a Modular Monolith With Vertical Slice Architecture in .NET | by Anton Martyniuk | CodeX | Medium"
source: https://medium.com/codex/building-a-modular-monolith-with-vertical-slice-architecture-in-net-55dad0729986
date_published: 2025-04-16T13:14:39.240Z
date_captured: 2025-08-08T18:26:27.263Z
domain: medium.com
author: Anton Martyniuk
category: architecture
technologies: [.NET, ASP.NET Core, Entity Framework Core, Carter, MediatR, ErrorOr, FluentValidation]
programming_languages: [C#]
tags: [modular-monolith, vertical-slice-architecture, dotnet, architecture, clean-architecture, microservices, web-api, software-design, entity-framework-core, mediatr]
key_concepts: [modular-monolith, vertical-slice-architecture, clean-architecture, microservices-comparison, domain-driven-design, dependency-injection, minimal-apis, inter-module-communication, event-driven-architecture]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces the Modular Monolith architecture as a pragmatic approach that balances the simplicity of a monolith with the clear boundaries of microservices. It demonstrates building a .NET application with three distinct business modules: Shipments, Stocks, and Carriers. The implementation combines Vertical Slice Architecture with Clean Architecture principles, ensuring features are encapsulated and concerns are separated. Each module manages its own database schema using EF Core and communicates with others via public interfaces. The author highlights the benefits of this architecture for development speed, maintainability, and future scalability into microservices.
---
```

# Building a Modular Monolith With Vertical Slice Architecture in .NET | by Anton Martyniuk | CodeX | Medium

# Building a Modular Monolith With Vertical Slice Architecture in .NET

"You shouldn’t start a new project with microservices, even if you’re sure your application will be big enough to make it worthwhile.” — Martin Fowler. I bet you have heard this phrase. And it exists for a reason.

Modern application development often pushes teams toward microservices, but this architecture isn’t always the best starting point. Because microservices, while flexible, are “premium” solutions with high complexity, overhead, and operational costs. Moreover, when starting with microservices, your development speed is limited because you need to coordinate multiple services together, often in different repositories.

So is it better to start a project with a good old Monolith? Not exactly.

A **Modular Monolith** offers the best parts of two worlds from a Monolith and Microservices Architectures. It combines the simplicity of development and deployment while providing clear boundaries between modules.

Today I want to introduce you to a **Modular Monolith**. We’ll explore a real-world example with three business modules: Shipments, Stocks, and Carriers. For the project structure, we’ll use [Vertical Slice Architecture](https://antondevtips.com/blog/vertical-slice-architecture-the-best-ways-to-structure-your-project/?utm_source=antondevtips&utm_medium=own&utm_campaign=15-04-2025-newsletter).

![Building a Modular Monolith with Vertical Slice Architecture in .NET](https://miro.medium.com/v2/resize:fit:700/1*i3mMGsKwQFXKaFgGe7asmQ.png)
*Image: Promotional banner for the article titled "Building a Modular Monolith with Vertical Slice Architecture in .NET" with a dev tips logo.*

# What is a Modular Monolith?

A Modular Monolith is an architecture that combines the best parts from Monolith and Microservices Architectures.

It has the following advantages:

*   Easier development in a single codebase
*   Single deployable application
*   Clear boundaries between modules
*   Independent development of modules
*   Easy integration testing of all the modules together
*   Each module can be extracted into a microservice in the future

Modular Monolith has clear boundaries between its modules. Modules can’t access a database of other modules directly. Modules can talk with each other only via a public API. This works exactly as in microservices but in a single Application.

Often, in a Modular Monolith, modules talk with each other by an interface method call. However, if you plan further migration to microservices — you can use Event-Driven Architecture for communication between modules.

Now let’s explore an application we will be building.

# Modular Monolith Project Structure

I have built an application with three core modules: Shipments, Carriers, and Stocks. In a microservice approach, these might each be separate services with their own codebases and endpoints.

In a Modular Monolith, they’re split into modules in the same solution.

*   **Shipments Module**: Handles creating orders for shipments.
*   **Carriers Module**: Maintains information about shipping partners and registers shipments for delivery.
*   **Stocks Module**: Manages product inventory levels.

Each module exposes an interface for the other modules to call. Under the hood, it’s a method call in the same process. No network overhead, no distributed transactions.

Each module has its own database schema and DbContext for EF Core, providing clear separation of data.

For code structure I like to combine [Clean Architecture](https://antondevtips.com/blog/the-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices/?utm_source=antondevtips&utm_medium=own&utm_campaign=15-04-2025-newsletter) together with [Vertical Slice](https://antondevtips.com/blog/the-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices/?utm_source=antondevtips&utm_medium=own&utm_campaign=15-04-2025-newsletter).

Clean Architecture aims to separate the concerns of the application into distinct layers, promoting high cohesion and low coupling. It consists of the following layers: Domain, Application, Infrastructure, Presentation.

Vertical Slice Architecture structures an application by features. Each slice encapsulates all aspects of a specific feature, including the UI, business logic, and data access.

I found that combining Clean Architecture with Vertical Slices is a great architecture design for the many applications, including Modular Monolith.

Each module follows the same structure in our application:

*   **Domain**: Contains business entities and logic
*   **Features**: Implements business features using Vertical Slices
*   **Infrastructure**: Handles technical concerns like Database
*   **PublicApi**: Defines contracts for inter-module communication

Here is a project’s structure:

![Modular Monolith Solution Structure](https://miro.medium.com/v2/resize:fit:575/0*2o6tCPV8r2NMfX0O.png)
*Image: A screenshot of a solution explorer showing the project structure for a Modular Monolith. It contains a "Modules" folder with subfolders for "Carriers", "Common", "Shipments", and "Stocks". Each module further breaks down into "Domain", "Features", "Infrastructure", and "PublicApi" projects.*

Let’s explore in depth the Shipments module.

# Shipments Module

Here is a detailed structure of the Shipments module:

![Shipments Module Project Structure](https://miro.medium.com/v2/resize:fit:506/0*HZKBsA4kZeyzd_Dn.png)
*Image: A detailed screenshot of the Shipments module's project structure within the solution explorer, showing subfolders like "Domain" (with Entities, Enums, ValueObjects), "Features" (with sub-features like CreateShipment, GetShipmentByNumber, UpdateShipmentStatus), and "Infrastructure".*

I have 2 entities in the Domain Layer:

```csharp
public class Shipment  
{  
    public Guid Id { get; set; }  
    public string Number { get; set; }  
    public string OrderId { get; set; }  
    public Address Address { get; set; }  
    public string Carrier { get; set; }  
    public string ReceiverEmail { get; set; }  
    public ShipmentStatus Status { get; set; }  
    public List<ShipmentItem> Items { get; set; }  
    public DateTime CreatedAt { get; set; }  
    public DateTime? UpdatedAt { get; set; }  
}  
  
public class ShipmentItem  
{  
    public Guid Id { get; set; }  
    public string Product { get; set; }  
    public int Quantity { get; set; }  
}
```

Infrastructure Layer contains a separate EF Core DbContext that only knows about Shipments module data and database migrations.

```csharp
public class ShipmentsDbContext(DbContextOptions<ShipmentsDbContext> options)  
    : DbContext(options)  
{  
    public DbSet<Shipment> Shipments { get; set; }  
    public DbSet<ShipmentItem> ShipmentItems { get; set; }  
      
    protected override void OnModelCreating(ModelBuilder modelBuilder)  
    {  
        base.OnModelCreating(modelBuilder);  
  
        modelBuilder.HasDefaultSchema(DbConsts.ShipmentsSchemaName);  
    }  
}
```

Features Project combines Application and Presentation Layers into Vertical Slices.

Each feature is a use case implementation:

*   Create Shipment
*   Update Shipment Status
*   Get Shipment by Number

Let’s explore an implementation of “Create Shipment” use case. It involves communication with Carriers and Stocks modules:

1.  Checks if a Shipment for a given OrderId is already created
2.  Calls Stocks Module to check products’ availability
3.  Creates Shipment in the database
4.  Calls Carrier Module to save the shipment details
5.  Calls Stocks Module to update stock levels

Here is the Minimal API endpoint:

```csharp
public class CreateShipmentEndpoint : ICarterModule  
{  
    public void AddRoutes(IEndpointRouteBuilder app)  
    {  
        app.MapPost("/api/shipments", Handle);  
    }  
  
    private static async Task<IResult> Handle(        [FromBody] CreateShipmentRequest request,  
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

I use [Carter](https://github.com/CarterCommunity/Carter) library for structuring Minimal APIs and MediatR for encapsulating application logic in command handlers:

```csharp
public async Task<ErrorOr<ShipmentResponse>> Handle(  
    CreateShipmentCommand request,  
    CancellationToken cancellationToken)  
{  
    // 1. Check if shipment already exists  
    var shipmentExists = await context.Shipments.AnyAsync(x => x.OrderId == request.OrderId, cancellationToken);  
    if (shipmentExists)  
    {  
        logger.LogInformation("Shipment for order '{OrderId}' already exists", request.OrderId);  
        return Error.Conflict($"Shipment for order '{request.OrderId}' already exists");  
    }  
  
    // 2. Check stock levels  
    var stockRequest = CreateStockRequest(request);  
    var stockResponse = await stockApi.CheckStockAsync(stockRequest, cancellationToken);  
      
    if (!stockResponse.IsSuccess)  
    {  
        logger.LogInformation("Stock check failed: {ErrorMessage}", stockResponse.ErrorMessage);  
        return Error.Validation("ProductsNotAvailableInStock", stockResponse.ErrorMessage ?? "Products not available in stock");  
    }  
  
    // 3. Save shipment in the database  
    var shipment = request.MapToShipment();  
  
    await context.Shipments.AddAsync(shipment, cancellationToken);  
    await context.SaveChangesAsync(cancellationToken);  
  
    logger.LogInformation("Created shipment: {@Shipment}", shipment);  
  
    // 4. Call Carrier Module to create a shipment  
    var carrierRequest = CreateCarrierRequest(request);  
    await carrierApi.CreateShipmentAsync(carrierRequest, cancellationToken);  
      
    // 5. Update stock levels  
    var updateRequest = CreateUpdateStockRequest(shipment);  
    var response = await stockApi.UpdateStockAsync(updateRequest, cancellationToken);  
      
    if (!response.IsSuccess)  
    {  
        return Error.Failure("StockUpdateFailed", "Failed to update stock");  
    }  
  
    return shipment.MapToResponse();  
}
```

Now let’s explore other modules.

# Carriers and Stocks Modules

Shipments Module calls Carriers Module to create a shipment. Carriers Module exposes a public API in the `Modules.Carriers.PublicApi` project:

```csharp
public interface ICarrierModuleApi  
{  
    Task CreateShipmentAsync(CreateCarrierShipmentRequest request,  
        CancellationToken cancellationToken = default);  
}
```

The implementation is as follows:

```csharp
internal sealed class CarrierModuleApi(    CarriersDbContext dbContext,  
    ILogger<CarrierModuleApi> logger) : ICarrierModuleApi  
{  
    public async Task CreateShipmentAsync(CreateCarrierShipmentRequest request,  
        CancellationToken cancellationToken = default)  
    {  
        logger.LogInformation("Creating shipment for order {OrderId}", request.OrderId);  
  
        var carrier = await dbContext.Carriers  
            .FirstOrDefaultAsync(x => x.Name == request.Carrier && x.IsActive, cancellationToken);  
  
        if (carrier is null)  
        {  
            throw new InvalidOperationException($"Active carrier with Name {request.Carrier} not found");  
        }  
  
        var shipment = CreateCarrierShipment(request, carrier);  
  
        dbContext.CarrierShipments.Add(shipment);  
        await dbContext.SaveChangesAsync(cancellationToken);  
  
        logger.LogInformation("Created shipment {ShipmentId} for order {OrderId}",  
            shipment.Id, request.OrderId);  
    }  
}
```

CarrierModuleApi implementation class is internal, so we are not leaking implementation details to other Modules.

Shipments Module calls Stocks Module to check and update stock levels. Stocks Module exposes a public API in the `Modules.Stocks.PublicApi` project:

```csharp
public interface IStockModuleApi  
{  
    Task<CheckStockResponse> CheckStockAsync(        CheckStockRequest request,   
        CancellationToken cancellationToken = default);  
      
    Task<UpdateStockResponse> UpdateStockAsync(        UpdateStockRequest request,  
        CancellationToken cancellationToken = default);  
}
```

The implementation is also internal:

```csharp
internal sealed class CarrierModuleApi(    CarriersDbContext dbContext,  
    ILogger<CarrierModuleApi> logger) : ICarrierModuleApi  
{  
    public async Task<CheckStockResponse> CheckStockAsync(        CheckStockRequest request,   
        CancellationToken cancellationToken = default)  
    {  
        // ...  
    }  
  
    public async Task<UpdateStockResponse> UpdateStockAsync(        UpdateStockRequest request,  
        CancellationToken cancellationToken = default)  
    {  
        // ...  
    }  
}
```

In all modules I use Vertical Slice Architecture for implementing features.

It gives a lot of advantages:

*   **Feature Focused**: changes are isolated to specific features, reducing the risk of unintended side effects.
*   **Scalability**: easier to scale development by allowing other developers and teams to work on different features independently.
*   **Flexibility**: allows using different technologies or approaches within each slice as needed.
*   **Maintainability**: easier to navigate in the solution, understand and maintain since all aspects of a feature are contained within a single slice.
*   **Reduced Coupling**: minimizes dependencies between different slices.

These advantages are extremely beneficial for Modular Monoliths.

# Summary

Why not starting with microservices?

Setting up microservices involves significant complexity:

*   Multiple databases
*   Network calls or event-driven communication
*   Service Discovery
*   Distributed transactions
*   Monitoring

And even more.

This complexity might not be necessary at the start.

A Modular Monolith can handle a lot of complexities in a single codebase while preserving good boundaries. Later you can extract a module into a microservice more easily, by using the communication via public interfaces.

Hope you find this newsletter useful. See you next time.

_Originally published at_ [_https://antondevtips.com_](https://antondevtips.com/blog/building-a-modular-monolith-with-vertical-slice-architecture-in-dotnet) _on April 15, 2025._

[**On my website**](https://antondevtips.com/blog/building-a-modular-monolith-with-vertical-slice-architecture-in-dotnet?utm_source=medium&utm_medium=social&utm_campaign=april-2025) I share .NET and Architecture best practices.  
[**Subscribe to my newsletter**](https://antondevtips.com/?utm_source=medium&utm_medium=social&utm_campaign=april-2025) to improve my .NET skills.  
[**Download the source code**](https://antondevtips.com/source-code/building-a-modular-monolith-with-vertical-slice-architecture-in-dotnet?utm_source=medium&utm_medium=social&utm_campaign=april-2025) for this newsletter for free