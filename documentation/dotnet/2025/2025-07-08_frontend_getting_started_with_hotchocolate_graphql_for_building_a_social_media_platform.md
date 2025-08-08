```yaml
---
title: Getting Started With HotChocolate GraphQL For Building a Social Media Platform
source: https://antondevtips.com/blog/getting-started-with-hot-chocolate-graphql
date_published: 2025-07-08T07:45:17.968Z
date_captured: 2025-08-06T16:20:46.752Z
domain: antondevtips.com
author: Anton Martyniuk
category: frontend
technologies: [HotChocolate, ASP.NET Core, .NET, Entity Framework Core, Nuget, WebSockets, SignalR, Git, OData, Nitro GraphQL UI]
programming_languages: [C#, GraphQL, SQL, HTTP, JSON, XML]
tags: [graphql, hotchocolate, dotnet, aspnet-core, api, data-access, entity-framework, backend, web-api, schema]
key_concepts: [GraphQL queries, GraphQL mutations, GraphQL subscriptions, GraphQL schema, data fetching, filtering-sorting-paging, projections, introspection]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to getting started with HotChocolate GraphQL in ASP.NET Core, demonstrating how to build a flexible API for a social media platform. It contrasts GraphQL's selective data fetching and single-request capabilities with traditional REST API drawbacks. The post covers setting up HotChocolate, defining GraphQL queries, mutations, and subscriptions, and leveraging the built-in Nitro UI for schema exploration and query testing. Furthermore, it delves into advanced features like integrating with Entity Framework Core for automatic projections, filtering, sorting, and paging, and discusses best practices for organizing GraphQL code within a clean architecture.]
---
```

# Getting Started With HotChocolate GraphQL For Building a Social Media Platform

![newsletter](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fdotnet%2Fcover_dotnet_hotchocolate_getting_started.png&w=3840&q=100)

# Getting Started With HotChocolate GraphQL For Building a Social Media Platform

Jul 8, 2025

[Download source code](/source-code/getting-started-with-hot-chocolate-graphql)

10 min read

### Newsletter Sponsors

Thousands of developers fixed EF Core performance — with just one [method](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=july-2025).  
→ [Discover this extension](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=july-2025)

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Modern client applications demand flexible data fetching. Usually, when using REST APIs, clients call multiple REST endpoints and over-fetch data.

One way to solve it is by using a Backend-For-Frontend (BFF) pattern. However, it is hard and tedious to build, and its flexibility is limited.

GraphQL was created to address these issues. Hot Chocolate is the most efficient, feature-rich, open-source GraphQL server in the .NET ecosystem that helps developers build powerful GraphQL APIs and Gateways with ease.

It brings the following benefits compared to traditional REST APIs:

**1\. Selective data fetching:** Each client chooses exactly the fields it needs, no over- or under-fetching. A web UI can pull rich profile details, while a mobile app requests only minimal essential fields.

**2\. Single request for multiple resources:** Bundle multiple requests into a single round-trip to the server. Eliminate client choreography across three or more REST endpoints.

**3\. Strongly-typed schema & introspection:** Auto-generated docs, code generation, and IDE autocomplete help frontend and backend stay in sync.

**4\. Built-in filtering, sorting, and paging:** HotChocolate's middleware takes care of common patterns. No need for manual dynamic querying, sorting and filtering. And it solves it better and easier than OData.

**5\. Enhanced developer experience:** The Nitro GraphQL UI in HotChocolate lets you explore types, build queries visually, and test in seconds.

I have been using HotChocolate GraphQL for more than 3 years in production. And it significantly improved how I craft my APIs.

Today I will help you to get started with HotChocolate GraphQL. We will explore how a simple Social Media application with REST APIs turns into a flexible GraphQL Server.

In this post, we will explore:

*   Social Media Application and the problem with REST APIs
*   What is GraphQL
*   How to add HotChocolate to the project
*   How to write GraphQL Queries
*   Nitro UI: schema browsing and query building
*   How HotChocolate integrates with EF Core for projections, filtering, sorting, and paging
*   How to organize code with best practices when using GraphQL

