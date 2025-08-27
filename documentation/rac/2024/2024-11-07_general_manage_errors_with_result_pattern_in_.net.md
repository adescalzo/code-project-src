```yaml
---
title: Manage Errors with Result Pattern in .NET
source: https://www.nikolatech.net/blogs/result-pattern-manage-errors-in-dotnet
date_published: 2024-11-07T06:00:00.000Z
date_captured: 2025-08-06T17:46:10.331Z
domain: www.nikolatech.net
author: Unknown
category: general
technologies: [.NET, ASP.NET Core, Entity Framework Core, ardalis/Result, amantinband/Error-Or, vkhorikov/CSharpFunctionalExtensions, YoussefSell/Result.Net, altmann/FluentResults]
programming_languages: [C#]
tags: [error-handling, result-pattern, dotnet, functional-programming, exception-handling, design-patterns, csharp, libraries, clean-code]
key_concepts: [Result Pattern, Exception Handling, Functional Programming, Custom Implementation, Third-party Libraries, Error Propagation, Middleware, Domain-Driven Design]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores the Result Pattern in .NET as an alternative to traditional exception handling, drawing influence from functional programming. It provides a detailed guide on how to implement a custom generic Result type, including an Error record and a Unit struct for void returns. The post also reviews several popular third-party libraries such as ErrorOr, FluentResults, and CSharpFunctionalExtensions, highlighting their features and use cases. The author discusses the benefits of the Result Pattern, such as clearer error context and reduced overhead, while acknowledging that exceptions still need to be handled within an application. The article concludes by advising careful consideration for its implementation and mentions the role of middlewares in centralizing error handling.
---
```

# Manage Errors with Result Pattern in .NET

![Banner: A blue and white banner with the text "RESULT PATTERN" at the top and "Manage Errors in .NET" below it. The banner also features an "NK" logo in the top left and an icon representing a flowchart with "200" and "404" boxes at the bottom left, symbolizing success and error states.]

#### Manage Errors with Result Pattern in .NET

The **Result Pattern** is becoming increasingly popular, especially in the .NET ecosystem.

It's influenced by functional programming and represents an interesting alternative to traditional exception handling. The idea is to enhance flow control by explicitly representing the outcomes of operations, such as success or failure without relying on exceptions.

This pattern encapsulates both result data and status in a single object, allowing developers to easily check the outcome and decide how to proceed.

In .NET, there is no built-in Result type. However, you can easily implement your own custom Result type or use a third-party library that provides this functionality.

In today's blog, we’ll cover both custom and third-party approaches to the Result Pattern.

Without further ado, let's move on to the implementation.

## Manual Implementation

While there are great third-party libraries available, a manual implementation is straightforward and gives you more control to tailor the solution to your specific needs.

Additionally, since it's easy to implement, minimizing dependencies in your project is often a practical choice.

First you need to define the Result type. It's important to remember that there's no single solution, different implementations can still effectively help you achieve your goals.

For example, you could design a generic Result type that includes generics for both the successful path and the failure path, allowing it to accept various error types.

Alternatively, you might opt for a simpler approach by accepting a single error type and using a generic property solely for the successful outcome.

Here's a simple example of what a Result type might look like:

```csharp
public sealed class Result<T>
{
    public T? Value { get; }

    public Error? Error { get; }

    public bool IsSuccess { get; }

    public bool IsError => !IsSuccess;

    private Result(T value)
    {
        Value = value ?? throw new ArgumentNullException(nameof(value), "Value cannot be null.");
        IsSuccess = true;
        Error = null;
    }

    private Result(Error error)
    {
        Error = error ?? throw new ArgumentNullException(nameof(error), "Error cannot be null.");
        IsSuccess = false;
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(Error error) => new(error);

    public TResult Match<TResult>(Func<T, TResult> onSuccess, Func<Error, TResult> onError)
    {
        return IsSuccess ? onSuccess(Value!) : onError(Error!);
    }
}
```

My Result class can store either a success value of type **T** or an **Error**, with **IsSuccess** and **IsError** properties to indicate the result's state.

The **Success()** and **Failure()** methods are used to return a Result based on whether the outcome is a success or a failure.

The **Match()** method enables branching logic by offering distinct handling for success and error cases, allowing the caller to manage each outcome individually.

