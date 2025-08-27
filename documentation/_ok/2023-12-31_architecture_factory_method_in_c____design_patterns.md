```yaml
---
title: Factory Method in C# / Design Patterns
source: https://refactoring.guru/design-patterns/factory-method/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T18:00:19.758Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [.NET]
programming_languages: [C#]
tags: [design-pattern, creational-pattern, factory-method, csharp, object-oriented-programming, oop]
key_concepts: [Factory Method, creational design pattern, abstract class, interface, object creation, polymorphism, inheritance]
code_examples: false
difficulty_level: intermediate
summary: |
  The article provides a detailed explanation of the Factory Method design pattern, a creational pattern that enables object creation without specifying concrete classes. It clarifies how the pattern defines a method for object instantiation, allowing subclasses to override it to alter the type of objects created. A comprehensive conceptual example in C# illustrates the pattern's structure, including abstract creators, concrete creators, product interfaces, and concrete products. The content emphasizes the flexibility and extensibility gained by using this pattern, particularly for client code interacting with creators via a base interface.
---
```

# Factory Method in C# / Design Patterns

/ [Design Patterns](/design-patterns) / [Factory Method](/design-patterns/factory-method) / [C#](/design-patterns/csharp)

![A small icon representing the Factory Method design pattern.](/images/patterns/cards/factory-method-mini.png?id=72619e9527893374b98a5913779ac167)

# Factory Method in C#

**Factory method** is a creational design pattern which solves the problem of creating product objects without specifying their concrete classes.

The Factory Method defines a method, which should be used for creating objects instead of using a direct constructor call (`new` operator). Subclasses can override this method to change the class of objects that will be created.

> If you can’t figure out the difference between various factory patterns and concepts, then read our [Factory Comparison](/design-patterns/factory-comparison).

[Learn more about Factory Method](/design-patterns/factory-method)

Navigation

*   [Intro](#)
*   [Conceptual Example](#example-0)
*   [Program](#example-0--Program-cs)
*   [Output](#example-0--Output-txt)

**Complexity:** (Information missing in original content, assuming visual representation)
**Popularity:** (Information missing in original content, assuming visual representation)

**Usage examples:** The Factory Method pattern is widely used in C# code. It’s very useful when you need to provide a high level of flexibility for your code.

**Identification:** Factory methods can be recognized by creation methods that construct objects from concrete classes. While concrete classes are used during the object creation, the return type of the factory methods is usually declared as either an abstract class or an interface.

## Conceptual Example

This example illustrates the structure of the **Factory Method** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### Program.cs: Conceptual example

```csharp
using System;

namespace RefactoringGuru.DesignPatterns.FactoryMethod.Conceptual
{
    // The Creator class declares the factory method that is supposed to return
    // an object of a Product class. The Creator's subclasses usually provide
    // the implementation of this method.
    abstract class Creator
    {
        // Note that the Creator may also provide some default implementation of
        // the factory method.
        public abstract IProduct FactoryMethod();

        // Also note that, despite its name, the Creator's primary
        // responsibility is not creating products. Usually, it contains some
        // core business logic that relies on Product objects, returned by the
        // factory method. Subclasses can indirectly change that business logic
        // by overriding the factory method and returning a different type of
        // product from it.
        public string SomeOperation()
        {
            // Call the factory method to create a Product object.
            var product = FactoryMethod();
            // Now, use the product.
            var result = "Creator: The same creator's code has just worked with "
                + product.Operation();

            return result;
        }
    }

    // Concrete Creators override the factory method in order to change the
    // resulting product's type.
    class ConcreteCreator1 : Creator
    {
        // Note that the signature of the method still uses the abstract product
        // type, even though the concrete product is actually returned from the
        // method. This way the Creator can stay independent of concrete product
        // classes.
        public override IProduct FactoryMethod()
        {
            return new ConcreteProduct1();
        }
    }

    class ConcreteCreator2 : Creator
    {
        public override IProduct FactoryMethod()
        {
            return new ConcreteProduct2();
        }
    }

    // The Product interface declares the operations that all concrete products
    // must implement.
    public interface IProduct
    {
        string Operation();
    }

    // Concrete Products provide various implementations of the Product
    // interface.
    class ConcreteProduct1 : IProduct
    {
        public string Operation()
        {
            return "{Result of ConcreteProduct1}";
        }
    }

    class ConcreteProduct2 : IProduct
    {
        public string Operation()
        {
            return "{Result of ConcreteProduct2}";
        }
    }

    class Client
    {
        public void Main()
        {
            Console.WriteLine("App: Launched with the ConcreteCreator1.");
            ClientCode(new ConcreteCreator1());
            
            Console.WriteLine("");

            Console.WriteLine("App: Launched with the ConcreteCreator2.");
            ClientCode(new ConcreteCreator2());
        }

        // The client code works with an instance of a concrete creator, albeit
        // through its base interface. As long as the client keeps working with
        // the creator via the base interface, you can pass it any creator's
        // subclass.
        public void ClientCode(Creator creator)
        {
            // ...
            Console.WriteLine("Client: I'm not aware of the creator's class," +
                "but it still works.\\n" + creator.SomeOperation());
            // ...
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            new Client().Main();
        }
    }
}
```

#### Output.txt: Execution result

```txt
App: Launched with the ConcreteCreator1.
Client: I'm not aware of the creator's class, but it still works.
Creator: The same creator's code has just worked with {Result of ConcreteProduct1}

App: Launched with the ConcreteCreator2.
Client: I'm not aware of the creator's class, but it still works.
Creator: The same creator's code has just worked with {Result of ConcreteProduct2}
```

#### Read next

[Prototype in C#](/design-patterns/prototype/csharp/example)

#### Return

[Builder in C#](/design-patterns/builder/csharp/example)

## Factory Method in Other Languages

*   [Factory Method in C++](/design-patterns/factory-method/cpp/example)
*   [Factory Method in Go](/design-patterns/factory-method/go/example)
*   [Factory Method in Java](/design-patterns/factory-method/java/example)
*   [Factory Method in PHP](/design-patterns/factory-method/php/example)
*   [Factory Method in Python](/design-patterns/factory-method/python/example)
*   [Factory Method in Ruby](/design-patterns/factory-method/ruby/example)
*   [Factory Method in Rust](/design-patterns/factory-method/rust/example)
*   [Factory Method in Swift](/design-patterns/factory-method/swift/example)
*   [Factory Method in TypeScript](/design-patterns/factory-method/typescript/example)

[![An illustration depicting various digital devices (desktop, tablet, smartphone) displaying code, UI elements, and data, surrounded by development tools, symbolizing software development and design.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)](/design-patterns/book)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

*   [Intro](#)
*   [Conceptual Example](#example-0)
*   [Program](#example-0--Program-cs)
*   [Output](#example-0--Output-txt)