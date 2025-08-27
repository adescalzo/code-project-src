```yaml
---
title: "Service Registry Pattern. In this article, we are going to talk… | by Mehmet Ozkaya | Design Microservices Architecture with Patterns & Principles | Medium"
source: https://medium.com/design-microservices-architecture-with-patterns/service-registry-pattern-75f9c4e50d09
date_published: 2022-06-04T07:25:25.387Z
date_captured: 2025-09-04T20:11:52.706Z
domain: medium.com
author: Mehmet Ozkaya
category: architecture
technologies: [Netflix Eureka, Spring Boot, Kubernetes, API Gateway]
programming_languages: [Java]
tags: [microservices, service-discovery, design-patterns, api-gateway, architecture, distributed-systems, container-orchestration, kubernetes, eureka, e-commerce]
key_concepts: [Service Registry Pattern, Microservices Architecture, Service Discovery, API Gateway, Container Orchestration, Service Aggregator Pattern, Design Principles, E-commerce Architecture]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Service Registry Pattern, a crucial design pattern in microservices architecture for enabling service discovery. It explains how API Gateways can locate backend microservices by querying a Service Registry, which acts as a database for service instances and their dynamic network locations. The author discusses open-source implementations like Netflix Eureka and notes that modern container orchestrators such as Kubernetes now handle service discovery automatically. The post emphasizes understanding the underlying pattern, illustrating its application within an e-commerce system design, and briefly mentions related concepts like the Service Aggregator Pattern.]
---
```

# Service Registry Pattern. In this article, we are going to talk… | by Mehmet Ozkaya | Design Microservices Architecture with Patterns & Principles | Medium

# Service Registry Pattern

In this article, we are going to talk about **Design Patterns** of Microservices architecture which is **The Service Registry Pattern**. As you know that we learned **practices** and **patterns** and add them into our **design toolbox**. And we will use these **pattern** and **practices** when **designing microservice architecture**.

![Diagram illustrating the Service Registry Pattern, showing a client interacting with a Gateway, which queries a Service Registry to find and call backend microservices (Product, ShoppingCart, Pricing, Order). Backend services register with the Service Registry.](https://miro.medium.com/v2/resize:fit:700/1*6XpOlHyp51gAyYxLtk51JQ.png)

By the end of the article, you will learn where and when to **apply Service Registry Pattern** into **Microservices Architecture** with designing **e-commerce application** system with following **KISS, YAGNI, SoC** and **SOLID principles**.

## Step by Step Design Architectures w/ Course

![Screenshot of a Udemy course titled "Design Microservices Architecture with Patterns & Principles".](https://miro.medium.com/v2/resize:fit:700/0*JAAtxc0pZZpiYFad.png)

[**I have just published a new course — Design Microservices Architecture with Patterns & Principles.**](https://www.udemy.com/course/design-microservices-architecture-with-patterns-principles/?couponCode=JUNE22)

In this course, we’re going to learn **how to Design Microservices Architecture** with using **Design Patterns, Principles** and the **Best Practices.** We will start with designing **Monolithic** to **Event-Driven Microservices** step by step and together using the right architecture design patterns and techniques.

## Service **Registry** Pattern

We can use **Microservice Discovery Patterns** and **Service Registry** for explaining this pattern. As the name suggests it will provide to **register** and **discover microservices** in the cluster.

As you know that, we have added **API Gateways** for **routing** the **traffic** with client and **internal microservices**. But **How** API GWs access the internal **backend microservices** ? **How API Gateways** know the **target ip** and **port** numbers for backend microservices ?

In that stage, the solution is applying **Service Registry Pattern**.

Because, in a Microservices based application, we have many instances of the several services on the different server. Due to **dynamically changes** in the **ips** and **port** numbers of these services, it is much more difficult to manage **service discovery** operations.

![Diagram illustrating the Service Registry Pattern, showing a client interacting with a Gateway, which queries a Service Registry to find and call backend microservices (Product, ShoppingCart, Pricing, Order). Backend services register with the Service Registry.](https://miro.medium.com/v2/resize:fit:700/1*6XpOlHyp51gAyYxLtk51JQ.png)

You can see the image and see how its solved, Basically there will be new service for **handling Service Registry**, so the client service **finds** the **location** of other service instances by querying a **Service Registry**.

And of course internal backend microservices should register to **Service Registry** service before. So we can say that The **Service Registry** is a database for microservice instances. When the client service need to access internal services, then it will query from **Service Registry** and access them.

There are some open source Service Registry implementations like **Netflix** provides a service discovery pattern called “**Netflix Eureka**” and available for **spring boot** **java** applications.

But nowadays, this pattern is **not require** to implement because of the container orchestrator systems automatically handle to service discovery operations itself. For example Kubernetes has **Service definitions** that basically perform all these task after definition of services on **K8s**. So its not a big deal now, but its good to know underlying pattern of this Microservice **Discovery Patterns** and **Service Registry** operations.

## Design — Service Aggregator Pattern — Service Registry Pattern — Microservices Communications Design patterns

You can find example implementation of **Service Registry Pattern** for our **e-commerce** architecture.

![Diagram showing a more complex e-commerce microservices architecture with Client Apps (Web, Mobile) interacting via an API Gateway, which then communicates with an Aggregator and various microservices (Catalog, Shopping Cart, Discount, Ordering), all leveraging a Service Registry. Each microservice has its own database.](https://miro.medium.com/v2/resize:fit:700/0*NCOl3MITCbhagoXT.png)

As you can see that we have applied Service Aggregator Pattern — Service **Registry Pattern** for our **e-commerce** architecture.

So we should **evolve our architecture** with applying new **microservices patterns** in order to **accommodate business adaptations** faster time-to-market and handle larger requests.