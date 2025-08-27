```yaml
---
title: Useful features in Entity Framework Core 8 for your application
source: https://dateo-software.de/blog/entity-framework-8?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=unit-testing-clean-architecture-use-cases&_bhlid=0e596b56af9678ea5c0f19c93dbc4bdf9fa24007
date_published: unknown
date_captured: 2025-08-12T18:13:48.132Z
domain: dateo-software.de
author: Unknown
category: architecture
technologies: [Entity Framework Core 8, .NET, SQLite, GitHub, Statamic, JSON]
programming_languages: [C#, SQL, JavaScript]
tags: [entity-framework-core, ef-core-8, .net, database, orm, data-access, csharp, sqlite, json, new-features]
key_concepts: [primitive-collection-mapping, complex-types, json-column-mapping, sentinel-values, in-memory-database, owned-types, shadow-properties, database-defaults]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores several useful new features introduced in Entity Framework Core 8, aiming to highlight improvements relevant to many developers. It delves into primitive collection mapping, which simplifies storing collections of basic types, and the new concept of complex types for keyless structured object mapping. The author also discusses enhancements to JSON column mapping and the configuration of sentinel values for database defaults. Code examples in C# and SQL are provided, often demonstrated using an in-memory SQLite database, to illustrate the practical application of these features.]
---
```

# Useful features in Entity Framework Core 8 for your application

# dateo. Coding Blog

Coding, Tech and Developers Blog

## Useful features in Entity Framework Core 8 for your application

_Dennis Frühauff_ on April 9th, 2024

Entity Framework has been around for 16 (!) years now. And while many of us are using it actively, not everyone is fortunate enough to be able to update with every new release. The latest version, Entity Framework Core 8, has been released. Let me introduce some of the features that I consider to be "generally useful".

### Introduction

With Microsoft now releasing new major versions every year, it has become difficult to keep track of every new and shiny feature. Especially if you are not able to upgrade that quickly in your corporate domain or circumstances, it is easy to get lost in the different minor and major improvements. The dedicated overview pages in Microsoft's [documentation pages](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew) give you everything, but they can take a while to digest.
So I thought I could present a selected set of features in today's article which I think might actually be useful for more than a handful of people.

