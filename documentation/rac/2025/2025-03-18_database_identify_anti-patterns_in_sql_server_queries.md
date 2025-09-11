```yaml
---
title: Identify Anti-Patterns in SQL Server Queries
source: https://www.mssqltips.com/sqlservertip/8207/sql-server-antipattern-extended-event/
date_published: 2025-03-18T04:00:00.000Z
date_captured: 2025-09-10T19:29:55.155Z
domain: www.mssqltips.com
author: Rajendra Gupta
category: database
technologies: [SQL Server 2022, SQL Server Management Studio (SSMS), SQL Server Extended Events]
programming_languages: [SQL]
tags: [sql-server, performance-tuning, extended-events, query-optimization, anti-patterns, database, diagnostics, execution-plan]
key_concepts: [sql-query-optimizer, query-anti-patterns, implicit-data-type-conversion, non-sargable-queries, parameter-sniffing, extended-events-configuration, performance-monitoring, execution-plans]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the new Query_AntiPattern Extended Event in SQL Server 2022, designed to identify inefficient SQL query patterns that hinder the query optimizer. It explains common anti-patterns such as non-sargable queries, parameter sniffing, and implicit data type conversions. The guide demonstrates how to configure and use this extended event via SQL Server Management Studio or T-SQL scripts to capture problematic queries. By identifying these anti-patterns, database professionals can modify their queries to improve performance and resource utilization. The article includes practical examples and steps to exclude IntelliSense queries for cleaner results.]
---
```

# Identify Anti-Patterns in SQL Server Queries

# Identify Anti-Patterns in SQL Server Queries

Problem

How can we capture queries that are causing issues and stopping the query optimizer from working efficiently? SQL Server 2022 introduces a new extended event for this purpose. In this article, we look at a new SQL Server Extended Event to help identify anti-patterns in SQL queries​ and improve performance.

Solution

Extended events are great for capturing and troubleshooting SQL Server performance issues. This tip discusses a valuable new extended event introduced in SQL Server 2022, the Query\_AntiPattern Extended Event.

In general, the SQL Server query optimizer devises several execution plans and chooses the lowest-cost optimized plan to execute a query. Sometimes, however, it cannot optimize the plan, which might lead to insufficient resource utilization despite proper indexing and statistics.

Examples of the issues that may lead to inefficient query plans are:

*   Non-Sargable Queries
*   Parameter Sniffing Issues
*   Implicit Data Type Conversions
*   Missing or Outdated Statistics
*   Over-Indexing or Under-Indexing
*   Complex Joins and Subqueries
*   Low-Selectivity Filters

SQL Server 2022 introduced a new extended event to capture queries that are having these issues and causing the query optimizer to not work efficiently.

## Query\_AntiPattern Extended Event

The extended event query\_antipattern captures insufficient query patterns in your workload that lead to performance issues. Once identified, you can modify your query to avoid antipatterns and help SQL Server optimize the code.

Querying **sys.dm\_xe\_map\_values** gives the values used in the query antipattern extended event.

```sql
SELECT name,map_value  
FROM sys.dm_xe_map_values
WHERE name = N'query_antipattern_type';
```

![Screenshot showing the results of querying sys.dm_xe_map_values for 'query_antipattern_type', listing anti-pattern types like LargeIn and TypeConvertPreventingSeek.](/wp-content/images-tips-8/8207_identify-anti-patterns-sql-server-queries-1.webp)

### LargeIn

A large number of values in the IN clause can be resource-intensive for large datasets. The query optimizer might need to use an index scan instead of a seek. Here is an example.

```sql
SELECT * FROM tableA WHERE [column_name] IN (1, 2, 3, ..., N);
```

### LargeNumberOfOrInPredicate

It checks for multiple OR clauses or large IN predicates in the where clause, like below. The query optimizer must evaluate each OR condition, leading to intensive scans and performance issues for large tables or rows.

```sql
SELECT * FROM tableA WHERE [column_name] = 1 OR[column_name] = 2 OR ... OR[column_name] = N;
```

### MAX

Queries using aggregate functions, such as MAX, may perform slowly without appropriate indexes. If you are trying to fetch the maximum value from a non-indexed column, the query optimizer may perform a scan, which can be a bottleneck for tables with a large number of rows.

### NonOptimalOrLogic

As the name suggests, this term refers to using poorly optimized OR clauses like **LargeNumberOfOrInPredicate.**

### TypeConvertPreventingSeek

