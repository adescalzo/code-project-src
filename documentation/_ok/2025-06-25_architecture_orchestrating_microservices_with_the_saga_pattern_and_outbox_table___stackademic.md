```yaml
---
title: "Orchestrating Microservices with the Saga Pattern and Outbox Table | Stackademic"
source: https://blog.stackademic.com/orchestrating-microservices-with-the-saga-pattern-and-outbox-table-0832cb4db60f
date_published: 2025-06-25T16:49:08.312Z
date_captured: 2025-08-17T22:08:50.394Z
domain: blog.stackademic.com
author: rahul sahay
category: architecture
technologies: [Microservices, Saga Pattern, Outbox Pattern, Entity Framework Core, ASP.NET Core, Kafka, RabbitMQ, Hangfire, Quartz.NET, PostgreSQL, SQL Server, JSON.NET, System.Text.Json, .NET 8, Spring Boot]
programming_languages: [C#, SQL]
tags: [microservices, saga-pattern, outbox-pattern, distributed-transactions, eventual-consistency, event-driven, dotnet, database, message-broker, system-design]
key_concepts: [Saga Pattern, Outbox Pattern, Distributed Transactions, Eventual Consistency, Compensating Transactions, Atomic Operations, Message Broker, Background Worker]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explains how to implement a reliable and eventually consistent Saga Pattern using the Outbox Pattern in a microservices architecture. It addresses the challenges of distributed transactions in a system with multiple services, like an e-commerce order placement flow. The author demonstrates how to ensure atomic operations by writing to a local database and an Outbox table within the same transaction, with a background service publishing events to a message broker. This approach prevents message loss and data inconsistency, enabling services to react to events and handle failures through compensating actions, ultimately promoting resilience and loose coupling.]
---
```

# Orchestrating Microservices with the Saga Pattern and Outbox Table | Stackademic

# 🛒 Orchestrating Microservices with the Saga Pattern and Outbox Table

# 📌 TL;DR

Hi Friends, in this section we are going to see how to use Saga Pattern with Outbox pattern. Basically, we will learn how to implement a **reliable and eventually consistent Saga Pattern** using the **Outbox Pattern** in a microservices-based e-commerce system. We’ll walk through an order placement use case involving multiple services — Order, Payment, Inventory, and Notification — while ensuring **no message loss, duplication, or data inconsistency.**

