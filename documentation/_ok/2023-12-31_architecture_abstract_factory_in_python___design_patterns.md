```yaml
---
title: Abstract Factory in Python / Design Patterns
source: https://refactoring.guru/design-patterns/abstract-factory/python/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:58:38.348Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [abc]
programming_languages: [Python, C#, C++, Go, Java, PHP, Ruby, Rust, Swift, TypeScript]
tags: [design-patterns, creational-patterns, python, abstract-factory, object-oriented-programming, software-design, interfaces, abstract-classes, factory-pattern]
key_concepts: [abstract-factory-pattern, creational-design-patterns, product-families, abstract-interfaces, concrete-classes, client-code, compatibility, polymorphism]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Abstract Factory design pattern, a creational pattern that enables the creation of entire product families without specifying their concrete classes. It explains how the pattern defines an interface for creating abstract products, with concrete factories responsible for instantiating specific product variants. The client code interacts solely with abstract interfaces, allowing for flexible integration of different product families. A detailed conceptual example in Python demonstrates the pattern's structure, roles of its components, and their relationships. The article also highlights the pattern's common usage in frameworks and libraries for extensibility.]
---
```

# Abstract Factory in Python / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Abstract Factory](/design-patterns/abstract-factory) / [Python](/design-patterns/python)

![Abstract Factory](/images/patterns/cards/abstract-factory-mini.png?id=4c3927c446313a38ce77dfee38111e27)
*Description: A small icon or card image representing the Abstract Factory design pattern, likely used for navigation or visual identification.*

# **Abstract Factory** in Python

**Abstract Factory** is a creational design pattern, which solves the problem of creating entire product families without specifying their concrete classes.

Abstract Factory defines an interface for creating all distinct products but leaves the actual product creation to concrete factory classes. Each factory type corresponds to a certain product variety.

The client code calls the creation methods of a factory object instead of creating products directly with a constructor call (`new` operator). Since a factory corresponds to a single product variant, all its products will be compatible.

Client code works with factories and products only through their abstract interfaces. This lets the client code work with any product variants, created by the factory object. You just create a new concrete factory class and pass it to the client code.

> If you can’t figure out the difference between various factory patterns and concepts, then read our [Factory Comparison](/design-patterns/factory-comparison).

