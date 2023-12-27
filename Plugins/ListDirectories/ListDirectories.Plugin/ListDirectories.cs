using System.Text;
using ListAll.Business.Services;
using ListAll.Business.Model;
using System.Text.Json;
using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.DependencyInjection;

[assembly: InternalsVisibleTo("Plugin.ListDirectories.Tests")]
namespace ListAll.Plugin.ListDirectories;

public class ListDirectories : IPlugin
{
    internal string OutputFile { get; set; } = string.Empty;

    internal string Outputtype { get; set; } = string.Empty;

    internal string RootDir { get; set; } = string.Empty;

    internal List<string> SourceDirectories { get; set; } = new List<string>();

    internal bool Recursive { get; set; } = false;

    internal int RecursiveDepth { get; set; } = -1;

    internal bool OnlyDir { get; set; } = false;

    internal bool FolderReverse { get; set; } = false;

    internal bool Md5Hash { get; set; } = false;

    internal string MediaPlugin { get; set; } = string.Empty;

    internal bool FilenameWithExtension { get; set; } = false;

    internal List<string> Extensions { get; set; } = new List<string>();

    internal string SettingPath { get; set; } = @"Settings/list-directory.json";

    internal string OutputFormat { get; set; } = string.Empty;

    internal List<string> Headers { get; set; } = new List<string>();

    private readonly IFileService _fileService;

    private readonly ILogger _logger;

    private readonly IStringLocalizer<ListDirectories> _localizer;

    private readonly IServiceProvider? _serviceProvider;


    //private static ListDirectories _listDirectories = null!;

    /*public static void SetBuilder(IServiceCollection services)
    {
        //_listDirectories = new ListDirectories();
        //services.AddTransient<IPlugin, ListDirectories>();
    }*/

    public ListDirectories(IFileService fileService, ILogger<ListDirectories> logger, IStringLocalizer<ListDirectories> localizer, IServiceProvider? serviceProvider)
    {
        _fileService = fileService ?? throw new ArgumentNullException($"{nameof(fileService)}");
        _logger = logger ?? throw new ArgumentNullException($"{ nameof(logger) }");
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
        _serviceProvider = serviceProvider;
    }

