```yaml
---
title: "Clean Architecture With .NET 9 And CQRS | by Michael Maurice | Aug, 2025 | Medium"
source: https://medium.com/@michaelmaurice410/clean-architecture-with-net-9-and-cqrs-62a44926f2d6
date_published: 2025-08-07T17:01:44.915Z
date_captured: 2025-08-08T11:56:41.184Z
domain: medium.com
author: Michael Maurice
category: architecture
technologies: [.NET 9, ASP.NET Core, MediatR, Entity Framework Core, SQL Server, FluentValidation, AutoMapper, xUnit, Moq, FluentAssertions, Swagger, OpenAPI, Microsoft.Extensions.Logging, Microsoft.AspNetCore.Identity, HttpClient, System.Text.Json]
programming_languages: [C#, SQL]
tags: [clean-architecture, cqrs, dotnet, web-api, entity-framework-core, mediatr, design-patterns, testing, domain-driven-design, software-architecture]
key_concepts: [Clean Architecture, CQRS, Domain-Driven Design, Dependency Injection, Pipeline Behaviors, Domain Events, Unit Testing, Integration Testing]
code_examples: false
difficulty_level: intermediate
summary: |
  This comprehensive guide demonstrates implementing Clean Architecture with CQRS in .NET 9. It details the project structure, layer responsibilities (Domain, Application, Infrastructure, WebApi), and core principles of both architectural patterns. The article provides extensive C# code examples for commands, queries, handlers, MediatR pipeline behaviors, and domain event handling. It also covers data persistence with Entity Framework Core, API development using controllers and Minimal APIs, and robust testing strategies, including a MediatR-free CQRS alternative.
---
```

# Clean Architecture With .NET 9 And CQRS | by Michael Maurice | Aug, 2025 | Medium

# Clean Architecture With .NET 9 And CQRS

