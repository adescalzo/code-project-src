```yaml
---
title: Implementing AES Encryption With C#
source: https://www.milanjovanovic.tech/blog/implementing-aes-encryption-with-csharp?utm_source=newsletter&utm_medium=email&utm_campaign=tnw126
date_published: 2025-01-25T00:00:00.000Z
date_captured: 2025-08-08T12:52:18.320Z
domain: www.milanjovanovic.tech
author: Milan Jovanović
category: security
technologies: [.NET, AES, RSA, Kestra, RavenDB Cloud, Azure Key Vault, AWS Key Management Service, HashiCorp Vault, ASP.NET Core]
programming_languages: [C#]
tags: [encryption, security, aes, csharp, dotnet, data-protection, cryptography, key-management, symmetric-encryption, application-security]
key_concepts: [AES encryption, Symmetric encryption, Asymmetric encryption, Initialization Vector (IV), Key management, Data security, CryptographicException, AES-256]
code_examples: false
difficulty_level: intermediate
summary: |
  [The article emphasizes the critical need for robust encryption to protect sensitive application data like API keys and database credentials. It differentiates between symmetric (AES) and asymmetric (RSA) encryption, advocating for AES in storing application secrets due to its efficiency. The core of the content provides a practical C# implementation of AES-256 encryption and decryption, stressing the importance of generating unique keys and Initialization Vectors (IVs) for each operation to maintain security. It also highlights the necessity of secure key management using dedicated services like Azure Key Vault or AWS KMS, along with proper error handling. The author concludes by positioning encryption as a fundamental part of a comprehensive security strategy.]
---
```

# Implementing AES Encryption With C#

