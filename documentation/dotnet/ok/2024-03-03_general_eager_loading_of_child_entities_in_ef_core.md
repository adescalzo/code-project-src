```yaml
---
title: Eager Loading of Child Entities in EF Core
source: https://antondevtips.com/blog/eager-loading-of-child-entities-in-ef-core
date_published: 2024-03-03T21:07:51.357Z
date_captured: 2025-08-06T17:19:40.067Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [EF Core, .NET, Entity Framework Extensions]
programming_languages: [C#, SQL]
tags: [ef-core, eager-loading, data-access, orm, dotnet, database, performance, entity-framework, query-optimization]
key_concepts: [eager-loading, explicit-loading, lazy-loading, n-plus-1-problem, navigation-properties, linq-to-entities, auto-include, include-filtering]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to eager loading of child entities in EF Core, a crucial strategy for managing related data and optimizing database queries. It explains the concept of eager loading, contrasting it with explicit and lazy loading, and demonstrates how to use the `Include` and `ThenInclude` methods for single and multi-level relationships. The post also covers advanced features like `AutoInclude` for automatic property loading and applying filters to included entities. By leveraging eager loading, developers can avoid the N+1 query problem, reduce database round trips, and enhance application performance.]
---
```

# Eager Loading of Child Entities in EF Core

