```yaml
---
title: Facade
source: https://refactoring.guru/design-patterns/facade
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:20:19.786Z
domain: refactoring.guru
author: Unknown
category: general
technologies: []
programming_languages: [C#, C++, Go, Java, JavaScript, PHP, Python, Ruby, Rust, Swift, TypeScript]
tags: [design-patterns, structural-patterns, facade, software-architecture, object-oriented-programming, api-design, code-organization, complexity-management, abstraction]
key_concepts: [facade-pattern, structural-design-patterns, interface-simplification, subsystem-management, loose-coupling, abstraction, layered-architecture, god-object-anti-pattern]
code_examples: false
difficulty_level: intermediate
summary: |
  The Facade design pattern is a structural pattern that simplifies the interface to a complex subsystem, library, or framework. It addresses the problem of tight coupling between client code and intricate third-party implementations by providing a single, simplified entry point. This pattern is useful when only a limited set of a complex system's features are needed, reducing boilerplate code and making maintenance easier. While it offers isolation and simpler interaction, a facade can potentially become a "god object" if not managed properly. The article illustrates its structure, provides pseudocode for a video conversion example, and discusses its applicability and relationship with other design patterns.
---
```

# Facade

[](/)/ [Design Patterns](/design-patterns) / [Structural Patterns](/design-patterns/structural-patterns)

# Facade

## Intent

**Facade** is a structural design pattern that provides a simplified interface to a library, a framework, or any other complex set of classes.

![Facade design pattern: A classical building (facade) stands in front of a complex industrial facility, surrounded by barbed wire, with small figures approaching the building. This visually represents the Facade pattern providing a simple interface to a complex, potentially dangerous, subsystem.](/images/patterns/content/facade/facade.png?id=1f4be17305b6316fbd548edf1937ac3b)

## Problem

Imagine that you must make your code work with a broad set of objects that belong to a sophisticated library or framework. Ordinarily, you’d need to initialize all of those objects, keep track of dependencies, execute methods in the correct order, and so on.

As a result, the business logic of your classes would become tightly coupled to the implementation details of 3rd-party classes, making it hard to comprehend and maintain.

## Solution

A facade is a class that provides a simple interface to a complex subsystem which contains lots of moving parts. A facade might provide limited functionality in comparison to working with the subsystem directly. However, it includes only those features that clients really care about.

Having a facade is handy when you need to integrate your app with a sophisticated library that has dozens of features, but you just need a tiny bit of its functionality.

For instance, an app that uploads short funny videos with cats to social media could potentially use a professional video conversion library. However, all that it really needs is a class with the single method `encode(filename, format)`. After creating such a class and connecting it with the video conversion library, you’ll have your first facade.

## Real-World Analogy

![An example of taking a phone order: A person on the phone (client) interacts with a group of call center operators (facade), who then interact with a complex network of internal departments like Warehouse, Payment Processing, Packaging, Suppliers, Delivery, and Taxes (subsystem). This illustrates the real-world analogy of a phone operator as a facade.](/images/patterns/diagrams/facade/live-example-en.png?id=461900f9fbacdd0ce981dcd24e121078)

Placing orders by phone.

When you call a shop to place a phone order, an operator is your facade to all services and departments of the shop. The operator provides you with a simple voice interface to the ordering system, payment gateways, and various delivery services.

## Structure

![Structure of the Facade design pattern: A UML-like diagram showing a "Client" interacting with a "Facade" class, which in turn interacts with multiple "Subsystem class" objects. An "Additional Facade" is also shown. This depicts the basic structure of the Facade pattern.](/images/patterns/diagrams/facade/structure.png?id=258401362234ac77a2aaf1cde62339e7)![Structure of the Facade design pattern with indexed elements: Same as the previous structure diagram, but with numbered labels (1-4) pointing to Facade, Additional Facade, Complex Subsystem, and Client, corresponding to the numbered explanations in the "Structure" section.](/images/patterns/diagrams/facade/structure-indexed.png?id=2da06d6b850701ea15cf72f9d2642fb8)

1.  The **Facade** provides convenient access to a particular part of the subsystem’s functionality. It knows where to direct the client’s request and how to operate all the moving parts.

2.  An **Additional Facade** class can be created to prevent polluting a single facade with unrelated features that might make it yet another complex structure. Additional facades can be used by both clients and other facades.

3.  The **Complex Subsystem** consists of dozens of various objects. To make them all do something meaningful, you have to dive deep into the subsystem’s implementation details, such as initializing objects in the correct order and supplying them with data in the proper format.

    Subsystem classes aren’t aware of the facade’s existence. They operate within the system and work with each other directly.

4.  The **Client** uses the facade instead of calling the subsystem objects directly.

## Pseudocode

In this example, the **Facade** pattern simplifies interaction with a complex video conversion framework.

![The structure of the Facade pattern example: A UML-like diagram showing an "Application" interacting with a "VideoConverter" class (the Facade), which then interacts with several video processing classes like VideoFile, OggCompressionCodec, MPEG4CompressionCodec, CodecFactory, BitrateReader, and AudioMixer (the subsystem). This illustrates the specific video conversion example used in the pseudocode.](/images/patterns/diagrams/facade/example.png?id=2249d134e3ff83819dfc19032f02eced)

An example of isolating multiple dependencies within a single facade class.

Instead of making your code work with dozens of the framework classes directly, you create a facade class which encapsulates that functionality and hides it from the rest of the code. This structure also helps you to minimize the effort of upgrading to future versions of the framework or replacing it with another one. The only thing you’d need to change in your app would be the implementation of the facade’s methods.

