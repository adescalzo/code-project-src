```yaml
---
title: "Pattern: Externalized configuration"
source: https://microservices.io/patterns/externalized-configuration.html
date_published: unknown
date_captured: 2025-09-04T20:29:18.838Z
domain: microservices.io
author: Unknown
category: architecture
technologies: [Kong, Spring Boot, Docker, Eureka, RestTemplate]
programming_languages: [Scala, SQL]
tags: [microservices, configuration, pattern, service-design, environment-variables, spring-boot, docker-compose, data-access, deployment, architecture]
key_concepts: [Externalized Configuration Pattern, Microservice Architecture, Service Registry, Client-side Discovery, Server-side Service Discovery, Infrastructure Services, Distributed Data Management, Microservice Chassis]
code_examples: false
difficulty_level: intermediate
summary: |
  This article details the Externalized Configuration pattern, a crucial aspect of microservice architecture. It addresses the challenge of deploying services across various environments without code modification by advocating for externalizing all configuration data, such as database credentials and network locations. Services are designed to read this configuration from external sources like OS environment variables upon startup. The content provides practical examples using Spring Boot and Docker Compose, illustrating how to implement this pattern. While beneficial for environment independence, the pattern also highlights the need to ensure configuration consistency during deployment.
---
```

# Pattern: Externalized configuration

# Pattern: Externalized configuration

---

## Context

An application typically uses one or more infrastructure and 3rd party services. Examples of infrastructure services include: a [Service registry](service-registry.html), a message broker and a database server. Examples of 3rd party services include: payment processing, email and messaging, etc.

## Problem

How to enable a service to run in multiple environments without modification?

## Forces

*   A service must be provided with configuration data that tells it how to connect to the external/3rd party services. For example, the database network location and credentials
*   A service must run in multiple environments - dev, test, qa, staging, production - without modification and/or recompilation
*   Different environments have different instances of the external/3rd party services, e.g. QA database vs. production database, test credit card processing account vs. production credit card processing account

## Solution

Externalize all application configuration including the database credentials and network location. On startup, a service reads the configuration from an external source, e.g. OS environment variables, etc.

## Examples

[Spring Boot externalized configuration](https://docs.spring.io/spring-boot/docs/current/reference/html/boot-features-external-config.html) reads values from a variety of sources including operating system environment variables, property files and command line arguments. These values are available within the Spring application context.

`RegistrationServiceProxy` from the [Microservices Example application](https://github.com/cer/microservices-examples) is an example of a component, which is written in Scala, is configured with the variable `user_registration_url`:

```
@Component
class RegistrationServiceProxy @Autowired()(restTemplate: RestTemplate) extends RegistrationService {

  @Value("${user_registration_url}")
  var userRegistrationUrl: String = _
```

The `docker-compose.yml` file supplies its value as an operating system environment variable:

```
web:
  image: sb_web
  ports:
    - "8080:8080"
  links:
    - eureka
  environment:
    USER_REGISTRATION_URL: http://REGISTRATION-SERVICE/user
```

`REGISTRATION-SERVICE` is the logical name of the service. It is resolved using [Client-side discovery](client-side-discovery.html).

## Resulting Context

This pattern has the following benefits:

*   The application runs in multiple environments without modification and/or recompilation

There are the following issues with this pattern:

*   How to ensure that when an application is deployed the supplied configuration matches what is expected?

## Related patterns

*   The service discovery patterns, [Server-side service discovery](server-side-discovery.html) and [Client-side service discovery](client-side-discovery.html), solve the related problem of how a service knows the network location of other application services

---

#### Microservices Patterns, 2nd edition

![Book cover for "Microservices Patterns Second Edition" by Chris Richardson, with a "MEAP" (Manning Early Access Program) overlay.](mp2e-book-cover.png)

I am very excited to announce that the MEAP for the second edition of my book, Microservices Patterns is now available!

[Learn more](/post/architecture/2025/06/26/announcing-meap-microservices-patterns-2nd-edition.html)

#### MICROSERVICES WORKSHOPS

![A group of people around a table in a workshop setting, with one person at a whiteboard, suggesting a training or collaborative session.](workshop-kata_small.jpg)

Chris teaches [comprehensive workshops](http://chrisrichardson.net/training.html) for architects and developers that will enable your organization use microservices effectively.

Avoid the pitfalls of adopting microservices and learn essential topics, such as service decomposition and design and how to refactor a monolith to microservices.

[Learn more](http://chrisrichardson.net/training.html)

#### Remote consulting session

![A person in a suit looking thoughtfully, with a small "zoom-consulting" label, implying a remote consulting session.](zoom-consulting.webp)

Got a specific microservice architecture-related question? For example:

*   Wondering whether your organization should adopt microservices?
*   Want to know how to migrate your monolith to microservices?
*   Facing a tricky microservice architecture design problem?

Consider signing up for a [two hour, highly focussed, consulting session.](https://chrisrichardson.net/consulting-office-hours.html)

#### Get the book: Microservices Patterns

Read Chris Richardson's book: ![Book cover for "Microservices Patterns" by Chris Richardson.](Microservices-Patterns-Cover-published.png)

---

#### Virtual bootcamp: Distributed data patterns in a microservice architecture

![Chris Richardson speaking at a conference, with a microphone, in front of a screen displaying "Distributed Data Patterns in a Microservice Architecture".](Chris_Speaking_Mucon_2018_a.jpg)

My virtual bootcamp, distributed data patterns in a microservice architecture, is now open for enrollment!

It covers the key distributed data management patterns including Saga, API Composition, and CQRS.

It consists of video lectures, code labs, and a weekly ask-me-anything video conference repeated in multiple timezones.

The regular price is $395/person but use coupon DWTWBMJI to sign up for $95 (valid until September 10th, 2025). There are deeper discounts for buying multiple seats.

[Learn more](https://chrisrichardson.net/virtual-bootcamp-distributed-data-management.html)

#### Learn how to create a service template and microservice chassis

Take a look at my [Manning LiveProject](/post/patterns/2022/03/15/service-template-chassis-live-project.html) that teaches you how to develop a service template and microservice chassis.

![A diagram illustrating a "Microservice Chassis" concept, showing common components like logging, metrics, health checks, configuration, and service discovery.](Microservice_chassis.png)