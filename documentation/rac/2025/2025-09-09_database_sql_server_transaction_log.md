```yaml
---
title: SQL Server Transaction Log
source: https://www.mssqltips.com/sqlservertip/8297/sql-server-transaction-log/
date_published: 2025-09-09T04:00:00.000Z
date_captured: 2025-09-10T19:29:34.471Z
domain: www.mssqltips.com
author: Muhammad Hassan Arshad
category: database
technologies: [SQL Server, SQL Agent, Change Data Capture, Availability Groups, Database Mirroring, Replication]
programming_languages: [SQL]
tags: [sql-server, database-administration, transaction-log, data-recovery, performance-tuning, backup, vlf, dba, logging, database-management]
key_concepts: [transaction-log, virtual-log-files, recovery-models, point-in-time-recovery, log-backups, log-truncation, log-shrinking, dbcc-commands]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a detailed explanation of SQL Server transaction logs, emphasizing their critical role in data recovery, integrity, and performance. It delves into the internal structure, including Virtual Log Files (VLFs), and offers practical steps for managing log growth and optimizing VLF count. The content covers essential topics such as log backups, point-in-time recovery, and log shrinking, alongside methods for monitoring log health using T-SQL commands and DMVs. The article also addresses common issues like swollen transaction logs and provides strategies for diagnosis and resolution, making it a comprehensive guide for data professionals.]
---
```

# SQL Server Transaction Log

# SQL Server Transaction Log

written by Muhammad Hassan Arshad September 9, 2025

## Problem

SQL Server transaction logs are critical and an important component for data recovery, integrity, and performance. Transaction logs record changes made to the database to ensure durable transactions and the database can be restored to a consistent state in case of any failures. However, many data professionals don’t, or they face difficulty understanding the functions of transaction logs internally, how Virtual Log Files affect the management of logs, and why taking backups of transaction logs is crucial. As a data professional, if we don’t understand all these details in-depth, we will face many issues, such as log growth (which becomes uncontrolled in many scenarios), inefficient backups, and recovery failures (which may impact the availability of the database), and all effect database performance drastically.

## Solution

This article will help to provide detailed information and educate you about how SQL Server transaction logs work internally. It will give practical steps to maintain and manage the health of the database for disaster recovery and improved performance.

## Understanding SQL Server Transaction Log Internals

Transaction logs basically record all the changes made to the SQL Server database sequentially. All transactions, whether it’s DML (INSERT, UPDATE, DELETE, etc.) or DDL (CREATE, ALTER, DROP, TRUNCATE), are written to the database a log file before it gets written to the data file. This ensures that the transaction is durable, which is an ACID property (atomicity, consistency, isolation, durability).

Key purpose:

*   Supports crash recovery
*   Enables point-in-time restores
*   Facilitates replication, database mirroring, availability groups, change data capture

