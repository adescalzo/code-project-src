```yaml
---
title: Facade in Rust / Design Patterns
source: https://refactoring.guru/design-patterns/facade/rust/example#example-0
date_published: 2024-01-01T00:00:00.000Z
date_captured: 2025-08-18T13:29:46.531Z
domain: refactoring.guru
author: Unknown
category: architecture
technologies: [Rust]
programming_languages: [Rust]
tags: [design-patterns, structural-patterns, facade-pattern, rust, software-design, code-organization, complexity-management, software-architecture]
key_concepts: [facade-pattern, structural-design-pattern, complexity-reduction, simplified-interface, dependency-management, subsystem-abstraction]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the Facade design pattern, a structural pattern that simplifies interaction with a complex subsystem by providing a unified, high-level interface. It explains how Facade reduces overall application complexity and centralizes unwanted dependencies. The core of the article features a practical implementation of the Facade pattern in Rust, using a `WalletFacade` to manage interactions with underlying `Account`, `Wallet`, `SecurityCode`, `Notification`, and `Ledger` components. Detailed Rust code examples demonstrate how the facade simplifies operations like adding or deducting money from a wallet, abstracting away the intricate logic of multiple internal classes.]
---
```

# Facade in Rust / Design Patterns

[](/)/ [Design Patterns](/design-patterns) / [Facade](/design-patterns/facade) / [Rust](/design-patterns/rust)

[Image: An icon representing the Facade design pattern, depicted as a stylized red building with columns.]

# **Facade** in Rust

**Facade** is a structural design pattern that provides a simplified (but limited) interface to a complex system of classes, library or framework.

While Facade decreases the overall complexity of the application, it also helps to move unwanted dependencies to one place.

