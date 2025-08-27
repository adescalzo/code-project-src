```yaml
---
title: "MongoDB EF Core Provider: What's New? - .NET Blog"
source: https://devblogs.microsoft.com/dotnet/mongodb-ef-core-provider-whats-new/?hide_banner=true
date_published: 2024-10-21T17:05:00.000Z
date_captured: 2025-08-12T14:21:51.540Z
domain: devblogs.microsoft.com
author: Rishit Bhatia
category: database
technologies: [MongoDB EF Core Provider, MongoDB, MongoDB Atlas, .NET, Entity Framework Core, .NET/C# Driver for MongoDB]
programming_languages: [C#, JavaScript, MQL]
tags: [mongodb, ef-core, dotnet, data-access, orm, database, change-tracking, indexing, transactions, concurrency]
key_concepts: [change-tracking, index-management, LINQ, optimistic-concurrency, transactions, document-model, code-first, data-access-patterns]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores new features in the generally available MongoDB EF Core Provider, demonstrating its capabilities with MongoDB Atlas. It covers how to leverage EF Core's Code First approach for flexible schema management, including adding properties and utilizing change tracking. The post also details advanced functionalities such as index management using the underlying C# driver, performing complex LINQ queries, and the newly introduced support for autotransactions and optimistic concurrency control. Code examples in C# are provided to illustrate these features, showcasing efficient interaction with MongoDB using familiar EF Core patterns.]
---
```

# MongoDB EF Core Provider: What's New? - .NET Blog

October 21st, 2024

![heart](/wp-content/themes/devblogs-evo/images/emojis/heart.svg)4 reactions

# MongoDB EF Core Provider: What’s New?

![Rishit Bhatia](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/nsl_avatars/7697ef660da58bf69c09f959cb9375c0.jpg) ![Luce Carter](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/10/Luce-96x96.jpg)

