```yaml
---
title: How To Deploy .NET Application to Azure using Neon Postgres and .NET Aspire
source: https://antondevtips.com/blog/how-to-deploy-dotnet-application-to-azure-using-neon-postgres-and-dotnet-aspire
date_published: 2025-02-18T08:45:51.306Z
date_captured: 2025-08-06T17:35:58.632Z
domain: antondevtips.com
author: Anton Martyniuk
category: database
technologies: [.NET, .NET Aspire, Azure, Neon Serverless Postgres, Entity Framework Core, Npgsql, Docker Compose, Visual Studio, JetBrains Rider, Azure Developer CLI (azd), Swagger, Postman, Git]
programming_languages: [C#, SQL, Bash]
tags: [dotnet, azure, deployment, postgres, serverless, web-api, aspire, database, cloud, ci-cd]
key_concepts: [cloud-deployment, serverless-database, microservices-orchestration, web-api, database-migration, dependency-injection, local-development, azure-cli]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide on deploying a .NET Web API application to Azure using .NET Aspire and Neon Serverless Postgres. It covers setting up the .NET application with EF Core, integrating .NET Aspire for simplified local development and deployment, and configuring a Neon Postgres database. The guide details steps for both local execution and deployment to Azure using the Azure Developer CLI, including authentication and provisioning. It emphasizes how .NET Aspire streamlines the deployment process, making it easier to manage complex cloud-native applications.
---
```

# How To Deploy .NET Application to Azure using Neon Postgres and .NET Aspire

