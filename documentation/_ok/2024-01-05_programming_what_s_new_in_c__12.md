```yaml
---
title: What’s new in C# 12
source: https://okyrylchuk.dev/blog/whats-new-in-csharp-12/
date_published: 2024-01-05T11:06:44.000Z
date_captured: 2025-08-11T16:14:23.192Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: programming
technologies: [.NET 8, C# 12, System.Span, System.ReadOnlySpan, System.Collections.Generic.List, System.Guid, System.DateTime, System.TimeSpan, System.Console, System.Runtime.CompilerServices, Microsoft]
programming_languages: [C#]
tags: [csharp, dotnet, csharp-12, language-features, new-features, programming, collection-expressions, primary-constructors, interceptors, performance]
key_concepts: [collection expressions, primary constructors, default lambda parameters, type aliasing, ref readonly parameters, inline arrays, experimental attribute, interceptors]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides an overview of the new features introduced in C# 12, released alongside .NET 8. It details eight key enhancements, beginning with collection expressions for more concise collection initialization and primary constructors for classes and structs. The post also covers the addition of default parameters to lambda expressions and the expanded capability to alias various types. Furthermore, it explains `ref readonly` parameters for passing immutable references, inline arrays for performance optimization, and the new `Experimental` attribute for marking experimental APIs. The article concludes with a deep dive into interceptors, an experimental feature enabling compile-time method call substitution.
---
```

# What’s new in C# 12

# What’s new in C# 12

Microsoft has released .NET 8 on November 14, 2023. The new release brought to us new C# 12. Let’s take a look at them.

## Collection expressions

Collection expressions present a concise syntax for generating familiar collection values. You can incorporate other collections into these values by employing the spread operator **..** for inline inclusion.

The supported types are:

*   **Array types**
*   **System.Span<T>** and **System.ReadOnlySpan<T>**
*   Types that support collection initializers, such as **System.Collections.Generic.List<T>**

```csharp
int[] numbers1 = [1, 2, 3];
int[] numbers2 = [4, 5, 6];
int[] combinedNumbers = [.. numbers1, .. numbers2];
char[] chars1 = ['d', 'e'];
List<char> chars = ['a', 'b', 'c', .. chars1];
```

## Primary constructors

If you are familiar with [Records](/blog/records-in-csharp/ "Records") introduced in C# 9, the primary constructor’s feature is not new. The positional records use primary constructors. The compiler generates the public properties for its parameters. They are referred to as positional parameters.

Naturally, the primary constructors come to **classes** and **structs**. Yet, for **classes** and **structs**, it doesn’t generate public properties. Primary constructor parameters are in scope for the entire body of the class but are not members of the class.

When you declare any constructor explicitly, it must call the primary constructor using **this()** syntax. It ensures that all primary constructor parameters are assigned.

```csharp
public class Person(Guid id, string name, string surname)
{
    public Person(string name, string surname)
        : this(Guid.NewGuid(), name, surname)
    { }
    public Guid Id => id;
    public string Name => name;
    public string Surname => surname;
}
```

## Default lambda parameters

Previously, you could define default parameters in methods, constructors, indexers, and delegates in C#.

The time came for **lambda expressions**. Now you can also define default parameters for them.

You can also access the default value of lambda via reflection, similar to other default values.

```csharp
var incrementNumber = (int number = 0) => number + 1;
incrementNumber(); // 1
incrementNumber(5); // 6
var defaultValue = incrementNumber
    .Method
    .GetParameters()
    .First()
    .DefaultValue;
```

## Alias any type

You can now alias almost any type in C#. You can create semantic aliases for tuple types, array types, pointer types, and unsafe types.

The example below shows how you can use the **alias** for tuple type.

```csharp
using Temperature = (float Degrees, string Unit);
void PrintTemperature(Temperature temperature) =>
    Console.WriteLine($"Temperature: {temperature.Degrees} {temperature.Unit}");
var temperature = new Temperature
{
    Degrees = 36.6f,
    Unit = "Celsius"
};
PrintTemperature(temperature); // Temperature: 36.6 Celsius
```

## ref readonly parameters

C# 7.2 introduced **in** parameter to pass readonly references.

C# 12 introduces **ref readonly** which basically does the same thing as **in** parameter. It’s used to passes readonly references.

Why was this feature added, you may ask? The answer you can find in the Microsoft documentation: “APIs which capture or return references from their parameters would like to disallow **rvalues** and also enforce some indication at the callsite that a reference is being captured. **ref readonly** parameters are ideal in such cases as they warn if used with **rvalues** or without any annotation at the callsite.”

What is **rvalue**? Let’s take a look on example **int x = 1;**

**Rvalue** is a right-hand side value. In the example it’s **1**. It’s a temporary value on the right side of the assignment. It doesn’t persist in the memory.

