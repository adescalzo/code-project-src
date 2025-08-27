```yaml
---
title: Refactoring From an Anemic Domain Model To a Rich Domain Model
source: https://www.milanjovanovic.tech/blog/refactoring-from-an-anemic-domain-model-to-a-rich-domain-model
date_published: 2023-06-17T00:00:00.000Z
date_captured: 2025-08-20T15:11:11.539Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: ai_ml
technologies: [ABP Commercial, ABP Framework, ASP.NET Core, IcePanel, .NET]
programming_languages: [C#]
tags: [domain-driven-design, refactoring, anemic-domain-model, rich-domain-model, domain-events, software-architecture, clean-code, csharp, dotnet, encapsulation]
key_concepts: [Anemic Domain Model, Rich Domain Model, Domain-Driven Design, Encapsulation, Result Object Pattern, Domain Events, Repository Pattern, Unit of Work]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article details the process of refactoring from an anemic domain model to a rich domain model, emphasizing the benefits of encapsulating business logic within domain entities. It illustrates common pitfalls of anemic models, such as scattered logic and lack of encapsulation, using C# code examples. The refactoring steps involve defining proper constructors, hiding internal collections, and moving validation rules into domain methods, advocating for the use of result objects over exceptions. Furthermore, the article demonstrates how to leverage domain events to handle side effects asynchronously, ensuring atomicity and decoupling components. It concludes by highlighting that this transition is a gradual process, advising a pragmatic approach based on the application's complexity.]
---
```

# Refactoring From an Anemic Domain Model To a Rich Domain Model

