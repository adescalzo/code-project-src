```yaml
---
title: Bridge in Python / Design Patterns
source: https://refactoring.guru/design-patterns/bridge/python/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:58:21.899Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Python, Python Standard Library]
programming_languages: [Python]
tags: [design-patterns, structural-pattern, python, object-oriented-programming, abstraction, decoupling, software-architecture, class-design]
key_concepts: [Bridge pattern, abstraction, implementation, class-hierarchies, delegation, decoupling, separation-of-concerns]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explains the Bridge structural design pattern, which separates business logic into independent class hierarchies: Abstraction and Implementation. The Abstraction holds a reference to an Implementation object, delegating work to it, allowing both hierarchies to evolve independently. The content provides a conceptual example in Python, demonstrating how the client code interacts solely with the Abstraction, supporting various abstraction-implementation combinations. It highlights the pattern's usefulness in cross-platform applications or when dealing with multiple external service providers.]
---
```

# Bridge in Python / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Bridge](/design-patterns/bridge) / [Python](/design-patterns/python)

![A stylized icon depicting a bridge structure, symbolizing the Bridge design pattern.](/images/patterns/cards/bridge-mini.png?id=b389101d8ee8e23ffa1b534c704d0774)

# **Bridge** in Python

**Bridge** is a structural design pattern that divides business logic or huge class into separate class hierarchies that can be developed independently.

One of these hierarchies (often called the Abstraction) will get a reference to an object of the second hierarchy (Implementation). The abstraction will be able to delegate some (sometimes, most) of its calls to the implementations object. Since all implementations will have a common interface, they’d be interchangeable inside the abstraction.

[Learn more about Bridge](/design-patterns/bridge)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [main](#example-0--main-py)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The Bridge pattern is especially useful when dealing with cross-platform apps, supporting multiple types of database servers or working with several API providers of a certain kind (for example, cloud platforms, social networks, etc.)

**Identification:** Bridge can be recognized by a clear distinction between some controlling entity and several different platforms that it relies on.

## Conceptual Example

This example illustrates the structure of the **Bridge** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### main.py: Conceptual example

```python
from __future__ import annotations
from abc import ABC, abstractmethod

class Abstraction:
    """
    The Abstraction defines the interface for the "control" part of the two
    class hierarchies. It maintains a reference to an object of the
    Implementation hierarchy and delegates all of the real work to this object.
    """

    def __init__(self, implementation: Implementation) -> None:
        self.implementation = implementation

    def operation(self) -> str:
        return (f"Abstraction: Base operation with:\n"
                f"{self.implementation.operation_implementation()}")

class ExtendedAbstraction(Abstraction):
    """
    You can extend the Abstraction without changing the Implementation classes.
    """

    def operation(self) -> str:
        return (f"ExtendedAbstraction: Extended operation with:\n"
                f"{self.implementation.operation_implementation()}")

class Implementation(ABC):
    """
    The Implementation defines the interface for all implementation classes. It
    doesn't have to match the Abstraction's interface. In fact, the two
    interfaces can be entirely different. Typically the Implementation interface
    provides only primitive operations, while the Abstraction defines higher-
    level operations based on those primitives.
    """

    @abstractmethod
    def operation_implementation(self) -> str:
        pass

"""
Each Concrete Implementation corresponds to a specific platform and implements
the Implementation interface using that platform's API.
"""

class ConcreteImplementationA(Implementation):
    def operation_implementation(self) -> str:
        return "ConcreteImplementationA: Here's the result on the platform A."

class ConcreteImplementationB(Implementation):
    def operation_implementation(self) -> str:
        return "ConcreteImplementationB: Here's the result on the platform B."

def client_code(abstraction: Abstraction) -> None:
    """
    Except for the initialization phase, where an Abstraction object gets linked
    with a specific Implementation object, the client code should only depend on
    the Abstraction class. This way the client code can support any abstraction-
    implementation combination.
    """

    # ...

    print(abstraction.operation(), end="")

    # ...

if __name__ == "__main__":
    """
    The client code should be able to work with any pre-configured abstraction-
    implementation combination.
    """

    implementation = ConcreteImplementationA()
    abstraction = Abstraction(implementation)
    client_code(abstraction)

    print("\n")

    implementation = ConcreteImplementationB()
    abstraction = ExtendedAbstraction(implementation)
    client_code(abstraction)
```

#### Output.txt: Execution result

```
Abstraction: Base operation with:
ConcreteImplementationA: Here's the result on the platform A.

ExtendedAbstraction: Extended operation with:
ConcreteImplementationB: Here's the result on the platform B.
```

#### Read next

[Composite in Python](/design-patterns/composite/python/example) 

#### Return

 [Adapter in Python](/design-patterns/adapter/python/example)

## **Bridge** in Other Languages

[![Bridge in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/bridge/csharp/example "Bridge in C#") [![Bridge in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/bridge/cpp/example "Bridge in C++") [![Bridge in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/bridge/go/example "Bridge in Go") [![Bridge in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/bridge/java/example "Bridge in Java") [![Bridge in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/bridge/php/example "Bridge in PHP") [![Bridge in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/bridge/ruby/example "Bridge in Ruby") [![Bridge in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/bridge/rust/example "Bridge in Rust") [![Bridge in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/bridge/swift/example "Bridge in Swift") [![Bridge in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/bridge/typescript/example "Bridge in TypeScript")

![An illustration showing various digital screens and development tools, including a tablet, smartphone, and desktop interface, suggesting a software development environment or cross-platform application development.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [main](#example-0--main-py)

 [Output](#example-0--Output-txt)