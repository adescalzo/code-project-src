```yaml
---
title: "Kafka and .NET: Practical Guide to Building Event-Driven Services"
source: https://hamedsalameh.com/kafka-and-net-practical-guide-to-building-event-driven-services/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2088
date_published: 2025-04-02T06:21:52.000Z
date_captured: 2025-08-17T22:11:59.663Z
domain: hamedsalameh.com
author: Unknown
category: frontend
technologies: [Kafka, .NET, ASP.NET Core, Confluent.Kafka, Docker, ZooKeeper, MemoryCache, RabbitMQ, MassTransit, Visual Studio, Rider, VS Code]
programming_languages: [C#, Java]
tags: [kafka, .net, event-driven, messaging, microservices, producer-consumer, docker, web-api, background-services, reliability]
key_concepts: [event-driven-architecture, distributed-systems, producer-consumer-pattern, idempotency, dead-letter-queue, manual-offset-commit, consumer-groups, kafka-adminclient]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a practical guide to building event-driven services using Apache Kafka and .NET. It covers setting up a local Kafka environment with Docker, then demonstrates how to implement Kafka producers in an ASP.NET Core Web API and consumers in .NET background services using the Confluent.Kafka library. The guide further enhances reliability by explaining and implementing idempotency, manual offset commits, and a Dead Letter Queue (DLQ) for handling malformed messages. Finally, it introduces the Kafka AdminClient for programmatic topic and metadata management, concluding with essential best practices for robust Kafka integration in .NET applications.
---
```

# Kafka and .NET: Practical Guide to Building Event-Driven Services

# Kafka and .NET: Practical Guide to Building Event-Driven Services

