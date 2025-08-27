```yaml
---
title: Write Data to MongoDB - EF Core Provider - MongoDB Docs
source: https://www.mongodb.com/docs/entity-framework/current/fundamentals/write-data/
date_published: unknown
date_captured: 2025-08-12T13:28:49.648Z
domain: www.mongodb.com
author: Unknown
category: database
technologies: [MongoDB, Entity Framework Core, EF Core Provider, .NET]
programming_languages: [C#]
tags: [mongodb, entity-framework-core, data-access, crud, csharp, orm, database, transactions, data-persistence]
key_concepts: [orm, crud-operations, transactions, data-persistence, dbcontext, dbset, automatic-transactions]
code_examples: false
difficulty_level: intermediate
summary: |
  This guide demonstrates how to perform common write operations—insert, update, and delete—on a MongoDB database using the Entity Framework Core Provider for C# applications. It explains how the `SaveChanges()` and `SaveChangesAsync()` methods automatically detect data changes and persist them, leveraging the MongoDB Query API. The article highlights that these methods are transactional by default, ensuring data consistency, and provides examples for inserting single or multiple entities, updating existing data, and deleting records. It also briefly discusses disabling automatic transactions and its implications.
---
```

# Write Data to MongoDB - EF Core Provider - MongoDB Docs

# Write Data to MongoDB

## Overview

Entity Framework Core allows you to work with data in your application without explicitly running database commands. You can insert, update, or delete data within your application and persist those changes to MongoDB by using the `SaveChanges()` or `SaveChangesAsync()` method.

When you call the `SaveChanges()` or `SaveChangesAsync()` method, the EF Core Provider automatically detects any changes made to your data and runs the necessary commands to update the database by using the MongoDB Query API.

In this guide, you can see examples of how to perform common write operations on an application configured to use the EF Core Provider.

## Tip

To learn how to configure an application to use the EF Core Provider, see [Configure the EF Core Provider.](/docs/entity-framework/current/fundamentals/configure/#std-label-entity-framework-configure)

## Transactional Write Operations

The `SaveChanges()` and `SaveChangesAsync()` methods are transactional by default. This means that if an error occurs during an operation, the provider rolls back any changes made to the database. Because of this, your application must be connected to a transaction-capable deployment of MongoDB server, such as a replica set.

You can disable automatic transactions in the `SaveChanges()` and `SaveChangesAsync()` methods by setting the `AutoTransactionBehavior` property to `AutoTransaction.Never` on your `DbContext` subclass during application setup. However, we do not recommend disabling this feature. Doing so causes any concurrency changes or operation failures during the save operation to leave the database in an inconsistent state.

The following example shows how to disable automatic transactions in the `SaveChanges()` and `SaveChangesAsync()` methods:

```csharp
dbContext.Database.AutoTransactionBehavior = AutoTransactionBehavior.Never;
```

## Warning

Disabling automatic transactions can lead to data inconsistencies. We recommend that you do not disable this feature.

## Insert

You can use the `Add()` method to insert a single entity into your collection, or you can use the `AddRange()` method to insert multiple entities at once.

### Insert One Entity

The `Add()` method accepts a single entity of the same type that you specified on the `DbSet` instance that you are modifying.

The following code uses the `Add()` method to add a new `Planet` object to the `DbSet` called `Planets`. It then calls the `SaveChanges()` method to insert that entity into the MongoDB collection.

```csharp
db.Planets.Add(new Planet(){    name = "Pluto",    hasRings = false,    orderFromSun = 9});db.SaveChanges();
```

### Insert Multiple Entities

The `AddRange()` method accepts an array of entities that you want to add to the `DbSet`.

The following code uses the `AddRange()` method to add an array of `Planet` objects to the `DbSet` called `Planets`. It then calls the `SaveChanges()` method to insert those entities into the MongoDB collection.

```csharp
var planets = new[]{    new Planet()    {        _id = ObjectId.GenerateNewId(),        name = "Pluto",        hasRings = false,        orderFromSun = 9    },    new Planet()    {        _id = ObjectId.GenerateNewId(),        name = "Scadrial",        hasRings = false,        orderFromSun = 10    }};db.Planets.AddRange(planets);db.SaveChanges();
```

## Update

To update an entity, first retrieve the entity that you want to update. Then make the changes to that entity. The provider tracks any changes made to the entity, such as setting properties or adding and removing items from fields with list values. To save the update to MongoDB, call the `SaveChanges()` method. The EF Core Provider compares the updated entity with a snapshot of the entity before the change and automatically updates the collection by using the MongoDB Query API.

The following code retrieves an entity in which the `name` value is `"Mercury"`, then updates the `name` field. The code then calls the `SaveChanges()` method to persist that change to the collection.

```csharp
var planet = db.Planets.FirstOrDefault(p => p.name == "Mercury");planet.name = "Mercury the first planet";db.SaveChanges();
```

## Delete

You can use the `Remove()` method to delete a single entity from your collection, or the `RemoveRange()` method to delete multiple entities at once.

### Delete One Entity

The `Remove()` method accepts a single entity of the same type that you specified on the `DbSet` instance that you are modifying.

The following code removes a `Planet` entity in which the `name` value is `"Pluto"`. It then calls the `SaveChanges()` method to delete that entity from the MongoDB collection.

```csharp
var planet = db.Planets.FirstOrDefault(p => p.name == "Pluto");db.Planets.Remove(planet);db.SaveChanges();
```

### Delete Multiple Entities

The `RemoveRange()` method accepts an array of entities to remove from the `DbSet`.

The following code finds two `Planet` entities and adds them to an array. It then uses the `RemoveRange()` method to remove both entities from the `DbSet`. Finally, it uses the `SaveChanges()` method to remove those entities from the MongoDB collection.

```csharp
var pluto = db.Planets.FirstOrDefault(p => p.name == "Pluto");var scadrial = db.Planets.FirstOrDefault(p => p.name == "Scadrial");var planets = new[] { pluto, scadrial };db.Planets.RemoveRange(planets);db.SaveChanges();
```

## Additional Information

To learn more about the methods discussed in this guide, see the following .NET API documentation links:

*   [SaveChanges()](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.saveChanges)
*   [Add()](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.add)
*   [AddRange()](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.addRange)
*   [Remove()](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.remove)
*   [RemoveRange()](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.removeRange)