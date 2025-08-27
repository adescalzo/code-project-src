```yaml
---
title: "Pattern: Strangler application"
source: https://microservices.io/patterns/refactoring/strangler-application.html#examples
date_published: unknown
date_captured: 2025-09-04T20:34:58.296Z
domain: microservices.io
author: Unknown
category: architecture
technologies: [Kong, Eventuate.io, CloudFoundry.com, Twitter, Disqus]
programming_languages: []
tags: [pattern, microservices, application-architecture, refactoring, monolith, legacy-systems, migration, strangler-pattern, adoption, modernization]
key_concepts: [Strangler Application pattern, Microservice Architecture, Monolithic application, Incremental modernization, Service decomposition, Distributed data management, Saga pattern, CQRS pattern]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Strangler Application pattern, a strategic approach for migrating legacy monolithic applications to a microservice architecture. The pattern advocates for incrementally developing a new "strangler" application that gradually envelops and replaces the functionality of the existing monolith. This new application consists of services that both replicate old features and introduce new ones, demonstrating the business value of microservices. The Strangler pattern enables a phased transition, allowing organizations to modernize their systems without a costly and risky complete rewrite.]
---
```

# Pattern: Strangler application

# Pattern: Strangler application

---

## Context

## Problem

How do you migrate a legacy monolithic application to a microservice architecture?

## Forces

## Solution

Modernize an application by incrementally developing a new (strangler) application around the legacy application. In this scenario, the strangler application has a [microservice architecture](/patterns/microservices.html).

![Diagram illustrating the Strangler Application pattern, showing a "strangler application" composed of services growing over time, while the "monolith" shrinks as its functionality is replaced. New features are added directly to the strangler application.](Strangling_the_monolith.png)

The strangler application consists of two types of services. First, there are services that implement functionality that previously resided in the monolith. Second, there are services that implement new features. The latter are particularly useful since they demonstrate to the business the value of using microservices.

## Examples

## Resulting context

## Related patterns

## Learn more

*   [Refactoring example](/refactoring)
*   [Chapter 13 of my book](/book) describes how to refactor a monolith to microservices