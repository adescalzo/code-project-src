```yaml
---
title: How To Implement Offset and Cursor-Based Pagination in EF Core
source: https://antondevtips.com/blog/how-to-implement-offset-and-cursor-based-pagination-in-ef-core?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2100
date_published: 2025-07-29T07:45:13.046Z
date_captured: 2025-08-14T11:38:15.954Z
domain: antondevtips.com
author: Anton Martyniuk
category: ai_ml
technologies: [EF Core, ASP.NET Core, .NET, Entity Framework Extensions, FusionAuth]
programming_languages: [C#, SQL, HTTP]
tags: [pagination, ef-core, data-access, performance, web-api, dotnet, database, csharp, optimization, api-design]
key_concepts: [offset-based-pagination, cursor-based-pagination, keyset-pagination, seek-pagination, data-consistency, performance-optimization, cursor-encoding, api-security]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide to implementing two primary pagination techniques, offset-based and cursor-based, using EF Core in .NET applications. It details the implementation steps for each method, including advanced cursor pagination with multiple fields and the crucial aspect of encoding/decoding cursors for enhanced security and API design. The article highlights the pros and cons of each approach, emphasizing performance and consistency considerations for large datasets. It serves as a practical resource for developers aiming to optimize data retrieval and user experience in their applications.]
---
```

# How To Implement Offset and Cursor-Based Pagination in EF Core