Let's dive in!

[](#application-overview-simple-social-media-platform)

## Application Overview: Simple Social Media Platform

Our example application manages users, posts, comments, likes, feeds, notifications, and categories.

Let's briefly explore the relations of entities of our Social Media Platform:

```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public List<Post> Posts { get; set; } = [];
    public List<Comment> Comments { get; set; } = [];
    public List<Feed> Feeds { get; set; } = [];
    public List<Notification>? Notifications { get; set; } = [];
}

public class Post
{
    public Category Category { get; set; } = null!;
    public List<Like> Likes { get; set; } = [];
    public List<Comment> Comments { get; set; } = [];
}

public class Like
{
    public Post Post { get; set; } = null!;
    public User User { get; set; } = null!;
}

public class Comment
{
    public User User { get; set; } = null!;
    public Post Post { get; set; } = null!;
}

public class Category
{
    public List<Post> Posts { get; set; } = [];
}

public enum NotificationType
{
    Post,
    Like,
    Comment
}

public class Notification
{
    public User User { get; set; } = null!;
    public NotificationType Type { get; set; }
    public int ItemId { get; set; }
}

public class Feed
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int PostId { get; set; }
}
```

> You can download a complete source code of an application at the end of the post

Let's explore a real use case: a user opens a social media website, and they see:

*   Recent feed with new posts
*   User's own posts
*   Notifications with recent posts, comments and likes to the posts the user is interested in

Consider we have the following endpoints:

```csharp
public class GetUserFeedEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/feed",
            async (int userId, int count, SocialMediaDbContext dbContext) => { }
        );
    }
}

public class GetUserNotificationsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/notifications",
            async (int userId, int count, SocialMediaDbContext dbContext) => { }
        );
    }
}

public class GetUserPostsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/users/{userId}/posts",
            async (int userId, int count, SocialMediaDbContext dbContext) => { }
        );
    }
}
```

A typical frontend application needs to send 3 separate requests to query the necessary data:

```http
### Get user feed
GET /api/feed?userId=1&count=10

### Get user notifications
GET /api/notifications?userId=1&count=10

### Get user posts
GET /api/users/1/posts?count=10
```

This implementation has two major drawbacks:

*   Each client application must send 3 separate API requests to the server
*   Each client application consumes all the fields the server returns. For a website it may be okay, but too much data will slow down a mobile application.

Let's explore what GraphQL offers to solve these drawbacks.

[](#understanding-graphql-queries-mutations-subscriptions)

## Understanding GraphQL: Queries, Mutations & Subscriptions

Before we jump into adding HotChocolate to the project, let's take a closer look at the core GraphQL concepts and why they map so naturally to modern client needs.

At its heart, GraphQL is both:

*   A type system: you define your object types and their fields in a schema
*   A query language: clients ask for exactly the data they need

Rather than dozens of REST endpoints, you publish a single schema and let each client shape its requests.

[](#queries)

### Queries

Queries are the read-only operations in GraphQL. You ask for a type and a selection of its fields:

```gql
query GetUserProfile {
  user(userId: 1) {
    id
    username
    posts {
      id
      content
    }
  }
}
```

The client controls exactly which fields are returned. No over-fetching or under-fetching.

[](#mutations)

### Mutations

Mutations modify the server state (create, update or delete). They look similar to queries:

```gql
mutation CreatePost {
  createPost(input: { userId: 1, content: "Hello World" }) {
    id
    content
    createdAt
  }
}
```

Mutations return the updated object(s) so the client can update its state. You can send multiple mutations in a single request.

[](#subscriptions)

### Subscriptions

Subscriptions enable real-time, server-to-client push. Under the hood, they use WebSockets:

```gql
subscription OnNewComment {
  commentAdded(postId: 42) {
    id
    text
    username
    createdAt
  }
}
```

Clients "subscribe" to events (e.g. new comments) and the server pushes updates automatically. Much like in SignalR.

Now that we understand how GraphQL works conceptually, let's wire up HotChocolate in our ASP.NET Core application.

[](#getting-started-with-hotchocolate)

## Getting Started with HotChocolate

To get started with HotChocolate, add the following Nuget packages to your solution:

```xml
<ItemGroup>
  <PackageReference Include="HotChocolate.AspNetCore" Version="15.1.5" />
  <PackageReference Include="HotChocolate.Data" Version="15.1.5" />
  <PackageReference Include="HotChocolate.Types.Analyzers" Version="15.1.5">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive;</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
  </PackageReference>
</ItemGroup>
```

Then configure GraphQLServer in DI:

```csharp
var builder = WebApplication.CreateBuilder(args);

// Register GraphQL services
builder.Services
    .AddGraphQLServer()
    .AddQueryType()
    .DisableIntrospection(builder.Environment.IsProduction());

var app = builder.Build();

// exposes /graphql endpoint
app.MapGraphQL();

app.Run();
```

Introspection is a GraphQL feature that allows clients to discover the schema and its types. Make sure you disable introspection in production for security reasons.

`AddQueryType` registers the QueryType, which is the root of the GraphQL schema.

Let's add a root query to our schema:

```csharp
public record UserRootType(int UserId);

[QueryType]
public class UserQueries
{
    public UserRootType GetUser(int userId) => new(userId);
}
```

This is what the query looks like in GQL (Graph Query Language):

```gql
query GetUser {
  user(userId: 1) {
    
  }
}
```

The `[QueryType]` attribute tells HotChocolate that his class defines the root Query.

`GetUser(int userId)` becomes the `user(userId: Int!)` field in the schema.

Now let's add user's posts, feed and notifications to our schema.

Rather than adding all methods into one large class, we can split them into separate classes:

```csharp
[ExtendObjectType(typeof(UserRootType))]
public class PostQueries
{
    public async Task<List<PostResponse>> GetUserPosts(
        SocialMediaDbContext dbContext,
        [Parent] UserRootType user,
        int count = 10)
    {
        var posts = await dbContext.Posts
            .Where(p => p.UserId == user.UserId)
            .OrderByDescending(p => p.Id)
            .Take(count)
            .Select(p => new PostResponse
            {
                // ...
            })
            .ToListAsync();

        return posts;
    }
}
```

Here we use `[ExtendObjectType]` attribute to tell HotChocolate that this class extends the `UserRootType`.

The `[Parent]` parameter injects the `UserRootType`, so we can get a `User.Id` inside our queries.

Other queries are similar:

```csharp
[ExtendObjectType(typeof(UserRootType))]
public class NotificationQueries
{
    public async Task<List<NotificationResponse>> GetUserNotifications(
        SocialMediaDbContext dbContext,
        [Parent] UserRootType user,
        int count = 10)
    {
        var notifications = await dbContext.Notifications
            .Where(n => n.UserId == user.UserId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(count)
            .Select(n => new NotificationResponse
            {
                /// ...
            })
            .ToListAsync();

        return notifications;
    }
}

[ExtendObjectType(typeof(UserRootType))]
public class FeedQueries
{
    public async Task<List<FeedResponse>> GetUserFeed(
        SocialMediaDbContext dbContext,
        [Parent] UserRootType user,
        int count = 10)
    {
        var feed = await dbContext.Feeds
            .Where(f => f.UserId == user.UserId)
            .OrderByDescending(f => f.CreatedAt)
            .Take(count)
            .Select(f => new FeedResponse
            {
                /// ...
            })
            .ToListAsync();

        return feed;
    }
}
```

We need to register these types in the GraphQL setup. Luckily, `HotChocolate.Types.Analyzers` package supports source generation to simplify this process.

Add the `ModuleInfo.cs` file into your project where GraphQL Queries are defined:

```csharp
using HotChocolate;

[assembly: Module("SocialMediaTypes")]
```

Now you can call the `AddSocialMediaTypes` extension method to register all types in the module:

```csharp
builder.Services
    .AddGraphQLServer()
    .AddQueryType()
    .AddSocialMediaTypes() // Add this line
    .AddQueryContext()
    .DisableIntrospection(builder.Environment.IsProduction());
```

Here is our project's structure. I have defined all API endpoints and GraphQL queries in the `HotChocolateGraphQL.Presentation` project:

![Screenshot_6](https://antondevtips.com/media/code_screenshots/dotnet/hotchocolate-getting-started/img_6.png)

This project follows a combination of [Vertical Slice](https://antondevtips.com/blog/the-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices/?utm_source=antondevtips&utm_medium=email&utm_campaign=newsletter) and [Clean Architecture](https://antondevtips.com/blog/the-best-way-to-structure-your-dotnet-projects-with-clean-architecture-and-vertical-slices/?utm_source=antondevtips&utm_medium=email&utm_campaign=newsletter).

Now it's time to run our project.

[](#exploring-nitro-hotchocolates-builtin-graphql-ui)

## Exploring Nitro: HotChocolate's Built-In GraphQL UI

Once you have HotChocolate wired up and your app running, navigate to `http://localhost:5000/graphql` to explore the schema.

You'll be greeted by Nitro, HotChocolate's GraphQL IDE:

![Screenshot_1](https://antondevtips.com/media/code_screenshots/dotnet/hotchocolate-getting-started/img_1.png)

It doesn't require any extra configuration.

Press a "Browse Schema" button and Nitro will send an introspection query (\_\_schema) to fetch all types, fields, enums, and directives:

On the "Schema" tab you can find:

*   All your query types like `UserRootType`
*   Response types (PostResponse, etc.)
*   Built-in scalars/ enums.

![Screenshot_2](https://antondevtips.com/media/code_screenshots/dotnet/hotchocolate-getting-started/img_2.png)

After you make changes in your schema and re-launch the app, press the "refresh" button to see the changes.

On the "Operation" tab you can write your queries:

![Screenshot_3](https://antondevtips.com/media/code_screenshots/dotnet/hotchocolate-getting-started/img_3.png)

Nitro has a built-in "Query Builder" that helps you build queries visually, just open the tree and select nodes you want to include in your query.

The "Request" tab shows the generated query and variables. You can write the query manually, it supports intellisense just like in a regular IDE.

Nitro supports writing and executing GraphQL queries, mutations and subscriptions.

Hit "Run" to execute your query over HTTP (or WebSocket for subscriptions).

The Response panel shows JSON results, errors, and execution timings.

You can also view logs for every request and response:

![Screenshot_4](https://antondevtips.com/media/code_screenshots/dotnet/hotchocolate-getting-started/img_4.png)

Let's write a Query that returns the user's feed, notifications and posts:

```gql
query User {
  user(userId: 9) {
    userFeed(count: 10) {
      commentsCount
      content
      createdAt
      id
      likesCount
      postId
      username
    }
    userNotifications(count: 10) {
      createdAt
      id
      type
      itemId
      isRead
    }
    userPosts(count: 10) {
      commentsCount
      content
      createdAt
      id
      likesCount
      recentComments {
        createdAt
        id
        text
        username
      }
    }
  }
}
```

It returns the following response:

```json
{
  "data": {
    "user": {
      "userFeed": [...],
      "userNotifications": [...],
      "userPosts": [...]
    }
  }
}
```

How this works under the hood:

1.  **Root resolver:** `GetUser(userId: 9)` returns a `UserRootType` holding `UserId = 9`.
2.  **Field resolvers:** HotChocolate sees three selections: userFeed, userNotifications, userPosts, and executes each corresponding method, passing in `UserRootType` and the count argument.
3.  **Projection & LINQ:** Each resolver issues a LINQ query against `SocialMediaDbContext`, projects into the lightweight response DTO, and returns the list.
4.  **Single HTTP round-trip:** Nitro (or any GraphQL client) makes one POST to /graphql with the above operation; the server returns a JSON payload combining all data.

By default, GraphQL executes each method in a query in parallel. However, when we use EF Core's DbContext, it executes each query in sequence, as it's only allowed to access DbContext from a single thread.

Let's explore what powerful features HotChocolate offers when integrating with EF Core.

[](#builtin-tools-for-projections-filtering-sorting-and-paging)

## Built-in Tools for Projections, Filtering, Sorting, and Paging

HotChocolate comes with middleware attributes that plug directly into your query methods for projections, filtering, sorting and pagination. These middlewares integrate directly with IQueryable or IEnumerable. And thus, integrate with EF Core.

Let's explore an example of an admin query that allows filtering, sorting, projecting and paging for Posts:

```csharp
[QueryType]
public class PostQueries
{
    [UseOffsetPaging(MaxPageSize = 100, IncludeTotalCount = true)]
    [UseProjection]
    [UseFiltering]
    [UseSorting]
    public IQueryable<Post> GetPosts(SocialMediaDbContext dbContext)
    {
        return dbContext.Posts.AsNoTracking();
    }
}
```

Here, we return all posts from DbContext and HotChocolate hooks into the IQueryable to apply the filters, sort, projection and paging.

Make sure to add the following Nuget package to your project:

```xml
<ItemGroup>
    <PackageReference Include="HotChocolate.Types.OffsetPagination" Version="15.1.5" />
</ItemGroup>
```

And register the middlewares in the DI:

```csharp
builder.Services
    //...
    .AddFiltering()
    .AddSorting()
    .AddProjections()
    .AddPagingArguments();
```

Here is a query we can send to the server to apply only paging:

```gql
query AdminArea {
  posts(skip: 0, take: 3, where: null, order: null) {
    totalCount
    items {
      id
      categoryId
      content
      createdAt
      category {
        id
        name
      }
      user {
        id
        username
      }
      comments {
        text
        userId
        createdAt
      }
    }
    pageInfo {
      hasNextPage
      hasPreviousPage
    }
  }
}
```

Here is how we can filter, sort and project the results:

```gql
query AdminArea {
  posts(skip: 0, take: 3,
    where: {
        user:  {
            username:  {
                eq: "Rory6"
            }
        }
    },
    order: {
        category:  {
            name: ASC
        }
    }
  ) {
    items {
      id
      categoryId
      content
    }
  }
```

No magic, HotChocolate just does all the heavy lifting for you.

Behind the scenes, HotChocolate translates your GraphQL arguments into LINQ:

```csharp
dbContext.Posts
    .OrderBy(p => /* order clause */)
    .Where(/* filter clause */)
    .Skip(0)
    .Take(3)
    .Select(/* projection based on requested fields */)
```

It's somewhat a similar experience to OData, but much more powerful and flexible.

[](#best-practices-for-organizing-code-with-hotchocolate)

## Best Practices for Organizing Code With HotChocolate

IQueryable and DbContext may be not something you want to use in production code in your GraphQL queries.

Ideally, we want our GraphQL layer to be as thin as possible. We may want to hide business logic inside our Application and Domain layers.

Let's explore how we can push this logic down into an Application Service layer:

```csharp
[UseOffsetPaging(MaxPageSize = 100)]
[UseProjection]
[UseFiltering]
[UseSorting]
public async Task<CollectionSegment<Post>> GetPostsNew(
    PostService postService,
    OffsetPagingArguments pagingArguments,
    QueryContext<Post>? query = null,
    CancellationToken cancellationToken = default)
    => await postService.GetPostsAsync(pagingArguments, query, cancellationToken);
```

Here we use 2 built-in HotChocolate types:

*   `PagingArguments`: represents the paging arguments, e.g. `skip`, `take`.
*   `QueryContext<T>`: encapsulates a selector expression, a filter expression, and a sort definition.

`PostService` implementation is straightforward:

```csharp
public class PostService(SocialMediaDbContext dbContext)
{
    public async Task<CollectionSegment<Post>> GetPostsAsync(
        OffsetPagingArguments pagingArguments,
        QueryContext<Post>? query = null,
        CancellationToken cancellationToken = default)
        => await dbContext.Posts
            .OrderByDescending(x => x.CreatedAt)
            .With(query)
            .ApplyOffsetPaginationAsync(pagingArguments, cancellationToken);
}
```

The key point here is that we use `With` method to apply the filter, projection and sort expressions on the DbContext. We call `ApplyOffsetPaginationAsync` to apply paging.

This is a new feature of HotChocolate 15, you learn more [here](https://chillicream.com/blog/2025/02/01/hot-chocolate-15).

You need to register the `AddQueryContext` in the GraphQL Server:

```csharp
builder.Services
    //...
    .AddQueryContext();
```

Here is one more advice to consider: create a GraphQL schema each time you launch your application:

```csharp
// Map GraphQL endpoint
app.MapGraphQL();

// Save schema into file to see the changed in GIT
var executor = await app.Services.GetRequestExecutorAsync();
await File.WriteAllTextAsync("schema.graphql", executor.Schema.ToString());

await app.RunAsync();
```

And commit `schema.graphql` to GIT. This way you can control all the changes in your GraphQL schema and review every change to it.

[](#summary)

## Summary

In this post, we explored how to replace multiple REST endpoints with a single, flexible GraphQL schema using HotChocolate and EF Core.

Key takeaways:

*   **GraphQL vs. REST:** Clients fetch exactly the fields they need and combine multiple resources into one request, reducing over- and under-fetching.
*   **Single-Round-Trip Queries:** Clients send one GraphQL query to fetch feed items, notifications, and posts simultaneously, with only requested fields.
*   **HotChocolate Setup:** A few lines in Program.cs register the GraphQL server and enable filtering, sorting, projections, paging.
*   **Query Types & Extensions:** A thin UserRootType plus \[QueryType\] and \[ExtendObjectType\] classes let us expose userFeed, userNotifications, and userPosts under one user(userId: Int!) field.
*   **Nitro UI:** HotChocolate's built-in Nitro IDE instantly introspects your schema, provides a visual query builder, and lets you test GraphQL queries, mutations and subscriptions in the browser.
*   **Advanced Patterns:** Middleware attributes like \[UseOffsetPaging\], \[UseFiltering\], and \[UseSorting\] plug directly into IQueryable<T> methods — automating filters, projections, sorting, and paging.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/getting-started-with-hot-chocolate-graphql)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-hot-chocolate-graphql&title=Getting%20Started%20With%20HotChocolate%20GraphQL%20For%20Building%20a%20Social%20Media%20Platform)[X](https://twitter.com/intent/tweet?text=Getting%20Started%20With%20HotChocolate%20GraphQL%20For%20Building%20a%20Social%20Media%20Platform&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-hot-chocolate-graphql)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fgetting-started-with-hot-chocolate-graphql)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.

![Image 1: Newsletter cover image with "dev tips" logo and text "GETTING STARTED WITH HOT CHOCOLATE GRAPHQL FOR BUILDING A SOCIAL MEDIA PLATFORM"](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fdotnet%2Fcover_dotnet_hotchocolate_getting_started.png&w=3840&q=100)
![Image 2: Screenshot showing a file explorer view of a .NET solution structure, highlighting the "HotChocolateGraphQL.Presentation" project with folders for Features, AdminArea, Feed, Notifications, and Posts, containing C# query and response files.](https://antondevtips.com/media/code_screenshots/dotnet/hotchocolate-getting-started/img_6.png)
![Image 3: Screenshot of the Nitro GraphQL IDE interface, showing a blank canvas with "Create Document" and "Browse Schema" buttons, and keyboard shortcuts for operations.](https://antondevtips.com/media/code_screenshots/dotnet/hotchocolate-getting-started/img_1.png)
![Image 4: Screenshot of the Nitro GraphQL IDE schema browser, displaying the "UserRootType" with its fields (userFeed, userNotifications, userPosts) and a sidebar showing root types, objects, scalars, enums, and directives.](https://antondevtips.com/media/code_screenshots/dotnet/hotchocolate-getting-started/img_2.png)
![Image 5: Screenshot of the Nitro GraphQL IDE's operation tab, showing a GraphQL query being constructed visually using a tree view on the left, the generated query in the center, and the JSON response on the right.](https://antondevtips.com/media/code_screenshots/dotnet/hotchocolate-getting-started/img_3.png)
![Image 6: Screenshot of the Nitro GraphQL IDE showing the logs panel, displaying details of a GraphQL request and its corresponding JSON response.](https://antondevtips.com/media/code_screenshots/dotnet/