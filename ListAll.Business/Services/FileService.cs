using ListAll.Business.Model;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ListAll.Business.Services;

public class FileService : IFileService
{
    private readonly ILogger _logger;
    private readonly IStringLocalizer<FileService> _localizer;

    public FileService(ILogger<FileService> logger, IStringLocalizer<FileService> localizer)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
    }

    /// <summary>
    /// Determines, whether the directory exists, if 'createIfNotExists' = true and directory not exists,  create the directory
    /// </summary>
    /// <param name="path">Name and path of the directory</param>
    /// <param name="createIfNotExists">if the directory not exists,  create the directory</param>
    /// <returns>true, if the directory exists. Always true, if the directory are created</returns>
    public bool DirectoryExists(string path, bool createIfNotExists = false)
    {
        var dirName = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(dirName))
        {
            string outputText = string.Format(_localizer["DirectoryNotExists"], dirName);
            _logger.LogDebug(outputText);
            return false;
        }

        DirectoryInfo dirInfo = new DirectoryInfo(path);
        bool directoryExists = dirInfo.Exists;
        if (!directoryExists && createIfNotExists)
        {
            try
            {
                string outputText = string.Format(_localizer["DirectoryShouldBeCreated"], dirName);
                _logger.LogDebug(outputText);

                Directory.CreateDirectory(dirName);
                directoryExists = true;
            }
            catch (Exception ex)
            {
                string outputText = string.Format(_localizer["DirectoryCanNotCreated"], dirName);
                _logger.LogError(ex, outputText);

                directoryExists = false;
            }
        }
        return directoryExists;
    }

    /// <summary>
    /// Returns the names of subdirectories
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public string[] GetDirectories(string path)
    {
        return Directory.GetDirectories(path);
    }

    /// <summary>
    /// return a list of FileDescription of all directories in the specified directory
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public List<FileDescription> GetFileDescriptionOfDirectories(string path)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));

        _logger.LogDebug($"Entry {nameof(GetFileDescriptionOfDirectories)}");

        var files = new List<FileDescription>();

        string[] directories = GetDirectories(path);

        foreach (var dir in directories)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);

            var fd = new FileDescription
            {
                Filename = dirInfo.Name,
                FileExtention = string.Empty,
                Path = dirInfo.FullName,
                CreationTime = dirInfo.CreationTime,
                FileSize = 0L
            };

            string outputText = string.Format(_localizer["AddDirectoryMessage"], fd.Filename);
            _logger.LogInformation(outputText);

            files.Add(fd);
        }
        return files;
    }

    public void DirectoryInfo(string path)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(path);
    }

    /// <summary>
    /// Determines, whether the specific file exists
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public bool FileExists(string path)
    {
        return File.Exists(path);
    }

    /// <summary>
    /// return a list of FileDescription of all files in the specified directory
    /// </summary>
    /// <param name="path"></param>
    /// <param name="extension"></param>
    /// <param name="folderReverse">The directories are reverse save into the list object</param>
    /// <param name="getMd5">Get MD5-Hash from the file</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    //public List<FileDescription> GetFiles(string path, string extension, bool folderReverse = false, bool getMd5 = false, Func<string, Dictionary<string, string>>? getPropertiesFunc = null /*IMediaPlugin? mediaPlugin = null*/)
    public List<FileDescription> GetFiles(string path, List<string> allowedExtensions, bool folderReverse = false, bool getMd5 = false, IMediaPlugin? mediaPlugin = null)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
        if (!allowedExtensions.Any()) throw new ArgumentNullException(nameof(allowedExtensions));

        var files = new List<FileDescription>();

        if (!DirectoryExists(path))
        {
            return files;
        }

        var filesInDirectory = Directory
            .GetFiles(path)
            .Where(file => allowedExtensions.Any(file.ToLower().EndsWith))
            .ToList();

        _logger.LogInformation($"  {nameof(GetFiles)} Dir: { path }: Count: { filesInDirectory.Count }");

        foreach (string filename in filesInDirectory)
        {
            FileInfo info = new FileInfo(filename);

            var ext = info.Extension;
            if (ext.StartsWith("."))
            {
                ext = ext.Substring(1);
            }

            List<string>? allDirectories;

            if (folderReverse)
            {
                allDirectories = info.DirectoryName?.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).Reverse().ToList();
            }
            else
            {
                allDirectories = info.DirectoryName?.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries).ToList();
            }

            var fd = new FileDescription
            {
                Filename = info.Name.Substring(0, info.Name.IndexOf(info.Extension)),
                FileExtention = ext,
                Path = info.DirectoryName ?? string.Empty,
                CreationTime = info.CreationTime,
                FileSize = info.Length,
                Directories = allDirectories,
                Md5Hash = getMd5 ? GetMD5Hash(info.FullName) : string.Empty
            };

            //if (getPropertiesFunc != null)
            if (mediaPlugin != null)
            {
                //var properties = getPropertiesFunc(info.FullName);
                var properties = mediaPlugin.GetProperties(info.FullName);
                if (properties != null && properties.Count > 0)
                {
                    foreach (var prop in properties)
                    {
                        fd.Properties.Add(prop.Key, prop.Value);
                    }
                }
            }

            string outputText = string.Format(_localizer["AddFileMessage"], info.Name);
            _logger.LogDebug(outputText);

            files.Add(fd);
        }
        return files;
    }

    private string GetMD5Hash(string filePath)
    {
        using (var md5 = MD5.Create())
        {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                var hash = md5.ComputeHash(stream);
                var convertedHash = BitConverter.ToString(hash).Replace("-", String.Empty).ToLowerInvariant();
                _logger.LogDebug($"md5 for File {filePath}: {convertedHash}");
                return convertedHash;
            }
        }
    }

    public JsonDocument ReadSettings(string settingPath)
    {
        if (string.IsNullOrEmpty(settingPath)) throw new ArgumentNullException($"{nameof(settingPath)}");

        var settings = File.ReadAllText(settingPath);
        return JsonDocument.Parse(settings);
    }

    /// <summary>
    /// Initialize a new instance of the streamwriter
    /// </summary>
    /// <param name="path"></param>
    /// <param name="append"></param>
    /// <param name="encoding"></param>
    /// <returns></returns>
    public StreamWriter GetStreamWriter(string path, bool append, Encoding encoding)
    {
        return new StreamWriter(path, append, encoding);
    }
}
