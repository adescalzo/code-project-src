```yaml
---
title: "Pattern: API Composition"
source: https://microservices.io/patterns/data/api-composition.html
date_published: unknown
date_captured: 2025-09-04T20:24:25.917Z
domain: microservices.io
author: Unknown
category: architecture
technologies: [Kong, Eventuate.io, Disqus, Twitter]
programming_languages: [JavaScript]
tags: [microservices, api-composition, service-collaboration, queries, distributed-data, architectural-pattern, database-per-service, cqrs, api-gateway, data-access]
key_concepts: [Microservice Architecture, API Composition pattern, Database per Service pattern, CQRS pattern, API Gateway, Saga pattern, Distributed Data Management, In-memory joins]
code_examples: false
difficulty_level: intermediate
summary: |
  The API Composition pattern addresses the challenge of implementing queries that span multiple services in a microservice architecture, particularly when using the Database per Service pattern. It proposes an API Composer component that invokes relevant services, gathers their data, and performs in-memory joins to construct the final composite query result. While offering a simple approach to cross-service queries, a potential drawback is the inefficiency of in-memory joins for very large datasets. This pattern is often implemented by an API Gateway and serves as an alternative to the CQRS pattern for complex query scenarios.
---
```

# Pattern: API Composition

# Pattern: API Composition

[pattern](/tags/pattern) [service-collaboration](/tags/service-collaboration) [implementing-queries](/tags/implementing-queries)

---

## Context

You have applied the [Microservices architecture pattern](../microservices.html) and the [Database per service pattern](database-per-service.html). As a result, it is no longer straightforward to implement queries that join data from multiple services.

## Problem

How to implement queries in a microservice architecture?

## Solution

Implement a query by defining an _API Composer_, which invokes the services that own the data and performs an in-memory join of the results.

![Diagram illustrating data flow and aggregation from multiple services to form a composite query result.](/i/data/ApiBasedQueryBigPicture.png)

![Diagram showing an API Composer invoking query methods on multiple Provider Services (A, B, C) to fulfill a composite query.](API_Composer_Diagram.png)

## Example

An [API Gateway](../apigateway.html) often does API composition.

## Resulting context

This pattern has the following benefits:

*   It is a simple way to query data in a microservice architecture

This pattern has the following drawbacks:

*   Some queries would result in inefficient, in-memory joins of large datasets.

## Related patterns

*   The [Database per Service pattern](database-per-service.html) creates the need for this pattern.
*   The [CQRS pattern](cqrs.html) is an alternative solution.

## Learn more

*   My book [Microservices patterns](/book) describes this pattern in a lot more detail.

---

[pattern](/tags/pattern) [service-collaboration](/tags/service-collaboration) [implementing-queries](/tags/implementing-queries)