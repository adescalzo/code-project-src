```yaml
---
title: "Understanding GraphQL in .NET: When and why to use it"
source: https://blog.elmah.io/understanding-graphql-in-net-when-and-why-to-use-it/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2040
date_published: 2025-04-15T10:28:32.000Z
date_captured: 2025-08-12T12:19:13.944Z
domain: blog.elmah.io
author: Unknown
category: backend
technologies: [GraphQL, .NET, ASP.NET Core, RESTful API, Entity Framework Core, HotChocolate, WebSockets, InMemory Database, Nitro]
programming_languages: [C#, GraphQL, SQL]
tags: [graphql, .net, asp.net-core, api, rest, data-fetching, hotchocolate, database, entity-framework-core, web-api]
key_concepts: [graphql-query-language, restful-api-design, overfetching, underfetching, graphql-mutations, graphql-queries, graphql-subscriptions, client-server-communication]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores GraphQL as an efficient alternative to traditional RESTful APIs, addressing common issues like overfetching and underfetching data. It illustrates GraphQL's benefits through practical examples in social media, e-commerce, and IoT applications, emphasizing its flexibility and real-time capabilities via subscriptions. The post provides a step-by-step guide on implementing a GraphQL API in ASP.NET Core using Entity Framework Core and the HotChocolate framework. It demonstrates how to define GraphQL queries and mutations, configure the server, and test the API using HotChocolate's Nitro IDE. The conclusion summarizes GraphQL's advantages, such as reduced server-side code and simplified client integration, while also acknowledging potential complexities.]
---
```

# Understanding GraphQL in .NET: When and why to use it

# Understanding GraphQL in .NET: When and why to use it

Written by Ali Hamza Ansari, April 15, 2025

APIs are the heart of most modern applications. Due to their simplicity and lightweight design, RESTful APIs are a popular choice for client-server communication in most applications. However, APIs can become limiting when fetching complex or related data. The front end may over-fetch or under-fetch the meaningful data. For example, different pages require different responses. RESTFul APIs require different field endpoints, each involving repetitive complex joining conditions. To eliminate some of the disadvantages of CRUD-based APIs, you can opt for GraphQL. In this post, I will explore how GraphQL can be a good alternative to RESTful APIs for several scenarios.

