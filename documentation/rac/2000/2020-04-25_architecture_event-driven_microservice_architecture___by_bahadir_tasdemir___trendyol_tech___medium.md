```yaml
---
title: "Event-Driven Microservice Architecture | by Bahadir Tasdemir | Trendyol Tech | Medium"
source: https://medium.com/trendyol-tech/event-driven-microservice-architecture-91f80ceaa21e
date_published: 2020-04-25T15:38:17.819Z
date_captured: 2025-09-04T20:27:00.526Z
domain: medium.com
author: Bahadir Tasdemir
category: architecture
technologies: [REST API, PostgreSQL, RabbitMQ, RDBMS]
programming_languages: [SQL]
tags: [microservices, event-driven-architecture, message-queue, asynchronous-communication, rest-api, outbox-pattern, system-design, database, performance, loose-coupling]
key_concepts: [Event-Driven Architecture, Microservice Architecture, Loose Coupling, Asynchronous Communication, Fat Event Pattern, Outbox Pattern, Idempotency, Distributed Monolith]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article discusses the benefits and implementation of an event-driven microservice architecture, contrasting it with traditional REST-driven approaches. It highlights how asynchronous communication and loose coupling can improve system performance and scalability. The author presents real-world solutions to common microservice challenges, including the "Fat Event" pattern to reduce external calls and the "Outbox Pattern" to ensure event message reliability. The piece also addresses potential downsides like single points of failure and duplicated messages, offering strategies to mitigate them, such as RabbitMQ clustering and idempotent consumers.]
---
```

# Event-Driven Microservice Architecture | by Bahadir Tasdemir | Trendyol Tech | Medium

# Event-Driven Microservice Architecture

