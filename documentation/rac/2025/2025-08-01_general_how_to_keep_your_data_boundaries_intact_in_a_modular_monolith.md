```yaml
---
title: How to Keep Your Data Boundaries Intact in a Modular Monolith
source: https://www.milanjovanovic.tech/blog/how-to-keep-your-data-boundaries-intact-in-a-modular-monolith
date_published: 2025-08-02T00:00:00.000Z
date_captured: 2025-08-20T19:01:27.036Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: general
technologies: [PostgreSQL, Entity Framework Core, .NET, ASP.NET Core, MongoDB Atlas, Azure AI Foundry, Postman]
programming_languages: [C#, SQL]
tags: [modular-monolith, data-isolation, database-design, postgresql, entity-framework-core, dotnet, software-architecture, data-boundaries, orm, database-security]
key_concepts: [modular-monolith-architecture, data-isolation, database-schemas, database-roles, multiple-dbcontexts, row-level-security, database-views, cross-cutting-concerns]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details how to maintain strict data boundaries within a modular monolith architecture, addressing the challenge of preventing modules from directly accessing each other's data. It proposes a practical strategy leveraging PostgreSQL features like schemas and roles, combined with Entity Framework Core's multi-DbContext capabilities. The core idea involves assigning each module its own database schema and a dedicated role with limited privileges, ensuring data encapsulation at the database level. For cross-cutting queries, the article recommends using read-only database views or dedicated read models to preserve boundaries and facilitate future microservice extraction. This approach aims to enforce architectural discipline while keeping operational overhead manageable.]
---
```

# How to Keep Your Data Boundaries Intact in a Modular Monolith

![How to Keep Your Data Boundaries Intact in a Modular Monolith](/blog-covers/mnw_153.png?imwidth=3840)

# How to Keep Your Data Boundaries Intact in a Modular Monolith

7 min read · August 02, 2025

**Unlock AI Success with MongoDB Atlas on Azure - Final Webinar**
Successful AI applications require a secure, scalable data foundation. Join MongoDB and Microsoft on August 6 for From Vectors to Value, the final webinar in this series, and learn how leading enterprises are leveraging MongoDB Atlas and Azure AI Foundry to build powerful AI-driven applications with native vector search and real-time reasoning. **Register now**.

**Share your thoughts about API development**🧠
Postman wants to learn how developers are really building and scaling APIs - what's working, what's broken, and how AI is changing the game (for better or worse). This year's survey covers: How AI is reshaping API development; What teams are actually doing for security and monitoring; How real-world teams are collaborating and testing in 2025. It only takes ~10-15 minutes. You'll also be entered to win an iPad Pro M4 (with the good accessories).

👉**Take the survey**. Your input helps keep this industry honest - and evolving.

Sponsor this newsletter

**Modular monoliths** promise the productivity of a monolith and the clear boundaries of microservices. They work because each module is self-contained: its domain model, behavior and data live behind a boundary. But one of the hardest places to maintain those boundaries is in the database.

Nothing stops a developer from running a rogue `JOIN` across tables or bypassing a public API.

In previous articles, I described **four levels of data isolation** (table, schema, database and alternative persistence) and argued that **modules should expose explicit APIs** to access their data.

This article goes deeper on the database side. You'll learn how to carve out logical and physical boundaries using PostgreSQL schemas and roles and EF Core, why those choices matter, and how to handle cross-cutting queries without breaking encapsulation.

## Why enforce database boundaries?

In a modular monolith each module **owns its data**. If Module A reaches into Module B's tables, you lose this constraint and your modules become tightly coupled. Instead, B exposes a public API and hides its persistence logic from consumers.

Beyond clean code, enforcing boundaries at the database level protects you against mistakes and makes it easier to extract a module into its own service later.

Database schemas act like folders: they let you organise objects and share a database among many users, but a user can access any schema only if they have privileges. This means we can deliberately lock each module to its own schema.

**The strategy**

*   Create a schema per module and a dedicated database role.
*   Grant that role privileges only on its schema and set its default search path.
*   Use EF with one `DbContext` per module, setting a default schema and connection string per module.
*   For cross-cutting queries, publish a read-only database view that acts like a public API.
*   _Optional_: Use row-level security policies to restrict access within a table.

These practices give us **enforceable boundaries** while keeping the **operational overhead low**.

## Schemas, roles and search paths

