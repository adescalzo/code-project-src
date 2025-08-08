# Migrating to Brighter V10 with RabbitMQ | by Rafael Andrade | Jul, 2025 | Medium

**Source:** https://medium.com/@actor-swe/migrating-to-brighter-v10-with-rabbitmq-eeebebb4462d
**Date Captured:** 2025-07-28T16:16:06.839Z
**Domain:** medium.com
**Author:** Rafael Andrade
**Category:** general

---

# Migrating to Brighter V10 with RabbitMQ

[

![Rafael Andrade](https://miro.medium.com/v2/resize:fill:64:64/1*cdc8thqXOlf2c6YJiLYDug.jpeg)





](/@actor-swe?source=post_page---byline--eeebebb4462d---------------------------------------)

[Rafael Andrade](/@actor-swe?source=post_page---byline--eeebebb4462d---------------------------------------)

Follow

3 min read

·

Jul 14, 2025

17

1

Listen

Share

More

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/1*1BZfFIkzBgBPUKlM4M3N0g.png)

In previous articles, I covered [Brighter integration with RabbitMQ](/@actor-swe/brighter-and-rabbitmq-how-to-setup-and-use-brighter-with-rabbitmq-dde838cc172e) and [Brighter V10 RC](/p/d49f2cd2f921)1\. This guide focuses on migrating to Brighter V10, emphasizing RabbitMQ configuration changes and breaking updates.

# Requirement

.NET 8 or superior

A .NET project with these NuGet packages

