```yaml
---
title: Getting Started with MongoDB in EF Core
source: https://antondevtips.com/blog/getting-started-with-mongodb-in-ef-core
date_published: 2024-08-13T11:00:23.241Z
date_captured: 2025-08-06T17:25:54.092Z
domain: antondevtips.com
author: Anton Martyniuk
category: database
technologies: [EF Core, MongoDB, SQL Server, PostgreSQL, .NET, Docker, docker-compose, MongoDB.EntityFrameworkCore, MongoDB.Driver, LINQ]
programming_languages: [C#, SQL, YAML]
tags: [mongodb, ef-core, nosql, .net, orm, data-access, docker, database, csharp, development]
key_concepts: [orm, nosql-databases, document-oriented-data, dbcontext, linq-queries, docker-containers, data-modeling, connection-strings]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to integrating MongoDB with Entity Framework Core 8.0 in .NET applications. It details the setup process, including using Docker for MongoDB and adding the necessary EF Core provider package. The post demonstrates how to create an EF Core DbContext for MongoDB collections and perform common data operations like creating, updating, deleting, and reading documents using familiar LINQ queries. It also highlights the current limitations of EF Core 8.0's MongoDB support, such as lack of select projections and transactions, while emphasizing the benefits of a unified data access approach.]
---
```

# Getting Started with MongoDB in EF Core

