using FluentAssertions;
using HySite.Application.Command;
using HySite.Application.Helpers;
using HySite.Domain.Enums;

namespace Web.UnitTests;

public class ThemePreferenceTests
{
    [Theory]
    [InlineData(null, Theme.Auto)]
    [InlineData("", Theme.Auto)]
    [InlineData("unknown", Theme.Auto)]
    [InlineData("auto", Theme.Auto)]
    [InlineData("dark", Theme.Dark)]
    [InlineData("light", Theme.Light)]
    public void ResolveThemeReturnsKnownThemeOrAutoDefault(string? cookieValue, Theme expectedTheme)
    {
        ThemePreferenceHelper.ResolveTheme(cookieValue).Should().Be(expectedTheme);
    }

    [Theory]
    [InlineData(Theme.Auto, "")]
    [InlineData(Theme.Dark, "theme-dark")]
    [InlineData(Theme.Light, "theme-light")]
    public void GetThemeClassReturnsCssClassOnlyForForcedTheme(Theme theme, string expectedClass)
    {
        ThemePreferenceHelper.GetThemeClass(theme).Should().Be(expectedClass);
    }

    [Theory]
    [InlineData(Theme.Auto, null)]
    [InlineData(Theme.Dark, "dark")]
    [InlineData(Theme.Light, "light")]
    public void GetCookieValueReturnsOnlyForcedThemeValues(Theme theme, string? expectedCookieValue)
    {
        ThemePreferenceHelper.GetCookieValue(theme).Should().Be(expectedCookieValue);
    }

    [Theory]
    [InlineData(Theme.Auto, Theme.Light)]
    [InlineData(Theme.Light, Theme.Dark)]
    [InlineData(Theme.Dark, Theme.Auto)]
    public void GetNextThemeCyclesThroughAllThemes(Theme theme, Theme expectedNextTheme)
    {
        ThemePreferenceHelper.GetNextTheme(theme).Should().Be(expectedNextTheme);
    }

    [Theory]
    [InlineData(Theme.Auto, "themeToggleIcon-auto")]
    [InlineData(Theme.Light, "themeToggleIcon-light")]
    [InlineData(Theme.Dark, "themeToggleIcon-dark")]
    public void GetThemeIconClassReturnsCssClassForTheme(Theme theme, string expectedClass)
    {
        ThemePreferenceHelper.GetThemeIconClass(theme).Should().Be(expectedClass);
    }

    [Theory]
    [InlineData("/", "/")]
    [InlineData("/Archive", "/Archive")]
    [InlineData("/posts/example?tag=css", "/posts/example?tag=css")]
    [InlineData("", "/")]
    [InlineData(null, "/")]
    [InlineData("https://example.com", "/")]
    [InlineData("//example.com", "/")]
    public void NormalizeReturnUrlAllowsOnlyLocalUrls(string? returnUrl, string expectedReturnUrl)
    {
        ThemePreferenceHelper.NormalizeReturnUrl(returnUrl).Should().Be(expectedReturnUrl);
    }

    [Fact]
    public async Task CreateCookieOptionsExpiresInOneDay()
    {
        var before = DateTimeOffset.UtcNow.AddDays(1).AddSeconds(-1);

        var result = await new ToggleThemeCommandHandler().Handle(new ToggleThemeCommand
        {
            CurrentTheme = Theme.Auto,
            RequestedTheme = Theme.Light,
            ReturnUrl = "/"
        }, CancellationToken.None);

        var after = DateTimeOffset.UtcNow.AddDays(1).AddSeconds(1);
        result.CookieExpiresAt.Should().BeOnOrAfter(before);
        result.CookieExpiresAt.Should().BeOnOrBefore(after);
    }

    [Fact]
    public async Task ToggleThemeCommandReturnsThemeToSetAndSafeReturnUrl()
    {
        var result = await new ToggleThemeCommandHandler().Handle(new ToggleThemeCommand
        {
            CurrentTheme = Theme.Light,
            RequestedTheme = Theme.Dark,
            ReturnUrl = "https://example.com"
        }, CancellationToken.None);

        result.Theme.Should().Be(Theme.Dark);
        result.ShouldRemoveCookie.Should().BeFalse();
        result.CookieValue.Should().Be("dark");
        result.ReturnUrl.Should().Be("/");
    }

    [Fact]
    public async Task ToggleThemeCommandRemovesCookieForAutoTheme()
    {
        var result = await new ToggleThemeCommandHandler().Handle(new ToggleThemeCommand
        {
            CurrentTheme = Theme.Dark,
            RequestedTheme = Theme.Auto,
            ReturnUrl = "/Archive"
        }, CancellationToken.None);

        result.Theme.Should().Be(Theme.Auto);
        result.ShouldRemoveCookie.Should().BeTrue();
        result.CookieValue.Should().BeNull();
        result.ReturnUrl.Should().Be("/Archive");
    }
}
