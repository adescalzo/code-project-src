```yaml
---
title: "Microservice Architecture: Sidecar Pattern | by Shashir | Tech Enthusiasts | Medium"
source: https://medium.com/tech-enthusiasts/microservice-design-pattern-sidecar-sidekick-pattern-dbcea9bed783
date_published: 2025-02-08T05:43:15.785Z
date_captured: 2025-09-04T20:25:13.983Z
domain: medium.com
author: Shashir
category: architecture
technologies: [Kubernetes, Nginx]
programming_languages: []
tags: [microservices, sidecar-pattern, kubernetes, containers, architecture, design-patterns, logging, configuration, deployment, cloud-native]
key_concepts: [microservice-architecture, sidecar-pattern, containerization, pod, separation-of-concerns, distributed-systems, legacy-integration, dynamic-configuration]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Sidecar Pattern, a common architectural pattern in microservices, particularly within containerized environments like Kubernetes. It explains how a sidecar container runs alongside a primary application container within the same Pod, sharing resources to extend functionality such as logging, configuration, and monitoring. The pattern promotes separation of concerns by decoupling supporting tasks from core business logic. The author illustrates its utility with practical examples, including adding HTTPS to legacy services, dynamic configuration management, and log aggregation, highlighting its benefits for independent updates and resource control.]
---
```

# Microservice Architecture: Sidecar Pattern | by Shashir | Tech Enthusiasts | Medium

# Microservice Architecture: Sidecar Pattern

In a microservice architecture, it’s very common to have multiple services/apps that often require common functionalities like logging, configuration, monitoring & networking services. These functionalities can be implemented and run as a separate service within the same container or in a separate container.

_Implementing Core logic and supporting functionality within the same application:_

When they are implemented in the same application, they are tightly linked & run within the same process by making efficient use of the shared resources. In this case, these components are not well segregated and they are interdependent which may lead to failure in one component and can in turn impact another component or the entire application.

_Implementing Core logic and supporting functionality in a separate application:_

When the application is segregated into services, each service can be developed with different languages and technologies best suited for the required functionality. In this case, each service has its own dependencies \\libraries to access the underlying platform and the shared resources with the primary application. It also adds latency to the application when we deploy two applications on different hosts and add complexity in terms of hosting, deployment and management.

**Sidecar Pattern (or) Sidekick Pattern?**

The sidecar concept in Kubernetes is getting popular and It's a common principle in the container world, that a container should address a single concern & it should do it well. The Sidecar pattern achieves this principle by decoupling the core business logic from additional tasks that extend the original functionality.

A sidecar pattern is a single-node pattern made up of two containers.

The first is the application container which contains the core logic of the application (primary application). Without this container, application wouldn’t exist.

In addition, there is a Sidecar container used to extend/enhance the functionalities of the primary application by running another container in parallel on the same container group (Pod). Since sidecar runs on the same Pod as the main application container it shares the resources — filesystem, disk, network etc.,

It also allows the the deployment of components (implemented with different technologies) of the same application into a separate, isolated & encapsulated containers. It proves extremely useful when there is an advantage to share the common components across the microservice architecture (eg: logging, monitoring, configuration properties etc..)

![Conceptual diagram illustrating the Sidecar Pattern, showing a primary application container and a sidecar container running within a Pod, sharing resources like filesystem, disk, and network.](https://miro.medium.com/v2/resize:fit:700/1*b2F4OPWS_ZEW_kUA1jM5Bg.png)

Sidecar Pattern

![Diagram of a Kubernetes Pod containing two containers: a 'Main Container' for the 'Primary App with Core functionality' and a 'Sidecar Container' for 'Configuration, Logging, etc.', with a 'Shared State: Disk, File system, Network ...' between them.](https://miro.medium.com/v2/resize:fit:700/1*eDcxLf2SRpxCgwKt_Gzzsg.png)

Eg: Sidecar: Pod with 2 containers

**What is a Pod?**

Pod is a basic atomic unit for deployment in Kubernetes (K8S).

In K8S, a pod is a group of one or more containers with shared storage and network. Sidecar acts as a utility container in a pod and its loosely coupled to the main application container. Pod’s can be considered as Consumer group (in Kafka terms) which runs multiple containers.

**When Sidecar pattern is useful ?**

*   When the services/components are implemented with multiple languages or technologies.
*   A service/component must be co-located on the same container group (pod) or host where primary application is running.
*   A service/component is owned by remote team or different organization.
*   A service which can be independently updated without the dependency of the primary application but share the same lifecycle as primary application.
*   If we need control over resource limits for a component or service.

**Examples**:

1\. Adding HTTPS to a Legacy Service
2\. Dynamic Configuration with Sidecars
3\. Log Aggregator with Sidecar

**_Adding HTTPS to a Legacy Service_**

Consider a legacy web service which services requests over unencrypted HTTP. We have a requirement to enhance the same legacy system to service requests with HTTPS in future.

![Diagram showing a Pod containing a 'Legacy Web Service (HTTP)' and a 'Sidecar SSL Proxy (HTTPS)'. External HTTPS traffic hits the Sidecar, which then forwards unencrypted HTTP traffic to the Legacy Web Service on localhost.](https://miro.medium.com/v2/resize:fit:617/1*23frGSPjof5yz_-J-4ikxg.png)

Example: Example: Adding HTTPS to a Legacy Service

The legacy app is configured to serve request exclusively on localhost, which means that only services that share the local network with the server able to access legacy application. In addition to the main container (legacy app) we can add Nginx Sidecar container which runs in the same network namespace as the main container so that it can access the service running on localhost.

At the same time Nginx terminate HTTPS traffic on the external IP address of the pod and delegate that traffic to the legacy application.

**_Dynamic Configuration with Sidecars_**

![Diagram illustrating dynamic configuration with a sidecar. A Pod contains a 'Legacy App' and a 'Configuration Manager Sidecar'. The sidecar syncs configuration from the 'Cloud' (1), updates a 'Shared Filesystem' (2), signals the 'Legacy App' (3), which then reads the new configuration (4).](https://miro.medium.com/v2/resize:fit:700/1*4IzaHMpT8UbHWGISAJiuww.png)

Example: Example: Dynamic Configuration with Sidecars

When the legacy app starts, it loads its configuration from the filesystem.

When configuration manager starts, it examines the differences between the configuration stored on the local file system and the configuration stored on the cloud. If there are differences, then the configuration manager downloads the new configuration to the local filesystem & notify legacy app to re-configure itself with the new configuration (Eg: can be EDD or Orchestration mechanism to pick new config changes)

**_Log Aggregator with Sidecar_**

Consider we have a web server which is generating access/error logs which is not soo critical to be persisted on the volume beyond the specific time interval/memory space. However, access/error logs helps to debug the application for errors/bugs.

![Diagram showing a 'Main Application' and a 'Sidecar Container' within a Pod. Clients interact with the Main Application. Both containers share a 'Volume'. The Sidecar Container reads from the Volume and sends logs to a 'Log Aggregator'.](https://miro.medium.com/v2/resize:fit:700/1*YErpHm7ftsrCNUs0unVWuA.png)

Example: Log Aggregator with Sidecar

As per separation of concerns principle, we can implement the Sidecar pattern by deploying a separate container to capture and transfer the access/error logs from the web server to log aggregator.

Web server performs its task well to serve client requests & Sidecar container handle access/error logs. Since containers are running on the same pod, we can use a shared volume to read/write logs.