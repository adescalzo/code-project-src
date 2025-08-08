```yaml
---
title: What Is a Modular Monolith?
source: https://www.milanjovanovic.tech/blog/what-is-a-modular-monolith?utm_source=LinkedIn&utm_medium=social&utm_campaign=24.03.2025
date_published: 2024-03-09T00:00:00.000Z
date_captured: 2025-08-06T17:47:37.918Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: ai_ml
technologies: [Blazor, Telerik UI for Blazor, .NET MAUI, WPF, WinForms, Shesha, .NET, ASP.NET Core]
programming_languages: [C#]
tags: [modular-monolith, software-architecture, monolith, microservices, dotnet, application-design, scalability, deployment, web-development, desktop-development]
key_concepts: [modular-monolith-architecture, microservices-architecture, monolith-first, distributed-systems, module-boundaries, loose-coupling, data-isolation]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Modular Monolith architecture, presenting it as a blend of traditional monolithic simplicity and microservices' flexibility. It defines a modular monolith as an application structured into independent, loosely coupled modules with well-defined boundaries. The author advocates for a "monolith first" approach, highlighting benefits such as simplified deployment, improved performance, and easier transaction management compared to distributed microservices. The article also explains how a well-structured modular monolith provides a clear and gradual path for transitioning to a microservices architecture when necessary, addressing common challenges associated with distributed systems.]
---
```

# What Is a Modular Monolith?

# What Is a Modular Monolith?

6 min read · March 09, 2024