[Learn more about Facade](/design-patterns/facade)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [wallet\_facade](#example-0--wallet_facade-rs)

 [wallet](#example-0--wallet-rs)

 [account](#example-0--account-rs)

 [ledger](#example-0--ledger-rs)

 [notification](#example-0--notification-rs)

 [security\_code](#example-0--security_code-rs)

 [main](#example-0--main-rs)

## Conceptual Example

`pub struct WalletFacade` hides a complex logic behind its API. A single method `add_money_to_wallet` interacts with the account, code, wallet, notification and ledger behind the scenes.

#### [](#example-0--wallet_facade-rs)**wallet\_facade.rs**

```rust
use crate::{
    account::Account, ledger::Ledger, notification::Notification, security_code::SecurityCode,
    wallet::Wallet,
};

/// Facade hides a complex logic behind the API.
pub struct WalletFacade {
    account: Account,
    wallet: Wallet,
    code: SecurityCode,
    notification: Notification,
    ledger: Ledger,
}

impl WalletFacade {
    pub fn new(account_id: String, code: u32) -> Self {
        println!("Starting create account");

        let this = Self {
            account: Account::new(account_id),
            wallet: Wallet::new(),
            code: SecurityCode::new(code),
            notification: Notification,
            ledger: Ledger,
        };

        println!("Account created");
        this
    }

    pub fn add_money_to_wallet(
        &mut self,
        account_id: &String,
        security_code: u32,
        amount: u32,
    ) -> Result<(), String> {
        println!("Starting add money to wallet");
        self.account.check(account_id)?;
        self.code.check(security_code)?;
        self.wallet.credit_balance(amount);
        self.notification.send_wallet_credit_notification();
        self.ledger.make_entry(account_id, "credit".into(), amount);
        Ok(())
    }

    pub fn deduct_money_from_wallet(
        &mut self,
        account_id: &String,
        security_code: u32,
        amount: u32,
    ) -> Result<(), String> {
        println!("Starting debit money from wallet");
        self.account.check(account_id)?;
        self.code.check(security_code)?;
        self.wallet.debit_balance(amount);
        self.notification.send_wallet_debit_notification();
        self.ledger.make_entry(account_id, "debit".into(), amount);
        Ok(())
    }
}
```

#### [](#example-0--wallet-rs)**wallet.rs**

```rust
pub struct Wallet {
    balance: u32,
}

impl Wallet {
    pub fn new() -> Self {
        Self { balance: 0 }
    }

    pub fn credit_balance(&mut self, amount: u32) {
        self.balance += amount;
    }

    pub fn debit_balance(&mut self, amount: u32) {
        self.balance
            .checked_sub(amount)
            .expect("Balance is not sufficient");
    }
}
```

#### [](#example-0--account-rs)**account.rs**

```rust
pub struct Account {
    name: String,
}

impl Account {
    pub fn new(name: String) -> Self {
        Self { name }
    }

    pub fn check(&self, name: &String) -> Result<(), String> {
        if &self.name != name {
            return Err("Account name is incorrect".into());
        }

        println!("Account verified");
        Ok(())
    }
}
```

#### [](#example-0--ledger-rs)**ledger.rs**

```rust
pub struct Ledger;

impl Ledger {
    pub fn make_entry(&mut self, account_id: &String, txn_type: String, amount: u32) {
        println!(
            "Make ledger entry for accountId {} with transaction type {} for amount {}",
            account_id, txn_type, amount
        );
    }
}
```

#### [](#example-0--notification-rs)**notification.rs**

```rust
pub struct Notification;

impl Notification {
    pub fn send_wallet_credit_notification(&self) {
        println!("Sending wallet credit notification");
    }

    pub fn send_wallet_debit_notification(&self) {
        println!("Sending wallet debit notification");
    }
}
```

#### [](#example-0--security_code-rs)**security\_code.rs**

```rust
pub struct SecurityCode {
    code: u32,
}

impl SecurityCode {
    pub fn new(code: u32) -> Self {
        Self { code }
    }

    pub fn check(&self, code: u32) -> Result<(), String> {
        if self.code != code {
            return Err("Security code is incorrect".into());
        }

        println!("Security code verified");
        Ok(())
    }
}
```

#### [](#example-0--main-rs)**main.rs**

```rust
mod account;
mod ledger;
mod notification;
mod security_code;
mod wallet;
mod wallet_facade;

use wallet_facade::WalletFacade;

fn main() -> Result<(), String> {
    let mut wallet = WalletFacade::new("abc".into(), 1234);
    println!();

    // Wallet Facade interacts with the account, code, wallet, notification and
    // ledger behind the scenes.
    wallet.add_money_to_wallet(&"abc".into(), 1234, 10)?;
    println!();

    wallet.deduct_money_from_wallet(&"abc".into(), 1234, 5)
}
```

### Output

```
Starting create account
Account created

Starting add money to wallet
Account verified
Security code verified
Sending wallet credit notification
Make ledger entry for accountId abc with transaction type credit for amount 10

Starting debit money from wallet
Account verified
Security code verified
Sending wallet debit notification
Make ledger entry for accountId abc with transaction type debit for amount 5
```

#### Read next

[Flyweight in Rust](/design-patterns/flyweight/rust/example) 

#### Return

 [Decorator in Rust](/design-patterns/decorator/rust/example)

## **Facade** in Other Languages

[Image: Icon representing the C# programming language.] [Image: Icon representing the C++ programming language.] [Image: Icon representing the Go programming language.] [Image: Icon representing the Java programming language.] [Image: Icon representing the PHP programming language.] [Image: Icon representing the Python programming language.] [Image: Icon representing the Ruby programming language.] [Image: Icon representing the Swift programming language.] [Image: Icon representing the TypeScript programming language.]

[Image: A banner image showing a desktop computer, tablet, and smartphone displaying code and various development-related icons, promoting an eBook about design patterns.]

[Archive with examples](/design-patterns/book)

Buy the eBook **Dive Into Design Patterns** and get the access to archive with dozens of detailed examples that can be opened right in your IDE.

[Learn more…](/design-patterns/book)

Navigation

 [Intro](#)

 [Conceptual Example](#example-0)

 [wallet\_facade](#example-0--wallet_facade-rs)

 [wallet](#example-0--wallet-rs)

 [account](#example-0--account-rs)

 [ledger](#example-0--ledger-rs)

 [notification](#example-0--notification-rs)

 [security\_code](#example-0--security_code-rs)

 [main](#example-0--main-rs)