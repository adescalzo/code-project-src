```yaml
---
title: How To Increase EF Core Performance for Read Queries in .NET
source: https://antondevtips.com/blog/how-to-increase-ef-core-performance-for-read-queries-in-dotnet
date_published: 2024-10-30T11:55:26.139Z
date_captured: 2025-08-06T17:29:34.476Z
domain: antondevtips.com
author: Anton Martyniuk
category: performance
technologies: [EF Core, .NET, SQL Server, PostgreSQL, Redis, BenchmarkDotNet, HybridCache, Entity Framework Extensions]
programming_languages: [C#, SQL]
tags: [ef-core, performance, dotnet, database, query-optimization, data-access, caching, sql, orm, read-queries]
key_concepts: [database-indexing, query-projection, change-tracking, eager-loading, pagination, compiled-queries, raw-sql, caching]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides actionable techniques to enhance the performance of read queries in EF Core for .NET applications. It covers optimizing database interactions through proper indexing, efficient query projections, and disabling change tracking for read-only operations using `AsNoTracking`. Further tips include the wise use of eager loading, implementing pagination (both offset-based and cursor-based), and leveraging compiled queries for frequently executed operations. The post also discusses `AsSplitQuery` to mitigate cartesian explosion, the use of raw SQL for complex scenarios, and introducing caching with systems like Redis or .NET 9's `HybridCache`. It concludes by recommending performance measurement tools like BenchmarkDotNet and SQL Server Management Studio for execution plan analysis.]
---
```

# How To Increase EF Core Performance for Read Queries in .NET

