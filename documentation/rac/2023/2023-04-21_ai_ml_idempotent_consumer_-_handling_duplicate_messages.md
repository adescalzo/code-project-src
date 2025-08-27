```yaml
---
title: Idempotent Consumer - Handling Duplicate Messages
source: https://www.milanjovanovic.tech/blog/idempotent-consumer-handling-duplicate-messages?utm_source=LinkedIn&utm_medium=social&utm_campaign=25.08.2025
date_published: 2023-04-22T00:00:00.000Z
date_captured: 2025-08-29T14:48:07.692Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: ai_ml
technologies: [Hasura, SQL Server, Rebus, .NET]
programming_languages: [C#]
tags: [idempotency, message-queues, distributed-systems, event-driven-architecture, design-patterns, dotnet, csharp, error-handling, data-access]
key_concepts: [Idempotent Consumer pattern, idempotency, at-least-once delivery, distributed-systems, event-driven-architecture, message-processing, lazy-processing, eager-processing]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Idempotent Consumer pattern, a crucial solution for handling duplicate messages in event-driven and distributed systems. It explains how this pattern ensures that an operation has no additional effect if called multiple times with the same input, effectively working with "at-least-once" message delivery guarantees. The author details the core algorithm, which involves checking if a message has been processed and storing its identifier. Two implementation strategies are presented: a "lazy" approach that stores the message identifier after successful processing, and an "eager" approach that stores it upfront and requires cleanup on failure. The article concludes by recommending the lazy approach for its simplicity and reduced database calls.]
---
```

# Idempotent Consumer - Handling Duplicate Messages

![Idempotent Consumer - Handling Duplicate Messages](/blog-covers/mnw_034.png?imwidth=3840)

# Idempotent Consumer - Handling Duplicate Messages

4 min read · April 22, 2023

What happens when a **message is retried** in an event-driven system?

It happens more often than you think.

The **worst case scenario** is that the **message is processed twice**, and the **side effects** can also be applied more than once.

Do you want your bank account to be double charged?

I'll assume the answer is no, of course.

You can use the **Idempotent Consumer** pattern to solve this problem.

In this week's issue I will show you:

*   How the Idempotent Consumer pattern works
*   How to implement an Idempotent Consumer
*   The tradeoffs you need to consider

Let's see why the **Idempotent Consumer** pattern is valuable.

## How The Idempotent Consumer Pattern Works

What's the idea behind the **Idempotent Consumer pattern**?

> An idempotent operation is one that has no additional effect if it is called more than once with the same input parameters.

We want to avoid handling the same message more than once.

This would require **Exactly-once** message delivery guarantees from our messaging system. And this is a really hard problem to solve in distributed systems.

A looser delivery guarantee is **At-least-once**, where we are aware that retries can happen and we can receive the same message more than once.

The **Idempotent Consumer** pattern works well with **At-least-once** message delivery, and solves the **problem of duplicate messages.**

Here's what the algorithm looks like from the moment we receive a message:

1.  Was the message already processed?
2.  If yes, it's a duplicate and there's nothing to do
3.  If not, we need to handle the message
4.  We also need to store the message identifier

![Flowchart illustrating the Idempotent Consumer algorithm: Begin -> Message comes in -> Message processed? (Yes -> End, No -> Process message -> Store message ID -> End)](/blogs/mnw_034/idempotent_consumer_algorithm.png?imwidth=828)

We need a **unique identifier** for every **message** we receive, and a table in the database to store processed messages.

However, it's interesting how we choose the implement the message handling and storing of the processed message identifier.

You can implement the idempotent consumer as a decorator around a regular message handler.

I'll show you two implementations:

*   Lazy idempotent consumer
*   Eager idempotent consumer

## Lazy Idempotent Consumer

The **lazy idempotent consumer** matches the flow shown in the algorithm above.

Lazy refers to how we store the message identifer to mark it as processed.

In the happy path, we handle the message and store the message identifier.

If the message handler throws an exception, we never store the message identifier and the consumer can be executed again.

Here's what the implementation looks like:

```csharp
public class IdempotentConsumer<T> : IHandleMessages<T>
    where T : IMessage
{
    private readonly IMessageRepository _messageRepository;
    private readonly IHandleMessages<T> _decorated;

    public IdempotentConsumer(
        IMessageRepository messageRepository,
        IHandleMessages<T> decorated)
    {
        _messageRepository = messageRepository;
        _decorated = decorated;
    }

    public async Task Handle(T message)
    {
        if (_messageRepository.IsProcessed(message.Id))
        {
            return;
        }

        await _decorated.Handle(message);

        _messageRepository.Store(message.Id);
    }
}
```

## Eager Idempotent Consumer

The **eager idempotent consumer** is slightly different from the lazy implementation, but the end result is the same.

In this version, we eagerly store the message identifier in the database and then continue to handle the message.

If the handler throws an exception, we need to perform cleanup in the database and remove the eagerly stored message identifier.

Otherwise, we risk leaving the system in an inconsistent state since the message was never handled correctly.

Here's what the implementation looks like:

```csharp
public class IdempotentConsumer<T> : IHandleMessages<T>
    where T : IMessage
{
    private readonly IMessageRepository _messageRepository;
    private readonly IHandleMessages<T> _decorated;

    public IdempotentConsumer(
        IMessageRepository messageRepository,
        IHandleMessages<T> decorated)
    {
        _messageRepository = messageRepository;
        _decorated = decorated;
    }

    public async Task Handle(T message)
    {
        try
        {
            if (_messageRepository.IsProcessed(message.Id))
            {
                return;
            }

            _messageRepository.Store(message.Id);

            await _decorated.Handle(message);
        }
        catch (Exception e)
        {
            _messageRepository.Remove(message.Id);

            throw;
        }
    }
}
```

## In Summary

Idempotency is an interesting problem to solve in a software system.

Some operations are **naturally idempotent**, and we don't need the overhead of the **Idempotent Consumer** pattern.

However, for those operations that aren't naturally idempotent, the **Idempotent Consumer** is a great solution.

The high-level algorithm is simple, and you can take two approaches in the implementation:

*   Lazy storing of message identifiers
*   Eager storing of message identifiers

I prefer to use the **lazy approach**, and only **store the message identifier** in the database when the **handler completes successfully.**

It's easier to reason about and there is one less call to the database.

Thanks for reading.

Hope that was helpful.