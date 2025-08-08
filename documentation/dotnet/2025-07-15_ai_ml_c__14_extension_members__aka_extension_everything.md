# C# 14 extension members; AKA extension everything

**Source:** https://andrewlock.net/exploring-dotnet-10-preview-features-3-csharp-14-extensions-members/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2096
**Date Captured:** 2025-07-28T15:57:12.980Z
**Domain:** andrewlock.net
**Author:** Unknown
**Category:** ai_ml

---

**Sponsored by** [**Dometrain Courses**](https://dometrain.com/dometrain-pro/?ref=andrew-lock&promo=banner&coupon_code=ANDREW30)—Get 30% off [**Dometrain Pro**](https://dometrain.com/dometrain-pro/?ref=andrew-lock&promo=banner&coupon_code=ANDREW30) with code [**ANDREW30**](https://dometrain.com/dometrain-pro/?ref=andrew-lock&promo=banner&coupon_code=ANDREW30) and access the best courses for .NET Developers

July 15, 2025 ~9 min read

*   [.NET 10](/tag/net-10/)
*   [C#](/tag/c/)

# C# 14 extension members; AKA extension everything

[Exploring the .NET 10 preview - Part 3](/series/exploring-the-dotnet-10-preview/)

Share on:

*   [Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-3-csharp-14-extensions-members%2F&t=C%23+14+extension+members%3B+AKA+extension+everything "Share on Facebook")
*   [](https://twitter.com/intent/tweet?source=https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-3-csharp-14-extensions-members%2F&text=C%23+14+extension+members%3B+AKA+extension+everything:https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-3-csharp-14-extensions-members%2F "Tweet")
*   [](https://twitter.com/intent/tweet?source=https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-3-csharp-14-extensions-members%2F&text=C%23+14+extension+members%3B+AKA+extension+everything:https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-3-csharp-14-extensions-members%2F "Share on Bluesky")
*   [](http://www.reddit.com/submit?url=https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-3-csharp-14-extensions-members%2F&title=C%23+14+extension+members%3B+AKA+extension+everything "Submit to Reddit")
*   [](http://www.linkedin.com/shareArticle?mini=true&url=https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-3-csharp-14-extensions-members%2F&title=C%23+14+extension+members%3B+AKA+extension+everything&source=https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-3-csharp-14-extensions-members%2F "Share on LinkedIn")

This is the third post in the series: [Exploring the .NET 10 preview](/series/exploring-the-dotnet-10-preview/).

1.  [Part 1 - Exploring the features of dotnet run app.cs](/exploring-dotnet-10-preview-features-1-exploring-the-dotnet-run-app.cs/)
2.  [Part 2 - Behind the scenes of dotnet run app.cs](/exploring-dotnet-10-preview-features-2-behind-the-scenes-of-dotnet-run-app.cs/)
3.  Part 3 - C# 14 extension members; AKA extension everything (this post)
4.  [Part 4 - Solving the source generator 'marker attribute' problem in .NET 10](/exploring-dotnet-10-preview-features-4-solving-the-source-generator-marker-attribute-problem-in-dotnet-10/)

In this post I take a short look at the C#14 _extension members_ feature which is available in the latest .NET 10 previews. In this post I describe what the feature is, how it extends the existing capabilities of extension methods, and how to create extension members in .NET 10. Finally, I show how I added support for the feature in the latest version of my [NetEscapades.EnumGenerators](https://github.com/andrewlock/NetEscapades.EnumGenerators) NuGet package.

> This post was written using the features available in .NET 10 preview 5. Many things may change between now and the final release of .NET 10.

## [Background: Extension methods](#background-extension-methods)

Extension methods were introduced _way_ back in 2007 with .NET Framework 3.5 as a supporting feature for [LINQ](https://learn.microsoft.com/en-us/dotnet/csharp/linq/). They provided a way to emulate adding an instance method to a type, by writing a `static` method. For example, you could add an extension method to `IEnumerable` called `IsEmpty()`:

Copy
```csharp
public static class EnumerableExtensions
{
    public static bool IsEmpty<T>(this IEnumerable<T> target)
        => !target.Any();
}
```

The extension method is pretty much just an "ordinary" extension method. You can call it the same way you would call any other `static` method in a class if you like:

Copy
```csharp
var values = Enumerable.Empty<string>();
if (EnumerableExtensions.IsEmpty(values)) // Calling the extension method using the type
{
    Console.WriteLine("Enumerable is empty");
}
```

However, the `this` modifier on the first argument indicates that the method can _also_ be called as an instance method, on the _receiver_ type, `IEnumerable<T>`. This is the typical way to call extension methods and results in terser code overall:

Copy
```csharp
var values = Enumerable.Empty<string>();
if (values.IsEmpty()) // Invoking as an extension method
{
    Console.WriteLine("Enumerable is empty");
}
```

The instance method approach is generally used everywhere possible, and the static invocation is only used where disambiguation is required due to naming clashes. Today, extension methods are used in a diverse range of scenarios and use many different patterns, but you will find them all over most code bases; including in the .NET runtime itself.

## [Extension members and extension everything](#extension-members-and-extension-everything)

Shortly after extension methods were introduced, way back in C# 3.0, there were requests to allow additional extension members. "Instance" extension methods were great, but why not extension properties? Why not _static_ extension methods?

Well apparently, for C# 4.0, an attempt to design the "extension everything" feature was explored, but ultimately failed. Periodically [new efforts to try to reimagine extension everything](https://github.com/dotnet/csharplang/blob/90ed288e3c4871b1d19862a418bced7dafa12721/meetings/2016/LDM-2016-05-10.md) would flare up, but ultimately they came to nothing. Until now.🎉 In C# 14.0, finally, extension everything is essentially here, under the name _extension members_.

Before we look at how to define extension members, a word of warning. The extension members feature introduces some new syntax, which can often be controversial. You may well think it's ugly, and to be honest, I probably wouldn't disagree with you. Ultimately, the C# design team opted to optimize the experience of _using_ the extension members over brevity of the syntax used to _author_ extension members, which I think makes sense.

> The [blog post announcing extension members in .NET 10 preview 3](https://devblogs.microsoft.com/dotnet/csharp-exploring-extension-members/) is great and covers a lot of the design decisions about the current feature, so I encourage you to check it out after this post, if you haven't already!

We'll start by looking at how to convert an existing extension method to use the new extension member syntax as a way of introducing the changes.

### [Converting an extension method to an extension member](#converting-an-extension-method-to-an-extension-member)

Before I show some code, it's important to know that you _don't_ have to convert your extension methods over to the new syntax; the new syntax is entirely optional. Your existing extension methods will continue to work with the old `this` syntax without any changes:

Copy
```csharp
public static class EnumerableExtensions
{
    // The "old" extension member syntax still works in .NET 10 and C#14
    public static bool IsEmpty<T>(this IEnumerable<T> target)
        => !target.Any();
}
```

If you wish to explore the new syntax, then you need to make sure you're using C#14 in your project. While .NET 10 is still in preview, that means you need to set the `<LangVersion>` property to `preview`, _even if you're using the .NET 10 SDK and targeting_ `net10.0`:

Copy
```xml
<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <!-- 👇 You can set any target framework as long as you're using the .NET 10 SDK...-->
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <!-- ... but you must set this 👇-->
    <LangVersion>preview</LangVersion>
  </PropertyGroup>

</Project>
```

If you wish to convert your method to the new extension member syntax, you need to do four things:

1.  Add an `extension(){ }` block around your method.
2.  Move the receiver parameter, `IEnumerable<T> target` in this case, as a "parameter" to the `extension` block.
3.  Move the generic type arguments, `T` in this case, from the method to the `extension` block.
4.  Remove the `static` modifier from the method to make it an instance method.

> Note that not _all_ extension methods can be converted to the new syntax, as described [in this post](https://devblogs.microsoft.com/dotnet/csharp-exploring-extension-members/).

Putting that all together, the new extension member implementation looks like the following:

Copy
```csharp
public static class EnumerableExtensions
{
    extension<T>(IEnumerable<T> target)
    {
        public bool IsEmpty() => !target.Any();
    }
}
```

which makes all the changes I described above:

Copy
```csharp
public static class EnumerableExtensions
{
    // 👇 extension block with type parameters and receiver parameter
    extension<T>(IEnumerable<T> target)
    {
        //   👇 no static modifier
        public bool IsEmpty() => !target.Any();
        //                 👆 'this' receiver parameter  removed

    }
}
```

The `extension` block is the key to the new syntax, and is likely where developer "feelings" are going to come out i.e. how much do you dislike this new unusual syntax and the extra nesting it requires 😅. Of course, there's very good reason for this syntax, and we'll explore that more in the next section, but there's some aspects which are clearly quite nice about it. For example, a nice side-effect is that the instance extension method now looks more like it does when it's _invoked_; i.e. its a zero-parameter instance method instead of a one parameter static method.

One downside of the very new syntax, is that it confuses any tooling that isn't aware of it 😅 I haven't installed [the Rider EAP](https://www.jetbrains.com/rider/nextversion/) yet to see if it handles this syntax, but I don't think it does yet. Visual Studio preview does support the syntax however, and even includes a code fix to automatically convert to the new syntax if you wish:

![Visual Studio provides a code fix to convert to extension members](/content/images/2025/extension_everything.png)

There's no particularly compelling reason to convert extension methods to the extension member syntax if you're not doing anything else. It's purely a syntactic difference; they both compile to the exact same thing. The more interesting case is if you want to use other _types_ of extension member.

### [Adding other extension members](#adding-other-extension-members)

We've seen the new syntax, but we haven't seen any new _features_ yet. However, the extension members feature that shipped in .NET 10 preview 3 adds support for three new _types_ of extension member

*   Static extension methods
*   Static extension properties
*   Instance extension properties

Additionally, extension operators have already been implemented and should be available in .NET 10 preview 7.

I'll start with static extension methods, because they're the most interesting to me. _Implementation_ wise, the syntax looks very similar to the instance extension method syntax we've already seen, with two minor differences:

1.  The method should be `static`
2.  You don't need to specify a name for the receiver parameter (and even if you add it, it can't be accessed from the `static` method you define)

Let's look at an example. The following creates a static method on `string` called `HasValue()` which is the inverse of `string.IsNullOrEmpty`

Copy
```csharp
public static class StringExtensions
{
    // 👇 Extension block specifies receiver type but doesn't specify a parameter name
    extension(string)
    {
        //    👇 static extension method
        public static bool HasValue(string value)
            => !string.IsNullOrEmpty(value);
    }
}
```

The following shows an example of its use:

Copy
```csharp
string someValue = "something";
if (string.HasValue(someValue))
{
    Console.WriteLine("The value was: " + someValue);
}
```

The revelation here, and the difference from instance extension methods, is that the extension method is added to the `string` _type_, not the `someValue` _variable_.

Next, we'll look at an extension property. The following adds an `IsAscii` instance extension property to string:

Copy
```csharp
public static class StringExtensions
{
    // 👇 Extension block specifies receiver type and target parameter
    extension(string target)
    {
        // 👇 You can just write the property like you'd write a "normal" property
        public bool IsAscii
            => target.All(x => char.IsAscii(x));
        //    👆 You can access the receive paramter because it's an instance extension
    }
}
```

You can then use the property on `string` instances, just as you would for an instance extension method

Copy
```csharp
string someValue = "something";
bool isAscii = someValue.IsAscii; // 👈 Access the instance extension property on a variable
```

Defining a static extension property is very similar, so for the sake of brevity I'll leave that for now. As a brief bonus treat, here's what a static operator definition might look like:

Copy
```csharp
static class PathExtensions
{
    extension(string)
    {
        public static string operator /(string left, string right)
            => Path.Combine(left, right);
    }
}
```

This could then be used something like the following:

Copy
```csharp
var fullPath = "part1" / "part2" / "test.txt"; // "part1/part2/test.txt"
```

So hold on for .NET 10 preview 7 for that!

### [Disambiguating extension members](#disambiguating-extension-members)

Before we move on, it's worth addressing the disambiguation story. Sometimes you may need to invoke the extension methods directly, typically due to clashes between multiple extension methods with the same signature. This has always been the way to handle the ambiguity for extension methods, and it's essentially the same for other extension members.

*   For instance extension methods, you invoke statically and pass the instance as the first parameter.
*   For static extension methods, you invoke as a simple zero parameter `static` method.
*   For instance extension properties, you call the `get_` prefixed method, and pass the instance as the first parameter.
*   For static extension methods, you call the `get_` prefixed method.

For example, for the extensions shown so far we might invoke them directly like this

Copy
```csharp
bool hasValue = EnumerableExtensions.IsEmpty([]); // An instance extension method
bool hasValue = StringExtensions.HasValue("something"); // A static extension method
bool isAscii = StringExtensions.get_IsAscii("something"); // An instance extension property
```

The `get_` prefix for the property might seem strange given normal C# naming, but it's actually how properties are _normally_ implemented behind the scenes in C#. Indeed if you looks at the assembly metadata in a .NET dll using a tool such as [dotPeek](https://www.jetbrains.com/decompiler/), then you'll see that properties really _are_ implemented as methods with `get_` and `set_` prefixes:

![Looking at the metadata in a dll](/content/images/2025/extension_everything_2.png)

The screenshot above shows that once compiled, the `StringExtensions.IsAscii` extension property has a very similar implementation to "normal" properties like this:

Copy
```csharp
public class SomeClass
{
    public string Value { get; set; }
}
```

That pretty much covers everything for this post, so I'll finish with a quick update on my [NetEscapades.EnumGenerators](https://github.com/andrewlock/NetEscapades.EnumGenerators) NuGet package.

## [A case study: NetEscapades.EnumGenerators](#a-case-study-netescapades-enumgenerators)

The [NetEscapades.EnumGenerators](https://github.com/andrewlock/NetEscapades.EnumGenerators) NuGet package lets you generate extension methods and helpers for enums, to allow fast "reflection". You use it by adding the `[EnumExtensions]` attribute to an enum:

Copy
```csharp
[EnumExtensions]
public enum MyColours
{
    Red,
    Green,
    Blue,
}
```

and by default the package generates a class called `MyColoursExtensions` similar to the following:

Copy
```csharp
public static partial class MyColoursExtensions
{
    public static string ToStringFast(this global::MyColours value) { }

    public static int AsUnderlyingType(this global::MyColours value) { }

    public static bool IsDefined(global::MyColours value) { }

    public static global::MyColours Parse(string? name) { }

    // ...
}
```

I've only shown a couple of the methods above but as you can see, there's basically two different types of method:

*   Extension methods like `ToStringFast()` and `AsUnderlyingType()`
*   Static methods like `IsDefined()` and `Parse()`

Until now, you always had to call the static methods using the `MyColoursExtensions` type, e.g.

Copy
```csharp
var colour = MyColoursExtensions.Parse("Red");
```

But if you're targeting C#14, the package simply adds an extension block around these members:

Copy
```csharp
public static partial class MyColoursExtensions
{
    public static string ToStringFast(this global::MyColours value) { }

    public static int AsUnderlyingType(this global::MyColours value) { }

    // 👇 Added if you target C#14
    extension(global::MyColours)
    {
        public static bool IsDefined(global::MyColours value) { }

        public static global::MyColours Parse(string? name) { }

        // ...
    }
}
```

and now those static extension members are available on the `MyColours` enum type itself:

Copy
```csharp
var colour = MyColours.Parse("Red");
```

which just feels much nicer to me! Anyway, the fact that it was frankly a trivial change to convert these static methods into extension methods was a real bonus, and definitely gives me renewed confidence that even if the new syntax looks a bit janky at first glance, it's clearly well thought out.

## [Summary](#summary)

In this post I described the new extension members feature coming in C#14 and .NET 10. I started by providing some background on extension methods in case you haven't seem then before, and then showed how you could convert these to use the new extension member syntax. I then showed how you could add the new extension member types: static extension methods, static and instance extension properties, and extension operators (coming in .NET 10 preview 7). Finally, I described how adding support for C#14 in the latest version of the [NetEscapades.EnumGenerators](https://github.com/andrewlock/NetEscapades.EnumGenerators) NuGet package makes for a better experience using the extensions.

This Series [Exploring the .NET 10 preview](/series/exploring-the-dotnet-10-preview/)

Follow me

*   [](https://www.facebook.com/NETescapades "Like .NET Escapades on Facebook")
*   [](https://twitter.com/andrewlocknet "Follow @andrewlocknet on Twitter")
*   [](https://hachyderm.io/@andrewlock "Follow @andrewlock@hachyderm.io on Mastadon")
*   [](https://uk.linkedin.com/in/andrewdlock "Andrew Lock on LinkedIn")
*   [](https://github.com/andrewlock "Andrew lock on Github")
*   [](/rss.xml "Subscribe to RSS")

Enjoy this blog?

*   [![Buy Me A Coffee](/assets/img/coffee.png)](https://www.buymeacoffee.com/andrewlock)
*   [![Donate with PayPal](/assets/img/paypal.png)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=M5VJREL5PTWNC&currency_code=GBP&source=url)

 [![Behind the scenes of dotnet run app.cs](/content/images/2025/single-file-dotnet-2.webp) Previous Behind the scenes of dotnet run app.cs: Exploring the .NET 10 preview - Part 2](/exploring-dotnet-10-preview-features-2-behind-the-scenes-of-dotnet-run-app.cs/) [![Solving the source generator 'marker attribute' problem in .NET 10](/content/images/2025/embeddedattribute_banner.png) Next Solving the source generator 'marker attribute' problem in .NET 10: Exploring the .NET 10 preview - Part 4](/exploring-dotnet-10-preview-features-4-solving-the-source-generator-marker-attribute-problem-in-dotnet-10/)

 [![ads via Carbon](https://srv.carbonads.net/static/30242/5553640155979510763aebb62751652e628b00e1)](https://srv.carbonads.net/ads/click/x/GTND427MCKSDLKJYC6A4YKQUCWBD62QICAYIVZ3JCABIVKQNCW7D6KJKC6BDP23WCY7DP2JUCV7IP5QLCWYIC53NHEYI52QICAYIV53ECTNCYBZ52K)[Design and Development tips in your inbox. Every weekday.](https://srv.carbonads.net/ads/click/x/GTND427MCKSDLKJYC6A4YKQUCWBD62QICAYIVZ3JCABIVKQNCW7D6KJKC6BDP23WCY7DP2JUCV7IP5QLCWYIC53NHEYI52QICAYIV53ECTNCYBZ52K) [ads via Carbon](http://carbonads.net/?utm_source=andrewlocknet&utm_medium=ad_via_link&utm_campaign=in_unit&utm_term=carbon)

Loading...

[![30% off with code ANDREW30 on Dometrain Pro](/content/images/a/nickchapsas2025.jpg)](https://dometrain.com/dometrain-pro/?ref=andrew-lock&promo=banner&coupon_code=ANDREW30)

[

![ASP.NET Core in Action, Third Edition](/content/images/aspnetcoreinaction3e.png)

My new book _ASP.NET Core in Action, Third Edition_ is available now! It supports .NET 7.0, and is available as an eBook or paperback.

](http://mng.bz/5mRz)

Enjoy this blog?

*   [![Buy Me A Coffee](/assets/img/coffee.png)](https://www.buymeacoffee.com/andrewlock)
*   [![Donate with PayPal](/assets/img/paypal.png)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=M5VJREL5PTWNC&currency_code=GBP&source=url)

Table of Contents

*   [Background: Extension methods](#background-extension-methods)
*   [Extension members and extension everything](#extension-members-and-extension-everything)
    *   [Converting an extension method to an extension member](#converting-an-extension-method-to-an-extension-member)
    *   [Adding other extension members](#adding-other-extension-members)
    *   [Disambiguating extension members](#disambiguating-extension-members)
*   [A case study: NetEscapades.EnumGenerators](#a-case-study-netescapades-enumgenerators)
*   [Summary](#summary)

This series [Exploring the .NET 10 preview](/series/exploring-the-dotnet-10-preview/)