# Design Pattern For Beginners - Part-2: Factory Design Pattern

**Source:** https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-2-factory-design-pattern/
**Date Captured:** 2025-07-28T16:08:52.297Z
**Domain:** www.c-sharpcorner.com
**Author:** Sourav Kayal
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
*   [Login](/userregistration/logincheck.aspx?returnurl=/uploadfile/dacca2/design-pattern-for-beginner-part-2-factory-design-pattern/)

// Get references to the elements const hoverTarget = document.getElementById('ctl00\_mainleft\_userProfImg'); const hiddenDiv = document.getElementById('userLinks'); // Add event listeners for mouse enter and leave if (hoverTarget != null) { hoverTarget.addEventListener('mouseenter', () => { // Show the hidden div when mouse enters hiddenDiv.style.display = 'block'; }); } if (hoverTarget != null) { hoverTarget.addEventListener('mouseleave', () => { // Hide the hidden div when mouse leaves setTimeout(function () { hiddenDiv.style.display = 'none'; }, 500); }); } document.addEventListener("DOMContentLoaded", function () { var listItems = document.querySelectorAll(".my-account-menu > li"); listItems.forEach(function (item) { item.addEventListener("mouseover", function () { setTimeout(function () { // Add your hover action here item.classList.add('hovered'); }, 500); // 500 milliseconds delay }); item.addEventListener("mouseout", function () { setTimeout(function () { item.classList.remove('hovered'); }, 500); }); }); });

 [![Prompt Engineering Training](https://www.c-sharpcorner.com/UploadFile/Ads/7.png)](https://viberondemand.com/prompt-engineering)document.addEventListener("DOMContentLoaded", function () { setTimeout(() => { const img = document.getElementById("imgAdslazyImageOnTop1"); img.src = img.getAttribute("data-src"); img.style.display = "block"; }, 3000); });

 [![Design Patterns & Practices](https://www.c-sharpcorner.com/UploadFile/MinorCatImages/230207PM.png "Design Patterns & Practices")](https://www.c-sharpcorner.com/technologies/design-patterns-and-practices) 

# Design Pattern For Beginners - Part-2: Factory Design Pattern

[](/featured-articles "Featured")

*   [](https://www.facebook.com/sharer.php?u=https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-2-factory-design-pattern/ "Share on Facebook")
*   [](https://twitter.com/intent/tweet?&via=CsharpCorner&related=CsharpCorner&text=Design+Pattern+For+Beginners+-+Part-2%3a+Factory+Design+Pattern https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-2-factory-design-pattern/ "Share on Twitter")
*   [](https://www.linkedin.com/shareArticle?mini=true&url=https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-2-factory-design-pattern/ "Share on Linkedin")
*   [](//www.reddit.com/submit?title=Design+Pattern+For+Beginners+-+Part-2%3a+Factory+Design+Pattern&url=https%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fdacca2%2fdesign-pattern-for-beginner-part-2-factory-design-pattern%2fdefault.aspx "Share on Reddit")
*   [WhatsApp](whatsapp://send?text=Design Pattern For Beginners - Part-2: Factory Design Pattern%0A%0Ahttps%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fdacca2%2fdesign-pattern-for-beginner-part-2-factory-design-pattern%2f "Share on Whatsapp")
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/articles/emailtofriend.aspx?articleid=1456a48a-0d05-4370-940a-3e8021f6087f "Email this article to friend")
*   [](javascript:void\(0\); "Bookmark this article")
*   [](javascript:void\(GetPrintVersion\('Article'\)\) "Print")
*   [](https://www.c-sharpcorner.com/members/sourav-kayal/articles "Author's other article")

*    [![Author Profile Photo](https://www.c-sharpcorner.com/UploadFile/AuthorImage/dacca220151120112330.jpg.ashx?height=24&width=24 "Sourav Kayal")](https://www.c-sharpcorner.com/members/sourav-kayal)[Sourav Kayal](https://www.c-sharpcorner.com/members/sourav-kayal)
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/uploadfile/dacca2/design-pattern-for-beginner-part-2-factory-design-pattern/history "Article history")2y
*   56.3k
*   4
*   5
    
[100](/members/rank.aspx?page=points-table "Points")*   [Article](/articles/)

Take the challenge 

## Introduction

Before reading this article, please go through the following article.

*   [Design Pattern For Beginners - Part 1: Singleton Design Pattern](https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-1-singleton-design-patt/)
*   [Design Pattern For Beginners - Part 2: Factory Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-2-factory-design-pattern/)
*   [Design Pattern For Beginners - Part 3: Prototype Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-3-prototype-design-patter/)
*   [Design Pattern For Beginners - Part 4: Decorator Design Pattern](/UploadFile/dacca2/design-pattern-for-beginners-part-4-decorator-design-patt/)
*   [Design Pattern For Beginners - Part 5: Composite Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-5-composite-design-patter/)
*   [Design Pattern For Beginners - Part 6: Adaptor Design Pattern](/UploadFile/dacca2/design-pattern-for-beginners-part-6-adaptor-design-patter/)
*   [Design Pattern For Beginners - Part 7: Bridge Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-bridge-design-pattern/)
*   [Design Pattern For Beginners - Part 8: memento Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-8-memento-design-patter/)
*   [Design Pattern for Beginners - Part-9: Strategy Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-9-strategy-design-pattern/)
*   [Design Pattern for Beginners - Part-10: Observer Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-10-observer-design-patter/)
*   [Design Pattern For Beginners - Part 11: Implement Decouple Classes in Application](https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-11-implement-decouple-cl/)

Before writing this article, I searched using Google for the keywords "factory design pattern," As expected, I found many good articles with excellent examples. Then I started to think, "Why to write one more?" OK, then I thought, let's check each of them individually. I visited each of the top 10 (from the first page). They are tremendous but stuffy enough; all the articles maintain a typical style and are the same type. Somehow reality is absent.

And that led me to write one more on the same topic. Here we will try to understand that topic with realistic examples and a little fun. And we will enjoy the learning.

So, let's start with the Factory Design Pattern. As promised, "We will try to understand the basic need first before starting any design pattern.".

Let me start with a small practical event that I experienced just a few days before (yes, just a few days). My present office is within one Tech Park and I see many ITians (yes, you are right, they work in various IT fields in many companies within the same tech park) going towards the food court at lunchtime. Of course, I am one of them. There is a long staircase in the middle of the food court (since the food court is underground).

One fine lunchtime, I was going for lunch as usual and heard a beautiful romantic song of love from someone's voice. I observed that a disabled fellow was singing and going just before me using his crutch (you know, walking stick). How happy is he? (Dear reader, I am not making fun of his physical disability. I mean to say, see how happy he is. However, sometimes we may not be a complete person.) I was thinking of being a physical challenger.

OK, now the staircase has come, and it's time to climb. I noticed that the guy (singing a song of love) just folded up his walking stick and, by holding the stair railing, began to climb down. I was thinking about how nice the stick is. Depending on demand, it's behaving.

My story ends here, and the walking stick is just one example of a factory class. A factory class serves the client's demands, and depending on the requirements, it supplies the proper form depending on the requirements.

OK, we have talked too much; let's learn the simple structure of the Factory Design Pattern. We will initially see the logical stricture of the Factory Design Pattern.

As the name implies, a factory is where manufacturing happens—for example, a car factory where many types of car manufacturing happen. Now you want a specific model, and it's always possible to produce it from this car factory. Again your friend's taste is different from yours. He wants a different model; you can request a car factory again to make your friend smile.

OK, one thing is clear from our discussion; the factory class is a supplier class that makes the client happy by supplying their demand.

How to will you implement it in C# code? OK, let's see the following example.

```sql
using System;
using System.Collections;
using System.Data.SqlClient;
using System.Threading;

namespace Test1
{
    public interface ISupplier
    {
        void CarSupplier();
    }
    class namo : ISupplier
    {
        public void CarSupplier()
        {
            Console.WriteLine("I am nano supplier");
        }
    }
    class alto : ISupplier
    {
        public void CarSupplier()
        {
            Console.WriteLine("I am Alto supplier");
        }
    }
    class CarFactory
    {
        public static ISupplier GiveMyCar(int Key)
        {
            if (Key == 0)
                return new namo();
            else if (Key == 1)
                return new alto();
            else
                return null;
        }
    }
    class Program
    {
        static void Main(string[] args)
        {
            ISupplier obj = CarFactory.GiveMyCar(0);
            obj.CarSupplier();
            obj = CarFactory.GiveMyCar(1);
            obj.CarSupplier();
            Console.ReadLine();
        }
    }
}
```

SQL

Copy

And here is the output.

Though the code is pretty simple to understand, we will discuss it further. At first, we created one interface and implemented that interface within two classes. The classes are nano and alto.

In addition to them, there is one more class called CarFactory, and it is the story's hero. If we observe closely inside the CarFactory class, we will find the mechanism to create a car. Though here we are producing two low-end cars (nano and alto), we can soon add many more models. Now here is the original beauty of the factory class.

Try to understand the following few lines carefully. As we indicated, we can add many more models in the future. Suppose we add seven or even 10 (or your preferred number) more models. In that case, the client code will also not be affected by a single line because we will add code in the factory class, not in the client, and will inform the client that, from now on, those models are also available; send the proper code (such as 0 for nano, 1 for alto) to get them.

Now return to my story of a walking stick. The walking stick was changed in behavior depending on needs. And just now, we have seen our factory class supplying a different object form depending on conditions. Somehow both share an expected behavior, right?

OK, now you may think, what a useless example this is. Car class? We don't know when our company will get a project from a car merchant, and we will implement the CarFactory class there.

OK, then let me show you a very realistic example that you can implement in your current project; yes, tomorrow morning.

Here we will learn how to implement a vendor-independent data access mechanism using a factory class. No, we will not create our factory class to do that; we will use a Dbfactory class from the .NET library.

Let's clarify our purpose again: "We will implement such a data access mechanism that is able to talk with various database vendors.".

Today you have developed one software product by targeting one of your clients who use SQLServer as their database. You have designed and implemented the necessary coding for SQLServer. At first, we will learn why we need to know. (Yes ADO.NET code, sqlconnection, sqlcommand bla bla..)

Now tomorrow, the client may say that we are not happy with SQLServer, and we have decided that from now we will use Oracle as our backend database.

The drama starts here. Let me explain the first scene.

By getting this proposal in the mail (from the client), the project manager will call the team leader of this product's team. After an hour of discussion, they will decide, "They need to change the data manipulation policy." It will take 30 days more to fix by five resources, and the budget for that is $$.

Now the client has received mail from the Software Company and replied: "Why do you people disclose the matter at the very first, and we are unable to give a single $ and day to do so., And we want to get it done within this time limit.".

What "If we develop such a data-accessing mechanism that will be compatible with all database vendors"? OOHhh, it's getting very complex; we will not go further. Have a look at the following code.

```sql
using System;
using System.Collections;
using System.Data.SqlClient;
using System.Threading;
using System.Data.Common;
using System.Data;

namespace Test1
{
   class Program
    {
        static void Main(string[] args)
        {
            DbProviderFactory provider = null;
            DbConnection con = null;
            DbCommand cmd = null;
            DbDataReader rdr = null;
            DataTable dt = new DataTable();
            provider =DbProviderFactories.GetFactory("System.Data.SqlClient");
            con = provider.CreateConnection();   //Create Connection according to Connection Class
            con.ConnectionString = "Data Source=SOURAV-PC\\SQL_INSTANCE;Initial Catalog=test;Integrated Security=True";
            cmd = provider.CreateCommand();   //Create command according to Provider
            try
            {
                cmd.CommandText = "select * from name";
                cmd.CommandType = CommandType.Text;
                if (con.State == ConnectionState.Closed || con.State == ConnectionState.Broken)
                {
                    con.Open();
                    cmd.Connection = con;
                    using (con)
                    {
                        rdr = cmd.ExecuteReader(CommandBehavior.CloseConnection);
                        while (rdr.Read())
                        {
                            Console.WriteLine(rdr["nametest"].ToString());
                            Console.WriteLine(rdr["surname"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                //trn.Rollback();
                con.Dispose();
                cmd.Dispose();
            }
            Console.ReadLine();
        }
    }
}
```

SQL

Copy

Here is a sample output.

You can see that nowhere in the example have we written any database-specific ADO.NET code. We have used various methods of the Dbfactory class to create an object depending on Supplier. Have a look at the following code.

```sql
con = provider.CreateConnection();   //Create Connection according to database provide
cmd = provider.CreateCommand();   //Create command according to database provider
```

SQL

Copy

Here the provider is nothing but an object of the DbProviderFactory class, and we are using a function like Createconnection() and CreateCommand() to initialize a connection and command object.

Now, if you want to change the database, then change the provider name or database supplier name like:

```sql
provider =DbProviderFactories.GetFactory("System.Data.SqlClient");
```

SQL

Copy

Here we have provided our database provider as SQLServer, and tomorrow, if you want to, you can use MySQL; modify the code as in the following:

```sql
provider =DbProviderFactories.GetFactory("MySql.Data.SqlClient");
```

SQL

Copy

We need to change the provider name; the rest of the code will work fine. And see again, the DbProviderFactory class supplies an object depending on the user's demands, and this is the beauty of a factory class.

## Conclusion

This is my best attempt to explain a Factory Design Pattern with an example and in my own words. I hope you enjoy it. 

**Continue reading** 

*   [Design Pattern For Beginners - Part 1: Singleton Design Pattern](https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-1-singleton-design-patt/)
*   [Design Pattern For Beginners - Part 2: Factory Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-2-factory-design-pattern/)
*   [Design Pattern For Beginners - Part 3: Prototype Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-3-prototype-design-patter/)
*   [Design Pattern For Beginners - Part 4: Decorator Design Pattern](/UploadFile/dacca2/design-pattern-for-beginners-part-4-decorator-design-patt/)
*   [Design Pattern For Beginners - Part 5: Composite Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-5-composite-design-patter/)
*   [Design Pattern For Beginners - Part 6: Adaptor Design Pattern](/UploadFile/dacca2/design-pattern-for-beginners-part-6-adaptor-design-patter/)
*   [Design Pattern For Beginners - Part 7: Bridge Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-bridge-design-pattern/)
*   [Design Pattern For Beginners - Part 8: memento Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-8-memento-design-patter/)
*   [Design Pattern for Beginners - Part-9: Strategy Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-9-strategy-design-pattern/)
*   [Design Pattern for Beginners - Part-10: Observer Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-10-observer-design-patter/)
*   [Design Pattern For Beginners - Part 11: Implement Decouple Classes in Application](https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-11-implement-decouple-cl/)

People also reading

﻿ .mentions-input-box textarea { max-width: inherit; } .mentions { display: none; } label\[for="FollowCheckBox"\] { position: relative; } .mentions-autocomplete-list { background: #12192d; margin-top: 10px; } .mentions-autocomplete-list ul li { display: flex; flex-direction: row; align-items: center; padding: 10px; border-bottom: 1px solid #fff; cursor: pointer; } .mentions-autocomplete-list ul li:hover { background: var(--featuredFeedbg); } .mentions-autocomplete-list ul li img { max-width: 40px; max-height: 40px; border-radius: 50%; margin-right: 15px; } .previousComments { display: flex; justify-content: right; cursor: pointer; } .comments-body-container { width: 100%; } .content-bottom-comment { margin-top: -20px; } .commetn-txt-box { width: 97% !important; } .build-comment { display: flex; margin-top: 20px; } .commentheading { color: var(--headingColor); font-size: var(--FontSize18); font-family: var(--sitefont); margin-bottom: 7px; } .commetn-txt-box::placeholder { color: #7F8AAB; } /\*For Internet Explorer 10-11\*/ .commetn-txt-box:-ms-input-placeholder { color: #7F8AAB; } /\* For Microsoft Edge\*/ .commetn-txt-box::-ms-input-placeholder { color: #7F8AAB; } @media (max-width: 767px) { .esc-msg { display: none !important; } } /\*popup css start\*/ .overlay-bg-response { background-color: rgba(0, 0, 0, .5); width: 100%; height: 100%; position: fixed; top: 0; bottom: 0; left: 0; z-index: 9999; } .popup-window-response { margin: auto; position: relative; top: 65px; background-color: var(--popupBg); height: auto; max-height: 660px; /\* min-height: 227px;\*/ overflow: hidden; overflow-y: auto; max-width: 600px; } .popup-wrap-response { /\*min-height: 227px;\*/ padding: 0px 20px 0px 20px; } .popup-header-response { text-align: center; background-color: var(--popupBg); padding: 10px; line-height: 30px; font-size: 18px; color: var(--fontColor); font-weight: bold; position: sticky; top: 0px; } .popup-heading-response, .popup-heading-response > span { font-size: 20px; color: var(--fontColor); font-weight: 600; line-height: 2; } .icon-close { cursor: pointer; } .icon-close::before { transform: rotate(45deg); } .icon-close::before, .icon-close::after { content: ""; display: block; position: absolute; top: 28px; right: 30px; background-color: #ffffff; width: 20px; height: 3px; border-radius: 2px; } .icon-close::after { transform: rotate(-45deg); } .popup-body-response { overflow-y: auto; display: flex; justify-content: center; padding: 12px 24px; overflow-y: auto; /\* min-height: 100px;\*/ align-items: center; } .messagebody { font-size: 18px; color: var(--textColor); font-weight: 300; } .popup-footer-response { width: auto; text-align: center; padding: 12px 0; position: relative; } #PopUp-body { display: none; } .user-image-56{ min-width:44px; min-height:44px; } /\*popup css end\*/

*   (4) Comments

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

[![C# Corner Ebook](https://www.c-sharpcorner.com/UploadFile/EBooks/10282022072733AM/10282022090131AMArchitecting-Modern-Applications-using-Monolithic-architecture-In-Asp.Net-Core.jpg)](https://www.c-sharpcorner.com/ebooks/architecting-modern-applications-using-monolithic-architecture-in-asp-net-core-w)

[

Architecting Modern Applications using Monolithic Architecture in Asp.Net Core Web API

](https://www.c-sharpcorner.com/ebooks/architecting-modern-applications-using-monolithic-architecture-in-asp-net-core-w)

Read by 6.3k people

[Download Now!](https://www.c-sharpcorner.com/ebooks/architecting-modern-applications-using-monolithic-architecture-in-asp-net-core-w)

/\* date-time conversion js Start \*/ var options = { weekday: "long", year: "numeric", month: "long", day: "numeric" }; function GetDayName(e) { var n = \["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"\]; return n\[e\]; } function getTimezoneName() { const e = new Date(); const n = e.toLocaleDateString(); const t = e.toLocaleDateString(undefined, { timeZoneName: "long" }); const o = t.indexOf(n); if (o >= 0) { return (t.substring(0, o) + t.substring(o + n.length)).replace(/^\[\\s,.\\-:;\]+|\[\\s,.\\-:;\]+$/g, ""); } return t; } function convertUTCDateToLocalDate(date) { var localDate = new Date(date); localDate.setMinutes(date.getMinutes() - date.getTimezoneOffset()); return localDate; } /\* date-time conversion js Ends \*/ document.addEventListener("DOMContentLoaded", function () { // Set the local timezone name var tzInput = document.getElementById("HiddenLocalTimezone"); if (tzInput) { tzInput.value = getTimezoneName(); } var index = 1; while (true) { var hiddenInput = document.getElementById("HiddenUTCDateTimeList\_" + index); if (!hiddenInput) break; var utcDateStr = hiddenInput.value; var utcDate = new Date(utcDateStr); // Convert UTC to Local var localDate = convertUTCDateToLocalDate(utcDate); var evntDt = convertUTCDateToLocalDate(utcDate); var monthDay = localDate.toLocaleString('default', { month: 'short' }); var shortMonthName = monthDay.substring(0, 3) + " " + localDate.getDate(); var localTimeString = localDate.toLocaleString('en-US', { hour: 'numeric', minute: 'numeric', hour12: true }); var time = localTimeString.split(' '); var timeHM = time\[0\].split(':'); var timeMin = (timeHM\[1\] === "00") ? timeHM\[0\] : time\[0\]; var localDayTime = GetDayName(localDate.getDay()) + " " + timeMin + " " + time\[1\]; var monthDaySplit = shortMonthName.split(' '); var monthEl = document.getElementById("LocalMonth" + index); var dayEl = document.getElementById("LocalDay" + index); if (monthEl) monthEl.textContent = monthDaySplit\[0\].toUpperCase(); if (dayEl) dayEl.textContent = monthDaySplit\[1\]; // Optional: Hide past event /\* const currentdate = new Date(); if (evntDt <= currentdate) { var eventElement = document.getElementById(\`event-${index}\`); if (eventElement) { eventElement.style.display = "none"; } } \*/ index++; } });

 [![Generative AI for Leaders](https://www.c-sharpcorner.com/UploadFile/Ads/1.gif "Generative AI for Leaders")](https://viberondemand.com/generative-ai-for-leaders)document.addEventListener("DOMContentLoaded", function () { setTimeout(() => { const img = document.getElementById("imgAdslazyImageOnTop"); img.src = img.getAttribute("data-src"); img.style.display = "block"; }, 5000); });