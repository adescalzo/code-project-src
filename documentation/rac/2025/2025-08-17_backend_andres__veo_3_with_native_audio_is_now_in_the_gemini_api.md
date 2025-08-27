```yaml
---
title: Andres, Veo 3 with native audio is now in the Gemini API
source: https://notifications.google.com/g/vib/ANiao5rAE7AI3c1VzXbIv8CTWeMOgQ4QuQ33fQJ-cKk2X52Wj7YIEuBJL1sq5W4o41aClonAL9OGFBHtHX2FwnxIBPTA9AQRO9nrXWi10DHO_otjqfE_VL86laowd1xylXX3wzo
date_published: unknown
date_captured: 2025-08-17T13:21:24.909Z
domain: notifications.google.com
author: Unknown
category: backend
technologies: [Veo 3, Gemini API, Google AI Studio, Google Cloud, REST API]
programming_languages: [Python, JavaScript, Go]
tags: [ai, generative-ai, video-generation, text-to-video, api, google-ai, machine-learning, sdk]
key_concepts: [text-to-video-generation, generative-ai, api-integration, sdk-usage, prompt-engineering, native-audio-generation]
code_examples: false
difficulty_level: intermediate
summary: |
  [Google AI Studio has announced the availability of Veo 3 and Veo 3 Fast within the Gemini API, now in paid preview. This new capability allows users to generate high-fidelity, 720p videos complete with native audio, dialogue, and sound effects from a single text prompt or an initial image. Developers can integrate Veo 3 using SDKs for Python, JavaScript, and Go, or via direct REST API calls. A Python code example is provided to demonstrate video generation with dialogue. Users need a billing-enabled Google Cloud project to obtain an API key and access the service.]
---
```

# Andres, Veo 3 with native audio is now in the Gemini API

# Veo 3 Now Available in the Gemini API for Video Generation

Google AI Studio announces the integration of Veo 3 and Veo 3 Fast into the Gemini API, now available in paid preview. This powerful new capability allows users to generate high-fidelity, 720p videos complete with native audio, dialogue, and sound effects directly from a single text prompt.

Veo 3 is designed to bring your creative prompts to life, producing 8-second videos with rich native audio. It supports various input methods, including generating video from a text prompt, an initial image, or a combination of both to guide the visual style and starting frame. The model is highly versatile, capable of creating a wide range of visual styles and natively generating dialogue in multiple languages, alongside realistic sound effects and ambient noise.

## Getting Started

Developers can quickly integrate Veo 3 into their applications using the provided SDKs for Python, JavaScript, and Go. For direct interaction, a REST API call with cURL is also supported. To begin, users need to obtain an API key, which requires a billing-enabled Google Cloud project.

## Example: Generating a Video with Python

The following Python code snippet demonstrates how to use the `genai` client to generate a video with dialogue from a text prompt:

```python
import time
from google import genai

client = genai.Client()

prompt = """A close up of two people staring at a cryptic drawing on a wall, torchlight flickering.
A man murmurs, 'This must be it. That's the secret code.' The woman looks at him and whispering excitedly, 'What did you find?'"""

# Start the generation job
operation = client.models.generate_videos(
    model="veo-3.0-generate-preview",
    prompt=prompt,
)

# Poll for the result
while not operation.done:
    print("Waiting for video generation to complete...")
    time.sleep(10)
    operation = client.operations.get(operation)

# Download the final video
video = operation.response.generated_videos[0]
video.video.save("dialogue_example.mp4")
print("Generated video saved to dialogue_example.mp4")
```

## Next Steps

For more detailed information, including exploring all parameters, learning advanced prompting techniques, and discovering additional examples, refer to the official documentation.