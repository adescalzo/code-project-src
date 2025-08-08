# Design Pattern For Beginner: Part-10: Observer Design Pattern

**Source:** https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-10-observer-design-patter/
**Date Captured:** 2025-07-28T16:09:11.375Z
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
*   [Login](/userregistration/logincheck.aspx?returnurl=/uploadfile/dacca2/design-pattern-for-beginner-part-10-observer-design-patter/)

// Get references to the elements const hoverTarget = document.getElementById('ctl00\_mainleft\_userProfImg'); const hiddenDiv = document.getElementById('userLinks'); // Add event listeners for mouse enter and leave if (hoverTarget != null) { hoverTarget.addEventListener('mouseenter', () => { // Show the hidden div when mouse enters hiddenDiv.style.display = 'block'; }); } if (hoverTarget != null) { hoverTarget.addEventListener('mouseleave', () => { // Hide the hidden div when mouse leaves setTimeout(function () { hiddenDiv.style.display = 'none'; }, 500); }); } document.addEventListener("DOMContentLoaded", function () { var listItems = document.querySelectorAll(".my-account-menu > li"); listItems.forEach(function (item) { item.addEventListener("mouseover", function () { setTimeout(function () { // Add your hover action here item.classList.add('hovered'); }, 500); // 500 milliseconds delay }); item.addEventListener("mouseout", function () { setTimeout(function () { item.classList.remove('hovered'); }, 500); }); }); });

 [![Prompt Engineering Training](https://www.c-sharpcorner.com/UploadFile/Ads/7.png)](https://viberondemand.com/prompt-engineering)document.addEventListener("DOMContentLoaded", function () { setTimeout(() => { const img = document.getElementById("imgAdslazyImageOnTop1"); img.src = img.getAttribute("data-src"); img.style.display = "block"; }, 3000); });

 [![Design Patterns & Practices](https://www.c-sharpcorner.com/UploadFile/MinorCatImages/230207PM.png "Design Patterns & Practices")](https://www.c-sharpcorner.com/technologies/design-patterns-and-practices) 

# Design Pattern For Beginner: Part-10: Observer Design Pattern

[](/featured-articles "Featured")

*   [](https://www.facebook.com/sharer.php?u=https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-10-observer-design-patter/ "Share on Facebook")
*   [](https://twitter.com/intent/tweet?&via=CsharpCorner&related=CsharpCorner&text=Design+Pattern+For+Beginner%3a+Part-10%3a+Observer+Design+Pattern https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-10-observer-design-patter/ "Share on Twitter")
*   [](https://www.linkedin.com/shareArticle?mini=true&url=https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-10-observer-design-patter/ "Share on Linkedin")
*   [](//www.reddit.com/submit?title=Design+Pattern+For+Beginner%3a+Part-10%3a+Observer+Design+Pattern&url=https%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fdacca2%2fdesign-pattern-for-beginner-part-10-observer-design-patter%2fdefault.aspx "Share on Reddit")
*   [WhatsApp](whatsapp://send?text=Design Pattern For Beginner: Part-10: Observer Design Pattern%0A%0Ahttps%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fdacca2%2fdesign-pattern-for-beginner-part-10-observer-design-patter%2f "Share on Whatsapp")
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/articles/emailtofriend.aspx?articleid=bf9ad164-09a7-4c83-b587-c0be287afc77 "Email this article to friend")
*   [](javascript:void\(0\); "Bookmark this article")
*   [](javascript:void\(GetPrintVersion\('Article'\)\) "Print")
*   [](https://www.c-sharpcorner.com/members/sourav-kayal/articles "Author's other article")

*    [![Author Profile Photo](https://www.c-sharpcorner.com/UploadFile/AuthorImage/dacca220151120112330.jpg.ashx?height=24&width=24 "Sourav Kayal")](https://www.c-sharpcorner.com/members/sourav-kayal)[Sourav Kayal](https://www.c-sharpcorner.com/members/sourav-kayal)
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/uploadfile/dacca2/design-pattern-for-beginner-part-10-observer-design-patter/history "Article history")6y
*   45.2k
*   0
*   0
    
[100](/members/rank.aspx?page=points-table "Points")*   [Article](/articles/)

Take the challenge 

Welcome to the Design Pattern For Beginners article series. In this article series we are discussing various design patterns of software development. This is the 10th presentation and if you are new to this series then I suggest you go through all previous articles.

*   [Design Pattern For Beginners - Part 1: Singleton Design Pattern](https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-1-singleton-design-patt/)
*   [Design Pattern For Beginners - Part 2: Factory Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-2-factory-design-pattern/)
*   [Design Pattern For Beginners - Part 3: Prototype Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-3-prototype-design-patter/)
*   [Design Pattern For Beginners - Part 4: Decorator Design Pattern](/UploadFile/dacca2/design-pattern-for-beginners-part-4-decorator-design-patt/)
*   [Design Pattern For Beginners - Part 5: Composite Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-5-composite-design-patter/)
*   [Design Pattern For Beginners - Part 6: Adaptor Design Pattern](/UploadFile/dacca2/design-pattern-for-beginners-part-6-adaptor-design-patter/)
*   [Design Pattern For Beginners - Part 7: Bridge Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-bridge-design-pattern/)
*   [Design Pattern For Beginners - Part 8: memento Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-8-memento-design-patter/)
*   [Design Pattern for Beginners - Part-9: Strategy Design Pattern](/UploadFile/dacca2/design-pattern-for-beginner-part-9-strategy-design-pattern/)

Let's discuss the importance of the Observer Design Pattern and when it needs to be implemented.

**Why observer pattern?  
**  
As the name suggests, it's something related to observation. The question is, who is the observer? The observers are nothing but various systems.  
  
The concept is, one or more systems will be the observer simultaneously and if necessary they can start their action. It's like a bodyguard. Right?  
  
Let's talk about a notification system where the user can send notifications in various ways. They may use SMS notification or Mail Notification or Event Log.  
  
Now, all the notification systems will be alive continuosly, and if needed we can use any one of them, or more than one simultaneously. So , if we draw the conclusion, observer pattern is fit that situation where we choose and use systems at run time. Whereas all systems will alive continuosly. Let's try to implement that in code.  
**  
Create various notification classes  
**  
We are interested in implementing a uniform naming convention. For that we will implement all notification classes from the INotifyObserver Interface. Each notification class will be implementing a Notify() method.  

1.  using System;  
2.  using System.Collections.Generic;  
3.  using System.Linq;  
4.  using System.Text;  
5.  namespace ObserverPattern  
6.  {  
7.      interface INotifyObserver  
8.      {  
9.          void Notify();  
10.      }  
11.      class MailNotify : INotifyObserver  
12.      {  
13.          public void Notify()  
14.          {  
15.              Console.WriteLine("Notify through Mail");  
16.          }  
17.      }  
18.      class EventNotify : INotifyObserver  
19.      {  
20.          public void Notify()  
21.          {  
22.              Console.WriteLine("Notify through Event");  
23.          }  
24.      }  
25.      class SMSNotify : INotifyObserver  
26.      {  
27.          public void Notify()  
28.          {  
29.              Console.WriteLine("Notify through SMS");  
30.          }  
31.      }  
32.  } 

**Create notifier class**

This is a very interesting and important part of the Observer Design Pattern. We can say clsNotifier (see the following code) is our control room. From here we will control which kind of notification will execute.  

1.  using System;  
2.  using System.Collections.Generic;  
3.  using System.Linq;  
4.  using System.Text;  
5.  using System.Collections;  
6.  namespace ObserverPattern  
7.  {  
8.      class clsNotifier  
9.      {  
10.          public ArrayList ALNotify = new ArrayList();  
11.          /// <summary>  
12.          /// Add object of notification System  
13.          /// </summary>  
14.          /// <param name="obj">Object is notification class</param>  
15.          public void AddService(INotifyObserver obj)  
16.          {  
17.              ALNotify.Add(obj);  
18.          }  
19.          /// <summary>  
20.          /// Remove object of notification System  
21.          /// </summary>  
22.          /// <param name="obj">Object of notification Calss</param>  
23.          public void RemoveService(INotifyObserver obj)  
24.          {  
25.              ALNotify.Remove(obj);  
26.          }  
27.          public void ExecuteNotifier()  
28.          {  
29.              foreach (INotifyObserver O in ALNotify)  
30.              {  
31.                  //Call all notification System  
32.                  O.Notify();  
33.              }  
34.          }  
35.      }  
36.  }  
    

AddService() and RemoveService() are two functions by which we can add an object of various notification classes and ExecuteNotifier() will call all the Notify() functions from each notification class.

**Design client code**  

This is the last part of the example. We will create client code to set and make decisions about which notification will be fired.  

1.  using System;  
2.  using System.Collections.Generic;  
3.  using System.Linq;  
4.  using System.Text;  
5.  namespace ObserverPattern  
6.  {  
7.      class Program  
8.      {  
9.          static void Main(string\[\] args)  
10.          {  
11.              //Generate exception to notify all client  
12.              try  
13.              {  
14.                  throw new ApplicationException("This is Exception");  
15.              }  
16.              catch (Exception ex)  
17.              {  
18.                  INotifyObserver obj1 = new MailNotify();  
19.                  INotifyObserver obj2 = new SMSNotify();  
20.                  clsNotifier O = new clsNotifier();  
21.                  O.AddService(obj1);  
22.                  O.AddService(obj2);  
23.                  O.ExecuteNotifier();  
24.              }  
25.              Console.ReadLine();  
26.          }  
27.      }  
28.  } 

Here, we are generating ApplicationException() and within the Catch block we are creating an object to the MailNotification and SMSnotification class.

The executeNotifier() will call all Notify() functions from each notification class.

**Output**

  

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