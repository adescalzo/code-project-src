# Unexpected inconsistency in records | Jon Skeet's coding blog

**Source:** https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2096
**Date Captured:** 2025-07-28T16:05:11.782Z
**Domain:** codeblog.jonskeet.uk
**Author:** jonskeet
**Category:** ai_ml

---

[C#](https://codeblog.jonskeet.uk/category/csharp/)

# Unexpected inconsistency in records

[July 19, 2025](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/) [jonskeet](https://codeblog.jonskeet.uk/author/jonskeet/) [13 Comments](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/#comments)

# Unexpected inconsistency in records

The other day, I was trying to figure out a bug in my code, and it turned out to be a misunderstanding on my part as to how C# records work. It’s entirely possible that I’m the only one who expected them to work in the way that I did, but I figured it was worth writing about in case.

As it happens, this is something I discovered when making a change to my 2029 UK general election site, but it isn’t actually related to the election, so I haven’t included it in the [election site blog series](https://codeblog.jonskeet.uk/category/election-2029/).

## Recap: nondestructive mutation

When records were introduced into C#, the [“nondestructive mutation”](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#nondestructive-mutation) `with` operator was introduced at the same time. The idea is that record types can be immutable, but you can easily and efficiently create a new instance which has the same data as an existing instance, but with some different property values.

For example, suppose you were to have a record like this:

1

`public` `sealed` `record HighScoreEntry(``string` `PlayerName,` `int` `Score,` `int` `Level);`

You could then have code of:

1

2

3

`HighScoreEntry entry =` `new``(``"Jon"``, 5000, 50);`

`var` `updatedEntry = entry with { Score = 6000, Level = 55 };`

This doesn’t change the data in the first instance (so `entry.Score` would still be 5000).

## Recap derived data

Records don’t allow you to specify constructor bodies for the primary constructor (something I meant to write about in my [earlier post about records and collections](https://codeblog.jonskeet.uk/2025/03/27/records-and-collections/), but you _can_ initialize fields (and therefore auto-implemented properties) based on the values for the parameters in the primary constructor.

So as a very simple (and highly contrived) example, you could create a record which determines whether or not a value is odd or even on initialization:

1

2

3

4

`public` `sealed` `record Number(``int` `Value)`

`{`

    `public` `bool` `Even {` `get``; } = (Value & 1) == 0;`

`}`

At first glance, this looks fine:

1

2

3

4

`var` `n2 =` `new` `Number(2);`

`var` `n3 =` `new` `Number(3);`

`Console.WriteLine(n2);` `// Output: Number { Value = 2, Even = True }`

`Console.WriteLine(n3);` `// Output: Number { Value = 3, Even = False }`

So far, so good. Until this week, I’d thought that was all fine.

## Oops: mixing `with` and derived data

The problem comes when mixing these two features. If we change the code above (while leaving the record itself the same) to create the second `Number` using the `with` operator instead of by calling the constructor, the output becomes incorrect:

1

2

3

4

`var` `n2 =` `new` `Number(2);`

`var` `n3 = n2 with { Value = 3 };`

`Console.WriteLine(n2);` `// Output: Number { Value = 2, Even = True }`

`Console.WriteLine(n3);` `// Output: Number { Value = 3, Even = True }`

“Value = 3, Even = True” is really not good.

How does this happen? Well, for some reason I’d always _assumed_ that the `with` operator called the constructor with the new values. That’s not actually what happens. The `with` operator above translates into code roughly like this:

1

2

3

`// This won't compile, but it's roughly what is generated.`

`var` `n3 = n2.<Clone>$();`

`n3.Value = 3;`

The `<Clone>$` method (at least in this case) calls a generated copy constructor (`Number(Number)`) which copies both `Value` and the backing field for `Even`.

This is all [documented](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/record#nondestructive-mutation) – but currently without any warning about the possible inconsistency it can introduce. (I’ll be emailing Microsoft folks to see if we can get something in there.)

Note that because `Value` is set _after_ the cloning operation, we couldn’t write a copy constructor to do the right thing here anyway. (At least, not in any sort of straightforward way – I’ll mention a convoluted approach later.)

In case anyone is thinking “why not just use a computed property?” obviously this works fine:

1

2

3

4

`public` `sealed` `record Number(``int` `Value)`

`{`

    `public` `bool` `Even => (Value & 1) == 0;`

`}`

Any property that can easily be computed on demand like this is great – as well as not exhibiting the problem from this post, it’s more efficient in memory too. But that really wouldn’t work for a lot of the properties in the records I use in the election site, where often the record is constructed with collections which are then indexed by ID, or other relatively expensive computations are performed.

## What can we do?

So far, I’ve thought of four ways forward, none of them pleasant. I’d be very interested to hear recommendations from others.

### Option 1: Shrug and get on with life

Now I know about this, I can avoid using the `with` operator for anything but “simple” records. If there are no computed properties or fields, the `with` operator is still really useful.

There’s a risk that I might use the `with` operator on a record type which is initially “simple” and then later introduce a computed member, of course. Hmm.

### Option 2: Write a Roslyn analyzer to detect the problem

In theory, at least for any records being used within the same solution in which they’re declared (which is everything for my election site) it should be feasible to write a Roslyn analyzer which:

*   Analyzes every member initializer in every declared record to see which parameters are used
*   Analyzes every `with` operator usage to see which parameters are being set
*   Records an error if there’s any intersection between the two

That’s quite appealing _and_ potentially useful to others. It does have the disadvantage of having to implement the Roslyn analyzer though. It’s been a long time since I’ve written an analyzer, but my guess is that it’s still a fairly involved process. If I actually find the time, this is probably what I’ll do – but I’m hoping that someone comments that either the analyzer already exists, or explains why it isn’t needed anyway.

### Option 3: Figure out a way of using `with` safely

I’ve been trying to work out how to potentially use `Lazy<T>` to defer computing any properties until they’re first used, which would come after the `with` operator set new values for properties. I’ve come up with the pattern below – which I think works, but is ever so messy. Adopting this pattern wouldn’t require every new parameter in the parent record to be reflected in the nested type – only for parameters used in computed properties.

1

2

3

4

5

6

7

8

9

10

11

12

13

14

15

16

17

18

19

20

21

22

23

24

25

26

27

`public` `sealed` `record Number(``int` `Value)`

`{`

    `private` `readonly` `Lazy<ComputedMembers> computed =`

        `new``(() =>` `new``(Value), LazyThreadSafetyMode.ExecutionAndPublication);`

    `public` `bool` `Even => computed.Value.Even;`

    `private` `Number(Number other)`

    `{`

        `Value = other.Value;`

        `// Defer creating the ComputedMembers instance until`

        `computed =` `new``(() =>` `new``(``this``), LazyThreadSafetyMode.ExecutionAndPublication);`

    `}`

    `// This is a struct (or could be a class) rather than a record,`

    `// to avoid creating a field for Value. We only need the computed properties.`

    `// (We don't even really need to use a primary`

    `// constructor, and in some cases it might be best not to.)`

    `private` `struct` `ComputedMembers(``int` `Value)`

    `{`

        `internal` `ComputedMembers(Number parent) :` `this``(parent.Value)`

        `{`

        `}`

        `public` `bool` `Even {` `get``; } = (Value & 1) == 0;`

    `}`

`}`

This is:

*   Painful to remember to do
*   A lot of extra code to start with (although after it’s been set up, adding a new computed member isn’t too bad)
*   Inefficient in terms of memory, due to adding a `Lazy<T>` instance

The inefficiency is likely to be irrelevant in “large” records, but it makes it painful to use computed properties in “small” records with only a couple of parameters, particularly if those are just numbers etc.

### Option 4: Request a change to the language

I bring this up only for completeness. I place a lot of trust in the C# design team: they’re smart folks who think things through very carefully. I would be shocked to discover that I’m the first person to raise this “problem”. I think it’s much more likely that the pros and cons of this behaviour have been discussed at length, and alternatives discussed and prototyped, before landing on the current behaviour as the least-worst option.

Now maybe the Roslyn compiler could start raising warnings (option 2) so that I don’t have to write an analyzer – and maybe there are alternatives that could be _added_ to C# for later versions (ideally giving more flexibility for initialization within records in general, e.g. a specially named member that is invoked when the instance is “ready” and which can still write to read-only properties)… but I’m probably not going to start creating a proposal for that without explicit encouragement to do so.

## Conclusion

It’s very rare that I discover a [footgun](https://en.wiktionary.org/wiki/footgun) in C#, but this really feels like one to me. Maybe it’s only because I’ve used computed properties so extensively in my election site – maybe records really aren’t designed to be used like this, and half of my record types should really be classes instead.

I don’t want to stop using records, and I’m definitely not encouraging anyone else to do so either. I don’t want to stop using the `with` operator, and again I’m not encouraging anyone else to do so. I hope this post will serve as a bit of a wake-up call to anyone who is using `with` in an unsound way though.

Oh, and of course if I _do_ write a Roslyn analyzer capable of detecting this, I’ll edit this post to link to it.

### Share this:

*   [Click to share on X (Opens in new window) X](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?share=twitter&nb=1)
*   [Click to share on Facebook (Opens in new window) Facebook](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?share=facebook&nb=1)

Like Loading...

### _Related_

[Records and Collections](https://codeblog.jonskeet.uk/2025/03/27/records-and-collections/ "Records and Collections")March 27, 2025In "Election 2029"

[Election 2029: Data Models](https://codeblog.jonskeet.uk/2025/03/16/election-2029-data-models/ "Election 2029: Data Models")March 16, 2025In "Election 2029"

[How many 32-bit types might we want?](https://codeblog.jonskeet.uk/2014/01/30/how-many-32-bit-types-might-we-want/ "How many 32-bit types might we want?")January 30, 2014In "General"

# Post navigation

[Previous PostElection 2029: Postcodes](https://codeblog.jonskeet.uk/2025/04/13/election-2029-postcodes/)

## 13 thoughts on “Unexpected inconsistency in records”

1.  ![Kyralessa's avatar](https://2.gravatar.com/avatar/20699ed8e9fe121c02e5ca5c7e2fc24798dd428fce1cc9bf3212d5c57d2f40c4?s=34&d=identicon&r=G) **Kyralessa** says:
    
    [July 20, 2025 at 6:20 am](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/#comment-73902)
    
    I dunno, seems a bit like a “Doctor, it hurts when I do this” kind of problem. If a field is dependent on the current, actual value of another field, and not just on the initialized value of that field, then clearly it should _always_ be computed, not only on initialization.
    
    But then I don’t use records myself ever since I discovered that field-level equality doesn’t extend to fields that are collections. With that caveat records just aren’t useful enough to justify themselves in my mind. Better to write a class with fully custom equality.
    
    [Like](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?like_comment=73902&_wpnonce=2c599d00ed)Like
    
    [Reply](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?replytocom=73902#respond)
    
    1.  ![jonskeet's avatar](https://2.gravatar.com/avatar/22e44f8d2ea51022ed71080e3105f5b775da0533a113085ac59dade5c17ed18d?s=34&d=identicon&r=G) **[jonskeet](https://jonskeetcodingblog.wordpress.com)** says:
        
        [July 20, 2025 at 6:38 am](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/#comment-73903)
        
        Given that records are generally intended to be immutable (and the ones I create are deeply so), I wouldn’t expect there to be any difference between “the initialized value” and “the current actual value”. That’s what caused the surprise.
        
        It turns out that they’re “one time mutable” – after field initializes have been run, but before the instance is otherwise observable. That’s the surprising part, to me.
        
        [Like](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?like_comment=73903&_wpnonce=33564b2e50)Like
        
        [Reply](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?replytocom=73903#respond)
        
        1.  ![Mark Adamson's avatar](https://graph.facebook.com/v6.0/61303068/picture?type=large) **Mark Adamson** says:
            
            [July 20, 2025 at 10:28 am](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/#comment-73904)
            
            I default to using computed property, but it doesn’t work for lazy initialisation as you say.
            
            I think a lazy language feature would be a great solution to this and I’m sure they would make it behave properly when cloning records
            
            [Like](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?like_comment=73904&_wpnonce=bb8341c29e)Like
            
            [Reply](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?replytocom=73904#respond)
            
2.  ![ericlippert's avatar](https://1.gravatar.com/avatar/1fcfc06c030e7f7556442b8bd00f08a8833ecfde8916576f26a1851d84d53c98?s=34&d=identicon&r=G) **[ericlippert](http://ericlippert.com)** says:
    
    [July 20, 2025 at 7:11 pm](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/#comment-73905)
    
    You are not the only person to run into this misunderstanding; here’s another:
    
    [https://langdev.stackexchange.com/questions/4372](https://langdev.stackexchange.com/questions/4372)
    
    [Like](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?like_comment=73905&_wpnonce=b6a6de955e)Like
    
    [Reply](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?replytocom=73905#respond)
    
3.  ![macias's avatar](https://0.gravatar.com/avatar/6e29c492d0bb1debc0bde254dc4432bbfc87b5791c9002e31d1844c0bc71034e?s=34&d=identicon&r=G) **[macias](http://przypadkopis.wordpress.com/)** says:
    
    [July 21, 2025 at 8:31 am](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/#comment-73906)
    
    “Records don’t allow you to specify constructor bodies for the primary constructor”. I use mainly record structs and they do, but to be sure I checked the class record, and… I see no error:  
    “\`
    
    public sealed record class Number  
    {  
    public Number(int Value)  
    {  
    this.Value = Value;  
    Even = (Value & 1) == 0;  
    }
    
    public bool Even { get; }  
    public int Value { get; init; }
    
    public void Deconstruct(out int Value)  
    {  
    Value = this.Value;  
    }  
    }  
    “\`
    
    [Like](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?like_comment=73906&_wpnonce=a962f04706)Like
    
    [Reply](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?replytocom=73906#respond)
    
    1.  ![jonskeet's avatar](https://2.gravatar.com/avatar/22e44f8d2ea51022ed71080e3105f5b775da0533a113085ac59dade5c17ed18d?s=34&d=identicon&r=G) **[jonskeet](https://jonskeetcodingblog.wordpress.com)** says:
        
        [July 21, 2025 at 8:45 am](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/#comment-73907)
        
        That’s not a primary constructor though. Primary constructors are the ones declared as part of the class/record declaration _before_ the body of the class, as per [https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/primary-constructors](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-12.0/primary-constructors)
        
        [Like](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?like_comment=73907&_wpnonce=2386ab4232)Liked by [1 person](#)
        
        [Reply](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?replytocom=73907#respond)
        
4.  Pingback: [Dew Drop – July 21, 2025 (#4463) – Morning Dew by Alvin Ashcraft](https://www.alvinashcraft.com/2025/07/21/dew-drop-july-21-2025-4463/)
    
5.  ![Nick's avatar](https://0.gravatar.com/avatar/098316b5aaeb9159002f3c16252ce9f9f437bc7e2bc630bab13f65efd22fbc7c?s=34&d=identicon&r=G) **Nick** says:
    
    [July 21, 2025 at 9:22 pm](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/#comment-73909)
    
    I agree this is confusing and a potential source of bugs. After reading Eric’s response on Stack Exchange, I realize how someone expects this to work might come down to how they read \`with\`:
    
    (1) “syntactic sugar to replace a constructor call”
    
    HighScoreEntry entry = new(“Jon”, 5000, 50);  
    var updatedEntry = new(entry.PlayerName, 6000, 55);
    
    or,
    
    (2) “copy and write to read-only properties”
    
    HighScoreEntry entry = new(“Jon”, 5000, 50);  
    var updatedEntry = new(entry) {  
    Score = 6000, Level = 55  
    }
    
    However there is some consistency between what you see using “with” and object initialization since you can also run into this by doing:
    
    var n = new Number(10) {  
    Value = 5  
    }
    
    Will show n.Even = true.
    
    Maybe that’s why “with” was designed to behave as it does?
    
    [Like](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?like_comment=73909&_wpnonce=d6c5da0313)Like
    
    [Reply](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?replytocom=73909#respond)
    
6.  Pingback: [Unexpected inconsistency in records - Data Debug Spot](https://eidm.co.za/2025/07/22/unexpected-inconsistency-in-records/)
    
7.  ![dmo's avatar](https://0.gravatar.com/avatar/682745a13cf587623484454f4225d0ec112a8f9c9346ba86bb3b6aadeca7e4ef?s=34&d=identicon&r=G) **dmo** says:
    
    [July 24, 2025 at 11:07 pm](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/#comment-73912)
    
    Another solution consistent with the field-copy behavior is to pass in a computed-value cache instance rather than caching the value itself in the record. This has the benefit that if the changed properties don’t impact the compound-key for a particular computed property, it wouldn’t need to be recomputed.
    
    [Like](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?like_comment=73912&_wpnonce=02f151f755)Like
    
    [Reply](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?replytocom=73912#respond)
    
8.  ![Brandon's avatar](https://2.gravatar.com/avatar/e874173ca44f591587ee1efd8c16813e1cebb0d8dae5f4cd9bd8de74adbec056?s=34&d=identicon&r=G) **Brandon** says:
    
    [July 28, 2025 at 6:31 am](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/#comment-73913)
    
    Yeah, agreed that it isn’t exactly intuitively obvious at first glance that the primary constructor of a record, whether it is just a positional record, or a full record with a primary constructor, is a bit more special than your typical primary constructor, unless you’ve read the docs already. Nor is it immediately intuitively obvious that the generated copy constructor copies _all_ fields, and does not, in fact, GAF about properties at all.
    
    \`with\` is shorthand for a call to the copy constructor _with the following block being an object initializer that follows the normal rules of an object initializer._
    
    If you mutate anything in the with statement, the field _did_ get copied, first, then field initializers in the record definition, if any, are executed, and _then_ the initializer in the with statement is executed before you are handed your shiny new mutated record instance.
    
    So you can as much as triple-assign a field if you have an explicit backing field with an initializer for the corresponding primary constructor parameter, and then mutate it in a with statement.
    
    And you can mix the behavior of positional properties and primary constructor parameters, which is extra non-obvious.
    
    Consider this record, which demonstrates these concepts:
    
    public record ExampleRecord(int A, int B)  
    {  
    public int A  
    {  
    get => field;  
    set  
    {  
    Console.WriteLine($”Called setter for {nameof(A)}. Previous value was {field}.”);  
    field = value;  
    }  
    }  
    }
    
    ExampleRecord thing1 = new(1,1);  
    Console.WriteLine(JsonSerializer.Serialize(thing1));
    
    Console.WriteLine(“Copying without mutation”);  
    ExampleRecord thing2 = thing1 with {};  
    Console.WriteLine(JsonSerializer.Serialize(thing2));
    
    Console.WriteLine(“Directly calling set accessor for A”);  
    thing2.A = 2;  
    Console.WriteLine(JsonSerializer.Serialize(thing2));
    
    Console.WriteLine(“Copying with mutation of A only”);  
    ExampleRecord thing3 = thing2 with { A = 3 };  
    Console.WriteLine(JsonSerializer.Serialize(thing3));
    
    Console.WriteLine(“Copying with mutation of A and B”);  
    ExampleRecord thing4 = thing3 with { A = 4, B = 4 };  
    Console.WriteLine(JsonSerializer.Serialize(thing4));
    
    Console.WriteLine(“Copying with mutation of B only”);  
    ExampleRecord thing5 = thing4 with { B = 5 };  
    Console.WriteLine(JsonSerializer.Serialize(thing5));
    
    Output:
    
    {“B”:1,”A”:0}  
    Copying without mutation  
    {“B”:1,”A”:0}  
    Directly calling set accessor for A  
    Called setter for A. Previous value was 0.  
    {“B”:1,”A”:2}  
    Copying with mutation of A only  
    Called setter for A. Previous value was 2.  
    {“B”:1,”A”:3}  
    Copying with mutation of A and B  
    Called setter for A. Previous value was 3.  
    {“B”:4,”A”:4}  
    Copying with mutation of B only  
    {“B”:5,”A”:4}
    
    As you can see, A was never set in the first record, as it is merely a parameter of the primary constructor with no auto-generated property or backing field associated with it, so it is just lost.  
    The only time it ever got assigned the value we asked for was when we set it explicitly, either directly via the set accessor or in the initializer of a with statement.
    
    B was set in all four because it is a positional property and has an auto-generated backing field that the generator is aware of.
    
    But the backing field of A is always copied, regardless, in the auto-generated copy constructor, and is set directly, which the mutations for thing3 and thin4 illustrate in their output, when the initializer is run and sets the property, resulting in the output with the previous value, which is the copied value.
    
    thing5 proves that the backing field is copied directly for A even though it’s not a positional property, as it has the value from thing4 and did not result in output for calling the set accessor.
    
    I used the field keyword here, but the behavior is identical if you explicitly declare the backing field for A.
    
    However, you can fix it all if you declare that backing field and use an initializer. That makes A behave the same as if it were a positional property, _plus_ also call the setter if you mutate it in the with statement.
    
    I was pleasantly surprised, though, to note that intellisense in Visual Studio is aware of the difference between the two subtly different symbols A and B, in the primary constructor, as were analyzers yelling at me for never using them. They all correctly referred to A, in the primary constructor, as “parameter A” and referred to B as “positional property B” even though they both have properties of the same name, in the final generated record.
    
    [Like](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?like_comment=73913&_wpnonce=cc606bf4fd)Like
    
    [Reply](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?replytocom=73913#respond)
    
9.  ![Brandon's avatar](https://2.gravatar.com/avatar/e874173ca44f591587ee1efd8c16813e1cebb0d8dae5f4cd9bd8de74adbec056?s=34&d=identicon&r=G) **Brandon** says:
    
    [July 28, 2025 at 6:50 am](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/#comment-73914)
    
    All that is to say that I don’t really think there’s an inconsistency here.
    
    Those properties behave the same way in a struct, when you use a with statement to copy a struct (not just a record struct), as well as in a normal class that has a primary constructor, which is then instantiated using an object initializer. It’s just a consequence of realizing you’re in a static context in those initializers, if you’re referring to a symbol that happens to appear in a primary constructor.
    
    So, lazy compute-and-store properties that are set with static initializers are more of a design flaw in the type being written than a language flaw, I’d argue.
    
    [Like](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?like_comment=73914&_wpnonce=3207f1cf82)Like
    
    [Reply](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?replytocom=73914#respond)
    
10.  ![Andrew Rondeau's avatar](https://0.gravatar.com/avatar/00b4894d21c5a40ca8ec52f760a11a479e62d5f65e9bc3edf7df6212a6da0042?s=34&d=identicon&r=G) **Andrew Rondeau** says:
    
    [July 28, 2025 at 1:32 pm](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/#comment-73915)
    
    > But that really wouldn’t work for a lot of the properties in the records I use in the election site, where often the record is constructed with collections which are then indexed by ID, or other relatively expensive computations are performed.
    
    I suspect that records might not be the right thing to do here.
    
    In a prior job, before records, we had immutable classes implemented the “old” way.
    
    A developer came up with a copy pattern where the Clone method had optional arguments that would default to null. Leaving the argument null told the clone method to keep the value. For properties that allowed null, we had a simple wrapper type that would easily typecast.
    
    I suspect this approach would be better in your case, because it allows the kind of nuanced logic on copy that you need.
    
    [Like](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?like_comment=73915&_wpnonce=95ddabe2ce)Like
    
    [Reply](https://codeblog.jonskeet.uk/2025/07/19/unexpected-inconsistency-in-records/?replytocom=73915#respond)
    

### Leave a comment [Cancel reply](/2025/07/19/unexpected-inconsistency-in-records/?utm_source=bonobopress&utm_medium=newsletter&utm_campaign=2096#respond)

   

Δdocument.getElementById( "ak\_js\_1" ).setAttribute( "value", ( new Date() ).getTime() );