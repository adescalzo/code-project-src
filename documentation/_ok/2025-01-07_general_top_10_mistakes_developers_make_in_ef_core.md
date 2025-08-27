```yaml
---
title: Top 10 Mistakes Developers Make in EF Core
source: https://antondevtips.com/blog/top-10-mistakes-developers-make-in-ef-core
date_published: 2025-01-07T08:55:17.804Z
date_captured: 2025-08-06T17:34:54.983Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [EF Core, .NET, Fluent API, LINQ, IDbContextFactory]
programming_languages: [C#, SQL]
tags: [ef-core, database, performance, data-access, dotnet, optimization, best-practices, orm, concurrency, migrations]
key_concepts: [database-indexing, data-projection, pagination, change-tracking, eager-loading, database-transactions, concurrency-control, database-migrations, asynchronous-programming, connection-pooling]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article identifies the top 10 common mistakes developers make when using EF Core, offering practical solutions to improve application performance, stability, and scalability. It covers crucial topics such as optimizing queries with indexing and projections, efficient data retrieval using pagination and `AsNoTracking`, and wise eager loading strategies. The article also emphasizes the importance of database transactions for batch operations, robust concurrency handling, proper `DbContext` disposal, and leveraging asynchronous methods. Additionally, it provides bonus tips on avoiding `SaveChanges` in loops, configuring relationships, database normalization, and connection pooling. By addressing these pitfalls, developers can build more efficient and reliable .NET applications.]
---
```

# Top 10 Mistakes Developers Make in EF Core

![Cover image for an article titled "Top 10 Mistakes Developers Make in EF Core", featuring a white code tag icon and "dev tips" text on a dark blue background with abstract purple shapes.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fefcore%2Fcover_ef_top_mistakes.png&w=3840&q=100)

# Top 10 Mistakes Developers Make in EF Core

Jan 7, 2025

7 min read

### Newsletter Sponsors

[EF Core is too slow?](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website) Discover how you can easily insert 14x faster (reducing saving time by 94%).  
Boost your performance with our method integrated within EF Core: Bulk Insert, update, delete, and merge.  
Join 5,000+ satisfied customers who have trusted our library since 2014.

[Learn more](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website)

