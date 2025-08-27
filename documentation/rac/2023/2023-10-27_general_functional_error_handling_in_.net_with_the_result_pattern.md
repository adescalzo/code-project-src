```yaml
---
title: Functional Error Handling in .NET With the Result Pattern
source: https://www.milanjovanovic.tech/blog/functional-error-handling-in-dotnet-with-the-result-pattern
date_published: 2023-10-28T00:00:00.000Z
date_captured: 2025-08-18T13:38:22.427Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: general
technologies: [.NET, .NET MAUI, ASP.NET Core, FluentResults]
programming_languages: [C#]
tags: [error-handling, result-pattern, functional-programming, dotnet, csharp, exceptions, api-design, clean-code]
key_concepts: [result-pattern, functional-error-handling, exception-handling, fail-fast-principle, api-response-transformation, extension-methods]
code_examples: true
difficulty_level: intermediate
summary: |
  This article discusses a functional approach to error handling in .NET using the Result pattern, advocating for its use over exceptions for expected failures. It explains why exceptions should be reserved for truly exceptional situations, making code harder to reason about when used for flow control. The author demonstrates how to implement custom `Error` and `Result` classes, refactor a service to return `Result` objects, and integrate this pattern with ASP.NET Core Minimal APIs for concise error handling. This pattern enhances code expressiveness, testability, and maintainability by explicitly indicating potential method failures.
---
```

# Functional Error Handling in .NET With the Result Pattern

![Functional Error Handling in .NET With the Result Pattern](/blog-covers/mnw_061.png?imwidth=3840)

# Functional Error Handling in .NET With the Result Pattern

6 min read · October 28, 2023

