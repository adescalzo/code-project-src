```yaml
---
title: How To Be More Productive When Creating CRUD APIs in .NET
source: https://antondevtips.com/blog/how-to-be-more-productive-when-creating-crud-apis-in-dotnet
date_published: 2024-11-05T11:55:44.629Z
date_captured: 2025-08-06T17:29:41.794Z
domain: antondevtips.com
author: Anton Martyniuk
category: backend
technologies: [.NET, ASP.NET Core, Modern (library), EF Core, Dapper, MongoDB, LiteDB, Npgsql.EntityFrameworkCore.PostgreSQL, Microsoft.EntityFrameworkCore.Design, Redis, Swagger, MediatR, OData]
programming_languages: [C#, SQL, Bash]
tags: [crud, web-api, dotnet, productivity, data-access, orm, micro-orm, architecture, code-generation, boilerplate]
key_concepts: [crud-apis, boilerplate-reduction, n-tier-architecture, vertical-slice-architecture, generic-repositories, generic-services, source-generators, dependency-injection, caching, cqrs]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces "Modern," a custom set of .NET libraries designed to significantly enhance productivity when developing CRUD APIs by minimizing boilerplate code. It demonstrates how to quickly set up a functional ASP.NET Core Web API for a `Ticket` entity using Modern, EF Core, and PostgreSQL, showcasing generic repositories, services, and controllers. The post also explores advanced features such as source generators for model creation, customization of the architectural layers (repository, service, controller), and integration with patterns like N-Tier and Vertical Slice Architecture. Modern supports various databases including EF Core, Dapper, MongoDB, and LiteDB, and offers built-in caching capabilities with Redis or in-memory stores.
---
```

# How To Be More Productive When Creating CRUD APIs in .NET

![A dark blue and purple banner image with the text "HOW TO BE MORE PRODUCTIVE WHEN CREATING CRUD APIs IN .NET" and a "dev tips" logo.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_asp_modern_crud.png&w=3840&q=100)

# How To Be More Productive When Creating CRUD APIs in .NET

Nov 5, 2024

[Download source code](/source-code/how-to-be-more-productive-when-creating-crud-apis-in-dotnet)

6 min read

### Newsletter Sponsors

