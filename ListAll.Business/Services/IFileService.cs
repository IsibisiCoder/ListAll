using ListAll.Business.Model;
using System.Text;
using System.Text.Json;

namespace ListAll.Business.Services;

public interface IFileService
{
    bool DirectoryExists(string path, bool createIfNotExists = false);

    string[] GetDirectories(string path);

    List<FileDescription> GetFileDescriptionOfDirectories(string path);

    bool FileExists(string path);

    List<FileDescription> GetFiles(string path, string extension, bool folderRecursive = false, bool getMd5 = false);

    JsonDocument ReadSettings(string settingPath);

    StreamWriter GetStreamWriter(string path, bool append, Encoding encoding);
}
