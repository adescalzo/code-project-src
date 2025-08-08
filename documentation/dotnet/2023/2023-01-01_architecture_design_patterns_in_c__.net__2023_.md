# Design Patterns In C# .NET (2023)

**Source:** https://www.c-sharpcorner.com/UploadFile/bd5be5/design-patterns-in-net/
**Date Captured:** 2025-07-28T16:06:38.852Z
**Domain:** www.c-sharpcorner.com
**Author:** Kanchan Naik
**Category:** architecture

---

*   Collapse
*   [Feed](/login?returnurl=/myaccount)
*   [Dashboard](/login?returnurl=/authors/dashboard.aspx)
*   [Wallet](/login?returnurl=/wallet "Wallet")
*   [Learn](/learn)
*   [Achievements](/login?returnurl=/achievements)
*   [Network](/network)
*   [Refer](/refer-and-earn)
*   [Rewards](/rewards)
*   [SharpGPT](http://sharp-gpt.ai/)
*   [Premium](/membership)
*   Contribute
    *   [Article](../../../publish/createarticle.aspx "Contribute an Article")
    *   [Blog](../../../blogs/createblog.aspx "Contribute a Blog")
    *   [Video](../../../publish/createarticle.aspx?type=videos "Contribute a Video")
    *   [Ebook](../../../aboutebookposting.aspx "Contribute an Ebook")
    *   [Interview Question](../../../interviews/question/postquestion.aspx "Contribute an Interview Question")

*   [Register](https://www.c-sharpcorner.com/register)
*   [Login](/userregistration/logincheck.aspx?returnurl=/uploadfile/bd5be5/design-patterns-in-net/)

// Get references to the elements const hoverTarget = document.getElementById('ctl00\_mainleft\_userProfImg'); const hiddenDiv = document.getElementById('userLinks'); // Add event listeners for mouse enter and leave if (hoverTarget != null) { hoverTarget.addEventListener('mouseenter', () => { // Show the hidden div when mouse enters hiddenDiv.style.display = 'block'; }); } if (hoverTarget != null) { hoverTarget.addEventListener('mouseleave', () => { // Hide the hidden div when mouse leaves setTimeout(function () { hiddenDiv.style.display = 'none'; }, 500); }); } document.addEventListener("DOMContentLoaded", function () { var listItems = document.querySelectorAll(".my-account-menu > li"); listItems.forEach(function (item) { item.addEventListener("mouseover", function () { setTimeout(function () { // Add your hover action here item.classList.add('hovered'); }, 500); // 500 milliseconds delay }); item.addEventListener("mouseout", function () { setTimeout(function () { item.classList.remove('hovered'); }, 500); }); }); });

 [![Prompt Engineering Training](https://www.c-sharpcorner.com/UploadFile/Ads/7.png)](https://viberondemand.com/prompt-engineering)document.addEventListener("DOMContentLoaded", function () { setTimeout(() => { const img = document.getElementById("imgAdslazyImageOnTop1"); img.src = img.getAttribute("data-src"); img.style.display = "block"; }, 3000); });

 [![C#](https://www.c-sharpcorner.com/UploadFile/MinorCatImages/csharp-programming_110028775.png "C#")](https://www.c-sharpcorner.com/technologies/csharp-programming) 

# Design Patterns In C# .NET (2023)

[](/featured-articles "Featured")

*   [](https://www.facebook.com/sharer.php?u=https://www.c-sharpcorner.com/UploadFile/bd5be5/design-patterns-in-net/ "Share on Facebook")
*   [](https://twitter.com/intent/tweet?&via=CsharpCorner&related=CsharpCorner&text=Design+Patterns+In+C%23+.NET+\(2023\) https://www.c-sharpcorner.com/UploadFile/bd5be5/design-patterns-in-net/ "Share on Twitter")
*   [](https://www.linkedin.com/shareArticle?mini=true&url=https://www.c-sharpcorner.com/UploadFile/bd5be5/design-patterns-in-net/ "Share on Linkedin")
*   [](//www.reddit.com/submit?title=Design+Patterns+In+C%23+.NET+\(2023\)&url=https%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fbd5be5%2fdesign-patterns-in-net%2fdefault.aspx "Share on Reddit")
*   [WhatsApp](whatsapp://send?text=Design Patterns In C# .NET \(2023\)%0A%0Ahttps%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fbd5be5%2fdesign-patterns-in-net%2f "Share on Whatsapp")
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/articles/emailtofriend.aspx?articleid=5f1da82c-7b51-4583-b71c-6ce7cd887519 "Email this article to friend")
*   [](javascript:void\(0\); "Bookmark this article")
*   [](javascript:void\(GetPrintVersion\('Article'\)\) "Print")
*   [](https://www.c-sharpcorner.com/members/kanchan-naik2/articles "Author's other article")

*    [![Author Profile Photo](https://www.c-sharpcorner.com/uploadfile/authorimage/defaultauthorimage.jpg.ashx?height=24&width=24 "Kanchan Naik")](https://www.c-sharpcorner.com/members/kanchan-naik2)[Kanchan Naik](https://www.c-sharpcorner.com/members/kanchan-naik2)
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/uploadfile/bd5be5/design-patterns-in-net/history "Article history")2y
*   2.6m
*   24
*   80
    
[100](/members/rank.aspx?page=points-table "Points")*   [Article](/articles/)

Take the challenge 

[Designpatterns.rar](javascript:void\(0\); "Designpatterns.rar")

## Design Patterns In C#

Design patterns provide general solutions or a flexible way to solve common design problems. This article introduces design patterns and how design patterns are implemented in C# and .NET.

Before starting with design patterns in .NET, let's understand the meaning of design patterns and why they are useful in software architecture and programming.

## What are Design Patterns in Software Development?

Design Patterns in the object-oriented world are a reusable solution to common software design problems that repeatedly occur in real-world application development. It is a template or description of how to solve problems that can be used in many situations.

"_A pattern is a recurring solution to a problem in a context._"

"_Each pattern describes a problem that occurs over and over again in our environment and then describes the core of the solution to that problem in such a way that you can use this solution a million times over without ever doing it the same way twice._" - Christopher Alexander, _A Pattern Language_.

Developers use patterns for their specific designs to solve their problems. Pattern choice and usage among various design patterns depend on individual needs and concerns. Design patterns are a very powerful tool for software developers. It is important to understand design patterns rather than memorizing their classes, methods, and properties. Learning how to apply patterns to specific problems is also important to get the desired result. This will require continuous practice using and applying design patterns in software development. First, identify the software design problem, then see how to address these problems using design patterns and determine the best-suited design problem to solve the problem.

There are 23 design patterns, also known as Gang of Four (GoF) design patterns. The Gang of Four is the authors of the book, "Design Patterns: Elements of Reusable Object-Oriented Software". These 23 patterns are grouped into three main categories:

![Design Patterns In CSharp](https://www.c-sharpcorner.com/UploadFile/bd5be5/design-patterns-in-net/Images/Design Patterns In DotNet.jpg)

## Creational Design Pattern

1.  Factory Method
2.  Abstract Factory
3.  Builder
4.  Prototype
5.  Singleton

## Structural Design Patterns

1.  Adapter
2.  Bridge
3.  Composite
4.  Decorator
5.  Façade
6.  Flyweight
7.  Proxy

## Behavioral Design Patterns

1.  Chain of Responsibility
2.  Command
3.  Interpreter
4.  Iterator
5.  Mediator
6.  Memento
7.  Observer
8.  State
9.  Strategy
10.  Visitor
11.  Template Method

In this article, we learn and understand Creational Design Patterns in detail, including a UML diagram, template source code, and a real-world example in C#. Creational Design Patterns provide ways to instantiate a single object or group of related objects. These patterns deal with the object creation process in such a way that they are separated from their implementing system. That provides more flexibility in deciding which object needs to be created or instantiated for a given scenario. There are the following five such patterns.

## Abstract Factory

This creates a set of related objects or dependent objects. The "family" of objects created by the factory is determined at run-time depending on the selection of concrete factory classes.

An abstract factory pattern acts as a super-factory that creates other factories. An abstract factory interface creates a set of related or dependent objects without specifying their concrete classes. 

The UML class diagram below describes an implementation of the abstract factory design pattern.

![Design Patterns In .NET](https://www.c-sharpcorner.com/UploadFile/bd5be5/design-patterns-in-net/Images/Design-Patterns-1.jpg)

The classes, objects, and interfaces used in the above UML diagram are described below.

1.  _Client_ This class uses the Abstract Factory and Abstract Product interfaces to create a family of related objects.
2.  _Abstract Factory_ This is an interface that creates abstract products.
3.  _Abstract Product_ This is an interface that declares a type of product.
4.  _Concrete factory_ This class implements the abstract factory interface to create concrete products.
5.  _Concrete Product_  This class implements the abstract product interface to create products.

The following code shows the basic template code of the abstract factory design pattern implemented using C#:

![Design Patterns In .NET](https://www.c-sharpcorner.com/UploadFile/bd5be5/design-patterns-in-net/Images/Design-Patterns-2.jpg)

![Design Patterns In .NET](https://www.c-sharpcorner.com/UploadFile/bd5be5/design-patterns-in-net/Images/Design-Patterns-3.jpg)

In the above abstract factory design pattern, the source code template client has two private fields that hold the instances of abstract product classes. These objects will be accessed by inheriting their base class interface. When the client is instantiated, a concrete factory object is passed to its constructor and populated private fields of the client with appropriate data or values.

The Abstract factory is a base class for concrete factory classes that generate or create a set of related objects. This base class contains the definition of a method for each type of object that will be instantiated. The base class is declared Abstract so that other concrete factory subclasses can inherit it.

The concrete factory classes are inherited from the Abstract factory class and override the base class method to generate a set of related objects required by the client. Depending on the software or application requirements, there can be a specified number of concrete factory classes.

Abstractproduct is a base class for the types of objects that the factory class can create. There should be one base type for every distinct type of product required by the client.

The concrete product classes are inherited from Abstractproduct class. Each class contains specific functionality. Objects of these classes are generated from the Abstractfactory to populate the client.

## A real-world example of an Abstract factory design pattern using C#

For example, consider a system that does the packaging and delivery of items for a web-based store. The company delivers two types of products. The first is a standard product placed in a box and delivered through the post with a simple label. The second is a delicate item that requires shock-proof packaging and is delivered via a courier. In this situation, two types of objects are required: a packaging object and a delivery documentation object. We could use two factories to generate these related objects. One factory will create packaging and other delivery objects for standard parcels. The second will create packaging and delivery objects for delicate parcels. **Class Client**  

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AbstractFactoryPatternExample
{
   public class Client
    {
        private Packaging _packaging;
       private DeliveryDocument _deliveryDocument;
       public Client(PacknDelvFactory factory)
       {
           _packaging = factory.CreatePackaging();
           _deliveryDocument = factory.CreateDeliveryDocument();
       }
       public Packaging ClientPackaging
       {
           get { return _packaging; }
       }
       public DeliveryDocument ClientDocument
       {
           get { return _deliveryDocument; }
       }
    }
   public abstract class PacknDelvFactory
   {
       public abstract Packaging CreatePackaging();
       public abstract DeliveryDocument CreateDeliveryDocument();
   }
   public class StandardFactory : PacknDelvFactory
   {
       public override Packaging CreatePackaging()
       {
           return new StandardPackaging();
       }
       public override DeliveryDocument CreateDeliveryDocument()
       {
           return new Postal();
       }
   }
   public class DelicateFactory : PacknDelvFactory
   {
       public override Packaging CreatePackaging()
       {
           return new ShockProofPackaging();
       }
       public override DeliveryDocument CreateDeliveryDocument()
       {
           return new Courier();
       }
   }
   public abstract class Packaging { }
   public class StandardPackaging : Packaging { }
   public class ShockProofPackaging : Packaging { }
   public abstract class DeliveryDocument { }
   public class Postal : DeliveryDocument { }
   public class Courier : DeliveryDocument { }
}
```

C#

Copy

**AbstractFactory Patterns Form**

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace AbstractFactoryPatternExample
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            GetClientObject();
        }
        void GetClientObject()
        {
            PacknDelvFactory sf = new StandardFactory();
            Client standard = new Client(sf);
            label1.Text = standard.ClientPackaging.GetType().ToString();
            label2.Text = standard.ClientDocument.GetType().ToString();

            PacknDelvFactory df = new DelicateFactory();
            Client delicate = new Client(df);
            label3.Text = delicate.ClientPackaging.GetType().ToString();
            label4.Text = delicate.ClientDocument.GetType().ToString();
        }
    }
}
```

C#

Copy

**Output**

The example code above creates two client objects, each passing to a different type of factory constructor. Types of generated objects are accessed through the client's properties.  

**Note** While studying abstract factory patterns, one question is, what are concrete classes? So I Googled that, and the following answers my question. A concrete class is nothing but a normal class that has all basic class features, like variables, methods, constructors, and so on. We can create an instance of the class in other classes.

Here is a detailed article on [Abstract Factory Design Pattern In C#](https://www.c-sharpcorner.com/article/abstract-factory-design-pattern-in-c-sharp/)

## Singleton Design Pattern

The Singleton design pattern is one of the simplest design patterns. This pattern ensures that the class has only one instance and provides a global point of access to it. The pattern ensures that only one object of a specific class is ever created. All further references to objects of the singleton class refer to the same underlying instance.

There are situations in a project where we want only one instance of the object to be created and shared among the clients. No client can create an instance from outside. It is more appropriate than creating a global variable since it may be copied and lead to multiple access points.

The UML class diagram below describes an implementation of the abstract factory design pattern:

![Design Patterns In .NET](https://www.c-sharpcorner.com/UploadFile/bd5be5/design-patterns-in-net/Images/Design-Patterns-8.jpg)

The UML diagram above the "GetInstace" method should be declared static in the singleton patterns. This is because this method returns a single instance held in a private "instance" variable.  In the singleton pattern, all the methods and instances are defined as static. The static keyword ensures that only one instance of the object is created, and you can call methods of the class without creating an object.

The constructor of a class is marked as private. This prevents any external classes from creating new instances. The class is also sealed to prevent inheritance, which could lead to subclassing that breaks the singleton rules. 

The following code shows the basic template code of the singleton design pattern implemented using C#.

**The eager initialization of singleton pattern**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SingetonDesignPattern
{
    public sealed class SingeltonTemp
    {

        private static SingeltonTemp _instance;
        private static object _lockThis = new object();
        private SingeltonTemp()
        {
        }
        public static SingeltonTemp GetSingleton()
        {
            lock (_lockThis)
            {
                if (_instance == null) _instance = new SingeltonTemp();
            }
            return _instance;
        }
    }
}
```

C#

Copy

**Lazy initialization of singleton pattern**

****![Design Patterns In .NET](https://www.c-sharpcorner.com/UploadFile/bd5be5/design-patterns-in-net/Images/Design-Patterns-10.jpg)****

**Thread-safe (Double-checked Locking) initialization of singleton pattern**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SingetonDesignPattern
{
    public class Singleton
    {
        private static Singleton instance = null;
        private Singleton()
        {
        }
        private static object lockThis = new object();

        public static Singleton GetInstance
        {
            get
            {
                lock (lockThis)
                {
                    if (instance == null)
                        instance = new Singleton();

                    return instance;
                }
            }
        }
    }

}
```

C#

Copy

The code above shows the "lockThis" object and the use of locking within the "GetInstance" method. Since programs can be multithreaded, it is possible that two threads could request the singleton before the instance variable is initialized. By locking the dummy "lockThis" variable, all other threads will be blocked. This means that two threads will not be able to create their copies of the object simultaneously.

## A real-world example of a Singleton design pattern using C#.net

I am trying to apply this pattern in my application where I want to maintain an application state for user login information and any other specific information required to be instantiated only once and held in only one instance. 

**Class ApplicationState**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SingetonDesignPattern
{
    class ApplicationState
    {
        private static ApplicationState instance=null;
        // State Information
        public string LoginId
        { get; set;}
        public string RoleId
        { get; set; }

        private ApplicationState()
        {
        }
        //Lock Object
        private static object lockThis = new object();
        public static ApplicationState GetState()
        {
            lock (lockThis)
            {

                if (ApplicationState.instance == null)
                  instance = new ApplicationState();
            }
                return instance;
        }

    }
}
```

C#

Copy

**Singleton pattern form**

```csharp
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SingetonDesignPattern
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            GetStateInfo();
        }
        void GetStateInfo()
        {
            ApplicationState state = ApplicationState.GetState();
            state.LoginId="Kanchan";
            state.RoleId= "Admin";

            ApplicationState state2 = ApplicationState.GetState();
            label3.Text = state2.LoginId;
            label5.Text = state2.RoleId;
            label6.Text = (state == state2).ToString();
        }
    }
}
```

C#

Copy

**Output**

****![Design Patterns In .NET](https://www.c-sharpcorner.com/UploadFile/bd5be5/design-patterns-in-net/Images/Design-Patterns-14.jpg)****

The preceding sample code creates two new variables and assigns the return value of the GetState method to each. They are then compared to check that they both contain the same values and a reference to the same object.

Here is a detailed article on [Singleton Design Pattern In C#](https://www.c-sharpcorner.com/UploadFile/8911c4/singleton-design-pattern-in-C-Sharp/)

**Interview Questions** 

Going for an interview, here are [Interview Questions on Design Patterns.](https://www.c-sharpcorner.com/UploadFile/questpond/design-pattern-interview-questions/)  

## Summary 

In this article, you learned the basics of design patterns and how to implement design patterns in C# and .NET.

**Recommended Articles**

Here is a list of some highly recommended articles related to design patterns. 

1.  [Abstract Factory Design Pattern In C#](https://www.c-sharpcorner.com/article/factory-method-design-pattern-in-c-sharp/)
2.  [Factory Method Design Pattern In C#](https://www.c-sharpcorner.com/article/factory-method-design-pattern-in-c-sharp/) 
3.  [Singleton Design Pattern In C#](https://www.c-sharpcorner.com/UploadFile/8911c4/singleton-design-pattern-in-C-Sharp/)
4.  [Bridge Design Pattern In C#](https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-bridge-design-pattern/)
5.  [Prototype Design Pattern In C#](https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-3-prototype-design-patter/)
6.  [Decorator Design Pattern In C#](https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginners-part-4-decorator-design-patt/)
7.  [Composite Design Pattern In C#](https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-5-composite-design-patter/)

Here is a list of more [Design Patterns In C#.](https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginners-part-6-adaptor-design-patter/)

People also reading

﻿ .mentions-input-box textarea { max-width: inherit; } .mentions { display: none; } label\[for="FollowCheckBox"\] { position: relative; } .mentions-autocomplete-list { background: #12192d; margin-top: 10px; } .mentions-autocomplete-list ul li { display: flex; flex-direction: row; align-items: center; padding: 10px; border-bottom: 1px solid #fff; cursor: pointer; } .mentions-autocomplete-list ul li:hover { background: var(--featuredFeedbg); } .mentions-autocomplete-list ul li img { max-width: 40px; max-height: 40px; border-radius: 50%; margin-right: 15px; } .previousComments { display: flex; justify-content: right; cursor: pointer; } .comments-body-container { width: 100%; } .content-bottom-comment { margin-top: -20px; } .commetn-txt-box { width: 97% !important; } .build-comment { display: flex; margin-top: 20px; } .commentheading { color: var(--headingColor); font-size: var(--FontSize18); font-family: var(--sitefont); margin-bottom: 7px; } .commetn-txt-box::placeholder { color: #7F8AAB; } /\*For Internet Explorer 10-11\*/ .commetn-txt-box:-ms-input-placeholder { color: #7F8AAB; } /\* For Microsoft Edge\*/ .commetn-txt-box::-ms-input-placeholder { color: #7F8AAB; } @media (max-width: 767px) { .esc-msg { display: none !important; } } /\*popup css start\*/ .overlay-bg-response { background-color: rgba(0, 0, 0, .5); width: 100%; height: 100%; position: fixed; top: 0; bottom: 0; left: 0; z-index: 9999; } .popup-window-response { margin: auto; position: relative; top: 65px; background-color: var(--popupBg); height: auto; max-height: 660px; /\* min-height: 227px;\*/ overflow: hidden; overflow-y: auto; max-width: 600px; } .popup-wrap-response { /\*min-height: 227px;\*/ padding: 0px 20px 0px 20px; } .popup-header-response { text-align: center; background-color: var(--popupBg); padding: 10px; line-height: 30px; font-size: 18px; color: var(--fontColor); font-weight: bold; position: sticky; top: 0px; } .popup-heading-response, .popup-heading-response > span { font-size: 20px; color: var(--fontColor); font-weight: 600; line-height: 2; } .icon-close { cursor: pointer; } .icon-close::before { transform: rotate(45deg); } .icon-close::before, .icon-close::after { content: ""; display: block; position: absolute; top: 28px; right: 30px; background-color: #ffffff; width: 20px; height: 3px; border-radius: 2px; } .icon-close::after { transform: rotate(-45deg); } .popup-body-response { overflow-y: auto; display: flex; justify-content: center; padding: 12px 24px; overflow-y: auto; /\* min-height: 100px;\*/ align-items: center; } .messagebody { font-size: 18px; color: var(--textColor); font-weight: 300; } .popup-footer-response { width: auto; text-align: center; padding: 12px 0; position: relative; } #PopUp-body { display: none; } .user-image-56{ min-width:44px; min-height:44px; } /\*popup css end\*/

*   (24) Comments

View All Comments

Press Esc key to cancel

[![User Image](https://www.c-sharpcorner.com/UploadFile/AuthorImage/DefaultAuthorImage.jpg.ashx?width=40&height=40)](/login)

![Loading...](/images/csharp/ajax-loader-small.gif)

Load More

Message

OK Close

                

                

.event-calendar{ display:flex; flex-direction:column-reverse; background:linear-gradient(-180deg, rgba(93,107,148, 40%) 40%, #0086dc 40%); text-align: center; border-radius: 4px; min-width: 42px; max-width: 42px; min-height:50px; } .event-list-item{ display:flex; flex-direction:row; align-items:center; gap:15px; margin-bottom:15px; } .event-card-detail .event-title a { font-size: 1.1rem; color: transparent !important; background: linear-gradient(120deg, #00ffcd, #03a9f4); -webkit-background-clip: text; -webkit-text-fill-color: transparent; font-weight: 600; } .local-day{ font-size:20px; font-weight:600; height:31px; color:var(--textColor); } .local-month{ font-size:12px; color:var(--textColor); }

Upcoming Events

[View all](/chapters/)

*   31 JUL
    
    [Empowering Government Staff with Cloud & AI Technologies](https://www.c-sharpcorner.com/events/empowering-government-staff-with-cloud-ai-technologies2 "Empowering Government Staff with Cloud & AI Technologies")
    
*   5 AUG
    
    [Software Architecture Conference - 2025](https://www.c-sharpcorner.com/events/software-architecture-conference-2025 "Software Architecture Conference - 2025")
    
*   5 AUG
    
    [Automated Sales Quote Approval Workflow for Faster Deal Closure: Automate Your Business Processes - Ep.9](https://www.c-sharpcorner.com/events/automated-sales-quote-approval-workflow-for-faster-deal-closure-automate-your-business-processes-ep9 "Automated Sales Quote Approval Workflow for Faster Deal Closure: Automate Your Business Processes - Ep.9")
    

 

Ebook download

[View all](/ebooks)

[![C# Corner Ebook](https://www.c-sharpcorner.com/UploadFile/EBooks/04092024094508AM/04092024095117AMCoding-Principles-resize.png)](https://www.c-sharpcorner.com/ebooks/coding-principles)

[

Coding Principles

](https://www.c-sharpcorner.com/ebooks/coding-principles)

Read by 3.1k people

[Download Now!](https://www.c-sharpcorner.com/ebooks/coding-principles)

/\* date-time conversion js Start \*/ var options = { weekday: "long", year: "numeric", month: "long", day: "numeric" }; function GetDayName(e) { var n = \["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"\]; return n\[e\]; } function getTimezoneName() { const e = new Date(); const n = e.toLocaleDateString(); const t = e.toLocaleDateString(undefined, { timeZoneName: "long" }); const o = t.indexOf(n); if (o >= 0) { return (t.substring(0, o) + t.substring(o + n.length)).replace(/^\[\\s,.\\-:;\]+|\[\\s,.\\-:;\]+$/g, ""); } return t; } function convertUTCDateToLocalDate(date) { var localDate = new Date(date); localDate.setMinutes(date.getMinutes() - date.getTimezoneOffset()); return localDate; } /\* date-time conversion js Ends \*/ document.addEventListener("DOMContentLoaded", function () { // Set the local timezone name var tzInput = document.getElementById("HiddenLocalTimezone"); if (tzInput) { tzInput.value = getTimezoneName(); } var index = 1; while (true) { var hiddenInput = document.getElementById("HiddenUTCDateTimeList\_" + index); if (!hiddenInput) break; var utcDateStr = hiddenInput.value; var utcDate = new Date(utcDateStr); // Convert UTC to Local var localDate = convertUTCDateToLocalDate(utcDate); var evntDt = convertUTCDateToLocalDate(utcDate); var monthDay = localDate.toLocaleString('default', { month: 'short' }); var shortMonthName = monthDay.substring(0, 3) + " " + localDate.getDate(); var localTimeString = localDate.toLocaleString('en-US', { hour: 'numeric', minute: 'numeric', hour12: true }); var time = localTimeString.split(' '); var timeHM = time\[0\].split(':'); var timeMin = (timeHM\[1\] === "00") ? timeHM\[0\] : time\[0\]; var localDayTime = GetDayName(localDate.getDay()) + " " + timeMin + " " + time\[1\]; var monthDaySplit = shortMonthName.split(' '); var monthEl = document.getElementById("LocalMonth" + index); var dayEl = document.getElementById("LocalDay" + index); if (monthEl) monthEl.textContent = monthDaySplit\[0\].toUpperCase(); if (dayEl) dayEl.textContent = monthDaySplit\[1\]; // Optional: Hide past event /\* const currentdate = new Date(); if (evntDt <= currentdate) { var eventElement = document.getElementById(\`event-${index}\`); if (eventElement) { eventElement.style.display = "none"; } } \*/ index++; } });

 [![Generative AI for Leaders](https://www.c-sharpcorner.com/UploadFile/Ads/1.gif "Generative AI for Leaders")](https://viberondemand.com/generative-ai-for-leaders)document.addEventListener("DOMContentLoaded", function () { setTimeout(() => { const img = document.getElementById("imgAdslazyImageOnTop"); img.src = img.getAttribute("data-src"); img.style.display = "block"; }, 5000); });