PostgreSQL lets you create multiple schemas within a single database. A module can define its own schema and a role that owns it. The role only has usage and table-level privileges on that schema. For example, for an orders module:

```sql
-- create a user for the module and its schema
CREATE ROLE orders_role LOGIN PASSWORD 'orders_secret';
CREATE SCHEMA orders AUTHORIZATION orders_role;
GRANT USAGE ON SCHEMA orders TO orders_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA orders TO orders_role;
ALTER ROLE orders_role SET search_path = orders;
```

The `ALTER ROLE` command sets the role's default search path so unqualified names resolve to the module's schema. If you don't want to rely on the search path, always qualify table names (`orders.table_name`).

Row-level security (RLS) allows you to filter rows based on a policy expression. You enable RLS on a table and define a policy referencing `current_user` or columns:

RLS is powerful for multi-tenant scenarios or sensitive data, but it adds complexity. Start with schemas and roles and add RLS only when necessary.

## Configuring EF schemas and multiple DbContexts

I assume readers are comfortable with EF Core, so we'll skip the basics and focus on what matters for modular monoliths:

*   Use [**one DbContext per module**](using-multiple-ef-core-dbcontext-in-single-application). Each context contains only the entities of its module, and you call `modelBuilder.HasDefaultSchema("orders")` in `OnModelCreating` to map entities to the correct schema. Setting a default schema also affects sequences and migrations.
*   Provide a connection string per module using the module's role. Even if modules share the same database, separate credentials ensure that a misconfigured context cannot access another schema.
*   Configure the migrations history table in each context using `MigrationsHistoryTable("__EFMigrationsHistory", "orders")` so that EF's migration metadata stays within the module's schema.

These settings ensure EF Core queries and migrations respect the boundaries established by the database.

## Step-by-step: Enforcing module boundaries

Suppose we have two modules (**Orders** and **Shipping**) and we want to enforce boundaries between them. Here's what we need to do:

1.  **Create schemas and roles**. Use SQL to create orders and shipping schemas and their corresponding roles (`orders_role`, `shipping_role`). Grant each role privileges only on its schema.

    ```sql
    -- Orders schema and role
    CREATE ROLE orders_role LOGIN PASSWORD 'orders_secret';
    CREATE SCHEMA orders AUTHORIZATION orders_role;
    GRANT USAGE ON SCHEMA orders TO orders_role;
    GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA orders TO orders_role;
    ALTER ROLE orders_role SET search_path = orders;

     -- Shipping schema and role
     CREATE ROLE shipping_role LOGIN PASSWORD 'shipping_secret';
     CREATE SCHEMA shipping AUTHORIZATION shipping_role;
     GRANT USAGE ON SCHEMA shipping TO shipping_role;
     GRANT SELECT, INSERT, UPDATE, DELETE ON ALL TABLES IN SCHEMA shipping TO shipping_role;
     ALTER ROLE shipping_role SET search_path = shipping;
    ```

2.  **Add connection strings**. In configuration, define separate connection strings per module:

    ```json
    {
      "ConnectionStrings": {
        "Orders": "Host=localhost;Database=appdb;Username=orders_role;Password=orders_secret",
        "Shipping": "Host=localhost;Database=appdb;Username=shipping_role;Password=shipping_secret"
      }
    }
    ```

3.  **Define DbContexts**. Each module defines its own context and sets the default schema. For the Orders module:

    ```csharp
    public class OrdersDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; } = default!;
        public DbSet<OrderLine> OrderLines { get; set; } = default!;

        public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // set default schema for all entities in this context
            modelBuilder.HasDefaultSchema("orders");
            // optional: configure tables explicitly
            modelBuilder.Entity<Order>().ToTable("orders");
            modelBuilder.Entity<OrderLine>().ToTable("order_lines");
            base.OnModelCreating(modelBuilder);
        }
    }
    ```

    Register each context with its connection string and specify the migrations history table:

    ```csharp
    builder.Services.AddDbContext<OrdersDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Orders"),
            o => o.MigrationsHistoryTable("__EFMigrationsHistory", "orders")));

    builder.Services.AddDbContext<ShippingDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("Shipping"),
            o => o.MigrationsHistoryTable("__EFMigrationsHistory", "shipping")));
    ```

