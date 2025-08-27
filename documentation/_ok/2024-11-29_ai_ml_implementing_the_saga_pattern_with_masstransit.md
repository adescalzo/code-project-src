```yaml
---
title: Implementing the Saga Pattern With MassTransit
source: https://www.milanjovanovic.tech/blog/implementing-the-saga-pattern-with-masstransit?utm_source=newsletter&utm_medium=email&utm_campaign=tnw118
date_published: 2024-11-30T00:00:00.000Z
date_captured: 2025-08-08T16:18:31.873Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: ai_ml
technologies: [MassTransit, Automatonymous, PostgreSQL, EntityFrameworkCore, RabbitMQ, ASP.NET Core, .NET]
programming_languages: [C#, SQL]
tags: [saga-pattern, distributed-systems, event-driven-architecture, state-machine, microservices, message-broker, dotnet, postgresql, fault-tolerance, long-running-processes]
key_concepts: [saga-pattern, compensating-actions, state-machine, message-consumers, event-driven-architecture, loose-coupling, fault-tolerance, distributed-transactions]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores the Saga pattern as a robust solution for managing complex, long-running business processes in distributed systems, offering an alternative to traditional two-phase commit. It provides a practical guide to implementing the Saga pattern using MassTransit, focusing on its state machine capabilities. The content demonstrates how to define saga instances, events, build a state machine for an order processing flow, and implement message consumers. Furthermore, it details the configuration of MassTransit with PostgreSQL for persistent saga state management. The article concludes by highlighting the significant benefits of this approach, including enhanced fault tolerance, clear state visibility, and improved system maintainability through loose coupling.]
---
```

# Implementing the Saga Pattern With MassTransit

![Implementing the Saga Pattern With MassTransit](/blog-covers/mnw_118.png?imwidth=3840)

# Implementing the Saga Pattern With MassTransit

6 min read · November 30, 2024

Long-running business processes often involve multiple services working together. Think about an e-commerce order: you need to process the payment, update inventory, and notify shipping. Traditional distributed transactions using two-phase commit (2PC) seem like a solution, but they come with significant drawbacks.

The main issue? Services can't make assumptions about how other services operate or how long they'll take. What if the payment service needs manual approval? What if the inventory check is delayed? Holding database locks across multiple services for extended periods isn't practical and can lead to system-wide issues.

Let's look at how the Saga pattern solves these problems and implement it using MassTransit.

## Understanding the Saga Pattern

A Saga is a sequence of related local transactions where each step has a defined action and a compensating action if something goes wrong. Instead of one big atomic transaction, we break the process into manageable steps that can be coordinated.

Here's a simple order processing flow:

![Sequence diagram showing an order processing flow with multiple services.](/blogs/mnw_118/order_flow.png?imwidth=1200)

Each step is independent and can be compensated if needed. If the inventory service reports items are out of stock, we can refund the payment. This approach gives us flexibility and reliability without tight coupling.

## Implementing Sagas with MassTransit

[**MassTransit**](using-masstransit-with-rabbitmq-and-azure-service-bus) provides a state machine-based approach to implementing Sagas through its integration with Automatonymous. Understanding how state machines work is crucial for implementing effective sagas.

### State Machine Fundamentals

