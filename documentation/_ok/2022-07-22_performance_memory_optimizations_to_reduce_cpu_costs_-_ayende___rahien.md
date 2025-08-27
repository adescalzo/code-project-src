```yaml
---
title: "Memory optimizations to reduce CPU costs - Ayende @ Rahien"
source: https://ayende.com/blog/203011-A/memory-optimizations-to-reduce-cpu-costs?Key=77d4c0db-6b32-4914-916e-d181ee2cfd95&utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2112
date_published: 2022-07-22T03:00:00.000Z
date_captured: 2025-08-25T10:14:04.003Z
domain: ayende.com
author: Unknown
category: performance
technologies: [.NET, Garbage Collector, Data Structures]
programming_languages: [C#, Bash]
tags: [memory-optimization, cpu-performance, garbage-collection, data-structures, csharp, performance-tuning, data-oriented-programming, reference-types, value-types]
key_concepts: [memory-optimization, cpu-optimization, garbage-collection, data-oriented-programming, array-optimization, reference-types, value-types, data-structures]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores advanced memory optimizations to reduce CPU costs when processing large datasets. It demonstrates how splitting data into parallel arrays and de-duplicating string references can significantly reduce memory footprint. The author highlights an unexpected and substantial CPU performance boost achieved by replacing an array of string references with an array of byte indexes. This improvement is attributed to the reduced workload on the Garbage Collector, which no longer needs to traverse numerous string objects, thereby showcasing the hidden costs associated with managing reference types in large collections.
---
```

# Memory optimizations to reduce CPU costs - Ayende @ Rahien

## Memory optimizations to reduce CPU costs

---

![A stylized robot or android with binary code on its face, raising its hands.](/blog/Images/RpfJpZ_H4LP-bN7mX-OyDQ.png)

Imagine that you are given the following task, with a file like this:

---

```bash
Name,Department,Salary,JoinDate
John Smith,Marketing,75000,2023-01-15
Alice Johnson,Finance,82000,2022-06-22
Bob Lee,Sales,68000,2024-03-10
Emma Davis,HR,71000,2021-09-01
```

---

You want to turn that into a single list of all the terms in the (potentially very large) file.

In other words, you want to turn it into something like this:

---

```bash
[
  {"term": "Name", "position": 0, "length": 4},
  {"term": "Department", "position": 5, "length": 10},
                   ...
  {"term": "2021-09-01", "position": 160, "length": 10}
]
```

---

In other words, there is a single continuous array that references the entire data, and it is _pretty_ efficient to do so. _Why_ we do that doesn’t actually matter, but the critical aspect is that we observed poor performance and high memory usage when using this approach.

Let’s assume that we have a total of 10 million rows, or 40,000,000 items. Each item costs us 24 bytes (8 bytes for the Field, 8 bytes for the Position, 4 bytes for the Length, and 4 bytes for padding). So we end up with about 1GB in memory just to store things.

We can use Data-Oriented programming and split the data into individual arrays, like so:

---

```csharp
public string[] Fields;
public long[] Positions;
public int[] Lengths;


public Item Get(int i) => new(Fields[i], Positions[i], Lengths[i]);
```

---

This saves us about 200 MB of memory, because we can now skip the padding costs by splitting the Item into its component parts.

Now, we didn’t account for the memory costs of the `Field` strings. And that is because all of them use the same exact string instances (only the field names are stored as strings).

In terms of memory usage, that means we don’t have 40 million string instances, but just 4.

The next optimization is to reduce the cost of memory even further, like so:

---

```csharp
public string[] FieldsNames; // small array of the field names - len = 4
public byte[] FieldIndexes; // the index of the field name
public long[] Positions;
public int[] Lengths;


public Item Get(int i) => new(
         FieldsNames[FieldIndexes[i]], 
         Positions[i], 
         Lengths[i]
);
```

---

Because we know that we have a very small set of field names, we hold all of them in a single array and refer to them using an index (in this case, using a single byte only). In terms of memory usage, we dropped from about 1GB to less than half that.

So far, that is pretty much as expected. What was _not_ expected was a _significant_ drop in CPU usage because of this last change.

Can you figure out why this is the case?

The key here is this change:

---

```diff
- public string[] FieldNames;
+ public byte[] FieldIndexes;
```

---

The size of the array in our example is 40,000,000 elements. So this represents moving from an 8-byte reference to a 1-byte index in the `FieldNames` array. The reason for the memory savings is clear, but what is the reason for the _CPU usage_ drop?

In this case, you have to understand the code that _isn’t_ there. When we write in C#, we have a silent partner we have to deal with, the GC. So let’s consider what the GC needs to do when it encounters an array of strings:

> The GC marks the array as reachable, then traverses and marks _each_ referenced string object. It has to traverse the entire array, performing an operation for each value in the array, regardless of what that value is (or whether it has seen it before).

> For that matter, even if the array is filled with `null`, the GC has to go through the array to verify that, which has a cost for large arrays.

In contrast, what does the GC need to do when it runs into an array of bytes:

> The GC marks the array as reachable, and since it knows that there are no references to be found there, it is done.

In other words, this change in our data model led to the GC’s costs dropping significantly.

It makes perfect sense when you think about it, but it was quite a surprising result to run into when working on memory optimizations.