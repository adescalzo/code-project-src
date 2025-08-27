```yaml
---
title: Chain of Responsibility in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/chain-of-responsibility/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:29:12.223Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Rust]
programming_languages: [Rust]
tags: [design-patterns, behavioral-patterns, chain-of-responsibility, rust, software-design, object-oriented-design]
key_concepts: [chain-of-responsibility-pattern, dynamic-dispatch, trait-objects, handler-interface, request-handling, loose-coupling]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces the Chain of Responsibility behavioral design pattern, which enables a request to be passed sequentially through a chain of potential handlers until it is processed. It provides a detailed conceptual example implemented in Rust, demonstrating how to build a flexible chain using `Box` pointers and trait objects for dynamic dispatch. The example simulates a patient's flow through various hospital departments, each acting as a handler in the chain. The content includes comprehensive Rust code snippets for the patient, department trait, and individual department handlers, along with the client code to illustrate the pattern's application.
---
```

# Chain of Responsibility in Rust / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Chain of Responsibility](/design-patterns/chain-of-responsibility) / [Rust](/design-patterns/rust)

![Chain of Responsibility pattern icon, depicting a series of interconnected gears or links, symbolizing a chain of processing.](/images/patterns/cards/chain-of-responsibility-mini.png?id=36d85eba8d14986f053123de17aac7a7)

# **Chain of Responsibility** in Rust

**Chain of Responsibility** is behavioral design pattern that allows passing request along the chain of potential handlers until one of them handles request.

The pattern allows multiple objects to handle the request without coupling sender class to the concrete classes of the receivers. The chain can be composed dynamically at runtime with any handler that follows a standard handler interface.

