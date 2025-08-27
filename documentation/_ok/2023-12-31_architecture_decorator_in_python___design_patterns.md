```yaml
---
title: Decorator in Python / Design Patterns
source: https://refactoring.guru/design-patterns/decorator/python/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:57:16.133Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Python]
programming_languages: [Python]
tags: [design-patterns, decorator, python, structural-pattern, object-oriented-programming, software-design, code-structure, wrapper, dynamic-behavior]
key_concepts: [decorator-pattern, structural-pattern, object-composition, dynamic-behavior, wrapper-objects, component-interface, inheritance, polymorphism]
code_examples: false
difficulty_level: intermediate
summary: |
  The article provides a detailed explanation of the Decorator design pattern, a structural pattern, specifically implemented in Python. It describes how this pattern allows for the dynamic addition of new behaviors to objects by wrapping them in special decorator objects, enabling a stacking of functionalities while maintaining a consistent interface. A comprehensive conceptual example in Python illustrates the pattern's core components: `Component`, `ConcreteComponent`, `Decorator`, and `ConcreteDecorator`. The content highlights the pattern's utility in Python, particularly in stream-related code, and offers links to its implementation in various other programming languages.
---
```

# Decorator in Python / Design Patterns

# **Decorator** in Python

**Decorator** is a structural pattern that allows adding new behaviors to objects dynamically by placing them inside special wrapper objects, called _decorators_.

Using decorators you can wrap objects countless number of times since both target objects and decorators follow the same interface. The resulting object will get a stacking behavior of all wrappers.

[Learn more about Decorator](/design-patterns/decorator)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [main](#example-0--main-py)

 [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The Decorator is pretty standard in Python code, especially in code related to streams.

**Identification:** Decorator can be recognized by creation methods or constructors that accept objects of the same class or interface as a current class.

## Conceptual Example

This example illustrates the structure of the **Decorator** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### **main.py:** Conceptual example

```python
class Component():
    """
    The base Component interface defines operations that can be altered by
    decorators.
    """

    def operation(self) -> str:
        pass


class ConcreteComponent(Component):
    """
    Concrete Components provide default implementations of the operations. There
    might be several variations of these classes.
    """

    def operation(self) -> str:
        return "ConcreteComponent"


class Decorator(Component):
    """
    The base Decorator class follows the same interface as the other components.
    The primary purpose of this class is to define the wrapping interface for
    all concrete decorators. The default implementation of the wrapping code
    might include a field for storing a wrapped component and the means to
    initialize it.
    """

    _component: Component = None

    def __init__(self, component: Component) -> None:
        self._component = component

    @property
    def component(self) -> Component:
        """
        The Decorator delegates all work to the wrapped component.
        """

        return self._component

    def operation(self) -> str:
        return self._component.operation()


class ConcreteDecoratorA(Decorator):
    """
    Concrete Decorators call the wrapped object and alter its result in some
    way.
    """

    def operation(self) -> str:
        """
        Decorators may call parent implementation of the operation, instead of
        calling the wrapped object directly. This approach simplifies extension
        of decorator classes.
        """
        return f"ConcreteDecoratorA({self.component.operation()})"


class ConcreteDecoratorB(Decorator):
    """
    Decorators can execute their behavior either before or after the call to a
    wrapped object.
    """

    def operation(self) -> str:
        return f"ConcreteDecoratorB({self.component.operation()})"


def client_code(component: Component) -> None:
    """
    The client code works with all objects using the Component interface. This
    way it can stay independent of the concrete classes of components it works
    with.
    """

    # ...

    print(f"RESULT: {component.operation()}", end="")

    # ...


if __name__ == "__main__":
    # This way the client code can support both simple components...
    simple = ConcreteComponent()
    print("Client: I've got a simple component:")
    client_code(simple)
    print("\n")

    # ...as well as decorated ones.
    #
    # Note how decorators can wrap not only simple components but the other
    # decorators as well.
    decorator1 = ConcreteDecoratorA(simple)
    decorator2 = ConcreteDecoratorB(decorator1)
    print("Client: Now I've got a decorated component:")
    client_code(decorator2)
```

#### **Output.txt:** Execution result

```
Client: I've got a simple component:
RESULT: ConcreteComponent

Client: Now I've got a decorated component:
RESULT: ConcreteDecoratorB(ConcreteDecoratorA(ConcreteComponent))
```

#### Read next

[Facade in Python](/design-patterns/facade/python/example) 

#### Return

 [Composite in Python](/design-patterns/composite/python/example)

## **Decorator** in Other Languages

[![Decorator in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/decorator/csharp/example "Decorator in C#") [![Decorator in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/decorator/cpp/example "Decorator in C++") [![Decorator in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/decorator/go/example "Decorator in Go") [![Decorator in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/decorator/java/example "Decorator in Java") [![Decorator in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/decorator/php/example "Decorator in PHP") [![Decorator in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/decorator/ruby/example "Decorator in Ruby") [![Decorator in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/decorator/rust/example "Decorator in Rust") [![Decorator in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/decorator/swift/example "Decorator in Swift") [![Decorator in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/decorator/typescript/example "Decorator in TypeScript")

![A detailed illustration of a desktop computer, a tablet, and a smartphone displaying various code snippets, charts, and user interfaces. Surrounding these devices are abstract representations of development tools like a wrench, a pen, and code tags, suggesting a comprehensive software development environment.](images/patterns/banners/examples-ide.png)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [main](#example-0--main-py)

 [Output](#example-0--Output-txt)