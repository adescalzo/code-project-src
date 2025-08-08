# Design Pattern For Beginner Part 7: Bridge Design Pattern

**Source:** https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-bridge-design-pattern/
**Date Captured:** 2025-07-28T16:08:22.140Z
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
*   [Login](/userregistration/logincheck.aspx?returnurl=/uploadfile/dacca2/design-pattern-for-beginner-bridge-design-pattern/)

// Get references to the elements const hoverTarget = document.getElementById('ctl00\_mainleft\_userProfImg'); const hiddenDiv = document.getElementById('userLinks'); // Add event listeners for mouse enter and leave if (hoverTarget != null) { hoverTarget.addEventListener('mouseenter', () => { // Show the hidden div when mouse enters hiddenDiv.style.display = 'block'; }); } if (hoverTarget != null) { hoverTarget.addEventListener('mouseleave', () => { // Hide the hidden div when mouse leaves setTimeout(function () { hiddenDiv.style.display = 'none'; }, 500); }); } document.addEventListener("DOMContentLoaded", function () { var listItems = document.querySelectorAll(".my-account-menu > li"); listItems.forEach(function (item) { item.addEventListener("mouseover", function () { setTimeout(function () { // Add your hover action here item.classList.add('hovered'); }, 500); // 500 milliseconds delay }); item.addEventListener("mouseout", function () { setTimeout(function () { item.classList.remove('hovered'); }, 500); }); }); });

 [![Prompt Engineering Training](https://www.c-sharpcorner.com/UploadFile/Ads/7.png)](https://viberondemand.com/prompt-engineering)document.addEventListener("DOMContentLoaded", function () { setTimeout(() => { const img = document.getElementById("imgAdslazyImageOnTop1"); img.src = img.getAttribute("data-src"); img.style.display = "block"; }, 3000); });

 [![Design Patterns & Practices](https://www.c-sharpcorner.com/UploadFile/MinorCatImages/230207PM.png "Design Patterns & Practices")](https://www.c-sharpcorner.com/technologies/design-patterns-and-practices) 

# Design Pattern For Beginner Part 7: Bridge Design Pattern

[](/featured-articles "Featured")

*   [](https://www.facebook.com/sharer.php?u=https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-bridge-design-pattern/ "Share on Facebook")
*   [](https://twitter.com/intent/tweet?&via=CsharpCorner&related=CsharpCorner&text=Design+Pattern+For+Beginner+Part+7%3a+Bridge+Design+Pattern https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-bridge-design-pattern/ "Share on Twitter")
*   [](https://www.linkedin.com/shareArticle?mini=true&url=https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-bridge-design-pattern/ "Share on Linkedin")
*   [](//www.reddit.com/submit?title=Design+Pattern+For+Beginner+Part+7%3a+Bridge+Design+Pattern&url=https%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fdacca2%2fdesign-pattern-for-beginner-bridge-design-pattern%2fdefault.aspx "Share on Reddit")
*   [WhatsApp](whatsapp://send?text=Design Pattern For Beginner Part 7: Bridge Design Pattern%0A%0Ahttps%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fdacca2%2fdesign-pattern-for-beginner-bridge-design-pattern%2f "Share on Whatsapp")
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/articles/emailtofriend.aspx?articleid=0fdfd45b-ff25-4231-8909-7587731cf629 "Email this article to friend")
*   [](javascript:void\(0\); "Bookmark this article")
*   [](javascript:void\(GetPrintVersion\('Article'\)\) "Print")
*   [](https://www.c-sharpcorner.com/members/sourav-kayal/articles "Author's other article")

*    [![Author Profile Photo](https://www.c-sharpcorner.com/UploadFile/AuthorImage/dacca220151120112330.jpg.ashx?height=24&width=24 "Sourav Kayal")](https://www.c-sharpcorner.com/members/sourav-kayal)[Sourav Kayal](https://www.c-sharpcorner.com/members/sourav-kayal)
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/uploadfile/dacca2/design-pattern-for-beginner-bridge-design-pattern/history "Article history")6y
*   94k
*   2
*   5
    
[100](/members/rank.aspx?page=points-table "Points")*   [Article](/articles/)

Take the challenge 

Welcome to the Design Pattern For Beginner article series. In this article we will try to understand the Bridge Design Pattern. If this article is your first article in this series then I suggest you visit the other articles in this series.

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
    

**Why Bridge Pattern?**

Before starting with a technical explanation and example we will try to understand why the Bridge Design Pattern is essential and in which scenario it will be implemented.  
  
As the name suggests, it makes a bridge between two components. Here the component may be two classes or any other entity. So the Bridge Design Pattern basically makes a channel between two components. And in this way it helps to create a de-couple architecture. We can communicate with two classes through the bridge component without changing existing class definitions.  
![BridgePattern1.jpg](https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-bridge-design-pattern/Images/BridgePattern1.jpg)  
So if we summarize the basic need for the Bridge Design Pattern then we will see that it help us to design a de-couple architecture in the software project. It makes abstraction over implementation.  
**  
Let's implement the first example**  
  
In the following we implement one small example of the Bridge Design Pattern. When we are talking of bridge let's implement a simple bridge between two cities. Here instead of city we will call it node.  
  
In the example we have implemented an Inode interface in both the Node\_A and Node\_B class. Then in the following, the two node classes, we have defined another class called Bridge, that will communicate between two cities. The ReachTo() function of the bridge class will take us to a specific city (node). Now we are clearly seeing the Program class (Main() function) is talking with any city class (node) through the Bridge class.  

1.  using System;  
2.  using System.Collections;  
3.  namespace BridgeDesign  
4.  {  
5.      public interface Inode  
6.      {  
7.          void Reach();  
8.      }  
9.      class Node\_A : Inode  
10.      {  
11.          public void Reach()  
12.          {  
13.              Console.WriteLine("Rreached to node A");  
14.          }  
15.      }  
16.      class Node\_B : Inode  
17.      {  
18.          public void Reach()  
19.          {  
20.              Console.WriteLine("Rreached to node B");  
21.          }  
22.      }  
23.      class Bridge  
24.      {  
25.          public void ReachTo(Inode obj)  
26.          {  
27.              obj.Reach();  
28.          }  
29.      }  
30.      class Program  
31.      {  
32.          static void Main(string\[\] args)  
33.          {  
34.              Bridge br = new Bridge();  
35.              Node\_A a = new Node\_A();  
36.              Node\_B b = new Node\_B();  
37.              br.ReachTo(a); //Reach to Node\_A  
38.              br.ReachTo(b); //Reach to Node\_B  
39.              Console.ReadLine();  
40.          }  
41.      }  
42.  }  
    

Here is sample output.

  
  
Hmm.. What an unrealistic example. Yes this example is only for you to understand the basic implementation of the Bridge Design Pattern. OK, let's work with one useful example (at least related to realistic project development).  
  
The Mail sending operation was implemented by a Bridge Pattern.  
  
Mail sending and notification is a very common task for a software developer. We can now send mail in various ways, for example in a new version of an old project you have developed a mail sending function in C# but in the same project there is another function to send mail that is written in VB6 (maybe it was written by a senior of your seniors. Ha ..Ha..) and is still working fine.  
  
Anyway, your manager said that they want to use both C# and VB versions associated with sending mail from a database (so, in total three different ways). Now, let's implement this scenario using the Bridge Design Pattern. Have a look at the following example.  

1.  using System;  
2.  using System.Collections;  
3.  namespace BridgeDesign  
4.  {  
5.      public interface IMessage  
6.      {  
7.          void Send();  
8.      }  
9.      class CSharp\_Mail : IMessage  
10.      {  
11.          public void Send()  
12.          {  
13.              Console.WriteLine("Mail send from C# code");  
14.          }  
15.      }  
16.      class VB\_Mail : IMessage  
17.      {  
18.          public void Send()  
19.          {  
20.              Console.WriteLine("Mail send from VB Code");  
21.          }  
22.      }  
23.      class Databas\_Mail : IMessage  
24.      {  
25.          public void Send()  
26.          {  
27.              Console.WriteLine("Mail send from Database");  
28.          }  
29.      }  
30.      class MailSendBridge  
31.      {  
32.          public void SendFrom(IMessage obj)  
33.          {  
34.              obj.Send();  
35.          }  
36.      }  
37.      class Program  
38.      {  
39.          static void Main(string\[\] args)  
40.          {  
41.              MailSendBridge mb = new MailSendBridge();  
42.              CSharp\_Mail objCS = new CSharp\_Mail();  
43.              VB\_Mail objVB = new VB\_Mail();  
44.              mb.SendFrom(objCS);    //Mail send from C# cod  
45.              mb.SendFrom(objVB);  //Mail Send from VB Code  
46.              Console.ReadLine();  
47.          }  
48.      }  
49.  }  
    

If you observe closely the body of the Main() function, you will find we are sending just an appropriate object to use the proper mail sending function. Here the MailSendBridge class is creating one abstraction over the implementation of the actual mail sending mechanism that was defined by a different mail sending class.

Here is sample output.  
  
  
  
**Conclusion**  
  
Here we were trying to understand the Bridge Design Pattern with examples. IU hope you have understood the basic concept and implementation of the Bridge Design Pattern.

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