```yaml
---
title: Efficient Bulk Updates in Entity Framework
source: https://okyrylchuk.dev/blog/efficient-bulk-updates-in-entity-framework/
date_published: 2024-05-10T19:36:13.000Z
date_captured: 2025-08-20T21:13:06.692Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [Entity Framework, Entity Framework 7, Entity Framework 8, .NET, SQL Server]
programming_languages: [C#, SQL]
tags: [entity-framework, bulk-operations, performance, database, dotnet, orm, data-access, sql, linq, update]
key_concepts: [bulk-updates, bulk-deletes, change-tracking, database-transactions, owned-types, linq-to-entities, concurrency-control, performance-optimization]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces Entity Framework 7's new `ExecuteUpdate` and `ExecuteDelete` methods for efficient bulk operations. It explains how these methods bypass EF's change tracker, sending operations directly to the database via LINQ queries, significantly improving performance for large datasets. The post details usage examples for both methods, including the generated SQL. It also covers important limitations, such as the lack of implicit transactions, no change synchronization with the DB context, and no concurrency control. Finally, it highlights how Entity Framework 8 addresses the use of these methods with owned types, allowing bulk operations on entities with complex types.]
---
```

# Efficient Bulk Updates in Entity Framework

# Efficient Bulk Updates in Entity Framework

![Oleg Kyrylchuk's avatar](https://okyrylchuk.dev/wp-content/litespeed/avatar/a85a89199a7d4486a29f67dc2fc371d0.jpg?ver=1755571532)

By default, the Entity Framework tracks the changes for loaded entities. When you call the SaveChanges method, EF sends the changes to the database.

The Entity Framework is designed to be highly efficient for most use cases. It only sends updates for changed properties or relationships, and it intelligently batches updates to reduce database round trips. All changes are seamlessly synchronized with the DB context and database.

But what if you want to perform a lot of updates or deletes? You need to load all entities from the database, change them, and save changes. It can produce performance issues for a lot of data.

In this post, you’ll learn how to do bulk updates efficiently and their limitations.

## New Bulk Updates Methods

Entity Framework 7 introduced two new methods, ExecuteUpdate and ExecuteDelete.

They perform updates and deletes without involving the changes tracker. You can use them on the LINQ query. They send the operation to the database immediately without calling the SaveChanges method.

Thus, you can perform an update operation in a single query without loading entities into the memory.

Let’s look at the usage examples given a naive implementation of the Product class.

```csharp
public class Product
{
    public long Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public int StockQuantity { get; set; }
}
```

### ExecuteDelete

You can see below how simply you can use the ExecuteDelete method.

```csharp
await context.Products
    .Where(p => p.StockQuantity == 0)
    .ExecuteDeleteAsync();
```

This code will generate the following SQL.

```sql
DELETE FROM[p]
    FROM [Products] AS[p]
    WHERE[p].[StockQuantity] = 0
```

### ExecuteUpdate

The usage of ExecuteUpdate is similar.

```csharp
await context.Products
    .Where(p => p.Category == "iPhone")
    .ExecuteUpdateAsync(
        p => p.SetProperty(x => x.StockQuantity, 0));
```

And SQL is.

```sql
UPDATE[p]
    SET[p].[StockQuantity] = 0
    FROM[Products] AS[p]
    WHERE[p].[Category] = N'iPhone'
```

## Limitations

As the usage of ExecuteDelete and ExecuteUpdate are simple, bear their limitations in mind.

The several method executions can not be batched. Every call will send a separate operation to the database.

Entity Framework doesn’t create implicit transactions for these methods as SaveChanges does. So, if you have several bulk updates and want to perform them within one transaction, you must begin it explicitly.

```csharp
using var transaction =
    await context.Database.BeginTransactionAsync();

// Execute ExecuteDelete or ExecuteUpdate

// Another ExecuteDelete or ExecuteUpdate

await transaction.CommitAsync();
```

After performing ExecuteDelete or ExecuteUpdate, the Entity Framework doesn’t synchronize the changes. Thus, it’s not recommended to mix them with SaveChanges calls.

The SaveChanges provides concurrency control. Since ExecuteDelete and ExecuteUpdate don’t involve changes tracker, they don’t support concurrency control. But they return a number of affected rows.

The methods target only one table. You cannot delete or update relationships. So if you want to delete from two related tables, you have to delete dependents first, then principal.

Only relational providers support these methods.

## Owned Types

From the beginning, the ExecuteUpdate and ExecuteDelete methods couldn’t target multiple entities, even if it’s one table. For instance, when an entity has owned types.

Entity Framework 8 fixes this issue.

Let’s provide the owned type of PriceInfo to the Product entity.

```csharp
public class Product
{
    public long Id { get; set; }
    public string Name { get; set; }
    public PriceInfo PriceInfo { get; set; }
    public string Category { get; set; }
    public int StockQuantity { get; set; }
}

public class PriceInfo
{
    public decimal Price { get; set; }
    public string Currency { get; set; }
}
```

And configure the entity.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
 {
     modelBuilder
         .Entity<Product>()
         .OwnsOne(p => p.PriceInfo);
 }
```

The following query will generate such SQL.

```csharp
await context.Products
    .Where(p => p.Category == "iPhone 15")
    .ExecuteUpdateAsync(
        p => p.SetProperty(x => x.PriceInfo.Price, 799)
              .SetProperty(x => x.StockQuantity, 1000));
```

SQL:

```sql
UPDATE Products AS p
    SET p.PriceInfo_Price = 799.0
    SELECT 1
    FROM Products AS p
    WHERE p.Category == N'iPhone 15'
```

## Summary

The new Bulk update methods can significantly improve the performance of update operations in EF.

However, it’s essential to know their limitations to use them efficiently.