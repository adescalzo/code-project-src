```yaml
---
title: "Pattern: Command Query Responsibility Segregation (CQRS)"
source: https://microservices.io/patterns/data/cqrs.html
date_published: unknown
date_captured: 2025-09-04T20:21:12.380Z
domain: microservices.io
author: Unknown
category: architecture
technologies: [Kong, NoSQL, Document database, Key-value store, Eventuate, GitHub, Disqus, Twitter, Constant Contact, Manning LiveProject]
programming_languages: []
tags: [microservices, cqrs, data-management, event-sourcing, nosql, distributed-systems, architecture-patterns, query-optimization, service-collaboration, view-database]
key_concepts: [Command Query Responsibility Segregation (CQRS), Microservices architecture, Database per service, Event sourcing, Domain events, View database, API Composition, Saga pattern]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Command Query Responsibility Segregation (CQRS) pattern within a microservice architecture. It addresses the challenge of implementing queries that join data from multiple services, especially when using the Database per Service and Event Sourcing patterns. The solution involves defining a read-only "view database" optimized for specific queries, which is kept updated by subscribing to domain events. While offering benefits like scalability and improved separation of concerns, CQRS introduces increased complexity, potential code duplication, and replication lag, leading to eventually consistent views. The article also highlights related patterns such as API Composition and Saga, and provides resources for further learning.]
---
```

# Pattern: Command Query Responsibility Segregation (CQRS)

#### [Microservice Architecture](/index.html)

