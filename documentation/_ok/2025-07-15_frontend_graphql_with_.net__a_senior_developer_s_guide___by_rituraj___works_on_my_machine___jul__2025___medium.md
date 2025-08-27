```yaml
---
title: "GraphQL with .NET: A Senior Developer’s Guide | by Rituraj | Works On My Machine | Jul, 2025 | Medium"
source: https://medium.com/works-on-my-machine/graphql-with-net-a-senior-developers-guide-3ffb7e6440c6
date_published: 2025-07-15T06:49:46.657Z
date_captured: 2025-08-22T10:53:34.510Z
domain: medium.com
author: Rituraj
category: frontend
technologies: [GraphQL, .NET, ASP.NET Core, Hot Chocolate, Entity Framework Core, Microsoft.EntityFrameworkCore.InMemory, Banana Cake Pop, StrawberryShake]
programming_languages: [C#, SQL, GraphQL]
tags: [graphql, dotnet, api, web-api, data-access, real-time, performance, authentication, error-handling, n-plus-1]
key_concepts: [GraphQL queries, GraphQL mutations, GraphQL subscriptions, N+1 problem, DataLoaders, API security, error handling, query optimization]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide for senior developers on implementing GraphQL APIs using .NET and the Hot Chocolate framework. It contrasts GraphQL with traditional REST APIs, highlighting benefits like efficient data fetching and real-time capabilities. The guide walks through setting up a GraphQL server, defining schemas, handling queries, mutations, and subscriptions, and integrating with Entity Framework Core. Furthermore, it delves into advanced topics such as solving the N+1 problem with DataLoaders, implementing authentication, authorization, error handling, and various performance optimizations, offering a holistic view of building robust GraphQL solutions.]
---
```

# GraphQL with .NET: A Senior Developer’s Guide | by Rituraj | Works On My Machine | Jul, 2025 | Medium

Top highlight

Press enter or click to view image in full size