    /// <summary>
    /// Set one parameter
    /// </summary>
    /// <param name="paramName"></param>
    /// <param name="paramValue"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public void SetParameter(string paramName, string paramValue)
    {
        if (string.IsNullOrWhiteSpace(paramName)) throw new ArgumentNullException(paramName);
        if (string.IsNullOrWhiteSpace(paramValue)) throw new ArgumentNullException(paramValue);

        _logger.LogDebug($"{nameof(SetParameter)} - {nameof(paramName)}: {paramValue}");

        if (paramName == $"{nameof(OutputFile)}") OutputFile = paramValue;
        else if (paramName == $"{nameof(Outputtype)}") Outputtype = paramValue;
        else if (paramName == $"{nameof(RootDir)}")
        {
            RootDir = paramValue;
            SourceDirectories.Add(RootDir);
        }
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
            Extensions.Add(paramValue);
        }
    }

    private bool CheckParameter()
    {
        if (!SourceDirectories.Any())
        {
            //TODO:Translation
            //TODO:Unittest
            Console.WriteLine("No RootDir");
            return false;
        }
        if (string.IsNullOrEmpty(OutputFile))
        {
            //TODO:Translation
            Console.WriteLine("No outputfile");
            return false;
        }
        return true;
    }

    public void Process()
    {
        if (string.IsNullOrWhiteSpace(OutputFile)) throw new ArgumentNullException(OutputFile);
        if (string.IsNullOrWhiteSpace(RootDir)) throw new ArgumentNullException(RootDir);

        if (!CheckParameter())
        {
            return;
        }

        _fileService.DirectoryExists(OutputFile, true);

        GetConfiguration();

        var allFiles = new List<FileDescription>();

        try
        {
            _logger.LogDebug($"{nameof(Process)}: GetFiles RootDir={nameof(SourceDirectories)}");

            IMediaPlugin? mediaPlugin = null;
            if (_serviceProvider != null)
            {
                mediaPlugin = _serviceProvider.GetKeyedService<IMediaPlugin>(MediaPlugin);
                _logger.LogDebug($"MediaPlugin available: {MediaPlugin}");
                if (mediaPlugin == null)
                {
                    _logger.LogWarning($"MediaPlugin not available: {MediaPlugin}");
                }
            }

            /*Func<string, Dictionary<string, string>>? func = null;

            if (mediaPlugin != null)
            {
                func = mediaPlugin.GetProperties;
            }

            GetFiles(RootDir, allFiles, func);*/

            foreach (string dir in SourceDirectories)
            {
                GetFiles(dir, allFiles, mediaPlugin, 0);
            }

            /*var result = allFiles.OrderBy(s => s.Md5Hash)
                .ThenBy(s => s.Filename, StringComparer.CurrentCultureIgnoreCase)
                .ThenBy(s => s.FileExtention)
                .ToList();*/

            _logger.LogInformation($"Finish GetFiles, { allFiles.Count } Files are found.");

            var result = allFiles.OrderBy(s => s.Filename, StringComparer.CurrentCultureIgnoreCase)
                .ToList();

            var outputFileExists = _fileService.FileExists(OutputFile);

            using (StreamWriter sr = _fileService.GetStreamWriter (OutputFile, true, Encoding.UTF8))
            {
                WriteFileDescription(result, sr, outputFileExists);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer["CriticalError"]);
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

            var fieldname = $"%{nameof(desc.Filename)}%";
            var filename = desc.Filename;
            if (FilenameWithExtension)
            {
                filename += desc.FileExtention;
            }
            col = col.Replace($"{fieldname}", filename);

            fieldname = $"%{nameof(desc.FileExtention)}%";
            col = col.Replace($"{fieldname}", desc.FileExtention);

            fieldname = $"%{nameof(desc.FileSize)}%";
            col = col.Replace($"{fieldname}", desc.FileSize.ToString());

            fieldname = $"%{nameof(desc.CreationTime)}%";
            col = col.Replace($"{fieldname}", desc.CreationTime.ToString());

            fieldname = $"%{nameof(desc.Path)}%";
            col = col.Replace($"{fieldname}", desc.Path);

            fieldname = $"%{nameof(desc.Md5Hash)}%";
            col = col.Replace($"{fieldname}", desc.Md5Hash);

            if (desc.Directories != null)
            {
                int i = 1;
                var directories = desc.Directories;
                foreach (var dir in directories)
                {
                    fieldname = $"%folder{i}%";
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

            if (desc.Properties != null && desc.Properties.Count > 0)
            {
                foreach (var prop in desc.Properties)
                {
                    fieldname = prop.Key;
                    col = col.Replace($"%{fieldname}%", prop.Value);
                }
            }

            //_logger.LogDebug($" Write column: {col}");

            sr.WriteLine(col);
        }
    }

    //private void GetFiles(string directory, List<FileDescription> allFiles, Func<string, Dictionary<string, string>>? getPropertiesFunc/*, IMediaPlugin? mediaPlugin*/)
    private void GetFiles(string directory, List<FileDescription> allFiles, IMediaPlugin? mediaPlugin, int recursiveLevel)
    {
        _logger.LogInformation($"GetFiles Dir: { directory }");

        if (!_fileService.DirectoryExists(directory))
        {
            string outputText = string.Format(_localizer["DirectoryNotExists"], directory);
            _logger.LogWarning(outputText);
            return;
        }

        try
        {
            if (!OnlyDir)
            {
                //var files = _fileService.GetFiles(directory, ext, FolderReverse, Md5Hash, getPropertiesFunc /*mediaPlugin*/);
                var files = _fileService.GetFiles(directory, Extensions, FolderReverse, Md5Hash, mediaPlugin);
                allFiles.AddRange(files);
            }
            else
            {
                var files = _fileService.GetFileDescriptionOfDirectories(directory);
                allFiles.AddRange(files);
            }

            if (Recursive && (RecursiveDepth == -1 || recursiveLevel < RecursiveDepth))
            {
                var directories = _fileService.GetDirectories(directory);

                foreach (string nextdirectory in directories)
                {
                    //GetFiles(nextdirectory, allFiles, getPropertiesFunc);
                    GetFiles(nextdirectory, allFiles, mediaPlugin, recursiveLevel+1);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, _localizer["CriticalError"]);
        }
    }

    internal void GetConfiguration()
    {
        var a = _fileService.ReadSettings(SettingPath);
        using (JsonDocument document = _fileService.ReadSettings(SettingPath))
        {
            JsonElement value;
            JsonElement element = document.RootElement;
            //OnlyDir = element.GetProperty("onlydir").GetBoolean();
            //FilenameWithExtension = element.GetProperty("filename-with-extension").GetBoolean();
            //OutputFormat = element.GetProperty("output").GetString() ?? string.Empty;
            //FolderReverse = element.GetProperty("folder-reverse").GetBoolean();
            //Md5Hash = element.GetProperty("md5-hash").GetBoolean();
            if (element.TryGetProperty("onlydir", out value)) OnlyDir = value.GetBoolean();
            if (element.TryGetProperty("filename-with-extension", out value)) FilenameWithExtension = value.GetBoolean();
            if (element.TryGetProperty("output", out value)) OutputFormat = value.GetString() ?? string.Empty;
            if (element.TryGetProperty("folder-reverse", out value)) FolderReverse = value.GetBoolean();
            if (element.TryGetProperty("md5-hash", out value)) Md5Hash = value.GetBoolean();
            if (element.TryGetProperty("recursive", out value)) Recursive = value.GetBoolean();
            if (element.TryGetProperty("recursive-depth", out value)) RecursiveDepth = value.GetInt16();
            if (element.TryGetProperty("mediaplugin", out value)) MediaPlugin = value.GetString() ?? string.Empty;
            if (element.TryGetProperty("outputfile", out value)) OutputFile = value.GetString() ?? string.Empty;

            //MediaPlugin = element.GetProperty("mediaplugin").GetString() ?? string.Empty;
            //            var extensions = element.GetProperty("extensions").GetString()?.Split(' ')?.ToList();
            if (element.TryGetProperty("extensions", out value))
            {
                var extensions = value.GetString()?.Split(' ')?.ToList();
                if (extensions != null)
                {
                    foreach (var ext in extensions)
                    {
                        SetParameter("Extensions", ext);
                    }
                }
            }

            if (element.TryGetProperty("headers", out var valueArray))
            {
                foreach (JsonElement headerElement in valueArray.EnumerateArray())
                {
                    if (headerElement.TryGetProperty("header", out var headerValue))
                    {
                        string? header = headerValue.GetString();
                        if (header != null)
                        {
                            Headers.Add(header);
                        }
                    }
                }
            }

            if (element.TryGetProperty("source-directories", out valueArray))
            {
                foreach (JsonElement headerElement in valueArray.EnumerateArray())
                {
                    if (headerElement.TryGetProperty("directory", out var headerValue))
                    {
                        string? header = headerValue.GetString();
                        if (header != null)
                        {
                            SourceDirectories.Add(header);
                        }
                    }
                }
            }

        }
    }
}
