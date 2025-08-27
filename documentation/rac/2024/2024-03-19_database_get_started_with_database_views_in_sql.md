```yaml
---
title: Get Started With Database Views in SQL
source: https://antondevtips.com/blog/getting-started-with-database-views-in-sql
date_published: 2024-03-19T12:02:33.129Z
date_captured: 2025-08-06T17:14:50.095Z
domain: antondevtips.com
author: Anton Martyniuk
category: database
technologies: [PostgreSQL, MS SQL Server, Oracle]
programming_languages: [SQL]
tags: [database, sql, views, data-access, query-optimization, data-abstraction, security, ddl, dml]
key_concepts: [database-views, virtual-table, data-abstraction, query-simplification, data-security, create-view, drop-view, create-or-replace-view]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive introduction to database views in SQL. It defines views as virtual tables that encapsulate predefined SQL queries, highlighting their benefits such as simplifying complex queries, reducing repetition, enhancing data abstraction, and improving security. The guide demonstrates how to create, update, and delete views using SQL commands, including practical examples with PostgreSQL. It further illustrates the utility of views for complex queries, such as calculating total sales per customer and identifying top-selling products.
---
```

# Get Started With Database Views in SQL

![A dark blue background with abstract purple wave shapes. On the left, a white icon of angle brackets `< />` inside a square, with the text "dev tips" below it. On the right, large white text reads "GET STARTED WITH DATABASE VIEWS IN SQL". Below this, a thin white line and the text "Improve your coding skills".](https://antondevtips.com/media/covers/databases/cover_db_views.png)

# Getting Started With Database Views in SQL

Mar 19, 2024

3 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

## What Are Database Views?

A **database view** is a virtual table that contains data from one or multiple database tables. Unlike a physical table, a **view** does not store data itself. It contains a set of predefined SQL queries to fetch data from the database. In other words, **view** is a uniquely named object in the database that stores a SQL select query for repeatable data access.

> All SQL queries from this blog post were tested in the **Postgres database**. While the same SQL syntax can be used in other databases.

## Benefits of Using Views

*   **Simplification of complex queries** - views can encapsulate complex queries, allowing users to access the results through simple SELECT statements.
*   **Less repeatable select queries** - views encapsulate select queries that can be repeatedly used in the data access code.
*   **Data abstraction** - views provide a level of abstraction, allowing changes in underlying table structures without affecting how the data is presented to the end-users.
*   **Enhanced security** - views enhance security by protecting data, as they can limit access to certain rows and columns of the table, instead of giving access to the whole table.

## How to Create a View

To create a **view** in the database, you can use the **CREATE VIEW** SQL statement:

```sql
CREATE VIEW view_name AS
SELECT column1, column2, ...
FROM table_name
WHERE condition;
```

Let's set up a database in PostgreSQL that includes tables for customers, products, orders, and order_details:

```sql
CREATE TABLE customers (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    name VARCHAR(255),
    phone VARCHAR(20),
    email VARCHAR(255),
    is_active BOOLEAN
);

CREATE TABLE products (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    name VARCHAR(255),
    price DECIMAL
);

CREATE TABLE orders (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    customer_id INT,
    date DATE,
    FOREIGN KEY (customer_id) REFERENCES customers(id)
);

CREATE TABLE order_details (
    id INT PRIMARY KEY GENERATED ALWAYS AS IDENTITY,
    order_id INT,
    product_id INT,
    quantity INT,
    price DECIMAL(10, 2),
    FOREIGN KEY (order_id) REFERENCES orders(id),
    FOREIGN KEY (product_id) REFERENCES products(id)
);
```

You can use regular **SELECT** statement to get all customers:

```sql
SELECT * FROM customers;
-- OR
SELECT id, name, phone, email, is_active FROM customers;
```

Instead of repeating this query each time, we can define a database view that returns frequently used information about active customers:

```sql
CREATE VIEW customer_info AS
SELECT id, name, email, phone
FROM customers
WHERE is_active = true;
```

To retrieve data from the **view** you need to use view's name in the **SELECT** statement as a table name:

```sql
SELECT * FROM customer_info;
-- OR if you need less fields
SELECT id, name, email FROM customer_info;
```

When selecting data from the **view** you can use all **SELECT** statement features that you can use when selecting data from the real database table.

For example, you can use **grouping** to select unique customer names from the view:

```sql
SELECT name FROM customer_info
GROUP BY name;
```

## How to Update and Delete view

Database allows you to recreate a **view** with an updated **SELECT** and **WHERE** statements. Use the **CREATE OR REPLACE VIEW** SQL command:

```sql
CREATE OR REPLACE VIEW customer_info AS
SELECT id, name, email, phone
FROM customers
```

However, you can only **add** and can't **remove** columns from the **SELECT** statement. To completely change the view, you need to delete it from the database using the **DROP VIEW** SQL command:

```sql
DROP VIEW IF EXISTS customer_info;
```

## Database Views for Complex Queries

Views are particularly useful for creating complex SQL queries or presenting aggregated data from multiple tables.

Before creating a **view**, I recommend first to create a SQL query, test it and only after that create a **view**. This will reduce the time for recreating or even deleting a view before it was tested.

Let's create a view that returns the total sales per customer:

```sql
CREATE OR REPLACE VIEW total_sales_per_customer AS
SELECT customer.name, SUM(order_details.quantity * order_details.price) AS total_sales
FROM customers customer
JOIN orders "order" ON customer.id = "order".customer_id
JOIN order_details order_details ON "order".id = order_details.order_id
GROUP BY customer.name
ORDER BY total_sales desc;

-- SELECT data from the view
select * from total_sales_per_customer;
```

And a view that returns the top-10 frequently sold products:

```sql
CREATE OR REPLACE VIEW top_sold_products AS
SELECT product.name, COUNT(orderDetails.quantity * orderDetails.price) AS total_sales
FROM order_details orderDetails
JOIN products product ON orderDetails.product_id = product.id
GROUP BY product.name
ORDER BY total_sales desc
LIMIT 10;
-- in MS SQL Server use "SELECT TOP 10" to get top-10 rows
-- in Oracle use "WHERE ROWNUM <= 10;" with a select subquery to get top-10 rows

-- SELECT data from the view
select * from top_sold_products;
```

## Summary

A **database view** is a virtual table that contains data from one or multiple database tables. Unlike a physical table, a **view** does not store data itself, it provides an interface for repeatable data access.

**Database views** are a powerful feature in SQL, offering data abstraction, simplification of queries, reduction of repeatable queries and enhanced security through controlled data access.

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-database-views-in-sql&title=Getting%20Started%20With%20Database%20Views%20in%20SQL)[X](https://twitter.com/intent/tweet?text=Getting%20Started%20With%20Database%20Views%20in%20SQL&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-database-views-in%20SQL)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-database-views-in-sql)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.