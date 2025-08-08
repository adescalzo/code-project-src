```yaml
---
title: Screaming Architecture
source: https://www.milanjovanovic.tech/blog/screaming-architecture?utm_source=newsletter&utm_medium=email&utm_campaign=tnw104
date_published: 2024-08-24T00:00:00.000Z
date_captured: 2025-08-06T18:26:25.591Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: architecture
technologies: [Syncfusion, Postman, ASP.NET Core, .NET]
programming_languages: [C#, PowerShell]
tags: [architecture, software-design, clean-architecture, vertical-slice-architecture, modular-monolith, use-cases, folder-structure, dotnet, software-architecture, domain-driven-design]
key_concepts: [Screaming Architecture, Use Case Driven Approach, Vertical Slice Architecture, Bounded Contexts, Clean Architecture, Modular Monolith Architecture, Cohesion, Coupling]
code_examples: false
difficulty_level: intermediate
summary: |
  The article introduces "Screaming Architecture," a concept by Robert Martin, advocating for organizing software systems around business use cases rather than technical concerns. It illustrates how a use-case driven folder structure improves system clarity and aligns development with business goals, contrasting it with traditional technical layouts. The author discusses the benefits, including improved cohesion and easier navigation, and connects screaming architecture to related concepts like Vertical Slice Architecture and Bounded Contexts. The piece emphasizes building systems that clearly communicate their purpose through their structure.
---
```

# Screaming Architecture

