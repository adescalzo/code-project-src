```yaml
---
title: "Adapter Design Pattern. The Adapter design pattern is a… | by Ferid Akşahin | Medium"
source: https://medium.com/@ferid.aksahin98/adapter-design-pattern-5a00e6e0cf38
date_published: 2023-12-03T15:29:04.889Z
date_captured: 2025-09-05T12:38:19.274Z
domain: medium.com
author: Ferid Akşahin
category: architecture
technologies: [ASP.NET Core, Swagger UI, Kestrel, .NET, GitHub]
programming_languages: [C#]
tags: [design-pattern, adapter-pattern, structural-pattern, oop, software-architecture, interface, compatibility, code-reusability, dotnet, web-api]
key_concepts: [Adapter design pattern, structural design pattern, interface transformation, incompatible interfaces, code reusability, abstraction, delegation, API integration]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides an in-depth explanation of the Adapter design pattern, a structural pattern designed to enable collaboration between classes with incompatible interfaces. It details the pattern's core purpose: transforming one interface into another to facilitate interaction without modifying existing code. The author outlines significant benefits, including enhanced compatibility, code reusability, and flexibility, alongside potential drawbacks such as increased complexity and performance overhead. A practical C# example demonstrates how an `OldMessageService` can be adapted to a `NewMessageService` interface, showcasing the pattern's application in a modular context. The article concludes by illustrating the integration via Swagger UI, confirming the adapter's successful operation.
---
```

# Adapter Design Pattern. The Adapter design pattern is a… | by Ferid Akşahin | Medium

# Adapter Design Pattern

![Adapter Image](https://miro.medium.com/v2/resize:fit:700/1*URDVDavAsCRSbMheQWSvug.jpeg)
*Image: An illustration of a car charger with multiple USB ports and a cable, visually representing an "adapter" that bridges different power sources or interfaces.*

The Adapter design pattern is a structural design pattern used to transform the interface of a class into another interface. By utilizing the Adapter design pattern, we bring together classes that have different interfaces.

## Overview

The Adapter design pattern is a structural design model used for the collaboration of classes with incompatible interfaces. With this model, we act as a bridge and enable collaboration between classes with different interfaces. With the Adapter model, the primary goal is to allow classes with existing source code to work together with other classes without any modifications.

### **Benefits**

*   **Compatibility:** With the adapter design pattern, we facilitate the collaboration of classes with incompatible interfaces, allowing interaction between components that might otherwise be incompatible.
*   **Code Reusability:** Existing classes or components can be reused without modification, saving development time and resources.
*   **Flexibility:** Flexibility is provided by adapting the model to changes in the adopted model or by adding new classes without affecting the code.
*   **Ease of Integration:** It simplifies the integration of new components into an existing system without requiring extensive modifications to the existing codebase.

### **Drawbacks**

*   **Complexity:** Introducing adapters may lead to increased code complexity, especially in systems with numerous adapters.
*   **Potential Performance Impact:** In certain cases, the use of adapters might introduce a slight performance overhead due to the additional layer of abstraction and delegation.
*   **Maintenance Challenges:** Managing multiple adapters and their interactions can be difficult and can be a maintenance challenge.
*   **Adapter Proliferation:** In systems with diverse interfaces, the introduction of multiple adapters for various classes can lead to an increased number of classes and files, potentially making the codebase harder to manage.
*   **Introduction of an Extra Layer:** The pattern adds an extra layer (the adapter) between the client and the adaptee, which, in some scenarios, may be considered unnecessary.

### Modular Example

This example demonstrates the modular application of the Adapter design pattern. The scenario involves adapting an old message service class to be compatible with the interface of a new message service.

Let’s start with the `OldMessageService.cs` files.

`IMessage` interface file

The `IMessage` interface contains a method for message retrieval.

`NewMessageService.cs` file

As you can see, the `IMessage` interface has been implemented. `GetMessage` method was written.

`OldMessageAdapter.cs` file

In the `OldMessageAdapter` class, the `IMessage` interface is implemented and the old message service is made compatible. As you can see, the old message service was adapted to the new interface with the `OldMessageService` object given to the constructor method of this class.

`MessageController.cs`

In the `MessageController` class, messages are retrieved using both the adapter and the new service directly.

On the Swagger UI, you can see an example request for the `GetMessage` service.

![Swagger UI Screenshot](https://miro.medium.com/v2/resize:fit:700/1*tB3SbTbVF-jP9pjmhTDv4w.png)
*Image: A screenshot of the Swagger UI showing an API endpoint `/api/Message/GetMessage`. The response body displays messages received from both "Legacy Message Service" and "New Message Service", illustrating the successful use of the adapter.*

Thanks for read, for clone:

## DesignPattern/AdapterDesignPattern at main · FeridAksahin/DesignPattern
[github.com](https://github.com/FeridAksahin/DesignPattern/tree/main/AdapterDesignPattern?source=post_page-----5a00e6e0cf38---------------------------------------)