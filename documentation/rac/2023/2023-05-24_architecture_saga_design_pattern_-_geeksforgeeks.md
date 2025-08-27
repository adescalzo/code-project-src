```yaml
---
title: SAGA Design Pattern - GeeksforGeeks
source: https://www.geeksforgeeks.org/system-design/saga-design-pattern/
date_published: 2023-05-24T09:46:38.000Z
date_captured: 2025-09-04T20:15:34.608Z
domain: www.geeksforgeeks.org
author: Unknown
category: architecture
technologies: [Microservices Architecture, Message Queues, Event Streams]
programming_languages: []
tags: [saga-pattern, distributed-transactions, microservices, system-design, design-patterns, event-driven, orchestration, fault-tolerance, consistency]
key_concepts: [SAGA Design Pattern, Distributed Transactions, Two-Phase Commit (2PC), Compensating Actions, Microservices Architecture, Choreography-based SAGA, Orchestration-based SAGA, SAGA Execution Coordinator, SAGA Log]
code_examples: false
difficulty_level: intermediate
summary: |
  The SAGA Design Pattern is a crucial approach for managing long-running and distributed transactions, especially within microservices architectures. It addresses the limitations of traditional protocols like Two-Phase Commit (2PC) by breaking complex transactions into a series of smaller, independent operations. Each step in a SAGA has a corresponding compensating action, ensuring consistency and fault tolerance by undoing previous successful steps if a failure occurs. The pattern can be implemented using either a choreography-based (event-driven) or orchestration-based (centralized coordinator) approach. While it offers significant advantages in scalability and resilience, implementing SAGA requires careful design of compensation logic to handle all possible failure scenarios.
---
```

# SAGA Design Pattern - GeeksforGeeks

# SAGA Design Pattern

Last Updated : 23 Jul, 2025

Comments

Improve

Suggest changes

7 Likes

Like

Report

