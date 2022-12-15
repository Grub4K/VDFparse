using System.CommandLine;
using System.CommandLine.Invocation;
using System.CommandLine.IO;
using System.Text.Json;
using VDFparse;

Option<bool> infoOption =
    new(
        aliases: new[] { "--info-only", "-i" },
        description: "Show only info and omit main data.",
        getDefaultValue: () => false
    );

Option<bool> indentOption =
    new(
        aliases: new[] { "--pretty", "-p" },
        description: "Indent the JSON output.",
        getDefaultValue: () => false
    );

Argument<FileInfo> pathArgument =
    new(name: "path", description: "The path to the vdf file, or `appinfo`/`packageinfo`.");

Argument<List<uint>> idArguments =
    new(name: "id", description: "Ids to filter by. If no id is specified output all ids.")
    {
        Arity = ArgumentArity.ZeroOrMore
    };

RootCommand rootCommand =
    new("Convert known binary valve data files (`.vdf` files) into json.")
    {
        infoOption,
        indentOption,
        pathArgument,
        idArguments
    };

rootCommand.SetHandler(
    (context) =>
    {
        bool infoOnly = context.ParseResult.GetValueForOption(infoOption);
        bool indented = context.ParseResult.GetValueForOption(indentOption);
        FileInfo path = context.ParseResult.GetValueForArgument(pathArgument);
        List<uint> idArgs = context.ParseResult.GetValueForArgument(idArguments);
        HashSet<uint>? ids = idArgs.Count == 0 ? null : idArgs.ToHashSet();

        Main(context, path, ids, new() { Indented = indented }, infoOnly);
    }
);

return rootCommand.Invoke(args);

static void Main(
    InvocationContext context,
    FileInfo path,
    HashSet<uint>? ids,
    JsonWriterOptions options = default,
    bool infoOnly = false
)
{
    var pathArg = path.ToString();
    if (pathArg is "appinfo" or "packageinfo")
    {
        string? steamPath;
        try
        {
            steamPath = GetSteamPath();
            if (steamPath is null)
            {
                context.Console.Error.WriteLine("Cannot find Steam.");
                context.ExitCode = 2;
                return;
            }
        }
        catch (PlatformNotSupportedException)
        {
            context.Console.Error.WriteLine("Platform unsupported for automated Steam locating.");
            context.ExitCode = 3;
            return;
        }
        path = new FileInfo(Path.Combine(steamPath, "appcache", pathArg + ".vdf"));
    }

    if (!path.Exists)
    {
        context.Console.Error.WriteLine($"File \"{path}\" does not exist.");
        context.ExitCode = 1;
        return;
    }

    using var input = path.OpenRead();
    using var output = Console.OpenStandardOutput();

    using VDFTransformer transformer = new(input, output, options, infoOnly);
    transformer.Transform(ids);

    context.ExitCode = 0;
}

static string? GetSteamPath()
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

    throw new PlatformNotSupportedException();
}
