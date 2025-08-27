```yaml
---
title: "EF Core Bulk Insert: Boost Your Performance With Entity Framework Extensions"
source: https://antondevtips.com/blog/ef-core-bulk-insert-boost-your-performance-with-entity-framework-extensions
date_published: 2025-04-01T07:45:14.691Z
date_captured: 2025-08-06T16:58:30.318Z
domain: antondevtips.com
author: Anton Martyniuk
category: performance
technologies: [Entity Framework Core, Entity Framework Extensions, Z.EntityFramework.Extensions.EFCore, Dapper, SqlBulkCopy, SQL Server, MySQL, MariaDB, Oracle, PostgreSQL, SQLite, .NET, ASP.NET Core, NuGet, Bogus]
programming_languages: [C#, SQL]
tags: [ef-core, bulk-operations, performance, data-access, database, dotnet, orm, optimization, web-api, library]
key_concepts: [bulk-insert, bulk-update, bulk-delete, bulk-merge, bulk-synchronize, performance-optimization, entity-tracking, object-graph]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article addresses performance bottlenecks when inserting large datasets with Entity Framework Core's `SaveChanges()`. It introduces Entity Framework Extensions as a superior alternative to `SaveChanges()`, Dapper, or `SqlBulkCopy` for bulk operations. The post details various bulk methods like `BulkInsert`, `BulkInsertOptimized`, `BulkUpdate`, `BulkDelete`, `BulkMerge`, `BulkSynchronize`, and `WhereBulkContains`, showcasing their usage and performance benefits. It highlights the library's multi-provider support, ease of use, and advanced configuration options for handling complex scenarios and object graphs. The article concludes by positioning EF Extensions as a versatile tool for efficient data management in .NET applications.]
---
```

# EF Core Bulk Insert: Boost Your Performance With Entity Framework Extensions

