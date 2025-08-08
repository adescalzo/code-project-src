```yaml
---
title: How to Organize Minimal APIs
source: https://www.nikolatech.net/blogs/organize-minimal-apis-aspnetcore
date_published: 2025-02-12T21:21:12.406Z
date_captured: 2025-08-08T19:34:01.065Z
domain: www.nikolatech.net
author: Unknown
category: backend
technologies: [.NET, ASP.NET Core, Minimal APIs, MediatR, Carter, FastEndpoints, NuGet, Entity Framework Core]
programming_languages: [C#, SQL]
tags: [minimal-apis, aspnet-core, api-design, code-organization, dotnet, web-development, architecture, dependency-injection, repr-pattern, libraries]
key_concepts: [Minimal APIs, Controller-based APIs, Separation of Concerns, Extension Methods, Dependency Injection, Request Endpoint Response (REPR) Pattern, Module Pattern, Boilerplate Reduction]
code_examples: true
difficulty_level: intermediate
summary: |
  The article addresses the inherent lack of structure in ASP.NET Core Minimal APIs, which can become challenging as applications grow. It explores various methods for organizing API endpoints, starting with basic C# extension methods and the `MapGroup` feature for grouping related routes. The post then introduces popular third-party libraries like Carter and FastEndpoints, demonstrating how they automate endpoint discovery and reduce boilerplate. Additionally, it provides a detailed manual approach using dependency injection to achieve similar automatic registration without external libraries. Finally, the article highlights the Request Endpoint Response (REPR) pattern as an effective architectural approach for structuring APIs around individual, single-responsibility endpoints.
---
```

# How to Organize Minimal APIs

# In this article

