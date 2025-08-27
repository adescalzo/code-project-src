```yaml
---
title: D D D_ Aggregate
source: https://martinfowler.com/bliki/DDD_Aggregate.html
date_published: unknown
date_captured: 2025-08-21T10:53:22.596Z
domain: martinfowler.com
author: Martin Fowler
category: general
technologies: []
programming_languages: []
tags: [domain-driven-design, ddd, aggregate, design-pattern, software-architecture, object-collaboration]
key_concepts: [Domain-Driven Design (DDD), Aggregate, Aggregate Root, Domain Objects, Transaction Boundaries]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces the concept of an Aggregate, a fundamental pattern within Domain-Driven Design (DDD). It defines an aggregate as a cluster of domain objects treated as a single unit, exemplified by an order and its line-items. The piece explains the role of an aggregate root in maintaining the integrity of the aggregate and emphasizes that external references should only target this root. It also clarifies that aggregates are the basic unit for data storage and that transactions should not cross aggregate boundaries. Finally, the article distinguishes DDD aggregates from generic collection classes and other uses of the term "aggregate."
---
```

# D D D_ Aggregate

# DDD Aggregate

23 April 2013

Martin Fowler

Aggregate is a pattern in Domain-Driven Design. A DDD aggregate is a cluster of domain objects that can be treated as a single unit. An example may be an order and its line-items; these will be separate objects, but it's useful to treat the order (together with its line items) as a single aggregate.

An aggregate will have one of its component objects be the aggregate root. Any references from outside the aggregate should only go to the aggregate root. The root can thus ensure the integrity of the aggregate as a whole.

Aggregates are the basic element of transfer of data storage - you request to load or save whole aggregates. Transactions should not cross aggregate boundaries.

DDD Aggregates are sometimes confused with collection classes (lists, maps, etc). DDD aggregates are domain concepts (order, clinic visit, playlist), while collections are generic. An aggregate will often contain multiple collections, together with simple fields. The term “aggregate” is a common one, and is used in various different contexts (e.g. UML), in which case it does not refer to the same concept as a DDD aggregate.

For more details see the [Domain-Driven Design book](https://www.amazon.com/gp/product/0321125215/ref=as_li_tl?ie=UTF8&camp=1789&creative=9325&creativeASIN=0321125215&linkCode=as2&tag=martinfowlerc-20).