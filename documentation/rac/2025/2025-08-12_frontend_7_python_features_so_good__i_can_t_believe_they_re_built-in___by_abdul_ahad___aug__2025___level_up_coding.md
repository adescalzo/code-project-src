```yaml
---
title: "7 Python Features So Good, I Can’t Believe They’re Built-In | by Abdul Ahad | Aug, 2025 | Level Up Coding"
source: https://levelup.gitconnected.com/7-python-features-so-good-i-cant-believe-they-re-built-in-c2a9215d9523
date_published: 2025-08-13T02:22:25.085Z
date_captured: 2025-08-13T11:15:52.214Z
domain: levelup.gitconnected.com
author: Abdul Ahad
category: frontend
technologies: [pathlib, os, shutil, itertools, functools, requests, concurrent.futures, sched, subprocess, cron, Windows Task Scheduler]
programming_languages: [Python]
tags: [python, automation, built-in, productivity, file-management, concurrency, caching, scheduling, system-automation, performance-optimization]
key_concepts: [file system automation, iterators, function caching, parallel processing, task scheduling, inter-process communication, standard library]
code_examples: false
difficulty_level: intermediate
summary: |
  [This article highlights seven powerful, often overlooked, built-in Python features that significantly enhance automation scripts. It covers modules like `pathlib` for object-oriented file handling, `itertools` for efficient loop operations, and `functools.lru_cache` for performance optimization through caching. The author also demonstrates `concurrent.futures` for easy multithreading, `sched` for in-script task scheduling, `subprocess` for interacting with the system shell, and `shutil` for high-level file operations. The piece emphasizes writing "smarter code" by leveraging Python's standard library to avoid reinventing the wheel.]
---
```

# 7 Python Features So Good, I Can’t Believe They’re Built-In | by Abdul Ahad | Aug, 2025 | Level Up Coding

# 7 Python Features So Good, I Can’t Believe They’re Built-In

One of the reasons I’ve stuck with Python for over 4 years isn’t just its clean syntax or massive ecosystem.

![An illustration of a glowing treasure chest on a desk next to a computer monitor displaying code. Various icons representing programming concepts like a gear, lightning bolt, curly braces, search icon, and puzzle piece are floating out of the chest, symbolizing powerful built-in features.](https://miro.medium.com/v2/resize:fit:1000/0*APpqVGc2O4D1JMKh)

It’s the fact that Python quietly hides these ridiculously powerful built-in features that make automation feel almost unfair.

The funny thing? Many Python developers — even experienced ones — either overlook them or underuse them.
Today, we’re fixing that.

We’re going through **seven built-ins that are so good, I still shake my head when I remember they’re part of Python’s standard arsenal**. And yes — we’re going to focus entirely on **automation superpowers**.

# `pathlib` — File Automation Without the Pain

If you’re still doing `os.path.join()` gymnastics in 2025, we need to talk.

`pathlib` is the modern, object-oriented way to work with files and directories, and it makes file automation scripts 10x cleaner.

```python
from pathlib import Path  
  
# Get all PDF files in a folder  
downloads = Path.home() / "Downloads"  
pdfs = list(downloads.glob("*.pdf"))  
for pdf in pdfs:  
    print(f"Found: {pdf.name}")
```

**Why it’s magic for automation:**

*   Works seamlessly across OSes (goodbye Windows path slashes).
*   Chaining operations feels natural.
*   Globbing and filtering files is ridiculously concise.

**Pro tip:** Combine it with `shutil.move()` for an auto-sorting downloads folder.

# `itertools` — Automation’s Secret Weapon for Loops

Think of `itertools` as **loop steroids**. It can chain, group, slice, and infinitely generate data — all without manually writing boilerplate loops.

Example: Rotate through tasks every hour.

```python
import itertools  
import time  
  
tasks = ["Backup DB", "Clean Temp Files", "Send Report"]  
for task in itertools.cycle(tasks):  
    print(f"Running task: {task}")  
    time.sleep(3600)  # every hour
```

This is automation gold when scheduling repetitive tasks without a cron job.

**Bonus move:** `itertools.groupby()` is perfect for log file grouping before batch processing.

# `functools.lru_cache` — Automate Speed Boosts

If your automation script calls the same function with the same arguments over and over, `lru_cache` is a free performance upgrade.

```python
from functools import lru_cache  
import requests  
  
@lru_cache(maxsize=100)  
def fetch_url(url):  
    return requests.get(url).text  
# Even if called twice, only hits the network once  
data = fetch_url("https://example.com")  
data_again = fetch_url("https://example.com")
```

Perfect for:

*   Web scraping with repeated requests
*   Heavy database queries
*   API calls with rate limits

Caching like this used to require external libraries — now it’s one line.

# `concurrent.futures` — Instant Multithreading

Automating tasks often means waiting for I/O (network, files, APIs).
With `concurrent.futures`, you can parallelize them in just a few lines.

```python
from concurrent.futures import ThreadPoolExecutor  
import requests  
  
urls = ["https://example.com", "https://httpbin.org/get"]  
def fetch(url):  
    return requests.get(url).status_code  
with ThreadPoolExecutor() as executor:  
    results = executor.map(fetch, urls)  
print(list(results))
```

That’s multithreading without the usual headache. Your scripts finish faster, and you spend less time staring at progress bars.

# `sched` — Timed Automation Without Cron

Most people jump straight to `cron` or `Windows Task Scheduler`, but Python has its own scheduling module.

```python
import sched  
import time  
  
scheduler = sched.scheduler(time.time, time.sleep)  
def job():  
    print("Automated task executed!")  
scheduler.enter(10, 1, job)  # run after 10 seconds  
scheduler.run()
```

Why I love it:

*   No external dependency
*   Great for lightweight, time-based automation scripts
*   Perfect for cross-platform scheduled tasks without OS-level configs

# `subprocess` — Python as Your Command Center

If your automation touches anything in the system shell, `subprocess` is your bridge.

```python
import subprocess  
  
# Automate a system backup  
subprocess.run(["tar", "-czf", "backup.tar.gz", "/path/to/folder"])
```

It’s a staple for:

*   Running system commands
*   Combining Python logic with shell utilities
*   Automating deployment pipelines

**Pro tip:** Use `capture_output=True, text=True` to instantly grab the command’s output.

# `shutil` — Copy, Move, Delete — The Easy Way

While `os` can technically handle file operations, `shutil` makes them human-friendly.

```python
import shutil  
  
# Move all log files to an archive folder  
shutil.move("server.log", "archive/server.log")
```

I’ve used this for:

*   Automated backups
*   Folder restructuring scripts
*   Archiving old files

When paired with `pathlib`, you’ve basically got a mini file manager in Python.

# Wrapping Up

Automation isn’t just about writing code — it’s about knowing **what’s already at your fingertips** so you don’t reinvent the wheel.

_“Don’t write more code. Write smarter code.”_

These 7 built-ins are the kind of tools that make Python feel like cheating. They’re **fast**, **cross-platform**, and **always there** — no `pip install` required.

Next time you’re building an automation script, try weaving one or two of these into your flow.
You’ll wonder how you ever lived without them.