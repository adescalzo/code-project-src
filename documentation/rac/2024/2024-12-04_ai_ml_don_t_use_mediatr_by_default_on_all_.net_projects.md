```yaml
---
title: "Don't use MediatR by default on all .NET projects"
source: https://goatreview.com/dont-use-mediatr-by-default-net-projects/?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=lesser-known-clr-gc-handles&_bhlid=46b30461a47be60a285fd3e1c55ba62047e3509b
date_published: 2024-12-04T09:00:59.000Z
date_captured: 2025-08-08T18:06:07.969Z
domain: goatreview.com
author: Unknown
category: ai_ml
technologies: [MediatR, .NET, .NET Core, ASP.NET Core, EFCore, FluentValidation, GitHub]
programming_languages: [C#]
tags: [mediatr, dotnet, software-design, architecture, clean-architecture, design-patterns, dependency-injection, code-quality, over-engineering]
key_concepts: [mediator-pattern, clean-architecture, CQRS, event-driven-architecture, dependency-injection, atomic-operations, service-adapters, over-engineering]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article critically examines the systematic use of MediatR in .NET projects, arguing against its default inclusion in all applications despite its utility for implementing the mediator pattern. It demonstrates how to achieve a use-case-centric architecture without MediatR, providing code examples for comparison. The author highlights potential pitfalls such as event-sharing complexities in atomic contexts and the dangers of over-engineering solutions. The piece advocates for a deliberate approach to library adoption, urging developers to consider genuine project needs over trendy implementations, and provides a GitHub repository for reference.]
---
```

# Don't use MediatR by default on all .NET projects

# Don't use MediatR by default on all .NET projects

