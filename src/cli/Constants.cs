namespace CLI
{
    static partial class Program
    {
        // 1 ErrorCannotFindSteam
        const string ErrorCannotFindSteam = "Can not find Steam";
        // 2 ErrorPlatformUnsupported
        const string ErrorPlatformUnsupported = "Platform unsupported for automated locating";
        // 3 ErrorUnknownParameter
        const string ErrorUnknownParameter = "Unknown parameter \"{0}\"";
        // 4 ErrorNotEnoughArguments
        const string ErrorNotEnoughArguments = "Not enough Arguments";
        // 5 ErrorInvalidNumber
        const string ErrorInvalidNumber = "Invalid Number \"{0}\"";
        // 6 ErrorNoId
        const string ErrorNoId = "Could not find dataset with ID {0}";
        // 7 ErrorParsingFile
        const string ErrorParsingFile = "Error while trying to parse \"{0}\"";
        // 9 ErrorUnknown
        const string ErrorUnknown = "Unknown Error while executing query: {0}";
        // Help Message
        const string HelpMessage =
    @"{0} - Parses and displays info about Steam .vdf files

    USAGE
        {0} <info|list|json|query> <path> [options]

    DESCRIPTION
        info    Show number of entries parsed

        list    Show all appids found

        json    Show key value of an app as JSON
            options:
                appid    The appid of the app to use


        query   Query specified values for the specified keys
            options:
                appid    The appid of the app to use
                query    The name of the value to query for

        path    A path to a vdf file. Use `appinfo` or `packageinfo` to
                automatically search for these files in the local Steam
                installation.

    EXAMPLES
        {0} info <path>
            Show the number of entries parsed in the vdf file specified by <path>

        {0} json <path> <appid>
            Show the key value data of <appid> in the vdf file specified by <path>

        {0} list <path>
            Show all appids found in the vdf file specified by <path>

        {0} query <path> <appid> <query> [query [...]]
            Query for a value of <appid> in the vdf file specified by <path>";
    }
}
