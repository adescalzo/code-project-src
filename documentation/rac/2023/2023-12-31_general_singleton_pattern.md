```yaml
---
title: Singleton
source: https://refactoring.guru/design-patterns/singleton
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:22:01.787Z
domain: refactoring.guru
author: Unknown
category: general
technologies: [Database]
programming_languages: [C#, C++, Go, Java, JavaScript, PHP, Python, Pseudocode, Ruby, Rust, SQL, Swift, TypeScript]
tags: [design-patterns, creational-patterns, singleton, software-design, object-oriented-programming, concurrency, global-access, class-design, software-architecture, oop]
key_concepts: [Singleton pattern, Creational design patterns, Single Responsibility Principle, Global access point, Lazy initialization, Multithreading, Private constructor, Static methods]
code_examples: false
difficulty_level: intermediate
summary: |
  [The Singleton pattern is a creational design pattern that ensures a class has only one instance while providing a global access point to it. It addresses the need to control access to shared resources, such as a database, and offers a safer alternative to global variables. The pattern is implemented by making the class constructor private and providing a static creation method that lazily initializes and returns the single instance. While beneficial for ensuring uniqueness and global access, it can violate the Single Responsibility Principle and requires careful consideration in multithreaded environments. The article includes pseudocode examples and discusses its relationships with other design patterns.]
---
```

# Singleton

[](/)/ [Design Patterns](/design-patterns) / [Creational Patterns](/design-patterns/creational-patterns)

# Singleton

## Intent

**Singleton** is a creational design pattern that lets you ensure that a class has only one instance, while providing a global access point to this instance.

![A central, meditating "document" figure (representing the Singleton instance) with arrows pointing outwards to multiple smaller "document" figures (representing clients) and arrows pointing inwards from them, illustrating a single point of access and control.](/images/patterns/content/singleton/singleton.png?id=108a0b9b5ea5c4426e0afa4504491d6f)

## Problem

The Singleton pattern solves two problems at the same time, violating the _Single Responsibility Principle_:

1.  **Ensure that a class has just a single instance**. Why would anyone want to control how many instances a class has? The most common reason for this is to control access to some shared resource—for example, a database or a file.
    
    Here’s how it works: imagine that you created an object, but after a while decided to create a new one. Instead of receiving a fresh object, you’ll get the one you already created.
    
    Note that this behavior is impossible to implement with a regular constructor since a constructor call **must** always return a new object by design.
    

![A two-panel comic. The left panel shows a "document" figure opening a door, apologizing, "Sorry, I thought this room wasn't occupied." Another "document" figure is already inside. The right panel shows a circular room with many doors, and the same "document" figure repeatedly entering through different doors, only to find the same single "document" figure already inside, illustrating that no matter how you try to create a new instance, you always get the same one.](/images/patterns/content/singleton/singleton-comic-1-en.png?id=157509c5693a657ba465c7a9d58a7c25)

Clients may not even realize that they’re working with the same object all the time.

2.  **Provide a global access point to that instance**. Remember those global variables that you (all right, me) used to store some essential objects? While they’re very handy, they’re also very unsafe since any code can potentially overwrite the contents of those variables and crash the app.
    
    Just like a global variable, the Singleton pattern lets you access some object from anywhere in the program. However, it also protects that instance from being overwritten by other code.
    
    There’s another side to this problem: you don’t want the code that solves problem #1 to be scattered all over your program. It’s much better to have it within one class, especially if the rest of your code already depends on it.
    

Nowadays, the Singleton pattern has become so popular that people may call something a _singleton_ even if it solves just one of the listed problems.

## Solution

All implementations of the Singleton have these two steps in common:

*   Make the default constructor private, to prevent other objects from using the `new` operator with the Singleton class.
*   Create a static creation method that acts as a constructor. Under the hood, this method calls the private constructor to create an object and saves it in a static field. All following calls to this method return the cached object.

If your code has access to the Singleton class, then it’s able to call the Singleton’s static method. So whenever that method is called, the same object is always returned.

## Real-World Analogy

The government is an excellent example of the Singleton pattern. A country can have only one official government. Regardless of the personal identities of the individuals who form governments, the title, “The Government of X”, is a global point of access that identifies the group of people in charge.

## Structure

