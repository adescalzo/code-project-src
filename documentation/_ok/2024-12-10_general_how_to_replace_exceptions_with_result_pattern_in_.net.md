```yaml
---
title: How To Replace Exceptions with Result Pattern in .NET
source: https://antondevtips.com/blog/how-to-replace-exceptions-with-result-pattern-in-dotnet
date_published: 2024-12-10T08:55:21.852Z
date_captured: 2025-08-06T17:34:47.458Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [.NET, ASP.NET Core, Entity Framework, FluentResults, CSharpFunctionalExtensions, error-or, Ardalis Result]
programming_languages: [C#]
tags: [error-handling, result-pattern, design-patterns, dotnet, csharp, exceptions, clean-code, web-api, performance, software-architecture]
key_concepts: [Result Pattern, Exception Handling, Global Exception Handling, Domain-Driven Design, Control Flow, Error-Or, Minimal API, Code Readability]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores how to replace traditional exception handling with the Result pattern in .NET applications to improve code readability, maintainability, and performance. It demonstrates implementing a custom Result object and showcases the popular `error-or` NuGet package for a more robust solution. The article provides practical C# code examples for both exception-based and Result-pattern-based error handling in ASP.NET Core Minimal APIs. It also discusses the pros and cons of the Result pattern and identifies specific scenarios where exceptions remain appropriate, such as global exception handling, library code, and domain entity guard validation.
---
```

# How To Replace Exceptions with Result Pattern in .NET

![A dark blue background with abstract purple shapes. On the left, a white square icon with a code tag symbol (`</>`) and the text "dev tips" below it. On the right, large white text reads "HOW TO REPLACE EXCEPTIONS WITH RESULT PATTERN IN .NET".](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_asp_result_patterns.png&w=3840&q=100)

# How To Replace Exceptions with Result Pattern in .NET

Dec 10, 2024

[Download source code](/source-code/how-to-replace-exceptions-with-result-pattern-in-dotnet)

6 min read

### Newsletter Sponsors

[Master The Clean Architecture](https://www.courses.milanjovanovic.tech/a/aff_q2snzxhl/external?affcode=1486372_j2vpyytw). This comprehensive course will teach you how to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture. Join 3,150+ students to accelerate your growth as a software architect.

[Master The Modular Monolith Architecture](https://www.courses.milanjovanovic.tech/a/aff_l825rpg0/external?affcode=1486372_j2vpyytw). This in-depth course will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario. Join 1,050+ students to accelerate your growth as a software architect.

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

In modern software development, handling errors and exceptional scenarios gracefully is crucial for building robust applications. While exceptions are a common mechanism in .NET for error handling, they can introduce performance overhead and complicate code flow.

Today we will explore how to replace exceptions with the **Result pattern** in .NET, enhancing code readability, maintainability, and performance.

## Introduction to Exception Handling in .NET

Exception handling is a fundamental concept in .NET programming, allowing developers to manage runtime errors gracefully. The typical approach involves using try, catch, and finally blocks to capture and handle exceptions.

Let's explore an application that creates a Shipment and uses exceptions for control flow:

```csharp
public async Task<ShipmentResponse> CreateAsync(
    CreateShipmentCommand request,
    CancellationToken cancellationToken)
{
    var shipmentAlreadyExists = await context.Shipments
        .Where(s => s.OrderId == request.OrderId)
        .AnyAsync(cancellationToken);

    if (shipmentAlreadyExists)
    {
        throw new ShipmentAlreadyExistsException(request.OrderId);
    }

    var shipment = request.MapToShipment(shipmentNumber);
    
    context.Shipments.Add(shipment);
    await context.SaveChangesAsync(cancellationToken);

    return shipment.MapToResponse();
}
```

Here `ShipmentAlreadyExistsException` is thrown if a shipment already exists in the database. In the Minimal API endpoint, this exception is handled as follows:

```csharp
public void MapEndpoint(WebApplication app)
{
    app.MapPost("/api/v1/shipments", Handle);
}

private static async Task<IResult> Handle(
    [FromBody] CreateShipmentRequest request,
    IShipmentService service,
    CancellationToken cancellationToken)
{
    try
    {
        var command = request.MapToCommand();
        var response = await service.CreateAsync(command, cancellationToken);
        return Results.Ok(response);
    }
    catch (ShipmentAlreadyExistsException ex)
    {
        return Results.Conflict(new { message = ex.Message });
    }
}
```

While this approach works, it has the following drawbacks:

*   code is unpredictable, by looking at `IShipmentService.CreateAsync` you can't know for sure if the method throws exceptions or not
*   you need to use catch statements, and you can no longer read code from top to bottom line-by-line, you need to jump your view back and forward
*   exceptions can lead to performance issues in some applications as they are pretty slow (they became faster in recent .NET 9 but still they are slow)

Remember, exceptions are for exceptional situations. They are not the best option for a control flow. Instead, I want to show you a better approach with a **Result Pattern**.

## Understanding the Result Pattern

The Result pattern is a design pattern that encapsulates the result of an operation, which can either be a success or a failure. Instead of throwing exceptions, methods return a `Result` object indicating whether the operation succeeded or failed, together with any relevant data or error messages.

**Result Object consists of the following parts**:

*   IsSuccess/IsError: A boolean indicating if the operation was successful or not.
*   Value: The result value when the operation is successful.
*   Error: An error message or object when the operation fails.

Let's explore a simple example on how to implement a `Result` object:

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string? Error { get; }

    private Result(bool isSuccess, T? value, string? error)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, value, null);
    }

    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default(T), error);
    }
}
```

Here we have defined a generic `Result` class that can hold either a successful value or an error. To create `Result` object you can use either `Success` or `Failure` static method.

Let's rewrite the previous `Create Shipment` endpoint implementation with our Result pattern:

```csharp
public async Task<Result<ShipmentResponse>> CreateAsync(
    CreateShipmentCommand request,
    CancellationToken cancellationToken)
{
    var shipmentAlreadyExists = await context.Shipments
        .Where(s => s.OrderId == request.OrderId)
        .AnyAsync(cancellationToken);

    if (shipmentAlreadyExists)
    {
        return Result.Failure<ShipmentResponse>($"Shipment for order '{request.OrderId}' is already created");
    }

    var shipment = request.MapToShipment(shipmentNumber);
    
    context.Shipments.Add(shipment);
    await context.SaveChangesAsync(cancellationToken);

    var response = shipment.MapToResponse();
    return Result.Success(response);
}
```

Here the method returns `Result<ShipmentResponse>` that wraps `ShipmentResponse` inside a Result class.

When a shipment already exists in the database, we return `Result.Failure` with a corresponding message. When a request succeeds, we return `Result.Success`.

Here is how you handle a Result Object in the endpoint:

```csharp
public void MapEndpoint(WebApplication app)
{
    app.MapPost("/api/v1/shipments", Handle);
}

