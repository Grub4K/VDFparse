using Microsoft.Win32;
using VDFparse;
using CommandLine;
using System.Text.Json;

namespace VDFParse.Cli;

class Options
{
    [Value(0, Required = true, MetaName = "file", HelpText = "The file to read from or `appinfo`/`packageinfo`.")]
    public string Path { get; set; } = null!;

    [Value(1, MetaName = "id", HelpText = "The ID of the item to query.")]
    public IEnumerable<uint> Ids { get; set; } = null!;

    [Option('i', "info", HelpText = "Write only info about the specified item or file.")]
    public bool Info { get; set; }
}

static class Program
{
    static int Main(string[] args)
    {
        return Parser.Default
            .ParseArguments<Options>(args)
            .MapResult(RunOptions, (_) => 1);
    }

    static int RunOptions(Options opts)
    {
        // Automatically determine path of default files
        if (opts.Path == "appinfo" || opts.Path == "packageinfo")
        {
            string? steamPath;
            try
            {
                steamPath = GetSteamPath();
                if (steamPath is null)
                {
                    Console.Error.WriteLine("Cannot find Steam.");
                    return 2;
                }
            }
            catch (PlatformNotSupportedException)
            {
                Console.Error.WriteLine("Platform unsupported for automated Steam locating.");
                return 3;
            }
            if (opts.Path == "appinfo")
            {
                opts.Path = Path.Combine(steamPath, "appcache", "appinfo.vdf");
            }
            else
            {
                opts.Path = Path.Combine(steamPath, "appcache", "packageinfo.vdf");
            }
        }

        VDFFile vdfFile;
        try
        {
            vdfFile = VDFFile.Read(opts.Path);
        }
        catch
        {
            Console.Error.WriteLine($"Error while trying to parse \"{opts.Path}\"");
            return 4;
        }

        // Output the right data
        dynamic data;
        if (opts.Ids is not null)
        {
            data = new Dictionary<uint, dynamic>();

            var ids = opts.Ids.ToHashSet();
            foreach (var dataset in vdfFile.Datasets)
            {
                if (ids.Contains(dataset.Id))
                {
                    if (opts.Info)
                        dataset.Data.Clear();

                    data[dataset.Id] = dataset;
                    ids.Remove(dataset.Id);
                }
            }
            if (ids.Count != 0)
            {
                var remaining = String.Join(", ", ids);
                Console.Error.WriteLine($"Could not find entries with id(s): {remaining}");
                return 5;
            }
        }
        else if (opts.Info)
        {
            data = new
            {
                Path = opts.Path,
                EUniverse = vdfFile.EUniverse,
                Length = vdfFile.Datasets.Count,
            };
        }
        else
        {
            data = vdfFile;
        }

        var options = new JsonSerializerOptions();
        options.Converters.Add(new BytesToBase64JsonConverter());

        Console.WriteLine(JsonSerializer.Serialize(data, options));
        return 0;
    }

    static string? GetSteamPath()
    {

        if (OperatingSystem.IsWindows())
        {
            return Registry.CurrentUser
                .OpenSubKey("SOFTWARE\\Valve\\Steam")
                ?.GetValue("SteamPath") as string;
        }
        else if (OperatingSystem.IsLinux())
        {
            var home = Environment.GetFolderPath(
                Environment.SpecialFolder.UserProfile);
            var paths = new[] {
                ".steam",
                ".steam/steam",
                ".steam/root",
                ".local/share/Steam",
            };

            foreach (var path in paths)
            {
                var steamPath = Path.Combine(home, path);
                var appCache = Path.Combine(steamPath, "appcache");
                if (Directory.Exists(appCache))
                    return steamPath;
            }
            return null;
        }

        throw new PlatformNotSupportedException();
    }
}
