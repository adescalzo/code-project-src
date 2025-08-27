```yaml
---
title: Visitor in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/visitor/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:26:51.621Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [serde]
programming_languages: [Rust]
tags: [design-patterns, visitor-pattern, rust, behavioral-patterns, deserialization, software-design, trait, generics]
key_concepts: [Visitor pattern, Deserialization, Traits, Generics, Double Dispatch, Method overloading]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explains the Visitor behavioral design pattern, demonstrating its implementation in Rust. It highlights how the Visitor pattern allows adding new behaviors to existing class hierarchies without modifying their core code. The content uses the `serde` serialization framework's deserialization model as a practical, real-world example. Code examples illustrate the creation of `Visitor` and `Deserializer` traits, along with concrete implementations to convert input data into different struct types.
---
```

# Visitor in Rust / Design Patterns

[Design Patterns](/design-patterns) / [Visitor](/design-patterns/visitor) / [Rust](/design-patterns/rust)

![A diagram illustrating the Visitor design pattern, showing a client (left) interacting with a hierarchy of elements (right) to apply operations.](/images/patterns/cards/visitor-mini.png?id=854a35a62963bec1d75eab996918989b)

# **Visitor** in Rust

**Visitor** is a behavioral design pattern that allows adding new behaviors to existing class hierarchy without altering any existing code.

> Read why Visitors can’t be simply replaced with method overloading in our article [Visitor and Double Dispatch](/design-patterns/visitor-double-dispatch).

[Learn more about Visitor](/design-patterns/visitor)

Navigation

 [Intro](#)

 [Deserialization](#example-0)

 [visitor](#example-0--visitor-rs)

 [main](#example-0--main-rs)

## Deserialization

A real-world example of the Visitor pattern is [serde serialization framework](https://serde.rs) and its deserialization model (see [Serde data model](https://serde.rs/data-model.html)).

1.  `Visitor` should be implemented for a deserializable type.
2.  `Visitor` is passed to a `Deserializer` (an “Element” in terms of the Visitor Pattern), which accepts and drives the `Visitor` in order to construct a desired type.

Let’s reproduce this deserializing model in our example.

#### [](#example-0--visitor-rs)**visitor.rs**

```rust
use crate::{TwoValuesArray, TwoValuesStruct};

/// Visitor can visit one type, do conversions, and output another type.
///
/// It's not like all visitors must return a new type, it's just an example
/// that demonstrates the technique.
pub trait Visitor {
    type Value;

    /// Visits a vector of integers and outputs a desired type.
    fn visit_vec(&self, v: Vec<i32>) -> Self::Value;
}

/// Visitor implementation for a struct of two values.
impl Visitor for TwoValuesStruct {
    type Value = TwoValuesStruct;

    fn visit_vec(&self, v: Vec<i32>) -> Self::Value {
        TwoValuesStruct { a: v[0], b: v[1] }
    }
}

/// Visitor implementation for a struct of values array.
impl Visitor for TwoValuesArray {
    type Value = TwoValuesArray;

    fn visit_vec(&self, v: Vec<i32>) -> Self::Value {
        let mut ab = [0i32; 2];

        ab[0] = v[0];
        ab[1] = v[1];

        TwoValuesArray { ab }
    }
}
```

#### [](#example-0--main-rs)**main.rs**

```rust
#![allow(unused)]

mod visitor;

use visitor::Visitor;

/// A struct of two integer values.
///
/// It's going to be an output of `Visitor` trait which is defined for the type
/// in `visitor.rs`.
#[derive(Default, Debug)]
pub struct TwoValuesStruct {
    a: i32,
    b: i32,
}

/// A struct of values array.
///
/// It's going to be an output of `Visitor` trait which is defined for the type
/// in `visitor.rs`.
#[derive(Default, Debug)]
pub struct TwoValuesArray {
    ab: [i32; 2],
}

/// `Deserializer` trait defines methods that can parse either a string or
/// a vector, it accepts a visitor which knows how to construct a new object
/// of a desired type (in our case, `TwoValuesArray` and `TwoValuesStruct`).
trait Deserializer<V: Visitor> {
    fn create(visitor: V) -> Self;
    fn parse_str(&self, input: &str) -> Result<V::Value, &'static str> {
        Err("parse_str is unimplemented")
    }
    fn parse_vec(&self, input: Vec<i32>) -> Result<V::Value, &'static str> {
        Err("parse_vec is unimplemented")
    }
}

struct StringDeserializer<V: Visitor> {
    visitor: V,
}

impl<V: Visitor> Deserializer<V> for StringDeserializer<V> {
    fn create(visitor: V) -> Self {
        Self { visitor }
    }

    fn parse_str(&self, input: &str) -> Result<V::Value, &'static str> {
        // In this case, in order to apply a visitor, a deserializer should do
        // some preparation. The visitor does its stuff, but it doesn't do everything.
        let input_vec = input
            .split_ascii_whitespace()
            .map(|x| x.parse().unwrap())
            .collect();

        Ok(self.visitor.visit_vec(input_vec))
    }
}

struct VecDeserializer<V: Visitor> {
    visitor: V,
}

impl<V: Visitor> Deserializer<V> for VecDeserializer<V> {
    fn create(visitor: V) -> Self {
        Self { visitor }
    }

    fn parse_vec(&self, input: Vec<i32>) -> Result<V::Value, &'static str> {
        Ok(self.visitor.visit_vec(input))
    }
}

fn main() {
    let deserializer = StringDeserializer::create(TwoValuesStruct::default());
    let result = deserializer.parse_str("123 456");
    println!("{:?}", result);

    let deserializer = VecDeserializer::create(TwoValuesStruct::default());
    let result = deserializer.parse_vec(vec![123, 456]);
    println!("{:?}", result);

    let deserializer = VecDeserializer::create(TwoValuesArray::default());
    let result = deserializer.parse_vec(vec![123, 456]);
    println!("{:?}", result);

    println!(
        "Error: {}",
        deserializer.parse_str("123 456").err().unwrap()
    )
}
```

### Output

```
Ok(TwoValuesStruct { a: 123, b: 456 })
Ok(TwoValuesStruct { a: 123, b: 456 })
Ok(TwoValuesArray { ab: [123, 456] })
Error: parse_str unimplemented
```

#### Read next

[Design Patterns in Swift](/design-patterns/swift) 

#### Return

 [Template Method in Rust](/design-patterns/template-method/rust/example)

## **Visitor** in Other Languages

[![Visitor in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58 "Visitor in C#")](/design-patterns/visitor/csharp/example) [![Visitor in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858 "Visitor in C++")](/design-patterns/visitor/cpp/example) [![Visitor in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca "Visitor in Go")](/design-patterns/visitor/go/example) [![Visitor in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e "Visitor in Java")](/design-patterns/visitor/java/example) [![Visitor in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618 "Visitor in PHP")](/design-patterns/visitor/php/example) [![Visitor in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f "Visitor in Python")](/design-patterns/visitor/python/example) [![Visitor in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb "Visitor in Ruby")](/design-patterns/visitor/ruby/example) [![Visitor in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d "Visitor in Swift")](/design-patterns/visitor/swift/example) [![Visitor in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7 "Visitor in TypeScript")](/design-patterns/visitor/typescript/example)

[![A promotional banner featuring various development tools, code snippets, and UI elements, suggesting a comprehensive resource for design pattern examples.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)](/design-patterns/book)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Deserialization](#example-0)

 [visitor](#example-0--visitor-rs)

 [main](#example-0--main-rs)