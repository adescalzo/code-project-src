```yaml
---
title: Complete Guide To Transaction Isolation Levels in SQL
source: https://antondevtips.com/blog/complete-guide-to-transaction-isolation-levels-in-sql
date_published: 2024-05-04T11:00:30.116Z
date_captured: 2025-08-06T17:19:58.316Z
domain: antondevtips.com
author: Anton Martyniuk
category: database
technologies: [SQL Server, PostgreSQL, MySQL]
programming_languages: [SQL]
tags: [sql, database, transactions, isolation-levels, concurrency, data-integrity, performance, rdbms]
key_concepts: [transaction-isolation-levels, dirty-reads, non-repeatable-reads, phantom-reads, concurrency-control, data-consistency, locking, transaction-management]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to transaction isolation levels in SQL, explaining their importance for data integrity and performance. It details the four main isolation levels—Read Uncommitted, Read Committed, Repeatable Read, and Serializable—and the specific concurrency issues each addresses, such as dirty reads, non-repeatable reads, and phantom reads. The post includes practical SQL code examples demonstrating how to set these levels in MS SQL Server, PostgreSQL, and MySQL, alongside scenarios illustrating their behavior. It also discusses the advantages and disadvantages of each level, highlighting the trade-offs between consistency and performance. The guide concludes with advice on selecting the appropriate isolation level based on application needs, testing, and monitoring.]
---
```

# Complete Guide To Transaction Isolation Levels in SQL

![Banner image for the article "Complete Guide To Transaction Isolation Levels in SQL" featuring a code icon and "dev tips" text on a dark background with purple accents.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fdatabases%2Fcover_sql_isolation_levels.png&w=3840&q=100)

# Complete Guide To Transaction Isolation Levels in SQL

May 4, 2024

6 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Understanding **transaction isolation levels** in SQL is crucial for maintaining data integrity and performance in databases. This blog post explores the various isolation levels, their impact on data consistency and system performance, and how to implement them with code examples.

