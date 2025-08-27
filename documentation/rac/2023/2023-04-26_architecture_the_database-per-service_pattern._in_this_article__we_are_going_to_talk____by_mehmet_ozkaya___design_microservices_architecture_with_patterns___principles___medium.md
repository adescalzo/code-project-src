```yaml
---
title: "The Database-per-Service Pattern. In this article, we are going to talk… | by Mehmet Ozkaya | Design Microservices Architecture with Patterns & Principles | Medium"
source: https://medium.com/design-microservices-architecture-with-patterns/the-database-per-service-pattern-9d511b882425
date_published: 2023-04-26T15:05:46.135Z
date_captured: 2025-09-04T20:31:31.048Z
domain: medium.com
author: Mehmet Ozkaya
category: architecture
technologies: [MongoDB, Redis, PostgreSQL, MySQL, Java, Spring Boot, H2, JPA, Hibernate, REST APIs]
programming_languages: [Java, SQL]
tags: [microservices, database-per-service, architecture, design-patterns, data-management, polyglot-persistence, e-commerce, spring-boot, data-isolation, scalability]
key_concepts: [Database-per-Service pattern, Microservices architecture, Loose coupling, Data isolation, Scalability, Polyglot persistence, REST APIs, Design principles (KISS, YAGNI, SoC, SOLID)]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Database-per-Service pattern, a core concept in microservices architecture where each service manages its own dedicated database. It highlights the benefits of this pattern, including better separation of concerns, data isolation, independent scalability, and the flexibility of polyglot persistence. The author illustrates the pattern with an e-commerce application example, suggesting different database technologies like MongoDB, Redis, and PostgreSQL for various microservices. A practical Java Spring Boot code example demonstrates the implementation using H2 in-memory databases for separate customer and order services. The pattern is crucial for building highly scalable and available microservice systems.]
---
```

# The Database-per-Service Pattern. In this article, we are going to talk… | by Mehmet Ozkaya | Design Microservices Architecture with Patterns & Principles | Medium

# The Database-per-Service Pattern

Mehmet Ozkaya

5 min read · Sep 6, 2021

In this article, we are going to talk about **Design Patterns** of Microservices architecture which is **The Database-per-Service pattern**. As you know that we learned **practices** and **patterns** and add them into our **design toolbox**. And we will use these **pattern** and **practices** when **designing microservice architecture**.

