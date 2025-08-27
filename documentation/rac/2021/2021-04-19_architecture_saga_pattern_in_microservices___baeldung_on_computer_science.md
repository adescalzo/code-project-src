```yaml
---
title: "Saga Pattern in Microservices | Baeldung on Computer Science"
source: https://www.baeldung.com/cs/saga-pattern-microservices
date_published: 2021-04-19T06:46:03.000Z
date_captured: 2025-09-04T20:16:11.739Z
domain: www.baeldung.com
author: Somnath Musib
category: architecture
technologies: [Microservices, Relational Database, NoSQL Database, Axon Saga, Spring Boot, Eclipse MicroProfile LRA, Eventuate Tram Saga, Micronaut, Seata, Camunda, Apache Camel]
programming_languages: [Java]
tags: [microservices, saga-pattern, distributed-transactions, architecture, design-patterns, data-consistency, choreography, orchestration, two-phase-commit, enterprise-integration-patterns]
key_concepts: [microservice-architecture, distributed-transactions, saga-pattern, two-phase-commit, ACID-properties, compensating-transactions, saga-choreography, saga-orchestration, saga-execution-coordinator, idempotency, retryable-transactions]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Saga pattern as a robust solution for managing distributed transactions within microservice architectures. It begins by outlining the inherent challenges of maintaining data consistency across independent services, especially when each service manages its own database. The article then critically examines the Two-Phase Commit (2PC) protocol, highlighting its limitations in a distributed microservice environment. Subsequently, it delves into the Saga pattern, explaining its core principle of using a sequence of local transactions with compensating transactions to ensure atomicity and data integrity. Finally, the article differentiates between the choreography and orchestration approaches to implementing the Saga pattern, providing examples of frameworks suitable for each method.]
---
```

# Saga Pattern in Microservices | Baeldung on Computer Science

# Saga Pattern in Microservices

Last updated: March 26, 2025

## 1. Overview

A microservice-based application is inherently a distributed system. While this architectural style offers numerous benefits, it also presents challenges, particularly in handling transactions that span multiple services.

This tutorial explores the Saga architecture pattern, which provides a method for managing distributed transactions in a microservice architecture.

## Further reading:

## Microservices and Cross-Cutting Concerns

Learn about common cross-cutting concerns in a microservices architecture.

## Service Discovery in Microservices

An introduction to Service Discovery in the context of microservices.

## Explaining the Context Design Pattern

Explore several patterns that use the idea of context.

## 2. Database per Service Pattern

One of the advantages of microservice architecture is the flexibility to choose the technology stack per service. For instance, one service might use a relational database while another uses a NoSQL database.

This model allows services to manage domain data independently on a data store that best suits their data types and schema. It also enables services to scale their data stores on demand and insulates them from failures in other services.

However, when a transaction spans across multiple services, ensuring data consistency across these independent service databases becomes a significant challenge. The next section will detail this challenge with an example.

## 3. Distributed Transaction

To illustrate distributed transactions, consider an e-commerce application that processes online orders, implemented with a microservice architecture.

This application might have microservices for creating orders, processing payments, updating inventory, and delivering orders.

Each of these microservices performs a local transaction to implement its individual functionality:

