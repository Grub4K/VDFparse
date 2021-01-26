using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using VDFparse;



namespace CLI
{
    static partial class Program
    {
        static int Main(string[] args)
        {
            switch (args.ElementAtOrDefault(0)){
                case "-v":
                case "--version":
                    Console.WriteLine(typeof(Program).Assembly.GetName().Version);
                    return 0;
                case "-?":
                case "/?":
                case "-h":
                case "--help":
                case null:
                    Console.WriteLine(HelpMessage, typeof(Program).Assembly.GetName().Name);
                    return 0;
                default:
                break;
            }
            switch (args[0])
            {
                case "list":
                case "info":
                    break;
                case "json":
                    if (args.Length < 3)
                    {
                        Console.Error.WriteLine(ErrorNotEnoughArguments);
                        return 4;
                    }
                    break;
                case "query":
                    if (args.Length < 4)
                    {
                        Console.Error.WriteLine(ErrorNotEnoughArguments);
                        return 4;
                    }
                    break;
                default:
                    Console.Error.WriteLine(ErrorUnknownParameter, args[0]);
                    return 3;
            }
            VDFFile vdfFile = new VDFFile();
            string filePath;
            if (args[1] == "appinfo" || args[1] == "packageinfo")
            {
                string steamPath;
                try
                {
                    steamPath = GetSteamPath();
                    if (steamPath == null)
                    {
                        Console.Error.WriteLine(ErrorCannotFindSteam);
                        return 1;
                    }
                }
                catch (PlatformNotSupportedException)
                {
                    Console.Error.WriteLine(ErrorPlatformUnsupported);
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
            try
            {
                vdfFile.Read(filePath);
            }
            catch
            {
                Console.Error.WriteLine(ErrorParsingFile, filePath);
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
                uint id;
                bool success = uint.TryParse(args[2], out id);
                if (!success)
                {
                    Console.Error.WriteLine(ErrorInvalidNumber, args[2]);
                    return 5;
                }
                switch (args[0])
                {
                    case "json":
                        int.TryParse(args.ElementAtOrDefault(4), out int indent);
                        return Json(vdfFile, id, indent: 4);
                    case "query":
                        return Query(vdfFile, id, args.Skip(3).ToArray());
                }
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(ErrorUnknown, e.Message);
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
            Console.WriteLine(source.Datasets.Count());
            return 0;
        }

        static int Json(VDFFile source, uint id, int indent)
        {
            var dataset = source.FindByID(id);
            if (dataset == null)
            {
                Console.Error.WriteLine(ErrorNoId, id);
                return 6;
            }
            Console.WriteLine(dataset.Data.ToJSON(indent: indent));
            return 0;
        }

        static int Query(VDFFile source, uint id, string[] queries)
        {
            var dataset = source.FindByID(id);
            if (dataset == null)
            {
                Console.Error.WriteLine(ErrorNoId, id);
                return 6;
            }
            foreach (var query in queries)
            {
                Console.WriteLine(String.Join("\n", dataset.Data.Search(query)));
            }
            return 0;
        }
        static string GetSteamPath()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                var key = Registry.CurrentUser.OpenSubKey("SOFTWARE\\Valve\\Steam") ??
                    RegistryKey
                        .OpenBaseKey(RegistryHive.LocalMachine,
                            RegistryView.Registry64)
                        .OpenSubKey("SOFTWARE\\Valve\\Steam");
                if (key != null && key.GetValue("SteamPath") is string steamPath)
                {
                    return steamPath;
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                var home = Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile);
                var paths = new [] {".steam", ".steam/steam", ".steam/root",
                    ".local/share/Steam"};

                return paths
                    .Select(path => Path.Combine(home, path))
                    .FirstOrDefault(steamPath => Directory.Exists(Path.Combine(
                        steamPath, "appcache")));
            }

            throw new PlatformNotSupportedException();
        }
    }
}
