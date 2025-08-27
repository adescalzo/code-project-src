```yaml
---
title: Configure the EF Core Provider - EF Core Provider v8.1 - MongoDB Docs
source: https://www.mongodb.com/docs/entity-framework/v8.1/fundamentals/configure/
date_published: unknown
date_captured: 2025-08-12T13:33:28.550Z
domain: www.mongodb.com
author: Unknown
category: database
technologies: [MongoDB Entity Framework Core Provider, MongoDB, Entity Framework Core, .NET, MongoDB .NET/C# Driver, Microsoft.EntityFrameworkCore, MongoDB.Bson, Microsoft.Extensions.Configuration, MongoDB.EntityFrameworkCore.Extensions]
programming_languages: [C#]
tags: [mongodb, entity-framework-core, data-access, orm, dotnet, csharp, database, configuration]
key_concepts: [POCO, DbContext, DbSet, model-building, database-connection, object-relational-mapping]
code_examples: false
difficulty_level: intermediate
summary: |
  This guide explains how to configure an application to use the MongoDB Entity Framework Core Provider. It covers creating Plain Old CLR Objects (POCOs) to serve as entity models and defining a `DbContext` derived class to interact with the database. The article demonstrates how to use the `UseMongoDB()` method to connect the `DbContext` to a MongoDB instance. It also shows how to map C# entities to MongoDB collections using the `OnModelCreating()` method. Finally, a complete code example illustrates the configuration and a basic data insertion operation.
---
```

# Configure the EF Core Provider - EF Core Provider v8.1 - MongoDB Docs

# Configure the EF Core Provider

In this guide, you will learn how to configure an application to use the MongoDB Entity Framework Core Provider. To learn how to set up a new project and install the EF Core Provider, see the [Quick Start](/docs/entity-framework/v8.1/quick-start/#std-label-entity-framework-quickstart).

## Create a POCO

Create a [Plain old CLR/Class object](https://en.wikipedia.org/wiki/Plain_old_CLR_object), or **POCO**, to use as a model for your entity. A POCO is a simple class object that doesn't inherit features from any framework-specific base classes or interfaces.

The following code example shows how to create a POCO that represents a customer:

```csharp
public class Customer{    public ObjectId Id { get; set; }    public String Name { get; set; }    public String Order { get; set; }}
```

## Tip

To learn more about POCOs, see the [POCO guide](https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/serialization/poco/) in the .NET/C# Driver documentation.

## Create a DB Context Class

To begin using Entity Framework Core, create a context class that derives from [DBContext](https://learn.microsoft.com/en-us/dotnet/api/system.data.entity.dbcontext). The `DbContext` derived class instance represents a database session and is used to query and save instances of your entities.

The `DBContext` class exposes `DBSet` properties that specify the entities you can interact with while using that context.

The following example creates an instance of a `DBContext` derived class and specifies the `Customer` object as a `DBSet` property:

```csharp
public class MyDbContext : DbContext{    public DbSet<Customer> Customers { get; init; }    public MyDbContext(DbContextOptions options)        : base(options)    {    }    protected override void OnModelCreating(ModelBuilder modelBuilder)    {        base.OnModelCreating(modelBuilder);        modelBuilder.Entity<Customer>().ToCollection("customers");    }}
```

The previous code example overrides the `OnModelCreating()` method. Overriding the `OnModelCreating()` method allows you to specify configuration details for your model and its properties. This example uses the `ToCollection()` method to specify that the `Customer` entities in your application map to the `customers` collection in MongoDB.

## Use MongoDB

Once you've created a `DBContext` class, construct a `DbContextOptionsBuilder` object and call its `UseMongoDB()` method. This method takes two parameters: a `MongoClient` instance and the name of the database that stores the collections you are working with.

The `UseMongoDB()` method returns a `DbContextOptions` object. Pass the `Options` property of this object to the constructor for your `DBContext` class.

The following example shows how to construct a `DBContext` object in this way:

```csharp
var mongoClient = new MongoClient("<Your MongoDB Connection URI>");var dbContextOptions =    new DbContextOptionsBuilder<MyDbContext>().UseMongoDB(mongoClient, "<Database Name");var db = new MyDbContext(dbContextOptions.Options);
```

## Tip

### **Creating a MongoClient**

You can call methods from the MongoDB .NET/C# Driver when using the EF Core Provider. The previous example uses the `MongoClient()` method from the .NET/C# Driver to create a MongoDB client that connects to a MongoDB instance.

To learn more about using the MongoDB .NET/C# Driver to connect to MongoDB, see the [Connection guide](https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/connection/connect/) in the .NET/C# Driver documentation.

## Example

The following code example shows how to configure the EF Core Provider and insert a document into the database:

```csharp
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using Microsoft.Extensions.Configuration;
using MongoDB.EntityFrameworkCore.Extensions;

var mongoClient = new MongoClient("<Your MongoDB Connection URI>");

var dbContextOptions =
 new DbContextOptionsBuilder<MyDbContext>().UseMongoDB(mongoClient, "<Database Name>");

var db = new MyDbContext(dbContextOptions.Options);

// Add a new customer and save it to the database
db.Customers.Add(new Customer() { Name = "John Doe", Order = "1 Green Tea" });
db.SaveChanges();

public class Customer
{
 public ObjectId Id { get; set; }
 public String Name { get; set; }
 public String Order { get; set; }
}

public class MyDbContext : DbContext
{
 public DbSet<Customer> Customers { get; init; }

 public MyDbContext(DbContextOptions options)
 : base(options)
 {
 }

 protected override void OnModelCreating(ModelBuilder modelBuilder)
 {
 base.OnModelCreating(modelBuilder);
 modelBuilder.Entity<Customer>().ToCollection("customers");
 }
}
```