```yaml
---
title: How to Implement Multitenancy in ASP.NET Core with EF Core
source: https://antondevtips.com/blog/how-to-implement-multitenancy-in-asp-net-core-with-ef-core
date_published: 2024-08-20T11:00:07.845Z
date_captured: 2025-08-06T17:28:13.420Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [ASP.NET Core, Entity Framework Core, .NET, JWT, PostgreSQL, Carter, Npgsql, HTTP]
programming_languages: [C#, SQL]
tags: [multitenancy, asp.net-core, ef-core, saas, database, data-isolation, web-api, dotnet, architecture, security]
key_concepts: [Multitenancy, Discriminator Column, Global Query Filters, Change Tracking, Dependency Injection, JWT Claims, Middleware, Model Caching]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article details implementing multitenancy in ASP.NET Core applications using Entity Framework Core, specifically focusing on the "Discriminator Column" approach. It demonstrates how to leverage EF Core's Global Query Filters for automatic data isolation during read operations and override `SaveChangesAsync` to assign `TenantId` for write operations. The guide also covers integrating tenant identification through JWT claims and HTTP headers, and discusses advanced topics like conditional query filters and their performance impact. This comprehensive approach ensures secure and efficient data separation for multiple tenants within a single application instance.]
---
```

# How to Implement Multitenancy in ASP.NET Core with EF Core

![Cover image for the article "How to Implement Multitenancy in ASP.NET Core with EF Core", featuring a code icon and "dev tips" text on a dark background with purple accents.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fefcore%2Fcover_ef_multitenancy.png&w=3840&q=100)

# How to Implement Multitenancy in ASP.NET Core with EF Core

Aug 20, 2024

[Download source code](/source-code/how-to-implement-multitenancy-in-asp-net-core-with-ef-core)

8 min read

### Newsletter Sponsors

[EF Core is too slow?](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website) Discover how you can easily insert 14x faster (reducing saving time by 94%).  
Boost your performance with our method integrated within EF Core: Bulk Insert, update, delete, and merge.  
Join 5,000+ satisfied customers who have trusted our library since 2014.

[Learn more](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website)

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

**Multitenancy** is a software architecture that allows a single instance of a software application to serve multiple customers, called **tenants**. Each tenant's data is isolated and remains invisible to other tenants for security reasons. This architecture is commonly used in Software as a Service (SaaS) applications, where multiple organizations or users share the same application infrastructure while keeping their data secure and separate.

In this blog post, I will share with you my own experience on how to implement multitenancy in ASP.NET Core using Entity Framework Core (EF Core).

## Introduction to Multitenancy

Multitenancy offers several advantages, including:

*   **Cost Efficiency:** shared infrastructure reduces the overall cost of hosting, infrastructure and maintenance.
*   **Simplified Deployment:** centralized updates simplify maintenance and support.
*   **Scalability:** it allows easy scaling of resources for individual tenants based on their needs.
*   **Isolation and Security:** each tenant's data is isolated, ensuring security and privacy.

There are several approaches to separate data for each tenant in multi-tenant applications:

*   **Database-per-Tenant:** each tenant has its own database. This model offers strong data isolation but may increase costs with many tenants.
*   **Schema-per-Tenant:** a single database with separate schemas for each tenant. It provides a balance between isolation and resource sharing.
*   **Table-per-Tenant:** a single database and schema, with tenant-specific tables. This model is efficient but may complicate data management.
*   **Discriminator Column:** a single database, schema, and tables, with a column indicating the tenant. This is the simplest but least isolated model.

**Discriminator column** is one of the most popular approaches to implementing multitenancy. It is cheap in terms of development, deployment and management compared to other options. With modern technologies like ASP.NET Core and EF Core, you can implement discriminator columns in your application without negatively affecting performance and security.

In this blog post I will show you how to implement multitenancy with discriminator column. Let's explore an application that needs to be multi-tenant.

## Multitenant Application Example

Today we will implement multitenancy for the "Books" application that has the following entities:

*   Books
*   Authors
*   Users
*   Tenants

The `Tenant` entity holds information about each customer:

```csharp
public class Tenant : IAuditableEntity
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}
```

Every `Book`, `Author` and `User` entities in our database belong to a specific tenant. You need to create a `ITenantEntity` interface that should be inherited by all entities that need to be multi-tenant:

```csharp
public interface ITenantEntity
{
    public Guid? TenantId { get; set; }
}
```

Let's explore, for example, User and Book entities:

