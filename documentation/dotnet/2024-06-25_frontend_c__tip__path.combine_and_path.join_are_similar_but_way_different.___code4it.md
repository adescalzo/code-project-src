## Summary
This C# tip clarifies the distinct behaviors of `Path.Combine` and `Path.Join` when constructing file paths. While both methods concatenate strings, `Path.Combine` discards preceding path segments if an absolute path is encountered later in the arguments, effectively starting a new path. In contrast, `Path.Join` simply concatenates all provided strings, preserving all parts regardless of absolute path indicators. The article provides illustrative code examples and a detailed comparison table to highlight these crucial differences, advising developers to validate inputs and use unit tests for robust path manipulation.

---

# C# Tip: Path.Combine and Path.Join are similar but way different.

```markdown
**Source:** https://www.code4it.dev/csharptips/path-combine-vs-path-join/
**Date Captured:** July 28, 2025
**Date Published:** June 25, 2024
**Read Time:** 3 min
**Domain:** www.code4it.dev
**Author:** Davide Bellone
**Category:** C# Tips
```

---

![C# Tip: Path.Combine vs Path.Join - Code4IT logo and title banner](https://www.code4it.dev/csharptips/path-combine-vs-path-join/featuredImage.png)

> **Just a second! 🫷**
> If you are here, it means that you are a software developer. So, you know that storage, networking, and domain management have a cost.
>
> If you want to support this blog, please ensure that you have disabled the adblocker for this site. **I configured Google AdSense to show as few ADS as possible** - I don't want to bother you with lots of ads, but I still need to add some to pay for the resources for my site.
>
> Thank you for your understanding.
> - _Davide_

When you need to compose the path to a folder or file location, you can rely on the `Path` class. It provides several static methods to create, analyze and modify strings that represent a file system.

`Path.Join` and `Path.Combine` look similar, yet they have some important differences that you should know to get the result you are expecting.

## Path.Combine: Take from the Last Absolute Path

`Path.Combine` concatenates several strings into a single string that represents a file path.

```csharp
Path.Combine("C:", "users", "davide");
// C:\users\davide
```

However, there’s a tricky behaviour: if any argument other than the first contains an absolute path, all the previous parts are discarded, and the returned string starts with the last absolute path:

```csharp
Path.Combine("foo", "C:bar", "baz");
// C:bar\baz

Path.Combine("foo", "C:bar", "baz", "D:we", "ranl");
// D:we\ranl
```

## Path.Join: Take Everything

`Path.Join` does not try to return an absolute path, but it just joins the string using the OS path separator:

```csharp
Path.Join("C:", "users", "davide");
// C:\users\davide
```

This means that if there is an absolute path in any argument position, all the previous parts are not discarded:

```csharp
Path.Join("foo", "C:bar", "baz");
// foo\C:bar\baz

Path.Join("foo", "C:bar", "baz", "D:we", "ranl");
// foo\C:bar\baz\D:we\ranl
```

## Final Comparison

As you can see, the behaviour is slightly different.

Let’s see a table where we call the two methods using the same input strings:

| Input                               | Path.Combine Output | Path.Join Output    |
| :---------------------------------- | :------------------ | :------------------ |
| `["singlestring"]`                  | `singlestring`      | `singlestring`      |
| `["foo", "bar", "baz"]`             | `foo\bar\baz`       | `foo\bar\baz`       |
| `["foo", " bar ", "baz"]`           | `foo\ bar \baz`     | `foo\ bar \baz`     |
| `["C:", "users", "davide"]`         | `C:\users\davide`   | `C:\users\davide`   |
| `["foo", " ", "baz"]`               | `foo\ \baz`         | `foo\ \baz`         |
| `["foo", "C:bar", "baz"]`           | `C:bar\baz`         | `foo\C:bar\baz`     |
| `["foo", "C:bar", "baz", "D:we", "ranl"]` | `D:we\ranl`         | `foo\C:bar\baz\D:we\ranl` |
| `["C:", "/users", "/davide"]`       | `/davide`           | `C:/users/davide`   |
| `["C:", "users/", "/davide"]`       | `/davide`           | `C:\users//davide`  |
| `["C:", "\users", "\davide"]`       | `\davide`           | `C:\users\davide`   |

Have a look at some specific cases:

*   Neither methods handle white and empty spaces: `["foo", " ", "baz"]` are transformed to `foo\ \baz`. Similarly, `["foo", " bar ", "baz"]` are combined into `foo\ bar \baz`, without removing the head and trail whitespaces. So, **always remove white spaces and empty values!**
*   `Path.Join` handles in a not-so-obvious way the case of a path starting with `/` or `\`: if a part starts with `\`, it is included in the final path; if it starts with `/`, it is escaped as `//`. This behaviour depends on the path separator used by the OS: in my case, I’m running these methods using Windows 11.

Finally, always remember that **the path separator depends on the Operating System** that is running the code. Don’t assume that it will always be `/`: this assumption may be correct for one OS but wrong for another one.

_This article first appeared on [Code4IT 🐧](https://www.code4it.dev/)_

## Wrapping Up

As we have learned, `Path.Combine` and `Path.Join` look similar but have profound differences.

Dealing with path building may look easy, but it hides some complexity. Always remember to:

*   **validate and clean your input** before using either of these methods (remove empty values, white spaces, and head or trailing path separators);
*   always **write some Unit Tests** to cover all the necessary cases;

I hope you enjoyed this article! Let’s keep in touch on [Twitter](https://twitter.com/BelloneDavide) or [LinkedIn](https://www.linkedin.com/in/BelloneDavide/)! 🤜🤛

Happy coding!

🐧

[CSharp](https://www.code4it.dev/tags/csharp/)