![High-level diagram of an e-commerce order placement flow in a microservices architecture, showing Order Service, Payment Service, Inventory Service, and Notification Service interacting.](https://miro.medium.com/v2/resize:fit:574/1*kqhkTKIk8NnEun-Gemle9Q.png)

Above, I have pasted the high level diagram for this flow. This is one part of the entire ecosystem. At a high level, this will look like this.

# 🚨 The Problem: Distributed Transactions Are Hard

In a monolithic application, a single database transaction can wrap multiple operations atomically. But in a **microservices architecture**, data lives in different databases, across services.

Let’s say a customer places an order:

1.  You save the order (Order Service).
2.  You charge the payment (Payment Service).
3.  You reduce the stock (Inventory Service).
4.  You send a confirmation email (Notification Service).

💣 **What if one step fails? How do you rollback the others?**

Enter 👉 **Saga Pattern + Outbox Table**

# 🎯 What Is the Saga Pattern?

A **Saga** is a sequence of **local transactions** in different services. Each transaction publishes an event, and the next service listens and reacts.

There are 2 flavors:

*   **Orchestration-Based**: A central Saga orchestrator directs the flow.
*   **Choreography-Based**: Services listen and react to events.

We’ll use **Choreography** with an **Outbox Table** to guarantee **reliable message delivery**.

# 🏗 Architecture Overview

Let’s break the use case down into services:

1.  **Order Service**
2.  **Payment Service**
3.  **Inventory Service**
4.  **Notification Service**
5.  **Message Relay (Outbox Publisher)**

🧱 Each service:

*   Has its **own DB**
*   Uses **Outbox Table** to store events before publishing to the message broker

**Step 1: Create the OutboxEvent Entity**

```csharp
public class OutboxEvent  
{  
    public Guid Id { get; set; }  
    public string Type { get; set; }  
    public string Payload { get; set; }  
    public DateTime CreatedAt { get; set; }  
    public DateTime? ProcessedAt { get; set; }  
}
```

**Step 2: Include OutboxEvent in DbContext**

```csharp
public class AppDbContext : DbContext  
{  
    public DbSet<Order> Orders { get; set; }  
    public DbSet<OutboxEvent> OutboxEvents { get; set; }  
}
```

**Step 3: Write to DB and Outbox in Same Transaction**

```csharp
public async Task PlaceOrderAsync(Order order)  
{  
    var orderCreatedEvent = new  
    {  
        OrderId = order.Id,  
        Total = order.TotalAmount,  
        Created = DateTime.UtcNow  
    };
  
    var outboxEvent = new OutboxEvent  
    {  
        Id = Guid.NewGuid(),  
        Type = "OrderCreated",  
        Payload = JsonSerializer.Serialize(orderCreatedEvent),  
        CreatedAt = DateTime.UtcNow  
    };
  
    _dbContext.Orders.Add(order);  
    _dbContext.OutboxEvents.Add(outboxEvent);
  
    await _dbContext.SaveChangesAsync(); // atomic commit  
}
```

**Step 4: Background Publisher**

Use a **background worker** (e.g., `IHostedService`) to publish unprocessed events:

```csharp
public class OutboxPublisherService : BackgroundService  
{  
    private readonly IServiceProvider _services;
  
    public OutboxPublisherService(IServiceProvider services)  
    {  
        _services = services;  
    }
  
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)  
    {  
        while (!stoppingToken.IsCancellationRequested)  
        {  
            using var scope = _services.CreateScope();  
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
  
            var pendingEvents = await db.OutboxEvents  
                .Where(e => e.ProcessedAt == null)  
                .Take(10)  
                .ToListAsync(stoppingToken);
  
            foreach (var evt in pendingEvents)  
            {  
                try  
                {  
                    // Simulate publishing (e.g., Kafka, RabbitMQ)  
                    await PublishEventAsync(evt.Type, evt.Payload);
  
                    evt.ProcessedAt = DateTime.UtcNow;  
                }  
                catch (Exception ex)  
                {  
                    // Log and retry later  
                }  
            }
  
            await db.SaveChangesAsync(stoppingToken);  
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);  
        }  
    }
  
    private Task PublishEventAsync(string type, string payload)  
    {  
        // Actual messaging logic will go here  
        Console.WriteLine($"Published: {type} - {payload}");  
        return Task.CompletedTask;  
    }
}
```

**Step 5: Payment Service**

*   Listens for `OrderCreated`.
*   Tries to charge the card.
*   On success, emits `PaymentCompleted` to its own outbox.
*   On failure, emits `PaymentFailed`.

**Step 6: Inventory Service**

*   Listens for `PaymentCompleted`.
*   Tries to reserve stock.
*   Emits `StockReserved` or `StockFailed`.

**Step 7: Notification Service**

*   Listens to all events like `OrderPlaced`, `PaymentFailed`, etc.
*   Sends relevant email/SMS.

# 🧠 Why Use the Outbox Pattern?

Without outbox:

*   If message broker is down, you lose the event.
*   If transaction fails but message is sent, you get inconsistency.

With outbox:  
✅ **Atomicity** (DB write + event emit in one TX)  
✅ **Retryable**  
✅ **Auditable**

# ⚖️ Eventual Consistency in Action

In the Saga world, we trade **strong consistency** for **eventual consistency**. That means:

*   Order might remain in PENDING state until payment/stock is confirmed.
*   Services update their own state based on events, not via direct calls.

# 🔐 Handling Failures (Compensating Actions)

If stock is unavailable:

*   Inventory emits `StockFailed`.
*   Order service listens and **cancels the order**.

If payment fails:

*   Payment emits `PaymentFailed`.
*   Order status = `FAILED`.

You can add **compensating transactions** (like refund, restock) if needed.

![Diagram illustrating the Outbox Pattern flow, showing microservices saving orders and events to a database within the same transaction, a background processor publishing events to a message broker (RabbitMQ/Kafka), and then marking events as published.](https://miro.medium.com/v2/resize:fit:630/1*-NgWh7dVnn2Doan2nvQA0A.png)

![Table listing recommended technologies for different concerns in a microservices architecture: Message Broker (RabbitMQ, Kafka), Background Publisher (Hangfire, Quartz.NET), DB (PostgreSQL, MSSQL), Serialization (JSON.NET, System.Text.Json), and Framework (.NET 8, Spring Boot).](https://miro.medium.com/v2/resize:fit:630/1*bT4k82u6w1m-20025176.png)

# 📊 Benefits Recap

*   💥 No distributed transactions required
*   🔁 Retriable message delivery
*   ✅ Eventual consistency maintained
*   🔍 Easy to audit and debug

# 🚧 Limitations

*   Slight latency between event and effect
*   More infra (message broker, worker)
*   Requires good observability and monitoring

# 🔚 Conclusion

Combining **Saga Pattern + Outbox Pattern** is one of the most reliable ways to manage workflows in a distributed, microservices-based e-commerce system.

It ensures your system is **resilient**, **loosely coupled**, and **event-driven**, while still preserving transactional integrity.

> _“If you can’t trust your transactions across services, trust your events.”_