![Cover image for the article titled 'How To Increase EF Core Performance for Read Queries in .NET', featuring a dark background with purple abstract shapes and a white icon with `</>` and 'dev tips' text.](https://antondevtips.com/media/covers/efcore/cover_ef_read_performance.png)

# How To Increase EF Core Performance for Read Queries in .NET

Oct 30, 2024

[Download source code](/source-code/how-to-increase-ef-core-performance-for-read-queries-in-dotnet)

5 min read

### Newsletter Sponsors

[EF Core is too slow?](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website) Discover how you can easily insert 14x faster (reducing saving time by 94%).  
Boost your performance with our method integrated within EF Core: Bulk Insert, update, delete, and merge.  
Join 5,000+ satisfied customers who have trusted our library since 2014.

[Learn more](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website)

[Master The Clean Architecture](https://www.courses.milanjovanovic.tech/a/aff_q2snzxhl/external?affcode=1486372_j2vpyytw). This comprehensive course will teach you how to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture. Join 3,150+ students to accelerate your growth as a software architect.

[Master The Modular Monolith Architecture](https://www.courses.milanjovanovic.tech/a/aff_l825rpg0/external?affcode=1486372_j2vpyytw). This in-depth course will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario. Join 1,050+ students to accelerate your growth as a software architect.

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Optimizing read queries in EF Core can significantly enhance the performance of your .NET applications.

In this blog post, I will share with you actionable techniques to enhance the performance of read queries in EF Core.

[](#understanding-ef-core-query-execution)

## Understanding EF Core Query Execution

Before diving into optimization techniques, it's essential to understand how EF Core processes queries:

*   **LINQ Query Definition:** you define queries using LINQ.
*   **Translation to SQL:** EF Core translates LINQ queries into SQL queries compatible with the underlying database.
*   **Query Execution:** SQL query is executed against the database.
*   **Materialization:** results are mapped back to .NET objects.

Performance issues can arise at any of these stages, so optimization strategies may target different parts of this process.

[](#performance-tip-1-add-database-indexes)

## Performance Tip 1: Add Database Indexes

Ensure your database tables are properly **indexed** based on query patterns. EF Core doesn't create indexes automatically, so you need to define them manually in your database schema.

Here is how you can create an index using SQL:

```sql
CREATE INDEX IX_Books_Title ON Books(Title);
```

You can also create an index in EF Core Mapping with `HasIndex` method, and it will be added to the database migration:

```csharp
public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("books");

        builder.HasKey(x => x.Id);
        builder.HasIndex(x => x.Title);
    }
}
```

Create **indexes** on columns that are frequently used in `WHERE`, `ORDER BY`, and `JOIN` clauses.

[](#performance-tip-2-optimize-query-projections)

## Performance Tip 2: Optimize Query Projections

Retrieve only the necessary fields rather than the entire entity.

Let's imagine that you need to show a Book Title, Author and Year of publication on your website in the book preview. Instead of retrieving a full `Book` entity together with `Author`:

```csharp
var book = await context.Books
    .Include(b => b.Author)
    .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
```

You can retrieve only the needed properties:

```csharp
var book = await context.Books
    .Include(b => b.Author)
    .Where(b => b.Id == id)
    .Select(b => new BooksPreviewResponse
    {
        Title = b.Title, Author = b.Author.Name, Year = b.Year
    })
    .FirstOrDefaultAsync(cancellationToken);
```

This reduces the amount of data transferred from the database and the time spent when materializing entities.

[](#performance-tip-3-use-asnotracking-for-readonly-queries)

## Performance Tip 3: Use AsNoTracking for Read-Only Queries

By default, EF Core tracks entities to detect changes. For read-only operations, this tracking is unnecessary and adds performance overhead.

You can use `AsNoTracking` method to disable change tracking for the query, reducing memory usage and CPU overhead.

```csharp
var book = await context.Books
    .AsNoTracking()
    .Include(b => b.Author)
    .Select(b => new BooksPreviewResponse
    {
        Title = b.Title, Author = b.Author.Name, Year = b.Year
    })
    .FirstOrDefaultAsync();
```

You can use `AsNoTracking` method when you only need to retrieve the entity which will be not further updated or deleted during [DbContext lifetime](https://antondevtips.com/blog/how-to-manage-ef-core-dbcontext-lifetime). In other words, you should be using `AsNoTracking` method when you don't plan to call the `SaveChanges` method.

In some scenarios, you might have a readonly DbContext. So you can disable change tracking for all queries:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>((provider, options) =>
{
    var interceptor = provider.GetRequiredService<AuditableInterceptor>();

    options.EnableSensitiveDataLogging()
        .UseNpgsql(connectionString)
        .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});
```

When using global `QueryTrackingBehavior.NoTracking` you can call the `.AsTracking()` method to enable change tracking for a given query.

[](#performance-tip-4-use-eager-loading-wisely)

## Performance Tip 4: Use Eager Loading Wisely

Fetching related data can result in multiple database queries (N+1 problem). Use eager loading to include related entities.

While `Include` and `ThenInclude` are useful for eager loading, they can lead to performance issues if not used carefully.

```csharp
var book = await context.Books
    .AsNoTracking()
    .Include(b => b.Author)
        .ThenInclude(a => a.Publisher)
    .ToListAsync();
```

Ensure you're not loading unnecessary related data.

You can use filters in `Include` and `ThenInclude` methods to limit data that is loaded together with the main entity. For example:

```csharp
var authors = await context.Authors
    .Include(a => a.Books.Where(b => b.Year >= 2023))
    .ToListAsync();
```

[](#performance-tip-5-use-pagination-for-large-datasets)

## Performance Tip 5: Use Pagination for Large Datasets

Prefer using pagination instead of retrieving the whole large dataset to avoid loading too much data into memory.

```csharp
int pageSize = 50;
int pageNumber = 1;

var books = context.Books
    .AsNoTracking()
    .OrderBy(p => p.Title)
    .Skip((pageNumber - 1) * pageSize)
    .Take(pageSize)
    .ToList();
```

If possible, prefer **cursor-based** pagination to **offset-based** pagination as it is much faster.

```csharp
var books = context.Books
    .AsNoTracking()
    .Where(b => b.Id > lastBookId)
    .OrderBy(b => b.Title)
    .Take(pageSize)
    .ToList();
```

**Offset-based** pagination is slower because it needs to go through a given numbers of database rows to skip them. While, **cursor-based** pagination uses a WHERE clause on the indexed field (often primary key) that discards a set of rows.

[](#performance-tip-6-use-compiled-queries)

## Performance Tip 6: Use Compiled Queries

For frequently executed queries, **compiled queries** can improve performance by caching the translation of LINQ queries to SQL.

Here is how you can create and use a **compiled query**:

```csharp
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    private static readonly Func<ApplicationDbContext, string, Book?> BookByTitle
        = EF.CompileQuery((ApplicationDbContext context, string title) =>
            context.Books
                .Include(b => b.Author)
                .FirstOrDefault(b => b.Title == title)
        );

    public Book? GetBookByTitle(string title)
        => BookByTitle(this, title);
}

var book = context.GetBookByTitle("Clean Code");
```

You can also create an **async** compiled query:

```csharp
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    private static readonly Func<ApplicationDbContext, string, CancellationToken, Task<Book?>> BookByTitleAsync
        = EF.CompileAsyncQuery((ApplicationDbContext context, string title, CancellationToken cancellationToken) =>
            context.Books
                .Include(b => b.Author)
                .FirstOrDefault(b => b.Title == title)
        );

    public async Task<Book?> GetBookByTitleAsync(string title, CancellationToken cancellationToken)
        => await BookByTitleAsync(this, title, cancellationToken);
}

var book = await context.GetBookByTitleAsync("Clean Code", cancellationToken);
```

[](#performance-tip-7-use-splitquery-to-avoid-cartesian-explosion)

## Performance Tip 7: Use SplitQuery to Avoid Cartesian Explosion

For queries that include related data, EF Core can split the query into multiple SQL statements to avoid cartesian explosion.

By default, EF Core generates a single SQL query with JOINs to retrieve all the data.

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

[](#performance-tip-8-use-raw-sql-queries-for-complex-operations)

## Performance Tip 8: Use Raw SQL Queries for Complex Operations

In some cases, it might be more efficient to write an SQL query yourself.

```csharp
var year = 2020;

var newBooks = await context.Database
    .SqlQuery<Book>($"SELECT * FROM Books WHERE Year > {year}")
    .ToListAsync();
```

You can also write queries that are not supported by EF Core:

```csharp
var sql = @"
    SELECT * FROM ""Books""
    WHERE ""Year"" < @Year
    FOR UPDATE
";

var yearParameter = new NpgsqlParameter("@Year", NpgsqlTypes.NpgsqlDbType.Integer)
{
    Value = 2020
};

var books = await context.Books
    .FromSqlRaw(sql, yearParameter)
    .ToListAsync();
```

[](#performance-tip-9-introduce-caching)

## Performance Tip 9: Introduce Caching

You can introduce caching for frequently used queries to increase performance and reduce a database load. You can use in-memory or distributed caching systems like Redis.

```csharp
public async Task<Book?> GetBookAsync(Guid id, CancellationToken cancellationToken)
{
    var cacheKey = $"Book_{id}";

    if (_cache.TryGetValue(cacheKey, out Book? book))
    {
        return book;
    }
    
    book = await _context.Books
        .Include(b => b.Author)
        .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        
    return book;
}
```

In .NET 9 I highly recommend using new `HybridCache` that solves the Cache Stampede problem.

[](#summary)

## Summary

In this blog post, you have learned various techniques that allow increasing the performance of your read queries in EF Core.

Before taking any significant actions, I recommend the following:

*   Measure the performance of your queries with BenchmarkDotNet
*   Use tools like SQL Server Management Studio to view database execution plans
*   Scan your indexes for fragmentation and take actions if needed

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-increase-ef-core-performance-for-read-queries-in-dotnet)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-increase-ef-core-performance-for-read-queries-in-dotnet&title=How%20To%20Increase%20EF%20Core%20Performance%20for%20Read%20Queries%20in%20.NET)[X](https://twitter.com/intent/tweet?text=How%20To%20Increase%20EF%20Core%20Performance%20for%20Read%20Queries%20in%20.NET&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-increase-ef-core-performance%2Fread-queries-in-dotnet)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-increase-ef-core-performance-for-read-queries-in-dotnet)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.