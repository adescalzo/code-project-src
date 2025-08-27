```yaml
---
title: "API Gateway Pattern. In this article, we are going to talk… | by Mehmet Ozkaya | Design Microservices Architecture with Patterns & Principles | Medium"
source: https://medium.com/design-microservices-architecture-with-patterns/api-gateway-pattern-8ed0ddfce9df
date_published: 2023-03-09T10:18:31.182Z
date_captured: 2025-09-04T20:10:20.980Z
domain: medium.com
author: Mehmet Ozkaya
category: architecture
technologies: [Ocelot, Consul, Eureka, Microservices Architecture]
programming_languages: []
tags: [microservices, api-gateway, design-patterns, distributed-systems, architecture, reverse-proxy, e-commerce, system-design, backend, web-api]
key_concepts: [API Gateway Pattern, Microservices Architecture, Reverse Proxy, Request Aggregation, Cross-Cutting Concerns, Backend-for-Frontend (BFF), Design Principles (KISS, YAGNI, SoC, SOLID), Single Point of Failure]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the API Gateway Pattern as a fundamental design pattern within Microservices Architecture. It explains how an API Gateway serves as a single entry point for client applications, abstracting the complexity of underlying microservices. Key functionalities discussed include reverse proxy routing, request aggregation, and handling cross-cutting concerns like authentication, authorization, and rate limiting. The author illustrates its application with an e-commerce system design, emphasizing the benefits of decoupling clients from internal services. The article also highlights the potential for a single API Gateway to become an anti-pattern and briefly introduces the Backend-for-Frontend (BFF) pattern as an evolution.]
---
```

# API Gateway Pattern. In this article, we are going to talk… | by Mehmet Ozkaya | Design Microservices Architecture with Patterns & Principles | Medium

# API Gateway Pattern

In this article, we are going to talk about **Design Patterns** of Microservices Architecture which is **The API Gateway Pattern**. As you know that we learned **practices** and **patterns** and add them into our **design toolbox**. And we will use these **pattern** and **practices** when **designing microservice architecture**.

