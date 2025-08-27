```yaml
---
title: "How We Rebuilt Our .NET API Without Controllers (And Why We Regretted It in 90 Days) | by Sukhpinder Singh | C# .Net | Inside the IF | Medium"
source: https://medium.com/inside-the-if/how-we-rebuilt-our-net-api-without-controllers-and-why-we-regretted-it-in-90-days-858520b0e748
date_published: 2025-06-21T16:32:38.912Z
date_captured: 2025-08-22T11:00:27.926Z
domain: medium.com
author: "Sukhpinder Singh | C# .Net"
category: frontend
technologies: [ASP.NET Core, Minimal APIs, .NET, FluentValidation, MediatR, Swagger, OpenAPI, HttpClient]
programming_languages: [C#]
tags: [minimal-apis, aspnet-core, web-api, dotnet, api-design, architecture, validation, testing, versioning, security]
key_concepts: [Minimal APIs, Controller-based APIs, API versioning, request validation, unit testing, authorization, middleware, MediatR pattern]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article details a team's experience rebuilding a .NET API using Minimal APIs, initially drawn by their simplicity and reduced boilerplate. However, as the project scaled, they encountered significant challenges with essential features like request validation, API versioning, testing, and security, which required extensive custom middleware and led to a bloated codebase. The team ultimately regretted their choice, finding Minimal APIs too restrictive for complex production needs, and decided to migrate back to a more structured Controller-based architecture integrated with MediatR. This shift restored predictability, improved testability, and streamlined development, proving that while minimalism has its place, structure is crucial for maintainability and scalability in larger systems.]
---
```

# How We Rebuilt Our .NET API Without Controllers (And Why We Regretted It in 90 Days) | by Sukhpinder Singh | C# .Net | Inside the IF | Medium

Member-only story

Featured

# How We Rebuilt Our .NET API Without Controllers (And Why We Regretted It in 90 Days)

## Going controller-free felt modern — until versioning, testing, and validation broke. Here’s why our dream setup didn’t survive real-world complexity.

