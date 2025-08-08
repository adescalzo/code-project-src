# Design Pattern For Beginners - Part 6: Adaptor Design Pattern

**Source:** https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginners-part-6-adaptor-design-patter/
**Date Captured:** 2025-07-28T16:08:15.860Z
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
*   [Login](/userregistration/logincheck.aspx?returnurl=/uploadfile/dacca2/design-pattern-for-beginners-part-6-adaptor-design-patter/)

// Get references to the elements const hoverTarget = document.getElementById('ctl00\_mainleft\_userProfImg'); const hiddenDiv = document.getElementById('userLinks'); // Add event listeners for mouse enter and leave if (hoverTarget != null) { hoverTarget.addEventListener('mouseenter', () => { // Show the hidden div when mouse enters hiddenDiv.style.display = 'block'; }); } if (hoverTarget != null) { hoverTarget.addEventListener('mouseleave', () => { // Hide the hidden div when mouse leaves setTimeout(function () { hiddenDiv.style.display = 'none'; }, 500); }); } document.addEventListener("DOMContentLoaded", function () { var listItems = document.querySelectorAll(".my-account-menu > li"); listItems.forEach(function (item) { item.addEventListener("mouseover", function () { setTimeout(function () { // Add your hover action here item.classList.add('hovered'); }, 500); // 500 milliseconds delay }); item.addEventListener("mouseout", function () { setTimeout(function () { item.classList.remove('hovered'); }, 500); }); }); });

 [![Prompt Engineering Training](https://www.c-sharpcorner.com/UploadFile/Ads/7.png)](https://viberondemand.com/prompt-engineering)document.addEventListener("DOMContentLoaded", function () { setTimeout(() => { const img = document.getElementById("imgAdslazyImageOnTop1"); img.src = img.getAttribute("data-src"); img.style.display = "block"; }, 3000); });

 [![Design Patterns & Practices](https://www.c-sharpcorner.com/UploadFile/MinorCatImages/230207PM.png "Design Patterns & Practices")](https://www.c-sharpcorner.com/technologies/design-patterns-and-practices) 

# Design Pattern For Beginners - Part 6: Adaptor Design Pattern

[](/featured-articles "Featured")

*   [](https://www.facebook.com/sharer.php?u=https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginners-part-6-adaptor-design-patter/ "Share on Facebook")
*   [](https://twitter.com/intent/tweet?&via=CsharpCorner&related=CsharpCorner&text=Design+Pattern+For+Beginners+-+Part+6%3a+Adaptor+Design+Pattern https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginners-part-6-adaptor-design-patter/ "Share on Twitter")
*   [](https://www.linkedin.com/shareArticle?mini=true&url=https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginners-part-6-adaptor-design-patter/ "Share on Linkedin")
*   [](//www.reddit.com/submit?title=Design+Pattern+For+Beginners+-+Part+6%3a+Adaptor+Design+Pattern&url=https%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fdacca2%2fdesign-pattern-for-beginners-part-6-adaptor-design-patter%2fdefault.aspx "Share on Reddit")
*   [WhatsApp](whatsapp://send?text=Design Pattern For Beginners - Part 6: Adaptor Design Pattern%0A%0Ahttps%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fdacca2%2fdesign-pattern-for-beginners-part-6-adaptor-design-patter%2f "Share on Whatsapp")
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/articles/emailtofriend.aspx?articleid=da5dee5c-d518-4113-8095-2ec92dfd964e "Email this article to friend")
*   [](javascript:void\(0\); "Bookmark this article")
*   [](javascript:void\(GetPrintVersion\('Article'\)\) "Print")
*   [](https://www.c-sharpcorner.com/members/sourav-kayal/articles "Author's other article")

*    [![Author Profile Photo](https://www.c-sharpcorner.com/UploadFile/AuthorImage/dacca220151120112330.jpg.ashx?height=24&width=24 "Sourav Kayal")](https://www.c-sharpcorner.com/members/sourav-kayal)[Sourav Kayal](https://www.c-sharpcorner.com/members/sourav-kayal)
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/uploadfile/dacca2/design-pattern-for-beginners-part-6-adaptor-design-patter/history "Article history")6y
*   74.2k
*   2
*   3
    
[100](/members/rank.aspx?page=points-table "Points")*   [Article](/articles/)

Take the challenge 

Welcome to the Design Pattern for Beginners article series. I have been writing about design patterns and implementations in a few articles. If you are also passionate to learn design patterns like me, please feel free to visit my previous articles here:

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
    

In today's article we will discuss a very important design pattern called "Adaptor Design Pattern". As the name suggests, it performs operations very similar to adaptors (Hum; laptop adaptor). Before starting the explanation and example code we will try to understand the real scenario where the Adaptor Design Pattern is relevant.  
  
**Why adaptor pattern?**  
  
A few years ago I bought a mobile phone (at that time it was a little costly and a decent one with very few features) and had been using it for a couple of years. One day I discovered that the charger for the phone was not working, I went to the mobile shop with both charger and phone. The shop checked both and told me "The charger is gone". Alas! Again I need to buy a new one. The next statement of shopkeeper made me more sorrowful, "This type of charger is out of the market. Since the model is very old, the company has stopped manufacturing it". But he continued, the solution is, you need to buy a multi-pin charger, then you can charge your mobile with that.  
  
  
  
I know, while you were reading the paragraph above, you were thinking. Hmm, Sourav is telling a story. Yes my dear reader, this is a story and using it we are attempting to understand the basic need for a Adaptor Design Pattern.  
  
Basically the Adaptor Design Pattern is relevant when two different classes talk with each other. Sometimes a situation may occur like that, you cannot change anything in an existing class and at the same time another class wants to talk with the existing class. In that situation we can implement a middle-level class called an adaptor class and by using the adaptor class both classes are able to talk with each other.  
**  
How to implement?**  
  
When we use the term adaptor, at first laptop adaptors comes to mind. Right? OK, let's implement an adaptor pattern concept with a few laptop classes.  
  
In the following example we have implemented an ILaptop interface in a few laptop classes. All classes have one common function called ShowModel(). If we call the ShowModel() function then it will show the laptop brand.

1.  using System;  
2.  using System.Collections;   
3.  namespace AdpatorDesign  
4.  {  
5.      interface ILaptop  
6.      {  
7.          void ShowModel();  
8.      }  
9.      class HP\_Laptop:ILaptop  
10.      {  
11.          public void ShowModel()  
12.          {  
13.              Console.WriteLine("I am HP Laptiop");  
14.          }  
15.      }  
16.      class Sony\_Laptop : ILaptop  
17.      {  
18.          public void ShowModel()  
19.          {  
20.              Console.WriteLine("I am Sony Laptop");  
21.          }  
22.      }  
23.      class Compaq\_Laptop : ILaptop  
24.      {  
25.          public void ShowModel()  
26.          {  
27.              Console.WriteLine("I am Compaq Laptop");  
28.          }  
29.      }   
30.      class LaptopAdaptor :ILaptop  
31.      {  
32.          public void ShowModel(){}  
33.          public static void ShowModel(ILaptop obj)  
34.          {  
35.              obj.ShowModel();  
36.          }  
37.      }   
38.      class Person  
39.      {  
40.          public void SwitchOn(ILaptop obj)  
41.          {  
42.              LaptopAdaptor.ShowModel(obj);    
43.          }  
44.      }   
45.      class Program  
46.      {  
47.          static void Main(string\[\] args)  
48.          {  
49.              Person p = new Person();  
50.              p.SwitchOn(new HP\_Laptop()); //On HP Laptop  
51.              p.SwitchOn(new Compaq\_Laptop()); //On Compaq laptop   
52.              Console.ReadLine();  
53.          }  
54.      }  
55.  } 

The Person class works with various types of laptops. And the SwitchOn() function will switch on the proper type of laptop through the LaptopAdaptor class. Here is sample output.  
  
  

People also reading

﻿ .mentions-input-box textarea { max-width: inherit; } .mentions { display: none; } label\[for="FollowCheckBox"\] { position: relative; } .mentions-autocomplete-list { background: #12192d; margin-top: 10px; } .mentions-autocomplete-list ul li { display: flex; flex-direction: row; align-items: center; padding: 10px; border-bottom: 1px solid #fff; cursor: pointer; } .mentions-autocomplete-list ul li:hover { background: var(--featuredFeedbg); } .mentions-autocomplete-list ul li img { max-width: 40px; max-height: 40px; border-radius: 50%; margin-right: 15px; } .previousComments { display: flex; justify-content: right; cursor: pointer; } .comments-body-container { width: 100%; } .content-bottom-comment { margin-top: -20px; } .commetn-txt-box { width: 97% !important; } .build-comment { display: flex; margin-top: 20px; } .commentheading { color: var(--headingColor); font-size: var(--FontSize18); font-family: var(--sitefont); margin-bottom: 7px; } .commetn-txt-box::placeholder { color: #7F8AAB; } /\*For Internet Explorer 10-11\*/ .commetn-txt-box:-ms-input-placeholder { color: #7F8AAB; } /\* For Microsoft Edge\*/ .commetn-txt-box::-ms-input-placeholder { color: #7F8AAB; } @media (max-width: 767px) { .esc-msg { display: none !important; } } /\*popup css start\*/ .overlay-bg-response { background-color: rgba(0, 0, 0, .5); width: 100%; height: 100%; position: fixed; top: 0; bottom: 0; left: 0; z-index: 9999; } .popup-window-response { margin: auto; position: relative; top: 65px; background-color: var(--popupBg); height: auto; max-height: 660px; /\* min-height: 227px;\*/ overflow: hidden; overflow-y: auto; max-width: 600px; } .popup-wrap-response { /\*min-height: 227px;\*/ padding: 0px 20px 0px 20px; } .popup-header-response { text-align: center; background-color: var(--popupBg); padding: 10px; line-height: 30px; font-size: 18px; color: var(--fontColor); font-weight: bold; position: sticky; top: 0px; } .popup-heading-response, .popup-heading-response > span { font-size: 20px; color: var(--fontColor); font-weight: 600; line-height: 2; } .icon-close { cursor: pointer; } .icon-close::before { transform: rotate(45deg); } .icon-close::before, .icon-close::after { content: ""; display: block; position: absolute; top: 28px; right: 30px; background-color: #ffffff; width: 20px; height: 3px; border-radius: 2px; } .icon-close::after { transform: rotate(-45deg); } .popup-body-response { overflow-y: auto; display: flex; justify-content: center; padding: 12px 24px; overflow-y: auto; /\* min-height: 100px;\*/ align-items: center; } .messagebody { font-size: 18px; color: var(--textColor); font-weight: 300; } .popup-footer-response { width: auto; text-align: center; padding: 12px 0; position: relative; } #PopUp-body { display: none; } .user-image-56{ min-width:44px; min-height:44px; } /\*popup css end\*/

*   (2) Comments

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

[![C# Corner Ebook](https://www.c-sharpcorner.com/UploadFile/EBooks/10032013032153AM/05062014012012AM10032013032438AMBook.png)](https://www.c-sharpcorner.com/ebooks/exploring-design-pattern-for-dummies)

[

Exploring Design Pattern for Dummies

](https://www.c-sharpcorner.com/ebooks/exploring-design-pattern-for-dummies)

Read by 13.4k people

[Download Now!](https://www.c-sharpcorner.com/ebooks/exploring-design-pattern-for-dummies)

/\* date-time conversion js Start \*/ var options = { weekday: "long", year: "numeric", month: "long", day: "numeric" }; function GetDayName(e) { var n = \["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"\]; return n\[e\]; } function getTimezoneName() { const e = new Date(); const n = e.toLocaleDateString(); const t = e.toLocaleDateString(undefined, { timeZoneName: "long" }); const o = t.indexOf(n); if (o >= 0) { return (t.substring(0, o) + t.substring(o + n.length)).replace(/^\[\\s,.\\-:;\]+|\[\\s,.\\-:;\]+$/g, ""); } return t; } function convertUTCDateToLocalDate(date) { var localDate = new Date(date); localDate.setMinutes(date.getMinutes() - date.getTimezoneOffset()); return localDate; } /\* date-time conversion js Ends \*/ document.addEventListener("DOMContentLoaded", function () { // Set the local timezone name var tzInput = document.getElementById("HiddenLocalTimezone"); if (tzInput) { tzInput.value = getTimezoneName(); } var index = 1; while (true) { var hiddenInput = document.getElementById("HiddenUTCDateTimeList\_" + index); if (!hiddenInput) break; var utcDateStr = hiddenInput.value; var utcDate = new Date(utcDateStr); // Convert UTC to Local var localDate = convertUTCDateToLocalDate(utcDate); var evntDt = convertUTCDateToLocalDate(utcDate); var monthDay = localDate.toLocaleString('default', { month: 'short' }); var shortMonthName = monthDay.substring(0, 3) + " " + localDate.getDate(); var localTimeString = localDate.toLocaleString('en-US', { hour: 'numeric', minute: 'numeric', hour12: true }); var time = localTimeString.split(' '); var timeHM = time\[0\].split(':'); var timeMin = (timeHM\[1\] === "00") ? timeHM\[0\] : time\[0\]; var localDayTime = GetDayName(localDate.getDay()) + " " + timeMin + " " + time\[1\]; var monthDaySplit = shortMonthName.split(' '); var monthEl = document.getElementById("LocalMonth" + index); var dayEl = document.getElementById("LocalDay" + index); if (monthEl) monthEl.textContent = monthDaySplit\[0\].toUpperCase(); if (dayEl) dayEl.textContent = monthDaySplit\[1\]; // Optional: Hide past event /\* const currentdate = new Date(); if (evntDt <= currentdate) { var eventElement = document.getElementById(\`event-${index}\`); if (eventElement) { eventElement.style.display = "none"; } } \*/ index++; } });

 [![Generative AI for Leaders](https://www.c-sharpcorner.com/UploadFile/Ads/1.gif "Generative AI for Leaders")](https://viberondemand.com/generative-ai-for-leaders)document.addEventListener("DOMContentLoaded", function () { setTimeout(() => { const img = document.getElementById("imgAdslazyImageOnTop"); img.src = img.getAttribute("data-src"); img.style.display = "block"; }, 5000); });