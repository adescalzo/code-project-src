```yaml
---
title: Critter Stack Improvements for Event Driven Architecture – The Shade Tree Developer
source: https://jeremydmiller.com/2024/05/02/critter-stack-improvements-for-event-driven-architecture/?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=adventures-serializing-absolutely-everything-in-c
date_published: 2024-05-02T16:00:46.000Z
date_captured: 2025-08-21T18:08:48.268Z
domain: jeremydmiller.com
author: jeremydmiller
category: architecture
technologies: [Marten, Wolverine, .NET, PostgreSQL, Npgsql, RabbitMQ, ASP.NET Core, JasperFx Software]
programming_languages: [C#]
tags: [event-driven-architecture, event-sourcing, message-bus, dotnet, marten, wolverine, postgresql, messaging, data-streaming, microservices]
key_concepts: [event-driven-architecture, event-sourcing, message-bus, event-subscriptions, asynchronous-daemon, transactional-outbox, dependency-injection, event-carried-state-transfer]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores enhancements to the "Critter Stack" (Marten and Wolverine) for implementing robust Event Driven Architectures in .NET. It demonstrates how Wolverine's new Event Subscriptions model facilitates automatic processing of Marten event data through Wolverine message handlers. The content covers various subscription patterns, including publishing all events, filtering by event type, and ensuring strict order processing. Furthermore, it illustrates how to create custom batched subscriptions for event transformation and integration with external systems like RabbitMQ. The integration provides features like transactional outbox support and resilient error handling, significantly simplifying complex event-driven data exchange.]
---
```

# Critter Stack Improvements for Event Driven Architecture – The Shade Tree Developer

# Critter Stack Improvements for Event Driven Architecture