![Screaming Architecture blog cover](https://www.milanjovanovic.tech/blog-covers/mnw_104.png?imwidth=3840)

# Screaming Architecture

4 min read · August 24, 2024

Stop coding common elements! [**Syncfusion**](https://www.syncfusion.com/?utm_source=milanjovanovicnewsletter&utm_medium=text&utm_campaign=esee_milanjovanovic_newsletter) offers a developer's dream-over 1,800 prebuilt UI components for web, mobile, and desktop platforms. Focus on your app's unique features, not repetitive UI tasks. A free community license is also available for individual developers and startups. [**Try Syncfusion for free today**](https://www.syncfusion.com/?utm_source=milanjovanovicnewsletter&utm_medium=text&utm_campaign=esee_milanjovanovic_newsletter).

Collaborate directly with your external partners in Postman! You can use [**Partner Workspaces**](https://learning.postman.com/docs/collaborating-in-postman/using-workspaces/partner-workspaces/) to help your partners consume your team's APIs and work together on API projects. [**Check out Postman's Partner Workspaces**](https://learning.postman.com/docs/collaborating-in-postman/using-workspaces/partner-workspaces/), now available for Professional and Enterprise plans in Postman v11.

If you were to glance at the folder structure of your system, could you tell what the system is about? And here's a more interesting question. Could a new developer on your team easily understand what the system does based on the folder structure?

Your architecture should communicate what problems it solves. Organizing your system around use cases leads to a structure aligned with the business domain. This approach is called **screaming architecture**.

[Screaming architecture](https://blog.cleancoder.com/uncle-bob/2011/09/30/Screaming-Architecture.html) is a term coined by Robert Martin (Uncle Bob). He argues that a software system's structure should communicate what the system is about. He draws a parallel between looking at a blueprint for a building, where you can tell the purpose of the building based on the blueprint.

In this article, I want to show some practical examples and discuss the benefits of screaming architecture.

## A Use Case Driven Approach

A use case represents a specific interaction or task that a user wants to achieve within your system. It encapsulates the business logic required to fulfill that task. A use case is a high-level description of a user's goal. For example, "reserving an apartment" or "purchasing a ticket". It focuses on the _what_ of the system's behavior, not the _how_.

When you look at the folder structure and source code files of your system:

*   Do they scream: Apartment Booking System or Ticketing System?
*   Or do they scream ASP.NET Core?

Here's an example of a folder structure organized around technical concerns:

```powershell
📁 Api/
|__ 📁 Controllers
|__ 📁 Entities
|__ 📁 Exceptions
|__ 📁 Repositories
|__ 📁 Services
    |__ #️⃣ ApartmentService.cs
    |__ #️⃣ BookingService.cs
    |__ ...
|__ 📁 Models
```

Somewhere inside these folders, we'll find concrete classes that contain the system's behavior. You'll notice that the cohesion with this folder structure is low.

How does screaming architecture help?

A use case driven approach will place the system's use cases as the top-level concept. I also like to group related use cases into a top-level feature folder. Inside a use case folder, we may find technical concepts required to implement it.

[**Vertical slice architecture**](vertical-slice-architecture) also approaches this from a similar perspective.

```powershell
📁 Api/
|__ 📁 Apartments
    |__ 📁 ReserveApartment
    |__ ...
|__ 📁 Bookings
    |__ 📁 CancelBooking
    |__ ...
|__ 📁 Payments
|__ 📁 Reviews
|__ 📁 Disputes
|__ 📁 Invoicing
```

The use case driven folder structure helps us better understand user needs and aligns development efforts with business goals.

## Screaming Architecture Benefits

The benefits of organizing our system around use cases are:

*   Improved cohesion since related use cases are close together
*   High coupling for a single use case and its related use cases
*   Low coupling between unrelated use cases
*   Easier navigation through the solution

## Bounded Contexts and Vertical Slices

We have many techniques for discovering the high-level modules within our system. For example, we could use [event storming](https://www.eventstorming.com/) to explore the system's use cases. Domain exploration happens before we write a single line of code.

The next step is decomposing the larger problem domain into smaller sub-domains and later bounded contexts. This gives us loosely coupled high-level modules that we can translate into code.

![Diagram showing three interconnected bounded contexts: Ticketing, Attendance, and Events.](https://www.milanjovanovic.tech/blogs/mnw_104/bounded_contexts.png?imwidth=3840)

The overarching idea here is thinking about cohesion around functionalities. We want to organize our system so that the cohesion between the components is high. Bounded contexts, vertical slices, and screaming architecture are complementary concepts.

Here's a screaming architecture example for this system. Let's say the `Ticketing` module uses [**Clean Architecture**](clean-architecture-folder-structure) internally. But we can still organize the system around feature folders and use cases. An alternative approach could be organizing around [**vertical slices**](vertical-slice-architecture-structuring-vertical-slices), resulting in a less nested folder structure.

```powershell
📁 Modules/
|__ 📁 Attendance
    |__ ...
|__ 📁 Events
    |__ ...
|__ 📁 Ticketing
    |__ 📁 Application
        |__ 📁 Carts
            |__ 📁 AddItemToCart
            |__ 📁 ClearCart
            |__ 📁 GetCart
            |__ 📁 RemoveItemFromCart
        |__ 📁 Orders
            |__ 📁 SubmitOrder
            |__ 📁 CancelOrder
            |__ 📁 GetOrder
        |__ 📁 Payments
            |__ 📁 RefundPayment
        |__ ...
    |__ 📁 Domain
        |__ 📁 Customers
        |__ 📁 Orders
        |__ 📁 Payments
        |__ 📁 Tickets
        |__ ...
    |__ 📁 infrastructure
        |__ 📁 Authentication
        |__ 📁 Customers
        |__ 📁 Database
        |__ 📁 Orders
        |__ 📁 Payments
        |__ 📁 Tickets
        |__ ...
|__ 📁 Users
    |__ ...
```

The example above is a small part of the system I built inside of [**Modular Monolith Architecture**](/modular-monolith-architecture).

## Takeaway

**Screaming Architecture** isn't just a catchy phrase, it's an approach that can profoundly impact how you build software. By organizing your system around use cases, you align your codebase with the core business domain. Your system exists to solve the business domain problems.

Remember, the goal is to create a system that communicates its purpose through its structure. Embrace a use case-driven approach, break down complex domains into bounded contexts. Build a system that truly "screams" about the problems it solves.

If you want to explore these powerful ideas further, check out [**Pragmatic Clean Architecture**](/pragmatic-clean-architecture). I share my entire framework for building robust applications from the ground up and organizing the system around use cases.

That's all for today.

See you next week.

---

Whenever you're ready, there are 4 ways I can help you:

1.  [Pragmatic Clean Architecture:](/pragmatic-clean-architecture) Join 4,200+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [Modular Monolith Architecture:](/modular-monolith-architecture) Join 2,100+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [(NEW) Pragmatic REST APIs:](/pragmatic-rest-apis) Join 1,100+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
4.  [Patreon Community:](https://www.patreon.com/milanjovanovic) Join a community of 5,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Become a Better .NET Software Engineer

Join 70,000+ engineers who are improving their skills every Saturday morning.

![Thumbnail for a video or course titled "Screaming Architecture" with a blue and black geometric design.](https://www.milanjovanovic.tech/_next/static/media/mnw_104.9961917f.png?imwidth=3840)
![Cover image for the "Pragmatic Clean Architecture" course, featuring a stylized "PCA" logo.](https://www.milanjovanovic.tech/_next/static/media/cover.27333f2f.png?imwidth=384)
![Cover image for the "Modular Monolith Architecture" course, featuring a stylized "MMA" logo.](https://www.milanjovanovic.tech/_next/static/media/cover.31e11f05.png?imwidth=384)
![Cover image for the "Pragmatic REST APIs" course, featuring a stylized "PRA" logo.](https://www.milanjovanovic.tech/_next/static/media/cover_1.fc0deb78.png?imwidth=384)