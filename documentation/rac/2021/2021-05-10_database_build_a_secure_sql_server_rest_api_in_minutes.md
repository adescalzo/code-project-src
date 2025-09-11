```yaml
---
title: Build a Secure SQL Server REST API in Minutes
source: https://www.mssqltips.com/sqlservertip/6803/build-secure-sql-server-rest-api-minutes/
date_published: 2021-05-11T00:00:00.000Z
date_captured: 2025-09-10T19:29:48.200Z
domain: www.mssqltips.com
author: Jeremy Kadlec
category: database
technologies: [SQL Server, SQL Server Integration Services (SSIS), DreamFactory, Oracle, SalesForce, Snowflake, AWS, MySQL, Apache Hive, Hadoop, SOAP, REST, Active Directory, OAuth, GitHub, BitBucket, FTP, SFTP, Excel, Power BI, Tableau, Azure Data Factory, Azure Synapse Analytics, NodeJS, Python, PHP, Ruby]
programming_languages: [SQL, T-SQL, NodeJS, Python, PHP, Ruby]
tags: [sql-server, rest-api, api-management, data-access, security, middleware, low-code, database-integration, real-time-data, devops]
key_concepts: [REST API generation, data security, role-based access control, multitenancy, data masking, denial-of-service prevention, data mesh, low-code development]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article addresses the growing complexity of SQL Server environments, particularly the challenges of providing secure, real-time data access to third-party and cross-platform applications, noting the limitations of traditional SSIS and SQL Server Replication. It introduces DreamFactory as a middleware solution that can auto-generate and manage secure REST APIs for SQL Server and a wide array of other data sources without requiring manual code. The content provides a tutorial on building, testing, and securing a SQL Server REST API using DreamFactory's management interface, highlighting features such as role-based access control, multitenant security, data masking, and DoS attack prevention. DreamFactory aims to accelerate application development, unlock enterprise data, and simplify data integration across disparate systems for improved decision-making and reporting.]
---
```

# Build a Secure SQL Server REST API in Minutes

Problem

SQL Server environments are growing more complicated. This is in part due to the need to interface with an increasing number of third-party technologies. Providing business units and partners with real time SQL Server data access has only added to the challenge. This is due to the need to manage access in a secure, manageable, and compliant fashion. As SQL Server professionals, we’ve relied on SQL Server Integration Services (SSIS) to assist with these challenges. However, SSIS struggles with real time reporting requirements and facilitating the exchange of data across the enterprise. Alternatively, we can use native SQL Server Replication. But this too is difficult to configure and maintain within organizations tasked with managing highly dynamic database needs. A SQL Server REST API has become a viable alternative for real time data access apps.

Beyond the purely technical data exchange difficulties, data security is always a concern. This needs to be top-of-mind whenever granting access to third parties. Limiting data access, assigning appropriate permissions and securing data transmissions is imperative to properly protect client and internal data.

With these challenges in mind, how can we build a secure conduit between our cross-platform apps? How can we begin meeting our business requirements for data exchange, real time reporting and data security?

Solution

Traditionally, as SQL Server Developers and DBAs, we think of solving remote data access issues by consolidating data with SSIS or Replication. Although these are common approaches, both require significant time and money to build, test, deploy and maintain. Further, due to the increasing use of Continuous Integration \ DEVOPS \ SCRUM methodologies, databases and applications are changing more frequently. This poses additional challenges for SSIS and Replication managers.

If you’re looking for a streamlined and secure solution for accessing SQL Server relational database management system objects (stored procedures, functions, tables, views), then perhaps it is time to consider a new option to securely address data access across the enterprise. Beyond SQL Server, DreamFactory can auto-generate and manage APIs and documentation for a wide variety of data sources, including:

*   21+ Databases – SQL Server, Oracle, SalesForce, Snowflake, AWS, MySQL, etc.
*   Big Data – Apache Hive, Hadoop, etc.
*   Remote SOAP to REST services
*   Universal Web Connector
*   Support for many identity providers, including Active Directory and OAuth
*   Automated generation of REST APIs for version control (GitHub and BitBucket)
*   FTP and SFTP servers, Excel, flat files, etc.

