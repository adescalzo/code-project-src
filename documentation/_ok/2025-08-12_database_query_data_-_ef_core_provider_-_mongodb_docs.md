```yaml
---
title: Query Data - EF Core Provider - MongoDB Docs
source: https://www.mongodb.com/docs/entity-framework/current/fundamentals/query-data/
date_published: unknown
date_captured: 2025-08-12T13:28:34.143Z
domain: www.mongodb.com
author: Unknown
category: database
technologies: [MongoDB, Entity Framework Core, MongoDB EF Core Provider, .NET]
programming_languages: [C#]
tags: [mongodb, entity-framework-core, data-access, querying, linq, csharp, orm, database, dotnet]
key_concepts: [Language-Integrated Query (LINQ), Object-Relational Mapping (ORM), Data querying, Data filtering, Data sorting, Database interaction]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a guide on querying data using the MongoDB EF Core Provider. It explains how to leverage Language-Integrated Query (LINQ) syntax in C# to perform database operations without writing explicit database commands. The guide demonstrates common query operations such as finding single or multiple entities using `FirstOrDefault()` and `Where()` methods. It also covers sorting data with `OrderBy()`, `OrderByDescending()`, `ThenBy()`, and `ThenByDescending()` methods, providing practical code examples for each. This content is ideal for developers looking to perform basic data retrieval with EF Core and MongoDB.]
---
```

# Query Data - EF Core Provider - MongoDB Docs

# Query Data

## Overview

Entity Framework Core allows you to work with data in your application without explicitly running database commands. To query your data, use the Language-Integrated Query (LINQ) syntax. LINQ allows you to write strongly typed queries using C#-specific keywords and operators. When you run the application, the EF Core Provider automatically translates the LINQ queries and runs them on the database using the MongoDB Query API.

In this guide you can see examples of common query operations on an application configured to use the EF Core Provider.

## Tip

To learn how to configure an application to use the EF Core Provider, see [Configure the EF Core Provider.](/docs/entity-framework/current/fundamentals/configure/#std-label-entity-framework-configure)

## Find Entities

Find a single entity by using the `FirstOrDefault()` method, or find multiple entities by using the `Where()` method.

### Find a Single Entity

The `FirstOrDefault()` method returns the first entity it finds in your collection that matches the search criteria, and returns `null` if no matching entities are found.

The following code uses the `FirstOrDefault()` method to find a planet with the `name` field of "Mercury" from a `DBSet` called `Planets` and prints the planet name to the console:

```csharp
var planet = db.Planets.FirstOrDefault(p => p.name == "Mercury");Console.WriteLine(planet.name);
```

### Find Multiple Entities

You can use the `Where()` method to retrieve multiple entities from your collections. `Where()` returns all entities that match the search criteria.

The following code uses the `Where()` method to find all planets that have the `hasRings` field set to `true` and prints the planet names to the console.

```csharp
var planets = db.Planets.Where(p => p.hasRings);foreach (var p in planets){    Console.WriteLine(p.name);}
```

## Sort Entities

Use the `OrderBy()` method to specify an order in which to return entities from a query. `OrderBy()` sorts the elements in ascending order based on a specified sort criteria.

The following code uses the `OrderBy()` method to find all planets and sort them by the value of the `orderFromSun` field in ascending order. It then prints the results to the console.

```csharp
var planetList = db.Planets.OrderBy(p => p.orderFromSun);foreach (var p in planetList){   Console.WriteLine(p.name);}
```

VIEW OUTPUT

```json
MercuryVenusEarthMarsJupiterSaturnUranusNeptune
```

## Tip

### **Sort in Descending Order**

You can sort the results of a query in descending order by using the `OrderByDescending()` method.

You can perform a secondary sort on your query by using the `ThenBy()` method. The `ThenBy()` method sorts the results of the `OrderBy()` method in ascending order based on a specified sort criteria. The `ThenBy()` method should be chained to the `OrderBy()` method.

## Tip

### **Secondary Sort in Descending Order**

You can perform a secondary sort in descending order by using the `ThenByDescending()` method.

The following code uses the `OrderBy()` and `ThenBy()` methods to find all planets and sort them by the `hasRings()` field, with a secondary sort on the `name` field.

```csharp
var planetList = db.Planets.OrderBy(o => o.hasRings).ThenBy(o => o.name);foreach (var p in planetList){   Console.WriteLine("Has rings: " + p.hasRings + ", Name: " + p.name);}
```

VIEW OUTPUT

```json
Has rings: False, Name: EarthHas rings: False, Name: MarsHas rings: False, Name: MercuryHas rings: False, Name: VenusHas rings: True, Name: JupiterHas rings: True, Name: NeptuneHas rings: True, Name: SaturnHas rings: True, Name: Uranus
```

## Tip

When sorting on fields with a boolean value, entities with a field value of `false` show before those with a value of `true`.

## Additional Information

To learn more about the methods discussed in this guide, see the following .NET API documentation links:

*   [FirstOrDefault()](https://learn.microsoft.com/en-us/dotnet/api/system.linq.queryable.firstordefault)
*   [Where()](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.queryablemethods.where)
*   [OrderBy()](https://learn.microsoft.com/en-us/dotnet/api/system.linq.queryable.orderby)
*   [OrderByDescending()](https://learn.microsoft.com/en-us/dotnet/api/system.linq.queryable.orderbydescending)
*   [ThenBy()](https://learn.microsoft.com/en-us/dotnet/api/system.linq.queryable.thenby)
*   [ThenByDescending()](https://learn.microsoft.com/en-us/dotnet/api/system.linq.queryable.thenbydescending)

[Back Configuration](/docs/entity-framework/current/fundamentals/configure/ "Previous Section")

[Next Write Data](/docs/entity-framework/current/fundamentals/write-data/ "Next Section")