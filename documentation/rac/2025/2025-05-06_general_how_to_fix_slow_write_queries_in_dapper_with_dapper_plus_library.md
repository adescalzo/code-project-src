```yaml
---
title: How To Fix Slow Write Queries in Dapper With Dapper Plus Library
source: https://antondevtips.com/blog/how-to-fix-slow-write-queries-in-dapper-with-dapper-plus-library
date_published: 2025-05-06T07:45:31.290Z
date_captured: 2025-08-06T16:42:42.649Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [Dapper, Dapper Plus, .NET, ASP.NET Core, SQL Server, Azure SQL Server, PostgreSQL, MySQL, Oracle, MariaDB, SQLite, NuGet, Entity Framework Core, Entity Framework Extensions]
programming_languages: [C#, SQL, Bash]
tags: [dapper, dapper-plus, .net, csharp, database, performance, bulk-operations, orm, data-access, sql]
key_concepts: [bulk-insert, bulk-update, bulk-delete, bulk-merge, bulk-synchronize, micro-orm, query-optimization, database-performance, batching, database-round-trips]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article addresses Dapper's performance limitations for bulk write operations, contrasting it with its fast read capabilities. It introduces Dapper Plus, a library designed to significantly improve the speed of bulk inserts, updates, deletes, merges, and synchronizations. Through code examples and performance benchmarks, the author demonstrates how Dapper Plus drastically reduces database round-trips and latency, making write operations up to 75-150 times faster than traditional Dapper methods. The article also covers configuration options, supported databases, and advanced features like method chaining, highlighting Dapper Plus as a crucial tool for optimizing .NET application database performance.]
---
```

# How To Fix Slow Write Queries in Dapper With Dapper Plus Library

