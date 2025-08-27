# Some more C# 12 | Red Hat Developer

```markdown
**Source:** https://developers.redhat.com/articles/2024/04/30/some-more-c-12?utm_source=newsletter.csharpdigest.net&utm_medium=newsletter&utm_campaign=adventures-serializing-absolutely-everything-in-c#
**Date Captured:** 2025-07-28T16:27:35.133Z
**Domain:** developers.redhat.com
**Author:** Unknown
**Category:** ai_ml
```

---

# Some more C# 12

Advanced features

April 30, 2024

[Tom Deseyn](/author/tom-deseyn)

Related topics:

[.NET](/topics/dotnet)[Linux](/topics/linux)

Related products:

[Red Hat Enterprise Linux](/products/rhel/overview)

### Share:

[](/#twitter)[](/#facebook)[](/#linkedin)[](/#email)

Table of contents:

*   [Inline arrays](#inline_arrays)
*   [Optional parameters and params in lambda expressions](#optional_parameters_and_params_in_lambda_expressions)
*   [Ref readonly parameters](#ref_readonly_parameters)
*   [Alias any type](#alias_any_type)
*   [UnsafeAccessorAttribute](#unsafeaccessorattribute)
*   [Conclusion](#conclusion)

In the [previous article on C# 12](https://developers.redhat.com/articles/2024/04/22/c-12-collection-expressions-and-primary-constructors), you learned about collection expressions and primary constructors. In this article, we’ll take a look at some advanced features that part of the latest C# version: inline arrays, optional params and params in lambda expressions, `ref readonly` parameters, aliasing any type, and the `UnsafeAccessorAttribute`.

Inline arrays

## Inline arrays

A regular C# array is a reference type that lives on the heap. Like other reference types, the garbage collector (GC) keeps track whether the array is still referenced, and it frees up memory when the array is no longer in use.

To avoid the GC overhead in performance sensitive code, when a small array is needed that is local to a function, it can be allocated on the stack using `stackalloc`. Thanks to the `Span<T>` type introduced in .NET Core 2.1 we can use such arrays without resorting to "unsafe" code.

```cs
int[] bufferOnHeap = new int[1024];
Span<int> bufferOnStack = stackalloc int[128];
```

Copy snippet

C# also allows us to allocate memory for an array as part of a struct. This can be interesting for performance, and also for interop to match a native type’s layout. Before C# 12, such arrays were declared using the `fixed` keyword, limited to primitive numeric types, and required using `unsafe` code. The following code compiles and makes no issue about the illegal out-of-bound access at compile time or run time.

```cs
Foo();

unsafe void Foo() {
  MyStruct s = default;
  s.Buffer[15] = 20; // Out-of-bounds access not caught.
}

unsafe struct MyStruct {
  public fixed byte Buffer[10];
}
```

Copy snippet

C# 12 improves the situation, and allows declaring inline arrays and accessing them in a safe way. The buffer must be declared as a `struct` type with a single field for the element type and an `InlineArray` attribute with the length. The element type is also no longer limited to primitive numeric types. When we update the previous example to C# 12, at compile time, we get an error for the out-of-bounds access.

```cs
void Foo() {
  MyStruct s = default;
  s.Buffer[15] = 20; // CS9166: out-of-bounds access
}

struct MyStruct {
  public MyBuffer Buffer;
}

[InlineArray(10)]
struct MyBuffer {
  private byte _element;
}
```

Copy snippet

As shown in the example, the buffer type supports indexing using an `int`. Indexing using an `Index` or `Range` type also works.

The buffer type also converts implicity to `Span<T>` and `ReadOnlySpan<T>`, and it can also be used in a `foreach`.

You can add members to the buffer type that operate on the stored data.

Optional parameters and params in lambda expressions

## Optional parameters and params in lambda expressions

C# 12 allows lambda expressions to have default parameters as shown in the next example.

```cs
var prefixer = (string value, string prefix = "_")
                  => $"{prefix}{value}";

Console.WriteLine(prefixer("name"));
Console.WriteLine(prefixer("name", "$"));
```

Copy snippet

We’ve used the `var` keyword as the target type of the lambda expression. Under the hood, the compiler will define a delegate type that stores the optional parameter values as shown in this expanded example.

```cs
// The optional values are captured in the delegate type.
delegate string PrefixerDelegate(string value, string prefixer = "_");

PrefixerDelegate prefixer = (string value, string prefix)
                               => $"{prefix}{value}";
```

Copy snippet

C# 12 also allows to use `params` in a lambda expressions.

```cs
var adder = (params int[] numbers)
                => numbers.Sum();

int sum = adder(1, 2, 3);
```

Copy snippet

Similar to the optional parameters, the `params` is captured in the delegate type.

Ref readonly parameters

## Ref readonly parameters

C# 7.2 introduced the `in` keyword which enables passing a value by reference while not allowing the value to be modified.

```cs
MyStruct s = new MyStruct { I = 1 };

Foo(s); // or: Foo(in s);

void Foo(in MyStruct value) {
  value.I = 10; // CS8332: value is readonly
}

struct MyStruct {
  public int I;
}
```

Copy snippet

As shown in the previous example, the caller is not required to use the `in` keyword when passing the variable. `in` arguments are also not limited to passing variables. As shown in the following example, we can pass temporary values that are not in scope before/after the call.

```cs
Foo(Bar());
Foo(default(MyStruct));

MyStruct Bar() { .. }
```

Copy snippet

C# 12 introduces passing values as `ref readonly`. In contrast to `in`, the caller is required to specify the `ref` keyword. This means the latter examples are no longer allowed because the temporary values passed in are not referenceable. This allows us to better capture the semantics of some APIs, like when calling `ReadOnlySpan(ref readonly T reference)` as shown in the next example.

```cs
MyStruct value = default;

// Calling ReadOnlySpan(ref readonly T reference)
// allows passing a referenceable value:
var span = new ReadOnlySpan<MyStruct>(ref value);
span[0] = ..; // operates on the referenced value

// and disallows passing a non-referenceable value:
var span = new ReadOnlySpan<int>(ref CreateMyStruct()); // CS1510: ref must be an assignable variable

MyStruct CreateMyStruct() => default;

struct MyStruct
{ }
```

Copy snippet

Alias any type

## Alias any type

C# type aliases were restricted to using the full type names:

```cs
using Int = System.Int32;
using TupleOfInts = System.ValueTuple<int, int>;
```

Copy snippet

While C# 12 allows us to use any C# type declarations:

```cs
using Int = int;
using TupleOfInts = (int, int);
using unsafe Pointer = int*;
```

Copy snippet

UnsafeAccessorAttribute

## UnsafeAccessorAttribute

Serializers require access to inaccessible members of types. Previously this was only achievable using reflection. .NET 8 is introducing the `UnsafeAccessorAttribute` which allows to do this without using reflection. This improves performance, enables source-generators to access these members, and it works well with NativeAOT.

The inaccessible members are made accessible by declaring an extern method declaration with an appropriate signature and adding the `UnsafeAccessorAttribute` to identify the member. The runtime will provide the implementation for these methods. If the member is not found, calling the method will throw `MissingFieldException` or `MissingMethodException`.

The following example shows calling a private constructor, and calling a private property getter.

```cs
using System.Runtime.CompilerServices;

MyClass instance = Ctor(1);
int value = GetPrivateProperty(instance);

[UnsafeAccessor(UnsafeAccessorKind.Constructor)]
extern static MyClass Ctor(int i);

[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "get_PrivateProperty")]
extern static int GetPrivateProperty(MyClass c);

public class MyClass {
   MyClass(int i) { PrivateProperty = i; }
   int PrivateProperty { get ; }
}
```

Copy snippet

The [UnsafeAccessorAttribute](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.compilerservices.unsafeaccessorattribute) documentation provides a full overview on how to access different members. Support for generic parameters is added as part of .NET 9.

Conclusion

## Conclusion

In this second and final article on C# 12, we looked at inline arrays, optional params and params in lambda expressions, `ref readonly` parameters, aliasing any type, and the `UnsafeAccessorAttribute`. These new features improve C# for specific use cases.

## Related Posts

*   ### [C# 12: Collection expressions and primary constructors](/articles/2024/04/22/c-12-collection-expressions-and-primary-constructors)
    
*   ### [C# 9 top-level programs and target-typed expressions](/blog/2021/03/30/c-9-top-level-programs-and-target-typed-expressions)
    
*   ### [C# 9 pattern matching](/blog/2021/04/06/c-9-pattern-matching)
    
*   ### [Improvements to static analysis in the GCC 14 compiler](/articles/2024/04/03/improvements-static-analysis-gcc-14-compiler)
    
*   ### [Three ways to containerize .NET applications on Red Hat OpenShift](/blog/2021/03/16/three-ways-to-containerize-net-applications-on-red-hat-openshift)
    
*   ### [Containerize .NET for Red Hat OpenShift: Linux containers and .NET Core](/blog/2021/04/15/containerize-net-for-red-hat-openshift-linux-containers-and-net-core)
    

## Recent Posts

*   ### [Integrate Red Hat build of Trustee with the External Secrets Operator](/articles/2025/07/28/integrate-red-hat-build-trustee-external-secrets-operator)
    
*   ### [Confidential virtual machines vs VMs: Latency analysis](/articles/2025/07/28/confidential-virtual-machines-vs-vms-latency-analysis)
    
*   ### [Enable Custom Logos branding in the OpenShift web console](/articles/2025/07/25/enable-custom-logos-branding-openshift-web-console)
    
*   ### [Submit remote RayJobs to a Ray cluster with the CodeFlare SDK](/articles/2025/07/24/submit-remote-rayjobs-ray-cluster-codeflare-sdk)
    
*   ### [Secure service-to-service authentication in Developer Hub](/articles/2025/07/24/secure-service-service-authentication-developer-hub)