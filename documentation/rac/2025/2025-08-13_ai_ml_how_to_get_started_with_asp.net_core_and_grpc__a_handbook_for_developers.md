```yaml
---
title: "How to Get Started with ASP.NET Core and gRPC: A Handbook for Developers"
source: https://www.freecodecamp.org/news/get-started-with-aspnet-core-and-grpc-handbook/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2115
date_published: 2025-08-13T14:37:40.211Z
date_captured: 2025-09-10T18:41:08.735Z
domain: www.freecodecamp.org
author: Unknown
category: ai_ml
technologies: [ASP.NET Core, gRPC, .NET, Protocol Buffers, HTTP/2, Entity Framework Core, SQLite, Visual Studio Code, Postman, GitHub, .NET CLI]
programming_languages: [C#, SQL, Protocol Buffers]
tags: [grpc, dotnet, aspnet-core, microservices, api, database, crud, protocol-buffers, data-access, tutorial]
key_concepts: [remote-procedure-call, protocol-buffers, microservices-architecture, crud-operations, dependency-injection, orm, pagination, http2]
code_examples: false
difficulty_level: intermediate
summary: |
  This handbook provides a comprehensive guide for developers to get started with ASP.NET Core and gRPC, focusing on building scalable and high-performance distributed applications. It covers setting up a gRPC service, defining type-safe contracts using Protocol Buffers, and implementing full CRUD operations with SQLite and Entity Framework Core. The article also delves into the advantages of gRPC over traditional REST APIs, explains key concepts like HTTP/2 and code generation, and demonstrates practical testing using Postman. Readers will gain hands-on experience in developing robust gRPC services from scratch.
---
```

# How to Get Started with ASP.NET Core and gRPC: A Handbook for Developers

