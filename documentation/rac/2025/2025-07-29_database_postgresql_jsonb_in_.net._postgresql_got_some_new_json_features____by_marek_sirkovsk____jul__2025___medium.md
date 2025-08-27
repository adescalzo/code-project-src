```yaml
---
title: "PostgreSQL JSONB in .NET. PostgreSQL got some new JSON features… | by Marek Sirkovský | Jul, 2025 | Medium"
source: https://mareks-082.medium.com/postgresql-jsonb-in-net-25fbcc7b64b2
date_published: 2025-07-29T05:08:53.367Z
date_captured: 2025-08-08T13:42:40.574Z
domain: mareks-082.medium.com
author: Marek Sirkovský
category: database
technologies: [PostgreSQL, .NET, MongoDB, MS SQL, Dapper, Entity Framework Core, Npgsql, System.Text.Json]
programming_languages: [C#, SQL]
tags: [postgresql, jsonb, dotnet, entity-framework-core, dapper, data-access, semi-structured-data, orm, relational-database, nosql]
key_concepts: [JSONB, semi-structured-data, polymorphic-data-models, evolving-schemas, GIN-index, TOAST, ORM, micro-orm]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explores the integration of PostgreSQL's JSONB data type with .NET applications, focusing on its use with Entity Framework Core and Dapper. It explains the advantages of JSONB over the standard JSON type, highlighting its binary storage, efficient indexing with GIN, and suitability for semi-structured data. The author demonstrates various mapping strategies in EF Core, including owned entities and `JsonDocument`, and discusses Dapper's more manual approach to JSONB serialization. The piece also delves into advanced topics like polymorphic JSON mapping, materialized views for denormalization, and the internal workings of JSONB, including TOAST storage. It concludes by advocating for JSONB as a powerful bridge between relational and document-oriented data models in modern applications.]
---
```

# PostgreSQL JSONB in .NET. PostgreSQL got some new JSON features… | by Marek Sirkovský | Jul, 2025 | Medium

# PostgreSQL JSONB in .NET

