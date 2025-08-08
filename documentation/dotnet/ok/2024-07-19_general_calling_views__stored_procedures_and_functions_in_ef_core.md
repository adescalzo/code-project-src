```yaml
---
title: Calling Views, Stored Procedures and Functions in EF Core
source: https://antondevtips.com/blog/calling-views-stored-procedures-and-functions-in-ef-core
date_published: 2024-07-19T11:00:34.020Z
date_captured: 2025-08-06T17:24:55.511Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [EF Core, PostgreSQL, ASP.NET Core, .NET, Entity Framework Extensions]
programming_languages: [C#, SQL]
tags: [ef-core, database, sql, views, stored-procedures, functions, data-access, dotnet, web-api, orm]
key_concepts: [database-views, stored-procedures, database-functions, keyless-entities, linq-to-entities, minimal-api, scalar-functions, table-valued-functions]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide on integrating database views, stored procedures, and functions with EF Core. It demonstrates how to map database views to keyless entities and query them using LINQ. The post also covers executing stored procedures directly via `ExecuteSqlAsync` and `ExecuteSqlRawAsync` methods. Furthermore, it details the process of mapping and invoking both table-valued and scalar database functions within EF Core, showcasing their usage in C# code and LINQ queries. Practical code examples for each scenario are provided, utilizing a PostgreSQL database and an ASP.NET Core minimal API.]
---
```

# Calling Views, Stored Procedures and Functions in EF Core

![Article banner for 'Calling Views, Stored Procedures and Functions in EF Core', featuring a white code icon and 'dev tips' text on a dark background with purple abstract shapes.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fefcore%2Fcover_ef_views_functions_procedures.png&w=3840&q=100)

# Calling Views, Stored Procedures and Functions in EF Core

Jul 19, 2024

[Download source code](/source-code/calling-views-stored-procedures-and-functions-in-ef-core)

5 min read

### Newsletter Sponsors

[EF Core is too slow?](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website) Discover how you can easily insert 14x faster (reducing saving time by 94%).  
Boost your performance with our method integrated within EF Core: Bulk Insert, update, delete, and merge.  
Join 5,000+ satisfied customers who have trusted our library since 2014.

[Learn more](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website)

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

While EF Core provides a robust API for interacting with the database, there are scenarios where you need to call existing database views, stored procedures, or functions. In this blog post, we will explore how to call views, stored procedures, and functions using EF Core.

