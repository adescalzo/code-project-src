```yaml
---
title: Strangler Fig
source: https://martinfowler.com/bliki/StranglerFigApplication.html
date_published: unknown
date_captured: 2025-09-04T20:34:46.807Z
domain: martinfowler.com
author: Martin Fowler
category: general
technologies: []
programming_languages: []
tags: [application-architecture, legacy-modernization, strangler-fig, architectural-patterns, refactoring, systems-design, organizational-change, incremental-development, software-evolution, enterprise-architecture]
key_concepts: [Strangler Fig pattern, legacy modernization, incremental approach, transitional architecture, "Conway's Law", architectural seams, organizational change, system replacement]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces the Strangler Fig pattern, an analogy for gradually modernizing legacy software systems. It proposes an incremental approach to replacing monolithic systems, contrasting it with the often-failed strategy of wholesale replacement. The pattern involves building new functionalities alongside the old system, slowly migrating behavior until the legacy system is fully replaced. Key aspects include defining clear outcomes, breaking down problems into manageable parts, delivering new components, and fostering necessary organizational changes. The author emphasizes the value of transitional architecture and adapting development practices for sustainable software evolution.
---
```

# Strangler Fig

# Strangler Fig

22 August 2024

[![Photo of Martin Fowler](/mf.jpg "Photo of Martin Fowler")](/)

Martin Fowler

[application architecture](/tags/application%20architecture.html)

[legacy modernization](/tags/legacy%20modernization.html)

During a vacation in the rain forests of Queensland in 2001, we saw some [strangler figs](https://en.wikipedia.org/wiki/Strangler_fig). These are vines that germinate in a nook of a tree. As it grows, it draws nutrients from the host tree until it reaches the ground to grow roots and the canopy to get sunlight. It can then become self-sustaining, and its original host tree may die leaving the fig as an echo of its shape. This gradual process of replacing the host tree struck me as a striking analogy to the way I saw colleagues doing modernization of legacy software systems. A couple of years later I posted a brief blog post about this metaphor. While I've not used the term in my writing since then, it caught attention anyway, and the term “Strangler Fig” is now often used to describe a gradual approach to legacy modernization. 1

1: The original post was just entitled “Strangler Application”. This led to people often using “strangler” and forgetting the botanical origin of the name. As the term gained popularity I became concerned about this due to its connotations of violence. The difficulty was that once a term sticks, it's very hard to change. I decided to try a subtle change, replacing the title with “Strangler Fig Application” to emphasize its metaphorical routes. Fortunately the new title strangled the old one.

![A large strangler fig tree with numerous aerial roots hanging down, intertwining with the main trunk and branches, illustrating the natural phenomenon that inspired the software pattern.](images/stranglerApplication/11090068.jpg)

These days, software systems underpin much of human activity, but often don't do a great job of it. They were built over the course of decades, often with little investment and care to keeping them healthy. As business needs and technology platforms change, this software needs to change too, but these changes are often done by building patch upon patch, each patch making it harder to adapt to future changes. Eventually people realize that they can't patch any more, and need a wholesale modernization.

Faced with this choice, it is easy to think of the exercise as a simple replacement. We know what the old system does, so just build a new system that does exactly the same, but in a new and better technology that is easier to support changes. But we've seen this simple-sounding plan go down in flames most of the time. Replacing a serious IT system takes a long time, and the users can't wait for new features. Replacements seem easy to specify, but often it's hard to figure out the details of existing behavior. What's worse, much of that behavior is stuff that isn't really wanted, so building it is a waste.

The alternative that my colleagues and I prefer, is to do a gradual process of modernization. Like the fig, it begins with small additions, often new features, that are built on top of, yet separate to the legacy code base. As we do this we move bits of behavior from the legacy system into the new code base.

Ian Cartwright, Rob Horn, and James Lewis [defined four high-level activities](/articles/patterns-legacy-displacement/) that we need for this kind of incremental approach.

1.  Understand the outcomes you want to achieve
2.  Decide how to break the problem up into smaller parts
3.  Successfully deliver the parts
4.  Change the organization to allow this to happen on an ongoing basis

(The ordering here does _not_ imply a sequence.)

To embark on a modernization activity, we need to be crystal clear about our desired outcomes. Too often we see muddled objectives, with different groups wanting different things. We need to establish alignment on the key outcomes early, and then revisit regularly to reinforce the alignment and to investigate if there should be a shift in priority.

Ideally, software should consist of clear components that can be replaced independently. Legacy systems rarely exhibit this characteristic, so there's considerable work in figuring out how to break it down into manageable pieces, This will involve identifying [seams](/bliki/LegacySeam.html) that we can insert into the system to allow it to be split2. It's often useful to identify how a system supports multiple business needs and to extract individual needs into new components.

2: In a well-designed system, these seams would already exist. But such systems are unicorns.

Once we have seams to isolate a few components, we can start replacing them. Since these components are small, there isn't so much risk involved when we introduce the new software. Once working, the business can reap the value from these new components, allowing earlier return on the investment. We also learn more about how to replace the legacy system, and its consequences for the business, which helps us make better decisions as the modernization continues.

When doing this, people often balk at the necessity of building [transitional architecture](/articles/patterns-legacy-displacement/transitional-architecture.html) to allow the new and legacy system to coexist, code that will go away once the modernization is complete. While this may appear to be a waste, the reduced risk and earlier value from the gradual approach outweigh its costs.

Threaded through all of this is broader organizational change. Legacy systems become rigid and brittle because the design thinking and organizational processes that produced them built them that way. If there's no change in organizational culture and leadership, the new systems will end up in a similar mess. So we also need to introduce new development practices, including reorganizing the development organization and its connections with the wider business in recognition of [Conways Law](/bliki/ConwaysLaw.html).

Using a Strangler Fig approach to Legacy Modernization doesn't make the exercise easy. Replacing a software system that's deeply embedded with business processes is never going to be an easy task. What this approach does do is make both investment and returns occur gradually and visibly, allowing the organization to evolve its software and business process to better support the current environment, and (perhaps more importantly) evolve its way of doing that to something more sustainable for the future.

## Further Reading

[Patterns of Legacy Displacement](/articles/patterns-legacy-displacement/) goes into more detail on using an incremental approach to modernization and is in the process of developing a catalog of patterns to describe useful techniques.

My colleague Premanand Chandrasekaran describes [what he has learned](https://www.thoughtworks.com/insights/articles/embracing-strangler-fig-pattern-legacy-modernization-part-one) from using strangler figs. Alessio Ferri and Tom Coggrave talk about how they [introduced seams](/articles/uncovering-mainframe-seams.html) into a mainframe system.

If you are interested in reading the original post, take a look at [Original Strangler Fig Application](/bliki/OriginalStranglerFigApplication.html).