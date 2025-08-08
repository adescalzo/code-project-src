```yaml
---
title: Building a Multitenant Cloud Application With Azure Functions and Neon Postgres
source: https://antondevtips.com/blog/building-a-multitenant-cloud-application-with-azure-functions-and-neon-postgres
date_published: 2025-03-18T08:45:32.087Z
date_captured: 2025-08-06T17:36:34.942Z
domain: antondevtips.com
author: Anton Martyniuk
category: database
technologies: [Azure Functions, Neon Postgres, .NET Aspire, Azure KeyVault, Refit, HttpClientFactory, Entity Framework Core, PostgreSQL, Visual Studio, JetBrains Rider, Postman, Azure Portal, IMemoryCache, ASP.NET Core, Microsoft.AspNetCore.App, Microsoft.Azure.Functions.Worker, Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore, Microsoft.Azure.Functions.Worker.Sdk, Refit.HttpClientFactory, Aspire.Hosting.AppHost, Aspire.Hosting.Azure.Functions, Aspire.Hosting.Azure.KeyVault, Aspire.Azure.Security.KeyVault, Npgsql]
programming_languages: [C#, SQL]
tags: [multitenancy, azure-functions, postgresql, cloud-native, dotnet, aspire, data-isolation, serverless, api-management, database-per-tenant]
key_concepts: [multitenancy, database-per-tenant, serverless-architecture, cloud-application-development, api-integration, dependency-injection, database-migrations, connection-string-management]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details building a multitenant cloud application using Azure Functions and Neon Postgres, focusing on the "Database-per-Tenant" approach. It demonstrates how to leverage Neon's APIs for automated database creation, updating, and deletion for each tenant. The solution integrates .NET Aspire for orchestrating services, Azure KeyVault for secure connection string storage, and Refit for streamlined API communication. The author provides step-by-step guidance on setting up tenant data models, creating an Azure Function for Neon API interactions, and programmatically applying EF Core migrations to new tenant databases. The post concludes with instructions on deploying and testing the complete solution in Azure, showcasing tenant isolation and dynamic database provisioning.]
---
```

# Building a Multitenant Cloud Application With Azure Functions and Neon Postgres

![Cover image for the article titled "Building a Multitenant Cloud Application With Azure Functions and Neon Postgres"](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fazure%2Fcover_azure_functions_neon_api.png&w=3840&q=100)

# Building a Multitenant Cloud Application With Azure Functions and Neon Postgres

Mar 18, 2025

[Download source code](/source-code/building-a-multitenant-cloud-application-with-azure-functions-and-neon-postgres)

9 min read

### Newsletter Sponsors

