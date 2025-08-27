```yaml
---
title: "Specification Design Pattern. I’ve recently been studying the… | by Smriti Agrawal | Medium"
source: https://medium.com/@smritiagrawal15/specification-design-pattern-0424995cb8df
date_published: 2025-04-20T11:42:49.882Z
date_captured: 2025-09-05T12:36:17.668Z
domain: medium.com
author: Smriti Agrawal
category: architecture
technologies: []
programming_languages: [Java]
tags: [design-pattern, specification-pattern, software-design, clean-code, maintainability, extensibility, java, object-oriented-programming, business-rules, filtering]
key_concepts: [Specification Pattern, Open/Closed Principle, Single Responsibility Principle, code reusability, composability, separation of concerns, business logic encapsulation]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article introduces the Specification Design Pattern, defining it as a method to encapsulate and combine business rules independently from domain objects. It highlights common challenges with ad-hoc filtering logic, such as code modification, duplication, and scalability issues. The author then provides a step-by-step implementation in Java, demonstrating how to create reusable specifications and combine them using an `AndSpecification`. The pattern promotes extensibility, reduces code duplication, and adheres to the Single Responsibility Principle, making complex filtering logic more maintainable. While it adds some initial complexity, the benefits outweigh the trade-offs as an application grows.]
---
```

# Specification Design Pattern. I’ve recently been studying the… | by Smriti Agrawal | Medium

# Specification Design Pattern

