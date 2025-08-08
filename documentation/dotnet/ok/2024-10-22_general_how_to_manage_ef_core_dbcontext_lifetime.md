```yaml
---
title: How To Manage EF Core DbContext Lifetime
source: https://antondevtips.com/blog/how-to-manage-ef-core-dbcontext-lifetime
date_published: 2024-10-22T10:55:15.432Z
date_captured: 2025-08-06T17:29:43.896Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [EF Core, ASP.NET Core, PostgreSQL, .NET, Entity Framework Extensions]
programming_languages: [C#, SQL]
tags: [ef-core, database, dotnet, data-access, performance, dependency-injection, lifetime-management, pooling, web-api, background-services]
key_concepts: [DbContext lifetime, Dependency Injection, DbContextFactory, DbContext pooling, Service lifetimes, Change tracking, Query execution, Thread safety]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to managing the DbContext lifetime in EF Core applications for improved performance and stability. It details the default Scoped lifetime of DbContext within the DI container and demonstrates scenarios where more control is needed. The post then introduces `IDbContextFactory<TContext>` for on-demand DbContext creation, suitable for background services and multi-threaded environments. Furthermore, it explains DbContext pooling and pooled DbContextFactory as advanced techniques to optimize performance by reusing DbContext instances. The author emphasizes best practices, including proper disposal and avoiding thread safety issues.]
---
```

# How To Manage EF Core DbContext Lifetime

![Cover image for the article "How To Manage EF Core DbContext Lifetime", featuring a dark background with purple abstract shapes, a white icon with "</>" and "dev tips" text on the left, and the article title in large white letters on the right.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fefcore%2Fcover_ef_dbcontext_options.png&w=3840&q=100)

# How To Manage EF Core DbContext Lifetime

Oct 22, 2024

[Download source code](/source-code/how-to-manage-ef-core-dbcontext-lifetime)

3 min read

### Newsletter Sponsors

[EF Core is too slow?](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website) Discover how you can easily insert 14x faster (reducing saving time by 94%).  
Boost your performance with our method integrated within EF Core: Bulk Insert, update, delete, and merge.  
Join 5,000+ satisfied customers who have trusted our library since 2014.

[Learn more](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website)

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Proper management of the DbContext lifecycle is crucial for application performance and stability.

While in many cases, registering **DbContext** with a scoped lifetime is simple enough, there are scenarios where more control is needed. EF Core offers more flexibility on DbContext creation.

In this blog post, I will show how to use DbContext, DbContextFactory and their pooled versions. These features allow for greater flexibility and efficiency, especially in applications that require high performance or have specific threading models.

[](#using-dbcontext)

## Using DbContext

`DbContext` is the heart of EF Core, it establishes connection with a database and allows performing CRUD operations.

**The `DbContext` class is responsible for:**

*   Managing database connections: opens and closes connections to the database as needed.
*   Change tracking: keeps track of changes made to entities so they can be persisted to the database.
*   Query execution: translates LINQ queries to SQL and executes them against the database.

When working with DbContext, you should be aware of the **following nuances:**

*   Not thread-safe: should not be shared across multiple threads simultaneously.
*   Lightweight: designed to be instantiated and disposed frequently.
*   Stateful: tracks entity states for change tracking and identity resolution.

Let's explore how you can register DbContext in the DI container:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.EnableSensitiveDataLogging().UseNpgsql(connectionString);
});

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }
}
```

`DbContext` is registered as **Scoped** in DI container. It has the lifetime of current scope which equals to the current request duration:

```csharp
app.MapPost("/api/authors", async (
    [FromBody] CreateAuthorRequest request,
    ApplicationDbContext context,
    CancellationToken cancellationToken) =>
{
    var author = request.MapToEntity();

    context.Authors.Add(author);
    await context.SaveChangesAsync(cancellationToken);

    var response = author.MapToResponse();
    return Results.Created($"/api/authors/{author.Id}", response);
});
```

You can also manually create a scope and resolve the DbContext:

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}
```

`DbContext` being a Scoped dependency is pretty flexible, you can inject the same instance of `DbContext` into the Controller/Minimal API endpoint, service, repository. But sometimes, you need more control on when `DbContext` is created and disposed. Such control you can get with `DbContextFactory`.

