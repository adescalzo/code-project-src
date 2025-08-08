```yaml
---
title: Optimizing SQL Performance with Indexing Strategies for Faster Queries
source: https://antondevtips.com/blog/optimizing-sql-performance-with-indexing-strategies-for-faster-queries
date_published: 2024-07-12T11:00:24.844Z
date_captured: 2025-08-06T17:24:51.549Z
domain: antondevtips.com
author: Anton Martyniuk
category: performance
technologies: [PostgreSQL, SQL databases]
programming_languages: [SQL]
tags: [sql, database, indexing, performance-tuning, query-optimization, data-access, postgresql, database-management]
key_concepts: [database-indexing, single-column-index, composite-index, unique-index, implicit-index, filtered-index, query-performance, soft-deletion]
code_examples: true
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to optimizing SQL query performance through indexing strategies. It explains what indexes are and how they speed up read operations by allowing database engines to locate rows faster. The post details various types of indexes, including single-column, unique, composite, implicit, and filtered indexes, illustrating each with practical SQL code examples tested on PostgreSQL. It also discusses the crucial pros and cons of using indexes, highlighting improved query speed versus increased storage and slower write operations. Finally, the article offers best practices for indexing, such as analyzing query patterns, using selective indexes, and regular maintenance, to achieve optimal database performance.
---
```

# Optimizing SQL Performance with Indexing Strategies for Faster Queries

