```yaml
---
title: The Best Way To Map Objects in .Net in 2024
source: https://antondevtips.com/blog/the-best-way-to-map-objects-in-dotnet-in-2024
date_published: 2024-06-21T08:55:21.852Z
date_captured: 2025-08-06T17:21:12.565Z
domain: antondevtips.com
author: Anton Martyniuk
category: general
technologies: [ASP.NET Core, .NET, AutoMapper, Mapster, NuGet, Entity Framework Core, Minimal APIs]
programming_languages: [C#]
tags: [object-mapping, dotnet, csharp, performance, code-quality, best-practices, auto-mapper, mapster, manual-mapping, data-transfer-objects]
key_concepts: [object-mapping, data-transfer-objects, separation-of-concerns, dependency-injection, reflection, required-properties, extension-methods, domain-models]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores various object mapping techniques in .NET, comparing manual mapping with popular libraries like AutoMapper and Mapster. It explains the fundamental reasons for using object mapping, such as separation of concerns, performance optimization, and security. While acknowledging the benefits of mapping libraries, the author argues that manual mapping, particularly when combined with C#'s `required` properties, offers superior control, performance, and compile-time safety. The piece provides detailed code examples for each approach, demonstrating how `required` properties enforce complete mapping and prevent runtime errors.
---
```

# The Best Way To Map Objects in .Net in 2024

