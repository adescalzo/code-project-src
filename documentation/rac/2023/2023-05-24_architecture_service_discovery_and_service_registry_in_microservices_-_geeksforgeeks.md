```yaml
---
title: Service Discovery and Service Registry in Microservices - GeeksforGeeks
source: https://www.geeksforgeeks.org/java/service-discovery-and-service-registry-in-microservices/
date_published: 2023-05-24T13:26:30.000Z
date_captured: 2025-09-04T20:12:17.490Z
domain: www.geeksforgeeks.org
author: Unknown
category: architecture
technologies: [Spring Boot, Spring Cloud, REST API, Netflix Eureka, Zookeeper, Consul, NGINX, AWS ELB, Load Balancer]
programming_languages: [Java, JavaScript]
tags: [microservices, service-discovery, service-registry, spring-cloud, distributed-systems, load-balancing, rest-api, architecture, java, networking]
key_concepts: [Microservices Architecture, Service Discovery, Service Registry, Client-Side Service Discovery, Server-Side Service Discovery, Load Balancing, Monolithic Architecture, Distributed Systems]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the concepts of Service Discovery and Service Registry within a microservices architecture. It explains the challenges of managing dynamic network locations (IP addresses and port numbers) for numerous distributed services, contrasting it with monolithic applications. The solution involves a central Discovery Service where microservices register their instances, forming a Service Registry. This mechanism allows other services and load balancers to efficiently locate and communicate with specific service instances. The article also distinguishes between client-side and server-side service discovery, providing examples for each type.]
---
```

# Service Discovery and Service Registry in Microservices - GeeksforGeeks

# Service Discovery and Service Registry in Microservices

Microservices are small, loosely coupled distributed services. Microservices architecture evolved as a solution to the scalability, independently deployable, and innovation challenges with Monolithic Architecture. It provides us to take a big application and break it into efficiently manageable small components with some specified responsibilities. It is considered the building block of modern applications. Before understanding what Service Discovery is, let's understand the need for **Service Discovery** in Microservices.

### What's the Problem/Challenge?

Imagine you are writing microservices. Your company has adopted the microservices architecture, and you have Address Service, Employee Service, Course Service, Student Service, etc. You have many Spring Boot applications, and all these applications have been deployed into many different servers, potentially thousands of applications. Right now, if Course Service wants to connect to Address Service, or Student Service wants to connect to Course Service to get some course-related data, how will these servers communicate with each other? We will simply make a [**REST**](https://www.geeksforgeeks.org/node-js/rest-api-introduction/) call, and all these servers will communicate with each other using the REST API. But the real challenging part is when a server wants to connect to another server, it needs to know the [**IP address**](https://www.geeksforgeeks.org/computer-science-fundamentals/what-is-an-ip-address/) and the [**port number**](https://www.geeksforgeeks.org/computer-networks/difference-between-ip-address-and-port-number/) where that particular application is running.

![Diagram showing multiple microservices (Address-service, Employee-service, Course-service, Student-service, X-service, Y-service, Location-service, Z-service) deployed across various servers. Arrows indicate inter-service communication. A separate box on the right asks "Server IP? Server Port?" highlighting the challenge of finding service locations in a distributed environment.](https://media.geeksforgeeks.org/wp-content/uploads/20230413194432/3-(1).webp)

Managing this becomes a complex job when you have thousands of applications. How will you manage server IPs and port numbers? Every server needs to know the IP and server address of another server to connect to it. This becomes a critical task when your monolithic application has been split into thousands of different modules and deployed into different servers. Don't you think managing the **IP** and the server **URL** will be critical? In the case of a [Monolithic Application](https://www.geeksforgeeks.org/software-engineering/monolithic-vs-microservices-architecture/), there was only one server, so we could remember its IP and port. But now, with thousands of applications, how do you handle that? For this, **Spring Cloud provides Service Discovery and Service Registry to handle this problem.**

### What's Service Discovery and Service Registry in Microservices?

Suppose we have Service-A and Service-B, and a Load Balancer placed inside a different server. Now, let's introduce our Discovery Service. Whenever Service-A and Service-B want to communicate, they will register themselves with the Discovery Service when they start. This Discovery Service will then know the IP and port number of Service-A and Service-B. All detailed information will be stored with the Discovery Service. Similarly, if we have many different instances of Service-B, all these instances running in different servers will register their information with the Discovery Service. So, it acts as a central location where we manage host and port number information. This process is called registration because all services, when they start, register themselves with the discovery service, which then maintains all their information in a map, list, or database. This central repository is called a **Service Registry**.

![A table representing a Service Registry. It has two columns: "Key" and "Instance List". "Service-B" has multiple IP:Port entries (111:22:33:44:9090, 100:2:3:4:8090, 333:111:33:1:8081, 111:22:33:44:8080). "Service-A" has one entry (111:22:33:44:8098). This illustrates how a registry stores the network locations of service instances.](https://media.geeksforgeeks.org/wp-content/uploads/20230514132146/1.webp)

So, Service Registry is a crucial part of service identification. It’s a database containing the network locations of service instances. A Service Registry must be highly available and up-to-date. In the example above, the Service Registry shows four different instances of Service-B running at various IP addresses and port numbers, and one instance of Service-A.

![A diagram illustrating the interaction between Service-A, a Load Balancer, a Discovery Service (containing the Registry), and multiple instances of Service-B. Service-A (111.22.33.44:8098) is connected to the Load Balancer. The Load Balancer queries the Discovery Service, which uses its internal Registry to find the IP:Port of Service-B instances (e.g., 111.22.33.44:9090, 100.2.3.4:8090, 333.111.33.1:8081, 111.22.33:8080).](https://media.geeksforgeeks.org/wp-content/uploads/20230514132147/3.webp)

Now, if Service-A wants to connect to Service-B, the Load Balancer receives the request and queries the Discovery Service: "Hey, can you tell me what instances are there for Service-B?" The Load Balancer then finds out that there are multiple instances available where Service-B has been deployed. The Load Balancer will then dispatch the request to one of the servers by looking into the Service Registry. It can choose among the four instances of Service-B, potentially sending the request to the instance with the least load to balance the workload.

![A refined diagram showing the flow of a request. Service-A sends a "Request" to the Load Balancer. The Load Balancer sends a "Query" to the Discovery Service. The Discovery Service, using its internal Registry, provides the list of Service-B instances. The Load Balancer then dispatches the request to one of the available Service-B instances. This highlights the role of the Discovery Service in resolving service locations for the Load Balancer.](https://media.geeksforgeeks.org/wp-content/uploads/20230514132146/2.webp)

> **Note**: Don't mix up load balancing and service discovery. The Load Balancer's job is to do load balancing, while the Service Discovery's job is to discover service information. When we build Microservices and call other microservices, we need service discovery to find the hosts and IP info. If multiple hosts are available, the load balancer helps to pick one and make a call in a load-balanced way.

### Types of Service Discovery

There are two types of Service Discovery:

*   Client-Side Service Discovery
*   Server-Side Service Discovery

**Points to Remember:**

*   **Client-Side Service Discovery Examples**: Netflix Eureka, Zookeeper, Consul
*   **Server-Side Service Discovery Examples**: NGINX, AWS ELB