```yaml
---
title: Composite in Python / Design Patterns
source: https://refactoring.guru/design-patterns/composite/python/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:57:46.784Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Python]
programming_languages: [Python]
tags: [design-patterns, composite-pattern, python, structural-pattern, object-oriented-programming, tree-structure, recursion]
key_concepts: [composite-design-pattern, tree-structures, component-hierarchy, leaf-component, composite-component, recursive-operations, abstraction]
code_examples: false
difficulty_level: intermediate
summary: |
  The article introduces the Composite design pattern, a structural pattern that enables composing objects into tree structures and treating individual objects and compositions uniformly. It highlights the pattern's utility for recursive operations over entire tree hierarchies, summing up results. A detailed conceptual example in Python demonstrates the `Component`, `Leaf`, and `Composite` classes, showcasing how to build and interact with these structures via a common interface. The pattern is commonly used for representing hierarchical data like UI components or graphs, allowing client code to work with complex and simple objects interchangeably.
---
```

# Composite in Python / Design Patterns

![Composite Pattern Icon](/images/patterns/cards/composite-mini.png?id=a369d98d18b417f255d04568fd0131b8)

# **Composite** in Python

**Composite** is a structural design pattern that lets you compose objects into tree structures and then work with these structures as if they were individual objects.

Composite became a pretty popular solution for the most problems that require building a tree structure. Composite’s great feature is the ability to run methods recursively over the whole tree structure and sum up the results.

**Complexity:** Medium
**Popularity:** High

**Usage examples:** The Composite pattern is pretty common in Python code. It’s often used to represent hierarchies of user interface components or the code that works with graphs.

**Identification:** If you have an object tree, and each object of a tree is a part of the same class hierarchy, this is most likely a composite. If methods of these classes delegate the work to child objects of the tree and do it via the base class/interface of the hierarchy, this is definitely a composite.

## Conceptual Example

This example illustrates the structure of the **Composite** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### **main.py:** Conceptual example

```python
from __future__ import annotations
from abc import ABC, abstractmethod
from typing import List

class Component(ABC):
    """
    The base Component class declares common operations for both simple and
    complex objects of a composition.
    """

    @property
    def parent(self) -> Component:
        return self._parent

    @parent.setter
    def parent(self, parent: Component):
        """
        Optionally, the base Component can declare an interface for setting and
        accessing a parent of the component in a tree structure. It can also
        provide some default implementation for these methods.
        """

        self._parent = parent

    """
    In some cases, it would be beneficial to define the child-management
    operations right in the base Component class. This way, you won't need to
    expose any concrete component classes to the client code, even during the
    object tree assembly. The downside is that these methods will be empty for
    the leaf-level components.
    """

    def add(self, component: Component) -> None:
        pass

    def remove(self, component: Component) -> None:
        pass

    def is_composite(self) -> bool:
        """
        You can provide a method that lets the client code figure out whether a
        component can bear children.
        """

        return False

    @abstractmethod
    def operation(self) -> str:
        """
        The base Component may implement some default behavior or leave it to
        concrete classes (by declaring the method containing the behavior as
        "abstract").
        """

        pass

class Leaf(Component):
    """
    The Leaf class represents the end objects of a composition. A leaf can't
    have any children.

    Usually, it's the Leaf objects that do the actual work, whereas Composite
    objects only delegate to their sub-components.
    """

    def operation(self) -> str:
        return "Leaf"

class Composite(Component):
    """
    The Composite class represents the complex components that may have
    children. Usually, the Composite objects delegate the actual work to their
    children and then "sum-up" the result.
    """

    def __init__(self) -> None:
        self._children: List[Component] = []

    """
    A composite object can add or remove other components (both simple or
    complex) to or from its child list.
    """

    def add(self, component: Component) -> None:
        self._children.append(component)
        component.parent = self

    def remove(self, component: Component) -> None:
        self._children.remove(component)
        component.parent = None

    def is_composite(self) -> bool:
        return True

    def operation(self) -> str:
        """
        The Composite executes its primary logic in a particular way. It
        traverses recursively through all its children, collecting and summing
        their results. Since the composite's children pass these calls to their
        children and so forth, the whole object tree is traversed as a result.
        """

        results = []
        for child in self._children:
            results.append(child.operation())
        return f"Branch({'+'.join(results)})"

def client_code(component: Component) -> None:
    """
    The client code works with all of the components via the base interface.
    """

    print(f"RESULT: {component.operation()}", end="")

def client_code2(component1: Component, component2: Component) -> None:
    """
    Thanks to the fact that the child-management operations are declared in the
    base Component class, the client code can work with any component, simple or
    complex, without depending on their concrete classes.
    """

    if component1.is_composite():
        component1.add(component2)

    print(f"RESULT: {component1.operation()}", end="")

if __name__ == "__main__":
    # This way the client code can support the simple leaf components...
    simple = Leaf()
    print("Client: I've got a simple component:")
    client_code(simple)
    print("\n")

    # ...as well as the complex composites.
    tree = Composite()

    branch1 = Composite()
    branch1.add(Leaf())
    branch1.add(Leaf())

    branch2 = Composite()
    branch2.add(Leaf())

    tree.add(branch1)
    tree.add(branch2)

    print("Client: Now I've got a composite tree:")
    client_code(tree)
    print("\n")

    print("Client: I don't need to check the components classes even when managing the tree:")
    client_code2(tree, simple)
```

#### **Output.txt:** Execution result

```
Client: I've got a simple component:
RESULT: Leaf

Client: Now I've got a composite tree:
RESULT: Branch(Branch(Leaf+Leaf)+Branch(Leaf))

Client: I don't need to check the components classes even when managing the tree:
RESULT: Branch(Branch(Leaf+Leaf)+Branch(Leaf)+Leaf)
```

#### Read next

[Decorator in Python](/design-patterns/decorator/python/example)

#### Return

[Bridge in Python](/design-patterns/bridge/python/example)

## **Composite** in Other Languages

[![Composite in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58 "Composite in C#")](/design-patterns/composite/csharp/example) [![Composite in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858 "Composite in C++")](/design-patterns/composite/cpp/example) [![Composite in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca "Composite in Go")](/design-patterns/composite/go/example) [![Composite in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e "Composite in Java")](/design-patterns/composite/java/example) [![Composite in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618 "Composite in PHP")](/design-patterns/composite/php/example) [![Composite in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb "Composite in Ruby")](/design-patterns/composite/ruby/example) [![Composite in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87 "Composite in Rust")](/design-patterns/composite/rust/example) [![Composite in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d "Composite in Swift")](/design-patterns/composite/swift/example) [![Composite in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7 "Composite in TypeScript")](/design-patterns/composite/typescript/example)

![Illustration of various software development tools and concepts](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)