*   [Intro](#Intro)
*   [Minimal APIs](#MinimalApis)
*   [Extension Methods](#ExtensionMethods)
*   [Carter](#Carter)
*   [Manual Approach](#ManualApproach)
*   [REPR](#REPR)
*   [FastEndpoints](#FastEndpoints)
*   [Conclusion](#Conclusion)

![A banner image with the text 'Minimal API Organization' and the NikolaTech logo.](https://coekcx.github.io/BlogImages/banners/organize-minimal-apis-aspnetcore-banner.png)

#### How to Organize Minimal APIs

###### 12 Feb 2025

###### 5 min

In .NET, the organization of API endpoints has evolved over time to improve separation of concerns, maintainability and even performance.

In the early days, APIs were organized around controllers, where each entity typically had a dedicated controller.

These controllers often became fat, handling too much logic and leading to tight coupling.

To address the issue of fat controllers, [MediatR](https://github.com/jbogard/MediatR) was a popular solution. Instead of injecting many services and handling logic within controllers, they would simply delegate work to request handlers, keeping each endpoint slim.

This approach improved scalability, separation of concerns and testability.

To further simplify APIs and reduce boilerplate, **Minimal APIs** were introduced, allowing developers to define endpoints directly without controllers.

However, a key drawback of Minimal APIs is the lack of built-in structure, which we will try to address in this blog post.

## Minimal APIs

Introduced in .NET 6, [Minimal APIs](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-9.0) provide a lightweight approach to building APIs with minimal boilerplate code.

They enable defining endpoints directly in Program.cs, eliminating the need for controllers or attributes, simplifying API development.

Minimal APIs can also handle more requests per second compared to traditional controllers, primarily due to their reduced overhead.

Creating your first Minimal API implementation is straightforward:

```csharp
app.MapGet("users/{id}", async (Guid id, ApplicationDbContext dbContext) =>
{
    var user = await dbContext.Users
        .Where(x => x.Id == id)
        .Select(x => new UserResponse(x.Id, x.Email, x.FirstName, x.LastName))
        .FirstOrDefaultAsync();

    return user is null
        ? Results.NotFound()
        : Results.Ok(user);
});
```

To define endpoints, you use **Map** methods (such as MapGet, MapPost, etc.), which map routes to handlers and process incoming requests.

Inside the handler, you define the logic to process the request. Endpoint handlers support DI at the method level, allowing services to be injected directly into them.

However, as you can imagine, this approach of defining all Minimal APIs within the Program.cs file can quickly become an issue as the application evolves.

## Extension Methods

The simplest solution to address this issue is to create **extension methods**, which will move the endpoint definitions into separate files.

```csharp
public static class UserEndpointsExtensions
{
    public static void MapUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("users/{id}", async (Guid id, ApplicationDbContext dbContext) =>
        {
            var user = await dbContext.Users
                .Where(x => x.Id == id)
                .Select(x => new UserResponse(x.Id, x.Email, x.FirstName, x.LastName))
                .FirstOrDefaultAsync();

            return user is null
                ? Results.NotFound()
                : Results.Ok(user);
        });

        app.MapGet("users/", async (ApplicationDbContext dbContext) =>
        {
            var users = await dbContext.Users
                .Select(x => new UserResponse(x.Id, x.Email, x.FirstName, x.LastName))
                .ToListAsync();

            return Results.Ok(users);
        });
    }
}
```

With this approach, in Program.cs, you only call the extension methods to register your endpoints, keeping the code clean and easier to maintain.

If you prefer a controller-like structure, you can use the **MapGroup** method to group related endpoints, define a common prefix or apply common filters.

```csharp
var group = app.MapGroup("users");

group.MapGet("{id}", async (Guid id, ApplicationDbContext dbContext) =>
{
    var user = await dbContext.Users
        .Where(x=>x.Id == id)
        .Select(x=> new UserResponse(x.Id, x.Email, x.FirstName, x.LastName))
        .FirstOrDefaultAsync();

    return user is null
        ? Results.NotFound()
        : Results.Ok(user);
});

group.MapGet("", async (ApplicationDbContext dbContext) =>
{
    var users = await dbContext.Users
        .Select(x => new UserResponse(x.Id, x.Email, x.FirstName, x.LastName))
        .ToListAsync();

    return Results.Ok(users);
});
```

The only downside is that you need to manually register each new extension method.

```csharp
app.MapUserEndpoints();
app.MapOrderEndpoints();
```

## Carter

To resolve this issue, you can use Carter, an open-source library designed to simplify the process of defining routes.

To get started with **Carter**, you'll first need to install the necessary NuGet packages. You can do this via the NuGet Package Manager or by running the following command in the Package Manager Console:

```shell
dotnet add package Carter
```

Once the package is installed, you need to define an API module by implementing the **ICarterModule** interface. This interface allows you to define routes for a specific endpoint or group of endpoints.

```csharp
public class UserModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{id}", async (Guid id, ApplicationDbContext dbContext) =>
        {
            var user = await dbContext.Users
                .Where(x => x.Id == id)
                .Select(x => new UserResponse(x.Id, x.Email, x.FirstName, x.LastName))
                .FirstOrDefaultAsync();

            return user is null
                ? Results.NotFound()
                : Results.Ok(user);
        });

        app.MapGet("users/", async (ApplicationDbContext dbContext) =>
        {
            var users = await dbContext.Users
                .Select(x => new UserResponse(x.Id, x.Email, x.FirstName, x.LastName))
                .ToListAsync();

            return Results.Ok(users);
        });
    }
}
```

In Program.cs, all you need to do is add Carter to the DI with **AddCarter** and map all routes using the **MapCarter** method.

```csharp
builder.Services.AddCarter();

var app = builder.Build();

app.MapCarter();
```

Carter automatically discovers and registers API endpoints based on modules that implement the ICarterModule interface while keeping Program.cs clean.

## Manual Approach

Carter does an excellent job, but if you prefer not to rely on third-party libraries, here's a manual approach I frequently use.

It shifts the registration of endpoints away from Program.cs, while still allowing for automatic discovery and registration of your endpoints.

To begin, you need to create an abstraction that should represent an endpoint:

```csharp
public interface IModule
{
    void Map(IEndpointRouteBuilder app);
}
```

**IModule** also includes a **Map** method, which is responsible for defining one or more endpoints. Depending on your approach, you can define either a single endpoint or multiple endpoints within each IModule implementation.

The next step is to register the modules in the DI container:

```csharp
var assembly = Assembly.GetExecutingAssembly();

var descriptors = assembly.DefinedTypes
    .Where(type => type.IsClass && type.IsAssignableTo(typeof(IModule)))
    .Select(type => ServiceDescriptor.Transient(typeof(IModule), type))
    .ToArray();

services.TryAddEnumerable(descriptors);
```

In my example, I load the defined types from the current assembly, then filter them to find all implementations of the IModule interface. Each of these endpoint types is registered as a transient service in the DI container.

Once the endpoints are registered, we can use DI to retrieve all implementations, allowing us to iterate through each IModule instance and call the Map method.

```csharp
var endpoints = app.Services.GetRequiredService<IEnumerable<IModule>>();

foreach (var endpoint in endpoints)
{
    endpoint.Map(app);
}
```

I prefer to move this logic into extension methods to keep Program.cs clean.

Finally, you can implement your endpoints and achieve a similar experience to what you had with Carter:

```csharp
public class UserModule : IModule
{
    public void Map(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{id}", async (Guid id, ApplicationDbContext dbContext) =>
        {
            var user = await dbContext.Users
                .Where(x => x.Id == id)
                .Select(x => new UserResponse(x.Id, x.Email, x.FirstName, x.LastName))
                .FirstOrDefaultAsync();

            return user is null
                ? Results.NotFound()
                : Results.Ok(user);
        });

        app.MapGet("users/", async (ApplicationDbContext dbContext) =>
        {
            var users = await dbContext.Users
                .Select(x => new UserResponse(x.Id, x.Email, x.FirstName, x.LastName))
                .ToListAsync();

            return Results.Ok(users);
        });
    }
}
```

## Request Endpoint Response Pattern

So far, we've focused more on creating a controller-like organization. In this section, I'd like to shift the focus a bit to the REPR pattern, which has become increasingly interesting with the introduction of Minimal APIs.

**REPR Pattern** emphasizes organizing APIs around individual endpoints rather than grouping logic into centralized class/controller.

Each endpoint is implemented as its own class, promoting a single responsibility principle. By designing around endpoints, the API becomes easier to locate, navigate and modify, as each class corresponds directly to a single route.

![A diagram illustrating the Request Endpoint Response (REPR) pattern, showing a 'Request' block and a 'Response' block both pointing to an 'Endpoint' block.](https://coekcx.github.io/BlogImages/images/repr-pattern-diagram.png)

You could easily adapt the examples above to align with the REPR pattern, except for the extension methods, because you would need to manually call each extension method.

### Fast Endpoints

If you're looking to follow the REPR pattern, there's another interesting library worth mentioning.

**FastEndpoints** is an open-source library that offers performance on par with Minimal APIs. It adheres to the REPR pattern, making it easier to work with endpoints while keeping them convenient and maintainable with virtually no boilerplate.

FastEndpoints is also feature-rich, offering many useful capabilities along with excellent documentation.

If you'd like to learn more about FastEndpoints, be sure to check out my dedicated blog post on the topic: [FastEndpoints](https://www.nikolatech.net/blogs/fast-endpoints-repr-minimal-api-alternative-dotnet)

## Conclusion

Minimal APIs are certainly a step in the right direction.

They simplify development and minimize boilerplate code, but they lack structure as applications grow.

To maintain organization and scalability, solutions such as extension methods, Carter, FastEndpoints, and manual DI registration provide effective approaches.

If you want to check out examples I created, you can find the source code here:

[Source Code](https://www.nikolatech.net/codes/organize-minimal-api-examples)