[](#introduction)

## Introduction

When multiple transactions execute concurrently in a database, various concurrency issues may occur, such as dirty reads, non-repeatable reads, and phantom reads. To manage these issues, SQL databases have different **transaction isolation levels**, which control what data changes are visible for transactions that are running concurrently.

SQL transactions support the following isolation levels:

1.  **Read Uncommitted**
    *   **Description:** The lowest level of isolation. Transactions may read changes made by other transactions even before they are committed, potentially leading to dirty reads.
    *   **Use Case:** Suitable in scenarios where accuracy is not critical and performance is a most priority.
2.  **Read Committed**
    *   **Description:** Default isolation level. Ensures that a transaction can only read data that has been committed before the transaction was started, preventing dirty reads but not non-repeatable reads.
    *   **Use Case:** Commonly used for most of the applications to prevent dirty reads.
3.  **Repeatable Read**
    *   **Description:** Prevents non-repeatable reads by ensuring that if a transaction reads a row, other transactions cannot modify or delete that row until the first transaction completes.
    *   **Use Case:** Useful in applications requiring consistent reads during a transaction but can lead to phantom reads.
4.  **Serializable**
    *   **Description:** The highest level of isolation. It simulates transactions being executed serially, thus preventing dirty reads, non-repeatable reads, and phantom reads.
    *   **Use Case:** Critical for applications that require complete isolation and data integrity, but it can severely impact performance due to intense locking.

[](#how-to-set-an-isolation-level)

## How To Set an Isolation Level

Let's have a look on examples how to set each isolation level for a transaction in different RDBMS.

[](#ms-sql-server)

### MS SQL Server

```sql
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
BEGIN TRANSACTION;
-- Your SQL statements here
COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL READ COMMITTED;
BEGIN TRANSACTION;
-- Your SQL statements here
COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL REPEATABLE READ;
BEGIN TRANSACTION;
-- Your SQL statements here
COMMIT TRANSACTION;

SET TRANSACTION ISOLATION LEVEL SERIALIZABLE;
BEGIN TRANSACTION;
-- Your SQL statements here
COMMIT TRANSACTION;
```

[](#postgresql)

### PostgreSQL

It is worth mentioning that PostgreSQL doesn't support **READ UNCOMMITTED** isolation level. As stated in [official docs](https://www.postgresql.org/docs/current/transaction-iso.html): PostgreSQL's Read Uncommitted mode behaves like Read Committed.

```sql
BEGIN TRANSACTION ISOLATION LEVEL READ COMMITTED;
-- Your SQL statements here
COMMIT;

BEGIN TRANSACTION ISOLATION LEVEL REPEATABLE READ;
-- Your SQL statements here
COMMIT;

BEGIN TRANSACTION ISOLATION LEVEL SERIALIZABLE;
-- Your SQL statements here
COMMIT;
```

[](#mysql)

### MySQL

```sql
SET SESSION TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
START TRANSACTION;
-- Your SQL statements here
COMMIT;

SET SESSION TRANSACTION ISOLATION LEVEL READ COMMITTED;
START TRANSACTION;
-- Your SQL statements here
COMMIT;

SET SESSION TRANSACTION ISOLATION LEVEL REPEATABLE READ;
START TRANSACTION;
-- Your SQL statements here
COMMIT;

SET SESSION TRANSACTION ISOLATION LEVEL SERIALIZABLE;
START TRANSACTION;
-- Your SQL statements here
COMMIT;
```

[](#advantages-and-disadvantages-of-isolation-levels)

## Advantages and Disadvantages of Isolation Levels

Now, as we briefly discussed each isolation level and their use cases, let's make a deep dive and explore the advantages and disadvantages of each isolation level.

1.  **Read Uncommitted**
    *   **Advantages:**
        *   Offers the highest performance among all isolation levels by allowing transactions to access uncommitted data, which minimizes lock contention and increases throughput.
    *   **Disadvantages:**
        *   Prone to dirty reads, where a transaction may read data that another transaction has written but not yet committed. This can lead to inconsistent and unreliable results if the other transaction is rolled back.
2.  **Read Committed**
    *   **Advantages:**
        *   Prevents dirty reads, ensuring that only committed data is visible to a transaction. Typically, offers better performance than higher isolation levels because it holds locks for a shorter duration.
    *   **Disadvantages:**
        *   Is susceptible to non-repeatable reads, where data read once in a transaction can change if read again due to other committed transactions.
3.  **Repeatable Read**
    *   **Advantages:**
        *   Prevents non-repeatable reads within the same transaction, ensuring that records read multiple times return the same data.
    *   **Disadvantages:**
        *   Can experience phantom reads, where new rows can be added by other transactions and be visible before the initial transaction completes. More locking overhead than lower isolation levels, potentially reducing concurrency.
4.  **Serializable**
    *   **Advantages:**
        *   Provides complete isolation from other transactions, preventing dirty reads, non-repeatable reads, and phantom reads.
    *   **Disadvantages:**
        *   Significantly reduces concurrency by locking large portions of the table or database, leading to potential performance bottlenecks. Highest potential for transaction serialization failures, requiring applications to handle transaction retries. Such transactions are vulnerable to deadlocks if two transactions wait for each other to unlock the same data.

[](#practical-examples-of-isolation-levels-behavior)

## Practical Examples of Isolation Levels Behavior

To demonstrate how different isolation levels behave in a practical scenario, let's consider two transactions working on the same data in a SQL database. Let's consider the following example with `accounts` table that has `account_id` and `balance` columns:

```sql
CREATE TABLE accounts (
    account_id INT PRIMARY KEY,
    balance DECIMAL(10, 2)
);

INSERT INTO accounts VALUES (1, 1000.00);
INSERT INTO accounts VALUES (2, 2000.00);
```

Now, let's explore how two simultaneous transactions interact with each other under different isolation levels.

> All SQL queries in the examples below were tested in the **Postgres database**. While the same SQL syntax can be used in other databases.

[](#1-read-uncommitted)

### 1\. Read Uncommitted

**Read Uncommitted** is the lowest isolation level and can suffer from **dirty reads**. A **dirty read** occurs when a transaction may read data that another transaction has written but not yet committed.

**Scenario:** Transaction 1 reads the `balance` for `account_id = 1`, while Transaction 2 is updating the balance.

```sql
-- Transaction 1
BEGIN TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
BEGIN TRANSACTION;
SELECT balance FROM accounts WHERE account_id = 1;  -- Might read uncommitted data

-- Transaction 2
BEGIN TRANSACTION;
UPDATE accounts SET balance = balance - 100 WHERE account_id = 1;
COMMIT;

-- Transaction 1 continues
SELECT balance FROM accounts WHERE account_id = 1;  -- Reads updated data
COMMIT;
```

**Outcome:** Transaction 1 may see the uncommitted change made by Transaction 2, leading to a dirty read. **Uncommitted** changes are the changes applied by `INSERT`, `UPDATE`, `DELETE` SQL commands, but are not committed to the database yet.

[](#read-committed)

### Read Committed

**Read Committed** isolation level solves **dirty reads** but can suffer from **non-repeatable reads**. A **non-repeatable read** occurs when data read once in a transaction can change if read again due to other committed transactions.

**Scenario:** Transaction 1 reads the same row multiple times, while Transaction 2 updates it between the reads of the 1st transaction.

```sql
-- Transaction Transaction 1
BEGIN TRANSACTION ISOLATION LEVEL READ COMMITTED;
BEGIN TRANSACTION;
SELECT balance FROM accounts WHERE account_id = 1;  -- Reads committed data

-- Transaction Transaction 2
BEGIN TRANSACTION;
UPDATE accounts SET balance = balance - 100 WHERE account_id = 1;
COMMIT;

-- Transaction Transaction 1 continues
SELECT balance FROM accounts WHERE account_id = 1;  -- Reads new committed data
COMMIT;
```

**Outcome:** Transaction 1 reads the `balance` twice and might see two different values if Transaction 2 commits between Transaction 1 SELECT statements. This is non-repeatable reads.

[](#repeatable-read)

### Repeatable Read

**Repeatable Read** isolation level solves **dirty** and **non-repeatable reads** but can suffer from **phantom reads**.

A **phantom read** occurs when a transaction re-executes a query returning a set of rows that satisfy a search condition and finds that the set of rows satisfying the condition has changed due to another committed transaction. This is different from a non-repeatable read in the number of rows returning from the query, rather than changes to the rows themselves.

**Scenario:** Transaction 1 reads multiple times all accounts that have `balance` more than or equal to 1000. Between these reads - Transaction 2 inserts a new account that has balance 2000.

```sql
-- Transaction 1 starts
BEGIN TRANSACTION ISOLATION LEVEL REPEATABLE READ;
SELECT * FROM accounts WHERE balance >= 1000; -- Returns 1 and 2 accounts

-- Meanwhile, Transaction 2 starts and commits
BEGIN;
INSERT INTO accounts (account_id, balance) VALUES (3, 1100.00);
COMMIT;

-- Transaction 1 continues and runs the same query again
SELECT * FROM accounts WHERE balance >= 1000; -- Now returns 1, 2 and 3 accounts
COMMIT;
```

**Outcome:** in Transaction 1, the second query unexpectedly includes an additional account, that was not part of the initial result set. This is a classic example of a phantom read. Transaction 1 reads a set of rows twice and finds more rows the second time due to the insertion committed by Transaction 2.

[](#serializable)

### Serializable

**Repeatable Read** is the highest isolation level that solves **dirty**, **non-repeatable reads** and **phantom reads**. Serializable isolation level effectively locks the range of records accessed, preventing new records from being added that match the queries of the ongoing transactions. But it can significantly decrease the overall performance of the database queries because of hard locked data rows.

**Scenario:** let's explore a previous example when a phantom read occurred.

```sql
-- Transaction Transaction 1
BEGIN TRANSACTION ISOLATION LEVEL SERIALIZABLE;
SELECT * FROM accounts WHERE balance >= 1000; -- Returns 1 and 2 accounts

-- Meanwhile, Transaction 2 starts and commits
BEGIN;
INSERT INTO accounts (account_id, balance) VALUES (3, 1100.00);
COMMIT;

-- Transaction 1 continues and runs the same query again
SELECT * FROM accounts WHERE balance >= 1000; -- Still returns 1 and 2 accounts
COMMIT;
```

**Outcome:** Both transactions operate as if they were executed serially and doesn't affect each other.

[](#summary)

## Summary

What Isolation Level to Select:

1.  **Read Uncommitted**. This isolation level offers the highest performance among all isolation levels. Use it when performance is crucial and when dirty reads won't have any impact on your application.
2.  **Read Committed**. This isolation level is enabled by default level and is recommended for most of the use cases as it prevents dirty reads and is highly performant.
3.  **Repeatable Read**. This isolation level is useful to prevent use cases when another transaction can modify rows of the currently running transaction. Consider that this isolation level is less performant than **Read Committed**.
4.  **Serializable**. This isolation level offers the highest data isolation and locking to prevent other transactions from adding new rows or modifying data. Consider this isolation level when it is critical for applications to have complete isolation and data integrity, but keep in mind that it can severely impact performance due to intense locking.

Consider the following advices when selecting an isolation level:

*   **Evaluate Needs:** Assess the criticality of data integrity versus system performance to choose the appropriate isolation level.
*   **Testing:** Always test the impact of changing isolation levels in a controlled environment before deploying changes to production.
*   **Monitoring:** Regularly monitor transaction performance and look for locks or other issues that could impact user experience.

Hope you find this newsletter useful. See you next time.

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fcomplete-guide-to-transaction-isolation-levels-in-sql&title=Complete%20Guide%20To%20Transaction%20Isolation%20Levels%20in%20SQL)[X](https://twitter.com/intent/tweet?text=Complete%20Guide%20To%20Transaction%20Isolation%20Levels%20in%20SQL&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fcomplete-guide-to-transaction-isolation-levels-in-sql)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fcomplete-guide-to-transaction-isolation-levels-in-sql)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.