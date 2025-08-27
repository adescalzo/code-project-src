```yaml
---
title: Importing JSON Files Into A C# Project
source: https://barretblake.dev/posts/development/2025/06/importing-json-files-into-csharp/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2088
date_published: 2025-06-30T00:00:00.000Z
date_captured: 2025-08-17T22:12:05.195Z
domain: barretblake.dev
author: Barret
category: ai_ml
technologies: [ASP.NET Core, .NET, Entity Framework Core, PostgreSQL, Aspire, System.Text.Json, Json2csharp.com, GitHub]
programming_languages: [C#, SQL, JSON]
tags: [dotnet, json, c-sharp, data-import, data-seeding, database, aspnet-core, entity-framework, serialization, deserialization]
key_concepts: [json-deserialization, data-seeding, database-migration, object-relational-mapping, file-io, data-modeling, project-structure]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a practical guide on importing JSON data files into a C# application to seed a database. It demonstrates how to leverage the Json2csharp.com tool to automatically generate C# classes that match the JSON schema, simplifying the deserialization process. The post also covers crucial project setup steps, such as configuring JSON files to be copied to the output directory, and illustrates how to use `System.Text.Json.JsonSerializer` to load and parse the data. Finally, it shows how to integrate the deserialized data into a database using an ORM, exemplified by Entity Framework Core, for efficient data management.
---
```

# Importing JSON Files Into A C# Project

# Importing JSON Files Into A C# Project