![Refactoring From an Anemic Domain Model To a Rich Domain Model](https://www.milanjovanovic.tech/blog-covers/mnw_042.png?imwidth=3840)

# Refactoring From an Anemic Domain Model To a Rich Domain Model

7 min read · June 17, 2023

Today's issue is sponsored by [**IcePanel.**](https://u.icepanel.io/1eec0e6f) IcePanel is a collaborative [**C4 model modelling & diagramming tool**](https://u.icepanel.io/1eec0e6f) that helps explain complex software systems. With an interactive map, you can align your software engineering & product teams on technical decisions across the business.

And by [**ABP Commercial.**](https://commercial.abp.io/pricing?utm_source=milanw&utm_medium=w42&utm_campaign=newsletter-sponsorship) ABP Commercial is a complete [**web development platform built on ABP Framework**](https://commercial.abp.io/pricing?utm_source=milanw&utm_medium=w42&utm_campaign=newsletter-sponsorship), perfect for enterprise-grade ASP.NET Core based web applications. Pre-built application modules, advanced startup templates, rapid application development tooling, professional UI themes and premium support.

[

Sponsor this newsletter

](/sponsor-the-newsletter)

Is the **anemic domain model** an **antipattern**?

It's a domain model without any behavior and only data properties.

Anemic domain models work great in simple applications, but they are difficult to maintain and evolve if you have rich business logic.

The important parts of your business logic and rules end up being scattered all over the application. It reduces cohesiveness and reusability, and makes adding new features more difficult.

**Rich domain model** attempts to solve this by encapsulating as much of the business logic as possible.

But how can you design a **rich domain model**?

This is a never-ending process of moving business logic into the domain and refining your domain model.

Let's see how to **refactor** from an **anemic domain model** to a **rich domain model**.

## Working With Anemic Domain Model

To understand what working with an **anemic domain model** looks like, I'll use an example of handling a `SendInvitationCommand`.

I omitted the class and its dependencies so that we can focus on the `Handle` method. It loads some entities from the database, performs validation, executes the business logic, and finally persists the changes in the database and sends an email.

It already implements some good practices like using repositories and returning result objects.

However, it's working with an **anemic domain model**.

A few things indicating this:

*   Parameterless constructors
*   Public property setters
*   Exposed collections

In other words - the classes representing domain entities contain only data properties and no behavior.

The **problems** of an **anemic domain model** are:

*   Discoverability of operations
*   Potential code duplication
*   Lack of encapsulation

We'll apply a few techniques to push logic down into the domain, and try to make the model more domain-driven. I hope you'll be able to see the value and benefits this will bring.

```csharp
public async Task<Result> Handle(SendInvitationCommand command)
{
    var member = await _memberRepository.GetByIdAsync(command.MemberId);

    var gathering = await _gatheringRepository.GetByIdAsync(command.GatheringId);

    if (member is null || gathering is null)
    {
        return Result.Failure(Error.NullValue);
    }

    if (gathering.Creator.Id == member.Id)
    {
        throw new Exception("Can't send invitation to the creator.");
    }

    if (gathering.ScheduledAtUtc < DateTime.UtcNow)
    {
        throw new Exception("Can't send invitation for the past.");
    }

    var invitation = new Invitation
    {
        Id = Guid.NewGuid(),
        Member = member,
        Gathering = gathering,
        Status = InvitationStatus.Pending,
        CreatedOnUtc = DateTime.UtcNow
    };

    gathering.Invitations.Add(invitation);

    _invitationRepository.Add(invitation);

    await _unitOfWork.SaveChangesAsync();

    await _emailService.SendInvitationSentEmailAsync(member, gathering);

    return Result.Success();
}
```

## Moving Business Logic Into The Domain

The goal is to move as much of the business logic as possible into the domain.

Let's start with the `Invitation` entity and defining a constructor for it. I can simplify the design by setting the `Status` and `CreatedOnUtc` properties inside the constructor. I'm also going to make it `internal` so that an `Invitation` instance can only be created within the domain.

```csharp
public sealed class Invitation
{
    internal Invitation(Guid id, Gathering gathering, Member member)
    {
        Id = id;
        Member = member;
        Gathering = gathering;
        Status = InvitationStatus.Pending;
        CreatedOnUtc = DateTime.Now;
    }

    // Data properties omitted for brevity.
}
```

The reason I made the `Invitation` constructor `internal` is so that I can introduce a new method on the `Gathering` entity. Let's call it `SendInvitation` and it will be responsible for instantiating a new `Invitation` instance and adding it to the internal collection.

Currently, the `Gathering.Invitations` collection is `public`, which means anyone can obtain a reference and modify the collection.

We don't want to allow this, so what we can do is encapsulate this collection behind a `private` field. This moves the responsibility for managing the `_invitations` collection to the `Gathering` class.

Here's how the `Gathering` class looks like now:

```csharp
public sealed class Gathering
{
    private readonly List<Invitation> _invitations;

    // Other members omitted for brevity.

    public void SendInvitation(Member member)
    {
        var invitation = new Invitation(Guid.NewGuid(), gathering, member);

        _invitations.Add(invitation);
    }
}
```

## Moving Validation Rules Into The Domain

The next thing we can do is move the validation rules into the `SendInvitation` method, further enriching the domain model.

Unfortunately, this is still a bad practice because of throwing "expected" exceptions when a validation fails. If you want to use exceptions to enforce your validation rules you should at least do it right, and use specific exceptions instead of generic ones.

But it would be even better to use a **result object** to express validation errors.

```csharp
public sealed class Gathering
{
    // Other members omitted for brevity.

    public void SendInvitation(Member member)
    {
        if (gathering.Creator.Id == member.Id)
        {
            throw new Exception("Can't send invitation to the creator.");
        }

        if (gathering.ScheduledAtUtc < DateTime.UtcNow)
        {
            throw new Exception("Can't send invitation for the past.");
        }

        var invitation = new Invitation(Guid.NewGuid(), gathering, member);

        _invitations.Add(invitation);
    }
}
```

Here's how using **result objects** would look like:

```csharp
public sealed class Gathering
{
    // Other members omitted for brevity.

    public Result SendInvitation(Member member)
    {
        if (gathering.Creator.Id == member.Id)
        {
            return Result.Failure(DomainErrors.Gathering.InvitingCreator);
        }

        if (gathering.ScheduledAtUtc < DateTime.UtcNow)
        {
            return Result.Failure(DomainErrors.Gathering.AlreadyPassed);
        }

        var invitation = new Invitation(Guid.NewGuid(), gathering, member);

        _invitations.Add(invitation);

        return Result.Success();
    }
}
```

The benefit of this approach is we can introduce constants for possible domain errors. The catalog of domain errors will act as **documentation** for your domain, and make it more expressive.

Finally, here's how the `Handle` method looks like with all the changes so far:

```csharp
public async Task<Result> Handle(SendInvitationCommand command)
{
    var member = await _memberRepository.GetByIdAsync(command.MemberId);

    var gathering = await _gatheringRepository.GetByIdAsync(command.GatheringId);

    if (member is null || gathering is null)
    {
        return Result.Failure(Error.NullValue);
    }

    var result = gathering.SendInvitation(member);

    if (result.IsFailure)
    {
        return Result.Failure(result.Errors);
    }

    await _unitOfWork.SaveChangesAsync();

    await _emailService.SendInvitationSentEmailAsync(member, gathering);

    return Result.Success();
}
```

If you take a closer look at the `Handle` method you'll notice it's doing two things:

*   Persisting changes to the database
*   Sending an email

This means it's **not atomic**.

There's a potential for the database transaction to complete, and the email sending to fail. Also, sending the email will slow down the method which could affect performance.

How can make this method atomic?

By sending the email in the background. It's not important for our business logic, so this is safe to do.

## Expressing Side Effects With Domain Events

You can use **domain events** to express that something occurred in your domain that might be interesting to other components in your system.

I often use **domain events** to trigger actions in the background, like sending a notification or email.

Let's introduce an `InvitationSentDomainEvent`:

```csharp
public record InvitationSentDomainEvent(Invitation Invitation) : IDomainEvent;
```

We're going to raise this **domain event** inside the `SendInvitation` method:

```csharp
public sealed class Gathering
{
    private readonly List<Invitation> _invitations;

    // Other members omitted for brevity.

    public Result SendInvitation(Member member)
    {
        if (gathering.Creator.Id == member.Id)
        {
            return Result.Failure(DomainErrors.Gathering.InvitingCreator);
        }

        if (gathering.ScheduledAtUtc < DateTime.UtcNow)
        {
            return Result.Failure(DomainErrors.Gathering.AlreadyPassed);
        }

        var invitation = new Invitation(Guid.NewGuid(), gathering, member);

        _invitations.Add(invitation);

        Raise(new InvitationSentDomainEvent(invitation));

        return Result.Success();
    }
}
```

The goal is to remove the code responsible for sending the email from the `Handle` method:

```csharp
public async Task<Result> Handle(SendInvitationCommand command)
{
    var member = await _memberRepository.GetByIdAsync(command.MemberId);

    var gathering = await _gatheringRepository.GetByIdAsync(command.GatheringId);

    if (member is null || gathering is null)
    {
        return Result.Failure(Error.NullValue);
    }

    var result = gathering.SendInvitation(member);

    if (result.IsFailure)
    {
        return Result.Failure(result.Errors);
    }

    await _unitOfWork.SaveChangesAsync();

    return Result.Success();
}
```

We only want to worry about executing the business logic and persisting any changes to the database. Part of those changes will also be the **domain event**, which the system will publish in the background.

Of course, we need a respective **handler** for the **domain event**:

```csharp
public sealed class InvitationSentDomainEventHandler
    : IDomainEventHandler<InvitationSentDomainEvent>
{
    private readonly IEmailService _emailService;

    public InvitationSentDomainEventHandler(IEmailService emailService)
    {
        _emailService = emailService;
    }

    public async Task Handle(InvitationSentDomainEvent domainEvent)
    {
        await _emailService.SendInvitationSentEmailAsync(
            domainEvent.Invitation.Member,
            domainEvent.Invitation.Gathering);
    }
}
```

We achieved two things:

*   Handling the `SendInvitationCommand` is now atomic
*   Email is sent in the background, and can be safely retried in case of an error

## Takeaway

Designing a **rich domain model** is a gradual process, and you can slowly evolve the domain model over time.

The first step could be making your domain model more defensive:

*   Hiding constructors with the `internal` keyword
*   Encapsulating collection access

The benefit is your domain models will have a fine-grained public API (methods) which act as an entry point for executing the business logic.

It's easy to test behavior when it's encapsulated in a class without having to mock external dependencies.

You can raise **domain events** to notify the system that something of important occurred, and any interested components can subscribe to that domain event. Domain events allow you to develop a **decoupled** system, where you focus on the core domain logic, and don't have to worry about the side effects.

However, this doesn't mean that every system needs a **rich domain model**.

You should be pragmatic and decide when the complexity is worth it.

That's all for this week.

See you next Saturday.

---

Whenever you're ready, there are 4 ways I can help you:

1.  [Pragmatic Clean Architecture:](/pragmatic-clean-architecture) Join 4,200+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [Modular Monolith Architecture:](/modular-monolith-architecture) Join 2,100+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [(NEW) Pragmatic REST APIs:](/pragmatic-rest-apis) Join 1,200+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
4.  [Patreon Community:](https://www.patreon.com/milanjovanovic) Join a community of 5,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Become a Better .NET Software Engineer

Join 70,000+ engineers who are improving their skills every Saturday morning.

Join 70K+ Engineers

![Refactoring From an Anemic Domain Model To a Rich Domain Model - Blog Cover](https://www.milanjovanovic.tech/blog-covers/mnw_042.png?imwidth=3840)

![Pragmatic Clean Architecture Course Cover](https://www.milanjovanovic.tech/_next/static/media/cover.27333f2f.png?imwidth=384)

![Modular Monolith Architecture Course Cover](https://www.milanjovanovic.tech/_next/static/media/cover.31e11f05.png?imwidth=384)

![Pragmatic REST APIs Course Cover](https://www.milanjovanovic.tech/_next/static/media/cover_1.fc0deb78.png?imwidth=384)