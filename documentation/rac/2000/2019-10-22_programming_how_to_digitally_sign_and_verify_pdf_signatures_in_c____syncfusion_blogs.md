```yaml
---
title: "How to Digitally Sign and Verify PDF Signatures in C# | Syncfusion Blogs"
source: https://www.syncfusion.com/blogs/post/sign-verify-pdf-signatures-in-csharp
date_published: 2019-10-22T11:00:10.000Z
date_captured: 2025-09-09T14:53:39.489Z
domain: www.syncfusion.com
author: Praveenkumar
category: programming
technologies: [Syncfusion PDF Library, .NET, ASP.NET Core, ASP.NET MVC, Adobe Acrobat Reader, Cloudflare, GitHub, Google reCAPTCHA, Google Analytics, Google Tag Manager, Microsoft Clarity, Bing Ads, PayPal, Stripe, YouTube, LinkedIn, Reddit, X (Twitter), OptinMonster, Hotjar, WordPress, PHP, Blazor, React, Angular, JavaScript, .NET MAUI, BoldSign, BoldDesk, Bold BI, Bold Reports, Essential Studio, Code Studio, DigiSign, Hardware Security Module (HSM), USB token, Smart card, Windows Certificate Store]
programming_languages: [C#, VB.NET, JavaScript, PHP]
tags: [pdf, digital-signature, csharp, dotnet, document-security, long-term-validation, eidas, pades, certificate-management, hashing]
key_concepts: [Digital Signatures (PDF), Document Integrity, Authenticity Verification, Cryptographic Standards, Hashing Algorithms, Long-Term Validation (LTV), Long-Term Archive (LTA), Certificate Management]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article provides a comprehensive guide on creating, customizing, and validating PDF digital signatures using the Syncfusion PDF Library in C#. It covers essential features such as applying basic digital signatures, customizing their visual appearance, and dynamically updating validation indicators. The guide also delves into advanced topics like implementing CAdES standards, using various hashing algorithms, adding multiple signatures, and integrating with the Windows Certificate Store. Furthermore, it explains how to enable Long-Term Validation (LTV) and Long-Term Archive (LTA) timestamps, retrieve signature information, and programmatically remove signatures, ensuring robust document security and compliance. Practical C# code examples are provided for each feature, making it a valuable resource for developers working with secure PDF workflows.]
---
```

# How to Digitally Sign and Verify PDF Signatures in C# | Syncfusion Blogs

# How to Digitally Sign and Verify PDF Signatures in C#

