```yaml
---
title: Adapter
source: https://refactoring.guru/design-patterns/adapter
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:21:14.632Z
domain: refactoring.guru
author: Unknown
category: general
technologies: [XML, JSON]
programming_languages: [C#, C++, Go, Java, PHP, Python, Ruby, Rust, Swift, TypeScript]
tags: [design-patterns, structural-patterns, adapter, interface-conversion, data-conversion, object-oriented, software-architecture, legacy-systems, third-party-integration, code-reuse]
key_concepts: [Adapter pattern, object adapter, class adapter, interface incompatibility, object composition, inheritance, single responsibility principle, open/closed principle]
code_examples: false
difficulty_level: intermediate
summary: |
  [The Adapter design pattern is a structural pattern that enables objects with incompatible interfaces to collaborate. It acts as a translator, wrapping one object to convert its interface or data format so that another object can understand it. The pattern can be implemented as an object adapter, which uses composition, or a class adapter, which uses inheritance and requires multiple inheritance support. The article explains the problem, solution, real-world analogies, detailed structure, pseudocode example, applicability, implementation steps, and discusses its pros, cons, and relations to other design patterns.]
---
```

# Adapter

[](/)/ [Design Patterns](/design-patterns) / [Structural Patterns](/design-patterns/structural-patterns)

# Adapter

Also known as: Wrapper

## Intent

**Adapter** is a structural design pattern that allows objects with incompatible interfaces to collaborate.

![Diagram illustrating the Adapter design pattern, showing a client interacting with an adapter, which then interacts with a service.](/images/patterns/content/adapter/adapter-en.png)

## Problem

Imagine that you’re creating a stock market monitoring app. The app downloads the stock data from multiple sources in XML format and then displays nice-looking charts and diagrams for the user.

At some point, you decide to improve the app by integrating a smart 3rd-party analytics library. But there’s a catch: the analytics library only works with data in JSON format.

![Diagram showing an application receiving XML data from a Stock Data Provider, but unable to connect to an Analytics Library that expects JSON data.](/images/patterns/diagrams/adapter/problem-en.png)

You can’t use the analytics library “as is” because it expects the data in a format that’s incompatible with your app.

You could change the library to work with XML. However, this might break some existing code that relies on the library. And worse, you might not have access to the library’s source code in the first place, making this approach impossible.

## Solution

You can create an _adapter_. This is a special object that converts the interface of one object so that another object can understand it.

An adapter wraps one of the objects to hide the complexity of conversion happening behind the scenes. The wrapped object isn’t even aware of the adapter. For example, you can wrap an object that operates in meters and kilometers with an adapter that converts all of the data to imperial units such as feet and miles.

Adapters can not only convert data into various formats but can also help objects with different interfaces collaborate. Here’s how it works:

1.  The adapter gets an interface, compatible with one of the existing objects.
2.  Using this interface, the existing object can safely call the adapter’s methods.
3.  Upon receiving a call, the adapter passes the request to the second object, but in a format and order that the second object expects.

Sometimes it’s even possible to create a two-way adapter that can convert the calls in both directions.

