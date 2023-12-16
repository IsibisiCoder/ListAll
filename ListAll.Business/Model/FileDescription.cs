using System.Diagnostics.CodeAnalysis;

namespace ListAll.Business.Model;

[ExcludeFromCodeCoverage]
public class FileDescription
{
    public string Filename { get; set; } = string.Empty;

    public string FileExtention { get; set; } = string.Empty;

    public string Path { get; set; } = string.Empty;

    public long FileSize { get; set; } = 0L;

    public DateTime CreationTime { get; set; }

    public List<string>? Directories { get; set; }
}