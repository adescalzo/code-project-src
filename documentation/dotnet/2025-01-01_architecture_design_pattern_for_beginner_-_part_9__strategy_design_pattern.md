# Design Pattern For Beginner - Part 9: Strategy Design Pattern

**Source:** https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-9-strategy-design-pattern/
**Date Captured:** 2025-07-28T16:09:05.655Z
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
*   [Login](/userregistration/logincheck.aspx?returnurl=/uploadfile/dacca2/design-pattern-for-beginner-part-9-strategy-design-pattern/)

// Get references to the elements const hoverTarget = document.getElementById('ctl00\_mainleft\_userProfImg'); const hiddenDiv = document.getElementById('userLinks'); // Add event listeners for mouse enter and leave if (hoverTarget != null) { hoverTarget.addEventListener('mouseenter', () => { // Show the hidden div when mouse enters hiddenDiv.style.display = 'block'; }); } if (hoverTarget != null) { hoverTarget.addEventListener('mouseleave', () => { // Hide the hidden div when mouse leaves setTimeout(function () { hiddenDiv.style.display = 'none'; }, 500); }); } document.addEventListener("DOMContentLoaded", function () { var listItems = document.querySelectorAll(".my-account-menu > li"); listItems.forEach(function (item) { item.addEventListener("mouseover", function () { setTimeout(function () { // Add your hover action here item.classList.add('hovered'); }, 500); // 500 milliseconds delay }); item.addEventListener("mouseout", function () { setTimeout(function () { item.classList.remove('hovered'); }, 500); }); }); });

 [![Prompt Engineering Training](https://www.c-sharpcorner.com/UploadFile/Ads/7.png)](https://viberondemand.com/prompt-engineering)document.addEventListener("DOMContentLoaded", function () { setTimeout(() => { const img = document.getElementById("imgAdslazyImageOnTop1"); img.src = img.getAttribute("data-src"); img.style.display = "block"; }, 3000); });

 [![Design Patterns & Practices](https://www.c-sharpcorner.com/UploadFile/MinorCatImages/230207PM.png "Design Patterns & Practices")](https://www.c-sharpcorner.com/technologies/design-patterns-and-practices) 

# Design Pattern For Beginner - Part 9: Strategy Design Pattern

[](/featured-articles "Featured")

*   [](https://www.facebook.com/sharer.php?u=https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-9-strategy-design-pattern/ "Share on Facebook")
*   [](https://twitter.com/intent/tweet?&via=CsharpCorner&related=CsharpCorner&text=Design+Pattern+For+Beginner+-+Part+9%3a+Strategy+Design+Pattern https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-9-strategy-design-pattern/ "Share on Twitter")
*   [](https://www.linkedin.com/shareArticle?mini=true&url=https://www.c-sharpcorner.com/UploadFile/dacca2/design-pattern-for-beginner-part-9-strategy-design-pattern/ "Share on Linkedin")
*   [](//www.reddit.com/submit?title=Design+Pattern+For+Beginner+-+Part+9%3a+Strategy+Design+Pattern&url=https%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fdacca2%2fdesign-pattern-for-beginner-part-9-strategy-design-pattern%2fdefault.aspx "Share on Reddit")
*   [WhatsApp](whatsapp://send?text=Design Pattern For Beginner - Part 9: Strategy Design Pattern%0A%0Ahttps%3a%2f%2fwww.c-sharpcorner.com%2fUploadFile%2fdacca2%2fdesign-pattern-for-beginner-part-9-strategy-design-pattern%2f "Share on Whatsapp")
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/articles/emailtofriend.aspx?articleid=8c3a2ef3-d3da-4365-b06b-5a38fdf1765e "Email this article to friend")
*   [](javascript:void\(0\); "Bookmark this article")
*   [](javascript:void\(GetPrintVersion\('Article'\)\) "Print")
*   [](https://www.c-sharpcorner.com/members/sourav-kayal/articles "Author's other article")

*    [![Author Profile Photo](https://www.c-sharpcorner.com/UploadFile/AuthorImage/dacca220151120112330.jpg.ashx?height=24&width=24 "Sourav Kayal")](https://www.c-sharpcorner.com/members/sourav-kayal)[Sourav Kayal](https://www.c-sharpcorner.com/members/sourav-kayal)
*   [](https://www.c-sharpcorner.com/userregistration/logincheck.aspx?returnurl=https://www.c-sharpcorner.com/uploadfile/dacca2/design-pattern-for-beginner-part-9-strategy-design-pattern/history "Article history")6y
*   38.9k
*   2
*   0
    
[100](/members/rank.aspx?page=points-table "Points")*   [Article](/articles/)

Take the challenge 

Welcome to the Design Pattern for Beginners article series. If you are a new reader then I suggest you visit the previous articles in this series.

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
    

**Why Strategies Design Pattern?**  
  
In this article we will discuss the Strategies Design Pattern. First of all we will try to understand what the Strategies Design Pattern is and in which scenario it is useful. OK, let's think about the situation where we need to make a decision during run time. For example, we want to display a name in an application. In one form we want to display the name with the combination of name+ surname and in another form we want to display surname+ name, in this fashion.  
  
So, how to display the name? Yes, we will make the decision at run time, as needed. Let's think of another scenario. In an automated mailing system we want to send mail to the proper person. When an error occurs, we will study the error and according to classification we would like to send one mail to the relevant department.  
  
If it is a software related error then the mail will go to the software division, if it is a hardware related issue then the mail will go to the hardware division. Now, we don't know which type of error will occur each time. We need to make the decision at run time after analysis of the error.  
  
**Let's implement strategy classes**  
  
Let's implement our first scenario, where we want to show the combination of name and surname depending on demand (runtime). We will create a Windows application to do that. So, let's create a Windows application and provide a nice name. Then add a .cs file and place the following code into it.  

1.  using System;  
2.  using System.Collections.Generic;  
3.  using System.Text;  
4.  namespace Statergy  
5.  {  
6.      public abstract class clsStatergy  
7.      {  
8.          public abstract  
9.              String Display(String Name, String Surname);  
10.      }  
11.      public class clsNameSurname : clsStatergy  
12.      {  
13.          public override String Display(String Name, String Surname)  
14.          {  
15.              return Name + Surname;  
16.          }  
17.      }  
18.      public class clssurnameName : clsStatergy  
19.      {  
20.          public override String Display(String Name, String Surname)  
21.          {  
22.              return Surname + Name;  
23.          }  
24.      }  
25.      public class clsDisplayName  
26.      {  
27.          public String Name = "";  
28.          public String Surname= "";  
29.          private clsStatergy objStatergy;  
30.          public void setStatergy(clsStatergy obj)  
31.          {  
32.              objStatergy = obj;  
33.          }  
34.          public String Show()  
35.          {  
36.              return objStatergy.Display(Name, Surname);  
37.          }  
38.      }  
39.  } 

Let's explain the code above .At first we created one abstract strategy class, it's nothing but to bring uniformity to the application. Then we have derived two types of display classes and have implemented (better to say overrode) the Display() function in each one of them.

Then another class called clsDisplayName will make the decision which displays the strategy needed to follow.  
  
**Implement client**  
  
Now it's time to implement the client section. Use one Windows Forms form and paste the following code into it.  

1.  using System;  
2.  using System.Collections.Generic;  
3.  using System.ComponentModel;  
4.  using System.Data;  
5.  using System.Drawing;  
6.  using System.Text;  
7.  using System.Windows.Forms;  
8.  using Statergy;  
9.  namespace WindowsStatergy  
10.  {  
11.      public partial class Form1 : Form  
12.      {  
13.          public Form1()  
14.          {  
15.              InitializeComponent();  
16.          }  
17.          private void btnNameSurname\_Click(object sender, EventArgs e)  
18.          {  
19.              clsDisplayName obj = new clsDisplayName();  
20.              obj.Name = txtNumber1.Text;  
21.              obj.Surname = txtNumber2.Text;  
22.              obj.setStatergy(new clsNameSurname());  
23.              txtResults.Text = obj.Show().ToString();  
24.          }  
25.          private void btnSurnameName\_Click(object sender, EventArgs e)  
26.          {  
27.              clsDisplayName obj = new clsDisplayName();  
28.              obj.Name = txtNumber1.Text;  
29.              obj.Surname = txtNumber2.Text;  
30.              obj.setStatergy(new clssurnameName());  
31.              txtResults.Text = obj.Show().ToString();  
32.          }  
33.      }  
34.  } 

Here is the design mode.

  
  
When the user clicks the Name+Surname button after giving the name and surname, it will show the proper combination. If the user needs the surname first then they will use the Surname+Name button.  
  
If we observe the button's click event of each button, we will find that we are creating an object during run time and the object is being set to the strategy class.  
  
Here is the working output of when we press the Name+Surname button.  
  
  
  
If we press Surname+Name then the output will show as in the following.  
  
  

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