![Pierre Belin's profile picture, featuring a goat](https://goatreview.com/content/images/size/w30/2022/10/Ch-vre-Pierre-profil-avec-fond-blanc-zoom.png "Pierre Belin")

Pierre Belin

![Main article image: A reflective goat with a thought bubble containing a digital, abstract symbol](https://goatreview.com/content/images/size/w300/2024/12/pierrebelin_Reflecting_goat_with_a_bubble_next_to_it_containing_b6f74da8-953e-4d4f-97b9-d7afc10e0506.webp "Don't use MediatR by default on all .NET projects")

Table of Contents

1.  [Use case with/without MediatR](#use-case-withwithout-mediatr)
2.  [What's wrong with the systematic use of MediatR?](#whats-wrong-with-the-systematic-use-of-mediatr)
    1.  [The event-sharing problem in an atomic context](#the-event-sharing-problem-in-an-atomic-context)
    2.  [Do not over-engineering solutions](#do-not-over-engineering-solutions)
3.  [Conclusion](#conclusion)

The .NET ecosystem has witnessed a significant shift in recent years, with MediatR becoming almost synonymous with Clean Architecture implementations. This library, designed to implement the mediator pattern, has found its way into countless tutorials, templates, and starter kits, making it seem like an essential component of modern .NET Core applications.

The appeal is understandable - MediatR provides an elegant solution for implementing the mediator pattern, facilitating communication between different layers of an application without direct dependencies.

While MediatR offers powerful features for handling cross-cutting concerns, it shouldn't be treated as a default inclusion in every .NET Core project. Instead, its implementation should be carefully considered based on specific use cases and requirements.

This article demonstrates how to maintain the benefits of a use-case-centric architecture without relying on MediatR's internal messaging system, offering a more streamlined approach that preserves architectural clarity without unnecessary complexity.

⚠️

DISCLAIMER: This article is not intended to criticize the MediatR library, which on the contrary, is very useful. It is a criticism of the systematic use of an external library, which has its advantages, but also its complexities, which must be used in particular contexts.

## Use case with/without MediatR

Before getting started, to achieve the same result as MediatR, it's essential to create the interfaces for commands and handlers to obtain an equivalent structure.

```dotnet
public interface ICommand<T>;
public interface IHandler<in TQuery, TResult> where TQuery : ICommand<TResult>
{
    Task<TResult> Handle(TQuery query);
}
```

The declaration of commands and responses is exactly the same, with the use of `ICommand` created above.

```dotnet
public record AddGoatWithMediatRCommand(string Name, string Description) : IRequest<AddGoatResponse>;
public record AddGoatWithMediatRResponse(Guid GoatId);

// vs without MediatR

public record AddGoatCommand(string Name, string Description) : ICommand<AddGoatResponse>;
public record AddGoatResponse(Guid GoatId);
```

At the handler level, it's much the same. The handler declares an input type (the command) and an output type (the response), with a `Handle` method to handle processing related to the command.

One notable difference is the use of a `CancellationToken`, which can easily be added to the `IHandler` interface declaration.

```dotnet
internal class AddGoatWithMediatRHandler : IRequestHandler<AddGoatWithMediatRCommand, AddGoatResponse>
{
    public Task<AddGoatResponse> Handle(AddGoatWithMediatRCommand request, CancellationToken cancellationToken)
    {
        return Task.FromResult(new AddGoatResponse(Guid.NewGuid()));
    }
}

// vs without MediatR

public interface IAddGoatHandler : IHandler<AddGoatCommand, AddGoatResponse>;
internal class AddGoatHandler : IAddGoatHandler
{
    public Task<AddGoatResponse> Handle(AddGoatCommand query)
    {
        return Task.FromResult(new AddGoatResponse(Guid.NewGuid()));
    }
}
```

The API display is exactly the same for both solutions, with the order sent to the internal messaging system and MediatR, and the handler injected directly to manage the order.

```dotnet
app.MapPost("/goat/with-mediatr", async (
            [FromServices] ISender mediator,
            GoatToCreate input)
        =>
    {
        var command = new AddGoatWithMediatRCommand(input.Name, input.Description);
        return await mediator.Send(command);
    });

// vs without MediatR

app.MapPost("/goat", async (
            [FromServices] IAddGoatHandler handler,
            GoatToCreate input)
        =>
    {
        var command = new AddGoatCommand(input.Name, input.Description);
        return await handler.Handle(command);
    });
```

As far as injection is concerned, MediatR is certainly simpler to use as it stands, since service registration is carried out via the assembly.

```dotnet
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(typeof(Program).Assembly));

// vs without MediatR

builder.Services.AddScoped<IAddGoatHandler, AddGoatHandler>();
```

But there's nothing to stop you building an extension method similar to `RegisterServicesFromAssemblies` to do the same thing.

We end up with 2 almost identical solutions for cutting up an application by use-case, with a folder containing the command, the response and the handler.

💡

I don't deliberately use the term CQRS here, as it's often misused. The notion of CQRS is a read/write database split, which implies that writes do not revise data, which has no connection with the creation of the commands presented here.

The advantage of using this solution is that you can easily switch to MediatR if you need to, by changing only the command and handler interfaces, as well as changing all the declarations in the endpoints (which is quite minimal).

```dotnet
public interface IHandler<in TQuery, TResult> : IRequestHandler<TQuery, TResult> where TQuery : IRequest<TResult>
public interface ICommand<T> : IRequest<T>;
```

## What's wrong with the systematic use of MediatR?

Using the MediatR pattern is not a bad thing, since it creates a decoupling between the exposure and the handler when the command is received.

It becomes dangerous when you also start using it to trigger other commands within a handler to access another context.

And to back it all up, we're going to take a simple example: when I add my goat, I want to add an audit trail to track all the changes.

The first question to ask is: in order for the addition to be validated, must the audit trail also have been performed? In other words, is there an atomic relationship between these two operations?

By atomic, we don't mean database transactions, but rather processing atomicity. Processing can be done synchronously or not, but it must be done in its entirety or not at all. Here, does the creation of an audit condition the creation of a goat?

If the answer is no, then you're right to create decoupling here, since the two operations are independent of each other. At the end of goat creation, all you need to do is `Publish` (and not `Send`) to start recording in another context. This is event-driven architecture in its own right.

If the answer is yes, then you'll be creating decoupling between two operations that are correlated, and this is where the complexity comes in.

### The event-sharing problem in an atomic context

Before getting started, it's important to clarify the difference between the notion of event and command:

*   An event is a notification which may have several handlers and which does not return any results
*   A command is a request that has a single handler and returns a result (void being a result).

Why this clarification? Because we're not here to criticize the notion of events produced in a handler, but rather the use of these events to generate commands.

To return to our example, it seems intuitive to start from the following premise:

*   Goat management is in a context
*   The goat context produces a created goat event
*   The audit trail subscribes to an command related to this event for processing

The slightly more complete handler performs the following steps:

```dotnet
public Task<AddGoatResponse> Handle(AddGoatWithMediatRCommand request, CancellationToken cancellationToken)
{
    // We create the goat : var goatCreated = new GoatCreated....
    // We save it in the database : Repository.Save(goatCreated)
    return Task.FromResult(new AddGoatResponse(Guid.NewGuid()));
}
```

To wire up the audit trail system, we could imagine several systems:

*   Send an event after the `Handle` has been processed, using a `PipelineBehavior` in order to raise a command
*   Send a command while the `Handle` is being processed.

The 1st case is technically the “cleanest”, since it doesn't use the handler's message sending mechanism, delegating it to the pipeline. To do this, we use the events generated in the handler to send commands to other contexts. This solution seems very clean.

Let's imagine the following case: I now want to check that I have access to the credits needed to create a goat.

With case 1, you'll have to create a new subscriber in the credit context to be able to validate that you have enough credits to add a goat.

In terms of performance, this is a disaster: you've run through the entire creation process (search in database, save in database) only to realize that you don't have enough credits because the command to retrieve the number of credits is only triggered once the handler has finished.

You can then fall back on case 2.

```dotnet
public Task<AddGoatResponse> Handle(AddGoatWithMediatRCommand request, CancellationToken cancellationToken)
{
    // We create the goat : var goatCreated = new GoatCreated....
    // We asked here for the credits : var credits = await mediator.Send(new GetCreditsCommand());
    // We save it in the database : Repository.Save(goatCreated)
    return Task.FromResult(new AddGoatResponse(Guid.NewGuid()));
}
```

We end up using an internal messaging call in a handler to call another context.

Can you see the problem?

We end up injecting into a handler an abstraction mechanism between two services that are dependent on each other by using internal messaging. What a mess...

Creating an event-driven application is not for novices. It's a complex infrastructure, with lots of side effects, complex debugging etc... Don't get into it unless you really need it, which will not be the case in 99% of your cases.

### Do not over-engineering solutions

A simple solution is to limit the sending of these messages by using service adapters, which enable communication from one service to another.

Let's go back to the example we used at the beginning to implement use-cases without MediatR.

```dotnet
public Task<AddGoatResponse> Handle(AddGoatWithMediatRCommand request, CancellationToken cancellationToken)
{
    // We create the goat : var goatCreated = new GoatCreated....
    // We asked here for the credits : var credits = creditsService.GetCredits();
    // We save it in the database : Repository.Save(goatCreated)
    return Task.FromResult(new AddGoatResponse(Guid.NewGuid()));
}
```

The credit context has an interface for exposing its methods to other contexts, creating a dependency. But in reality, it's just an exposure, which we could, if need be, transform into an API call to a microservice or similar.

The key here is to maintain an atomic relationship between separate contexts.

## Conclusion

I haven't mentioned here the advantages of using a library like MediatR, specifically to show the complexities of using it without realizing its impact.

The example taken here is MediatR, and it could be applied to all the libraries we're used to using: EFCore, FluentValidation...

Always ask yourself: Am I making this technical choice because it makes me tick as a developer, or because I really need it?

In this case, I was able to completely remove MediatR from my application and return to simple, easy-to-understand development with no external dependencies.

You can find all the code here:

[
![GitHub repository preview for goatreview/BlogArticles](https://opengraph.githubassets.com/f2a709f5ce2d458d792afb2e6433c33be131e9b1c8c3351b502b5800fd093e22/goatreview/BlogArticles)
GitHub - goatreview/BlogArticles at do-not-use-mediator-as-default
Contains all examples of blog articles. Contribute to goatreview/BlogArticles development by creating an account on GitHub.
](https://github.com/goatreview/BlogArticles/tree/do-not-use-mediator-as-default?ref=goatreview.com)

Have a goat day 🐐

---

[.NET](/tag/net/ ".NET") [#MediatR](/tag/mediatr/ "MediatR") [#Software Design](/tag/software-design/ "Software Design")

---

Goat Review

Improve your code review, from 🐐 to GOAT

![Microsoft MVP Badge](https://goatreview.com/assets/icons/mvpbadge.png "MVP Badge")

[](https://twitter.com/goat_review "Twitter")[](https://github.com/goatreview/ "Github")[](/rss "RSS")

Explore topics

[.NET

69

](/tag/net/ ".NET")[Tests

16

](/tag/tests/ "Tests")[Software Design

11

](/tag/software-design/ "Software Design")[AI

11

](/tag/artificial-intelligence/ "AI")[API

7

](/tag/api/ "API")[Architecture

7

](/tag/architecture/ "Architecture")