![Abstract diagram illustrating Clean Architecture layers and concepts. The text within the diagram is largely unreadable due to low resolution, but it attempts to show "Web API Layer", "Application Layer", and "Domain Layer" with associated concepts and possibly some performance metrics.](https://miro.medium.com/v2/resize:fit:517/1*2QmZeZ1SAag_vdUJctVBMg.png)

Clean Architecture combined with CQRS (Command Query Responsibility Segregation) represents one of the most effective approaches for building maintainable, scalable, and testable applications in .NET 9. This architectural pattern provides clear separation of concerns, explicit business logic boundaries, and exceptional flexibility for complex enterprise applications. This comprehensive guide explores how to implement Clean Architecture with CQRS in .NET 9, covering everything from project structure to advanced implementation patterns.

# Understanding Clean Architecture and CQRS

## Clean Architecture Principles

Clean Architecture, popularized by Robert C. Martin (Uncle Bob), organizes code into concentric circles where dependencies flow inward. The core principles include:

*   **Dependency Inversion**: Outer layers depend on inner layers, never the reverse
*   **Independence**: The core business logic remains independent of frameworks, databases, and external concerns
*   **Testability**: Each layer can be tested in isolation with clear boundaries
*   **Flexibility**: Easy to change external dependencies without affecting business logic

## CQRS Pattern Overview

CQRS separates read and write operations into distinct models:

*   **Commands**: Operations that modify system state (Create, Update, Delete)
*   **Queries**: Operations that retrieve data without side effects
*   **Handlers**: Process commands and queries with specific business logic
*   **Separation**: Different optimization strategies for reads vs writes

## Why Combine Clean Architecture with CQRS?

The combination provides several advantages:

*   **Clear Use Cases**: Each command/query represents a specific business use case
*   **Single Responsibility**: Handlers focus on one specific operation
*   **Enhanced Testability**: Easy to unit test individual use cases
*   **Scalability**: Read and write sides can be optimized independently
*   **Maintainability**: Changes are localized to specific features

# Project Structure and Layer Organization

## .NET 9 Clean Architecture Template Structure

Here’s the recommended project structure for Clean Architecture with CQRS in .NET 9:

```
CleanArchitecture.sln
├── src/
│   ├── Domain/                     # Core business logic
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── DomainEvents/
│   │   ├── Enums/
│   │   └── Exceptions/
│   ├── Application/                # Use cases and business rules
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   ├── Behaviors/
│   │   │   ├── Exceptions/
│   │   │   └── Models/
│   │   ├── Features/
│   │   │   ├── Products/
│   │   │   │   ├── Commands/
│   │   │   │   │   ├── CreateProduct/
│   │   │   │   │   ├── UpdateProduct/
│   │   │   │   │   └── DeleteProduct/
│   │   │   │   └── Queries/
│   │   │   │       ├── GetProduct/
│   │   │   │       └── GetProducts/
│   │   │   └── Orders/
│   │   └── DependencyInjection.cs
│   ├── Infrastructure/             # External concerns
│   │   ├── Data/
│   │   ├── Services/
│   │   ├── Configurations/
│   │   └── DependencyInjection.cs
│   └── WebApi/                     # Presentation layer
│       ├── Controllers/
│       ├── Middleware/
│       ├── Program.cs
│       └── appsettings.json
└── tests/
    ├── Domain.UnitTests/
    ├── Application.UnitTests/
    └── WebApi.IntegrationTests/
```

## Domain Layer Implementation

The domain layer contains the core business entities and rules:

```csharp
namespace CleanArchitecture.Domain.Entities;
public class Product : BaseAuditableEntity, IAggregateRoot
{
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int StockQuantity { get; private set; }
    public ProductStatus Status { get; private set; }
      
    private readonly List<DomainEvent> _domainEvents = new();
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();
    // Private constructor for EF Core
    private Product() { }
    public Product(string name, string description, decimal price, int stockQuantity)
    {
        Guard.Against.NullOrEmpty(name, nameof(name));
        Guard.Against.NullOrEmpty(description, nameof(description));
        Guard.Against.NegativeOrZero(price, nameof(price));
        Guard.Against.Negative(stockQuantity, nameof(stockQuantity));
        Name = name;
        Description = description;
        Price = price;
        StockQuantity = stockQuantity;
        Status = ProductStatus.Active;
        RaiseDomainEvent(new ProductCreatedEvent(this));
    }
    public void UpdatePrice(decimal newPrice)
    {
        Guard.Against.NegativeOrZero(newPrice, nameof(newPrice));
          
        var oldPrice = Price;
        Price = newPrice;
          
        RaiseDomainEvent(new ProductPriceChangedEvent(this, oldPrice, newPrice));
    }
    public void UpdateStock(int quantity)
    {
        Guard.Against.Negative(quantity, nameof(quantity));
        StockQuantity = quantity;
          
        if (quantity == 0)
        {
            Status = ProductStatus.OutOfStock;
            RaiseDomainEvent(new ProductOutOfStockEvent(this));
        }
        else if (Status == ProductStatus.OutOfStock)
        {
            Status = ProductStatus.Active;
            RaiseDomainEvent(new ProductBackInStockEvent(this));
        }
    }
    public void Deactivate()
    {
        Status = ProductStatus.Inactive;
        RaiseDomainEvent(new ProductDeactivatedEvent(this));
    }
    private void RaiseDomainEvent(DomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
public enum ProductStatus
{
    Active,
    Inactive,
    OutOfStock,
    Discontinued
}
```

## Value Objects and Domain Events

```csharp
namespace CleanArchitecture.Domain.ValueObjects;
public class Money : ValueObject
{
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
    private Money() { } // EF Core
    public Money(decimal amount, string currency)
    {
        Guard.Against.NegativeOrZero(amount, nameof(amount));
        Guard.Against.NullOrEmpty(currency, nameof(currency));
          
        Amount = amount;
        Currency = currency;
    }
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Amount;
        yield return Currency;
    }
    public static Money Create(decimal amount, string currency) => new(amount, currency);
      
    public Money Add(Money other)
    {
        if (Currency != other.Currency)
            throw new InvalidOperationException("Cannot add money with different currencies");
              
        return new Money(Amount + other.Amount, Currency);
    }
}
namespace CleanArchitecture.Domain.Events;
public record ProductCreatedEvent(Product Product) : DomainEvent;
public record ProductPriceChangedEvent(Product Product, decimal OldPrice, decimal NewPrice) : DomainEvent;
public record ProductOutOfStockEvent(Product Product) : DomainEvent;
```

# Application Layer with CQRS Implementation

## CQRS Abstractions

```csharp
namespace CleanArchitecture.Application.Common.Messaging;
public interface ICommand : IRequest
{
}
public interface ICommand<TResponse> : IRequest<TResponse>
{
}
public interface IQuery<TResponse> : IRequest<TResponse>
{
}
public interface ICommandHandler<TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand
{
}
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, TResponse>
    where TCommand : ICommand<TResponse>
{
}
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, TResponse>
    where TQuery : IQuery<TResponse>
{
}
```

## Command Implementation

```csharp
namespace CleanArchitecture.Application.Features.Products.Commands.CreateProduct;
public record CreateProductCommand(    string Name,
    string Description,
    decimal Price,
    int StockQuantity) : ICommand<CreateProductResponse>;
public record CreateProductResponse(Guid ProductId);
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Product name is required")
            .MaximumLength(200).WithMessage("Product name must not exceed 200 characters");
        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Product description is required")
            .MaximumLength(1000).WithMessage("Product description must not exceed 1000 characters");
        RuleFor(x => x.Price)
            .GreaterThan(0).WithMessage("Product price must be greater than zero");
        RuleFor(x => x.StockQuantity)
            .GreaterThanOrEqualTo(0).WithMessage("Stock quantity cannot be negative");
    }
}
public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreateProductCommandHandler> _logger;
    public CreateProductCommandHandler(        IApplicationDbContext context,
        ILogger<CreateProductCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }
    public async Task<CreateProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating product with name: {ProductName}", request.Name);
        var product = new Product(
            request.Name,
            request.Description,
            request.Price,
            request.StockQuantity);
        _context.Products.Add(product);
        await _context.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product created successfully with ID: {ProductId}", product.Id);
        return new CreateProductResponse(product.Id);
    }
}
```

## Query Implementation

```csharp
namespace CleanArchitecture.Application.Features.Products.Queries.GetProduct;
public record GetProductQuery(Guid ProductId) : IQuery<ProductResponse>;
public record ProductResponse(    Guid Id,
    string Name,
    string Description,
    decimal Price,
    int StockQuantity,
    string Status);
public class GetProductQueryValidator : AbstractValidator<GetProductQuery>
{
    public GetProductQueryValidator()
    {
        RuleFor(x => x.ProductId)
            .NotEmpty().WithMessage("Product ID is required");
    }
}
public class GetProductQueryHandler : IQueryHandler<GetProductQuery, ProductResponse>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    public GetProductQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }
    public async Task<ProductResponse> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _context.Products
            .Where(p => p.Id == request.ProductId)
            .ProjectTo<ProductResponse>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync(cancellationToken);
        if (product is null)
        {
            throw new NotFoundException(nameof(Product), request.ProductId);
        }
        return product;
    }
}
// AutoMapper Profile
public class ProductMappingProfile : Profile
{
    public ProductMappingProfile()
    {
        CreateMap<Product, ProductResponse>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
    }
}
```

# Advanced CQRS Patterns

## Pipeline Behaviors

Implement cross-cutting concerns using MediatR pipeline behaviors:

```csharp
namespace CleanArchitecture.Application.Common.Behaviors;
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (_validators.Any())
        {
            var context = new ValidationContext<TRequest>(request);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults
                .Where(r => r.Errors.Any())
                .SelectMany(r => r.Errors)
                .ToList();
            if (failures.Any())
            {
                throw new ValidationException(failures);
            }
        }
        return await next();
    }
}
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger, ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId ?? string.Empty;
        _logger.LogInformation("Processing request: {Name} {@UserId} {@Request}",
            requestName, userId, request);
        var stopwatch = Stopwatch.StartNew();
          
        try
        {
            var response = await next();
              
            stopwatch.Stop();
            _logger.LogInformation("Completed request: {Name} in {ElapsedMilliseconds}ms",
                requestName, stopwatch.ElapsedMilliseconds);
                  
            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "Request failed: {Name} in {ElapsedMilliseconds}ms",
                requestName, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }
}
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;
    public PerformanceBehavior(        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }
    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();
        var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;
        if (elapsedMilliseconds > 500) // Log slow requests
        {
            var requestName = typeof(TRequest).Name;
            var userId = _currentUserService.UserId ?? string.Empty;
            _logger.LogWarning("Long running request: {Name} ({ElapsedMilliseconds} ms) {@UserId} {@Request}",
                requestName, elapsedMilliseconds, userId, request);
        }
        return response;
    }
}
```

## Domain Event Handling

```csharp
namespace CleanArchitecture.Application.Features.Products.Events;
public class ProductCreatedEventHandler : INotificationHandler<ProductCreatedEvent>
{
    private readonly ILogger<ProductCreatedEventHandler> _logger;
    private readonly IEmailService _emailService;
    private readonly IInventoryService _inventoryService;
    public ProductCreatedEventHandler(        ILogger<ProductCreatedEventHandler> logger,
        IEmailService emailService,
        IInventoryService inventoryService)
    {
        _logger = logger;
        _emailService = emailService;
        _inventoryService = inventoryService;
    }
    public async Task Handle(ProductCreatedEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Handling product created event for product: {ProductId}",
            notification.Product.Id);
        // Initialize inventory tracking
        await _inventoryService.InitializeTrackingAsync(notification.Product.Id, cancellationToken);
        // Send notification to administrators
        await _emailService.SendProductCreatedNotificationAsync(notification.Product, cancellationToken);
        _logger.LogInformation("Product created event handled successfully for product: {ProductId}",
            notification.Product.Id);
    }
}
public class ProductOutOfStockEventHandler : INotificationHandler<ProductOutOfStockEvent>
{
    private readonly ILogger<ProductOutOfStockEventHandler> _logger;
    private readonly INotificationService _notificationService;
    private readonly IApplicationDbContext _context;
    public ProductOutOfStockEventHandler(        ILogger<ProductOutOfStockEventHandler> logger,
        INotificationService notificationService,
        IApplicationDbContext context)
    {
        _logger = logger;
        _notificationService = notificationService;
        _context = context;
    }
    public async Task Handle(ProductOutOfStockEvent notification, CancellationToken cancellationToken)
    {
        _logger.LogWarning("Product out of stock: {ProductId} - {ProductName}",
            notification.Product.Id, notification.Product.Name);
        // Notify purchasing team
        await _notificationService.SendOutOfStockAlertAsync(notification.Product, cancellationToken);
        // Create reorder suggestion
        var reorderSuggestion = new ReorderSuggestion
        {
            ProductId = notification.Product.Id,
            SuggestedQuantity = 100, // Business logic for reorder quantity
            CreatedAt = DateTime.UtcNow,
            Status = ReorderStatus.Pending
        };
        _context.ReorderSuggestions.Add(reorderSuggestion);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
```

# Infrastructure Layer Implementation

## Database Context Configuration

```csharp
namespace CleanArchitecture.Infrastructure.Data;
public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUserService;
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        IMediator mediator,
        ICurrentUserService currentUserService) : base(options)
    {
        _mediator = mediator;
        _currentUserService = currentUserService;
    }
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Customer> Customers => Set<Customer>();
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await DispatchDomainEventsAsync();
        ApplyAuditInformation();
        return await base.SaveChangesAsync(cancellationToken);
    }
    protected override void OnModelCreating(ModelBuilder builder)
    {
        // Apply all configurations from current assembly
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
          
        base.OnModelCreating(builder);
    }
    private async Task DispatchDomainEventsAsync()
    {
        var domainEntities = ChangeTracker
            .Entries<BaseEntity>()
            .Where(x => x.Entity.DomainEvents.Any())
            .ToList();
        var domainEvents = domainEntities
            .SelectMany(x => x.Entity.DomainEvents)
            .ToList();
        domainEntities.ForEach(entity => entity.Entity.ClearDomainEvents());
        foreach (var domainEvent in domainEvents)
        {
            await _mediator.Publish(domainEvent);
        }
    }
    private void ApplyAuditInformation()
    {
        var entries = ChangeTracker.Entries<BaseAuditableEntity>();
        var currentUserId = _currentUserService.UserId;
        var utcNow = DateTime.UtcNow;
        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = currentUserId;
                    entry.Entity.CreatedAt = utcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.ModifiedBy = currentUserId;
                    entry.Entity.ModifiedAt = utcNow;
                    break;
            }
        }
    }
}
// Entity configurations
public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();
        builder.Property(p => p.Description)
            .HasMaxLength(1000)
            .IsRequired();
        builder.Property(p => p.Price)
            .HasPrecision(18, 2)
            .IsRequired();
        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50);
        // Configure indexes
        builder.HasIndex(p => p.Name);
        builder.HasIndex(p => p.Status);
        // Ignore domain events for EF Core
        builder.Ignore(p => p.DomainEvents);
    }
}
```

## Repository Pattern (Optional)

While Clean Architecture with CQRS often uses DbContext directly, you may choose to implement repositories for complex queries:

```csharp
namespace CleanArchitecture.Infrastructure.Data.Repositories;
public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    public async Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Products.FindAsync(new object[] { id }, cancellationToken);
    }
    public async Task<List<Product>> GetActiveProductsAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.Status == ProductStatus.Active)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }
    public async Task<List<Product>> GetLowStockProductsAsync(int threshold, CancellationToken cancellationToken = default)
    {
        return await _context.Products
            .Where(p => p.StockQuantity <= threshold && p.Status == ProductStatus.Active)
            .OrderBy(p => p.StockQuantity)
            .ToListAsync(cancellationToken);
    }
    public void Add(Product product)
    {
        _context.Products.Add(product);
    }
    public void Update(Product product)
    {
        _context.Products.Update(product);
    }
    public void Remove(Product product)
    {
        _context.Products.Remove(product);
    }
}
```

# Presentation Layer (Web API)

## Controller Implementation

```csharp
namespace CleanArchitecture.WebApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;
    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }
    /// <summary>
    /// Creates a new product
    /// </summary>
    /// <param name="command">Product creation details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created product information</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateProductResponse>> CreateProduct(
        [FromBody] CreateProductCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetProduct), new { id = result.ProductId }, result);
    }
    /// <summary>
    /// Gets a product by ID
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Product details</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductResponse>> GetProduct(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetProductQuery(id);
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
    /// <summary>
    /// Gets all products with optional filtering
    /// </summary>
    /// <param name="query">Query parameters</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of products</returns>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedList<ProductResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedList<ProductResponse>>> GetProducts(
        [FromQuery] GetProductsQuery query,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
    /// <summary>
    /// Updates product price
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="command">Price update details</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpPatch("{id:guid}/price")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateProductPrice(        Guid id,
        [FromBody] UpdateProductPriceCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.ProductId)
        {
            return BadRequest("Product ID mismatch");
        }
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
    /// <summary>
    /// Deletes a product
    /// </summary>
    /// <param name="id">Product ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteProduct(        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new DeleteProductCommand(id);
        await _mediator.Send(command, cancellationToken);
        return NoContent();
    }
}
```

## Minimal API Alternative

For those preferring Minimal APIs in .NET 9:

```csharp
namespace CleanArchitecture.WebApi.Endpoints;
public static class ProductEndpoints
{
    public static void MapProductEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/products")
            .WithTags("Products")
            .WithOpenApi();
        group.MapPost("/", CreateProduct)
            .WithName