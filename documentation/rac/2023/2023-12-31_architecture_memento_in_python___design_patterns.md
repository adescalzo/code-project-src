```yaml
---
title: Memento in Python / Design Patterns
source: https://refactoring.guru/design-patterns/memento/python/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:51:48.124Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Python, C#, C++, Go, Java, PHP, Ruby, Rust, Swift, TypeScript]
programming_languages: [Python]
tags: [memento-pattern, design-patterns, behavioral-patterns, python, object-state, snapshot, software-design, oop]
key_concepts: [memento-pattern, behavioral-design-pattern, object-state-management, originator, caretaker, memento-interface, encapsulation, undo-redo-functionality]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Memento design pattern, a behavioral pattern used to capture and restore an object's internal state without violating its encapsulation. It explains the pattern's structure, involving an Originator, Memento, and Caretaker, and provides a detailed conceptual example implemented in Python. The example demonstrates how to save an object's state, perform operations that modify it, and then restore a previous state using the Memento pattern. The article also briefly mentions the applicability of the Memento pattern across various other programming languages.]
---
```

# Memento in Python / Design Patterns

![Memento Design Pattern Icon](/images/patterns/cards/memento-mini.png?id=8b2ea4dc2c5d15775a654808cc9de099)

# Memento in Python

The Memento is a behavioral design pattern that allows making snapshots of an object’s state and restoring it in future.

The Memento doesn’t compromise the internal structure of the object it works with, as well as data kept inside the snapshots.

[Learn more about Memento](/design-patterns/memento)

**Usage examples:** The Memento’s principle can be achieved using serialization, which is quite common in Python. While it’s not the only and the most efficient way to make snapshots of an object’s state, it still allows storing state backups while protecting the originator’s structure from other objects.

## Conceptual Example

This example illustrates the structure of the **Memento** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### main.py: Conceptual example

```python
from __future__ import annotations
from abc import ABC, abstractmethod
from datetime import datetime
from random import sample
from string import ascii_letters


class Originator:
    """
    The Originator holds some important state that may change over time. It also
    defines a method for saving the state inside a memento and another method
    for restoring the state from it.
    """

    _state = None
    """
    For the sake of simplicity, the originator's state is stored inside a single
    variable.
    """

    def __init__(self, state: str) -> None:
        self._state = state
        print(f"Originator: My initial state is: {self._state}")

    def do_something(self) -> None:
        """
        The Originator's business logic may affect its internal state.
        Therefore, the client should backup the state before launching methods
        of the business logic via the save() method.
        """

        print("Originator: I'm doing something important.")
        self._state = self._generate_random_string(30)
        print(f"Originator: and my state has changed to: {self._state}")

    @staticmethod
    def _generate_random_string(length: int = 10) -> str:
        return "".join(sample(ascii_letters, length))

    def save(self) -> Memento:
        """
        Saves the current state inside a memento.
        """

        return ConcreteMemento(self._state)

    def restore(self, memento: Memento) -> None:
        """
        Restores the Originator's state from a memento object.
        """

        self._state = memento.get_state()
        print(f"Originator: My state has changed to: {self._state}")


class Memento(ABC):
    """
    The Memento interface provides a way to retrieve the memento's metadata,
    such as creation date or name. However, it doesn't expose the Originator's
    state.
    """

    @abstractmethod
    def get_name(self) -> str:
        pass

    @abstractmethod
    def get_date(self) -> str:
        pass


class ConcreteMemento(Memento):
    def __init__(self, state: str) -> None:
        self._state = state
        self._date = str(datetime.now())[:19]

    def get_state(self) -> str:
        """
        The Originator uses this method when restoring its state.
        """
        return self._state

    def get_name(self) -> str:
        """
        The rest of the methods are used by the Caretaker to display metadata.
        """

        return f"{self._date} / ({self._state[0:9]}...)"

    def get_date(self) -> str:
        return self._date


class Caretaker:
    """
    The Caretaker doesn't depend on the Concrete Memento class. Therefore, it
    doesn't have access to the originator's state, stored inside the memento. It
    works with all mementos via the base Memento interface.
    """

    def __init__(self, originator: Originator) -> None:
        self._mementos = []
        self._originator = originator

    def backup(self) -> None:
        print("\nCaretaker: Saving Originator's state...")
        self._mementos.append(self._originator.save())

    def undo(self) -> None:
        if not len(self._mementos):
            return

        memento = self._mementos.pop()
        print(f"Caretaker: Restoring state to: {memento.get_name()}")
        try:
            self._originator.restore(memento)
        except Exception:
            self.undo()

    def show_history(self) -> None:
        print("Caretaker: Here's the list of mementos:")
        for memento in self._mementos:
            print(memento.get_name())


if __name__ == "__main__":
    originator = Originator("Super-duper-super-puper-super.")
    caretaker = Caretaker(originator)

    caretaker.backup()
    originator.do_something()

    caretaker.backup()
    originator.do_something()

    caretaker.backup()
    originator.do_something()

    print()
    caretaker.show_history()

    print("\nClient: Now, let's rollback!\n")
    caretaker.undo()

    print("\nClient: Once more!\n")
    caretaker.undo()
```

#### Output.txt: Execution result

```
Originator: My initial state is: Super-duper-super-puper-super.

Caretaker: Saving Originator's state...
Originator: I'm doing something important.
Originator: and my state has changed to: wQAehHYOqVSlpEXjyIcgobrxsZUnat

Caretaker: Saving Originator's state...
Originator: I'm doing something important.
Originator: and my state has changed to: lHxNORKcsgMWYnJqoXjVCbQLEIeiSp

Caretaker: Saving Originator's state...
Originator: I'm doing something important.
Originator: and my state has changed to: cvIYsRilNOtwynaKdEZpDCQkFAXVMf

Caretaker: Here's the list of mementos:
2019-01-26 21:11:24 / (Super-dup...)
2019-01-26 21:11:24 / (wQAehHYOq...)
2019-01-26 21:11:24 / (lHxNORKcs...)

Client: Now, let's rollback!

Caretaker: Restoring state to: 2019-01-26 21:11:24 / (lHxNORKcs...)
Originator: My state has changed to: lHxNORKcsgMWYnJqoXjVCbQLEIeiSp

Client: Once more!

Caretaker: Restoring state to: 2019-01-26 21:11:24 / (wQAehHYOq...)
Originator: My state has changed to: wQAehHYOqVSlpEXjyIcgobrxsZUnat
```

![Abstract illustration of software development tools and interfaces, including a tablet displaying code, a mobile phone, various UI elements, data visualizations, and coding tools like a hammer and wrench.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

## Memento in Other Languages

[![Memento in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/memento/csharp/example "Memento in C#") [![Memento in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/memento/cpp/example "Memento in C++") [![Memento in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/memento/go/example "Memento in Go") [![Memento in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/memento/java/example "Memento in Java") [![Memento in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/memento/php/example "Memento in PHP") [![Memento in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/memento/ruby/example "Memento in Ruby") [![Memento in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/memento/rust/example "Memento in Rust") [![Memento in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/memento/swift/example "Memento in Swift") [![Memento in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/memento/typescript/example "Memento in TypeScript")