```csharp
public class User : IAuditableEntity, ITenantEntity
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    
    public Guid? TenantId { get; set; }
}

public class Book : IAuditableEntity, ITenantEntity
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required int Year { get; set; }
    public Guid AuthorId { get; set; }
    public Author Author { get; set; } = null!;

    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }

    public Guid? TenantId { get; set; }
}
```

Every entity has a foreign key relationship with `Tenant` entity, for example, User:

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users");

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Email);

        builder.Property(x => x.Id).ValueGeneratedOnAdd();
        builder.Property(x => x.Email).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.UpdatedAtUtc);

        builder.HasOne<Tenant>()
            .WithMany()
            .HasForeignKey(e => e.TenantId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
```

Depending on your application needs, you may have this foreign key or have a plain `TenantId` column without a reference.

## How To Implement Multitenancy in EF Core

To implement multitenancy, we need to do the following:

1.  After a user logs in, we need to add a tenant identifier in the user claims.
2.  From every request from the frontend (or another application) whether we need to retrieve or modify data, we need to get the user tenant identifier from the user claims.
3.  Whenever a user requests data from the backend - we can use EF Core [Global Query Filters](https://antondevtips.com/blog/global-query-filters-in-ef-core) to automatically filter only those records from the database that a user has access to.
4.  Whenever a user creates, updates or deletes data - we can automatically assign a "TenantId" column in the [Change Tracker](https://antondevtips.com/blog/understanding-change-tracking-for-better-performance-in-ef-core) with a user tenant identifier.

By utilizing EF Core capabilities (Global Query Filters and Change Tracker) we no longer need to write custom filters in each query or provide a tenant id in every create, update or delete action. EF Core helps us to implement multitenancy once that will be applied to all entities that need to be multi-tenant.

This approach secures your code from the critical bugs where you may forget to add a check for a tenant in one of the queries and expose another customer's data.

## Implementing Multitenancy for Read Operations in EF Core

We can implement multitenancy in EF Core DbContext that will automatically be applied to all entities that inherit from `ITenantEntity`.

Let's define a `TenantProvider` that will retrieve user and tenant identifiers from the current HttpRequest:

```csharp
public interface ITenantProvider
{
    TenantInfo GetCurrentTenantInfo();
}

public class TenantProvider : ITenantProvider
{
    private readonly TenantInfo _tenantInfo;

    public TenantProvider(IHttpContextAccessor accessor)
    {
        var userIdValue = accessor.HttpContext?.User.FindFirstValue("user-id");
        var tenantIdValue = accessor.HttpContext?.User.FindFirstValue("tenant-id");

        Guid? userId = Guid.TryParse(userIdValue, out var guid) ? guid : null;
        Guid? tenantId = Guid.TryParse(tenantIdValue, out guid) ? guid : null;

        _tenantInfo = new TenantInfo(userId, tenantId);
    }

    public TenantInfo GetCurrentTenantInfo() => _tenantInfo;
}
```

Here we retrieve the "user-id" and "tenant-id" from the `ClaimsPrinciple`.

You need to register the provider and `IHttpContextAccessor` in the DI:

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ITenantProvider, TenantProvider>();
```

We need to inject `ITenantProvider` into DbContext and create a public property for it:

```csharp
public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ITenantProvider tenantProvider)
    : DbContext(options)
{
    public ITenantProvider TenantProvider => tenantProvider;

    public DbSet<Author> Authors { get; set; } = default!;
    public DbSet<Book> Books { get; set; } = default!;
    public DbSet<User> Users { get; set; } = default!;
    public DbSet<Tenant> Tenants { get; set; } = default!;
}
```

In the `OnModelCreating` method you can specify Global Query Filters for all our tenant entities:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder.Entity<User>()
        .HasQueryFilter(x => x.TenantId.Equals(TenantProvider.GetCurrentTenantInfo().TenantId));

    modelBuilder.Entity<Author>()
        .HasQueryFilter(x => x.TenantId.Equals(TenantProvider.GetCurrentTenantInfo().TenantId));

    modelBuilder.Entity<Book>()
        .HasQueryFilter(x => x.TenantId.Equals(TenantProvider.GetCurrentTenantInfo().TenantId));

    base.OnModelCreating(modelBuilder);

    modelBuilder.HasDefaultSchema("devtips_multitenancy");
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(BookConfiguration).Assembly);
}
```

It is important to specify `HasQueryFilter` before calling `base.OnModelCreating(modelBuilder);`.

Whenever you add a new entity in your project, simply add a new query filter and all read operations for this entity will be filtered accordingly by a user's tenant. This ensures that a user will only have access to their own data.

> **It is crucial** to work with `TenantProvider` with a public property, otherwise EF Core Global Query Filters won't be applied correctly per each request.

## Implementing Multitenancy for Write Operations in EF Core

We can override `SaveChangesAsync` method in EF Core DbContext to implement multitenancy for write operations:

```csharp
public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
{
    var tenantInfo = TenantProvider.GetCurrentTenantInfo();

    var modifiedTenantEntries = ChangeTracker.Entries<ITenantEntity>()
        .Where(x => x.State is EntityState.Added or EntityState.Modified);

    foreach (var entry in modifiedTenantEntries)
    {
        entry.Entity.TenantId = tenantInfo.TenantId
            ?? throw new InvalidOperationException($"Tenant id is required but was not provided for entity '{entry.Entity.GetType()}' with state '{entry.State}'");
    }

    return await base.SaveChangesAsync(cancellationToken);
}
```

We iterate over a list of tenant entities and automatically set the `TenantId` property. If a tenant identifier is not available for any reason - an exception is thrown and the operation is aborted. A tenant identifier is required to be set whenever we create, update or delete an entity.

Now let's explore multitenancy in action.

## Adding Tenant Identifier to User Claims on a Login

When a user logs in, we need to use an `IgnoreQueryFilters` method to be able to search for a user in every tenant:

```csharp
public class LoginUserEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/users/login", Handle);
    }

    private static async Task<IResult> Handle(
        [FromBody] LoginUserRequest request,
        ApplicationDbContext context,
        IOptions<AuthConfiguration> jwtSettingsOptions,
        CancellationToken cancellationToken)
    {
        var user = await context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email, cancellationToken);

        if (user is null)
        {
            return Results.NotFound("User not found");
        }

        var token = GenerateJwtToken(user, jwtSettingsOptions.Value);
        return Results.Ok(new { Token = token });
    }
}
```

This method disables all query filters applied to a `User` entity. After a user is found, you need to add a "tenant-id" claim:

```csharp
private static string GenerateJwtToken(User user, AuthConfiguration auth)
{
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(auth.Key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new[]
    {
        new Claim(JwtRegisteredClaimNames.Sub, user.Email),
        new Claim("use-id", user.Id.ToString()),
        new Claim("tenant-id", user.TenantId?.ToString() ?? string.Empty)
    };

    var token = new JwtSecurityToken(
        issuer: auth.Issuer,
        audience: auth.Audience,
        claims: claims,
        expires: DateTime.Now.AddMinutes(30),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}
```

Now that the user is logged in, we can start calling other endpoints with a JWT token.

## Implementing API endpoints for a Multitenant Entity

Let's explore a `Create` and `Get By Id` endpoints for a book entity:

```csharp
public sealed record CreateBookRequest(string Title, int Year, Guid AuthorId);

public class CreateBookEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/books", Handle);
    }

    private static async Task<IResult> Handle(
        [FromBody] CreateBookRequest request,
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var author = await context.Authors.FindAsync([request.AuthorId], cancellationToken);
        if (author is null)
        {
            return Results.BadRequest("Author not found");
        }

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Year = request.Year,
            AuthorId = request.AuthorId,
            Author = author
        };

        context.Books.Add(book);
        await context.SaveChangesAsync(cancellationToken);

        var response = new BookResponse(book.Id, book.Title, book.Year, book.AuthorId);
        return Results.Created($"/api/books/{book.Id}", response);
    }
}
```
```csharp
public class GetBookByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/books/{id}", Handle);
    }

    private static async Task<IResult> Handle(
        [FromRoute] Guid id,
        ApplicationDbContext context,
        CancellationToken cancellationToken)
    {
        var book = await context.Books
            .Include(b => b.Author)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (book is null)
        {
            return Results.NotFound();
        }

        var response = new BookResponse(book.Id, book.Title, book.Year, book.AuthorId);
        return Results.Ok(response);
    }
}
```

As you may have expected, with code doesn't know anything about Tenants. And it is what we were striving for — to create a secure implementation that will now allow customers to interact with another customer's data. Without a need to worry about tenants in each database call.

When creating a book, we check if an Author exists in the database by a provided `AuthorId` in the request. And EF Core makes sure that we can't add a Book using an Author from another tenant. Amazing!

## Using Tenant Id Header

In some applications a user can have access to multiple tenants, for example, a "super-admin" user that can manage entities for different tenants. In such a case we need to send "X-TenantId" header in each request from the frontend (or another application) whenever we need to retrieve or modify data.

If you have such a case, you can modify your `TenantProvider` as follows:

```csharp
public class TenantProvider : ITenantProvider
{
    private readonly TenantInfo _tenantInfo;

