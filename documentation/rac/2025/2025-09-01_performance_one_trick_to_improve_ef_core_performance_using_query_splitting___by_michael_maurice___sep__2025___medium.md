```yaml
---
title: "One Trick To Improve EF Core Performance Using Query Splitting | by Michael Maurice | Sep, 2025 | Medium"
source: https://medium.com/@michaelmaurice410/one-trick-to-improve-ef-core-performance-using-query-splitting-17133e04a7a5
date_published: 2025-09-01T17:01:39.999Z
date_captured: 2025-09-11T11:29:09.370Z
domain: medium.com
author: Michael Maurice
category: performance
technologies: [Entity Framework Core, .NET, SQL Server, BenchmarkDotNet, Microsoft.Extensions.Logging]
programming_languages: [C#, SQL]
tags: [ef-core, performance-optimization, query-splitting, database, orm, csharp, data-access, sql-server, benchmarking, cartesian-explosion]
key_concepts: [Query Splitting, Cartesian Explosion, ORM, Performance Benchmarking, Include statements, Global Configuration, Repository Pattern, SQL Query Analysis]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details how to significantly improve Entity Framework Core performance using the `AsSplitQuery()` feature, introduced in EF Core 5.0. It explains the "Cartesian Explosion" problem caused by multiple `Include` statements generating a single, inefficient SQL query with excessive data duplication. By using `AsSplitQuery()`, EF Core generates multiple optimized queries, drastically reducing memory usage, network overhead, and execution time, with benchmarks showing up to 5.6x faster performance. The article covers implementation, global configuration, selective control, best practices, and troubleshooting, guiding developers on when and how to apply this powerful optimization.]
---
```

# One Trick To Improve EF Core Performance Using Query Splitting | by Michael Maurice | Sep, 2025 | Medium

# One Trick To Improve EF Core Performance Using Query Splitting

Entity Framework Core is already fast, but there’s one simple trick that can dramatically improve performance when working with related entities: Query Splitting. This feature, introduced in EF Core 5.0, can transform slow, memory-hungry queries into fast, efficient ones by changing how EF Core handles `Include` statements. Let's explore how this powerful feature works and when to use it.

