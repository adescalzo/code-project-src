# What Is An Entity? | Domain-Driven Design, Clean Architecture, .NET 9 | by Michael Maurice | Jul, 2025 | Medium

**Source:** https://medium.com/@michaelmaurice410/what-is-an-entity-domain-driven-design-clean-architecture-net-9-024cfc9b2224
**Date Captured:** 2025-07-28T16:13:21.955Z
**Domain:** medium.com
**Author:** Michael Maurice
**Category:** architecture

---

Member-only story

# What Is An Entity? | Domain-Driven Design, Clean Architecture, .NET 9

[

![Michael Maurice](https://miro.medium.com/v2/resize:fill:64:64/1*Vydee41-YhCgiyTaA_dPoA.png)





](/@michaelmaurice410?source=post_page---byline--024cfc9b2224---------------------------------------)

[Michael Maurice](/@michaelmaurice410?source=post_page---byline--024cfc9b2224---------------------------------------)

Follow

6 min read

·

Jul 19, 2025

17

1

Listen

Share

More

An Entity in Domain-Driven Design represents one of the most fundamental building blocks for creating robust, business-focused applications. Understanding entities is crucial when implementing Clean Architecture patterns in .NET 9, as they form the core of your domain layer and embody your business logic.

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/0*8jTBiai1GkH8M1nn.png)

