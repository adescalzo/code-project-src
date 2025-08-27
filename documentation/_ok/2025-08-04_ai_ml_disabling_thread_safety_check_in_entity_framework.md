```yaml
---
title: Disabling thread safety check in Entity Framework
source: https://steven-giesel.com/blogPost/f1c14e0f-840c-4dd6-921c-364298a35d6f?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2104
date_published: 2025-08-04T08:51:00.000Z
date_captured: 2025-08-11T19:51:24.213Z
domain: steven-giesel.com
author: Unknown
category: ai_ml
technologies: [Entity Framework, EF Core, SQLite, BenchmarkDotNet, .NET]
programming_languages: [C#, SQL]
tags: [entity-framework, ef-core, performance, thread-safety, dotnet, database, concurrency, orm, benchmarking]
key_concepts: [thread-safety, dbcontext, dbcontext-factory, performance-optimization, concurrency, benchmarking, data-access]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores the thread safety check within Entity Framework's `DbContext` and how to disable it. It highlights that `DbContext` is not thread-safe by default, demonstrating how concurrent access can lead to exceptions and suggesting `IDbContextFactory` as the proper solution for multi-threaded scenarios. The post then details how to disable the internal thread safety check using `DbContextOptionsBuilder.EnableThreadSafetyChecks(false)`, cautioning about the risks of data corruption if not handled carefully. Benchmark results are presented, showing a very minor performance gain, leading to the conclusion that this optimization is an advanced use case and `IDbContextFactory` is generally a superior approach.
---
```

# Disabling thread safety check in Entity Framework

# Disabling thread safety check in Entity Framework

8/4/2025

4 minute read

