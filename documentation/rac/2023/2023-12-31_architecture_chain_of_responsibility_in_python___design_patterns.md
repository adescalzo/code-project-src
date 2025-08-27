```yaml
---
title: Chain of Responsibility in Python / Design Patterns
source: https://refactoring.guru/design-patterns/chain-of-responsibility/python/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:56:10.170Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Python]
programming_languages: [Python]
tags: [design-patterns, behavioral-patterns, chain-of-responsibility, object-oriented-programming, software-design, handler, request-handling]
key_concepts: [chain-of-responsibility-pattern, handler-interface, abstract-classes, request-delegation, dynamic-chain-composition]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article explains the Chain of Responsibility behavioral design pattern, which allows requests to be passed along a chain of potential handlers until one processes it. It details how the pattern decouples the sender from concrete receiver classes and enables dynamic chain composition. A conceptual example implemented in Python illustrates the Handler interface, abstract handler, and concrete handlers, demonstrating how requests like "Banana" or "Nut" are handled by specific objects in the chain. The provided code showcases the structure and interaction of the pattern's components.]
---
```

# Chain of Responsibility in Python / Design Patterns

# **Chain of Responsibility** in Python

**Chain of Responsibility** is behavioral design pattern that allows passing request along the chain of potential handlers until one of them handles request.

The pattern allows multiple objects to handle the request without coupling sender class to the concrete classes of the receivers. The chain can be composed dynamically at runtime with any handler that follows a standard handler interface.

**Usage examples:** The Chain of Responsibility is pretty common in Python. It’s mostly relevant when your code operates with chains of objects, such as filters, event chains, etc.

**Identification:** The pattern is recognizable by behavioral methods of one group of objects that indirectly call the same methods in other objects, while all the objects follow the common interface.

## Conceptual Example

This example illustrates the structure of the **Chain of Responsibility** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### **main.py:** Conceptual example

```python
from __future__ import annotations
from abc import ABC, abstractmethod
from typing import Any, Optional


class Handler(ABC):
    """
    The Handler interface declares a method for building the chain of handlers.
    It also declares a method for executing a request.
    """

    @abstractmethod
    def set_next(self, handler: Handler) -> Handler:
        pass

    @abstractmethod
    def handle(self, request) -> Optional[str]:
        pass


class AbstractHandler(Handler):
    """
    The default chaining behavior can be implemented inside a base handler
    class.
    """

    _next_handler: Handler = None

    def set_next(self, handler: Handler) -> Handler:
        self._next_handler = handler
        # Returning a handler from here will let us link handlers in a
        # convenient way like this:
        # monkey.set_next(squirrel).set_next(dog)
        return handler

    @abstractmethod
    def handle(self, request: Any) -> str:
        if self._next_handler:
            return self._next_handler.handle(request)

        return None


"""
All Concrete Handlers either handle a request or pass it to the next handler in
the chain.
"""


class MonkeyHandler(AbstractHandler):
    def handle(self, request: Any) -> str:
        if request == "Banana":
            return f"Monkey: I'll eat the {request}"
        else:
            return super().handle(request)


class SquirrelHandler(AbstractHandler):
    def handle(self, request: Any) -> str:
        if request == "Nut":
            return f"Squirrel: I'll eat the {request}"
        else:
            return super().handle(request)


class DogHandler(AbstractHandler):
    def handle(self, request: Any) -> str:
        if request == "MeatBall":
            return f"Dog: I'll eat the {request}"
        else:
            return super().handle(request)


def client_code(handler: Handler) -> None:
    """
    The client code is usually suited to work with a single handler. In most
    cases, it is not even aware that the handler is part of a chain.
    """

    for food in ["Nut", "Banana", "Cup of coffee"]:
        print(f"\nClient: Who wants a {food}?")
        result = handler.handle(food)
        if result:
            print(f"  {result}", end="")
        else:
            print(f"  {food} was left untouched.", end="")


if __name__ == "__main__":
    monkey = MonkeyHandler()
    squirrel = SquirrelHandler()
    dog = DogHandler()

    monkey.set_next(squirrel).set_next(dog)

    # The client should be able to send a request to any handler, not just the
    # first one in the chain.
    print("Chain: Monkey > Squirrel > Dog")
    client_code(monkey)
    print("\n")

    print("Subchain: Squirrel > Dog")
    client_code(squirrel)
```

#### **Output.txt:** Execution result

```
Chain: Monkey > Squirrel > Dog

Client: Who wants a Nut?
  Squirrel: I'll eat the Nut
Client: Who wants a Banana?
  Monkey: I'll eat the Banana
Client: Who wants a Cup of coffee?
  Cup of coffee was left untouched.

Subchain: Squirrel > Dog

Client: Who wants a Nut?
  Squirrel: I'll eat the Nut
Client: Who wants a Banana?
  Banana was left untouched.
Client: Who wants a Cup of coffee?
  Cup of coffee was left untouched.
```

![Software Development Environment Illustration](https://i.imgur.com/example.png)
Description: This image depicts a stylized software development environment, featuring a central desktop monitor displaying a user interface with charts and code snippets. Surrounding it are a tablet and a smartphone showing similar interfaces, alongside various tools like a wrench, a pen, and abstract representations of data and code, symbolizing the multi-platform nature and various aspects of software design and development.