![Diagram showing an e-commerce order processing flow as a distributed transaction. It illustrates four microservices: Create Order (Transaction 1), Process Payment (Transaction 2), Update Inventory (Transaction 3), and Deliver Order (Transaction 4), all contributing to a single logical transaction across multiple services.](https://www.baeldung.com/wp-content/uploads/sites/4/2021/04/distributed-transaction.png)

This scenario exemplifies a distributed transaction, as the transaction boundary crosses multiple services and their respective databases.

For a successful order processing service, all four microservices must complete their individual local transactions. If any microservice fails its local transaction, all preceding completed transactions should roll back to ensure data integrity.

## 4. Challenges of Distributed Transaction

Distributed transactions in a microservice architecture present two key challenges.

**The first challenge is maintaining ACID properties.** To ensure transaction correctness, it must be Atomic, Consistent, Isolated, and Durable (ACID). Atomicity ensures all or none of the transaction steps complete. Consistency moves data from one valid state to another. Isolation guarantees that concurrent transactions produce the same result as sequential ones. Durability means committed transactions remain committed despite system failures. In a distributed transaction scenario, ensuring ACID properties across several services is crucial.

**The second challenge is managing the transaction isolation level.** This specifies the amount of data visible within a transaction when other services simultaneously access the same data. For example, if one object in a microservice is persisted while another request reads the data, should the service return the old or new data?

## 5. Understanding Two-Phase Commit

The Two-Phase Commit protocol (2PC) is **a widely used pattern to implement distributed transactions.** It can be applied in a microservice architecture for this purpose.

In 2PC, a coordinator component controls the transaction and manages its logic.

The other components are the participating nodes (e.g., the microservices) that run their local transactions:

![Diagram illustrating the Two-Phase Commit (2PC) protocol. A central Coordinator interacts with multiple participating nodes (microservices) to manage a distributed transaction across them in two phases: Prepare and Commit/Rollback.](https://www.baeldung.com/wp-content/uploads/sites/4/2021/04/two-phase-commit.png)

As its name suggests, the two-phase commit protocol executes a distributed transaction in two phases:

1.  **Prepare Phase** – The coordinator asks participating nodes if they are ready to commit the transaction. Participants respond with a _yes_ or _no_.
2.  **Commit Phase** – If all participating nodes respond affirmatively in phase 1, the coordinator instructs all of them to commit. If at least one node responds negatively, the coordinator instructs all participants to roll back their local transactions.

## 6. Problems With 2PC

Despite its utility for distributed transactions, 2PC has several shortcomings:

*   The **transaction responsibility rests solely on the coordinator node**, making it a single point of failure.
*   All other services must wait for the slowest service to confirm, meaning the overall transaction performance is limited by the slowest participant.
*   The two-phase commit protocol is **inherently slow due to its chattiness and dependency on the coordinator.** This can lead to scalability and performance issues in microservice architectures with many services.
*   Two-phase commit protocol is **not supported in NoSQL databases.** Therefore, it cannot be applied in microservice architectures where one or more services utilize NoSQL databases.

## 7. Introduction to Saga

### 7.1. What Is Saga Architecture Pattern?

The Saga architecture pattern **provides transaction management using a sequence of local transactions.**

A local transaction is a unit of work performed by a Saga participant. Every operation within a Saga can be rolled back by a compensating transaction. The Saga pattern guarantees that either all operations complete successfully, or the corresponding compensation transactions are executed to undo previously completed work.

In the Saga pattern, **a compensating transaction must be _idempotent_ and _retryable_.** These two principles ensure that transactions can be managed without manual intervention.

The Saga Execution Coordinator (SEC) guarantees these principles:

![Diagram illustrating the Saga pattern for online order processing. It shows a sequence of local transactions (Create Order, Process Payment, Update Inventory, Deliver Order) and corresponding compensating transactions (Cancel Order, Reverse Payment, Reverse Inventory Update, Cancel Delivery) to handle failures.](https://www.baeldung.com/wp-content/uploads/sites/4/2021/04/saga-pattern.png)

The diagram above visualizes the Saga pattern for the online order processing scenario discussed earlier.

### 7.2. The Saga Execution Coordinator

The **Saga Execution Coordinator is the central component for implementing a Saga flow.** It maintains a Saga log that captures the sequence of events for a distributed transaction.

In case of any failure, the SEC component inspects the Saga log to identify the impacted components and the correct sequence for running compensating transactions.

If the SEC component itself fails, it can read the Saga log upon recovery. It can then identify successfully rolled-back transactions, pending ones, and take appropriate actions:

![Diagram showing the Saga Execution Coordinator (SEC) interacting with multiple microservices (Create Order, Process Payment, Update Inventory, Deliver Order) and maintaining a Saga Log to track the state of a distributed transaction.](https://www.baeldung.com/wp-content/uploads/sites/4/2021/04/saga-execution.png)

There are two main approaches to implementing the Saga pattern: choreography and orchestration. These are discussed in the following sections.

### 7.3. Implementing Saga Choreography Pattern

In the Saga Choreography pattern, **each microservice participating in the transaction publishes an event that is then processed by the next microservice.**

To use this pattern, a microservice needs to be designed to be part of the Saga and use an appropriate framework for its implementation. In this pattern, the Saga Execution Coordinator can either be embedded within the microservice or exist as a standalone component.

In Saga choreography, the flow is successful if all microservices complete their local transactions without reporting any failures.

The following diagram demonstrates a successful Saga flow for the online order processing application:

![Diagram illustrating a successful Saga Choreography flow for order processing. Each microservice (Create Order, Process Payment, Update Inventory, Deliver Order) publishes an event upon completion, triggering the next service in the sequence.](https://www.baeldung.com/wp-content/uploads/sites/4/2021/04/saga-coreography.png)

In the event of a failure, **the microservice reports the failure to the SEC, which is then responsible for invoking the relevant compensation transactions**:

![Diagram illustrating a Saga Choreography flow with a failure. The Payment microservice reports a failure, and the Saga Execution Coordinator (SEC) initiates compensating transactions (e.g., Cancel Order) to roll back previous successful steps.](https://www.baeldung.com/wp-content/uploads/sites/4/2021/04/saga-coreography-2.png)

In this example, if the Payment microservice reports a failure, the SEC invokes the compensating transaction to unblock the seat (or cancel the order). If the call to the compensating transaction fails, the SEC is responsible for retrying it until it successfully completes. Recall that in Saga, a compensating transaction must be _idempotent_ and _retryable_.

The Choreography pattern **works well for greenfield microservice application development.** It is also suitable when there are fewer participants in the transaction.

Here are a few frameworks available to implement the choreography pattern:

*   [Axon Saga](https://docs.axoniq.io/axon-framework-reference/4.10/sagas/) – a lightweight framework widely used with Spring Boot-based microservices.
*   [Eclipse MicroProfile LRA](https://github.com/eclipse/microprofile-lra) – an implementation of distributed transactions in Saga for HTTP transport based on REST principles.
*   [Eventuate Tram Saga](https://eventuate.io/docs/manual/eventuate-tram/latest/getting-started-eventuate-tram-sagas.html) – a Saga orchestration framework for Spring Boot and Micronaut-based microservices.
*   [Seata](https://www.seata.io/docs/dev/mode/saga-mode/) – an open-source distributed transaction framework offering high-performance and easy-to-use distributed transaction services.

### 7.4. Implementing Saga Orchestration Pattern

In the Orchestration pattern, **a single orchestrator is responsible for managing the overall transaction status.**

If any of the microservices encounter a failure, the orchestrator is responsible for invoking the necessary compensating transactions:

![Diagram illustrating the Saga Orchestration pattern. A central Orchestrator component directs the flow of local transactions across multiple microservices (Create Order, Process Payment, Update Inventory, Deliver Order) and manages compensating transactions in case of failures.](https://www.baeldung.com/wp-content/uploads/sites/4/2021/04/saga-orchestration.png)

The Saga orchestration pattern is **useful for brownfield microservice application development architecture.** This pattern is suitable when an existing set of microservices needs to implement the Saga pattern. It requires defining appropriate compensating transactions for the existing services.

Here are a few frameworks available to implement the orchestrator pattern:

*   [Camunda](https://camunda.com/) is a Java-based framework that supports the Business Process Model and Notation (BPMN) standard for workflow and process automation.
*   [Apache Camel](https://camel.apache.org/components/latest/eips/saga-eip.html) provides an implementation for the Saga Enterprise Integration Pattern (EIP).

## 8. Conclusion

In this article, we discussed the Saga architecture pattern for implementing distributed transactions in a microservice-based application.

We first introduced the challenges inherent in these implementations.

We then explored the Two-Phase Commit protocol, a popular alternative to Saga, and examined its limitations for distributed transactions in microservice-based applications.

Lastly, we discussed the Saga architecture pattern, how it works, and the two main approaches to implementing the Saga pattern in microservice-based applications: choreography and orchestration.