August 13, 2025 / [#.NET](/news/tag/net/)

# How to Get Started with ASP.NET Core and gRPC: A Handbook for Developers

![Isaiah Clifford Opoku](https://cdn.hashnode.com/res/hashnode/image/upload/v1732626868119/2d7d4d9b-4740-4b10-8c77-971d93b3cad0.jpeg?w=500&h=500&fit=crop&crop=entropy&auto=compress,format&format=webp)

[Isaiah Clifford Opoku](/news/author/Clifftech/)

![Thumbnail for the article, showing "Get Started with ASP.NET Core and gRPC" text, .NET logo, gRPC logo, and the author.](https://cdn.hashnode.com/res/hashnode/image/upload/v1755043329753/f5ff4a61-79b7-44f0-9871-9dfef9f8d08a.png)

In today's distributed computing landscape, efficient service-to-service communication is crucial for building scalable, high-performance applications. gRPC (Google Remote Procedure Call) has emerged as one of the most powerful frameworks for creating robust, type-safe APIs that can handle thousands of requests per second with minimal latency.

gRPC is a modern, open-source RPC framework that leverages HTTP/2, Protocol Buffers, and advanced streaming capabilities to deliver exceptional performance. Unlike traditional REST APIs, gRPC offers strongly-typed contracts, automatic code generation, and built-in support for multiple programming languages. This makes it an ideal choice for microservices architectures and cross-platform development.

In this handbook, I’ll take you on a journey from absolute beginner to building production-ready gRPC services with [ASP.NET](http://ASP.NET) Core. Whether you're migrating from REST APIs or starting fresh with gRPC, this guide will provide you with practical, hands-on experience and real-world examples.

**What you'll learn:**

*   How to set up your first gRPC service in .NET
*   How to define service contracts with Protocol Buffers
*   How to implement unary, server streaming, and client streaming operations
*   How to build CRUD (Create, Read, Update, Delete) operations

Let's dive in and discover how gRPC can revolutionize your API development experience!

You can find all the code in this [**GitHub Repository**](https://github.com/Clifftech123/IsaiahCliffordOpokuBlog)**.**

### Table of Contents

1.  [gRPC Overview and How It Works with .NET](#heading-grpc-overview-and-how-it-works-with-net)
2.  [How to Set Up gRPC with .NET](#heading-how-to-set-up-grpc-with-net)
3.  [How to Create the Product Model](#heading-how-to-create-the-product-model)
4.  [How to Set Up the SQLite Database](#heading-how-to-set-up-the-sqlite-database)
5.  [How to Create Product Protocol Buffers](#heading-how-to-create-product-protocol-buffers)
6.  [How to Implement CRUD Operations Services with gRPC](#heading-how-to-implement-crud-operations-services-with-grpc)
7.  [How to Implement gRPC CRUD Database Operations With SQLite](#heading-how-to-implement-grpc-crud-database-operations-with-sqlite)
8.  [How to Test gRPC Services with Postman](#heading-how-to-test-grpc-services-with-postman)
9.  [How to Test Product Creation](#heading-how-to-test-product-creation)
10. [How to Test All Product Operations](#heading-how-to-test-all-product-operations)
11. [Conclusion](#heading-conclusion)

### Perquisites

Before we start, make sure you have the following installed:

*   [.NET SDK](https://dotnet.microsoft.com/download)
*   [Visual Studio Code](https://code.visualstudio.com/download)
*   [Postman](https://www.postman.com/downloads/)

## gRPC Overview and How It Works with .NET

gRPC is a high-performance, cross-platform framework that works seamlessly with many technologies, including .NET Core.

### Why choose gRPC with .NET?

There are many reasons why this is a good combination. First, of all, this combo is up to 8x faster than using REST APIs with JSON. Its strongly-typed contracts also help prevent runtime errors.

It also has built-in support for client, server, and bidirectional streaming, as well as seamless integration across different languages and platforms. Finally, it leverages HTTP/2 for multiplexing and header compression – so as you can see, these two tools are a super effective pair.

To understand in more detail why gRPC is so valuable, let's explore a common real-world scenario.

### The Challenge: Microservices Communication

Imagine you're building a large e-commerce application. For better maintainability and scalability, you decide to split your monolithic application into smaller, focused services:

*   **Product Service** – Handles product catalog, inventory, and product management
*   **Authentication Service** – Manages user authentication, authorization, and user profiles

These services need to communicate with each other frequently. For example, before a user can add a product to their cart, the Product Service must verify with the Authentication Service that the user is logged in and has the proper permissions.

### Traditional Approach: HTTP REST APIs

Traditionally, in .NET applications, we solve this inter-service communication using `HttpClient` to make REST API calls between services. While this works, it comes with several challenges:

*   Network failures: API calls can fail unexpectedly, even when everything appears correct
*   Performance bottlenecks: JSON serialization/deserialization adds overhead
*   Slow response times: HTTP/1.1 limitations affect performance under high load
*   Type safety: No compile-time contract validation between services
*   Verbose payloads: JSON can be bulky compared to binary formats

### The gRPC Solution

This is where gRPC shines. It addresses these challenges by providing some really helpful features in addition to the ones we’ve already discussed above like protocol buffers, code generation for client and server, and more.

### When to Use gRPC in .NET

gRPC is particularly beneficial in certain scenarios, but it’s not a great choice in others. Here are some example use cases, as well as some to avoid:

**✅ Perfect for:**

*   **Microservices architecture**: High-frequency service-to-service communication
*   **Real-time applications**: Chat applications, live updates, gaming
*   **High-performance APIs**: When speed and efficiency are critical
*   **Polyglot environments**: Services written in different programming languages
*   **Internal APIs**: Backend services that don't need browser compatibility

**❌ Consider Alternatives When:**

*   **Browser-based applications**: Limited browser support (use gRPC-Web instead)
*   **Public APIs**: REST might be more familiar to external developers
*   **Simple CRUD operations**: Where REST's simplicity is sufficient
*   **Legacy system integration**: When existing systems only support HTTP/1.1

### gRPC vs REST: A Quick Comparison

Here’s a quick side-by-side comparison of their main features:

| Feature             | gRPC                       | REST          |
| :------------------ | :------------------------- | :------------ |
| Protocol            | HTTP/2                     | HTTP/1.1      |
| Data Format         | Protocol Buffers (Binary)  | JSON (Text)   |
| Performance         | High                       | Moderate      |
| Browser Support     | Limited (needs gRPC-Web)   | Full          |
| Streaming           | Built-in                   | Limited       |
| Code Generation     | Automatic                  | Manual        |

In this handbook, we'll build a complete Product Management system using gRPC with .NET, demonstrating how to implement efficient service-to-service communication with full CRUD operations.

## How to Set Up gRPC with .NET

In this tutorial, we'll use Visual Studio Code to build our complete gRPC application. Let's start by creating a new gRPC project using the .NET CLI.

### Creating Your First gRPC Project

Start by opening your terminal (you can use VS Code's integrated terminal or your system terminal) and navigate to your desired directory where you want to create the project.

Run the following command to create a new gRPC project:

```bash
dotnet new grpc -o ProductGrpc
```

**What this command does:**

*   `dotnet new grpc` creates a new project using the gRPC template
*   `-o ProductGrpc` specifies the output directory name for our project

Next, navigate into the project directory:

```bash
cd ProductGrpc
```

Then open the project in Visual Studio Code:

```bash
code .
```

### Understanding the Project Structure

After running the command, you should see output similar to the following in your terminal, confirming that the project was created successfully:

![Screenshot of Visual Studio Code showing the initial project structure of a ProductGrpc project, including Protos, Services, Program.cs, and ProductGrpc.csproj.](https://cdn.hashnode.com/res/hashnode/image/upload/v1753873861602/6d135358-2065-40eb-9fe9-9a58bd9dc2eb.png)

Let's explore what the .NET gRPC template has generated for us:

```makefile
ProductGrpc/
├── Protos/
│   └── greet.proto          # Protocol Buffer definition file
├── Services/
│   └── GreeterService.cs    # Sample gRPC service implementation
├── Program.cs               # Application entry point
├── ProductGrpc.csproj       # Project file
└── appsettings.json         # Configuration file
```

Key files:

*   `Protos/greet.proto`: Defines the service contract using Protocol Buffers
*   `Services/GreeterService.cs`: Contains the actual service implementation
*   `Program.cs`: Configures and starts the gRPC server
*   `ProductGrpc.csproj`: Contains project dependencies and build settings

### Verifying the Setup

Let's make sure everything is working correctly by running the default application:

```yaml
dotnet run
```

You should see output indicating that the gRPC server is running:

```json
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7042
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
```

**🎉 Congratulations!** You've successfully created your first gRPC application using the .NET CLI. The server is now running and ready to accept gRPC requests.

Let's move on to the next section, where we'll start building our Product Management system.

## How to Create the Product Model

Now that we have our gRPC project set up, let's create our Product model. In .NET applications, models represent the data structure and business entities that our application will work with. Think of models as blueprints that define what properties our data objects should have.

### Understanding Models in gRPC Applications

Models serve several important purposes:

*   **Data structure**: They define the shape and properties of our business entities.
*   **Type safety**: They ensure compile-time validation of our data.
*   **Business logic**: They represent real-world objects in our application.
*   **Database mapping**: They serve as entities for database operations.

### Creating the Models Folder

Let’s organize our code by creating a dedicated folder for our models called `Models` in your project root directory.

Inside the Models folder, create a new file called `Product.cs`.

Your project structure should now look like this:

```yaml
ProductGrpc/
├── Models/
│   └── Product.cs           # Our new Product model
├── Protos/
├── Services/
└── ...
```

### Implementing the Product Model

Add the following code to your `Product.cs` file:

```csharp
// Models/Product.cs
using System.ComponentModel.DataAnnotations;

namespace ProductGrpc.Models
{
    public class Product
    {

        public Guid Id { get; set; }
        public required string Name { get; set; }
        public required string Description { get; set; }
        public decimal Price { get; set; }

        public DateTime Created { get; set; } = DateTime.UtcNow;

        public DateTime Updated { get; set; } = DateTime.UtcNow;
        public string? Tags { get; set; }
    }
}
```

Modern C# features:

*   `required` keyword: Ensures properties must be initialized when creating an object
*   `string?`: Nullable reference type for optional properties
*   Default values: `Created` and `Updated` automatically set to the current UTC

### Why Use Guid for ID?

We're using `Guid` instead of `int` for our primary key for a few reasons:

*   **Uniqueness**: Guaranteed to be unique across different systems
*   **Security**: Harder to guess than sequential integers
*   **Distributed systems**: No need for centralized ID generation
*   **Scalability**: Perfect for microservices architecture

### Namespace Considerations

**Important Note:** If you changed your project name when creating it, make sure your namespace matches your project name. For example:

*   If your project is named `MyProductService`, use `namespace MyProductService.Models`
*   If your project is named `ProductGrpc`, use `namespace ProductGrpc.Models`

🎉 **Excellent work!** You've successfully created your first business model that will serve as the foundation for our entire gRPC application.

### Next Steps

Now that we have our Product model ready, let's move on to setting up SQLite as our database and configuring Entity Framework Core to handle our data persistence. This will allow us to store and retrieve our Product data efficiently.

## How to Set Up the SQLite Database

To persist our product data, we need a database that can handle our CRUD (Create, Read, Update, Delete) operations efficiently. We'll use **SQLite** for this tutorial because it's lightweight, requires no separate server installation, and works perfectly for developing small-to-medium applications.

### Installing the Required Packages

Before we create our database context, we need to install the necessary Entity Framework Core packages. Open your terminal and make sure you're in the root directory of your project, then run these commands:

```powershell
dotnet add package Microsoft.EntityFrameworkCore.Design
```
```powershell
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

What these packages do:

*   **Microsoft.EntityFrameworkCore.Design** provides design-time tools for EF Core (migrations, scaffolding)
*   **Microsoft.EntityFrameworkCore.SQLite** is a SQLite database provider for Entity Framework Core

You should see output confirming the packages were added successfully:

```bash
info : PackageReference for 'Microsoft.EntityFrameworkCore.Design' version 'x.x.x' added to file 'ProductGrpc.csproj'.
info : PackageReference for 'Microsoft.EntityFrameworkCore.Sqlite' version 'x.x.x' added to file 'ProductGrpc.csproj'.
```

### Creating the Database Context

Now let's create our database context, which acts as a bridge between our .NET objects and the database.

First, create a new folder called `Data` In your project root. Inside the Data folder, create a file called `AppDbContext.cs`.

Your project structure should now look like this:

```yaml
ProductGrpc/
├── Data/
│   └── AppDbContext.cs      # Our new database context
├── Models/
│   └── Product.cs
├── Protos/
├── Services/
└── ...
```

Add the following code to your `AppDbContext.cs` file:

```cs
// Data/AppDbContext.cs
using Microsoft.EntityFrameworkCore;
using ProductGrpc.Models;

namespace ProductGrpc.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Product> Products { get; set; }
    }
}
```

Let’s understand the key components of DbContext:

*   **Constructor**: Accepts `DbContextOptions` for configuration (connection string, provider, and so on)
*   **DbSet Products**: Represents the Products table in our database

### Registering the Database Context

Now we need to register our `AppDbContext` With the dependency injection container so our application can use it.

Open your `Program.cs` file and add the database configuration:

```cs
// Program.cs

using ProductGrpc.Data;
using ProductGrpc.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt=> 
    opt.UseSqlite("Data Source=ProductGrpc.db "));
// Add services to the container.
builder.Services.AddGrpc();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();


app.Run();
```

`Data Source=ProductGrpc.db` creates a SQLite database file named `ProductGrpc.db` in your project directory.

### Creating and Running Migrations

Now we need to create a migration to generate the database schema based on our Product model.

Start by creating the initial migration:

```powershell
dotnet ef migrations add InitialCreate
```

This command will:

*   Analyze your models and DbContext
*   Generate migration files in a `Migrations` folder
*   Create SQL commands needed to build your database schema

You should see output like this:

```powershell
Build succeeded.
Done. To undo this action, use 'dotnet ef migrations remove'
```

Apply the migration to create the database:

```powershell
dotnet ef database update
```

This command will:

*   Execute the migration SQL commands
*   Create the `ProductGrpc.db` file in your project directory
*   Set up the Products table with all the correct columns

You should see output confirming the database was created:

```powershell
Build succeeded.
Applying migration '20240101000000_InitialCreate'.
Done.
```

### Verifying the Setup

After running the migration, you should see:

1.  A new `Migrations` folder in your project with migration files
2.  A `ProductGrpc.db` file in your project root (this is your SQLite database)
3.  No errors in the terminal output

Your project structure should now look like this:

```yaml
ProductGrpc/
├── Data/
│   └── AppDbContext.cs
├── Migrations/
│   ├── 20240101000000_InitialCreate.cs
│   └── AppDbContextModelSnapshot.cs
├── Models/
│   └── Product.cs
├── ProductGrpc.db            # Your SQLite database file
└── ...
```

Congratulations! You've successfully installed Entity Framework Core packages, created a database context, registered the context with dependency injection, generated and applied your first migration, and created a working SQLite database. Whew!

### What's Next?

Now that our database is set up and ready, we can move on to creating our Protocol Buffer definitions (`.proto` files) and implementing our gRPC services for CRUD operations.

## How to Create Product Protocol Buffers

Protocol Buffers (protobuf) are the heart of gRPC communication. They define the structure of your data and services in a language-neutral way, which then gets compiled into native C# code. Protocol Buffers use the efficient **HTTP/2** protocol, making service-to-service communication fast and reliable.

### Understanding Protocol Buffers vs REST APIs

To better understand Protocol Buffers, let's compare them to what you might already know from REST API development.

In REST API development, you typically define your API endpoints using controllers and action methods. The contract between client and server is often documented separately (like with OpenAPI/Swagger), and there's no compile-time guarantee that your documentation matches your actual implementation.

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(int id) { ... }
```

With **gRPC,** the service contract is defined first in the `.proto` file using the `service` keyword. This contract becomes the single source of truth, and both client and server code are generated from it, ensuring they're always in sync.

```csharp
service ProductService {
  rpc GetProduct(GetProductRequest) returns (GetProductResponse);
}
```

### Data Transfer and Serialization

REST APIs typically use JSON for data transfer, which is human-readable and widely supported. But JSON is text-based, which has a few negatives. First, it has larger payload sizes due to text encoding. It also comes with some runtime parsing overhead. It doesn’t have any built-in schema validation, and there’s a high potential for typos in field names

```json
{
  "id": "123e4567-e89b-12d3-a456-426614174000",
  "name": "Wireless Headphones",
  "price": 99.99
}
```

gRPC instead uses Protocol Buffers, which serialize data into a compact binary format. This provides significantly smaller payloads (up to 6x smaller than JSON), faster serialization/deserialization, strong typing with compile-time validation, and schema evolution without breaking changes.

#### Transport Protocol Differences

REST APIs run over **HTTP/1.1**, which has some limitations:

*   One request-response cycle per connection
*   Text-based headers (larger overhead)
*   No built-in multiplexing
*   Limited streaming capabilities

gRPC leverages **HTTP/2**, which offers some advantages:

*   **Multiplexing**: Multiple requests over a single connection
*   **Header compression**: Reduced overhead with HPACK
*   **Server push**: The Server can initiate streams to clients
*   **Flow control**: Better handling of slow consumers

#### Data Structure Definitions

In REST APIs, you define **DTOs (Data Transfer Objects)** as regular classes:

```csharp
public class ProductDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
}
```

These DTOs exist only in your specific language and need manual synchronization across different services or languages.

In gRPC, you define **Messages** in the proto file:

```csharp
message ProductModel {
  string id = 1;
  string name = 2;
  double price = 3;
}
```

These message definitions are language-agnostic and automatically generate equivalent classes in any supported programming language.

Here's a quick comparison table to summarize these differences:

| Service Contracts and Interfaces | Service Contracts and Interfaces |
| :------------------------------- | :------------------------------- |
| REST API Concept                 | gRPC Equivalent                  |
| Interface                        | Service                          |
| DTO (Data Transfer Object)       | Message                          |
| JSON Request/Response            | Binary Protocol Buffer           |
| HTTP/1.1                         | HTTP/2                           |

### Creating the Product Proto File

Navigate to the `Protos` folder in your project and create a new file called `product.proto`. Make sure the file extension is `.proto`.

Your project structure should look like this:

```yaml
ProductGrpc/
├── Protos/
│   ├── greet.proto          # Default template file
│   └── product.proto        # Our new proto file
└── ...
```

### Setting Up the Proto File Header

Add the following header to your `product.proto` file:

```cs
//  Protos/product.proto
syntax = "proto3";

option csharp_namespace = "ProductGrpc";

package product;
```

Here’s what’s going on:

*   `syntax = "proto3"`: Specifies that we're using Protocol Buffers version 3
*   `option csharp_namespace = "ProductGrpc"`: Sets the C# namespace for generated code
*   `package product`: Defines the protobuf package name

**Note:** If you named your project differently, make sure the `csharp_namespace` matches your project name.

### Defining the Product Service

In gRPC, services define the available operations (similar to interfaces in REST APIs). Add the following service definition:

```csharp
// :Protos/product.proto
service  ProductsServiceProto {
  rpc CreateProduct(CreateProductRequest) returns (CreateProductResponse);
  rpc GetProduct(GetProductRequest) returns (GetProductResponse);
  rpc ListProducts(ListProductsRequest) returns (ListProductsResponse);
  rpc UpdateProduct(UpdateProductRequest) returns (UpdateProductResponse);
  rpc DeleteProduct(DeleteProductRequest) returns (DeleteProductResponse);
}
```

Service methods explained:

*   `rpc`: Defines a remote procedure call
*   `CreateProduct`: Method name
*   `(CreateProductRequest)`: Input message type
*   `returns (CreateProductResponse)`: Output message type

### Defining Protocol Buffer Messages

Messages in gRPC are equivalent to DTOs in REST APIs. They define the structure of data being exchanged. Let's create all the messages we need:

#### Product Model Message:

```yaml
// product.proto
message ProductModel {
  string id = 1;
  string name = 2;
  string description = 3;
  double price = 4;
  string created_at = 5;
  string updated_at = 6;
  string tags = 7;
}
```

#### Create Operation Messages:

```cs
// Protos/product.proto
message CreateProductRequest {
  string name = 1;
  string description = 2;
  double price = 3;
  string tags = 4;
}

message CreateProductResponse {
  bool success = 1;
  string message = 2;
  ProductModel product = 3;
}
```

#### Read Operation Messages:

```cs
// Protos/product.proto
message GetProductRequest {
  string id = 1;
}

message GetProductResponse {
  bool success = 1;
  string message = 2;
  ProductModel product = 3;
}

message ListProductsRequest {
  int32 page = 1;
  int32 page_size = 2;
}

message ListProductsResponse {
  bool success = 1;
  string message = 2;
  repeated ProductModel products = 3;
  int32 total_count = 4;
}
```

#### Update Operation Messages:

```cs
// Protos/product.proto
message UpdateProductRequest {
  string id = 1;
  string name = 2;
  string description = 3;
  double price = 4;
  string tags = 5;
}

message UpdateProductResponse {
  bool success = 1;
  string message = 2;
  ProductModel product = 3;
}
```

#### Delete Operation Messages:

```cs
// Protos/product.proto
message DeleteProductRequest {
  string id = 1;
}

message DeleteProductResponse {
  bool success = 1;
  string message = 2;
}
```

### Understanding Protocol Buffer Syntax

There are a few key concepts you should understand about how protocol buffers work:

*   **Field Numbers**: Each field has a unique number (for example, `= 1`, `= 2`) used for binary encoding
*   **Field Types**: `string`, `int32`, `double`, `bool` are common scalar types
*   **repeated**: Indicates an array/list (for example, `repeated ProductModel products`)
*   **Message Nesting**: Messages can contain other messages (for example, `ProductModel product`)

Keep in mind that field numbers must be unique within a message, field numbers 1-15 use 1 byte encoding (more efficient), and you should never reuse field numbers (for backward compatibility).

### Complete Product Proto File

Here's your complete `product.proto` file:

```cs
// Protos/product.proto
syntax = "proto3";

option csharp_namespace = "ProductGrpc";

package product;

// Product service definition
service  ProductsServiceProto {
  rpc CreateProduct(CreateProductRequest) returns (CreateProductResponse);
  rpc GetProduct(GetProductRequest) returns (GetProductResponse);
  rpc ListProducts(ListProductsRequest) returns (ListProductsResponse);
  rpc UpdateProduct(UpdateProductRequest) returns (UpdateProductResponse);
  rpc DeleteProduct(DeleteProductRequest) returns (DeleteProductResponse);
}

// Product model message
message ProductModel {
  string id = 1;
  string name = 2;