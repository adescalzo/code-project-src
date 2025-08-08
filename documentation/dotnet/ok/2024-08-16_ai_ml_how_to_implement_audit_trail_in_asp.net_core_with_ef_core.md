```yaml
---
title: How to Implement Audit Trail in ASP.NET Core with EF Core
source: https://antondevtips.com/blog/how-to-implement-audit-trail-in-asp-net-core-with-ef-core
date_published: 2024-08-16T08:55:21.852Z
date_captured: 2025-08-06T17:28:04.732Z
domain: antondevtips.com
author: Anton Martyniuk
category: ai_ml
technologies: [ASP.NET Core, Entity Framework Core, .NET 8, EF 8, PostgreSQL, SQLite, NpgsqlDataSourceBuilder, IHttpContextAccessor]
programming_languages: [C#, SQL]
tags: [audit-trail, aspnet-core, ef-core, database, data-access, change-tracking, dotnet, web-development, compliance, logging]
key_concepts: [audit-trail, change-tracking, dependency-injection, json-columns, database-interception, entity-lifecycle, data-auditing, IAuditableEntity]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article demonstrates how to implement an automatic audit trail system in an ASP.NET Core application using Entity Framework Core. It covers creating an `IAuditableEntity` interface and a dedicated `AuditTrail` entity to store historical data changes. The implementation leverages EF Core's change tracking capabilities by overriding the `SaveChangesAsync` method in the `DbContext` to capture create, update, and delete operations. The solution also integrates a `CurrentSessionProvider` to identify the user performing changes and utilizes PostgreSQL's JSONB columns for efficient storage of old/new values and changed properties.]
---
```

# How to Implement Audit Trail in ASP.NET Core with EF Core

![Cover image for the article "How to Implement Audit Trail in ASP.NET Core with EF Core", featuring a "dev tips" logo and abstract purple shapes on a dark background.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fefcore%2Fcover_ef_audit_trail.png&w=3840&q=100)

# How to Implement Audit Trail in ASP.NET Core with EF Core

Aug 16, 2024

[Download source code](/source-code/how-to-implement-audit-trail-in-asp-net-core-with-ef-core)

6 min read

### Newsletter Sponsors

[EF Core is too slow?](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website) Discover how you can easily insert 14x faster (reducing saving time by 94%).  
Boost your performance with our method integrated within EF Core: Bulk Insert, update, delete, and merge.  
Join 5,000+ satisfied customers who have trusted our library since 2014.

[Learn more](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=website)

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

In modern web applications, tracking changes to data can be needed for monitoring, compliance, and debugging reasons. This process, known as creating **audit trails**, allows developers to see who made changes, when they were made, and what the changes were. **Audit trails** provide a historical record of changes made to data.

In this blog post, I will show how to implement an audit trail in an ASP.NET Core application using Entity Framework Core (EF Core).

