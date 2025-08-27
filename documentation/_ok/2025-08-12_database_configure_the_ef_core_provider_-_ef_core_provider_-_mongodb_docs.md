```yaml
---
title: Configure the EF Core Provider - EF Core Provider - MongoDB Docs
source: https://www.mongodb.com/docs/entity-framework/current/fundamentals/configure/
date_published: unknown
date_captured: 2025-08-12T13:27:30.772Z
domain: www.mongodb.com
author: Unknown
category: database
technologies: [MongoDB Entity Framework Core Provider, Entity Framework Core, MongoDB, .NET, MongoDB .NET/C# Driver, Microsoft.EntityFrameworkCore, MongoDB.Bson, MongoDB.Driver, Microsoft.Extensions.Configuration, MongoDB.EntityFrameworkCore.Extensions]
programming_languages: [C#]
tags: [mongodb, entity-framework-core, orm, data-access, dotnet, nosql, database, configuration, poco]
key_concepts: [POCO, DbContext, DbSet, ModelBuilder, database-connection, object-mapping, entity-configuration]
code_examples: false
difficulty_level: intermediate
summary: |
  [This guide explains how to configure an application to use the MongoDB Entity Framework Core Provider. It details the creation of Plain Old CLR Objects (POCOs) to serve as entity models, followed by defining a `DbContext` class to manage database sessions and expose `DbSet` properties. The article then demonstrates how to connect to a MongoDB instance using `DbContextOptionsBuilder` and the `UseMongoDB()` method, including how to map entities to MongoDB collections. A complete code example illustrates the entire setup process and data insertion, enabling developers to leverage EF Core's familiar patterns with a NoSQL database like MongoDB.]
---
```

# Configure the EF Core Provider - EF Core Provider - MongoDB Docs

