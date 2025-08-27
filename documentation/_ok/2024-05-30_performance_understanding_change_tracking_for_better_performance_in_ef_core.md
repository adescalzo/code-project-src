```yaml
---
title: Understanding Change Tracking for Better Performance in EF Core
source: https://antondevtips.com/blog/understanding-change-tracking-for-better-performance-in-ef-core
date_published: 2024-05-30T11:00:57.787Z
date_captured: 2025-08-06T17:21:13.516Z
domain: antondevtips.com
author: Anton Martyniuk
category: performance
technologies: [EF Core, .NET, ASP.NET Core, SQLite, Entity Framework Extensions]
programming_languages: [C#, SQL]
tags: [ef-core, change-tracking, performance, database, data-access, dotnet, orm, entity-states, interceptors, auditing]
key_concepts: [Change Tracking, Entity States, Performance Optimization, Batch Operations, DbContext, EF Core Interceptors, Time Auditing, Data Persistence]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to EF Core's Change Tracker, explaining its fundamental role in monitoring entity states (Added, Modified, Deleted, Detached, Unchanged) and facilitating database updates. It demonstrates how to perform various operations, including adding, updating, deleting, and attaching entities, both individually and in batches. The post also covers performance optimization techniques like `AsNoTracking` for read-only queries. A practical real-world example illustrates how to automate `CreatedAtUtc` and `UpdatedAtUtc` properties using Change Tracker within `DbContext` overrides and EF Core Interceptors, showcasing a clean approach to implementing auditing logic.]
---
```

# Understanding Change Tracking for Better Performance in EF Core

![A banner image with a dark background and abstract purple shapes. On the left, a white square icon with `</>` (code tags) inside, and the text "dev tips" below it. On the right, large white text reads "UNDERSTANDING CHANGE TRACKING FOR BETTER PERFORMANCE IN EF CORE".](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fefcore%2Fcover_ef_change_tracking.png&w=3840&q=100)

# Understanding Change Tracking for Better Performance in EF Core

May 30, 2024

[Download source code](/source-code/understanding-change-tracking-for-better-performance-in-ef-core)

8 min read

### Newsletter Sponsors

[EF Core is too slow?](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website) Discover how you can easily insert 14x faster (reducing saving time by 94%).  
Boost your performance with our method integrated within EF Core: Bulk Insert, update, delete, and merge.  
Join 5,000+ satisfied customers who have trusted our library since 2014.

[Learn more](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website)

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

**Change Tracker** is the heart of EF Core, that keeps an eye on entities that are added, updated and deleted. In today's post you will learn how Change Tracker works, how entities are tracked, and how to attach existing entities to the Change Tracker. You will receive guidelines on how to improve your application's performance with tracking techniques.

In the end, we will explore how EF Core Change Tracker can significantly improve our code in the read-world scenario.

