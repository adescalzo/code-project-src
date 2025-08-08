```yaml
---
title: 10 Reasons to Upgrade to .NET 9
source: https://antondevtips.com/blog/10-reasons-to-upgrade-to-dotnet-9
date_published: 2025-01-02T11:22:03.931Z
date_captured: 2025-08-06T17:35:02.631Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [.NET 9, C# 13, ASP.NET Core, Minimal APIs, Entity Framework Core, HybridCache, Microsoft.Extensions.Caching.Hybrid, IMemoryCache, IDistributedCache, Redis, Kestrel, Blazor, .NET MAUI Blazor Hybrid, Npgsql, System.Threading.Lock, System.Threading.Monitor, nginx, Apache, Fluent UI Blazor]
programming_languages: [C#, SQL, bash]
tags: [dotnet, csharp, aspnet-core, performance, ef-core, linq, caching, web-api, blazor, upgrade]
key_concepts: [minimal-apis, hybrid-cache, linq-extensions, database-seeding, static-asset-delivery, thread-synchronization, blazor-improvements, performance-optimization]
code_examples: false
difficulty_level: intermediate
summary: |
  This article outlines 10 compelling reasons to upgrade projects to .NET 9 and C# 13, which were released on November 12, 2024. Key improvements include significant performance boosts for Minimal APIs and exceptions, and the introduction of the new HybridCache to address cache stampede problems. The article also highlights new LINQ methods, enhanced EF Core seeding and SQL translation capabilities, and C# 13 features like Params Collections and the new `System.Threading.Lock` for better thread synchronization. Furthermore, it covers ASP.NET Core's static asset delivery optimizations and various Blazor enhancements, making a strong case for adopting the latest .NET version.
---
```

# 10 Reasons to Upgrade to .NET 9

![Article banner: A dark blue background with purple abstract shapes. On the left, a white icon of a code tag `< />` next to "dev tips". On the right, large white text reads "10 REASONS TO UPGRADE TO .NET 9".](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_dotnet_9_reasons.png&w=3840&q=100)

# 10 Reasons to Upgrade to .NET 9

Jan 2, 2025

[Download source code](/source-code/10-reasons-to-upgrade-to-dotnet-9)

6 min read

### Newsletter Sponsors