![Cover image for the article "How To Fix Slow Write Queries in Dapper With Dapper Plus Library". The image features a dark blue background with purple abstract shapes. On the left, there's a white square icon with a `</>` symbol, next to the text "dev tips". The main title in white text reads "HOW TO FIX SLOW WRITE QUERIES IN DAPPER WITH DAPPER PLUS LIBRARY".](https://antondevtips.com/media/covers/dotnet/cover_dotnet_dapper_plus.png)

# How To Fix Slow Write Queries in Dapper With Dapper Plus Library

May 6, 2025

[Download source code](/source-code/how-to-fix-slow-write-queries-in-dapper-with-dapper-plus-library)

6 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Dapper is known for blazing-fast reads, making it a popular micro-ORM choice for retrieving data in .NET applications. Dapper gives you full control over SQL queries you send to the database.

That's why it's often a preferred ORM over EF Core for whole projects or at least for read operations.

However, when it comes to inserting or updating large batches of records, Dapper can become surprisingly slow, as it doesn't provide built-in optimized bulk operations. It's much slower than EF Core when writing multiple rows.

Today I want to introduce you to a [Dapper Plus](https://dapper-plus.net) library that adds high-performance bulk operation capabilities to Dapper. It allows making ultra-fast database write queries with minimal effort.

In this post you'll learn about the following options [Dapper Plus](https://dapper-plus.net) provides:

*   Bulk Insert
*   Bulk Update
*   Bulk Delete
*   Bulk Merge
*   Bulk Synchronize

Let's dive in.

## How Dapper Performs Writes

Let's explore how you can insert multiple rows in Dapper:

```csharp
using var connection = new SqlConnection(connectionString)
connection.Open();

var orders = GenerateOrders(1000);

foreach (var order in orders)
{
    var sqlQuery = "INSERT INTO Orders (Number, Price) VALUES (@Number, @Price)";
    connection.Execute(sqlQuery, order);
}
```

There is a better option to provide a list for Dapper, and it will automatically populate the script with all the values:

```csharp
var sqlQuery = "INSERT INTO Orders (Number, Price) VALUES (@Number, @Price)";        
connection.Execute(sqlQuery, orders);
```

However, both options will result in sending multiple `INSERT` commands to the database one-by-one. This significantly increases database round-trips and latency.

Here is where **Dapper Plus** comes in.

## Bulk Inserts with Dapper Plus

To get started with **Dapper Plus** install the following Nuget package:

```bash
dotnet add package Z.Dapper.Plus
```

**Dapper Plus** allows you to bulk insert thousands of entities with a single line of code:

```csharp
using Z.Dapper.Plus;

using var connection = new SqlConnection(connectionString)
connection.Open();

var products = GenerateProducts(10_000);
await connection.BulkInsertAsync(products);
```

Dapper Plus dramatically reduces round-trips by batching multiple inserts into a single database call, improving performance significantly.

Both [BulkInsert](https://dapper-plus.net/bulk-insert) and [BulkInsertAsync](https://dapper-plus.net/bulk-insert) methods are available.

`BulkInsert` has many configurable options:

*   **BatchSize:** number of records to be inserted in a single database round-trip.
*   **DestinationSchemaName:** name of the database schema.
*   **InsertIfNotExists:** inserts only new entities that aren't already in the database.
*   **InsertKeepIdentity:** allows inserting specific values into an identity column.
*   **PrimaryKeyExpression:** lets you customize which key is used to check if an entity already exists

**Dapper Plus** allows you to choose which properties should be used in insertion:

*   **ColumnInputExpression:** select which properties to map to the database.
*   **IgnoreOnInsertExpression:** select with properties to ignore, that should be auto-mapped.

```csharp
DapperPlusManager.Entity<Product>()
    .Table("products")
    .Identity(x => x.Id)
    .UseBulkOptions(options =>
    {
        options.DestinationSchemaName = DatabaseConsts.Schema;
        options.ColumnInputExpression = c => new { c.Name, c.Description, c.Price };
        
        // You can either use "ColumnInputExpression" or "IgnoreOnInsertExpression".
        // You choose what to map or what to ignore
        //options.IgnoreOnInsertExpression = c => new { c.ColumnToIgnore };
    });

connection.BulkInsert(products);
```

*   **PrimaryKeyExpression:** customize which key to use to check for existing entities.

```csharp
DapperPlusManager.Entity<Product>()
    .Table("products")
    .Identity(x => x.Id)
    .UseBulkOptions(options =>
    {
        options.DestinationSchemaName = DatabaseConsts.Schema;
        options.InsertIfNotExists = true;
        options.ColumnPrimaryKeyExpression = c => c.Name;
    });

connection.BulkInsert(products);
```

For more scenarios you can learn more [here](https://dapper-plus.net/options).

**Dapper Plus supports the following database providers:**

*   Microsoft SQL Server
*   Azure SQL Server
*   PostgreSQL
*   MySQL
*   Oracle
*   MariaDB
*   SQLite

## How Fast is Dapper Plus?

I have examined the benchmarks of Dapper Plus from the official website. The results are quite impressive or even shocking when compared to pure Dapper:

*   **Insert:** 75x faster, reducing time by 93% [Online Benchmark](https://dotnetfiddle.net/zlTePU)
*   **Update:** 50x faster, reducing time by 75% [Online Benchmark](https://dotnetfiddle.net/qnbq6o)
*   **Delete:** 150x faster, reducing time by 65% [Online Benchmark](https://dotnetfiddle.net/18paED)
*   **Merge:** 50x faster, reducing time by 98% [Online Benchmark](https://dotnetfiddle.net/piaZmp)

The `BulkInsert` method allows you to reduce saving times by up to **99%** for SQL Server when saving a large number of entities. Even for just 50 rows, it can reduce saving times by **88%**, which can dramatically improve the user experience you provide to your clients.

So I decided to run my own performance tests. I have created the following Web API endpoints:

```csharp
app.MapPost("/products/dapper-insert", async (IConnectionFactory connectionFactory) =>
{
    var products = GenerateProducts(10_000);
    
    using var connection = connectionFactory.CreateConnection();
    
    const string sql = @"
        INSERT INTO products.products (name, description, price)
        VALUES (@Name, @Description, @Price)";
            
    await connection.ExecuteAsync(sql, products);
        
    return Results.Ok("10,000 products inserted using Dapper Execute.");
});

app.MapPost("/products/dapper-bulk-insert", async (IConnectionFactory connectionFactory) =>
{
    var products = GenerateProducts(10_000);
    
    // Configure DapperPlusManager for entity mapping
    DapperPlusManager.Entity<Product>()
        .Table("products")
        .Identity(x => x.Id)
        .UseBulkOptions(options =>
        {
            options.DestinationSchemaName = DatabaseConsts.Schema;
        });
        
    using var connection = connectionFactory.CreateConnection();
    
    await connection.BulkInsertAsync(products);
    
    return Results.Ok("10,000 products inserted using Dapper Plus Bulk Insert.");
});
```

I have tested these queries on a Postgres database, and here are the results for inserting 10\_000 products via Web API requests:

*   Dapper - 18.41 seconds
*   BulkInsert - 532 ms

Yes, with Dapper Plus I was able to insert 10K records in just half a second.

Try these online benchmarks on .NET Fiddle and see the performance difference for yourself.

> Note: benchmarks can vary depending on your hardware and database provider.

Now let's explore what other bulk write methods Dapper Plus offers.

## BulkUpdate

[BulkUpdate](https://dapper-plus.net/bulk-update) provides a very efficient way to update many records in the database. For example, you may need to apply a global price increase for thousands of products at once.

`BulkUpdate` sends all changes to the database in a single round-trip:

```csharp
// First get existing products to update
const string selectSql = "SELECT * FROM products.products LIMIT 100";
var existingProducts = (await connection.QueryAsync<Product>(selectSql)).ToList();

// Update properties with Bogus
var faker = new Faker();
foreach (var product in existingProducts)
{
    product.Name = faker.Commerce.ProductName();
    product.Description = faker.Commerce.ProductDescription();
    product.Price = decimal.Parse(faker.Commerce.Price());
}

// Perform bulk update
await connection.BulkUpdateAsync(existingProducts);
```

### BulkDelete

You want to delete a large subset of records — for instance, cleaning up historical records that are over five years old.

Here are the traditional ways of deleting many records in the database with Dapper:

```csharp
connection.Execute(@"DELETE Product WHERE ProductID = @ProductID", products);

connection.Execute(@"DELETE Product WHERE ProductID IN @ProductIDs", new { ProductIDs = products.Select(x => x.ProductID).ToList() });
```

`Where IN` Parameter solution is very fast for simple cases. This method is effective if you do not use a surrogate key with a small number of entities.

However, if you use a surrogate key or multiple entities - [BulkDelete](https://dapper-plus.net/bulk-delete) method can reduce deletion times by up to 99% for SQL Server when deleting a large number of entities.

`BulkDelete` provides a very efficient way to delete many records from the database:

```csharp
// Get products to delete
var productsToDelete = GetProducts();

// Perform bulk delete
await connection.BulkDeleteAsync(productsToDelete);
```

### BulkMerge

[BulkMerge](https://dapper-plus.net/bulk-merge) performs an upsert: inserting new records and updating existing ones based on a key (like a primary key or an alternate key):

```csharp
// Get some existing products for updating
const string selectIdsSql = "SELECT id FROM products.products LIMIT 50";
var existingIds = (await connection.QueryAsync<int>(selectIdsSql)).ToList();

var productsToMerge = UpdateProducts(existingProducts);

var newProducts = GenerateProducts(50);

productsToMerge.AddRange(newProducts);

// Perform bulk merge
await connection.BulkMergeAsync(productsToMerge);
```

This method combines insert and update with a single merge command.

### BulkSynchronize

The [BulkSynchronize](https://dapper-plus.net/bulk-synchronize) method allows you to synchronize a large number of entities between your data source and the database.

This operation involves multiple steps:

*   Update rows that match the entity key
    
*   Insert rows that exist in the source but not in the database
    
*   Delete rows that exist in the database but not in the source
    
*   It's an all-in-one solution if you need a reliable way to ensure your database is always in sync with a given set of data.
    

```csharp
const string selectIdsSql = "SELECT id FROM products.products LIMIT 100";
var currentIds = (await connection.QueryAsync<int>(selectIdsSql)).ToList();

var productsToSync = UpdateProducts(existingProducts);

var newProducts = GenerateProducts(50);

productsToSync.AddRange(newProducts);

// Perform bulk synchronize (will insert new, update existing, delete missing)
await connection.BulkSynchronizeAsync(productsToSync);
```

## Chaining Methods in Dapper Plus

Dapper Plus allows you to chain few bulk operations together.

There are 4 types of chaining methods:

*   **AlsoBulk[Action]:** Performs additional bulk actions at the same hierarchy level without moving deeper.
*   **ThenBulk[Action]:** Performs additional bulk actions at a deeper hierarchy level, such as child or grandchild entities.
*   **Include:** Groups a set of bulk actions together, letting you specify different chained operations at once.
*   **ThenForEach:** Perform an action for each entity, such as propagating the identity value.

For example:

```csharp
connection.BulkMerge("KeepIdentity", newCustomerOrders)
  .AlsoBulkInsert(customer => customer.Orders)
  .AlsoBulkInsert(customer => customer.Orders.SelectMany(y => y.Items));

connection.BulkMerge("KeepIdentity", newCustomerOrders)
  .Include(c => c.ThenBulkInsert(customer => customer.Orders)
        .AlsoBulkInsert(order => order.Items))
  .Include(c => c.ThenBulkInsert(customer => customer.Invoices)
        .AlsoBulkInsert(invoice => invoice.Items));
```

Here is how the second example works:

1.  Merges `newCustomerOrders` into the database while preserving identity columns.
2.  Inside each `Include` block, it uses `ThenBulkInsert` to insert child entities (like Orders or Invoices).
3.  `AlsoBulkInsert` adds related items (Items in Orders or Invoices) at the same hierarchy level.

By doing so, you can perform multiple coordinated inserts in a single, chained operation.  
It ensures that both parent records and their nested child records are consistently inserted together.

## Free Single Method

If you're not ready to adopt the full capabilities of Dapper Plus, you can still benefit from its power with [single-method](https://dapper-plus.net/single-extensions-methods) extensions provided by the library for free!

These extensions give you the same customization options but without the bulk performance enhancements:

```csharp
connection.Insert(product);
connection.Update(product);
connection.Delete(product);
connection.Merge(product);
```

For example:

```csharp
connection.SingleInsert(products[0]);

connection.UseBulkOptions(options => options.InsertIfNotExists = true)
    .BulkInsert(products[1]);
```

You can easily upgrade later to the full bulk methods when needed.

You test these methods in [dotnetfiddle](https://dotnetfiddle.net/pwcR2q).

## Relation between Dapper and Dapper Plus

**Dapper Plus** doesn't depend on Dapper and can run independently, avoiding unnecessary complexity, especially as it shares core code with another fantastic library, [Entity Framework Extensions](https://entityframework-extensions.net).

However, **Dapper Plus** is actively sponsoring Dapper and its ecosystem, following Dapper's mindset, and maintaining widely-used Dapper tutorials.

Ultimately, Dapper Plus helps sustain Dapper's growth and enrich the entire .NET community.

You can find more information [here](https://www.learndapper.com/dapper-and-dapper-plus).

## Summary

Dapper excels in data retrieval but can struggle with bulk write operations. By integrating **Dapper Plus**, you dramatically enhance performance, turning slow inserts and updates into rapid-fire operations. Try out the powerful bulk operations from Dapper Plus that will give you blazing fast write operations.

Boost your application's database performance today with [Dapper Plus](https://dapper-plus.net).

Disclaimer: this newsletter is sponsored by ZZZ Projects.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-fix-slow-write-queries-in-dapper-with-dapper-plus-library)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-fix-slow-write-queries-in-dapper-with-dapper-plus-library&title=How%20To%20Fix%20Slow%20Write%20Queries%20in%20Dapper%20With%20Dapper%20Plus%20Library)[X](https://twitter.com/intent/tweet?text=How%20To%20Fix%20Slow%20Write%20Queries%20in%20Dapper%20With%20Dapper%20Plus%20Library&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-fix-slow-write-queries-in%2Fhow-to-fix-slow-write-queries-in-dapper-with-dapper-plus-library)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-fix-slow-write-queries-in-dapper-with-dapper-plus-library)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.