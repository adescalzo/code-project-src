```yaml
---
title: "Effective Integration Testing with a Database in .NET: A Comprehensive Guide | NimblePros Blog"
source: https://blog.nimblepros.com/blogs/integration-testing-with-database/?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=effective-integration-testing-with-a-database-in-net&_bhlid=a98b5fa7c7bba747edbec6e2d7fb8c2f6174c164
date_published: unknown
date_captured: 2025-08-08T13:48:26.074Z
domain: blog.nimblepros.com
author: Unknown
category: testing
technologies: [.NET, ASP.NET Core, Entity Framework Core, xUnit, WebApplicationFactory, Microsoft.AspNetCore.Mvc.Testing, SQL Server, MediatR, Ardalis.Specification, NuGet, SQLite, Visual Studio]
programming_languages: [C#, SQL]
tags: [integration-testing, dotnet, database, entity-framework-core, xunit, web-api, data-access, test-automation, software-testing, test-isolation]
key_concepts: [integration-testing, webapplicationfactory, dependency-injection, repository-pattern, in-memory-database, database-migrations, test-isolation, parallel-testing]
code_examples: false
difficulty_level: intermediate
summary: |
  This comprehensive guide details effective integration testing with databases in .NET applications, primarily focusing on Entity Framework Core. It explores various testing strategies, including mocking, in-memory providers (EF Core and SQLite), and advocates for using the same production database provider for reliability. The article covers setting up the testing environment with `WebApplicationFactory`, managing database migrations and seeding, and resolving parallel test conflicts using xUnit features. It emphasizes the importance of test isolation and achieving confidence in test results by interacting with a real database.
---
```

# Effective Integration Testing with a Database in .NET: A Comprehensive Guide | NimblePros Blog

# Effective Integration Testing with a Database in .NET: A Comprehensive Guide

September 03, 2024#Testing

![Article banner showing "Effective Integration Testing with a Database in .NET" on a chalkboard background with a checklist.](/static/06c0d846ef9d909eee4e4a348f4e09ad/7b044/integration-testing-with-database-net.png)

## Introduction