A [state machine](https://masstransit.io/documentation/patterns/saga/state-machine) consists of several key components:

1.  **States**: Represent the possible conditions of your saga instance
2.  **Events**: Messages that can trigger state transitions
3.  **Behaviors**: Actions that occur when events are received in specific states
4.  **Instance**: Contains the data and current state for a specific saga

Here's the state machine diagram for our order processing saga:

![State diagram showing an order saga state machine.](/blogs/mnw_118/order_saga_state_machine.png?imwidth=1200)

Every state machine automatically includes `Initial` and `Final` states. The `Initial` state is where new saga instances begin, and the `Final` state marks the end of a saga's lifecycle.

### Defining the Saga Instance

The saga instance holds the data for a specific process:

```csharp
public class OrderState : SagaStateMachineInstance
{
    public Guid CorrelationId { get; set; }
    public string CurrentState { get; set; }

    // Business data
    public decimal OrderTotal { get; set; }
    public string? PaymentIntentId { get; set; }
    public DateTime? OrderDate { get; set; }
    public string? CustomerEmail { get; set; }
}
```

The `CorrelationId` uniquely identifies the saga instance, while `CurrentState` tracks its current state. Any additional properties store business data needed for the process.

### Defining Events

Events are messages that can trigger state transitions. They must be correlated to a specific saga instance:

```csharp
public record OrderSubmitted
{
    public Guid OrderId { get; init; }
    public decimal Total { get; init; }
    public string Email { get; init; }
}

public record PaymentProcessed
{
    public Guid OrderId { get; init; }
    public string PaymentIntentId { get; init; }
}

public record InventoryReserved
{
    public Guid OrderId { get; init; }
}

public record OrderFailed
{
    public Guid OrderId { get; init; }
    public string Reason { get; init; }
}
```

### Building the State Machine

Let's implement the order processing flow as a state machine:

```csharp
public class OrderStateMachine : MassTransitStateMachine<OrderState>
{
    public OrderStateMachine()
    {
        Event(() => OrderSubmitted, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => PaymentProcessed, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => InventoryReserved, x => x.CorrelateById(m => m.Message.OrderId));
        Event(() => OrderFailed, x => x.CorrelateById(m => m.Message.OrderId));

        InstanceState(x => x.CurrentState);

        Initially(
            When(OrderSubmitted)
                .Then(context =>
                {
                    context.Saga.OrderTotal = context.Message.Total;
                    context.Saga.CustomerEmail = context.Message.Email;
                    context.Saga.OrderDate = DateTime.UtcNow;
                })
                .PublishAsync(context => context.Init<ProcessPayment>(new
                {
                    OrderId = context.Saga.CorrelationId,
                    Amount = context.Saga.OrderTotal
                }))
                .TransitionTo(ProcessingPayment)
        );

        During(ProcessingPayment,
            When(PaymentProcessed)
                .PublishAsync(context => context.Init<ReserveInventory>(new
                {
                    OrderId = context.Saga.CorrelationId
                }))
                .TransitionTo(ReservingInventory),
            When(OrderFailed)
                .TransitionTo(Failed)
                .Finalize()
        );

        During(ReservingInventory,
            When(InventoryReserved)
                .PublishAsync(context => context.Init<OrderConfirmed>(new
                {
                    OrderId = context.Saga.CorrelationId
                }))
                .TransitionTo(Completed)
                .Finalize(),
            When(OrderFailed)
                .PublishAsync(context => context.Init<RefundPayment>(new
                {
                    OrderId = context.Saga.CorrelationId,
                    Amount = context.Saga.OrderTotal
                }))
                .TransitionTo(Failed)
                .Finalize()
        );

        SetCompletedWhenFinalized();
    }

    public State ProcessingPayment { get; private set; }
    public State ReservingInventory { get; private set; }
    public State Completed { get; private set; }
    public State Failed { get; private set; }

    public Event<OrderSubmitted> OrderSubmitted { get; private set; }
    public Event<PaymentProcessed> PaymentProcessed { get; private set; }
    public Event<InventoryReserved> InventoryReserved { get; private set; }
    public Event<OrderFailed> OrderFailed { get; private set; }
}
```

The state machine defines the possible states and transitions. Each step can trigger compensating actions if needed. For example, if inventory reservation fails, we automatically trigger a payment refund.

### Implementing Message Consumers

Services interact with the saga by consuming and publishing messages. Here's an example of a payment processing consumer:

```csharp
public class ProcessPaymentConsumer(
    IPaymentService paymentService,
    ILogger<ProcessPaymentConsumer> logger) : IConsumer<ProcessPayment>
{
    public async Task Consume(ConsumeContext<ProcessPayment> context)
    {
        try
        {
            var paymentResult = await paymentService.ProcessPaymentAsync(
                context.Message.OrderId,
                context.Message.Amount
            );

            if (paymentResult.Succeeded)
            {
                await context.Publish<PaymentProcessed>(new
                {
                    OrderId = context.Message.OrderId,
                    PaymentIntentId = paymentResult.PaymentIntentId
                });
            }
            else
            {
                await context.Publish<OrderFailed>(new
                {
                    OrderId = context.Message.OrderId,
                    Reason = paymentResult.FailureReason
                });
            }
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Failed to process payment for order {OrderId}",
                context.Message.OrderId);

            await context.Publish<OrderFailed>(new
            {
                OrderId = context.Message.OrderId,
                Reason = "Payment processing error"
            });
        }
    }
}
```

Each consumer handles a specific part of the business process and communicates back to the saga through events. This separation of concerns allows each service to focus on its specific responsibility while the saga coordinates the overall process.

## Configuring MassTransit with PostgreSQL Persistence

To persist the saga state, we'll use PostgreSQL. First, let's install the required packages:

```powershell
Install-Package MassTransit.EntityFrameworkCore
Install-Package Npgsql.EntityFrameworkCore.PostgreSQL
```

Create a `DbContext` for saga persistence:

```csharp
public class OrderSagaDbContext : SagaDbContext
{
    public OrderSagaDbContext(DbContextOptions options) : base(options)
    {
    }

    protected override IEnumerable<ISagaClassMap> Configurations
    {
        get
        {
            yield return new OrderStateMap();
        }
    }
}

public class OrderStateMap : SagaClassMap<OrderState>
{
    protected override void Configure(EntityTypeBuilder<OrderState> entity, ModelBuilder model)
    {
        entity.Property(x => x.CurrentState).HasMaxLength(64);
        entity.Property(x => x.CustomerEmail).HasMaxLength(256);
        entity.Property(x => x.PaymentIntentId).HasMaxLength(64);
    }
}
```

Configure MassTransit in your application:

```csharp
builder.Services.AddDbContext<OrderSagaDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

builder.Services.AddMassTransit(x =>
{
    x.AddSagaStateMachine<OrderStateMachine, OrderState>()
        .EntityFrameworkRepository(r =>
        {
            r.ConcurrencyMode = ConcurrencyMode.Pessimistic;
            r.AddDbContext<DbContext, OrderSagaDbContext>();
            r.UsePostgres();
        });

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(builder.Configuration.GetConnectionString("RabbitMQ"));
        cfg.ConfigureEndpoints(context);
    });
});
```

## Benefits of This Approach

Using sagas with MassTransit provides several advantages:

1.  **Fault Tolerance**: Each step can be retried independently. And we can compensate failed operation, making our system more resilient.
2.  **State Visibility**: The saga's state machine provides clear insight into where each process stands, making debugging and monitoring straightforward.
3.  **Loose Coupling**: Services communicate through messages, allowing them to evolve independently while maintaining process integrity.
4.  **Maintainability**: Changes can be made to individual steps without affecting others.

The state machine approach also makes the business process explicit. Each state and transition is clearly defined, making it easier to understand and maintain the workflow.

## Takeaway

The Saga pattern with MassTransit provides a robust solution for managing distributed business processes. Instead of dealing with distributed transactions, you get clear state management, automatic compensation for failures, and the ability to handle long-running operations without blocking resources.

Want to dive deeper into event-driven architecture and distributed systems? Check out my [**Modular Monolith Architecture**](/modular-monolith-architecture) course, where we explore sagas, event-driven patterns, and other essential techniques for building maintainable systems.

Good luck out there, and see you next week.

**P.S.** You can find the code for the saga pattern implementation with MassTransit in [this repository](https://github.com/m-jovanovic/saga-pattern-masstransit).

---

Whenever you're ready, there are 4 ways I can help you:

1.  [Pragmatic Clean Architecture:](/pragmatic-clean-architecture) Join 4,200+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [Modular Monolith Architecture:](/modular-monolith-architecture) Join 2,100+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [(NEW) Pragmatic REST APIs:](/pragmatic-rest-apis) Join 1,100+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
4.  [Patreon Community:](https://www.patreon.com/milanjovanovic) Join a community of 5,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Become a Better .NET Software Engineer

Join 70,000+ engineers who are improving their skills every Saturday morning.

Join 70K+ Engineers

![PCA Cover](/_next/static/media/cover.27333f2f.png?imwidth=384)

Pragmatic Clean Architecture

![MMA Cover](/_next/static/media/cover.31e11f05.png?imwidth=384)

Modular Monolith Architecture

![PRA Cover](/_next/static/media/cover_1.fc0deb78.png?imwidth=384)

Pragmatic REST APIs

NEW