![An illustration depicting a database server with connections to "Internals", "Virtual Log Files", "Logged Operations", and "Improve Performance", all centered around the concept of a "SQL Server Transaction Log".](https://www.mssqltips.com/wp-content/uploads/8297_SQL-Server-Transaction-Log.webp)

## Internal Structure: Virtual Log Files (VLFs)

VLFs are internal divisions of the LDF file (transaction log file). SQL Server generates multiple VLFs to manage the log space more efficiently. However, creating excessive VLFs can cause degradation in terms of performance.

### Viewing VLFs for a Database

This script lets you see all virtual log files that exist for the database transaction log file.

```sql
-- MSSQLTips.com (T-SQL)
-- View VLF count for a database
DBCC LOGINFO('MSSQLTipsDB');
```

Below, we can see the output.

![Screenshot showing the output of `DBCC LOGINFO('MSSQLTipsDB')`, displaying Virtual Log File (VLF) information for a database, including RecoveryUnitId, FileId, FileSize, StartOffset, FSeqNo, Status, Parity, and CreateLSN.](/wp-content/images-tips-8/8297_sql-server-transaction-log-for-data-integrity-and-recovery-1.webp)

### Steps for Reducing VLF Count for Better Performance

In some cases there can be an excessive amount of virtual log files which could impact performance. Here are some tips to keep the VLFs in check.

*   **Avoid frequent small growth increments.** The server automatically grows the size of log file when it is running out of space. If we set the auto-growth to a small size, like 1MB, it will create a lot of VLFs, which can impact performance. The solution is to use a larger fixed size, like 256MB or 512MB for example, for the auto-growth. Also, don’t use a percentage.
*   **Pre-size log files appropriately.** When you create a new database, set the log file size according to your expected usage. For example, if you create a database that is 10GB, create the initial log file around 20% to ensure there is enough space for your largest transactions. You can also adjust an existing database as follows:

```sql
-- MSSQLTips.com (T-SQL)
-- Set log file to appropriate initial size and auto growth
ALTER DATABASE MSSQLTipsDB
MODIFY FILE (NAME = MSSQLTipsDB_log, SIZE = 1GB, FILEGROWTH = 512MB);
```

## Demonstrating Log Operations with a Sample Database

We will run through a very simple example to show you what happens when there are database transactions and ways to check log usage.

### Create a Sample Database

Below we are creating a sample database based on the default create database settings.

```sql
-- MSSQLTips.com (T-SQL)
-- Sample schema to observe log behavior
CREATE DATABASE MSSQLTipsDB;
GO
USE MSSQLTipsDB;
GO
CREATE TABLE EMPLOYEES
(ID INT PRIMARY KEY IDENTITY(1,1),
F_NAME VARCHAR(50),
L_NAME VARCHAR(50),
AGE VARCHAR(50),
JOINING_DATE DATETIME,
JOB_ROLE VARCHAR(50)
);
```

### Perform Transactions

We then insert some rows into the table that was created.

```sql
-- MSSQLTips.com (T-SQL)
-- Insert statement
INSERT INTO EMPLOYEES VALUES 
('Hassan','Arshad','30','01-01-2025','DATA ENGINEER'),
('Ethan','Carter','32','01-01-2025','MANAGER'),
('Liam','Bennett','28','01-01-2025','ACCOUNTANT'),
('Ava','Thompson','25','01-01-2025','DBA'),
('Mia','Reynolds','29','01-01-2025','DATA ENGINEER'),
('Noah','Sullivan','23','01-01-2025','BI ENGINEER'),
('Sophia','Grant','24','01-01-2025','DATA ARCHITECT');
```

### Monitor Log Space Usage

We can then use this command to see how much log space was used. This will show the log file sizes for all databases and how much log space is currently being used. Based on the number of transactions, size of transactions, the recovery model used, and frequency of log backups the size and percentage could change.

```sql
-- MSSQLTips.com (T-SQL)
-- Monitor log usage
DBCC SQLPERF(LOGSPACE);
```

Below we can see the log file is only 8MB and 8% of the log was used for the above operations. This is just a very small example just to show you how you can check the log space usage.

![Screenshot of the `DBCC SQLPERF(LOGSPACE)` output, listing database names, their log sizes in MB, and the percentage of log space used. It highlights 'MSSQLTipsDB' with 7.99MB log size and 8.25% usage.](/wp-content/images-tips-8/8297_sql-server-transaction-log-for-data-integrity-and-recovery-2.webp)

## Log Backups and Truncation

When a database is set to FULL or BULK LOGGED recovery model the transaction log will continue to grow until a log backup occurs. If the database is in the SIMPLE recovery model there is not a way to do point in time recovery and also the transaction log file space can be used over and over again which will avoid continued growth of the transaction log file. The transaction log will grow if there are transactions that the current transaction log size cannot accommodate which all depends on size, recovery model and backup schedule.

### Why Backup the Transaction Log?

*   Prevents uncontrolled growth of log
*   Truncates inactive part of the log
*   Helps enable point-in-time recovery

### Example: Full Recovery Model with Log Backup

Here is a simple example to back the database and log. Ideally this would be scheduled with SQL Agent or some other tool on a set basis.

```sql
-- MSSQLTips.com (T-SQL)
-- Set recovery model
ALTER DATABASE MSSQLTipsDB SET RECOVERY FULL;
 
-- Take full backup
BACKUP DATABASE MSSQLTipsDB TO DISK = 'C:\mssqltips\MSSQLTipsDB.bak';
 
-- Perform transactions, then take a log backup
BACKUP LOG MSSQLTipsDB TO DISK = 'C:\mssqltips\MSSQLTipsDB_Log.trn';
```

Below we can see the full backup and log backup files that were generated from the above commands.

![Screenshot of a file explorer window showing a `.bak` file (full backup) and a `.trn` file (transaction log backup) for 'MSSQLTipsDB', located in the 'C:\mssqltips' directory.](/wp-content/images-tips-8/8297_sql-server-transaction-log-for-data-integrity-and-recovery-3.webp)

## Point-in-Time Recovery

This allows us to recover a database to the point in time right before a transaction we did not want occurred.

### Restore to a Specific Point in Time

Below is an example of how a point in time recovery for a database can be done.

We first restore the full backup with NORECOVERY and then restore the transaction log backup and use a specific date and time for the last transaction to be recovered. Any transaction that occurred after that time will not be restored.

```sql
-- MSSQLTips.com (T-SQL)
-- Restore with STOPAT
RESTORE DATABASE MSSQLTipsDB FROM DISK = 'C:\mssqltips\MSSQLTipsDB.bak'
WITH NORECOVERY;
 
RESTORE LOG MSSQLTipsDB FROM DISK = 'C:\mssqltips\MSSQLTipsDB_Log.trn'
WITH STOPAT = '2025-06-04 15:00:00', RECOVERY;
```

## Shrinking the Transaction Log

### When to Shrink

If a database transaction log gets very large it can use up excessive disk space. Also, depending on the auto-growth setting this growth could be exponential that is why it is a good idea to use a fixed size instead of a percentage for the growth. Keep in mind that the log file should be around 20% of the data file, although each database could be different based on use. 

Shrink the log if you need to free up disk space. Don’t include the shrink activity in your routine tasks, as it may impact performance. Also, try to do this during low production use time of your database.

### How to Shrink

The following performs a checkpoint which writes current in-memory modified pages and transaction log information from memory to disk and records the information in the transaction log. The backup log creates a transaction log backup. The shrink file command will attempt to shrink the transaction log to 1GB.

```sql
-- MSSQLTips.com (T-SQL)
USE MSSQLTipsDB
GO
CHECKPOINT;
-- It will trigger truncation
BACKUP LOG MSSQLTipsDB TO DISK = 'path\MSSQLTipsDB_Log.trn';
-- Shrink the log file
DBCC SHRINKFILE(MSSQLTipsDB_log, 1024); -- size in MB
```

This shows the output of the shrink file command. We can see the current size is

![Screenshot showing the output of `DBCC SHRINKFILE` command, indicating the current size of the 'MSSQLTipsDB_log' file after a shrink operation.](/wp-content/images-tips-8/8297_sql-server-transaction-log-for-data-integrity-and-recovery-4.webp)

## Monitoring Log Health

### Dynamic Management Views

Here are a couple DMVs that show database transactions (see what type of transaction and impact on log) and log space usage which is similar to **DBCC SQLPERF(LOGSPACE)**.

```sql
-- MSSQLTips.com (T-SQL)
-- View active transactions
SELECT * FROM sys.dm_tran_database_transactions;
 
-- View file usage
SELECT * FROM sys.dm_db_log_space_usage;
```

Output

![Screenshot displaying the output of `sys.dm_tran_database_transactions`, showing details of active transactions within databases.](/wp-content/images-tips-8/8297_sql-server-transaction-log-for-data-integrity-and-recovery-5.webp)

![Screenshot displaying the output of `sys.dm_db_log_space_usage`, showing log space usage details for various databases, including total log size, used log size, and percentage used.](/wp-content/images-tips-8/8297_sql-server-transaction-log-for-data-integrity-and-recovery-6.webp)

### Use fn\_dblog to Investigate Log Contents

```sql
-- MSSQLTips.com (T-SQL)
-- Read the transaction log
SELECT * FROM fn_dblog(NULL, NULL);
```

**What Does (NULL, NULL) Mean?**First, NULL is the starting Log Sequence Number. If we set it to NULL, then it means it will start from the very beginning of the active log. Similarly, the second NULL means it will go to the end of the log.

Output

![Screenshot showing a partial output of `fn_dblog(NULL, NULL)`, displaying detailed transaction log records, including LSN, Operation, Context, Transaction ID, and other log entry specifics.](/wp-content/images-tips-8/8297_sql-server-transaction-log-for-data-integrity-and-recovery-7.webp)

## How Often Should SQL Transaction Logs Be Backed Up?

The frequency of transaction log backups determines how much your data is at risk.

### Best Approaches

*   **Recovery models: Full or Bulk-Logged.** Log backups play an important role and are required to manage the growth of logs and enable point-in-time recovery.
*   **Commonly used backup frequencies**
    *   Every 5 to 15 minutes for critical OLTP databases/applications.
    *   Every 30 to 60 minutes for moderately critical systems.

### How to Automate

SQL Agent Jobs can be used to schedule this activity as per demand, or maintenance plans can be created. Both approaches can be used as best practice.

## How to Solve a Swollen SQL Transaction Log

A swollen transaction log is usually due to following reasons:

*   No recent logs backups taken in recovery mode (Full or Bulk-logged).
*   Long-running transactions.
*   Open transactions or transactions that are not committed properly.
*   Replication issues.

Follow these steps to diagnose the issue:

```sql
-- MSSQLTips.com (T-SQL)
-- Check for long-running queries or transactions
DBCC OPENTRAN('MSSQLTipsDB');
 
-- Check usage from the transaction log space
DBCC SQLPERF(LOGSPACE);
 
-- Check recovery model
SELECT name, recovery_model_desc FROM sys.databases WHERE name = 'MSSQLTipsDB';
```

Follow these strategies to fix the issue:

*   If using a FULL or Bulk-logged model, take a log backup.
*   Make sure there are no long-running queries.
*   Check features that are holding the log, e.g., replication (check sp\_repldone) and CDC (sys.dm\_cdc\_log\_scan\_sessions).
*   Perform shrink if required, but only perform this after a log backup.

## How to Truncate the SQL Server Transaction Log

Truncating the transaction log will not reduce the physical file size; it will only free up the internal space that is inside the log file. To shrink the log file physically, use the DBCC SHRINKFILE command.

### Methods

**In Simple Recovery**

```sql
-- MSSQLTips.com (T-SQL)
-- Truncation happen automatically on checkpoint
CHECKPOINT;
```

**In Full Recovery**

```sql
-- MSSQLTips.com (T-SQL)
-- Backup log as it will truncates the log and mark used portion for reuse
BACKUP LOG MSSQLTipsDB TO DISK = 'C:\mssqltips\MSSQLTipsDB_Manual.trn';
```

**Force Truncation** (Note: This is not recommended for Production environments)

```sql
-- MSSQLTips.com (T-SQL)
-- Change to SIMPLE Recovery Model (not recommended for production)
ALTER DATABASE MSSQLTipsDB SET RECOVERY SIMPLE;
CHECKPOINT;
DBCC SHRINKFILE (MSSQLTipsDB_Log, 5);
ALTER DATABASE MSSQLTipsDB SET RECOVERY FULL;
```

Note: Changing recovery models will disable point-in-time recovery until a full backup is issued along will transaction log backups.

Next Steps

*   Experiment with different advanced recovery models and investigate disaster recovery planning.
*   Automate log backups using SQL Agent by creating jobs.
*   Explore [sys.dm\_tran\_database\_transactions](https://learn.microsoft.com/en-us/sql/relational-databases/system-dynamic-management-views/sys-dm-tran-database-transactions-transact-sql) and [fn\_dblog](https://www.sqlshack.com/sql-server-transaction-log-explained/) for insights and analyzing log contents.