Visual representation of an Entity in Domain-Driven Design showing its key characteristics  
If you want the full source code, download it from this link: [https://www.elitesolutions.shop/](https://www.elitesolutions.shop/)

# The Core Definition of an Entity

An Entity is a domain object that is defined by its unique identity rather than its attributes. This fundamental characteristic distinguishes entities from other domain objects and makes them essential for modeling real-world business concepts that need to be tracked and managed over time.

The key principle is simple yet powerful: two entities are considered the same if they have the same identity, regardless of whether their attributes differ. This identity-based equality allows entities to maintain continuity throughout their lifecycle, even as their state changes.

# Essential Characteristics of Entities

# Unique and Immutable Identity

Every entity must possess a unique identifier that never changes throughout its lifetime. This identity serves as the entity’s fingerprint, allowing the system to distinguish it from all other entities, even those of the same type. The identity should be:

*   Unique: No two entities can share the same identifier within the same bounded context
*   Immutable: Once assigned, the identity must never change
*   Stable: The identity should remain consistent across different system states and time periods

# Mutable State with Business Logic

Unlike Value Objects, entities are designed to change over time. They encapsulate business rules that govern how and when these changes can occur. This mutability is not arbitrary but follows strict business constraints defined within the entity itself.

# Lifecycle Management

Entities have a distinct lifecycle that typically includes creation, modification, persistence, and potentially deletion or archiving. During this lifecycle, the entity may transition between different states while maintaining its core identity.

# Continuity and Thread of Identity

As Eric Evans describes, entities maintain “a thread of continuity and identity”. This means that regardless of how an entity’s attributes change, it remains the same conceptual object throughout its existence.

# Implementation in .NET 9

# Base Entity Class Structure

Here’s how to implement a robust base Entity class in .NET 9:

public abstract class Entity<TId\> : IEquatable<Entity<TId\>> where TId : notnull  
{  
    private readonly List<IDomainEvent> \_domainEvents = new();  
      
    public TId Id { get; protected set; }  
      
    protected Entity(TId id)  
    {  
        Id = id;  
    }  
      
    // Parameterless constructor for EF Core  
    protected Entity() { }  
      
    public IReadOnlyList<IDomainEvent> DomainEvents => \_domainEvents.AsReadOnly();  
      
    public void AddDomainEvent(IDomainEvent domainEvent)  
    {  
        \_domainEvents.Add(domainEvent);  
    }  
      
    public void ClearDomainEvents()  
    {  
        \_domainEvents.Clear();  
    }  
      
    public override bool Equals(object? obj)  
    {  
        return obj is Entity<TId> entity && Equals(entity);  
    }  
      
    public bool Equals(Entity<TId>? other)  
    {  
        if (other is null) return false;  
        if (ReferenceEquals(this, other)) return true;  
        if (GetType() != other.GetType()) return false;  
          
        return EqualityComparer<TId>.Default.Equals(Id, other.Id);  
    }  
      
    public override int GetHashCode()  
    {  
        return EqualityComparer<TId>.Default.GetHashCode(Id);  
    }  
      
    public static bool operator ==(Entity<TId>? left, Entity<TId>? right)  
    {  
        return Equals(left, right);  
    }  
      
    public static bool operator !=(Entity<TId>? left, Entity<TId>? right)  
    {  
        return !Equals(left, right);  
    }  
}

# Concrete Entity Implementation

Here’s a practical example of a Customer entity that demonstrates rich business logic:

public sealed class Customer : Entity<CustomerId\>  
{  
    private readonly List<Order> \_orders = new();  
      
    public string FirstName { get; private set; }  
    public string LastName { get; private set; }  
    public Email Email { get; private set; }  
    public CustomerStatus Status { get; private set; }  
    public DateTime CreatedAt { get; private set; }  
    public DateTime? LastLoginAt { get; private set; }  
      
    public IReadOnlyList<Order> Orders => \_orders.AsReadOnly();  
      
    private Customer(CustomerId id, string firstName, string lastName, Email email)   
        : base(id)  
    {  
        FirstName = firstName;  
        LastName = lastName;  
        Email = email;  
        Status = CustomerStatus.Active;  
        CreatedAt = DateTime.UtcNow;  
          
        AddDomainEvent(new CustomerCreatedDomainEvent(Id, Email));  
    }  
      
    public static Customer Create(string firstName, string lastName, Email email)  
    {  
        // Business rule validation  
        if (string.IsNullOrWhiteSpace(firstName))  
            throw new DomainException("First name cannot be empty");  
          
        if (string.IsNullOrWhiteSpace(lastName))  
            throw new DomainException("Last name cannot be empty");  
          
        return new Customer(new CustomerId(Guid.NewGuid()), firstName, lastName, email);  
    }  
      
    public void UpdateContactInformation(string firstName, string lastName, Email email)  
    {  
        // Business rule: Cannot update contact info for inactive customers  
        if (Status == CustomerStatus.Inactive)  
            throw new DomainException("Cannot update contact information for inactive customer");  
          
        FirstName = firstName;  
        LastName = lastName;  
        Email = email;  
          
        AddDomainEvent(new CustomerContactUpdatedDomainEvent(Id, Email));  
    }  
      
    public void Deactivate(string reason)  
    {  
        if (Status == CustomerStatus.Inactive)  
            throw new DomainException("Customer is already inactive");  
          
        if (\_orders.Any(o => o.Status == OrderStatus.Processing))  
            throw new DomainException("Cannot deactivate customer with processing orders");  
          
        Status = CustomerStatus.Inactive;  
        AddDomainEvent(new CustomerDeactivatedDomainEvent(Id, reason));  
    }  
      
    public void RecordLogin()  
    {  
        if (Status == CustomerStatus.Inactive)  
            throw new DomainException("Inactive customer cannot login");  
          
        LastLoginAt = DateTime.UtcNow;  
    }  
      
    public void AddOrder(Order order)  
    {  
        if (Status == CustomerStatus.Inactive)  
            throw new DomainException("Cannot add orders for inactive customer");  
          
        \_orders.Add(order);  
    }  
}

# Strongly Typed Identity

Implement strongly typed identities to prevent mixing different entity types:

public readonly record struct CustomerId(Guid Value)  
{  
    public static CustomerId New() => new(Guid.NewGuid());  
    public static CustomerId Empty => new(Guid.Empty);  
      
    public override string ToString() => Value.ToString();  
}  
  
public readonly record struct OrderId(Guid Value)  
{  
    public static OrderId New() => new(Guid.NewGuid());  
    public static OrderId Empty => new(Guid.Empty);  
      
    public override string ToString() => Value.ToString();  
}

# Entity vs Value Object: Key Distinctions

Understanding when to use entities versus value objects is crucial for proper domain modeling:

AspectEntityValue ObjectIdentityHas unique, immutable identityNo identity; defined by attributesEqualityCompared by IDCompared by all attributesMutabilityMutable state with business rulesImmutable once createdLifecycleHas distinct lifecycleNo independent lifecyclePersistenceDirectly persistablePersisted as part of entitiesBusiness LogicRich behavior and business rulesSimple behavior, mostly calculations

# Best Practices for Entity Design

# Encapsulation and Invariants

Entities should encapsulate their internal state and expose behavior through well-defined methods. Never allow direct access to mutable fields; instead, provide methods that enforce business rules and maintain invariants.

# Rich Domain Behavior

Place business logic as close to the data it operates on as possible. Entities should be the first place you consider for implementing domain rules and behaviors that naturally belong to that concept.

# Domain Events for Side Effects

Use domain events to handle side effects and maintain loose coupling between aggregates. When an entity’s state changes in a way that other parts of the system need to know about, raise a domain event.

# Factory Methods for Complex Creation

Use static factory methods to handle complex entity creation logic while maintaining encapsulation. This ensures that entities are always created in a valid state.

# Entity Lifecycle in Clean Architecture

In Clean Architecture with .NET 9, entities flow through different layers while maintaining their core identity:

1.  Domain Layer: Entities are created with business logic and rules
2.  Application Layer: Use cases orchestrate entity operations
3.  Infrastructure Layer: Repositories handle persistence concerns
4.  Presentation Layer: Entities are projected to DTOs for external communication

The key principle is that entities remain unchanged as they flow through these layers — they maintain their business logic and identity regardless of how they’re persisted or presented.

# Common Pitfalls to Avoid

# Anemic Domain Models

Avoid creating entities that are merely data containers without behavior. Entities should be rich in business logic, not simple property bags.

# Breaking Encapsulation

Don’t expose internal collections or allow direct manipulation of entity state. Always provide controlled access through well-designed methods.

# Identity Management Issues

Never change an entity’s identity after creation. Also, be careful about identity assignment — consider whether to generate IDs in the domain or let the infrastructure handle it.

# Mixing Concerns

Keep entities focused on business logic. Don’t add persistence, serialization, or presentation concerns directly to entity classes.Entities are the cornerstone of domain-driven design and clean architecture. When implemented correctly in .NET 9, they create a robust foundation for business logic that can evolve and adapt to changing requirements while maintaining consistency and integrity. By focusing on identity, encapsulation, and rich behavior, entities become powerful tools for expressing complex business domains in code that is both maintainable and expressive.  
If you want the full source code, download it from this link: [https://www.elitesolutions.shop/](https://www.elitesolutions.shop/)