```yaml
---
title: FastEndpoints - REPR alternative to Minimal APIs
source: https://www.nikolatech.net/blogs/fast-endpoints-repr-minimal-api-alternative-dotnet
date_published: 2024-12-04T18:57:41.440Z
date_captured: 2025-08-26T14:13:23.524Z
domain: www.nikolatech.net
author: Unknown
category: backend
technologies: [.NET, FastEndpoints, ASP.NET Core, Minimal APIs, ApiEndpoints, NuGet, Swagger, Serilog, FluentValidation]
programming_languages: [C#, Bash]
tags: [dotnet, web-api, fastendpoints, minimal-apis, repr-pattern, api-design, boilerplate-reduction, performance, dependency-injection, versioning]
key_concepts: [REPR Pattern, Single Responsibility Principle, Dependency Injection, API Versioning, Model Validation, HTTP Request Pipeline, Endpoint Organization, Boilerplate Reduction]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces FastEndpoints, an open-source .NET library designed as a modern, lightweight alternative to traditional ASP.NET Core controllers and Minimal APIs. It emphasizes the REPR (Request-Endpoint-Response) pattern, promoting modular and maintainable API development with significantly reduced boilerplate. The post details how to get started with FastEndpoints, including installation, dependency injection setup, and defining various endpoint types. It also covers advanced features like configuration, response handling, and API versioning, providing practical C# code examples. FastEndpoints aims to combine the performance of Minimal APIs with superior endpoint organization and a rich feature set, making it a strong contender for modern web API design.]
---
```

# FastEndpoints - REPR alternative to Minimal APIs

# In this article

