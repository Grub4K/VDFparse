using System.CommandLine;
using System.CommandLine.Invocation;

namespace VDFparse;


public static class Program
{

    private static readonly Option<bool> infoOption =
        new(
            aliases: ["--info-only", "-i"],
            description: "Show only info and omit main data.",
            getDefaultValue: () => false
        );

    private static readonly Option<bool> indentOption =
        new(
            aliases: ["--pretty", "-p"],
            description: "Indent the JSON output.",
            getDefaultValue: () => false
        );

    private static readonly Option<FileInfo> outputPathOption =
        new(
            aliases: ["--output", "-o"],
            description: "The output path or `-` to write to stdout",
            getDefaultValue: () => new("-")
        );

    private static readonly Argument<FileInfo> inputPathArgument =
        new(
            name: "path",
            description: "The path to the vdf file, `appinfo`/`packageinfo` to search or `-` to read from stdin."
        );

    private static readonly Argument<List<uint>> idArguments =
        new(name: "id", description: "Ids to filter by. If no id is specified output all ids.")
        {
            Arity = ArgumentArity.ZeroOrMore
        };

    private static readonly RootCommand rootCommand =
        new("Convert known binary valve data files (`.vdf` files) into json.")
        {
            infoOption,
            indentOption,
            outputPathOption,
            inputPathArgument,
            idArguments
        };

    private static int Main(string[] args)
    {
        rootCommand.SetHandler(RunWrapper);
        return rootCommand.Invoke(args);
    }

    private static void RunWrapper(InvocationContext context)
    {
        try
        {
            Run(context);
        }
        catch (Exception e)
        {
            var saved = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Error.WriteLine(e.ToString());
            Console.ForegroundColor = saved;
            context.ExitCode = 1;
        }
    }

    private static void Run(InvocationContext context)
    {
        var inputPath = context.ParseResult.GetValueForArgument(inputPathArgument);
        var input = OpenInputStream(inputPath);

        FileInfo outputPath = context.ParseResult.GetValueForOption(outputPathOption)!;

        List<uint> idArgs = context.ParseResult.GetValueForArgument(idArguments);
        HashSet<uint>? ids = idArgs.Count == 0 ? null : idArgs.ToHashSet();

        bool indented = context.ParseResult.GetValueForOption(indentOption);
        bool infoOnly = context.ParseResult.GetValueForOption(infoOption);

        using (input)
        {
            using var output = outputPath.ToString() is "-"
                ? Console.OpenStandardOutput()
                : outputPath.OpenWrite();

            using VDFTransformer transformer =
                new(input, output, new() { Indented = indented }, infoOnly);
            transformer.Transform(ids);
        }

        context.ExitCode = 0;
    }

    private static Stream OpenInputStream(FileInfo inputPath)
    {
        var inputPathArg = inputPath.ToString();
        if (inputPathArg is "-")
        {
            return Console.OpenStandardInput();
        }

        if (inputPathArg is "appinfo" or "packageinfo")
        {
            string? steamPath = GetSteamPath()
                ?? throw new FileNotFoundException("Cannot find steam base path");
            inputPath = new FileInfo(Path.Combine(steamPath, "appcache", inputPathArg + ".vdf"));
        }

        if (!inputPath.Exists)
        {
            throw new FileNotFoundException($"File \"{inputPath}\" does not exist", inputPathArg);
        }

        return inputPath.OpenRead();
    }

    private static string? GetSteamPath()
    {
        if (OperatingSystem.IsWindows())
        {
            return Microsoft.Win32.Registry.CurrentUser
                    .OpenSubKey(@"SOFTWARE\Valve\Steam")
                    ?.GetValue("SteamPath") as string;
        }
        else if (OperatingSystem.IsLinux())
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            var paths = new[] { ".steam", ".steam/steam", ".steam/root", ".local/share/Steam" };

            foreach (var path in paths)
            {
                var steamPath = Path.Combine(home, path);
                var appCache = Path.Combine(steamPath, "appcache");
                if (Directory.Exists(appCache))
                    return steamPath;
            }
            return null;
        }

        throw new PlatformNotSupportedException("Platform unsupported for automated Steam locating");
    }
}
