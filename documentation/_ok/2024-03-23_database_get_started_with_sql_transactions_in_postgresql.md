```yaml
---
title: Get Started with SQL Transactions in PostgreSQL
source: https://antondevtips.com/blog/get-started-with-sql-transactions-in-postgresql
date_published: 2024-03-23T12:00:34.061Z
date_captured: 2025-08-06T17:16:12.216Z
domain: antondevtips.com
author: Anton Martyniuk
category: database
technologies: [PostgreSQL]
programming_languages: [SQL]
tags: [sql, transactions, postgresql, database, data-integrity, acid, atomicity, consistency, isolation, durability]
key_concepts: [SQL transactions, ACID principles, BEGIN command, COMMIT command, ROLLBACK command, Transaction isolation levels, Common Table Expressions (CTEs), Data integrity]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article provides an introduction to SQL transactions, highlighting their role in ensuring data integrity and consistency in database operations. It elaborates on the ACID principles (Atomicity, Consistency, Isolation, Durability) that govern transactions. The content demonstrates how to initiate, commit, and rollback transactions in PostgreSQL using practical SQL commands. A real-world scenario involving customer and order creation showcases the atomic nature of transactions, ensuring all related data modifications are saved or none are. This guide is ideal for understanding fundamental database transaction concepts.]
---
```

# Get Started with SQL Transactions in PostgreSQL

![A dark blue banner with abstract purple shapes. On the left, a white square icon with a code tag `< />` and the text "dev tips" below it. On the right, large white text reads "GET STARTED WITH SQL TRANSACTIONS IN POSTGRESQL". Below this, a thin white line and the text "Improve your coding skills".](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fdatabases%2Fcover_db_transactions.png&w=3840&q=100)

# Get Started with SQL Transactions in PostgreSQL

Mar 23, 2024

4 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

## Introduction to SQL Transactions

**SQL transactions** are essential for ensuring data integrity and consistency in database operations. They allow multiple SQL commands to be executed as a single, atomic operation, meaning either all commands are successfully executed, or none are, ensuring the database remains in a consistent state. This blog post introduces the concept of **SQL transactions**, with a focus on their implementation in PostgreSQL, using a practical database model as an example.

A **transaction** in SQL is a sequence of one or more SQL operations performed as a single unit. Transactions follow the **ACID principles**:

*   **Atomicity** - ensures that all operations within a transaction are completed successfully; if at least one operation failed - the transaction is aborted.
*   **Consistency** - guarantees that a transaction takes the database from one valid state to another.
*   **Isolation** - ensures that the concurrent execution of transactions leaves the database in the same state as if the transactions were executed one after another.
*   **Durability** - ensures that once a transaction has been committed, it will remain so, even in the event of a power loss, crashes, or errors.

> All SQL queries from this blog post were tested in the **Postgres database**. While the same SQL syntax can be used in other databases.

## How to Create and Commit a Transaction

To start a **transaction** in PostgreSQL, use the `BEGIN` command. All SQL operations that come after the `BEGIN` command - are a part of the transaction until it's either **committed** or **rolled** back.

*   **COMMIT** - ensures that all changes made to the database are persisted (saved).
*   **ROLLBACK** - ensures that all changes made to the database are rejected (not saved).

Here is the general syntax of creating a SQL **transaction**:

```sql
-- Start transaction
BEGIN;

COMMIT;
-- or ROLLBACK:
```

All databases including PostgreSQL has a feature called **transaction isolation levels**, which control how concurrent transaction changes are visible to each other. To set an isolation level, you can use the `SET TRANSACTION` command right after `BEGIN` statement:

```sql
BEGIN;
SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;

-- Transaction operations here...

COMMIT;
```

Transaction isolation levels is a big topic and deserves a separate blog post.

Within a transaction, you can perform various operations such as selecting, inserting, updating, or deleting records across multiple tables.

If any operation within the transaction fails or if you detect an error in your logic, you can undo all operations within the transaction using the `ROLLBACK` command. This reverts the database to its state before the transaction began and all changes are not saved into the database.

## Real-World Use Case with Transactions

Let's set up a database in PostgreSQL that includes tables for customers, products, orders, and order\_details:

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

Imagine an online store where you order a product and register in a single step. We can create a new customer, create an order, and add order details into the database as a single transaction, ensuring data integrity. Here's an example of such a transaction:

```sql
-- Start transaction
BEGIN;

WITH new_customer AS (
    -- Return a created customer id
    INSERT INTO customers (name, phone, email, is_active)
    VALUES ('Jack Sparrow', '123-456-7890', 'captain@jack.com', TRUE)
    RETURNING id
),
new_order AS (
    -- Use the saved customer ID to create a new order
    -- Return a created order id
    INSERT INTO orders (customer_id, date)
    VALUES ((SELECT id FROM new_customer), '2024-02-20')
    RETURNING id
),
selected_product AS (
    SELECT id as product_id, price as product_price FROM products
    WHERE name = 'Samsung Galaxy S24 Ultra'
)

-- Use the saved details to create order details
INSERT INTO order_details (order_id, product_id, quantity, price)
SELECT
    (SELECT id FROM new_order) AS order_id,
    product_id,
    2 AS quantity,
    product_price * 2
FROM
    selected_product;

COMMIT;
```

Here we register a new customer "Jack Sparrow" and place an order for him with 2 mobile phones "Samsung Galaxy S24 Ultra".

Here **customer**, **order** and **order\_details** are all saved into database, or none of them is saved. This ensures **atomicity** when all data modifications are saved to the database as a whole.

Of course, in a production system you should check if there is a sufficient quantity of products in the **storage** tables before creating an order. If not - you can `REVERT` the transaction and show a corresponding error to a user or send an email with rejected order.

## Summary

Transactions ensure that your database operations are executed safely, following the **ACID principles**: Atomicity, Consistency, Isolation and Durability. Within a transaction, you can perform various operations such as selecting, inserting, updating, or deleting records across multiple tables.

Transaction ensures that all operations within a transaction are completed successfully; if at least one operation failed - the transaction is aborted.

Transaction supports 2 states:

*   **COMMIT** - ensures that all changes made to the database are saved.
*   **ROLLBACK** - ensures that all changes made to the database are not saved.

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fget-started-with-sql-transactions-in-postgresql&title=Get%20Started%20with%20SQL%20Transactions%20in%20PostgreSQL)[X](https://twitter.com/intent/tweet?text=Get%20Started%20with%20SQL%20Transactions%20in%20PostgreSQL&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fget-started-with-sql-transactions-in-postgresql)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fget-started-with-sql-transactions-in-postgresql)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.