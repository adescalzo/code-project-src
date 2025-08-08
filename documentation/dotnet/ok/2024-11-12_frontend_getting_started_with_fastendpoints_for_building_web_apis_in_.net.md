```yaml
---
title: Getting Started with FastEndpoints for Building Web APIs in .NET
source: https://antondevtips.com/blog/getting-started-with-fastendpoints-for-building-web-apis-in-dotnet
date_published: 2024-11-12T11:55:43.028Z
date_captured: 2025-08-06T17:29:42.478Z
domain: antondevtips.com
author: Anton Martyniuk
category: frontend
technologies: [FastEndpoints, ASP.NET Core, .NET, NuGet, FluentValidation]
programming_languages: [C#, Bash]
tags: [web-api, dotnet, fastendpoints, api-development, validation, minimal-apis, architecture, performance, boilerplate-reduction, csharp]
key_concepts: [REPR design pattern, Vertical Slice Architecture, Minimal APIs, endpoint definition, model binding, request-response pattern, input-validation, dependency-injection]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces FastEndpoints, a lightweight .NET library designed to simplify Web API development by reducing boilerplate code in ASP.NET Core. It explains how FastEndpoints leverages Minimal APIs while providing a structured approach to defining endpoints using the REPR design pattern. The post demonstrates creating API endpoints, handling various request and response types, and integrating with FluentValidation for robust input validation. Practical code examples illustrate how to implement a shipping application with FastEndpoints, showcasing its benefits in terms of simplicity, performance, and maintainability.]
---
```

# Getting Started with FastEndpoints for Building Web APIs in .NET

![A dark blue and purple banner with a white square icon containing a code tag (`</>`) and the text 'dev tips'. To the right, large white text reads 'GETTING STARTED WITH FASTENDPOINTS FOR BUILDING WEB APIS IN .NET.'](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_asp_fast_endpoint_started.png&w=3840&q=100)

# Getting Started with FastEndpoints for Building Web APIs in .NET

Nov 12, 2024

[Download source code](/source-code/getting-started-with-fastendpoints-for-building-web-apis-in-dotnet)

5 min read

### Newsletter Sponsors