![Cover image for an article titled "Optimizing SQL Performance with Indexing Strategies for Faster Queries". The image features a dark background with abstract purple shapes, a white icon with code tags (< />) and "dev tips" text, and the article title in large white letters.](https://antondevtips.com/media/covers/databases/cover_db_performance_indexes.png)

# Optimizing SQL Performance with Indexing Strategies for Faster Queries

Jul 12, 2024

5 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

A slow SQL queries can degrade the application's performance and overall user experience. Imagine you're on the shopping website and want to buy a refrigerator of a specific manufacturer. After selecting a manufacturer, you need to wait for 5 or even more seconds for a web page to show the filtered refrigerators. Probably you will leave this website and find another that will give you results faster.

In this blog post, you will learn how to increase read queries in SQL databases by using indexes. I will show you different types of indexes with practical examples. In the end, you'll receive guidelines on how and when to use indexes, and what potential problems they can lead to when not properly used.

## What Are Indexes in SQL Databases?

One of the most effective ways to optimize SQL performance is through the use of **indexing**.

**Indexes** are data structures that improve the speed of read operations on a database table. You can compare an index in the database to an index in a book. Indexes allow the database engine to find rows faster by reading table of contents rather than scanning the entire table.

Indexes are created using one or more columns of a table. Under the hood, they store a sorted copy of those columns, which allows the database engine to find rows more quickly.

To create an index in the database, you can use the `CREATE INDEX` SQL command. You can also delete an index.

```sql
CREATE INDEX index_name ON table_name(column_name);

DROP INDEX index_name;
```

> All SQL queries from this blog post were tested in the **Postgres database**. While the similar SQL syntax can be used in other databases.

## Types Of Indexes in SQL Databases

The most common **types of indexes**, available in all popular databases, like **postgres**, are the following:

*   Single-Column Indexes
*   Composite Indexes
*   Unique Indexes
*   Implicit Indexes
*   Filtered Indexes

In this post I'll showcase all types of indexes for `employees` and `departments` tables. Where the `employee` has a foreign key to `departments` table:

```sql
CREATE TABLE departments (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    name VARCHAR(100) NOT NULL,
    location VARCHAR(100)
);

CREATE TABLE employees (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    name VARCHAR(100) NOT NULL,
    department_id INT,
    job_title VARCHAR(100),
    status VARCHAR(20),
    FOREIGN KEY (department_id) REFERENCES departments(id)
);
```

## Single-Column Indexes

A **single-column index** is created on one column of a table. It is used to speed up queries that filter or sort on that column.

Let's create our first index on the `departments` table on the `name` column:

```sql
CREATE INDEX idx_department_name ON departments(name);
```

This index will speed up queries on the `departments` table if you filter or sort on the `name` column. For example:

```sql
SELECT * FROM departments WHERE name = 'Sales';

SELECT * FROM departments
WHERE name = 'Sales'
ORDER BY name;
```

With the index, the database can quickly locate the rows where the department name is 'Sales'.

A single index scan can only be used in `WHERE` clauses that are combined with the `AND` logical operator:

```sql
SELECT * FROM employees WHERE name = 'Anton' AND department_id = 1;
```

When using an `OR` condition, the index cannot be used:

```sql
SELECT * FROM employees WHERE name = 'Anton' or job_title = 'Solution Architect';
```

## Unique Indexes

A **unique index** ensures that all values in the indexed column are **unique**. This is useful for columns that should not have duplicate values.

In the `departments` table a `name` column is good candidate for a `unique` index, which ensures that there are no two departments with the same name:

```sql
CREATE UNIQUE INDEX idx_department_name_unique ON departments(name);

INSERT INTO departments (name, location) VALUES ('Sales', 'New York');
```

If there is already a department named 'Sales', this insert statement will fail, maintaining data integrity.

## Composite Indexes

A **composite index** is created on two or more columns of a table. It is used to speed up queries that filter or sort on multiple columns.

To create such an index, you need to specify a list of comma separated columns in the `( )` brackets:

```sql
CREATE INDEX idx_employee_name_department ON employees(name, department_id);
```

This composite index on the `name` and `department_id` columns of the `employees` table will speed up queries that filter or sort by both employee name and department ID:

```sql
SELECT * FROM employees
WHERE name = 'Anton' AND department_id = 1;
```

## Implicit Indexes

An **implicit index** is automatically created by the database engine for a **primary key**.

When creating the `departments` and `employees` tables, implicit indexes are automatically created for the primary keys:

```sql
CREATE TABLE departments (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
);

CREATE TABLE employees (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY
);
```

The primary key constraint on the `id` columns of both tables implicitly creates **unique indexes** on these columns. These indexes ensure that each row has a unique identifier, which speeds up lookups by primary key:

```sql
SELECT * FROM employees WHERE id = 1;
```

## Filtered Indexes

A **filtered index** is an index that includes only a subset of rows in a table, based on a filter condition. This type of index is useful when you often query a specific subset of data, as it reduces the index size and improves query performance for those specific queries.

Consider a scenario where you frequently query employees who are active based on the `status` column.

To create a filtered index, add the `WHERE` clause in the `CREATE INDEX` statement:

```sql
CREATE INDEX idx_active_employees ON employees(id, name) WHERE status = 'Active';
```

This **filtered index** includes only the rows where status is 'Active'.

Queries that filter on 'Active' status will benefit from this index, for example:

```sql
SELECT id, name FROM employees WHERE status = 'Active';
```

Another good use-case for a filtered index is a **soft deletion**.

**Soft deletion** is a technique where rows in the database are marked as deleted by setting a specific column value, instead of deleting rows forever. This is particularly useful when data needs to remain in the database, so the history and foreign keys are preserved.

This can be implemented in the same manner, where you create a filtered index on those employees that are not deleted:

```sql
CREATE INDEX idx_non_deleted_employees ON employees(id, name) WHERE is_deleted = false;

SELECT id, name FROM employees WHERE is_deleted = false;
```

Deleted employees are not indexed as they are rarely accessed, thus saving the storage occupied by an index.

## Pros and Cons of Using Indexes

**Pros:**

*   **Improved query performance:** indexes can greatly speed up data retrieval, reducing the time it takes to execute SELECT queries, especially those involving WHERE clauses, JOINs, and ORDER BY clauses.
*   **Efficient sorting:** indexes help in sorting operations, making ORDER BY and GROUP BY queries more efficient.
*   **Enforcing uniqueness:** unique indexes ensure that the indexed columns contain only unique values, maintaining data integrity.

**Cons:**

*   **Increased storage:** indexes require additional storage space. The more indexes you have, or the more data you have - the more disk space they will consume.
*   **Slower writes:** insert, update, and delete operations can be slower because the database must also update the indexes. This can be particularly impactful in write-heavy applications.
*   **Maintenance overhead:** indexes require maintenance. As data changes, indexes can become fragmented and may need to be rebuilt or reorganized periodically to maintain performance.

**NOTE:** the `UPDATE` operation can benefit from the index when a `WHERE` clause includes an indexed column, for example:

```sql
CREATE INDEX idx_employee_name ON employees(name);

UPDATE employees SET job_title = 'Senior Software Engineer' WHERE name = 'Nick';
```

## Best Practices for Indexing

1.  **Analyze query patterns:** understand the most frequent and performance-critical queries to determine which columns to index.
2.  **Use selective indexes:** create indexes on columns with high selectivity, meaning columns that have a wide range of unique values.
3.  **Limit the number of indexes:** while indexes can improve read performance, they can also slow down write operations. Balance the number of indexes to optimize both read and write performance.
4.  **Index columns used in WHERE clauses:** index columns that are frequently used in WHERE clauses, JOIN conditions, and ORDER BY clauses.
5.  **Monitor and maintain indexes:** regularly check the performance of indexes and rebuild or reorganize them as needed to maintain optimal performance.

## Summary

Indexing is a powerful tool for optimizing SQL query performance. Properly managing and balancing the use of indexes can lead to significant performance improvements for read-heavy applications.

Remember that indexes come with trade-offs. In write-heavy applications, the overhead on write operations and storage costs must be carefully considered.

Remember to analyze your query patterns, use appropriate indexes, and maintain your indexes regularly to achieve the best performance.

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Foptimizing-sql-performance-with-indexing-strategies-for-faster-queries&title=Optimizing%20SQL%20Performance%20with%20Indexing%20Strategies%20for%20Faster%20Queries)[X](https://twitter.com/intent/tweet?text=Optimizing%20SQL%20Performance%20with%20Indexing%20Strategies%20for%20Faster%20Queries&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Foptimizing-sql-performance-with-indexing-strategies-for-faster-queries)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Foptimizing-sql-performance-with-indexing-strategies-for-faster-queries)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.