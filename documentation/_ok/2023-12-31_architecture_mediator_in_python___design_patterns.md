```yaml
---
title: Mediator in Python / Design Patterns
source: https://refactoring.guru/design-patterns/mediator/python/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:56:02.081Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: []
programming_languages: [Python]
tags: [design-patterns, behavioral-patterns, mediator, python, software-design, decoupling, object-oriented, communication, oop]
key_concepts: [mediator-pattern, behavioral-design-patterns, loose-coupling, indirect-communication, mvc-pattern, component-interaction, abstraction, object-oriented-programming]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Mediator behavioral design pattern, explaining how it reduces coupling between software components by centralizing their communication through a dedicated mediator object. It highlights the pattern's benefits, such as easier modification, extension, and reuse of individual components. The content provides a conceptual Python example, demonstrating the core classes—Mediator, ConcreteMediator, BaseComponent, and Concrete Components—and their roles in facilitating indirect communication. The example illustrates how components notify the mediator, which then orchestrates interactions with other components, showcasing the pattern's structure and behavior. The article also briefly mentions common usage scenarios, such as facilitating communication in GUI applications, and notes its relation to the Controller part of the MVC pattern.]
---
```

# Mediator in Python / Design Patterns

/ [Design Patterns](/design-patterns) / [Mediator](/design-patterns/mediator) / [Python](/design-patterns/python)

![Mediator Pattern Icon](/images/patterns/cards/mediator-mini.png?id=a7e43ee8e17e4474737b1fcb3201d7ba)
*A small icon representing the Mediator design pattern, showing interconnected components around a central hub.*

# Mediator in Python

**Mediator** is a behavioral design pattern that reduces coupling between components of a program by making them communicate indirectly, through a special mediator object.

The Mediator makes it easy to modify, extend and reuse individual components because they’re no longer dependent on the dozens of other classes.

[Learn more about Mediator](/design-patterns/mediator)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [main](#example-0--main-py)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The most popular usage of the Mediator pattern in Python code is facilitating communications between GUI components of an app. The synonym of the Mediator is the Controller part of MVC pattern.

## Conceptual Example

This example illustrates the structure of the **Mediator** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### main.py: Conceptual example

```python
from __future__ import annotations
from abc import ABC

class Mediator(ABC):
    """
    The Mediator interface declares a method used by components to notify the
    mediator about various events. The Mediator may react to these events and
    pass the execution to other components.
    """

    def notify(self, sender: object, event: str) -> None:
        pass

class ConcreteMediator(Mediator):
    def __init__(self, component1: "Component1", component2: "Component2") -> None:
        self._component1 = component1
        self._component1.mediator = self
        self._component2 = component2
        self._component2.mediator = self

    def notify(self, sender: object, event: str) -> None:
        if event == "A":
            print("Mediator reacts on A and triggers following operations:")
            self._component2.do_c()
        elif event == "D":
            print("Mediator reacts on D and triggers following operations:")
            self._component1.do_b()
            self._component2.do_c()

class BaseComponent:
    """
    The Base Component provides the basic functionality of storing a mediator's
    instance inside component objects.
    """

    def __init__(self, mediator: Mediator = None) -> None:
        self._mediator = mediator

    @property
    def mediator(self) -> Mediator:
        return self._mediator

    @mediator.setter
    def mediator(self, mediator: Mediator) -> None:
        self._mediator = mediator

"""
Concrete Components implement various functionality. They don't depend on other
components. They also don't depend on any concrete mediator classes.
"""

class Component1(BaseComponent):
    def do_a(self) -> None:
        print("Component 1 does A.")
        self.mediator.notify(self, "A")

    def do_b(self) -> None:
        print("Component 1 does B.")
        self.mediator.notify(self, "B")

class Component2(BaseComponent):
    def do_c(self) -> None:
        print("Component 2 does C.")
        self.mediator.notify(self, "C")

    def do_d(self) -> None:
        print("Component 2 does D.")
        self.mediator.notify(self, "D")

if __name__ == "__main__":
    # The client code.
    c1 = Component1()
    c2 = Component2()
    mediator = ConcreteMediator(c1, c2)

    print("Client triggers operation A.")
    c1.do_a()

    print("\n", end="")

    print("Client triggers operation D.")
    c2.do_d()
```

#### Output.txt: Execution result

```text
Client triggers operation A.
Component 1 does A.
Mediator reacts on A and triggers following operations:
Component 2 does C.


Client triggers operation D.
Component 2 does D.
Mediator reacts on D and triggers following operations:
Component 1 does B.
Component 2 does C.
```

#### Read next

[Memento in Python](/design-patterns/memento/python/example)

#### Return

 [Iterator in Python](/design-patterns/iterator/python/example)

## Mediator in Other Languages

![C# Language Icon](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58) ![C++ Language Icon](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858) ![Go Language Icon](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca) ![Java Language Icon](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e) ![PHP Language Icon](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618) ![Ruby Language Icon](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb) ![Rust Language Icon](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87) ![Swift Language Icon](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d) ![TypeScript Language Icon](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)
*These are small SVG icons representing various programming languages where the Mediator pattern can also be implemented.*

[![Dive Into Design Patterns eBook Banner](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)](/design-patterns/book)
*A banner promoting an eBook "Dive Into Design Patterns," showing various devices (desktop, tablet, phone) displaying code and UI elements, surrounded by development tools, suggesting a comprehensive resource for software development.*

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [main](#example-0--main-py)

 [Output](#example-0--Output-txt)