[](#how-to-call-database-views-in-ef-core)

## How To Call Database Views in EF Core

A [database view](https://antondevtips.com/blog/getting-started-with-database-views-in-sql) is a virtual table that contains data from one or multiple database tables. Unlike a physical table, a **view** does not store data itself. It contains a set of predefined SQL queries to fetch data from the database.

Let's explore an application that stores Customers, Products, Orders, and OrderDetails in the **Postgres** database. Here is what our database looks like:

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

And we have a EF Core DbContext that maps to this database:

```csharp
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<Customer> Customers { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderDetail> OrderDetails { get; set; }
}
```

Our database has a view that returns total sales per each Customer:

```sql
CREATE VIEW IF NOT EXISTS total_sales_per_customer AS
SELECT customer.name, SUM(order_details.quantity * order_details.price) AS total_sales
FROM customers customer
         JOIN orders "order" ON customer.id = "order".customer_id
         JOIN order_details order_details ON "order".id = order_details.order_id
GROUP BY customer.name
ORDER BY total_sales desc;
```

In this **Postgres** database, you can call the view to get the results:

```sql
select * from total_sales_per_customer;
```

To call a view in EF Core, you need to map it to a **model class** and add to the DbContext as **keyless** entity:

```csharp
public class TotalSalesPerCustomer
{
    public string Name { get; set; }
    public decimal TotalSales { get; set; }
}

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public DbSet<TotalSalesPerCustomer> TotalSalesPerCustomers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TotalSalesPerCustomer>()
            .HasNoKey()
            .ToView("total_sales_per_customer");
    }
}
```

We map entity as `HasNoKey` because we are calling the database view that doesn't have a primary key. In the `ToView` method you need to specify the name of the database view.

And here is how you can call the view in the minimal API endpoint:

```csharp
app.MapGet("/api/total-sales", async (ApplicationDbContext dbContext) =>
{
    var totalSales = await dbContext.TotalSalesPerCustomers
        .ToListAsync();

    return Results.Ok(totalSales);
});
```

As a result, you will receive something like this:

```json
[
    {
        "name": "Virginia Champlin",
        "totalSales": 153486.75
    },
    {
        "name": "Mary Kessler",
        "totalSales": 132898.65
    },
    {
        "name": "Louis Stark",
        "totalSales": 112785.54
    },
    {
        "name": "Kirk Renner",
        "totalSales": 104335.26
    },
    {
        "name": "Jan Keebler",
        "totalSales": 78566.76
    }
]
```

[](#how-to-call-stored-procedures-in-ef-core)

## How To Call Stored Procedures in EF Core

A **stored procedure** in SQL is a group of SQL queries that can be saved and reused multiple times. Unlike a database view, stored procedure executes SQL statements that don't return data.

Let's assume that our database has the following stored procedure to update price in the `order_details` price:

```sql
CREATE OR REPLACE PROCEDURE update_order_details_price(
    orderDetailId INT,
    newPrice DECIMAL
)
LANGUAGE plpgsql
AS $$
BEGIN
    UPDATE order_details
    SET price = newPrice
    WHERE id = orderDetailId;
    COMMIT;
END;
$$;
```

EF Core allows you to call stored procedures directly.

```csharp
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    public async Task UpdateOrderDetailPriceAsync(int orderDetailId, decimal newPrice)
    {
        await Database.ExecuteSqlAsync($"CALL update_order_details_price({orderDetailId}, {newPrice})");
    }
}
```

Despite using interpolated string, `ExecuteSqlAsync` method is safe as the string is actually a `FormattableString`.

If you don't want to use string interpolation, you can use `ExecuteSqlRawAsync` method and pass arguments:

```csharp
public async Task UpdateOrderDetailPriceAsync(int orderDetailId, decimal newPrice)
{
    await Database.ExecuteSqlRawAsync("CALL update_order_details_price({0}, {1})", orderDetailId, newPrice);
}
```

Now, let's define a POST minimal API endpoint to update the price:

```csharp
public record UpdateRequest(int OrderDetailId, decimal NewPrice);

app.MapPost("/update-order-detail-price",
    async ([FromBody] UpdateRequest request, ApplicationDbContext context) =>
{
    await context.UpdateOrderDetailPriceAsync(request.OrderDetailId, request.NewPrice);
    return Results.Ok();
});
```

[](#how-to-call-database-functions-in-ef-core)

## How To Call Database Functions in EF Core

A **function** in SQL is a group of SQL queries that can be saved and reused multiple times. It is similar to stored procedure but return data.

A function can be one of the following types:

*   table-valued function
*   scalar function.

[](#how-to-call-tablevalued-function-in-ef-core)

### How To Call Table-Valued Function in EF Core

**Table-Valued** function is a function that returns data of a table type.

Let's assume that our database has the following function that returns total sales of the customer:

```sql
CREATE OR REPLACE FUNCTION get_customer_total_sales(customerId INT)
RETURNS TABLE (
    name VARCHAR,
    total_sales DECIMAL
) AS $$
BEGIN
    RETURN QUERY
    SELECT c.name, SUM(od.quantity * od.price) AS total_sales
    FROM customers c
    JOIN orders o ON c.id = o.customer_id
    JOIN order_details od ON o.id = od.order_id
    WHERE c.id = customerId
    GROUP BY c.name;
END;
$$ LANGUAGE plpgsql;
```

You can map this function by using the `HasDbFunction` method in the DbContext:

```csharp
public class CustomerTotalSales
{
    public string Name { get; set; }
    public decimal TotalSales { get; set; }
}

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDbFunction(() => GetCustomerTotalSales(default))
            .HasName("get_customer_total_sales");
    }
    
    public IQueryable<CustomerTotalSales> GetCustomerTotalSales(int customerId) =>
        FromExpression(() => GetCustomerTotalSales(customerId));
}
```

Here we create a public function, that returns a SQL expression, by wrapping `GetCustomerTotalSales` method using `FromExpression` method from the base DbContext class. Then we map it to the database function "get\_customer\_total\_sales" using the `HasDbFunction` method.

Now we can call this function inside a minimal API endpoint:

```csharp
app.MapGet("/get-customer-total-sales/{customerId}",
    async (int customerId, ApplicationDbContext context) =>
{
    var customerTotalSales = await context.GetCustomerTotalSales(customerId)
        .ToListAsync();
        
    return Results.Ok(customerTotalSales);
});
```

When calling this endpoint, you'll get the following response:

```json
[
  {
    "name": "Kirk Renner",
    "totalSales": 34778.42
  }
]
```

[](#how-to-call-scalar-function-in-ef-core)

### How To Call Scalar Function in EF Core

**Scalar** function is a function that returns a single value.

Let's assume that our database has the following function that returns total order detail price:

```sql
CREATE OR REPLACE FUNCTION get_order_detail_price(productId INT)
RETURNS DECIMAL AS $$
BEGIN
    RETURN (SELECT SUM(price * quantity) FROM order_details WHERE product_id = productId);
END;
$$ LANGUAGE plpgsql;
```

This function returns a sum of all order details of the particular product.

We can map it in EF Core using the `HasDbFunction` method:

```csharp
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDbFunction(
            typeof(ApplicationDbContext).GetMethod(nameof(GetOrderDetailPrice), 
            new[] { typeof(int) })!
        ).HasName("get_order_detail_price");
    }
    
    public static decimal GetOrderDetailPrice(int productId)
        => throw new NotImplementedException();
}
```

Just never mind `throw new NotImplementedException();` as this method won't be called directly and is proxied by EF Core to call the actual database function.

Now you can use this function inside a LINQ query to get products that have total order details more than 20 000:

```csharp
app.MapGet("/get-expensive-products-by-orders",
    async (ApplicationDbContext context) =>
{
    var expensiveProducts = await context.Products
        .Where(x => ApplicationDbContext.GetOrderDetailPrice(x.Id) > 20_000m)
        .ToListAsync();

    return Results.Ok(expensiveProducts);
});
```

When calling this endpoint, you'll get the following response:

```json
[
  {
    "id": 7,
    "name": "Fantastic Soft Pizza",
    "price": 60.46,
    "orderDetails": []
  },
  {
    "id": 9,
    "name": "Handmade Wooden Shoes",
    "price": 893.77,
    "orderDetails": []
  },
  {
    "id": 10,
    "name": "Awesome Soft Bike",
    "price": 256.69,
    "orderDetails": []
  }
]
```

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/calling-views-stored-procedures-and-functions-in-ef-core)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fcalling-views-stored-procedures-and-functions-in-ef-core&title=Calling%20Views%2C%20Stored%20Procedures%20and%20Functions%20in%20EF%20Core)[X](https://twitter.com/intent/tweet?text=Calling%20Views%2C%20Stored%20Procedures%20and%20Functions%20in%20EF%20Core&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fcalling-views-stored-procedures-and-functions-in-ef-core)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fcalling-views-stored-procedures-and-functions-in-ef-core)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.