![Banner image for the article titled 'EF Core Bulk Insert: Boost Your Performance With Entity Framework Extensions', featuring a code icon and 'dev tips' text on a dark background with purple abstract shapes.](https://antondevtips.com/media/covers/efcore/cover_efcore_ef_extensions.png)

# EF Core Bulk Insert: Boost Your Performance With Entity Framework Extensions

Apr 1, 2025

[Download source code](/source-code/ef-core-bulk-insert-boost-your-performance-with-entity-framework-extensions)

8 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

When working with large datasets in Entity Framework Core, developers often hit performance bottlenecks using `SaveChanges()`.

Each entity insertion triggers a separate database round-trip and increases memory usage because of entity tracking overhead. This becomes even more noticeable as the number of rows grows into the thousands or millions.

I remember when I had to insert one million records into the database. I started the insertion, then went to make a cup of coffee while waiting for completion. It took that long to finish.

What can we do to improve insert performance?

*   **Using Dapper?** No, as it also sends each insert as a separate round-trip to the database.
*   **Maybe using SqlBulkCopy?** That's not ideal because you need a lot of custom code, especially if you want to insert child entities or return identity values. And it only works with SQL Server, so it's not suitable if you need to support other providers.

There is a better solution: [Entity Framework Extensions](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) library.

This library offers simpler, more elegant and configurable options for bulk inserts.

Let's dive in.

## Bulk Insert

To get started with **Entity Framework Extensions** install the following Nuget package:

```bash
dotnet add package Z.EntityFramework.Extensions.EFCore
```

**Entity Framework Extensions** allows you to bulk insert thousands of entities with a single line of code:

```csharp
using Z.EntityFramework.Extensions;

var products = GenerateProducts(10_000);
await dbContext.BulkInsertAsync(products);
```

Both [BulkInsert](https://entityframework-extensions.net/bulk-insert?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) and [BulkInsertAsync](https://entityframework-extensions.net/bulk-insert?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) methods are available.

### BulkInsert and Child Entities

What if you want to insert a collection of entities with their related children, a few levels deep, all in a single operation?

EF Extensions has an [IncludeGraph](https://entityframework-extensions.net/bulk-insert#insert-with-related-child-entities-include-graph?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) option for this case. It lets you bulk insert an entire object graph without manually saving each level:

Let's explore the `ProductCart` entity and all the child entities, they have the following hierarchy:

*   ProductCart
    *   ProductCartItem
        *   Product
    *   User

```csharp
public class ProductCart
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }
    
    public List<ProductCartItem> CartItems { get; set; } = [];
    
    public int UserId { get; set; }
    public User User { get; set; }
    
    public DateTime CreatedOn { get; set; } = DateTime.UtcNow;
}

public class ProductCartItem
{
    public int Id { get; set; }

    public Guid ProductCartId { get; set; }
    public ProductCart ProductCart { get; set; } = null!;

    public int ProductId { get; set; }
    public Product Product { get; set; } = null!;

    public int Quantity { get; set; }
}

public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Description { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
}
```

This hierarchy of 4 entities can be inserted with a single line of code:

```csharp
var productCarts = GenerateProductCarts(10_000);
await dbContext.BulkInsertAsync(productCarts, options => options.IncludeGraph = true);

private static List<ProductCart> GenerateProductCarts(int count)
{
    var users = GenerateUsers(100);

    var products = GenerateProducts(200);

    return GenerateProductCarts(products, users, count);
}
```

### BulkInsert and Identity Values

By default, the `BulkInsert` method returns the Identity Value when inserting. However, this behavior decreases performance.

You can improve performance by setting `AutoMapOutputDirection` option to `false`:

```csharp
context.BulkInsert(products, options => options.AutoMapOutputDirection = false);
```

This parameter turns off returning the Identity value after insertion.

There are even more options to make your inserts more performant. Let's have a look at `BulkInsertOptimized` method.

## Bulk Insert Optimized

EF Extensions provide the [BulkInsertOptimized](https://entityframework-extensions.net/bulk-insert-optimized?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) method that avoids returning identity or other output values after insertion.

Under the hood, EF Extensions use temporary table when outputting values. Instead, `BulkInsertOptimized` uses BulkCopy strategy directly into the destination table.

In general `BulkInsertOptimized` acts the same as `BulkInsert` method with the `AutoMapOutputDirection = false` option.

But the main difference is that [BulkInsertOptimized](https://entityframework-extensions.net/bulk-insert-optimized?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) provides hints and recommendations for better performance. It returns the following object:

```csharp
public class BulkOptimizedAnalysis 
{
    /// <summary>True if the bulk insert is optimized.</summary>
    public bool IsOptimized { get; }

    /// <summary>Gets a text containing all tips to optimize the bulk insert method.</summary>
    public string TipsText { get; }
    
    /// <summary>Gets a list of tips to optimize the bulk insert method.</summary>
    public List<string> Tips { get; }
}
```

Let's explore an example when insertion is optimized:

```csharp
var products = GenerateProducts(10_000);
var result = await dbContext.BulkInsertOptimizedAsync(products);

Console.WriteLine($"Was optimized: {result.IsOptimized}");
// {"isOptimized":true,"tipsText":"The `BulkInsertOptimized` operation is optimized.","tips":[]}
```

In some cases, you might lose optimization if you enable certain options. For example:

```csharp
var products = GenerateProducts(10_000);

var result = context.BulkInsertOptimized(products, options => {
    options.InsertIfNotExists = true;
});

Console.WriteLine($"Was optimized: {result.IsOptimized}");
// {"isOptimized":false,"tipsText":"The option InsertIfNotExists = true forces the use of a less efficient strategy...","tips":[]}
```

The following tip is returned: "The option InsertIfNotExists = true forces the use of a less efficient strategy, resulting in a considerable performance penalty."

For SQL Server, EF Extensions cannot directly use a `SqlBulkCopy` to insert if the row doesn't already exist. This requires using a temporary table.

## Bulk Insert Performance and Memory Usage

When working with large datasets, EF Core's default `SaveChanges` can consume a lot of memory and slow down your application. EF Extensions' bulk methods significantly reduce memory usage and improve performance by:

*   Minimizing database round-trips with batch inserts
*   Avoiding entity tracking
*   Not returning database-generated values

Let's compare the performance of bulk insert methods with `SaveChanges`:

*   **Insert:** 14x faster, reducing time by 93% [Online Benchmark](https://dotnetfiddle.net/cFWgKV)
*   **Update:** 4x faster, reducing time by 75% [Online Benchmark](https://dotnetfiddle.net/ope4nq)
*   **Delete:** 3x faster, reducing time by 65% [Online Benchmark](https://dotnetfiddle.net/zzMQgZ)

I have tested the following database queries via Web API:

```csharp
app.MapPost("/products/efcore-insert", async (ProductDbContext dbContext) =>
{
    var products = GenerateProducts(10_000);
    dbContext.Products.AddRange(products);

    await dbContext.SaveChangesAsync();

    return Results.Ok("10,000 products inserted using EF Core SaveChanges.");
});

app.MapPost("/products/efcore-bulk-insert", async (ProductDbContext dbContext) =>
{
    var products = GenerateProducts(10_000);
    await dbContext.BulkInsertAsync(products);

    return Results.Ok("10,000 products inserted using Bulk Insert of EF Core Extensions.");
});

app.MapPost("/products/efcore-bulk-insert-optimized", async (ProductDbContext dbContext) =>
{
    var products = GenerateProducts(10_000);
    var result = await dbContext.BulkInsertOptimizedAsync(products);

    return Results.Ok(result);
});
```

I have tested these queries on a Postgres database, and here are the results for inserting 10\_000 products via Web API requests:

*   SaveChanges - 2,011 ms
*   BulkInsert - 560 ms
*   BulkInsertOptimized - 270 ms

> Note: benchmarks can vary depending on your hardware and database provider.

## Customizing EF Core Extensions in Real-Life Scenarios

`BulkInsert` has many configurable options:

*   **BatchSize:** number of records to be inserted in a single database round-trip.
*   **InsertIfNotExists:** inserts only new entities that aren't already in the database.
*   **InsertKeepIdentity:** allows inserting specific values into an identity column.
*   **PrimaryKeyExpression:** lets you customize which key is used to check if an entity already exists

EF Extensions allow you to choose which properties should be used in insertion:

*   **ColumnInputExpression:** select which properties to map to the database.
*   **IgnoreOnInsertExpression:** select with properties to ignore, that should be auto-mapped.

```csharp
context.BulkInsert(products,
    options => options.ColumnInputExpression = c => new { c.Name, c.Description, c.Price } );
            
context.BulkInsert(products,
    options => options.IgnoreOnInsertExpression = c => new { c.ColumnToIgnore } );
```

*   **PrimaryKeyExpression:** customize which key to use to check for existing entities.

```csharp
context.BulkInsert(products, options => {
    options.InsertIfNotExists = true;
    options.ColumnPrimaryKeyExpression = c => c.Name;
});
```

By default, `BulkInsert` is an immediate operation. That means it's executed as soon as you call the method.

If you need to chain few bulk methods and execute them later - you can use a [FutureAction](https://entityframework-extensions.net/bulk-insert#insert-with-future-action?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) method.

To execute all pending FutureActions - call the `ExecuteFutureAction` method:

```csharp
// Generate data
var users = GenerateUsers(100);
var products = GenerateProducts(200);

// Queue actions for future execution
dbContext.FutureAction(x => x.BulkInsert(users));
dbContext.FutureAction(x => x.BulkInsert(products));

// Execute all queued actions
dbContext.ExecuteFutureAction();
```

For more scenarios you can learn more [here](https://entityframework-extensions.net/bulk-insert#documentation?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025).

## EF Core Extensions better than SqlBulkCopy?

You might be wondering, "Why not just using `SqlBulkCopy` library?" Indeed, raw `SqlBulkCopy` can be blazing fast, but it works only on SQL Server.

On the other hand, EF Core Extensions have the following benefits when compared with `SqlBulkCopy`:

*   **Supports Multiple Providers:** EF Extensions don't lock you into SQL Server; it works with various database providers.
*   **Not Just Inserts:** EF Extensions offer a full suite of methods, including BulkInsert, BulkUpdate, BulkDelete, BulkMerge, BulkSynchronize, WhereBulkContains, and more.
*   **Easy Identity Retrieval:** Need to get identity values back without extra hacks? EFE handles that seamlessly, while SqlBulkCopy alone doesn't automatically handle returning generated IDs.
*   **Child Entity Management:** EF Extensions automatically handle complex object graphs of child entities.
*   **Well-Tested & Configurable:** EF Extensions provides hundreds of well-tested options, ensuring advanced scenarios can be tackled with minimal custom code.

**EF Bulk Extensions support the following database providers:**

*   SQL Server
*   MySQL
*   MariaDB
*   Oracle
*   PostgreSQL
*   SQLite

**EF Core Extensions support the following:**

*   All Entity Framework Core Versions from EF Core 2 to EF Core 9
*   All Inheritances (TPC, TPH, TPT)
*   Complex Type/Owned Entity Type
*   Enums
*   EF Core Value Converters

Let's explore the other EF Core extension methods with examples.

## Using BulkUpdate, BulkDelete, BulkMerge, WhereBulkContains methods

### BulkUpdate

If you need to update thousands of records at once (e.g., applying a global price increase), [BulkUpdate](https://entityframework-extensions.net/bulk-update?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) can send all changes to the database in a single round-trip:

```csharp
// First get existing products to update
var existingProducts = await dbContext.Products.Take(100).ToListAsync();
if (!existingProducts.Any())
{
    return;
}

// Update properties with Bogus
var faker = new Faker();
foreach (var product in existingProducts)
{
    product.Name = faker.Commerce.ProductName();
    product.Description = faker.Commerce.ProductDescription();
    product.Price = decimal.Parse(faker.Commerce.Price());
}

// Perform bulk update
await dbContext.BulkUpdateAsync(existingProducts);
```

### BulkDelete

You want to delete a large subset of records — for instance, cleaning up historical records that are over five years old.

The default EF Core approach would require loading entities into memory, marking them as deleted, and calling `SaveChanges()`, which is highly inefficient at scale. [BulkDelete](https://entityframework-extensions.net/bulk-delete?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) can execute the deletion in a single round-trip:

```csharp
// Generate IDs to delete
var productsToDelete = GetProducts();

if (!productsToDelete.Any())
{
    return;
}

// Perform bulk delete
await dbContext.BulkDeleteAsync(productsToDelete);
```

This method is memory-friendly: you don't need to track or load each entity.

### BulkMerge

[BulkMerge](https://entityframework-extensions.net/bulk-merge?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) performs an upsert: inserting new records and updating existing ones based on a key (like a primary key or an alternate key):

```csharp
// Get some existing products for updating
var existingProducts = await dbContext.Products
    .Take(50)
    .ToListAsync();

var productsToMerge = UpdateProducts(existingProducts);

var newProducts = GenerateProducts(50);

productsToMerge.AddRange(newProducts);

// Perform bulk merge
await dbContext.BulkMergeAsync(productsToMerge);
```

This method replaces multi-step queries (select, insert, update) with a single merge command.

### BulkSynchronize

The [BulkSynchronize](https://entityframework-extensions.net/bulk-synchronize?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) method allows you to synchronize a large number of entities between your data source and the database.

This operation involves multiple steps:

*   Update rows that match the entity key
    
*   Insert rows that exist in the source but not in the database
    
*   Delete rows that exist in the database but not in the source
    
*   It's an all-in-one solution if you need a reliable way to ensure your database is always in sync with a given set of data.
    

```csharp
var existingProducts = await dbContext.Products
    .Take(50)
    .ToListAsync();

var productsToSync = UpdateProducts(existingProducts);

var newProducts = GenerateProducts(50);

// Perform bulk synchronize (will insert new, update existing, delete missing)
await dbContext.BulkSynchronizeAsync(productsToSync);
```

### WhereBulkContains

Scenario: You need to filter or query rows based on a large list of values — say you have a thousand product IDs in memory and want to retrieve only matching rows from the database.

Normally, you might do something like `.Where(x => idList.Contains(x.ProductId))`. But if `idList` is huge, EF Core tries to build a large `IN` statement, which can be problematic in both query performance and parameter limits. That's where [WhereBulkContains](https://entityframework-extensions.net/where-bulk-contains?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) comes in:

```csharp
var productIds = new List<int>();
for (var i = 1; i <= 10_000; i++)
{
    productIds.Add(i);
}

// Get all products by IDs
var foundProducts = await dbContext.Products
    .WhereBulkContains(productIds, p => p.Id)
    .ToListAsync();
```

This method bypasses parameter count limits in massive `IN()` queries.

## Summary

Entity Framework Extensions transform how you handle large datasets in EF Core. It boosts performance, cuts memory usage, and offers a vast range of bulk operations.

Whether you're doing simple inserts, complex object-graph inserts, updates, deletions, or merges, EFE makes the process faster and more reliable.

While `SqlBulkCopy` has its place for SQL Server, EFE provides multi-provider support and robust flexibility for more advanced scenarios.

After trying EF Core Extensions different bulk methods in a real project, I feel like using it as a Swiss Army knife. I was able to implement all bulk operations within a day or two, without spending weeks on custom implementations with possible bugs.

If you often work with big data in .NET, give [EF Extensions](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) a try — it may become your go-to choice for bulk operations in EF Core.

Disclaimer: this newsletter is sponsored by ZZZ Projects.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/ef-core-bulk-insert-boost-your-performance-with-entity-framework-extensions)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fef-core-bulk-insert-boost-your-performance-with-entity-framework-extensions&title=EF%20Core%20Bulk%20Insert%3A%20Boost%20Your%20Performance%20With%20Entity%20Framework%20Extensions)[X](https://twitter.com/intent/tweet?text=EF%20Core%20Bulk%20Insert%3A%20Boost%20Your%20Performance%20With%20Entity%20Framework%20Extensions&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fef-core-bulk-insert-boost-your-performance-with-entity-framework-extensions)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fef-core-bulk-insert-boost-your-performance-with-entity-framework-extensions)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.