```yaml
---
title: "Rejigs: Making Regular Expressions Human-Readable | by Omar | Jul, 2025 | Medium"
source: https://medium.com/@omarzawahry/rejigs-making-regular-expressions-human-readable-1fad37cb3eae
date_published: 2025-07-08T15:56:27.708Z
date_captured: 2025-08-17T22:11:26.320Z
domain: medium.com
author: Omar
category: backend
technologies: [Rejigs, .NET, NuGet, Regex]
programming_languages: [C#]
tags: [regex, regular-expressions, csharp, library, text-processing, code-readability, maintainability, fluent-api, pattern-matching, text-validation]
key_concepts: [regular-expressions, fluent-api, code-readability, maintainability, pattern-matching, text-validation, text-parsing, regex-options]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces Rejigs, a C# library designed to transform cryptic regular expressions into human-readable and maintainable code. It highlights the common problems with traditional regex, such as poor readability and difficult maintenance, and presents Rejigs as a solution using a fluent, English-like API. The library covers essential regex features like text matching, character classes, quantifiers, grouping, and alternation, with practical examples for email validation, log parsing, and URL validation. Rejigs generates standard .NET Regex objects, ensuring identical runtime performance while significantly improving code clarity and reducing errors. It serves as an excellent tool for developers seeking to simplify complex text processing tasks.]
---
```

# Rejigs: Making Regular Expressions Human-Readable | by Omar | Jul, 2025 | Medium

# Rejigs: Making Regular Expressions Human-Readable

