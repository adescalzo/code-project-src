# EP 57 : How can you make your password more secure?

```markdown
**Source:** https://mwaseemzakir.substack.com/p/ep-57-how-can-you-make-your-password?utm_source=post-email-title&publication_id=1232416&post_id=144019282&utm_campaign=email-post-title&isFreemail=true&r=a97lu&triedRedirect=true
**Date Captured:** 2025-07-28T16:25:56.890Z
**Domain:** mwaseemzakir.substack.com
**Author:** Muhammad Waseem
**Category:** ai_ml
```

---

#### Share this post

[

![](https://substackcdn.com/image/fetch/$s_!YpVb!,w_520,h_272,c_fill,f_auto,q_auto:good,fl_progressive:steep,g_auto/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2Fa5c4652e-d95a-424b-a3a1-5b4c5cc22a3f_1170x554.png)

![.NET Weekly Newsletter](https://substackcdn.com/image/fetch/$s_!wfyx!,w_36,h_36,c_fill,f_auto,q_auto:good,fl_progressive:steep,g_auto/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2F4268d508-d10e-401a-be77-86e8a51d9474_256x256.png)

.NET Weekly Newsletter

EP 57 : How can you make your password more secure?









](#)

Copy link

Facebook

Email

Notes

More

# EP 57 : How can you make your password more secure?

### Read Time : 3 Mins

[

![Muhammad Waseem's avatar](https://substackcdn.com/image/fetch/$s_!IVD2!,w_36,h_36,c_fill,f_auto,q_auto:good,fl_progressive:steep/https%3A%2F%2Fbucketeer-e05bbc84-baa3-437e-9518-adb32be77984.s3.amazonaws.com%2Fpublic%2Fimages%2Fcb4892b1-9b75-48c0-abc7-f4974714b04d_256x256.jpeg)



](https://substack.com/@mwaseemzakir)

[Muhammad Waseem](https://substack.com/@mwaseemzakir)

Apr 27, 2024

6

#### Share this post

[

![](https://substackcdn.com/image/fetch/$s_!YpVb!,w_520,h_272,c_fill,f_auto,q_auto:good,fl_progressive:steep,g_auto/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2Fa5c4652e-d95a-424b-a3a1-5b4c5cc22a3f_1170x554.png)

![.NET Weekly Newsletter](https://substackcdn.com/image/fetch/$s_!wfyx!,w_36,h_36,c_fill,f_auto,q_auto:good,fl_progressive:steep,g_auto/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2F4268d508-d10e-401a-be77-86e8a51d9474_256x256.png)

.NET Weekly Newsletter

EP 57 : How can you make your password more secure?









](#)

Copy link

Facebook

Email

Notes

More

[](https://mwaseemzakir.substack.com/p/ep-57-how-can-you-make-your-password/comments)

[

Share

](javascript:void\(0\))

**[Sponsor this newsletter](https://mwaseemzakir.substack.com/p/waseems-net-newsletter-sponsorship)**

---

Security is the most important yet neglected part of our development. Security should be our top consideration from the coding phase of any application.

Today, I will share how we can create almost unbreakable passwords.

We have the following approaches to save passwords:

1.  Plain text password (Not effective)
    
2.  Text password but encoded (Not effective)
    
3.  Hash {Salt + password} (Effective)
    
4.  Hash {Salt + Peeper + Password} ( Most effective)
    

### What is the problem with the first two approaches?

The first two types of passwords could be victims of brute-force attacks following dictionary attacks with a combination of common hashing algorithms.

### What is Salt and how it works?

Salt is nothing else than a random string. At the time of user creation, we generate a random string and save it along with our user's other information in the user’s table.

One can also use GUID as a SALT, but using a random generator is more common.

The following code can be used to generate random salt in C# :

[

![](https://substackcdn.com/image/fetch/$s_!YpVb!,w_1456,c_limit,f_auto,q_auto:good,fl_progressive:steep/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2Fa5c4652e-d95a-424b-a3a1-5b4c5cc22a3f_1170x554.png)



](https://substackcdn.com/image/fetch/$s_!YpVb!,f_auto,q_auto:good,fl_progressive:steep/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2Fa5c4652e-d95a-424b-a3a1-5b4c5cc22a3f_1170x554.png)

→ On user creation, we generate a SALT and then encode our password with this SALT.

1.  SalatedPassword = Password + Salt
    
2.  Then we convert it to bytes
    
3.  And hash it using a string hashing algorithm (e.g. SHA256, SHA512)
    

→ On-user retrieval

1.  Retrieve user based on email or username
    
2.  Get its salt, combine it with the password coming in a request
    
3.  Hash it using the same algorithm used earlier
    
4.  If the password coming from the database matches this hashed password you are good to go
    

This approach is effective but what’s the problem with adding an extra layer that will hardly take a couple of lines of code and a few minutes but that is +1 more secure than the salt approach

### What is Peeper and how does it work with Salt and Password?

Once again peeper is a fancy term nothing else than a string. We create a random key, you can use online tools as well that generate random strong strings.

Save that sting somewhere secure e.g. (Environment variables or user secrets) as per your environment suitability ( Development, Staging, or Production).

It comes with an extra security effort of keeping your peeper in a safe place.

On user creation generate salt and then:

1.  SaltPeeperPassword = Password + Salt + Peeper
    
2.  Then we convert it to bytes
    
3.  And hash it using a string hashing algorithm (e.g. SHA256, SHA512)
    

Choose the hashing algorithm of your choice by exploring them

On-user retrieval

1.  Retrieve user based on email or username
    
2.  Get salt, and pepper and combine with the password
    
3.  Hash it using the same algorithm
    
4.  If the password matches you are good to go
    

### (Salt + Peeper + Password) with N iterations for hashing

In this approach, we iterate the hashing process up to n time e.g. three, four, or five times, and save the final version of the password in the database. This approach is the most secure.

Make sure you use the same n for saving this password and then retrieving it. One can save iterations count in app settings and retrieve it as needed.

The hashing code can be found at [Microsft Docs](https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.hmacsha256?view=net-8.0), the main idea was to explain the concept of the Salt and pepper approach.

---

## **Whenever you’re ready, there are 2 ways I can help you:**

1.  **[Promote yourself to 10000+ subscribers](https://mwaseemzakir.substack.com/p/waseems-net-newsletter-sponsorship)** by sponsoring this newsletter
    
2.  **[Patreon Community:](https://www.patreon.com/mwaseemzakir)** Join and gain access to the 200+ articles I have published so far in one place. **[Join here](https://www.patreon.com/mwaseemzakir)**
    

---

#### _Why are you knocking at every door? Go, knock at the door of your own heart - Rumi_

---

#### Subscribe to .NET Weekly Newsletter

By Muhammad Waseem · Launched 3 years ago

Boost your .NET skills to next level with a quick read under 5 minutes every Saturday

Subscribe

By subscribing, I agree to Substack's [Terms of Use](https://substack.com/tos), and acknowledge its [Information Collection Notice](https://substack.com/ccpa#personal-data-collected) and [Privacy Policy](https://substack.com/privacy).

[

![Pieter's avatar](https://substackcdn.com/image/fetch/$s_!ryS2!,w_32,h_32,c_fill,f_auto,q_auto:good,fl_progressive:steep/https%3A%2F%2Fsubstack.com%2Fimg%2Favatars%2Fyellow.png)



](https://substack.com/profile/219018866-pieter)

[

![Arif Mohammad's avatar](https://substackcdn.com/image/fetch/$s_!x49C!,w_32,h_32,c_fill,f_auto,q_auto:good,fl_progressive:steep/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2Ff045d145-94e2-4d88-a613-917c60408cb7_144x144.png)



](https://substack.com/profile/17273128-arif-mohammad)

[

![jayamoorthi parasuraman's avatar](https://substackcdn.com/image/fetch/$s_!XaM5!,w_32,h_32,c_fill,f_auto,q_auto:good,fl_progressive:steep/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2F8c94beff-f4ae-4a95-a447-c01230efba88_144x144.png)



](https://substack.com/profile/139481737-jayamoorthi-parasuraman)

[

![Gianluca Gagliano's avatar](https://substackcdn.com/image/fetch/$s_!PyAt!,w_32,h_32,c_fill,f_auto,q_auto:good,fl_progressive:steep/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2Ffb032b58-1e10-4f7a-a235-d8173e106943_96x96.jpeg)



](https://substack.com/profile/17128357-gianluca-gagliano)

6 Likes

[](https://substack.com/note/p-144019282/restacks?utm_source=substack&utm_content=facepile-restacks)

6

#### Share this post

[

![](https://substackcdn.com/image/fetch/$s_!YpVb!,w_520,h_272,c_fill,f_auto,q_auto:good,fl_progressive:steep,g_auto/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2Fa5c4652e-d95a-424b-a3a1-5b4c5cc22a3f_1170x554.png)

![.NET Weekly Newsletter](https://substackcdn.com/image/fetch/$s_!wfyx!,w_36,h_36,c_fill,f_auto,q_auto:good,fl_progressive:steep,g_auto/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2F4268d508-d10e-401a-be77-86e8a51d9474_256x256.png)

.NET Weekly Newsletter

EP 57 : How can you make your password more secure?









](#)

Copy link

Facebook

Email

Notes

More

[](https://mwaseemzakir.substack.com/p/ep-57-how-can-you-make-your-password/comments)

[

Share

](javascript:void\(0\))

PreviousNext

#### Discussion about this post

CommentsRestacks

![User's avatar](https://substackcdn.com/image/fetch/$s_!owWd!,w_32,h_32,c_fill,f_auto,q_auto:good,fl_progressive:steep/https%3A%2F%2Fsubstack.com%2Fimg%2Favatars%2Fdefault-dark.png)

TopLatestDiscussions

[EP 26 : 75+ Resources to boost your .NET skills](https://mwaseemzakir.substack.com/p/ep-26-75-resources-to-boost-your)

[Read Time : 4 Mins](https://mwaseemzakir.substack.com/p/ep-26-75-resources-to-boost-your)

Aug 19, 2023 • 

[Muhammad Waseem](https://substack.com/@mwaseemzakir)

22

#### Share this post

[

![](https://substackcdn.com/image/fetch/$s_!KPZa!,w_520,h_272,c_fill,f_auto,q_auto:good,fl_progressive:steep,g_auto/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2Fa196c3f3-4748-40f9-8bf8-8a4963386bc9_1147x739.png)

![.NET Weekly Newsletter](https://substackcdn.com/image/fetch/$s_!wfyx!,w_36,h_36,c_fill,f_auto,q_auto:good,fl_progressive:steep,g_auto/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2F4268d508-d10e-401a-be77-86e8a51d9474_256x256.png)

.NET Weekly Newsletter

EP 26 : 75+ Resources to boost your .NET skills









](#)

Copy link

Facebook

Email

Notes

More

[

1

](https://mwaseemzakir.substack.com/p/ep-26-75-resources-to-boost-your/comments)

[](javascript:void\(0\))

[Top 18 Essential .NET Libraries for Developers](https://mwaseemzakir.substack.com/p/top-15-essential-net-libraries-for)

[Read Time : 4 Mins](https://mwaseemzakir.substack.com/p/top-15-essential-net-libraries-for)

Oct 5, 2024 • 

[Muhammad Waseem](https://substack.com/@mwaseemzakir)

25

#### Share this post

[

![](https://substackcdn.com/image/fetch/$s_!_4tu!,w_520,h_272,c_fill,f_auto,q_auto:good,fl_progressive:steep,g_auto/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2Fcaf2284f-7a49-494f-a58e-94a7d7cc43d2_1200x600.png)

![.NET Weekly Newsletter](https://substackcdn.com/image/fetch/$s_!wfyx!,w_36,h_36,c_fill,f_auto,q_auto:good,fl_progressive:steep,g_auto/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2F4268d508-d10e-401a-be77-86e8a51d9474_256x256.png)

.NET Weekly Newsletter

Top 18 Essential .NET Libraries for Developers









](#)

Copy link

Facebook

Email

Notes

More

[](https://mwaseemzakir.substack.com/p/top-15-essential-net-libraries-for/comments)

[](javascript:void\(0\))

[EP 16 : How to secure your endpoints using JWT in .NET 6 ?](https://mwaseemzakir.substack.com/p/ep-16-how-to-secure-your-endpoints)

[Read Time : 3 Mins](https://mwaseemzakir.substack.com/p/ep-16-how-to-secure-your-endpoints)

Jun 10, 2023 • 

[Muhammad Waseem](https://substack.com/@mwaseemzakir)

18

#### Share this post

[

![](https://substackcdn.com/image/fetch/$s_!PdfJ!,w_520,h_272,c_fill,f_auto,q_auto:good,fl_progressive:steep,g_auto/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2Fcafcdc71-c705-427e-8d76-e15443ab944c_1195x624.png)

![.NET Weekly Newsletter](https://substackcdn.com/image/fetch/$s_!wfyx!,w_36,h_36,c_fill,f_auto,q_auto:good,fl_progressive:steep,g_auto/https%3A%2F%2Fsubstack-post-media.s3.amazonaws.com%2Fpublic%2Fimages%2F4268d508-d10e-401a-be77-86e8a51d9474_256x256.png)

.NET Weekly Newsletter

EP 16 : How to secure your endpoints using JWT in .NET 6 ?









](#)

Copy link

Facebook

Email

Notes

More

[

4

](https://mwaseemzakir.substack.com/p/ep-16-how-to-secure-your-endpoints/comments)

[](javascript:void\(0\))

See all

Ready for more?

Subscribe