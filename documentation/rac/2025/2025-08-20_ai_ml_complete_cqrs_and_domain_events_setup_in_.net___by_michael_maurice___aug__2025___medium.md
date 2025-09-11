```yaml
---
title: "Complete CQRS and Domain Events Setup in .NET | by Michael Maurice | Aug, 2025 | Medium"
source: https://medium.com/@michaelmaurice410/complete-cqrs-and-domain-events-setup-in-net-0d2b4ac0ccad
date_published: 2025-08-20T17:22:24.617Z
date_captured: 2025-09-11T11:29:12.750Z
domain: medium.com
author: Michael Maurice
category: ai_ml
technologies: [.NET, ASP.NET Core, Entity Framework Core, FluentValidation, Microsoft.Extensions.Logging, SQL Server, ConcurrentDictionary, System.Diagnostics.Stopwatch]
programming_languages: [C#, SQL]
tags: [cqrs, domain-events, dotnet, clean-architecture, domain-driven-design, validation, error-handling, entity-framework-core, web-api, pipeline]
key_concepts: [Command Query Responsibility Segregation, Domain-Driven Design, Domain Events, Result Pattern, Dependency Injection, Pipeline Behavior, Aggregate Root, Unit of Work, Custom Dispatcher, Exception Handling]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to implementing CQRS and Domain Events in .NET applications, adhering to Domain-Driven Design principles. It details the creation of core abstractions, a custom strongly-typed dispatcher, and a robust Result pattern for error handling. The guide integrates FluentValidation for a comprehensive validation pipeline and demonstrates how to manage domain events within an Entity Framework Core DbContext. It also covers pipeline behaviors for logging and performance monitoring, along with structured dependency injection and API controller examples, offering a production-ready, clean architecture solution.
---
```

# Complete CQRS and Domain Events Setup in .NET | by Michael Maurice | Aug, 2025 | Medium

# Complete CQRS and Domain Events Setup in .NET

