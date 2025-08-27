```yaml
---
title: "Records and the ‘with’ operator, redux | Jon Skeet's coding blog"
source: https://codeblog.jonskeet.uk/2025/07/29/records-and-the-with-operator-redux/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2100
date_published: 2025-07-29T20:00:38.000Z
date_captured: 2025-08-12T11:16:54.817Z
domain: codeblog.jonskeet.uk
author: jonskeet
category: frontend
technologies: [C#, .NET, Roslyn Analyzers, NuGet, GitHub]
programming_languages: [C#]
tags: [csharp, records, immutability, roslyn-analyzers, code-analysis, language-features, dotnet, with-operator, software-design, best-practices]
key_concepts: [csharp-records, with-operator, immutability, roslyn-analyzers, code-analysis, primary-constructors, derived-data, design-patterns]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article delves into a subtle behavioral aspect of C# record types and the `with` operator, specifically concerning how derived properties are handled during object copying. The author explains how their initial mental model of `with` as re-evaluating construction parameters was incorrect, leading to unexpected inconsistencies with precomputed properties. To address this, the post introduces custom Roslyn Analyzers designed to identify and warn about potentially unsafe uses of the `with` operator on record parameters. It also explores alternative design patterns, such as wrapping records within classes, for managing more complex types with significant precomputation or validation logic, offering practical solutions for safer record usage.]
---
```

# Records and the ‘with’ operator, redux | Jon Skeet's coding blog

