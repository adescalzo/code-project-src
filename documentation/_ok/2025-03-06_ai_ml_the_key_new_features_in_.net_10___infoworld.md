```yaml
---
title: "The key new features in .NET 10 | InfoWorld"
source: https://www.infoworld.com/article/3839444/the-key-new-features-in-net-10.html?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2024
date_published: 2025-03-06T03:00:00.000Z
date_captured: 2025-08-19T11:25:44.184Z
domain: www.infoworld.com
author: By Martin Heller
category: ai_ml
technologies: [.NET 10, ASP.NET Core, .NET Aspire, SignalR, Blazor, OpenAPI, JSON Schema, WebAssembly, Intel AVX 10.2, Arm64, x64, JSON, YAML]
programming_languages: [C#]
tags: [dotnet, .net-10, asp.net-core, runtime, performance, web-development, api, developer-productivity, preview, long-term-support]
key_concepts: [.NET runtime, Just-In-Time (JIT) compilation, array optimization, hardware acceleration, API development, microservices, OpenAPI documentation, developer productivity]
code_examples: false
difficulty_level: intermediate
summary: |
  This article provides an overview of the first preview release of .NET 10, highlighting its key new features and improvements. The focus areas include significant enhancements to the underlying .NET runtime for better performance, particularly with JIT compilation and array handling, and support for next-generation processor architectures like Arm64 and upcoming x64 instructions (AVX 10.2). ASP.NET Core also receives updates, expanding its capabilities for web applications, APIs, and microservices, with notable improvements to Blazor and OpenAPI documentation. The release also introduces smaller, impactful features aimed at improving developer productivity, such as a new numeric string comparer. As a long-term support (LTS) release, .NET 10 aims for stability and compatibility with modern cloud-native and desktop applications.
---
```

# The key new features in .NET 10 | InfoWorld

# The key new features in .NET 10

## Microsoft has rolled out the first preview of .NET 10, with improvements to the runtime, ASP.NET Core, APIs, and developer productivity.

