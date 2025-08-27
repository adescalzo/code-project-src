```yaml
---
title: A Correct Way to Use Batchupdate and Batchdelete Methods in EF Core
source: https://antondevtips.com/blog/correct-way-to-use-batch-update-and-batch-delete-methods-in-ef-core
date_published: 2025-04-08T07:45:12.443Z
date_captured: 2025-08-06T16:45:25.825Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [EF Core, .NET, ASP.NET Core, SQL Server, PostgreSQL, EntityFramework-Extensions.net, dotConnect, Entity Developer]
programming_languages: [C#, SQL]
tags: [ef-core, batch-operations, performance, database, data-access, dotnet, orm, transactions, web-api, csharp]
key_concepts: [batch-operations, change-tracking, database-transactions, performance-optimization, data-consistency, orm, executeupdate, executedelete]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details the correct usage of `ExecuteUpdate` and `ExecuteDelete` methods in EF Core to enhance performance for mass updates and deletions. It contrasts the traditional Change Tracker approach, which generates individual SQL commands, with these newer batch operations that execute a single, efficient SQL query directly against the database. The content highlights the significant performance benefits of bypassing the Change Tracker but crucially warns about potential data consistency issues. To mitigate these risks, the article emphasizes the necessity of wrapping `ExecuteUpdate` and `ExecuteDelete` calls within explicit database transactions, especially when combined with other database operations, to ensure atomicity and data integrity. Code examples in C# and SQL illustrate both the problem and the transaction-based solution.]
---
```

# A Correct Way to Use Batchupdate and Batchdelete Methods in EF Core

