```yaml
---
title: Complex Types in Entity Framework 8
source: https://okyrylchuk.dev/blog/complex-types-in-entity-framework-8/
date_published: 2024-01-12T10:32:32.000Z
date_captured: 2025-08-11T16:13:55.325Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: general
technologies: [Entity Framework 8, Entity Framework Core 2.0, .NET, GitHub, Cosmos DB]
programming_languages: [C#]
tags: [entity-framework, ef-core, complex-types, owned-types, data-modeling, dotnet, orm, value-objects]
key_concepts: [complex-types, owned-types, value-objects, data-modeling, immutability, shadow-properties]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores Complex Types in Entity Framework 8, differentiating them from the previously introduced Owned Types in EF Core 2.0. It explains that while Owned Types are suitable for Value Objects with a one-to-one relationship and implicitly manage a hidden primary key, they cannot be shared across multiple owner entities. Complex Types, however, overcome this limitation by not having a hidden key, allowing a single instance to be referenced by various entities. The author illustrates this distinction with practical C# code examples, demonstrating the error encountered when attempting to share an Owned Type and how Complex Types resolve it. The article concludes by listing current limitations of Complex Types and relevant GitHub issues for future enhancements.
---
```

# Complex Types in Entity Framework 8

# Complex Types in Entity Framework 8

By Oleg Kyrylchuk / January 12, 2024

## Owned Types

To understand what are **Complex Types**, we need to recall about **Owned Types**.

The **Owned Types** have been released with **Entity Framework Core 2.0**. This feature allows you to model entity types that can only ever appear on navigation properties of the entity types (owners). You cannot create a **DbSet<T>** for Owned Types.

The **Owned Types** is a good choice for representing **Value Objects** in **DDD**. They have a one-to-one relationship with the owner. You don’t have to define a primary key for them. However, if you want a list of Owned Types, you need to define a primary key, and **EF Core** will create a one-to-many relationship.

But let’s return to the more straightforward example of a one-to-one relationship between owner and owned type. If you don’t add one, **EF Core** will add a primary key as a shadow property to track the owned types. And that’s why you cannot share instances of owned types by multiple owners.

Let’s see this limitation in the example. Assume we have a **Company** and **Office** entities. Both of them have **Addresses**. Even more, the **Office** entity has two addresses – **Headquarters** and **Location**. The **Company** has a list of **Offices**.

```csharp
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
}

public class Office
{
    public int Id { get; set; }
    public string Name { get; set; }

    public Address Location { get; set; }
    public Address Heardquarters { get; set; }
}

public class Company
{
    public int Id { get; set; }
    public string Name { get; set; }

    public Address Address { get; set; }
    public IList<Office> Offices { get; set; } = new List<Office>();
}
```

Let’s configure the **Address** entity as Owned Type of **Company** and **Office**.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder
        .Entity<Company>()
        .OwnsOne(p => p.Address);

    modelBuilder
        .Entity<Office>()
        .OwnsOne(p => p.Location);

    modelBuilder
        .Entity<Office>()
        .OwnsOne(p => p.Heardquarters);
}
```

Now, we want to create a company with a headquarters office. It’s the same address. So let’s do it.

```csharp
Address address = new()
{
    Street = "123 Main St",
    City = "Springfield",
    ZipCode = "12345"

};

Company company = new()
{
    Name = "Contoso",
    Address = address
};

company.Offices.Add(new Office
{
    Name = "HQ",
    Location = address,
    Heardquarters = address
});

context.Companies.Add(company);
await context.SaveChangesAsync();
```

This code won’t work. You’ll get the following error:

**System.InvalidOperationException: Cannot save instance of ‘Office.Location#Address’ because it is an owned entity without any reference to its owner. Owned entities can only be saved as part of an aggregate also including the owner entity.**

It happens because a single instance of the **Address** entity type (with the same hidden key value) is used for three different entity instances. This is a place where **Complex Types** come for help.

## Complex Types

The Complex Types are similar to Owned Types. They must be defined as part of the entity type.

**The key difference is that Complex Types have no hidden key value**.

Therefore, they can be shared by multiple entity instances.

Let’s fix our application with the Complex Types.

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    modelBuilder
        .Entity<Company>()
        .ComplexProperty(e => e.Address);

    modelBuilder.Entity<Office>(o =>
    {
        o.ComplexProperty(e => e.Location);
        o.ComplexProperty(e => e.Heardquarters);
    });
}
```

If you’re a fan of mapping attributes, you have a new **ComplexAttribute** attribute.

```csharp
[ComplexType]
public class Address
{
    public string Street { get; set; }
    public string City { get; set; }
    public string ZipCode { get; set; }
}
```

Sharing the same **Complex Type** instances can cause an issue with its updating. Modifying it will generate the update for all places where it’s used. That’s why it’s recommended to make **Complex Type immutable**.

The **Complex Types** have limitations in this release. Some gaps will be closed in future releases. You can vote on the GitHub issues you want to see as improvements:

*   Support collections of complex types. ([Issue #31237](https://github.com/dotnet/efcore/issues/31237 "Issue #31237"))
*   Allow complex type properties to be null. ([Issue #31376](https://github.com/dotnet/efcore/issues/31376 "Issue #31376"))
*   Map complex type properties to JSON columns. ([Issue #31252](https://github.com/dotnet/efcore/issues/31252 "Issue #31252"))
*   Constructor injection for complex types. ([Issue #31621](https://github.com/dotnet/efcore/issues/31621 "Issue #31621"))
*   Add seed data support for complex types. ([Issue #31254](https://github.com/dotnet/efcore/issues/31254 "Issue #31254"))
*   Map complex type properties for the Cosmos provider. ([Issue #31253](https://github.com/dotnet/efcore/issues/31253 "Issue #31253"))
*   Implement complex types for the in-memory database. ([Issue #31464](https://github.com/dotnet/efcore/issues/31464 "Issue #31464"))

---

## Related Posts

### Logging and Diagnostics in Entity Framework

![Thumbnail for an article titled 'Logging and Diagnostics in Entity Framework'. The image features a dark blue background with abstract light blue wave patterns. The title is prominently displayed in large yellow and white text.](https://okyrylchuk.dev/wp-content/uploads/2024/02/featured7.png.webp)

### Single vs. Split Query in Entity Framework

![Partial thumbnail for an article titled 'Single vs. Split Query in Entity Framework'. The image shows a dark blue background with a light blue abstract wave pattern in the top left corner and large yellow text that reads 'Single Query'.](https://okyrylchuk.dev/wp-content/uploads/2024/03/featured10.png.webp)