```yaml
---
title: "The Strangler Fig Pattern in Microservices | Baeldung on Computer Science"
source: https://www.baeldung.com/cs/microservices-strangler-pattern
date_published: 2024-03-05T03:27:42.000Z
date_captured: 2025-09-04T20:32:39.904Z
domain: www.baeldung.com
author: Bruno Unna
category: architecture
technologies: []
programming_languages: []
tags: [strangler-fig-pattern, microservices, monolith-to-microservices, migration, software-architecture, design-patterns, refactoring, agile, system-modernization, facade-pattern]
key_concepts: [Strangler Fig Pattern, Monolithic architecture, Microservices architecture, Gradual replacement, Inventory of functionality, Safety net, Collaboration, Façade pattern, Iterative process]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Strangler Fig Pattern, a strategy for migrating monolithic systems to microservices by gradually replacing functionalities. It likens the process to a strangler fig growing around a host tree, eventually rendering the original structure obsolete. The pattern emphasizes creating an inventory of functionality, establishing a safety net of automated tests, fostering collaboration, and defining a façade to abstract new services. This iterative approach aims to reduce risk and provide flexibility during system modernization, leading to a more maintainable and stable architecture.]
---
```

# The Strangler Fig Pattern in Microservices | Baeldung on Computer Science

# The Strangler Fig Pattern in Microservices