[Learn more about Chain of Responsibility](/design-patterns/chain-of-responsibility)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [patient](#example-0--patient-rs)

 [department](#example-0--department-rs)

  [cashier](#example-0--department-cashier-rs)

  [doctor](#example-0--department-doctor-rs)

  [medical](#example-0--department-medical-rs)

  [reception](#example-0--department-reception-rs)

 [main](#example-0--main-rs)

## Conceptual Example

The example demonstrates processing a patient through a chain of departments. The chain of responsibility is constructed as follows:

Patient -> Reception -> Doctor -> Medical -> Cashier

The chain is constructed using `Box` pointers, which means dynamic dispatch in runtime. Why? It seems quite difficult to narrow down implementation to a strict compile-time typing using generics: in order to construct a type of a full chain Rust needs full knowledge of the “next of the next” link in the chain. Thus, it would look like this:

let mut reception \= Reception::<Doctor::<Medical::<Cashier\>>>::new(doctor); // 😱

Instead, `Box` allows chaining in any combination:

let mut reception \= Reception::new(doctor); // 👍

let mut reception \= Reception::new(cashier); // 🕵️‍♀️

#### [](#example-0--patient-rs)**patient.rs:** Request

#\[derive(Default)\]
pub struct Patient {
    pub name: String,
    pub registration\_done: bool,
    pub doctor\_check\_up\_done: bool,
    pub medicine\_done: bool,
    pub payment\_done: bool,
}

#### [](#example-0--department-rs)**department.rs:** Handlers

mod cashier;
mod doctor;
mod medical;
mod reception;

pub use cashier::Cashier;
pub use doctor::Doctor;
pub use medical::Medical;
pub use reception::Reception;

use crate::patient::Patient;

/// A single role of objects that make up a chain.
/// A typical trait implementation must have \`handle\` and \`next\` methods,
/// while \`execute\` is implemented by default and contains a proper chaining
/// logic.
pub trait Department {
    fn execute(&mut self, patient: &mut Patient) {
        self.handle(patient);

        if let Some(next) \= &mut self.next() {
            next.execute(patient);
        }
    }

    fn handle(&mut self, patient: &mut Patient);
    fn next(&mut self) \-> &mut Option<Box<dyn Department\>>;
}

/// Helps to wrap an object into a boxed type.
pub fn into\_next(department: impl Department + Sized + 'static) \-> Option<Box<dyn Department\>> {
    Some(Box::new(department))
}

#### [](#example-0--department-cashier-rs)**department/cashier.rs**

use super::{Department, Patient};

#\[derive(Default)\]
pub struct Cashier {
    next: Option<Box<dyn Department\>>,
}

impl Department for Cashier {
    fn handle(&mut self, patient: &mut Patient) {
        if patient.payment\_done {
            println!("Payment done");
        } else {
            println!("Cashier getting money from a patient {}", patient.name);
            patient.payment\_done \= true;
        }
    }

    fn next(&mut self) \-> &mut Option<Box<dyn Department\>> {
        &mut self.next
    }
}

#### [](#example-0--department-doctor-rs)**department/doctor.rs**

use super::{into\_next, Department, Patient};

pub struct Doctor {
    next: Option<Box<dyn Department\>>,
}

impl Doctor {
    pub fn new(next: impl Department + 'static) \-> Self {
        Self {
            next: into\_next(next),
        }
    }
}

impl Department for Doctor {
    fn handle(&mut self, patient: &mut Patient) {
        if patient.doctor\_check\_up\_done {
            println!("A doctor checkup is already done");
        } else {
            println!("Doctor checking a patient {}", patient.name);
            patient.doctor\_check\_up\_done \= true;
        }
    }

    fn next(&mut self) \-> &mut Option<Box<dyn Department\>> {
        &mut self.next
    }
}

#### [](#example-0--department-medical-rs)**department/medical.rs**

use super::{into\_next, Department, Patient};

pub struct Medical {
    next: Option<Box<dyn Department\>>,
}

impl Medical {
    pub fn new(next: impl Department + 'static) \-> Self {
        Self {
            next: into\_next(next),
        }
    }
}

impl Department for Medical {
    fn handle(&mut self, patient: &mut Patient) {
        if patient.medicine\_done {
            println!("Medicine is already given to a patient");
        } else {
            println!("Medical giving medicine to a patient {}", patient.name);
            patient.medicine\_done \= true;
        }
    }

    fn next(&mut self) \-> &mut Option<Box<dyn Department\>> {
        &mut self.next
    }
}

#### [](#example-0--department-reception-rs)**department/reception.rs**

use super::{into\_next, Department, Patient};

#\[derive(Default)\]
pub struct Reception {
    next: Option<Box<dyn Department\>>,
}

impl Reception {
    pub fn new(next: impl Department + 'static) \-> Self {
        Self {
            next: into\_next(next),
        }
    }
}

impl Department for Reception {
    fn handle(&mut self, patient: &mut Patient) {
        if patient.registration\_done {
            println!("Patient registration is already done");
        } else {
            println!("Reception registering a patient {}", patient.name);
            patient.registration\_done \= true;
        }
    }

    fn next(&mut self) \-> &mut Option<Box<dyn Department\>> {
        &mut self.next
    }
}

#### [](#example-0--main-rs)**main.rs:** Client code

mod department;
mod patient;

use department::{Cashier, Department, Doctor, Medical, Reception};
use patient::Patient;

fn main() {
    let cashier \= Cashier::default();
    let medical \= Medical::new(cashier);
    let doctor \= Doctor::new(medical);
    let mut reception \= Reception::new(doctor);

    let mut patient \= Patient {
        name: "John".into(),
        ..Patient::default()
    };

    // Reception handles a patient passing him to the next link in the chain.
    // Reception -> Doctor -> Medical -> Cashier.
    reception.execute(&mut patient);

    println!("\\nThe patient has been already handled:\\n");

    reception.execute(&mut patient);
}

### Output

Reception registering a patient John
Doctor checking a patient John
Medical giving medicine to a patient John
Cashier getting money from a patient John

The patient has been already handled:

Patient registration is already done
A doctor checkup is already already done
Medicine is already given to a patient
Payment done

#### Read next

[Command in Rust](/design-patterns/command/rust/example) 

#### Return

 [Proxy in Rust](/design-patterns/proxy/rust/example)

## **Chain of Responsibility** in Other Languages

[![Chain of Responsibility in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/chain-of-responsibility/csharp/example "Chain of Responsibility in C#") [![Chain of Responsibility in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/chain-of-responsibility/cpp/example "Chain of Responsibility in C++") [![Chain of Responsibility in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/chain-of-responsibility/go/example "Chain of Responsibility in Go") [![Chain of Responsibility in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/chain-of-responsibility/java/example "Chain of Responsibility in Java") [![Chain of Responsibility in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/chain-of-responsibility/php/example "Chain of Responsibility in PHP") [![Chain of Responsibility in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/chain-of-responsibility/python/example "Chain of Responsibility in Python") [![Chain of Responsibility in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/chain-of-responsibility/ruby/example "Chain of Responsibility in Ruby") [![Chain of Responsibility in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/chain-of-responsibility/swift/example "Chain of Responsibility in Swift") [![Chain of Responsibility in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/chain-of-responsibility/typescript/example "Chain of Responsibility in TypeScript")

![A banner image featuring various programming and design elements like code snippets, a tablet with UI mockups, a smartphone, and development tools, suggesting a comprehensive resource for design patterns and code examples.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [patient](#example-0--patient-rs)

 [department](#example-0--department-rs)

  [cashier](#example-0--department-cashier-rs)

  [doctor](#example-0--department-doctor-rs)

  [medical](#example-0--department-medical-rs)

  [reception](#example-0--department-reception-rs)

 [main](#example-0--main-rs)