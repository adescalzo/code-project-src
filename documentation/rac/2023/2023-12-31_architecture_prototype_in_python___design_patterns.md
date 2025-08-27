```yaml
---
title: Prototype in Python / Design Patterns
source: https://refactoring.guru/design-patterns/prototype/python/example#lang-features
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:57:19.908Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Python copy module]
programming_languages: [Python]
tags: [design-patterns, creational-pattern, prototype-pattern, python, object-cloning, shallow-copy, deep-copy]
key_concepts: [Prototype pattern, creational design pattern, object cloning, shallow copy, deep copy, circular references, __copy__ method, __deepcopy__ method]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Prototype creational design pattern, which facilitates cloning objects without coupling to their specific classes. It explains how Python's built-in `copy` module supports this pattern, distinguishing between shallow and deep copies. A comprehensive conceptual example demonstrates overriding `__copy__` and `__deepcopy__` methods for custom cloning logic, including handling complex scenarios like circular references. The provided code illustrates the practical differences and implications of shallow versus deep copying on nested data structures and object relationships.]
---
```

# Prototype in Python / Design Patterns

![Prototype pattern icon](/images/patterns/cards/prototype-mini.png)

# **Prototype** in Python

**Prototype** is a creational design pattern that allows cloning objects, even complex ones, without coupling to their specific classes.

All prototype classes should have a common interface that makes it possible to copy objects even if their concrete classes are unknown. Prototype objects can produce full copies since objects of the same class can access each other’s private fields.

[Learn more about Prototype](/design-patterns/prototype)

**Complexity:**

**Popularity:**

**Usage examples:** The Prototype pattern is available in Python out of the box with a `copy` module.

**Identification:** The prototype can be easily recognized by a `clone` or `copy` methods, etc.

## Conceptual Example

This example illustrates the structure of the **Prototype** design pattern. It focuses on answering these questions:

*   What classes does it consist of?
*   What roles do these classes play?
*   In what way the elements of the pattern are related?

#### **main.py:** Conceptual example

```python
import copy


class SelfReferencingEntity:
    def __init__(self):
        self.parent = None

    def set_parent(self, parent):
        self.parent = parent


class SomeComponent:
    """
    Python provides its own interface of Prototype via `copy.copy` and
    `copy.deepcopy` functions. And any class that wants to implement custom
    implementations have to override `__copy__` and `__deepcopy__` member
    functions.
    """

    def __init__(self, some_int, some_list_of_objects, some_circular_ref):
        self.some_int = some_int
        self.some_list_of_objects = some_list_of_objects
        self.some_circular_ref = some_circular_ref

    def __copy__(self):
        """
        Create a shallow copy. This method will be called whenever someone calls
        `copy.copy` with this object and the returned value is returned as the
        new shallow copy.
        """

        # First, let's create copies of the nested objects.
        some_list_of_objects = copy.copy(self.some_list_of_objects)
        some_circular_ref = copy.copy(self.some_circular_ref)

        # Then, let's clone the object itself, using the prepared clones of the
        # nested objects.
        new = self.__class__(
            self.some_int, some_list_of_objects, some_circular_ref
        )
        new.__dict__.update(self.__dict__)

        return new

    def __deepcopy__(self, memo=None):
        """
        Create a deep copy. This method will be called whenever someone calls
        `copy.deepcopy` with this object and the returned value is returned as
        the new deep copy.

        What is the use of the argument `memo`? Memo is the dictionary that is
        used by the `deepcopy` library to prevent infinite recursive copies in
        instances of circular references. Pass it to all the `deepcopy` calls
        you make in the `__deepcopy__` implementation to prevent infinite
        recursions.
        """
        if memo is None:
            memo = {}

        # First, let's create copies of the nested objects.
        some_list_of_objects = copy.deepcopy(self.some_list_of_objects, memo)
        some_circular_ref = copy.deepcopy(self.some_circular_ref, memo)

        # Then, let's clone the object itself, using the prepared clones of the
        # nested objects.
        new = self.__class__(
            self.some_int, some_list_of_objects, some_circular_ref
        )
        new.__dict__ = copy.deepcopy(self.__dict__, memo)

        return new


if __name__ == "__main__":

    list_of_objects = [1, {1, 2, 3}, [1, 2, 3]]
    circular_ref = SelfReferencingEntity()
    component = SomeComponent(23, list_of_objects, circular_ref)
    circular_ref.set_parent(component)

    shallow_copied_component = copy.copy(component)

    # Let's change the list in shallow_copied_component and see if it changes in
    # component.
    shallow_copied_component.some_list_of_objects.append("another object")
    if component.some_list_of_objects[-1] == "another object":
        print(
            "Adding elements to `shallow_copied_component`'s "
            "some_list_of_objects adds it to `component`'s "
            "some_list_of_objects."
        )
    else:
        print(
            "Adding elements to `shallow_copied_component`'s "
            "some_list_of_objects doesn't add it to `component`'s "
            "some_list_of_objects."
        )

    # Let's change the set in the list of objects.
    component.some_list_of_objects[1].add(4)
    if 4 in shallow_copied_component.some_list_of_objects[1]:
        print(
            "Changing objects in the `component`'s some_list_of_objects "
            "changes that object in `shallow_copied_component`'s "
            "some_list_of_objects."
        )
    else:
        print(
            "Changing objects in the `component`'s some_list_of_objects "
            "doesn't change that object in `shallow_copied_component`'s "
            "some_list_of_objects."
        )

    deep_copied_component = copy.deepcopy(component)

    # Let's change the list in deep_copied_component and see if it changes in
    # component.
    deep_copied_component.some_list_of_objects.append("one more object")
    if component.some_list_of_objects[-1] == "one more object":
        print(
            "Adding elements to `deep_copied_component`'s "
            "some_list_of_objects adds it to `component`'s "
            "some_list_of_objects."
        )
    else:
        print(
            "Adding elements to `deep_copied_component`'s "
            "some_list_of_objects doesn't add it to `component`'s "
            "some_list_of_objects."
        )

    # Let's change the set in the list of objects.
    component.some_list_of_objects[1].add(10)
    if 10 in deep_copied_component.some_list_of_objects[1]:
        print(
            "Changing objects in the `component`'s some_list_of_objects "
            "changes that object in `deep_copied_component`'s "
            "some_list_of_objects."
        )
    else:
        print(
            "Changing objects in the `component`'s some_list_of_objects "
            "doesn't change that object in `deep_copied_component`'s "
            "some_list_of_objects."
        )

    print(
        f"id(deep_copied_component.some_circular_ref.parent): "
        f"{id(deep_copied_component.some_circular_ref.parent)}"
    )
    print(
        f"id(deep_copied_component.some_circular_ref.parent.some_circular_ref.parent): "
        f"{id(deep_copied_component.some_circular_ref.parent.some_circular_ref.parent)}"
    )
    print(
        "^^ This shows that deepcopied objects contain same reference, they "
        "are not cloned repeatedly."
    )
```

