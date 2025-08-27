```yaml
---
title: Quick Start - EF Core Provider - MongoDB Docs
source: https://www.mongodb.com/docs/entity-framework/current/quick-start/
date_published: unknown
date_captured: 2025-08-12T13:22:08.428Z
domain: www.mongodb.com
author: Unknown
category: database
technologies: [MongoDB Atlas, EF Core Provider, .NET, Microsoft.EntityFrameworkCore, MongoDB.Bson, MongoDB.Driver]
programming_languages: [C#, Shell]
tags: [mongodb, entity-framework-core, dotnet, csharp, database, cloud-database, orm, data-access, quickstart, nosql]
key_concepts: [ORM, NoSQL database, cloud-database, connection-strings, environment-variables, data-modeling, data-access]
code_examples: false
difficulty_level: intermediate
summary: |
  [This guide provides a quick start for connecting a .NET application to a MongoDB Atlas cluster using the EF Core Provider. It details the steps for setting up a free MongoDB Atlas cluster, including creating a database user and obtaining a connection string. The guide then walks through setting up a .NET console project, adding the MongoDB EF Core Provider as a dependency, and writing C# code to query sample data from the cluster. It emphasizes best practices like storing connection strings in environment variables, ensuring secure credential management. Upon completion, users will have a functional application querying MongoDB Atlas data.]
---
```

# Quick Start - EF Core Provider - MongoDB Docs

# Quick Start

This guide shows you how to create a .NET application that uses the EF Core Provider to connect to a **MongoDB Atlas cluster**. If you prefer to connect to MongoDB using another programming language, see our [list of official MongoDB drivers.](https://www.mongodb.com/docs/drivers/)

The EF Core Provider simplifies operations on data in MongoDB clusters by mapping the data to .NET objects.

MongoDB Atlas is a fully-managed cloud database service that hosts your data on MongoDB clusters. In this guide, we show you how to get started with your own free (no credit card required) cluster.

Follow the steps below to connect your EF Core Provider application to a MongoDB Atlas cluster.

## Create a MongoDB Cluster

1

### Set Up a Free Tier Cluster in Atlas

To set up your Atlas free cluster required for this Quick Start, complete the guide [MongoDB Atlas Setup](https://www.mongodb.com/docs/guides/atlas/account/) guide.

After completing the steps in the Atlas guide, you have a new MongoDB cluster deployed in Atlas, a new database user, and sample datasets loaded into your cluster. You also have a connection string similar to the following in your copy buffer:

```
"mongodb+srv://<username>:<password>@cluster0.abc.mongodb.net/?retryWrites=true&w=majority"
```

2

### Update the Placeholders

Paste the connection string in your copy buffer into a file in your preferred text editor. Replace the `<username>` and `<password>` placeholders with your database user's username and password.

Save this file to a safe location for use in the next step.

3

### Add Your Connection String to an Environment Variable

Run the following code in your shell to save the MongoDB connection string in your copy buffer from the previous step to an environment variable. Storing your connection string in an environment variable keeps your credentials separate from your source code. This separation makes it less likely to expose your credentials when sharing your code.

```
export MONGODB_URI='<your connection string>'
```

## Important

Make sure to replace the `<username>` and `<password>` sections of the connection string with the username and password of your database user.

## Set Up Your Project

1

### Create the Project

Create a new directory and use the `dotnet new` command to initialize your project as follows:

```
mkdir entity-quickstart
cd entity-quickstart
dotnet new console
```

2

### Add the EF Core Provider as a Dependency

Use the `dotnet add` command to add the EF Core Provider to your project as a dependency.

```
dotnet add package MongoDB.EntityFrameworkCore
```

## Query Your MongoDB Cluster from Your Application

1

### Add the Sample Code

Open the file named `Program.cs` in the base directory of your project. Copy the following sample code into `Program.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using MongoDB.EntityFrameworkCore.Extensions;

var connectionString = Environment.GetEnvironmentVariable("MONGODB_URI");

if (connectionString == null)
{
    Console.WriteLine("You must set your 'MONGODB_URI' environment variable. To learn how to set it, see https://www.mongodb.com/docs/drivers/csharp/current/quick-start/#set-your-connection-string");
    Environment.Exit(0);
}
var client = new MongoClient(connectionString);
var db = MflixDbContext.Create(client.GetDatabase("sample_mflix"));

var movie = db.Movies.First(m => m.Title == "Back to the Future");
Console.WriteLine(movie.Plot);

public class MflixDbContext : DbContext
{
    public DbSet<Movie> Movies { get; init; }

    public static MflixDbContext Create(IMongoDatabase database) =>
        new(new DbContextOptionsBuilder<MflixDbContext>()
            .UseMongoDB(database.Client, database.DatabaseNamespace.DatabaseName)
            .Options);

    public MflixDbContext(DbContextOptions options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Movie>().ToCollection("movies");
    }
}

public class Movie
{
    [BsonId]
    public ObjectId _id { get; set; }

    [BsonElement("title")]
    public string Title { get; set; }

    [BsonElement("rated")]
    public string Rated { get; set; }

    [BsonElement("plot")]
    public string Plot { get; set; }
}
```

2

### Query the Sample Data

Run the following command in your shell. It should print the plot of the movie "Back to the Future" from the sample dataset:

```
dotnet run entity-quickstart.csproj
```

```
A young man is accidentally sent 30 years into the past in a time-travelingDeLorean invented by his friend, Dr. Emmett Brown, and must make sure hishigh-school-age parents unite in order to save his own existence.
```

## Tip

If your output is empty, ensure you have loaded the [sample datasets](https://www.mongodb.com/docs/atlas/sample-data/) into your cluster.

After completing these steps, you should have a working Entity Framework application that connects to your MongoDB cluster, runs a query on the sample data, and prints out the result.

## Next Steps

Learn how to use the EF Core Provider to perform common operations in Quick Reference.