[Master The Clean Architecture](https://www.courses.milanjovanovic.tech/a/aff_q2snzxhl/external?affcode=1486372_j2vpyytw). This comprehensive course will teach you how to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture. Join 3,150+ students to accelerate your growth as a software architect.

[Master The Modular Monolith Architecture](https://www.courses.milanjovanovic.tech/a/aff_l825rpg0/external?affcode=1486372_j2vpyytw). This in-depth course will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario. Join 1,050+ students to accelerate your growth as a software architect.

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Building Web APIs with ASP.NET Core can involve a lot of boilerplate code, especially when dealing with controllers, routing, and model binding. **FastEndpoints** is a lightweight library that simplifies this process, allowing you to define endpoints with minimal code with great performance.

In this blog post, we'll explore how to get started with **FastEndpoints**. I will show you to create API endpoints, handle requests, responses and add validation.

## What is FastEndpoints?

[FastEndpoints](https://fast-endpoints.com/) is an open-source library for .NET that simplifies the creation of Web APIs by eliminating the need for controllers and routing attributes. Built on top of ASP.NET Core Minimal APIs, it leverages all the performance benefits while providing a more straightforward programming model.

In the Minimal APIs, you need to define yourself how you want to structure your endpoints, how to group or not group them together in a single file. In **FastEndpoints** you define each endpoint in a separate class, which results in a Single Responsible and maintainable endpoints.

For me, this concept ideally fits in [Vertical Slice Architecture](https://antondevtips.com/blog/vertical-slice-architecture-the-best-ways-to-structure-your-project).

**FastEndpoints** follows **REPR** Design Pattern (Request-Endpoint-Response) and offers the following advantages for Web API development:

*   **Simplicity**: reduces complexity by allowing you to define endpoints as individual classes
*   **Performance**: optimized for speed, providing better throughput and lower latency
*   **Maintainability**: cleaner code structure makes it easier to maintain and scale your application
*   **Rapid Development**: faster to set up and start building APIs, improving productivity

## Getting Started with FastEndpoints

To get started with **FastEndpoints** you need to create a WebApi project and add the following Nuget package:

```bash
dotnet add package FastEndpoints
```

Here is how you can create an API Endpoint using FastEndpoints:

```csharp
public record RegisterUserRequest(string Email, string Password, string Name);
public record RegisterUserResponse(Guid Id, string Email, string Name);

public class CreateUserEndpoint : Endpoint<RegisterUserRequest, RegisterUserResponse>
{
    public override void Configure()
    {
        Post("/users/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterUserRequest request, CancellationToken token)
    {
        await SendAsync(new RegisterUserResponse(Guid.NewGuid(), "email", "name"));
    }
}
```

You need to define request, response models and a class that inherits from the base `Endpoint<TRequest, TResponse>`. In the `Configure` method you can specify:

*   HTTP method type
*   endpoint URL
*   extra attributes: like authentication, authorization, allow anonymous, versioning, rate limiting, etc.

## Endpoint Types in FastEndpoints

**FastEndpoints** offers 4 endpoint base types, that you can inherit from:

*   **Endpoint<TRequest>** - use this type if there's only a request DTO. You can, however, send any object to the client that can be serialized as a response with this generic overload.
*   **Endpoint<TRequest,TResponse>** - use this type if you have both request and response DTOs. The benefit of this generic overload is that you get strongly-typed access to properties of the DTO when doing integration testing and validations.
*   **EndpointWithoutRequest** - use this type if there's no request nor response DTO. You can send any serializable object as a response here also.
*   **EndpointWithoutRequest<TResponse>** - use this type if there's no request DTO but there is a response DTO.

It is also possible to define endpoints with EmptyRequest and EmptyResponse if needed:

```csharp
public class Endpoint : Endpoint<EmptyRequest,EmptyResponse> { }
```

## Sending Responses in FastEndpoints

**FastEndpoints** offers multiple ways to send responses, let's explore them.

1.  Directly assigning `Response` property of the base `Endpoint` class, for example:

```csharp
public class CreateUserEndpoint : Endpoint<RegisterUserRequest, RegisterUserResponse>
{
    public override void Configure()
    {
        Post("/users/register");
        AllowAnonymous();
    }

    public override Task HandleAsync(RegisterUserRequest request, CancellationToken token)
    {
        Response = new RegisterUserResponse(Guid.NewGuid(), "email", "name");
        return Task.CompletedTask;
    }
}
```

2.  Returning `Response` type directly:

```csharp
public class CreateUserEndpoint : Endpoint<RegisterUserRequest, RegisterUserResponse>
{
    public override void Configure()
    {
        Post("/users/register");
        AllowAnonymous();
    }

    public override Task HandleAsync(RegisterUserRequest request, CancellationToken token)
    {
        await SendAsync(new RegisterUserResponse(Guid.NewGuid(), "email", "name"));
    }
}
```

Here you need to pass a response model directly to the base `SendAsync` method.

3.  Using **TypedResults** in `HandleAsync` method:

```csharp
public class CreateUserEndpoint : Endpoint<RegisterUserRequest, RegisterUserResponse>
{
    public override void Configure()
    {
        Post("/users/register");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RegisterUserRequest request, CancellationToken token)
    {
        if (...)
        {
            await SendResultAsync(TypedResults.BadRequest("Email already exists"));
        }
    
        var response = new RegisterUserResponse(Guid.NewGuid(), "email", "name");
        await SendResultAsync(TypedResults.Ok(response));
    }
}
```

Here you need to pass a corresponding `TypedResults` response model to the base `SendResultAsync` method.

4.  Using **TypedResults** as Union-Type in `ExecuteAsync` method:

```csharp
public class CreateUserEndpoint
    : Endpoint<RegisterUserRequest, Results<Ok<RegisterUserResponse>, BadRequest<string>>>
{
    public override void Configure()
    {
        Post("/users/register");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<RegisterUserResponse>, BadRequest<string>>> ExecuteAsync(
        RegisterUserRequest request, CancellationToken token)
    {
        if (...)
        {
            return TypedResults.BadRequest("Email already exists");
        }
    
        var response = new RegisterUserResponse(Guid.NewGuid(), "email", "name");
        return TypedResults.Ok(response);
    }
}
```

In this case you need to use `ExecuteAsync` method instead of `HandleAsync`. You need to specify all `TypedResults` your method will be returning. If you try to return a wrong type - a compilation error will be raised.

## Using FastEndpoints in a Real Application

Today I'll show you how to use FastEndpoints for a **Shipping Application** that is responsible for creating and updating shipments for ordered products.

This application has 3 Web API endpoints:

*   Create Shipment
*   Update Shipment Status
*   Get Shipment by Number

Let's explore the POST "Create Shipment" endpoint implementation:

```csharp
public sealed record CreateShipmentRequest(
    string OrderId,
    Address Address,
    string Carrier,
    string ReceiverEmail,
    List<ShipmentItem> Items);

public class CreateShipmentEndpoint(IShipmentRepository repository,
    ILogger<CreateShipmentEndpoint> logger)
    : Endpoint<CreateShipmentRequest, Results<Ok<ShipmentResponse>, Conflict<string>>>
{
    public override void Configure()
    {
        Post("/api/shipments");
        AllowAnonymous();
    }

    public override async Task<Results<Ok<ShipmentResponse>, Conflict<string>>> ExecuteAsync(
        CreateShipmentRequest request, CancellationToken cancellationToken)
    {
        var shipmentAlreadyExists = await repository.ExistsAsync(request.OrderId, cancellationToken);
        if (shipmentAlreadyExists)
        {
            logger.LogInformation("Shipment for order '{OrderId}' is already created", request.OrderId);
            return TypedResults.Conflict($"Shipment for order '{request.OrderId}' is already created");
        }

        var shipmentNumber = new Faker().Commerce.Ean8();
        var shipment = request.MapToShipment(shipmentNumber);

        await repository.AddAsync(shipment, cancellationToken);

        logger.LogInformation("Created shipment: {@Shipment}", shipment);

        var response = shipment.MapToResponse();
        return TypedResults.Ok(response);
    }
}
```

Here FastEndpoints automatically binds the request's JSON body to the `CreateShipmentRequest` model:

```json
{
    "number": "10000001",
    "orderId": "11100001",
    "carrier": "Modern Delivery",
    "receiverEmail": "TODO: SET EMAIL HERE",
    "address": {
        "street": "123 Main St",
        "city": "Springfield",
        "zip": "12345"
    },
    "items": [
        {
            "product": "Acer Nitro 5",
            "quantity": 7
        }
    ]
}
```

For returning response I use `TypedResults.Conflict` and `TypedResults.Ok` that I specified in my endpoint:

```csharp
public class CreateShipmentEndpoint(IShipmentRepository repository,
    ILogger<CreateShipmentEndpoint> logger)
    : Endpoint<CreateShipmentRequest, Results<Ok<ShipmentResponse>, Conflict<string>>>
{
    public override async Task<Results<Ok<ShipmentResponse>, Conflict<string>>> ExecuteAsync(
        CreateShipmentRequest request, CancellationToken cancellationToken)
    {
    }
}
```

This ensures that you return a correct type from the endpoint; otherwise a compilation error will be raised.

For validation, FastEndpoints has built-in support for FluentValidation. You need to create the validator that inherits from the base `Validator` class:

```csharp
public class CreateShipmentRequestValidator : Validator<CreateShipmentRequest>
{
    public CreateShipmentRequestValidator()
    {
        RuleFor(shipment => shipment.OrderId).NotEmpty();
        RuleFor(shipment => shipment.Carrier).NotEmpty();
        RuleFor(shipment => shipment.ReceiverEmail).NotEmpty();
        RuleFor(shipment => shipment.Items).NotEmpty();

        RuleFor(shipment => shipment.Address)
            .Cascade(CascadeMode.Stop)
            .NotNull()
            .WithMessage("Address must not be null")
            .SetValidator(new AddressValidator());
    }
}
```

When the "Create" endpoint is called, FastEndpoint will automatically perform model validation and return `BadRequest` in the following format:

```json
{
    "StatusCode": 400,
    "Message": "One or more errors occured!",
    "Errors": {
        "ReceiverEmail": ["Email is required!", "Email is invalid!"],
        "Carrier": ["Carrier is required!"]
    }
}
```

Let's explore the GET "Get Shipment by Number" endpoint implementation:

```csharp
public record GetShipmentByNumberRequest(string ShipmentNumber);

public class GetShipmentByNumberEndpoint(IShipmentRepository repository,
    ILogger<GetShipmentByNumberEndpoint> logger)
    : Endpoint<GetShipmentByNumberRequest, ShipmentResponse>
{
    public override void Configure()
    {
        Get("/api/shipments/{ShipmentNumber}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(GetShipmentByNumberRequest request, CancellationToken cancellationToken)
    {
        var shipment = await repository.GetByNumberWithItemsAsync(request.ShipmentNumber, cancellationToken);
        if (shipment is null)
        {
            logger.LogDebug("Shipment with number {ShipmentNumber} not found", request.ShipmentNumber);
            await SendNotFoundAsync(cancellationToken);
            return;
        }

        var response = shipment.MapToResponse();
        await SendAsync(response, cancellation: cancellationToken);
    }
}
```

Here FastEndpoints automatically binds the route parameter to the `GetShipmentByNumberRequest` model:

```HTTP
GET /api/shipments/74119066
```

Now let's explore how to map this POST "Update Shipment Status" request:

```HTTP
POST /api/shipments/update-status/74119066
Content-Type: application/json
{
    "status": "WaitingCustomer"
}
```

ShipmentStatus is a part of request's JSON body that maps to `UpdateShipmentStatusRequest`:

```csharp
public sealed record UpdateShipmentStatusRequest(ShipmentStatus Status);
```

Route parameter "ShipmentNumber" you can get inside an `ExecuteAsync` or `HandleAsync` method:

```csharp
public override void Configure()
{
    Post("/api/shipments/update-status/{ShipmentNumber}");
    AllowAnonymous();
}

public override async Task<Results<NoContent, NotFound<string>>> ExecuteAsync(
    UpdateShipmentStatusRequest request, CancellationToken cancellationToken)
{
    var shipmentNumber = Route<string>("ShipmentNumber")!;
}
```

## Summary

**FastEndpoints** is a great library that simplifies Web API implementation, allowing you to define endpoints with minimal code with great performance.

**FastEndpoints** offers a ready code structure for your endpoints with a great design, so you don't need to implement your own with Minimal APIs.

For more information on FastEndpoints and various supported features, I recommend reading their [official documentation](https://fast-endpoints.com/docs).

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/getting-started-with-fastendpoints-for-building-web-apis-in-dotnet)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-fastendpoints-for-building-web-apis-in-dotnet&title=Getting%20Started%20with%20FastEndpoints%20for%20Building%20Web%20APIs%20in%20.NET)[X](https://twitter.com/intent/tweet?text=Getting%20Started%20with%20FastEndpoints%20for%20Building%20Web%20APIs%20in%20.NET&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-fastendpoints-for-building-web-apis-in-dotnet)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-fastendpoints-for-building-web-apis-in-dotnet)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.