4.  **Maintain migrations separately**. Because each module has its own context and schema, you maintain migrations separately. When generating a migration, specify the context:

    ```bash
    dotnet ef migrations add InitialOrders --context OrdersDbContext --output-dir Data/Migrations/Orders
    ```

    Repeat this for the Shipping context. EF Core will generate migration classes that create tables within the specified schema (because of `HasDefaultSchema`). Remember to apply the migrations in the correct order when deploying. You can automate this by having a migration runner iterate through the contexts.

With schemas, roles and multiple contexts in place, the data boundary becomes enforceable at the database level:

*   The **Orders** module's `DbContext` knows only about the orders schema and uses credentials that have no privileges on the **Shipping** schema.
*   The **Shipping** module cannot query `orders.orders` directly because its role lacks the necessary privileges.
*   Cross-module communication must go through the module's public API (or an asynchronous event). This explicit coupling makes dependencies obvious and maintainable.

## Cross-cutting queries

Even in a modular system you occasionally need a screen that spans multiple modules, such as an **Order History** page that shows order and shipping data. Resist the temptation to `JOIN` across schemas. Two approaches can help:

*   **Dedicated read model**. One module owns a view model and subscribes to events from others. This pattern works well when modules might be extracted later.

*   **Database views with privileges**. Since our modules share a database, we can create a read-only view in the public schema that joins the relevant tables. We grant `SELECT` on the view to a special role or module. This view acts like a controlled public API. Consumers query the view but cannot access the underlying tables directly. The trade-off is similar to calling a synchronous API: if you later split the database, the view will have to be replaced with a service call.

For example:

```sql
CREATE VIEW public.order_summary AS
SELECT o.id, o.total, s.status
FROM orders.orders o
JOIN shipping.shipments s ON s.order_id = o.id;

-- grant read access to a reporting role
GRANT SELECT ON public.order_summary TO reporting_role;
```

This approach lets you build dashboards or admin screens without breaking boundaries.

## Conclusion

By giving each module its own schema, role and DbContext, and by controlling cross-module access via views or APIs, you **ensure that boundaries in your modular monolith are enforceable** rather than aspirational. This discipline makes it easier to evolve your system and paves the way for a future microservice extraction.

If you found this article valuable, check out my previous posts on modular monoliths:

*   [What Is a Modular Monolith?](what-is-a-modular-monolith)
*   [Modular Monolith Data Isolation](modular-monolith-data-isolation)
*   [Internal vs Public APIs in Modular Monoliths](internal-vs-public-apis-in-modular-monoliths)

If you want a structured, hands-on approach to building modular systems, from defining boundaries to extracting services, check out my [**Modular Monolith Architecture**](/modular-monolith-architecture) course. Join more than 2,100+ students who have mastered modular monoliths with it. The course walks through these patterns in depth and shows how to apply them in a real codebase.

Thanks for reading.

And stay awesome!

---

Whenever you're ready, there are 4 ways I can help you:

1.  [Pragmatic Clean Architecture:](/pragmatic-clean-architecture) Join 4,200+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [Modular Monolith Architecture:](/modular-monolith-architecture) Join 2,100+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [(NEW) Pragmatic REST APIs:](/pragmatic-rest-apis) Join 1,200+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
4.  [Patreon Community:](https://www.patreon.com/milanjovanovic) Join a community of 5,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Become a Better .NET Software Engineer

Join 70,000+ engineers who are improving their skills every Saturday morning.

Join 70K+ Engineers

Accelerate Your .NET Skills 🚀

![PCA Cover - Pragmatic Clean Architecture Course](/_next/static/media/cover.27333f2f.png?imwidth=384)

Pragmatic Clean Architecture

![MMA Cover - Modular Monolith Architecture Course](/_next/static/media/cover.31e11f05.png?imwidth=384)

Modular Monolith Architecture

![PRA Cover - Pragmatic REST APIs Course](/_next/static/media/cover_1.fc0deb78.png?imwidth=384)

Pragmatic REST APIs

NEW

![Thumbnail for 'Enforcing Isolation in Modular Monoliths' video, with text 'ENFORCING ISOLATION IN MODULAR MONOLITHS' on a blue background with geometric patterns and an 'MJ Tech' logo.](https://img.youtube.com/vi/12345/maxresdefault.jpg)

![Cover image for the "Pragmatic Clean Architecture" course, showing a blue box with the course title and a diagram illustrating Clean Architecture layers (Domain, Application, Presentation, Infrastructure, DB, Endpoints, Queries, External Services), along with a photo of Milan Jovanović.](https://img.youtube.com/vi/12345/maxresdefault.jpg)