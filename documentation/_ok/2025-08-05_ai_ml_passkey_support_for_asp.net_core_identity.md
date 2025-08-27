```yaml
---
title: Passkey support for ASP.NET Core identity
source: https://andrewlock.net/exploring-dotnet-10-preview-features-6-passkey-support-for-aspnetcore-identity/?utm_source=dotnetnews.beehiiv.com&utm_medium=newsletter&utm_campaign=the-net-news-daily-issue-259
date_published: 2025-08-05T13:00:00.000Z
date_captured: 2025-08-11T14:17:58.007Z
domain: andrewlock.net
author: Unknown
category: ai_ml
technologies: [.NET 10, ASP.NET Core, ASP.NET Core Identity, Blazor, FIDO, WebAuthn, Fido2 library, Entity Framework Core, 1Password, Windows Hello, Chrome, Windows]
programming_languages: [C#, JavaScript, SQL]
tags: [passkeys, security, authentication, aspnet-core, dotnet, blazor, identity, webauthn, preview-features, passwordless]
key_concepts: [Passkeys, WebAuthn, ASP.NET Core Identity, Passwordless authentication, Biometric authentication, FIDO Alliance, Custom Elements, API Endpoints]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores the new passkey support integrated into ASP.NET Core Identity within the .NET 10 preview 6, focusing on its implementation in the Blazor Web App template. It guides readers through the user experience of adding, managing, and utilizing passkeys for login, highlighting the current requirement for initial password registration. The post then delves into the underlying code changes, examining the client-side JavaScript for WebAuthn interactions and the server-side C# APIs that handle passkey creation and request options. It also touches upon the Entity Framework Core migration for storing passkey data, providing a comprehensive overview of this evolving security feature.
---
```

# Passkey support for ASP.NET Core identity

