```yaml
---
title: What are the Best Practices for EF Core Performance Optimization?
source: https://www.c-sharpcorner.com/article/what-are-the-best-practices-for-ef-core-performance-optimization/?utm_source=dotnetnews.beehiiv.com&utm_medium=newsletter&utm_campaign=the-net-news-daily-issue-261
date_published: 2025-08-07T00:00:00.000Z
date_captured: 2025-08-13T11:17:12.669Z
domain: www.c-sharpcorner.com
author: Gaurav Kumar
category: performance
technologies: [EF Core, .NET, SQL Server, EFCore.BulkExtensions, MiniProfiler, ILogger, MemoryCache, Visual Studio Diagnostic Tools]
programming_languages: [C#, SQL, JavaScript, CSS]
tags: [ef-core, performance-optimization, dotnet, database, orm, csharp, data-access, query-optimization, best-practices, scalability]
key_concepts: [ORM, performance-tuning, query-optimization, change-tracking, batching, eager-loading, caching, compiled-queries]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to optimizing Entity Framework Core performance in .NET applications. It outlines 14 best practices, including using `AsNoTracking()` for read-only queries, projecting only required columns, and avoiding the N+1 query problem with eager loading. The guide also covers batching operations, leveraging database indexes, using compiled queries, and implementing caching strategies. Practical C# code examples illustrate each optimization technique, emphasizing the importance of profiling and using asynchronous queries for improved scalability and reduced latency.]
---
```

# What are the Best Practices for EF Core Performance Optimization?