[Rishit](https://devblogs.microsoft.com/dotnet/author/rishit-bhatia) ,

[Luce](https://devblogs.microsoft.com/dotnet/author/luce)

## Table of contents

*   [Prerequisites](#prerequisites)
*   [Features highlight](#features-highlight)
*   [Adding properties and change tracking](#adding-properties-and-change-tracking)
*   [Index management](#index-management)
*   [Querying data](#querying-data)
*   [Autotransactions and optimistic concurrency](#autotransactions-and-optimistic-concurrency)
*   [Summary](#summary)
*   [Learn more](#learn-more)

## Read next

October 22, 2024

### [.NET MAUI Welcomes Syncfusion Open-source Contributions](https://devblogs.microsoft.com/dotnet/dotnet-maui-welcomes-syncfusion-open-source-contributions/)

![David Ortinau](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2021/11/davidortinau2024_800-96x96.jpg)

David Ortinau

October 23, 2024

### [eShop infused with AI – a comprehensive intelligent app sample](https://devblogs.microsoft.com/dotnet/e-shop-infused-with-ai-comprehensive-intelligent-dotnet-app-sample/)

![Jeremy Likness](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2018/10/Jeremy-Likness-150x150.jpg)

Jeremy Likness

> This is a guest post by Rishit Bhatia and Luce Carter. Rishit is a Senior Product Manager at MongoDB focusing on the .NET Developer Experience and has been working with C# since many years hands on before moving into Product Management. Luce is a Developer Advocate at MongoDB, Microsoft MVP and lover of code, sunshine and learning. This blog was reviewed by the Microsoft .NET team for EF Core.

The EF Core provider for [MongoDB](https://www.mongodb.com/) went [GA](https://www.mongodb.com/blog/post/mongodb-provider-entity-framework-core-now-generally-available) in May 2024. We’ve come a long way since we initially released this package in preview six months ago. We wanted to share some interesting features that we’ve been working on which would not have been possible without the support of and collaboration with Microsoft’s .NET Data and Entity Framework team.

In this post, we will be using the [MongoDB EF Core provider](https://www.mongodb.com/docs/drivers/csharp/current/) with [MongoDB Atlas](https://www.mongodb.com/products/platform/atlas-database) to showcase the following:

*   Adding a property to an entity and change tracking
*   Leveraging the escape hatch to create an index
*   Performing complex queries
*   Transactions and optimistic concurrency

The code related to this blog can be found on [Github](https://github.com/mongodb-developer/efcore_highlights). The boilerplate code to get started is in the “start” branch. The full code with all the feature highlights mentioned below is in the “main” branch.

## Prerequisites

We will be using a [sample dataset](https://www.mongodb.com/docs/atlas/sample-data/) — specifically, the movies collection from the _sample\_mflix_ database available for MongoDB Atlas in this example. To set up an Atlas cluster with sample data, you can follow the steps [in the docs](https://www.mongodb.com/docs/atlas/getting-started/). We’ll create a simple .NET Console App to get started with the MongoDB EF Core provider. For more details on how to do that, you can check the [quickstart guide](https://www.mongodb.com/docs/entity-framework/current/quick-start/).

At this point, you should be connected to Atlas and able to output the movie plot from the movie being read in the quickstart guide.

## Features highlight

### Adding properties and change tracking

One of the advantages of MongoDB’s document model is that it supports a flexible schema. This, coupled with EF Core’s ability to support a Code First approach, lets you add properties to your entities on the fly. To show this, we are going to add a new nullable boolean property called `adapted_from_book` to our model class. This will make our model class as seen below:

```csharp
public class Movie
{
    public ObjectId Id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; }

    [BsonElement("rated")]
    public string Rated { get; set; }

    [BsonElement("plot")]
    public string Plot { get; set; }

    [BsonElement("adaptedFromBook")]
    public bool? AdaptedFromBook { get; set; }
}
```

Now, we are going to set this newly added property for the movie entity we found and see [EF Core’s Change Tracking](https://learn.microsoft.com/ef/core/change-tracking/) in action after we save our changes. To do so, we’ll add the following lines of code after printing the movie plot:

```csharp
movie.AdaptedFromBook = false;
await db.SaveChangesAsync();
```

Before we run our program, let’s go to our collection in Atlas and find this movie to make sure that this newly created field `adapted_from_book` does not exist in our database. To do so, simply go to your cluster in the Atlas Web UI and select Browse Collections.

![A screenshot of the MongoDB Atlas UI, showing the "Browse Collections" button highlighted within the "Clusters" overview page.](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/10/mongodb-atlas-browse-collections-ui.png)

Then, choose the movies collection from the _sample\_mflix_ database. In the filter tab, we can find our movie using the query below:

```javascript
{title: "Back to the Future"}
```

This should find our movie and we can confirm that the new field we intend to add is indeed not seen.

![A screenshot of the MongoDB Atlas UI displaying a single movie document (Back to the Future) from the `sample_mflix.movies` collection, showing its various fields like `_id`, `plot`, `title`, etc., before the `adapted_from_book` field is added.](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/10/mongodb-sample-movie-document.png)

Next, let’s add a breakpoint to the two new lines we just added to make sure that we can track the changes live as we proceed. Select the Start Debugging button to run the app. When the first breakpoint is hit, we can see that the local field value has been assigned.

![A screenshot of a C# debugger window in Visual Studio, showing the local variables for a `Movie` object, with the `AdaptedFromBook` property highlighted and set to `false`.](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/10/mongodb-contents-movie-field-view-debugger.png)

Let’s hit Continue and check the document in the database. We can see that the new field has not yet been added. Let’s step over the save changes call which will end the program. At this point, if we check our document in the database, we’ll notice that the new field has been added as seen below!

![A screenshot of the MongoDB Atlas UI showing the same movie document as before, but now with the new `adapted_from_book: false` field successfully added at the bottom.](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/10/mongodb-new-document-field-adapted.png)

### Index management

The MongoDB EF Core provider is built on top of the existing [.NET/C# driver](https://www.mongodb.com/docs/drivers/csharp/current/). One advantage of this architecture is that we can reuse the `MongoClient` already created for the `DbContext` to leverage other capabilities exposed by MongoDB’s developer data platform. This includes but is not limited to features such as [Index Management](https://www.mongodb.com/docs/drivers/csharp/upcoming/fundamentals/indexes/#list-indexes), [Atlas Search](https://www.mongodb.com/docs/drivers/csharp/upcoming/fundamentals/atlas-search/), and [Vector Search](https://www.mongodb.com/docs/atlas/atlas-vector-search/tutorials/vector-search-quick-start/).

We’ll see how we can create a new index using the driver in this same application. First, we’ll list the indexes in our collection to see which indexes already exist. MongoDB creates an index on the `_id` field by default. We’re going to create a helper function to print the indexes:

```csharp
var moviesCollection = client.GetDatabase("sample_mflix").GetCollection<Movie>("movies");
Console.WriteLine("Before creating a new Index:");
PrintIndexes();

void PrintIndexes()
{
    var indexes = moviesCollection.Indexes.List();
    foreach (var index in indexes.ToList())
    {
        Console.WriteLine(index);
    }
}
```

The expected output is as seen below:

```javascript
{ "v" : 2, "key" : { "_id" : 1 }, "name" : "_id_" }
```

Now, we’ll create a [compound index](https://www.mongodb.com/docs/manual/core/indexes/index-types/index-compound/) on the title and rated fields in our collection and print the indexes again.

```csharp
var moviesIndex = new CreateIndexModel<Movie>(Builders<Movie>.IndexKeys
    .Ascending(m => m.Title)
    .Ascending(x => x.Rated));
await moviesCollection.Indexes.CreateOneAsync(moviesIndex);

Console.WriteLine("After creating a new Index:");
PrintIndexes();
```

We can see that a new index with the name `title_1_rated_1` has been created.

```javascript
After creating a new Index:
{ "v" : 2, "key" : { "_id" : 1 }, "name" : "_id_" }
{ "v" : 2, "key" : { "title" : 1, "rated" : 1 }, "name" : "title_1_rated_1" }
```

### Querying data

Since EF Core already supports Language Integrated Query (LINQ) Syntax, it becomes easy to write strongly typed queries in C#. Based on the fields available in our model classes, we can try to find some interesting movies from our collection. Let’s say I wanted to find all movies that are rated “PG-13” with their plot containing the word “shark” but I wanted them ordered by their title field. I can do so easily with the following query:

```csharp
var myMovies = await db.Movies
    .Where(m => m.Rated == "PG-13" && m.Plot.Contains("shark"))
    .OrderBy(m => m.Title)
    .ToListAsync();

foreach (var m in myMovies)
{
    Console.WriteLine(m.Title);
}
```

We can then print out the queries using the code above and run the program using `dotnet run` to see the results. We should be able to see two movie names from the 20K+ movies in our collection printed in the console as seen below.

```text
Jaws: The Revenge
Shark Night 3D
```

If you would like to see the query that is sent to the server, which in this case is the MQL, then you can enable logging in the `Create` function on the DbContext as seen below:

```csharp
public static MflixDbContext Create(IMongoDatabase database) =>
       new(new DbContextOptionsBuilder<MflixDbContext>()
           .UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName)
           .LogTo(Console.WriteLine)
           .EnableSensitiveDataLogging()
           .Options);
```

This way we can see the following as a part of our detailed logs when we run the program again:

```text
Executed MQL query
sample_mflix.movies.aggregate([{ "$match" : { "rated" : "PG-13", "plot" : /shark/s } }, { "$sort" : { "title" : 1 } }])
```

### Autotransactions and optimistic concurrency

Yes, you read that right! The MongoDB EF Core provider from its 8.1.0 release supports transactions and optimistic concurrency. What this means is that by default, `SaveChanges` and `SaveChangesAsync` are transactional. This will empower automatic rollback of operations in production grade workloads in case of any failures and ensure that all operations are fulfilled with [optimistic concurrency](https://en.wikipedia.org/wiki/Optimistic_concurrency_control).

If you want to turn off transactions, you can do so during the initialization phase before calling any `SaveChanges` operation.

```csharp
db.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
```

The provider supports two methods of optimistic concurrency depending on your requirements which are through a concurrency check or row versions. You can read more about it [in the docs](https://www.mongodb.com/docs/entity-framework/current/fundamentals/optimistic-concurrency/). We’ll be using the RowVersion to demonstrate this use case. This leverages the `Version` field in our model class which will be updated automatically by the MongoDB EF Provider. To add the version, we add the following to our model class.

```csharp
[Timestamp]
 public long? Version { get; set; }
```

First, let’s create a new movie entity called `myMovie` as seen below and add it to the `DbSet`, followed by `SaveChangesAsync`.

```csharp
Movie myMovie1= new Movie {
    Title = "The Rise of EF Core 1",
    Plot = "Entity Framework (EF) Core is a lightweight, extensible, open source and cross-platform version of the popular Entity Framework data access technology.",
    Rated = "G"
};

db.Movies.Add(myMovie1);
await db.SaveChangesAsync();
```

Now, let’s create a new `DbContext` similar to the one we created above. We can move the database creation into a variable so we don’t have to define the name of the database again. With this new context, let’s add a sequel for our movie and add it to the DbSet. We’ll also add a third part (yes, it’s a trilogy) but use the same ID as our second movie entity to this new context and then save our changes.

```csharp
var dbContext2 = MflixDbContext.Create(database);
dbContext2.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
var myMovie2 = new Movie { title = "The Rise of EF Core 2" };
dbContext2.Movies.Add(myMovie2);

var myMovie3 = new Movie { Id = myMovie2.Id,Title = "The Rise of EF Core 3" };
dbContext2.Movies.Add(myMovie3);
await dbContext2.SaveChangesAsync();
```

With transactions now being supported, the second set of operations for our latter two movie entities should not go through since we are trying to add them with an already existing `_id`. We should see an exception and the transaction should be rolled with only one movie being seen in our database. Let’s run and see if that is true.

We rightfully see an exception and we can confirm that we have only one movie (the first part) inserted into the database.

![A screenshot of a C# debugger in Visual Studio, showing a `System.InvalidOperationException` being thrown during `dbContext2.SaveChangesAsync()`, indicating that an entity with the same key (`_id`) is already being tracked, demonstrating the transaction rollback due to a duplicate ID.](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/10/mongodb-transaction-exception.png)

The following shows only a single document in the database as the transaction was rolled back.

![A screenshot of the MongoDB Atlas UI showing the query results for "The Rise of EF Core 1", confirming that only one document was inserted into the database after the transaction for the duplicate ID was rolled back.](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/10/mongodb-transaction-rollback.png)

Don’t worry, we will correctly add our trilogy in the database. Let’s remove the `_id` assignment on our third entity and let MongoDB automatically insert it for us.

```csharp
var myMovie3 = new Movie { Title = "The Rise of EF Core 3" };
```

Once we re-run the program, we can see that all our entities have been added to the database.

![A screenshot of the MongoDB Atlas UI displaying three movie documents (The Rise of EF Core 1, 2, and 3) in the `sample_mflix.movies` collection, confirming that all entities were successfully added after resolving the duplicate ID issue.](https://devblogs.microsoft.com/dotnet/wp-content/uploads/sites/10/2024/10/mongodb-duplicate-exception-fix.png)

## Summary

We were able to use the [MongoDB EF Core provider](https://www.mongodb.com/docs/drivers/csharp/current/) with [MongoDB Atlas](https://www.mongodb.com/products/platform/atlas-database) to showcase different capabilities like adding a property to an entity on the fly, leveraging the Escape Hatch to create an index, performing complex queries via LINQ, and demonstrating the newly added transactions and optimistic concurrency support.

## Learn more

To learn more about EF Core and MongoDB:

*   See the [EF Core documentation](https://learn.microsoft.com/ef/core/) to learn more about using EF Core to access all kinds of databases.
*   See the [MongoDB documentation](https://www.mongodb.com/docs/) to learn more about using MongoDB from any platform.
*   See the [MongoDB EF Core provider documentation](https://www.mongodb.com/docs/entity-framework/current/quick-start/) for more information on how to get started.
*   Watch the talk about [EF Core 9: Evolving Data Access in .NET](https://www.youtube.com/watch?v=LuvdiUggQrU&list=PLdo4fOcmZ0oUZz7p8H1HsQjgv5tRRIvAS&index=19) on the Microsoft Youtube channel.