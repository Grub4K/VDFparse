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

Option<FileInfo> outputPathOption =
    new(
        aliases: new[] { "--output", "-o" },
        description: "The output path or `-` to write to stdout",
        getDefaultValue: () => new("-")
    );

Argument<FileInfo> inputPathArgument =
    new(
        name: "path",
        description: "The path to the vdf file, `appinfo`/`packageinfo` to search or `-` to read from stdin."
    );

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
        outputPathOption,
        inputPathArgument,
        idArguments
    };

rootCommand.SetHandler(Run);
return rootCommand.Invoke(args);

void Run(InvocationContext context)
{
    var input = OpenInputStream(context);
    if (input is null)
    {
        return;
    }
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

Stream? OpenInputStream(InvocationContext context)
{
    FileInfo inputPath = context.ParseResult.GetValueForArgument(inputPathArgument);
    var inputPathArg = inputPath.ToString();

    if (inputPathArg is "-")
    {
        return Console.OpenStandardInput();
    }

    if (inputPathArg is "appinfo" or "packageinfo")
    {
        string? steamPath;
        try
        {
            steamPath = GetSteamPath();
            if (steamPath is null)
            {
                context.Console.Error.WriteLine("Cannot find Steam.");
                context.ExitCode = 2;
                return null;
            }
        }
        catch (PlatformNotSupportedException)
        {
            context.Console.Error.WriteLine("Platform unsupported for automated Steam locating.");
            context.ExitCode = 3;
            return null;
        }
        inputPath = new FileInfo(Path.Combine(steamPath, "appcache", inputPathArg + ".vdf"));
    }

    if (inputPath.Exists)
    {
        return inputPath.OpenRead();
    }

    context.Console.Error.WriteLine($"File \"{inputPath}\" does not exist.");
    context.ExitCode = 1;
    return null;
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
