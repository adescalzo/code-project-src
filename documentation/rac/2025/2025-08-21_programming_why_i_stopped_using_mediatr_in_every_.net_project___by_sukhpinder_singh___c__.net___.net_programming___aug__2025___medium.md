```yaml
---
title: "Why I Stopped Using MediatR in Every .NET Project | by Sukhpinder Singh | C# .Net | .Net Programming | Aug, 2025 | Medium"
source: https://medium.com/c-sharp-programming/why-i-stopped-using-mediatr-in-every-net-project-e417e8c22d66
date_published: 2025-08-21T15:06:02.008Z
date_captured: 2025-08-29T10:33:11.342Z
domain: medium.com
author: "Sukhpinder Singh | C# .Net"
category: programming
technologies: [MediatR, .NET, ASP.NET Core, Moq, ILogger, IServiceCollection, IServiceProvider, Canva]
programming_languages: [C#]
tags: [mediatr, dotnet, microservices, clean-architecture, event-driven, performance, debugging, testing, software-design, csharp]
key_concepts: [MediatR pattern, Clean Architecture, Microservices, CQRS, Domain Events, Dependency Injection, Pipeline Behaviors, Unit Testing]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article critically examines the widespread use of MediatR in .NET projects, particularly within microservices architectures. The author argues that while MediatR initially appears to promote clean code and decoupling, it often leads to significant debugging challenges, performance degradation due to reflection and pipeline overhead, and increased complexity in unit testing. As an alternative, the article proposes a simpler, hybrid event-driven pattern where queries directly invoke services, commands use application services, and side effects are handled via domain events. The author concludes that for large, production-grade systems, prioritizing explicitness and traceability over pure abstraction ultimately results in more maintainable, observable, and performant software.]
---
```

# Why I Stopped Using MediatR in Every .NET Project | by Sukhpinder Singh | C# .Net | .Net Programming | Aug, 2025 | Medium

# Why I Stopped Using MediatR in Every .NET Project

## It seems clean… until you debug it. I share how MediatR bloated our microservices, complicated tests, and killed performance — and the simpler event-driven pattern we replaced it with.

