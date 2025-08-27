```yaml
---
title: Seeding Data with EF Core 9
source: https://www.nikolatech.net/blogs/ef-core-seeding-asp-dotnet-core
date_published: 2025-08-14T00:00:00.000Z
date_captured: 2025-08-19T11:22:04.010Z
domain: www.nikolatech.net
author: Unknown
category: general
technologies: [EF Core 9, .NET 9, ASP.NET Core, PostgreSQL, Bogus, Entity Framework Extensions]
programming_languages: [C#, SQL]
tags: [ef-core, data-seeding, dotnet, database, orm, aspnet-core, testing, development, csharp, postgresql]
key_concepts: [data-seeding, ef-core-9-features, synchronous-seeding, asynchronous-seeding, use-seeding-method, use-async-seeding-method, ensure-created-async, test-data-generation]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the new data seeding capabilities in EF Core 9, specifically focusing on the `UseSeeding` and `UseAsyncSeeding` methods. It explains how these methods offer a flexible and straightforward way to insert predefined data during application setup, crucial for testing and consistent environments. The post provides C# code examples demonstrating how to configure these methods within `DbContext` setup, including generating fake data with the Bogus library. It also highlights the importance of matching seeding methods with `EnsureCreated()` or `EnsureCreatedAsync()` calls and advises on best practices for production environments.]
---
```

# Seeding Data with EF Core 9

![Banner for Seeding Data with EF Core 9](https://coekcx.github.io/BlogImages/banners/ef-core-seeding-asp-dotnet-core-banner.png)

#### Seeding Data with EF Core 9

###### 14 Aug 2025

###### 5 min

### Special Thanks to Our Sponsors:

![Logo for ZZZ Projects, promoting Entity Framework Extensions](https://zzzprojects.com/images/logo256X256.png)

[EF Core is too slow?](https://entityframework-extensions.net/?utm_source=nikolatech&utm_medium=nikolatech&utm_campaign=nikolatech) Discover how you can easily insert **14x faster** (reducing saving time by **94%**).

Boost your performance with our method within EF Core: Bulk Insert, update, delete and merge.

Thousands of satisfied customers have trusted our library **since 2014**.

👉 [Learn more](https://entityframework-extensions.net/?utm_source=nikolatech&utm_medium=nikolatech&utm_campaign=nikolatech)

Data seeding is the process of inserting predefined data during application setup or initialization.

It’s especially valuable for testing and for ensuring the essential tables and records our application needs are available from the start. This approach guarantees consistency across environments and speeds up development.

If you’re using EF Core, you’re in luck, it recently introduced yet another way to perform seeding and I love it!

## Data Seeding with EF Core

EF Core offers 4 ways to add seeding data:

*   Custom initialization logic
*   Model managed data
*   Manual migration
*   Data seeding through configuration options

In this post, we’ll focus solely on data seeding using configuration options, a new feature introduced in EF Core 9.

If you’re on an older version and don’t plan to upgrade soon, don’t worry, I'll be covering the other seeding methods shortly as well.

## Getting Started

For this example, we’ll use a simple .NET 9 API that performs CRUD operations on a Product entity:

```csharp
public sealed class Product
{
    public Guid Id { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ModifiedAt { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public decimal Price { get; set; }
}
```

## UseSeeding and UseSeedingAsync

EF Core 9 introduces two new methods to simplify data seeding:

*   **UseSeeding**
*   **UseAsyncSeeding**

Both serve the same purpose, the only difference is whether the seeding logic runs synchronously or asynchronously.

You can configure them wherever you set up your DbContext (e.g. Program.cs or extension methods):

```csharp
services.AddDbContext<AppDbContext>(options =>
    options
        .UseNpgsql(configuration.GetConnectionString("Postgres"))
        .UseAsyncSeeding(async (dbContext, _, cancellationToken) =>
        {
            if (!(await dbContext.Set<Product>().AnyAsync(cancellationToken)))
            {
                var products = GenerateProducts();

                dbContext.Set<Product>().AddRange(products);

                await dbContext.SaveChangesAsync(cancellationToken);
            }
        }));
```

To keep things tidy, I created the helper method that generates fake products using **Bogus**:

```csharp
private static IEnumerable<Product> GenerateProducts()
{
    var faker = new Faker<Product>()
        .RuleFor(x => x.Id, f => f.Random.Guid())
        .RuleFor(x => x.CreatedAt, f => f.Date.Past())
        .RuleFor(x => x.Name, f => f.Commerce.ProductName())
        .RuleFor(x => x.Description, f => f.Commerce.ProductDescription())
        .RuleFor(x => x.Price, f => f.Random.Decimal());

    return faker.Generate(10);
}
```

Bogus helps me generate realistic test data. If you want to learn more about Bogus, check my blog post on it: [Generate Realistic Fake Data in C#](https://www.nikolatech.net/blogs/bogus-ganerate-fake-data)

First, we check if the table already contains any records, preventing duplicate seeding.

If no records exist, we generate a list of products and insert them.

Notice how flexible this approach is, this approach definitely gives you flexibility covering more complex seeding scenarios as well.

The primary key is generated here by Bogus, but you could also let the database generate it if you prefer.

## Triggering Seeding

To trigger the seeding logic we’ll use EnsureCreatedAsync() in the app:

```csharp
await using var scope = builder.ApplicationServices.CreateAsyncScope();

await using var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

await dbContext.Database.EnsureCreatedAsync();
```

It's important to note that EnsureCreated() triggers the synchronous seeding method (UseSeeding).

EnsureCreatedAsync() triggers the asynchronous seeding method (UseAsyncSeeding).

Mixing synchronous and asynchronous methods will prevent your seeding logic from running, so be careful to use the matching pair.

Additionally, for production environments, it’s highly recommended to separate seeding into its own application or container rather than running it inside your main app.

This approach helps avoid concurrency issues and potential deadlocks when multiple instances start simultaneously.

## Conclusion

With EF Core 9’s new UseSeeding and UseAsyncSeeding methods, seeding has become more flexible and straightforward than ever.

You can write normal C# code to seed your data asynchronously or synchronously, query the database during seeding, and even integrate complex logic if needed.

This approach is by far my favorite way of seeding data.

Just remember to match your seeding method with the corresponding EnsureCreated() or EnsureCreatedAsync() call.

If you want to check out examples I created, you can find the source code here:

[Source Code](https://www.nikolatech.net/codes/simple-api-dotnet9-example)