![Cover image for the article titled 'A Correct Way to Use BatchUpdate and BatchDelete Methods in EF Core', featuring a 'dev tips' logo.](https://antondevtips.com/media/covers/efcore/cover_efcore_batch_operations.png)

# Correct Way to Use BatchUpdate and BatchDelete Methods in EF Core

Apr 8, 2025

[Download source code](/source-code/correct-way-to-use-batch-update-and-batch-delete-methods-in-ef-core)

4 min read

### Newsletter Sponsors

[EF Core is too slow?](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025) Discover how you can easily insert 14x faster (reducing saving time by 94%).  
Boost your performance with our method integrated within EF Core: Bulk Insert, update, delete, and merge.  
Join 5,000+ satisfied customers who have trusted our library since 2014.

[Learn more](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=april-2025)

Boost your EF Core performance with [dotConnect](https://www.devart.com/dotconnect/?utm_source=martyniuk&utm_medium=referral&utm_campaign=Q2), a powerful suite of high-performance data providers for databases and cloud services, offering optimized batch operations. Pair it with [Entity Developer](https://www.devart.com/entitydeveloper/?utm_source=martyniuk&utm_medium=referral&utm_campaign=Q2) for visual ORM modeling. Try a 30-day free trial now!

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Have you faced performance issues when performing mass updates or deletions in your EF Core applications?

EF Core offers efficient batch operations using `ExecuteUpdate` and `ExecuteDelete` methods, significantly enhancing performance. These operations allow updating and deleting multiple entities in a single SQL query without retrieving them from the database.

In this blog post I will show you how to correctly use `ExecuteUpdate` and `ExecuteDelete` methods in EF Core to ensure data consistency.

## Default Approach to Updating And Deleting Entities

First, let's explore how updating and deletion of entities works in EF Core.

The default approach involves loading entities into the [EF Core Change Tracker](https://antondevtips.com/blog/understanding-change-tracking-for-better-performance-in-ef-core?utm_source=antondevtips&utm_medium=website&utm_campaign=website) that holds them in memory.

This tracking mechanism is essential for EF Core to know which entities must be inserted, updated, or deleted in the database.

```csharp
var users = await dbContext.Users.ToListAsync();
```

After querying users from the database, all entities are automatically added to the Change Tracker. When updating the users - EF Core will compare the current users collection with the saved collection stored in Change Tracker. EF Core will use the comparison result to decide what SQL commands to generate to update entities in the database:

```sql
Executed DbCommand (0ms) [Parameters=[@p1='****', @p0='test@mail.com' (Nullable = false) (Size = 13)], CommandType='Text', CommandTimeout='30']
      UPDATE "users" SET "email" = @p0
      WHERE "id" = @p1
      RETURNING 1;
```

Let's explore an example of updating books' price for a given `Author`:

```csharp
public sealed record UpdateBooksPriceRequest(decimal Delta);

app.MapPut("/authors/{authorId:guid}/books/update-price",
    async (Guid authorId,
    UpdateBooksPriceRequest request,
    ApplicationDbContext dbContext) =>
{
    var books = await dbContext.Books.Where(b => b.AuthorId == authorId).ToListAsync();
    foreach (var book in books)
    {
        book.Price += request.Delta;
        book.UpdatedAtUtc = DateTime.UtcNow;
    }

    await dbContext.SaveChangesAsync();
    return Results.Ok(new { updated = books.Count });
});
```

This approach is straightforward: load entities from the database, update needed properties, and EF Core will figure out what SQL statements to generate to update entities:

```sql
UPDATE devtips_batch_operations.books SET price = @p0, updated_at_utc = @p1
      WHERE id = @p2;
      UPDATE devtips_batch_operations.books SET price = @p3, updated_at_utc = @p4
      WHERE id = @p5;
      UPDATE devtips_batch_operations.books SET price = @p6, updated_at_utc = @p7
      WHERE id = @p8;
```

Let's explore another example of deleting multiple books for a given author:

```csharp
app.MapDelete("/authors/{authorId:guid}/books",
    async (Guid authorId, ApplicationDbContext dbContext) =>
{
    var booksToDelete = await dbContext.Books
        .Where(b => b.AuthorId == authorId)
        .ToListAsync();

    if (booksToDelete.Count == 0)
    {
        return Results.NotFound("No books found for the given author.");
    }

    dbContext.Books.RemoveRange(booksToDelete);
    await dbContext.SaveChangesAsync();

    return Results.Ok(new { deletedCount = booksToDelete.Count });
});
```

This approach is straightforward: load entities from the database, call `RemoveRange` method, and EF Core will figure out what SQL statements to generate to delete entities:

```sql
DELETE FROM devtips_batch_operations.books
  WHERE id = @p0;
  DELETE FROM devtips_batch_operations.books
  WHERE id = @p1;
  DELETE FROM devtips_batch_operations.books
  WHERE id = @p2;
```

As you can see, both operations generate individual SQL commands for each updated and deleted entity, which can be inefficient. While simple and effective for small datasets, this approach can be inefficient for medium and large numbers of records.

Let's explore a more efficient solution.

## Using ExecuteUpdate and ExecuteDelete Methods

EF Core 7 introduced `ExecuteUpdate` and `ExecuteDelete` methods for batch operations. These methods bypath the Change Tracker and allow to perform updates and deletions directly in the database with a single SQL statement.

These methods have the following advantages:

*   Remove the overhead of loading entities from the database into ChangeTracker
*   Update and delete operations are executed as a single SQL command, making such queries very efficient

Let's explore how we can rewrite the previous examples using these methods.

This is how you can update books' price with `ExecuteUpdate`:

```csharp
app.MapPut("/authors/{authorId:guid}/books/batch-update-price",
    async (Guid authorId,
        UpdateBooksPriceRequest request,
        ApplicationDbContext dbContext) =>
{
    var updatedCount = await dbContext.Books
        .Where(b => b.AuthorId == authorId)
        .ExecuteUpdateAsync(s => s
            .SetProperty(b => b.Price, u => u.Price + request.Delta)
            .SetProperty(b => b.UpdatedAtUtc, DateTime.UtcNow));

    return Results.Ok(new { updated = updatedCount });
});
```

First we filter books by a given `Author` identifier and update the needed properties by calling a `SetProperty` method.

This generates a single SQL command:

```sql
UPDATE devtips_batch_operations.books AS b
SET updated_at_utc = now(),
  price = b.price + @__request_Delta_1
WHERE b.author_id = @__authorId_0
```

Let's explore a Book deletion example:

```csharp
app.MapDelete("/authors/{authorId:guid}/books/batch",
    async (Guid authorId, ApplicationDbContext context) =>
{
    var deletedCount = await dbContext.Books
        .Where(b => b.AuthorId == authorId)
        .ExecuteDeleteAsync();

    return Results.Ok(new { deleted = deletedCount });
});
```

This also generates a single SQL command:

```sql
DELETE FROM devtips_batch_operations.books AS b
  WHERE b.author_id = @__authorId_0
```

These methods are significantly more efficient for larger modifications.

These methods can be beneficial even when updating or deleting a single entity. You execute a single SQL command instead of two separate operations (loading and then updating or deleting). And if you have multiple entities, you need to send 1 + N requests to the database.

This can slow your application significantly.

But keep in mind that `ExecuteUpdate` and `ExecuteDelete` methods have one **major caveat**. They are detached from EF Core's Change Tracker. If you call `SaveChanges` afterward, and it fails, changes made via `ExecuteUpdate` and `ExecuteDelete` **won't be reverted**.

Let's explore how to fix this problem!

## How to Ensure Data Consistency with ExecuteUpdate and ExecuteDelete Methods

You need to ensure that data is consistent when executing multiple batch operations, or executing a batch operation together with SaveChanges.

You need to wrap all database commands manually in a **transaction**. Let's explore an example:

```csharp
app.MapPut("/authors/{authorId:guid}/books/multi-update", 
    async(Guid authorId,
        UpdateBooksPriceRequest request,
        ApplicationDbContext dbContext) =>
{
    await using var transaction = await dbContext.Database.BeginTransactionAsync();

    try
    {
        var authorBooks = await dbContext.Books
            .Where(b => b.AuthorId == authorId)
            .Select(x => new  { x.Id, x.Price })
            .ToListAsync();

        var updatedCount = await dbContext.Books
            .Where(b => b.AuthorId == authorId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.Price, u => u.Price + request.Delta)
                .SetProperty(b => b.UpdatedAtUtc, DateTime.UtcNow));

        await dbContext.Authors
            .Where(b => b.Id == authorId)
            .ExecuteUpdateAsync(s => s
                .SetProperty(b => b.UpdatedAtUtc, DateTime.UtcNow));

        var priceRecords = authorBooks.Select(x => new PriceRecord
        {
            Id = Guid.NewGuid(),
            BookId = x.Id,
            OldPrice = x.Price,
            NewPrice = x.Price + request.Delta,
            CreatedAtUtc = DateTime.UtcNow
        }).ToList();

        dbContext.PriceRecords.AddRange(priceRecords);

        await dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        return Results.Ok(new { updated = updatedCount });
    }
    catch (Exception)
    {
        await transaction.RollbackAsync();
        return Results.BadRequest("Error updating books");
    }
});
```

In this API endpoint, there are 3 update operations:

1.  Updating `Book` Prices
2.  Updating `Author` row timestamp
3.  Creating Price Change Records

Wrapping these operations in a transaction ensures that either all operations succeed or none do, thus maintaining database integrity.

## Summary

`ExecuteUpdate` and `ExecuteDelete` methods significantly boost EF Core performance for batch operations. However, to avoid data consistency issues, always wrap these methods within manual transactions if you combine them with other operations. This ensures robust, fast and consistent database state management.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/correct-way-to-use-batch-update-and-batch-delete-methods-in-ef-core)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fcorrect-way-to-use-batch-update-and-batch-delete-methods-in-ef-core&title=Correct%20Way%20to%20Use%20BatchUpdate%20and%20BatchDelete%20Methods%20in%20EF%20Core)[X](https://twitter.com/intent/tweet?text=Correct%20Way%20to%20Use%20BatchUpdate%20and%20BatchDelete%20Methods%20in%20EF%20Core&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fcorrect-way-to-use-batch-update-and-batch-delete-methods%20in%20ef-core)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fcorrect-way-to-use-batch-update-and-batch-delete-methods-in-ef-core)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.