You might have seen implicit conversion warnings in the SQL Server execution plan.

```sql
CREATE TABLE ImplicitConversionTest (    ID INT PRIMARY KEY,
    ColumnA VARCHAR(50) 
);
INSERT INTO ImplicitConversionTest (ID, ColumnA)
VALUES (1, '100'), (2, '200'), (3, '300');
Go
 
SELECT *
FROM ImplicitConversionTest
WHERE ColumnA = 100;
```

Below is the actual execution plan of the selected statement above and we can see convert implicit warnings.

In the above query, column A’s data type is NVARCHAR, but we are comparing it with the integer value in the select statement. Due to this data type difference, SQL Server needs to do an implicit conversion. The execution plan displays a warning that the implicit conversion might affect the cardinality estimation in the query plan choice and ultimately cause performance impact.

![SQL Server Management Studio execution plan showing a 'Clustered Index Scan' and a warning for 'Type conversion in expression (CONVERT_IMPLICIT)' due to implicit data type conversion.](/wp-content/images-tips-8/8207_identify-anti-patterns-sql-server-queries-2.webp)

## Configuring the Query\_AntiPattern Extended Event

The Query\_AntiPattern extended event is available in the following:

*   Category: Warnings
*   Channel: Operational
*   Package: sqlserver

![Screenshot of the SQL Server Extended Events wizard, specifically the 'Select Events To Capture' step, with 'query_antipattern' highlighted in the event library.](/wp-content/images-tips-8/8207_identify-anti-patterns-sql-server-queries-3.webp)

You can use the extended event wizard in SQL Server Management Studio (SSMS) or the following query to set it up with required fields, such as client\_app\_name, plan\_handle, query\_hash, query\_plan\_hash, and sql\_text.

```sql
USE master;
GO 
 
IF EXISTS (SELECT * 
           FROM   sys.dm_xe_sessions 
           WHERE  NAME = 'antipattern_identify') 
  BEGIN 
      DROP event session [antipattern_identify] ON server; 
  END
GO 
 
CREATE EVENT SESSION [antipattern_identify] ON SERVER  
     ADD EVENT sqlserver.query_antipattern (   
        ACTION(sqlserver.client_app_name,sqlserver.plan_handle, 
               sqlserver.query_hash,sqlserver.query_plan_hash,
               sqlserver.sql_text)
          ) ADD TARGET package0.ring_buffer(SET max_memory=(500)) 
GO
```

After setting up the extended event, start it with the ALTER EVENT SESSION command to find and identify anti-patterns in SQL queries.

```sql
ALTER EVENT SESSION antipattern_identify ON SERVER
STATE = START;
GO
```

This extended event session is viewable in the SSMS, as shown below.

![Screenshot of SQL Server Management Studio showing the 'antipattern_identify' extended event session listed under Management -> Extended Events -> Sessions.](/wp-content/images-tips-8/8207_identify-anti-patterns-sql-server-queries-4.webp)

Right-click on the antipattern\_identify extended event and click on **_Watch Live Data_**.

![Screenshot of SQL Server Management Studio context menu for an Extended Event session, highlighting the 'Watch Live Data' option.](/wp-content/images-tips-8/8207_identify-anti-patterns-sql-server-queries-5.webp)

## Capture Bad Queries with the Extended Event

Let’s run a few sample queries and see how it gets the data.

```sql
USE WideWorldImporters
GO
 
SELECT *  
FROM   Sales.Orders 
WHERE  CustomerPurchaseOrderNumber = 16374;
```

As shown below, the extended event captured the TypeConvertPreventingSeek extended event. The query is looking to search a specific record. However, due to this implicit conversion, the query optimizer used an index scan.

![Screenshot of the Live Data window for the Query_AntiPattern Extended Event, showing an event for 'TypeConvertPreventingSeek' triggered by a sample query.](/wp-content/images-tips-8/8207_identify-anti-patterns-sql-server-queries-6.webp)

We can see the data the data below that was captured with the Extended Event.

![Detailed view of an Extended Event captured data, showing fields like client_app_name, sql_text, and the specific anti-pattern type 'TypeConvertPreventingSeek'.](/wp-content/images-tips-8/8207_identify-anti-patterns-sql-server-queries-7.webp)

Let’s run another query that contains around 500 values in the IN expression of the WHERE clause.