private static async Task<IResult> Handle(
    [FromBody] CreateShipmentRequest request,
    IShipmentService service,
    CancellationToken cancellationToken)
{
    var command = request.MapToCommand();
    var response = await service.CreateAsync(command, cancellationToken);
    
    return response.IsSuccess ? Results.Ok(response.Value) : Results.Conflict(response.Error);
}
```

You need to check if the response is successful or failed and return an appropriate HTTP result. Now the code looks more predictable and reads more easily, right?

The current `Result` object implementation is very simplified, in real applications you need more features within it. You can spend some time and build one for you and reuse in all projects. Or you can use a ready Nuget package.

There are a plenty Nuget packages with **Result Pattern** implementation, let me introduce you to a few most popular:

*   [FluentResults](https://github.com/altmann/FluentResults)
*   [CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions)
*   [error-or](https://github.com/amantinband/error-or)
*   [Ardalis Result](https://github.com/ardalis/Result)

My favourite one is `error-or`, let's explore it.

## Result Pattern with Error-Or

As author of the package stated: **Error-Or** is a simple, fluent discriminated union of an error or a result. Result class in this library is called `ErrorOr<T>`.

Here is how you can use it for control flow:

```csharp
public async Task<ErrorOr<ShipmentResponse>> Handle(
    CreateShipmentCommand request,
    CancellationToken cancellationToken)
{
    var shipmentAlreadyExists = await context.Shipments
        .Where(s => s.OrderId == request.OrderId)
        .AnyAsync(cancellationToken);

    if (shipmentAlreadyExists)
    {
        return Error.Conflict($"Shipment for order '{request.OrderId}' is already created");
    }

    var shipment = request.MapToShipment(shipmentNumber);

    context.Shipments.Add(shipment);
    await context.SaveChangesAsync(cancellationToken);

    return shipment.MapToResponse();
}
```

Here the return type is `ErrorOr<ShipmentResponse>` that indicates whether an error is returned or `ShipmentResponse`.

`Error` class has built-in errors for the following types, providing a method for each error type:

```csharp
public enum ErrorType
{
    Failure,
    Unexpected,
    Validation,
    Conflict,
    NotFound,
    Unauthorized,
    Forbidden,
}
```

The library allows you to create custom errors if needed.

In the API endpoint you can handle the error similar to what we did before with our custom `Result` Object:

```csharp
public void MapEndpoint(WebApplication app)
{
    app.MapPost("/api/v2/shipments", Handle);
}