[Master Key .Net Tools and Skills](https://amzn.to/3Tea89H). Learn debugging, source code management, testing, cloud-native development, intelligent apps, and more with [Tools and Skills by .NET 8](https://amzn.to/3Tea89H) by bestseller Author Mark J Price.

[Master The Clean Architecture](https://www.courses.milanjovanovic.tech/a/aff_q2snzxhl/external?affcode=1486372_j2vpyytw). This comprehensive course will teach you how to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture. Join 3,150+ students to accelerate your growth as a software architect.

[Master The Modular Monolith Architecture](https://www.courses.milanjovanovic.tech/a/aff_l825rpg0/external?affcode=1486372_j2vpyytw). This in-depth course will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario. Join 1,050+ students to accelerate your growth as a software architect.

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

I have been creating .NET webapps for more than 10 years. I have created a lot of complex applications and a lot of CRUD APIs. CRUD APIs by their nature are straightforward, but in every project you need to write the same boilerplate code to create, update, delete and read entities from the database.

A few years ago, I was looking for ready free solutions that will allow me to ship CRUD APIs faster. And I couldn't find the perfect solution. So I created my own.

In today's blog post, I will show you tools that allowed me to be more productive when creating CRUD APIs in .NET. I will show you examples of how to use these tools in an N-Tier (Layered) and Vertical Slice Architecture.

## Getting Started With a Modern Set of Libraries

I created a set of libraries called [Modern](https://github.com/anton-martyniuk/Modern) for fast and efficient development of CRUD APIs in .NET. Have a look at the documentation on the [Github page](https://github.com/anton-martyniuk/Modern/wiki).

It allows creating a production ready applications with just a set of models and configuration which can be further extended. Modern tools are flexible, easily changeable and extendable.

**It includes the following components:**

*   generic repositories for SQL and NoSQL databases
*   generic services with and without caching support
*   generic in memory services with in-memory filtering capabilities
*   in-memory and redis generic caches
*   generic set of CQRS queries and commands over repository (if you prefer CQRS over services)
*   generic controllers for all types of services
*   OData controllers for all types of services

**Modern supports the following databases and frameworks:**

*   EF Core
*   Dapper
*   MongoDB
*   LiteDB

Let's get started. Let's create a Web Api with CRUD operations for Tickets.

**Step 1:** Create a regular ASP.NET Core Web API project and add the following Nuget packages

```bash
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package Microsoft.EntityFrameworkCore.Design

dotnet add package Modern.Repositories.EFCore
dotnet add package Modern.Services.DataStore
dotnet add package Modern.Controllers.DataStore
```

**Step 2:** Create database Ticket entity

```csharp
public class Ticket
{
    public int Id { get; set; }
    public string Number { get; set; }
    public string Description { get; set; }
    public string Status { get; set; }
    public decimal Price { get; set; }
    public DateTime PurchasedAtUtc { get; set; }
}
```

**Step 3:** Configure EF Core

```csharp
public class EfCoreDbContext(DbContextOptions<EfCoreDbContext> options) : DbContext(options)
{
    public DbSet<Ticket> Tickets { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("tickets");

        modelBuilder.Entity<Ticket>(entity =>
        {
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Number);
        });
    }
}

var postgresConnectionString = configuration.GetConnectionString("Postgres");

services.AddDbContext<EfCoreDbContext>(x => x
    .UseNpgsql(postgresConnectionString)
    .UseSnakeCaseNamingConvention()
);
```

**Step 4:** Create public contract models for the Ticket entity

```csharp
public record TicketDto
{
    public required int Id { get; init; }
    public required string Number { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public required decimal Price { get; init; }
}

public record CreateTicketRequest
{
    public required string Number { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public required decimal Price { get; init; }
}

public record UpdateTicketRequest
{
    public required string Id { get; init; }
    public required string Number { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public required decimal Price { get; init; }
}
```

**Step 5:** Register and map controllers in DI with Swagger support

```csharp
builder.Services.AddControllers();

builder.Services
    .AddEndpointsApiExplorer()
    .AddSwaggerGen();

// ...

var app = builder.Build();
app.MapControllers();
```

**Step 6:** Create database migration with EF Core Tools

**Step 7:** Register Modern tools in DI

```csharp
builder.Services
    .AddModern()
    .AddRepositoriesEfCore(options =>
    {
        options.AddRepository<EfCoreDbContext, Ticket, int>();
    })
    .AddServices(options =>
    {
        options.AddService<TicketDto, Ticket, int>();
    })
    .AddControllers(options =>
    {
        options.AddController<CreateTicketRequest, UpdateTicketRequest, TicketDto, Ticket, int>("api/tickets");
    });
```

Let's break down what we are registering here:

*   EF Core DbContext, type of Ticket entity, and type of the primary key (int).
*   Service that uses Ticket entity and returns TicketDto.
*   Controller that uses CreateTicketRequest, UpdateTicketRequest and TicketDto to expose CRUD API endpoints

Let's run the application and see what happens.

![A screenshot of a Swagger UI showing a list of automatically generated API endpoints for `/api/tickets`. The endpoints include GET, PUT, PATCH, DELETE for single entities, and GET, POST, PUT, DELETE for multiple entities (e.g., create-many, update-many, delete-many).](https://antondevtips.com/media/code_screenshots/aspnetcore/modern-crud-apis/img_1.png)

Within a few minutes, we have created a fully functional CRUD API for `Ticket` entity that has the following endpoints:

*   Get entity by id
*   get all entities
*   create entity
*   update entity
*   partial entity update
*   delete entity
*   create many entities
*   update many entities
*   delete many entities

## Source generation for WebApi models in Modern Libraries

You're not limited to only using DTO models. You can also use DBO models and expose them from the APIs, in case you need such an option:

```csharp
options.AddService<Ticket, Ticket, int>();
options.AddController<CreateTicketRequest, UpdateTicketRequest, Ticket, Ticket, int>("api/tickets");
```

I have created a Nuget package with source generators for WebApi models:

```bash
dotnet add package Modern.Controllers.SourceGenerators
```

You can add this attribute to the public contract model, and the source generator will create a `CreateTicketRequest` and `UpdateTicketRequest` models:

```csharp
[WebApiEntityRequest(CreateRequestName = "CreateTicketRequest",
    UpdateRequestName = "UpdateTicketRequest")]
public record TicketDto
{
    [IgnoreCreateRequest]
    public required int Id { get; init; }
    public required string Number { get; init; }
    public required string Description { get; init; }
    public required string Status { get; init; }
    public required decimal Price { get; init; }
}
```

Source generator uses all the properties for Create and Update requests. You can additionally specify what properties should be excluded from the requests by using the `IgnoreCreateRequest` or `IgnoreUpdateRequest`.

## Customizing Repositories, Services and Controllers

At its core, Modern libraries follow the classic N-Tier (Layered) approach:

*   Repository
*   Service
*   Controller

Each layer can be overridden and further extended.

Modern Repository supports the following [Query](https://github.com/anton-martyniuk/modern/wiki/Repository-Query-data) and [Write](https://github.com/anton-martyniuk/modern/wiki/Service-CRUD-Data) operations.

Modern Repository has `Where` method that you can use to filter data by a given condition. However, you may need to create your own methods.

You can create your own repository interface that inherits from `IModernRepository<TEntity, TId>`. And create an implementation that inherits from `ModernEfCoreRepository<TDbContext, TEntity, TId>`.

Here is an example:

```csharp
public interface ICustomTicketRepository: IModernRepository<Ticket, int>
{
    Task<List<Ticket>> GetTicketsByDateAsync(DateTime date);
}

public class CustomTicketRepository
    : ModernEfCoreRepository<EfCoreDbContext, Ticket, int>, ICustomTicketRepository
{
    public CustomTicketRepository(
        EfCoreDbContext dbContext,
        IOptions<EfCoreRepositoryConfiguration> configuration)
        : base(dbContext, configuration)
    {
    }

    public async Task<List<Ticket>> GetTicketsByDateAsync(DateTime date)
    {
        return await DbContext.Tickets
            .Where(x => x.PurchasedAtUtc >= date)
            .ToListAsync();
    }
}
```

In the same manner you can create your own service interface that inherits from `IModernService<TEntityDto, TEntityDbo, TId>`. And create an implementation that inherits from `ModernService<TEntityDto, TEntityDbo, TId>`.

```csharp
public interface ICustomTicketService : IModernService<TicketDto, Ticket, int>
{
    Task<List<Ticket>> GetTicketsByDateAsync(DateTime date);
}

public class CustomTicketService : ModernService<TicketDto, Ticket, int>, ICustomTicketService
{
    private readonly ICustomTicketRepository _repository;

    public CustomTicketService(
        ICustomTicketRepository repository,
        ILogger<CustomTicketService> logger)
            : base(repository, logger)
    {
        _repository = repository;
    }

    public async Task<List<Ticket>> GetTicketsByDateAsync(DateTime date)
    {
        return await _repository.GetTicketsByDateAsync(date);
    }
}
```

The same goes with Controllers, you can create a custom Controller that inherits from `ModernController<TCreateRequest, TUpdateRequest, TEntityDto, TEntityDbo, TId>`:

```csharp
public record GetTicketsByDateRequest(DateTime Date);

[ApiController]
[Route("/api/custom-tickets")]
public class CustomTicketController
    : ModernController<CreateTicketRequest, UpdateTicketRequest, TicketDto, Ticket, int>
{
    private readonly ICustomTicketService _service;

    public CustomTicketController(ICustomTicketService service) : base(service)
    {
        _service = service;
    }

    [HttpGet("get-by-date")]
    public async Task<IActionResult> GetTicketsByDate(
        [Required, FromBody] GetTicketsByDateRequest request)
    {
        var entities = await _service.GetTicketsByDateAsync(request.Date).ConfigureAwait(false);
        return Ok(entities);
    }
}
```

After creating your own implementations, you need to register them as **Concrete** implementations:

```csharp
builder.Services
    .AddModern()
    .AddRepositoriesEfCore(options =>
    {
        options.AddRepository<EfCoreDbContext, Ticket, int>();
        options.AddConcreteRepository<ICustomTicketRepository, CustomTicketRepository>();
    })
    .AddServices(options =>
    {
        options.AddService<TicketDto, Ticket, int>();
        options.AddConcreteService<ICustomTicketService, CustomTicketService>();
    })
    .AddControllers(options =>
    {
        options.AddController<CreateTicketRequest, UpdateTicketRequest, TicketDto, Ticket, int>("api/tickets");
        options.AddController<CustomTicketController>();
    });
```

In all previous examples, we were using our own customized implementations of a Repository and Service. What if you don't need to create your own version of Repository or Service and use just a base version from the package?

The first option is to use the base interface of `IModernService` or `IModernRepository` from the Modern package:

```csharp
// Use IModernService<...> instead of ICustomTicketService
IModernService<TicketDto, Ticket, int> service

// Use IModernRepository<...> instead of ICustomTicketRepository
IModernRepository<Ticket, int> repository
```

If you dislike such long types, you can create your own interface and implementation that inherit from the base types and make them empty. But this can be tedious.

If this is the case, you can use the following packages with source generators:

```bash
dotnet add package Modern.Repositories.EFCore.SourceGenerators
dotnet add package Modern.Services.DataStore.SourceGenerators
```

You can need to use a marker empty class and specify the `ModernEfCoreRepository` attribute:

```csharp
[ModernEfCoreRepository(typeof(EfCoreDbContext), typeof(Ticket), typeof(int))]
public class IServiceMarker;
```

To autogenerate service, you need to add a `ModernService` attribute to the Dto class:

```csharp
[ModernService(typeof(Ticket))]
public class TicketDto
{
}
```

**NOTE:** source generator only supports classes for Dto at the moment.

As a result, source generators will create the following classes:

![A screenshot of a Visual Studio Solution Explorer showing the generated C# files under the "Source Generators" node for a project. It highlights `Modern.Controllers.SourceGenerators.EntityRequestGenerator`, `Modern.Repositories.EFCore.SourceGenerators.ModernEfCoreRepositoryGenerator`, and `Modern.Services.DataStore.SourceGenerators.ModernServiceGenerator`, with their respective generated files like `TicketDto_EntityRequests_gen.cs`, `TicketRepository_efcore_gen.cs`, and `NewTicketService_service_gen.cs`.](https://antondevtips.com/media/code_screenshots/aspnetcore/modern-crud-apis/img_2.png)

## Using Modern Libraries in Vertical Slice Architecture

If you prefer using [Vertical Slice Architecture](https://antondevtips.com/blog/vertical-slice-architecture-the-best-ways-to-structure-your-project) or even combination of Vertical Slices and [Clean Architecture](https://antondevtips.com/blog/the-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices), you can find Modern libraries very helpful for you.

You can create a minimal API Endpoint for your `CreateTicketEndpoint` Vertical Slice:

```csharp
public class CreateTicketEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapPost("/api/tickets/v2", Handle);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateTicketRequestV2 request,
        IValidator<CreateTicketRequestV2> validator,
        ITicketRepository ticketRepository,
        CancellationToken cancellationToken)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Results.ValidationProblem(validationResult.ToDictionary());
        }

        var entity = request.MapToEntity();
        entity = await ticketRepository.CreateAsync(entity, cancellationToken);

        var response = entity.MapToResponse();
        return Results.Ok(response);
    }
}
```

Here I directly use `ITicketRepository` that can eliminate writing some boilerplate code.

## Additional Features Supported By Modern Libraries

Modern supports repositories for the following databases and frameworks:

*   EF Core: EF Core DbContext, DbContextFactory and UnitOfWork
*   Dapper (doesn't support dynamic filtering)
*   MongoDB
*   LiteDB

Modern supports the following services:

*   Services that perform CRUD operations over entities in the database
*   Services that add a caching layer (InMemory or Redis) to save items and retrieve items by id
*   Services that use a full in-memory cache that has all items cached

If you prefer using MediatR, Modern supports the following:

*   Queries and Commands that perform CRUD operations over entities in the database
*   Queries and Commands that add a caching layer (InMemory or Redis) to save items and retrieve items by id

Modern supports the following controllers:

*   Controllers that perform CRUD operations over entities in the database (use regular or cached service)
*   Controllers that use a full in-memory service

## Summary

Modern libraries provide various components that help you write less boilerplate code for your CRUD APIs. I definitely recommend checking this library on the [GitHub](https://github.com/anton-martyniuk/Modern) and the documentation on the [GitHub Wiki](https://github.com/anton-martyniuk/Modern/wiki).

If you like the library - make sure to give a star on the GitHub.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-be-more-productive-when-creating-crud-apis-in-dotnet)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-be-more-productive-when-creating-crud-apis-in-dotnet&title=How%20To%20Be%20More%20Productive%20When%20Creating%20CRUD%20APIs%20in%20.NET)[X](https://twitter.com/intent/tweet?text=How%20To%20Be%20More%20Productive%20When%20Creating%20CRUD%20APIs%20in%20.NET&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-be-more-productive-when-creating-crud-apis-in-dotnet)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-be-more-productive-when-creating-crud-apis-in-dotnet)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.