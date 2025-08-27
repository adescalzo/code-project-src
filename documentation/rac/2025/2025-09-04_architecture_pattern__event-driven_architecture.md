```yaml
---
title: "Pattern: Event-driven architecture"
source: https://microservices.io/patterns/data/event-driven-architecture.html
date_published: unknown
date_captured: 2025-09-04T20:25:57.357Z
domain: microservices.io
author: Unknown
category: architecture
technologies: [Kong, Disqus, Twitter, Eventuate.io]
programming_languages: []
tags: [microservices, event-driven, architecture-pattern, data-consistency, distributed-systems, saga, transactional-outbox, event-sourcing, database-per-service, eventually-consistent]
key_concepts: [Event-driven architecture, Microservice architecture, Database per Service, Data consistency, Eventually consistent, Distributed transactions, Event Sourcing, Transactional Outbox, Saga pattern, CQRS, API Composition]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article describes the Event-driven architecture pattern, a solution for maintaining data consistency across multiple services in a microservice environment where each service has its own database. It explains how services publish events upon data updates, which other services subscribe to and react to, achieving eventual consistency without distributed ACID transactions. The pattern is illustrated with an e-commerce example involving order and customer services. It highlights benefits like avoiding distributed transactions but notes increased programming complexity and the need for reliable event publishing mechanisms like Event Sourcing or Transactional Outbox.]
---
```

# Pattern: Event-driven architecture

# Pattern: Event-driven architecture [§](#undefined)

[pattern](/tags/pattern)    

---

## Context [§](#context)

You have applied the [Database per Service](database-per-service.html) pattern. Each service has its own database. Some business transactions, however, span multiple service so you need a mechanism to ensure data consistency across services. For example, lets imagine that you are building an e-commerce store where customers have a credit limit. The application must ensure that a new order will not exceed the customer’s credit limit. Since Orders and Customers are in different databases the application cannot simply use a local ACID transaction.

## Problem [§](#problem)

How to maintain data consistency across services?

## Forces [§](#forces)

*   2PC is not an option

## Solution [§](#solution)

Use an event-driven, eventually consistent approach. Each service publishes an event whenever it update its data. Other service subscribe to events. When an event is received, a service updates its data.

## Example [§](#example)

An e-commerce application that uses this approach would work as follows:

1.  The `Order Service` creates an Order in a _pending_ state and publishes an `OrderCreated` event.
2.  The `Customer Service` receives the event and attempts to reserve credit for that Order. It then publishes either a `Credit Reserved` event or a `CreditLimitExceeded` event.
3.  The `Order Service` receives the event from the `Customer Service` and changes the state of the order to either _approved_ or _cancelled_

## Resulting context [§](#resulting-context)

This pattern has the following benefits:

*   It enables an application to maintain data consistency across multiple services without using distributed transactions

This solution has the following drawbacks:

*   The programming model is more complex

There are also the following issues to address:

*   In order to be reliable, an application must atomically update its database and publish an event. It cannot use the traditional mechanism of a distributed transaction that spans the database and the message broker. Instead, it must use one the patterns listed below.

## Related patterns [§](#related-patterns)

*   The [Database per Service pattern](database-per-service.html) creates the need for this pattern
*   The following patterns are ways to _atomically_ update state _and_ publish events:
    *   [Event sourcing](event-sourcing.html)
    *   [Transactional Outbox](transactional-outbox.html)
    *   [Database triggers](database-triggers.html)
    *   [Transaction log tailing](transaction-log-tailing.html)

## See also [§](#see-also)

The article [Event-Driven Data Management for Microservices](https://www.nginx.com/blog/event-driven-data-management-microservices/) by @crichardson describes this pattern

---

[pattern](/tags/pattern)    

---

![Cover of "Microservices Patterns, Second Edition" by Chris Richardson, with a "MEAP" (Manning Early Access Program) overlay. Features an illustration of a person in historical attire.](/i/posts/mp2e-book-cover.png)

![A man in a suit looking thoughtfully at a large, glowing screen displaying a complex "SYSTEM ARCHITECTURE" diagram with interconnected components and data.](/i/posts/cxo-wondering.webp)

![A simple logo with "Microservices.IO Premium" text on a white background with a jagged black border on the right and bottom.](/i/posts/premium-logo.png)

![A group of people around tables in a workshop setting, with one person standing and explaining something on a whiteboard, and others looking at laptops.](/i/workshop-kata_small.jpg)

![A desktop computer screen displaying a "Microservices Architecture" presentation in a video conferencing application (like Zoom), with a grid of participants visible on a smartphone screen next to it. A person's hands are visible at the keyboard.](/i/posts/zoom-consulting.webp)

![Cover of the book "Microservices Patterns" by Chris Richardson, showing a stylized illustration of a person in historical attire.](/i/Microservices-Patterns-Cover-published.png)

![A man (Chris Richardson) speaking at a conference, standing on a stage with a microphone, in front of a large screen displaying a presentation.](/i/Chris_Speaking_Mucon_2018_a.jpg)

![A diagram illustrating a "Microservice Chassis" with various components like logging, metrics, configuration, health checks, and service discovery, all surrounding the "Business Logic."](/i/patterns/microservice-template-and-chassis/Microservice_chassis.png)

![Eventuate.io logo.](https://eventuate.io/i/logo.gif)