The **Error** standardizes error handling by providing structured data for different error types. This approach ensures consistent errors that are easy to understand and use across the application.

```csharp
public sealed record Error(int Code, string Description)
{
    public static Error ProductNotFound => new(100, "Product not found");

    public static Error ProductBadRequest => new(101, "Product bad request");
}
```

Additionally, I've created a **Unit** struct to represent a void return type for cases where we want to indicate success without returning a value.

```csharp
public struct Unit
{
    public static readonly Unit Value = new Unit();
}
```

Here’s an example of the Result Pattern in action:

```csharp
public async Task<Result<Guid>> Handle(Command request, CancellationToken cancellationToken)
{
    var validationResult = await _validator.ValidateAsync(request, cancellationToken);
    if (!validationResult.IsValid)
    {
        return Result<Guid>.Failure(Error.ProductBadRequest);
    }

    var product = new Product(
        Guid.NewGuid(),
        DateTime.UtcNow,
        request.Name,
        request.Description,
        request.Price);

    _dbContext.Products.Add(product);

    await _dbContext.SaveChangesAsync(cancellationToken);

    return Result<Guid>.Success(product.Id);
}
```

```csharp
app.MapPost("products/", async (
    ISender sender,
    CreateRequest request,
    CancellationToken cancellationToken) =>
{
    var command = request.Adapt<Command>();

    var response = await sender.Send(command, cancellationToken);

    return response.Match(
        x => Results.Ok(x),
        error => Results.BadRequest(error)
    );
});
```

## Third-Party Libraries

.NET offers a variety of great third-party libraries for implementing the Result Pattern:

*   [ardalis/Result](https://github.com/ardalis/Result)
*   [amantinband/Error-Or](https://github.com/amantinband/error-or)
*   [vkhorikov/CSharpFunctionalExtensions](https://github.com/vkhorikov/CSharpFunctionalExtensions)
*   [YoussefSell/Result.Net](https://github.com/YoussefSell/Result.Net)
*   [altmann/FluentResults](https://github.com/altmann/FluentResults)

Unfortunately, I haven't had the chance to try them all, but here are my thoughts on ErrorOr, FluentResult and CSharpFunctionalExtensions.

Both **ErrorOr** and **FluentResult** are excellent choices when starting a project from scratch. They support multiple errors instead of just one, offer various extension methods, provide a fluent interface and more.

But what if you already have a project with many exceptions and want to transition to the Result Pattern?

That's where CSharpFunctionalExtensions excels, offering the easiest transition possible.

**CSharpFunctionalExtensions** is a popular library in C# created to bring functional programming concepts to the language.

The Result type is one them, but what sets this library apart is its ability to wrap exceptions automatically. Instead of refactoring the whole codebase to remove Exceptions you can simply wrap them in a Result using **Result.Try()** method.

Here is simple example:

```csharp
await _validator.ValidateAndThrowAsync(request, cancellationToken);
```

Instead of handling exceptions, you can wrap them like this:

```csharp
var result = await Result.Try(async () => await _validator.ValidateAndThrowAsync(request, cancellationToken));
if (result.IsFailure)
{
    return result;
}
```

This method executes code that may throw an exception, and if one occurs, it catches the exception and returns a failure containing the exception message.

You can achieve similar results with other libraries as well, but this one perfectly suited my use case.

## Conclusion

The Result Pattern is not a silver bullet, nor are exceptions inherently bad.

It is a valuable tool, especially in scenarios requiring clear error handling.

Result type often include detailed context about the error, such as messages or specific error types. On top of that exceptions can indeed introduce additional overhead.

However, even if you choose to use only the Result Pattern, your code will still need to handle exceptions, as frameworks and many libraries you may use will throw them.

The Result Pattern requires implementing conditional logic, which can become increasingly complex when handling multiple potential errors. When an error occurs, it must be propagated to the top, requiring validation at each level.

Additionally, if you can implement middlewares, you can centralize error handling and maintain a clear separation of concerns, distinguishing business logic from error handling logic.

The key takeaway is to carefully consider whether and how you want to implement the Result Pattern.

If you want to check out examples I created, you can find the source code here:

[Source Code](https://www.nikolatech.net/codes/result-pattern-example)

I hope you enjoyed it, subscribe and get a notification when new a blog is up!