![Architectural diagram showing client apps connecting to an API Gateway, which then routes requests to multiple microservices, each with its own database.](https://miro.medium.com/v2/resize:fit:700/1*gW4JrHTr86HnTrouQYLgJQ.png)

By the end of the article, you will learn where and when to **apply API Gateway Pattern** into **Microservices Architecture** with designing **e-commerce application** system with following **KISS, YAGNI, SoC** and **SOLID principles**.

## Step by Step Design Architectures w/ Course

![Promotional image for a Udemy course titled "Design Microservices Architecture with Patterns & Principles."](https://miro.medium.com/v2/resize:fit:700/0*9X9V-GI7RNtJCiZy.png)

[**I have just published a new course — Design Microservices Architecture with Patterns & Principles.**](https://www.udemy.com/course/design-microservices-architecture-with-patterns-principles/?couponCode=MARC23)

In this course, we’re going to learn **how to Design Microservices Architecture** with using **Design Patterns, Principles** and the **Best Practices.** We will start with designing **Monolithic** to **Event-Driven Microservices** step by step and together using the right architecture design patterns and techniques.

## **API Gateway** Pattern

The **API gateway pattern** is recommended if you want to design and build **complex large microservices**\-**based applications** with multiple client applications. The pattern is similar to the **facade pattern** from object-oriented design, but it is part of a distributed system **reverse proxy** or **gateway routing** for using as a synchronous communication model.

We said that It is similar to the **facade pattern** of Object-Oriented Design, so it provides a **single entry point** to the APIs with encapsulating the underlying system architecture.

The pattern provides a **reverse proxy** to **redirect** or **route requests** to your internal microservices endpoints. An API gateway provides a single endpoint for the client applications, and it **internally maps** the requests to internal microservices.

In summary, the **API gateway** locate between the client apps and the internal microservices. It is working as a **reverse proxy** and routing requests from clients to backend services. It is also provide **cross**\-**cutting concerns** like **authentication**, **SSL termination**, and cache.

![Architectural diagram showing client apps connecting to an API Gateway, which then routes requests to multiple microservices, each with its own database.](https://miro.medium.com/v2/resize:fit:700/1*gW4JrHTr86HnTrouQYLgJQ.png)
You can see the image that is collect client request in **single entrypoint** and route request to internal microservices.

So there are several client applications connect to **single API Gateway** in here. We should careful about this situation, because if we put here a single API Gateway, that means its possible to **single-point-of-failure risk** in here. If these client applications increase, or adding more logic to **business complexity** in API Gateway, it would be **anti-pattern.**

**API Gateway** service can be **growing** and **evolving** based on many different requirements from the client apps. That’s why the best practices is splitting the API Gateway in multiple services or multiple smaller API Gateways. We will see the **BFF-Backend-for-Frontend pattern** later.
In summary, we need to **careful** about using **single API Gateway,** it should be segregated based on **business boundaries** of the client applications and not be a **single aggregator** for all the internal microservices.

## Main Features of API Gateway Pattern

The API Gateway pattern provide **several benefits**. Since we accommodate the client request and route the internal microservices, we can handle some useful features into **API Gateway**. Lets see features.

![Diagram titled "Main Features of API Gateway Pattern" listing key functionalities such as Reverse Proxy, Request Aggregation, and Cross-Cutting Concerns.](https://miro.medium.com/v2/resize:fit:700/1*yKZ2J43avV51lyJnCInW_w.png)

### Reverse proxy or gateway routing

This is part of gateway routing pattern features. The API Gateway provides reverse proxy to redirect requests to the endpoints of the internal microservices. Usually, It is using **layer 7 routing** for HTTP requests for request redirections. This routing feature provides to decouple client applications from the internal microservices. So it is separating responsibilities on **network layer**. Another benefit is abstracting internal operations, API GW provide abstraction over the backend microservices, so even there is changes on backend microservices, it wont be affect to client applications. That means don’t need to update client applications when changing backend services.

### Requests aggregation

This is part of gateway aggregation pattern features. API Gateway can **aggregate multiple internal microservices** into a single client request. With this approach, the client application sends a single request to the API Gateway. After that API Gateway **dispatches several requests** to the internal microservices and then aggregates the results and sends everything back to the client application in 1 single response. The main benefit of this gateway aggregation pattern is to reduce chattiness communication between the client applications and the backend microservices.

### Cross-cutting concerns and gateway offloading

This is part of gateway offloading pattern features. Since API Gateway handle client request in centralized placed, its best practice to implement cross cutting functionality on the API Gateways.

The cross-cutting functionalities can be;

*   **Authentication** and **authorization**
*   **Service discovery** integration
*   Response **caching**
*   **Retry policies**, **circuit breaker**, and QoS
*   **Rate limiting** and throttling
*   **Load balancing**
*   Logging, tracing, correlation
*   Headers, query strings, and claims transformation
*   IP allowlisting

As you can see that we have understood the API Gateway Pattern and the benefits. Now we will design our **e-commerce architecture** with adding **API Gateway.**

## Design API Gateway — Microservices Communications Design Patterns

We are going to iterate our **e-commerce architecture** with adding API Gateway pattern.

![Architectural diagram showing client apps connecting to an API Gateway, which then routes requests to multiple microservices, each with its own database.](https://miro.medium.com/v2/resize:fit:700/1*XqLB8U0wGRBCm5jOSbu6lw.png)
As you can see that we have added to **single API Gateway** in our application.

This will handle the client requests and route the internal microservices, also **aggregate multiple internal microservices** into a single client request and performs **cross-cutting concerns** like **Authentication** and **authorization**, Rate limiting and throttling and so on..

We will continue to **evolve our architecture**, but please look at the current design and think how we can **improve** design ?

As we said that there are **several client applications** connect to **single API Gateway** in here. We should careful about this situation, because if we put here a single API Gateway, that means its possible to **single-point-of-failure** **risk** in here. If these client applications increase, or adding more logic to business complexity in API Gateway, it would be anti-pattern.

So we should solve this problem with **BFF-backends-for-frontend**s pattern.

---
### Additional Images from Content Analysis:

![Promotional image for a Udemy course titled "Design Microservices Architecture with Patterns & Principles."](https://miro.medium.com/v2/resize:fit:700/0*9X9V-GI7RNtJCiZy.png)

![Table titled "Ocelot Features" listing functionalities like Routing, Request Aggregation, Service Discovery with Consul & Eureka, Load Balancing, Authentication, Authorization, and Throttling.](https://miro.medium.com/v2/resize:fit:700/1*yKZ2J43avV51lyJnCInW_w.png)