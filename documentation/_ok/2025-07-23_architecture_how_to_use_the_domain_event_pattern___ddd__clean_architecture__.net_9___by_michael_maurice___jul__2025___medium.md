```yaml
---
title: "How to Use the Domain Event Pattern | DDD, Clean Architecture, .NET 9 | by Michael Maurice | Jul, 2025 | Medium"
source: https://medium.com/@michaelmaurice410/how-to-use-the-domain-event-pattern-ddd-clean-architecture-net-9-76a6c314e610
date_published: 2025-07-23T17:01:49.934Z
date_captured: 2025-08-22T11:01:47.286Z
domain: medium.com
author: Michael Maurice
category: architecture
technologies: [.NET 9, MediatR, Entity Framework Core, RabbitMQ, Azure Service Bus]
programming_languages: [C#, SQL]
tags: [domain-driven-design, clean-architecture, domain-events, dotnet, event-driven, software-architecture, decoupling, solid-principles, mediatr, microservices]
key_concepts: [Domain Event Pattern, Domain-Driven Design (DDD), Clean Architecture, Aggregate Root, SOLID Principles, Event Dispatching, Integration Events, Bounded Context]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces the Domain Event pattern as a fundamental concept in Domain-Driven Design (DDD) and Clean Architecture, specifically within a .NET 9 context. It explains that domain events represent past occurrences within the business domain, enabling the creation of loosely coupled and scalable applications by explicitly modeling side effects. The benefits highlighted include clear implementation of side effects, decoupling of aggregates, and adherence to SOLID principles. The guide provides a step-by-step implementation, covering event definition, raising events from aggregate roots, dispatching them via `DbContext` using MediatR, and handling them in the application layer. Finally, it distinguishes between domain events and integration events, clarifying their respective roles in communication within or across bounded contexts.
---
```

# How to Use the Domain Event Pattern | DDD, Clean Architecture, .NET 9 | by Michael Maurice | Jul, 2025 | Medium

# How to Use the Domain Event Pattern | DDD, Clean Architecture, .NET 9