#### **Output.txt:** Execution result

```
Adding elements to `shallow_copied_component`'s some_list_of_objects adds it to `component`'s some_list_of_objects.
Changing objects in the `component`'s some_list_of_objects changes that object in `shallow_copied_component`'s some_list_of_objects.
Adding elements to `deep_copied_component`'s some_list_of_objects doesn't add it to `component`'s some_list_of_objects.
Changing objects in the `component`'s some_list_of_objects doesn't change that object in `deep_copied_component`'s some_list_of_objects.
id(deep_copied_component.some_circular_ref.parent): 4429472784
id(deep_copied_component.some_circular_ref.parent.some_circular_ref.parent): 4429472784
^^ This shows that deepcopied objects contain same reference, they are not cloned repeatedly.
```

#### Read next

[Singleton in Python](/design-patterns/singleton/python/example)

#### Return

[Factory Method in Python](/design-patterns/factory-method/python/example)

## **Prototype** in Other Languages

[![Prototype in C#](/images/patterns/icons/csharp.svg)](/design-patterns/prototype/csharp/example "Prototype in C#") [![Prototype in C++](/images/patterns/icons/cpp.svg)](/design-patterns/prototype/cpp/example "Prototype in C++") [![Prototype in Go](/images/patterns/icons/go.svg)](/design-patterns/prototype/go/example "Prototype in Go") [![Prototype in Java](/images/patterns/icons/java.svg)](/design-patterns/prototype/java/example "Prototype in Java") [![Prototype in PHP](/images/patterns/icons/php.svg)](/design-patterns/prototype/php/example "Prototype in PHP") [![Prototype in Ruby](/images/patterns/icons/ruby.svg)](/design-patterns/prototype/ruby/example "Prototype in Ruby") [![Prototype in Rust](/images/patterns/icons/rust.svg)](/design-patterns/prototype/rust/example "Prototype in Rust") [![Prototype in Swift](/images/patterns/icons/swift.svg)](/design-patterns/prototype/swift/example "Prototype in Swift") [![Prototype in TypeScript](/images/patterns/icons/typescript.svg)](/design-patterns/prototype/typescript/example "Prototype in TypeScript")

![Examples IDE banner](/images/patterns/banners/examples-ide.png)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

---
**Image Analysis:**

![Software Development and Design Elements](image.png)
An abstract illustration depicting various elements of software development and design, including a central tablet displaying UI components, a smartphone, code snippets, charts, and tools like a hammer and wrench, symbolizing the process of building and designing software systems.