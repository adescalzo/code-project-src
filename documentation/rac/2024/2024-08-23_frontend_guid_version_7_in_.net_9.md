```yaml
---
title: Guid Version 7 in .NET 9
source: https://okyrylchuk.dev/blog/guid-version7-in-dotnet-9/
date_published: 2024-08-23T15:15:00.000Z
date_captured: 2025-08-20T21:16:04.251Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: frontend
technologies: [.NET 9, .NET, Entity Framework Core, NuGet, SequentialGuid, Relational Databases]
programming_languages: [C#]
tags: [guid, uuid, dotnet, csharp, performance, database, index-fragmentation, sequential-ids, new-features, distributed-systems]
key_concepts: [GUID versions, UUID specification, sequential IDs, index fragmentation, distributed systems, timestamp-based GUIDs]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces Guid Version 7 in .NET 9, contrasting it with the traditional Guid Version 4. While Guid V4 is globally unique and suitable for distributed systems, its non-sequential nature can lead to significant index fragmentation in relational databases, degrading performance. Guid V7, based on timestamps and random components, generates sequential GUIDs, making it a more efficient choice for database primary keys. The article provides clear C# code examples demonstrating the creation and version checking of both GUID types, highlighting the performance benefits of the new sequential GUIDs.
---
```

# Guid Version 7 in .NET 9

# Guid Version 7 in .NET 9

![Author's avatar](https://okyrylchuk.dev/wp-content/litespeed/avatar/a85a89199a7d4486a29f67dc2fc371d0.jpg?ver=1755571532)By Oleg Kyrylchuk / August 23, 2024

## Guid V4

The Guid type has been in .NET from the very beginning. You can create a new Guid with the NewGuid() method.

```csharp
var guid = Guid.NewGuid();

Console.WriteLine(guid);
// ee9d5db0-33cb-4408-ba9c-71289a823ba5
```

The NewGuid() method generates the GUID of version 4 following the [UUID Version 4 specification](https://en.wikipedia.org/wiki/Universally_unique_identifier#Version_4_\(random\)) in RFC 9562.

GUIDs are used in distributed systems. They ensure an ID is unique across multiple databases, servers, or systems. GUIDs are globally unique, so they’re perfect for scenarios where data might be merged from different sources or entities that need to be uniquely identified across distributed systems.

However, GUID v4 is not sequential, which can lead to significant index fragmentation in relational databases. When new rows are inserted, they will likely be placed in random positions within the index, leading to frequent page splits and a fragmented index. This fragmentation can degrade performance over time.

Entity Framework Core can generate sequential GUIDs. However, it has [disadvantages](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.valuegeneration.sequentialguidvaluegenerator?view=efcore-8.0). There are also NuGet packages for generating sequential GUIDs, such as [SequentialGuid](https://github.com/buvinghausen/SequentialGuid).

## Guid V7

.NET 9 introduces a new GUID implementation based on [timestamp and random](https://en.wikipedia.org/wiki/Universally_unique_identifier#Version_7_\(timestamp_and_random\)).

You can create a Guid using the **CreateVersion7()** method.

```csharp
var guid7 = Guid.CreateVersion7();

Console.WriteLine(guid7);
// 01917ba0-a4b5-793b-a915-1caeceb5843e
```

Also, the Guid type has a new Version property, so you can check the version your GUID was created with.

Let’s compare the creation of five GUIDs of both versions.

```csharp
for (int i = 0; i < 5; i++)
{
    var guid = Guid.NewGuid();

    Console.WriteLine($"V{guid.Version}: {guid}");
}
Console.WriteLine();
for (int i = 0; i < 5; i++)
{
    var guid = Guid.CreateVersion7();

    Console.WriteLine($"V{guid.Version}: {guid}");
}
```

The output:

```
V4: 0557b321-abcf-4390-abee-4b8fbf93ff34
V4: 21a98165-af1e-477e-9dee-7eb9c79e6c77
V4: 7dbbf973-c55a-4917-87a5-95c16f356262
V4: b13892f2-334f-409a-b9de-d90dea21eed4
V4: 52dc44f7-76e0-4689-a5e6-1a0f1c5f37a3

V7: 01917bbe-d973-7beb-a813-106fcb4eff98
V7: 01917bbe-d973-703c-8365-b7596740ac82
V7: 01917bbe-d973-7234-a580-5f07730a3ad7
V7: 01917bbe-d973-7751-b8ba-bb73afab4a5d
V7: 01917bbe-d973-7d36-9be0-2e6317919153
```

You can see that GUID V7 is sequential, which makes it more suitable for relationship databases.

You can also pass the DateTimeOffset when creating a new Guide to control TimeStamp.

```csharp
Guid.CreateVersion7(TimeProvider.System.GetUtcNow());
```