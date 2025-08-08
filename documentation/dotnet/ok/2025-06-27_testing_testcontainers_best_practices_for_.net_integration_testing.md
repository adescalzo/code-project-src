```yaml
---
title: Testcontainers Best Practices for .NET Integration Testing
source: https://www.milanjovanovic.tech/blog/testcontainers-best-practices-dotnet-integration-testing?utm_source=newsletter&utm_medium=email&utm_campaign=tnw148
date_published: 2025-06-28T00:00:00.000Z
date_captured: 2025-08-06T18:28:07.067Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: testing
technologies: [Testcontainers, .NET, ASP.NET Core, Docker, PostgreSQL, Redis, xUnit, Microsoft.AspNetCore.Mvc.Testing, Testcontainers.PostgreSql, Testcontainers.Redis]
programming_languages: [C#, PowerShell]
tags: [integration-testing, dotnet, testcontainers, docker, testing, best-practices, postgresql, redis, xunit, web-application-factory]
key_concepts: [integration-testing, testcontainers, docker-containers, iasynclifetime, webapplicationfactory, xunit-fixtures, dynamic-configuration, test-isolation]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article outlines best practices for building robust and maintainable integration tests in .NET using Testcontainers. It explains how Testcontainers leverages Docker to provide isolated, real-world environments for dependencies like PostgreSQL and Redis, eliminating issues with shared or in-memory databases. Key recommendations include managing container lifecycles with `IAsyncLifetime`, dynamically injecting connection strings into `WebApplicationFactory`, and strategically using xUnit's class and collection fixtures for optimal performance and test isolation. The guide emphasizes writing tests that focus on business logic by abstracting infrastructure concerns, ultimately leading to more reliable and confident testing.]
---
```

# Testcontainers Best Practices for .NET Integration Testing

![Testcontainers Best Practices for .NET Integration Testing](/blog-covers/mnw_148.png?imwidth=3840)
*Image: Blog cover titled "Testcontainers Best Practices for .NET" with a blue and black abstract design and the "MJ Tech" logo.*

# Testcontainers Best Practices for .NET Integration Testing

5 min read · June 28, 2025

