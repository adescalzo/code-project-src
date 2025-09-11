```yaml
---
title: "Specification Pattern in EF Core: Flexible Data Access Without Repositories"
source: https://antondevtips.com/blog/specification-pattern-in-ef-core-flexible-data-access-without-repositories?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2115
date_published: 2025-08-26T07:45:52.227Z
date_captured: 2025-09-09T18:36:32.752Z
domain: antondevtips.com
author: Anton Martyniuk
category: ai_ml
technologies: [EF Core, .NET, ASP.NET Core, Entity Framework Extensions, SQL Server]
programming_languages: [C#, SQL]
tags: [specification-pattern, ef-core, data-access, repository-pattern, dotnet, query-composition, software-architecture, design-patterns, clean-code, orm]
key_concepts: [Specification Pattern, Repository Pattern, Unit of Work Pattern, Separation of Concerns, Expression Trees, Queryable Extensions, Eager Loading, Dependency Injection]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article introduces the Specification Pattern as a robust alternative to the traditional Repository Pattern for managing complex data access with EF Core in .NET applications. It explains how repositories can become unwieldy and difficult to maintain as project requirements evolve, leading to code duplication and large classes. The Specification Pattern is presented as a solution, enabling the creation of small, reusable, and composable classes that encapsulate filtering, inclusion, and sorting logic. The post provides a detailed implementation guide, including how to apply specifications directly to DbContext and advanced techniques for combining them using logical operators and Expression Trees. This approach promotes cleaner, more flexible, and testable data access code, ultimately enhancing project maintainability.]
---
```

# Specification Pattern in EF Core: Flexible Data Access Without Repositories

