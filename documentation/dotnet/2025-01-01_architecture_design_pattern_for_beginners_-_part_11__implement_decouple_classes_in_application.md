# Design Pattern For Beginners - Part 11: Implement Decouple Classes in Application

**Source:** https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-11-implement-decouple-cl/
**Date Captured:** 2025-07-28T16:09:17.395Z
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
*   [Login](/userregistration/logincheck.aspx?returnurl=/uploadfile/dacca2/design-pattern-for-beginner-part-11-implement-decouple-cl/)

// Get references to the elements const hoverTarget = document.getElementById('ctl00\_mainleft\_userProfImg'); const hiddenDiv = document.getElementById('userLinks'); // Add event listeners for mouse enter and leave if (hoverTarget != null) { hoverTarget.addEventListener('mouseenter', () => { // Show the hidden div when mouse enters hiddenDiv.style.display = 'block'; }); } if (hoverTarget != null) { hoverTarget.addEventListener('mouseleave', () => { // Hide the hidden div when mouse leaves setTimeout(function () { hiddenDiv.style.display = 'none'; }, 500); }); } document.addEventListener("DOMContentLoaded", function () { var listItems = document.querySelectorAll(".my-account-menu > li"); listItems.forEach(function (item) { item.addEventListener("mouseover", function () { setTimeout(function () { // Add your hover action here item.classList.add('hovered'); }, 500); // 500 milliseconds delay }); item.addEventListener("mouseout", function () { setTimeout(function () { item.classList.remove('hovered'); }, 500); }); }); });

 [![Prompt Engineering Training](https://www.c-sharpcorner.com/UploadFile/Ads/7.png)](https://viberondemand.com/prompt-engineering)document.addEventListener("DOMContentLoaded", function () { setTimeout(() => { const img = document.getElementById("imgAdslazyImageOnTop1"); img.src = img.getAttribute("data-src"); img.style.display = "block"; }, 3000); });

 [![Design Patterns & Practices](https://www.c-sharpcorner.com/UploadFile/MinorCatImages/230207PM.png "Design Patterns & Practices")](https://www.c-sharpcorner.com/technologies/design-patterns-and-practices) 

# Design Pattern For Beginners - Part 11: Implement Decouple Classes in Application

[](/featured-articles "Featured")

*   [](https://www.facebook.com/sharer.php?u=https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-11-implement-decouple-cl/ "Share on Facebook")
*   [](https://twitter.com/intent/tweet?&via=CsharpCorner&related=CsharpCorner&text=Design+Pattern+For+Beginners+-+Part+11%3a+Implement+Decouple+Classes+in+Application https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-11-implement-decouple-cl/ "Share on Twitter")
*   [](https://www.linkedin.com/shareArticle?mini=true&url=https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-11-implement-decouple-cl/ "Share on Linkedin")
*   [](//www.reddit.com/submit?title=Design+Pattern+For+Beginners+-+Part+11%3a+Implement+Decouple+Classes+in+Application&url=https%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fdacca2%2fdesign-pattern-for-beginner-part-11-implement-decouple-cl%2fdefault.aspx "Share on Reddit")
*   [WhatsApp](whatsapp://send?text=Design Pattern For Beginners - Part 11: Implement Decouple Classes in Application%0A%0Ahttps%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fdacca2%2fdesign-pattern-for-beginner-part-11-implement-decouple-cl%2f "Share on Whatsapp")
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/articles/emailtofriend.aspx?articleid=874609e4-7fa0-4e4e-8b96-5e7ad03df307 "Email this article to friend")
*   [](javascript:void\(0\); "Bookmark this article")
*   [](javascript:void\(GetPrintVersion\('Article'\)\) "Print")
*   [](https://www.c-sharpcorner.com/members/sourav-kayal/articles "Author's other article")

*    [![Author Profile Photo](https://www.c-sharpcorner.com/UploadFile/AuthorImage/dacca220151120112330.jpg.ashx?height=24&width=24 "Sourav Kayal")](https://www.c-sharpcorner.com/members/sourav-kayal)[Sourav Kayal](https://www.c-sharpcorner.com/members/sourav-kayal)
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/uploadfile/dacca2/design-pattern-for-beginner-part-11-implement-decouple-cl/history "Article history")6y
*   114.1k
*   2
*   2
    
[100](/members/rank.aspx?page=points-table "Points")*   [Article](/articles/)

Take the challenge 

Welcome to the Design Pattern for Beginners article series. Thanks to all, special thanks to those who were with us from the beginning. If you are new to this series then I suggest you go through all the previous articles.

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

In this article we will learn how to implement the Implement Decouple Architecture in applications. Decoupling is very important in application development. It helps up to attach and detach components of any class without affecting another class.  
  
The main goal of decoupling is to reduce dependency. Let's use an example to understand the concept. Think about a Desktop computer. If the monitor does not work then we just unplug it and replace it with a new one, even a different brand would work.  
  
If the hard disk does not work then we just unplug it (hope you know how to unplug it! Ha ..Ha) and install a new one. Now, think of how to decouple each and every component? When we are changing one, it does not at all affect other components and the computer is running happily.  
  
Now, our purpose is to implement the same kind of concept in software development. OK, one thing has been forgotten for explanation, tight coupling. Yes, it indicates that two entities are very much dependent on each other. If we change one then it will definitely effect another one.  
  
OK, no more discussion. Let's implement the concept of decoupling.  
  
**How to implement?  
**  
Hmm…, one big question is, how to implement it? The solution is an interface. As the name suggests, it will be in the middle of two classes.  
  
**How it will come?  
**  
Let's see with an example. Here we will communicate with two classes using an interface.  

1.  using System;  
2.  using System.Collections.Generic;  
3.  using System.Linq;  
4.  using System.Text;  
5.  namespace InterfaceService  
6.  {  
7.      public interface IService  
8.      {  
9.          void Serve();  
10.      }  
11.      public class Server : IService  
12.      {  
13.          public void Serve()  
14.          {  
15.              Console.WriteLine("Server executes");  
16.          }  
17.      }  
18.      public class Consumer  
19.      {  
20.          IService obj;  
21.          public Consumer(IService temp)  
22.          {  
23.              this.obj = temp;  
24.          }  
25.          public void Execute()  
26.          {  
27.              obj.Serve();  
28.          }  
29.      }  
30.  }  
    

Let's discuss the code above. At first we have created an Interface called IService. Then we implemented the IService interface within the Server class. Now, let's discuss the Consumer class. Within the Consumer constructor we are assigning an object of the Iservice interface and this object is being set to a local objet of the IService interface.

  
Now you can see that nowhere in the Consumer class have we specified a class name server and hence we are achieving decoupling.  
  
Let's call the consumer class from the client code.

1.  using System;  
2.  using System.Collections.Generic;  
3.  using System.Linq;  
4.  using System.Text;  
5.  namespace DependencyInjection  
6.  {  
7.      class Program  
8.      {  
9.          static void Main(string\[\] args)  
10.          {  
11.              Consumer C = new Consumer(new Server());  
12.              C.Execute();  
13.              Console.ReadLine();  
14.          }  
15.      }  
16.  }  
    

Within the Main function we are creating an object of the Consumer class by passing an object of the server class. And we are calling the Execute function to execute the service.  
  
Here is sample output.  
  
  
  
Now the question is, how will it help us? What is the facility for that? Let's change the class name from Server to Server123.

1.  public class Server123 : IService  
2.  {  
3.      public void Serve()  
4.      {  
5.          Console.WriteLine("Server executes");  
6.      }  
7.  }  
8.  public class Consumer  
9.  {  
10.      IService obj;  
11.      public Consumer(IService temp)  
12.      {  
13.          this.obj = temp;  
14.      }  
15.      public void Execute()  
16.      {  
17.          obj.Serve();  
18.      }   
19.  }  
    

See, we have changed the server class name but does not change anything in the Consumer class. Now, just create an object of the new server class (Server123) from the client, as in the following.

1.  static void Main(string\[\] args)  
2.  {  
3.      Consumer C = new Consumer(new Server123());  
4.      C.Execute();  
5.      Console.ReadLine();  
6.  }  
    

And you will get the same output. Now again change the Class name from Consumer to Consumer123.

  
Have a look at the following code.

1.  public class Server : IService  
2.  {  
3.      public void Serve()  
4.      {  
5.          Console.WriteLine("Server executes");  
6.      }  
7.  }  
8.  public class Consumer123  
9.  {  
10.      IService obj;  
11.      public Consumer123(IService temp)  
12.      {  
13.          this.obj = temp;  
14.      }  
15.      public void Execute()  
16.      {  
17.          obj.Serve();  
18.      }  
19.  }  
    

Here is the client code:

1.  static void Main(string\[\] args)  
2.  {  
3.      Consumer123 C = new Consumer123(new Server());  
4.      C.Execute();  
5.      Console.ReadLine();  
6.  }  
    

Now, we have tuned up the Consumer class and within the Main() function (Client code) but it does not even touch the server class. And we will get the same output.

This is the advantage of decouple architecture.  Hope you understood the concept.

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

[![C# Corner Ebook](https://www.c-sharpcorner.com/UploadFile/EBooks/10282022072733AM/10282022090131AMArchitecting-Modern-Applications-using-Monolithic-architecture-In-Asp.Net-Core.jpg)](https://www.c-sharpcorner.com/ebooks/architecting-modern-applications-using-monolithic-architecture-in-asp-net-core-w)

[

Architecting Modern Applications using Monolithic Architecture in Asp.Net Core Web API

](https://www.c-sharpcorner.com/ebooks/architecting-modern-applications-using-monolithic-architecture-in-asp-net-core-w)

Read by 6.3k people

[Download Now!](https://www.c-sharpcorner.com/ebooks/architecting-modern-applications-using-monolithic-architecture-in-asp-net-core-w)

/\* date-time conversion js Start \*/ var options = { weekday: "long", year: "numeric", month: "long", day: "numeric" }; function GetDayName(e) { var n = \["SUN", "MON", "TUE", "WED", "THU", "FRI", "SAT"\]; return n\[e\]; } function getTimezoneName() { const e = new Date(); const n = e.toLocaleDateString(); const t = e.toLocaleDateString(undefined, { timeZoneName: "long" }); const o = t.indexOf(n); if (o >= 0) { return (t.substring(0, o) + t.substring(o + n.length)).replace(/^\[\\s,.\\-:;\]+|\[\\s,.\\-:;\]+$/g, ""); } return t; } function convertUTCDateToLocalDate(date) { var localDate = new Date(date); localDate.setMinutes(date.getMinutes() - date.getTimezoneOffset()); return localDate; } /\* date-time conversion js Ends \*/ document.addEventListener("DOMContentLoaded", function () { // Set the local timezone name var tzInput = document.getElementById("HiddenLocalTimezone"); if (tzInput) { tzInput.value = getTimezoneName(); } var index = 1; while (true) { var hiddenInput = document.getElementById("HiddenUTCDateTimeList\_" + index); if (!hiddenInput) break; var utcDateStr = hiddenInput.value; var utcDate = new Date(utcDateStr); // Convert UTC to Local var localDate = convertUTCDateToLocalDate(utcDate); var evntDt = convertUTCDateToLocalDate(utcDate); var monthDay = localDate.toLocaleString('default', { month: 'short' }); var shortMonthName = monthDay.substring(0, 3) + " " + localDate.getDate(); var localTimeString = localDate.toLocaleString('en-US', { hour: 'numeric', minute: 'numeric', hour12: true }); var time = localTimeString.split(' '); var timeHM = time\[0\].split(':'); var timeMin = (timeHM\[1\] === "00") ? timeHM\[0\] : time\[0\]; var localDayTime = GetDayName(localDate.getDay()) + " " + timeMin + " " + time\[1\]; var monthDaySplit = shortMonthName.split(' '); var monthEl = document.getElementById("LocalMonth" + index); var dayEl = document.getElementById("LocalDay" + index); if (monthEl) monthEl.textContent = monthDaySplit\[0\].toUpperCase(); if (dayEl) dayEl.textContent = monthDaySplit\[1\]; // Optional: Hide past event /\* const currentdate = new Date(); if (evntDt <= currentdate) { var eventElement = document.getElementById(\`event-${index}\`); if (eventElement) { eventElement.style.display = "none"; } } \*/ index++; } });

 [![Generative AI for Leaders](https://www.c-sharpcorner.com/UploadFile/Ads/1.gif "Generative AI for Leaders")](https://viberondemand.com/generative-ai-for-leaders)document.addEventListener("DOMContentLoaded", function () { setTimeout(() => { const img = document.getElementById("imgAdslazyImageOnTop"); img.src = img.getAttribute("data-src"); img.style.display = "block"; }, 5000); });