![Author's profile picture: Sukhpinder Singh | C# .Net](https://miro.medium.com/v2/da:true/resize:fill:64:64/1*yz64EFgxow-2pZGcqdcN1g.gif)

[Sukhpinder Singh | C# .Net](/@singhsukhpinder?source=post_page---byline--e417e8c22d66---------------------------------------)

Follow

8 min read

·

Aug 21, 2025

33

6

Listen

Share

More

Press enter or click to view image in full size

![Article title image with "Why I Stopped Using MediatR in Every .NET Project" and a large C# logo on a gradient background.](https://miro.medium.com/v2/resize:fit:700/1*BnTEnbFYvMgGSb3mB0AE0Q.png)

Created by Author using Canva

## The Illusion of Clean Code

At first, MediatR felt like magic.
It decoupled everything. No more service-to-service spaghetti. Just tidy `IMediator.Send()` calls floating through our handlers like well-behaved drones.
For a while, it worked — especially in our early Clean Architecture experiments.

But somewhere between our third microservice and the tenth production incident, that clean code turned into a debugging nightmare.

## When Abstractions Leak, Logs Die

Here’s what happened.

We were scaling a system with dozens of loosely-coupled microservices. Each service used MediatR internally to handle commands and queries. The idea was noble: separation of concerns, testability, and single responsibility everywhere.

But the abstraction turned into a blindfold.
When a user clicked “submit” on the frontend and the request failed mid-flight, **we had no idea why**.

> The call stack? Obfuscated.
> The logs? Scattered.
> The error? Buried three handlers deep.

One of our devs — let’s call him Raj — spent **six hours** tracking a null ref bug that was silently swallowed by a pre-handler. It turned out the pipeline was injecting a different scoped service than expected. No exception. No hint. Just silence.

That’s when we knew: something was wrong.

It wasn’t just the debugging pain. Here’s a breakdown of what MediatR really did to our project:

## 1. Performance Hit from Reflection and Pipeline Overhead

Every `Send()` triggered a waterfall of behaviors — logging, validation, authorization — even for trivial queries. On hot paths (like a public API that fetched user dashboards), this compounded fast.

Benchmarking one endpoint before and after:

```csharp
// With MediatR (pipeline behaviors + handler)
var result = await _mediator.Send(new GetUserDashboardQuery(userId));

// After refactor (direct handler call)
var result = await _dashboardService.GetUserDashboardAsync(userId);
```

| Approach         | Avg Response Time |
| :--------------- | :---------------- |
| MediatR          | 118ms             |
| Direct Service   | 52ms              |

![A bar chart comparing average response times. MediatR shows 118ms and Direct Service shows 52ms, indicating MediatR is over 2x slower.](https://miro.medium.com/v2/resize:fit:700/1*BnTEnbFYvMgGSb3mB0AE0Q.png)

That’s over **2x slower**, just from abstraction overhead.

## 2. Testing Turned Fragile and Verbose

Because MediatR splits logic across handlers and behaviors, unit tests ballooned in setup complexity. Mocking every injected dependency — including pipeline behaviors — became a chore.

We had tests like this:

```csharp
var mediator = new Mock<IMediator>();
mediator.Setup(m => m.Send(It.IsAny<MyCommand>(), default))
        .ReturnsAsync(new MyResult());

var sut = new MyController(mediator.Object);
// Act + Assert
```

After ditching MediatR:

```csharp
var dashboardService = new Mock<IDashboardService>();
dashboardService.Setup(s => s.GetUserDashboardAsync(userId))
                .ReturnsAsync(new DashboardData());

var sut = new DashboardController(dashboardService.Object);
// Act + Assert
```

Simpler. More readable. Fewer mocks. We were finally testing behavior, not plumbing.

## 3. Developer Onboarding Became Slower

New hires found MediatR confusing. “Where is this command handled?” they’d ask.
We’d pull up four files — the controller, the command, the handler, and the validator — just to explain a single flow. Clean in theory. Disorienting in practice.

One junior dev said it best:

> _“It feels like I’m debugging by playing fetch.”_

## What We Switched To: A Simpler, Event-Driven Pattern

Instead of using MediatR as our default for everything, we adopted a **hybrid event-driven pattern** that combines service-layer calls with domain events. Here’s the shift:

*   **Queries** now call services directly. No handler indirection.
*   **Commands** stay simple, invoking use-case methods in application services.
*   **Side effects** are triggered via **domain events**, published from within aggregates.

We implemented a lightweight domain event dispatcher like this:

```csharp
public interface IDomainEvent { }

public interface IEventHandler<T> where T : IDomainEvent
{
    Task HandleAsync(T @event);
}
```

And events get published after committing the unit of work:

```csharp
public class User
{
    public void Activate()
    {
        IsActive = true;
        AddDomainEvent(new UserActivatedEvent(Id));
    }
}
```

Handlers stay separate, fire-and-forget:

```csharp
public class SendWelcomeEmailHandler : IEventHandler<UserActivatedEvent>
{
    public Task HandleAsync(UserActivatedEvent @event)
    {
        return _emailService.SendWelcomeEmailAsync(@event.UserId);
    }
}
```

It’s explicit. Traceable. And the code reads like a story — not a mystery novel.

## When MediatR Still Makes Sense

Let me be clear: MediatR isn’t bad.
It’s just **not a default** anymore.

We still use it in:

*   Internal tools where CQRS is overkill
*   Mediating cross-cutting concerns in monoliths
*   Prototyping apps where team size is small and speed matters

But for large, production-grade systems?
We favor **explicitness over purity**.

## Final Thoughts: The Price of Over-Abstraction

The Clean Architecture crowd loves MediatR because it checks all the boxes: SOLID, DDD, CQRS. But if you’re not careful, you’ll build a system that’s perfectly decoupled — from reality.

If your goal is maintainable, observable, and performant software, ask yourself:

*   Do I need a mediator here?
*   Or am I solving a problem that doesn’t exist yet?

In our case, removing MediatR didn’t just clean up the architecture — it clarified ownership, reduced latency, and made devs happier.

That’s worth more than a textbook diagram.

**Claps welcomed. Especially if you’ve rage-debugged a pipeline behaviour.**
**Let’s talk: Are you still using MediatR? Would you stop?**

### Full Code Sample

The below sample demonstrates the key points from the article about moving away from MediatR. This will show the “before” and “after” approaches with practical examples.

```csharp
// =============================================================================
// BEFORE: Using MediatR Pattern
// =============================================================================

// Command/Query Definitions
public class GetUserDashboardQuery : IRequest<DashboardData>
{
    public int UserId { get; set; }
}

public class ActivateUserCommand : IRequest<bool>
{
    public int UserId { get; set; }
}

// MediatR Handlers
public class GetUserDashboardHandler : IRequestHandler<GetUserDashboardQuery, DashboardData>
{
    private readonly IUserRepository _userRepository;
    private readonly IDashboardService _dashboardService;

    public GetUserDashboardHandler(IUserRepository userRepository, IDashboardService dashboardService)
    {
        _userRepository = userRepository;
        _dashboardService = dashboardService;
    }

    public async Task<DashboardData> Handle(GetUserDashboardQuery request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null) throw new UserNotFoundException();
          
        return await _dashboardService.BuildDashboardAsync(user);
    }
}

public class ActivateUserHandler : IRequestHandler<ActivateUserCommand, bool>
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailService _emailService;

    public ActivateUserHandler(IUserRepository userRepository, IEmailService emailService)
    {
        _userRepository = userRepository;
        _emailService = emailService;
    }

    public async Task<bool> Handle(ActivateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await _userRepository.GetByIdAsync(request.UserId);
        if (user == null) return false;

        user.Activate();
        await _userRepository.UpdateAsync(user);
          
        // Side effect handling inline
        await _emailService.SendWelcomeEmailAsync(user.Id);
          
        return true;
    }
}

// Pipeline Behaviors (adds complexity)
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        _logger.LogInformation($"Handling {typeof(TRequest).Name}");
        var response = await next();
        _logger.LogInformation($"Handled {typeof(TRequest).Name}");
        return response;
    }
}

// Controller with MediatR
[ApiController]
[Route("[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IMediator _mediator;

    public DashboardController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{userId}")]
    public async Task<DashboardData> GetUserDashboard(int userId)
    {
        return await _mediator.Send(new GetUserDashboardQuery { UserId = userId });
    }

    [HttpPost("{userId}/activate")]
    public async Task<bool> ActivateUser(int userId)
    {
        return await _mediator.Send(new ActivateUserCommand { UserId = userId });
    }
}

// Complex Test Setup with MediatR
[Test]
public async Task GetUserDashboard_ShouldReturnDashboard_WhenUserExists()
{
    // Arrange
    var mediator = new Mock<IMediator>();
    var expectedDashboard = new DashboardData { UserId = 1, Name = "Test User" };
      
    mediator.Setup(m => m.Send(It.IsAny<GetUserDashboardQuery>(), default))
            .ReturnsAsync(expectedDashboard);

    var controller = new DashboardController(mediator.Object);

    // Act
    var result = await controller.GetUserDashboard(1);

    // Assert
    Assert.AreEqual(expectedDashboard, result);
    mediator.Verify(m => m.Send(It.IsAny<GetUserDashboardQuery>(), default), Times.Once);
}

// =============================================================================
// AFTER: Direct Service Approach with Domain Events
// =============================================================================

// Domain Events
public interface IDomainEvent { }

public class UserActivatedEvent : IDomainEvent
{
    public int UserId { get; }
    public DateTime OccurredAt { get; }

    public UserActivatedEvent(int userId)
    {
        UserId = userId;
        OccurredAt = DateTime.UtcNow;
    }
}

// Event Handler Interface
public interface IEventHandler<T> where T : IDomainEvent
{
    Task HandleAsync(T @event);
}

// Domain Event Dispatcher
public interface IDomainEventDispatcher
{
    Task DispatchAsync<T>(T @event) where T : IDomainEvent;
}

public class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public DomainEventDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task DispatchAsync<T>(T @event) where T : IDomainEvent
    {
        var handlers = _serviceProvider.GetServices<IEventHandler<T>>();
        var tasks = handlers.Select(handler => handler.HandleAsync(@event));
        await Task.WhenAll(tasks);
    }
}

// Enhanced Domain Entity with Events
public class User
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public int Id { get; private set; }
    public string Name { get; private set; }
    public bool IsActive { get; private set; }

    public void Activate()
    {
        if (!IsActive)
        {
            IsActive = true;
            AddDomainEvent(new UserActivatedEvent(Id));
        }
    }

    public void AddDomainEvent(IDomainEvent @event)
    {
        _domainEvents.Add(@event);
    }

    public IReadOnlyList<IDomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}

// Application Services (Direct, No MediatR)
public interface IUserService
{
    Task<DashboardData> GetUserDashboardAsync(int userId);
    Task<bool> ActivateUserAsync(int userId);
}

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IDashboardService _dashboardService;
    private readonly IDomainEventDispatcher _eventDispatcher;

    public UserService(        IUserRepository userRepository,   
        IDashboardService dashboardService,
        IDomainEventDispatcher eventDispatcher)
    {
        _userRepository = userRepository;
        _dashboardService = dashboardService;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<DashboardData> GetUserDashboardAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) throw new UserNotFoundException();
          
        return await _dashboardService.BuildDashboardAsync(user);
    }

    public async Task<bool> ActivateUserAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null) return false;

        user.Activate();
        await _userRepository.UpdateAsync(user);
          
        // Dispatch domain events after successful persistence
        var events = user.GetDomainEvents();
        foreach (var @event in events)
        {
            await _eventDispatcher.DispatchAsync(@event);
        }
        user.ClearDomainEvents();
          
        return true;
    }
}

// Event Handlers (Separated Side Effects)
public class SendWelcomeEmailHandler : IEventHandler<UserActivatedEvent>
{
    private readonly IEmailService _emailService;
    private readonly ILogger<SendWelcomeEmailHandler> _logger;

    public SendWelcomeEmailHandler(IEmailService emailService, ILogger<SendWelcomeEmailHandler> logger)
    {
        _emailService = emailService;
        _logger = logger;
    }

    public async Task HandleAsync(UserActivatedEvent @event)
    {
        _logger.LogInformation($"Sending welcome email to user {event.UserId}");
        await _emailService.SendWelcomeEmailAsync(@event.UserId);
    }
}

public class UpdateUserStatsHandler : IEventHandler<UserActivatedEvent>
{
    private readonly IUserStatsService _userStatsService;

    public UpdateUserStatsHandler(IUserStatsService userStatsService)
    {
        _userStatsService = userStatsService;
    }

    public async Task HandleAsync(UserActivatedEvent @event)
    {
        await _userStatsService.IncrementActiveUsersAsync();
    }
}

// Simplified Controller (Direct Service Calls)
[ApiController]
[Route("[controller]")]
public class DashboardController : ControllerBase
{
    private readonly IUserService _userService;

    public DashboardController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("{userId}")]
    public async Task<DashboardData> GetUserDashboard(int userId)
    {
        return await _userService.GetUserDashboardAsync(userId);
    }

    [HttpPost("{userId}/activate")]
    public async Task<bool> ActivateUser(int userId)
    {
        return await _userService.ActivateUserAsync(userId);
    }
}

// Simplified Test Setup
[Test]
public async Task GetUserDashboard_ShouldReturnDashboard_WhenUserExists()
{
    // Arrange
    var userService = new Mock<IUserService>();
    var expectedDashboard = new DashboardData { UserId = 1, Name = "Test User" };
      
    userService.Setup(s => s.GetUserDashboardAsync(1))
               .ReturnsAsync(expectedDashboard);

    var controller = new DashboardController(userService.Object);

    // Act
    var result = await controller.GetUserDashboard(1);

    // Assert
    Assert.AreEqual(expectedDashboard, result);
    userService.Verify(s => s.GetUserDashboardAsync(1), Times.Once);
}

// =============================================================================
// Supporting Classes and Interfaces
// =============================================================================

public class DashboardData
{
    public int UserId { get; set; }
    public string Name { get; set; }
    public List<string> RecentActivities { get; set; } = new();
}

public interface IUserRepository
{
    Task<User> GetByIdAsync(int id);
    Task UpdateAsync(User user);
}

public interface IDashboardService
{
    Task<DashboardData> BuildDashboardAsync(User user);
}

public interface IEmailService
{
    Task SendWelcomeEmailAsync(int userId);
}

public interface IUserStatsService
{
    Task IncrementActiveUsersAsync();
}

public class UserNotFoundException : Exception
{
    public UserNotFoundException() : base("User not found") { }
}

// =============================================================================
// DI Container Setup Comparison
// =============================================================================

// MediatR Setup (Startup.cs)
public void ConfigureServicesMediatR(IServiceCollection services)
{
    services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
    services.AddScoped(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
    // ... other pipeline behaviors
}

// Direct Service Setup (Startup.cs)
public void ConfigureServicesDirect(IServiceCollection services)
{
    services.AddScoped<IUserService, UserService>();
    services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
    services.AddScoped<IEventHandler<UserActivatedEvent>, SendWelcomeEmailHandler>();
    services.AddScoped<IEventHandler<UserActivatedEvent>, UpdateUserStatsHandler>();
    // ... other services
}
```

### More Articles

[

## How to Refactor a Legacy Codebase in One Weekend: A Step-by-Step Guide

### Refactor legacy .NET code in a single weekend? Yes — this real-world guide shows how to tame 2K-line files, add tests…

medium.com

](/c-sharp-programming/how-to-refactor-a-legacy-codebase-in-one-weekend-a-step-by-step-guide-d6ab6629252b?source=post_page-----e417e8c22d66---------------------------------------)

[

## 2025’s Most Stolen Docker Secret (And How to Deploy Faster Than Everyone Else)

### Discover the secret Docker trick that slashed .NET CI build times by 80%. Learn how prebuilt containers + volume…

medium.com

](/devs-community/2025s-most-stolen-docker-secret-and-how-to-deploy-faster-than-everyone-else-e07732f96dd8?source=post_page-----e417e8c22d66---------------------------------------)

[

## 7 Mistakes Junior Developers Make With Microservices (And How to Avoid Them) in .NET

### New to .NET microservices? Avoid the 7 common mistakes junior developers make — like distributed CRUD and sync chains — …

medium.com

](/c-sharp-programming/7-mistakes-junior-developers-make-with-microservices-and-how-to-avoid-them-in-net-144e7a6f0cce?source=post_page-----e417e8c22d66---------------------------------------)

[

## Optimising .NET for High-Frequency Trading: Microseconds Matter

### From profiling pain to performance breakthroughs — how we carved nanoseconds out of .NET

medium.com

](/c-sharp-programming/optimising-net-for-high-frequency-trading-microseconds-matter-2f975a5dc8ff?source=post_page-----e417e8c22d66---------------------------------------)

[

## Zero-Allocation Patterns in Modern .NET Web APIs

### How I Eliminated Hidden Allocations from a Real Production API. They Laughed When I Swapped ToList() — Until Our P95…

medium.com

](/c-sharp-programming/zero-allocation-patterns-in-modern-net-web-apis-8c06c18743b4?source=post_page-----e417e8c22d66---------------------------------------)

## C# Programming🚀

Thank you for being a part of the C# community! Before you leave:

Follow us: [**LinkedIn**](https://www.linkedin.com/in/sukhpinder-singh/) **|** [**Dev.to**](https://dev.to/ssukhpinder)
Visit our other platforms: [**GitHub**](https://github.com/ssukhpinder)
More content at [**C# Programming**](https://medium.com/c-sharp-progarmming)

![A "Buy Me A Coffee" logo.](https://miro.medium.com/v2/resize:fit:170/0*V8xexJ0JTwrkCqf_.png)