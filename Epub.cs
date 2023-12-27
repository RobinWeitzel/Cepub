namespace Cepub;

public class Epub
{
  public string Title { get; set; }
  public string Author { get; set; }
  public string Language { get; set; }
  public string Description { get; set; }
  public DateTime Date { get; set; }
  public List<Chapter> Chapters { get; set; } = new List<Chapter>();

  public void AddChapter(Chapter chapter)
  {
    Chapters.Add(chapter);
  }

  public void AddChapter(string title, string content)
  {
    Chapters.Add(new Chapter { Title = title, Content = content });
  }
  
  public void Save(string outputPath, string filename)
  {
    EpubWriter.CreateEpub(outputPath, filename, this);
  }
}
