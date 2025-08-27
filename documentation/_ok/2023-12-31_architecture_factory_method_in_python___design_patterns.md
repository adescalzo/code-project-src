```yaml
---
title: Factory Method in Python / Design Patterns
source: https://refactoring.guru/design-patterns/factory-method/python/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:58:12.468Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Python]
programming_languages: [Python, C#, C++, Go, Java, PHP, Ruby, Rust, Swift, TypeScript]
tags: [design-patterns, creational-patterns, factory-method, python, object-oriented-programming, software-design, abstraction, inheritance, oop, code-structure]
key_concepts: [factory-method, creational-design-pattern, abstraction, interface, inheritance, polymorphism, object-creation, design-principles]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article introduces the Factory Method, a creational design pattern that facilitates object creation without specifying concrete classes. It explains how this pattern defines a method for object instantiation, allowing subclasses to customize the type of objects produced. A detailed conceptual example in Python illustrates the roles of Creator, Product, and their concrete implementations. The content emphasizes the pattern's utility in providing code flexibility and its common identification characteristics. It also briefly mentions the pattern's applicability across various other programming languages.]
---
```

# Factory Method in Python / Design Patterns

![Factory Method](/images/patterns/cards/factory-method-mini.png?id=72619e9527893374b98a5913779ac167)

# **Factory Method** in Python

**Factory method** is a creational design pattern which solves the problem of creating product objects without specifying their concrete classes.

The Factory Method defines a method, which should be used for creating objects instead of using a direct constructor call (`new` operator). Subclasses can override this method to change the class of objects that will be created.

> If you can’t figure out the difference between various factory patterns and concepts, then read our [Factory Comparison](/design-patterns/factory-comparison).

**Complexity:**

**Popularity:**

**Usage examples:** The Factory Method pattern is widely used in Python code. It’s very useful when you need to provide a high level of flexibility for your code.

**Identification:** Factory methods can be recognized by creation methods that construct objects from concrete classes. While concrete classes are used during the object creation, the return type of the factory methods is usually declared as either an abstract class or an interface.

## Conceptual Example

This example illustrates the structure of the **Factory Method** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

**main.py:** Conceptual example

```python
from __future__ import annotations
from abc import ABC, abstractmethod


class Creator(ABC):
    """
    The Creator class declares the factory method that is supposed to return an
    object of a Product class. The Creator's subclasses usually provide the
    implementation of this method.
    """

    @abstractmethod
    def factory_method(self):
        """
        Note that the Creator may also provide some default implementation of
        the factory method.
        """
        pass

    def some_operation(self) -> str:
        """
        Also note that, despite its name, the Creator's primary responsibility
        is not creating products. Usually, it contains some core business logic
        that relies on Product objects, returned by the factory method.
        Subclasses can indirectly change that business logic by overriding the
        factory method and returning a different type of product from it.
        """

        # Call the factory method to create a Product object.
        product = self.factory_method()

        # Now, use the product.
        result = f"Creator: The same creator's code has just worked with {product.operation()}"

        return result


"""
Concrete Creators override the factory method in order to change the resulting
product's type.
"""


class ConcreteCreator1(Creator):
    """
    Note that the signature of the method still uses the abstract product type,
    even though the concrete product is actually returned from the method. This
    way the Creator can stay independent of concrete product classes.
    """

    def factory_method(self) -> Product:
        return ConcreteProduct1()


class ConcreteCreator2(Creator):
    def factory_method(self) -> Product:
        return ConcreteProduct2()


class Product(ABC):
    """
    The Product interface declares the operations that all concrete products
    must implement.
    """

    @abstractmethod
    def operation(self) -> str:
        pass


"""
Concrete Products provide various implementations of the Product interface.
"""


class ConcreteProduct1(Product):
    def operation(self) -> str:
        return "{Result of the ConcreteProduct1}"


class ConcreteProduct2(Product):
    def operation(s_elf) -> str:
        return "{Result of the ConcreteProduct2}"


def client_code(creator: Creator) -> None:
    """
    The client code works with an instance of a concrete creator, albeit through
    its base interface. As long as the client keeps working with the creator via
    the base interface, you can pass it any creator's subclass.
    """

    print(f"Client: I'm not aware of the creator's class, but it still works.\n"
          f"{creator.some_operation()}", end="")


if __name__ == "__main__":
    print("App: Launched with the ConcreteCreator1.")
    client_code(ConcreteCreator1())
    print("\n")

    print("App: Launched with the ConcreteCreator2.")
    client_code(ConcreteCreator2())
```

**Output.txt:** Execution result

```
App: Launched with the ConcreteCreator1.
Client: I'm not aware of the creator's class, but it still works.
Creator: The same creator's code has just worked with {Result of the ConcreteProduct1}

App: Launched with the ConcreteCreator2.
Client: I'm not aware of the creator's class, but it still works.
Creator: The same creator's code has just worked with {Result of the ConcreteProduct2}
```

#### Read next

[Prototype in Python](/design-patterns/prototype/python/example)

#### Return

[Builder in Python](/design-patterns/builder/python/example)

## **Factory Method** in Other Languages

[![Factory Method in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/factory-method/csharp/example "Factory Method in C#") [![Factory Method in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/factory-method/cpp/example "Factory Method in C++") [![Factory Method in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/factory-method/go/example "Factory Method in Go") [![Factory Method in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/factory-method/java/example "Factory Method in Java") [![Factory Method in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/factory-method/php/example "Factory Method in PHP") [![Factory Method in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/factory-method/ruby/example "Factory Method in Ruby") [![Factory Method in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/factory-method/rust/example "Factory Method in Rust") [![Factory Method in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/factory-method/swift/example "Factory Method in Swift") [![Factory Method in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/factory-method/typescript/example "Factory Method in TypeScript")

[![](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)](/design-patterns/book)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

---
### Image Analysis
An abstract illustration depicting various aspects of software development and user interface/experience design. It features a central desktop monitor displaying a complex UI with charts and data, flanked by a tablet and a mobile phone showing application interfaces. Surrounding these devices are elements like code snippets, a database icon, a hammer, a wrench, and various abstract shapes representing data flow and connectivity. The overall theme suggests a comprehensive approach to building and designing software across multiple platforms.