```yaml
---
title: How to Work with String Literals in C#
source: https://okyrylchuk.dev/blog/how-to-work-with-string-literals-in-csharp/
date_published: 2024-03-22T19:25:02.000Z
date_captured: 2025-08-20T18:56:44.504Z
domain: okyrylchuk.dev
author: Oleg Kyrylchuk
category: programming
technologies: [.NET, C# 11]
programming_languages: [C#]
tags: [string-literals, csharp, dotnet, text-manipulation, string-interpolation, raw-strings, verbatim-strings, code-readability]
key_concepts: [quoted-string-literals, verbatim-string-literals, raw-string-literals, string-interpolation, escaping-characters, compiler-optimizations, multiline-strings]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides a comprehensive guide to working with string literals in C#. It explains the different types of string literals, including quoted, verbatim, and raw string literals, highlighting their uses and advantages. The article also covers string interpolation, demonstrating how to embed expressions within strings for enhanced readability and dynamic content generation. Specific attention is given to features introduced in C# 11, such as multiline interpolated expressions and advanced raw string literal escaping. Numerous code examples illustrate each concept, making it easy for developers to understand and apply these string handling techniques.
---
```

# How to Work with String Literals in C#

# How to Work with String Literals in C#

![Author's avatar](https://okyrylchuk.dev/wp-content/litespeed/avatar/a85a89199a7d4486a29f67dc2fc371d0.jpg?ver=1755571532)

String literals in C# are vital for representing fixed character sequences directly within code, enhancing readability and clarity. They simplify string handling, support interpolation, and facilitate efficient memory usage through compiler optimizations like string interning. Thus, string literals are essential components that empower developers to work effectively with textual data.

## Quoted String Literals

These literals are the most often used because they suit most straightforward cases.

The literals start and end with a single double-quote character (`"`).

```csharp
var message = "Hello, World";
Console.WriteLine(message);
// Hello, World
```

We use them for single-line strings where no escaping is required.

But what if the string is more complex? Let’s say we have a piece of HTML.

```html
<div class="greeting">    
        Hello, World
</div>
```

Then our quoted string literal gets messy.

```csharp
var html =
    "<div class=\\"greeting\\">\\r\\n\\tHello, World\\r\\n</div>";
Console.WriteLine(html);
//<div class= "greeting">
//    Hello, World
//</div >
```

It gets even worse if we want to reflect the lines in the code. We need to break the lines and use concatenation.

```csharp
var html = "<div class=\\"greeting\\">" +
    "\\r\\n\\tHello, World\\r\\n" +
    "</div>";
```

## Verbatim String Literals

Verbatim string literals are a better choice for multi-line strings. To define a verbatim string literal, we put an at (`@`) sign at the beginning of the string.

```csharp
var html = @"<div class=""greeting"">
    Hello, World
</div>";
Console.WriteLine(html);
//<div class= "greeting">
//    Hello, World
//</div >
```

It looks better without concatenation and escaping characters. Verbatim literals basically mean display it “as is”.

Only a quote escape sequence (`""`) isn’t interpreted literally. It’s used to produce one double quotation mark.

The disadvantage of verbatim literal is indentations. You could notice the weird indentation in the literal but correct display in the console. That’s because the indentations are interpreted literally.

Using verbatim literals in the nested code can be a problem. 

```csharp
void Test()
{
    if (true)
    {
        var html = @"<div class=""greeting"">
    Hello, World
</div>";

        Console.WriteLine(html);
        //<div class="greeting">
        //    Hello, World
        //</div >
    }
}
```

It could look more pretty.

## Raw String Literals

The raw string literals were introduced in C# 11.

They can contain whitespaces, new lines, embedded quotes, and other special characters without escape sequences.

The raw literal starts with at least three double quotes (`"""`) and ends with the same number of quotes. The starting and ending quotes must be in separate lines, and the content between them. 

```csharp
var html = """
            <div class=""greeting"">
                Hello, World
            </div>
            """;
Console.WriteLine(html);
//<div class= "greeting">
//    Hello, World
//</div >
```

The raw literals also fix the issue with indentation, which verbatim literals have.

Why should raw literals start with at least three double quotes? Well, if you have three double quotes in your text, you should put four double quotes at the start and end of the literal, and so on. 

```csharp
var text = """"
    The text with """ double quotes
    """";
Console.WriteLine(text);
// The text with """ double quotes
```

## String Interpolation

String interpolation allows us to insert an expression into a string literal.

It’s easy with quoted string literals. You should put a dollar sign (`$`) before the literal and use curly braces for the expression.

```csharp
var user = "Oleg";
var text = $"Hello, {user}";
Console.WriteLine(text);
// Hello, Oleg
```

Since C# 11, we can use multiline interpolated expressions. It can improve readability within curly braces.

```csharp
var user = "Oleg";
var text = $"Hello, {
    (user == "Oleg" 
        ? "me" 
        : user)}";
Console.WriteLine(text);
// Hello, me
```

Also, we can use interpolation in the verbatim literals. We should put a dollar sign before or after at sign (`$@`), (`@$`). The order doesn’t matter.

```csharp
var user = "Oleg";
var html = $@"
<div class=""greeting"">
    Hello, {user}
</div>";
Console.WriteLine(html);
//<div class= "greeting">
//    Hello, Oleg
//</div >
```

The most interesting is interpolation in the raw string literals. For basic, where you don’t have curly braces in the text, it’s the same as for other literals. You should put a dollar sign (`$`) before triple double quotes.

```csharp
var user = "Oleg";
var html = $"""
    <div class=""greeting"">
        Hello, {user}
    </div>
    """;
Console.WriteLine(html);
//<div class= "greeting">
//    Hello, Oleg
//</div >
```

Let’s say we want to set up JSON for our unit tests. JSON contains curly braces, so we need to use double curly braces for interpolation.

To escape JSON’s curly braces, we need to put two dollar signs, which means we need to use double curly braces for interpolation.

```csharp
var user = "Oleg";
var html = $$"""
            {
                "Name": "{{user}}"
            }
            """;
Console.WriteLine(html);
//{
//    "Name": "Oleg"
//}
```

If you have double curly braces in your text, you should put three dollar signs at the literal start and use triple curly braces for interpolation.

![Placeholder image for related content](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMjgwIiBoZWlnaHQ9IjcyMCIgdmlld0JveD0iMCAwIDEyODAgNzIwIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBzdHlsZT09ImZpbGw6I2NmZDRkYjtmaWxsLW9wYWNpdHk6IDAuMTsiLz48L3N2Zz4=)
![Placeholder image for related content](data:image/svg+xml;base64,PHN2ZyB4bWxucz0iaHR0cDovL3d3dy53My5vcmcvMjAwMC9zdmciIHdpZHRoPSIxMjgwIiBoZWlnaHQ9IjcyMCIgdmlld0JveD0iMCAwIDEyODAgNzIwIj48cmVjdCB3aWR0aD0iMTAwJSIgaGVpZ2h0PSIxMDAlIiBzdHlsZT09ImZpbGw6I2NmZDRkYjtmaWxsLW9wYWNpdHk6IDAuMTsiLz48L3N2Zz4=)