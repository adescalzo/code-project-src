```yaml
---
title: Abstract Factory in C# / Design Patterns
source: https://refactoring.guru/design-patterns/abstract-factory/csharp/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T18:02:34.509Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [.NET]
programming_languages: [C#]
tags: [design-patterns, creational-patterns, abstract-factory, csharp, software-design, object-oriented-programming]
key_concepts: [abstract-factory-pattern, creational-design-pattern, product-families, abstract-interfaces, concrete-classes, factory-pattern, object-oriented-design]
code_examples: false
difficulty_level: intermediate
summary: |
  The Abstract Factory is a creational design pattern that facilitates the creation of entire product families without requiring the specification of their concrete classes. It achieves this by defining an interface for product creation, with concrete factory classes responsible for instantiating specific product variants. Client code interacts solely with abstract interfaces of factories and products, enabling flexibility to work with various product families by simply providing a different concrete factory. The article provides a detailed conceptual example in C# to illustrate the pattern's structure and the relationships between its components.
---
```

# Abstract Factory in C# / Design Patterns

/ [Design Patterns](/design-patterns) / [Abstract Factory](/design-patterns/abstract-factory) / [C#](/design-patterns/csharp)

![Abstract Factory](/images/patterns/cards/abstract-factory-mini.png?id=4c3927c446313a38ce77dfee38111e27)

# Abstract Factory in C#

**Abstract Factory** is a creational design pattern, which solves the problem of creating entire product families without specifying their concrete classes.

Abstract Factory defines an interface for creating all distinct products but leaves the actual product creation to concrete factory classes. Each factory type corresponds to a certain product variety.

The client code calls the creation methods of a factory object instead of creating products directly with a constructor call (`new` operator). Since a factory corresponds to a single product variant, all its products will be compatible.

Client code works with factories and products only through their abstract interfaces. This lets the client code work with any product variants, created by the factory object. You just create a new concrete factory class and pass it to the client code.

> If you can’t figure out the difference between various factory patterns and concepts, then read our [Factory Comparison](/design-patterns/factory-comparison).

[Learn more about Abstract Factory](/design-patterns/abstract-factory)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The Abstract Factory pattern is pretty common in C# code. Many frameworks and libraries use it to provide a way to extend and customize their standard components.

**Identification:** The pattern is easy to recognize by methods, which return a factory object. Then, the factory is used for creating specific sub-components.

## Conceptual Example

This example illustrates the structure of the **Abstract Factory** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### Program.cs: Conceptual example

```csharp
using System;

namespace RefactoringGuru.DesignPatterns.AbstractFactory.Conceptual
{
    // The Abstract Factory interface declares a set of methods that return
    // different abstract products. These products are called a family and are
    // related by a high-level theme or concept. Products of one family are
    // usually able to collaborate among themselves. A family of products may
    // have several variants, but the products of one variant are incompatible
    // with products of another.
    public interface IAbstractFactory
    {
        IAbstractProductA CreateProductA();

        IAbstractProductB CreateProductB();
    }

    // Concrete Factories produce a family of products that belong to a single
    // variant. The factory guarantees that resulting products are compatible.
    // Note that signatures of the Concrete Factory's methods return an abstract
    // product, while inside the method a concrete product is instantiated.
    class ConcreteFactory1 : IAbstractFactory
    {
        public IAbstractProductA CreateProductA()
        {
            return new ConcreteProductA1();
        }

        public IAbstractProductB CreateProductB()
        {
            return new ConcreteProductB1();
        }
    }

    // Each Concrete Factory has a corresponding product variant.
    class ConcreteFactory2 : IAbstractFactory
    {
        public IAbstractProductA CreateProductA()
        {
            return new ConcreteProductA2();
        }

        public IAbstractProductB CreateProductB()
        {
            return new ConcreteProductB2();
        }
    }

    // Each distinct product of a product family should have a base interface.
    // All variants of the product must implement this interface.
    public interface IAbstractProductA
    {
        string UsefulFunctionA();
    }

    // Concrete Products are created by corresponding Concrete Factories.
    class ConcreteProductA1 : IAbstractProductA
    {
        public string UsefulFunctionA()
        {
            return "The result of the product A1.";
        }
    }

    class ConcreteProductA2 : IAbstractProductA
    {
        public string UsefulFunctionA()
        {
            return "The result of the product A2.";
        }
    }

    // Here's the the base interface of another product. All products can
    // interact with each other, but proper interaction is possible only between
    // products of the same concrete variant.
    public interface IAbstractProductB
    {
        // Product B is able to do its own thing...
        string UsefulFunctionB();

        // ...but it also can collaborate with the ProductA.
        //
        // The Abstract Factory makes sure that all products it creates are of
        // the same variant and thus, compatible.
        string AnotherUsefulFunctionB(IAbstractProductA collaborator);
    }

    // Concrete Products are created by corresponding Concrete Factories.
    class ConcreteProductB1 : IAbstractProductB
    {
        public string UsefulFunctionB()
        {
            return "The result of the product B1.";
        }

        // The variant, Product B1, is only able to work correctly with the
        // variant, Product A1. Nevertheless, it accepts any instance of
        // AbstractProductA as an argument.
        public string AnotherUsefulFunctionB(IAbstractProductA collaborator)
        {
            var result = collaborator.UsefulFunctionA();

            return $"The result of the B1 collaborating with the ({result})";
        }
    }

    class ConcreteProductB2 : IAbstractProductB
    {
        public string UsefulFunctionB()
        {
            return "The result of the product B2.";
        }

       // The variant, Product B2, is only able to work correctly with the
       // variant, Product A2. Nevertheless, it accepts any instance of
       // AbstractProductA as an argument.
        public string AnotherUsefulFunctionB(IAbstractProductA collaborator)
        {
            var result = collaborator.UsefulFunctionA();

            return $"The result of the B2 collaborating with the ({result})";
        }
    }

    // The client code works with factories and products only through abstract
    // types: AbstractFactory and AbstractProduct. This lets you pass any
    // factory or product subclass to the client code without breaking it.
    class Client
    {
        public void Main()
        {
            // The client code can work with any concrete factory class.
            Console.WriteLine("Client: Testing client code with the first factory type...");
            ClientMethod(new ConcreteFactory1());
            Console.WriteLine();

            Console.WriteLine("Client: Testing the same client code with the second factory type...");
            ClientMethod(new ConcreteFactory2());
        }

        public void ClientMethod(IAbstractFactory factory)
        {
            var productA = factory.CreateProductA();
            var productB = factory.CreateProductB();

            Console.WriteLine(productB.UsefulFunctionB());
            Console.WriteLine(productB.AnotherUsefulFunctionB(productA));
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

```
Client: Testing client code with the first factory type...
The result of the product B1.
The result of the B1 collaborating with the (The result of the product A1.)

Client: Testing the same client code with the second factory type...
The result of the product B2.
The result of the B2 collaborating with the (The result of the product A2.)
```

#### Read next

[Builder in C#](/design-patterns/builder/csharp/example) 

#### Return

 [Design Patterns in C#](/design-patterns/csharp)

## Abstract Factory in Other Languages

[![Abstract Factory in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858 "Abstract Factory in C++")](/design-patterns/abstract-factory/cpp/example) [![Abstract Factory in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca "Abstract Factory in Go")](/design-patterns/abstract-factory/go/example) [![Abstract Factory in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e "Abstract Factory in Java")](/design-patterns/abstract-factory/java/example) [![Abstract Factory in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618 "Abstract Factory in PHP")](/design-patterns/abstract-factory/php/example) [![Abstract Factory in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f "Abstract Factory in Python")](/design-patterns/abstract-factory/python/example) [![Abstract Factory in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb "Abstract Factory in Ruby")](/design-patterns/abstract-factory/ruby/example) [![Abstract Factory in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87 "Abstract Factory in Rust")](/design-patterns/abstract-factory/rust/example) [![Abstract Factory in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d "Abstract Factory in Swift")](/design-patterns/abstract-factory/swift/example) [![Abstract Factory in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7 "Abstract Factory in TypeScript")](/design-patterns/abstract-factory/typescript/example)

![Illustration of software development tools and interfaces](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [Program](#example-0--Program-cs)

 [Output](#example-0--Output-txt)