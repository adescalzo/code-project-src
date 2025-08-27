```yaml
---
title: Building High-Performance .NET Apps With C# Channels
source: https://antondevtips.com/blog/building-high-performance-dotnet-apps-with-csharp-channels?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2104#what-is-a-bounded-channel
date_published: 2025-08-05T07:45:14.490Z
date_captured: 2025-08-11T19:12:11.938Z
domain: antondevtips.com
author: Anton Martyniuk
category: performance
technologies: [.NET, C# Channels, System.Threading.Channels, ASP.NET Core, HybridCache, BackgroundService, "Queue<T>", "ConcurrentQueue<T>", "BlockingCollection<T>"]
programming_languages: [C#]
tags: [dotnet, csharp, channels, concurrency, asynchronous-programming, producer-consumer, performance, background-tasks, web-api, caching]
key_concepts: [csharp-channels, producer-consumer-pattern, asynchronous-programming, concurrency, bounded-channels, unbounded-channels, background-services, write-back-caching, flow-control, thread-safety]
code_examples: false
difficulty_level: intermediate
summary: |
  This article delves into C# Channels, presenting them as a modern solution for building high-performance, concurrent .NET applications. It explains the producer-consumer pattern implemented by Channels, differentiating between bounded and unbounded types and their respective use cases. The content demonstrates practical applications, including integrating Channels into ASP.NET Core background services for efficient task processing and showcasing a real-world write-back caching strategy. The article concludes with essential best practices for working with Channels, emphasizing safety, proper completion, and cancellation handling to optimize application stability and responsiveness.
---
```

# Building High-Performance .NET Apps With C# Channels

