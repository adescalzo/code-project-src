```yaml
---
title: "Mastering CQRS and Event Sourcing in .NET 8: From Theory to Production-Ready Implementation (Part 2) | by Bhargava Koya - Fullstack .NET Developer | Aug, 2025 | Medium"
source: https://medium.com/@bhargavkoya56/mastering-cqrs-and-event-sourcing-in-net-74248fc01b93
date_published: 2025-08-01T19:59:16.970Z
date_captured: 2025-09-08T11:31:11.752Z
domain: medium.com
author: Bhargava Koya - Fullstack .NET Developer
category: general
technologies: [.NET 8, ASP.NET Core, Entity Framework Core, SQL Server, MediatR, System.Text.Json, Microsoft.Extensions.Logging, Microsoft.Extensions.Caching.Memory, Debezium, EventStoreDB]
programming_languages: [C#, SQL]
tags: [cqrs, event-sourcing, dotnet, architecture, design-patterns, data-persistence, audit-trail, scalability, web-api, database]
key_concepts: [Command Query Responsibility Segregation, Event Sourcing, Domain Events, Event Store, Aggregates, Read Models, Snapshotting, Event Versioning]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article, Part 2 of a series, delves into Event Sourcing in .NET 8, building upon the concepts of CQRS. It thoroughly explains Event Sourcing fundamentals, including events, event stores, aggregates, and projections, highlighting its benefits for auditability, temporal queries, and debugging. The content provides detailed C# implementation examples for event design, event store, aggregate management, and the repository pattern. It further demonstrates the powerful integration of CQRS with Event Sourcing through Web API controllers, command/query handlers, and event-driven read model updates. The article concludes with advanced techniques like snapshotting, event versioning, projection rebuilding, and best practices for performance and operational excellence.]
---
```

# Mastering CQRS and Event Sourcing in .NET 8: From Theory to Production-Ready Implementation (Part 2) | by Bhargava Koya - Fullstack .NET Developer | Aug, 2025 | Medium

# Mastering CQRS and Event Sourcing in .NET 8: From Theory to Production-Ready Implementation (Part 2)

