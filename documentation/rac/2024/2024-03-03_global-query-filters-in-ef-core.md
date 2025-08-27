```yaml
---
title: Global Query Filters in EF Core
source: https://antondevtips.com/blog/global-query-filters-in-ef-core
date_published: 2024-04-10T11:00:57.933Z
date_captured: 2025-08-06T17:16:15.452Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [Entity Framework Core, ASP.NET Core, .NET, LINQ]
programming_languages: [C#, SQL]
tags: [ef-core, global-query-filters, data-access, soft-delete, multi-tenancy, dotnet, orm, database, linq, web-api]
key_concepts: [Global Query Filters, Soft Deletion, Multi-tenancy, Entity Framework Core, LINQ, Dependency Injection, Minimal APIs, Data Access Patterns]
code_examples: false
difficulty_level: intermediate
summary: |
  [Global Query Filters in Entity Framework Core are a powerful feature for managing data access patterns by automatically applying LINQ predicates to entity queries. This article demonstrates their utility in two key scenarios: implementing soft deletion, where entities are marked as deleted rather than permanently removed, and building multi-tenant applications, ensuring data isolation between different customers. It provides C# code examples for configuring filters in DbContext, including how to bypass them using `IgnoreQueryFilters`, and integrates them with ASP.NET Core Minimal APIs and dependency injection for a practical implementation. The feature simplifies data access code, enhances data integrity, and reduces the risk of overlooking critical filters.]
---
```

# Global Query Filters in EF Core