![UML-like diagram showing the structure of the Singleton pattern. A "Client" class interacts with a "Singleton" class. The "Singleton" class has a private static `instance` field, a private constructor, and a public static `getInstance()` method.](/images/patterns/diagrams/singleton/structure-en.png?id=4e4306d3a90f40d74c7a4d2d2506b8ec)![UML-like diagram showing the structure of the Singleton pattern, with an indexed callout. A "Client" class interacts with a "Singleton" class. The "Singleton" class has a private static `instance` field, a private constructor, and a public static `getInstance()` method. The `getInstance` method's logic is shown, including lazy initialization and a note about thread locking for multithreading.](/images/patterns/diagrams/singleton/structure-en-indexed.png?id=b0217ae066cd3b757677d119551f9a8f)

1.  The **Singleton** class declares the static method `getInstance` that returns the same instance of its own class.
    
    The Singleton’s constructor should be hidden from the client code. Calling the `getInstance` method should be the only way of getting the Singleton object.
    

## Pseudocode

In this example, the database connection class acts as a **Singleton**. This class doesn’t have a public constructor, so the only way to get its object is to call the `getInstance` method. This method caches the first created object and returns it in all subsequent calls.

```
// The Database class defines the `getInstance` method that lets
// clients access the same instance of a database connection
// throughout the program.
class Database is
    // The field for storing the singleton instance should be
    // declared static.
    private static field instance: Database

    // The singleton's constructor should always be private to
    // prevent direct construction calls with the `new`
    // operator.
    private constructor Database() is
        // Some initialization code, such as the actual
        // connection to a database server.
        // ...

    // The static method that controls access to the singleton
    // instance.
    public static method getInstance() is
        if (Database.instance == null) then
            acquireThreadLock() and then
                // Ensure that the instance hasn't yet been
                // initialized by another thread while this one
                // has been waiting for the lock's release.
                if (Database.instance == null) then
                    Database.instance = new Database()
        return Database.instance

    // Finally, any singleton should define some business logic
    // which can be executed on its instance.
    public method query(sql) is
        // For instance, all database queries of an app go
        // through this method. Therefore, you can place
        // throttling or caching logic here.
        // ...

class Application is
    method main() is
        Database foo = Database.getInstance()
        foo.query("SELECT ...")
        // ...
        Database bar = Database.getInstance()
        bar.query("SELECT ...")
        // The variable `bar` will contain the same object as
        // the variable `foo`.
```

## Applicability

Use the Singleton pattern when a class in your program should have just a single instance available to all clients; for example, a single database object shared by different parts of the program.

The Singleton pattern disables all other means of creating objects of a class except for the special creation method. This method either creates a new object or returns an existing one if it has already been created.

Use the Singleton pattern when you need stricter control over global variables.

Unlike global variables, the Singleton pattern guarantees that there’s just one instance of a class. Nothing, except for the Singleton class itself, can replace the cached instance.

Note that you can always adjust this limitation and allow creating any number of Singleton instances. The only piece of code that needs changing is the body of the `getInstance` method.

## How to Implement

1.  Add a private static field to the class for storing the singleton instance.
    
2.  Declare a public static creation method for getting the singleton instance.
    
3.  Implement “lazy initialization” inside the static method. It should create a new object on its first call and put it into the static field. The method should always return that instance on all subsequent calls.
    
4.  Make the constructor of the class private. The static method of the class will still be able to call the constructor, but not the other objects.
    
5.  Go over the client code and replace all direct calls to the singleton’s constructor with calls to its static creation method.
    

## Pros and Cons

*   You can be sure that a class has only a single instance.
*   You gain a global access point to that instance.
*   The singleton object is initialized only when it’s requested for the first time.

*   Violates the _Single Responsibility Principle_. The pattern solves two problems at the time.
*   The Singleton pattern can mask bad design, for instance, when the components of the program know too much about each other.
*   The pattern requires special treatment in a multithreaded environment so that multiple threads won’t create a singleton object several times.
*   It may be difficult to unit test the client code of the Singleton because many test frameworks rely on inheritance when producing mock objects. Since the constructor of the singleton class is private and overriding static methods is impossible in most languages, you will need to think of a creative way to mock the singleton. Or just don’t write the tests. Or don’t use the Singleton pattern.

## Relations with Other Patterns

*   A [Facade](/design-patterns/facade) class can often be transformed into a [Singleton](/design-patterns/singleton) since a single facade object is sufficient in most cases.
    
*   [Flyweight](/design-patterns/flyweight) would resemble [Singleton](/design-patterns/singleton) if you somehow managed to reduce all shared states of the objects to just one flyweight object. But there are two fundamental differences between these patterns:
    
    1.  There should be only one Singleton instance, whereas a _Flyweight_ class can have multiple instances with different intrinsic states.
    2.  The _Singleton_ object can be mutable. Flyweight objects are immutable.
*   [Abstract Factories](/design-patterns/abstract-factory), [Builders](/design-patterns/builder) and [Prototypes](/design-patterns/prototype) can all be implemented as [Singletons](/design-patterns/singleton).
    

## Code Examples

[![Singleton in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/singleton/csharp/example "Singleton in C#") [![Singleton in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/singleton/cpp/example "Singleton in C++") [![Singleton in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/singleton/go/example "Singleton in Go") [![Singleton in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/singleton/java/example "Singleton in Java") [![Singleton in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/singleton/php/example "Singleton in PHP") [![Singleton in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/singleton/python/example "Singleton in Python") [![Singleton in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/singleton/ruby/example "Singleton in Ruby") [![Singleton in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/singleton/rust/example "Singleton in Rust") [![Singleton in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/singleton/swift/example "Singleton in Swift") [![Singleton in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/singleton/typescript/example "Singleton in TypeScript")

![A promotional banner for an e-book titled "Dive Into Design Patterns", featuring a stylized hexagonal logo.](/images/patterns/banners/patterns-book-banner-3.png?id=7d445df13c80287beaab234b4f3b698c)

### Support our free website and own the eBook!

*   22 design patterns and 8 principles explained in depth.
*   409 well-structured, easy to read, jargon-free pages.
*   225 clear and helpful illustrations and diagrams.
*   An archive with code examples in 11 languages.
*   All devices supported: PDF/EPUB/MOBI/KFX formats.

[Learn more…](/design-patterns/book)

#### Read next

[Structural Patterns](/design-patterns/structural-patterns) 

#### Return

 [Prototype](/design-patterns/prototype)

![A collage of devices (desktop monitor, laptop, tablet, smartphone) all displaying the cover of the "Dive Into Design Patterns" e-book, alongside physical book covers, emphasizing multi-device compatibility.](/images/patterns/book/web-cover-en.png?id=328861769fd11617674e3b8a7e2dd9e7)

This article is a part of our eBook  
**Dive Into Design Patterns**.

[Learn more…](/design-patterns/book)