```yaml
---
title: Prototype in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/prototype/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:30:20.718Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Rust, "std::clone::Clone"]
programming_languages: [Rust]
tags: [design-patterns, creational-patterns, prototype-pattern, rust, object-cloning, traits, software-design]
key_concepts: [prototype-design-pattern, creational-design-pattern, object-cloning, traits, derive-macro]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explains the Prototype design pattern, a creational pattern focused on cloning objects without tight coupling to their specific classes. It demonstrates how Rust's built-in `std::clone::Clone` trait simplifies the implementation of this pattern. A practical code example shows how to use `#[derive(Clone)]` on a struct to enable easy object duplication. The example illustrates that modifications to the cloned object do not affect the original, showcasing the pattern's utility. The article also highlights the pattern's applicability across various other programming languages.]
---
```

# Prototype in Rust / Design Patterns

![Image: A gray robot on the left with an arrow pointing to an identical red robot on the right, symbolizing object cloning or duplication.](/images/patterns/cards/prototype-mini.png?id=bc3046bb39ff36574c08d49839fd1c8e)

# Prototype in Rust

**Prototype** is a creational design pattern that allows cloning objects, even complex ones, without coupling to their specific classes.

All prototype classes should have a common interface that makes it possible to copy objects even if their concrete classes are unknown. Prototype objects can produce full copies since objects of the same class can access each other’s private fields.

## Built-in `Clone` trait

Rust has a built-in `std::clone::Clone` trait with many implementations for various types (via `#[derive(Clone)]`). Thus, the Prototype pattern is ready to use out of the box.

### `main.rs`

```rust
#[derive(Clone)]
struct Circle {
    pub x: u32,
    pub y: u32,
    pub radius: u32,
}

fn main() {
    let circle1 = Circle {
        x: 10,
        y: 15,
        radius: 10,
    };

    // Prototype in action.
    let mut circle2 = circle1.clone();
    circle2.radius = 77;

    println!("Circle 1: {}, {}, {}", circle1.x, circle1.y, circle1.radius);
    println!("Circle 2: {}, {}, {}", circle2.x, circle2.y, circle2.radius);
}
```

### Output

```
Circle 1: 10, 15, 10
Circle 2: 10, 15, 77
```

## Prototype in Other Languages

[![Prototype in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/prototype/csharp/example "Prototype in C#") [![Prototype in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/prototype/cpp/example "Prototype in C++") [![Prototype in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/prototype/go/example "Prototype in Go") [![Prototype in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/prototype/java/example "Prototype in Java") [![Prototype in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/prototype/php/example "Prototype in PHP") [![Prototype in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/prototype/python/example "Prototype in Python") [![Prototype in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/ruby/example "Prototype in Ruby") [![Prototype in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/prototype/swift/example "Prototype in Swift") [![Prototype in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/prototype/typescript/example "Prototype in TypeScript")

![Image: An abstract illustration of a desktop computer and a mobile phone displaying code and various UI elements, surrounded by development tools like a wrench, hammer, and code snippets, representing a comprehensive development environment or a collection of coding examples.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)