Let’s assume we understand the need for a good suite of [integration tests](https://ardalis.com/unit-test-or-integration-test-and-why-you-should-care/). Personally, these are my sweet spots for the level of testing. For a decently sized brownfield application, you can’t beat the _bang for your buck_ you get with a decent set of integration tests, in my opinion. Getting up to speed with effective integration tests for your .NET application can be tricky, especially when a database is involved.

Do I fake the database, use the production database (spoiler alert, no you shouldn’t), or use a dedicated test database? How do things stay in sync as the application evolves, what if I’m using Entity Framework Core, what if I’m not? How do I isolate test effects? What about schema changes?

I had _all_ the questions when I first started with this. We’ll go step by step and answer all of these.

### Assumptions

*   We’ll mainly focus on Entity Framework Core as a means for data access, as there are some nuances here to take into consideration. But a lot of the lessons learned here are applicable even without it.
*   The examples will use [xUnit](https://github.com/xunit/xunit) (the .NET testing framework of choice).
*   You’re working on an ASP.NET Core application using [Dependency Injection](https://deviq.com/practices/dependency-injection)

## Why Integration Testing Matters

Integration tests are all about checking that the different pieces of your app play nicely together. For EF Core, that means making sure your code talks to the database correctly—whether it’s querying data, saving changes, or handling complex operations. Unlike unit tests, which are all about isolating code, integration tests let you see the whole picture. When it comes to EF Core, integration tests can catch issues that might slip through unit tests, like:

*   Incorrect database schema
*   Mismatched entity configurations
*   Performance issues with complex queries
*   Problems with database migrations

## Setting Up Your Testing Environment

In your IDE, set up a new _unit test_ project for xUnit:

![Screenshot of Visual Studio's "Create a new project" dialog, with "xUnit Test Project" highlighted.](/static/3a1ccd437e503c2ea58d6fedfdbb6d91/f058b/visual-studio-create-xUnit-screen-shot.png)

Then, reference the Web API project you’re trying to test.

If you are testing your API layer (and sometimes, even if you’re not - more on this later) you’ll want to use the [WebApplicationFactory](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0#customize-webapplicationfactory). For this we need to add the `Microsoft.AspNetCore.Mvc.Testing` framework.

`dotnet add package Microsoft.AspNetCore.Mvc.Testing`

Then go ahead and set up a _CustomWebApplicationFactory_:

```csharp
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    // Dedicated Test database
    private readonly string _connectionString = "...";
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            // Remove the existing AppDbContext registration
            services.RemoveAll<DbContextOptions<AppDbContext>>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlServer(_connectionString);
            });
        });

        builder.UseEnvironment("Development");
    }
}
```

The great thing about the WebApplicationFactory is that it bootstraps the application just how it would in production. So whatever is in your `Program.cs` or `Startup.cs` (if you haven’t switched styles yet) will be executed and wired up.

The `ConfigureWebHost` method gives you a hook to do some additional setup that will only get executed for your test projects. Usually, we do things like reconfigure certain services (to point to _Test_ or _Fake_ instances instead of production), replace configurations, etc. In the case above, we’re just pointing the database to a test-specific instance.

## Executing Test Cases

In your test class you can simply instantiate the factory to start using it: `var factory = new CustomWebApplicationFactory()`.

The _preferred_ method of running your test cases with the Web app factory is to use the `HttpClient`:

```csharp
var factory = new CustomWebApplicationFactory();
var client = factory.CreateClient();
var result = await client.GetFromJsonAsync<List<Customer>>("api/customers");

result.Should().HaveCount(3);
```

The benefit here is that this runs through the entire ASP.NET core pipeline: HTTP protocol, JSON serialization and deserialization, authentication, filters, middleware, etc. These are closer to _[functional tests](https://en.wikipedia.org/wiki/Functional_testing)_.

An alternative to this is to skip the ASP.NET layer and test your services directly. These are sometimes referred to as _[subcutaneous tests](https://martinfowler.com/bliki/SubcutaneousTest.html)_.

```csharp
var factory = new CustomWebApplicationFactory();
var sut = factory.Services.GetRequiredService<ICustomerService>();

var result = await sut.GetCustomers();

result.Should().HaveCount(3);

/// Or in the case of MediatR
var sut = factory.Services.GetRequiredService<IMediator>();
var result = await sut.Send(new GetCustomerRequest());

result.Should().HaveCount(3);
```

These can sometimes be extremely valuable if your services and UI are separated cleanly.

## To Fake or Not to Fake - That is the Question

So now for the fun part. Well, sort of. First, we need to make some decisions. We have to decide which [testing strategy](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy) we want to follow.

Here are the options (hint: I saved the best for last):

### Mock the Database - Repository Pattern

If your database access is firmly rooted behind an implementation of the _[Repository Pattern](https://deviq.com/design-patterns/repository-pattern)_, like the one provided by the [Ardalis.Specification](https://www.nuget.org/packages/Ardalis.Specification) package, then you can simply skip the database altogether. At this point, you could replace the repository implementations with a custom one for your tests. You could use a mocking framework for this or build custom in-memory implementations that are backed by regular C# `List<T>`.

Some argue that EF Core is thoroughly tested by Microsoft, so why bother _retesting_ it?

Pros:

*   Your tests are lightning-fast
*   No database setup is required

Cons:

*   Setup can be involved

### [EF Core In-memory provider](https://learn.microsoft.com/en-us/ef/core/providers/in-memory/)

I’ve seen way too many developers (including myself) be lulled into a false sense of security with this option. On the surface, it seems like an answer to all of our problems. There’s no hardware to worry about, no modifications to your application code are required, and you can swap it out and everything just _magically_ continues to work.

But I caution you, just don’t. Do not pass Go, do not collect $200.

One obvious problem is if you application code uses RAW SQL queries for some things. There are times when you might need to bypass LINQ and the entities and just run some raw queries (usually for performance reasons). If you’re not hitting a real database, you can’t test areas of your application.

So you don’t use raw SQL in your application, so you figure why not?

Well, this method is ”[highly discouraged](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy#in-memory-as-a-database-fake)” from Microsoft. That’s some strong language. I can’t remember a time when they’ve come on this strong before. Typically, Microsoft just provides the tools, they may give you their recommendation, but they never usually advise you _not_ to do something.

Because this provider has nothing to do with SQL you’re going to have false positives in your tests for certain things.

Consider this code snippet:

```csharp
var employee = dbContext.Employee.First(x => x.FirstName == "kevin");
Console.WriteLine(employee.Department.Name);
```

If this is run on SQL Server the application would throw an exception when printing the department name. This is because lazy loading is off by default and we didn’t run a `.Include(...)` to pull in the Department navigation property.

The in-memory provider would happily carry along, giving a false positive test.

Also, SQL Server, by default, is case-insensitive with its queries. This means `== "kevin"` and `== "Kevin"` would both return the same result.

The in-memory provider is case-sensitive. One of these would return and the other would fail.

```csharp
var employee = dbContext.Employee.First(x => x.DepartmentId == 10);
Console.WriteLine(employee.FirstName);

var employee = dbContext.Employee.First(x => x.Department.Id == 10);
Console.WriteLine(employee.FirstName);
```

Because the SQL Provider understands the concept of navigation properties and foreign keys, both of these statements would translate to the same SQL query and return the same results.

The in-memory provider is only looking at the Employee object it has stored. These statements would have different results depending on how the data is set up (setting `DepartmentId` on the Employee or setting `Id` on the navigation property `Department`).

And it’s these subtle differences that make the in-memory provider dangerous, in my opinion.

Now, with all these warnings in place, if you know _**exactly**_ what you’re doing, here’s how you can get started with the in-memory provider:

Add the NuGet package:

`dotnet add package Microsoft.EntityFrameworkCore.InMemory`

Replace the code in `ConfigureWebHost` with the following:

```csharp
services.AddDbContext<AppDbContext>(options =>
{
    options.UseInMemoryDatabase("AppDb");
});
```

Pros:

*   Same as repository the pattern above
*   Setup is much simpler than the repository pattern

Cons:

*   Doesn’t work with raw SQL
*   Doesn’t support transactions
*   **Highly discouraged** by Microsoft
*   False positives: may _appear_ to work then fail you in subtle ways later (usually in production)
*   Ignore foreign key constraints
*   Will allow certain data setups that are impossible in production

### SQLite In-Memory provider

If you find the _need_ to use an in-memory database implementation, the [SQLite provider](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/) is a far better bet for a fake database. You _can_ run raw SQL, and it does respect foreign key constraints.

But there are still issues. You will have similar inconsistencies with case sensitivity. SQLite has a much more limited set of data types than other database systems, which could introduce subtle differences. And depending on the SQL features you’re using in your application code, they won’t be supported in SQLite.

Here is how we implement SQLite into our testing strategy:

Add the NuGet package:

`dotnet add package Microsoft.EntityFrameworkCore.Sqlite`

Replace the code in `ConfigureWebHost` with the following:

```csharp
// Create open SqliteConnection so EF won't automatically close it.
services.AddSingleton<DbConnection>(container =>
{
    var connection = new SqliteConnection("DataSource=:memory:");
    connection.Open();
    return connection;
});

services.AddDbContext<AppDbContext>((container, options) =>
{
    var connection = container.GetRequiredService<DbConnection>();
    options.UseSqlite(connection);
});
```

Pros:

*   Fast
*   No hardware requirements
*   Respects foreign key constraints
*   Raw SQL _may_ work
*   Can work for simple implementations

Cons:

*   SQL compatibility is different than other production database systems.

### Use the Same Provider that Your Application Code Uses

I’ve been bitten enough by subtle issues with the above options that this is my go-to method. Simply use the provider you’re using in your application code. Save yourself hours (honestly probably weeks collectively) and brain cells that I’ve already burned trying to make other methods work.

Pros:

*   Confidence that what works in test will work in production and vice versa.

Cons:

*   _Slower_ tests

## Database Migrations and Seeding

Now that you’ve selected a proper EF Core provider, you will need a way to set up your database before each test.

This startup code needs to be run before your tests start up. One way to implement this is to have your test case implement `IAsyncLifetime`. Then implement the method `InitializeAsync` as follows:

```csharp
public async Task InitializeAsync()
{
    var context = factory.Services.GetRequiredService<AppDbContext>();

    await context.Database.EnsureDeletedAsync();
    await context.Database.EnsureCreatedAsync();

    await SeedDatabaseWithTestData(context);
}
```

Now you’ll need to add this to each and every test class. Oooooor you can use a handy feature of xUnit to share context between tests:

1.  Move the implementation of `InitializeAsync` to the `CustomWebApplicationFactory`
2.  Make sure each test implements: [`IClassFixture<CustomWebApplicationFactory>`](https://xunit.net/docs/shared-context#class-fixture)
3.  Add a constructor to the tests that take `CustomWebApplicationFactory` as a parameter

```csharp
public class MyTest : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    // CustomWebApplicationFactory will be automatically injected by xUnit
    public MyTest(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }
    
    [Fact]
    public async Task Testing_Using_HttpClient()
    {
        var client = _factory.CreateClient();
        var result = await client.GetFromJsonAsync<List<Customer>>("api/customers");
        result.Should().HaveCount(3);
    }
}
```

So what have we accomplished? Every time a test class gets instantiated, the existing database is deleted (one of the best ways to clean up, IMO) and re-created. Then initial data is seeded (populated) into the database.

Your test code is free to interact with the database in whatever way it needs to, with the confidence that the database is in a _clean_ state.

Do note, that the database setup code will be run once for each test class even if there are multiple test methods in the class.

## Solving Parallel Test Conflicts

If you took my advice and you’re running EF Core with the same database provider as your production system, you may have issues where multiple test classes are trying to interact with the database at the same time. At best, your tests aren’t going to be as isolated as you think, since multiple tests could be adding or deleting data at the same time. At worst, one set of tests would be deleting and recreating the database while it’s still being used by another set of tests.

Why is that? Well, it has to do with the defaults in xUnit. xUnit uses a [collection](https://xunit.net/docs/running-tests-in-parallel.html#parallelism-in-test-frameworks) as a mechanism to group tests. By default, every test class is a unique collection fixture, all tests in a collection fixture run sequentially, but collections run in parallel.

There are simple solutions to this problem:

### Disable Parallelism

Somewhere in your Integration Test assembly, place the following code:

```csharp
[assembly: CollectionBehavior(DisableTestParallelization = true)]
```

Doesn’t get easier than this. Note that this applies to the whole assembly. If you have multiple types of tests (unit, integration, functional) in the same assembly (highly discouraged, but I’ve seen it in the past) other tests that don’t involve your database will also be forced to run sequentially.

### Use a Common Collection Fixture

We can group the tests that interact with the database so that they all run as the same collection, by utilizing a [collection fixture](https://xunit.net/docs/shared-context.html#collection-fixture). In our case the changes would look like this:

```csharp
[CollectionDefinition(nameof(DatabaseCollection))]
public class DatabaseCollection : ICollectionFixture<CustomWebApplicationFactory>
{
   // just a marker class for xUnit
}

// drop the IClassFixture from the tests and add the Collection attribute:
[Collection(nameof(DatabaseCollection))]
public class Test1: IAsyncLifetime
{
    //....
}
```

### Change the Default Collection Behavior

You could also add another assembly marker that tells xUnit instead of using each class as a collection, the entire assembly should be used as one collection:

```csharp
[assembly: CollectionBehavior(CollectionBehavior.CollectionPerAssembly)]
```

## Wrapping Up

And there you have it folks! We’ve covered the _why’s_, setting up, gotchas with different EF Core providers, migrations, and parallel conflicts. Oof, that seems like a look. But trust me, it pays off when you see your tests catching real-world issues before they hit production.

## Resources

*   [Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-8.0)
*   [EF Core - Choosing a testing strategy](https://learn.microsoft.com/en-us/ef/core/testing/choosing-a-testing-strategy)
*   [EF Core - Testing against your production database system](https://learn.microsoft.com/en-us/ef/core/testing/testing-with-the-database)
*   [Running Integration Tests in Build Pipelines with a Real Database](https://ardalis.com/running-integration-tests-in-build-pipelines-with-a-real-database/)