```yaml
---
title: How to Avoid Common EF Core Performance Pitfalls
source: https://okyrylchuk.dev/blog/how-to-avoid-common-ef-core-performance-pitfalls/
date_published: 2025-04-18T17:38:13.000Z
date_captured: 2025-08-11T15:31:28.678Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: performance
technologies: [Entity Framework Core, .NET, SQL Server, ASP.NET Core, Entity Framework 8]
programming_languages: [C#, SQL, LINQ]
tags: [ef-core, performance, optimization, database, orm, dotnet, csharp, data-access, sql]
key_concepts: [n+1-problem, projections, asynchronous-apis, change-tracking, cartesian-explosion, bulk-operations, pagination, streaming, database-indexing]
code_examples: false
difficulty_level: intermediate
summary: |
  This article identifies and provides solutions for common performance pitfalls when using Entity Framework Core. It covers strategies such as avoiding the N+1 problem with `Include`, optimizing data retrieval using projections and `AsNoTracking`, and leveraging asynchronous APIs. The post also explains how to mitigate Cartesian explosions with `AsSplitQuery`, limit data size through pagination, and perform efficient bulk operations. Ultimately, it emphasizes that while EF Core optimizations are crucial, a well-designed database with proper indexing remains fundamental for overall application performance.
---
```

# How to Avoid Common EF Core Performance Pitfalls

# How to Avoid Common EF Core Performance Pitfalls

By Oleg Kyrylchuk / April 18, 2025

**Entity Framework Core** is one of the most popular ORMs in the .NET ecosystem — for good reason. It abstracts away the complexity of working with databases, allowing developers to focus on business logic rather than SQL queries. But here’s the catch:

The abstraction is only helpful **until it gets in your way.**

When things go wrong — like performance issues, unexpected queries, or subtle bugs — understanding how EF Core works under the hood becomes essential. Knowing what it tracks, when it queries, and how it translates LINQ into SQL can mean the difference between an app that flies and crawls.

In this post, we’ll examine **common EF Core performance pitfalls**, why they happen, and, most importantly, how to avoid them. 

## **Avoid N+1 Problem**

Let’s take a look at the following code:

```csharp
foreach (var author in await context.Authors.ToListAsync())
{
    foreach (var book in author.Books)
    {
        Console.WriteLine($"Author {author.Name}, Book: {book.Title}");
    }
}
```

What’s happening here? With lazy loading, EF Core triggers a separate query each time the Books property is accessed in the loop. So, after loading all authors, it sends **one query per author** to load books — this is known as the **N+1 problem** and can seriously hurt performance.

If you need all the books, load them all in one query using the Include method. 

```csharp
var authors = await context.Authors
    .Include(a => a.Books)
    .ToListAsync();
```

## **Use Projections**

In the example above, we need only the author’s name. We don’t use other properties. For read-only operations, project out only the properties you need.

```csharp
var authors = await context.Authors
    .Select(a => new { a.Name, a.Books })
    .ToListAsync();
```

## Use Async API

To build scalable applications, prefer **asynchronous APIs** over synchronous ones—like **SaveChangesAsync** instead of **SaveChanges**.

Synchronous calls block the thread during database I/O, leading to higher thread usage and more context switching, which can hurt performance under load.

```csharp
// Don't use
context.SaveChanges();

// Use instead
await context.SaveChangesAsync(token);
```

## **Use AsNoTracking**

For read-only queries, use the AsNoTracking method. It’ll optimize the read performance and memory usage as EF Core doesn’t have to track entity changes, which means **less overhead**.

```csharp
var authors = await context.Authors
    .Include(a => a.Books)
    .AsNoTracking()
    .ToListAsync();
```

## Use AsSplitQuery

Take a look at the following LINQ.

```csharp
var departments = await context.Departments
    .Include(d => d.Employees)
    .Include(d => d.Projects)
    .ToListAsync();
```

EF Core generates the SQL query with LEFT JOINs when we include relational collections.

