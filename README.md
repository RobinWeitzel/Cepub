# Cepub Library

## Introduction

Cepub is a library for creating EPUB files in C#. It provides a simple and efficient way to generate EPUB files programmatically.

## Installation

To use the Cepub library in your project, add a reference to the `Cepub.csproj` in your project file.

## Usage

Here's a basic example of how to use the Cepub library:

```csharp
using Cepub;

Epub epub = new Epub();
epub.Title = "Test Book";
epub.Author = "Test Author";
epub.Date = DateTime.Now;
epub.AddChapter("Chapter 1", "This is the content of chapter 1");
epub.AddChapter("Chapter 2", "This is the content of chapter 2");

epub.Save(".", "test");
```

## License
This project is licensed under the terms of the License file.

## Contributing
Contributions are welcome! Please feel free to submit a pull request.