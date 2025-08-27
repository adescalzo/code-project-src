```yaml
---
title: Understanding IEnumerable and IQueryable in C#
source: https://okyrylchuk.dev/blog/understanding-ienumerable-and-iqueryable-in-csharp/
date_published: 2024-11-08T16:49:04.000Z
date_captured: 2025-08-12T11:27:07.081Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: programming
technologies: [Entity Framework, SQL]
programming_languages: [C#, SQL]
tags: [csharp, linq, ienumerable, iqueryable, performance, data-access, entity-framework, collections, database]
key_concepts: [IEnumerable, IQueryable, deferred-execution, client-side-processing, server-side-processing, LINQ-providers, in-memory-collections, database-querying]
code_examples: false
difficulty_level: intermediate
summary: |
  This article clarifies the distinctions between IEnumerable and IQueryable interfaces in C#, crucial for optimizing data operations. It explains that IEnumerable is suited for in-memory collections, processing data on the client side after loading the entire dataset. In contrast, IQueryable is designed for external data sources like databases, enabling LINQ providers such as Entity Framework to translate queries into native SQL for efficient server-side execution. The piece emphasizes that IQueryable minimizes memory usage and network traffic by fetching only necessary data, making it the preferred choice for large datasets and database interactions.
---
```

# Understanding IEnumerable and IQueryable in C#

# Understanding IEnumerable and IQueryable in C#

When working with collections in C#, two interfaces frequently come into play: **IEnumerable** and **IQueryable**. Understanding the differences between these interfaces is essential to optimize performance, especially when working with databases or large data sets. In this post, we’ll explore what makes these interfaces unique, when to use each, and how they impact the efficiency of your code.

## **What is IEnumerable?**

**IEnumerable** is an interface primarily used for working with in-memory collections like arrays, lists, and other data structures already loaded into memory.

**Key Characteristics of IEnumerable**:

*   **Namespace**: System.Collections.Generic
*   **Execution**: Deferred execution. Operations like **Where** or **Select** are evaluated only when you iterate over the collection (e.g., in a foreach loop).
*   **Client-Side Processing**: When you perform filtering, transformations, or projections, the entire collection is loaded into memory, and then the filtering happens on the client side.

**Best Use Cases for IEnumerable**:

*   **In-Memory Collections**: **IEnumerable** works well for collections already loaded in memory.
*   **Small Data Sets**: Suitable for relatively small collections since operations are done in memory and can consume resources if the collection is large.

## **What is IQueryable?**

**IQueryable** is an interface designed for working with data sources like databases where LINQ providers (like Entity Framework) can translate queries into SQL or other query languages.

**Key Characteristics of IQueryable**:

*   **Namespace**: System.Linq
*   **Execution**: Deferred execution, but with a difference – the query is only executed when the data is needed.
*   **Server-Side Processing**: With IQueryable, the LINQ provider (e.g., Entity Framework) translates LINQ queries into the native query language of the data source, optimizing performance by retrieving only the necessary data.

**Best Use Cases for IQueryable**:

*   **Database Querying**: Ideal for querying databases, as it fetches only the data you need, minimizing memory usage and network traffic.
*   **Large Data Sets**: Use IQueryable when working with large data sets, as it can optimize filtering, sorting, and paging directly in the database.

## **Example with Entity Framework**

Let’s create a simple entity, Person.

```csharp
public class Person
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
    public string Country { get; set; }
}
```

When we use **IQueryable**, the filtering happens on the database side.

```csharp
var people = await context.People
    .Where(p => p.Country == "PL")
    .ToListAsync();
```

The EF will generate the following SQL:

```sql
SELECT [p].[Id], [p].[Country], [p].[Name], [p].[Surname]
FROM [People] AS [p]
WHERE [p].[Country] = N'PL'
```

When we use **IEnumerable**, the EF fetches all persons from the database, and the filtering happens in memory on the client’s side.

```csharp
var people = context.People
    .AsEnumerable()
    .Where(p => p.Country == "PL")
    .ToList();
```

The EF will generate the following SQL:

```sql
SELECT [p].[Id], [p].[Country], [p].[Name], [p].[Surname]
FROM [People] AS [p]
```

## **Key Differences Between IEnumerable and IQueryable**

| Feature           | IEnumerable             | IQueryable                       |
| :---------------- | :---------------------- | :------------------------------- |
| Execution         | In-memory               | Database or external source      |
| Evaluation        | Immediate or deferred execution | Deferred execution               |
| Use Case          | Small, in-memory collections | Large data sources (e.g., databases) |
| LINQ Provider     | No translation to SQL   | Translates to data source query  |

The choice between **IEnumerable** and **IQueryable** affects not just performance but also how and where data processing occurs. Using **IQueryable** for in-memory data will result in no real performance gain, as there’s no external source to optimize against. Likewise, using **IEnumerable** for database data may lead to inefficient memory usage and high network traffic, as the full dataset is loaded before filtering.

## Summary

Use **IQueryable** for efficient, server-side querying and to leverage database optimizations.

Use **IEnumerable** when you need in-memory processing, are working with small data sets, or need to mix data sources.

In most cases, **IQueryable** is the preferred choice with Entity Framework Core to keep queries efficient and optimized.