[**Progress**](https://www.telerik.com/campaigns/maui/migration-to-dot-net-maui?utm_medium=cpm&utm_source=milanjovanovic&utm_campaign=maui-ebook-mnm-na) prepared a free eBook for you - [**Migration to .NET MAUI.**](https://www.telerik.com/campaigns/maui/migration-to-dot-net-maui?utm_medium=cpm&utm_source=milanjovanovic&utm_campaign=maui-ebook-mnm-na) Do you plan to start your first cross-platform app with .NET MAUI? You're in the right place. This eBook will help you examine the benefits of using .NET MAUI for the dev experience, the app and its end users, plus any downsides. [**Grab the free MAUI eBook here.**](https://www.telerik.com/campaigns/maui/migration-to-dot-net-maui?utm_medium=cpm&utm_source=milanjovanovic&utm_campaign=maui-ebook-mnm-na)

[**IcePanel**](https://u.icepanel.io/1eec0e6f) is a collaborative C4 model modelling & diagramming tool that helps explain complex software systems. With an interactive map, you can align your software engineering & product teams on technical decisions across the business. [**Check it out here.**](https://u.icepanel.io/1eec0e6f)

[

Sponsor this newsletter

](/sponsor-the-newsletter)

How should you handle errors in your code?

This has been a topic of many discussions, and I want to share my opinion.

One school of thought suggests using exceptions for flow control. This is not a good approach because it makes the code harder to reason about. The caller must know the implementation details and which exceptions to handle.

Exceptions are for exceptional situations.

Today, I want to show you how to implement error handling using the **Result pattern.**

It's a functional approach to error handling, making your code more expressive.

## Exceptions For Flow Control

Using exceptions for flow control is an approach to implement the **fail-fast** principle.

As soon as you encounter an error in the code, you throw an exception — effectively terminating the method, and making the caller responsible for handling the exception.

The problem is the caller must know which exceptions to handle. And this isn't obvious from the method signature alone.

Another common use case is throwing exceptions for validation errors.

Here's an example in the `FollowerService`:

```csharp
public sealed class FollowerService
{
    private readonly IFollowerRepository _followerRepository;

    public FollowerService(IFollowerRepository followerRepository)
    {
        _followerRepository = followerRepository;
    }

    public async Task StartFollowingAsync(
        User user,
        User followed,
        DateTime createdOnUtc,
        CancellationToken cancellationToken = default)
    {
        if (user.Id == followed.Id)
        {
            throw new DomainException("Can't follow yourself");
        }

        if (!followed.HasPublicProfile)
        {
            throw new DomainException("Can't follow non-public profile");
        }

        if (await _followerRepository.IsAlreadyFollowingAsync(
                user.Id,
                followed.Id,
                cancellationToken))
        {
            throw new DomainException("Already following");
        }

        var follower = Follower.Create(user.Id, followed.Id, createdOnUtc);

        _followerRepository.Insert(follower);
    }
}
```

## Use Exceptions for Exceptional Situations

A rule of thumb I follow is to use exceptions for exceptional situations. Since you already expect potential errors, why not make it explicit?

You can group all application errors into two groups:

*   Errors you know how to handle
*   Errors you don't know how to handle

Exceptions are an excellent solution for the errors you don't know how to handle. And you should catch and handle them at the lowest level possible.

What about the errors you know how to handle?

You can handle them in a functional way with the **Result pattern.** It's explicit and clearly expresses the intent that the method can fail. The drawback is the caller has to manually check if the operation failed.

## Expressing Errors Using the Result Pattern

The first thing you will need is an `Error` class to represent application errors.

*   `Code` - unique name for the error in the application
*   `Description` - contains developer-friendly details about the error

```csharp
public sealed record Error(string Code, string Description)
{
    public static readonly Error None = new(string.Empty, string.Empty);
}
```

Then, you can implement the `Result` class using the `Error` to describe the failure. This implementation is very bare-bones, and you could add many more features. In most cases, you also need a generic `Result<T>` class, which will wrap a value inside.

Here's what the `Result` class looks like:

```csharp
public class Result
{
    private Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);
}
```

The only way to create a `Result` instance is by using static methods:

*   `Success` - creates a success result
*   `Failure` - creates a failure result with the specified `Error`

If you want to avoid building your own `Result` class, take a look at the [FluentResults](https://github.com/altmann/FluentResults) library.

## Applying the Result Pattern

Now that we have the `Result` class let's see how to apply it in practice.

Here's a refactored version of the `FollowerService`. Notice a few things:

*   No more throwing exceptions
*   The `Result` return type is explicit
*   It's clear which errors the method returns

Another benefit of error handling using the **Result pattern** is that it's easier to test.

```csharp
public sealed class FollowerService
{
    private readonly IFollowerRepository _followerRepository;

    public FollowerService(IFollowerRepository followerRepository)
    {
        _followerRepository = followerRepository;
    }

    public async Task<Result> StartFollowingAsync(
        User user,
        User followed,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        if (user.Id == followed.Id)
        {
            return Result.Failure(FollowerErrors.SameUser);
        }

        if (!followed.HasPublicProfile)
        {
            return Result.Failure(FollowerErrors.NonPublicProfile);
        }

        if (await _followerRepository.IsAlreadyFollowingAsync(
                user.Id,
                followed.Id,
                cancellationToken))
        {
            return Result.Failure(FollowerErrors.AlreadyFollowing);
        }

        var follower = Follower.Create(user.Id, followed.Id, utcNow);

        _followerRepository.Insert(follower);

        return Result.Success();
    }
}
```

## Documenting Application Errors

You can use the `Error` class to document all possible errors in your application.

One approach is to create a static class called `Errors`. It will have nested classes inside containing the specific errors. The usage would look like `Errors.Followers.NonPublicProfile`.

However, the approach I like to use is to create a specific class containing the errors.

Here's the `FollowerErrors` class documenting the possible errors for the `Follower` entity:

```csharp
public static class FollowerErrors
{
    public static readonly Error SameUser = new Error(
        "Followers.SameUser", "Can't follow yourself");

    public static readonly Error NonPublicProfile = new Error(
        "Followers.NonPublicProfile", "Can't follow non-public profiles");

    public static readonly Error AlreadyFollowing = new Error(
        "Followers.AlreadyFollowing", "Already following");
}
```

Instead of static fields, you can also use static methods returning an error. You would call this method with a concrete argument to get an `Error` instance.

```csharp
public static class FollowerErrors
{
    public static Error NotFound(Guid id) => new Error(
        "Followers.NotFound", $"The follower with Id '{id}' was not found");
}
```

## Converting Results Into API Responses

The `Result` object will eventually reach the Minimal API (or controller) endpoint in ASP.NET Core. Minimal APIs return an `IResult` response, and controllers return an `IActionResult` response. Regardless, you must convert the `Result` instance into a valid API response.

The straightforward approach is checking the `Result` state and returning an HTTP response. Here's an example where we check the `Result.IsFailure` flag:

```csharp
app.MapPost(
    "users/{userId}/follow/{followedId}",
    (Guid userId, Guid followedId, FollowerService followerService) =>
    {
        var result = await followerService.StartFollowingAsync(
            userId,
            followedId,
            DateTime.UtcNow);

        if (result.IsFailure)
        {
            return Results.BadRequest(result.Error);
        }

        return Results.NoContent();
    });
```

However, this is an excellent opportunity for a more functional approach. You can implement the `Match` extension method to provide a callback for each `Result` state. The `Match` method will execute the respective callback and return the result.

Here's the implementation of `Match`:

```csharp
public static class ResultExtensions
{
    public static T Match<T>(
        this Result result,
        Func<T> onSuccess,
        Func<Error, T> onFailure)
    {
        return result.IsSuccess ? onSuccess() : onFailure(result.Error);
    }
}
```

And this is how you would use the `Match` method in a Minimal API endpoint:

```csharp
app.MapPost(
    "users/{userId}/follow/{followedId}",
    (Guid userId, Guid followedId, FollowerService followerService) =>
    {
        var result = await followerService.StartFollowingAsync(
            userId,
            followedId,
            DateTime.UtcNow);

        return result.Match(
            onSuccess: () => Results.NoContent(),
            onFailure: error => Results.BadRequest(error));
    });
```

Much more concise. Don't you think so?

## Summary

If you take one thing with you from this week's issue, it should be this: exceptions are for exceptional situations. Moreover, you should only use exceptions for errors you don't know how to handle. In all other cases, expressing the error clearly with the **Result pattern** is more valuable.

Using the `Result` class allows you to:

*   Express the intent that a method _could_ fail
*   Encapsulate an application error inside
*   Provide a functional way to handle errors

Additionally, you can document all application errors with the `Error` class. This is helpful for developers to know which errors they need to handle.

You can even convert this to actual _documentation_. For example, I wrote a simple program that scans the project for all `Error` fields. It then converts this into a table format and uploads it to a Confluence page.

So I encourage you to try the **Result pattern** and see how it can improve your code.

See you next week.

---

Whenever you're ready, there are 4 ways I can help you:

1.  [Pragmatic Clean Architecture:](/pragmatic-clean-architecture) Join 4,200+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [Modular Monolith Architecture:](/modular-monolith-architecture) Join 2,100+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [(NEW) Pragmatic REST APIs:](/pragmatic-rest-apis) Join 1,200+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
4.  [Patreon Community:](https://www.patreon.com/milanjovanovic) Join a community of 5,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Become a Better .NET Software Engineer

Join 70,000+ engineers who are improving their skills every Saturday morning.

Join 70K+ Engineers

Accelerate Your .NET Skills 🚀

![Cover image for the "Pragmatic Clean Architecture" course, featuring a diagram of architectural layers (Endpoints, Presentation, Application, Domain, DB, Commands, Queries, External Services, Infrastructure) and a photo of Milan Jovanović.](/_next/static/media/cover.27333f2f.png?imwidth=384)

[Pragmatic Clean Architecture](/pragmatic-clean-architecture?utm_source=article_page)

![Cover image for the "Modular Monolith Architecture" course.](/_next/static/media/cover.31e11f05.png?imwidth=384)

[Modular Monolith Architecture](/modular-monolith-architecture?utm_source=article_page)

![Cover image for the "Pragmatic REST APIs" course.](/_next/static/media/cover_1.fc0deb78.png?imwidth=384)

[Pragmatic REST APIs

NEW](/pragmatic-rest-apis?utm_source=article_page)