_Hero image credit: [Photo by Kevin Ku](https://www.pexels.com/photo/data-codes-through-eyeglasses-577585/)_

As part of this fun side project I’ve been working on, I’ve been playing around with data that comes in the form of JSON data files. What I want to do is use these data files to seed the data in my database. So here’s a quick one for anyone looking to import JSON data into a running C# application.

I showed in a [previous post](../posts/development/2025/06/efcore-migration-project-in-aspire/) how I set up my Aspire project to have a separate data migration project to handle the DbContext, schema migrations, and seeding for my Postgres database. While some of the basic seeding is hardcoded into C# classes, most of the data will be coming from various JSON files filled with the data. Let’s walk through how I set up the loading of those files into my database.

## JSON Import Classes

The side project is just a little website to provide some game master tools for someone running an RPG like Dungeons & Dragons. As part of that, I’m currently working on a magic shop generator for 5e D&D. As one of the sources of data, I need a list of magic items from the 5e SRD. I found this [fantastic repository](https://github.com/5e-bits/5e-database) on Github that provides a lot of SRD data in the format of JSON files that they use to back up an API they have to provide that data. So I’ll make use of a couple of these files to help provide some backing for what I’m creating. (Don’t worry, I will fully attribute the source when I publish my project.)

The first thing we need is to set up the import C# classes that match the structure of my JSON data files. Thankfully, there’s a site that makes this really easy to do. Go to [Json2csharp.com](https://json2csharp.com/) and there is a form there where you can paste in a sample of your JSON and it will auto-generate a set of C# classes that match the schema of the JSON. So here’s an example of creating classes for equipment.

![Screenshot of Json2csharp.com showing JSON input on the left and generated C# classes on the right.](/posts/development/2025/06/importing-json-files-into-csharp/images/json2csharp.jpg)

As you can see from this, a JSON with a sample structure like:

```JSON
[
  {
    "index": "adamantine-armor",
    "name": "Adamantine Armor",
    "equipment_category": {
      "index": "armor",
      "name": "Armor",
      "url": "/api/2014/equipment-categories/armor"
    },
    "rarity": {
      "name": "Uncommon"
    },
    "variants": [],
    "variant": false,
    "desc": [
      "Armor (medium or heavy, but not hide), uncommon",
      "This suit of armor is reinforced with adamantine, one of the hardest substances in existence. While you're wearing it, any critical hit against you becomes a normal hit."
    ],
    "image": "/api/images/magic-items/adamantine-armor.png",
    "url": "/api/2014/magic-items/adamantine-armor"
  }
]
```

gets converted to the following C# classes:

```csharp
// Root myDeserializedClass = JsonConvert.DeserializeObject<List<Root>>(myJsonResponse);
    public class EquipmentCategory
    {
        public string index { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }

    public class Rarity
    {
        public string name { get; set; }
    }

    public class Root
    {
        public string index { get; set; }
        public string name { get; set; }
        public EquipmentCategory equipment_category { get; set; }
        public Rarity rarity { get; set; }
        public List<Variant> variants { get; set; }
        public bool variant { get; set; }
        public List<string> desc { get; set; }
        public string image { get; set; }
        public string url { get; set; }
    }

    public class Variant
    {
        public string index { get; set; }
        public string name { get; set; }
        public string url { get; set; }
    }
```

The website also provides a few options if need be, allowing you to automatically add JsonProperty attributes and nullable types if appropriate. Now, having these classes, we use them to easily deserialize the JSON into C# entities with a simple import.

## Loading The File

The next step is to add the file to our project. We could do this as a one off import somewhere, but I want these to be able to refresh or update that data if the SRD changes or if I just want to reload the data. So I’ll include the JSON file in the project. I created a folder in my API project to hold my JSON data files since the API project is the one that will run the data seeding.

![Screenshot of Visual Studio Solution Explorer showing a 'Resources' folder containing a 'FiveE2014' subfolder with a '5e-SRD-Magic-Items.json' file within the 'DungeonHost.ApiService' project.](/posts/development/2025/06/importing-json-files-into-csharp/images/jsonstorefolderinapi.jpg)

The most important part of this is to set the property “Copy to Output Directory” on each of the JSON files to be “Copy always”. By default, a JSON file will not be deployed to the bin folder when you run the project and the code won’t be able to find it when you deploy to a server. If you want to deploy the files somewhere else, that’s fine too, so long as the running code can “see” the folder that holds the JSON files.

Now, in my seeding service, I’ll add the following block of code (Note that I renamed the Root class in the generated classes to FiveE2014MagicItemImport so it will be unique, as I intend to reuse the process for other JSON file imports):

```csharp
List<FiveE2014MagicItemImport> fiveEMagicItems = new List<FiveE2014MagicItemImport>();
using (StreamReader r = new StreamReader("Resources/FiveE2014/5e-SRD-Magic-Items.json"))
{
    string json = r.ReadToEnd();
    fiveEMagicItems = System.Text.Json.JsonSerializer.Deserialize<List<FiveE2014MagicItemImport>>(json);
}
```

This block of code tells the application to look in the subfolder for the JSON file, load it into a StreamReader, read the data into a string, and then deserialize the string into a List of FiveE2014MagicItemImport entities.

Now that I’ve got my list of entities, I’ll convert each one into my Item object and save it to my database table if it doesn’t already exist:

```csharp
foreach (var fiveE2014MagicItemImport in fiveEMagicItems)
{
    var exists = context.Items.FirstOrDefault(x =>
        x.SourceId == fiveE2014SRD.Id && x.Index == fiveE2014MagicItemImport.index);
    if (exists == null)
    {
        var item = new Item
        {
            Name = fiveE2014MagicItemImport.name,
            Description = Get5eDescription(fiveE2014MagicItemImport.desc),
            Rarity = Get5eRarity(fiveE2014MagicItemImport.rarity.name),
            Cost =  0,
            ItemType = Get5eItemType(fiveE2014MagicItemImport.equipment_category.index, fiveE2014MagicItemImport.name.ToLowerInvariant()),
            Level =  0,
            IsMagical = true,
            MagicItemType = get5EMagicItemEnum(fiveE2014MagicItemImport.equipment_category.index),
            IsCursed =  false, 
            Index = fiveE2014MagicItemImport.index,
            SourceId = fiveE2014SRD.Id,
            PublisherId = wizards.Id,
        };

        if(item.Description.Contains("cursed"))
            item.IsCursed = true;

        context.Items.Add(item);
    }
}
context.SaveChanges();
```

It needs a little tweaking, but you get the idea.

## Conclusion

For me, the hardest part of importing JSON data was figuring out the proper structure of the C# classes to match the format of the JSON file. It can get tricky, especially if the structure of the JSON changes throughout the file. But the Json2csharp website makes it a lot easier to grind out 90% of the work. You just have to make sure that the sample JSON you paste in covers all the scenarios you might encounter. Do that, and you’re almost there.

There’s a lot of resources out there for projects, and a lot of it is structured as JSON data in files. This handy workflow can make the whole thing so much easier to deal with.