The SAGA Design Pattern is a pattern used to manage long-running and distributed transactions, particularly in [microservices architecture](https://www.geeksforgeeks.org/system-design/microservices/). Unlike traditional monolithic transactions, which require a single, centralized transaction management system, the SAGA pattern breaks down a complex transaction into a series of smaller, isolated operations, each handled by a different service.

![Diagram illustrating the SAGA Design Pattern with a message broker coordinating between an order service, payment service, and shipping service.](https://media.geeksforgeeks.org/wp-content/uploads/20241108164412754920/saga-design-patterns_.webp)

SAGA Design Pattern

Table of Content

*   [What is a Distributed Transaction?](#what-is-a-distributed-transaction)
*   [What is 2PC for Distributed Transaction Management?](#what-is-2pc-for-distributed-transaction-management)
*   [Problems with Traditional Distributed Transaction Protocols](#problems-with-traditional-distributed-transaction-protocols)
*   [What is the SAGA Design Pattern?](#what-is-the-saga-design-pattern)
*   [Why do we need SAGA Design Pattern?](#why-do-we-need-saga-design-pattern)
*   [How SAGA Design Pattern Works?](#how-saga-design-pattern-works)
*   [Example of SAGA Design Pattern](#example-of-saga-design-pattern)
*   [Approaches to Implemement SAGA Design Pattern](#approaches-to-implemement-saga-design-pattern)
*   [Advantages and Disadvantages of SAGA Pattern](#advantages-and-disadvantages-of-saga-pattern)

## What is a Distributed Transaction?

A distributed transaction refers to a type of transaction that involves multiple, separate systems or databases, often spread across different locations or networks, which need to work together to complete a task. It’s like a coordinated team effort, where each system handles a small part of the work, but they all must complete their respective tasks successfully for the overall transaction to be considered successful.

For example, imagine you're making an online purchase. The transaction might involve:

*   Your bank checks if you have enough funds.
*   The e-commerce platform reserving the product you're buying.
*   A shipping service getting ready to send your item.
*   A warehouse updating its stock levels

## What is 2PC for Distributed Transaction Management?

**2PC (Two-Phase Commit)** is a protocol used to ensure all participants in a distributed transaction either commit or abort, ensuring consistency. In the first phase, the coordinator asks all participants to agree to commit, and in the second phase, participants either vote to commit or abort the transaction.

## Problems with Traditional Distributed Transaction Protocols

Traditional distributed transaction protocols like Two-Phase Commit (2PC) have limitations in modern systems, primarily due to:

*   **Blocking Nature:** If the coordinator fails after initiating the transaction, participants may be left waiting indefinitely, causing delays.
*   **Single Points of Failure:** The coordinator is crucial for decision-making. If it crashes, the entire transaction can get stuck, impacting reliability.
*   **Network Partitions:** If the network splits, some nodes might not receive the final decision, leading to inconsistent states (i.e., some nodes might commit while others don’t), which causes data inconsistency.

These problems make 2PC unsuitable for modern, highly available, and fault-tolerant systems.

## What is the SAGA Design Pattern?

The SAGA Design Pattern is a pattern used to manage long-running, distributed transactions in a microservices architecture.

*   Instead of relying on traditional, monolithic transactions (which require locking databases across services), the SAGA pattern breaks the transaction into a series of smaller, independent operations that each belong to a different service.
*   These smaller operations, also called saga steps, are executed sequentially or in parallel, with each step compensating for potential failures in the others.

## Why do we need SAGA Design Pattern?

SAGA is needed because 2PC, while simple, doesn't work well in distributed systems where availability and fault tolerance are critical. In real-world systems, network failures, crash recovery, and long-running transactions are common, and 2PC's blocking behavior and reliance on a single coordinator can make the system unreliable and slow. **SAGA** provides a more flexible, decentralized approach to managing long-running distributed transactions.

> **SAGA** addresses the limitations of **2PC** by breaking a transaction into smaller, independent steps, each with its own compensating action if something goes wrong. Here's how it solves key issues of 2PC:

*   **No Blocking**: Unlike 2PC, which blocks participants until the transaction is complete, SAGA allows each step to execute independently, avoiding delays due to coordinator failure.
*   **No Single Point of Failure**: SAGA doesn’t rely on a central coordinator. Each step is independent, so if one step fails, it doesn't block the entire process.
*   **Graceful Failure Handling**: Instead of rolling back the entire transaction (as in 2PC), SAGA uses compensating actions to undo successful steps if a failure occurs, ensuring the system remains consistent.
*   [**Resilience**](https://www.geeksforgeeks.org/system-design/resilient-system-system-design/) **to Network Partitions**: SAGA can continue even if parts of the network fail, because each step is independent, and compensating actions can be executed later when the partition resolves.

## How SAGA Design Pattern Works?

The SAGA Design Pattern manages long-running distributed transactions by breaking them into smaller steps, each with its own compensating action in case of failure. Here’s how it works:

*   **Breaking Down the Transaction:** A big transaction is divided into smaller, independent sub-transactions (steps), each handled by different services. For example, reserving a product, charging the customer, and shipping the item.
*   **Independent Execution:** Each step runs independently without waiting for the others to finish. If a step succeeds, the next step proceeds.
*   **SAGA Execution Coordinator**: The SAGA Execution Coordinator manages and coordinate the flow of these steps, triggering each one in sequence.
*   **Compensating Actions:** If any step fails, the system doesn’t roll back everything. Instead, it executes compensating actions to undo the work done in previous successful steps, like refunding a payment or canceling a reservation.
*   **SAGA Log: H**elps manage and track the state of a long-running distributed transaction, ensuring that all steps are completed successfully or properly compensated in case of failure.

## Example of SAGA Design Pattern

Let's understand how SAGA works using the example of an e-commerce order process with the SAGA Execution Coordinator and SAGA Log.

> *   **Step 1: Create Order**: Reserve the product.
> *   **Step 2: Process Payment**: Charge the customer’s card.
> *   **Step 3: Update Inventory**: Reduce the stock.
> *   **Step 4: Deliver Order**: Ship the product to the customer.

### How It Works:

![Diagram showing an example of the SAGA Design Pattern, with a Saga Execution Coordinator managing steps like Create Order, Process Payment, Update Inventory, and Deliver Order, alongside a Saga Log tracking their states.](https://media.geeksforgeeks.org/wp-content/uploads/20241108164457445402/example-of-saga-design-pattern_.webp)

Example of SAGA Design Pattern

*   The SAGA Execution Coordinator manages the flow of these steps, triggering each one in sequence.
*   Each step has a compensating action that is triggered if something goes wrong (e.g., if payment fails, the product is unreserved).
*   The SAGA Log tracks the state of each step. It logs each step as in-progress, completed, or failed, and records any compensating actions needed.

### **Flow of SAGA**:

![Flowchart illustrating the SAGA pattern with transaction steps (Create Order, Process Payment, Update Inventory, Deliver Order) and their corresponding compensating transactions (Cancel Order, Reverse Payment, Reverse Inventory Update, Cancel Delivery).](https://media.geeksforgeeks.org/wp-content/uploads/20241108164520549995/flow-of-saga.webp)

Flow of SAGA

*   **Start the SAGA**:
    *   The process begins by executing the first step in the sequence.
*   **Execute Step 1**:
    *   The system performs the first sub-transaction (e.g., creating the order and reserving the product). If this step is successful, move to Step 2. If it fails, trigger its compensating action (e.g., cancel the order) and stop.
*   **Execute Step 2**:
    *   If Step 1 was successful, the next step (e.g., process the payment) is executed. If Step 2 fails (e.g., payment is declined), its compensating action (e.g., refund the payment) is triggered, and Step 1’s compensating action (e.g., unreserve the product) is also executed.
*   **Execute Step 3**:
    *   If Step 2 was successful, proceed to the next step (e.g., update inventory). If Step 3 fails, its compensating action (e.g., reverse inventory update) is triggered, and Step 2’s compensating action (e.g., refund payment) is executed.
*   **Execute Step 4**:
    *   Finally, if all previous steps are successful, the last step (e.g., deliver the order) is executed. If any prior step has failed, its compensating actions are triggered, ensuring the system remains consistent.

## Approaches to Implemement SAGA Design Pattern

Below are the two main approaches to implementing the SAGA pattern:

### 1. **Choreography-Based Approach (Event-Driven)**

*   There is no central coordinator; each service knows what to do next and triggers the next step by emitting events.
*   Services communicate through events (e.g., via message queues or event streams). Each service listens for events and performs its own action when the event occurs.
*   Services operate independently and don’t need to know about each other’s details, just the events they need to react to.
*   If a service fails, it publishes a failure event, and other services can listen to it and perform compensating actions.

![Diagram illustrating the Choreography-Based (Event-Driven) SAGA approach, where services (order-service, payment-service, inventory-service) communicate by emitting and listening to events.](https://media.geeksforgeeks.org/wp-content/uploads/20241108164601760972/choreography-based-approach-event-driven_.webp)

Choreography-Based Approach (Event-Driven)

**Example**:

> *   **Order Service** creates an order and publishes the `"OrderCreated"` event.
> *   **Payment Service** listens for `"OrderCreated"` and processes payment, publishing `"PaymentProcessed"`.
> *   **Inventory Service** listens for `"PaymentProcessed"` and updates inventory, and so on.

### 2. **Orchestration-Based Approach (Centralized)**

*   A single SAGA Execution Coordinator (orchestrator) controls the flow of the entire saga.
*   The orchestrator tells each service when to start, what to do, and when to proceed to the next step.
*   The orchestrator has detailed knowledge of each service and their responsibilities in the saga.
*   The orchestrator manages failure recovery and calls the compensating actions if needed.

![Diagram illustrating the Orchestration-Based (Centralized) SAGA approach, with a Saga Execution Coordinator managing the flow of transactions (Create Order, Process Payment, Update Inventory, Deliver Order) and a Saga Log.](https://media.geeksforgeeks.org/wp-content/uploads/20241108164623117202/orchestration-based-approach-centralized_.webp)

Orchestration-Based Approach (Centralized)

**Example**:

> *   The SAGA Execution Coordinator starts the saga and tells the Order Service to create the order.
> *   Once the Order Service succeeds, the orchestrator tells the Payment Service to process the payment, and so on.

## Advantages and Disadvantages of SAGA Pattern

Below are the main advantages and disadvantages of SAGA Pattern:

**Advantages of SAGA Pattern**

**Disadvantages of SAGA Pattern**

With SAGA, if one step fails, the entire process can be rolled back or compensated without affecting other steps.

Implementing SAGA requires additional coding and architecture to handle compensation and rollback steps.

SAGA provides a clear and standardized way to handle errors and compensations, making it easier to debug and maintain.

Not all frameworks or platforms support SAGA out of the box, which can make implementation more difficult.

SAGA can support asynchronous processing, allowing for greater concurrency and performance.

The SAGA pattern requires careful design to ensure that the compensations and rollbacks are implemented correctly and can handle all possible failure scenarios.

SAGA can handle transactions across multiple services or databases, allowing for more scalable and distributed architectures.

SAGA may result in additional latency due to the need to coordinate between different services or databases.

Overall, the SAGA pattern is a powerful tool for managing distributed transactions and providing fault tolerance. However, it requires careful design and implementation to ensure that it is used effectively and does not introduce additional complexity or latency.

Comment

More info

[Advertise with us](https://www.geeksforgeeks.org/about/contact-us/?listicles)

[

A

](https://www.geeksforgeeks.org/user/abhishekdutt32/)

[abhishekdutt32](https://www.geeksforgeeks.org/user/abhishekdutt32/)

Follow

7

Improve

Article Tags :

*   [Design Pattern](https://www.geeksforgeeks.org/category/design-pattern/)
*   [System Design](https://www.geeksforgeeks.org/category/system-design/)