[jeremydmiller](https://jeremydmiller.com/author/jeremydmiller/ "Posts by jeremydmiller") [Uncategorized](https://jeremydmiller.com/category/uncategorized/) May 2, 2024 6 Minutes

[JasperFx Software](https://jasperfx.net/) is open for business and offering consulting services (like helping you craft modular monolith strategies!) and support contracts for both [Marten](https://martendb.io/) and [Wolverine](https://wolverinefx.io/) so you know you can feel secure taking a big technical bet on these tools and reap all the advantages they give for productive and maintainable server side .NET development.

![Two cartoonish weasel-like creatures, one brown and one yellow, are lifting a heavy barbell together in what appears to be a gym setting. The brown creature is on the left, holding one end of the barbell, and the yellow creature is on the right, holding the other end.](https://jeremydmiller.com/wp-content/uploads/2024/04/image-12.png?w=1024)

As a follow on post from [First Class Event Subscriptions in Marten](https://jeremydmiller.com/2024/04/25/first-class-event-subscriptions-in-marten/) last week, let’s introduce [Wolverine](https://wolverinefx.net) into the mix for end to end [Event Driven Architecture](https://www.thoughtworks.com/en-us/insights/decoder/e/event-driven-architecture) approaches. Using [Wolverine’s new Event Subscriptions model](https://wolverinefx.net/guide/durability/marten/subscriptions.html), “Critter Stack” systems can automatically process Marten event data with Wolverine message handlers:

![A flowchart illustrating the flow of events in a system. "Command Handlers" write events to a blue cylinder representing a database. The "Marten Async Daemon" polls the database for new events. From the daemon, events are either "Published" or "Invoked," leading to further processing represented by key icons.](https://jeremydmiller.com/wp-content/uploads/2024/05/image-2.png?w=1024)

If all we want to do is publish Marten event data through Wolverine’s message publishing (which remember, can be either to local queues or external message brokers), we have this simple recipe:

```csharp
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.Services
            .AddMarten()
            // Just pulling the connection information from
            // the IoC container at runtime.
            .UseNpgsqlDataSource()
            // You don't absolutely have to have the Wolverine
            // integration active here for subscriptions, but it's
            // more than likely that you will want this anyway
            .IntegrateWithWolverine()
            // The Marten async daemon most be active
            .AddAsyncDaemon(DaemonMode.HotCold)
            // This would attempt to publish every non-archived event
            // from Marten to Wolverine subscribers
            .PublishEventsToWolverine("Everything")
            // You wouldn't do this *and* the above option, but just to show
            // the filtering
            .PublishEventsToWolverine("Orders", relay =>
            {
                // Filtering
                relay.FilterIncomingEventsOnStreamType(typeof(Order));
                // Optionally, tell Marten to only subscribe to new
                // events whenever this subscription is first activated
                relay.Options.SubscribeFromPresent();
            });
    }).StartAsync();
```

First off, what’s a “subscriber?” _That_ would mean any event that Wolverine recognizes as having:

Advertisement

*   A local message handler in the application for the specific event type, which would effectively direct Wolverine to publish the event data to a local queue
*   A local message handler in the application for the specific `IEvent<T>` type, which would effectively direct Wolverine to publish the event with its `IEvent` Marten metadata wrapper to a local queue
*   Any event type where Wolverine can discover subscribers through routing rules

All the Wolverine subscription is doing is effectively calling `IMessageBus.PublishAsync()` against the event data or the `IEvent<T>` wrapper. You can make the subscription run more efficiently by applying event or stream type filters for the subscription.

If you need to do a transformation of the raw `IEvent<T>` or the internal event type to some kind of external event type for publishing to external systems when you want to avoid directly coupling other subscribers to your system’s internals, you can accomplish that by just building a message handler that does the transformation and publishes a cascading message like so:

```csharp
public record OrderCreated(string OrderNumber, Guid CustomerId);

// I wouldn't use this kind of suffix in real life, but it helps
// document *what* this is for the sample in the docs:)
public record OrderCreatedIntegrationEvent(string OrderNumber, string CustomerName, DateTimeOffset Timestamp);

// We're going to use the Marten IEvent metadata and some other Marten reference
// data to transform the internal OrderCreated event
// to an OrderCreatedIntegrationEvent that will be more appropriate for publishing to
// external systems
public static class InternalOrderCreatedHandler
{
    public static Task<Customer?> LoadAsync(IEvent<OrderCreated> e, IQuerySession session,
        CancellationToken cancellationToken)
        => session.LoadAsync<Customer>(e.Data.CustomerId, cancellationToken);

    public static OrderCreatedIntegrationEvent Handle(IEvent<OrderCreated> e, Customer customer)
    {
        return new OrderCreatedIntegrationEvent(e.Data.OrderNumber, customer.Name, e.Timestamp);
    }
}
```

## Process Events as Messages in Strict Order

In some cases you may want the events to be executed by Wolverine message handlers in strict order. With the recipe below:

```csharp
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.Services
            .AddMarten(o =>
            {
                // This is the default setting, but just showing
                // you that Wolverine subscriptions will be able
                // to skip over messages that fail without
                // shutting down the subscription
                o.Projections.Errors.SkipApplyErrors = true;
            })
            // Just pulling the connection information from
            // the IoC container at runtime.
            .UseNpgsqlDataSource()
            // You don't absolutely have to have the Wolverine
            // integration active here for subscriptions, but it's
            // more than likely that you will want this anyway
            .IntegrateWithWolverine()
            // The Marten async daemon most be active
            .AddAsyncDaemon(DaemonMode.HotCold)
            // Notice the allow list filtering of event types and the possibility of overriding
            // the starting point for this subscription at runtime
            .ProcessEventsWithWolverineHandlersInStrictOrder("Orders", o =>
            {
                // It's more important to create an allow list of event types that can be processed
                o.IncludeType<OrderCreated>();
                // Optionally mark the subscription as only starting from events from a certain time
                o.Options.SubscribeFromTime(new DateTimeOffset(new DateTime(2023, 12, 1)));
            });
    }).StartAsync();
```

In this recipe, Marten & Wolverine are working together to call `IMessageBus.InvokeAsync()` on each event in order. You can use both the actual event type (`OrderCreated`) or the wrapped Marten event type (`IEvent<OrderCreated>`) as the message type for your message handler.

Advertisement

In the case of exceptions from processing the event with Wolverine:

1.  Any built in “retry” error handling will kick in to retry the event processing inline
2.  If the retries are exhausted, and the Marten setting for `StoreOptions.Projections.Errors.SkipApplyErrors` is `true`, Wolverine will persist the event to its PostgreSQL backed dead letter queue and proceed to the next event. This setting is the default with Marten when the daemon is running continuously in the background, but `false` in rebuilds or replays
3.  If the retries are exhausted, and `SkipApplyErrors = false`, Wolverine will tell Marten to pause the subscription at the last event sequence that succeeded

## Custom, Batched Subscriptions

The base type for all Wolverine subscriptions is the `Wolverine.Marten.Subscriptions.BatchSubscription` class. If you need to do something completely custom, or just to take action on a batch of events at one time, subclass that type. Here is an example usage where I’m using [event carried state transfer](https://martinfowler.com/articles/201701-event-driven.html) to publish batches of reference data about customers being activated or deactivated within our system:

```csharp
public record CompanyActivated(string Name);

public record CompanyDeactivated();

public record NewCompany(Guid Id, string Name);

// Message type we're going to publish to external
// systems to keep them up to date on new companies
public class CompanyActivations
{
    public List<NewCompany> Additions { get; set; } = new();
    public List<Guid> Removals { get; set; } = new();
    public void Add(Guid companyId, string name)
    {
        Removals.Remove(companyId);
        // Fill is an extension method in JasperFx.Core that adds the
        // record to a list if the value does not already exist
        Additions.Fill(new NewCompany(companyId, name));
    }
    public void Remove(Guid companyId)
    {
        Removals.Fill(companyId);
        Additions.RemoveAll(x => x.Id == companyId);
    }
}

public class CompanyTransferSubscription : BatchSubscription
{
    public CompanyTransferSubscription() : base("CompanyTransfer")
    {
        IncludeType<CompanyActivated>();
        IncludeType<CompanyDeactivated>();
    }
    public override async Task ProcessEventsAsync(EventRange page, ISubscriptionController controller, IDocumentOperations operations,
        IMessageBus bus, CancellationToken cancellationToken)
    {
        var activations = new CompanyActivations();
        foreach (var e in page.Events)
        {
            switch (e)
            {
                // In all cases, I'm assuming that the Marten stream id is the identifier for a customer
                case IEvent<CompanyActivated> activated:
                    activations.Add(activated.StreamId, activated.Data.Name);
                    break;
                case IEvent<CompanyDeactivated> deactivated:
                    activations.Remove(deactivated.StreamId);
                    break;
            }
        }
        // At the end of all of this, publish a single message
        // In case you're wondering, this will opt into Wolverine's
        // transactional outbox with the same transaction as any changes
        // made by Marten's IDocumentOperations passed in, including Marten's
        // own work to track the progression of this subscription
        await bus.PublishAsync(activations);
    }
}
```

And the related code to register this subscription:

```csharp
using var host = await Host.CreateDefaultBuilder()
    .UseWolverine(opts =>
    {
        opts.UseRabbitMq();
        // There needs to be *some* kind of subscriber for CompanyActivations
        // for this to work at all
        opts.PublishMessage<CompanyActivations>()
            .ToRabbitExchange("activations");
        opts.Services
            .AddMarten()
            // Just pulling the connection information from
            // the IoC container at runtime.
            .UseNpgsqlDataSource()
            .IntegrateWithWolverine()
            // The Marten async daemon most be active
            .AddAsyncDaemon(DaemonMode.HotCold)
            // Register the new subscription
            .SubscribeToEvents(new CompanyTransferSubscription());
    }).StartAsync();
```

## Summary

The feature set shown here has been a very long planned set of capabilities to truly extend the “Critter Stack” into the realm of supporting Event Driven Architecture approaches from soup to nuts. Using the Wolverine subscriptions automatically gets you support to publish Marten events to any transport supported by Wolverine itself, and does so in a much more robust way than you can easily roll by hand like folks did previously with Marten’s `IProjection` interface. I’m currently helping a [JasperFx Software](https://jasperfx.net) client utilize this functionality for data exchange that has strict ordering and at least once delivery guarantees.

Advertisement

### Share this:

*   [Click to share on X (Opens in new window) X](https://jeremydmiller.com/2024/05/02/critter-stack-improvements-for-event-driven-architecture/?share=twitter&nb=1)
*   [Click to share on Facebook (Opens in new window) Facebook](https://jeremydmiller.com/2024/05/02/critter-stack-improvements-for-event-driven-architecture/?share=facebook&nb=1)

Like Loading...

*   Tagged
*   [Marten](https://jeremydmiller.com/tag/marten/)
*   [Wolverine](https://jeremydmiller.com/tag/wolverine/)

![Unknown's avatar](https://0.gravatar.com/avatar/c4ea4a7e0b4088fb62a4d5386566b6bd45df7b1d33100c49bb775fa6edfff6fa?s=80&d=identicon&r=G)

## Published by jeremydmiller

[View all posts by jeremydmiller](https://jeremydmiller.com/author/jeremydmiller/)

**Published** May 2, 2024

## Post navigation

[Previous Post Marten, PostgreSQL, and .NET Aspire walk into a bar…](https://jeremydmiller.com/2024/05/01/marten-postgresql-and-net-aspire-walk-into-a-bar/)

[Next Post Recent Marten & Wolverine Improvements and Roadmap Update](https://jeremydmiller.com/2024/05/03/recent-marten-wolverine-improvements-and-roadmap-update/)

### Leave a comment [Cancel reply](/2024/05/02/critter-stack-improvements-for-event-driven-architecture/?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=adventures-serializing-absolutely-everything-in-c#respond)

Δdocument.getElementById( "ak_js_1" ).setAttribute( "value", ( new Date() ).getTime() );