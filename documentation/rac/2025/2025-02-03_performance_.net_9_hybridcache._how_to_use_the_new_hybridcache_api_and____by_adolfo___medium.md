```yaml
---
title: ".NET 9 HybridCache. How to use the new HybridCache API and… | by Adolfo | Medium"
source: https://medium.com/@FitoMAD/net-9-hybridcache-5c0b4137f70d
date_published: 2025-02-03T16:43:02.823Z
date_captured: 2025-08-17T22:04:05.745Z
domain: medium.com
author: Adolfo
category: performance
technologies: [.NET 9, HybridCache, IDistributedCache, IMemoryCache, Microsoft.Extensions.Caching.Hybrid, ASP.NET Core, NuGet]
programming_languages: [C#]
tags: [caching, dotnet, .net-9, performance, distributed-cache, in-memory-cache, api, software-design, repository-pattern, delegates]
key_concepts: [HybridCache, L1/L2 caching, cache-stampede-protection, configurable-serialization, repository-pattern, single-responsibility-principle, functional-programming, dependency-injection]
code_examples: false
difficulty_level: intermediate
summary: |
  The article introduces .NET 9's new HybridCache API, a unified caching solution combining in-memory (L1) and distributed (L2) caching with features like cache stampede protection. It explains how to configure and use HybridCache with practical C# code examples for adding, retrieving, and removing data. The author also discusses advanced software design considerations, particularly how to integrate HybridCache with the Repository Pattern using a functional approach with delegates to maintain architectural principles like SRP. This guide helps developers implement efficient and well-designed caching strategies in .NET 9 applications.
---
```

# .NET 9 HybridCache. How to use the new HybridCache API and… | by Adolfo | Medium

# .NET 9 HybridCache

