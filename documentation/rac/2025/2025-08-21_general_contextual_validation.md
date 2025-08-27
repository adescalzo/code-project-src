```yaml
---
title: Contextual Validation
source: https://martinfowler.com/bliki/ContextualValidation.html
date_published: unknown
date_captured: 2025-08-21T10:56:29.023Z
domain: martinfowler.com
author: Martin Fowler
category: general
technologies: []
programming_languages: []
tags: [validation, domain-driven-design, application-architecture, software-design, data-integrity, business-rules, design-patterns, object-oriented]
key_concepts: [contextual-validation, domain-driven-design, object-validity, application-architecture, data-integrity, business-rules]
code_examples: false
difficulty_level: intermediate
summary: |
  This article by Martin Fowler discusses the common pitfalls of object validation, particularly the idea of a context-independent `isValid` method. Fowler argues that validation should instead be tied to specific actions or contexts, such as `isValidForCheckIn`. He challenges the notion that objects must always be perfectly valid before saving to a database, suggesting that preventing saving due to errors might be overly restrictive in some scenarios. The piece encourages a more nuanced approach to validation, considering the purpose and context of the data.
---
```

# Contextual Validation

# [Contextual Validation](ContextualValidation.html)

7 December 2005

[![](/mf.jpg "Photo of Martin Fowler")](/)

[Martin Fowler](/)

[domain driven design](/tags/domain%20driven%20design.html)

[application architecture](/tags/application%20architecture.html)

In my writing endeavors, I've long intended to write a chunk of material on validation. It's an area that leads to a lot of confusion and it would be good to get some solid description of some of the techniques that work well. However life is full of things to write about, rather more than time allows.

Some recent readings made me think about saying a few preliminary things on the topic. One common thing I see people do is to develop validation routines for objects. These routines come in various ways, they may be in the object or external, they may return a boolean or throw an exception to indicate failure. But one thing that I think constantly trips people up is when they think object validity on a context independent way such as an `isValid` method implies.

I think it's much more useful to think of validation as something that's bound to a context - typically an action that you want to do. Is this order valid to be filled, is this customer valid to check in to the hotel. So rather than have methods like `isValid` have methods like `isValidForCheckIn`.

One of the consequences of this is that saving an object to a database is itself an action. Thinking about it that way raises some important questions. Often when people talk about a context-free validity, they mean it in terms of saving to a database. But the various validity checks that make this up should be interrogated with the question “should failing this test prevent saving?”

In [About Face](https://www.amazon.com/gp/product/1568843224/ref=as_li_tl?ie=UTF8&camp=1789&creative=9325&creativeASIN=1568843224&linkCode=as2&tag=martinfowlerc-20) Alan Cooper advocated that we shouldn't let our ideas of valid states prevent a user from entering (and saving) incomplete information. I was reminded by this a few days ago when reading a draft of a book that [Jimmy Nilsson](http://www.jnsk.se/weblog/rss.xml) is working on. He stated a principle that you should always be able to save an object, even if it has errors in it. While I'm not convinced that this should be an absolute rule, I do think people tend to prevent saving more than they ought. Thinking about the context for validation may help prevent that.

reposted on 03 Nov 2011