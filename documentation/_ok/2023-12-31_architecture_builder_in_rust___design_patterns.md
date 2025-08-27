```yaml
---
title: Builder in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/builder/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:41:10.369Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Rust]
programming_languages: [Rust]
tags: [design-patterns, creational-patterns, builder-pattern, rust, object-construction, software-architecture, code-example, object-oriented-programming]
key_concepts: [builder-pattern, creational-design-patterns, fluent-interface, object-composition, separation-of-concerns, traits, object-construction, director-pattern]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article explains the Builder design pattern, a creational pattern used to construct complex objects step by step. It highlights how the Builder pattern allows for producing different types of products using the same construction process, unlike other creational patterns. The content provides a detailed example in Rust, demonstrating how to build both a Car and a CarManual using a common Builder trait and a Director. It also clarifies the distinction between the Builder pattern and the Fluent Interface idiom, which often get confused. Extensive Rust code examples illustrate the implementation of builders, products, components, and the director.]
---
```

# Builder in Rust / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Builder](/design-patterns/builder) / [Rust](/design-patterns/rust)

![Builder pattern illustration showing a hard hat next to modular building blocks being assembled.](/images/patterns/cards/builder-mini.png?id=19b95fd05e6469679752c0554b116815)

# **Builder** in Rust

**Builder** is a creational design pattern, which allows constructing complex objects step by step.

Unlike other creational patterns, Builder doesn’t require products to have a common interface. That makes it possible to produce different products using the same construction process.

[Learn more about Builder](/design-patterns/builder)

Navigation

 [Intro](#)

 [Car & car manual builders](#example-0)

 builders

  [mod](#example-0--builders-mod-rs)

  [car](#example-0--builders-car-rs)

  [car\_manual](#example-0--builders-car_manual-rs)

 cars

  [mod](#example-0--cars-mod-rs)

  [car](#example-0--cars-car-rs)

  [manual](#example-0--cars-manual-rs)

 [components](#example-0--components-rs)

 [director](#example-0--director-rs)

 [main](#example-0--main-rs)

## Car & car manual builders

This slightly synthetic example illustrates how you can use the Builder pattern to construct totally different products using the same building process. For example, the trait `Builder` declares steps for assembling a car. However, depending on the builder implementation, a constructed object can be something different, for example, a car manual. The resulting manual will contain instructions from each building step, making it accurate and up-to-date.

The **Builder** design pattern is not the same as the **Fluent Interface** idiom (that relies on _method chaining_), although Rust developers sometimes use those terms interchangeably.

1.  **Fluent Interface** is a way to chain methods for constructing or modifying an object using the following approach:
    
    ```rust
    let car = Car::default().places(5).gas(30)
    ```
    
    It’s pretty elegant way to construct an object. Still, such a code may not be an instance of the Builder pattern.
    
2.  While the **Builder** pattern also suggests constructing object step by step, it also lets you build different types of products using the same construction process.
    

### **builders:** Builders

#### **builders/mod.rs**

```rust
mod car;
mod car_manual;

use crate::components::{CarType, Engine, GpsNavigator, Transmission};

/// Builder defines how to assemble a car.
pub trait Builder {
    type OutputType;
    fn set_car_type(&mut self, car_type: CarType);
    fn set_seats(&mut self, seats: u16);
    fn set_engine(&mut self, engine: Engine);
    fn set_transmission(&mut self, transmission: Transmission);
    fn set_gsp_navigator(&mut self, gps_navigator: GpsNavigator);
    fn build(self) -> Self::OutputType;
}

pub use car::CarBuilder;
pub use car_manual::CarManualBuilder;
```

#### **builders/car.rs**

```rust
use crate::{
    cars::Car,
    components::{CarType, Engine, GpsNavigator, Transmission},
};

use super::Builder;

pub const DEFAULT_FUEL: f64 = 5f64;

#[derive(Default)]
pub struct CarBuilder {
    car_type: Option<CarType>,
    engine: Option<Engine>,
    gps_navigator: Option<GpsNavigator>,
    seats: Option<u16>,
    transmission: Option<Transmission>,
}

impl Builder for CarBuilder {
    type OutputType = Car;

    fn set_car_type(&mut self, car_type: CarType) {
        self.car_type = Some(car_type);
    }

    fn set_engine(&mut self, engine: Engine) {
        self.engine = Some(engine);
    }

    fn set_gsp_navigator(&mut self, gps_navigator: GpsNavigator) {
        self.gps_navigator = Some(gps_navigator);
    }

    fn set_seats(&mut self, seats: u16) {
        self.seats = Some(seats);
    }

    fn set_transmission(&mut self, transmission: Transmission) {
        self.transmission = Some(transmission);
    }

    fn build(self) -> Car {
        Car::new(
            self.car_type.expect("Please, set a car type"),
            self.seats.expect("Please, set a number of seats"),
            self.engine.expect("Please, set an engine configuration"),
            self.transmission.expect("Please, set up transmission"),
            self.gps_navigator,
            DEFAULT_FUEL,
        )
    }
}
```

#### **builders/car_manual.rs**

```rust
use crate::{
    cars::Manual,
    components::{CarType, Engine, GpsNavigator, Transmission},
};

use super::Builder;

#[derive(Default)]
pub struct CarManualBuilder {
    car_type: Option<CarType>,
    engine: Option<Engine>,
    gps_navigator: Option<GpsNavigator>,
    seats: Option<u16>,
    transmission: Option<Transmission>,
}

/// Builds a car manual instead of an actual car.
impl Builder for CarManualBuilder {
    type OutputType = Manual;

    fn set_car_type(&mut self, car_type: CarType) {
        self.car_type = Some(car_type);
    }

    fn set_engine(&mut self, engine: Engine) {
        self.engine = Some(engine);
    }

    fn set_gsp_navigator(&mut self, gps_navigator: GpsNavigator) {
        self.gps_navigator = Some(gps_navigator);
    }

    fn set_seats(&mut self, seats: u16) {
        self.seats = Some(seats);
    }

    fn set_transmission(&mut self, transmission: Transmission) {
        self.transmission = Some(transmission);
    }

    fn build(self) -> Manual {
        Manual::new(
            self.car_type.expect("Please, set a car type"),
            self.seats.expect("Please, set a number of seats"),
            self.engine.expect("Please, set an engine configuration"),
            self.transmission.expect("Please, set up transmission"),
            self.gps_navigator,
        )
    }
}
```

### **cars:** Products

#### **cars/mod.rs**

```rust
mod car;
mod manual;

pub