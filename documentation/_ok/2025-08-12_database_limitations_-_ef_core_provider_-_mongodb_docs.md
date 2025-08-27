```yaml
---
title: Limitations - EF Core Provider - MongoDB Docs
source: https://www.mongodb.com/docs/entity-framework/current/limitations/
date_published: unknown
date_captured: 2025-08-12T13:29:22.553Z
domain: www.mongodb.com
author: Unknown
category: database
technologies: [MongoDB, Entity Framework Core, LINQ, MongoDB Atlas]
programming_languages: [C#]
tags: [mongodb, entity-framework-core, data-access, orm, limitations, document-database, csharp, database]
key_concepts: [orm, document-database, relational-database, linq-queries, migrations, database-first-development, scalar-aggregations, select-projections]
code_examples: false
difficulty_level: intermediate
summary: |
  [This document details the current limitations of the MongoDB EF Core Provider, outlining features that are not yet supported. It categorizes these limitations into unsupported Entity Framework Core features and unsupported MongoDB-specific features. Key EF Core features like select projections, most scalar aggregations, migrations, and foreign keys are not supported due to MongoDB's document-oriented nature. Additionally, MongoDB-exclusive features such as time series, Atlas Search, and Vector Search are also unsupported by the provider. The article notes that the provider is under active development, with potential future support for some features based on user demand.]
---
```

# Limitations - EF Core Provider - MongoDB Docs

# Limitations

## Overview

On this page, you can find a list of Entity Framework and MongoDB features that the EF Core Provider does not support. Because the provider is in active development, some features listed on this page might be considered for future releases based on customer demand.

## Unsupported Entity Framework Core Features

The following sections describe Entity Framework Core features that the EF Core Provider does not support.

### Select Projections

Select projections use the `Select()` method in a LINQ query to change the structure of the created object. The projection changes the object by performing groupings, or selecting fields into anonymous types or alternative types not recognized by Entity Framework.

This version of the EF Core Provider does not support Select Projections.

### Scalar Aggregations

Top-level scalar aggregations are operations you can perform on a query, such as `Count()`, `Min()`, and `Max()`. This version of the EF Core Provider supports only the following scalar aggregation operations:

*   `Count()`
*   `LongCount()`
*   `Any()`, with or without predicates

This version of the EF Core Provider does not support other scalar aggregation operations.

### Migrations

Migrations in Entity Framework Core are designed for relational databases. Because MongoDB is a document database, migrations are not supported by the EF Core Provider.

### Database-First Development

MongoDB is designed to be flexible and does not require a database schema. Because of MongoDB's schema-flexible design, database-first development is not supported in the EF Core Provider.

### Foreign Keys

Because MongoDB is a document database, the EF Core Provider does not support foreign keys.

### Alternate Keys

Because MongoDB is a document database, the EF Core Provider does not support alternate keys.

### Table Splitting

MongoDB is a document database and does not have normalized tables. Because of this, table splitting is not supported by the EF Core Provider.

### Temporal Tables

The EF Core Provider does not support temporal tables.

### Spatial Data

The EF Core Provider does not support spatial data.

## Unsupported MongoDB Features

The following sections describe MongoDB features that the EF Core Provider does not support.

### Time Series

The EF Core Provider does not support time series data or time series collections. These are MongoDB-exclusive features that are not available in Entity Framework Core.

### Atlas Search

The EF Core Provider does not support MongoDB Atlas search. Atlas Search is a MongoDB-exclusive feature with no similar feature available in Entity Framework Core.

### Vector Search

The EF Core Provider does not support MongoDB Atlas Vector search. Atlas Vector Search is a MongoDB-exclusive feature with no similar feature available in Entity Framework Core.