```yaml
---
title: "Microservice: Dapr, Dapper, and PostgreSQL (.NET 10) | by Engr. Md. Hasan Monsur | Jul, 2025 | Medium"
source: https://medium.com/@hasanmcse/microservice-dapr-dapper-and-postgresql-net-10-66597c0fa6ad
date_published: 2025-07-25T18:43:32.561Z
date_captured: 2025-08-27T10:54:41.145Z
domain: medium.com
author: Engr. Md. Hasan Monsur
category: architecture
technologies: [Dapr, Dapper, PostgreSQL, .NET 10, Redis, ASP.NET Core, Npgsql, Docker, Swagger, SwaggerGen, CloudEvents]
programming_languages: [C#, SQL, YAML]
tags: [microservices, dapr, dapper, postgresql, dotnet, data-access, state-management, pubsub, web-api, orm]
key_concepts: [microservices-architecture, distributed-systems, service-to-service-communication, state-management, publish-subscribe-pattern, micro-orm, repository-pattern, dependency-injection]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article guides developers in building a high-performance, next-generation microservice application using Dapr, Dapper, and PostgreSQL with .NET 10. It demonstrates how Dapr facilitates service-to-service communication and state management, while Dapper provides lightweight ORM capabilities for efficient database access. The tutorial walks through setting up a POS backend microservice, including Dapr component configuration, PostgreSQL database schema, and C# implementation of domain models, repositories, services, and API controllers. Key aspects covered include Dapr's pub/sub and state store features, Dapper for data operations, and dependency injection. The goal is to equip readers with the foundation for scalable and robust distributed solutions.]
---
```

# Microservice: Dapr, Dapper, and PostgreSQL (.NET 10) | by Engr. Md. Hasan Monsur | Jul, 2025 | Medium

# Microservice: Dapr, Dapper, and PostgreSQL (.NET 10)

This article is for you if you are trying to develop next-generation applications that handle high-performance enterprise solutions. In this article, we use Dapr for building block service-to-service communication and to store session state in microservices. Dapper is used for lightweight ORM database connection, and PostgreSQL is utilized with .NET 10, a powerful enterprise SDK. So, let's start…

