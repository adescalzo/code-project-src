```yaml
---
title: "Pattern: Sidecar"
source: https://microservices.io/patterns/deployment/sidecar.html
date_published: unknown
date_captured: 2025-09-04T20:24:39.532Z
domain: microservices.io
author: Unknown
category: architecture
technologies: [Kong, CloudFoundry.com, Eventuate.io, Disqus, Twitter, Manning, Constant Contact]
programming_languages: [JavaScript]
tags: [microservices, architecture, patterns, deployment, sidecar, distributed-systems, consulting, training, books, event-driven]
key_concepts: [Microservice architecture pattern, Sidecar pattern, cross-cutting-concerns, distributed-data-management, Saga pattern, CQRS, service-template, monolith-to-microservices-refactoring]
code_examples: false
difficulty_level: intermediate
summary: |
  This content introduces the Sidecar pattern, a deployment strategy in microservice architecture for handling cross-cutting concerns in a separate process or container. It is part of a broader collection of resources by Chris Richardson, including books, workshops, and consulting services focused on microservices. The site offers comprehensive learning materials on microservice patterns, distributed data management, and strategies for adopting and refactoring to microservices. These resources are supported by platforms like Kong and Eventuate.io, aiming to help organizations improve agility and competitiveness through better software architecture.
---
```

# Pattern: Sidecar

#### Microservice Architecture

**Supported by [Kong](https://konghq.com/)**

*   [Patterns](/patterns/index.html)
*   [Articles](/articles/index.html)
*   [Presentations](/presentations/index.html)
*   [Adopt](/adopt/index.html)
*   [Refactoring](/refactoring/index.html)
*   [Testing](/testing/index.html)

*   [About](/about.html)

# Pattern: Sidecar

[pattern](/tags/pattern)   [deployment](/tags/deployment)    

---

## Context

You have applied the [Microservice architecture pattern](/patterns/microservices.html) and architected your system as a set of services.

## Problem

## Forces

## Solution

Implement cross-cutting concerns in a sidecar process or container that runs alongside the service instance.

## Examples

## Resulting context

## Related patterns

---

[pattern](/tags/pattern)   [deployment](/tags/deployment)    

---

Copyright © 2025 Chris Richardson • All rights reserved • Supported by [Kong](https://konghq.com/).

#### About Microservices.io

![Gravatar of Chris Richardson, the author of Microservices.io](https://gravatar.com/avatar/a290a8643359e2495e1c6312e662012f)

Microservices.io is brought to you by [Chris Richardson](/about.html). Experienced software architect, author of POJOs in Action, the creator of the original [CloudFoundry.com](http://CloudFoundry.com), and the author of Microservices patterns.

#### Microservices Patterns, 2nd edition

![Cover of "Microservices Patterns, Second Edition" by Chris Richardson, with a "MEAP" (Manning Early Access Program) overlay.](/i/posts/mp2e-book-cover.png)

I am very excited to announce that the MEAP for the second edition of my book, Microservices Patterns is now available!

[Learn more](/post/architecture/2025/06/26/announcing-meap-microservices-patterns-2nd-edition.html)

#### ASK CHRIS

?

Got a question about microservices?

Fill in [this form](https://forms.gle/ppYDAF1JxHGec8Kn9). If I can, I'll write a blog post that answers your question.

#### NEED HELP?

![An image of a man in a suit looking thoughtfully at a large, futuristic display showing "SYSTEM ARCHITECTURE" with interconnected nodes and data, representing architectural design or assessment.](/i/posts/cxo-wondering.webp)

I help organizations improve agility and competitiveness through better software architecture.

Learn more about my [consulting engagements](https://chrisrichardson.net/consulting.html), and [training workshops](https://chrisrichardson.net/training.html).

#### PREMIUM CONTENT

![A simple logo with the text "Microservices.IO Premium" on a white background with a jagged black border.](/i/posts/premium-logo.png) Premium content now available for paid subscribers at [premium.microservices.io](https://premium.microservices.io).

#### MICROSERVICES WORKSHOPS

![A group of people around a table in a workshop setting, with one person standing at a whiteboard explaining concepts, and others using laptops, illustrating a training session.](/i/workshop-kata_small.jpg)

Chris teaches [comprehensive workshops](http://chrisrichardson.net/training.html) for architects and developers that will enable your organization use microservices effectively.

Avoid the pitfalls of adopting microservices and learn essential topics, such as service decomposition and design and how to refactor a monolith to microservices.

[Learn more](http://chrisrichardson.net/training.html)

#### Remote consulting session

![A computer monitor displaying a "Microservices Architecture" presentation with various diagrams and best practices, alongside a smartphone showing a video conference call with multiple participants, representing a virtual bootcamp or consulting session.](/i/posts/zoom-consulting.webp)

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

Read Chris Richardson's book: ![Cover of "Microservices Patterns" by Chris Richardson, showing a historical figure in colorful attire.](/i/Microservices-Patterns-Cover-published.png)

---

#### Example microservices applications

Want to see an example? Check out Chris Richardson's example applications. [See code](http://eventuate.io/exampleapps.html)

#### Virtual bootcamp: Distributed data patterns in a microservice architecture

![A photo of Chris Richardson speaking at a conference (Mucon 2018), standing on stage with a microphone.](/i/Chris_Speaking_Mucon_2018_a.jpg)

My virtual bootcamp, distributed data patterns in a microservice architecture, is now open for enrollment!

It covers the key distributed data management patterns including Saga, API Composition, and CQRS.

It consists of video lectures, code labs, and a weekly ask-me-anything video conference repeated in multiple timezones.

The regular price is $395/person but use coupon DWTWBMJI to sign up for $95 (valid until September 10th, 2025). There are deeper discounts for buying multiple seats.

[Learn more](https://chrisrichardson.net/virtual-bootcamp-distributed-data-management.html)

#### Learn how to create a service template and microservice chassis

Take a look at my [Manning LiveProject](/post/patterns/2022/03/15/service-template-chassis-live-project.html) that teaches you how to develop a service template and microservice chassis.

![A diagram illustrating a "Microservice Chassis" with components like logging, metrics, configuration, and health checks, connected to a "Service Template".](/i/patterns/microservice-template-and-chassis/Microservice_chassis.png)

[Signup for the newsletter](http://visitor.r20.constantcontact.com/d.jsp?llr=ula8akwab&p=oi&m=1123470377332&sit=l6ktajjkb&f=15d9bba9-b33d-491f-b874-73a41bba8a76)

For Email Marketing you can trust.

#### BUILD microservices

Ready to start using the microservice architecture?

#### Consulting services

[Engage Chris](http://www.chrisrichardson.net/consulting.html) to create a microservices adoption roadmap and help you define your microservice architecture,

---

#### The Eventuate platform

Use the [Eventuate.io platform](https://eventuate.io) to tackle distributed data management challenges in your microservices architecture.

![A simple logo for "Eventuate.io" with a stylized 'E'.](https://eventuate.io/i/logo.gif)

Eventuate is Chris's latest startup. It makes it easy to use the Saga pattern to manage transactions and the CQRS pattern to implement queries.

---

Join the [microservices google group](https://groups.google.com/forum/#!forum/microservices)

Please enable JavaScript to view the [comments powered by Disqus.](https://disqus.com/?ref_noscript)