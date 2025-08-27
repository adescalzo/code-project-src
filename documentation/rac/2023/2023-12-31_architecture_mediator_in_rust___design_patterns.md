```yaml
---
title: Mediator in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/mediator/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:28:46.889Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Rust]
programming_languages: [Rust]
tags: [design-patterns, mediator-pattern, rust, behavioral-patterns, ownership, decoupling, software-design, code-example]
key_concepts: [Mediator pattern, behavioral design pattern, coupling, ownership model, borrow checker, traits, dependency management]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explains the Mediator behavioral design pattern, focusing on its implementation in Rust. It details how the Mediator pattern reduces direct coupling between components by centralizing their communication through a dedicated mediator object. The content introduces the "Top-Down Ownership" approach, which is particularly well-suited for Rust's strict ownership model and borrow checker rules. Extensive Rust code examples demonstrate a practical scenario involving a train station mediating interactions between different types of trains, illustrating the pattern's application and benefits in managing complex component relationships.
---
```

# Mediator in Rust / Design Patterns

[Design Patterns](/design-patterns) / [Mediator](/design-patterns/mediator) / [Rust](/design-patterns/rust)

![Mediator pattern icon showing a central object mediating communication between four other objects.](/images/patterns/cards/mediator-mini.png?id=a7e43ee8e17e4474737b1fcb3201d7ba)

# Mediator in Rust

**Mediator** is a behavioral design pattern that reduces coupling between components of a program by making them communicate indirectly, through a special mediator object.

The Mediator makes it easy to modify, extend and reuse individual components because they’re no longer dependent on the dozens of other classes.

[Learn more about Mediator](/design-patterns/mediator)