Extend your existing [**Blazor Applications**](https://www.telerik.com/blazor-ui/blazor-hybrid?utm_medium=cpm&utm_source=milanjovanovic&utm_campaign=blazor-hybrid-general-awareness) to Desktop and Mobile. Bring the best of web UI to native client development. Embed Telerik UI for Blazor web components in .NET MAUI, WPF or WinForms applications. [**Download free trial today.**](https://www.telerik.com/blazor-ui/blazor-hybrid?utm_medium=cpm&utm_source=milanjovanovic&utm_campaign=blazor-hybrid-general-awareness)

Introducing [**Shesha**](https://shesha.io?utm_source=milan&utm_medium=email&utm_campaign=20240309), a brand new, open-source, low-code framework for .NET developers. Create business applications faster and with >80% less code! [**Learn more here.**](https://shesha.io?utm_source=milan&utm_medium=email&utm_campaign=20240309)

[Sponsor this newsletter](/sponsor-the-newsletter)

I've worked with many different software architectures over the years.

There's one that clearly stands out for its benefits: **Modular Monolith architecture**.

Modular monoliths blend the simplicity and robustness of traditional monolithic applications with the flexibility and scalability of microservices. I'm tempted to say they bring together the best of both worlds.

The modular monolith architecture allows you to work in a unified codebase with clearly defined boundaries and independent modules. You can have a high development velocity without the complexity of distributed systems.

Today, I'll introduce you to the modular monolith architecture and why you should know about it.

## What is a Modular Monolith?

A **modular monolith** is an architectural pattern that structures the application into independent modules or components with well-defined boundaries. The modules are split based on logical boundaries, grouping together related functionalities. This approach significantly improves the cohesion of the system.

The modules are loosely coupled, which further promotes modularity and separation of concerns. Modules communicate through a public API, and you can learn more about this in my article on [modular monolith communication patterns](modular-monolith-communication-patterns).

But what are the benefits of a modular design?

![Modular monolith.](/blogs/mnw_080/modular_monolith.png?imwidth=3840)

If we take the example of an apartment booking system illustrated above. During the holiday season, the system is expecting a traffic spike. The bookings and payments modules need to scale so they can be deployed independently. At the end of the holiday season, they can be merged back into a single deployment. Modular monoliths give you this kind of flexibility.

## Modular Architecture

Modular monoliths introduce a few important technical challenges that we will need to solve.

To achieve a modular architecture, the modules:

*   Must be independent and interchangeable
*   Must be able to provide the required functionality
*   Must have a well-defined interface exposed to other modules

Is it possible for a module to be completely independent? Not really. That would mean it's not integrated with other modules. We want loosely coupled modules and to keep the number of dependencies low. We can use a few techniques to keep the modules independent, and having good [data isolation](modular-monolith-data-isolation) is one example.

Another factor you need to consider is how strong the dependency is. If two modules are very "chatty", you might have incorrectly defined the boundaries. You should consider merging these modules together.

Remember, a module is a grouping of related functionalities accessed via a [well-defined interface](modular-monolith-communication-patterns).

Having a modular architecture allows you to easily extract modules into separate services.

## Monolith First

Microservices have become the most popular architectural pattern in recent years, and for good reason. Microservices offer many benefits like clearly defined service boundaries, independent deployments, independent scalability, and much more.

However, most teams would be better off starting with a monolith application.

A monolith is an architectural pattern where all components are deployed as a single physical deployment unit.

Here's an interesting quote from Martin Fowler:

> You shouldn't start a new project with microservices, even if you're sure your application will be big enough to make it worthwhile.

_— [Martin Fowler](https://martinfowler.com/bliki/MonolithFirst.html)_

And I wholeheartedly agree with this. Better yet, consider starting with a modular monolith.

Even Google is jumping on board the modular monolith trend in their recent research paper, [Towards Modern Development of Cloud Applications](https://dl.acm.org/doi/pdf/10.1145/3593856.3595909).

Here are the five main challenges Google identified with microservices:

*   **Performance** - The overhead of serializing data and sending it across the network has a noticeable impact on performance.
*   **Correctness** - It's difficult to reason about the correctness of a distributed system when there are many interactions between components.
*   **Management** - We have to manage multiple different applications, each with its release schedule.
*   **Frozen APIs** - Once an API is established, it becomes hard to change without breaking any existing API consumers.
*   **Development speed** - Making a change in one microservice may affect many other microservices, which requires carefully planning deployments.

When you factor in the complexity of distributed systems, starting with a modular monolith becomes increasingly compelling. I also recommend reading about the [fallacies of distributed computing](https://en.wikipedia.org/wiki/Fallacies_of_distributed_computing) if you're unfamiliar with them.

Well-defined, in-process components (modules) can be an excellent stepping stone to out-of-process components (services).

## Benefits of a Modular Monolith

Modular monoliths have many benefits. So, I want to highlight a few that I consider important:

*   **Simplified deployment** - Unlike microservices, which require complex deployment strategies, a modular monolith can be deployed as a single unit.
*   **Improved performance** - Communication between modules occurs in-process. This means that there's no network latency or data serialization/deserialization overhead.
*   **Enhanced development velocity** - There's a single codebase to manage, simplifying debugging and the overall development experience.
*   **Easier transaction management** - Managing transactions in a distributed system is very challenging. Modular monoliths simplify this since modules can share the same database.
*   **Lower operational complexity** - Modular monoliths reduce the operational overhead that comes with managing and deploying a distributed microservices system.
*   **Easier transition to Microservices** - A well-structured modular monolith offers a clear path to a microservices architecture. You can gradually [extract modules into separate services](monolith-to-microservices-how-a-modular-monolith-helps) when the need arises.

## Modular Monolith vs Microservices

The biggest difference between modular monoliths and microservices is how they're deployed. Microservices elevate the logical boundaries inside a modular monolith into physical boundaries.

Microservices give you a clear strategy for modularity and decomposing the bounded contexts. But, you can also achieve this without building a distributed system. The problem is people end up using microservices to enforce code boundaries.

![Modular monolith vs. microservices.](/blogs/mnw_080/modular_monolith_vs_microservices.png?imwidth=3840)

Instead, you can build a modular monolith to get most of the same benefits. Modular monoliths give you high cohesion, low coupling, data encapsulation, focus on business functionalities, and more.

Microservices give you all that, plus independent deployments, independent scalability, and the ability to use different technology stacks per service.

> Choose microservices for the benefits, not because your monolithic codebase is a mess.

_— [Simon Brown](https://twitter.com/simonbrown)_

## Next Steps

Modular monoliths offer a compelling way to structure applications. They balance the benefits of well-organized code, scalability potential, and a smooth path for transitioning to microservices if needed. If you want to improve the maintainability and adaptability of your software, consider exploring modular monoliths.

Want to dive deeper into modular monoliths? Check out these resources:

*   [Modular Monolith Communication Patterns](modular-monolith-communication-patterns)
*   [Modular Monolith Data Isolation](modular-monolith-data-isolation)
*   [Monolith to Microservices: How a Modular Monolith Helps](monolith-to-microservices-how-a-modular-monolith-helps)
*   [Modular Monoliths: How To Build One & Lessons Learned](https://youtu.be/Xo3rsiZYsJQ)
*   [How to Structure a Modular Monolith Project in .NET](https://youtu.be/z3piPJ7x4WU)
*   [Getting Started with Modular Monoliths in .NET](https://youtu.be/5dilYMii9T4)
*   [Modular Monolith Architecture course](/modular-monolith-architecture)

That's all for today. Stay awesome, and I'll see you next week.