![Diagram illustrating the concept of EF Core Query Splitting. On the left, a "Single Query" uses a JOIN operation, showing a complex SQL query and a C# `Include` statement. On the right, "Split Queries" show a database icon branching into multiple separate queries (Query 1, Query 2, Query 3, Query 4). In the center, the `.AsSplitQuery()` method is highlighted with benefits: "Up to 5.6x faster", "82% less memory", "99% fewer rows". It also lists "When to use" (Multiple collection Includes, Paginated + includes) and "Avoid for" (Small results, High-latency network, Simple one-to-one).](https://miro.medium.com/v2/resize:fit:700/1*ocCn1FGBqUNcdudta48J6A.png)

## Understanding the Problem: Cartesian Explosion

### The Silent Performance Killer

When you use multiple `Include` statements to load related data, EF Core generates a single SQL query with multiple JOINs. This creates a phenomenon called Cartesian Explosion:

```csharp
// This innocent-looking query can be a performance disaster  
var orders = await context.Orders  
    .Include(o => o.Customer)  
    .Include(o => o.OrderItems)  
    .Include(o => o.ShipmentHistory)  
    .ToListAsync();
```

What Happens Behind the Scenes:

*   If an order has 10 items and 5 history entries
*   Instead of returning `10 + 5 = 15` related rows
*   The database returns `10 × 5 = 50` rows with massive duplication
*   In real scenarios: 150 × 880 = 132,000 duplicate rows for one object

### The Generated SQL Problem

Without query splitting, EF Core generates something like this:

```sql
SELECT o.*, c.*, oi.*, sh.*  
FROM Orders o  
LEFT JOIN Customers c ON o.CustomerId = c.Id  
LEFT JOIN OrderItems oi ON o.Id = oi.OrderId  
LEFT JOIN ShipmentHistory sh ON o.Id = sh.OrderId  
WHERE o.Id = @orderId
```

Problems with This Approach:

*   **Data Duplication**: Order and customer data repeated for every combination
*   **Memory Explosion**: Massive result sets consuming excessive memory
*   **Network Overhead**: Transferring duplicated data across the network
*   **Processing Time**: EF Core must process and deduplicate all this data

## The Solution: AsSplitQuery()

### Simple Implementation

The fix is incredibly simple — just add `.AsSplitQuery()` to your query:

```csharp
// The magic fix - one method call transforms performance  
var orders = await context.Orders  
    .Include(o => o.Customer)  
    .Include(o => o.OrderItems)  
    .Include(o => o.ShipmentHistory)  
    .AsSplitQuery()  // 🎯 This is the game changer  
    .ToListAsync();
```

### What Query Splitting Does

Instead of one massive query, EF Core generates separate optimized queries:

```sql
-- Query 1: Load the main entity  
SELECT o.* FROM Orders o WHERE o.Id = @orderId;  
-- Query 2: Load related customers  
SELECT c.* FROM Customers c   
INNER JOIN Orders o ON c.Id = o.CustomerId   
WHERE o.Id = @orderId;  
-- Query 3: Load order items  
SELECT oi.* FROM OrderItems oi   
INNER JOIN Orders o ON oi.OrderId = o.Id   
WHERE o.Id = @orderId;  
-- Query 4: Load shipment history  
SELECT sh.* FROM ShipmentHistory sh   
INNER JOIN Orders o ON sh.OrderId = o.Id   
WHERE o.Id = @orderId;
```

Benefits of This Approach:

*   **No Data Duplication**: Each entity returned only once
*   **Reduced Memory Usage**: 99% reduction in rows returned
*   **Faster Execution**: Multiple small queries often faster than one giant query
*   **Network Efficiency**: Minimal data transfer

## Real-World Performance Comparison

### Benchmark Results

Based on comprehensive benchmarks, here are the dramatic differences:

| Scenario                        | Single Query | Split Query | Improvement      |
| :------------------------------ | :----------- | :---------- | :--------------- |
| Simple Relations                | 16.64 ms     | 18.97 ms    | Single query slightly faster |
| Complex Relations (Cartesian Explosion) | 200.60 ms    | 35.62 ms    | 5.6x faster      |
| Memory Usage                    | 46.93 MB     | 8.35 MB     | 82% less memory  |
| Rows Returned                   | 132,000+     | 1,000+      | 99% reduction    |

### Performance Analysis Code

Test the impact yourself with this benchmark setup:

```csharp
// Performance testing setup  
public class QueryPerformanceBenchmark  
{  
    private readonly ApplicationDbContext _context;  

    [Benchmark]  
    public async Task<List<Order>> GetOrdersWithSingleQuery()  
    {  
        return await _context.Orders  
            .Include(o => o.Customer)  
            .Include(o => o.OrderItems)  
            .Include(o => o.ShipmentHistory)  
            .AsSingleQuery() // Force single query  
            .Take(100)  
            .ToListAsync();  
    }  

    [Benchmark]  
    public async Task<List<Order>> GetOrdersWithSplitQuery()  
    {  
        return await _context.Orders  
            .Include(o => o.Customer)  
            .Include(o => o.OrderItems)  
            .Include(o => o.ShipmentHistory)  
            .AsSplitQuery() // Use split queries  
            .Take(100)  
            .ToListAsync();  
    }  

    [Benchmark]  
    public async Task<List<Order>> GetOrdersWithManualOptimization()  
    {  
        // Load main entities first  
        var orders = await _context.Orders  
            .Take(100)  
            .ToListAsync();  
        var orderIds = orders.Select(o => o.Id).ToList();  

        // Load related data separately  
        var customers = await _context.Customers  
            .Where(c => orderIds.Contains(c.Orders.First().Id))  
            .ToListAsync();  
        var orderItems = await _context.OrderItems  
            .Where(oi => orderIds.Contains(oi.OrderId))  
            .ToListAsync();  
        var shipmentHistory = await _context.ShipmentHistory  
            .Where(sh => orderIds.Contains(sh.OrderId))  
            .ToListAsync();  

        // EF Core's change tracker will automatically fix up relationships  
        return orders;  
    }  
}
```

## Advanced Usage Patterns

### Global Configuration

Configure split queries as the default behavior for your entire application:

```csharp
// Program.cs or Startup.cs  
services.AddDbContext<ApplicationDbContext>(options =>  
    options.UseSqlServer(connectionString, sqlOptions =>  
    {  
        // Enable split queries globally  
        sqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);  
    }));  

// Alternative configuration in DbContext  
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)  
{  
    optionsBuilder  
        .UseSqlServer(connectionString)  
        .UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);  
}  

// Or in OnModelCreating for specific configurations  
protected override void OnModelCreating(ModelBuilder modelBuilder)  
{  
    // Configure split query behavior  
    modelBuilder.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);  
      
    base.OnModelCreating(modelBuilder);  
}
```

### Selective Query Control

When split queries are configured globally, you can still opt into single queries for specific cases:

```csharp
// Override global split query setting for specific queries  
var orders = await context.Orders  
    .Include(o => o.Customer)  
    .Include(o => o.OrderItems)  
    .AsSingleQuery() // Force single query despite global setting  
    .Where(o => o.CustomerId == customerId)  
    .ToListAsync();  

// Example: Small result sets might benefit from single queries  
var recentOrder = await context.Orders  
    .Include(o => o.OrderItems)  
    .AsSingleQuery() // Single query for one record is fine  
    .OrderByDescending(o => o.CreatedDate)  
    .FirstOrDefaultAsync();
```

### Complex Relationship Handling

Handle deeply nested relationships efficiently:

```csharp
// Complex nested includes with split queries  
var companies = await context.Companies  
    .Include(c => c.Departments)  
        .ThenInclude(d => d.Employees)  
    .Include(c => c.Departments)  
        .ThenInclude(d => d.Projects)  
            .ThenInclude(p => p.Tasks)  
    .Include(c => c.Offices)  
        .ThenInclude(o => o.Equipment)  
    .AsSplitQuery() // Prevents massive Cartesian explosion  
    .ToListAsync();  

// Alternative: Selective loading for better control  
var companies = await context.Companies  
    .AsSplitQuery()  
    .ToListAsync();  

// Load departments separately if needed  
await context.Entry(companies.First())  
    .Collection(c => c.Departments)  
    .Query()  
    .Include(d => d.Employees)  
    .Include(d => d.Projects)  
    .LoadAsync();
```

## When to Use Split Queries

### Perfect Use Cases ✅

**Large Collections with Multiple Includes:**

```csharp
// Ideal scenario for split queries  
var blogs = await context.Blogs  
    .Include(b => b.Posts)  
    .Include(b => b.Categories)  
    .Include(b => b.Tags)  
    .AsSplitQuery()  
    .ToListAsync();
```

**Paginated Results with Related Data:**

```csharp
// Pagination with includes benefits from split queries  
var pagedOrders = await context.Orders  
    .Include(o => o.OrderItems)  
    .Include(o => o.ShipmentHistory)  
    .AsSplitQuery()  
    .Skip(page * pageSize)  
    .Take(pageSize)  
    .ToListAsync();
```

**Complex Hierarchical Data:**

```csharp
// Deep hierarchies cause Cartesian explosions  
var organizations = await context.Organizations  
    .Include(o => o.Departments)  
        .ThenInclude(d => d.Teams)  
            .ThenInclude(t => t.Members)  
    .Include(o => o.Projects)  
        .ThenInclude(p => p.Tasks)  
    .AsSplitQuery()  
    .ToListAsync();
```

### When to Avoid Split Queries ❌

**Small Result Sets:**

```csharp
// Single query might be faster for small data  
var userProfile = await context.Users  
    .Include(u => u.Profile)  
    .Include(u => u.Settings)  
    .AsSingleQuery() // Few records = single query OK  
    .FirstOrDefaultAsync(u => u.Id == userId);
```

**High Network Latency Environments:**

```csharp
// When database is remote and latency is high  
// Multiple round trips might be slower than one large query  
var remoteData = await context.RemoteEntities  
    .Include(e => e.RelatedData)  
    .AsSingleQuery() // Minimize network round trips  
    .ToListAsync();
```

**Simple One-to-One Relationships:**

```csharp
// One-to-one relationships don't cause Cartesian explosions  
var orders = await context.Orders  
    .Include(o => o.Customer)        // One-to-one: OK  
    .Include(o => o.ShippingAddress) // One-to-one: OK  
    .AsSingleQuery() // No Cartesian explosion risk  
    .ToListAsync();
```

## Monitoring and Optimization

### SQL Query Analysis

Use EF Core logging to analyze generated queries:

```csharp
// Enable sensitive data logging in development  
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)  
{  
    optionsBuilder  
        .UseSqlServer(connectionString)  
        .EnableSensitiveDataLogging() // Development only!  
        .LogTo(Console.WriteLine, LogLevel.Information);  
}  

// Analyze queries in code  
public async Task<List<Order>> GetOrdersWithQueryAnalysis()  
{  
    var query = context.Orders  
        .Include(o => o.OrderItems)  
        .Include(o => o.ShipmentHistory)  
        .AsSplitQuery();  

    // Log the SQL that will be executed  
    var sql = query.ToQueryString();  
    _logger.LogInformation("Generated SQL: {Sql}", sql);  

    var stopwatch = Stopwatch.StartNew();  
    var results = await query.ToListAsync();  
    stopwatch.Stop();  
    _logger.LogInformation("Query executed in {ElapsedMs}ms, returned {Count} entities",   
        stopwatch.ElapsedMilliseconds, results.Count);  
    return results;  
}
```

### Performance Monitoring Extension

Create reusable performance monitoring:

```csharp
public static class QueryExtensions  
{  
    public static async Task<List<T>> ToListWithPerformanceLoggingAsync<T>(        this IQueryable<T> query,  
        ILogger logger,  
        string queryName = null)  
    {  
        queryName ??= typeof(T).Name;  
          
        var sql = query.ToQueryString();  
        logger.LogDebug("Executing query {QueryName}: {Sql}", queryName, sql);  

        var stopwatch = Stopwatch.StartNew();  
        var results = await query.ToListAsync();  
        stopwatch.Stop();  
        logger.LogInformation("Query {QueryName} completed in {ElapsedMs}ms, returned {Count} results",  
            queryName, stopwatch.ElapsedMilliseconds, results.Count);  
        return results;  
    }  
}  

// Usage  
var orders = await context.Orders  
    .Include(o => o.OrderItems)  
    .AsSplitQuery()  
    .ToListWithPerformanceLoggingAsync(_logger, "GetOrdersWithItems");
```

## Best Practices and Guidelines

### Decision Matrix

Use this decision framework to choose between single and split queries:

```csharp
public static class QueryStrategySelector  
{  
    public static IQueryable<T> ApplyOptimalStrategy<T>(        this IQueryable<T> query,  
        int expectedResultCount,  
        int includeCount,  
        bool hasCollectionIncludes) where T : class  
    {  
        // Decision logic based on query characteristics  
        if (expectedResultCount <= 10 && includeCount <= 2)  
        {  
            // Small result set with few includes: single query is fine  
            return query.AsSingleQuery();  
        }  

        if (hasCollectionIncludes && includeCount > 2)  
        {  
            // Multiple collection includes: high risk of Cartesian explosion  
            return query.AsSplitQuery();  
        }  

        if (expectedResultCount > 100 && hasCollectionIncludes)  
        {  
            // Large result set with collections: definitely use split queries  
            return query.AsSplitQuery();  
        }  

        // Default to split query when in doubt  
        return query.AsSplitQuery();  
    }  
}  

// Usage with strategy selection  
var orders = await context.Orders  
    .Include(o => o.OrderItems)  
    .Include(o => o.ShipmentHistory)  
    .ApplyOptimalStrategy(expectedResultCount: 50, includeCount: 2, hasCollectionIncludes: true)  
    .ToListAsync();
```

### Repository Pattern Integration

Integrate split query decisions into repository patterns:

```csharp
public class OrderRepository  
{  
    private readonly ApplicationDbContext _context;  

    public async Task<List<Order>> GetOrdersWithDetailsAsync(  
        int customerId,   
        bool includeLargeCollections = false)  
    {  
        var query = _context.Orders  
            .Where(o => o.CustomerId == customerId)  
            .Include(o => o.Customer)  
            .Include(o => o.OrderItems);  

        if (includeLargeCollections)  
        {  
            query = query  
                .Include(o => o.ShipmentHistory)  
                .Include(o => o.PaymentHistory)  
                .AsSplitQuery(); // Use split queries for large collections  
        }  
        return await query.ToListAsync();  
    }  

    public async Task<Order> GetOrderByIdAsync(int orderId)  
    {  
        // Single record: single query is optimal  
        return await _context.Orders  
            .Include(o => o.Customer)  
            .Include(o => o.OrderItems)  
            .AsSingleQuery()  
            .FirstOrDefaultAsync(o => o.Id == orderId);  
    }  
}
```

## Troubleshooting Common Issues

### Warning Detection

EF Core generates warnings when it detects potential Cartesian explosion:

```csharp
protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)  
{  
    optionsBuilder.ConfigureWarnings(warnings =>  
    {  
        // Make Cartesian explosion warnings into errors  
        warnings.Throw(CoreEventId.CartesianExpansionWarning);  
          
        // Or log them for analysis  
        warnings.Log(CoreEventId.CartesianExpansionWarning);  
    });  
}
```

### Memory Usage Monitoring

Monitor memory usage to detect performance issues:

```csharp
public class QueryMemoryProfiler  
{  
    public async Task<T> ExecuteWithMemoryProfiling<T>(        Func<Task<T>> queryExecution,  
        string queryName)  
    {  
        var initialMemory = GC.GetTotalMemory(false);  
          
        var result = await queryExecution();  
          
        GC.Collect();  
        GC.WaitForPendingFinalizers();  
        GC.Collect();  
          
        var finalMemory = GC.GetTotalMemory(false);  
        var memoryUsed = finalMemory - initialMemory;  
          
        Console.WriteLine($"Query {queryName} used approximately {memoryUsed:N0} bytes");  
          
        return result;  
    }  
}  

// Usage  
var profiler = new QueryMemoryProfiler();  
var ordersWithSingleQuery = await profiler.ExecuteWithMemoryProfiling(  
    () => context.Orders.Include(o => o.OrderItems).AsSingleQuery().ToListAsync(),  
    "SingleQuery");  
var ordersWithSplitQuery = await profiler.ExecuteWithMemoryProfiling(  
    () => context.Orders.Include(o => o.OrderItems).AsSplitQuery().ToListAsync(),  
    "SplitQuery");
```

## Conclusion

Query Splitting with `AsSplitQuery()` is a powerful performance optimization technique that can transform slow, memory-intensive EF Core queries into fast, efficient ones:

### Key Benefits Achieved:

🚀 **Dramatic Performance Improvements**: Up to 5.6x faster execution for complex queries
💾 **Massive Memory Reduction**: Up to 82% less memory usage
📊 **Elimination of Data Duplication**: 99% reduction in unnecessary data transfer
🎯 **Simple Implementation**: One method call transforms your query performance
⚙️ **Flexible Configuration**: Apply globally or per-query as needed

### When to Use Split Queries:

*   Multiple collection includes in the same query
*   Large result sets with related data
*   Complex hierarchical relationships
*   Paginated queries with includes
*   Performance problems with existing queries

### Best Practices:

1.  **Monitor Query Performance**: Use logging and profiling to identify problems
2.  **Choose Strategy Based on Data Size**: Small queries might benefit from single queries
3.  **Configure Globally When Appropriate**: Set as default for consistent behavior
4.  **Test Both Approaches**: Benchmark your specific scenarios
5.  **Consider Network Latency**: Multiple queries vs. one large query trade-offs

The `AsSplitQuery()` method is truly a game-changing trick that every EF Core developer should know. It's simple to implement, provides dramatic performance improvements, and can save your application from the silent performance killer known as Cartesian explosion. Try it on your most complex queries and watch your performance soar! 🚀

**If you want the full source code, download it from this link:** [https://www.elitesolutions.shop/](https://www.elitesolutions.shop/)