[**BREAKING: PagerDuty Actually Fixed Incident Management**](https://fnf.dev/3FSidO0)  
Remember when PagerDuty was just for alerting? Plot twist: They've quietly bundled and enhanced complete incident management into ALL plans - including custom workflows, Slack-first everything, AI-powered summaries, and incident roles. Your incident management deserves better than browser tabs and hope. [**Check it out here**](https://fnf.dev/3FSidO0).

Nick Chapsas' Dometrain is celebrating 2 years of teaching .NET developers, and they are offering their [**"From Zero to Hero: REST APIs in .NET"**](https://dometrain.com/course/from-zero-to-hero-rest-apis-in-asp-net-core/?ref=milan-jovanovic&promo=newsletter-20250628) course for free. Until the end of June, use the link below, and the course is yours to keep for 1 month. [**Get it for free**](https://dometrain.com/course/from-zero-to-hero-rest-apis-in-asp-net-core/?ref=milan-jovanovic&promo=newsletter-20250628).

[Sponsor this newsletter](/sponsor-the-newsletter)

Integration tests with Testcontainers are powerful, but they can quickly become a maintenance nightmare if you don't follow the right patterns.

I've seen teams struggle with flaky tests, slow test suites, and configuration headaches that could have been avoided with better practices from the start.

Today, I'll show you the patterns that make [Testcontainers](https://testcontainers.com/) tests reliable, fast, and easy to maintain.

## How Testcontainers Changes Integration Testing

Traditional [**integration tests**](testcontainers-integration-testing-using-docker-in-dotnet) often rely on shared test databases or in-memory alternatives that don't match production behavior. You either deal with test pollution between runs or sacrifice realism for speed.

Testcontainers solves this by spinning up real [Docker](https://www.docker.com/) containers for your dependencies. Your tests run against actual PostgreSQL, Redis, or any other service you use in production. When tests complete, containers are destroyed, giving you a clean slate every time.

The magic happens through Docker's API. Testcontainers manages the entire lifecycle: pulling images, starting containers, waiting for readiness, and cleanup. Your test code just needs to know how to connect.

## Prerequisites

First, make sure you have the necessary packages:

```powershell
Install-Package Microsoft.AspNetCore.Mvc.Testing
Install-Package Testcontainers.PostgreSql
Install-Package Testcontainers.Redis
```

If you want to learn more about the basic setup, check out my article on [**integrating testing with Testcontainers**](testcontainers-integration-testing-using-docker-in-dotnet).

## Creating Test Containers

Here's how to set up your containers with proper configuration:

```csharp
PostgreSqlContainer _postgresContainer = new PostgreSqlBuilder()
    .WithImage("postgres:17")
    .WithDatabase("devhabit")
    .WithUsername("postgres")
    .WithPassword("postgres")
    .Build();

RedisContainer _redisContainer = new RedisBuilder()
    .WithImage("redis:latest")
    .Build();
```

To start and stop containers cleanly across your test suite, implement `IAsyncLifetime` in your `WebApplicationFactory`:

```csharp
public sealed class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _redisContainer.StartAsync();
        // Start other dependencies here
    }

    public async Task DisposeAsync()
    {
        await _postgresContainer.StopAsync();
        await _redisContainer.StopAsync();
    }
}
```

This ensures containers are ready before tests run and cleaned up afterward. This means no leftover Docker state or race conditions.

**A tip**: pin your image versions (like `postgres:17`) to avoid surprises from upstream changes

I learned this the hard way when a minor version update caused my tests to fail unexpectedly.

## Pass Configuration to Your App

The biggest mistake I see is hardcoding connection strings. Testcontainers assigns dynamic ports. Don't hardcode anything.

Instead, inject values via `WebApplicationFactory.ConfigureWebHost`:

```csharp
protected override void ConfigureWebHost(IWebHostBuilder builder)
{
    builder.UseSetting("ConnectionStrings:Database", _postgresContainer.GetConnectionString());
    builder.UseSetting("ConnectionStrings:Redis", _redisContainer.GetConnectionString());
}
```

The key is to use the `UseSetting` method to pass connection strings dynamically. It also avoids any race conditions or conflicts with other tests that might run in parallel.

This ensures your tests always connect to the right ports, regardless of what Docker assigns.

There's no need to remove services from the service collection or manually configure them (contrary to what you might find online). Just set the connection strings, and your application will use them automatically.

## Share Expensive Setup with xUnit Collection Fixtures

What's a test fixture? A **fixture** is a shared context for your tests, allowing you to set up expensive resources like databases or message brokers once and reuse them across multiple tests.

This is where most teams get tripped up. The choice between class and collection fixtures affects both test performance and isolation.

**Class Fixture** - One container per test class:

Use class fixtures when tests modify global state or when debugging test interactions becomes difficult.

```csharp
public class AddItemToCartTests : IClassFixture<DevHabitWebAppFactory>
{
    private readonly DevHabitWebAppFactory _factory;

    public AddItemToCartTests(DevHabitWebAppFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Should_ReturnFailure_WhenNotEnoughQuantity() { ... }
}
```

**Collection Fixture** - One container shared across multiple test classes:

Use collection fixtures when your tests don't modify shared state or when you can reliably clean up between tests.

```csharp
[CollectionDefinition(nameof(IntegrationTestCollection))]
public sealed class IntegrationTestCollection : ICollectionFixture<DevHabitWebAppFactory>
{
}
```

Then apply it to your test classes:

```csharp
[Collection(nameof(IntegrationTestCollection))]
public class AddItemToCartTests : IntegrationTestFixture
{
    public AddItemToCartTests(DevHabitWebAppFactory factory) : base(factory) { }

    [Fact]
    public async Task Should_ReturnFailure_WhenNotEnoughQuantity()
    {
        Guid customerId = await Sender.CreateCustomerAsync(Guid.NewGuid());
        var command = new AddItemToCartCommand(customerId, ticketTypeId, Quantity + 1);

        Result result = await Sender.Send(command);

        result.Error.Should().Be(TicketTypeErrors.NotEnoughQuantity(Quantity));
    }
}
```

When to use which:

*   Class fixtures when you need full isolation between test classes (slower but safer)
*   Collection fixtures when test classes don't interfere with each other (faster but requires discipline)

With collection fixtures, you have to take care of cleaning up any state that might persist between tests. This could include resetting databases, clearing caches, or removing test data. If you don't do this, you risk tests affecting each other, leading to flaky results.

## Utility Methods for Auth and Cleanup

Your fixture can expose helpers to simplify test writing:

```csharp
public async Task<HttpClient> CreateAuthenticatedClientAsync() { ... }

protected async Task CleanupDatabaseAsync() { ... }
```

These methods can handle authentication setup and database cleanup, so you don't have to repeat boilerplate code in every test. This lets your test code focus on assertions, not setup.

## Writing Maintainable Integration Tests

With the infrastructure properly configured, your actual tests should focus on business logic:

```csharp
[Fact]
public async Task Should_ReturnFailure_WhenNotEnoughQuantity()
{
    //Arrange
    Guid customerId = await Sender.CreateCustomerAsync(Guid.NewGuid());
    var eventId = Guid.NewGuid();
    var ticketTypeId = Guid.NewGuid();

    await Sender.CreateEventWithTicketTypeAsync(eventId, ticketTypeId, Quantity);

    var command = new AddItemToCartCommand(customerId, ticketTypeId, Quantity + 1);

    //Act
    Result result = await Sender.Send(command);

    //Assert
    result.Error.Should().Be(TicketTypeErrors.NotEnoughQuantity(Quantity));
}
```

Notice how the tests focus on business rules rather than infrastructure concerns. The container complexity is hidden behind well-designed base classes and helper methods. You're not mocking Postgres or Redis, you're testing real behavior.

## Conclusion

Testcontainers transforms integration testing by giving you the confidence that comes from testing against real dependencies. No more wondering if your in-memory database behavior matches production, or dealing with shared test environments that break when someone else runs their tests.

Start simple: pick one integration test that currently uses mocks or in-memory databases, and convert it to use Testcontainers. You'll immediately notice the difference in confidence when that test passes. Then gradually expand to cover your critical business flows.

If you want to learn how to structure your applications for testability from day one, my [**Pragmatic Clean Architecture**](/pragmatic-clean-architecture) course covers integration testing with Testcontainers alongside domain modeling, API design, and the architectural decisions that make applications maintainable over time.

That's all for today.

See you next week.

---

Whenever you're ready, there are 4 ways I can help you:

1.  [Pragmatic Clean Architecture:](/pragmatic-clean-architecture) Join 4,200+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [Modular Monolith Architecture:](/modular-monolith-architecture) Join 2,100+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [(NEW) Pragmatic REST APIs:](/pragmatic-rest-apis) Join 1,100+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
4.  [Patreon Community:](https://www.patreon.com/milanjovanovic) Join a community of 5,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Become a Better .NET Software Engineer

Join 70,000+ engineers who are improving their skills every Saturday morning.

.formkit-form[data-uid="134c4e25db"] *{box-sizing:border-box;}.formkit-form[data-uid="134c4e25db"]{-webkit-font-smoothing:antialiased;-moz-osx-font-smoothing:grayscale;}.formkit-form[data-uid="134c4e25db"] legend{border:none;font-size:inherit;margin-bottom:10px;padding:0;position:relative;display:table;}.formkit-form[data-uid="134c4e25db"] fieldset{border:0;padding:0.01em 0 0 0;margin:0;min-width:0;}.formkit-form[data-uid="134c4e25db"] body:not(:-moz-handler-blocked) fieldset{display:table-cell;}.formkit-form[data-uid="134c4e25db"] h1,.formkit-form[data-uid="134c4e25db"] h2,.formkit-form[data-uid="134c4e25db"] h3,.formkit-form[data-uid="134c4e25db"] h4,.formkit-form[data-uid="134c4e25db"] h5,.formkit-form[data-uid="134c4e25db"] h6{color:inherit;font-size:inherit;font-weight:inherit;}.formkit-form[data-uid="134c4e25db"] h2{font-size:1.5em;margin:1em 0;}.formkit-form[data-uid="134c4e25db"] h3{font-size:1.17em;margin:1em 0;}.formkit-form[data-uid="134c4e25db"] p{color:inherit;font-size:inherit;font-weight:inherit;}.formkit-form[data-uid="134c4e25db"] ol:not([template-default]),.formkit-form[data-uid="134c4e25db"] ul:not([template-default]),.formkit-form[data-uid="134c4e25db"] blockquote:not([template-default]){text-align:left;}.formkit-form[data-uid="134c4e25db"] p:not([template-default]),.formkit-form[data-uid="134c4e25db"] hr:not([template-default]),.formkit-form[data-uid="134c4e25db"] blockquote:not([template-default]),.formkit-form[data-uid="134c4e25db"] ol:not([template-default]),.formkit-form[data-uid="134c4e25db"] ul:not([template-default]){color:inherit;font-style:initial;}.formkit-form[data-uid="134c4e25db"] .ordered-list,.formkit-form[data-uid="134c4e25db"] .unordered-list{list-style-position:outside !important;padding-left:1em;}.formkit-form[data-uid="134c4e25db"] .list-item{padding-left:0;}.formkit-form[data-uid="134c4e25db"][data-format="modal"]{display:none;}.formkit-form[data-uid="134c4e25db"][data-format="slide in"]{display:none;}.formkit-form[data-uid="134c4e25db"][data-format="sticky bar"]{display:none;}.formkit-sticky-bar .formkit-form[data-uid="134c4e25db"][data-format="sticky bar"]{display:block;}.formkit-form[data-uid="134c4e25db"] .formkit-input,.formkit-form[data-uid="134c4e25db"] .formkit-select,.formkit-form[data-uid="134c4e25db"] .formkit-checkboxes{width:100%;}.formkit-form[data-uid="134c4e25db"] .formkit-button,.formkit-form[data-uid="134c4e25db"] .formkit-submit{border:0;border-radius:5px;color:#ffffff;cursor:pointer;display:inline-block;text-align:center;font-size:15px;font-weight:500;cursor:pointer;margin-bottom:15px;overflow:hidden;padding:0;position:relative;vertical-align:middle;}.formkit-form[data-uid="134c4e25db"] .formkit-button:hover,.formkit-form[data-uid="134c4e25db"] .formkit-submit:hover,.formkit-form[data-uid="134c4e25db"] .formkit-button:focus,.formkit-form[data-uid="134c4e25db"] .formkit-submit:focus{outline:none;}.formkit-form[data-uid="134c4e25db"] .formkit-button:hover > span,.formkit-form[data-uid="134c4e25db"] .formkit-submit:hover > span,.formkit-form[data-uid="134c4e25db"] .formkit-button:focus > span,.formkit-form[data-uid="134c4e25db"] .formkit-submit:focus > span{background-color:rgba(0,0,0,0.1);}.formkit-form[data-uid="134c4e25db"] .formkit-button > span,.formkit-form[data-uid="134c4e25db"] .formkit-submit > span{display:block;-webkit-transition:all 300ms ease-in-out;transition:all 300ms ease-in-out;padding:12px 24px;}.formkit-form[data-uid="134c4e25db"] .formkit-input{background:#ffffff;font-size:15px;padding:12px;border:1px solid #e3e3e3;-webkit-flex:1 0 auto;-ms-flex:1 0 auto;flex:1 0 auto;line-height:1.4;margin:0;-webkit-transition:border-color ease-out 300ms;transition:border-color ease-out 300ms;}.formkit-form[data-uid="134c4e25db"] .formkit-input:focus{outline:none;border-color:#1677be;-webkit-transition:border-color ease 300ms;transition:border-color ease 300ms;}.formkit-form[data-uid="134c4e25db"] .formkit-input::-webkit-input-placeholder{color:inherit;opacity:0.8;}.formkit-form[data-uid="134c4e25db"] .formkit-input::-moz-placeholder{color:inherit;opacity:0.8;}.formkit-form[data-uid="134c4e25db"] .formkit-input:-ms-input-placeholder{color:inherit;opacity:0.8;}.formkit-form[data-uid="134c4e25db"] .formkit-input::placeholder{color:inherit;opacity:0.8;}.formkit-form[data-uid="134c4e25db"] [data-group="dropdown"]{position:relative;display:inline-block;width:100%;}.formkit-form[data-uid="134c4e25db"] [data-group="dropdown"]::before{content:"";top:calc(50% - 2.5px);right:10px;position:absolute;pointer-events:none;border-color:#4f4f4f transparent transparent transparent;border-style:solid;border-width:6px 6px 0 6px;height:0;width:0;z-index:999;}.formkit-form[data-uid="134c4e25db"] [data-group="dropdown"] select{height:auto;width:100%;cursor:pointer;color:#333333;line-height:1.4;margin-bottom:0;padding:0 6px;-webkit-appearance:none;-moz-appearance:none;appearance:none;font-size:15px;padding:12px;padding-right:25px;border:1px solid #e3e3e3;background:#ffffff;}.formkit-form[data-uid="134c4e25db"] [data-group="dropdown"] select:focus{outline:none;}.formkit-form[data-uid="134c4e25db"] [data-group="checkboxes"]{text-align:left;margin:0;}.formkit-form[data-uid="134c4e25db"] [data-group="checkboxes"] [data-group="checkbox"]{margin-bottom:10px;}.formkit-form[data-uid="134c4e25db"] [data-group="checkboxes"] [data-group="checkbox"] *{cursor:pointer;}.formkit-form[data-uid="134c4e25db"] [data-group="checkboxes"] [data-group="checkbox"]:last-of-type{margin-bottom:0;}.formkit-form[data-uid="134c4e25db"] [data-group="checkboxes"] [data-group="checkbox"] input[type="checkbox"]{display:none;}.formkit-form[data-uid="134c4e25db"] [data-group="checkboxes"] [data-group="checkbox"] input[type="checkbox"] + label::after{content:none;}.formkit-form[data-uid="134c4e25db"] [data-group="checkboxes"] [data-group="checkbox"] input[type="checkbox"]:checked + label::after{border-color:#ffffff;content:"";}.formkit-form[data-uid="134c4e25db"] [data-group="checkboxes"] [data-group="checkbox"] input[type="checkbox"]:checked + label::before{background:#10bf7a;border-color:#10bf7a;}.formkit-form[data-uid="134c4e25db"] [data-group="checkboxes"] [data-group="checkbox"] label{position:relative;display:inline-block;padding-left:28px;}.formkit-form[data-uid="134c4e25db"] [data-group="checkboxes"] [data-group="checkbox"] label::before,.formkit-form[data-uid="134c4e25db"] [data-group="checkboxes"] [data-group="checkbox"] label::after{position:absolute;content:"";display:inline-block;}.formkit-form[data-uid="134c4e25db"] [data-group="checkboxes"] [data-group="checkbox"] label::before{height:16px;width:16px;border:1px solid #e3e3e3;background:#ffffff;left:0px;top:3px;}.formkit-form[data-uid="134c4e25db"] [data-group="checkboxes"] [data-group="checkbox"] label::after{height:4px;width:8px;border-left:2px solid #4d4d4d;border-bottom:2px solid #4d4d4d;-webkit-transform:rotate(-45deg);-ms-transform:rotate(-45deg);transform:rotate(-45deg);left:4px;top:8px;}.formkit-form[data-uid="134c4e25db"] .formkit-alert{background:#f9fafb;border:1px solid #e3e3e3;border-radius:5px;-webkit-flex:1 0 auto;-ms-flex:1 0 auto;flex:1 0 auto;list-style:none;margin:25px auto;padding:12px;text-align:center;width:100%;}.formkit-form[data-uid="134c4e25db"] .formkit-alert:empty{display:none;}.formkit-form[data-uid="134c4e25db"] .formkit-alert-success{background:#d3fbeb;border-color:#10bf7a;color:#0c905c;}.formkit-form[data-uid="134c4e25db"] .formkit-alert-error{background:#fde8e2;border-color:#f2643b;color:#ea4110;}.formkit-form[data-uid="134c4e25db"] .formkit-spinner{display:-webkit-box;display:-webkit-flex;display:-ms-flexbox;display:flex;height:0px;width:0px;margin:0 auto;position:absolute;top:0;left:0;right:0;width:0px;overflow:hidden;text-align:center;-webkit-transition:all 300ms ease-in-out;transition:all 300ms ease-in-out;}.formkit-form[data-uid="134c4e25db"] .formkit-spinner > div{margin:auto;width:12px;height:12px;background-color:#fff;opacity:0.3;border-radius:100%;display:inline-block;-webkit-animation:formkit-bouncedelay-formkit-form-data-uid-134c4e25db- 1.4s infinite ease-in-out both;animation:formkit-bouncedelay-formkit-form-data-uid-134c4e25db- 1.4s infinite ease-in-out both;}.formkit-form[data-uid="134c4e25db"] .formkit-spinner > div:nth-child(1){-webkit-animation-delay:-0.32s;animation-delay:-0.32s;}.formkit-form[data-uid="134c4e25db"] .formkit-spinner > div:nth-child(2){-webkit-animation-delay:-0.16s;animation-delay:-0.16s;}.formkit-form[data-uid="134c4e25db"] .formkit-submit[data-active] .formkit-spinner{opacity:1;height:100%;width:50px;}.formkit-form[data-uid="134c4e25db"] .formkit-submit[data-active] .formkit-spinner ~ span{opacity:0;}.formkit-form[data-uid="134c4e25db"] .formkit-powered-by[data-active="false"]{opacity:0.35;}.formkit-form[data-uid="134c4e25db"] .formkit-powered-by-convertkit-container{display:-webkit-box;display:-webkit-flex;display:-ms-flexbox;display:flex;width:100%;margin:10px 0;position:relative;}.formkit-form[data-uid="134c4e25db"] .formkit-powered-by-convertkit-container[data-active="false"]{opacity:0.35;}.formkit-form[data-uid="134c4e25db"] .formkit-powered-by-convertkit{-webkit-align-items:center;-webkit-box-align:center;-ms-flex-align:center;align-items:center;background-color:#ffffff;border-radius:9px;color:#3d3d3d;cursor:pointer;display:block;height:36px;margin:0 auto;opacity:0.95;padding:0;-webkit-text-decoration:none;text-decoration:none;text-indent:100%;-webkit-transition:ease-in-out all 200ms;transition:ease-in-out all 200ms;white-space:nowrap;overflow:hidden;-webkit-user-select:none;-moz-user-select:none;-ms-user-select:none;user-select:none;width:157px;background-repeat:no-repeat;background-position:center;background-image:url("data:image/svg+xml;charset=utf8,%3Csvg width='133' height='36' viewBox='0 0 133 36' fill='none' xmlns='http://www.w3.org/2000/svg'%3E%3Cpath d='M0.861 25.5C0.735 25.5 0.651 25.416 0.651 25.29V10.548C0.651 10.422 0.735 10.338 0.861 10.338H6.279C9.072 10.338 10.668 11.451 10.668 13.824C10.668 15.819 9.219 16.932 8.001 17.226C7.707 17.268 7.707 17.625 8.022 17.688C9.912 18.108 11.088 19.116 11.088 21.321C11.088 23.715 9.429 25.5 6.426 25.5H0.861ZM5.397 23.085C6.825 23.085 7.518 22.224 7.518 21.006C7.518 19.683 6.825 18.948