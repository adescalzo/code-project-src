```yaml
---
title: "Stop Using .Include() — It's Slowing Down Your EF Core Queries (Do This Instead) | by Nikhil Sharma | CodeElevation | Jul, 2025 | Medium"
source: https://medium.com/codeelevation/stop-using-include-its-slowing-down-your-ef-core-queries-do-this-instead-36ef746cd68c
date_published: 2025-07-30T15:46:21.811Z
date_captured: 2025-08-08T11:55:09.641Z
domain: medium.com
author: Nikhil Sharma
category: general
technologies: [Entity Framework Core, .NET, MiniProfiler, GraphQL]
programming_languages: [C#, SQL]
tags: [ef-core, performance, database, query-optimization, data-access, dotnet, orm, csharp, sql-queries, data-shaping]
key_concepts: [cartesian-explosion, data-overfetching, projections, split-queries, service-layer, lazy-loading, query-optimization, data-shaping]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article highlights the performance pitfalls of using `.Include()` in Entity Framework Core, such as Cartesian explosion and data over-fetching, which lead to slow queries and high memory consumption. It proposes several effective alternatives, including using `Select()` for precise data projections, splitting complex queries into smaller ones, and leveraging `.AsSplitQuery()` for better SQL generation. The author also suggests implementing smart data loaders within a service layer and adopting flexible field selection similar to GraphQL. The piece emphasizes the importance of intentional data loading and query monitoring to optimize application performance and reduce resource usage.]
---
```

# Stop Using .Include() — It's Slowing Down Your EF Core Queries (Do This Instead) | by Nikhil Sharma | CodeElevation | Jul, 2025 | Medium

# Stop Using `.Include()` — It's Slowing Down Your EF Core Queries (Do This Instead)

![A stressed developer sitting at a desk in front of a computer screen displaying code. A red "STOP" sign glows above the monitor, and sticky notes with "include)" written on them are attached to the screen, symbolizing the frustration and performance issues associated with the `.Include()` method in EF Core.](https://miro.medium.com/v2/resize:fit:700/1*XGzrrhPIXLMbjXBnTZ9lug.jpeg)

`.Include()` in Entity Framework Core _seems_ helpful… until your app scales and queries slow to a crawl. What looks like a shortcut often bloats your SQL, fetches unnecessary data, and hurts performance.

Let’s break down why `.Include()` is dangerous—and what smarter strategies you should use instead.

# Why `.Include()` Can Be a Problem

# 1. It Blows Up Your Result Sets (Cartesian Explosion)

When you include multiple related entities, EF creates complex JOINs. The result? Duplicate rows, wasted memory, and slow queries.

```csharp
// Creates a cartesian product  
var blogs = await context.Blogs  
    .Include(b => b.Posts)  
        .ThenInclude(p => p.Comments)  
    .Include(b => b.Tags)  
    .ToListAsync();
```

If each blog has 10 posts and 20 comments, you’re not getting 10 rows — you’re getting hundreds or even thousands.

# 2. It Over-fetches by Default

You often don’t need _all_ the related data, but `.Include()` doesn’t care. It loads everything—every field, every relation.

```csharp
// You only need the user's name but load the full profile  
var orders = await context.Orders  
    .Include(o => o.Customer)  
        .ThenInclude(c => c.Profile)  
    .ToListAsync();
```

This is like using a bulldozer to move a paperclip.

# What To Do Instead

# ✅ 1. Use Projections (`Select`)

Only grab the fields you actually use.

```csharp
var orderSummaries = await context.Orders  
    .Select(o => new {  
        o.Id,  
        o.OrderDate,  
        CustomerName = o.Customer.Profile.FirstName + " " + o.Customer.Profile.LastName,  
        o.TotalAmount  
    })  
    .ToListAsync();
```

**Why it’s better:**

*   Smaller queries
*   Lower memory usage
*   Faster execution

# ✅ 2. Break Up Large Includes Into Smaller Queries

Instead of fetching everything in one go, split the workload:

```csharp
var blog = await context.Blogs  
    .Include(b => b.Tags)  
    .FirstOrDefaultAsync(b => b.Id == blogId);  
  
var posts = await context.Posts  
    .Where(p => p.BlogId == blogId)  
    .Select(p => new { p.Id, p.Title })  
    .ToListAsync();
```

This gives you more control and avoids complex SQL.

# ✅ 3. Use `.AsSplitQuery()` If You Must Use `.Include()`

EF Core 5+ lets you break large JOINs into smaller queries:

```csharp
var blogs = await context.Blogs  
    .Include(b => b.Posts)  
    .AsSplitQuery()  
    .ToListAsync();
```

Fewer JOINs, cleaner SQL, better performance.

# ✅ 4. Create Smart Data Loaders (Service Layer)

Wrap your data access in well-structured services:

```csharp
public async Task<BlogDto> GetBlogDetail(int blogId) {  
    var blog = await _context.Blogs  
        .Select(b => new BlogDto {  
            Id = b.Id,  
            Title = b.Title,  
            Tags = b.Tags.Select(t => t.Name).ToList()  
        })  
        .FirstOrDefaultAsync(b => b.Id == blogId);  
      
    // Load related posts only if needed  
    blog.RecentPosts = await _context.Posts  
        .Where(p => p.BlogId == blogId)  
        .Select(p => new PostDto {  
            Id = p.Id,  
            Title = p.Title  
        })  
        .ToListAsync();  
  
    return blog;  
}
```

# ✅ 5. Use Flexible Field Selection (Like GraphQL)

Build generic methods that return just what the caller asks for:

```csharp
public Task<List<T>> GetOrdersAsync<T>(Expression<Func<Order, T>> selector)  
{  
    return _context.Orders  
        .OrderByDescending(o => o.OrderDate)  
        .Take(20)  
        .Select(selector)  
        .ToListAsync();  
}
```

# Real-World Performance Win

```csharp
// ❌ .Include(): ~2.5s | 50K rows | 45 MB memory  
var blogs = await context.Blogs  
    .Include(b => b.Posts)  
        .ThenInclude(p => p.Comments)  
    .Take(100)  
    .ToListAsync();  
  
// ✅ Projection: ~150ms | ~2 MB memory  
var summaries = await context.Blogs  
    .Select(b => new {  
        b.Id,  
        PostCount = b.Posts.Count(),  
        TotalComments = b.Posts.Sum(p => p.Comments.Count())  
    })  
    .Take(100)  
    .ToListAsync();
```

# Best Practices

*   ✅ Use `Select()` to shape the data
*   ✅ Split heavy queries into smaller ones
*   ✅ Use `.AsSplitQuery()` when needed
*   ✅ Implement paging for lists
*   ✅ Monitor queries with `.ToQueryString()` or MiniProfiler
*   ✅ Avoid lazy loading unless you _really_ know what you’re doing

# When `.Include()` Is Okay

*   Loading simple 1:1 relations
*   Small datasets
*   Prototypes / internal tools
*   With `.AsSplitQuery()` and measured impact

**Bottom line:** Don’t blindly reach for `.Include()`. Be intentional. Measure. Optimize. Your users (and your cloud bill) will thank you.