On the other hand, **x** is a **lvalue**. It’s left-hand side value. It represents a variable with a specific memory location.

```csharp
TimeSpan SubsctractDate(ref readonly DateTime x, ref readonly DateTime y)
{
    // Can't modify x and y values
    x = DateTime.UtcNow;
    y = DateTime.UtcNow;
    return x - y;
}
var x = new DateTime(2024, 1, 4);
var y = new DateTime(2024, 1, 1);
SubsctractDate(x, y);
SubsctractDate(ref x, ref y);
SubsctractDate(in x, in y);
```

## Inline arrays

Inline arrays allow creating an array of fixed size in a struct type. It provides performance characteristics similar to an unsafe fixed-size buffer. The feature is used by the runtime team and other library authors to improve performance.

It’s not a feature for everyday usage. However, it is available for performance improvements.

To declare the feature you have to use an attribute **InlineArrays** over the **struct** type. Inline array struct must declare one and only one instance field which must not be a ref field.

```csharp
var array = new InlineArray[10];
for (int i = 0; i < array.Length; i++)
    array[i] = i;
[InlineArray(10)]
public struct InlineArray
{
    private int _element;
}
```

## Experimental attribute

C# introduces a new **Experimental** attribute. If you mark with it any type, method, or assembly, you indicate it as an experimental feature.

The compiler issues a warning if you access a method or type annotated with the **ExperimentalAttribute** with **diagnosticId** you pass into its constructor as a parameter.

If you put the attribute on assembly, then all types, included in it, will be marked as experimental.

```csharp
#pragma warning disable MyCode007
var api1 = new ExampleApi();
#pragma warning restore MyCode007
var api2 = new ExampleApi();
[Experimental("MyCode007")]
public class ExampleApi
{ }
```

## Interceptors

Interceptors generally refer to a mechanism or component that intercepts or observes operations or events in a system, allowing you to take specific actions before, during, or after the execution of these operations. The purpose of interceptors is to enable developers to introduce additional behavior or logic without modifying the core functionality of the system.

New interceptors in C# allow you to substitute a call to an interceptable method with a call to itself at compile time. This substitution occurs by having the interceptor declare the source locations of the calls that it intercepts.

To enable the feature you need to add the property **<InterceptorsPreviewNamespaces>**. It contains a list of namespaces which are allowed to contain interceptors.

For example:

```xml
<PropertyGroup>
  <OutputType>Exe</OutputType>
  <TargetFramework>net8.0</TargetFramework>
  <ImplicitUsings>enable</ImplicitUsings>
  <Nullable>enable</Nullable>
  <InterceptorsPreviewNamespaces>
      $(InterceptorsPreviewNamespaces);InterceptorsPreview
  </InterceptorsPreviewNamespaces>
</PropertyGroup>
```

Let’s add **the MyService** example class with the **Print** method which we want to intercept.

```csharp
var myService = new MyService();
myService.Print("Hello");
public class MyService
{
    // Method to be intercepted
    public void Print(string message)
        => Console.WriteLine($"Message: {message}");
}
```

Then you need to create the **InterceptsLocationAttribute** in the **System.Runtime.CompilerServices** namespace.

```csharp
namespace System.Runtime.CompilerServices
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public sealed class InterceptsLocationsAttribute : Attribute
    {
        public InterceptsLocationsAttribute(string filePath, int line, int character) { }
    }
}
```

Then you add your interceptors. Remember about your allowed namespace for interceptors. While adding an attribute, you need to specify the path to your file where your intercepted method is invoked. Also, you need to provide a line number and the start character of the method name. If you change the location of the invoking method, the interceptor won’t work.

```csharp
namespace InterceptorsPreview
{
    public static class Interceptors
    {
        [InterceptsLocations(
            @"D:\[Path_to_the_project]\InterceptorsPreview\Program.cs",
            2,
            11)]
        public static void InterceptPrintMethod(this MyService service, string message)
        {
            service.Print(message);
            Console.WriteLine($"Interceptor message: {message}");
        }
    }
}
```

The method **Print is** intercepted and its implementation will be substituted with a new implementation of **InterceptMethodPrint**. However, in my example, I’m invoking also the origin implementation at the beginning.

The console output is the following:

![Image description](https://ci3.googleusercontent.com/meips/ADKq_NZa2EInwyeUw0nhmtCg-iPuK_KN5kgr_A7Q3wo0prhHi6ZHLQvAB85Hdg9djFpAuFp9W8x9igavrVzg3KPPyt06FzNcHeQc4ylNBrH1dJM1hx0RaxtjNxw9N6tt0w=s0-d-e1-ft#https://cdn.sender.net/email_images/226571/images/all/consoleoutput.png)