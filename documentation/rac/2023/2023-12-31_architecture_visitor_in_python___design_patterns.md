```yaml
---
title: Visitor in Python / Design Patterns
source: https://refactoring.guru/design-patterns/visitor/python/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:46:26.246Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Python Standard Library]
programming_languages: [Python]
tags: [design-patterns, visitor-pattern, behavioral-pattern, python, object-oriented-programming, extensibility, software-design, code-structure]
key_concepts: [Visitor pattern, behavioral design pattern, double dispatch, class hierarchy, extensibility, separation of concerns, object-oriented design, polymorphism]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Visitor design pattern, a behavioral pattern that enables adding new operations to existing class hierarchies without modifying their code. It explains the pattern's structure, including the Component, Concrete Component, Visitor, and Concrete Visitor roles. A detailed conceptual example implemented in Python demonstrates how the pattern works, allowing new behaviors to be defined in separate visitor classes. The content also briefly discusses the pattern's complexity and applicability, and links to implementations in other programming languages.]
---
```

# Visitor in Python / Design Patterns

# **Visitor** in Python

**Visitor** is a behavioral design pattern that allows adding new behaviors to existing class hierarchy without altering any existing code.

> Read why Visitors can’t be simply replaced with method overloading in our article [Visitor and Double Dispatch](/design-patterns/visitor-double-dispatch).

**Complexity:**

**Popularity:**

**Usage examples:** Visitor isn’t a very common pattern because of its complexity and narrow applicability.

## Conceptual Example

This example illustrates the structure of the **Visitor** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

### main.py: Conceptual example

```python
from __future__ import annotations
from abc import ABC, abstractmethod
from typing import List


class Component(ABC):
    """
    The Component interface declares an `accept` method that should take the
    base visitor interface as an argument.
    """

    @abstractmethod
    def accept(self, visitor: Visitor) -> None:
        pass


class ConcreteComponentA(Component):
    """
    Each Concrete Component must implement the `accept` method in such a way
    that it calls the visitor's method corresponding to the component's class.
    """

    def accept(self, visitor: Visitor) -> None:
        """
        Note that we're calling `visitConcreteComponentA`, which matches the
        current class name. This way we let the visitor know the class of the
        component it works with.
        """

        visitor.visit_concrete_component_a(self)

    def exclusive_method_of_concrete_component_a(self) -> str:
        """
        Concrete Components may have special methods that don't exist in their
        base class or interface. The Visitor is still able to use these methods
        since it's aware of the component's concrete class.
        """

        return "A"


class ConcreteComponentB(Component):
    """
    Same here: visitConcreteComponentB => ConcreteComponentB
    """

    def accept(self, visitor: Visitor):
        visitor.visit_concrete_component_b(self)

    def special_method_of_concrete_component_b(self) -> str:
        return "B"


class Visitor(ABC):
    """
    The Visitor Interface declares a set of visiting methods that correspond to
    component classes. The signature of a visiting method allows the visitor to
    identify the exact class of the component that it's dealing with.
    """

    @abstractmethod
    def visit_concrete_component_a(self, element: ConcreteComponentA) -> None:
        pass

    @abstractmethod
    def visit_concrete_component_b(self, element: ConcreteComponentB) -> None:
        pass


"""
Concrete Visitors implement several versions of the same algorithm, which can
work with all concrete component classes.

You can experience the biggest benefit of the Visitor pattern when using it with
a complex object structure, such as a Composite tree. In this case, it might be
helpful to store some intermediate state of the algorithm while executing
visitor's methods over various objects of the structure.
"""


class ConcreteVisitor1(Visitor):
    def visit_concrete_component_a(self, element) -> None:
        print(f"{element.exclusive_method_of_concrete_component_a()} + ConcreteVisitor1")

    def visit_concrete_component_b(self, element) -> None:
        print(f"{element.special_method_of_concrete_component_b()} + ConcreteVisitor1")


class ConcreteVisitor2(Visitor):
    def visit_concrete_component_a(self, element) -> None:
        print(f"{element.exclusive_method_of_concrete_component_a()} + ConcreteVisitor2")

    def visit_concrete_component_b(self, element) -> None:
        print(f"{element.special_method_of_concrete_component_b()} + ConcreteVisitor2")


def client_code(components: List[Component], visitor: Visitor) -> None:
    """
    The client code can run visitor operations over any set of elements without
    figuring out their concrete classes. The accept operation directs a call to
    the appropriate operation in the visitor object.
    """

    # ...
    for component in components:
        component.accept(visitor)
    # ...


if __name__ == "__main__":
    components = [ConcreteComponentA(), ConcreteComponentB()]

    print("The client code works with all visitors via the base Visitor interface:")
    visitor1 = ConcreteVisitor1()
    client_code(components, visitor1)

    print("It allows the same client code to work with different types of visitors:")
    visitor2 = ConcreteVisitor2()
    client_code(components, visitor2)
```

### Output.txt: Execution result

```text
The client code works with all visitors via the base Visitor interface:
A + ConcreteVisitor1
B + ConcreteVisitor1
It allows the same client code to work with different types of visitors:
A + ConcreteVisitor2
B + ConcreteVisitor2
```

#### Read next

[Design Patterns in Ruby](/design-patterns/ruby)

#### Return

[Template Method in Python](/design-patterns/template-method/python/example)

## **Visitor** in Other Languages

[![Visitor in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/visitor/csharp/example "Visitor in C#") [![Visitor in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/visitor/cpp/example "Visitor in C++") [![Visitor in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/visitor/go/example "Visitor in Go") [![Visitor in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/visitor/java/example "Visitor in Java") [![Visitor in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/visitor/php/example "Visitor in PHP") [![Visitor in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/visitor/ruby/example "Visitor in Ruby") [![Visitor in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/visitor/rust/example "Visitor in Rust") [![Visitor in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/visitor/swift/example "Visitor in Swift") [![Visitor in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/visitor/typescript/example "Visitor in TypeScript")

[![](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)](/design-patterns/book)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

**Image Analysis:**
1.  **Image 1 (visitor-mini.png):** This small icon visually represents the Visitor design pattern. It depicts a single red rectangular object (likely the "Visitor") on the left, with three red arrows pointing towards a hierarchy of three distinct, multi-layered objects on the right. These three objects are connected to a common abstract parent object at the top. This illustrates how a visitor can interact with and perform operations on different elements within an object structure without altering the elements themselves.
2.  **Image 2 (examples-ide.png):** This banner image promotes an eBook titled "Dive Into Design Patterns." It features a central tablet displaying code and UI elements, flanked by a desktop monitor and a smartphone, also showing development-related content. Surrounding these devices are various icons symbolizing software development tools and concepts, such as code tags (`</>`), a wrench, a hammer, gears, and charts. The overall composition suggests a comprehensive resource for learning design patterns with practical, IDE-ready examples.