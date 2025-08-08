# Singleton Design Pattern In C#

**Source:** https://www.c-sharpcorner.com/UploadFile/8911c4/singleton-design-pattern-in-C-Sharp/
**Date Captured:** 2025-07-28T16:06:56.699Z
**Domain:** www.c-sharpcorner.com
**Author:** Mahesh Alle
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
*   [Login](/userregistration/logincheck.aspx?returnurl=/uploadfile/8911c4/singleton-design-pattern-in-c-sharp/)

// Get references to the elements const hoverTarget = document.getElementById('ctl00\_mainleft\_userProfImg'); const hiddenDiv = document.getElementById('userLinks'); // Add event listeners for mouse enter and leave if (hoverTarget != null) { hoverTarget.addEventListener('mouseenter', () => { // Show the hidden div when mouse enters hiddenDiv.style.display = 'block'; }); } if (hoverTarget != null) { hoverTarget.addEventListener('mouseleave', () => { // Hide the hidden div when mouse leaves setTimeout(function () { hiddenDiv.style.display = 'none'; }, 500); }); } document.addEventListener("DOMContentLoaded", function () { var listItems = document.querySelectorAll(".my-account-menu > li"); listItems.forEach(function (item) { item.addEventListener("mouseover", function () { setTimeout(function () { // Add your hover action here item.classList.add('hovered'); }, 500); // 500 milliseconds delay }); item.addEventListener("mouseout", function () { setTimeout(function () { item.classList.remove('hovered'); }, 500); }); }); });

 [![Prompt Engineering Training](https://www.c-sharpcorner.com/UploadFile/Ads/7.png)](https://viberondemand.com/prompt-engineering)document.addEventListener("DOMContentLoaded", function () { setTimeout(() => { const img = document.getElementById("imgAdslazyImageOnTop1"); img.src = img.getAttribute("data-src"); img.style.display = "block"; }, 3000); });

 [![C#](https://www.c-sharpcorner.com/UploadFile/MinorCatImages/csharp-programming_110028775.png "C#")](https://www.c-sharpcorner.com/technologies/csharp-programming) 

# Singleton Design Pattern In C#

[](/featured-articles "Featured")

*   [](https://www.facebook.com/sharer.php?u=https://www.c-sharpcorner.com/UploadFile/8911c4/singleton-design-pattern-in-C-Sharp/ "Share on Facebook")
*   [](https://twitter.com/intent/tweet?&via=CsharpCorner&related=CsharpCorner&text=Singleton+Design+Pattern+In+C%23 https://www.c-sharpcorner.com/UploadFile/8911c4/singleton-design-pattern-in-C-Sharp/ "Share on Twitter")
*   [](https://www.linkedin.com/shareArticle?mini=true&url=https://www.c-sharpcorner.com/UploadFile/8911c4/singleton-design-pattern-in-C-Sharp/ "Share on Linkedin")
*   [](//www.reddit.com/submit?title=Singleton+Design+Pattern+In+C%23&url=https%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2f8911c4%2fsingleton-design-pattern-in-C-Sharp%2fdefault.aspx "Share on Reddit")
*   [WhatsApp](whatsapp://send?text=Singleton Design Pattern In C#%0A%0Ahttps%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2f8911c4%2fsingleton-design-pattern-in-C-Sharp%2f "Share on Whatsapp")
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/articles/emailtofriend.aspx?articleid=e63e94cb-9a26-4992-9cff-a0bc443a1b39 "Email this article to friend")
*   [](javascript:void\(0\); "Bookmark this article")
*   [](javascript:void\(GetPrintVersion\('Article'\)\) "Print")
*   [](https://www.c-sharpcorner.com/members/mahesh-alle/articles "Author's other article")

*    [![Author Profile Photo](https://www.c-sharpcorner.com/UploadFile/AuthorImage/8911c420210714080918.jpg.ashx?height=24&width=24 "Mahesh Alle")](https://www.c-sharpcorner.com/members/mahesh-alle)[Mahesh Alle](https://www.c-sharpcorner.com/members/mahesh-alle)
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/uploadfile/8911c4/singleton-design-pattern-in-c-sharp/history "Article history")2y
*   2.1m
*   19
*   48
    
[100](/members/rank.aspx?page=points-table "Points")*   [Article](/articles/)

Take the challenge 

[Singleton.zip](javascript:void\(0\); "Singleton.zip")

## What is Singleton Design Pattern?

Singleton design pattern in C# is one of the most popular design patterns. In this pattern, a class has only one instance in the program that provides a global point of access to it. In other words, a singleton is a class that allows only a single instance of itself to be created and usually gives simple access to that instance.

There are various ways to implement a singleton pattern in C#. The following are the common characteristics of a singleton pattern.

*   Private and parameterless single constructor
*   Sealed class.
*   Static variable to hold a reference to the single created instance
*   A public and static way of getting the reference to the created instance.

## Advantages of Singleton Design Pattern

The advantages of a Singleton Pattern are,

1.  Singleton pattern can implement interfaces.
2.  Can be lazy-loaded and has Static Initialization.
3.  It helps to hide dependencies.
4.  It provides a single point of access to a particular instance, so it is easy to maintain.

## Disadvantages of Singleton Design Pattern

The disadvantages of a Singleton Pattern are,

1.  Unit testing is a bit difficult as it introduces a global state into an application
2.  Reduces the potential for parallelism within a program by locking.

## Singleton class vs. Static methods

The following compares Singleton class vs. Static methods,

1.  A Static Class cannot be extended whereas a singleton class can be extended.
2.  A Static Class cannot be initialized whereas a singleton class can be.
3.  A Static class is loaded automatically by the CLR when the program containing the class is loaded.

## How to Implement Singleton Pattern in C# code

There are several ways to implement a Singleton Pattern in C#.

1.  No Thread Safe Singleton.
2.  Thread-Safety Singleton.
3.  Thread-Safety Singleton using Double-Check Locking.
4.  Thread-safe without a lock.
5.  Using .NET 4's Lazy<T> type.

## No Thread Safe Singleton

Explanation of the following code,

1.  The following code is not thread-safe.
2.  Two different threads could both have evaluated the test (if instance == null) and found it to be true, then both create instances, which violates the singleton pattern.  

```csharp
public sealed class Singleton1 {
    private Singleton1() {}
    private static Singleton1 instance = null;
    public static Singleton1 Instance {
        get {
            if (instance == null) {
                instance = new Singleton1();
            }
            return instance;
        }
    }
}
```

C#

Copy

## Thread Safety Singleton

Explanation of the following code,

1.  The following code is thread-safe.
2.  In the code, the thread is locked on a shared object and checks whether an instance has been created or not. It takes care of the memory barrier issue and ensures that only one thread will create an instance. For example: Since only one thread can be in that part of the code at a time, by the time the second thread enters it, the first thread will have created the instance, so the expression will evaluate as false.
3.  The biggest problem with this is performance; performance suffers since a lock is required every time an instance is requested.  

```csharp
public sealed class Singleton2 {
    Singleton2() {}
    private static readonly object lock = new object();
    private static Singleton2 instance = null;
    public static Singleton2 Instance {
        get {
            lock(lock) {
                if (instance == null) {
                    instance = new Singleton2();
                }
                return instance;
            }
        }
    }
}
```

C#

Copy

## Thread Safety Singleton using Double-Check Locking

Explanation of the following code,

In the following code, the thread is locked on a shared object and checks whether an instance has been created or not with double checking.

```csharp
public sealed class Singleton3 {
    Singleton3() {}
    private static readonly object lock = new object();
    private static Singleton3 instance = null;
    public static Singleton3 Instance {
        get {
            if (instance == null) {
                lock(lock) {
                    if (instance == null) {
                        instance = new Singleton3();
                    }
                }
            }
            return instance;
        }
    }
}
```

C#

Copy

## Thread Safe Singleton without using locks and no lazy instantiation

Explanation of the following code:

1.  The preceding implementation looks like a very simple code.
2.  This type of implementation has a static constructor, so it executes only once per Application Domain.
3.  It is not as lazy as the other implementation.   

```csharp
public sealed class Singleton4
{
    private static readonly Singleton4 instance = new Singleton4();
    static Singleton4()
    {
    }
    private Singleton4()
    {
    }
    public static Singleton4 Instance
    {
        get
        {
            return instance;
        }
    }
}
```

C#

Copy

## Using .NET 4's Lazy<T> type

Explanation of the following code:

1.  If you are using .NET 4 or higher then you can use the System.Lazy<T> type to make the laziness really simple.
2.  You can pass a delegate to the constructor that calls the Singleton constructor, which is done most easily with a lambda expression.
3.  Allows you to check whether or not the instance has been created with the IsValueCreated property.    

```csharp
public sealed class Singleton5
{
    private Singleton5()
    {
    }
    private static readonly Lazy<Singleton5> lazy = new Lazy<Singleton5>(() => new Singleton5());
    public static Singleton5 Instance
    {
        get
        {
            return lazy.Value;
        }
    }
}
```

C#

Copy

Hope you liked the article. Please let me know the feedback in the comments section.

People also reading

﻿ .mentions-input-box textarea { max-width: inherit; } .mentions { display: none; } label\[for="FollowCheckBox"\] { position: relative; } .mentions-autocomplete-list { background: #12192d; margin-top: 10px; } .mentions-autocomplete-list ul li { display: flex; flex-direction: row; align-items: center; padding: 10px; border-bottom: 1px solid #fff; cursor: pointer; } .mentions-autocomplete-list ul li:hover { background: var(--featuredFeedbg); } .mentions-autocomplete-list ul li img { max-width: 40px; max-height: 40px; border-radius: 50%; margin-right: 15px; } .previousComments { display: flex; justify-content: right; cursor: pointer; } .comments-body-container { width: 100%; } .content-bottom-comment { margin-top: -20px; } .commetn-txt-box { width: 97% !important; } .build-comment { display: flex; margin-top: 20px; } .commentheading { color: var(--headingColor); font-size: var(--FontSize18); font-family: var(--sitefont); margin-bottom: 7px; } .commetn-txt-box::placeholder { color: #7F8AAB; } /\*For Internet Explorer 10-11\*/ .commetn-txt-box:-ms-input-placeholder { color: #7F8AAB; } /\* For Microsoft Edge\*/ .commetn-txt-box::-ms-input-placeholder { color: #7F8AAB; } @media (max-width: 767px) { .esc-msg { display: none !important; } } /\*popup css start\*/ .overlay-bg-response { background-color: rgba(0, 0, 0, .5); width: 100%; height: 100%; position: fixed; top: 0; bottom: 0; left: 0; z-index: 9999; } .popup-window-response { margin: auto; position: relative; top: 65px; background-color: var(--popupBg); height: auto; max-height: 660px; /\* min-height: 227px;\*/ overflow: hidden; overflow-y: auto; max-width: 600px; } .popup-wrap-response { /\*min-height: 227px;\*/ padding: 0px 20px 0px 20px; } .popup-header-response { text-align: center; background-color: var(--popupBg); padding: 10px; line-height: 30px; font-size: 18px; color: var(--fontColor); font-weight: bold; position: sticky; top: 0px; } .popup-heading-response, .popup-heading-response > span { font-size: 20px; color: var(--fontColor); font-weight: 600; line-height: 2; } .icon-close { cursor: pointer; } .icon-close::before { transform: rotate(45deg); } .icon-close::before, .icon-close::after { content: ""; display: block; position: absolute; top: 28px; right: 30px; background-color: #ffffff; width: 20px; height: 3px; border-radius: 2px; } .icon-close::after { transform: rotate(-45deg); } .popup-body-response { overflow-y: auto; display: flex; justify-content: center; padding: 12px 24px; overflow-y: auto; /\* min-height: 100px;\*/ align-items: center; } .messagebody { font-size: 18px; color: var(--textColor); font-weight: 300; } .popup-footer-response { width: auto; text-align: center; padding: 12px 0; position: relative; } #PopUp-body { display: none; } .user-image-56{ min-width:44px; min-height:44px; } /\*popup css end\*/

*   (19) Comments

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

[![C# Corner Ebook](https://www.c-sharpcorner.com/UploadFile/EBooks/06252012015906AM/01302023054435AMProgramming-Strings-using-Csharp.png)](https://www.c-sharpcorner.com/ebooks/programming-strings-using-csharp)

[

Programming Strings using C#

](https://www.c-sharpcorner.com/ebooks/programming-strings-using-csharp)

Read by 23.5k people

[Download Now!](https://www.c-sharpcorner.com/ebooks/programming-strings-using-csharp)

/\* date-time conversion js Start \*/ var options = { weekday: "long", year: "numeric", month: "long", day: "numeric" }; function GetDayName(e) { var n = \["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"\]; return n\[e\]; } function getTimezoneName() { const e = new Date(); const n = e.toLocaleDateString(); const t = e.toLocaleDateString(undefined, { timeZoneName: "long" }); const o = t.indexOf(n); if (o >= 0) { return (t.substring(0, o) + t.substring(o + n.length)).replace(/^\[\\s,.\\-:;\]+|\[\\s,.\\-:;\]+$/g, ""); } return t; } function convertUTCDateToLocalDate(date) { var localDate = new Date(date); localDate.setMinutes(date.getMinutes() - date.getTimezoneOffset()); return localDate; } /\* date-time conversion js Ends \*/ document.addEventListener("DOMContentLoaded", function () { // Set the local timezone name var tzInput = document.getElementById("HiddenLocalTimezone"); if (tzInput) { tzInput.value = getTimezoneName(); } var index = 1; while (true) { var hiddenInput = document.getElementById("HiddenUTCDateTimeList\_" + index); if (!hiddenInput) break; var utcDateStr = hiddenInput.value; var utcDate = new Date(utcDateStr); // Convert UTC to Local var localDate = convertUTCDateToLocalDate(utcDate); var evntDt = convertUTCDateToLocalDate(utcDate); var monthDay = localDate.toLocaleString('default', { month: 'short' }); var shortMonthName = monthDay.substring(0, 3) + " " + localDate.getDate(); var localTimeString = localDate.toLocaleString('en-US', { hour: 'numeric', minute: 'numeric', hour12: true }); var time = localTimeString.split(' '); var timeHM = time\[0\].split(':'); var timeMin = (timeHM\[1\] === "00") ? timeHM\[0\] : time\[0\]; var localDayTime = GetDayName(localDate.getDay()) + " " + timeMin + " " + time\[1\]; var monthDaySplit = shortMonthName.split(' '); var monthEl = document.getElementById("LocalMonth" + index); var dayEl = document.getElementById("LocalDay" + index); if (monthEl) monthEl.textContent = monthDaySplit\[0\].toUpperCase(); if (dayEl) dayEl.textContent = monthDaySplit\[1\]; // Optional: Hide past event /\* const currentdate = new Date(); if (evntDt <= currentdate) { var eventElement = document.getElementById(\`event-${index}\`); if (eventElement) { eventElement.style.display = "none"; } } \*/ index++; } });

 [![Generative AI for Leaders](https://www.c-sharpcorner.com/UploadFile/Ads/1.gif "Generative AI for Leaders")](https://viberondemand.com/generative-ai-for-leaders)document.addEventListener("DOMContentLoaded", function () { setTimeout(() => { const img = document.getElementById("imgAdslazyImageOnTop"); img.src = img.getAttribute("data-src"); img.style.display = "block"; }, 5000); });