.topAdContainer { overflow: hidden; height: 300px; display: flex; align-items: center; justify-content: right; border: 1px solid #e5e5e5; border-radius: 2px }

if (typeof window.freestar !== 'undefined' && typeof window.freestar.queue !== 'undefined') { freestar.config.enabled\_slots.push({ placementName: "baeldung\_top\_1", slotId: "baeldung\_top\_1" }); }

[![freestar](https://a.pub.network/core/imgs/fslogo-green.svg)](https://ads.freestar.com/?utm_campaign=branding&utm_medium=display&utm_source=baeldung.com&utm_content=baeldung_top_1)

Last updated: March 5, 2024

![](https://www.baeldung.com/wp-content/uploads/custom_avatars/bruno_unna_128x128.jpg)

Written by: [Bruno Unna](https://www.baeldung.com/cs/author/brunounna "Posts by Bruno Unna")

![](https://secure.gravatar.com/avatar/cbf63a957b4a567004d20f4b9bb5258224da5762f9fb50e665fb11bccfb8ce44?s=50&r=g)

Reviewed by: [Grzegorz Piwowarek](https://www.baeldung.com/cs/editor/grzegorz-author "Reviewed by Grzegorz Piwowarek")

*   [Software Architecture](https://www.baeldung.com/cs/category/software-architecture)

*   [Design Patterns](https://www.baeldung.com/cs/tag/design-patterns)
*   [Microservices](https://www.baeldung.com/cs/tag/microservices)

Baeldung Pro – CS – NPI EA (cat = Baeldung on Computer Science)

![announcement - icon](/wp-content/uploads/2022/04/announcement-icon.png)

Learn through the super-clean Baeldung Pro experience:

[\>> Membership and Baeldung Pro](https://www.baeldung.com/cs-baeldung-pro-NPI-EA-4-tenku).

No ads, dark-mode and 6 months free of IntelliJ Idea Ultimate to start with.

## 1\. Introduction[](#introduction)

In this tutorial, we’ll explore a typical pattern that can be used when migrating a monolithic system to microservices.

The [idea behind this pattern](https://martinfowler.com/bliki/StranglerFigApplication.html) is to emulate what the strangler fig does with its host tree: it grows around it, gradually strangling it, until at some point, the structure provided by the tree becomes irrelevant because it’s been replaced by the fig’s.

This pattern is about gradually replacing functionality, eventually rendering the monolithic system completely obsolete. **A gradual replacement is preferable to a full-rewrite in most situations** because it reduces the risk (each functionality can be tested in isolation and also in the context of the whole system), and it’s flexible (the replacement process, taking time, can be modified along the way, as new needs are identified).

An additional but significant advantage is that the pace at which the replacement happens can be adapted to the availability of resources since, at any given time, there is a fully operational system to serve the needs of its users.

## 2\. Write Down an Inventory of Functionality[](#write-down-an-inventory-of-functionality)

Naturally, to gradually replace functionality, we need to understand what that functionality is. This step must consider two perspectives: one from the system’s users (we can call this the external perspective) and another the result of inspecting the current architecture and code of the plan (this is an internal perspective).

[![freestar](https://a.pub.network/core/imgs/fslogo-green.svg)](https://ads.freestar.com/?utm_campaign=branding&utm_medium=display&utm_source=baeldung.com&utm_content=baeldung_leaderboard_mid_1)

In a non-trivial sense, these perspectives resemble what, during the development of code, separates integration tests (an end-to-end vision of the system, the consideration of non-functional aspects like behavior under load, throughput, and latency, resiliency, etc.) from unit tests (the developer’s vision of what could go wrong: validation of inputs, consistency of outputs, performance).

**The result of this step is a strategy**: we should be able to answer the following questions: What can be replaced next? What functional dependencies exist? Of the available possibilities, which is the most critical? What happens if we find unforeseen obstacles? And ultimately, what should be replaced next? When writing down this inventory, it’s convenient to use balanced criteria. Typically, the main factors will be minimizing risk, identifying opportunities, and a mentality that welcomes change:

![Diagram illustrating the inventory of functionality. The diagram shows a monolithic system on the left, transforming into a monolith on the right with identified functionalities (Functionality 1, Functionality 2, Functionality n) and an arrow indicating "Inventory of Functionality".](https://www.baeldung.com/wp-content/uploads/sites/4/2024/03/strangler-inventory.png)

An inherent component of the spirit of this approach is its agility. Among other things, that implies the process’s iterative nature. The inventory of functionality, once created, is not set in stone. It can be changed on successive iterations because of changes in priorities, a better understanding of the system, shifts in the availability of resources, and so on.

## 3\. Create a Safety Net[](#create-a-safety-net)

Following on the above description of creating an inventory of functionality, and although it’s not always possible, **we should aim to count on some automated mechanism to validate that each change we apply to the system doesn’t break it**. Ideally, we should routinely execute a suite of automated, functional tests (even if we need to invest in creating them) in an independent, secure environment.

Since the ideal situation is not always attainable, and especially because the very reason we’re applying the pattern is how difficult it can be to operate the monolithic system, alternative (even if sub-optimal) safety measures should be taken. One possibility is the execution of run-books by human beings using non-real data.

if (typeof window.freestar !== 'undefined' && typeof window.freestar.queue !== 'undefined') { freestar.config.enabled\_slots.push({ placementName: "baeldung\_leaderboard\_mid\_2", slotId: "baeldung\_leaderboard\_mid\_2" }); }

[![freestar](https://a.pub.network/core/imgs/fslogo-green.svg)](https://ads.freestar.com/?utm_campaign=branding&utm_medium=display&utm_source=baeldung.com&utm_content=baeldung_leaderboard_mid_2)

## 4\. Foster Collaboration[](#foster-collaboration)

In this approach, the inter-dependency of components is possibly even more critical than in others. This is due to the dual nature of that dependency: the system being replaced needs to depend on the new services, but the new services depend on each other. This forces the team (or teams) to focus on improved communication, shared prioritization, and great flexibility.

This is another aspect of the agile nature of the process. Transparency, horizontal collaboration structures, frequent but efficient interactions, and widely distributed documentation are all elements that must be fostered and explicitly taken care of for properly undertaking this type of effort.

At this point, **measures to guarantee collaboration should be established, documented, and shared**.

## 5\. Define a Façade[](#define-a-faade)

As much as possible, the monolith shouldn’t be aware of the changes around it. Once a piece of functionality has been identified (using the inventory that we described above), the old system should be able to invoke the replacement part transparently. To achieve that, we will represent that new functionality using a façade: an in-between component that allows the old system to use the new services without incurring heavy (and risky) modifications:

![Diagram with a facade defined. The diagram shows a monolithic system with identified functionalities. Functionality 2 is highlighted in blue, and a "Façade 2" component is introduced outside the monolith, connected to Functionality 2.](https://www.baeldung.com/wp-content/uploads/sites/4/2024/03/strangler-facade.png)

A wrapping façade has been defined around “functionality 2” (in blue). From this point onward, that’s the only way the monolith interacts with this functionality. From the old system’s point of view, this should be as simple as importing one library instead of another or slightly modifying the names of the methods (or functions or procedures). The job of the façade is to receive the invocations from the monolith, invoke the new components (applying any transformations required), receive the results, and give them back to the monolith as if nothing strange had happened.

[![freestar](https://a.pub.network/core/imgs/fslogo-green.svg)](https://ads.freestar.com/?utm_campaign=branding&utm_medium=display&utm_source=baeldung.com&utm_content=baeldung_leaderboard_mid_3)

## 6\. Implement the New Components[](#implement-the-new-components)

So far, most of our work has been preparation for this step. All that’s left to do is implement the new components, but when doing so, it’s important to remember all the previous steps. **It’s good to remember (and make sure that the team remembers) the importance of good prioritization, proper testing, excellent communication, and a properly defined façade,** **in addition to the usual quality-assurance criteria** (good documentation, unit testing, change management, team-enriching feedback, and so on):

![Diagram with the "microservice 2" implemented. The diagram depicts a monolithic system where Functionality 2 has been replaced by a "μ-service 2" (microservice 2) component, with the façade now pointing to this new microservice.](https://www.baeldung.com/wp-content/uploads/sites/4/2024/03/strangler-microservice.png)

The “microservice 2” has been implemented, and the façade has been changed to point to it. The rest of the monolith doesn’t notice the change. It’s not difficult to see how organizations might be tempted to introduce, remove or modify functionality during this process. This is, after all, an opportunity to review the whole monolithic system, something that probably would never happen otherwise. However, this temptation must be avoided, we should do our best to push back, explaining that by accepting functional modifications at this point we would be breaking the safety network that supports our migration work.

In exchange, we can offer the organization a more maintainable, stable, and reliable system once the migration is finished, one that can withstand the challenges of the new times more easily.

## 7\. Conclusion[](#conclusion)

**The strangler pattern is a battle-tested strategy allowing us to confidently perform ambitious monolithic system modernization.** By following its principles, we can make informed and intelligent decisions at any point during the process without investing in a detailed plan upfront, reducing the risk and yielding results quickly.

if (typeof window.freestar !== 'undefined' && typeof window.freestar.queue !== 'undefined') { freestar.config.enabled\_slots.push({ placementName: "baeldung\_leaderboard\_btf\_2", slotId: "baeldung\_leaderboard\_btf\_2" }); }