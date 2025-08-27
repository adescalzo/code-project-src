```yaml
---
title: Ensure Data Consistency with Optimistic Concurrency - EF Core Provider - MongoDB Docs
source: https://www.mongodb.com/docs/entity-framework/current/fundamentals/optimistic-concurrency/
date_published: unknown
date_captured: 2025-08-12T13:28:55.603Z
domain: www.mongodb.com
author: Unknown
category: database
technologies: [MongoDB, EF Core Provider, Entity Framework Core, .NET]
programming_languages: [C#]
tags: [optimistic-concurrency, entity-framework-core, mongodb, data-consistency, concurrency-tokens, row-versioning, database, dotnet]
key_concepts: [optimistic-concurrency-control, concurrency-tokens, row-versioning, data-consistency, DbContext, data-annotations, fluent-api]
code_examples: false
difficulty_level: intermediate
summary: |
  [This guide explains how to implement optimistic concurrency control using the EF Core Provider for MongoDB. It details two primary methods: concurrency tokens and row versioning. Concurrency tokens involve tracking a specified property's value, typically using the `ConcurrencyCheck` attribute or `IsConcurrencyToken()` fluent API. Row versioning uses a version field that automatically increments on changes, configured with the `Timestamp` attribute or `IsRowVersion()` fluent API. The article provides C# code examples for both approaches and highlights limitations when integrating with other applications.]
---
```

# Ensure Data Consistency with Optimistic Concurrency - EF Core Provider - MongoDB Docs

# Ensure Data Consistency with Optimistic Concurrency

## Overview

In this guide, you can learn how to use **optimistic concurrency control** with the EF Core Provider. Optimistic concurrency control ensures that data is not overwritten between the time an application reads it and the time the application writes it back to the database. The EF Core Provider supports two ways to implement optimistic concurrency control:

*   Concurrency tokens, by using the `ConcurrencyCheck` attribute or the `IsConcurrencyToken()` fluent API method
*   Row versioning, by using the `Timestamp` attribute or the `IsRowVersion()` fluent API method

## Tip

We recommend ensuring optimistic concurrency by using only one of the preceding implementations per entity.

## Concurrency Tokens

You can ensure optimistic concurrency on a specified property by using a concurrency token. When querying the entity, the EF Core Provider tracks the concurrency token. Then, when the provider calls the `SaveChanges()` or `SaveChangesAsync()` method, it compares the value of the concurrency token the value saved in the database to ensure that the original value hasn't changed.

You can configure a concurrency token by specifying the `ConcurrencyCheck` attribute when defining a class. The following example shows how to specify the `ConcurrencyCheck` attribute on the `LastModified` property of a `Customer` class:

```csharp
public class Customer
{
    public ObjectId Id { get; set; }
    public String Name { get; set; }
    public String Order { get; set; }
    [ConcurrencyCheck]
    public DateTimeOffset LastModified { get; set; }
}
```

You can also configure a concurrency token by using the `IsConcurrencyToken()` method. Call the `IsConcurrencyToken()` method in the `OnModelCreating()` method of the `DbContext` class, as shown in the following example:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.Entity<Customer>()
       .Property(p => p.LastModified)
       .IsConcurrencyToken();
}
```

The EF Core Provider supports setting concurrency tokens on any property type supported by the driver. You can also set multiple concurrency tokens on a single entity. If you need to update a concurrency token, you must do so manually.

## Row Versioning

You can ensure optimistic concurrency by using row versioning. Row versioning allows you to track changes to an entity by specifying a version field that increments automatically when the entity changes. You can configure row versioning by specifying the `Timestamp` attribute when defining a class. The following example shows how to specify the `Timestamp` attribute on the `Version` property of a `Customer` class:

```csharp
public class Customer
{
    public ObjectId Id { get; set; }
    public String Name { get; set; }
    public String Order { get; set; }
    [Timestamp]
    public long Version { get; set; }
}
```

You can also configure row versioning by using the `IsRowVersion()` method. Call the `IsRowVersion()` method in the `OnModelCreating()` method of the `DbContext` class, as shown in the following example:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.Entity<Customer>()
        .Property(p => p.Version)
        .IsRowVersion();
}
```

The EF Core Provider supports row versioning on only a single property of an entity. The property must be of type `long`, `int`, `ulong`, or `uint`.

## Limitations

Consider the following limitations when configuring optimistic concurrency control on a database that shares data with other applications:

*   Other applications must support the same mechanisms you are using for optimistic concurrency control in your Entity Framework Core application.
*   Other applications must support checks for concurrency tokens and row version fields during any update or delete operation.
*   If you are using row versioning, other applications must map row versioned fields to the property's name followed by the string: `_version`. The application must increment the field's value by 1 for each update.

## Additional Information

For more information about optimistic concurrency control with Entity Framework Core, see [Optimistic Concurrency](https://learn.microsoft.com/en-us/ef/core/saving/concurrency?tabs=data-annotations#optimistic-concurrency) in the Microsoft Entity Framework Core documentation.