[](#what-is-change-tracker-in-ef-core)

## What is Change Tracker in EF Core

The **Change Tracker** is a key part of EF Core responsible for keeping track of entity instances and their states. It monitors changes to these instances and ensures the database is updated accordingly. This tracking mechanism is essential for EF Core to know which entities must be inserted, updated, or deleted in the database.

When you query the database, EF Core automatically starts tracking the returned entities.

```csharp
using (var dbContext = new ApplicationDbContext())
{
    var users = await dbContext.Users.ToListAsync();
}
```

After querying users from the database, all entities are automatically added to the Change Tracker. When updating the users - change tracker will compare the `users` collection with its inner collection of `User` entities that were retrieved from the database. EF Core will use the comparison result to decide what SQL commands to generate to update entities in the database.

```csharp
using (var dbContext = new ApplicationDbContext())
{
    var users = await dbContext.Users.ToListAsync();
    users[0].Email = "test@mail.com";

    await dbContext.SaveChangesAsync();
}
```

In this example, we are updating the first user's email. After calling `dbContext.SaveChangesAsync()` EF Core compares `users` collection with the one saved in the Change Tracker. After comparing, EF Core finds out that `users` collection was updated and the **update** SQL query is sent to the database:

```sql
Executed DbCommand (0ms) [Parameters=[@p1='****', @p0='test@mail.com' (Nullable = false) (Size = 13)], CommandType='Text', CommandTimeout='30']
      UPDATE "users" SET "email" = @p0
      WHERE "id" = @p1
      RETURNING 1;
```

To add and delete entities you should call the `Add` and `Remove` methods:

```csharp
using (var dbContext = new ApplicationDbContext())
{
    var users = await dbContext.Users.ToListAsync();
    
    dbContext.Users.Remove(users[1]);
    
    dbContext.Users.Add(new User
    {
        Id = Guid.NewGuid(),
        Email = "one@mail.com"
    });

    await dbContext.SaveChangesAsync();
}
```

Change Tracker will detect that a second user is deleted and a new user is added. As a result, the following SQL commands will be sent to the database to delete and create a user:

```sql
Executed DbCommand (0ms) [Parameters=[@p0='***'], CommandType='Text', CommandTimeout='30']
      DELETE FROM "users"
      WHERE "id" = @p0
      RETURNING 1;
      
Executed DbCommand (0ms) [Parameters=[@p0='***', @p1='one@mail.com' (Nullable = false) (Size = 12)], CommandType='Text', CommandTimeout='30']
      INSERT INTO "users" ("id", "email")
      VALUES (@p0, @p1);
```

[](#change-tracker-and-child-entities)

## Change Tracker and Child Entities

Change Tracker in EF Core also tracks child entities that are loaded together with other entities. Let's explore the following entities:

```csharp
public class Book
{
    public required Guid Id { get; set; }
    
    public required string Title { get; set; }
    
    public required int Year { get; set; }
    
    public Guid AuthorId { get; set; }

    public Author Author { get; set; } = null!;
}

public class Author
{
    public required Guid Id { get; set; }
    
    public required string Name { get; set; }

    public List<Book> Books { get; set; } = [];
}
```

A `Book` is mapped as one-to-many to the `Author`.

When executing the following code and updating the first book's author name:

```csharp
using (var dbContext = new ApplicationDbContext())
{
    var books = await dbContext.Books
        .Include(x => x.Author)
        .ToListAsync();
    
    books[0].Author.Name = "Jack Sparrow";

    await dbContext.SaveChangesAsync();
}
```

EF Core generates an update request to the database:

```sql
Executed DbCommand (0ms) [Parameters=[@p1='***', @p0='Jack Sparrow' (Nullable = false) (Size = 12)], CommandType='Text', CommandTimeout='30']
      UPDATE "authors" SET "name" = @p0
      WHERE "id" = @p1
      RETURNING 1;
```

Now let's try to add a new book to the first author:

```csharp
using (var dbContext = new ApplicationDbContext())
{
    var authors = await dbContext.Authors
        .Include(x => x.Books)
        .ToListAsync();
    
    var newBook = new Book
    {
        Id = Guid.NewGuid(),
        Title = "Asp.Net Core In Action",
        Year = 2024
    };

    authors[0].Books.Add(newBook);
    
    dbContext.Entry(newBook).State = EntityState.Added;

    await dbContext.SaveChangesAsync();
}
```

In this case, you need to manually notify Change Tracker that book was added to the author:

```csharp
dbContext.Entry(newBook).State = EntityState.Added;
```

As a result, an insert query with a foreign key to `Author` will be sent to the database:

```sql
Executed DbCommand (11ms) [Parameters=[@p0='fba984cd-a7b8-4eee-998b-165db95068a5', @p1='1072efd7-a71f-40a5-a939-5e68b7e34e0c', @p2='Asp.Net Core In Action' (Nullable = false) (Size = 22), @p3='2024'], CommandType='Text', CommandTimeout='30']
      INSERT INTO "books" ("id", "author_id", "title", "year")
      VALUES (@p0, @p1, @p2, @p3);
```

[](#how-entities-are-tracked-in-ef-core)

## How Entities are Tracked In EF Core

Entities in EF Core are tracked based on their state, which can be one of the following:

*   **Added** - the entity is new and will be inserted into the database.
*   **Modified** - the entity has been modified and will be updated in the database
*   **Deleted** - the entity has been marked for deletion
*   **Detached** - the entity should not be tracked and will be removed from the change tracker
*   **Unchanged** - the entity has not been modified since it was loaded

You can check the state of an entity using the `Entry` property of the DbContext:

```csharp
using (var dbContext = new ApplicationDbContext())
{
    var book = dbContext.Books.First();
    var entry = dbContext.Entry(book);
    var state = entry.State; // EntityState.Unchanged
}
```

[](#attaching-existing-entities-to-the-change-tracker)

## Attaching Existing Entities to the Change Tracker

As you've already seen, sometimes, you might need to attach an existing entity to the Change Tracker. This is common in scenarios where entities are retrieved from a different context or from outside the database (e.g., from an API).

To attach an entity, you can use the `Attach` method so the Change Tracker will start tracking this entity. This method marks the entity as `Unchanged` by default.

You need to specify whether this entity should be either modified or deleted in the database:

```csharp
using (var dbContext = new ApplicationDbContext())
{
    var book = new Book
    {
        Id = Guid.NewGuid(),
        Title = "Asp.Net Core In Action",
        Year = 2024
    };
    
    dbContext.Books.Attach(book);
    dbContext.Entry(book).State = EntityState.Modified;
    
    dbContext.Books.Attach(book);
    dbContext.Entry(book).State = EntityState.Deleted;
}
```

[](#batch-tracking-operations-in-ef-core)

## Batch Tracking Operations in EF Core

EF Core provides range operations to perform batch operations on multiple entities. These methods can simplify code and improve performance.

[](#addrange)

### AddRange

Adds a collection of new entities to the context:

```csharp
using (var dbContext = new ApplicationDbContext())
{
    var author = new Author
    {
        Id = Guid.NewGuid(),
        Name = "Andrew Lock"
    };
    
    var books = new List<Book>
    {
        new()
        {
            Id = Guid.NewGuid(),
            Title = "Asp.Net Core In Action 2.0",
            Year = 2020,
            Author = author
        },
        new()
        {
            Id = Guid.NewGuid(),
            Title = "Asp.Net Core In Action 3.0",
            Year = 2024,
            Author = author
        }
    };
    
    dbContext.Books.AddRange(books);
    await dbContext.SaveChangesAsync();
}
```

[](#updaterange)

### UpdateRange

Updates a collection of entities in the context:

```csharp
using (var dbContext = new ApplicationDbContext())
{
    var booksToUpdate = await dbContext.Books
        .Where(x => x.Year >= 2020)
        .ToListAsync();
    
    booksToUpdate.ForEach(b => b.Title += "-updated");
    
    dbContext.Books.UpdateRange(booksToUpdate);
    await dbContext.SaveChangesAsync();
}
```

[](#removerange)

### RemoveRange

Removes a collection of entities from the context:

```csharp
using (var dbContext = new ApplicationDbContext())
{
    var blogsToDelete = await dbContext.Books
        .Where(x => x.Year < 2020)
        .ToListAsync();
    
    dbContext.Books.RemoveRange(blogsToDelete);
    await dbContext.SaveChangesAsync();
}
```

[](#attachrange)

### AttachRange

Attaches a collection of existing entities to the context:

```csharp
using (var dbContext = new ApplicationDbContext())
{
    var books = new List<Book>
    {
        // ...
    };
    
    dbContext.Books.AttachRange(books);
    
    foreach (var book in books)
    {
        dbContext.Entry(book).State = EntityState.Modified;
    }
}
```

[](#how-to-disable-change-tracker)

## How to Disable Change Tracker

When you read entities from the database, and you don't need to update them, you can inform EF Core to not track these entities in the Change Tracker. It is especially useful when you are retrieving a lot of records from the database and don't want to waste memory for tracking these entities as they won't be modified.

The `AsNoTracking` method is used to query entities without tracking them. This can improve performance for read-only operations, as EF Core skips the overhead of tracking changes:

```csharp
using (var dbContext = new ApplicationDbContext())
{
    var books = await dbContext.Books
        .Include(x => x.Author)
        .AsNoTracking()
        .ToListAsync();
}
```

It's a small performance tip for optimizing read-only queries in EF Core and you need to know it.

[](#how-to-access-tracking-entities-in-ef-core)

## How To Access Tracking Entities in EF Core

EF Core allows you to access and manipulate tracked entities in the Change Tracker of the current DbContext. You can retrieve all tracked entities using the `Entries` method:

```csharp
using (var dbContext = new ApplicationDbContext())
{
    var books = await dbContext.Books
        .Include(x => x.Author)
        .ToListAsync();
    
    var trackedEntities = dbContext.ChangeTracker.Entries();
    foreach (var entry in trackedEntities)
    {
        Console.WriteLine($"Entity: {entry.Entity}, State: {entry.State}");
    }
}
```

You can also filter entities by their state:

```csharp
using (var dbContext = new ApplicationDbContext())
{
    var books = await dbContext.Books
        .Include(x => x.Author)
        .ToListAsync();
    
    books[0].Author.Name = "Jack Sparrow";
    
    var modifiedEntities = dbContext.ChangeTracker.Entries()
        .Where(e => e.State == EntityState.Modified);
    
    foreach (var entry in modifiedEntities)
    {
        Console.WriteLine($"Modified Entity: {entry.Entity}");
    }
}
```

[](#a-realworld-example-of-using-change-tracker)

## A Real-World Example of Using Change Tracker

Let's explore a real world example on how using a Change Tracker can significantly simplify our code. Imagine that you have entities that have `CreatedAtUtc` and `UpdatedAtUtc` properties. These properties are used for time audit.

`CreatedAtUtc` should be assigned with current UTC time when a new entity is added to the database.

`UpdatedAtUtc` should be assigned with current UTC time whenever an existing entity is updated in the database.

Let's explore the most basic implementation for a `User` entity:

```csharp
public class User
{
    public Guid Id { get; set; }
    
    public required string Email { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    
    public DateTime? UpdatedAtUtc { get; set; }
}
```

When creating a new user or updating an existing one, you need to manually specify these values:

```csharp
using (var dbContext = new ApplicationDbContext())
{
    var user = new User
    {
        Id = Guid.NewGuid(),
        Email = "test@mail.com",
        CreatedAtUtc = DateTime.UtcNow
    };

    dbContext.Users.Add(user);
    await dbContext.SaveChangesAsync();

    user.Email = "another@mail.com";
    user.UpdatedAtUtc = DateTime.UtcNow;

    await dbContext.SaveChangesAsync();
}
```

It might seem that this is not a big deal, but imagine you have a more complex application where you can update not only a user's email, but his password, personal data and permissions. And you can have a lot of entities that should have `CreatedAtUtc` and `UpdatedAtUtc` properties.

Using manual approach will clutter your code, you will have code duplications here and there. Moreover, you can forget to set these properties and introduce a bug in your code.

What if I tell you that you can use Change Tracker in EF Core to set these properties automatically in one place for all entities that should have time audit?

First, let's introduce an interface:

```csharp
public interface ITimeAuditableEntity
{
    DateTime CreatedAtUtc { get; set; }
    
    DateTime? UpdatedAtUtc { get; set; }
}
```

All entities that need time audit should inherit from this interface:

```csharp
public class Book : ITimeAuditableEntity
{
    // Other properties
    
    public DateTime CreatedAtUtc { get; set; }
    
    public DateTime? UpdatedAtUtc { get; set; }
}

public class Author : ITimeAuditableEntity
{
    // Other properties
    
    public DateTime CreatedAtUtc { get; set; }
    
    public DateTime? UpdatedAtUtc { get; set; }
}
```

Now in the DbContext you can override the `SaveChangesAsync` method to automatically set the `CreatedAtUtc` and `UpdatedAtUtc` properties:

```csharp
public class ApplicationDbContext : DbContext
{
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new CancellationToken())
    {
        var entries = ChangeTracker.Entries<ITimeAuditableEntity>();
    
        foreach (var entry in entries)
        {
            if (entry.State is EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
            }
            else if (entry.State is EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }
    
        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

By using `ChangeTracker.Entries<ITimeAuditableEntity>();` you can receive filtered tracked entities. After that, `CreatedAtUtc` and `UpdatedAtUtc` properties are set for entities that are added and updated. Finally, we are calling `base.SaveChangesAsync` method to save changes to the database.

If you have multiple DbContexts in your application, you can use an EF Core **Interceptor** to achieve the same goal. This way you won't need to duplicate the code across all DbContexts.

Here is how to create such an **Interceptor**:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

public class TimeAuditableInterceptor : SaveChangesInterceptor
{
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        var context = eventData.Context!;
        var entries = context.ChangeTracker.Entries<ITimeAuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
            }
            else if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
            }
        }

        return await base.SavingChangesAsync(eventData, result, cancellationToken);
    }
}
```

And register the interceptor in the DbContext:

```csharp
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
{
    options.EnableSensitiveDataLogging().UseSqlite(connectionString);
    options.AddInterceptors(new TimeAuditableInterceptor());
});
```

You can register this interceptor for multiple DbContexts and reuse the single code base for performing time audit for any number of entities.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/understanding-change-tracking-for-better-performance-in-ef-core)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Funderstanding-change-tracking-for-better-performance-in-ef-core&title=Understanding%20Change%20Tracking%20for%20Better%20Performance%20in%20EF%20Core)[X](https://twitter.com/intent/tweet?text=Understanding%20Change%20Tracking%20for%2FBetter%20Performance%20in%20EF%20Core&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Funderstanding-change-tracking-for-better-performance-in-ef-core)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Funderstanding-change-tracking-for-better-performance-in-ef-core)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.