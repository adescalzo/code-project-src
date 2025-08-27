```yaml
---
title: "MassTransit RabbitMQ and Azure Service Bus: Is It Worth a Commercial License"
source: https://antondevtips.com/blog/masstransit-rabbitmq-and-azure-service-bus-is-it-worth-a-commercial-license
date_published: 2025-05-27T07:45:08.049Z
date_captured: 2025-08-27T13:19:05.224Z
domain: antondevtips.com
author: Anton Martyniuk
category: devops
technologies: [MassTransit, RabbitMQ, Azure Service Bus, Kafka, Amazon SQS, Amazon SNS, ActiveMQ, PostgreSQL, Docker, Seq, .NET, ASP.NET Core, Entity Framework Core, GitHub Copilot, Neon MCP Server, Visual Studio Code, MediatR, Wolverine, OpenTelemetry, Quartz.NET, Hangfire]
programming_languages: [C#, SQL, YAML]
tags: [masstransit, message-broker, microservices, dotnet, rabbitmq, azure-service-bus, distributed-systems, event-driven, outbox-pattern, licensing]
key_concepts: [message-based-communication, microservices-architecture, event-driven-architecture, outbox-pattern, retry-policies, sagas, state-machines, request-response-pattern, dependency-injection, distributed-tracing, commercial-licensing]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article analyzes MassTransit's upcoming transition to a commercial licensing model and its implications for developers building distributed .NET applications. It demonstrates MassTransit's capabilities by integrating it with RabbitMQ and Azure Service Bus in a microservices architecture, providing practical code examples for message publishing, consumption, and advanced features like the Outbox pattern. The post also details MassTransit's extensive features, including sagas, observability, and testing, concluding with a discussion on whether the commercial license is a worthwhile investment compared to alternatives or native cloud SDKs for mission-critical systems.]
---
```

# MassTransit RabbitMQ and Azure Service Bus: Is It Worth a Commercial License

May 27, 2025

[Download source code](/source-code/masstransit-rabbitmq-and-azure-service-bus-is-it-worth-a-commercial-license)

6 min read

## Newsletter Sponsors

