```yaml
---
title: Quick Reference - EF Core Provider - MongoDB Docs
source: https://www.mongodb.com/docs/entity-framework/current/quick-reference/
date_published: unknown
date_captured: 2025-08-12T13:27:18.691Z
domain: www.mongodb.com
author: Unknown
category: database
technologies: [MongoDB, Entity Framework Core, .NET]
programming_languages: [C#, JSON]
tags: [mongodb, entity-framework-core, orm, data-access, .net, csharp, database, nosql, crud, query]
key_concepts: [orm, dbcontext, crud-operations, querying-data, data-modeling, connection-management, linq]
code_examples: false
difficulty_level: intermediate
summary: |
  [This document provides a quick reference guide for interacting with MongoDB using the Entity Framework Core Provider in C#. It covers fundamental operations such as configuring a DbContext and creating its instances. The guide demonstrates common CRUD operations including finding, inserting, updating, and deleting single or multiple entities. Additionally, it illustrates various data querying techniques like ordering, skipping, and taking a specified number of results. Each section includes practical C# code snippets and links to relevant API documentation for further details.]
---
```

# Quick Reference - EF Core Provider - MongoDB Docs

# Quick Reference

This page shows the provider syntax for several commands and links to their related API documentation.

Command

Syntax

**Configure a DBContext**

[API Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext)

[Fundamentals](/docs/entity-framework/current/fundamentals/configure/#std-label-entity-framework-configure)

```csharp
public class PlanetDbContext : DbContext{    public DbSet<Planet> Planets { get; init; }    public static PlanetDbContext Create(IMongoDatabase database) =>        new(new DbContextOptionsBuilder<PlanetDbContext>()            .UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName)            .Options);    public PlanetDbContext(DbContextOptions options)        : base(options)    {    }    protected override void OnModelCreating(ModelBuilder modelBuilder)    {        base.OnModelCreating(modelBuilder);        modelBuilder.Entity<Planet>().ToCollection("planets");    }}
```

**Create an Instance of the DBContext**

[Fundamentals](/docs/entity-framework/current/fundamentals/configure/#std-label-entity-framework-db-context)

```csharp
// Replace the placeholder with your connection URIvar client = new MongoClient("<Your connection URI>");var db = PlanetDbContext.Create(client.GetDatabase("sample_planets"));
```

**Find an Entity**

[API Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.queryablemethods.firstordefaultwithpredicate)

[Fundamentals](/docs/entity-framework/current/fundamentals/query-data/#std-label-entity-framework-find-one)

```csharp
var planet = db.Planets.FirstOrDefault(p => p.name == "Mercury");Console.WriteLine(planet.name);
```

VIEW OUTPUT

```json
Mercury
```

**Find Multiple Entities**

[API Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.queryablemethods.where)

[Fundamentals](/docs/entity-framework/current/fundamentals/query-data/#std-label-entity-framework-find-multiple)

```csharp
var planets = db.Planets.Where(p => p.hasRings == true);foreach (var p in planets){   Console.WriteLine(p.name);}
```

VIEW OUTPUT

```json
NeptuneUranusSaturnJupiter
```

**Insert an Entity**

[API Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.add)

[Fundamentals](/docs/entity-framework/current/fundamentals/write-data/#std-label-entity-framework-insert-one)

```csharp
db.Planets.Add(new Planet(){    name = "Pluto",    hasRings = false,    orderFromSun = 9});db.SaveChanges();
```

**Insert Multiple Entities**

[API Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.addrange)

[Fundamentals](/docs/entity-framework/current/fundamentals/write-data/#std-label-entity-framework-insert-multiple)

```csharp
var planets = new[]{    new Planet()    {        _id = ObjectId.GenerateNewId(),        name = "Pluto",        hasRings = false,        orderFromSun = 9    },    new Planet()    {        _id = ObjectId.GenerateNewId(),        name = "Scadrial",        hasRings = false,        orderFromSun = 10    }};db.Planets.AddRange(planets);db.SaveChanges();
```

**Update an Entity**

[API Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.update)

[Fundamentals](/docs/entity-framework/current/fundamentals/write-data/#std-label-entity-framework-update)

```csharp
var planet = db.Planets.FirstOrDefault(p => p.name == "Mercury");planet.name = "Mercury the first planet";db.SaveChanges();
```

**Update Multiple Entities**

[API Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.updaterange)

[Fundamentals](/docs/entity-framework/current/fundamentals/write-data/#std-label-entity-framework-update)

```csharp
var planets = db.Planets.Where(p => p.orderFromSun > 0);foreach (var p in planets){    p.orderFromSun++;}db.SaveChanges();
```

**Delete an Entity**

[API Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.remove)

[Fundamentals](/docs/entity-framework/current/fundamentals/write-data/#std-label-entity-framework-delete-one)

```csharp
var planet = db.Planets.FirstOrDefault(p => p.name == "Pluto");db.Planets.Remove(planet);db.SaveChanges();
```

**Delete Multiple Entities**

[API Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbcontext.removerange)

[Fundamentals](/docs/entity-framework/current/fundamentals/write-data/#std-label-entity-framework-delete-multiple)

```csharp
var pluto = db.Planets.FirstOrDefault(p => p.name == "Pluto");var scadrial = db.Planets.FirstOrDefault(p => p.name == "Scadrial");var planets = new[] { pluto, scadrial };db.Planets.RemoveRange(planets);db.SaveChanges();
```

**Specify the Order in Which to Retrieve Entities**

[API Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.queryablemethods.orderby)

[Fundamentals](/docs/entity-framework/current/fundamentals/query-data/#std-label-entity-framework-sort)

```csharp
var planetList = db.Planets.OrderBy(p => p.orderFromSun);foreach (var p in planetList){    Console.WriteLine(p.name);}
```

VIEW OUTPUT

```json
MercuryVenusEarthMarsJupiterSaturnUranusNeptune
```

**Specify Multiple Orderings to Retrieve Entities**

[API Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.queryablemethods.thenby)

[Fundamentals](/docs/entity-framework/current/fundamentals/query-data/#std-label-entity-framework-sort)

```csharp
var planetList = db.Planets.OrderBy(o => o.hasRings).ThenBy(o => o.name);foreach (var p in planetList){    Console.WriteLine("Has rings: " + p.hasRings + ", Name: " + p.name);}
```

VIEW OUTPUT

```json
Has rings: False, Name: EarthHas rings: False, Name: MarsHas rings: False, Name: MercuryHas rings: False, Name: VenusHas rings: True, Name: JupiterHas rings: True, Name: NeptuneHas rings: True, Name: SaturnHas rings: True, Name: Uranus
```

**Specify the Number of Entities to Retrieve**

[API Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.queryablemethods.take)

```csharp
var planetList = db.Planets.Take(3);foreach (var p in planetList){    Console.WriteLine(p.name);}
```

VIEW OUTPUT

```json
NeptuneMercuryMars
```

**Specify the Number of Entities to Skip when Retrieving**

[API Documentation](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.query.queryablemethods.skip)

```csharp
var planetList = db.Planets.OrderBy(p => p.orderFromSun).Skip(5);foreach (var p in planetList){    Console.WriteLine(p.name);}
```

VIEW OUTPUT

```json
SaturnUranusNeptune
```