![CQRS and Domain Events Architecture Diagram](https://miro.medium.com/v2/resize:fit:700/1*Aq-ee2SiYYmg9YS07PAvAA.png)
*A diagram illustrating the flow of Commands/Queries through a Sender, Domain Events, Handlers, and Dispatcher, with pipeline behaviors for Logging, Validation, and Performance, and integration with Entity Framework SaveChanges.*

**If you want the full source code, download it from this link:** [https://www.elitesolutions.shop/](https://www.elitesolutions.shop/)

Building a robust CQRS (Command Query Responsibility Segregation) architecture with domain events is essential for creating scalable, maintainable applications that follow Domain-Driven Design principles. This comprehensive guide walks through my complete setup for implementing CQRS with domain events in .NET, featuring a custom strongly-typed dispatcher, comprehensive validation pipeline, and production-ready error handling.

## Understanding CQRS and Domain Events

## The CQRS Pattern

CQRS separates read and write operations into distinct models, allowing for optimized handling of commands (state changes) and queries (data retrieval):

*   **Commands:** Modify application state (Create, Update, Delete operations)
*   **Queries:** Retrieve data without side effects (Read operations)

**Benefits:** Single responsibility, improved scalability, better testability

## Domain Events

Domain events represent significant business occurrences that other parts of the system should be aware of:

*   **Explicit Business Rules:** Make domain logic visible and understandable
*   **Decoupled Side Effects:** Separate core business logic from secondary concerns
*   **Event-Driven Architecture:** Enable reactive programming patterns

## Core Abstractions and Interfaces

## Foundation Interfaces

Start with clean, focused abstractions that form the foundation of the CQRS system:

```csharp
// Core marker interfaces  
public interface IRequest<out TResponse>  
{  
}  
public interface ICommand<out TResponse> : IRequest<TResponse>  
{  
}  
public interface IQuery<out TResponse> : IRequest<TResponse>  
{  
}  
// Handler interfaces  
public interface IRequestHandler<in TRequest, TResponse>  
    where TRequest : IRequest<TResponse>  
{  
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);  
}  
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, TResponse>  
    where TCommand : ICommand<TResponse>  
{  
}  
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, TResponse>  
    where TQuery : IQuery<TResponse>  
{  
}  
// Domain Events  
public interface IDomainEvent  
{  
    Guid Id { get; }  
    DateTime OccurredOn { get; }  
}  
public interface IDomainEventHandler<in TDomainEvent>  
    where TDomainEvent : IDomainEvent  
{  
    Task Handle(TDomainEvent domainEvent, CancellationToken cancellationToken);  
}  
// Sender abstraction  
public interface ISender  
{  
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);  
}  
// Publisher abstraction  
public interface IPublisher  
{  
    Task Publish<TDomainEvent>(TDomainEvent domainEvent, CancellationToken cancellationToken = default)  
        where TDomainEvent : IDomainEvent;  
      
    Task Publish(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);  
}
```

## Result Pattern Implementation

Implement a robust Result pattern for error handling:

```csharp
public class Result  
{  
    protected Result(bool isSuccess, Error error)  
    {  
        if (isSuccess && error != Error.None ||  
            !isSuccess && error == Error.None)  
        {  
            throw new ArgumentException("Invalid error", nameof(error));  
        }  
        IsSuccess = isSuccess;  
        Error = error;  
    }  
    public bool IsSuccess { get; }  
    public bool IsFailure => !IsSuccess;  
    public Error Error { get; }  
    public static Result Success() => new(true, Error.None);  
    public static Result Failure(Error error) => new(false, error);  
    public static Result<TValue> Success<TValue>(TValue value) => new(value, true, Error.None);  
    public static Result<TValue> Failure<TValue>(Error error) => new(default, false, error);  
}  
public class Result<TValue> : Result  
{  
    private readonly TValue? _value;  
    protected internal Result(TValue? value, bool isSuccess, Error error)  
        : base(isSuccess, error)  
    {  
        _value = value;  
    }  
    public TValue Value => IsSuccess  
        ? _value!  
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");  
    public static implicit operator Result<TValue>(TValue? value) =>  
        value is not null ? Success(value) : Failure<TValue>(Error.NullValue);  
    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<Error, TResult> onFailure) =>  
        IsSuccess ? onSuccess(Value) : onFailure(Error);  
}  
public record Error(string Code, string Description)  
{  
    public static readonly Error None = new(string.Empty, string.Empty);  
    public static readonly Error NullValue = new("Error.NullValue", "Null value was provided");  
    public static implicit operator Result(Error error) => Result.Failure(error);  
    public override string ToString() => $"{Code}: {Description}";  
}
```

## Custom Strongly-Typed Dispatcher

## Enhanced Domain Events Dispatcher

Build a high-performance, strongly-typed dispatcher that eliminates reflection:

```csharp
// Dispatcher interface  
public interface IDomainEventsDispatcher  
{  
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);  
}  
// Handler wrapper interface  
public interface IDomainEventHandlerWrapper  
{  
    Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken);  
}  
// Strongly-typed handler wrapper  
public sealed class DomainEventHandlerWrapper<TDomainEvent> : IDomainEventHandlerWrapper  
    where TDomainEvent : IDomainEvent  
{  
    private readonly IDomainEventHandler<TDomainEvent> _handler;  
    public DomainEventHandlerWrapper(IDomainEventHandler<TDomainEvent> handler)  
    {  
        _handler = handler;  
    }  
    public async Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken)  
    {  
        await _handler.Handle((TDomainEvent)domainEvent, cancellationToken);  
    }  
}  
// High-performance dispatcher implementation  
public sealed class DomainEventsDispatcher : IDomainEventsDispatcher  
{  
    private readonly IServiceProvider _serviceProvider;  
    private readonly ILogger<DomainEventsDispatcher> _logger;  
      
    // Cache for handler types to avoid repeated reflection  
    private static readonly ConcurrentDictionary<Type, Type[]> _handlerTypesCache = new();  
    public DomainEventsDispatcher(IServiceProvider serviceProvider, ILogger<DomainEventsDispatcher> logger)  
    {  
        _serviceProvider = serviceProvider;  
        _logger = logger;  
    }  
    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)  
    {  
        var events = domainEvents.ToList();  
          
        if (!events.Any())  
        {  
            return;  
        }  
        _logger.LogInformation("Dispatching {Count} domain events", events.Count);  
        // Group events by type for efficient processing  
        var eventsByType = events.GroupBy(e => e.GetType()).ToList();  
        foreach (var eventGroup in eventsByType)  
        {  
            var eventType = eventGroup.Key;  
            var eventsOfType = eventGroup.ToList();  
            _logger.LogDebug("Processing {Count} events of type {EventType}", eventsOfType.Count, eventType.Name);  
            // Get handler types for this event type (cached)  
            var handlerTypes = GetHandlerTypes(eventType);  
            if (!handlerTypes.Any())  
            {  
                _logger.LogWarning("No handlers found for domain event type {EventType}", eventType.Name);  
                continue;  
            }  
            // Process each event with all its handlers  
            foreach (var domainEvent in eventsOfType)  
            {  
                await ProcessEventWithHandlers(domainEvent, handlerTypes, cancellationToken);  
            }  
        }  
        _logger.LogInformation("Completed dispatching {Count} domain events", events.Count);  
    }  
    private async Task ProcessEventWithHandlers(        IDomainEvent domainEvent,   
        Type[] handlerTypes,   
        CancellationToken cancellationToken)  
    {  
        var tasks = new List<Task>();  
        foreach (var handlerType in handlerTypes)  
        {  
            // Create strongly-typed wrapper  
            var wrapper = CreateHandlerWrapper(domainEvent.GetType(), handlerType);  
              
            if (wrapper != null)  
            {  
                tasks.Add(SafeHandle(wrapper, domainEvent, cancellationToken));  
            }  
        }  
        if (tasks.Any())  
        {  
            await Task.WhenAll(tasks);  
        }  
    }  
    private IDomainEventHandlerWrapper? CreateHandlerWrapper(Type eventType, Type handlerType)  
    {  
        try  
        {  
            // Get the handler instance from DI  
            var handler = _serviceProvider.GetService(handlerType);  
            if (handler == null)  
            {  
                _logger.LogWarning("Could not resolve handler of type {HandlerType}", handlerType.Name);  
                return null;  
            }  
            // Create the wrapper type  
            var wrapperType = typeof(DomainEventHandlerWrapper<>).MakeGenericType(eventType);  
              
            // Create wrapper instance  
            return (IDomainEventHandlerWrapper)Activator.CreateInstance(wrapperType, handler)!;  
        }  
        catch (Exception ex)  
        {  
            _logger.LogError(ex, "Error creating handler wrapper for {HandlerType}", handlerType.Name);  
            return null;  
        }  
    }  
    private Type[] GetHandlerTypes(Type eventType)  
    {  
        return _handlerTypesCache.GetOrAdd(eventType, type =>  
        {  
            var handlerInterfaceType = typeof(IDomainEventHandler<>).MakeGenericType(type);  
              
            // Find all registered services that implement the handler interface  
            return _serviceProvider.GetServices(handlerInterfaceType)  
                .Select(handler => handler.GetType())  
                .ToArray();  
        });  
    }  
    private async Task SafeHandle(        IDomainEventHandlerWrapper wrapper,   
        IDomainEvent domainEvent,   
        CancellationToken cancellationToken)  
    {  
        try  
        {  
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();  
              
            await wrapper.Handle(domainEvent, cancellationToken);  
              
            stopwatch.Stop();  
              
            _logger.LogDebug("Handler {HandlerType} processed {EventType} in {Duration}ms",  
                wrapper.GetType().Name, domainEvent.GetType().Name, stopwatch.ElapsedMilliseconds);  
        }  
        catch (Exception ex)  
        {  
            _logger.LogError(ex, "Error handling domain event {EventType} with handler {HandlerType}",  
                domainEvent.GetType().Name, wrapper.GetType().Name);  
              
            // Decide whether to rethrow or continue processing other handlers  
            // For resilience, we'll log and continue  
        }  
    }  
}
```

## Request/Response Handler Implementation

## Sender Implementation

Create a robust sender that handles commands and queries:

```csharp
public sealed class Sender : ISender  
{  
    private readonly IServiceProvider _serviceProvider;  
    private readonly ILogger<Sender> _logger;  
      
    // Cache for handler types  
    private static readonly ConcurrentDictionary<Type, Type> _handlerTypeCache = new();  
    public Sender(IServiceProvider serviceProvider, ILogger<Sender> logger)  
    {  
        _serviceProvider = serviceProvider;  
        _logger = logger;  
    }  
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)  
    {  
        ArgumentNullException.ThrowIfNull(request);  
        var requestType = request.GetType();  
        var responseType = typeof(TResponse);  
        _logger.LogDebug("Sending request {RequestType} expecting response {ResponseType}",   
            requestType.Name, responseType.Name);  
        var handlerType = GetHandlerType(requestType, responseType);  
          
        var handler = _serviceProvider.GetService(handlerType);  
          
        if (handler == null)  
        {  
            throw new InvalidOperationException($"Handler not found for request type {requestType.Name}");  
        }  
        try  
        {  
            var handleMethod = handlerType.GetMethod("Handle");  
            if (handleMethod == null)  
            {  
                throw new InvalidOperationException($"Handle method not found on handler {handlerType.Name}");  
            }  
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();  
              
            var result = await (Task<TResponse>)handleMethod.Invoke(handler, new object[] { request, cancellationToken })!;  
              
            stopwatch.Stop();  
              
            _logger.LogInformation("Request {RequestType} handled in {Duration}ms",   
                requestType.Name, stopwatch.ElapsedMilliseconds);  
            return result;  
        }  
        catch (Exception ex)  
        {  
            _logger.LogError(ex, "Error handling request {RequestType}", requestType.Name);  
            throw;  
        }  
    }  
    private Type GetHandlerType(Type requestType, Type responseType)  
    {  
        var cacheKey = requestType;  
          
        return _handlerTypeCache.GetOrAdd(cacheKey, _ =>  
        {  
            // Check for command handler  
            var commandHandlerType = typeof(ICommandHandler<,>).MakeGenericType(requestType, responseType);  
            var commandHandler = _serviceProvider.GetService(commandHandlerType);  
            if (commandHandler != null)  
            {  
                return commandHandlerType;  
            }  
            // Check for query handler  
            var queryHandlerType = typeof(IQueryHandler<,>).MakeGenericType(requestType, responseType);  
            var queryHandler = _serviceProvider.GetService(queryHandlerType);  
            if (queryHandler != null)  
            {  
                return queryHandlerType;  
            }  
            // Check for generic request handler  
            var requestHandlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, responseType);  
            var requestHandler = _serviceProvider.GetService(requestHandlerType);  
            if (requestHandler != null)  
            {  
                return requestHandlerType;  
            }  
            throw new InvalidOperationException($"No handler registered for request type {requestType.Name}");  
        });  
    }  
}
```

## Domain Entity Base Classes

## Aggregate Root with Domain Events

Implement a robust aggregate root that manages domain events:

```csharp
// Domain event accumulator interface  
public interface IDomainEventAccumulator  
{  
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }  
    void ClearDomainEvents();  
}  
// Base domain event  
public abstract record DomainEvent(Guid Id, DateTime OccurredOn) : IDomainEvent  
{  
    protected DomainEvent() : this(Guid.NewGuid(), DateTime.UtcNow)  
    {  
    }  
}  
// Entity base class  
public abstract class Entity<TId> : IEquatable<Entity<TId>>  
    where TId : class  
{  
    protected Entity(TId id)  
    {  
        Id = id;  
    }  
    public TId Id { get; protected set; }  
    public override bool Equals(object? obj)  
    {  
        return obj is Entity<TId> entity && Id.Equals(entity.Id);  
    }  
    public bool Equals(Entity<TId>? other)  
    {  
        return Equals((object?)other);  
    }  
    public static bool operator ==(Entity<TId> left, Entity<TId> right)  
    {  
        return Equals(left, right);  
    }  
    public static bool operator !=(Entity<TId> left, Entity<TId> right)  
    {  
        return !Equals(left, right);  
    }  
    public override int GetHashCode()  
    {  
        return Id.GetHashCode();  
    }  
}  
// Aggregate root with domain events  
public abstract class AggregateRoot<TId> : Entity<TId>, IDomainEventAccumulator  
    where TId : class  
{  
    private readonly List<IDomainEvent> _domainEvents = new();  
    protected AggregateRoot(TId id) : base(id)  
    {  
    }  
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();  
    protected void RaiseDomainEvent(IDomainEvent domainEvent)  
    {  
        _domainEvents.Add(domainEvent);  
    }  
    public void ClearDomainEvents()  
    {  
        _domainEvents.Clear();  
    }  
}
```

## Validation Pipeline Implementation

## FluentValidation Integration

Implement comprehensive validation using FluentValidation:

```csharp
// Validation behavior  
public sealed class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>  
    where TRequest : IRequest<TResponse>  
{  
    private readonly IEnumerable<IValidator<TRequest>> _validators;  
    private readonly ILogger<ValidationBehavior<TRequest, TResponse>> _logger;  
    public ValidationBehavior(        IEnumerable<IValidator<TRequest>> validators,  
        ILogger<ValidationBehavior<TRequest, TResponse>> logger)  
    {  
        _validators = validators;  
        _logger = logger;  
    }  
    public async Task<TResponse> Handle(        TRequest request,  
        RequestHandlerDelegate<TResponse> next,  
        CancellationToken cancellationToken)  
    {  
        var requestName = typeof(TRequest).Name;  
        _logger.LogDebug("Validating request {RequestName}", requestName);  
        if (!_validators.Any())  
        {  
            _logger.LogDebug("No validators found for {RequestName}", requestName);  
            return await next();  
        }  
        var context = new ValidationContext<TRequest>(request);  
        var validationTasks = _validators.Select(validator =>   
            validator.ValidateAsync(context, cancellationToken));  
        var validationResults = await Task.WhenAll(validationTasks);  
        var failures = validationResults  
            .Where(result => !result.IsValid)  
            .SelectMany(result => result.Errors)  
            .ToList();  
        if (failures.Any())  
        {  
            _logger.LogWarning("Validation failed for {RequestName}. Errors: {@ValidationErrors}",   
                requestName, failures.Select(f => new { f.PropertyName, f.ErrorMessage }));  
            throw new ValidationException(failures);  
        }  
        _logger.LogDebug("Validation passed for {RequestName}", requestName);  
        return await next();  
    }  
}  
// Custom validation exception  
public sealed class ValidationException : Exception  
{  
    public ValidationException(IEnumerable<ValidationFailure> failures)  
        : base("Validation failed")  
    {  
        Errors = failures  
            .GroupBy(f => f.PropertyName)  
            .ToDictionary(  
                group => group.Key,  
                group => group.Select(f => f.ErrorMessage).ToArray());  
    }  
    public IDictionary<string, string[]> Errors { get; }  
}  
// Pipeline behavior interface  
public interface IPipelineBehavior<in TRequest, TResponse>  
    where TRequest : IRequest<TResponse>  
{  
    Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken);  
}  
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();
```

## Additional Pipeline Behaviors

Implement logging and performance monitoring behaviors:

```csharp
// Logging behavior  
public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>  
    where TRequest : IRequest<TResponse>  
{  
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;  
    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)  
    {  
        _logger = logger;  
    }  
    public async Task<TResponse> Handle(        TRequest request,  
        RequestHandlerDelegate<TResponse> next,  
        CancellationToken cancellationToken)  
    {  
        var requestName = typeof(TRequest).Name;  
        var requestId = Guid.NewGuid();  
        _logger.LogInformation("Starting request {RequestName} with ID {RequestId}", requestName, requestId);  
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();  
        try  
        {  
            var response = await next();  
            stopwatch.Stop();  
            _logger.LogInformation("Completed request {RequestName} with ID {RequestId} in {Duration}ms",  
                requestName, requestId, stopwatch.ElapsedMilliseconds);  
            return response;  
        }  
        catch (Exception ex)  
        {  
            stopwatch.Stop();  
            _logger.LogError(ex, "Request {RequestName} with ID {RequestId} failed after {Duration}ms",  
                requestName, requestId, stopwatch.ElapsedMilliseconds);  
            throw;  
        }  
    }  
}  
// Performance behavior  
public sealed class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>  
    where TRequest : IRequest<TResponse>  
{  
    private readonly ILogger<PerformanceBehavior<TRequest, TResponse>> _logger;  
    private readonly IMetrics _metrics; // IMetrics is an abstraction, specific implementation not shown  
    public PerformanceBehavior(        ILogger<PerformanceBehavior<TRequest, TResponse>> logger,  
        IMetrics metrics)  
    {  
        _logger = logger;  
        _metrics = metrics;  
    }  
    public async Task<TResponse> Handle(        TRequest request,  
        RequestHandlerDelegate<TResponse> next,  
        CancellationToken cancellationToken)  
    {  
        var requestName = typeof(TRequest).Name;  
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();  
        try  
        {  
            var response = await next();  
            stopwatch.Stop();  
            // Record metrics  
            _metrics.Counter("requests_total")  
                .WithTag("request_type", requestName)  
                .WithTag("status", "success")  
                .Increment();  
            _metrics.Histogram("request_duration_ms")  
                .WithTag("request_type", requestName)  
                .Record(stopwatch.ElapsedMilliseconds);  
            // Log slow requests  
            if (stopwatch.ElapsedMilliseconds > 500)  
            {  
                _logger.LogWarning("Slow request detected: {RequestName} took {Duration}ms",  
                    requestName, stopwatch.ElapsedMilliseconds);  
            }  
            return response;  
        }  
        catch (Exception)  
        {  
            stopwatch.Stop();  
            _metrics.Counter("requests_total")  
                .WithTag("request_type", requestName)  
                .WithTag("status", "error")  
                .Increment();  
            throw;  
        }  
    }  
}
```

## Entity Framework Integration

## DbContext with Domain Events

Integrate domain events with Entity Framework Core:

```csharp
public sealed class ApplicationDbContext : DbContext, IUnitOfWork  
{  
    private readonly IDomainEventsDispatcher _domainEventsDispatcher;  
    private readonly ILogger<ApplicationDbContext> _logger;  
    public ApplicationDbContext(        DbContextOptions<ApplicationDbContext> options,  
        IDomainEventsDispatcher domainEventsDispatcher,  
        ILogger<ApplicationDbContext> logger)   
        : base(options)  
    {  
        _domainEventsDispatcher = domainEventsDispatcher;  
        _logger = logger;  
    }  
    public DbSet<User> Users => Set<User>();  
    public DbSet<Product> Products => Set<Product>();  
    public DbSet<Order> Orders => Set<Order>();  
    protected override void OnModelCreating(ModelBuilder modelBuilder)  
    {  
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);  
          
        base.OnModelCreating(modelBuilder);  
    }  
    public async Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default)  
    {  
        // Get domain events before saving  
        var domainEvents = GetDomainEvents();  
        _logger.LogDebug("Found {Count} domain events to dispatch", domainEvents.Count);  
        // Save changes first  
        var result = await base.SaveChangesAsync(cancellationToken) > 0;  
        // Dispatch domain events after successful save  
        if (result && domainEvents.Any())  
        {  
            await _domainEventsDispatcher.DispatchAsync(domainEvents, cancellationToken);  
        }  
        return result;  
    }  
    private List<IDomainEvent> GetDomainEvents()  
    {  
        var domainEvents = new List<IDomainEvent>();  
        var aggregateRoots = ChangeTracker  
            .Entries<IDomainEventAccumulator>()  
            .Where(entry => entry.Entity.DomainEvents.Any())  
            .Select(entry => entry.Entity)  
            .ToList();  
        foreach (var aggregateRoot in aggregateRoots)  
        {  
            domainEvents.AddRange(aggregateRoot.DomainEvents);  
            aggregateRoot.ClearDomainEvents();  
        }  
        return domainEvents;  
    }  
}  
// Unit of work interface  
public interface IUnitOfWork  
{  
    Task<bool> SaveEntitiesAsync(CancellationToken cancellationToken = default);  
}
```

## Complete Implementation Example

## Domain Model Example

Create a complete domain model with events:

```csharp
// Domain events  
public sealed record UserRegisteredEvent(    Guid UserId,  
    string Email,  
    string FirstName,  
    string LastName) : DomainEvent;  
public sealed record UserEmailChangedEvent(    Guid UserId,  
    string OldEmail,  
    string NewEmail) : DomainEvent;  
// User aggregate  
public sealed class User : AggregateRoot<UserId>  
{  
    private User() : base(UserId.Empty) { } // EF constructor  
    private User(UserId id, string email, string firstName, string lastName, string passwordHash)  
        : base(id)  
    {  
        Email = email;  
        FirstName = firstName;  
        LastName = lastName;  
        PasswordHash = passwordHash;  
        CreatedAt = DateTime.UtcNow;  
        IsActive