![A female developer working on a computer with multiple screens displaying code, symbolizing modern software development.](https://www.infoworld.com/wp-content/uploads/2025/03/3839444-0-98017200-1741251708-shutterstock_2324952341.jpg?quality=50&strip=all&w=1024)

The first preview of the next long-term support release of .NET is now available. Like previous even-numbered releases, .NET 10 builds on features introduced in the current short-term release, with a focus on performance and reliability improvements. As .NET 10 gets three years of support, it will be a logical update for any .NET 8 code you have in production.

An early release of a preview gives you time to see how existing code will translate to new runtimes and lets you gain experience with new language and SDK features. Two years between long-term support releases is more than enough time for new techniques and tools to mature and to get them ready for the applications developers are building today.

## Improving the .NET runtime

The underlying .NET runtime is one focus area for the .NET team in this release cycle. This is key to application performance, as it’s how code is interpreted and compiled. A more efficient just-in-time (JIT) engine in the runtime should make code faster, especially as .NET now supports multiple processor architectures, including Arm64. At the same time, the runtime needs support for new instructions in x64, which continues to evolve as both Intel and AMD add security features along with new integer and floating-point types that are needed for new use cases.

One key requirement here is reducing the amount of abstraction between code and runtime, where commonly used language features don’t map directly to runtime capabilities. There’s an ongoing project within the .NET runtime development process to deliver this. It’s a big task, and the team is focusing on a specific subset of operations in .NET 10. Much of the work focuses on how .NET works with arrays.

We need significant improvements here. If you write code to sum an array, using direct access to a static array versus using an interface to manage the same operation, the direct access approach is more than four times faster than using the interface, even though using interfaces is the better practice. This is because adding the abstraction of an interface to an array adds several operations to generate the right JIT code, as the compiler is unable to devirtualize the interface.

Earlier releases of .NET have improved performance and added the necessary components to reduce the overhead that comes with abstraction. This allows .NET 10 to finally devirtualize array interfaces, enabling the JIT to optimize the code. There’s still work to be done, but the improvements are important.

## Supporting next-generation silicon

Other compiler-level features target upcoming new x64 instructions, specifically AVX 10.2. These add important new processor features across a wide selection of different tasks: from AI to WebAssembly and cryptography. More and more of today’s software depends on vector processing, and support for these new features will allow .NET code to work more effectively. However, silicon that supports these new functions is still under development, so while there’s support ready for when processors ship, it’s currently disabled.

With a three-year support window, getting features like this baked into .NET early makes a lot of sense. Microsoft can switch them on when the hardware is ready without having to make significant changes to the .NET runtime—and Microsoft can assess performance on sample hardware running in its own labs before shipping it to the wider world.

## Rewiring ASP.NET Core

The .NET platform is about a lot more than programming languages. It’s a platform that’s used on-premises, in the cloud, and across multiple operating systems. Much of its platform capabilities come from tools such as .NET Aspire and ASP.NET Core. Aspire’s .NET 10 feature set is still under development, and although Aspire 9.1 arrived at the same time as the first .NET 10 preview, it’s still targeted at .NET 8 and .NET 9.

ASP.NET Core offers more than web application development, as it’s become the best way to build and deliver APIs and microservices in .NET. Technologies such as SignalR support real-time data and offer an opinionated way to deliver minimal APIs for microservices. It’s also where Microsoft is doing a lot of its .NET-based WebAssembly work as part of its full-stack Blazor framework.

Blazor is getting a major update in .NET 10, ready for use with modern content delivery networks and reducing the load on your web applications. Static asset delivery has been part of ASP.NET Core for a while, and now Blazor scripts are treated as static assets. Updates to SignalR and minimal APIs are due in later preview releases so, for now, they’re not currently documented.

## Automatically documenting APIs

With the increased importance of OpenAPI to client application development, there’s a version bump to OpenAPI 3.1.1 for ASP.NET Core-powered APIs. This changes how ASP.NET Core generates OpenAPI documents for you, with support for an updated version of the JSON Schema specification. This isn’t mandatory; you can choose an earlier version as there are breaking changes in the underlying library.

If you’ve been using your own transformers to work with OpenAPI content—for example, when generating documentation from an API definition—you need to switch from the OpenAPI classes to `JsonNode`. This simplifies things. Where you had to use `OpenApiInteger` or `OpenApiString`, you can now use simple integers and strings.

You’re now able to deliver OpenAPI documents as YAML rather than JSON. The feature isn’t yet complete. (It only supports OpenAPI endpoints for dynamic content at this point of the development cycle.) A future preview should allow you to create YAML documents at build time. YAML documents are easier for humans to read and are smaller than their JSON alternatives, so you may prefer to use them where possible.

## Improving developer productivity

When it comes to libraries and other components of the .NET platform, it’s still early in the development cycle and we can expect new features with each preview release. Looking through the documentation, a couple of features caught my eye.

One that I’ve been wanting for a long time now is a way to sort strings based not only on alphabetical content but also on numerals. With the new `numericStringComparer` operator, your code will differentiate between “Airbus A320,” “Airbus A319,” and “Airbus A321” and put them in the right order. This used to be a complex task requiring multiple string comparisons and conversions, but now it’s all in one line of code.

Small upgrades like this, along with .NET’s improved zip archive support, are where a lot of the benefit lies. Making developers’ lives a little bit easier ends up giving us all better code with fewer bugs, as there’s no longer any need to code around something that should have been straightforward to start with.

The final release at .NET Conf is still more than half a year away, but getting an early look at what .NET 10 brings to the table is always good. Developing the platform in the open like this has made it easier for development teams to experiment before suggesting and even making changes.

This first preview is only part of what Microsoft plans to deliver, with at least another two preview releases to come. Unsurprisingly, much of what’s promised is a more stable version of .NET 9, but the new features are important, ensuring that .NET is compatible with developers’ needs and with the requirements of modern cloud-native applications as well as the desktop. It’s a difficult balancing act, but .NET 10 seems ready to take it on.