![Thumbnail for an article titled "GraphQL with .NET: A Senior Developer's Guide". It features a man with a surprised expression, with "C# / .NET" and "GraphQL" logos, and the article title in large yellow and white text on a purple background.](https://miro.medium.com/v2/resize:fit:1000/1*_M3-CrtIxqgLAox6wwkTrg.png)

Member-only story

# GraphQL with .NET: A Senior Developer’s Guide

[

![Rituraj](https://miro.medium.com/v2/resize:fill:64:64/1*DKw0FuiAgaUk2O2RpwLEeg.jpeg)

](/@riturajpokhriyal?source=post_page---byline--3ffb7e6440c6---------------------------------------)

[Rituraj](/@riturajpokhriyal?source=post_page---byline--3ffb7e6440c6---------------------------------------)

Following

8 min read

·

Jul 2, 2025

253

Listen

Share

More

_Stop wrestling with REST endpoints and embrace the future of API development_

You’re staring at your API documentation again. Twenty-three different endpoints, each returning slightly different data structures. Your mobile team is complaining about over-fetching, your web team about under-fetching, and you’re wondering if there’s a better way.

[Read article for free](/@riturajpokhriyal/graphql-with-net-a-senior-developers-guide-3ffb7e6440c6?sk=8426d32abebe41afe480e15c504b311b)

There is. And it’s called **GraphQL**.

If you’ve been curious about GraphQL but haven’t taken the plunge, or if you’re a .NET developer wondering how these two technologies play together, you’re in the right place. We’re going to build something real, understand the why behind every decision, and by the end, you’ll have the confidence to pitch GraphQL to your team (and actually implement it).

# Why GraphQL Exists (And Why You Should Care)

Remember the last time you needed user data for your dashboard? You probably hit `/api/users/{id}`, got back a massive JSON object with 47 fields, and used exactly 3 of them. Then you needed their recent posts, so another call to `/api/users/{id}/posts`. Then their followers count, so `/api/users/{id}/followers`.

Three round trips. Three opportunities for failure. Three times the latency.

GraphQL flips this script. One request, exactly the data you need, structured the way you want it:

```graphql
query {  
  user(id: "123") {  
    name  
    email  
    posts(limit: 5) {  
      title  
      publishedAt  
    }  
    followerCount  
  }  
}
```

That’s not just cleaner — it’s fundamentally different. You’re describing what you want, not how to get it.

# Setting Up Your .NET GraphQL Playground

Let’s get our hands dirty. We’ll build a book review API because everyone can relate to books, and it gives us interesting relationships to work with.

First, create a new ASP.NET Core project:

```bash
dotnet new webapi -n BookGraphQL  
cd BookGraphQL
```

Install the essential packages:

```bash
dotnet add package HotChocolate.AspNetCore  
dotnet add package HotChocolate.Data.EntityFramework  
dotnet add package Microsoft.EntityFrameworkCore.InMemory
```

**Hot Chocolate** is our GraphQL server. It’s not just good — it’s exceptional. Built specifically for .NET, it understands your C# types, integrates seamlessly with dependency injection, and has features that’ll make you wonder how you lived without them.

# Building Your Data Layer (The Foundation)

Every great API starts with solid models. Here’s our domain:

```csharp
public class Book  
{  
    public int Id { get; set; }  
    public string Title { get; set; } = string.Empty;  
    public string ISBN { get; set; } = string.Empty;  
    public DateTime PublishedDate { get; set; }  
    public int AuthorId { get; set; }  
    public Author Author { get; set; } = null!;  
    public List<Review> Reviews { get; set; } = new();  
}  
  
public class Author  
{  
    public int Id { get; set; }  
    public string Name { get; set; } = string.Empty;  
    public string Bio { get; set; } = string.Empty;  
    public DateTime BirthDate { get; set; }  
    public List<Book> Books { get; set; } = new();  
}  
public class Review  
{  
    public int Id { get; set; }  
    public string Content { get; set; } = string.Empty;  
    public int Rating { get; set; }  
    public string ReviewerName { get; set; } = string.Empty;  
    public DateTime CreatedAt { get; set; }  
    public int BookId { get; set; }  
    public Book Book { get; set; } = null!;  
}
```

Now the DbContext:

```csharp
public class BookContext : DbContext  
{  
    public BookContext(DbContextOptions<BookContext> options) : base(options) { }  
      
    public DbSet<Book> Books { get; set; }  
    public DbSet<Author> Authors { get; set; }  
    public DbSet<Review> Reviews { get; set; }  
      
    protected override void OnModelCreating(ModelBuilder modelBuilder)  
    {  
        // Seed some data  
        modelBuilder.Entity<Author>().HasData(  
            new Author { Id = 1, Name = "J.K. Rowling", Bio = "British author", BirthDate = new DateTime(1965, 7, 31) },  
            new Author { Id = 2, Name = "George Orwell", Bio = "English novelist", BirthDate = new DateTime(1903, 6, 25) }  
        );  
          
        modelBuilder.Entity<Book>().HasData(  
            new Book { Id = 1, Title = "Harry Potter and the Philosopher's Stone", ISBN = "978-0747532699", PublishedDate = new DateTime(1997, 6, 26), AuthorId = 1 },  
            new Book { Id = 2, Title = "1984", ISBN = "978-0451524935", PublishedDate = new DateTime(1949, 6, 8), AuthorId = 2 }  
        );  
          
        modelBuilder.Entity<Review>().HasData(  
            new Review { Id = 1, Content = "A magical journey!", Rating = 5, ReviewerName = "BookLover42", CreatedAt = DateTime.Now.AddDays(-10), BookId = 1 },  
            new Review { Id = 2, Content = "Dystopian masterpiece", Rating = 5, ReviewerName = "CriticalReader", CreatedAt = DateTime.Now.AddDays(-5), BookId = 2 }  
        );  
    }  
}
```

# Creating Your First GraphQL Schema

Here’s where GraphQL starts to shine. Instead of thinking about endpoints, think about the graph of your data:

```csharp
public class Query  
{  
    [UseDbContext(typeof(BookContext))]  
    [UseProjection]  
    [UseFiltering]  
    [UseSorting]  
    public IQueryable<Book> GetBooks([ScopedService] BookContext context)  
        => context.Books;  
      
    [UseDbContext(typeof(BookContext))]  
    [UseProjection]  
    public IQueryable<Author> GetAuthors([ScopedService] BookContext context)  
        => context.Authors;  
      
    [UseDbContext(typeof(BookContext))]  
    public async Task<Book?> GetBookByIdAsync(  
        int id,  
        [ScopedService] BookContext context,  
        CancellationToken cancellationToken)  
        => await context.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);  
}
```

Those attributes aren’t just decoration — they’re superpowers:

*   `[UseProjection]`: Automatically converts GraphQL field selections into Entity Framework projections
*   `[UseFiltering]`: Adds dynamic filtering capabilities
*   `[UseSorting]`: Enables client-side sorting

# Wiring It All Together

In your `Program.cs`:

```csharp
var builder = WebApplication.CreateBuilder(args);  
  
builder.Services.AddDbContext<BookContext>(options =>  
    options.UseInMemoryDatabase("BookDb"));  
builder.Services  
    .AddGraphQLServer()  
    .AddQueryType<Query>()  
    .AddProjections()  
    .AddFiltering()  
    .AddSorting();  
var app = builder.Build();  
// Ensure database is created and seeded  
using (var scope = app.Services.CreateScope())  
{  
    var context = scope.ServiceProvider.GetRequiredService<BookContext>();  
    context.Database.EnsureCreated();  
}  
app.MapGraphQL();  
app.Run();
```

Fire up your application (`dotnet run`) and navigate to `https://localhost:5001/graphql`. You'll see Banana Cake Pop, Hot Chocolate's GraphQL IDE. It's like Postman, but designed specifically for GraphQL.

# Your First Query (The Magic Moment)

Try this query:

```graphql
query {  
  books {  
    title  
    isbn  
    author {  
      name  
      bio  
    }  
  }  
}
```

Look at that response. Clean, structured, with exactly the fields you requested. No more, no less.

Want to get fancy? Try filtering:

```graphql
query {  
  books(where: { title: { contains: "Harry" } }) {  
    title  
    reviews {  
      content  
      rating  
      reviewerName  
    }  
  }  
}
```

# Adding Mutations (Making Changes)

Queries are great, but what about creating and updating data? Enter mutations:

```csharp
public class Mutation  
{  
    [UseDbContext(typeof(BookContext))]  
    public async Task<Book> AddBookAsync(        AddBookInput input,  
        [ScopedService] BookContext context,  
        CancellationToken cancellationToken)  
    {  
        var book = new Book  
        {  
            Title = input.Title,  
            ISBN = input.ISBN,  
            PublishedDate = input.PublishedDate,  
            AuthorId = input.AuthorId  
        };  
          
        context.Books.Add(book);  
        await context.SaveChangesAsync(cancellationToken);  
          
        return book;  
    }  
      
    [UseDbContext(typeof(BookContext))]  
    public async Task<Review> AddReviewAsync(        AddReviewInput input,  
        [ScopedService] BookContext context,  
        CancellationToken cancellationToken)  
    {  
        var review = new Review  
        {  
            Content = input.Content,  
            Rating = input.Rating,  
            ReviewerName = input.ReviewerName,  
            BookId = input.BookId,  
            CreatedAt = DateTime.UtcNow  
        };  
          
        context.Reviews.Add(review);  
        await context.SaveChangesAsync(cancellationToken);  
          
        return review;  
    }  
}  
public record AddBookInput(string Title, string ISBN, DateTime PublishedDate, int AuthorId);  
public record AddReviewInput(string Content, int Rating, string ReviewerName, int BookId);
```

Register the mutation in your `Program.cs`:

```csharp
builder.Services  
    .AddGraphQLServer()  
    .AddQueryType<Query>()  
    .AddMutationType<Mutation>()  // Add this line  
    .AddProjections()  
    .AddFiltering()  
    .AddSorting();
```

Now you can create books:

```graphql
mutation {  
  addBook(input: {  
    title: "The Hobbit"  
    isbn: "978-0547928227"  
    publishedDate: "1937-09-21"  
    authorId: 1  
  }) {  
    id  
    title  
    author {  
      name  
    }  
  }  
}
```

# Subscriptions: Real-Time Magic

Want to blow minds? Add real-time subscriptions. Every time someone adds a review, subscribers get notified instantly:

```csharp
public class Subscription  
{  
    [Subscribe]  
    [Topic]  
    public Review OnReviewAdded([EventMessage] Review review) => review;  
}
```

Update your mutation to publish events:

```csharp
[UseDbContext(typeof(BookContext))]  
public async Task<Review> AddReviewAsync(    AddReviewInput input,  
    [ScopedService] BookContext context,  
    [Service] ITopicEventSender eventSender,  
    CancellationToken cancellationToken)  
{  
    var review = new Review  
    {  
        Content = input.Content,  
        Rating = input.Rating,  
        ReviewerName = input.ReviewerName,  
        BookId = input.BookId,  
        CreatedAt = DateTime.UtcNow  
    };  
      
    context.Reviews.Add(review);  
    await context.SaveChangesAsync(cancellationToken);  
      
    await eventSender.SendAsync(nameof(Subscription.OnReviewAdded), review, cancellationToken);  
      
    return review;  
}
```

Add subscription support:

```csharp
builder.Services  
    .AddGraphQLServer()  
    .AddQueryType<Query>()  
    .AddMutationType<Mutation>()  
    .AddSubscriptionType<Subscription>()  // Add this  
    .AddInMemorySubscriptions()           // And this  
    .AddProjections()  
    .AddFiltering()  
    .AddSorting();
```

# Advanced Patterns: DataLoaders (Solving N+1 Problems)

Here’s where GraphQL can bite you. Consider this query:

```graphql
query {  
  books {  
    title  
    author {  
      name  
    }  
  }  
}
```

Without proper handling, this creates an N+1 problem: one query for books, then N queries for each author. DataLoaders fix this elegantly:

```csharp
public class AuthorDataLoader : BatchDataLoader<int, Author>  
{  
    private readonly IDbContextFactory<BookContext> _dbContextFactory;  
      
    public AuthorDataLoader(        IDbContextFactory<BookContext> dbContextFactory,  
        IBatchScheduler batchScheduler,  
        DataLoaderOptions? options = null)  
        : base(batchScheduler, options)  
    {  
        _dbContextFactory = dbContextFactory;  
    }  
      
    protected override async Task<IReadOnlyDictionary<int, Author>> LoadBatchAsync(  
        IReadOnlyList<int> keys,  
        CancellationToken cancellationToken)  
    {  
        await using var context = _dbContextFactory.CreateDbContext();  
          
        return await context.Authors  
            .Where(a => keys.Contains(a.Id))  
            .ToDictionaryAsync(a => a.Id, cancellationToken);  
    }  
}
```

Update your Book type:

```csharp
[ExtendObjectType<Book>]  
public class BookExtensions  
{  
    public async Task<Author> GetAuthorAsync(        [Parent] Book book,  
        AuthorDataLoader authorLoader,  
        CancellationToken cancellationToken)  
        => await authorLoader.LoadAsync(book.AuthorId, cancellationToken);  
}
```

Register everything:

```csharp
builder.Services.AddDbContextFactory<BookContext>(options =>  
    options.UseInMemoryDatabase("BookDb"));  
  
builder.Services  
    .AddGraphQLServer()  
    .AddQueryType<Query>()  
    .AddMutationType<Mutation>()  
    .AddSubscriptionType<Subscription>()  
    .AddTypeExtension<BookExtensions>()  // Add this  
    .AddDataLoader<AuthorDataLoader>()   // And this  
    .AddInMemorySubscriptions()  
    .AddProjections()  
    .AddFiltering()  
    .AddSorting();
```

Now that same query executes in exactly two database calls, regardless of how many books you have.

# Authentication & Authorization

Real applications need security. Hot Chocolate integrates beautifully with ASP.NET Core’s authentication:

```csharp
[Authorize]  
public class Mutation  
{  
    [Authorize(Roles = "Admin")]  
    public async Task<Book> AddBookAsync(/* parameters */) { /* implementation */ }  
      
    [Authorize] // Any authenticated user can add reviews  
    public async Task<Review> AddReviewAsync(/* parameters */) { /* implementation */ }  
}
```

# Error Handling Done Right

GraphQL has a different approach to errors. Instead of HTTP status codes, errors are part of the response:

```csharp
public class BookNotFoundException : Exception  
{  
    public BookNotFoundException(int bookId)   
        : base($"Book with ID {bookId} was not found.")  
    {  
        BookId = bookId;  
    }  
      
    public int BookId { get; }  
}  
  
public class BookNotFoundError : IErrorFilter  
{  
    public IError OnError(IError error)  
    {  
        if (error.Exception is BookNotFoundException ex)  
        {  
            return error.WithMessage($"The book you're looking for doesn't exist.")  
                       .WithCode("BOOK_NOT_FOUND")  
                       .SetExtension("bookId", ex.BookId);  
        }  
          
        return error;  
    }  
}
```

Register the error filter:

```csharp
builder.Services  
    .AddGraphQLServer()  
    // ... other configuration  
    .AddErrorFilter<BookNotFoundError>();
```

# Performance Optimization: The Devil’s in the Details

# 1\. Query Complexity Analysis

Prevent malicious queries from bringing down your server:

```csharp
builder.Services  
    .AddGraphQLServer()  
    // ... other configuration  
    .AddQueryRequestInterceptor(async (context, requestExecutor, requestBuilder, cancellationToken) =>  
    {  
        requestBuilder.AddProperty("complexity", 100); // Max complexity  
    })  
    .AddMaxExecutionDepthRule(10, skipIntrospectionFields: true);
```

# 2\. Persisted Queries

For production apps, consider persisted queries to reduce payload size and improve security:

```csharp
builder.Services  
    .AddGraphQLServer()  
    // ... other configuration  
    .AddReadOnlyFileSystemQueryStorage("./queries")  
    .UsePersistedQueryPipeline();
```

# 3\. Response Caching

Cache expensive operations:

```csharp
[UseDbContext(typeof(BookContext))]  
[UsePaging]  
[UseProjection]  
[UseFiltering]  
[UseSorting]  
[GraphQLName("books")]  
public IQueryable<Book> GetBooks([ScopedService] BookContext context)  
    => context.Books;
```

# Testing Your GraphQL API

Testing GraphQL is different from testing REST. Here’s a clean approach:

```csharp
[Fact]  
public async Task GetBooks_ReturnsAllBooks()  
{  
    // Arrange  
    var result = await ExecuteRequestAsync(b => b.SetQuery(@"  
        query {  
            books {  
                title  
                author {  
                    name  
                }  
            }  
        }"));  
      
    // Act & Assert  
    result.MatchSnapshot();  
}  
  
private async Task<IExecutionResult> ExecuteRequestAsync(Action<IQueryRequestBuilder> configureRequest)  
{  
    var requestBuilder = QueryRequestBuilder.New();  
    configureRequest(requestBuilder);  
      
    return await TestServer.ExecuteAsync(requestBuilder.Create());  
}
```

# Deployment Considerations

When you’re ready to ship:

# 1\. Disable Introspection in Production

```csharp
builder.Services  
    .AddGraphQLServer()  
    // ... other configuration  
    .ModifyRequestOptions(opt => opt.IncludeExceptionDetails = !app.Environment.IsProduction())  
    .AddIntrospection(!app.Environment.IsProduction());
```

# 2\. Add CORS for Web Clients

```csharp
builder.Services.AddCors(options =>  
{  
    options.AddDefaultPolicy(builder =>  
    {  
        builder.AllowAnyOrigin()  
               .AllowAnyHeader()  
               .AllowAnyMethod();  
    });  
});  
  
app.UseCors();
```

# 3\. Consider Rate Limiting

```csharp
builder.Services.Configure<RequestExecutorOptions>(options =>  
{  
    options.ExecutionTimeout = TimeSpan.FromSeconds(30);  
});
```

# The Client Side Story

Your beautiful GraphQL API deserves an equally elegant client. For .NET applications, consider **StrawberryShake** (Hot Chocolate’s client):

```bash
dotnet add package StrawberryShake.AspNetCore
```

Generate strongly-typed clients from your schema:

```csharp
[GraphQLName("BookApi")]  
public interface IBookApiClient  
{  
    Task<IGetBooksResult> GetBooksAsync(CancellationToken cancellationToken = default);  
    Task<IAddBookResult> AddBookAsync(AddBookInput input, CancellationToken cancellationToken = default);  
}
```

# Common Pitfalls (Learn from Others’ Pain)

# 1\. Over-fetching in Disguise

Just because you can request any field doesn’t mean you should. Design your queries thoughtfully.

# 2\. Ignoring the N+1 Problem

Always implement DataLoaders for related data. Your database will thank you.

# 3\. Forgetting About Caching

GraphQL’s flexibility can make HTTP caching tricky. Plan your caching strategy early.

# 4\. Exposing Too Much

Not every database field should be a GraphQL field. Think about your API surface carefully.

# What We’ve Built

You now have a fully functional GraphQL API with:

*   Type-safe queries, mutations, and subscriptions
*   Advanced filtering and sorting
*   Efficient data loading with DataLoaders
*   Real-time capabilities
*   Authentication and authorization
*   Proper error handling
*   Performance optimizations

But more importantly, you understand the _why_ behind each decision.

# The Road Ahead

GraphQL isn’t just a technology choice — it’s a paradigm shift. You’re not just building APIs anymore; you’re building a query language for your data. Your frontend teams can move faster, your mobile apps can be more efficient, and your API evolves without breaking existing clients.

The learning curve is real, but so are the benefits. Start small, perhaps with a single domain like we did with books, and gradually expand. Your future self (and your teammates) will thank you.