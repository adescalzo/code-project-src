# Design Pattern For Beginner: Part 5: Composite Design Pattern

**Source:** https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-5-composite-design-patter/
**Date Captured:** 2025-07-28T16:08:10.058Z
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
*   [Login](/userregistration/logincheck.aspx?returnurl=/uploadfile/dacca2/design-pattern-for-beginner-part-5-composite-design-patter/)

// Get references to the elements const hoverTarget = document.getElementById('ctl00\_mainleft\_userProfImg'); const hiddenDiv = document.getElementById('userLinks'); // Add event listeners for mouse enter and leave if (hoverTarget != null) { hoverTarget.addEventListener('mouseenter', () => { // Show the hidden div when mouse enters hiddenDiv.style.display = 'block'; }); } if (hoverTarget != null) { hoverTarget.addEventListener('mouseleave', () => { // Hide the hidden div when mouse leaves setTimeout(function () { hiddenDiv.style.display = 'none'; }, 500); }); } document.addEventListener("DOMContentLoaded", function () { var listItems = document.querySelectorAll(".my-account-menu > li"); listItems.forEach(function (item) { item.addEventListener("mouseover", function () { setTimeout(function () { // Add your hover action here item.classList.add('hovered'); }, 500); // 500 milliseconds delay }); item.addEventListener("mouseout", function () { setTimeout(function () { item.classList.remove('hovered'); }, 500); }); }); });

 [![Prompt Engineering Training](https://www.c-sharpcorner.com/UploadFile/Ads/7.png)](https://viberondemand.com/prompt-engineering)document.addEventListener("DOMContentLoaded", function () { setTimeout(() => { const img = document.getElementById("imgAdslazyImageOnTop1"); img.src = img.getAttribute("data-src"); img.style.display = "block"; }, 3000); });

 [![Design Patterns & Practices](https://www.c-sharpcorner.com/UploadFile/MinorCatImages/230207PM.png "Design Patterns & Practices")](https://www.c-sharpcorner.com/technologies/design-patterns-and-practices) 

# Design Pattern For Beginner: Part 5: Composite Design Pattern

[](/featured-articles "Featured")

*   [](https://www.facebook.com/sharer.php?u=https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-5-composite-design-patter/ "Share on Facebook")
*   [](https://twitter.com/intent/tweet?&via=CsharpCorner&related=CsharpCorner&text=Design+Pattern+For+Beginner%3a+Part+5%3a+Composite+Design+Pattern https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-5-composite-design-patter/ "Share on Twitter")
*   [](https://www.linkedin.com/shareArticle?mini=true&url=https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-5-composite-design-patter/ "Share on Linkedin")
*   [](//www.reddit.com/submit?title=Design+Pattern+For+Beginner%3a+Part+5%3a+Composite+Design+Pattern&url=https%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fdacca2%2fdesign-pattern-for-beginner-part-5-composite-design-patter%2fdefault.aspx "Share on Reddit")
*   [WhatsApp](whatsapp://send?text=Design Pattern For Beginner: Part 5: Composite Design Pattern%0A%0Ahttps%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fdacca2%2fdesign-pattern-for-beginner-part-5-composite-design-patter%2f "Share on Whatsapp")
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/articles/emailtofriend.aspx?articleid=4021d709-8cff-4e18-992e-0b5d082eee34 "Email this article to friend")
*   [](javascript:void\(0\); "Bookmark this article")
*   [](javascript:void\(GetPrintVersion\('Article'\)\) "Print")
*   [](https://www.c-sharpcorner.com/members/sourav-kayal/articles "Author's other article")

*    [![Author Profile Photo](https://www.c-sharpcorner.com/UploadFile/AuthorImage/dacca220151120112330.jpg.ashx?height=24&width=24 "Sourav Kayal")](https://www.c-sharpcorner.com/members/sourav-kayal)[Sourav Kayal](https://www.c-sharpcorner.com/members/sourav-kayal)
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/uploadfile/dacca2/design-pattern-for-beginner-part-5-composite-design-patter/history "Article history")6y
*   72.5k
*   0
*   3
    
[100](/members/rank.aspx?page=points-table "Points")*   [Article](/articles/)

Take the challenge 

Welcome to the Design Patterns for Beginners article aeries. In this article we will be learning how to implement a Composite Design Pattern in applications. From the past few articles I have been discussing various design patterns. If you are rely passionate about learning design pattern then please visit my previous articles:

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
    

In today's session we will learn about the Composite Design Pattern and we will discuss a few scenarios where the Composite Design Pattern may rescue you. At first we will discuss why the Composite Design Pattern is needed.  
  
**What the Composite Design Pattern Is  
**  
The Composite Design Pattern always forms a tree structure with the objects, where each root element contains sub nodes called child nodes and child nodes contain leaf nodes (last label node). Leaf nodes do not contain any elements. Now, in this object hierarchy we can point any node and from this node we can traverse all other nodes.  
  
For example, think about an employee stricture of an organization. We can easily represent it by a tree structure and if we start from the CEO (the root of the organization) then we can reach leaf employees by traversing through the middle-level managers.  
  
Take another example of a Composite Design Pattern. Think about the folder structure of your computer. Each drive contains many folders and each folder contains many subfolders and each subfolder contains many files. Here the files are nothing but the leaf nodes. A node from any drive partition you may traverse down to any file by a couple of mouse clicks.  
  
**Why composite pattern?**  
  
The Composite Design Pattern is useful when individual objects as well as a group of those kinds of objects are treated uniformly.  
  
Hmm.., bookish definition. Let's think of a scenario where we want to draw a few shapes in the same canvas. And we will draw all the components in the same way (by calling a respective function). We will try to implement it with a small working example. Have a look at the following code:

1.  using System;  
2.  using System.Collections;  
3.  using System.Globalization;  
4.  using System.Data.SqlClient;  
5.  using System.Data;  
6.  namespace Test1  
7.  {  
8.      interface IDraw  
9.      {  
10.          void Draw();  
11.      }  
12.      class Circle : IDraw  
13.      {  
14.          public void Draw()  
15.          {  
16.              Console.WriteLine("I am Circl");  
17.          }  
18.      }  
19.      class Square: IDraw  
20.      {  
21.          public void Draw()  
22.          {  
23.              Console.WriteLine("I am Square");  
24.          }  
25.      }  
26.      class Oval : IDraw  
27.      {  
28.          public void Draw()  
29.          {  
30.              Console.WriteLine("I am Oval");  
31.          }  
32.      }  
33.      class Program  
34.      {  
35.          static void Main(string\[\] args)  
36.          {  
37.              ArrayList objList = new ArrayList();  
38.              IDraw objcircl = new Circle();  
39.              IDraw objSquare = new Square();  
40.              IDraw objOval = new Oval();  
41.              objList.Add(objcircl);  
42.              objList.Add(objSquare);  
43.              objList.Add(objOval);  
44.              foreach (IDraw obj in objList)  
45.              {  
46.                  obj.Draw();  
47.              }  
48.              Console.ReadLine();  
49.          }  
50.      }  
51.  } 

Here, we have created a draw interface and three classes are deriving from them. Now, try to match this class hierarchy with the basic structure of the Composite Design Pattern. Here the IDraw interface is the root node and since three classes are derived from it (here implemented) and all of them are child nodes.

Each and every child class contains the same function definition and that's why we are able to call the Draw() function from each class within the for loop.  
  
Here is sample output of this example:  
  
  
  
In the next example we will implement one organization designation scenario in the Composite Design Pattern. We will assume that it is a small organization where employees are categories only in three levels.  
  
**CEO  
Head Manager  
Area Manager**  
  
Now, in this example we have implemented the following tree structure.  
  
  
Since this is the tree structure we can reach any leaf node from any non-leaf node. For example if we start from the root (CEO) node then by traversing the HM (Head Manager) nodes we can easily reach an Area Manager node.  
  
We will consider each node as a class in a sample example. Each class will also contain a common function called Designation(). In the following example we will start from a leaf node and will reach to the root node. Have a look at the following implementation.  

1.  using System;  
2.  using System.Collections;  
3.  using System.Globalization;  
4.  using System.Data.SqlClient;  
5.  using System.Data;  
6.  using System.Collections.Generic;  
7.  namespace Test1  
8.  {  
9.      interface IEmployee  
10.      {  
11.          void Designation();  
12.      }  
13.      class CEO :IEmployee  
14.      {  
15.          public virtual void Designation()  
16.          {  
17.              Console.WriteLine("I am sourav.CEO of Company");  
18.          }  
19.      }  
20.      class HeadManager\_1:CEO,IEmployee  
21.      {  
22.          public override void Designation()  
23.          {  
24.              Console.WriteLine("I am Rick. My Boss is sourav");  
25.          }  
26.      }  
27.      class HeadManager\_2 : CEO,IEmployee  
28.      {  
29.          public override void Designation()  
30.          {  
31.              Console.WriteLine("I am Martin. My Boss is sourav");  
32.          }  
33.      }  
34.      class AreaManager:HeadManager\_1,IEmployee  
35.      {  
36.          public new void Designation()  
37.          {  
38.              Console.WriteLine("I am Mack. My Boss is Rick");  
39.          }  
40.      }  
41.      class Program  
42.      {  
43.          static void Main(string\[\] args)  
44.          {  
45.              IEmployee AreaManager = new AreaManager();  
46.              IEmployee Hm\_1 = new HeadManager\_1();  
47.              IEmployee CEO = new CEO();  
48.              List<IEmployee> objEmployee = new List<IEmployee>();  
49.              objEmployee.Add(AreaManager);  
50.              objEmployee.Add(Hm\_1);  
51.              objEmployee.Add(CEO);  
52.              foreach (IEmployee O in objEmployee)  
53.              {  
54.                  O.Designation();  
55.              }  
56.              Console.ReadLine();  
57.          }  
58.      }  
59.  } 

Here is a sample example.

  
  
**Conclusion**  
  
In this article we have seen how to implement a Composite Design Pattern in C#. Hope you have enjoyed it.

People also reading

﻿ .mentions-input-box textarea { max-width: inherit; } .mentions { display: none; } label\[for="FollowCheckBox"\] { position: relative; } .mentions-autocomplete-list { background: #12192d; margin-top: 10px; } .mentions-autocomplete-list ul li { display: flex; flex-direction: row; align-items: center; padding: 10px; border-bottom: 1px solid #fff; cursor: pointer; } .mentions-autocomplete-list ul li:hover { background: var(--featuredFeedbg); } .mentions-autocomplete-list ul li img { max-width: 40px; max-height: 40px; border-radius: 50%; margin-right: 15px; } .previousComments { display: flex; justify-content: right; cursor: pointer; } .comments-body-container { width: 100%; } .content-bottom-comment { margin-top: -20px; } .commetn-txt-box { width: 97% !important; } .build-comment { display: flex; margin-top: 20px; } .commentheading { color: var(--headingColor); font-size: var(--FontSize18); font-family: var(--sitefont); margin-bottom: 7px; } .commetn-txt-box::placeholder { color: #7F8AAB; } /\*For Internet Explorer 10-11\*/ .commetn-txt-box:-ms-input-placeholder { color: #7F8AAB; } /\* For Microsoft Edge\*/ .commetn-txt-box::-ms-input-placeholder { color: #7F8AAB; } @media (max-width: 767px) { .esc-msg { display: none !important; } } /\*popup css start\*/ .overlay-bg-response { background-color: rgba(0, 0, 0, .5); width: 100%; height: 100%; position: fixed; top: 0; bottom: 0; left: 0; z-index: 9999; } .popup-window-response { margin: auto; position: relative; top: 65px; background-color: var(--popupBg); height: auto; max-height: 660px; /\* min-height: 227px;\*/ overflow: hidden; overflow-y: auto; max-width: 600px; } .popup-wrap-response { /\*min-height: 227px;\*/ padding: 0px 20px 0px 20px; } .popup-header-response { text-align: center; background-color: var(--popupBg); padding: 10px; line-height: 30px; font-size: 18px; color: var(--fontColor); font-weight: bold; position: sticky; top: 0px; } .popup-heading-response, .popup-heading-response > span { font-size: 20px; color: var(--fontColor); font-weight: 600; line-height: 2; } .icon-close { cursor: pointer; } .icon-close::before { transform: rotate(45deg); } .icon-close::before, .icon-close::after { content: ""; display: block; position: absolute; top: 28px; right: 30px; background-color: #ffffff; width: 20px; height: 3px; border-radius: 2px; } .icon-close::after { transform: rotate(-45deg); } .popup-body-response { overflow-y: auto; display: flex; justify-content: center; padding: 12px 24px; overflow-y: auto; /\* min-height: 100px;\*/ align-items: center; } .messagebody { font-size: 18px; color: var(--textColor); font-weight: 300; } .popup-footer-response { width: auto; text-align: center; padding: 12px 0; position: relative; } #PopUp-body { display: none; } .user-image-56{ min-width:44px; min-height:44px; } /\*popup css end\*/

*   (0) Comments

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