    public TenantProvider(IHttpContextAccessor accessor)
    {
        var userIdValue = accessor.HttpContext?.User.FindFirstValue("user-id");

        Guid? userId = Guid.TryParse(userIdValue, out var guid) ? guid : null;
        Guid? headerTenantId = null;

        if (accessor.HttpContext?.Request.Headers.TryGetValue("X-TenantId", out var headerGuid) is true)
        {
            headerTenantId = Guid.Parse(headerGuid.ToString());
        }

        _tenantInfo = new TenantInfo(userId, headerTenantId);
    }

    public TenantInfo GetCurrentTenantInfo() => _tenantInfo;
}
```

In some applications, it is preferable to always use "X-TenantId" header instead of putting it inside a JWT token. This can also be a case when you use external auth providers that have no idea about tenants.

There are different approaches to solving this problem. I will show you one possible solution with a middleware that checks if a user has access to "X-TenantId". If a user doesn't have access to the requested tenant, a 403 Forbidden response is returned.

```csharp
public class TenantCheckerMiddleware : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        var tenantClaimValue = context.User.Claims.FirstOrDefault(x => x.Type.Equals("tenant-id"))?.Value;
        if (tenantClaimValue is null)
        {
            await next(context);
            return;
        }

        if (!context.Request.Headers.TryGetValue("X-TenantId", out var headerGuid))
        {
            await next(context);
            return;
        }

        if (tenantClaimValue.Contains(headerGuid.ToString(), StringComparison.Ordinal))
        {
            await next(context);
            return;
        }

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status403Forbidden,
            Title = "Bad Request",
            Detail = "X-TenantId header contains a tenant id that a user doesn't have access to"
        };

        context.Response.StatusCode = problemDetails.Status.Value;
        context.Response.ContentType = "application/problem+json";

        await context.Response.WriteAsJsonAsync(problemDetails);
    }
}
```

In this example, I made a basic check if "X-TenantId" header matches the "tenant-id" from the JWT token. In your case, you might want to check in a database, or even better in a cache.

## Conditional Global Query Filters

Unfortunately, EF Core doesn't support Global Query Filters with conditions, though it is a highly requested feature.

For example, you may want to implement the following query filter that will only be applied when a user has a limited access to tenants. While a "super-admin" user can view all the tenants.

```csharp
if (!TenantProvider.GetCurrentTenantInfo().IsSuperAdmin)
{
    builder.Entity<User>()
        .HasQueryFilter(x => x.TenantId.Equals(TenantProvider.GetCurrentTenantInfo().TenantId));
}
```

If you try to test this application, your filters won't trigger even for a regular user. All because DbContext `OnModelCreating` method is only called once per lifetime of the application — when the first DbContext instance is created.

If you absolutely need to use such Global Query Filters that are applied conditionally depending on each request, you can create a `DynamicModelCacheKeyFactory`:

```csharp
public class DynamicModelCacheKeyFactory : IModelCacheKeyFactory
{
    public object Create(DbContext context, bool designTime)
        => context is ApplicationDbContext dynamicContext
            ? (context.GetType(), dynamicContext.TenantProvider.GetCurrentTenantInfo(), designTime)
            : context.GetType();
    
