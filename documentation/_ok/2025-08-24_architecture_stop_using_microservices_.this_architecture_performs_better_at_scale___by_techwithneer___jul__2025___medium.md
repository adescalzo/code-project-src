```yaml
---
title: "Stop Using Microservices .This Architecture Performs Better at Scale | by techWithNeer | Jul, 2025 | Medium"
source: https://medium.com/@neerupujari5/stop-using-microservices-this-architecture-performs-better-at-scale-92afff527332
date_published: 2025-08-24T19:35:32.007Z
date_captured: 2025-08-27T11:17:47.214Z
domain: medium.com
author: techWithNeer
category: architecture
technologies: [wrk, HTTP, gRPC, Go, Wire, Fx, Uber Zap, GORM, GitHub Actions, Docker Compose, MySQL, Kafka, KV Cache, twemproxy, chef, HAProxy, Zookeeper, Thrift RPC, QUIC, Distributed Hash Table]
programming_languages: [Go, SQL]
tags: [microservices, modular-monolith, architecture, scalability, performance, latency, go, software-design, database, ci-cd]
key_concepts: [Microservices architecture, Modular monolith architecture, Operational complexity, Network latency, Dependency injection, Clean architecture principles, Distributed transactions, Feature flags]
code_examples: false
difficulty_level: intermediate
summary: |
  This article challenges the common belief that microservices are the ultimate solution for scaling, presenting a case study where a company achieved superior performance by transitioning from a 12-service microservices setup to a modular monolith. The author details how this shift led to a 60% reduction in latency, 2x faster deployments, and 70% lower infrastructure costs. The modular monolith architecture, which uses internal APIs and dependency injection to maintain separation of concerns without network overhead, is highlighted as a more efficient alternative for many organizations. The piece emphasizes the benefits of in-process function calls and single database transactions, concluding that microservices are often an unnecessary complexity for teams under 100 people.
---
```

# Stop Using Microservices .This Architecture Performs Better at Scale | by techWithNeer | Jul, 2025 | Medium

Member-only story

# Stop Using Microservices .This Architecture Performs Better at Scale