```
// These are some of the classes of a complex 3rd-party video
// conversion framework. We don't control that code, therefore
// can't simplify it.

class VideoFile
// ...

class OggCompressionCodec
// ...

class MPEG4CompressionCodec
// ...

class CodecFactory
// ...

class BitrateReader
// ...

class AudioMixer
// ...


// We create a facade class to hide the framework's complexity
// behind a simple interface. It's a trade-off between
// functionality and simplicity.
class VideoConverter is
    method convert(filename, format):File is
        file = new VideoFile(filename)
        sourceCodec = (new CodecFactory).extract(file)
        if (format == "mp4")
            destinationCodec = new MPEG4CompressionCodec()
        else
            destinationCodec = new OggCompressionCodec()
        buffer = BitrateReader.read(filename, sourceCodec)
        result = BitrateReader.convert(buffer, destinationCodec)
        result = (new AudioMixer()).fix(result)
        return new File(result)

// Application classes don't depend on a billion classes
// provided by the complex framework. Also, if you decide to
// switch frameworks, you only need to rewrite the facade class.
class Application is
    method main() is
        convertor = new VideoConverter()
        mp4 = convertor.convert("funny-cats-video.ogg", "mp4")
        mp4.save()
```

## Applicability

Use the Facade pattern when you need to have a limited but straightforward interface to a complex subsystem.

Often, subsystems get more complex over time. Even applying design patterns typically leads to creating more classes. A subsystem may become more flexible and easier to reuse in various contexts, but the amount of configuration and boilerplate code it demands from a client grows ever larger. The Facade attempts to fix this problem by providing a shortcut to the most-used features of the subsystem which fit most client requirements.

Use the Facade when you want to structure a subsystem into layers.

Create facades to define entry points to each level of a subsystem. You can reduce coupling between multiple subsystems by requiring them to communicate only through facades.

For example, let’s return to our video conversion framework. It can be broken down into two layers: video- and audio-related. For each layer, you can create a facade and then make the classes of each layer communicate with each other via those facades. This approach looks very similar to the [Mediator](/design-patterns/mediator) pattern.

## How to Implement

1.  Check whether it’s possible to provide a simpler interface than what an existing subsystem already provides. You’re on the right track if this interface makes the client code independent from many of the subsystem’s classes.

2.  Declare and implement this interface in a new facade class. The facade should redirect the calls from the client code to appropriate objects of the subsystem. The facade should be responsible for initializing the subsystem and managing its further life cycle unless the client code already does this.

3.  To get the full benefit from the pattern, make all the client code communicate with the subsystem only via the facade. Now the client code is protected from any changes in the subsystem code. For example, when a subsystem gets upgraded to a new version, you will only need to modify the code in the facade.

4.  If the facade becomes [too big](/smells/large-class), consider extracting part of its behavior to a new, refined facade class.

## Pros and Cons

*   You can isolate your code from the complexity of a subsystem.

*   A facade can become [a god object](/antipatterns/god-object) coupled to all classes of an app.

## Relations with Other Patterns

*   [Facade](/design-patterns/facade) defines a new interface for existing objects, whereas [Adapter](/design-patterns/adapter) tries to make the existing interface usable. _Adapter_ usually wraps just one object, while _Facade_ works with an entire subsystem of objects.

*   [Abstract Factory](/design-patterns/abstract-factory) can serve as an alternative to [Facade](/design-patterns/facade) when you only want to hide the way the subsystem objects are created from the client code.

*   [Flyweight](/design-patterns/flyweight) shows how to make lots of little objects, whereas [Facade](/design-patterns/facade) shows how to make a single object that represents an entire subsystem.

*   [Facade](/design-patterns/facade) and [Mediator](/design-patterns/mediator) have similar jobs: they try to organize collaboration between lots of tightly coupled classes.

    *   _Facade_ defines a simplified interface to a subsystem of objects, but it doesn’t introduce any new functionality. The subsystem itself is unaware of the facade. Objects within the subsystem can communicate directly.
    *   _Mediator_ centralizes communication between components of the system. The components only know about the mediator object and don’t communicate directly.
*   A [Facade](/design-patterns/facade) class can often be transformed into a [Singleton](/design-patterns/singleton) since a single facade object is sufficient in most cases.

*   [Facade](/design-patterns/facade) is similar to [Proxy](/design-patterns/proxy) in that both buffer a complex entity and initialize it on its own. Unlike _Facade_, _Proxy_ has the same interface as its service object, which makes them interchangeable.

## Code Examples

[![Facade in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/facade/csharp/example "Facade in C#") [![Facade in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/facade/cpp/example "Facade in C++") [![Facade in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/facade/go/example "Facade in Go") [![Facade in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/facade/java/example "Facade in Java") [![Facade in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/facade/php/example "Facade in PHP") [![Facade in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/facade/python/example "Facade in Python") [![Facade in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/facade/ruby/example "Facade in Ruby") [![Facade in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/facade/rust/example "Facade in Rust") [![Facade in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/facade/swift/example "Facade in Swift") [![Facade in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/facade/typescript/example "Facade in TypeScript")

[![](/images/patterns/banners/patterns-book-banner-3.png?id=7d445df13c80287beaab234b4f3b698c)](/design-patterns/book)

### Support our free website and own the eBook!

*   22 design patterns and 8 principles explained in depth.
*   409 well-structured, easy to read, jargon-free pages.
*   225 clear and helpful illustrations and diagrams.
*   An archive with code examples in 11 languages.
*   All devices supported: PDF/EPUB/MOBI/KFX formats.

[Learn more…](/design-patterns/book)

#### Read next

[Flyweight](/design-patterns/flyweight)

#### Return

[Decorator](/design-patterns/decorator)

[![](/images/patterns/book/web-cover-en.png?id=328861769fd11617674e3b8a7e2dd9e7)](/design-patterns/book)

This article is a part of our eBook
**Dive Into Design Patterns**.

[Learn more…](/design-patterns/book)