![Marek Sirkovský](https://miro.medium.com/v2/resize:fill:64:64/1*sd6GN4VkST6HdJW7e5xMKQ.jpeg)
*Author's profile picture.*

Marek Sirkovský

Following

8 min read

·

Jul 22, 2025

12

2

Listen

Share

More

I remember the first time I read about storing JSON data in a database. It was while reading a printed magazine(!) that I was introduced to the concept of NoSQL databases. I thought to myself, “What a weird idea. Why would anyone choose to store data in a format that isn’t a table?” Despite my initial skepticism, I was curious to give it a try.

![Conceptual image showing a cloud icon connected to several nodes, alongside the text '.NET + {JSONB}', symbolizing the integration of .NET with PostgreSQL's JSONB.](https://miro.medium.com/v2/resize:fit:700/1*uxh8nTq-ugAQtUmRWef2eA.png)

The first NoSQL database I used was MongoDB, which stores data in BSON (Binary JSON). It was fun, and I liked the idea of being able to store data without a schema and the lightning-fast data retrieval. However, I lost interest after reading a comment from a developer who benchmarked MongoDB against PostgreSQL. He used a single table with no indexes or constraints and found that, in such a setup, PostgreSQL performed just as well as MongoDB.

Since then, I’ve occasionally worked with JSON data in relational databases (MS SQL and others) and found it worked well enough. But now that PostgreSQL offers even more support for JSON, it’s worth taking a fresh look to see how much more we can do with it.

# PostgreSQL and JSON

PostgreSQL introduced limited JSON support in 2014, two years **after** MongoDB’s release and two years **before** MS SQL did the same.

PostgreSQL offers two types: JSON and JSONB. I will focus only on JSONB, as JSON is very limited, and you should use it only if you want to keep white space in your JSON payload. Basically, the JSON type is, under the hood, just a text type with additional validation and a few not-so-performant operators. The real hero here is its counterpart, JSONB.

## JSONB

JSONB stores data in a binary format, which allows for efficient indexing and querying. It supports various operators and functions for manipulating JSON data, such as containment, existence checks, and path queries. JSONB also allows for indexing using GIN (Generalized Inverted Index) which enables fast searches within JSON documents.

The insignificant whitespace is removed during the parsing process, meaning that JSONB does not preserve formatting, such as indentation or line breaks. The advantage of this is that the JSONB data does not need to be parsed every time it is accessed, unlike the JSON type.

## Use cases

JSONB is primarily used for storing semi-structured data, handling polymorphic data models, and supporting evolving schemas. I’m referring to scenarios such as metadata, user preferences, product attributes, and event logs. Additionally, the JSON format aligns well with architectural patterns such as CQRS and Event Sourcing.

Now you might be thinking, *"Cool, but what does this have to do with .NET?"* Fair enough. Most of my day-to-day work has been with Microsoft SQL, like many .NET developers, but I wanted to see how my favorite tools, Dapper and Entity Framework, play with PostgreSQL’s JSONB.

# Entity Framework Core integration

Entity Framework Core (EF Core) supports JSONB natively through the [Npgsql provider](https://www.npgsql.org/). You can map JSONB columns to a .NET type, such as a class or a dictionary, and EF Core will automatically handle serialization and deserialization.

There are multiple ways to map the JSONB column to your entity framework model:

1.  As simple strings — the simplest way but you take care of serialization
2.  As EF-owned entities, the preferred way for complex objects
3.  JsonDocument or JsonElement
4.  As strongly-typed user-defined types (POCOs) (deprecated)
5.  Low-level parsing with `Utf8JsonReader`

The preferred way to work with JSONB in Entity Framework Core is to use EF-owned entities:

```csharp
public class UserSetting  
{  
  public string DisplayName { get; set; } = "Default User";  
  public NotificationSettings Notifications { get; set; = new();  
}  
public class NotificationSettings  
{  
  public bool EmailEnabled { get; set; } = true;  
  public bool SmsEnabled { get; set; } = false;  
  public bool PushEnabled { get; set; } = true;  
}  
  
// Mapping:  
// userProfile is a standard entity with a JSONB column for Settings  
userProfile.OwnsOne(e => e.Settings, settings =>  
{  
  settings.ToJson();  
  settings.OwnsOne(s => s.Notifications);  
});
```

Then, if you need to query by a specific property, you can utilize the following pattern:

```csharp
var profilesWithEmailEnabled = await context.UserProfiles  
  .Where(p => p.Settings.Notifications.EmailEnabled == true)  
  .ToArrayAsync();
```

Quick and hassle-free. As you can see, using EF-owned entities makes it easy to query even deeply nested properties within a JSONB column.

If your schema is more dynamic, it’s better to store the JSONB data as a string or a `JsonDocument`/`JsonElement`. This approach lets you query the JSONB content without needing to define a fixed schema.

```csharp
public class UserProfile  
{  
  public JsonDocument? Events { get; set; }  
}  
…  
  
//store it like this:  
var userProfile = new UserProfile  
{  
  Events = JsonDocument.Parse(  
    """  
       {  
         "lastLogin": "2025–01–03T08:45:30",   
         "passwordChanged": "2025–01–02T16:20:15"  
       }  
    """)  
};
```

**Note:** *One thing worth mentioning is that* `JsonDocument` *offers a read-only API. It means you can't modify its contents. If you need to retrieve and then update JSON data, you'll need to use a different approach. As of the time of writing this blog post, we’re still waiting for [support for the mutation API](https://github.com/npgsql/npgsql/issues/6052) (a.k.a. JsonNode).*

To query the `JsonDocument`, you can take advantage of `EF.Functions.JsonExists` or `EF.Functions.JsonContains`:

```csharp
var profilesWithLoggedEvents = await context.UserProfilesOwned.Where(  
   p => p.Events != null   
        && EF.Functions.JsonContains(p.Events, """{"lastLogin": "2025–01–03T08:45:30"}"""))  
  .ToArrayAsync();  
  
var profilesWithAnyLoggedEvent = await context.UserProfilesOwned.Where(  
  p => p.Events != null   
       && EF.Functions.JsonExists(p.Events, "$.logged"))  
  .ToArrayAsync();
```

The same `EF.Functions.*` applies to strongly-typed user-defined types(POCOs), but as I mentioned, this approach was deprecated.

The last but not least option is to use `Utf8JsonReader`:

```csharp
var sql = "SELECT \"Settings\" FROM \"UserPreference\"";  
await using var command = new NpgsqlCommand(sql, connection);  
await using var reader = await command.ExecuteReaderAsync();  
while (await reader.ReadAsync())  
{  
  var utf8Data = reader.GetFieldValue<byte[]>(0);  
  var jsonReader = new Utf8JsonReader(utf8Data);  
  while (jsonReader.Read())  
  {  
    switch (jsonReader.TokenType)  
    {  
      case JsonTokenType.PropertyName:  
        var propertyName = jsonReader.GetString();  
        reader.Read(); // Move to the value  
        switch (propertyName)  
        {  
          case "Theme":  
          //...
```

It can be slightly faster than standard serialization, particularly when you don’t need to access all the data in the JSON.

# Dapper integration

Dapper integration is straightforward. The easiest way is to have Dapper map the JSONB column to a string:

```csharp
using (var connection = new NpgsqlConnection(connectionString))  
{  
  var result = connection.Query("SELECT \"Settings\" FROM \"UserPreference\" WHERE id = @Id", new { Id = 1 });  
  foreach (var item in result)  
  {  
    // Settings is a string not an object  
    var jsonData = item.Settings;   
  }  
}
```

To store data:

```csharp
var userPreference = new UserPreference  
{  
   ...  
};  
// this is needed to serialize the UserSettings object to string  
var settingsJson = JsonSerializer.Serialize(settings);  
  
const string insertSQL =   
  """  
    INSERT INTO "UserPreference" ("Settings", "CreatedAt", "UpdatedAt")  
    VALUES (@UserPreference::jsonb, @CreatedAt, @UpdatedAt)  
    RETURNING "Id";  
    """;  
  
var parameters = new  
{  
    UserPreference = userPreference,  
    CreatedAt = DateTime.UtcNow,  
    UpdatedAt = (DateTime?)null  
};  
  
var newId = await connection.ExecuteScalarAsync<int>(insertSQL, parameters);
```

As far as I can tell, Dapper doesn’t provide built-in support for the JSONB type. You must either retrieve the JSON as a string and handle serialization manually or implement a custom [JSONB converter for your type](https://github.com/DapperLib/Dapper/issues/1734). Unlike EF Core, there’s currently no clean, native solution available.

# Polymorphic JSON mapping in EF

If you want to use polymorphic JSON mapping, you can use Npgsql’s JSONB support to map different types to a single JSONB column. This allows you to store different types of data in the same column and query them based on their type:

```csharp
// the model classes  
public abstract class PaymentMethod  
{  
  public string Id { get; set; } = Guid.NewGuid().ToString();  
  public abstract string Type { get; }  
  public bool IsDefault { get; set; }  
}  
  
public class BankAccount : PaymentMethod  
{  
  public override string Type => "BankAccount";  
  public string AccountNumber { get; set; } = string.Empty;  
  public string BankName { get; set; } = string.Empty;  
}  
  
public class DigitalWallet : PaymentMethod  
{  
  public override string Type => "DigitalWallet";  
  public string WalletType { get; set; } = string.Empty;  
  public string WalletNumber { get; set; } = string.Empty;  
}  
  
//in OnModelCreating:  
modelBuilder.Entity<UserPaymentProfile>(entity =>  
{        
  // Configure polymorphic JSON collection  
  entity.OwnsMany(e => e.PaymentMethods, builder =>  
  {  
    builder.ToJson();  
});
```

That seems similar to a standard [inheritance table in EF Core](https://learn.microsoft.com/en-us/ef/core/modeling/inheritance#table-per-hierarchy-and-discriminator-configuration). However, I didn’t manage to get the same smooth experience as with standard tables.

This query doesn’t work:

```csharp
var usersWithCreditCards = await context.UserPaymentProfiles  
  .Where(p => p.PaymentMethods.Any(pm => pm is CreditCard))  
  .ToArrayAsync();
```

Working fallback with `EF.Functions`:

```csharp
var usersWithCreditCards = await context.UserPaymentProfiles  
  .Where(p =>   
    EF.Functions.JsonExists(p.PaymentMethods, "$[*] ? (@.Type == \"CreditCard\")"))  
  .ToArrayAsync();
```

So, while EF Core *does* support polymorphic JSON, the experience isn’t as smooth as working with standard tables. The alternative is a manual approach: serialize the data as a string and use attributes like `[JsonDerivedType]` and `[JsonPolymorphic]`. You can find more details [here](https://learn.microsoft.com/en-us/dotnet/standard/serialization/system-text-json/polymorphism#polymorphic-type-discriminators).

# JSONB and (materialized) views

I grew up in a world of relational databases, where not normalizing your data was considered a *serious crime*. So whenever I dealt with JSON, my instinct was to make sure I could still treat it like a normalized table in case a client needed it in the future.

That’s where views come in handy. Using a standard PostgreSQL view, you can extract JSONB data and query it just like a regular table:

```sql
CREATE VIEW UserPreferenceTheme AS  
SELECT  
  id,  
  (data->'theme')->>'action' AS theme    
FROM  
  UserPreference;
```

A more performant alternative is to use a materialized view, though it requires manual or scheduled refreshes. This is where I miss the automatic refresh feature that MS SQL offers for materialized views.

*Note: PostgreSQL 17 introduced the [new `JSON_TABLE` function](https://neon.com/docs/functions/json_table#query-examples), making it even easier to create views from JSON data.*

# How does JSONB internally work?

Working with JSONB is fairly straightforward, as we’ve seen. Now, let’s take a closer look at how JSONB works internally. This section doesn’t involve .NET, so feel free to skip it if you’re already familiar with the internals of JSONB.

JSONB stores data in a binary format for efficient reads and indexing, but updates rewrite the entire value. Even for single-field changes. That makes it less efficient for frequent writes. The JSONB type is best suited for read-heavy use cases.

Even though JSONB is optimized for read-mostly scenarios, you can further improve its performance by using GIN (Generalized Inverted Index). It enables fast searches within JSON documents, allowing for efficient queries of specific keys or values. There are two flavors of JSONB GIN indexes, the default one and the specialized one. The default GIN index is used for general-purpose indexing of JSONB data, while the specialized GIN index is optimized for specific use cases, such as containment queries. GIN index is a bit out of the scope of this blog post, but you can learn more [here](https://www.postgresql.org/docs/current/datatype-json.html#JSON-INDEXING).

## Where is the data in JSONB stored?

One thing worth mentioning is that JSONB is a [TOASTable type](https://www.crunchydata.com/blog/postgres-toast-the-greatest-thing-since-sliced-bread). That means that PostgreSQL can store large JSONB documents efficiently by compressing and storing them in a separate TOAST (The Oversized-Attribute Storage Technique) table.

The default TOAST threshold is 2KB, so if your JSONB document, stored in the binary format, exceeds that size, PostgreSQL will automatically compress it and store it in the TOAST table.

You can check the size of a JSONB document using the `pg_column_size` function:

```sql
SELECT  
pg_column_size(your_column) AS size_bytes  
FROM your_table WHERE your_primary_key = ?;
```

If the value exceeds 2000 bytes, it’s stored in TOAST. Data stored in TOAST is usually not an issue unless you’re micro-optimizing for a specific case.

# Conclusion

If you’re using PostgreSQL and haven’t explored JSONB yet, I highly recommend giving it a try. It offers a powerful way to work with semi-structured data while still allowing for the use of this data as *normalized* tables. It’s nice to see how modern PostgreSQL offers a seamless bridge between relational and document-oriented data, enabling effective storage and powerful querying across both models.