![A dark blue banner with abstract purple shapes. On the left, a white square icon with `</>` inside, and below it, the text "dev tips" in white. On the right, the title of the article is displayed in large white text: "SPECIFICATION PATTERN IN EF CORE: FLEXIBLE DATA ACCESS WITHOUT REPOSITORIES."](https://antondevtips.com/media/covers/efcore/cover_efcore_specifications.png)

# Specification Pattern in EF Core: Flexible Data Access Without Repositories

Aug 26, 2025

[Download source code](/source-code/specification-pattern-in-ef-core-flexible-data-access-without-repositories)

8 min read

### Newsletter Sponsors

Bulk Insert, Update, Delete & Merge — seamlessly built for [EF Core](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=august-2025).  
→ [Explore Entity Framework Extensions](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=august-2025)

[Sponsor my newsletter to reach 13,000+ readers](/sponsorship)

As your .NET projects grow, handling data gets more and more complicated. Many teams start with the Repository Pattern, wrapping their EF Core queries inside.

At first, this works fine. But as your project grows, your Repositories either don't do enough or try to do too much. Your code becomes harder to understand and modify as business requirements change.

Each time you need a new filter or query, you add another method or even a new repository.

How can you solve this issue? The answer is a **Specification** pattern.

In this post, you will learn how the **Specification** Pattern works in real .NET projects, why it is better than writing lots of repositories for reading data.

In this post, we will explore:

*   Why Repositories Become a Bottleneck in Real Projects
*   What Is the Specification Pattern?
*   How to Implement Specifications in EF Core
*   Advanced Specifications

Let's dive in!

## Why Repositories Become a Bottleneck in Real Projects

When your application is small, using the Repository Pattern seems easy.

You put all your data queries in one place, such as a `PostRepository` or `UserRepository`. Each method is designed to answer a specific question, such as "Get all recent posts" or "Find user by email".

But as your project grows, you will notice a few big problems:

**1\. Repositories Become Too Large**

Every new business requirement means adding another method to the Repository. Over time, you end up with classes full of similar methods:

```csharp
public class PostRepository
{
    public Task<List<Post>> GetPostsByUser(int userId) { ... }
    public Task<List<Post>> GetPopularPosts() { ... }
    public Task<List<Post>> GetPostsByCategory(string category) { ... }
    public Task<List<Post>> GetRecentViralPosts(int daysBack) { ... }
    // ...and many more!
}
```

It gets harder to find the correct method or even remember what's already in every repository.

**2\. Method Naming and Duplication**

You start writing awkward and long method names to describe every filter. For example, `GetPostsByCategoryAndLikesCountAndDate`.

And what if you need the same method but with sorting by date in descending order? This results in a lot of code duplication.

**3\. Too Many Repositories**

Trying to keep repositories small sometimes means splitting them into many tiny classes. But then you lose the benefit of having everything in one place.

Now, it's harder to find the necessary repository and reuse logic between repositories.

Remember that EF Core's DbContext already implements the Repository and Unit of Work patterns, as stated in the official DbContext's code summary. When we create a repository over EF Core, we create an abstraction over an abstraction, leading to over-engineered solutions.

Let's explore what a **Specification** pattern is and how it solves our problem.

## What Is the Specification Pattern?

The **Specification** Pattern is a way to describe what data you want from your database using small, reusable classes called "specifications".

Each Specification represents a filter or a rule that can be applied to a query. This lets you build complex queries by combining simple, easy-to-understand classes.

The Specification Pattern brings the following benefits to the table:

*   Reusability: You can write a specification once and use it anywhere in your project.
*   Combination: You can combine two or more specifications to make more advanced queries.
*   Testability: Specifications are classes over EF Core (or any other ORM), so you can cover them with unit tests, or even better - integration tests.
*   Separation of Concerns: Your query logic is separated from your data access code. This keeps things clean.

Instead of writing dozens of methods in your Repository, you can just create new specifications as your requirements grow. You can then pass these specifications to your DbContext (or even a repository, if you still want to use one).

Here is an example of a Specification that returns viral posts in a social media application with at least 150 likes:

```csharp
public class ViralPostSpecification : Specification<Post>
{
    public ViralPostSpecification(int minLikesCount = 150)
    {
        AddFilteringQuery(post => post.Likes.Count >= minLikesCount);
        AddOrderByDescendingQuery(post => post.Likes.Count);
    }
}
```

You can reuse this Specification anywhere in your code to get "viral" posts.

Let's explore how to implement Specifications.

## How to Implement Specifications in EF Core

To implement specifications in EF Core, you need to follow these steps:

**Step 1: Define a Specification Interface**

You need a common way to describe filters, includes, and sorting for any entity. Here's a simple interface:

```csharp
public interface ISpecification<TEntity>
    where TEntity : class
{
    Expression<Func<TEntity, bool>>? FilterQuery { get; }
    IReadOnlyCollection<Expression<Func<TEntity, object>>>? IncludeQueries { get; }
    IReadOnlyCollection<Expression<Func<TEntity, object>>>? OrderByQueries { get; }
    IReadOnlyCollection<Expression<Func<TEntity, object>>>? OrderByDescendingQueries { get; }
}
```

**Step 2: Create a Base Specification Class**

This base class holds the logic for building up specifications. You add filters, includes, and sort expressions in one place:

```csharp
using System.Linq.Expressions;

public abstract class Specification<TEntity> : ISpecification<TEntity>
    where TEntity : class
{
    private List<Expression<Func<TEntity, object>>>? _includeQueries;
    private List<Expression<Func<TEntity, object>>>? _orderByQueries;
    private List<Expression<Func<TEntity, object>>>? _orderByDescendingQueries;

    public Expression<Func<TEntity, bool>>? FilterQuery { get; private set; }
    public IReadOnlyCollection<Expression<Func<TEntity, object>>>? IncludeQueries => _includeQueries;
    public IReadOnlyCollection<Expression<Func<TEntity, object>>>? OrderByQueries => _orderByQueries;
    public IReadOnlyCollection<Expression<Func<TEntity, object>>>? OrderByDescendingQueries => _orderByDescendingQueries;

    protected Specification() {}

    protected Specification(Expression<Func<TEntity, bool>> query)
    {
        FilterQuery = query;
    }

    protected Specification(ISpecification<TEntity> specification)
    {
        FilterQuery = specification.FilterQuery;

        _includeQueries = specification.IncludeQueries?.ToList();
        _orderByQueries = specification.OrderByQueries?.ToList();
        _orderByDescendingQueries = specification.OrderByDescendingQueries?.ToList();
    }

    protected void AddFilteringQuery(Expression<Func<TEntity, bool>> query)
    {
        FilterQuery = query;
    }

    protected void AddIncludeQuery(Expression<Func<TEntity, object>> query)
    {
        _includeQueries ??= new();
        _includeQueries.Add(query);
    }

    protected void AddOrderByQuery(Expression<Func<TEntity, object>> query)
    {
        _orderByQueries ??= new();
        _orderByQueries.Add(query);
    }

    protected void AddOrderByDescendingQuery(Expression<Func<TEntity, object>> query)
    {
        _orderByDescendingQueries ??= new();
        _orderByDescendingQueries.Add(query);
    }
}
```

Here is a description of each option:

*   `FilterQuery`: A filtering function to test each entity for a condition
*   `IncludeQueries`: A collection of functions that describe included entities
*   `OrderByQueries`, `OrderByDescendingQueries`: A function that describes how to order entities by ascending/descending order

**Step 3: Applying a Specification in EF Core**

Now let's connect our `Specification` class to EF Core:

```csharp
public class EfCoreSpecification<TEntity> : Specification<TEntity>
    where TEntity : class
{
    public EfCoreSpecification(ISpecification<TEntity> specification)
        : base(specification) { }
    
    public virtual IQueryable<TEntity> Apply(IQueryable<TEntity> queryable)
    {
        if (FilterQuery is not null)
        {
            queryable = queryable.Where(FilterQuery);
        }

        if (IncludeQueries?.Count > 0)
        {
            queryable = IncludeQueries.Aggregate(queryable,
                (current, includeQuery) => current.Include(includeQuery));
        }

        if (OrderByQueries?.Count > 0)
        {
            var orderedQueryable = queryable.OrderBy(OrderByQueries.First());
            
            orderedQueryable = OrderByQueries.Skip(1)
                .Aggregate(orderedQueryable, (current, orderQuery) => current.ThenBy(orderQuery));
            
            queryable = orderedQueryable;
        }
        
        if (OrderByDescendingQueries?.Count > 0)
        {
            var orderedQueryable = queryable.OrderByDescending(OrderByDescendingQueries.First());
            
            orderedQueryable = OrderByDescendingQueries.Skip(1)
                .Aggregate(orderedQueryable, (current, orderQuery) => current.ThenByDescending(orderQuery));
            
            queryable = orderedQueryable;
        }

        return queryable;
    }
}
```

The `Apply` method takes a database query and adds different types of filters and sorting to it step by step.

First, it checks if there's a filter condition (like "show only posts with more than 10 likes") and applies it using the `Where` method.

Next, it looks for any related data that needs to be loaded together (called "includes") and adds those using the `Include` method (EF Core Eager Loading).

Then it handles sorting - it applies the first one with `OrderBy` and any additional ones with `ThenBy`.

Finally, the method returns the modified query with all these conditions applied, ready to be sent to the database.

**Step 4: Use Specifications Directly in Your Endpoints**

With this pattern, you put your query logic into small, reusable classes called specifications. You can use these directly with DbContext in EF Core. That means you don't need the Repository Pattern at all.

Now you can write simple, clean endpoints:

```csharp
public class GetViralPostsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/social-media/viral-posts", Handle);
    }

    private static async Task<IResult> Handle(
        [FromQuery] int? minLikesCount,
        [FromServices] ApplicationDbContext dbContext,
        [FromServices] ILogger<GetViralPostsEndpoint> logger,
        CancellationToken cancellationToken)
    {
        var specification = new ViralPostSpecification(minLikesCount ?? 150);

        var response = await dbContext
            .ApplySpecification(specification)
            .Select(post => post.ToDto())
            .ToListAsync(cancellationToken);

        logger.LogInformation("Retrieved {Count} viral posts with minimum {MinLikes} likes",
            response.Count, minLikesCount ?? 150);

        return Results.Ok(response);
    }
}
```

I apply the Specification on DbContext with a small extension method:

```csharp
public static class SpecificationExtensions
{
    public static IQueryable<TEntity> ApplySpecification<TEntity>(
        this ApplicationDbContext dbContext,
        ISpecification<TEntity> specification) where TEntity : class
    {
        var efCoreSpecification = new EfCoreSpecification<TEntity>(specification);

        var query = dbContext.Set<TEntity>().AsNoTracking();
        query = efCoreSpecification.Apply(query);

        return query;
    }
}
```

If you still want to use a Repository pattern, you can materialize the specification query and return the result from your Repository:

```csharp
public abstract class BaseRepository<TEntity> where TEntity : class
{
    private readonly ApplicationDbContext _dbContext;

    public Repository(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<TEntity>> WhereAsync(
        ISpecification<TEntity> specification,
        CancellationToken cancellationToken = default)
    {
        var efCoreSpecification = new EfCoreSpecification<TEntity>(specification);

        var query = _dbContext.Set<TEntity>().AsNoTracking();
        query = efCoreSpecification.Apply(query);

        return await query.ToListAsync(cancellationToken).ConfigureAwait(false);
    }
}
```

## Advanced Specifications

One of the best things about the Specification Pattern is how easily you can build advanced queries by combining small, focused specifications.

You can join two or more specifications together using logical operators like **AND** and **OR**.

Here is how to create `AndSpecification` and `OrSpecification`:

```csharp
public class AndSpecification<TEntity> : Specification<TEntity>
    where TEntity : class
{
    public AndSpecification(Specification<TEntity> left, Specification<TEntity> right)
    {
        RegisterFilteringQuery(left, right);
    }

    private void RegisterFilteringQuery(Specification<TEntity> left, Specification<TEntity> right)
    {
        var leftExpression = left.FilterQuery;
        var rightExpression = right.FilterQuery;

        if (leftExpression is null && rightExpression is null)
        {
            return;
        }
        
        if (leftExpression is not null && rightExpression is null)
        {
            AddFilteringQuery(leftExpression);
            return;
        }
        
        if (leftExpression is null && rightExpression is not null)
        {
            AddFilteringQuery(rightExpression);
            return;
        }
        
        var replaceVisitor = new ReplaceExpressionVisitor(rightExpression!.Parameters.Single(), leftExpression!.Parameters.Single());
        var replacedBody = replaceVisitor.Visit(rightExpression.Body);

        var andExpression = Expression.AndAlso(leftExpression.Body, replacedBody);
        var lambda = Expression.Lambda<Func<TEntity, bool>>(andExpression, leftExpression.Parameters.Single());

        AddFilteringQuery(lambda);
    }
}
```
```csharp
public class OrSpecification<TEntity> : Specification<TEntity>
    where TEntity : class
{
    public OrSpecification(Specification<TEntity> left, Specification<TEntity> right)
    {
        RegisterFilteringQuery(left, right);
    }

    private void RegisterFilteringQuery(Specification<TEntity> left, Specification<TEntity> right)
    {
        var leftExpression = left.FilterQuery;
        var rightExpression = right.FilterQuery;

        if (leftExpression is null && rightExpression is null)
        {
            return;
        }
        
        if (leftExpression is not null && rightExpression is null)
        {
            AddFilteringQuery(leftExpression);
            return;
        }
        
        if (leftExpression is null && rightExpression is not null)
        {
            AddFilteringQuery(rightExpression);
            return;
        }
        
        var replaceVisitor = new ReplaceExpressionVisitor(
            rightExpression!.Parameters.Single(),
            leftExpression!.Parameters.Single()
        );
        
        var replacedBody = replaceVisitor.Visit(rightExpression.Body);

        var andExpression = Expression.OrElse(leftExpression.Body, replacedBody);
        
        var lambda = Expression.Lambda<Func<TEntity, bool>>(
            andExpression, leftExpression.Parameters.Single());

        AddFilteringQuery(lambda);
    }
}
```

To combine two queries, we need to use `Expression.AndAlso` or `Expression.OrElse` with an expression Visitor:

```csharp
internal class ReplaceExpressionVisitor : ExpressionVisitor
{
    private readonly Expression _oldValue;
    private readonly Expression _newValue;

    /// <summary>
    /// Initializes a new instance of the class
    /// </summary>
    /// <param name="oldValue">Old expression to be replaced</param>
    /// <param name="newValue">A new expression that replaces the old one</param>
    public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
    {
        _oldValue = oldValue;
        _newValue = newValue;
    }

    public override Expression Visit(Expression? node)
        => (node == _oldValue ? _newValue : base.Visit(node))!;
}
```

To simplify these Specifications' usage, let's create 2 helper methods in the `Specification` class:

```csharp
public Specification<TEntity> And(Specification<TEntity> specification)
    => new AndSpecification<TEntity>(this, specification);

public Specification<TEntity> Or(Specification<TEntity> specification)
    => new OrSpecification<TEntity>(this, specification);
```

Now let's explore a practical example.

Let's say you have a Specification that searches for posts in a given category:

```csharp
public class PostByCategorySpecification : Specification<Post>
{
    public PostByCategorySpecification(string categoryName)
    {
        AddFilteringQuery(post => post.Category.Name == categoryName);
    }
}
```

You can combine specifications to select posts that belong to ".NET" **or** "Architecture" categories:

```csharp
public class DotNetAndArchitecturePostSpecification : Specification<Post>
{
    public DotNetAndArchitecturePostSpecification()
    {
        var dotNetSpec = new PostByCategorySpecification(".NET");
        var architectureSpec = new PostByCategorySpecification("Architecture");

        // Combine 2 specifications with OrSpecification
        var combinedSpec = dotNetSpec.Or(architectureSpec);

        AddFilteringQuery(combinedSpec.FilterQuery!);

        AddOrderByDescendingQuery(post => post.Id);
    }
}
```

Another example, where we select posts that are both recent **and** have high engagement:

```csharp
public class HighEngagementRecentPostSpecification : Specification<Post>
{
    public HighEngagementRecentPostSpecification(int daysBack = 7,
        int minLikes = 100, int minComments = 30)
    {
        var recentSpec = new RecentPostSpecification(daysBack);
        var highEngagementSpec = new HighEngagementPostSpecification(minLikes, minComments);

        // Combine 2 specifications with AndSpecification
        var combinedSpec = recentSpec.And(highEngagementSpec);

        AddFilteringQuery(combinedSpec.FilterQuery!);

        AddOrderByDescendingQuery(post => post.Likes.Count + post.Comments.Count);
    }
}
```

This provides us with the flexibility and reusability of existing Specifications to form new specifications.

**Note:** Adding Include queries like `AddIncludeQuery(post => post.Category);` is not necessary here, as we do not materialize our database query in the Specification itself, as we use Select Projection:

If you use repositories and want one universal method that applies specifications, you have 2 options:

*   Add Include queries, but it will result in returning too much data from the database in the `WhereAsync` method
*   Push a delegate with a mapping function to the `WhereAsync` method in the Repository

Both of these approaches come with trade-offs; that's why I prefer to work with EF Core without a Repository pattern.

So our API endpoints (or Application handlers or services, depending on the project complexity) will be as simple as this:

```csharp
public class GetDotNetAndArchitecturePostsEndpoint : IEndpoint
{
    public void MapEndpoint(WebApplication app)
    {
        app.MapGet("/api/social-media/dotnet-architecture-posts", Handle);
    }

    private static async Task<IResult> Handle(
        ApplicationDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var specification = new DotNetAndArchitecturePostSpecification();

        var response = await dbContext
            .ApplySpecification(specification)
            .Select(post => post.ToDto())
            .ToListAsync(cancellationToken);

        return Results.Ok(response);
    }
}
```

> P.S.: You can find more real-world Specifications for social media in the source code at the end of the post

## Summary

The Specification Pattern is a powerful tool for building flexible and reusable database queries in your .NET projects. By defining your filters, includes, and sorting rules as specification classes, you avoid the problems that come with large, hard-to-maintain repositories.

With EF Core, you don't need the Repository Pattern — you can apply your specifications directly to the DbContext.

This approach:

*   Keeps your codebase clean and easy to change
*   Makes your queries reusable across many parts of your application
*   Lets you combine and compose specifications for advanced scenarios
*   Helps you test your query logic separately from the database

Whenever you find yourself adding more and more methods to a repository or writing duplicate query logic, consider using the Specification Pattern instead. It will help your project grow in a healthy, maintainable way.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/specification-pattern-in-ef-core-flexible-data-access-without-repositories)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fspecification-pattern-in-ef-core-flexible-data-access-without-repositories&title=Specification%20Pattern%20in%20EF%20Core%3A%20Flexible%20Data%20Access%20Without%20Repositories)[X](https://twitter.com/intent/tweet?text=Specification%20Pattern%20in%20EF%20Core%3A%20Flexible%20Data%20Access%20Without%20Repositories&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fspecification-pattern-in-ef-core-flexible-data-access-without-repositories)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fspecification-pattern-in-ef-core-flexible-data-access-without-repositories)

# Improve Your **.NET** and Architecture Skills

Join my community of **13,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 13,000+ Subscribers

Join 13,000+ developers already reading

No spam. Unsubscribe any time.