[C#](/searchByTag/C%23)[EF](/searchByTag/EF)[Performance](/searchByTag/Performance)

## 

Table of Contents

[Thread safety check in Entity Framework](https://steven-giesel.com/blogPost/f1c14e0f-840c-4dd6-921c-364298a35d6f?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2104#thread-safety-check-in-entity-framework)[Disabling the thread safety check](https://steven-giesel.com/blogPost/f1c14e0f-840c-4dd6-921c-364298a35d6f?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2104#disabling-the-thread-safety-check)[Resources](https://steven-giesel.com/blogPost/f1c14e0f-840c-4dd6-921c-364298a35d6f?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2104#resources)

In this blog post we will have a look into how to disable the thread safety check in Entity Framework. What are the implications of doing so and how to do it.

## Thread safety check in Entity Framework

`DbContext` by default is not thread safe. Therefore the `DbContext` tries to detect whether or not it is being used in a thread safe manner. Imagine the following code:

```csharp
using var context = new MyDbContext();

var task1 = context.Users.ToListAsync();
var task2 = context.BlogPosts.ToListAsync();

await Task.WhenAll(task1, task2);
```

This will throw an exception because the `DbContext` is being used in two different threads at the same time. If you want to do the above, you have to resort to the `IDbContextFactory` to create a new `DbContext` for each task:

```csharp
public MyService(IDbContextFactory<MyDbContext> dbContextFactory)
{
    _dbContextFactory = dbContextFactory;
}

var dbContext1 = _dbContextFactory.CreateDbContext();
var dbContext2 = _dbContextFactory.CreateDbContext();
var task1 = dbContext1.Users.ToListAsync();
var task2 = dbContext2.BlogPosts.ToListAsync();
await Task.WhenAll(task1, task2);
```

## Disabling the thread safety check

That check comes with a performance "penalty" (that is very minor). If you want to disable the thread safety check, you can do so by setting the `DbContextOptionsBuilder.EnableThreadSafetyChecks` property to `false`:

```csharp
var optionsBuilder = new DbContextOptionsBuilder<MyDbContext>()
    .UseSqlite("Data Source=mydatabase.db")
    .EnableThreadSafetyChecks(false);
```

**Word of caution here:** Doing this will disable the thread safety check and you have to be 100% sure that you are not using the `DbContext` in a different thread at the same time. For example by using the `IDbContextFactory` to create a new `DbContext` for each task as shown above that makes the `DbContext` thread safe.

If you fail to ensure that, you will run into hard-to-debug issues that can lead to data corruption or other unexpected behavior in your application. (I will put this into the resources section down below).

The small upside is: Performance gain. Let's have a check here (code is also at the end of the post):

```csharp
[MemoryDiagnoser]
public class EntityFrameworkBenchmark
{
    private const int Iterations = 1000;
    private TestDbContextWithThreadSafety? _dbContextWithThreadSafety;
    private TestDbContextWithoutThreadSafety? _dbContextWithoutThreadSafety;

    [GlobalSetup]
    public void Setup()
    {
        _dbContextWithThreadSafety = new TestDbContextWithThreadSafety();
        _dbContextWithThreadSafety.Database.EnsureCreated();
        
        _dbContextWithoutThreadSafety = new TestDbContextWithoutThreadSafety();
        _dbContextWithoutThreadSafety.Database.EnsureCreated();
    }

    [GlobalCleanup]
    public async Task Cleanup()
    {
        if (_dbContextWithThreadSafety is not null)
        {
            await _dbContextWithThreadSafety.DisposeAsync();
        }
        
        if (_dbContextWithoutThreadSafety is not null)
        {
            await _dbContextWithoutThreadSafety.DisposeAsync();
        }
    }

    [Benchmark(Baseline = true)]
    public async Task<List<TestEntity>> WithThreadSafetyChecks()
    {
        var results = new List<TestEntity>();
        for (var i = 0; i < Iterations; i++)
        {
            results.AddRange(await _dbContextWithThreadSafety!.TestEntities.ToListAsync());
        }

        return results;
    }

    [Benchmark]
    public async Task<List<TestEntity>> WithoutThreadSafetyChecks()
    {
        var results = new List<TestEntity>();
        for (var i = 0; i < Iterations; i++)
        {
            results.AddRange(await _dbContextWithoutThreadSafety!.TestEntities.ToListAsync());
        }

        return results;
    }
}
```

And here are the results:

```no
| Method                    | Mean      | Error     | StdDev    | Ratio | RatioSD | Gen0     | Gen1     | Allocated | Alloc Ratio |
|-------------------------- |----------:|----------:|----------:|------:|--------:|---------:|---------:|----------:|------------:|
| WithThreadSafetyChecks    | 10.090 ms | 0.1381 ms | 0.1224 ms |  1.00 |    0.02 | 875.0000 | 437.5000 |   6.98 MB |        1.00 |
| WithoutThreadSafetyChecks |  9.423 ms | 0.1332 ms | 0.1112 ms |  0.93 |    0.02 | 843.7500 | 421.8750 |    6.8 MB |        0.97 |
```

A very small gain - keep in mind that there is exactly 0 data coming back from the database, so this is just a pure performance test of the `DbContext` itself. If we would have data coming back from the database, the performance gain would be even smaller (relative to the overall runtime)!

So does it make sense to use that? Well, as a very advanced use case, maybe. But using something like a pooled `IDbContextFactory` is a much better option in my opinion before resorting to this.

## Resources

*   Official Microsoft Documentation for Entity Framework: [https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cexpression-api-with-constant](https://learn.microsoft.com/en-us/ef/core/performance/advanced-performance-topics?tabs=with-di%2Cexpression-api-with-constant)
*   Source code for this blog post: [https://github.com/linkdotnet/BlogExamples/tree/main/EFDisableThreadSafety](https://github.com/linkdotnet/BlogExamples/tree/main/EFDisableThreadSafety)
*   Source code for many of my blog posts: [https://github.com/linkdotnet/BlogExamples](https://github.com/linkdotnet/BlogExamples)

2

*   [Copy To Clipboard](javascript:void\(0\))
*   ---
    
*   [Share on LinkedIn](https://www.linkedin.com/shareArticle?mini=true&url=https://steven-giesel.com/blogPost/f1c14e0f-840c-4dd6-921c-364298a35d6f?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2104)
*   [Share on X](https://twitter.com/intent/tweet?url=https://steven-giesel.com/blogPost/f1c14e0f-840c-4dd6-921c-364298a35d6f?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2104)
*   [Share on Bluesky](https://bsky.app/intent/compose?text=https://steven-giesel.com/blogPost/f1c14e0f-840c-4dd6-921c-364298a35d6f?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2104)

[![Buy Me a Coffee at ko-fi.com](https://cdn.ko-fi.com/cdn/kofi2.png?v=3)](https://ko-fi.com/U7U8BDPWR)

[Sponsors](https://github.com/sponsors/linkdotnet)

## Related articles you might enjoy

[](blogPost/12c2926a-cf51-438a-af5a-5d160e9f3844/entity-framework-and-ordered-indexes)

[

![Entity Framework and ordered indexes](https://linkdotnetblog.azureedge.net/blog/20230507_OrderedIndex/Thumbnail.avif)

](blogPost/12c2926a-cf51-438a-af5a-5d160e9f3844/entity-framework-and-ordered-indexes)

[](blogPost/12c2926a-cf51-438a-af5a-5d160e9f3844/entity-framework-and-ordered-indexes)

[

###### Entity Framework and ordered indexes

In Entity Framework 7, the team has added support for ordered indexes to the fluent API. In this blog post we will look at how to use this feature and what it means for your database.

5/8/2023

3 minute read

](blogPost/12c2926a-cf51-438a-af5a-5d160e9f3844/entity-framework-and-ordered-indexes)

[](blogPost/12c2926a-cf51-438a-af5a-5d160e9f3844/entity-framework-and-ordered-indexes)[Entity Framework](/searchByTag/Entity%20Framework)[.NET](/searchByTag/.NET)[C#](/searchByTag/C%23)

[](blogPost/5bf635cb-3533-4207-905f-81eb86512219/entity-framework-storing-complex-objects-as-json)

[

![Entity Framework - Storing complex objects as JSON](https://linkdotnetblog.azureedge.net/blog/20231214_EFJson/Thumbnail.avif)

](blogPost/5bf635cb-3533-4207-905f-81eb86512219/entity-framework-storing-complex-objects-as-json)

[](blogPost/5bf635cb-3533-4207-905f-81eb86512219/entity-framework-storing-complex-objects-as-json)

[

###### Entity Framework - Storing complex objects as JSON

From time to time, it is nice to store complex objects or lists as JSON in the database. With Entity Framework 8, this is now easily possible. But this was possible all along with Entity Framework 7.

12/14/2023

2 minute read

](blogPost/5bf635cb-3533-4207-905f-81eb86512219/entity-framework-storing-complex-objects-as-json)

[](blogPost/5bf635cb-3533-4207-905f-81eb86512219/entity-framework-storing-complex-objects-as-json)[C#](/searchByTag/C%23)[.NET](/searchByTag/.NET)[Entity Framework](/searchByTag/Entity%20Framework)[EF](/searchByTag/EF)[EF 8](/searchByTag/EF%208)

[](blogPost/d1f069fb-7f6d-4f80-a98f-734755474ae1/entity-framework-8-raw-sql-queries-on-unmapped-types)

[

![Entity Framework 8: Raw SQL queries on unmapped types](https://linkdotnetblog.azureedge.net/blog/20230112_EF8/Thumbnail.avif)

](blogPost/d1f069fb-7f6d-4f80-a98f-734755474ae1/entity-framework-8-raw-sql-queries-on-unmapped-types)

[](blogPost/d1f069fb-7f6d-4f80-a98f-734755474ae1/entity-framework-8-raw-sql-queries-on-unmapped-types)

[

###### Entity Framework 8: Raw SQL queries on unmapped types

The next iteration of Entity Framework, namely Entity Framework 8, will have a new and exciting feature: Support raw SQL queries without defining an entity type for the result That means less boilerplate code!

1/12/2023

1 minute read

](blogPost/d1f069fb-7f6d-4f80-a98f-734755474ae1/entity-framework-8-raw-sql-queries-on-unmapped-types)

[](blogPost/d1f069fb-7f6d-4f80-a98f-734755474ae1/entity-framework-8-raw-sql-queries-on-unmapped-types)[C#](/searchByTag/C%23)[Entity Framework](/searchByTag/Entity%20Framework)[.NET 8](/searchByTag/.NET%208)