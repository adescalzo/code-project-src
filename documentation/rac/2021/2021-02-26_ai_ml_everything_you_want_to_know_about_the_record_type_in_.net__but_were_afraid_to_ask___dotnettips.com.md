```yaml
---
title: Everything You Want to Know About the Record Type in .NET… But Were Afraid to Ask – dotNetTips.com
source: https://dotnettips.wordpress.com/2021/02/26/everything-you-want-to-know-about-the-record-type-in-net-5-but-were-afraid-to-ask/?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=10-lessons-i-learned-from-using-aspire-in-production&_bhlid=02b275dc8f7af750ddde9a88efe8c83efdcd1cbe
date_published: 2021-02-26T09:00:00.000Z
date_captured: 2025-08-08T15:47:47.226Z
domain: dotnettips.wordpress.com
author: View all posts by dotNetDave
category: ai_ml
technologies: [.NET 5, ASP.NET Web API, Entity Framework, Spargine, Newtonsoft.Json, System.Text.Json]
programming_languages: [C#]
tags: [.net, csharp, record-types, immutability, performance, dto, poco, boilerplate-code, data-access, web-api]
key_concepts: [record-types, immutability, data-transfer-objects, plain-old-clr-objects, value-types, reference-types, performance-benchmarking, boilerplate-code-reduction, structural-equality, init-accessor, with-expression]
code_examples: true
difficulty_level: intermediate
summary: |
  This article introduces the `record` type in .NET 5, highlighting its benefits for creating immutable data models like DTOs and POCOs. It explains how `record` types reduce boilerplate code by automatically generating equality operators, `GetHashCode()`, and a meaningful `ToString()` method. The author demonstrates key features such as the `init` accessor for initialization and the `with` expression for non-destructive updates. The article also presents performance benchmarks comparing `record` types against traditional reference and value types for operations like cloning, hashing, JSON serialization/deserialization, and list operations.
---
```

# Everything You Want to Know About the Record Type in .NET… But Were Afraid to Ask – dotNetTips.com

# Everything You Want to Know About the Record Type in .NET… But Were Afraid to Ask

Posted on October 1, 2024 by dotNetDave

