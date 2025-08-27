```yaml
---
title: How I Optimized EF Core Query from 30 Seconds to 30 Milliseconds
source: https://antondevtips.com/blog/ef-core-query-optimization-from-30-seconds-to-30-milliseconds
date_published: 2025-06-24T07:45:24.112Z
date_captured: 2025-08-06T16:22:45.845Z
domain: antondevtips.com
author: Anton Martyniuk
category: performance
technologies: [EF Core, PostgreSQL, .NET, Entity Framework Extensions]
programming_languages: [C#, SQL]
tags: [ef-core, performance-optimization, database, query-optimization, dotnet, linq, sql, data-access, benchmarking]
key_concepts: [query-optimization, eager-loading, client-side-evaluation, server-side-evaluation, projection, n+1-problem, cartesian-explosion, split-query]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article details a step-by-step process to optimize a slow EF Core database query, reducing its execution time from 30 seconds to 30 milliseconds. It demonstrates various optimization techniques, starting from pre-filtering data and limiting results, to more advanced strategies like projecting only required columns and structuring queries in multiple phases. The author compares single-query projections, split queries, and multi-phase approaches, analyzing their impact on both performance and memory allocation. Key takeaways emphasize the importance of filtering data early, using two-phase projections, and understanding the trade-offs between query complexity and execution efficiency. The post also highlights the critical role of benchmarking and profiling in identifying and resolving performance bottlenecks.]
---
```

# How I Optimized EF Core Query from 30 Seconds to 30 Milliseconds

![A banner image for the article titled "How I Optimized EF Core Query from 30 Seconds to 30 Milliseconds" with a "dev tips" logo.](/_next/image?url=https%3A%2F%2Fantondevtips.com%2Fmedia%2Fcovers%2Fefcore%2Fcover_efcore_optimization_challenge.png&w=3840&q=100)

# How I Optimized EF Core Query from 30 Seconds to 30 Milliseconds

Jun 24, 2025

[Download source code](/source-code/ef-core-query-optimization-from-30-seconds-to-30-milliseconds)

12 min read

### Newsletter Sponsors

[EF Core is too slow?](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=june-2025) Discover how you can easily insert 14x faster (reducing saving time by 94%).  
Boost your performance with our method integrated within EF Core: Bulk Insert, update, delete, and merge.  
Join 5,000+ satisfied customers who have trusted our library since 2014.

