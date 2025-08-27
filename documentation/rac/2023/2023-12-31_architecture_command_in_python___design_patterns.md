```yaml
---
title: Command in Python / Design Patterns
source: https://refactoring.guru/design-patterns/command/python/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:53:54.492Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Design Patterns, Python, C#, C++, Go, Java, PHP, Ruby, Rust, Swift, TypeScript]
programming_languages: [Python]
tags: [design-patterns, behavioral-patterns, command-pattern, python, object-oriented-programming, software-design, code-example, abstraction, encapsulation, callbacks]
key_concepts: [Command pattern, behavioral design pattern, Receiver, Invoker, Command interface, deferred execution, command history, callbacks]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explains the Command behavioral design pattern, which encapsulates requests or operations into objects. This pattern enables features like deferred or remote execution of commands and the ability to store command history. The article provides a detailed conceptual example implemented in Python, illustrating the roles of the Command interface, SimpleCommand, ComplexCommand, Receiver, and Invoker classes. It highlights how the Invoker interacts with commands indirectly, decoupling the sender from the receiver. The content also mentions common usage examples, such as parameterizing UI elements with actions and queueing tasks.
---
```

# Command in Python / Design Patterns

![A small icon representing the Command design pattern.](/images/patterns/cards/command-mini.png?id=b149eda017c0583c1e92343b83cfb1eb)

# **Command** in Python

**Command** is behavioral design pattern that converts requests or simple operations into objects.

The conversion allows deferred or remote execution of commands, storing command history, etc.

[Learn more about Command](/design-patterns/command)

## Navigation

*   [Intro](#)
*   [Conceptual Example](#example-0)
*   [main](#example-0--main-py)
*   [Output](#example-0--Output-txt)

**Usage examples:** The Command pattern is pretty common in Python code. Most often it’s used as an alternative for callbacks to parameterizing UI elements with actions. It’s also used for queueing tasks, tracking operations history, etc.

**Identification:** The Command pattern is recognizable by behavioral methods in an abstract/interface type (sender) which invokes a method in an implementation of a different abstract/interface type (receiver) which has been encapsulated by the command implementation during its creation. Command classes are usually limited to specific actions.

## Conceptual Example

This example illustrates the structure of the **Command** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

### **main.py:** Conceptual example

```python
from __future__ import annotations
from abc import ABC, abstractmethod


class Command(ABC):
    """
    The Command interface declares a method for executing a command.
    """

    @abstractmethod
    def execute(self) -> None:
        pass


class SimpleCommand(Command):
    """
    Some commands can implement simple operations on their own.
    """

    def __init__(self, payload: str) -> None:
        self._payload = payload

    def execute(self) -> None:
        print(f"SimpleCommand: See, I can do simple things like printing"
              f"({self._payload})")


class ComplexCommand(Command):
    """
    However, some commands can delegate more complex operations to other
    objects, called "receivers."
    """

    def __init__(self, receiver: Receiver, a: str, b: str) -> None:
        """
        Complex commands can accept one or several receiver objects along with
        any context data via the constructor.
        """

        self._receiver = receiver
        self._a = a
        self._b = b

    def execute(self) -> None:
        """
        Commands can delegate to any methods of a receiver.
        """

        print("ComplexCommand: Complex stuff should be done by a receiver object", end="")
        self._receiver.do_something(self._a)
        self._receiver.do_something_else(self._b)


class Receiver:
    """
    The Receiver classes contain some important business logic. They know how to
    perform all kinds of operations, associated with carrying out a request. In
    fact, any class may serve as a Receiver.
    """

    def do_something(self, a: str) -> None:
        print(f"\nReceiver: Working on ({a}.)", end="")

    def do_something_else(self, b: str) -> None:
        print(f"\nReceiver: Also working on ({b}.)", end="")


class Invoker:
    """
    The Invoker is associated with one or several commands. It sends a request
    to the command.
    """

    _on_start = None
    _on_finish = None

    """
    Initialize commands.
    """

    def set_on_start(self, command: Command):
        self._on_start = command

    def set_on_finish(self, command: Command):
        self._on_finish = command

    def do_something_important(self) -> None:
        """
        The Invoker does not depend on concrete command or receiver classes. The
        Invoker passes a request to a receiver indirectly, by executing a
        command.
        """

        print("Invoker: Does anybody want something done before I begin?")
        if isinstance(self._on_start, Command):
            self._on_start.execute()

        print("Invoker: ...doing something really important...")

        print("Invoker: Does anybody want something done after I finish?")
        if isinstance(self._on_finish, Command):
            self._on_finish.execute()


if __name__ == "__main__":
    """
    The client code can parameterize an invoker with any commands.
    """

    invoker = Invoker()
    invoker.set_on_start(SimpleCommand("Say Hi!"))
    receiver = Receiver()
    invoker.set_on_finish(ComplexCommand(
        receiver, "Send email", "Save report"))

    invoker.do_something_important()
```

### **Output.txt:** Execution result

```
Invoker: Does anybody want something done before I begin?
SimpleCommand: See, I can do simple things like printing (Say Hi!)
Invoker: ...doing something really important...
Invoker: Does anybody want something done after I finish?
ComplexCommand: Complex stuff should be done by a receiver object
Receiver: Working on (Send email.)
Receiver: Also working on (Save report.)
```

### Read next

[Iterator in Python](/design-patterns/iterator/python/example)

### Return

[Chain of Responsibility in Python](/design-patterns/chain-of-responsibility/python/example)

## **Command** in Other Languages

[![Command in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/command/csharp/example "Command in C#") [![Command in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/command/cpp/example "Command in C++") [![Command in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/command/go/example "Command in Go") [![Command in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/command/java/example "Command in Java") [![Command in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/command/php/example "Command in PHP") [![Command in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/command/ruby/example "Command in Ruby") [![Command in Rust](/images/patterns/icons/rust.svg?id=1f5698a4b5ae23fe79413511747e4a87)](/design-patterns/command/rust/example "Command in Rust") [![Command in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/command/swift/example "Command in Swift") [![Command in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/command/typescript/example "Command in TypeScript")

![An illustration depicting various digital devices (desktop, tablet, smartphone) surrounded by coding elements, development tools, and data visualizations, symbolizing software development and design.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)