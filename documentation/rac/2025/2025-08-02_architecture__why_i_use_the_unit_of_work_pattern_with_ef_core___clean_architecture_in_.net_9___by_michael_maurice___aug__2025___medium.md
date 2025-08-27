```yaml
---
title: "#Why I Use The Unit of Work Pattern With EF Core | Clean Architecture in .NET 9 | by Michael Maurice | Aug, 2025 | Medium"
source: https://medium.com/@michaelmaurice410/why-i-use-the-unit-of-work-pattern-with-ef-core-clean-architecture-in-net-9-20ba736d1f2c
date_published: 2025-08-02T17:02:43.537Z
date_captured: 2025-08-12T21:01:42.609Z
domain: medium.com
author: Michael Maurice
category: architecture
technologies: [.NET 9, Entity Framework Core, SQL Server, ASP.NET Core, MediatR, Moq, NUnit, MSTest, Microsoft.Extensions.Logging, Microsoft.Extensions.Caching.Memory, Microsoft.AspNetCore.Mvc.Testing]
programming_languages: [C#, SQL]
tags: [unit-of-work, clean-architecture, entity-framework, dotnet, design-patterns, repository-pattern, database, testing, orm, transactions]
key_concepts: [Unit of Work pattern, Clean Architecture, Repository pattern, Dependency Injection, Transaction Management, Domain Events, Auditing, CQRS, Testability]
code_examples: false
difficulty_level: intermediate
summary: |
  This comprehensive guide explores the rationale and implementation of an explicit Unit of Work pattern in .NET 9 applications adhering to Clean Architecture principles. It argues that, despite Entity Framework Core's built-in Unit of Work capabilities, an additional abstraction layer enhances domain layer independence, testability, and the integration of cross-cutting concerns like auditing and domain events. The article provides detailed C# code examples for implementing the pattern, including advanced scenarios such as CQRS and multi-context transactions. It also covers dependency injection, testing strategies (unit and integration), performance considerations, and common pitfalls, offering a clear checklist for best practices and guidance on when to apply or skip the pattern.
---
```

# #Why I Use The Unit of Work Pattern With EF Core | Clean Architecture in .NET 9 | by Michael Maurice | Aug, 2025 | Medium

# Why I Use The Unit of Work Pattern With EF Core | Clean Architecture in .NET 9