[Master The Clean Architecture](https://www.courses.milanjovanovic.tech/a/aff_q2snzxhl/external?affcode=1486372_j2vpyytw). This comprehensive course will teach you how to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture. Join 3,600+ students to accelerate your growth as a software architect.

[Master The Modular Monolith Architecture](https://www.courses.milanjovanovic.tech/a/aff_l825rpg0/external?affcode=1486372_j2vpyytw). This in-depth course will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario. Join 1,600+ students to accelerate your growth as a software architect.

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

.NET 9 and C# 13 were released on November 12, 2024. In this blog post, I want to show 10 reasons why you should consider upgrading your projects to .NET 9.

[](#1-performance-improvements-of-minimal-apis)

## 1\. Performance improvements of Minimal APIs

Minimal APIs in .NET 9 received a huge performance boost and can process **15%** more requests per second than in .NET 8.

Also, Minimal APIs consume **93%** less memory compared to a previous version.

![Bar chart titled ".NET 9 Minimal API Performance". It shows "JSON Benchmarks" with two sets of bars: "Requests per Second" and "Memory (MB)". For Requests per Second, .NET 9 shows a 15% increase over .NET 8. For Memory, .NET 9 shows a 93% decrease compared to .NET 8. Source is TechEmpower.](https://antondevtips.com/media/code_screenshots/aspnetcore/dotnet-9-top-reasons/img_1.png)

This means that Minimal APIs became much faster than Controllers.

Minimal APIs have the following advantages over Controllers:

*   Better Performance (as I showed you above)
*   Single Responsibility: Each endpoint is responsible for one thing, has its own dependencies, compared to bloated Controllers in many applications that have a lot of dependencies that are used by some methods.
*   Flexibility and Control: The Minimal API approach allows for more granular control over routing and middleware configuration.
*   Development Speed: The ability to quickly set up an endpoint without the ceremony of defining controllers makes you more productive.
*   Minimal APIs also align with such modern Concepts as Vertical Slice Architecture.

[](#2-performance-improvements-of-exceptions)

## 2\. Performance improvements of Exceptions

Exceptions became 2-4 times faster, according to Microsoft Benchmarks.

![Two bar charts titled ".NET 9 Exceptions Performance". Both charts compare .NET 8 and .NET 9 for "Execution time in ms (lower is better)". The left chart shows .NET 9 is 2x faster (54.68 ms vs 123.03 ms). The right chart shows .NET 9 is 4x faster (45.53 ms vs 182.12 ms). The image also includes a small profile picture of Anton Martyniuk with his website.](https://antondevtips.com/media/code_screenshots/aspnetcore/dotnet-9-top-reasons/img_2.png)

Exceptions happen everywhere:

*   database access error
*   external system is down
*   file is not found
*   request timeout ...

Doesn't matter if you use exceptions for control flow or you have Global Exception Handler - good news, exceptions are now faster.

I have always appreciated such performance improvements for free without a need to touch my code.

Remember, exceptions are for exceptional situations. They are not the best option for a control flow. A better option is a [Result Pattern](https://antondevtips.com/blog/how-to-replace-exceptions-with-result-pattern-in-dotnet).

[](#3-new-hybridcache)

## 3\. New HybridCache

When working with cache, you must be aware of the cache stampede problem: when multiple requests receive a cache miss and will all call the database to retrieve the entity. Instead of calling the database once to get the entity and write it into cache.

Standard Microsoft `IMemoryCache` suffers from this problem. You can introduce manual locking to solve this problem, but this approach doesn't scale so well.

That's why I am using a new caching library `HybridCache` available in .NET 9 that solves a cache stampede problem.

`HybridCache` is a great replacement of old `IMemoryCache` and `IDistributedCache` as it combines them both and can work with:

*   in-memory cache
*   distributed cache like Redis

To get started with `HybridCache` add the following Nuget package:

```bash
dotnet add package Microsoft.Extensions.Caching.Hybrid
```

Here is how you can register it in DI:

```csharp
#pragma warning disable EXTEXP0018
services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(10),
        LocalCacheExpiration = TimeSpan.FromMinutes(10)
    };
});
#pragma warning restore EXTEXP0018
```

You need to disable the warning with pragma as `HybridCache` is in preview at the moment (it might be already released when you're reading this).

By default, it enables the InMemory Cache.

`HybridCache` API is similar to those from the old caches. You can use `GetOrCreateAsync` method that will check if entity exists in the cache and returns it. If the entity is not found - a delegate method is called and to get the entity from the database:

```csharp
private static async Task<IResult> Handle(
    [FromRoute] Guid id,
    HybridCache cache,
    ApplicationDbContext context,
    CancellationToken cancellationToken)
{
    var bookResponse = await cache.GetOrCreateAsync($"book-{id}",
        async token =>
        {
            var entity = await context.Books
                .AsNoTracking()
                .Include(b => b.Author)
                .FirstOrDefaultAsync(b => b.Id == id, token);

            return entity is not null ? new BookResponse(entity.Id, entity.Title, entity.Year, entity.Author.Name) : null;
        },
        cancellationToken: cancellationToken
    );

    return bookResponse is not null ? Results.Ok(bookResponse) : Results.NotFound();
}
```

[](#4-new-linq-methods-aggregateby-countby-index)

## 4\. New LINQ methods: AggregateBy, CountBy, Index

.NET 9 introduces 3 new LINQ methods: `CountBy`, `AggregateBy` and `Index`.

Let's explore the code we needed to write before and what improvements these methods bring to the table.

[](#countby)

### CountBy

Previously, we had to group the items and then count them:

```csharp
var countByName = orders
    .GroupBy(p => p.Name)
    .ToDictionary(
        g => g.Key,
        g => g.Count()
    );
```

The `CountBy` method simplifies this by directly providing a count for each key:

```csharp
var countByName = orders.CountBy(p => p.Name);

foreach (var item in countByName)
{
    Console.WriteLine($"Name: {item.Key}, Count: {item.Value}");
}
```

[](#aggregateby)

### AggregateBy

In older versions, we used `GroupBy` to group orders by category and then `Aggregate` within each group to sum up the prices:

```csharp
var totalPricesByCategory = orders
    .GroupBy(x => x.Category)
    .ToDictionary(
        g => g.Key,
        g => g.Sum(x => x.Quantity * x.Price)
    );
```

The new `AggregateBy` method simplifies this process by combining grouping and aggregation into one step:

```csharp
var totalPricesByCategory = _orders.AggregateBy(
    x => x.Category,
    _ => 0.0m,
    (total, order) => total + order.Quantity * order.Price
);

foreach (var item in totalPricesByCategory)
{
    Console.WriteLine($"Category: {item.Key}, Total Price: {item.Value}");
}
```

[](#index)

### Index

In older versions, we manually managed the index or used `Select` with a projection:

```csharp
int index = 0;
foreach (var item in orders)
{
    Console.WriteLine($"Order #{index}: {item}");
    index++;
}

foreach (var item in orders.Select((order, index) => new {order, index}))
{
    Console.WriteLine($"Order #{index}: {item}");
}
```

The new `Index` method provides a more elegant solution by providing direct access to the item and its index in a form of Tuple:

```csharp
foreach (var (index, item) in orders.Index())
{
    Console.WriteLine($"Order #{index}: {item}");
}
```

[](#5-ef-core-new-seeding-methods)

## 5\. EF Core New Seeding methods.

To seed your database with initial data in previous EF Core versions you had to:

*   create seeding with limited capabilities in DbContext in the `OnConfiguring` method
*   create separate classes, resolve and call them before the application was started (in Program.cs or a Hosted Service).

EF 9 brings a new way to seed your database. Microsoft recommends using `UseSeeding` and `UseAsyncSeeding` methods for seeding the database with initial data when working with EF Core.

You can use `UseSeeding` and `UseAsyncSeeding` methods directly in the `OnConfiguring` method in your DbContext when registering DbContext in DI:

```csharp
builder.Services.AddDbContext<ApplicationDbContext>((provider, options) =>
{
    options
        .UseNpgsql(connectionString)
        .UseAsyncSeeding(async (dbContext, _, cancellationToken) =>
        {
            var authors = GetAuthors(3);
            var books = GetBooks(20, authors);

            await dbContext.Set<Author>().AddRangeAsync(authors);
            await dbContext.Set<Book>().AddRangeAsync(books);
            
            await dbContext.SaveChangesAsync();
        });
});
```

`UseSeeding` is called from the `EnsureCreated` method, and `UseAsyncSeeding` is called from the `EnsureCreatedAsync` method. When using this feature, it is recommended to implement both `UseSeeding` and `UseAsyncSeeding` methods using similar logic, even if the code using EF is asynchronous. EF Core tooling currently relies on the synchronous version of the method and will not seed the database correctly if the `UseSeeding` method is not implemented.

[](#6-ef-core-better-linq-and-sql-translation)

## 6\. EF Core Better LINQ and SQL Translation

EF 9 allows more LINQ queries to be translated to SQL, and many SQL translations for existing scenarios have been improved. EF 9 has better performance and readability than the previous version.

Let's explore 2 most popular use cases for complex types.

[](#ef9-supports-grouping-by-a-complex-type-instance)

### EF9 supports grouping by a complex type instance.

```csharp
var groupedAddresses = await context.Customer
  .GroupBy(b => b.Address)
  .Select(g => new { g.Key, Count = g.Count() })
  .ToListAsync();
```

[](#executeupdate-has-been-improved-to-accept-complex-type-properties)

### ExecuteUpdate has been improved to accept complex type properties.

However, each member of the complex type must be specified explicitly:

```csharp
var address = new Address("New York City", "Baker's st.", "54");

await context.Customers
  .Where(e => e.Address.City == "New York City")
  .ExecuteUpdateAsync(s => s.SetProperty(b => b.StoreAddress, address));
```

[](#7-c-13-params-collection)

## 7\. C# 13 Params Collection

Previously, when using `params` keyword you were able to use only arrays:

```csharp
PrintNumbers(1, 2, 3, 4, 5);

public void PrintNumbers(params int[] numbers)
{
    // ...
}
```

C# 13 introduces Params Collections, allowing you to use the following concrete types:

*   Arrays
*   IEnumerable<T>
*   List<T>
*   Span<T>

For example, you can use a list:

```csharp
List<int>numbers = [ 1, 2, 3, 4, 5 ];
PrintNumbers(numbers);

public void PrintNumbers(params List<int> numbers)
{
    // ...
}
```

And further in `PrintNumbers` method you can use, for example, LINQ methods over the params collection.

[](#8-c-13-new-lock-object)

## 8\. C# 13 New Lock Object

`System.Threading.Lock` is a new thread synchronization type in .NET 9 runtime. It offers better and faster thread synchronization through the API.

How it works:

*   Lock.EnterScope() method enters an exclusive scope and returns a ref struct.
*   Dispose method exits the exclusive scope

When you replace `object` with a new `Lock` type inside a `lock` statement, it starts using a new thread synchronization API. Rather than using old API through `System.Threading.Monitor`.

```csharp
public class LockClass
{
    private readonly System.Threading.Lock _lockObj = new();

    public void Do(int i)
    {
        lock (_lockObj)
        {
            Console.WriteLine($"Do work: {i}");
        }
    }
}
```

[](#9-aspnet-core-static-asset-delivery-optimization)

## 9\. ASP.NET Core: Static Asset delivery optimization

Did you know that you can serve static files from your ASP NET Core application? Kestrel web server supports static files, and in .NET 9 it became even faster.

You can host your backend and frontend code using a single service (executable), so you don't need an extra service or docker container to host your frontend inside a nginx or Apache.

In .NET 8 and earlier versions you could add static files using `UseStaticFiles` function.

In .NET 9 there is a new function called `MapStaticAssets` which is a preferable way of serving static content. In .NET 9 static files are much better compressed compared to .NET 8.

`MapStaticAssets` provides the following benefits when compared to `UseStaticFiles`:

*   Build time compression for all the assets in the app: gzip during development and gzip + brotli during publish.
*   All assets are compressed with the goal of reducing the size of the assets to the minimum.
*   Content based ETags: The Etags for each resource are the Base64 encoded string of the SHA-256 hash of the content. This ensures that the browser only redownloads a file if its content has changed.

![A table showing "Original", "Compressed", and "% Reduction" sizes for various static assets. The first part of the table shows `bootstrap.min.css`, `jquery.js`, `bootstrap.min.js` with total reduction of 80.20%. The second part shows `fluent.js` and `fluent.css` (Fluent UI Blazor components library) with a total reduction of 82.43%. This illustrates the static asset delivery optimization in .NET 9.](https://antondevtips.com/media/code_screenshots/aspnetcore/dotnet-9-top-reasons/img_3.png)

[](#10-blazor-improvements)

## 10\. Blazor Improvements

.NET 9 brings the following nice updates to Blazor:

1.  .NET MAUI Blazor Hybrid and Web App solution template
2.  Detect rendering location, interactivity, and assigned render mode at runtime
3.  Improved server-side reconnection experience
4.  Simplified authentication state serialization for Blazor Web Apps
5.  Add static server-side rendering (SSR) pages to a globally-interactive Blazor Web App
6.  Constructor injection
7.  Websocket compression for Interactive Server components
8.  Handle keyboard composition events in Blazor

[](#summary)

## Summary

.NET 9 release has brought a lot of new and improvements to the existing features. The main focus for this release was:

*   to improve the performance of .NET runtime, including WebApplications running on Kestrel, Minimal APIs, Exceptions, EF Core, LINQ and others
*   to extend existing features to support more scenarios
*   add completely new features to make development better and more enjoyable

Note: .NET 9 is STS (Standard Term Support) but Microsoft's stated quality of STS and LTS (Long-Term Support) are the same. So I see no reason why you shouldn't upgrade to .NET 9. I have already migrated a lot of services to .NET 9 without a significant effort.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/10-reasons-to-upgrade-to-dotnet-9)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2F10-reasons-to-upgrade-to-dotnet-9&title=10%20Reasons%20to%20Upgrade%20to%20.NET%209)[X](https://twitter.com/intent/tweet?text=10%20Reasons%20to%20Upgrade%20to%20.NET%209&url=https%3A%2F%2Fantondevtips.com%2Fblog%2F10-reasons-to-upgrade-to-dotnet-9)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2F10-reasons-to-upgrade-to-dotnet-9)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.