![Diagram illustrating the Adapter's solution: an XML to JSON Adapter is placed between the application and the analytics library, converting XML data to JSON.](/images/patterns/diagrams/adapter/solution-en.png)

Let’s get back to our stock market app. To solve the dilemma of incompatible formats, you can create XML-to-JSON adapters for every class of the analytics library that your code works with directly. Then you adjust your code to communicate with the library only via these adapters. When an adapter receives a call, it translates the incoming XML data into a JSON structure and passes the call to the appropriate methods of a wrapped analytics object.

## Real-World Analogy

![A comic strip showing a car mounted on a platform with train wheels, labeled "CAR-TO-RAIL ADAPTER", illustrating the concept of adapting one system to another.](/images/patterns/content/adapter/adapter-comic-1-en.png)

A suitcase before and after a trip abroad.

When you travel from the US to Europe for the first time, you may get a surprise when trying to charge your laptop. The power plug and sockets standards are different in different countries. That’s why your US plug won’t fit a German socket. The problem can be solved by using a power plug adapter that has the American-style socket and the European-style plug.

## Structure

#### Object adapter

This implementation uses the object composition principle: the adapter implements the interface of one object and wraps the other one. It can be implemented in all popular programming languages.

![UML-like diagram showing the structure of an Object Adapter. A Client interacts with a Client Interface, which the Adapter implements. The Adapter also contains a reference to a Service object and delegates calls to it after converting data.](/images/patterns/diagrams/adapter/structure-object-adapter.png)![UML-like diagram showing the structure of an Object Adapter with indexed labels.](/images/patterns/diagrams/adapter/structure-object-adapter-indexed.png)

1.  The **Client** is a class that contains the existing business logic of the program.
    
2.  The **Client Interface** describes a protocol that other classes must follow to be able to collaborate with the client code.
    
3.  The **Service** is some useful class (usually 3rd-party or legacy). The client can’t use this class directly because it has an incompatible interface.
    
4.  The **Adapter** is a class that’s able to work with both the client and the service: it implements the client interface, while wrapping the service object. The adapter receives calls from the client via the client interface and translates them into calls to the wrapped service object in a format it can understand.
    
5.  The client code doesn’t get coupled to the concrete adapter class as long as it works with the adapter via the client interface. Thanks to this, you can introduce new types of adapters into the program without breaking the existing client code. This can be useful when the interface of the service class gets changed or replaced: you can just create a new adapter class without changing the client code.
    

#### Class adapter

This implementation uses inheritance: the adapter inherits interfaces from both objects at the same time. Note that this approach can only be implemented in programming languages that support multiple inheritance, such as C++.

![UML-like diagram showing the structure of a Class Adapter. The Class Adapter inherits from both the Client Interface and the Service, adapting methods internally.](/images/patterns/diagrams/adapter/structure-class-adapter.png)![UML-like diagram showing the structure of a Class Adapter with indexed labels.](/images/patterns/diagrams/adapter/structure-class-adapter-indexed.png)

1.  The **Class Adapter** doesn’t need to wrap any objects because it inherits behaviors from both the client and the service. The adaptation happens within the overridden methods. The resulting adapter can be used in place of an existing client class.
    

## Pseudocode

This example of the **Adapter** pattern is based on the classic conflict between square pegs and round holes.

![Diagram illustrating the "square pegs into round holes" analogy for the Adapter pattern, showing a square peg being adapted to fit into a round hole.](/images/patterns/diagrams/adapter/example.png)

Adapting square pegs to round holes.

The Adapter pretends to be a round peg, with a radius equal to a half of the square’s diameter (in other words, the radius of the smallest circle that can accommodate the square peg).

```
// Say you have two classes with compatible interfaces:
// RoundHole and RoundPeg.
class RoundHole is
    constructor RoundHole(radius) { ... }

    method getRadius() is
        // Return the radius of the hole.

    method fits(peg: RoundPeg) is
        return this.getRadius() >= peg.getRadius()

class RoundPeg is
    constructor RoundPeg(radius) { ... }

    method getRadius() is
        // Return the radius of the peg.


// But there's an incompatible class: SquarePeg.
class SquarePeg is
    constructor SquarePeg(width) { ... }

    method getWidth() is
        // Return the square peg width.


// An adapter class lets you fit square pegs into round holes.
// It extends the RoundPeg class to let the adapter objects act
// as round pegs.
class SquarePegAdapter extends RoundPeg is
    // In reality, the adapter contains an instance of the
    // SquarePeg class.
    private field peg: SquarePeg

    constructor SquarePegAdapter(peg: SquarePeg) is
        this.peg = peg

    method getRadius() is
        // The adapter pretends that it's a round peg with a
        // radius that could fit the square peg that the adapter
        // actually wraps.
        return peg.getWidth() * Math.sqrt(2) / 2


// Somewhere in client code.
hole = new RoundHole(5)
rpeg = new RoundPeg(5)
hole.fits(rpeg) // true

small_sqpeg = new SquarePeg(5)
large_sqpeg = new SquarePeg(10)
hole.fits(small_sqpeg) // this won't compile (incompatible types)

small_sqpeg_adapter = new SquarePegAdapter(small_sqpeg)
large_sqpeg_adapter = new SquarePegAdapter(large_sqpeg)
hole.fits(small_sqpeg_adapter) // true
hole.fits(large_sqpeg_adapter) // false
```

## Applicability

Use the Adapter class when you want to use some existing class, but its interface isn’t compatible with the rest of your code.

The Adapter pattern lets you create a middle-layer class that serves as a translator between your code and a legacy class, a 3rd-party class or any other class with a weird interface.

Use the pattern when you want to reuse several existing subclasses that lack some common functionality that can’t be added to the superclass.

You could extend each subclass and put the missing functionality into new child classes. However, you’ll need to duplicate the code across all of these new classes, which [smells really bad](/smells/duplicate-code).

The much more elegant solution would be to put the missing functionality into an adapter class. Then you would wrap objects with missing features inside the adapter, gaining needed features dynamically. For this to work, the target classes must have a common interface, and the adapter’s field should follow that interface. This approach looks very similar to the [Decorator](/design-patterns/decorator) pattern.

## How to Implement

1.  Make sure that you have at least two classes with incompatible interfaces:
    
    *   A useful _service_ class, which you can’t change (often 3rd-party, legacy or with lots of existing dependencies).
    *   One or several _client_ classes that would benefit from using the service class.
2.  Declare the client interface and describe how clients communicate with the service.
    
3.  Create the adapter class and make it follow the client interface. Leave all the methods empty for now.
    
4.  Add a field to the adapter class to store a reference to the service object. The common practice is to initialize this field via the constructor, but sometimes it’s more convenient to pass it to the adapter when calling its methods.
    
5.  One by one, implement all methods of the client interface in the adapter class. The adapter should delegate most of the real work to the service object, handling only the interface or data format conversion.
    
6.  Clients should use the adapter via the client interface. This will let you change or extend the adapters without affecting the client code.
    

## Pros and Cons

*   _Single Responsibility Principle_. You can separate the interface or data conversion code from the primary business logic of the program.
*   _Open/Closed Principle_. You can introduce new types of adapters into the program without breaking the existing client code, as long as they work with the adapters through the client interface.

*   The overall complexity of the code increases because you need to introduce a set of new interfaces and classes. Sometimes it’s simpler just to change the service class so that it matches the rest of your code.

## Relations with Other Patterns

*   [Bridge](/design-patterns/bridge) is usually designed up-front, letting you develop parts of an application independently of each other. On the other hand, [Adapter](/design-patterns/adapter) is commonly used with an existing app to make some otherwise-incompatible classes work together nicely.
    
*   [Adapter](/design-patterns/adapter) provides a completely different interface for accessing an existing object. On the other hand, with the [Decorator](/design-patterns/decorator) pattern the interface either stays the same or gets extended. In addition, _Decorator_ supports recursive composition, which isn’t possible when you use _Adapter_.
    
*   With [Adapter](/design-patterns/adapter) you access an existing object via different interface. With [Proxy](/design-patterns/proxy), the interface stays the same. With [Decorator](/design-patterns/decorator) you access the object via an enhanced interface.
    
*   [Facade](/design-patterns/facade) defines a new interface for existing objects, whereas [Adapter](/design-patterns/adapter) tries to make the existing interface usable. _Adapter_ usually wraps just one object, while _Facade_ works with an entire subsystem of objects.
    
*   [Bridge](/design-patterns/bridge), [State](/design-patterns/state), [Strategy](/design-patterns/strategy) (and to some degree [Adapter](/design-patterns/adapter)) have very similar structures. Indeed, all of these patterns are based on composition, which is delegating work to other objects. However, they all solve different problems. A pattern isn’t just a recipe for structuring your code in a specific way. It can also communicate to other developers the problem the pattern solves.
    

## Code Examples

[![Adapter in C#](/images/patterns/icons/csharp.svg)](/design-patterns/adapter/csharp/example "Adapter in C#") [![Adapter in C++](/images/patterns/icons/cpp.svg)](/design-patterns/adapter/cpp/example "Adapter in C++") [![Adapter in Go](/images/patterns/icons/go.svg)](/design-patterns/adapter/go/example "Adapter in Go") [![Adapter in Java](/images/patterns/icons/java.svg)](/design-patterns/adapter/java/example "Adapter in Java") [![Adapter in PHP](/images/patterns/icons/php.svg)](/design-patterns/adapter/php/example "Adapter in PHP") [![Adapter in Python](/images/patterns/icons/python.svg)](/design-patterns/adapter/python/example "Adapter in Python") [![Adapter in Ruby](/images/patterns/icons/ruby.svg)](/design-patterns/adapter/ruby/example "Adapter in Ruby") [![Adapter in Rust](/images/patterns/icons/rust.svg)](/design-patterns/adapter/rust/example "Adapter in Rust") [![Adapter in Swift](/images/patterns/icons/swift.svg)](/design-patterns/adapter/swift/example "Adapter in Swift") [![Adapter in TypeScript](/images/patterns/icons/typescript.svg)](/design-patterns/adapter/typescript/example "Adapter in TypeScript")

[![](/images/patterns/banners/patterns-book-banner-3.png)](/design-patterns/book)

### Support our free website and own the eBook!

*   22 design patterns and 8 principles explained in depth.
*   409 well-structured, easy to read, jargon-free pages.
*   225 clear and helpful illustrations and diagrams.
*   An archive with code examples in 11 languages.
*   All devices supported: PDF/EPUB/MOBI/KFX formats.

[Learn more…](/design-patterns/book)

#### Read next

[Bridge](/design-patterns/bridge)

#### Return

[Structural Patterns](/design-patterns/structural-patterns)

![Cover image for the "Dive Into Design Patterns" eBook.](/images/patterns/book/web-cover-en.png)

This article is a part of our eBook
**Dive Into Design Patterns**.

[Learn more…](/design-patterns/book)