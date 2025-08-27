```yaml
---
title: "Strangler Pattern in Micro-services | System Design - GeeksforGeeks"
source: https://www.geeksforgeeks.org/system-design/strangler-pattern-in-micro-services-system-design/
date_published: 2023-06-01T16:43:05.000Z
date_captured: 2025-09-04T20:32:31.838Z
domain: www.geeksforgeeks.org
author: Unknown
category: architecture
technologies: [API Gateway, CI/CD Pipeline]
programming_languages: [JavaScript]
tags: [strangler-pattern, microservices, monolith-to-microservices, system-design, architectural-pattern, migration, legacy-systems, api-gateway, ci-cd, refactoring]
key_concepts: [Strangler Pattern, monolithic architecture, microservices architecture, API Gateway, gradual migration, coexistence, data consistency, dependency management]
code_examples: false
difficulty_level: intermediate
summary: |
  [The Strangler pattern is an architectural approach for migrating from monolithic applications to microservices by incrementally replacing legacy components with new services. This process, named after a vine, allows the old and new systems to coexist, minimizing risks and disruptions associated with complete rewrites. Key steps involve transforming, coexisting, and eliminating monolithic parts, often leveraging an API Gateway for routing requests. While offering flexibility and controlled modernization, it introduces complexities in data consistency, network calls, and dependency management. It's a valuable strategy for modernizing complex legacy systems gradually.]
---
```

# Strangler Pattern in Micro-services | System Design - GeeksforGeeks

# Strangler Pattern in Micro-services | System Design

The Strangler pattern is an architectural approach employed during the migration from a monolithic application to a microservices-based architecture. It derives its name from the way a vine slowly strangles a tree, gradually replacing its growth. Similarly, the Strangler pattern involves replacing parts of a monolithic application with microservices over time.

In order to implement the Strangler pattern, we need to follow 3 steps that are as follows:

*   Transform
*   Co-exists
*   Eliminate

![Diagram illustrating the three phases of the Strangler Pattern: Transform (develop new component), Co-exist (new and old components exist with a router), and Eliminate (delete old component, only modern remains).](https://media.geeksforgeeks.org/wp-content/uploads/20230829114015/Transform-and-eliminate-pattern-(1).png)

## Use Cases for the Strangler Pattern:

The Strangler pattern is primarily used when migrating from a monolithic architecture to microservices. It proves beneficial in scenarios where complete system rewrites pose significant risks and disruptions. This pattern is particularly suitable for legacy systems with complex codebases that are challenging to refactor entirely.

## Features of the Strangler Pattern:

The Strangler pattern offers several essential features:

*   **Gradual Migration:** This pattern enables a step-by-step migration from a monolithic application to microservices. It allows organizations to replace specific functionality or modules incrementally.
*   **Coexistence:** During the migration process, the monolithic application and microservices coexist, ensuring uninterrupted system functionality.
*   **Strangling Behavior:** The Strangler pattern gradually replaces components or modules of the monolithic application with microservices, leading to the eventual replacement of the legacy system.

## Implementation of Strangler Pattern:

Consider an e-commerce application with a monolithic architecture. To migrate the order management functionality to microservices using the Strangler pattern, follow these implementation steps:

*   Identify the order management functionality within the monolithic application.
*   Create an order management microservice.
*   Configure the API gateway to route order management requests to the microservice.
*   Migrate specific functionalities from the monolithic application to the microservice.
*   Repeat steps 1-4 until the monolithic application is fully replaced.

![Diagram showing the Strangler Pattern in action, contrasting an independent service (Team A's Order Service with its own CI/CD pipeline) with a dependent service (Teams B, C, D's Pricing, Title, Currency Services, all dependent on Team C and a monolithic CI/CD pipeline).](https://media.geeksforgeeks.org/wp-content/uploads/20230829111904/strangler-pattern-in-action-(1).png)

Now let us come up with the advantages and disadvantages of the above pattern as follows:

## Advantages of Strangler Pattern

The Strangler pattern offers several benefits, such as:

1.  **Incremental Migration:** This pattern mitigates risks associated with complete system rewrites and minimizes disruptions by allowing a gradual migration process.
2.  **Flexibility:** Organizations can independently refactor and update specific parts of the system based on business priorities.
3.  **Coexistence:** The monolithic application and microservices coexist harmoniously, ensuring the system remains operational during the migration.

## Drawbacks of Strangler Pattern

1.  **Complexity:** The migration process can introduce complexity due to the coexistence and interaction between the monolithic application and microservices.
2.  **Data Consistency:** Synchronizing data between the monolithic application and microservices can pose challenges, requiring careful management to ensure consistency.
3.  **Increased Network Calls:** The introduction of microservices can lead to an increase in network calls, potentially impacting system performance and latency.
4.  **Dependency Management:** Managing dependencies between the remaining monolithic components and new microservices requires proper versioning and dependency strategies to avoid conflicts.

## Which components should be strangled or refactored first?

*   Playing it safe and choosing a straightforward component is not a bad choice if you are using the Strangler Pattern for the first time and are unfamiliar with this design pattern. This will make sure that before creating a complex component, you get real-world experience and familiarize yourself with the difficulties and best practices.
*   Starting with a component that has strong test coverage and little related technical debt can provide Teams a great deal of confidence during the migration process.
*   Start with a component that has scalability needs if there are any that are better suited for the cloud.
*   Start with a component that needs to be deployed much more frequently due to frequent business requirements if there is one. You won't need to frequently again deploy the full monolithic application. You can scale and deploy the application independently if you divide it into a distinct process.
*   You will face numerous obstacles on your journey to migrate to the cloud. Due to the fact that you are dealing with small components at once, the Strangler design pattern helps you to make this journey somewhat smooth and risk-free. When you intend to complete the move in bits and pieces, it is not a difficult task.
*   You can provide Business features faster by reducing the complexity of an application. You may scale your application using the rising load feature as well. It is significantly simpler to deploy microservices and can result in a much smoother transition from monoliths to microservices if there is an automated CI/CD pipeline.

## Conclusion:

The Strangler pattern offers a gradual and controlled approach to migrating from a monolithic architecture to microservices. By replacing components or modules incrementally, organizations can modernize their systems while minimizing risks and disruptions. While it presents certain complexities and considerations, the Strangler pattern remains a valuable tool for successful system design and migration.