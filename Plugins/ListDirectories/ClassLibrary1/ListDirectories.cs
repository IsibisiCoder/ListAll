using System.Text;
using ListAll.Business.Services;
using ListAll.Business.Model;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.Extensions.Options;
using System.Xml.Linq;
using System.Runtime.CompilerServices;
using System;

[assembly: InternalsVisibleTo("ListDirectories.Tests")]
namespace ListAll.Plugins.Directories;

public class ListDirectories : IPlugin
{
    internal string OutputFile { get; set; } = string.Empty;

    internal string Outputtype { get; set; } = string.Empty;

    internal string RootDir { get; set; } = string.Empty;

    internal bool Recursive { get; set; } = false;

    internal bool OnlyDir { get; set; } = false;

    internal bool FolderRecursive { get; set; } = false;

    internal bool FilenameWithExtension { get; set; } = false;

    internal List<string> Extensions { get; set; } = new List<string>();

    internal string SettingPath { get; set; } = @"Settings/list-directory.json";

    internal string OutputFormat { get; set; } = string.Empty;

    internal List<string> Headers { get; set; } = new List<string>();

    private IFileService _fileService;

    //private ILogger _logger;


    //private static ListDirectories _listDirectories = null!;

    /*public static void SetBuilder(IServiceCollection services)
    {
        //_listDirectories = new ListDirectories();
        //services.AddTransient<IPlugin, ListDirectories>();
    }*/

    public ListDirectories(IFileService fileService)
    {
        //, ILogger logger
        ArgumentNullException.ThrowIfNull(fileService);

        _fileService = fileService;
        //_logger = logger;
    }


    /// <summary>
    /// <Set one parameter
    /// </summary>
    /// <param name="paramName"></param>
    /// <param name="paramValue"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void SetParameter(string paramName, string paramValue)
    {
        if (string.IsNullOrWhiteSpace(paramName)) throw new ArgumentNullException(paramName);
        if (string.IsNullOrWhiteSpace(paramValue)) throw new ArgumentNullException(paramValue);

        if (paramName == $"{nameof(OutputFile)}") OutputFile = paramValue;
        else if (paramName == $"{nameof(Outputtype)}") Outputtype = paramValue;
        else if (paramName == $"{nameof(RootDir)}") RootDir = paramValue;
        else if (paramName == $"{nameof(SettingPath)}") SettingPath = paramValue;
        else if (paramName == $"{nameof(Recursive)}")
        {
            bool.TryParse(paramValue, out bool rec);
            Recursive = rec;
        }
        else if (paramName == $"{nameof(OnlyDir)}")
        {
            bool.TryParse(paramValue, out bool od);
            OnlyDir = od;
        }
        else if (paramName == $"{nameof(Extensions)}")
        {
            if (paramValue.Length >= 2)
            {
                if (paramValue.StartsWith(".")) paramValue = "*" + paramValue;
                else if (!paramValue.Contains("."))
                {
                    paramValue = "*." + paramValue;
                }
            }
            else if (paramValue.Length == 0) paramValue = "*.";
            Extensions.Add(paramValue);
        }
    }

    public void Process()
    {
        if (string.IsNullOrWhiteSpace(OutputFile)) throw new ArgumentNullException(OutputFile);
        if (string.IsNullOrWhiteSpace(RootDir)) throw new ArgumentNullException(RootDir);

        _fileService.DirectoryExists(OutputFile, true);

        GetConfiguration();

        var allFiles = new List<FileDescription>();

        try
        {
            GetFiles(RootDir, allFiles);

            //allFiles.Sort();

            var outputFileExists = _fileService.FileExists(OutputFile);

            using (StreamWriter sr = _fileService.GetStreamWriter (OutputFile, true, Encoding.UTF8))
            {
                WriteFileDescription(allFiles, sr, outputFileExists);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
        }
    }

    private void WriteFileDescription(List<FileDescription> allFiles, StreamWriter sr, bool outputFileExists)
    {
        string col;
        if (!outputFileExists)
        {
            foreach (var header in Headers)
            {
                sr.WriteLine(header);
            }

        }
        foreach (FileDescription desc in allFiles)
        {
            col = OutputFormat;

            var fieldname = $"{ nameof(desc.Filename) }";
            var filename = desc.Filename;
            if (FilenameWithExtension)
            {
                filename += desc.FileExtention;
            }
            col = col.Replace($"{fieldname}", filename);

            fieldname = $"{nameof(desc.FileExtention)}";
            col = col.Replace($"{fieldname}", desc.FileExtention);

            fieldname = $"{nameof(desc.FileSize)}";
            col = col.Replace($"{fieldname}", desc.FileSize.ToString());

            fieldname = $"{nameof(desc.CreationTime)}";
            col = col.Replace($"{fieldname}", desc.CreationTime.ToString());

            fieldname = $"{nameof(desc.Path)}";
            col = col.Replace($"{fieldname}", desc.Path);

            if (desc.Directories != null)
            {
                int i = 1;
                var directories = desc.Directories;
                foreach (var dir in directories)
                {
                    fieldname = $"folder{i}";
                    col = col.Replace($"{fieldname}", dir);
                    i++;
                }
                for (int y = i; y < 10; y++)
                {
                    fieldname = $"folder{y}";
                    if (col.Contains(fieldname))
                    {
                        col = col.Replace($"{fieldname}", "");
                    }
                }
            }

            sr.WriteLine(col);
        }
    }

    private void GetFiles(string directory, List<FileDescription> allFiles)
    {
        try
        {
            foreach (string ext in Extensions)
            {
                if (!_fileService.DirectoryExists(directory))
                {
                    Console.WriteLine("Das Verzeichnis " + directory + " existiert nicht");
                }
                else
                {
                    if (!OnlyDir)
                    {
                        var files = _fileService.GetFiles(directory, ext, FolderRecursive);
                        allFiles.AddRange(files);
                    }
                }
            }

            if (OnlyDir)
            {
                var files = _fileService.GetFileDescriptionOfDirectories(directory);
                allFiles.AddRange(files);
            }

            if (Recursive)
            {
                var directories = _fileService.GetDirectories(directory);

                foreach (string nextdirectory in directories)
                {
                    GetFiles(nextdirectory, allFiles);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Write(ex.ToString());
        }
    }

    internal void GetConfiguration()
    {
        var a = _fileService.ReadSettings(SettingPath);
        using (JsonDocument document = _fileService.ReadSettings(SettingPath))
        {
            JsonElement element = document.RootElement;
            OnlyDir = element.GetProperty("onlydir").GetBoolean();
            FilenameWithExtension = element.GetProperty("filename-with-extension").GetBoolean();
            OutputFormat = element.GetProperty("output").GetString() ?? string.Empty;
            FolderRecursive = element.GetProperty("folder-recursive").GetBoolean();
            var extensions = element.GetProperty("extensions").GetString()?.Split(' ')?.ToList();
            if (extensions != null)
            {
                foreach (var ext in extensions)
                {
                    SetParameter("Extensions", ext);
                }
            }

            foreach (JsonElement headerElement in element.GetProperty("headers").EnumerateArray())
            {
                var value = headerElement.GetProperty("header").GetString();
                if (value != null)
                {
                    Headers.Add(value);
                }
            }
        }
    }
}
