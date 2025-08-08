# Underrated .NET packages that will save you 100s of lines of code | by Gulam Ali H. | Jul, 2025 | Stackademic

**Source:** https://blog.stackademic.com/underrated-net-packages-that-will-save-you-100s-of-lines-of-code-3ee34d0d7f0e
**Date Captured:** 2025-07-28T16:13:10.026Z
**Domain:** blog.stackademic.com
**Author:** Gulam Ali H.
**Category:** general

---

Member-only story

# Underrated .NET packages that will save you 100s of lines of code

## A curated list of C# packages that you should be using in your projects for increased productivity.

[

![Gulam Ali H.](https://miro.medium.com/v2/resize:fill:64:64/1*gVdmjBANaWiYYNgEzUoiRQ.jpeg)





](https://medium.com/@freakyali?source=post_page---byline--3ee34d0d7f0e---------------------------------------)

[Gulam Ali H.](https://medium.com/@freakyali?source=post_page---byline--3ee34d0d7f0e---------------------------------------)

Follow

5 min read

·

Jul 20, 2025

212

4

Listen

Share

More

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:1000/1*PCYWpnWwSt4w28RxTQj9Tg.jpeg)

image is from : [https://dotnet.microsoft.com/en-us/learn/dotnet/what-is-dotnet-framework](https://dotnet.microsoft.com/en-us/learn/dotnet/what-is-dotnet-framework)

# Introduction

Let’s be honest: most of us waste a lot of time writing the same boilerplate code over and over. Property change notifications, API clients, logging setup, validation, you get the idea.

If you’re not a member, [**I’ve got you covered**](https://medium.com/@freakyali/underrated-net-packages-that-will-save-you-100s-of-lines-of-code-3ee34d0d7f0e?sk=ec15fc1150eee057b54835ee6115fdc2)**!** ❤

If you enjoy it, consider [**subscribing**](https://medium.com/@freakyali/subscribe) or [**buying me a coffee**](https://buymeacoffee.com/freakyali) to show your support! ❤

The good news? The .NET ecosystem is packed with libraries that handle this stuff for you, saving you **hundreds of lines of code** without sacrificing control over your project.

In this post, I’m listing the **actual packages I use** to cut through repetitive tasks and speed up development. If you’re working in .NET, whether it’s APIs, desktop, or mobile, these tools will make your life a lot easier.

Let’s get into it.

## 1\. morelinq

Let’s start with my favourite C# feature, LINQ. But this package makes LINQ even better; it gives you a bunch of methods that LINQ is missing. And trust me it's worth more than you might think.

[

## morelinq 4.4.0

### This project enhances LINQ to Objects with the following methods: Acquire, Aggregate (some EXPERIMENTAL)…

www.nuget.org



](https://www.nuget.org/packages/morelinq/?source=post_page-----3ee34d0d7f0e---------------------------------------)

## 2\. System.Linq.Async

If you have ever used **_IAsyncEnumerables_** this package is a blessing; it will let you use LINQ with it.

**Note:** AsyncEnumerable was introduced in .NET 10, and hence this package is not needed

[

## System.Linq.Async 6.0.3

### Provides support for Language-Integrated Query (LINQ) over IAsyncEnumerable sequences.

www.nuget.org



](https://www.nuget.org/packages/system.linq.async/?source=post_page-----3ee34d0d7f0e---------------------------------------)

## 3\. FastEnum:

Recently, I was working on a project where we had to work with a lot of enums, and as you already know, working with enums in C# can be annoying and very inefficient. That’s where FastEnum comes in, FastEnum is an enum utility for .NET and is much faster than .NET provided methods and all this achieved with **zero** allocations!!

[

## FastEnum 2.0.5

### The extremely fast enum utilities for C#/.NET. Provided methods are all achieved zero allocation and faster than…

www.nuget.org



](https://www.nuget.org/packages/FastEnum/?source=post_page-----3ee34d0d7f0e---------------------------------------)

## 4\. AsyncAwaitBestPractices

This is my favourite package hands down, It provides a set of extension methods for **_System.Threading.Tasks.Task_** that let’s you gracefully handle Tasks that cannot be awaited and much more definitely check it out.

[

## AsyncAwaitBestPractices 9.0.0

### Extensions for System.Threading.Tasks Includes extension methods to safely fire-and-forget a Task and/or a ValueTask…

www.nuget.org



](https://www.nuget.org/packages/AsyncAwaitBestPractices?source=post_page-----3ee34d0d7f0e---------------------------------------)

## 5\. FastEndpoints

This is something I have been using a lot lately, especially for MVPs, **FastEndpoints,** it is an alternative for Minimal Apis and MVC and It nudges you towards the **REPR** design pattern for convenient & maintainable endpoint creation with virtually no boilerplate.

[

## FastEndpoints 6.2.0

### A light-weight REST Api framework for ASP.Net 8 and newer that implements REPR (Request-Endpoint-Response) Pattern.

www.nuget.org



](https://www.nuget.org/packages/FastEndpoints/?source=post_page-----3ee34d0d7f0e---------------------------------------)

## 6\. Humanizer

This one is an absolute banger and I am suprised I did not know of it all these years, but I am sure there was a time when you wanted to convert maybe a string, or a date or time or a timestamp into a human readable version of it, that is where humanizer comes in, Humanizer helps you convert a bunch of data types into human readable format and trust me a two year old can use it, It’s insane how well thought this is.

[

## Humanizer 2.14.1

### Humanizer meets all your .NET needs for manipulating and displaying strings, enums, dates, times, timespans, numbers…

www.nuget.org



](https://www.nuget.org/packages/Humanizer/?source=post_page-----3ee34d0d7f0e---------------------------------------)

## 7\. Bogus

Remember the millions of times when you needed dummy data since the Web-APIs or DB isn’t ready well this library will help you get started, Bogus let’s you generate fake data.

[

## Bogus 35.6.3

### A simple and sane data generator for populating objects that supports different locales. A delightful port of the famed…

www.nuget.org



](https://www.nuget.org/packages/Bogus/?source=post_page-----3ee34d0d7f0e---------------------------------------)

## 8 Obfuscar

Obfuscar is an obfuscation library for .NET that let’s you apply basic levels of obfuscation and help secure secrets in a .NET assembly.

[

## Obfuscar.GlobalTool 2.2.49

### Obfuscar is a basic obfuscator for .NET assemblies. It uses massive overloading to rename metadata in .NET assemblies…

www.nuget.org



](https://www.nuget.org/packages/Obfuscar.GlobalTool/?source=post_page-----3ee34d0d7f0e---------------------------------------)

## 9\. MailKit

MailKit is a cross-platform mail client library which supports IMAP, POP3, and SMTP.

[

## MailKit 4.13.0

### MailKit is an Open Source cross-platform .NET mail-client library that is based on MimeKit and optimized for mobile…

www.nuget.org



](https://www.nuget.org/packages/MailKit/?source=post_page-----3ee34d0d7f0e---------------------------------------)

## 10\. Wolverine

If you are looking for a Mediator and Message Bus that has great features and consistent quality of life improvements, Wolverine is something you should definitely consider

[

## GitHub - JasperFx/wolverine: Supercharged .NET server side development!

### Supercharged .NET server side development! Contribute to JasperFx/wolverine development by creating an account on…

github.com



](https://github.com/JasperFx/wolverine?source=post_page-----3ee34d0d7f0e---------------------------------------)

## 11\. Jitbit.FastCache

FastCache is a high-performance, lighweight and thread-safe memory cache for .NET, Definitely a must have when it comes to dealing with in memory caching.

[

## Jitbit.FastCache 1.1.0

### Fastest in-memoery cache for .NET

www.nuget.org



](https://www.nuget.org/packages/Jitbit.FastCache?source=post_page-----3ee34d0d7f0e---------------------------------------)

## 12\. SharpCompress

SharpCompress is a compression library in pure C# for .NET Framework 4.62, .NET Standard 2.1, .NET 6.0 and NET 8.0 that can unrar, un7zip, unzip, untar unbzip2, ungzip, unlzip with forward-only reading and file random access APIs. Write support for zip/tar/bzip2/gzip/lzip are implemented.

[

## SharpCompress 0.40.0

### SharpCompress is a compression library for NET Standard 2.0/NET 4.8/NET 4.8.1/NET 6.0/NET 8.0 that can unrar…

www.nuget.org



](https://www.nuget.org/packages/SharpCompress?source=post_page-----3ee34d0d7f0e---------------------------------------)

# Honorable Mentions

These are some packages that I would have certainly added to the list, but they are not really underrated, So I am just gonna mention them here for those who might not know of them.

## 1\. MassTransit

MassTransit is a _free, open-source_ distributed application framework for .NET. MassTransit makes it easy to create applications and services that leverage message-based, loosely-coupled asynchronous communication for higher availability, reliability, and scalability.

[

## MassTransit 8.5.1

### MassTransit provides a developer-focused, modern platform for creating distributed applications without complexity.

www.nuget.org







](https://www.nuget.org/packages/MassTransit?source=post_page-----3ee34d0d7f0e---------------------------------------)

## 2\. FluentValidation

A validation library for .NET that uses a fluent interface and lambda expressions for building strongly-typed validation rules.

[

## FluentValidation 12.0.0

### A validation library for .NET that uses a fluent interface to construct strongly-typed validation rules.

www.nuget.org



](https://www.nuget.org/packages/FluentValidation?source=post_page-----3ee34d0d7f0e---------------------------------------)

## 3\. Polly

Well this one isn’t something you’ve never heard of but if you don’t know, Polly is a .NET resilience and transient-fault-handling library that allows developers to express resilience strategies such as Retry, Circuit Breaker, Hedging, Timeout, Rate Limiter and Fallback in a fluent and thread-safe manner.

[

## Polly 8.6.2

### Polly is a .NET resilience and transient-fault-handling library that allows developers to express resilience and…

www.nuget.org



](https://www.nuget.org/packages/Polly?source=post_page-----3ee34d0d7f0e---------------------------------------)

## 4\. Refit

Refit is an automatic type-safe REST library for .NET Core, Xamarin and .NET. Heavily inspired by Square’s Retrofit library, Refit turns your REST API into a live interface.

[

## Refit 8.0.0

### The automatic type-safe REST library for Xamarin and .NET

www.nuget.org



](https://www.nuget.org/packages/Refit?source=post_page-----3ee34d0d7f0e---------------------------------------)

# Conclusion

This was a curated list of .NET packages that I like using and recommend to my fellow developers. Hope you liked this one. If you want a Part 2 to this one, let me know in the comments :)

If I’ve missed a package that you think should have been there, go ahead and add it in the comments. Also, if you find something incorrect in the blog, please go ahead and correct me in the comments.

Smash that clap button if you liked this post.

Follow me on [GitHub](https://github.com/FreakyAli), or reach out to me on [LinkedIn](https://www.linkedin.com/in/gulam-ali-h-7b1940135/) or [StackOverflow](https://stackoverflow.com/users/7462031/g-hakim)!

# Thank you for being a part of the community

_Before you go:_

*   Be sure to **clap** and **follow** the writer ️👏️**️**
*   Follow us: [**X**](https://x.com/inPlainEngHQ) | [**LinkedIn**](https://www.linkedin.com/company/inplainenglish/) | [**YouTube**](https://www.youtube.com/@InPlainEnglish) | [**Newsletter**](https://newsletter.plainenglish.io/) | [**Podcast**](https://open.spotify.com/show/7qxylRWKhvZwMz2WuEoua0) | [**Twitch**](https://twitch.tv/inplainenglish)
*   [**Start your own free AI-powered blog on Differ**](https://differ.blog/) 🚀
*   [**Join our content creators community on Discord**](https://discord.gg/in-plain-english-709094664682340443) 🧑🏻‍💻
*   For more content, visit [**plainenglish.io**](https://plainenglish.io/) + [**stackademic.com**](https://stackademic.com/)