using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using CommandLine;
using ListAll.Business.Services;

//TODO:
// Sort
// Language / Resource
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
        [Option('o', "outputfile", Required = true, HelpText = "Set outputfilename")]
        public required string Outputfilename { get; set; }

        [Option('s', "setting", Required = false, Default = false, HelpText = "Path of seperate config file with settings")]
        public string? Setting { get; set; }

        [Option('d', "rootdir", Required = true, HelpText = "start directory")]
        public required string RootDir { get; set; }

        [Option('e', "extensions", Required = false, HelpText = "Use only this extensions")]
        public IEnumerable<string>? Extensions { get; set; }

        [Option('r', "recursive", Required = false, Default = false, HelpText = "Use recursive directories")]
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

        var builder = Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                //services.AddHostedService<ListAllService>();
                services.AddSingleton<ListAll.Plugins.Directories.ListDirectories>();
                services.AddTransient<IFileService, FileService>();
                services.AddLogging();
                //services.AddLocalization(options => options.ResourcesPath = "Resources");
                //services.AddDbContext<WebApiMockDbContext>(options => { options.UseSqlite(Configuration.GetConnectionString("SqliteConnection")); });
                //ListDirectories.SetBuilder(services);
            })
            .ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.Sources.Clear();

                configuration
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables();
            })
            .ConfigureLogging((hostContext, logging) =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Debug);
                logging.AddConfiguration(hostContext.Configuration.GetSection("Logging"));
                logging.AddConsole();
                logging.AddDebug();
            });

        var app = builder.Build();

        IConfiguration? config = app?.Services.GetRequiredService<IConfiguration>();
        ILogger<Program>? logger = app?.Services.GetService<ILogger<Program>>();
        ILoggerFactory? loggerFactory = app?.Services.GetService<ILoggerFactory>();

        var log4NetFilename = config?["ListAll:Log4NetConfigFilename"];

        if (!string.IsNullOrEmpty(log4NetFilename))
        {
            loggerFactory.AddLog4Net(log4NetFilename);
            logger?.LogDebug($"use log4Net-Filename: {log4NetFilename}");
        }
        else
        {
            loggerFactory.AddLog4Net();
        }

        //string outputfilename = GetArgument(args, "--outputfile") ?? System.IO.Path.Combine(System.Environment.CurrentDirectory, "output.csv");
        //string outputtype = GetArgument(args, "--outputtype") ?? "csv";
        //string extensionParam = GetArgument(args, "--extensions") ?? "*";
        //List<string> extensions = extensionParam.Split(',').ToList();
        //string rootDir = GetArgument(args, "--rootdir");

        using (var scope = app?.Services.CreateScope())
        {
            var services = scope?.ServiceProvider;
            var context = services?.GetRequiredService<ListAll.Plugins.Directories.ListDirectories>();
            //context?.SetParameter("OutputFile", outputfilename);
            //context?.SetParameter("Outputtype", outputtype);
            //context?.SetParameter("RootDir", rootDir);
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
