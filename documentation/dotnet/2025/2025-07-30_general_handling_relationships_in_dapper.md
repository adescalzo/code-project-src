```yaml
---
title: Handling Relationships in Dapper
source: https://www.nikolatech.net/blogs/dapper-relationship-asp-dotnet-core
date_published: 2025-07-30T23:56:15.000Z
date_captured: 2025-08-06T11:42:13.551Z
domain: www.nikolatech.net
author: Unknown
category: general
technologies: [Dapper, .NET, ASP.NET Core, Dapper Plus, SQL Server]
programming_languages: [C#, SQL]
tags: [dapper, micro-orm, database, dotnet, relationships, data-access, sql, csharp, orm, query-mapping]
key_concepts: [one-to-one-relationship, one-to-many-relationship, many-to-many-relationship, QueryAsync, splitOn, data-mapping, join-operations, micro-orm]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide on handling different types of relationships using Dapper, a popular micro ORM in the .NET ecosystem. It covers one-to-one, one-to-many, and many-to-many relationships, illustrating each with C# code examples and corresponding SQL queries. The post explains how to effectively use Dapper's `QueryAsync` method and the `splitOn` parameter for accurate object mapping. It also demonstrates the use of dictionaries to manage duplicate instances when mapping one-to-many and many-to-many relationships, ensuring efficient data retrieval and organization.]
---
```

# Handling Relationships in Dapper

