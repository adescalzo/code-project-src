```yaml
---
title: Bridge in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/bridge/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:40:48.045Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Rust, "std::cmp"]
programming_languages: [Rust]
tags: [design-patterns, structural-pattern, bridge-pattern, rust, software-architecture, abstraction, implementation, object-oriented-programming]
key_concepts: [bridge-pattern, abstraction, implementation, trait-based-programming, separation-of-concerns, design-patterns]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Bridge structural design pattern, demonstrating its application using Rust. The pattern aims to decouple an abstraction from its implementation, allowing both to vary independently. The content provides a practical example of managing various devices (TV, Radio) with different remote controls (Basic, Advanced) by separating the device logic from the remote control logic using Rust traits. This clear separation illustrates how the Bridge pattern enhances flexibility and maintainability in software design. The code examples showcase the pattern's structure and behavior in a real-world scenario.]
---
```

# Bridge in Rust / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Bridge](/design-patterns/bridge) / [Rust](/design-patterns/rust)

![Bridge - An icon representing a bridge structure, symbolizing the Bridge design pattern.](/images/patterns/cards/bridge-mini.png?id=b389101d8ee8e23ffa1b534c704d0774)

# **Bridge** in Rust

**Bridge** is a structural design pattern that divides business logic or huge class into separate class hierarchies that can be developed independently.

One of these hierarchies (often called the Abstraction) will get a reference to an object of the second hierarchy (Implementation). The abstraction will be able to delegate some (sometimes, most) of its calls to the implementations object. Since all implementations will have a common interface, they’d be interchangeable inside the abstraction.

[Learn more about Bridge](/design-patterns/bridge)

