```yaml
---
title: Minimal API with Vertical slice architecture - Treblle
source: https://treblle.com/blog/minimal-api-with-vertical-slice-architecture
date_published: 2024-01-12T13:00:51.000Z
date_captured: 2025-08-06T17:23:51.456Z
domain: treblle.com
author: Unknown
category: architecture
technologies: [Minimal API, ASP.NET Core, .NET, Entity Framework Core, Xamarin.Forms, MAUI, Fast Endpoints, Api.Endpoints]
programming_languages: [C#, SQL]
tags: [api-design, architecture, minimal-api, vertical-slice-architecture, dotnet, web-api, software-design, cqrs, clean-architecture, layered-architecture]
key_concepts: [Vertical Slice Architecture, Layered Architecture, REPR pattern, CQRS pattern, Single Responsibility Principle, Data Transfer Objects, Immutability, Positional Records]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores the integration of Minimal APIs with Vertical Slice Architecture (VSA) as a modern approach to API design, contrasting it with traditional layered architectures like N-Tier or Clean Architecture. It defines a "slice" as a self-contained functional unit encompassing all necessary layers for a specific feature, promoting reduced coupling and streamlined testing. The REPR (Request, Endpoint, Response) pattern is introduced for structuring these slices, utilizing C# records for immutable data contracts. The post provides practical C# code examples for implementing user-related endpoints within this architecture and discusses potential scaling challenges, suggesting libraries like Fast Endpoints and Api.Endpoints as solutions.
---
```

# Minimal API with Vertical slice architecture - Treblle

# Minimal API with Vertical slice architecture

API Design | Jan 12, 2024 | 9 min read | By [Pavle Davitković](/blog/authors/pavle-davitkovic)

![Minimal API with Vertical slice architecture image](/_next/image?url=%2Fcms%2Fimages%2Fvertical-slice-architecture.jpg&w=3840&q=75)