![newsletter](https://antondevtips.com/media/covers/dotnet/cover_dotnet_aspire_neon.png "Cover image for the article: 'How To Deploy .NET Application to Azure using Neon Postgres and .NET Aspire' with a stylized .NET logo.")

# How To Deploy .NET Application to Azure using Neon Postgres and .NET Aspire

Feb 18, 2025

[Download source code](/source-code/how-to-deploy-dotnet-application-to-azure-using-neon-postgres-and-dotnet-aspire)

4 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Where are you deploying your .NET applications? One of the popular choices is Azure.

However, deploying your .NET application to Azure can be a bit challenging. That's exactly why .NET Aspire was created - to simplify both local development and deployment to Azure.

Today I will show you how to deploy your .NET Web API application to Azure using .NET Aspire. For the database, we will be using a Neon Serverless Postgres, which you can set up in less than a minute for free.

Let's dive in!

## Prerequisites

Before we start, ensure you have the following:

*   Azure Subscription: An active [Azure account](https://azure.microsoft.com/) to create and manage your resources.
*   Neon Postgres Account: [Sign up](https://neon.tech/?refcode=44WD03UH) and set up your Neon Postgres instance, you can start for free.

> Note: if you don't have an Azure account and don't plan to create one - you can still run all the code locally on your machine and deploy it using Docker Compose or another instrument of your choice.

## Step 1: Setting Up the .NET Application

I have built a Products API web application that has the following entities:

*   User
*   Product
*   ProductCart
   *   ProductCartItem

This application lets you manage products and create a shopping cart with multiple items.

Let's explore our API endpoints to create, update and delete a product cart for the online store:

```csharp
app.MapPost("/product-carts", async (ProductCartRequest request, IProductCartService productCartService) =>
{
    var response = await productCartService.AddAsync(request);

    return Results.Created($"/product-carts/{response.Id}", response);
});

app.MapPut("/product-carts/{cartId:guid}", async (Guid cartId, ProductCartRequest request, IProductCartService productCartService) =>
{
    var existingProductCart = await productCartService.GetByIdAsync(cartId);
    if (existingProductCart is null)
    {
        return Results.NotFound($"ProductCart with ID {cartId} not found.");
    }

    if (existingProductCart.UserId != request.UserId)
    {
        return Results.BadRequest("Cannot update a ProductCart with a different UserId.");
    }

    await productCartService.UpdateAsync(cartId, request);

    return Results.NoContent();
});

app.MapDelete("/product-carts/{cartId:guid}", async (Guid cartId, IProductCartService productCartService) =>
{
    await productCartService.DeleteByIdAsync(cartId);
    return Results.NoContent();
});
```

For database access, I use EF Core, and here is how I register the DbContext:

```csharp
var connectionString = configuration.GetConnectionString("Postgres");

builder.Services.AddDbContext<ApplicationDbContext>((_, options) =>
{
    options
        .UseNpgsql(connectionString, npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable(DatabaseConsts.MigrationHistoryTable, DatabaseConsts.Schema);
        });

    options.UseSnakeCaseNamingConvention();
});
```

## Step 2: Adding .NET Aspire Support

.NET Aspire makes it super easy to run your application locally and further deploy it to Azure.

Depending on your IDE, there are a couple of options:

**Visual Studio:**

*   Right-click on your Web Application project and select "Add .NET Aspire Orchestrator Support".
*   This creates two new projects: **Aspire App Host** and **ServiceDefaults**.

![Screenshot_1](https://antondevtips.com/media/code_screenshots/dotnet/aspire-neon/img_3.png "Screenshot showing how to add .NET Aspire Orchestrator Support in Visual Studio by right-clicking the project and navigating through the 'Add' menu.")

**JetBrains Rider:**

*   Manually create two projects: **Aspire App Host** and **ServiceDefaults**

![Screenshot_2](https://antondevtips.com/media/code_screenshots/dotnet/aspire-neon/img_1.png "Screenshot of JetBrains Rider's 'New Project' dialog, highlighting the 'Aspire' project type and the option to select 'App Host' or 'ServiceDefaults' for creating Aspire-related projects.")

Here is how our solution looks like (I am using Rider):

![Screenshot_3](https://antondevtips.com/media/code_screenshots/dotnet/aspire-neon/img_2.png "Screenshot of the solution explorer in JetBrains Rider, showing the 'AzureNeonAspire' solution with five projects, specifically highlighting the 'Aspire.ServiceDefaults' and 'AspireHost' projects as 'Aspire Projects'.")

If you're using Rider or prefer manual setup, follow these steps:

1.  Add references:

*   In `ProductService.Host` (our WebApplication) project add reference to `ServiceDefaults` project.
*   In `AspireHost` project add reference to `ProductService.Host` project.

2.  Register Aspire Services in `ProductService.Host` project:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Register other dependencies ...
```

3.  Map Default Endpoints in `ProductService.Host` project:

```csharp
var app = builder.Build();

app.MapDefaultEndpoints();

// Map other endpoints ...
```

4.  Modify `AspireHost` Program.cs:

```csharp
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var connectionString = builder.AddConnectionString("Postgres");

builder.AddProject<ProductService_Host>("product-service")
    .WithExternalHttpEndpoints()
    .WithReference(connectionString);

builder.Build().Run();
```

Here we register our WebApplication project called "product-service" and a connection string to it.

You need to add this connection string to the Aspire Host Project in the `appsettings.json` file:

```json
{
  "ConnectionStrings": {
    "Postgres": "*****"
  }
}
```

## Step 3: Connecting Application to Neon Serverless Postgres

[Neon](https://neon.tech/?refcode=44WD03UH) is a serverless Postgres database designed to help you build reliable and scalable applications faster.

When I observed Neon postgres for the first time, I was really impressed how easy it is to set up a database there, and it's free.

**It offers the following features:**

*   [Scalability](https://neon.tech/docs/introduction/autoscaling?refcode=44WD03UH): instant provisioning, autoscaling and scale to zero.
*   [Instant Provisioning](https://neon.tech/docs/introduction/serverless?refcode=44WD03UH): no waiting, no config.
*   [Branching](https://neon.tech/flow?refcode=44WD03UH): you can create a full database copy in seconds, like you can create a branch in GIT.

The most exciting part for .NET developers - Neon integrates into the .NET ecosystem within seconds. All you need is to setup a Neon database and get a connection string.

**Step 1:** Create your free account in Neon.

![Screenshot_4](https://antondevtips.com/media/code_screenshots/dotnet/aspire-neon/neon1.png "Screenshot of the Neon.tech homepage, highlighting the 'Get Started' button to begin creating a free account.")

![Screenshot_5](https://antondevtips.com/media/code_screenshots/dotnet/aspire-neon/neon2.png "Screenshot of the Neon account creation page, showing options to sign up with Google, GitHub, or email.")

**Step 2:** Create your first database

![Screenshot_6](https://antondevtips.com/media/code_screenshots/dotnet/aspire-neon/neon3.png "Screenshot of the Neon console's 'Create a new project' page, prompting the user to enter a project name, select a region, and choose a Postgres version.")

**Step 3:** Here is how you can manage your database schema and tables

![Screenshot_7](https://antondevtips.com/media/code_screenshots/dotnet/aspire-neon/neon4.png "Screenshot of the Neon console's 'Tables' view, displaying a list of tables within the database schema, including 'ProductCart', 'ProductCartItem', 'Product', and 'User'.")

**Step 4:** Get the connection string to Neon

![Screenshot_8](https://antondevtips.com/media/code_screenshots/dotnet/aspire-neon/neon5.png "Screenshot of the Neon console's 'Connection Details' section, showing the connection string for the Postgres database, with options to copy it.")

For testing purposes, I run EF migrations on application startup (you may consider another option for Production):

```csharp
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    await DatabaseSeedService.SeedAsync(dbContext);
}

await app.RunAsync();
```

That's all we need to do. Let's run our application.

## Step 4: Running .NET Aspire Application Locally

1.  In your IDE, launch the Aspire Host Project. You will see a dashboard:

![Screenshot_9](https://antondevtips.com/media/code_screenshots/dotnet/aspire-neon/img_4.png "Screenshot of the .NET Aspire dashboard, showing the running 'product-service' application with its endpoints and logs, and a 'Postgres' connection string.")

2.  Verify Your Database: Log in to the Neon console to confirm your database is migrated and seeded with data.

![Screenshot_10](https://antondevtips.com/media/code_screenshots/dotnet/aspire-neon/img_5.png "Screenshot of the Neon console's 'Tables' view, showing the 'ProductCart' table with seeded data, including columns like 'Id', 'UserId', 'CreatedOnUtc', and 'UpdatedOnUtc'.")

3.  Test your API. You can use Swagger locally (I have added it manually to my .NET 9 project) or send requests from HTTP Request files (you can find them in the source code).

Once everything works locally, you're ready for deployment to Azure.

## Step 5: Deploying .NET Aspire Application to Azure

With Aspire, it has never been easier to deploy our application to Azure. Follow these steps:

**1\. Create an Azure Profile:** Sign up at [Azure](https://azure.microsoft.com/).

**2\. Install Azure Developer CLI (azd):**

On Windows execute the following command:

```bash
winget install microsoft.azd
```

For other OS, check the [documentation](https://learn.microsoft.com/en-us/azure/developer/azure-developer-cli/install-azd).

**3\. Authenticate with Azure:**

```bash
azd auth login
```

**4\. Init Azure configuration for the project:**

Navigate to the solution folder and execute the command:

```bash
azd init
```

**4\. Provision and Deploy:**

Navigate to the solution folder and execute the command to begin provisioning and deployment:

```bash
azd up
```

This command provisions and deploys your application. Follow the prompts (select “use code in the current directory” and provide a resource group name), then wait for the deployment to finish.

**5\. Verify on Azure:** Once deployed, check the Azure Portal to see your containers running.

![Screenshot_11](https://antondevtips.com/media/code_screenshots/dotnet/aspire-neon/img_6.png "Screenshot of the Azure Portal, showing the deployed 'product-service' container app within a resource group, indicating its running status.")

**6\. Test the API:** Use Postman or your preferred tool to send a request (e.g., get a product cart) and confirm everything works.

![Screenshot_12](https://antondevtips.com/media/code_screenshots/dotnet/aspire-neon/img_7.png "Screenshot of Postman showing a successful GET request to the deployed product-carts API endpoint, returning a JSON response with product cart data.")

Our mission is accomplished, as we have deployed an application to Azure, that connects to Neon Postgres database.

## Bonus: Deploying Neon Natively to Azure

You can also deploy Neon Serverless Postgres as a native Azure container. Simply find "Neon Serverless Postgres" in the Azure Marketplace, select the free subscription option, set up your database, and update your connection string. You can even connect to your Neon Azure database via the [Neon Cloud](https://neon.tech/?refcode=44WD03UH)!

![Screenshot_13](https://antondevtips.com/media/code_screenshots/dotnet/aspire-neon/img_8.png "Screenshot of the Azure Marketplace, showing 'Neon Serverless Postgres' as a selectable service, indicating its availability for native deployment within Azure.")

## Summary

In this post, we covered everything from setting up your .NET Web API application, integrating .NET Aspire for easy deployments, and connecting to a Neon Serverless Postgres database.

**We walked through:**

*   Setting up the Products API application.
*   Adding and configuring .NET Aspire support.
*   Setting up a Neon Postgres database and connecting it to your application.
*   Running your application locally and deploying it to Azure using the Azure CLI.

With .NET Aspire and Neon, you can focus more on building great features and less on deployment hassles.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-deploy-dotnet-application-to-azure-using-neon-postgres-and-dotnet-aspire)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-deploy-dotnet-application-to-azure-using-neon-postgres-and-dotnet-aspire&title=How%20To%20Deploy%20.NET%20Application%20to%20Azure%20using%20Neon%20Postgres%20and%20.NET%20Aspire)[X](https://twitter.com/intent/tweet?text=How%20To%20Deploy%20.NET%20Application%20to%20Azure%20using%20Neon%20Postgres%20and%20.NET%20Aspire&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-deploy-dotnet-application-to-azure-using-neon-postgres-and-dotnet-aspire)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-deploy-dotnet-application-to-azure-using-neon-postgres-and-dotnet-aspire)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.