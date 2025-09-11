```yaml
---
title: "IEnumerable vs IQueryable in C#: The Key Differences | by Sudhi | Sep, 2025 | Medium"
source: https://medium.com/@sudhisudhi0834/ienumerable-vs-iqueryable-in-c-the-key-differences-a0642f3b63ac
date_published: 2025-09-03T03:34:03.319Z
date_captured: 2025-09-08T11:25:23.418Z
domain: medium.com
author: Sudhi
category: programming
technologies: [IEnumerable, IQueryable, LINQ, SQL Server, Entity Framework, Dapper, OData, .NET]
programming_languages: [C#, SQL]
tags: [csharp, linq, ienumerable, iqueryable, database, performance, data-access, dotnet, deferred-execution, expression-trees]
key_concepts: [deferred-execution, expression-trees, client-side-execution, server-side-execution, query-optimization, in-memory-processing, database-querying, linq-providers]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article clarifies the fundamental differences between `IEnumerable` and `IQueryable` in C#, two interfaces commonly used for working with collections and queries. It highlights that `IEnumerable` executes queries in memory, fetching all data before filtering, which can be inefficient for database operations. In contrast, `IQueryable` builds an expression tree to push query logic down to the data source (like a database), ensuring only relevant data is retrieved. The article emphasizes the significant performance impact of choosing the correct interface, especially when interacting with databases, and provides clear guidelines on when to use each for optimal application efficiency.]
---
```

# IEnumerable vs IQueryable in C#: The Key Differences | by Sudhi | Sep, 2025 | Medium

# IEnumerable vs IQueryable in C#: The Key Differences

_If you’ve been coding in C# for a while, you’ve probably bumped into_ **_IEnumerable_** _and_ **_IQueryable_**_._

_At first glance, they look kinda similar, both let you work with collections and queries. But under the hood, they behave very differently. And if you pick the wrong one, your app might end up pulling a truckload of unnecessary data from the database 🚚📦._

![Illustration of a truck pulling a large amount of data, symbolizing inefficient data retrieval by IEnumerable.](https://miro.medium.com/v2/resize:fit:700/1*DT3QOvNeLaaCDSjkSNa3EQ.png)

Let’s break this down in the simplest way possible.

🔑 Quick Comparison

![Table providing a quick comparison of IEnumerable and IQueryable features.](https://miro.medium.com/v2/resize:fit:700/1*ZsNHRFNXxlC5cOUhmi2kfA.png)

## Where the Query Actually Runs

### IEnumerable: Runs in Memory

Think of `IEnumerable` as a “pull everything, then filter locally” approach.

Operations (`Where`, `Select`, etc.) run in memory (client-side).

```csharp
IEnumerable<int> nums = new List<int> { 1, 2, 3, 4, 5 };  
var filtered = nums.Where(x => x > 3).ToList(); // Runs in memory
```

✅ Great if your data is already in memory.  
❌ Terrible if it’s coming from a database, because it first pulls _all rows_ into memory, then applies filters. If you query a database, it pulls ALL data first, then filters in-memory (inefficient).

### IQueryable: Runs in Database

`IQueryable` builds an **expression tree** and pushes the filter down to the database (or any remote provider) / Builds an expression tree and executes the query remotely (e.g., SQL Server).

```csharp
IQueryable<int> nums = dbContext.Numbers.AsQueryable();  
var filtered = nums.Where(x => x > 3).ToList();   
// Translates to SQL: SELECT * FROM Numbers WHERE Value > 3
```

✅ Only the relevant data is fetched.  
✅ Database does the heavy lifting.  
❌ Limited to operations that the database understands (no fancy custom methods).

## Performance Impact

This is where the real difference shows.

❌ Inefficient with IEnumerable

```csharp
var badQuery = dbContext.Customers.AsEnumerable()  
                   .Where(c => c.Age > 30) // Runs in memory  
                   .ToList();
```

What happens here?  
👉 All customers are fetched from the DB, then filtered in memory.  
That’s like downloading the entire Netflix library 🎥 just to watch one movie.

✅Efficient with IQueryable

```csharp
var goodQuery = dbContext.Customers  
                   .Where(c => c.Age > 30)   
                   .ToList();
```

This becomes:

```sql
SELECT * FROM Customers WHERE Age > 30
```

👉 Only the required data is sent across. Much faster, much cleaner.

## Supported Operations

*   **IEnumerable**: Works with all LINQ methods, because everything is local. You can even use your own custom methods.
*   **IQueryable**: Only supports what can be converted to SQL (or another provider’s language).

Example:

```csharp
// Works with IEnumerable  
var result = customers.Where(c => SomeComplexMethod(c)).ToList();  
  
// FAILS with IQueryable (can’t translate SomeComplexMethod into SQL)  
var result = dbContext.Customers  
                      .Where(c => SomeComplexMethod(c))  
                      .ToList();
```

### ✅ When to Use What?

*   Use **IEnumerable** when:
    *   Data is already in memory (List<T>, Array, etc.)
    *   You need to use complex methods in your LINQ queries.
*   Use **IQueryable** when:
    *   You’re talking to a database (EF, Dapper, OData, etc.)
    *   You want the DB to optimize filtering, sorting, paging, etc.

### 📝 Quick Summary

*   Same results? Yes (in small cases).
*   Different execution? Absolutely.
*   Performance? IQueryable wins hands down for DB queries.

> 👉 Rule of thumb:  
> If you’re hitting a **database**, use `IQueryable`.  
> If you’re working with **in-memory collections**, use `IEnumerable`.