**Sponsored by** [**Dometrain Courses**](https://dometrain.com/dometrain-pro/?ref=andrew-lock&promo=banner&coupon_code=ANDREW30)—Get 30% off [**Dometrain Pro**](https://dometrain.com/dometrain-pro/?ref=andrew-lock&promo=banner&coupon_code=ANDREW30) with code [**ANDREW30**](https://dometrain.com/dometrain-pro/?ref=andrew-lock&promo=banner&coupon_code=ANDREW30) and access the best courses for .NET Developers

August 05, 2025 ~13 min read

*   [.NET 10](/tag/net-10/)
*   [ASP.NET Core](/tag/aspnet-core/)
*   [Security](/tag/security/)

# Passkey support for ASP.NET Core identity

[Exploring the .NET 10 preview - Part 6](/series/exploring-the-dotnet-10-preview/)

Share on:

*   [Facebook](https://www.facebook.com/sharer/sharer.php?u=https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-6-passkey-support-for-aspnetcore-identity%2F&t=Passkey+support+for+ASP.NET+Core+identity "Share on Facebook")
*   [](https://twitter.com/intent/tweet?source=https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-6-passkey-support-for-aspnetcore-identity%2F&text=Passkey+support+for+ASP.NET+Core+identity:https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-6-passkey-support-for-aspnetcore-identity%2F "Tweet")
*   [](https://twitter.com/intent/tweet?source=https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-6-passkey-support-for-aspnetcore-identity%2F&text=Passkey+support+for+ASP.NET+Core+identity:https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-6-passkey-support-for-aspnetcore-identity%2F "Share on Bluesky")
*   [](http://www.reddit.com/submit?url=https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-6-passkey-support-for-aspnetcore-identity%2F&title=Passkey+support+for+ASP.NET+Core+identity "Submit to Reddit")
*   [](http://www.linkedin.com/shareArticle?mini=true&url=https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-6-passkey-support-for-aspnetcore-identity%2F&title=Passkey+support+for+ASP.NET+Core+identity&source=https%3A%2F%2Fandrewlock.net%2Fexploring-dotnet-10-preview-features-6-passkey-support-for-aspnetcore-identity%2F "Share on LinkedIn")

This is the sixth post in the series: [Exploring the .NET 10 preview](/series/exploring-the-dotnet-10-preview/).

1.  [Part 1 - Exploring the features of dotnet run app.cs](/exploring-dotnet-10-preview-features-1-exploring-the-dotnet-run-app.cs/)
2.  [Part 2 - Behind the scenes of dotnet run app.cs](/exploring-dotnet-10-preview-features-2-behind-the-scenes-of-dotnet-run-app.cs/)
3.  [Part 3 - C# 14 extension members; AKA extension everything](/exploring-dotnet-10-preview-features-3-csharp-14-extensions-members/)
4.  [Part 4 - Solving the source generator 'marker attribute' problem in .NET 10](/exploring-dotnet-10-preview-features-4-solving-the-source-generator-marker-attribute-problem-in-dotnet-10/)
5.  [Part 5 - Running one-off .NET tools with dnx](/exploring-dotnet-10-preview-features-5-running-one-off-dotnet-tools-with-dnx/)
6.  Part 6 - Passkey support for ASP.NET Core identity (this post)

In this post I look at the passkey support added to ASP.NET Core Identity in .NET 10 preview 6. I primarily focus on the changes included in the Blazor template, looking at what's been added and changed as part of the passkey support. Finally, we take a peek at the source code of the template changes, to understand the new WebAuthn interactions with the browser.

> This post was written using the features available in .NET 10 preview 6. Many things may change between now and the final release of .NET 10.

## What are passkeys?

[Passkeys](https://fidoalliance.org/passkeys/) provide a secure, unphishable, password-less way to authenticate with websites and apps. They're based on standards provided by [FIDO (Fast IDentity Online)](https://fidoalliance.org/) and let you sign in to apps using the same mechanisms that you use to unlock your laptop or phone: such as your biometrics or a PIN. They're inherently more secure than passwords, though [they do have some usability challenges](https://arstechnica.com/security/2024/12/passkey-technology-is-elegant-but-its-most-definitely-not-usable-security/) when it comes to sharing your passkeys between multiple devices.

In .NET 10 preview 6, ASP.NET Core has added support for passkeys as an alternative way to login to apps that are using ASP.NET Core Identity. They don't completely replace passwords in the current templates, so you still need to register with a password initially, but you can add a passkey to your account subsequently for easier logging in.

> Personally, this seems like it fundamentally misses the point of passkeys. The whole point of passkeys in my eyes is that you _don't_ have passwords, so you can't be phished. Having an ever-present mandatory password seems to defeat half the purpose of passkeys.

In the next section I take a brief look at the default Blazor template with individual authentication enabled to see how it's changed from the .NET 9 version.

## Trying out the new template

Passkey support was added [in this giant PR](https://github.com/dotnet/aspnetcore/pull/62112), which added the new passkey abstractions to ASP.NET Core Identity and made the changes to the Blazor Web App template that we're looking at in this post. Before we get started, it's worth noting what "passkey supports" actually _means_. As noted in the PR description:

> Note that the goal of this PR is to add support for passkey authentication in ASP.NET Core Identity. While it implements core WebAuthn functionality, it does not provide a complete and general-purpose WebAuthn/FIDO2 library. The public API surface is limited in order to enable long-term stability of the feature. Targeted extensibility points were added to enable functionality not implemented by default, most notably attestation statement validation. This allows the use of third-party libraries to fill the missing gaps, when desired. Community feedback may result in additional extensibility APIs being added in the future.

If you want (or need) a more full-featured library, then you might want to consider [the Fido2 library](https://github.com/passwordless-lib/fido2-net-lib) which supports .NET 8 and above. You can also use this library in combination with the built-in passkey support to enable additional features such as attestation statement validation.

Before looking at the code, we'll start by creating a new web app with ASP.NET Core Identity, and explore what the new passkey support looks like in the UI.

> Passkey support was added in .NET 10 preview 6, so you need to be using at least this version of the SDK. If you're using a later version, then things may change from what I show here.

Create a new Blazor Web App with individual authentication:

```bash
dotnet new blazor -au Individual
```

You can run the application using `dotnet run` or by pressing F5 in your IDE, and you'll be greeted with the familiar Blazor web app. Navigate to the **Register** page and create a new user:

![The register page of an ASP.NET Core Blazor application, showing fields for email and password.](/content/images/2025/passkeys_02.png)

So far, there's no obvious difference. After creating your account, click the "Click here to confirm your account" link and then navigate to the login page.

> Note that in the default template, users must still create a password for the account, even if they later want to use passkeys. This isn't a requirement of ASP.NET Core Identity itself, just of how the template works.

After registering, login, and navigate to the account page. Here you'll find a new section, **Passkeys**, which allows you to register a passkey:

![The account management page in an ASP.NET Core Blazor application, displaying options like Profile, Email, Password, Two-factor authentication, Passkeys, and Personal data. The "Passkeys" section shows "No passkeys are registered" and an "Add a new passkey" button.](/content/images/2025/passkeys_03.png)

Click **Add a new passkey** to initiate the registration process. Clicking this button will pop up a native dialog from your browser. If you're using a password manager with passkey support, like [1Password](https://1password.com/), then it will likely prompt you to save your passkeys there. Otherwise you'll get a native popup from your browser with your available options:

![A Windows Security dialog titled "Making sure it's you", prompting the user to save a passkey to the device, with options for Face, PIN, or "Use another device" for authentication.](/content/images/2025/passkeys_04.png)

What this dialog looks like and which options are available to you will depend on the device you're using. In the case above I was using a Windows device with a Windows Hello camera.

> You'll notice there's also a **Use another device** option, which lets you use (for example) a nearby phone with biometric support to perform the authentication. You can read more about [cross-device sign-in here](https://www.passkeycentral.org/design-guidelines/optional-patterns/cross-device-sign-in).

Choose where to save your passkey, perform the necessary authentication, and you should see confirmation that the passkey is enrolled:

![A Windows Security dialog titled "Passkey saved", confirming that the passkey for "test@test.com" on "localhost" has been successfully saved.](/content/images/2025/passkeys_05.png)

Now that you've saved the passkey to your device, the Blazor app prompts you to choose a name for the passkey. Technically the passkey is already saved at this point (with the name "Unnamed passkey") but you should choose a more descriptive key name and click **Continue**:

![The account management page in the Blazor application, prompting the user to "Enter a name for your passkey" with an input field pre-filled with "My Laptop Passkey" and a "Continue" button.](/content/images/2025/passkeys_07.png)

Now that your passkey is enrolled you can enrol another passkey, rename an existing key, or delete the key from the passkeys page:

![The passkey management page showing an enrolled passkey named "My Laptop Passkey", with options to "Add a new passkey", "Rename", and "Delete".](/content/images/2025/passkeys_08.png)

Next we'll try-out the login flow. Logout of your account, and on the login page don't enter your username and password. Instead, click the **Log in with a passkey** link:

![The login page of the ASP.NET Core Blazor application, featuring fields for email and password, and a prominent "Log in with a passkey" link.](/content/images/2025/passkeys_01.png)

When you click this link, the browser will generally pop-up a window prompting you to choose a passkey to use to login with:

![A Chrome browser dialog on Windows titled "Use a saved passkey", prompting the user to choose a passkey to sign in, showing "test@test.com" for "localhost".](/content/images/2025/passkeys_06.png)

After choosing the saved passkey, you'll be prompted to authenticate with your device using a native prompt. In my case this involved another face-recognition Windows Hello authentication, but it will vary by device. After authenticating, you're immediately logged in to the website, without needing to enter a username and password.

> As a small bonus, if you want to delete the passkeys saved on your Windows device, for example after testing with a sample app, go to the Windows Settings app and choose Accounts > Passkeys (or click [this link](ms-settings:savedpasskeys)). From there you can delete your old passkeys (but make sure you don't delete any you actually need!)

That pretty much covers all the user-facing changes to support passkeys in the new template. There's no ability to use passkeys as an additional factor for multi-factor authentication, or to remove the password associated with the account entirely.

To finish off this post we'll look at some of the code changes behind the passkey support, focusing on the parts that interact with the ASP.NET Core Identity system and the browser.

## Looking at the code changes

All the code I show in this section is part of the template when you create a new Blazor Web App template using .NET 10 preview 6. There were also changes made to the ASP.NET Core Identity system to support the template additions, but I don't go into those here.

> Note that the code shown here is specifically for .NET 10 preview 6. [This code has already been updated](https://github.com/dotnet/aspnetcore/pull/62530) for newer previews, and will likely change again before final GA release, so take it all with a pinch of salt!

On the UI side, the most important new component is _Components/Account/Shared/PasskeySubmit.razor_ and its corresponding collocated JavaScript file _PasskeySubmit.razor.js_. The JavaScript in particular contains all the functions for calling the browser's WebAuthn features for interacting with passkeys. We'll look at this file in detail shortly.

Aside from the `PasskeySubmit` component, there are several new and updated components:

*   _Components/Account/Pages/Login.razor_—Updated to include the "log in with a passkey" link.
*   _Components/Account/Shared/ManageNavMenu.razor_—Updated to include the "Passkeys" menu item.
*   _Components/Account/Manage/Passkeys.razor_—The passkey management page for adding and deleting passkeys.
*   _Components/Account/Manage/RenamePasskey.razor_—The page for renaming passkeys.

On the backend of the application there are two main changes:

*   New APIs in `IdentityComponentsEndpointRouteBuilderExtensions` called by the Blazor components for interacting with ASP.NET Core Identity
*   A new EF Core migration for saving a user's passkey information to the database.

That covers pretty much all of the public-facing changes in the templates, so let's look at each of them in more detail.

We'll start with the `PasskeySubmit` component, the markup for which is shown below:

```html
<button type="submit" name="__passkeySubmit" @attributes="AdditionalAttributes">@ChildContent</button>
<passkey-submit operation="@Operation" name="@Name" email-name="@EmailName"></passkey-submit>
```

The component itself is pretty simple, just a form submit button and a custom element called `passkey-submit`. If we take a look in `PasskeySubmit.razor.js` we can see how this custom-element is wired up. The outline of this is shown below, along with some bonus comments explaining various API calls:

```javascript
// register a custom element definition
customElements.define('passkey-submit', class extends HTMLElement {
    static formAssociated = true;

    // connectedCallback fires when an element is inserted into the DOM
    connectedCallback() {
        // attaches the custom-element to a form
        this.internals = this.attachInternals();
        // grab the details passed as attributes to the element
        this.attrs = {
            operation: this.getAttribute('operation'),
            name: this.getAttribute('name'),
            emailName: this.getAttribute('email-name'),
        };

        // Register a submit handler on the form, and if it was triggered
        // by the __passkeySubmit button then try to submit a Passkey credential
        this.internals.form.addEventListener('submit', (event) => {
            if (event.submitter?.name === '__passkeySubmit') {
                event.preventDefault();
                // get or create a passkey credential and submit the form
                this.obtainCredentialAndSubmit();
            }
        });

        // try to auto-fill the passkey, to improve the user experience
        this.tryAutofillPasskey();
    }

    // disconnectedCallback fires when an element is removed from the DOM
    disconnectedCallback() {
        this.abortController?.abort();
    }

    async tryAutofillPasskey() {
        if (this.attrs.operation === 'Request' && await PublicKeyCredential.isConditionalMediationAvailable()) {
            // If the component is in 'request' mode (i.e. login), 
            // and autofill is available and supported in the browser
            // then try to pre-autofill
            await this.obtainCredentialAndSubmit(/* useConditionalMediation */ true);
        }
    }

    async obtainCredentialAndSubmit(useConditionalMediation = false) {
        // AbortController works similarly to a CancelationToken in .NET
        this.abortController?.abort();
        this.abortController = new AbortController();
        const signal = this.abortController.signal;
        const formData = new FormData();
        try {
            let credential;
            // Either create a new credential or request an existing one
            if (this.attrs.operation === 'Create') {
                credential = await createCredential(signal);
            } else if (this.attrs.operation === 'Request') {
                const email = new FormData(this.internals.form).get(this.attrs.emailName);
                const mediation = useConditionalMediation ? 'conditional' : undefined;
                credential = await requestCredential(email, mediation, signal);
            } else {
                throw new Error(`Unknown passkey operation '${operation}'.`);
            }

            // convert the credential to JSON and store it in the form data
            const credentialJson = JSON.stringify(credential);
            formData.append(`${this.attrs.name}.CredentialJson`, credentialJson);
        } catch (error) {
            if (error.name === 'AbortError') {
                // Canceled by user action, do not submit the form
                return;
            }
            formData.append(`${this.attrs.name}.Error`, error.message);
            console.error(error);
        }

        // Set the form data and submit it
        this.internals.setFormValue(formData);
        this.internals.form.submit();
    }
});
```

This code shows all the behaviour added to the `passkey-submit` element. We're just missing the definition of two functions: `createCredential()` and `requestCredential()`, shown below:

```javascript
// Called to create a new passkey
async function createCredential(signal) {
    // Call the ASP.NET Core Identity endpoint to get the passkey options for the app
    const optionsResponse = await fetchWithErrorHandling('/Account/PasskeyCreationOptions', {
        method: 'POST',
        signal,
    });

    // Convert the response to a passkey options JSON object
    const optionsJson = await optionsResponse.json();
    const options = PublicKeyCredential.parseCreationOptionsFromJSON(optionsJson);

    // Trigger the browser to create a passkey credential using 
    // the provided options and return the credentials
    return await navigator.credentials.create({ publicKey: options, signal });
}

// Called to trigger a login using a passkey
async function requestCredential(email, mediation, signal) {
    // Call the ASP.NET Core Identity endpoint to get the passkey options for the app
    const optionsResponse = await fetchWithErrorHandling(`/Account/PasskeyRequestOptions?username=${email}`, {
        method: 'POST',
        signal,
    });

    // Convert the response to a passkey options JSON object
    const optionsJson = await optionsResponse.json();
    const options = PublicKeyCredential.parseRequestOptionsFromJSON(optionsJson);

    // Trigger the browser to try to login to a passkey credential using
    // the provided options and return the credentials
    return await navigator.credentials.get({ publicKey: options, mediation, signal });
}

// Helper function for sending an HTTP request and returning the response
async function fetchWithErrorHandling(url, options = {}) {
    const response = await fetch(url, {
        credentials: 'include',
        ...options
    });
    if (!response.ok) {
        const text = await response.text();
        console.error(text);
        throw new Error(`The server responded with status ${response.status}.`);
    }
    return response;
}
```

These functions make calls to 2 API endpoints, exposed in `IdentityComponentsEndpointRouteBuilderExtensions`. The first is `/Account/PasskeyCreationOptions`, which is called when you're adding a passkey to an existing logged-in user's account:

```csharp
internal static class IdentityComponentsEndpointRouteBuilderExtensions
{
    public static IEndpointConventionBuilder MapAdditionalIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var accountGroup = endpoints.MapGroup("/Account");
        // ...
        
        accountGroup.MapPost("/PasskeyCreationOptions", async (
            HttpContext context,
            [FromServices] UserManager<ApplicationUser> userManager,
            [FromServices] SignInManager<ApplicationUser> signInManager) =>
        {
            var user = await userManager.GetUserAsync(context.User);
            if (user is null)
            {
                return Results.NotFound($"Unable to load user with ID '{userManager.GetUserId(context.User)}'.");
            }

            // Collect current user's details to create a PasskeyCreationArgs object
            var userId = await userManager.GetUserIdAsync(user);
            var userName = await userManager.GetUserNameAsync(user) ?? "User";
            var userEntity = new PasskeyUserEntity(userId, userName, displayName: userName);
            var passkeyCreationArgs = new PasskeyCreationArgs(userEntity);

            // Use the arguments to create the passkey options object
            // and return it as JSON, for use client-side
            var options = await signInManager.ConfigurePasskeyCreationOptionsAsync(passkeyCreationArgs);
            return TypedResults.Content(options.AsJson(), contentType: "application/json");
        });

        //...
    }
}
```

The `SignInManager.ConfigurePasskeyCreationOptionsAsync()` method is where all the actual work occurs (renamed to `MakePasskeyCreationOptionsAsync` in a future .NET 10 release). This method is responsible for generating the passkey options, storing them in an authentication cookie, and returning the JSON. For reference, the returned JSON will look something like this (I removed most of the `pubKeyCredParams` options for brevity):

```json
{
    "rp": {
        "name": "localhost",
        "id": "localhost"
    },
    "user": {
        "id": "OWVhMDBjMDUtYjU4LThmODEtMWNlNWNihmYS00NWMmRlNjdi",
        "name": "test@test.com",
        "displayName": "test@test.com"
    },
    "challenge": "4ZIzlOlk9bTwB4veQVQc9w",
    "pubKeyCredParams": [
        {
            "type": "public-key",
            "alg": -7
        },
        {
            "type": "public-key",
            "alg": -37
        }
    ],
    "timeout": 60000,
    "excludeCredentials": [],
    "hints": [],
    "attestation": "none",
    "attestationFormats": []
}
```

These options are returned to the browser and are used to generate the passkey client-side.

The other API endpoint, `PasskeyRequestOptions` is almost identical, though as this is intended for logging-in, there's no authenticated user _required_ at this point (though if you enter your username first, it can be used to improve the UX of choosing a passkey).

```csharp
accountGroup.MapPost("/PasskeyRequestOptions", async (
    [FromServices] UserManager<ApplicationUser> userManager,
    [FromServices] SignInManager<ApplicationUser> signInManager,
    [FromQuery] string? username) =>
{
    var user = string.IsNullOrEmpty(username) ? null : await userManager.FindByNameAsync(username);
    var passkeyRequestArgs = new PasskeyRequestArgs<ApplicationUser>
    {
        User = user,
    };
    var options = await signInManager.ConfigurePasskeyRequestOptionsAsync(passkeyRequestArgs);
    return TypedResults.Content(options.AsJson(), contentType: "application/json");
});
```

Note that these options are used both during passkey creation and login with the `PasskeySubmit` component, but the _results_ of that operation, i.e. the credential created or retrieved from the browser, are just stored in a form field and submitted. The _handling_ of that data happens in the `Passkeys` and `Login` components.

The `Passkeys` component contains markup similar to the following, which places the `PasskeySubmit` component inside a form, and hooks up the `AddPasskey()` handler:

```cshtml
<form @formname="add-passkey" @onsubmit="AddPasskey" method="post">
    <AntiforgeryToken />
    <PasskeySubmit Operation="PasskeyOperation.Create" Name="Input" class="btn btn-primary">Add a new passkey</PasskeySubmit>
</form>
```

As you've already seen, the `PasskeySubmit` component, handles the registration of a passkey client-side, and then stores the details about the passkey in the surrounding form. The `AddPasskey()` method must then use this form data to actually save and persist the passkey details. This method is shown below with comments (with some error handling elided for brevity):

```csharp
private async Task AddPasskey()
{
    //