![Building High-Performance .NET Apps With C# Channels - Dev Tips newsletter cover image](https://antondevtips.com/media/covers/csharp/cover_csharp_channels.png)

# Building High-Performance .NET Apps With C# Channels

Aug 5, 2025

[Download source code](/source-code/building-high-performance-dotnet-apps-with-csharp-channels)

8 min read

[Sponsor my newsletter to reach 12,000+ readers](/sponsorship)

Building reliable, scalable, and high-performance .NET applications often comes down to how you handle concurrency and data processing. **C# Channels** bring a new, modern approach for building safe, asynchronous, and high-throughput pipelines in .NET.

Channels allow you to create in-memory producer-consumer queues that scale naturally across async workflows and background services. However, a crucial architectural decision is choosing between bounded and unbounded channels.

In today's post, we will explore:

*   What are C# Channels?
*   Bounded vs. Unbounded Channels
*   Background Processing with Channels
*   Using channels in an ASP.NET Core real-world application
*   Best practices and tips for working with Channels

Let's dive in!

## What are C# Channels?

When building .NET apps, you often need to send data from one part of your code to another.

In the past, developers used constructs such as Queue<T>, ConcurrentQueue<T>, or BlockingCollection<T> for this purpose. They encapsulated these queues into classes and used them to manage their data flows.

However, such implementations have a significant drawback: tight code coupling.

C# Channels solve this problem. They implement a producer-consumer pattern. One class produces data and the other consumes it, without knowing about each other.

C# Channels come from the `System.Threading.Channels` namespace. Channels make it simple to send data between producers and consumers, helping you avoid common threading problems.

A channel has two parts:

*   **Writer:** pushes data into the Channel.
*   **Reader:** pulls data out of the Channel.

Both reading and writing can occur on different threads, and Channels ensure thread safety. They let you use async code everywhere, so your app can handle lots of data without blocking threads or locking.

Here's a basic example of using channels in C#. Here we asynchronously produce numbers and consume them from the Channel:

```csharp
using System;
using System.Threading.Channels;
using System.Threading.Tasks;

var channel = Channel.CreateUnbounded<int>();

// Producer
_ = Task.Run(async () =>
{
    for (var i = 0; i < 10; i++)
    {
        await channel.Writer.WriteAsync(i);
        Console.WriteLine($"Produced: {i}");
        await Task.Delay(100); // simulate work
    }
    channel.Writer.Complete();
});

// Consumer
await foreach (var item in channel.Reader.ReadAllAsync())
{
    Console.WriteLine($"Consumed: {item}");
    await Task.Delay(150); // simulate processing
}

Console.WriteLine("Processing complete.");
```

Here is how it works:

*   The producer runs on a separate Thread (Task), writing data to the Channel.
*   The consumer reads data from the Channel and processes it.
*   The Channel handles all communication and thread safety.

Channels are a great fit when:

*   You need to connect producers and consumers using async/await.
*   You want to decouple the producing and consuming logic
*   You want to control the flow of data (for example, slow down producers if consumers can't keep up).
*   You want to avoid low-level threading, locking, or manual synchronization.

Channels are ideal for streaming events and processing background tasks. They are a simple in-memory alternative to message queues within a single application.

## Bounded vs. Unbounded Channels

C# channels can be of 2 types: **bounded** and **unbounded**. Both let you pass data from a producer to a consumer, but they handle flow control and memory differently.

### What is a Bounded Channel?

A bounded channel has a fixed maximum capacity. When you create it, you set the limit for the number of items it can hold at once.

If the producer tries to add more items after the Channel is full, it has to wait until there is space.

When should you use a bounded channel?

*   When you want to limit memory usage and prevent overload.
*   When the consumer is sometimes slower than the producer.
*   When you need backpressure to avoid flooding your system.

```csharp
var channel = Channel.CreateBounded<int>(5);

// Producer
await channel.Writer.WriteAsync(1); // Adds item if there's space, waits if full

// Consumer
var item = await channel.Reader.ReadAsync(); // Takes an item
```

Bounded channels are a good choice for background processing, job queues, and any application where you need to control resource usage. They help protect your app from sudden spikes and runaway producers.

### What is an Unbounded Channel?

An unbounded channel has no fixed limit. The producer can keep adding items as fast as they want.

The Channel will grow to handle as many items as you put in it, limited only by available system memory.

When should you use an unbounded channel?

*   When you are sure the producer will never outpace the consumer for long.
*   For simple cases where flow control is not a problem.
*   When you expect only a small or steady stream of items.

Here is how to create an unbounded channel:

```csharp
var channel = Channel.CreateUnbounded<int>();

// Producer
await channel.Writer.WriteAsync(42); // Always accepts new items

// Consumer
var item = await channel.Reader.ReadAsync(); // Takes an item
```

Unbounded channels are simple to use, but they can be risky in high-load situations. If the producer writes items faster than the consumer reads them, you can run out of memory.

Which one should you use?

If you want safety and stability in your system, start with bounded channels. They protect your app if the consumer falls behind.

If you know your producer is always in control and the data rate is low, you can use unbounded channels.

In most real-world .NET services, bounded channels are the safer default.

## Background Processor with a Bounded Channel

In production, channels are often used in ASP.NET Core background services.

A common pattern is to set up a background service that reads messages or tasks from a channel and processes them one by one. This makes your code easy to scale and keeps the main thread free for other work.

Let's explore an example of a BackgroundService that processes items from a Channel:

```csharp
builder.Services.AddSingleton(_ => Channel.CreateBounded<string>(new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait
}));

public class MessageProcessor : BackgroundService
{
    private readonly Channel<string> _channel;
    private readonly ILogger<MessageProcessor> _logger;

    public MessageProcessor(Channel<string> channel, ILogger<MessageProcessor> logger)
    {
       _channel = channel;
       _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
       _logger.LogInformation("Message processor starting");

       await foreach (var message in _channel.Reader.ReadAllAsync(stoppingToken))
       {
          _logger.LogInformation("Processing message: {Message}", message);

          await Task.Delay(100, stoppingToken);

          _logger.LogInformation("Message processed successfully: {Message}", message);
       }
    }
}
```

To publish items into the Channel, you need to call the `WriteAsync` method:

```csharp
private Task AddSampleMessagesAsync(CancellationToken stoppingToken)
{
    _ = Task.Run(async () =>
    {
        // Add 150 messages (more than channel capacity of 100)
        for (var i = 1; i <= 150 && !stoppingToken.IsCancellationRequested; i++)
        {
            var message = $"Sample message #{i}";

            // Wait until there's space in the Channel (this will block when the Channel is full)
            await _channel.Writer.WriteAsync(message, stoppingToken);

            _logger.LogInformation("Added message to channel: {Message}", message);

            await Task.Delay(50, stoppingToken);
        }

        // Complete the Channel when done adding messages
        _channel.Writer.Complete();
    }, stoppingToken);
    return Task.CompletedTask;
}
```

Notice that we run the foreach loop over `AsyncEnumerable`:

```csharp
await foreach (var message in _channel.Reader.ReadAllAsync(stoppingToken))
{
    // Process message
}
```

The `_channel.Reader.ReadAllAsync` waits for a new message to appear in the Channel. When done publishing messages, you can call `_channel.Writer.Complete()` and the loop will end.

The Channel is bounded with a capacity of 100, so if 100 messages are already waiting, the producer will pause until a free slot becomes available. The background service reads messages as fast as it can process them.

If you try to add messages too quickly, the producer will slow down, which keeps your memory usage under control.

When creating a bounded channel, you can set `BoundedChannelFullMode` to control what happens when the Channel is full:

*   Wait: The writer waits until space is available (most common, safest).
*   DropWrite: New items are dropped if the Channel is full.
*   DropOldest: The oldest item is removed to make space for the new one.
*   DropNewest: The newest item is dropped instead.

For most background tasks, use `Wait`. When losing a few messages is acceptable, `DropWrite` or `DropOldest` can be a suitable option. You can use `DropOldest` when the latest event is more relevant than the older ones.

## Real-World Application with Channels

In a real-world ASP.NET Core application, you can use Channels to implement [Write Back Caching Strategy](https://antondevtips.com/blog/how-to-implement-caching-strategies-in-dotnet#4-write-back).

This caching strategy is used in high-speed, write-intensive scenarios.

The main idea is that the data is first written to the cache. The cache asynchronously writes the data back to the database after a certain condition or interval.

Let's explore a real-world example: an Online Store application.

Users frequently add or remove items from their online shopping carts. The final state is only really critical at checkout. Writes can be very fast since they hit the cache first, and the system can periodically flush updates to the database.

This enables high write throughput during peak shopping times, with the database eventually getting the final cart details.

Here is the WebApi endpoint that creates a product cart:

```csharp
public record ProductCartRequest(string UserId, List<ProductCartItemRequest> ProductCartItems);

public record ProductCartItemRequest(Guid ProductId, int Quantity);

[HttpPost]
public async Task<ActionResult<ProductCartResponse>> CreateCart(ProductCartRequest request)
{
    var response = await _service.AddAsync(request);
    return CreatedAtAction(nameof(GetCart), new { id = response.Id }, response);
}
```

When creating a new `ProductCart`, it is immediately added to the cache only:

```csharp
public class WriteBackCacheProductCartService
{
    private readonly HybridCache _cache;
    private readonly IProductCartRepository _repository;
    private readonly Channel<ProductCartDispatchEvent> _channel;

    public WriteBackCacheProductCartService(
        HybridCache cache,
        IProductCartRepository repository,
        Channel<ProductCartDispatchEvent> channel)
    {
        _cache = cache;
        _repository = repository;
        _channel = channel;
    }

    public async Task<ProductCartResponse> AddAsync(ProductCartRequest request)
    {
        var productCart = new ProductCart
        {
            Id = Guid.NewGuid(),
            UserId = request.UserId,
            CartItems = request.ProductCartItems.Select(x => new CartItem
            {
                Id = Guid.NewGuid(),
                Quantity = x.Quantity,
                Price = Random.Shared.Next(100, 1000)
            }).ToList()
        };

        var cacheKey = $"productCart:{productCart.Id}";

        var productCartResponse = MapToProductCartResponse(productCart);
        await _cache.SetAsync(cacheKey, productCartResponse);

        await _channel.Writer.WriteAsync(new ProductCartDispatchEvent(productCart));

        return productCartResponse;
    }
}
```

Here we use a Bounded Channel to publish `ProductCartDispatchEvent`.

```csharp
public record ProductCartDispatchEvent(ProductCart ProductCart);

builder.Services.AddSingleton(_ => Channel.CreateBounded<ProductCartDispatchEvent>(new BoundedChannelOptions(100)
{
    FullMode = BoundedChannelFullMode.Wait
}));
```

A background reads from the Channel and writes cart data to the database asynchronously:

```csharp
public class WriteBackCacheBackgroundService(IServiceScopeFactory scopeFactory,
    Channel<ProductCartDispatchEvent> channel) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var command in channel.Reader.ReadAllAsync(stoppingToken))
        {
            using var scope = scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IProductCartRepository>();

            var existingCart = await repository.GetByIdAsync(command.ProductCart.Id);
            if (existingCart is null)
            {
                await repository.AddAsync(command.ProductCart);
                return;
            }

            existingCart.CartItems = command.ProductCart.CartItems;
            await repository.UpdateAsync(existingCart);
        }
    }
}
```

> Note: This pattern can significantly speed up writes, but it requires robust conflict resolution and failure handling to ensure consistency. Ensuring the cart state is never lost in case of a cache failure, requires robust replication or backup mechanisms.

## Best Practices and Tips for Working with Channels

Channels are a powerful tool, but to get the most out of them (and avoid problems), it's important to follow a few key best practices. Here are some practical tips for using channels in your .NET applications.

**1\. Prefer Bounded Channels for Safety:**

Most of the time, you should start with a bounded channel. A bounded channel sets a limit on the amount of data that can be waiting to be processed. This makes your application more stable under a heavy load.

Set the capacity to a reasonable number for your workload. If you expect spikes in data, pick a value that covers typical bursts but is not too large.

Unbounded channels are only safe when you know the data rate is always low.

**2\. Remember to Complete the Channel:**

When your producer is done writing, call `.Writer.Complete()` on the Channel. This lets the consumer know that there will be no more data, allowing it to finish.

If you don't complete the Channel, your consumer's ReadAllAsync loop will never end.

**3\. Use await with Write and Read Operations:**

Channels are made for async code. Always use `await` when writing or reading to avoid blocking threads and to keep your app responsive.

```csharp
await Channel.Writer.WriteAsync(data, cancellationToken);
await foreach (var item in channel.Reader.ReadAllAsync(cancellationToken))
{
    // process item
}
```

**4\. Handle Cancellation Properly:**

Always pass a `CancellationToken` when reading or writing to a channel. This allows you to stop processing if your app is shutting down or if a user cancels an operation.

```csharp
await channel.Writer.WriteAsync(message, cancellationToken);
```

**5\. Don't Share Channels Between Too Many Producers or Consumers:**

While channels can handle multiple writers and readers, having too many can make things hard to debug. In most cases, stick to a single producer and a single consumer for best performance and easiest reasoning.

If you need more, .NET channels support multiple readers and writers, but you should design carefully to avoid surprises.

**6\. Monitor Your Channel Usage:**

Watch your Channel's capacity in production. If you often see the Channel filling up (producers waiting to write), it may mean your consumers are too slow. You might need to speed up your processing or increase the channel size.

**7\. Choose the Right FullMode (For Bounded Channels):**

When creating a bounded channel, you can set `BoundedChannelFullMode` to control what happens when the Channel is full.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/building-high-performance-dotnet-apps-with-csharp-channels)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fbuilding-high-performance-dotnet-apps-with-csharp-channels&title=Building%20High-Performance%20.NET%20Apps%20With%20C%23%20Channels)[X](https://twitter.com/intent/tweet?text=Building%20High-Performance%20.NET%20Apps%20With%20C%23%20Channels&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fbuilding-high-performance-dotnet-apps-with-csharp-channels)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fbuilding-high-performance-dotnet-apps-with-csharp-channels)

# Improve Your **.NET** and Architecture Skills

Join my community of **12,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 12,000+ Subscribers

Join 12,000+ developers already reading

No spam. Unsubscribe any time.