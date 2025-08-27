```yaml
---
title: Getting Started with Dapper in .NET
source: https://www.nikolatech.net/blogs/dapper-asp-dotnet-core
date_published: 2025-05-11T10:58:01.963Z
date_captured: 2025-08-06T18:19:29.643Z
domain: www.nikolatech.net
author: Unknown
category: general
technologies: [Dapper, .NET, Entity Framework, SQL Server, PostgreSQL, NuGet, ADO.NET, Dapper.Transaction, Npgsql, ASP.NET Core]
programming_languages: [C#, SQL, Shell]
tags: [micro-orm, database, dotnet, data-access, performance, sql-injection, transactions, stored-procedures, ado-net, nuget]
key_concepts: [micro-orm, query-to-object-mapping, dependency-injection, parameterized-queries, database-transactions, stored-procedures, idbconnection, raw-sql]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to getting started with Dapper, a lightweight micro ORM for .NET. It highlights Dapper's advantages over full-fledged ORMs like Entity Framework, emphasizing its performance and direct SQL control. The post covers essential topics such as installing Dapper, configuring database connections using `IDbConnection` and `Npgsql`, and performing basic CRUD operations with `ExecuteAsync` and `Query` methods. Furthermore, it delves into managing database transactions using `IDbTransaction` and `TransactionScope`, and executing stored procedures. The article also stresses the importance of parameterized queries for security.]
---
```

# Getting Started with Dapper in .NET

# In this article

