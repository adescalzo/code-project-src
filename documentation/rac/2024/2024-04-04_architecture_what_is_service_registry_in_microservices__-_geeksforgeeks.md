```yaml
---
title: What is Service Registry in Microservices? - GeeksforGeeks
source: https://www.geeksforgeeks.org/system-design/what-is-service-registry-in-microservices/
date_published: 2024-04-04T18:02:08.000Z
date_captured: 2025-09-04T20:11:51.499Z
domain: www.geeksforgeeks.org
author: Unknown
category: architecture
technologies: [Microservices, Service Registry, Load Balancer]
programming_languages: [JavaScript]
tags: [microservices, service-discovery, service-registry, distributed-systems, system-design, load-balancing, scalability, reliability]
key_concepts: [Service Registration, Service Lookup, Health Monitoring, Load Balancing, Dynamic Updates, Microservices Architecture, Distributed Systems]
code_examples: false
difficulty_level: intermediate
summary: |
  A Service Registry is a centralized database that stores and maintains information about available microservices and their locations. It is a critical component for service discovery, enabling services to register themselves and clients to dynamically look up service instances. Its core functions include service registration, service lookup, health monitoring, load balancing, and dynamic updates. This mechanism ensures scalable and resilient communication within a microservices architecture, promoting agility, scalability, and reliability in distributed systems.
---
```

# What is Service Registry in Microservices? - GeeksforGeeks

# What is Service Registry in Microservices?

A Service Registry serves as a centralized database or directory where information about available services and their locations is stored and maintained. It acts as a vital component of service discovery by providing a central point for service registration, lookup, and management.

Below is how a Service Registry typically works:

1.  **Service Registration**
    When a microservice instance starts up or becomes available, it registers itself with the Service Registry. This registration process includes providing metadata such as the service name, network location (IP address and port), health status, and possibly other attributes.

2.  **Service Lookup**
    Clients or other microservices that need to communicate with a particular service do not have prior knowledge of the service's location. Instead, they query the Service Registry to dynamically discover the available instances of the desired service. The Service Registry responds with the network location of one or more service instances.

3.  **Health Monitoring**
    Service Registries often include health-checking mechanisms to monitor the status of registered service instances. This allows the registry to detect and remove unhealthy or unavailable instances from its database, ensuring that clients only receive information about healthy and operational services.

4.  **Load Balancing**
    Some Service Registries incorporate load-balancing capabilities to distribute incoming requests among multiple instances of the same service. This helps in optimizing resource utilization, improving performance, and enhancing fault tolerance.

5.  **Dynamic Updates**
    Service instances may come and go dynamically due to scaling, deployment changes, or failures. The Service Registry continuously updates its records to reflect changes in the availability and health status of services, ensuring that clients receive up-to-date information.

Overall, a Service Registry serves as a central hub for managing service discovery and communication in a microservices architecture. It provides a scalable and resilient solution for dynamically locating and accessing services within a distributed system, promoting agility, scalability, and reliability.