private static async Task<IResult> Handle(
    [FromBody] CreateShipmentRequest request,
    IShipmentService service,
    CancellationToken cancellationToken)
{
    var command = request.MapToCommand();

    var response = await mediator.Send(command, cancellationToken);
    if (response.IsError)
    {
        return Results.Conflict(response.Errors);
    }

    return Results.Ok(response.Value);
}
```

When working with **Error-Or** I like creating a static method `ToProblem` that transforms error to the appropriate status code:

```csharp
public static class EndpointResultsExtensions
{
    public static IResult ToProblem(this List<Error> errors)
    {
        if (errors.Count == 0)
        {
            return Results.Problem();
        }

        return CreateProblem(errors);
    }

    private static IResult CreateProblem(List<Error> errors)
    {
        var statusCode = errors.First().Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.ValidationProblem(errors.ToDictionary(k => k.Code, v => new[] { v.Description }),
            statusCode: statusCode);
    }
}
```

This way a previous code will be transformed to:

```csharp
var response = await mediator.Send(command, cancellationToken);
if (response.IsError)
{
    return response.Errors.ToProblem();
}

return Results.Ok(response.Value);
```

## Pros and Cons of Using the Result Pattern

**Benefits of Using the Result Pattern:**

*   Explicit Error Handling: the caller must handle the success and failure cases explicitly. From the method signature, it's obvious that an error may be returned.
*   Improved Performance: reduces the overhead associated with exceptions.
*   Better Testing: simplifies unit testing as it's much easier to mock Result Object than throwing and handling exceptions.
*   Safety: a result object should contain information that can be exposed to the outside world. While you can save all the details using Logger or other tools.

**Potential Drawbacks:**

*   Verbosity: can introduce more code compared to using exceptions as you need to mark all methods in the stacktrace to return Result Object
*   Not Suitable for All Cases: exceptions are still appropriate for truly exceptional situations that are not expected during normal operation.

Result pattern looks great, but should we just forget about exceptions? Absolutely not! Exceptions still have their usage. Let's discuss it.

## When To Use Exceptions

Exceptions are for exceptional cases, and I see the following use cases where they might be a good fit:

*   Global Exception Handling
*   Library Code
*   Domain Entities Guard Validation

Let's have a closer look at these 3 cases:

### Global Exception Handling

In your asp.net core applications, you definitely need to handle exceptions. They can be thrown anywhere: database access, network calls, I/O operations, libraries, etc.

You need to be prepared that exceptions will occur and handle them gracefully. For this I implement `IExceptionHandler` which is available starting from .NET 8:

```csharp
internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Server error"
        };

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        return true;
    }
}
```

### Library Code

In libraries, usually exceptions are thrown when something goes wrong.

Here are the reasons for this:

*   most of the developers are familiar with exceptions and know how to handle them
*   libraries don't want to be opinionated and use a one or another Result Pattern library or implement their own version

But remember when building libraries use exceptions as the last available option. Often it is better to return a null, an empty collection, boolean false value than throwing an exception.

### Domain Entities Guard Validation

When following Domain-Driven Design Principles (DDD), you construct your domain models using constructors or factory methods. If you pass data for Domain Object creation and this data is invalid (but it should never be) - you may throw an exception. This will be an indication that your input validation, mapping or other application layers have a bug which should be addressed.

## Summary

Replacing exceptions with the Result pattern in .NET can lead to more robust and maintainable code.

By explicitly handling success and failure cases, developers can write clearer code that is easier to test and understand.

While it may not be suitable for all scenarios, incorporating the Result pattern can significantly improve error handling in your applications.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-replace-exceptions-with-result-pattern-in-dotnet)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-replace-exceptions-with-result-pattern-in-dotnet&title=How%20To%20Replace%20Exceptions%20with%20Result%20Pattern%20in%20.NET)[X](https://twitter.com/intent/tweet?text=How%20To%20Replace%20Exceptions%20with%20Result%20Pattern%20in%20.NET&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-replace-exceptions-with-result-pattern-in-dotnet)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-replace-exceptions-with-result-pattern-in-dotnet)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.