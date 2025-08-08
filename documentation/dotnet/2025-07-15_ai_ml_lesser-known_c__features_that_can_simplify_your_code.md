# Lesser-known C# features that can simplify your code

**Source:** https://blog.elmah.io/lesser-known-c-features-that-can-simplify-your-code/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2096
**Date Captured:** 2025-07-28T16:00:39.199Z
**Domain:** blog.elmah.io
**Author:** Unknown
**Category:** ai_ml

---

# Lesser-known C# features that can simplify your code

[](https://www.facebook.com/sharer.php?u=https://blog.elmah.io/lesser-known-c-features-that-can-simplify-your-code/ "Share to Facebook")[](https://x.com/share?text=Lesser-known%20C%23%20features%20that%20can%20simplify%20your%20code&url=https://blog.elmah.io/lesser-known-c-features-that-can-simplify-your-code/ "Share to X")[](https://www.linkedin.com/sharing/share-offsite/?url=https://blog.elmah.io/lesser-known-c-features-that-can-simplify-your-code/ "Share to LinkedIn")[](mailto:?subject=Lesser-known%20C%23%20features%20that%20can%20simplify%20your%20code&body=https://blog.elmah.io/lesser-known-c-features-that-can-simplify-your-code/ "Share to Email")

Written by [Ali Hamza Ansari](/author/ali/), July 15, 2025

## Contents

Show contents

*   [Records](#records)
*   [Null Coalescing Operator (??)](#null-coalescing-operator)
*   [Tuple Deconstruction](#tuple-deconstruction)
*   [Global Usings](#global-usings)
*   [Collection Expressions](#collection-expressions)
*   [Pattern Matching](#pattern-matching)
*   [is pattern for type matching](#is-pattern-for-type-matching)
*   [Property pattern matching](#property-pattern-matching)
*   [Positional Pattern Matching (with deconstruct)](#positional-pattern-matching-with-deconstruct)
*   [Ref Returns and Ref Locals](#ref-returns-and-ref-locals)
*   [Primary Constructors for Classes](#primary-constructors-for-classes)
*   [Dynamic Keyword](#dynamic-keyword)
*   [Target-Typed new](#target-typed-new)
*   [Reflection](#reflection)
*   [Caller Information Attributes](#caller-information-attributes)
*   [Switch Expressions](#switch-expressions)
*   [File-Scoped Namespaces](#file-scoped-namespaces)
*   [Range and Index Operators](#range-and-index-operators)
*   [Init-Only Setters](#init-only-setters)
*   [Conclusion](#conclusion)

C# is a robust OOP language that provides the foundation for building almost every application with .NET. As a C# developer, you are likely familiar with its fundamentals. However, like any legacy technology, it holds more depth to explore and enhance your code's maintainability and beauty. In this post, I will share some lesser-known features to help you unlock even more of what C# can do. While many probably already know multiple of these features, I still hope you'll find something you didn't already know.

[![Lesser-known C# features that can simplify your code](https://blog.elmah.io/content/images/2025/06/lesser-known-csharp-features-that-can-simplify-your-code-o-1.png)](https://blog.elmah.io/content/images/2025/06/lesser-known-csharp-features-that-can-simplify-your-code-o-1.png)

## Records

Records are the value-based type that declares immutable objects. You can use them as a concise and more straightforward alternative to a class for simple type declaration:

```csharp
public record Coordinate(double Latitude, double Longitude);
var city1Coordinates = new Coordinate(21.472, 55.2911);
var city2Coordinates = new Coordinate(21.472, 55.2911);
bool isEqual = city1Coordinates == city2Coordinates; // true
```

**Benefit**: Simplifies immutable object creation with value equality as shown in the example, reducing boilerplate and improving code clarity.

I already wrote a detailed post about records for anyone interested here: [Exploring C# Records and Their Use Cases](https://blog.elmah.io/exploring-c-records-and-their-use-cases/).

## **Null Coalescing Operator (??)**

C# concisely handles null values by returning the default value, eliminating verbose `if` statements or ternary checks. You can avoid null reference errors by using the null coalescing operator:

```csharp
int? marks = null;
int result = marks ?? 0; // Assigns 0 if marks is null

string? username = null;
string name = username ?? "Guest"; // Assigns "Guest" if username is null
```

**Benefit:** Prevents `NullReferenceException` and removes verbose `if` checks by providing default fallback values in a clean, readable way.

## Tuple Deconstruction

A tuple is another C# feature that enhances maintainability and conciseness. It allows you to return multiple values from a function and doesn't require creating a custom class or struct, making it memory and time-efficient.

```csharp
var location = (Latitude: 49.0232, Longitude: -98.4822);

// Deconstruct the tuple to get its fields as variables
var (lat, lon) = location;
Console.WriteLine($"Latitude: {lat}, Longitude: {lon}");
```

```csharp
(string Subject, string Body) GenerateEmailContent(EmailTypeEnum emailType, string recipientName)
{
   switch (emailType)
   {
       case EmailTypeEnum.WelcomeEmail:
           return ($"Welcome to Our Platform, {recipientName}!",
               $"Hi {recipientName}, welcome aboard! We're excited to have you.");

       case EmailTypeEnum.PasswordReset:
           return ($"Password Reset Request",
               $"Hi {recipientName}, click the link to reset your password.");

       default:
           return ("Unknown Email Type", "No content available.");
   }
}
```

**Benefit:** Makes method returns cleaner by unpacking multiple values without defining custom types, improving readability and reducing overhead.

If you want to know more about tuples, I wrote a detailed post about it here: [Leveraging Tuples in C#: Real-World Use Cases](https://blog.elmah.io/leveraging-tuples-in-c-real-world-use-cases/).

## Global Usings

Projects can be complex and big. Even smaller projects have hundreds of files, and managing the code is challenging. Many techniques are used to achieve readability and maintainability. Global usings eliminate the need to import the same namespaces in multiple files. You may use a single class or library in dozens of other files, and repeated imports lead to unnecessary code duplication. C# introduces global usings, where you keep imports in a central place to include in all the files. Just define a separate file and import everything like this:

```csharp
global using System;
global using BankSystem.Models.Dtos;
global using BankSystem.Models;
global using System.Collections.Generic;
global using BankSystem.Data.CustomerRepos;
```

**Benefit:** Eliminates repetitive `using` statements across files, streamlining code and improving maintainability, especially in large projects.

## Collection Expressions

Available in C# 12 and onwards, collection expressions offer a cleaner and more concise approach to initialize arrays, lists, and spans:

```csharp
int[] numbers = [ 3, 5, 8, 14 ];
// Traditionally done like new int[] { 3, 5, 8, 14 }

List<string> names = [ "Patrick", "Hunt", "Abraham" ];

// You can spread it
int[] a = [ 15, 20 ];
int[] b = [ ..a, 23, 4 ];
// Result: [ 15, 20, 23, 4 ]
```

**Benefit:** Enables concise, readable initialization of collections and supports value spreading, reducing boilerplate and enhancing clarity.

## Pattern Matching

Introduced in C# 7, pattern matching offers a readable, concise, and expressive way to test data instead of traditional conditionals like `if` or `switch.` It can be used in many ways, like type matching, property pattern, or position checking.

### `is` pattern for type matching

```csharp
object myVar = 50;

if (myVar is int i)
{
    Console.WriteLine($"It's an integer: {i}");
}
```

### Property pattern matching

```csharp
public class ExamResult
{
    public string CourseName { get; set; }
    public int Marks { get; set; }
}

ExamResult result = new ExamResult { CourseName = "History", Marks = 61 };

if (result is { Marks: > 33 })
{
    Console.WriteLine($"You passed the subject {result.CourseName}.");
}
```

### **Positional Pattern Matching** (with `deconstruct`)

```csharp
public record Point(int X, int Y);

Point point1 = new Point(0, 5);

string position = point1 switch
{
    (0, 0) => "At Origin",
    (0, _) => "On Y axis",
    (_, 0) => "On X axis",
    _ => "Somewhere in the coordinates"
};

Console.WriteLine(position);
```

**Benefit:** Improves code expressiveness by enabling conditions to be written declaratively, supporting type, property, and position-based checks.

## **Ref Returns** and **Ref Locals**

By default, functions return values from a function that is a copy of the original variable returned. In some cases, you need to work directly with the memory location. You can do it by:

```csharp
ref int FindElementRef(int[] numbers, int target)
{
    for (int i = 0; i < numbers.Length; i++)
    {
        if (numbers[i] == target)
            return ref numbers[i];
    }

    throw new InvalidOperationException("Element not found");
}

void Main()
{
    int[] data = { 2, 5, 6, 9 };
    ref int found = ref FindElementRef(data, 6);
    found = 99;  // This modifies data[2] directly

    Console.WriteLine(string.Join(", ", data)); // Output: 2, 5, 6, 9
}
```

**Benefit:** Allows direct manipulation of memory, enabling performance-critical scenarios without copying data unnecessarily.

## Primary Constructors for Classes

Introduced in C# 12, primary constructors for classes are similar to records. Now you can concisely define it by reducing boilerplate code:

```csharp
public class Course(string name, string instructor)
{
    public void PrintInfo() => Console.WriteLine($"Name: {name}, Instructor: {instructor}");
}
```

**Benefit:** Reduces boilerplate by allowing constructor parameters to be defined directly in the class signature.

## Dynamic Keyword

Although C# is known as a strongly typed language, it provides type flexibility. You can use the dynamic keyword to defer type checking until runtime, allowing you to work with different types without specifying them at compile time. This is particularly useful in handling various input types dynamically:

```csharp
public async Task<EmailMessage> BuildEmail(EmailTypeEnum type, dynamic? value, string token = null)
{
    await Task.CompletedTask; // Placeholder for async ops

    return type switch
    {
        EmailTypeEnum.Welcome => BuildWelcomeEmail(Parse<WelcomeModel>(value)),
        EmailTypeEnum.PasswordReset => BuildPasswordResetEmail(Parse<ResetModel>(value), token),
        EmailTypeEnum.Notification => BuildNotificationEmail(Parse<NotificationModel>(value)),
        _ => throw new ArgumentException("Unknown email type")
    };
}
```

**Benefit:** Adds flexibility by enabling runtime type resolution, useful for handling loosely-typed data (like JSON and reflection-heavy APIs). Be aware that using dynamics in C# causes you to lose compile-time safety.

## Target-Typed `new`

It is a simple feature to notice, but it can save time and additional code for your project. If you are using C# 9+, then instead of initializing like this:

```csharp
List<string> names = new List<string>();
```

You can do this:

```csharp
List<string> names = new ();
```

Some people love it while others absolutely hate it. So, make sure to agree on which one to use to avoid starting a civil war inside your team ☺️

**Benefit:** Simplifies object instantiation by removing redundant type declarations, making the code cleaner and easier to scan.

## Reflection

Reflection in C# is a runtime feature that dynamically enables the program to inspect and interact with the metadata of assemblies and types, such as classes, interfaces, value types, methods, fields, and properties. Reflection makes dynamic tasks handy, such as parsing, JSON reading, CSV creation, model mapping, etc.

```csharp
public static class CsvExporter
{
    public static string ExportToCsv<T>(List<T> data)
    {
        if (data == null || data.Count == 0) return string.Empty;

        var type = typeof(T);
        var properties = type.GetProperties();

        var sb = new StringBuilder();

        // Header
        sb.AppendLine(string.Join(",", properties.Select(p => p.Name)));

        // Rows
        foreach (var item in data)
        {
            var values = properties.Select(p =>
            {
                var val = p.GetValue(item, null);
                return val?.ToString()?.Replace(",", ";") ?? "";
            });
            sb.AppendLine(string.Join(",", values));
        }

        return sb.ToString();
    }
}
```

**Benefit:** Empowers advanced scenarios like dynamic serialization, object inspection, and tooling, especially useful for generic utilities.

## Caller Information Attributes

Another feature related to metadata is the information attribute, allowing you to fetch details about the caller from where the method is called, such as the caller's method name, calling line number, and file name. This information is helpful in logging, debugging, and error tracking.

```csharp
using System;
using System.Runtime.CompilerServices;

public class Logger
{
    public void Log(
        string message,
        [CallerMemberName] string memberName = "",
        [CallerLineNumber] int lineNumber = 0,
        [CallerFilePath] string filePath = "")
    {
        Console.WriteLine($"[{filePath} | Line {lineNumber} | {memberName}] {message}");
    }
}

// Caller code 
var logger = new Logger();
logger.Log("Something happened!");

// Output 
C:\Projects\CustomerCart\Program.cs | Line 32 | Main] Something happened!
```

**Benefit:** Improves debugging and logging by automatically capturing file, method, and line number info.

## Switch Expressions

Usually, defining a switch case statement involves multiple bracket statements and case conditions. However, the code can be simplified using a switch expression:

```csharp
var marks = 33;
string result = marks switch
{
    > 33 => "Pass",
    <= 33 => "Fail",
    _ => "Invalid"
};
// Result is "Fail"
```

**Benefit:** Offers a more expressive and compact syntax for simple conditions, improving readability and reducing verbosity.

## File-Scoped Namespaces

Namespace definition is simpler than ever if you are using C# 10+. Instead of this:

```csharp
namespace CustomerApp.Services
{
    public class OrdersService { }
}
```

You can use a flat structure like this:

```csharp
namespace CustomerApp.Services;

public class OrdersService { }
```

**Benefit:** Removes one level of indentation and clutter, making files easier to read and helping developers stay focused on the logic.

## Range and Index Operators

You can slice arrays and spans by:

```csharp
var lastTwo = items[^2..]; // returns last two elements
```

**Benefit:** Provides a clean way to access slices of collections using intuitive syntax, boosting readability and reducing off-by-one errors.

## Init-Only Setters

C# 9 brought init-only setters that create immutable objects, only allowing setters during object initialization. You can make init-only properties like this:

```csharp
public class Player
{
    public string Name { get; init; }
    public string JerseyNo { get; init; }
    public int Age { get; init; }
}
```

C# 11 built on this even further with the `required` modifier, which ensures that properties are always set (either through a constructor or object initializers):

```csharp
public class Player
{
    public required string Name { get; init; }
    public required string JerseyNo { get; init; }
    public int Age { get; init; }
}
```

**Benefit:** Enforces immutability while still allowing object initialization, improving safety and preventing accidental modification.

## Conclusion

C# is a powerful OOP language. Along with .NET, it is used in almost every domain, from mobile to desktop, from web to game development. In this post, I shared what may be some of the lesser-known features of C# that make this language more robust and advanced. You can use them to eliminate complexity, enhance maintainability, and reduce the chances of errors in your application. Luckily, tools like Visual Studio and Rider, plus static code analyzers like Roslynator and SonarQube, have become good companions for suggestions on many of the features mentioned above.

[

Previous post

16 common mistakes C#/.NET developers make (and how to avoid them)

](/16-common-mistakes-c-net-developers-make-and-how-to-avoid-them/)

[

Next post

Using Result<T> or OneOf<T> for Better Error Handling in .NET

](/using-result-t-or-oneof-t-for-better-error-handling-in-net/)

### [elmah.io](https://elmah.io): Error logging and Uptime Monitoring for your web apps

This blog post is brought to you by elmah.io. elmah.io is error logging, uptime monitoring, deployment tracking, and service heartbeats for your .NET and JavaScript applications. Stop relying on your users to notify you when something is wrong or dig through hundreds of megabytes of log files spread across servers. With elmah.io, we store all of your log messages, notify you through popular channels like email, Slack, and Microsoft Teams, and help you fix errors fast.

[![elmah.io app banner](https://blog.elmah.io/assets/img/elmahio-app-banner.webp?v=80cb303a5f)](https://elmah.io)

[See how we can help you monitor your website for crashes Monitor your website](https://app.elmah.io/signup/)

[Copyright](https://blog.elmah.io/copyright/)

•

[Guest posts](https://blog.elmah.io/guest-posts/)