![Diagram illustrating the Unit of Work pattern in a layered architecture. The Application Layer interacts with multiple Repositories, which in turn communicate with the Unit of Work in the Domain Layer. The Unit of Work manages Domain Events and Audit Logging, and then interacts with the Database in the Persistence Layer via the Infrastructure Layer.](https://miro.medium.com/v2/resize:fit:700/1*0HOPB3BhOqbvhJI04d9LOA.png)

# If you want the full source code, download it from this link: [https://www.elitesolutions.shop/](https://www.elitesolutions.shop/)

The Unit of Work pattern is one of the most controversial topics in .NET development, especially when combined with Entity Framework Core. While EF Core itself implements a form of Unit of Work through its `DbContext`, I believe there are compelling reasons to add an explicit Unit of Work abstraction layer in Clean Architecture applications built with .NET 9. This comprehensive guide explores why I consistently use this pattern, how to implement it effectively, and how it enhances the overall architecture of modern applications.

# Understanding the Unit of Work Pattern

The Unit of Work pattern maintains a list of objects affected by a business transaction and coordinates writing out changes and resolving concurrency problems. In essence, it treats a set of related operations as a single, atomic transaction that either succeeds completely or fails completely, ensuring data consistency and integrity.

# Core Principles

*   **Transaction Boundary:** Groups multiple operations into a single, atomic unit
*   **Consistency:** Ensures all changes succeed or fail together, maintaining data integrity
*   **Coordination:** Manages the interaction between multiple repositories within a single transaction
*   **Explicit Control:** Provides clear, intentional points for committing changes to the database

# The EF Core Dilemma: Built-in vs. Explicit Unit of Work

It’s true that EF Core’s `DbContext` already implements the Unit of Work pattern. The `DbContext` tracks changes, coordinates operations, and commits them all at once through `SaveChanges()`. So why add another layer?

# EF Core as Unit of Work: What It Provides

```csharp
// EF Core DbContext already provides:
public class ECommerceDbContext : DbContext
{
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<Customer> Customers { get; set; }
    // Unit of Work functionality built-in:
    // - Change tracking
    // - Transaction coordination    
    // - Single SaveChanges() call commits all changes
}

// Usage in application layer
public class OrderService
{
    private readonly ECommerceDbContext _context;

    public async Task CreateOrderAsync(CreateOrderRequest request)
    {
        var customer = await _context.Customers.FindAsync(request.CustomerId);
        var products = await _context.Products.Where(p => request.ProductIds.Contains(p.Id)).ToListAsync();
          
        var order = new Order(customer, products);
        _context.Orders.Add(order);
          
        // All changes committed together
        await _context.SaveChangesAsync();
    }
}
```

This works perfectly fine for simple scenarios. However, as applications grow in complexity and we adopt Clean Architecture principles, several limitations become apparent.

# Why I Choose Explicit Unit of Work in Clean Architecture

# 1. Domain Layer Independence

Clean Architecture demands that the domain layer remains independent of infrastructure concerns. By injecting `DbContext` directly into application services, we create a tight coupling between our business logic and Entity Framework.

```csharp
// ❌ Tight coupling to EF Core
public class OrderApplicationService
{
    private readonly ECommerceDbContext _context; // EF Core dependency!

    public OrderApplicationService(ECommerceDbContext context)
    {
        _context = context;
    }
}

// ✅ Clean Architecture with abstraction
public class OrderApplicationService
{
    private readonly IUnitOfWork _unitOfWork; // Infrastructure-agnostic!

    public OrderApplicationService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }
}
```

# 2. Explicit Transaction Boundaries

The Unit of Work pattern makes transaction boundaries explicit and intentional. Instead of calling `SaveChanges()` on a context (which might not be obvious to other developers), we explicitly commit through the unit of work.

```csharp
// ❌ Hidden transaction boundary
public async Task ProcessOrderAsync(Order order)
{
    _orderRepository.Add(order);
    _inventoryRepository.UpdateStock(order.Items);
    _emailRepository.QueueWelcomeEmail(order.CustomerId);
      
    // When does the transaction actually commit? Not clear!
    await _context.SaveChangesAsync();
}

// ✅ Explicit transaction boundary
public async Task ProcessOrderAsync(Order order)
{
    _orderRepository.Add(order);
    _inventoryRepository.UpdateStock(order.Items);
    _emailRepository.QueueWelcomeEmail(order.CustomerId);
      
    // Clear, explicit transaction boundary
    await _unitOfWork.CommitAsync();
}
```

# 3. Enhanced Testability

Unit of Work interfaces are significantly easier to mock than `DbContext` for unit testing.

```csharp
// ✅ Simple unit test with mocked Unit of Work
[Test]
public async Task CreateOrder_ShouldCommitTransaction()
{
    // Arrange
    var mockUnitOfWork = new Mock<IUnitOfWork>();
    var mockOrderRepository = new Mock<IOrderRepository>();
      
    var service = new OrderService(mockUnitOfWork.Object, mockOrderRepository.Object);
    var order = new Order();

    // Act
    await service.CreateOrderAsync(order);

    // Assert
    mockUnitOfWork.Verify(uow => uow.CommitAsync(), Times.Once);
}
```

# 4. Cross-Cutting Behavior Integration

The Unit of Work provides a centralized location for implementing cross-cutting concerns like auditing, domain events, and logging.

```csharp
public class UnitOfWork : IUnitOfWork
{
    private readonly ECommerceDbContext _context;
    private readonly IDomainEventPublisher _eventPublisher;
    private readonly IAuditService _auditService;

    public async Task<int> CommitAsync(CancellationToken cancellationToken = default)
    {
        // Cross-cutting concern: Audit tracking
        await _auditService.SetAuditFieldsAsync(_context.ChangeTracker.Entries());
          
        // Commit database changes
        var result = await _context.SaveChangesAsync(cancellationToken);
          
        // Cross-cutting concern: Domain events
        await _eventPublisher.PublishEventsAsync(_context.ChangeTracker.Entries());
          
        return result;
    }
}
```

# 5. Multi-Repository Coordination

When working with multiple repositories in a single transaction, the Unit of Work clearly coordinates their interactions.

```csharp
public class TransferService
{
    private readonly IAccountRepository _accountRepository;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IAuditRepository _auditRepository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task TransferFundsAsync(TransferRequest request)
    {
        // Multiple repository operations
        var fromAccount = await _accountRepository.GetByIdAsync(request.FromAccountId);
        var toAccount = await _accountRepository.GetByIdAsync(request.ToAccountId);
          
        fromAccount.Debit(request.Amount);
        toAccount.Credit(request.Amount);
          
        var transaction = new Transaction(request.FromAccountId, request.ToAccountId, request.Amount);
        await _transactionRepository.AddAsync(transaction);
          
        await _auditRepository.LogTransferAsync(request);
          
        // Single commit point for all operations
        await _unitOfWork.CommitAsync();
    }
}
```

# Implementation in .NET 9

# Core Unit of Work Interface

```csharp
namespace ECommerce.Application.Common.Interfaces;

public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitTransactionAsync(CancellationToken cancellationToken = default);
    Task RollbackTransactionAsync(CancellationToken cancellationToken = default);
}
```

# Enhanced Unit of Work Implementation

```csharp
namespace ECommerce.Infrastructure.Data;

public class UnitOfWork : IUnitOfWork, IDisposable
{
    private readonly ECommerceDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDomainEventService _domainEventService;
    private readonly ILogger<UnitOfWork> _logger;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(
        ECommerceDbContext context,
        ICurrentUserService currentUser,
        IDomainEventService domainEventService,
        ILogger<UnitOfWork> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _domainEventService = domainEventService;
        _logger = logger;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Apply audit information
            await ApplyAuditInformationAsync();
            // Collect domain events before saving
            var domainEvents = CollectDomainEvents();
            // Save changes to database
            var result = await _context.SaveChangesAsync(cancellationToken);
            // Publish domain events after successful save
            await PublishDomainEventsAsync(domainEvents, cancellationToken);
            _logger.LogInformation("Successfully saved {Count} changes to database", result);
              
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while saving changes to database");
            throw;
        }
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction != null)
        {
            throw new InvalidOperationException("A transaction is already in progress");
        }
        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
        _logger.LogInformation("Database transaction started");
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit");
        }
        try
        {
            await SaveChangesAsync(cancellationToken);
            await _currentTransaction.CommitAsync(cancellationToken);
            _logger.LogInformation("Database transaction committed successfully");
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction == null)
        {
            return;
        }
        try
        {
            await _currentTransaction.RollbackAsync(cancellationToken);
            _logger.LogWarning("Database transaction rolled back");
        }
        finally
        {
            _currentTransaction?.Dispose();
            _currentTransaction = null;
        }
    }

    private async Task ApplyAuditInformationAsync()
    {
        var entries = _context.ChangeTracker.Entries()
            .Where(e => e.Entity is IAuditableEntity &&   
                       (e.State == EntityState.Added || e.State == EntityState.Modified))
            .ToList();
        var currentUser = _currentUser.UserId;
        var currentTime = DateTime.UtcNow;

        foreach (var entry in entries)
        {
            var auditableEntity = (IAuditableEntity)entry.Entity;
            if (entry.State == EntityState.Added)
            {
                auditableEntity.CreatedBy = currentUser;
                auditableEntity.CreatedAt = currentTime;
            }
            auditableEntity.ModifiedBy = currentUser;
            auditableEntity.ModifiedAt = currentTime;
        }
    }

    private List<IDomainEvent> CollectDomainEvents()
    {
        var domainEntities = _context.ChangeTracker.Entries<IHasDomainEvents>()
            .Where(x => x.Entity.DomainEvents.Any())
            .Select(x => x.Entity)
            .ToList();

        var domainEvents = domainEntities
            .SelectMany(x => x.DomainEvents)
            .ToList();

        // Clear events to prevent them from being raised again
        domainEntities.ForEach(entity => entity.ClearDomainEvents());
        return domainEvents;
    }

    private async Task PublishDomainEventsAsync(List<IDomainEvent> domainEvents, CancellationToken cancellationToken)
    {
        foreach (var domainEvent in domainEvents)
        {
            await _domainEventService.PublishAsync(domainEvent, cancellationToken);
        }
    }

    public void Dispose()
    {
        _currentTransaction?.Dispose();
        _context?.Dispose();
    }
}
```

# Repository Base Class

```csharp
namespace ECommerce.Infrastructure.Data.Repositories;

public abstract class RepositoryBase<T> : IRepository<T> where T : class, IAggregateRoot
{
    protected readonly ECommerceDbContext Context;
    protected readonly DbSet<T> DbSet;

    protected RepositoryBase(ECommerceDbContext context)
    {
        Context = context;
        DbSet = context.Set<T>();
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await DbSet.FindAsync(new object[] { id }, cancellationToken);
    }

    public virtual async Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await DbSet.ToListAsync(cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        DbSet.Update(entity);
    }

    public virtual void Remove(T entity)
    {
        DbSet.Remove(entity);
    }

    // Note: No SaveChanges here - responsibility belongs to Unit of Work
}
```

# Advanced Scenarios and Patterns

# CQRS with Unit of Work

In CQRS applications, the Unit of Work pattern shines particularly bright:

```csharp
namespace ECommerce.Application.Orders.Commands.CreateOrder;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<CreateOrderHandler> _logger;

    public CreateOrderHandler(
        IOrderRepository orderRepository,
        ICustomerRepository customerRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork,
        ILogger<CreateOrderHandler> logger)
    {
        _orderRepository = orderRepository;
        _customerRepository = customerRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<CreateOrderResponse> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        using var activity = _logger.BeginScope("Creating order for customer {CustomerId}", request.CustomerId);
        // Begin explicit transaction for complex operation
        await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try   
        {
            // Business logic across multiple aggregates
            var customer = await _customerRepository.GetByIdAsync(request.CustomerId, cancellationToken);
            if (customer == null)
                throw new NotFoundException($"Customer {request.CustomerId} not found");
            var products = await _productRepository.GetByIdsAsync(request.Items.Select(i => i.ProductId), cancellationToken);
              
            // Domain logic - Order aggregate validates business rules
            var order = Order.Create(customer, request.Items, products);
              
            // Repository operations (no immediate persistence)
            await _orderRepository.AddAsync(order, cancellationToken);
              
            // Update inventory
            foreach (var item in request.Items)
            {
                var product = products.First(p => p.Id == item.ProductId);
                product.ReserveStock(item.Quantity);
            }
            // Single commit point - all changes succeed or fail together
            await _unitOfWork.CommitTransactionAsync(cancellationToken);
              
            _logger.LogInformation("Order {OrderId} created successfully", order.Id);
            return new CreateOrderResponse(order.Id, order.TotalAmount);
        }
        catch   
        {
            await _unitOfWork.RollbackTransactionAsync(cancellationToken);
            throw;
        }
    }
}
```

# Multi-Context Scenarios

For applications using multiple bounded contexts with separate DbContexts:

```csharp
public interface IDistributedUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync(CancellationToken cancellationToken = default);
    Task CommitAsync(CancellationToken cancellationToken = default);
    void Enlist(IUnitOfWork unitOfWork);
}
  
public class DistributedUnitOfWork : IDistributedUnitOfWork
{
    private readonly List<IUnitOfWork> _unitOfWorks = new();
    private readonly ILogger<DistributedUnitOfWork> _logger;

    public DistributedUnitOfWork(ILogger<DistributedUnitOfWork> logger)
    {
        _logger = logger;
    }

    public void Enlist(IUnitOfWork unitOfWork)
    {
        _unitOfWorks.Add(unitOfWork);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        foreach (var unitOfWork in _unitOfWorks)
        {
            await unitOfWork.BeginTransactionAsync(cancellationToken);
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var totalChanges = 0;
          
        foreach (var unitOfWork in _unitOfWorks)
        {
            totalChanges += await unitOfWork.SaveChangesAsync(cancellationToken);
        }
          
        return totalChanges;
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var unitOfWork in _unitOfWorks)
            {
                await unitOfWork.CommitTransactionAsync(cancellationToken);
            }
              
            _logger.LogInformation("Distributed transaction committed successfully across {Count} contexts", _unitOfWorks.Count);
        }
        catch
        {
            // Rollback all contexts
            foreach (var unitOfWork in _unitOfWorks)
            {
                await unitOfWork.RollbackTransactionAsync(cancellationToken);
            }
            throw;
        }
    }
}
```

# Dependency Injection and Registration

# Service Registration in .NET 9

```csharp
// Program.cs
var builder = WebApplication.CreateSlimBuilder(args);

// Register DbContext with appropriate lifetime
builder.Services.AddDbContext<ECommerceDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Register Unit of Work with scoped lifetime (same as DbContext)
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Register repositories
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// For multi-context scenarios
builder.Services.AddScoped<IDistributedUnitOfWork, DistributedUnitOfWork>();

// Domain services
builder.Services.AddScoped<IDomainEventService, DomainEventService>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();

var app = builder.Build();
```

# Extension Method for Clean Registration

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<ECommerceDbContext>(options =>
        {
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            options.EnableSensitiveDataLogging(false);
            options.EnableDetailedErrors(false);
        });
  
        // Unit of Work
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        // Repositories
        services.Scan(scan => scan
            .FromAssemblyOf<ECommerceDbContext>()
            .AddClasses(classes => classes.AssignableTo(typeof(IRepository<>)))
            .AsImplementedInterfaces()
            .WithScopedLifetime());
        return services;
    }
}
```

# Testing Strategies

# Unit Testing with Mocked Unit of Work

```csharp
[TestClass]
public class CreateOrderHandlerTests
{
    private Mock<IOrderRepository> _mockOrderRepository;
    private Mock<ICustomerRepository> _mockCustomerRepository;
    private Mock<IProductRepository> _mockProductRepository;
    private Mock<IUnitOfWork> _mockUnitOfWork;
    private CreateOrderHandler _handler;