```sql
SELECT *  
FROM Sales.Orders
WHERE CustomerPurchaseOrderNumber IN (
    1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 
    11, 12, 13, 14, 15, 16, 17, 18, 19, 20,
    21, 22, 23, 24, 25, 26, 27, 28, 29, 30,
    31, 32, 33, 34, 35, 36, 37, 38, 39, 40,
    41, 42, 43, 44, 45, 46, 47, 48, 49, 50,
    51, 52, 53, 54, 55, 56, 57, 58, 59, 60,
    61, 62, 63, 64, 65, 66, 67, 68, 69, 70,
    71, 72, 73, 74, 75, 76, 77, 78, 79, 80,
    81, 82, 83, 84, 85, 86, 87, 88, 89, 90,
    91, 92, 93, 94, 95, 96, 97, 98, 99, 100,
    101, 102, 103, 104, 105, 106, 107, 108, 109, 110,
    111, 112, 113, 114, 115, 116, 117, 118, 119, 120,
    121, 122, 123, 124, 125, 126, 127, 128, 129, 130,
    131, 132, 133, 134, 135, 136, 137, 138, 139, 140,
    141, 142, 143, 144, 145, 146, 147, 148, 149, 150,
    151, 152, 153, 154, 155, 156, 157, 158, 159, 160,
    161, 162, 163, 164, 165, 166, 167, 168, 169, 170,
    171, 172, 173, 174, 175, 176, 177, 178, 179, 180,
    181, 182, 183, 184, 185, 186, 187, 188, 189, 190,
    191, 192, 193, 194, 195, 196, 197, 198, 199, 200,
    201, 202, 203, 204, 205, 206, 207, 208, 209, 210,
    211, 212, 213, 214, 215, 216, 217, 218, 219, 220,
    221, 222, 223, 224, 225, 226, 227, 228, 229, 230,
    231, 232, 233, 234, 235, 236, 237, 238, 239, 240,
    241, 242, 243, 244, 245, 246, 247, 248, 249, 250,
    251, 252, 253, 254, 255, 256, 257, 258, 259, 260,
    261, 262, 263, 264, 265, 266, 267, 268, 269, 270,
    271, 272, 273, 274, 275, 276, 277, 278, 279, 280,
    281, 282, 283, 284, 285, 286, 287, 288, 289, 290,
    291, 292, 293, 294, 295, 296, 297, 298, 299, 300,
    301, 302, 303, 304, 305, 306, 307, 308, 309, 310,
    311, 312, 313, 314, 315, 316, 317, 318, 319, 320,
    321, 322, 323, 324, 325, 326, 327, 328, 329, 330,
    331, 332, 333, 334, 335, 336, 337, 338, 339, 340,
    341, 342, 343, 344, 345, 346, 347, 348, 349, 350,
    351, 352, 353, 354, 355, 356, 357, 358, 359, 360,
    361, 362, 363, 364, 365, 366, 367, 368, 369, 370,
    371, 372, 373, 374, 375, 376, 377, 378, 379, 380,
    381, 382, 383, 384, 385, 386, 387, 388, 389, 390,
    391, 392, 393, 394, 395, 396, 397, 398, 399, 400,
    401, 402, 403, 404, 405, 406, 407, 408, 409, 410,
    411, 412, 413, 414, 415, 416, 417, 418, 419, 420,
    421, 422, 423, 424, 425, 426, 427, 428, 429, 430,
    431, 432, 433, 434, 435, 436, 437, 438, 439, 440,
    441, 442, 443, 444, 445, 446, 447, 448, 449, 450,
    451, 452, 453, 454, 455, 456, 457, 458, 459, 460,
    461, 462, 463, 464, 465, 466, 467, 468, 469, 470,
    471, 472, 473, 474, 475, 476, 477, 478, 479, 480,
    481, 482, 483, 484, 485, 486, 487, 488, 489, 490,
    491, 492, 493, 494, 495, 496, 497, 498, 499, 500
);
```

Execute the query and switch to the watch live data window. As shown below, it generates an error for **_query\_antipattern – LargeNumberOfOrInPredicate_**.

![Screenshot of the Live Data window for the Query_AntiPattern Extended Event, showing an event for 'LargeNumberOfOrInPredicate' triggered by a query with many values in an IN clause.](/wp-content/images-tips-8/8207_identify-anti-patterns-sql-server-queries-8.webp)

Similarly, if we rewrite the above query using the OR operator, it raises the **LargeNumberOfOrInPredicate** warning as well in the query\_antipatten extended event.