![Pavle Davitković's picture](/_next/image?url=%2Fcms%2Fimages%2Fpavle-davitkovic.jpeg&w=96&q=75)[Pavle Davitković](/blog/authors/pavle-davitkovic)

Pavle Davitković is a .NET software developer working with APIs and cross-platform technologies like Xamarin.Forms, MAUI, and Entity Framework Core. He focuses on building efficient, scalable solutions backed by relational databases. Pavle also shares insights with the developer community through technical content on LinkedIn.

*   [Challenges of layered architecture](#heading-challenges-of-layered-architecture)
*   [What is vertical slice architecture?](#heading-what-is-vertical-slice-architecture)
*   [What is a slice?](#heading-what-is-a-slice)
*   [Possible problem and solution](#heading-possible-problem-and-solution)

Explore the fusion of Minimal APIs with Vertical Slice Architecture in this blog. We'll compare it with traditional layered approaches, introduce 'slices' in software design, and guide you through implementing this innovative, modular method.

In a previous blog I've talked about [structuring Minimal API](https://treblle.com/blog/how-to-structure-your-minimal-api-in-net).

One of the differences I mentioned was “**better fit for vertical slice architecture**”.

So today I want to explain to you a few things:

*   What is vertical slice architecture?
*   What makes them different from traditional approach
*   How to implement this architecture with Minimal API?

But before we dive into the “new way” of creating APIs, let's take a brief look at the traditional approach.

### Layered “traditional” way

The traditional layered approach in software architecture refers to a design method that organizes a system's components or modules into distinct layers, each responsible for specific functionalities.

The most notable architectures utilizing this approach are:

*   N-Tier architecture
*   Clean architecture
*   Onion architecture

![Clean architecture diagram](https://bucketeer-e05bbc84-baa3-437e-9518-adb32be77984.s3.amazonaws.com/public/images/0607e977-aaf3-430a-8d4b-22038432be68_1358x760.png)

Clean architecture

Here, each layer communicates with the adjacent layers following specific guidelines.
Often adhering to principles like separation of concerns and single responsibility, where each layer focuses on its designated tasks without tightly coupling with other layers.

But, this approach has a few challenges.

## Challenges of layered architecture

As with everything in the world of software development, benefits are often followed by challenges.

Layered architecture introduces:

*   **Tight coupling between layers**: Changes in one layer might affect others because tight coupling can hinder flexibility and make the system more fragile.
*   **Dependency management**: Changes in one layer might necessitate modifications in dependent layers.
*   **Choosing the right abstraction level**: Over- or under-segmentation of layers can lead to architectural issues.

Now, aware of these challenges, consider how you structure a feature.
Do you progress from “top to bottom,” intersecting the layers?

If so, congratulations, you are now thinking in slices.

## What is vertical slice architecture?

Vertical Slice Architecture (VSA) is a software development approach that organizes applications into distinct functional slices.

The fundamental concept behind this architecture involves grouping code based on business functionalities and consolidating related code.

This approach yields several benefits:

*   **Reduced coupling**: Coupling between features is minimized. Entire feature implementations reside within one slice, enhancing maintainability.
*   **Streamlined testing**: Testing becomes more straightforward by focusing on individual slices, ensuring each feature functions as intended.
*   **Flexibility and adaptability**: VSA provides flexibility for scaling, deployment, and future modifications without impacting the entire system.

![Vertical slice architecture diagram](/_next/image?url=%2Fcms%2Fimages%2FYQAqmIUlxJ7xe-L23-HOSnh6-S_orj5yywwQgD26Smu5iL97JY76oxb_HWG2PF6bMYSObl8dVbbIaPdTYNjwjz-vR6QzSLsY-2Km-cy7dXwwZ2w4nJectf7FZ06YpyckVGLIuMN41VNV63HV8I5tku4&w=3840&q=75)

Vertical slice architecture

Before we dive into the implementation part, we need to:

*   Point out difference between this two architectures
*   Explain what is slice

### Vertical slice vs layered architecture

| Aspect | Vertical slice architecture | Layer architecture |
| :----- | :-------------------------- | :----------------- |
| Scope | Focuses on end-to-end feature development for a single vertical or user story. | Divides the application into horizontal layers (e.g., presentation, business logic, data access) |
| Organization | Organized around features/modules. | Organized around functional layers |
| Dependency management | Reduces inter-module dependencies by encapsulating complete feature sets. | Tends to have interdependencies between layers. |
| Flexibility | Easier to modify or replace specific features/modules. | Changing one layer may impact other layers. |
| Testing | Encourages comprehensive testing of specific features/modules. | Testing often involves mocking layers for isolated tests. |
| Initial setup | Often quicker to set up as it involves a smaller initial scope by focusing on a specific feature. | May require more upfront planning and architecture design to establish the structure of various layers. |
| Complexity | Helps in managing complexity by focusing on cohesive feature development, potentially reducing the complexity within each vertical slice. | May lead to increased complexity within individual layers but promotes separation of concerns. |
| Maintenance | Easier to maintain individual features/modules. | Maintenance can be complex due to interconnected layers. |

As you can see from the table, both approaches have their merits, and the choice ultimately depends on achieving the right balance between maintainability, flexibility, and scalability.

## What is a slice?

A "slice" denotes a self-contained development unit within an application, encompassing all necessary layers to implement a particular feature.
Every slice operates independently and strives to deliver a fully functional segment of the application capable of functioning autonomously.

![Visual representation of slice](/_next/image?url=%2Fcms%2Fimages%2F24xZQNzvo5wLgsI7vcwXB0G-TCe29l2fnSpx__I2cdTRLMeU4TocPxJ8C7Hpe4uZVG9BBaUXAsYoE8iv6cn-lNwpBggyaFrHQkEFPHaNhv4a5wcZmaRmqLRlNqEZ1x78dLWj8F-tMkDMwyuWRz8wTDg&w=3840&q=75)

Visual representation of slice

And now…the implementation part!

### Implementing vertical slice architecture with Minimal API

For the sake of simplicity, we are going to rely on data from previous [_post_](https://treblle.com/blog/how-to-structure-your-minimal-api-in-net/#endpoint-grouping).

This is the project structure:

![Project structure screenshot](/_next/image?url=%2Fcms%2Fimages%2FcHquHqO72dXAniHf8dnPEhFr_3qsoosdZ_ca5ixDkMhOfMAtvBaueF8lwM74sGY5CStn2UAuPuSXjeAizV9LeW3vsyBz3jzY_kvFjcfsHT7vEDbCfvKCsuqn09zLIteHuRJnNig7xxJ-qqphGLI1blw&w=3840&q=75)

Project structure

I predominantly employ the Features folder approach for code organization.
This method mandates structuring slices within a single folder as extensively as feasible.

Features can be defined in two ways:

*   **Shallow organization:** Code is arranged based on technical concerns.
*   **Deep organization:** Code is structured according to the action it performs.

Moreover, this approach necessitates the implementation of the [CQRS pattern](https://martinfowler.com/bliki/CQRS.html) since each slice is divided into sub-slices, or endpoints.

![Organization types diagram](/_next/image?url=%2Fcms%2Fimages%2FRfi2LddGNoNPpCfMH7uT0rhZICSPeoLl2tfWG7pfoObkhxSNDZ4Wi9Y5jDMfnKMo2fi0kJc1nCg_pWO1hEScgE_VwddbsdzgzF8F9K7c45vYdEAsJBhgM0-mYQPkJYJ_CM8En48iUTt_er03E1HvopQ&w=3840&q=75)

Organization types

Personally, I favor deep organization due to its advantages in maintainability and ease of navigation.
However, feel free to utilize the approach that best suits your preferences.

Now that we've finalized the project structure, let's direct our attention to defining the contents of each individual slice.

### Slice setup

Each slice functions as a separate, independent unit, akin to a thread. In other words, a slice can be agnostic to the underlying libraries it employs.

![Type of different slices inside one application](/_next/image?url=%2Fcms%2Fimages%2Fp5SeeNAr61wGcQTKe7ttoi9XSb-wM2P3NgEzuZ4dBk1KEKT8MzT6_qVJriZ7qqSiha5Ut6ec5Lr3UAGqEB20fc7XDgJ96zSkrQRZPhUzeYY65kStxIU_aHyGrdKWY-WagVa_hqFtkenLOyqM1GewbpQ&w=3840&q=75)

Type of different slices inside one application

One slice can use EF Core, second raw SQL, and third can use stored procedure. Use whatever fits your needs to achieve the desired result.

All our slices will follow the **REPR** pattern.

![REPR pattern diagram](/_next/image?url=%2Fcms%2Fimages%2FN9hQUigKj1ujeSSGuhCKNblglEsAgeBPchwWifHl_tnKHYS6YToqLVE62s_uwGLj3oy8cX_jn6eZRZQhl_VV5pH3Har3mHzgnrUuOm-algev1lHU27b0Y_fIxNvCcNMAO2n93wNQgz0QOWZHeNZeuFA&w=3840&q=75)

It delineates web API endpoints with three primary components:

*   **Request:** Contracts outlining the expected data for the endpoint.
*   **Endpoint:** Business logic executed upon receiving the request.
*   **Response:** Contracts conveying the output produced by the handler.

### Why REPR?

The solution is simple.
By consolidating the request, endpoint, and response within a slice, you minimize the time spent navigating through layers, thereby enhancing the maintainability and ease of working with endpoints.

### Request and response contracts

Simply put, these two serve as Data Transfer Objects (DTOs), exclusively designed for data transfer purposes. Achieving immutability would be highly advantageous.

Fortunately, [records](https://devblogs.microsoft.com/dotnet/c-9-0-on-the-record/) align perfectly with this requirement.

We'll be implementing two endpoints:

*   **Get Users:** Queries all active users within the system.
*   **Add User:** Adds a new user to the system.

The folder structures will resemble the following:

![Folder structure for GetUsers and AddUser endpoints](/_next/image?url=%2Fcms%2Fimages%2FqRhgWqLU-9wJElyuNrOo33lmL-KiSzFqJn9AJnJ_aYt9D3_E6c1IipWnXTiiHisKy2ib8F5mQyZZMfWu_ci2WxJINhqR7HW6ge1aihpGmA-55Sm-Gy4Vo1PgOR37k-rvRzZGjL_hHBmCtcxhAf79Eo&w=3840&q=75)

Folder structure

For achieving full immutability, we are going to use [_positional records_](https://devblogs.microsoft.com/dotnet/c-9-0-on-the-record/#positional-records):

*   **Get users**:

    This endpoint is dedicated to retrieving all users for read-only purposes. Consequently, only a response object is required in this context.

```csharp
public class Contracts
{
   public record Response(string FirstName, 
                          string LastName,
                          DateOnly BirthDate,
                          bool isActive);
}
```

*   **Add user:**

    The request will bear resemblance to the response object from _GetUsers_, with added properties.
    Meanwhile, for the response, we'll return the newly created Id.

```csharp
public class Contracts
{
   public record Response(string FirstName, 
                          string LastName,
                          DateOnly BirthDate,
                          bool isActive,
                          string Address,
                          string PhoneNumber);
                          
   public record Request(int Id);
}
```

In both scenarios, we utilize a root class named _**Contracts**_.
While employing positional records, I typically opt to consolidate them within the same file to expedite navigation and ensure easier maintainability.

However, the choice of organizing records per file is also feasible.

Regarding the contract, our task is complete.

Let's proceed to the endpoint.

### Endpoint

For the endpoint handler, we'll employ a class segregated into two parts:

*   **Handler:** Housing the business logic.
*   **AddEndpoint:** A Minimal API endpoint responsible for consuming the handler method.

```csharp
public static class Endpoint
{
    private static List<User> GetUsers() => 
                    Collection.Users
                              .Where(user => user.IsActive)
                              .ToList();
 
    public static void AddEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapGet("api/v1/users", () =>
        {
            var activeUsers = GetActiveUsers();
 
            return Results.Ok(activeUsers);
        });
    }
}
```

```csharp
public static class Endpoint
{
    private static Response AddUser(Request request)
    {
        Collections.Users.Add(request);
 
        return new Response(request.Id);
    }
 
    public static void AddEndpoint(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/v1/users", (Request request) =>
        {
            var response = AddUser(request);
 
            return Results.Ok(response);
        });
    }
}
```

I employ a practice known as **mouse wheel-driven development**, emphasizing the consolidation of all related data within a single file.
However, this approach might conflict with the Single Responsibility Principle (SRP), posing a tradeoff between expedited and centralized development and the adherence to SRP for better code organization.

It the end, add endpoint to the pipeline:

```csharp
var app = builder.Build();
 
GetUsers.Endpoint.AddEndpoint(app);
 
AddUser.Endpoint.AddEndpoint(app);
```

## Possible problem and solution

While this approach seems suitable for managing a small number of features, scaling it to handle 50+ slices could pose challenges.

In such scenarios, I recommend considering specialized libraries:

*   **Fast Endpoints**: Described as a developer-friendly alternative to Minimal APIs & MVC
*   **Api.Endpoints:** Designed to support API Endpoints in ASP.NET Core web applications.

Scaling to a larger number of slices might lead to various code smells, such as:

*   **Shared code:**

    *   **Smell:** Repeated code or logic scattered across multiple slices, indicating a lack of shared components or libraries.
    *   **Solution:** Identify common functionalities and refactor them into shared components to prevent duplication and encourage reusability.
*   **Cross-Cutting Concerns:**

    *   **Smell:** Duplication of implementations like logging, error handling, or authorization across slices.
    *   **Solution:** Extract and centralize cross-cutting concerns into shared modules to ensure consistency and minimize redundancy.
*   **Poorly Defined Slice Boundaries:**

    *   **Smell:** Ambiguity in delineating slice boundaries, leading to overlaps or gaps in functionality.
    *   **Solution:** Clearly articulate the scope and boundaries of each slice, ensuring they encapsulate all necessary components for delivering specific features. Continuously refine these boundaries through discussions and reviews to maintain clarity and coherence.

### Conclusion

The integration of Minimal API alongside vertical slice architecture represents a transformative paradigm in software development, providing an efficient means to design and deploy APIs.

Emphasizing simplicity, clarity, and modularity, this architectural approach streamlines the development process while elevating the maintainability and scalability of applications.

In conclusion, here are two crucial considerations, akin to "gold nuggets," to ponder before implementation:

*   **Single Responsibility Principle:** Embed all related components (controllers, services, repositories, etc.) pertaining to a feature within a slice. This practice maintains clear and focused responsibilities within the architecture.
*   **Focused Endpoints:** Refrain from overloading endpoints with excessive functionalities or intermixing concerns from disparate slices. Maintain endpoint focus aligned with specific slice responsibilities to enhance code clarity and maintainability.