    [TestInitialize]
    public void Setup()
    {
        _mockOrderRepository = new Mock<IOrderRepository>();
        _mockCustomerRepository = new Mock<ICustomerRepository>();
        _mockProductRepository = new Mock<IProductRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _handler = new CreateOrderHandler(
            _mockOrderRepository.Object,
            _mockCustomerRepository.Object,
            _mockProductRepository.Object,
            _mockUnitOfWork.Object,
            Mock.Of<ILogger<CreateOrderHandler>>());
    }

    [TestMethod]
    public async Task Handle_ValidRequest_ShouldCommitTransaction()
    {
        // Arrange
        var command = new CreateOrderCommand   
        {   
            CustomerId = Guid.NewGuid(),
            Items = new List<OrderItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 2 }
            }
        };
        var customer = new Customer();
        var product = new Product();
        _mockCustomerRepository.Setup(r => r.GetByIdAsync(command.CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);
        _mockProductRepository.Setup(r => r.GetByIdsAsync(It.IsAny<IEnumerable<Guid>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Product> { product });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        _mockUnitOfWork.Verify(uow => uow.BeginTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(uow => uow.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        _mockOrderRepository.Verify(repo => repo.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()), Times.Once);
          
        Assert.IsNotNull(result);
    }

    [TestMethod]
    public async Task Handle_CustomerNotFound_ShouldRollbackTransaction()
    {
        // Arrange
        var command = new CreateOrderCommand { CustomerId = Guid.NewGuid() };
        _mockCustomerRepository.Setup(r => r.GetByIdAsync(command.CustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer)null);

        // Act & Assert
        await Assert.ThrowsExceptionAsync<NotFoundException>(
            () => _handler.Handle(command, CancellationToken.None));
        _mockUnitOfWork.Verify(uow => uow.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
```

# Integration Testing

```csharp
[TestClass]
public class UnitOfWorkIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UnitOfWorkIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [TestMethod]
    public async Task UnitOfWork_MultipleRepositoryOperations_ShouldCommitTogether()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var orderRepository = scope.ServiceProvider.GetRequiredService<IOrderRepository>();
        var customerRepository = scope.ServiceProvider.GetRequiredService<ICustomerRepository>();
        var customer = new Customer("John Doe", "john@example.com");
        var order = new Order(customer.Id, 100m);

        // Act
        await unitOfWork.BeginTransactionAsync();
          
        await customerRepository.AddAsync(customer);
        await orderRepository.AddAsync(order);
          
        var result = await unitOfWork.CommitTransactionAsync();

        // Assert
        Assert.IsTrue(result > 0);
        // Verify both entities were saved
        var savedCustomer = await customerRepository.GetByIdAsync(customer.Id);
        var savedOrder = await orderRepository.GetByIdAsync(order.Id);
          
        Assert.IsNotNull(savedCustomer);
        Assert.IsNotNull(savedOrder);
    }
}
```

# Performance Considerations

# Optimizing Unit of Work Performance in .NET 9

```csharp
public class OptimizedUnitOfWork : IUnitOfWork
{
    private readonly ECommerceDbContext _context;
    private readonly IMemoryCache _cache;
      
    // Use compiled queries for better performance
    private static readonly Func<ECommerceDbContext, IEnumerable<object>> GetTrackedEntitiesQuery =   
        EF.CompileQuery((ECommerceDbContext ctx) =>   
            ctx.ChangeTracker.Entries().Select(e => e.Entity));

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Batch operations for better performance
        var entries = _context.ChangeTracker.Entries().ToList();
          
        if (!entries.Any())
            return 0;

        // Apply audit information in batches
        ApplyAuditInformationBatch(entries);

        // Use async enumerable for large datasets
        await ProcessDomainEventsAsync(entries.AsAsyncEnumerable(), cancellationToken);

        return await _context.SaveChangesAsync(cancellationToken);
    }

    private void ApplyAuditInformationBatch(List<EntityEntry> entries)
    {
        var auditableEntries = entries
            .Where(e => e.Entity is IAuditableEntity &&   
                       (e.State == EntityState.Added || e.State == EntityState.Modified))
            .ToList();
        if (!auditableEntries.Any())
            return;

        var currentTime = DateTime.UtcNow;
        var currentUser = GetCurrentUserId(); // Assuming this method exists and gets current user ID

        // Batch update audit fields
        Parallel.ForEach(auditableEntries, entry =>
        {
            var entity = (IAuditableEntity)entry.Entity;
              
            if (entry.State == EntityState.Added)
            {
                entity.CreatedAt = currentTime;
                entity.CreatedBy = currentUser;
            }
              
            entity.ModifiedAt = currentTime;
            entity.ModifiedBy = currentUser;
        });
    }

    private async Task ProcessDomainEventsAsync(IAsyncEnumerable<EntityEntry> entries, CancellationToken cancellationToken)
    {
        var domainEvents = new List<IDomainEvent>();
        await foreach (var entry in entries.WithCancellation(cancellationToken))
        {
            if (entry.Entity is IHasDomainEvents domainEntity && domainEntity.DomainEvents.Any())
            {
                domainEvents.AddRange(domainEntity.DomainEvents);
                domainEntity.ClearDomainEvents();
            }
        }
        // Process events in parallel for better performance
        await Parallel.ForEachAsync(domainEvents, cancellationToken, async (domainEvent, ct) =>
        {
            await PublishDomainEventAsync(domainEvent, ct); // Assuming this method exists
        });
    }
}
```

# Common Pitfalls and Best Practices

# Avoiding Common Mistakes

```csharp
// ❌ DON'T: Call SaveChanges in repositories
public class BadOrderRepository : IOrderRepository
{
    private readonly ECommerceDbContext _context;

    public async Task AddAsync(Order order)
    {
        _context.Orders.Add(order);
        await _context.SaveChangesAsync(); // This breaks Unit of Work!
    }
}

// ✅ DO: Let Unit of Work manage transactions
public class GoodOrderRepository : IOrderRepository
{
    private readonly ECommerceDbContext _context;

    public async Task AddAsync(Order order)
    {
        await _context.Orders.AddAsync(order);
        // No SaveChanges - Unit of Work handles this
    }
}

// ❌ DON'T: Multiple Unit of Work instances per request
public void ConfigureServices(IServiceCollection services)
{
    services.AddTransient<IUnitOfWork, UnitOfWork>(); // Wrong lifetime!
}

// ✅ DO: Single Unit of Work per request scope
public void ConfigureServices(IServiceCollection services)
{
    services.AddScoped<IUnitOfWork, UnitOfWork>(); // Correct lifetime
}
```

# Best Practices Checklist

1.  **Single Responsibility:** Unit of