[Learn more about Abstract Factory](/design-patterns/abstract-factory)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [main](#example-0--main-py)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The Abstract Factory pattern is pretty common in Python code. Many frameworks and libraries use it to provide a way to extend and customize their standard components.

**Identification:** The pattern is easy to recognize by methods, which return a factory object. Then, the factory is used for creating specific sub-components.

## Conceptual Example

This example illustrates the structure of the **Abstract Factory** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### [](#example-0--main-py)**main.py:** Conceptual example

from \_\_future\_\_ import annotations
from abc import ABC, abstractmethod

class AbstractFactory(ABC):
    """
    The Abstract Factory interface declares a set of methods that return
    different abstract products. These products are called a family and are
    related by a high-level theme or concept. Products of one family are usually
    able to collaborate among themselves. A family of products may have several
    variants, but the products of one variant are incompatible with products of
    another.
    """
    @abstractmethod
    def create\_product\_a(self) \-\> AbstractProductA:
        pass

    @abstractmethod
    def create\_product\_b(self) \-\> AbstractProductB:
        pass

class ConcreteFactory1(AbstractFactory):
    """
    Concrete Factories produce a family of products that belong to a single
    variant. The factory guarantees that resulting products are compatible. Note
    that signatures of the Concrete Factory's methods return an abstract
    product, while inside the method a concrete product is instantiated.
    """

    def create\_product\_a(self) \-\> AbstractProductA:
        return ConcreteProductA1()

    def create\_product\_b(self) \-\> AbstractProductB:
        return ConcreteProductB1()

class ConcreteFactory2(AbstractFactory):
    """
    Each Concrete Factory has a corresponding product variant.
    """

    def create\_product\_a(self) \-\> AbstractProductA:
        return ConcreteProductA2()

    def create\_product\_b(self) \-\> AbstractProductB:
        return ConcreteProductB2()

class AbstractProductA(ABC):
    """
    Each distinct product of a product family should have a base interface. All
    variants of the product must implement this interface.
    """

    @abstractmethod
    def useful\_function\_a(self) \-\> str:
        pass

"""
Concrete Products are created by corresponding Concrete Factories.
"""

class ConcreteProductA1(AbstractProductA):
    def useful\_function\_a(self) \-\> str:
        return "The result of the product A1."

class ConcreteProductA2(AbstractProductA):
    def useful\_function\_a(self) \-\> str:
        return "The result of the product A2."

class AbstractProductB(ABC):
    """
    Here's the the base interface of another product. All products can interact
    with each other, but proper interaction is possible only between products of
    the same concrete variant.
    """
    @abstractmethod
    def useful\_function\_b(self) \-\> None:
        """
        Product B is able to do its own thing...
        """
        pass

    @abstractmethod
    def another\_useful\_function\_b(self, collaborator: AbstractProductA) \-\> None:
        """
        ...but it also can collaborate with the ProductA.

        The Abstract Factory makes sure that all products it creates are of the
        same variant and thus, compatible.
        """
        pass

"""
Concrete Products are created by corresponding Concrete Factories.
"""

class ConcreteProductB1(AbstractProductB):
    def useful\_function\_b(self) \-\> str:
        return "The result of the product B1."

    """
    The variant, Product B1, is only able to work correctly with the variant,
    Product A1. Nevertheless, it accepts any instance of AbstractProductA as an
    argument.
    """

    def another\_useful\_function\_b(self, collaborator: AbstractProductA) \-\> str:
        result \= collaborator.useful\_function\_a()
        return f"The result of the B1 collaborating with the ({result})"

class ConcreteProductB2(AbstractProductB):
    def useful\_function\_b(self) \-\> str:
        return "The result of the product B2."

    def another\_useful\_function\_b(self, collaborator: AbstractProductA):
        """
        The variant, Product B2, is only able to work correctly with the
        variant, Product A2. Nevertheless, it accepts any instance of
        AbstractProductA as an argument.
        """
        result \= collaborator.useful\_function\_a()
        return f"The result of the B2 collaborating with the ({result})"

def client\_code(factory: AbstractFactory) \-\> None:
    """
    The client code works with factories and products only through abstract
    types: AbstractFactory and AbstractProduct. This lets you pass any factory
    or product subclass to the client code without breaking it.
    """
    product\_a \= factory.create\_product\_a()
    product\_b \= factory.create\_product\_b()

    print(f"{product\_b.useful\_function\_b()}")
    print(f"{product\_b.another\_useful\_function\_b(product\_a)}", end\="")

if \_\_name\_\_ \== "\_\_main\_\_":
    """
    The client code can work with any concrete factory class.
    """
    print("Client: Testing client code with the first factory type:")
    client\_code(ConcreteFactory1())

    print("\\n")

    print("Client: Testing the same client code with the second factory type:")
    client\_code(ConcreteFactory2())

#### [](#example-0--Output-txt)**Output.txt:** Execution result

Client: Testing client code with the first factory type:
The result of the product B1.
The result of the B1 collaborating with the (The result of the product A1.)

Client: Testing the same client code with the second factory type:
The result of the product B2.
The result of the B2 collaborating with the (The result of the product A2.)

#### Read next

[Builder in Python](/design-patterns/builder/python/example) 

#### Return

 [Design Patterns in Python](/design-patterns/python)

## **Abstract Factory** in Other Languages

[![Abstract Factory in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/abstract-factory/csharp/example "Abstract Factory in C#") [![Abstract Factory in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/abstract-factory/cpp/example "Abstract Factory in C++") [![Abstract Factory in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/abstract-factory/go/example "Abstract Factory in Go") [![Abstract Factory in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/abstract-factory/java/example "Abstract Factory in Java") [![Abstract Factory in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/abstract-factory/php/example "Abstract Factory in PHP") [![Abstract Factory in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/abstract-factory/ruby/example "Abstract Factory in Ruby") [![Abstract Factory in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/abstract-factory/rust/example "Abstract Factory in Rust") [![Abstract Factory in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/abstract-factory/swift/example "Abstract Factory in Swift") [![Abstract Factory in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/abstract-factory/typescript/example "Abstract Factory in TypeScript")

[![](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)](/design-patterns/book)
*Description: A banner image promoting an eBook "Dive Into Design Patterns" and access to an archive of examples that can be opened in an IDE. It depicts various development tools and code snippets on screens.*

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [main](#example-0--main-py)

 [Output](#example-0--Output-txt)

*Description of the separately provided image:*
A complex illustration featuring a central tablet/monitor displaying UI elements and code snippets, surrounded by various development tools like a hammer, wrench, code editor windows, and mobile phone interfaces. This image visually represents software development, coding, and potentially the interconnectedness of different components or tools in a system.