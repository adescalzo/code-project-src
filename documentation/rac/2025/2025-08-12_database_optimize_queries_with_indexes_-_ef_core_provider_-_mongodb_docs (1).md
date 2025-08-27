```yaml
---
title: Optimize Queries with Indexes - EF Core Provider - MongoDB Docs
source: https://www.mongodb.com/docs/entity-framework/current/fundamentals/indexes/
date_published: unknown
date_captured: 2025-08-12T13:29:08.303Z
domain: www.mongodb.com
author: Unknown
category: database
technologies: [MongoDB, EF Core Provider, Entity Framework Core, .NET/C# Driver]
programming_languages: [C#]
tags: [mongodb, entity-framework-core, indexes, query-optimization, data-access, dotnet, database, performance]
key_concepts: [indexes, single-field-index, compound-index, unique-index, sparse-index, query-optimization, data-modeling, onmodelcreating]
code_examples: false
difficulty_level: intermediate
summary: |
  [This guide details how to define and optimize queries using various types of indexes within the MongoDB EF Core Provider. It explains the importance of indexes in improving query efficiency by reducing the need for full collection scans. The article demonstrates how to create single-field, compound, and unique indexes using C# code examples within the `OnModelCreating` method. Additionally, it covers advanced index options such as custom naming, specifying index order, and applying MongoDB-specific configurations like sparse indexes. The content also clarifies that index modifications or deletions require direct interaction with the .NET/C# Driver.]
---
```

# Optimize Queries with Indexes - EF Core Provider - MongoDB Docs