[C#](https://codeblog.jonskeet.uk/category/csharp/), [Election 2029](https://codeblog.jonskeet.uk/category/election-2029/)

# Records and the ‘with’ operator, redux

[July 29, 2025](https://codeblog.jonskeet.uk/2025/07/29/records-and-the-with-operator-redux/) [jonskeet](https://codeblog.jonskeet.uk/author/jonskeet/) [4 Comments](https://codeblog.jonskeet.uk/2025/07/29/records-and-the-with-operator-redux/#comments)

In my [previous blog post](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/) I described some behaviour of C# record types which was unexpected to _me_, though entirely correct according to the documentation. This is a follow-up post to that one, so if you haven’t read that one yet, please do so – I won’t go over all the same ground.

## Is this a problem or not?

The previous blog post made it to Hacker News, and the comments there, on Bluesky, and on the post itself have been decidedly mixed.

Several people believe this is working as intended, several people believe it’s a terrible decision on the part of the C# language team.

Perhaps unsurprisingly, the most insightful comment was from Eric Lippert, who referred to [a post on the Programming Language Design and Implementation Stack Exchange site](https://langdev.stackexchange.com/questions/4372). Eric has answered the question thoroughly, as ever.

I believe the difference in opinions comes from interpretation about what “with” is requesting. Eric wrote:

> The `with` makes a copy of the record, changing only the value of the property you identified as wishing to change, so it ought not to be surprising that nothing else changed.

That’s not how I’ve ever thought of `with` – I haven’t expected it to say “this object but with these different properties”, but instead “a new record, with the same _parameters_ as the original one, but with these different _parameters_“. It’s a subtle distinction – sufficiently subtle that I hadn’t even bothered to think about it until running into this problem – but I suspect it explains how different people think about the same feature in different ways. I wouldn’t have thought of “setting the property” because I think of records as being immutable to start with: that the only way you can end up with a record where the property returns a different value is by providing that different value _as part of construction_. (Again, to be crystal clear: I don’t think I’ve found any bits of documentation which are incorrect. It’s my mental model that has been wrong.)

I haven’t gone back over previous YouTube videos describing the feature – either from the C# team itself or from other developers – to see whether a) it’s described in terms of setting _properties_ rather than _parameters_; b) the videos describe the distinction in order to make it clear which is “right”.

In my defence, even when you _do_ have a better mental model for how records work, this is a pretty easy mistake to make, and you need to be on the ball to spot it in code review. The language absolutely _allows_ you to write records which aren’t just “lightweight data records” in the same way that you do for classes – so I don’t think it should be surprising that folks are going to do that.

So, after this introductory spiel, this post has two aspects to it:

*   How am I going to stop myself from falling into the same trap again?
*   What changes have I made within the Election 2029 code base?

## Trap avoidance: Roslyn Analyzers

In the previous post, I mentioned writing a Roslyn analyzer as a possible way forward. My initial hope was to have a single analyzer which would just spot the use of `with` operators targeting any parameter which was used during initialization.

That initial attempt worked to some extent – it would have spotted the dangerous code from the original blog post – but it _only_ worked when the source code for the record and the source code using the `with` operator were in the same project. I’ve now got a slightly better solution with _two_ analyzers, which can even work with package references where you may not have access to the source code for the record at all… so long as the package author is using the same analyzers! (This will make more sense when you’ve seen the analyzers.)

The [source code of the analyzers is on GitHub](https://github.com/jskeet/DemoCode/tree/main/RoslynAnalyzers) and the analyzers themselves are in the [JonSkeet.RoslynAnalyzers](https://www.nuget.org/packages/JonSkeet.RoslynAnalyzers) NuGet package. To install them in a project, just add this to an item group in your project file:

```xml
<PackageReference Include="JonSkeet.RoslynAnalyzers" Version="1.0.0-beta.6"
        PrivateAssets="all"
        IncludeAssets="runtime; build; native; contentfiles; analyzers"/>
```

Obviously, it’s all very beta – and there are _lots_ of corner cases it probably wouldn’t find at the moment. (Pull requests are welcome.) But it scratches my particular itch for now. (If someone else wants to take the idea and run with it in a more professional, supported way, ideally in a package with dozens of other useful analyzers, that’s great.)

As I mentioned, there are two analyzers, with IDs of JS0001 and JS0002. Let’s look at how they work by going back to the original demo code from the previous post. Here’s the complete buggy code:

```csharp
// Record
public sealed record Number(int Value)
{
    public bool Even { get; } = (Value & 1) == 0;
}
// Use of record
var n2 = new Number(2);
var n3 = n2 with { Value = 3 };
Console.WriteLine(n2); // Output: Number { Value = 2, Even = True }
Console.WriteLine(n3); // Output: Number { Value = 3, Even = True }
```

Adding the analyzer package highlights the `int Value` parameter declaration in `Number`, with this warning:

> JS0001 Record parameter ‘Value’ is used during initialization; it should be annotated with `[DangerousWithTarget]`

Currently, there’s no code fix, but we need to do two things:

*   Declare an attribute called `DangerousWithTargetAttribute`
*   Apply the attribute to the parameter

Here’s the complete attribute and record code with the fix applied:

```csharp
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class DangerousWithTargetAttribute : Attribute;

public sealed record Number([DangerousWithTarget] int Value)
{
    public bool Even { get; } = (Value & 1) == 0;
}
```

The attribute doesn’t _have_ to be internal, and indeed in my election code base it’s not. But it can be, even if you’re using the record from a different assembly. The analyzer doesn’t care what namespace it’s in or any other details (although it does currently have to be called `DangerousWithTargetAttribute` rather than just `DangerousWithTarget`).

At this point:

*   The source code makes it clear to humans that we know it would be dangerous to set the `Value` property in a `with` operator with `Number`
*   The compiled code makes that clear (to the other analyzer) as well

After applying the above change, we get a different warning – this time on `n2 with { Value = 3 }`:

> JS0002: Record parameter ‘Value’ is annotated with `[DangerousWithTarget]`

(Both of these warnings have more detailed descriptions associated with them as well as the summary.)

Now you know the problem exists, it’s up to you to fix it… and there are multiple different ways you could do that. Let’s try to get warning-free by replacing our precomputed property with one which is computed on demand. The analyzers _don’t_ try to tell you if `[DangerousWithTarget]` is applied where it doesn’t need to be, so this code compiles without any warnings, but it doesn’t remove our JS0002 warning:

```csharp
// No warning here, but the expression 'n2 with { Value = 3 }' still warns.
public sealed record Number([DangerousWithTarget] int Value)
{
    public bool Even => (Value & 1) == 0;
}
```

As it happens, this has proved unexpectedly useful within the Election2029 code, where even though a parameter _isn’t_ used in initialization, there’s an expected consistency between parameters which should discourage the use of the `with` operator to set one of them.

Once we remove the `[DangerousWithTarget]` attribute from the parameter though, all the warnings are gone:

```csharp
public sealed record Number(int Value)
{
    public bool Even => (Value & 1) == 0;
}
```

The analyzer ignores the `Even` property because it doesn’t have an initializer – it’s fine to use `Value` for computed properties _after_ initialization.

## A new pattern for Election2029

So, what happened when I enabled the analyzers in my Election2029 project? (Let’s leave aside the bits where it didn’t work first time… there’s a reason the version number is 1.0.0-beta.6 already.)

Predictably enough, a bunch of records were flagged for not specifying the `[DangerousWithTarget]`… and when I’d applied the attribute, there were just one or two places where I was using the `with` operator in an unsafe way. (Of course, I checked whether the original bug which had highlighted the issue for me in the first first place was caught by the analyzers – and it was.)

For most of the records, the precomputation _feels_ okay to me. They’re still fundamentally pretty lightweight records, with a smattering of precomputation which would feel pointlessly inefficient if I made it on-demand. I like the functionality that I’m given automatically by virtue of them being records. I’ve chosen to leave those as records, knowing that at least if I try to use the `with` operator in a dangerous operator, I’ll be warned about it.

However, there are two types – `ElectionCoreContext` and `ElectionContext`, which I \[wrote about earlier\] – which have a _lot_ of precomputation. They feel more reasonable to be classes. Initially, I converted them into just “normal” classes, with a primary constructor and properties. It felt okay, but not _quite_ right somehow. I liked the idea of the record type for _just_ the canonical information for the context… so I’ve transformed `ElectionContext` like this (there’s something similar for `ElectionCoreContext`):

```csharp
public sealed class ElectionContext : IEquatable<ElectionContext>
{
    public ElectionContextSourceData SourceData { get; }
    // Bunch of properties proxying access
    public Instant Timestamp => SourceData.Timestamp;
    // ...
    public ElectionContext(ElectionContextSourceData sourceData)
    {
        // Initialization and validation
    }
    public sealed record ElectionContextSourceData(Instance Timestamp, ...)
    {
        // Equals and GetHashCode, but nothing else
    }
}
```

At this point:

*   I’ve been able to add validation to the constructor. I couldn’t do that with a record in its primary constructor.
*   It’s really clear what’s canonical information vs derived data – I could even potentially refactor the storage layer to only construct and consume the `ElectionContextSourceData`, for example. (I’m now tempted to try that. I suspect it would be somewhat inefficient though, as it uses the derived data to look things up when deserializing.)
*   I can still use the `with` operator with the record, when I need to (which is handy in a few places)
*   There’s no risk of the derived data being out of sync with the canonical data, because the ordering is very explicit

Ignoring the naming (and possibly the nesting), is this a useful pattern? I wouldn’t want to do it for _every_ record, but for these two core, complex types it feels like it’s working well _so far_. It’s early days though.

## Conclusion

I’m really pleased that I can now use records more safely, even if I’m using them in ways that other folks may not entirely condone. I may well change my mind and go back to using regular classes for all but the most cut-and-dried cases. But for now, the approaches I’ve got of “use records where it feels right, even if that means precomputation” and “use classes to wrap records where there’s enough behavior to justify it” are working reasonably well.

I don’t really expect other developers to use my analyzers (although you’re very welcome to do so, of course) – but the fact that they’re even feasible points to Roslyn being a bit of a miracle. I’m not recommending either my “careful use of records slightly beyond their intended use” or “class wrapping a record” approaches yet. I’ve got plenty of time to refactor if they don’t work out for the Election2029 project. But I’d still be interested in getting feedback on whether my decisions at least seem _somewhat_ reasonable to others.

### Share this:

*   [Click to share on X (Opens in new window) X](https://codeblog.jonskeet.uk/2025/07/29/records-and-the-with-operator-redux/?share=twitter&nb=1)
*   [Click to share on Facebook (Opens in new window) Facebook](https://codeblog.jonskeet.uk/2025/07/29/records-and-the-with-operator-redux/?share=facebook&nb=1)

Like Loading...

### _Related_

[Language design, when is a language “done”, and why does it matter?](https://codeblog.jonskeet.uk/2008/02/11/language-design-when-is-a-language-quot-done-quot-and-why-does-it-matter/ "Language design, when is a language &#8220;done&#8221; and why does it matter?")February 11, 2008In "C#"

[Generic constraints for enums and delegates](https://codeblog.jonskeet.uk/2009/09/10/generic-constraints-for-enums-and-delegates/ "Generic constraints for enums and delegates")September 10, 2009In "C#"

[How many Jedi?](https://codeblog.jonskeet.uk/2010/07/01/how-many-jedi/ "How many Jedi?")July 1, 2010In "Books"

# Post navigation

[Previous PostUnexpected inconsistency in records](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/)

## 4 thoughts on “Records and the ‘with’ operator, redux”

1.  Pingback: [Unexpected inconsistency in records | Jon Skeet's coding blog](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/)
    
2.  ![Ronald's avatar](https://2.gravatar.com/avatar/b170dce9f9be53c5a24cea37e3adb3e6703476b47c164f7a275c529e15e6f4f9?s=34&d=identicon&r=G) **Ronald** says:
    
    [July 30, 2025 at 6:23 am](https://codeblog.jonskeet.uk/2025/07/29/records-and-the-with-operator-redux/#comment-73918)
    
    Awesome research write-up! Just looks like the first code sample here (the copy of the “buggy” code from the previous post) has a tiny difference compared to the previous post; I believe the buggy behavior is for n3 to have Even=True, but your comment here shows Even=False which would’ve been correct behavior.
    
    [Like](https://codeblog.jonskeet.uk/2025/07/29/records-and-the-with-operator-redux/?like_comment=73918&_wpnonce=6bc5cfa550)Like
    
    [Reply](https://codeblog.jonskeet.uk/2025/07/29/records-and-the-with-operator-redux/?replytocom=73918#respond)
    
    1.  ![jonskeet's avatar](https://2.gravatar.com/avatar/22e44f8d2ea51022ed71080e3105f5b775da0533a113085ac59dade5c17ed18d?s=34&d=identicon&r=G) **[jonskeet](https://jonskeetcodingblog.wordpress.com)** says:
        
        [July 30, 2025 at 6:26 am](https://codeblog.jonskeet.uk/2025/07/29/records-and-the-with-operator-redux/#comment-73919)
        
        Fixed, thanks!
        
        [Like](https://codeblog.jonskeet.uk/2025/07/29/records-and-the-with-operator-redux/?like_comment=73919&_wpnonce=bbb46bc5b6)Like
        
        [Reply](https://codeblog.jonskeet.uk/2025/07/29/records-and-the-with-operator-redux/?replytocom=73919#respond)
        
3.  Pingback: [Dew Drop – July 30, 2025 (#4469) – Morning Dew by Alvin Ashcraft](https://morningdew-bpc6g3a0fgaxdxcu.eastus2-01.azurewebsites.net/2025/07/30/dew-drop-july-30-2025-4469/)
    

### Leave a comment [Cancel reply](/2025/07/29/records-and-the-with-operator-redux/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2100#respond)

Δdocument.getElementById( "ak_js_1" ).setAttribute( "value", ( new Date() ).getTime() );