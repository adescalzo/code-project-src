```yaml
---
title: State in Python / Design Patterns
source: https://refactoring.guru/design-patterns/state/python/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:46:15.320Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Python]
programming_languages: [Python]
tags: [design-patterns, state-pattern, behavioral-patterns, python, object-oriented-programming, software-design]
key_concepts: [state-pattern, behavioral-design-patterns, context-object, state-classes, delegation, object-oriented-design]
code_examples: false
difficulty_level: intermediate
summary: |
  [The State design pattern is a behavioral pattern that allows an object to alter its behavior when its internal state changes, making it appear as if the object has changed its class. This pattern extracts state-specific behaviors into separate state classes, and the original object, known as the Context, delegates its work to an instance of these state classes. This approach helps to convert large, switch-based state machines into a more organized, object-oriented structure. The article provides a conceptual example in Python, illustrating the Context, State, and Concrete State classes and their interactions. It also highlights how the pattern can be identified and its common usage in Python.]
---
```

# State in Python / Design Patterns

/ [Design Patterns](/design-patterns) / [State](/design-patterns/state) / [Python](/design-patterns/python)

![Diagram illustrating the State design pattern, showing a context object delegating behavior to different state objects, with transitions between states.](images/patterns/cards/state-mini.png)

# **State** in Python

**State** is a behavioral design pattern that allows an object to change the behavior when its internal state changes.

The pattern extracts state-related behaviors into separate state classes and forces the original object to delegate the work to an instance of these classes, instead of acting on its own.

[Learn more about State](/design-patterns/state)

Navigation

*   [Intro](#)
*   [Conceptual Example](#example-0)
*   [main](#example-0--main-py)
*   [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The State pattern is commonly used in Python to convert massive `switch`-base state machines into objects.

**Identification:** State pattern can be recognized by methods that change their behavior depending on the objects’ state, controlled externally.

## Conceptual Example

This example illustrates the structure of the **State** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### **main.py:** Conceptual example

```python
from __future__ import annotations
from abc import ABC, abstractmethod


class Context:
    """
    The Context defines the interface of interest to clients. It also maintains
    a reference to an instance of a State subclass, which represents the current
    state of the Context.
    """

    _state = None
    """
    A reference to the current state of the Context.
    """

    def __init__(self, state: State) -> None:
        self.transition_to(state)

    def transition_to(self, state: State):
        """
        The Context allows changing the State object at runtime.
        """

        print(f"Context: Transition to {type(state).__name__}")
        self._state = state
        self._state.context = self

    """
    The Context delegates part of its behavior to the current State object.
    """

    def request1(self):
        self._state.handle1()

    def request2(self):
        self._state.handle2()


class State(ABC):
    """
    The base State class declares methods that all Concrete State should
    implement and also provides a backreference to the Context object,
    associated with the State. This backreference can be used by States to
    transition the Context to another State.
    """

    @property
    def context(self) -> Context:
        return self._context

    @context.setter
    def context(self, context: Context) -> None:
        self._context = context

    @abstractmethod
    def handle1(self) -> None:
        pass

    @abstractmethod
    def handle2(self) -> None:
        pass


"""
Concrete States implement various behaviors, associated with a state of the
Context.
"""


class ConcreteStateA(State):
    def handle1(self) -> None:
        print("ConcreteStateA handles request1.")
        print("ConcreteStateA wants to change the state of the context.")
        self.context.transition_to(ConcreteStateB())

    def handle2(self) -> None:
        print("ConcreteStateA handles request2.")


class ConcreteStateB(State):
    def handle1(self) -> None:
        print("ConcreteStateB handles request1.")

    def handle2(self) -> None:
        print("ConcreteStateB handles request2.")
        print("ConcreteStateB wants to change the state of the context.")
        self.context.transition_to(ConcreteStateA())


if __name__ == "__main__":
    # The client code.

    context = Context(ConcreteStateA())
    context.request1()
    context.request2()
```

#### **Output.txt:** Execution result

```
Context: Transition to ConcreteStateA
ConcreteStateA handles request1.
ConcreteStateA wants to change the state of the context.
Context: Transition to ConcreteStateB
ConcreteStateB handles request2.
ConcreteStateB wants to change the state of the context.
Context: Transition to ConcreteStateA
```

#### Read next

[Strategy in Python](/design-patterns/strategy/python/example)

#### Return

[Observer in Python](/design-patterns/observer/python/example)

## **State** in Other Languages

[![State in C#](/images/patterns/icons/csharp.svg "State in C#")](/design-patterns/state/csharp/example) [![State in C++](/images/patterns/icons/cpp.svg "State in C++")](/design-patterns/state/cpp/example) [![State in Go](/images/patterns/icons/go.svg "State in Go")](/design-patterns/state/go/example) [![State in Java](/images/patterns/icons/java.svg "State in Java")](/design-patterns/state/java/example) [![State in PHP](/images/patterns/icons/php.svg "State in PHP")](/design-patterns/state/php/example) [![State in Ruby](/images/patterns/icons/ruby.svg "State in Ruby")](/design-patterns/state/ruby/example) [![State in Rust](/images/patterns/icons/rust.svg "State in Rust")](/design-patterns/state/rust/example) [![State in Swift](/images/patterns/icons/swift.svg "State in Swift")](/design-patterns/state/swift/example) [![State in TypeScript](/images/patterns/icons/typescript.svg "State in TypeScript")](/design-patterns/state/typescript/example)

![Promotional banner for an e-book titled "Dive Into Design Patterns", showing various development tools and screens, implying access to an archive of examples.](images/patterns/banners/examples-ide.png)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

*   [Intro](#)
*   [Conceptual Example](#example-0)
*   [main](#example-0--main-py)
*   [Output](#example-0--Output-txt)