![Cover image for the article "Global Query Filters in EF Core" featuring a dark blue and purple abstract background with a white icon of angle brackets and the text "dev tips" and "Improve your coding skills".](https://antondevtips.com/media/covers/efcore/cover_efcore_gqf.png)

# Global Query Filters in EF Core

Apr 10, 2024

[Download source code](/source-code/global-query-filters-in-ef-core)

4 min read

## What are global query filters?

**Global query filters** in Entity Framework Core (EF Core) is a powerful feature that can be effectively used to manage data access patterns.

**Global query filters** are LINQ query predicates applied to EF Core entity models. These filters are automatically applied to all queries involving corresponding entities. This is especially useful in **multi-tenant** applications or scenarios requiring **soft deletion**.

## Query filters for a soft delete use case

Let's explore a use case where global query filters are particularly useful - **entity soft deletion**. In some applications entities can't be completely deleted from the database and should remain for statistics and history purposes, for example. Or to ensure that related data remains unchanged, i.e: referenced by foreign keys. A solution for this use case is **soft deletion**.

Soft deletion is implemented by adding an `is_deleted` column to the database table for required entities. Whenever an entity is considered deleted - this column is set to `true`. In most of the application's database queries "deleted" entities should be ignored in read operations and not visible to the end user.

Let's explore an example for the following entities:

```csharp
public class Author
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Country { get; set; }
    public required List<Book> Books { get; set; } = [];
}

public class Book
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required int Year { get; set; }
    public required bool IsDeleted { get; set; }
    public required Guid TenantId { get; set; }
    public required Author Author { get; set; }
}
```

We need to create and setup our DbContext with a global query filter:

```csharp
public class ApplicationDbContext : DbContext
{
    public DbSet<Author> Authors { get; set; } = default!;
    
    public DbSet<Book> Books { get; set; } = default!;
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>()
            .HasQueryFilter(x => !x.IsDeleted);
        
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.Entity<Author>(entity =>
        {
            entity.ToTable("authors");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Name);
            
            entity.Property(x => x.Id).IsRequired();
            entity.Property(x => x.Name).IsRequired();
            entity.Property(x => x.Country).IsRequired();
            
            entity.HasMany(x => x.Books)
                .WithOne(x => x.Author);
        });
        
        modelBuilder.Entity<Book>(entity =>
        {
            entity.ToTable("books");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Title);
            
            entity.Property(x => x.Id).IsRequired();
            entity.Property(x => x.Title).IsRequired();
            entity.Property(x => x.Year).IsRequired();
            entity.Property(x => x.IsDeleted).IsRequired();

            entity.HasOne(x => x.Author)
                .WithMany(x => x.Books);
        });
    }
}
```

In this code we're applying a query filter to the `Book` entity on the `IsDeleted` property:

```csharp
modelBuilder.Entity<Book>()
    .HasQueryFilter(x => !x.IsDeleted);
```

Here we are filtering out all softly deleted books from the result query. When querying books from **DbContext** this query filter is applied automatically. Let's have a look on the following minimal API endpoint:

```csharp
app.MapGet("/api/books", async (ApplicationDbContext dbContext) =>
{
    var nonDeletedBooks = await dbContext.Books.ToListAsync();
    return Results.Ok(nonDeletedBooks);
});
```

Every time we query books, we only get those that are not deleted, thus we don't need to use a LINQ **Where** statement in all **DbContext** queries.

In some cases, however, we might need to access all entities and ignore the query filter. EF Core has a special method called `IgnoreQueryFilters` for such a case:

```csharp
app.MapGet("/api/all-books", async (ApplicationDbContext dbContext) =>
{
    var allBooks = await dbContext.Books
        .IgnoreQueryFilters()
        .Where(x => x.IsDeleted)
        .ToListAsync();
    
    return Results.Ok(allBooks);
});
```

That way all the books are retrieved from the database and query filter on the `Book` entity is completely ignored.

## Query filters for a multi-tenant application

Another useful use case for global query filters is **multi-tenancy**. A multi-tenant application is an application that shares a software for different customers. All the data stored for customers should not be visible to other customers.

Let's explore the simplest implementation of multi-tenancy by storing all the data in the same database and table.

First, we need to add a `TenantId` property to the `Books` entity:

```csharp
public class Book
{
    // Other properties ...
    
    public required Guid TenantId { get; set; }
}
```

Second, we need to update the DbContext:

```csharp
public class ApplicationDbContext : DbContext
{
    private readonly Guid? _currentTenantId;
    
    public DbSet<Author> Authors { get; set; } = default!;
    
    public DbSet<Book> Books { get; set; } = default!;
    
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ITenantService tenantService) : base(options)
    {
        _currentTenantId = tenantService.GetCurrentTenantId();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>()
            .HasQueryFilter(x => !x.IsDeleted && x.TenantId == _currentTenantId);
        
        base.OnModelCreating(modelBuilder);
        
        // Rest of the code remains unchanged
    }
}
```

In this code we've updated the query filter and added a `x.TenantId == _currentTenantId` statement. As we're creating DbContext per request - we can inject the current **tenant id** from the request (identifier of the customer accessing data in our application).

Here's a simple tenant service implementation that retrieves a **tenant id** from HTTP request headers:

```csharp
public interface ITenantService
{
    Guid? GetCurrentTenantId();
}

public class TenantService : ITenantService
{
    private readonly Guid? _currentTenantId;
    
    public TenantService(IHttpContextAccessor accessor)
    {
        var headers = accessor.HttpContext?.Request.Headers;

        _currentTenantId = headers.TryGetValue("Tenant-Id", out var value) is true
            ? Guid.Parse(value.ToString())
            : null;
    }

    public Guid? GetCurrentTenantId() => _currentTenantId;
}
```

Now let's create a corresponding minimal API endpoint:

```csharp
app.MapGet("/api/tenant-books", async (ApplicationDbContext dbContext) =>
{
    var tenantBooks = await dbContext.Books.ToListAsync();
    return Results.Ok(tenantBooks);
});
```

On every read query a tenant id **global query filter** is applied ensuring the data integrity. As a result, when calling this endpoint, each customer can only retrieve their own books.

## Summary

Global query filters in EF Core is a powerful feature that enforces data access rules consistently across the application. They are particularly useful in **multi-tenant** architectures and scenarios like **soft deletion**, ensuring that filter queries are automatically applied during all read operations. By applying these filters to EF Core entity models, you can significantly simplify your data access code, ensure data integrity and reduce the risk of forgetting to apply important filters to the read operations.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/global-query-filters-in-ef-core)