![A digital art image depicting abstract data flow, with three distinct colored streams of binary code (11001101011, 100110011011, 10110101010) converging and fanning out into a multitude of thin lines that merge into a dense column of binary data on the right, all against a dark blue background.](https://hamedsalameh.com/wp-content/uploads/2025/04/data-8841981_1280.jpg)

## Introduction

In today’s modern software landscape, event-driven architecture is becoming increasingly popular—and it’s not just another buzzword. It’s a practical and effective approach for building scalable, loosely coupled, and highly responsive systems. In scenarios such as handling user interactions, system logs, payment events, sensor data, or database changes, the ability to stream and react to events in real-time is often essential.

This is where Apache Kafka comes in. Kafka is a distributed event streaming platform designed for speed, scalability, high availability, and fault tolerance. Originally developed by LinkedIn and later open-sourced under the Apache Software Foundation, Kafka has become a common choice for building event pipelines and handling real-time data across a wide range of systems, including those built with .NET.

While Kafka is built in Java, the .NET ecosystem has solid support for it. With libraries like Confluent.Kafka, you can implement reliable Kafka producers and consumers in .NET Core—whether you’re building console applications, background services, or ASP.NET Core APIs that leverage the power of Kafka and .NET. This article focuses on integrating Kafka and .NET, highlighting how you can utilize these technologies together to create efficient event-driven services.

Kafka isn’t the only messaging technology available to .NET developers. If you’re curious about a different type of message broker, consider exploring our article on messaging with RabbitMQ and MassTransit, a popular queue-based alternative often chosen for its simplicity and ease of integration with .NET.

## What we will cover in this article

*   Kafka – Core Concepts and Fundamentals
*   Setting up Kafka locally (with Docker)
*   .NET project setup
*   Building your first producer and consumer
*   Integrating Kafka into .Net Core WebAPI
*   Implement real-time even handling with background services

## Kafka – Core Concepts and Fundamentals

Before we dive into code and implementation, it’s important to understand a few core concepts about Kafka, especially when using it with .NET. These concepts will give you a clearer picture of how Kafka works in conjunction with .NET, making it easier to integrate it into your system and troubleshoot issues when they come up.

Let’s start by reviewing the foundational building blocks of Kafka.

**Topic**  
A **topic** is a named channel where messages are published and read. You can think of it as a logical stream of events. Producers write messages to topics, and consumers read messages from them.

**Partition**  
Each topic is split into one or more **partitions**, which are the actual logs that store events in an ordered, immutable sequence. Partitions are what enable Kafka to scale horizontally and handle large volumes of data.

**Offset**  
An **offset** is a unique identifier for each message within a partition. It helps Kafka and consumers keep track of which messages have been read. Offsets are managed either manually or automatically, depending on the consumer configuration.

**Producer**  
A **producer** is any application or service that writes (publishes) messages to a Kafka topic. Producers can choose which partition to write to, usually based on a key (for ordering) or round-robin strategy (for load balancing).

**Consumer**  
A **consumer** reads messages from Kafka topics. Kafka supports **consumer groups**, which allow multiple instances of the same application to process messages in parallel while ensuring that each message is only processed once by a single consumer in the group.

**Broker**  
A **broker** is a Kafka server that stores data and serves client requests. A Kafka cluster is made up of multiple brokers working together to provide scalability and fault tolerance.

#### Kafka Key APIs

Kafka provides several APIs for interacting with the system. The most common ones are:

**Producer API**  
Used to send records (events/messages) to Kafka topics. In .NET, this is available through the Confluent.Kafka library’s `IProducer<TKey, TValue>` interface.

**Consumer API**  
Used to read records from Kafka topics. The `IConsumer<TKey, TValue>` interface allows you to subscribe to one or more topics, poll for new messages, and commit offsets.

**Admin API**  
Used to manage Kafka topics, partitions, and configurations programmatically. In .NET, you can use `AdminClientBuilder` to perform operations like creating topics or checking metadata.

**Kafka Streams API**  
A client library for building applications and microservices that process real-time data using event streams. It provides a high-level DSL for operations like filtering, mapping, joining, and aggregating streams.

**Connect API _(Used with Kafka Connect)_**  
Kafka Connect is a framework for streaming data between Kafka and other systems (e.g., databases, storage systems, etc.) using pre-built or custom connectors. It’s commonly used for **Change Data Capture** (CDC).

## Project setup | Kafka and .NET

#### Prerequisites

Before starting, make sure you have:

*   .NET 8 SDK
*   Docker Desktop
*   A code editor like Visual Studio or Rider, or just use VS Code

Our example is a simple Web API project that receives orders from the client through an API endpoint. After basic validation and billing, it publishes an event to Kafka indicating that the order was placed.

The solution also includes two additional projects: `InventoryService` and `ShippingService`. Both are background service workers that subscribe to the `order-placed` topic. When a new event is published on the topic, each service processes the event independently.

![Diagram showing a .NET Web API publishing an OrderPlaced event to a Kafka topic (order-placed). Two background services—Shipping Service and Inventory Service—subscribe to the topic and process the event independently.](https://hamedsalameh.com/wp-content/uploads/2025/04/KafkaDemo.png "Kafka Publisher and Consumers")

To Start, create a new empty solution, then add a new Web API project and two Worker Service projects.

In our example, the .NET solution has the following structure:

```
Solution/  
│  
├── src/  
│   ├── OrderProcessingDemo/        # NET Core API (produces OrderPlaced events)  
│   ├── InventoryServiceWorker/     # Kafka consumer (reserves inventory)  
│   ├── ShippingServiceWoker/       # Kafka consumer (schedules shipment)  
│   └── Shared/                     # Shared models & Kafka config  
│  
└── docker-compose.yml             # Kafka + Zookeeper setup
```

For the API service, as well as the **InventoryService** and **ShippingService**, install the Kafka client library:

```bash
dotnet add package Confluent.Kafka
```

#### Spin Up Kafka with Docker

Before we start writing code, we need a working Kafka setup. There are a few ways to run Kafka locally, but the most common and convenient approach for development is using Docker. If you prefer installing Kafka manually, that works too—but using Docker keeps things isolated and reproducible.

In this post, we’ll focus on a simple setup using a single Kafka instance and ZooKeeper. Kafka requires ZooKeeper to manage metadata, coordinate brokers, and perform leader election (at least in versions before KRaft mode became stable).

If you have Docker installed, you can run Kafka and Zookeeper using the following `docker-compose.yml` file:

```yaml
version: '2'

services:
  zookeeper:
    image: confluentinc/cp-zookeeper:7.6.0
    environment:
      ZOOKEEPER_CLIENT_PORT: 2181
      ZOOKEEPER_TICK_TIME: 2000
  kafka:
    image: confluentinc/cp-kafka:7.6.0
    depends_on:
      - zookeeper
    ports:
      - "9092:9092"
    environment:
      KAFKA_BROKER_ID: 1
      KAFKA_ZOOKEEPER_CONNECT: zookeeper:2181
      KAFKA_ADVERTISED_LISTENERS: PLAINTEXT://localhost:9092
      KAFKA_OFFSETS_TOPIC_REPLICATION_FACTOR: 1
```

To start the services, run:

```bash
docker-compose up -d
```

This will spin up a single-node Kafka cluster with Zookeeper, accessible at `localhost:9092`.

Note: place the `docker-compose.yaml` file in the root of your .NET solution.

Now that we have an instance of Kafka up and running, we need to configure the .NET projects to work with it.  
To keep things simple, we will use the `appsettings.json` file to store our Kafka configuration.

## Kafka and .Net | Basic Producer & Consumer

#### Step 1: The Event Class

Since the demo simulates orders being placed, we’ll start by defining the structure of the message that will be sent to Kafka. This message is represented by the `OrderPlacedEvent` class, which should be added under the Shared project. It includes basic order details like the order ID, user ID, total amount, list of items, a timestamp, and the payment ID.

```csharp
namespace Shared
{
    public class OrderPlacedEvent
    {
        public string OrderId { get; set; } = default!;
        public string UserId { get; set; } = default!;
        public decimal Total { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public DateTime Timestamp { get; set; }
        public string PaymentId { get; set; } = default!;
    }
    public class OrderItem
    {
        public string ProductId { get; set; } = default!;
        public int Quantity { get; set; }
    }
}
```

#### Step 2: Web API Setup

In the Web API project, we’ll also need a class to handle the incoming request from the client. This class doesn’t need to match the Kafka event structure exactly—it only needs to represent what the client sends when placing an order.

Create the following two classes to handle the request payload:

```csharp
public class OrderItemDto
{
    public string ProductId { get; set; } = default!;
    public int Quantity { get; set; }
}
public class PlaceOrderRequest
{
    public string UserId { get; set; } = default!;
    public decimal Total { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}
```

Update `appsettings.json` file with Kafka configuration:

```json
"Kafka": {
  "BootstrapServers": "localhost:9092",
  "OrderPlacedTopic": "order-placed",
  "OrderPlacedDLQTopic": "order-placed-dlq"
}
```

This will tell the producer where Kafka is located and how to communicate with it.

Next, we’ll configure Kafka in `Program.cs` to load the necessary configuration and register a Kafka producer as a singleton service. This allows us to inject and reuse the same Kafka producer throughout the app.

First, we bind Kafka settings from `appsettings.json` using a simple configuration class (`KafkaSettings`), then set up the producer using `ProducerBuilder`. The producer configuration includes options to improve reliability, such as enabling idempotence (to prevent duplicate messages), waiting for acknowledgments from all Kafka replicas, and retrying if something goes wrong while sending the message.

```csharp
// Bind Kafka settings from config
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection("Kafka"));

// setup Kafka producer
ConfigureKafka(builder);

static void ConfigureKafka(WebApplicationBuilder builder)
{
    builder.Services.AddSingleton(sp =>
    {
        var kafkaSettings = sp.GetRequiredService<IOptions<KafkaSettings>>().Value;
        var config = new ProducerConfig
        {
            BootstrapServers = kafkaSettings.BootstrapServers,
            Acks = Acks.All,           // Wait for all replicas to acknowledge
            EnableIdempotence = true,  // Ensure exactly-once semantics
            MessageSendMaxRetries = 3, // Retry 3 times
            RetryBackoffMs = 100       // Wait 100ms between retries
        };
        return new ProducerBuilder<Null, string>(config).Build();
    });
}
```

This setup allows the application to publish events to Kafka in a reliable and consistent way. In the next step, we’ll implement the endpoint that uses this producer to publish an `OrderPlacedEvent` when a new order is received.

#### Step 3: OrderPlaced Endpoint and Kafka Producer

Instead of using the traditional controller-based approach, this demo uses the **minimal API** style available in ASP.NET Core. This keeps the API lightweight and allows you to define endpoints directly using extension methods and delegates.

Start by creating a static class to define the `POST /orders` endpoint. This class contains:

*   An extension method to register the endpoint in your `Program.cs`
*   The delegate method (`PlaceOrderAsync`) that handles the incoming request and sends the `OrderPlacedEvent` to Kafka

Here’s the full implementation:

```csharp
public static class PlaceOrderEndpoint
{
    public static IEndpointConventionBuilder MapPostEndpoint(this IEndpointRouteBuilder builder, string pattern = "")
    {
        var endpoint = builder.MapPost(pattern, PlaceOrderAsync)
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status500InternalServerError);
        return endpoint;
    }
    public static async Task<IResult> PlaceOrderAsync([FromBody] PlaceOrderRequest placeOrderRequest,
        CancellationToken cancellationToken,
        ILoggerFactory loggerFactory,
        IProducer<Null, string> producer,
        IOptions<KafkaSettings> kafkaOptions)
    {
        ILogger logger = loggerFactory.CreateLogger("PlaceOrderEndpoint");
        if (placeOrderRequest == null)
        {
            logger.LogError("Invalid request object");
            return Results.BadRequest("The submitted request is not valid or empty");
        }
        await Task.Delay(1000, cancellationToken).ConfigureAwait(false); // Fake some processing time, charge payment, etc.
        logger.LogInformation("Order processed successfully");
        // Submit the order to Kafka for further processing
        var orderPlaceEvent = new OrderPlacedEvent
        {
            OrderId = Guid.NewGuid().ToString(),
            UserId = placeOrderRequest.UserId,
            Total = placeOrderRequest.Total,
            Items = placeOrderRequest.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList(),
            Timestamp = DateTime.UtcNow,
            PaymentId = Guid.NewGuid().ToString()
        };
        var json = JsonSerializer.Serialize(orderPlaceEvent);
        await producer.ProduceAsync(kafkaOptions.Value.OrderPlacedTopic, new Message<Null, string>
        {
            Value = json
        }).ConfigureAwait(false);
        logger.LogInformation("Order placed event sent to Kafka");
        return Results.Ok();
    }
}
```

Now let’s add this end point to the application in `Program.cs` file:

```csharp
app.MapPostEndpoint("/placeorder");
```

And that’s it. When a `POST` request is made to place an order, the API processes the request, builds an `OrderPlacedEvent`, and publishes it to Kafka.

Next, we’ll implement the consumers that subscribe to this event and handle it in their own services.

#### Step 4 : Creating Kafka Consumer

We have two background services that subscribe to the `order-placed` topic. Each service will consume the event and handle it based on its own business logic.

###### InventoryService

Add the following settings to `appsettings.json`:

```json
"Kafka": {
  "BootstrapServers": "localhost:9092",
  "Topic": "order-placed",
  "GroupId": "inventory-service"
}
```

In the `InventoryServiceWorker`, we override the default worker implementation and inject the Kafka configuration from the `appsettings.json` file that we set up earlier.

Next, inside the `StartAsync` method, we configure the Kafka consumer and subscribe to the `order-placed` topic.

Here’s the implementation of the worker class:

```csharp
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly KafkaSettings _kafkaSettings;
    private IConsumer<Ignore, string>? _consumer;
    public Worker(ILogger<Worker> logger, IOptions<KafkaSettings> kafkaOptions)
    {
        _logger = logger;
        _kafkaSettings = kafkaOptions.Value;
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            GroupId = _kafkaSettings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };
        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        _consumer.Subscribe(_kafkaSettings.Topic);
        _logger.LogInformation("Kafka consumer started and subscribed to topic: {Topic}", _kafkaSettings.Topic);
        return base.StartAsync(cancellationToken);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // Ensures method runs asynchronously
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer?.Consume(stoppingToken);
                if (result == null || string.IsNullOrWhiteSpace(result.Message?.Value))
                    continue;
                OrderPlacedEvent? order;
                try
                {
                    order = JsonSerializer.Deserialize<OrderPlacedEvent>(result.Message.Value);
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "Failed to deserialize message: {Message}", result.Message.Value);
                    continue;
                }
                if (order == null)
                {
                    _logger.LogWarning("Received null or malformed order event");
                    continue;
                }
                _logger.LogInformation("Order received: {OrderId} at {Timestamp}", order.OrderId, order.Timestamp);
                await HandleOrder(order);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize Kafka message");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing Kafka message");
            }
        }
    }
    private async Task HandleOrder(OrderPlacedEvent order)
    {
        _logger.LogInformation("Order received: {OrderId} at {Timestamp}", order.OrderId, order.Timestamp);
        foreach (var item in order.Items)
        {
            _logger.LogInformation(" - Product: {ProductId}, Quantity: {Quantity}", item.ProductId, item.Quantity);
            await Task.Delay(125);
        }
        _logger.LogInformation("Order inventory updated: {OrderId}", order.OrderId);
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_consumer != null)
        {
            _logger.LogInformation("Closing Kafka consumer...");
            _consumer.Close();
            _consumer.Dispose();
        }
        await base.StopAsync(cancellationToken);
    }
}
```

Update `Program.cs` to load Kafka settings and make them injectable:

```csharp
// Bind Kafka settings from config
builder.Services.Configure<KafkaSettings>(
    builder.Configuration.GetSection("Kafka"));
```

###### ShippingService

Start by adding Kafka settings in `appsettings.json`:

```json
"Kafka": {
  "BootstrapServers": "localhost:9092",
  "Topic": "order-placed",
  "GroupId": "shipping-service"
}
```

Just as we did with the **InventoryService**, override the default worker class, inject Kafka configuration and initialize the consumer object.

Here is the full implementation:

```csharp
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly KafkaSettings _kafkaSettings;
    private IConsumer<Ignore, string>? _consumer;
    public Worker(ILogger<Worker> logger, IOptions<KafkaSettings> kafkaOptions)
    {
        _logger = logger;
        _kafkaSettings = kafkaOptions.Value;
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            GroupId = _kafkaSettings.GroupId,   // Consumer group ID
            AutoOffsetReset = AutoOffsetReset.Earliest, // Start from the beginning of the topic
            EnableAutoCommit = true // Commit offsets automatically
        };
        _consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        _consumer.Subscribe(_kafkaSettings.Topic);
        _logger.LogInformation("Kafka consumer started and subscribed to topic: {Topic}", _kafkaSettings.Topic);
        return base.StartAsync(cancellationToken);
    }
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield(); // Ensures method runs asynchronously
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = _consumer?.Consume(stoppingToken);
                if (result == null || string.IsNullOrWhiteSpace(result.Message?.Value))
                    continue;
                var order = JsonSerializer.Deserialize<OrderPlacedEvent>(result.Message.Value);
                if (order == null)
                {
                    _logger.LogWarning("Received null or malformed order event");
                    continue;
                }
                await handleOrderShipping(order);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error: {Reason}", ex.Error.Reason);
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Failed to deserialize Kafka message");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing Kafka message");
            }
        }
    }
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_consumer != null)
        {
            _logger.LogInformation("Closing Kafka consumer...");
            _consumer.Close();
            _consumer.Dispose();
        }
        await base.StopAsync(cancellationToken);
    }
    private async Task handleOrderShipping(OrderPlacedEvent order)
    {
        _logger.LogInformation("Order received: {OrderId} at {Timestamp}", order.OrderId, order.Timestamp);
        foreach (var item in order.Items)
        {
            _logger.LogInformation(" - Product: {ProductId}, Quantity: {Quantity}", item.ProductId, item.Quantity);
        }
        await Task.Delay(500); // Simulate processing time
        _logger.LogInformation("Order shipping prepared: {OrderId}", order.OrderId);
    }
}
```

That’s it!

Once you run all the projects, any message that gets published by the **Producer**, will be consumed by two different services, and processed separately.

## Idempotency, Manual Offset, and Dead Letter Queue

When building event-driven systems, it’s important to think beyond just “consume and process”. In real-world scenarios, failure can, and probably will happen – services crash, message might contain some unsupported content, message might get retried, and some might be malformed. If we don’t plan for these situations, we risk losing data, or processing the same data more than once.

To make our consumer more reliable, we’ll improve the **InventoryServiceWorker** with the following capabilities:

*   #### Idempotency
    
    Make sure we don’t process the same order more than once, even if the message is re-consumed.
    
*   #### Manual Offset Commit
    
    Only commit the Kafka offset after successful processing, to avoid losing events during failures.
    
*   #### Dead Letter Queue
    
    If the message can’t be processed (e.g., invalid payload), we’ll send it to a separate Kafka topic so it can be reviewed or retried later.
    

![A diagram illustrating an enhanced event-driven architecture with a Dead Letter Queue (DLQ). A web browser icon represents a client interacting with a .NET Web API. The Web API publishes an event to a Kafka instance, specifically to an "order-placed topic". Two consumers subscribe to this topic. One consumer is shown with an arrow pointing to a separate "order-placed-dlq" topic within Kafka, indicating that it publishes problematic messages to this Dead Letter Queue.](https://hamedsalameh.com/wp-content/uploads/2025/04/DLQ.png "Dead Letter Queue")

#### Implementation

To support idempotency, we need a way to track the IDs of recently processed orders. In this demo, we use an in-memory cache (`MemoryCache`) with a 24-hour expiration to store processed order IDs. This helps avoid processing the same order more than once if the message is re-consumed.

If we detect that an order ID already exists in the cache, we safely skip the message and move on without re-processing it.

After an order is successfully processed, we manually commit the Kafka offset. This ensures that the offset is only marked as complete if the processing logic has run without errors.

In case we receive a malformed message (e.g., invalid JSON), we don’t drop it silently. Instead, we send it to a separate dead-letter topic. This gives us a way to inspect and troubleshoot problematic messages later.

Update your **`InventoryServiceWorker`** class with the following logic:

```csharp
public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly KafkaSettings _kafkaSettings;
    private IConsumer<Ignore, string>? _consumer;
    private IProducer<Null, string>? _producer;
    // Simulated store for idempotency check (use a persistent store in production)
    private readonly MemoryCache _processedOrderCache = new(new MemoryCacheOptions());
    public Worker(ILogger<Worker> logger, IOptions<KafkaSettings> kafkaOptions)
    {
        _logger = logger;
        _kafkaSettings = kafkaOptions.Value;
    }
    public override Task StartAsync(CancellationToken cancellationToken)
    {
        InitializeConsumer();
        InitializeProducer();
        _logger.LogInformation("Kafka consumer started and subscribed to topic: {Topic}", _kafkaSettings.Topic);
        return base.StartAsync(cancellationToken);
    }
    private void InitializeProducer()
    {