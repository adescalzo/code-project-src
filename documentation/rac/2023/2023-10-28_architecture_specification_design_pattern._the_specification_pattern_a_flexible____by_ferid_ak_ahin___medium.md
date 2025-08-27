```yaml
---
title: "Specification Design Pattern. The Specification pattern a flexible… | by Ferid Akşahin | Medium"
source: https://medium.com/@ferid.aksahin98/specification-design-pattern-210b5df70252
date_published: 2023-10-28T14:37:21.396Z
date_captured: 2025-09-05T12:35:46.693Z
domain: medium.com
author: Ferid Akşahin
category: architecture
technologies: [.NET, GitHub]
programming_languages: [C#]
tags: [design-pattern, specification-pattern, csharp, object-oriented-programming, software-design, filtering, reusability, modularity, e-commerce, code-structure]
key_concepts: [Specification pattern, Composite pattern, Separation of Concerns, Method Injection, Lazy Loading, Object-Oriented Design, Interfaces, Generics]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article introduces the Specification design pattern, a flexible approach for evaluating objects against defined criteria. It explains how criteria are encapsulated into reusable specification objects, which can be combined using logical operators like AND, OR, and NOT to form complex rules. The benefits, such as flexibility, separation of concerns, and reusability, are discussed alongside drawbacks like increased complexity and potential performance considerations. A practical e-commerce example in C# demonstrates filtering products based on condition and popularity, showcasing both simple and composite specifications, method injection, and lazy loading. The implementation details for interfaces, classes, and their interactions are provided.]
---
```

# Specification Design Pattern. The Specification pattern a flexible… | by Ferid Akşahin | Medium

# Specification Design Pattern

The Specification pattern offers a flexible way to evaluate objects against a specific set of criteria or rules.