![Digitally sign and verify PDF documents](https://www.syncfusion.com/blogs/wp-content/uploads/2019/10/Digitally-sign-and-verify-PDF-documents.jpg)

**TL;DR:** Learn how to create, customize, and validate PDF digital signatures in C# using Syncfusion PDF Library. This guide covers signing PDFs, adding custom appearances, using CAdES, enabling Long-Term Validation (LTV), and more with practical code examples to ensure document security and compliance.

The [Syncfusion PDF Library](https://www.syncfusion.com/pdf-framework/net/pdf-library ".NET PDF Library") is a powerful and feature-rich **.NET PDF library** that enables developers to **create, apply, and validate digital signatures in PDF documents** using **C# and VB.NET**. Whether you’re building secure document workflows or ensuring compliance, this library provides all the tools you need for robust digital signature handling.

## What is a PDF digital signature?

A **PDF digital signature** is a cryptographic mechanism used to:

*   **Ensure document integrity:** Confirms that the document has not been altered after signing.
*   **Verify authenticity:** Validates the identity of the signer.
*   **Enable non-repudiation:** Prevents the signer from denying their signature.

## Key features covered in this guide

This comprehensive guide walks you through various ways to digitally sign and verify PDF signatures in C# using Syncfusion’s PDF Library:

*   [Create PDF digital signatures](#_Create_PDF_digital)
*   [Customize signature appearance](#_Customize_signature_appearance)
*   [Dynamically change PDF signature appearance based on validation](#_Dynamically_change_the)
*   [Use CAdES and different hashing algorithms](#_Use_CAdES_and)
*   [Add multiple signatures to a single PDF](#_Add_multiple_signatures)
*   [Sign PDFs using Windows Certificate Store](#_Sign_PDFs_using)
*   [Add author or certifying signatures to PDF](#_Add_author_or)
*   [Apply external digital signatures to PDF](#_Apply_external_digital)
*   [Sign existing signature fields in a PDF](#_Sign_existing_signature)
*   [Add timestamps to PDF digital signatures](#_Add_timestamps_to)
*   [Enable Long-Term Validation (LTV) for PDF digital signatures](#_Enable_Long-Term_Validation)
*   [Implement Long-Term Archive Timestamps (LTA) in PDFs](#_Implement_Long-Term_Archive)
*   [Retrieve digital signature information from PDFs](#_Retrieve_digital_signature)
*   [Remove existing digital signatures from PDF](#_Remove_existing_digital)
*   [Validate PDF digital signatures](#_Validate_PDF_Digital)

## Create PDF digital signatures

To **digitally sign a PDF document in C#**, you need a **digital ID**, which includes a **private key** and a **certificate with a public key**. You can generate a **self-signed digital ID** using tools like [Adobe Acrobat Reader](https://helpx.adobe.com/acrobat/using/digital-ids.html#create_a_self_signed_digital_id "Digital ID overview").

With the [Syncfusion .NET PDF Library](https://www.syncfusion.com/document-processing/pdf-framework/net/pdf-library ".NET PDF Library"), you can easily apply digital signatures to existing PDF files. Here’s a step-by-step guide:

1.  **Load the existing PDF document** you want to sign.
2.  **Import the digital ID** (PFX file) using the associated password.
3.  **Create and apply digital signature** using the loaded digital ID.
4.  **Save the signed PDF document**.

The following C# code example demonstrates how to digitally sign a PDF using Syncfusion PDF Library.

```csharp
using Syncfusion.Pdf.Parsing;
using Syncfusion.Pdf.Security;
class Program
{
    static void Main(string[] args)
    {

        //Load existing PDF document.
        PdfLoadedDocument document = new PdfLoadedDocument("PDF_Succinctly.pdf");

        //Load digital ID with password.
        PdfCertificate certificate = new PdfCertificate(@"DigitalSignatureTest.pfx", "DigitalPass123");

        //Create a signature with loaded digital ID.
        PdfSignature signature = new PdfSignature(document, document.Pages[0], certificate, "DigitalSignature");

        //Save the PDF document.
        document.Save("SignedDocument.pdf");

        //Close the document.
        document. Close(true);
    }
}
```

By executing this code example, you will get a PDF document similar to the following screenshot.

![Screenshot of a PDF signed with a basic digital signature in Adobe Acrobat Reader, showing a "Signed and all signatures are valid" banner and signature details in the right panel.](https://www.syncfusion.com/blogs/wp-content/uploads/2019/10/Digitally-signed-PDF-document-Syncfusion-PDF-Library.png)

To get a valid green tick in your Adobe Acrobat Reader, as seen in the previous screenshot, you will have to [register](https://helpx.adobe.com/acrobat/using/digital-ids.html#register_a_digital_id "Digital ID overview") the self-signed digital ID in a trusted source.

Otherwise, to get a valid signature in any Adobe Acrobat Reader, your digital ID should be an [AATL-enabled signing credential](https://helpx.adobe.com/acrobat/kb/approved-trust-list2.html#HowdoIgetanAATLenabledsigningcredential "Adobe Approved Trust List").

## Customize signature appearance

Creating a custom appearance for PDF digital signatures helps users visually identify and verify signatures directly on the PDF page. With the [Syncfusion .NET PDF Library](https://www.syncfusion.com/document-processing/pdf-framework/net/pdf-library ".NET PDF Library"), you can easily design and apply a personalized signature appearance using text, images, or shapes.

A **visible digital signature** enhances document clarity and trust by displaying signer details, logos, or handwritten signatures. This is especially useful for legal, financial, and official documents.

To create a custom signature appearance:

1.  **Set the signature bounds** to define its position and size on the PDF page.
2.  Use the appearance property of the **PdfSignature** class to draw:
    *   Custom text (e.g., signer name, date)
    *   Images (e.g., company logo, handwritten signature)
    *   Shapes or graphics

```csharp
//Load existing PDF document.
using (PdfLoadedDocument document = new PdfLoadedDocument(@"../../Data/PDF_Succinctly.pdf"))
{
    //Load digital ID with password.
    PdfCertificate certificate = new PdfCertificate(@"../../Data/DigitalSignatureTest.pfx", "DigitalPass123");

    //Create a signature with loaded digital ID.
    PdfSignature signature = new PdfSignature(document, document.Pages[0], certificate, "DigitalSignature");
    
    //Set bounds to the signature.
    signature.Bounds = new System.Drawing.RectangleF(40, 40, 350, 100);

    //Load image from file.
    PdfBitmap image = new PdfBitmap(@"../../Data/signature.png");

    //Create a font to draw text.
    PdfStandardFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 15);

    //Drawing text, shape, and image into the signature appearance.
    signature.Appearance.Normal.Graphics.DrawRectangle(PdfPens.Black, PdfBrushes.White, new System.Drawing.RectangleF(0, 0, 350, 100));
    signature.Appearance.Normal.Graphics.DrawImage(image, 0, 0, 100, 100);
    signature.Appearance.Normal.Graphics.DrawString("Digitally Signed by Syncfusion", font, PdfBrushes.Black, 120, 17);
    signature.Appearance.Normal.Graphics.DrawString("Reason: Testing signature", font, PdfBrushes.Black, 120, 39);
    signature.Appearance.Normal.Graphics.DrawString("Location: USA", font, PdfBrushes.Black, 120, 60);

    //Save the PDF document.
    document.Save("SignedAppearance.pdf");
}
```

By executing this code example, you will get a PDF document similar to the following screenshot.

![Screenshot of a PDF with a customized digital signature appearance, including a company logo, "Digitally Signed by Syncfusion", "Reason: Testing signature", and "Location: USA".](https://www.syncfusion.com/blogs/wp-content/uploads/2019/10/Appearance-customized-in-PDF-digital-signature-Syncfusion-PDF-Library.png)

## Dynamically change the PDF signature appearance based on validation

With the [Syncfusion .NET PDF Library](https://www.syncfusion.com/document-processing/pdf-framework/net/pdf-library ".NET PDF Library"), you can enhance the visibility and trustworthiness of digital signatures by enabling **dynamic signature validation appearance**. This feature visually reflects the **validation status** of a digital signature when the PDF is opened in a reader like Adobe Acrobat.

To enable this feature, simply set the [EnableValidationAppearance](https://help.syncfusion.com/cr/document-processing/Syncfusion.Pdf.Security.PdfSignature.html#Syncfusion_Pdf_Security_PdfSignature_EnableValidationAppearance "EnableValidationAppearance property of PdfSignature class in .NET PDF Library") property of the [PdfSignature](https://help.syncfusion.com/cr/document-processing/Syncfusion.Pdf.Security.PdfSignature.html "PdfSignature class in .NET PDF Library") class to true. The signature appearance will automatically update based on the validation result provided by the PDF viewer.

### Visual indicators for signature validation

Depending on the validation status, the signature field will display one of the following icons:

*   **Green checkmark**: Valid digital signature
*   **Red X mark**: Invalid signature
*   **Yellow question mark**: Signature is unknown or not validated

This provides a clear and immediate visual cue to users about the authenticity and integrity of the signed document.

The following code example shows how to sign a PDF document with signature validation.

```csharp
using (PdfLoadedDocument document = new PdfLoadedDocument(@"../../Data/PDF_Succinctly.pdf"))
{
    // Load digital ID with password.
    PdfCertificate certificate = new PdfCertificate(@"../../Data/DigitalSignatureTest.pfx", "DigitalPass123");

    // Create a signature with loaded digital ID.
    PdfSignature signature = new PdfSignature(document, document.Pages[0], certificate, "DigitalSignature");         

    // Set bounds to the signature.
    signature.Bounds = new System.Drawing.RectangleF(40, 30, 350, 100);

    // Enable the signature validation appearance.
    signature.EnableValidationAppearance = true;

    // Load image from file.
    PdfImage image = PdfImage.FromFile(@"../../Data/signature.png");
    // Create a font to draw text.
    PdfStandardFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 15);

    signature.Appearance.Normal.Graphics.DrawImage(image, new System.Drawing.RectangleF(0, 0, 75, 75));
    signature.Appearance.Normal.Graphics.DrawString("Digitally Signed by Syncfusion", font, PdfBrushes.Black, 110, 5);
    signature.Appearance.Normal.Graphics.DrawString("Reason: Testing signature", font, PdfBrushes.Black, 110, 25);
    signature.Appearance.Normal.Graphics.DrawString("Location: USA", font, PdfBrushes.Black, 110, 45);               

    // Save the PDF document.
    document.Save("SignedAppearance.pdf");
}
```

By executing this code example, you will get a PDF document similar to the following screenshot.

![Screenshot showing a PDF with a dynamically validated signature (green checkmark) in Adobe Acrobat Reader, indicating "Signature valid".](https://www.syncfusion.com/blogs/wp-content/uploads/2019/10/Validating-signature-in-a-PDF-document-1.png)

## Use CAdES and different hashing algorithms

**CAdES (CMS Advanced Electronic Signatures)** is a European standard developed by [ETSI](https://www.etsi.org/deliver/etsi_en/319100_319199/31914201/01.01.00_30/en_31914201v010100v.pdf "Electronic signatures and infrastructures") to support secure and legally compliant electronic signatures across the EU. It is widely used for **long-term digital signature validation** in electronic transactions.

By default, the [Syncfusion .NET PDF Library](https://www.syncfusion.com/document-processing/pdf-framework/net/pdf-library ".NET PDF Library") uses the **CMS (PAdES Part 2)** standard with the **SHA-256 hashing algorithm**. However, you can easily switch to the **CAdES (PAdES Part 3)** standard and configure different hashing algorithms like **SHA-384** or **SHA-512** for enhanced security.

To apply the CAdES standard and a custom digest algorithm:

1.  Use the [PdfSignatureSettings](https://help.syncfusion.com/cr/document-processing/Syncfusion.Pdf.Security.PdfSignatureSettings.html "PdfSignatureSettings class in .NET PDF Library") class.
2.  Set the [CryptographicStandard](https://help.syncfusion.com/cr/document-processing/Syncfusion.Pdf.Security.CryptographicStandard.html "CryptographicStandard class in .NET PDF Library") property to [CryptographicStandard.CADES](https://help.syncfusion.com/cr/document-processing/Syncfusion.Pdf.Security.CryptographicStandard.html#Syncfusion_Pdf_Security_CryptographicStandard_CADES "CryptographicStandard.CADES field in .NET PDF Library").
3.  Set the [DigestAlgorithm](https://help.syncfusion.com/cr/document-processing/Syncfusion.Pdf.Security.PdfSignatureSettings.html#Syncfusion_Pdf_Security_PdfSignatureSettings_DigestAlgorithm "DigestAlgorithm property of PdfSignatureSettings class in .NET PDF Library") property to your preferred hashing algorithm.

The following code example shows how to create a PDF digital signature in C# with **CAdES** standard and a different hashing algorithm.

```csharp
//Load existing PDF document.
using (PdfLoadedDocument document = new PdfLoadedDocument(@"../../Data/PDF_Succinctly.pdf"))
{
    //Load digital ID with password.
    PdfCertificate certificate = new PdfCertificate(@"../../Data/DigitalSignatureTest.pfx", "DigitalPass123");

    //Create a signature with loaded digital ID.
    PdfSignature signature = new PdfSignature(document, document.Pages[0], certificate, "DigitalSignature");

    //Changing the digital signature standard and hashing algorithm.
    signature.Settings.CryptographicStandard = CryptographicStandard.CADES;
    signature.Settings.DigestAlgorithm = DigestAlgorithm.SHA512;

    //Save the PDF document.
    document.Save("SigneCAdES.pdf");
}
```

By executing this code example, you will get a PDF document with the following digital signature properties.

![Screenshot of Adobe Acrobat Reader showing digital signature properties, including CAdES standard and SHA-512 algorithm, confirming the signature is valid.](https://www.syncfusion.com/blogs/wp-content/uploads/2019/10/Digital-signature-properties-in-a-PDF-document-2.png)

## Add multiple signatures to a single PDF

In real-world scenarios, such as **publishing contracts or legal agreements**, a PDF document may require **multiple digital signatures** from different parties. For example, a publisher might apply for a **certification signature**, followed by an **approval signature** from the author.

With the [Syncfusion .NET PDF Library](https://www.syncfusion.com/document-processing/pdf-framework/net/pdf-library ".NET PDF Library"), you can easily add **multiple digital signatures** to a single PDF by appending new signatures to an already signed document—each signature creating a new revision.

### Key concept: PDF signature revisions

*   **First revision**: Initial digital signature using a certificate (e.g., **TestAgreement.pfx**)
*   **Second revision**: Additional signature using another certificate (e.g., **DigitalSignatureTest.pfx**)

Each signature is preserved and validated independently, ensuring document integrity and traceability.

The following code example shows how to create multiple PDF digital signatures in C#.

```csharp
//Load existing PDF document.
PdfLoadedDocument document = new PdfLoadedDocument(@"../../Data/PDF_Succinctly.pdf");

//Load digital ID with password.
PdfCertificate certificate = new PdfCertificate(@"../../Data/TestAgreement.pfx", "Test123");

//Create a Revision