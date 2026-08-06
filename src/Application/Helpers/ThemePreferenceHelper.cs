using HySite.Domain.Enums;

namespace HySite.Application.Helpers;

public static class ThemePreferenceHelper
{
    public const string CookieName = "hysite-theme";

    public static Theme ResolveTheme(string? themeValue) =>
        Enum.TryParse<Theme>(themeValue, ignoreCase: true, out var theme) ? theme : Theme.Auto;

    public static string GetThemeClass(Theme theme) =>
        theme == Theme.Auto ? string.Empty : $"theme-{GetCookieValue(theme)}";

    public static string? GetCookieValue(Theme theme) =>
        theme switch
        {
            Theme.Dark => "dark",
            Theme.Light => "light",
            _ => null
        };

    public static Theme GetNextTheme(Theme theme) =>
        theme switch
        {
            Theme.Auto => Theme.Light,
            Theme.Light => Theme.Dark,
            _ => Theme.Auto
        };

    public static string GetThemeIconClass(Theme theme) =>
        theme switch
        {
            Theme.Dark => "themeToggleIcon-dark",
            Theme.Light => "themeToggleIcon-light",
            _ => "themeToggleIcon-auto"
        };

    public static string NormalizeReturnUrl(string? returnUrl)
    {
        if (string.IsNullOrWhiteSpace(returnUrl))
            return "/";

        if (!returnUrl.StartsWith('/') || returnUrl.StartsWith("//"))
            return "/";

        return returnUrl;
    }
}
