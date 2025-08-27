```yaml
---
title: Facade in Python / Design Patterns
source: https://refactoring.guru/design-patterns/facade/python/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:56:39.353Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: []
programming_languages: [Python, C#, C++, Go, Java, PHP, Ruby, Rust, Swift, TypeScript]
tags: [design-patterns, facade-pattern, python, structural-pattern, software-design, object-oriented-programming, api-design, code-organization]
key_concepts: [facade-pattern, structural-design-pattern, subsystem, simplified-interface, complexity-management, dependency-management, client-code]
code_examples: false
difficulty_level: intermediate
summary: |
  [The Facade design pattern is a structural pattern that offers a simplified, limited interface to a complex system of classes, libraries, or frameworks. Its primary goal is to reduce the overall complexity of an application and centralize unwanted dependencies. This article provides a conceptual example of the Facade pattern implemented in Python, illustrating its structure, the roles of its constituent classes (Facade and Subsystems), and their relationships. The example demonstrates how a client interacts with the complex subsystems through the simplified Facade interface, effectively shielding the client from underlying complexity. The article also references implementations of the Facade pattern in other programming languages.]
---
```

# Facade in Python / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Facade](/design-patterns/facade) / [Python](/design-patterns/python)

![A red outlined icon depicting a classical building with columns, symbolizing the Facade design pattern.](/images/patterns/cards/facade-mini.png?id=71ad6fa98b168c11cb3a1a9517dedf78)

# **Facade** in Python

**Facade** is a structural design pattern that provides a simplified (but limited) interface to a complex system of classes, library or framework.

While Facade decreases the overall complexity of the application, it also helps to move unwanted dependencies to one place.

[Learn more about Facade](/design-patterns/facade)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [main](#example-0--main-py)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The Facade pattern is commonly used in apps written in Python. It’s especially handy when working with complex libraries and APIs.

**Identification:** Facade can be recognized in a class that has a simple interface, but delegates most of the work to other classes. Usually, facades manage the full life cycle of objects they use.

## Conceptual Example

This example illustrates the structure of the **Facade** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### [](#example-0--main-py)**main.py:** Conceptual example

from \_\_future\_\_ import annotations


class Facade:
    """
    The Facade class provides a simple interface to the complex logic of one or
    several subsystems. The Facade delegates the client requests to the
    appropriate objects within the subsystem. The Facade is also responsible for
    managing their lifecycle. All of this shields the client from the undesired
    complexity of the subsystem.
    """

    def \_\_init\_\_(self, subsystem1: Subsystem1, subsystem2: Subsystem2) -> None:
        """
        Depending on your application's needs, you can provide the Facade with
        existing subsystem objects or force the Facade to create them on its
        own.
        """

        self.\_subsystem1 = subsystem1 or Subsystem1()
        self.\_subsystem2 = subsystem2 or Subsystem2()

    def operation(self) -> str:
        """
        The Facade's methods are convenient shortcuts to the sophisticated
        functionality of the subsystems. However, clients get only to a fraction
        of a subsystem's capabilities.
        """

        results = []
        results.append("Facade initializes subsystems:")
        results.append(self.\_subsystem1.operation1())
        results.append(self.\_subsystem2.operation1())
        results.append("Facade orders subsystems to perform the action:")
        results.append(self.\_subsystem1.operation\_n())
        results.append(self.\_subsystem2.operation\_z())
        return "\\n".join(results)


class Subsystem1:
    """
    The Subsystem can accept requests either from the facade or client directly.
    In any case, to the Subsystem, the Facade is yet another client, and it's
    not a part of the Subsystem.
    """

    def operation1(self) -> str:
        return "Subsystem1: Ready!"

    # ...

    def operation\_n(self) -> str:
        return "Subsystem1: Go!"


class Subsystem2:
    """
    Some facades can work with multiple subsystems at the same time.
    """

    def operation1(self) -> str:
        return "Subsystem2: Get ready!"

    # ...

    def operation\_z(self) -> str:
        return "Subsystem2: Fire!"


def client\_code(facade: Facade) -> None:
    """
    The client code works with complex subsystems through a simple interface
    provided by the Facade. When a facade manages the lifecycle of the
    subsystem, the client might not even know about the existence of the
    subsystem. This approach lets you keep the complexity under control.
    """

    print(facade.operation(), end="")


if \_\_name\_\_ == "\_\_main\_\_":
    # The client code may have some of the subsystem's objects already created.
    # In this case, it might be worthwhile to initialize the Facade with these
    # objects instead of letting the Facade create new instances.
    subsystem1 = Subsystem1()
    subsystem2 = Subsystem2()
    facade = Facade(subsystem1, subsystem2)
    client\_code(facade)

#### [](#example-0--Output-txt)**Output.txt:** Execution result

Facade initializes subsystems:
Subsystem1: Ready!
Subsystem2: Get ready!
Facade orders subsystems to perform the action:
Subsystem1: Go!
Subsystem2: Fire!

#### Read next

[Flyweight in Python](/design-patterns/flyweight/python/example) 

#### Return

 [Decorator in Python](/design-patterns/decorator/python/example)

## **Facade** in Other Languages

[![Facade in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58 "Facade in C#")](/design-patterns/facade/csharp/example) [![Facade in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858 "Facade in C++")](/design-patterns/facade/cpp/example) [![Facade in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca "Facade in Go")](/design-patterns/facade/go/example) [![Facade in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e "Facade in Java")](/design-patterns/facade/java/example) [![Facade in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618 "Facade in PHP")](/design-patterns/facade/php/example) [![Facade in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb "Facade in Ruby")](/design-patterns/facade/ruby/example) [![Facade in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87 "Facade in Rust")](/design-patterns/facade/rust/example) [![Facade in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d "Facade in Swift")](/design-patterns/facade/swift/example) [![Facade in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7 "Facade in TypeScript")](/design-patterns/facade/typescript/example)

![An illustration showing various digital devices (desktop, tablet, smartphone) displaying code and user interfaces, surrounded by icons representing development tools and data visualizations, suggesting a comprehensive development environment or system.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [main](#example-0--main-py)

 [Output](#example-0--Output-txt)