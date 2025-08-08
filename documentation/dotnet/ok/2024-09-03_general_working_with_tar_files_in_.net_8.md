```yaml
---
title: Working with tar files in .NET 8
source: https://andrewlock.net/working-with-tar-files-in-dotnet/
date_published: 2024-09-03T12:00:00.000Z
date_captured: 2025-08-08T13:48:13.784Z
domain: andrewlock.net
author: Unknown
category: general
technologies: [.NET Core, .NET 7, .NET 8, tar, gzip, GZipStream, TarFile, TarReader, TarEntry, SharpZipLib, SharpCompress, Aspose.ZIP, NuGet, Linux, Windows]
programming_languages: [C#, Bash]
tags: [file-compression, tar, gzip, dotnet, file-operations, cross-platform, command-line, archives, system.formats.tar, base-class-library]
key_concepts: [tarball, gzip-compression, file-archiving, file-extraction, symbolic-links, hard-links, file-permissions, base-class-library]
code_examples: false
difficulty_level: intermediate
summary: |
  This article explores how to work with `tar` and `tar.gz` files using the built-in `System.Formats.Tar` namespace in .NET 8. It provides practical examples for common operations such as creating, extracting, and listing the contents of archives, comparing the .NET API usage with the traditional `tar` command-line utility. The author demonstrates how to combine `TarFile` with `GZipStream` to handle the ubiquitous `tar.gz` format. The post concludes by discussing several limitations and missing features in the current .NET implementation, including issues with hardlink creation, ownership control during extraction, and handling of absolute paths.
---
```

# Working with tar files in .NET 8

# Working with tar files in .NET 8

Back in 2022 .NET 7 gained support for natively working with `tar` files in the base class library. In this post I describe how to perform some basic operations on tar files, how I typically use the `tar` command-line utility for doing them, and how to instead use the support built-in to .NET. I then discuss the various limitations of the existing support.