[](#application-we-will-be-auditing)

## Application We Will Be Auditing

Today we will implement audit trails for the "Books" application that has the following entities:

*   Books
*   Authors
*   Users

I find it useful to include the following properties in all entities that need to be audited:

```csharp
public interface IAuditableEntity
{
    DateTime CreatedAtUtc { get; set; }
    
    DateTime? UpdatedAtUtc { get; set; }
    
    string CreatedBy { get; set; }

    string? UpdatedBy { get; set; }
}
```

We need to inherit all our auditable entities from this interface, for example, User and Book:

```csharp
public class User : IAuditableEntity
{
    public Guid Id { get; set; }
    public required string Email { get; set; }
    
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = null!;
    public string? UpdatedBy { get; set; }
}

public class Book : IAuditableEntity
{
    public required Guid Id { get; set; }
    public required string Title { get; set; }
    public required int Year { get; set; }
    public Guid AuthorId { get; set; }
    public Author Author { get; set; } = null!;
    
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? UpdatedAtUtc { get; set; }
    public string CreatedBy { get; set; } = null!;
    public string? UpdatedBy { get; set; }
}
```

Now we have a few options, we can implement **audit trails** manually for each entity or have one implementation that automatically applies to all the entities. In this blog post, I will show you the second option, as it is more robust and easier to maintain.

[](#configuring-audit-trails-entity-in-ef-core)

## Configuring Audit Trails Entity in EF Core

The first step in implementing an **audit trail** is to create an entity that will store the audit logs in a separate database table. This entity should capture details such as the entity type, primary key, a list of changed properties, old values, new values, and the timestamp of the change.

```csharp
public class AuditTrail
{
    public required Guid Id { get; set; }

    public Guid? UserId { get; set; }

    public User? User { get; set; }

    public TrailType TrailType { get; set; }

    public DateTime DateUtc { get; set; }

    public required string EntityName { get; set; }

    public string? PrimaryKey { get; set; }

    public Dictionary<string, object?> OldValues { get; set; } = [];

    public Dictionary<string, object?> NewValues { get; set; } = [];

    public List<string> ChangedColumns { get; set; } = [];
}
```

Here we have a reference to a `User` entity. Depending on your application needs, you may have this reference or not.

Every audit trail can be of the following types:

*   Entity was created
*   Entity was updated
*   Entity was deleted

```csharp
public enum TrailType : byte
{
    None = 0,
    Create = 1,
    Update = 2,
    Delete = 3
}
```

Let's have a look at how to configure an audit trail entity in EF Core:

```csharp
public class AuditTrailConfiguration : IEntityTypeConfiguration<AuditTrail>
{
    public void Configure(EntityTypeBuilder<AuditTrail> builder)
    {
        builder.ToTable("audit_trails");
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => e.EntityName);

        builder.Property(e => e.Id);

        builder.Property(e => e.UserId);
        builder.Property(e => e.EntityName).HasMaxLength(100).IsRequired();
        builder.Property(e => e.DateUtc).IsRequired();
        builder.Property(e => e.PrimaryKey).HasMaxLength(100);

        builder.Property(e => e.TrailType).HasConversion<string>();

        builder.Property(e => e.ChangedColumns).HasColumnType("jsonb");
        builder.Property(e => e.OldValues).HasColumnType("jsonb");
        builder.Property(e => e.NewValues).HasColumnType("jsonb");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
```

I like using **json columns** to express `ChangedColumns`, `OldValues`, and `NewValues`. In this blog post, in my code example, I use a **Postgres** database.

If you're using SQLite or another database that doesn't support json columns - you can use string types in your entity and create a EF Core Conversion that serializes an object to a string to save it in a database. When retrieving data from the database, this Conversion will deserialize a JSON string into a corresponding .NET type.

In **Postgres** database, when using NET 8 and EF 8 you need to `EnableDynamicJson` in order to be able to have a dynamic json in "jsonb" columns:

```csharp
var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
dataSourceBuilder.EnableDynamicJson();

builder.Services.AddDbContext<ApplicationDbContext>((provider, options) =>
{
    var interceptor = provider.GetRequiredService<AuditableInterceptor>();

    options.EnableSensitiveDataLogging()
        .UseNpgsql(dataSourceBuilder.Build(), npgsqlOptions =>
        {
            npgsqlOptions.MigrationsHistoryTable("__MyMigrationsHistory", "devtips_audit_trails");
        })
        .AddInterceptors(interceptor)
        .UseSnakeCaseNamingConvention();
});
```

[](#implementing-audit-trails-for-all-auditable-entities)

## Implementing Audit Trails for all Auditable Entities

We can implement an auditing in EF Core DbContext that will automatically be applied to all entities that inherit from `IAuditableEntity`. But first we need to get a user that is performing create, update or delete actions on these entities.

Let's define a `CurrentSessionProvider` that will retrieve current user identifier from the `ClaimsPrinciple` of a current `HttpRequest`:

```csharp
public interface ICurrentSessionProvider
{
    Guid? GetUserId();
}

public class CurrentSessionProvider : ICurrentSessionProvider
{
    private readonly Guid? _currentUserId;

    public CurrentSessionProvider(IHttpContextAccessor accessor)
    {
        var userId = accessor.HttpContext?.User.FindFirstValue("userid");
        if (userId is null)
        {
            return;
        }

        _currentUserId = Guid.TryParse(userId, out var guid) ? guid : null;
    }

    public Guid? GetUserId() => _currentUserId;
}
```

You need to register the provider and `IHttpContextAccessor` in the DI:

```csharp
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentSessionProvider, CurrentSessionProvider>();
```

To create the audit trails, we can use EF Core [Changer Tracker](https://antondevtips.com/blog/understanding-change-tracking-for-better-performance-in-ef-core) capabilities to get entities that are created, updated or deleted.

We need to inject `ICurrentSessionProvider` into DbContext and override `SaveChangesAsync` method to create audit trails.

```csharp
public class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    ICurrentSessionProvider currentSessionProvider)
    : DbContext(options)
{
    public ICurrentSessionProvider CurrentSessionProvider => currentSessionProvider;
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        var userId = CurrentSessionProvider.GetUserId();
        
        SetAuditableProperties(userId);

        var auditEntries = HandleAuditingBeforeSaveChanges(userId).ToList();
        if (auditEntries.Count > 0)
        {
            await AuditTrails.AddRangeAsync(auditEntries, cancellationToken);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
```

Note, that we are creating `AuditTrails` before calling `base.SaveChangesAsync` to make sure that we persist all changes to the database in a single transaction.

In the code above we are performing two operations:

*   setting auditable properties to the created, updated or deleted records
*   creating audit trail records

For all entities that inherit from `IAuditableEntity` we set `Created` and `Updated` fields. In some cases changes might not be triggered by a user, but rather a code. In such cases we set that a "system" performed changes.

For example, this can be a background job, database seeding, etc.

```csharp
private void SetAuditableProperties(Guid? userId)
{
    const string systemSource = "system";
    foreach (var entry in ChangeTracker.Entries<IAuditableEntity>())
    {
        switch (entry.State)
        {
            case EntityState.Added:
                entry.Entity.CreatedAtUtc = DateTime.UtcNow;
                entry.Entity.CreatedBy = userId?.ToString() ?? systemSource;
                break;

            case EntityState.Modified:
                entry.Entity.UpdatedAtUtc = DateTime.UtcNow;
                entry.Entity.UpdatedBy = userId?.ToString() ?? systemSource;
                break;
        }
    }
}
```

Now let's have a look at how to create audit trail records. Again we're iterating through `IAuditableEntity` entities and select those that were created, updated or deleted:

```csharp
private List<AuditTrail> HandleAuditingBeforeSaveChanges(Guid? userId)
{
    var auditableEntries = ChangeTracker.Entries<IAuditableEntity>()
        .Where(x => x.State is EntityState.Added or EntityState.Deleted or EntityState.Modified)
        .Select(x => CreateTrailEntry(userId, x))
        .ToList();

    return auditableEntries;
}

private static AuditTrail CreateTrailEntry(Guid? userId, EntityEntry<IAuditableEntity> entry)
{
    var trailEntry = new AuditTrail
    {
        Id = Guid.NewGuid(),
        EntityName = entry.Entity.GetType().Name,
        UserId = userId,
        DateUtc = DateTime.UtcNow
    };

    SetAuditTrailPropertyValues(entry, trailEntry);
    SetAuditTrailNavigationValues(entry, trailEntry);
    SetAuditTrailReferenceValues(entry, trailEntry);

    return trailEntry;
}
```

An audit trail record can contain the following types of properties:

*   plain properties (like Book's Title or Year of Publication)
*   reference property (like Book's Author)
*   navigation property (like Author's Books)

Let's have a look at how to add plain properties to audit trails:

```csharp
private static void SetAuditTrailPropertyValues(EntityEntry entry, AuditTrail trailEntry)
{
    // Skip temp fields (that will be assigned automatically by ef core engine, for example: when inserting an entity
    foreach (var property in entry.Properties.Where(x => !x.IsTemporary))
    {
        if (property.Metadata.IsPrimaryKey())
        {
            trailEntry.PrimaryKey = property.CurrentValue?.ToString();
            continue;
        }

        // Filter properties that should not appear in the audit list
        if (property.Metadata.Name.Equals("PasswordHash"))
        {
            continue;
        }

        SetAuditTrailPropertyValue(entry, trailEntry, property);
    }
}

private static void SetAuditTrailPropertyValue(EntityEntry entry, AuditTrail trailEntry, PropertyEntry property)
{
    var propertyName = property.Metadata.Name;

    switch (entry.State)
    {
        case EntityState.Added:
            trailEntry.TrailType = TrailType.Create;
            trailEntry.NewValues[propertyName] = property.CurrentValue;

            break;

        case EntityState.Deleted:
            trailEntry.TrailType = TrailType.Delete;
            trailEntry.OldValues[propertyName] = property.OriginalValue;

            break;

        case EntityState.Modified:
            if (property.IsModified && (property.OriginalValue is null || !property.OriginalValue.Equals(property.CurrentValue)))
            {
                trailEntry.ChangedColumns.Add(propertyName);
                trailEntry.TrailType = TrailType.Update;
                trailEntry.OldValues[propertyName] = property.OriginalValue;
                trailEntry.NewValues[propertyName] = property.CurrentValue;
            }

            break;
    }

    if (trailEntry.ChangedColumns.Count > 0)
    {
        trailEntry.TrailType = TrailType.Update;
    }
}
```

If you need to exclude any sensitive fields, you can do it here. For example, we are excluding `PasswordHash` property from audit trails.

Now let's explore how to add reference and navigation properties into audit trails:

```csharp
private static void SetAuditTrailReferenceValues(EntityEntry entry, AuditTrail trailEntry)
{
    foreach (var reference in entry.References.Where(x => x.IsModified))
    {
        var referenceName = reference.EntityEntry.Entity.GetType().Name;
        trailEntry.ChangedColumns.Add(referenceName);
    }
}

private static void SetAuditTrailNavigationValues(EntityEntry entry, AuditTrail trailEntry)
{
    foreach (var navigation in entry.Navigations.Where(x => x.Metadata.IsCollection && x.IsModified))
    {
        if (navigation.CurrentValue is not IEnumerable<object> enumerable)
        {
            continue;
        }

        var collection = enumerable.ToList();
        if (collection.Count == 0)
        {
            continue;
        }

        var navigationName = collection.First().GetType().Name;
        trailEntry.ChangedColumns.Add(navigationName);
    }
}
```

Finally, we can run our application to see auditing in action.

Here is an example of auditing properties set by a system and by a user in the `authors` table:

![Screenshot of a database table, likely 'authors', showing columns like 'id', 'name', 'created_at_utc', 'updated_at_utc', 'created_by', and 'updated_by'. Highlighted sections show 'created_by' as 'system' for initial entries and 'updated_by' with a GUID for user-triggered updates, demonstrating how audit properties are populated.](https://antondevtips.com/media/code_screenshots/efcore/audit-trails/img_1.png)

Here is how the `audit_trails` table looks like:

![Screenshot of the 'audit_trails' database table, displaying columns such as 'user_id', 'trail_type', 'date_utc', 'entity_name', 'primary_key', 'old_values', and 'new_values'. The entries show various 'trail_type' values like 'Create', 'Delete', and 'Update', along with the corresponding entity names and primary keys.](https://antondevtips.com/media/code_screenshots/efcore/audit-trails/img_2.png)

![Continuation screenshot of the 'audit_trails' database table, focusing on the 'old_values', 'new_values', and 'changed_columns' columns. This image illustrates the use of JSONB columns to store detailed changes, showing the specific properties that were modified and their original and new values.](https://antondevtips.com/media/code_screenshots/efcore/audit-trails/img_3.png)

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-implement-audit-trail-in-asp-net-core-with-ef-core)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-implement-audit-trail-in-asp-net-core-with-ef-core&title=How%20to%20Implement%20Audit%20Trail%20in%20ASP.NET%20Core%20with%20EF%20Core)[X](https://twitter.com/intent/tweet?text=How%20to%20Implement%20Audit%20Trail%20in%20ASP.NET%20Core%20with%20EF%20Core&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-implement-audit-trail-in-asp-net-core-with-ef-core)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fhow-to-implement-audit-trail-in-asp-net-core-with-ef-core)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.