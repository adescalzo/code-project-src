```yaml
---
title: Single vs. Split Query in Entity Framework
source: https://okyrylchuk.dev/blog/single-vs-split-query-in-entity-framework/
date_published: 2024-03-08T20:06:25.000Z
date_captured: 2025-08-20T18:56:24.846Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [Entity Framework, SQL Server, .NET]
programming_languages: [C#, SQL]
tags: [entity-framework, database, performance, query-optimization, data-access, dotnet, orm, sql-server, join, cartesian-product]
key_concepts: [Cartesian Explosion, Query Splitting, Global Query Splitting, Data Duplication, Performance Issues, Database Joins, Transaction Isolation Levels, Eager Loading]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores the performance implications of Entity Framework's default single query behavior when loading related entities. It details how multiple JOINs can lead to a "Cartesian explosion," causing excessive data transfer and performance bottlenecks. The author introduces "Query Splitting" using `AsSplitQuery()` as a solution to generate separate queries, thereby avoiding the Cartesian product. The article also covers configuring global query splitting behavior and discusses potential data consistency issues with split queries, suggesting transaction isolation levels as a mitigation. Finally, it highlights how data duplication, even with a single `Include`, can also cause performance problems.]
---
```

# Single vs. Split Query in Entity Framework

# Single vs. Split Query in Entity Framework

![Oleg Kyrylchuk's avatar](https://okyrylchuk.dev/wp-content/litespeed/avatar/a85a89199a7d4486a29f67dc2fc371d0.jpg?ver=1755571532)By [Oleg Kyrylchuk](https://okyrylchuk.dev/blog/author/kyrylchuk-oleggmail-com/ "View all posts by Oleg Kyrylchuk") / March 8, 2024

Entity Framework generates single queries using JOINs for related entities. JOINs can create significant performance issues in some scenarios.

EF can split a single query into a few to eliminate such performance issues.

Let’s look at these performance issues and how splitting queries can resolve them.

## Table Of Contents

1.  [Cartesian Explosion](#cartesian-explosion)
2.  [Query Splitting](#query-splitting)
3.  [Global Query Splitting](#global-query-splitting)
4.  [Data duplication](#data-duplication)

## Cartesian Explosion

Let’s assume such entity models. There is a Department that can contain lists of Employees and Projects.

```csharp
public class Employee
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DepartmentId { get; set; }
}

public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int DepartmentId { get; set; }
}

public class Department
{
    public int Id { get; set; }
    public string Name { get; set; }
    public IList<Employee> Employees { get; set; } = new List<Employee>();
    public IList<Project> Projects { get; set; } = new List<Project>();
}
```

Now, we want to select all Departments, including Employees and Projects.

```csharp
var departments = context.Departments
    .Include(d => d.Employees)
    .Include(d => d.Projects)
    .ToListAsync();
```

EF generates the following SQL with two LEFT JOINs.

![SQL query showing two LEFT JOINs for Departments, Employees, and Projects, leading to a Cartesian product.](https://ci3.googleusercontent.com/meips/ADKq_NaAteZj50ltdlK_COpm7dn9w3X2an0QrrQjsDAO7piMqcQFqIK_6nNij630ErqD_e0q98kgEtRaDf8NDPX1w0YHq9jbE9t85TyubSO2iZOwIJeD0xfhqrJEIQwWjq75B5dTSIYES7KXcqfsCqfDqeRWyuvFSjM1Btk-qUK0lIFRkS03ONQL-vpCim4=s0-d-e1-ft#https://gallery.eocampaign1.com/53887302-b7d9-11ee-b61e-9583dbb2845e%2Fmedia-manager%2F1709761043464-cartesiansql.png)

Since Employees and Projects are related collections of Departments at the same level, the relational database produces a cross product. It means that each row from Employees is joined with each row from Projects.

Having 10 Projects and 10 Employees for a given Department, the database returns 100 rows for each Department.

It’s called a Cartesian explosion. It refers to a situation where a query produces an unexpectedly large number of results due to unintended cartesian products (cross joins) between tables.

All this unintended data is transferred to the client. If there is a lot of data in the database or we include even more related data at the same level, the performance issue could be significant.

**Note**: Cartesian explosion does not occur when two JOINs aren’t at the same level (when you use the ThenInclude method after the Include method).

## Query Splitting

Splitting query resolves the issue with the Cartesian explosion. EF generates several separate queries to avoid the problem.

```csharp
var departments = context.Departments
    .Include(d => d.Employees)
    .Include(d => d.Projects)
    .AsSplitQuery()
    .ToListAsync();
```

EF generates three separate queries. The first query selects Departments. The other two include Projects and Employees with INNER JOINs separately.

![Three separate SQL queries generated by Entity Framework's AsSplitQuery, showing one query for Departments and two separate queries for Employees and Projects using INNER JOINs.](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My4wcmcvMjAwMC9zdmciIHdpZHRoPSI4MzkiIGhlaWdodD0iNDM0IiB2aWV3Qm94PSIwIDAgODM5IDQzNCI+PHJlY3Qgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgc3R5bGU9ImZpbGw6I2NmZDRkYjtmaWxsLW9wYWNpdHk6IDAuMTsiLz48L3N2Zz4=)

No Cartesian explosion.

However, it’s three separate queries, three round-trips to a database. It can result in inconsistent results when concurrent updates occur. You can use Serializable or Snapshot transaction isolation levels to mitigate the problem with data consistency. However, it may bring other performance and behavioral differences.

## Global Query Splitting

You can configure split queries as the default behavior for your database context.

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    => optionsBuilder
        .UseSqlServer("[ConnectionString]",
            o => o.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
```

With such a configuration, you can still execute specific queries as a single query.

```csharp
var departments = context.Departments
    .Include(d => d.Employees)
    .Include(d => d.Projects)
    .AsSingleQuery()
    .ToListAsync();
```

## Data duplication

Let’s go back to our first query, do only one Include, and see the SQL generated by EF.

```csharp
var departments = context.Departments
    .Include(d => d.Employees)
    .ToQueryString();
```

SQL:

![SQL query showing a single LEFT JOIN for Departments and Employees, illustrating how Department columns are duplicated for each Employee row.](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My4wcmcvMjAwMC9zdmciIHdpZHRoPSI5MzIiIGhlaWdodD0iMTMyIiB2aWV3Qm94PSIwIDAgOTMyIDEzMiI+PHJlY3Qgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgc3R5bGU9ImZpbGw6I2NmZDRkYjtmaWxsLW9wYWNpdHk6IDAuMTsiLz48L3N2Zz4=)

Department columns (Name column) repeat for every Employee row. This is usually normal and causes no issues.

However, if our Department table has a big column (e.g., binary data, huge text, etc.), then this big column will be duplicated for each Employee row. It can also cause performance issues similar to a Cartesian explosion. A splitting query is also a good choice in such cases.