Create full-stack apps running in the cloud with GitHub Copilot and Neon MCP Server in VS Code. Learn [how to use it](https://neon.tech/guides/neon-mcp-server-github-copilot-vs-code?refcode=44WD03UH)!

[Sponsor my newsletter to reach 12,000+ readers](/sponsorship)

Disclaimer: This post isn't sponsored by MassTransit; I'm just sharing my own thoughts and experience with MassTransit over the years.

Recently, the **MassTransit** team announced that they will move to a commercial licensing model starting in 2026. If you rely on MassTransit, you'll need to decide whether the new paywall justifies continuing to use its features – or if alternative solutions might be more suitable.

**MassTransit** is a powerful and flexible library for building distributed applications using message-based communication. It provides a set of tools and abstractions for creating, sending, and receiving messages across various messaging transports, such as RabbitMQ, Kafka, Azure Service Bus, and more.

In this post, we'll explore some of MassTransit's core features, how it supports different transports like RabbitMQ and Azure Service Bus.

Let's dive in.

## Building Microservices with MassTransit

Let's explore a project involving three microservices: ShipmentService, OrderService, and PaymentService. They communicate with each other via events over a message queue.

**ShipmentService:**

*   Creates and updates shipments for purchased products. Publishes `ShipmentCreatedEvent` and `ShipmentStatusUpdatedEvent`.
*   Stores shipments in a database

**OrderService:**

*   Creates orders when a user checks out. Publishes `OrderCreatedEvent`.
*   Subscribes to payment events (`PaymentCompletedEvent` or `PaymentFailedEvent`) to update order status.

**PaymentService:**

*   Processes payment transactions. Publishes `PaymentCompletedEvent` or `PaymentFailedEvent`.
*   Order service subscribes to know if payment was successful or failed.

Here is what the communication between these services looks like:

![A diagram illustrating the communication flow between three microservices: ShipmentService, OrderService, and PaymentService, using message queues for event-driven interactions.](https://antondevtips.com/media/code_screenshots/dotnet/masstransit-transports/img_1.png)

For these services we need to install Postgres, RabbitMQ and Seq as docker containers via `docker-compose-yml`:

```yml
services:
  postgres:
    image: postgres:latest
    container_name: postgres
    environment:
      - TZ
      - POSTGRES_USER=admin 
      - POSTGRES_PASSWORD=admin
      - POSTGRES_DB=shipping
    volumes:
      - ./docker_data/pgdata:/var/lib/postgresql/data
    ports:
      - "5432:5432"
    restart: always
    networks:
      - docker-web

  rabbitmq:
    image: rabbitmq:3-management
    container_name: rabbitmq
    ports:
      - "15672:15672"
      - "5672:5672"
    restart: always
    volumes:
      - ./docker_data/rabbitmq:/var/lib/rabbitmq
    networks:
      - docker-web

  seq:
    image: datalust/seq:2024.3
    container_name: seq
    restart: always
    environment:
      - ACCEPT_EULA=Y
    volumes:
      - ./docker_data/seq:/data
    ports:
      - "5341:5341"
      - "8081:80"
    networks:
      - docker-web

networks:
  docker-web:
    driver: bridge
```

I use [Seq](https://antondevtips.com/blog/how-to-implement-structured-logging-and-distributed-tracing-for-microservices-with-seq/?utm_source=antondevtips&utm_medium=email&utm_campaign=13-05-2025-newsletter) to visualize logs and distributed traces involving the services.

`ShipmentService` implements the following use cases, available publicly in webapi:

1.  Create Shipment: saves shipment details to the database, publishes `ShipmentCreatedEvent` to a RabbitMQ and returns a shipment number.
2.  Update Shipment Status: updates the status of a shipment in the database and publishes `ShipmentStatusUpdatedEvent` to a RabbitMQ.
3.  `OrderService` consumes both `ShipmentCreatedEvent` and `ShipmentStatusUpdatedEvent` events from a RabbitMQ.

This approach makes service loosely coupled. Each service only "listens" to the events it cares about.

> If you want to learn how to observe microservices and their integration with each other, databases and other external services, you can check my [blog post here](https://antondevtips.com/blog/how-to-implement-structured-logging-and-distributed-tracing-for-microservices-with-seq).

## Using MassTransit with RabbitMQ

Many developers prefer RabbitMQ because it is easy to run locally, and it is free to run in production.

RabbitMQ is easy to run via Docker and offers a Web UI for managing and viewing queues, exchanges and messages.

If your application doesn't run on the Cloud (Azure or AWS) - RabbitMQ is often a recommended choice. MassTransit integrates seamlessly with it, giving you a consistent, message-driven application flow.

First, add the following Nuget package:

```bash
dotnet add package MassTransit.RabbitMQ
```

Here is how you can configure MassTransit with RabbitMQ:

```csharp
var rabbitMqConfiguration = configuration
    .GetSection(nameof(RabbitMQConfiguration))
    .Get<RabbitMQConfiguration>()!;

services.AddMassTransit(busConfig =>
{
    busConfig.SetKebabCaseEndpointNameFormatter();

    busConfig.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(rabbitMqConfiguration.Host), h =>
        {
            h.Username(rabbitMqConfiguration.Username);
            h.Password(rabbitMqConfiguration.Password);
        });

        cfg.ConfigureEndpoints(context);
    });
});
```

When running a service locally, you can use the following default credentials to connect to RabbitMQ:

```json
{
  "RabbitMQConfiguration": {
    "Host": "amqp://127.0.0.1:5672",
    "Username": "guest",
    "Password": "guest"
  }
}
```

You can explore the connections to RabbitMQ via RabbitMQ Management Web UI available on `http://localhost:15672/`:

![A screenshot of the RabbitMQ Management Web UI showing active connections from ShipmentService, PaymentService, and OrderService, along with their user, state, protocol, and network details.](https://antondevtips.com/media/code_screenshots/dotnet/masstransit-transports/img_2.png)

You can use the same `guest/guest` credentials to log into UI.

Let's explore a `CreateShipment` use case:

```csharp
public async Task<ErrorOr<CreateShipmentResponse>> Handle(
    CreateShipmentCommand request,
    CancellationToken cancellationToken)
{
    var shipmentAlreadyExists = await context.Shipments
        .Where(s => s.OrderId == request.OrderId)
        .AnyAsync(cancellationToken);

    if (shipmentAlreadyExists)
    {
        logger.LogInformation("Shipment for order '{OrderId}' is already created", request.OrderId);
        return Error.Failure($"Shipment for order '{request.OrderId}' is already created");
    }

    var shipmentNumber = new Faker().Commerce.Ean8();
    var shipment = CreateShipment(request, shipmentNumber);

    context.Shipments.Add(shipment);

    var shipmentCreatedEvent = CreateShipmentCreatedEvent(shipment);
    await publishEndpoint.Publish(shipmentCreatedEvent, cancellationToken);

    await context.SaveChangesAsync(cancellationToken);

    logger.LogInformation("Created shipment: {@Shipment}", shipment);

    return new CreateShipmentResponse(shipment.Number);
}
```

We're storing the shipment in the database and publishing `ShipmentCreatedEvent` to the RabbitMQ.

If you want to make your service more robust, you can add retries and [Outbox pattern](https://antondevtips.com/blog/use-masstransit-to-implement-outbox-pattern-with-ef-core-and-mongodb/?utm_source=antondevtips&utm_medium=email&utm_campaign=13-05-2025-newsletter):

```csharp
services.AddMassTransit(busConfig =>
{
    // Register Outbox
    busConfig.AddEntityFrameworkOutbox<EfCoreDbContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(30);
        o.UsePostgres().UseBusOutbox();
    });

    busConfig.SetKebabCaseEndpointNameFormatter();

    busConfig.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(new Uri(rabbitMqConfiguration.Host), h =>
        {
            h.Username(rabbitMqConfiguration.Username);
            h.Password(rabbitMqConfiguration.Password);
        });

        // Register Retries
        cfg.UseMessageRetry(r => r.Exponential(10, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(5)));

        cfg.ConfigureEndpoints(context);
    });
});
```

If RabbitMQ is unavailable or a network issue occurs, MassTransit will hold the message in the Outbox table and retry again until this message is successfully delivered to the message queue. MassTransit ensures that the message is delivered at least once.

Make sure you call `IPublishEndpoint.Publish` before saving changes to the database:

```csharp
var shipmentCreatedEvent = CreateShipmentCreatedEvent(shipment);
await publishEndpoint.Publish(shipmentCreatedEvent, cancellationToken);

await context.SaveChangesAsync(cancellationToken);
```

This way we ensure that a shipment is saved in the database together with an outbox message or neither of them is saved.

MassTransit supports Outbox implementation for EF Core and MongoDB, [learn more](https://antondevtips.com/blog/use-masstransit-to-implement-outbox-pattern-with-ef-core-and-mongodb/?utm_source=antondevtips&utm_medium=email&utm_campaign=13-05-2025-newsletter).

Here is how you can configure MassTransit for message consumption in `OrderService`:

```csharp
services.AddMassTransit(busConfig =>
{
    busConfig.SetKebabCaseEndpointNameFormatter();

    busConfig.AddConsumer<ShipmentCreatedConsumer>()
        .Endpoint(c => c.InstanceId = ServiceName);
    
    busConfig.AddConsumer<PaymentCompletedConsumer>()
        .Endpoint(c => c.InstanceId = ServiceName);

    busConfig.AddConsumer<PaymentFailedConsumer>()
        .Endpoint(c => c.InstanceId = ServiceName);

    busConfig.AddConfigureEndpointsCallback((context, name, cfg) =>
    {
        cfg.UseEntityFrameworkOutbox<EfCoreDbContext>(context);
    });

    busConfig.UsingRabbitMq((context, cfg) =>
    {
        //...
    });
});
```

`OrderService` consumes:

*   ShipmentCreatedEvent
*   PaymentCompletedEvent
*   PaymentFailedEvent

Here is a `ShipmentCreatedEvent` consumer implementation:

```csharp
public class ShipmentCreatedConsumer(
	IPublishEndpoint publishEndpoint,
	ILogger<ShipmentCreatedConsumer> logger)
	: IConsumer<ShipmentCreatedEvent>
{
	public async Task Consume(ConsumeContext<ShipmentCreatedEvent> context)
	{
		var message = context.Message;

		logger.LogInformation("Received shipment created event: {@Event}", message);

        // Save Order to the database
        
		var orderEvent = new OrderCreatedEvent(message.OrderId, DateTime.UtcNow, 100, message.ReceiverEmail);
		await publishEndpoint.Publish(orderEvent);
	}
}
```

Here, we are saving Order to the database and publishing `OrderCreatedEvent` to the RabbitMQ.

Here is the list of queues created in RabbitMQ for our services:

![A screenshot of the RabbitMQ Management Web UI displaying a list of queues created for the microservices, including `order-created-payment-service`, `payment-completed-order-service`, `payment-failed-order-service`, `shipment-created`, and `shipment-created-order-service`.](https://antondevtips.com/media/code_screenshots/dotnet/masstransit-transports/img_3.png)

As you can see, MassTransit makes it really easy to publish and consume events, abstracting from transport details and focusing on the actual business logic.

## Using MassTransit with Azure Service Bus

For cloud-native production environments on Azure, Azure Service Bus is often the go-to message broker. It's fully managed, highly scalable, and easily integrates with other Azure services.

MassTransit supports Service Bus out of the box.

First, you'll need to install the MassTransit package for Azure Service Bus integration:

```bash
dotnet add package MassTransit.Azure.ServiceBus.Core
```

Azure Service Bus supports both queue-based and topic-based messaging. You can still use the same MassTransit patterns (consumers, retries, sagas, etc.) — just swap out the transport:

```csharp
busConfig.UsingAzureServiceBus((context, cfg) =>
{
    cfg.Host(azureServiceBusConfiguration.ConnectionString);

    cfg.UseMessageRetry(r => r.Exponential(10, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(60), TimeSpan.FromSeconds(5)));

    cfg.ConfigureEndpoints(context);
});
```

To get started with Azure Service Bus, you can add it via Azure Portal. Look for a "Service Bus" resource:

![A screenshot of the Azure Portal Marketplace search results, highlighting the "Service Bus" resource for creating a new Azure Service Bus instance.](https://antondevtips.com/media/code_screenshots/dotnet/masstransit-transports/img_5.png)

For a pricing tier you need to select "Standard", which is required to access advanced messaging features such as topics/subscriptions and sessions.

And make sure to choose the region closest to your users or services.

Here is the list of queues created in Azure Service Bus for our services:

![A screenshot of the Azure Service Bus portal showing a list of topics and subscriptions created for the microservices, similar to the RabbitMQ queues.](https://antondevtips.com/media/code_screenshots/dotnet/masstransit-transports/img_4.png)

## MassTransit Features

MassTransit is more than just a "wrapper" around message brokers. It's an enterprise framework for building distributed .NET applications.

It has the following features:

**1. Message Routing**

*   Automatically handles broker topology (exchanges, queues) based on message types.
*   Simplifies publish/subscribe patterns with type-based message routing.

**2. Exception Handling**

*   Built-in retry policies, dead-letter queues and error queues.
*   Automatically redelivers or moves faulted messages without extra code.

**3. Sagas, State Machines**

*   Reliable, durable, event-driven workflow orchestration.
*   Maintains state across multiple messages and services.

**4. Request/Response**

*   Supports synchronous "request/response" patterns over the bus without tight coupling between the services.

**5. Inbox/Outbox Patterns**

*   Helps with message [idempotency](https://antondevtips.com/blog/use-masstransit-to-implement-outbox-pattern-with-ef-core-and-mongodb/?utm_source=antondevtips&utm_medium=email&utm_campaign=13-05-2025-newsletter) with a guarantee that the messages will be delivered at least once.

**6. Routing Slip Activities**

*   Complex distributed workflows with built-in compensation logic.

**7. Observability**

*   Native OpenTelemetry (OTEL) support for end-to-end tracing.
*   Simplifies distributed diagnostics.

**8. Scheduling:**

*   Allows scheduling of message delivery via transport delays, like Quartz.NET or Hangfire.
*   Perfect for delayed or recurring tasks.

**9. Dependency Injection**

*   Built-in service collection configuration.
*   Automatic scope management for consumers, sagas, etc.

**10. Test Harness**

*   In-memory testing that captures published, sent, and consumed messages.
*   Allows fast [unit/integration tests](https://antondevtips.com/blog/asp-net-core-integration-testing-best-practises/?utm_source=antondevtips&utm_medium=email&utm_campaign=13-05-2025-newsletter) without deploying a real broker.

**MassTransit currently supports:**

*   RabbitMQ
*   Kafka
*   Azure Service Bus
*   Amazon SQS/SNS
*   ActiveMQ
*   InMemory and SQL/DB.

A key reason many developers choose MassTransit is its simplicity in writing consumers and producers:

*   **Producers:** Send or publish messages without dealing directly with broker-specific details.
*   **Consumers:** Automatically wired up with dependency injection to handle incoming messages.
*   **Retries:** Easily configure the number of retry attempts before moving a message to an error queue.
*   **Request/Response:** Built-in support for loosely coupled, synchronous messaging patterns.

By the way, MediatR (the popular in-process mediator library) is also going commercial. For some teams, it might be "pay once" if you adopt both in MassTransit.

## Summary

MassTransit is a reliable and well-tested tool for building scalable and distributed applications. However, with the announcement of its move to a commercial licensing model in 2026, it's important to thoughtfully consider your project's future requirements.

Throughout this post, we've explored how MassTransit enables effective message-based communication through transports like RabbitMQ and Azure Service Bus. Its seamless integration capabilities allow you to switch or combine different messaging technologies based on your project's evolving requirements.

As you weigh your options, ask yourself:

*   Do you use advanced features like Sagas and Request/Response that only MassTransit can handle elegantly?
*   Does your team require enterprise-level support or guaranteed SLAs for mission-critical systems?
*   Could alternatives like Wolverine better meet your needs (but there is no guarantee that this library will remain free in the future) ?

For teams that depend on MassTransit's out-of-the-box reliability, message routing, or test harness, a license might be a worthwhile investment. Re-implementing some features on your own might cost you a few times more than using MassTransit.

The existing MassTransit v8 codebase will remain open-source and available under its current license. v9 will be commercial.

The Transition Plan from a [MassTransit team](https://masstransit.io/introduction/v9-announcement):

*   **Q3 2025:** MassTransit v9 prerelease packages available to early adopters.
*   **Q1 2026:** MassTransit v9 official release under a commercial license.
*   **After 2026:** End of official maintenance for MassTransit v8.

**Pricing Model:**

*   **Small/Medium Businesses:** $400 per month or $4,000 annually.
*   **Large Enterprises:** $1,200 per month or $12,000 annually.

If you are a cloud user, you can use native libraries for Azure Service Bus or AWS SQS/SNS instead. They are also reliable and well-tested.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/masstransit-rabbitmq-and-azure-service-bus-is-it-worth-a-commercial-license)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fmasstransit-rabbitmq-and-azure-service-bus-is-it-worth-a-commercial-license&title=MassTransit%20RabbitMQ%20and%20Azure%20Service%20Bus%3A%20Is%20It%20Worth%20a%20Commercial%20License)[X](https://twitter.com/intent/tweet?text=MassTransit%20RabbitMQ%20and%20Azure%20Service%20Bus%3A%20Is%20It%20Worth%20a%20Commercial%20License&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fmasstransit-rabbitmq-and-azure-service-bus-is-it-worth%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D%2D