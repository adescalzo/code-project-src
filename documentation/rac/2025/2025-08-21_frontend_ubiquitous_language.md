```yaml
---
title: Ubiquitous Language
source: https://martinfowler.com/bliki/UbiquitousLanguage.html
date_published: unknown
date_captured: 2025-08-21T10:52:50.451Z
domain: martinfowler.com
author: Martin Fowler
category: frontend
technologies: []
programming_languages: []
tags: [domain-driven-design, ddd, ubiquitous-language, domain-modeling, software-development, communication, shared-understanding]
key_concepts: [ubiquitous-language, domain-driven-design, domain-model, communication, software-design, shared-understanding, iterative-development]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article introduces the concept of Ubiquitous Language, a term coined by Eric Evans in Domain Driven Design. It describes the practice of establishing a common, rigorous language between developers and users, based on the software's Domain Model. The language is crucial for clear communication and for testing and evolving the domain model as the team's understanding of the domain deepens. Both domain experts and developers are encouraged to refine this language to ensure it accurately conveys domain understanding and avoids ambiguity in software design.]
---
```

# Ubiquitous Language

# [Ubiquitous Language](UbiquitousLanguage.html)

31 October 2006

[![](/mf.jpg "Photo of Martin Fowler")](/)

[Martin Fowler](/)

[domain driven design](/tags/domain%20driven%20design.html)

Ubiquitous Language is the term Eric Evans uses in [Domain Driven Design](https://www.amazon.com/gp/product/0321125215/ref=as_li_tl?ie=UTF8&camp=1789&creative=9325&creativeASIN=0321125215&linkCode=as2&tag=martinfowlerc-20) for the practice of building up a common, rigorous language between developers and users. This language should be based on the [Domain Model](/eaaCatalog/domainModel.html) used in the software - hence the need for it to be rigorous, since software doesn't cope well with ambiguity.

Evans makes clear that using the ubiquitous language in conversations with domain experts is an important part of testing it, and hence the domain model. He also stresses that the language (and model) should evolve as the team's understanding of the domain grows.

> By using the model-based language pervasively and not being satisfied until it flows, we approach a model that is complete and comprehensible, made up of simple elements that combine to express complex ideas.
> 
> ...
> 
> Domain experts should object to terms or structures that are awkward or inadequate to convey domain understanding; developers should watch for ambiguity or inconsistency that will trip up design.
> 
> \-- Eric Evans