*   [What is a tar file?](#what-is-a-tar-file-)
*   [Creating a `.tar.gz` archive](#creating-a-tar-gz-archive)
    *   [Creating a `.tar.gz` archive using `tar`](#creating-a-tar-gz-archive-using-tar)
    *   [Creating a `.tar.gz` archive using .NET](#creating-a-tar-gz-archive-using-net)
*   [Extracting a `.tar.gz` archive](#extracting-a-tar-gz-archive)
    *   [Extracting a `.tar.gz` archive using `tar`](#extracting-a-tar-gz-archive-using-tar)
    *   [Extracting a `.tar.gz` archive using .NET](#extracting-a-tar-gz-archive-using-net)
*   [Extracting a single file from a `.tar.gz` archive](#extracting-a-single-file-from-a-tar-gz-archive)
    *   [Extracting a single file from a `.tar.gz` archive using `tar`](#extracting-a-single-file-from-a-tar-gz-archive-using-tar)
    *   [Extracting a single file from a `.tar.gz` archive using .NET](#extracting-a-single-file-from-a-tar-gz-archive-using-net)
*   [Listing all the files in a `.tar.gz` without extraction](#listing-all-the-files-in-a-tar-gz-without-extraction)
    *   [Listing all the files in a `.tar.gz` using `tar`](#listing-all-the-files-in-a-tar-gz-using-tar)
    *   [Listing all the files in a `.tar.gz` using .NET](#listing-all-the-files-in-a-tar-gz-using-net)
*   [Caveats, missing features, and bugs](#caveats-missing-features-and-bugs)
    *   [.NET can't create hardlinks in `.tar` archives](#-net-can-t-create-hardlinks-in-tar-archives)
    *   [.NET can't control ownership during extraction](#-net-can-t-control-ownership-during-extraction)
    *   [.NET can't handle absolute paths](#-net-can-t-handle-absolute-paths)

## What is a tar file?

[A _tar_ file](https://en.wikipedia.org/wiki/Tar_\(computing\)) (often called a _tarball_) is a file (typically with the suffix `.tar`) that combines multiple files into a single file. Tar files are very common in Linux and other \*nix based OSs for distributing multiple files or for archiving/backing up files. On Windows it's more common [to see `.zip` files](https://en.wikipedia.org/wiki/ZIP_\(file_format\)) used for these purposes (though Windows now also has native support for tar files).

Unlike `.zip` files, `.tar` files don't natively have compression, so it's extremely common to see `.tar.gz` files. These files are "normal" `.tar` files that have been subsequently compressed using [`gzip`](https://en.wikipedia.org/wiki/Gzip) (which is based on the same [DEFLATE](https://en.wikipedia.org/wiki/DEFLATE) algorithm as ZIP files).

Creating a `tar` from a directory can preserve many of the attributes of the files on the file system, such as:

*   Directory structure
*   File names (normally relative, but you _can_ create absolute paths)
*   Permissions ([POSIX-style](https://en.wikipedia.org/wiki/Unix_file_types#Representations))
*   Modification date
*   Owner IDs
*   [Symbolic and Hard links](https://www.redhat.com/sysadmin/linking-linux-explained)

Working with tar files in .NET prior to .NET 7 had always required a third-party library. There are a bunch of options available on NuGet:

*   [SharpZipLib](https://github.com/icsharpcode/SharpZipLib) (Open source)
*   [SharpCompress](https://github.com/adamhathcock/sharpcompress) (Open source)
*   [Aspose.ZIP](https://products.aspose.com/zip/net/) (Commercial)

In .NET 7, basic support for working with tar files was added to the base class library. For the rest of this post I show how to use these APIs to perform common functions on tar files.

> Note that while the APIs I use in this post all exist in .NET 7 as well, .NET 8 includes a variety of bug fixes and support for more tar file features and formats, and is what I'm using in this post.

All the examples of using the `tar` command-line are shown running on Linux, but the .NET code should work on any OS.

## Creating a `.tar.gz` archive

We'll start with the most obvious place, _creating_ a tar file from an existing directory. Lets imagine you have a directory of files in your home directory, in `~/my-files`, which you want to distribute. This also includes a symbolic link (`myapp.so`) and a hard link (`someother.so`):

```bash
$ ls -lR ~/my-files
/home/andrewlock/my-files:
total 1420
drwxr-xr-x 2 root root    4096 Aug 11 16:00 bin
drwxr-xr-x 2 root root    4096 Aug 11 15:57 docs
lrwxrwxrwx 1 root root      17 Aug 11 16:01 myapp.so -> ./bin/myapp.so
-rw-r--r-- 2 root root 1443232 Aug 11 15:56 someother.so

/home/andrewlock/my-files/bin:
total 3756
-rw-r--r-- 1 root root 2399608 Aug 11 15:55 myapp.so
-rw-r--r-- 2 root root 1443232 Aug 11 15:56 someother.so

/home/andrewlock/my-files/docs:
total 5896
-rw-r--r-- 1 root root      10 Aug 11 15:57 README
-rw-r--r-- 1 root root 6027280 Aug 11 15:57 someother.xml
```

### Creating a `.tar.gz` archive using `tar`

A common command to create a tarball of these files called `myarchive.tar.gz` in the home directory would be:

```bash
cd ~/my-files 
tar -czvf ~/myarchive.tar.gz .
```

In this example we change the working directory to `~/my-files` (if we were running from `~`, `tar` would include `my-files` as a prefix to the path names in the tar directory). The flags passed to the `tar` command mean:

*   `-c` Create a new archive
*   `-z` Compress the resulting `tar` file with `gzip`
*   `-v` List the files being processed (optional)
*   `-f <FILE>` Output the archive to file `<FILE>`

Note that if you omit the `-z` flag, `tar` creates a `tar` file which is _not_ compressed.

### Creating a `.tar.gz` archive using .NET

So how can we achieve this in .NET? .NET 7 added [the `TarFile` class](https://learn.microsoft.com/en-us/dotnet/api/system.formats.tar.tarfile?view=net-8.0) which includes [static methods for creating a tar archive](https://learn.microsoft.com/en-us/dotnet/api/system.formats.tar.tarfile.createfromdirectory), so you might think you could do something like this:

```csharp
using System.Formats.Tar;

string sourceDir = "./my-files";
string outputFile = "./myarchive.tar"; // note this _doesn't_ create a valid .tar.gz file

TarFile.CreateFromDirectory(sourceDir, outputFile, includeBaseDirectory: false);
```

The problem is that the `TarFile` utility _only_ handles the `tar` format, it _doesn't_ include the `gzip` handling which is so ubiquitous when working with tar files. Luckily, it's not too hard to add support for that using `GZipStream` and handling the file and stream creation ourselves:

```csharp
using System.Formats.Tar;
using System.IO.Compression;

string sourceDir = "./my-files";
string outputFile = "./myarchive.tar.gz";

using FileStream fs = new(outputFile, FileMode.CreateNew, FileAccess.Write);
using GZipStream gz = new(fs, CompressionMode.Compress, leaveOpen: true);

TarFile.CreateFromDirectory(sourceDir, gz, includeBaseDirectory: false);
```

When you run this you'll get a similar gzipped tarball to the one produced by the `tar` command!

> Note that the _details_ matter here, so the resulting file may not be the _same_ as the one produced by `tar`. I discuss more about that at the end of the post.

The `includeBaseDirectory` argument specifies whether you want the paths in the tarball to include initial base segments relative to the current working directory. If it was set to `true` in the above example, the paths would be prefixed with `my-files/`.

So we can create `.tar.gz` files using .NET, now lets looks at how to extract them.

## Extracting a `.tar.gz` archive

As I mentioned previously, one of the features of `tar` files is supporting permissions, hard/symbolic links, owners etc. That inevitably means there are a lot of options available to you when you extract an archive, based on what you want to preserve and what you want to ignore for example. For the purposes of this section, I'm only looking at very simple examples.

### Extracting a `.tar.gz` archive using `tar`

To extract an archive into the current working directory with the `tar` utility, you would use a command something like this:

```bash
tar -xzvf ~/my_archive.tar.gz
```

where the options mean:

*   `-x` Extract the archive
*   `-z` Decompress the file with `gzip` before processing
*   `-v` List the files being processed (optional)
*   `-f <FILE>` Output the archive to file `<FILE>`

If you want to output the files to a different directory you need to use an additional argument `-C <DIR>`, for example:

```bash
tar -xzvf ~/my_archive.tar.gz -C /path/to/dir
```

Now lets see how we can do this with .NET.

### Extracting a `.tar.gz` archive using .NET

As before, the [the `TarFile` class](https://learn.microsoft.com/en-us/dotnet/api/system.formats.tar.tarfile?view=net-8.0) has a helpful `ExtractToDirectory` method, but once again it works only with `tar` files, not `tar.gz` files that are also compressed. But yet again, we can work around this using the `GZipStream` class, giving very similar code to before:

```csharp
using System.Formats.Tar;

string sourceTar = "./myarchive.tar.gz";
string extractTo = "/path/to/dir";

using FileStream fs = new(sourceTar, FileMode.Open, FileAccess.Read);
using GZipStream gz = new(fs, CompressionMode.Decompress, leaveOpen: true);

TarFile.ExtractToDirectory(gz, extractTo, overwriteFiles: false);
```

The only option available in the .NET code here is `overwriteFiles`; if a file exists during extraction and `overwriteFiles` is not `true`, this throws an `IOException`.

The .NET implementation of extraction generally performs similarly to the `tar` utility, but there are some differences such as [extracting absolute paths](https://github.com/dotnet/runtime/issues/74135) and [preserving ownership](https://github.com/dotnet/runtime/issues/69780) which I'll discuss later.

## Extracting a single file from a `.tar.gz` archive

Sometimes you only want to extract a _single_ file from an archive instead of extracting the whole archive. That's particularly important when you have very large archives that would be difficult or impossible to fully extract.

### Extracting a single file from a `.tar.gz` archive using `tar`

To extract a single file from an archive using `tar`, you can just add the path to the file at the end of the command. The following command extracts the file with the path `./bin/someother.so` _inside_ the archive and writes it to the current directory:

```bash
tar -xzvf ~/my_archive.tar.gz ./bin/someother.so
```

The options for this are the same as described in the full extraction, so I won't repeat them here.

### Extracting a single file from a `.tar.gz` archive using .NET

Unfortunately, we don't have any more high-level helpers for .NET to handle this requirement, so we're going to fallback to using the slightly lower APIs of `TarReader` and `TarEntry`.

In the following code we open an existing `.tar.gz` file as a `FileStream` and decompress it using `GZipStream`, as we have in the previous examples. We then pass this stream to an instance of `TarReader` and iterate through each `TarEntry` it finds. When we find an entry with the correct name, we extract the file and exit.

```csharp
string sourceTar = "./my_archive.tar.gz";
string pathInTar = "./bin/someother.so";
string destination = "./extractedFile.so";

// Open the source tar file, decompress, and pass stream to TarReader
using FileStream fs = new(sourceTar, FileMode.Open, FileAccess.Read);
using GZipStream gz = new(fs, CompressionMode.Decompress, leaveOpen: true);
using var reader = new TarReader(gz, leaveOpen: true);

// Loop through all the entries in the tar
while (reader.GetNextEntry() is TarEntry entry)
{
    // If the entry matches the required path, extract the file
    if (entry.Name == pathInTar)
    {
        Console.WriteLine($"Found '{pathInTar}', extracting to '{destination}");
        entry.ExtractToFile(destination, overwrite: false);
        return; // all done
    }
}

// If we get here, we didn't find the file
```

The `ExtractToFile` helper can extract both files and directories, but it won't extract symbolic links or hard links; those are only extracted if you extract the _whole_ archive.

## Listing all the files in a `.tar.gz` without extraction

Sometimes you don't actually need to extract anything from the file, you just want to look at the files contained inside. This section shows how to do that both with `tar` and using .NET.

### Listing all the files in a `.tar.gz` using `tar`

To list all the files in an archive using `tar`, you can use the following:

```bash
tar -tzvf ~/myarchive.tar.gz
```

Most of these options

*   `-t` List the contents of an archive
*   `-z` Decompress the file with `gzip` before processing
*   `-v` List the files verbosely (optional)
*   `-f <FILE>` Output the archive to file `<FILE>`

The `-v` option is not required, but adding it outputs additional information about each entry, similar to `ls -l`:

```bash
drwxr-xr-x root/root         0 2024-08-11 16:02 ./
lrwxrwxrwx root/root         0 2024-08-11 16:01 ./myapp.so -> ./bin/myapp.so
drwxr-xr-x root/root         0 2024-08-11 15:57 ./docs/
-rw-r--r-- root/root        10 2024-08-11 15:57 ./docs/README
-rw-r--r-- root/root   6027280 2024-08-11 15:57 ./docs/someother.xml
-rw-r--r-- root/root   1443232 2024-08-11 15:56 ./someother.so
drwxr-xr-x root/root         0 2024-08-11 16:00 ./bin/
-rw-r--r-- root/root   2399608 2024-08-11 15:55 ./bin/myapp.so
hrw-r--r-- root/root         0 2024-08-11 15:56 ./bin/someother.so link to ./someother.so
```

You can read the full spec for `ls -l` [here](https://pubs.opengroup.org/onlinepubs/9699919799/utilities/ls.html) but in summary, this shows:

*   The type of entry (`d` for directory, `-` for file, `l` for symbolic link, `h` for hard link)
*   The [permissions](https://en.wikipedia.org/wiki/Unix_file_types#Representations) for the entry
*   The owner
*   The size of the entry (in bytes)
*   The modification time
*   The path (and link location for symbolic and hard links)

### Listing all the files in a `.tar.gz` using .NET

As you might expect, there's no built-in method helper for printing this information with .NET. Writing one is a little annoying, but not very difficult; all the information contained in the tar entry is exposed on `TarEntry`.

The following code _mostly_ emulates the display format of `tar`'s `-tzvf` format shown above:

```csharp
using System.Formats.Tar;
using System.Globalization;
using System.IO.Compression;

string sourceTar = "./myarchive.tar.gz"

// read the tar and loop through the entries
using FileStream fs = new(sourceTar, FileMode.Open, FileAccess.Read);
using GZipStream gz = new(fs, CompressionMode.Decompress, leaveOpen: true);
using var reader = new TarReader(gz, leaveOpen: true);

while (reader.GetNextEntry() is TarEntry entry)
{
    // Get the file descriptor
    char type = entry.EntryType switch
    {
        TarEntryType.Directory => 'd',
        TarEntryType.HardLink => 'h',
        TarEntryType.SymbolicLink => 'l',
        _ => '-',
    };

    // Construct the permissions e.g. rwxr-xr-x
    // Moved to a separate function just because it's a bit verbose
    string permissions = GetPermissions(entry);

    // Display the owner info. 0 is special (root) but .NET doesn't
    // expose the mappings for these IDs natively, so ignoring for now 
    string ownerUser = entry.Uid == 0 ? "root" : entry.Uid.ToString(CultureInfo.InvariantCulture);
    string ownerGroup = entry.Gid == 0 ? "root" : entry.Gid.ToString(CultureInfo.InvariantCulture);

    // The length of the data and the modification date in bytes
    long size = entry.Length;
    DateTimeOffset date = entry.ModificationTime.UtcDateTime;

    // Match the display format used by tar -tzvf 
    string path = entry.EntryType switch
    {
        TarEntryType.HardLink => $"{entry.Name} link to {entry.LinkName}",
        TarEntryType.SymbolicLink => $"{entry.Name} -> {entry.LinkName}",
        _ => entry.Name,
    };

    // Write the entry!
    Console.WriteLine($"{type}{permissions} {ownerUser}/{ownerGroup} {size,9} {date:yyyy-MM-dd hh:mm} {path}");
}

// Construct the permissions
static string GetPermissions(TarEntry entry)
{
    var userRead = entry.Mode.HasFlag(UnixFileMode.UserRead) ? 'r' : '-';
    var userWrite = entry.Mode.HasFlag(UnixFileMode.UserWrite) ? 'w' : '-';
    var userExecute = entry.Mode.HasFlag(UnixFileMode.UserExecute) ? 'x' : '-';
    var groupRead = entry.Mode.HasFlag(UnixFileMode.GroupRead) ? 'r' : '-';
    var groupWrite = entry.Mode.HasFlag(UnixFileMode.GroupWrite) ? 'w' : '-';
    var groupExecute = entry.Mode.HasFlag(UnixFileMode.GroupExecute) ? 'x' : '-';
    var otherRead = entry.Mode.HasFlag(UnixFileMode.OtherRead) ? 'r' : '-';
    var otherWrite = entry.Mode.HasFlag(UnixFileMode.OtherWrite) ? 'w' : '-';
    var otherExecute = entry.Mode.HasFlag(UnixFileMode.OtherExecute) ? 'x' : '-';
    
    return $"{userRead}{userWrite}{userExecute}{groupRead}{groupWrite}{groupExecute}{otherRead}{otherWrite}{otherExecute}";
}
```

When you run the above, you get pretty much the same output as `tar -tzvf`:

```bash
drwxr-xr-x root/root         0 2024-08-11 15:02 ./
lrwxrwxrwx root/root         0 2024-08-11 15:01 ./myapp.so -> ./bin/myapp.so
drwxr-xr-x root/root         0 2024-08-11 14:57 ./docs/
-rw-r--r-- root/root        10 2024-08-11 14:57 ./docs/README
-rw-r--r-- root/root   6027280 2024-08-11 14:57 ./docs/someother.xml
-rw-r--r-- root/root   1443232 2024-08-11 14:56 ./someother.so
drwxr-xr-x root/root         0 2024-08-11 15:00 ./bin/
-rw-r--r-- root/root   2399608 2024-08-11 14:55 ./bin/myapp.so
hrw-r--r-- root/root         0 2024-08-11 14:56 ./bin/someother.so link to ./someother.so
```

Pretty neat 🙂 There's just a couple of things to note here:

*   The owners are stored in the `.tar` archive as IDs of the current user and group. `root` is a well known value (`0`) so we can decode that one easily, but you can't easily get the names of the other users from .NET (you need to invoke `id` or read the `/etc/passwd` file for example).
*   The output of `tar -tzvf` displays modification time in _local_ time, whereas I used UTC because, you know, why not 😄

That covers the main operations I want to talk about in this post.

## Caveats, missing features, and bugs

In the final section of this post I describe some of the limitations and differences from `tar` that I've run into.

### .NET can't create hardlinks in `.tar` archives

One of the biggest problems I ran into (which ended up being a blocker for me to use it) was that .NET currently _can't_ create hardlinks in `tar` archives, unlike the `tar` utility.

[Hardlinks in Linux](https://www.redhat.com/sysadmin/linking-linux-explained) are relatively simple: a hard link is the link between the _filename_ and the actual _data_ of the file. Every file you create starts with one hardlink, but you can create additional hard links, so multiple filenames point to the same underlying data.

> The other type of link is a _symbolic_ link. The advantage of hard links is that they mostly appear like completely normal files to applications, whereas applications need to specifically handle symbolic links.

I wanted to use hardlinks to de-duplicate files inside a `tar` file. The `tar` utility (and archive format) both handle this perfectly well, preserving the hard link, but .NET currently does not preserve the link when creating an archive. Any hardlinks will be _duplicated_ as additional data in the resulting `.tar` file, increasing the size of the archive _and_ the size of the expanded data after extraction.

You can see this in practice comparing an archive created using `tar` directly compared to .NET when the directory contains a hard link:

```bash
# For the `tar` utility 👇
$ tar -vtzf ./myarchive.tar.gz
drwxr-xr-x root/root         0 2024-08-11 16:02 ./
-rw-r--r-- root/root   1443232 2024-08-11 15:56 ./someother.so
drwxr-xr-x root/root         0 2024-08-11 16:00 ./bin/
hrw-r--r-- root/root         0 2024-08-11 15:56 ./bin/someother.so link to ./someother.so
👆# Note the 'h'

# For the .NET archive 👇
$ tar -vtzf ./myarchive.tar.gz
-rw-r--r-- root/root   1443232 2024-08-11 15:56 someother.so
drwxr-xr-x root/root         0 2024-08-11 16:00 bin/
-rw-r--r-- root/root   1443232 2024-08-11 15:56 bin/someother.so
👆# Normal file, not a hardlink
```

> Note that .NET _will_ preserve any hardlinks in the `.tar` archive when _expanding_ an archive. It just can't currently _create_ those hardlinks in the `.tar` archive in the first place.

There's already [a two year old issue about the behaviour](https://github.com/dotnet/runtime/issues/74404), but it's not getting much love by the looks of it. Hopefully it does soon 🤞

### .NET can't control ownership during extraction

The `tar` utility has a _huge_ number of options and flags, but one I often use is `--same-owner` (implicitly, by extracting using `sudo`) when I want to make sure that files marked as `root` in the archive _remain_ that way after extraction.

Unfortunately there's no way to do this currently in .NET. You _might_ be able to hack around it yourself by "fixing" the permissions manually, but it really feels like this should just be an explicit built-in option. Speaking of which, there's [an old issue](https://github.com/dotnet/runtime/issues/69780) about adding additional options to the implementation, and controlling the owner/group is one of the explicit missing features mentioned.

### .NET can't handle absolute paths

In general, it's not recommended to use absolute paths in tar files, but you _can_ if you want to. The `tar` utility automatically converts any absolute paths to relative paths, but also provides an option to extract to the "real" path using `--absolute-names`.

> You should be very careful extracting with `--absolute-names` as expanding the tar file could overwrite practically anywhere on your system.

Unfortunately .NET flat out refuses to expand a tar that has absolute paths. Instead it throws an `IOException`:

```bash
Unhandled exception. System.IO.IOException: Extracting the Tar entry '/bin/busybox' would have resulted in a link target outside the specified destination directory: '/tmp/extracted-alpine'
```

There's [an issue raised](https://github.com/dotnet/runtime/issues/74135) about this one too.

In general it feels like what's currently available built in to .NET should be good enough for most simple cases, but unfortunately you're likely to run up against the edges once you break outside the 80% common cases.

## Summary

In this post I described how to perform some of common operations on `.tar.gz` files using the built-in .NET support. I show how to compress a directory into a `.tar.gz` file, how to expand a `.tar.gz` file into a directory, how to extract a single file from the directory, and how to list the contents of the directory without extracting the files. Finally I discuss some of the limitations in the current .NET implementations.

Source code [Example source code for this post](https://github.com/andrewlock/blog-examples/tree/