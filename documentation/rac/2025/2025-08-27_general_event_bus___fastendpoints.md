```yaml
---
title: "Event Bus | FastEndpoints"
source: https://fast-endpoints.com/docs/event-bus#_1-define-an-event-model-dto
date_published: unknown
date_captured: 2025-08-27T15:57:43.574Z
domain: fast-endpoints.com
author: Unknown
category: general
technologies: [FastEndpoints, .NET]
programming_languages: [C#]
tags: [event-bus, pub-sub, event-driven, dotnet, csharp, asynchronous, decoupling, fastendpoints, design-patterns]
key_concepts: [event-bus-pattern, publish-subscribe-pattern, event-handlers, dependency-injection, asynchronous-programming, data-transfer-object, fire-and-forget, one-to-many-communication]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the in-process event bus pattern, also known as publish/subscribe, for building event-driven applications with decoupled event handlers. It demonstrates how to define event models (DTOs) and implement event handlers in C#, showcasing the one-to-many relationship between events and their subscribers. The content covers publishing events using a `PublishAsync()` method, including options for controlling subscriber completion waiting modes like `WaitForAll` or `WaitForNone`. It also touches upon publishing events from various parts of an application and how dependency injection is handled for event handlers within the FastEndpoints framework.]
---
```

# Event Bus | FastEndpoints

# In-Process Event Bus Pattern (Pub/Sub)

If you'd like to take an event driven approach to building your application, you have the option to publish events and have completely decoupled **Event-Handlers** take action when events are published. An event can have more than one handler and has a one-to-many relationship. Due to the nature of pub/sub event bus pattern, handlers cannot return any results back to the caller/publisher.

### 1. Define An Event Model/ DTO

This is the data contract that will be delivered to the subscribers/event-handlers.

```csharp
public class OrderCreatedEvent
{
    public string OrderID { get; set; }
    public string CustomerName { get; set; }
    public decimal OrderTotal { get; set; }
}
```

### 2. Define An Event Handler

This is the code that will be executed when events of the above DTO type gets published.

```csharp
public class OrderCreationHandler : IEventHandler<OrderCreatedEvent>
{
    private readonly ILogger _logger;

    public OrderCreationHandler(ILogger<OrderCreationHandler> logger)
    {
        _logger = logger;
    }

    public Task HandleAsync(OrderCreatedEvent eventModel, CancellationToken ct)
    {
        _logger.LogInformation($"order created event received:[{eventModel.OrderID}]");
        return Task.CompletedTask;
    }
}
```

You can create as many implementations of **IEventHandler<OrderCreatedEvent>** as you like, and they all are receivers/subscribers of the **OrderCreatedEvent**. No other boilerplate (explicit subscription registration) is necessary.

### 3. Publish The Event

Simply hand in an event model to the **PublishAsync()** method.

```csharp
public class CreateOrderEndpoint : Endpoint<CreateOrderRequest>
{
    public override void Configure()
    {
        Post("/sales/orders/create");
    }

    public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
    {
        var orderID = await orderRepo.CreateNewOrder(req);

        await PublishAsync(new OrderCreatedEvent
        {
            OrderID = orderID,
            CustomerName = req.Customer,
            OrderTotal = req.OrderValue
        });

        await Send.OkAsync();
    }
}
```

## The PublishAsync() Method

The **PublishAsync()** method has an overload that will take a **Mode** enum that lets you specify whether to wait for **all subscribers** to finish; wait for **any subscriber** to finish; or wait for **none of the subscribers** to finish.

For example, you can publish an event in a fire-n-forget manner with the following:

```csharp
await PublishAsync(eventModel, Mode.WaitForNone);
```

The default mode is **Mode.WaitForAll** which will await all subscribers. I.e. execution will only continue after each and every subscriber of the event has completed their work.

## Publish From Anywhere

It is possible to publish events even from outside of endpoints by marking the event model with the **IEvent** interface, which would provide **PublishAsync()** as an extension method.

```csharp
public class OrderCreatedEvent : IEvent { ... }
```

```csharp
await new OrderCreatedEvent
{
    OrderID = "12345",
    CustomerName = "scarlet johanson",
    OrderTotal = 123.45m
}
.PublishAsync(Mode.WaitForAll);
```

## Dependency Injection

Dependencies in event handlers can be resolved as described [here](dependency-injection#event-handler-dependencies).

---

Previous [<- Pre / Post Processors](/docs/pre-post-processors)

Next [Command Bus ->](/docs/command-bus)

© FastEndpoints 2025