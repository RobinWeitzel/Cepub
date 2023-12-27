namespace Cepub;

using System;
using System.Reflection;
using System.IO;
using System.IO.Compression;

public static class EpubWriter
{
  private static void CreateMimeType(string outputPath)
  {
    string mimetypePath = Path.Combine(outputPath, "mimetype");
    File.WriteAllText(mimetypePath, "application/epub+zip");
  }

  private static void CreateMetaInf(string outputPath)
  {
    string metaInfPath = Path.Combine(outputPath, "META-INF");

    string content = @"<?xml version='1.0' encoding='utf-8'?>
<container xmlns=""urn:oasis:names:tc:opendocument:xmlns:container"" version=""1.0"">
  <rootfiles>
    <rootfile media-type=""application/oebps-package+xml"" full-path=""OEBPS/content.opf""/>
  </rootfiles>
</container>";

    string containerXmlPath = Path.Combine(metaInfPath, "container.xml");
    File.WriteAllText(containerXmlPath, content);
  }

  private static void CreateContent(string outputPath,string description, string date, string version, string bookid, string booktitle, string language, string author, List<Tuple<string, string>> chapters)
  {
    string oebpsPath = Path.Combine(outputPath, "OEBPS");

    string content = $@"<?xml version='1.0' encoding='utf-8'?>
<package xmlns=""http://www.idpf.org/2007/opf"" unique-identifier=""id"" version=""3.0"" prefix=""rendition: http://www.idpf.org/vocab/rendition/#"">
  <metadata xmlns:dc=""http://purl.org/dc/elements/1.1/"" xmlns:opf=""http://www.idpf.org/2007/opf"">
    <meta property=""dcterms:modified"">{date}</meta>
    <meta name=""generator"" content=""Cepub-{version}""/>
    <dc:identifier id=""id"">{bookid}</dc:identifier>
    <dc:description>{description}</dc:description>
    <dc:title>{booktitle}</dc:title>
    <dc:language>{language}</dc:language>
    <dc:creator id=""creator"">{author}</dc:creator>
  </metadata>
  <manifest>
    {string.Join("\n", chapters.Select((chapter, index) => $@"<item href=""{chapter.Item1}"" id=""chapter_{index}"" media-type=""application/xhtml+xml""/>"))}
    <item href=""toc.ncx"" id=""ncx"" media-type=""application/x-dtbncx+xml""/>
    <item href=""nav.xhtml"" id=""nav"" media-type=""application/xhtml+xml"" properties=""nav""/>
  </manifest>
  <spine toc=""ncx"">
    <itemref idref=""nav""/>
    {string.Join("\n", chapters.Select((chapter, index) => $@"<itemref idref=""chapter_{index}""/>"))}
  </spine>
</package>
";

    string contentOpfPath = Path.Combine(oebpsPath, "content.opf");
    File.WriteAllText(contentOpfPath, content);
  }

  private static void CreateNav(string outputPath, string title, List<Tuple<string, string>> chapters)
  {
    string oebpsPath = Path.Combine(outputPath, "OEBPS");
    Directory.CreateDirectory(oebpsPath);

    string content = $@"<?xml version='1.0' encoding='utf-8'?>
<!DOCTYPE html>
<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:epub=""http://www.idpf.org/2007/ops"" lang=""en"" xml:lang=""en"">
  <head>
    <title>{title}</title>
  </head>
  <body>
    <nav epub:type=""toc"" id=""id"" role=""doc-toc"">
      <h2>{title}</h2>
      <ol>
        {string.Join("\n", chapters.Select((chapter, index) => $@"<li><a href=""{chapter.Item1}"">{chapter.Item2}</a></li>"))}
      </ol>
    </nav>
  </body>
</html>
";

    string navXhtmlPath = Path.Combine(oebpsPath, "nav.xhtml");
    File.WriteAllText(navXhtmlPath, content);
  }