![A Twitter poll by David McCarter (dotNetDave) asking developers if they plan to move their model classes used in APIs to use the new "record" type in .NET 5. The poll results show 50% YES, 30% Maybe?, and 20% No.](https://dotnettips.wordpress.com/wp-content/uploads/2021/02/recordtype-tweet.png)

In January 2021, I posed a question to viewers during one of my “**Rockin’ the Code World with dotNetDave**” shows: “**_What is the new class type for .NET 5?_**” The answer is the `**record**` class. I’ve been actively using this feature in my open-source project, Spargine, and I even conducted a Twitter poll to gauge interest among developers regarding the transition to .NET 5 for utilizing record types. In this article, I’ll explain why I’m _enthusiastic_ about this new feature, which has quickly become my favorite addition in .NET 5.

# Understanding Model Types

Many of the classes I create in my assemblies are what I refer to as “_model types_,” which include Data Transfer Objects (DTOs) or Plain Old CLR Objects (POCOs). These classes are primarily used to transport data to and from back-end API services, which I typically develop using ASP.NET Web API. You can think of them as code-first classes in Entity Framework. While it’s essential for these classes to adhere to good architecture and coding standards, their primary role is simply to represent data.

# The Introduction of Record Types

With the release of .NET 5 in 2020, the .NET team at Microsoft introduced the **record** class type, significantly reducing the “boilerplate” code developers need to write. Defining a record type is similar to defining a class, with the key distinction being the use of the record keyword instead of class. For instance, a type representing a person can be defined as follows:

```csharp
public sealed record Person : IDataRecord
```

The `IDataRecord` interface is part of my Spargine OSS that implements an Id property for consistency across all DTOs and adds a method `AllPropertiesToString()` that converts all the properties and their values to a string representation.

# Setting Values

One significant difference with record types is how values are assigned. Instead of using `set`, we utilize **`init`**:

```csharp
public string Email
{
    get => this._email;
    init
    {
        if (string.Equals(this._email, value, StringComparison.Ordinal))
        {
            return;
        }
        this._email = value.HasValue(0, 75) is false
            ? throw new ArgumentOutOfRangeException(
            nameof(this.Email),
            Resources.EmailLengthIsLimitedTo75Characters)
            : value;
    }
}
```

The `init` accessor behaves like `set`, with two key rules:

1.  Init values can be set in the constructor, akin to read-only variables.
2.  Init values can be set during object initialization.

Once the object is created, the data cannot be modified, maintaining the immutability characteristic.

# Updating Values

Since the values of record classes cannot be updated after creation, how can you modify data, particularly on the client side, for backend updates? Instead of modifying existing objects, a new object must be created. Record types simplify this process using the `**with**` keyword:

```csharp
var email = "test@example.com";
var id = "12345";
var person = new PersonRecord(email, id)
{
    FirstName = "John"
};
person = person with { FirstName = "Jane" };
```

# Equality and Comparisons

Whenever you create a class, especially model classes, it is important to implement `IComparable<T>`, `IEquatable<T>` and override the equality operators like this:

```csharp
public static bool operator >(Person left, Person right) =>
    left is not null &&
    left.CompareTo(right) > 0;
```

This often leads to extensive boilerplate code, particularly for the `CompareTo()` method, and can discourage developers from implementing these methods. Fortunately, with record types, these methods are automatically generated.

# Hashing and ToString()

Another method frequently overlooked in types like these is `GetHashCode()`. Here’s how a developer might manually implement it:

```csharp
public override int GetHashCode()
{
    var hash = new HashCode();
    hash.Add(this.Addresses);
    hash.Add(this.BornOn);
    hash.Add(this.CellPhone);
    hash.Add(this.Email);
    hash.Add(this.FirstName);
    hash.Add(this.Phone);
    hash.Add(this.Id);
    hash.Add(this.LastName);
    return hash.ToHashCode();
}
```

With record types, there’s no need to override the GetHashCode() method; it’s generated by the compiler. This alleviates maintenance concerns.

![A screenshot of generated C# Intermediate Language (IL) code for the GetHashCode method of a PersonRecord type, showing how the compiler automatically generates the hashing logic for all properties.](https://dotnettips.wordpress.com/wp-content/uploads/2021/02/personrecord-gethashcode-source.jpg?w=1024)

The default implementation of `ToString()` in most types typically returns the type name, which isn’t very informative. It’s generally recommended to override this method in classes and structures to provide a more meaningful representation of the data. However, with record types, overriding `ToString()` is unnecessary. For example, calling the `ToString()` method for a `Person` record might look like this:

```
Person { Addresses = System.Collections.Generic.List`1[dotNetTips.Spargine.Tester.Models.Address], BornOn = 2/20/1974 1:06:36 PM -08:00, CellPhone = (858) 123-1234, Email = cokqfkkrfwmu@ysvwbustiojh.ly, FirstName = OkRd_TQXfONhtH, HomePhone = 744-817-4861, Id = d6e1664bb11b421fb80fb8f1ef1804ab, LastName = gUbkABVdnrZ[crPCgTMfoGoe[ }
```

The `Person` type contains a collection of `Address` objects. However, as demonstrated in the output above, the `ToString()` method does not accurately serialize these addresses. To address this issue, I developed a method called `PropertiesToString()` in the `IDataRecord` interface, which effectively serializes the addresses. Below is the implementation in the code:

```csharp
public override string ToString() => this.PropertiesToString();
```

Here is the expected output from the `ToString()` method:

```
Person.Addresses[0].Address1:13870 usuuffcrnjsdpgphhjp,
Person.Addresses[0].Address2:tnfuunfxrbrqkbigidw 43833,
Person.Addresses[0].City:,
Person.Addresses[0].Country:British Indian Ocean Territory,
Person.Addresses[0].CountyProvince:kyxbkbonverjxoetvcea,
Person.Addresses[0].Id:2d237cc1a73b47d7b5c1a8106a87fc98,
Person.Addresses[0].Phone:1647578148,
Person.Addresses[0].PostalCode:BB3D 1ZZ,
Person.Addresses[0].State:,
Person.Addresses[1].Address1:13728 xvvwxgjmrpdvjhxljgn,
Person.Addresses[1].Address2:ktstrsjmjqrfslpuxsm 55781,
Person.Addresses[1].City:,
Person.Addresses[1].Country:British Indian Ocean Territory,
Person.Addresses[1].CountyProvince:ubveipwvqkywdowqokhi,
Person.Addresses[1].Id:03aa7cde242545869832924fae8d2697,
Person.Addresses[1].Phone:2517715883,
Person.Addresses[1].PostalCode:XMWF5LG,
Person.Addresses[1].State:,
Person.BornOn:10/5/2004 2:01:09 PM -07:00,
Person.CellPhone:(858) 123-1234,
Person.Email:Ivy.Hebert@vmnjibch.భారతం,
Person.FirstName:Ivy,
Person.Id:0f8db6fcbba1499d9857466b84a585b8,
Person.LastName:Hebert,
Person.Phone:4048740882
```

This output provides a clear and accurate serialization of the `Address` collection, ensuring that all relevant information is represented properly. I have submitted an issue to the .NET team, but so far, they have not fixed it.

# Performance Considerations

Performance is _**critical**_ in my work; I even wrote a book on the subject. I conducted benchmarks to compare the performance of record types against value and reference types.

## Cloning

The benchmark below illustrates the performance differences in cloning between record types, value types, and reference types. This comparison highlights the varying speeds associated with each type of cloning operation, providing insights into their efficiency.

![A horizontal bar chart titled "Cloning Types" comparing the mean time in nanoseconds (ns) for cloning operations. Record types are the fastest, followed by Value Types (Val Type), and then Reference Types (Ref Type) which are the slowest.](https://dotnettips.wordpress.com/wp-content/uploads/2021/02/cloning-types-chart.png?w=1024)

The benchmark results indicate that cloning a record is **1.12 times faster** than a value type and **1.23 times faster** than reference types.

## Computing Hash

The chart below displays the time required to hash reference types, value types, and record types.

![A horizontal bar chart titled "Hashing Classes, Records, and Structures" comparing the mean time in nanoseconds (ns) for hashing operations. Reference Types (Ref) are the fastest, followed by Value Types (Val), and then Record types which are the slowest.](https://dotnettips.wordpress.com/wp-content/uploads/2021/02/hashing-classes-records-structures.png?w=827)

When it comes to hashing, reference types are **1.02 times faster** than value types and **1.05 times faster** than record types.

## JSON Serialization

Serialization and deserialization of types to JSON is common, especially in API endpoints. Below are the results for this process.

![A horizontal bar chart titled "Json Serialization" comparing the mean time in nanoseconds (ns) for JSON serialization using both JsonSerializer (System.Text.Json) and Newtonsoft.Json. For JsonSerializer, Record types are fastest, then Ref Type, then Value Type. For Newtonsoft, Record types are fastest, then Value Type, then Ref Type.](https://dotnettips.wordpress.com/wp-content/uploads/2021/02/json-serialization.png?w=825)

![A horizontal bar chart titled "Json Deserialization" comparing the mean time in nanoseconds (ns) for JSON deserialization using both JsonSerializer (System.Text.Json) and Newtonsoft.Json. For JsonSerializer, Record types are fastest, then Ref Type, then Value Type. For Newtonsoft, Record types are fastest, then Value Type, then Ref Type.](https://dotnettips.wordpress.com/wp-content/uploads/2021/02/json-deserialization.png?w=825)

## Looping and Sorting

Iterating over a list of records showed that it is less performant than reference types but faster than value types.

![A horizontal bar chart titled "List ForEach" comparing the mean time in nanoseconds (ns) for iterating over lists of different types. List<Ref> is the fastest, followed by List<Record>, and then List<Val> which is the slowest.](https://dotnettips.wordpress.com/wp-content/uploads/2021/02/list-foreach.png?w=902)

Sorting a list of record types is slightly faster than reference types and faster than value types.

![A horizontal bar chart titled "List Sort" comparing the mean time in nanoseconds (ns) for sorting lists of different types. List<Record> is the fastest, followed by List<Ref>, and then List<Val> which is the slowest.](https://dotnettips.wordpress.com/wp-content/uploads/2021/02/list-sort.jpg?w=572)

These charts highlight some key performance differences between record types, reference types, and value types. If performance is a critical factor in your project, it’s essential to benchmark the performance of record types within your specific context.

# Summary

I hope this article enhances your understanding of record types and their performance benefits. If you’re interested, I encourage you to check out my coding standards book, where I detail the distinctions between record, reference, and value types. The key takeaway is that utilizing record types can significantly reduce coding effort and maintenance costs down the line.

Here are my main reasons for adopting record types, especially for DTOs:

1.  Simplifies the creation of immutable classes.
2.  Supports inheritance just like normal classes.
3.  Automatically generates equality operators, GetHashCode(), and a meaningful ToString() (with exceptions for collection properties).
4.  Can offer improved performance.

There are numerous advantages to migrating your projects to .NET 5 or above, and the introduction of record types is certainly one of them! If you have any comments or suggestions, please feel free to share below.