![Bhargava Koya - Fullstack .NET Developer](https://miro.medium.com/v2/resize:fill:64:64/1*mXaNHb5mqlRTxc-4SqZPyA.jpeg)

Bhargava Koya - Fullstack .NET Developer

Follow

24 min read

·

Aug 1, 2025

1

Listen

Share

More

Press enter or click to view image in full size

![Title image for the article "Mastering CQRS and Event Sourcing in .NET 8: From Theory to Production-Ready Implementation"](https://miro.medium.com/v2/resize:fit:700/0*mN_q6Lyy91Uxa9vU.png)

### **Building Audit-Rich, Time-Travel Capable Applications with Event Sourcing**

Welcome back to our comprehensive exploration of modern architectural patterns in .NET 8. In Part 1, we mastered Command Query Responsibility Segregation (CQRS) and learned how separating reads from writes can dramatically improve application performance and scalability. Now, we venture into the fascinating world of Event Sourcing, a pattern that fundamentally changes how we think about data persistence by capturing the complete history of changes as immutable events.

Event Sourcing transforms your application’s relationship with data from storing **“what is” to preserving “what happened.”** This shift unlocks powerful capabilities: complete audit trails, time-travel queries, and self-healing systems that can reconstruct any historical state. When combined with CQRS, these patterns create enterprise applications capable of handling the most demanding business requirements with unprecedented visibility and reliability.

### Event Sourcing Fundamentals

Event Sourcing takes a radically different approach to data persistence by storing the complete history of changes rather than just the current state. Instead of updating records in place, every modification becomes an immutable event appended to an event log, from which the current state can be derived by replaying all events in sequence.

#### -How Event Sourcing Works?

The fundamental concept is elegantly simple yet powerful in practice. When a business operation occurs, instead of updating database fields directly, the system generates and stores an event representing what happened. For example, rather than updating a bank account balance from $500 to $300, Event Sourcing records a “**MoneyWithdrawnEvent**” with amount $200 and timestamp.

The current state is reconstructed by replaying all events in chronological order. This approach provides complete auditability every change is preserved forever and enables powerful capabilities like time travel queries (viewing system state at any point in history) and event replay for testing or recovery scenarios.

#### -Event Sourcing Building Blocks

Successful Event Sourcing implementations rely on several key components:
- **Events:** Immutable records representing business facts that have occurred. Events are always named in past tense (**OrderCreated**, **PaymentProcessed**) and contain all necessary data to reconstruct state changes.
- **Event Store:** A specialized database optimized for append-only operations and sequential reads. Unlike traditional databases, event stores never update or delete events, they only append new ones.
- **Aggregates:** Domain objects that enforce business rules and generate events in response to commands. Aggregates are reconstructed from their event history before processing new commands.
- **Projections:** Read models built by processing event streams to create optimized views for queries. These can be materialized in various formats and technologies based on query requirements.

### Event Sourcing Concepts We’ll Cover

Our comprehensive exploration of Event Sourcing in .NET 8 will include:

*   **Event Design Patterns:** Creating meaningful, evolvable event schemas.
*   **Aggregate Implementation:** Building domain objects that generate and apply events.
*   **Event Store Mechanics:** Persisting and retrieving event streams efficiently.
*   **Snapshot Strategies:** Optimizing performance for aggregates with long histories.
*   **Event Versioning:** Handling schema evolution without breaking existing events.
*   **Projection Building:** Creating and maintaining read models from event streams.
*   **Concurrency Control:** Managing concurrent modifications to the same aggregate.
*   **Event Replay Mechanisms:** Rebuilding system state and projections from events.

### Problems Event Sourcing Solves

Event Sourcing addresses fundamental business and technical challenges that traditional approaches struggle to handle effectively. Understanding these problems helps architects recognize when Event Sourcing provides genuine value versus when it introduces unnecessary complexity.

#### -Complete Auditability and Compliance

*   Modern enterprises face increasingly stringent regulatory compliance requirements that demand complete audit trails of all business operations. Traditional systems struggle to provide this visibility without significant performance overhead and storage costs. Financial services, healthcare, and government sectors often require comprehensive historical records that can withstand legal scrutiny.
*   Event Sourcing transforms audit trails from a secondary concern into a primary architectural benefit. Since all changes are preserved as events, complete historical information is available by design. This approach satisfies even the most demanding compliance requirements while providing business value through historical analytics and debugging capabilities.
*   The immutable nature of events provides cryptographic-grade auditability. Events can be digitally signed and their integrity verified, providing legal-grade evidence of what occurred and when. This level of auditability is virtually impossible to achieve with traditional update-in-place approaches.

#### -Temporal Query Requirements

*   Enterprise applications increasingly need to answer historical questions that traditional systems cannot address without complex and expensive workarounds. Questions like “What was the customer’s account balance at the end of last quarter?” or “Which permissions did this user have when they performed this action?” require maintaining historical snapshots or change logs.
*   Event Sourcing makes time-travel queries natural and efficient. Since all events are preserved with timestamps, the system can replay events up to any point in time to reconstruct historical state. This capability enables sophisticated analytics, regulatory reporting, and debugging scenarios that would be prohibitively expensive with traditional approaches.

#### -Debugging and Root Cause Analysis

*   Complex enterprise systems often experience issues that are difficult to reproduce and diagnose using traditional approaches. When problems occur, teams need to understand not just what the current state is, but how the system arrived at that state through a series of business operations.
*   Event Sourcing provides complete system observability by preserving every state change as an event. When issues arise, developers can replay the exact sequence of events that led to the problem, enabling precise root cause analysis and reliable bug reproduction. This capability dramatically reduces mean time to resolution for production issues.

#### - Business Process Modeling and Workflow Management

*   Many enterprise applications involve complex business processes that span multiple steps, require approval workflows, or may need to be reversed or compensated. Traditional approaches struggle to model these processes clearly and often resort to complex state machines or workflow engines.
*   Event Sourcing naturally aligns with business process thinking by capturing each step as a domain event. Business workflows become explicit through event sequences, making processes easier to understand, modify, and optimize. The immutable event history also enables safe experimentation with process changes through event replay and projection rebuilding.

### When to Use Event Sourcing

Making informed decisions about Event Sourcing adoption requires understanding specific scenarios where this pattern provides clear value versus situations where it introduces unnecessary complexity.

#### -Strong Audit Trail Requirements

*   Event Sourcing excels in domains where complete audit trails are essential. Financial services, healthcare, and regulatory environments often require comprehensive historical records that traditional systems struggle to provide without significant overhead. Event Sourcing makes auditability a core architectural benefit rather than an expensive add-on feature.
*   Consider a trading system that must maintain complete records of every order, modification, and execution for regulatory compliance. Traditional approaches require separate audit tables, change tracking mechanisms, and complex synchronization logic. Event Sourcing captures this information naturally as part of normal business operations.

#### -Complex Business Logic and State Transitions

*   Applications with sophisticated business rules that involve multiple state transitions, approval workflows, or process orchestration benefit significantly from Event Sourcing’s explicit state modeling. The pattern makes business logic visible through event sequences and enables reliable process management.
*   Insurance claim processing exemplifies this scenario: claims move through investigation, approval, payment, and dispute resolution stages, each with complex business rules and potential rollback scenarios. Event Sourcing captures each transition as a domain event, making the entire process transparent and manageable.

#### - Temporal Analysis and Historical Reporting

*   Systems requiring sophisticated historical analysis or the ability to answer questions about past states find Event Sourcing invaluable. Traditional approaches often lose historical context or require expensive data warehousing solutions to maintain time-based views.
*   Customer relationship management systems that need to track customer lifecycle progression, campaign effectiveness over time, or account status at specific moments benefit dramatically from Event Sourcing’s natural time-travel capabilities.

#### -High-Performance Write Scenarios

*   Event Sourcing can provide superior write performance in high-throughput scenarios because events are only appended, never updated. This eliminates lock contention, reduces complex constraint checking, and enables massive write scalability that traditional update-in-place approaches cannot match.
*   IoT applications collecting sensor data, financial systems processing transactions, or social media platforms capturing user interactions often find Event Sourcing’s append-only nature enables throughput levels impossible with traditional approaches.

### When NOT to Use Event Sourcing

Understanding inappropriate use cases is crucial for architectural decision-making:

**-Simple CRUD Applications:** Systems with straightforward business logic and minimal audit requirements rarely justify Event Sourcing’s complexity. The overhead of event design, storage, and projection management often outweighs benefits in basic scenarios.

**-Limited Storage Resources:** Event Sourcing requires storing complete event history, which can consume significant storage over time. Organizations with strict storage constraints or cost sensitivities should carefully evaluate this trade-off.

**-Team Inexperience:** Event Sourcing requires different thinking patterns and specialized skills. Teams without distributed systems experience may struggle with the complexity and operational overhead these patterns introduce.

**-Strong Consistency Requirements:** Applications requiring immediate consistency across all views may find Event Sourcing’s eventual consistency model problematic. While consistency can be managed, it adds complexity that may not be justified in simple scenarios.

### Event Sourcing Implementation Patterns in .NET 8

Event Sourcing requires careful consideration of event design, storage mechanisms, and aggregate reconstruction patterns. Successful implementations balance performance, maintainability, and business value through thoughtful architectural decisions.

### Event Design and Schema Evolution

Well-designed events serve as the permanent business record and must evolve gracefully over time. Events should capture business intent rather than technical implementation details, use consistent naming conventions, and include sufficient context for future consumers who may not understand current system constraints.

```csharp
// Base Event Class  
public abstract class Event  
{  
    public Guid Id { get; } = Guid.NewGuid();  
    public DateTime OccurredOn { get; } = DateTime.UtcNow;  
    public string EventType => GetType().Name;  
    public int Version { get; set; } = 1;  
    public string CorrelationId { get; set; }  
    public string CausationId { get; set; }  
}  
  
// Well-designed Domain Events  
public record AccountOpenedEvent(    Guid AccountId,  
    string AccountNumber,  
    string CustomerName,  
    string CustomerEmail,  
    decimal InitialDeposit,  
    string BranchCode,  
    Guid OpenedBy) : Event;  
  
public record MoneyDepositedEvent(    Guid AccountId,  
    decimal Amount,  
    string Description,  
    string DepositMethod,  
    Guid TransactionId,  
    Guid ProcessedBy) : Event;  
  
public record MoneyWithdrawnEvent(    Guid AccountId,  
    decimal Amount,  
    string Description,  
    string WithdrawalMethod,  
    Guid TransactionId,  
    Guid ProcessedBy) : Event;  
  
public record AccountClosedEvent(    Guid AccountId,  
    string Reason,  
    decimal FinalBalance,  
    string ClosureMethod,  
    Guid ClosedBy) : Event;  
  
// Event versioning for schema evolution  
public record AccountOpenedEventV2(    Guid AccountId,  
    string AccountNumber,  
    string CustomerName,  
    string CustomerEmail,  
    decimal InitialDeposit,  
    string BranchCode,  
    Guid OpenedBy,  
    CustomerType CustomerType,  // New field in V2  
    string PreferredLanguage    // New field in V2) : Event;
```

**Event Store Implementation**
The event store serves as the system of record for all business changes and must provide reliable persistence, efficient retrieval, and scalability characteristics appropriate for the application’s requirements.

```csharp
// Event Store Interface  
public interface IEventStore  
{  
    Task SaveEventsAsync(string streamName, IEnumerable<Event> events, int expectedVersion);  
    Task<IEnumerable<Event>> GetEventsAsync(string streamName);  
    Task<IEnumerable<Event>> GetEventsAsync(string streamName, int fromVersion);  
    Task<IEnumerable<Event>> GetEventsByTypeAsync<T>() where T : Event;  
    Task<bool> StreamExistsAsync(string streamName);  
}  
  
// Event Store Implementation  
public class EventStore : IEventStore  
{  
    private readonly ApplicationDbContext _context;  
    private readonly IPublisher _publisher;  
    private readonly ILogger<EventStore> _logger;  
  
    public EventStore(ApplicationDbContext context, IPublisher publisher, ILogger<EventStore> logger)  
    {  
        _context = context;  
        _publisher = publisher;  
        _logger = logger;  
    }  
  
    public async Task SaveEventsAsync(string streamName, IEnumerable<Event> events, int expectedVersion)  
    {  
        using var transaction = await _context.Database.BeginTransactionAsync();  
        try  
        {  
            // Check for concurrency conflicts  
            var currentVersion = await GetStreamVersionAsync(streamName);  
            if (currentVersion != expectedVersion)  
            {  
                throw new ConcurrencyException($"Expected version {expectedVersion} but stream is at version {currentVersion}");  
            }  
  
            var version = expectedVersion;  
            foreach (var @event in events)  
            {  
                version++;  
                var eventEntity = new EventEntity  
                {  
                    Id = @event.Id,  
                    StreamName = streamName,  
                    EventType = @event.EventType,  
                    Data = JsonSerializer.Serialize(@event, @event.GetType()),  
                    Version = version,  
                    OccurredOn = @event.OccurredOn,  
                    CorrelationId = @event.CorrelationId,  
                    CausationId = @event.CausationId  
                };  
  
                _context.Events.Add(eventEntity);  
                _logger.LogInformation("Saving event: {EventType} for stream: {StreamName} at version {Version}",   
                                     @event.EventType, streamName, version);  
            }  
  
            await _context.SaveChangesAsync();  
            await transaction.CommitAsync();  
  
            // Publish events for projections  
            foreach (var @event in events)  
            {  
                await _publisher.Publish(@event);  
            }  
  
            _logger.LogInformation("Successfully saved {EventCount} events to stream: {StreamName}",   
                                 events.Count(), streamName);  
        }  
        catch (Exception ex)  
        {  
            await transaction.RollbackAsync();  
            _logger.LogError(ex, "Failed to save events to stream: {StreamName}", streamName);  
            throw;  
        }  
    }  
  
    public async Task<IEnumerable<Event>> GetEventsAsync(string streamName)  
    {  
        var eventEntities = await _context.Events  
            .Where(e => e.StreamName == streamName)  
            .OrderBy(e => e.Version)  
            .ToListAsync();  
  
        return eventEntities.Select(DeserializeEvent).Where(e => e != null);  
    }  
  
    public async Task<IEnumerable<Event>> GetEventsAsync(string streamName, int fromVersion)  
    {  
        var eventEntities = await _context.Events  
            .Where(e => e.StreamName == streamName && e.Version > fromVersion)  
            .OrderBy(e => e.Version)  
            .ToListAsync();  
  
        return eventEntities.Select(DeserializeEvent).Where(e => e != null);  
    }  
  
    private async Task<int> GetStreamVersionAsync(string streamName)  
    {  
        return await _context.Events  
            .Where(e => e.StreamName == streamName)  
            .MaxAsync(e => (int?)e.Version) ?? 0;  
    }  
  
    private Event DeserializeEvent(EventEntity eventEntity)  
    {  
        try  
        {  
            var eventType = Type.GetType(eventEntity.EventType) ??   
                           Assembly.GetExecutingAssembly().GetTypes()  
                                  .FirstOrDefault(t => t.Name == eventEntity.EventType);  
              
            if (eventType == null)  
            {  
                _logger.LogWarning("Unknown event type: {EventType}", eventEntity.EventType);  
                return null;  
            }  
  
            var @event = (Event)JsonSerializer.Deserialize(eventEntity.Data, eventType);  
            @event.CorrelationId = eventEntity.CorrelationId;  
            @event.CausationId = eventEntity.CausationId;  
              
            return @event;  
        }  
        catch (Exception ex)  
        {  
            _logger.LogError(ex, "Failed to deserialize event: {EventType}", eventEntity.EventType);  
            return null;  
        }  
    }  
}  
  
// Event Entity for persistence  
public class EventEntity  
{  
    public Guid Id { get; set; }  
    public string StreamName { get; set; }  
    public string EventType { get; set; }  
    public string Data { get; set; }  
    public int Version { get; set; }  
    public DateTime OccurredOn { get; set; }  
    public string CorrelationId { get; set; }  
    public string CausationId { get; set; }  
}
```

**Aggregate Implementation with Event Sourcing**
Aggregates in event-sourced systems generate events in response to commands and reconstruct their state by replaying historical events. This approach ensures that all business logic is captured as events while maintaining consistency within aggregate boundaries.

```csharp
// Event-Sourced Aggregate Base Class  
public abstract class AggregateRoot  
{  
    private readonly List<Event> _uncommittedEvents = new();  
      
    public Guid Id { get; protected set; }  
    public int Version { get; protected set; }  
  
    protected void ApplyEvent(Event @event)  
    {  
        When(@event);  
        _uncommittedEvents.Add(@event);  
    }  
  
    protected abstract void When(Event @event);  
  
    public IEnumerable<Event> GetUncommittedEvents() => _uncommittedEvents.AsReadOnly();  
  
    public void MarkEventsAsCommitted()  
    {  
        _uncommittedEvents.Clear();  
    }  
  
    public void LoadFromHistory(IEnumerable<Event> history)  
    {  
        foreach (var @event in history.OrderBy(e => e.OccurredOn))  
        {  
            When(@event);  
            Version++;  
        }  
    }  
}  
  
// Bank Account Aggregate Implementation  
public class BankAccount : AggregateRoot  
{  
    public string AccountNumber { get; private set; }  
    public string CustomerName { get; private set; }  
    public string CustomerEmail { get; private set; }  
    public decimal Balance { get; private set; }  
    public bool IsClosed { get; private set; }  
    public string BranchCode { get; private set; }  
  
    // Factory method for creating new accounts  
    public static BankAccount Open(string accountNumber, string customerName, string customerEmail,   
                                  decimal initialDeposit, string branchCode, Guid openedBy)  
    {  
        if (initialDeposit < 0)  
            throw new BusinessRuleException("Initial deposit cannot be negative");  
  
        var account = new BankAccount();  
        account.ApplyEvent(new AccountOpenedEvent(  
            Guid.NewGuid(), accountNumber, customerName, customerEmail,   
            initialDeposit, branchCode, openedBy));  
        return account;  
    }  
  
    // Business methods that generate events  
    public void Deposit(decimal amount, string description, string depositMethod, Guid transactionId, Guid processedBy)  
    {  
        if (IsClosed)  
            throw new BusinessRuleException("Cannot deposit to closed account");  
  
        if (amount <= 0)  
            throw new BusinessRuleException("Deposit amount must be positive");  
  
        ApplyEvent(new MoneyDepositedEvent(Id, amount, description, depositMethod, transactionId, processedBy));  
    }  
  
    public void Withdraw(decimal amount, string description, string withdrawalMethod, Guid transactionId, Guid processedBy)  
    {  
        if (IsClosed)  
            throw new BusinessRuleException("Cannot withdraw from closed account");  
  
        if (amount <= 0)  
            throw new BusinessRuleException("Withdrawal amount must be positive");  
  
        if (Balance < amount)  
            throw new BusinessRuleException("Insufficient funds");  
  
        ApplyEvent(new MoneyWithdrawnEvent(Id, amount, description, withdrawalMethod, transactionId, processedBy));  
    }  
  
    public void Close(string reason, string closureMethod, Guid closedBy)  
    {  
        if (IsClosed)  
            throw new BusinessRuleException("Account is already closed");  
  
        ApplyEvent(new AccountClosedEvent(Id, reason, Balance, closureMethod, closedBy));  
    }  
  
    // Event application logic  
    protected override void When(Event @event)  
    {  
        switch (@event)  
        {  
            case AccountOpenedEvent e:  
                Id = e.AccountId;  
                AccountNumber = e.AccountNumber;  
                CustomerName = e.CustomerName;  
                CustomerEmail = e.CustomerEmail;  
                Balance = e.InitialDeposit;  
                BranchCode = e.BranchCode;  
                break;  
              
            case MoneyDepositedEvent e:  
                Balance += e.Amount;  
                break;  
              
            case MoneyWithdrawnEvent e:  
                Balance -= e.Amount;  
                break;  
              
            case AccountClosedEvent e:  
                IsClosed = true;  
                break;  
        }  
    }  
  
    // Static method for rebuilding from event history  
    public static BankAccount FromHistory(IEnumerable<Event> events)  
    {  
        if (!events.Any())  
            return null;  
  
        var account = new BankAccount();  
        account.LoadFromHistory(events);  
        return account;  
    }  
}
```

**Repository Pattern for Event-Sourced Aggregates**

```csharp
public interface IRepository<T> where T : AggregateRoot  
{  
    Task<T> GetByIdAsync(Guid id);  
    Task SaveAsync(T aggregate);  
}  
  
public class Repository<T> : IRepository<T> where T : AggregateRoot  
{  
    private readonly IEventStore _eventStore;  
    private readonly ISnapshotStore _snapshotStore;  
    private readonly string _streamPrefix;  
  
    public Repository(IEventStore eventStore, ISnapshotStore snapshotStore)  
    {  
        _eventStore = eventStore;  
        _snapshotStore = snapshotStore;  
        _streamPrefix = typeof(T).Name;  
    }  
  
    public async Task<T> GetByIdAsync(Guid id)  
    {  
        var streamName = $"{_streamPrefix}-{id}";  
          
        // Try to load from snapshot first  
        var snapshot = await _snapshotStore.GetLatestSnapshotAsync<T>(id);  
        var fromVersion = 0;  
          
        T aggregate = null;  
        if (snapshot != null)  
        {  
            aggregate = snapshot.Data;  
            fromVersion = snapshot.Version;  
        }  
  
        // Load events since snapshot (or from beginning)  
        var events = await _eventStore.GetEventsAsync(streamName, fromVersion);  
          
        if (aggregate == null)  
        {  
            // Create new aggregate from history  
            var method = typeof(T).GetMethod("FromHistory", BindingFlags.Static | BindingFlags.Public);  
            aggregate = (T)method?.Invoke(null, new object[] { events });  
        }  
        else  
        {  
            // Apply events since snapshot  
            aggregate.LoadFromHistory(events);  
        }  
  
        return aggregate;  
    }  
  
    public async Task SaveAsync(T aggregate)  
    {  
        var streamName = $"{_streamPrefix}-{aggregate.Id}";  
        var events = aggregate.GetUncommittedEvents();  
          
        if (events.Any())  
        {  
            await _eventStore.SaveEventsAsync(streamName, events, aggregate.Version - events.Count());  
            aggregate.MarkEventsAsCommitted();  
  
            // Create snapshot if needed (every 20 events)  
            if (aggregate.Version % 20 == 0)  
            {  
                await _snapshotStore.SaveSnapshotAsync(aggregate.Id, aggregate, aggregate.Version);  
            }  
        }  
    }  
}
```

### Combining CQRS with Event Sourcing

The combination of CQRS and Event Sourcing creates a powerful architectural pattern that leverages the strengths of both approaches. Event Sourcing provides the complete audit trail and historical reconstruction capabilities, while CQRS enables optimized read models and independent scaling of query operations.

#### -Architectural Integration

*   When combining these patterns, commands generate events that are stored in the event store, while queries read from optimized projections built by processing those events. This separation enables the write side to focus on business logic and event generation, while the read side optimizes for specific query patterns and user experience requirements.
*   The event store becomes the single source of truth, while read models serve as denormalized views optimized for specific consumption patterns. This architecture enables multiple read models from the same event stream, each tailored for different use cases: user interfaces, reporting systems, analytics, and third-party integrations.

#### -Event-Driven Read Model Updates

*   Read model projections are built by processing events asynchronously as they’re published from the event store. This approach enables near-real-time updates to query models while maintaining loose coupling between the write and read sides of the application.

```csharp
// Account Read Model  
public class AccountReadModel  
{  
    public Guid Id { get; set; }  
    public string AccountNumber { get; set; }  
    public string CustomerName { get; set; }  
    public string CustomerEmail { get; set; }  
    public decimal Balance { get; set; }  
    public bool IsClosed { get; set; }  
    public string BranchCode { get; set; }  
    public DateTime OpenedDate { get; set; }  
    public DateTime? ClosedDate { get; set; }  
    public int TransactionCount { get; set; }  
    public decimal TotalDeposits { get; set; }  
    public decimal TotalWithdrawals { get; set; }  
}  
  
// Event Handlers for Read Model Updates  
public class AccountProjectionHandler :   
    INotificationHandler<AccountOpenedEvent>,  
    INotificationHandler<MoneyDepositedEvent>,  
    INotificationHandler<MoneyWithdrawnEvent>,  
    INotificationHandler<AccountClosedEvent>  
{  
    private readonly ApplicationDbContext _context;  
    private readonly ILogger<AccountProjectionHandler> _logger;  
  
    public AccountProjectionHandler(ApplicationDbContext context, ILogger<AccountProjectionHandler> logger)  
    {