[

![techWithNeer](https://miro.medium.com/v2/resize:fill:64:64/1*Q5QE-CwwE671flvLPBekwA.jpeg)

](/@neerupujari5?source=post_page---byline--92afff527332---------------------------------------)

[techWithNeer](/@neerupujari5?source=post_page---byline--92afff527332---------------------------------------)

Follow

3 min read

·

Jul 11, 2025

65

18

Listen

Share

More

_When everyone is busy breaking down their systems into a hundred services, we decided to do the opposite. Here’s why it worked._

## Introduction

Microservices have been heralded as the ultimate solution to scaling software. Break things apart. Isolate them. Deploy independently. Scale independently. Everyone’s doing it , from startups to FAANG.

> But what if I told you that the very pattern you’ve been following is the reason your system is _not scaling efficiently_?

![Complex Microservices Architecture Diagram](https://i.stack.imgur.com/gK9qQ.png)
*A detailed microservices architecture diagram showcasing a large-scale system. It depicts client applications (Rider App, Partner App) interacting through an API Gateway, which then communicates with various mission-critical services (Dispatch, Trip, Payment, UserProfile) and 'Thousands of Microservices'. The diagram also illustrates advanced concepts like geo-sharding, service discovery (Chef, HAProxy, Zookeeper), database proxies (MySQL, Schemaless), cache proxies (twemproxy, KV Cache), and a data pipeline (Kafka, Data Warehouse) feeding into Business Intelligence.*

![Modular Monolith Architecture Diagram](https://miro.medium.com/v2/resize:fit:700/0*TQsgpje9NhlTr64Y.png)
*A simplified diagram illustrating the modular monolith architecture. It shows an HTTP/gRPC Gateway leading to distinct internal modules (Auth, Subscription, Payment) that communicate internally via function calls, all sharing a common infrastructure layer (DB, Cache, Logger).*

We dumped our microservices architecture for a **modular monolith** and saw:

*   **60% reduction in latency**
*   **2x faster deployments**
*   **70% fewer infrastructure costs**
*   **Dramatic improvements in developer velocity**

Let me explain.

### The Problem With Microservices at Scale

### 1\. Operational Complexity

Every microservice introduces overhead:

*   API contracts
*   CI/CD pipelines
*   Deployment coordination
*   Observability tooling
*   Retry logic and circuit breakers

> In a system of 12 microservices, the number of inter-service failure points exploded.

### 2\. Network Overhead and Latency

Here’s a basic example of what a single user request might trigger:

[Gateway] --> [Auth Service] --> [User Profile Service] --> [Subscription Service] --> [Payment Service]

Each hop adds:

*   Network latency
*   Serialization/deserialization
*   Security checks
*   Potential retries on failure

Under load, this overhead becomes non-trivial.

### Latency Benchmark:

(Using `wrk` load testing on a user dashboard request)

| Architecture | Avg Latency (ms) | 99th Percentile |
| :------------- | :--------------- | :-------------- |
| Microservices | 320 | 610 |
| Modular Monolith | 118 | 190 |

### The Architecture That Replaced It: Modular Monolith

No, not the classic spaghetti monolith.

We’re talking about a **modular, well-isolated, layered monolith** with **internal APIs** that resemble service boundaries just without the networking overhead.

### High-Level Architecture

```
                         +-----------------------------+
                         |     HTTP / gRPC Gateway     |
                         +-----------------------------+
                                   |
          +------------------------+------------------------+
          |                        |                        |
+------------------+   +------------------+   +------------------+
| Auth Module      |   | Subscription Mod |   | Payment Module   |
+------------------+   +------------------+   +------------------+
          \\                 /                      /
           \\               /                      /
            +-------------+----------------------+
                          |
               +--------------------------+
               |   Shared Infrastructure  |
               | (DB, Cache, Logger, etc)|
               +--------------------------+
```

### How Internal Calls Work

Instead of HTTP or gRPC, module boundaries are preserved **via function interfaces** and **interfaces with dependency injection**.

```go
// SubscriptionService.go

type SubscriptionService struct {
    repo SubscriptionRepo
}
func (s *SubscriptionService) GetUserSubscription(userID string) (*Subscription, error) {
    return s.repo.Fetch(userID)
}
```

> No HTTP. No JSON. Just direct function calls.

### How We Maintain Separation of Concerns

We use **package-level boundaries** and **clean architecture principles**.

```
/internal
  /auth
    - service.go
    - handler.go
    - model.go
  /subscription
    - service.go
    - handler.go
    - model.go
```

Each module:

*   Has its own models and services
*   Communicates with other modules via **interfaces**
*   Does not share database tables

### Deployment: Monolith with Feature Flags

One big binary, but smartly divided via configs:

```
enabled_modules:
  - auth
  - subscription
  - payment
```

> We deploy everything together, but **only run what’s needed** via flags (very useful in staging/testing environments).

### Why This Performed Better

### 1\. In-Memory Function Calls

In our microservices setup:

*   Every service call was a gRPC/HTTP call
*   Required retries, backoff, and resilience logic

In modular monolith:

*   Everything is in-process
*   Latency dropped significantly

### 2\. Single Database Transaction Across Modules

In microservices, distributed transactions are a pain. We had to use the **Saga pattern** to coordinate changes across modules.

In monolith:

```go
tx := db.Begin()
authRepo.WithTx(tx).CreateUser(user)
subscriptionRepo.WithTx(tx).CreateTrial(user.ID)
tx.Commit()
```

> **All in one transaction. No external orchestrator needed.**

### When Should You NOT Use Microservices?

*   When your **team size is under 100**
*   When you’re not operating in **multiple dev teams with full autonomy**
*   When you need **strong consistency**
*   When you don’t want to hire a **full-time platform/infra team**

### How to Transition

1.  **Keep microservice boundaries as modules**
2.  **Build a monorepo and share interfaces**
3.  **Avoid hard coupling to shared databases**
4.  **Use internal APIs via function calls**
5.  **Deploy as a single service with feature toggles**

### Final Thoughts

Microservices were built for **large organizations** with **complex teams** and **massive infra budgets**. For many startups and mid-size companies, they’re an **overkill**.

We replaced a 12-service setup with a single binary and scaled better, faster, and cheaper.

Stop blindly following architectural trends.

> Sometimes, **the best architecture is the one that’s boring, simple, and fast**.

### Bonus: Tools That Helped Us

*   **Go + Wire** for DI
*   **Fx / Uber Zap** for service lifecycle and logging
*   **GORM** with transaction-scoped repos
*   **GitHub Actions + Docker Compose** for single-unit CI/CD
*   **Flags + Configs** for module toggling