[![.NET Logo](https://www.c-sharpcorner.com/UploadFile/MinorCatImages/dot_net_2015_060314128.png ".NET")](https://www.c-sharpcorner.com/technologies/dotnet)

# What are the Best Practices for EF Core Performance Optimization?

*   [![Author Profile Photo](https://www.c-sharpcorner.com/UploadFile/AuthorImage/eotjnmo420250225083445.jpeg.ashx?height=24&width=24 "Gaurav Kumar")](https://www.c-sharpcorner.com/members/gaurav-kumar217)Gaurav Kumar
*   6d
*   416
*   0
*   2

[100](/members/rank.aspx?page=points-table "Points")*   [Article](/articles/)

Take the challenge

## ⚡ Introduction to EF Core Performance Optimization

Entity Framework Core (EF Core) is a powerful and flexible ORM for .NET, but if not used carefully, it can introduce performance bottlenecks. We'll explore best practices to improve EF Core performance, reduce query latency, and ensure efficient memory usage, all with practical C# examples.

## 🚀 1. Use AsNoTracking() for Read-Only Queries

EF Core tracks entities by default, which is unnecessary when you're only reading data. Disabling tracking speeds up queries and reduces memory usage.

```csharp
var users = context.Users
    .AsNoTracking()
    .ToList();
```

### ✅ When to Use

*   In read-only queries
*   In reporting or dashboard views
*   When tracking adds unnecessary overhead

## 🧠 2. Project Only Required Columns with `Select`

Avoid fetching entire entities when you only need a few fields.

```csharp
var names = context.Users
    .Select(u => u.Name)
    .ToList();
```

### ✅ Benefits

*   Reduces payload size
*   Improves query performance
*   Avoids unnecessary joins and data hydration

## 🔄 3. Avoid the N+1 Query Problem

The N+1 issue happens when lazy loading causes multiple queries unintentionally.

### ❌ Bad

```csharp
var orders = context.Orders.ToList();

foreach (var order in orders)
{
    Console.WriteLine(order.Customer.Name); // Triggers a query each time
}
```

### ✅ Good (Eager Loading)

```csharp
var orders = context.Orders
    .Include(o => o.Customer)
    .ToList();
```

## 📦 4. Use Batching with `AddRange` / `UpdateRange`

Instead of calling SaveChanges() repeatedly, batch your inserts or updates:

```csharp
context.Users.AddRange(user1, user2, user3);
await context.SaveChangesAsync();
```

### 🔥 For large batches

Use libraries like EFCore.BulkExtensions to dramatically reduce insert/update time.

## 📌 5. Prefer FirstOrDefault() or Any() Instead of Count()

Count() checks all matching rows. Use Any() if you just need to check existence.

```csharp
// Slower
bool hasUsers = context.Users.Count() > 0;

// Faster
bool hasUsers = context.Users.Any();
```

## 🛠️ 6. Use Indexes on Frequently Queried Columns

EF Core doesn’t manage database indexes, you must define them manually or in your migrations:

```csharp
modelBuilder.Entity<User>()
    .HasIndex(u => u.Email)
    .IsUnique();
```

### ✅ When to Use

*   On columns used in WHERE, JOIN, or ORDER BY
*   For foreign keys or commonly filtered fields

## 🔍 7. Use ToList() or ToArray() at the Right Time

Don’t call ToList() too early if you can keep the query in-memory longer to add more filters or projections.

### ✅ Example:

```csharp
var recentUsers = context.Users
    .Where(u => u.CreatedAt > DateTime.UtcNow.AddDays(-7))
    .Select(u => u.Name)
    .ToList(); // Only at the end!
```

## 🧮 8. Use Compiled Queries for High-Throughput Scenarios

Compiled queries cache the query translation step, reducing overhead on repeated execution.

```csharp
private static readonly Func<AppDbContext, int, User?> _getUserById =
    EF.CompileQuery((AppDbContext context, int id) =>
        context.Users.FirstOrDefault(u => u.Id == id));

// Usage
var user = _getUserById(context, 5);
```

## 📁 9. Cache Frequently Used Data

Don’t hit the database for static data. Use in-memory caching:

```csharp
var roles = memoryCache.GetOrCreate("roles", entry =>
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1);
    return context.Roles.AsNoTracking().ToList();
});
```

Or use a distributed cache for multiple server environments.

## 🧵 10. Use `async` Queries for Scalability

Always prefer `async` versions of EF Core methods:

```csharp
var user = await context.Users
    .FirstOrDefaultAsync(u => u.Email == "test@example.com");
```

### ✅ Benefits

*   Prevents thread blocking
*   Improves scalability in web apps

## 🧹 11. Avoid Unnecessary Change Tracking and Detach When Needed

If you need to load and discard data without tracking:

```csharp
context.Entry(entity).State = EntityState.Detached;
```

Useful in long-running contexts or background jobs to avoid memory bloat.

## 📊 12. Profile Queries and Use Logging

Use tools like:

*   EF Core logging (ILogger)
*   MiniProfiler
*   SQL Server Profiler
*   Visual Studio Diagnostic Tools

To inspect:

*   Query duration
*   Redundant database calls
*   Unoptimized SQL generated by LINQ

## 🧱 13. Use Raw SQL for Complex Queries (When Necessary)

When LINQ becomes inefficient or unreadable:

```csharp
var users = context.Users
    .FromSqlRaw("SELECT * FROM Users WHERE IsActive = 1")
    .AsNoTracking()
    .ToList();
```

⚠️ Be cautious with SQL injection, parameterize your queries!

## 🧰 14. Use Route-Level Filtering in APIs

Instead of filtering in-memory or in the controller, do it in the query itself:

```csharp
// Good
var result = await context.Users
    .Where(u => u.Role == "Admin")
    .ToListAsync();
```

## 📝 Conclusion

EF Core is powerful, but performance can suffer without thoughtful design. Apply these best practices to:

*   Reduce latency
*   Minimize memory usage
*   Avoid costly database operations
*   Scale efficiently

⚡ A fast app is a happy app, optimize early, monitor often!

People also reading

*   AsNoTracking vs. Change Tracking: When is it absolutely crucial to disable change tracking and are there scenarios where the cost of re-attaching entities outweighs the benefits of using AsNoTracking?

    EF Core's change tracking is powerful but comes at a cost. When querying for read-only purposes, `AsNoTracking()` significantly reduces overhead by preventing the context from tracking entity modifications. This saves memory and improves query speed. However, disabling tracking might require re-attaching entities if you later need to update them, which can be complex. Consider scenarios with frequent updates to small datasets where tracking overhead is minimal. Conversely, in large reporting systems or data warehousing, `AsNoTracking()` is essential. The decision hinges on the balance between read performance gains and the complexity of managing detached entities. Analyze query patterns and data modification needs to determine the optimal approach. Pay close attention to scenarios where child entities are involved, as detaching can lead to unexpected behavior if relationships aren't handled correctly. Furthermore, in highly concurrent environments, the reduced memory footprint of `AsNoTracking()` is beneficial.

Upcoming Events

[View all](/chapters/)

*   13 AUG

    [The Cloud Show with Magnus Mårtensson ft. Prabhat Nigam - Ep: 74](https://www.c-sharpcorner.com/events/the-cloud-show-with-magnus-mårtensson-ft-prabhat-nigam-ep-74 "The Cloud Show with Magnus Mårtensson ft. Prabhat Nigam - Ep: 74")

*   14 AUG

    [ChainTalks: The Web3 Spotlight](https://www.c-sharpcorner.com/events/chaintalks-the-web3-spotlight "ChainTalks: The Web3 Spotlight")

*   19 AUG

    [Automated Sales Quote Approval Workflow for Faster Deal Closure: Automate Your Business Processes - Ep.9](https://www.c-sharpcorner.com/events/automated-sales-quote-approval-workflow-for-faster-deal-closure-automate-your-business-processes-ep9 "Automated Sales Quote Approval Workflow for Faster Deal Closure: Automate Your Business Processes - Ep.9")

Ebook download

[View all](/ebooks)

[![Ebook cover for 'DateTime in C#/.NET' by Mahesh Chand, featuring a clock and calendar icon.](https://www.c-sharpcorner.com/UploadFile/EBooks/04042024184802PM/04042024191209PMDateTime%20in%20csharp.png)](https://www.c-sharpcorner.com/ebooks/datetime-in-csharp)

[

DateTime in C#

](https://www.c-sharpcorner.com/ebooks/datetime-in-csharp)

Read by 1.3k people

[Download Now!](https://www.c-sharpcorner.com/ebooks/datetime-in-csharp)

[![Ad for C# Corner's AI Trainings, showing a woman working on a laptop and listing courses like Generative AI, Prompt Engineering, and LLMs, with an 80% off offer.](https://www.c-sharpcorner.com/UploadFile/Ads/13.jpg)](https://learnai.c-sharpcorner.com "C# Corner's AI Trainings")

[![Ad for Instructor-led Trainings: Mastering Prompt Engineering, featuring a futuristic robot head.](https://www.c-sharpcorner.com/UploadFile/Ads/14.jpg)](https://learnai.c-sharpcorner.com/trainings/prompt-engineering "Mastering Prompt Engineering")