![Cover image for the article showing a microservice architecture diagram and the author, Hasan Monsur, a Software Architect and Fintect Innovator.](https://miro.medium.com/v2/resize:fill:64:64/1*9eHl8V2MMmMTCFcSYSIwvA.jpeg)

This article shows how Dapr integrates with a microservice application and manages service-to-service communication. We demonstrate this with a POS application backend as an example.

## Solution Architecture

![Diagram illustrating the Microservice Data Flow. It shows Client Applications interacting with an API Gateway, which then uses Dapr Service Invocation to communicate with various .NET 10 Microservices (Order Service, Inventory Service, Payment Service). Each microservice has a Dapr Sidecar for Pub/Sub Events and State Management, which then interacts with Dapper ORM to perform SQL Queries against a PostgreSQL Cluster.](https://miro.medium.com/v2/resize:fit:700/1*rfKFqCaaUUaJ2ja4--s8Rw.png)

POS Backend Microservice
├── Dapr (for distributed app runtime)
├── PostgreSQL (data store)
|-- Redis (Store PubSub)
├── Dapper (micro-ORM)
└── .NET 10 (runtime)

## Implementation Steps

### 1. Create the .NET 10 Project

```bash
dotnet new webapi -n PosBackend -f net10.0
cd PosBackend
dotnet add package Dapr.AspNetCore
dotnet add package Dapper
dotnet add package Npgsql

dotnet new globaljson --sdk-version 10.0.100-preview.6.25358.103
```

### 2. Configure Dapr Components

Create `components` folder with these files:

`pubsub.yaml`
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: pubsub
spec:
  type: pubsub.redis
  version: v1
  metadata:
  - name: redisHost
    value: "localhost:6379"
```

Install PostgreSQL if the database is not installed:

```bash
docker run -d --name pgdb -e POSTGRES_USER=postgres -e POSTGRES_PASSWORD=password -e POSTGRES_DB=posdb -p 5432:5432 postgres:15
```

`statestore.yaml`
```yaml
apiVersion: dapr.io/v1alpha1
kind: Component
metadata:
  name: statestore
spec:
  type: state.postgresql
  version: v1
  initTimeout: 30s # Increased from default 5s
  metadata:
  - name: connectionString
    value: "host=localhost port=5432 user=hasan password=postgres dbname=posdb sslmode=disable"
```

### 3. Database Setup (PostgreSQL)

SQL Schema (`init.sql`)
```sql
CREATE TABLE products (
    id SERIAL PRIMARY KEY,
    sku VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(100) NOT NULL,
    description TEXT,
    price DECIMAL(10,2) NOT NULL,
    stock_quantity INT NOT NULL DEFAULT 0,
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE orders (
    id SERIAL PRIMARY KEY,
    order_number VARCHAR(20) UNIQUE NOT NULL,
    customer_id INT,
    total_amount DECIMAL(10,2) NOT NULL,
    status VARCHAR(20) DEFAULT 'pending',
    created_at TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE order_items (
    id SERIAL PRIMARY KEY,
    order_id INT REFERENCES orders(id),
    product_id INT REFERENCES products(id),
    quantity INT NOT NULL,
    unit_price DECIMAL(10,2) NOT NULL,
    total_price DECIMAL(10,2) NOT NULL
);
```

### 4. Implement Domain Models

`Models/Product.cs`
```csharp
namespace PosBackend.Models;

public class Product
{
    public int Id { get; set; }
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public DateTime CreatedAt { get; set; }
}
```

`Models/Order.cs`
```csharp
namespace PosBackend.Models;

public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public int? CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; } = "pending";
    public DateTime CreatedAt { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public Product? Product { get; set; }
}
```

### 5. Create Repository Layer with Dapper

`Repositories/IRepository.cs`
```csharp
namespace PosBackend.Repositories;

public interface IRepository<T>
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<int> CreateAsync(T entity);
    Task<bool> UpdateAsync(T entity);
    Task<bool> DeleteAsync(int id);
}
```

`Repositories/ProductRepository.cs`
```csharp
using Dapper;
using PosBackend.Models;
using System.Data;

namespace PosBackend.Repositories
{
    public class ProductRepository : IRepository<Product>
    {
        private readonly IDbConnection _db;

        public ProductRepository(IDbConnection db)
        {
            _db = db;

            // Ensure connection is open
            if (_db.State != ConnectionState.Open)
            {
                _db.Open();
            }
        }

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _db.QueryAsync<Product>("SELECT * FROM products");
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _db.QueryFirstOrDefaultAsync<Product>(
                "SELECT * FROM products WHERE id = @Id", new { Id = id });
        }

        public async Task<int> CreateAsync(Product product)
        {
            var sql = @"INSERT INTO products (sku, name, description, price, stock_quantity)
                    VALUES (@Sku, @Name, @Description, @Price, @StockQuantity)
                    RETURNING id";
            return await _db.ExecuteScalarAsync<int>(sql, product);
        }

        public async Task<bool> UpdateAsync(Product product)
        {
            var affectedRows = await _db.ExecuteAsync(
                @"UPDATE products SET
                sku = @Sku,
                name = @Name,
                description = @Description,
                price = @Price,
                stock_quantity = @StockQuantity
            WHERE id = @Id", product);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var affectedRows = await _db.ExecuteAsync(
                "DELETE FROM products WHERE id = @Id", new { Id = id });
            return affectedRows > 0;
        }
    }
}
```

Now I create Order Repository:

```csharp
using Dapper;
using PosBackend.Models;
using System.Data;

namespace PosBackend.Repositories
{
    public class OrdersRepository : IRepository<Order>
    {
        private readonly IDbConnection _db;

        public OrdersRepository(IDbConnection db)
        {
            _db = db;

            // Ensure connection is open
            if (_db.State != ConnectionState.Open)
            {
                _db.Open();
            }
        }

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            var orders = await _db.QueryAsync<Order>("SELECT * FROM orders");

            // Load order items for each order
            foreach (var order in orders)
            {
                order.Items = (await GetOrderItemsAsync(order.Id)).ToList();
            }

            return orders;
        }

        public async Task<Order?> GetByIdAsync(int id)
        {
            var order = await _db.QueryFirstOrDefaultAsync<Order>(
                "SELECT * FROM orders WHERE id = @Id", new { Id = id });

            if (order != null)
            {
                order.Items = (await GetOrderItemsAsync(order.Id)).ToList();
            }

            return order;
        }

        public async Task<Order?> GetByOrderNumberAsync(string orderNumber)
        {
            var order = await _db.QueryFirstOrDefaultAsync<Order>(
                "SELECT * FROM orders WHERE order_number = @OrderNumber",
                new { OrderNumber = orderNumber });

            if (order != null)
            {
                order.Items = (await GetOrderItemsAsync(order.Id)).ToList();
            }

            return order;
        }

        public async Task<int> CreateAsync(Order order)
        {
            using var transaction = _db.BeginTransaction();

            try
            {
                // Insert order
                var sql = @"INSERT INTO orders
                        (order_number, customer_id, total_amount, status)
                        VALUES (@OrderNumber, @CustomerId, @TotalAmount, @Status)
                        RETURNING id";

                order.Id = await _db.ExecuteScalarAsync<int>(sql, order, transaction);

                // Insert order items
                foreach (var item in order.Items)
                {
                    item.OrderId = order.Id;
                    await CreateOrderItemAsync(item, transaction);
                }

                transaction.Commit();
                return order.Id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateAsync(Order order)
        {
            var sql = @"UPDATE orders SET
                    customer_id = @CustomerId,
                    total_amount = @TotalAmount,
                    status = @Status
                WHERE id = @Id";

            var affectedRows = await _db.ExecuteAsync(sql, order);
            return affectedRows > 0;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            using var transaction = _db.BeginTransaction();

            try
            {
                // Delete order items first
                await _db.ExecuteAsync(
                    "DELETE FROM order_items WHERE order_id = @OrderId",
                    new { OrderId = id },
                    transaction);

                // Then delete the order
                var affectedRows = await _db.ExecuteAsync(
                    "DELETE FROM orders WHERE id = @Id",
                    new { Id = id },
                    transaction);

                transaction.Commit();
                return affectedRows > 0;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(int orderId, string status)
        {
            var affectedRows = await _db.ExecuteAsync(
                "UPDATE orders SET status = @Status WHERE id = @OrderId",
                new { OrderId = orderId, Status = status });

            return affectedRows > 0;
        }

        private async Task<IEnumerable<OrderItem>> GetOrderItemsAsync(int orderId)
        {
            var sql = @"SELECT oi.*, p.*
                    FROM order_items oi
                    LEFT JOIN products p ON oi.product_id = p.id
                    WHERE oi.order_id = @OrderId";

            return await _db.QueryAsync<OrderItem, Product, OrderItem>(sql,
                (orderItem, product) =>
                {
                    orderItem.Product = product;
                    return orderItem;
                },
                new { OrderId = orderId },
                splitOn: "id");
        }

        private async Task CreateOrderItemAsync(OrderItem item, IDbTransaction transaction)
        {
            var sql = @"INSERT INTO order_items
                    (order_id, product_id, quantity, unit_price, total_price)
                    VALUES (@OrderId, @ProductId, @Quantity, @UnitPrice, @TotalPrice)";

            await _db.ExecuteAsync(sql, item, transaction);
        }
    }
}
```

### 6. Implement Services with Dapr State Management

`Services/PosService.cs`
```csharp
using Dapr.Client;
using PosBackend.Models;
using PosBackend.Repositories;

namespace PosBackend.Services
{
    public class PosService
    {
        private readonly OrdersRepository _ordersRepo;
        private readonly ProductRepository _productRepo;
        private readonly DaprClient _daprClient;
        private const string StoreName = "statestore";

        public PosService(OrdersRepository ordersRepo,ProductRepository productRepo, DaprClient daprClient)
        {
            _ordersRepo = ordersRepo;
            _productRepo = productRepo;
            _daprClient = daprClient;
        }

        // Product operations
        public async Task<IEnumerable<Product>> GetProductsAsync() =>
            await _productRepo.GetAllAsync();

        public async Task<Product?> GetProductAsync(int id) =>
            await _productRepo.GetByIdAsync(id);

        public async Task<Product> CreateProductAsync(Product product)
        {
            product.Id = await _productRepo.CreateAsync(product);
            return product;
        }

        // Order operations with Dapr state management
        public async Task<Order?> GetOrderAsync(string orderId)
        {
            return await _daprClient.GetStateAsync<Order>(StoreName, orderId);
        }

        public async Task<Order> CreateOrderAsync(Order order)
        {
            // Generate order number
            order.OrderNumber = $"ORD-{DateTime.UtcNow:yyyyMMdd-HHmmss}-{Guid.NewGuid().ToString()[..4]}";
            order.CreatedAt = DateTime.UtcNow;

            // Calculate total
            order.TotalAmount = order.Items.Sum(i => i.TotalPrice);

            // Save to Dapr state store
            Console.WriteLine($"Save to Dapr state store {order.OrderNumber}");

            await _daprClient.SaveStateAsync(StoreName, order.OrderNumber, order);

            // Publish order created event
            Console.WriteLine($"Publish order created event {order.OrderNumber}");
            await _daprClient.PublishEventAsync("pubsub", "orderCreated", order);

            return order;
        }

        public async Task<bool> UpdateOrderStatusAsync(string orderNumber, string status)
        {
            var order = await GetOrderAsync(orderNumber);
            if (order == null) return false;

            order.Status = status;
            await _daprClient.SaveStateAsync(StoreName, orderNumber, order);

            // Publish status updated event
            await _daprClient.PublishEventAsync("pubsub", "orderStatusUpdated",
                new { OrderNumber = orderNumber, Status = status });

            return true;
        }

        public async Task<bool> SaveOrderAsync(Order order)
        {
            bool opstat = false;

            try
            {
                int result = await _ordersRepo.CreateAsync(order);

                if (result > 0)
                {
                    opstat = true;
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving order: {ex.Message}");
                // Handle logging or rethrow as needed
            }


            return opstat;
        }

        public async Task<bool> UpdateOrderAsync(Order order)
        {
            bool opstat = false;

            await _ordersRepo.UpdateAsync(order);

            return opstat;
        }
    }
}
```

### 7. Create API Controllers

`Controllers/ProductsController.cs`
```csharp
using Microsoft.AspNetCore.Mvc;
using PosBackend.Models;
using PosBackend.Services;

namespace PosBackend.Controllers;

[ApiController]
[Route("[controller]")]
public class ProductsController : ControllerBase
{
    private readonly PosService _posService;

    public ProductsController(PosService posService)
    {
        _posService = posService;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
    {
        var products = await _posService.GetProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _posService.GetProductAsync(id);
        return product == null ? NotFound() : Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        var createdProduct = await _posService.CreateProductAsync(product);
        return CreatedAtAction(nameof(GetProduct), new { id = createdProduct.Id }, createdProduct);
    }
}
```

`Controllers/OrdersController.cs`
```csharp
using Dapr;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PosBackend.Models;
using PosBackend.Repositories;
using PosBackend.Services;

namespace PosBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OrdersController : ControllerBase
    {
        private readonly PosService _posService;

        public OrdersController(PosService posService)
        {
            _posService = posService;
        }

        [HttpGet("{orderNumber}")]
        public async Task<ActionResult<Order>> GetOrder(string orderNumber)
        {
            var order = await _posService.GetOrderAsync(orderNumber);
            return order == null ? NotFound() : Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<Order>> CreateOrder(Order order)
        {
            var createdOrder = await _posService.CreateOrderAsync(order);

            return CreatedAtAction(nameof(GetOrder), new { orderNumber = createdOrder.OrderNumber }, createdOrder);
        }

        [HttpPatch("{orderNumber}/status")]
        public async Task<IActionResult> UpdateOrderStatus(string orderNumber, [FromBody] string status)
        {
            var success = await _posService.UpdateOrderStatusAsync(orderNumber, status);
            return success ? NoContent() : NotFound();
        }


        [Topic("pubsub", "orderCreated")]
        [HttpPost("orderCreated")]
        public async Task<IActionResult> HandleOrderCreateEvent([FromBody] Order order)
        {
            Console.WriteLine($"HandleOrderCreateEvent Handler Order Create for {order.Id}");

            if (order == null || order.Id<=0)
            {
                //logger.LogWarning("Invalid order received");
                return BadRequest("Invalid order data");
            }

            // 3. Save to database
              await _posService.SaveOrderAsync(order);

            return Ok();
        }

        [Topic("pubsub", "orderStatusUpdated")]
        [HttpPost("orderStatusUpdated")]
        public async Task<IActionResult> HandleOrderUpdateEvent([FromBody] Order order)
        {
            Console.WriteLine($"HandleOrderCreateEvent Handler Order Create for {order.Id}");

            if (order == null || order.Id > 0)
            {
                //logger.LogWarning("Invalid order received");
                return BadRequest("Invalid order data");
            }

            // 3. Save to database
            await _posService.UpdateOrderAsync(order);


            return Ok();
        }
    }
}
```

### 8. Configure Dependency Injection

`Program.cs`
```csharp
using Npgsql;
using PosBackend.Repositories;
using PosBackend.Services;
using System.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddControllers().AddDapr();
builder.Services.AddDaprClient(builder => builder
    .UseHttpEndpoint($"http://localhost:3500")
    .UseGrpcEndpoint($"http://localhost:50001"));

// Configure PostgreSQL connection
var connectionString = builder.Configuration.GetConnectionString("PostgreSQL");
builder.Services.AddScoped<IDbConnection>(_ =>
{
    var connection = new NpgsqlConnection(connectionString);
    connection.Open(); // Explicitly open the connection
    return connection;
});

// Register repositories
builder.Services.AddScoped<OrdersRepository>();
builder.Services.AddScoped<ProductRepository>();
// Register services
builder.Services.AddScoped<PosService>();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseSwagger();
app.UseSwaggerUI();


app.UseAuthorization();
app.MapControllers();

// Enable Dapr pub/sub
app.UseCloudEvents();
app.MapSubscribeHandler();

app.Run();
```

### 9. Add Configuration

`appsettings.json`
```json
{
  "ConnectionStrings": {
    "PostgreSQL": "User ID=postgres;Password=password;Host=localhost;Port=5432;Database=posdb;"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

## Deployment and Running

1.  Start PostgreSQL and create the database
2.  Run the Dapr sidecar with the application:

```bash
dapr run --app-id PosBackend --app-port 5194 --components-path ./components --enable-profiling=false -- dotnet run
```

**Download Project Source Code —**[**Microservice-Dapr-Dapper-and-PostgreSQL-.NET-10-**](https://github.com/hasanmonsur/Microservice-Dapr-Dapper-and-PostgreSQL-.NET-10-)

## Conclusion

Thanks for reading my article. You are now ready to build next-generation, high-performance enterprise solutions, and this article serves as your starting point. We demonstrated how **Dapr** simplifies microservice communication and state management, how **Dapper** offers fast and lightweight database access, and how **PostgreSQL** with **.NET 10** empowers your backend with modern, scalable tools. Whether you’re architecting for speed, scalability, or simplicity, this guide sets the foundation. Let’s build smarter microservices, together.

I offered you other Medium articles: [Visit My Profile](/@hasanmcse)

Also, My GitHub : [Md Hasan Monsur](https://github.com/hasanmonsur)

Connect with me at LinkedIn : [Md Hasan Monsur](https://www.linkedin.com/in/hasan-monsur/)