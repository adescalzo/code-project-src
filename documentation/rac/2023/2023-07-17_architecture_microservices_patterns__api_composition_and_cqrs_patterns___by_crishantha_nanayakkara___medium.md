```yaml
---
title: "Microservices Patterns: API Composition and CQRS Patterns | by Crishantha Nanayakkara | Medium"
source: https://crishantha.medium.com/microservices-patterns-api-composition-pattern-27040cae5bd3
date_published: 2023-07-17T16:03:25.604Z
date_captured: 2025-09-04T20:25:23.380Z
domain: crishantha.medium.com
author: Crishantha Nanayakkara
category: architecture
technologies: [Microservices, API Gateway, Event Bus]
programming_languages: []
tags: [microservices, api-composition, cqrs, data-access, architectural-patterns, distributed-systems, event-driven, eventual-consistency, query-optimization]
key_concepts: [API Composition Pattern, CQRS Pattern, Microservices Architecture, Database per Service Pattern, Eventual Consistency, Command Query Separation, Domain Events, Event Bus]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article compares two crucial data query patterns in microservices architecture: API Composition and CQRS. It begins by highlighting the challenge of accessing data across multiple services, a common scenario with the "Database per service" pattern. The API Composition pattern is introduced as a simpler approach where an API Composer queries multiple services and combines results. However, the article details its drawbacks, including performance overhead, increased coupling, and data inconsistency issues. Conversely, the CQRS pattern is presented as a more powerful solution that segregates command (CUD) and query (Read) operations, using an Event Bus to maintain a read-only view for queries. This separation enhances performance, scalability, and addresses some of the consistency challenges faced by API Composition.]
---
```

# Microservices Patterns: API Composition and CQRS Patterns | by Crishantha Nanayakkara | Medium

# Microservices Patterns: API Composition and CQRS Patterns

![Crishantha Nanayakkara](https://miro.medium.com/v2/resize:fill:64:64/1*-wTKneSqkgll9CvONKLAow.jpeg)

Crishantha Nanayakkara

Follow

4 min read

·

Jul 8, 2023

173

2

Listen

Share

More

_Microservices Patterns Series — Part 07_

In the typical microservices design, the [_Database per service pattern_](https://microservices.io/patterns/data/database-per-service.html) indicates that each microservice should be responsible for its own data. However, if the consumer wants to access data, which spans across multiple microservices, we should be able to find a solution for that. There are two data query patterns, which we can introduce here.

1.  The API Composition Pattern \[1\]
2.  The Command Query Responsibility Pattern (CQRS) \[2\]

Out of these two patterns, the _API Composition Pattern_ is more simpler to use and adopt. Comparatively, The _CQRS Pattern_ is more powerful and complex. In this blog, lets compare these two patterns in detail.

### The API Composition Pattern

> The API Composition Pattern: Implement a query that retrieves data from several services by querying each service via its API and combining the results. \[1\]

Structurally the API Composition Pattern deals with **two key participants**. (1. The API Composer 2. The Provider Services / Microservices).

The API Composer implements the query operation by querying the provider services.

The API Composer invokes all involved provider services and combines the results together with an in-memory join before presenting to the consumer.

![Figure 01: Diagram illustrating the API Composition Pattern. A Client App/API Gateway sends a request to an API Composer. The API Composer then makes parallel requests to multiple Query APIs (e.g., Order Microservice, Payment Microservice, Delivery Microservice), each with its own dedicated database. The API Composer combines the results before responding to the client.](https://miro.medium.com/v2/resize:fit:700/1*0DzTc9L1CWBJmWV07VWRsA.png)

Figure 01 — API Composition Pattern

In the API Composition pattern, these provider service invocations could happen in parallel or in sequence. For example, in figure 01, all three provider services (_order, payment, deliver_) should be called in sequence or in parallel (in a quick time) in order to get an output.

### API Composition Pattern — Drawbacks

1.  **The overhead of invoking multiple service providers** using multiple database queries can be time consuming. In such a situation, more computing and networking resources are required and that would add some additional cost as well to the application in general.
2.  **Having multiple service endpoints can be a hindrance to the overall availability** of the application and **increases the coupling with related service endpoints**. There can be situations, where at least one failed service endpoint, which would result a less reliable system.
3.  **The data inconsistencies** could be there in the API composer result due to the fact that, API composer does retrieve data from multiple external service providers. Each service provider can have data with inconsistencies attached to them. For example as in figure 01 example, if a consumer decides to cancel an already placed order, which the consumer had completed a few minutes ago, the _Order_ microservices is now updated as a CANCELLED order. However, with eventual consistency around there could be a possibility that the _Purchasing_ microservice is not yet updated with a payment cancellation, which could lead to an inconsistency of the final data query output via the API composer. In addition to that, sometimes certain service provider queries need data from other service providers to respond. In such situations, having data replicas in many service provider level databases are required. Maintaining multiple data replicas of other service providers could lead to data inconsistencies as well.
4.  The inner joins, which are being used by the API composer can be quite confusing at times and difficult to apply.

### CQRS (Command and Query Responsibility Segregation) Pattern

> The CQRS Pattern: Implement a query that needs data from several services by using events to maintain a read only view that replicates data from the services

In order to overcome some of the drawbacks, which have been identified under the API Composition Pattern, the CQRS Pattern can be applied to many microservices based applications.

### So, what can CQRS can do for us?

In simple terms, it is all about segregation or the separation of commands and queries. As you see in figure 02, CUD (Create, Update and Delete) operations are routed through the **domain / command model** and the R(Read) operations are routed through the **query model**.

Furthermore, the command model publishes domain events whenever its data changes to the query model as an eventual consistency transaction via an Event Bus.

With this approach, all the data queries pertaining to the given service is accommodated via the Query DB and will “not” be depending on the Command level DB anymore. This will certainly improve the performance and the scalability of the application.

![Figure 02: Diagram illustrating the CQRS Pattern. A Consumer Request is handled by a Service Facade. Create, Update, Delete (CUD) operations are routed to a Command Model (Domain Aggregates) which interacts with a Command DB. Read (R) operations are routed to a Query Model (Event Handler) which interacts with a Query DB. The Command Model publishes Events to the Event Handler in the Query Model to maintain eventual consistency between the two data stores.](https://miro.medium.com/v2/resize:fit:700/1*zk90MBvjf781jAQB5Mqj7A.png)

Figure 02 — CQRS Pattern

### Conclusion

Hope the above simple comparison will give you the importance of the CQRS method and its application primarily on a microservices architecture compared to the API composition pattern, which we mostly tend to implement.

Thank You!

### References

1.  API Composition Pattern: [https://microservices.io/patterns/data/api-composition.html](https://microservices.io/patterns/data/api-composition.html)
2.  CQRS Pattern: [https://microservices.io/patterns/data/cqrs.html](https://microservices.io/patterns/data/cqrs.html)
3.  Database per Service Pattern: [https://microservices.io/patterns/data/database-per-service.html](https://microservices.io/patterns/data/database-per-service.html)