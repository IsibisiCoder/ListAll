using ListAll.Business.Model;
using ListAll.Business.Services;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;

namespace ListAll.Business.Tests.FileServiceMocks;

[ExcludeFromCodeCoverage]
public class FileServiceMock : IFileService
{
    public bool DirectoryExists(string path, bool createIfNotExists = false)
    {
        return true;
    }

    public bool FileExists(string path)
    {
        return true;
    }

    public string[] GetDirectories(string path)
    {
        return new string[5]
        {
            "subfolder1",
            "subfolder2",
            "subfolder3",
            "subfolder4",
            "subfolder5"
        };
    }

    public List<FileDescription> GetFileDescriptionOfDirectories(string path)
    {
        return new List<FileDescription>
        {
            new FileDescription
            {
                Filename = "Folder1",
                Path = path,
                CreationTime = new DateTime(2020, 10, 5, 20, 10, 5),
                Directories = new List<string>
                {
                    "subfolder3",
                    "subfolder2",
                    "subfolder1",
                    "temp",
                    "c:"
                }
            }
        };
    }

    public List<FileDescription> GetFiles(string path, string extension, bool folderRecursive = false)
    {
        return new List<FileDescription>
        {
            new FileDescription
            {
                Filename = "File1",
                Path = path,
                FileSize = 1024,
                FileExtention = "txt",
                CreationTime = new DateTime(2020, 10, 5, 20, 10, 5),
                Directories = new List<string>
                {
                    "subfolder3",
                    "subfolder2",
                    "subfolder1",
                    "temp",
                    "c:"
                }
            }
        };
    }

    public StreamWriter GetStreamWriter(string path, bool append, Encoding encoding)
    {
        MemoryStream ms = new MemoryStream(100);
        StreamWriter sw = new StreamWriter(ms, Encoding.UTF8);
        return sw;
    }

    public JsonDocument ReadSettings(string settingPath)
    {
        var jsonSetting = "{\r\n  \"onlydir\": true,\r\n  \"filename-with-extension\": true,\r\n  \"folder-recursive\": true,\r\n  \"extensions\": \"txt\",\r\n  \"output\": \"Filename;FileExtention\",\r\n  \"headers\": [\r\n    {\r\n      \"header\": \"Filename;Ext;Size;Date;Path\"\r\n    },\r\n    {\r\n      \"header\": \"--|--|--|--|--\"\r\n    }\r\n  ]\r\n}";
        var jsonDocument = JsonDocument.Parse(jsonSetting);
        return jsonDocument;
    }
}