![Filter icon representing the concept of filtering or selection](https://miro.medium.com/v2/resize:fit:473/1*V-8HRbLmYmxyEZiFXcOc9w.png)

### Overview

With the Specification design pattern, criteria are encapsulated into separate specification objects. These specification objects define rules or conditions that an object must meet. By combining multiple specifications using logical operators (such as AND, OR, NOT), complex composite specifications can be made.

### Benefits

*   **Flexibility:** The Specification pattern provides dynamic and flexible rule evaluation by encapsulating criteria in separate specification objects. This allows for easy modification and composition of rules to meet different requirements.
*   **Separation of Concerns:** It supports a clean and modular codebase by separating the logic of evaluating specifications and rules from the objects themselves. Objects can focus on their primary responsibilities, while specifications handle the rule evaluation.
*   **Reusability:** Specifications can be reused across different objects and scenarios. Once defined, specifications can be applied to multiple objects to evaluate their compliance with the defined rules.

### Drawbacks

*   **Increased Complexity:** The Specification pattern introduces additional complexity, especially when dealing with complex composite specifications. Managing and combining multiple specifications may require careful consideration and additional code.
*   **Performance Considerations:** Evaluating composite specifications or complex rules can impact performance, particularly when dealing with a large number of objects or complex criteria. Care should be taken to optimize the evaluation process if performance is a concern.

### Modular Example

This section includes an example of the Specification design pattern implemented in a hypothetical e-commerce scenario. The goal is to focus on filtering products on an e-commerce website based on certain criteria, and the Specification design model is used to perform this filtering.

### Scenario

The scenario involves filtering products based on their condition (new, used, renewed) and popularity (trend, best seller, new arrival, recommended).

Let’s start with the enum files.

> The `Product` class takes 3 parameters in its constructor. With the given parameters, we determine the product’s name, product condition, and popularity.

> We use the `IsSatisfied` function of the `ISpecification<T>` interface to check whether an item meets a specific criteria rule or not.

> The `IFilter<T>` interface is used to filter a list of items based on a specific specification (`ISpecification`).

> The `ConditionSpecification` class is what we use for product status filtering. This class has a field of type `Condition`. It’s used to check if the condition of a product matches a specific value (e.g., New, Used, or Renewed). The condition parameter taken in its constructor is referenced for this check. The `ISpecification<Product>` interface has been implemented in this class. So we have to provide the `IsSatisfied` method.

> In the `FilterSpecification` class, there’s a loop within the `Filter` method where all products are checked against the given specification (`ISpecification<Product> spec`):

> This method takes a list of products (`IEnumerable<Product>`) as the first parameter and the specification (`ISpecification<Product>`) we want to apply as the second parameter.
>
> When the `ConditionSpecification` class is passed as a parameter to the `Filter` method in the `FilterSpecification` class, we invoke `ConditionSpecification`’s `IsSatisfied` method through **method injection**. This way, the desired product status filter is applied to the products provided to the `Filter` method.

The `PopularitySpecification` class is used to filter the product based on its popularity. It operates on the same logic as the `ConditionSpecification`.

For Example:

```csharp
var filter = new FilterSpecification();  
Console.WriteLine("New Products:");  
foreach (var product in filter.Filter(GetDummyProductData(),   
    new ConditionSpecification(Condition.New)))  
{  
    Console.WriteLine($" - {product.Name}");  
}
```

> We want to filter out products with the status ‘New’. We access the `Filter` method with an object of the `FilterSpecification` class type, our sample data, and an object from the `ConditionSpecification` class, passing the `New` enum value as a parameter to its constructor. Using **method injection** in the `Filter` method, the `IsSatisfied` method of the `ConditionSpecification` class gets executed, and among the products passed as a parameter to the `Filter` method, those with a `Condition` status of ‘New’ are returned with **yield return**, meaning they are returned using **lazy loading**.

Besides these simple features, we can write new features representing more complex criteria using logical operators ‘**and**’, ‘**or**’ and ‘**not**’.

For Example:

> This specification is for an ‘**and**’ filter that requires both properties to be true. This is a **Composite Filter**. In the context of filtering, a composite filter means **combining two or more filter** criteria to operate as a single filter. With the composite filter approach, we can easily make and manage complex filtering criteria.

As An Example Usage:

```csharp
var filter = new FilterSpecification();          
Console.WriteLine("Best Seller And Used Products:");  
foreach (var product in filter.Filter(GetDummyProductData(),   
    new AndSpecification<Product>(new PopularitySpecification(Popularity.BestSeller),  
    new ConditionSpecification(Condition.Used))))  
{  
    Console.WriteLine($" - {product.Name}");  
}
```

> We invoke the `Filter` method using the object we obtained from the `FilterSpecification` class and pass our sample products as a parameter. In this example, we use the `AndSpecification` class. This class has been written to allow us to combine two different specifications. Therefore, we send two specifications to the constructor of the `AndSpecification` class: `PopularitySpecification` and `ConditionSpecification`.

Since the `AndSpecification` class is of a generic structure, we need to specify on which type this specification will operate. In this example, since we want to filter based on the `Product` class, we specify the `Product` type as generic.

> When the `IsSatisfied` method of the `AndSpecification` class is executed, the `IsSatisfied` method of the `PopularitySpecification` is called first. The same applies for the `ConditionSpecification`, which we provided as the second parameter.

### Usage

Below are some sample scenarios for its use in the main method in the `Program.cs` file.

> Filtering for New Products:

```csharp
Console.WriteLine("New Products:");  
foreach (var product in filter.Filter(GetDummyProductData(), new ConditionSpecification(Condition.New)))  
{  
    Console.WriteLine($" - {product.Name}");  
}
```

> Filtering for Best Seller And Used Products:

```csharp
Console.WriteLine("Best Seller And Used Products:");  
foreach (var product in filter.Filter(GetDummyProductData(), new AndSpecification<Product>(new PopularitySpecification(Popularity.BestSeller),  
new ConditionSpecification(Condition.Used))))  
{  
    Console.WriteLine($" - {product.Name}");  
}
```

Thanks for reading.

[GitHub Project For Clone.](https://github.com/FeridAksahin/DesignPattern/tree/main/SpecificationDesignPattern)