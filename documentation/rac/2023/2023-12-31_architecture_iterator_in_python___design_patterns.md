```yaml
---
title: Iterator in Python / Design Patterns
source: https://refactoring.guru/design-patterns/iterator/python/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:54:00.330Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [collections.abc]
programming_languages: [Python, C#, C++, Go, Java, PHP, Ruby, Rust, Swift, TypeScript]
tags: [design-patterns, behavioral-patterns, iterator, python, data-structures, collections, traversal, object-oriented-programming]
key_concepts: [iterator-pattern, behavioral-design-pattern, sequential-traversal, iterable-interface, collection-interface, __iter__ method, __next__ method, StopIteration exception]
code_examples: false
difficulty_level: intermediate
summary: |
  [The Iterator design pattern is a behavioral pattern that enables sequential traversal through complex data structures without exposing their internal details. This article explains the Iterator pattern in Python, demonstrating how clients can iterate over different collections uniformly using a single interface. It provides a conceptual example with Python code, illustrating the roles of Concrete Iterators and Concrete Collections, and how they implement the `__iter__` and `__next__` methods. The example showcases both straight and reverse traversal of a custom collection of words. The article also briefly mentions the pattern's applicability in other programming languages.]
---
```

# Iterator in Python / Design Patterns

![Iterator pattern icon: A red stick figure with a backpack and walking stick traverses a series of linked boxes.](/images/patterns/cards/iterator-mini.png)

# **Iterator** in Python

**Iterator** is a behavioral design pattern that allows sequential traversal through a complex data structure without exposing its internal details.

Thanks to the Iterator, clients can go over elements of different collections in a similar fashion using a single iterator interface.

[Learn more about Iterator](/design-patterns/iterator)

## Navigation

*   [Intro](#)
*   [Conceptual Example](#example-0)
*   [main.py](#example-0--main-py)
*   [Output](#example-0--Output-txt)

**Complexity:**

**Popularity:**

**Usage examples:** The pattern is very common in Python code. Many frameworks and libraries use it to provide a standard way for traversing their collections.

**Identification:** Iterator is easy to recognize by the navigation methods (such as `next`, `previous` and others). Client code that uses iterators might not have direct access to the collection being traversed.

## Conceptual Example

This example illustrates the structure of the **Iterator** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

### **main.py:** Conceptual example

```python
from __future__ import annotations
from collections.abc import Iterable, Iterator
from typing import Any


"""
To create an iterator in Python, there are two abstract classes from the built-
in `collections` module - Iterable, Iterator. We need to implement the
`__iter__()` method in the iterated object (collection), and the `__next__ ()`
method in the iterator.
"""


class AlphabeticalOrderIterator(Iterator):
    """
    Concrete Iterators implement various traversal algorithms. These classes
    store the current traversal position at all times.
    """

    """
    `_position` attribute stores the current traversal position. An iterator may
    have a lot of other fields for storing iteration state, especially when it
    is supposed to work with a particular kind of collection.
    """
    _position: int = None

    """
    This attribute indicates the traversal direction.
    """
    _reverse: bool = False

    def __init__(self, collection: WordsCollection, reverse: bool = False) -> None:
        self._collection = collection
        self._reverse = reverse
        self._sorted_items = None  # Will be set on first __next__ call
        self._position = 0

    def __next__(self) -> Any:
        """
        Optimization: sorting happens only when the first items is actually
        requested.
        """
        if self._sorted_items is None:
            self._sorted_items = sorted(self._collection._collection)
            if self._reverse:
                self._sorted_items = list(reversed(self._sorted_items))

        """
        The __next__() method must return the next item in the sequence. On
        reaching the end, and in subsequent calls, it must raise StopIteration.
        """
        if self._position >= len(self._sorted_items):
            raise StopIteration()
        value = self._sorted_items[self._position]
        self._position += 1
        return value


class WordsCollection(Iterable):
    """
    Concrete Collections provide one or several methods for retrieving fresh
    iterator instances, compatible with the collection class.
    """

    def __init__(self, collection: list[Any] | None = None) -> None:
        self._collection = collection or []


    def __getitem__(self, index: int) -> Any:
        return self._collection[index]

    def __iter__(self) -> AlphabeticalOrderIterator:
        """
        The __iter__() method returns the iterator object itself, by default we
        return the iterator in ascending order.
        """
        return AlphabeticalOrderIterator(self)

    def get_reverse_iterator(self) -> AlphabeticalOrderIterator:
        return AlphabeticalOrderIterator(self, True)

    def add_item(self, item: Any) -> None:
        self._collection.append(item)


if __name__ == "__main__":
    # The client code may or may not know about the Concrete Iterator or
    # Collection classes, depending on the level of indirection you want to keep
    # in your program.
    collection = WordsCollection()
    collection.add_item("B")
    collection.add_item("A")
    collection.add_item("C")

    print("Straight traversal:")
    print("\n".join(collection))
    print("")

    print("Reverse traversal:")
    print("\n".join(collection.get_reverse_iterator()), end="")
```

### **Output.txt:** Execution result

```text
Straight traversal:
A
B
C

Reverse traversal:
C
B
A
```

### Read next

[Mediator in Python](/design-patterns/mediator/python/example)

### Return

[Command in Python](/design-patterns/command/python/example)

## **Iterator** in Other Languages

![C# icon](/images/patterns/icons/csharp.svg) ![C++ icon](/images/patterns/icons/cpp.svg) ![Go icon](/images/patterns/icons/go.svg) ![Java icon](/images/patterns/icons/java.svg) ![PHP icon](/images/patterns/icons/php.svg) ![Ruby icon](/images/patterns/icons/ruby.svg) ![Rust icon](/images/patterns/icons/rust.svg) ![Swift icon](/images/patterns/icons/swift.svg) ![TypeScript icon](/images/patterns/icons/typescript.svg)

![Illustration of development tools and screens, promoting an e-book with code examples.](/images/patterns/banners/examples-ide.png)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

## Navigation

*   [Intro](#)
*   [Conceptual Example](#example-0)
*   [main.py](#example-0--main-py)
*   [Output](#example-0--Output-txt)