*   [Paramore.Brighter.MessagingGateway.RMQ.Sync](https://www.nuget.org/packages/Paramore.Brighter.MessagingGateway.RMQ.Sync/): Enables RabbitMQ integration using the RabbitMQ.Client V6 (with sync api).
*   [Paramore.Brighter.MessagingGateway.RMQ.Async](https://www.nuget.org/packages/Paramore.Brighter.MessagingGateway.RMQ.Sync/): Enables RabbitMQ integration using the RabbitMQ.Client V7 (with async api).
*   [Paramore.Brighter.ServiceActivator.Extensions.DependencyInjection](https://www.nuget.org/packages/Paramore.Brighter.ServiceActivator.Extensions.Hosting/): Enable register Brighter with Microsoft DI.
*   [Paramore.Brighter.ServiceActivator.Extensions.Hosting](https://www.nuget.org/packages/Paramore.Brighter.ServiceActivator.Extensions.Hosting/): Hosts Brighter as a background service.
*   [Serilog.AspNetCore](https://www.nuget.org/packages/Serilog.AspNetCore/): For structured logging (optional but recommended).

# Brighter Recap

Before continuing about RabbitMQ configuration, let’s recap what we already know about Brighter.

## Request (Command/Event)

Define messages using `IRequest`:

public class Greeting() : Event(Guid.NewGuid())  
{  
    public string Name { get; set; } = string.Empty;  
}

*   Commands: Single-recipient operations (e.g., `SendEmail`).
*   Events: Broadcast notifications (e.g., `OrderShipped`).

## Message Mapper (Optional)

Translates between Brighter messages and your app objects:

public class GreetingMapper : IAmAMessageMapper<Greeting\>  
{  
    public Message MapToMessage(Greeting request, Publication publication)  
    {  
        var header = new MessageHeader  
        {  
            MessageId = request.Id,  
            TimeStamp = DateTimeOffset.UtcNow,  
            Topic = publication.Topic!,  
            MessageType = MessageType.MT\_EVENT  
        };  
  
        var body = new MessageBody(JsonSerializer.Serialize(request));  
        return new Message(header, body);  
    }  
    public Greeting MapToRequest(Message message)  
    {  
        return JsonSerializer.Deserialize<Greeting>(message.Body.Bytes)!;  
    }  
    public IRequestContext? Context { get; set; }  
}

**V10 Change**: Async mappers now require `IAmAMessageMapperAsync`

## Request Handler

Processes incoming messages:

public class GreetingHandler(ILogger<GreetingHandler> logger) : RequestHandler<Greeting>  
{  
    public override Greeting Handle(Greeting command)  
    {  
        logger.LogInformation("Hello {Name}", command.Name);  
        return base.Handle(command);  
    }  
}

# Configuring Brighter with RabbitMQ

## 1\. Choosing Sync vs. Async

*   Sync (_RMQ.Sync_) : Uses _RabbitMQ.Client_ V6 (blocking API). Suitable for gradual migrations.
*   Async (_RMQ.Async_) : Uses _RabbitMQ.Client_ V7 (fully async API). Recommended for new projects

**Tip**: Migrate to Brighter V10 first, then switch to `RMQ.Async` to isolate changes.

## 2\. Connection Setup

Define RabbitMQ connection details:

var connection = new RmqMessagingGatewayConnection  
{  
    AmpqUri = new AmqpUriSpecification(new Uri("amqp://guest:guest@localhost:5672")),  
    Exchange = new Exchange("paramore.brighter.exchange"),  
};

## 3\. RabbitMQ Subscription

Subscribe to a queue/topic:

.AddServiceActivator(opt =>  
{  
    opt.Subscriptions = \[  
       new RmqSubscription<Greeting>(  
           new SubscriptionName("kafka.greeting.subscription"),  
           new ChannelName("greeting.queue"),  
           new RoutingKey("greeting.topic"),  
           makeChannels: OnMissingChannel.Create,  
           messagePumpType: MessagePumpType.Reactor  
       ),  
    \];  
  
    opt.DefaultChannelFactory = new ChannelFactory(new RmqMessageConsumerFactory(connection));  
})

## 4\. RabbitMQ Producer Configuration

Publish events to a topic:

.UseExternalBus(opt =>  
{  
    opt.ProducerRegistry = new RmqProducerRegistryFactory(connection, \[  
       new RmqPublication<Greeting>  
       {  
           MakeChannels = OnMissingChannel.Create,  
           Topic = new RoutingKey("greeting.topic"),  
       },  
    \]).Create();  
});

# Breaking Changes in Brighter V10

Brighter V10 introduces significant updates to RabbitMQ integration, primarily driven by the adoption of `RabbitMQ.Client` V6/V7 and improved async/sync separation. Below are the key breaking changes:

## Package rename

To align with `RabbitMQ.Client` versioning and clarify async/sync usage:

Old: `Paramore.Brighter.MessagingGateway.RMQ`

New:

*   `Paramore.Brighter.MessagingGateway.RMQ.Sync` (V6 sync API)
*   `Paramore.Brighter.MessagingGateway.RMQ.Async` (V7 async API)

**Why?** `RabbitMQ.Client` V7 introduced breaking changes requiring code-level adjustments. The split ensures explicit compatibility and avoids runtime surprises.

## Message Mapper Overhaul

**Default JSON Serialization :**

In V9, message mappers were mandatory. In V10, JSON serialization is built-in unless custom logic is required.

**Interface Split**

*   `IAmAMessageMapper` (sync/reactor): For synchronous workflows.
*   `IAmAMessageMapperAsync` (async/proactor): For asynchronous workflows.

**Changes on the** `**IAmAMessageMapper**`

// V10  
IRequestContext? Context { get; set; }  
Message MapToMessage(Greeting request, Publication publication);  
  
// V9  
Message MapToMessage(Greeting request);

# Subscription

We had 2 main changes on subscription.

**Explicit Message Pump Types**

The first one is before we had a field called `runAsync` or `isAsync` it was a boolean, to make everything clear we change it to `messagePumpType` and it's the `MessagePumpType`(`Reactor`, `Proactor`, `Unknown`).

**Property Renaming**

On the `AddServiceActivator` where rename the `ChannelFactory` property to `DefaultChannelFactory`

## Publication

Here we also have 2 main changes

**Configuration Overhaul**

Removed `IAmAProducerRegistry`. Use `ExternalBusConfiguration` to configure producers and outbox patterns:

// V10  
.UseExternalBus(opt => { ... })  
  
// V9  
.UseExternalBus(new RmqProducerRegistryFactory(...))

**Request Type Specification**

The other change was about the `Publication`, you must setup an the request type or change the default `IAmAPublicationFinder` by `UsePublicationFinder`:

new RmqPublication<Greeting>  
{  
    MakeChannels = OnMissingChannel.Create,  
    Topic = new RoutingKey("greeting.topic"),  
}  
  
// or  
new RmqPublication  
{  
    MakeChannels = OnMissingChannel.Create,  
    Topic = new RoutingKey("greeting.topic"),  
    RequestType = typeof(Greeting)  
}

**Migration Tips**

*   Start with `RMQ.Sync`: Migrate to Brighter V10 first using `RabbitMQ.Client` V6 before adopting the async V7 package.
*   Audit Mappers: Replace `IAmAMessageMapper` with `IAmAMessageMapperAsync` for async workflows.
*   Validate Subscriptions: Ensure `messagePumpType` matches your async/sync requirements.
*   Update Publications: Add `RequestType` to non-generic `RmqPublication` instances or set a \``IAmAPublicationFinder`.

# Conclusion

Brighter V10 simplifies RabbitMQ integration with clearer async/sync separation and reduced boilerplate. Key steps include:

*   Updating NuGet packages
*   Refactoring message mappers to async patterns