using System.Resources;
using System.Globalization;

namespace VDFParse.Cli;

internal static class Translations
{
    private static bool useFallback;
    private static string currentLanguage = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
    private static ResourceManager Resources = new(GetResourcePath(currentLanguage), typeof(Program).Assembly);
    private static ResourceManager FallbackResources = new(GetResourcePath("en"), typeof(Program).Assembly);

    private static string GetResourcePath(string languageCode) =>
        $"{typeof(Translations).Namespace}.resources.language-{languageCode}";

    // The resource for en should always be embedded
    // and is assumed to have all translations
    #pragma warning disable CA1304
    public static string Get(string id, bool fallback = false)
    {
        if (fallback || useFallback)
            return FallbackResources.GetString(id) ?? "TRANSLATION ERROR";

        try
        {
            return Resources.GetString(id) ?? Get(id, true);
        }
        catch (MissingManifestResourceException)
        {
            useFallback = true;
            return Get(id, true);
        }

    }
}