    public object Create(DbContext context)
        => Create(context, false);
}
```

You need to replace a standard `IModelCacheKeyFactory` with a dynamic one when registering a DbContext:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>((provider, options) =>
{
    var interceptor = provider.GetRequiredService<AuditableInterceptor>();

    options.EnableSensitiveDataLogging()
        .UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__MyMigrationsHistory", "devtips_multitenancy");
        })
        .AddInterceptors(interceptor)
        .UseSnakeCaseNamingConvention();
        
    options.ReplaceService<IModelCacheKeyFactory, DynamicModelCacheKeyFactory>();
});
```

This `DynamicModelCacheKeyFactory` tells EF Core not to cache a created model (mapping with query filters) and call `OnModelCreating` method for every instance of DbContext. This ensures that conditional Global Query Filters work, but this approach **severely damages the performance** of each database call within a DbContext. So use this carefully and benchmark your requests.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-implement-multitenancy-in-asp-net-core-with-ef-core)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-implement-multitenancy-in-asp-net-core-with-ef-core&title=How%20to%20Implement%20Multitenancy%20in%20ASP.NET%20Core%20with%20EF%20Core)[X](https://twitter.com/intent/tweet?text=How%20to%20Implement%20Multitenancy%20in%20ASP.NET%20Core%20with%20EF%20Core&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-implement-multitenancy-in-asp-net-core-with-ef-core)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-implement-multitenancy-in-asp-net-core-with-ef-core)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.