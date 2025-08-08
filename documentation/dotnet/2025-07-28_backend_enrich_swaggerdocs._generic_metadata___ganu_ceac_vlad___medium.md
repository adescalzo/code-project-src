# Enrich SwaggerDocs. Generic Metadata | Ganușceac Vlad | Medium

**Source:** https://medium.com/@vlad.ganuscheak/enriching-swagger-docs-for-minimal-apis-using-generic-metadata-26c198f31020
**Date Captured:** 2025-07-28T16:11:42.288Z
**Domain:** medium.com
**Author:** Vlad Ganușceac
**Category:** backend

---

# Enriching Swagger Docs for Minimal APIs Using Generic Metadata

[

![Vlad Ganușceac](https://miro.medium.com/v2/resize:fill:64:64/1*Z0uMotYNMM2X6tTHu1MY2g.jpeg)





](/@vlad.ganuscheak?source=post_page---byline--26c198f31020---------------------------------------)

[Vlad Ganușceac](/@vlad.ganuscheak?source=post_page---byline--26c198f31020---------------------------------------)

Follow

3 min read

·

Jul 13, 2025

8

1

Listen

Share

More

> “Code once. Document smartly.”

![](https://miro.medium.com/v2/resize:fit:685/1*ecjTRyJ3dSlu_5vjuVPm6g.png)

Besides what code _does_ or how well it _performs_, the true quality of a codebase often hinges on how intuitive or well-documented it is.

In a previous article, I described a common issue in Minimal API: **generated autodocumentation fails to properly handle complex query parameters**.

[

## Minimal API Misses This Feature: The Gap in Complex Query Bindings

### Minimal API endpoints do most things Web API controllers do, but with simpler code, lower response overhead, and a more…

medium.com



](/@vlad.ganuscheak/minimal-api-misses-this-feature-the-gap-in-complex-query-bindings-22e8c9e4002f?source=post_page-----26c198f31020---------------------------------------)

## Bridging the Gap with Custom Metadata

If the built-in functionality doesn’t document things right, there is always a possibility to apply a generic method for persisting proper metadata.

app.MapGet("/api/v1/persons", async (\[FromQuery\] Pagination.QueryRequest query, \[FromServices\] IMongoDatabase db) =>  
{  
    var result = await db.GetCollection<Person>("Persons").ApplyRequestAsync(query);  
  
    return Results.Ok(result);  
})  
.WithPaginationMetadata<Person>();

In the provided example above, the `WithPaginationMetadata` applies customized summary for the endpoint based on the generic parameter.

Also, the main endpoint’s description contains **available properties for filters** and a **possible query variation** based on the previously mentioned generic parameter.

## The Implementation: `WithPaginationMetadata<T>`

Below is the implementation of the metadata descriptor used in the mentioned scenario:

public static RouteHandlerBuilder WithPaginationMetadata<T\>(this RouteHandlerBuilder builder)  
{  
    var pagedType = typeof(PagedResult<>).MakeGenericType(typeof(T));  
  
    var parameters = typeof(T).GetProperties();  
  
    var filterProperties = parameters  
        .Select(p => $"- \`{p.Name}\` ({p.PropertyType.Name})")  
        .ToList();  
  
    var filterDesc = string.Join("\\n", filterProperties);  
  
    var queryRequest = new QueryRequest(  
        \[.. parameters.Select(BuildSortingParameter)\],  
        \[.. parameters.Select(BuildSearchFilter)\],  
        new AggregationParameter(1, 10));  
  
    var exampleOfUsage = JsonSerializer.Serialize(queryRequest);  
  
    builder.WithSummary($"Gets paginated results for \`{typeof(T).Name}\` with advanced filtering")  
           .WithDescription($"""  
       Supports pagination and sorting.  
  
       \*\*Available properties for filters:\*\*  
       {filterDesc}  
  
       \*\*\*A possible variation:\*\*\*  
       {exampleOfUsage}  
       """)  
           .WithTags("Pagination")  
           .Produces(StatusCodes.Status200OK, pagedType);  
  
    return builder;  
}

The method above uses reflection to retrieve the list of properties that will be returned in the paginated response. Each list element contains a description of a specific model property: name, type, attributes, etc.

## Sorting Parameters

The `BuildSortingParameter` method used within the `Select` statement provides a description of what sorting parameters may be applied in the query.

private static SortingParameter BuildSortingParameter(PropertyInfo propertyInfo)  
{  
    var number = Rand.Next(1, 3);  
  
    return new SortingParameter(propertyInfo.Name, (Enums.SortingOrder)number);  
}

## Filter Parameters

The `BuildSearchFilter` method used within the `Select` statement provides a description of what search parameters may be applied in the query.

private static FilterParameter BuildSearchFilter(PropertyInfo propertyInfo)  
{  
    var clrType = Nullable.GetUnderlyingType(propertyInfo.PropertyType) ?? propertyInfo.PropertyType;  
  
    string\[\] values = clrType switch  
    {  
        Type t when t == typeof(byte)  
                 || t == typeof(sbyte)  
                 || t == typeof(short)  
                 || t == typeof(ushort)  
                 || t == typeof(int)  
                 || t == typeof(uint)  
                 || t == typeof(long)  
                 || t == typeof(ulong)  
                 => \["1", "2"\],  
  
        Type t when t == typeof(float)  
                 || t == typeof(double)  
                 || t == typeof(decimal)  
                 => \["1.1", "2.2"\],  
  
        Type t when t == typeof(bool) => \["true", "false"\],  
        Type t when t == typeof(char) => \["A", "B"\],  
        Type t when t == typeof(string) => \["search value"\],  
  
        Type t when t == typeof(DateOnly)  
                 => \[DateOnly.FromDateTime(DateTime.Now).ToString("yyyy.MM.dd")\],  
  
        Type t when t == typeof(DateTime)  
                 => \[DateTime.Now.ToString("yyyy.MM.dd")\],  
  
        \_ => \[\]  
    };  
  
    return new FilterParameter(propertyInfo.Name, values);  
}

The final result is illustrated on the screenshot below. Both tags, summary, and description are automatically applied on the endpoint within the SwaggerUI.

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/1*BcMKpJYKceiOleMM2GfbZw.png)

Documentation After Applying Generic MetaData Descriptor

## Conclusion

Clear documentation is just as important as working code. With a small effort, you can make your Minimal APIs easier to understand and use.

While SwaggerUI does a great job of exposing endpoints, there are a few advanced considerations to keep in mind:

*   ❌ You can’t dynamically inject fully custom components (like React/Vue-based playgrounds) into the Swagger UI from .NET code alone.
*   ✅ If needed, you can override the `index.html` to replace Swagger UI entirely with your own interactive setup.
*   ✅ Alternatively, you can host custom pages (like `/playground`) with full HTML, CSS, and JavaScript support alongside Swagger, and link to them directly from the UI or documentation.

Take a look at the previous articles on Minimal APIs:

[

## Low-level Validations in .NET Minimal APIs

### Have you ever thought about what the most low-level validations in an API are?

medium.com



](/@vlad.ganuscheak/low-level-validations-in-net-minimal-apis-b894b4ca1261?source=post_page-----26c198f31020---------------------------------------)

[

## Minimal API Misses This Feature: The Gap in Complex Query Bindings

### Minimal API endpoints do most things Web API controllers do, but with simpler code, lower response overhead, and a more…

medium.com



](/@vlad.ganuscheak/minimal-api-misses-this-feature-the-gap-in-complex-query-bindings-22e8c9e4002f?source=post_page-----26c198f31020---------------------------------------)