![Implementing AES Encryption With C#](/blog-covers/mnw_126.png?imwidth=3840)

# Implementing AES Encryption With C#

5 min read · January 25, 2025

**Kestra** is a powerful open-source orchestration platform for developers, offering a declarative approach to defining workflows with minimal complexity. With robust plugins and seamless integrations, it simplifies automating and scaling processes. **Check it out on GitHub**.

Accelerate your projects with **RavenDB Cloud** - an affordable NoSQL document database for developers prioritizing speed and simplicity. Set up your database in minutes: **start here**.

Sponsor this newsletter

A single exposed API key or database password can compromise your entire infrastructure. Yet many developers still store sensitive data with basic encoding or weak encryption.

Properly implemented encryption is your last line of defense. When other security measures fail, it ensures stolen data remains unreadable. This is especially crucial for API keys, database credentials, and user secrets that grant direct access to your systems.

In today's issue, we will cover implementing AES encryption in .NET with practical code examples and essential security considerations.

## Symmetric vs Asymmetric Encryption

Symmetric encryption (like AES) uses the same key for encryption and decryption. It's fast and ideal for storing data that only your application needs to read. The main challenge is securely storing the encryption key.

Asymmetric encryption (like RSA) uses different keys for encryption and decryption. It's slower but allows secure communication between parties who don't share secrets. Common uses include SSL/TLS and digital signatures.

For storing API keys and application secrets, symmetric encryption with AES is the appropriate choice.

![AES encryption algorithm.](/blogs/mnw_126/aes_encryption.png?imwidth=1920)

AES encryption and decryption process block diagram.

## AES Encryption Implementation

Let's examine a secure [AES (Advanced Encryption Standard)](https://en.wikipedia.org/wiki/Advanced_Encryption_Standard) encryption implementation in C#. This implementation uses AES-256, which provides the strongest security currently available in the AES standard.

```csharp
public class Encryptor
{
    private const int KeySize = 256;
    private const int BlockSize = 128;

    public static EncryptionResult Encrypt(string plainText)
    {
        // Generate a random key and IV
        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;

        // Generate a random key and IV for each encryption operation
        aes.GenerateKey();
        aes.GenerateIV();

        byte[] encryptedData;

        // Create encryptor and encrypt the data
        using (var encryptor = aes.CreateEncryptor())
        using (var msEncrypt = new MemoryStream())
        {
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            using (var swEncrypt = new StreamWriter(csEncrypt))
            {
                swEncrypt.Write(plainText);
            }

            encryptedData = msEncrypt.ToArray();
        }

        // Package everything together, storing IV with the encrypted data
        var result = EncryptionResult.CreateEncryptedData(
            encryptedData,
            aes.IV,
            Convert.ToBase64String(aes.Key)
        );

        return result;
    }
}

public class EncryptionResult
{
    // The IV is prepended to the encrypted data
    public string EncryptedData { get; set; }
    public string Key { get; set; }

    public static EncryptionResult CreateEncryptedData(byte[] data, byte[] iv, string key)
    {
        // Combine IV and encrypted data
        var combined = new byte[iv.Length + data.Length];
        Array.Copy(iv, 0, combined, 0, iv.Length);
        Array.Copy(data, 0, combined, iv.Length, data.Length);

        return new EncryptionResult
        {
            EncryptedData = Convert.ToBase64String(combined),
            Key = key
        };
    }

    public (byte[] iv, byte[] encryptedData) GetIVAndEncryptedData()
    {
        var combined = Convert.FromBase64String(EncryptedData);

        // Extract IV and data
        var iv = new byte[16]; // AES block size is 16 bytes (128 / 8)
        var encryptedData = new byte[combined.Length - 16];

        Array.Copy(combined, 0, iv, 0, 16);
        Array.Copy(combined, 16, encryptedData, 0, encryptedData.Length);

        return (iv, encryptedData);
    }
}
```

Let's break down what's happening in this implementation:

*   Every encryption operation generates a new random key and IV (Initialization Vector). This is crucial - reusing either of these compromises security. The IV prevents identical plaintext from producing identical ciphertext.
*   We use `CryptoStream` for efficient encryption of potentially large data. The stream pattern ensures we don't load everything into memory at once.
*   The `EncryptionResult` class provides a way to package the encrypted data with its key and IV. In production, the key should be stored separately in a key management service.

## AES Decryption Implementation

Here's the corresponding decryption implementation:

```csharp
public class Decryptor
{
    private const int KeySize = 256;
    private const int BlockSize = 128;

    public static string Decrypt(EncryptionResult encryptionResult)
    {
        var key = Convert.FromBase64String(encryptionResult.Key);
        var (iv, encryptedData) = encryptionResult.GetIVAndEncryptedData();

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Key = key;
        aes.IV = iv;

        // Create decryptor and decrypt the data
        using var decryptor = aes.CreateDecryptor();
        using var msDecrypt = new MemoryStream(encryptedData);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        using var srDecrypt = new StreamReader(csDecrypt);

        try
        {
            return srDecrypt.ReadToEnd();
        }
        catch (CryptographicException ex)
        {
            // Log the error securely - avoid exposing details
            throw new CryptographicException("Decryption failed", ex);
        }
    }
}
```

The decryption process reverses the encryption steps. Note the error handling - we catch cryptographic exceptions but avoid exposing details that could help an attacker. In production, you should log these errors securely for debugging while keeping security in mind.

## Usage Example

Here's an example of encrypting and decrypting sensitive data using the implementations above:

```csharp
// Encrypt sensitive data
var apiKey = "your-sensitive-api-key";
var encryptionResult = Encryptor.Encrypt(apiKey);

// Output example: DCGT9kEwPglBonWWPa7PQPbr2I+6rskJ0lSFybbicvZ+wKMTU7cbJD2s3QSF2Yu6

// Store encrypted data in database
// IV is stored with the encrypted data
SaveToDatabase(encryptionResult.EncryptedData);

// Store key in key vault
await keyVault.StoreKeyAsync("apikey_1", encryptionResult.Key);

// Later, decrypt when needed
// IV is retrieved from the encrypted data
var encryptedData = LoadFromDatabase();
var key = await keyVault.GetKeyAsync("apikey_1");

var result = new EncryptionResult
{
    EncryptedData = encryptedData,
    Key = key,
    IV = iv
};

var decrypted = Decryptor.Decrypt(result);
```

## Takeaway

AES encryption provides strong security for sensitive application data when implemented correctly.

Proper key management is very important. Use a dedicated key storage service in production. Popular options include [Azure Key Vault](https://learn.microsoft.com/en-us/azure/key-vault/), [AWS Key Management Service](https://aws.amazon.com/kms/), and [HashiCorp Vault](https://www.vaultproject.io/).

In my [**Pragmatic REST APIs**](/pragmatic-rest-apis) course, I cover secure data storage and encryption in more detail. These are critical aspects of building secure and robust APIs and integrating with third-party APIs. Check it out if you're interested in learning more.

Remember that encryption is just one part of a comprehensive security strategy. Keep your encryption keys separate from encrypted data and rotate them regularly.

Thanks for reading.

And stay awesome!

---

Whenever you're ready, there are 4 ways I can help you:

1.  [Pragmatic Clean Architecture:](/pragmatic-clean-architecture) Join 4,200+ students in this comprehensive course that will teach you the system I use to ship production-ready applications using Clean Architecture. Learn how to apply the best practices of modern software architecture.
2.  [Modular Monolith Architecture:](/modular-monolith-architecture) Join 2,100+ engineers in this in-depth course that will transform the way you build modern systems. You will learn the best practices for applying the Modular Monolith architecture in a real-world scenario.
3.  [(NEW) Pragmatic REST APIs:](/pragmatic-rest-apis) Join 1,100+ students in this course that will teach you how to build production-ready REST APIs using the latest ASP.NET Core features and best practices. It includes a fully functional UI application that we'll integrate with the REST API.
4.  [Patreon Community:](https://www.patreon.com/milanjovanovic) Join a community of 5,000+ engineers and software architects. You will also unlock access to the source code I use in my YouTube videos, early access to future videos, and exclusive discounts for my courses.

Become a Better .NET Software Engineer

Join 70,000+ engineers who are improving their skills every Saturday morning.

Accelerate Your .NET Skills 🚀

![PCA Cover](/_next/static/media/cover.27333f2f.png?imwidth=384)

Pragmatic Clean Architecture

![MMA Cover](/_next/static/media/cover.31e11f05.png?imwidth=384)

Modular Monolith Architecture

![PRA Cover](/_next/static/media/cover_1.fc0deb78.png?imwidth=384)

Pragmatic REST APIs

NEW