[Learn more](https://entityframework-extensions.net?utm_source=antondevtips&utm_medium=newsletter&utm_campaign=june-2025)

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

Performance is crucial for any application.

Developers often add a Caching Layer over slow database queries. They are hiding symptoms instead of fixing the root problem.

In this post, we will do a challenge: how to optimize a slow real-world EF Core query. EF Core offers great tools but can lead to slow queries if used improperly.

I will show you how I optimized EF Core query step-by-step from an unacceptable **30 seconds** down to a blazing-fast **30 milliseconds**.

Let's dive in!

[](#challenge-and-the-slow-query)

## Challenge and the Slow Query

We will explore a Social Media platform where we have the following entities:

```csharp
public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = null!;
    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

public class Post
{
    public int Id { get; set; }
    public string Content { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int CategoryId { get; set; }
    public Category Category { get; set; } = null!;
    public ICollection<Like> Likes { get; set; } = new List<Like>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}

public class Category
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public ICollection<Post> Posts { get; set; } = new List<Post>();
}

public class Comment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
    public string Text { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class Like
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public Post Post { get; set; } = null!;
}
```

These entities have the following relations:

*   Users: Each user has many posts and many comments.
*   Comments: Each comment belongs to a user and is linked to a post.
*   Categories: Posts are categorized.
*   Posts: Each post has a category and can have multiple likes.
*   Likes: Each like is associated with a post.

Here is our challenge requirement:

*   Select the top 5 users who made the most comments in the last 7 days on posts in the ".NET" category.

For each user return:

*   UserId
*   Username
*   Count of comments by a user (only comments on ".NET" posts in the last 7 days)
*   Top 3 ".NET" posts, by likes, each user commented on most (PostId, LikesCount)

Here's the initial very slow query:

```csharp
public List<ActiveUserDto> GetTopCommenters_Slow()
{
    var since = DateTime.UtcNow.AddDays(-7);

    // 1) Eagerly load every user + all their comments → post → category → likes
    var users = _dbContext.Users
        .Include(u => u.Comments)
            .ThenInclude(c => c.Post)
                .ThenInclude(p => p.Category)
        .Include(u => u.Comments)
            .ThenInclude(c => c.Post)
                .ThenInclude(p => p.Likes)
        .ToList();

    var result = new List<ActiveUserDto>();

    foreach (var u in users)
    {
        // 2) Filter to recent ".NET" comments
        var comments = u.Comments
            .Where(c =>
                c.CreatedAt >= since &&
                c.Post.Category.Name == ".NET")
            .ToList();

        var commentsCount = comments.Count;
        if (commentsCount == 0)
        {
            continue;
        }

        // 3) Top 3 posts by like count
        var topPosts = comments
            .GroupBy(c => c.Post)
            .Select(g => new PostDto(
                g.Key.Id,
                _dbContext.Likes.Count(l => l.PostId == g.Key.Id)))
            .OrderByDescending(p => p.LikesCount)
            .Take(3)
            .ToList();

        // 4) Latest 2 comments _on those top 3 posts_
        var topPostIds = topPosts.Select(p => p.PostId).ToList();
        
        var recentTexts = _dbContext.Comments
            .Where(c =>
                c.UserId == u.Id &&
                c.CreatedAt >= since &&
                topPostIds.Contains(c.PostId))
            .OrderByDescending(c => c.CreatedAt)
            .Take(2)
            .Select(c => c.Text)
            .ToList();

        result.Add(new ActiveUserDto(
            u.Id, u.Username,
            commentsCount,
            topPosts,
            recentTexts));
    }

    // 5) Final top-5 in memory
    return result
        .OrderByDescending(x => x.CommentsCount)
        .Take(5)
        .ToList();
}
```

This implementation loads everything eagerly into memory and then performs extensive filtering, sorting, and aggregation purely client-side. This approach results in massive datasets being transferred from the database to the application, consuming a large amount of memory and dramatically slowing performance.

This query runs in **29-30 s** in **Postgres** database.

> Note: every benchmark result depends on your PC hardware and a database provider and location

[](#optimization-1-prefilter-users)

## Optimization 1: Pre-Filter Users

We loaded every user and all their comments, even those who hadn't commented on ".NET" posts in the last week.

Let's add a filter to return only users who actually made comments on ".NET" posts.

This simple filter cuts the number of records retrieved from the database, and thus other requests will be faster:

```csharp
/// <summary>
/// Optimization 1: Pre-filter users who have any recent .NET comments
/// </summary>
public List<ActiveUserDto> GetTopCommenters_Optimization1_PreFilter()
{
    var since = DateTime.UtcNow.AddDays(-7);

    // Only users with at least one .NET comment in the window
    var users = _dbContext.Users
        .Where(u => u.Comments
            .Any(c =>
                c.CreatedAt >= since &&
                c.Post.Category.Name == ".NET"))
        .Include(u => u.Comments)
            .ThenInclude(c => c.Post)
                .ThenInclude(p => p.Category)
        .Include(u => u.Comments)
            .ThenInclude(c => c.Post)
                .ThenInclude(p => p.Likes)
        .ToList();

    // The same code as in the slow query
}
```

Let's run the query and compare the performance:

![A benchmark.NET results table showing the performance of 'Optimization 1: Pre-filter users' at 27.16 seconds and 'Optimization 2: Limit users in SQL' at 17.85 seconds, along with memory allocation.](https://antondevtips.com/media/code_screenshots/efcore/optimization_challenge/img_1.png)

We won just 1 second, but it can be more on bigger datasets.

Let's improve further.

[](#optimization-2-limit-top5-users)

## Optimization 2: Limit Top-5 Users

After we've filtered out users in the previous step, the next step is to tell the database to only return the top 5 commenters by their activity. That way, we never load more than 5 users we actually care about.

```csharp
/// <summary>
/// Optimization 2: Limit users (take top-5 by comment count early)
/// </summary>
public List<ActiveUserDto> GetTopCommenters_Optimization2_LimitUsers()
{
    var since = DateTime.UtcNow.AddDays(-7);

    // Compute comment count in the database, then take top 5
    var users = _dbContext.Users
        .Where(u => u.Comments
            .Any(c =>
                c.CreatedAt >= since &&
                c.Post.Category.Name == ".NET"))
        .OrderByDescending(u => u.Comments
            .Count(c =>
                c.CreatedAt >= since &&
                c.Post.Category.Name == ".NET")
        )
        .Take(5)
        .Include(u => u.Comments)
            .ThenInclude(c => c.Post)
                .ThenInclude(p => p.Category)
        .Include(u => u.Comments)
            .ThenInclude(c => c.Post)
                .ThenInclude(p => p.Likes)
        .ToList();

    // The same code as in the slow query
}
```

Here we sort users by the most comments and get only top-5 users back.

After we run the benchmarks, we see that we are down from 27 s to **17 s**.

![A benchmark.NET results table showing the performance of 'Optimization 2: Limit users in SQL' at 17.07 seconds and 'Optimization 3: Filter child comments at query time' at 10.03 seconds, along with memory allocation.](https://antondevtips.com/media/code_screenshots/efcore/optimization_challenge/img_2.png)

That's a significant performance boost.

[](#optimization-3-limit-the-number-of-joins)

## Optimization 3: Limit the number of JOINs

Instead of JOINing all tables together: users, comments, posts, categories, likes - let's join only the needed data:

```csharp
/// <summary>
/// Optimization 3: Filter child comments at query time
/// </summary>
public List<ActiveUserDto> GetTopCommenters_Optimization3_FilterComments()
{
    var since = DateTime.UtcNow.AddDays(-7);

    // Only include comments that match our filter
    var users = _dbContext.Users
        .Include(u => u.Comments.Where(c =>
            c.CreatedAt >= since &&
            c.Post.Category.Name == ".NET"))
        .ThenInclude(c => c.Post)
            .ThenInclude(p => p.Likes)
        .Where(u => u.Comments.Any())
        .OrderByDescending(u => u.Comments
            .Count(c =>
                c.CreatedAt >= since &&
                c.Post.Category.Name == ".NET")
        )
        .Take(5)
        .ToList();

    var result = new List<ActiveUserDto>();
    foreach (var u in users)
    {
        // u.Comments now only contains recent .NET comments
        var comments = u.Comments.ToList();
        var commentsCount = comments.Count;

        var topPosts = comments
            .GroupBy(c => c.Post)
            .Select(g => new PostDto(
                g.Key.Id,
                g.Key.Likes.Count))
            .OrderByDescending(p => p.LikesCount)
            .Take(3)
            .ToList();

        var topPostIds = topPosts.Select(p => p.PostId).ToList();
        var recentTexts = _dbContext.Comments
            .Where(c =>
                c.UserId == u.Id &&
                c.CreatedAt >= since &&
                topPostIds.Contains(c.PostId))
            .OrderByDescending(c => c.CreatedAt)
            .Take(2)
            .Select(c => c.Text)
            .ToList();

        result.Add(new ActiveUserDto(
            u.Id,
            u.Username,
            commentsCount,
            topPosts,
            recentTexts));
    }

    return result;
}
```

After running the benchmarks you can see that we are down to **10 s** and consume almost twice less memory:

![A benchmark.NET results table showing the performance of 'Optimization 3: Filter child comments at query time' at 10.03 seconds and 'Optimization 4: Project only required columns' at 42.68 milliseconds, along with memory allocation.](https://antondevtips.com/media/code_screenshots/efcore/optimization_challenge/img_3.png)

[](#optimization-4-project-only-required-columns)

## Optimization 4: Project Only Required Columns

Let's optimize further and select only the needed columns.

First, we ask for just the top 5 users and their total comment counts. Then, in a loop, we run two small queries:

*   Top 3 posts they commented on (by likes).
*   Latest 2 comment texts on those top posts.

```csharp
/// <summary>
/// Step 4: Project only required columns—two-phase approach to avoid deep nested subqueries.
/// </summary>
public List<ActiveUserDto> GetTopCommenters_Optimization4_Projection()
{
    var since = DateTime.UtcNow.AddDays(-7);

    // Get users with comment counts and pre-filter them
    var topUsers = _dbContext.Users
        .AsNoTracking() // Add AsNoTracking for read-only operations
        .Where(u => u.Comments.Any(c =>
            c.CreatedAt >= since &&
            c.Post.Category.Name == ".NET"))
        .Select(u => new
        {
            u.Id,
            u.Username,
            CommentsCount = u.Comments.Count(c =>
                c.CreatedAt >= since &&
                c.Post.Category.Name == ".NET")
        })
        .OrderByDescending(u => u.CommentsCount)
        .Take(5)
        .ToList();

    var result = new List<ActiveUserDto>();

    foreach (var user in topUsers)
    {
        // Get the top posts for this user using projection
        var topPosts = _dbContext.Comments
            .AsNoTracking()
            .Where(c =>
                c.UserId == user.Id &&
                c.CreatedAt >= since &&
                c.Post.Category.Name == ".NET")
            .GroupBy(c => c.PostId)
            .Select(g => new
            {
                PostId = g.Key,
                LikesCount = _dbContext.Posts
                    .Where(p => p.Id == g.Key)
                    .Select(p => p.Likes.Count)
                    .FirstOrDefault()
            })
            .OrderByDescending(p => p.LikesCount)
            .Take(3)
            .Select(p => new PostDto(p.PostId, p.LikesCount))
            .ToList();

        // Get the post IDs to use in the next query
        var topPostIds = topPosts.Select(p => p.PostId).ToList();

        // Get the latest comments for this user on their top posts
        var recentTexts = _dbContext.Comments
            .AsNoTracking()
            .Where(c =>
                c.UserId == user.Id &&
                c.CreatedAt >= since &&
                topPostIds.Contains(c.PostId))
            .OrderByDescending(c => c.CreatedAt)
            .Take(2)
            .Select(c => c.Text)
            .ToList();

        // Add to result
        result.Add(new ActiveUserDto(
            user.Id,
            user.Username,
            user.CommentsCount,
            topPosts,
            recentTexts
        ));
    }

    return result;
}
```

Despite having N+1 queries, we are down from 10 s to **40 milliseconds**:

![A benchmark.NET results table showing the performance of 'Optimization 4: Project only required columns' at 42.68 milliseconds and 'Optimization 5: One Query' at 39.30 milliseconds, along with memory allocation.](https://antondevtips.com/media/code_screenshots/efcore/optimization_challenge/img_4.png)

Here we send `1 + (2 × 5) = 11` simple calls to the database.

Now it's time to get rid of the loop and try to make everything in a single query.

[](#optimization-5-onequery-projection)

## Optimization 5: One-Query Projection

Let's try to put filtering, counting, grouping, sorting, and paging into one LINQ statement:

```csharp
/// <summary>
/// Optimization 5: One Query
/// </summary>
public List<ActiveUserDto> GetTopCommenters_Optimization5_OneQuery()
{
    var since = DateTime.UtcNow.AddDays(-7);

    var projected = _dbContext.Users
        // 1) Only users with at least one ".NET" comment in the window
        .Where(u => u.Comments
            .Any(c =>
                c.CreatedAt >= since &&
                c.Post.Category.Name == ".NET"))

        // 2) Project everything in one go
        .Select(u => new
        {
            u.Id,
            u.Username,
            CommentsCount = u.Comments
                .Count(c =>
                    c.CreatedAt >= since &&
                    c.Post.Category.Name == ".NET"),

            // 3) Top 3 posts by like count
            TopPosts = u.Comments
                .Where(c =>
                    c.CreatedAt >= since &&
                    c.Post.Category.Name == ".NET")
                .GroupBy(c => new { c.Post.Id, c.Post.Likes.Count })
                .Select(g => new { g.Key.Id, LikesCount = g.Key.Count })
                .OrderByDescending(p => p.LikesCount)
                .Take(3),

            // 4) Latest 2 comments on those top-3 posts
            RecentComments = u.Comments
                .Where(c =>
                    c.CreatedAt >= since &&
                    c.Post.Category.Name == ".NET" &&
                    // subquery filter: only these top IDs
                    u.Comments
                        .Where(d =>
                            d.CreatedAt >= since &&
                            d.Post.Category.Name == ".NET")
                        .GroupBy(d => d.PostId)
                        .OrderByDescending(g => g.Count())
                        .Take(3)
                        .Select(g => g.Key)
                        .Contains(c.PostId))
                .OrderByDescending(c => c.CreatedAt)
                .Take(2)
                .Select(c => c.Text)
        })

        // 5) Order & take top 5 users
        .OrderByDescending(x => x.CommentsCount)
        .Take(5)

        // 6) Shape into our DTO
        .Select(x => new ActiveUserDto(
            x.Id,
            x.Username,
            x.CommentsCount,
            x.TopPosts
                .Select(p => new PostDto(p.Id, p.LikesCount))
                .ToList(),
            x.RecentComments.ToList()
        ))
        .ToList();

    return projected;
}
```

![A benchmark.NET results table showing the performance of 'Optimization 5: One Query' at 44.82 milliseconds and 'Optimization 6: Query splitting' at 51.76 milliseconds, along with memory allocation.](https://antondevtips.com/media/code_screenshots/efcore/optimization_challenge/img_5.png)

Interesting that instead of 11 small queries - we did 1 and got only **3 ms** of performance boost.

Let's examine the underline SQL query:

```sql
SELECT u0.id, u0.username, u0.c, s0.id0, s0."Count", s1.text, s1.id, s1.id0, s1.id1
  FROM (
      SELECT u.id, u.username, (
          SELECT count(*)::int
          FROM devtips_optimization_challenge.comments AS c3
          INNER JOIN devtips_optimization_challenge.posts AS p1 ON c3.post_id = p1.id
          INNER JOIN devtips_optimization_challenge.categories AS c4 ON p1.category_id = c4.id
          WHERE u.id = c3.user_id AND c3.created_at >= @__since_0 AND c4.name = '.NET') AS c, (
          SELECT count(*)::int
          FROM devtips_optimization_challenge.comments AS c1
          INNER JOIN devtips_optimization_challenge.posts AS p0 ON c1.post_id = p0.id
          INNER JOIN devtips_optimization_challenge.categories AS c2 ON p0.category_id = c2.id
          WHERE u.id = c1.user_id AND c1.created_at >= @__since_0 AND c2.name = '.NET') AS c0
      FROM devtips_optimization_challenge.users AS u
      WHERE EXISTS (
          SELECT 1
          FROM devtips_optimization_challenge.comments AS c
          INNER JOIN devtips_optimization_challenge.posts AS p ON c.post_id = p.id
          INNER JOIN devtips_optimization_challenge.categories AS c0 ON p.category_id = c0.id
          WHERE u.id = c.user_id AND c.created_at >= @__since_0 AND c0.name = '.NET')
      ORDER BY (
          SELECT count(*)::int
          FROM devtips_optimization_challenge.comments AS c1
          INNER JOIN devtips_optimization_challenge.posts AS p0 ON c1.post_id = p0.id
          INNER JOIN devtips_optimization_challenge.categories AS c2 ON p0.category_id = c2.id
          WHERE u.id = c1.user_id AND c1.created_at >= @__since_0 AND c2.name = '.NET') DESC
      LIMIT @__p_1
  ) AS u0
  LEFT JOIN LATERAL (
      SELECT s.id0, s."Count"
      FROM (
          SELECT p2.id AS id0, (
              SELECT count(*)::int
              FROM devtips_optimization_challenge.likes AS l
              WHERE p2.id = l.post_id) AS "Count"
          FROM devtips_optimization_challenge.comments AS c5
          INNER JOIN devtips_optimization_challenge.posts AS p2 ON c5.post_id = p2.id
          INNER JOIN devtips_optimization_challenge.categories AS c6 ON p2.category_id = c6.id
          WHERE u0.id = c5.user_id AND c5.created_at >= @__since_0 AND c6.name = '.NET'
      ) AS s
      GROUP BY s.id0, s."Count"
      ORDER BY s."Count" DESC
      LIMIT 3
  ) AS s0 ON TRUE
  LEFT JOIN LATERAL (
      SELECT c7.text, c7.id, p3.id AS id0, c8.id AS id1, c7.created_at
      FROM devtips_optimization_challenge.comments AS c7
      INNER JOIN devtips_optimization_challenge.posts AS p3 ON c7.post_id = p3.id
      INNER JOIN devtips_optimization_challenge.categories AS c8 ON p3.category_id = c8.id
      WHERE u0.id = c7.user_id AND c7.created_at >= @__since_0 AND c8.name = '.NET' AND c7.post_id IN (
          SELECT c9.post_id
          FROM devtips_optimization_challenge.comments AS c9
          INNER JOIN devtips_optimization_challenge.posts AS p4 ON c9.post_id = p4.id
          INNER JOIN devtips_optimization_challenge.categories AS c