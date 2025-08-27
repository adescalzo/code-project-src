```yaml
---
title: "Pattern: Saga"
source: https://microservices.io/patterns/data/saga.html
date_published: unknown
date_captured: 2025-09-04T20:15:54.249Z
domain: microservices.io
author: Unknown
category: architecture
technologies: [Kong, Eventuate Tram Sagas framework, Eventuate Tram framework, Eventuate event sourcing framework, HTTP, Websocket, Web hook, Disqus, Twitter]
programming_languages: [Java]
tags: [microservices, saga-pattern, distributed-transactions, transaction-management, data-consistency, service-collaboration, event-driven, choreography, orchestration, database-per-service]
key_concepts: [saga-pattern, microservice-architecture, distributed-transactions, local-transactions, compensating-transactions, database-per-service, ACID-transactions, domain-events, message-broker, API-composition, CQRS, event-sourcing, transactional-outbox, aggregates, command-side-replica]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces the Saga pattern, a crucial solution for managing distributed transactions in a microservice architecture where each service maintains its own database. It explains how a saga coordinates a sequence of local transactions across multiple services, using compensating transactions to undo changes if a step fails. The article details two primary coordination mechanisms: choreography, where services communicate via domain events, and orchestration, where a central orchestrator directs the flow. It highlights the benefits of maintaining data consistency without distributed transactions, while also discussing drawbacks such as the lack of automatic rollback and isolation, necessitating careful design and countermeasures.
---
```

# Pattern: Saga

#### [Microservice Architecture](/index.html)

