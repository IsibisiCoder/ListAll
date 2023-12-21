using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Localization;
using CommandLine;
using ListAll.Business.Services;

//TODO:
// Sort
// weitere Unittests (Auslagerung aus Process / internal)
// -n onlyDir => Größe des Unterverzeichnisses
// param => fehler prüfen, Param prüfen
// option Video
// option Audio
// freier Speicherplatz auf SystemDrive prüfen
// SetParameter zusätzlich als Long, Bool, IEnumable<string> ins Interface
// Modul auswählen, ggf. Video und Audio als eigenes Modul/Bibliothek

namespace ListAll;

internal class Program
{
    public class Options
    {
        [Option('o', "outputfile", Required = true, HelpText = "Outputfilename", ResourceType = typeof(Resources.ProgramParameter))]
        public required string Outputfilename { get; set; }

        [Option('s', "setting", Required = false, Default = false, HelpText = "Setting", ResourceType = typeof(Resources.ProgramParameter))]
        public string? Setting { get; set; }

        [Option('d', "rootdir", Required = true, HelpText = "Rootdir", ResourceType = typeof(Resources.ProgramParameter))]
        public required string RootDir { get; set; }

        [Option('e', "extensions", Required = false, HelpText = "Extensions", ResourceType = typeof(Resources.ProgramParameter))]
        public IEnumerable<string>? Extensions { get; set; }

        [Option('r', "recursive", Required = false, Default = false, HelpText = "Recursive", ResourceType = typeof(Resources.ProgramParameter))]
        public bool Recursive { get; set; } = false;
    }

    static void RunOptions(Options opts)
    {
        var e = opts.Outputfilename;
        //handle options
    }
    static void HandleParseError(IEnumerable<Error> errs)
    {
        var result = -2;
        Console.WriteLine("errors {0}", errs.Count());
        if (errs.Any(x => x is HelpRequestedError || x is VersionRequestedError))
            result = -1;
        Console.WriteLine("Exit code {0}", result);
    }

    static void Main(string[] args)
    {
        //--outputfile="c:\temp-ulf\o.csv" --rootdir="c:\temp" --extensions="txt,mp4"

        ParserResult<Options> parserResult = Parser.Default.ParseArguments<Options>(args)
        .WithParsed(RunOptions)
        .WithNotParsed(HandleParseError);

        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSingleton<ListAll.Plugin.ListDirectories.ListDirectories>();
        builder.Services.AddTransient<IFileService, FileService>();

        builder.Services.AddLogging();
        builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

        builder.Logging.ClearProviders();
        builder.Logging.SetMinimumLevel(LogLevel.Debug);
        builder.Logging.AddConsole();
        builder.Logging.AddDebug();
        builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));

        builder.Configuration.Sources.Clear();
        builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        builder.Configuration.AddEnvironmentVariables();

        var app = builder.Build();

        IConfiguration? config = app?.Services.GetRequiredService<IConfiguration>();
        ILogger<Program>? logger = app?.Services.GetService<ILogger<Program>>();
        IStringLocalizer<Program>? localizer = app?.Services.GetService<IStringLocalizer<Program>>();
        ILoggerFactory? loggerFactory = app?.Services.GetService<ILoggerFactory>();

        var log4NetFilename = config?["ListAll:Log4NetConfigFilename"];

        if (!string.IsNullOrEmpty(log4NetFilename))
        {
            loggerFactory.AddLog4Net(log4NetFilename);
            logger?.LogDebug($"{ localizer!["UseLog4netFilename"] } {log4NetFilename}");
        }
        else
        {
            loggerFactory.AddLog4Net();
        }

        using (var scope = app?.Services.CreateScope())
        {
            var services = scope?.ServiceProvider;
            var context = services?.GetRequiredService<ListAll.Plugin.ListDirectories.ListDirectories>();
            context?.SetParameter("OutputFile", parserResult.Value.Outputfilename);
            context?.SetParameter("RootDir", parserResult.Value.RootDir);
            context?.SetParameter("SettingPath", parserResult.Value.Setting ?? string.Empty);
            context?.SetParameter("Recursive", parserResult.Value.Recursive.ToString());
            if (parserResult.Value.Extensions != null)
            {
                foreach (var ext in parserResult.Value.Extensions)
                {
                    context?.SetParameter("Extensions", ext);
                }
            }
            context?.Process();
        };
    }

    static string GetArgument(string[] args, string searchedParameter)
    {
        if (string.IsNullOrEmpty(searchedParameter) || args == null || args?.Length < 2)
        {
            return string.Empty;
        }

        foreach (string arg in args!)
        {
            if (arg.Contains(searchedParameter))
            {
                string[] strings = arg.Split("=");
                if (strings.Length == 2)
                {
                    return strings[1];
                }
            }
        }
        return string.Empty;
    }
}