[](#using-dbcontextfactory)

## Using DbContextFactory

In some use cases, such as background services, multi-threaded applications, or factories that create services, you might need full control to create and dispose `DbContext` instance.

The `IDbContextFactory<TContext>` is a service provided by EF Core that allows creating of DbContext instances on demand. It ensures that each instance is configured correctly and can be used safely without being tied directly to the DI container's service lifetime.

You can register `IDbContextFactory<TContext>` in the following way, similar to `DbContext`:

```csharp
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.EnableSensitiveDataLogging().UseNpgsql(connectionString);
});
```

`IDbContextFactory` is registered as **Singleton** in the DI container.

You can the `CreateDbContext` or `CreateDbContextAsync` from the `IDbContextFactory` to create a DbContext:

```csharp
public class HostedService : IHostedService
{
    private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

    public HostedService(IDbContextFactory<ApplicationDbContext> contextFactory)
    {
        _contextFactory = contextFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
        var books = await context.Books.ToListAsync(cancellationToken: cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
```

`DbContext` is disposed when the using block ends.

You should use `IDbContextFactory<TContext>` with caution - make sure that you won't create too many database connections.

You can register both `DbContext` and `IDbContextFactory` at the same time. You need to do a small tweak for this and set the `optionsLifetime` to `Singleton` as the `IDbContextFactory` is registered as Singleton:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.EnableSensitiveDataLogging().UseNpgsql(connectionString);
}, optionsLifetime: ServiceLifetime.Singleton);

builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.EnableSensitiveDataLogging().UseNpgsql(connectionString);
});
```

[](#using-dbcontext-pooling)

## Using DbContext Pooling

DbContext **pooling** is a feature introduced in EF Core that allows for reusing DbContext instances, reducing the overhead of creating and disposing of contexts frequently.

DbContext pooling maintains a pool of pre-configured DbContext instances. When you request a DbContext, it provides one from the pool. When you're done, it resets the state and returns the instance to the pool for reuse.

This feature is crucial for high performance scenarios or when you need to create a lot of database connections.

Registration of Pooled DbContext is straightforward:

```csharp
builder.Services.AddDbContextPool<ApplicationDbContext>(options =>
{
    options.EnableSensitiveDataLogging().UseNpgsql(connectionString);
});
```

In your classes you inject a regular `DbContext` without knowing that it is being pooled.

[](#using-dbcontextfactory-pooling)

## Using DbContextFactory Pooling

You can combine `DbContextFactory` and DbContext pooling to create a **pooled** DbContextFactory. This allows you to create DbContext instances on demand, which are also pooled for performance.

```csharp
builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(options =>
{
    options.EnableSensitiveDataLogging().UseNpgsql(connectionString);
});
```

The API for using pooled factory is the same:

```csharp
await using var context = await _contextFactory.CreateDbContextAsync(cancellationToken);
var books = await context.Books.ToListAsync(cancellationToken: cancellationToken);
```

Each `CreateDbContextAsync` call retrieves a DbContext from the pool. After disposing, the DbContext is returned to the pool.

In your classes you inject a regular `DbContextFactory` without knowing that it is being pooled.

[](#summary)

## Summary

Managing the DbContext lifetime is essential for building efficient EF Core applications. By leveraging DbContextFactory and DbContext pooling, you can gain greater control over context creation and optimize performance.

**Key Takeaways:**

*   **DbContextFactory:** use when you need to create DbContext instances on demand, especially in multi-threaded or background tasks.
*   **DbContext Pooling:** use to reduce the overhead of creating and disposing of DbContext instances frequently.
*   **Pooled DbContextFactory:** combine both features to create pooled DbContext instances on demand.
*   **Always Dispose DbContext Instances:** use using statements or ensure that contexts are disposed or returned to the pool.
*   **Avoid Thread Safety Issues:** do not share DbContext instances across threads.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-manage-ef-core-dbcontext-lifetime)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-manage-ef-core-dbcontext-lifetime&title=How%20To%20Manage%20EF%20Core%20DbContext%20Lifetime)[X](https://twitter.com/intent/tweet?text=How%20To%20Manage%20EF%20Core%20DbContext%20Lifetime&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-manage-ef-core-dbcontext-lifetime)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-manage-ef-core-dbcontext-lifetime)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.