![File folder tab labeled "Specifications" in a row of other document tabs.](https://miro.medium.com/v2/resize:fit:700/1*0xlYVGmWD5CFJZPO-VUPtw.jpeg)

I’ve recently been studying the Specification Design Pattern, and I’d like to share my insights and findings with you.

## Definition of Specification Design Pattern

> The **Specification Pattern** is a design pattern that is used to encapsulate business rules or criteria. It allows you to separate complex business logic from the domain objects (such as entities or models) by defining specifications that can be combined and reused independently.

The core concept of the Specification Pattern is to separate the logic for matching a candidate from the candidate object itself. Let’s break it down.

Imagine we have a class called `Product`, which looks like this:

```java
class Product {
    String name;
    String description;
    String category;
    double price;

    public Product(String name, String description, String category, double price) {
        this.name = name;
        this.description = description;
        this.category = category;
        this.price = price;
    }
}
```

And suppose we want to retrieve all products in the “Apparel” category with a price of less than 1000 rupees. How could we typically implement this?

```java
public List<Product> getFilteredProducts(List<Product> products) {
    List<Product> filteredProducts = new ArrayList<>();

    for (Product product : products) {
        if ("apparel".equals(product.category) && product.price < 1000) {
            filteredProducts.add(product);
        }
    }
    return filteredProducts;
}
```

It seems simple, right?

For a small application, this approach works perfectly fine. But what happens when our application grows and new filtering logic is needed? Or when multiple people need to work with this code? Let’s explore the challenges.

### Challenges with the above approach:

1.  **Modification of Existing Code**: What if we want to add another filter, like filtering by brand or rating? We’d have to modify this method each time — violating the “open for extension, closed for modification” principle.
2.  **Code Duplication**: The filtering logic is embedded within this method. Want to filter by price elsewhere? Or by category? We’d have to copy-paste or repeat the logic. This might lead to inconsistencies.
3.  **Multiple Responsibilities**: Right now, the method is responsible for both filtering and handling different criteria (category and price). In the future, if the filtering logic grows, we’ll be modifying this one method.
4.  **Scalability**: As our app grows, the filtering logic will get more complicated. It’ll require constant modifications, which is both time-consuming and error-prone.

### The Solution: The Specification Pattern

The **Specification Pattern** is exactly what we need! It allows us to separate each filtering criterion into a **reusable, composable** component. Let’s build it step-by-step.

We start by creating a generic interface `ISpecification`, which will define the `isSatisfied` method to check if an object meets a certain condition.

```java
public interface ISpecification<T> {
    boolean isSatisfied(T candidate);
}
```

Then we will implement this `ISpecification` interface to define two concrete specifications for category and price: `CategorySpecification` and `PriceSpecification`.

```java
public class CategorySpecification implements ISpecification<Product> {
    private String category;

    public CategorySpecification(String category) {
        this.category = category;
    }

    @Override
    public boolean isSatisfied(Product product) {
        return category.equals(product.category);
    }
}
```

```java
public class PriceSpecification implements ISpecification<Product> {
    private double maxPrice;

    public PriceSpecification(double maxPrice) {
        this.maxPrice = maxPrice;
    }

    @Override
    public boolean isSatisfied(Product product) {
        return product.price < maxPrice;
    }
}
```

Now, we create an `AndSpecification` class to combine multiple specifications (e.g., category and price).

```java
public class AndSpecification<T> implements ISpecification<T> {
    private ISpecification<T> spec1;
    private ISpecification<T> spec2;

    public AndSpecification(ISpecification<T> spec1, ISpecification<T> spec2) {
        this.spec1 = spec1;
        this.spec2 = spec2;
    }

    @Override
    public boolean isSatisfied(T candidate) {
        return spec1.isSatisfied(candidate) && spec2.isSatisfied(candidate);
    }
}
```

Now we will write an `IFilter` interface that will have a method called `filter` that will be responsible for filtering the product that satisfies the certain specification.

```java
public interface IFilter<T> {
    List<T> filter(List<T> items, ISpecification<T> spec);
}
```

We will implement this interface to define a filter class for our product.

```java
public class ProductFilter implements IFilter<Product> {

    public List<Product> filter(List<Product> products,
                                 ISpecification<Product> spec) {

        List<Product> result = new ArrayList<>();

        for (Product product : products) {
            if (spec.isSatisfied(product)) {
                result.add(product);
            }
        }

        return result;
    }
}
```

Now in the driver code, we will initialize the specification objects and use them to filter the product based on the criteria.

```java
public class Main {
    public static void main(String[] args) {
        List<Product> products = List.of(
                new Product("T-Shirt", "Cotton T-Shirt", "Apparel", 499),
                new Product("Jeans", "Denim Jeans", "Apparel", 1200),
                new Product("Shoes", "Running Shoes", "Footwear", 800)
        );

        IFilter<Product> filter = new ProductFilter();

        // Create specifications
        ISpecification<Product> categorySpec = new CategorySpecification("Apparel");
        ISpecification<Product> priceSpec = new PriceSpecification(1000);
        ISpecification<Product> andSpec = new AndSpecification(categorySpec, priceSpec);

        // Apply filter
        List<Product> filteredProducts = filter.filter(products, andSpec);

        filteredProducts.forEach(product -> System.out.println(product.name + " - " + product.price));
    }
}
```

### How Does This Solve Our Problems?

1.  **Open for Extension, Closed for Modification:** New filtering criteria can be added by creating new specification classes without modifying existing code. If we want to add new filtering criteria (e.g., filtering by brand, rating), we can add new specification classes without touching the existing filtering code. This makes the filtering logic extendable without modifying existing code and more scalable, as you can extend it without modifying existing code.
2.  **No Code Duplication:** The specifications can be reused throughout our code, reducing duplication. Imagine filtering by category or price in other parts of the app — without re-writing the logic!
3.  **Single Responsibility:** Each specification has a single responsibility. Want to make the category filter more specific (e.g., filtering by Shirts in the Apparel category)? We can change just that specification without touching the others.

### What’s the Catch?

Some might argue that the Specification Pattern adds complexity to what could be a simple filtering task. It involves managing multiple classes and methods, and in some cases, it could introduce performance overhead (e.g., extra object initialization).

However, these trade-offs become worth it as your application grows. The increased extensibility, reduced risk of errors, and easier maintainability outweigh the upfront complexity.

Hope this helps. Thank you for reading!