![A banner image with a dark background and purple abstract shapes. It features a white square icon with angle brackets `</>` and the text 'dev tips'. The main title reads 'EAGER LOADING OF CHILD ENTITIES IN EF CORE' in large white font, with 'Improve your coding skills' below it.](https://antondevtips.com/media/covers/efcore/cover_efcore_eager.png)

# Eager Loading of Child Entities in EF Core

Mar 3, 2024

4 min read

### Newsletter Sponsors

[EF Core is too slow?](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website) Discover how you can easily insert 14x faster (reducing saving time by 94%).  
Boost your performance with our method integrated within EF Core: Bulk Insert, update, delete, and merge.  
Join 5,000+ satisfied customers who have trusted our library since 2014.

[Learn more](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website)

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

## Understanding Eager Loading

One of advantages of using EF Core is the ability to manage related data through various loading strategies. **There are 3 types of loading the child entities in EF Core:**

*   **Eager loading**: Preloading related entities as part of the initial query, ensuring all necessary entities are loaded together.
*   **Explicit loading**: Manually loading related entities on-demand, providing control over when and what related entities are retrieved.
*   **Lazy loading**: Automatically loading related entities as they are accessed, offering a deferred retrieval approach for related entities.

In today's post were are going to have a look on Eager loading. **Eager loading** is a technique where related entities are loaded from the database as a part of the initial query.

Eager loading is essential for scenarios where you know you'll need related data for every entity retrieved. It avoids the N+1 query problem, reducing the number of round trips to the database.

## Using an Include method for loading related entities

Let's have a look on the following entities:

```csharp
public class Author
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required List<Book> Books { get; set; } = new();
}

public class Category
{
    public required Guid Id { get; set; }
    public required string Name { get; set; }
    public required List<Book> Books { get; set; } = new();
}

public class Book
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required int Year { get; set; }
    public required Author Author { get; set; }
    public required Category Category { get; set; }
}
```

You can use the **Include** method in EF Core method to specify the related entities to be included in the query. In the following example, when books are queried, their respective authors are fetched in the same query:

```csharp
await using var dbContext = new BooksDbContext();

// Select books with authors
var books = await dbContext.Books
    .Include(b => b.Author)
    .ToListAsync();
```

You can include multiple related entities in a single query:

```csharp
await using var dbContext = new BooksDbContext();

// Select books with authors and categories
var books = await dbContext.Books
    .Include(b => b.Author)
    .Include(b => b.Category)
    .ToListAsync();
```

In this example, when books are queried, their respective authors and categories are fetched in the same query.

**Include** method can also be used to load related entities by a collection navigation property:

```csharp
await using var dbContext = new BooksDbContext();

var authors = await dbContext.Authors
    .Include(a => a.Books)
    .ToListAsync();
```

In this example, when books are queried, their respective authors and categories are fetched in the same query.

## Multi-level Include

Multi-level eager loading allows to load entities together with their related entities, including relationships of included entities any level deep, in a single query. This can be particularly useful when you have complex data models with multiple levels of relationships.

You can use **ThenInclude** method to include related entities of the included ones. Let's expand our Book and Author example:

```csharp
public class Author
{
    public required Guid AddressId { get; set; }
    public required Address Address { get; set; }
    // Other properties...
}

public class Address
{
    public required Guid Id { get; set; }
    public required string City { get; set; }
    public required string Street { get; set; }
    // Other properties...
}

await using var dbContext = new BooksDbContext();

// Select books with authors and their addresses
var books = await dbContext.Books
    .Include(b => b.Category)
    .Include(b => b.Author)
        .ThenInclude(a => a.Address)
    .ToListAsync();
```

In this example we are loading Book entities and their related Authors, after that we're loading Authors' Addresses. A query may contain multiple Includes with ThenIncludes, the are no limitations on how deep we can use the these methods.

## Auto Include

EF Core 5.0 introduced a feature called **AutoInclude** which automatically includes certain navigation properties every time the entity is loaded from the database. You can use **AutoInclude** method in the entity configuration to specify what navigation properties should be loaded with a given entity in every database query. This feature is especially useful for navigation properties that are frequently loaded with the primary entity, simplifying queries and reducing the risk of forgetting to include the related data. Let's have a look on the example how to include Author everytime a book is queried from the database:

```csharp
// Put this code in the DbContext or EntityConfiguration class
// where you do the mapping
modelBuilder.Entity<Book>()
    .Navigation(e => e.Author)
    .AutoInclude();
    
await using var dbContext = new BooksDbContext();

// Authors are automatically included here
var books = await dbContext.Books
    .ToListAsync();
```

If in some case you don't need to load the AutoIncluded entities, you can use the **IgnoreAutoIncludes** method to remove the effect of AutoInclude:

```csharp
// Put this code in the DbContext or EntityConfiguration class
// where you do the mapping
modelBuilder.Entity<Book>()
    .Navigation(e => e.Author)
    .AutoInclude();
    
await using var dbContext = new BooksDbContext();

// Select books without auto included authors
var books = await dbContext.Books
    .IgnoreAutoIncludes()
    .ToListAsync();
```

## Include with filtering

In EF Core you can apply filtering on the included entities. This feature allows to load only those related entities that meet a given condition.

```csharp
await using var dbContext = new BooksDbContext();
    
var authors = await dbContext.Authors
    .Include(a => a.Books.Where(b => b.Year >= 2023))
    .ToListAsync();
```

In this example, the Include method is used with a filtering condition inside a Where clause. This ensures that only books published in the year 2023 and later are included for each author.

EF Core supports the following operations in the Include statement: `Where`, `Skip`, `Take`, `OrderBy`, `OrderByDescending`, `ThenBy`, `ThenByDescending`.

**EF Core has a limitation:** each included navigation property can only have one unique set of filtering operations.

## Summary

EF Core provides a robust mechanism for handling data relationships through various loading strategies, with **eager loading** being used most widely. Eager loading allows for the pre-loading of related entities as part of the initial query, ensuring that all necessary data is retrieved in a single round trip to the database. This method is essential for optimizing performance and simplifying data access patterns in applications. In EF Core, eager loading is implemented using the **Include** method, which specifies the navigation properties to be loaded along with the primary entity. This can be further extended with **ThenInclude** for multi-level relationships, allowing for deep navigation through related entities.

If some navigations are frequently loaded with the primary entity, simplify the queries using **AutoInclude** feature. It automatically includes certain navigation properties every time the entity is loaded from the database. EF Core also supports such powerful feature as **Include with filtering** even for a more control of the related data being loaded.

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Feager-loading-of-child-entities-in-ef-core&title=Eager%20Loading%20of%20Child%20Entities%20in%20EF%20Core)[X](https://twitter.com/intent/tweet?text=Eager%20Loading%20of%20Child%20Entities%20in%20EF%20Core&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Feager-loading-of-child-entities-in-ef-core)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Feager-loading-of-child-entities-in-ef-core)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.