[Docs Home](https://www.mongodb.com/docs/)

/

[Languages](https://www.mongodb.com/docs/drivers/)

/

[C#](https://www.mongodb.com/docs/languages/csharp/)

/

[EF Core Provider](/docs/entity-framework/current/)

/

[Fundamentals](/docs/entity-framework/current/fundamentals/)

# Optimize Queries with Indexes[

](#optimize-queries-with-indexes "Permalink to this heading")

Copy page

## Overview[

](#overview "Permalink to this heading")

In this guide, you can learn how to define **indexes** in the EF Core Provider. Indexes can improve the efficiency of queries and add additional functionality to querying and storing documents.

Without indexes, MongoDB must scan every document in a collection to find the documents that match each query. These collection scans are slow and can negatively affect the performance of your application. However, if you create an index that covers a query, MongoDB can use the index to limit the documents it must inspect.

To improve query performance, build indexes on fields that appear often in your application's queries and operations that return sorted results. Each index that you add consumes disk space and memory when active, so we recommend that you track index memory and disk usage for capacity planning.

## Create an Index[

](#create-an-index "Permalink to this heading")

The EF Core Provider supports the following types of indexes:

*   [Single field indexes](#std-label-entity-framework-single-field-index)
    
*   [Compound indexes](#std-label-entity-framework-compound-index)
    
*   [Unique indexes](#std-label-entity-framework-unique-index)
    

The examples in this guide use the sample application created in the [Quick Start](/docs/entity-framework/current/quick-start/#std-label-entity-framework-quickstart) guide. After you set up the quick start application, you can run the examples in this guide by adding the code to the `OnModelCreating()` method of the `PlanetDbContext`, as shown in the following example:

{"@context":"https://schema.org","@type":"SoftwareSourceCode","codeSampleType":"code snippet","text":"protected override void OnModelCreating(ModelBuilder modelBuilder)\\n{\\n base.OnModelCreating(modelBuilder);\\n\\n // Paste example code here\\n\\n}","programmingLanguage":"C#"}

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder){    base.OnModelCreating(modelBuilder);    // Paste example code here}
```

## Note

The EF Core Provider creates your indexes when you call the `dbContext.Database.EnsureCreated()` method. You cannot modify or delete indexes by using the provider after they are created. If you need to modify or delete an index in your application, you must use the .NET/C# Driver directly.

To learn more about working with indexes in the driver, see the [Indexes](https://www.mongodb.com/docs/drivers/csharp/current/fundamentals/indexes/) guide in the .NET/C# Driver documentation.

The following sections show how to create each of the preceding types of indexes.

### Single Field Index[

](#single-field-index "Permalink to this heading")

[Single Field Indexes](https://www.mongodb.com/docs/manual/core/indexes/index-types/index-single/) are indexes with a reference to a single field within a collection's documents. They improve single field query and sort performance. The `_id_` index is an example of a single field index.

You can create a single field index by calling the `HasIndex()` method with a lambda expression that specifies the field to index. The following example creates a single field index on a `Planet` entity:

{"@context":"https://schema.org","@type":"SoftwareSourceCode","codeSampleType":"code snippet","text":"modelBuilder.Entity&lt;planet&gt;(p =&gt;\\n{\\n p.HasIndex(p =&gt; p.orderFromSun);\\n p.ToCollection(\\"planets\\");\\n});","programmingLanguage":"C#"}

```csharp
modelBuilder.Entity<Planet>(p =>{    p.HasIndex(p => p.orderFromSun);    p.ToCollection("planets");});
```

### Compound Index[

](#compound-index "Permalink to this heading")

[Compound indexes](https://www.mongodb.com/docs/manual/core/indexes/index-types/index-compound/) are indexes that cover multiple fields within a collection's documents. These indexes improve multi-field query and sort performance.

You can create a compound index by calling the `HasIndex()` method with a lambda expression that specifies the fields to index. The following example creates a compound index on a `Planet` entity:

{"@context":"https://schema.org","@type":"SoftwareSourceCode","codeSampleType":"code snippet","text":"modelBuilder.Entity&lt;planet&gt;(p =&gt;\\n{\\n p.HasIndex(p =&gt; new { p.orderFromSun, p.name });\\n p.ToCollection(\\"planets\\");\\n});","programmingLanguage":"C#"}

```csharp
modelBuilder.Entity<Planet>(p =>{    p.HasIndex(p => new { p.orderFromSun, p.name });    p.ToCollection("planets");});
```

### Unique Index[

](#unique-index "Permalink to this heading")

Unique indexes ensure that multiple documents don't contain the same value for the indexed field. By default, MongoDB creates a unique index on the `_id` field during the creation of a collection. You cannot modify or remove this index.

You can create a unique index by creating an index by using the `HasIndex()` methods as shown in the preceding sections, then chaining the `IsUnique()` method. The following example creates a unique index on a `Planet` entity:

{"@context":"https://schema.org","@type":"SoftwareSourceCode","codeSampleType":"code snippet","text":"modelBuilder.Entity&lt;planet&gt;(p =&gt;\\n{\\n p.HasIndex(p =&gt; p.orderFromSun).IsUnique();\\n p.ToCollection(\\"planets\\");\\n});","programmingLanguage":"C#"}

```csharp
modelBuilder.Entity<Planet>(p =>{    p.HasIndex(p => p.orderFromSun).IsUnique();    p.ToCollection("planets");});
```

## Index Options[

](#index-options "Permalink to this heading")

You can specify options when creating your index to customize the index name, properties, or index type. The following sections describe some of the options that you can specify.

### Named Index[

](#named-index "Permalink to this heading")

By default, MongoDB creates an index with a generated name based on the fields and options for the index. To specify a custom name for the index, pass in the name as a string when you create the index. The following example creates a compound index on the `Planet` entity with the name `"named_order"`:

{"@context":"https://schema.org","@type":"SoftwareSourceCode","codeSampleType":"code snippet","text":"modelBuilder.Entity&lt;planet&gt;(p =&gt;\\n{\\n p.HasIndex(p =&gt; new { p.orderFromSun, p.name }, \\"named\_order\\");\\n p.ToCollection(\\"planets\\");\\n});","programmingLanguage":"C#"}

```csharp
modelBuilder.Entity<Planet>(p =>{    p.HasIndex(p => new { p.orderFromSun, p.name }, "named_order");    p.ToCollection("planets");});
```

### Index Order[

](#index-order "Permalink to this heading")

By default, MongoDB creates indexes in ascending order. You can call the `IsDescending()` method when creating a new index to create the index in descending order. The following example creates a descending index on a `Planet` entity:

{"@context":"https://schema.org","@type":"SoftwareSourceCode","codeSampleType":"code snippet","text":"modelBuilder.Entity&lt;planet&gt;(p =&gt;\\n{\\n p.HasIndex(p =&gt; p.orderFromSun).IsDescending();\\n p.ToCollection(\\"planets\\");\\n});","programmingLanguage":"C#"}

```csharp
modelBuilder.Entity<Planet>(p =>{    p.HasIndex(p => p.orderFromSun).IsDescending();    p.ToCollection("planets");});
```

## Note

Using a descending single field index might negatively impact index performance. For best performance, use only ascending indexes.

### MongoDB-Specific Options[

](#mongodb-specific-options "Permalink to this heading")

You can specify additional MongoDB-specific options when creating an index by using the `HasCreateIndexOptions()` method and passing in an instance of the `CreateIndexOptions` class of the .NET/C# Driver. You can pass in any options that the `CreateIndexOptions` class supports. To learn more about the supported options, see the [CreateIndexOptions](https://mongodb.github.io/mongo-csharp-driver/3.0.0/api/MongoDB.Driver/MongoDB.Driver.CreateIndexOptions.html) API documentation.

The following example creates an index and specifies the `Sparse` option to create a [Sparse Index:](https://www.mongodb.com/docs/manual/core/index-sparse/)

{"@context":"https://schema.org","@type":"SoftwareSourceCode","codeSampleType":"code snippet","text":"modelBuilder.Entity&lt;planet&gt;(p =&gt;\\n{\\n p.HasIndex(p =&gt; p.orderFromSun)\\n .HasCreateIndexOptions(new CreateIndexOptions() { Sparse = true });\\n p.ToCollection(\\"planets\\");\\n});","programmingLanguage":"C#"}

```csharp
modelBuilder.Entity<Planet>(p =>{    p.HasIndex(p => p.orderFromSun)        .HasCreateIndexOptions(new CreateIndexOptions() { Sparse = true });    p.ToCollection("planets");});
```

## Additional Information[

](#additional-information "Permalink to this heading")

For more information about indexes in MongoDB, see the [Indexes](https://www.mongodb.com/docs/manual/core/indexes/) guide in the MongoDB Server manual.

[

Back

Optimistic Concurrency

](/docs/entity-framework/current/fundamentals/optimistic-concurrency/ "Previous Section")

[

Next

Limitations

](/docs/entity-framework/current/limitations/ "Next Section")