![SQL query showing a Cartesian product with LEFT JOINs for Departments, Employees, and Projects.](https://okyrylchuk.dev/wp-content/uploads/2025/04/cartesiansql.png.webp)

Since Employees and Projects are related collections of Departments at the same level, the relational database produces a cross product. This means that each row from Employees is joined by each row from Projects.

Having 10 Projects and 10 Employees for a given Department, the database returns 100 rows for each Department.

It’s called a **Cartesian explosion**. It refers to a situation where a query produces an unexpectedly large number of results due to unintended cartesian products (cross joins) between tables.

To avoid a Cartesian explosion, use the **AsSplitQuery** method. 

```csharp
var departments = await context.Departments
    .Include(d => d.Employees)
    .Include(d => d.Projects)
    .AsSplitQuery()
    .ToListAsync();
```

EF Core generates three separate queries. The first query selects Departments. The other two include Projects and Employees with INNER JOINs separately.

![Three separate SQL queries generated by AsSplitQuery to avoid Cartesian explosion, fetching Departments, Employees, and Projects independently.](https://okyrylchuk.dev/wp-content/uploads/2025/04/splittingsql.png.webp)

To learn more about splitting queries, read my blog post, [Single vs. Split Query in Entity Framework](https://okyrylchuk.dev/blog/single-vs-split-query-in-entity-framework/).

## **Limit the Size of Data**

By default, a query returns all rows that match its filters. 

Limit the size of the data fetch. Use the Take method to limit the number of records you want to fetch.

You can also use the pagination. The following code shows the **Offset pagination**. 

```csharp
int position = 200;
int pageSize = 100;
var authors = await context.Authors
    .Where(a => a.Name.StartsWith('O'))
    .Skip(position)
    .Take(pageSize)
    .ToListAsync();
```

There is also **Keyset pagination.** For more information, [see the documentation page for pagination](https://learn.microsoft.com/en-us/ef/core/querying/pagination). 

## Use Bulk Delete and Update

Use the **ExecuteDeleteAsync** and **ExecuteUpdateAsync** methods for batch operations. 

```csharp
await context.Products
    .Where(p => p.StockQuantity == 0)
    .ExecuteDeleteAsync();

await context.Products
    .Where(p => p.Category == "iPhone")
    .ExecuteUpdateAsync(
        p => p.SetProperty(x => x.StockQuantity, 0));
```

But remember some limitations. 

Entity Framework Core doesn’t create implicit transactions for these methods as SaveChanges does. So, if you have several bulk updates and want to perform them within one transaction, you must begin it explicitly. 

For more details, see my post [Efficient Bulk Updates in Entity Framework](https://okyrylchuk.dev/blog/efficient-bulk-updates-in-entity-framework/).

## Use Raw SQL

```csharp
var authors = await context.Authors
    .FromSqlRaw("SELECT * FROM Authors WHERE Name LIKE {0}", "O%")
    .ToListAsync();
```

To learn how you can diagnose what queries EF Core generates, see my post [Logging and Diagnostics in Entity Framework](https://okyrylchuk.dev/blog/logging-and-diagnostics-in-entity-framework/).

## **Use Streaming**

**Buffering** occurs when you call methods like ToListAsync() or ToArrayAsync( )—the entire result set is loaded into memory at once.

On the other hand, **Streaming** returns one row at a time without holding everything in memory.

Streaming uses a fixed amount of memory, whether your query returns 1 row or 10,000. Buffering, however, consumes more memory as the result grows.

Streaming can be a much more efficient option for large result sets or performance-sensitive scenarios.

```csharp
// Use AsAsyncEnumerable to stream data
await foreach (var author in 
    context.Authors.Where(p => p.Name.StartsWith('O')).AsAsyncEnumerable())
{
    
}
```

## **Conclusion**

All the above tips are related to client-side performance improvements. However, they could not help if the database was poorly designed.

For example, use Indexes to improve read performance.

Learn how to design relational databases better because EF Core cannot fix everything.