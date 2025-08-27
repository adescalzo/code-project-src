```yaml
---
title: "Clean Architecture: Standing on the shoulders of giants – @hgraca"
source: https://herbertograca.com/2017/09/28/clean-architecture-standing-on-the-shoulders-of-giants/
date_published: 2017-09-28T13:30:43.000Z
date_captured: 2025-09-04T13:48:30.848Z
domain: herbertograca.com
author: hgraca
category: architecture
technologies: [MVC, Domain-Driven Design, Dependency Injection Container]
programming_languages: []
tags: [clean-architecture, software-architecture, architectural-patterns, uncle-bob, hexagonal-architecture, onion-architecture, ebi-architecture, mvc, design-principles, testability]
key_concepts: [Clean Architecture, Hexagonal Architecture, Onion Architecture, EBI Architecture, Model-View-Controller, Dependency Inversion Principle, Testability, Separation of Concerns]
code_examples: false
difficulty_level: intermediate
summary: |
  [Robert C. Martin's Clean Architecture integrates well-known patterns like Hexagonal, Onion, and EBI to standardize application development. It emphasizes independence from tools and delivery mechanisms, along with isolated testability, by enforcing a strict dependency rule towards the core. The architecture defines layers such as Entities, Use Cases, and Interface Adapters, and outlines a control flow involving Controllers, Interactors, and Presenters. While not introducing entirely new concepts, Clean Architecture is crucial for clarifying and combining these principles into a cohesive framework. This approach aims to build maintainable and robust complex applications.]
---
```

# Clean Architecture: Standing on the shoulders of giants – @hgraca

# Clean Architecture: Standing on the shoulders of giants