![Banner image for the article "Dapper Relationships in .NET" featuring the Dapper logo and a blue background.](https://coekcx.github.io/BlogImages/banners/dapper-relationship-asp-dotnet-core-banner.png)

#### Handling Relationships in Dapper

###### 30 Jul 2025

###### 5 min

### Special Thanks to Our Sponsors:

![Sponsor Logo for ZZZ Projects, featuring a stylized 'Z' logo.](https://zzzprojects.com/images/logo256X256.png)

[Optimize SQL write operations](https://dapper-plus.net/?utm_source=nikolatech&utm_medium=nikolatech&utm_campaign=nikolatech) for speed, scale, and minimal server load.

Insert and update your data up to **75x faster** and reduce save time by **99%** compared to Dapper.

👉 [Discover Dapper Plus](https://dapper-plus.net/?utm_source=nikolatech&utm_medium=nikolatech&utm_campaign=nikolatech) - Bulk Insert, Update, Delete & Merge.

**Dapper** is the most popular micro ORM in the .NET ecosystem, and it's a must to learn for any developer working with .NET.

If you're completely new to Dapper, we explored its basic methods in the previous blog post:

In this post, we'll focus on one of the most important aspects of working with any ORM, including Dapper, and that is handling relationships between entities.

## Relationships

**Relationships** define how data in one table is connected to data in another. They are essential for organizing data efficiently and avoiding redundancy.

For this blog post, we’ll cover how to handle the three main types of relationships:

*   **One to one relationship**
*   **One to many relationship**
*   **Many to many relationship**

Without further ado, let’s master relationships with Dapper.

## One to one relationships

True 1:1 relationships are typically used to extend existing models. Most real-world domains lean toward 1:N or N:N relationships instead.

In this example, a 1:1 relationship exists between User and UserPreferences. Each user has exactly one preference and each preference belongs to only one user.

```csharp
public class User
{
    public int Id { get; set; }

    public string Name { get; set; }

    public UserPreferences Preferences { get; set; }

    public List<Blog> Blogs { get; set; }
}

public class UserPreferences
{
    public int Id { get; set; }

    public bool DarkMode { get; set; }

    public int UserId { get; set; }

    public User User { get; set; }
}
```

Here is how to retrieve a user along with their preferences using Dapper:

```csharp
using var connection = factory.OpenConnection();

var sql = """
          SELECT u."Id", u."Name",
                  up."Id", up."DarkMode", up."UserId"
          FROM "Users" u
          JOIN "UserPreferences" up ON u."Id" = up."UserId"
          WHERE u."Id" = @UserId;
          """;

var users = await connection.QueryAsync(
    sql,
    (user, prefs) =>
    {
        user.Preferences = prefs;
        return user;
    },
    new { UserId = userId },
    splitOn: "Id"
);

return users.FirstOrDefault();
```

**QueryAsync<User, UserPreferences, User>** tells Dapper how to map the result into objects.

Thanks to the splitOn property, Dapper knows how to split the row between User and UserPreferences.

### SplitOn

The **splitOn** parameter indicates where the second object begins in the result set.

In our case, Dapper reads the result from left to right and starts filling the User object first. When it reaches the column named Id, it knows that this column belongs to the UserPreferences object, and starts mapping those properties accordingly.

When you have multiple objects to map in a single query result, the splitOn parameter should contain a comma separated list of column names, with one column per object.

## One to many relationships

A 1:N relationship is a type of data association where a single record in one table can be linked to multiple records in another table, while each record in the second table is linked to only one record in the first table.

For example, each owner can own multiple blogs, but each blog is associated with only one owner.

```csharp
public class User
{
    public int Id { get; set; }

    public string Name { get; set; }

    public UserPreferences Preferences { get; set; }

    public List<Blog> Blogs { get; set; }
}

public class Blog
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int UserId { get; set; }

    public User User { get; set; }

    public List Tags { get; set; }
}
```

1:N relationships are a bit more complex, since it requires additional effort to map correctly.

```csharp
using var connection = factory.OpenConnection();

var sql = """
          SELECT u."Id", u."Name",
                 b."Id", b."Name", b."Description", b."UserId"
          FROM "Users" u
          LEFT JOIN "Blogs" b ON u."Id" = b."UserId"
          WHERE u."Id" = @UserId;
          """;

var userDict = new Dictionary<int, User>();

await connection.QueryAsync(
    sql,
    (user, blog) =>
    {
        if (!userDict.TryGetValue(user.Id, out var existingUser))
        {
            existingUser = user;
            existingUser.Blogs = new List<Blog>();
            userDict.Add(existingUser.Id, existingUser);
        }

        if (blog != null)
            existingUser.Blogs.Add(blog);

        return existingUser;
    },
    new { UserId = userId },
    splitOn: "Id"
);

return userDict[userId];
```

And here's the catch, when using a JOIN, the same user will appear for each related blog.

To avoid creating duplicate User instances, we need to use a dictionary. If not, we add the user and append the blog to that user's list. This process repeats for each row in the result set.

It might not be the cleanest approach, but it’s effective and gets the job done.

## Many to many relationships

Just like with 1:N relationships, handling a N:M relationship also requires the use of a dictionary to avoid duplicate instances when mapping the results.

A N:M relationship means that a single record in Table A can be related to multiple records in Table B and vice versa.

For example, a single blog can have multiple tags, and each tag can be associated with multiple blogs.

```csharp
public class Blog
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public int UserId { get; set; }

    public User User { get; set; }

    public List<Tag> Tags { get; set; }
}

public class Tag
{
    public int Id { get; set; }
    
    public string Name { get; set; }

    public List<Blog> Blogs { get; set; }
}
```

The implementation looks pretty similar:

```csharp
using var connection = factory.OpenConnection();

var sql = """
          SELECT b."Id", b."Name", b."Description", b."UserId",
                  t."Id", t."Name"
          FROM "Blogs" b
          LEFT JOIN "BlogTag" bt ON b."Id" = bt."BlogId"
          LEFT JOIN "Tags" t ON bt."TagId" = t."Id"
          WHERE b."UserId" = @UserId;
          """;

var blogDict = new Dictionary<int, Blog>();

await connection.QueryAsync<Blog, Tag, Blog>(
    sql,
    (blog, tag) =>
    {
        if (!blogDict.TryGetValue(blog.Id, out var existingBlog))
        {
            existingBlog = blog;
            existingBlog.Tags = [];
            blogDict.Add(existingBlog.Id, existingBlog);
        }

        if (tag != null)
        {
            existingBlog.Tags.Add(tag);
        }

        return existingBlog;
    },
    new { UserId = userId },
    splitOn: "Id"
);

return blogDict.Values;
```

## Conclusion

In this post, we explored how to manage the three main types of relationships using Dapper.

By understanding how to map these relationships properly with Dapper’s QueryAsync method and using the splitOn parameter, you can efficiently retrieve and organize related data with minimal overhead.

With these techniques, you are well on your way to mastering relationships in Dapper.

If you want to check out examples I created, you can find the source code here:

[Source Code](https://www.nikolatech.net/codes/dapper-relationships-examples)

I hope you enjoyed it, subscribe and get a notification when a new blog is up!

# Subscribe

###### Stay tuned for valuable insights every Thursday morning.

##### Share This Article: