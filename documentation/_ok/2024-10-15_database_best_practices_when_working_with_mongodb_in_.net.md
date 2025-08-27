```yaml
---
title: Best Practices When Working With MongoDb in .NET
source: https://antondevtips.com/blog/best-practices-when-working-with-mongodb-in-dotnet
date_published: 2024-10-15T08:55:21.852Z
date_captured: 2025-08-06T17:29:23.619Z
domain: antondevtips.com
author: Anton Martyniuk
category: database
technologies: [MongoDB, .NET, ASP.NET Core, Docker, docker-compose, MongoDB.Driver, Faker]
programming_languages: [C#, YAML, Bash]
tags: [mongodb, dotnet, aspnet-core, nosql, database, best-practices, data-access, dependency-injection, serialization, docker]
key_concepts: [NoSQL-databases, dependency-injection, MongoDB-connection, ID-generation, BSON-serialization, convention-registry, MongoDbContext-pattern, docker-containerization]
code_examples: false
difficulty_level: intermediate
summary: |
  This article outlines best practices for integrating MongoDB into .NET applications, particularly with ASP.NET Core. It covers essential setup steps, including Docker containerization for MongoDB and configuring the official MongoDB.Driver. Key recommendations include choosing appropriate ID types (Guid or string), configuring BSON serialization for camelCase property names, and serializing enums as strings for readability. The author also introduces a `MongoDbContext` pattern to streamline collection access, drawing parallels to EF Core, and provides a downloadable source code example of a shipping application.
---
```

# Best Practices When Working With MongoDb in .NET

![Cover image for the article "Best Practices When Working With MongoDb in .NET"](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Faspnetcore%2Fcover_asp_mongodb_best_practices.png&w=3840&q=100)

# Best Practices When Working With MongoDb in .NET

Oct 15, 2024

[Download source code](/source-code/best-practices-when-working-with-mongodb-in-dotnet)

4 min read

### Newsletter Sponsors

Go From Undiscovered to Growing & Monetizing Your [LinkedIn Account](https://learn.justinwelsh.me/a/2147505019/qvcFZYU6). Join 60+ LinkedIn Top Voices and 25,000+ students to accelerate the growth of your personal brand on LinkedIn.

Build a better, faster content production system. The [Content OS](https://learn.justinwelsh.me/a/2147507660/qvcFZYU6) is a multi-step system for creating a high-quality newsletter and 6-12 pieces of high-performance social media content each week.

Build a lean, profitable internet business in 2024. [The Creator MBA](https://learn.justinwelsh.me/a/2147771038/qvcFZYU6) delivers a complete blueprint for starting, building, and sustaining a profitable Internet business.

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

MongoDB is one of the most popular NoSQL databases, it allows building modern and scalable applications.

In this blog post, I will show you what are the best practices when working with MongoDB in .NET and C#.

[](#get-started-with-mongodb-in-aspnet-core)

## Get Started with MongoDB in ASP.NET Core

You need to follow these steps to Add MongoDB to your project.

[](#step-1-set-up-mongodb)

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

[](#step-2-add-mongodb-provider-and-connect-to-database)

### Step 2: Add MongoDB Provider and Connect to Database

To connect to MongoDB, you need to add the official MongoDB package to your project:

```bash
dotnet add package MongoDB.Driver
```

Next you need to configure a connection string to the MongoDB in `appsettings.json`:

```csharp
{
  "ConnectionStrings": {
    "MongoDb": "mongodb://admin:admin@mongodb:27017"
  }
}
```

[](#step-3-register-mongodb-dependencies-in-di-container)

### Step 3: Register MongoDB Dependencies in DI Container

You need to register a `IMongoClient` as a single instance in the DI container:

```csharp
var mongoConnectionString = configuration.GetConnectionString("MongoDb");
var mongoClientSettings = MongoClientSettings.FromConnectionString(mongoConnectionString);

services.AddSingleton<IMongoClient>(new MongoClient(mongoClientSettings));
```

This class is used to create a connection with a MongoDB database and allow performing database commands.

Now, as we're ready to go, let's explore a real-world application that uses MongoDB.

[](#an-example-application)

## An Example Application

Let's explore a **Shipping Application** that is responsible for creating and updating customers, orders and shipments for ordered products.

This application has the following entities:

*   Customers
*   Orders, OrderItems
*   Shipments, ShipmentItems

Let's explore a `Shipment` and `ShipmentItem` entities:

```csharp
public class Shipment
{
    public Guid Id { get; set; }
    public required string Number { get; set; }
    public required string OrderId { get; set; }
    public required Address Address { get; set; }
    public required ShipmentStatus Status { get; set; }
    public required List<ShipmentItem> Items { get; set; } = [];
}

public class ShipmentItem
{
    public required string Product { get; set; }
    public required int Quantity { get; set; }
}
```

At the end of the blog post, you can **download** a **source code** of Shippping Application.

[](#how-to-work-with-ids-in-mongodb)

## How To Work With IDs in MongoDB

There are multiple options to create entity IDs in MongoDB:

*   using `ObjectId` MongoDB class
*   using `string` type with attributes
*   using `Guid` type without attributes

Let's explore all the options in code:

```csharp
public class Shipment
{
    public ObjectId Id { get; set; }
}

public class Shipment
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; }
}

public class Shipment
{
    public Guid Id { get; set; }
}
```

The first option has the following disadvantages:

*   your entity is now aware of MongoDB
*   you need to manually create an ObjectId when inserting an entity

The second option allows MongoDB to automatically create IDs when inserting a record in the database. But it also makes your entity aware of MongoDB.

The third option is the most common option for C# developers, which is also widely used when working with SQL databases. This approach makes your entity separated from MongoDB.

Among these options, I prefer using the **3rd** option with `Guid`. In some cases, I can use the **2nd option** with `string` and attributes as well.

To be able to work with `Guid` you need to turn this feature on:

```csharp
BsonDefaults.GuidRepresentationMode = GuidRepresentationMode.V3;
BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
```

Never mind that `BsonDefaults.GuidRepresentationMode` is obsolete. In a future version it will be removed from the API and `GuidRepresentation.Standard` will turn on the `GuidRepresentationMode.V3` by default.

[](#how-to-configure-properties-serialization-in-mongodb)

## How To Configure Properties Serialization in MongoDB

MongoDB stores all the data in the database in the BSON format (binary JSON). The default casing in JSON is camelCasing, which I recommend to turn on for serialization of C# classes to MongoDB collections.

You need to register `CamelCaseElementNameConvention` in the MongoDB `ConventionRegistry`:

```csharp
ConventionRegistry.Register("camelCase", new ConventionPack {
    new CamelCaseElementNameConvention()
}, _ => true);
```

When working with `Enum` values, I recommend serializing it as `string` in the database. It makes your collection data much more expressive and readable rather than having numbers, which is the default way of `Enum` serialization.

This is how you can register it in the `ConventionRegistry`:

```csharp
ConventionRegistry.Register("EnumStringConvention", new ConventionPack
{
    new EnumRepresentationConvention(BsonType.String)
}, _ => true);
```

![Screenshot showing MongoDB Compass interface with two shipment documents. The _id field is a UUID and the status field is a string "Created", demonstrating the discussed serialization practices.](https://antondevtips.com/media/code_screenshots/aspnetcore/mongodb-best-practices/img_1.png)

In our application, a Shipment has an enum value `ShipmentStatus`:

```csharp
public class Shipment
{
    public Guid Id { get; set; }
    public required string Number { get; set; }
    public required string OrderId { get; set; }
    public required Address Address { get; set; }
    public required ShipmentStatus Status { get; set; }
    public required List<ShipmentItem> Items { get; set; } = [];
}

public enum ShipmentStatus
{
    Created,
    Processing,
    Dispatched,
    InTransit,
    Delivered,
    Received,
    Cancelled
}
```

[](#the-best-way-to-work-with-collections-in-mongodb)

## The Best Way To Work With Collections in MongoDB

And here is the most interesting part: I will show you what I think is the best way to work with MongoDB collections in C# code.

Every time you need to perform a database command you need to extract a database from `IMongoClient`. Then you need to extract a collection from the database.

```csharp
var database = mongoClient.GetDatabase("shipping-api");
var collection = database.GetCollection<Shipment>("shipments");
```

Every time you need to path a database and a collection name. This is tedious and can be error-prone.

One way to solve this problem is by introduction of constants:

```csharp
public static class MongoDbConsts
{
    public const string DatabaseName = "shipping-api";

    public const string ShipmentCollection = "shipments";
}

var database = mongoClient.GetDatabase(DatabaseName);
var collection = database.GetCollection<Shipment>(ShipmentCollection);
```

This approach is also error-prone - you can pass a wrong collection name, for example.

Here is my favourite approach for organizing code when working with MongoDB collections:

```csharp
public class MongoDbContext(IMongoClient mongoClient)
{
    private readonly IMongoDatabase _database = mongoClient.GetDatabase("shipping-api");

    public IMongoCollection<Shipment> Shipments => _database.GetCollection<Shipment>("shipments");

    public IMongoCollection<Customer> Customers => _database.GetCollection<Customer>("customers");

    public IMongoCollection<Order> Orders => _database.GetCollection<Order>("orders");
}
```

I like creating a `MongoDbContext` class that encapsulates all `IMongoCollections`. I find this approach useful as I can keep all the database and collection names in one place. With this approach, I can't mess up with a wrong collection name.

To be able to inject `MongoDbContext` into your classes, simply register it as Singleton in DI:

```csharp
services.AddSingleton<MongoDbContext>();
```

Here is how you can use `MongoDbContext`:

```csharp
public async Task<ErrorOr<ShipmentResponse>> Handle(
    CreateShipmentCommand request,
    CancellationToken cancellationToken)
{
    var shipmentAlreadyExists = await mongoDbContext.Shipments
        .Find(x => x.OrderId == request.OrderId)
        .AnyAsync(cancellationToken);

    if (shipmentAlreadyExists)
    {
        logger.LogInformation("Shipment for order '{OrderId}' is already created", request.OrderId);
        return Error.Conflict($"Shipment for order '{request.OrderId}' is already created");
    }

    var shipmentNumber = new Faker().Commerce.Ean8();
    var shipment = request.MapToShipment(shipmentNumber);

    await mongoDbContext.Shipments.InsertOneAsync(shipment, cancellationToken: cancellationToken);

    logger.LogInformation("Created shipment: {@Shipment}", shipment);

    var response = shipment.MapToResponse();
    return response;
}
```

If you have experience with EF Core, it really looks familiar.

At the end of the blog post, you can **download** a **source code** of Shippping Application.

[](#summary)

## Summary

When working with MongoDB in .NET and C#, I recommend using the following best practices:

*   Use `Guid` or `string` for `Id` field
*   Use `camelCase` when serializing entities into a database
*   Serialize enums as strings
*   Create MongoDbContext to manage database collections in one place

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/best-practices-when-working-with-mongodb-in-dotnet)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fbest-practices-when-working-with-mongodb-in-dotnet&title=Best%20Practices%20When%20Working%20With%20MongoDb%20in%20.NET)[X](https://twitter.com/intent/tweet?text=Best%20Practices%20When%20Working%20With%20MongoDb%20in%20.NET&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fbest-practices-when-working-with-mongodb-in-dotnet)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fbest-practices-when-working-with-mongodb-in-dotnet)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.