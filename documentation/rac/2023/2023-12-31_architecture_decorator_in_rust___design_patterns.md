```yaml
---
title: Decorator in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/decorator/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:29:40.249Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: ["std::io", BufReader, Cursor]
programming_languages: [Rust]
tags: [design-patterns, decorator-pattern, rust, structural-patterns, io, buffering, object-composition]
key_concepts: [decorator-pattern, structural-pattern, object-composition, dynamic-behavior-extension, input-output-operations, buffering]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces the Decorator design pattern, a structural pattern that allows adding new behaviors to objects dynamically by wrapping them in special decorator objects. It explains that decorators enable stacking behaviors since both target objects and decorators adhere to the same interface. The article provides a practical example from Rust's standard library, demonstrating how `BufReader` decorates a `Cursor` to add buffered behavior to input/output operations. A Rust code example illustrates the concept, showing how to read data from a buffered reader that wraps input data.
---
```

# Decorator in Rust / Design Patterns

![An illustration depicting the Decorator pattern, showing a paint roller with an added red border (decoration) and a set of nested Matryoshka dolls, symbolizing wrapping objects.](/images/patterns/cards/decorator-mini.png?id=d30458908e315af195cb183bc52dbef9)

# **Decorator** in Rust

**Decorator** is a structural pattern that allows adding new behaviors to objects dynamically by placing them inside special wrapper objects, called _decorators_.

Using decorators you can wrap objects countless number of times since both target objects and decorators follow the same interface. The resulting object will get a stacking behavior of all wrappers.

## Input streams decoration

There is a **_practical example_** in Rust’s standard library for input/output operations.

A buffered reader decorates a vector reader adding buffered behavior.

```rust
let mut input = BufReader::new(Cursor::new("Input data"));
input.read(&mut buf).ok();
```

#### **main.rs**

```rust
use std::io::{BufReader, Cursor, Read};

fn main() {
    let mut buf = [0u8; 10];

    // A buffered reader decorates a vector reader which wraps input data.
    let mut input = BufReader::new(Cursor::new("Input data"));

    input.read(&mut buf).ok();

    print!("Read from a buffered reader: ");

    for byte in buf {
        print!("{}", char::from(byte));
    }

    println!();
}
```

### Output

```
Read from a buffered reader: Input data
```