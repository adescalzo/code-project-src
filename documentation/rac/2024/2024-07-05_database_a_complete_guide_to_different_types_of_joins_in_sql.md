```yaml
---
title: A Complete Guide to Different Types of Joins in SQL
source: https://antondevtips.com/blog/a-complete-guide-to-different-types-of-joins-in-sql
date_published: 2024-07-05T11:00:50.702Z
date_captured: 2025-08-06T17:24:53.657Z
domain: antondevtips.com
author: Anton Martyniuk
category: database
technologies: [PostgreSQL]
programming_languages: [SQL]
tags: [sql, database, joins, data-access, relational-database, querying, postgresql]
key_concepts: [inner-join, left-join, right-join, full-join, cross-join, self-join, union-operator, data-normalization]
code_examples: false
difficulty_level: intermediate
summary: |
  [This guide provides a comprehensive overview of different SQL join types, essential for combining data from multiple tables. It explains the purpose and syntax of Inner, Left, Right, Full, Cross, and Self Joins, illustrating each with clear examples using `employees` and `departments` tables. The article also covers the `UNION` operator, distinguishing it from joins and highlighting its use for combining query results. Practical SQL code snippets and visual diagrams aid understanding, making it a valuable resource for mastering relational database querying. The content emphasizes data integrity and normalization through the use of keys and joins.]
---
```

# A Complete Guide to Different Types of Joins in SQL