[Docs Home](https://www.mongodb.com/docs/)

/

[Languages](https://www.mongodb.com/docs/drivers/)

/

[C#](https://www.mongodb.com/docs/languages/csharp/)

/

[EF Core Provider](/docs/entity-framework/current/)

/

[Fundamentals](/docs/entity-framework/current/fundamentals/)

# Configure the EF Core Provider[

](#configure-the-ef-core-provider "Permalink to this heading")

Copy page

In this guide, you will learn how to configure an application to use the MongoDB Entity Framework Core Provider. To learn how to set up a new project and install the EF Core Provider, see the [Quick Start.](/docs/entity-framework/current/quick-start/#std-label-entity-framework-quickstart)

## Create a POCO[

](#create-a-poco "Permalink to this heading")

Create a [Plain old CLR/Class object](https://en.wikipedia.org/wiki/Plain_old_CLR_object), or **POCO**, to use as a model for your entity. A POCO is a simple class object that doesn't inherit features from any framework-specific base classes or interfaces.

The following code example shows how to create a POCO that represents a customer:

{"@context":"https://schema.org","@type":"SoftwareSourceCode","codeSampleType":"code snippet","text":"public class Customer\\n{\\n public ObjectId Id { get; set; }\\n public String Name { get; set; }\\n public String Order { get; set; }\\n}","programmingLanguage":"C#"}

```
public class Customer{    public ObjectId Id { get; set; }    public String Name { get; set; }    public String Order { get; set; }}
```

## Tip

To learn more about POCOs, see the [POCO guide](https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/serialization/poco/) in the .NET/C# Driver documentation.

## Create a DB Context Class[

](#create-a-db-context-class "Permalink to this heading")

To begin using Entity Framework Core, create a context class that derives from [DBContext](https://learn.microsoft.com/en-us/dotnet/api/system.data.entity.dbcontext). The `DbContext` derived class instance represents a database session and is used to query and save instances of your entities.

The `DBContext` class exposes `DBSet` properties that specify the entities you can interact with while using that context.

The following example creates an instance of a `DBContext` derived class and specifies the `Customer` object as a `DBSet` property:

{"@context":"https://schema.org","@type":"SoftwareSourceCode","codeSampleType":"code snippet","text":"public class MyDbContext : DbContext\\n{\\n public DbSet&lt;customer&gt; Customers { get; init; }\\n\\n public MyDbContext(DbContextOptions options)\\n : base(options)\\n {\\n }\\n\\n protected override void OnModelCreating(ModelBuilder modelBuilder)\\n {\\n base.OnModelCreating(modelBuilder);\\n modelBuilder.Entity&lt;customer&gt;().ToCollection(\\"customers\\");\\n }\\n}","programmingLanguage":"C#"}

```
public class MyDbContext : DbContext{    public DbSet<Customer> Customers { get; init; }    public MyDbContext(DbContextOptions options)        : base(options)    {    }    protected override void OnModelCreating(ModelBuilder modelBuilder)    {        base.OnModelCreating(modelBuilder);        modelBuilder.Entity<Customer>().ToCollection("customers");    }}
```

The previous code example overrides the `OnModelCreating()` method. Overriding the `OnModelCreating()` method allows you to specify configuration details for your model and its properties. This example uses the `ToCollection()` method to specify that the `Customer` entities in your application map to the `customers` collection in MongoDB.

## Use MongoDB[

](#use-mongodb "Permalink to this heading")

Once you've created a `DBContext` class, construct a `DbContextOptionsBuilder` object and call its `UseMongoDB()` method. This method takes two parameters: a `MongoClient` instance and the name of the database that stores the collections you are working with.

The `UseMongoDB()` method returns a `DbContextOptions` object. Pass the `Options` property of this object to the constructor for your `DBContext` class.

The following example shows how to construct a `DBContext` object in this way:

{"@context":"https://schema.org","@type":"SoftwareSourceCode","codeSampleType":"code snippet","text":"var mongoClient = new MongoClient(\\"&lt;your&gt;\\");\\n\\nvar dbContextOptions =\\n new DbContextOptionsBuilder&lt;mydbcontext&gt;().UseMongoDB(mongoClient, \\"","programmingLanguage":"C#"}

```
var mongoClient = new MongoClient("<Your MongoDB Connection URI>");var dbContextOptions =    new DbContextOptionsBuilder<MyDbContext>().UseMongoDB(mongoClient, "<Database Name");var db = new MyDbContext(dbContextOptions.Options);
```

## Tip

### **Creating a MongoClient**

You can call methods from the MongoDB .NET/C# Driver when using the EF Core Provider. The previous example uses the `MongoClient()` method from the .NET/C# Driver to create a MongoDB client that connects to a MongoDB instance.

To learn more about using the MongoDB .NET/C# Driver to connect to MongoDB, see the [Connection guide](https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/connection/connect/) in the .NET/C# Driver documentation.

## Example[

](#example "Permalink to this heading")

The following code example shows how to configure the EF Core Provider and insert a document into the database:

{"@context":"https://schema.org","@type":"SoftwareSourceCode","codeSampleType":"code snippet","text":"using Microsoft.EntityFrameworkCore;\\nusing MongoDB.Bson;\\nusing MongoDB.Driver;\\nusing Microsoft.Extensions.Configuration;\\nusing MongoDB.EntityFrameworkCore.Extensions;\\n\\nvar mongoClient = new MongoClient(\\"&lt;your&gt;\\");\\n\\nvar dbContextOptions =\\n new DbContextOptionsBuilder&lt;mydbcontext&gt;().UseMongoDB(mongoClient, \\"&lt;database&gt;\\");\\n\\nvar db = new MyDbContext(dbContextOptions.Options);\\n\\n// Add a new customer and save it to the database\\ndb.Customers.Add(new Customer() { name = \\"John Doe\\", Order = \\"1 Green Tea\\" });\\ndb.SaveChanges();\\n\\npublic class Customer\\n{\\n public ObjectId Id { get; set; }\\n public String Name { get; set; }\\n public String Order { get; set; }\\n}\\n\\npublic class MyDbContext : DbContext\\n{\\n public DbSet&lt;customer&gt; Customers { get; init; }\\n\\n public MyDbContext(DbContextOptions options)\\n : base(options)\\n {\\n }\\n\\n protected override void OnModelCreating(ModelBuilder modelBuilder)\\n {\\n base.OnModelCreating(modelBuilder);\\n modelBuilder.Entity&lt;customer&gt;().ToCollection(\\"customers\\");\\n }\\n}","programmingLanguage":"C#"}

```
using Microsoft.EntityFrameworkCore;using MongoDB.Bson;using MongoDB.Driver;using Microsoft.Extensions.Configuration;using MongoDB.EntityFrameworkCore.Extensions;var mongoClient = new MongoClient("<Your MongoDB Connection URI>");var dbContextOptions =    new DbContextOptionsBuilder<MyDbContext>().UseMongoDB(mongoClient, "<Database Name>");var db = new MyDbContext(dbContextOptions.Options);// Add a new customer and save it to the databasedb.Customers.Add(new Customer() { name = "John Doe", Order = "1 Green Tea" });db.SaveChanges();public class Customer{    public ObjectId Id { get; set; }    public String Name { get; set; }    public String Order { get; set; }}public class MyDbContext : DbContext{    public DbSet<Customer> Customers { get; init; }    public MyDbContext(DbContextOptions options)        : base(options)    {    }    protected override void OnModelCreating(ModelBuilder modelBuilder)    {        base.OnModelCreating(modelBuilder);        modelBuilder.Entity<Customer>().ToCollection("customers");    }}
```

[

Back

Fundamentals





](/docs/entity-framework/current/fundamentals/ "Previous Section")

[

Next

Query Data





](/docs/entity-framework/current/fundamentals/query-data/ "Next Section")