![A dark background image with a white coding icon and the text 'dev tips' on the left. On the right, large white text reads 'THE BEST WAY TO MAP OBJECTS .NET IN 2024'. Purple abstract shapes are in the background.](https://antondevtips.com/media/covers/aspnetcore/cover_asp_best_mapping.png)

# The Best Way To Map Objects in .Net in 2024

Jun 21, 2024

[Download source code](/source-code/the-best-way-to-map-objects-in-dotnet-in-2024)

8 min read

[Sponsor my newsletter to reach 11,000+ readers](/sponsorship)

In today's blog post you will learn how to map objects in .NET using various techniques and libraries. We'll explore what is the best way to map objects in .NET in 2024.

## What is Object Mapping

What is **object mapping** and why do you need one in ASP.NET Core applications?

**Object mapping** is a transformation of objects from one type to another, often between different layers of an application.

Mapping is essential for separation of concerns

Here are a few reasons to use object mapping:

*   **Separation of concerns:** keeps business logic and data transfer logic distinct.
*   **Performance:** decreases size of objects transferred through the network, sending only the necessary piece of data needed by clients.
*   **Security:** hides private domain data from the clients. Like real entity ids, personal data, and internal domain properties.
*   **Maintainability:** simplifies updates and changes to the data structures, as domain models are distinguished from the public contract used by clients.

In many applications, usually there are 2 levels of models: domain and public contracts. These public contract models are often called DTOs (data transfer objects).

DTOs are the models consumed by a client, and usually they are tinier than their domain counterparts. Also, a DTO model may contain properties from few domain models.

It is essential to distinguish domain internal models from the public models. Clients should receive the exact piece of data they need, no more. And they shouldn't consume models that contain private and internal data due to security considerations.

Another reason for using separate models for public contracts: you can change your domain models while keeping your public contracts unchanged, so you won't break the clients using the API.

## How To Do Object Mapping

We figured out what object mapping is, now let's explore some of the most used mapping techniques.

There are 2 main approaches of object mapping: **manual** and **automated** by using mapping libraries.

Manual approach involves manually writing code that performs mapping, for example:

```csharp
public static BookDto MapToBookDto(this Book entity)
{
    return new BookDto
    {
        Title = entity.Title,
        Year = entity.Year,
        Isbn = entity.Isbn,
        Price = entity.Price
    };
}

var bookDto = book.MapToBookDto();
```

When mapping properties of `Book` domain entity to `BookDto` we need to manually set all properties of the DTO model.

This if often tiresome, that's why a mapping libraries were created. These libraries automate the process of mapping and reduce the boilerplate code. On paper, these libraries should minimize the risk of errors, but the reality is the opposite.

To showcase why using mapping libraries is not the best option in 2024, first we need to have a look at these libraries and how they perform object mapping.

## Mapping Using AutoMapper Library

**AutoMapper** is one of the most popular libraries for object-to-object mapping in .NET.

To add AutoMapper to your project, you need to run the following command to install NuGet package:

```bash
dotnet add package AutoMapper
```

Let's create a mapping from `Book` entity to `BookDto`:

```csharp
public class Book
{
    public Guid Id { get; set; }
    
    public string Title { get; set; }
    
    public int Year { get; set; }
    
    public string Isbn { get; set; }
    
    public decimal Price { get; set; }
    
    public Author Author { get; set; }
}

public record BookDto
{
    public string Isbn { get; set; }
    
    public string Title { get; init; }
    
    public int Year { get; init; }
    
    public decimal Price { get; set; }
    
    public string Author { get; set; }
}
```

First, you need to configure AutoMapper how to map these objects. You need to create a mapping class that inherits from base `Profile` class.

In this mapping profile, you need to specify only those fields that differ between the models. Other properties are mapped automatically.

```csharp
public class BookProfile : Profile
{
    public BookProfile()
    {
        CreateMap<Book, BookDto>()
            .ForMember(dest => dest.Author, opt => opt.MapFrom(src => src.Author.Name));
            
        CreateMap<BookDto, Book>()
            .ForPath(dest => dest.Author.Name, opt => opt.MapFrom(src => src.Author));
    }
}
```

`BookDto` is very similar to the `Book` entity, but has no `Id` property and has `Author` name as string that is mapped to the child `Author` entity.

Next, you need to register AutoMapper and its profiles in the DI container.

```csharp
builder.Services.AddAutoMapper(typeof(Program));
```

In the `AddAutoMapper` method you need to specify types of assemblies that contain the mapping profiles.

Finally, to use mapping, you need to use the `IMapper` interface and call the `Map` method:

```csharp
public class SomeService
{
    private readonly IMapper _mapper;

    public SomeService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public BookDto ToBookDto(Book entity)
    {
        return _mapper.Map<BookDto>(entity);
    }
}
```

## Mapping Using Mapster Library

**Mapster** is another powerful library for object mapping in .NET, known for its high performance and flexibility. This library is newer and faster when compared to AutoMapper.

To install Mapster, use the NuGet package manager:

```bash
dotnet add package Mapster.DependencyInjection
```

Mapster setup is much similar to the AutoMapper. First, create a profile by inheriting from the `IRegister` interface:

```csharp
public class BookProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config
            .NewConfig<Book, BookDto>()
            .TwoWays()
            .Map(dest => dest.Author, src => src.Author.Name);
    }
}
```

Then register Mapster in DI and add all the mapping profiles:

```csharp
builder.Services.AddMapster();

TypeAdapterConfig.GlobalSettings.Scan(Assembly.GetExecutingAssembly());
```

To use Mapster, inject the same `IMapper` interface, but from another namespace:

```csharp
public class SomeService
{
    private readonly IMapper _mapper;

    public SomeService(IMapper mapper)
    {
        _mapper = mapper;
    }

    public BookDto ToBookDto(Book entity)
    {
        return _mapper.Map<BookDto>(entity);
    }
}
```

Mapster also supports a static extension method `Adapt`, available for any object. This method performs an automatic mapping that doesn't require any mapping configuration:

```csharp
var bookDto = bookEntity.Adapt<BookDto>();
```

You can use this method for simple mappings where two models differ slightly.

So mapping libraries seem to be a great option, but why this is not the best approach to do the object mapping? Let's find out!

## Why Using Mapping Libraries is Not a Silver Bullet

Mapping libraries have a lot of advantages.

Despite their advantages, mapping libraries also come with several potential drawbacks:

1.  **Performance Overhead** - mapping libraries often use reflection to inspect and map object properties at runtime. This can introduce performance overhead, especially in high-performance applications or when dealing with complex or large volumes of data.
2.  **Complex configurations** - while mapping libraries aim to simplify the mapping process, they can sometimes lead to complex configurations, especially for advanced scenarios. Configuring custom mappings, value resolvers, and type converters can become cumbersome and error-prone.
3.  **Debugging Challenges** - when using mapping libraries, debugging mapping issues can be challenging. Errors might appear only at runtime, making it harder to trace and fix problems.
4.  **Error prone** - if you ever used mapping libraries in real applications, I am certain that you have run into runtime errors, only because you forgot to update the mapping profile after adding a new property to the mapping object.

So what is the best option to map objects?

It might sound shocking - but this is manual mapping. But with one interesting addition from me. Let's have a look!

## The Best Way To Do Mapping in .NET in 2024

First, let's explore entities for the blog posts application:

```csharp
public class BlogPost
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Content { get; set; }

    public required DateTime PublishedUtc { get; set; }

    public required Guid PublisherId { get; set; }

    public required Publisher Publisher { get; set; }

    public required List<BlogHistoryRecord> BlogHistoryRecords { get; init; } = [];
}

public class Publisher
{
    public required Guid Id { get; init; }

    public required string Email { get; init; }

    public required string Name { get; init; }
    
    public required string Role { get; init; }

    public required List<BlogPost> BlogPosts { get; init; } = [];
}

public class BlogHistoryRecord
{
    public required Guid Id { get; init; }

    public required Guid BlogPostId { get; init; }

    public required BlogPost BlogPost { get; init; }

    public required DateTime Date { get; init; }

    public required double Rating { get; init; }
}
```

We have a `BlogPost` entity, a `Publisher` entity that represents a user in the system that write blogs. And a `BlogHistoryRecord` entity that holds information for all user ratings for each blog post.

Now, let's explore the public contract (DTO) models that are returned from the webapi:

```csharp
public record BlogPostDto
{
    public required string Url { get; init; }

    public required string Title { get; init; }

    public required string Content { get; init; }

    public required DateOnly PublishedDate { get; init; }

    public required PublisherDto Publisher { get; init; }

    public required double Rating { get; init; }
}

public record PublisherDto
{
    public required string Name { get; init; }

    public required int TotalPosts { get; init; }

    public required double Rating { get; init; }
}
```

These models have interesting features:

*   `BlogPost` has `Rating` property that represents the average blog rating, taking into account all the user reviews
*   `PublisherDto` has `TotalPosts` and `Rating` properties. The `Rating` property represents the average blog rating of all publisher's blogs, taking into account all the user reviews

Now let's create a mapping for these objects. First, you need to create a static class which is a place for the mapping extension methods.

Next, you need to define mapping methods, I like making them in the following way:

```csharp
public static class BlogPostMappingExtensions
{
    public static BlogPostDto MapToBlogPostDto(this BlogPost entity)
    {
        // ...
    }
}

var blogPostDto = blogPost.MapToBlogPostDto();
```

This code is pretty straightforward as you can navigate to the `MapToBlogPostDto` method while reading the code or debugging and see exactly what is going on in the mapping. When using mapping libraries, you need to search for the mapping profiles or extensions in the entire solution to find out how the mapping is done.

Let's explore the full mapping implementation:

```csharp
public static class BlogPostMappingExtensions
{
    public static BlogPostDto MapToBlogPostDto(this BlogPost entity)
    {
        return new BlogPostDto
        {
            Url = entity.Id.ToString(),
            Title = entity.Title,
            Content = entity.Content,
            PublishedDate = DateOnly.FromDateTime(entity.PublishedUtc),
            Publisher = entity.Publisher.MapToPublisherDto(),
            Rating = CalculateRating(entity.BlogHistoryRecords)
        };
    }

    public static PublisherDto MapToPublisherDto(this Publisher entity)
    {
        var blogPostRatings = entity.BlogPosts
            .SelectMany(x => x.BlogHistoryRecords)
            .Select(x => x.Rating)
            .ToList();

        var averageRating = Math.Round(blogPostRatings.Average(), 2);

        return new PublisherDto
        {
            Name = entity.Name,
            TotalPosts = entity.BlogPosts.Count,
            Rating = averageRating
        };
    }

    private static double CalculateRating(List<BlogHistoryRecord> historyRecords)
    {
        return Math.Round(historyRecords.Average(record => record.Rating), 2);
    }
}
```

Let's see the mapping in action when returning DTO models in the asp.net core minimal APIs:

```csharp
app.MapGet("/api/blogs", async (ApplicationDbContext dbContext) =>
{
    var blogPosts = await dbContext.BlogPosts
        .Include(b => b.Publisher)
        .Include(b => b.BlogHistoryRecords)
        .ToListAsync();

    var blogPostDtos = blogPosts
        .Select(x => x.MapToBlogPostDto())
        .ToList();

    return Results.Ok(blogPostDtos);
});

app.MapGet("/api/publishers", async (ApplicationDbContext dbContext) =>
{
    var publishers = await dbContext.Publishers
        .Include(b => b.BlogPosts)
        .ThenInclude(b => b.BlogHistoryRecords)
        .ToListAsync();

    var publisherDtos = publishers
        .Select(x => x.MapToPublisherDto())
        .ToList();

    return Results.Ok(publisherDtos);
});
```

As you have noticed, all the properties for entities and DTOs are marked as **required**. This **secret addition** I find as the game changer in the object mapping.

Whenever you create mapping, you're not able to forget to map a property.

Let's see this in practice. We are going to modify the `BlogPost` entity and add two new properties:

```csharp
public class BlogPost
{
    // ...
    
    public required string Description { get; set; }

    public required string Category { get; set; }
}
```

And let's suppose that we forget to update the mapping, let's compile our application:

![A screenshot of Visual Studio or a similar IDE showing C# code with compilation errors. The errors indicate 'Required member 'BlogPostDto.Description' must be set in the object initializer' and 'Required member 'BlogPostDto.Category' must be set in the object initializer', demonstrating the compile-time safety of using `required` properties for object mapping.](https://antondevtips.com/media/code_screenshots/aspnetcore/best-mapping/img_aspnet_mapping_1.png)

Our application doesn't compile and we receive a list of compilation errors that are easy to fix.

## Summary

I think that **manual mapping** with **required properties** is the best way to do the object mapping in 2024.

Let's recap why this approach is better than using mapping libraries:

*   Following this approach, while reading the code, you can exactly see what is going on in the mapping. You don't need to search for mapping classes in the entire application to understand how the libraries do the mapping magic.
*   You have code safety. If you forget to update the mapping method - a compiler error is raised.
*   You have entire control over the mapping process, you don't need to spend time learning how to do the fancy mapping stuff in the libraries.
*   This approach is much more performant as no reflection is required during the runtime
*   Debugging is straightforward. Have you ever tried to step into the breakpoint in the mapping profile while debugging a mapping library? This is really hard or almost impossible to do. Forget about this problem and have the stress-less debugging.

Hope you find this newsletter useful. See you next time.

> You can download source code for this newsletter for free

[Download source code](/source-code/the-best-way-to-map-objects-in-dotnet-in-2024)

### Enjoyed this article? Share it with your network

[LinkedIn](https://www.linkedin.com/sharing/share-offsite/?url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fthe-best-way-to-map-objects-in-dotnet-in-2024&title=The%20Best%20Way%20To%20Map%20Objects%20in%20.Net%20in%202024)[X](https://twitter.com/intent/tweet?text=The%20Best%20Way%20To%20Map%20Objects%20in%20.Net%20in%202024&url=https%3A%2F%2Fantondevtips.com%2Fblog%2Fthe-best-way-to-map-objects-in-dotnet-in-2024)[Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fantondevtips.com%2Fblog%2Fthe-best-way-to-map-objects-in-dotnet-in-2024)

# Improve Your **.NET** and Architecture Skills

Join my community of **11,000+** developers and architects.

Each week you will get 1 practical tip with best practices and real-world examples.

Learn how to craft better software with source code available for my newsletter.

Subscribe

Email address

Join 11,000+ Subscribers

Join 11,000+ developers already reading

No spam. Unsubscribe any time.