![Diagram showing three microservices, each with its own dedicated database, accessed by web and mobile clients.](https://miro.medium.com/v2/resize:fit:700/1*5CUirR-FYRC2nN4dA8dCjg.png)

By the end of the article, you will learn where and when to **apply Database-per-Service pattern** into **Microservices Architecture** with designing **e-commerce application** system with following **KISS, YAGNI, SoC** and **SOLID principles**.

![Advertisement for a Udemy course titled "Design Microservices Architecture with Patterns & Principles."](https://miro.medium.com/v2/resize:fit:700/0*-xZlSEv3hLpfD29p.png)

**I have just published a new course — Design Microservices Architecture with Patterns & Principles.**

In this course, we’re going to learn **how to Design Microservices Architecture** with using **Design Patterns, Principles** and the **Best Practices.** We will start with designing **Monolithic** to **Event-Driven Microservices** step by step and together using the right architecture design patterns and techniques.

## The Database-per-Service pattern

**The Database-per-Service Pattern** is used in microservices architecture where each microservice has its own dedicated database. This pattern allows for better separation of concerns, data isolation, and scalability.

**In a microservices architecture**, services are designed to be small, focused, and independent, each responsible for a specific functionality. To maintain this separation, it’s essential to ensure that each microservice manages its **data independently**. The Database-per-Service Pattern enforces this by **allocating** a **separate database** for each microservice.

One of the **core characteristic** of the microservices architecture is the **loose coupling** of **services**. For that reason every service must have its own databases, it can be **polyglot persistence** among to microservices.

![Diagram showing three microservices, each with its own dedicated database, accessed by web and mobile clients.](https://miro.medium.com/v2/resize:fit:700/1*5CUirR-FYRC2nN4dA8dCjg.png)

By using the **Database-per-Service Pattern** in microservices:

*   **Data schema changes** made easy without impacting other microservices
*   Each microservice has **its own data store**, preventing accidental or unauthorized access to another service’s data.
*   Since each microservice and its database are separate, they can be **scaled independently** based on their specific needs.
*   **Microservices Domain** data is **encapsulated** within the service.
*   Each microservice can **choose the database technology** that best suits its requirements, without being bound to a single, monolithic database.
*   If one of the database server is **down**, this will **not affect** to other services.

### E-Commerce Microservices with Database-per-Service Pattern

Let’s think about our **e-commerce** application. We will have **Product, Shopping Cart** and **Order microservices** that each services data in their own databases. Each of these microservices would have its own dedicated database. Any changes to one database don’t impact other microservices.

The service’s database **can’t be accessed directly** by other microservices. Each service’s persistent data can only be accessed via **Rest APIs**. So database per microservice provides many benefits, especially for **evolve rapidly** and support **massive scale systems**.

![Diagram illustrating an e-commerce microservices architecture where client applications (Web, Mobile) interact with Catalog, Shopping Cart, Discount, and Ordering microservices, each connected to its own dedicated database.](https://miro.medium.com/v2/resize:fit:700/1*yftphSLwxoI4aSrpeoT7pw.png)

So if we see the image that **example** of **microservice architecture** of e-commerce application;

*   **Product Catalog Microservice:** This service manages product information and may use a **NoSQL document-based database** like MongoDB to store the product details, as it offers flexibility in handling complex data structures which is storing JSON objects to accommodate high-volumes of read operations.
*   **Shopping Cart Microservice:** This service handles user shopping carts and may use a **key-value store distributed cache like Redis**, as it provides high performance and low latency for frequent read-write operations.
*   **Order Management Microservice:** This service manages customer orders and may use a **relational database like PostgreSQL or MySQL**, as it requires transactional support and strong consistency to accommodate the rich relational structure of its underlying data.

Because of the ability of massive scale and **high availability**, **NoSQL databases** are getting high popularity and becoming widely use in enterprise application. Also their **schema-less structure** give flexibility to developments on microservices. We cover NoSQL databases later in upcoming articles.

### Sample Code Example for Database-per-Service pattern

Here’s a simple example to demonstrate the Database-per-Service pattern in a Java application using Spring Boot. We’ll create two microservices: `CustomerService` and `OrderService`, each with its own dedicated H2 in-memory database.

Create a `CustomerRepository` for the `CustomerService` microservice:

```java
// CustomerService/src/main/java/com/example/customerservice/repository/CustomerRepository.java
  
package com.example.customerservice.repository;
  
import com.example.customerservice.entity.Customer;
import org.springframework.data.jpa.repository.JpaRepository;
  
public interface CustomerRepository extends JpaRepository<Customer, Long> {
}
```

Create a `CustomerController` for the `CustomerService` microservice:

```java
// CustomerService/src/main/java/com/example/customerservice/controller/CustomerController.java
  
package com.example.customerservice.controller;
  
import com.example.customerservice.entity.Customer;
import com.example.customerservice.repository.CustomerRepository;
  
import java.util.List;
  
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;
  
@RestController
@RequestMapping("/api/customers")
public class CustomerController {
  
    @Autowired
    private CustomerRepository customerRepository;
  
    @PostMapping
    public Customer createCustomer(@RequestBody Customer customer) {
        return customerRepository.save(customer);
    }
  
    @GetMapping
    public List<Customer> getAllCustomers() {
        return customerRepository.findAll();
    }
}
```

So far we have developed Customer Microservices DB implementation code. With following the same steps you can;

*   Create an `OrderRepository` for the `OrderService` microservice
*   Create an `OrderController` for the `OrderService` microservice

To Configure separate H2 in-memory databases for both microservices:

```properties
# CustomerService/src/main/resources/application.properties
  
spring.datasource.url=jdbc:h2:mem:customers
spring.datasource.driverClassName=org.h2.Driver
spring.datasource.username=sa
spring.datasource.password=
spring.jpa.database-platform=org.hibernate.dialect.H2Dialect
spring.h2.console.enabled=true
```

Each microservice has its own dedicated in-memory H2 database, following the Database-per-Service pattern. In a production environment, you would use separate instances of a more robust database system, such as PostgreSQL, MySQL, or any other suitable database system.

As you can see that we have understand the **Design Patterns — The Database-per-Service pattern.** The Database-per-Service Pattern enables the microservices architecture to fully realize its benefits and maintain the desired level of separation and autonomy.

After this, we should **evolve our architecture** with applying new **microservices patterns** in order to **accommodate business adaptations** faster time-to-market and handle larger requests.

### What’s Next ?

*   [**Design Modular Monolithic Architecture for E-Commerce Applications**](/design-microservices-architecture-with-patterns/design-modular-monolithic-architecture-for-e-commerce-applications-with-step-by-step-c6ddf466f3e6)
*   [**Macro-services to Nano-services: Evolution of Software Architecture**](/design-microservices-architecture-with-patterns/macro-services-to-nano-services-evolution-of-software-architecture-424f927b63cb)
*   [**Microservices Architecture: Problems and Solutions with Pattern and Principles**](/design-microservices-architecture-with-patterns/microservices-architecture-problems-and-solutions-with-pattern-and-principles-b673f342dc10)

### Step by Step Design Architectures w/ Course

![Advertisement for a Udemy course titled "Design Microservices Architecture with Patterns & Principles" with details about handling millions of requests and designing scalable systems.](https://miro.medium.com/v2/resize:fit:700/0*cZQJEq0383RFZkuY.png)

**I have just published a new course — Design Microservices Architecture with Patterns & Principles.**

In this course, we’re going to learn **how to Design Microservices Architecture** with using **Design Patterns, Principles** and the **Best Practices.** We will start with designing **Monolithic** to **Event-Driven Microservices** step by step and together using the right architecture design patterns and techniques.