```yaml
---
title: Memento in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/memento/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:27:31.359Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Serde serialization framework, serde_json]
programming_languages: [Rust]
tags: [design-patterns, memento, rust, serialization, state-management, behavioral-patterns, data-snapshot, object-state]
key_concepts: [Memento design pattern, object state, serialization, deserialization, traits (Rust), data snapshots, state restoration]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Memento behavioral design pattern, explaining its purpose of allowing an object's state to be saved and restored without compromising its internal structure. It provides two practical examples implemented in Rust. The first demonstrates a conceptual Memento pattern using custom traits and structs, while the second showcases a more robust implementation leveraging the Serde serialization framework for efficient state persistence. Both examples illustrate how to capture and restore the state of an `Originator` object, highlighting the pattern's utility in managing historical states or implementing undo/redo functionalities.]
---
```

# Memento in Rust / Design Patterns

![Memento pattern concept: A document with highlighted sections being saved to a floppy disk, symbolizing state capture.](/images/patterns/cards/memento-mini.png?id=8b2ea4dc2c5d15775a654808cc9de099)

# **Memento** in Rust

**Memento** is a behavioral design pattern that allows making snapshots of an object’s state and restoring it in future.

The Memento doesn’t compromise the internal structure of the object it works with, as well as data kept inside the snapshots.

## Conceptual example

This is a conceptual example of Memento pattern.

#### **conceptual.rs**

```rust
trait Memento<T> {
    fn restore(self) -> T;
    fn print(&self);
}

struct Originator {
    state: u32,
}

impl Originator {
    pub fn save(&self) -> OriginatorBackup {
        OriginatorBackup {
            state: self.state.to_string(),
        }
    }
}

struct OriginatorBackup {
    state: String,
}

impl Memento<Originator> for OriginatorBackup {
    fn restore(self) -> Originator {
        Originator {
            state: self.state.parse().unwrap(),
        }
    }

    fn print(&self) {
        println!("Originator backup: '{}'", self.state);
    }
}

fn main() {
    let mut history = Vec::<OriginatorBackup>::new();

    let mut originator = Originator { state: 0 };

    originator.state = 1;
    history.push(originator.save());

    originator.state = 2;
    history.push(originator.save());

    for moment in history.iter() {
        moment.print();
    }

    let originator = history.pop().unwrap().restore();
    println!("Restored to state: {}", originator.state);

    let originator = history.pop().unwrap().restore();
    println!("Restored to state: {}", originator.state);
}
```

### Output

```
Originator backup: '1'
Originator backup: '2'
Restored to state: 2
Restored to state: 1
```

## Serde serialization framework

A common way to make a structure serializable is to derive `Serialize` and `Deserialize` traits from the [serde serialization framework](https://crates.io/crates/serde). Then an object of serializable type can be converted to many different formats, e.g. JSON with [serde_json](https://crates.io/crates/serde_json) crate.

#### **serde.rs**

```rust
use serde::{Deserialize, Serialize};

/// An object to be stored. It derives a default
/// `Serialize` and `Deserialize` trait implementation, which
/// allows to convert it into many different formats (e.g. JSON).
#[derive(Serialize, Deserialize)]
struct Originator {
    state: u32,
}

impl Originator {
    /// Serializes an originator into a string of JSON format.
    pub fn save(&self) -> String {
        serde_json::to_string(self).unwrap()
    }

    /// Deserializes an originator into a string of JSON format.
    pub fn restore(json: &str) -> Self {
        serde_json::from_str(json).unwrap()
    }
}

fn main() {
    // A stack of mementos.
    let mut history = Vec::<String>::new();

    let mut originator = Originator { state: 0 };

    originator.state = 1;
    history.push(originator.save());

    originator.state = 2;
    history.push(originator.save());

    for moment in history.iter() {
        println!("{}", moment);
    }

    let originator = Originator::restore(&history.pop().unwrap());
    println!("Restored to state: {}", originator.state);

    let originator = Originator::restore(&history.pop().unwrap());
    println!("Restored to state: {}", originator.state);
}
```

### Output

```
{"state":1}
{"state":2}
Restored to state: 2
Restored to state: 1
```

---

### Visual Aids

![A document with wavy lines (representing content or state) being saved to a red floppy disk, symbolizing the saving of an object's state.](/images/patterns/cards/memento-mini.png?id=8b2ea4dc2c5d15775a654808cc9de099)

![A detailed illustration of a desktop computer, tablet, and smartphone surrounded by various development tools, code snippets, and UI elements, representing a software development environment or the application of design patterns in software.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)