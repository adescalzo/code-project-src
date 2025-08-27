```yaml
---
title: "Building Resilient Microservices with .NET Core, Polly, and RabbitMQ | by Kevin Patrick Boylan | .NET Software Engineer | Medium"
source: https://medium.com/@kevinpatrickboylan/building-resilient-microservices-with-net-core-polly-and-rabbitmq-f11fb83d31e0
date_published: 2025-05-22T15:13:43.127Z
date_captured: 2025-08-22T10:52:41.591Z
domain: medium.com
author: "Kevin Patrick Boylan | .NET Software Engineer"
category: architecture
technologies: [ASP.NET Core, Polly, RabbitMQ, SQL Server, Angular, Swagger, RabbitMQ.Client, .NET, NuGet, MassTransit]
programming_languages: [C#, SQL, JavaScript]
tags: [microservices, resilience, fault-tolerance, dotnet, message-queue, asynchronous, circuit-breaker, retry-policy, fallback, background-service]
key_concepts: [microservice-architecture, resilience-patterns, transient-fault-handling, message-queues, dependency-injection, circuit-breaker-pattern, retry-policy, persistent-outbox]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores building resilient microservices using .NET Core, Polly, and RabbitMQ, drawing insights from the lifescience manufacturing domain. It highlights the limitations of monolithic architectures and advocates for microservices to achieve better scalability, maintainability, and fault tolerance. The author demonstrates practical resilience patterns like Retry, Circuit Breaker, and Fallback using Polly, combined with RabbitMQ for asynchronous communication. A "Persistent Outbox" pattern is introduced using a SQL Server table and a .NET Core Worker Service to ensure message delivery even during prolonged outages. The article provides a comprehensive walkthrough of handling transient failures and ensuring data integrity in a distributed system.]
---
```

# Building Resilient Microservices with .NET Core, Polly, and RabbitMQ | by Kevin Patrick Boylan | .NET Software Engineer | Medium

# Building Resilient Microservices with .NET Core, Polly, and RabbitMQ

