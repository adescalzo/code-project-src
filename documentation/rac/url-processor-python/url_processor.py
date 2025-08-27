#!/usr/bin/env python3

import os
import sys
import time
import asyncio
import aiohttp
import google.generativeai as genai
from datetime import datetime
from pathlib import Path
from urllib.parse import urlparse
import json
import re
import html2text
from bs4 import BeautifulSoup
from dotenv import load_dotenv
import logging

# Load environment variables
load_dotenv()

# Setup logging
logging.basicConfig(
    level=logging.INFO,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('url_processor.log'),
        logging.StreamHandler()
    ]
)
logger = logging.getLogger(__name__)

class URLProcessor:
    def __init__(self):
        # Configuration from environment
        self.api_key = os.getenv('GEMINI_API_KEY')
        if not self.api_key:
            raise ValueError("GEMINI_API_KEY not found in environment variables")
        
        self.delay_between_requests = int(os.getenv('DELAY_BETWEEN_REQUESTS', 5))
        self.max_retries = int(os.getenv('MAX_RETRIES', 3))
        self.output_dir = Path(os.getenv('OUTPUT_DIR', 'processed_documents'))
        self.source_file = Path(os.getenv('SOURCE_FILE', '../source_links.txt'))
        self.processed_file = Path(os.getenv('PROCESSED_FILE', 'processed_links.txt'))
        
        # Create output directory if it doesn't exist
        self.output_dir.mkdir(parents=True, exist_ok=True)
        
        # Initialize Gemini
        genai.configure(api_key=self.api_key)
        self.model = genai.GenerativeModel('gemini-2.0-flash-exp')
        
        # HTML to markdown converter
        self.html_converter = html2text.HTML2Text()
        self.html_converter.ignore_links = False
        self.html_converter.ignore_images = False
        self.html_converter.body_width = 0  # No line wrapping
        
        logger.info(f"URLProcessor initialized with output directory: {self.output_dir}")
    
    async def fetch_url_content(self, session, url):
        """Fetch content from URL"""
        try:
            async with session.get(url, timeout=30) as response:
                if response.status == 200:
                    return await response.text()
                else:
                    logger.warning(f"HTTP {response.status} for URL: {url}")
                    return None
        except Exception as e:
            logger.error(f"Error fetching {url}: {str(e)}")
            return None
    
    def extract_metadata_from_html(self, html_content, url):
        """Extract metadata from HTML content"""
        soup = BeautifulSoup(html_content, 'html.parser')
        
        # Extract title
        title = soup.find('title')
        title = title.text.strip() if title else urlparse(url).netloc
        
        # Extract author
        author = None
        author_meta = soup.find('meta', {'name': 'author'}) or \
                     soup.find('meta', {'property': 'article:author'})
        if author_meta:
            author = author_meta.get('content', '')
        
        if not author:
            author_elem = soup.find(class_=re.compile('author', re.I)) or \
                         soup.find(attrs={'itemprop': 'author'})
            if author_elem:
                author = author_elem.get_text(strip=True)
        
        # Extract publication date
        pub_date = None
        date_meta = soup.find('meta', {'property': 'article:published_time'}) or \
                   soup.find('meta', {'name': 'publish_date'}) or \
                   soup.find('meta', {'property': 'og:published_time'})
        if date_meta:
            pub_date = date_meta.get('content', '')
        
        if not pub_date:
            time_elem = soup.find('time', {'datetime': True})
            if time_elem:
                pub_date = time_elem.get('datetime', '')
        
        # Extract main content
        main_content = soup.find('main') or soup.find('article') or \
                      soup.find(attrs={'role': 'main'}) or soup.body
        
        return {
            'title': title,
            'author': author or 'Unknown',
            'publication_date': pub_date,
            'domain': urlparse(url).netloc,
            'content': main_content
        }
    
    async def process_with_gemini(self, content, metadata, url):
        """Process content with Gemini API"""
        prompt = f"""You are a technical content analyzer. Analyze the provided content and extract metadata.

CRITICAL: You MUST format your response EXACTLY as shown below. Do not deviate from this format.

## Metadata
Technologies: [LIST HERE - e.g., ASP.NET Core, Dapper, Entity Framework, SQL Server, PostgreSQL]
Programming_Languages: [LIST HERE - e.g., C#, SQL, JavaScript]
Tags: [LIST HERE - e.g., orm, database, dotnet, web-api, data-access]
Key_Concepts: [LIST HERE - e.g., micro-orm, repository-pattern, dependency-injection]
Code_Examples: [yes or no]
Difficulty_Level: [beginner or intermediate or advanced]
Summary: [Write 4-6 sentences here]

## Content
[Put the improved markdown content here]

IMPORTANT RULES:
1. For Technologies: List ALL technologies, frameworks, libraries, tools mentioned
2. For Programming_Languages: List ONLY programming languages, NOT frameworks
3. For Tags: Use lowercase, hyphenated terms for categorization (max 10)
4. For Key_Concepts: List main technical concepts, patterns, or methodologies (max 8)
5. Never leave any field empty - if nothing found, write "none"
6. Keep the original content structure but improve formatting and clarity

Now analyze this content from {url}:

{content}"""
        
        try:
            response = self.model.generate_content(
                prompt,
                generation_config=genai.GenerationConfig(
                    temperature=0.3,
                    max_output_tokens=8192,
                )
            )
            
            result_text = response.text
            
            # Parse metadata from response
            metadata_match = re.search(r'## Metadata\n([\s\S]*?)\n\n## Content', result_text)
            content_match = re.search(r'## Content\n([\s\S]*)', result_text)
            
            extracted_metadata = {}
            if metadata_match:
                metadata_text = metadata_match.group(1)
                
                # Extract each field
                patterns = {
                    'technologies': r'Technologies:\s*\[([^\]]*)\]',
                    'programming_languages': r'Programming_Languages:\s*\[([^\]]*)\]',
                    'tags': r'Tags:\s*\[([^\]]*)\]',
                    'key_concepts': r'Key_Concepts:\s*\[([^\]]*)\]',
                    'code_examples': r'Code_Examples:\s*(yes|no)',
                    'difficulty_level': r'Difficulty_Level:\s*(beginner|intermediate|advanced)',
                    'summary': r'Summary:\s*(.*?)(?=\n\n|$)',
                }
                
                for key, pattern in patterns.items():
                    match = re.search(pattern, metadata_text, re.IGNORECASE | re.DOTALL)
                    if match:
                        value = match.group(1).strip()
                        if key in ['technologies', 'programming_languages', 'tags', 'key_concepts']:
                            if value and value.lower() != 'none':
                                extracted_metadata[key] = [item.strip() for item in value.split(',') if item.strip()]
                            else:
                                extracted_metadata[key] = []
                        elif key == 'code_examples':
                            extracted_metadata[key] = value.lower() == 'yes'
                        else:
                            extracted_metadata[key] = value
            
            improved_content = content_match.group(1) if content_match else content
            
            return {
                'metadata': extracted_metadata,
                'content': improved_content
            }
            
        except Exception as e:
            logger.error(f"Gemini API error for {url}: {str(e)}")
            raise
    
    def create_markdown_file(self, url, metadata, gemini_data):
        """Create markdown file with metadata and content"""
        # Generate filename from URL
        parsed = urlparse(url)
        domain = parsed.netloc.replace('www.', '')
        path = parsed.path.strip('/').replace('/', '_')
        
        # Create a safe filename
        if path:
            filename = f"{domain}_{path[:100]}.md"
        else:
            filename = f"{domain}_{datetime.now().strftime('%Y%m%d_%H%M%S')}.md"
        
        # Sanitize filename
        filename = re.sub(r'[^\w\-_\.]', '_', filename)
        
        # Format YAML frontmatter
        yaml_content = f"""---
title: {metadata.get('title', 'Untitled')}
source: {url}
date_published: {metadata.get('publication_date') or 'unknown'}
date_captured: {datetime.now().isoformat()}
domain: {metadata['domain']}
author: {metadata.get('author', 'Unknown')}
category: programming
technologies: {gemini_data['metadata'].get('technologies', [])}
programming_languages: {gemini_data['metadata'].get('programming_languages', [])}
tags: {gemini_data['metadata'].get('tags', [])}
key_concepts: {gemini_data['metadata'].get('key_concepts', [])}
code_examples: {gemini_data['metadata'].get('code_examples', False)}
difficulty_level: {gemini_data['metadata'].get('difficulty_level', 'unknown')}
summary: |
  {gemini_data['metadata'].get('summary', 'No summary available').replace(chr(10), chr(10) + '  ')}
---

# {metadata.get('title', 'Untitled')}

{gemini_data['content']}
"""
        
        # Write to file
        output_path = self.output_dir / filename
        with open(output_path, 'w', encoding='utf-8') as f:
            f.write(yaml_content)
        
        return output_path
    
    def read_urls_from_file(self):
        """Read URLs from source file"""
        if not self.source_file.exists():
            logger.error(f"Source file not found: {self.source_file}")
            return []
        
        with open(self.source_file, 'r', encoding='utf-8') as f:
            urls = [line.strip() for line in f if line.strip()]
        
        return urls
    
    def remove_processed_url(self, url):
        """Remove processed URL from source file"""
        urls = self.read_urls_from_file()
        remaining_urls = [u for u in urls if u != url]
        
        with open(self.source_file, 'w', encoding='utf-8') as f:
            for u in remaining_urls:
                f.write(f"{u}\n")
        
        logger.info(f"Removed {url} from source file")
    
    def add_to_processed_file(self, url, output_path):
        """Add processed URL to processed file"""
        with open(self.processed_file, 'a', encoding='utf-8') as f:
            f.write(f"{datetime.now().isoformat()}\t{url}\t{output_path}\n")
    
    async def process_url(self, session, url):
        """Process a single URL"""
        logger.info(f"Processing: {url}")
        
        try:
            # Fetch content
            html_content = await self.fetch_url_content(session, url)
            if not html_content:
                logger.error(f"Failed to fetch content for {url}")
                return False
            
            # Extract metadata
            metadata = self.extract_metadata_from_html(html_content, url)
            
            # Convert to markdown
            markdown_content = self.html_converter.handle(str(metadata['content']))
            
            # Process with Gemini
            logger.info(f"Processing with Gemini API...")
            gemini_data = await self.process_with_gemini(markdown_content, metadata, url)
            
            # Create markdown file
            output_path = self.create_markdown_file(url, metadata, gemini_data)
            logger.info(f"Created file: {output_path}")
            
            # Update files
            self.remove_processed_url(url)
            self.add_to_processed_file(url, str(output_path))
            
            return True
            
        except Exception as e:
            logger.error(f"Error processing {url}: {str(e)}")
            if "gemini" in str(e).lower():
                logger.error("Gemini API error detected. Stopping processing.")
                raise
            return False
    
    async def run(self):
        """Main processing loop"""
        urls = self.read_urls_from_file()
        
        if not urls:
            logger.info("No URLs to process")
            return
        
        logger.info(f"Found {len(urls)} URLs to process")
        
        async with aiohttp.ClientSession() as session:
            for i, url in enumerate(urls, 1):
                logger.info(f"Processing URL {i}/{len(urls)}")
                
                try:
                    success = await self.process_url(session, url)
                    
                    if success:
                        logger.info(f"Successfully processed {url}")
                    else:
                        logger.warning(f"Failed to process {url}")
                    
                    # Delay between requests
                    if i < len(urls):
                        logger.info(f"Waiting {self.delay_between_requests} seconds before next request...")
                        await asyncio.sleep(self.delay_between_requests)
                        
                except Exception as e:
                    if "gemini" in str(e).lower():
                        logger.error("Stopping due to Gemini API error")
                        break
                    logger.error(f"Unexpected error: {str(e)}")
                    continue
        
        logger.info("Processing complete")

def main():
    try:
        processor = URLProcessor()
        asyncio.run(processor.run())
    except KeyboardInterrupt:
        logger.info("Processing interrupted by user")
    except Exception as e:
        logger.error(f"Fatal error: {str(e)}")
        sys.exit(1)

if __name__ == "__main__":
    main()