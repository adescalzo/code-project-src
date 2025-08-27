# URL Processor - Python Migration

This is a Python migration of the Chrome extension for processing URLs with Gemini API.

## Features

- Fetches content from URLs in `source_links.txt`
- Extracts metadata (title, author, publication date)
- Processes content with Gemini API for enrichment
- Generates markdown files with YAML frontmatter
- Removes processed URLs from source file
- Tracks processed URLs in separate file
- Includes delay between requests
- Stops on Gemini API errors

## Setup

1. Install dependencies:
```bash
pip install -r requirements.txt
```

2. Configure environment:
```bash
cp .env.example .env
# Edit .env and add your GEMINI_API_KEY
```

3. Run the processor:
```bash
python url_processor.py
```

## Configuration

Edit `.env` file:

- `GEMINI_API_KEY`: Your Gemini API key
- `DELAY_BETWEEN_REQUESTS`: Seconds to wait between requests (default: 5)
- `MAX_RETRIES`: Max retry attempts for failed requests (default: 3)
- `OUTPUT_DIR`: Directory for processed markdown files (default: processed_documents)
- `SOURCE_FILE`: Path to source URLs file (default: ../source_links.txt)
- `PROCESSED_FILE`: File to track processed URLs (default: processed_links.txt)

## Output

Processed files are saved in the `processed_documents` directory with metadata including:
- Technologies used
- Programming languages
- Tags and key concepts
- Code examples presence
- Difficulty level
- Summary

## Logging

Logs are written to both console and `url_processor.log` file.