![Adolfo](https://miro.medium.com/v2/resize:fill:64:64/1*2Gux-0OARM8qSp5jo-Fxbg@2x.jpeg)

Adolfo

Follow

5 min read

·

Feb 3, 2025

7

3

Listen

Share

More

![.NET 9 HybridCache - The new unified caching API that combines in-memory and distributed caching.](https://miro.medium.com/v2/resize:fit:700/1*Rahj-_7CdVGej8Ygywg1tg.jpeg)
*Image: A banner image for the article titled ".NET 9 HybridCache" with the subtitle "The new unified caching API that combines in-memory and distributed caching." and the .NET logo.*

The new release of .NET 9 brings developers a new API to work with caching filling the gaps that currently exist with the `IDistributedCache` and the `IMemoryCache` APIs.

The new `**HybridCache**` is an L1/L2 cache systems that **combines in-memory and distributed cache systems and while also offering features like cache stampede protection and configurable serialization** for custom types.

If you’re wondering what cache stampede is I can tell you that…

> Cache stampede happens when a frequently used cache entry is revoked, and too many requests try to repopulate the same cache entry at the same time

This new API is still under development and the final release will be available in a .NET 9.X release.

![Microsoft's HybrisCache documentation page with an important note about its preview status.](https://miro.medium.com/v2/resize:fit:700/1*hGDdSPdJEn1IYg5JFkT4vg.png)
*Image: A screenshot of Microsoft's HybridCache documentation page, highlighting an "Important" note stating that HybridCache is currently in preview and will be fully released after .NET 9.0 in a future minor release of .NET Extensions.*

Microsoft’s HybrisCache documentation page.

# Start an HybridCache solution

The `HybridCache` is available in the `Microsoft.Extensions.Caching.Hybrid` nuget package. To install it in your project, run the following command:

```bash
dotnet add package Microsoft.Extensions.Caching.Hybrid --prerelease
```

Now, you can add the `HybridCache` to your application service using the default parameters.

```csharp
builder.Services.AddHybridCache();
```

Or you can configure the cache using the overloaded method that accepts an `option` named parameter that you can use to set your desired values.

```csharp
builder.Services.AddHybridCache(options =>
{
    options.MaximumPayloadBytes = 1024 * 1024; // 1MB
    options.MaximumKeyLength = 1024;
});
```

The `option` parameter is a `HybridCacheOptions` instance with the following properties:

*   `MaximumPayloadBytes` It’s the maximum size for each cache entry. The default value is 1 MB
*   `MaximumKeyLength` The maximum allowed length for a cache Key. Default value is 1024 characters.

If a new entry exceeds one or both of these parameters the entry will not be added to the cache.

The `HybridCacheEntryOptions` object is used to set the lifetime for a given entry in the L1/L2 caches.

*   `Expiration` Set the expiration time for an entry in the distributed cache.
*   `LocalCacheExpiration` The expiration time for an entry but in the in-memory cache

# Working with HybridCache

Congrats! You got your HybridCache service up and running, so it’s time to start populating the cache with your data, retrieving some stored information from it, and even removing data if necessary.

Let’s start with the most common operation when working with a cache: Get some data from it.

## Getting information

You can retrieve data from the cache thanks to the `GetOrCreateAsync` method. This method allows developers to fetch specific data from the cache, and if it’s not available, provide a data source to populate the cache with the desired data. Let’s see an example

```csharp
public async Task<Book> GetBookAsync(string bookId, CancellationToken cancellationToken = default)
{
    var book = await _hybridCache.GetOrCreateAsync(bookId, async (token) =>
    {
        // Simulate a database call
        await Task.Delay(100, token);
        return new Book(bookId, "The Hitchhiker's Guide to the Galaxy", "Douglas Adams");
    },
    cancellationToken: cancellationToken);

    return book;
}
```

The `GetOrCreateAsync` method accepts the following parametes:

*   **Key**: It’s a `string` value for the element we look for or create.
*   **Factory**: It’s a `Func<CancellationToken, ValueTask<T>>` using to retrieve the data if it’s not available at this moment on the cache. The data will be added to the cache to make it available in next cache calls.
*   **Options**: An `HybridCacheEntryOptions` object to set custom settings for this entry.
*   **Tags**: An `IEnumerable<string>` object with tags associated with this entry.
*   **CancellationToken**: A `CancellationToken` object used to propagate notifications than the operation should be cancelled.

In my example I only need the Key, the Factory and the CancellationToken parameters.

## Storing data

As you has seen in the previous section, you can store information at the same time you request it if that information it’s not available, but there are a way to store data directly using the `SetAsync` method.

It’s important to mention that `SetAsync` update the L1 and L2 cache levels, and if you provide and existing cache item ID, the data will be overwritten.

```csharp
public async Task SetBookAsync(string bookId, Book book, CancellationToken cancellationToken = default)
{
    var options = new HybridCacheEntryOptions
    {
        Expiration = TimeSpan.FromMinutes(30), // Distributed cache expiration
        LocalCacheExpiration = TimeSpan.FromMinutes(5) // In-memory cache expiration
    };

    await _hybridCache.SetAsync(bookId, book, options, cancellationToken: cancellationToken);
}
```

*   In the example above, I create first a `Book` object that I want to store in my cache.
*   Then, I set custom lifetime options for the distributed and local caches.
*   And finally I store the book in the cache with a custom ID.

The `SetAsync` method also accepts a set of tags and a `CancellationToken` as parameters.

## Deleting data

The API also provides the overloaded method `RemoveAsync` that allows us to delete a single element or a collection from the cache.

In the example below I will remove only one element from the cache, passing the element ID.

```csharp
public async Task RemoveBookAsync(string bookId, CancellationToken cancellationToken = default)
{
    await _hybridCache.RemoveAsync(bookId, cancellationToken: cancellationToken);
}
```

In the next example, I’m going to delete a collection of elements passing an `IEnumerable<string>` object implementation.

```csharp
public async Task RemoveBooksAsync(IEnumerable<string> bookIds, CancellationToken cancellationToken = default)
{
    await _hybridCache.RemoveAsync(bookIds, cancellationToken: cancellationToken);
}
```

The method also accepts a `CancellationToken` type parameter in case we need to cancel the whole operation.

# Design an HybridCache solution

Implementing the new cache solution is easy, but as a software designer, you might encounter challenges when designing how to use the cache, especially when working with the `GetOrCreateAsync` method.

As I mentioned in the dedicated section, this method uses a `Func` to request the data if it is not available at the time of the request, and in many cases that request is implemented in a different repository.

The Repository Pattern allows us to call one repository from another, but we must be careful not to violate fundamental design principles.

*   **Violating Single Reposability Principle**. A repository should only manage a single entity type (in our example, the `Book` entity).
    If we introduce a second entity, such as `BookCache`, with different properties, we risk breaking SRP.
*   **Encapsulation issues**. Imagine we have a `CacheRepository` that contains a `DatabaseRepository` inside it. This tight coupling can make testing more difficult.

However, the way the `GetOrCreateAsync` method works makes this more complex, since the cache repository must call the source-of-truth repository when data is missing.

## A functional approach using delegates

One effective way to solve this issue is by injecting a `Func` delegate into the cache repository. This delegate will be executed only if the requested data is not available in the cache.

Here you will find an example of this approach.

First, we define the interfaces for the database and the cache repositories. Please, notice that the cache repository operation requires a `Func` delegate parameter.

```csharp
public interface IBookRepository
{
    Task<Book> GetBookAsync(string id, CancellationToken cancellationToken = default);
}

public interface IBookCacheRepository
{
    Task<Book> GetBookAsync(string id, Func<CancellationToken, ValueTask<Book>> sourceOfTruth, CancellationToken cancellationToken = default);
}
```

Now we implement the repositories. The `MockBookRepository` is a test repository that only contains one book. The `LocalCacheBookRepository` is a L1 cache implementation.

```csharp
public class MockBookRepository : IBookRepository
{
    public Task<Book> GetBookAsync(string id, CancellationToken cancellationToken = default)
    {
        // Simulate a database call
        return Task.FromResult(new Book(id, "The Hitchhiker's Guide to the Galaxy", "Douglas Adams"));
    }
}

public class LocalCacheBookRepository : IBookCacheRepository
{
    private readonly IHybridCache _hybridCache;

    public LocalCacheBookRepository(IHybridCache hybridCache)
    {
        _hybridCache = hybridCache;
    }

    public async Task<Book> GetBookAsync(string id, Func<CancellationToken, ValueTask<Book>> sourceOfTruth, CancellationToken cancellationToken = default)
    {
        return await _hybridCache.GetOrCreateAsync(id, sourceOfTruth, cancellationToken: cancellationToken);
    }
}
```

As you can see on line 28, we invoke the `Func` delegate named `sourceOfTruth` in case requested data is not available in the cache. This approach makes the cache repository free of other repositories instances and makes testing easier.

Now let’s go with the service implementation, using the `BookService` type that accepts both cache implementations in the constructor.

```csharp
public class BookService
{
    private readonly IBookRepository _bookRepository;
    private readonly IBookCacheRepository _bookCacheRepository;

    public BookService(IBookRepository bookRepository, IBookCacheRepository bookCacheRepository)
    {
        _bookRepository = bookRepository;
        _bookCacheRepository = bookCacheRepository;
    }

    public async Task<Book> GetBookAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _bookCacheRepository.GetBookAsync(id, async (token) =>
        {
            return await _bookRepository.GetBookAsync(id, token);
        }, cancellationToken);
    }
}
```

In the `GetBookAsync` method, first I creation the `Func` delegate that invokes the database repository, and I pass this delegate to the cache repository in case the request book is not available in the cache.

This is an approach, and of course there are other approaches that could fit better to your project depending on the design.

# Links

*   Microsoft.Extensions.Caching.Hybrid [nuget package](https://www.nuget.org/packages/Microsoft.Extensions.Caching.Hybrid/9.1.0-preview.1.25064.3)
*   [HybridCache library in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/performance/caching/hybrid?view=aspnetcore-9.0)