![A Complete Guide to Different Types of Joins in SQL - Newsletter Cover](https://antondevtips.com/media/covers/databases/cover_db_joins.png)

# A Complete Guide to Different Types of Joins in SQL

Jul 5, 2024

6 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

In SQL databases, relations between tables are established through keys, which help maintain data integrity and ensure that the data is logically connected.

**Joins** are fundamental to SQL queries, allowing you to combine data from two or more tables based on related columns. This guide explores different types of joins, providing explanations and examples to help you master their usage.

## Why Do We Need Joins?

Joins are essential because they allow you to:

*   **Combine Data:** fetch data from multiple tables based on related columns.
*   **Avoid Data Duplication:** maintain data normalization by using multiple tables, reducing redundancy.

Joins are performed on columns that have a logical relationship. These columns typically have the same data type and meaning.

In SQL you can perform the following **joins**:

*   Inner Join
*   Left Join
*   Right Join
*   Full Join
*   Cross Join
*   Self Join
*   Union

Most of the joins in SQL have the common syntax:

```sql
SELECT table1.column1, table2.column2
FROM table1
INNER JOIN table2
ON table1.common_column = table2.common_column;
```

In place of `INNER JOIN` you can use any other join.

> All SQL queries from this blog post were tested in the **Postgres database**. While the similar SQL syntax can be used in other databases.

## Inner Join

The **INNER JOIN** returns only the rows that match values in both tables.

For better understanding, have a look at the following image with Table A and Table B:

![Venn diagram illustrating an Inner Join, showing the intersection of Table A and Table B highlighted in blue.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_1.png)

Here, the rows that are returned by inner join have a colored background. As you can see, the rows that match values in both tables are in the middle of Table A and Table B.

We will use this circle's example for all other joins.

In this post I'll showcase all types of joins for `employees` and `departments` tables. Where the `employee` has a foreign key to `departments` table:

```sql
CREATE TABLE departments (
    id INT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    location VARCHAR(100)
);

CREATE TABLE employees (
    id INT PRIMARY KEY,
    name VARCHAR(100) NOT NULL,
    department_id INT FOREIGN KEY REFERENCES departments(id),
    job_title VARCHAR(100)
);
```

Let's insert some data into these tables:

![Screenshot of a database client showing 'employees' and 'departments' tables with sample data. The employees table includes columns for id, name, department_id, job_title, and manager_id. The departments table includes id, name, and location.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_8.png)

You can select `employees` and their `departments` by using **INNER JOIN** SQL statement:

```sql
SELECT employees.name, departments.name
FROM employees
INNER JOIN departments
ON employees.department_id = departments.id;
```

**INNER JOIN** returns only those `employees` that have matching `departments`.

![Screenshot of SQL query result showing employee names and their corresponding department names after an INNER JOIN, displaying only matching records.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_10.png)

When making joins, you can use **table aliases** for brevity:

```sql
SELECT employees.name, departments.name
FROM employees e
INNER JOIN departments d
ON e.department_id = d.id;
```

## Left Join

The **LEFT JOIN** (also called LEFT OUTER JOIN) returns all rows from the left table and the matched rows from the right table. If no match is found, NULL values are returned for columns from the right table:

![Venn diagram illustrating a Left Join, showing all of Table A and the intersection with Table B highlighted in blue.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_2.png)

```sql
SELECT employees.name, departments.name
FROM employees
LEFT JOIN departments
ON employees.department_id = departments.id;
```

**LEFT JOIN** returns all rows from the `employees` table. For those rows that have matching `departments`, the `department_id` will have a corresponding value from the `departments` table. If there is no matching department, the result will show `NULL` for the `department_name`.

![Screenshot of SQL query result showing employee names and their department names after a LEFT JOIN, including employees without a matching department (NULL for department name).](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_9.png)

If you need to remove the `employees` rows that don't have a matching `department`, you can add the `WHERE` clause to filter the NULL rows:

```sql
SELECT employees.name, departments.name
FROM employees
LEFT JOIN departments
ON employees.department_id = departments.id
WHERE departments.id IS NOT NULL;
```

![Screenshot of SQL query result showing employee names and their corresponding department names after an INNER JOIN, displaying only matching records.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_10.png)

You can see a visualization of this case on the picture:

![Venn diagram illustrating a Left Join with a WHERE clause to exclude NULLs, showing only the intersection of Table A and Table B highlighted in blue.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_3.png)

## Right Join

The **RIGHT JOIN** (also called RIGHT OUTER JOIN) returns all rows from the right table and the matched rows from the left table. If no match is found, NULL values are returned for columns from the left table:

![Venn diagram illustrating a Right Join, showing all of Table B and the intersection with Table A highlighted in blue.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_4.png)

```sql
SELECT employees.name, departments.name
FROM employees
RIGHT JOIN departments
ON employees.department_id = departments.id;
```

**RIGHT JOIN** returns all rows from the `departments` table. For those rows that have matching `employees`, the `department_id` in the `employees` table will have a corresponding value. If there is no matching employee, the result will show `NULL` for the corresponding column.

![Screenshot of SQL query result showing department names and their corresponding employee names after a RIGHT JOIN, including departments without a matching employee (NULL for employee name).](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_11.png)

If you need to remove the `department` rows that don't have a matching `employee`, you can add the `WHERE` clause to filter the NULL rows:

```sql
SELECT employees.name, departments.name
FROM employees
RIGHT JOIN departments
ON employees.department_id = departments.id
WHERE employees.id IS NOT NULL;
```

![Screenshot of SQL query result showing employee names and their corresponding department names after an INNER JOIN, displaying only matching records.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_10.png)

You can see a visualization of this case on the picture:

![Venn diagram illustrating a Right Join with a WHERE clause to exclude NULLs, showing only the intersection of Table A and Table B highlighted in blue.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_5.png)

## Full Join

The **FULL JOIN** (also called FULL OUTER JOIN) returns all rows when there is a match in either left or right table. Rows without a match in one of the tables will have NULL values for columns of that table:

![Venn diagram illustrating a Full Join, showing the union of Table A and Table B highlighted in blue.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_6.png)

```sql
SELECT employees.name, departments.name
FROM employees
FULL JOIN departments
ON employees.department_id = departments.id;
```

**FULL JOIN** returns all rows when there is a match in either the `employees` or the `departments` table. For those rows that have matching departments, the `department_id` will have a corresponding value from the `departments` table. If there is no matching department, the result will show `NULL` for the `department.name`. Similarly, if there is no matching `employee`, the result will show NULL for the `employees.name` column.

![Screenshot of SQL query result showing all employees and departments after a FULL JOIN, including NULLs for non-matching records from either table.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_12.png)

If you need to get a list of all `employees` and `departments` but exclude rows where there are NULLs on both sides, you can add the `WHERE` clause to filter the NULL rows:

```sql
SELECT employees.name, departments.name
FROM employees
FULL JOIN departments
ON employees.department_id = departments.id
WHERE employees.id IS NOT NULL
AND departments.id IS NOT NULL;
```

![Screenshot of SQL query result showing employee names and their corresponding department names after an INNER JOIN, displaying only matching records.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_10.png)

You can see a visualization of this case on the picture:

![Venn diagram illustrating a Full Join with a WHERE clause to exclude NULLs, showing only the intersection of Table A and Table B highlighted in blue.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_7.png)

## Cross Join

The **CROSS JOIN** returns the cartesian product of the two tables, combining each row of the first table with all rows of the second table.

**CROSS JOIN** has a different syntax, comparing to other joins:

```sql
SELECT table1.column1, table2.column2
FROM table1
CROSS JOIN table2;
```

Let's have a look at example:

```sql
SELECT employees.name, departments.name
FROM employees
CROSS JOIN departments;
```

**CROSS JOIN** returns the cartesian product of the `employees` and `departments` tables. This means that each row from the `employees` table is combined with each row from the `departments` table, resulting in all possible combinations of rows between the two tables.

![Screenshot of SQL query result showing the Cartesian product of employees and departments after a CROSS JOIN, listing every employee combined with every department.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_13.png)

## Self Join

A **SELF JOIN** is a regular join, but the table is joined with itself.

**SELF JOIN** has a different syntax, comparing to other joins:

```sql
SELECT a.column1, b.column2
FROM table a, table b
WHERE condition;
```

To showcase this type of join, let's add the `manager_id` column to the `employees`:

```sql
ALTER TABLE employees ADD COLUMN manager_id INT;
```

This column has a reference to `employees` table, without a foreign key.

Now we can make a **SELF JOIN**:

```sql
SELECT e1.name AS Employee, e2.name AS Manager
FROM employees e1, employees e2
WHERE e1.manager_id = e2.id;
```

In this example, we are joining the `employees` table to itself. This can be used to find pairs of `employees` where one is the `manager` of the other. Each row in the `employees` table is compared with every other row to find matching rows.

![Screenshot of SQL query result showing employees and their managers after a SELF JOIN, displaying pairs of employee and manager names from the same table.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_14.png)

## Union

The **UNION** operator stays aside from all joins. **UNION** is used to combine the results of multiple tables or `SELECT` statements into a single result set.

First, let's explore the syntax of **UNION** operator:

```sql
SELECT column1, column2, ...
FROM table1
UNION
SELECT column1, column2, ...
FROM table2;
```

Now, let's have a look at example:

```sql
SELECT 'Department' AS Type, name AS Name
FROM Departments
UNION
SELECT 'Employee' AS Type, name as Name
FROM employees;
```

The first `SELECT` statement retrieves data from the `departments` table, the other - from the `employees` table. The **UNION** operator combines these results into a single result set, where the `Type` column indicates whether the row represents a department or an employee.

![Screenshot of SQL query result showing combined names from departments and employees tables using UNION, with a 'Type' column indicating if the entry is a 'Department' or 'Employee'.](https://antondevtips.com/media/code_screenshots/databases/joins/img_db_joins_15.png)

By default, **UNION** eliminates duplicate rows from the result set. If you want to include duplicates, you can use **UNION ALL**.

```sql
SELECT 'Department' AS Type, name AS Name
FROM Departments
UNION ALL
SELECT 'Employee' AS Type, name as Name
FROM employees;
```

**There are few limitations when using a UNION statement:**

*   **Column Count and Data Types:** each `SELECT` statement within the `UNION` must have the same number of columns and the corresponding columns must have compatible data types.
*   **Order of Columns:** the order of columns must be the same in all `SELECT` statements.

You can sort the final result set using the `ORDER BY` clause after the last `SELECT` statement:

```sql
SELECT 'Department' AS Type, DepartmentName AS Name, Location AS Details
FROM Departments
UNION ALL
SELECT 'Employee', EmployeeName, JobTitle
FROM Employees;
```

## When To Use Each Type of Join

### Inner Join

Inner Join: Use this join when you need to retrieve only the rows that have matching values in both tables, which is ideal when you need intersection of the datasets.

### Left Join

Left Join: Use this join when you need all rows from the left table and the matching rows from the right table, including cases where there might not be a match, which is useful for keeping all data from the left table.

### Right Join

Right Join: Use this join when you need all rows from the right table and the matching rows from the left table, including cases where there might not be a match, which is useful for keeping all data from the right table.

### Full Join

Full Join: Use this join when you need all rows when there is a match in either left or right table, which is helpful for a complete view that includes all records from both tables regardless of matching.

### Cross Join

Cross Join: Use this join when you need to create a Cartesian product of the tables, which is typically used for generating combinations of rows or for testing purposes.

### Self Join

Self Join: Use this join when you need to compare rows within the same table, which is useful for hierarchical data or for finding relationships among rows in a single dataset.

### Union

Union: Use this operation when you need to combine the results of two or more SELECT queries into a single result set, eliminating duplicates, which is ideal for merging similar datasets from different sources.

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fa-complete-guide-to-different-types-of-joins-in-sql&title=A%20Complete%20Guide%20to%20Different%20Types%20of%20Joins%20in%20SQL)[X](https://twitter.com/intent/tweet?text=A%20Complete%20Guide%20to%20Different%20Types%20of%20Joins%20in%20SQL&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fa-complete-guide-to-different-types-of-joins-in-sql)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fa-complete-guide-to-different-types-of-joins-in-sql)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.