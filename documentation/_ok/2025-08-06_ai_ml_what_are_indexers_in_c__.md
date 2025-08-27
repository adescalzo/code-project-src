```yaml
---
title: What are indexers in C#?
source: https://www.c-sharpcorner.com/article/what-are-indexers-in-c-sharp/?utm_source=dotnetnews.beehiiv.com&utm_medium=newsletter&utm_campaign=the-net-news-daily-issue-260
date_published: 2025-08-07T00:00:00.000Z
date_captured: 2025-08-12T11:12:21.666Z
domain: www.c-sharpcorner.com
author: Baibhav Kumar
category: ai_ml
technologies: [.NET, Dictionary, JavaScript]
programming_languages: [C#, JavaScript]
tags: [csharp, indexer, object-oriented-programming, data-access, collection, encapsulation, programming, code-examples, software-design]
key_concepts: [indexers, get-set-accessors, overloading, encapsulation, data-structures, read-only-indexer, multi-parameter-indexer, this-keyword]
code_examples: false
difficulty_level: intermediate
summary: |
  Indexers in C# provide a way to access class or struct instances like arrays using square brackets, acting as "smart arrays" or "parameterized properties." They expose internal data structures such as arrays, lists, or dictionaries in an intuitive manner. This article explains the syntax of indexers, their benefits like improved readability and data encapsulation, and provides practical examples including simple integer-based, string-based, and overloaded indexers. It highlights key characteristics such as the use of the `this` keyword, get/set accessors, and their application in real-world scenarios like student grade systems or phonebooks. The content covers multi-parameter and read-only indexers, emphasizing their flexibility and power in C# programming.
---
```

# What are indexers in C#?

![Title Image](https://www.c-sharpcorner.com/article/what-are-indexers-in-c-sharp/Images/Indexers%20in%20CSharp.jpeg "A title image for the article 'What are Indexers in C#?' featuring the C# logo (a purple hexagon with a white C# symbol) on a dark blue background.")

# What are indexers in C#?

An indexer in C# is a special type of class or struct member that allows its instances to be accessed like arrays using square brackets ( `[ ]` ). It acts as a shortcut to expose internal data, such as arrays, lists, or dictionaries, in a clean and intuitive way, making the object behave like a virtual array or key-value store.

Indexers are declared using the `this` keyword along with at least one parameter and `get`/`set` accessors. Unlike properties, which do not take parameters, indexers must take one or more parameters to determine which data element to access or modify.

## 🧪 Syntax

```csharp
[access_modifier] [return_type] this [parameter_list]
{
    get { // return value }
    set { // assign value }
}
```

*   **access\_modifier:** Usually public
*   **return\_type:** Type of data returned (e.g., int, string)
*   **this:** Indicates it's an indexer (not a method or property)
*   **parameter\_list:** Type(s) of parameter(s) used to access elements (e.g., int, string)
*   **get:** Returns the value at the specified index
*   **set:** Assigns a value to the specified index

## 🎯 Why Use Indexers?

*   **✔️ Improves readability:** Access data like you access arrays.
*   **🔒 Encapsulates data:** Internal implementation is hidden from the user.
*   **🧼 Cleaner code:** Fewer method calls, more intuitive syntax.
*   **🧰 Custom behavior:** Control what happens when getting/setting values.

## 👨‍💻 Example: Student Grades (Simple Indexer)

```csharp
public class StudentGrades
{
    private int[] grades = new int[5];

    public int this[int index]
    {
        get { return grades[index]; }
        set { grades[index] = value; }
    }
}
```

### Usage:

```csharp
StudentGrades student = new StudentGrades();
student[0] = 85;
Console.WriteLine(student[0]);  // Output: 85
```

## 📒 String-Based Indexer (Like Dictionary)

```csharp
public class PhoneBook
{
    private Dictionary<string, string> contacts = new();

    public string this[string name]
    {
        get => contacts.ContainsKey(name) ? contacts[name] : "Not Found";
        set => contacts[name] = value;
    }
}
```

### Usage:

```csharp
PhoneBook pb = new PhoneBook();
pb["Alice"] = "123-4567";
Console.WriteLine(pb["Alice"]); // Output: 123-4567
```

## 🔁 Advanced: Multiple & Overloaded Indexers

```csharp
public class Indexer
{
    private int[] data = new int[10];

    // Multi-parameter indexer
    public int this[int index, bool square]
    {
        get => square ? data[index] * data[index] : data[index];
        set => data[index] = square ? (int)Math.Sqrt(value) : value;
    }

    // Overloaded indexer with string
    public int this[string key]
    {
        get
        {
            switch (key.ToLower())
            {
                case "first": return data[0];
                case "last": return data[data.Length - 1];
                default: throw new ArgumentException("Invalid key.");
            }
        }
    }

    // Read-only indexer
    public int this[int index]
    {
        get { return data[index]; }
    }
}
```

### Usage:

```csharp
Indexer i = new Indexer();
i[0, false] = 5;
Console.WriteLine(i[0, false]);  // 5
Console.WriteLine(i[1, true]);   // 0 (0 squared)
Console.WriteLine(i["first"]);   // 5
Console.WriteLine(i[2]);         // 0 (read-only)
```

## 🧾 Key Points to Remember

| Feature           | Description                                                              |
| :---------------- | :----------------------------------------------------------------------- |
| `this` keyword    | Used to define an indexer in a class                                     |
| `get` accessor    | Retrieves the value at the specified index                               |
| `set` accessor    | Assigns a value at the specified index using the `value` keyword         |
| Parameters        | Must take at least one parameter (unlike properties)                     |
| Return type       | Can be any type, like int, string, object, etc.                          |
| Index types       | Can be int, string, or any type you define                               |
| Overloading       | Multiple indexers with different parameter types are allowed             |
| Multi-dimensional | You can define indexers with multiple parameters                         |
| Read-only indexer | Leave out `set` to make an indexer read-only                             |
| Not static        | Indexers are always instance members (cannot be static)                  |
| Smart arrays      | Indexers are also called "smart arrays" or "parameterized properties"    |

## 🧰 Real-World Use Cases

*   📚 Student grade systems (index students by ID)
*   📞 Phonebook/contact apps (index by name)
*   📦 Inventory systems (index by SKU)
*   🔢 Matrix and grid implementations
*   🔧 Wrapper classes around arrays or dictionaries
*   📁 Custom data containers or API models

## 📝 Conclusion

Indexers in C# offer a clean and intuitive way to interact with objects just like arrays or dictionaries. Whether you’re wrapping a collection or building a custom data structure, indexers allow you to expose data safely while improving code readability. With support for multiple parameters, string keys, and overloading, indexers make your C# code more expressive and powerful. If your class needs indexed access, like a collection or mapping, indexers are your go-to feature.