  private static void CreateToc(string outputPath, string title, List<Tuple<string, string>> chapters, string depth, string totalPageCount, string maxPageNumber)
  {
    string oebpsPath = Path.Combine(outputPath, "OEBPS");

    string content = $@"<?xml version='1.0' encoding='utf-8'?>
<ncx xmlns=""http://www.daisy.org/z3986/2005/ncx/"" version=""2005-1"">
  <head>
    <meta content=""{title}"" name=""dtb:uid""/>
    <meta content=""{depth}"" name=""dtb:depth""/>
    <meta content=""{totalPageCount}"" name=""dtb:totalPageCount""/>
    <meta content=""{maxPageNumber}"" name=""dtb:maxPageNumber""/>
  </head>
  <docTitle>
    <text>{title}</text>
  </docTitle>
  <navMap>
  {string.Join("\n", chapters.Select((chapter, index) => $@"<navPoint id=""chapter_{index}"" playOrder=""{index + 1}"">
      <navLabel>
        <text>{chapter.Item2}</text>
      </navLabel>
      <content src=""{chapter.Item1}""/>
    </navPoint>"))}
  </navMap>
</ncx>
";
    string tocNcxPath = Path.Combine(oebpsPath, "toc.ncx");
    File.WriteAllText(tocNcxPath, content);
  }

  private static void CreateChapter(string outputPath, string chapterPath, string title, string body, string language)
  {
    string oebpsPath = Path.Combine(outputPath, "OEBPS");

    string content = $@"<?xml version='1.0' encoding='utf-8'?>
<!DOCTYPE html>
<html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:epub=""http://www.idpf.org/2007/ops"" epub:prefix=""z3998: http://www.daisy.org/z3998/2012/vocab/structure/#"" lang=""{language}"" xml:lang=""{language}"">
  <head>
    <title>{title}</title>
  </head>
  <body>
  {body}
  </body>
</html>
";
    string chapterXhtmlPath = Path.Combine(oebpsPath, chapterPath);
    File.WriteAllText(chapterXhtmlPath, content);
  }

  public static void CreateEpub(string outputPath, string filename, Epub epub)
  {
    string tempPath = Path.Combine(Path.GetTempPath(), filename);

    string description = epub.Description;
    string date = epub.Date.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
    string version = Assembly.GetEntryAssembly()?.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version ?? "1.0.0";
    string bookid = epub.Title.Replace(" ", "_");
    string booktitle = epub.Title;
    string language = epub.Language;
    string author = epub.Author;
    List<Tuple<string, string>> chapters = epub.Chapters.Select((chapter, index) => new Tuple<string, string>("chapter_" + index + ".xhtml", chapter.Title)).ToList();

    // Create the EPUB file structure
    string metaInfPath = Path.Combine(tempPath, "META-INF");
    string oebpsPath = Path.Combine(tempPath, "OEBPS");

    Directory.CreateDirectory(metaInfPath);
    Directory.CreateDirectory(oebpsPath);

    // Create the mimetype file
    CreateMimeType(tempPath);

    // Create the META-INF directory
    CreateMetaInf(tempPath);

    // Create the content.opf file
    CreateContent(tempPath, description, date, version, bookid, booktitle, language, author, chapters);

    // Create the nav.xhtml file
    CreateNav(tempPath, booktitle, chapters);

    // Create the toc.ncx file
    CreateToc(tempPath, booktitle, chapters, "0", "0", "0");

    // Create the chapter files
    for(int i = 0; i < chapters.Count; i++)
    {
      string title = chapters[i].Item2;
      string body = epub.Chapters[i].Content;

      CreateChapter(tempPath, chapters[i].Item1, title, body, language);
    }

    // Create the EPUB file by compressing the directory
    string epubPath = Path.Combine(outputPath, filename + ".epub");
    ZipFile.CreateFromDirectory(tempPath, epubPath);

    // Clean up the temporary directory
    Directory.Delete(tempPath, true);
  }
}