*   [Intro](#Intro)
*   [What is Dapper](#WhatIsDapper)
*   [Getting Started](#GettingStarted)
*   [Configuring Connection](#ConfiguringConnection)
*   [Basic Usage](#BasicUsage)
*   [Transactions](#Transactions)
*   [Stored Procedures](#StoredProcedures)
*   [Conclusion](#Conclusion)

![Banner: A blue background with geometric shapes. In the top left corner is a white square with a stylized 'NK' logo. In the top right, a large white rectangular area contains the bold black text "DAPPER". Below this, centered on the blue background, is the white text "Fast & Simple Data Access in .NET". In the bottom left, there's an abstract, colorful logo with orange, pink, and light blue elements on a black background.](https://coekcx.github.io/BlogImages/banners/dapper-asp-dotnet-core-banner.png)

#### Getting Started with Dapper in .NET

###### 11 May 2025

###### 5 min

If you've been following me for a while, you know I’m a big fan of Entity Framework.

**Entity Framework** is a powerful, fast and reliable ORM, easily the best I’ve used across any language or framework.

But sometimes, we don’t need the full capabilities of a heavyweight ORM. Sometimes, all we need is simple and fast query to object mapping. That’s where micro ORMs like **Dapper** come in.

In today's blog post, we'll explore what Dapper is, what it offers and how you can get started with it.

## What is Dapper?

**Dapper** is micro ORM, which means it's only responsible for query to object mapping.

Unlike full-fledged ORMs like Entity Framework Core, Dapper focuses on performance and simplicity. It doesn’t generate SQL for you, instead it helps you map query results to your .NET objects quickly and efficiently.

This means that Dapper doesn't have overhead of translating LINQ into SQL queries and it also means that you have full control over SQL that will be sent.

It doesn't have change tracking and because you write all your own SQL queries, you don’t have to worry about the performance concerns that can come with LINQ parsing.

You can also run your stored procedures with Dapper easily.

While EF Core also supports raw SQL and stored procedures, Dapper stands out when you want lightweight data access without the complexity or overhead of a full ORM.

Without further ado, let’s begin our journey with Dapper.

## Getting Started

To get started with **Dapper**, you'll first need to install the necessary NuGet packages. You can do this via the NuGet Package Manager or by running the following command in the Package Manager Console:

```shell
dotnet add package Dapper
```

Dapper extends the **IDbConnection** interface, which is the base interface for database connections in **ADO.NET**.

This means you can easily integrate with any IDbConnection, using any database of your choice (SQL Server, Postgresql, etc..).

In this example I've used PostgreSql:

```shell
dotnet add package Npgsql
```

Once you install necessary nuget packages we need to configure connection to the database in our application.

## Configuring Connection

For better testability and easier dependency injection, it's a good practice to define an **ISqlConnectionFactory** interface responsible for establishing a connection:

```csharp
public interface ISqlConnectionFactory
{
    IDbConnection OpenConnection();
}
```

The implementation looks like this:

```csharp
public sealed class PostgresConnectionFactory(string connectionString) : ISqlConnectionFactory
{
    public IDbConnection OpenConnection()
    {
        var dbConnection = new NpgsqlConnection(connectionString);
        dbConnection.Open();
        return dbConnection;
    }
}
```

Once the interface is implemented, we can register it in dependency injection, passing the connection string:

```csharp
builder.Services.AddSingleton<ISqlConnectionFactory>(_ =>
    new PostgresConnectionFactory(builder.Configuration["Postgres"]!));
```

With everything setup, we can continue building the rest of the application.

## Basic Usage

Dapper provides a set of methods for performing various database operations.

### ExecuteAsync

**ExecuteAsync** is used for non-query commands such as **INSERT**, **UPDATE**, and **DELETE**. It returns an int representing the number of rows affected by the operation.

In the example below, we create a new Order object and pass it directly to ExecuteAsync. Dapper automatically maps the object's properties (Id, Status etc.) to the corresponding SQL parameters (@Id, @Status, etc.).

```csharp
private async Task<IResult> Handler(ISqlConnectionFactory connectionFactory)
{
    var order = new Order(OrderStatus.Created);
    using var connection = connectionFactory.OpenConnection();
    await connection.ExecuteAsync(
        """
        insert into "Orders" ("Id", "Status", "CreatedAt", "ModifiedAt")
        values (@Id, @Status, @CreatedAt, @ModifiedAt)
        """,
        order);

    return Results.Ok(order.Id);
}
```

In the case of an update, instead of fetching and then passing the entire Order object, I used an anonymous type. This also allows Dapper to map specified values to their corresponding SQL parameters.

```csharp
private async Task<IResult> Handler(ISqlConnectionFactory connectionFactory, Guid id, Request request)
{
    using var connection = connectionFactory.OpenConnection();

    var affectedRows = await connection.ExecuteAsync(
        """
        UPDATE "Orders" SET "Status" = @Status, "ModifiedAt" = @ModifiedAt WHERE "Id" = @Id
        """, new
        {
            request.Status,
            ModifiedAt = DateTime.UtcNow,
            Id = id
        });

    return affectedRows == 0
        ? Results.NotFound()
        : Results.NoContent();
}
```

Additionally, I used the method's response to determine whether an order with the specified id exists.

### Query

Dapper, offers a wide range of methods for selecting a single row, multiple rows or just some columns from a table.

We're not going to go through each one individually since there are many variations:

*   **Query/QueryAsync** - Returns an enumerable collection of elements.
*   **QuerySingle** - Returns exactly one row, throws an exception if the result is empty or contains more than one element.
*   **QuerySingleOrDefault** - Returns a single row or null if none is found, throws an exception if more than one row is returned.
*   **QueryFirst** - Returns the first row, throws an exception if no rows are returned.
*   **QueryFirstOrDefault** - Returns the first row or null if the result set is empty.

Query/QueryAsync is used to return an enumerable collection of elements:

```csharp
private async Task<IResult> Handler(ISqlConnectionFactory connectionFactory)
{
    using var connection = connectionFactory.OpenConnection();
    var orders = await connection.QueryAsync<Order>(
        "SELECT * FROM \"Orders\"");
    return Results.Ok(orders);
}
```

When you need to fetch a single row, be intentional about the method you choose and how you handle the result.

```csharp
private async Task<IResult> Handler(ISqlConnectionFactory connectionFactory, Guid id)
{
    using var connection = connectionFactory.OpenConnection();
    var order = await connection.QuerySingleOrDefaultAsync<Order>(
        "SELECT * FROM \"Orders\" WHERE \"Id\" = @Id",
        new { Id = id });
    return Results.Ok(order);
}
```

To return only specific fields all you need is to simply provide a comma-separated list of column names within the SQL statement for your query:

```csharp
private async Task<IResult> Handler(ISqlConnectionFactory connectionFactory)
{
    using var connection = connectionFactory.OpenConnection();
    var orders = await connection.QueryAsync<OrderResponse>(
    "SELECT Id, Status FROM \"Orders\"");
    return Results.Ok(orders);
}
```

Using generic methods like **Query<T>** is highly recommended, as Dapper automatically maps query results to objects of type T by matching column names with property names. This provides strong typing, improves readability, and helps catch errors at compile time.

While Dapper also supports returning dynamic results, which can be handy for quick or flexible queries, relying on generic methods is a better choice for producing predictable and maintainable code.

**NOTE:** Always use parameterized queries. Avoid inserting user input directly into SQL strings, as this can expose your application to SQL Injection attacks. Instead, use named parameters (e.g., @Username) in your SQL and pass values separately using an anonymous object or a matching class. Dapper will handle parameter binding automatically, making your queries secure by default.

## Transactions

Dapper provides different ways to work with transactions.

The most common way is to use the **BeginTransaction** method available from the **IDbConnection** interface. That will return an instance of **IDbTransaction**, which you can use with **Execute** and **Query** methods to add, remove and modify data in your database tables.

Once you have finished executing your commands within the transaction scope, you can either commit the transaction or roll back the changes.

Committing the transaction will save all your changes to the database while rolling back any changes will restore the data to its previous state before you started the transaction.

```csharp
private async Task<IResult> Handler(ISqlConnectionFactory connectionFactory)
{
    var order = new Order(OrderStatus.Created);

    using var connection = connectionFactory.OpenConnection();
    using var transaction = connection.BeginTransaction();
    try
    {
        await connection.ExecuteAsync(
            """
            insert into "Orders" ("Id", "Status", "CreatedAt", "ModifiedAt")
            values (@Id, @Status, @CreatedAt, @ModifiedAt)
            """,
            order,
            transaction: transaction);

        transaction.Commit();

        return Results.Ok(order.Id);
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}
```

Dapper also offers **Dapper.Transaction** library to extend **IDbTransaction** interface to use Dapper under the hood:

```shell
dotnet add package Dapper.Transaction
```

```csharp
private async Task<IResult> Handler(ISqlConnectionFactory connectionFactory, Guid id, Request request)
{
    using var connection = connectionFactory.OpenConnection();
    using var transaction = connection.BeginTransaction();
    try
    {
        var affectedRows = await transaction.ExecuteAsync(
            """
            UPDATE "Orders" SET "Status" = @Status, "ModifiedAt" = @ModifiedAt WHERE "Id" = @Id
            """, new
            {
                request.Status,
                ModifiedAt = DateTime.UtcNow,
                Id = id
            });
        transaction.Commit();
        return affectedRows == 0
            ? Results.NotFound()
            : Results.NoContent();
    }
    catch
    {
        transaction.Rollback();
        throw;
    }
}
```

Alternatively, you could use the **TransactionScope** class, which provides a much simpler way to deal with transactions:

```csharp
private async Task<IResult> Handler(ISqlConnectionFactory connectionFactory, Guid id)
{
    using var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled);
    using var connection = connectionFactory.OpenConnection();

    await connection.ExecuteAsync(
        "DELETE FROM \"Orders\" WHERE \"Id\" = @Id",
        new { Id = id });

    scope.Complete();

    return Results.NoContent();
}
```

## Stored Procedures

**Stored procedures** are a great way to encapsulate business logic and reduce the complexity of handling multiple queries.

They can also improve performance by optimizing query execution and reduce changes of duplicate queries making it easier to identify errors quickly.

Here's an example of stored procedure:

```csharp
CREATE OR REPLACE PROCEDURE cancel_old_created_orders()
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE "Orders"
    SET "Status" = 4,
        "ModifiedAt" = NOW()
    WHERE "Status" = 0
      AND "CreatedAt" <= NOW() - INTERVAL '1 minute';
END;
$$;
```

The only thing we need to do in the code is to simply provide the name of the stored procedure we're calling:

```csharp
private async Task Handler(ISqlConnectionFactory connectionFactory)
{
    using var connection = connectionFactory.OpenConnection();

    await connection.ExecuteAsync(
        "cancel_old_created_orders",
        commandType: CommandType.StoredProcedure);

    return Results.NoContent();
}
```

To tell Dapper to treat it as a stored procedure we need to set **CommandType** property with **StoredProcedure**.

## Conclusion

Dapper is a powerful and lightweight micro ORM that shines when you need fast, direct access to your database without the overhead of a full-fledged ORM.

In this post, we walked through the basics. Dapper has many more powerful features waiting for you to explore.

If you'd like to dive deeper into those or see real-world use cases, let me know. I’d be happy to cover them in future posts.

If you want to check out examples I created, you can find the source code here:

[Source Code](https://www.nikolatech.net/codes/dapper-examples)