![Event-Driven Microservice Architecture](https://miro.medium.com/v2/resize:fit:359/1*OoQ9609aIu95XC9PvDII7A.png)

To begin with, in an **_event-driven microservice architecture_**, services communicate each-other via event messages. When business events occur, producers publish them with messages. At the same time, other services consume them through event listeners.

Thus, the main benefits of event-driven systems are asynchronous behavior and loosely coupled structures. For example, instead of requesting data when needed, apps consume them via events before the need. Therefore overall app performance increases. On the other hand, keeping coupling loose is one of the main key points of a microservice environment.

## Event-Driven Architecture as a Solution

As well as you can build your systems with event-driven structures, you can also use it as a solution to your already built highly coupled environments. Let’s discuss how we can apply the event-driven approach as a solution.

## A Basic REST Driven Approach

![REST (synchronous) API Call](https://miro.medium.com/v2/resize:fit:381/1*mUyGHyXYF3-MPoAld-2I2w.png)

Even though your application may work properly, these are the downsides:

1.  Module 1 waits for the response
2.  Module 2 can be down
3.  Network delays decrease performance
4.  If data is huge, it will paginate. This means more REST calls
5.  Module 2 can be under heavy load and can respond very late

When your system becomes less efficient because of synchronized connections, you can apply the event-driven solution.

## Real-World Scenario: How Our Team Applied It

In Trendyol/Marketplace team, we have a reporting application (GIB API). It transmits all sale reports to the government. So, this app has to fetch all the sale data from another API. In the beginning, the transaction volume was very low. Thus, we have quickly built the API with the REST approach.

![Report Generation Timing Diagram](https://miro.medium.com/v2/resize:fit:1000/1*Vqc4J7kmYNROSpHn9JA4sw.png)

In spite of the low amount of data at the beginning, it increased up suddenly. Because Trendyol is a fast-growing company, we often face this problem. On the other hand, the solution is simple: converting to event messaging.

## Solution 1: Converting to Event Messaging

As soon as we realized that the reports are not being generated efficiently, we applied the event-driven solution.

Above all, our plan was simple:

1.  Publish an event when a transaction item created
2.  Fetch the related data when event received
3.  Convert to a piece of report string
4.  Persist in the RDBMS (PostgreSQL)
5.  Query data when generating the report
6.  Concat the string data and persist as a file to disk

![Gathering DATA via Async Event Messaging](https://miro.medium.com/v2/resize:fit:531/1*bSOYF16WUrgUuC0Kg6MUkQ.png)

As a result of this, the needed transaction items are persisted in the Reporting API. As soon as report creation starts, it queries and concatenates the report data from the RDBMS.

![Generate Transaction Report](https://miro.medium.com/v2/resize:fit:401/1*rhLoIJDOGQ_qcKBoCkIpQA.png)

## Success doesn’t Come Easy

While we converted the sync process into an async architecture, the transaction API faced another performance issue. Because the reporting (GIB) API requested the detail every time a transaction item created, the transaction API went under a heavy load. The reason is, the transaction records are created for every item sold in Trendyol. So, the huge number of transaction item detail requests choked the API.

## Solution 2: Fat Event

To explain, a **_fat event_** means that the message contains the detail with the entity identifier.

```json
{
  "identifier": 1,
  "detail": "sale transaction item detail",
  "created date": "11.11.2019"
}
```
![Fat Event Message Sample](https://miro.medium.com/v2/resize:fit:304/1*nCTWqcJQctJ7wAzIauHk7Q.png)

After converting the message into a fat event, we didn’t need any additional REST calls. As a result of this, our architecture became a complete async event-driven system.

![Complete Event-Driven Architecture](https://miro.medium.com/v2/resize:fit:630/1*-0Auy2uQld_oLhNhbpVDKg.png)

## Solution 3: Outbox Pattern

While we are talking about the sale transactions, it is already clear how important these data. Because they are about financial business. Thus, the calculations must be correct 100%.

To be able to access this accuracy, we must be sure that our system is not losing any event messages. As a result of this, we applied the outbox pattern.

What is the outbox pattern? Simply, when your API publishes event messages, it doesn’t directly send them. Instead, the messages are persisted in a DB table. A job sends cumulative messages in predefined time intervals.

![Outbox Pattern](https://miro.medium.com/v2/resize:fit:700/1*4m4_-08loz6MNmNNqm6zKA.png)

To explain the figure:

1.  Business module publishes an event
2.  Event service persists the message in RDBMS
3.  Scheduler service triggers the job “Send Event Messages”
4.  Event service queries the cumulative event messages
5.  Event service publishes the messages via RabbitMQ

Let’s list down the pros and cons of the outbox pattern.

### Pros:

1.  Event messages first persisted in RDBMS. ACID properties of transactions guarantee the persistence.
2.  When an event is lost, the message can be checked from the DB.
3.  A lost event can be recovered from the RDBMS efficiently.

### Cons:

1.  Increase in complexity.
2.  Delay in publishing events.
3.  To publish a basic event, at least two technologies are needed: Storage System and Message Queueing Protocol.

## Benefits of the Event-Driven Microservice Architecture

1.  Loosely coupled structure
2.  Complete isolation of the microservices
3.  No synchronous REST calls
4.  Asynchronous event-driven functionality
5.  Performance gain

Among all of them, the most important benefit is the first one. Because we want to separate the components by microservice architecture, all of the units must be separated enough (loosely-coupled). Otherwise, microservice architecture won’t work and your system will turn into a distributed-monolith.

## Downsides

**_Single point of failure_**: If your **_RabbitMQ_** faces any issues during the production processes, your whole system will also fail.

![RabbitMQ Single Point of Failure](https://miro.medium.com/v2/resize:fit:630/1*QFVSFEVGbxY8mn8UonxDTw.png)

To overcome failures:

1.  Construct your RabbitMQ as a cluster
2.  Create your queues durable
3.  Publish your messages persisted

As a result of this, you can quickly recover any failures. In the time any error happens, your other instances in the cluster will take the work over and recreate the durable queues. Also, your persisted messages will be recovered from the disk.

**_Duplicated event messages:_** An event publisher API can face trouble and resend the same messages. To resolve any duplication in the system, any consumer endpoint has to be **_idempotent_**: always consider to check first if your API acquired the event before.

**TL;DR**

In **_microservice architecture environments_**, we have to **_keep coupling low_**. To be able to keep the coupling low, we have to **_focus on the connections between modules_**. One way to do this is to use **_event-driven approaches_**.

In the meanwhile, **_direct REST calls are expensive_**. The destination API can be out of service. Additionally, the source API has to wait until the response is received.

To create an event-driven microservice structure, we can simply create a **_RabbitMQ cluster_** with persisted messages. All needed events can be published via the service-in-responsibility. Also, all the other services can bind their consumers and process their works when event messages are sent.

While building event-driven systems, we can consider **_fat event_**s. Fat events provide all the needed data when the event occurs. As a result of this, the **_APIs don’t need any additional external calls_**.

On the other hand, **_there can be lost events_** because of a system failure or a network brake-down. To be sure that all events are published and consumed successfully, the **_outbox-pattern_** can be applied. Simply, the events are stored in a storage system instead of publishing them directly. After that, a configured job sends the events at definite time intervals. The lost messages can be recovered easily via the storage system.

## Conclusion

To sum up, the microservice architecture is quite new and we, all the developers are learning it better every day. Whenever we are not careful, our system can turn into a distributed monolith and this is the worst case. Because you can’t gain any benefits as well as you have to deal with the complexity. Above all, keeping coupling loose with event-driven architecture is one of the most important things.

If you want to learn more about the RabbitMQ please follow [this link](https://www.rabbitmq.com/documentation.html).

Finally, if you like the post, please like it and share it. Also, please don’t forget to read my other [post about the Trendyol Scheduler Service](/trendyol-tech/trendyol-scheduler-service-58751d3a5080).

## Personal Links

*   Twitter: [https://twitter.com/RequestParam](https://twitter.com/RequestParam)
*   Website: [https://www.btasdemir.com](https://www.btasdemir.com/)