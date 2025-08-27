```yaml
---
title: Composite in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/composite/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:30:20.365Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: []
programming_languages: []
tags: []
key_concepts: []
code_examples: true
difficulty_level: intermediate
summary: |
  The article introduces the Composite design pattern, a structural pattern that allows composing objects into tree structures and treating individual objects and compositions uniformly. It demonstrates the pattern's implementation in Rust using a file system example, where `File` and `Folder` types both implement a common `Component` trait. This enables recursive operations, such as searching, across the entire hierarchical structure. The provided Rust code illustrates the trait definition, struct implementations, and a `main` function showcasing how to build and interact with the composite structure.
---
```

# Composite in Rust / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Composite](/design-patterns/composite) / [Rust](/design-patterns/rust)

![Composite Pattern Diagram: A tree-like structure with nodes representing individual objects and composite objects, illustrating how they are composed into a hierarchy.](/images/patterns/cards/composite-mini.png?id=a369d98d18b417f255d04568fd0131b8)

# **Composite** in Rust

**Composite** is a structural design pattern that lets you compose objects into tree structures and then work with these structures as if they were individual objects.

Composite became a pretty popular solution for the most problems that require building a tree structure. Composite’s great feature is the ability to run methods recursively over the whole tree structure and sum up the results.

[Learn more about Composite](/design-patterns/composite)

Navigation

 [Intro](#)

 [Files and Folders](#example-0)

  [mod](#example-0--fs-mod-rs)

  [file](#example-0--fs-file-rs)

  [folder](#example-0--fs-folder-rs)

 [main](#example-0--main-rs)

## Files and Folders

Let’s try to understand the Composite pattern with an example of an operating system’s file system. In the file system, there are two types of objects: files and folders. There are cases when files and folders should be treated to be the same way. This is where the Composite pattern comes in handy.

`File` and `Directory` are both of the `trait Component` with a single `search` method. For a file, it will just look into the contents of the file; for a folder, it will go through all files of that folder to find that keyword.

#### [](#example-0--fs-mod-rs)**fs/mod.rs**

```rust
mod file;
mod folder;

pub use file::File;
pub use folder::Folder;

pub trait Component {
    fn search(&self, keyword: &str);
}
```

#### [](#example-0--fs-file-rs)**fs/file.rs**

```rust
use super::Component;

pub struct File {
    name: &'static str,
}

impl File {
    pub fn new(name: &'static str) -> Self {
        Self { name }
    }
}

impl Component for File {
    fn search(&self, keyword: &str) {
        println!("Searching for keyword {} in file {}", keyword, self.name);
    }
}
```

#### [](#example-0--fs-folder-rs)**fs/folder.rs**

```rust
use super::Component;

pub struct Folder {
    name: &'static str,
    components: Vec<Box<dyn Component>>,
}

impl Folder {
    pub fn new(name: &'static str) -> Self {
        Self {
            name,
            components: vec![],
        }
    }

    pub fn add(&mut self, component: impl Component + 'static) {
        self.components.push(Box::new(component));
    }
}

impl Component for Folder {
    fn search(&self, keyword: &str) {
        println!(
            "Searching recursively for keyword {} in folder {}",
            keyword, self.name
        );

        for component in self.components.iter() {
            component.search(keyword);
        }
    }
}
```

#### [](#example-0--main-rs)**main.rs**

```rust
mod fs;

use fs::{Component, File, Folder};

fn main() {
    let file1 = File::new("File 1");
    let file2 = File::new("File 2");
    let file3 = File::new("File 3");

    let mut folder1 = Folder::new("Folder 1");
    folder1.add(file1);

    let mut folder2 = Folder::new("Folder 2");
    folder2.add(file2);
    folder2.add(file3);
    folder2.add(folder1);

    folder2.search("rose");
}
```

### Output

```
Searching recursively for keyword rose in folder Folder 2
Searching for keyword rose in file File 2
Searching for keyword rose in file File 3
Searching recursively for keyword rose in folder Folder 1
Searching for keyword rose in file File 1
```
------------------------------------

#### Read next

[Decorator in Rust](/design-patterns/decorator/rust/example) 

#### Return

 [Bridge in Rust](/design-patterns/bridge/rust/example)

## **Composite** in Other Languages

[![Composite in C#](/images/patterns/icons/csharp.svg?id=da64592defc6e86d57c39c66e9de3e58)](/design-patterns/composite/csharp/example "Composite in C#") [![Composite in C++](/images/patterns/icons/cpp.svg?id=f7782ed8b8666246bfcc3f8fefc3b858)](/design-patterns/composite/cpp/example "Composite in C++") [![Composite in Go](/images/patterns/icons/go.svg?id=1a89927eb99b1ea3fde7701d97970aca)](/design-patterns/composite/go/example "Composite in Go") [![Composite in Java](/images/patterns/icons/java.svg?id=e6d87e2dca08c953fe3acd1275ed4f4e)](/design-patterns/composite/java/example "Composite in Java") [![Composite in PHP](/images/patterns/icons/php.svg?id=be1906eb26b71ec1d3b93720d6156618)](/design-patterns/composite/php/example "Composite in PHP") [![Composite in Python](/images/patterns/icons/python.svg?id=6d815d43c0f7050a1151b43e51569c9f)](/design-patterns/composite/python/example "Composite in Python") [![Composite in Ruby](/images/patterns/icons/ruby.svg?id=b065b718c914bf8e960ef731600be1eb)](/design-patterns/composite/ruby/example "Composite in Ruby") [![Composite in Swift](/images/patterns/icons/swift.svg?id=0b716c2d52ec3a48fbe91ac031070c1d)](/design-patterns/composite/swift/example "Composite in Swift") [![Composite in TypeScript](/images/patterns/icons/typescript.svg?id=2239d0f16cb703540c205dd8cb0c0cb7)](/design-patterns/composite/typescript/example "Composite in TypeScript")

![Promotional banner for "Dive Into Design Patterns" eBook, showing various development tools, code snippets, and devices like a desktop, tablet, and smartphone, suggesting comprehensive examples for an IDE.](/images/patterns/banners/examples-ide.png?id=3115b4b548fb96b75974e2de8f4f49bc)

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Files and Folders](#example-0)

  [mod](#example-0--fs-mod-rs)

  [file](#example-0--fs-file-rs)

  [folder](#example-0--fs-folder-rs)

 [main](#example-0--main-rs)