![A dark blue and purple banner image with the title "GETTING STARTED WITH MONGODB OFFICIAL PROVIDER IN EF CORE" in large white text. On the left, a white icon with `</>` symbol and "dev tips" text is visible.](https://antondevtips.com/media/covers/efcore/cover_ef_mongodb.png)

# Getting Started with MongoDB in EF Core

Aug 13, 2024

[Download source code](/source-code/getting-started-with-mongodb-in-ef-core)

4 min read

### Newsletter Sponsors

[EF Core is too slow?](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website) Discover how you can easily insert 14x faster (reducing saving time by 94%).
Boost your performance with our method integrated within EF Core: Bulk Insert, update, delete, and merge.
Join 5,000+ satisfied customers who have trusted our library since 2014.

[Learn more](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website)

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Entity Framework Core (EF Core) is a popular ORM for .NET, typically used with relational databases like SQL Server or PostgreSQL. However, with the increasing popularity of NoSQL databases like MongoDB, developers often need to integrate these technologies into their applications. EF Core 8.0 introduces support for MongoDB, making it easier to work with document-oriented data in your .NET projects with your favourite ORM.

In this blog post, I will show you how to get started with MongoDB in EF Core 8.0.

## Getting Started With MongoDB In EF Core 8.0

### Step 1: Set Up MongoDB

We will set up MongoDB in a docker container using docker-compose-yml:

```yaml
services:
  mongodb:
    image: mongo:latest
    container_name: mongodb
    environment:
      - MONGO_INITDB_ROOT_USERNAME=admin
      - MONGO_INITDB_ROOT_PASSWORD=admin
    volumes:
      - ./docker_data/mongodb:/data/db
    ports:
      - "27017:27017"
    restart: always
    networks:
      - docker-web

networks:
  docker-web:
    driver: bridge
```

### Step 2: Add MongoDB Provider and Connect to Database

To connect to MongoDB in EF Core 8.0, you need to add the official MongoDB provider package to your project:

```bash
dotnet add package MongoDB.EntityFrameworkCore
```

Next you need to configure a connection string to the MongoDB in `appsettings.json`:

```csharp
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://admin:admin@mongodb:27017"
  }
}
```

### Step 3: Create EF Core DbContext

First, let's create a `Shipment` entity:

```csharp
public class Shipment
{
    public required ObjectId Id { get; set; }
    public required string Number { get; set; }
    public required string OrderId { get; set; }
    public required Address Address { get; set; }
    public required string Carrier { get; set; }
    public required string ReceiverEmail { get; set; }
    public required ShipmentStatus Status { get; set; }
    public required List<ShipmentItem> Items { get; set; } = [];
    public required DateTime CreatedAt { get; set; }
    public required DateTime? UpdatedAt { get; set; }
}
```

Here a `ObjectId` represents a document identifier in a MongoDb collection.

When working with MongoDB, you can create a familiar EF Core DbContext, the same way you do when working with SQL databases:

```csharp
public class EfCoreMongoDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<Shipment> Shipments { get; init; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Shipment>()
            .ToCollection("shipments")
            .Property(x => x.Status).HasConversion<string>();
    }
}
```

You need to add all the entities to the model builder and specify the collection names from the MongoDB. And now you can use the DbSet in the same manner as with an SQL table.

Notice, that you can use `Conversion` for your entities. In the code above we use string Conversion to make sure that enums are stored as strings in the database.

Finally, we need to register our DbContext and specify `UseMongoDB` for it:

```csharp
var mongoConnectionString = configuration.GetConnectionString("MongoDb")!;

builder.Services.AddDbContext<EfCoreMongoDbContext>(x => x
    .EnableSensitiveDataLogging()
    .UseMongoDB(mongoConnectionString, "shipping")
);
```

MongoDB is a NoSQL database that doesn't have a strict schema definition like SQL does, so you don't need to create any migrations.

Now we are ready to execute our first queries in the MongoDB.

## Writing Data Into MongoDb with EF Core

Writing data into Mongodb with EF Core doesn't differ from writing data into SQL Server or PostgreSQL. The real benefit of using EF Core with MongoDB is that you don't know that you are working with a NoSQL database under the hood.

This is beneficial as you can migrate from a SQL database to MongoDB with only a few tweaks if you're using EF Core. Or get started with MongoDB right away without having to learn a new API.

> But be aware that not all MongoDB features are available in EF Core. You may need to use the MongoDB.Driver directly for some advanced features.

Here is how you can create, update and delete a document in MongoDB with EF Core:

```csharp
// Create new shipment
var shipment = request.MapToShipment(shipmentNumber);
context.Shipments.Add(shipment);
await context.SaveChangesAsync(cancellationToken);

// Update shipment
shipment.Status = ShipmentStatus.Delivered;
await context.SaveChangesAsync(cancellationToken);

// Delete shipment
context.Shipments.Remove(shipment);
await context.SaveChangesAsync(cancellationToken);
```

When assigning an `Id`, you can use the `ObjectId` factory method:

```csharp
Id = ObjectId.GenerateNewId();
```

## Reading Data From MongoDb with EF Core

As you can guess, you can use the familiar LINQ methods in EF Core to read data from MongoDB.

Here is how to select single or multiple records:

```csharp
var shipment = await context.Shipments
    .Where(x => x.Number == request.ShipmentNumber)
    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

var shipments = await context.Shipments
    .Where(x => x.Status == ShipmentStatus.Dispatched)
    .ToListAsync(cancellationToken: cancellationToken);
```

You can also provide a filtering predicate inside a `FirstOrDefaultAsync` or other similar methods.

Here is how to check if any entity exists or get count of entities:

```csharp
var shipmentAlreadyExists = await context.Shipments
    .Where(s => s.OrderId == request.OrderId)
    .AnyAsync(cancellationToken);
    
var count = await context.Shipments
    .CountAsync(x => x.Status == ShipmentStatus.Delivered);
```

## Limitations When Using Entity Framework 8.0 With MongoDB

The following features are not supported in EF 8.0 for MongoDB:

**1\. Select Projections:** Select projections use the Select() method in a LINQ query to change the structure of the created object. EF 8 doesn't support such projections.

**2\. Scalar Aggregations:** EF 8 only supports the following scalar aggregation operations:

*   Count(), CountAsync()
*   LongCount(), LongCountAsync()
*   Any(), AnyAsync() with or without predicates.

**3\. Transactions:** EF 8 does not support the Entity Framework Core transaction model for MongoDB.

If you need any of these features without restrictions - use `MongoDB.Driver` directly.

MongoDB is a NoSQL database, it doesn't support the following EF Core features:

*   Migrations
*   Foreign and Alternate Keys
*   Table Splitting and Temporal Tables
*   Spatial Data

For more information, you can read the [official MongoDB documentation](https://www.mongodb.com/docs/entity-framework/current/limitations/).

## Summary

EF Core 8 allows seamless integration with MongoDB with familiar Entity Framework classes, interfaces and methods. You can use the DbContext to configure MongoDB collections. You can use your favourite LINQ methods to perform read operations and the familiar methods for write operations.

For sure, in the next versions of Entity Framework, support for more and more features will be added for MongoDB.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/getting-started-with-mongodb-in-ef-core)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-mongodb-in-ef-core&title=Getting%20Started%20with%20MongoDB%20in%20EF%20Core)[X](https://twitter.com/intent/tweet?text=Getting%20Started%20with%20MongoDB%20in%20EF%20Core&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-mongodb-in%2Fef-core)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-mongodb-in-ef-core)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.