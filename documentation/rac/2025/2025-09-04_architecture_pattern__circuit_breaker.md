```yaml
---
title: "Pattern: Circuit Breaker"
source: https://microservices.io/patterns/reliability/circuit-breaker.html
date_published: unknown
date_captured: 2025-09-04T20:14:28.683Z
domain: microservices.io
author: Unknown
category: architecture
technologies: [Kong, Netflix Hystrix, Spring Framework, RestTemplate, Eventuate.io platform, Disqus, Twitter, Constant Contact, Google Groups, Manning LiveProject, GitHub]
programming_languages: [Scala, Java, JavaScript]
tags: [pattern, service-design, resilience, inter-service-communication, microservices, circuit-breaker, fault-tolerance, distributed-systems, api-gateway, microservice-chassis]
key_concepts: [Microservice Architecture, Circuit Breaker pattern, Resilience, Fault Tolerance, Inter-service Communication, API Gateway, Microservice Chassis, Server-side Discovery, Saga pattern, CQRS pattern, Distributed Data Management]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Circuit Breaker pattern, a crucial design pattern within microservice architectures. It addresses the challenge of preventing cascading failures when services synchronously invoke one another, which can lead to resource exhaustion and widespread system outages. The proposed solution involves using a proxy that acts like an electrical circuit breaker, tripping after a threshold of consecutive failures and immediately failing subsequent requests for a defined timeout period. After the timeout, it allows a limited number of test requests to determine if the remote service has recovered. An example implementation using Scala and Netflix Hystrix demonstrates the pattern's application, highlighting its benefits for service resilience while acknowledging the difficulty in choosing optimal timeout values.]
---
```

# Pattern: Circuit Breaker

#### [Microservice Architecture](/index.html)