In the simplest terms, [DreamFactory](https://www.dreamfactory.com/about?utm_source=mssqltips) is middleware that connects all of your applications across the enterprise in a secure manner. DreamFactory interrogates your database and associated objects, then builds a REST API. This is used for seamless JSON- and XML-based data access and integration. Using the DreamFactory management interface, you can create a secure REST API in minutes without writing a single line of code to build your dataset! Let’s dive in and check it out.

## Tutorial – Build a REST API for SQL Server

Building a REST API in DreamFactory starts in the API Generation tab. DreamFactory is able to generate APIs for many different sources. The SQL Server connector is located in the database section as shown below.

![DreamFactory UI showing API Types, with "Database" selected and "SQL Server" highlighted among database options.](/wp-content/images-tips/6803_dream-factory-sql-server-api.001-1.png)

Then, you’ll start generating the SQL Server API by first specifying a namespace, label and description. The latter two are just used for referential purposes within the administration console. The namespace serves a more important role of being used as a namespace within the generated API URI structure.

![DreamFactory "Create Database API" form, specifying Namespace, Label, and Description for a new SQL Server API.](/wp-content/images-tips/6803_dream-factory-sql-server-api.002-2.png)

Additional security options on this interface pertain to generating a read-only API Read Only data access and configuring SSL-based communication with authentication to the API endpoints.

![DreamFactory "Create Database API" form, showing advanced security options like "Read Only" and "Encrypted communication" for the SQL Server connection.](/wp-content/images-tips/6803_dream-factory-sql-server-api.003-3.png)

Once the API configuration is saved, DreamFactory builds the API in a matter of seconds (even with thousands of objects). It also generates interactive Swagger documentation. To view the documentation, you can navigate to the API Docs Tab. This has all the available API endpoints including those used to access schemas, tables, views, functions and stored procedures:

![DreamFactory API Docs interface, listing available API endpoints for schemas, tables, views, functions, and stored procedures for the generated SQL Server API.](/wp-content/images-tips/6803_dream-factory-sql-server-api.004-4.png)

![DreamFactory API Docs interface, expanded to show specific API endpoints for tables, including GET, POST, PUT, PATCH operations.](/wp-content/images-tips/6803_dream-factory-sql-server-api.005-5.png)

To learn more about the API generation process, check out the video [Generating a SQL Server API in Less Than 5 Minutes](https://blog.dreamfactory.com/creating-a-microsoft-sql-server-api-in-less-than-5-minutes-with-dreamfactory/?utm_source=mssqltips).

## Test the REST API from SQL Server

Once the DreamFactory API has been built, testing can begin for functionality, performance and security. Starting with functionality, we will show a simple test to retrieve data from the Sales.Customer table in the WorldWideImporters database. In this API request example, rather than returning all of the columns, only the CustomerName column is returned in the dataset. This can be easily modified to include additional columns from your MS SQL Server database.

![Screenshot of an API request in DreamFactory for a GET operation on the Sales.Customer table, with a filter to return only the CustomerName column.](/wp-content/images-tips/6803_dream-factory-sql-server-api.006-6.png)

Here are the associate records for this API call:

![JSON response showing the results of the GET request for Sales.Customer table, displaying only CustomerName values.](/wp-content/images-tips/6803_dream-factory-sql-server-api.007-7.png)

This second example demonstrates calling the Website.SearchForCustomers stored procedure in the WorldWideImporters database, including passing two input parameters to the stored procedure (SearchText and MaximumRowsToReturn):

![Screenshot of an API request in DreamFactory to execute the Website.SearchForCustomers stored procedure with SearchText and MaximumRowsToReturn parameters.](/wp-content/images-tips/6803_dream-factory-sql-server-api.008-8.png)

In this example, the result set returned is JSON:

![JSON response showing the result set from the execution of the Website.SearchForCustomers stored procedure.](/wp-content/images-tips/6803_dream-factory-sql-server-api.009-9.png)

DreamFactory’s [Getting Started Guide](https://guide.dreamfactory.com/docs/?utm_source=mssqltips) offers many additional examples demonstrating various CRUD-based API calls.

## DreamFactory Roles to Secure Database Objects

DreamFactory’s role-based access control manager is a crucial feature used by administrators to grant fine-grained permissions to specific database objects. The process starts with Roles Overview where the Role Name, Description and Role Status are specified.

![DreamFactory Roles Overview interface, used to define a new role with a name, description, and status.](/wp-content/images-tips/6803_dream-factory-sql-server-api.010-10.png)

Next, you’ll choose the desired service (API), which then prompts you to choose which corresponding Endpoints (Schemas, Tables, Functions and Stored Procedures) are accessible through the API.

![DreamFactory role configuration, selecting a service (API) and its corresponding endpoints (schemas, tables, functions, stored procedures) for access control.](/wp-content/images-tips/6803_dream-factory-sql-server-api.011-11.png)

The next step on the Access Overview is to specify the Access (GET, POST, PUT, PATCH or DELETE) for the Endpoint.

![DreamFactory Access Controls interface, allowing specification of HTTP methods (GET, POST, PUT, PATCH, DELETE) for selected API endpoints.](/wp-content/images-tips/6803_dream-factory-sql-server-api.012-12.png)

In this example, multiple stored procedures are granted permissions from a single service, but there could be multiple services and hundreds of Endpoints specified to properly restrict permissions needed in an enterprise application.

![DreamFactory Final Access Overview, summarizing the permissions granted to a role for multiple stored procedures within a service.](/wp-content/images-tips/6803_dream-factory-sql-server-api.013-13.png)

## Multitenant Database Security Example

DreamFactory includes native support for multitenant database security. A specific value such as a CustomerID or variable can be passed with all query or stored procedure executions. In the example below, we can specify a field equal to a specific value such as “WHERE CustomerID = 15” that will ensure proper security across the database.

![DreamFactory Access Overview demonstrating multitenant database security by applying a WHERE clause (e.g., CustomerID = 15) to API requests.](/wp-content/images-tips/6803_dream-factory-sql-server-api.014-14.png)

For a time comparison between DreamFactory vs. creating an API from scratch, check out this resource – [API Calculator – DreamFactory Software](https://calculator.dreamfactory.com/?utm_source=dfcomblog&utm_medium=blog&utm_campaign=traffic).

## Preventing Denial-of-Service API Attacks with DreamFactory’s Volume Limiting Feature

Another layer of security in DreamFactory is Limits, which restricts access to an API to prevent a DOS attack during a specific period of time. If traffic exceeds a particular threshold for an Instance, User, Service, Role, etc., then DreamFactory will begin returning a 429 status code and cease further interaction with the backend data source.

![DreamFactory Limit Configuration interface, used to set volume limits on API access to prevent Denial-of-Service attacks.](/wp-content/images-tips/6803_dream-factory-sql-server-api.015-15.png)

## Data Masking with DreamFactory

Protecting sensitive data is critical to ensuring adherence to various compliance and regulatory requirements regardless of industry. If data masking is not possible at the database level, then DreamFactory is able to mask or entirely anonymize the data with the internal DreamFactory scripting engine. The scripting engine supports multiple languages, including NodeJS, Python (versions 2 and 3), PHP, and Ruby (coming soon in 2021 Q2). Using these languages, you can easily add data masking logic which can manipulate both the request and response.

See this article to learn more about [De-identifying sensitive API data](https://www.dreamfactory.com/developers/scripts/de-identifying-sensitive-api-data?utm_source=mssqltips).

## Consolidating Result Sets with DreamFactory

Another valuable feature in DreamFactory is the ability to combine result sets from multiple tables across multiple databases and have the data appear as a single data source. This feature is known as Data Mesh, and can be configured using the Schema Manager where virtual relationships among data from different database platforms can be established. This means data from your ERP, accounting, and CRM systems can be returned as a single result set for decision making and reporting that could not be accessed in real time without DreamFactory.

![DreamFactory Schema Manager Overview, illustrating the creation of virtual relationships for Data Mesh functionality, combining data from different sources.](/wp-content/images-tips/6803_dream-factory-sql-server-api.016-16.png)

## DreamFactory Use Cases – SQL Server REST API

DreamFactory helps organizations rapidly address numerous core business needs in a secure and high-performance manner. Let’s dive into a number of scenarios to understand DreamFactory’s value.

### Enterprise Business Intelligence

As Enterprises have expanded globally as well as grown via mergers and acquisitions, the associated data infrastructure has become very complex. What has not changed is Mid-Level and Executive Management’s need for real time data across numerous platforms for accurate decision making. Traditionally, these reporting needs were addressed with Data Warehouse \ Data Mart projects, which can be very expensive and time consuming. Further this data is often not available in real time. With DreamFactory, enterprises are able to seamlessly access data from the API to consolidate data from numerous systems across numerous locations and visualize the data in a single report with Power BI or Tableau. DreamFactory’s flexible and high-performance API enables the consolidation of data with the ability to scale as the Enterprise grows and shifts to meet client needs.

### SQL Server Security Lockdown

SQL Server stored procedure data access has long been relied upon to encapsulate business logic, secure data and limit the application surface area. With DreamFactory, an API can be built just for specific stored procedures across numerous servers and databases. This enables the SQL Developers to properly build and tune the T-SQL code, return only the needed data and setup permissions while the Front-End Developers can call the stored procedure and focus on the application aesthetics, enterprise reporting and enterprise metrics. You no longer have to be impacted by Linked Server performance, scheduled SSIS extracts or Replication snafus. DreamFactory enables your team to focus on their strengths and act as the needed conduit to enable the necessary data access seamlessly across the enterprise.

Check out this additional resource on [How to Code a REST API for Microsoft SQL Server: The Hard Way and the Easy Way.](https://blog.dreamfactory.com/how-to-code-a-rest-api-for-microsoft-sql-server-the-hard-way-and-the-easy-way/?utm_source=mssqltips)

### Low Code Developer Tool Integration

With the popularity of Low Code Developer tools to quickly build and deploy applications, DreamFactory enables SQL Server DBAs and Developers to design and build the data model, then code stored procedures for database access. Then they can meet the Front-End Developers at the DreamFactory middleware to connect the front-end tool with the back end stored procedures from a single SQL Server, a cloud-based instance or from numerous databases distributed across the organization. DreamFactory helps to simplify and standardize database access, reduce the time needed for application development and deployment, as well as help Low Code Developer Tools meet their goal of rapidly building applications to meet the organizational needs.

### Enterprise Data Silos

Whether it is ERP, CRM, SCM, PM or HR applications, in most organizations these systems are mission critical and are siloed data stores with a proprietary interface. Accessing this data can be extremely difficult when building custom reports and dashboards for decision making. DreamFactory enables your system Subject Matter Experts to unlock valuable data for the enterprise with a simple and secure API.

### Multi-Platform Relational Databases

In large enterprises, multiple database platforms including public clouds are common. Unfortunately, accessing data in real time across these disparate databases is time consuming, a moving target and can quickly become a mess with duplicate data, overlapping code and more. In some circumstances there are native access options, and typically ETL (Extract, Transform and Load) processes with SQL Server Integration Services (SSIS), Azure Data Factory or Azure Synapse Analytics are needed to move data or files between systems. DreamFactory’s access to 50+ applications and 20+ databases, simplifies this data access process and eliminates Linked Servers, Replication and ETL nightmares.

### SQL Server Express Proliferation @ Retail Locations

At retail branches around the United States and the globe, SQL Server Express is often installed on a local machine to support day to day operations on the front lines for the organization. In this scenario, the Retailer needs to ensure consistent operations at the store even if connectivity is unavailable or a centralized application is down. However, data (pending orders, purchases, inventory, shipments, employee scheduling, etc.) from each retail branch are critical to understand trends, customer demand, promotional performance and more. This valuable data in SQL Server Express (as a free product) has limited options for data sharing. This is another distributed environment common scenario where DreamFactory demonstrates significant value. By building an API for real time data access to the SQL Server Express data across the enterprise.  This data which can be consolidated into a single report or dashboard to drive decision making.

## DreamFactory Value Proposition as a SQL Server REST API

DreamFactory delivers significant time savings serving as middleware to build APIs across numerous database and application platforms. With DreamFactory, SQL Server Developers and DBAs no longer have to generate and move flat files, create SSIS Packages, support Replication or rely on Linked Servers to gain real time insights into enterprise data. DreamFactory enables enterprises to:

*   Accelerate application development across all database platforms
*   Unlock access to all enterprise data for decision making
*   Consolidate data from numerous platforms into a single result set
*   Encrypt data during transmission
*   Assign granular data access permissions
*   Protect multitenant databases
*   Seamlessly plugin to your enterprise

Next Steps

*   How do I get started with DreamFactory?
    *   [Create a Microsoft SQL Server REST API (dreamfactory.com)](https://blog.dreamfactory.com/creating-a-microsoft-sql-server-api-in-less-than-5-minutes-with-dreamfactory/?utm_source=mssqltips)
*   How can I learn more about DreamFactory?
    *   [API Blog from DreamFactory](https://blog.dreamfactory.com/?utm_source=mssqltips)
    *   [DreamFactory Getting Started Guide](https://guide.dreamfactory.com/docs/?utm_source=mssqltips)
    *   [DreamFactory Docs](https://wiki.dreamfactory.com/Main_Page?utm_source=mssqltips)

_MSSQLTips.com Product Spotlight sponsored by DreamFactory._