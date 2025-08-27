```yaml
---
title: Template Method in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/template-method/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:26:42.715Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Rust, C#, C++, Go, Java, PHP, Python, Ruby, Swift, TypeScript]
programming_languages: [Rust]
tags: [design-pattern, behavioral-pattern, template-method, rust, software-design, code-example, abstraction, object-oriented-programming]
key_concepts: [template-method-pattern, behavioral-design-pattern, algorithm-skeleton, trait-implementation, hooks, polymorphism]
code_examples: false
difficulty_level: intermediate
summary: |
  This article introduces the Template Method, a behavioral design pattern that structures an algorithm's core in a base class while allowing subclasses to customize specific steps. It presents a conceptual example implemented in Rust, utilizing traits to define the template method, base operations, and customizable required operations and hooks. The example demonstrates how different concrete structs can implement the required steps, showcasing the pattern's flexibility. The provided client code illustrates how to interact with these concrete implementations polymorphically, producing distinct outputs based on the specific implementation.
---
```

# Template Method in Rust / Design Patterns

# **Template Method** in Rust

**Template Method** is a behavioral design pattern that allows you to define a skeleton of an algorithm in a base class and let subclasses override the steps without changing the overall algorithm’s structure.

## Conceptual Example

#### **main.rs**

```rust
trait TemplateMethod {
    fn template_method(&self) {
        self.base_operation1();
        self.required_operations1();
        self.base_operation2();
        self.hook1();
        self.required_operations2();
        self.base_operation3();
        self.hook2();
    }

    fn base_operation1(&self) {
        println!("TemplateMethod says: I am doing the bulk of the work");
    }

    fn base_operation2(&self) {
        println!("TemplateMethod says: But I let subclasses override some operations");
    }

    fn base_operation3(&self) {
        println!("TemplateMethod says: But I am doing the bulk of the work anyway");
    }

    fn hook1(&self) {}
    fn hook2(&self) {}

    fn required_operations1(&self);
    fn required_operations2(&self);
}

struct ConcreteStruct1;

impl TemplateMethod for ConcreteStruct1 {
    fn required_operations1(&self) {
        println!("ConcreteStruct1 says: Implemented Operation1")
    }

    fn required_operations2(&self) {
        println!("ConcreteStruct1 says: Implemented Operation2")
    }
}

struct ConcreteStruct2;

impl TemplateMethod for ConcreteStruct2 {
    fn required_operations1(&self) {
        println!("ConcreteStruct2 says: Implemented Operation1")
    }

    fn required_operations2(&self) {
        println!("ConcreteStruct2 says: Implemented Operation2")
    }
}

fn client_code(concrete: impl TemplateMethod) {
    concrete.template_method()
}

fn main() {
    println!("Same client code can work with different concrete implementations:");
    client_code(ConcreteStruct1);
    println!();

    println!("Same client code can work with different concrete implementations:");
    client_code(ConcreteStruct2);
}
```

### Output

```
Same client code can work with different concrete implementations:
TemplateMethod says: I am doing the bulk of the work
ConcreteStruct1 says: Implemented Operation1
TemplateMethod says: But I let subclasses override some operations
ConcreteStruct1 says: Implemented Operation2
TemplateMethod says: But I am doing the bulk of the work anyway

Same client code can work with different concrete implementations:
TemplateMethod says: I am doing the bulk of the work
ConcreteStruct2 says: Implemented Operation1
TemplateMethod says: But I let subclasses override some operations
ConcreteStruct2 says: Implemented Operation2
TemplateMethod says: But I am doing the bulk of the work anyway
```

---
**Image Analysis:**

The image is an abstract, illustrative representation of software development and data processing. It features a central tablet displaying a user interface with charts and data, surrounded by various elements like smaller screens, code snippets (`</>`), gears, and development tools such as a hammer and wrench. A mobile phone UI is also visible. The overall composition suggests interconnected systems, data flow, and the process of building and interacting with software applications. The color scheme is predominantly blue, orange, and grey against a dark background.