This post is part of [The Software Architecture Chronicles](https://herbertograca.com/2017/07/03/the-software-architecture-chronicles/), a [series of posts about Software Architecture](https://herbertograca.com/category/development/series/software-architecture/). In them, I write about what I’ve learned on Software Architecture, how I think of it, and how I use that knowledge. The contents of this post might make more sense if you read the previous posts in this series.

Robert C. Martin (AKA Uncle Bob) published his ideas about *Clean Architecture* back in 2012, in a [post on his blog](https://blog.8thlight.com/uncle-bob/2012/08/13/the-clean-architecture.html), and lectured about it at a few conferences.

The Clean Architecture leverages well-known and not so well-known concepts, rules, and patterns, explaining how to fit them together, to propose a standardised way of building applications.

## Standing on the shoulders of EBI, Hexagonal and Onion Architectures

The core objectives behind Clean Architecture are the same as for Ports & Adapters (Hexagonal) and Onion Architectures:

*   Independence of tools;
*   Independence of delivery mechanisms;
*   Testability in isolation.

In the post about Clean Architecture was published, this was the diagram used to explain the global idea:

![Diagram of Clean Architecture showing concentric circles: Entities (innermost, yellow), Use Cases (red), Interface Adapters (green, including Controllers, Presenters, Gateways), and Frameworks & Drivers (light blue, including Web, UI, DB, Devices, External Interfaces). Arrows indicate dependencies pointing inwards. A smaller diagram illustrates the flow of control between Controller, Use Case Input Port, Use Case Interactor, Use Case Output Port, and Presenter.](https://herbertograca.com/wp-content/uploads/2017/04/cleanarchitecture-5c6d7ec787d447a81b708b73abba1680.jpg?w=1100)
Robert C. Martin 2012, [The Clean Architecture](https://blog.8thlight.com/uncle-bob/2012/08/13/the-clean-architecture.html)

As Uncle Bob himself says in his post, the diagram above is an attempt at integrating the most recent architecture ideas *into a single actionable idea*.

Let’s compare the Clean Architecture diagram with the diagrams used to explain Hexagonal Architecture and Onion Architecture, and see where they coincide:

![Diagram of Hexagonal Architecture, showing a central yellow hexagon labeled "Application" surrounded by various adapters (e.g., HTTP adapter, test adapter, app-to-app adapter, email adapter, DB adapter) that interact with external components like GUIs, databases, and other applications.](https://i0.wp.com/herbertograca.com/wp-content/uploads/2017/04/hexagonal_original.gif?w=567&h=371&ssl=1 "hexagonal_original")
Hexagonal Architecture

![Diagram of Onion Architecture, showing concentric layers from innermost to outermost: Application Core, Domain Model, Domain Services, Application Services, and finally User Interface, Tests, and Infrastructure. Arrows indicate dependencies pointing inwards, with external components like Web Service, File, and DB interacting with the outermost Infrastructure layer.](https://i0.wp.com/herbertograca.com/wp-content/uploads/2017/04/4ioq9.png?w=525&h=371&ssl=1 "4ioq9")
Onion Architecture

*   #### Externalisation of tools and delivery mechanisms

    Hexagonal Architecture focuses on externalising the tools and the delivery mechanisms from the application, using interfaces (ports) and adapters. This is also one of the core fundaments of Onion Architecture, as we can see by its diagram, the UI, the infrastructure and the tests are all in the outermost layer of the diagram. The Clean Architecture has exactly the same characteristic, having the UI, the web, the DB, etc, in the outermost layer. In the end, all application core code is framework/library independent.

*   #### Dependencies direction

    In the Hexagonal Architecture, we don’t have anything explicitly telling us the direction of the dependencies. Nevertheless, we can easily infer it: The Application has a port (an interface) which must be implemented or used by an adapter. So the Adapter depends on the interface, it depends on the application which is in the centre. What is outside depends on what is inside, the direction of the dependencies is towards the centre. In the Onion Architecture diagram, we also don’t have anything explicitly telling us the dependencies direction, however, in his second post, Jeffrey Palermo states very clearly that *all dependencies are toward the centre*. The Clean Architecture diagram, in turn, it’s quite explicit in pointing out that the dependencies direction is towards the centre. They all introduce the Dependency Inversion Principle at the architectural level. *Nothing in an inner circle can know anything at all about something in an outer circle.* Furthermore, *when we pass data across a boundary, it is always in the form that is most convenient for the inner circle.*

*   #### Layers

    The Hexagonal Architecture diagram only shows us two layers: Inside of the application and outside of the application. The Onion Architecture, on the other hand, brings to the mix the application layers identified by DDD: Application Services holding the use case logic; Domain Services encapsulating domain logic that does not belong in Entities nor Value Objects; and the Entities, Value Objects, etc.. When compared to the Onion Architecture, the Clean Architecture maintains the Application Services layer (Use Cases) and the Entities layer but it seems to forget about the Domain Services layer. However, reading Uncle Bob post we realise that he considers an Entity not only as and Entity in the DDD sense but as any Domain object: “*An entity can be an object with methods, or it can be a set of data structures and functions.*“. In reality, he merged those 2 innermost layers to simplify the diagram.

*   #### Testability in isolation

    In all three Architecture styles the rules they abide by provide them with insulation of the application and domain logic. This means that in all cases we can simply mock the external tools and delivery mechanisms and test the application code in insulation, without using any DB nor HTTP requests.

As we can see, Clean Architecture incorporates the rules of Hexagonal Architecture and Onion Architecture. So far, the Clean Architecture does not add anything new to the equation. However, in the bottom right corner of the Clean Architecture diagram, we can see a small extra diagram…

## Standing on the shoulders of MVC and EBI

The small extra diagram in the bottom right corner of the Clean Architecture diagram explains how the flow of control works. That small diagram does not give us much information, but the blog post explanations and the conference lectures given by Robert C. Martin expand on the subject.

![A detailed diagram illustrating the flow of control in Clean Architecture, showing an HTTP Request entering a Controller. The Controller interacts with an Interactor (via a Boundary interface), which in turn uses an Entity Gateway (via an Entity Gateway Interface) to find Entities and orchestrate interactions. The Interactor creates a Response Model and populates a Presenter, which then generates a ViewModel for the View. The View is then returned to the client.](https://herbertograca.com/wp-content/uploads/2017/04/cleanarchitecturedesign.png?w=1100)

In the diagram above, on the left side, we have the View and the Controller of MVC. Everything inside/between the black double lines represents the Model in MVC. That Model also represents the EBI Architecture (we can clearly see the Boundaries, the Interactor and the Entities), the “Application” in Hexagonal Architecture, the “Application Core” in the Onion Architecture, and the “Entities” and “Use Cases” layers in the Clean Architecture diagram above.

Following the control flow, we have an HTTP Request that reaches the Controller. The controller will then:

1.  Dismantle the Request;
2.  Create a Request Model with the relevant data;
3.  Execute a method in the Interactor (which was injected into the Controller using the Interactor’s interface, the Boundary), passing it the Request Model;
4.  The Interactor:
    1.  Uses the Entity Gateway Implementation (which was injected into the Interactor using the Entity Gateway Interface) to find the relevant Entities;
    2.  Orchestrates interactions between Entities;
    3.  Creates a Response Model with the data result of the Operation;
    4.  Populates the Presenter giving it the Response Model;
    5.  Returns the Presenter to the Controller;
5.  Uses the Presenter to generate a ViewModel;
6.  Binds the ViewModel to the View;
7.  Returns the View to the client.

The only thing here where I feel some friction and do differently in my projects is the usage of the “*Presenter*“. I rather have the Interactor return the data in some kind of DTO, as opposed to injecting an object that gets populated with data.

What I usually do is the actual MVP implementation, where the Controller has the responsibility of receiving and responding to the client.

## Conclusion

I would not say that the Clean Architecture is revolutionary because it does not actually bring a new groundbreaking concept or pattern to the table.

However, I would say that it is a work of the utmost importance:

*   It recovers somewhat forgotten concepts, rules, and patterns;
*   It clarifies useful and important concepts, rules and patterns;
*   It tells us how all these concepts, rules and patterns fit together to provide us with a standardised way to build complex applications with maintainability in mind.

When I think about Uncle Bob work with the Clean Architecture, It makes me think of Isaac Newton. Gravity had always been there, everybody knew that if we release an apple one meter above the ground, it will move towards the ground. The “only” thing Newton did was to publish a paper making that fact explicit\*. It was a “simple” thing to do, but it allowed people to reason about it and use that concrete idea as a foundation to other ideas.

In other words, I see Robert C. Martin is the Isaac Newton of software development! 🙂

## Resources

2012 – Robert C. Martin – [Clean Architecture (NDC 2012)](https://youtu.be/Nltqi7ODZTM)

2012 – Robert C. Martin – [The Clean Architecture](https://blog.8thlight.com/uncle-bob/2012/08/13/the-clean-architecture.html)

2012 – Benjamin Eberlei – [OOP Business Applications: Entity, Boundary, Interactor](https://beberlei.de/2012/08/13/oop_business_applications_entity_boundary_interactor.html)

2017 – Lieven Doclo – [A couple of thoughts on Clean Architecture](https://www.insaneprogramming.be/article/2017/02/14/thoughts-on-clean-architecture/)

2017 – Grzegorz Ziemoński – [Clean Architecture Is Screaming](https://dzone.com/articles/clean-architecture-is-screaming)

\* I know Sir Isaac Newton did more than that, but I just want to emphasize how important I consider the views of Robert C. Martin.