As often, you can find the examples in this article demonstrated through unit tests in this [GitHub repository](https://github.com/FoxDawg/Sandbox.NewInEfCore8).

#### Bonus tip:

The tests in the repository use an in-memory SQLite database since that does indeed support most of the new features in EF Core 8. If you ever wondered, how you can create a shared instance of an in-memory database (i.e., can be shared across instances of `DbContext`) that is still _unique_ to a specific test, it works like this:

```csharp
var uniqueIdentifier = "myCustomId";
optionsBuilder.UseSqlite($"datasource=file:{uniqueIdentifier}?mode=memory&cache=shared");
```

It is one of the things I seem to forget again and again and come around looking it up in past projects. So I am glad that I am writing it now down for public access.

### Support of primitive collection mapping

Entity Framework Core 8 introduced the support of [primitive collection mapping](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#primitive-collections). This is something that could have been achieved before via custom code - now it comes out of the box.

You can find the demos for this feature [here](https://github.com/FoxDawg/Sandbox.NewInEfCore8/tree/main/Demos/PrimitiveCollections).

What is a primitive collection?
It is a collection of values that are of primitive type to the database providers, so that usually means numbers and strings, but also enumerations (as, e.g., integers).

For example, the following properties can be easily mapped now:

```csharp
public List<string> Tags { get; }
public List<BreakfastOptions> BreakfastOptions { get; }

public enum BreakfastOptions
{
    Toast,
    Tea,
    Juice
}
```

with their corresponding entity configurations

```csharp
builder.Property(o => o.Tags);
builder.Property(o => o.BreakfastOptions);
```

If we look at the database command that is issued in the `Tags` example, it looks like this:

```sql
CREATE TABLE "Tweets" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Tweets" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Tags" TEXT NOT NULL
);
```

This simply means that Entity Framework will store the collection of values in (in this case) a single string and parse them back into the list of objects that you would expect when returning them from the database, which is pretty neat.

As I mentioned, having this done via custom code can easily be done, but having this as a built-in convenience feature can really come in handy.
This will also work for types like `uint`, `bool`, `DateOnly`, `DateTime`, `Uri`
...
wait... `Uri`? Yes - this feature will work for both database-primitive types as well as types for which Entity Frameworks has built-in converters!

All in all, I consider the native collection support to be something that can be really useful in abstracting the database layer from your application code.

### Complex types

When talking about what types of objects are stored and mapped to relational databases with Entity Framework, we can roughly identify three different categories:

*   Single value types, like `int`, `string`, but also `Guid`, `DateTime`, so types with a primitively supported mapping.
*   Structured, multi-valued objects uniquely defined via a key, so-called \_entity types.
*   Structured, multi-valued objects that are _not_ defined by a key.

Only since Entity Framework Core 8, there is a good way of mapping the third category of objects between application and database. While there already was support for [owned types](https://learn.microsoft.com/en-us/ef/core/modeling/owned-entities), this type of mapping still requires a key to be defined on the owned type. And even if you did not define a key on the owned entity, EF Core will still create one known as a [shadow property](https://learn.microsoft.com/en-us/ef/core/modeling/shadow-properties). In that sense, the new _complex types_ offer a more loosely coupled between different entities.

The demos for this feature can be found [here](https://github.com/FoxDawg/Sandbox.NewInEfCore8/tree/main/Demos/ComplexTypes).

Consider a simple example of a model like the following:

```csharp
public class Price
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Price(decimal amount, Currency currency)
    {
        Amount = amount;
        Currency = currency;
    }
}

public class Product
{
    public int Id { get; }
    public string Name { get; }

    public required Price Price { get; init; }
  
    public Product(string name)
    {
        Name = name;
    }
}
```

The mapping configuration for the `Product` entity can look like this:

```csharp
public void Configure(EntityTypeBuilder<Product> builder)
{
    builder.ToTable("Products");
    builder.HasKey(p => p.Id);
    builder.Property(p => p.Id).IsRequired().ValueGeneratedOnAdd();
    builder.Property(p => p.Name).IsRequired().HasMaxLength(20);
    builder.ComplexProperty(p => p.Price);
}
```

If we take a look at the database that is created from this kind of configuration, we will see the following SQL statement:

```sql
CREATE TABLE "Products" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Products" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Price_Amount" TEXT NOT NULL,
    "Price_Currency" INTEGER NOT NULL
);
```

As we can see, EF Core will automatically flatten the configured properties of the complex entity into an equal amount of columns.

> Note: It is currently not possible to map complex types to a different table.
> 
>   

Now, a very important difference between owned types and complex types is the fact that one could not _share_ the same owned entity across different owning entities. So for example, creating an instance of `Price` and assigning it to two different products would result in an error when saving those changes to the database. This is due to the fact that owned types have stronger ownership semantics.
In contrast to this, sharing instances of entities that are mapped as complex types is allowed. This, however, can also lead to problems.

The following example is directly taken from one of the demo samples that are provided [here](https://github.com/FoxDawg/Sandbox.NewInEfCore8/blob/b1f707fe9b0f7d8f2a1e7580af826c5a0b2178ba/Demos/ComplexTypes/ComplexTypeDemos.cs#L20). Consider a product that carries two prices that we initially assigned with the same instance of that price. Now, when changing the value of only one of them, the resulting SQL statement will actually change more than what was intended:

```sql
SELECT "m"."Id", "m"."Name", "m"."RegularPrice_Amount", "m"."RetailPrice_Amount"
FROM "MutablePriceProducts" AS "m"
LIMIT 1
UPDATE "MutablePriceProducts" SET "RegularPrice_Amount" = @p0, "RetailPrice_Amount" = @p1
WHERE "Id" = @p2
RETURNING 1;
```

This mutability issue can obviously easily be circumvented by making the underlying `Price` entity immutable. In fact, this is one of the rare occasions where it can be quite useful to model the complex entities as C# `record`, thereby using their syntactic sugar for the best.

That aside, the addition of complex types is in my opinion a great addition to make your entities cleaner and more readable.

### JSON columns

One other feature that goes in a similar direction is the enhanced mapping for JSON column types. JSON column mapping has already been a part of EF Core 7, so it is not "new", but I thought I'd still give you a quick demo because it can be quite useful on occasion.

One point of advice though: Having a column on your table that supports JSON can quickly lead to a dumping ground of properties. When developers are forced to do things quickly, they might use the path of less resistance. Adding just one additional property to the already mapped JSON entity is not harmful after all, is it? Please be aware that this might happen to your entities as well.

Let's assume we are building some kind of authorization-related code, with custom roles and policies:

```csharp
public class Role
{
    public int Id { get; }
    public string Name { get; }

    public Role(string name, IEnumerable<CustomPolicy> policies)
    {
        Name = name;
        this.Policies = policies.ToList();
    }

    public IList<CustomPolicy> Policies { get; } = new List<CustomPolicy>();
}

public class CustomPolicy
{
    public string Key { get; }
    public bool Value { get; }

    public CustomPolicy(string key, bool value)
    {
        Key = key;
        Value = value;
    }
}
```

To map these entities to a single table and the `CustomPolicies` being mapped to a single column, the configuration will look like this:

```csharp
public void Configure(EntityTypeBuilder<Role> builder)
{
    builder.ToTable("Roles");
    builder.HasKey(o => o.Id);
    builder.Property(o => o.Id).IsRequired().ValueGeneratedOnAdd();
    builder.Property(o => o.Name).IsRequired().HasMaxLength(25);
    builder.OwnsMany(o => o.Policies, b =>
    {
        b.Property(p => p.Key);
        b.Property(p => p.Value);
        b.ToJson();
    });
}
```

On database creation, the SQL statement to create this table will simply be:

```sql
CREATE TABLE "Roles" (
    "Id" INTEGER NOT NULL CONSTRAINT "PK_Roles" PRIMARY KEY AUTOINCREMENT,
    "Name" TEXT NOT NULL,
    "Policies" TEXT NULL
);
```

With that in place, any number of policies will automatically be mapped to a JSON string. Again, this has been around since EF Core 7 now, so what's new?

You can read the full documentation [here](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#enhancements-to-json-column-mapping). In essence, there are improvements in:

*   Querying JSON entities that are arrays (like above).
*   Extension of JSON support to SQLite (which is why the [demo](https://github.com/FoxDawg/Sandbox.NewInEfCore8/blob/main/Demos/JsonColumns/JsonColumnsDemos.cs) works in the first place).

### Sentinel values

EF Core's new feature centered around [sentinel value configuration](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew#sentinel-values-and-database-defaults) is an interesting addition to a problem, that we usually find an easy workaround for. But let's start at the beginning.

> Sentinel value? The sentinel value is the value, that issues EF Core to use the default value by configuration for the property that needs to be set.
> 
>   

Sounds complicated? Let's have an example from my [demos](https://github.com/FoxDawg/Sandbox.NewInEfCore8/blob/main/Demos/SentinelValues/SentinelValuesDemos.cs):

Assume that we are modeling a `ToDoItem` entity like this:

```csharp
public class ToDoItem
{
    public int Id { get; }
    public string Name { get; }
    public int Priority { get; init; }

    public ToDoItem(string name)
    {
        Name = name;
    }
}
```

For business reasons the default priority needs to be `100`, so we might configure the entity like this:

```csharp
public void Configure(EntityTypeBuilder<ToDoItem> builder)
{
    builder.ToTable("ToDoItems");
    builder.HasKey(o => o.Id);
    builder.Property(o => o.Id).IsRequired().ValueGeneratedOnAdd();
    builder.Property(o => o.Name).IsRequired().HasMaxLength(100);
    builder.Property(o => o.Priority).HasDefaultValueSql(100);
}
```

Now, since the `Priority` property is non-nullable, and the default for an `int` is `0`, we will not have any means to save an instance of `ToDoItem` with the value set to `0`. If we were to do that, EF Core would identify `0` as the default for this type and just assume that the property is _unset_ and we would end up with `100` anyway.

A possible solution to this problem is obvious right away: Making the property nullable so that the default is `null` will solve the problem. But it will also force us to handle that `Priority` could _actually_ be null sometimes. In that sense, database or modeling design decisions will bleed into our domain logic, which is not very convenient.

With EF Core 8, Microsoft gives us the opportunity to actually _configure_ the sentinel value, if we want to. In other words, we can now configure the value that tells EF Core that it should use the configured database default:

```csharp
public void Configure(EntityTypeBuilder<ToDoItem> builder)
{
    builder.ToTable("ToDoItems");
    builder.HasKey(o => o.Id);
    builder.Property(o => o.Id).IsRequired().ValueGeneratedOnAdd();
    builder.Property(o => o.Name).IsRequired().HasMaxLength(100);
    builder.Property(o => o.Priority).HasSentinel(-1).HasDefaultValueSql(100);
}
```

This configuration has only slightly changed. In this example, I am making sure that not `0` is the indicator for triggering the default but rather `-1`, which is a value that I do not want to store anyway. This way, storing an instance with a priority of `0` actually becomes possible, without a workaround in the actual entity model.

Although this is only a minor change, I feel that this feature is a very convenient addition to the feature set, potentially saving a few lines of code and null checks on our side of the fence.

### Conclusion

In this article, we took a quick look at some of the new features and enhancements that were released with Entity Framework 8. You can read the full list in Microsoft's [release notes](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-8.0/whatsnew) and also take a look at what might be in store for [EF Core 9](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/plan).

Also, feel free to take a look at the samples I provided in this [GitHub repository](https://github.com/FoxDawg/Sandbox.NewInEfCore8) to go along with this article.

Please share on social media, stay in touch via the contact form, and subscribe to our post newsletter!

© 2025 dateo. Coding Blog – Powered by [Statamic](https://statamic.com?ref=stumblr). Production. Commit: 4f95239