Bad PDF are slowing your progress? [Nutrient's PDF SDK](https://www.nutrient.io/sdk?utm_campaign=newsletter4-2025-03-18&utm_source=anton-dev-tips&utm_medium=sponsoring) delivers fast loading, reliable signatures, forms and annotations with seamless rendering for any platforms. Trusted by over 1 billion users in more than 150 different countries. [Check out](https://www.nutrient.io/sdk?utm_campaign=newsletter4-2025-03-18&utm_source=anton-dev-tips&utm_medium=sponsoring) how it stands out with their free plan.

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

When I observed [Neon Postgres](https://neon.tech?refcode=44WD03UH) for the first time, I was really impressed how easy it is to set up a free Cloud database there. Neon has an amazing set of Web APIs which you can use to create projects, databases, branches (like in GIT) and so much more.

I was really interested in what I can build with Neon APIs. And I decided to build a Cloud Multitenant Application on Azure.

Today I want to share my journey on building a Multitenant Cloud Application with Azure Functions and Neon.

[](#a-quick-introduction-to-multitenancy)

## A Quick Introduction To Multitenancy

**Multitenancy** is a software architecture that allows a single instance of a software application to serve multiple customers, called **tenants**. Each tenant's data is isolated and remains invisible to other tenants for security reasons.

There are several approaches to separate data for each tenant in multi-tenant applications:

*   **Database-per-Tenant:** each tenant has its own database. This model offers strongest data isolation but may introduce database management complexities and increase costs with many tenants.
*   **Schema-per-Tenant:** a single database with separate schemas for each tenant. It provides a balance between isolation and resource sharing.
*   **Table-per-Tenant:** a single database and schema, with tenant-specific tables. This model is efficient but may complicate data management.
*   **Discriminator Column:** a single database, schema, and tables, with a column indicating the tenant. This is the simplest but least isolated model.

Today I want to show you how Neon greatly simplifies a **Database-per-Tenant** approach. With [Neon APIs](https://api-docs.neon.tech/reference/getting-started-with-neon-api?refcode=44WD03UH) you can automate database creation, updating and deletion for each tenant.

[](#prerequisites)

## Prerequisites

Before we start, ensure you have the following:

*   Azure Subscription: An active [Azure account](https://azure.microsoft.com/) to create and manage your resources.
*   Neon Postgres Account: [Sign up](https://neon.tech/signup?refcode=44WD03UH) and set up your Neon Postgres instance, you can start for free.

For Neon database, you have two options:

*   Setup a database in Neon Cloud
*   Setup "Neon Serverless Postgres" as native Azure container

For both options, you can select a free subscription option and upgrade as-you-go.

In one of the [previous articles](https://antondevtips.com/blog/how-to-deploy-dotnet-application-to-azure-using-neon-postgres-and-dotnet-aspire?utm_source=antondevtips&utm_medium=website&utm_campaign=website) I explained how to build a Products API and deploy it to Azure with .NET Aspire and Neon. This application lets you manage products and allows users to create a shopping cart with multiple items.

Today we will evolve this application and make it multi-tenant. A tenant in this system will be a store that has their own products, stored in separate databases. Users will be able to order products in each of these stores.

Building such an application involves the following steps:

1.  Adding `Tenant` data models and APIS to the Products API service
2.  Creating Azure Function for managing tenant databases with Neon API
3.  Setting up .NET Aspire project
4.  Adding communication between Products API and Tenant Management Azure Function
5.  Migrating a tenant database and managing connection strings in Azure KeyVault with Caching support
6.  Deploying and testing our solution in Azure

> You can download source code for the entire solution at the end of the post.

Let's dive in.

[](#step-1-adding-tenant-to-products-api)

## Step 1: Adding Tenant To Products API

Create a `Tenant` entity in the Products API (simplified version to keep it simple):

```csharp
public class Tenant
{
    public Guid Id { get; set; }
    public string Name { get; set; }
}
```

In `Product` and `ProductCart` entities add a foreign key to the `Tenant`:

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public Guid? TenantId { get; set; }
}

public class ProductCart
{
    public Guid Id { get; set; }
    public int Quantity { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public List<ProductCartItem> CartItems { get; set; } = [];
    
    public Guid? TenantId { get; set; }
}
```

Let's define the APIs for managing tenants in our application:

```csharp
public record CreateTenantRequest(string Name);
public record UpdateTenantRequest(string CurrentName, string NewName);

app.MapPost("/tenants", (CreateTenantRequest request) => { });
app.MapPatch("/tenants/{currentName}", (string currentName, UpdateTenantRequest request) => { });
app.MapDelete("/tenants/{name}", (string name) => { });
app.MapGet("/tenants", () => { });
```

Our first step is ready, we will return to implement these endpoints later.

[](#step-2-creating-tenant-management-azure-function)

## Step 2: Creating Tenant Management Azure Function

To create Azure Function in Visual Studio, select "Azure Function" project template and select the Function's type to "HTTP" trigger. You can find a detailed manual [here](https://learn.microsoft.com/en-us/azure/azure-functions/functions-develop-vs?pivots=isolated).

For JetBrains Rider, install [Azure Toolkit for Rider](https://plugins.jetbrains.com/plugin/11220-azure-toolkit-for-rider) and create "Azure Function" project. You can find a detailed manual [here](https://blog.jetbrains.com/dotnet/2020/10/29/build-serverless-apps-with-azure-functions/).

In our Tenant Management Function, make sure the following Nuget packages are installed:

```xml
<ItemGroup>
    <FrameworkReference Include="Microsoft.AspNetCore.App" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker" Version="2.0.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Extensions.Http.AspNetCore" Version="2.0.0" />
    <PackageReference Include="Microsoft.Azure.Functions.Worker.Sdk" Version="2.0.0" />
    <PackageReference Include="Refit.HttpClientFactory" Version="8.0.0" />
</ItemGroup>
```

Before diving into the code, let's briefly discuss the structure of Neon.

First, you need to create a project in Neon:

![Neon Console: "Your projects" dashboard showing account usage and a list of projects, with "DevTips" project highlighted.](https://antondevtips.com/media/code_screenshots/azure/azure-functions-neon-api/img_1.png)

Each project has 1 or more branches (like in GIT):

![Neon Console: "Project Dashboard" showing branches, with "main" branch highlighted, and metrics like storage and compute.](https://antondevtips.com/media/code_screenshots/azure/azure-functions-neon-api/img_2.png)

Each branch contains a database.

We need to implement the following API requests:

*   create a database
*   update a database
*   delete a database
*   get all databases for reference
*   get database connection string

We will use a **Refit** library to simplify our API calls to Neon to manage databases. Refit is a wrapper around HttpClientFactory that turns REST calls into simple method calls, which makes our code cleaner and easier to work with.

Let's explore `INeonApi` interface used for communication with Neon:

```csharp
public interface INeonApi
{
    [Get("/api/v2/projects/{projectId}/branches/{branchId}/databases")]
    Task<NeonDatabaseListResponseJson> GetDatabasesAsync(
        [AliasAs("projectId")] string projectId,
        [AliasAs("branchId")] string branchId);
    
    [Post("/api/v2/projects/{projectId}/branches/{branchId}/databases")]
    Task<NeonDatabaseCreateResponse> CreateDatabaseAsync(
        [AliasAs("projectId")] string projectId,
        [AliasAs("branchId")] string branchId,
        [Body] CreateNeonDatabaseRequest request);
    
    [Patch("/api/v2/projects/{projectId}/branches/{branchId}/databases/{databaseName}")]
    Task<NeonDatabaseCreateResponse> UpdateDatabaseAsync(
        [AliasAs("projectId")] string projectId,
        [AliasAs("branchId")] string branchId,
        [AliasAs("databaseName")] string databaseName,
        [Body] UpdateNeonDatabaseRequest request);

    [Delete("/api/v2/projects/{projectId}/branches/{branchId}/databases/{databaseName}")]
    Task<NeonDatabaseCreateResponse> DeleteDatabaseAsync(
        [AliasAs("projectId")] string projectId,
        [AliasAs("branchId")] string branchId,
        [AliasAs("databaseName")] string databaseName);

    [Get("/api/v2/projects/{projectId}/connection_uri")]
    Task<ConnectionStringResponse> GetConnectionStringAsync(
        [AliasAs("projectId")] string projectId,
        [AliasAs("branch_id")] string? branchId,
        [AliasAs("database_name")] string databaseName,
        [AliasAs("role_name")] string roleName);
}
```

Each API call needs a project and a branch identifier.

You can find **project identifier** in the `settings` tab:

![Neon Console: "Settings" page for a project, showing "Project ID" field highlighted with an arrow pointing to it.](https://antondevtips.com/media/code_screenshots/azure/azure-functions-neon-api/img_5.png)

You can find **branch identifier** in the `overview` tab:

![Neon Console: "Overview" tab for a branch, showing the "ID" field (branch ID) highlighted with an arrow.](https://antondevtips.com/media/code_screenshots/azure/azure-functions-neon-api/img_6.png)

To learn more about Neon API, get started with documentation [here](https://api-docs.neon.tech/reference/getting-started-with-neon-api?refcode=44WD03UH).

You can also examine all the requests and responses in details and even execute them in the complete [Neon API reference](https://api-docs.neon.tech/reference/createprojectbranchdatabase?refcode=44WD03UH).

To be able to send API requests to Neon, you need to create an API Key. You can create one in the "Account settings" > "API keys":

![Neon Console: "Account settings" page, showing "API keys" section with an arrow pointing to the "Create API Key" button.](https://antondevtips.com/media/code_screenshots/azure/azure-functions-neon-api/img_7.png)

We use Azure Functions to handle tenant database requests. Each Azure Function has HTTP triggers and is designed to handle one task.

Let's explore the create database function:

```csharp
internal record CreateTenantDatabaseRequest(string DatabaseName);
internal record CreateTenantDatabaseResponse(TenantDatabaseDetails Database,
    string ConnectionString);

public class CreateTenantDatabaseFunction(
    ILogger<CreateTenantDatabaseFunction> logger,
    NeonConfigurationProvider configurationProvider,
    INeonApi neonApi)
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    
    [Function(nameof(CreateNeonDatabase))]
    public async Task<HttpResponseData> CreateNeonDatabase(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "database/create")]
        HttpRequestData requestData)
    {
        try
        {
            var requestBody = await new StreamReader(requestData.Body).ReadToEndAsync();
            var createRequest = JsonSerializer.Deserialize<CreateTenantDatabaseRequest>(
                requestBody, SerializerOptions);

            if (createRequest is null)
            {
                logger.LogError("Failed to deserialize request");

                var badRequestResponse = requestData.CreateResponse(HttpStatusCode.BadRequest);
                await badRequestResponse.WriteAsJsonAsync(new { error = "Invalid request body" });
                return badRequestResponse;
            }
            
            var neonResponse = await CreateNeonDatabaseAsync(createRequest);
            var connectionStringResponse = await GetConnectionStringAsync(
                createRequest.DatabaseName);
            
            logger.LogInformation("Database {DatabaseName} created: {@NeonResponse}",
                createRequest.DatabaseName, neonResponse);

            var response = requestData.CreateResponse(HttpStatusCode.OK);

            var tenantDatabaseDetails = neonResponse.MapToResponseDetails();
            
            var response = new CreateTenantDatabaseResponse(tenantDatabaseDetails,
                connectionStringResponse.Uri);
                
            await response.WriteAsJsonAsync(response);
            
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create Neon database");
            var errorResponse = requestData.CreateResponse(HttpStatusCode.InternalServerError);
            await errorResponse.WriteAsJsonAsync(new { error = "Failed to create database" });
            return errorResponse;
        }
    }
}
```

Let's walk step by step:

1.  Deserialize the HTTP request body that triggers function.
2.  Create a Neon database in the given project and branch.
3.  Get connection string to a newly created database.
4.  Return response.

To create a database and get its connection string, we use the Refit interface:

```csharp
private async Task<NeonDatabaseCreateResponse> CreateNeonDatabaseAsync(
    CreateTenantDatabaseRequest createRequest)
{
    var neonConfiguration = configurationProvider.Get();

    var neonRequest = new CreateNeonDatabaseRequest(new CreateDatabaseInfo
    {
        Name = createRequest.DatabaseName,
        OwnerName = neonConfiguration.OwnerName
    });

    return await neonApi.CreateDatabaseAsync(
        neonConfiguration.ProjectId,
        neonConfiguration.BranchId,
        neonRequest);
}

private async Task<ConnectionStringResponse> GetConnectionStringAsync(
    string databaseName)
{
    var neonConfiguration = configurationProvider.Get();
    return await neonApi.GetConnectionStringAsync(
        neonConfiguration.ProjectId,
        neonConfiguration.BranchId,
        databaseName,
        neonConfiguration.OwnerName);
}
```

Here is our Azure Function setup in `Program.cs`:

```csharp
var builder = FunctionsApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Logging.AddConsole();
builder.ConfigureFunctionsWebApplication();

builder.Services.AddTransient<AuthDelegatingHandler>();
builder.Services.AddTransient<NeonConfigurationProvider>();

var neonUrl = builder.Configuration.GetConnectionString("NeonUrl")!;

builder.Services.AddRefitClient<INeonApi>()
    .ConfigureHttpClient(c => c.BaseAddress = new Uri(neonUrl))
    .AddHttpMessageHandler<AuthDelegatingHandler>()
    .AddStandardResilienceHandler();

builder.Build().Run();
```

In each API request to Neon, you need to provide an `Authorization` header with "Bearer API\_KEY". For this purpose I use `DelegatingHandler` that I add to the Refit HttpClientFactory:

```csharp
public class AuthDelegatingHandler(IConfiguration configuration) : DelegatingHandler
{
    private readonly string _apiKey = configuration.GetConnectionString("NeonApiKey")
        ?? throw new InvalidOperationException("NeonApiKey configuration is missing");

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, 
        CancellationToken cancellationToken)
    {
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        return await base.SendAsync(request, cancellationToken);
    }
}
```

This function has the following connection strings (coming from Aspire):

*   NeonApiKey
*   NeonUrl

And configuration parameters in `local.settings.json` (coming from Aspire):

```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "NEON_PROJECT_ID": "[COMING_FROM_ASPIRE]",
        "NEON_BRANCH_ID": "[COMING_FROM_ASPIRE]",
        "NEON_DATABASE_OWNER": "[COMING_FROM_ASPIRE]"
    }
}
```

Other functions are implemented in the same way.

[](#step-3-setting-up-net-aspire-project)

## Step 3: Setting up .NET Aspire project

You can find a detailed guide on how to add .NET Aspire to the project in one of my previous [articles](https://antondevtips.com/blog/how-to-deploy-dotnet-application-to-azure-using-neon-postgres-and-dotnet-aspire?utm_source=antondevtips&utm_medium=website&utm_campaign=website).

Let's explore our Aspire Host project:

```csharp
using Projects;

var builder = DistributedApplication.CreateBuilder(args);

var neonApiKey = builder.AddConnectionString("NeonApiKey");
var neonUrl = builder.AddConnectionString("NeonUrl");

var configuration = builder.Configuration;

var neonProjectId = configuration["NeonProjectId"];
var neonBranchId = configuration["NeonBranchId"];
var neonOwnerName = configuration["NeonDatabaseOwner"];

var keyVault = builder.ExecutionContext.IsPublishMode
    ? builder.AddAzureKeyVault("Secrets")
    : builder.AddConnectionString("Secrets");

var function = builder.AddAzureFunctionsProject<Multitenancy_Function>("multitenancy-api")
    .WithReference(neonApiKey)
    .WithReference(neonUrl)
    .WithEnvironment("NEON_PROJECT_ID", neonProjectId)
    .WithEnvironment("NEON_BRANCH_ID", neonBranchId)
    .WithEnvironment("NEON_DATABASE_OWNER", neonOwnerName)
    .WithExternalHttpEndpoints();

var databaseConnectionString = builder.AddConnectionString("Postgres");

builder.AddProject<ProductService_Host>("product-service")
    .WithExternalHttpEndpoints()
    .WithReference(function)
    .WithReference(keyVault)
    .WithReference(databaseConnectionString)
    .WaitFor(function);

builder.Build().Run();
```

I use the following Nuget packages:

```xml
<ItemGroup>
  <PackageReference Include="Aspire.Hosting.AppHost" Version="9.1.0" />
  <PackageReference Include="Aspire.Hosting.Azure.Functions" Version="9.1.0-preview.1.25121.10" />
  <PackageReference Include="Aspire.Hosting.Azure.KeyVault" Version="9.1.0" />
</ItemGroup>
```

Here we specify:

*   Azure Function that sends requests to Neon
*   Products API application that manages products, product cart and sends request to our Azure Function
*   Azure Key Vault for storing connection strings

[](#step-4-integrating-products-api-with-tenant-management-azure-function)

## Step 4: Integrating Products API with Tenant Management Azure Function

Add the Azure KeyVault Nuget package to the Products API project:

```xml
<PackageReference Include="Aspire.Azure.Security.KeyVault" Version="9.1.0" />
```

Register the dependencies in DI:

```csharp
services.AddMemoryCache();
services.AddHttpContextAccessor();

builder.Configuration.AddAzureKeyVaultSecrets("Secrets");
builder.AddAzureKeyVaultClient("Secrets");
```

I will also use Refit for sending requests from Products API to the Azure Function:

```csharp
public interface ITenantApi
{
    [Post("/api/database/create")]
    Task<CreateTenantDatabaseResponse> CreateDatabaseAsync([Body] CreateTenantDatabaseRequest request);
    
    [Patch("/api/database/update/{database}")]
    Task<DatabaseDetails> UpdateDatabaseAsync(string database, [Body] UpdateTenantDatabaseRequest request);
    
    [Delete("/api/database/delete/{database}")]
    Task<DatabaseDetails> DeleteDatabaseAsync(string database);
    
    [Get("/api/database")]
    Task<ListDatabasesResponse> ListDatabasesAsync();
}
```

Let's explore the create tenant endpoint:

```csharp
app.MapPost("/tenants", async (
    CreateTenantRequest request,
    ITenantApi tenantApi,
    ApplicationDbContext applicationDbContext,
    IDatabaseMigrator databaseMigrator,
    ILogger<TenantEndpoints> logger) =>
{
    await using var transaction = await applicationDbContext.Database.BeginTransactionAsync();

    try
    {
        // Implementation code ...
        
        await transaction.CommitAsync();

        return Results.Ok(new
        {
            TenantId = tenant.Id,
            DatabaseName = databaseName
        });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Failed to create tenant {TenantName}", request.Name);
        await transaction.RollbackAsync();
        return Results.Problem("Failed to create tenant");
    }
}
```

1.  Make sure to wrap everything in a transaction, so we can rollback the changes in case if Azure Function returns failure.
    
2.  Save tenant in the "master" database, the one that will hold all the tenants data:
    

```csharp
var tenant = new Tenant
{
    Id = Guid.NewGuid(),
    Name = request.Name
};

applicationDbContext.Tenants.Add(tenant);
await applicationDbContext.SaveChangesAsync();
```

3.  Send request to create a tenant database:

```csharp
var databaseName = $"products-{request.Name}";
var createDatabaseRequest = new CreateTenantDatabaseRequest(databaseName);
var response = await tenantApi.CreateDatabaseAsync(createDatabaseRequest);
```

4.  We need to somehow create tables in our newly created database, for this purpose we can use EF Core migrations and programmatically apply them to the database.

```csharp
try
{
    await databaseMigrator.MigrateDatabaseAsync(tenant.Id.ToString(), connectionString);
}
catch (Exception ex)
{
    logger.LogError(ex, "Failed to apply migrations for tenant {TenantName}", request.Name);
    await transaction.RollbackAsync();

    try
    {
        await tenantApi.DeleteDatabaseAsync(databaseName);
    }
    catch (Exception cleanupEx)
    {
        logger.LogError(cleanupEx, "Failed to cleanup tenant database after migration failure");
    }

    return Results.Problem("Failed to setup tenant database");
}
```

You can like or dislike this approach, but it works for me, as it is fully automated.

5.  If everything worked fine - make sure to commit changes, otherwise delete the tenant database by sending a corresponding API request and rollback the transaction

[](#step-5-migrating-a-tenant-database-and-managing-connection-strings)

## Step 5: Migrating a Tenant Database and Managing Connection Strings

Let's explore how migrations are applied to a tenant database. Here is the code for `DatabaseMigrator`:

```csharp
await databaseMigrator.MigrateDatabaseAsync(tenant.Id.ToString(), connectionString);

public class DatabaseMigrator(IServiceScopeFactory scopeFactory) : IDatabaseMigrator
{
    public async Task MigrateDatabaseAsync(
        string tenantId,
        string connectionString,
        TimeSpan? cachingExpiration = null)
    {
        using var scope = scopeFactory.CreateScope();
        
        var tenantConnectionFactory = scope.ServiceProvider
            .GetRequiredService<ITenantConnectionFactory>();
            
        tenantConnectionFactory.SetConnectionString(tenantId, connectionString, cachingExpiration);

        using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();

        await DatabaseSeedService.SeedAsync(dbContext);
    }
}
```

> NOTE: here I am seeding the database just for the testing purpose.

I have