**Supported by [Kong](https://konghq.com/)**

*   [Patterns](/patterns/index.html)
*   [Articles](/articles/index.html)
*   [Presentations](/presentations/index.html)
*   [Adoptnew](/adopt/index.html)
*   [Refactoringnew](/refactoring/index.html)
*   [Testingnew](/testing/index.html)

*   [About](/about.html)

# Pattern: Saga

[pattern](/tags/pattern)   [transaction management](/tags/transaction management)   [sagas](/tags/sagas)   [service collaboration](/tags/service collaboration)   [implementing commands](/tags/implementing commands)    

---

## Want to learn more about this pattern?

Take a look at my [self-paced, online bootcamp](https://chrisrichardson.net/virtual-bootcamp-distributed-data-management.html) that teaches you how to use the Saga, API Composition, and CQRS patterns to design operations that span multiple services.

The regular price is $395/person but use coupon DWTWBMJI to sign up for $95 (valid until September 10th, 2025)

![Microservices.io Logo White](/i/Microservices_IO_Logo_White.png)

## Context

You have applied the [Database per Service](database-per-service.html) pattern. Each service has its own database. Some business transactions, however, span multiple service so you need a mechanism to implement transactions that span services. For example, let’s imagine that you are building an e-commerce store where customers have a credit limit. The application must ensure that a new order will not exceed the customer’s credit limit. Since Orders and Customers are in different databases owned by different services the application cannot simply use a local ACID transaction.

## Problem

How to implement transactions that span services?

## Forces

*   2PC is not an option

## Solution

Implement each business transaction that spans multiple services as a saga. A saga is a sequence of local transactions. Each local transaction updates the database and publishes a message or event to trigger the next local transaction in the saga. If a local transaction fails because it violates a business rule then the saga executes a series of compensating transactions that undo the changes that were made by the preceding local transactions.

![Diagram showing the transition from a Distributed Transaction (2PC) involving Order Service and Customer Service to a Saga pattern. The Saga involves a sequence of Local Transactions in Order Service, Customer Service, and Order Service again, communicating via messages/events.](/i/sagas/From_2PC_To_Saga.png)

There are two ways of coordination sagas:

*   Choreography - each local transaction publishes domain events that trigger local transactions in other services
*   Orchestration - an orchestrator (object) tells the participants what local transactions to execute

## Example: Choreography-based saga

![Diagram illustrating a choreography-based Saga for creating an order. A POST /orders request initiates the Order Service, which creates an Order in PENDING state and emits an "Order Created" event. This event is consumed by the Customer Service, which attempts to reserve credit and emits either "Credit Reserved" or "Credit Limit Exceeded" events. The Order Service then consumes these events to approve or reject the order.](/i/sagas/Create_Order_Saga.png)

An e-commerce application that uses this approach would create an order using a choreography-based saga that consists of the following steps:

1.  The `Order Service` receives the `POST /orders` request and creates an `Order` in a `PENDING` state
2.  It then emits an `Order Created` event
3.  The `Customer Service`’s event handler attempts to reserve credit
4.  It then emits an event indicating the outcome
5.  The `OrderService`’s event handler either approves or rejects the `Order`

[Take a tour of an example saga](/post/architecture/2024/03/20/tour-of-two-sagas.html)

## Example: Orchestration-based saga

![Diagram illustrating an orchestration-based Saga for creating an order. A POST /orders request initiates the Order Service. An Order Controller creates a "Create Order Saga" orchestrator, which creates an Order in PENDING state. The orchestrator sends a "Reserve Credit" command to a Message Broker, which forwards it to the Customer Service. The Customer Service attempts to reserve credit and sends a reply message ("Reserve Credit Response") back via the Message Broker to the orchestrator. The orchestrator then approves or rejects the Order.](/i/sagas/Create_Order_Saga_Orchestration.png)

An e-commerce application that uses this approach would create an order using an orchestration-based saga that consists of the following steps:

1.  The `Order Service` receives the `POST /orders` request and creates the `Create Order` saga orchestrator
2.  The saga orchestrator creates an `Order` in the `PENDING` state
3.  It then sends a `Reserve Credit` command to the `Customer Service`
4.  The `Customer Service` attempts to reserve credit
5.  It then sends back a reply message indicating the outcome
6.  The saga orchestrator either approves or rejects the `Order`

[Take a tour of an example saga](/post/architecture/2024/03/20/tour-of-two-sagas.html)

## Resulting context

This pattern has the following benefits:

*   It enables an application to maintain data consistency across multiple services without using distributed transactions

This solution has the following drawbacks:

*   Lack of automatic rollback - a developer must design compensating transactions that explicitly undo changes made earlier in a saga rather than relying on the automatic rollback feature of ACID transactions
    
*   Lack of isolation (the “I” in ACID) - the lack of isolation means that there’s risk that the concurrent execution of multiple sagas and transactions can use data anomalies. consequently, a saga developer must typical use countermeasures, which are design techniques that implement isolation. Moreover, careful analysis is needed to select and correctly implement the countermeasures. See [Chapter 4/section 4.3 of my book Microservices Patterns](https://livebook.manning.com/book/microservices-patterns/chapter-4/143) for more information.
    

There are also the following issues to address:

*   In order to be reliable, a service must atomically update its database _and_ publish a message/event. It cannot use the traditional mechanism of a distributed transaction that spans the database and the message broker. Instead, it must use one of the patterns listed below.
    
*   A client that initiates the saga, which an asynchronous flow, using a synchronous request (e.g. HTTP `POST /orders`) needs to be able to determine its outcome. There are several options, each with different trade-offs:
    
    *   The service sends back a response once the saga completes, e.g. once it receives an `OrderApproved` or `OrderRejected` event.
    *   The service sends back a response (e.g. containing the `orderID`) after initiating the saga and the client periodically polls (e.g. `GET /orders/{orderID}`) to determine the outcome
    *   The service sends back a response (e.g. containing the `orderID`) after initiating the saga, and then sends an event (e.g. websocket, web hook, etc) to the client once the saga completes.

## Related patterns

*   The [Database per Service pattern](database-per-service.html) creates the need for this pattern
*   The following patterns are ways to _atomically_ update state _and_ publish messages/events:
    *   [Event sourcing](event-sourcing.html)
    *   [Transactional Outbox](transactional-outbox.html)
*   A choreography-based saga can publish events using [Aggregates](aggregate.html) and [Domain Events](domain-event.html)
*   The [Command-side replica](command-side-replica.html) is an alternative pattern, which can replace saga step that query data

## Learn more

*   My book [Microservices patterns](/book) describes this pattern in a lot more detail. The book’s [example application](https://github.com/microservice-patterns/ftgo-application) implements orchestration-based sagas using the [Eventuate Tram Sagas framework](https://github.com/eventuate-tram/eventuate-tram-sagas)
*   Take a look at my [self-paced, online bootcamp](https://chrisrichardson.net/virtual-bootcamp-distributed-data-management.html) that teaches you how to use the Saga, API Composition, and CQRS patterns to design operations that span multiple services.
*   Read these [articles](/tags/sagas.html) about the Saga pattern
*   My [presentations](/presentations) on sagas and asynchronous microservices.

## Example code

The following examples implement the customers and orders example in different ways:

*   [Choreography-based saga](https://github.com/eventuate-tram/eventuate-tram-examples-customers-and-orders) where the services publish domain events using the [Eventuate Tram framework](https://github.com/eventuate-tram/eventuate-tram-core)
*   [Orchestration-based saga](https://github.com/eventuate-tram/eventuate-tram-sagas-examples-customers-and-orders) where the `Order Service` uses a saga orchestrator implemented using the [Eventuate Tram Sagas framework](https://github.com/eventuate-tram/eventuate-tram-sagas)
*   [Choreography and event sourcing-based saga](https://github.com/eventuate-examples/eventuate-examples-java-customers-and-orders) where the services publish domain events using the [Eventuate event sourcing framework](http://eventuate.io/)

---

[pattern](/tags/pattern)   [transaction management](/tags/transaction management)   [sagas](/tags/sagas)   [service collaboration](/tags/service collaboration)   [implementing commands](/tags/implementing commands)    

---

---

Copyright © 2025 Chris Richardson • All rights reserved • Supported by [Kong](https://konghq.com/).

#### About Microservices.io

![Gravatar image of Chris Richardson](/images/gravatar.com/avatar/a290a8643359e2495e1c6312e662012f)

Microservices.io is brought to you by [Chris Richardson](/about.html). Experienced software architect, author of POJOs in Action, the creator of the original [CloudFoundry.com](http://CloudFoundry.com), and the author of Microservices patterns.

#### Microservices Patterns, 2nd edition

![Cover of the "Microservices Patterns, Second Edition" book with a "MEAP" (Manning Early Access Program) overlay.](/i/posts/mp2e-book-cover.png)

I am very excited to announce that the MEAP for the second edition of my book, Microservices Patterns is now available!

[Learn more](/post/architecture/2025/06/26/announcing-meap-microservices-patterns-2nd-edition.html)

#### ASK CHRIS

?

Got a question about microservices?

Fill in [this form](https://forms.gle/ppYDAF1JxHGec8Kn9). If I can, I'll write a blog post that answers your question.

#### NEED HELP?

![Image of a person (likely a CXO) looking thoughtful, representing "NEED HELP?".](/i/posts/cxo-wondering.webp)

I help organizations improve agility and competitiveness through better software architecture.

Learn more about my [consulting engagements](https://chrisrichardson.net/consulting.html), and [training workshops](https://chrisrichardson.net/training.html).

#### PREMIUM CONTENT

![Premium content logo](/i/posts/premium-logo.png) Premium content now available for paid subscribers at [premium.microservices.io](https://premium.microservices.io).

#### MICROSERVICES WORKSHOPS

![Image related to workshops, possibly showing a group or a speaker.](/i/workshop-kata_small.jpg)

Chris teaches [comprehensive workshops](http://chrisrichardson.net/training.html) for architects and developers that will enable your organization use microservices effectively.

Avoid the pitfalls of adopting microservices and learn essential topics, such as service decomposition and design and how to refactor a monolith to microservices.

[Learn more](http://chrisrichardson.net/training.html)

#### Remote consulting session

![Image related to remote consulting, possibly a video call screenshot.](/i/posts/zoom-consulting.webp)

Got a specific microservice architecture-related question? For example:

*   Wondering whether your organization should adopt microservices?
*   Want to know how to migrate your monolith to microservices?
*   Facing a tricky microservice architecture design problem?

Consider signing up for a [two hour, highly focussed, consulting session.](https://chrisrichardson.net/consulting-office-hours.html)

#### ASSESS your architecture

Assess your application's microservice architecture and identify what needs to be improved. [Engage Chris](http://www.chrisrichardson.net/consulting.html) to conduct an architect review.

#### LEARN about microservices

Chris offers numerous other resources for learning the microservice architecture.

#### Get the book: Microservices Patterns

Read Chris Richardson's book: ![Cover of the "Microservices Patterns" book.](/i/Microservices-Patterns-Cover-published.png)

---

#### Example microservices applications

Want to see an example? Check out Chris Richardson's example applications. [See code](http://eventuate.io/exampleapps.html)

#### Virtual bootcamp: Distributed data patterns in a microservice architecture

![Image of Chris Richardson speaking at a conference.](/i/Chris_Speaking_Mucon_2018_a.jpg)

My virtual bootcamp, distributed data patterns in a microservice architecture, is now open for enrollment!

It covers the key distributed data management patterns including Saga, API Composition, and CQRS.

It consists of video lectures, code labs, and a weekly ask-me-anything video conference repeated in multiple timezones.

The regular price is $395/person but use coupon DWTWBMJI to sign up for $95 (valid until September 10th, 2025). There are deeper discounts for buying multiple seats.

[Learn more](https://chrisrichardson.net/virtual-bootcamp-distributed-data-management.html)

#### Learn how to create a service template and microservice chassis

Take a look at my [Manning LiveProject](/post/patterns/2022/03/15/service-template-chassis-live-project.html) that teaches you how to develop a service template and microservice chassis.

![Diagram illustrating a microservice chassis concept, showing multiple services with common infrastructure components.](/i/patterns/microservice-template-and-chassis/Microservice_chassis.png)

[Signup for the newsletter](http://visitor.r20.constantcontact.com/d.jsp?llr=ula8akwab&p=oi&m=1123470377332&sit=l6ktajjkb&f=15d9bba9-b33d-491f-b874-73a41bba8a76)

For Email Marketing you can trust.

#### BUILD microservices

Ready to start using the microservice architecture?

#### Consulting services

[Engage Chris](http://www.chrisrichardson.net/consulting.html) to create a microservices adoption roadmap and help you define your microservice architecture,

---

#### The Eventuate platform

Use the [Eventuate.io platform](https://eventuate.io) to tackle distributed data management challenges in your microservices architecture.

![Eventuate.io platform logo](/i/logo.gif)

Eventuate is Chris's latest startup. It makes it easy to use the Saga pattern to manage transactions and the CQRS pattern to implement queries.

---

Join the [microservices google group](https://groups.google.com/forum/#!forum/microservices)

Please enable JavaScript to view the [comments powered by Disqus.](https://disqus.com/?ref_noscript)

BDOW!