[

![Sukhpinder Singh | C# .Net](https://miro.medium.com/v2/da:true/resize:fill:64:64/1*yz64EFgxow-2pZGcqdcN1g.gif)

](/@singhsukhpinder?source=post_page---byline--858520b0e748---------------------------------------)

[Sukhpinder Singh | C# .Net](/@singhsukhpinder?source=post_page---byline--858520b0e748---------------------------------------)

Follow

5 min read

·

Jun 21, 2025

163

11

Listen

Share

More

It started like most things in tech do these days — with excitement.

Press enter or click to view image in full size

![Diagram comparing Minimal APIs and Controllers + MediatR, highlighting pros and cons. Minimal APIs are simple, fast, with less boilerplate but require custom glue for validation, testing, versioning, security, and filters. Controllers + MediatR are structured, testable, and scalable but have more boilerplate and slower startup. The title of the diagram is "Minimal APIs vs Controllers: The Reality Check".](https://miro.medium.com/v2/resize:fit:700/1*dW4ffDc91RTVg5aHyr7tRw.png)

Created by Author using Canva

I had just finished watching a .NET Conf keynote where Minimal APIs were the star. The simplicity was magnetic: five lines of code, a fast-starting app, and barely any boilerplate. We were starting a new internal tool — a good use case for something lean and modern.

```csharp
var app = WebApplication.Create(args);  
app.MapPost("/login", async (LoginRequest req, IUserService service) =>  
{  
    var result = await service.AuthenticateAsync(req);  
    return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result.Errors);  
});  
app.Run();
```

That felt slick.

No controllers. No attributes. The function signature _was_ the endpoint. We were moving fast and shipping endpoints by the dozen. We even started building a convention around endpoint grouping:

```csharp
app.MapGroup("/v1/users")  
   .MapPost("/register", RegisterUser)  
   .MapGet("/{id}", GetUserById);
```

At first, it felt like freedom. But it came with invisible strings.

## **The First Red Flag: Validation**

At week three, we noticed the first bump: request validation. We used FluentValidation, but integrating it cleanly into Minimal APIs required custom glue.

We couldn’t rely on `[ApiController]`\-based automatic validation because—well—we didn’t have controllers.

So we added this:

```csharp
app.Use(async (context, next) =>  
{  
    var endpoint = context.GetEndpoint();  
    if (endpoint?.Metadata.GetMetadata<EndpointValidationMetadata>() is { Validator: var validator })  
    {  
        context.Request.EnableBuffering();  
        var body = await JsonSerializer.DeserializeAsync<UserRequest>(context.Request.Body);  
        var result = await validator.ValidateAsync(body);  
        if (!result.IsValid)  
        {  
            context.Response.StatusCode = 400;  
            await context.Response.WriteAsJsonAsync(result.Errors);  
            return;  
        }  
    }  
    await next();  
});
```

This worked, but every new endpoint required us to tag it with metadata and wire up validation manually. It started to smell like we were replicating MVC’s model binding engine… badly.

## **Testing Pain**

By the time our service had 30+ endpoints, our test suite was bloated with `HttpClient`\-based tests.

Here’s a problem: when handlers are lambdas, you can’t inject them in isolation.

This endpoint:

```csharp
app.MapPost("/orders", async (CreateOrderRequest req, IOrderService svc) =>  
{  
    return await svc.CreateAsync(req);  
});
```

Couldn’t be unit tested directly. Either we:

1.  Pulled that logic into a service (which made the MapPost line redundant), or
2.  Wrote integration tests for everything (which was slow and overkill for simple rules).

When we moved back to controllers and MediatR, our handlers became testable again:

```csharp
public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, Result>  
{  
    public Task<Result> Handle(CreateOrderCommand request, CancellationToken cancellationToken)  
    {  
        // pure logic  
    }  
}
```

```csharp
[HttpPost]  
public async Task<IActionResult> Create([FromBody] CreateOrderCommand command)  
{  
    var result = await _mediator.Send(command);  
    return result.IsSuccess ? Ok(result) : BadRequest(result.Errors);  
}
```

Now, unit tests looked like this:

```csharp
[Fact]  
public async Task Should_Create_Order()  
{  
    var handler = new CreateOrderHandler();  
    var result = await handler.Handle(new CreateOrderCommand { ProductId = 1 }, default);  
    Assert.True(result.IsSuccess);  
}
```

Minimal APIs didn’t _prevent_ this. But they encouraged skipping it until it hurt.

## **Versioning Hell**

Then came versioning. Clients wanted both v1 and v2 of the same endpoint group.

With controllers:

```csharp
[ApiVersion("1.0")]  
[ApiVersion("2.0")]  
[Route("api/v{version:apiVersion}/products")]  
public class ProductsController : ControllerBase  
{  
    [HttpGet]  
    public IActionResult Get() => Ok();  
}
```

Easy, declarative, and discoverable in Swagger.

With Minimal APIs? Well…

```csharp
app.MapGroup("/api/v1/products")  
   .MapGet("/", GetV1Products);  
  
app.MapGroup("/api/v2/products")  
   .MapGet("/", GetV2Products);
```

It didn’t scale. Adding filters or shared logic across versions meant copy-pasting middleware or inventing your own abstraction. Swagger documentation became cluttered and inconsistent across versions. New devs didn’t know where to add a v3 endpoint. It started to rot.

## **Security Scenarios Got Ugly**

For public endpoints, a simple `.RequireAuthorization()` worked fine. But for multi-tenant, role-based flows, or per-endpoint policies, it got messy quickly.

```csharp
app.MapPost("/admin/report", AdminReportHandler)  
   .RequireAuthorization("AdminOnlyPolicy");
```

Now try adding claim-based filtering _inside_ a group of endpoints or enforcing policy scopes on action-specific data. We ended up writing code like:

```csharp
app.Use(async (ctx, next) =>  
{  
    var user = ctx.User;  
    if (!user.HasClaim("scope", "read:internal"))  
    {  
        ctx.Response.StatusCode = 403;  
        await ctx.Response.WriteAsync("Forbidden");  
        return;  
    }  
    await next();  
});
```

In MVC, you’d just write:

```csharp
[Authorize(Policy = "InternalReadAccess")]  
public IActionResult Get() => Ok();
```

Much cleaner, and central.

## **The Breaking Point**

One day we needed to apply a custom exception filter across all endpoints and return a standard error shape.

With controllers:

```csharp
services.AddControllers(options =>  
{  
    options.Filters.Add<GlobalExceptionFilter>();  
});
```

In Minimal APIs? It was back to custom middleware. Again.

We had a 1000+ line `Program.cs` file. Route registrations were in extension methods, but scattered. Tracing a request from entry to handler was painful. Swagger docs were fragile. Integration tests were brittle.

The final straw? A junior dev accidentally broke all v2 endpoints by changing an input model shared with v1. No clear version boundaries. No compiler help. No structure.

## **Going Back to Controllers**

We regrouped.

The team voted to migrate the project to Controllers + MediatR. We built a clean `Application` layer for logic, `Api` for delivery, and `Infrastructure` for services.

*   Controllers handled routing.
*   MediatR handled behavior.
*   FluentValidation was back in `[ApiController]`’s arms with `ModelState.IsValid`.
*   Swagger worked out of the box.
*   Authorization logic moved back into declarative filters.
*   Exception filters became global and composable.

Best of all? New developers onboarded in hours, not days. Testing became granular and fast. Debugging made sense.

Here’s a simple controller we shipped the next day:

```csharp
[ApiController]  
[Route("api/v1/users")]  
public class UsersController : ControllerBase  
{  
    private readonly IMediator _mediator;  
    public UsersController(IMediator mediator) => _mediator = mediator;  
    [HttpPost("register")]  
    public async Task<IActionResult> Register(RegisterUserCommand command)  
    {  
        var result = await _mediator.Send(command);  
        return result.IsSuccess ? Ok(result) : BadRequest(result.Errors);  
    }  
}
```

It wasn’t “cool.” But it was _right_ for our team and the scale of the system.

## **The Takeaway**

Minimal APIs are amazing for:

*   Prototypes
*   Single-function services
*   Rapid internal tooling

But the moment your requirements include:

*   API versioning
*   OpenAPI stability
*   Complex validation
*   Role-based access
*   Exception filters
*   Testable logic

…you will end up either rebuilding what MVC already gives you or making compromises you’ll regret.

Minimalism in code is good. But not at the cost of predictability, scalability, and maintainability.

We didn’t give up on Minimal APIs because they’re bad. We gave up because they’re too minimal for what we needed.

And as it turns out, structure isn’t overhead — it’s leverage.

**More Articles**

[

## The 5 “Unethical” LINQ Tricks That Outsmarted My Code Reviews

### Discover 5 dangerously clever LINQ tricks that passed code reviews but wreaked havoc in production. Learn what not to…

medium.com

](/c-sharp-programming/the-5-unethical-linq-tricks-that-outsmarted-my-code-reviews-a397a336452f?source=post_page-----858520b0e748---------------------------------------)

[

## Why I Stopped Copy-Pasting StackOverflow Code (And What I Do Now)

### Learn why copy-pasting StackOverflow code might be hurting your .NET projects — and how a mindful, test-driven approach…

medium.com

](/c-sharp-programming/why-i-stopped-copy-pasting-stackoverflow-code-and-what-i-do-now-6816d35b0898?source=post_page-----858520b0e748---------------------------------------)

[

## Why Everyone’s Wrong About “You Must Write Unit Tests Before Shipping”

### They told me I was crazy to ship a .NET API without unit tests. Five days later, we had a rock-solid system with…

medium.com

](/c-sharp-programming/why-everyones-wrong-about-you-must-write-unit-tests-before-shipping-23bf79eb8c90?source=post_page-----858520b0e748---------------------------------------)

[

## 17 Tips from a .NET Developer — Part 2

### 10 years in .NET development have taught me more than just writing clean code.

medium.com

](/c-sharp-programming/17-tips-from-a-net-developer-part-2-c842a3e3c9aa?source=post_page-----858520b0e748---------------------------------------)

# C# Programming🚀

Thank you for being a part of the C# community! Before you leave:

Follow us: [**LinkedIn**](https://www.linkedin.com/in/sukhpinder-singh/) **|** [**Dev.to**](https://dev.to/ssukhpinder)  
Visit our other platforms: [**GitHub**](https://github.com/ssukhpinder)  
More content at [**C# Programming**](https://medium.com/c-sharp-progarmming)

[

![Buy Me A Coffee logo](https://miro.medium.com/v2/resize:fit:170/0*V8xexJ0JTwrkCqf_.png)

](https://buymeacoffee.com/sukhpindersingh)