![Profile picture of Omar, the author of the article.](https://miro.medium.com/v2/resize:fill:64:64/1*IJ5NwKGYSc19eFFthG628w@2x.jpeg)

[Omar](/@omarzawahry?source=post_page---byline--1fad37cb3eae---------------------------------------)

Follow

4 min read

·

Jul 8, 2025

37

2

Listen

Share

More

Press enter or click to view image in full size

![A full bookshelf with many books, symbolizing knowledge and organization, contrasting with the complexity of traditional regular expressions.](https://miro.medium.com/v2/resize:fit:700/0*BcmphBMElYjeUrQu)

Photo by [Aleksei Ieshkin](https://unsplash.com/@alekssei199?utm_source=medium&utm_medium=referral) on [Unsplash](https://unsplash.com/?utm_source=medium&utm_medium=referral)

Regular expressions are powerful, but let’s be honest — they’re also notoriously difficult to read and maintain. What does `^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$` mean at first glance? If you're like most developers, you'll need a moment (or several) to decode it.

Enter **Rejigs** — a fluent C# library that transforms the cryptic world of regex into readable, maintainable code. Instead of wrestling with arcane symbols, you can now build regular expressions using intuitive, English-like methods.

# The Problem with Traditional Regex

Traditional regular expressions suffer from several issues:

1.  **Readability**: Complex patterns are nearly impossible to understand without deep regex knowledge
2.  **Maintainability**: Modifying existing patterns is error-prone and time-consuming
3.  **Learning Curve**: New team members struggle with regex syntax
4.  **Documentation**: Patterns require extensive comments to explain their purpose

Consider this email validation regex:

^\[a-zA-Z0-9.\_%+-\]+@\[a-zA-Z0-9.-\]+\\.\[a-zA-Z\]{2,}$

What does it do? How would you modify it to allow underscores in the domain? These simple questions become complex puzzles.

# The Rejigs Solution

Rejigs transforms the same email validation into this:

var emailRegex =   
    Rejigs.Create()  
          .AtStart()  
          .OneOrMore(r => r.AnyLetterOrDigit().Or().AnyOf(".\_%+-"))  
          .Text("@")  
          .OneOrMore(r => r.AnyLetterOrDigit().Or().AnyOf(".-"))  
          .Text(".")  
          .AnyLetterOrDigit().AtLeast(2)  
          .AtEnd()  
          .Build();

Now the intent is crystal clear: start at the beginning, match one or more valid email characters, then an @ symbol, then domain characters, then a period, then at least two letters for the top-level domain, and end there.

# Core Features

## 1\. Text Matching and Anchoring

// Match exact text  
Rejigs.Create().Text("hello").Build();  // Matches "hello"

// Anchor to start/end  
Rejigs.Create()  
      .AtStart()  
      .Text("Hello")  
      .AtEnd()  
      .Build();  // Matches exactly "Hello", nothing more or less  
  
// Word boundaries  
Rejigs.Create()  
      .AtWordBoundary()  
      .Text("cat")  
      .AtWordBoundary()  
      .Build();  // Matches "cat" as a whole word, not as part of "category"

## 2\. Character Classes Made Simple

Instead of memorizing `\d`, `\w`, `\s` and their counterparts:

var regex =   
      Rejigs.Create()  
            .AnyDigit()           // \\d  
            .AnyLetterOrDigit()   // \\w    
            .AnySpace()           // \\s  
            .AnyCharacter()       // .  
            .AnyNonDigit()        // \\D  
            .Build();

// Character sets and ranges  
var phoneRegex =   
      Rejigs.Create()  
            .AnyOf("0123456789-() ")     // Custom character set  
            .AnyInRange('A', 'Z')        // Character range  
            .AnyExcept("@#$")            // Everything except these  
            .Build();

## 3\. Quantifiers That Make Sense

var passwordRegex =   
      Rejigs.Create()  
            .AtStart()  
            .AnyLetterOrDigit().AtLeast(8)        // At least 8 characters  
            .AnyDigit().Exactly(2)                // Exactly 2 digits  
            .AnyOf("!@#$").Between(1, 3)          // 1-3 special characters  
            .AtEnd()  
            .Build();

// Optional elements  
var urlRegex =   
      Rejigs.Create()  
            .AtStart()  
            .Optional("http://")                   // Optional protocol  
            .Optional(r => r.Text("www."))         // Optional www  
            .OneOrMore(r => r.AnyLetterOrDigit())  // Domain name  
            .Text(".")  
            .AnyLetterOrDigit()  
            .AtLeast(2)  
            .AtEnd()  
            .Build();

## 4\. Grouping and Alternation

// Either/or patterns  
var fileExtension =   
      Rejigs.Create()  
            .Text(".")  
            .Either(  
                r => r.Text("jpg"),  
                r => r.Text("png"),  
                r => r.Text("gif")  
            )  
            .Build();

// Complex grouping  
var phoneNumber =   
      Rejigs.Create()  
            .Optional(r => r.Text("+").AnyDigit().Between(1, 3))  // Country code  
            .Grouping(r => r.AnyDigit().Exactly(3))               // Area code  
            .Optional("-")  
            .Grouping(r => r.AnyDigit().Exactly(3))               // Exchange  
            .Optional("-")  
            .Grouping(r => r.AnyDigit().Exactly(4))               // Number  
            .Build();

# Real-World Examples

## Parsing Log Files

var logEntryRegex =   
      Rejigs.Create()  
            .AtStart()  
            .Grouping(r => r.AnyDigit().Exactly(4))  // Year  
            .Text("-")  
            .Grouping(r => r.AnyDigit().Exactly(2))  // Month  
            .Text("-")  
            .Grouping(r => r.AnyDigit().Exactly(2))  // Day  
            .AnySpace().OneOrMore()                   // Whitespace  
            .Grouping(r => r.AnyExcept(" ").OneOrMore()) // Log level  
            .AnySpace().OneOrMore()  
            .Grouping(r => r.AnyCharacter().ZeroOrMore()) // Message  
            .Build();

## URL Validation

var urlRegex =   
      Rejigs.Create()  
            .AtStart()  
            .Either(  
                r => r.Text("http://"),  
                r => r.Text("https://")  
            )  
            .Optional(r => r.Text("www."))  
            .OneOrMore(r => r.AnyLetterOrDigit().Or().AnyOf(".-"))  
            .Optional(r => r.Text(":").AnyDigit().OneOrMore())  // Port  
            .Optional(r => r.Text("/").AnyCharacter().ZeroOrMore()) // Path  
            .AtEnd()  
            .Build();

# Advanced Features

## Raw Patterns When Needed

Sometimes you need the full power of regex. Rejigs doesn’t restrict you:

var advancedRegex =   
      Rejigs.Create()  
            .Text("start")  
            .Pattern(@"(?=.\*\\d)(?=.\*\[a-z\])(?=.\*\[A-Z\])")  // Lookaheads  
            .Text("end")  
            .Build();

## Regex Options Support

var caseInsensitiveRegex =   
        Rejigs.Create()  
              .Text("Hello")  
              .Build(RegexOptions.IgnoreCase | RegexOptions.Multiline);

# Why Choose Rejigs?

## 1\. Self-Documenting Code

Your regex patterns become their own documentation. New team members can understand what a pattern does just by reading the method calls.

## 2\. Easier Maintenance

Need to modify a pattern? Change the fluent method calls instead of deciphering regex syntax. Want to make email domains case-insensitive? Add `.Build(RegexOptions.IgnoreCase)`.

## 3\. Reduced Errors

The fluent API guides you toward correct patterns. No more wondering if you need to escape that dot or whether you have the right number of backslashes.

## 4\. Better Testing

You can build patterns incrementally and test each part:

var basePattern = Rejigs.Create().AtStart().Text("user");  
var withDomain = basePattern.Text("@").OneOrMore(r => r.AnyLetterOrDigit());  
var complete = withDomain.Text(".com").AtEnd();

// Test each stage  
Assert.IsTrue(basePattern.Build().IsMatch("user@example.com"));  
Assert.IsTrue(withDomain.Build().IsMatch("user@example.com"));  
Assert.IsTrue(complete.Build().IsMatch("user@example.com"));

## 5\. Learning Tool

Rejigs helps developers learn regex concepts without getting lost in syntax. You can always call `.Expression` to see the generated regex pattern and learn how traditional regex works.

# Getting Started

## Installation

dotnet add package Rejigs

## Basic Usage

using Rejigs;

// Create a simple pattern  
var pattern =   
      Rejigs.Create()  
            .Text("Hello")  
            .AnySpace()  
            .Text("World")  
            .Build();  
  
// Use it like any Regex  
bool matches = pattern.IsMatch("Hello World");  
string result = pattern.Replace("Hello World", "Hi Earth");

# Performance Considerations

Rejigs generates standard .NET `Regex` objects, so runtime performance is identical to hand-written regex patterns. The only overhead is during pattern construction, which typically happens once during application startup.

For frequently used patterns, consider caching the built `Regex` object:

public static class CommonPatterns   
{  
    public static readonly Regex Email =   
                                    Rejigs.Create()  
                                          .AtStart()  
                                          .OneOrMore(r => r.AnyLetterOrDigit().Or().AnyOf(".\_%+-"))  
                                          .Text("@")  
                                          .OneOrMore(r => r.AnyLetterOrDigit().Or().AnyOf(".-"))  
                                          .Text(".")  
                                          .AnyLetterOrDigit().AtLeast(2)  
                                          .AtEnd()  
                                          .Build();  
}

# When to Use Rejigs

**Perfect for:**

*   Complex validation patterns
*   Data parsing and extraction
*   Pattern matching in business logic
*   Teams with mixed regex experience levels
*   Applications requiring maintainable text processing

**Consider alternatives when:**

*   You need maximum performance for simple patterns
*   Working with regex experts who prefer traditional syntax
*   Building very simple patterns (like single-character matches)

# Conclusion

Regular expressions don’t have to be cryptic. Rejigs brings clarity to pattern matching, making your code more readable, maintainable, and accessible to your entire team.

Whether you’re validating user input, parsing log files, or extracting data from text, Rejigs helps you express your intent clearly while leveraging the full power of .NET’s regex engine.

Try Rejigs today and transform your regex patterns from mysterious hieroglyphs into readable, maintainable code that your future self (and your teammates) will thank you for.

**Learn More:**

*   [GitHub Repository](https://github.com/omarzawahry/Rejigs)
*   [NuGet Package](https://www.nuget.org/packages/Rejigs)
*   [Documentation and Examples](https://github.com/omarzawahry/Rejigs/blob/main/README.md)