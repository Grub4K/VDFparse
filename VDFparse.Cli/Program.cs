using System.Text;
using Microsoft.Win32;
using VDFparse;

namespace VDFParse.Cli;

static class Program
{
    static int Main(string[] args)
    {
        Console.OutputEncoding = Encoding.UTF8;
        switch (args.ElementAtOrDefault(0))
        {
            case "-v":
            case "--version":
                Console.WriteLine(typeof(Program).Assembly.GetName().Version);
                return 0;
            case "-?":
            case "/?":
            case "-h":
            case "--help":
            case null:
                Console.WriteLine(Translations.Get("HelpMessage"),
                    typeof(Program).Assembly.GetName().Name);
                return 0;
            default:
                break;
        }
        switch (args[0])
        {
            case "list":
            case "info":
                if (args.Length < 2)
                {
                    Console.Error.WriteLine(Translations.Get("ErrorNotEnoughArguments"));
                    return 4;
                }
                break;
            case "json":
                if (args.Length < 3)
                {
                    Console.Error.WriteLine(Translations.Get("ErrorNotEnoughArguments"));
                    return 4;
                }
                break;
            case "query":
                if (args.Length < 4)
                {
                    Console.Error.WriteLine(Translations.Get("ErrorNotEnoughArguments"));
                    return 4;
                }
                break;
            default:
                Console.Error.WriteLine(Translations.Get("ErrorUnknownParameter"), args[0]);
                return 3;
        }
        VDFFile vdfFile = new VDFFile();
        string filePath;
        if (args[1] == "appinfo" || args[1] == "packageinfo")
        {
            string? steamPath;
            try
            {
                steamPath = GetSteamPath();
                Console.WriteLine(steamPath);
                if (steamPath is null)
                {
                    Console.Error.WriteLine(Translations.Get("ErrorCannotFindSteam"));
                    return 1;
                }
            }
            catch (PlatformNotSupportedException)
            {
                Console.Error.WriteLine(Translations.Get("ErrorPlatformUnsupported"));
                return 2;
            }
            if (args[1] == "appinfo")
            {
                filePath = Path.Combine(steamPath, "appcache", "appinfo.vdf");
            }
            else
            {
                filePath = Path.Combine(steamPath, "appcache", "packageinfo.vdf");
            }
        }
        else
        {
            filePath = args[1];
        }
        vdfFile.Read(filePath);
        try
        {
            vdfFile.Read(filePath);
        }
        // TODO: Catch explicit errors
        #pragma warning disable CA1031
        catch
        {
            Console.Error.WriteLine(Translations.Get("ErrorParsingFile"), filePath);
            return 7;
        }
        try
        {
            switch (args[0])
            {
                case "list":
                    return List(vdfFile);
                case "info":
                    return Info(vdfFile);
            }
            List<Dataset> processing;
            if (args[2] == "*")
            {
                processing = vdfFile.Datasets;
            }
            else
            {
                uint id;
                bool success = uint.TryParse(args[2], out id);
                if (!success)
                {
                    Console.Error.WriteLine(Translations.Get("ErrorInvalidNumber"), args[2]);
                    return 5;
                }
                var dataset = vdfFile.FindByID(id);
                if (dataset == null)
                {
                    Console.Error.WriteLine(Translations.Get("ErrorNoId"), id);
                    return 6;
                }
                processing = new List<Dataset> { dataset };
            }
            switch (args[0])
            {
                case "json":
                    #pragma warning disable CA1806
                    int.TryParse(args.ElementAtOrDefault(4), out int indent);
                    return Json(processing, indent: 4);
                case "query":
                    return Query(processing, args.Skip(3).ToArray());
            }
        }
        // TODO: catch explicit errors
        #pragma warning disable CA1031
        catch (Exception e)
        {
            Console.Error.WriteLine(Translations.Get("ErrorUnknown"), e.Message);
        }
        return 9;
    }

    static int List(VDFFile source)
    {
        Console.WriteLine(String.Join("\n", source.Datasets.Select(dataset => dataset.ID)));
        return 0;
    }

    static int Info(VDFFile source)
    {
        Console.WriteLine(source.Datasets.Count);
        return 0;
    }

    static int Json(List<Dataset> datasets, int indent)
    {
        foreach (var dataset in datasets)
        {
            Console.WriteLine(dataset.Data.ToJSON(indent: indent));
        }
        return 0;
    }

    static int Query(List<Dataset> datasets, string[] queries)
    {
        foreach (var dataset in datasets)
        {
            foreach (var query in queries)
            {
                Console.WriteLine(String.Join("\n", dataset.Data.Search(query)));
            }
        }
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