*   [Intro](#Intro)
*   [FastEndpoints](#FastEndpoints)
*   [Getting Started](#GettingStarted)
*   [Basic Usage](#BasicUsage)
*   [Configure](#Configure)
*   [Handling Responses](#Responses)
*   [Examples](#Examples)
*   [Versioning](#Versioning)
*   [Conclusion](#Conclusion)

![FastEndpoints - REPR Alternative to Minimal APIs banner image. The image features the text "FastEndpoints REPR Alternative to Minimal APIs" in white against a blue background with geometric patterns. The NikolaTech logo (NK) is in the top left, and the FastEndpoints logo (F) is in the bottom left.](https://coekcx.github.io/BlogImages/banners/fast-endpoints-repr-minimal-api-alternative-dotnet-banner.png)

#### FastEndpoints - REPR alternative to Minimal APIs

###### 04 Dec 2024

###### 7 min

Traditional controllers were an essential part of web API development in .NET for many years.

However, their verbosity and reliance on attributes often resulted in implementations filled with boilerplate code. As applications grew more complex, controllers frequently became bloated and increasingly difficult to manage.

To address these issues, developers created third-party libraries to enhance endpoint organization and replace traditional controllers. This led to the development of the **REPR Pattern** and the **ApiEndpoints library**, created by Steve Smith (Ardalis).

While ApiEndpoints retained the underlying controller architecture, they provided modularity by separating endpoints into individual classes.

FastEndpoints takes a different approach, while they follow the REPR Pattern, they are built around Minimal APIs instead of traditional controllers.

## FastEndpoints

**FastEndpoints** is an open-source library for .NET Web APIs aiming to replace controllers and minimal APIs.

Performance is on par with Minimal APIs and does noticeably better than controllers in synthetic benchmarks. Problem with Minimal APIs is that they lack endpoint organization which is why FastEndpoints are a compelling alternative.

FastEndpoints follow REPR Design Pattern which makes working with endpoints convenient & maintainable with virtually no boilerplate.

[REPR Pattern](https://deviq.com/design-patterns/repr-design-pattern) emphasizes organizing APIs around individual endpoints rather than grouping logic into centralized controllers.

Each endpoint is implemented as its own class, promoting a single responsibility principle. By designing around endpoints, the API becomes easier to locate, navigate and modify, as each class corresponds directly to a single route.

FastEndpoints are also feature rich providing:

*   Auto discovery & registration of endpoints
*   API versioning
*   Constructor & property injection of endpoint dependencies
*   Supports swagger, serilog etc.
*   Model validation with FluentValidation rules

## Getting Started with FastEndpoints

To get started with FastEndpoints, you need to install the NuGet package. You can do this via the NuGet Package Manager or by running the following command in the Package Manager Console:

```bash
Install-Package FastEndpoints
```

To start using FastEndpoints, you’ll need to add it to your dependency injection setup:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFastEndpoints();

var app = builder.Build();

app.UseFastEndpoints();

app.Run();
```

**AddFastEndpoints** method discovers and registers FastEndpoints in the application's dependency injection container.

**UseFastEndpoints** method integrates FastEndpoints into the HTTP request pipeline, mapping them to the ASP.NET Core middleware pipeline.

## Basic Usage

To define your endpoints, in FastEndpoints there are 4 different endpoint base types you can inherit from depending on your use case:

*   **Endpoint<TRequest, TResponse>** - Use when you have both request and response DTOs.

```csharp
public sealed class CreateEndpoint : Endpoint<CreateRequest, Guid> { ... }
```

*   **Endpoint<TRequest>** - Use when you have only a request DTO.

```csharp
public sealed class DeleteEndpoint : EndpointWithoutRequest { ... }
```

*   **EndpointWithoutRequest<TResponse>** - Use this when a request DTO is not required, but a response DTO is needed.

```csharp
public sealed class GetAllEndpoint : EndpointWithoutRequest<IEnumerable<ProductResponse>>  { ... }
```

*   **EndpointWithoutRequest** - Use this when neither a request DTO nor a response DTO is needed.

```csharp
public sealed class UpdateEndpoint : Endpoint<UpdateRequest> { ... }
```

For full control you can avoid base classes and implement IEndpoint interface directly.

### Configure

Configure method is used to set up the configuration for an endpoint, including its HTTP verb, route, authentication and other behaviors.

*   Verb method defines the HTTP verb (GET, POST, PUT, DELETE, etc.) that the endpoint should respond to.
*   Routes method can define one or more routes for endpoint.
*   Endpoints are secure by default. You'd have to explicitly call AllowAnonymous method in the configuration if unauthenticated access is to be allowed to a particular endpoint.

Here is an example of methods you may need to utilize:

```csharp
public override void Configure()
{
    Put("products/{id:guid}");
    Version(2);
    Claims("Admin");
    Roles("Admin");
    Permissions("CreateProductPermission");
}
```

### Handling Responses

FastEndpoints support multiple approaches as well when it comes to responses.

*   You can directly assign the Response property of the endpoint.
*   You can utilize TypedResults. From .NET 7 you can conditionally return one of multiple results.
*   FastEndpoints provide multiple response sending methods.

### Examples

Without further ado, let's dive into the implementation examples:

```csharp
public sealed record CreateRequest(string Name, string Description, decimal Price);

public sealed class CreateEndpoint(ISender sender) : Endpoint<CreateRequest, Guid>
{
    public override void Configure()
    {
        AllowAnonymous();
        Post("products/");
    }

    public override async Task HandleAsync(CreateRequest request, CancellationToken cancellationToken)
    {
        var command = request.Adapt<CreateProductCommand>();

        var response = await sender.Send(command, cancellationToken);

        if (response.IsSuccess)
        {
            await SendOkAsync(response.Value, cancellationToken);
            return;
        }
        
        await SendErrorsAsync((int)HttpStatusCode.BadRequest, cancellationToken);
    }
}
```

```csharp
public sealed class DeleteEndpoint(ISender sender) : EndpointWithoutRequest
{
    public override void Configure()
    {
        AllowAnonymous();
        Delete("products/{id:guid}");
    }

    public override async Task HandleAsync(CancellationToken cancellationToken)
    {
        var id = Route<Guid>("id");

        var command = new DeleteProductCommand(id);
        var result = await sender.Send(command, cancellationToken);

        if (result.IsNotFound())
        {
            await SendNotFoundAsync(cancellationToken);
            return;
        }
        
        await SendNoContentAsync(cancellationToken);
    }
}
```

In these examples, I have used FastEndpoints methods to return results.

Note that to retrieve a value from the route, we use the **Route<T>** method, specifying the parameter name.

For the create endpoint, we used **Endpoint<TRequest, TResponse>** to define the request and response types.

On the other hand, the delete endpoint doesn’t require a request or response. It simply deletes a product if one exists, which is why we used **EndpointWithoutRequest**.

To download the complete solution with full CRUD functionality, refer to the Conclusion section.

## Versioning

Each endpoint is versioned independently and ultimately grouped into a Swagger document.

To enable versioning simply specify one of the versioning options during startup to activate versioning.

```csharp
app.UseFastEndpoints(c =>
{
    c.Versioning.Prefix = "v";
});
```

When it's time for an endpoint to change, simply leave the existing endpoint and create a brand-new endpoint class and call the **Version** method to indicate its version.

```csharp
public class CreateEndpointV2 : Endpoint<Request, Response>
{
    public override void Configure()
    {
        Get("products");
        Version(2);
    }
}
```

## Conclusion

FastEndpoints is a modern, efficient alternative to traditional controllers and Minimal APIs in .NET.

Its adherence to the REPR pattern ensures maintainable and modular development with minimal boilerplate, while its performance and feature set makes it a strong contender for replacing traditional approaches in web API design.

Feel free to check out the project on GitHub and give it a star: [FastEndpoints Github](https://github.com/FastEndpoints/FastEndpoints)

If you want to check out examples I created, you can find the source code here:

[Source Code](https://www.nikolatech.net/codes/fast-endpoints-example)