﻿using ListAll.Business.Model;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Runtime;
using System.Text;
using System.Text.Json;

namespace ListAll.Business.Services;

public class FileService : IFileService
{
    //private ILogger _logger;

    public FileService()
    {
        //ILogger logger
        //_logger = logger;
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
            return false;
        }

        bool directoryExists = Directory.Exists(dirName);
        if (!directoryExists && createIfNotExists)
        {
            try
            {
                Directory.CreateDirectory(dirName);
                directoryExists = true;
            }
            catch (Exception ex)
            {
                //_logger.LogWarning(ex, "directory can not create");
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

        var files = new List<FileDescription>();

        string[] directories = GetDirectories(path);

        foreach (var dir in directories)
        {
            DirectoryInfo dirInfo = new DirectoryInfo(path);

            var fd = new FileDescription
            {
                Filename = dirInfo.Name,
                FileExtention = string.Empty,
                Path = dirInfo.FullName,
                CreationTime = dirInfo.CreationTime,
                FileSize = 0L
            };
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
    /// <param name="folderRecursive">The directories are recursive save into the list object</param>
    /// <returns></returns>
    /// <exception cref="ArgumentNullException"></exception>
    public List<FileDescription> GetFiles(string path, string extension, bool folderRecursive = false)
    {
        if (string.IsNullOrWhiteSpace(path)) throw new ArgumentNullException(nameof(path));
        if (string.IsNullOrWhiteSpace(extension)) throw new ArgumentNullException(nameof(extension));

        var files = new List<FileDescription>();

        DirectoryInfo dirInfo = new DirectoryInfo(path);

        FileInfo[] fileInfo = dirInfo.GetFiles(extension);

        foreach (FileInfo info in fileInfo)
        {
            var ext = info.Extension;
            if (ext.StartsWith("."))
            {
                ext = ext.Substring(1);
            }

            List<string>? allDirectories;

            if (folderRecursive)
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
                Directories = allDirectories
            };
            files.Add(fd);
        }
        return files;
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
        //only for test
        //ms = new MemoryStream();
        //return new StreamWriter(ms);
    }
}