Navigation

 [Intro](#)

 [Top-Down Ownership](#example-0)

 [train\_station](#example-0--train_station-rs)

  [mod](#example-0--trains-mod-rs)

  [freight\_train](#example-0--trains-freight_train-rs)

  [passenger\_train](#example-0--trains-passenger_train-rs)

 [main](#example-0--main-rs)

## Top-Down Ownership

Top-Down Ownership approach allows to apply Mediator in Rust as it is a suitable for Rust’s ownership model with strict borrow checker rules. It’s not the only way to implement Mediator, but it’s a fundamental one.

The key point is thinking in terms of OWNERSHIP.

1.  A mediator takes ownership of all components.
    
2.  A component doesn’t preserve a reference to a mediator. Instead, it gets the reference via a method call.
    
    ```rust
    // A train gets a mediator object by reference.
    pub trait Train {
        fn name(&self) -> &String;
        fn arrive(&mut self, mediator: &mut dyn Mediator);
        fn depart(&mut self, mediator: &mut dyn Mediator);
    }
    
    // Mediator has notification methods.
    pub trait Mediator {
        fn notify_about_arrival(&mut self, train_name: &str) -> bool;
        fn notify_about_departure(&mut self, train_name: &str);
    }
    ```
    
3.  Control flow starts from `fn main()` where the mediator receives external events/commands.
    
4.  `Mediator` trait for the interaction between components (`notify_about_arrival`, `notify_about_departure`) is not the same as its external API for receiving external events (`accept`, `depart` commands from the main loop).
    
    ```rust
    let train1 = PassengerTrain::new("Train 1");
    let train2 = FreightTrain::new("Train 2");
    
    // Station has `accept` and `depart` methods,
    // but it also implements `Mediator`.
    let mut station = TrainStation::default();
    
    // Station is taking ownership of the trains.
    station.accept(train1);
    station.accept(train2);
    
    // `train1` and `train2` have been moved inside,
    // but we can use train names to depart them.
    station.depart("Train 1");
    station.depart("Train 2");
    station.depart("Train 3");
    ```

![Diagram illustrating the Top-Down Ownership approach for the Mediator pattern in Rust, showing a central mediator owning and interacting with multiple components.](https://github.com/fadeevab/mediator-pattern-rust/raw/main/images/mediator-rust-approach.jpg)

#### Extra info

There is a research and discussion of the Mediator pattern in Rust: [https://github.com/fadeevab/mediator-pattern-rust](https://github.com/fadeevab/mediator-pattern-rust)

#### train\_station.rs

```rust
use std::collections::{HashMap, VecDeque};

use crate::trains::Train;

// Mediator has notification methods.
pub trait Mediator {
    fn notify_about_arrival(&mut self, train_name: &str) -> bool;
    fn notify_about_departure(&mut self, train_name: &str);
}

#[derive(Default)]
pub struct TrainStation {
    trains: HashMap<String, Box<dyn Train>>,
    train_queue: VecDeque<String>,
    train_on_platform: Option<String>,
}

impl Mediator for TrainStation {
    fn notify_about_arrival(&mut self, train_name: &str) -> bool {
        if self.train_on_platform.is_some() {
            self.train_queue.push_back(train_name.into());
            false
        } else {
            self.train_on_platform.replace(train_name.into());
            true
        }
    }

    fn notify_about_departure(&mut self, train_name: &str) {
        if Some(train_name.into()) == self.train_on_platform {
            self.train_on_platform = None;

            if let Some(next_train_name) = self.train_queue.pop_front() {
                let mut next_train = self.trains.remove(&next_train_name).unwrap();
                next_train.arrive(self);
                self.trains.insert(next_train_name.clone(), next_train);

                self.train_on_platform = Some(next_train_name);
            }
        }
    }
}

impl TrainStation {
    pub fn accept(&mut self, mut train: impl Train + 'static) {
        if self.trains.contains_key(train.name()) {
            println!("{} has already arrived", train.name());
            return;
        }

        train.arrive(self);
        self.trains.insert(train.name().clone(), Box::new(train));
    }

    pub fn depart(&mut self, name: &'static str) {
        let train = self.trains.remove(name);
        if let Some(mut train) = train {
            train.depart(self);
        } else {
            println!("'{}' is not on the station!", name);
        }
    }
}
```

#### trains/mod.rs

```rust
mod freight_train;
mod passenger_train;

pub use freight_train::FreightTrain;
pub use passenger_train::PassengerTrain;

use crate::train_station::Mediator;

// A train gets a mediator object by reference.
pub trait Train {
    fn name(&self) -> &String;
    fn arrive(&mut self, mediator: &mut dyn Mediator);
    fn depart(&mut self, mediator: &mut dyn Mediator);
}
```

#### trains/freight\_train.rs

```rust
use super::Train;
use crate::train_station::Mediator;

pub struct FreightTrain {
    name: String,
}

impl FreightTrain {
    pub fn new(name: &'static str) -> Self {
        Self { name: name.into() }
    }
}

impl Train for FreightTrain {
    fn name(&self) -> &String {
        &self.name
    }

    fn arrive(&mut self, mediator: &mut dyn Mediator) {
        if !mediator.notify_about_arrival(&self.name) {
            println!("Freight train {}: Arrival blocked, waiting", self.name);
            return;
        }

        println!("Freight train {}: Arrived", self.name);
    }

    fn depart(&mut self, mediator: &mut dyn Mediator) {
        println!("Freight train {}: Leaving", self.name);
        mediator.notify_about_departure(&self.name);
    }
}
```

#### trains/passenger\_train.rs

```rust
use super::Train;
use crate::train_station::Mediator;

pub struct PassengerTrain {
    name: String,
}

impl PassengerTrain {
    pub fn new(name: &'static str) -> Self {
        Self { name: name.into() }
    }
}

impl Train for PassengerTrain {
    fn name(&self) -> &String {
        &self.name
    }

    fn arrive(&mut self, mediator: &mut dyn Mediator) {
        if !mediator.notify_about_arrival(&self.name) {
            println!("Passenger train {}: Arrival blocked, waiting", self.name);
            return;
        }

        println!("Passenger train {}: Arrived", self.name);
    }

    fn depart(&mut self, mediator: &mut dyn Mediator) {
        println!("Passenger train {}: Leaving", self.name);
        mediator.notify_about_departure(&self.name);
    }
}
```

#### main.rs: Client code

```rust
mod train_station;
mod trains;

use train_station::TrainStation;
use trains::{FreightTrain, PassengerTrain};

fn main() {
    let train1 = PassengerTrain::new("Train 1");
    let train2 = FreightTrain::new("Train 2");

    // Station has `accept` and `depart` methods,
    // but it also implements `Mediator`.
    let mut station = TrainStation::default();

    // Station is taking ownership of the trains.
    station.accept(train1);
    station.accept(train2);

    // `train1` and `train2` have been moved inside,
    // but we can use train names to depart them.
    station.depart("Train 1");
    station.depart("Train 2");
    station.depart("Train 3");
}
```

### Output

```
Passenger train Train 1: Arrived
Freight train Train 2: Arrival blocked, waiting
Passenger train Train 1: Leaving
Freight train Train 2: Arrived
Freight train Train 2: Leaving
'Train 3' is not on the station!
```

#### Read next

[Memento in Rust](/design-patterns/memento/rust/example) 

#### Return

 [Iterator in Rust](/design-patterns/iterator/rust/example)

## **Mediator** in Other Languages

![Mediator in C# icon](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58) [Mediator in C#](/design-patterns/mediator/csharp/example "Mediator in C#") ![Mediator in C++ icon](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858) [Mediator in C++](/design-patterns/mediator/cpp/example "Mediator in C++") ![Mediator in Go icon](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca) [Mediator in Go](/design-patterns/mediator/go/example "Mediator in Go") ![Mediator in Java icon](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e) [Mediator in Java](/design-patterns/mediator/java/example "Mediator in Java") ![Mediator in PHP icon](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618) [Mediator in PHP](/design-patterns/mediator/php/example "Mediator in PHP") ![Mediator in Python icon](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f) [Mediator in Python](/design-patterns/mediator/python/example "Mediator in Python") ![Mediator in Ruby icon](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb) [Mediator in Ruby](/design-patterns/mediator/ruby/example "Mediator in Ruby") ![Mediator in Swift icon](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d) [Mediator in Swift](/design-patterns/mediator/swift/example "Mediator in Swift") ![Mediator in TypeScript icon](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7) [Mediator in TypeScript](/design-patterns/mediator/typescript/example "Mediator in TypeScript")

![Promotional banner for an eBook about design patterns with IDE examples.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Top-Down Ownership](#example-0)

 [train\_station](#example-0--train_station-rs)

  [mod](#example-0--trains-mod-rs)

  [freight\_train](#example-0--trains-freight_train-rs)

  [passenger\_train](#example-0--trains-passenger_train-rs)

 [main](#example-0--main-rs)