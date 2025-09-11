```yaml
---
title: "Loading Data in ML.NET: A Beginner’s Guide with C# Examples | by Anton Baksheiev | Sep, 2025 | ITNEXT"
source: https://itnext.io/loading-data-in-ml-net-a-beginners-guide-with-c-examples-0741af5a0ae1
date_published: 2025-09-05T22:21:13.827Z
date_captured: 2025-09-09T11:16:48.278Z
domain: itnext.io
author: Anton Baksheiev
category: frontend
technologies: [ML.NET, .NET, GitHub, AWS S3]
programming_languages: [C#]
tags: [machine-learning, data-loading, mlnet, csharp, binary-data, text-files, csv, data-preparation, dotnet, ai]
key_concepts: [data-loading, binary-data, in-memory-data, text-file-parsing, schema-definition, machine-learning, data-preparation, mlnet-idataview]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article serves as a beginner's guide to loading data into ML.NET using C# examples. It explores various data sources, including binary files, in-memory collections (enumerables), and text/CSV files. The guide demonstrates how to prepare and load data efficiently, highlighting the flexibility of ML.NET for different scenarios. It covers both attribute-based and schema-definition approaches for text file loading, providing clear code samples. The author emphasizes that ML.NET adapts to needs for speed, simplicity, or customization, laying a solid foundation for building and training machine learning models.]
---
```

# Loading Data in ML.NET: A Beginner’s Guide with C# Examples | by Anton Baksheiev | Sep, 2025 | ITNEXT

# Loading Data in ML.NET: A Beginner’s Guide with C# Examples

Learn how to prepare and load data into ML.NET using binary files, in-memory collections, and CSV/text loaders, with clear code samples and explanations.

![Handwritten agenda for the article, listing ML.NET: Loading Data, Binary data loader, Enumerable data loader, Load data from text file, and Custom Text loader.](https://miro.medium.com/v2/resize:fit:700/1*D-KFPA33shKTcnJD6xEh9Q.png)

AI is a vast area, and it can be difficult to decide where to start learning. It is easy to get lost in the overwhelming amount of articles available on the internet. However, at its core, AI is about prediction, which means you need data to use as a database. (Yes, this is a very simple comparison, and there are much more complex aspects to it, but for beginners, diving too deeply into those complexities can be overwhelming and distracting.)

In this article, I would like to demonstrate how to use ML.NET to load data, explore the different types of data sources it supports, and provide examples for each approach using C# along with screenshots of the results. All code examples will be available for download from a GitHub repository.

So, let’s take our first steps into the vast and fascinating world of ML/AI with ML.NET!

## Agenda

*   **Binary Data Loader**
*   **Enumerable Data Loader**
*   **Load Data in ML.NET from Text File**
*   **Create a Text Loader**

> In all demonstration code snippets will be used [https://github.com/abaksheiev/MLNET.LoadData](https://github.com/abaksheiev/MLNET.LoadData)

## Binary Data Loader

Binary data generally has a smaller size in most cases, which makes it preferable if you need to download it to the cloud (for example, AWS S3). If you want to use this format, you need to prepare a binary file for it, which can also be done using `mlContext`.

### Code of creation bin file

```csharp
public void CreateBinaryData(IDataView data, string fileName)
{
    using (FileStream fs = new(Path.Combine(_dataBin, fileName), FileMode.Create))
    {
       _mlContext.Data.SaveAsBinary(data, fs);
    }
}
```

### Code of load data from already existing bin file

```csharp
public IDataView LoadBinaryData(string fileName)
{
    return _mlContext.Data.LoadFromBinary(Path.Combine(_dataBin, fileName));
}
```

## Enumerable Data Loader

Loading from an existing collection is the easiest way, as it does not require any preparation. This approach allows you to reuse models and collections that are already available in your solution.

```csharp
public IDataView LoadData(List<T> data) {

  return _mlContext.Data.LoadFromEnumerable(data.ToArray());
}
```

## Load Data in ML.NET from Text File

Loading data from a file, in my opinion, is very similar to serialization because it requires some preparation of the model. In this case, you need to add attributes such as `LoadColumn` and `Name` to your model properties. Below is a snippet showing how it should look:

```csharp
[LoadColumn(0), Name("hour_of_day")]
public int HourOfDay { get; set; }
```

Because different delimiters can be used in a CSV file, the separator should be specified directly when calling the function. Also, you need to account for the first row, which usually contains the column names, so the second parameter is `hasHeader`.

```csharp
public IDataView LoadData<TInput>(string fileName) where TInput : class
{
    return _mlContext.Data.LoadFromTextFile<CoffeSales>(fileName, hasHeader: true, separatorChar: ',');
}
```

## Create a Text Loader

As always, the same goal can be achieved in a different way. If you don’t want to add attributes to each property in your model, you can create a loader that requires a description of the schema. This is a great option because it allows you to select all the columns you need, just make sure not to forget the column indices.

```csharp
public IDataView LoadDataCustom<TInput>(string fileName) where TInput : class
{
    var loader = _mlContext.Data.CreateTextLoader(
    [
        new TextLoader.Column("hour_of_day", DataKind.Single,0),
        new TextLoader.Column("cash_type", DataKind.Single,1),
        new TextLoader.Column("money", DataKind.String, 2),
        new TextLoader.Column("coffee_name", DataKind.String, 3),
        new TextLoader.Column("Time_of_Day", DataKind.String, 4),
        new TextLoader.Column("Weekday", DataKind.String, 5),
        new TextLoader.Column("Month_name", DataKind.String, 6),
        new TextLoader.Column("Weekdaysort", DataKind.String, 7),
        new TextLoader.Column("Monthsort", DataKind.String, 7),
        new TextLoader.Column("Date", DataKind.String, 9),
        new TextLoader.Column("Time", DataKind.String, 10)
    ], separatorChar: ',', hasHeader: true);

    return loader.Load(fileName);
}
```

In conclusion, this article showed how flexible and beginner-friendly ML.NET is when it comes to loading data. No matter what type of source you are working with — binary files, in-memory collections, or text/CSV files — ML.NET provides straightforward ways to prepare your dataset for machine learning tasks. Binary loaders are a good option when performance and file size are important, especially if you plan to move data to the cloud. Enumerable data loaders are the simplest choice when you already have collections or lists in memory. Text loaders, on the other hand, give you more control over how CSV data is mapped into your model, either by adding attributes to your class properties or by explicitly defining a schema.

The most important point is that ML.NET adapts to your scenario, whether you want speed, simplicity, or full customization. Once the data is loaded, you can move on to building and training models, knowing that your input pipeline is solid. With these techniques, you now have the foundation to work with different data sources and start exploring the power of AI and machine learning directly in C#.

Thank you for reading! I hope you found this article helpful. If you have any questions, suggestions, or would like to discuss further, feel free to connect with me on [LinkedIn](https://www.linkedin.com/in/baksheievanton/). If you have any tips, experiences, or thoughts to add, I’d love to hear them in the comments below. Let’s keep the conversation going!