**Supported by [Kong](https://konghq.com/)**

*   [Patterns](/patterns/index.html)
*   [Articles](/articles/index.html)
*   [Presentations](/presentations/index.html)
*   [Adoptnew](/adopt/index.html)
*   [Refactoringnew](/refactoring/index.html)
*   [Testingnew](/testing/index.html)

*   [About](/about.html)

# Pattern: Command Query Responsibility Segregation (CQRS)

[pattern](/tags/pattern)   [service collaboration](/tags/service collaboration)   [implementing queries](/tags/implementing queries)    

---

## Want to learn more about this pattern?

Take a look at my [self-paced, online bootcamp](https://chrisrichardson.net/virtual-bootcamp-distributed-data-management.html) that teaches you how to use the Saga, API Composition, and CQRS patterns to design operations that span multiple services.

The regular price is $395/person but use coupon DWTWBMJI to sign up for $95 (valid until September 10th, 2025)

![Microservices.IO Logo White](/i/Microservices_IO_Logo_White.png)
*Description: The white logo for Microservices.io, a resource for microservice architecture patterns.*

## Context

You have applied the [Microservices architecture pattern](../microservices.html) and the [Database per service pattern](database-per-service.html). As a result, it is no longer straightforward to implement queries that join data from multiple services. Also, if you have applied the [Event sourcing pattern](event-sourcing.html) then the data is no longer easily queried.

## Problem

How to implement a query that retrieves data from multiple services in a microservice architecture?

## Solution

Define a view database, which is a read-only ‘replica’ that is designed specifically to support that query, or a group related queries. The application keeps the database up to date by subscribing to [Domain events](domain-event.html) published by the service that own the data. The type of database and its schema are optimized for the query or queries. It’s often a NoSQL database, such as a document database or a key-value store.

![QuerySideService Diagram](/i/patterns/data/QuerySideService.png)
*Description: A diagram illustrating the CQRS pattern. Multiple services (represented by orange hexagons with databases) publish events (represented by inverted triangles) to message queues (cylinders). A query-side service (another orange hexagon with a database) consumes these events to update its dedicated view database. Queries then access this view database.*

## Examples

*   My book’s FTGO example application has the [`Order History Service`](https://github.com/microservices-patterns/ftgo-application#chapter-7-implementing-queries-in-a-microservice-architecture), which implements this pattern.
    
*   There are [several Eventuate-based example applications](http://eventuate.io/exampleapps.html) that illustrate how to use this pattern.
    
![Order History Service Diagram](image-2.png)
*Description: A detailed architectural diagram showing how an "Order History Service" implements CQRS. It consumes "Order events", "Ticket events", "Delivery events", and "Accounting events" from various source services (Order, Kitchen, Delivery, Accounting Service) via "Event Handlers". These events are used to populate an "Order History View database". Client queries, such as `findOrderHistory()` and `findOrder()`, are directed at the Order History Service, which then queries its optimized view database.*

## Resulting context

This pattern has the following benefits:

*   Supports multiple denormalized views that are scalable and performant
*   Improved separation of concerns = simpler command and query models
*   Necessary in an event sourced architecture

This pattern has the following drawbacks:

*   Increased complexity
*   Potential code duplication
*   Replication lag/eventually consistent views

## Related patterns

*   The [Database per Service pattern](database-per-service.html) creates the need for this pattern
*   The [API Composition pattern](api-composition.html) is an alternative solution
*   The [Domain event](domain-event.html) pattern generates the events
*   CQRS is often used with [Event sourcing](event-sourcing.html)

## See also

*   [Eventuate](http://eventuate.io), which is a platform for developing transactional business applications.

## Learn more

*   My book [Microservices patterns](/book) describes this pattern in a lot more detail
*   Take a look at my [self-paced, online bootcamp](https://chrisrichardson.net/virtual-bootcamp-distributed-data-management.html) that teaches you how to use the Saga, API Composition, and CQRS patterns to design operations that span multiple services.

---

[pattern](/tags/pattern)   [service collaboration](/tags/service collaboration)   [implementing queries](/tags/implementing queries)    

---

---

Copyright © 2025 Chris Richardson • All rights reserved • Supported by [Kong](https://konghq.com/).

#### About Microservices.io

![Chris Richardson Gravatar](https://gravatar.com/avatar/a290a8643359e2495e1c6312e662012f)
*Description: A Gravatar image of Chris Richardson, the author and creator of Microservices.io.*

Microservices.io is brought to you by [Chris Richardson](/about.html). Experienced software architect, author of POJOs in Action, the creator of the original [CloudFoundry.com](http://CloudFoundry.com), and the author of Microservices patterns.

#### Microservices Patterns, 2nd edition

![Microservices Patterns 2nd Edition MEAP Cover](/i/posts/mp2e-book-cover.png)
*Description: The cover of the book "Microservices Patterns, Second Edition" by Chris Richardson, with a prominent "MEAP" (Manning Early Access Program) overlay.*

I am very excited to announce that the MEAP for the second edition of my book, Microservices Patterns is now available!

[Learn more](/post/architecture/2025/06/26/announcing-meap-microservices-patterns-2nd-edition.html)

#### ASK CHRIS

?

Got a question about microservices?

Fill in [this form](https://forms.gle/ppYDAF1JxHGec8Kn9). If I can, I'll write a blog post that answers your question.

#### NEED HELP?

![CXO Wondering](/i/posts/cxo-wondering.webp)
*Description: An image of a person in a suit looking thoughtfully, representing a CXO or decision-maker contemplating a problem.*

I help organizations improve agility and competitiveness through better software architecture.

Learn more about my [consulting engagements](https://chrisrichardson.net/consulting.html), and [training workshops](https://chrisrichardson.net/training.html).

#### PREMIUM CONTENT

![Microservices.IO Premium Logo](/i/posts/premium-logo.png)
*Description: A black and white logo for "Microservices.IO Premium".*

Premium content now available for paid subscribers at [premium.microservices.io](https://premium.microservices.io).

#### MICROSERVICES WORKSHOPS

![Workshop Kata Small](/i/workshop-kata_small.jpg)
*Description: A small image related to technical workshops, possibly depicting a coding kata or a learning exercise.*

Chris teaches [comprehensive workshops](http://chrisrichardson.net/training.html) for architects and developers that will enable your organization use microservices effectively.

Avoid the pitfalls of adopting microservices and learn essential topics, such as service decomposition and design and how to refactor a monolith to microservices.

[Learn more](http://chrisrichardson.net/training.html)

#### Remote consulting session

![Zoom Consulting](/i/posts/zoom-consulting.webp)
*Description: An image depicting a remote consulting session, likely via video conference like Zoom, with a person looking at a screen.*

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

Read Chris Richardson's book: ![Microservices Patterns Book Cover](/i/Microservices-Patterns-Cover-published.png)
*Description: The cover of the book "Microservices Patterns" by Chris Richardson, featuring a historical figure in colorful attire.*

---

#### Example microservices applications

Want to see an example? Check out Chris Richardson's example applications. [See code](http://eventuate.io/exampleapps.html)

#### Virtual bootcamp: Distributed data patterns in a microservice architecture

![Chris Speaking Mucon 2018](/i/Chris_Speaking_Mucon_2018_a.jpg)
*Description: A photograph of Chris Richardson speaking at a conference, Mucon 2018.*

My virtual bootcamp, distributed data patterns in a microservice architecture, is now open for enrollment!

It covers the key distributed data management patterns including Saga, API Composition, and CQRS.

It consists of video lectures, code labs, and a weekly ask-me-anything video conference repeated in multiple timezones.

The regular price is $395/person but use coupon DWTWBMJI to sign up for $95 (valid until September 10th, 2025). There are deeper discounts for buying multiple seats.

[Learn more](https://chrisrichardson.net/virtual-bootcamp-distributed-data-management.html)

#### Learn how to create a service template and microservice chassis

Take a look at my [Manning LiveProject](/post/patterns/2022/03/15/service-template-chassis-live-project.html) that teaches you how to develop a service template and microservice chassis.

![Microservice Chassis Diagram](/i/patterns/microservice-template-and-chassis/Microservice_chassis.png)
*Description: A diagram illustrating the concept of a Microservice Chassis, showing various common components and cross-cutting concerns that can be standardized across microservices.*

[Signup for the newsletter](http://visitor.r20.constantcontact.com/d.jsp?llr=ula8akwab&p=oi&m=1123470377332&sit=l6ktajjkb&f=15d9bba9-b33d-491f-b874-73a41bba8a76)

For Email Marketing you can trust.

#### BUILD microservices

Ready to start using the microservice architecture?

#### Consulting services

[Engage Chris](http://www.chrisrichardson.net/consulting.html) to create a microservices adoption roadmap and help you define your microservice architecture,

---

#### The Eventuate platform

Use the [Eventuate.io platform](https://eventuate.io) to tackle distributed data management challenges in your microservices architecture.

![Eventuate.io Logo](https://eventuate.io/i/logo.gif)
*Description: The logo for Eventuate.io, a platform designed to simplify distributed data management in microservices architectures.*

Eventuate is Chris's latest startup. It makes it easy to use the Saga pattern to manage transactions and the CQRS pattern to implement queries.

---

Join the [microservices google group](https://groups.google.com/forum/#!forum/microservices)

Please enable JavaScript to view the [comments powered by Disqus.](https://disqus.com/?ref_noscript)

BDOW!

![System Architecture Display](image-4.png)
*Description: A man in a blue suit stands thoughtfully in a modern office, looking at a large, transparent digital display showing a complex system architecture diagram with interconnected components, data flows, and geographical data.*

![Microservices.IO Premium Banner](image-5.png)
*Description: A white banner with a jagged black border, displaying "Microservices.IO Premium" in bold black text.*