Navigation

 [Intro](#)

 [Devices and Remotes](#example-0)

  [mod](#example-0--remotes-mod-rs)

  [basic](#example-0--remotes-basic-rs)

  [advanced](#example-0--remotes-advanced-rs)

  [mod](#example-0--device-mod-rs)

  [radio](#example-0--device-radio-rs)

  [tv](#example-0--device-tv-rs)

 [main](#example-0--main-rs)

## Devices and Remotes

This example illustrates how the Bridge pattern can help divide the monolithic code of an app that manages devices and their remote controls. The Device classes act as the implementation, whereas the Remotes act as the abstraction.

#### [](#example-0--remotes-mod-rs)**remotes/mod.rs**

```rust
mod advanced;
mod basic;

pub use advanced::AdvancedRemote;
pub use basic::BasicRemote;

use crate::device::Device;

pub trait HasMutableDevice<D: Device> {
    fn device(&mut self) -> &mut D;
}

pub trait Remote<D: Device>: HasMutableDevice<D> {
    fn power(&mut self) {
        println!("Remote: power toggle");
        if self.device().is_enabled() {
            self.device().disable();
        } else {
            self.device().enable();
        }
    }

    fn volume_down(&mut self) {
        println!("Remote: volume down");
        let volume = self.device().volume();
        self.device().set_volume(volume - 10);
    }

    fn volume_up(&mut self) {
        println!("Remote: volume up");
        let volume = self.device().volume();
        self.device().set_volume(volume + 10);
    }

    fn channel_down(&mut self) {
        println!("Remote: channel down");
        let channel = self.device().channel();
        self.device().set_channel(channel - 1);
    }

    fn channel_up(&mut self) {
        println!("Remote: channel up");
        let channel = self.device().channel();
        self.device().set_channel(channel + 1);
    }
}
```

#### [](#example-0--remotes-basic-rs)**remotes/basic.rs**

```rust
use crate::device::Device;

use super::{HasMutableDevice, Remote};

pub struct BasicRemote<D: Device> {
    device: D,
}

impl<D: Device> BasicRemote<D> {
    pub fn new(device: D) -> Self {
        Self { device }
    }
}

impl<D: Device> HasMutableDevice<D> for BasicRemote<D> {
    fn device(&mut self) -> &mut D {
        &mut self.device
    }
}

impl<D: Device> Remote<D> for BasicRemote<D> {}
```

#### [](#example-0--remotes-advanced-rs)**remotes/advanced.rs**

```rust
use crate::device::Device;

use super::{HasMutableDevice, Remote};

pub struct AdvancedRemote<D: Device> {
    device: D,
}

impl<D: Device> AdvancedRemote<D> {
    pub fn new(device: D) -> Self {
        Self { device }
    }

    pub fn mute(&mut self) {
        println!("Remote: mute");
        self.device.set_volume(0);
    }
}

impl<D: Device> HasMutableDevice<D> for AdvancedRemote<D> {
    fn device(&mut self) -> &mut D {
        &mut self.device
    }
}

impl<D: Device> Remote<D> for AdvancedRemote<D> {}
```

#### [](#example-0--device-mod-rs)**device/mod.rs**

```rust
mod radio;
mod tv;

pub use radio::Radio;
pub use tv::Tv;

pub trait Device {
    fn is_enabled(&self) -> bool;
    fn enable(&mut self);
    fn disable(&mut self);
    fn volume(&self) -> u8;
    fn set_volume(&mut self, percent: u8);
    fn channel(&self) -> u16;
    fn set_channel(&mut self, channel: u16);
    fn print_status(&self);
}
```

#### [](#example-0--device-radio-rs)**device/radio.rs**

```rust
use super::Device;

#[derive(Clone)]
pub struct Radio {
    on: bool,
    volume: u8,
    channel: u16,
}

impl Default for Radio {
    fn default() -> Self {
        Self {
            on: false,
            volume: 30,
            channel: 1,
        }
    }
}

impl Device for Radio {
    fn is_enabled(&self) -> bool {
        self.on
    }

    fn enable(&mut self) {
        self.on = true;
    }

    fn disable(&mut self) {
        self.on = false;
    }

    fn volume(&self) -> u8 {
        self.volume
    }

    fn set_volume(&mut self, percent: u8) {
        self.volume = std::cmp::min(percent, 100);
    }

    fn channel(&self) -> u16 {
        self.channel
    }

    fn set_channel(&mut self, channel: u16) {
        self.channel = channel;
    }

    fn print_status(&self) {
        println!("------------------------------------");
        println!("| I'm radio.");
        println!("| I'm {}", if self.on { "enabled" } else { "disabled" });
        println!("| Current volume is {}%", self.volume);
        println!("| Current channel is {}", self.channel);
        println!("------------------------------------\n");
    }
}
```

#### [](#example-0--device-tv-rs)**device/tv.rs**

```rust
use super::Device;

#[derive(Clone)]
pub struct Tv {
    on: bool,
    volume: u8,
    channel: u16,
}

impl Default for Tv {
    fn default() -> Self {
        Self {
            on: false,
            volume: 30,
            channel: 1,
        }
    }
}

impl Device for Tv {
    fn is_enabled(&self) -> bool {
        self.on
    }

    fn enable(&mut self) {
        self.on = true;
    }

    fn disable(&mut self) {
        self.on = false;
    }

    fn volume(&self) -> u8 {
        self.volume
    }

    fn set_volume(&mut self, percent: u8) {
        self.volume = std::cmp::min(percent, 100);
    }

    fn channel(&self) -> u16 {
        self.channel
    }

    fn set_channel(&mut self, channel: u16) {
        self.channel = channel;
    }

    fn print_status(&self) {
        println!("------------------------------------");
        println!("| I'm TV set.");
        println!("| I'm {}", if self.on { "enabled" } else { "disabled" });
        println!("| Current volume is {}%", self.volume);
        println!("| Current channel is {}", self.channel);
        println!("------------------------------------\n");
    }
}
```

#### [](#example-0--main-rs)**main.rs**

```rust
mod device;
mod remotes;

use device::{Device, Radio, Tv};
use remotes::{AdvancedRemote, BasicRemote, HasMutableDevice, Remote};

fn main() {
    test_device(Tv::default());
    test_device(Radio::default());
}

fn test_device(device: impl Device + Clone) {
    println!("Tests with basic remote.");
    let mut basic_remote = BasicRemote::new(device.clone());
    basic_remote.power();
    basic_remote.device().print_status();

    println!("Tests with advanced remote.");
    let mut advanced_remote = AdvancedRemote::new(device);
    advanced_remote.power();
    advanced_remote.mute();
    advanced_remote.device().print_status();
}
```

### Output

```
Tests with basic remote.
Remote: power toggle
------------------------------------
| I'm TV set.
| I'm enabled
| Current volume is 30%
| Current channel is 1
------------------------------------

Tests with advanced remote.
Remote: power toggle
Remote: mute
------------------------------------
| I'm TV set.
| I'm enabled
| Current volume is 0%
| Current channel is 1
------------------------------------

Tests with basic remote.
Remote: power toggle
------------------------------------
| I'm radio.
| I'm enabled
| Current volume is 30%
| Current channel is 1
------------------------------------

Tests with advanced remote.
Remote: power toggle
Remote: mute
------------------------------------
| I'm radio.
| I'm enabled
| Current volume is 0%
| Current channel is 1
------------------------------------
```

#### Read next

[Composite in Rust](/design-patterns/composite/rust/example) 

#### Return

 [Adapter in Rust](/design-patterns/adapter/rust/example)

## **Bridge** in Other Languages

[![Bridge in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58 "Bridge in C#")](/design-patterns/bridge/csharp/example) [![Bridge in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858 "Bridge in C++")](/design-patterns/bridge/cpp/example) [![Bridge in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca "Bridge in Go")](/design-patterns/bridge/go/example) [![Bridge in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e "Bridge in Java")](/design-patterns/bridge/java/example) [![Bridge in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618 "Bridge in PHP")](/design-patterns/bridge/php/example) [![Bridge in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f "Bridge in Python")](/design-patterns/bridge/python/example) [![Bridge in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb "Bridge in Ruby")](/design-patterns/bridge/ruby/example) [![Bridge in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d "Bridge in Swift")](/design-patterns/bridge/swift/example) [![Bridge in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7 "Bridge in TypeScript")](/design-patterns/bridge/typescript/example)

[![A banner image depicting various development tools and interfaces, including a laptop, tablet, and smartphone displaying code and UI elements, suggesting a comprehensive development environment or a collection of examples.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)](/design-patterns/book)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Devices and Remotes](#example-0)

  [mod](#example-0--remotes-mod-rs)

  [basic](#example-0--remotes-basic-rs)

  [advanced](#example-0--remotes-advanced-rs)

  [mod](#example-0--device-mod-rs)

  [radio](#example-0--device-radio-rs)

  [tv](#example-0--device-tv-rs)

 [main](#example-0--main-rs)