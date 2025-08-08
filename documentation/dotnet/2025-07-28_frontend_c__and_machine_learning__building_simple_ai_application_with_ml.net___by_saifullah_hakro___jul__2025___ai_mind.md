# C# and Machine Learning: Building Simple AI Application with ML.NET | by Saifullah Hakro | Jul, 2025 | AI Mind

**Source:** https://pub.aimind.so/c-and-machine-learning-building-simple-ai-application-with-ml-net-87175736d887
**Date Captured:** 2025-07-28T16:15:00.183Z
**Domain:** pub.aimind.so
**Author:** Saifullah Hakro
**Category:** frontend

---

Zoom image will be displayed

![](https://miro.medium.com/v2/resize:fit:700/1*nRd0POAL0aZjvf4exHSSbg.jpeg)

Member-only story

# C# and Machine Learning: Building Simple AI Application with ML.NET

[

![Saifullah Hakro](https://miro.medium.com/v2/resize:fill:64:64/1*ty8CD_PlXR7sRyc2-7868w.jpeg)





](https://medium.com/@saifullahhakro?source=post_page---byline--87175736d887---------------------------------------)

[Saifullah Hakro](https://medium.com/@saifullahhakro?source=post_page---byline--87175736d887---------------------------------------)

Follow

5 min read

·

Jul 20, 2025

74

1

Listen

Share

More

Curious how C# can power AI? With ML.NET, you can build machine learning models directly in .NET without switching languages. From sentiment analysis to predictions, ML.NET brings powerful tools to your fingertips. This guide walks you through creating a real-world AI app — fast, simple, and all in C#.

**Article Outline:**

1.  **What is Machine Learning (ML)?**
2.  **What is ML.NET?**
3.  **Why ML.NET?**
4.  **Features of ML.NET**
5.  **Example Application with step-by-step code**
6.  **Conclusion**
7.  **Call to Action**

# **What is Machine Learning (ML)?**

*   Machine Learning (ML) is a subset of artificial intelligence (AI) that allows systems to learn from data and improve over time without being explicitly programmed.
*   It uses statistical algorithms to identify patterns and make predictions or decisions.
*   ML automates analytical model building by learning from historical data.
*   Common applications include image recognition, recommendation systems, and fraud detection.

# What is ML.NET?

*   ML.NET is an open-source, cross-platform machine learning framework developed by Microsoft.
*   It allows .NET developers to build, train, and deploy custom machine learning models using C# or F#.
*   ML.NET supports various ML tasks such as classification, regression, clustering, and recommendation.
*   It integrates seamlessly with existing .NET applications and tools like Visual Studio.

# Why ML.NET?

*   Enables .NET developers to add machine learning capabilities without switching to Python or R.
*   Offers a familiar C# environment with strong integration into the .NET ecosystem.
*   Provides tooling support like Model Builder and AutoML for ease of use.
*   Supports training and consuming custom ML models offline, ensuring full control and data privacy.

# Features of ML.NET

*   **Cross-platform:** Works on Windows, Linux, and macOS using .NET Core.
*   **AutoML support:** Simplifies model selection and tuning for beginners.
*   **Data pipeline support:** Allows preprocessing, transformation, and training in a single workflow.
*   **Extensibility:** Can incorporate TensorFlow, ONNX, and custom models for advanced scenarios.

# Example Application with step-by-step code

Prerequisites

1.  .NET 6+
2.  VS Code
3.  C# extension for VS Code (ms-dotnettools.csharp)

Lets Start.

**Project Structure is Like this**

![](https://miro.medium.com/v2/resize:fit:368/1*LtB8QT-iWxxGld_xlXWVnA.png)

**Step 1: Create a new .NET console project in VS Code**

Open the terminal and run:

dotnet new console -n SentimentApp  
cd SentimentApp

**Step 2: Add ML.NET NuGet package**

Open the terminal and run:

dotnet add package Microsoft.ML

**Step 3: Create CSV File for training data, give it name as sentiment.csv**

Text,Label  
This is great!,true  
Horrible experience,false  
Absolutely fantastic!,true  
I hate it,false  
Not bad,true  
Worst ever,false  
I love this,true  
Terrible,false

This file will be used to train the model. Text is the user comment, and Label is the sentiment (true = positive, false = negative).

**Step 4: Create below files**

**A: SentimentData.cs**

using Microsoft.ML.Data;  
  
public class SentimentData  
{  
    \[LoadColumn(0)\]  
    public string? Text { get; set; }  
  
    \[LoadColumn(1)\]  
    public bool Label { get; set; }  
}

The SentimentData class defines the data schema for sentiment analysis in ML.NET. Each instance represents a single data point with:

*   Text: The input text to analyze (e.g., a review or comment).
*   Label: The sentiment label (true/false, e.g., positive/negative).

The `[LoadColumn]` attributes specify which columns from the input data file map to these properties. This class is used to load and process data for training or prediction in ML.NET models.

**B: SentimentPrediction.cs**

using Microsoft.ML.Data;  
public class SentimentPrediction : SentimentData  
{  
    \[ColumnName("PredictedLabel")\]  
    public bool PredictedLabel { get; set; }  
  
    public float Score { get; set; }  
    public float Probability { get; set; }  
}

The SentimentPrediction class represents the output of a sentiment analysis model, containing the predicted label (positive/negative), the prediction score, and the probability, and inherits input features from SentimentData.

**C: SentimentService.cs**

using Microsoft.ML;  
  
public class SentimentService  
{  
    private readonly MLContext \_mlContext;  
    private readonly PredictionEngine<SentimentData, SentimentPrediction> \_predEngine;  
  
    public SentimentService(string dataPath)  
    {  
        \_mlContext = new MLContext();  
        var dataView = LoadData(dataPath);  
        var model = BuildAndTrainModel(dataView);  
        \_predEngine = \_mlContext.Model.CreatePredictionEngine<SentimentData, SentimentPrediction>(model);  
    }  
  
    public SentimentPrediction Predict(string text)  
    {  
        var input = new SentimentData { Text = text };  
        return \_predEngine.Predict(input);  
    }  
  
    private IDataView LoadData(string dataPath)  
    {  
        return \_mlContext.Data.LoadFromTextFile<SentimentData>(  
            path: dataPath,  
            hasHeader: true,  
            separatorChar: ',');  
    }  
  
    private ITransformer BuildAndTrainModel(IDataView dataView)  
    {  
        var pipeline = \_mlContext.Transforms.Text.FeaturizeText("Features", nameof(SentimentData.Text))  
            .Append(\_mlContext.BinaryClassification.Trainers.SdcaLogisticRegression(  
                labelColumnName: nameof(SentimentData.Label), featureColumnName: "Features"));  
  
        return pipeline.Fit(dataView);  
    }  
}

The SentimentService class loads training data, builds and trains a sentiment analysis model using ML.NET, and provides a method to predict sentiment for input text.

**D: Program.cs**

using System;  
  
class Program  
{  
    static void Main()  
    {  
        Console.WriteLine("Enter text to predict sentiment");  
        var textToPredict = Console.ReadLine() ?? string.Empty;  
  
        var sentimentService = new SentimentService("sentiment.csv");  
        var prediction = sentimentService.Predict(textToPredict);  
  
        PrintPrediction(textToPredict, prediction);  
    }  
  
    private static void PrintPrediction(string input, SentimentPrediction prediction)  
    {  
        Console.WriteLine($"Text: {input}");  
        Console.WriteLine($"Prediction: {(prediction.PredictedLabel ? "Positive" : "Negative")}, Probability: {prediction.Probability:P2}");  
    }  
}

The Program class is the application’s entry point; it reads user input, predicts sentiment using SentimentService, and displays the result.

**Sample Outputs**

![](https://miro.medium.com/v2/resize:fit:600/1*JMhV3wyHWO5ilyPGLzg2qw.png)

![](https://miro.medium.com/v2/resize:fit:629/1*DQaA8JtIckL5IgBbMc9XQw.png)

# **Conclusion**

ML.NET makes machine learning accessible to every .NET developer. You can build, train, and deploy models seamlessly using familiar C# tools. With just a few lines of code, you’ve created a working sentiment analysis app. The future of AI in .NET is here — don’t miss out.

## Call to Action

Ready to bring AI into your .NET projects? Try ML.NET today and explore endless possibilities!

Github Link: [https://github.com/saifFast/SentimentApp\_ML.NET](https://github.com/saifFast/SentimentApp_ML.NET)

## A Message from AI Mind

![](https://miro.medium.com/v2/resize:fit:250/0*5Wm7sOfTpe5DEbhg.gif)

Thanks for being a part of our community! Before you go:

*   👏 Clap for the story and follow the author 👉
*   📰 View more content in the [AI Mind Publication](https://pub.aimind.so/)
*   🧠 Improve your [AI prompts effortlessly and FREE](https://www.aimind.so/prompt-generator?utm_source=pub&utm_medium=message)
*   **🧰 Discover** [**Intuitive AI Tools**](https://www.aimind.so/?utm_source=pub&utm_medium=message)