**Supported by [Kong](https://konghq.com/)**

*   [Patterns](/patterns/index.html)
*   [Articles](/articles/index.html)
*   [Presentations](/presentations/index.html)
*   [Adopt](/adopt/index.html)
*   [Refactoring](/refactoring/index.html)
*   [Testing](/testing/index.html)

*   [About](/about.html)

# Pattern: Circuit Breaker

[pattern](/tags/pattern)   [service design](/tags/service design)   [resilience](/tags/resilience)   [inter-service communication](/tags/inter-service communication)  

---

## Context

You have applied the [Microservice architecture](../microservices.html). Services sometimes collaborate when handling requests. When one service synchronously invokes another there is always the possibility that the other service is unavailable or is exhibiting such high latency it is essentially unusable. Precious resources such as threads might be consumed in the caller while waiting for the other service to respond. This might lead to resource exhaustion, which would make the calling service unable to handle other requests. The failure of one service can potentially cascade to other services throughout the application.

## Problem

How to prevent a network or service failure from cascading to other services?

## Forces

## Solution

A service client should invoke a remote service via a proxy that functions in a similar fashion to an electrical circuit breaker. When the number of consecutive failures crosses a threshold, the circuit breaker trips, and for the duration of a timeout period all attempts to invoke the remote service will fail immediately. After the timeout expires the circuit breaker allows a limited number of test requests to pass through. If those requests succeed the circuit breaker resumes normal operation. Otherwise, if there is a failure the timeout period begins again.

## Example

`RegistrationServiceProxy` from the [Microservices Example application](https://github.com/cer/microservices-examples) is an example of a component, which is written in Scala, that uses a circuit breaker to handle failures when invoking a remote service.

```scala
@Component
class RegistrationServiceProxy @Autowired()(restTemplate: RestTemplate) extends RegistrationService {

  @Value("${user_registration_url}")
  var userRegistrationUrl: String = _

  @HystrixCommand(commandProperties=Array(new HystrixProperty(name="execution.isolation.thread.timeoutInMilliseconds", value="800")))
  override def registerUser(emailAddress: String, password: String): Either[RegistrationError, String] = {
    try {
      val response = restTemplate.postForEntity(userRegistrationUrl,
        RegistrationBackendRequest(emailAddress, password),
        classOf[RegistrationBackendResponse])
      response.getStatusCode match {
        case HttpStatus.OK =>
          Right(response.getBody.id)
      }
    } catch {
      case e: HttpClientErrorException if e.getStatusCode == HttpStatus.CONFLICT =>
        Left(DuplicateRegistrationError)
    }
  }
}
```

The `@HystrixCommand` arranges for calls to `registerUser()` to be executed using a circuit breaker.

The circuit breaker functionality is enabled using the `@EnableCircuitBreaker` annotation on the `UserRegistrationConfiguration` class.

```java
@EnableCircuitBreaker
class UserRegistrationConfiguration {
```

## Resulting Context

This pattern has the following benefits:

*   Services handle the failure of the services that they invoke

This pattern has the following issues:

*   It is challenging to choose timeout values without creating false positives or introducing excessive latency.

## Related patterns

*   The [Microservice Chassis](../microservice-chassis.html) might implement this pattern
*   An [API Gateway](../apigateway.html) will use this pattern to invoke services
*   A [Server-side discovery](../server-side-discovery.html) router might use this pattern to invoke services

## See also

*   [Netflix Hystrix](https://github.com/Netflix/Hystrix) is an example of a library that implements this pattern

---

[pattern](/tags/pattern)   [service design](/tags/service design)   [resilience](/tags/resilience)   [inter-service communication](/tags/inter-service communication)  

---

Copyright © 2025 Chris Richardson • All rights reserved • Supported by [Kong](https://konghq.com/).

#### About Microservices.io

![](https://gravatar.com/avatar/a290a8643359e2495e1c6312e662012f "Gravatar profile picture of Chris Richardson")

Microservices.io is brought to you by [Chris Richardson](/about.html). Experienced software architect, author of POJOs in Action, the creator of the original [CloudFoundry.com](http://CloudFoundry.com), and the author of Microservices patterns.

#### Microservices Patterns, 2nd edition

![](i/posts/mp2e-book-cover.png "Cover of 'Microservices Patterns, Second Edition' book by Chris Richardson, with a 'MEAP' (Manning Early Access Program) overlay.")

I am very excited to announce that the MEAP for the second edition of my book, Microservices Patterns is now available!

[Learn more](/post/architecture/2025/06/26/announcing-meap-microservices-patterns-2nd-edition.html)

#### ASK CHRIS

?

Got a question about microservices?

Fill in [this form](https://forms.gle/ppYDAF1JxHGec8Kn9). If I can, I'll write a blog post that answers your question.

#### NEED HELP?

![](i/posts/cxo-wondering.webp "A man in a suit, looking thoughtfully at a large digital display showing 'SYSTEM ARCHITECTURE' with complex diagrams and data, representing strategic architectural planning.")

I help organizations improve agility and competitiveness through better software architecture.

Learn more about my [consulting engagements](https://chrisrichardson.net/consulting.html), and [training workshops](https://chrisrichardson.net/training.html).

#### PREMIUM CONTENT

![](i/posts/premium-logo.png "A simple logo with 'Microservices.IO Premium' text.") Premium content now available for paid subscribers at [premium.microservices.io](https://premium.microservices.io).

#### MICROSERVICES WORKSHOPS

![](i/workshop-kata_small.jpg "A group of people around a table, engaged in a workshop, with one person at a whiteboard explaining concepts. Laptops are open.")

Chris teaches [comprehensive workshops](http://chrisrichardson.net/training.html) for architects and developers that will enable your organization use microservices effectively.

Avoid the pitfalls of adopting microservices and learn essential topics, such as service decomposition and design and how to refactor a monolith to microservices.

[Learn more](http://chrisrichardson.net/training.html)

#### Remote consulting session

![](i/posts/zoom-consulting.webp "A desktop computer screen showing a 'Microservices Architecture' presentation, with a video conference call (Zoom-like interface) on the right side, featuring multiple participants. This represents remote consulting or training.")

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

Read Chris Richardson's book: [![](i/Microservices-Patterns-Cover-published.png "Cover of 'Microservices Patterns' book by Chris Richardson.")](/book)

---

#### Example microservices applications

Want to see an example? Check out Chris Richardson's example applications. [See code](http://eventuate.io/exampleapps.html)

#### Virtual bootcamp: Distributed data patterns in a microservice architecture

![](i/Chris_Speaking_Mucon_2018_a.jpg "Chris Richardson speaking at a conference (Mucon 2018), with a microphone, in front of an audience.")

My virtual bootcamp, distributed data patterns in a microservice architecture, is now open for enrollment!

It covers the key distributed data management patterns including Saga, API Composition, and CQRS.

It consists of video lectures, code labs, and a weekly ask-me-anything video conference repeated in multiple timezones.

The regular price is $395/person but use coupon DWTWBMJI to sign up for $95 (valid until September 10th, 2025). There are deeper discounts for buying multiple seats.

[Learn more](https://chrisrichardson.net/virtual-bootcamp-distributed-data-management.html)

#### Learn how to create a service template and microservice chassis

Take a look at my [Manning LiveProject](/post/patterns/2022/03/15/service-template-chassis-live-project.html) that teaches you how to develop a service template and microservice chassis.

![](i/patterns/microservice-template-and-chassis/Microservice_chassis.png "A diagram illustrating components of a 'Microservice Chassis,' showing elements like 'Service Template,' 'Configuration,' 'Logging,' 'Metrics,' 'Health Check,' 'Circuit Breaker,' 'Service Discovery,' etc.")

[Signup for the newsletter](http://visitor.r20.constantcontact.com/d.jsp?llr=ula8akwab&p=oi&m=1123470377332&sit=l6ktajjkb&f=15d9bba9-b33d-491f-b874-73a41bba8a76)

For Email Marketing you can trust.

#### BUILD microservices

Ready to start using the microservice architecture?

#### Consulting services

[Engage Chris](http://www.chrisrichardson.net/consulting.html) to create a microservices adoption roadmap and help you define your microservice architecture,

---

#### The Eventuate platform

Use the [Eventuate.io platform](https://eventuate.io) to tackle distributed data management challenges in your microservices architecture.

[![](https://eventuate.io/i/logo.gif "The Eventuate.io platform logo.")](https://eventuate.io)

Eventuate is Chris's latest startup. It makes it easy to use the Saga pattern to manage transactions and the CQRS pattern to implement queries.

---

Join the [microservices google group](https://groups.google.com/forum/#!forum/microservices)

Please enable JavaScript to view the [comments powered by Disqus.](https://disqus.com/?ref_noscript)