![A purple robot character with a .NET logo on its chest stands next to a screen displaying the GraphQL logo and text. The overall theme is about understanding GraphQL in .NET.](https://blog.elmah.io/content/images/2025/04/understanding-graphql-in-net-when-and-why-to-use-it-o-1.png)

## What is GraphQL?

[GraphQL](https://graphql.org/) is an open-source API query language and runtime that allows data manipulation from different sources and returns the result as a single, unified response. Facebook introduced GraphQL in 2012 and made it open-source in 2015. Clients can request customized data responses using the GraphQL query runtime, making fetching flexible and easy.

## When should you choose between RESTful API and GraphQL?

RESTful API and GraphQL both offer advantages and are used in all-scale projects. Before knowing the implementation, you must analyze whether your application needs GraphQL.

GraphQL provides client flexibility, so it is suitable for scenarios where complex data needs to be fetched.

### Example 1: Social Media App

Take, for example, a social media app where you need to load a user's feed. If we naively implement them, then multiple endpoints will be required such as:

```undefined
/posts
/users/{id}
/likes?postId=
/comments?postId=
```

This solution will result in multiple round trips to the server. Another approach is to return all the data at once. To return complex data, you will need to perform joins that may overfetch the data, leading to additional data transfer over the network, and negatively impacting performance and the user experience. And what if slightly different data is required, such as displaying a compact feed for mobile? A classic API will need a separate endpoint, requiring more work done.

However, GraphQL offers a flexible and efficient solution to the problem with one query:

```undefined
{
  feed {
    id
    image
    caption
    author {
      username
      profilePicture
    }
    likesCount
    comments {
      user {
        username
      }
      message
    }
  }
}
```

Similarly, you can fetch a customized set of fields by querying GraphQL. You will need a lot of work in creating separate models and endpoints.

### Example 2: E-commerce app

Another scalable application can utilize GraphQL to improve performance and enhance resource usage. An e-commerce app needs product descriptions, images, prices, and stock. In contrast, mobile apps require a lightweight response with product name and price only. A RESTful API can solve the issue, but you must develop different endpoints. Similarly, this number increases with the customization of the returned data.

With GraphQL, your client needs to query the specific fields

For mobile:

```undefined
{
  products {
    name
    price
  }
}
```

And for web:

```undefined
{
  products {
    name
    price
    description
    stock
    images { url }
  }
}
```

### Example 3: Dashboard with real-time IoT data

If you are designing a dashboard for IoT sensors' real-time data, you must poll the server every few seconds for sensor updates with RESTful APIs. Additionally, each sensor will require separate data fetching and multiplying of the API calls.

On the other hand, GraphQL natively supports subscriptions (real-time) via WebSockets, enabling efficient live updates. You can do it like this:

```undefined
subscription {
  sensorUpdates {
    id
    temperature
    humidity
  }
}
```

## How to implement GraphQL in ASP .NET Core?

There are long lists of scenarios where GraphQL outperforms RESTful APIs. I already mentioned a few of them, so let's move towards how to create and query a GraphQL API in .NET.

**Step 1: Create a demo API project**

```console
dotnet new webapi -n GraphQLProductDemo
cd GraphQLProductDemo
```

**Step 2: Install the necessary packages**

We'll use Entity Framework Core and HotChocolate which is an advanced GraphQL framework for .NET:

```console
dotnet add package Microsoft.EntityFrameworkCore.InMemory
dotnet add package HotChocolate.AspNetCore
dotnet add package HotChocolate.Data.EntityFramework
```

**Step 3: Define models**

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
}
```

**Step 4: Setup EFCore DB context**

```csharp
using GraphQLProductDemo.Models;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext: DbContext
{
    public DbSet<Product> Products { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }
}
```

**Step 5: Create Query and Mutation Types**

A query is a class in GraphQL that defines data read operations. A mutation class defines data write operations.

```csharp
using GraphQLProductDemo.Data;
using GraphQLProductDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphQLProductDemo;

public class Query
{
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Product> GetProducts([Service] IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
       var context = dbContextFactory.CreateDbContext();
       return context.Products;
    }
}
```

In the code above, the `GetProducts` method inside the query class encapsulates the fetch logic. `[Service]` injects services container. `[UseFiltering]` / `[UseSorting]` adds automatic support for filtering and sorting your GraphQL queries.

```csharp
using GraphQLProductDemo.Data;
using GraphQLProductDemo.Models;
using Microsoft.EntityFrameworkCore;

namespace GraphQLProductDemo;

public class Mutation
{
    public async Task<Product> AddProductAsync(Product product, [Service] IDbContextFactory<ApplicationDbContext> dbContextFactory)
    {
       var context = dbContextFactory.CreateDbContext();
       context.Products.Add(product);
       await context.SaveChangesAsync();
       return product;
    }
}
```

The `AddProductAsync` mutation method inputs a `Product` object and saves it into the database.

**Step 6: Configure DbContext, Data seeding, and GraphQL in Program.cs**

```csharp
using GraphQLProductDemo;
using GraphQLProductDemo.Data;
using GraphQLProductDemo.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Register EF Core InMemory
builder.Services.AddPooledDbContextFactory<ApplicationDbContext>(
    options => options.UseInMemoryDatabase("ProductDb"));

// Add services to the container.
builder.Services.AddOpenApi();

// Register GraphQL services
builder.Services.AddGraphQLServer()
    .AddQueryType<Query>()
    .AddMutationType<Mutation>()
    .AddProjections() // Enables [UseProjection]
    .AddFiltering()   // Enables [UseFiltering]
    .AddSorting();    // Enables [UseSorting]

var app = builder.Build();

// Seed data
using (var scope = app.Services.CreateScope())
{
    var dbContextFactory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
    using var context = dbContextFactory.CreateDbContext();
    context.Database.EnsureCreated();
    context.Products.AddRange(
        new Product { Name = "Shoes", Category = "Footware", Price = 128.99M },
        new Product { Name = "Speaker", Category = "Accessories", Price = 49.49M }
    );
    context.SaveChanges();
}

// Map GraphQL endpoint
app.MapGraphQL();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.Run();
```

**Step 7: Run and Test**

```console
dotnet run
```

On running the project, we can see the app started

![A screenshot of a command prompt showing console output from a .NET application startup, indicating that the application is listening on http://localhost:5062 and has saved entities to an in-memory store.](https://blog.elmah.io/content/images/2025/04/app-started-1.png)

I navigated to `localhost:5062/graphql/` in the browser and HotChocolate's "Nitro" (formerly **Banana Cake Pop**) is loaded for querying:

![A screenshot of the HotChocolate "Nitro" (formerly Banana Cake Pop) GraphQL IDE in a web browser, showing the initial interface with options like "Create Document" and "Browse Schema".](https://blog.elmah.io/content/images/2025/04/nitro-1.png)

When clicking the _Create Document_ button the following is shown:

![A screenshot of the HotChocolate Nitro IDE, displaying a context menu with options like "Open New Tab", "Save Current Tab", and "Run Current Operation" after clicking the "Create Document" button.](https://blog.elmah.io/content/images/2025/04/create-document-1.png)

To show how to modify the desired fields, let's change the query a bit:

![A screenshot of the HotChocolate Nitro IDE showing a GraphQL query in the request panel: { products { id, name, price } }.](https://blog.elmah.io/content/images/2025/04/create-request-1.png)

In the example, I only requested an ID and name, so the server returned a specific response.

Let's test an Add operation:

![A screenshot of the HotChocolate Nitro IDE showing a GraphQL mutation query to add a new product, along with its successful response.](https://blog.elmah.io/content/images/2025/04/testing-add-operation-1.png)

We can verify that the add was successful by querying again:

![A screenshot of the HotChocolate Nitro IDE showing a GraphQL query for products and the response, which includes the newly added product.](https://blog.elmah.io/content/images/2025/04/verify-new-record-1.png)

That's it. By the help of Entity Framework Core, HotChocolate, and ASP.NET Core, adding GraphQL to your API only requires a few lines of code. The example above is, of course, highly simplified to show the basis of adding the feature in ASP.NET Core. If someone is interested in more complex examples, I'll consider writing a follow-up post.

## Conclusion

In modern application development, APIs are central in connecting clients and servers. RESTful (CRUD-based) APIs have long been the preferred choice for developers due to their simplicity, lightweight design, and well-defined conventions. However, the responses need to be defined in a model. Most often the client gets more fields than required, or it may miss some essential fields in the response, resulting in overfetching and underfetching, respectively. You must provide separate APIs for different use cases to cope with this. The work increases if a client needs a different response from an entity.

GraphQL is an alternative to deal with these limitations. It allows the client to get only the required fields by dynamically requesting them in the query. The server only needs to expose a query method, while the front end can get arbitrary fields that meet the display need. GraphQL significantly decreases code at the server and also eases the front-end integration to avoid calling multiple endpoints. On the downside, GraphQL also introduces complexity on the server side, such as query depth limiting, performance concerns, and the potential for overly complex queries.

In this post, I presented different scenarios that could benefit from GraphQL and used the HotChocolate framework to implement GraphQL in an ASP.NET Core example.