[Master The Clean Architecture](https://www.courses.milanjovanovic.tech/a/aff_q2snzxhl/external?affcode=1486372_j2vpyytw). This comprehensive course will teach you how to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture. Join 3,600+ students to accelerate your growth as a software architect.

[Master The Modular Monolith Architecture](https://www.courses.milanjovanovic.tech/a/aff_l825rpg0/external?affcode=1486372_j2vpyytw). This in-depth course will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario. Join 1,600+ students to accelerate your growth as a software architect.

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

EF Core simplifies database access and management in .NET applications a lot. While EF Core makes database operations easier, there are common mistakes developers frequently make, even senior devs.

These mistakes can lead to performance and maintainability issues and bugs. Today I want to show you the top 10 mistakes developers make in EF Core and how to avoid them. Ensuring you get the best results when using EF Core.

## 1. Not Using Indexes

**Mistake:** Without proper indexing, EF Core queries can trigger unnecessary table scans, impacting the responsiveness of your application. Indexed queries are fast and table scans can result in a slower query performance.

**Solution:** Identify columns frequently used in `WHERE` clauses or for sorting and create indexes for them. In EF Core, you can define these indexes using Fluent API configurations in your DbContext configuration.

```csharp
public class Book
{
    public long Id { get; set; }
    public string Title { get; set; }
    public string AuthorName { get; set; }
    public int PublishedYear { get; set; }
    public int NumberOfPages { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
}

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("books");
        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Title);
    }
}

// Fast query that uses index on "Title" column
var book = await context.Books
    .FirstOrDefaultAsync(b => b.Title == title, cancellationToken);
```

## 2. Not Using Projections

**Mistake:** Fetching the entire entity (including all columns and related data) when you only need a subset of fields - leads to unnecessary data transfer and increased memory usage.

```csharp
// Fetching ALL columns
var book = await context.Books
    .Include(b => b.Author)
    .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
```

**Solution:** Retrieve only the necessary fields rather than the entire entity using Projection with a `Select` method in LINQ. This reduces the overhead and makes your queries more efficient.

```csharp
// Fetching only needed columns
var book = await context.Books
    .Where(b => b.Id == id)
    .Select(b => new BooksPreviewResponse
    {
        Title = b.Title,
        Author = b.Author.Name,
        Year = b.Year
    })
    .FirstOrDefaultAsync(cancellationToken);
```

## 3. Overfetching Data

**Mistake:** Retrieving entire tables or large data sets without pagination can overload both memory and network, especially as the number of records grows.

```csharp
// Selecting all books (entire database)
var allBooks = await context.Books
    .Include(b => b.Author)
    .ToListAsync();
```

**Solution:** Use paging to select a fixed number of records, preventing from loading too many records into memory at once. You can use traditional **Offset-based** pagination by using `Skip` and `Take` methods.

```csharp
// Use paging to select fixed number of records
int pageSize = 50;
int pageNumber = 1;

var books = context.Books
    .AsNoTracking()
    .OrderBy(p => p.Title)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToList();
```

You can also use **Cursor-based** pagination. It involves using a "cursor" (often the value of a specific field or a special token) to determine where the next page of results should start. This approach can be more efficient and consistent than offset-based pagination as it prevents a database from iterating through rows that are skipped when using Offset-Based pagination.

```csharp
var pageSize = 50;
var lastId = 10024; // Obtained from the previous query

// Fetch next set of results starting after the last fetched Id
var books = await context.Books
    .AsNoTracking()
    .OrderBy(b => b.Title)
    .Where(b => b.Id > lastId)
    .Take(pageSize)
    .ToListAsync();
```

Also, consider the following when choosing a pagination type:

*   when you need to go to the next and previous page only - you can use Cursor-Based Pagination
*   when you need to access any page by its number - you can only use Offset-Based Pagination

## 4. Not Using AsNoTracking

**Mistake:** By default, EF Core tracks all entities to detect changes. For read-only operations, this tracking is unnecessary and adds performance overhead.

```csharp
// Selecting books and loading them to the Change Tracker
var authors = await context.Books
    .Where(b => b.Year >= 2023)
    .ToListAsync();
```

**Solution:** Use `AsNoTracking` on read-only queries to disable change tracking, improving performance and reducing overhead.

```csharp
// Using AsNoTracking to prevent loading entities to
// EF Core Change Tracker to improve memory usage
var authors = await context.Books
    .AsNoTracking()
    .Where(b => b.Year >= 2023)
    .ToListAsync();
```

If needed, you can also make all queries non-trackable within DbContext, this can be useful for readonly DbContext scenarios.

## 5. Using Eager Loading Unwisely

**Mistake:** Fetching related data when using Eager Loading can lead to performance issues if not used carefully. Including unnecessary related data via eager loading by using `Include` and `ThenInclude` methods can cause excessive database joins and result in large result sets that hurt performance.

```csharp
// Include and ThenInclude are for eager loading,
// they can lead to performance issues if not used carefully
var book = await context.Books
    .AsNoTracking()
    .Include(b => b.Author)
        .ThenInclude(a => a.Publisher)
    .ToListAsync();
```

**Solution:** Use eager loading wisely. If possible, consider using filters in `Include` and ThenInclude methods to limit data that is loaded together with the main entity.

```csharp
// You can use filters in Include and ThenInclude methods
// to limit data that is loaded together with the main entity
var authors = await context.Authors
    .Include(a => a.Books.Where(b => b.Year >= 2023))
    .ToListAsync();
```

You can also use a SplitQuery to get related data in a separate query.

```csharp
var book = await context.Books
    .AsNoTracking()
    .Include(b => b.Author)
        .ThenInclude(a => a.Publisher)
    .AsSplitQuery()
    .ToListAsync();
```

With `.AsSplitQuery()`, EF Core will generate multiple queries: one for Book data, one for Author data, and one for Publisher data.

You should use this method with cautious, as it may decrease performance in a lot of scenarios when you don't have a big set of joined data.

## 6. Ignoring Transactions When Using Batch Operations

**Mistake:** When using `ExecuteUpdate` and `ExecuteDelete` batch operations - they are not tracked in EF Core.

Batch operations like `ExecuteUpdateAsync` and `ExecuteDeleteAsync` aren't tracked by EF Core. When a `SaveChanges` method fails afterward, there is no automatic rollback for the batch operations that already happened.

```csharp
await context.Books
    .Where(b => b.Id == update.BookId)
    .ExecuteUpdateAsync(b => b
      .SetProperty(book => book.Title, book => update.NewTitle)
      .SetProperty(book => book.UpdatedAtUtc, book => DateTime.UtcNow));

// Add new author
await context.Authors.AddAsync(newAuthor);

// Save changes
await context.SaveChangesAsync();

// If Author creation fails, update operation to Books won't be reverted
// as ExecuteUpdate and ExecuteDelete batch operations are not tracked in EF Core.
```

**Solution:** Wrap your batch operations and other changes in a transaction. Commit if all operations succeed; otherwise, rollback.

```csharp
using var transaction = await _context.Database.BeginTransactionAsync();
try
{
    await context.Books
        .Where(b => b.Id == update.BookId)
        .ExecuteUpdateAsync(b => b
        .SetProperty(book => book.Title, book => update.NewTitle)
        .SetProperty(book => book.UpdatedAtUtc, book => DateTime.UtcNow));

    await context.Authors.AddAsync(newAuthor);
    await context.SaveChangesAsync();
    await transaction.CommitAsync();
}
catch (Exception)
{
    // Now you can rollback both operations
    await transaction.RollbackAsync();
    throw;
}
```

## 7. Ignoring Concurrency Handling

**Mistake:** Multiple users might edit the same record simultaneously, overwriting each other's updates without any checks.

```csharp
var book = await context.Books
    .FirstOrDefaultAsync(b => b.Id == update.BookId);

// Update properties
book.Title = update.NewTitle;
book.Year = update.NewYear;
book.UpdatedAtUtc = DateTime.UtcNow;

// What if someone already updated this book - your or their changes may be replaced
await _context.SaveChangesAsync();
```

**Solution:** Implement concurrency tokens (e.g., a row version column) and handle `DbUpdateConcurrencyException` to manage concurrent updates gracefully.

```csharp
modelBuilder.Entity<Book>()
    .Property<byte[]>("Version")
    .IsRowVersion();

var book = await context.Books
    .FirstOrDefaultAsync(b => b.Id == update.BookId);

book.Title = update.NewTitle;
book.Year = update.NewYear;
book.UpdatedAtUtc = DateTime.UtcNow;

try
{
    await _context.SaveChangesAsync();
}
catch (DbUpdateConcurrencyException ex)
{} // Decide what to do here: for example, show error to the user
```

If you catch a `DbUpdateConcurrencyException` you can do the following:

*   show error to the user
*   decide whether to keep previous or current updated data

## 8. Not Using Migrations

**Mistake:** Manually modifying the database schema without using EF Core migrations can cause an application model and the database to become out of sync. This creates a potential for many application errors.

**Solution:** Use EF Core migrations to manage schema changes.

**Recommended Steps:**

*   Use `Add-Migration` <Name> to create a migration based on model changes.
*   Use `Update-Database` to apply the migration to the database.
*   Keep your database schema versioned and consistent across environments.

**Ways to execute migrations:**

*   You can execute migrations in code to update DB directly
*   You can execute `Update-Database` command to update DB directly
*   You can export migrations to a SQL file and execute it manually in the database

## 9. Forgetting to Dispose Manually Created DbContext

**Mistake:** Not disposing of `DbContext` instances, especially when using `IDbContextFactory`, can cause memory leaks and resource exhaustion.

```csharp
private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

// 🚫 BAD CODE: Memory Leak, DbContext is not disposed
public async Task StartAsync(CancellationToken cancellationToken)
{
    var context = await _contextFactory.CreateDbContextAsync();
    var books = await context.Books.ToListAsync();
}
```

**Solution:** Correctly wrap your manual `DbContext` creation in a using statement for proper disposal.

```csharp
private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

// ✅ DbContext is correctly disposed
public async Task StartAsync(CancellationToken cancellationToken)
{
    using var context = await _contextFactory.CreateDbContextAsync();
    var books = await context.Books.ToListAsync();
}
```

## 10. Not Using Asynchronous Methods

**Mistake:** Using synchronous methods for database operations can block threads and degrade application responsiveness.

```csharp
// 🚫 The Mistake: Using synchronous methods for database operations,
// which can block threads and degrade application responsiveness.
var authors = context.Books
    .AsNoTracking()
    .Where(b => b.Year >= 2023)
    .ToList();
// ...    
context.SaveChanges();
```

**Solution:** Async methods make your application to handle more concurrent operations.

Prefer using asynchronous methods like `ToListAsync` and `SaveChangesAsync` to keep your application responsive and scalable.

```csharp
// ✅ Async methods make your application to handle more concurrent operations
var authors = await context.Books
    .AsNoTracking()
    .Where(b => b.Year >= 2023)
    .ToListAsync();
// ...    
await context.SaveChangesAsync();
```

## BONUS Mistakes to Watch Out For

*   **Saving Changes in the Loop:** Calling `SaveChanges` in a loop can hurt performance, that results in sending requests to the database in each loop iteration. Instead, make updates in the loop and call `SaveChanges` after the loop once, after all changes are made.
    
*   **Not Configuring Relationships Properly:** Incorrect relationship mappings can lead to data inconsistencies and performance issues.
    
*   **Ignoring Database Normalization:** Poor schema design can cause data duplication, anomalies, and performance issues.
    
*   **Ignoring Connection Pooling:** Not taking advantage of connection pooling in high database access scenarios - can lead to performance degradation and resource exhaustion. And you could potentially face a "Max Connection Reached" error in the database.
    

## Summary

Avoiding these common mistakes in EF Core can significantly improve your application's stability, performance, and scalability.

Keep these best practices in mind as you develop and maintain your .NET applications. Over time, these small optimizations add up, resulting in a better user experience and a more maintainable codebase.

As a result, you will be able to build efficient and reliable applications with database access.

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Ftop-10-mistakes-developers-make-in-ef-core&title=Top%2010%20Mistakes%20Developers%20Make%20in%20EF%20Core)[X](https://twitter.com/intent/tweet?text=Top%2010%20Mistakes%20Developers%20Make%20in%20EF%20Core&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Ftop-10-mistakes-developers-make-in-ef-core)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Ftop-10-mistakes-developers-make-in%2Fef-core)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.