```sql
SELECT *  
FROM Sales.Orders
WHERE CustomerPurchaseOrderNumber = 1 
OR CustomerPurchaseOrderNumber = 2 OR CustomerPurchaseOrderNumber = 3 OR CustomerPurchaseOrderNumber  = 4 OR CustomerPurchaseOrderNumber  = 5 
OR CustomerPurchaseOrderNumber = 6 OR CustomerPurchaseOrderNumber  = 7 OR CustomerPurchaseOrderNumber  = 8 OR CustomerPurchaseOrderNumber  = 9 
OR CustomerPurchaseOrderNumber  = 10 OR CustomerPurchaseOrderNumber  = 11 OR CustomerPurchaseOrderNumber  = 12 OR CustomerPurchaseOrderNumber  = 13 
OR CustomerPurchaseOrderNumber  = 14 OR CustomerPurchaseOrderNumber  = 15 OR CustomerPurchaseOrderNumber  = 16 OR CustomerPurchaseOrderNumber  = 17 
OR CustomerPurchaseOrderNumber  = 18 OR CustomerPurchaseOrderNumber  = 19 OR CustomerPurchaseOrderNumber  = 20 OR CustomerPurchaseOrderNumber  = 21 
OR CustomerPurchaseOrderNumber  = 22 OR CustomerPurchaseOrderNumber  = 23 OR CustomerPurchaseOrderNumber  = 24 OR CustomerPurchaseOrderNumber  = 25 
OR CustomerPurchaseOrderNumber  = 26 OR CustomerPurchaseOrderNumber  = 27 OR CustomerPurchaseOrderNumber  = 28 OR CustomerPurchaseOrderNumber  = 29 
OR CustomerPurchaseOrderNumber  = 30 OR CustomerPurchaseOrderNumber  = 31 OR CustomerPurchaseOrderNumber  = 32 OR CustomerPurchaseOrderNumber  = 33 
OR CustomerPurchaseOrderNumber  = 34 OR CustomerPurchaseOrderNumber  = 35 OR CustomerPurchaseOrderNumber  = 36 OR CustomerPurchaseOrderNumber  = 37 
OR CustomerPurchaseOrderNumber  = 38 OR CustomerPurchaseOrderNumber  = 39 OR CustomerPurchaseOrderNumber  = 40 OR CustomerPurchaseOrderNumber  = 41 
OR CustomerPurchaseOrderNumber  = 42 OR CustomerPurchaseOrderNumber  = 43 OR CustomerPurchaseOrderNumber  = 44 OR CustomerPurchaseOrderNumber  = 45 
OR CustomerPurchaseOrderNumber  = 46 OR CustomerPurchaseOrderNumber  = 47 OR CustomerPurchaseOrderNumber  = 48 OR CustomerPurchaseOrderNumber  = 49 
OR CustomerPurchaseOrderNumber  = 50 OR CustomerPurchaseOrderNumber  = 51 OR CustomerPurchaseOrderNumber  = 52 OR CustomerPurchaseOrderNumber  = 53 
OR CustomerPurchaseOrderNumber  = 54 OR CustomerPurchaseOrderNumber  = 55 OR CustomerPurchaseOrderNumber  = 56 OR CustomerPurchaseOrderNumber  = 57 
OR CustomerPurchaseOrderNumber  = 58 OR CustomerPurchaseOrderNumber  = 59 OR CustomerPurchaseOrderNumber  = 60 OR CustomerPurchaseOrderNumber  = 61 
OR CustomerPurchaseOrderNumber  = 62 OR CustomerPurchaseOrderNumber  = 63 OR CustomerPurchaseOrderNumber  = 64 OR CustomerPurchaseOrderNumber  = 65 
OR CustomerPurchaseOrderNumber  = 66 OR CustomerPurchaseOrderNumber  = 67 OR CustomerPurchaseOrderNumber  = 68 OR CustomerPurchaseOrderNumber  = 69 
OR CustomerPurchaseOrderNumber  = 70 OR CustomerPurchaseOrderNumber  = 71 OR CustomerPurchaseOrderNumber  = 72 OR CustomerPurchaseOrderNumber  = 73 
OR CustomerPurchaseOrderNumber  = 74 OR CustomerPurchaseOrderNumber  = 75 OR CustomerPurchaseOrderNumber  = 76 OR CustomerPurchaseOrderNumber  = 77 
OR CustomerPurchaseOrderNumber  = 78 OR CustomerPurchaseOrderNumber  = 79 OR CustomerPurchase