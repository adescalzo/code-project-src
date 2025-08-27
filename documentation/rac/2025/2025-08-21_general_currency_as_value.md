```yaml
---
title: Currency As Value
source: https://martinfowler.com/bliki/CurrencyAsValue.html
date_published: unknown
date_captured: 2025-08-21T10:54:05.994Z
domain: martinfowler.com
author: Martin Fowler
category: general
technologies: []
programming_languages: []
tags: [value-object, currency, domain-driven-design, object-oriented-design, immutability, design-patterns, object-modeling]
key_concepts: [ValueObject, immutability, Reference Object, Domain-Driven Design, bi-semantic behavior, object-modeling]
code_examples: false
difficulty_level: intermediate
summary: |
  The article discusses the design of a `Currency` object, typically a `ValueObject` due to its immutable nature. It presents a unique scenario where a `Currency` required a mutable 'pip value', which conflicted with the `ValueObject` principle of immutability. The solution involved a "bi-semantic" approach, using two classes: an immutable `ValueObject` for the currency code and a mutable `Reference Object` for changing attributes like the pip value. The `ValueObject` delegated calls for mutable data to the `Reference Object`, which was often static data held in a lookup table. The author notes this bi-semantic behavior is unusual but effective for specific complex requirements.
---
```

# Currency As Value

# Currency As Value

26 August 2003

There are many common examples of [ValueObject](/bliki/ValueObject.html), my favorite is [Money](/eaaCatalog/money.html) - and one closely linked to Money is currency.

For many systems currency works well as a value, the key part you need is the internationally recognized currency code (such as USD for US Dollars).

However I was once involved with a system where it got a bit more interesting. If my memory serves me correctly, one of the things they wanted from their currency was a 'pip value'. On their UI they had nudge buttons to nudge the currencies up and down in value. Each currency had its own pip value, and this pip value could change. It didn't change often, but it did change. This violates a very useful rule of value objects - that they are immutable.

The solution we did was to have two currency classes. One was a value object, it held the currency code and maybe a couple of other immutable things. The second was a reference object (I think we called them something like CurrencyValue and CurrencyReference). The value object was passed around most of the time, but some methods in the value object, such as pip value, delegated to the reference object - which was mostly static data. The reference objects were held in a lookup table indexed by the currency code (that's not the only way to do it.)

This bi-semantic behavior is unusual - it's the only time I've seen it. Certainly most systems can get away with a simple value object for currency.