![Michael Maurice](https://miro.medium.com/v2/resize:fill:64:64/1*Vydee41-YhCgiyTaA_dPoA.png)

Michael Maurice

Follow

5 min read

·

Jul 23, 2025

42

1

Listen

Share

More

![Diagram illustrating Domain Events in DDD and Clean Architecture. It highlights core benefits: Explicit Side Effects, Decouples Aggregates, and Adheres to SOLID Principles. Below, a flowchart shows the implementation steps: Define Events -> Raise Events from Aggregate Root -> Dispatch Events Through Infrastructure -> Handle Events in Application Layer.](https://miro.medium.com/v2/resize:fit:522/1*So-LakU76IBW81FJJcesJA.png)

**If you want the full source code, download it from this link:** [**https://www.elitesolutions.shop/**](https://www.elitesolutions.shop/)

The Domain Event pattern is a cornerstone of Domain-Driven Design (DDD) that enables developers to build loosely coupled, scalable, and maintainable applications. When combined with Clean Architecture in .NET 9, it becomes a powerful tool for explicitly modeling the side effects of business operations, ensuring your system remains robust and easy to evolve.

# What is a Domain Event?

A domain event is a message that signifies something important has happened within the business domain. Unlike commands, which represent a request to perform an action, events represent facts that have already occurred. They are named in the past tense, such as `OrderCreated`, `PaymentProcessed`, or `UserBecamePreferred`.

In the context of Clean Architecture, domain events are crucial for decoupling logic across different parts of your domain and application. They allow an aggregate to announce a state change without being aware of what other parts of the system might need to react to that change.

# Core Benefits of Using Domain Events

*   **Explicitly Implements Side Effects**: The pattern makes the consequences of a domain change a first-class citizen in your code. Instead of hiding logic (like sending an email) inside a primary business method, you create a dedicated handler for it, making the process clear and explicit.
*   **Decouples Aggregates**: An aggregate’s responsibility ends after it validates its state and raises an event. It doesn’t need to know about other aggregates or application services that react to its changes, promoting a cleaner separation of concerns.
*   **Adheres to SOLID Principles**:
    *   **Single Responsibility Principle (SRP)**: Aggregates are responsible for enforcing their own invariants, while event handlers are responsible for handling the side effects. This separates core domain logic from application-level coordination.
    *   **Open/Closed Principle**: To add a new side effect, you can create a new event handler without modifying the existing aggregate that raises the event. This allows the system to be extended without changing tested code.

# Implementing the Domain Event Pattern in .NET 9

Implementing domain events involves three primary steps: defining and raising the event within the domain layer, dispatching it from the infrastructure layer, and handling it in the application layer.

# 1. Define the Domain Event

First, define the event itself. It’s typically an immutable class or record that carries data about what happened. A common practice is to use a marker interface like `IDomainEvent`. For easy integration with dispatching libraries, the event can also implement an interface like MediatR's `INotification`.

```csharp
// Marker interface in the Domain layer  
public interface IDomainEvent  
{  
}  

// Concrete event in the Domain layer  
public class OrderStartedDomainEvent : IDomainEvent, INotification  
{  
    public string UserId { get; }  
    public Order Order { get; }  
    
    public OrderStartedDomainEvent(Order order, string userId)  
    {  
        Order = order;  
        UserId = userId;  
    }  
}
```

# 2. Raise Events from the Aggregate Root

The aggregate root is responsible for raising events. To facilitate this, the base `AggregateRoot` class can be modified to manage a collection of events.

The deferred approach is the recommended way to handle events. Instead of dispatching them immediately, events are collected within the aggregate and dispatched later, typically just before or after the database transaction is committed. This prevents side effects from running if the main transaction fails and simplifies testing.

```csharp
// Base class for all aggregate roots in the Domain layer  
public abstract class AggregateRoot<TId> : Entity<TId>  
{  
    private readonly List<IDomainEvent> _domainEvents = new();  
  
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

// Example usage in an Order aggregate  
public class Order : AggregateRoot<OrderId>  
{  
    public static Order Create( /* parameters */ )  
    {  
        var order = new Order( /* ... */ );  
          
        // After the order is successfully created, raise the event  
        order.RaiseDomainEvent(new OrderStartedDomainEvent(order, order.UserId));  
          
        return order;  
    }  
}
```

# 3. Dispatch the Events

Dispatching the collected events is an infrastructure concern. A common place to handle this is within your `DbContext` by overriding the `SaveChangesAsync` method. This ensures that events are only dispatched if the main business operation is successfully saved to the database.

```csharp
// In the Infrastructure layer  
public class ApplicationDbContext : DbContext  
{  
    private readonly IPublisher _publisher; // MediatR's IPublisher
  
    public ApplicationDbContext(DbContextOptions options, IPublisher publisher) : base(options)  
    {  
        _publisher = publisher;  
    }  
  
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)  
    {  
        var domainEvents = ChangeTracker  
            .Entries<AggregateRoot<Guid>>()  
            .Select(e => e.Entity)  
            .Where(e => e.DomainEvents.Any())  
            .SelectMany(e => e.DomainEvents)  
            .ToList();  
        
        // Save changes to the database first  
        var result = await base.SaveChangesAsync(cancellationToken);  
        
        // After saving, dispatch the events  
        foreach (var domainEvent in domainEvents)  
        {  
            await _publisher.Publish(domainEvent, cancellationToken);  
        }  
        return result;  
    }  
}
```

# 4. Handle the Events

Event handlers reside in the Application Layer, as they orchestrate application-level logic like sending notifications, updating read models, or calling external services. The domain layer should remain pure and focused on business rules, not infrastructure concerns.

Using a library like MediatR, a handler implements the `INotificationHandler<TEvent>` interface.

```csharp
// In the Application layer  
public class SendOrderConfirmationEmailHandler : INotificationHandler<OrderStartedDomainEvent>  
{  
    private readonly IEmailService _emailService;  
    private readonly IUserRepository _userRepository;  
  
    public SendOrderConfirmationEmailHandler(IEmailService emailService, IUserRepository userRepository)  
    {  
        _emailService = emailService;  
        _userRepository = userRepository;  
    }  
    
    public async Task Handle(OrderStartedDomainEvent notification, CancellationToken cancellationToken)  
    {  
        var user = await _userRepository.GetByIdAsync(notification.UserId);  
        if (user is null) return;  
          
        var subject = "Your Order is Confirmed!";  
        var body = $"Dear {user.Name}, your order with ID {notification.Order.Id} has been placed.";  
          
        await _emailService.SendEmailAsync(user.Email, subject, body);  
    }  
}  
```

# Domain Events vs. Integration Events

It’s important to distinguish between domain events and integration events:

*   **Domain Events** are typically handled synchronously within the same process and often within the same transaction. They communicate changes between aggregates inside a single bounded context.
*   **Integration Events** are used to communicate between different bounded contexts, microservices, or applications. They are always asynchronous and rely on a message bus (like RabbitMQ or Azure Service Bus). A domain event handler can be responsible for creating and publishing an integration event.

**If you want the full source code, download it from this link:** [**https://www.elitesolutions.shop/**](https://www.elitesolutions.shop/)