![Cover image for the article "How To Implement Offset and Cursor-Based Pagination in EF Core" featuring a code icon and "dev tips" text on a dark blue background with abstract purple shapes.](https://antondevtips.com/media/covers/efcore/cover_efcore_pagination.png)

# How To Implement Offset and Cursor-Based Pagination in EF Core

Jul 29, 2025

[Download source code](/source-code/how-to-implement-offset-and-cursor-based-pagination-in-ef-core)

7 min read

Efficient pagination is crucial for enhancing application performance and user experience, particularly when working with large datasets. Pagination ensures quick data retrieval, reduces server load, and improves usability by delivering content in manageable chunks.

In this newsletter, you'll learn step-by-step how to implement two popular pagination techniques in EF Core: **offset-based** pagination and **cursor-based** pagination (also known as **keyset** or **seek** pagination).

In this post, we will explore:

*   How to implement offset-based pagination
*   How to implement cursor-based pagination
*   Advanced techniques to handle cursor pagination on multiple properties
*   Encoding and decoding cursors

Let's dive in!

## How To Implement Offset-Based Pagination

**Offset-based** pagination is one of the simplest and most widely-used pagination methods. It works by skipping a specified number of records (offset) and then fetching a set number of records (page size).

The offset-based approach uses two parameters:

*   Offset: The number of records to skip.
*   Limit (Page Size): The number of records to retrieve.

In EF Core, you typically achieve this by combining `.Skip()` and `.Take()` methods.

Let's explore an example with a Book's store:

```csharp
public class Book
{
    public Guid Id { get; set; }
    public string Title { get; set; }
    public DateTime PublishedAt { get; set; }
    public string Isbn { get; set; }
    
    public Guid AuthorId { get; set; }
    public Author Author { get; set; }
    
    public int PageCount { get; set; }
    public decimal Price { get; set; }
    public decimal Rating { get; set; }
}
```

Here is a simple API endpoint that implements Offset-Based pagination:

```csharp
app.MapGet("/api/books", async (
    ApplicationDbContext context,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    CancellationToken cancellationToken = default) => 
{
    if (pageSize > 100)
    {
        pageSize = 100;
    }

    var query = context.Books
        .Include(b => b.Author)
        .AsQueryable();

    var totalItems = await query.CountAsync(cancellationToken);

    var books = await query
        .OrderBy(b => b.Title)
        // We implement Offset-based pagination with Skip + Take
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(b => new BookResponse(
            b.Id,
            b.Title,
            b.Year,
            b.AuthorId,
            b.Rating,
            b.PublishedAt))
        .ToListAsync(cancellationToken);

    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

    var result = new
    {
        Books = books,
        Pagination = new
        {
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasPrevious = page > 1,
            HasNext = page < totalPages
        }
    };

    return Results.Ok(result);
});
```

Note: always limit the maximum number of items per page on the backend. This will prevent exploit issues with API methods that ask for too many items, which can slow down the entire server.

You can also combine filters with pagination. For example, we can filter books by Year and return paginated results:

```csharp
app.MapGet("/api/books", async (
    ApplicationDbContext context,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 10,
    [FromQuery] int? year = null,
    CancellationToken cancellationToken = default) => 
{
    if (pageSize > 100)
    {
        pageSize = 100;
    }

    var query = context.Books
        .Include(b => b.Author)
        .AsQueryable();

    // Apply the filter if specified
    if (year.HasValue)
    {
        query = query.Where(b => b.Year == year.Value);
    }

    var totalItems = await query.CountAsync(cancellationToken);

    var books = await query
        .OrderBy(b => b.Title)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        // Omitted for brevity
        .Select(b => new BookResponse(...)
        .ToListAsync(cancellationToken);

    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

    var result = new
    {
        Books = books,
        Pagination = new
        {
            CurrentPage = page,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            HasPrevious = page > 1,
            HasNext = page < totalPages
        }
    };

    return Results.Ok(result);
});
```

Here are the pros and cons of offset-based pagination.

**Pros:**

*   **Simplicity:** Easy to understand, implement, and debug.
*   **Statelessness:** Each request independently fetches a specific page without needing context from previous requests.

**Cons:**

*   **Performance:** Efficiency decreases with higher offsets because the database must scan all records up to the offset point. Even if you use indexes, the database still needs to go through all the rows.
*   **Consistency Issues:** If the underlying data changes frequently, users may experience inconsistencies, such as duplicated or missed records, when navigating between pages.

Offset-based pagination is ideal for small to medium-sized datasets or scenarios where the simplicity of implementation outweighs performance concerns.

**Real-world example:** online store where a user can navigate on a random page number.

## How To Implement Cursor-Based Pagination

**Cursor-based** pagination, also known as **keyset** or **seek** pagination, is a more efficient method for large datasets.

Instead of skipping records, cursor-based pagination retrieves the next set of records starting from a specific point, called **cursor**, usually based on a unique or sorted column. The cursor represents a unique column (or a group of unique columns) in the database, such as an ID or timestamp.

How it works:

1.  The client requests the first page, and the server sends the cursor in response.
2.  On the next request, the client sends this cursor to the server to fetch the next set of data.
3.  The server queries records that are greater (or less) than the cursor value.

In our book's store, we can create a cursor based on the `Id` (primary key) column.

Here is a basic example of how to implement cursor-based pagination:

```csharp
var query = context.Books
    .Include(b => b.Author)
    .AsQueryable();

if (lastId.HasValue)
{
    // Filter by cursor to get the next items
    query = query.Where(b => b.Id >= lastId.Value);
}

var books = await query
    .OrderBy(b => b.Id)
    .Take(limit + 1)
    .ToListAsync(cancellationToken);
```

The first time, the client sends an empty `lastId` and gets the first page.

To get the next page, the client must send an `Id` of the last item on the page.

Here is a complete example of a WebApi request:

```csharp
private static async Task<IResult> HandleCursorPagination(
    ApplicationDbContext context,
    [FromQuery] Guid? lastId = null,
    [FromQuery] int limit = 10,
    CancellationToken cancellationToken = default)
{
    var query = context.Books
        .Include(b => b.Author)
        .AsQueryable();

    if (lastId.HasValue)
    {
        // Filter by cursor to get the next items
        query = query.Where(b => b.Id >= lastId.Value);
    }

    var books = await query
        .OrderBy(b => b.Id)
        .Take(limit + 1)
        .ToListAsync(cancellationToken);

    var hasMore = books.Count > limit;
    if (hasMore)
    {
        books.RemoveAt(books.Count - 1);
    }

    var result = new
    {
        Books = books
            // Omitted for brevity
            .Select(b => new BookResponse(...)
            .ToList(),
        Pagination = new
        {
            HasMore = hasMore
        }
    };

    return Results.Ok(result);
}
```

The server sorts the database records by the `Id` column and returns records that are bigger than the `lastId`.

Each time we perform pagination, we retrieve one additional record from the database to determine if there are more pages.

Here are the pros and cons of cursor-based pagination.

**Pros:**

*   **Performance:** Efficient even for large datasets, as it avoids scanning all records up to an offset.
*   **Consistency:** Provides consistent results even when data changes frequently, reducing duplicates and missing records between pages.

**Cons:**

*   **Complexity:** More challenging to implement and understand compared to offset-based pagination.
*   **Statefulness:** Requires maintaining the cursor state between requests.

Cursor-based pagination is ideal for scenarios that demand high performance and consistency, especially for handling large-scale data.

**Real-world example:** social media feed or an image gallery where you can only get the next items.

## Using Multiple Fields in a Cursor-Based Pagination

In real-world use cases, you might need to use multiple properties in the cursor.

If your data can have duplicate values for the single-field cursor, you may need to add more fields to the cursor.

In the case of an online store, these could be `Id` and `PublishedAt` columns:

```csharp
var query = context.Books
    .Include(b => b.Author)
    .OrderByDescending(b => b.PublishedAt)
    .ThenBy(b => b.Id)
    .AsQueryable();

if (publishedAt.HasValue)
{
    query = query.Where(b =>
        b.PublishedAt > publishedAt.Value ||
        (b.PublishedAt == publishedAt.Value && b.Id >= lastId!.Value));
}

var books = await query
    .Take(limit + 1)
    .ToListAsync(cancellationToken);
```

In this example, pagination handles two cases:

1.  Books published after the cursor date
2.  Books published on the same date but with ID greater than or equal to the last seen ID

This ensures proper pagination even when multiple books share the same publication date.

Real-world examples: paginating comments, transactions, or any time-series data with possible collisions.

## Encoding and decoding Cursor

In practical terms, a cursor is a token representing the position of the last item on the current page. When the client requests the next page, it sends this token back to the server, which uses it to continue the query from the correct position.

Sending multiple parameters from client to server in the paging requests has a few major drawbacks:

*   Each time when you need to update the cursor fields - you need to change both client and server
*   Raw property values (like IDs or timestamps) might expose internal details.

Instead, we can encapsulate the logic of cursor generation on the server by encoding and decoding the cursor. Encoding the cursor hides sensitive implementation details and prevents users from easily guessing or manipulating pagination positions.

It also simplifies the process for clients using the API, as they don't need to know how to construct the cursor. A client receives the next (and back if needed) cursor from the server and sends it in the next request.

**Encoding** a cursor involves the following steps:

*   Combine all the necessary cursor property values into a single string (such as JSON or a delimiter-separated string).
*   Encode the result using Base64 (or another encoding) so it's URL-safe.
*   Optionally, sign or encrypt the encoded cursor for extra security.

On the server, the **decoding** process is the reverse:

*   Decrypt the cursor (optionally)
*   Decode the cursor from Base64.
*   Parse the decoded string back into the original property values (e.g., parse JSON).

Here is how to implement Cursor encoding and decoding for our application:

```csharp
public sealed record Cursor(Guid LastId, DateTime? PublishedAt = null)
{
    public static string Encode(Guid lastId, DateTime? publishedAt = null)
    {
        var cursor = new Cursor(lastId, publishedAt);
        var json = JsonSerializer.Serialize(cursor);
        return Base64UrlTextEncoder.Encode(Encoding.UTF8.GetBytes(json));
    }

    public static Cursor? Decode(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return null;
        }

        try
        {
            var json = Encoding.UTF8.GetString(Base64UrlTextEncoder.Decode(cursor));
            return JsonSerializer.Deserialize<Cursor>(json);
        }
        catch
        {
            return null;
        }
    }
}
```

Here is an improved WebApi request that uses cursor encoding and decoding:

```csharp
private static async Task<IResult> HandleCursorDatePagination(
    ApplicationDbContext context,
    [FromQuery] string? cursor = null,
    [FromQuery] int limit = 10,
    CancellationToken cancellationToken = default)
{
    var decodedCursor = Cursor.Decode(cursor);
    var lastId = decodedCursor?.LastId;
    var publishedAt = decodedCursor?.PublishedAt;

    var query = context.Books
        .Include(b => b.Author)
        .OrderByDescending(b => b.PublishedAt)
        .ThenBy(b => b.Id)
        .AsQueryable();

    if (publishedAt.HasValue)
    {
        query = query.Where(b =>
            b.PublishedAt > publishedAt.Value ||
            (b.PublishedAt == publishedAt.Value && b.Id >= lastId!.Value));
    }

    var books = await query
        .Take(limit + 1)
        .ToListAsync(cancellationToken);

    var hasMore = books.Count > limit;
    if (hasMore)
    {
        books.RemoveAt(books.Count - 1);
    }

    var lastBook = books.LastOrDefault();
    var nextCursor = hasMore && lastBook != null
        ? Cursor.Encode(lastBook.Id, lastBook.PublishedAt)
        : null;

    var result = new
    {
        Books = books
            // Omitted for brevity
            .Select(b => new BookResponse(...)
            .ToList(),
        Pagination = new
        {
            NextCursor = nextCursor,
            HasMore = hasMore
        }
    };

    return Results.Ok(result);
}
```

To get the first page, send the following request without a cursor:

```http
GET {{host}}/api/books/cursor-date?limit=10
```

As a result, you will receive the following response:

```json
{
  "books": [
    {
      "id": "01979c64-41bd-779c-9ef9-7252db9f00c0",
      "title": "Generic Soft Gloves",
      "year": 2020,
      "authorId": "9d6f4384-e89a-497a-8699-d4ed95a1803f",
      "rating": 1.83,
      "publishedAt": "2025-06-22T01:59:44.754392Z"
    },
    ...
  ],
  "pagination": {
    "nextCursor": "eyJMYXN0SWQiOiIwMTk3OWM2NC00MWI1LTdmNjgtODQ3OC1kMWRiYmYwODNlZjUiLCJQdWJsaXNoZWRBdCI6IjIwMjUtMDUtMjdUMDg6MzQ6MTQuMzM5MTEzWiJ9",
    "hasMore": true
  }
}
```

To get to the next page, send the received cursor:

```http
GET {{host}}/api/books/cursor-date?cursor=eyJMYXN0SWQiOiIwMTk3OWM2NC00MWI1LTdmNjgtODQ3OC1kMWRiYmYwODNlZjUiLCJQdWJsaXNoZWRBdCI6IjIwMjUtMDUtMjdUMDg6MzQ6MTQuMzM5MTEzWiJ9&limit=10
```

**Important Considerations:**

*   **Cursor Expiry:** In fast-changing datasets, consider expiring or validating old cursors to avoid referencing deleted or changed data.
*   **Validation:** Always validate decoded cursor data to ensure it's well-formed and matches expected types.
*   **Compatibility:** If you change the sort logic or schema, old cursors may become invalid. Plan versioning or migration as needed.

## Summary

Pagination is a foundational feature for any application working with large datasets, directly impacting performance, scalability, and user experience. While offset-based pagination is simple and easy to implement, it can struggle with large data and real-time changes.

Cursor-based pagination (keyset/seek pagination) offers better performance and consistency, especially for large and dynamic datasets. Though it comes with added complexity.

Use Offset-Based pagination when you need to access a random page or in scenarios where the simplicity of implementation outweighs performance concerns.

In other cases, you can use Cursor-Based pagination for better performance, control over pagination behavior, and enhanced security by encapsulation fields into the cursor without a client knowing about it.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/how-to-implement-offset-and-cursor-based-pagination-in-ef-core)