[

![Kevin Patrick Boylan | .NET Software Engineer](https://miro.medium.com/v2/resize:fill:64:64/1*ck1Avv8imtXIj6XzUfcVOO.jpeg)





](/@kevinpatrickboylan?source=post_page---byline--f11fb83d31e0---------------------------------------)

[Kevin Patrick Boylan | .NET Software Engineer](/@kevinpatrickboylan?source=post_page---byline--f11fb83d31e0---------------------------------------)

Follow

12 min read

·

May 22, 2025

29

2

Listen

Share

More

# Introduction

As someone with experience in developing software in the lifescience manufacturing domain, I’m interested in finding alternatives to more traditional and monolithic approaches. In this domain, system reliability and traceability **aren’t optional** — they’re absolutely critical and essential.

But I’ve seen from experience just a few of the limitations of the **monolithic approach**: tight coupling, poor fault isolation, difficult deployments, maintenance and troubleshooting are just a few of the shortcomings.

That’s when I decided to shift toward a **microservice architecture** using **.NET Core**. This new perspective allowed me to envision breaking down large, tightly coupled applications into smaller, focused, and independently deployable services. Each service could then evolve on its own, making the overall system more scalable and maintainable.

In what follows, I’ll share some of the key lessons, patterns, and techniques I learned while building a simple but resilient microservice-based system with .NET Core, **Polly**, and **RabbitMQ** — with practical insights from real-world implementations.

## What is Microservice Architecture?

**Microservice architecture** is a design approach where a system is decomposed into a collection of small, independent services. Each service is responsible for a single business capability and can be developed, deployed, and scaled independently of the others.

Unlike monolithic applications — where all functionality lives in one large codebase — microservices promote **separation of concerns**. This allows teams to iterate faster, deploy services without affecting the whole system, and choose the most appropriate technologies for each part.

Some of the key characterstics of a microservice architecture:

**Autonomous and Decentralized**: Each microservice is self-contained, with its own data and logic. It can be developed, deployed, and scaled independently.

**Resilience and Fault Tolerance**: Failure in one service doesn’t bring down the whole system — especially when fault-handling strategies (like those provided by Polly) are in place.

**Loose Coupling and Scalability**: Microservices interact through well-defined APIs or asynchronous messaging systems, reducing dependencies. Individual microservices can be scaled horizontally based on demand (e.g., scaling only the payment service during peak sales time periods).

## When to Use Microservices

_The system is large and complex enough to benefit from modularization_  
_Multiple teams are working on different parts of the application_  
_Independent scaling and deployment are required_  
_High availability and fault tolerance are business-critical_

Bear in mind though, that a microservice architecture can introduce complexity in operations, monitoring, testing, and communication between services. It’s not always the right choice for small or simple applications.

In my case, the shift to microservices was driven by a need for **scalability**, **resilience**, and **better maintainability** in a production environment that must comply with strict quality and reliability standards.

## Technology Stack Overview

Here’s a quick summary of the technologies I used in my examples.

**.NET Core**

.NET Core was my natural choice, for many obvious and well-known reasons:

It offers performance and reliability.  
It has built-in support for modern architecture patterns such as **Dependency Injection** (DI).  
It has a wide availability (e.g. through NuGet packages) of reliable, open source libraries to extend .NET Core capabilities.

**Polly**

Polly is a .NET resilience and transient-fault-handling library that allows you to configure policies such as Retry, Circuit Breaker, Timeout, and Fallback in a fluent and thread-safe manner.

Some of my use-case requirements were:

**Retry logic** for an unstable or unavailable message queue.  
**Circuit breaker logic** to prevent cascading failures.  
**Timeout policies** to avoid hanging requests.  
**Fallback strategies** to persist messages (for re-publishing) during partial failure cases.

Polly turned out to be an extremely straightforward and powerful tool for implementing a resilient and self-healing system.

**RabbitMQ**

Microservices need to communicate, and while REST APIs work well for synchronous requests, they don’t scale well for event-driven or loosely coupled communication. That’s where **RabbitMQ** came in.

These are just a few reasons I used RabbitMQ:

It supports asynchronous, decoupled communication.  
Message queues help absorb traffic spikes.  
It ensures delivery even when consumers are temporarily offline.  
With the .NET Core **RabbitMQ.Client** library, message queueing can be achieved quickly with minimal code.

# First Scenario: When Everything Works

In what follows, I want to go through a simple but realistic use case that includes these modules:

1.  A front-end UI developed with **Angular**. The UI is a simple example of a fictional **Manufacturing Execution System** (MES) application, where users can view and operate different aspects of a pharmaceutical manufacturing process. The back-end database for the MES is **SQL Server**. In my use case, we’ll look at the case of a user adding a new Material (e.g. Cellulose, Lactose).

![Screenshot of an "Add New Material" form in an "ApexPharm ERP Portal UI", likely an Angular front-end, showing fields like Material Name, Description, Current Stock, Material Type, UOM, and an "Is Active" checkbox.](https://miro.medium.com/v2/resize:fit:700/1*LUZ77-p03_KWJc7QO67_Zg.jpeg)

2\. A .NET Core API that hosts a **RabbitMQ** publisher service. This service processes requests received from the UI, by publishing them to a RabbitMQ message queue. The payload structure (submitted from the UI) can be viewed in Swagger:

![Screenshot of a Swagger UI displaying the POST /api/Material/rabbitmqCreate endpoint, showing the expected JSON request body schema for adding a material.](https://miro.medium.com/v2/resize:fit:700/1*uGDtKXP3G-2ey23AeZY1UA.jpeg)

3\. A .NET Core **Worker Service**. This service runs in the background and has just one responsibility: To monitor a RabbitMQ message queue for new messages. When it finds a new message, the message is taken off the queue and inserted into a SQL Server database. The service functions as a **subscriber**, and it has only one job to perform: Transform published Material messages into SQL Server data:

![Screenshot of a SQL Server Management Studio (SSMS) window showing a table named 'Material' with various material entries (e.g., Lactose, Magnesium Stearate, Amoxicillin).](https://miro.medium.com/v2/resize:fit:700/1*ib12JRmlxFH1gO1m_NpEMA.jpeg)

4\. A RabbitMQ **message queue** that exists on my localhost. The queue I’ll be using in my examples is AddMaterialQueue, and it is currently empty:

![Screenshot of the RabbitMQ Management UI showing the "Queues and Streams" tab. It lists two queues, "AddMaterialQueue" and "UpdateMaterialQueue", both currently empty.](https://miro.medium.com/v2/resize:fit:700/1*3pjg_WC1jdrHUKUWEhYUig.jpeg)

To publish a new Material message to the queue, I’ll use Swagger to execute a **Http POST**:

![Screenshot of a Swagger UI showing the POST /api/Material/rabbitmqCreate endpoint with an example JSON payload for "Microcrystalline Cellulose" and an "Execute" button.](https://miro.medium.com/v2/resize:fit:700/1*TInl7hZoBs5mLx3vKPuPZQ.jpeg)

We can now see a new message in the queue:

![Screenshot of the RabbitMQ Management UI showing the "Queues and Streams" tab. The "AddMaterialQueue" now has 1 message "Ready" and 1 "Total", indicating a message has been published.](https://miro.medium.com/v2/resize:fit:700/1*nI7KvChT6Dq6HFUNoifDfg.jpeg)

And we can view the message payload:

![Screenshot of the RabbitMQ Management UI showing the details of a message in the "AddMaterialQueue", displaying its JSON payload.](https://miro.medium.com/v2/resize:fit:700/1*CMKDmb9s2mYxaXbVRfACHw.jpeg)

If I run my subscriber worker service, **MaterialQueueWorkerSvc**, the message is removed from the queue, its payload processed and inserted into SQL Server:

![Screenshot of a console window output from the `MaterialQueueWorkerSvc` showing logs indicating that a message was consumed from RabbitMQ and successfully inserted into the SQL Server database.](https://miro.medium.com/v2/resize:fit:700/1*CyMwf7TBiE9j3teq-4_TPg.jpeg)

![Screenshot of a SQL Server Management Studio (SSMS) window showing the 'Material' table with a newly added row for "Microcrystalline Cellulose", confirming the message processing.](https://miro.medium.com/v2/resize:fit:643/1*MaPdiBIDtfQu5YN49b3UIQ.jpeg)

**_So far, so good_**. This was all a very high-level description of a use case executing correctly. But what if something goes wrong?

# Second Scenario: When Something Goes Wrong

There are a number of reasons why a message can fail to publish (i.e. make it successfully onto the queue). In my simple case, my message queue is on the same machine as my APIs and backround services. But in a real-world scenario, a message queue will likely be accessible over a network, which means there can be multiple reasons for publishing failures: Network latency, or delays, server reboots, DNS issues, etc.

So let’s go through the following use case: one of our front-end users tries to add a new Material, but for some reason the request fails to make it to the message queue. In this case, we assume a Server 500 error is returned to the user.

Depending on the nature and severity of the problem, the user may (or may not) successfully publish the material message after a second attempt. But if she does not succeed the second time, she might just give up for the day and work on something else. In the meantime, **_there is no persisted record of the material message_**. The message is gone, without a trace, and our user will have to try adding it again at some later time.

**Adding Resilience with Polly**

In a distributed system, **bad things can and will happen**. Networks fail, services go down, and queues become temporarily unreachable. The question isn’t whether something will break — it’s **how your system responds when it does**. That’s where **Polly** becomes an essential tool in a .NET microservice architecture.

**Handling Transient Failures Gracefully**

Let’s revisit the scenario from the end of the last section: a front-end user submits a request to create a new Material. But in our current scenario, the API attempts to publish the message to RabbitMQ, but for some reason — a brief network hiccup, DNS timeout, or a short-lived queue server restart — the message doesn’t make it onto the queue. The result? A 500 Internal Server Error is returned, and the user is frustrated by the system.

Without resilience, this is a dead end.

With Polly, we can introduce fault-handling logic that automatically retries the operation, or falls back to an alternate strategy (such as logging the failure for later replay), improving both user experience and data integrity.

**Polly in Action: Retry Policy**

Here’s an example of how I used Polly to wrap a message-publishing operation:

![Code snippet (C#) demonstrating a Polly `WaitAndRetryAsync` policy, handling `BrokerUnreachableException` and `TimeoutException` with exponential backoff and logging.](https://miro.medium.com/v2/resize:fit:700/1*A1kAqoMZz1Xcq0eiMgAFmw.jpeg)

In this example:

The policy handles transient exceptions like **BrokerUnreachableException** or **TimeoutException**.  
Retries happen with exponential backoff (2, 4, 8 seconds).  
A log entry is created for every retry, giving us insight into persistent issues.

**Circuit Breakers for System Protection**

Retries are useful — but in some cases, they **can make things worse**. If RabbitMQ is completely down, for example, retrying repeatedly will only overload the service and slow down the user experience.

That’s where **circuit breakers** come in:

![Code snippet (C#) demonstrating a Polly `CircuitBreakerAsync` policy, configured to break after 3 failures for 30 seconds.](https://miro.medium.com/v2/resize:fit:700/1*ByS7piqvCrZpHaq5smn9Sw.jpeg)

This prevents the system from making repeated failing calls and gives the failing component time to recover.

**Fallbacks and Graceful Degradation**

Sometimes it’s better to return a meaningful fallback response than a generic 500 error. Polly allows you to define a fallback like this:

![Code snippet (C#) demonstrating a Polly `FallbackAsync` policy, which executes a fallback action (e.g., logging or returning a default value) if the primary operation fails.](https://miro.medium.com/v2/resize:fit:700/1*Ln9Y8zqlPcnEf6jUkC39Xw.jpeg)

Fallbacks don’t fix the root cause, but they allow the system to **degrade gracefully**, buying you time to fix the problem without disrupting user trust.

**Results of Using Polly**

By combining \*Retry\*, \*Circuit Breaker\*, and \*Fallback\* policies:

1.  Message delivery became far more reliable in the face of transient faults.
2.  User-facing errors were reduced, even during infrastructure issues.
3.  Logging and observability improved, helping me diagnose and fix issues faster.

**Ensuring Message Delivery with a Pending Queue Table**

While Polly’s retries and fallbacks can handle **temporary failures,** there are still cases where all retries fail and message publishing isn’t possible in the moment. In these situations, rather than losing the message, we fall back to **persisting it locally** in a dedicated SQL Server table — PendingQueueMessages — for later reprocessing.

This approach turns a transient failure into a **recoverable one**, using a pattern sometimes called a “**Persistent Outbox**”.

**Fallback to Local Persistence**

Here’s how the fallback works in practice. If all retries to publish to RabbitMQ fail, we write the message payload into the **PendingQueueMessages** table:

![Code snippet (C#) showing a `CreatePendingMessageAsync` method in a repository, responsible for inserting a message payload into a `PendingQueueMessages` SQL Server table.](https://miro.medium.com/v2/resize:fit:700/1*KDGKhfQRkdk5Pk_oGlsFgA.jpeg)

This ensures:  
1\. The original message isn’t lost.  
2\. We retain full traceability of failed events.  
3\. Recovery is possible without user intervention.

**An Example of Exception-Handling and Fallback Execution**

Now I’ll go through an example of exception-handling to provide some useful details about three Polly policies: **Retry**, **Circuitbreaker** and **Fallback**. To raise an exception, I simply created a boolean variable **\_simulateFailure** with a value of **True** to test my Polly policies. Below is the service method that publishes (when there is no exception, of course) a message to RabbitMQ:

![Code snippet (C#) of a service method `PublishMaterialMessageAsync` that attempts to publish a message to RabbitMQ, including a `_simulateFailure` boolean for testing.](https://miro.medium.com/v2/resize:fit:700/1*rhqo1vv3n63D8-iFar2xGA.jpeg)

With \_simulateFailure set to True, the method will not successfully publish the message. Because I set RetryCount to 3 in the Retry policy, the method will execute three times before the Fallback policy is executed. The Fallback policy helps ensure that the Json payload from the Http Request is saved in SQL Server. To save, I created a new Repository method to handle the insertion:

![Code snippet (C#) showing the `CreatePendingMessageAsync` method within a repository, which saves a `PendingQueueMessage` entity to the database.](https://miro.medium.com/v2/resize:fit:700/1*Bna-75_fCcxP8PP49K7Pxw.jpeg)

Then I updated the service method to handle the Fallback scenario. The important thing to notice here is the assignment of the **CreatePendingMessageAsync** repository method to the Fallback policy.

![Code snippet (C#) showing how a Polly `FallbackAsync` policy is configured to call the `CreatePendingMessageAsync` repository method if the primary operation fails.](https://miro.medium.com/v2/resize:fit:700/1*JefAKzG6sq2MKHuTS9me7w.jpeg)

Now I’ll create and post a Material request with **Swagger** and we’ll see what happens when \_simulateFailure is True.

![Screenshot of a Swagger UI showing a POST request for adding a material, with the "Execute" button clicked, simulating a failure scenario.](https://miro.medium.com/v2/resize:fit:700/1*3_dVkmdiXmZY-Q21SBzaOw.jpeg)

One interesting thing to note in this example is that, although I’m raising an exception three times, the returned response code is 200, indicating success. This makes sense, because our Fallback policy ensures a smooth handling of a scenario where a message cannot successfully publish to RabbitMQ.

![Screenshot of a Swagger UI showing the response of the POST request, indicating a 200 OK status despite an internal simulated failure, due to the Fallback policy.](https://miro.medium.com/v2/resize:fit:700/1*uOCTMCSqGgq9IOjFo3URVA.jpeg)

Below we can see that, although the message failed to publish, it was successfully saved to the database. Three important things to note here are the saved Payload Json, the name of the RabbitMQ queue, and the IsPublished flag. With these data points, I can create a background service that monitors the table for unpublished messages and then re-publishes them.

![Screenshot of a SQL Server Management Studio (SSMS) window showing the `PendingQueueMessages` table with a new entry. The `IsPublished` column is 0 (false), indicating the message was saved due to a failed RabbitMQ publish.](https://miro.medium.com/v2/resize:fit:700/1*bYyw0k7ykCSCrkTUQfaCiw.jpeg)

**Background Service for Reprocessing Messages**

To complete the pattern, I created a hosted **.NET Core Worker** background service that periodically scans the **PendingQueueMessages** table and tries to republish any messages that failed previously.

Here is my simple Worker class. Every 10 seconds, it make an asynchronous call to **ProcessPendingMessagesAsync**.

![Code snippet (C#) of a `PendingMessageWorkerSvc` class, inheriting from `BackgroundService`, which periodically calls `ProcessPendingMessagesAsync` every 10 seconds.](https://miro.medium.com/v2/resize:fit:700/1*1DtdJbYT3-H86Pguu3_qWA.jpeg)

The responsibility of **ProcessPendingMessagesAsync** is to query the database for unpublished messages. Retrieved messages are re-published to RabbitMQ and the corresponding database records are updated to IsPublished = 1 (true).

![Code snippet (C#) of the `ProcessPendingMessagesAsync` method, which queries the database for unpublished messages, republishes them to RabbitMQ, and updates their `IsPublished` status.](https://miro.medium.com/v2/resize:fit:700/1*rbkWTywcYvMgi0QzdIBoeg.jpeg)

Just to refresh, let’s recall that we have an unpublished message queued up in our database:

![Screenshot of a SQL Server Management Studio (SSMS) window showing the `PendingQueueMessages` table with a new entry. The `IsPublished` column is 0 (false), indicating the message was saved due to a failed RabbitMQ publish.](https://miro.medium.com/v2/resize:fit:700/1*bYyw0k7ykCSCrkTUQfaCiw.jpeg)

I’ll now start the **PendingMessageWorkerSvc** background service. If all goes well, the service will process the unpublished message by successfully re-publishing it and then updating the database record.

We can see signs of success in the console window output:

![Screenshot of a console window output from the `PendingMessageWorkerSvc` showing logs indicating that a pending message was retrieved from the database and successfully re-published to RabbitMQ.](https://miro.medium.com/v2/resize:fit:700/1*YWZ90lJHzmrPz3zBlYsCNA.jpeg)

Now we’ll look at RabbitMQ and confirm that the message was successfully re-published:

![Screenshot of the RabbitMQ Management UI showing the "AddMaterialQueue" with 1 message "Ready" and 1 "Total", indicating the message was successfully re-published by the worker service.](https://miro.medium.com/v2/resize:fit:700/1*vInOBLIjA7Rug_biMk5gVA.jpeg)

And finally, we can see that the message was successfully updated in the database:

![Screenshot of a SQL Server Management Studio (SSMS) window showing the `PendingQueueMessages` table. The `IsPublished` column for the previously pending message is now 1 (true), confirming successful reprocessing.](https://miro.medium.com/v2/resize:fit:700/1*dZGx3Pn3u_MHpOCmrwov0A.jpeg)

# Conclusion

Building resilient microservices isn’t just about choice of architecture (e.g. **Microservice versus Monolithic**) — it’s about planning and designing for failure and recovery from the start. By combining .NET Core, Polly, and RabbitMQ, I created a simple but realistic system that not only scales well, but also recovers gracefully when things go wrong.

With the implementation of layers of retries, circuit breakers, fallback queues and background reprocessing, each one can add stability to the flow of messages between services. These patterns don’t just improve reliability — they can also give end-users a better experience and give developers peace